using Microsoft.Win32;
using PanelLayout_App;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows.Forms;

namespace PanelLayout_App.Licensing
{
    public class LicenseUtilities
    {
        public static string licenseErrorMsg = "License Error";
        public static string licenseFileName = "PanelLayout_App.lic";
        private static bool m_HasElevation = false;
        public static bool hasElevation { get { return m_HasElevation; } }
        public static string lnceErrorMsg = "Please contact " + "swapnil@aneeindia.com" + " for further assistance.";
        public static int IsValidLicense(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {

#if _LICVALIDATE_DEBUG_MSGSON_
            System.Windows.Forms.MessageBox.Show("Check License file Name");
#endif

                bool flagOfLicenseFile = false;
                var appPath = getAppPath();
                string[] licenseFilePath = System.IO.Directory.GetFiles(appPath, "*.lic");
                foreach (var licenseFileWithPath in licenseFilePath)
                {
                    var lincenseFileName = Path.GetFileNameWithoutExtension(licenseFileWithPath);
                    if (lincenseFileName == licenseFileName)
                    {
                        flagOfLicenseFile = true;
                        break;
                    }
                }
                if (flagOfLicenseFile == false && licenseFilePath.Length != 0)
                {
                    string msg1 = "Invalid license file name. ";
                    msg1 = msg1 + lnceErrorMsg;
                    MessageBox.Show(msg1, licenseErrorMsg, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return -1;
                }
#if _LICVALIDATE_DEBUG_MSGSON_
            System.Windows.Forms.MessageBox.Show("License file name exist");
#endif


#if _LICVALIDATE_DEBUG_MSGSON_
            System.Windows.Forms.MessageBox.Show("Check License file");
#endif
                string msg = "License file does not exist. ";
                msg = msg + lnceErrorMsg;
                MessageBox.Show(msg, licenseErrorMsg, MessageBoxButtons.OK, MessageBoxIcon.Error);

                return -1;
            }


#if _LICVALIDATE_DEBUG_MSGSON_
            System.Windows.Forms.MessageBox.Show("License file exists");
#endif
            LicenseData licData = Utilities.Deserialize<LicenseData>(CommandModule.licFileNameWithPath, true);
            if (!IsValidEthernetAddress(licData))
            {
#if _LICVALIDATE_DEBUG_MSGSON_
                System.Windows.Forms.MessageBox.Show("Invalid ethernet address");
#endif
                string msg = "Invalid ethernet address. ";
                msg = msg + lnceErrorMsg;
                MessageBox.Show(msg, licenseErrorMsg, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }

#if _LICVALIDATE_DEBUG_MSGSON_
            System.Windows.Forms.MessageBox.Show("Ethernet test passed");
#endif

            if (!IsValidWindowVersion(licData))
            {
                //sample windowVersionText = "Windows 10"

#if _LICVALIDATE_DEBUG_MSGSON_
                System.Windows.Forms.MessageBox.Show("Invalid window version address");
#endif
                //string msg = getLicenseErrorMessage(5);
                string msg = "Window version is not compatible. ";
                msg = msg + lnceErrorMsg;

                MessageBox.Show(msg, licenseErrorMsg, MessageBoxButtons.OK, MessageBoxIcon.Error);

                return -1;
            }

            if (!IsValidOfficeVersion(licData))
            {
                //sample officeVersionText = "2010"

#if _LICVALIDATE_DEBUG_MSGSON_
                System.Windows.Forms.MessageBox.Show("Invalid office version");
#endif
                string msg = "Office version is not compatible. ";
                msg = msg + lnceErrorMsg;

                MessageBox.Show(msg, licenseErrorMsg, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }

            if (licData.GUID != "")
            {

#if _LICVALIDATE_DEBUG_MSGSON_
            System.Windows.Forms.MessageBox.Show("Check for GUID started");
#endif
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

                System.Runtime.InteropServices.GuidAttribute[] attributes = (System.Runtime.InteropServices.GuidAttribute[])assembly.GetCustomAttributes(typeof(System.Runtime.InteropServices.GuidAttribute), true);
                if (attributes.Length > 0)
                {
                    System.Runtime.InteropServices.GuidAttribute attribute = (System.Runtime.InteropServices.GuidAttribute)assembly.GetCustomAttributes(typeof(System.Runtime.InteropServices.GuidAttribute), true)[0];
                    string id = attribute.Value;
                    if (licData.GUID.ToLower() != id.ToLower())
                    {
#if _LICVALIDATE_DEBUG_MSGSON_
                        System.Windows.Forms.MessageBox.Show("GUID mismatch");
#endif
                        return -1;
                    }
                }
            }

            string autoCADYear = GetAutoCADYearFromVersion();
            if (autoCADYear != licData.CADFormatLock)
            {
                string msg = "AutoCAD version is not compatible. ";
                msg = msg + LicenseUtilities.lnceErrorMsg;

                System.Windows.Forms.MessageBox.Show(msg, LicenseUtilities.licenseErrorMsg, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    string msg = "License is expired. ";
                    msg = msg + lnceErrorMsg;

                    MessageBox.Show(msg, licenseErrorMsg, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return -1;
                }
            }
            else
            {
#if _LICVALIDATE_DEBUG_MSGSON_
            System.Windows.Forms.MessageBox.Show("Date validation ignored");
#endif
                m_HasElevation = licData.Elevation;
                return 0;
            }
        }

        public static void SetElevation()
        {
            m_HasElevation = true;
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
        public static string GetVersion(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
#if _LICVALIDATE_DEBUG_MSGSON_
                System.Windows.Forms.MessageBox.Show("License file missing");
#endif
                return "";
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
                return "";
            }
            return licData.CADFormatLock.Trim();
        }




        public static bool IsValidWindowVersion(LicenseData licData)
        {
            if (licData == null)
                return false;

            var reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            string productName = (string)reg.GetValue("ProductName");

            return productName.StartsWith(licData.WindowVersion);
        }
        public static bool IsValidOfficeVersion(LicenseData licData)
        {
            if (licData == null)
                return false;

            string sVersion = string.Empty;
            Microsoft.Office.Interop.Excel.Application appVersion = new Microsoft.Office.Interop.Excel.Application();
            appVersion.Visible = false;

            switch (appVersion.Version.ToString())
            {
                case "7.0":
                    sVersion = "95";
                    break;
                case "8.0":
                    sVersion = "97";
                    break;
                case "9.0":
                    sVersion = "2000";
                    break;
                case "10.0":
                    sVersion = "2002";
                    break;
                case "11.0":
                    sVersion = "2003";
                    break;
                case "12.0":
                    sVersion = "2007";
                    break;
                case "14.0":
                    sVersion = "2010";
                    break;
                case "15.0":
                    sVersion = "2013";
                    break;
                case "16.0":
                    sVersion = "2016";
                    break;
            }
            appVersion.Quit();

            if (licData.OfficeVersion == sVersion)
            {
                return true;
            }
            return false;
        }


        public static string getAppPath()
        {
            string appPath = "";
            var path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            string[] pathArray = path.Split('\\');

            for (int i = 1; i < pathArray.Length; i++)
            {
                string name = Convert.ToString((pathArray.GetValue(i)));

                appPath = appPath + name + "\\";
            }
            return appPath;
        }
        public static string GetAutoCADYearFromVersion()
        {
            var _version = Autodesk.AutoCAD.ApplicationServices.Application.Version;
            var acadVersion = _version.Major + "." + _version.Minor;

            string version = "";
            if (acadVersion == "24.2")
                version = "2023";
            else if (acadVersion == "24.1")
                version = "2022";
            else if (acadVersion == "24.0")
                version = "2021";
            else if (acadVersion == "23.1")
                version = "2020";
            else if (acadVersion == "23.0")
                version = "2019";
            else if (acadVersion == "22.0")
                version = "2018";
            else if (acadVersion == "21")
                version = "2017";
            else if (acadVersion == "20.0")
                version = "2015";
            else if (acadVersion == "19.1")
                version = "2014";
            else if (acadVersion == "19.0")
                version = "2013";
            else
                version = "Unknown";

            return version;
        }

    }
}
