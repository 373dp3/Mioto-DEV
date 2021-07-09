namespace CalcHelper
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.textBoxStatus = new System.Windows.Forms.TextBox();
            this.textBoxPing = new System.Windows.Forms.TextBox();
            this.buttonPingManualCheck = new System.Windows.Forms.Button();
            this.checkBoxPeriodicCheck = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxCalcFolder = new System.Windows.Forms.TextBox();
            this.buttonFolderSelect = new System.Windows.Forms.Button();
            this.checkBoxMultiCalc = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // textBoxStatus
            // 
            this.textBoxStatus.Location = new System.Drawing.Point(12, 163);
            this.textBoxStatus.Multiline = true;
            this.textBoxStatus.Name = "textBoxStatus";
            this.textBoxStatus.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxStatus.Size = new System.Drawing.Size(443, 178);
            this.textBoxStatus.TabIndex = 0;
            // 
            // textBoxPing
            // 
            this.textBoxPing.Location = new System.Drawing.Point(12, 24);
            this.textBoxPing.Name = "textBoxPing";
            this.textBoxPing.Size = new System.Drawing.Size(237, 19);
            this.textBoxPing.TabIndex = 5;
            // 
            // buttonPingManualCheck
            // 
            this.buttonPingManualCheck.Location = new System.Drawing.Point(255, 22);
            this.buttonPingManualCheck.Name = "buttonPingManualCheck";
            this.buttonPingManualCheck.Size = new System.Drawing.Size(75, 23);
            this.buttonPingManualCheck.TabIndex = 6;
            this.buttonPingManualCheck.Text = "確認";
            this.buttonPingManualCheck.UseVisualStyleBackColor = true;
            this.buttonPingManualCheck.Click += new System.EventHandler(this.buttonPingManualCheck_Click);
            // 
            // checkBoxPeriodicCheck
            // 
            this.checkBoxPeriodicCheck.AutoSize = true;
            this.checkBoxPeriodicCheck.Location = new System.Drawing.Point(12, 141);
            this.checkBoxPeriodicCheck.Name = "checkBoxPeriodicCheck";
            this.checkBoxPeriodicCheck.Size = new System.Drawing.Size(136, 16);
            this.checkBoxPeriodicCheck.TabIndex = 7;
            this.checkBoxPeriodicCheck.Text = "10秒毎に自動確認する";
            this.checkBoxPeriodicCheck.UseVisualStyleBackColor = true;
            this.checkBoxPeriodicCheck.CheckedChanged += new System.EventHandler(this.checkBoxPeriodicCheck_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(146, 12);
            this.label1.TabIndex = 8;
            this.label1.Text = "ネットワーク確認アドレス(ping)";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 68);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(241, 12);
            this.label2.TabIndex = 10;
            this.label2.Text = "複数表示用LibreOffice Calcファイル保存フォルダ";
            // 
            // textBoxCalcFolder
            // 
            this.textBoxCalcFolder.Location = new System.Drawing.Point(12, 83);
            this.textBoxCalcFolder.Name = "textBoxCalcFolder";
            this.textBoxCalcFolder.Size = new System.Drawing.Size(237, 19);
            this.textBoxCalcFolder.TabIndex = 11;
            // 
            // buttonFolderSelect
            // 
            this.buttonFolderSelect.Location = new System.Drawing.Point(255, 81);
            this.buttonFolderSelect.Name = "buttonFolderSelect";
            this.buttonFolderSelect.Size = new System.Drawing.Size(75, 23);
            this.buttonFolderSelect.TabIndex = 12;
            this.buttonFolderSelect.Text = "フォルダ選択";
            this.buttonFolderSelect.UseVisualStyleBackColor = true;
            this.buttonFolderSelect.Click += new System.EventHandler(this.button2_Click);
            // 
            // checkBoxMultiCalc
            // 
            this.checkBoxMultiCalc.AutoSize = true;
            this.checkBoxMultiCalc.Location = new System.Drawing.Point(12, 49);
            this.checkBoxMultiCalc.Name = "checkBoxMultiCalc";
            this.checkBoxMultiCalc.Size = new System.Drawing.Size(356, 16);
            this.checkBoxMultiCalc.TabIndex = 13;
            this.checkBoxMultiCalc.Text = "複数ファイルを参照する(未チェック時は起動済みのCalcを対象とします)";
            this.checkBoxMultiCalc.UseVisualStyleBackColor = true;
            this.checkBoxMultiCalc.CheckedChanged += new System.EventHandler(this.checkBoxMultiCalc_CheckedChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(467, 353);
            this.Controls.Add(this.checkBoxMultiCalc);
            this.Controls.Add(this.buttonFolderSelect);
            this.Controls.Add(this.textBoxCalcFolder);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.checkBoxPeriodicCheck);
            this.Controls.Add(this.buttonPingManualCheck);
            this.Controls.Add(this.textBoxPing);
            this.Controls.Add(this.textBoxStatus);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "CalcHelper";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxStatus;
        private System.Windows.Forms.TextBox textBoxPing;
        private System.Windows.Forms.Button buttonPingManualCheck;
        private System.Windows.Forms.CheckBox checkBoxPeriodicCheck;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxCalcFolder;
        private System.Windows.Forms.Button buttonFolderSelect;
        private System.Windows.Forms.CheckBox checkBoxMultiCalc;
    }
}

