namespace MiotoServerW
{
    partial class Form1
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.dateTimePickerHHMM = new System.Windows.Forms.DateTimePicker();
            this.buttonUpdateComList = new System.Windows.Forms.Button();
            this.buttonStart = new System.Windows.Forms.Button();
            this.buttonStop = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxStatus = new System.Windows.Forms.TextBox();
            this.textBoxDbDir = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.buttonDbDir = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.textBoxBackupDir = new System.Windows.Forms.TextBox();
            this.buttonBackupDir = new System.Windows.Forms.Button();
            this.buttonDoBackup = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.listBoxComport = new System.Windows.Forms.ListBox();
            this.label9 = new System.Windows.Forms.Label();
            this.numericServerPortNumber = new System.Windows.Forms.NumericUpDown();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.checkBoxMemBackup = new System.Windows.Forms.CheckBox();
            this.dataGridViewSerialCurrent = new System.Windows.Forms.DataGridView();
            this.buttonUpdateSerialCurrentView = new System.Windows.Forms.Button();
            this.buttonRegSeriCurrentCfg = new System.Windows.Forms.Button();
            this.textBoxSerialCurrentUnitName = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.comboBoxSerialCurrent = new System.Windows.Forms.ComboBox();
            this.checkBoxAutoMinimize = new System.Windows.Forms.CheckBox();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericServerPortNumber)).BeginInit();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewSerialCurrent)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(2, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(126, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "Monostick用 COMポート";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(137, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "日付変更時刻";
            // 
            // dateTimePickerHHMM
            // 
            this.dateTimePickerHHMM.CustomFormat = "HH:mm";
            this.dateTimePickerHHMM.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePickerHHMM.Location = new System.Drawing.Point(220, 28);
            this.dateTimePickerHHMM.Name = "dateTimePickerHHMM";
            this.dateTimePickerHHMM.ShowUpDown = true;
            this.dateTimePickerHHMM.Size = new System.Drawing.Size(49, 19);
            this.dateTimePickerHHMM.TabIndex = 3;
            // 
            // buttonUpdateComList
            // 
            this.buttonUpdateComList.Location = new System.Drawing.Point(6, 91);
            this.buttonUpdateComList.Name = "buttonUpdateComList";
            this.buttonUpdateComList.Size = new System.Drawing.Size(84, 23);
            this.buttonUpdateComList.TabIndex = 1;
            this.buttonUpdateComList.Text = "ポート更新";
            this.buttonUpdateComList.UseVisualStyleBackColor = true;
            this.buttonUpdateComList.Click += new System.EventHandler(this.buttonUpdateComList_Click);
            // 
            // buttonStart
            // 
            this.buttonStart.Location = new System.Drawing.Point(419, 26);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(87, 23);
            this.buttonStart.TabIndex = 6;
            this.buttonStart.Text = "サービス開始";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // buttonStop
            // 
            this.buttonStop.Location = new System.Drawing.Point(514, 26);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(49, 23);
            this.buttonStop.TabIndex = 7;
            this.buttonStop.Text = "停止";
            this.buttonStop.UseVisualStyleBackColor = true;
            this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 122);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 12);
            this.label3.TabIndex = 7;
            this.label3.Text = "ステータス";
            // 
            // textBoxStatus
            // 
            this.textBoxStatus.Location = new System.Drawing.Point(8, 137);
            this.textBoxStatus.Multiline = true;
            this.textBoxStatus.Name = "textBoxStatus";
            this.textBoxStatus.ReadOnly = true;
            this.textBoxStatus.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxStatus.Size = new System.Drawing.Size(553, 107);
            this.textBoxStatus.TabIndex = 8;
            // 
            // textBoxDbDir
            // 
            this.textBoxDbDir.Location = new System.Drawing.Point(220, 61);
            this.textBoxDbDir.Name = "textBoxDbDir";
            this.textBoxDbDir.ReadOnly = true;
            this.textBoxDbDir.Size = new System.Drawing.Size(193, 19);
            this.textBoxDbDir.TabIndex = 4;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(137, 64);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(57, 12);
            this.label5.TabIndex = 10;
            this.label5.Text = "DB格納先";
            // 
            // buttonDbDir
            // 
            this.buttonDbDir.Location = new System.Drawing.Point(419, 61);
            this.buttonDbDir.Name = "buttonDbDir";
            this.buttonDbDir.Size = new System.Drawing.Size(49, 23);
            this.buttonDbDir.TabIndex = 5;
            this.buttonDbDir.Text = "選択";
            this.buttonDbDir.UseVisualStyleBackColor = true;
            this.buttonDbDir.Click += new System.EventHandler(this.buttonDbDir_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(137, 98);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(55, 12);
            this.label6.TabIndex = 13;
            this.label6.Text = "バックアップ";
            // 
            // textBoxBackupDir
            // 
            this.textBoxBackupDir.Location = new System.Drawing.Point(220, 95);
            this.textBoxBackupDir.Name = "textBoxBackupDir";
            this.textBoxBackupDir.ReadOnly = true;
            this.textBoxBackupDir.Size = new System.Drawing.Size(193, 19);
            this.textBoxBackupDir.TabIndex = 14;
            // 
            // buttonBackupDir
            // 
            this.buttonBackupDir.Location = new System.Drawing.Point(419, 93);
            this.buttonBackupDir.Name = "buttonBackupDir";
            this.buttonBackupDir.Size = new System.Drawing.Size(49, 23);
            this.buttonBackupDir.TabIndex = 15;
            this.buttonBackupDir.Text = "選択";
            this.buttonBackupDir.UseVisualStyleBackColor = true;
            this.buttonBackupDir.Click += new System.EventHandler(this.ButtonBackupDir_Click);
            // 
            // buttonDoBackup
            // 
            this.buttonDoBackup.Location = new System.Drawing.Point(474, 93);
            this.buttonDoBackup.Name = "buttonDoBackup";
            this.buttonDoBackup.Size = new System.Drawing.Size(87, 23);
            this.buttonDoBackup.TabIndex = 16;
            this.buttonDoBackup.Text = "バックアップ実行";
            this.buttonDoBackup.UseVisualStyleBackColor = true;
            this.buttonDoBackup.Click += new System.EventHandler(this.ButtonDoBackup_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(577, 283);
            this.tabControl1.TabIndex = 17;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.checkBoxAutoMinimize);
            this.tabPage1.Controls.Add(this.listBoxComport);
            this.tabPage1.Controls.Add(this.label9);
            this.tabPage1.Controls.Add(this.numericServerPortNumber);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.buttonDoBackup);
            this.tabPage1.Controls.Add(this.buttonBackupDir);
            this.tabPage1.Controls.Add(this.dateTimePickerHHMM);
            this.tabPage1.Controls.Add(this.textBoxBackupDir);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.label6);
            this.tabPage1.Controls.Add(this.buttonUpdateComList);
            this.tabPage1.Controls.Add(this.buttonStart);
            this.tabPage1.Controls.Add(this.buttonStop);
            this.tabPage1.Controls.Add(this.buttonDbDir);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this.label5);
            this.tabPage1.Controls.Add(this.textBoxStatus);
            this.tabPage1.Controls.Add(this.textBoxDbDir);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(569, 257);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "基本";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // listBoxComport
            // 
            this.listBoxComport.FormattingEnabled = true;
            this.listBoxComport.ItemHeight = 12;
            this.listBoxComport.Location = new System.Drawing.Point(8, 28);
            this.listBoxComport.Name = "listBoxComport";
            this.listBoxComport.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.listBoxComport.Size = new System.Drawing.Size(105, 52);
            this.listBoxComport.TabIndex = 19;
            this.listBoxComport.SelectedIndexChanged += new System.EventHandler(this.listBoxComport_SelectedIndexChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(275, 31);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(97, 12);
            this.label9.TabIndex = 18;
            this.label9.Text = "サーバのポート番号";
            // 
            // numericServerPortNumber
            // 
            this.numericServerPortNumber.Location = new System.Drawing.Point(378, 28);
            this.numericServerPortNumber.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numericServerPortNumber.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericServerPortNumber.Name = "numericServerPortNumber";
            this.numericServerPortNumber.Size = new System.Drawing.Size(35, 19);
            this.numericServerPortNumber.TabIndex = 17;
            this.numericServerPortNumber.Value = new decimal(new int[] {
            80,
            0,
            0,
            0});
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.checkBoxMemBackup);
            this.tabPage2.Controls.Add(this.dataGridViewSerialCurrent);
            this.tabPage2.Controls.Add(this.buttonUpdateSerialCurrentView);
            this.tabPage2.Controls.Add(this.buttonRegSeriCurrentCfg);
            this.tabPage2.Controls.Add(this.textBoxSerialCurrentUnitName);
            this.tabPage2.Controls.Add(this.label8);
            this.tabPage2.Controls.Add(this.label7);
            this.tabPage2.Controls.Add(this.comboBoxSerialCurrent);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(569, 257);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "電流";
            this.tabPage2.UseVisualStyleBackColor = true;
            this.tabPage2.Enter += new System.EventHandler(this.tabPage2_Enter);
            // 
            // checkBoxMemBackup
            // 
            this.checkBoxMemBackup.AutoSize = true;
            this.checkBoxMemBackup.Location = new System.Drawing.Point(9, 6);
            this.checkBoxMemBackup.Name = "checkBoxMemBackup";
            this.checkBoxMemBackup.Size = new System.Drawing.Size(345, 16);
            this.checkBoxMemBackup.TabIndex = 7;
            this.checkBoxMemBackup.Text = "DBと同じフォルダにバックアップを作成(変更は停止時に行ってください)";
            this.checkBoxMemBackup.UseVisualStyleBackColor = true;
            this.checkBoxMemBackup.CheckedChanged += new System.EventHandler(this.checkBoxMemBackup_CheckedChanged);
            // 
            // dataGridViewSerialCurrent
            // 
            this.dataGridViewSerialCurrent.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewSerialCurrent.Location = new System.Drawing.Point(204, 110);
            this.dataGridViewSerialCurrent.Name = "dataGridViewSerialCurrent";
            this.dataGridViewSerialCurrent.RowTemplate.Height = 21;
            this.dataGridViewSerialCurrent.Size = new System.Drawing.Size(362, 141);
            this.dataGridViewSerialCurrent.TabIndex = 6;
            this.dataGridViewSerialCurrent.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewSerialCurrent_CellValueChanged);
            // 
            // buttonUpdateSerialCurrentView
            // 
            this.buttonUpdateSerialCurrentView.Location = new System.Drawing.Point(9, 110);
            this.buttonUpdateSerialCurrentView.Name = "buttonUpdateSerialCurrentView";
            this.buttonUpdateSerialCurrentView.Size = new System.Drawing.Size(189, 23);
            this.buttonUpdateSerialCurrentView.TabIndex = 5;
            this.buttonUpdateSerialCurrentView.Text = "最新の情報に更新";
            this.buttonUpdateSerialCurrentView.UseVisualStyleBackColor = true;
            this.buttonUpdateSerialCurrentView.Click += new System.EventHandler(this.buttonUpdateSerialCurrentView_Click);
            // 
            // buttonRegSeriCurrentCfg
            // 
            this.buttonRegSeriCurrentCfg.Enabled = false;
            this.buttonRegSeriCurrentCfg.Location = new System.Drawing.Point(123, 192);
            this.buttonRegSeriCurrentCfg.Name = "buttonRegSeriCurrentCfg";
            this.buttonRegSeriCurrentCfg.Size = new System.Drawing.Size(75, 23);
            this.buttonRegSeriCurrentCfg.TabIndex = 4;
            this.buttonRegSeriCurrentCfg.Text = "設定保存";
            this.buttonRegSeriCurrentCfg.UseVisualStyleBackColor = true;
            this.buttonRegSeriCurrentCfg.Click += new System.EventHandler(this.buttonRegSeriCurrentCfg_Click);
            // 
            // textBoxSerialCurrentUnitName
            // 
            this.textBoxSerialCurrentUnitName.Location = new System.Drawing.Point(9, 194);
            this.textBoxSerialCurrentUnitName.Name = "textBoxSerialCurrentUnitName";
            this.textBoxSerialCurrentUnitName.Size = new System.Drawing.Size(108, 19);
            this.textBoxSerialCurrentUnitName.TabIndex = 3;
            this.textBoxSerialCurrentUnitName.TextChanged += new System.EventHandler(this.textBoxSerialCurrentUnitName_TextChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(7, 179);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(51, 12);
            this.label8.TabIndex = 2;
            this.label8.Text = "ユニット名";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(7, 141);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(39, 12);
            this.label7.TabIndex = 1;
            this.label7.Text = "ユニット";
            // 
            // comboBoxSerialCurrent
            // 
            this.comboBoxSerialCurrent.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSerialCurrent.FormattingEnabled = true;
            this.comboBoxSerialCurrent.Location = new System.Drawing.Point(9, 156);
            this.comboBoxSerialCurrent.Name = "comboBoxSerialCurrent";
            this.comboBoxSerialCurrent.Size = new System.Drawing.Size(189, 20);
            this.comboBoxSerialCurrent.TabIndex = 0;
            this.comboBoxSerialCurrent.SelectedIndexChanged += new System.EventHandler(this.comboBoxSerialCurrent_SelectedIndexChanged);
            // 
            // checkBoxAutoMinimize
            // 
            this.checkBoxAutoMinimize.AutoSize = true;
            this.checkBoxAutoMinimize.Location = new System.Drawing.Point(475, 64);
            this.checkBoxAutoMinimize.Name = "checkBoxAutoMinimize";
            this.checkBoxAutoMinimize.Size = new System.Drawing.Size(79, 16);
            this.checkBoxAutoMinimize.TabIndex = 20;
            this.checkBoxAutoMinimize.Text = "最小化する";
            this.checkBoxAutoMinimize.UseVisualStyleBackColor = true;
            this.checkBoxAutoMinimize.CheckedChanged += new System.EventHandler(this.checkBoxAutoMinimize_CheckedChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(601, 304);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.Text = "MiotoServer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericServerPortNumber)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewSerialCurrent)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DateTimePicker dateTimePickerHHMM;
        private System.Windows.Forms.Button buttonUpdateComList;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxStatus;
        private System.Windows.Forms.TextBox textBoxDbDir;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button buttonDbDir;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBoxBackupDir;
        private System.Windows.Forms.Button buttonBackupDir;
        private System.Windows.Forms.Button buttonDoBackup;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button buttonRegSeriCurrentCfg;
        private System.Windows.Forms.TextBox textBoxSerialCurrentUnitName;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox comboBoxSerialCurrent;
        private System.Windows.Forms.DataGridView dataGridViewSerialCurrent;
        private System.Windows.Forms.Button buttonUpdateSerialCurrentView;
        private System.Windows.Forms.CheckBox checkBoxMemBackup;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.NumericUpDown numericServerPortNumber;
        private System.Windows.Forms.ListBox listBoxComport;
        private System.Windows.Forms.CheckBox checkBoxAutoMinimize;
    }
}

