using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PanelLayout_App.CivilModel
{
    public class StairCase_UI_Helper
    {
        public static List<string> listOfStairCaseLayerName = new List<string>();

        public static int stairCaseLayerColumnIndex = 0;    

        public static string stairCaseStrtText = "AEE_StairCase";
        public static int stairCaseLayerCount = 1;


        public void getDataFromStairCaseGridView(DataGridView dataGridViewRow)
        {
            listOfStairCaseLayerName.Clear();

            int columnCount = dataGridViewRow.ColumnCount;
            int rowCount = dataGridViewRow.RowCount;
            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                string staircaseLayerName = null;
       
                for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
                {
                    var obj = dataGridViewRow.Rows[rowIndex].Cells[columnIndex].Value;
                    string data = Convert.ToString(obj);
                    if (string.IsNullOrEmpty(data) || string.IsNullOrWhiteSpace(data))
                    {

                    }
                    else
                    {
                        if (stairCaseLayerColumnIndex == (columnIndex))
                        {
                            staircaseLayerName = data;
                        }                     
                    }
                }
                if (string.IsNullOrEmpty(staircaseLayerName) || string.IsNullOrWhiteSpace(staircaseLayerName))
                {

                }
                else
                {
                    listOfStairCaseLayerName.Add(staircaseLayerName);               
                }
            }
        }
        public void previousDataInStairCaseGridView(DataGridView dataGridViewRow)
        {
            dataGridViewRow.Rows.Clear();
            if (listOfStairCaseLayerName.Count != 0)
            {
                stairCaseLayerCount = Lift_UI_Helper.getLayerIndex(listOfStairCaseLayerName);
                for (int index = 0; index < listOfStairCaseLayerName.Count; index++)
                {
                    string prvsStairCaseLayerName = listOfStairCaseLayerName[index];             
                    dataGridViewRow.Rows.Add(prvsStairCaseLayerName);
                }
            }
            else
            {
                stairCaseLayerCount = 1;
            }
        }

        public void setDefaultValueInStairCaseGridView(DataGridView dataGridViewRow)
        {
            dataGridViewRow.Rows.Clear();
            listOfStairCaseLayerName.Clear();
            stairCaseLayerCount = 1;

            string stairCaseLayerName1 = stairCaseStrtText + (Convert.ToString(stairCaseLayerCount));
            stairCaseLayerCount++;

            dataGridViewRow.Rows.Add(stairCaseLayerName1);

            listOfStairCaseLayerName.Add(stairCaseLayerName1);


            string stairCaseLayerName2 = stairCaseStrtText + (Convert.ToString(stairCaseLayerCount));
            stairCaseLayerCount++;

            dataGridViewRow.Rows.Add(stairCaseLayerName2);

            listOfStairCaseLayerName.Add(stairCaseLayerName2);
        }

        public static bool checkStairCaseLayerIsExist(string inputStairCaseLayer)
        {
            if (listOfStairCaseLayerName.Count != 0)
            {
                int index = listOfStairCaseLayerName.IndexOf(inputStairCaseLayer);
                if (index != -1)
                {
                    return true;
                }
            }
            return false;
        }
  

    }
}
