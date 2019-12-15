using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace CalcHelper
{
    public partial class Form1 : Form
    {
        private static Form1 instance = null;
        public static bool isActive { get; private set; }
        HttpdWorker httpd = null;
        public Form1()
        {
            isActive = true;
            InitializeComponent();
        }


        private static int logCnt = 0;
        public static void log(string msg)
        {
            if(instance == null) { return; }
            if (instance.InvokeRequired)
            {
                try
                {
                    instance.Invoke(new Action<string>(log), msg);
                }
                catch (Exception e) { log(e.ToString()); }
            }
            else
            {
                if (logCnt > 100) {
                    instance.textBoxStatus.Text = "";
                    logCnt = 0;
                }
                var dt = DateTime.Now;
                instance.textBoxStatus.AppendText(dt.ToLongTimeString()+"\t");
                instance.textBoxStatus.AppendText(msg + Environment.NewLine);
                logCnt++;
            }
        }

        private void buttonPingManualCheck_Click(object sender, EventArgs e)
        {
            var address = this.textBoxPing.Text;
            var checker = new ProcessChecker();
            var result = checker.isNetworkActive(address);
            if (result)
            {
                Properties.Settings.Default.pingHost = address;
                Properties.Settings.Default.Save();
            }
            log("ping result : " + result);
            log(checker.pingResult);
        }

        private void checkBoxPeriodicCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxPeriodicCheck.Checked)
            {
                startWorkerThread();
            }else
            {
                stopWorkerThread();
            }
        }

        //定期的な確認処理
        private void fetchWorker(string msg)
        {
            if(instance==null) { return; }
            if (InvokeRequired)
            {
                instance.Invoke(new Action<string>(fetchWorker), "");
            }
            else
            {
                if (this.checkBoxMultiCalc.Checked == false)
                {
                    //単一ファイル時
                    ch.fetchSingle(this);
                }
                else
                {
                    //複数ファイル時
                    var docName = ch.getDocumentNameInProcess();
                    if (docName == null)
                    {
                        //起動処理
                        button1_Click(null, null);
                    }
                    else
                    {
                        //キャプチャ処理
                        ch.fetchSingle(this);

                        //終了処理
                        Thread.Sleep(1000);
                        SendKeys.SendWait("{ESC}");
                        Thread.Sleep(1000);
                        SendKeys.SendWait("^q");
                        Thread.Sleep(1000);
                        SendKeys.SendWait("d");
                    }
                }
            }
        }

        public string getPingAddress()
        {
            return this.textBoxPing.Text;
        }
        private Thread workerTh = null;
        private bool isEnableWorkerTh = false;
        ProcessChecker ch = new ProcessChecker();
        private void startWorkerThread()
        {
            if(workerTh != null) { return; }
            isEnableWorkerTh = true;
            workerTh = new Thread(()=>{
                log("Threadを開始しました");
                while (isEnableWorkerTh)
                {
                    try
                    {
                        fetchWorker("");
                    }
                    catch (Exception e)
                    {
                        log(e.ToString());
                    }

                    Thread.Sleep(10*1000);
                }
            });
            workerTh.Start();
        }
        public void stopWorkerThread()
        {
            isActive = false;
            if(workerTh == null) { return; }
            log("スレッドを終了しています");
            isEnableWorkerTh = false;
            workerTh.Join(5 * 1000);
            workerTh = null;
            log("スレッドを終了しました");
            log("httpサービススレッド終了を待機しています");
            if(httpd!=null) httpd.Stop();
            httpd = null;
            log("スレッドを終了しました");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            stopWorkerThread();
        }

        bool isBootOpDone = false;
        bool isMinimizedOnce = false;
        private void Form1_Shown(object sender, EventArgs e)
        {
            //タイトルにバージョン情報を追加
            var ver = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
            this.Text += " " + ver;

            log("form shown");
            if(isBootOpDone) { return; }
            this.checkBoxMultiCalc.Checked = Properties.Settings.Default.isMultiCalc;
            checkBoxMultiCalc_CheckedChanged(null, null);
            this.textBoxPing.Text = Properties.Settings.Default.pingHost;
            if ((Properties.Settings.Default.calcFolder.Length > 0)
                && (Directory.Exists(Properties.Settings.Default.calcFolder)))
            {
                this.textBoxCalcFolder.Text = Properties.Settings.Default.calcFolder;
            }
            this.checkBoxPeriodicCheck.Checked = true;

            if (isMinimizedOnce == false)
            {
                (new Thread(() => {
                    try
                    {
                        Thread.Sleep(1000);
                        log("10秒後にこのウィンドウを最小化します。");
                        Thread.Sleep(10 * 1000);
                        this.WindowState = FormWindowState.Minimized;
                    }
                    catch (Exception ex) { log(ex.ToString()); }

                })).Start();
                isMinimizedOnce = true;
            }
            isBootOpDone = true;
            instance = this;

            if (this.checkBoxPeriodicCheck.Checked)
            {
                log("httpサーバサービスを起動します");
                httpd = new HttpdWorker();
                httpd.Run();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            log("load");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //*
            var docName = ch.getDocumentNameInProcess();
            if (docName == null)
            {
                ch.fetchMultiOds(this.textBoxCalcFolder.Text);
                return;
            }

            /*
            var p = MemoryDb.getInstance();

            log("(" + docName + ") " + HttpUtility.UrlEncode(docName));

            using (var bmp = ScreenCapture.CaptureActiveWindow())
            using (var mem = new MemoryStream(1024 * 1024))//1MBのメモリストリーム
            {
                var dt = DateTime.Now.ToString("HHmmss");
                bmp.Save(mem, ImageFormat.Jpeg);
                p.insertImage(HttpUtility.UrlEncode(docName), mem.ToArray());
            }
            //*/
        }

        public const string DEFAULT_IMG_NAME = "default";
        public void captureFowerGoundApp()
        {
            var p = MemoryDb.getInstance();
            //var desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            var docName = ch.getDocumentNameInProcess();
            if(docName == null) { log("Calc not found");  return; }
            log("("+docName+") " + HttpUtility.UrlEncode(docName));

            using (var bmp = ScreenCapture.CaptureActiveWindow())
            using(var mem = new MemoryStream(1024*1024))//1MBのメモリストリーム
            {
                var dt = DateTime.Now.ToString("HHmmss");
                bmp.Save(mem, ImageFormat.Jpeg);
                //p.insertImage(DEFAULT_IMG_NAME, mem.ToArray());
                p.insertImage(HttpUtility.UrlEncode(docName), mem.ToArray());
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var fbd = new FolderBrowserDialog();
            if((this.textBoxCalcFolder.Text.Length>0)
                &&(Directory.Exists(this.textBoxCalcFolder.Text)))
            {
                fbd.SelectedPath = this.textBoxCalcFolder.Text;
            }
            if(fbd.ShowDialog(this)== DialogResult.OK)
            {
                this.textBoxCalcFolder.Text = fbd.SelectedPath;
                Properties.Settings.Default.calcFolder = this.textBoxCalcFolder.Text;
                Properties.Settings.Default.Save();
            }
        }

        private void checkBoxMultiCalc_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBoxMultiCalc.Checked)
            {
                //MessageBox.Show("複数ファイル参照機能は未実装です");
                //this.checkBoxMultiCalc.Checked = false;
            }else
            {
            }
            Properties.Settings.Default.isMultiCalc = this.checkBoxMultiCalc.Checked;
            this.textBoxCalcFolder.Enabled = this.checkBoxMultiCalc.Checked;
            this.buttonFolderSelect.Enabled = this.checkBoxMultiCalc.Checked;

            Properties.Settings.Default.Save();
        }
    }
}
