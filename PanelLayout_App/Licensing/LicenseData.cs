using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanelLayout_App.Licensing
{
    public class LicenseData
    {
        public LicenseData()
        {
            this.CADFormatLock = string.Empty;
            this.EthernetAddress = string.Empty;
            this.GUID = string.Empty;
            this.CanRunTill = DateTime.Today;

            this.WindowVersion = string.Empty;
            this.OfficeVersion = string.Empty;
            Elevation = false;
        }

        public string CADFormatLock { get; set; }
        public string EthernetAddress { get; set; }

        public string GUID { get; set; }

        public bool IncludeDate { get; set; }

        public DateTime CanRunTill { get; set; }

        public string WindowVersion { get; set; }

        public string OfficeVersion { get; set; }

        public bool Elevation { get; set; }
    }
}
