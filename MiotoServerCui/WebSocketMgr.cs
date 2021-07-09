using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MiotoServer
{
    public class WebSocketMgr
    {
        private static WebSocketMgr instance = null;
        private WebSocketMgr() { }
        public static WebSocketMgr getInstance()
        {
            if (instance == null) instance = new WebSocketMgr();
            return instance;
        }

        private Mutex wcMutex = new Mutex();

        public void fetchCsv(string csv)
        {
            new Thread(async () => {
                wcMutex.WaitOne();
                try
                {
                    //[TODO]パラレル化による高速処理を。TCPでロストした時に足を引っ張られる可能性あり
                    foreach (var worker in HttpWebsocketWorker.collectionWebSocketWorker)
                    {
                        await worker.TxCsvCtData(csv);
                    }
                }
                finally
                {
                    wcMutex.ReleaseMutex();
                }
            }).Start();
        }

        public void fetchProFactor(string factor)
        {
            new Thread(async () => {
                wcMutex.WaitOne();
                try
                {
                    //[TODO]パラレル化による高速処理を。TCPでロストした時に足を引っ張られる可能性あり
                    foreach (var worker in HttpWebsocketWorker.collectionWebSocketWorker)
                    {
                        await worker.TxCsvCtData(factor);
                    }
                }
                finally
                {
                    wcMutex.ReleaseMutex();
                }
            }).Start();
        }

    }
}
