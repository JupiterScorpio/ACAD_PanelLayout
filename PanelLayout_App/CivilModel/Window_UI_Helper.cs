using Autodesk.AutoCAD.DatabaseServices;
using PanelLayout_App.WallModule;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PanelLayout_App.CivilModel
{
    public class Window_UI_Helper
    {
        public static string windowStrtText = "AEE_Window";
        public static string chajjaStrtText = "AEE_Chajja";
        public static int windowLayerColorIndex = 4;

        public static int windowLayerColumnIndex = 0;
        public static int sillLevelColumnIndex = 1;
        public static int lintelLevelColumnIndex = 2;
        public static int windowLayerCount = 1;
        public static int chajjaLayerCount = 1;

        public static int chajjaLayerColumnIndex = 0;
        public static int nearThickColumnIndex = 1;
        public static int farThickColumnIndex = 2;

        public static List<string> listOfWindowLayerName = new List<string>();
        public static List<double> listOfWindowSillLevel = new List<double>();
        public static List<double> listOfWindowLintelLevel = new List<double>();

        public static List<string> listOfChajjaLayerName = new List<string>();
        public static List<double> listOfNearSideThickness = new List<double>();
        public static List<double> listOfFarSideThickness = new List<double>();//Added on 21/06/2023 by PRT

        PanelLayoutInputHelper panelLayoutInputHlp = new PanelLayoutInputHelper();
        public void getDataFromWindowGridView(DataGridView dataGridViewRow)
        {
            listOfWindowLayerName.Clear();
            listOfWindowSillLevel.Clear();
            listOfWindowLintelLevel.Clear();

            int columnCount = dataGridViewRow.ColumnCount;
            int rowCount = dataGridViewRow.RowCount;
            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                string windowLayerName = null;
                string sellLevel = null;
                string lintelLevel = null;
                for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
                {
                    var obj = dataGridViewRow.Rows[rowIndex].Cells[columnIndex].Value;
                    string data = Convert.ToString(obj);
                    if (string.IsNullOrEmpty(data) || string.IsNullOrWhiteSpace(data))
                    {

                    }
                    else
                    {
                        if (windowLayerColumnIndex == (columnIndex))
                        {
                            windowLayerName = data;
                        }
                        else if (sillLevelColumnIndex == (columnIndex))
                        {
                            sellLevel = data;
                        }
                        else if (lintelLevelColumnIndex == (columnIndex))
                        {
                            lintelLevel = data;
                        }
                    }
                }
                if (string.IsNullOrEmpty(windowLayerName) || string.IsNullOrWhiteSpace(windowLayerName) || string.IsNullOrEmpty(sellLevel) || string.IsNullOrWhiteSpace(sellLevel) || string.IsNullOrEmpty(lintelLevel) || string.IsNullOrWhiteSpace(lintelLevel))
                {

                }
                else
                {
                    listOfWindowLayerName.Add(windowLayerName);
                    listOfWindowSillLevel.Add(Convert.ToDouble(sellLevel));
                    listOfWindowLintelLevel.Add(Convert.ToDouble(lintelLevel));
                }
            }
        }
        public void getDataFromchajjaGridView(DataGridView dataGridViewRow)
        {
            listOfChajjaLayerName.Clear();
            listOfNearSideThickness.Clear();
            listOfFarSideThickness.Clear();

            int columnCount = dataGridViewRow.ColumnCount;
            int rowCount = dataGridViewRow.RowCount;
            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                string ChajjaLayerName = null;
                string nearThick = null;
                string farThick = null;
                for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
                {
                    var obj = dataGridViewRow.Rows[rowIndex].Cells[columnIndex].Value;
                    string data = Convert.ToString(obj);
                    if (string.IsNullOrEmpty(data) || string.IsNullOrWhiteSpace(data))
                    {

                    }
                    else
                    {
                        if (chajjaLayerColumnIndex == (columnIndex))
                        {
                            ChajjaLayerName = data;
                        }
                        else if (nearThickColumnIndex == (columnIndex))
                        {
                            nearThick = data;
                        }
                        else if (farThickColumnIndex == (columnIndex))
                        {
                            farThick = data;
                        }
                    }
                }
                if (string.IsNullOrEmpty(ChajjaLayerName) || string.IsNullOrWhiteSpace(ChajjaLayerName) || string.IsNullOrEmpty(nearThick) || string.IsNullOrWhiteSpace(nearThick) || string.IsNullOrEmpty(farThick) || string.IsNullOrWhiteSpace(farThick))
                {

                }
                else
                {
                    listOfChajjaLayerName.Add(ChajjaLayerName);
                    listOfNearSideThickness.Add(Convert.ToDouble(nearThick));
                    listOfFarSideThickness.Add(Convert.ToDouble(farThick));
                }
            }
        }
        public void previousDataInWindowGridView(DataGridView dataGridViewRow)
        {
            dataGridViewRow.Rows.Clear();
            if (listOfWindowLayerName.Count != 0)
            {
                windowLayerCount = Lift_UI_Helper.getLayerIndex(listOfWindowLayerName);
                for (int index = 0; index < listOfWindowLayerName.Count; index++)
                {
                    string prvsWindowLayerName = listOfWindowLayerName[index];
                    string prvsWindowSillLevel = Convert.ToString(listOfWindowSillLevel[index]);
                    string prvsWindowLintelLevel = Convert.ToString(listOfWindowLintelLevel[index]);
                    dataGridViewRow.Rows.Add(prvsWindowLayerName, prvsWindowSillLevel, prvsWindowLintelLevel);
                }
            }
            else
            {
                windowLayerCount = 1;
            }
        }
        public void previousDataInChajjaGridView(DataGridView dataGridViewRow)
        {
            dataGridViewRow.Rows.Clear();
            if (listOfChajjaLayerName.Count != 0)
            {
                chajjaLayerCount = Lift_UI_Helper.getLayerIndex(listOfChajjaLayerName);
                for (int index = 0; index < listOfChajjaLayerName.Count; index++)
                {
                    string prvsChajjaLayerName = listOfChajjaLayerName[index];
                    string prvsChajjaNearlLevel = Convert.ToString(listOfNearSideThickness[index]);
                    string prvsChajjaFarLevel = Convert.ToString(listOfFarSideThickness[index]);
                    dataGridViewRow.Rows.Add(prvsChajjaLayerName, prvsChajjaNearlLevel, prvsChajjaFarLevel);
                }
            }
            else
            {
                chajjaLayerCount = 1;
            }
        }


        public void setDefaultValueInWindowGridView(DataGridView dataGridViewRow)
        {
            dataGridViewRow.Rows.Clear();

            listOfWindowLayerName.Clear();
            listOfWindowSillLevel.Clear();
            listOfWindowLintelLevel.Clear();
            windowLayerCount = 1;



            if (PanelLayoutInputHelper.listOfAllSelectedEntity.Count != 0)
            {
                double windowSillLevel = 0;
                double windowLintelLevel = 0;

                for (int i = 0; i < PanelLayoutInputHelper.listOfAllSelectedEntity.Count; i++)
                {
                    string data = PanelLayoutInputHelper.listOfAllSelectedEntity[i];
                    if (data.Contains("Win"))
                    {
                        data = PanelLayoutInputHelper.listOfAllSelectedEntity[i + 1];
                        string[] arr = data.Split(':');
                        string winData = arr[1];
                        string[] splitData = winData.Split(',');

                        data = PanelLayoutInputHelper.listOfAllSelectedEntity[i + 2];
                        arr = data.Split(':');
                        winData = arr[1];
                        string[] splitData2 = winData.Split(',');
                        int count = splitData.Count();
                        if (splitData.Count() > splitData2.Count())
                        {

                            count = count - 1;
                        }

                        for (int j = 0; j < count; j++)
                        {

                            windowSillLevel = Convert.ToDouble(splitData[j]);

                            for (int k = 0; k < splitData2.Count(); k++)
                            {

                                windowLintelLevel = Convert.ToDouble(splitData2[j]);

                                break;
                            }

                            string windowLayerName1 = windowStrtText + (Convert.ToString(windowLayerCount));
                            dataGridViewRow.Rows.Add(windowLayerName1, windowSillLevel, windowLintelLevel);
                            listOfWindowLayerName.Add(windowLayerName1);
                            listOfWindowSillLevel.Add(Convert.ToDouble(windowSillLevel));
                            listOfWindowLintelLevel.Add(Convert.ToDouble(windowLintelLevel));
                            windowLayerCount++;

                        }
                        break;
                    }
                }
            }

            else
            {
                string windowLayerName1 = windowStrtText + (Convert.ToString(windowLayerCount));
                windowLayerCount++;

                double windowSillLevel1 = 900;
                double windowLintelLevel1 = 2100;
                dataGridViewRow.Rows.Add(windowLayerName1, windowSillLevel1, windowLintelLevel1);

                listOfWindowLayerName.Add(windowLayerName1);
                listOfWindowSillLevel.Add(Convert.ToDouble(windowSillLevel1));
                listOfWindowLintelLevel.Add(Convert.ToDouble(windowLintelLevel1));

                string windowLayerName = windowStrtText + (Convert.ToString(windowLayerCount));
                windowLayerCount++;

                double windowSillLevel = 1200;
                double windowLintelLevel = 2100;
                dataGridViewRow.Rows.Add(windowLayerName, windowSillLevel, windowLintelLevel);

                listOfWindowLayerName.Add(windowLayerName);
                listOfWindowSillLevel.Add(Convert.ToDouble(windowSillLevel));
                listOfWindowLintelLevel.Add(Convert.ToDouble(windowLintelLevel));
            }

        }
        public void setDefaultValueInChajjaGridView(DataGridView dataGridViewRow)
        {
            dataGridViewRow.Rows.Clear();
            listOfChajjaLayerName.Clear();
            listOfNearSideThickness.Clear();
            listOfFarSideThickness.Clear(); ;//Added on 21/06/2023 by PRT

            chajjaLayerCount = 1;

            if (PanelLayoutInputHelper.listOfAllSelectedEntity.Count != 0)
            {
                double nearSideThickness = 0;
                double farSideThickness = 0;

                for (int i = 0; i < PanelLayoutInputHelper.listOfAllSelectedEntity.Count; i++)
                {
                    string data = PanelLayoutInputHelper.listOfAllSelectedEntity[i];
                    if (data.Contains("Chajja"))
                    {
                        data = PanelLayoutInputHelper.listOfAllSelectedEntity[i + 1];
                        string[] arr = data.Split(':');
                        string chajjaData = arr[1];
                        string[] splitData = chajjaData.Split(',');

                        data = PanelLayoutInputHelper.listOfAllSelectedEntity[i + 2];
                        arr = data.Split(':');
                        chajjaData = arr[1];
                        string[] splitData2 = chajjaData.Split(',');
                        int count = splitData.Count();
                        if (splitData.Count() > splitData2.Count())
                        {

                            count = count - 1;
                        }

                        for (int j = 0; j < count; j++)
                        {

                            nearSideThickness = Convert.ToDouble(splitData[j]);

                            for (int k = 0; k < splitData2.Count(); k++)
                            {
                                if(splitData2[j].Contains("}"))
                                {
                                    splitData2[j] = splitData2[j].Replace("}","");
                                }
                                farSideThickness = Convert.ToDouble(splitData2[j]);

                                break;
                            }

                            string chajjaLayerName1 = "AEE_Chajja" + (Convert.ToString(chajjaLayerCount));
                            dataGridViewRow.Rows.Add(chajjaLayerName1, nearSideThickness, farSideThickness);
                            listOfChajjaLayerName.Add(chajjaLayerName1);
                            listOfNearSideThickness.Add(Convert.ToDouble(nearSideThickness));
                            listOfFarSideThickness.Add(Convert.ToDouble(farSideThickness));
                            chajjaLayerCount++;

                        }
                        break;
                    }
                }
            }//Changes made on 27/06/2023 by PRT
            else
            {
                string chajjaLayerName1 = "AEE_Chajja" + (Convert.ToString(chajjaLayerCount));
                chajjaLayerCount++;

                double nearSideThickness = 150;
                double farSideThickness = 100;
                dataGridViewRow.Rows.Add(chajjaLayerName1, nearSideThickness, farSideThickness);

                listOfChajjaLayerName.Add(chajjaLayerName1);
                listOfNearSideThickness.Add(nearSideThickness);
                listOfFarSideThickness.Add(farSideThickness);
            }
        }
        public static bool checkWindowLayerIsExist(string inputWindowLayer)
        {
            if (listOfWindowLayerName.Count != 0)
            {
                int index = listOfWindowLayerName.IndexOf(inputWindowLayer);

                if (index != -1)
                    return true;
            }

            return false;
        }

        public static bool checkChajjaLayerIsExist(string inputChajjaLayer)
        {
            if (listOfChajjaLayerName.Count != 0)
            {
                int index = listOfChajjaLayerName.IndexOf(inputChajjaLayer);

                if (index != -1)
                    return true;
            }

            return false;
        }

        public static double getWindowWallHeight(string inputWindowLayer, double RC, bool flagOfPanelWithRC, ObjectId sunkanSlabId)
        {
            double windowWallHeight = 0;

            if (listOfWindowLayerName.Count != 0)
            {
                int index = listOfWindowLayerName.IndexOf(inputWindowLayer);

                if (index != -1)
                {
                    double windowSillLevel = getWindowSillLevel(inputWindowLayer);

                    var flagOfGreater = SunkanSlabHelper.checkSunkanSlabLevelDiffrnce_Is_Greater(sunkanSlabId);

                    if (flagOfGreater == true || flagOfPanelWithRC == false)
                        windowWallHeight = windowSillLevel - RC;
                    else
                        windowWallHeight = windowSillLevel;
                }
            }


            return windowWallHeight;
        }

        public static double getWindowLintelLevel(string inputWindowLayer)
        {
            double windowLintelLevel = 0;

            if (listOfWindowLayerName.Count != 0)
            {
                int index = listOfWindowLayerName.IndexOf(inputWindowLayer);

                if (index != -1)
                    windowLintelLevel = listOfWindowLintelLevel[index];
            }

            return windowLintelLevel;
        }

        public static double getWindowSillLevel(string inputWindowLayer)
        {
            double windowSillLevel = 0;

            if (listOfWindowLayerName.Count != 0)
            {
                int index = listOfWindowLayerName.IndexOf(inputWindowLayer);

                if (index != -1)
                    windowSillLevel = listOfWindowSillLevel[index];
            }

            return windowSillLevel;
        }
    }
}
