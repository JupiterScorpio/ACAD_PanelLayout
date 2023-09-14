using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PanelLayout_App.CivilModel
{
    public class ExternalSunkanSlab_UI_Helper
    {
        public static int externalSunkanSlabLayerColumnIndex = 0;
        public static int externalSunkanSlabLevelDifferenceColumnIndex = 1;
        public static int externalSunkanSlabThickColumnIndex = 2;
        public static string externalSunkanSlabStrtText = "AEE_ExternalSunkanSlab";
        public static int externalSunkanSlabNameCount = 1;
        public static int externalSunkanSlabLayerColorIndex = 5;

        public static List<string> listOfExternalSunkanSlabLayerName = new List<string>();
        public static List<double> listOfExternalSunkanSlabLevelDifference = new List<double>();
        public static List<double> listOfExternalSunkanSlabThick = new List<double>();

        public void getDataFromExternalSunkanSlabGridView(DataGridView dataGridViewRow)
        {
            listOfExternalSunkanSlabLayerName.Clear();
            listOfExternalSunkanSlabLevelDifference.Clear();
            listOfExternalSunkanSlabThick.Clear();

            int columnCount = dataGridViewRow.ColumnCount;
            int rowCount = dataGridViewRow.RowCount;
            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                string sunkanSlabLayerName = null;
                string sunkanSlabBottom = null;
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
                        if (externalSunkanSlabLayerColumnIndex == (columnIndex))
                        {
                            sunkanSlabLayerName = data;
                        }
                        else if (externalSunkanSlabLevelDifferenceColumnIndex == (columnIndex))
                        {
                            sunkanSlabBottom = data;
                        }
                        else if (externalSunkanSlabThickColumnIndex == (columnIndex))
                        {
                            sunkanSlabThick = data;
                        }
                    }
                }
                if (string.IsNullOrEmpty(sunkanSlabLayerName) || string.IsNullOrWhiteSpace(sunkanSlabLayerName) || string.IsNullOrEmpty(sunkanSlabThick) || string.IsNullOrWhiteSpace(sunkanSlabThick) || string.IsNullOrEmpty(sunkanSlabBottom) || string.IsNullOrWhiteSpace(sunkanSlabBottom))
                {

                }
                else
                {
                    listOfExternalSunkanSlabLayerName.Add(sunkanSlabLayerName);
                    listOfExternalSunkanSlabLevelDifference.Add(Convert.ToDouble(sunkanSlabBottom));
                    listOfExternalSunkanSlabThick.Add(Convert.ToDouble(sunkanSlabThick));
                }
            }
        }
        public void previousDataInExternalSunkanSlabGridView(DataGridView dataGridViewRow)
        {
            dataGridViewRow.Rows.Clear();
            if (listOfExternalSunkanSlabLayerName.Count != 0)
            {
                externalSunkanSlabNameCount = Lift_UI_Helper.getLayerIndex(listOfExternalSunkanSlabLayerName);
                for (int index = 0; index < listOfExternalSunkanSlabLayerName.Count; index++)
                {
                    string prvsSunkanSlabLayer = listOfExternalSunkanSlabLayerName[index];
                    string prvsSunkanSlabBottom = Convert.ToString(listOfExternalSunkanSlabLevelDifference[index]);
                    string prvSunkanSlabThick = Convert.ToString(listOfExternalSunkanSlabThick[index]);
                    dataGridViewRow.Rows.Add(prvsSunkanSlabLayer, prvsSunkanSlabBottom, prvSunkanSlabThick);
                }
            }
            else
            {
                externalSunkanSlabNameCount = 1;
            }
        }

        public void setDefaultValueInExternalSunkanSlabGridView(DataGridView dataGridViewRow)
        {
            //if (listOfSunkanSlabLayerName.Count == 0)
            {
                dataGridViewRow.Rows.Clear();
                listOfExternalSunkanSlabLayerName.Clear();
                listOfExternalSunkanSlabLevelDifference.Clear();
                listOfExternalSunkanSlabThick.Clear();
                externalSunkanSlabNameCount = 1;

                if (PanelLayoutInputHelper.listOfAllSelectedEntity.Count != 0)
                {
                    for (int i = 0; i < PanelLayoutInputHelper.listOfAllSelectedEntity.Count; i++)
                    {
                        string data = PanelLayoutInputHelper.listOfAllSelectedEntity[i];
                        if (data.Contains("External"))
                        {


                            data = PanelLayoutInputHelper.listOfAllSelectedEntity[i + 1];
                            string[] arr = data.Split(':');
                            string winData = arr[1];
                            string[] splitData = winData.Split(',');

                            data = PanelLayoutInputHelper.listOfAllSelectedEntity[i + 2];
                            arr = data.Split(':');
                            winData = arr[1];
                            string[] splitData2 = winData.Split(',');
                            double sunkanSlabThick = 0;

                            int count = splitData.Count();
                            if (splitData.Count() > splitData2.Count())
                            {

                                count = count - 1;
                            }

                            for (int j = 0; j < count; j++)
                            {

                                double sunkanSlabBottom = Convert.ToDouble(splitData[j]);

                                for (int k = 0; k < splitData2.Count(); k++)
                                {
                                    //if (splitData2[j] != "")
                                    //{
                                    sunkanSlabThick = Convert.ToDouble(splitData2[j]);
                                    //}


                                    break;
                                }

                                string sunkanSlabLayerName = externalSunkanSlabStrtText + (Convert.ToString(externalSunkanSlabNameCount));

                                dataGridViewRow.Rows.Add(sunkanSlabLayerName, sunkanSlabBottom, sunkanSlabThick);

                                listOfExternalSunkanSlabLayerName.Add(sunkanSlabLayerName);
                                listOfExternalSunkanSlabLevelDifference.Add(Convert.ToDouble(sunkanSlabBottom));
                                listOfExternalSunkanSlabThick.Add(Convert.ToDouble(sunkanSlabThick));
                                externalSunkanSlabNameCount++;

                            }
                            break;
                        }
                    }
                }
                else
                {
                    string sunkanSlabLayerName1 = externalSunkanSlabStrtText + (Convert.ToString(externalSunkanSlabNameCount));
                    externalSunkanSlabNameCount++;

                    double sunkanSlabBottom1 = 60;
                    double sunkanSlabThick1 = 150;

                    dataGridViewRow.Rows.Add(sunkanSlabLayerName1, sunkanSlabBottom1, sunkanSlabThick1);

                    listOfExternalSunkanSlabLayerName.Add(sunkanSlabLayerName1);
                    listOfExternalSunkanSlabLevelDifference.Add(Convert.ToDouble(sunkanSlabBottom1));
                    listOfExternalSunkanSlabThick.Add(Convert.ToDouble(sunkanSlabThick1));


                    string sunkanSlabLayerName2 = externalSunkanSlabStrtText + (Convert.ToString(externalSunkanSlabNameCount));
                    externalSunkanSlabNameCount++;

                    double sunkanSlabBottom2 = 120;
                    double sunkanSlabThick2 = 150;

                    dataGridViewRow.Rows.Add(sunkanSlabLayerName2, sunkanSlabBottom2, sunkanSlabThick2);

                    listOfExternalSunkanSlabLayerName.Add(sunkanSlabLayerName2);
                    listOfExternalSunkanSlabLevelDifference.Add(Convert.ToDouble(sunkanSlabBottom2));
                    listOfExternalSunkanSlabThick.Add(Convert.ToDouble(sunkanSlabThick2));
                }

                //dataGridViewRow.Rows.Add(sunkanSlabLayerName1, sunkanSlabBottom1, sunkanSlabThick1);

                //listOfExternalSunkanSlabLayerName.Add(sunkanSlabLayerName1);
                //listOfExternalSunkanSlabLevelDifference.Add(Convert.ToDouble(sunkanSlabBottom1));
                //listOfExternalSunkanSlabThick.Add(Convert.ToDouble(sunkanSlabThick1));


                //string sunkanSlabLayerName2 = externalSunkanSlabStrtText + (Convert.ToString(externalSunkanSlabNameCount));
                //externalSunkanSlabNameCount++;

                ////double sunkanSlabBottom2 = 120;
                ////double sunkanSlabThick2 = 150;

                //dataGridViewRow.Rows.Add(sunkanSlabLayerName2, sunkanSlabBottom2, sunkanSlabThick2);

                //listOfExternalSunkanSlabLayerName.Add(sunkanSlabLayerName2);
                //listOfExternalSunkanSlabLevelDifference.Add(Convert.ToDouble(sunkanSlabBottom2));
                //listOfExternalSunkanSlabThick.Add(Convert.ToDouble(sunkanSlabThick2));

                //Commented on 20/06/2023 by PRT
            }
        }
        public static bool checkExternalSunkanSlabLayerIsExist(string inputWindowLayer)
        {
            if (listOfExternalSunkanSlabLayerName.Count != 0)
            {
                int index = listOfExternalSunkanSlabLayerName.IndexOf(inputWindowLayer);
                if (index != -1)
                {
                    return true;
                }
            }
            return false;
        }
        public static bool checkExternalSunkanSlabLayerIsExist(ObjectId sukanSlabId)
        {
            bool flag = false;
            if (listOfExternalSunkanSlabLayerName.Count != 0)
            {
                if (sukanSlabId.IsValid == true)
                {
                    var sukanSlab = AEE_Utility.GetEntityForRead(sukanSlabId);
                    flag = checkExternalSunkanSlabLayerIsExist(sukanSlab.Layer);
                }
            }
            return flag;
        }

        public static double getExternalLevelDifferenceOfSunkanSlab(string inputSunkanSlabLayer)
        {
            double levelDifference = 0;

            if (listOfExternalSunkanSlabLayerName.Count != 0)
            {
                int index = listOfExternalSunkanSlabLayerName.IndexOf(inputSunkanSlabLayer);
                if (index != -1)
                {
                    levelDifference = listOfExternalSunkanSlabLevelDifference[index];
                }
            }

            return levelDifference;
        }


        internal static double getThickness(string layerName)
        {
            if (layerName == null)
            {
                return -1;
            }
            if (!layerName.ToUpper().StartsWith(externalSunkanSlabStrtText.ToUpper()))
            {
                return -1;
            }
            for (var n = 0; n < listOfExternalSunkanSlabLayerName.Count; ++n)
            {
                if (layerName.ToUpper().Trim().CompareTo(listOfExternalSunkanSlabLayerName[n].ToUpper().Trim()) == 0)
                {
                    if (n < listOfExternalSunkanSlabThick.Count)
                    {
                        if (listOfExternalSunkanSlabThick[n] > 0)
                        {
                            return listOfExternalSunkanSlabThick[n];
                        }
                    }
                }
            }
            return PanelLayout_UI.slabThickness;
        }


    }
}
