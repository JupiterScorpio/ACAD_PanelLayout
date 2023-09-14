using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PanelLayout_App.CivilModel
{
    public class Lift_UI_Helper
    {
        public static List<string> listOfLiftLayerName = new List<string>();

        public static int liftLayerColumnIndex = 0;

        public static string liftStrtText = "AEE_Lift";
        public static int liftLayerCount = 1;
        public static int liftLayerColorIndex = 8; //MODIFIED by N - OCT22 
        public void getDataFromLiftGridView(DataGridView dataGridViewRow)
        {
            listOfLiftLayerName.Clear();

            int columnCount = dataGridViewRow.ColumnCount;
            int rowCount = dataGridViewRow.RowCount;
            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                string liftLayerName = null;

                for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
                {
                    var obj = dataGridViewRow.Rows[rowIndex].Cells[columnIndex].Value;
                    string data = Convert.ToString(obj);
                    if (string.IsNullOrEmpty(data) || string.IsNullOrWhiteSpace(data))
                    {

                    }
                    else
                    {
                        if (liftLayerColumnIndex == (columnIndex))
                        {
                            liftLayerName = data;
                        }
                    }
                }
                if (string.IsNullOrEmpty(liftLayerName) || string.IsNullOrWhiteSpace(liftLayerName))
                {

                }
                else
                {
                    listOfLiftLayerName.Add(liftLayerName);
                }
            }
        }
        public void previousDataInLiftGridView(DataGridView dataGridViewRow)
        {
            dataGridViewRow.Rows.Clear();
            if (listOfLiftLayerName.Count != 0)
            {
                liftLayerCount = Lift_UI_Helper.getLayerIndex(listOfLiftLayerName);           

                for (int index = 0; index < listOfLiftLayerName.Count; index++)
                {
                    string prvsLiftLayerName = listOfLiftLayerName[index];
                    dataGridViewRow.Rows.Add(prvsLiftLayerName);
                }
            }
            else
            {
                liftLayerCount = 1;
            }
        }

        public void setDefaultValueInLiftGridView(DataGridView dataGridViewRow)
        {
            dataGridViewRow.Rows.Clear();
            listOfLiftLayerName.Clear();
            liftLayerCount = 1;

            string liftLayerName1 = liftStrtText + (Convert.ToString(liftLayerCount));
            liftLayerCount++;

            dataGridViewRow.Rows.Add(liftLayerName1);

            listOfLiftLayerName.Add(liftLayerName1);


            string liftLayerName2 = liftStrtText + (Convert.ToString(liftLayerCount));
            liftLayerCount++;

            dataGridViewRow.Rows.Add(liftLayerName2);

            listOfLiftLayerName.Add(liftLayerName2);
        }

        public static bool checkLiftLayerIsExist(string inputLiftLayer)
        {
            if (listOfLiftLayerName.Count != 0)
            {
                int index = listOfLiftLayerName.IndexOf(inputLiftLayer);
                if (index != -1)
                {
                    return true;
                }
            }
            return false;
        }


        public static int getLayerIndex(List<string> listOfAllLayerName)
        {
            List<int> listOfAllLayerIndex = new List<int>();
            foreach (var layerName in listOfAllLayerName)
            {
                int lengthOfStrt = layerName.Length;
                int layerNumber = Convert.ToInt32(layerName.Substring(lengthOfStrt - 1));
                listOfAllLayerIndex.Add(layerNumber);
            }
            int layerCount = listOfAllLayerIndex.Max();
            layerCount++;
            return layerCount;
        }
    }
}
