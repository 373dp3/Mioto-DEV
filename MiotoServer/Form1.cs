using MiotoBlazorCommon.Struct;
using MiotoServer;
using MiotoServer.CfgOption;
using MiotoServerW.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiotoServerW
{
    public partial class Form1 : Form
    {
        public static AppConfig config { get; private set; } = null;
        private string jsonFile = getBaseDir() + Path.DirectorySeparatorChar + "config.json";

        public Form1()
        {
            InitializeComponent();
            if (File.Exists(jsonFile))
            {
                try
                {
                    using (var sr = new StreamReader(jsonFile))
                    {
                        config = AppConfig.formJSON(sr.ReadToEnd());
                        sr.Close();
                    }
                    UpdateBlazorConfig();
                }
                catch (Exception e) { }
            }
            if(config==null)
            {
                config = new AppConfig() { dbdir = getBaseDir() };
                config.listComPort.Add(new ComPort() { portName = PORT_NO_USE_KEY, portBps = "115200" });
                config.listComPort.Add(new ComPort() { portName = PORT_NO_USE_KEY, portBps = "115200" });
            }
            config.appVer = GetAppVer();
        }
        /// <summary>
        /// Blazor WebassemblyとLocal Appと共用の設定をSQLiteDBに更新する。
        /// </summary>
        public static void UpdateBlazorConfig()
        {
            //Blazor用の日付変更設定保持
            var p = DbWrapper.getInstance();
            var msg = p.getConfig(WebSocketWorker.CONFIG_DB_KEY);
            var cfg = JsonSerializer.Deserialize<Config>(msg);
            cfg.dateLineHHMM = config.hhmm;
            cfg.appVer = config.appVer;
            msg = JsonSerializer.Serialize<Config>(cfg);
            DbWrapper.getInstance().setConfig(WebSocketWorker.CONFIG_DB_KEY, msg);
        }

        #region 共通関連
        const string PORT_NO_USE_KEY = AppConfig.PORT_NO_USE_KEY;
        private int dMsgCnt = 0;

        public void d(string msg)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action<string>(d), msg);
                }
                else
                {
                    if (dMsgCnt > 100) { this.textBoxStatus.Text = ""; dMsgCnt = 0; }
                    var hhmm = DateTime.Now.ToString("HH:mm:ss ");
                    this.textBoxStatus.AppendText(hhmm + msg + Environment.NewLine);
                    dMsgCnt++;

                }
            }
            catch (Exception e) { e.ToString(); }
        }

        private void buttonUpdateComList_Click(object sender, EventArgs e)
        {
            var preSelectAry = new string[] { PORT_NO_USE_KEY, PORT_NO_USE_KEY };
            var comboBoxComAry = new ComboBox[] { comboBoxComList, comboBoxComList2 };

            for(var i=0; i< preSelectAry.Length; i++)
            {
                if (i < config.listComPort.Count)
                {
                    preSelectAry[i] = config.listComPort[i].portName;
                }
                comboBoxComAry[i].Items.Clear();
                setComboItemAndUpdateSelected(comboBoxComAry[i], PORT_NO_USE_KEY, preSelectAry[i]);
            }

            var portlist = new List<string>();
            portlist.AddRange(SerialPort.GetPortNames());
            portlist.Sort();

            for (var i = 0; i < preSelectAry.Length; i++)
            {
                foreach(var port in portlist)
                {
                    setComboItemAndUpdateSelected(comboBoxComAry[i], port, preSelectAry[i]);
                }
            }

            //前回使用したポートは必ず残す
            for(var i=0; i< preSelectAry.Length; i++)
            {
                setComboItemAndUpdateSelected(comboBoxComAry[i], preSelectAry[i], preSelectAry[i]);
            }

            // nullなら未選択項目を指定
            for (var i = 0; i < preSelectAry.Length; i++)
            {
                if(comboBoxComAry[i] == null)
                {
                    comboBoxComAry[i].SelectedItem = PORT_NO_USE_KEY;
                }
            }
        }

        private void setComboItemAndUpdateSelected(ComboBox box, string item, string preSelected)
        {
            if (box.Items.Contains(item) == false)
            {
                box.Items.Add(item);
            }
            if (item.CompareTo(preSelected) != 0) { return; }
            box.SelectedItem = item;
        }

        private void saveConfigJson()
        {
            var json = config.toJSON();
            try
            {
                using (var sw = new StreamWriter(jsonFile, false))
                {
                    sw.Write(json);
                    sw.Close();
                }
                UpdateBlazorConfig();
            }
            catch(DirectoryNotFoundException dne) { }
            catch (Exception e)
            {

            }
        }

        private void comboBoxComList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(!(sender is ComboBox)) { return; }
            var box = (ComboBox)sender;

            if (box.Name.CompareTo(comboBoxComList.Name) == 0)
            {
                config.listComPort[0].portName = box.SelectedItem.ToString();
                if(comboBoxComList2.SelectedIndex == box.SelectedIndex)
                {
                    comboBoxComList2.SelectedIndex = 0;
                }
                if (config.listComPort[0].portName.CompareTo(PORT_NO_USE_KEY) != 0)
                {
                    saveConfigJson();
                }
            }
            if (box.Name.CompareTo(comboBoxComList2.Name) == 0)
            {
                config.listComPort[1].portName = box.SelectedItem.ToString();
                if (comboBoxComList.SelectedIndex == box.SelectedIndex)
                {
                    comboBoxComList.SelectedIndex = 0;
                }
                if (config.listComPort[1].portName.CompareTo(PORT_NO_USE_KEY) != 0)
                {
                    saveConfigJson();
                }
            }
        }

        private static string getBaseDir()
        {
            var oldBase = Environment.GetFolderPath(Environment.SpecialFolder.Personal)
                + Path.DirectorySeparatorChar + "MitoServerDb";

            if(Directory.Exists(oldBase)) { return oldBase; }

            return Environment.GetFolderPath(Environment.SpecialFolder.Personal)
                    + Path.DirectorySeparatorChar + "MiotoServerDb";
        }
        private string GetAppVer()
        {
            return FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
        }
        private void Form1_Shown(object sender, EventArgs e)
        {
            //タイトルにバージョン情報を追加
            this.Text += " "+ GetAppVer();

            //COMポート初期値更新
            buttonUpdateComList_Click(null, null);

            //bps初期値更新            
            comboBoxBps.SelectedItem = config.listComPort[0].portBps;
            comboBoxBps2.SelectedItem = config.listComPort[1].portBps;

            //Server port number
            numericServerPortNumber.Value = config.serverPortNumber;

            //DBフォルダ
            if ((config.dbdir == null)
                || (config.dbdir.Length == 0))
            {
                config.dbdir = getBaseDir();
                saveConfigJson();
            }
            textBoxDbDir.Text = config.dbdir;

            //Backupフォルダ
            textBoxBackupDir.Text = config.backupDir;

            //timepicker更新
            var hhmm = config.hhmm;
            var hh = (int)(hhmm / 100);
            var mm = (hhmm - 100 * hh);
            var hhmmStr = string.Format("{0:D2}:{1:D2}", hh, mm);
            dateTimePickerHHMM.Value = DateTime.Parse(DateTime.Now.ToString("yyyy/MM/dd")+" "+hhmmStr+":00");

            //サービス開始
            buttonStart_Click(null, null);

            d("config: " + jsonFile);
        }

        private Process p = null;
        private Thread thPoling = null;

        private void buttonStart_Click(object sender, EventArgs e)
        {
            //timepicker情報をpropに更新
            var hhmm = dateTimePickerHHMM.Value.ToString("HH:mm");
            hhmm = hhmm.Replace(":", "");
            config.hhmm = Convert.ToInt32(hhmm);

            //ポート設定更新確認
            config.serverPortNumber = (int)numericServerPortNumber.Value;
            if (config.serverPortNumber != config.serverPortNumberPre)
            {
                try
                {
                    Installer.updateFirewall(config.serverPortNumber);
                    config.serverPortNumberPre = config.serverPortNumber;
                }
                catch(Exception ex)
                {
                    d("エラー: "+ex.Message);
                    return;
                }
            }

            //設定の保存
            saveConfigJson();

            updateButtonCtrl(true);

            tokenSource = new CancellationTokenSource();

            config.func = d;
            config.dbdir = textBoxDbDir.Text;
            config.appVer = GetAppVer();

            var wrapper = MiotoServerWrapper.getInstance(config);
            Task.Run(() => wrapper.start(tokenSource.Token)).ConfigureAwait(false);

        }

        CancellationTokenSource tokenSource = null;

        private void doBackupThreadLoop()
        {
            var thHHMM = getHHMM(dateTimePickerHHMM.Value.AddMinutes(5));
            try
            {
                var preHHMM = getHHMM(DateTime.Now);
                while (thPoling.IsAlive)
                {
                    //時間経過を判定した後にバックアップ処理の呼出
                    var hhmm = getHHMM(DateTime.Now);
                    if ((preHHMM < thHHMM) && (hhmm >= thHHMM))
                    {
                        ButtonDoBackup_Click(null, null);
                    }
                    preHHMM = hhmm;
                    Thread.Sleep(2000);
                }
            }
            catch(ThreadAbortException te) { }
            catch (Exception e)
            {
                d("Polling thread exception:" + e.ToString());
            }
        }

        private int getHHMM(DateTime dt)
        {
            var str = dt.ToString("HHmm");
            return Convert.ToInt32(str);
        }


        private void P_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            d(e.Data);
        }

        /// <summary>
        /// ボタン状態を更新する
        /// </summary>
        /// <param name="isStart"></param>
        private void updateButtonCtrl(bool isStart)
        {
            this.comboBoxComList.Enabled = !isStart;
            this.comboBoxComList2.Enabled = !isStart;
            this.comboBoxBps.Enabled = !isStart;
            this.comboBoxBps2.Enabled = !isStart;
            this.dateTimePickerHHMM.Enabled = !isStart;
            this.buttonStart.Enabled = !isStart;
            this.buttonUpdateComList.Enabled = !isStart;
            this.textBoxDbDir.Enabled = !isStart;
            this.buttonDbDir.Enabled = !isStart;
            this.textBoxBackupDir.Enabled = !isStart;
            this.buttonBackupDir.Enabled = !isStart;
            this.checkBoxMemBackup.Enabled = !isStart;
            this.numericServerPortNumber.Enabled = !isStart;

            this.buttonStop.Enabled = isStart;
            this.buttonDoBackup.Enabled = isStart;
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            stopServiceIfExist();
            updateButtonCtrl(false);
        }

        private void stopServiceIfExist(bool isExit = false)
        {
            if (tokenSource != null)
            {
                tokenSource.Cancel();
            }
            if(isExit) Task.Run(() => {
                //シリアルポート処理の終了待機
                while (MiotoServerWrapper.getInstance().isSerialPortStop == false)
                {
                    Thread.Sleep(100);
                }
                Application.Exit();
            });
            tokenSource = null;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //SerialPortを確実に終了させないとObjectDisposedExceptionが発生するため
            if (tokenSource != null) { e.Cancel = true; }
            stopServiceIfExist(true);
        }

        private void comboBoxBps_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (sender == null) { return; }
            if(!(sender is ComboBox)) { return; }

            var box = (ComboBox)sender;
            if (box.Name.CompareTo(comboBoxBps.Name) == 0)
            {
                config.listComPort[0].portBps = box.SelectedItem.ToString();
                saveConfigJson();
            }
            if (box.Name.CompareTo(comboBoxBps2.Name) == 0)
            {
                config.listComPort[1].portBps = box.SelectedItem.ToString();
                saveConfigJson();
            }
        }

        private void buttonDbDir_Click(object sender, EventArgs e)
        {
            var dlg = new FolderBrowserDialog();
            dlg.Description = "データの格納先フォルダを選択してください";
            dlg.SelectedPath = this.textBoxDbDir.Text;
            dlg.ShowNewFolderButton = true;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                this.textBoxDbDir.Text = dlg.SelectedPath;
                config.dbdir = dlg.SelectedPath;
                saveConfigJson();
            }
        }

        private void ButtonBackupDir_Click(object sender, EventArgs e)
        {
            var dlg = new FolderBrowserDialog();
            dlg.Description = "バックアップ先フォルダを選択してください";
            dlg.SelectedPath = this.textBoxBackupDir.Text;
            dlg.ShowNewFolderButton = true;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                config.backupDir = dlg.SelectedPath;
                this.textBoxBackupDir.Text = config.backupDir;
                saveConfigJson();
            }
        }

        private Dictionary<string, string> getBackupFileDict()
        {
            var ans = new Dictionary<string, string>();
            var keys = new string[] { "", "pal", "twe", "t2525" };
            var filePrefix = "mioto_backup";
            foreach (var key in keys)
            {
                var targetFile = this.textBoxBackupDir.Text + Path.DirectorySeparatorChar + filePrefix + "_";
                if (key.Length == 0)
                {
                    targetFile += "ct.csv";
                }
                else
                {
                    targetFile += key + ".csv";
                }
                ans.Add(key, targetFile);
            }

            return ans;
        }

        private void ButtonDoBackup_Click(object sender, EventArgs e)
        {
            if (this.textBoxBackupDir.Text.Length==0)
            {
                if (sender != null) d("バックアップ先フォルダ指定されていません。");
                return;
            }
            if (Directory.Exists(this.textBoxBackupDir.Text) == false)
            {
                if (sender != null) d("バックアップ先フォルダが存在しないためバックアップを中断しました。");
                return;
            }
            Task.Run(() => {
                using (var wc = new WebClient())
                {
                    var dt = DateTime.Parse("2016/1/1 0:00:00");
                    var files = getBackupFileDict();
                    foreach (var file in files.Values)
                    {
                        if (File.Exists(file))
                        {
                            var fileDt = File.GetLastWriteTime(file);
                            if (dt < fileDt) { dt = fileDt; }
                        }
                    }
                    foreach (var key in files.Keys)
                    {
                        var targetFile = files[key];

                        var url = "http://localhost/" + key + "/backup/-1d/";
                        url += dt.ToString("yyyyMMdd");

                        var contents = wc.DownloadString(url);
                        if ((contents.Length > 5) || (File.Exists(targetFile) == false))
                        {
                            d("backup file:" + targetFile);
                            d("url:" + url);
                            using (var sw = new StreamWriter(targetFile, true, Encoding.ASCII))
                            {
                                sw.Write(contents);
                            }
                        }
                    }
                }
            }).ConfigureAwait(false);

        }
        #endregion //--共通関連

        #region 電流関連

        private void tabPage2_Enter(object sender, EventArgs e)
        {
            updateSerialCurrentView();
        }

        /// <summary>
        /// タブグループの電流欄を更新
        /// </summary>
        private void updateSerialCurrentView()
        {
            var c = MiotoServerWrapper.config;
            var flg = buttonRegSeriCurrentCfg.Enabled;
            if (this.comboBoxSerialCurrent.Items != null) {
                this.comboBoxSerialCurrent.Items.Clear();
            }
            foreach(var item in c.listSerialCurrent)
            {
                this.comboBoxSerialCurrent.Items.Add(item.name + " (" + item.mac.ToString("X") + ")");
                comboBoxSerialCurrent.SelectedIndex = 0;
            }
            buttonRegSeriCurrentCfg.Enabled = flg;
            this.checkBoxMemBackup.Checked = c.isMemoryDbBackup;
        }

        private void comboBoxSerialCurrent_SelectedIndexChanged(object sender, EventArgs e)
        {
            var c = MiotoServerWrapper.config;
            var item = c.listSerialCurrent.ElementAt(comboBoxSerialCurrent.SelectedIndex);
            textBoxSerialCurrentUnitName.Text = item.name;
            dataGridViewSerialCurrent.DataSource = item.listChInfo;
        }

        private void buttonUpdateSerialCurrentView_Click(object sender, EventArgs e)
        {
            updateSerialCurrentView();
        }

        private void buttonRegSeriCurrentCfg_Click(object sender, EventArgs e)
        {
            var c = MiotoServerWrapper.config;
            var item = c.listSerialCurrent.ElementAt<SerialCurrent>(this.comboBoxSerialCurrent.SelectedIndex);
            item.name = this.textBoxSerialCurrentUnitName.Text;
            saveConfigJson();
            buttonRegSeriCurrentCfg.Enabled = false;
        }


        private void dataGridViewSerialCurrent_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            buttonRegSeriCurrentCfg.Enabled = true;
        }

        private void textBoxSerialCurrentUnitName_TextChanged(object sender, EventArgs e)
        {
            buttonRegSeriCurrentCfg.Enabled = true;
        }

        private void checkBoxMemBackup_CheckedChanged(object sender, EventArgs e)
        {
            MiotoServerWrapper.config.isMemoryDbBackup = this.checkBoxMemBackup.Checked;
            config.isMemoryDbBackup = MiotoServerWrapper.config.isMemoryDbBackup;
            saveConfigJson();
        }

        #endregion //-- 電流関連


    }
}
