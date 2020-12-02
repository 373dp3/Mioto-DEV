using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MiotoServer
{
    public class MiotoServerWrapper
    {
        public static MiotoServerWrapper instance { get; private set; }
        private MiotoServerWrapper()
        {
            MiotoServerWrapper.config = new AppConfig();
        }

        public static AppConfig config { get; private set; } = null;

        public static MiotoServerWrapper getInstance(AppConfig config = null)
        {
            if ((instance == null) && (config == null))
            {
                throw new Exception("初回呼び出し時にはConfigが必要です");
            }
            if (instance == null) { instance = new MiotoServerWrapper(); }
            if (config != null)
            {
                MiotoServerWrapper.config = config;
            }
            return instance;
        }



        public delegate void StatusMsgFunc(string msg);
        public bool isSerialPortStop { get; private set; } = false;
        public void start(CancellationToken token)
        {
            isNetworkChenged();//IPアドレスの出力

            d("DBインスタンスの確保");
            try
            {
                DbWrapper dbWrapper = DbWrapper.getInstance(config.dbdir);
                dbWrapper.refreshLastInfoList();
                dbWrapper.vacuum();
            }
            catch (Exception e)
            {
                d(e.ToString());
            }

            Task.Run(() => dbCheckTask(token)).ConfigureAwait(false);

            d("http worker thread の起動");
            var httpd = new HttpdWorker(config.serverPortNumber);
            httpd.Run(token);

            d("シリアルポート処理の開始");
            Task.Run(() => isSerialPortStop = (new SerialPortWorker()).run(token)).ConfigureAwait(false);

            //終了指示待ちループ兼、ネットワーク変更監視
            long cnt = 0;
            while (token.IsCancellationRequested == false)
            {
                if((cnt % 5 == 0) && (isNetworkChenged())) { httpd.httpdRestart(); }
                cnt++;
                Thread.Sleep(1000);
            }

        }

        private static List<IPAddress> ipList = new List<IPAddress>();

        public static bool isNetworkChenged()
        {
            // ホスト名を取得する
            string hostname = Dns.GetHostName();
            bool isChange = false;
            // ホスト名からIPアドレスを取得する
            IPAddress[] adrList = Dns.GetHostAddresses(hostname);
            foreach (IPAddress address in adrList)
            {
                var strIp = address.ToString();
                //15文字を超えるアドレスはIPv6のため省く
                if (strIp.Length > 15) { continue; }
                if (ipList.Contains(address) == false) { isChange = true; }
            }
            //ipListに含まれてadrListに含まれないものの確認
            foreach (var address in ipList)
            {
                if (adrList.Contains(address) == false) { isChange = true; }
            }
            //次回用に保存
            ipList.Clear();
            foreach (var address in adrList)
            {
                var strIp = address.ToString();
                if (strIp.Length > 15) { continue; }
                ipList.Add(address);
                if (isChange) Program.d("IP Address: " + strIp);
            }
            return isChange;
        }

        private void dbCheckTask(CancellationToken token)
        {
            d("DBチェッカ起動");
            var sIns = DbSoundOrder.getInstance();
            sIns.setDataPath(config.dbdir);
            var dbChecker = DbChecker.getInstance(config.hhmm.ToString());

            while (token.IsCancellationRequested == false)
            {
                try
                {
                    sIns.fetch();
                    dbChecker.fetch();
                    Thread.Sleep(1000);
                }
                catch (Exception dbex)
                {
                    Program.d(dbex.ToString());
                }
                finally { }
            }
            d("DBチェッカ終了");
        }


        public static void d(string msg)
        {
            if(config == null) { return; }
            if (config.func == null) { return; }
            config.func(msg);
        }



    }
}
