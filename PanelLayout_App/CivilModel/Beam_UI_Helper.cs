using Autodesk.AutoCAD.DatabaseServices;
using PanelLayout_App.WallModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PanelLayout_App.CivilModel
{
    public class Beam_UI_Helper
    {
        public static List<string> listOfOffsetBeamLayerName = new List<string>();
        public static List<string> listOfOffsetBeamTop = new List<string>();
        public static List<double> listOfOffsetBeamBottom = new List<double>();

        public static int offsetBeamLayerColumnIndex = 0;
        public static int offsetBeamTopColumnIndex = 2;
        public static int offsetBeamBottomColumnIndex = 1;

        public static string offsetBeamStrtText = "AEE_OffsetBeam";
        public static int offsetBeamLayerCount = 1;
        public static int offsetBeamLayerColorIndex = 8;
        public static string defaultOffsetBeamTopValue = "Up to slab";

        public static int beamLayerColumnIndex = 0;
        public static int beamHeightColumnIndex = 1;
        public static int beamLayerCount = 1;

        public static List<string> listOfBeamLayerName = new List<string>();
        public static List<double> listOfBeamHeight = new List<double>();

        public static int beamLayerColor = 230;
        public static string beamStrtText = "AEE_Beam";
        public static string beamTextLayerName = "AEE_Beam_Text";

        public static double beamTop = 0;

        //Added on 13/03/2023 by SDM
        public static double offsetBeamBottom1 = 2700;
        public static double offsetBeamBottom2 = 2500;
        public static double beamHeight1 = 775;
        public static double beamHeight2 = 975;
        public void getDataFromOffsetBeamGridView(DataGridView dataGridViewRow)
        {
            listOfOffsetBeamLayerName.Clear();
            listOfOffsetBeamTop.Clear();
            listOfOffsetBeamBottom.Clear();

            int columnCount = dataGridViewRow.ColumnCount;
            int rowCount = dataGridViewRow.RowCount;
            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                string beamLayerName = null;
                string beamTop = null;
                string beamBottom = null;
                for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
                {
                    var obj = dataGridViewRow.Rows[rowIndex].Cells[columnIndex].Value;
                    string data = Convert.ToString(obj);
                    if (string.IsNullOrEmpty(data) || string.IsNullOrWhiteSpace(data))
                    {

                    }
                    else
                    {
                        if (offsetBeamLayerColumnIndex == (columnIndex))
                        {
                            beamLayerName = data;
                        }
                        else if (offsetBeamTopColumnIndex == (columnIndex))
                        {
                            beamTop = data;
                        }
                        else if (offsetBeamBottomColumnIndex == (columnIndex))
                        {
                            beamBottom = data;
                        }
                    }
                }
                if (string.IsNullOrEmpty(beamLayerName) || string.IsNullOrWhiteSpace(beamLayerName) || string.IsNullOrEmpty(beamTop) || string.IsNullOrWhiteSpace(beamTop) || string.IsNullOrEmpty(beamBottom) || string.IsNullOrWhiteSpace(beamBottom))
                {

                }
                else
                {
                    listOfOffsetBeamLayerName.Add(beamLayerName);
                    listOfOffsetBeamBottom.Add(Convert.ToDouble(beamBottom));
                    listOfOffsetBeamTop.Add(Convert.ToString(beamTop));
                }
            }
        }
        public void previousDataInOffsetBeamGridView(DataGridView dataGridViewRow)
        {
            dataGridViewRow.Rows.Clear();
            if (listOfOffsetBeamLayerName.Count != 0)
            {
                offsetBeamLayerCount = Lift_UI_Helper.getLayerIndex(listOfOffsetBeamLayerName);
                for (int index = 0; index < listOfOffsetBeamLayerName.Count; index++)
                {
                    string prvsBeamLayerName = listOfOffsetBeamLayerName[index];
                    string prvsBeamBottom = Convert.ToString(listOfOffsetBeamBottom[index]);
                    string prvsBeamTop = Convert.ToString(listOfOffsetBeamTop[index]);
                    dataGridViewRow.Rows.Add(prvsBeamLayerName, prvsBeamBottom, prvsBeamTop);
                }
            }
            else
            {
                offsetBeamLayerCount = 1;
            }
        }


        public void setDefaultValueInOffsetBeamGridView(DataGridView dataGridViewRow)
        {
            dataGridViewRow.Rows.Clear();
            listOfOffsetBeamLayerName.Clear();
            listOfOffsetBeamBottom.Clear();
            listOfOffsetBeamTop.Clear();
            offsetBeamLayerCount = 1;

            //string beamLayerName1 = offsetBeamStrtText + (Convert.ToString(offsetBeamLayerCount));
            //offsetBeamLayerCount++;

            string beamTop1 = defaultOffsetBeamTopValue;

            if (PanelLayoutInputHelper.listOfAllSelectedEntity.Count != 0)
            {


                for (int i = 0; i < PanelLayoutInputHelper.listOfAllSelectedEntity.Count; i++)
                {
                    string data = PanelLayoutInputHelper.listOfAllSelectedEntity[i];
                    if (data.Contains("Offset Beam"))
                    {

                        data = PanelLayoutInputHelper.listOfAllSelectedEntity[i + 1];
                        string[] arr = data.Split(':');
                        string beamData = arr[1];
                        string[] splitData = beamData.Split(',');
                        for (int j = 0; j < splitData.Count(); j++)
                        {
                            double offsetBeamBottom = Convert.ToDouble(splitData[j]);
                            string beamLayerName = offsetBeamStrtText + (Convert.ToString(offsetBeamLayerCount));
                            dataGridViewRow.Rows.Add(beamLayerName, offsetBeamBottom, beamTop1);

                            listOfOffsetBeamLayerName.Add(beamLayerName);
                            listOfOffsetBeamBottom.Add(Convert.ToDouble(offsetBeamBottom));
                            listOfOffsetBeamTop.Add(Convert.ToString(beamTop1));
                            offsetBeamLayerCount++;
                        }
                        break;
                    }
                }
            }
            else
            {
                string beamLayerName1 = offsetBeamStrtText + (Convert.ToString(offsetBeamLayerCount));
                offsetBeamLayerCount++;

                dataGridViewRow.Rows.Add(beamLayerName1, offsetBeamBottom1, beamTop1);

                listOfOffsetBeamLayerName.Add(beamLayerName1);
                listOfOffsetBeamBottom.Add(Convert.ToDouble(offsetBeamBottom1));
                listOfOffsetBeamTop.Add(Convert.ToString(beamTop1));


                string beamLayerName2 = offsetBeamStrtText + (Convert.ToString(offsetBeamLayerCount));
                offsetBeamLayerCount++;


                string beamTop2 = defaultOffsetBeamTopValue;

                dataGridViewRow.Rows.Add(beamLayerName2, offsetBeamBottom2, beamTop2);

                listOfOffsetBeamLayerName.Add(beamLayerName2);
                listOfOffsetBeamBottom.Add(Convert.ToDouble(offsetBeamBottom2));
                listOfOffsetBeamTop.Add(Convert.ToString(beamTop2));
            }


            //Changes on 20/06/2023 by PRT
        }


        public static bool checkOffsetBeamLayerIsExist(string inputBeamLayer)
        {
            if (listOfOffsetBeamLayerName.Count != 0)
            {
                int index = listOfOffsetBeamLayerName.IndexOf(inputBeamLayer);
                if (index != -1)
                {
                    return true;
                }
            }
            return false;
        }


        public static double getOffsetBeamBottom(string inputBeamLayer)
        {
            double beamBottom = 0;
            if (listOfOffsetBeamLayerName.Count != 0)
            {
                int index = listOfOffsetBeamLayerName.IndexOf(inputBeamLayer);
                if (index != -1)
                {
                    beamBottom = listOfOffsetBeamBottom[index];
                }
            }
            return beamBottom;
        }
        public static double getOffsetBeamBottom(int index)
        {
            double beamBottom = offsetBeamBottom1;
            if (listOfOffsetBeamBottom.Count != 0)
            {
                if (index != -1)
                {
                    beamBottom = listOfOffsetBeamBottom[index];
                }
            }
            return beamBottom;
        }
        public static double getOffsetBeamBottom(ObjectId beamId)
        {
            double beamBottom = 0;
            if (beamId.IsValid == true)
            {
                var beamEnt = AEE_Utility.GetEntityForRead(beamId);
                if (listOfOffsetBeamLayerName.Count != 0)
                {
                    int index = listOfOffsetBeamLayerName.IndexOf(beamEnt.Layer);
                    if (index != -1)
                    {
                        beamBottom = listOfOffsetBeamBottom[index];
                    }
                }
            }

            return beamBottom;
        }
        public static double getOffsetBeamToWallDistance(ObjectId beamId)
        {
            double disWallToBeam = 0;
            if (beamId.IsValid == true)
            {
                var beam_idx = BeamHelper.listOfLinesBtwTwoCrners_BeamInsctId.IndexOf(beamId);
                if (beam_idx != -1)
                    disWallToBeam = BeamHelper.listOfDistanceBtwWallToBeam[beam_idx];
            }

            return disWallToBeam;
        }
        public static double getBeamHeight(ObjectId beamId)
        {
            double beamBottom = 0;
            if (beamId.IsValid == true)
            {
                var beamEnt = AEE_Utility.GetEntityForRead(beamId);
                if (listOfBeamLayerName.Count != 0)
                {
                    int index = listOfBeamLayerName.IndexOf(beamEnt.Layer);
                    if (index != -1)
                    {
                        beamBottom = listOfBeamHeight[index];
                    }
                }
            }

            return beamBottom;
        }
        public static double getBeamSidePanelHeight(string beamLayer, string wallLayer)
        {
            double height = 0;
            if (checkBeamLayerIsExist(beamLayer))
            {
                var beamHt = getBeamHeight(beamLayer);
                height = beamHt - InternalWallSlab_UI_Helper.getThickness(wallLayer) - PanelLayout_UI.SL;
            }

            return height;
        }


        public void getDataFromBeamGridView(DataGridView dataGridViewRow)
        {
            listOfBeamLayerName.Clear();
            listOfBeamHeight.Clear();

            int columnCount = dataGridViewRow.ColumnCount;
            int rowCount = dataGridViewRow.RowCount;
            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                string beamLayerName = null;
                string beamHeight = null;
                for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
                {
                    var obj = dataGridViewRow.Rows[rowIndex].Cells[columnIndex].Value;
                    string data = Convert.ToString(obj);
                    if (string.IsNullOrEmpty(data) || string.IsNullOrWhiteSpace(data))
                    {

                    }
                    else
                    {
                        if (beamLayerColumnIndex == (columnIndex))
                        {
                            beamLayerName = data;
                        }
                        else if (beamHeightColumnIndex == (columnIndex))
                        {
                            beamHeight = data;
                        }
                    }
                }
                if (string.IsNullOrEmpty(beamLayerName) || string.IsNullOrWhiteSpace(beamLayerName) || string.IsNullOrEmpty(beamHeight) || string.IsNullOrWhiteSpace(beamHeight))
                {

                }
                else
                {
                    listOfBeamLayerName.Add(beamLayerName);
                    listOfBeamHeight.Add(Convert.ToDouble(beamHeight));
                }
            }
        }
        public void previousDataInBeamGridView(DataGridView dataGridViewRow)
        {
            dataGridViewRow.Rows.Clear();
            if (listOfBeamLayerName.Count != 0)
            {
                beamLayerCount = Lift_UI_Helper.getLayerIndex(listOfBeamLayerName);

                for (int index = 0; index < listOfBeamLayerName.Count; index++)
                {
                    string prvsBeamLayerName = listOfBeamLayerName[index];
                    string prvsBeamHeight = Convert.ToString(listOfBeamHeight[index]);
                    dataGridViewRow.Rows.Add(prvsBeamLayerName, prvsBeamHeight);
                }
            }
            else
            {
                beamLayerCount = 1;
            }
        }

        public void setDefaultValueInBeamGridView(DataGridView dataGridViewRow)
        {
            dataGridViewRow.Rows.Clear();
            listOfBeamLayerName.Clear();
            listOfBeamHeight.Clear();
            beamLayerCount = 1;

            if (PanelLayoutInputHelper.listOfAllSelectedEntity.Count != 0)
            {

                for (int i = 0; i < PanelLayoutInputHelper.listOfAllSelectedEntity.Count; i++)
                {
                    string data = PanelLayoutInputHelper.listOfAllSelectedEntity[i];
                    if (data.Contains("Beam"))
                    {

                        data = PanelLayoutInputHelper.listOfAllSelectedEntity[i + 1];
                        string[] arr = data.Split(':');
                        string beamData = arr[1];
                        string[] splitData = beamData.Split(',');
                        for (int j = 0; j < splitData.Count(); j++)
                        {
                            double beamHeight = Convert.ToDouble(splitData[j]);

                            string beamLayerName = beamStrtText + (Convert.ToString(beamLayerCount));
                            dataGridViewRow.Rows.Add(beamLayerName, beamHeight);

                            listOfBeamLayerName.Add(beamLayerName);
                            listOfBeamHeight.Add(Convert.ToDouble(beamHeight));
                            beamLayerCount++;
                        }
                        break;
                    }
                }
            }
            else
            {
                string beamLayerName1 = beamStrtText + (Convert.ToString(beamLayerCount));
                beamLayerCount++;


                dataGridViewRow.Rows.Add(beamLayerName1, beamHeight1);

                listOfBeamLayerName.Add(beamLayerName1);
                listOfBeamHeight.Add(Convert.ToDouble(beamHeight1));

                string beamLayerName2 = beamStrtText + (Convert.ToString(beamLayerCount));
                beamLayerCount++;

                dataGridViewRow.Rows.Add(beamLayerName2, beamHeight2);

                listOfBeamLayerName.Add(beamLayerName2);
                listOfBeamHeight.Add(Convert.ToDouble(beamHeight2));
            }
            

            //Changes made on 20/06/2023 by PRT
        }


        public static bool checkBeamLayerIsExist(string inputBeamLayer)
        {
            if (listOfBeamLayerName.Count != 0)
            {
                int index = listOfBeamLayerName.IndexOf(inputBeamLayer);

                if (index != -1)
                    return true;
            }

            return false;
        }

        public static double getBeamHeight(string beamLayer)
        {
            double beamHeight = 0;

            if (listOfBeamLayerName.Count != 0)
            {
                int index = listOfBeamLayerName.IndexOf(beamLayer);

                if (index != -1)
                    beamHeight = listOfBeamHeight[index];
            }

            return beamHeight;
        }

        public static double GetBeamDepth(ObjectId beamId, ObjectId lineId)
        {
            var lines = AEE_Utility.ExplodeEntity(beamId);
            double depth = 0;
            foreach (var l1 in lines)
            {
                var flag = WallPanelHelper.checkAngleOfBaseLine(l1, lineId);
                if (flag == true)
                {
                    var diff = AEE_Utility.GetDistanceBetweenTwoLines(l1, lineId);
                    if (depth < diff)
                        depth = diff;

                }

            }
            return depth;
        }
        public static double GetBeamLintelLevel(string beamLayer, string xdataName)
        {
            double lintel_level = 0;
            var wallLine = InternalWallHelper.GetInternalWallLineByxDataRegAppName(xdataName);
            double sunkan = 0;
            if (SunkanSlabHelper.listOfWallName_SunkanSlabId.ContainsKey(xdataName))
                sunkan = PanelLayout_UI.RC;
            lintel_level = InternalWallSlab_UI_Helper.getSlabBottom(wallLine.Layer) - (Beam_UI_Helper.getBeamHeight(beamLayer) - InternalWallSlab_UI_Helper.getThickness(wallLine.Layer)) - sunkan;
            return lintel_level;
        }
        public static double GetBeamLintelLevelWithoutSunkan(string beamLayer, string wallLayer)
        {
            double lintel_level = 0;
            lintel_level = InternalWallSlab_UI_Helper.getSlabBottom(wallLayer) - (Beam_UI_Helper.getBeamHeight(beamLayer) - InternalWallSlab_UI_Helper.getThickness(wallLayer));
            return lintel_level;
        }
    }
}
