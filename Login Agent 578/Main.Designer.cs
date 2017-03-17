namespace Login_Agent_578
{
    partial class Main
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.maintainanceCheckBox = new System.Windows.Forms.CheckBox();
            this.lbZA = new System.Windows.Forms.Label();
            this.lbZaStatus = new System.Windows.Forms.Label();
            this.lbPort = new System.Windows.Forms.Label();
            this.LaMsgLog = new System.Windows.Forms.RichTextBox();
            this.lbPortStatus = new System.Windows.Forms.Label();
            this.btnReload = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lbUidStatus = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lbUserCount = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // maintainanceCheckBox
            // 
            this.maintainanceCheckBox.AutoSize = true;
            this.maintainanceCheckBox.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.maintainanceCheckBox.Location = new System.Drawing.Point(13, 45);
            this.maintainanceCheckBox.Name = "maintainanceCheckBox";
            this.maintainanceCheckBox.Size = new System.Drawing.Size(159, 18);
            this.maintainanceCheckBox.TabIndex = 32;
            this.maintainanceCheckBox.Text = "Server Maintainance";
            this.maintainanceCheckBox.UseVisualStyleBackColor = true;
            this.maintainanceCheckBox.CheckedChanged += new System.EventHandler(this.maintainanceCheckBox_CheckedChanged);
            // 
            // lbZA
            // 
            this.lbZA.AutoSize = true;
            this.lbZA.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbZA.Location = new System.Drawing.Point(10, 25);
            this.lbZA.Name = "lbZA";
            this.lbZA.Size = new System.Drawing.Size(77, 14);
            this.lbZA.TabIndex = 33;
            this.lbZA.Text = "ZoneAgent:";
            // 
            // lbZaStatus
            // 
            this.lbZaStatus.AutoSize = true;
            this.lbZaStatus.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbZaStatus.Location = new System.Drawing.Point(83, 25);
            this.lbZaStatus.Name = "lbZaStatus";
            this.lbZaStatus.Size = new System.Drawing.Size(14, 14);
            this.lbZaStatus.TabIndex = 35;
            this.lbZaStatus.Text = "0";
            // 
            // lbPort
            // 
            this.lbPort.AutoSize = true;
            this.lbPort.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbPort.Location = new System.Drawing.Point(10, 10);
            this.lbPort.Name = "lbPort";
            this.lbPort.Size = new System.Drawing.Size(91, 14);
            this.lbPort.TabIndex = 33;
            this.lbPort.Text = "Listen Port:";
            // 
            // LaMsgLog
            // 
            this.LaMsgLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LaMsgLog.BackColor = System.Drawing.Color.Black;
            this.LaMsgLog.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LaMsgLog.ForeColor = System.Drawing.Color.DarkGray;
            this.LaMsgLog.Location = new System.Drawing.Point(6, 100);
            this.LaMsgLog.Name = "LaMsgLog";
            this.LaMsgLog.ReadOnly = true;
            this.LaMsgLog.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.LaMsgLog.Size = new System.Drawing.Size(362, 110);
            this.LaMsgLog.TabIndex = 80;
            this.LaMsgLog.Text = "";
            // 
            // lbPortStatus
            // 
            this.lbPortStatus.AutoSize = true;
            this.lbPortStatus.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbPortStatus.Location = new System.Drawing.Point(96, 10);
            this.lbPortStatus.Name = "lbPortStatus";
            this.lbPortStatus.Size = new System.Drawing.Size(35, 14);
            this.lbPortStatus.TabIndex = 35;
            this.lbPortStatus.Text = "3550";
            // 
            // btnReload
            // 
            this.btnReload.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnReload.Location = new System.Drawing.Point(248, 4);
            this.btnReload.Name = "btnReload";
            this.btnReload.Size = new System.Drawing.Size(120, 60);
            this.btnReload.TabIndex = 81;
            this.btnReload.Text = "RELOAD\r\n*AllowStatus.SDB\r\n*BanIP.txt\r\n*AllowIP.txt";
            this.btnReload.UseVisualStyleBackColor = true;
            this.btnReload.Click += new System.EventHandler(this.btnReload_Click);
            // 
            // label1
            // 
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label1.Location = new System.Drawing.Point(5, 70);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(365, 2);
            this.label1.TabIndex = 82;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 75);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 14);
            this.label2.TabIndex = 83;
            this.label2.Text = "Unique ID:";
            // 
            // lbUidStatus
            // 
            this.lbUidStatus.AutoSize = true;
            this.lbUidStatus.Location = new System.Drawing.Point(83, 75);
            this.lbUidStatus.Name = "lbUidStatus";
            this.lbUidStatus.Size = new System.Drawing.Size(14, 14);
            this.lbUidStatus.TabIndex = 83;
            this.lbUidStatus.Text = "0";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(175, 75);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(84, 14);
            this.label3.TabIndex = 83;
            this.label3.Text = "User Count:";
            // 
            // lbUserCount
            // 
            this.lbUserCount.AutoSize = true;
            this.lbUserCount.Location = new System.Drawing.Point(255, 75);
            this.lbUserCount.Name = "lbUserCount";
            this.lbUserCount.Size = new System.Drawing.Size(77, 14);
            this.lbUserCount.TabIndex = 83;
            this.lbUserCount.Text = "0(L:0/Z:0)";
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(374, 216);
            this.Controls.Add(this.lbUidStatus);
            this.Controls.Add(this.lbUserCount);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnReload);
            this.Controls.Add(this.LaMsgLog);
            this.Controls.Add(this.lbPortStatus);
            this.Controls.Add(this.lbZaStatus);
            this.Controls.Add(this.lbPort);
            this.Controls.Add(this.lbZA);
            this.Controls.Add(this.maintainanceCheckBox);
            this.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Main";
            this.Text = "Login Agent v578";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
            this.Load += new System.EventHandler(this.Main_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox maintainanceCheckBox;
        private System.Windows.Forms.Label lbZA;
        private System.Windows.Forms.Label lbZaStatus;
        private System.Windows.Forms.Label lbPort;
        private System.Windows.Forms.RichTextBox LaMsgLog;
        private System.Windows.Forms.Label lbPortStatus;
        private System.Windows.Forms.Button btnReload;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lbUidStatus;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lbUserCount;
    }
}

