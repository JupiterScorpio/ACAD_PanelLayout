using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PanelLayout_App.CivilModel
{
    public class Parapet_UI_Helper
    {
        public static List<string> listOfParapetLayerName = new List<string>();
        public static List<double> listOfParapetTop = new List<double>();
        public static List<string> listOfParapetBottom = new List<string>();

        public static int parapetLayerColumnIndex = 0;
        public static int parapetTopColumnIndex = 2;
        public static int parapetBottomColumnIndex = 1;

        public static string parapetStrtText = "AEE_Parapet";
        public static int parapetLayerCount = 1;
        public static int parapetLayerColorIndex = 8;
        public static string defaultParapetBottomValue = "Up to slab";

        public void getDataFromParapetGridView(DataGridView dataGridViewRow)
        {
            Parapet_UI_Helper.listOfParapetLayerName.Clear();
            Parapet_UI_Helper.listOfParapetTop.Clear();
            Parapet_UI_Helper.listOfParapetBottom.Clear();

            int columnCount = dataGridViewRow.ColumnCount;
            int rowCount = dataGridViewRow.RowCount;
            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                string parapetLayerName = null;
                string parapetTop = null;
                string parapetBottom = null;
                for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
                {
                    var obj = dataGridViewRow.Rows[rowIndex].Cells[columnIndex].Value;
                    string data = Convert.ToString(obj);
                    if (string.IsNullOrEmpty(data) || string.IsNullOrWhiteSpace(data))
                    {

                    }
                    else
                    {
                        if (parapetLayerColumnIndex == (columnIndex))
                        {
                            parapetLayerName = data;
                        }
                        else if (parapetTopColumnIndex == (columnIndex))
                        {
                            parapetTop = data;
                        }
                        else if (parapetBottomColumnIndex == (columnIndex))
                        {
                            parapetBottom = data;
                        }
                    }
                }
                if (string.IsNullOrEmpty(parapetLayerName) || string.IsNullOrWhiteSpace(parapetLayerName) || string.IsNullOrEmpty(parapetTop) || string.IsNullOrWhiteSpace(parapetTop) || string.IsNullOrEmpty(parapetBottom) || string.IsNullOrWhiteSpace(parapetBottom))
                {

                }
                else
                {
                    listOfParapetLayerName.Add(parapetLayerName);
                    listOfParapetBottom.Add(Convert.ToString(parapetBottom));
                    listOfParapetTop.Add(Convert.ToDouble(parapetTop));
                }
            }
        }

        public void previousDataInParapetGridView(DataGridView dataGridViewRow)
        {
            dataGridViewRow.Rows.Clear();
            if (listOfParapetLayerName.Count != 0)
            {
                parapetLayerCount = Lift_UI_Helper.getLayerIndex(listOfParapetLayerName);
                for (int index = 0; index < listOfParapetLayerName.Count; index++)
                {
                    string prvsBeamLayerName = listOfParapetLayerName[index];
                    string prvsBeamBottom = Convert.ToString(listOfParapetBottom[index]);
                    string prvsBeamTop = Convert.ToString(listOfParapetTop[index]);
                    dataGridViewRow.Rows.Add(prvsBeamLayerName, prvsBeamBottom, prvsBeamTop);
                }
            }
            else
            {
                parapetLayerCount = 1;
            }
        }

        public void setDefaultValueInParapetGridView(DataGridView dataGridViewRow)
        {
            dataGridViewRow.Rows.Clear();
            listOfParapetLayerName.Clear();
            listOfParapetTop.Clear();
            listOfParapetBottom.Clear();
            parapetLayerCount = 1;


            string parapetBottom = defaultParapetBottomValue;

            if (PanelLayoutInputHelper.listOfAllSelectedEntity.Count != 0)
            {
                for (int i = 0; i < PanelLayoutInputHelper.listOfAllSelectedEntity.Count; i++)
                {
                    string data = PanelLayoutInputHelper.listOfAllSelectedEntity[i];
                    if (data.Contains("Parapet:"))
                    {


                        data = PanelLayoutInputHelper.listOfAllSelectedEntity[i + 1];
                        string[] arr = data.Split(':');
                        string beamData = arr[1];
                        string[] splitData = beamData.Split(',');
                        for (int j = 0; j < splitData.Count(); j++)
                        {
                            double parapetTop = Convert.ToDouble(splitData[j]);


                            string parapetLayerName = parapetStrtText + (Convert.ToString(parapetLayerCount));

                            dataGridViewRow.Rows.Add(parapetLayerName, parapetBottom, parapetTop);
                            listOfParapetLayerName.Add(parapetLayerName);
                            listOfParapetBottom.Add(parapetBottom);
                            listOfParapetTop.Add(parapetTop);
                            parapetLayerCount++;
                        }

                        break;

                    }
                }
            }
            else
            {
                string parapetLayerName1 = parapetStrtText + (Convert.ToString(parapetLayerCount));
                parapetLayerCount++;

                string parapetBottom1 = defaultParapetBottomValue;
                double parapetTop1 = 1000;

                dataGridViewRow.Rows.Add(parapetLayerName1, parapetBottom1, parapetTop1);

                listOfParapetLayerName.Add(parapetLayerName1);
                listOfParapetBottom.Add(parapetBottom1);
                listOfParapetTop.Add(parapetTop1);


                string parapetLayerName2 = parapetStrtText + (Convert.ToString(parapetLayerCount));
                parapetLayerCount++;

                string parapetBottom2 = defaultParapetBottomValue;
                double parapetTop2 = 1200;

                dataGridViewRow.Rows.Add(parapetLayerName2, parapetBottom2, parapetTop2);

                listOfParapetLayerName.Add(parapetLayerName2);
                listOfParapetBottom.Add(parapetBottom2);
                listOfParapetTop.Add(parapetTop2);
            }

            //dataGridViewRow.Rows.Add(parapetLayerName1, parapetBottom1, parapetTop1);

            //listOfParapetLayerName.Add(parapetLayerName1);
            //listOfParapetBottom.Add(parapetBottom1);
            //listOfParapetTop.Add(parapetTop1);


            //string parapetLayerName2 = parapetStrtText + (Convert.ToString(parapetLayerCount));
            //parapetLayerCount++;

            //string parapetBottom2 = defaultParapetBottomValue;
            ////double parapetTop2 = 1200;

            //dataGridViewRow.Rows.Add(parapetLayerName2, parapetBottom2, parapetTop2);

            //listOfParapetLayerName.Add(parapetLayerName2);
            //listOfParapetBottom.Add(parapetBottom2);
            //listOfParapetTop.Add(parapetTop2);

            //Commented on 20/06/2023 by PRT
        }

        public static bool checkParapetLayerIsExist(string inputParapetLayer)
        {
            if (listOfParapetLayerName.Count != 0)
            {
                int index = listOfParapetLayerName.IndexOf(inputParapetLayer);
                if (index != -1)
                {
                    return true;
                }
            }
            return false;
        }

        public static double getParapetTopHeight(string inputParapetLayer)
        {
            double parapetTopHeigh = 0;
            if (listOfParapetLayerName.Count != 0)
            {
                int index = listOfParapetLayerName.IndexOf(inputParapetLayer);
                if (index != -1)
                {
                    parapetTopHeigh = listOfParapetTop[index];
                }
            }
            return parapetTopHeigh;
        }


    }
}
