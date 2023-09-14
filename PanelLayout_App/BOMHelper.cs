using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using PanelLayout_App.WallModule;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

namespace PanelLayout_App
{
    public class BOMHelper
    {
        //public static string appPath = "";
        //public static string saveFolderPath = "";

        //public static string bomTemplateFile = "";
        //public static string outputExcelFile = "";

        public static string symbol = "@";

        private static List<List<string>> listOfListOfWallPanelText_With_Room = new List<List<string>>();
        private static List<List<string>> listOfListOfWallPanelText_ForExist = new List<List<string>>();

        private static List<List<string>> listOfListOfWallCornerText_With_Room = new List<List<string>>();
        private static List<List<string>> listOfListOfWallCornerText_ForExist = new List<List<string>>();
        private static List<string> listOfWallRoomName_ForCorner = new List<string>();
        private static List<string> listOfWallRoomText_ForCorner = new List<string>();
        private static List<double> listOfWallRoomNumber_ForCorner = new List<double>();
        private static List<List<string>> listOfListOfWallName_ForCorner = new List<List<string>>();

        private static List<double> listOfWallRoomNumber_ForWall = new List<double>();
        private static List<string> listOfWallRoomName_ForWall = new List<string>();
        private static List<string> listOfWallRoomText_ForWall = new List<string>();
        private static List<List<string>> listOfListOfWallName_ForWall = new List<List<string>>();

        public static bool flagOfCornerData = false;

        List<List<string>> listOfListOfWallPanelText_With_Room_Sort = new List<List<string>>();
        List<List<string>> listOfListOfWallPanelText_ForExist_Sort = new List<List<string>>();

        List<List<string>> listOfListOfWallCornerText_With_Room_Sort = new List<List<string>>();
        List<List<string>> listOfListOfWallCornerText_ForExist_Sort = new List<List<string>>();


        public void clearMethodOfBOMHelper()
        {
            listOfWallRoomName_ForCorner.Clear();
            listOfWallRoomText_ForCorner.Clear();
            listOfWallRoomNumber_ForCorner.Clear();
            listOfListOfWallName_ForCorner.Clear();

            listOfWallRoomNumber_ForWall.Clear();       
            listOfWallRoomName_ForWall.Clear();
            listOfWallRoomText_ForWall.Clear();
            listOfListOfWallName_ForWall.Clear();

            listOfListOfWallPanelText_With_Room.Clear();
            listOfListOfWallPanelText_ForExist.Clear();

            listOfListOfWallCornerText_With_Room.Clear();
            listOfListOfWallCornerText_ForExist.Clear();            
        }


        public void addBOMDataOfWallPanelAndCorner(ObjectId panelOrCornrId, ObjectId panelLineId, double width, double height, double length, string itemCode, string description, string type)
        {
            var xDataRegAppName_PanelLine = AEE_Utility.GetXDataRegisterAppName(panelLineId);
            var xDataRegAppName_PanelRect = AEE_Utility.GetXDataRegisterAppName(panelOrCornrId);

            string xDataRegAppName = "";
            if (string.IsNullOrEmpty(xDataRegAppName_PanelLine) || string.IsNullOrWhiteSpace(xDataRegAppName_PanelLine))
            {
                if (string.IsNullOrEmpty(xDataRegAppName_PanelRect) || string.IsNullOrWhiteSpace(xDataRegAppName_PanelRect))
                {
                    return;
                }
                else
                {
                    xDataRegAppName = xDataRegAppName_PanelRect;
                }
            }
            else
            {
                xDataRegAppName = xDataRegAppName_PanelLine;
            }

            string[] array = xDataRegAppName.Split('_');
            string wallName = Convert.ToString(array.GetValue(0));
            string wallNameWithRoomName = xDataRegAppName;

            string roomText = "";
            string roomName = "";
            int roomNumber = 0;
            if (array.Length == 2)
            {
                wallName = "";
                wallNameWithRoomName = "";
                roomText = Convert.ToString(array.GetValue(0));
                roomNumber = Convert.ToInt32(array.GetValue(1));
                roomName = roomText + CommonModule.symbolOfUnderScore + Convert.ToString(roomNumber);
            }
            else
            {
                roomText = Convert.ToString(array.GetValue(1));
                roomNumber = Convert.ToInt32(array.GetValue(2));
                roomName = roomText + CommonModule.symbolOfUnderScore + Convert.ToString(roomNumber);
            }

            int indexOfExistRoom = -1;
            if (flagOfCornerData == false)
            {
                indexOfExistRoom = setRoomData_WallPanel(roomName, roomNumber, roomText, wallName);               
            }
            if (flagOfCornerData == true)
            {
                indexOfExistRoom = setRoomData_WallCorner(roomName, roomNumber, roomText, wallName);            
            }

            double quantity = 1;
            if (itemCode.StartsWith(CommonModule.kickerCornerText) ||
                itemCode.StartsWith(CommonModule.externalKickerCornerText))
                quantity = 2;

            addRoomPanelData(indexOfExistRoom, roomName, wallNameWithRoomName, itemCode, description,  type, width, height, length, quantity);
        }


