﻿namespace TEUpload
{
    partial class TEUploadDlg
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.styleManager1 = new DevComponents.DotNetBar.StyleManager(this.components);
            this.labelX1 = new DevComponents.DotNetBar.LabelX();
            this.PartNoLabel = new DevComponents.DotNetBar.LabelX();
            this.labelX3 = new DevComponents.DotNetBar.LabelX();
            this.OISLabel = new DevComponents.DotNetBar.LabelX();
            this.labelX5 = new DevComponents.DotNetBar.LabelX();
            listView1 = new System.Windows.Forms.ListView();
            this.OK = new DevComponents.DotNetBar.ButtonX();
            this.SuspendLayout();
            // 
            // styleManager1
            // 
            this.styleManager1.ManagerStyle = DevComponents.DotNetBar.eStyle.Windows7Blue;
            this.styleManager1.MetroColorParameters = new DevComponents.DotNetBar.Metro.ColorTables.MetroColorGeneratorParameters(System.Drawing.Color.White, System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154))))));
            // 
            // labelX1
            // 
            // 
            // 
            // 
            this.labelX1.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelX1.Font = new System.Drawing.Font("新細明體", 13F);
            this.labelX1.Location = new System.Drawing.Point(12, 12);
            this.labelX1.Name = "labelX1";
            this.labelX1.Size = new System.Drawing.Size(75, 23);
            this.labelX1.TabIndex = 0;
            this.labelX1.Text = "料號：";
            // 
            // PartNoLabel
            // 
            // 
            // 
            // 
            this.PartNoLabel.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.PartNoLabel.Font = new System.Drawing.Font("新細明體", 13F);
            this.PartNoLabel.Location = new System.Drawing.Point(66, 9);
            this.PartNoLabel.Name = "PartNoLabel";
            this.PartNoLabel.Size = new System.Drawing.Size(161, 23);
            this.PartNoLabel.TabIndex = 1;
            this.PartNoLabel.Text = "labelX2";
            // 
            // labelX3
            // 
            // 
            // 
            // 
            this.labelX3.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelX3.Font = new System.Drawing.Font("新細明體", 13F);
            this.labelX3.Location = new System.Drawing.Point(12, 53);
            this.labelX3.Name = "labelX3";
            this.labelX3.Size = new System.Drawing.Size(75, 23);
            this.labelX3.TabIndex = 2;
            this.labelX3.Text = "步序：";
            // 
            // OISLabel
            // 
            // 
            // 
            // 
            this.OISLabel.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.OISLabel.Font = new System.Drawing.Font("新細明體", 13F);
            this.OISLabel.Location = new System.Drawing.Point(66, 50);
            this.OISLabel.Name = "OISLabel";
            this.OISLabel.Size = new System.Drawing.Size(161, 23);
            this.OISLabel.TabIndex = 3;
            this.OISLabel.Text = "labelX4";
            // 
            // labelX5
            // 
            // 
            // 
            // 
            this.labelX5.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelX5.Font = new System.Drawing.Font("新細明體", 13F);
            this.labelX5.Location = new System.Drawing.Point(152, 97);
            this.labelX5.Name = "labelX5";
            this.labelX5.Size = new System.Drawing.Size(141, 23);
            this.labelX5.TabIndex = 4;
            this.labelX5.Text = "上傳檔案";
            // 
            // listView1
            // 
            listView1.Location = new System.Drawing.Point(12, 126);
            listView1.Name = "listView1";
            listView1.Size = new System.Drawing.Size(352, 227);
            listView1.TabIndex = 5;
            listView1.UseCompatibleStateImageBehavior = false;
            listView1.View = System.Windows.Forms.View.List;
            // 
            // OK
            // 
            this.OK.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.OK.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.OK.Location = new System.Drawing.Point(152, 359);
            this.OK.Name = "OK";
            this.OK.Size = new System.Drawing.Size(75, 23);
            this.OK.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.OK.TabIndex = 6;
            this.OK.Text = "OK";
            this.OK.Click += new System.EventHandler(this.OK_Click);
            // 
            // TEUploadDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(376, 394);
            this.Controls.Add(this.OK);
            this.Controls.Add(listView1);
            this.Controls.Add(this.labelX5);
            this.Controls.Add(this.OISLabel);
            this.Controls.Add(this.labelX3);
            this.Controls.Add(this.PartNoLabel);
            this.Controls.Add(this.labelX1);
            this.DoubleBuffered = true;
            this.Name = "TEUploadDlg";
            this.Text = "TEUploadDlg";
            this.Load += new System.EventHandler(this.TEUploadDlg_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private DevComponents.DotNetBar.StyleManager styleManager1;
        private DevComponents.DotNetBar.LabelX labelX1;
        private DevComponents.DotNetBar.LabelX PartNoLabel;
        private DevComponents.DotNetBar.LabelX labelX3;
        private DevComponents.DotNetBar.LabelX OISLabel;
        private DevComponents.DotNetBar.LabelX labelX5;
        private DevComponents.DotNetBar.ButtonX OK;
        public static System.Windows.Forms.ListView listView1;
    }
}