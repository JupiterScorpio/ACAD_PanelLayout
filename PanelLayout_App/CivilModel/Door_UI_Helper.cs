using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PanelLayout_App.CivilModel
{
    public class Door_UI_Helper
    {
        public static int doorLayerColumnIndex = 0;
        public static int doorLintelLevelColumnIndex = 1;
        public static int doorLayerCount = 1;

        public static List<string> listOfDoorLayerName = new List<string>();
        public static List<double> listOfDoorLintelLevel = new List<double>();

        public static int doorLayerColor = 150;
        public static string doorStrtText = "AEE_Door";
        public static string doorTextLayerName = "AEE_Door_Text";

        public void getDataFromDoorGridView(DataGridView dataGridViewRow)
        {
            listOfDoorLayerName.Clear();
            listOfDoorLintelLevel.Clear();

            int columnCount = dataGridViewRow.ColumnCount;
            int rowCount = dataGridViewRow.RowCount;
            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                string doorLayerName = null;
                string doorLintelLevel = null;
                for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
                {
                    var obj = dataGridViewRow.Rows[rowIndex].Cells[columnIndex].Value;
                    string data = Convert.ToString(obj);
                    if (string.IsNullOrEmpty(data) || string.IsNullOrWhiteSpace(data))
                    {

                    }
                    else
                    {
                        if (doorLayerColumnIndex == (columnIndex))
                        {
                            doorLayerName = data;
                        }
                        else if (doorLintelLevelColumnIndex == (columnIndex))
                        {
                            doorLintelLevel = data;
                        }
                    }
                }
                if (string.IsNullOrEmpty(doorLayerName) || string.IsNullOrWhiteSpace(doorLayerName) || string.IsNullOrEmpty(doorLintelLevel) || string.IsNullOrWhiteSpace(doorLintelLevel))
                {

                }
                else
                {
                    listOfDoorLayerName.Add(doorLayerName);
                    listOfDoorLintelLevel.Add(Convert.ToDouble(doorLintelLevel));
                }
            }
        }
        public void previousDataInDoorGridView(DataGridView dataGridViewRow)
        {
            dataGridViewRow.Rows.Clear();
            if (listOfDoorLayerName.Count != 0)
            {
                doorLayerCount = Lift_UI_Helper.getLayerIndex(listOfDoorLayerName);

                for (int index = 0; index < listOfDoorLayerName.Count; index++)
                {
                    string prvsDoorLayerName = listOfDoorLayerName[index];
                    string prvsDoorLintelLevel = Convert.ToString(listOfDoorLintelLevel[index]);
                    dataGridViewRow.Rows.Add(prvsDoorLayerName, prvsDoorLintelLevel);
                }
            }
            else
            {
                doorLayerCount = 1;
            }
        }

        public void setDefaultValueInDoorGridView(DataGridView dataGridViewRow)
        {
            dataGridViewRow.Rows.Clear();
            listOfDoorLayerName.Clear();
            listOfDoorLintelLevel.Clear();
            doorLayerCount = 1;

            //string doorLayerName1 = doorStrtText + (Convert.ToString(doorLayerCount));
            //doorLayerCount++;

            //double doorLintelLevel1 =0;
            //double doorLintelLevel2 =0;

            if (PanelLayoutInputHelper.listOfAllSelectedEntity.Count != 0)
            {
                for (int i = 0; i < PanelLayoutInputHelper.listOfAllSelectedEntity.Count; i++)
                {
                    string data = PanelLayoutInputHelper.listOfAllSelectedEntity[i];
                    if (data.Contains("Door"))
                    {


                        data = PanelLayoutInputHelper.listOfAllSelectedEntity[i + 1];
                        string[] arr = data.Split(':');
                        string beamData = arr[1];
                        string[] splitData = beamData.Split(',');
                        for (int j = 0; j < splitData.Count(); j++)
                        {
                            double doorLintelLevel = Convert.ToDouble(splitData[j]);

                            string doorLayerName = doorStrtText + (Convert.ToString(doorLayerCount));
                            dataGridViewRow.Rows.Add(doorLayerName, doorLintelLevel);

                            listOfDoorLayerName.Add(doorLayerName);
                            listOfDoorLintelLevel.Add(Convert.ToDouble(doorLintelLevel));
                            doorLayerCount++;
                        }

                        break;
                    }
                }
            }
            else
            {

                string doorLayerName1 = doorStrtText + (Convert.ToString(doorLayerCount));
                doorLayerCount++;

                double doorLintelLevel1 = 2100;
                dataGridViewRow.Rows.Add(doorLayerName1, doorLintelLevel1);

                listOfDoorLayerName.Add(doorLayerName1);
                listOfDoorLintelLevel.Add(Convert.ToDouble(doorLintelLevel1));

                string doorLayerName2 = doorStrtText + (Convert.ToString(doorLayerCount));
                doorLayerCount++;

                double doorLintelLevel2 = 1900;
                dataGridViewRow.Rows.Add(doorLayerName2, doorLintelLevel2);

                listOfDoorLayerName.Add(doorLayerName2);
                listOfDoorLintelLevel.Add(Convert.ToDouble(doorLintelLevel2));
            }
            //dataGridViewRow.Rows.Add(doorLayerName1, doorLintelLevel1);

            //listOfDoorLayerName.Add(doorLayerName1);
            //listOfDoorLintelLevel.Add(Convert.ToDouble(doorLintelLevel1));

            //string doorLayerName2 = doorStrtText + (Convert.ToString(doorLayerCount));
            //doorLayerCount++;


            //dataGridViewRow.Rows.Add(doorLayerName2, doorLintelLevel2);

            //listOfDoorLayerName.Add(doorLayerName2);
            //listOfDoorLintelLevel.Add(Convert.ToDouble(doorLintelLevel2));

            //Commented on 20/06/2023 by PRT
        }

        public static bool checkDoorLayerIsExist(string inputDoorLayer)
        {
            if (listOfDoorLayerName.Count != 0)
            {
                int index = listOfDoorLayerName.IndexOf(inputDoorLayer);

                if (index != -1)
                    return true;
            }

            return false;
        }

        public static double getDoorLintelLevel(string inputWindowLayer)
        {
            double doorLintelLevel = 0;

            if (listOfDoorLayerName.Count != 0)
            {
                int index = listOfDoorLayerName.IndexOf(inputWindowLayer);

                if (index != -1)
                    doorLintelLevel = listOfDoorLintelLevel[index];
            }

            return doorLintelLevel;
        }
    }
}