        private int setRoomData_WallPanel(string roomName, double roomNumber, string roomText, string wallName)
        {
            int indexOfExistRoom = listOfWallRoomName_ForWall.IndexOf(roomName);
            if (indexOfExistRoom == -1)
            {
                listOfWallRoomNumber_ForWall.Add(roomNumber);
                listOfWallRoomName_ForWall.Add(roomName);
                listOfWallRoomText_ForWall.Add(roomText);

                List<string> listOfWallName = new List<string>();
                listOfWallName.Add(wallName);
                listOfListOfWallName_ForWall.Add(listOfWallName);
            }
            else
            {
                var listOfPrvsWallName = listOfListOfWallName_ForWall[indexOfExistRoom];
                int indexOfWallNameExist = listOfPrvsWallName.IndexOf(wallName);
                if (indexOfWallNameExist == -1)
                {
                    var listOfNewWallName = listOfPrvsWallName;
                    listOfNewWallName.Add(wallName);

                    listOfListOfWallName_ForWall[listOfListOfWallName_ForWall.FindIndex(x => x.Equals(listOfPrvsWallName))] = listOfNewWallName;
                }
            }

            return indexOfExistRoom;
        }


        private int setRoomData_WallCorner(string roomName, double roomNumber, string roomText, string wallName)
        {
            int indexOfExistRoom = listOfWallRoomName_ForCorner.IndexOf(roomName);
            if (indexOfExistRoom == -1)
            {
                listOfWallRoomNumber_ForCorner.Add(roomNumber);
                listOfWallRoomName_ForCorner.Add(roomName);
                listOfWallRoomText_ForCorner.Add(roomText);

                List<string> listOfWallName = new List<string>();
                listOfWallName.Add(wallName);
                listOfListOfWallName_ForCorner.Add(listOfWallName);                   
            }
            else
            {
                var listOfPrvsWallName = listOfListOfWallName_ForCorner[indexOfExistRoom];
                int indexOfWallNameExist = listOfPrvsWallName.IndexOf(wallName);
                if (indexOfWallNameExist == -1)
                {
                    var listOfNewWallName = listOfPrvsWallName;
                    listOfNewWallName.Add(wallName);

                    listOfListOfWallName_ForCorner[listOfListOfWallName_ForCorner.FindIndex(x => x.Equals(listOfPrvsWallName))] = listOfNewWallName;
                }
            }

            return indexOfExistRoom;
        }


        private void addRoomPanelData(int indexOfExistRoom, string roomName, string wallNameWithRoomName, string itemCode, string description, string type, double width, double height, double length, double quantity)
        {
            string width_Str = Convert.ToString(width);
            if (width <= 0)
            {
                width_Str = "";
            }
            string height_Str = Convert.ToString(height);
            if (height <= 0)
            {
                height_Str = "";
            }

            string length_Str = Convert.ToString(length);
            if (length <= 0)
            {
                length_Str = "";
            }
                     
            string wallData_ForCheck = roomName + symbol + wallNameWithRoomName + symbol + itemCode + symbol + description + symbol + type + symbol + width_Str + symbol + height_Str + symbol + length_Str;
            string wallDataWithQty = wallData_ForCheck + symbol + Convert.ToString(quantity);

            if (flagOfCornerData == false)
            {
                addRoomPanelDataIfExist(indexOfExistRoom, wallData_ForCheck, wallDataWithQty, quantity, symbol);
            }
            else
            {
                addRoomCornerDataIfExist(indexOfExistRoom, wallData_ForCheck, wallDataWithQty, quantity, symbol);
            }
        }


