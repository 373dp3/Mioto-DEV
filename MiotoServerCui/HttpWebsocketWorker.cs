using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MiotoServer
{
    class HttpWebsocketWorker
    {
        public static SynchronizedCollection<WebSocketWorker> collectionWebSocketWorker = new SynchronizedCollection<WebSocketWorker>();
        public async Task<bool> doOperateIfWebsocketRequestAsync(HttpListenerContext context, HttpListenerResponse res)
        {
            if (context.Request.IsWebSocketRequest == false)
            {
                return false;
            }

            var wsContext = await context.AcceptWebSocketAsync(null);
            using (var worker = new WebSocketWorker() { wsContext = wsContext })
            {
                collectionWebSocketWorker.Add(worker);
                d($"WebSocket #{collectionWebSocketWorker.Count}");
                try
                {
                    await worker.Start();
                }
                catch (Exception e) { }
                collectionWebSocketWorker.Remove(worker);
                d($"WebSocket #{collectionWebSocketWorker.Count}");
            }

            return true;
        }

        private void d(string msg)
        {
            Program.d(msg);
        }
    }
}