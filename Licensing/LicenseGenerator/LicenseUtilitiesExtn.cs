using System.Net.NetworkInformation;
using System.Collections.Generic;

namespace CommonUtilities
{
    internal partial class LicenseUtilities
    {
        public static void WriteLicense(LicenseData data, string filePath)
        {
            Utilities.Serialize<LicenseData>(data, filePath, true);
        }

        public static LicenseData GetLicenseData(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
#if _LICVALIDATE_DEBUG_MSGSON_
                System.Windows.Forms.MessageBox.Show("License file could not be located");
#endif
                return null;
            }

#if _LICVALIDATE_DEBUG_MSGSON_
            System.Windows.Forms.MessageBox.Show("License file located successfully");
#endif
            LicenseData licData = Utilities.Deserialize<LicenseData>(filePath, true);
            return licData;
        }

        public static List<string> GetEthernetAddresses()
        {
            List<string> retStringList = new List<string>();
            retStringList.Clear();

            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            if (nics == null || nics.Length < 1)
                return retStringList;

            foreach (NetworkInterface adapter in nics)
            {
                if (adapter.NetworkInterfaceType != NetworkInterfaceType.Ethernet)
                    continue;

                PhysicalAddress address = adapter.GetPhysicalAddress();
                byte[] bytes = address.GetAddressBytes();
                string addressString = "";
                for (int i = 0; i < bytes.Length; i++)
                {
                    // Formats the physical address in hexadecimal.
                    addressString += bytes[i].ToString("X2");
                    // Insert a hyphen after each byte, unless we are at the end of the address.
                    if (i != bytes.Length - 1)
                    {
                        addressString += "-";
                    }
                }

                retStringList.Add(addressString);
            }
            return retStringList;
        }
    }
}