        private void addRoomPanelDataIfExist(int indexOfExistRoom, string wallData_ForCheck, string wallDataWithQty, double quantity, string symbol)
        {
            if (indexOfExistRoom != -1)
            { 
                var listOfPrvsWallPanelData_ForExist = listOfListOfWallPanelText_ForExist[indexOfExistRoom];
                var listtOfPrvsWallPanelText_With_Room = listOfListOfWallPanelText_With_Room[indexOfExistRoom];

                int indexOfExistWallData = listOfPrvsWallPanelData_ForExist.IndexOf(wallData_ForCheck);
                if (indexOfExistWallData != -1)
                {
                    string prvsWallDataWithQty = listtOfPrvsWallPanelText_With_Room[indexOfExistWallData];
                    string[] array = prvsWallDataWithQty.Split('@');
                    int prvsQty = Convert.ToInt32(array.GetValue(array.Length - 1));
                    double totalQty = prvsQty + quantity;

                    string newItem = wallData_ForCheck + symbol + Convert.ToString(totalQty);

                    List<string> listtOfNewWallPanelText_With_Room = new List<string>();
                    for (int i = 0; i < listtOfPrvsWallPanelText_With_Room.Count; i++)
                    {
                        if (i == indexOfExistWallData)
                        {
                            listtOfNewWallPanelText_With_Room.Add(newItem);
                        }
                        else
                        {
                            listtOfNewWallPanelText_With_Room.Add(listtOfPrvsWallPanelText_With_Room[i]);
                        }
                    }
                    listOfListOfWallPanelText_With_Room[listOfListOfWallPanelText_With_Room.FindIndex(ind => ind.Equals(listtOfPrvsWallPanelText_With_Room))] = listtOfNewWallPanelText_With_Room;
                }
                else
                {
                    var listOfNewWallPanelData_ForExist = listOfPrvsWallPanelData_ForExist;
                    listOfNewWallPanelData_ForExist.Add(wallData_ForCheck);

                    var listOfNewWallPanelText_With_Room = listtOfPrvsWallPanelText_With_Room;
                    listOfNewWallPanelText_With_Room.Add(wallDataWithQty);

                    listOfListOfWallPanelText_With_Room[listOfListOfWallPanelText_With_Room.FindIndex(ind => ind.Equals(listtOfPrvsWallPanelText_With_Room))] = listOfNewWallPanelText_With_Room;

                    listOfListOfWallPanelText_ForExist[listOfListOfWallPanelText_ForExist.FindIndex(ind => ind.Equals(listOfPrvsWallPanelData_ForExist))] = listOfNewWallPanelData_ForExist;
                }
            }
            else
            {
                List<string> listOfBomData = new List<string>();
                listOfBomData.Add(wallDataWithQty);

                List<string> listOfBomData_ForCheck = new List<string>();
                listOfBomData_ForCheck.Add(wallData_ForCheck);

                listOfListOfWallPanelText_With_Room.Add(listOfBomData);
                listOfListOfWallPanelText_ForExist.Add(listOfBomData_ForCheck);
            }        
        }


