using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace CalcHelper
{
    class ProcessChecker
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        //soffice.bin	[DEMO-Online]CT.ods(読み取り専用) - LibreOffice Calc
        Regex ptnCalc = new Regex("(.*) - LibreOffice Calc$", RegexOptions.IgnoreCase);

        public void rebootOS()
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "shutdown.exe";
            //コマンドラインを指定
            psi.Arguments = "/r /t 20 /c \"ネット不通の為\" ";
            //ウィンドウを表示しないようにする
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            //起動
            Process p = Process.Start(psi);
        }

        public Process getDocumentProcess()
        {
            var procList = Process.GetProcesses();
            foreach (var proc in procList)
            {
                if (proc.MainWindowTitle == null) { continue; }
                if (proc.MainWindowTitle.Length == 0) { continue; }

                //log(proc.ProcessName + "\t" + proc.MainWindowTitle);
                if (proc.ProcessName.CompareTo("soffice.bin") != 0) { continue; }
                if (ptnCalc.IsMatch(proc.MainWindowTitle) == false) { continue; }

                return proc;
            }
            return null;
        }

        public string getDocumentNameInProcess()
        {
            var proc = getDocumentProcess();
            if(proc==null) { return null; }

            var m = ptnCalc.Match(proc.MainWindowTitle);
            if(m.Success==false) { return null; }

            return m.Groups[1].Value.ToString();
        }

        public bool isCalcResponding(Process proc)
        {
            return proc.Responding;
        }
        private List<string> listFetchedProc = new List<string>();
        public void setCalcFullScreenIfNeed(Process proc, bool isThread=false)
        {
            //最大化、ESC、最大化と2回送出することで、現状が最大化済みかに関係なく
            //画面を最大化する事ができる。また、LibreOfficeのバージョンによって、
            //最大化時にボタンが残ってしまう現象の回避・軽減に期待ができる。

            //実施済みのプロセスは処理しない
            if (listFetchedProc.Contains(proc.MainWindowTitle))
            {
                log("最大化実施済み");
                return;
            }
            try
            {
                log(" 最大化処理開始");
                var rect = new RECT();
                log(" 領域取得");
                GetClientRect(proc.MainWindowHandle, out rect);
                log(" 条件確認");
                if ((Screen.PrimaryScreen.Bounds.Height != rect.Height)
                    || (Screen.PrimaryScreen.Bounds.Width != rect.Width))
                {
                    log(" screen is not full");
                }
                else
                {
                    log(" screen is full");
                }
                log("size:" + rect.Width + "," + rect.Height);

                SetForegroundWindow(proc.MainWindowHandle);
                Thread.Sleep(1000);
                sendKeys("{ESC}", isThread);
                Thread.Sleep(2000);
                sendKeys("^+j", isThread);
                Thread.Sleep(2000);
                sendKeys("{ESC}", isThread);
                Thread.Sleep(2000);
                sendKeys("^+j", isThread);
                Thread.Sleep(2000);
                listFetchedProc.Add(proc.MainWindowTitle);

            }
            catch (Exception e)
            {
                log(e.ToString());
            }
        }

        private void sendKeys(string msg, bool isThread)
        {
            if (isThread)
            {
                SendKeys.SendWait(msg);
            }
            else
            {
                SendKeys.Send(msg);
            }
        }

        Regex ptnRepair = new Regex("^LibreOffice(.*)ドキュメントの回復$", RegexOptions.IgnoreCase);
        public bool fetchRepairDialogIfExist()
        {
            //回復ウィンドウが表示される場合
            //soffice.bin	LibreOffice 6.1 ドキュメントの回復
            //となる。ESCキーで回避可能なため、このプロセスを検出した場合、
            //最前面に移動させてESCを送る
            var procList = Process.GetProcesses();
            foreach(var proc in procList)
            {
                if(proc.MainWindowTitle == null) { continue; }
                if(proc.MainWindowTitle.Length==0) { continue; }
                //log(proc.ProcessName + "\t" + proc.MainWindowTitle);
                if(proc.ProcessName.CompareTo("soffice.bin")!=0) { continue; }
                if(ptnRepair.IsMatch(proc.MainWindowTitle)==false) { continue; }

                SetForegroundWindow(proc.MainWindowHandle);
                SendKeys.Send("{ESC}");
                return true;
            }

            return false;
        }

        public string pingResult {get; private set;}

        public bool isNetworkActive(string server)
        {
            var psi = new ProcessStartInfo();
            psi.FileName = "ping";
            psi.Arguments = "-n 2 "+server;

            //ウィンドウを表示しないようにする
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.RedirectStandardOutput = true;
            //起動
            var p = Process.Start(psi);
            p.WaitForExit();
            pingResult = p.StandardOutput.ReadToEnd();
            //var result = p.ExitCode == 0; //終了コードでは判定できない場合がある
            var result = pingResult.Contains("TTL");//正常応答時には結果にTTLの文字が含まれる
            p.Dispose();

            return result;
        }

        public void log(string msg)
        {
            Form1.log(msg);
        }

        DateTime rebootDt = DateTime.Now.AddDays(31);

        public void fetchSingle(Form1 form, bool isMultiAccess=false)
        {
            try
            {
                //回復ウィンドウが出ていれば回避処理を実施
                log("回復ウィンドウの確認");
                fetchRepairDialogIfExist();

                var proc = getDocumentProcess();
                if ((proc != null) && (isMultiAccess == false))
                {
                    var respon = isCalcResponding(proc);
                    log("responding:" + respon);
                    if (respon == false)
                    {
                        if (rebootDt < DateTime.Now.AddSeconds(60))
                        {
                            int rest = (int)((rebootDt - DateTime.Now).TotalSeconds);
                            log(string.Format("{0}秒後に再起動します", rest));

                        }
                        else
                        {
                            log("プログラムの応答がありません。1分後に再起動します。");
                            rebootDt = DateTime.Now.AddSeconds(60);
                        }
                    }
                    else
                    {
                        log("プログラムから応答がありました。");
                        //log(ch.pingResult);
                        rebootDt = DateTime.Now.AddDays(31);
                    }
                    log("画面最大化確認:" + proc.MainWindowTitle);
                    setCalcFullScreenIfNeed(proc);

                    log("キャプチャ");
                    form.captureFowerGoundApp();
                }
                else
                {
                    if (isMultiAccess == false) { log("LibreOfficeが見つかりません"); }
                }

                if (form.getPingAddress().Length > 0)
                {
                    var isNetOk = isNetworkActive(form.getPingAddress());
                    if (isNetOk == false)
                    {
                        if (rebootDt < DateTime.Now.AddSeconds(60))
                        {
                            int rest = (int)((rebootDt - DateTime.Now).TotalSeconds);
                            log(string.Format("{0}秒後に再起動します", rest));

                        }
                        else
                        {
                            log("ネットワークの応答がありません。1分後に再起動します。");
                            rebootDt = DateTime.Now.AddSeconds(60);
                        }
                    }
                    else
                    {
                        log("ネットワークから応答がありました。");
                        //log(ch.pingResult);
                        rebootDt = DateTime.Now.AddDays(31);
                    }
                }
                if (DateTime.Now > rebootDt)
                {
                    //chekerに対して再起動指示
                    log("再起動します。");
                    rebootOS();
                    form.stopWorkerThread();
                }
            }
            catch (Exception e)
            {
                log(e.ToString());
            }

        }

        private List<string> odsFiles = new List<string>();

        public void fetchMultiOds(string dirPath)
        {
            if (Directory.Exists(dirPath) == false)
            {
                log("ディレクトリが見つかりませんでした。" + dirPath);
                return;
            }
            Regex ptnOds = new Regex(".*\\.ods$", RegexOptions.IgnoreCase);
            if (odsFiles.Count == 0)
            {
                //ディレクトリ一覧の取得
                var fileList = Directory.GetFiles(dirPath);
                foreach (var file in fileList)
                {
                    if (ptnOds.IsMatch(file) == false) { continue; }
                    odsFiles.Add(file);
                }
            }
            if (odsFiles.Count > 0)
            {
                var file = odsFiles[0];
                odsFiles.RemoveAt(0);
                log(file + "を処理します");
                openCalcOdsFile(file);
            }
        }

        private void openCalcOdsFile(string odsFilePath)
        {
            var p = new Process();
            //cmd.exe
            p.StartInfo.FileName = @"C:\Program Files\LibreOffice\program\soffice.exe";
            p.StartInfo.UseShellExecute = false;
            //p.StartInfo.CreateNoWindow = true;
            p.StartInfo.Arguments = " --nolockcheck --nologo --nofirststartwizard \"" + odsFilePath+"\" ";
            p.Start();
            listFetchedProc.Clear();//最大化実施済みフラグのクリア
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left, Top, Right, Bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public RECT(System.Drawing.Rectangle r) : this(r.Left, r.Top, r.Right, r.Bottom) { }

            public int X
            {
                get { return Left; }
                set { Right -= (Left - value); Left = value; }
            }

            public int Y
            {
                get { return Top; }
                set { Bottom -= (Top - value); Top = value; }
            }

            public int Height
            {
                get { return Bottom - Top; }
                set { Bottom = value + Top; }
            }

            public int Width
            {
                get { return Right - Left; }
                set { Right = value + Left; }
            }

            public System.Drawing.Point Location
            {
                get { return new System.Drawing.Point(Left, Top); }
                set { X = value.X; Y = value.Y; }
            }

            public System.Drawing.Size Size
            {
                get { return new System.Drawing.Size(Width, Height); }
                set { Width = value.Width; Height = value.Height; }
            }

            public static implicit operator System.Drawing.Rectangle(RECT r)
            {
                return new System.Drawing.Rectangle(r.Left, r.Top, r.Width, r.Height);
            }

            public static implicit operator RECT(System.Drawing.Rectangle r)
            {
                return new RECT(r);
            }

            public static bool operator ==(RECT r1, RECT r2)
            {
                return r1.Equals(r2);
            }

            public static bool operator !=(RECT r1, RECT r2)
            {
                return !r1.Equals(r2);
            }

            public bool Equals(RECT r)
            {
                return r.Left == Left && r.Top == Top && r.Right == Right && r.Bottom == Bottom;
            }

            public override bool Equals(object obj)
            {
                if (obj is RECT)
                    return Equals((RECT)obj);
                else if (obj is System.Drawing.Rectangle)
                    return Equals(new RECT((System.Drawing.Rectangle)obj));
                return false;
            }

            public override int GetHashCode()
            {
                return ((System.Drawing.Rectangle)this).GetHashCode();
            }

            public override string ToString()
            {
                return string.Format(System.Globalization.CultureInfo.CurrentCulture, "{{Left={0},Top={1},Right={2},Bottom={3}}}", Left, Top, Right, Bottom);
            }
        }
    }
}
