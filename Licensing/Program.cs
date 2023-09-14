using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CompactorStorage
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
#if !DEBUG__
            string appVersion = AutoCADApplication.Version;
            if (CommonUtilities.LicenseUtilities.IsValidLicense("CompactStorageLicense.lic", appVersion) < 0)
            {
                MessageBox.Show("Invalid license file!!!", "License Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
#endif
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainInputForm());
            //if (AutoCADApplication.NeedClose)
            //    AutoCADApplication.Application.Quit();
        }
    }
}