        private void addRoomCornerDataIfExist(int indexOfExistRoom, string cornerData_ForCheck, string wallDataWithQty, double quantity, string symbol)
        {
            if (indexOfExistRoom != -1)
            {
                var listOfPrvsWallCornerData_ForExist = listOfListOfWallCornerText_ForExist[indexOfExistRoom];
                var listtOfPrvsWallCornerText_With_Room = listOfListOfWallCornerText_With_Room[indexOfExistRoom];

                int indexOfExistCornerData = listOfPrvsWallCornerData_ForExist.IndexOf(cornerData_ForCheck);
                if (indexOfExistCornerData != -1)
                {
                    string prvsCornerDataWithQty = listtOfPrvsWallCornerText_With_Room[indexOfExistCornerData];
                    string[] array = prvsCornerDataWithQty.Split('@');
                    int prvsQty = Convert.ToInt32(array.GetValue(array.Length - 1));
                    double totalQty = prvsQty + quantity;

                    string newItem = cornerData_ForCheck + symbol + Convert.ToString(totalQty);

                    List<string> listtOfNewWallCornerText_With_Room = new List<string>();
                    for (int i = 0; i < listtOfPrvsWallCornerText_With_Room.Count; i++)
                    {
                        if (i == indexOfExistCornerData)
                        {
                            listtOfNewWallCornerText_With_Room.Add(newItem);
                        }
                        else
                        {
                            listtOfNewWallCornerText_With_Room.Add(listtOfPrvsWallCornerText_With_Room[i]);
                        }
                    }
                    listOfListOfWallCornerText_With_Room[listOfListOfWallCornerText_With_Room.FindIndex(ind => ind.Equals(listtOfPrvsWallCornerText_With_Room))] = listtOfNewWallCornerText_With_Room;
                }
                else
                {
                    var listOfNewWallCornerData_ForExist = listOfPrvsWallCornerData_ForExist;
                    listOfNewWallCornerData_ForExist.Add(cornerData_ForCheck);

                    var listOfNewWallCornerText_With_Room = listtOfPrvsWallCornerText_With_Room;
                    listOfNewWallCornerText_With_Room.Add(wallDataWithQty);

                    listOfListOfWallCornerText_With_Room[listOfListOfWallCornerText_With_Room.FindIndex(ind => ind.Equals(listtOfPrvsWallCornerText_With_Room))] = listOfNewWallCornerText_With_Room;

                    listOfListOfWallCornerText_ForExist[listOfListOfWallCornerText_ForExist.FindIndex(ind => ind.Equals(listOfPrvsWallCornerData_ForExist))] = listOfNewWallCornerData_ForExist;
                }
            }
            else
            {
                List<string> listOfBomData = new List<string>();
                listOfBomData.Add(wallDataWithQty);

                List<string> listOfBomData_ForCheck = new List<string>();
                listOfBomData_ForCheck.Add(cornerData_ForCheck);

                listOfListOfWallCornerText_With_Room.Add(listOfBomData);
                listOfListOfWallCornerText_ForExist.Add(listOfBomData_ForCheck);
            }
        }


        private void sortDataAsPerRoomNumber(string symbol)
        {
            listOfListOfWallPanelText_With_Room_Sort.Clear();
            listOfListOfWallPanelText_ForExist_Sort.Clear();
            listOfListOfWallCornerText_With_Room_Sort.Clear();
            listOfListOfWallCornerText_ForExist_Sort.Clear();

            var listOfSortWallRoomName = sortRoomNameWithIndex();

            if (listOfSortWallRoomName.Count == 0)
            {
                return;
            }

            List<List<string>> listOfListOfWallName_Sort = new List<List<string>>();

            for (int index = 0; index < listOfSortWallRoomName.Count; index++)
            {
                List<string> listOfWallName_ForWall = new List<string>();

                string roomName_Sort = listOfSortWallRoomName[index];
                int indexOfRoomExist_In_WallPanel = listOfWallRoomName_ForWall.IndexOf(roomName_Sort);
                if (indexOfRoomExist_In_WallPanel != -1)
                {
                    listOfListOfWallPanelText_With_Room_Sort.Add(listOfListOfWallPanelText_With_Room[indexOfRoomExist_In_WallPanel]);
                    listOfListOfWallPanelText_ForExist_Sort.Add(listOfListOfWallPanelText_ForExist[indexOfRoomExist_In_WallPanel]);

                    listOfWallName_ForWall = listOfListOfWallName_ForWall[indexOfRoomExist_In_WallPanel];
                }

                List<string> listOfWallName_ForCorner = new List<string>();

                int indexOfRoomExist_In_WallCorner = listOfWallRoomName_ForCorner.IndexOf(roomName_Sort);
                if (indexOfRoomExist_In_WallCorner != -1)
                {
                    listOfListOfWallCornerText_With_Room_Sort.Add(listOfListOfWallCornerText_With_Room[indexOfRoomExist_In_WallCorner]);
                    listOfListOfWallCornerText_ForExist_Sort.Add(listOfListOfWallCornerText_ForExist[indexOfRoomExist_In_WallCorner]);

                    listOfWallName_ForCorner = listOfListOfWallName_ForCorner[indexOfRoomExist_In_WallCorner];
                }

                var listOfAllWallName = sortWallNameWithInndex(listOfWallName_ForWall, listOfWallName_ForCorner);
                listOfListOfWallName_Sort.Add(listOfAllWallName);
            }

            sortRoomDataWithWallIndex(listOfListOfWallName_Sort, listOfSortWallRoomName);
        }


