using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using PanelLayout_App.CivilModel;
using System;
using System.Collections.Generic;
using System.Linq;
using static PanelLayout_App.RotationHelper;
using PanelLayout_App.ElevationModule;
using Autodesk.AutoCAD.MacroRecorder;
using System.Net;
using static PanelLayout_App.AEE_Utility;


namespace PanelLayout_App.WallModule
{
    public class WindowHelper
    {
        public static double windowOffsetValue = 10;

        public static List<ObjectId> listOfWindowObjId = new List<ObjectId>();
        public static List<string> listOfWindowObjId_Str = new List<string>();

        private static List<ObjectId> listOfIntersectWindowObjId = new List<ObjectId>();
        private static List<List<Point3d>> listOfListIntersectWindow_Points = new List<List<Point3d>>();

        private static List<List<ObjectId>> listOfListIntersectWindow_LineId = new List<List<ObjectId>>();
        private static List<ObjectId> listOfIntersectWindowId_ForTrimLineId = new List<ObjectId>();
        List<LCX_CCX_mid_CLine> LCX_CCX_mid_CLine = new List<ElevationModule.LCX_CCX_mid_CLine>();

        public static List<ObjectId> listOfWindowPanelLine_ObjId = new List<ObjectId>();
        public static List<string> listOfWindowPanelLine_ObjId_InStr = new List<string>();
        public static List<ObjectId> listOfWindowPanelOffsetLine_ObjId = new List<ObjectId>();

        private static List<string> listOfExistWindowPanelLineObj = new List<string>();
        private static List<string> listOfWindowObjId_With_WindowPanelLine = new List<string>();
        private static List<ObjectId> listOfWindowObjId_With_WindowLine = new List<ObjectId>();

        private static List<ObjectId> listOfWndowId_NotInsctToCorner = new List<ObjectId>();
        private static List<List<ObjectId>> listOfListOfWndowCornerId_NotInsctToWallCorner = new List<List<ObjectId>>();
        private static List<List<List<ObjectId>>> listOfListOfWndowCornerTextId_NotInsctToWallCorner = new List<List<List<ObjectId>>>();

        public static bool flagOfChajja = false;

        private enum windowPanelSide
        {
            TopAndLeft = 1,
            BottomAndRight = 2,
        }
        public void createNewWindowAsPerNewShellPlan(ProgressForm progressForm, string progressBarMsg)
        {
            listOfListIntersectWindow_LineId.Clear();
            listOfIntersectWindowId_ForTrimLineId.Clear();
            listOfWindowObjId_With_WindowPanelLine.Clear();
            listOfWindowObjId_With_WindowLine.Clear();
            listOfExistWindowPanelLineObj.Clear();
            listOfWndowId_NotInsctToCorner.Clear();
            listOfListOfWndowCornerId_NotInsctToWallCorner.Clear();
            listOfListOfWndowCornerTextId_NotInsctToWallCorner.Clear();

            progressForm.ReportProgress(1, progressBarMsg);
            if (CheckShellPlanHelper.listOfSelectedWindow_ObjId.Count != 0)
            {
                listOfIntersectWindowObjId.Clear();
                listOfListIntersectWindow_Points.Clear();

                for (int index = 0; index < CheckShellPlanHelper.listOfSelectedWindow_ObjId.Count; index++)
                {
                    ObjectId windowId = CheckShellPlanHelper.listOfSelectedWindow_ObjId[index];
                    if ((index % 10) == 0)
                    {
                        progressForm.ReportProgress(1, progressBarMsg);
                    }

                    ObjectId newWindowId = createNewWindow_ForIntersectWall_Check(windowId);
                    getIntersectPoint(windowId, newWindowId, InternalWallHelper.listOfInternalWallObjId);
                    getIntersectPoint(windowId, newWindowId, ExternalWallHelper.listOfExternalWallObjId);
                    getIntersectPoint(windowId, newWindowId, DuctHelper.listOfDuctObjId);
                    getIntersectPoint(windowId, newWindowId, StairCaseHelper.listOfStairCaseObjId);
                    getIntersectPoint(windowId, newWindowId, LiftHelper.listOfLiftObjId);

                    AEE_Utility.deleteEntity(newWindowId);
                }
                drawNewWindowAsPerNewShellPlan();

                AEE_Utility.deleteEntity(listOfIntersectWindowObjId);

                AEE_Utility.deleteEntity(CheckShellPlanHelper.listOfSelectedWindow_ObjId);
            }
        }

        private void getIntersectPoint(ObjectId orginalWindowId, ObjectId newWindowId, List<ObjectId> listOfWallIds)
        {
            foreach (var wallId in listOfWallIds)
            {
                var listOfWallExplodeIds = AEE_Utility.ExplodeEntity(wallId);
                for (int index = 0; index < listOfWallExplodeIds.Count; index++)
                {
                    ObjectId explodeWallId = listOfWallExplodeIds[index];
                    List<ObjectId> listOfTrimLineId = AEE_Utility.TrimBetweenTwoClosedPolylines(newWindowId, explodeWallId);
                    if (listOfTrimLineId.Count == 0)
                    {
                        Point3d midPointOfWall = AEE_Utility.GetMidPoint(explodeWallId);
                        var flagOfInside = AEE_Utility.GetPointIsInsideThePolyline(newWindowId, midPointOfWall);
                        if (flagOfInside == true)
                        {
                            listOfTrimLineId.Add(explodeWallId);
                        }
                    }

                    if (listOfTrimLineId.Count != 0)
                    {
                        ObjectId trimLineId = listOfTrimLineId[0];

                        List<double> listOfTrimStrtEndPoint = AEE_Utility.getStartEndPointWithAngle_Line(trimLineId);
                        var flagOfWindowLineAngle = checkMaxLengthWindowLineSegmentAngle(orginalWindowId, trimLineId);
                        if (flagOfWindowLineAngle == true)
                        {
                            //var intersectLineId = AEE_Utility.CreateLine(listOfTrimStrtEndPoint[0], listOfTrimStrtEndPoint[1], 0, listOfTrimStrtEndPoint[2], listOfTrimStrtEndPoint[3], 0, "A", 3);
                            setIntersectWindowPoints(orginalWindowId, trimLineId);
                        }
                        ////AEE_Utility.deleteEntity(listOfTrimLineId);
                    }
                }
                ////AEE_Utility.deleteEntity(listOfWallExplodeIds);
            }
        }


        private void setIntersectWindowPoints(ObjectId orginalWindowId, ObjectId trimLineId)
        {
            var trimLineLength = AEE_Utility.GetLengthOfLine(trimLineId);
            if (trimLineLength >= 1)
            {
                List<Point3d> listOfTrimLineStrtEndPoint = AEE_Utility.GetStartEndPointOfLine(trimLineId);

                if (listOfIntersectWindowObjId.Contains(orginalWindowId))
                {
                    int indexOfExistId = listOfIntersectWindowObjId.IndexOf(orginalWindowId);
                    List<Point3d> listOfPrvsTrimLinePoint = listOfListIntersectWindow_Points[indexOfExistId];
                    List<Point3d> listOfAllTrimLinePoint = new List<Point3d>();
                    foreach (var point in listOfPrvsTrimLinePoint)
                    {
                        listOfAllTrimLinePoint.Add(point);
                    }
                    foreach (var point in listOfTrimLineStrtEndPoint)
                    {
                        listOfAllTrimLinePoint.Add(point);
                    }
                    listOfListIntersectWindow_Points[listOfListIntersectWindow_Points.FindIndex(ind => ind.Equals(listOfPrvsTrimLinePoint))] = listOfAllTrimLinePoint;
                }
                else
                {
                    listOfIntersectWindowObjId.Add(orginalWindowId);
                    listOfListIntersectWindow_Points.Add(listOfTrimLineStrtEndPoint);
                }
            }

            setIntersectWindowTrimLine(orginalWindowId, trimLineId);
        }

        private void setIntersectWindowTrimLine(ObjectId orginalWindowId, ObjectId trimLineId)
        {
            List<ObjectId> listOfTrimId = new List<ObjectId>();
            listOfTrimId.Add(trimLineId);

            if (listOfIntersectWindowId_ForTrimLineId.Contains(orginalWindowId))
            {
                int indexOfExistId = listOfIntersectWindowId_ForTrimLineId.IndexOf(orginalWindowId);

                List<ObjectId> listOfPrvsTrimLineId = listOfListIntersectWindow_LineId[indexOfExistId];
                foreach (var prvsTrimLineId in listOfPrvsTrimLineId)
                {
                    listOfTrimId.Add(prvsTrimLineId);
                }
                listOfListIntersectWindow_LineId[listOfListIntersectWindow_LineId.FindIndex(ind => ind.Equals(listOfPrvsTrimLineId))] = listOfTrimId;
            }
            else
            {
                listOfIntersectWindowId_ForTrimLineId.Add(orginalWindowId);
                listOfListIntersectWindow_LineId.Add(listOfTrimId);
            }
        }

        internal static Dictionary<Point3d, Tuple<double, bool>> FindCorners()
        {
            List<List<ObjectId>> listOfListDoorPanelLines_ObjId = new List<List<ObjectId>>();
            List<List<ObjectId>> listOfListDoorPanelOffsetLines_ObjId = new List<List<ObjectId>>();
            List<ObjectId> listOfDoorObjId_InsctWall = new List<ObjectId>();
            List<ObjectId> listOfParapetId_WithDoorWallLine = new List<ObjectId>();

            DoorHelper.getDoorPanelsLines(out listOfListDoorPanelLines_ObjId, out listOfListDoorPanelOffsetLines_ObjId, out listOfDoorObjId_InsctWall, out listOfParapetId_WithDoorWallLine);

            List<List<ObjectId>> listOfListWindowPanelLines_ObjId = new List<List<ObjectId>>();
            List<List<ObjectId>> listOfListWindowPanelOffsetLines_ObjId = new List<List<ObjectId>>();
            List<ObjectId> listOfWindowObjId_InsctWall = new List<ObjectId>();
            List<ObjectId> listOfParapetId_WithWindowWallLine = new List<ObjectId>();
            getWindowPanelsLines(out listOfListWindowPanelLines_ObjId, out listOfListWindowPanelOffsetLines_ObjId, out listOfWindowObjId_InsctWall, out listOfParapetId_WithWindowWallLine);

            List<ObjectId> lstObj = new List<ObjectId>();
            lstObj.AddRange(listOfDoorObjId_InsctWall);
            lstObj.AddRange(listOfWindowObjId_InsctWall);
            Dictionary<Point3d, List<Tuple<ObjectId, double>>> dic = new Dictionary<Point3d, List<Tuple<ObjectId, double>>>(new PointComparer());
            foreach (ObjectId obj in lstObj)
            {
                var pts = AEE_Utility.GetPolyLinePointList(obj);
                var minLength = -1.0;
                for (int n = 1; n < pts.Count; ++n)
                {
                    var dist = pts[n].DistanceTo(pts[n - 1]);
                    if (minLength < 0 || minLength > dist)
                    {
                        minLength = dist;
                    }
                }
                for (int i = 0; i < pts.Count; i++)
                {
                    Point3d pt = pts[i];
                    //if (!dic.ContainsKey(pt))
                    //{
                    //    dic[pt] = new List<Tuple<ObjectId,double>>();
                    //}

                    bool flag = false;
                    foreach (Point3d pt1 in dic.Keys)
                    {
                        //some cases pts are not matching thats why we are considering length/gap b/w points less than 1 mm.
                        double dist = AEE_Utility.GetLengthOfLine(pt1, pt);
                        if (dist <= 1)
                        {
                            pt = pt1;
                            flag = true;
                            break;
                        }
                    }

                    if (!flag)
                    {
                        dic[pt] = new List<Tuple<ObjectId, double>>();
                    }

                    if (dic[pt].Where(o => o.Item1 == obj).Any())
                    {
                        continue;
                    }
                    dic[pt].Add(new Tuple<ObjectId, double>(obj, minLength));
                }
            }
            List<List<ObjectId>> lst = new List<List<ObjectId>>();
            var dicResult = new Dictionary<Point3d, Tuple<double, bool>>(new PointComparer());
            foreach (KeyValuePair<Point3d, List<Tuple<ObjectId, double>>> pair in dic)
            {
                if (pair.Value.Count == 1)
                {
                    continue;
                }
                dicResult[pair.Key] = new Tuple<double, bool>(pair.Value.Max(o => o.Item2), false);
            }
            return dicResult;
        }

        public List<ObjectId> getMaxLengthOfWindowLineSegment(List<ObjectId> listOfWindowExplodeIds)
        {
            List<ObjectId> listOfMaxLengthExplodeLine = new List<ObjectId>();

            List<double> listOfLength = new List<double>();
            foreach (var explodeId in listOfWindowExplodeIds)
            {
                double lengthOfLine = AEE_Utility.GetLengthOfLine(explodeId);
                listOfLength.Add(lengthOfLine);
            }
            listOfLength = listOfLength.OrderByDescending(n => n).ToList();
            if (listOfLength.Count != 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    double length = listOfLength[i];
                    foreach (var explodeId in listOfWindowExplodeIds)
                    {
                        double lengthOfLine = AEE_Utility.GetLengthOfLine(explodeId);
                        if ((lengthOfLine == length) && (!listOfMaxLengthExplodeLine.Contains(explodeId)))
                        {
                            listOfMaxLengthExplodeLine.Add(explodeId);
                            break;
                        }
                    }
                }
            }
            return listOfMaxLengthExplodeLine;
        }


        public List<ObjectId> getMaxLengthOfWindowLineSegment(ObjectId windowId)
        {
            List<ObjectId> listOfExplodeId = AEE_Utility.ExplodeEntity(windowId);
            List<ObjectId> listOfMaxLengthExplodeLine = getMaxLengthOfWindowLineSegment(listOfExplodeId);
            return listOfMaxLengthExplodeLine;
        }


        private bool checkMaxLengthWindowLineSegmentAngle(ObjectId orginalWindowId, ObjectId trimLineId)
        {
            var listOfExlplodeEnt = AEE_Utility.ExplodeEntity(orginalWindowId);
            List<ObjectId> listOfMaxLengthExplodeLine = getMaxLengthOfWindowLineSegment(listOfExlplodeEnt);
            foreach (var explodeId in listOfMaxLengthExplodeLine)
            {
                var flagOfBaseLineAngle = WallPanelHelper.checkAngleOfBaseLine(explodeId, trimLineId);
                if (flagOfBaseLineAngle == true)
                {
                    AEE_Utility.deleteEntity(listOfExlplodeEnt);
                    return true;
                }
            }

            AEE_Utility.deleteEntity(listOfExlplodeEnt);

            return false;
        }


