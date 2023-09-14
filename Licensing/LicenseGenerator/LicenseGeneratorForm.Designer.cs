using CommonUtilities;

namespace LicenseGenerator
{
    partial class LicenseGeneratorForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LicenseGeneratorForm));
            this.lblLicensePath = new System.Windows.Forms.Label();
            this.txtLicensePath = new System.Windows.Forms.TextBox();
            this.btnBrowseForFolder = new System.Windows.Forms.Button();
            this.txtCADFormatLock = new System.Windows.Forms.TextBox();
            this.txtEthernetAddress = new System.Windows.Forms.TextBox();
            this.lblEthernetAddress = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.toolTipCADVersion = new System.Windows.Forms.ToolTip(this.components);
            this.dateTimeLimit = new System.Windows.Forms.DateTimePicker();
            this.chkIncludeDateLimitation = new System.Windows.Forms.CheckBox();
            this.txtLicenseFileName = new System.Windows.Forms.TextBox();
            this.lblLicenseFileName = new System.Windows.Forms.Label();
            this.windowVersionTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.officeVersionTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.chkElevation = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // lblLicensePath
            // 
            this.lblLicensePath.AutoSize = true;
            this.lblLicensePath.Location = new System.Drawing.Point(8, 11);
            this.lblLicensePath.Name = "lblLicensePath";
            this.lblLicensePath.Size = new System.Drawing.Size(127, 13);
            this.lblLicensePath.TabIndex = 0;
            this.lblLicensePath.Text = "Folder path for license file";
            // 
            // txtLicensePath
            // 
            this.txtLicensePath.Location = new System.Drawing.Point(140, 11);
            this.txtLicensePath.Name = "txtLicensePath";
            this.txtLicensePath.Size = new System.Drawing.Size(234, 20);
            this.txtLicensePath.TabIndex = 1;
            // 
            // btnBrowseForFolder
            // 
            this.btnBrowseForFolder.Location = new System.Drawing.Point(385, 11);
            this.btnBrowseForFolder.Name = "btnBrowseForFolder";
            this.btnBrowseForFolder.Size = new System.Drawing.Size(26, 23);
            this.btnBrowseForFolder.TabIndex = 2;
            this.btnBrowseForFolder.Text = "...";
            this.btnBrowseForFolder.UseVisualStyleBackColor = true;
            this.btnBrowseForFolder.Click += new System.EventHandler(this.btnBrowseForFolder_Click);
            // 
            // txtCADFormatLock
            // 
            this.txtCADFormatLock.Location = new System.Drawing.Point(140, 73);
            this.txtCADFormatLock.Name = "txtCADFormatLock";
            this.txtCADFormatLock.Size = new System.Drawing.Size(234, 20);
            this.txtCADFormatLock.TabIndex = 4;
            // 
            // txtEthernetAddress
            // 
            this.txtEthernetAddress.Location = new System.Drawing.Point(140, 104);
            this.txtEthernetAddress.Name = "txtEthernetAddress";
            this.txtEthernetAddress.Size = new System.Drawing.Size(234, 20);
            this.txtEthernetAddress.TabIndex = 5;
            // 
            // lblEthernetAddress
            // 
            this.lblEthernetAddress.AutoSize = true;
            this.lblEthernetAddress.Location = new System.Drawing.Point(8, 107);
            this.lblEthernetAddress.Name = "lblEthernetAddress";
            this.lblEthernetAddress.Size = new System.Drawing.Size(88, 13);
            this.lblEthernetAddress.TabIndex = 6;
            this.lblEthernetAddress.Text = "Ethernet Address";
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(338, 209);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(257, 209);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 7;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // toolTipCADVersion
            // 
            this.toolTipCADVersion.AutomaticDelay = 1;
            this.toolTipCADVersion.AutoPopDelay = 10000;
            this.toolTipCADVersion.InitialDelay = 1;
            this.toolTipCADVersion.ReshowDelay = 1;
            this.toolTipCADVersion.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.toolTipCADVersion.ToolTipTitle = "License Generation";
            // 
            // dateTimeLimit
            // 
            this.dateTimeLimit.Location = new System.Drawing.Point(113, 210);
            this.dateTimeLimit.Name = "dateTimeLimit";
            this.dateTimeLimit.Size = new System.Drawing.Size(138, 20);
            this.dateTimeLimit.TabIndex = 12;
            // 
            // chkIncludeDateLimitation
            // 
            this.chkIncludeDateLimitation.AutoSize = true;
            this.chkIncludeDateLimitation.Checked = true;
            this.chkIncludeDateLimitation.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkIncludeDateLimitation.Location = new System.Drawing.Point(12, 215);
            this.chkIncludeDateLimitation.Name = "chkIncludeDateLimitation";
            this.chkIncludeDateLimitation.Size = new System.Drawing.Size(92, 17);
            this.chkIncludeDateLimitation.TabIndex = 11;
            this.chkIncludeDateLimitation.Text = "Set Date Limit";
            this.chkIncludeDateLimitation.UseVisualStyleBackColor = true;
            this.chkIncludeDateLimitation.CheckedChanged += new System.EventHandler(this.chkIncludeDateLimitation_CheckedChanged);
            // 
            // txtLicenseFileName
            // 
            this.txtLicenseFileName.AllowDrop = true;
            this.txtLicenseFileName.Location = new System.Drawing.Point(140, 42);
            this.txtLicenseFileName.Name = "txtLicenseFileName";
            this.txtLicenseFileName.ReadOnly = false;
            this.txtLicenseFileName.Size = new System.Drawing.Size(234, 20);
            this.txtLicenseFileName.TabIndex = 18;
            this.txtLicenseFileName.DragDrop += new System.Windows.Forms.DragEventHandler(this.txtLicenseFileName_DragDrop);
            this.txtLicenseFileName.DragEnter += new System.Windows.Forms.DragEventHandler(this.txtLicenseFileName_DragEnter);
            // 
            // lblLicenseFileName
            // 
            this.lblLicenseFileName.AutoSize = true;
            this.lblLicenseFileName.Location = new System.Drawing.Point(8, 43);
            this.lblLicenseFileName.Name = "lblLicenseFileName";
            this.lblLicenseFileName.Size = new System.Drawing.Size(89, 13);
            this.lblLicenseFileName.TabIndex = 17;
            this.lblLicenseFileName.Text = "License file name";
            // 
            // windowVersionTextBox
            // 
            this.windowVersionTextBox.Location = new System.Drawing.Point(140, 135);
            this.windowVersionTextBox.Name = "windowVersionTextBox";
            this.windowVersionTextBox.Size = new System.Drawing.Size(234, 20);
            this.windowVersionTextBox.TabIndex = 20;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 139);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(83, 13);
            this.label2.TabIndex = 19;
            this.label2.Text = "Window version";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 169);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 13);
            this.label1.TabIndex = 21;
            this.label1.Text = "Office Version";
            // 
            // officeVersionTextBox
            // 
            this.officeVersionTextBox.AllowDrop = true;
            this.officeVersionTextBox.Location = new System.Drawing.Point(140, 166);
            this.officeVersionTextBox.Name = "officeVersionTextBox";
            this.officeVersionTextBox.Size = new System.Drawing.Size(234, 20);
            this.officeVersionTextBox.TabIndex = 22;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 75);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(76, 13);
            this.label3.TabIndex = 23;
            this.label3.Text = "AutoCAD Year";
            // 
            // chkElevation
            // 
            this.chkElevation.AutoSize = true;
            this.chkElevation.Location = new System.Drawing.Point(11, 192);
            this.chkElevation.Name = "chkElevation";
            this.chkElevation.Size = new System.Drawing.Size(70, 17);
            this.chkElevation.TabIndex = 11;
            this.chkElevation.Text = "Elevation";
            this.chkElevation.UseVisualStyleBackColor = true;
            this.chkElevation.CheckedChanged += new System.EventHandler(this.chkIncludeDateLimitation_CheckedChanged);
            // 
            // LicenseGeneratorForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(432, 237);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.officeVersionTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.windowVersionTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtLicenseFileName);
            this.Controls.Add(this.lblLicenseFileName);
            this.Controls.Add(this.dateTimeLimit);
            this.Controls.Add(this.chkElevation);
            this.Controls.Add(this.chkIncludeDateLimitation);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.lblEthernetAddress);
            this.Controls.Add(this.txtEthernetAddress);
            this.Controls.Add(this.txtCADFormatLock);
            this.Controls.Add(this.btnBrowseForFolder);
            this.Controls.Add(this.txtLicensePath);
            this.Controls.Add(this.lblLicensePath);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LicenseGeneratorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Developed By A & E Enterprises";
            this.Load += new System.EventHandler(this.LicenseGeneratorForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblLicensePath;
        private System.Windows.Forms.TextBox txtLicensePath;
        private System.Windows.Forms.Button btnBrowseForFolder;
        private System.Windows.Forms.TextBox txtCADFormatLock;
        private System.Windows.Forms.TextBox txtEthernetAddress;
        private System.Windows.Forms.Label lblEthernetAddress;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.ToolTip toolTipCADVersion;
        private System.Windows.Forms.DateTimePicker dateTimeLimit;
        private System.Windows.Forms.CheckBox chkIncludeDateLimitation;
        private System.Windows.Forms.TextBox txtLicenseFileName;
        private System.Windows.Forms.Label lblLicenseFileName;
        private System.Windows.Forms.TextBox windowVersionTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox officeVersionTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox chkElevation;
    }
}