        private List<string> sortRoomNameWithIndex()
        {
            List<string> listOfRoomName_With_Sort = new List<string>();

            var listOfWallRoomNumber = listOfWallRoomNumber_ForWall.Distinct().ToList();
            listOfWallRoomNumber = listOfWallRoomNumber.OrderBy(x => x).ToList();

            var listOfWallRoomText = listOfWallRoomText_ForWall.Distinct().ToList();
            listOfWallRoomText = listOfWallRoomText.OrderBy(x => x).ToList();

            for (int i = 0; i < listOfWallRoomText.Count; i++)
            {
                string roomText = listOfWallRoomText[i];
                for (int j = 0; j < listOfWallRoomNumber.Count; j++)
                {
                    double roomNumber = listOfWallRoomNumber[j];
                    string roomName = roomText + CommonModule.symbolOfUnderScore + Convert.ToString(roomNumber);

                    int indexOfRoomExist = listOfWallRoomName_ForWall.IndexOf(roomName);
                    if (indexOfRoomExist != -1 && !listOfRoomName_With_Sort.Contains(roomName))
                    {
                        listOfRoomName_With_Sort.Add(roomName);
                    }
                }
            }

            return listOfRoomName_With_Sort;
        }


        private List<string> sortWallNameWithInndex(List<string> listOfWallName_ForWall, List<string> listOfWallName_ForCorner)
        {
            List<string> listOfOutputWallName = new List<string>();

            List<string> listOfNewWallName = listOfWallName_ForWall;
            foreach (var cornerWallName in listOfWallName_ForCorner)
            {
                listOfNewWallName.Add(cornerWallName);
            }
            listOfNewWallName = listOfNewWallName.Distinct().ToList();

            List<int> listOfWallNumber = new List<int>();
            foreach (var wallName in listOfNewWallName)
            {
                if (wallName != "")
                {
                    var resultString = Regex.Match(wallName, @"\d+").Value;
                    var wallNumber = Int32.Parse(resultString);
                    listOfWallNumber.Add(Convert.ToInt32(wallNumber));
                }              
            }

            var listOfWallNumber_Sort = listOfWallNumber.OrderBy(x => x).ToList();
            foreach (var wallNumber in listOfWallNumber_Sort)
            {
                int indexOfWallNumber = listOfWallNumber.IndexOf(wallNumber);
                if (indexOfWallNumber != -1)
                {
                    listOfOutputWallName.Add(listOfNewWallName[indexOfWallNumber]);
                }
            }

            return listOfOutputWallName;
        }


        private void sortRoomDataWithWallIndex(List<List<string>> listOfListOfWallName_Sort, List<string> listOfSortWallRoomName)
        {
            listOfListOfWallPanelText_With_Room.Clear();
            listOfListOfWallPanelText_ForExist.Clear();

            for (int index = 0; index < listOfListOfWallName_Sort.Count; index++)
            {                
                var listOfWallPanelText_With_Room_Sort = listOfListOfWallPanelText_With_Room_Sort[index];
                var listOfWallPanelText_ForExist_Sort = listOfListOfWallPanelText_ForExist_Sort[index];

                List<string> listOfWallPanelData = new List<string>();
                List<string> listOfWallPanelText_ForCheck = new List<string>();

                var listOfAllWallName = listOfListOfWallName_Sort[index];
                string roomName = listOfSortWallRoomName[index];
                for (int k = 0; k < listOfAllWallName.Count; k++)
                {
                    string wallName = listOfAllWallName[k];
                    string wallName_With_RoomName = wallName + CommonModule.symbolOfUnderScore + roomName;

                    for (int m = 0; m < listOfWallPanelText_With_Room_Sort.Count; m++)
                    {
                        string data = listOfWallPanelText_With_Room_Sort[m];
                        string[] array = data.Split('@');
                        string wallNameWithRoomName = Convert.ToString(array.GetValue(1));
                        if (wallNameWithRoomName == wallName_With_RoomName && !listOfWallPanelData.Contains(data))
                        {
                            listOfWallPanelData.Add(data);
                            listOfWallPanelText_ForCheck.Add(listOfWallPanelText_ForExist_Sort[m]);
                        }
                    }

                    var listOfWallCornerText_With_Room_Sort = listOfListOfWallCornerText_With_Room_Sort[index];
                    var listOfWallCornerText_ForExist_Sort = listOfListOfWallCornerText_ForExist_Sort[index];

                    List<string> listOfWallCornerData = new List<string>();
                    List<string> listOfWallCornerText_ForCheck = new List<string>();

                    for (int m = 0; m < listOfWallCornerText_With_Room_Sort.Count; m++)
                    {
                        string data = listOfWallCornerText_With_Room_Sort[m];
                        string[] array = data.Split('@');
                        string wallNameWithRoomName = Convert.ToString(array.GetValue(1));
                        if (wallNameWithRoomName == wallName_With_RoomName && !listOfWallCornerData.Contains(data))
                        {
                            listOfWallCornerData.Add(data);
                            listOfWallCornerText_ForCheck.Add(listOfWallCornerText_ForExist_Sort[m]);
                        }
                    }

                    for (int p = 0; p < listOfWallCornerData.Count; p++)
                    {
                        listOfWallPanelData.Add(listOfWallCornerData[p]);
                        listOfWallPanelText_ForCheck.Add(listOfWallCornerText_ForCheck[p]);
                    }
                }

                List<string> listOfRemainingWallPanelData = listOfWallPanelText_With_Room_Sort.Where(i => !listOfWallPanelData.Contains(i)).ToList();
                foreach (var data in listOfRemainingWallPanelData)
                {
                    listOfWallPanelData.Add(data);
                }

                List<string> listOfRemainingWallPanelData_Exist = listOfWallPanelText_ForExist_Sort.Where(i => !listOfWallPanelText_ForCheck.Contains(i)).ToList();
                foreach (var data in listOfRemainingWallPanelData_Exist)
                {
                    listOfWallPanelText_ForCheck.Add(data);
                }

                listOfListOfWallPanelText_With_Room.Add(listOfWallPanelData);
                listOfListOfWallPanelText_ForExist.Add(listOfWallPanelText_ForCheck);
            }
        }


