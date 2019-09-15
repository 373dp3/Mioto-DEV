﻿using MiotoServerW.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiotoServerW
{
    public partial class Form1 : Form
    {
        const string PORT_NO_USE_KEY = "使用しない";
        private int dMsgCnt = 0;

        public Form1()
        {
            InitializeComponent();
        }

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

            string preSelect = Settings.Default.comport;
            string preSelect2 = Settings.Default.comport2;
            this.comboBoxComList.Items.Clear();
            this.comboBoxComList2.Items.Clear();

            setComboItemAndUpdateSelected(comboBoxComList, PORT_NO_USE_KEY, preSelect);
            setComboItemAndUpdateSelected(comboBoxComList2, PORT_NO_USE_KEY, preSelect2);

            var portlist = new List<string>();
            portlist.AddRange(SerialPort.GetPortNames());
            portlist.Sort();
            foreach (var port in portlist)
            {
                setComboItemAndUpdateSelected(comboBoxComList, port, preSelect);
            }
            foreach (var port in portlist)
            {
                setComboItemAndUpdateSelected(comboBoxComList2, port, preSelect2);
            }

            //前回使用したポートは必ず残す
            setComboItemAndUpdateSelected(comboBoxComList, preSelect, preSelect);
            setComboItemAndUpdateSelected(comboBoxComList2, preSelect2, preSelect2);

            if (comboBoxComList.SelectedItem == null)
            {
                comboBoxComList.SelectedItem = PORT_NO_USE_KEY;
            }
            if (comboBoxComList2.SelectedItem == null)
            {
                comboBoxComList2.SelectedItem = PORT_NO_USE_KEY;
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

        private void comboBoxComList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(!(sender is ComboBox)) { return; }
            var box = (ComboBox)sender;

            if (box.Name.CompareTo(comboBoxComList.Name) == 0)
            {
                Settings.Default.comport = box.SelectedItem.ToString();
                if(comboBoxComList2.SelectedIndex == box.SelectedIndex)
                {
                    comboBoxComList2.SelectedIndex = 0;
                }
                if (Settings.Default.comport.CompareTo(PORT_NO_USE_KEY) != 0)
                {
                    Settings.Default.Save();
                }
            }
            if (box.Name.CompareTo(comboBoxComList2.Name) == 0)
            {
                Settings.Default.comport2 = box.SelectedItem.ToString();
                if (comboBoxComList.SelectedIndex == box.SelectedIndex)
                {
                    comboBoxComList.SelectedIndex = 0;
                }
                if (Settings.Default.comport2.CompareTo(PORT_NO_USE_KEY) != 0)
                {
                    Settings.Default.Save();
                }
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            //COMポート初期値更新
            buttonUpdateComList_Click(null, null);

            //bps初期値更新
            comboBoxBps.SelectedItem = Settings.Default.bps;
            comboBoxBps2.SelectedItem = Settings.Default.bps2;

            //DBフォルダ
            if ((Settings.Default.dbDir == null)
                || (Settings.Default.dbDir.Length==0))
            {
                Settings.Default.dbDir = 
                    Environment.GetFolderPath(Environment.SpecialFolder.Personal)
                    + "\\MitoServerDb";
                Settings.Default.Save();
            }
            textBoxDbDir.Text = Settings.Default.dbDir;

            //timepicker更新
            var hhmm = Settings.Default.HHMM;
            dateTimePickerHHMM.Value = DateTime.Parse(DateTime.Now.ToString("yyyy/MM/dd")+" "+hhmm+":00");

            //サービス開始
            buttonStart_Click(null, null);
        }

        private Process p = null;

        private void buttonStart_Click(object sender, EventArgs e)
        {
            //timepicker情報をpropに更新
            var hhmm = dateTimePickerHHMM.Value.ToString("HH:mm");
            Settings.Default.HHMM = hhmm;
            Settings.Default.Save();
            hhmm = hhmm.Replace(":", "");

            //引数の整理
            var dir = Directory.GetParent(Assembly.GetEntryAssembly().Location).FullName;
            var exe = dir + "\\MiotoServer.exe";
            var arg = "-hm "+hhmm;//-hm 500 -p COM5
            var portList = new List<string>();
            var bpsList = new List<string>();
            var boxComList = new List<ComboBox> { comboBoxComList, comboBoxComList2 };
            var boxBpsList = new List<ComboBox> { comboBoxBps, comboBoxBps2 };

            for(var i=0; i< boxComList.Count; i++)
            {
                var port = boxComList[i].SelectedItem.ToString();
                if (port.CompareTo(PORT_NO_USE_KEY) == 0) { continue; }
                portList.Add(port);
                bpsList.Add(boxBpsList[i].SelectedItem.ToString());
            }

            if (portList.Count > 0)
            {
                arg += " -p " + String.Join(",", portList);
                arg += " -bps " + String.Join(",", bpsList);
            }

            arg += " -d \"" + textBoxDbDir.Text+"\" "; 

            d(exe);
            d(arg);

            //プロセス準備
            p = new Process();
            p.StartInfo.FileName = exe;
            p.StartInfo.Arguments = arg;

            //  出力をストリームに書き出し
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.CreateNoWindow = true;//ウィンドウ非表示
            //  onDataRecievedハンドラ追加
            p.OutputDataReceived += P_OutputDataReceived;

            //  プロセス起動
            p.Start();
            p.BeginOutputReadLine();//非同期読み取り開始

            updateButtonCtrl(true);

        }

        private void P_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            d(e.Data);
        }

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

            this.buttonStop.Enabled = isStart;
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            stopServiceIfExist();
            updateButtonCtrl(false);
        }

        private void stopServiceIfExist()
        {
            if (p != null)
            {
                try
                {
                    d("サービスを終了しています");
                    p.Kill();
                    p.WaitForExit(5000);
                    p.Close();
                    p = null;
                    d("サービスを終了しました");

                }
                catch (Exception ee) { d(ee.ToString()); }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            stopServiceIfExist();
        }

        private void comboBoxBps_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (sender == null) { return; }
            if(!(sender is ComboBox)) { return; }

            var box = (ComboBox)sender;
            if (box.Name.CompareTo(comboBoxBps.Name) == 0)
            {
                Settings.Default.bps = box.SelectedItem.ToString();
                Settings.Default.Save();
            }
            if (box.Name.CompareTo(comboBoxBps2.Name) == 0)
            {
                Settings.Default.bps2 = box.SelectedItem.ToString();
                Settings.Default.Save();
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
                Settings.Default.dbDir = dlg.SelectedPath;
                Settings.Default.Save();
                Settings.Default.Save();
            }
        }
    }
}