using Microsoft.AspNetCore.Components;
using MiotoBlazorCommon;
using MiotoBlazorCommon.Struct;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MiotoBlazorClient
{
    public class MiotoSockPage : ComponentBase, IDisposable
    {
        public Config config { get; set; } = null;
        public List<PanelModel> _listPanelModel = new List<PanelModel>();
        public string debugMsg = "";
        public CancellationTokenSource tokenSource = new CancellationTokenSource();
        private NavigationManager NavMgr;
        private HttpClient Http;
        //表示順のMACアドレス一覧(loadPanelDefinitionの後に利用可能)
        private List<long> targetMacList = new List<long>();
        public enum Mode { SINGLE, PANEL_LIST}

        Func<PanelModel> factory = null;

        private bool isHttpDone = false;

        /// <summary>
        /// razorページからロードすべき情報の指示を受ける
        /// </summary>
        /// <param name="mgr"></param>
        /// <param name="client"></param>
        public async Task init(NavigationManager mgr, HttpClient client, List<PanelModel> listPanelModel,
            string id, Mode mode, Func<PanelModel> factory, string dateOrder=null)
        {
            this._listPanelModel = listPanelModel;
            //ページ遷移の際に前ページの情報を破棄
            //listPanelModel.Clear(); => チラツキ防止のため、直前にクリア
            tokenSource.Cancel();
            tokenSource.Dispose();
            tokenSource = new CancellationTokenSource();
            isHttpDone = false; ;
            targetMacList.Clear();


            //末尾にスラッシュがない場合の補足
            if ((dateOrder!=null) && (dateOrder[dateOrder.Length - 1] != '/')){
                dateOrder += "/";
            }
            NavMgr = mgr;
            Http = client;
            this.factory = factory;
            Action<string> funcLoadPanel = null;
            switch (mode)
            {
                case Mode.SINGLE:
                    funcLoadPanel = loadPanelSingleDefinition;
                    break;
                case Mode.PANEL_LIST:
                    funcLoadPanel = loadPanelListDefinition;
                    break;
            }
            //Configの取得
            await ConfigSingleton.getInstance().getConfigAsync(NavMgr, c => {
                config = c;

                //listPanelModeの実体化
                funcLoadPanel(id);

                //ProductionFactorの取得
                _ = loadCsvByHttp(() => new ProductionFactor(),
                        q => {
                        var factor = (ProductionFactor)q;
                            listPanelModel.Where(q => q.mac == factor.mac)
                                .FirstOrDefault()
                                .SetProductionFactor(factor);
                        },
                        () => {
                            //完了後の処理

                            //サイクルタイム情報の取得
                            _ = loadCsvByHttp(() => new CycleTime(), q =>
                            {
                                OnCycle((CycleTime)q);
                            },
                            () => {
                                isHttpDone = true;
                                //完了後の処理
                                //今日の日付の場合に変更受信処理の登録
                                if ((dateOrder == null) || (dateOrder.Length == 0))
                                {
                                    _ = pollingWorker();
                                }
                                StateHasChanged();
                            },
                            dateOrder);

                        },
                        $"{ProductionFactor.KEY}/{dateOrder}");
            });
        }

        /// <summary>
        /// 受信したCycle情報解釈
        /// </summary>
        /// <param name="cycle"></param>
        public void OnCycle(CycleTime cycle)
        {
            if (cycle == null) return;
            var panel = _listPanelModel.Where(q => q.mac == cycle.mac).FirstOrDefault();
            panel.updateCycleTime(cycle);
        }

        public void OnProductionFactor(ProductionFactor factor)
        {
            //debugMsg += $"/1/ (OnProductionFactor) {factor.ToCSV()} ";
            if (_listPanelModel == null) return;
            var ans =_listPanelModel.Where(q => q.mac == factor.mac).FirstOrDefault();
            if((ans == null) || (ans.mac == 0)) { return; }
            ans.SetProductionFactor(factor);
            this.StateHasChanged();
        }

        private long maxTicks = 0;
        private DateTime preDt = DateTime.Now;
        private void updateMaxTicks(HttpResponseMessage response)
        {
            if (response.Headers.Contains("MaxTicks"))
            {
                try
                {
                    var ans = Convert.ToInt64(response.Headers.GetValues("MaxTicks").First());

                    //稼働日切り替え時のリロード対応
                    if (maxTicks > 0)
                    {
                        var preHHMM = Convert.ToInt32(preDt.ToString("HHmm"));
                        var curHHMM = Convert.ToInt32(DateTime.Now.ToString("HHmm"));
                        var cfg = config.dateLineHHMM;
                        if ((preHHMM< cfg) && (cfg <= curHHMM))
                        {
                            forceReload();
                        }
                        Console.WriteLine($"{preHHMM} => {curHHMM} [cfg:{cfg}]");
                        preDt = DateTime.Now;
                    }

                    //値が上回る時のみ更新する。Factorのみ、CSV空の応答が生じる場合があり、
                    //その時、Ticksの値が過去に遡るケースがあるため。
                    if (ans > maxTicks) { maxTicks = ans; }
                }
                catch (Exception e) { }
                Console.WriteLine("MaxTicks: " + maxTicks);
            }
        }

        /// <summary>
        /// CSV情報の一括取得
        /// </summary>
        /// <param name="createFunc">新規インスタンス取得用のメソッド</param>
        /// <param name="callback">変換後に受け取る関数</param>
        /// <param name="uriPathWithEndSlash">URLに含めるオプション末尾に/をつけること</param>
        /// <returns></returns>
        public async Task loadCsvByHttp(Func<IParseable> createFunc, 
            Action<IParseable> callback, Action funcDone,
            string uriPathWithEndSlash="")
        {
            //CSV情報の一括取得
            var macListStr = String.Join('/', targetMacList.Select(q => q.ToString("x8")).ToArray());
            var url = $"{NavMgr.BaseUri.Replace("/html/", "/")}{uriPathWithEndSlash}{macListStr}/backup/_{DateTime.Now.Ticks}";

            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            using(var response = await Http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
            {
                if(response.StatusCode != System.Net.HttpStatusCode.OK) { return; }
                updateMaxTicks(response);

                using (var content = response.Content)
                using(var stream = await content.ReadAsStreamAsync())
                using (var sr = new System.IO.StreamReader(stream))
                {
                    var dt = DateTime.Now.AddSeconds(1);
                    while (sr.EndOfStream == false)
                    {
                        var line = sr.ReadLine();
                        if (line == null) { break; }
                        //Console.WriteLine("http: " + line);
                        try
                        {
                            var item = createFunc();
                            try
                            {
                                item.ParseInto(line);
                            }
                            catch (Exception e) { continue; }
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
            }
            if(funcDone!=null) { funcDone(); }
        }

        /// <summary>
        /// 現時点ではPolling用のhttpgetだが、後々初回取得時の処理と共通化を
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private async Task<bool> httpGetAsync(string url)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            using (var response = await Http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
            {
                if (response.StatusCode != System.Net.HttpStatusCode.OK) { return false; }
                updateMaxTicks(response);

                using (var content = response.Content)
                using (var stream = await content.ReadAsStreamAsync())
                using (var sr = new System.IO.StreamReader(stream))
                {
                    var dt = DateTime.Now.AddSeconds(1);
                    while (sr.EndOfStream == false)
                    {
                        var msg = sr.ReadLine();
                        if (msg == null) { break; }
                        //Console.WriteLine("http-long_polling: " + msg);
                        if (msg[0] != '!')//Cycle情報は従来どおり
                        {
                            OnCycle(CycleTime.Parse(msg));
                            continue;
                        }
                        //サイクル情報以外は先頭に!と識別情報をつける
                        //!OnProductionFactor,4,1,81.....
                        if (msg.Contains($"!{ProductionFactor.KEY}"))
                        {
                            var factor = new ProductionFactor();
                            factor.ParseInto(msg.Replace($"!{ProductionFactor.KEY},", ""));
                            OnProductionFactor(factor);
                        }

                        if (msg.Contains($"!{Config.RELOAD_FORCE_KEY}"))
                        {
                            forceReload();
                        }
                    }
                }
            }
            return true;
        }
        
        public void forceReload()
        {
            NavMgr.NavigateTo(NavMgr.Uri, true);
        }

        protected async Task HttpLongPolling()
        {
            var macListStr = String.Join('/', targetMacList.Select(q => q.ToString("x8")).ToArray());

            var token = tokenSource.Token;

            while (token.IsCancellationRequested == false)
            {
                try
                {
                    var url = $"{NavMgr.BaseUri.Replace("/html/", "/")}bz{maxTicks}/{macListStr}/_{DateTime.Now.Ticks}";

                    if (token.IsCancellationRequested) { continue; }

                    if(await httpGetAsync(url) == false)
                    {
                        await Task.Delay(1000);
                        continue;
                    }
                    this.StateHasChanged();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                await Task.Delay(200);
            }
        }

        /// <summary>
        /// 最新情報受信用Web long polling
        /// </summary>
        /// <returns></returns>
        protected async Task pollingWorker()
        {
            await Task.WhenAny(
                HttpLongPolling(),
                tickAndDisposeTrigger());
        }

        protected async Task Send(ProductionFactor factor)
        {
            var json = JsonSerializer.Serialize(factor);
            Console.WriteLine($"Dialog send json: {json}");

            var macListStr = String.Join('/', targetMacList.Select(q => q.ToString("x8")).ToArray());
            var url = $"{NavMgr.BaseUri.Replace("/html/", "/")}production_factor_post/_{DateTime.Now.Ticks}";
            var response = await Http.PostAsJsonAsync(url, factor);
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
            preparePanel();
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
            preparePanel();
        }

        /// <summary>
        /// 子機ごとの表示パネルに子機名称、mac情報を設定
        /// </summary>
        private void preparePanel()
        {
            //パネル内の子機表示順序でmac一覧を作成
            if (_listPanelModel.Count == 0)
            {
                foreach (var mac in targetMacList)
                {
                    var panel = factory();
                    panel.title = config.listTwe.Where(q => q.mac == mac).Select(q => q.name).FirstOrDefault();
                    panel.mac = mac;
                    //子機情報を構成する小パネルを作成
                    _listPanelModel.Add(panel);
                }

            }
            else
            {
                foreach (var panel in _listPanelModel)
                {
                    panel.ClearPrevInfo();
                }
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

        public async Task ToCSV(Action<string> func, string idDate, bool isHeader)
        {
            StringBuilder sb = new StringBuilder();
            await Task.Yield();
            var date = DateTime
                        .ParseExact(idDate, "yyyyMMdd", CultureInfo.InvariantCulture)
                        .ToString("yyyy/MM/dd");
            
            for(var i=0; i<100; i++)
            {
                if (isHttpDone)
                {
                    break;
                }
                await Task.Delay(100);
            }

            if (isHeader)
            {
                sb.AppendLine("日付,子機名,MAC,要因,品番,担当,開始時刻,期間(分),動作回数,出来高,可動率,標準CT,平均CT");
            }

            foreach (var item in this._listPanelModel)
            {
                foreach (var factor in item.listProductionFactor)
                {
                    var btn = new ButtonAttr(factor.status);
                    sb.AppendLine(
                        $"{date},\"{item.title}\",{item.mac.ToString("x")}," +
                        btn.name + "," + factor.ToUserSaveCsv());
                }
                await Task.Yield();
            }
            func(sb.ToString());
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