        public void callMethodOfBOMExport()
        {
            if (listOfListOfWallPanelText_With_Room.Count == 0)
            {
                string emptyListMsg = "Empty data found";
                MessageBox.Show(emptyListMsg, CommonModule.headerText_messageBox, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }        

            string bomCreatorMsg = "BOM in progress.....";

            ProgressForm progressForm = new ProgressForm();
            progressForm.Show();

            progressForm.ReportProgress(1, bomCreatorMsg);
    
            sortDataAsPerRoomNumber(symbol);                 

            List<string> headerList = new List<string>();

            headerList.Add("Srl No.");
            headerList.Add("Room");
            headerList.Add("Wall");
            headerList.Add("Item Code");
            headerList.Add("Description");
            headerList.Add("Type");
            headerList.Add("Width");
            headerList.Add("Height");
            headerList.Add("Length");
            headerList.Add("Quantity");

            progressForm.ReportProgress(1, bomCreatorMsg);

            excelExport(progressForm, bomCreatorMsg, headerList, listOfListOfWallPanelText_With_Room);
        }


        private void excelExport(ProgressForm progressForm, string bomCreatorMsg, List<string> headerList, List<List<string>> listOfListOfWriteData)
        {
            Microsoft.Office.Interop.Excel.Application xlApp = new Microsoft.Office.Interop.Excel.Application();

            if (xlApp == null)
            {
                //just to check if we get hold of the excel aplication  
                return;
            }
            try
            {
                Excel.Workbook xlWorkBook;
                Excel.Worksheet xlWorkSheet;
                object misValue = System.Reflection.Missing.Value;

                xlWorkBook = xlApp.Workbooks.Add(misValue);
                xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);
                xlWorkSheet.Name = "PanelLayout";

                progressForm.ReportProgress(1, bomCreatorMsg);

                xlWorkSheet.Cells.Style.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;

                writeDataInExcel(progressForm, bomCreatorMsg, xlWorkSheet, headerList, listOfListOfWriteData);

                xlApp.Visible = true;

                progressForm.Close();
                MessageBox.Show("Excel file is opened", CommonModule.headerText_messageBox, MessageBoxButtons.OK, MessageBoxIcon.Information);

                //xlWorkBook.SaveAs("E:\\csharp-Excel.xls", XlFileFormat.xlWorkbookNormal, misValue, misValue, misValue, misValue, XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);
                //xlWorkBook.Close(true, misValue, misValue);      
                //xlApp.Quit();

                releaseObject(xlWorkSheet);
                releaseObject(xlWorkBook);
                releaseObject(xlApp);

            }
            catch
            {
                progressForm.Close();
                MessageBox.Show("Unable to open excel file", CommonModule.headerText_messageBox, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }


        private Excel.Worksheet writeDataInExcel(ProgressForm progressForm, string bomCreatorMsg, Excel.Worksheet xlWorkSheet, List<string> headerList, List<List<string>> listOfListOfWriteData)
        {
            int srlNumber = 1;

            int strRowIndex = 1;
            int rowIndex = strRowIndex;
            for (int index = 0; index < headerList.Count; index++)
            {
                string headerText = headerList[index];
                int colIndex = index + 1;
                xlWorkSheet.Cells[rowIndex, colIndex].Font.Size = 30;
                xlWorkSheet.Cells[rowIndex, colIndex].Font.Bold = true;
                xlWorkSheet.Cells[rowIndex, colIndex].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Gray);

                xlWorkSheet.Cells[rowIndex, colIndex] = headerText;
            }

            rowIndex++;

            for (int index = 0; index < listOfListOfWriteData.Count; index++)
            {
                if (index % 50 == 0)
                {
                    progressForm.ReportProgress(1, bomCreatorMsg);
                }               

                var listOfWriteData = listOfListOfWriteData[index];
                for (int k = 0; k < listOfWriteData.Count; k++)
                {
                    string data = listOfWriteData[k];
                    string[] dataArray = data.Split('@');

                    int srlColIndex = 1;
                    for (int i = 0; i < dataArray.Length; i++)
                    {
                        if (i == 0)
                        {
                            xlWorkSheet.Cells[rowIndex, srlColIndex] = Convert.ToString(srlNumber);
                            srlColIndex++;
                            srlNumber++;
                        }
                        int colIndex = i + srlColIndex;
                        string text = Convert.ToString(dataArray.GetValue(i));
                        xlWorkSheet.Cells[rowIndex, colIndex] = text;
                    }
                    rowIndex++;
                }           
            }
            return xlWorkSheet;
        }


        private void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                obj = null;
            }
            finally
            {
                GC.Collect();
            }
        }


