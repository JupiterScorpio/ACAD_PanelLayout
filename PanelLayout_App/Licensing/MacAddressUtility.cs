using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace PanelLayout_App.Licensing
{
    public class MacAddressUtility
    {
        public static List<string> GetMACAddresses()
        {
            var mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            var moc = mc.GetInstances();
            var lstMacAddresses = new List<string>();
            foreach (ManagementObject mo in moc)
            {
                if (mo == null || mo["MacAddress"] == null)
                {
                    continue;
                }
                var addr = mo["MacAddress"].ToString();
                if (!string.IsNullOrEmpty(addr))
                {
                    lstMacAddresses.Add(addr);
                }
                mo.Dispose();
            }
            return lstMacAddresses;
        }
        public static bool IsLicenseCheckNeeded()
        {
            var skipAddress = new List<string>() {
            /* Swapnil Sir PC MacID */  "34:E6:AD:A2:2D:09", "C4:65:16:E9:6F:CB", "10:62:E5:C9:19:A5", "2C:F0:5D:E7:C4:D2","84:A9:3E:36:09:9F","18:31:BF:6F:B6:3E","00:BE:43:7D:F4:5A"  /* Swapnil Sir PC MacID */,

                  /*Muzammil_PC MacID */  "18:31:BF:6F:B6:3E", "04:92:26:CD:FD:38",  /* Muzammil_PC MacID   
               
              /* Dell_PC MacID */   "8C:EC:4B:62:5C:03", "2C:F0:5D:E7:C4:D7",   /* Dell_PC MacID */ 
              /*Liwei PC MacID*/     "64:00:6A:4A:13:B5", "E8:F8:20:52:41:53", "EC:BE:20:52:41:53", "EE:F6:20:52:41:53",   /*Liwei PC MacID*/
                     
/* Tejaswini Mam PC MacID */  "88:B1:11:23:B2:A7", "88:B1:11:23:B2:A8", "8A:B1:11:23:B2:A7", "F4:30:B9:AF:38:B9", "D8:BB:C1:D2:64:3A",   /* Tejaswini Mam PC MacID */
            };

            var addresses = GetMACAddresses();
            foreach (var str in skipAddress)
            {
                if (addresses.Contains(str))
                {
                    DXFExport = true;
                    SATExport = true;
                    CreateTab = true;
                    LicenseUtilities.SetElevation();
                    return true;
                }
            }
            return false;
        }
        public static bool SATExport { get; internal set; }
        public static bool DXFExport { get; internal set; }
        public static bool CreateTab { get; internal set; }
    }
}
