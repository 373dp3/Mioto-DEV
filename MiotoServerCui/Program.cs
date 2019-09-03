/*
Copyright (c) 2018 Toshiaki Minami
Released under the MIT license
https://opensource.org/licenses/mit-license.php
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MiotoServer
{
    /*
     * 
     *  MiotoServer.exe 使用方法
     *  
     *  
     *  MiotoServer.exe -hhmm <日付切り替え時刻4桁> -port <TocostickのCOMポート>
     *  Tocostick以外でも、ESP-WROOM-02からのhttp POST時で情報を受け取ります。
     *  
     *  POST時のURLは関知せず(ESPのサンプルコードでは便宜上 http://server-ip/post/)、
     *  メソッドがPOSTであれば、リクエストボディ部の文字列をTWE-Liteと同じ形式と
     *  して解釈処理を実施。LRCに合致しなければ破棄します。
     *  
     *  蓄積結果はCSV形式にて、http://server-ip/ として最新の情報を参照可能。
     *  それ以前の日付のものは、http://server-ip/yyyymmdd として取得できます。
     *  
     *  MiotoServer.exeと同じフォルダにLogsというサブフォルダを作成すると、
     *  ログが保存されます。
     *  
     *  -hm 500 -p COM4 -d .\test2 -bps 38400
     *  
     */

    public class Program
    {
        // Win32 APIであるSetConsoleCtrlHandler関数の宣言
        [DllImport("Kernel32")]
        static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);

        // SetConsoleCtrlHandler関数にメソッド（ハンドラ・ルーチン）を
        // 渡すためのデリゲート型
        delegate bool HandlerRoutine(CtrlTypes CtrlType);

        // ハンドラ・ルーチンに渡される定数の定義
        public enum CtrlTypes
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        public static SerialPort serial = null;
        static HandlerRoutine myHandlerDele;
        static TwePacketParser parser = new TwePacketParser();

        public static bool isActive { get; private set; }

        public const string PORT_KEY = "p";
        public const string PORT_BPS_KEY = "bps";
        public const string HM_KEY = "hm";
        public const string DIR_KEY = "d";
        public static Dictionary<string, string> config { get; set; }

        static void Main(string[] args)
        {
            isActive = true;
            isNetworkChenged();


            if (paramCheck(args)) { return; }

            myHandlerDele = new HandlerRoutine(myHandler);
            SetConsoleCtrlHandler(myHandlerDele, true);

            //シリアルポート番号のキーが含まれている場合、ポートを開く
            //指定時は引数にて「-p com22 -hm 500」等。
            if (config.ContainsKey(PORT_KEY))
            {
                //SerialのみでCPU負荷が5%前後コンスタントにかかる。
                var bps = config[PORT_BPS_KEY];
                int bpsInt = 0;
                if(bps==null) { bps = "115200"; }
                try
                {
                    bpsInt = Convert.ToInt32(bps);
                }
                catch
                {
                    bpsInt = 115200;
                    d("ポートの速度設定に初期値を用いました。 " + bpsInt.ToString());
                }
                serial = new SerialPort(config[PORT_KEY], bpsInt);
                //serial = new SerialPort(config[PORT_KEY], 38400);//ハードコーディング oida
                //serial = new SerialPort(config[PORT_KEY], 115200);//ハードコーディング
                serial.DataReceived += Serial_DataReceived;
                serial.ReadTimeout = 100;//100msまで待機
                serial.Open();
            }

            var dbDir = ".\\db";
            if (config.ContainsKey(DIR_KEY))
            {
                dbDir = config[DIR_KEY];
            }

            d("DBインスタンスの確保");
            try
            {
                DbWrapper dbWrapper = DbWrapper.getInstance(dbDir);
            }
            catch (Exception e)
            {
                d(e.ToString());
            }

            var dbChecker = DbChecker.getInstance(config[HM_KEY]);
            var dbcTh = new Thread(() => {
                d("DBチェッカ起動");
                while (isActive)
                {
                    try
                    {
                        dbChecker.fetch();
                        Thread.Sleep(1000);
                    }catch(Exception dbex)
                    {
                        Program.d(dbex.ToString());
                    }
                    finally { }
                }
                d("DBチェッカ終了");
            });
            dbcTh.Start();

            d("http worker thread の起動");
            httpd = new HttpdWorker();
            httpd.Run();

            new Thread(()=> {
                while (isActive)
                {
                    try
                    {
                        if (isNetworkChenged())
                        {
                            httpd.httpdRestart();
                        }
                        Thread.Sleep(5000);
                    }
                    catch (Exception e) { d(e.ToString()); }
                }
                d("http worker thread の終了");
            }).Start();


            //終了処理待機
            Program.d("http start");
            dbcTh.Join();
        }

        private static HttpdWorker httpd = null;
        private static List<IPAddress> ipList = new List<IPAddress>();

        private static bool isNetworkChenged()
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
            foreach(var address in ipList)
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
                if(isChange) Program.d("IP Address: " + strIp);
            }
            return isChange;
        }

        public static void d(string msg)
        {
            Console.WriteLine(msg);
            Trace.WriteLine(DateTime.Now.ToString("yyyyMMdd HH:mm:ss")+" "+msg);
        }

        static bool paramCheck(string[] args)
        {
            config = parseArg(args);
            if ((config.Count == 0)
                || (config.ContainsKey("h"))
                || (config.ContainsKey("help"))
                )
            {
                Program.d("MiotoServer -" + PORT_KEY + " <ポート番号> -"
                    + PORT_BPS_KEY + " <bps> -"
                    + DIR_KEY + " <DB保存場所> -" 
                    + HM_KEY + " <日付変更時刻 HHMM>");
                return true;
            }
            var isInValid = true;
            if (config.ContainsKey(PORT_KEY))
            {
                foreach (var port in SerialPort.GetPortNames())
                {
                    if (port.ToLower().CompareTo(config[PORT_KEY].ToLower()) == 0)
                    {
                        isInValid = false;
                        break;
                    }
                }
            }
            if (isInValid)
            {
                Program.d("シリアルポート番号が無効か指定されていません。");
                return false;
            }
            return false;
        }

        static Dictionary<string, string> parseArg(string[] args)
        {
            var ans = new Dictionary<string,string>();
            var key = "";
            foreach(var node in args)
            {
                if(node[0] == '-')
                {
                    key = node.Substring(1).ToLower();//小文字保証
                    continue;
                }
                if(key.Length ==0) { continue; }
                if (key.CompareTo(DIR_KEY) == 0)
                {
                    ans.Add(key, node);
                }
                else
                {
                    ans.Add(key, node.ToLower());//小文字保証
                }
                key = "";
            }
            //初期値設定
            //  日付切り替え時刻初期値
            if (ans.ContainsKey(HM_KEY) == false)
            {
                ans.Add(HM_KEY, "500");//時刻切り替え初期値は5:00
            }
            try{
                var hhmm = Convert.ToInt16(ans[HM_KEY]);
                if((hhmm < 0) || (hhmm > 2359))
                {
                    Program.d("HHMM format error: " + ans[HM_KEY] + ". User default value.");
                    ans[HM_KEY] = "500";
                }
            }
            catch (Exception fe)
            {
                ans[HM_KEY] = "500";
            }

            return ans;
        }

        private static void Serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string line = serial.ReadLine();
                while (line != null)
                {
                    try
                    {
                        parser.parse(line);
                        line = serial.ReadLine();
                    }
                    catch(TimeoutException te) { line = null; }
                    catch (Exception ex)
                    {
                        if (config.ContainsKey(DIR_KEY))
                        {
                            using (var sw = new StreamWriter(config[DIR_KEY]+"/error_log.txt", true))
                            {
                                sw.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")+" "+ex.ToString());
                                sw.WriteLine(" message: " + ex.Message);
                                sw.WriteLine(" source: " + ex.Source);
                                sw.WriteLine(" stack: " + ex.StackTrace);
                                sw.WriteLine("");
                                sw.Close();
                            }
                        }
                        line = null;
                    }
                }
            }
            catch (Exception ee) { }
            finally { }
        }

        static bool myHandler(CtrlTypes ctrlType)
        {
            isActive = false;
            if ((serial != null) && (serial.IsOpen))
            {
                serial.Close();
                serial.Dispose();
                serial = null;
            }
            return false;
        }
    }
}
