using System;
using System.Collections.Generic;

namespace CommonUtilities
{
    public class LicenseData
    {
        public LicenseData()
        {
            this.CADFormatLock = string.Empty;
            this.EthernetAddress = string.Empty;
            this.CanRunTill = DateTime.Today;

            this.WindowVersion = string.Empty;
            this.OfficeVersion = string.Empty;
            Elevation = false;
        }

        public string LicenseName { get; set; } //Added on 08/05/2023 by SDM
        public string CADFormatLock { get; set; }
        public string EthernetAddress { get; set; }

        public bool IncludeDate { get; set; }

        public DateTime CanRunTill { get; set; }

        public string WindowVersion { get; set; }

        public string OfficeVersion { get; set; }

        public bool RectangularGrating { get; set; }

        public bool CircularGrating { get; set; }

        public bool CreateGroup { get; set; }

        public bool CreateUniformEdgeForCGR { get; set; }

        public bool Elevation { get; set; }
    }
}