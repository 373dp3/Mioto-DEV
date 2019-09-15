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
            this.comboBoxComList = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.dateTimePickerHHMM = new System.Windows.Forms.DateTimePicker();
            this.buttonUpdateComList = new System.Windows.Forms.Button();
            this.buttonStart = new System.Windows.Forms.Button();
            this.buttonStop = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxStatus = new System.Windows.Forms.TextBox();
            this.comboBoxBps = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxDbDir = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.buttonDbDir = new System.Windows.Forms.Button();
            this.comboBoxComList2 = new System.Windows.Forms.ComboBox();
            this.comboBoxBps2 = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // comboBoxComList
            // 
            this.comboBoxComList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxComList.FormattingEnabled = true;
            this.comboBoxComList.Location = new System.Drawing.Point(12, 24);
            this.comboBoxComList.Name = "comboBoxComList";
            this.comboBoxComList.Size = new System.Drawing.Size(92, 20);
            this.comboBoxComList.TabIndex = 0;
            this.comboBoxComList.SelectedIndexChanged += new System.EventHandler(this.comboBoxComList_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(126, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "Monostick用 COMポート";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(294, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "日付変更時刻";
            // 
            // dateTimePickerHHMM
            // 
            this.dateTimePickerHHMM.CustomFormat = "HH:mm";
            this.dateTimePickerHHMM.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePickerHHMM.Location = new System.Drawing.Point(294, 25);
            this.dateTimePickerHHMM.Name = "dateTimePickerHHMM";
            this.dateTimePickerHHMM.ShowUpDown = true;
            this.dateTimePickerHHMM.Size = new System.Drawing.Size(77, 19);
            this.dateTimePickerHHMM.TabIndex = 3;
            // 
            // buttonUpdateComList
            // 
            this.buttonUpdateComList.Location = new System.Drawing.Point(110, 22);
            this.buttonUpdateComList.Name = "buttonUpdateComList";
            this.buttonUpdateComList.Size = new System.Drawing.Size(75, 23);
            this.buttonUpdateComList.TabIndex = 1;
            this.buttonUpdateComList.Text = "ポート更新";
            this.buttonUpdateComList.UseVisualStyleBackColor = true;
            this.buttonUpdateComList.Click += new System.EventHandler(this.buttonUpdateComList_Click);
            // 
            // buttonStart
            // 
            this.buttonStart.Location = new System.Drawing.Point(477, 22);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(88, 23);
            this.buttonStart.TabIndex = 6;
            this.buttonStart.Text = "サービス開始";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // buttonStop
            // 
            this.buttonStop.Location = new System.Drawing.Point(477, 49);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(88, 23);
            this.buttonStop.TabIndex = 7;
            this.buttonStop.Text = "停止";
            this.buttonStop.UseVisualStyleBackColor = true;
            this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 109);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 12);
            this.label3.TabIndex = 7;
            this.label3.Text = "ステータス";
            // 
            // textBoxStatus
            // 
            this.textBoxStatus.Location = new System.Drawing.Point(12, 124);
            this.textBoxStatus.Multiline = true;
            this.textBoxStatus.Name = "textBoxStatus";
            this.textBoxStatus.ReadOnly = true;
            this.textBoxStatus.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxStatus.Size = new System.Drawing.Size(553, 125);
            this.textBoxStatus.TabIndex = 8;
            // 
            // comboBoxBps
            // 
            this.comboBoxBps.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxBps.FormattingEnabled = true;
            this.comboBoxBps.Items.AddRange(new object[] {
            "38400",
            "115200"});
            this.comboBoxBps.Location = new System.Drawing.Point(191, 24);
            this.comboBoxBps.Name = "comboBoxBps";
            this.comboBoxBps.Size = new System.Drawing.Size(97, 20);
            this.comboBoxBps.TabIndex = 2;
            this.comboBoxBps.SelectedIndexChanged += new System.EventHandler(this.comboBoxBps_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(191, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(80, 12);
            this.label4.TabIndex = 8;
            this.label4.Text = "COM速度(bps)";
            // 
            // textBoxDbDir
            // 
            this.textBoxDbDir.Location = new System.Drawing.Point(87, 81);
            this.textBoxDbDir.Name = "textBoxDbDir";
            this.textBoxDbDir.ReadOnly = true;
            this.textBoxDbDir.Size = new System.Drawing.Size(284, 19);
            this.textBoxDbDir.TabIndex = 4;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 84);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(69, 12);
            this.label5.TabIndex = 10;
            this.label5.Text = "データ格納先";
            // 
            // buttonDbDir
            // 
            this.buttonDbDir.Location = new System.Drawing.Point(377, 79);
            this.buttonDbDir.Name = "buttonDbDir";
            this.buttonDbDir.Size = new System.Drawing.Size(63, 23);
            this.buttonDbDir.TabIndex = 5;
            this.buttonDbDir.Text = "選択";
            this.buttonDbDir.UseVisualStyleBackColor = true;
            this.buttonDbDir.Click += new System.EventHandler(this.buttonDbDir_Click);
            // 
            // comboBoxComList2
            // 
            this.comboBoxComList2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxComList2.FormattingEnabled = true;
            this.comboBoxComList2.Location = new System.Drawing.Point(12, 49);
            this.comboBoxComList2.Name = "comboBoxComList2";
            this.comboBoxComList2.Size = new System.Drawing.Size(92, 20);
            this.comboBoxComList2.TabIndex = 11;
            // 
            // comboBoxBps2
            // 
            this.comboBoxBps2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxBps2.FormattingEnabled = true;
            this.comboBoxBps2.Items.AddRange(new object[] {
            "38400",
            "115200"});
            this.comboBoxBps2.Location = new System.Drawing.Point(191, 49);
            this.comboBoxBps2.Name = "comboBoxBps2";
            this.comboBoxBps2.Size = new System.Drawing.Size(97, 20);
            this.comboBoxBps2.TabIndex = 12;
            this.comboBoxBps2.SelectedIndexChanged += new System.EventHandler(this.comboBoxBps_SelectedIndexChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(577, 261);
            this.Controls.Add(this.comboBoxBps2);
            this.Controls.Add(this.comboBoxComList2);
            this.Controls.Add(this.buttonDbDir);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBoxDbDir);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.comboBoxBps);
            this.Controls.Add(this.textBoxStatus);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.buttonStop);
            this.Controls.Add(this.buttonStart);
            this.Controls.Add(this.buttonUpdateComList);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dateTimePickerHHMM);
            this.Controls.Add(this.comboBoxComList);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "Form1";
            this.Text = "MiotoServer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxComList;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DateTimePicker dateTimePickerHHMM;
        private System.Windows.Forms.Button buttonUpdateComList;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxStatus;
        private System.Windows.Forms.ComboBox comboBoxBps;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxDbDir;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button buttonDbDir;
        private System.Windows.Forms.ComboBox comboBoxComList2;
        private System.Windows.Forms.ComboBox comboBoxBps2;
    }
}

