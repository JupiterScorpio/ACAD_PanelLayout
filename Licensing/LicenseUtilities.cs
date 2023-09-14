using System.Net.NetworkInformation;

namespace CommonUtilities
{
    internal partial class LicenseUtilities
    {
        public static int IsValidLicense(string filePath, string CADFormatWithVersion)
        {
            if (!System.IO.File.Exists(filePath))
            {
#if _LICVALIDATE_DEBUG_MSGSON_
                System.Windows.Forms.MessageBox.Show("License file missing");
#endif
                return -1;
            }

#if _LICVALIDATE_DEBUG_MSGSON_
            System.Windows.Forms.MessageBox.Show("License file exists");
#endif
            LicenseData licData = Utilities.Deserialize<LicenseData>(filePath, true);
            if (!IsValidEthernetAddress(licData))
            {
#if _LICVALIDATE_DEBUG_MSGSON_
                System.Windows.Forms.MessageBox.Show("Invalid ethernet address");
#endif
                return -1;
            }

#if _LICVALIDATE_DEBUG_MSGSON_
            System.Windows.Forms.MessageBox.Show("Ethernet test passed");
#endif


#if _LICVALIDATE_DEBUG_MSGSON_
            System.Windows.Forms.MessageBox.Show("GUID test passed");
#endif
            if (!licData.CADFormatLock.Contains(CADFormatWithVersion))
            {
#if _LICVALIDATE_DEBUG_MSGSON_
                System.Windows.Forms.MessageBox.Show("Wrong CAD format/version ");
#endif
                return -1;
            }

#if _LICVALIDATE_DEBUG_MSGSON_
            System.Windows.Forms.MessageBox.Show("CAD version lock test passed");
#endif
            if (licData.IncludeDate)
            {
                int dateDiff = (licData.CanRunTill - System.DateTime.Today).Days;
                if (dateDiff >= 0)
                {
#if _LICVALIDATE_DEBUG_MSGSON_
                    System.Windows.Forms.MessageBox.Show("Date expiry/validity test passed");
#endif
                    return dateDiff + 1;
                }
                else
                {
#if _LICVALIDATE_DEBUG_MSGSON_
                    System.Windows.Forms.MessageBox.Show("License validity expired");
#endif
                    return -1;
                }
            }
            else
            {
#if _LICVALIDATE_DEBUG_MSGSON_
            System.Windows.Forms.MessageBox.Show("Date validation ignored");
#endif
                return 0;
            }
        }
        public static bool IsValidEthernetAddress(LicenseData licData)
        {
            if (licData == null)
                return false;

            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            if (nics == null || nics.Length < 1)
                return false;

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

                if (licData.EthernetAddress.ToLower() == addressString.ToLower())
                    return true;
            }
            return false;
        }

    }
}