using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PanelLayout_App.CivilModel
{
    public class InternalSunkanSlab_UI_Helper
    {
        public static int internalSunkanSlabLayerColumnIndex = 0;
        public static int internalSunkanSlabLevelDifferenceColumnIndex = 1;
        public static int internalSunkanSlabThickColumnIndex = 2;
        public static string internalSunkanSlabStrtText = "AEE_InternalSunkanSlab";
        public static int internalSunkanSlabNameCount = 1;
        public static int internalSunkanSlabLayerColorIndex = 6;

        public static List<string> listOfInternalSunkanSlabLayerName = new List<string>();
        public static List<double> listOfInternalSunkanSlabLevelDifference = new List<double>();
        public static List<double> listOfInternalSunkanSlabThick = new List<double>();

        public void getDataFromSunkanSlabGridView(DataGridView dataGridViewRow)
        {
            listOfInternalSunkanSlabLayerName.Clear();
            listOfInternalSunkanSlabLevelDifference.Clear();
            listOfInternalSunkanSlabThick.Clear();

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
                        if (internalSunkanSlabLayerColumnIndex == (columnIndex))
                        {
                            sunkanSlabLayerName = data;
                        }
                        else if (internalSunkanSlabLevelDifferenceColumnIndex == (columnIndex))
                        {
                            sunkanSlabBottom = data;
                        }
                        else if (internalSunkanSlabThickColumnIndex == (columnIndex))
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
                    listOfInternalSunkanSlabLayerName.Add(sunkanSlabLayerName);
                    listOfInternalSunkanSlabLevelDifference.Add(Convert.ToDouble(sunkanSlabBottom));
                    listOfInternalSunkanSlabThick.Add(Convert.ToDouble(sunkanSlabThick));
                }
            }
        }


        public void previousDataInSunkanSlabGridView(DataGridView dataGridViewRow)
        {
            dataGridViewRow.Rows.Clear();
            if (listOfInternalSunkanSlabLayerName.Count != 0)
            {
                internalSunkanSlabNameCount = Lift_UI_Helper.getLayerIndex(listOfInternalSunkanSlabLayerName);
                for (int index = 0; index < listOfInternalSunkanSlabLayerName.Count; index++)
                {
                    string prvsSunkanSlabLayer = listOfInternalSunkanSlabLayerName[index];
                    string prvsSunkanSlabBottom = Convert.ToString(listOfInternalSunkanSlabLevelDifference[index]);
                    string prvSunkanSlabThick = Convert.ToString(listOfInternalSunkanSlabThick[index]);
                    dataGridViewRow.Rows.Add(prvsSunkanSlabLayer, prvsSunkanSlabBottom, prvSunkanSlabThick);
                }
            }
            else
            {
                internalSunkanSlabNameCount = 1;
            }
        }


        public void setDefaultValueInSunkanSlabGridView(DataGridView dataGridViewRow)
        {
            dataGridViewRow.Rows.Clear();
            listOfInternalSunkanSlabLayerName.Clear();
            listOfInternalSunkanSlabLevelDifference.Clear();
            listOfInternalSunkanSlabThick.Clear();
            internalSunkanSlabNameCount = 1;

            if (PanelLayoutInputHelper.listOfAllSelectedEntity.Count != 0)
            {
                for (int i = 0; i < PanelLayoutInputHelper.listOfAllSelectedEntity.Count; i++)
                {
                    string data = PanelLayoutInputHelper.listOfAllSelectedEntity[i];
                    if (data.Contains("Internal"))
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
                               
                                sunkanSlabThick = Convert.ToDouble(splitData2[j]);
                               
                                break;
                            }

                            string sunkanSlabLayerName = internalSunkanSlabStrtText + (Convert.ToString(internalSunkanSlabNameCount));

                            dataGridViewRow.Rows.Add(sunkanSlabLayerName, sunkanSlabBottom, sunkanSlabThick);

                            listOfInternalSunkanSlabLayerName.Add(sunkanSlabLayerName);
                            listOfInternalSunkanSlabLevelDifference.Add(Convert.ToDouble(sunkanSlabBottom));
                            listOfInternalSunkanSlabThick.Add(Convert.ToDouble(sunkanSlabThick));
                            internalSunkanSlabNameCount++;
                            //}
                        }
                        break;
                    }
                }
            }
             
            else
            {
                string sunkanSlabLayerName1 = internalSunkanSlabStrtText + (Convert.ToString(internalSunkanSlabNameCount));
                internalSunkanSlabNameCount++;

                double sunkanSlabBottom1 = 60;
                double sunkanSlabThick1 = 150;

                dataGridViewRow.Rows.Add(sunkanSlabLayerName1, sunkanSlabBottom1, sunkanSlabThick1);

                listOfInternalSunkanSlabLayerName.Add(sunkanSlabLayerName1);
                listOfInternalSunkanSlabLevelDifference.Add(Convert.ToDouble(sunkanSlabBottom1));
                listOfInternalSunkanSlabThick.Add(Convert.ToDouble(sunkanSlabThick1));


                string sunkanSlabLayerName2 = internalSunkanSlabStrtText + (Convert.ToString(internalSunkanSlabNameCount));
                internalSunkanSlabNameCount++;

                double sunkanSlabBottom2 = 120;
                double sunkanSlabThick2 = 150;

                dataGridViewRow.Rows.Add(sunkanSlabLayerName2, sunkanSlabBottom2, sunkanSlabThick2);

                listOfInternalSunkanSlabLayerName.Add(sunkanSlabLayerName2);
                listOfInternalSunkanSlabLevelDifference.Add(Convert.ToDouble(sunkanSlabBottom2));
                listOfInternalSunkanSlabThick.Add(Convert.ToDouble(sunkanSlabThick2));
            }


                //Changes made on 20/06/2023 by PRT
           
        }


        public static bool checkSunkanSlabLayerIsExist(string inputWindowLayer)
        {
            if (listOfInternalSunkanSlabLayerName.Count != 0)
            {
                int index = listOfInternalSunkanSlabLayerName.IndexOf(inputWindowLayer);
                if (index != -1)
                {
                    return true;
                }
            }
            return false;
        }

        public static double getSunkanSlabThick(string inputSunkanSlabLayer) // Added by PBAC 15/11/2021
        {
            double slabThick = 0;

            int index = listOfInternalSunkanSlabLayerName.IndexOf(inputSunkanSlabLayer);
            if (index != -1)
            {
                slabThick = listOfInternalSunkanSlabThick[index];
            }
            else
            {
                int indexOfExternalSunkan = ExternalSunkanSlab_UI_Helper.listOfExternalSunkanSlabLayerName.IndexOf(inputSunkanSlabLayer);
                if (indexOfExternalSunkan != -1)
                {
                    slabThick = ExternalSunkanSlab_UI_Helper.listOfExternalSunkanSlabThick[indexOfExternalSunkan];
                }
            }

            return slabThick;
        }
        public static double getLevelDifferenceOfSunkanSlab(string inputSunkanSlabLayer)
        {
            double levelDifference = 0;

            int index = listOfInternalSunkanSlabLayerName.IndexOf(inputSunkanSlabLayer);
            if (index != -1)
            {
                levelDifference = listOfInternalSunkanSlabLevelDifference[index];
            }
            else
            {
                int indexOfExternalSunkan = ExternalSunkanSlab_UI_Helper.listOfExternalSunkanSlabLayerName.IndexOf(inputSunkanSlabLayer);
                if (indexOfExternalSunkan != -1)
                {
                    levelDifference = ExternalSunkanSlab_UI_Helper.listOfExternalSunkanSlabLevelDifference[indexOfExternalSunkan];
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
            if (!layerName.ToUpper().StartsWith(internalSunkanSlabStrtText.ToUpper()))
            {
                return -1;
            }
            for (var n = 0; n < listOfInternalSunkanSlabLayerName.Count; ++n)
            {
                if (layerName.ToUpper().Trim().CompareTo(listOfInternalSunkanSlabLayerName[n].ToUpper().Trim()) == 0)
                {
                    if (n < listOfInternalSunkanSlabThick.Count)
                    {
                        if (listOfInternalSunkanSlabThick[n] > 0)
                        {
                            return listOfInternalSunkanSlabThick[n];
                        }
                    }
                }
            }
            return PanelLayout_UI.slabThickness;
        }
    }
}
