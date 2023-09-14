using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace LicenseDebugger
{
    public partial class LicenseDebuggerForm : Form
    {
        public string LicensePath
        {
            get
            {
                if (txtLicenseFileName.Text.Trim() == "")
                    txtLicenseFileName.Text = "GR_App.lic";
                return Path.Combine(txtLicensePath.Text.Trim(), txtLicenseFileName.Text.Trim());
            }
        }

        public LicenseDebuggerForm()
        {
            InitializeComponent();

            lstEthernetAddresses.Items.Clear();
            List<string> ethernetAddresses = CommonUtilities.LicenseUtilities.GetEthernetAddresses();

            if (ethernetAddresses.Count >= 0)
            {
                foreach (string add in ethernetAddresses)
                {
                    lstEthernetAddresses.Items.Add(add);
                }
            }
        }

        private void txtLicenseFileName_DragEnter(object sender, DragEventArgs e)
        {
            if (DropFile(e.Data) != null)
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void txtLicenseFileName_DragDrop(object sender, DragEventArgs e)
        {
            string fileName = DropFile(e.Data);
            txtLicenseFileName.Text = Path.GetFileName(fileName);
            txtLicensePath.Text = Path.GetDirectoryName(fileName);

            GetLicenseData(fileName);
        }

        private void btnGetLicenseData_Click(object sender, EventArgs e)
        {
            string fileName = txtLicensePath.Text;
            if (!fileName.EndsWith("\\"))
                fileName += "\\";
            fileName += txtLicenseFileName.Text;

            if (!File.Exists(fileName))
            {
                MessageBox.Show("License file does not exist:\n\"" + fileName + "\"\n" + "Please ensure that the provided file name and path are correct.", "License debugger", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            GetLicenseData(fileName);
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

        private void btnBrowseForFolder_Click(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.Description = "Select the folder for License file debugging";
            dialog.ShowNewFolderButton = true;
            dialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var res = dialog.ShowDialog();
            if (res != DialogResult.OK)
            {
                return;
            }
            txtLicensePath.Text = dialog.SelectedPath;
        }

        private void GetLicenseData(string fileName)
        {
            CommonUtilities.LicenseData licData = CommonUtilities.LicenseUtilities.GetLicenseData(fileName);
            if (licData == null)
            {
                MessageBox.Show("License file is in old format and could not be pharsed by this application.", "License debugger", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }
            txtEthernetAddress.Text = licData.EthernetAddress;

            if (licData.GUID != "")
            {
                txtGUID.Text = licData.GUID;
            }
            else
            {
                txtGUID.Text = "No restriction";
            }

            if (licData.CADFormatLock != "")
            {
                txtCADFormatLock.Text = licData.CADFormatLock;
            }
            else
            {
                txtCADFormatLock.Text = "No restriction";
            }

            if (licData.IncludeDate)
            {
                txtDateLimit.Text = licData.CanRunTill.ToString("MMM dd, yyyy");
            }
            else
            {
                txtDateLimit.Text = "No limit";
            }
        }

    }
}
