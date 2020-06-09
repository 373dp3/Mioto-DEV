using Microsoft.AspNetCore.Components;
using MiotoServer.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

        /// <summary>
        /// razorページからロードすべき情報の指示を受ける
        /// </summary>
        /// <param name="mgr"></param>
        /// <param name="client"></param>
        public async Task init(NavigationManager mgr, HttpClient client, string id, Mode mode)
        {
            NavMgr = mgr;
            Http = client;
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
            await ConfigSingleton.getInstance().getConsigAsync(NavMgr, c => {
                config = c;
                func(id);
                Task.WhenAll(loadCsvByHttp());
                Task.WhenAll(socketListener());
            });
        }

        /// <summary>
        /// http受信したCSV情報の解釈
        /// </summary>
        /// <param name="ctAry"></param>
        public void OnCycleAry(CycleTime[] ctAry)
        {
            if (ctAry == null) { return; }
            foreach (var cycle in ctAry)
            {
                var panel = listPanelModel.Where(q => q.mac == cycle.mac).FirstOrDefault();
                if (panel.mac == 0) { continue; }
                panel.updateCycleTime(cycle);
            }
            this.StateHasChanged();
        }

        /// <summary>
        /// WebSocketで受信した単一CSV情報の解釈
        /// </summary>
        /// <param name="cycle"></param>
        public void OnCycleAry(CycleTime cycle)
        {
            if (cycle == null) return;
            var panel = listPanelModel.Where(q => q.mac == cycle.mac).FirstOrDefault();
            //呼び出し元で保証 if (panel.mac == 0) { return; }
            panel.updateCycleTime(cycle);
        }

        /// <summary>
        /// http経由でCSV情報を一括取得
        /// </summary>
        /// <returns></returns>
        public async Task loadCsvByHttp()
        {
            //CSV情報の一括取得
            var macListStr = String.Join('/', targetMacList.Select(q => q.ToString("x8")).ToArray());
            var url = $"http://{new Uri(NavMgr.Uri).Host}/{macListStr}/backup/_{DateTime.Now.Ticks}";
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
                        var cycle = CycleTime.Parse(line);
                        if (cycle == null) { continue; }
                        OnCycleAry(cycle);
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
            using (var csvSocket = new CsvCashSocket(NavMgr, macListStr))
            {
                try
                {
                    //tickAndDisposeTriggerで処理を抜けるためWhenAnyを用いる
                    //WhenAllだとソケット待機が終わらないため
                    await Task.WhenAny(
                            csvSocket.connectAsync(OnCycleAry),
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
                //子機情報を構成する小パネルを作成
                listPanelModel.Add(new PanelModel()
                {
                    title = config.listTwe.Where(q => q.mac == mac).Select(q => q.name).FirstOrDefault(),
                    mac = mac
                });
            }
        }

        /// <summary>
        /// デバッグ用情報設定支援
        /// </summary>
        /// <param name="msg"></param>
        private void t(string msg)
        {
            debugMsg += $"{msg} {DateTime.Now.ToLongTimeString()} //";
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