        public List<ObjectId> getMinLengthOfWindowLineSegment(List<ObjectId> listOfWindowExplodeIds)
        {
            List<ObjectId> listOfMinLengthExplodeLine = new List<ObjectId>();

            List<double> listOfLength = new List<double>();
            foreach (var explodeId in listOfWindowExplodeIds)
            {
                double lengthOfLine = AEE_Utility.GetLengthOfLine(explodeId);
                listOfLength.Add(lengthOfLine);
            }
            listOfLength = listOfLength.OrderBy(n => n).ToList();
            if (listOfLength.Count != 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    double length = listOfLength[i];
                    foreach (var explodeId in listOfWindowExplodeIds)
                    {
                        double lengthOfLine = AEE_Utility.GetLengthOfLine(explodeId);
                        if ((lengthOfLine == length) && (!listOfMinLengthExplodeLine.Contains(explodeId)))
                        {
                            listOfMinLengthExplodeLine.Add(explodeId);
                            break;
                        }
                    }
                }
            }
            return listOfMinLengthExplodeLine;
        }


        public List<ObjectId> getMinLengthOfWindowLineSegment(ObjectId windowId)
        {
            List<ObjectId> listOfExplodeId = AEE_Utility.ExplodeEntity(windowId);
            List<ObjectId> listOfMinLengthExplodeLine = getMinLengthOfWindowLineSegment(listOfExplodeId);
            return listOfMinLengthExplodeLine;
        }


        public ObjectId createNewWindow_ForIntersectWall_Check(ObjectId windowId)
        {
            List<ObjectId> listOfExplodeWindowId = AEE_Utility.ExplodeEntity(windowId);
            List<ObjectId> listOfMinLengthExplodeLine = getMinLengthOfWindowLineSegment(listOfExplodeWindowId);
            List<Point3d> listOfNewWindowPoint = new List<Point3d>();

            foreach (var lineId in listOfMinLengthExplodeLine)
            {
                List<double> listOfStrtEndPoint = AEE_Utility.getStartEndPointWithAngle_Line(lineId);
                double startX = listOfStrtEndPoint[0];
                double startY = listOfStrtEndPoint[1];
                double endX = listOfStrtEndPoint[2];
                double endY = listOfStrtEndPoint[3];
                double angleOfLine = listOfStrtEndPoint[4];
                var point = AEE_Utility.get_XY(angleOfLine, windowOffsetValue);
                listOfNewWindowPoint.Add(new Point3d((startX - point.X), (startY - point.Y), 0));
                listOfNewWindowPoint.Add(new Point3d((endX + point.X), (endY + point.Y), 0));

                //AEE_Utility.getLineId((startX - point.X), (startY - point.Y), 0, (endX + point.X), (endY + point.Y), 0, true);
            }

            AEE_Utility.deleteEntity(listOfExplodeWindowId);

            ObjectId newWindowId = AEE_Utility.GetRectangleId(listOfNewWindowPoint, false);
            return newWindowId;
        }


        private void drawNewWindowAsPerNewShellPlan()
        {
            for (int index = 0; index < listOfListIntersectWindow_Points.Count; index++)
            {
                List<Point3d> listOfNewWindowVertexPoint = listOfListIntersectWindow_Points[index];
                ObjectId windowId = listOfIntersectWindowObjId[index];
                var windowEnt = AEE_Utility.GetEntityForWrite(windowId);
                if (listOfNewWindowVertexPoint.Count == 4)
                {
                    ObjectId newWindowId = new ObjectId();

                    var newWindowId_ForInsctCheck = AEE_Utility.GetRectangleId(listOfNewWindowVertexPoint, false);
                    var listOfExplodeEntity = AEE_Utility.ExplodeEntity(newWindowId_ForInsctCheck);
                    ObjectId secondObjId = listOfExplodeEntity[1];
                    ObjectId lastObjId = listOfExplodeEntity[(listOfExplodeEntity.Count - 1)];
                    var listOfInsctPoint = AEE_Utility.InterSectionBetweenTwoEntity(secondObjId, lastObjId);
                    if (listOfInsctPoint.Count != 0)
                    {
                        ////////wap the third and fourth line, because diagonal rectangle is creating

                        Point3d fisrtPoint = listOfNewWindowVertexPoint[0];
                        Point3d secondPoint = listOfNewWindowVertexPoint[1];
                        Point3d thirdPoint = listOfNewWindowVertexPoint[2];
                        Point3d forthPoint = listOfNewWindowVertexPoint[3];

                        listOfNewWindowVertexPoint.Clear();
                        listOfNewWindowVertexPoint.Add(fisrtPoint);
                        listOfNewWindowVertexPoint.Add(secondPoint);
                        listOfNewWindowVertexPoint.Add(forthPoint);
                        listOfNewWindowVertexPoint.Add(thirdPoint);
                        newWindowId = AEE_Utility.createRectangleWithSameProperty(listOfNewWindowVertexPoint, windowEnt);
                    }
                    else
                    {
                        newWindowId = AEE_Utility.createRectangleWithSameProperty(listOfNewWindowVertexPoint, windowEnt);
                    }
                    AEE_Utility.deleteEntity(listOfExplodeEntity);
                    AEE_Utility.deleteEntity(newWindowId_ForInsctCheck);

                    listOfWindowObjId.Add(newWindowId);
                    listOfWindowObjId_Str.Add(Convert.ToString(newWindowId));
                }
                else
                {
                    int indexOfWindow = listOfIntersectWindowId_ForTrimLineId.IndexOf(windowId);
                    if (indexOfWindow != -1)
                    {
                        ObjectId newWindowId = drawNewWindowWall(windowId, indexOfWindow, listOfListIntersectWindow_LineId);
                        if (newWindowId.IsValid == true)
                        {
                            listOfWindowObjId.Add(newWindowId);
                            listOfWindowObjId_Str.Add(Convert.ToString(newWindowId));
                        }
                    }
                }
            }
        }


        public ObjectId drawNewWindowWall(ObjectId windowId, int index, List<List<ObjectId>> listOfListIntersectWindow_LineId)
        {
            ObjectId newWindowId = new ObjectId();

            List<ObjectId> listOfNewWindowLineId = listOfListIntersectWindow_LineId[index];
            if (listOfNewWindowLineId.Count == 2)
            {
                ObjectId windowLineId1 = listOfNewWindowLineId[0];
                ObjectId windowLineId2 = listOfNewWindowLineId[1];

                var length = AEE_Utility.GetDistanceBetweenTwoLines(windowLineId1, windowLineId2);

                double windowLineLength1 = AEE_Utility.GetLengthOfLine(windowLineId1);

                double windowLineLength2 = AEE_Utility.GetLengthOfLine(windowLineId2);

                //if (Math.Truncate(windowLineLength1) != Math.Truncate(windowLineLength2))
                {
                    ObjectId maxLengthWindowLineId = new ObjectId();
                    if (windowLineLength1 > windowLineLength2)
                    {
                        maxLengthWindowLineId = windowLineId1;
                    }
                    else
                    {
                        maxLengthWindowLineId = windowLineId2;
                    }

                    var offsetLineId = AEE_Utility.OffsetLine(maxLengthWindowLineId, length, false);
                    var midPoint = AEE_Utility.GetMidPoint(offsetLineId);
                    var flagOfInside = AEE_Utility.GetPointIsInsideThePolyline(windowId, midPoint);
                    if (flagOfInside == false)
                    {
                        AEE_Utility.deleteEntity(offsetLineId);
                        offsetLineId = AEE_Utility.OffsetLine(maxLengthWindowLineId, -length, false);
                    }

                    List<double> listOfStrtEndPoint_MaxLngth = AEE_Utility.getStartEndPointWithAngle_Line(maxLengthWindowLineId);

                    List<double> listOfStrtEndPoint_Offset = AEE_Utility.getStartEndPointWithAngle_Line(offsetLineId);

                    Point3d strtPoint_MaxLngth = new Point3d(listOfStrtEndPoint_MaxLngth[0], listOfStrtEndPoint_MaxLngth[1], 0);
                    Point3d endPoint_MaxLngth = new Point3d(listOfStrtEndPoint_MaxLngth[2], listOfStrtEndPoint_MaxLngth[3], 0);

                    Point3d strtPoint_Offst = new Point3d(listOfStrtEndPoint_Offset[0], listOfStrtEndPoint_Offset[1], 0);
                    Point3d endPoint_Offst = new Point3d(listOfStrtEndPoint_Offset[2], listOfStrtEndPoint_Offset[3], 0);

                    List<Point3d> listOfNewVertexPoint = new List<Point3d>();
                    listOfNewVertexPoint.Add(strtPoint_MaxLngth);
                    listOfNewVertexPoint.Add(endPoint_MaxLngth);
                    listOfNewVertexPoint.Add(endPoint_Offst);
                    listOfNewVertexPoint.Add(strtPoint_Offst);
                    var window_Ent = AEE_Utility.GetEntityForWrite(windowId);

                    newWindowId = AEE_Utility.createRectangleWithSameProperty(listOfNewVertexPoint, window_Ent);
                }
            }
            return newWindowId;
        }

        public void checkWindowIsIntersectToChajjaLine()
        {
            for(int i=0;i< CheckShellPlanHelper.listOfSelectedWindow_ObjId.Count;i++)
            {
                ObjectId winId= CheckShellPlanHelper.listOfSelectedWindow_ObjId[i];

                for (int j = 0; j <CheckShellPlanHelper.listOfSelectedChajja_ObjId.Count;j++)
                {
                    ObjectId chajjaId = CheckShellPlanHelper.listOfSelectedChajja_ObjId[j];
                    var listOfInsctPoint = AEE_Utility.InterSectionBetweenTwoEntity(winId, chajjaId);//PRT
                    if (listOfInsctPoint.Count != 0)
                    {
                        flagOfChajja = true;
                        break;
                    }

                }
            }
        }//Changes made on 22/06/2023 by PRT

        public void checkWindowIsIntersectToWallPanelLine(string xDataRegAppName, ObjectId wallPanelLineId, out List<ObjectId> listOfWindowPanelLine_ObjId, out List<ObjectId> listOfWallPanelLine_ObjId, out List<ObjectId> listOfWindowObjId_With_WallInsct)
        {
            listOfWindowObjId_With_WallInsct = new List<ObjectId>();
            listOfWindowPanelLine_ObjId = new List<ObjectId>();
            listOfWallPanelLine_ObjId = new List<ObjectId>();

            bool flag_X_Axis = false;
            bool flag_Y_Axis = false;

            var listOfWallPanelLineStrtEndPoint = AEE_Utility.getStartEndPointWithAngle_Line(wallPanelLineId);
            Point3d strtPointOfWallPanel = new Point3d(listOfWallPanelLineStrtEndPoint[0], listOfWallPanelLineStrtEndPoint[1], 0);
            Point3d endPointOfWallPanel = new Point3d(listOfWallPanelLineStrtEndPoint[2], listOfWallPanelLineStrtEndPoint[3], 0);
            double wallPanelLineAngle = listOfWallPanelLineStrtEndPoint[4];

            CommonModule commonModule_Obj = new CommonModule();
            commonModule_Obj.checkAngleOfLine_Axis(wallPanelLineAngle, out flag_X_Axis, out flag_Y_Axis);

            List<Point3d> listOfAllInsctPoint = new List<Point3d>();
            List<Point3d> listOfAllWindowPoint = new List<Point3d>();
            List<string> listOfWindowObjId_And_IntersecPoint = new List<string>();
            List<ObjectId> listOfWindowObjId_OfWallInsct = new List<ObjectId>();

            Entity wallPanelEnt_ForSamePropty = AEE_Utility.GetEntityForRead(wallPanelLineId);

            bool flagOfWallLineIsInsideTheWindow = false;

            DoorHelper doorHlp = new DoorHelper();

            foreach (var windowId in listOfWindowObjId)
            {
                flagOfWallLineIsInsideTheWindow = DoorHelper.checkWallPanelLine_IsInsideThe_Window_Or_Door(wallPanelLineId, windowId);
                if (flagOfWallLineIsInsideTheWindow == true)
                {
                    if (listOfAllInsctPoint.Count == 0)
                    {
                        listOfWindowObjId_And_IntersecPoint.Add(Convert.ToString(windowId) + "@" + Convert.ToString(strtPointOfWallPanel));
                        ObjectId windowPanel_LineId = AEE_Utility.getLineId(wallPanelEnt_ForSamePropty, strtPointOfWallPanel.X, strtPointOfWallPanel.Y, 0, endPointOfWallPanel.X, endPointOfWallPanel.Y, 0, false);
                        listOfWindowPanelLine_ObjId.Add(windowPanel_LineId);
                        listOfWindowObjId_OfWallInsct.Add(windowId);
                        setWindowObjId_WithWindowPanelLine(listOfWindowObjId_And_IntersecPoint, listOfWindowObjId_OfWallInsct, strtPointOfWallPanel, endPointOfWallPanel, windowPanel_LineId);
                    }
                    AEE_Utility.AttachXData(windowId, xDataRegAppName, CommonModule.xDataAsciiName);
                    break;
                }

                var listOfInsctPoint = AEE_Utility.InterSectionBetweenTwoEntity(windowId, wallPanelLineId);//PRT
                if (listOfInsctPoint.Count != 0)
                {
                    AEE_Utility.AttachXData(windowId, xDataRegAppName, CommonModule.xDataAsciiName);

                    listOfWindowObjId_With_WallInsct.Add(windowId);
                    foreach (var insctPoint in listOfInsctPoint)
                    {
                        listOfAllInsctPoint.Add(insctPoint);
                        listOfAllWindowPoint.Add(insctPoint);
                        listOfWindowObjId_And_IntersecPoint.Add(Convert.ToString(windowId) + "@" + Convert.ToString(insctPoint));
                        listOfWindowObjId_OfWallInsct.Add(windowId);
                    }

                    if (listOfInsctPoint.Count == 1)
                    {
                        Point3d windowStrtOrEndPoint = new Point3d();
                        Point3d wallStrtOrEndPoint = new Point3d();
                        getStartOrEndPointOfWindow(listOfInsctPoint, wallPanelLineId, windowId, flag_X_Axis, flag_Y_Axis, out windowStrtOrEndPoint, out wallStrtOrEndPoint);

                        var listOfInsctPoint1 = AEE_Utility.InterSectionBetweenTwoEntity(windowId, CornerHelper.listOfAllCornerId_ForBOM[0]);


                        listOfAllInsctPoint.Add(wallStrtOrEndPoint);
                        listOfAllWindowPoint.Add(windowStrtOrEndPoint);
                        listOfWindowObjId_And_IntersecPoint.Add(Convert.ToString(windowId) + "@" + Convert.ToString(windowStrtOrEndPoint));
                        listOfWindowObjId_OfWallInsct.Add(windowId);
                    }
                    else
                    {
                        listOfAllInsctPoint.Add(strtPointOfWallPanel);
                        listOfAllInsctPoint.Add(endPointOfWallPanel);
                    }
                }
            }
            if (listOfAllInsctPoint.Count != 0)
            {
                drawWindowWallPanelLine(listOfAllInsctPoint, listOfAllWindowPoint, listOfWindowObjId_And_IntersecPoint, listOfWindowObjId_OfWallInsct, flag_X_Axis, flag_Y_Axis, out listOfWindowPanelLine_ObjId, out listOfWallPanelLine_ObjId, wallPanelEnt_ForSamePropty);
            }
            else
            {
                if (flagOfWallLineIsInsideTheWindow == false)
                {
                    listOfWallPanelLine_ObjId.Add(wallPanelLineId);
                }
            }
        }


        private void getStartOrEndPointOfWindow(List<Point3d> listOfInsctPoint, ObjectId wallPanelLineId, ObjectId windowId, bool flag_X_Axis, bool flag_Y_Axis, out Point3d windowStrtOrEndPoint, out Point3d wallStrtOrEndPoint)
        {
            CommonModule commonModl = new CommonModule();
            var listOfStrtEndPoint = commonModl.getStartEndPointOfLine(wallPanelLineId);
            var strtPointOfWallPanel = listOfStrtEndPoint[0];
            var endPointOfWallPanel = listOfStrtEndPoint[1];

            var listOfWindowVertexPoint = AEE_Utility.GetPolylineVertexPoint(windowId);
            var window_MinX = listOfWindowVertexPoint.Min(sortPoint => sortPoint.X);
            var window_MaxX = listOfWindowVertexPoint.Max(sortPoint => sortPoint.X);
            var window_MinY = listOfWindowVertexPoint.Min(sortPoint => sortPoint.Y);
            var window_MaxY = listOfWindowVertexPoint.Max(sortPoint => sortPoint.Y);

            windowStrtOrEndPoint = new Point3d();
            wallStrtOrEndPoint = new Point3d();

            Point3d insctPoint = listOfInsctPoint[0];
            if (flag_X_Axis == true)
            {
                if (insctPoint.X > strtPointOfWallPanel.X && window_MaxX > endPointOfWallPanel.X)
                {
                    windowStrtOrEndPoint = endPointOfWallPanel;
                    wallStrtOrEndPoint = strtPointOfWallPanel;
                }
                else
                {
                    windowStrtOrEndPoint = strtPointOfWallPanel;
                    wallStrtOrEndPoint = endPointOfWallPanel;
                }
            }
            else if (flag_Y_Axis == true)
            {
                if (insctPoint.Y > strtPointOfWallPanel.Y && window_MaxY > endPointOfWallPanel.Y)
                {
                    windowStrtOrEndPoint = endPointOfWallPanel;
                    wallStrtOrEndPoint = strtPointOfWallPanel;
                }
                else
                {
                    windowStrtOrEndPoint = strtPointOfWallPanel;
                    wallStrtOrEndPoint = endPointOfWallPanel;
                }
            }
        }


        private void drawWindowWallPanelLine(List<Point3d> listOfAllInsctPoint, List<Point3d> listOfAllWindowPoint, List<string> listOfWindowObjId_And_IntersecPoint, List<ObjectId> listOfWindowObjId_OfWallInsct, bool flag_X_Axis, bool flag_Y_Axis, out List<ObjectId> listOfWindowPanelLine_ObjId, out List<ObjectId> listOfWallPanelLine_ObjId, Entity wallPanelEnt_ForSamePropty)
        {
            listOfWindowPanelLine_ObjId = new List<ObjectId>();
            listOfWallPanelLine_ObjId = new List<ObjectId>();

            if (flag_X_Axis == true)
            {
                listOfAllInsctPoint = listOfAllInsctPoint.OrderBy(sortPoint => sortPoint.X).ToList();
                listOfAllWindowPoint = listOfAllWindowPoint.OrderBy(sortPoint => sortPoint.X).ToList();
            }
            else if (flag_Y_Axis == true)
            {
                listOfAllInsctPoint = listOfAllInsctPoint.OrderBy(sortPoint => sortPoint.Y).ToList();
                listOfAllWindowPoint = listOfAllWindowPoint.OrderBy(sortPoint => sortPoint.Y).ToList();
            }

            for (int k = 0; k < listOfAllInsctPoint.Count; k++)
            {
                if (k == (listOfAllInsctPoint.Count - 1))
                {
                    break;
                }
                Point3d point1 = listOfAllInsctPoint[k];
                Point3d point2 = listOfAllInsctPoint[k + 1];
                bool flagOfExistWindowPoints = false;
                for (int p = 0; p < listOfAllWindowPoint.Count; p = p + 2)
                {
                    if (p == (listOfAllWindowPoint.Count - 1))
                    {
                        break;
                    }
                    Point3d windowPoint1 = listOfAllWindowPoint[p];
                    Point3d windowPoint2 = listOfAllWindowPoint[p + 1];
                    if (point1 == windowPoint1 && point2 == windowPoint2)
                    {
                        flagOfExistWindowPoints = true;
                        break;
                    }
                }
                if (flagOfExistWindowPoints == false)
                {
                    var length = AEE_Utility.GetLengthOfLine(point1.X, point1.Y, point2.X, point2.Y);
                    if (length >= 1)
                    {
                        ObjectId wallPanel_LineId = AEE_Utility.getLineId(wallPanelEnt_ForSamePropty, point1.X, point1.Y, 0, point2.X, point2.Y, 0, false);
                        listOfWallPanelLine_ObjId.Add(wallPanel_LineId);
                    }

                }
            }

            for (int p = 0; p < listOfAllWindowPoint.Count; p = p + 2)
            {
                if (p == (listOfAllWindowPoint.Count - 1))
                {
                    break;
                }
                Point3d windowPoint1 = listOfAllWindowPoint[p];
                Point3d windowPoint2 = listOfAllWindowPoint[p + 1];

                ObjectId windowPanel_LineId = AEE_Utility.getLineId(wallPanelEnt_ForSamePropty, windowPoint1.X, windowPoint1.Y, 0, windowPoint2.X, windowPoint2.Y, 0, false);
                listOfWindowPanelLine_ObjId.Add(windowPanel_LineId);

                setWindowObjId_WithWindowPanelLine(listOfWindowObjId_And_IntersecPoint, listOfWindowObjId_OfWallInsct, windowPoint1, windowPoint2, windowPanel_LineId);
            }
        }


        private string setWindowObjId_WithWindowPanelLine(List<string> listOfWindowObjId_And_IntersecPoint, List<ObjectId> listOfWindowObjId_OfWallInsct, Point3d windowPoint1, Point3d windowPoint2, ObjectId windowPanel_LineId)
        {
            string outputWindowObjId_Str = "";
            ObjectId windowObjId = new ObjectId();
            for (int k = 0; k < listOfWindowObjId_And_IntersecPoint.Count; k++)
            {
                var data = listOfWindowObjId_And_IntersecPoint[k];
                var array = data.Split('@');
                string windowObjId_Str = Convert.ToString(array.GetValue(0));
                string windowInsctPoint = Convert.ToString(array.GetValue(1));
                if (windowInsctPoint == Convert.ToString(windowPoint1))
                {
                    outputWindowObjId_Str = windowObjId_Str;
                    windowObjId = listOfWindowObjId_OfWallInsct[k];
                    break;
                }
                else if (windowInsctPoint == Convert.ToString(windowPoint2))
                {
                    outputWindowObjId_Str = windowObjId_Str;
                    windowObjId = listOfWindowObjId_OfWallInsct[k];
                    break;
                }
            }

            if (outputWindowObjId_Str != "")
            {
                if (listOfExistWindowPanelLineObj.Contains(outputWindowObjId_Str))
                {
                    int indexOfExistWindow = listOfExistWindowPanelLineObj.IndexOf(outputWindowObjId_Str);
                    string prvsData = listOfWindowObjId_With_WindowPanelLine[indexOfExistWindow];
                    string newData = prvsData + "," + Convert.ToString(windowPanel_LineId);
                    listOfWindowObjId_With_WindowPanelLine[listOfWindowObjId_With_WindowPanelLine.FindIndex(ind => ind.Equals(prvsData))] = newData;
                }
                else
                {
                    listOfWindowObjId_With_WindowPanelLine.Add(outputWindowObjId_Str + "@" + Convert.ToString(windowPanel_LineId));
                    listOfExistWindowPanelLineObj.Add(outputWindowObjId_Str);
                    listOfWindowObjId_With_WindowLine.Add(windowObjId);
                }
            }
            return outputWindowObjId_Str;
        }


        public void setWindowPanelLines(string xDataRegAppName, ObjectId nearestWallToBeamWallId, double distBtwWallToBeam, ObjectId beamId_InsctWall, string beamLayerNameInsctToWindowWall, List<ObjectId> listOfWindowPanelLine_Id, ObjectId cornerId1, ObjectId cornerId2, double wallPanelLineLength, bool flagOfSunkanSlab, ObjectId sunkanSlabId, ObjectId parapetId)
        {
            InternalWallHelper internalHlper = new InternalWallHelper();
            List<ObjectId> listOfLineBtwTwoCrners_ObjId = new List<ObjectId>();
            List<double> listOfDistBtwTwoCrners = new List<double>();
            List<string> listOfDistAndObjId_BtwTwoCrners = new List<string>();
            List<ObjectId> listOfOffstWindowPanelLines_ObjId = new List<ObjectId>();
            internalHlper.getPanelLine(listOfWindowPanelLine_Id, cornerId1, cornerId2, wallPanelLineLength, out listOfLineBtwTwoCrners_ObjId, out listOfDistBtwTwoCrners, out listOfDistAndObjId_BtwTwoCrners, out listOfOffstWindowPanelLines_ObjId);

            for (int p = 0; p < listOfLineBtwTwoCrners_ObjId.Count; p++)
            {
                ObjectId windowPanelLine_ObjId = listOfLineBtwTwoCrners_ObjId[p];
                AEE_Utility.AttachXData(windowPanelLine_ObjId, xDataRegAppName, CommonModule.xDataAsciiName);

                listOfWindowPanelLine_ObjId.Add(windowPanelLine_ObjId);
                listOfWindowPanelLine_ObjId_InStr.Add(Convert.ToString(windowPanelLine_ObjId));
                ObjectId windowPanelOffsetLine_ObjId = listOfOffstWindowPanelLines_ObjId[p];
                listOfWindowPanelOffsetLine_ObjId.Add(windowPanelOffsetLine_ObjId);
                BeamHelper.listOfBeamName_InsctToWindowWall.Add(beamLayerNameInsctToWindowWall);
                BeamHelper.listOfInsctWindow_BeamInsctId.Add(beamId_InsctWall);
                BeamHelper.listOfDistanceBtwWindowToBeam.Add(distBtwWallToBeam);
                BeamHelper.listOfNearestBtwWindowToBeamWallLineId.Add(nearestWallToBeamWallId);

                SunkanSlabHelper.listOfWindowWall_SunkanSlabWallLineId.Add(windowPanelLine_ObjId);
                SunkanSlabHelper.listOfWindowWall_SunkanSlabWallLineflag.Add(flagOfSunkanSlab);
                SunkanSlabHelper.listOfWindowWall_SunkanSlabId.Add(sunkanSlabId);


                ParapetHelper.listOfParapetId_WithWindowWallLine.Add(parapetId);
                ParapetHelper.listOfWindowWallLineId_InsctToParapet.Add(windowPanelLine_ObjId);

                ////listOfDistnceBtwTwoCrners.Add(listOfDistBtwTwoCrners[p]);
                ////listOfDistnceAndObjId_BtwTwoCrners.Add(listOfDistAndObjId_BtwTwoCrners[p]);
                ////listOfOffsetLinesBtwTwoCrners_ObjId.Add(listOfOffstLinesBtwTwoCrners_ObjId[p]);
            }
        }

        public void createCornersInWindow(string xDataRegAppName, string beamLayerNameInsctToWall, List<ObjectId> listOfWindowObjId_With_WallInsct, List<ObjectId> listOfWindowPanelLine_ObjId, List<ObjectId> listOfWindowPanelOffsetLine_ObjId, ObjectId offsetWall_ObjId, ObjectId wallCornerId1, ObjectId wallCornerId2, List<ObjectId> cornerId1_TextId, List<ObjectId> cornerId2_TextId, ObjectId sunkanSlabId, ObjectId parapetId)
        {
            PartHelper partHelper = new PartHelper();

            if (listOfWindowObjId_With_WallInsct.Count != listOfWindowPanelLine_ObjId.Count)
            {
                return;
            }
            //Fix WSXR group bug for wall that has more than one window by SDM 29_May_2022
            //start add a for loop
            for (int m = 0; m < listOfWindowPanelLine_ObjId.Count; m++)
            {
                //end

                ObjectId windowId = listOfWindowObjId_With_WallInsct[m];
                for (int i = 0; i < listOfWindowPanelLine_ObjId.Count; i++)
                {
                    ObjectId windowWallPanelId = listOfWindowPanelLine_ObjId[i];
                    ObjectId offsetWindowWallPanelId = listOfWindowPanelOffsetLine_ObjId[i];

                    double windowWallAngle = AEE_Utility.GetAngleOfLine(windowWallPanelId);
                    List<double> listOfAngle = new List<double>();
                    listOfAngle.Add(windowWallAngle);
                    listOfAngle.Add(windowWallAngle + CommonModule.angle_90);
                    listOfAngle.Add(windowWallAngle + CommonModule.angle_180);
                    listOfAngle.Add(windowWallAngle + CommonModule.angle_270);

                    List<ObjectId> listOfWndowCrnrId_NotInsctToWallCrnr = new List<ObjectId>();
                    List<List<ObjectId>> listOfWndowCrnrTextId_NotInsctToWallCrnr = new List<List<ObjectId>>();

                    var listOfStrtEndPoint = AEE_Utility.GetStartEndPointOfLine(windowWallPanelId);
                    for (int j = 0; j < listOfStrtEndPoint.Count; j++)
                    {
                        Point3d basePoint = listOfStrtEndPoint[j];

                        for (int k = 0; k < listOfAngle.Count; k++)
                        {
                            var externalCornerId = partHelper.drawExternalCorner(xDataRegAppName, basePoint.X, basePoint.Y, CommonModule.panelDepth, CommonModule.externalCornerThick, CommonModule.externalCornerLyrName, CommonModule.externalCornerLyrColor);
                            AEE_Utility.AttachXData(externalCornerId, xDataRegAppName, CommonModule.xDataAsciiName);
                            AEE_Utility.AttachXData(externalCornerId, xDataRegAppName + "_1", CommonModule.panelDepth.ToString() + "," +
                                                                                              CommonModule.externalCornerThick.ToString());

                            double angle = listOfAngle[k];
                            CommonModule.rotateAngle = (angle * Math.PI) / 180; // variable assign for rotation             
                            CommonModule.basePoint = new Point3d(basePoint.X, basePoint.Y, 0);
                            List<ObjectId> listOfObjId = new List<ObjectId>();
                            listOfObjId.Add(externalCornerId);
                            MoveRorationScaleJig.RotationScaleJig_Method(listOfObjId);


                            var listOfInsctWithWindow = AEE_Utility.InterSectionBetweenTwoEntity(externalCornerId, windowId);
                            if (listOfInsctWithWindow.Count == 0)
                            {
                                AEE_Utility.deleteEntity(externalCornerId);
                                continue;
                            }

                            var listOfInsctWithOffsetWall = AEE_Utility.InterSectionBetweenTwoEntity(externalCornerId, offsetWall_ObjId);
                            if (listOfInsctWithOffsetWall.Count == 0)
                            {
                                AEE_Utility.deleteEntity(externalCornerId);
                            }
                            else
                            {
                                var listOfInsctWithWindowPanelLine = AEE_Utility.InterSectionBetweenTwoEntity(externalCornerId, windowWallPanelId);
                                if (listOfInsctWithWindowPanelLine.Count <= 1)
                                {
                                    AEE_Utility.deleteEntity(externalCornerId);
                                }
                                else
                                {
                                    List<ObjectId> windowCornerTextId = writeTextInWindowCorner(xDataRegAppName, beamLayerNameInsctToWall, windowWallPanelId, basePoint, angle, externalCornerId, sunkanSlabId, parapetId);
                                    listOfWndowCrnrId_NotInsctToWallCrnr.Add(externalCornerId);
                                    listOfWndowCrnrTextId_NotInsctToWallCrnr.Add(windowCornerTextId);
                                    break;
                                }
                            }
                        }
                    }
                    setDataOfWindowCornerId_NotInsctToWallCornerId(windowId, listOfWndowCrnrId_NotInsctToWallCrnr, listOfWndowCrnrTextId_NotInsctToWallCrnr, xDataRegAppName);
                }
            }
        }


        private void setDataOfWindowCornerId_NotInsctToWallCornerId(ObjectId windowId, List<ObjectId> listOfWindowCornerId, List<List<ObjectId>> listOfWindowCornerTextId, string xDataRegAppName)
        {
            if (listOfWindowCornerId.Count != 2)
            {
                return;
            }

            Point3d offsetPoint = new Point3d(12345678, 1234578, 0);
            double offsetValue = 10;
            int indexOfExist = -1;

            for (int index = 0; index < listOfWndowId_NotInsctToCorner.Count; index++)
            {
                ObjectId prvsWindowId = listOfWndowId_NotInsctToCorner[index];

                ObjectId prvsOffsetWindowId = AEE_Utility.OffsetEntity_WithoutLine(offsetValue, offsetPoint, prvsWindowId, false);
                ObjectId cornerId = listOfWindowCornerId[0];
                var insctOfPoint = AEE_Utility.InterSectionBetweenTwoEntity(prvsWindowId, cornerId);
                if (insctOfPoint.Count != 0)
                {
                    indexOfExist = index;
                    AEE_Utility.deleteEntity(prvsOffsetWindowId);
                    break;
                }
                AEE_Utility.deleteEntity(prvsOffsetWindowId);
            }

            //  indexOfExist = listOfWndowId_NotInsctToCorner.IndexOf(windowId);
            if (indexOfExist == -1)
            {
                listOfWndowId_NotInsctToCorner.Add(windowId);
                listOfListOfWndowCornerId_NotInsctToWallCorner.Add(listOfWindowCornerId);
                listOfListOfWndowCornerTextId_NotInsctToWallCorner.Add(listOfWindowCornerTextId);
            }
            else
            {
                var listOfPrvsWndowCornerId_NotInsctToWallCorner = listOfListOfWndowCornerId_NotInsctToWallCorner[indexOfExist];

                var listOfNewWndowCornerId_NotInsctToWallCorner = listOfPrvsWndowCornerId_NotInsctToWallCorner;
                foreach (var wndowCornerId in listOfWindowCornerId)
                {
                    listOfNewWndowCornerId_NotInsctToWallCorner.Add(wndowCornerId);
                }
                listOfListOfWndowCornerId_NotInsctToWallCorner[indexOfExist] = listOfNewWndowCornerId_NotInsctToWallCorner;

                var listOfPrvsWndowCornerTextId_NotInsctToWallCorner = listOfListOfWndowCornerTextId_NotInsctToWallCorner[indexOfExist];
                var listOfNewWndowCornerTextId_NotInsctToWallCorner = listOfPrvsWndowCornerTextId_NotInsctToWallCorner;
                foreach (var wndowCornerTextId in listOfWindowCornerTextId)
                {
                    listOfNewWndowCornerTextId_NotInsctToWallCorner.Add(wndowCornerTextId);
                }
                listOfListOfWndowCornerTextId_NotInsctToWallCorner[indexOfExist] = listOfNewWndowCornerTextId_NotInsctToWallCorner;
            }
        }


        private List<ObjectId> writeTextInWindowCorner(string xDataRegAppName, string beamLayerNameInsctToWall, ObjectId windowWallPanelId, Point3d basePoint, double rotationAngle, ObjectId newExternalCornerId, ObjectId sunkanSlabId, ObjectId parapetId)
        {
            ObjectId windowId = new ObjectId();
            string windowLayer = "";
            foreach (var data in listOfWindowObjId_With_WindowPanelLine)
            {
                string[] splitWithAddTheRateSymbol = data.Split('@');
                string windowIdStr = Convert.ToString(splitWithAddTheRateSymbol.GetValue(0));
                string windowWallPanelLinesStr = Convert.ToString(splitWithAddTheRateSymbol.GetValue(1));
                string[] splitWithCommas = windowWallPanelLinesStr.Split(',');
                foreach (string windowPanelLineId_Str in splitWithCommas)
                {
                    if (windowPanelLineId_Str == Convert.ToString(windowWallPanelId))
                    {
                        int index = listOfWindowObjId_Str.IndexOf(windowIdStr);
                        if (index != -1)
                        {
                            windowId = listOfWindowObjId[index];
                            var windowEnt = AEE_Utility.GetEntityForRead(windowId);
                            windowLayer = windowEnt.Layer;
                            break;
                        }
                    }
                }
            }

            CornerHelper cornerHlpr = new CornerHelper();
            var windowWallPanelLineEntity = AEE_Utility.GetEntityForRead(windowWallPanelId);
            List<List<object>> listOfCornerData_ForBOM = new List<List<object>>();
            List<string> cornerTexts = getCornerText(windowLayer, beamLayerNameInsctToWall, windowWallPanelLineEntity, newExternalCornerId, sunkanSlabId, out listOfCornerData_ForBOM);

            double crnerRotationAngle = rotationAngle + CommonModule.cornerRotationAngle;
            List<ObjectId> windowCornerTextId = cornerHlpr.writeMultipleTextInCorner(newExternalCornerId, cornerTexts, crnerRotationAngle, CommonModule.externalCornerTextLyrName, CommonModule.externalCornerLyrColor);

            bool flagOfParapet = false;
            ParapetHelper parapetHlp = new ParapetHelper();
            parapetHlp.changeHeightOfWindowOrDoorCornerWithParapet(xDataRegAppName, parapetId, newExternalCornerId, windowCornerTextId, sunkanSlabId, windowWallPanelLineEntity.Layer, out flagOfParapet);

            if (flagOfParapet == false)
            {
                for (int i = 0; i < listOfCornerData_ForBOM.Count; i++)
                {
                    string cornerDescp = Convert.ToString(listOfCornerData_ForBOM[i][0]);
                    string cornerType = Convert.ToString(listOfCornerData_ForBOM[i][1]);
                    double flange1 = Convert.ToDouble(listOfCornerData_ForBOM[i][2]);
                    double flange2 = Convert.ToDouble(listOfCornerData_ForBOM[i][3]);
                    double cornerWallHeight = Convert.ToDouble(listOfCornerData_ForBOM[i][4]);
                    CornerHelper.setCornerDataForBOM(xDataRegAppName, newExternalCornerId, windowCornerTextId[i], cornerTexts[i], cornerDescp, cornerType, flange1, flange2, cornerWallHeight/*, 0.0/*elev*/);
                }
            }

            return windowCornerTextId;
        }


        private List<string> getCornerText(string windowLayer, string beamLayerNameInsctToWall, Entity windowWallPanelLineEntity, ObjectId newExternalCornerId, ObjectId sunkanSlabId, out List<List<object>> listOfCornerData_ForBOM)
        {
            listOfCornerData_ForBOM = new List<List<object>>();

            ObjectId cornerId = newExternalCornerId;

            List<string> cornerTexts = new List<string>();

            double windowLintelLevel = Window_UI_Helper.getWindowLintelLevel(windowLayer);
            double windowSillLevel = Window_UI_Helper.getWindowSillLevel(windowLayer);

            double beamBottom = Beam_UI_Helper.getOffsetBeamBottom(beamLayerNameInsctToWall);

            double cornerWallHeight = GeometryHelper.getHeightOfWindow_InAtNotCornerSide_EC_In_InternalExternalWall(windowLintelLevel, windowSillLevel);
            double levelDifferenceOfSunkanSlb = SunkanSlabHelper.getSunkanSlabLevelDifference_LessThan_RC__Corners(sunkanSlabId);
            cornerWallHeight = cornerWallHeight + levelDifferenceOfSunkanSlb;

            var cornerEnt = AEE_Utility.GetEntityForRead(cornerId);

            if (cornerEnt.Layer == CommonModule.internalCornerLyrName)
            {
                cornerTexts = CornerHelper.UpdateCornerText(listOfCornerData_ForBOM, cornerWallHeight, sunkanSlabId);
            }
            else if (cornerEnt.Layer == CommonModule.externalCornerLyrName)
            {
                cornerTexts = CornerHelper.UpdateCornerTextExternal(listOfCornerData_ForBOM, cornerWallHeight);
            }

            return cornerTexts;
        }


        public void drawWindowPanels(ProgressForm progressForm, string panelCreationMsg, Dictionary<Point3d, Tuple<double, bool>> dicCornerPoints)
        {


            List<List<ObjectId>> listOfListWindowPanelLines_ObjId = new List<List<ObjectId>>();
            List<List<ObjectId>> listOfListWindowPanelOffsetLines_ObjId = new List<List<ObjectId>>();
            List<ObjectId> listOfWindowObjId_InsctWall = new List<ObjectId>();
            List<ObjectId> listOfParapetId_WithWindowWallLine = new List<ObjectId>();
            getWindowPanelsLines(out listOfListWindowPanelLines_ObjId, out listOfListWindowPanelOffsetLines_ObjId, out listOfWindowObjId_InsctWall, out listOfParapetId_WithWindowWallLine);

            for (int i = 0; i < listOfListWindowPanelLines_ObjId.Count; i++)
            {
                if ((i % 20) == 0)
                {
                    progressForm.ReportProgress(1, panelCreationMsg);
                }

                List<ObjectId> listWindowPanelLines_ObjId = listOfListWindowPanelLines_ObjId[i];
                List<ObjectId> listWindowPanelOffsetLines_ObjId = listOfListWindowPanelOffsetLines_ObjId[i];
                ObjectId parapetId = listOfParapetId_WithWindowWallLine[i];

                if (listWindowPanelLines_ObjId.Count == 2)
                {

                    ObjectId windowObjId = listOfWindowObjId_InsctWall[i];
                    Entity windowEnt = AEE_Utility.GetEntityForRead(windowObjId);
                    string windowLayer = windowEnt.Layer;

                    // TODO: added elevation initialize
                    //commented by SDM 2022-07-04 to fix the elevation
                    double elevation = 0;// Window_UI_Helper.getWindowLintelLevel(windowLayer); //RTJ 10-06-2021

                    List<string> listOfBeamLayerName_WindowWallInsct = new List<string>();
                    List<ObjectId> listOfInsctWindow_BeamInsctId = new List<ObjectId>();
                    List<double> listOfDistanceBtwWindowToBeam = new List<double>();
                    List<ObjectId> listOfNearestBtwWindowToBeamWallLineId = new List<ObjectId>();
                    List<bool> listOfWindowWall_SunkanSlabWallLineflag = new List<bool>();
                    List<ObjectId> listOfWindowWall_SunkanSlabId = new List<ObjectId>();

                    DoorHelper doorHlp = new DoorHelper();
                    List<List<ObjectId>> listOfListOfWindowWallNewLineId = doorHlp.getDoorOrWindowWallLinesId(windowObjId, listWindowPanelLines_ObjId, listOfWindowPanelLine_ObjId, out listOfBeamLayerName_WindowWallInsct, out listOfInsctWindow_BeamInsctId, out listOfDistanceBtwWindowToBeam, out listOfNearestBtwWindowToBeamWallLineId, out listOfWindowWall_SunkanSlabWallLineflag, out listOfWindowWall_SunkanSlabId);

                    var isHorzLayout = PanelLayout_UI.flagOfHorzPanel_ForDoorWindowBeam;
                    for (int j = 0; j < listOfListOfWindowWallNewLineId.Count; j++)
                    {
                        string beamLayerName_InscWindowWall = listOfBeamLayerName_WindowWallInsct[j];
                        ObjectId beamId = listOfInsctWindow_BeamInsctId[j];
                        double distanceBtwDoorToBeam = listOfDistanceBtwWindowToBeam[j];
                        ObjectId nrstBtwDoorToBeamWallLineId = listOfNearestBtwWindowToBeamWallLineId[j];

                        bool flagOfSunkanSlab = listOfWindowWall_SunkanSlabWallLineflag[j];
                        ObjectId sunkanSlabId = listOfWindowWall_SunkanSlabId[j];

                        List<ObjectId> listOfWindowWallNewLineId = listOfListOfWindowWallNewLineId[j];

                        if (isHorzLayout)
                        {
                            for (int k = 0; k < listOfWindowWallNewLineId.Count; k = (k + 2))
                            {
                                var windowPanelLine_Id = listOfWindowWallNewLineId[k];
                                var offsetWindowPanelLine_Id = listOfWindowWallNewLineId[k + 1];

                                double windowWallHeight = 0;
                                double windowWall_X_Height = 0;
                                getWindowWallText(windowLayer, beamLayerName_InscWindowWall, windowPanelLine_Id, parapetId, sunkanSlabId, out windowWallHeight, out windowWall_X_Height);
                                if (windowWall_X_Height > PanelLayout_UI.maxValueToConvertHorzToVert)
                                {
                                    isHorzLayout = false;
                                    break;
                                }
                            }
                        }
                    }

                    for (int j = 0; j < listOfListOfWindowWallNewLineId.Count; j++)
                    {
                        string beamLayerName_InscWindowWall = listOfBeamLayerName_WindowWallInsct[j];
                        ObjectId beamId = listOfInsctWindow_BeamInsctId[j];
                        double distanceBtwDoorToBeam = listOfDistanceBtwWindowToBeam[j];
                        ObjectId nrstBtwDoorToBeamWallLineId = listOfNearestBtwWindowToBeamWallLineId[j];

                        bool flagOfSunkanSlab = listOfWindowWall_SunkanSlabWallLineflag[j];
                        ObjectId sunkanSlabId = listOfWindowWall_SunkanSlabId[j];

                        List<ObjectId> listOfWindowWallNewLineId = listOfListOfWindowWallNewLineId[j];

                        for (int k = 0; k < listOfWindowWallNewLineId.Count; k = (k + 2))
                        {
                            var windowPanelLine_Id = listOfWindowWallNewLineId[k];
                            var offsetWindowPanelLine_Id = listOfWindowWallNewLineId[k + 1];

                            double windowWallHeight = 0;
                            double windowWall_X_Height = 0;
                            getWindowWallText(windowLayer, beamLayerName_InscWindowWall, windowPanelLine_Id, parapetId, sunkanSlabId, out windowWallHeight, out windowWall_X_Height);
                            var xDataRegAppName = AEE_Utility.GetXDataRegisterAppName(windowPanelLine_Id);
#if !WNPANEL //Added on 26/04/2023 by SDM
                            isHorzLayout = windowWall_X_Height <= PanelLayout_UI.maxValueToConvertHorzToVert;//by SDM 2022-08-04
#endif
                            //Fix elevation when there is a sunkan by SDM 2022-07-04
                            double elev = elevation;
                            if (flagOfSunkanSlab)
                            {
                                windowWall_X_Height -= SunkanSlabHelper.getSunkanSlabLevelDifference(sunkanSlabId);
                                elev = -PanelLayout_UI.RC;
                            }

                            List<ObjectId> listOfWallPanelRect_Id = new List<ObjectId>();
                            List<ObjectId> listOfTextId = new List<ObjectId>();
                            List<ObjectId> listOfWallXTextId = new List<ObjectId>();
                            drawWindowPanels(xDataRegAppName, flagOfSunkanSlab, sunkanSlabId, beamLayerName_InscWindowWall, beamId, distanceBtwDoorToBeam, nrstBtwDoorToBeamWallLineId, windowPanelLine_Id, offsetWindowPanelLine_Id, windowWallHeight, windowWall_X_Height, parapetId, out listOfWallPanelRect_Id, out listOfTextId, out listOfWallXTextId, isHorzLayout, windowLayer, elev); //RTJ 10-06-2021
                        }
                    }

                    ObjectId sunkanSlabId_Beam = new ObjectId();
                    drawWindowDoorBottomPanel(windowObjId, windowLayer, sunkanSlabId_Beam, CommonModule.wndowwallPanelType, dicCornerPoints);
                }
            }
        }


        public void groupCreateInWindow(List<List<ObjectId>> listOfListWallPanelRect_Id, List<List<ObjectId>> listOfListTextId, List<List<ObjectId>> listOfListWallXTextId)
        {
            if (listOfListWallPanelRect_Id.Count == 2)
            {
                List<ObjectId> listWallPanelRect_Id1 = listOfListWallPanelRect_Id[0];
                List<ObjectId> listTextId1 = listOfListTextId[0];
                List<ObjectId> listWallXTextId1 = listOfListWallXTextId[0];

                List<ObjectId> listWallPanelRect_Id2 = listOfListWallPanelRect_Id[1];
                List<ObjectId> listTextId2 = listOfListTextId[1];
                List<ObjectId> listWallXTextId2 = listOfListWallXTextId[1];

                for (int k = 0; k < listWallPanelRect_Id1.Count; k++)
                {
                    List<ObjectId> listOfObjId_ForGroupCreator = new List<ObjectId>();

                    if (k < listWallPanelRect_Id1.Count)
                    {
                        listOfObjId_ForGroupCreator.Add(listWallPanelRect_Id1[k]);
                    }

                    if (k < listTextId1.Count)
                    {
                        listOfObjId_ForGroupCreator.Add(listTextId1[k]);
                    }

                    if (k < listWallXTextId1.Count)
                    {
                        listOfObjId_ForGroupCreator.Add(listWallXTextId1[k]);
                    }

                    if (k < listWallPanelRect_Id2.Count)
                    {
                        listOfObjId_ForGroupCreator.Add(listWallPanelRect_Id2[k]);
                    }

                    if (k < listTextId2.Count)
                    {
                        listOfObjId_ForGroupCreator.Add(listTextId2[k]);
                    }

                    if (k < listWallXTextId2.Count)
                    {
                        listOfObjId_ForGroupCreator.Add(listWallXTextId2[k]);
                    }

                    if (listOfObjId_ForGroupCreator.Count != 0)
                    {
                        CommonModule commonMod = new CommonModule();
                        commonMod.createGroup(listOfObjId_ForGroupCreator);
                    }
                }
            }
        }


        private static void getWindowPanelsLines(out List<List<ObjectId>> listOfListWindowPanelLines_ObjId, out List<List<ObjectId>> listOfListWindowPanelOffsetLines_ObjId, out List<ObjectId> listOfWindowObjId_InsctWall, out List<ObjectId> listOfParapetId_WithWindowWallLine)
        {
            listOfParapetId_WithWindowWallLine = new List<ObjectId>();
            listOfListWindowPanelLines_ObjId = new List<List<ObjectId>>();
            listOfListWindowPanelOffsetLines_ObjId = new List<List<ObjectId>>();
            listOfWindowObjId_InsctWall = new List<ObjectId>();

            for (int i = 0; i < listOfWindowObjId_With_WindowPanelLine.Count; i++)
            {
                string data = listOfWindowObjId_With_WindowPanelLine[i];
                var array = data.Split('@');
                var windowObj_Str = Convert.ToString(array.GetValue(0));
                var windowPanelLineData = Convert.ToString(array.GetValue(1));
                var windowPanelLineDataArray = windowPanelLineData.Split(',');

                List<ObjectId> listOfPanelLineObjId = new List<ObjectId>();
                List<ObjectId> listOfPanelOffstLineObjId = new List<ObjectId>();
                List<ObjectId> listOfParapetId = new List<ObjectId>();
                foreach (var windowPanelLine_ObjId in windowPanelLineDataArray)
                {
                    int indexOfWindowPanelId_Str = listOfWindowPanelLine_ObjId_InStr.IndexOf(windowPanelLine_ObjId);
                    if (indexOfWindowPanelId_Str != -1)
                    {
                        listOfPanelLineObjId.Add(listOfWindowPanelLine_ObjId[indexOfWindowPanelId_Str]);
                        listOfPanelOffstLineObjId.Add(listOfWindowPanelOffsetLine_ObjId[indexOfWindowPanelId_Str]);
                        listOfParapetId.Add(ParapetHelper.listOfParapetId_WithWindowWallLine[indexOfWindowPanelId_Str]);
                    }
                }
                if (listOfPanelLineObjId.Count != 0)
                {
                    listOfListWindowPanelLines_ObjId.Add(listOfPanelLineObjId);
                    listOfListWindowPanelOffsetLines_ObjId.Add(listOfPanelOffstLineObjId);
                    listOfParapetId_WithWindowWallLine.Add(listOfParapetId[0]);
                }
                ObjectId windowObjId = listOfWindowObjId_With_WindowLine[i];
                listOfWindowObjId_InsctWall.Add(windowObjId);
            }
        }
        public void drawHorzPanel_ForWindowAndDoor(string wallType, string xDataRegAppName, double standardWallHeight, List<Point3d> listOfLineAndOffsetLinePoint, double wallXHeight, string wallXHeightText, string layerName, int layerColor, bool flag_X_Axis, bool flag_Y_Axis, string beamLayerName, ObjectId beamId, double distanceBtwWallToBeam, ObjectId nearestBeamWallLineId, ObjectId doorPanelLine_ObjId, bool flagOfSunkanSlab, ObjectId sunkanSlabId, string windowLayer)  //RTJ 10-06-2021
        {
            Point3d baseStrtPoint = listOfLineAndOffsetLinePoint[0];
            Point3d baseEndPoint = listOfLineAndOffsetLinePoint[1];
            Point3d offsetBaseStrtPoint = listOfLineAndOffsetLinePoint[2];
            Point3d offsetBaseEndPoint = listOfLineAndOffsetLinePoint[3];

            BOMHelper bomHlp = new BOMHelper();

            List<ObjectId> listOfWallPanelRect_ObjId_ForBeamLayout = new List<ObjectId>();
            List<ObjectId> listOfWallPanelLine_ObjId_ForBeamLayout = new List<ObjectId>();

            double lineAngle = AEE_Utility.GetAngleOfLine(baseStrtPoint.X, baseStrtPoint.Y, baseEndPoint.X, baseEndPoint.Y);
            double lengthOfLine = AEE_Utility.GetLengthOfLine(baseStrtPoint.X, baseStrtPoint.Y, baseEndPoint.X, baseEndPoint.Y);
            lengthOfLine = Math.Round(lengthOfLine);

            if (lengthOfLine != 0)
            {
                List<double> listOfWallPanelLength = getListOfHorzPanelLength(lengthOfLine, PanelLayout_UI.maxHeightOfWindowAndDoor/*standardWallHeight*/, PanelLayout_UI.minWidthOfPanel);
                Point3d startPoint = baseStrtPoint;
                Point3d offsetStartPoint = offsetBaseStrtPoint;

                for (int k = 0; k < listOfWallPanelLength.Count; k++)
                {
                    double wallPnlLngth = listOfWallPanelLength[k];
                    var point = AEE_Utility.get_XY(lineAngle, wallPnlLngth);
                    Point3d endPoint = new Point3d((startPoint.X + point.X), (startPoint.Y + point.Y), 0);
                    Point3d offstEndPoint = new Point3d((offsetStartPoint.X + point.X), (offsetStartPoint.Y + point.Y), 0);

                    double length = 0;
                    if (flag_X_Axis == true)
                    {
                        length = Math.Truncate(Math.Abs(startPoint.X - endPoint.X));
                    }
                    else if (flag_Y_Axis == true)
                    {
                        length = Math.Truncate(Math.Abs(startPoint.Y - endPoint.Y));
                    }
                    if (length != 0)
                    {
                        List<ObjectId> listOfMoveId = new List<ObjectId>();
                        var wallPanelLineId = AEE_Utility.getLineId(startPoint.X, startPoint.Y, 0, endPoint.X, endPoint.Y, 0, false);
                        AEE_Utility.AttachXData(wallPanelLineId, xDataRegAppName, CommonModule.xDataAsciiName);

                        var wallPanelRect_Id = AEE_Utility.createRectangle(startPoint, endPoint, offstEndPoint, offsetStartPoint, CommonModule.wallPanelLyrName, CommonModule.wallPanelLyrColor);
                        AEE_Utility.AttachXData(wallPanelRect_Id, xDataRegAppName, CommonModule.xDataAsciiName);

                        var colonWallPanelLineId = AEE_Utility.createColonEntityInSamePoint(wallPanelLineId, false);
                        var colonWallPanelRectId = AEE_Utility.createColonEntityInSamePoint(wallPanelRect_Id, true);

                        listOfWallPanelRect_ObjId_ForBeamLayout.Add(colonWallPanelRectId);
                        listOfWallPanelLine_ObjId_ForBeamLayout.Add(colonWallPanelLineId);

                        listOfMoveId.Add(wallPanelLineId);
                        listOfMoveId.Add(wallPanelRect_Id);

                        List<Point2d> listOfPanelRectVertex = AEE_Utility.GetPolylineVertexPoint(wallPanelRect_Id);

                        double minX = listOfPanelRectVertex.Min(sortPoint => sortPoint.X);
                        double minY = listOfPanelRectVertex.Min(sortPoint => sortPoint.Y);
                        double maxX = listOfPanelRectVertex.Max(sortPoint => sortPoint.X);
                        double maxY = listOfPanelRectVertex.Max(sortPoint => sortPoint.Y);

                        double centerX = minX + ((maxX - minX) / 2);
                        double centerY = minY + ((maxY - minY) / 2);
                        Point3d dimTextPoint = new Point3d(centerX, centerY, 0);

                        WallPanelHelper wallPanelHlp = new WallPanelHelper();

                        double windowTopPanelHght1 = 0;
                        double windowTopPanelHght2 = 0;

                        if (wallXHeight > PanelLayout_UI.standardPanelWidth)
                        {
                            windowTopPanelHght1 = PanelLayout_UI.standardPanelWidth;
                            windowTopPanelHght2 = wallXHeight - windowTopPanelHght1;
                            var lst = getListOfHorzPanelLength(wallXHeight, PanelLayout_UI.standardPanelWidth, PanelLayout_UI.minWidthOfPanel);
                            if (wallXHeight <= (PanelLayout_UI.standardPanelWidth + PanelLayout_UI.minWidthOfPanel))
                            {
                                windowTopPanelHght1 = PanelLayout_UI.minHeightOfPanel;
                                windowTopPanelHght2 = wallXHeight - PanelLayout_UI.minHeightOfPanel;
                            }
                            Point3d wallPanel_X_MidPoint = wallPanelHlp.getTextPosition_HeightOfWallX(listOfPanelRectVertex, lineAngle, wallPanelRect_Id, wallPnlLngth);

                            string wallTopPanelText = WallPanelHelper.getWallTopPanelText(wallPnlLngth, windowTopPanelHght2, wallXHeightText);

                            var dimTextId2 = wallPanelHlp.writeDimensionTextInWallPanel(wallTopPanelText, wallPanelLineId, wallPanel_X_MidPoint, lineAngle, layerName, layerColor);
                            listOfMoveId.Add(dimTextId2);

                            string wallTopDescp = wallTopPanelText;
                            string itemCode_Top = wallXHeightText;

                            // RTJ 10-06-2021 start
                            if (windowLayer != "")
                            {
                                WallPanelHelper.listOfWindowLineIdAndLayerName.Add(wallPanelLineId, windowLayer); //RTJ 10-06-2021
                            }
                            // RTJ 10-06-2021 end

                            //Fix the horizontal Door Top Panel by SDM 22-06-2022
                            //wallPanelHlp.setBOMDataOfWallPanel(dimTextId2, wallPanelRect_Id, wallPanelLineId, wallPnlLngth, windowTopPanelHght2, 0, itemCode_Top, wallTopDescp, wallType,
                            //   (standardWallHeight - windowTopPanelHght2).ToString());
                            //start
                            if (wallType == CommonModule.wndowwallPanelType)
                                wallPanelHlp.setBOMDataOfWallPanel(dimTextId2, wallPanelRect_Id, wallPanelLineId, windowTopPanelHght2, wallPnlLngth, 0, itemCode_Top, wallTopDescp, wallType,
                                    (standardWallHeight - windowTopPanelHght2).ToString());
                            else
                                wallPanelHlp.setBOMDataOfWallPanel(dimTextId2, wallPanelRect_Id, wallPanelLineId, wallPnlLngth, windowTopPanelHght2, 0, itemCode_Top, wallTopDescp, wallType,
                               (standardWallHeight - windowTopPanelHght2).ToString());
                            //end

                            var circleId = wallPanelHlp.createCircleInNonStandardWallPanel(dimTextPoint.X, dimTextPoint.Y, wallPnlLngth);
                            if (circleId.IsValid == true)
                            {
                                listOfMoveId.Add(circleId);
                            }
                        }
                        else
                        {
                            windowTopPanelHght1 = wallXHeight;
                        }

                        AEE_Utility.AttachXData(wallPanelLineId, xDataRegAppName + "_1",
                            windowTopPanelHght1.ToString() + "," + (standardWallHeight - windowTopPanelHght1).ToString() +
                            (windowTopPanelHght2 > 0 ? "," + windowTopPanelHght2.ToString() + "," + (standardWallHeight - windowTopPanelHght1 - windowTopPanelHght2).ToString()
                                                     : ""));

                        string wallTopPanelText2 = WallPanelHelper.getWallTopPanelText(wallPnlLngth, windowTopPanelHght1, wallXHeightText);
                        var dimTextId1 = wallPanelHlp.writeDimensionTextInWallPanel(wallTopPanelText2, wallPanelLineId, dimTextPoint, lineAngle, layerName, layerColor);
                        listOfMoveId.Add(dimTextId1);

                        string itemCode = wallXHeightText;
                        string wallDescp = wallTopPanelText2;

                        //RTJ Start 10-06-2021
                        if (windowLayer != "")
                        {
                            if (!WallPanelHelper.listOfWindowLineIdAndLayerName.ContainsKey(wallPanelLineId))
                            {
                                WallPanelHelper.listOfWindowLineIdAndLayerName.Add(wallPanelLineId, windowLayer); //RTJ 10-06-2021
                            }
                        }
                        //RTJ end 10-06-2021



                        //Fix the horizontal Door Top Panel by SDM 22-06-2022

                        //wallPanelHlp.setBOMDataOfWallPanel(dimTextId1, wallPanelRect_Id, wallPanelLineId, wallPnlLngth, windowTopPanelHght1, 0, itemCode, wallDescp, wallType,
                        //(standardWallHeight - windowTopPanelHght1 - windowTopPanelHght2).ToString());
                        //start
                        if (wallType == CommonModule.wndowwallPanelType)
                            wallPanelHlp.setBOMDataOfWallPanel(dimTextId1, wallPanelRect_Id, wallPanelLineId, windowTopPanelHght1, wallPnlLngth, 0, itemCode, wallDescp, wallType,
                            (standardWallHeight - windowTopPanelHght1 - windowTopPanelHght2).ToString());
                        else
                            wallPanelHlp.setBOMDataOfWallPanel(dimTextId1, wallPanelRect_Id, wallPanelLineId, wallPnlLngth, windowTopPanelHght1, 0, itemCode, wallDescp, wallType,
                          (standardWallHeight - windowTopPanelHght1 - windowTopPanelHght2).ToString());
                        //end

                        AEE_Utility.MoveEntity(listOfMoveId, CreateShellPlanHelper.moveVector_ForWindowDoorLayout);
                    }
                    startPoint = endPoint;
                    offsetStartPoint = offstEndPoint;
                }

                BeamHelper beamHlp = new BeamHelper();
                beamHlp.drawBeamWallPanel(sunkanSlabId, beamLayerName, beamId, distanceBtwWallToBeam, nearestBeamWallLineId, listOfWallPanelRect_ObjId_ForBeamLayout, listOfWallPanelLine_ObjId_ForBeamLayout, doorPanelLine_ObjId);

                SunkanSlabHelper sunkanSlabHlp = new SunkanSlabHelper();
                sunkanSlabHlp.drawSunkanSlabWallPanel(flagOfSunkanSlab, sunkanSlabId, listOfWallPanelRect_ObjId_ForBeamLayout, listOfWallPanelLine_ObjId_ForBeamLayout, doorPanelLine_ObjId);

                AEE_Utility.deleteEntity(listOfWallPanelRect_ObjId_ForBeamLayout);
                AEE_Utility.deleteEntity(listOfWallPanelLine_ObjId_ForBeamLayout);
            }
        }


        private void drawWindowPanels(string xDataRegAppName, bool flagOfSunkanSlab, ObjectId sunkanSlabId, string beamLayerName, ObjectId beamId, double distanceBtwDoorToBeam,
            ObjectId nrstBtwDoorToBeamWallLineId, ObjectId windowPanelLine_ObjId, ObjectId windowPanelOffsetLine_ObjId, double windowWall_Height, double windowWall_X_Height,
            ObjectId parapetId, out List<ObjectId> listOfWallPanelRect_Id, out List<ObjectId> listOfTextId, out List<ObjectId> listOfWallXTextId, bool isHorzLayout, string windowLayer, double elevation = 0d) // RTJ 10-06-2021
        {
            listOfWallPanelRect_Id = new List<ObjectId>();
            listOfTextId = new List<ObjectId>();
            listOfWallXTextId = new List<ObjectId>();
            List<ObjectId> listOfCircleId = new List<ObjectId>();

            CommonModule commonModule = new CommonModule();
            WallPanelHelper wallPanelHlpr = new WallPanelHelper();

            var windowPanelLine = AEE_Utility.GetLine(windowPanelLine_ObjId);
            var listOfWindowPanelLineStrtEndPoint = commonModule.getStartEndPointOfLine(windowPanelLine);
            Point3d wndwPanelLineStrtPoint = listOfWindowPanelLineStrtEndPoint[0];
            Point3d wndwPanelLineEndPoint = listOfWindowPanelLineStrtEndPoint[1];

            var windowPanelOffsetLine = AEE_Utility.GetLine(windowPanelOffsetLine_ObjId);
            var listOfWindowPanelOffsetLineStrtEndPoint = commonModule.getStartEndPointOfLine(windowPanelOffsetLine);
            Point3d wndwPanelOffstLineStrtPoint = listOfWindowPanelOffsetLineStrtEndPoint[0];
            Point3d wndwPanelOffstLineEndPoint = listOfWindowPanelOffsetLineStrtEndPoint[1];

            double angleOfLine = AEE_Utility.GetAngleOfLine(wndwPanelLineStrtPoint.X, wndwPanelLineStrtPoint.Y, wndwPanelLineEndPoint.X, wndwPanelLineEndPoint.Y);
            bool flag_X_Axis = false;
            bool flag_Y_Axis = false;

            commonModule.checkAngleOfLine_Axis(angleOfLine, out flag_X_Axis, out flag_Y_Axis);

            double windowTopPanelHght = 0;
            if (isHorzLayout == true)
            {
                double beamBottom = Beam_UI_Helper.getOffsetBeamBottom(beamLayerName);
                bool flagOfTopPanel = false;
                var standardWallHeight = GeometryHelper.getHeightOfWall(windowPanelLine.Layer, PanelLayout_UI.maxHeightOfPanel, InternalWallSlab_UI_Helper.getSlabBottom(windowPanelLine.Layer), beamBottom, PanelLayout_UI.SL, PanelLayout_UI.RC, PanelLayout_UI.getSlabThickness(windowPanelLine.Layer), PanelLayout_UI.kickerHt, PanelLayout_UI.floorHeight, CommonModule.internalCorner, sunkanSlabId, out flagOfTopPanel);

                if (parapetId.IsValid == true)
                {
                    standardWallHeight = ParapetHelper.getParapetWallHeightWithoutLevelDiffOfSunkanSlab(parapetId, sunkanSlabId, windowPanelLine.ObjectId, windowPanelLine.Layer);
                }
                List<Point3d> listOfLineAndOffsetLinePoint = new List<Point3d>();
                listOfLineAndOffsetLinePoint.Add(wndwPanelLineStrtPoint);
                listOfLineAndOffsetLinePoint.Add(wndwPanelLineEndPoint);
                listOfLineAndOffsetLinePoint.Add(wndwPanelOffstLineStrtPoint);
                listOfLineAndOffsetLinePoint.Add(wndwPanelOffstLineEndPoint);

                //drawHorzPanel_ForWindowAndDoor(CommonModule.wndowwallPanelType, xDataRegAppName, standardWallHeight, listOfLineAndOffsetLinePoint, windowWall_X_Height, PanelLayout_UI.windowTopPanelName, CommonModule.wallPanelLyrName, CommonModule.wallPanelLyrColor, flag_X_Axis, flag_Y_Axis, beamLayerName, beamId, distanceBtwDoorToBeam, nrstBtwDoorToBeamWallLineId, windowPanelLine_ObjId, flagOfSunkanSlab, sunkanSlabId, windowLayer); //RTJ 10-06-2021
                drawHorzPanel_ForWindowAndDoor(CommonModule.wndowwallPanelType, xDataRegAppName, standardWallHeight, listOfLineAndOffsetLinePoint, windowWall_X_Height, PanelLayout_UI.beamPanelName, CommonModule.wallPanelLyrName, CommonModule.wallPanelLyrColor, flag_X_Axis, flag_Y_Axis, beamLayerName, beamId, distanceBtwDoorToBeam, nrstBtwDoorToBeamWallLineId, windowPanelLine_ObjId, flagOfSunkanSlab, sunkanSlabId, windowLayer); //RTJ 16-06-2021

                windowTopPanelHght = 0;
            }
            else
            {
                windowTopPanelHght = windowWall_X_Height;
                if (windowTopPanelHght <= 0)
                {
                    windowTopPanelHght = 0;
                }
            }

            double lengthOfLine = AEE_Utility.GetLengthOfLine(wndwPanelLineStrtPoint.X, wndwPanelLineStrtPoint.Y, wndwPanelLineEndPoint.X, wndwPanelLineEndPoint.Y);
            List<double> listOfWallPanelLength = wallPanelHlpr.getListOfWallPanelLength(lengthOfLine, PanelLayout_UI.standardPanelWidth, PanelLayout_UI.minWidthOfPanel);

            List<ObjectId> listOfWallPanelLine_ObjId = new List<ObjectId>();
            listOfWallPanelRect_Id = wallPanelHlpr.drawWallPanels(xDataRegAppName, windowPanelLine_ObjId, wndwPanelLineStrtPoint, wndwPanelLineEndPoint, wndwPanelOffstLineStrtPoint, wndwPanelOffstLineEndPoint, flag_X_Axis, flag_Y_Axis, listOfWallPanelLength, out listOfWallPanelLine_ObjId);

            string windowWallPanelNameWithRC = "";
            double standardWindowWall_Height = 0;

            //double levelDifferenceOfSunkanSlb = SunkanSlabHelper.getSunkanSlabLevelDifference_WallPanel(sunkanSlabId);
            //double RC_WithLevelDiff = levelDifferenceOfSunkanSlb + PanelLayout_UI.RC;
            double RC_WithLevelDiff = PanelLayout_UI.RC;

            WallPanelHelper.getWallPanelHeight_PanelWithRC(sunkanSlabId, PanelLayout_UI.flagOfPanelWithRC, windowWall_Height, PanelLayout_UI.windowPanelName, PanelLayout_UI.RC, windowPanelLine.Layer, out standardWindowWall_Height, out windowWallPanelNameWithRC);
            if (parapetId.IsValid == true)
            {
                standardWindowWall_Height = ParapetHelper.getParapetWallHeight(parapetId, sunkanSlabId, windowPanelLine.ObjectId, windowPanelLine.Layer);
                windowTopPanelHght = 0;
            }
            string doorLayer = "";  //RTJ 10-06-2021
            wallPanelHlpr.writeTextInWallPanel(CommonModule.wndowwallPanelType, sunkanSlabId, windowPanelLine.Layer, listOfWallPanelRect_Id, listOfWallPanelLine_ObjId, standardWindowWall_Height, windowWallPanelNameWithRC, windowTopPanelHght, PanelLayout_UI.windowTopPanelName, RC_WithLevelDiff, CommonModule.wallPanelTextLyrName, CommonModule.wallPanelLyrColor, true, out listOfTextId, out listOfWallXTextId, out listOfCircleId, doorLayer, windowLayer, elevation); // RTJ 10-06-2021

            if (isHorzLayout == false)
            {
                BeamHelper beamHlp = new BeamHelper();
                beamHlp.drawBeamWallPanel(sunkanSlabId, beamLayerName, beamId, distanceBtwDoorToBeam, nrstBtwDoorToBeamWallLineId, listOfWallPanelRect_Id, listOfWallPanelLine_ObjId, windowPanelLine_ObjId);

                SunkanSlabHelper internalSunkanSlabHelper = new SunkanSlabHelper();
                internalSunkanSlabHelper.drawSunkanSlabWallPanel(flagOfSunkanSlab, sunkanSlabId, listOfWallPanelRect_Id, listOfWallPanelLine_ObjId, windowPanelLine_ObjId);
            }
        }


        internal void DrawHorizontalPanel_ForWindowAndDoor(string wallType, string xDataRegAppName, double standardWallHeight, List<Point3d> listOfLineAndOffsetLinePoint, double wallXHeight, string wallXHeightText, string layerName, int layerColor, bool flag_X_Axis, bool flag_Y_Axis, string beamLayerName, ObjectId beamId, double distanceBtwWallToBeam, ObjectId nearestBeamWallLineId, ObjectId doorPanelLine_ObjId, bool flagOfSunkanSlab, ObjectId sunkanSlabId, string windowLayer)  //RTJ 10-06-2021
        {
            Point3d baseStrtPoint = listOfLineAndOffsetLinePoint[0];
            Point3d baseEndPoint = listOfLineAndOffsetLinePoint[1];
            Point3d offsetBaseStrtPoint = listOfLineAndOffsetLinePoint[2];
            Point3d offsetBaseEndPoint = listOfLineAndOffsetLinePoint[3];

            BOMHelper bomHlp = new BOMHelper();

            List<ObjectId> listOfWallPanelRect_ObjId_ForBeamLayout = new List<ObjectId>();
            List<ObjectId> listOfWallPanelLine_ObjId_ForBeamLayout = new List<ObjectId>();

            double lineAngle = AEE_Utility.GetAngleOfLine(baseStrtPoint.X, baseStrtPoint.Y, baseEndPoint.X, baseEndPoint.Y);
            double lengthOfLine = AEE_Utility.GetLengthOfLine(baseStrtPoint.X, baseStrtPoint.Y, baseEndPoint.X, baseEndPoint.Y);
            lengthOfLine = Math.Round(lengthOfLine);

            if (lengthOfLine != 0)
            {
                List<double> listOfWallPanelLength = getListOfHorzPanelLength(lengthOfLine, PanelLayout_UI.maxHeightOfWindowAndDoor/*standardWallHeight*/, PanelLayout_UI.minWidthOfPanel);
                Point3d startPoint = baseStrtPoint;
                Point3d offsetStartPoint = offsetBaseStrtPoint;

                for (int k = 0; k < listOfWallPanelLength.Count; k++)
                {
                    double wallPnlLngth = listOfWallPanelLength[k];
                    var point = AEE_Utility.get_XY(lineAngle, wallPnlLngth);
                    Point3d endPoint = new Point3d((startPoint.X + point.X), (startPoint.Y + point.Y), 0);
                    Point3d offstEndPoint = new Point3d((offsetStartPoint.X + point.X), (offsetStartPoint.Y + point.Y), 0);

                    double length = 0;
                    if (flag_X_Axis == true)
                    {
                        length = Math.Truncate(Math.Abs(startPoint.X - endPoint.X));
                    }
                    else if (flag_Y_Axis == true)
                    {
                        length = Math.Truncate(Math.Abs(startPoint.Y - endPoint.Y));
                    }
                    if (length != 0)
                    {
                        List<ObjectId> listOfMoveId = new List<ObjectId>();
                        var wallPanelLineId = AEE_Utility.getLineId(startPoint.X, startPoint.Y, 0, endPoint.X, endPoint.Y, 0, false);
                        AEE_Utility.AttachXData(wallPanelLineId, xDataRegAppName, CommonModule.xDataAsciiName);

                        var wallPanelRect_Id = AEE_Utility.createRectangle(startPoint, endPoint, offstEndPoint, offsetStartPoint, CommonModule.wallPanelLyrName, CommonModule.wallPanelLyrColor);
                        AEE_Utility.AttachXData(wallPanelRect_Id, xDataRegAppName, CommonModule.xDataAsciiName);

                        var colonWallPanelLineId = AEE_Utility.createColonEntityInSamePoint(wallPanelLineId, false);
                        var colonWallPanelRectId = AEE_Utility.createColonEntityInSamePoint(wallPanelRect_Id, true);

                        listOfWallPanelRect_ObjId_ForBeamLayout.Add(colonWallPanelRectId);
                        listOfWallPanelLine_ObjId_ForBeamLayout.Add(colonWallPanelLineId);

                        listOfMoveId.Add(wallPanelLineId);
                        listOfMoveId.Add(wallPanelRect_Id);

                        List<Point2d> listOfPanelRectVertex = AEE_Utility.GetPolylineVertexPoint(wallPanelRect_Id);

                        double minX = listOfPanelRectVertex.Min(sortPoint => sortPoint.X);
                        double minY = listOfPanelRectVertex.Min(sortPoint => sortPoint.Y);
                        double maxX = listOfPanelRectVertex.Max(sortPoint => sortPoint.X);
                        double maxY = listOfPanelRectVertex.Max(sortPoint => sortPoint.Y);

                        double centerX = minX + ((maxX - minX) / 2);
                        double centerY = minY + ((maxY - minY) / 2);
                        Point3d dimTextPoint = new Point3d(centerX, centerY, 0);

                        WallPanelHelper wallPanelHlp = new WallPanelHelper();

                        double windowTopPanelHght1 = 0;
                        double windowTopPanelHght2 = 0;


                        if (wallXHeight > PanelLayout_UI.standardPanelWidth)
                        {
                            windowTopPanelHght1 = PanelLayout_UI.standardPanelWidth;
                            windowTopPanelHght2 = wallXHeight - windowTopPanelHght1;
                            var lst = getListOfHorzPanelLength(wallXHeight, PanelLayout_UI.standardPanelWidth, PanelLayout_UI.minWidthOfPanel);
                            if (wallXHeight <= (PanelLayout_UI.standardPanelWidth + PanelLayout_UI.minWidthOfPanel))
                            {
                                windowTopPanelHght1 = PanelLayout_UI.minHeightOfPanel;
                                windowTopPanelHght2 = wallXHeight - PanelLayout_UI.minHeightOfPanel;
                            }
                            Point3d wallPanel_X_MidPoint = wallPanelHlp.getTextPosition_HeightOfWallX(listOfPanelRectVertex, lineAngle, wallPanelRect_Id, wallPnlLngth);

                            string wallTopPanelText = WallPanelHelper.getWallTopPanelText(wallPnlLngth, windowTopPanelHght2, wallXHeightText);

                            var dimTextId2 = wallPanelHlp.writeDimensionTextInWallPanel(wallTopPanelText, wallPanelLineId, wallPanel_X_MidPoint, lineAngle, layerName, layerColor);
                            listOfMoveId.Add(dimTextId2);

                            string wallTopDescp = wallTopPanelText;
                            string itemCode_Top = wallXHeightText;
                            wallPanelHlp.setBOMDataOfWallPanel(dimTextId2, wallPanelRect_Id, wallPanelLineId, windowTopPanelHght2, wallPnlLngth, 0, itemCode_Top, wallTopDescp, wallType,
                                (standardWallHeight - windowTopPanelHght2).ToString());

                            var circleId = wallPanelHlp.createCircleInNonStandardWallPanel(dimTextPoint.X, dimTextPoint.Y, wallPnlLngth);
                            if (circleId.IsValid == true)
                            {
                                listOfMoveId.Add(circleId);
                            }
                        }
                        else
                        {
                            windowTopPanelHght1 = wallXHeight;
                        }

                        AEE_Utility.AttachXData(wallPanelLineId, xDataRegAppName + "_1",
                            windowTopPanelHght1.ToString() + "," + (standardWallHeight - windowTopPanelHght1).ToString() +
                            (windowTopPanelHght2 > 0 ? "," + windowTopPanelHght2.ToString() + "," + (standardWallHeight - windowTopPanelHght1 - windowTopPanelHght2).ToString()
                                                     : ""));

                        string wallTopPanelText2 = WallPanelHelper.getWallTopPanelText(wallPnlLngth, windowTopPanelHght1, wallXHeightText);
                        var dimTextId1 = wallPanelHlp.writeDimensionTextInWallPanel(wallTopPanelText2, wallPanelLineId, dimTextPoint, lineAngle, layerName, layerColor);
                        listOfMoveId.Add(dimTextId1);

                        string itemCode = wallXHeightText;
                        string wallDescp = wallTopPanelText2;
                        wallPanelHlp.setBOMDataOfWallPanel(dimTextId1, wallPanelRect_Id, wallPanelLineId, windowTopPanelHght1, wallPnlLngth, 0, itemCode, wallDescp, wallType,
                            (standardWallHeight - windowTopPanelHght1 - windowTopPanelHght2).ToString());

                        AEE_Utility.MoveEntity(listOfMoveId, CreateShellPlanHelper.moveVector_ForWindowDoorLayout);
                    }
                    startPoint = endPoint;
                    offsetStartPoint = offstEndPoint;
                }

                BeamHelper beamHlp = new BeamHelper();
                beamHlp.drawBeamWallPanel(sunkanSlabId, beamLayerName, beamId, distanceBtwWallToBeam, nearestBeamWallLineId, listOfWallPanelRect_ObjId_ForBeamLayout, listOfWallPanelLine_ObjId_ForBeamLayout, doorPanelLine_ObjId);

                SunkanSlabHelper sunkanSlabHlp = new SunkanSlabHelper();
                sunkanSlabHlp.drawSunkanSlabWallPanel(flagOfSunkanSlab, sunkanSlabId, listOfWallPanelRect_ObjId_ForBeamLayout, listOfWallPanelLine_ObjId_ForBeamLayout, doorPanelLine_ObjId);

                AEE_Utility.deleteEntity(listOfWallPanelRect_ObjId_ForBeamLayout);
                AEE_Utility.deleteEntity(listOfWallPanelLine_ObjId_ForBeamLayout);
            }
        }


        public List<double> getListOfHorzPanelLength(double lengthOfLine, double standardWallHeight, double minWidthOfPanel)
        {
            List<double> listOfWallPanelLength = new List<double>();
            if (lengthOfLine > standardWallHeight)
            {
                double qtyOfPanelWall = lengthOfLine / standardWallHeight;
                double qtyOfStandardPanelWall = Math.Truncate(qtyOfPanelWall);
                if ((qtyOfPanelWall % 1) != 0)
                {
                    double totalLength = qtyOfStandardPanelWall * standardWallHeight;
                    double difference = Math.Abs(lengthOfLine - totalLength);
                    double subtractLength = (standardWallHeight + difference) - minWidthOfPanel;
                    if (difference < minWidthOfPanel)
                    {
                        for (int i = 0; i < qtyOfStandardPanelWall; i++)
                        {
                            if (i == (qtyOfStandardPanelWall - 1))
                            {
                                listOfWallPanelLength.Add((subtractLength));
                            }
                            else
                            {
                                listOfWallPanelLength.Add(standardWallHeight);
                            }
                        }
                        listOfWallPanelLength.Add(minWidthOfPanel);
                    }
                    else
                    {
                        for (int i = 0; i < qtyOfStandardPanelWall; i++)
                        {
                            listOfWallPanelLength.Add(standardWallHeight);
                        }
                        listOfWallPanelLength.Add(difference);
                    }
                }
                else
                {
                    for (int i = 0; i < qtyOfStandardPanelWall; i++)
                    {
                        listOfWallPanelLength.Add(standardWallHeight);
                    }
                }
            }
            else
            {
                listOfWallPanelLength.Add(lengthOfLine);
            }
            return listOfWallPanelLength;
        }


        private void getWindowWallText(string windowLayer, string beamLayer, ObjectId windowPanelLineId, ObjectId parapetId, ObjectId sunkanSlabId, out double windowWallHeight, out double windowWallXHeight)
        {
            windowWallHeight = 0;
            windowWallXHeight = 0;
            Entity windowPanelLineEnt = AEE_Utility.GetEntityForRead(windowPanelLineId);
            double windowLintelLevel = Window_UI_Helper.getWindowLintelLevel(windowLayer);
            double beamBottom = Beam_UI_Helper.getOffsetBeamBottom(beamLayer);
            if (InternalWallSlab_UI_Helper.IsInternalWall(windowPanelLineEnt.Layer) || StairCase_UI_Helper.checkStairCaseLayerIsExist(windowPanelLineEnt.Layer))
            {
                windowWallHeight = Window_UI_Helper.getWindowWallHeight(windowLayer, PanelLayout_UI.RC, PanelLayout_UI.flagOfPanelWithRC, sunkanSlabId);
                windowWallXHeight = GeometryHelper.getHeightOfWindowWallPanelX_InternalWall(InternalWallSlab_UI_Helper.getSlabBottom(windowPanelLineEnt.Layer), beamBottom, windowLintelLevel, PanelLayout_UI.SL, CommonModule.internalCorner, sunkanSlabId);
            }
            else if (windowPanelLineEnt.Layer == CommonModule.externalWallLayerName || windowPanelLineEnt.Layer == CommonModule.ductLayerName || Lift_UI_Helper.checkLiftLayerIsExist(windowPanelLineEnt.Layer))
            {
                windowWallHeight = Window_UI_Helper.getWindowWallHeight(windowLayer, PanelLayout_UI.RC, PanelLayout_UI.flagOfPanelWithRC && (windowPanelLineEnt.Layer != CommonModule.externalWallLayerName), sunkanSlabId);
                windowWallXHeight = GeometryHelper.getHeightOfWindowWallPanelX_ExternalWall(InternalWallSlab_UI_Helper.getSlabBottom(windowPanelLineEnt.Layer), beamBottom, PanelLayout_UI.getSlabThickness(windowPanelLineEnt.Layer), windowLintelLevel, PanelLayout_UI.kickerHt, PanelLayout_UI.RC);
            }

            if (parapetId.IsValid == true)
            {
                windowWallHeight = ParapetHelper.getParapetWallHeightWithoutLevelDiffOfSunkanSlab(parapetId, sunkanSlabId, windowPanelLineEnt.ObjectId, windowPanelLineEnt.Layer);
                windowWallXHeight = 0;
            }
        }


        public static bool getWallIdFromName(string name, out ObjectId wallObId)
        {
            int wallId, roomId;
            bool intr, ext, lift, stair,duct, ret;
            ret = ElevationHelper.getWallDetails(name, out wallId, out roomId, out intr, out ext, out lift, out stair,out duct);
            wallObId = new ObjectId();

            if (ret)
            {
                if (ext)
                    wallObId = ExternalWallHelper.listOfListOfExternalWallRoomLineId_With_WallName[roomId - 1][wallId - 1];
                if (intr)
                    wallObId = InternalWallHelper.listOfListOfInternalWallRoomLineId_With_WallName[roomId - 1][wallId - 1];
                if (lift)
                    wallObId = LiftHelper.listOfListOfLiftRoomLineId_With_WallName[roomId - 1][wallId - 1];
                if (stair)
                    wallObId = StairCaseHelper.listOfListOfStaircaseRoomLineId_With_WallName[roomId - 1][wallId - 1];
                if(duct)
                    wallObId= DuctHelper.listOfListOfDuctRoomLineId_With_WallName[roomId - 1][wallId - 1];
            }

            return ret;
        }


        public void GetWindowDoorTopPanelOffset(ObjectId windowObjId, List<ObjectId> listOfMinLengthwindowObjExplodeLines, string wallPanelType, out Tuple<List<Point3d>, List<double>> minLengthExplodeLinePoint3dIdWithBottomPanelCornerDist)
        {

            var wallEnt = AEE_Utility.GetEntityForWrite(windowObjId);
            var plPts = new List<Point3d>();
            if (wallEnt is Polyline)
            {
                AEE_Utility.GetPolylinePoints(wallEnt as Polyline, plPts);
            }

            bool isInside = false;
            List<bool> listOfwindowCornersInsideBeam = new List<bool>();
            List<ObjectId> listOfOffsetBeamCornerID = new List<ObjectId>();

            List<Point3d> listOfPoint3dForDist = new List<Point3d>();
            List<double> listOfDistForPoint3d = new List<double>();


            double beamBottomLevel = -1;
            double windowDoorLintelLevel = -1;

            if (wallPanelType == CommonModule.wndowwallPanelType)
                windowDoorLintelLevel = Window_UI_Helper.getWindowLintelLevel(wallEnt.Layer);
            if (wallEnt.Layer.Contains(Beam_UI_Helper.beamStrtText))//Added on 19/03/2023 by SDM for "Beam" implementation
                windowDoorLintelLevel = Beam_UI_Helper.GetBeamLintelLevel(wallEnt.Layer, AEE_Utility.GetXDataRegisterAppName(windowObjId));
            else
                windowDoorLintelLevel = Door_UI_Helper.getDoorLintelLevel(wallEnt.Layer);

            foreach (var item in BeamHelper.listOfAllOffsetBeamCornerIdsWithBeamID)
            {
                isInside = false;
                listOfwindowCornersInsideBeam.Clear();
                foreach (Point3d point in plPts)
                {
                    isInside = AEE_Utility.GetPointIsInsideThePolyline(item.Item1, point);
                    listOfwindowCornersInsideBeam.Add(isInside);

                }
                if (listOfwindowCornersInsideBeam.Contains(true))
                {
                    beamBottomLevel = Beam_UI_Helper.getOffsetBeamBottom(item.Item5);
                    listOfOffsetBeamCornerID = item.Item2;
                    break;
                }
            }

            // Check Beam Bottom Level and Window/Door Lintel level only if beam is available

            if (windowDoorLintelLevel == beamBottomLevel)
            {

                List<ObjectId> listOfMovedOffsetBeamCornerID = new List<ObjectId>();

                foreach (ObjectId cornerId in listOfOffsetBeamCornerID)
                {
                    // Move beam CornerIds from Beam Layout to the new Shell Plan
                    listOfMovedOffsetBeamCornerID.Add(AEE_Utility.copyColonEntity(new Vector3d(CreateShellPlanHelper.moveVector_ForBeamLayout.X * -1, CreateShellPlanHelper.moveVector_ForBeamLayout.Y * -1, CreateShellPlanHelper.moveVector_ForBeamLayout.Z * -1), cornerId, true));
                }


                foreach (ObjectId lineObjID in listOfMinLengthwindowObjExplodeLines)
                {

                    foreach (ObjectId cornerId in listOfMovedOffsetBeamCornerID)
                    {
                        // Create  Rectangle for CornerId Object

                        List<Point3d> cornerIdBoundingBox = AEE_Utility.GetBoundingBoxOfPolyline(cornerId);
                        ObjectId cornerIdRecObjectID = AEE_Utility.createRectangle(cornerIdBoundingBox[0].X, cornerIdBoundingBox[0].Y, cornerIdBoundingBox[1].X, cornerIdBoundingBox[1].Y, AEE_Utility.GetLayerName(cornerId), 256);

                        List<Point3d> intersectPoints = AEE_Utility.InterSectionPointBetweenPolyLineAndLine(cornerIdRecObjectID, lineObjID);

                        ObjectId offsetLineId = AEE_Utility.OffsetLine(lineObjID, 1, false);

                        Point3d offsetLineMidPoint3d = AEE_Utility.GetMidPoint(offsetLineId);

                        bool isInsideWindow = AEE_Utility.GetPointIsInsideThePolyline(windowObjId, offsetLineMidPoint3d);

                        if (!isInsideWindow)
                        {
                            AEE_Utility.deleteEntity(offsetLineId);
                            offsetLineId = AEE_Utility.OffsetLine(lineObjID, -1, false);
                        }

                        List<Point3d> offsetLineintersectPoints = AEE_Utility.InterSectionPointBetweenPolyLineAndLine(cornerIdRecObjectID, offsetLineId);
                        AEE_Utility.deleteEntity(offsetLineId);

                        if (intersectPoints.Count > 0 && offsetLineintersectPoints.Count > 0)
                        {
                            List<ObjectId> listOfExplodeId = AEE_Utility.ExplodeEntity(cornerIdRecObjectID);

                            List<Point3d> templistOfPoint3dForDist = new List<Point3d>();
                            List<double> listOfLineClosestPointDist = new List<double>();

                            Line line1 = AEE_Utility.GetLine(lineObjID);

                            double windowLineObjAngle = AEE_Utility.GetAngleOfLine(line1);

                            foreach (ObjectId cornerIdLine in listOfExplodeId)
                            {
                                Line line2 = AEE_Utility.GetLine(cornerIdLine);

                                double cornerLineObjAngle = Math.Round(AEE_Utility.GetAngleOfLine(line2));

                                if (Math.Abs(windowLineObjAngle - cornerLineObjAngle) == 90 || Math.Abs(windowLineObjAngle - cornerLineObjAngle) == 270)
                                {
                                    PointOnCurve3d[] pointOnCurve3d = line2.GetGeCurve().GetClosestPointTo(line1.GetGeCurve());

                                    double distWithStart = AEE_Utility.GetLengthOfLine(pointOnCurve3d.Last().Point, line1.StartPoint);
                                    double distWithEnd = AEE_Utility.GetLengthOfLine(pointOnCurve3d.Last().Point, line1.EndPoint);

                                    if (distWithStart < distWithEnd)
                                        templistOfPoint3dForDist.Add(line1.StartPoint);
                                    else
                                        templistOfPoint3dForDist.Add(line1.EndPoint);

                                    double closestPtDist = AEE_Utility.GetDistanceBetweenTwoLines(line1.ObjectId, line2.ObjectId);
                                    listOfLineClosestPointDist.Add(closestPtDist);
                                }
                            }

                            AEE_Utility.deleteEntity(listOfExplodeId);
                            if (listOfLineClosestPointDist.Any())
                            {
                                int matchIndx = -1;
                                bool flagmatch = AEE_Utility.IsPoint3dPresentInPoint3dList(listOfPoint3dForDist, templistOfPoint3dForDist.First(), out matchIndx);
                                if (flagmatch)
                                {
                                    if (listOfLineClosestPointDist.Max() < listOfDistForPoint3d[matchIndx])
                                    {
                                        listOfDistForPoint3d[matchIndx] = listOfLineClosestPointDist.Max();
                                    }
                                }
                                else
                                {
                                    listOfDistForPoint3d.Add(listOfLineClosestPointDist.Max());
                                    listOfPoint3dForDist.Add(templistOfPoint3dForDist.First());
                                }
                            }
                        }
                        AEE_Utility.deleteEntity(cornerIdRecObjectID);
                    }
                }


                foreach (ObjectId cornerId in listOfMovedOffsetBeamCornerID)
                {
                    AEE_Utility.deleteEntity(cornerId);
                }

                minLengthExplodeLinePoint3dIdWithBottomPanelCornerDist = new Tuple<List<Point3d>, List<double>>(listOfPoint3dForDist, listOfDistForPoint3d);

            }
            else
            {
                // Implement same logic of bottom panel
                GetWindowDoorBottomPanelOffset(windowObjId, listOfMinLengthwindowObjExplodeLines, out minLengthExplodeLinePoint3dIdWithBottomPanelCornerDist);

            }
        }


        private bool CheckCornerIdBasePtMatchWithAnyOfWindowOrDoorObjId(ObjectId windowOrDoorObjId, ObjectId cornerId)
        {
            bool flag = false;
            Point3d cornerbasePt = AEE_Utility.GetBasePointOfPolyline(cornerId);
            List<Point3d> listOfPloylinePts = AEE_Utility.GetPolyLinePointList(windowOrDoorObjId);

            foreach (Point3d point in listOfPloylinePts)
            {
                //some cases window and corner pts are not matching thats why we are considering length/gap b/w points less than 1 mm.
                double gapBwPts = AEE_Utility.GetLengthOfLine(cornerbasePt, point);
                if (gapBwPts <= 1)
                //if (AEE_Utility.IsPointsAreEqual(cornerbasePt,point))
                {
                    flag = true;
                    break;
                }
            }

            return flag;
        }
        public void GetWindowDoorBottomPanelOffset(ObjectId windowObjId, List<ObjectId> listOfMinLengthwindowObjExplodeLines, out Tuple<List<Point3d>, List<double>> minLengthExplodeLinePoint3dIdWithBottomPanelCornerDist)
        {


            var wallEnt = AEE_Utility.GetEntityForWrite(windowObjId);
            var plPts = new List<Point3d>();
            if (wallEnt is Polyline)
            {
                AEE_Utility.GetPolylinePoints(wallEnt as Polyline, plPts);
            }

            bool isInside = false;

            List<Point3d> listOfPoint3dForDist = new List<Point3d>();
            List<double> listOfDistForPoint3d = new List<double>();


            AEE_Utility.MoveEntity(DoorHelper.listOfMoveCornerIds_In_New_ShellPlan, CreateShellPlanHelper.moveVector_ForWindowDoorLayout * -1);

            foreach (ObjectId lineObjID in listOfMinLengthwindowObjExplodeLines)
            {
                foreach (ObjectId cornerId in CornerHelper.listOfInternalCornerId_ForStretch)
                {
                    // Create  Rectangle for CornerId Object

                    if (CheckCornerIdBasePtMatchWithAnyOfWindowOrDoorObjId(windowObjId, cornerId))
                    {
                        List<Point3d> cornerIdBoundingBox = AEE_Utility.GetBoundingBoxOfPolyline(cornerId);
                        ObjectId cornerIdRecObjectID = AEE_Utility.createRectangle(cornerIdBoundingBox[0].X, cornerIdBoundingBox[0].Y, cornerIdBoundingBox[1].X, cornerIdBoundingBox[1].Y, AEE_Utility.GetLayerName(cornerId), 256);

                        List<Point3d> intersectPoints = AEE_Utility.InterSectionPointBetweenPolyLineAndLine(cornerIdRecObjectID, lineObjID);

                        ObjectId offsetLineId = AEE_Utility.OffsetLine(lineObjID, 1, true);

                        Point3d offsetLineMidPoint3d = AEE_Utility.GetMidPoint(offsetLineId);

                        bool isInsideWindow = AEE_Utility.GetPointIsInsideThePolyline(windowObjId, offsetLineMidPoint3d);

                        if (!isInsideWindow)
                        {
                            AEE_Utility.deleteEntity(offsetLineId);
                            offsetLineId = AEE_Utility.OffsetLine(lineObjID, -1, true);
                        }

                        List<Point3d> offsetLineintersectPoints = AEE_Utility.InterSectionPointBetweenPolyLineAndLine(cornerIdRecObjectID, offsetLineId);
                        AEE_Utility.deleteEntity(offsetLineId);

                        if (intersectPoints.Count > 0 && offsetLineintersectPoints.Count > 0)
                        {
                            List<ObjectId> listOfExplodeId = AEE_Utility.ExplodeEntity(cornerIdRecObjectID);

                            List<Point3d> templistOfPoint3dForDist = new List<Point3d>();
                            List<double> listOfLineClosestPointDist = new List<double>();

                            Line line1 = AEE_Utility.GetLine(lineObjID);

                            double windowLineObjAngle = Math.Round(AEE_Utility.GetAngleOfLine(line1));

                            foreach (ObjectId cornerIdLine in listOfExplodeId)
                            {
                                Line line2 = AEE_Utility.GetLine(cornerIdLine);

                                double cornerLineObjAngle = AEE_Utility.GetAngleOfLine(line2);

                                if (Math.Abs(windowLineObjAngle - cornerLineObjAngle) == 90 || Math.Abs(windowLineObjAngle - cornerLineObjAngle) == 270)
                                {
                                    PointOnCurve3d[] pointOnCurve3d = line1.GetGeCurve().GetClosestPointTo(line2.GetGeCurve());

                                    double distWithStart = AEE_Utility.GetLengthOfLine(pointOnCurve3d.Last().Point, line1.StartPoint);
                                    double distWithEnd = AEE_Utility.GetLengthOfLine(pointOnCurve3d.Last().Point, line1.EndPoint);

                                    if (distWithStart < distWithEnd)
                                        templistOfPoint3dForDist.Add(line1.StartPoint);
                                    else
                                        templistOfPoint3dForDist.Add(line1.EndPoint);

                                    double closestPtDist = AEE_Utility.GetDistanceBetweenTwoLines(line1.ObjectId, line2.ObjectId);
                                    listOfLineClosestPointDist.Add(closestPtDist);
                                }
                            }

                            AEE_Utility.deleteEntity(listOfExplodeId);
                            if (listOfLineClosestPointDist.Any())
                            {
                                listOfDistForPoint3d.Add(listOfLineClosestPointDist.Max());
                                listOfPoint3dForDist.Add(templistOfPoint3dForDist.First());
                            }
                        }
                        AEE_Utility.deleteEntity(cornerIdRecObjectID);
                    }
                }
            }

            AEE_Utility.MoveEntity(DoorHelper.listOfMoveCornerIds_In_New_ShellPlan, CreateShellPlanHelper.moveVector_ForWindowDoorLayout);

            minLengthExplodeLinePoint3dIdWithBottomPanelCornerDist = new Tuple<List<Point3d>, List<double>>(listOfPoint3dForDist, listOfDistForPoint3d);
        }

        public void drawWindowDoorBottomPanel(ObjectId wallObjId, string wallLayerName, ObjectId sunkanSlabId, string wallPanelType,
           Dictionary<Point3d, Tuple<double, bool>> dicCornerPoints)
        {

            var wallEnt = AEE_Utility.GetEntityForWrite(wallObjId);
            var plPts = new List<Point3d>();
            if (wallEnt is Polyline)
            {
                AEE_Utility.GetPolylinePoints(wallEnt as Polyline, plPts);
            }

            var xDataRegAppName = AEE_Utility.GetXDataRegisterAppName(wallObjId);

            var listOfExplodeId = AEE_Utility.ExplodeEntity(wallObjId);
            List<ObjectId> listOfMaxLengthExplodeLine = getMaxLengthOfWindowLineSegment(listOfExplodeId);
            List<ObjectId> listOfMinLengthExplodeLine = getMinLengthOfWindowLineSegment(listOfExplodeId);

            ObjectId maxLengthWallPanelLineId1 = listOfMaxLengthExplodeLine[0];
            double maxLengthWallPanelLineLngth = AEE_Utility.GetLengthOfLine(maxLengthWallPanelLineId1);

            ObjectId maxLengthWallPanelLineId2 = listOfMaxLengthExplodeLine[1];

            ObjectId minLengthWallPanelLineId1 = listOfMinLengthExplodeLine[0];
            double minLengthWallPanelLineLngth = AEE_Utility.GetLengthOfLine(minLengthWallPanelLineId1);
            minLengthWallPanelLineLngth = Math.Round(minLengthWallPanelLineLngth);

            var overlapCorners = new List<Point3d>();

            foreach (var pt in plPts)
            {
                bool flag = false;
                foreach (Point3d pt1 in dicCornerPoints.Keys)
                {
                    //some cases pts are not matching thats why we are considering length/gap b/w points less than 1 mm.
                    double dist = AEE_Utility.GetLengthOfLine(pt1, pt);
                    if (dist <= 1)
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                    continue;
                if (!dicCornerPoints.ContainsKey(pt))
                {
                    continue;
                }
                //if (Math.Abs(dicCornerPoints[pt].Item1 - minLengthWallPanelLineLngth) > 1e-6 || dicCornerPoints[pt].Item2)
                if (Math.Abs(dicCornerPoints[pt].Item1 - minLengthWallPanelLineLngth) > 0.5 || dicCornerPoints[pt].Item2)
                {
                    continue;
                }
                dicCornerPoints[pt] = new Tuple<double, bool>(dicCornerPoints[pt].Item1, true);
                overlapCorners.Add(pt);
            }


            double angleOfMinLngth1Line = AEE_Utility.GetAngleOfLine(minLengthWallPanelLineId1);

            var listStrtEndPointOfMaxLengthWallPanelLine1 = AEE_Utility.getStartEndPointWithAngle_Line(maxLengthWallPanelLineId1);
            var listStrtEndPointOfMaxLengthWallPanelLine2 = AEE_Utility.getStartEndPointWithAngle_Line(maxLengthWallPanelLineId2);

            double angleOfMaxLngth1Line = listStrtEndPointOfMaxLengthWallPanelLine1[4];

            var maxLngth1Point = AEE_Utility.get_XY(angleOfMaxLngth1Line, CommonModule.internalCorner);

            Point3d maxLngth1StrtPoint = new Point3d(((listStrtEndPointOfMaxLengthWallPanelLine1[0])), ((listStrtEndPointOfMaxLengthWallPanelLine1[1])), 0);
            Point3d maxLngth1EndPoint = new Point3d(((listStrtEndPointOfMaxLengthWallPanelLine1[2])), ((listStrtEndPointOfMaxLengthWallPanelLine1[3])), 0);

            Point3d midRectMaxLngth1StrtPoint = new Point3d((maxLngth1StrtPoint.X + maxLngth1Point.X), (maxLngth1StrtPoint.Y + maxLngth1Point.Y), 0);
            Point3d midRectMaxLngth1EndPoint = new Point3d((maxLngth1EndPoint.X - maxLngth1Point.X), (maxLngth1EndPoint.Y - maxLngth1Point.Y), 0);

            var lineId = AEE_Utility.getLineId(midRectMaxLngth1StrtPoint, midRectMaxLngth1EndPoint, false);

            var lineId_ForCheck = AEE_Utility.OffsetLine(lineId, (minLengthWallPanelLineLngth / 2), false);
            Point3d midPoint = AEE_Utility.GetMidPoint(lineId_ForCheck);
            var flagOfLineIsInside = AEE_Utility.GetPointIsInsideThePolyline(wallObjId, midPoint);

            ObjectId offsetLineId = new ObjectId();

            if (flagOfLineIsInside == true)
            {
                offsetLineId = AEE_Utility.OffsetLine(lineId, minLengthWallPanelLineLngth, false);
            }
            else
            {
                offsetLineId = AEE_Utility.OffsetLine(lineId, -minLengthWallPanelLineLngth, false);
            }

            var listOfMidRectStrtEndPoint = AEE_Utility.GetStartEndPointOfLine(offsetLineId);
            Point3d midRectMaxLngth2StrtPoint = listOfMidRectStrtEndPoint[0];
            Point3d midRectMaxLngth2EndPoint = listOfMidRectStrtEndPoint[1];

            AEE_Utility.deleteEntity(lineId_ForCheck);
            AEE_Utility.deleteEntity(offsetLineId);
            AEE_Utility.deleteEntity(lineId);

            Point3d maxLngth2StrtPoint = new Point3d(((listStrtEndPointOfMaxLengthWallPanelLine2[2])), ((listStrtEndPointOfMaxLengthWallPanelLine2[3])), 0);
            Point3d maxLngth2EndPoint = new Point3d(((listStrtEndPointOfMaxLengthWallPanelLine2[0])), ((listStrtEndPointOfMaxLengthWallPanelLine2[1])), 0);


            if (Window_UI_Helper.checkWindowLayerIsExist(wallLayerName))
            {
                drawWindowThickPanel(wallObjId, xDataRegAppName, wallLayerName, listOfMinLengthExplodeLine, minLengthWallPanelLineLngth, sunkanSlabId);
            }

            #region "Fixing the side Text L or R" 
            //Fixing the side L or R -----------------------------------------------------------------------------------------------------

            CommonModule commonMod = new CommonModule();
            double angleOfLine = AEE_Utility.GetAngleOfLine(maxLngth1StrtPoint, maxLngth1EndPoint);
            bool flag_X_Axis = false;
            bool flag_Y_Axis = false;

            commonMod.checkAngleOfLine_Axis(angleOfLine, out flag_X_Axis, out flag_Y_Axis);
            windowPanelSide startSide = windowPanelSide.TopAndLeft;
            windowPanelSide endSide = windowPanelSide.BottomAndRight;
            if (flag_X_Axis)
            {
                if (maxLngth1StrtPoint.X > maxLngth1EndPoint.X)
                {
                    startSide = windowPanelSide.BottomAndRight;
                    endSide = windowPanelSide.TopAndLeft;
                }
                else
                {
                    startSide = windowPanelSide.TopAndLeft;
                    endSide = windowPanelSide.BottomAndRight;
                }
            }
            else if (flag_Y_Axis)
            {
                if (maxLngth1StrtPoint.Y > maxLngth1EndPoint.Y)
                {
                    startSide = windowPanelSide.TopAndLeft;
                    endSide = windowPanelSide.BottomAndRight;
                }
                else
                {
                    startSide = windowPanelSide.BottomAndRight;
                    endSide = windowPanelSide.TopAndLeft;
                }
            }

            //----------------------------------------------------------------------------------------------------------

            #endregion

            // --------------------------------------------------            

            bool isBeam = Beam_UI_Helper.checkBeamLayerIsExist(wallEnt.Layer);//Changes made on 13/04/2023 by SDM
            var lstCorners1 = new HashSet<Point3d>(new PointComparer());
            var lstCorners2 = new HashSet<Point3d>(new PointComparer());

            CornerHelper crnerHlpr = new CornerHelper();
            string doorWindowThickTopCrnrText = isBeam ? CommonModule.beamThickTopCrnrText : CommonModule.doorWindowThickTopCrnrText;


            double internalMid_Thick = minLengthWallPanelLineLngth;
            double internalMid_Lngth = maxLengthWallPanelLineLngth - (2 * CommonModule.wallPanelThickness);

            double ele = 0.0;
            CornerElevation oelev;

            ObjectId wallID;
            getWallIdFromName(xDataRegAppName, out wallID);
            Line wallLn = AEE_Utility.GetLine(wallID);
            double dist1 = 0;
            double dist2 = 0;

            if (wallPanelType == "Window Wall Panel")
                ele = Window_UI_Helper.getWindowLintelLevel(wallEnt.Layer);
            if (isBeam)//Added on 19/03/2023 by SDM for "Beam" implementation
                ele = Beam_UI_Helper.GetBeamLintelLevelWithoutSunkan(wallEnt.Layer, wallLn.Layer);
            else
                ele = Door_UI_Helper.getDoorLintelLevel(wallEnt.Layer);
            //Fix elevation when there is a sunkan by SDM 2022-07-04
            var ids = SunkanSlabHelper.listOfLinesBtwTwoCrners_SunkanSlabWallLineId;
            var flags = SunkanSlabHelper.listOfLinesBtwTwoCrners_SunkanSlabWallLineflag;
            if (SunkanSlabHelper.listOfWallName_SunkanSlabId.ContainsKey(xDataRegAppName) && !Beam_UI_Helper.checkBeamLayerIsExist(wallEnt.Layer))
                ele -= PanelLayout_UI.RC;
            int pointExistIndx = -1;

            Point2d minLngth1Point = new Point2d();
            Point2d minLngth2Point = new Point2d();

            var offsetCount1 = 0;
            Vector3d offsetVecSide1_1 = new Vector3d();
            Vector3d offsetVecSide1_2 = new Vector3d();

            var offsetCount2 = 0;
            Vector3d offsetVecSide2_1 = new Vector3d();
            Vector3d offsetVecSide2_2 = new Vector3d();

            bool negated = false;
            double checkLngth = 0;

            LineSegment3d lseg1;
            LineSegment3d lseg2;

            ObjectId modifiedWinPanelObj = new ObjectId();

            #region "Draw Bottom Panel"

            double bottomPanelCorner1 = CommonModule.bottomPanelCorner;
            double bottomPanelCorner2 = CommonModule.bottomPanelCorner;
            double bottomPanelCorner3 = CommonModule.bottomPanelCorner;
            double bottomPanelCorner4 = CommonModule.bottomPanelCorner;

            double ICA_ThickRect_Height1 = 0;
            double ICA_ThickRect_Height2 = 0;

            List<ObjectId> listOfMoveIds = new List<ObjectId>();

            //Added --------- RTJ 14-06-2021
            //Point3d ICA_MidRectMinLngth1EndPointBottomPanel = midRectMaxLngth1EndPoint - offsetVecSide2_1;
            //Point3d ICA_MidRectMinLngth2EndPointBottomPanel = midRectMaxLngth2EndPoint + offsetVecSide2_2;

            //Point3d ICA_MidRectMinLngth1StrtPointBottomPanel = midRectMaxLngth1StrtPoint - offsetVecSide1_1;
            //Point3d ICA_MidRectMinLngth2StrtPointBottomPanel = midRectMaxLngth2StrtPoint + offsetVecSide1_2;

            //Point3d ICA_MinLngth1EndPointBottomPanel = maxLngth1EndPoint - offsetVecSide2_1;
            //Point3d ICA_MinLngth2EndPointBottomPanel = maxLngth2EndPoint + offsetVecSide2_2;

            //offsetVecSide1_1 = getOffsetVector(maxLngth1StrtPoint, minLngth1Point, overlapCorners, ref offsetCount1, lstCorners1);
            //offsetVecSide1_2 = getOffsetVector(maxLngth2StrtPoint, minLngth2Point, overlapCorners, ref offsetCount1, lstCorners1);

            //Point3d ICA_MinLngth1StrtPointBottomPanel = maxLngth1StrtPoint - offsetVecSide1_1;
            //Point3d ICA_MinLngth2StrtPointBottomPanel = maxLngth2StrtPoint + offsetVecSide1_2;
            //dist1 = wallLn.StartPoint.DistanceTo(wallLn.GetClosestPointTo(ICA_MinLngth1StrtPointBottomPanel, false));
            //dist2 = wallLn.StartPoint.DistanceTo(wallLn.GetClosestPointTo(ICA_MinLngth1EndPointBottomPanel, false));
            //End --------- RTJ 14-06-2021

            Vector3d moveVectorBottom = new Vector3d();
            Vector3d moveVectorTop = new Vector3d();

            double bottomPanelCornerMax = new List<double> { ICA_ThickRect_Height1, ICA_ThickRect_Height2 }.Max();


            if (wallPanelType == "Window Wall Panel")
            {
                moveVectorBottom = getPositionOfWindowInternalPanel(wallObjId, maxLengthWallPanelLineId1, maxLengthWallPanelLineId2, maxLngth1StrtPoint, maxLngth2StrtPoint, angleOfMaxLngth1Line, minLengthWallPanelLineLngth);

                AEE_Utility.MoveEntity(listOfMoveIds, moveVectorBottom);

                moveVectorTop = getPositionOfWindowInternalPanel(wallObjId, maxLengthWallPanelLineId1, maxLengthWallPanelLineId2, maxLngth1StrtPoint, maxLngth2StrtPoint, angleOfMaxLngth1Line, bottomPanelCornerMax + minLengthWallPanelLineLngth / 2);
            }
            else
            {
                moveVectorTop = getPositionOfWindowInternalPanel(wallObjId, maxLengthWallPanelLineId1, maxLengthWallPanelLineId2, maxLngth1StrtPoint, maxLngth2StrtPoint, angleOfMaxLngth1Line, minLengthWallPanelLineLngth);
            }

            if (wallPanelType == "Window Wall Panel")
            {
                ele = Window_UI_Helper.getWindowSillLevel(wallEnt.Layer);//Added by RTJ 15-06-2020
                //Fix elevation when there is a sunkan by SDM 2022-07-04
                if (SunkanSlabHelper.listOfWallName_SunkanSlabId.ContainsKey(xDataRegAppName))
                    ele -= PanelLayout_UI.RC;


                //Draw Mid Rectangle for Bottom
                listOfMoveIds = drawMidRectOfWindowDoorBottomPanel(wallEnt, xDataRegAppName,
                ref midRectMaxLngth1StrtPoint, ref midRectMaxLngth1EndPoint, ref midRectMaxLngth2EndPoint, ref midRectMaxLngth2StrtPoint, ref maxLngth1EndPoint, ref maxLngth2EndPoint, ref maxLngth1StrtPoint, ref maxLngth2StrtPoint, internalMid_Lngth, internalMid_Thick, wallPanelType, out modifiedWinPanelObj);

                Tuple<List<Point3d>, List<double>> ExtendDistForWindowDoorBottomPanel = null;
                GetWindowDoorBottomPanelOffset(wallObjId, listOfMinLengthExplodeLine, out ExtendDistForWindowDoorBottomPanel);

                if (AEE_Utility.IsPoint3dPresentInPoint3dList(ExtendDistForWindowDoorBottomPanel.Item1, maxLngth1EndPoint, out pointExistIndx))
                    bottomPanelCorner1 = ExtendDistForWindowDoorBottomPanel.Item2.ElementAt(pointExistIndx);

                minLngth1Point = AEE_Utility.get_XY(Math.Round(AEE_Utility.GetLineAngle(new Line(maxLngth1EndPoint, maxLngth2EndPoint))), bottomPanelCorner1);

                if (AEE_Utility.IsPoint3dPresentInPoint3dList(ExtendDistForWindowDoorBottomPanel.Item1, maxLngth2EndPoint, out pointExistIndx))
                    bottomPanelCorner2 = ExtendDistForWindowDoorBottomPanel.Item2.ElementAt(pointExistIndx);


                minLngth2Point = AEE_Utility.get_XY(Math.Round(AEE_Utility.GetLineAngle(new Line(maxLngth1EndPoint, maxLngth2EndPoint))), bottomPanelCorner2);

                offsetCount2 = 0;
                offsetVecSide2_1 = getOffsetVector(maxLngth1EndPoint, minLngth1Point, overlapCorners, ref offsetCount2, lstCorners2);
                offsetVecSide2_2 = getOffsetVector(maxLngth2EndPoint, minLngth2Point, overlapCorners, ref offsetCount2, lstCorners2);

                if (AEE_Utility.IsPoint3dPresentInPoint3dList(ExtendDistForWindowDoorBottomPanel.Item1, maxLngth1StrtPoint, out pointExistIndx))
                    bottomPanelCorner3 = ExtendDistForWindowDoorBottomPanel.Item2.ElementAt(pointExistIndx);

                minLngth1Point = AEE_Utility.get_XY(Math.Round(AEE_Utility.GetLineAngle(new Line(maxLngth1StrtPoint, maxLngth2StrtPoint))), bottomPanelCorner3);

                if (AEE_Utility.IsPoint3dPresentInPoint3dList(ExtendDistForWindowDoorBottomPanel.Item1, maxLngth2StrtPoint, out pointExistIndx))
                    bottomPanelCorner4 = ExtendDistForWindowDoorBottomPanel.Item2.ElementAt(pointExistIndx);

                minLngth2Point = AEE_Utility.get_XY(Math.Round(AEE_Utility.GetLineAngle(new Line(maxLngth1StrtPoint, maxLngth2StrtPoint))), bottomPanelCorner4);

                offsetCount1 = 0;
                offsetVecSide1_1 = getOffsetVector(maxLngth1StrtPoint, minLngth1Point, overlapCorners, ref offsetCount1, lstCorners1);
                offsetVecSide1_2 = getOffsetVector(maxLngth2StrtPoint, minLngth2Point, overlapCorners, ref offsetCount1, lstCorners1);

                lseg1 = findCornerPoint(lstCorners1, plPts);
                lseg2 = findCornerPoint(lstCorners2, plPts);

                Point3d ICA_MidRectMinLngth1EndPointBottomPanel = midRectMaxLngth1EndPoint - offsetVecSide2_1;
                Point3d ICA_MidRectMinLngth2EndPointBottomPanel = midRectMaxLngth2EndPoint + offsetVecSide2_2;

                Point3d ICA_MidRectMinLngth1StrtPointBottomPanel = midRectMaxLngth1StrtPoint - offsetVecSide1_1;
                Point3d ICA_MidRectMinLngth2StrtPointBottomPanel = midRectMaxLngth2StrtPoint + offsetVecSide1_2;

                Point3d ICA_MinLngth1EndPointBottomPanel = maxLngth1EndPoint - offsetVecSide2_1;
                Point3d ICA_MinLngth2EndPointBottomPanel = maxLngth2EndPoint + offsetVecSide2_2;

                Point3d ICA_MinLngth1StrtPointBottomPanel = maxLngth1StrtPoint - offsetVecSide1_1;
                Point3d ICA_MinLngth2StrtPointBottomPanel = maxLngth2StrtPoint + offsetVecSide1_2;


                negated = false;
                // check length is less than window thick
                checkLngth = AEE_Utility.GetLengthOfLine(ICA_MidRectMinLngth1EndPointBottomPanel.X, ICA_MidRectMinLngth1EndPointBottomPanel.Y, ICA_MidRectMinLngth2EndPointBottomPanel.X, ICA_MidRectMinLngth2EndPointBottomPanel.Y);
                if (checkLngth < minLengthWallPanelLineLngth)
                {
                    negated = true;
                    ICA_MidRectMinLngth1EndPointBottomPanel = midRectMaxLngth1EndPoint + offsetVecSide2_1;
                    ICA_MidRectMinLngth2EndPointBottomPanel = midRectMaxLngth2EndPoint - offsetVecSide2_2;

                    ICA_MidRectMinLngth1StrtPointBottomPanel = midRectMaxLngth1StrtPoint + offsetVecSide1_1;
                    ICA_MidRectMinLngth2StrtPointBottomPanel = midRectMaxLngth2StrtPoint - offsetVecSide1_2;

                    ICA_MinLngth1EndPointBottomPanel = maxLngth1EndPoint + offsetVecSide2_1;
                    ICA_MinLngth2EndPointBottomPanel = maxLngth2EndPoint - offsetVecSide2_2;

                    ICA_MinLngth1StrtPointBottomPanel = maxLngth1StrtPoint + offsetVecSide1_1;
                    ICA_MinLngth2StrtPointBottomPanel = maxLngth2StrtPoint - offsetVecSide1_2;
                }

                dist1 = wallLn.StartPoint.DistanceTo(wallLn.GetClosestPointTo(ICA_MinLngth1StrtPointBottomPanel, false));
                dist2 = wallLn.StartPoint.DistanceTo(wallLn.GetClosestPointTo(ICA_MinLngth1EndPointBottomPanel, false));

                ICA_ThickRect_Height1 = Math.Round(AEE_Utility.GetLengthOfLine(ICA_MinLngth1StrtPointBottomPanel, ICA_MinLngth2StrtPointBottomPanel));
                ICA_ThickRect_Height2 = Math.Round(AEE_Utility.GetLengthOfLine(ICA_MidRectMinLngth1EndPointBottomPanel, ICA_MidRectMinLngth2EndPointBottomPanel));


                var cwTxt = CommonModule.windowThickBottomCrnrText;
                var cText1 = CornerHelper.getCornerText(cwTxt, CommonModule.intrnlCornr_Flange1, CommonModule.intrnlCornr_Flange2, ICA_ThickRect_Height1) + getTypes(-offsetVecSide1_1, offsetVecSide1_2, negated, lseg1, startSide);
                var cText2 = CornerHelper.getCornerText(cwTxt, CommonModule.intrnlCornr_Flange1, CommonModule.intrnlCornr_Flange2, ICA_ThickRect_Height2) + getTypes(-offsetVecSide2_1, offsetVecSide2_2, negated, lseg2, endSide);


                var ICA_ThickRectId1 = AEE_Utility.createRectangle(ICA_MinLngth1StrtPointBottomPanel, ICA_MidRectMinLngth1StrtPointBottomPanel, ICA_MidRectMinLngth2StrtPointBottomPanel, ICA_MinLngth2StrtPointBottomPanel, CommonModule.internalCornerLyrName, CommonModule.internalCornerLyrColor);


                AEE_Utility.AttachXData(ICA_ThickRectId1, xDataRegAppName, CommonModule.xDataAsciiName);
                listOfMoveIds.Add(ICA_ThickRectId1);
                var ICA_ThickRect_TextId1 = crnerHlpr.writeTextInCorner(ICA_ThickRectId1, cText1, angleOfMinLngth1Line, CommonModule.internalCornerTextLyrName, CommonModule.internalCornerLyrColor);
                listOfMoveIds.Add(ICA_ThickRect_TextId1);
                //RTJ Start
                if (dist1 < dist2)
                {
                    oelev = new CornerElevation(xDataRegAppName, "", dist1, 90, ele);
                }
                else
                {
                    oelev = new CornerElevation(xDataRegAppName, "", dist1, 180, ele);
                }
                //RTJ Start


                CornerHelper.setCornerDataForBOM(xDataRegAppName, ICA_ThickRectId1, ICA_ThickRect_TextId1, cText1, CommonModule.internalCornerDescp, doorWindowThickTopCrnrText, CommonModule.intrnlCornr_Flange1, CommonModule.intrnlCornr_Flange2, ICA_ThickRect_Height1, oelev);

                var ICA_ThickRectId2 = AEE_Utility.createRectangle(ICA_MidRectMinLngth1EndPointBottomPanel, ICA_MinLngth1EndPointBottomPanel, ICA_MinLngth2EndPointBottomPanel, ICA_MidRectMinLngth2EndPointBottomPanel, CommonModule.internalCornerLyrName, CommonModule.internalCornerLyrColor);

                AEE_Utility.AttachXData(ICA_ThickRectId2, xDataRegAppName, CommonModule.xDataAsciiName);
                listOfMoveIds.Add(ICA_ThickRectId2);
                var ICA_ThickRect_TextId2 = crnerHlpr.writeTextInCorner(ICA_ThickRectId2, cText2, angleOfMinLngth1Line, CommonModule.internalCornerTextLyrName, CommonModule.internalCornerLyrColor);
                listOfMoveIds.Add(ICA_ThickRect_TextId2);
                //RTJ Start
                if (dist2 < dist1)
                {
                    oelev = new CornerElevation(xDataRegAppName, "", dist2, 90, ele);
                }
                else
                {
                    oelev = new CornerElevation(xDataRegAppName, "", dist2, 180, ele);
                }
                // oelev = new CornerElevation(xDataRegAppName, "", dist2, 90, ele);
                CornerHelper.setCornerDataForBOM(xDataRegAppName, ICA_ThickRectId2, ICA_ThickRect_TextId2, cText2, CommonModule.internalCornerDescp, doorWindowThickTopCrnrText, CommonModule.intrnlCornr_Flange1, CommonModule.intrnlCornr_Flange2, ICA_ThickRect_Height2, oelev);

                LCX_CCX_corner_CLine cline = new LCX_CCX_corner_CLine();
                cline.startpoint1 = midRectMaxLngth1StrtPoint;
                cline.endpoint1 = midRectMaxLngth1EndPoint;
                cline.startpoint2 = midRectMaxLngth2StrtPoint;
                cline.endpoint2 = midRectMaxLngth2EndPoint;
                cline.angle = angleOfMinLngth1Line;
                if ((cline.angle == 90) || (cline.angle == 270))
                {
                    if (cText1.ToLower().Contains("lcx"))
                    {
                        cline.panellayername = CommonModule.LCXPanelHorWallLayerName;
                    }
                    else
                    {
                        cline.panellayername = CommonModule.CCXPanelHorWallLayerName;
                    }
                }
                else
                {
                    if (cText1.ToLower().Contains("lcx"))
                    {
                        cline.panellayername = CommonModule.LCXPanelVerWallLayerName;
                    }
                    else
                    {
                        cline.panellayername = CommonModule.CCXPanelVerWallLayerName;
                    }
                }
                //cline.LCX_CCX_mid_CLine = LCX_CCX_mid_CLine;
                cline.bottommove = moveVectorBottom;
                cline.topmove = moveVectorTop;
                cline.panelype = wallPanelType;
                CommonModule.LCX_CCX_corner_CLine.Add(cline);

            }

            #endregion

            #region "Draw Top Panel"

            double topPanelCorner1 = CommonModule.bottomPanelCorner;
            double topPanelCorner2 = CommonModule.bottomPanelCorner;
            double topPanelCorner3 = CommonModule.bottomPanelCorner;
            double topPanelCorner4 = CommonModule.bottomPanelCorner;

            double ICA_ThickRect_Height3 = 0;
            double ICA_ThickRect_Height4 = 0;

            List<ObjectId> listOfTopMoveIds = new List<ObjectId>();

            if (wallPanelType == "Window Wall Panel" || wallPanelType == "Door Wall Panel")
            {
                //Draw Mid Rectangle for Top
                listOfTopMoveIds = drawMidRectOfWindowDoorBottomPanel(wallEnt, xDataRegAppName, ref midRectMaxLngth1StrtPoint, ref midRectMaxLngth1EndPoint, ref midRectMaxLngth2EndPoint, ref midRectMaxLngth2StrtPoint, ref maxLngth1EndPoint, ref maxLngth2EndPoint, ref maxLngth1StrtPoint, ref maxLngth2StrtPoint, internalMid_Lngth, internalMid_Thick, wallPanelType, out modifiedWinPanelObj, true);

                List<ObjectId> listOfModifiedObjExplodeId;
                List<ObjectId> listOfMinLengthModifiedObjExplodeLine;
                Tuple<List<Point3d>, List<double>> ExtendDistForWindowDoorTopPanel = null;

                if (modifiedWinPanelObj.IsValid)
                {
                    listOfModifiedObjExplodeId = AEE_Utility.ExplodeEntity(modifiedWinPanelObj);
                    listOfMinLengthModifiedObjExplodeLine = getMinLengthOfWindowLineSegment(listOfModifiedObjExplodeId);

                    GetWindowDoorTopPanelOffset(modifiedWinPanelObj, listOfMinLengthModifiedObjExplodeLine, wallPanelType, out ExtendDistForWindowDoorTopPanel);

                    AEE_Utility.deleteEntity(listOfModifiedObjExplodeId);
                    AEE_Utility.deleteEntity(modifiedWinPanelObj);
                }
                else
                {
                    GetWindowDoorTopPanelOffset(wallObjId, listOfMinLengthExplodeLine, wallPanelType, out ExtendDistForWindowDoorTopPanel);
                }


                if (AEE_Utility.IsPoint3dPresentInPoint3dList(ExtendDistForWindowDoorTopPanel.Item1, maxLngth1EndPoint, out pointExistIndx))
                    topPanelCorner1 = ExtendDistForWindowDoorTopPanel.Item2.ElementAt(pointExistIndx);

                minLngth1Point = AEE_Utility.get_XY(Math.Round(AEE_Utility.GetLineAngle(new Line(maxLngth1EndPoint, maxLngth2EndPoint))), topPanelCorner1);

                if (AEE_Utility.IsPoint3dPresentInPoint3dList(ExtendDistForWindowDoorTopPanel.Item1, maxLngth2EndPoint, out pointExistIndx))
                    topPanelCorner2 = ExtendDistForWindowDoorTopPanel.Item2.ElementAt(pointExistIndx);

                minLngth2Point = AEE_Utility.get_XY(Math.Round(AEE_Utility.GetLineAngle(new Line(maxLngth1EndPoint, maxLngth2EndPoint))), topPanelCorner2);

                offsetCount2 = 0;
                offsetVecSide2_1 = getOffsetVector(maxLngth1EndPoint, minLngth1Point, overlapCorners, ref offsetCount2, lstCorners2);
                offsetVecSide2_2 = getOffsetVector(maxLngth2EndPoint, minLngth2Point, overlapCorners, ref offsetCount2, lstCorners2);

                if (AEE_Utility.IsPoint3dPresentInPoint3dList(ExtendDistForWindowDoorTopPanel.Item1, maxLngth1StrtPoint, out pointExistIndx))
                    topPanelCorner3 = ExtendDistForWindowDoorTopPanel.Item2.ElementAt(pointExistIndx);

                minLngth1Point = AEE_Utility.get_XY(Math.Round(AEE_Utility.GetLineAngle(new Line(maxLngth1StrtPoint, maxLngth2StrtPoint))), topPanelCorner3);

                if (AEE_Utility.IsPoint3dPresentInPoint3dList(ExtendDistForWindowDoorTopPanel.Item1, maxLngth2StrtPoint, out pointExistIndx))
                    topPanelCorner4 = ExtendDistForWindowDoorTopPanel.Item2.ElementAt(pointExistIndx);


                minLngth2Point = AEE_Utility.get_XY(Math.Round(AEE_Utility.GetLineAngle(new Line(maxLngth1StrtPoint, maxLngth2StrtPoint))), topPanelCorner4);

                offsetCount1 = 0;
                offsetVecSide1_1 = getOffsetVector(maxLngth1StrtPoint, minLngth1Point, overlapCorners, ref offsetCount1, lstCorners1);
                offsetVecSide1_2 = getOffsetVector(maxLngth2StrtPoint, minLngth2Point, overlapCorners, ref offsetCount1, lstCorners1);

                lseg1 = findCornerPoint(lstCorners1, plPts);
                lseg2 = findCornerPoint(lstCorners2, plPts);


                Point3d ICA_MidRectMinLngth1EndPointTopPanel = midRectMaxLngth1EndPoint - offsetVecSide2_1;
                Point3d ICA_MidRectMinLngth2EndPointTopPanel = midRectMaxLngth2EndPoint + offsetVecSide2_2;

                Point3d ICA_MidRectMinLngth1StrtPointTopPanel = midRectMaxLngth1StrtPoint - offsetVecSide1_1;
                Point3d ICA_MidRectMinLngth2StrtPointTopPanel = midRectMaxLngth2StrtPoint + offsetVecSide1_2;

                Point3d ICA_MinLngth1EndPointTopPanel = maxLngth1EndPoint - offsetVecSide2_1;
                Point3d ICA_MinLngth2EndPointTopPanel = maxLngth2EndPoint + offsetVecSide2_2;

                Point3d ICA_MinLngth1StrtPointTopPanel = maxLngth1StrtPoint - offsetVecSide1_1;
                Point3d ICA_MinLngth2StrtPointTopPanel = maxLngth2StrtPoint + offsetVecSide1_2;

                negated = false;
                // check length is less than window thick
                checkLngth = AEE_Utility.GetLengthOfLine(ICA_MidRectMinLngth1EndPointTopPanel.X, ICA_MidRectMinLngth1EndPointTopPanel.Y, ICA_MidRectMinLngth2EndPointTopPanel.X, ICA_MidRectMinLngth2EndPointTopPanel.Y);
                if (checkLngth < minLengthWallPanelLineLngth)
                {
                    negated = true;
                    ICA_MidRectMinLngth1EndPointTopPanel = midRectMaxLngth1EndPoint + offsetVecSide2_1;
                    ICA_MidRectMinLngth2EndPointTopPanel = midRectMaxLngth2EndPoint - offsetVecSide2_2;

                    ICA_MidRectMinLngth1StrtPointTopPanel = midRectMaxLngth1StrtPoint + offsetVecSide1_1;
                    ICA_MidRectMinLngth2StrtPointTopPanel = midRectMaxLngth2StrtPoint - offsetVecSide1_2;

                    ICA_MinLngth1EndPointTopPanel = maxLngth1EndPoint + offsetVecSide2_1;
                    ICA_MinLngth2EndPointTopPanel = maxLngth2EndPoint - offsetVecSide2_2;

                    ICA_MinLngth1StrtPointTopPanel = maxLngth1StrtPoint + offsetVecSide1_1;
                    ICA_MinLngth2StrtPointTopPanel = maxLngth2StrtPoint - offsetVecSide1_2;
                }
                dist1 = wallLn.StartPoint.DistanceTo(wallLn.GetClosestPointTo(ICA_MinLngth1StrtPointTopPanel, false));
                dist2 = wallLn.StartPoint.DistanceTo(wallLn.GetClosestPointTo(ICA_MinLngth1EndPointTopPanel, false));

                ICA_ThickRect_Height3 = Math.Round(AEE_Utility.GetLengthOfLine(ICA_MinLngth1StrtPointTopPanel, ICA_MinLngth2StrtPointTopPanel));

                ICA_ThickRect_Height4 = Math.Round(AEE_Utility.GetLengthOfLine(ICA_MidRectMinLngth1EndPointTopPanel, ICA_MidRectMinLngth2EndPointTopPanel));

                double minCornerWidth = minLengthWallPanelLineLngth + 2 * CommonModule.bottomPanelCorner;//added by SDM 2022-08-23


                var cText3 = CornerHelper.getCornerText(doorWindowThickTopCrnrText, CommonModule.doorWindowTopCornerHt, CommonModule.intrnlCornr_Flange1, ICA_ThickRect_Height3) + getTypes(-offsetVecSide1_1, offsetVecSide1_2, negated, lseg1, startSide);
                var cText4 = CornerHelper.getCornerText(doorWindowThickTopCrnrText, CommonModule.intrnlCornr_Flange1, CommonModule.doorWindowTopCornerHt, ICA_ThickRect_Height4) + getTypes(-offsetVecSide2_1, offsetVecSide2_2, negated, lseg2, endSide);

                //------------------------------------------------------------------------------------------
                if(AEE_Utility.CustType==eCustType.WNPanel)
                {
                     cText3 = CornerHelper.getCornerText(doorWindowThickTopCrnrText, CommonModule.intrnlCornr_Flange1, CommonModule.doorWindowTopCornerHt, ICA_ThickRect_Height3) + getTypes(-offsetVecSide1_1, offsetVecSide1_2, negated, lseg1, startSide);
                     cText4 = CornerHelper.getCornerText(doorWindowThickTopCrnrText, CommonModule.doorWindowTopCornerHt, CommonModule.intrnlCornr_Flange1, ICA_ThickRect_Height4) + getTypes(-offsetVecSide2_1, offsetVecSide2_2, negated, lseg2, endSide);
                }
                
                //------------------------------------------------------------------------------------------//Commented on 05/07/2023 by PRT


                // ele = Window_UI_Helper.getWindowSillLevel(wallEnt.Layer);//Commented BY RTJ 14-06-2021

                if (wallPanelType == "Window Wall Panel")
                    ele = Window_UI_Helper.getWindowLintelLevel(wallEnt.Layer);
                else
                    ele = Door_UI_Helper.getDoorLintelLevel(wallEnt.Layer);
                //Fix elevation when there is a sunkan by SDM 2022-07-04 //Updated to fix LF and DU case by SDM 2022-08-25
                if (SunkanSlabHelper.listOfWallName_SunkanSlabId.ContainsKey(xDataRegAppName) || xDataRegAppName.Contains("_LF_") || xDataRegAppName.Contains("_DU_"))
                    ele -= PanelLayout_UI.RC;

                //Added on 19/03/2023 by SDM for "Beam" implementation
                if (wallEnt.Layer.Contains(Beam_UI_Helper.beamStrtText))
                    ele = Beam_UI_Helper.GetBeamLintelLevel(wallEnt.Layer, xDataRegAppName);

                var ICA_ThickRectId1Top = AEE_Utility.createRectangle(ICA_MinLngth1StrtPointTopPanel, ICA_MidRectMinLngth1StrtPointTopPanel, ICA_MidRectMinLngth2StrtPointTopPanel, ICA_MinLngth2StrtPointTopPanel, CommonModule.internalCornerLyrName, CommonModule.internalCornerLyrColor);

                double flange1 = CommonModule.internalCorner;
                double flange2 = CommonModule.internalCorner;

                AEE_Utility.AttachXData(ICA_ThickRectId1Top, xDataRegAppName, CommonModule.xDataAsciiName);
                listOfTopMoveIds.Add(ICA_ThickRectId1Top);
                var ICA_ThickRect_TextId1Top = crnerHlpr.writeTextInCorner(ICA_ThickRectId1Top, cText3, angleOfMinLngth1Line, CommonModule.internalCornerTextLyrName, CommonModule.internalCornerLyrColor);
                listOfTopMoveIds.Add(ICA_ThickRect_TextId1Top);
                //Added checking Condition 23-06-2021--->Start 
                //RTJ Start
                if (wallPanelType == "Window Wall Panel")
                {
                    if (dist1 < dist2)
                    {
                        oelev = new CornerElevation(xDataRegAppName, "", dist1, 0, ele);////Added by RTJ  14-06-21
                        flange1 = CommonModule.doorWindowTopCornerHt;
                        flange2 = CommonModule.internalCorner;
                    }
                    else
                    {
                        oelev = new CornerElevation(xDataRegAppName, "", dist1, 270, ele);////Added by RTJ  14-06-21
                        flange1 = CommonModule.internalCorner;
                        flange2 = CommonModule.doorWindowTopCornerHt;
                    }
                }
                else
                {
                    // For Door End Corner is inline with Wall Panel because of that will add the wall2 param by SDM 05-06-2022
                    //start
                    //Updated by SDM 2022-08-22
                    string wall2 = "";
                    var wall = InternalWallHelper.GetInternalWallLineByxDataRegAppName(xDataRegAppName);
                    if (isBeam || (ICA_ThickRect_Height3 > minCornerWidth && DoorHelper.listOfListCornerId_With_InsctDoorId.SelectMany(X => X).Contains(wallObjId)))
                    {
                        var st = wall.StartPoint.DistanceTo(ICA_MinLngth2StrtPointTopPanel);
                        var ed = wall.EndPoint.DistanceTo(ICA_MinLngth2StrtPointTopPanel);
                        if (Math.Round(ed) <= Math.Round(checkLngth) || Math.Round(st) <= Math.Round(checkLngth))
                        {
                            var nextWall = "";
                            var prevWall = "";
                            InternalWallHelper.GetPreviousNextWallName(xDataRegAppName, out nextWall, out prevWall);
                            wall2 = st < ed ? prevWall : nextWall;
                        }
                    }
                    //ends
                    if (dist1 < dist2)
                    {
                        oelev = new CornerElevation(xDataRegAppName, wall2, dist1, 0, ele);//UnCommented by RTJ  23-06-21
                        flange1 = CommonModule.doorWindowTopCornerHt;
                        flange2 = CommonModule.internalCorner;
                    }
                    else
                    {
                        oelev = new CornerElevation(xDataRegAppName, wall2, dist1, 270, ele);//UnCommented by RTJ  23-06-21
                        flange1 = CommonModule.internalCorner;
                        flange2 = CommonModule.doorWindowTopCornerHt;
                    }
                    //Added by RTJ  23-06-21--End
                }
                //------->End 23-06-2021
                CornerHelper.setCornerDataForBOM(xDataRegAppName, ICA_ThickRectId1Top, ICA_ThickRect_TextId1Top, cText3, wallPanelType,// CommonModule.internalCornerDescp,
                    doorWindowThickTopCrnrText, flange1, flange2, ICA_ThickRect_Height3, oelev);

                var ICA_ThickRectId2Top = AEE_Utility.createRectangle(ICA_MidRectMinLngth1EndPointTopPanel, ICA_MinLngth1EndPointTopPanel, ICA_MinLngth2EndPointTopPanel, ICA_MidRectMinLngth2EndPointTopPanel, CommonModule.internalCornerLyrName, CommonModule.internalCornerLyrColor);

                //Changes made on 13/04/2023 by SDM
                string xDataRegAppName2 = xDataRegAppName;
                if (isBeam)
                {
                    if (maxLengthWallPanelLineLngth > wallLn.Length)
                    {
                        var listOfAllInternalWallLines = InternalWallHelper.listOfListOfInternalWallRoomLineId_With_WallName.SelectMany(X => X).ToList();
                        for (int i = 0; i < listOfAllInternalWallLines.Count; i++)
                        {
                            ObjectId wallObj = listOfAllInternalWallLines[i];
                            var _wallLn = AEE_Utility.GetLine(wallObj); ;
                            var insctPts = AEE_Utility.InterSectionBetweenTwoEntity(wallObj, ICA_ThickRectId2Top);
                            if (insctPts.Count > 0)
                            {
                                double closestPt; double offset = 0;
                                var isparallel = AEE_Utility.IsLinesAreParallel(wallLn, _wallLn, out offset, out closestPt);
                                if (isparallel && Math.Abs(offset) < minLengthWallPanelLineLngth)
                                {
                                    xDataRegAppName2 = AEE_Utility.GetXDataRegisterAppName(wallObj);
                                    dist2 = _wallLn.StartPoint.DistanceTo(_wallLn.GetClosestPointTo(ICA_MinLngth1EndPointTopPanel, false));
                                    break;
                                }
                            }
                        }
                    }
                }//-----------------------------------

                AEE_Utility.AttachXData(ICA_ThickRectId2Top, xDataRegAppName2, CommonModule.xDataAsciiName);
                listOfTopMoveIds.Add(ICA_ThickRectId2Top);
                var ICA_ThickRect_TextId2Top = crnerHlpr.writeTextInCorner(ICA_ThickRectId2Top, cText4, angleOfMinLngth1Line, CommonModule.internalCornerTextLyrName, CommonModule.internalCornerLyrColor);
                listOfTopMoveIds.Add(ICA_ThickRect_TextId2Top);
                //Added checking Condition 23-06-2021--->Start 
                //RTJ Start
                if (wallPanelType == "Window Wall Panel")
                {
                    if (dist2 < dist1)
                    {
                        oelev = new CornerElevation(xDataRegAppName, "", dist2, 0, ele);//Added by RTJ 14-06-21
                        flange1 = CommonModule.doorWindowTopCornerHt;
                        flange2 = CommonModule.internalCorner;
                    }
                    else
                    {
                        oelev = new CornerElevation(xDataRegAppName, "", dist2, 270, ele);//Added by RTJ 14-06-21
                        flange1 = CommonModule.internalCorner;
                        flange2 = CommonModule.doorWindowTopCornerHt;
                    }
                }
                else
                {
                    // For Door End Corner is inline with Wall Panel because of that will add the wall2 param by SDM 27-06-2022
                    //start
                    string wall2 = "";
                    var wall = InternalWallHelper.GetInternalWallLineByxDataRegAppName(xDataRegAppName2);
                    if (isBeam || (ICA_ThickRect_Height4 > minCornerWidth && DoorHelper.listOfListCornerId_With_InsctDoorId.SelectMany(X => X).Contains(wallObjId)))//Changes made on 13/04/2023 by SDM "isBeam"
                    {
                        var st = wall.StartPoint.DistanceTo(ICA_MinLngth1EndPointTopPanel);
                        var ed = wall.EndPoint.DistanceTo(ICA_MinLngth1EndPointTopPanel);
                        if (Math.Round(ed) <= Math.Round(checkLngth) || Math.Round(st) <= Math.Round(checkLngth))
                        {
                            var nextWall = "";
                            var prevWall = "";
                            InternalWallHelper.GetPreviousNextWallName(xDataRegAppName2, out nextWall, out prevWall);
                            wall2 = st < ed ? prevWall : nextWall;
                        }
                    }
                    //end
                    if (dist2 < dist1)
                    {
                        oelev = new CornerElevation(xDataRegAppName2, wall2, dist2, 0, ele);//UnCommented by RTJ  23-06-21
                        flange1 = CommonModule.doorWindowTopCornerHt;
                        flange2 = CommonModule.internalCorner;
                    }
                    else
                    {
                        oelev = new CornerElevation(xDataRegAppName2, wall2, dist2, 270, ele);//UnCommented by RTJ  23-06-21
                       
                        flange1 = CommonModule.internalCorner;
                        flange2 = CommonModule.doorWindowTopCornerHt;
                    }
                }
                //--->End 23-06-2021
                CornerHelper.setCornerDataForBOM(xDataRegAppName, ICA_ThickRectId2Top, ICA_ThickRect_TextId2Top, cText4, wallPanelType,   //CommonModule.internalCornerDescp,
                    doorWindowThickTopCrnrText, flange1,flange2, ICA_ThickRect_Height4, oelev);

                LCX_CCX_corner_CLine cline = new LCX_CCX_corner_CLine();
                cline.startpoint1 = midRectMaxLngth1StrtPoint;
                cline.endpoint1 = midRectMaxLngth1EndPoint;
                cline.startpoint2 = midRectMaxLngth2StrtPoint;
                cline.endpoint2 = midRectMaxLngth2EndPoint;
                cline.angle = angleOfMinLngth1Line;
                if ((cline.angle == 90) || (cline.angle == 270))
                {
                    if (cText3.ToLower().Contains("lcx"))
                    {
                        cline.panellayername = CommonModule.LCXPanelHorWallLayerName;
                    }
                    else
                    {
                        cline.panellayername = CommonModule.CCXPanelHorWallLayerName;
                    }
                }
                else
                {
                    if (cText3.ToLower().Contains("lcx"))
                    {
                        cline.panellayername = CommonModule.LCXPanelVerWallLayerName;
                    }
                    else
                    {
                        cline.panellayername = CommonModule.CCXPanelVerWallLayerName;
                    }
                }
                //cline.LCX_CCX_mid_CLine = LCX_CCX_mid_CLine;
                cline.bottommove = moveVectorBottom;
                cline.topmove = moveVectorTop;
                cline.panelype = wallPanelType;
                CommonModule.LCX_CCX_corner_CLine.Add(cline);

                //Add "BA" panels Chnages made on 11/04/2023 by SDM for "Beam"
                if (Beam_UI_Helper.checkBeamLayerIsExist(wallEnt.Layer))
                {
                    BeamHelper beamHlp = new BeamHelper();
                    WallPanelHelper wallPanelHlpr = new WallPanelHelper();
                    if (BeamHelper.listOfBeamId_AllBASidePanelsPts.ContainsKey(wallEnt.ObjectId))
                        foreach (var str_end_pts in BeamHelper.listOfBeamId_AllBASidePanelsPts[wallEnt.ObjectId])
                        {
                            var startPoint = str_end_pts.Item1;
                            var endPoint = str_end_pts.Item2;
                            var beamSideBAPanelLineId = AEE_Utility.getLineId(startPoint, endPoint, false);



                            double externalCorner = CommonModule.externalCorner;
                            var shift_vect = (flag_X_Axis ? new Vector3d(0, externalCorner, 0) : new Vector3d(externalCorner, 0, 0));
                            var checkPoint = startPoint + shift_vect;
                            if (AEE_Utility.GetPointIsInsideThePolyline(wallEnt.ObjectId, checkPoint))
                                shift_vect *= -1;


                            var listOfStrtEndPoint = commonMod.getStartEndPointOfLine(beamSideBAPanelLineId);
                            var beamLineStrtPoint = listOfStrtEndPoint[0];
                            var beamLineEndPoint = listOfStrtEndPoint[1];
                            List<(Point3d, Point3d)> listofBeamLinesPts = new List<(Point3d, Point3d)> { (beamLineStrtPoint, beamLineEndPoint) };
                            //check if there is T beam intersection
                            if (BeamHelper.listOfBeamId_AllInsctPtsWithAnotherBeam.ContainsKey(wallEnt.ObjectId))
                            {
                                var inscts = BeamHelper.listOfBeamId_AllInsctPtsWithAnotherBeam[wallEnt.ObjectId];
                                foreach (var insct in inscts)
                                {
                                    var pt1 = insct.Item2.Item1;
                                    var pt2 = insct.Item2.Item2;
                                    Vector3d shift = pt2 - pt1;
                                    shift *= CommonModule.internalCorner / shift.Length;
                                    pt1 -= shift;
                                    pt2 += shift;

                                    if (Math.Abs(pt1.X - pt2.X) < 1 && flag_Y_Axis && Math.Abs(pt1.X - beamLineStrtPoint.X) < 1)//vertical
                                    {
                                        var min = (pt1.Y < pt2.Y) ? pt1 : pt2;
                                        var max = !(pt1.Y < pt2.Y) ? pt1 : pt2;
                                        if (beamLineStrtPoint.IsBetween(pt1, pt2, false, true) && !beamLineEndPoint.IsBetween(pt1, pt2, false, true))
                                        {
                                            listofBeamLinesPts.Clear();
                                            listofBeamLinesPts.Add((new Point3d(beamLineStrtPoint.X, max.Y, 0), beamLineEndPoint));
                                        }
                                        else if (beamLineEndPoint.IsBetween(pt1, pt2, false, true) && !beamLineStrtPoint.IsBetween(pt1, pt2, false, true))
                                        {
                                            listofBeamLinesPts.Clear();
                                            listofBeamLinesPts.Add((beamLineStrtPoint, new Point3d(beamLineEndPoint.X, min.Y, 0)));
                                        }
                                        else if (pt1.IsBetween(beamLineStrtPoint, beamLineEndPoint, false, true) && pt1.IsBetween(beamLineStrtPoint, beamLineEndPoint, false, true))
                                        {
                                            listofBeamLinesPts.Clear();
                                            listofBeamLinesPts.Add((beamLineStrtPoint, new Point3d(beamLineStrtPoint.X, min.Y, 0)));
                                            listofBeamLinesPts.Add((new Point3d(beamLineStrtPoint.X, max.Y, 0), beamLineEndPoint));
                                        }

                                    }
                                    else if (Math.Abs(pt1.Y - pt2.Y) < 1 && flag_X_Axis && Math.Abs(pt1.Y - beamLineStrtPoint.Y) < 1)//horizontal
                                    {
                                        var min = (pt1.X < pt2.X) ? pt1 : pt2;
                                        var max = !(pt1.X < pt2.X) ? pt1 : pt2;

                                        if (beamLineStrtPoint.IsBetween(pt1, pt2, true, false) && !beamLineEndPoint.IsBetween(pt1, pt2, true, false))
                                        {
                                            listofBeamLinesPts.Clear();
                                            listofBeamLinesPts.Add((new Point3d(max.X, beamLineStrtPoint.Y, 0), beamLineEndPoint));
                                        }
                                        else if (beamLineEndPoint.IsBetween(pt1, pt2, true, false) && !beamLineStrtPoint.IsBetween(pt1, pt2, true, false))
                                        {
                                            listofBeamLinesPts.Clear();
                                            listofBeamLinesPts.Add((beamLineStrtPoint, new Point3d(min.X, beamLineEndPoint.Y, 0)));
                                        }
                                        else if (pt1.IsBetween(beamLineStrtPoint, beamLineEndPoint, true, false) && pt1.IsBetween(beamLineStrtPoint, beamLineEndPoint, true, false))
                                        {
                                            listofBeamLinesPts.Clear();
                                            listofBeamLinesPts.Add((new Point3d(max.X, beamLineStrtPoint.Y, 0), beamLineEndPoint));
                                            listofBeamLinesPts.Add((beamLineStrtPoint, new Point3d(min.X, beamLineEndPoint.Y, 0)));
                                        }
                                    }

                                }
                            }
                            foreach (var beamStr_End in listofBeamLinesPts)
                            {
                                var startPt = beamStr_End.Item1;
                                var endPt = beamStr_End.Item2;
                                var beamOffstLineStrtPoint = startPt + shift_vect;
                                var beamOffstLineEndPoint = endPt + shift_vect;
                                var length = (endPt - startPt).Length;

                                List<ObjectId> listOfWallPanelLine_ObjId = new List<ObjectId>();
                                var listOfWallPanelRect_ObjId = wallPanelHlpr.drawWallPanels(xDataRegAppName, beamSideBAPanelLineId, startPt, endPt, beamOffstLineStrtPoint, beamOffstLineEndPoint, flag_X_Axis, flag_Y_Axis, new List<double> { length }, out listOfWallPanelLine_ObjId);
                                listOfMoveIds.AddRange(listOfWallPanelRect_ObjId);
                                beamHlp.AddBeamBottomExternalWallPanelObjs(xDataRegAppName, listOfWallPanelLine_ObjId, listOfWallPanelRect_ObjId);

                                string wallPanelText = PanelLayout_UI.beamSidePanelName;
                                listOfMoveIds.AddRange(beamHlp.writeBeamBottomPanel(xDataRegAppName, wallPanelText, listOfWallPanelRect_ObjId, listOfWallPanelLine_ObjId, angleOfLine, CommonModule.beamBottomExternalPanelType, ele));
                            }

                        }
                }
            }
            #endregion

            bottomPanelCornerMax = new List<double> { ICA_ThickRect_Height1, ICA_ThickRect_Height2 }.Max();

            double topPanelCornerMax = new List<double> { ICA_ThickRect_Height3, ICA_ThickRect_Height4 }.Max();

            //var moveVector = getPositionOfWindowInternalPanel(wallObjId, maxLengthWallPanelLineId1, maxLengthWallPanelLineId2, maxLngth1StrtPoint, maxLngth2StrtPoint, angleOfMaxLngth1Line, minLengthWallPanelLineLngth);  

            moveVectorBottom = new Vector3d();
            moveVectorTop = new Vector3d();

            if (wallPanelType == "Window Wall Panel")
            {
                moveVectorBottom = getPositionOfWindowInternalPanel(wallObjId, maxLengthWallPanelLineId1, maxLengthWallPanelLineId2, maxLngth1StrtPoint, maxLngth2StrtPoint, angleOfMaxLngth1Line, minLengthWallPanelLineLngth);

                AEE_Utility.MoveEntity(listOfMoveIds, moveVectorBottom);

                moveVectorTop = getPositionOfWindowInternalPanel(wallObjId, maxLengthWallPanelLineId1, maxLengthWallPanelLineId2, maxLngth1StrtPoint, maxLngth2StrtPoint, angleOfMaxLngth1Line, bottomPanelCornerMax + minLengthWallPanelLineLngth / 2);
            }
            else if (!Beam_UI_Helper.checkBeamLayerIsExist(wallLayerName))//Added on 19/03/2023 by SDM for "Beam" implementation
            {
                moveVectorTop = getPositionOfWindowInternalPanel(wallObjId, maxLengthWallPanelLineId1, maxLengthWallPanelLineId2, maxLngth1StrtPoint, maxLngth2StrtPoint, angleOfMaxLngth1Line, minLengthWallPanelLineLngth);
            }

            AEE_Utility.MoveEntity(listOfTopMoveIds, moveVectorTop);



            if (Beam_UI_Helper.checkOffsetBeamLayerIsExist(wallLayerName))
            {
                AEE_Utility.MoveEntity(listOfMoveIds, CreateShellPlanHelper.moveVector_ForBeamLayout);
                AEE_Utility.MoveEntity(listOfTopMoveIds, CreateShellPlanHelper.moveVector_ForBeamLayout);
            }
            //Added on 19/03/2023 by SDM for "Beam" implementation
            else if (Beam_UI_Helper.checkBeamLayerIsExist(wallLayerName))
            {
                AEE_Utility.MoveEntity(listOfMoveIds, CreateShellPlanHelper.moveVector_ForBeamBottmLayout);
                AEE_Utility.MoveEntity(listOfTopMoveIds, CreateShellPlanHelper.moveVector_ForBeamBottmLayout);
            }
            else
            {
                AEE_Utility.MoveEntity(listOfMoveIds, CreateShellPlanHelper.moveVector_ForWindowDoorLayout);
                AEE_Utility.MoveEntity(listOfTopMoveIds, CreateShellPlanHelper.moveVector_ForWindowDoorLayout);
            }

            AEE_Utility.deleteEntity(listOfExplodeId);
        }

        private LineSegment3d findCornerPoint(HashSet<Point3d> lstCorners1, List<Point3d> overlapCorners)
        {
            var index = -1;
            var t = 1e-6;
            var tol = new Tolerance(t, t);
            if (overlapCorners.Count < 2)
            {
                return null;
            }
            BoundBlock3d blk = new BoundBlock3d();
            blk.Set(overlapCorners[0], overlapCorners[1]);
            for (var n = 2; n < overlapCorners.Count; ++n)
            {
                blk.Extend(overlapCorners[n]);
            }
            var ptMid = (blk.GetMinimumPoint() + blk.GetMaximumPoint().GetAsVector()) / 2;
            foreach (var item in lstCorners1)
            {
                for (var n = 0; n < overlapCorners.Count - 1; ++n)
                {
                    if (!item.IsEqualTo(overlapCorners[n], tol))
                    {
                        continue;
                    }
                    var prevIndex = n - 1;
                    var nextIndex = n + 1;
                    if (n == 0)
                    {
                        prevIndex = overlapCorners.Count - 2;
                    }
                    var line1 = new LineSegment3d(overlapCorners[n], overlapCorners[prevIndex]);
                    var line2 = new LineSegment3d(overlapCorners[nextIndex], overlapCorners[n]);
                    if (line1.Length < line2.Length)
                    {
                        return new LineSegment3d(line1.MidPoint, ptMid);
                    }
                    return new LineSegment3d(line2.MidPoint, ptMid);
                }
            }
            return null;
        }

        private string getTypes(Vector3d vec1, Vector3d vec2, bool negated, LineSegment3d lSeg, windowPanelSide side)
        {
#if WNPANEL
            return "";
#endif

            var dir1 = new Vector3d(vec1.X, vec1.Y, 0);
            var dir2 = new Vector3d(vec2.X, vec2.Y, 0);
            if (negated)
            {
                dir1 = dir1.Negate();
                dir2 = dir2.Negate();
            }
            var curDirs = new Vector3d[] { dir1, dir2 };
            var dirs = new Vector3d[] { -Vector3d.XAxis, Vector3d.XAxis, -Vector3d.YAxis, Vector3d.YAxis };
            var dirStr = new String[] { "L", "R", "L", "R" };
            if (side == windowPanelSide.BottomAndRight)
            {
                dirStr = new String[] { "R", "L", "R", "L" };
            }
            var t = 1e-6;
            var tol = new Tolerance(t, t);
            var resStr = new List<string>();
            Vector3d vec = new Vector3d(0, 0, 0);
            foreach (var cDir in curDirs)
            {
                if (cDir.IsZeroLength(tol))
                {
                    continue;
                }
                for (var n = 0; n < dirs.Length; ++n)
                {
                    if (dirs[n].IsCodirectionalTo(cDir, tol))
                    {
                        //resStr.Add("(" + dirStr[n] + ")");
                        // only Bottom Panel Corner is 25 then add the L or R accordingly
                        if (CommonModule.bottomPanelCorner == cDir.Length)
                        {
                            resStr.Add(dirStr[n]);
                            vec = dirs[n];
                        }
                        break;
                    }
                }
            }
            if (!resStr.Any())
            {
                return "";
            }

            ///Change 'RL' to 'LR' in CCX and LCX by SDM 11_May_2022
            //if (lSeg == null || resStr.Count == 2 || vec.IsZeroLength())
            //{
            //    return " (" + string.Join(" ", resStr) + ")";
            //}

            if (lSeg == null || resStr.Count == 2 || vec.IsZeroLength())
            {
                return " (LR)";
            }

            var ang = lSeg.Direction.GetAngleTo(vec, Vector3d.ZAxis);
            if (ang > Math.PI)
            {
                return " (L)";
            }
            return " (R)";
        }

        private Vector3d getOffsetVector(Point3d point, Point2d ptDir, List<Point3d> overlapCorners, ref int offsetCount,
            HashSet<Point3d> lstInterPts)
        {
            bool isNearer = false;
            foreach (var pt in overlapCorners)
            {
                var dist = pt.DistanceTo(point);
                if (dist <= CommonModule.bottomPanelCorner * 1.2)
                {
                    lstInterPts.Add(pt);
                    isNearer = true;
                    break;
                }
            }
            if (!isNearer)
            {
                return new Vector3d(ptDir.X, ptDir.Y, 0);
            }
            ++offsetCount;
            return new Vector3d(0, 0, 0);
        }

        private void drawWindowThickPanel(ObjectId windowId, string xDataRegAppName, string windowLayer, List<ObjectId> listOfMinLengthExplodeLine, double minLengthWallPanelLineLngth, ObjectId sunkanSlabId)
        {
            double windowLintelLevel = Window_UI_Helper.getWindowLintelLevel(windowLayer);
            double windowSillLevel = Window_UI_Helper.getWindowSillLevel(windowLayer);
            double heightOfEC_AtNotCornerSide = GeometryHelper.getHeightOfWindow_InAtNotCornerSide_EC_In_InternalExternalWall(windowLintelLevel, windowSillLevel);

            List<ObjectId> listOfWndowThickWallPanelId_InsctToCorner = new List<ObjectId>();
            List<ObjectId> listOffsetWndowThickWallPanelId_InsctToCorner = new List<ObjectId>();

            foreach (var minLengthId in listOfMinLengthExplodeLine)
            {
                var colonMinLngthId = AEE_Utility.createColonEntityInSamePoint(minLengthId, false);
                var offsetMinLngthId = AEE_Utility.OffsetEntity(CommonModule.panelDepth, minLengthId, false);
                var midPoint = AEE_Utility.GetMidPoint(offsetMinLngthId);
                bool flagOfInside = AEE_Utility.GetPointIsInsideThePolyline(windowId, midPoint);
                if (flagOfInside == true)
                {
                }
                else
                {
                    AEE_Utility.deleteEntity(offsetMinLngthId);
                    offsetMinLngthId = AEE_Utility.OffsetEntity(-CommonModule.panelDepth, minLengthId, false);
                }

                listOfWndowThickWallPanelId_InsctToCorner.Add(colonMinLngthId);
                AEE_Utility.AttachXData(colonMinLngthId, xDataRegAppName, CommonModule.xDataAsciiName);

                listOffsetWndowThickWallPanelId_InsctToCorner.Add(offsetMinLngthId);
                AEE_Utility.AttachXData(offsetMinLngthId, xDataRegAppName, CommonModule.xDataAsciiName);
            }

            List<Point3d> listOfMinLngth1_StrtEndPoint = AEE_Utility.GetStartEndPointOfLine(listOfWndowThickWallPanelId_InsctToCorner[0]);
            List<Point3d> listOfMinLngth1_OffstStrtEndPoint = AEE_Utility.GetStartEndPointOfLine(listOffsetWndowThickWallPanelId_InsctToCorner[0]);
            List<Point3d> listOfMinLngth2_StrtEndPoint = AEE_Utility.GetStartEndPointOfLine(listOfWndowThickWallPanelId_InsctToCorner[1]);
            List<Point3d> listOfMinLngth2_OffstStrtEndPoint = AEE_Utility.GetStartEndPointOfLine(listOffsetWndowThickWallPanelId_InsctToCorner[1]);

            Point3d minLengt1_StrtPnt = listOfMinLngth1_StrtEndPoint[0];
            Point3d minLengt1_EndPnt = listOfMinLngth1_StrtEndPoint[1];
            Point3d minLengt1_OffstStrtPnt = listOfMinLngth1_OffstStrtEndPoint[0];
            Point3d minLengt1_OffstEndPnt = listOfMinLngth1_OffstStrtEndPoint[1];

            Point3d minLengt2_StrtPnt = listOfMinLngth2_StrtEndPoint[0];
            Point3d minLengt2_EndPnt = listOfMinLngth2_StrtEndPoint[1];
            Point3d minLengt2_OffstStrtPnt = listOfMinLngth2_OffstStrtEndPoint[0];
            Point3d minLengt2_OffstEndPnt = listOfMinLngth2_OffstStrtEndPoint[1];

            DoorHelper doorHlp = new DoorHelper();
            List<List<ObjectId>> listOfListRemainingDoorThickLineId = doorHlp.drawDoorThickPanel_NotIntersectToWallCorner(CommonModule.wndowwallPanelType, windowId, listOfWndowId_NotInsctToCorner, listOfWndowThickWallPanelId_InsctToCorner, listOffsetWndowThickWallPanelId_InsctToCorner, listOfListOfWndowCornerId_NotInsctToWallCorner, listOfListOfWndowCornerTextId_NotInsctToWallCorner, CommonModule.windowThickWallPanelText);

            if (listOfListRemainingDoorThickLineId.Count != 0)
            {
                //By MT on 10/04/2022----------------------------------------
                string windowThickWallPanelName = CommonModule.windowThickWallPanelText.Replace("X","");// "WS";
                //var thickWallPanelRectId1 = AEE_Utility.createRectangle(minLengt1_StrtPnt, minLengt1_EndPnt, minLengt1_OffstEndPnt, minLengt1_OffstStrtPnt, CommonModule.wallPanelLyrName, CommonModule.wallPanelLyrColor);
                //ObjectId thickWallPanelRectTextId1 = writeTextInWindowThickPanel(xDataRegAppName, windowLayer, thickWallPanelRectId1, minLengthWallPanelLineLngth, heightOfEC_AtNotCornerSide, windowThickWallPanelName, CommonModule.wallPanelTextLyrName, CommonModule.wallPanelLyrColor, sunkanSlabId);
                //var thickWallPanelRectId2 = AEE_Utility.createRectangle(minLengt2_StrtPnt, minLengt2_EndPnt, minLengt2_OffstEndPnt, minLengt2_OffstStrtPnt, CommonModule.wallPanelLyrName, CommonModule.wallPanelLyrColor);
                //ObjectId thickWallPanelRectTextId2 = writeTextInWindowThickPanel(xDataRegAppName, windowLayer, thickWallPanelRectId2, minLengthWallPanelLineLngth, heightOfEC_AtNotCornerSide, windowThickWallPanelName, CommonModule.wallPanelTextLyrName, CommonModule.wallPanelLyrColor, sunkanSlabId);          
                bool isCreateMinLen1 = true;
                bool isCreateMinLen2 = true;
                if (!(listOfListRemainingDoorThickLineId[0].Count > 1))
                {
                    var dTLine1 = AEE_Utility.GetLine(listOfListRemainingDoorThickLineId[0][0]);
                    //var dTLine2 = AEE_Utility.GetLine(listOfListRemainingDoorThickLineId[1][0]);
                    isCreateMinLen1 = dTLine1.StartPoint.Equals(minLengt1_StrtPnt) && dTLine1.EndPoint.Equals(minLengt1_EndPnt);
                    isCreateMinLen2 = dTLine1.StartPoint.Equals(minLengt2_StrtPnt) && dTLine1.EndPoint.Equals(minLengt2_EndPnt);
                }

                //Draw WS elevation by SDM 2022-07-20
                //start
                WallPanelHelper WallHlpr = new WallPanelHelper();
                string description = minLengthWallPanelLineLngth + " " + windowThickWallPanelName + " " + heightOfEC_AtNotCornerSide;
                double elev = windowSillLevel + CommonModule.intrnlCornr_Flange1;
                if (SunkanSlabHelper.listOfWallName_SunkanSlabId.ContainsKey(xDataRegAppName))
                    elev -= PanelLayout_UI.RC;
                //end
                if (isCreateMinLen1)
                {
                    //By MT on 24/04/2022-----------------
                    CheckAndUpdateStartEndPointWithWallCorner(ref minLengt1_StrtPnt, ref minLengt1_EndPnt, ref minLengt1_OffstStrtPnt, ref minLengt1_OffstEndPnt, ref minLengthWallPanelLineLngth, windowId);
                    //------------------------------------
                    var thickWallPanelRectId1 = AEE_Utility.createRectangle(minLengt1_StrtPnt, minLengt1_EndPnt, minLengt1_OffstEndPnt, minLengt1_OffstStrtPnt, CommonModule.wallPanelLyrName, CommonModule.wallPanelLyrColor);
                    ObjectId thickWallPanelRectTextId1 = writeTextInWindowThickPanel(xDataRegAppName, windowLayer, thickWallPanelRectId1, minLengthWallPanelLineLngth, heightOfEC_AtNotCornerSide, windowThickWallPanelName, CommonModule.wallPanelTextLyrName, CommonModule.wallPanelLyrColor, sunkanSlabId);

                    //Draw WS elevation by SDM 2022-07-20
                    WallHlpr.setBOMDataOfWallPanel(thickWallPanelRectTextId1, thickWallPanelRectId1, listOfListRemainingDoorThickLineId[0][0], minLengthWallPanelLineLngth,
                        heightOfEC_AtNotCornerSide, minLengthWallPanelLineLngth, windowThickWallPanelName, description, CommonModule.wallPanelType, elev.ToString());
                }

                if (isCreateMinLen2)
                {
                    //By MT on 24/04/2022-----------------
                    CheckAndUpdateStartEndPointWithWallCorner(ref minLengt2_StrtPnt, ref minLengt2_EndPnt, ref minLengt2_OffstStrtPnt, ref minLengt2_OffstEndPnt, ref minLengthWallPanelLineLngth, windowId);
                    //------------------------------------
                    var thickWallPanelRectId2 = AEE_Utility.createRectangle(minLengt2_StrtPnt, minLengt2_EndPnt, minLengt2_OffstEndPnt, minLengt2_OffstStrtPnt, CommonModule.wallPanelLyrName, CommonModule.wallPanelLyrColor);
                    ObjectId thickWallPanelRectTextId2 = writeTextInWindowThickPanel(xDataRegAppName, windowLayer, thickWallPanelRectId2, minLengthWallPanelLineLngth, heightOfEC_AtNotCornerSide, windowThickWallPanelName, CommonModule.wallPanelTextLyrName, CommonModule.wallPanelLyrColor, sunkanSlabId);

                    //Changes made on 31/05/2023 by SDM
                    ObjectId thickLineId =new ObjectId();
                    if (isCreateMinLen1 != isCreateMinLen2)
                        thickLineId = listOfListRemainingDoorThickLineId[0][0];
                    else
                        thickLineId = listOfListRemainingDoorThickLineId[0][1];
                    //Draw WS elevation by SDM 2022-07-20
                    WallHlpr.setBOMDataOfWallPanel(thickWallPanelRectTextId2, thickWallPanelRectId2,thickLineId , minLengthWallPanelLineLngth,
                            heightOfEC_AtNotCornerSide, minLengthWallPanelLineLngth, windowThickWallPanelName, description, CommonModule.wallPanelType, elev.ToString());
                }
                //------------------------------------------------------------

            }
        }

        private void CheckAndUpdateStartEndPointWithWallCorner(ref Point3d minLengt1_StrtPnt, ref Point3d minLengt1_EndPnt, ref Point3d minLengt1_OffstStrtPnt, ref Point3d minLengt1_OffstEndPnt, ref double wallPanelLineLen, ObjectId windowId)
        {
            string windowLayer = AEE_Utility.GetLayerName(windowId);
            //double wallPanelLineLen = 0;
            List<ObjectId> reqWCIA = new List<ObjectId>();
            double[] xArr = new double[] { minLengt1_StrtPnt.X, minLengt1_EndPnt.X, minLengt1_OffstStrtPnt.X, minLengt1_OffstEndPnt.X };
            double[] yArr = new double[] { minLengt1_StrtPnt.Y, minLengt1_EndPnt.Y, minLengt1_OffstStrtPnt.Y, minLengt1_OffstEndPnt.Y };
            double x1 = xArr.Min(), y1 = yArr.Min(), x2 = xArr.Max(), y2 = yArr.Max();
            for (int i = 0; i < InternalWallHelper.listOfAllInternalWallCornerId.Count; i++)
            {
                //}
                //foreach (var item in InternalWallHelper.listOfAllInternalWallCornerId)
                //{
                var item = InternalWallHelper.listOfAllInternalWallCornerId[i];
                bool isReqquiredwc = false;
                var wc = AEE_Utility.GetEntityForWrite(item) as Polyline;
                var wcminPt = wc.GeometricExtents.MinPoint;
                var wcMaxPt = wc.GeometricExtents.MaxPoint;
                double wcx1 = wcminPt.X, wcy1 = wcminPt.Y, wcx2 = wcMaxPt.X, wcy2 = wcMaxPt.Y;
                double xPointOffset = 0, yPointOffset = 0;
                if ((Math.Round(x1, 4) == Math.Round(wcx1, 4) || Math.Round(x2, 4) == Math.Round(wcx2, 4)) && Math.Round(y1, 4) == Math.Round(wcy2, 4))
                {

                    if (minLengt1_StrtPnt.Y < minLengt1_EndPnt.Y)
                    {
                        minLengt1_StrtPnt = new Point3d(minLengt1_StrtPnt.X, wcy1, minLengt1_StrtPnt.Z);
                        minLengt1_OffstStrtPnt = new Point3d(minLengt1_OffstStrtPnt.X, wcy1, minLengt1_OffstStrtPnt.Z);
                        yPointOffset = -1;
                        xPointOffset = -1;
                    }
                    else
                    {
                        minLengt1_EndPnt = new Point3d(minLengt1_EndPnt.X, wcy1, minLengt1_EndPnt.Z);
                        minLengt1_OffstEndPnt = new Point3d(minLengt1_OffstEndPnt.X, wcy1, minLengt1_OffstEndPnt.Z);
                        yPointOffset = 1;
                        xPointOffset = 1;
                    }
                    wallPanelLineLen += Math.Abs(y1 - wcy1);
                    xPointOffset = xPointOffset * Math.Abs(y1 - wcy1) / 2;
                    yPointOffset = yPointOffset * xPointOffset;
                    isReqquiredwc = true;
                }
                else if ((Math.Round(x1, 4) == Math.Round(wcx1, 4) || Math.Round(x2, 4) == Math.Round(wcx2, 4)) && Math.Round(y2, 4) == Math.Round(wcy1, 4))
                {

                    if (minLengt1_StrtPnt.Y < minLengt1_EndPnt.Y)
                    {
                        minLengt1_StrtPnt = new Point3d(minLengt1_StrtPnt.X, wcy2, minLengt1_StrtPnt.Z);
                        minLengt1_OffstStrtPnt = new Point3d(minLengt1_OffstStrtPnt.X, wcy2, minLengt1_OffstStrtPnt.Z);
                        yPointOffset = -1;
                        xPointOffset = 1;
                    }
                    else
                    {
                        minLengt1_EndPnt = new Point3d(minLengt1_EndPnt.X, wcy2, minLengt1_EndPnt.Z);
                        minLengt1_OffstEndPnt = new Point3d(minLengt1_OffstEndPnt.X, wcy2, minLengt1_OffstEndPnt.Z);
                        yPointOffset = 1;
                        xPointOffset = -1;
                    }
                    wallPanelLineLen += Math.Abs(y2 - wcy2);
                    xPointOffset = xPointOffset * Math.Abs(y2 - wcy2) / 2;
                    yPointOffset = yPointOffset * xPointOffset;
                    isReqquiredwc = true;
                }
                else if ((Math.Round(y1, 4) == Math.Round(wcy1, 4) || Math.Round(y2, 4) == Math.Round(wcy2, 4)) && Math.Round(x1, 4) == Math.Round(wcx2, 4))
                {
                    if (minLengt1_StrtPnt.X > minLengt1_EndPnt.X)
                    {
                        minLengt1_StrtPnt = new Point3d(wcx1, minLengt1_StrtPnt.Y, minLengt1_StrtPnt.Z);
                        minLengt1_OffstStrtPnt = new Point3d(wcx1, minLengt1_OffstStrtPnt.Y, minLengt1_OffstStrtPnt.Z);
                        xPointOffset = 1;
                        yPointOffset = 1;
                    }
                    else
                    {
                        minLengt1_EndPnt = new Point3d(wcx1, minLengt1_EndPnt.Y, minLengt1_EndPnt.Z);
                        minLengt1_OffstEndPnt = new Point3d(wcx1, minLengt1_OffstEndPnt.Y, minLengt1_OffstEndPnt.Z);
                        xPointOffset = -1;
                        yPointOffset = -1;
                    }
                    wallPanelLineLen += Math.Abs(x1 - wcx1);
                    yPointOffset = yPointOffset * Math.Abs(x1 - wcx1) / 2;
                    xPointOffset = xPointOffset * yPointOffset;
                    isReqquiredwc = true;
                }
                else if ((Math.Round(y1, 4) == Math.Round(wcy1, 4) || Math.Round(y2, 4) == Math.Round(wcy2, 4)) && Math.Round(x2, 4) == Math.Round(wcx1, 4))
                {
                    if (minLengt1_StrtPnt.X > minLengt1_EndPnt.X)
                    {
                        minLengt1_StrtPnt = new Point3d(wcx2, minLengt1_StrtPnt.Y, minLengt1_StrtPnt.Z);
                        minLengt1_OffstStrtPnt = new Point3d(wcx2, minLengt1_OffstStrtPnt.Y, minLengt1_OffstStrtPnt.Z);
                        xPointOffset = 1;
                        yPointOffset = -1;
                    }
                    else
                    {
                        minLengt1_EndPnt = new Point3d(wcx2, minLengt1_EndPnt.Y, minLengt1_EndPnt.Z);
                        minLengt1_OffstEndPnt = new Point3d(wcx2, minLengt1_OffstEndPnt.Y, minLengt1_OffstEndPnt.Z);
                        xPointOffset = -1;
                        yPointOffset = 1;
                    }
                    wallPanelLineLen += Math.Abs(x2 - wcx2);
                    yPointOffset = yPointOffset * Math.Abs(x2 - wcx2) / 2;
                    xPointOffset = xPointOffset * yPointOffset;
                    isReqquiredwc = true;
                }
                // Update wall corner text & Add offset text panel
                if (isReqquiredwc)
                {
                    var wallCornerTextID = InternalWallHelper.listOfAllInternalWallCornerTextId[i][0];
                    var ent = AEE_Utility.GetEntityForRead(wallCornerTextID) as MText;
                    double sillLevel = Window_UI_Helper.getWindowSillLevel(windowLayer);
                    var splittxt = ent.Text.Split(' ');
                    double topHt = GeometryHelper.getHeightOfWindowWallPanelX_InternalWall(InternalWallSlab_UI_Helper.getSlabBottom(windowLayer), 0, Window_UI_Helper.getWindowLintelLevel(windowLayer), PanelLayout_UI.SL, CommonModule.internalCorner, new ObjectId());//Fix Ht when Floor ht changed by SDM 2022-08-10
                    //PanelLayout_UI.floorHeight - PanelLayout_UI.getSlabThickness(windowLayer) - PanelLayout_UI.SL - Window_UI_Helper.getWindowLintelLevel(windowLayer);
                    // - PanelLayout_UI.RC;//Added RC to fix elevation by SDM 2022-08-03

                    double topPanelElev = Window_UI_Helper.getWindowLintelLevel(windowLayer);//Fixed by SDM 2022-08-10
                    //PanelLayout_UI.floorHeight - PanelLayout_UI.getSlabThickness(windowLayer) - PanelLayout_UI.SL - topHt; //Save info about Splited IAX by SDM 2022-07-19
                    //  - PanelLayout_UI.RC;//Added RC to fix elevation by SDM 2022-08-03

                    string wall1 = AEE_Utility.GetXDataRegisterAppName(windowId);  //Save info about Splited IAX by SDM 2022-07-19
                    string wall2 = AEE_Utility.GetXDataRegisterAppName(windowId, 3);  //Save info about Splited IAX by SDM 2022-07-19

                    string xDataRegName = AEE_Utility.GetXDataRegisterAppName(item);  //Save info about Splited IAX by SDM 2022-07-19 
                    xDataRegName = xDataRegName == wall1 || xDataRegName == wall2 ? xDataRegName : wall1.Split('_')[2] == xDataRegName.Split('_')[2] ? wall1 : wall2;  //Save info about Splited IAX by SDM 2022-07-19
                    if (SunkanSlabHelper.listOfWallName_SunkanSlabId.ContainsKey(xDataRegName))
                    {
                        double levelDiff = SunkanSlabHelper.getSunkanSlabLevelDifference(SunkanSlabHelper.listOfWallName_SunkanSlabId[xDataRegName]);
                        sillLevel -= PanelLayout_UI.RC;
                        topHt -= levelDiff;
                        topPanelElev -= PanelLayout_UI.RC;
                    }
                    AEE_Utility.changeMText(wallCornerTextID, CornerHelper.getCornerText(splittxt[0], double.Parse(splittxt[1]), double.Parse(splittxt[3]), sillLevel));

                    AEE_Utility.CreateMultiLineText(CornerHelper.getCornerText(splittxt[0], double.Parse(splittxt[1]), double.Parse(splittxt[3]), topHt), new Point3d(ent.Location.X + xPointOffset, ent.Location.Y + yPointOffset, ent.Location.Z), ent.TextHeight, ent.Layer, ent.ColorIndex, AEE_Utility.ConvertRadiansToDegrees(ent.Rotation));

                    //Save info about Splited IAX by SDM 2022-07-19
                    CornerHelper.listOfInternalCornerId_InsctToWindow.Add(item, new List<object> { xDataRegName, sillLevel, topHt, topPanelElev });


                }
                //--------------------------------------------------------
            }
            //return wallPanelLineLen;
        }

        private Vector3d getPositionOfWindowInternalPanel(ObjectId windowId, ObjectId maxLengthWallPanelLineId1, ObjectId maxLengthWallPanelLineId2, Point3d maxLngth1StrtPoint, Point3d maxLngth2StrtPoint, double angleOfWallPanel, double minLengthWallPanelLineLngth)
        {
            ObjectId textLineId = new ObjectId();

            CommonModule commonModl = new CommonModule();
            bool flag_X_Axis = false;
            bool flag_Y_Axis = false;
            commonModl.checkAngleOfLine_Axis(angleOfWallPanel, out flag_X_Axis, out flag_Y_Axis);
            if (flag_X_Axis == true)
            {
                if (maxLngth1StrtPoint.Y > maxLngth2StrtPoint.Y)
                {
                    textLineId = maxLengthWallPanelLineId2;
                }
                else
                {
                    textLineId = maxLengthWallPanelLineId1;
                }
            }
            else if (flag_Y_Axis == true)
            {
                if (maxLngth1StrtPoint.X > maxLngth2StrtPoint.X)
                {
                    textLineId = maxLengthWallPanelLineId2;
                }
                else
                {
                    textLineId = maxLengthWallPanelLineId1;
                }
            }

            var offsetId1 = AEE_Utility.OffsetEntity(5, textLineId, false);
            var midPointOfOffst = AEE_Utility.GetMidPoint(offsetId1);

            var flagOfInside = AEE_Utility.GetPointIsInsideThePolyline(windowId, midPointOfOffst);

            ObjectId outSideRectLineId = new ObjectId();
            if (flagOfInside == true)
            {
                outSideRectLineId = AEE_Utility.OffsetEntity(-(3 * minLengthWallPanelLineLngth), textLineId, false);
            }
            else
            {
                outSideRectLineId = AEE_Utility.OffsetEntity((3 * minLengthWallPanelLineLngth), textLineId, false);
            }

            var listOfTextLineStrtEndPoint = AEE_Utility.GetStartEndPointOfLine(textLineId);
            var listOfOutSideStrtEndPoint = AEE_Utility.GetStartEndPointOfLine(outSideRectLineId);

            Point3d inSideStrtPoint = listOfTextLineStrtEndPoint[0];

            Point3d outSideStrtPoint = listOfOutSideStrtEndPoint[0];
            double x = outSideStrtPoint.X - inSideStrtPoint.X;
            double y = outSideStrtPoint.Y - inSideStrtPoint.Y;

            Vector3d moveVector = new Vector3d(x, y, 0);

            //AEE_Utility.deleteEntity(textLineId);
            AEE_Utility.deleteEntity(offsetId1);
            AEE_Utility.deleteEntity(outSideRectLineId);

            return moveVector;
        }


        private ObjectId writeTextInWindowThickPanel(string xDataRegAppName, string wallPanelLayerName, ObjectId wndwWallIntrnPanelRect_Id, double wndwWallIntrnPanelLineLngth, double wallHeight, string wallHeightText, string layerName, int layerColor, ObjectId sunkanSlabId)
        {
            AEE_Utility.AttachXData(wndwWallIntrnPanelRect_Id, xDataRegAppName, CommonModule.xDataAsciiName);

            var listOfWndwWallIntrnPanelExplodeId = AEE_Utility.ExplodeEntity(wndwWallIntrnPanelRect_Id);
            List<ObjectId> listOfMaxLengthWndwWallIntrnPanel = getMaxLengthOfWindowLineSegment(listOfWndwWallIntrnPanelExplodeId);
            ObjectId wndwWallIntrnPanelLine_Id = listOfMaxLengthWndwWallIntrnPanel[0];

            double wndwWallIntrnPanelLineAngle = AEE_Utility.GetAngleOfLine(wndwWallIntrnPanelLine_Id);
            Point3d dimTextPoint = WallPanelHelper.getCenterPointOfPanelRectangle(wndwWallIntrnPanelRect_Id);

            //double levelDifferenceOfSunkanSlb = SunkanSlabHelper.getSunkanSlabLevelDifference_WallPanel(sunkanSlabId);
            //double RC_WithLevelDiff = levelDifferenceOfSunkanSlb + PanelLayout_UI.RC;
            double RC_WithLevelDiff = PanelLayout_UI.RC;

            List<string> listOfWallPanelText_With_RC = new List<string>();
            string wallPanelText = WallPanelHelper.getWallPanelText(wallPanelLayerName, wallHeight, wndwWallIntrnPanelLineLngth, wallHeightText, RC_WithLevelDiff, PanelLayout_UI.flagOfPanelWithRC, sunkanSlabId, false, out listOfWallPanelText_With_RC);

            WallPanelHelper wallPanelHplr = new WallPanelHelper();
            ObjectId textId = wallPanelHplr.writeDimensionTextInWallPanel(wallPanelText, wndwWallIntrnPanelLine_Id, dimTextPoint, wndwWallIntrnPanelLineAngle, layerName, layerColor);

            WallPanelHelper wallPanelHlp = new WallPanelHelper();
            if (textId.IsValid == true)
            {
                string wallDescp = wallPanelText;
                string wallItemCode = wallHeightText;
                wallPanelHlp.setBOMDataOfWallPanel(textId, wndwWallIntrnPanelRect_Id, wndwWallIntrnPanelLine_Id, wndwWallIntrnPanelLineLngth, wallHeight, 0, wallItemCode, wallDescp, CommonModule.wndowwallPanelType);
            }

            if (listOfWallPanelText_With_RC.Count == 2)
            {
                string itemCode_RC = "RC";
                string RC_Descp = listOfWallPanelText_With_RC[1];
                wallPanelHlp.setBOMDataOfWallPanel(textId, wndwWallIntrnPanelRect_Id, wndwWallIntrnPanelLine_Id, wndwWallIntrnPanelLineLngth, PanelLayout_UI.RC, 0, itemCode_RC, RC_Descp, CommonModule.rockerPanelType);
            }

            ////// Circle not create in window thickness panel
            //var circleId = wallPanelHplr.createCircleInNonStandardWallPanel(dimTextPoint.X, dimTextPoint.Y, wndwWallIntrnPanelLineLngth);

            AEE_Utility.deleteEntity(listOfWndwWallIntrnPanelExplodeId);
            return textId;
        }


        private ObjectId writeTextInMidWindowWallInternalPanel(string xDataRegAppName, ObjectId wndwWallIntrnPanelRect_Id, double wndwWallIntrnPanelLineLngth, double wallHeight, string wallHeightText, string layerName, int layerColor, string wallPanelType, out ObjectId circleId)
        {
            circleId = new ObjectId();

            var listOfWndwWallIntrnPanelExplodeId = AEE_Utility.ExplodeEntity(wndwWallIntrnPanelRect_Id);
            List<ObjectId> listOfMaxLengthWndwWallIntrnPanel = getMaxLengthOfWindowLineSegment(listOfWndwWallIntrnPanelExplodeId);
            ObjectId wndwWallIntrnPanelLine_Id = listOfMaxLengthWndwWallIntrnPanel[0];

            double wndwWallIntrnPanelLineAngle = AEE_Utility.GetAngleOfLine(wndwWallIntrnPanelLine_Id);
            double lengthOfWallPanel = AEE_Utility.GetLengthOfLine(wndwWallIntrnPanelLine_Id);

            Point3d dimTextPoint = WallPanelHelper.getCenterPointOfPanelRectangle(wndwWallIntrnPanelRect_Id);
            ObjectId textId = new ObjectId();

            if (wallHeight != 0)
            {
                AEE_Utility.AttachXData(wndwWallIntrnPanelRect_Id, xDataRegAppName, CommonModule.xDataAsciiName);

                wallHeight = Math.Round(wallHeight);
                lengthOfWallPanel = Math.Round(lengthOfWallPanel);

                string text = Convert.ToString(lengthOfWallPanel) + "NRU" + wallHeightText + " " + Convert.ToString(wallHeight);
                textId = AEE_Utility.CreateTextWithAngle(text, dimTextPoint.X, dimTextPoint.Y, 0, CommonModule.wallPanelTextHght, layerName, layerColor, wndwWallIntrnPanelLineAngle);
                string wallItemCode = wallHeightText;
                string wallDescp = text;

                WallPanelHelper wallPanelHlp = new WallPanelHelper();
                wallPanelHlp.setBOMDataOfWallPanel(textId, wndwWallIntrnPanelRect_Id, wndwWallIntrnPanelLine_Id, lengthOfWallPanel, wallHeight, 0, wallItemCode, wallDescp, wallPanelType);

                circleId = wallPanelHlp.createCircleInNonStandardWallPanel(dimTextPoint.X, dimTextPoint.Y, lengthOfWallPanel);

                //string dimText = Convert.ToString(wallHeight) + " " + wallHeightText + " " + "<>" + " ";
                //textId = AEE_Utility.CreateAlignedDimWithTextWithStyle(CommonModule.newDimensionStyleName, dimText, wndwWallIntrnPanelLine_Id, dimTextPoint, CommonModule.wallPanelTextHght, layerName, layerColor);
            }

            AEE_Utility.deleteEntity(listOfWndwWallIntrnPanelExplodeId);
            return textId;
        }

        private void getPanelBOMNameAndPropInterval(Entity windowEnt, string wallPanelType, bool top, out double propInterval, ref bool beam_bottom, out string BOMname, ref bool swapDimension, ref double windwInternalMid_Lngth, out List<double> distFromWindToBeamBoundary, out bool x_Axis, out bool y_Axis)
        {
            List<ObjectId> windowExplodedObjs = AEE_Utility.ExplodeEntity(windowEnt.ObjectId);
            List<double> listOfDist = new List<double>();
            List<ObjectId> listOfMaxLengthWindowExplodeLine = getMaxLengthOfWindowLineSegment(windowExplodedObjs);
            List<ObjectId> listOfMaxLengthBeamExplodeLine = new List<ObjectId>();
            CommonModule commonMod = new CommonModule();
            double windowDirectionAngle = 0;

            windowDirectionAngle = AEE_Utility.GetAngleOfLine(listOfMaxLengthWindowExplodeLine[0]);
            bool flagOfWindow_X_Axis = false;
            bool flagOfWindow_Y_Axis = false;
            commonMod.checkAngleOfLine_Axis(windowDirectionAngle, out flagOfWindow_X_Axis, out
            flagOfWindow_Y_Axis);

            double windowDoorLintelLevel;
            double beamBottomLevel = -1;


            if (wallPanelType == CommonModule.wndowwallPanelType)
                windowDoorLintelLevel = Window_UI_Helper.getWindowLintelLevel(AEE_Utility.GetLayerName(windowEnt.ObjectId));
            else
                windowDoorLintelLevel = Door_UI_Helper.getDoorLintelLevel(AEE_Utility.GetLayerName(windowEnt.ObjectId));


            x_Axis = flagOfWindow_X_Axis;
            y_Axis = flagOfWindow_Y_Axis;

            if (Beam_UI_Helper.checkOffsetBeamLayerIsExist(windowEnt.Layer))
            {
                propInterval = CommonModule.beamPropInterval;
                BOMname = PanelLayout_UI.doorWindowBottomPanelName;
            }
            else
            {
                propInterval = CommonModule.doorWindowPropInterval;
                beam_bottom = true;
                BOMname = PanelLayout_UI.windowThickBottomWallPanelText;
            }
            // Its for only Window Top and Door Top panel
            if (wallPanelType == CommonModule.doorWallPanelType || top)
            {
                if (Beam_UI_Helper.checkBeamLayerIsExist(windowEnt.Layer))//Changes made on 02/04/2023 by SDM
                {
                    BOMname = PanelLayout_UI.doorWindowBottomPanelNameStandard;
                    propInterval = CommonModule.doorPropInterval;
                    swapDimension = true;
                    distFromWindToBeamBoundary = new List<double> { 0, windwInternalMid_Lngth, 0 };
                    return;
                }
                else
                {
                    BOMname = PanelLayout_UI.doorWindowBottomPanelName;
                    propInterval = CommonModule.doorPropInterval;
                    swapDimension = true;
                }

                List<string> listOfBeamLayerName_DoorWallInsct = new List<string>();
                List<ObjectId> listOfDoor_BeamInsctId = new List<ObjectId>();
                List<double> listOfDistBtwDoorToBeam = new List<double>();
                List<ObjectId> listOfNrstBtwDoorToBeamWallLineId = new List<ObjectId>();

                //BeamHelper.listOfInsctDoor_BeamInsctId;

                List<Point3d> listOfPoints3dWindow = AEE_Utility.GetPolyLinePointList(windowEnt.ObjectId);
                double beamWidth = 0;
                double beamLength = 0;
                bool isInsideBeam = false;

                foreach (Point3d winPoint in listOfPoints3dWindow)
                {
                    for (int i = 0; i < CheckShellPlanHelper.listOfSelectedOffsetBeam_ObjId.Count; i++)
                    {
                        ObjectId insctBeamId = CheckShellPlanHelper.listOfSelectedOffsetBeam_ObjId[i];
                        if (insctBeamId.IsValid == true)
                        {
                            Entity insctBeamEnt = AEE_Utility.GetEntityForRead(insctBeamId);

                            if (AEE_Utility.GetPointIsInsideThePolyline(insctBeamId, winPoint))
                            {
                                isInsideBeam = true;
                                List<ObjectId> beamExplodedObjs = AEE_Utility.ExplodeEntity(insctBeamId);
                                beamBottomLevel = Beam_UI_Helper.getOffsetBeamBottom(insctBeamId);

                                listOfMaxLengthBeamExplodeLine = getMaxLengthOfWindowLineSegment(beamExplodedObjs);
                                commonMod.getRectangleWidthLength(windowDirectionAngle, insctBeamId, out beamLength, out beamWidth);

                                List<double> listOfPositions = new List<double>();

                                if (flagOfWindow_X_Axis)
                                {
                                    listOfPositions.Add(AEE_Utility.GetLine(listOfMaxLengthWindowExplodeLine[0]).StartPoint.Y);
                                    listOfPositions.Add(AEE_Utility.GetLine(listOfMaxLengthWindowExplodeLine[1]).StartPoint.Y);

                                    listOfPositions.Add(AEE_Utility.GetLine(listOfMaxLengthBeamExplodeLine[0]).StartPoint.Y);
                                    listOfPositions.Add(AEE_Utility.GetLine(listOfMaxLengthBeamExplodeLine[1]).StartPoint.Y);
                                }

                                else if (flagOfWindow_Y_Axis)
                                {
                                    listOfPositions.Add(AEE_Utility.GetLine(listOfMaxLengthWindowExplodeLine[0]).StartPoint.X);
                                    listOfPositions.Add(AEE_Utility.GetLine(listOfMaxLengthWindowExplodeLine[1]).StartPoint.X);

                                    listOfPositions.Add(AEE_Utility.GetLine(listOfMaxLengthBeamExplodeLine[0]).StartPoint.X);
                                    listOfPositions.Add(AEE_Utility.GetLine(listOfMaxLengthBeamExplodeLine[1]).StartPoint.X);
                                }


                                listOfPositions.Sort();

                                for (int j = 0; j < listOfPositions.Count - 1; j++)
                                {
                                    listOfDist.Add(Math.Round(listOfPositions[j + 1] - listOfPositions[j]));
                                }
                                AEE_Utility.deleteEntity(beamExplodedObjs);
                                break;
                            }
                        }
                    }
                    if (isInsideBeam)
                        break;
                }

                if (isInsideBeam && (windowDoorLintelLevel == beamBottomLevel))
                {
                    if (beamWidth <= 200)
                    {
                        //if (flagOfWindow_X_Axis)
                        //{
                        //    if (listOfDist[0] == 0 && listOfDist[2] == 0)
                        //        BOMname = PanelLayout_UI.doorWindowBottomPanelName;
                        //    else if (listOfDist[0] != 0 && listOfDist[2] == 0)
                        //        BOMname = PanelLayout_UI.doorWindowBottomPanelName + " (L)";
                        //    else if (listOfDist[0] == 0 && listOfDist[2] != 0)
                        //        BOMname = PanelLayout_UI.doorWindowBottomPanelName + " (R)";
                        //    else if (listOfDist[0] != 0 && listOfDist[2] != 0)
                        //        BOMname = PanelLayout_UI.doorWindowBottomPanelName + " (L)(R)";

                        //}
                        //else
                        //{
                        //    if (listOfDist[0] == 0 && listOfDist[2] == 0)
                        //        BOMname = PanelLayout_UI.doorWindowBottomPanelName;
                        //    else if (listOfDist[0] != 0 && listOfDist[2] == 0)
                        //        BOMname = PanelLayout_UI.doorWindowBottomPanelName + " (L)";
                        //    else if (listOfDist[0] == 0 && listOfDist[2] != 0)
                        //        BOMname = PanelLayout_UI.doorWindowBottomPanelName + " (R)";
                        //    else if (listOfDist[0] != 0 && listOfDist[2] != 0)
                        //        BOMname = PanelLayout_UI.doorWindowBottomPanelName + " (L)(R)";
                        //}
                        BOMname = PanelLayout_UI.doorWindowBottomPanelName + " (L R)";
                        windwInternalMid_Lngth = beamWidth;
                    }
                    else if (beamWidth > 200 && beamWidth <= 325)
                    {
                        BOMname = PanelLayout_UI.doorWindowBottomPanelName + " (L)";
                        windwInternalMid_Lngth = beamWidth;
                    }
                    else if (beamWidth > 325 && beamWidth <= 450)
                    {
                        BOMname = PanelLayout_UI.doorWindowBottomPanelNameStandard;
                        windwInternalMid_Lngth = beamWidth;
                    }
                    else if (beamWidth > 450 && beamWidth <= 525)
                    {
                        BOMname = PanelLayout_UI.doorWindowBottomPanelNameStandard;
                        windwInternalMid_Lngth = beamWidth;
                    }
                }
            }

            AEE_Utility.deleteEntity(windowExplodedObjs);

            if (listOfDist.Any())
            {
                distFromWindToBeamBoundary = listOfDist;
            }
            else
            {
                distFromWindToBeamBoundary = new List<double> { 0, windwInternalMid_Lngth, 0 };
            }

        }

        private List<ObjectId> drawMidRectOfWindowDoorBottomPanel(Entity wallEnt, string xDataRegAppName, ref Point3d midRectMaxLngth1StrtPoint,
          ref Point3d midRectMaxLngth1EndPoint, ref Point3d midRectMaxLngth2EndPoint, ref Point3d midRectMaxLngth2StrtPoint, ref Point3d maxLngth1EndPoint, ref Point3d maxLngth2EndPoint, ref Point3d maxLngth1StrtPoint, ref Point3d maxLngth2StrtPoint, double windwInternalMid_Lngth, double windwInternalMid_Thick, string wallPanelType, out ObjectId modifiedWinPanelObj, bool top = false)
        {
            double propInterval = 0;
            bool beam_bottom = false;
            string BOMname = "";
            bool bSwapDimension = false;
            double modifiedWindInternalMid_Thick = windwInternalMid_Thick;
            bool x_Axis = false;
            bool y_Axis = false;
            List<double> distToBeamBoundary = new List<double>();

            getPanelBOMNameAndPropInterval(wallEnt, wallPanelType, top, out propInterval, ref beam_bottom, out BOMname, ref bSwapDimension, ref modifiedWindInternalMid_Thick, out distToBeamBoundary, out x_Axis, out y_Axis);

            bool panelSizeIsModified = !(modifiedWindInternalMid_Thick == windwInternalMid_Thick);

            //if (Beam_UI_Helper.checkBeamLayerIsExist(wallEnt.Layer) || top)
            //{
            //    propInterval = CommonModule.beamPropInterval;
            //    BOMname = PanelLayout_UI.doorWindowBottomPanelName;
            //}
            //else
            //{
            //    propInterval = CommonModule.doorWindowPropInterval;
            //    beam_bottom = true;
            //    BOMname = PanelLayout_UI.windowThickBottomWallPanelText;
            //}
            //if (wallPanelType == CommonModule.doorWallPanelType)
            //{
            //    BOMname = PanelLayout_UI.doorWindowBottomPanelName;
            //    propInterval = CommonModule.doorPropInterval;
            //    bSwapDimension = true;
            //}

            ObjectId circleId = new ObjectId();
            List<ObjectId> listOfMoveIds = new List<ObjectId>();
            DeckPanelHelper deckPanelHlp = new DeckPanelHelper();
            double newLineLngth = AEE_Utility.GetLengthOfLine(midRectMaxLngth1StrtPoint, midRectMaxLngth1EndPoint);
            newLineLngth = Math.Round(newLineLngth);

            List<double> listOfDeckPanelSpanLngth = deckPanelHlp.getListOfDeckPanelSpanLength(newLineLngth, CommonModule.bottomPanel_StndrdLngth, CommonModule.bottomPanel_MaxLngth, CommonModule.bottomPanel_MinLngth, propInterval - (beam_bottom ? 100 : 0));

            if (beam_bottom && listOfDeckPanelSpanLngth.Count > 1)
                for (int k = 0; k < listOfDeckPanelSpanLngth.Count; k++)
                    listOfDeckPanelSpanLngth[k] -= (k == 0 || k == listOfDeckPanelSpanLngth.Count - 1) ? 50 : 100;

            List<Point3d> listOfRectPoint = new List<Point3d>();
            modifiedWinPanelObj = new ObjectId();
            Point3d refMidPoint3d = new Point3d();
            double tempLineLength = 0;

            if (panelSizeIsModified)
            {
                double tempVal1 = 0;
                double tempVal2 = 0;
                ObjectId tempLineId;

                if (x_Axis)
                {

                    if (midRectMaxLngth1StrtPoint.Y < midRectMaxLngth2StrtPoint.Y)
                    {
                        tempVal1 = distToBeamBoundary[0] * -1;
                        tempVal2 = distToBeamBoundary[2];

                        tempLineId = AEE_Utility.getLineId(midRectMaxLngth1StrtPoint, midRectMaxLngth1EndPoint, wallEnt.Layer, 256, false);
                    }
                    else
                    {
                        tempVal1 = distToBeamBoundary[2];
                        tempVal2 = distToBeamBoundary[0] * -1;

                        tempLineId = AEE_Utility.getLineId(midRectMaxLngth2StrtPoint, midRectMaxLngth2EndPoint, wallEnt.Layer, 256, false);
                    }

                    midRectMaxLngth1StrtPoint = new Point3d(midRectMaxLngth1StrtPoint.X, midRectMaxLngth1StrtPoint.Y + tempVal1, 0);
                    midRectMaxLngth1EndPoint = new Point3d(midRectMaxLngth1EndPoint.X, midRectMaxLngth1EndPoint.Y + tempVal1, 0);
                    midRectMaxLngth2StrtPoint = new Point3d(midRectMaxLngth2StrtPoint.X, midRectMaxLngth2StrtPoint.Y + tempVal2, 0);
                    midRectMaxLngth2EndPoint = new Point3d(midRectMaxLngth2EndPoint.X, midRectMaxLngth2EndPoint.Y + tempVal2, 0);

                    maxLngth1StrtPoint = new Point3d(maxLngth1StrtPoint.X, maxLngth1StrtPoint.Y + tempVal1, 0);
                    maxLngth1EndPoint = new Point3d(maxLngth1EndPoint.X, maxLngth1EndPoint.Y + tempVal1, 0);
                    maxLngth2StrtPoint = new Point3d(maxLngth2StrtPoint.X, maxLngth2StrtPoint.Y + tempVal2, 0);
                    maxLngth2EndPoint = new Point3d(maxLngth2EndPoint.X, maxLngth2EndPoint.Y + tempVal2, 0);

                }
                else
                {
                    if (midRectMaxLngth1StrtPoint.X < midRectMaxLngth2StrtPoint.X)
                    {
                        tempVal1 = distToBeamBoundary[0] * -1;
                        tempVal2 = distToBeamBoundary[2];

                        tempLineId = AEE_Utility.getLineId(midRectMaxLngth1StrtPoint, midRectMaxLngth1EndPoint, wallEnt.Layer, 256, false);
                    }
                    else
                    {
                        tempVal1 = distToBeamBoundary[2];
                        tempVal2 = distToBeamBoundary[0] * -1;

                        tempLineId = AEE_Utility.getLineId(midRectMaxLngth2StrtPoint, midRectMaxLngth2EndPoint, wallEnt.Layer, 256, false);
                    }

                    midRectMaxLngth1StrtPoint = new Point3d(midRectMaxLngth1StrtPoint.X + tempVal1, midRectMaxLngth1StrtPoint.Y, 0);
                    midRectMaxLngth1EndPoint = new Point3d(midRectMaxLngth1EndPoint.X + tempVal1, midRectMaxLngth1EndPoint.Y, 0);
                    midRectMaxLngth2StrtPoint = new Point3d(midRectMaxLngth2StrtPoint.X + tempVal2, midRectMaxLngth2StrtPoint.Y, 0);
                    midRectMaxLngth2EndPoint = new Point3d(midRectMaxLngth2EndPoint.X + tempVal2, midRectMaxLngth2EndPoint.Y, 0);


                    maxLngth1StrtPoint = new Point3d(maxLngth1StrtPoint.X + tempVal1, maxLngth1StrtPoint.Y, 0);
                    maxLngth1EndPoint = new Point3d(maxLngth1EndPoint.X + tempVal1, maxLngth1EndPoint.Y, 0);
                    maxLngth2StrtPoint = new Point3d(maxLngth2StrtPoint.X + tempVal2, maxLngth2StrtPoint.Y, 0);
                    maxLngth2EndPoint = new Point3d(maxLngth2EndPoint.X + tempVal2, maxLngth2EndPoint.Y, 0);
                }

                tempLineLength = AEE_Utility.GetLengthOfLine(tempLineId);
                refMidPoint3d = AEE_Utility.GetMidPoint(tempLineId);
                AEE_Utility.deleteEntity(tempLineId);

                modifiedWinPanelObj = AEE_Utility.createRectangle(maxLngth1StrtPoint, maxLngth1EndPoint, maxLngth2EndPoint, maxLngth2StrtPoint, wallEnt.Layer, 256);
            }

            listOfRectPoint.Add(midRectMaxLngth1StrtPoint);
            listOfRectPoint.Add(midRectMaxLngth1EndPoint);
            listOfRectPoint.Add(midRectMaxLngth2StrtPoint);
            listOfRectPoint.Add(midRectMaxLngth2EndPoint);

            WallPanelHelper wallPanelHelp = new WallPanelHelper();

            List<double> listOfWallPanelLength = wallPanelHelp.getListOfWallPanelLength(modifiedWindInternalMid_Thick, PanelLayout_UI.standardPanelWidth, 300);
            //if (modifiedWindInternalMid_Thick>)
            listOfWallPanelLength.Sort();

            List<ObjectId> tempListOfMoveIds = new List<ObjectId>();

            if (listOfWallPanelLength.Count == 2)
            {
                for (int i = 0; i < listOfWallPanelLength.Count; i++)
                {
                    double panelLen = listOfWallPanelLength[i];
                    double x_Val, y_Val;
                    listOfRectPoint.Clear();

                    if (x_Axis)
                    {
                        x_Val = tempLineLength;
                        y_Val = panelLen;

                        listOfRectPoint.Add(new Point3d(refMidPoint3d.X - x_Val / 2, refMidPoint3d.Y, 0));
                        listOfRectPoint.Add(new Point3d(refMidPoint3d.X + x_Val / 2, refMidPoint3d.Y, 0));
                        listOfRectPoint.Add(new Point3d(refMidPoint3d.X - x_Val / 2, refMidPoint3d.Y + y_Val, 0));
                        listOfRectPoint.Add(new Point3d(refMidPoint3d.X + x_Val / 2, refMidPoint3d.Y + y_Val, 0));

                        refMidPoint3d = new Point3d(refMidPoint3d.X, refMidPoint3d.Y + y_Val, 0);
                    }
                    else
                    {
                        x_Val = panelLen;
                        y_Val = tempLineLength;

                        listOfRectPoint.Add(new Point3d(refMidPoint3d.X, refMidPoint3d.Y - y_Val / 2, 0));
                        listOfRectPoint.Add(new Point3d(refMidPoint3d.X, refMidPoint3d.Y + y_Val / 2, 0));
                        listOfRectPoint.Add(new Point3d(refMidPoint3d.X + x_Val, refMidPoint3d.Y - y_Val / 2, 0));
                        listOfRectPoint.Add(new Point3d(refMidPoint3d.X + x_Val, refMidPoint3d.Y + y_Val / 2, 0));

                        refMidPoint3d = new Point3d(refMidPoint3d.X + x_Val, refMidPoint3d.Y, 0);
                    }

                    tempListOfMoveIds = drawMidRectOfWindowDoorBottomPanel(wallEnt, xDataRegAppName, listOfRectPoint, listOfDeckPanelSpanLngth, panelLen, propInterval, wallPanelType, BOMname, bSwapDimension);

                    listOfMoveIds.AddRange(tempListOfMoveIds);

                }
            }
            else
            {
                listOfMoveIds = drawMidRectOfWindowDoorBottomPanel(wallEnt, xDataRegAppName, listOfRectPoint, listOfDeckPanelSpanLngth, modifiedWindInternalMid_Thick, propInterval, wallPanelType, BOMname, bSwapDimension);
            }



            ////var midRectWallPanelId = AEE_Utility.createRectangle(midRectMaxLngth1StrtPoint, midRectMaxLngth1EndPoint, midRectMaxLngth2EndPoint, midRectMaxLngth2StrtPoint, CommonModule.wallPanelLyrName, CommonModule.wallPanelLyrColor);
            ////listOfMoveIds.Add(midRectWallPanelId);

            ////var midRectWallPanel_TextId = writeTextInMidWindowWallInternalPanel(xDataRegAppName, midRectWallPanelId, windwInternalMid_Lngth, windwInternalMid_Thick, PanelLayout_UI.bottomPanelName, CommonModule.wallPanelTextLyrName, CommonModule.wallPanelLyrColor, wallPanelType, out circleId);
            ////listOfMoveIds.Add(midRectWallPanel_TextId);
            ////if (circleId.IsValid == true)
            ////{
            ////    listOfMoveIds.Add(circleId);
            ////}



            return listOfMoveIds;
        }

        private List<ObjectId> drawMidRectOfWindowDoorBottomPanel(Entity wallEnt, string xDataRegAppName, List<Point3d> listOfRectPoint, List<double> listOfDeckPanelSpanLngth, double panelWidth, double propInterval, string wallPanelType, string BOMName, bool bSwap)
        {
            List<ObjectId> listOfBottomPanelsId = new List<ObjectId>();

            Point3d deckPnl_1_StrtPnt = listOfRectPoint[0];
            Point3d deckPnl_1_EndPnt = listOfRectPoint[1];
            Point3d deckPnl_2_StrtPnt = listOfRectPoint[2];
            Point3d deckPnl_2_EndPnt = listOfRectPoint[3];

            var deckPanelEnt = wallEnt;

            Point3d strtPoint1 = deckPnl_1_StrtPnt;
            Point3d strtPoint2 = deckPnl_2_StrtPnt;

            var lngth = AEE_Utility.GetLengthOfLine(deckPnl_1_StrtPnt, deckPnl_2_StrtPnt);
            var lngth1 = AEE_Utility.GetLengthOfLine(deckPnl_1_EndPnt, deckPnl_2_EndPnt);

            WallPanelHelper wallPanelHlp = new WallPanelHelper();
            //LCX_CCX_mid_CLine = new List<ElevationModule.LCX_CCX_mid_CLine>();

            for (int j = 0; j < listOfDeckPanelSpanLngth.Count; j++)
            {
                double deckPanelSpanLength = listOfDeckPanelSpanLngth[j];
                var angleOfLine = AEE_Utility.GetAngleOfLine(deckPnl_1_StrtPnt, deckPnl_1_EndPnt);

                Point2d lengthPoint = AEE_Utility.get_XY(angleOfLine, deckPanelSpanLength);
                Point3d endPoint1 = new Point3d((strtPoint1.X + lengthPoint.X), (strtPoint1.Y + lengthPoint.Y), 0);
                Point3d endPoint2 = new Point3d((strtPoint2.X + lengthPoint.X), (strtPoint2.Y + lengthPoint.Y), 0);

                ObjectId deckWallLineId = AEE_Utility.getLineId(deckPanelEnt, strtPoint1, endPoint1, false);
                double deckWallLineAngle = AEE_Utility.GetAngleOfLine(deckWallLineId);

                var deckPanelRectId = AEE_Utility.createRectangle(strtPoint1, endPoint1, endPoint2, strtPoint2, CommonModule.wallPanelLyrName, CommonModule.wallPanelLyrColor);
                AEE_Utility.AttachXData(deckPanelRectId, xDataRegAppName, CommonModule.xDataAsciiName);
                listOfBottomPanelsId.Add(deckPanelRectId);

                //Changes made on 11/04/2023 by SDM
                if (Beam_UI_Helper.checkBeamLayerIsExist(wallEnt.Layer))
                {
                    if (!BeamHelper.listOfBeamId_AllBASidePanelsPts.ContainsKey(wallEnt.ObjectId))
                        BeamHelper.listOfBeamId_AllBASidePanelsPts.Add(wallEnt.ObjectId, new List<(Point3d, Point3d)>());
                    BeamHelper.listOfBeamId_AllBASidePanelsPts[wallEnt.ObjectId].Add((strtPoint1, endPoint1));
                    BeamHelper.listOfBeamId_AllBASidePanelsPts[wallEnt.ObjectId].Add((strtPoint2, endPoint2));
                }

                //LCX_CCX_mid_CLine cline = new LCX_CCX_mid_CLine();
                //cline.startpoint1 = strtPoint1;
                //cline.endpoint1 = endPoint1;
                //cline.startpoint2 = strtPoint2;
                //cline.endpoint2 = endPoint2;
                //cline.angle = deckWallLineAngle;
                //LCX_CCX_mid_CLine.Add(cline);

                Point3d dimTextPoint = WallPanelHelper.getCenterPointOfPanelRectangle(deckPanelRectId);

                string deckPanelText = DeckPanelHelper.getWallPanelText(panelWidth, BOMName, deckPanelSpanLength);
                string wallDescp = deckPanelText;
                ObjectId dimTextId2 = wallPanelHlp.writeDimensionTextInWallPanel(deckPanelText, deckWallLineId, dimTextPoint, deckWallLineAngle, CommonModule.wallPanelTextLyrName, CommonModule.wallPanelLyrColor);
                listOfBottomPanelsId.Add(dimTextId2);

                double ele = 0.0;
                string PropBOMName = "";
                if (Beam_UI_Helper.checkBeamLayerIsExist(wallEnt.Layer))
                {
                    ele = Beam_UI_Helper.GetBeamLintelLevel(wallEnt.Layer, xDataRegAppName) - CommonModule.externalCorner;
                    PropBOMName = CommonModule.doorWindowThickTopPropText;
                }
                else if (BOMName == PanelLayout_UI.windowThickBottomWallPanelText)
                {
                    if (wallPanelType == "Window Wall Panel")
                    {
                        // ele = Window_UI_Helper.getWindowLintelLevel(wallEnt.Layer);//Commented BY RTJ 15-06-2021
                        ele = Window_UI_Helper.getWindowSillLevel(wallEnt.Layer) + CommonModule.externalCorner; ;//Added BY RTJ 15-06-2021
                    }
                    else
                    {
                        ele = Door_UI_Helper.getDoorLintelLevel(wallEnt.Layer);
                    }
                    PropBOMName = CommonModule.windowThickBottomPropText;
                }
                else if (BOMName == PanelLayout_UI.doorWindowBottomPanelName)
                {
                    //ele = Window_UI_Helper.getWindowSillLevel(wallEnt.Layer);
                    /// RTJ 11-06-2021 Starts
                    if (Door_UI_Helper.checkDoorLayerIsExist(wallEnt.Layer))
                    {
                        ele = Door_UI_Helper.getDoorLintelLevel(wallEnt.Layer) - CommonModule.externalCorner;
                    }
                    else
                    {
                        // ele = Window_UI_Helper.getWindowSillLevel(wallEnt.Layer);//Commented BY RTJ 15-06-2021
                        ele = Window_UI_Helper.getWindowLintelLevel(wallEnt.Layer) - CommonModule.externalCorner; ;//Added By RTJ 15-06-2021
                    }
                    //RTJ 11-06-2021 ends 
                    PropBOMName = CommonModule.doorWindowThickTopPropText;
                }

                //Fix elevation when there is a sunkan by SDM 2022-07-04  //Updated to fix LF and DU case by SDM 2022-08-25
                if (!Beam_UI_Helper.checkBeamLayerIsExist(wallEnt.Layer) && (SunkanSlabHelper.listOfWallName_SunkanSlabId.ContainsKey(xDataRegAppName) || xDataRegAppName.Contains("_LF_") || xDataRegAppName.Contains("_DU_")))
                    ele -= PanelLayout_UI.RC;
                wallPanelHlp.setBOMDataOfWallPanel(dimTextId2, deckPanelRectId, deckWallLineId, panelWidth, 0, deckPanelSpanLength, BOMName, wallDescp, wallPanelType, ele.ToString());

                Vector3d v = (endPoint2 - endPoint1);
                v *= 50.0 / v.Length;
                Point3d gapStrtPoint1 = endPoint1 - v;
                Point3d gapStrtPoint2 = endPoint2 + v;

                Point2d gapPoint = AEE_Utility.get_XY(angleOfLine, propInterval);
                Point3d gapEndPoint1 = new Point3d((gapStrtPoint1.X + gapPoint.X), (gapStrtPoint1.Y + gapPoint.Y), 0);
                Point3d gapEndPoint2 = new Point3d((gapStrtPoint2.X + gapPoint.X), (gapStrtPoint2.Y + gapPoint.Y), 0);


                if (j != (listOfDeckPanelSpanLngth.Count - 1))
                {
                    //Changes made on 09/03/2023 by SDM for T beam connection
                    double _panelwidth = panelWidth;
                    if (BeamHelper.listOfBeamId_AllInsctPtsWithAnotherBeam.ContainsKey(deckPanelEnt.ObjectId))
                    {
                        var inscts = BeamHelper.listOfBeamId_AllInsctPtsWithAnotherBeam[deckPanelEnt.ObjectId];
                        foreach (var insct in inscts)
                        {
                            var pt1 = insct.Item2.Item1;
                            var pt2 = insct.Item2.Item2;
                            Vector3d shift = pt2 - pt1;
                            shift *= CommonModule.internalCorner / shift.Length;
                            pt1 -= shift;
                            pt2 += shift;

                            if (Math.Abs(pt1.X - pt2.X) < 1 && Math.Abs(lengthPoint.X) < 1)//vertical
                            {
                                if (gapStrtPoint1.IsBetween(pt1, pt2, false, true) || gapEndPoint1.IsBetween(pt1, pt2, false, true))
                                {
                                    if (Math.Abs(pt1.X - gapStrtPoint1.X) < Math.Abs(pt1.X - gapStrtPoint2.X))
                                    {
                                        gapStrtPoint1 += v;
                                        gapEndPoint1 += v;
                                    }
                                    else
                                    {
                                        gapStrtPoint2 -= v;
                                        gapEndPoint2 -= v;
                                    }
                                    _panelwidth -= 50;
                                }
                            }
                            else if (Math.Abs(pt1.Y - pt2.Y) < 1 && Math.Abs(lengthPoint.Y) < 1)//horizontal
                            {
                                if (gapStrtPoint1.IsBetween(pt1, pt2, true, false) || gapEndPoint1.IsBetween(pt1, pt2, true, false))
                                {
                                    if (Math.Abs(pt1.Y - gapStrtPoint1.Y) < Math.Abs(pt1.Y - gapStrtPoint2.Y))
                                    {
                                        gapStrtPoint1 += v;
                                        gapEndPoint1 += v;
                                    }
                                    else
                                    {
                                        gapStrtPoint2 -= v;
                                        gapEndPoint2 -= v;
                                    }
                                    _panelwidth -= 50;
                                }
                            }

                        }
                    }//------------------------------------------------                    
                    //Fix overlapping SDM 2022-08-20
                    //start
                    ObjectId winPnlLnId = new ObjectId();
                    var tt = "";
                    try
                    {
                        if (wallPanelType == CommonModule.wndowwallPanelType)
                        {
                            var idx = WindowHelper.listOfWindowObjId_With_WindowLine.IndexOf(wallEnt.ObjectId);
                            tt = idx == -1 ? "" : listOfWindowObjId_With_WindowPanelLine[idx]?.Split('@')[1]?.Split(',')[0]?.Replace('(', ' ')?.Replace(')', ' ')?.Trim();
                        }
                        else
                        {
                            var mm = DoorHelper.listOfDoorObjId_With_DoorPanelLine.FirstOrDefault(X => X.Contains(wallEnt.ObjectId.ToString()));
                            tt = mm?.Split('@')[1]?.Split(',')[0]?.Replace('(', ' ')?.Replace(')', ' ')?.Trim();
                        }
                        if (tt != null && tt != "")//Changes made on 09/04/2023 by SDM
                            winPnlLnId = new ObjectId(new IntPtr(long.Parse(tt)));
                    }
                    catch
                    {
                    }
                    //end
                    List<ObjectId> listOfPropObjId = drawWindowDoor_PropRect(xDataRegAppName, deckPanelEnt, gapStrtPoint1, gapEndPoint1, gapStrtPoint2, gapEndPoint2, deckWallLineAngle, _panelwidth + 100, propInterval, wallPanelType, ele, PropBOMName, bSwap
                        , listOfDeckPanelSpanLngth[0], winPnlLnId);//Added by SDM 2022-08-20
                    foreach (var propId in listOfPropObjId)
                    {
                        listOfBottomPanelsId.Add(propId);
                    }
                }

                strtPoint1 = new Point3d((endPoint1.X + gapPoint.X), (endPoint1.Y + gapPoint.Y), 0);
                strtPoint2 = new Point3d((endPoint2.X + gapPoint.X), (endPoint2.Y + gapPoint.Y), 0);
            }

            return listOfBottomPanelsId;
        }


        private List<ObjectId> drawWindowDoor_PropRect(string xDataRegAppName, Entity deckPanelEnt,
                                                       Point3d gapStrtPoint1, Point3d gapEndPoint1, Point3d gapStrtPoint2, Point3d gapEndPoint2,
                                                       double angleOfPropPanel, double panelWidth, double propInterval,
                                                       string wallPanelType, double ele, string PropBOMName, bool swap
            , double spanLngth, ObjectId windowPanelLineId)//Added by SDM 2022-08-20
        {
            WallPanelHelper wallPanelHlp = new WallPanelHelper();
            List<ObjectId> listOfPropObjId = new List<ObjectId>();
            ObjectId deckWallLineId = AEE_Utility.getLineId(deckPanelEnt, gapStrtPoint1, gapEndPoint1, false);

            ObjectId deckPanelRectId = AEE_Utility.createRectangle(gapStrtPoint2, gapEndPoint2, gapEndPoint1, gapStrtPoint1, CommonModule.wallPanelLyrName, CommonModule.wallPanelLyrColor);
            AEE_Utility.AttachXData(deckPanelRectId, xDataRegAppName, CommonModule.xDataAsciiName);
            listOfPropObjId.Add(deckPanelRectId);

            Point3d dimTextPoint = WallPanelHelper.getCenterPointOfPanelRectangle(deckPanelRectId);

            double propRadius = CommonModule.propRadius;
            var circleId = AEE_Utility.CreateCircle(dimTextPoint.X, dimTextPoint.Y, 0, propRadius, CommonModule.wallPanelLyrName, CommonModule.wallPanelLyrColor);
            listOfPropObjId.Add(circleId);

            string panelName = PropBOMName;
            string deckPanelText = DeckPanelHelper.getWallPanelText(panelWidth, panelName, propInterval);
            if (swap)
            {
                deckPanelText = DeckPanelHelper.getWallPanelText(propInterval, panelName, panelWidth);
            }
            string wallDescp = deckPanelText;
            ObjectId dimTextId2 = wallPanelHlp.writeDimensionTextInWallPanel(deckPanelText, deckWallLineId, dimTextPoint, angleOfPropPanel + 90.0, CommonModule.wallPanelTextLyrName, CommonModule.wallPanelLyrColor);
            listOfPropObjId.Add(dimTextId2);

            //Updated by SDM 2022-08-20
            //wallPanelHlp.setBOMDataOfWallPanel(dimTextId2, deckPanelRectId, deckWallLineId, panelWidth, 0, propInterval, panelName, wallDescp, wallPanelType, ele.ToString());
            wallPanelHlp.setBOMDataOfWallPanel(dimTextId2, deckPanelRectId, windowPanelLineId.IsValid ? windowPanelLineId : deckWallLineId, panelWidth, spanLngth, propInterval, panelName, wallDescp, wallPanelType, ele.ToString());
            return listOfPropObjId;
        }
    }

    public class TupleComparer : IEqualityComparer<Tuple<Point3d, Point3d>>
    {
        public bool Equals(Tuple<Point3d, Point3d> x, Tuple<Point3d, Point3d> y)
        {
            double t = 1e-6;
            Tolerance tol = new Tolerance(t, t);
            return x.Item1.IsEqualTo(y.Item1, tol) && x.Item2.IsEqualTo(y.Item2, tol);
        }

        public int GetHashCode(Tuple<Point3d, Point3d> obj)
        {
            string str = getString(obj);
            return str.GetHashCode();
        }

        private static string getString(Tuple<Point3d, Point3d> obj)
        {
            var dblArray = new List<double>();
            dblArray.AddRange(obj.Item1.ToArray());
            dblArray.AddRange(obj.Item2.ToArray());
            var lst = dblArray.Select(o => o.ToString("0.000000"));
            var str = string.Join(",", lst);
            return str;
        }
    }

    public class PointComparer : IEqualityComparer<Point3d>
    {
        public bool Equals(Point3d x, Point3d y)
        {
            double t = 1e-6;
            Tolerance tol = new Tolerance(t, t);
            return x.IsEqualTo(y, tol);
        }

        public int GetHashCode(Point3d obj)
        {
            string str = getString(obj);
            return str.GetHashCode();
        }

        private static string getString(Point3d obj)
        {
            var dblArray = new List<double>();
            dblArray.AddRange(obj.ToArray());
            var lst = dblArray.Select(o => o.ToString("0.000000"));
            var str = string.Join(",", lst);
            return str;
        }
    }
}
