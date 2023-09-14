using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Linq;

namespace LicenseGenerator
{
    public partial class LicenseGeneratorForm : Form
    {
        string licenseFileName = "PanelLayout_App.lic";
        public string LicensePath
        {
            get
            {
                if (txtLicenseFileName.Text.Trim() == "")
                    txtLicenseFileName.Text = licenseFileName;
                return Path.Combine(txtLicensePath.Text.Trim(), txtLicenseFileName.Text.Trim());
            }
        }

        public LicenseGeneratorForm()
        {
            InitializeComponent();
        }

        private void btnBrowseForFolder_Click(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.Description = "Select the folder for License file generation";
            dialog.ShowNewFolderButton = true;
            dialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var res = dialog.ShowDialog();
            if(res != DialogResult.OK)
            {
                return;
            }
            txtLicensePath.Text = dialog.SelectedPath;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if(HasValidData())
            {
                CommonUtilities.LicenseData licenseData = new CommonUtilities.LicenseData();
                licenseData.CADFormatLock = txtCADFormatLock.Text.Trim();
                licenseData.EthernetAddress = txtEthernetAddress.Text.Trim();
                licenseData.IncludeDate = chkIncludeDateLimitation.Checked;
                licenseData.LicenseName = txtLicenseFileName.Text.Trim();//Added on 08/05/2023 by SDM

                if (licenseData.IncludeDate)
                    licenseData.CanRunTill = dateTimeLimit.Value.Date;

                licenseData.WindowVersion = windowVersionTextBox.Text.Trim();            
                licenseData.OfficeVersion = officeVersionTextBox.Text.Trim();
                licenseData.Elevation = chkElevation.Checked;
               
                CommonUtilities.LicenseUtilities.WriteLicense(licenseData, LicensePath);
                DialogResult = DialogResult.OK;
                this.Close();
                return;
            }
        }

        private bool HasValidData()
        {
            if(string.IsNullOrEmpty(txtLicensePath.Text.Trim()))
            {
                DisplayError(txtLicensePath, "Please enter the valid folder name.");
                return false;
            }
            System.IO.Directory.CreateDirectory(txtLicensePath.Text);
            if(!System.IO.Directory.Exists(txtLicensePath.Text.Trim()))
            {
                DisplayError(txtLicensePath, "Please enter the valid folder name.");
                return false;
            }      
            if (string.IsNullOrEmpty(txtEthernetAddress.Text.Trim()))
            {
                DisplayError(txtEthernetAddress, "Please enter the valid mac address.");
                return false;
            }

            if (string.IsNullOrEmpty(txtCADFormatLock.Text.Trim()))
            {
                DisplayError(txtCADFormatLock, "Please enter AutoCAD year");
                return false;
            }

            if (string.IsNullOrEmpty(windowVersionTextBox.Text.Trim()))
            {
                DisplayError(windowVersionTextBox, "Please enter window version.");
                return false;
            }
            if (string.IsNullOrEmpty(officeVersionTextBox.Text.Trim()))
            {
                DisplayError(officeVersionTextBox, "Please enter office version.");
                return false;
            }
            return true;
        }

        private void DisplayError(TextBox txtBox, string msg)
        {
            MessageBox.Show(msg, "License Generation", MessageBoxButtons.OK, MessageBoxIcon.Error);
            txtBox.Focus();
        }

        private void LicenseGeneratorForm_Load(object sender, EventArgs e)
        {
            //toolTipCADVersion.SetToolTip(this.txtLicenseFileName, "Enter License file name is " + licenseFileName);
            toolTipCADVersion.SetToolTip(this.txtCADFormatLock, "Enter AutoCAD year. Ex: 2017");
            toolTipCADVersion.SetToolTip(this.txtEthernetAddress, "Enter Physical Address.  Ex:- 18-31-BF-6F-B6-3E");

            toolTipCADVersion.SetToolTip(this.windowVersionTextBox, "Enter Window version number.  Ex:- Windows 10");
            toolTipCADVersion.SetToolTip(this.officeVersionTextBox, "Enter Office version number.  Ex:- 2010");

            txtLicenseFileName.Text = licenseFileName;
        }

        private void chkIncludeDateLimitation_CheckedChanged(object sender, EventArgs e)
        {
            if (chkIncludeDateLimitation.Checked)
                dateTimeLimit.Enabled = true;
            else
                dateTimeLimit.Enabled = false;
        }
  

        private void txtLicenseFileName_DragDrop(object sender, DragEventArgs e)
        {
            string fileName = DropFile(e.Data);
            txtLicensePath.Text = Path.GetDirectoryName(fileName);

            CommonUtilities.LicenseData licData = CommonUtilities.LicenseUtilities.GetLicenseData(fileName);
            txtEthernetAddress.Text = licData.EthernetAddress;
            if(licData.LicenseName != null && licData.LicenseName!="")
            txtLicenseFileName.Text = licData.LicenseName;
            else
            {
                var name = fileName.Split('\\').LastOrDefault();
                txtLicenseFileName.Text = name;
            }
            if (licData.CADFormatLock != "")
            {
                txtCADFormatLock.Text = licData.CADFormatLock;
            }        

            if (licData.IncludeDate)
            {
                chkIncludeDateLimitation.Checked = true;

                dateTimeLimit.Value = licData.CanRunTill;
            }
            else
            {
                chkIncludeDateLimitation.Checked = false;
            }               
               
            windowVersionTextBox.Text = licData.WindowVersion;
            officeVersionTextBox.Text = licData.OfficeVersion;
            chkElevation.Checked = licData.Elevation;
        }

        private void txtLicenseFileName_DragEnter(object sender, DragEventArgs e)
        {
            if (DropFile(e.Data) != null)
            {
               e.Effect = DragDropEffects.Copy;
            }
        }

        private string DropFile(IDataObject data)
        {
            if (data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])data.GetData(DataFormats.FileDrop);
                if (files.Length != 1)
                    return null;

                if (Path.GetExtension(files[0]).ToLower() != ".lic")
                    return null;

                return files[0];
            }

            return null;
        }
    }
}
