using MiotoBlazorCommon.Struct;
using MiotoServer.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MiotoServer
{
    public class WebSocketWorker : IDisposable
    {
        public HttpListenerWebSocketContext wsContext { get; set; } = null;
        private WebSocket ws = null;
        public enum OperationType { NOOP, CSV, CONFIG }
        public OperationType type { get; private set; } = OperationType.NOOP;

        public void Dispose()
        {
            if ((wsContext != null) && (wsContext.WebSocket != null))
            {
                wsContext.WebSocket.Dispose();
            }
        }

        public async Task TxCsvCtData(string msg)
        {
            if (type != OperationType.CSV) { return; }
            await TxDataAsync(msg);
        }
        public async Task TxDataAsync(string msg)
        {
            if (ws == null) { return; }
            if (ws.State != WebSocketState.Open) { return; }

            await ws.SendAsync(
                new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg)),
                WebSocketMessageType.Text, true,
                System.Threading.CancellationToken.None
                );
        }

        public const string CONFIG_DB_KEY = "BLAZOR_CONFIG";

        private async Task SendConfig()
        {
            var wrapper = DbWrapper.getInstance();
            var cfg = new Config();
            try
            {
                var jsonCfg = wrapper.getConfig(CONFIG_DB_KEY);
                cfg = JsonSerializer.Deserialize<Config>(jsonCfg);
            }
            catch (Exception e)
            {
                cfg.insertOrUpdateTwe(wrapper.getLastInfoList());
                d("no blazor cfg exist");
            }
            //TWE-Lite子機のリスト確認と追加
            var list = DbWrapper.getInstance().getLastInfoList();
            foreach (var twe in list)
            {
                if (cfg.listTwe.Where(q => q.mac == twe.mac).Count() != 0) continue;
                cfg.listTwe.Add(new ConfigTwe() { mac = twe.mac, Ticks = twe.ticks });
            }



            cfg.appVer = MiotoServerWrapper.config.appVer;
            var json = JsonSerializer.Serialize(cfg);
            await TxDataAsync(json);
        }

        private void RxData(string msg)
        {
            switch (type)
            {
                case OperationType.CONFIG:
                    try
                    {
                        var cfg = JsonSerializer.Deserialize<Config>(msg);
                        DbWrapper.getInstance().setConfig(CONFIG_DB_KEY, msg);
                    }
                    catch (Exception e) { d("受信した設定情報が破損しています。"); }
                    break;
                case OperationType.CSV:
                    //生産要因の登録
                    try
                    {
                        var factor = JsonSerializer.Deserialize<ProductionFactor>(msg);
                        //日時指定がない場合は受信時刻を設定する
                        if (factor.stTicks == ProductionFactor.SET_TICKS_AT_SERVER)
                        {
                            factor.stTicks = DateTime.Now.Ticks;
                        }
                        DbWrapper.getInstance().conn.Insert(factor);

                        //Echo back
                        mgr.fetchProFactor($"!{ProductionFactor.KEY},{factor.ToCSV()}");
                        Program.d("set factor");
                    }
                    catch (Exception e) { }
                    break;
                default:
                    break;
            }
        }

        WebSocketMgr mgr = WebSocketMgr.getInstance();

        public async Task Start()
        {
            ws = wsContext.WebSocket;
            var uri = wsContext.RequestUri.AbsoluteUri;


            //接続待機
            while (ws.State == WebSocketState.Connecting) { Thread.Sleep(200); }

            if (uri.Contains("/csvcash/"))
            {
                type = OperationType.CSV;
            }
            if (uri.Contains("/config/"))
            {
                type = OperationType.CONFIG;
                await SendConfig();
            }
            d($"ws uri: {wsContext.RequestUri.AbsoluteUri}");


            //通信処理
            var rdBuffer = new byte[1024 * 1024];
            while (ws.State == WebSocketState.Open)
            {
                var segment = new ArraySegment<byte>(rdBuffer);

                var result = await ws.ReceiveAsync(
                    segment,
                    CancellationToken.None);

                if (await checkCloseAsync(ws, result, rdBuffer.Length)) { break; }

                //セグメントの復元
                var count = result.Count;
                while (!result.EndOfMessage)
                {
                    segment = new ArraySegment<byte>(rdBuffer, count, rdBuffer.Length - count);
                    result = await ws.ReceiveAsync(segment, CancellationToken.None);
                    count += result.Count;
                    d($"cout: {count}");
                }
                var rdData = Encoding.UTF8.GetString(rdBuffer, 0, count);
                //d($"ws: {rdData}");
                RxData(rdData);
            }
            d("WebSocket closed.");
        }

        private async Task<bool> checkCloseAsync(WebSocket ws, WebSocketReceiveResult result, int bufferSize)
        {
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close", CancellationToken.None);
                return true;
            }
            if (result.MessageType == WebSocketMessageType.Binary)
            {
                await ws.CloseAsync(WebSocketCloseStatus.InvalidMessageType,
                    "Cannot support binary message", CancellationToken.None);
                return true;
            }
            if (result.Count > 20 * 1024 * 1024)//20MBまで
            {
                await ws.CloseAsync(WebSocketCloseStatus.MessageTooBig,
                    "Cannot support over 1MB payload", CancellationToken.None);
                return true;
            }
            return false;
        }

        private void d(string msg) { Program.d(msg); }
    }
}