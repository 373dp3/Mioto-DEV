﻿using Microsoft.AspNetCore.Components;
using MiotoServer.DB;
using MiotoServer.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MiotoBlazorClient
{
    public class MiotoSockPage : ComponentBase, IDisposable
    {
        public Config config { get; set; } = null;
        public List<PanelModel> listPanelModel = new List<PanelModel>();
        public string debugMsg = "";
        public CancellationTokenSource tokenSource = new CancellationTokenSource();
        private NavigationManager NavMgr;
        private HttpClient Http;
        //表示順のMACアドレス一覧(loadPanelDefinitionの後に利用可能)
        private List<long> targetMacList;
        public enum Mode { SINGLE, PANEL_LIST}

        Func<PanelModel> factory = null;
        private SocketWorker wsWorker { get; set; } = null;

        /// <summary>
        /// razorページからロードすべき情報の指示を受ける
        /// </summary>
        /// <param name="mgr"></param>
        /// <param name="client"></param>
        public async Task init(NavigationManager mgr, HttpClient client, string id, Mode mode, Func<PanelModel> factory)
        {
            NavMgr = mgr;
            Http = client;
            this.factory = factory;
            Action<string> func = null;
            switch (mode)
            {
                case Mode.SINGLE:
                    func = loadPanelSingleDefinition;
                    break;
                case Mode.PANEL_LIST:
                    func = loadPanelListDefinition;
                    break;
            }
            await ConfigSingleton.getInstance().getConfigAsync(NavMgr, c => {
                config = c;
                func(id);//func呼び出し完了でlistPanelModelが実体化済み

                _ = loadCsvByHttp(() => new ProductionFactor(),
                        q => {
                        var factor = (ProductionFactor)q;
                            tmp += ((ProductionFactor)q).ToCSV() + " // ";
                            listPanelModel.Where(q => q.mac == factor.mac)
                                .FirstOrDefault()
                                .SetProductionFactor(factor);
                        },
                        $"{ProductionFactor.KEY}/");

                _ = loadCsvByHttp(() => new CycleTime(), q => OnCycle((CycleTime)q));
                _ = socketListener();
            });
        }

        public string tmp { get; set; } = "";

        /// <summary>
        /// 受信したCycle情報解釈
        /// </summary>
        /// <param name="cycle"></param>
        public void OnCycle(CycleTime cycle)
        {
            if (cycle == null) return;
            var panel = listPanelModel.Where(q => q.mac == cycle.mac).FirstOrDefault();
            panel.updateCycleTime(cycle);
        }

        public void OnProductionFactor(ProductionFactor factor)
        {
            debugMsg += $"// (OnProductionFactor) {factor.ToCSV()} ";
        }

        /// <summary>
        /// CSV情報の一括取得
        /// </summary>
        /// <param name="createFunc">新規インスタンス取得用のメソッド</param>
        /// <param name="callback">変換後に受け取る関数</param>
        /// <param name="uriPathWithEndSlash">URLに含めるオプション末尾に/をつけること</param>
        /// <returns></returns>
        public async Task loadCsvByHttp(Func<IParseable> createFunc, Action<IParseable> callback, string uriPathWithEndSlash="")
        {
            //CSV情報の一括取得
            var macListStr = String.Join('/', targetMacList.Select(q => q.ToString("x8")).ToArray());
            var url = $"http://{new Uri(NavMgr.Uri).Host}/{uriPathWithEndSlash}{macListStr}/backup/_{DateTime.Now.Ticks}";
            using (var stream = await Http.GetStreamAsync(url))
            using (var sr = new System.IO.StreamReader(stream))
            {
                var dt = DateTime.Now.AddSeconds(1);
                while (sr.EndOfStream == false)
                {
                    var line = sr.ReadLine();
                    if (line == null) { break; }
                    try
                    {
                        var item = createFunc();
                        try
                        {
                            item.ParseInto(line);
                        }catch(Exception e) { continue; }
                        callback(item);

                        if (DateTime.Now > dt)
                        {
                            dt = DateTime.Now.AddSeconds(1);
                            debugMsg = $"情報を一括取得しています・・({stream.Position}/{stream.Length})";
                            this.StateHasChanged();
                            await Task.Yield();
                        }
                    }
                    catch (Exception e) { }
                }
            }

            debugMsg = "";
            this.StateHasChanged();
        }

        /// <summary>
        /// 最新情報受信用WebSocketを開始
        /// </summary>
        /// <returns></returns>
        protected async Task socketListener()
        {
            var macListStr = String.Join('/', targetMacList.Select(q => q.ToString("x8")).ToArray());
            using (wsWorker = new SocketWorker($"ws://{new Uri(NavMgr.Uri).Host}/csvcash/{macListStr}/"))
            {
                try
                {
                    //tickAndDisposeTriggerで処理を抜けるためWhenAnyを用いる
                    //WhenAllだとソケット待機が終わらないため
                    await Task.WhenAny(
                            wsWorker.connectAsync(msg => {
                                if (msg[0] != '!')//Cycle情報は従来どおり
                                {
                                    OnCycle(CycleTime.Parse(msg));
                                    return;
                                }
                                //サイクル情報以外は先頭に!と識別情報をつける
                                //!OnProductionFactor,4,1,81.....
                                if (msg.Contains($"!{ProductionFactor.KEY}"))
                                {
                                    var factor = new ProductionFactor();
                                    factor.ParseInto(msg.Replace($"!{ProductionFactor.KEY},", ""));
                                    OnProductionFactor(factor);
                                }
                            }),
                            tickAndDisposeTrigger()
                        );
                }
                catch (Exception e)
                {
                    debugMsg = e.ToString();
                    this.StateHasChanged();
                }
            }
        }

        protected async Task Send(ProductionFactor factor)
        {
            if (wsWorker == null) { return; }
            if (wsWorker.state != System.Net.WebSockets.WebSocketState.Open) { return; }
            var json = JsonSerializer.Serialize(factor);
            await wsWorker.SendData(json);
        }

        /// <summary>
        /// 複数パネル一覧用パネル構成情報のロード
        /// </summary>
        /// <param name="panelIndexOrder"></param>
        /// <returns></returns>
        protected void loadPanelListDefinition(string idStr)
        {
            long panelIndex = 0;

            //引数の確認
            try
            {
                if ((idStr != null) && (idStr.Length > 0))
                {
                    panelIndex = Convert.ToInt64(idStr);
                }
                idStr = "";
            }
            catch (Exception e) { }

            if ((panelIndex == 0) || (config == null))
            {
                debugMsg = "情報が見つかりません。管理者によって削除された可能性があります。";
                this.StateHasChanged();
                return;
            }


            //該当のパネル情報取得
            var panel2idx = config.listCtPanel2Index
                        .Where(q => q.index == panelIndex)
                        .FirstOrDefault();
            if (panel2idx == null)
            {
                debugMsg = "パネル情報が見つかりません。管理者によって削除された可能性があります。";
                this.StateHasChanged();
                return;
            }

            targetMacList = panel2idx.panel.listMac2Index.OrderBy(q => q.index).Select(q => q.mac).ToList();
            updatePanel();
        }


        /// <summary>
        /// 子機単一パネル用情報ロード
        /// </summary>
        /// <param name="idStr"></param>
        private void loadPanelSingleDefinition(string idStr)
        {
            long id = 0;
            try
            {
                if ((idStr != null) && (idStr.Length > 0))
                {
                    id = Convert.ToInt64(idStr);
                }
                idStr = "";
            }
            catch (Exception e) { }
            if ((id == 0) || (config == null))
            {
                debugMsg = "情報が見つかりません。管理者によって削除された可能性があります。";
                this.StateHasChanged();
                return;
            }
            targetMacList = new List<long>();
            targetMacList.Add(id);
            updatePanel();
        }

        /// <summary>
        /// 子機ごとの表示パネルに子機名称、mac情報を設定
        /// </summary>
        private void updatePanel()
        {
            //パネル内の子機表示順序でmac一覧を作成
            foreach (var mac in targetMacList)
            {
                var panel = factory();
                panel.title = config.listTwe.Where(q => q.mac == mac).Select(q => q.name).FirstOrDefault();
                panel.mac = mac;
                //子機情報を構成する小パネルを作成
                listPanelModel.Add(panel);
            }
        }

        /// <summary>
        /// 表示の定期更新フック
        /// </summary>
        /// <returns></returns>
        private async Task tickAndDisposeTrigger()
        {
            try
            {
                var token = tokenSource.Token;
                await Task.Run(async () =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        await Task.Delay(1000);
                        this.StateHasChanged();
                    }
                });
            }
            catch (Exception e) { }
        }

        public virtual void Dispose()
        {
            try
            {
                tokenSource.Cancel();
                tokenSource.Dispose();
                tokenSource = null;
            }
            catch (Exception e) { }
        }
    }
}
