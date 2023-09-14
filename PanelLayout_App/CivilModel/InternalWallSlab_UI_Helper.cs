using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PanelLayout_App.CivilModel
{
    public class InternalWallSlab_UI_Helper
    {
        public static int internalWallSlabLayerColumnIndex = 0;
        public static int internalWallSlabBottomColumnIndex = 1;
        public static string internalWallSlabStrtText = "AEE_IC_Wall";
        public static int internalWallSlabNameCount = 1;
        public static int internalWallSlabLayerColorIndex = 5;

        public static List<string> listOfInternalWallSlabLayerName = new List<string>();
        public static List<double> listOfInternalWallSlabBottom = new List<double>();

        public void getDataFromInternalWallSlabGridView(DataGridView dataGridViewRow)
        {
            listOfInternalWallSlabLayerName.Clear();
            listOfInternalWallSlabBottom.Clear();

            int columnCount = dataGridViewRow.ColumnCount;
            int rowCount = dataGridViewRow.RowCount;
            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                string sunkanSlabLayerName = null;
                string sunkanSlabThick = null;

                for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
                {
                    var obj = dataGridViewRow.Rows[rowIndex].Cells[columnIndex].Value;
                    string data = Convert.ToString(obj);
                    if (string.IsNullOrEmpty(data) || string.IsNullOrWhiteSpace(data))
                    {

                    }
                    else
                    {
                        if (internalWallSlabLayerColumnIndex == (columnIndex))
                        {
                            sunkanSlabLayerName = data;
                        }
                        else if (internalWallSlabBottomColumnIndex == (columnIndex))
                        {
                            sunkanSlabThick = data;
                        }
                    }
                }
                if (string.IsNullOrEmpty(sunkanSlabLayerName) || string.IsNullOrWhiteSpace(sunkanSlabLayerName) || string.IsNullOrEmpty(sunkanSlabThick) || string.IsNullOrWhiteSpace(sunkanSlabThick))
                {

                }
                else
                {
                    listOfInternalWallSlabLayerName.Add(sunkanSlabLayerName);
                    listOfInternalWallSlabBottom.Add(Convert.ToDouble(sunkanSlabThick));
                }
            }
        }

        public static String ValidDateSlabBottom(DataGridView dataGridViewRow, double floorHeight)
        {
            if (floorHeight <= 0)
            {
                return "Please enter the valid floor Height";
            }
            bool hasEntries = false;
            int columnCount = dataGridViewRow.ColumnCount;
            int rowCount = dataGridViewRow.RowCount;
            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                string sunkanSlabLayerName = null;
                string sunkanSlabThick = null;

                for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
                {
                    var obj = dataGridViewRow.Rows[rowIndex].Cells[columnIndex].Value;
                    string data = Convert.ToString(obj);
                    if (string.IsNullOrEmpty(data) || string.IsNullOrWhiteSpace(data))
                    {

                    }
                    else
                    {
                        if (internalWallSlabLayerColumnIndex == (columnIndex))
                        {
                            sunkanSlabLayerName = data;
                        }
                        else if (internalWallSlabBottomColumnIndex == (columnIndex))
                        {
                            sunkanSlabThick = data;
                            double dSlabBottom = 0;
                            if (!double.TryParse(sunkanSlabThick, out dSlabBottom))
                            {
                                return "Please enter valid Slab Bottom in Slab Thickness Tab";
                            }
                            if (dSlabBottom >= floorHeight)
                            {
                                return "Please enter Slab Bottom less than floor Height in Slab Thickness Tab";
                            }
                            hasEntries = true;
                        }
                    }
                }
            }
            if (hasEntries)
            {
                return "";
            }
            return "Please add entries in Slab Thickness Tab";
        }

        public void previousDataInInternalWallSlabGridView(DataGridView dataGridViewRow)
        {
            dataGridViewRow.Rows.Clear();
            if (listOfInternalWallSlabLayerName.Count != 0)
            {
                internalWallSlabNameCount = Lift_UI_Helper.getLayerIndex(listOfInternalWallSlabLayerName);
                for (int index = 0; index < listOfInternalWallSlabLayerName.Count; index++)
                {
                    string prvsSunkanSlabLayer = listOfInternalWallSlabLayerName[index];
                    string prvSunkanSlabThick = Convert.ToString(listOfInternalWallSlabBottom[index]);
                    dataGridViewRow.Rows.Add(prvsSunkanSlabLayer, prvSunkanSlabThick);
                }
            }
            else
            {
                internalWallSlabNameCount = 1;
            }
        }

        public static string getWallLayer(string[] lyrNames)
        {
            foreach (var item in lyrNames)
            {
                if (item.ToUpper().StartsWith(internalWallSlabStrtText.ToUpper()))
                {
                    return item;
                }
            }
            return "";
        }

        public static void updateSlabThickness(DataGridView dataGridViewRow, TextBox floorHeight)
        {
            try
            {
                double fh = 0;
                if (!double.TryParse(floorHeight.Text, out fh))
                {
                    return;
                }
                if (dataGridViewRow.Rows.Count != 0)
                {
                    for (int index = 0; index < dataGridViewRow.Rows.Count; index++)
                    {
                        var row = dataGridViewRow.Rows[index];
                        if (row.Cells.Count < 2)
                        {
                            continue;
                        }
                        row.Cells[2].Value = "";
                        if (row.Cells[1].Value == null)
                        {
                            continue;
                        }

                        String str = row.Cells[1].Value.ToString();
                        int val = 0;
                        if (int.TryParse(str, out val))
                        {
                            row.Cells[2].Value = ((int)(fh - val)).ToString();
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        public static void InitOnEmpty()
        {
            if (!InternalWallSlab_UI_Helper.listOfInternalWallSlabLayerName.Any())
            {
                InternalWallSlab_UI_Helper.setDefaultValueInInternalWallSlabGridView(null);
            }
        }

        public static void setDefaultValueInInternalWallSlabGridView(DataGridView dataGridViewRow)
        {
            //if (listOfSunkanSlabLayerName.Count == 0)
            {
                dataGridViewRow.Rows.Clear();
                listOfInternalWallSlabLayerName.Clear();
                listOfInternalWallSlabBottom.Clear();
                internalWallSlabNameCount = 1;

                if (PanelLayoutInputHelper.listOfAllSelectedEntity.Count != 0)
                {
                    for (int i = 0; i < PanelLayoutInputHelper.listOfAllSelectedEntity.Count; i++)
                    {
                        string data = PanelLayoutInputHelper.listOfAllSelectedEntity[i];
                        if (data.Contains("Slab"))
                        {

                            data = PanelLayoutInputHelper.listOfAllSelectedEntity[i + 2];
                            string[] arr = data.Split(':');
                            string beamData = arr[1];
                            string[] splitData = beamData.Split(',');
                            for (int j = 0; j < splitData.Count(); j++)
                            {
                                double slabBottom = Convert.ToDouble(splitData[j]);

                                string sunkanSlabLayerName = internalWallSlabStrtText + (Convert.ToString(internalWallSlabNameCount));

                                dataGridViewRow.Rows.Add(sunkanSlabLayerName, slabBottom);
                                listOfInternalWallSlabLayerName.Add(sunkanSlabLayerName);
                                listOfInternalWallSlabBottom.Add(Convert.ToDouble(slabBottom));
                                internalWallSlabNameCount++;
                            }

                            break;
                        }
                    }
                }
                else
                {
                    string sunkanSlabLayerName1 = internalWallSlabStrtText + (Convert.ToString(internalWallSlabNameCount));
                    internalWallSlabNameCount++;

                    double sunkanSlabThick1 = 3000;

                    if (dataGridViewRow != null)
                        dataGridViewRow.Rows.Add(sunkanSlabLayerName1, sunkanSlabThick1);

                    listOfInternalWallSlabLayerName.Add(sunkanSlabLayerName1);
                    listOfInternalWallSlabBottom.Add(Convert.ToDouble(sunkanSlabThick1));


                    string sunkanSlabLayerName2 = internalWallSlabStrtText + (Convert.ToString(internalWallSlabNameCount));
                    internalWallSlabNameCount++;

                    double sunkanSlabThick2 = 2950;

                    if (dataGridViewRow != null)
                        dataGridViewRow.Rows.Add(sunkanSlabLayerName2, sunkanSlabThick2);

                    listOfInternalWallSlabLayerName.Add(sunkanSlabLayerName2);
                    listOfInternalWallSlabBottom.Add(Convert.ToDouble(sunkanSlabThick2));
                }

                //if (dataGridViewRow != null)
                //    dataGridViewRow.Rows.Add(sunkanSlabLayerName1, sunkanSlabThick1);

                //listOfInternalWallSlabLayerName.Add(sunkanSlabLayerName1);
                //listOfInternalWallSlabBottom.Add(Convert.ToDouble(sunkanSlabThick1));


                //string sunkanSlabLayerName2 = internalWallSlabStrtText + (Convert.ToString(internalWallSlabNameCount));
                //internalWallSlabNameCount++;

                ////double sunkanSlabThick2 = 2950;

                //if (dataGridViewRow != null)
                //    dataGridViewRow.Rows.Add(sunkanSlabLayerName2, sunkanSlabThick2);

                //listOfInternalWallSlabLayerName.Add(sunkanSlabLayerName2);
                //listOfInternalWallSlabBottom.Add(Convert.ToDouble(sunkanSlabThick2));

                //Commented on 20/06/2023 by PRT
            }
        }
        public static bool checkInternalWallSlabLayerIsExist(string inputWindowLayer)
        {
            if (listOfInternalWallSlabLayerName.Count != 0)
            {
                int index = listOfInternalWallSlabLayerName.IndexOf(inputWindowLayer);
                if (index != -1)
                {
                    return true;
                }
            }
            return false;
        }
        public static bool checkInternalWallSlabLayerIsExist(ObjectId sukanSlabId)
        {
            bool flag = false;
            if (listOfInternalWallSlabLayerName.Count != 0)
            {
                if (sukanSlabId.IsValid == true)
                {
                    var sukanSlab = AEE_Utility.GetEntityForRead(sukanSlabId);
                    flag = checkInternalWallSlabLayerIsExist(sukanSlab.Layer);
                }
            }
            return flag;
        }

        internal static bool IsInternalWall(string wall)
        {
            if (string.IsNullOrEmpty(wall))
            {
                return false;
            }
            return wall.ToUpper().StartsWith(internalWallSlabStrtText.ToUpper());
        }

        internal static string defaulLayerName
        {
            get
            {
                return internalWallSlabStrtText + "1";
            }
        }

        internal static double getSlabBottom(string layerName)
        {
            double sb = (listOfInternalWallSlabBottom == null || listOfInternalWallSlabLayerName.Count == 0) ? 0 :
                listOfInternalWallSlabBottom[0];
            if (sb == 0)
            {
                return 0;
            }
            if (!layerName.ToUpper().StartsWith(internalWallSlabStrtText.ToUpper()))
            {
                return sb;
            }
            for (var n = 0; n < listOfInternalWallSlabLayerName.Count; ++n)
            {
                if (layerName.ToUpper().Trim().CompareTo(listOfInternalWallSlabLayerName[n].ToUpper().Trim()) == 0)
                {
                    if (n < listOfInternalWallSlabBottom.Count)
                    {
                        if (listOfInternalWallSlabBottom[n] > 0)
                        {
                            return listOfInternalWallSlabBottom[n];
                        }
                    }
                }
            }
            return sb;
        }

        internal static double getThickness(string layerName)
        {
            if (layerName == null)
            {
                return -1;
            }
            if (!layerName.ToUpper().StartsWith(internalWallSlabStrtText.ToUpper()))
            {
                return -1;
            }
            for (var n = 0; n < listOfInternalWallSlabLayerName.Count; ++n)
            {
                if (layerName.ToUpper().Trim().CompareTo(listOfInternalWallSlabLayerName[n].ToUpper().Trim()) == 0)
                {
                    if (n < listOfInternalWallSlabBottom.Count)
                    {
                        if (listOfInternalWallSlabBottom[n] > 0)
                        {
                            return PanelLayout_UI.floorHeight - listOfInternalWallSlabBottom[n];
                        }
                    }
                }
            }
            return PanelLayout_UI.slabThickness;
        }


    }
}
