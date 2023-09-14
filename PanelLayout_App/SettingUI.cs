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
    public partial class SettingUI : Form
    {
        public SettingUI()
        {
            InitializeComponent();
        }

        private void PanelRC_CheckBox_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void horzPanel_ForDoorWindow_CheckBox_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