        //private void ExportBOMFile(ProgressForm progressForm, string bomCreatorMsg, string bomTemplateFile, string outputExcelFile)
        //{
        //    if (File.Exists(bomTemplateFile))
        //    {
        //        try
        //        {
        //            if (File.Exists(outputExcelFile))
        //            {
        //                File.Delete(outputExcelFile);
        //            }
        //            Excel.Application xlApp = new Microsoft.Office.Interop.Excel.Application();
        //            if (xlApp == null)
        //            {
        //                MessageBox.Show("Excel is not properly installed!!");
        //                return;
        //            }
        //            Excel.Workbook xlWorkBook;
        //            Excel.Worksheet xlWorkSheet;
        //            object missing = Type.Missing;
        //            object misValue = System.Reflection.Missing.Value;

        //            xlWorkBook = xlApp.Workbooks.Open(bomTemplateFile, 0, true, 5, "", "", true, Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);

        //            xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);

        //            string sheetName = xlWorkSheet.Name;

        //            Excel.Range range = xlWorkSheet.UsedRange;
        //            writeDataInExcel(progressForm, bomCreatorMsg, xlWorkSheet, listOfWallPanelTextData_ForBOM);

        //            ////xlWorkSheet.Protect(MasterBomHelper.password, missing, true, missing, missing, missing, missing, missing, missing, missing, missing, missing, missing, missing, missing, missing);

        //            //Here saving the file in xlsx
        //            xlWorkBook.SaveAs(outputExcelFile, Microsoft.Office.Interop.Excel.XlFileFormat.xlOpenXMLWorkbook, misValue,
        //            misValue, misValue, misValue, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, misValue,
        //            misValue, misValue, misValue, misValue);

        //            xlWorkBook.Close(true, misValue, misValue);

        //            xlApp.Quit();
        //            Marshal.ReleaseComObject(xlWorkSheet);
        //            Marshal.ReleaseComObject(xlWorkBook);
        //            Marshal.ReleaseComObject(xlApp);

        //            progressForm.ReportProgress(100, "BOM file created");
        //            progressForm.Close();
        //            string msg = "BOM file created in " + outputExcelFile;
        //            MessageBox.Show(msg, CommonModule.headerText_messageBox, MessageBoxButtons.OK, MessageBoxIcon.Information);
        //        }
        //        catch (Exception ex)
        //        {
        //            string msg = "Unable to create BOM file";
        //            MessageBox.Show(msg, CommonModule.headerText_messageBox, MessageBoxButtons.OK, MessageBoxIcon.Information);

