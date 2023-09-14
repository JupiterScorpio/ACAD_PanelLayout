namespace LicenseDebugger
{
    partial class LicenseDebuggerForm
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
            this.txtLicenseFileName = new System.Windows.Forms.TextBox();
            this.lblLicenseFileName = new System.Windows.Forms.Label();
            this.btnBrowseForFolder = new System.Windows.Forms.Button();
            this.txtLicensePath = new System.Windows.Forms.TextBox();
            this.lblLicensePath = new System.Windows.Forms.Label();
            this.txtGUID = new System.Windows.Forms.TextBox();
            this.dateTimeLimit = new System.Windows.Forms.DateTimePicker();
            this.lblEthernetAddress = new System.Windows.Forms.Label();
            this.txtEthernetAddress = new System.Windows.Forms.TextBox();
            this.txtCADFormatLock = new System.Windows.Forms.TextBox();
            this.btnGetLicenseData = new System.Windows.Forms.Button();
            this.lblGUID = new System.Windows.Forms.Label();
            this.lblCADFormatLock = new System.Windows.Forms.Label();
            this.lblDateLimit = new System.Windows.Forms.Label();
            this.txtDateLimit = new System.Windows.Forms.TextBox();
            this.lstEthernetAddresses = new System.Windows.Forms.ListBox();
            this.lblValidAddresses = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtLicenseFileName
            // 
            this.txtLicenseFileName.AllowDrop = true;
            this.txtLicenseFileName.Location = new System.Drawing.Point(210, 116);
            this.txtLicenseFileName.Margin = new System.Windows.Forms.Padding(4);
            this.txtLicenseFileName.Name = "txtLicenseFileName";
            this.txtLicenseFileName.Size = new System.Drawing.Size(311, 22);
            this.txtLicenseFileName.TabIndex = 23;         
            this.txtLicenseFileName.DragDrop += new System.Windows.Forms.DragEventHandler(this.txtLicenseFileName_DragDrop);
            this.txtLicenseFileName.DragEnter += new System.Windows.Forms.DragEventHandler(this.txtLicenseFileName_DragEnter);
            // 
            // lblLicenseFileName
            // 
            this.lblLicenseFileName.AutoSize = true;
            this.lblLicenseFileName.Location = new System.Drawing.Point(32, 120);
            this.lblLicenseFileName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblLicenseFileName.Name = "lblLicenseFileName";
            this.lblLicenseFileName.Size = new System.Drawing.Size(118, 17);
            this.lblLicenseFileName.TabIndex = 22;
            this.lblLicenseFileName.Text = "License file name";
            // 
            // btnBrowseForFolder
            // 
            this.btnBrowseForFolder.Location = new System.Drawing.Point(536, 76);
            this.btnBrowseForFolder.Margin = new System.Windows.Forms.Padding(4);
            this.btnBrowseForFolder.Name = "btnBrowseForFolder";
            this.btnBrowseForFolder.Size = new System.Drawing.Size(35, 28);
            this.btnBrowseForFolder.TabIndex = 21;
            this.btnBrowseForFolder.Text = "...";
            this.btnBrowseForFolder.UseVisualStyleBackColor = true;
            this.btnBrowseForFolder.Click += new System.EventHandler(this.btnBrowseForFolder_Click);
            // 
            // txtLicensePath
            // 
            this.txtLicensePath.Location = new System.Drawing.Point(210, 78);
            this.txtLicensePath.Margin = new System.Windows.Forms.Padding(4);
            this.txtLicensePath.Name = "txtLicensePath";
            this.txtLicensePath.Size = new System.Drawing.Size(311, 22);
            this.txtLicensePath.TabIndex = 20;
            // 
            // lblLicensePath
            // 
            this.lblLicensePath.AutoSize = true;
            this.lblLicensePath.Location = new System.Drawing.Point(32, 82);
            this.lblLicensePath.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblLicensePath.Name = "lblLicensePath";
            this.lblLicensePath.Size = new System.Drawing.Size(171, 17);
            this.lblLicensePath.TabIndex = 19;
            this.lblLicensePath.Text = "Folder path for license file";
            // 
            // txtGUID
            // 
            this.txtGUID.Location = new System.Drawing.Point(210, 191);
            this.txtGUID.Margin = new System.Windows.Forms.Padding(4);
            this.txtGUID.Name = "txtGUID";
            this.txtGUID.ReadOnly = true;
            this.txtGUID.Size = new System.Drawing.Size(278, 22);
            this.txtGUID.TabIndex = 29;
            // 
            // dateTimeLimit
            // 
            this.dateTimeLimit.Location = new System.Drawing.Point(210, 307);
            this.dateTimeLimit.Margin = new System.Windows.Forms.Padding(4);
            this.dateTimeLimit.Name = "dateTimeLimit";
            this.dateTimeLimit.Size = new System.Drawing.Size(183, 22);
            this.dateTimeLimit.TabIndex = 28;
            this.dateTimeLimit.Visible = false;
            // 
            // lblEthernetAddress
            // 
            this.lblEthernetAddress.AutoSize = true;
            this.lblEthernetAddress.Location = new System.Drawing.Point(32, 267);
            this.lblEthernetAddress.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblEthernetAddress.Name = "lblEthernetAddress";
            this.lblEthernetAddress.Size = new System.Drawing.Size(118, 17);
            this.lblEthernetAddress.TabIndex = 26;
            this.lblEthernetAddress.Text = "Ethernet Address";
            // 
            // txtEthernetAddress
            // 
            this.txtEthernetAddress.Location = new System.Drawing.Point(210, 263);
            this.txtEthernetAddress.Margin = new System.Windows.Forms.Padding(4);
            this.txtEthernetAddress.Name = "txtEthernetAddress";
            this.txtEthernetAddress.ReadOnly = true;
            this.txtEthernetAddress.Size = new System.Drawing.Size(357, 22);
            this.txtEthernetAddress.TabIndex = 25;
            // 
            // txtCADFormatLock
            // 
            this.txtCADFormatLock.Location = new System.Drawing.Point(210, 227);
            this.txtCADFormatLock.Margin = new System.Windows.Forms.Padding(4);
            this.txtCADFormatLock.Name = "txtCADFormatLock";
            this.txtCADFormatLock.ReadOnly = true;
            this.txtCADFormatLock.Size = new System.Drawing.Size(165, 22);
            this.txtCADFormatLock.TabIndex = 24;
            // 
            // btnGetLicenseData
            // 
            this.btnGetLicenseData.Location = new System.Drawing.Point(210, 151);
            this.btnGetLicenseData.Margin = new System.Windows.Forms.Padding(4);
            this.btnGetLicenseData.Name = "btnGetLicenseData";
            this.btnGetLicenseData.Size = new System.Drawing.Size(311, 28);
            this.btnGetLicenseData.TabIndex = 32;
            this.btnGetLicenseData.Text = "Get license file data";
            this.btnGetLicenseData.UseVisualStyleBackColor = true;
            this.btnGetLicenseData.Click += new System.EventHandler(this.btnGetLicenseData_Click);
            // 
            // lblGUID
            // 
            this.lblGUID.AutoSize = true;
            this.lblGUID.Location = new System.Drawing.Point(32, 194);
            this.lblGUID.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblGUID.Name = "lblGUID";
            this.lblGUID.Size = new System.Drawing.Size(42, 17);
            this.lblGUID.TabIndex = 33;
            this.lblGUID.Text = "GUID";
            // 
            // lblCADFormatLock
            // 
            this.lblCADFormatLock.AutoSize = true;
            this.lblCADFormatLock.Location = new System.Drawing.Point(32, 227);
            this.lblCADFormatLock.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCADFormatLock.Name = "lblCADFormatLock";
            this.lblCADFormatLock.Size = new System.Drawing.Size(118, 17);
            this.lblCADFormatLock.TabIndex = 34;
            this.lblCADFormatLock.Text = "CAD Format Lock";
            // 
            // lblDateLimit
            // 
            this.lblDateLimit.AutoSize = true;
            this.lblDateLimit.Location = new System.Drawing.Point(32, 309);
            this.lblDateLimit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDateLimit.Name = "lblDateLimit";
            this.lblDateLimit.Size = new System.Drawing.Size(71, 17);
            this.lblDateLimit.TabIndex = 35;
            this.lblDateLimit.Text = "Date Limit";
            // 
            // txtDateLimit
            // 
            this.txtDateLimit.Location = new System.Drawing.Point(210, 306);
            this.txtDateLimit.Margin = new System.Windows.Forms.Padding(4);
            this.txtDateLimit.Name = "txtDateLimit";
            this.txtDateLimit.ReadOnly = true;
            this.txtDateLimit.Size = new System.Drawing.Size(183, 22);
            this.txtDateLimit.TabIndex = 36;
            // 
            // lstEthernetAddresses
            // 
            this.lstEthernetAddresses.FormattingEnabled = true;
            this.lstEthernetAddresses.ItemHeight = 16;
            this.lstEthernetAddresses.Location = new System.Drawing.Point(205, 12);
            this.lstEthernetAddresses.Name = "lstEthernetAddresses";
            this.lstEthernetAddresses.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.lstEthernetAddresses.Size = new System.Drawing.Size(362, 52);
            this.lstEthernetAddresses.TabIndex = 37;
            // 
            // lblValidAddresses
            // 
            this.lblValidAddresses.AutoSize = true;
            this.lblValidAddresses.Location = new System.Drawing.Point(32, 12);
            this.lblValidAddresses.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblValidAddresses.Name = "lblValidAddresses";
            this.lblValidAddresses.Size = new System.Drawing.Size(168, 17);
            this.lblValidAddresses.TabIndex = 38;
            this.lblValidAddresses.Text = "Valid Ethernet Addresses";
            // 
            // LicenseDebuggerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 352);
            this.Controls.Add(this.lblValidAddresses);
            this.Controls.Add(this.lstEthernetAddresses);
            this.Controls.Add(this.txtDateLimit);
            this.Controls.Add(this.lblDateLimit);
            this.Controls.Add(this.lblCADFormatLock);
            this.Controls.Add(this.lblGUID);
            this.Controls.Add(this.btnGetLicenseData);
            this.Controls.Add(this.txtGUID);
            this.Controls.Add(this.dateTimeLimit);
            this.Controls.Add(this.lblEthernetAddress);
            this.Controls.Add(this.txtEthernetAddress);
            this.Controls.Add(this.txtCADFormatLock);
            this.Controls.Add(this.txtLicenseFileName);
            this.Controls.Add(this.lblLicenseFileName);
            this.Controls.Add(this.btnBrowseForFolder);
            this.Controls.Add(this.txtLicensePath);
            this.Controls.Add(this.lblLicensePath);
            this.Name = "LicenseDebuggerForm";
            this.Text = "License debugger";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtLicenseFileName;
        private System.Windows.Forms.Label lblLicenseFileName;
        private System.Windows.Forms.Button btnBrowseForFolder;
        private System.Windows.Forms.TextBox txtLicensePath;
        private System.Windows.Forms.Label lblLicensePath;
        private System.Windows.Forms.TextBox txtGUID;
        private System.Windows.Forms.DateTimePicker dateTimeLimit;
        private System.Windows.Forms.Label lblEthernetAddress;
        private System.Windows.Forms.TextBox txtEthernetAddress;
        private System.Windows.Forms.TextBox txtCADFormatLock;
        private System.Windows.Forms.Button btnGetLicenseData;
        private System.Windows.Forms.Label lblGUID;
        private System.Windows.Forms.Label lblCADFormatLock;
        private System.Windows.Forms.Label lblDateLimit;
        private System.Windows.Forms.TextBox txtDateLimit;
        private System.Windows.Forms.ListBox lstEthernetAddresses;
        private System.Windows.Forms.Label lblValidAddresses;
    }
}

