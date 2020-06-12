using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MiotoBlazorClient
{
    public class SocketWorker : IDisposable
    {
        protected ClientWebSocket ws = null;
        protected Uri uri = null;
        public WebSocketState state {
            get
            {
                if(ws!=null) { return ws.State; }
                return WebSocketState.None;
            }
            set { state = value; } 
        }
        CancellationTokenSource tokenSource = new CancellationTokenSource();

        public SocketWorker() { }

        public SocketWorker(NavigationManager mgr, string method)
        {
            prepare($"ws://{new Uri(mgr.Uri).Host}/{method}/");
        }

        public SocketWorker(string url)
        {
            prepare(url);
        }

        public void prepare(string url)
        {
            try
            {
                uri = new Uri(url);
            }
            catch (Exception e)
            {
                ws = null;
                this.state = WebSocketState.Aborted;
                return;
            }
        }

        Action<string> callback = null;
        public const int WebSocketBufferSize = 1024 * 1024;
        private byte[] buffer = new byte[WebSocketBufferSize];
        bool isActive = false;

        public async Task connectAsync<T>(Action<T> action)
        {
            await connectAsync((msg) => {
                action(JsonSerializer.Deserialize<T>(msg));
            });
        }
        public async Task connectAsync(Action<string> func)
        {
            callback = func;

            ws = new ClientWebSocket();
            await ws.ConnectAsync(uri, System.Threading.CancellationToken.None);

            //接続待機
            while (ws.State == WebSocketState.Connecting) { Thread.Sleep(200); }

            var buffer = new byte[WebSocketBufferSize];
            CancellationToken token = tokenSource.Token;
            token.ThrowIfCancellationRequested();
            isActive = true;
            try
            {
                while (callback != null)
                {

                    //通信処理
                    while (ws.State == WebSocketState.Open)
                    {
                        var segment = new ArraySegment<byte>(buffer);
                        var result = await ws.ReceiveAsync(
                            segment,
                            token);

                        if (await checkCloseAsync(ws, result, buffer.Length))
                        {
                            break;
                        }


                        //セグメントの復元
                        var count = result.Count;
                        while (!result.EndOfMessage)
                        {
                            segment = new ArraySegment<byte>(buffer, count, buffer.Length - count);
                            result = await ws.ReceiveAsync(segment, CancellationToken.None);
                            count += result.Count;
                        }
                        if (callback != null)
                            callback(Encoding.UTF8.GetString(buffer, 0, count));
                    }
                }
            }
            catch (Exception e) { }
            isActive = false;
        }

        public async Task SendData(string msg)
        {
            if (ws == null)
            {
                ws = new ClientWebSocket();
                await ws.ConnectAsync(uri, System.Threading.CancellationToken.None);

                //接続待機
                while (ws.State == WebSocketState.Connecting) { Thread.Sleep(200); }
            }

            await ws.SendAsync(
                    new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg)),
                    WebSocketMessageType.Text, true,
                    CancellationToken.None
                    );
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

        public void Dispose()
        {
            callback = null;
            buffer = null;
            tokenSource.Cancel();
            for (var i=0; i<10; i++)
            {
                if (isActive == false) break;
                Thread.Sleep(200);
            }
            tokenSource.Dispose();
            tokenSource = null;
            if (ws != null) { ws.Dispose(); }
            ws = null;
        }
    }
}