        //            progressForm.Close();
        //        }
        //    }
        //    else
        //    {
        //        MessageBox.Show("BOM template file does not exist", CommonModule.headerText_messageBox, MessageBoxButtons.OK, MessageBoxIcon.Information);
        //        progressForm.Close();
        //    }
        //}
        //private Excel.Worksheet writeDataInExcel(ProgressForm progressForm, string bomCreatorMsg, Excel.Worksheet xlWorkSheet, List<string> listOfWriteData)
        //{
        //    int strRowIndex = 3;
        //    for (int index = 0; index < listOfWriteData.Count; index++)
        //    {
        //        if (index % 100 == 0)
        //        {
        //            progressForm.ReportProgress(1, bomCreatorMsg);
        //        }
        //        int rowIndex = strRowIndex + index;
        //        string data = listOfWriteData[index];
        //        string[] dataArray = data.Split('@');

        //        for (int i = 0; i < dataArray.Length; i++)
        //        {
        //            int colIndex = i + 1;
        //            string text = Convert.ToString(dataArray.GetValue(i));
        //            xlWorkSheet.Cells[rowIndex, colIndex] = text;
        //        }
        //    }
        //    return xlWorkSheet;
        //}

        //public bool checkExcelFileIsNotOpen()
        //{
        //    bool flag = false;
        //    string pluginPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        //    string appDebugPath = Path.GetDirectoryName(pluginPath);

        //    string saveFolderPath = createSaveFileFolder();

        //    string bomTemplateFile = appDebugPath + "Panel_BOM_Temp" + ".xlsx";
        //    string outputExcelFile = saveFolderPath + "Panel_BOM_Output" + ".xlsx";

        //    if (!File.Exists(bomTemplateFile))
        //    {
        //        MessageBox.Show("BOM template file does not exist", CommonModule.headerText_messageBox, MessageBoxButtons.OK, MessageBoxIcon.Information);
        //        return flag;
        //    }
        //    bool flagOfFileOverwrite = checkFileIsExist(outputExcelFile);
        //    if (flagOfFileOverwrite == true)
        //    {
        //        bool flagOfFileInUse = checkFileIsInUse(outputExcelFile);
        //        if (flagOfFileInUse == false)
        //        {
        //            flag = true;
        //        }
        //    }
        //    return flag;
        //}
        //private bool checkFileIsExist(string fileName)
        //{
        //    bool flag = false;

        //    List<string> fileNameList = new List<string>();
        //    fileNameList.Add(fileName);

        //    foreach (string path in fileNameList)
        //    {
        //        if (File.Exists(path))
        //        {
        //            DialogResult dr;
        //            string fileExistMsg = path + " already exists.\nDo you want to overwrite it.";
        //            dr = MessageBox.Show(fileExistMsg, CommonModule.headerText_messageBox, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        //            if (dr == DialogResult.Yes)
        //            {
        //                flag = true;
        //            }
        //            else
        //            {
        //                flag = false;
        //            }
        //            break;
        //        }
        //        else
        //        {
        //            flag = true;
        //        }
        //    }
        //    return flag;
        //}
        //private bool checkFileIsInUse(string fileName)
        //{
        //    bool flag = true;

        //    bool flagOfExcelFile = FileInUse(fileName);
        //    if (flagOfExcelFile == false)
        //    {
        //        File.Delete(fileName);
        //    }
        //    if (flagOfExcelFile == false)
        //    {
        //        flag = false;
        //    }
        //    else
        //    {
        //        string msg = "File is in use, close the file \n" + fileName;
        //        MessageBox.Show(msg, CommonModule.headerText_messageBox, MessageBoxButtons.OK, MessageBoxIcon.Information);
        //    }
        //    return flag;
        //}
        //private bool FileInUse(string path)
        //{
        //    try
        //    {
        //        using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
        //        {

        //        }
        //        return false;
        //    }
        //    catch (IOException ex)
        //    {
        //        return true;
        //    }
        //}       
        //private string createSaveFileFolder()
        //{
        //    string saveFolderPath = "C:\\A & E Enterprises\\PanelLayout_App\\";

        //    // If directory does not exist, create it.

        //    if (!Directory.Exists(saveFolderPath))
        //    {
        //        Directory.CreateDirectory(saveFolderPath);
        //    }
        //    return saveFolderPath;
        //}
    }
}
