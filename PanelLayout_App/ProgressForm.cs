using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PanelLayout_App
{
    public partial class ProgressForm : Form, IProgressUpdate
    {
        public ProgressForm()
        {
            InitializeComponent();
        }
        public void disposeDialogBox()
        {
            this.Hide();
            this.Close();
        }
        private void ProgressForm_Load(object sender, EventArgs e)
        {
            this.Text = CommonModule.headerText_messageBox;

            ReportProgress(1, "");
            Application.DoEvents();
        }
        public void ReportProgress(int nPercentage, string msg)
        {
            try
            {
                if (progressBar1.Value > (progressBar1.Maximum - 2))
                {
                    return;
                }

                if (nPercentage == 100)
                {
                    var diff = nPercentage - progressBar1.Value;
                    progressBar1.Value += diff;
                    lblProgress.Text = msg;
                    Application.DoEvents(); //keep form active in every loop
                    return;
                }

                progressBar1.Value += nPercentage;
                lblProgress.Text = msg;
                Application.DoEvents(); //keep form active in every loop
            }
            catch (Exception ex)
            {

            }
        }
    }

    public interface IProgressUpdate
    {
        void ReportProgress(int nPercentage, string msg);
    }
}
