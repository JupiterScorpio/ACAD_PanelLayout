﻿using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using PanelLayout_App.CivilModel;
using System;
using System.Collections.Generic;
using System.Linq;
using static PanelLayout_App.RotationHelper;

namespace PanelLayout_App.WallModule
{
    public class DoorHelper
    {
        public static string doorThickPanelGroupName = "PanelLayoutDoor";
        public static int doorThickPanelGroupNumber = 1;
        public static int doorThicknessLength = 75;

        public static List<List<ObjectId>> listOfDoorCornerTextId_ForBeamTextChange = new List<List<ObjectId>>();

        public static List<ObjectId> listOfDoorPanelLine_ObjId = new List<ObjectId>();
        public static List<OverlapExternalDoorCorners> listOverlapExternals = new List<OverlapExternalDoorCorners>();
        public static List<string> listOfDoorPanelLine_ObjId_InStr = new List<string>();
        public static Dictionary<ObjectId, List<DoorEACorner>> dicDoorToEACorners = new Dictionary<ObjectId, List<DoorEACorner>>();
        public static Dictionary<ObjectId, List<Tuple<LineSegment3d, LineSegment3d>>> dicDoorToEALines = new Dictionary<ObjectId, List<Tuple<LineSegment3d, LineSegment3d>>>();
        public static List<ObjectId> listOfDoorPanelOffsetLine_ObjId = new List<ObjectId>();

        private static List<ObjectId> listOfDeleteIdOfDoorPanelThick = new List<ObjectId>();

        private static List<ObjectId> listOfIntersectDoorObjId = new List<ObjectId>();
        private static List<List<Point3d>> listOfListIntersectDoor_Points = new List<List<Point3d>>();

        private static List<ObjectId> listOfIntersectDoorId_ForTrimLineId = new List<ObjectId>();
        private static List<List<ObjectId>> listOfListIntersectDoor_LineId = new List<List<ObjectId>>();

        private static List<string> listOfExistDoorPanelLineObj = new List<string>();
        public static List<string> listOfDoorObjId_With_DoorPanelLine = new List<string>();

        public static List<ObjectId> listOfDoorObjId = new List<ObjectId>();
        public static List<string> listOfDoorObjId_Str = new List<string>();

        public static List<ObjectId> listOfMoveCornerIds_In_New_ShellPlan = new List<ObjectId>();

        private static List<List<ObjectId>> listOfListDoorId_With_InsctCornerId = new List<List<ObjectId>>();
        private static List<ObjectId> listOfExistDoorId_InsctToCorner_ForCheck = new List<ObjectId>();

        public static List<List<ObjectId>> listOfListCornerId_With_InsctDoorId = new List<List<ObjectId>>();
        private static List<ObjectId> listOfExistCornerId_ForCheck = new List<ObjectId>();

        private static List<ObjectId> listOfAllCornerId_InsctDoor_ForCheck = new List<ObjectId>();

        private static List<List<ObjectId>> listOfListOfDoorThickWallPanelId_InsctToCorner = new List<List<ObjectId>>();
        private static List<List<ObjectId>> listOfListOffsetDoorThickWallPanelId_InsctToCorner = new List<List<ObjectId>>();
        public static List<ObjectId> listOfDoorIdThickWallPanel_InsctToCorner = new List<ObjectId>();

        private static List<string> listOfDoorIds_CreateCornerBtwTwoDoor = new List<string>();

        public static List<ObjectId> listOfCornerIfDoorXHeightIsLessThanZero = new List<ObjectId>();

        private static List<ObjectId> listOfDoorId_NotInsctToCorner = new List<ObjectId>();
        private static List<List<ObjectId>> listOfListOfDoorCornerId_NotInsctToWallCorner = new List<List<ObjectId>>();
        private static List<List<List<ObjectId>>> listOfListOfDoorCornerTextId_NotInsctToWallCorner = new List<List<List<ObjectId>>>();

        public static List<List<LineSegment3d>> listDoorLines = new List<List<LineSegment3d>>();

        public static int testColorIndex = 2;
        public static void clearListMethod()
        {
            listOfDeleteIdOfDoorPanelThick.Clear();
            listOfIntersectDoorId_ForTrimLineId.Clear();
            listOfExistCornerId_ForCheck.Clear();
            listOfListCornerId_With_InsctDoorId.Clear();
            listOfListOfDoorThickWallPanelId_InsctToCorner.Clear();
            listOfDoorIdThickWallPanel_InsctToCorner.Clear();
            listOfListOffsetDoorThickWallPanelId_InsctToCorner.Clear();
            listOfListDoorId_With_InsctCornerId.Clear();
            listOfExistDoorId_InsctToCorner_ForCheck.Clear();
            listOfDoorObjId_With_DoorPanelLine.Clear();
            listOfExistDoorPanelLineObj.Clear();
            listOfMoveCornerIds_In_New_ShellPlan.Clear();
            listOfAllCornerId_InsctDoor_ForCheck.Clear();
            listOfDoorObjId.Clear();
            listOfDoorObjId_Str.Clear();
            listOfDoorPanelLine_ObjId.Clear();
            listOverlapExternals.Clear();
            dicDoorToEACorners.Clear();
            dicDoorToEALines.Clear();
            listOfDoorPanelOffsetLine_ObjId.Clear();
            listOfDoorPanelLine_ObjId_InStr.Clear();
            listOfDoorIds_CreateCornerBtwTwoDoor.Clear();
            listOfCornerIfDoorXHeightIsLessThanZero.Clear();
            listOfDoorId_NotInsctToCorner.Clear();
            listOfListOfDoorCornerId_NotInsctToWallCorner.Clear();
            listOfListOfDoorCornerTextId_NotInsctToWallCorner.Clear();
            listOfDoorCornerTextId_ForBeamTextChange.Clear();
            listDoorLines.Clear();
        }
        public void createNewDoorAsPerNewShellPlan(ProgressForm progressForm, string progressBarMsg)
        {
            progressForm.ReportProgress(1, progressBarMsg);
            if (CheckShellPlanHelper.listOfSelectedDoor_ObjId.Count != 0)
            {
                listOfIntersectDoorObjId.Clear();
                listOfListIntersectDoor_Points.Clear();
                listOfListIntersectDoor_LineId.Clear();

                for (int index = 0; index < CheckShellPlanHelper.listOfSelectedDoor_ObjId.Count; index++)
                {
                    ObjectId doorId = CheckShellPlanHelper.listOfSelectedDoor_ObjId[index];
                    if ((index % 10) == 0)
                    {
                        progressForm.ReportProgress(1, progressBarMsg);
                    }

                    ObjectId newDoorId = createNewWindow_ForIntersectWall_Check(doorId);
                    getIntersectPoint(doorId, newDoorId, InternalWallHelper.listOfInternalWallObjId);
                    getIntersectPoint(doorId, newDoorId, ExternalWallHelper.listOfExternalWallObjId);
                    getIntersectPoint(doorId, newDoorId, DuctHelper.listOfDuctObjId);
                    getIntersectPoint(doorId, newDoorId, StairCaseHelper.listOfStairCaseObjId);
                    getIntersectPoint(doorId, newDoorId, LiftHelper.listOfLiftObjId);

                    AEE_Utility.deleteEntity(newDoorId);
                }
                drawNewDoorAsPerNewShellPlan();

                AEE_Utility.deleteEntity(listOfIntersectDoorObjId);

                AEE_Utility.deleteEntity(CheckShellPlanHelper.listOfSelectedDoor_ObjId);
            }
        }
        public static void getIntersectPoint(ObjectId orginalDoorId, ObjectId newDoorId, List<ObjectId> listOfWallIds)
        {
            foreach (var wallId in listOfWallIds)
            {
                var listOfWallExplodeIds = AEE_Utility.ExplodeEntity(wallId);
                for (int index = 0; index < listOfWallExplodeIds.Count; index++)
                {
                    ObjectId explodeWallId = listOfWallExplodeIds[index];
                    List<ObjectId> listOfTrimLineId = AEE_Utility.TrimBetweenTwoClosedPolylines(newDoorId, explodeWallId);
                    if (listOfTrimLineId.Count == 0)
                    {
                        Point3d midPointOfWall = AEE_Utility.GetMidPoint(explodeWallId);
                        var flagOfInside = AEE_Utility.GetPointIsInsideThePolyline(newDoorId, midPointOfWall);
                        if (flagOfInside == true)
                        {
                            listOfTrimLineId.Add(explodeWallId);
                        }
                    }

                    if (listOfTrimLineId.Count != 0)
                    {
                        ObjectId trimLineId = listOfTrimLineId[0];

                        var flagOfDoorLineAngle = checkMaxLengthDoorLineSegmentAngle(orginalDoorId, trimLineId);
                        if (flagOfDoorLineAngle == true)
                        {
                            List<double> listOfTrimStrtEndPoint = AEE_Utility.getStartEndPointWithAngle_Line(trimLineId);
                            setIntersectDoorPoints(orginalDoorId, trimLineId);
                        }
                        //////AEE_Utility.deleteEntity(listOfTrimLineId);
                    }
                }
                ////AEE_Utility.deleteEntity(listOfWallExplodeIds);
            }
        }
        public static void setIntersectDoorPoints(ObjectId orginalDoorId, ObjectId trimLineId)
        {
            List<ObjectId> listOfTrimId = new List<ObjectId>();
            listOfTrimId.Add(trimLineId);
            var trimLineLength = AEE_Utility.GetLengthOfLine(trimLineId);
            if (trimLineLength >= 1)
            {
                List<Point3d> listOfTrimLineStrtEndPoint = AEE_Utility.GetStartEndPointOfLine(trimLineId);
                if (listOfIntersectDoorObjId.Contains(orginalDoorId))
                {
                    int indexOfExistId = listOfIntersectDoorObjId.IndexOf(orginalDoorId);
                    List<Point3d> listOfPrvsTrimLinePoint = listOfListIntersectDoor_Points[indexOfExistId];
                    List<Point3d> listOfAllTrimLinePoint = new List<Point3d>();
                    foreach (var point in listOfPrvsTrimLinePoint)
                    {
                        listOfAllTrimLinePoint.Add(point);
                    }
                    foreach (var point in listOfTrimLineStrtEndPoint)
                    {
                        listOfAllTrimLinePoint.Add(point);
                    }
                    listOfListIntersectDoor_Points[listOfListIntersectDoor_Points.FindIndex(ind => ind.Equals(listOfPrvsTrimLinePoint))] = listOfAllTrimLinePoint;
                }
                else
                {
                    listOfIntersectDoorObjId.Add(orginalDoorId);
                    listOfListIntersectDoor_Points.Add(listOfTrimLineStrtEndPoint);
                }
            }
            setIntersectDoorTrimLine(orginalDoorId, trimLineId);
        }
        public static void setIntersectDoorTrimLine(ObjectId orginalDoorId, ObjectId trimLineId)
        {
            List<ObjectId> listOfTrimId = new List<ObjectId>();
            listOfTrimId.Add(trimLineId);

            if (listOfIntersectDoorId_ForTrimLineId.Contains(orginalDoorId))
            {
                int indexOfExistId = listOfIntersectDoorId_ForTrimLineId.IndexOf(orginalDoorId);

                List<ObjectId> listOfPrvsTrimLineId = listOfListIntersectDoor_LineId[indexOfExistId];
                foreach (var prvsTrimLineId in listOfPrvsTrimLineId)
                {
                    listOfTrimId.Add(prvsTrimLineId);
                }
                listOfListIntersectDoor_LineId[listOfListIntersectDoor_LineId.FindIndex(ind => ind.Equals(listOfPrvsTrimLineId))] = listOfTrimId;
            }
            else
            {
                listOfIntersectDoorId_ForTrimLineId.Add(orginalDoorId);
                listOfListIntersectDoor_LineId.Add(listOfTrimId);
            }
        }

        public static bool checkMaxLengthDoorLineSegmentAngle(ObjectId orginalDoorId, ObjectId trimLineId)
        {
            var listOfExlplodeEnt = AEE_Utility.ExplodeEntity(orginalDoorId);
            WindowHelper windowHlp = new WindowHelper();
            List<ObjectId> listOfMaxLengthExplodeLine = windowHlp.getMaxLengthOfWindowLineSegment(listOfExlplodeEnt);
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
        public static ObjectId createNewWindow_ForIntersectWall_Check(ObjectId doorId)
        {
            WindowHelper windowHlp = new WindowHelper();
            List<ObjectId> listOfExplodeDoorId = AEE_Utility.ExplodeEntity(doorId);
            List<ObjectId> listOfMinLengthExplodeLine = windowHlp.getMinLengthOfWindowLineSegment(listOfExplodeDoorId);
            List<Point3d> listOfNewDoorPoint = new List<Point3d>();

            foreach (var lineId in listOfMinLengthExplodeLine)
            {
                List<double> listOfStrtEndPoint = AEE_Utility.getStartEndPointWithAngle_Line(lineId);
                double startX = listOfStrtEndPoint[0];
                double startY = listOfStrtEndPoint[1];
                double endX = listOfStrtEndPoint[2];
                double endY = listOfStrtEndPoint[3];
                double angleOfLine = listOfStrtEndPoint[4];
                var point = AEE_Utility.get_XY(angleOfLine, WindowHelper.windowOffsetValue);
                listOfNewDoorPoint.Add(new Point3d((startX - point.X), (startY - point.Y), 0));
                listOfNewDoorPoint.Add(new Point3d((endX + point.X), (endY + point.Y), 0));

                //AEE_Utility.getLineId((startX - point.X), (startY - point.Y), 0, (endX + point.X), (endY + point.Y), 0, true);
            }

            AEE_Utility.deleteEntity(listOfExplodeDoorId);

            ObjectId newDoorId = AEE_Utility.GetRectangleId(listOfNewDoorPoint, false);
            return newDoorId;
        }
        private void drawNewDoorAsPerNewShellPlan()
        {
            for (int index = 0; index < listOfListIntersectDoor_Points.Count; index++)
            {
                List<Point3d> listOfNewDoorVertexPoint = listOfListIntersectDoor_Points[index];
                ObjectId doorId = listOfIntersectDoorObjId[index];

                var hasIntersection = false;

                List<ObjectId> listOfInternalWalls = new List<ObjectId>();
                listOfInternalWalls.AddRange(InternalWallHelper.listOfInternalWallObjId);
                listOfInternalWalls.AddRange(SunkanSlabHelper.listOfSunkanSlab_ObjId);

                foreach (var idInternal in listOfInternalWalls)
                {
                    var interPts = AEE_Utility.InterSectionBetweenTwoEntity(idInternal, doorId);
                    if (interPts != null && interPts.Any())
                    {
                        hasIntersection = true;
                        break;
                    }
                }
                var doorEnt = AEE_Utility.GetEntityForWrite(doorId);

                if (listOfNewDoorVertexPoint.Count == 4)
                {
                    ObjectId newDoorId = new ObjectId();

                    var newDoorId_ForInsctCheck = AEE_Utility.GetRectangleId(listOfNewDoorVertexPoint, false);
                    var listOfExplodeEntity = AEE_Utility.ExplodeEntity(newDoorId_ForInsctCheck);
                    ObjectId secondObjId = listOfExplodeEntity[1];
                    ObjectId lastObjId = listOfExplodeEntity[(listOfExplodeEntity.Count - 1)];
                    var listOfInsctPoint = AEE_Utility.InterSectionBetweenTwoEntity(secondObjId, lastObjId);
                    if (listOfInsctPoint.Count != 0)
                    {
                        // swap the third and fourth line, because diagonal rectangle is creating
                        Point3d fisrtPoint = listOfNewDoorVertexPoint[0];
                        Point3d secondPoint = listOfNewDoorVertexPoint[1];
                        Point3d thirdPoint = listOfNewDoorVertexPoint[2];
                        Point3d forthPoint = listOfNewDoorVertexPoint[3];

                        listOfNewDoorVertexPoint.Clear();
                        listOfNewDoorVertexPoint.Add(fisrtPoint);
                        listOfNewDoorVertexPoint.Add(secondPoint);
                        listOfNewDoorVertexPoint.Add(forthPoint);
                        listOfNewDoorVertexPoint.Add(thirdPoint);
                        newDoorId = AEE_Utility.createRectangleWithSameProperty(listOfNewDoorVertexPoint, doorEnt);
                    }
                    else
                    {
                        newDoorId = AEE_Utility.createRectangleWithSameProperty(listOfNewDoorVertexPoint, doorEnt);
                    }
                    if (hasIntersection)
                    {
                        AddToListDoorRect(listOfNewDoorVertexPoint);
                    }
                    AEE_Utility.deleteEntity(listOfExplodeEntity);
                    AEE_Utility.deleteEntity(newDoorId_ForInsctCheck);

                    listOfDoorObjId.Add(newDoorId);
                    listOfDoorObjId_Str.Add(Convert.ToString(newDoorId));
                }
                else
                {
                    WindowHelper windowHlp = new WindowHelper();
                    int indexOfDoor = listOfIntersectDoorId_ForTrimLineId.IndexOf(doorId);
                    if (indexOfDoor != -1)
                    {
                        ObjectId newDoorId = windowHlp.drawNewWindowWall(doorId, indexOfDoor, listOfListIntersectDoor_LineId);
                        if (newDoorId.IsValid == true)
                        {
                            listOfDoorObjId.Add(newDoorId);
                            listOfDoorObjId_Str.Add(Convert.ToString(newDoorId));
                        }
                    }
                }
            }
        }

        private void AddToListDoorRect(List<Point3d> listOfNewDoorVertexPoint)
        {
            var lst = new List<LineSegment3d>();
            for (var n = 0; n < listOfNewDoorVertexPoint.Count; ++n)
            {
                var nextIndex = (n + 1) % listOfNewDoorVertexPoint.Count;
                lst.Add(new LineSegment3d(listOfNewDoorVertexPoint[n], listOfNewDoorVertexPoint[nextIndex]));
            }
            listDoorLines.Add(lst);
        }

        public void checkDoorIsIntersectToWallPanelLine(string xDataRegAppName, ObjectId wallPanelLineId, ObjectId cornerId1, ObjectId cornerId2, List<ObjectId> listOfWallPanelLineWithWindowInsct_ObjId, out List<ObjectId> listOfDoorPanelLine_ObjId, out List<ObjectId> listOfWallPanelLine_ObjId, out List<ObjectId> listOfDoorObjId_With_WallInsct)
        {
            listOfDoorPanelLine_ObjId = new List<ObjectId>();
            listOfWallPanelLine_ObjId = new List<ObjectId>();
            listOfDoorObjId_With_WallInsct = new List<ObjectId>();

            Entity wallPanelEnt_ForSamePropty = AEE_Utility.GetEntityForRead(wallPanelLineId);

            bool flag_X_Axis = false;
            bool flag_Y_Axis = false;
            for (int m = 0; m < listOfWallPanelLineWithWindowInsct_ObjId.Count; m++)
            {
                ObjectId wallPanel_With_WindowInsct_ObjId = listOfWallPanelLineWithWindowInsct_ObjId[m];
                var listOfWallPanelLineStrtEndPoint = AEE_Utility.getStartEndPointWithAngle_Line(wallPanel_With_WindowInsct_ObjId);
                Point3d strtPointOfWallPanel = new Point3d(listOfWallPanelLineStrtEndPoint[0], listOfWallPanelLineStrtEndPoint[1], 0);
                Point3d endPointOfWallPanel = new Point3d(listOfWallPanelLineStrtEndPoint[2], listOfWallPanelLineStrtEndPoint[3], 0);
                double wallPanelLineAngle = listOfWallPanelLineStrtEndPoint[4];

                CommonModule commonModule_Obj = new CommonModule();
                commonModule_Obj.checkAngleOfLine_Axis(wallPanelLineAngle, out flag_X_Axis, out flag_Y_Axis);

                List<Point3d> listOfAllInsctPoint = new List<Point3d>();
                List<Point3d> listOfAllDoorPoint = new List<Point3d>();
                List<string> listOfDoorObjId_And_IntersecPoint = new List<string>();
                List<ObjectId> listOfDoorObjId_OfWallInsct = new List<ObjectId>();

                bool flagOfWallLineIsInsideTheDoor = false;
                foreach (var doorId in listOfDoorObjId)
                {
                    flagOfWallLineIsInsideTheDoor = checkWallPanelLine_IsInsideThe_Window_Or_Door(wallPanel_With_WindowInsct_ObjId, doorId);
                    if (flagOfWallLineIsInsideTheDoor == true)
                    {
                        if (listOfAllInsctPoint.Count == 0)
                        {
                            var listOfDoorPanelLineStrtEndPoint = getDoorWallPaneLinelPoint_ForCorner(doorId, wallPanel_With_WindowInsct_ObjId);
                            Point3d strtPointOfDoorLine = listOfDoorPanelLineStrtEndPoint[0];
                            Point3d endPointOfDoorLine = listOfDoorPanelLineStrtEndPoint[1];

                            listOfDoorObjId_And_IntersecPoint.Add(Convert.ToString(doorId) + "@" + Convert.ToString(strtPointOfDoorLine));
                            ObjectId doorPanel_LineId = AEE_Utility.getLineId(wallPanelEnt_ForSamePropty, strtPointOfDoorLine.X, strtPointOfDoorLine.Y, 0, endPointOfDoorLine.X, endPointOfDoorLine.Y, 0, false);
                            listOfDoorPanelLine_ObjId.Add(doorPanel_LineId);

                            setDoorObjId_WithWindowPanelLine(listOfDoorObjId_And_IntersecPoint, strtPointOfDoorLine, endPointOfDoorLine, doorPanel_LineId);
                        }
                        AEE_Utility.AttachXData(doorId, xDataRegAppName, CommonModule.xDataAsciiName);
                        break;
                    }

                    var listOfInsctPoint = AEE_Utility.InterSectionBetweenTwoEntity(doorId, wallPanel_With_WindowInsct_ObjId);
                    if (listOfInsctPoint.Count != 0)
                    {
                        var listOfDoorPanelLineStrtEndPoint = getDoorWallPaneLinelPoint_ForCorner(doorId, wallPanel_With_WindowInsct_ObjId);
                        Point3d strtPointOfDoorLine = listOfDoorPanelLineStrtEndPoint[0];
                        Point3d endPointOfDoorLine = listOfDoorPanelLineStrtEndPoint[1];
                        AEE_Utility.AttachXData(doorId, xDataRegAppName, CommonModule.xDataAsciiName);

                        listOfDoorObjId_With_WallInsct.Add(doorId);


                        if (listOfInsctPoint.Count == 1)
                        {
                            Point3d insctPoint = listOfInsctPoint[0];
                            List<double> listOfLngth = new List<double>();
                            foreach (var point in listOfDoorPanelLineStrtEndPoint)
                            {
                                var lenght = AEE_Utility.GetLengthOfLine(point, insctPoint);
                                listOfLngth.Add(lenght);
                            }
                            double minLngth = listOfLngth.Min();
                            int indexOfMinLngth = listOfLngth.IndexOf(minLngth);
                            var nearestPoint = listOfDoorPanelLineStrtEndPoint[indexOfMinLngth];
                            double maxLngth = listOfLngth.Max();
                            int indexOfMaxLngth = listOfLngth.IndexOf(maxLngth);


                            listOfAllInsctPoint.Add(nearestPoint);
                            listOfAllDoorPoint.Add(nearestPoint);
                            listOfDoorObjId_And_IntersecPoint.Add(Convert.ToString(doorId) + "@" + Convert.ToString(nearestPoint));
                            listOfDoorObjId_OfWallInsct.Add(doorId);

                            Point3d doorStrtOrEndPoint = new Point3d();
                            Point3d wallStrtOrEndPoint = new Point3d();
                            getStartOrEndPointOfDoor(listOfInsctPoint, wallPanel_With_WindowInsct_ObjId, doorId, flag_X_Axis, flag_Y_Axis, out doorStrtOrEndPoint, out wallStrtOrEndPoint);
                            listOfAllInsctPoint.Add(wallStrtOrEndPoint);

                            doorStrtOrEndPoint = listOfDoorPanelLineStrtEndPoint[indexOfMaxLngth];
                            listOfAllDoorPoint.Add(doorStrtOrEndPoint);
                            listOfDoorObjId_And_IntersecPoint.Add(Convert.ToString(doorId) + "@" + Convert.ToString(doorStrtOrEndPoint));
                            listOfDoorObjId_OfWallInsct.Add(doorId);
                        }
                        else
                        {
                            foreach (var doorLinePoint in listOfDoorPanelLineStrtEndPoint)
                            {
                                listOfAllInsctPoint.Add(doorLinePoint);
                                listOfAllDoorPoint.Add(doorLinePoint);
                                listOfDoorObjId_And_IntersecPoint.Add(Convert.ToString(doorId) + "@" + Convert.ToString(doorLinePoint));
                                listOfDoorObjId_OfWallInsct.Add(doorId);
                            }

                            listOfAllInsctPoint.Add(strtPointOfWallPanel);
                            listOfAllInsctPoint.Add(endPointOfWallPanel);
                        }
                    }
                }
                if (listOfAllInsctPoint.Count != 0)
                {
                    List<ObjectId> listOfWallPanelLineId_WithDoor = new List<ObjectId>();
                    drawDoorWallPanelLine(listOfDoorObjId_OfWallInsct, listOfAllInsctPoint, listOfAllDoorPoint, listOfDoorObjId_And_IntersecPoint, flag_X_Axis, flag_Y_Axis, out listOfDoorPanelLine_ObjId, out listOfWallPanelLineId_WithDoor, wallPanelEnt_ForSamePropty);
                    foreach (var id in listOfWallPanelLineId_WithDoor)
                    {
                        listOfWallPanelLine_ObjId.Add(id);
                    }
                }
                else
                {
                    if (flagOfWallLineIsInsideTheDoor == false)
                    {
                        listOfWallPanelLine_ObjId.Add(wallPanel_With_WindowInsct_ObjId);
                    }
                }
            }
        }

        public static List<Point3d> getDoorWallPaneLinelPoint_ForCorner(ObjectId doorId, ObjectId wallLineId)
        {
            WindowHelper windowHlp = new WindowHelper();

            List<ObjectId> listOfExplodeId = AEE_Utility.ExplodeEntity(doorId);

            List<ObjectId> listOfMaxLengthExplodeLine = windowHlp.getMaxLengthOfWindowLineSegment(listOfExplodeId);

            List<double> listOfDistance = new List<double>();
            foreach (var doorMaxLngthLineId in listOfMaxLengthExplodeLine)
            {
                var distance = AEE_Utility.GetDistanceBetweenTwoLines(doorMaxLngthLineId, wallLineId);
                listOfDistance.Add(distance);
            }
            double minDistance = listOfDistance.Min();
            int indexOfMinDist = listOfDistance.IndexOf(minDistance);
            ObjectId minDistLineId = listOfMaxLengthExplodeLine[indexOfMinDist];

            List<Point3d> listOfStrtEndPoint = AEE_Utility.GetStartEndPointOfLine(minDistLineId);


            AEE_Utility.deleteEntity(listOfExplodeId);

            return listOfStrtEndPoint;
        }
        public static bool checkWallPanelLine_IsInsideThe_Window_Or_Door(ObjectId wallPanelId, ObjectId doorId)
        {
            bool flagOfWallLineIsInsideTheDoor = false;
            var listOfInsctPoint = AEE_Utility.InterSectionBetweenTwoEntity(doorId, wallPanelId);
            if (listOfInsctPoint.Count != 0)
            {
                //return flagOfWallLineIsInsideTheDoor;
            }

            WindowHelper windowHlp = new WindowHelper();
            List<ObjectId> listOfDoorExplodeIds = AEE_Utility.ExplodeEntity(doorId);
            var listOfMaxLengthLineId = windowHlp.getMaxLengthOfWindowLineSegment(listOfDoorExplodeIds);
            double doorLength = AEE_Utility.GetLengthOfLine(listOfMaxLengthLineId[0]);

            var listOfDoorVertex = AEE_Utility.GetPolylineVertexPoint(doorId);
            double maxX = listOfDoorVertex.Max(sortPoint => sortPoint.X);
            double maxY = listOfDoorVertex.Max(sortPoint => sortPoint.Y);

            Point3d offsetPoint = new Point3d((maxX + WindowHelper.windowOffsetValue), (maxY + WindowHelper.windowOffsetValue), 0);
            var offsetDoorId = AEE_Utility.OffsetEntity_WithoutLine(WindowHelper.windowOffsetValue, offsetPoint, doorId, false);


            var midPointOfWallPanel = AEE_Utility.GetMidPoint(wallPanelId);
            var flagOfInside = AEE_Utility.GetPointIsInsideThePolyline(offsetDoorId, midPointOfWallPanel);
            if (flagOfInside == true)
            {
                double wallPanelLength = AEE_Utility.GetLengthOfLine(wallPanelId);
                if ((doorLength + WindowHelper.windowOffsetValue) >= wallPanelLength)
                {
                    flagOfWallLineIsInsideTheDoor = true;
                }
            }

            AEE_Utility.deleteEntity(listOfDoorExplodeIds);
            AEE_Utility.deleteEntity(offsetDoorId);

            return flagOfWallLineIsInsideTheDoor;
        }
        public static void getStartOrEndPointOfDoor(List<Point3d> listOfInsctPoint, ObjectId wallPanelLineId, ObjectId doorId, bool flag_X_Axis, bool flag_Y_Axis, out Point3d doorStrtOrEndPoint, out Point3d wallStrtOrEndPoint)
        {
            CommonModule commonModl = new CommonModule();
            var wallPnelLine = AEE_Utility.GetLine(wallPanelLineId);
            var listOfStrtEndPoint = commonModl.getStartEndPointOfLine(wallPnelLine);
            var strtPointOfWallPanel = listOfStrtEndPoint[0];
            var endPointOfWallPanel = listOfStrtEndPoint[1];

            var listOfDoorVertexPoint = AEE_Utility.GetPolylineVertexPoint(doorId);
            var door_MinX = listOfDoorVertexPoint.Min(sortPoint => sortPoint.X);
            var door_MaxX = listOfDoorVertexPoint.Max(sortPoint => sortPoint.X);
            var door_MinY = listOfDoorVertexPoint.Min(sortPoint => sortPoint.Y);
            var door_MaxY = listOfDoorVertexPoint.Max(sortPoint => sortPoint.Y);

            doorStrtOrEndPoint = new Point3d();
            wallStrtOrEndPoint = new Point3d();

            Point3d insctPoint = listOfInsctPoint[0];
            if (flag_X_Axis == true)
            {
                if (insctPoint.X > strtPointOfWallPanel.X && door_MaxX > endPointOfWallPanel.X)
                {
                    doorStrtOrEndPoint = endPointOfWallPanel;
                    wallStrtOrEndPoint = strtPointOfWallPanel;
                }
                else
                {
                    doorStrtOrEndPoint = strtPointOfWallPanel;
                    wallStrtOrEndPoint = endPointOfWallPanel;
                }
            }
            else if (flag_Y_Axis == true)
            {
                if (insctPoint.Y > strtPointOfWallPanel.Y && door_MaxY > endPointOfWallPanel.Y)
                {
                    doorStrtOrEndPoint = endPointOfWallPanel;
                    wallStrtOrEndPoint = strtPointOfWallPanel;
                }
                else
                {
                    doorStrtOrEndPoint = strtPointOfWallPanel;
                    wallStrtOrEndPoint = endPointOfWallPanel;
                }
            }
        }
        private void drawDoorWallPanelLine(List<ObjectId> listOfDoorObjId_OfWallInsct, List<Point3d> listOfAllInsctPoint, List<Point3d> listOfAllDoorPoint, List<string> listOfDoorObjId_And_IntersecPoint, bool flag_X_Axis, bool flag_Y_Axis, out List<ObjectId> listOfDoorPanelLine_ObjId, out List<ObjectId> listOfWallPanelLine_ObjId, Entity wallPanelEnt_ForSamePropty)
        {
            listOfDoorPanelLine_ObjId = new List<ObjectId>();
            listOfWallPanelLine_ObjId = new List<ObjectId>();

            if (flag_X_Axis == true)
            {
                listOfAllInsctPoint = listOfAllInsctPoint.OrderBy(sortPoint => sortPoint.X).ToList();
                listOfAllDoorPoint = listOfAllDoorPoint.OrderBy(sortPoint => sortPoint.X).ToList();
            }
            else if (flag_Y_Axis == true)
            {
                listOfAllInsctPoint = listOfAllInsctPoint.OrderBy(sortPoint => sortPoint.Y).ToList();
                listOfAllDoorPoint = listOfAllDoorPoint.OrderBy(sortPoint => sortPoint.Y).ToList();
            }

            for (int k = 0; k < listOfAllInsctPoint.Count; k++)
            {
                if (k == (listOfAllInsctPoint.Count - 1))
                {
                    break;
                }
                Point3d point1 = listOfAllInsctPoint[k];
                Point3d point2 = listOfAllInsctPoint[k + 1];
                var length = AEE_Utility.GetLengthOfLine(point1.X, point1.Y, point2.X, point2.Y);
                if (length >= 1)
                {
                    bool flagOfExistWindowPoints = false;
                    for (int p = 0; p < listOfAllDoorPoint.Count; p = p + 2)
                    {
                        if (p == (listOfAllDoorPoint.Count - 1))
                        {
                            break;
                        }
                        Point3d doorPoint1 = listOfAllDoorPoint[p];
                        Point3d doorPoint2 = listOfAllDoorPoint[p + 1];


                        var lineId = AEE_Utility.getLineId(point1, point2, false);
                        var midPoint = AEE_Utility.GetMidPoint(lineId);
                        var flagOfInsidePoint1 = checkPointIdExistInDoor(listOfDoorObjId_OfWallInsct, midPoint);
                        AEE_Utility.deleteEntity(lineId);

                        if (flagOfInsidePoint1 == true)
                        {
                            flagOfExistWindowPoints = true;
                            break;
                        }
                    }
                    if (flagOfExistWindowPoints == false)
                    {
                        ObjectId wallPanel_LineId = AEE_Utility.getLineId(wallPanelEnt_ForSamePropty, point1.X, point1.Y, 0, point2.X, point2.Y, 0, false);
                        listOfWallPanelLine_ObjId.Add(wallPanel_LineId);
                    }
                }
            }

            for (int p = 0; p < listOfAllDoorPoint.Count; p = p + 2)
            {
                if (p == (listOfAllDoorPoint.Count - 1))
                {
                    break;
                }
                Point3d doorPoint1 = listOfAllDoorPoint[p];
                Point3d doorPoint2 = listOfAllDoorPoint[p + 1];

                ObjectId doorPanel_LineId = AEE_Utility.getLineId(wallPanelEnt_ForSamePropty, doorPoint1.X, doorPoint1.Y, 0, doorPoint2.X, doorPoint2.Y, 0, false);
                listOfDoorPanelLine_ObjId.Add(doorPanel_LineId);

                setDoorObjId_WithWindowPanelLine(listOfDoorObjId_And_IntersecPoint, doorPoint1, doorPoint2, doorPanel_LineId);
            }
        }

        private bool checkPointIdExistInDoor(List<ObjectId> listOfDoorObjId_OfWallInsct, Point3d point)
        {
            bool flagOfInside = false;
            foreach (var doorId in listOfDoorObjId_OfWallInsct)
            {
                flagOfInside = AEE_Utility.GetPointIsInsideThePolyline(doorId, point);
                if (flagOfInside == true)
                {
                    break;
                }
            }
            return flagOfInside;
        }
        private string setDoorObjId_WithWindowPanelLine(List<string> listOfDoorObjId_And_IntersecPoint, Point3d doorPoint1, Point3d doorPoint2, ObjectId doorPanel_LineId)
        {
            string outputDoorObjId = "";

            foreach (var data in listOfDoorObjId_And_IntersecPoint)
            {
                var array = data.Split('@');
                string doorObjId = Convert.ToString(array.GetValue(0));
                string doorInsctPoint = Convert.ToString(array.GetValue(1));
                if (doorInsctPoint == Convert.ToString(doorPoint1))
                {
                    outputDoorObjId = doorObjId;
                    break;
                }
                else if (doorInsctPoint == Convert.ToString(doorPoint2))
                {
                    outputDoorObjId = doorObjId;
                    break;
                }
            }

            if (outputDoorObjId != "")
            {
                if (listOfExistDoorPanelLineObj.Contains(outputDoorObjId))
                {
                    int indexOfExistDoor = listOfExistDoorPanelLineObj.IndexOf(outputDoorObjId);
                    string prvsData = listOfDoorObjId_With_DoorPanelLine[indexOfExistDoor];
                    string newData = prvsData + "," + Convert.ToString(doorPanel_LineId);
                    listOfDoorObjId_With_DoorPanelLine[listOfDoorObjId_With_DoorPanelLine.FindIndex(ind => ind.Equals(prvsData))] = newData;
                }
                else
                {
                    listOfDoorObjId_With_DoorPanelLine.Add(outputDoorObjId + "@" + Convert.ToString(doorPanel_LineId));
                    listOfExistDoorPanelLineObj.Add(outputDoorObjId);
                }
            }
            return outputDoorObjId;
        }
        public List<List<ObjectId>> setDoorPanelLines(string xDataRegAppName, ObjectId nearestWallToBeamWallId, double distBtwWallToBeam, ObjectId beamId_InsctWall, string beamLayerNameInsctToWall, List<ObjectId> listOfDoorPanelLine_Id, ObjectId cornerId1, ObjectId cornerId2, double wallPanelLineLength, bool flagOfSunkanSlab, ObjectId sunkanSlabId, ObjectId parapetId)
        {
            List<List<ObjectId>> listOfListOfDoorPanelsId = new List<List<ObjectId>>();

            InternalWallHelper internalHlper = new InternalWallHelper();
            List<ObjectId> listOfLineBtwTwoCrners_ObjId = new List<ObjectId>();
            List<double> listOfDistBtwTwoCrners = new List<double>();
            List<string> listOfDistAndObjId_BtwTwoCrners = new List<string>();
            List<ObjectId> listOfOffstDoorPanelLines_ObjId = new List<ObjectId>();
            internalHlper.getPanelLine(listOfDoorPanelLine_Id, cornerId1, cornerId2, wallPanelLineLength, out listOfLineBtwTwoCrners_ObjId, out listOfDistBtwTwoCrners, out listOfDistAndObjId_BtwTwoCrners, out listOfOffstDoorPanelLines_ObjId);

            for (int p = 0; p < listOfLineBtwTwoCrners_ObjId.Count; p++)
            {
                ObjectId doorPanelLine_ObjId = listOfLineBtwTwoCrners_ObjId[p];
                AEE_Utility.AttachXData(doorPanelLine_ObjId, xDataRegAppName, CommonModule.xDataAsciiName);

                listOfDoorPanelLine_ObjId.Add(doorPanelLine_ObjId);
                listOfDoorPanelLine_ObjId_InStr.Add(Convert.ToString(doorPanelLine_ObjId));
                ObjectId doorPanelOffsetLine_ObjId = listOfOffstDoorPanelLines_ObjId[p];
                listOfDoorPanelOffsetLine_ObjId.Add(doorPanelOffsetLine_ObjId);
                BeamHelper.listOfBeamName_InsctToDoorWall.Add(beamLayerNameInsctToWall);
                BeamHelper.listOfInsctDoor_BeamInsctId.Add(beamId_InsctWall);
                BeamHelper.listOfDistanceBtwDoorToBeam.Add(distBtwWallToBeam);
                BeamHelper.listOfNearestBtwDoorToBeamBeamWallLineId.Add(nearestWallToBeamWallId);

                SunkanSlabHelper.listOfDoorWall_SunkanSlabWallLineId.Add(doorPanelLine_ObjId);
                SunkanSlabHelper.listOfDoorWall_SunkanSlabWallLineflag.Add(flagOfSunkanSlab);
                SunkanSlabHelper.listOfDoorWall_SunkanSlabId.Add(sunkanSlabId);

                ParapetHelper.listOfParapetId_WithDoorWallLine.Add(parapetId);
                ParapetHelper.listOfDoorWallLineId_InsctToParapet.Add(doorPanelLine_ObjId);

                List<ObjectId> listOfDoorPanelsId = new List<ObjectId>();
                listOfDoorPanelsId.Add(doorPanelLine_ObjId);
                listOfDoorPanelsId.Add(doorPanelOffsetLine_ObjId);
                listOfListOfDoorPanelsId.Add(listOfDoorPanelsId);
            }
            return listOfListOfDoorPanelsId;
        }

        public void createCornersInDoor(string xDataRegAppName, ObjectId beamId_InsctWall, string beamLayerNameInsctToWall, List<List<ObjectId>> listOfListOfDoorPanelsId, ObjectId offsetWall_ObjId, ObjectId wallCornerId1, ObjectId wallCornerId2, List<ObjectId> cornerId1_TextId, List<ObjectId> cornerId2_TextId, ObjectId sunkanSlabId, ObjectId parapetId)
        {
            PartHelper partHelper = new PartHelper();

            for (int i = 0; i < listOfListOfDoorPanelsId.Count; i++)
            {
                var listOfDoorPanelLine_ObjId = listOfListOfDoorPanelsId[i];
                ObjectId doorWallPanelId = listOfDoorPanelLine_ObjId[0];
                ObjectId offsetDoorWallPanelId = listOfDoorPanelLine_ObjId[1];

                ObjectId doorId = getDoorId_ForCorner(doorWallPanelId);

                double doorWallAngle = AEE_Utility.GetAngleOfLine(doorWallPanelId);
                List<double> listOfAngle = new List<double>();
                listOfAngle.Add(doorWallAngle);
                listOfAngle.Add(doorWallAngle + CommonModule.angle_90);
                listOfAngle.Add(doorWallAngle + CommonModule.angle_180);
                listOfAngle.Add(doorWallAngle + CommonModule.angle_270);

                var listOfStrtEndPoint = AEE_Utility.GetStartEndPointOfLine(doorWallPanelId);
                List<ObjectId> listOfDoorCrnrId_NotInsctToWallCrnr = new List<ObjectId>();
                List<List<ObjectId>> listOfDoorCrnrTextId_NotInsctToWallCrnr = new List<List<ObjectId>>();
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

                        var flagOfCorner = drawDoorCorner(externalCornerId, basePoint, angle, doorWallPanelId, offsetWall_ObjId);

                        if (flagOfCorner == true)
                        {
                            List<ObjectId> existCornerTextId_With_Door = new List<ObjectId>();
                            ObjectId existCornerId_With_Door = new ObjectId();
                            ObjectId overlapCornerId = new ObjectId();
                            List<ObjectId> overlapCornerTextIds = new List<ObjectId>();

                            bool flagOfExist = checkCornerIsAlreadyInDoorVertex(doorId, doorWallPanelId, externalCornerId, wallCornerId1, wallCornerId2, cornerId1_TextId, cornerId2_TextId, out existCornerTextId_With_Door, out existCornerId_With_Door, overlapCornerId, overlapCornerTextIds);

                            double doorCornerWallHeight = 0;
                            List<ObjectId> cornerTextIds = writeTextInDoorCorner(xDataRegAppName, beamLayerNameInsctToWall, doorId, doorWallPanelId, basePoint, angle, existCornerId_With_Door, existCornerTextId_With_Door, externalCornerId, sunkanSlabId, parapetId, out doorCornerWallHeight);

                            if (flagOfExist == true)
                            {
                                Point2d nearestDoorVertexPoint = new Point2d();
                                var existCornerBasePoint = AEE_Utility.GetBasePointOfPolyline(existCornerId_With_Door);
                                var listOfDoorVertex = AEE_Utility.GetPolylineVertexPoint(doorId);
                                double distanceBtwTwoDoor = getDistanceBetweenDoorToCorner(listOfDoorVertex, existCornerBasePoint, out nearestDoorVertexPoint);
                                //BY MT 03/05/2022------------------------------------------------                             
                                bool isMatchDoorVertex = false;
                                if (Math.Truncate(distanceBtwTwoDoor) == 0 && InternalWallHelper.flagForDoorCorner && !InternalWallHelper.listOfNearestCornerPoint.Contains(nearestDoorVertexPoint))
                                {
                                    int drIndex = DoorHelper.listOfDoorObjId.IndexOf(doorId);
                                    for (int l = 0; l < DoorHelper.listOfDoorObjId.Count; l++)
                                    {
                                        if (l != drIndex)
                                        {
                                            var nxtDoorVertext = AEE_Utility.GetPolylineVertexPoint(DoorHelper.listOfDoorObjId[l]);
                                            isMatchDoorVertex = nxtDoorVertext.IndexOf(nearestDoorVertexPoint) != -1;
                                            if (isMatchDoorVertex && !InternalWallHelper.listOfDoorCornerId.Contains(externalCornerId))
                                            {
                                                var mmtxt = AEE_Utility.GetEntityForRead(cornerId1_TextId[0]) as MText;
                                                InternalWallHelper.listOfNearestCornerPoint.Add(nearestDoorVertexPoint);
                                                //InternalWallHelper.listOfDoorCornerId.Add(externalCornerId);
                                                //InternalWallHelper.listOfDoorCornerTextId.Add(cornerTextIds);
                                            }
                                            if (isMatchDoorVertex)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                }
                                //------------------------------------------------------------------------
                                //if ((Math.Truncate(distanceBtwTwoDoor) == 0 && distanceBtwTwoDoor != 0) || (distanceBtwTwoDoor == 0 && (InternalWallHelper.cornerbasePoint.Contains(existCornerBasePoint) || !InternalWallHelper.flagForDoorCorner))) //By MT on 29/04/2022
                                if ((Math.Truncate(distanceBtwTwoDoor) == 0 && !InternalWallHelper.flagForDoorCorner) || (!isMatchDoorVertex || !InternalWallHelper.flagForDoorCorner)) //By MT on 29/04/2022
                                {
                                    AEE_Utility.deleteEntity(externalCornerId);
                                }
                                else
                                {
                                    drawCornerIfDistnc_Of_DoorDiff(xDataRegAppName, doorId, externalCornerId, angle, nearestDoorVertexPoint, isMatchDoorVertex);
                                }

                                listOfMoveCornerIds_In_New_ShellPlan.Add(existCornerId_With_Door);
                                listOfMoveCornerIds_In_New_ShellPlan.AddRange(cornerTextIds);

                                setDataOfDoorInsctToCorners(existCornerId_With_Door, doorId);

                                if (doorCornerWallHeight <= 0)
                                {
                                    listOfCornerIfDoorXHeightIsLessThanZero.Add(existCornerId_With_Door);
                                    listOfCornerIfDoorXHeightIsLessThanZero.AddRange(cornerTextIds);
                                }
                                else
                                {
                                    changeDoorWallPanelId_With_CornerInsct(existCornerId_With_Door, doorWallPanelId, offsetDoorWallPanelId, doorId);
                                }
                            }
                            else
                            {
                                if (overlapCornerId.IsValid == true)
                                {
                                    Point2d nearestDoorVertexPoint = new Point2d();
                                    var existCornerBasePoint = AEE_Utility.GetBasePointOfPolyline(overlapCornerId);
                                    var listOfDoorVertex = AEE_Utility.GetPolylineVertexPoint(doorId);
                                    double distanceBtwTwoDoor = getDistanceBetweenDoorToCorner(listOfDoorVertex, existCornerBasePoint, out nearestDoorVertexPoint);

                                    var canAdd = (distanceBtwTwoDoor - doorThicknessLength) > -1e-6;
                                    if (!canAdd)
                                    {
                                        listOverlapExternals.Add(new OverlapExternalDoorCorners()
                                        {
                                            CornerId = externalCornerId,
                                            TextIds = cornerTextIds,
                                            WallCornerId = overlapCornerId,
                                            WallCornerTextId = overlapCornerTextIds,
                                            WallCornerBasePoint = existCornerBasePoint,
                                            WallCornerPointOnDoor = nearestDoorVertexPoint
                                        });
                                    }
                                }

                                if (!dicDoorToEACorners.ContainsKey(doorId))
                                {
                                    dicDoorToEACorners[doorId] = new List<DoorEACorner>();
                                }
                                dicDoorToEACorners[doorId].Add(new DoorEACorner()
                                {
                                    WallCornerPointOnDoor = basePoint,
                                    CornerId = externalCornerId,
                                    TextIds = cornerTextIds,
                                });
                                listOfDoorCrnrId_NotInsctToWallCrnr.Add(externalCornerId);
                                listOfDoorCrnrTextId_NotInsctToWallCrnr.Add(cornerTextIds);
                            }
                            break;
                        }
                    }
                }
                setDataOfDoorCornerId_NotInsctToWallCornerId(doorId, listOfDoorCrnrId_NotInsctToWallCrnr, listOfDoorCrnrTextId_NotInsctToWallCrnr);
            }
        }

        public static void changeDoorWallPanelId_With_CornerInsct(ObjectId existCornerId_With_Door, ObjectId doorWallPanelId, ObjectId offsetDoorWallPanelId, ObjectId doorId)
        {
            var listOfInsct = AEE_Utility.InterSectionBetweenTwoEntity(existCornerId_With_Door, doorWallPanelId);
            var existCornerId_With_Door_Ent = AEE_Utility.GetEntityForRead(existCornerId_With_Door);
            if (listOfInsct.Count != 0 && existCornerId_With_Door_Ent.Layer != CommonModule.externalCornerLyrName)
            {
                var existCornrBasePnt = AEE_Utility.GetBasePointOfPolyline(existCornerId_With_Door);
                List<double> listOfLength = new List<double>();
                foreach (var insct in listOfInsct)
                {
                    var length = AEE_Utility.GetLengthOfLine(existCornrBasePnt, insct);
                    listOfLength.Add(length);
                }
                double maxLngth = listOfLength.Max();
                int indexOfMaxLngth = listOfLength.IndexOf(maxLngth);
                Point3d maxLngthPointFrmBasePntOfCrnr = listOfInsct[indexOfMaxLngth];

                List<Point3d> listOfStrtEndPoint = AEE_Utility.GetStartEndPointOfLine(doorWallPanelId);
                listOfLength = new List<double>();
                foreach (var linePoint in listOfStrtEndPoint)
                {
                    var length = AEE_Utility.GetLengthOfLine(maxLngthPointFrmBasePntOfCrnr, linePoint);
                    listOfLength.Add(length);
                }
                maxLngth = listOfLength.Max();
                indexOfMaxLngth = listOfLength.IndexOf(maxLngth);
                Point3d maxLngthPointOfDoorLine = listOfStrtEndPoint[indexOfMaxLngth];

                InternalWallHelper internalWallHlp = new InternalWallHelper();
                internalWallHlp.changeLinePoint(doorWallPanelId, maxLngthPointFrmBasePntOfCrnr, maxLngthPointOfDoorLine);


                double panelDepth = CommonModule.panelDepth;
                var offsetLineId = AEE_Utility.OffsetLine(doorWallPanelId, 10, false);
                var midPoint = AEE_Utility.GetMidPoint(offsetLineId);
                bool flagOfInside = AEE_Utility.GetPointIsInsideThePolyline(doorId, midPoint);

                if (flagOfInside == true)
                {
                    AEE_Utility.deleteEntity(offsetLineId);
                    offsetLineId = AEE_Utility.OffsetLine(doorWallPanelId, -panelDepth, false);
                }
                else
                {
                    offsetLineId = AEE_Utility.OffsetLine(doorWallPanelId, panelDepth, false);
                }
                var listOfOffstStrtEndPoint = AEE_Utility.GetStartEndPointOfLine(offsetLineId);
                internalWallHlp.changeLinePoint(offsetDoorWallPanelId, listOfOffstStrtEndPoint[0], listOfOffstStrtEndPoint[1]);

                AEE_Utility.deleteEntity(offsetLineId);
            }
        }

        public static bool drawDoorCorner(ObjectId externalCornerId, Point3d basePoint, double angle, ObjectId doorWallPanelId, ObjectId offsetWall_ObjId)
        {
            CommonModule.rotateAngle = (angle * Math.PI) / 180; // variable assign for rotation             
            CommonModule.basePoint = new Point3d(basePoint.X, basePoint.Y, 0);
            List<ObjectId> listOfObjId = new List<ObjectId>();
            listOfObjId.Add(externalCornerId);
            MoveRorationScaleJig.RotationScaleJig_Method(listOfObjId);

            var listOfInsctWithOffsetWall = AEE_Utility.InterSectionBetweenTwoEntity(externalCornerId, offsetWall_ObjId);
            if (listOfInsctWithOffsetWall.Count == 0)
            {
                AEE_Utility.deleteEntity(externalCornerId);
            }
            else
            {
                var listOfInsctWithDoorPanelLine = AEE_Utility.InterSectionBetweenTwoEntity(externalCornerId, doorWallPanelId);
                if (listOfInsctWithDoorPanelLine.Count <= 1)
                {
                    AEE_Utility.deleteEntity(externalCornerId);
                }
                else
                {
                    Point3d point1 = listOfInsctWithDoorPanelLine[0];
                    Point3d point2 = listOfInsctWithDoorPanelLine[1];
                    var length = AEE_Utility.GetLengthOfLine(point1.X, point1.Y, point2.X, point2.Y);
                    if (length <= 1)
                    {
                        AEE_Utility.deleteEntity(externalCornerId);
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void drawCornerIfDistnc_Of_DoorDiff(string xDataRegAppName, ObjectId doorId, ObjectId externalCornerId, double angle, Point2d nearestDoorVertexPoint, bool isDoorCornerIntersect)
        {
            // if gap is available between door and corner
            var door_Ent = AEE_Utility.GetEntityForRead(doorId);
            double door_LintelHght = Door_UI_Helper.getDoorLintelLevel(door_Ent.Layer);
            string cornerText = CornerHelper.getCornerText(CommonModule.externalCornerText, CommonModule.extrnlCornr_Flange, CommonModule.extrnlCornr_Flange, door_LintelHght);
            double crnerRotationAngle = angle + CommonModule.cornerRotationAngle;
            CornerHelper cornerHlp = new CornerHelper();
            var externalCornerTextId = cornerHlp.writeTextInCorner(externalCornerId, cornerText, crnerRotationAngle, CommonModule.externalCornerTextLyrName, CommonModule.externalCornerLyrColor);

            CornerHelper.setCornerDataForBOM(xDataRegAppName, externalCornerId, externalCornerTextId, cornerText, CommonModule.externalCornerDescp, CommonModule.externalCornerText, CommonModule.extrnlCornr_Flange, CommonModule.extrnlCornr_Flange, door_LintelHght/*, 0.0/*elev*/);

            var externalCornerBasePoint = AEE_Utility.GetBasePointOfPolyline(externalCornerId);

            double x = nearestDoorVertexPoint.X - externalCornerBasePoint.X;
            double y = nearestDoorVertexPoint.Y - externalCornerBasePoint.Y;
            Vector3d moveVector = new Vector3d(x, y, 0);
            AEE_Utility.MoveEntity(externalCornerId, moveVector);
            AEE_Utility.MoveEntity(externalCornerTextId, moveVector);
            //By MT on 07/05/2022------
            if (isDoorCornerIntersect)
            {
                InternalWallHelper.listOfDoorCornerId.Add(externalCornerId);
                InternalWallHelper.listOfDoorCornerTextId.Add(new List<ObjectId>() { externalCornerTextId });
            }
            //--------------------------------
        }
        public static bool checkCornerIsAlreadyInDoorVertex(ObjectId doorId, ObjectId doorWallPanelId, ObjectId doorCornerId,
            ObjectId wallCornerId1, ObjectId wallCornerId2, List<ObjectId> cornerId1_TextId, List<ObjectId> cornerId2_TextId,
            out List<ObjectId> existCornerTextId_With_Door, out ObjectId existCornerId_With_Door, ObjectId overlapCornerId, List<ObjectId> overlapCornerTextIds)
        {
            existCornerId_With_Door = new ObjectId();
            existCornerTextId_With_Door = new List<ObjectId>();
            bool flagOfExist = false;
            var listOfDoorCornerVertex = AEE_Utility.GetPolylineVertexPoint(doorCornerId);
            double maxX = listOfDoorCornerVertex.Max(sortPoint => sortPoint.X);
            double maxY = listOfDoorCornerVertex.Max(sortPoint => sortPoint.Y);
            Point3d offsetPoint = new Point3d((maxX + 10), (maxY + 10), 0);
            //ObjectId offsetDoorCornerId = AEE_Utility.OffsetEntity_WithoutLine(10, offsetPoint, doorCornerId, false);

            var basePointOfDoorCorner = AEE_Utility.GetBasePointOfPolyline(doorCornerId);
            var basePointOfWallCorner1 = AEE_Utility.GetBasePointOfPolyline(wallCornerId1);
            var basePointOfWallCorner2 = AEE_Utility.GetBasePointOfPolyline(wallCornerId2);

            var lngthBtwDoorToWall_Corner1 = AEE_Utility.GetLengthOfLine(basePointOfDoorCorner, basePointOfWallCorner1);
            lngthBtwDoorToWall_Corner1 = Math.Truncate(lngthBtwDoorToWall_Corner1);

            var lngthBtwDoorToWall_Corner2 = AEE_Utility.GetLengthOfLine(basePointOfDoorCorner, basePointOfWallCorner2);
            lngthBtwDoorToWall_Corner2 = Math.Truncate(lngthBtwDoorToWall_Corner2);

            var listOfInsctPoint = AEE_Utility.InterSectionBetweenTwoEntity(doorCornerId, wallCornerId1);
            if (listOfInsctPoint.Count != 0)
            {
                bool isOverlaps = false;
                bool flagOfCrnrExist = checkDistncBtwDoorCrnrToWallCrnr(doorWallPanelId, doorId, wallCornerId1, doorCornerId, listOfInsctPoint, ref isOverlaps);
                if (flagOfCrnrExist == true)
                {
                    flagOfExist = true;
                    existCornerTextId_With_Door = cornerId1_TextId;
                    existCornerId_With_Door = wallCornerId1;
                }
                if (isOverlaps)
                {
                    overlapCornerId = wallCornerId1;
                    overlapCornerTextIds = cornerId1_TextId;
                }
            }
            listOfInsctPoint = AEE_Utility.InterSectionBetweenTwoEntity(doorCornerId, wallCornerId2);
            if (listOfInsctPoint.Count != 0)
            {
                bool isOverlaps = false;
                bool flagOfCrnrExist = checkDistncBtwDoorCrnrToWallCrnr(doorWallPanelId, doorId, wallCornerId2, doorCornerId, listOfInsctPoint, ref isOverlaps);
                if (flagOfCrnrExist == true)
                {
                    flagOfExist = true;
                    existCornerTextId_With_Door = cornerId2_TextId;
                    existCornerId_With_Door = wallCornerId2;
                }
                if (isOverlaps)
                {
                    overlapCornerId = wallCornerId2;
                    overlapCornerTextIds = cornerId2_TextId;
                }
            }
            //AEE_Utility.deleteEntity(offsetDoorCornerId);
            return flagOfExist;
        }

        public static bool checkDistncBtwDoorCrnrToWallCrnr(ObjectId doorWallPanelId, ObjectId doorId, ObjectId wallCornerId,
            ObjectId doorCornerId, List<Point3d> listOfInsctPointOfWallCrner, ref bool isOverlaps)
        {
            bool flagOfExist = false;
            var doorCornerBasePoint = AEE_Utility.GetBasePointOfPolyline(doorCornerId);
            var wallCornerBasePoint = AEE_Utility.GetBasePointOfPolyline(wallCornerId);

            var distBtwDoorCrnrToWallCrnr = AEE_Utility.GetLengthOfLine(doorCornerBasePoint, wallCornerBasePoint);
            distBtwDoorCrnrToWallCrnr = Math.Truncate(distBtwDoorCrnrToWallCrnr);
            if (distBtwDoorCrnrToWallCrnr <= 0)
            {
                flagOfExist = true;
                return flagOfExist;
            }

            foreach (var insctPoint in listOfInsctPointOfWallCrner)
            {
                var distance = AEE_Utility.GetLengthOfLine(doorCornerBasePoint, insctPoint);
                distance = Math.Truncate(distance);
                if (distance < 0)
                {
                    flagOfExist = true;
                }
                if (distance <= 0)
                {
                    isOverlaps = true;
                }
            }

            return flagOfExist;
        }
        public static List<ObjectId> writeTextInDoorCorner(string xDataRegAppName, string beamLayerNameInsctToWall, ObjectId doorId, ObjectId doorWallPanelId, Point3d basePoint, double rotationAngle, ObjectId existCornerId_With_Door, List<ObjectId> existCornerTextId_With_Door, ObjectId newExternalCornerId, ObjectId sunkanSlabId, ObjectId parapetId, out double cornerWallHeight)
        {
            cornerWallHeight = 0;
            List<ObjectId> cornerTextIds = new List<ObjectId>();

            var doorEnt = AEE_Utility.GetEntityForRead(doorId);

            //Fix IAX elevation by SDM 2022-08-03
            if (SunkanSlabHelper.listOfWallName_SunkanSlabId.ContainsKey(xDataRegAppName))
                sunkanSlabId = SunkanSlabHelper.listOfWallName_SunkanSlabId[xDataRegAppName];

            List<List<object>> listOfCornerData_ForBOM = new List<List<object>>();
            CornerHelper cornerHlpr = new CornerHelper();
            var doorWallPanelLineEntity = AEE_Utility.GetEntityForRead(doorWallPanelId);
            List<string> cornerTexts = getCornerText(doorEnt.Layer, beamLayerNameInsctToWall, doorWallPanelLineEntity, existCornerId_With_Door, newExternalCornerId, sunkanSlabId, out listOfCornerData_ForBOM);

            if (existCornerId_With_Door.IsValid == true)
            {
                //Added to Fix IAX when wall height more than 3000 by SDM 2022-08-04
                CornerElevation elev = CornerHelper.DeleteCornerData_ForBOM(existCornerId_With_Door);//Updated by SDM 2022-08-13

                for (int j = 0; j < existCornerTextId_With_Door.Count; j++)
                {
                    if (listOfCornerData_ForBOM.Count <= j)
                    {
                        break; // Testing - 123
                    }
                    string cornerDescp = Convert.ToString(listOfCornerData_ForBOM[j][0]);
                    string cornerType = Convert.ToString(listOfCornerData_ForBOM[j][1]);
                    double flange1 = Convert.ToDouble(listOfCornerData_ForBOM[j][2]);
                    double flange2 = Convert.ToDouble(listOfCornerData_ForBOM[j][3]);
                    cornerWallHeight = Convert.ToDouble(listOfCornerData_ForBOM[j][4]);

                    AEE_Utility.changeMText(existCornerTextId_With_Door[j], cornerTexts[j]);
                    //Added to Fix IAX when wall height more than 3000 by SDM 2022-08-04
                    if (elev != null)
                    {
                        double rc = sunkanSlabId.IsValid ? PanelLayout_UI.RC : 0;//Fix IAX elevation by SDM 2022-08-03
                        elev.elev = Door_UI_Helper.getDoorLintelLevel(doorEnt.Layer) - rc;
                    }

                    CornerHelper.setCornerDataForBOM(xDataRegAppName, existCornerId_With_Door, existCornerTextId_With_Door[j], cornerTexts[j], cornerDescp, cornerType, flange1, flange2, cornerWallHeight, elev);

                    //Commented to Fix IAX when wall height more than 3000 by SDM 2022-08-04
                    //// Update Door Corner elevation level 20-10-2021 PBAC
                    //int indexOfExistCornerId = CornerHelper.listOfAllCornerId_ForBOM.IndexOf(existCornerId_With_Door);

                    //if (indexOfExistCornerId != -1)
                    //{
                    //    double rc = sunkanSlabId.IsValid ? PanelLayout_UI.RC : 0;//Fix IAX elevation by SDM 2022-08-03
                    //    CornerHelper.listOfAllCornerElevInfo[indexOfExistCornerId].elev = Door_UI_Helper.getDoorLintelLevel(doorEnt.Layer) - rc;/* + PanelLayout_UI.SL;*/
                    //}

                }
                cornerTextIds = existCornerTextId_With_Door;
                listOfDoorCornerTextId_ForBeamTextChange.Add(cornerTextIds); // stored the value for beam corner text change
            }
            else
            {
                double crnerRotationAngle = rotationAngle + CommonModule.cornerRotationAngle;
                cornerTextIds = cornerHlpr.writeMultipleTextInCorner(newExternalCornerId, cornerTexts, crnerRotationAngle, CommonModule.externalCornerTextLyrName, CommonModule.externalCornerLyrColor);


                ParapetHelper parapetHlp = new ParapetHelper();
                for (int j = 0; j < cornerTextIds.Count; j++)
                {
                    string cornerDescp = Convert.ToString(listOfCornerData_ForBOM[j][0]);
                    string cornerType = Convert.ToString(listOfCornerData_ForBOM[j][1]);
                    double flange1 = Convert.ToDouble(listOfCornerData_ForBOM[j][2]);
                    double flange2 = Convert.ToDouble(listOfCornerData_ForBOM[j][3]);
                    cornerWallHeight = Convert.ToDouble(listOfCornerData_ForBOM[j][4]);

                    bool flagOfParapet = false;
                    parapetHlp.changeHeightOfWindowOrDoorCornerWithParapet(xDataRegAppName, parapetId, newExternalCornerId, cornerTextIds, sunkanSlabId, doorWallPanelLineEntity.Layer, out flagOfParapet);
                    if (flagOfParapet == false)
                    {
                        CornerHelper.setCornerDataForBOM(xDataRegAppName, newExternalCornerId, cornerTextIds[j], cornerTexts[j], cornerDescp, cornerType, flange1, flange2, cornerWallHeight/*, 0.0/*elev*/);
                    }
                }
            }
            return cornerTextIds;
        }

        private ObjectId getDoorId_ForCorner(ObjectId doorWallPanelId)
        {
            ObjectId doorId = new ObjectId();
            string doorLayer = "";
            foreach (var data in listOfDoorObjId_With_DoorPanelLine)
            {
                string[] splitWithAddTheRateSymbol = data.Split('@');
                string doorIdStr = Convert.ToString(splitWithAddTheRateSymbol.GetValue(0));
                string doorWallPanelLinesStr = Convert.ToString(splitWithAddTheRateSymbol.GetValue(1));
                string[] splitWithCommas = doorWallPanelLinesStr.Split(',');
                foreach (string doorPanelLineId_Str in splitWithCommas)
                {
                    if (doorPanelLineId_Str == Convert.ToString(doorWallPanelId))
                    {
                        int index = listOfDoorObjId_Str.IndexOf(doorIdStr);
                        if (index != -1)
                        {
                            doorId = listOfDoorObjId[index];
                            var doorEnt = AEE_Utility.GetEntityForRead(doorId);
                            doorLayer = doorEnt.Layer;
                            break;
                        }
                    }
                }
            }
            return doorId;
        }
        public static List<string> getCornerText(string doorLayer, string beamLayerNameInsctToWall, Entity doorWallPanelLineEntity, ObjectId existCornerId_With_Door, ObjectId newExternalCornerId, ObjectId sunkanSlabId, out List<List<object>> listOfCornerData_ForBOM)
        {
            listOfCornerData_ForBOM = new List<List<object>>();
            List<string> listOfCornerTexts = new List<string>();

            ObjectId cornerId = new ObjectId();

            if (existCornerId_With_Door.IsValid == true)
            {
                cornerId = existCornerId_With_Door;
            }
            else
            {
                cornerId = newExternalCornerId;
            }

            string cornerText = "";
            double cornerWallHeight = 0;
            double doorLintelLevel = Door_UI_Helper.getDoorLintelLevel(doorLayer);
            double beamBottom = Beam_UI_Helper.getOffsetBeamBottom(beamLayerNameInsctToWall);
            if (existCornerId_With_Door.IsValid == true)
            {
                if (InternalWallSlab_UI_Helper.IsInternalWall(doorWallPanelLineEntity.Layer) || StairCase_UI_Helper.checkStairCaseLayerIsExist(doorWallPanelLineEntity.Layer))
                {
                    cornerWallHeight = GeometryHelper.getHeightOfDoorX_InAtCornerSide_EC_IC_InternallWall(InternalWallSlab_UI_Helper.getSlabBottom(doorWallPanelLineEntity.Layer), beamBottom, doorLintelLevel, PanelLayout_UI.SL, CommonModule.internalCorner);
                    double levelDifferenceOfSunkanSlb = SunkanSlabHelper.getSunkanSlabLevelDifference(sunkanSlabId);
                    cornerWallHeight -= levelDifferenceOfSunkanSlb;
                }
                else if (doorWallPanelLineEntity.Layer == CommonModule.externalWallLayerName || doorWallPanelLineEntity.Layer == CommonModule.ductLayerName || Lift_UI_Helper.checkLiftLayerIsExist(doorWallPanelLineEntity.Layer))
                {
                    cornerWallHeight = GeometryHelper.getHeightOfDoorX_InAtCornerSide_EC_IC_ExternallWall(InternalWallSlab_UI_Helper.getSlabBottom(doorWallPanelLineEntity.Layer), beamBottom, PanelLayout_UI.getSlabThickness(doorWallPanelLineEntity.Layer), doorLintelLevel, PanelLayout_UI.kickerHt, PanelLayout_UI.RC);
                }
            }
            else
            {
                double doorCornerHght = GeometryHelper.getHeightOfDoorCorner(doorLayer, CommonModule.internalCorner);
                double levelDifferenceOfSunkanSlb = SunkanSlabHelper.getSunkanSlabLevelDifference_LessThan_RC__Corners(sunkanSlabId);
                cornerWallHeight = doorCornerHght + levelDifferenceOfSunkanSlb;
            }

            var cornerEnt = AEE_Utility.GetEntityForRead(cornerId);
            if (cornerEnt.Layer == CommonModule.internalCornerLyrName)
            {
                listOfCornerTexts = CornerHelper.UpdateCornerText(listOfCornerData_ForBOM, cornerWallHeight, sunkanSlabId);
            }
            else if (cornerEnt.Layer == CommonModule.externalCornerLyrName)
            {
                listOfCornerTexts = CornerHelper.UpdateCornerTextExternal(listOfCornerData_ForBOM, cornerWallHeight);

            }

            return listOfCornerTexts;
        }


        public static void setDataOfDoorInsctToCorners(ObjectId cornerId, ObjectId doorId)
        {
            List<ObjectId> listOfCornerId_InsctToDoor = new List<ObjectId>();
            if (listOfExistDoorId_InsctToCorner_ForCheck.Contains(doorId))
            {
                int index = listOfExistDoorId_InsctToCorner_ForCheck.IndexOf(doorId);
                List<ObjectId> listOfPrvsCornerId_InsctToDoor = listOfListDoorId_With_InsctCornerId[index];
                foreach (var prvsCornerId in listOfPrvsCornerId_InsctToDoor)
                {
                    listOfCornerId_InsctToDoor.Add(prvsCornerId);
                }
                listOfCornerId_InsctToDoor.Add(cornerId);
                listOfListDoorId_With_InsctCornerId[listOfListDoorId_With_InsctCornerId.FindIndex(ind => ind.Equals(listOfPrvsCornerId_InsctToDoor))] = listOfCornerId_InsctToDoor;
            }
            else
            {
                listOfExistDoorId_InsctToCorner_ForCheck.Add(doorId);

                listOfCornerId_InsctToDoor.Add(cornerId);
                listOfListDoorId_With_InsctCornerId.Add(listOfCornerId_InsctToDoor);
            }

            listOfAllCornerId_InsctDoor_ForCheck.Add(cornerId);


            // add corner id with door insct
            List<ObjectId> listOfDoorId_InsctToCorner = new List<ObjectId>();
            if (listOfExistCornerId_ForCheck.Contains(cornerId))
            {
                int index = listOfExistCornerId_ForCheck.IndexOf(cornerId);
                List<ObjectId> listOfPrvsDoorId_InsctToCorner = listOfListCornerId_With_InsctDoorId[index];
                foreach (var prvsDoorId in listOfPrvsDoorId_InsctToCorner)
                {
                    listOfDoorId_InsctToCorner.Add(prvsDoorId);
                }
                listOfDoorId_InsctToCorner.Add(doorId);
                listOfListCornerId_With_InsctDoorId[listOfListCornerId_With_InsctDoorId.FindIndex(ind => ind.Equals(listOfPrvsDoorId_InsctToCorner))] = listOfDoorId_InsctToCorner;

            }
            else
            {
                listOfExistCornerId_ForCheck.Add(cornerId);

                listOfDoorId_InsctToCorner.Add(doorId);
                listOfListCornerId_With_InsctDoorId.Add(listOfDoorId_InsctToCorner);
            }
        }

        private void setDataOfDoorCornerId_NotInsctToWallCornerId(ObjectId doorId, List<ObjectId> listOfDoorCornerId, List<List<ObjectId>> listOfDoorCornerTextId)
        {
            if (listOfDoorCornerId.Count == 0)
            {
                return;
            }

            int indexOfExist = listOfDoorId_NotInsctToCorner.IndexOf(doorId);
            if (indexOfExist == -1)
            {
                listOfDoorId_NotInsctToCorner.Add(doorId);
                listOfListOfDoorCornerId_NotInsctToWallCorner.Add(listOfDoorCornerId);
                listOfListOfDoorCornerTextId_NotInsctToWallCorner.Add(listOfDoorCornerTextId);
            }
            else
            {
                var listOfPrvsDoorCornerId_NotInsctToWallCorner = listOfListOfDoorCornerId_NotInsctToWallCorner[indexOfExist];

                var listOfNewDoorCornerId_NotInsctToWallCorner = listOfPrvsDoorCornerId_NotInsctToWallCorner;
                foreach (var doorCornerId in listOfDoorCornerId)
                {
                    listOfNewDoorCornerId_NotInsctToWallCorner.Add(doorCornerId);
                }
                listOfListOfDoorCornerId_NotInsctToWallCorner[listOfListOfDoorCornerId_NotInsctToWallCorner.FindIndex(ind => ind.Equals(listOfPrvsDoorCornerId_NotInsctToWallCorner))] = listOfNewDoorCornerId_NotInsctToWallCorner;


                var listOfPrvsDoorCornerTextId_NotInsctToWallCorner = listOfListOfDoorCornerTextId_NotInsctToWallCorner[indexOfExist];
                var listOfNewDoorCornerTextId_NotInsctToWallCorner = listOfPrvsDoorCornerTextId_NotInsctToWallCorner;
                foreach (var doorCornerTextId in listOfDoorCornerTextId)
                {
                    listOfNewDoorCornerTextId_NotInsctToWallCorner.Add(doorCornerTextId);
                }
                listOfListOfDoorCornerTextId_NotInsctToWallCorner[listOfListOfDoorCornerTextId_NotInsctToWallCorner.FindIndex(ind => ind.Equals(listOfPrvsDoorCornerTextId_NotInsctToWallCorner))] = listOfNewDoorCornerTextId_NotInsctToWallCorner;
            }
        }


        public void drawDoorThicknessWallPanel()
        {
            WindowHelper windowHlp = new WindowHelper();

            for (int i = 0; i < listOfExistDoorId_InsctToCorner_ForCheck.Count; i++)
            {
                ObjectId doorId = listOfExistDoorId_InsctToCorner_ForCheck[i];
                var xDataRegAppName = AEE_Utility.GetXDataRegisterAppName(doorId);

                List<ObjectId> listOfCornerId_InsctToDoor = listOfListDoorId_With_InsctCornerId[i];
                List<ObjectId> listOfExplodeLineId = AEE_Utility.ExplodeEntity(doorId);
                List<ObjectId> listOfMinLengthExplodeLine = windowHlp.getMinLengthOfWindowLineSegment(listOfExplodeLineId);

                for (int k = 0; k < listOfMinLengthExplodeLine.Count; k++)
                {
                    ObjectId doorWallThicknessId = listOfMinLengthExplodeLine[k];
                    bool flagOfDoorWallPanelCreate = drawDoorWallPanel_In_DoorThick_With_Corner(doorId, doorWallThicknessId, listOfCornerId_InsctToDoor);
                    if (flagOfDoorWallPanelCreate == false)
                    {
                        var newDoorThickLineId = AEE_Utility.createColonEntityInSamePoint(doorWallThicknessId, false);
                        var newOffsetDoorThickLineId = getOffsetDoorWallThickLineId(newDoorThickLineId, doorId);
                        setDoorWallThickLineId(xDataRegAppName, newDoorThickLineId, newOffsetDoorThickLineId, doorId);
                    }
                }
                AEE_Utility.deleteEntity(listOfExplodeLineId);
            }

            foreach (var doorId in listOfDoorObjId)
            {
                if (!listOfExistDoorId_InsctToCorner_ForCheck.Contains(doorId))
                {
                    var xDataRegAppName = AEE_Utility.GetXDataRegisterAppName(doorId);

                    List<ObjectId> listOfExplodeLineId = AEE_Utility.ExplodeEntity(doorId);
                    List<ObjectId> listOfMinLengthExplodeLine = windowHlp.getMinLengthOfWindowLineSegment(listOfExplodeLineId);
                    for (int k = 0; k < listOfMinLengthExplodeLine.Count; k++)
                    {
                        ObjectId doorWallThicknessId = listOfMinLengthExplodeLine[k];

                        var newDoorThickLineId = AEE_Utility.createColonEntityInSamePoint(doorWallThicknessId, false);
                        var newOffsetDoorThickLineId = getOffsetDoorWallThickLineId(newDoorThickLineId, doorId);
                        setDoorWallThickLineId(xDataRegAppName, newDoorThickLineId, newOffsetDoorThickLineId, doorId);
                    }
                    AEE_Utility.deleteEntity(listOfExplodeLineId);
                }
            }

            //WindowHelper windowHlp = new WindowHelper();

            //for (int i = 0; i < listOfExistDoorId_InsctToCorner_ForCheck.Count; i++)
            //{
            //    ObjectId doorId = listOfExistDoorId_InsctToCorner_ForCheck[i];
            //    var xDataRegAppName = AEE_Utility.GetXDataRegisterAppName(doorId);

            //    List<ObjectId> listOfCornerId_InsctToDoor = listOfListDoorId_With_InsctCornerId[i];
            //    List<ObjectId> listOfExplodeLineId = AEE_Utility.ExplodeEntity(doorId);
            //    List<ObjectId> listOfMinLengthExplodeLine = windowHlp.getMinLengthOfWindowLineSegment(listOfExplodeLineId);

            //    for (int k = 0; k < listOfMinLengthExplodeLine.Count; k++)
            //    {
            //        ObjectId doorWallThicknessId = listOfMinLengthExplodeLine[k];

            //        var newDoorThickLineId = AEE_Utility.createColonEntityInSamePoint(doorWallThicknessId, false);
            //        var newOffsetDoorThickLineId = getOffsetDoorWallThickLineId(newDoorThickLineId, doorId);
            //        setDoorWallThickLineId(xDataRegAppName, newDoorThickLineId, newOffsetDoorThickLineId, doorId);
            //    }
            //    AEE_Utility.deleteEntity(listOfExplodeLineId);
            //}

            //foreach (var doorId in listOfDoorObjId)
            //{
            //    if (!listOfExistDoorId_InsctToCorner_ForCheck.Contains(doorId))
            //    {
            //        var xDataRegAppName = AEE_Utility.GetXDataRegisterAppName(doorId);

            //        List<ObjectId> listOfExplodeLineId = AEE_Utility.ExplodeEntity(doorId);
            //        List<ObjectId> listOfMinLengthExplodeLine = windowHlp.getMinLengthOfWindowLineSegment(listOfExplodeLineId);
            //        for (int k = 0; k < listOfMinLengthExplodeLine.Count; k++)
            //        {
            //            ObjectId doorWallThicknessId = listOfMinLengthExplodeLine[k];

            //            var newDoorThickLineId = AEE_Utility.createColonEntityInSamePoint(doorWallThicknessId, false);
            //            var newOffsetDoorThickLineId = getOffsetDoorWallThickLineId(newDoorThickLineId, doorId);
            //            setDoorWallThickLineId(xDataRegAppName, newDoorThickLineId, newOffsetDoorThickLineId, doorId);
            //        }
            //        AEE_Utility.deleteEntity(listOfExplodeLineId);
            //    }
            //}
        }

        public void setDoorWallThickLineId(string xDataRegAppName, ObjectId doorThickLineId, ObjectId offsetDoorThickLineId, ObjectId doorId)
        {
            int indexOfExistDoorId = listOfDoorIdThickWallPanel_InsctToCorner.IndexOf(doorId);
            AEE_Utility.AttachXData(doorThickLineId, xDataRegAppName, CommonModule.xDataAsciiName);
            AEE_Utility.AttachXData(doorId, xDataRegAppName, CommonModule.xDataAsciiName);

            List<ObjectId> listOfDoorThickLineId = new List<ObjectId>();
            listOfDoorThickLineId.Add(doorThickLineId);

            List<ObjectId> listOfOffsetDoorThickLineId = new List<ObjectId>();
            listOfOffsetDoorThickLineId.Add(offsetDoorThickLineId);

            if (indexOfExistDoorId == -1)
            {
                listOfDoorIdThickWallPanel_InsctToCorner.Add(doorId);
                listOfListOfDoorThickWallPanelId_InsctToCorner.Add(listOfDoorThickLineId);
                listOfListOffsetDoorThickWallPanelId_InsctToCorner.Add(listOfOffsetDoorThickLineId);
            }
            else
            {
                List<ObjectId> listOfPrvsDoorThickLineId = listOfListOfDoorThickWallPanelId_InsctToCorner[indexOfExistDoorId];
                List<ObjectId> listOfPrvsOffsetDoorThickLineId = listOfListOffsetDoorThickWallPanelId_InsctToCorner[indexOfExistDoorId];

                List<ObjectId> listOfNewDoorThickLineId = listOfPrvsDoorThickLineId;
                listOfNewDoorThickLineId.Add(doorThickLineId);

                List<ObjectId> listOfNewOffsetDoorThickLineId = listOfPrvsOffsetDoorThickLineId;
                listOfNewOffsetDoorThickLineId.Add(offsetDoorThickLineId);

                listOfListOfDoorThickWallPanelId_InsctToCorner[listOfListOfDoorThickWallPanelId_InsctToCorner.FindIndex(ind => ind.Equals(listOfPrvsDoorThickLineId))] = listOfNewDoorThickLineId;
                listOfListOffsetDoorThickWallPanelId_InsctToCorner[listOfListOffsetDoorThickWallPanelId_InsctToCorner.FindIndex(ind => ind.Equals(listOfPrvsOffsetDoorThickLineId))] = listOfNewOffsetDoorThickLineId;
            }
        }

        public bool drawDoorWallPanel_In_DoorThick_With_Corner(ObjectId doorId, ObjectId doorWallThicknessId, List<ObjectId> listOfCornerId_InsctToDoor)
        {
            bool flagOfDoorWallPanelCreate = false;
            var xDataRegAppNameOfDoor = AEE_Utility.GetXDataRegisterAppName(doorId);

            var listOfStrtEndPoint = AEE_Utility.getStartEndPointWithAngle_Line(doorWallThicknessId);
            Point3d startPoint = new Point3d(listOfStrtEndPoint[0], listOfStrtEndPoint[1], 0);
            Point3d endPoint = new Point3d(listOfStrtEndPoint[2], listOfStrtEndPoint[3], 0);
            double angleOfLine = listOfStrtEndPoint[4];
            double lengthOfDoorThick = AEE_Utility.GetLengthOfLine(doorWallThicknessId);

            var point = AEE_Utility.get_XY(angleOfLine, (lengthOfDoorThick / 2));

            Point3d newStrtPoint = new Point3d((startPoint.X - point.X), (startPoint.Y - point.Y), 0);
            Point3d newEndPoint = new Point3d((endPoint.X + point.X), (endPoint.Y + point.Y), 0);

            var lineId = AEE_Utility.getLineId(newStrtPoint, newEndPoint, false);

            ObjectId lineId_PositiveDir = AEE_Utility.OffsetLine(lineId, WindowHelper.windowOffsetValue, false);
            ObjectId lineId_NegativeDir = AEE_Utility.OffsetLine(lineId, -WindowHelper.windowOffsetValue, false);

            List<ObjectId> listOfCornerId_InsctToDoorThickLine = new List<ObjectId>();

            ParapetHelper parapetHlp = new ParapetHelper();

            foreach (var cornerId in listOfCornerId_InsctToDoor)
            {
                ObjectId insctLineId = new ObjectId();
                var listOfInsct = AEE_Utility.InterSectionBetweenTwoEntity(lineId, cornerId);
                insctLineId = lineId;
                if (listOfInsct.Count == 0)
                {
                    listOfInsct = AEE_Utility.InterSectionBetweenTwoEntity(lineId_PositiveDir, cornerId);
                    if (listOfInsct.Count == 0)
                    {
                        listOfInsct = AEE_Utility.InterSectionBetweenTwoEntity(lineId_NegativeDir, cornerId);
                    }
                }

                if (listOfInsct.Count != 0)
                {
                    listOfCornerId_InsctToDoorThickLine.Add(cornerId);
                }

                if (listOfCornerId_InsctToDoorThickLine.Count == 2)
                {
                    flagOfDoorWallPanelCreate = true;

                    ObjectId cornerId1 = listOfCornerId_InsctToDoorThickLine[0];
                    var basePointOfCorner1 = AEE_Utility.GetBasePointOfPolyline(cornerId1);
                    bool flagOfDuplic_Corner1 = checkDuplicateCornerId(cornerId1);
                    ObjectId maxLengthVerticeLine_Corner1 = new ObjectId();
                    double maxLengthOfCorner1Vertex = getMaximumLengthOfCornerVertex(angleOfLine, cornerId1, out maxLengthVerticeLine_Corner1);
                    double distanceBtwTwoDoor_Corner1 = 0;

                    ObjectId doorId2_Corner1 = new ObjectId();
                    bool flagOfDoorInscToAnotherDoor_Corner1 = checkDoorIsIntersectToAnotherDoor(cornerId1, basePointOfCorner1, doorId, out distanceBtwTwoDoor_Corner1, out doorId2_Corner1);

                    if (flagOfDuplic_Corner1 == true && flagOfDoorInscToAnotherDoor_Corner1 == false)
                    {
                        flagOfDuplic_Corner1 = false;
                        maxLengthOfCorner1Vertex = distanceBtwTwoDoor_Corner1;
                    }
                    else if (flagOfDuplic_Corner1 == true && flagOfDoorInscToAnotherDoor_Corner1 == true)
                    {
                        var xDataRegAppName = AEE_Utility.GetXDataRegisterAppName(cornerId1);
                        drawCornerBetweenTwoDoorInsctPoint(xDataRegAppName, basePointOfCorner1, doorId, doorId2_Corner1);
                    }

                    ObjectId cornerId2 = listOfCornerId_InsctToDoorThickLine[1];
                    var basePointOfCorner2 = AEE_Utility.GetBasePointOfPolyline(cornerId2);
                    bool flagOfDuplic_Corner2 = checkDuplicateCornerId(cornerId2);
                    ObjectId maxLengthVerticeLine_Corner2 = new ObjectId();
                    double maxLengthOfCorner2Vertex = getMaximumLengthOfCornerVertex(angleOfLine, cornerId2, out maxLengthVerticeLine_Corner2);

                    double distanceBtwTwoDoor_Corner2 = 0;
                    ObjectId doorId2_Corner2 = new ObjectId();
                    bool flagOfDoorInscToAnotherDoor_Corner2 = checkDoorIsIntersectToAnotherDoor(cornerId2, basePointOfCorner2, doorId, out distanceBtwTwoDoor_Corner2, out doorId2_Corner2);

                    if (flagOfDuplic_Corner2 == true && flagOfDoorInscToAnotherDoor_Corner2 == false)
                    {
                        flagOfDuplic_Corner2 = false;
                        maxLengthOfCorner2Vertex = distanceBtwTwoDoor_Corner2;
                    }
                    else if (flagOfDuplic_Corner2 == true && flagOfDoorInscToAnotherDoor_Corner2 == true)
                    {
                        var xDataRegAppName = AEE_Utility.GetXDataRegisterAppName(cornerId2);
                        drawCornerBetweenTwoDoorInsctPoint(xDataRegAppName, basePointOfCorner2, doorId, doorId2_Corner2);
                    }

                    if (ParapetHelper.listOfAllCornersId_InsctToParapet.Contains(cornerId1))
                    {
                        parapetHlp.setWallDataInSameCornerOfParapetOrDoor(doorId, cornerId1, maxLengthOfCorner1Vertex, maxLengthVerticeLine_Corner1);
                        flagOfDuplic_Corner1 = false;
                        maxLengthOfCorner1Vertex = 0;
                    }
                    if (ParapetHelper.listOfAllCornersId_InsctToParapet.Contains(cornerId2))
                    {
                        parapetHlp.setWallDataInSameCornerOfParapetOrDoor(doorId, cornerId2, maxLengthOfCorner2Vertex, maxLengthVerticeLine_Corner2);
                        flagOfDuplic_Corner2 = false;
                        maxLengthOfCorner2Vertex = 0;
                    }

                    checkTwoCornersAreInStrtOrEndPoint(xDataRegAppNameOfDoor, doorId, angleOfLine, startPoint, endPoint, basePointOfCorner1, basePointOfCorner2, maxLengthOfCorner1Vertex, maxLengthOfCorner2Vertex, flagOfDuplic_Corner1, flagOfDuplic_Corner2);
                    break;
                }
            }

            if (listOfCornerId_InsctToDoorThickLine.Count == 1)
            {
                flagOfDoorWallPanelCreate = true;
                ObjectId cornerId1 = listOfCornerId_InsctToDoorThickLine[0];
                var basePointOfCorner1 = AEE_Utility.GetBasePointOfPolyline(cornerId1);
                bool flagOfDuplic_Corner1 = checkDuplicateCornerId(cornerId1);
                ObjectId maxLengthVerticeLine_Corner1 = new ObjectId();
                double maxLengthOfCorner1Vertex = getMaximumLengthOfCornerVertex(angleOfLine, cornerId1, out maxLengthVerticeLine_Corner1);
                if (ParapetHelper.listOfAllCornersId_InsctToParapet.Contains(cornerId1))
                {
                    parapetHlp.setWallDataInSameCornerOfParapetOrDoor(doorId, cornerId1, maxLengthOfCorner1Vertex, maxLengthVerticeLine_Corner1);
                    flagOfDuplic_Corner1 = false;
                    maxLengthOfCorner1Vertex = 0;
                }

                checkOneCornerIsInStrtOrEndPoint(xDataRegAppNameOfDoor, doorId, angleOfLine, startPoint, endPoint, basePointOfCorner1, maxLengthOfCorner1Vertex, flagOfDuplic_Corner1);
            }
            AEE_Utility.deleteEntity(lineId);
            AEE_Utility.deleteEntity(lineId_PositiveDir);
            AEE_Utility.deleteEntity(lineId_NegativeDir);

            return flagOfDoorWallPanelCreate;
        }

        public double getMaximumLengthOfCornerVertex(double angleOfLine, ObjectId cornerId, out ObjectId maxLengthVerticeLine)
        {
            maxLengthVerticeLine = new ObjectId();
            CommonModule commonMod = new CommonModule();

            bool flagOfDoorThick_X_Axis = false;
            bool flagOfDoorThick_Y_Axis = false;

            commonMod.checkAngleOfLine_Axis(angleOfLine, out flagOfDoorThick_X_Axis, out flagOfDoorThick_Y_Axis);

            List<double> listOfMaxLengthOfCornerVertex = new List<double>();
            List<ObjectId> listOfMaxLengthOfCornerLineId = new List<ObjectId>();

            var listOfExplodeId = AEE_Utility.ExplodeEntity(cornerId);
            foreach (var cornerVertexId in listOfExplodeId)
            {
                var angleOfCornerVertex = AEE_Utility.GetAngleOfLine(cornerVertexId);

                bool flagOfCorner_X_Axis = false;
                bool flagOfCorner_Y_Axis = false;

                commonMod.checkAngleOfLine_Axis(angleOfCornerVertex, out flagOfCorner_X_Axis, out flagOfCorner_Y_Axis);
                if (flagOfDoorThick_X_Axis == true && flagOfCorner_X_Axis == true)
                {
                    double length = AEE_Utility.GetLengthOfLine(cornerVertexId);
                    listOfMaxLengthOfCornerVertex.Add(length);
                    listOfMaxLengthOfCornerLineId.Add(cornerVertexId);
                }
                else if (flagOfDoorThick_Y_Axis == true && flagOfCorner_Y_Axis == true)
                {
                    double length = AEE_Utility.GetLengthOfLine(cornerVertexId);
                    listOfMaxLengthOfCornerVertex.Add(length);
                    listOfMaxLengthOfCornerLineId.Add(cornerVertexId);
                }
            }

            double maxLengthOfCornerVertex = 0;
            if (listOfMaxLengthOfCornerVertex.Count != 0)
            {
                maxLengthOfCornerVertex = listOfMaxLengthOfCornerVertex.Max();
                int indexOfMaxLngth = listOfMaxLengthOfCornerVertex.IndexOf(maxLengthOfCornerVertex);
                maxLengthVerticeLine = AEE_Utility.createColonEntityInSamePoint(listOfMaxLengthOfCornerLineId[indexOfMaxLngth], false);
            }

            AEE_Utility.deleteEntity(listOfExplodeId);

            return maxLengthOfCornerVertex;
        }
        private void checkTwoCornersAreInStrtOrEndPoint(string xDataRegAppNameOfDoor, ObjectId doorId, double angleOfLine, Point3d startPoint, Point3d endPoint, Point3d basePointOfCorner1, Point3d basePointOfCorner2, double maxLengthOfCorner1Vertex, double maxLengthOfCorner2Vertex, bool flagOfDuplic_Corner1, bool flagOfDuplic_Corner2)
        {
            ObjectId doorCornerLineId = new ObjectId();
            ObjectId offsetDoorCornerLineId = new ObjectId();

            double lengthOfStrtPoint_To_basePointOfCorner1 = AEE_Utility.GetLengthOfLine(startPoint.X, startPoint.Y, basePointOfCorner1.X, basePointOfCorner1.Y);
            double lengthOfStrtPoint_To_basePointOfCorner2 = AEE_Utility.GetLengthOfLine(startPoint.X, startPoint.Y, basePointOfCorner2.X, basePointOfCorner2.Y);

            Point3d newStrtPoint = new Point3d();
            if (lengthOfStrtPoint_To_basePointOfCorner1 > lengthOfStrtPoint_To_basePointOfCorner2)
            {
                var point = AEE_Utility.get_XY(angleOfLine, maxLengthOfCorner2Vertex);
                newStrtPoint = new Point3d((startPoint.X - point.X), (startPoint.Y - point.Y), 0);
                if (flagOfDuplic_Corner2 == true)
                {
                    newStrtPoint = startPoint;
                }
            }
            else
            {
                var point = AEE_Utility.get_XY(angleOfLine, maxLengthOfCorner1Vertex);
                newStrtPoint = new Point3d((startPoint.X - point.X), (startPoint.Y - point.Y), 0);
                if (flagOfDuplic_Corner1 == true)
                {
                    newStrtPoint = startPoint;
                }
            }

            double lengthOfEndPoint_To_basePointOfCorner1 = AEE_Utility.GetLengthOfLine(endPoint.X, endPoint.Y, basePointOfCorner1.X, basePointOfCorner1.Y);
            double lengthOfEndPoint_To_basePointOfCorner2 = AEE_Utility.GetLengthOfLine(endPoint.X, endPoint.Y, basePointOfCorner2.X, basePointOfCorner2.Y);
            Point3d newEndPoint = new Point3d();
            if (lengthOfEndPoint_To_basePointOfCorner1 > lengthOfEndPoint_To_basePointOfCorner2)
            {
                var point = AEE_Utility.get_XY(angleOfLine, maxLengthOfCorner2Vertex);
                newEndPoint = new Point3d((endPoint.X + point.X), (endPoint.Y + point.Y), 0);
                if (flagOfDuplic_Corner2 == true)
                {
                    newEndPoint = endPoint;
                }
            }
            else
            {
                var point = AEE_Utility.get_XY(angleOfLine, maxLengthOfCorner1Vertex);
                newEndPoint = new Point3d((endPoint.X + point.X), (endPoint.Y + point.Y), 0);
                if (flagOfDuplic_Corner1 == true)
                {
                    newEndPoint = endPoint;
                }
            }
            var doorEnt = AEE_Utility.GetEntityForRead(doorId);
            doorCornerLineId = AEE_Utility.getLineId(doorEnt, newStrtPoint, newEndPoint, false);
            double doorCornerLineLngth = AEE_Utility.GetLengthOfLine(doorCornerLineId);
            if (doorCornerLineLngth >= 1)
            {
                getOffsetDoorCornerLine(doorId, doorCornerLineId, angleOfLine, newStrtPoint, newEndPoint, out offsetDoorCornerLineId);
                setDoorWallThickLineId(xDataRegAppNameOfDoor, doorCornerLineId, offsetDoorCornerLineId, doorId);
            }
        }
        private void getOffsetDoorCornerLine(ObjectId doorId, ObjectId doorCornerLineId, double angleOfLine, Point3d startPoint, Point3d endPoint, out ObjectId offsetDoorCornerLineId)
        {
            offsetDoorCornerLineId = new ObjectId();

            var point = AEE_Utility.get_XY(angleOfLine, 100);
            var lineId_ForCheck = AEE_Utility.getLineId((startPoint.X - point.X), (startPoint.Y - point.Y), 0, (endPoint.X + point.X), (endPoint.Y + point.Y), 0, false);

            var offsetId_Postive_Dir = AEE_Utility.OffsetLine(lineId_ForCheck, CommonModule.panelDepth, false);
            var offsetId_Negative_Dir = AEE_Utility.OffsetLine(lineId_ForCheck, -CommonModule.panelDepth, false);

            var listOfInsct = AEE_Utility.InterSectionBetweenTwoEntity(doorId, offsetId_Postive_Dir);
            if (listOfInsct.Count != 0)
            {
                offsetDoorCornerLineId = AEE_Utility.OffsetLine(doorCornerLineId, CommonModule.panelDepth, false);
            }
            else
            {
                offsetDoorCornerLineId = AEE_Utility.OffsetLine(doorCornerLineId, -CommonModule.panelDepth, false);
            }
            AEE_Utility.deleteEntity(lineId_ForCheck);
            AEE_Utility.deleteEntity(offsetId_Postive_Dir);
            AEE_Utility.deleteEntity(offsetId_Negative_Dir);
        }


        private void checkOneCornerIsInStrtOrEndPoint(string xDataRegAppNameOfDoor, ObjectId doorId, double angleOfLine, Point3d startPoint, Point3d endPoint, Point3d basePointOfCorner1, double maxLengthOfCorner1Vertex, bool flagOfDuplic_Corner1)
        {
            ObjectId doorCornerLineId = new ObjectId();
            ObjectId offsetDoorCornerLineId = new ObjectId();

            double lengthOfStrtPoint_To_basePointOfCorner1 = AEE_Utility.GetLengthOfLine(startPoint.X, startPoint.Y, basePointOfCorner1.X, basePointOfCorner1.Y);
            double lengthOfEndPoint_To_basePointOfCorner1 = AEE_Utility.GetLengthOfLine(endPoint.X, endPoint.Y, basePointOfCorner1.X, basePointOfCorner1.Y);

            Point3d newStrtPoint = new Point3d();
            Point3d newEndPoint = new Point3d();
            var line = new LineSegment3d();
            if (lengthOfStrtPoint_To_basePointOfCorner1 > lengthOfEndPoint_To_basePointOfCorner1)
            {
                var point = AEE_Utility.get_XY(angleOfLine, maxLengthOfCorner1Vertex);
                newEndPoint = new Point3d((endPoint.X + point.X), (endPoint.Y + point.Y), 0);
                line = new LineSegment3d(startPoint + (startPoint - endPoint).GetNormal().MultiplyBy(CommonModule.externalCorner), newEndPoint);
                //newStrtPoint = startPoint + (startPoint - endPoint).GetNormal().MultiplyBy(CommonModule.externalCorner);
                if (flagOfDuplic_Corner1 == true)
                {
                    newEndPoint = endPoint;
                    line = new LineSegment3d(startPoint, endPoint);
                }
                newStrtPoint = startPoint;
            }
            else
            {
                var point = AEE_Utility.get_XY(angleOfLine, maxLengthOfCorner1Vertex);
                newStrtPoint = new Point3d((startPoint.X - point.X), (startPoint.Y - point.Y), 0);
                line = new LineSegment3d(newStrtPoint, endPoint + (endPoint - startPoint).GetNormal().MultiplyBy(CommonModule.externalCorner));
                if (flagOfDuplic_Corner1 == true)
                {
                    newStrtPoint = startPoint;
                    line = new LineSegment3d(startPoint, endPoint);
                }
                newEndPoint = endPoint;
            }

            var doorEnt = AEE_Utility.GetEntityForRead(doorId);
            doorCornerLineId = AEE_Utility.getLineId(doorEnt, newStrtPoint, newEndPoint, false);
            double doorCornerLineLngth = AEE_Utility.GetLengthOfLine(doorCornerLineId);
            if (doorCornerLineLngth >= 1)
            {
                getOffsetDoorCornerLine(doorId, doorCornerLineId, angleOfLine, newStrtPoint, newEndPoint, out offsetDoorCornerLineId);
                if (dicDoorToEACorners.ContainsKey(doorId))
                {
                    if (!dicDoorToEALines.ContainsKey(doorId))
                    {
                        dicDoorToEALines[doorId] = new List<Tuple<LineSegment3d, LineSegment3d>>();
                    }
                    dicDoorToEALines[doorId].Add(new Tuple<LineSegment3d, LineSegment3d>(new LineSegment3d(newStrtPoint, newEndPoint), line));
                }
                setDoorWallThickLineId(xDataRegAppNameOfDoor, doorCornerLineId, offsetDoorCornerLineId, doorId);
            }
        }

        private bool checkDuplicateCornerId(ObjectId inputCornerId)
        {
            int count = 0;
            foreach (var crnerId in listOfAllCornerId_InsctDoor_ForCheck)
            {
                if (crnerId == inputCornerId)
                {
                    count++;
                }
                if (count == 2)
                {
                    break;
                }
            }
            if (count == 2)
            {
                return true;
            }
            return false;
        }

        private bool checkDoorIsIntersectToAnotherDoor(ObjectId cornerId, Point3d basePointOfCorner, ObjectId doorId1, out double distanceBtwTwoDoor, out ObjectId doorId2)
        {
            doorId2 = new ObjectId();
            distanceBtwTwoDoor = 0;
            bool flagOfDoorInscToAnotherDoor = false;
            var index = listOfExistCornerId_ForCheck.IndexOf(cornerId);
            if (index != -1)
            {
                var listOfDoorId_InsctToCorner = listOfListCornerId_With_InsctDoorId[index];
                if (listOfDoorId_InsctToCorner.Count == 2)
                {
                    foreach (var doorId in listOfDoorId_InsctToCorner)
                    {
                        if (doorId != doorId1)
                        {
                            doorId2 = doorId;
                            var listOfDoor1_Vertex = AEE_Utility.GetPolylineVertexPoint(doorId1);
                            double door1_MaxX = listOfDoor1_Vertex.Max(sortPoint => sortPoint.X);
                            double door1_MaxY = listOfDoor1_Vertex.Max(sortPoint => sortPoint.Y);
                            Point3d door1_OffsetPoint = new Point3d((door1_MaxX + 1), (door1_MaxY + 1), 0);
                            var offsetDoorId1 = AEE_Utility.OffsetEntity_WithoutLine(1, door1_OffsetPoint, doorId1, false);

                            var listOfDoor2_Vertex = AEE_Utility.GetPolylineVertexPoint(doorId2);
                            double door2_MaxX = listOfDoor2_Vertex.Max(sortPoint => sortPoint.X);
                            double door2_MaxY = listOfDoor2_Vertex.Max(sortPoint => sortPoint.Y);
                            Point3d door2_OffsetPoint = new Point3d((door2_MaxX + 1), (door2_MaxY + 1), 0);
                            var offsetDoorId2 = AEE_Utility.OffsetEntity_WithoutLine(1, door2_OffsetPoint, doorId2, false);

                            var listOfIntersect = AEE_Utility.InterSectionBetweenTwoEntity(offsetDoorId1, offsetDoorId2);
                            if (listOfIntersect.Count == 0)
                            {
                                Point2d nearestDoorVertexPoint = new Point2d();
                                flagOfDoorInscToAnotherDoor = false;
                                distanceBtwTwoDoor = getDistanceBetweenDoorToCorner(listOfDoor2_Vertex, basePointOfCorner, out nearestDoorVertexPoint);
                            }
                            else
                            {
                                flagOfDoorInscToAnotherDoor = true;
                            }
                            break;
                        }
                    }
                }
            }
            return flagOfDoorInscToAnotherDoor;
        }

        private double getDistanceBetweenDoorToCorner(List<Point2d> listOfDoorVertex, Point3d basePointOfCorner, out Point2d nearestDoorVertexPoint)
        {
            nearestDoorVertexPoint = new Point2d();
            double distanceBtwTwoDoor = 0;

            List<double> listOfLength = new List<double>();
            for (int index = 0; index < listOfDoorVertex.Count; index++)
            {
                var vertexPoint = listOfDoorVertex[index];
                var length = AEE_Utility.GetLengthOfLine(vertexPoint.X, vertexPoint.Y, basePointOfCorner.X, basePointOfCorner.Y);
                listOfLength.Add(length);
            }
            if (listOfLength.Count != 0)
            {
                distanceBtwTwoDoor = listOfLength.Min();
                int indexOfMin = listOfLength.IndexOf(distanceBtwTwoDoor);
                nearestDoorVertexPoint = listOfDoorVertex[indexOfMin];
            }
            return distanceBtwTwoDoor;
        }
        private void drawCornerBetweenTwoDoorInsctPoint(string xDataRegAppName, Point3d basePoint, ObjectId doorId1, ObjectId doorId2)
        {
            string doorText1 = Convert.ToString(doorId1) + "," + Convert.ToString(doorId2);
            string doorText2 = Convert.ToString(doorId2) + "," + Convert.ToString(doorId1);
            if (listOfDoorIds_CreateCornerBtwTwoDoor.Contains(doorText1) || listOfDoorIds_CreateCornerBtwTwoDoor.Contains(doorText2))
            {

            }
            else
            {
                PartHelper partHelper = new PartHelper();

                ObjectId door1WallLineId = getNearestDoorLineIdOfCorner(basePoint, doorId1);
                ObjectId door2WallLineId = getNearestDoorLineIdOfCorner(basePoint, doorId2);

                var door1WallLineAngle = AEE_Utility.GetAngleOfLine(door1WallLineId);
                var door2WallLineAngle = AEE_Utility.GetAngleOfLine(door2WallLineId);

                List<double> listOfAngle = new List<double>();
                listOfAngle.Add(door1WallLineAngle);
                listOfAngle.Add(door1WallLineAngle + CommonModule.angle_90);
                listOfAngle.Add(door1WallLineAngle + CommonModule.angle_180);
                listOfAngle.Add(door1WallLineAngle + CommonModule.angle_270);
                for (int k = 0; k < listOfAngle.Count; k++)
                {
                    var externalCornerId = partHelper.drawExternalCorner(xDataRegAppName, basePoint.X, basePoint.Y, CommonModule.panelDepth, CommonModule.externalCornerThick, CommonModule.externalCornerLyrName, CommonModule.externalCornerLyrColor);
                    AEE_Utility.AttachXData(externalCornerId, xDataRegAppName, CommonModule.xDataAsciiName);
                    AEE_Utility.AttachXData(externalCornerId, xDataRegAppName + "_1", CommonModule.panelDepth.ToString() + "," +
                                                                                      CommonModule.externalCornerThick.ToString());
                    double rotationAngle = listOfAngle[k];

                    CommonModule.rotateAngle = (rotationAngle * Math.PI) / 180; // variable assign for rotation             
                    CommonModule.basePoint = new Point3d(basePoint.X, basePoint.Y, 0);
                    List<ObjectId> listOfObjId = new List<ObjectId>();
                    listOfObjId.Add(externalCornerId);
                    MoveRorationScaleJig.RotationScaleJig_Method(listOfObjId);
                    var listOfInsctWithOffsetWall = AEE_Utility.InterSectionBetweenTwoEntity(externalCornerId, door1WallLineId);

                    bool flagOfCornerCreation = false;

                    if (listOfInsctWithOffsetWall.Count == 0 || listOfInsctWithOffsetWall.Count <= 1)
                    {
                        AEE_Utility.deleteEntity(externalCornerId);
                    }
                    else
                    {
                        listOfInsctWithOffsetWall = AEE_Utility.InterSectionBetweenTwoEntity(externalCornerId, door2WallLineId);
                        if (listOfInsctWithOffsetWall.Count == 0 || listOfInsctWithOffsetWall.Count <= 1)
                        {
                            AEE_Utility.deleteEntity(externalCornerId);
                        }
                        else
                        {
                            var door1_Ent = AEE_Utility.GetEntityForRead(doorId1);
                            var door2_Ent = AEE_Utility.GetEntityForRead(doorId2);

                            double door1_LintelHght = Door_UI_Helper.getDoorLintelLevel(door1_Ent.Layer);
                            double door2_LintelHght = Door_UI_Helper.getDoorLintelLevel(door2_Ent.Layer);

                            double maxLintelHght = Math.Max(door1_LintelHght, door2_LintelHght);

                            string cornerText = CornerHelper.getCornerText(CommonModule.externalCornerText, CommonModule.extrnlCornr_Flange, CommonModule.extrnlCornr_Flange, maxLintelHght);
                            double crnerRotationAngle = rotationAngle + CommonModule.cornerRotationAngle;
                            CornerHelper cornerHlp = new CornerHelper();
                            var externalCornerTextId = cornerHlp.writeTextInCorner(externalCornerId, cornerText, crnerRotationAngle, CommonModule.externalCornerTextLyrName, CommonModule.externalCornerLyrColor);

                            CornerHelper.setCornerDataForBOM(xDataRegAppName, externalCornerId, externalCornerTextId, cornerText, CommonModule.externalCornerDescp, CommonModule.externalCornerText, CommonModule.extrnlCornr_Flange, CommonModule.extrnlCornr_Flange, maxLintelHght/*, 0.0/*elev*/);

                            flagOfCornerCreation = true;
                            break;
                        }
                    }

                    if (flagOfCornerCreation == false)
                    {
                        AEE_Utility.deleteEntity(externalCornerId);
                    }
                }

                listOfDoorIds_CreateCornerBtwTwoDoor.Add(doorText1);
                listOfDoorIds_CreateCornerBtwTwoDoor.Add(doorText2);
            }
        }

        private ObjectId getNearestDoorLineIdOfCorner(Point3d basepoint, ObjectId doorId)
        {
            ObjectId doorWallLineId = new ObjectId();

            List<double> listOfMinLengthBtwCornerToDoor = new List<double>();
            List<ObjectId> listOfDoorWallId = new List<ObjectId>();

            WindowHelper windowHlp = new WindowHelper();
            var listOfExplodeId = AEE_Utility.ExplodeEntity(doorId);
            List<ObjectId> listOfMaxLengthExplodeLine = windowHlp.getMaxLengthOfWindowLineSegment(listOfExplodeId);

            bool flagOfMinLngth = false;
            foreach (var explodeLineId in listOfMaxLengthExplodeLine)
            {
                var listOfStrtEndPoint = AEE_Utility.GetStartEndPointOfLine(explodeLineId);
                foreach (var point in listOfStrtEndPoint)
                {
                    var length = AEE_Utility.GetLengthOfLine(point.X, point.Y, basepoint.X, basepoint.Y);
                    listOfMinLengthBtwCornerToDoor.Add(length);
                    listOfDoorWallId.Add(explodeLineId);
                    if (Math.Truncate(length) == 0)
                    {
                        doorWallLineId = explodeLineId;
                        flagOfMinLngth = true;
                        break;
                    }
                }
                if (flagOfMinLngth == true)
                {
                    break;
                }
            }

            if (flagOfMinLngth == false)
            {
                double minLngth = listOfMinLengthBtwCornerToDoor.Min();
                int indexOfMin = listOfMinLengthBtwCornerToDoor.IndexOf(minLngth);
                doorWallLineId = listOfDoorWallId[indexOfMin];
            }
            doorWallLineId = AEE_Utility.createColonEntityInSamePoint(doorWallLineId, false);

            AEE_Utility.deleteEntity(listOfExplodeId);

            return doorWallLineId;
        }

        public ObjectId getOffsetDoorWallThickLineId(ObjectId doorLineId, ObjectId doorId)
        {
            ObjectId offsetLineId = AEE_Utility.OffsetLine(doorLineId, CommonModule.panelDepth, false);
            var midPoint = AEE_Utility.GetMidPoint(offsetLineId);
            var flagOfInside = AEE_Utility.GetPointIsInsideThePolyline(doorId, midPoint);
            if (flagOfInside == false)
            {
                AEE_Utility.deleteEntity(offsetLineId);
                offsetLineId = AEE_Utility.OffsetLine(doorLineId, -CommonModule.panelDepth, false);
            }
            return offsetLineId;
        }

        public void drawDoorPanels(ProgressForm progressForm, string wallCreationMsg, Dictionary<Point3d, Tuple<double, bool>> dicCornerPoints)
        {
            List<List<ObjectId>> listOfListDoorPanelLines_ObjId = new List<List<ObjectId>>();
            List<List<ObjectId>> listOfListDoorPanelOffsetLines_ObjId = new List<List<ObjectId>>();
            List<ObjectId> listOfDoorObjId_InsctWall = new List<ObjectId>();
            List<ObjectId> listOfParapetId_WithDoorWallLine = new List<ObjectId>();

            getDoorPanelsLines(out listOfListDoorPanelLines_ObjId, out listOfListDoorPanelOffsetLines_ObjId, out listOfDoorObjId_InsctWall, out listOfParapetId_WithDoorWallLine);
            if (listOfListDoorPanelLines_ObjId.Count != 0)
            {
                for (int i = 0; i < listOfListDoorPanelLines_ObjId.Count; i++)
                {
                    List<ObjectId> listOfDoorPanelLine_Id = listOfListDoorPanelLines_ObjId[i];
                    ObjectId doorId = listOfDoorObjId_InsctWall[i];
                    var doorEnt = AEE_Utility.GetEntityForRead(doorId);
                    string doorLayer = doorEnt.Layer;

                    List<ObjectId> listOfOffsetDoorPanelLine_Id = listOfListDoorPanelOffsetLines_ObjId[i];
                    ObjectId parapetId = listOfParapetId_WithDoorWallLine[i];

                    List<string> listOfBeamLayerName_DoorWallInsct = new List<string>();
                    List<ObjectId> listOfDoor_BeamInsctId = new List<ObjectId>();
                    List<double> listOfDistBtwDoorToBeam = new List<double>();
                    List<ObjectId> listOfNrstBtwDoorToBeamWallLineId = new List<ObjectId>();
                    List<bool> listOfDoor_SunkanSlabWallLineflag = new List<bool>();
                    List<ObjectId> listOfDoor_SunkanSlabId = new List<ObjectId>();

                    List<List<ObjectId>> listOfListOfDoorWallNewLineId = getDoorOrWindowWallLinesId(doorId, listOfDoorPanelLine_Id, listOfDoorPanelLine_ObjId, out listOfBeamLayerName_DoorWallInsct, out listOfDoor_BeamInsctId, out listOfDistBtwDoorToBeam, out listOfNrstBtwDoorToBeamWallLineId, out listOfDoor_SunkanSlabWallLineflag, out listOfDoor_SunkanSlabId);

                    var isHorzLayout = PanelLayout_UI.flagOfHorzPanel_ForDoorWindowBeam;
                    for (int j = 0; j < listOfListOfDoorWallNewLineId.Count; j++)
                    {
                        List<ObjectId> listOfDoorWallNewLineId = listOfListOfDoorWallNewLineId[j];

                        string beamLayerName_InsctDoorWall = listOfBeamLayerName_DoorWallInsct[j];
                        ObjectId beamId = listOfDoor_BeamInsctId[j];
                        double distanceBtwDoorToBeam = listOfDistBtwDoorToBeam[j];
                        ObjectId nrstBtwDoorToBeamWallLineId = listOfNrstBtwDoorToBeamWallLineId[j];
                        bool flagOfSunkanSlab = listOfDoor_SunkanSlabWallLineflag[j];
                        ObjectId sunkanSlabId = listOfDoor_SunkanSlabId[j];

                        if (isHorzLayout)
                        {
                            for (int k = 0; k < listOfDoorWallNewLineId.Count; k = (k + 2))
                            {
                                var doorPanelLine_Id = listOfDoorWallNewLineId[k];
                                var offsetDoorPanelLine_Id = listOfDoorWallNewLineId[k + 1];

                                double doorWall_X_Height = 0;
                                getDoorWallText(doorLayer, beamLayerName_InsctDoorWall, doorPanelLine_Id, sunkanSlabId, out doorWall_X_Height);
                                if (doorWall_X_Height > PanelLayout_UI.maxValueToConvertHorzToVert)
                                {
                                    isHorzLayout = false;
                                    break;
                                }
                            }
                        }
                    }

                    for (int j = 0; j < listOfListOfDoorWallNewLineId.Count; j++)
                    {
                        List<ObjectId> listOfDoorWallNewLineId = listOfListOfDoorWallNewLineId[j];

                        string beamLayerName_InsctDoorWall = listOfBeamLayerName_DoorWallInsct[j];
                        ObjectId beamId = listOfDoor_BeamInsctId[j];
                        double distanceBtwDoorToBeam = listOfDistBtwDoorToBeam[j];
                        ObjectId nrstBtwDoorToBeamWallLineId = listOfNrstBtwDoorToBeamWallLineId[j];
                        bool flagOfSunkanSlab = listOfDoor_SunkanSlabWallLineflag[j];
                        ObjectId sunkanSlabId = listOfDoor_SunkanSlabId[j];

                        for (int k = 0; k < listOfDoorWallNewLineId.Count; k = (k + 2))
                        {
                            var doorPanelLine_Id = listOfDoorWallNewLineId[k];
                            var offsetDoorPanelLine_Id = listOfDoorWallNewLineId[k + 1];

                            double doorWall_X_Height = 0;
                            getDoorWallText(doorLayer, beamLayerName_InsctDoorWall, doorPanelLine_Id, sunkanSlabId, out doorWall_X_Height);

                            ////Fix EX elevation by SDM 2022-08-04
                            var xDataRegAppName = AEE_Utility.GetXDataRegisterAppName(doorPanelLine_Id);
                            if (xDataRegAppName.Contains("_EX_"))
                            //doorWall_X_Height += PanelLayout_UI.RC;
                            {
                                flagOfSunkanSlab = SunkanSlabHelper.listOfWallName_SunkanSlabId.ContainsKey(xDataRegAppName);
                                sunkanSlabId = flagOfSunkanSlab ? SunkanSlabHelper.listOfWallName_SunkanSlabId[xDataRegAppName] : new ObjectId();
                            }

                            List<ObjectId> listOfWallPanelRect_Id = new List<ObjectId>();
                            List<ObjectId> listOfTextId = new List<ObjectId>();
                            List<ObjectId> listOfWallXTextId = new List<ObjectId>();
                            drawDoorPanels(beamLayerName_InsctDoorWall, beamId, distanceBtwDoorToBeam, nrstBtwDoorToBeamWallLineId, doorPanelLine_Id, offsetDoorPanelLine_Id, doorWall_X_Height, out listOfWallPanelRect_Id, out listOfTextId, out listOfWallXTextId, flagOfSunkanSlab, sunkanSlabId, parapetId, isHorzLayout, doorLayer);//RTJ 10-06-2021
                        }
                    }
                }
            }

            drawWallPanels_In_DoorThick(dicCornerPoints);

            eraseOverlapCorner(listOverlapExternals);
        }

        public void groupCreateInDoor(List<List<ObjectId>> listOfListWallPanelRect_Id, List<List<ObjectId>> listOfListTextId, List<List<ObjectId>> listOfListWallXTextId)
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

        private void drawDoorPanels(string beamLayerName, ObjectId beamId, double distanceBtwWallToBeam, ObjectId nearestBeamWallLineId, ObjectId doorPanelLine_ObjId,
            ObjectId doorPanelOffsetLine_ObjId, double doorWall_X_Height, out List<ObjectId> listOfWallPanelRect_Id, out List<ObjectId> listOfTextId,
            out List<ObjectId> listOfWallXTextId, bool flagOfSunkanSlab, ObjectId sunkanSlabId, ObjectId parapetId, bool isHorzLayout, string doorLayer)//RTJ 10-06-2021
        {
            listOfWallPanelRect_Id = new List<ObjectId>();
            listOfTextId = new List<ObjectId>();
            listOfWallXTextId = new List<ObjectId>();

            CommonModule commonModule = new CommonModule();
            WallPanelHelper wallPanelHlpr = new WallPanelHelper();

            var xDataRegAppName = AEE_Utility.GetXDataRegisterAppName(doorPanelLine_ObjId);

            var doorPanelLine = AEE_Utility.GetLine(doorPanelLine_ObjId);
            var listOfDoorPanelLineStrtEndPoint = commonModule.getStartEndPointOfLine(doorPanelLine);
            Point3d doorPanelLineStrtPoint = listOfDoorPanelLineStrtEndPoint[0];
            Point3d doorPanelLineEndPoint = listOfDoorPanelLineStrtEndPoint[1];

            var doorPanelOffsetLine = AEE_Utility.GetLine(doorPanelOffsetLine_ObjId);
            var listOfDoorPanelOffsetLineStrtEndPoint = commonModule.getStartEndPointOfLine(doorPanelOffsetLine);
            Point3d doorPanelOffstLineStrtPoint = listOfDoorPanelOffsetLineStrtEndPoint[0];
            Point3d doorPanelOffstLineEndPoint = listOfDoorPanelOffsetLineStrtEndPoint[1];

            double angleOfLine = AEE_Utility.GetAngleOfLine(doorPanelLineStrtPoint.X, doorPanelLineStrtPoint.Y, doorPanelLineEndPoint.X, doorPanelLineEndPoint.Y);
            bool flag_X_Axis = false;
            bool flag_Y_Axis = false;

            commonModule.checkAngleOfLine_Axis(angleOfLine, out flag_X_Axis, out flag_Y_Axis);
            if (isHorzLayout == true)
            {
                List<Point3d> listOfLineAndOffsetLinePoint = new List<Point3d>();
                listOfLineAndOffsetLinePoint.Add(doorPanelLineStrtPoint);
                listOfLineAndOffsetLinePoint.Add(doorPanelLineEndPoint);
                listOfLineAndOffsetLinePoint.Add(doorPanelOffstLineStrtPoint);
                listOfLineAndOffsetLinePoint.Add(doorPanelOffstLineEndPoint);

                string doorWindowLayerName = doorPanelLine.Layer;
                double beamBottom = Beam_UI_Helper.getOffsetBeamBottom(beamLayerName);
                bool flagOfTopPanel = false;
                var standardWallHeight = GeometryHelper.getHeightOfWall(doorPanelLine.Layer, PanelLayout_UI.maxHeightOfPanel, InternalWallSlab_UI_Helper.getSlabBottom(doorPanelLine.Layer), beamBottom, PanelLayout_UI.SL, PanelLayout_UI.RC, PanelLayout_UI.getSlabThickness(doorPanelLine.Layer), PanelLayout_UI.kickerHt, PanelLayout_UI.floorHeight, CommonModule.internalCorner, sunkanSlabId, out flagOfTopPanel);

                // flagOfSunkanSlab = false; // door wall panel does not create in sunkan slab//Commented by SDM 2022-08-18
                //Added to fix BS door top panel ht by SDM 2022-08-18
                if (sunkanSlabId.IsValid)
                    doorWall_X_Height -= SunkanSlabHelper.getSunkanSlabLevelDifference(sunkanSlabId);

                WindowHelper windowHlp = new WindowHelper();
                string windowLayer = ""; //RTJ 10-06-2021
                //windowHlp.drawHorzPanel_ForWindowAndDoor(CommonModule.doorWallPanelType, xDataRegAppName, standardWallHeight, listOfLineAndOffsetLinePoint, doorWall_X_Height, PanelLayout_UI.doorTopPanelName, Door_UI_Helper.doorTextLayerName, Door_UI_Helper.doorLayerColor, flag_X_Axis, flag_Y_Axis, beamLayerName, beamId, distanceBtwWallToBeam, nearestBeamWallLineId, doorPanelLine_ObjId, flagOfSunkanSlab, sunkanSlabId, windowLayer); //RTJ 10-06-2021
                windowHlp.drawHorzPanel_ForWindowAndDoor(CommonModule.doorWallPanelType, xDataRegAppName, standardWallHeight, listOfLineAndOffsetLinePoint, doorWall_X_Height, PanelLayout_UI.beamPanelName, Door_UI_Helper.doorTextLayerName, Door_UI_Helper.doorLayerColor, flag_X_Axis, flag_Y_Axis, beamLayerName, beamId, distanceBtwWallToBeam, nearestBeamWallLineId, doorPanelLine_ObjId, flagOfSunkanSlab, sunkanSlabId, windowLayer); //RTJ 16-06-2021
            }
            else
            {
                List<ObjectId> listOfWallPanelLine_ObjId = new List<ObjectId>();

                double lengthOfLine = AEE_Utility.GetLengthOfLine(doorPanelLineStrtPoint.X, doorPanelLineStrtPoint.Y, doorPanelLineEndPoint.X, doorPanelLineEndPoint.Y);
                List<double> listOfWallPanelLength = wallPanelHlpr.getListOfWallPanelLength(lengthOfLine, PanelLayout_UI.standardPanelWidth, PanelLayout_UI.minWidthOfPanel);
                listOfWallPanelRect_Id = wallPanelHlpr.drawWallPanels(xDataRegAppName, doorPanelLine_ObjId, doorPanelLineStrtPoint, doorPanelLineEndPoint, doorPanelOffstLineStrtPoint, doorPanelOffstLineEndPoint, flag_X_Axis, flag_Y_Axis, listOfWallPanelLength, out listOfWallPanelLine_ObjId);
                List<ObjectId> listOfCircleId = new List<ObjectId>();
                string windowLayer = ""; //RTJ 10-06-2021
                //Fix the TopPanel height when there is a sunkan by SDM 04-07-2022
                double elev = 0;
                var xdataRegName = AEE_Utility.GetXDataRegisterAppName(doorPanelLine_ObjId);
                if (sunkanSlabId.IsValid)
                {
                    doorWall_X_Height -= SunkanSlabHelper.getSunkanSlabLevelDifference(sunkanSlabId);
                    elev = -PanelLayout_UI.RC;
                }
                if (xdataRegName.Contains("_LF_") || xdataRegName.Contains("_DU_"))
                    elev = -PanelLayout_UI.RC;

                wallPanelHlpr.writeTextInWallPanel(CommonModule.doorWallPanelType, sunkanSlabId, doorPanelLine.Layer, listOfWallPanelRect_Id, listOfWallPanelLine_ObjId, 0, "", doorWall_X_Height, PanelLayout_UI.doorTopPanelName, PanelLayout_UI.RC, CommonModule.wallPanelTextLyrName, CommonModule.wallPanelLyrColor, true, out listOfTextId, out listOfWallXTextId, out listOfCircleId, doorLayer, windowLayer, ele: elev);//RTJ 10-06-2021


                BeamHelper beamHlp = new BeamHelper();
                beamHlp.drawBeamWallPanel(sunkanSlabId, beamLayerName, beamId, distanceBtwWallToBeam, nearestBeamWallLineId, listOfWallPanelRect_Id, listOfWallPanelLine_ObjId, doorPanelLine_ObjId);

                //////// sunkan slab wall panel
                SunkanSlabHelper internalSunkanSlabHlp = new SunkanSlabHelper();
                internalSunkanSlabHlp.drawSunkanSlabWallPanel(flagOfSunkanSlab, sunkanSlabId, listOfWallPanelRect_Id, listOfWallPanelLine_ObjId, doorPanelLine_ObjId);

                if (doorWall_X_Height <= 0 || parapetId.IsValid == true)
                {
                    // Move entity in new shell plan
                    AEE_Utility.deleteEntity(listOfWallPanelRect_Id);
                    AEE_Utility.deleteEntity(listOfTextId);
                    AEE_Utility.deleteEntity(listOfWallXTextId);
                    AEE_Utility.deleteEntity(listOfCircleId);
                }
                else
                {
                    // Move entity in new shell plan
                    AEE_Utility.MoveEntity(listOfWallPanelRect_Id, CreateShellPlanHelper.moveVector_ForWindowDoorLayout);
                    AEE_Utility.MoveEntity(listOfTextId, CreateShellPlanHelper.moveVector_ForWindowDoorLayout);
                    AEE_Utility.MoveEntity(listOfWallXTextId, CreateShellPlanHelper.moveVector_ForWindowDoorLayout);
                    AEE_Utility.MoveEntity(listOfCircleId, CreateShellPlanHelper.moveVector_ForWindowDoorLayout);
                }
            }
        }

        public static void getDoorPanelsLines(out List<List<ObjectId>> listOfListDoorPanelLines_ObjId, out List<List<ObjectId>> listOfListDoorPanelOffsetLines_ObjId, out List<ObjectId> listOfDoorObjId_InsctWall, out List<ObjectId> listOfParapetId_WithDoorWallLine)
        {
            listOfListDoorPanelLines_ObjId = new List<List<ObjectId>>();
            listOfListDoorPanelOffsetLines_ObjId = new List<List<ObjectId>>();
            listOfDoorObjId_InsctWall = new List<ObjectId>();
            listOfParapetId_WithDoorWallLine = new List<ObjectId>();
            for (int i = 0; i < listOfDoorObjId_With_DoorPanelLine.Count; i++)
            {
                string data = listOfDoorObjId_With_DoorPanelLine[i];
                var array = data.Split('@');
                var doorObj_Str = Convert.ToString(array.GetValue(0));
                var doorPanelLineData = Convert.ToString(array.GetValue(1));
                var doorPanelLineDataArray = doorPanelLineData.Split(',');

                List<ObjectId> listOfPanelLineObjId = new List<ObjectId>();
                List<ObjectId> listOfPanelOffstLineObjId = new List<ObjectId>();
                List<ObjectId> listOfParapetId = new List<ObjectId>();
                foreach (var doorPanelLine_ObjId in doorPanelLineDataArray)
                {
                    int indexOfDoorPanelId_Str = listOfDoorPanelLine_ObjId_InStr.IndexOf(doorPanelLine_ObjId);
                    if (indexOfDoorPanelId_Str != -1)
                    {
                        listOfPanelLineObjId.Add(listOfDoorPanelLine_ObjId[indexOfDoorPanelId_Str]);
                        listOfPanelOffstLineObjId.Add(listOfDoorPanelOffsetLine_ObjId[indexOfDoorPanelId_Str]);
                        listOfParapetId.Add(ParapetHelper.listOfParapetId_WithDoorWallLine[indexOfDoorPanelId_Str]);
                    }
                }
                if (listOfPanelLineObjId.Count != 0)
                {
                    listOfListDoorPanelLines_ObjId.Add(listOfPanelLineObjId);
                    listOfListDoorPanelOffsetLines_ObjId.Add(listOfPanelOffstLineObjId);
                    listOfParapetId_WithDoorWallLine.Add(listOfParapetId[0]);
                }
                var indexOfDoor = DoorHelper.listOfDoorObjId_Str.IndexOf(doorObj_Str);
                if (indexOfDoor != -1)
                {
                    listOfDoorObjId_InsctWall.Add(listOfDoorObjId[indexOfDoor]);
                }
            }
        }

        public List<List<ObjectId>> getDoorOrWindowWallLinesId(ObjectId doorId, List<ObjectId> listOfDoorPanelLine_Id, List<ObjectId> listOfDoorPanelLine_ObjId, out List<string> listOfBeamLayerName_DoorWallInsct, out List<ObjectId> listOfDoor_BeamInsctId, out List<double> listOfDistBtwDoorToBeam, out List<ObjectId> listOfNrstBtwDoorToBeamWallLineId, out List<bool> listOfDoor_SunkanSlabWallLineflag, out List<ObjectId> listOfDoor_SunkanSlabId)
        {
            listOfBeamLayerName_DoorWallInsct = new List<string>();
            listOfDoor_BeamInsctId = new List<ObjectId>();
            listOfDistBtwDoorToBeam = new List<double>();
            listOfNrstBtwDoorToBeamWallLineId = new List<ObjectId>();
            listOfDoor_SunkanSlabWallLineflag = new List<bool>();
            listOfDoor_SunkanSlabId = new List<ObjectId>();

            List<List<ObjectId>> listOfListOfDoorWallNewLineId = new List<List<ObjectId>>();

            if (listOfDoorPanelLine_Id.Count == 2)
            {
                CommonModule commonMod = new CommonModule();
                List<Point3d> listOfAllPoint = new List<Point3d>();

                ObjectId doorWallLineId1 = listOfDoorPanelLine_Id[0];
                List<Point3d> listOfDoorWallLine1_StrtEndPoint = commonMod.getStartEndPointOfLine(doorWallLineId1);
                Point3d doorWallLine1_StrtPoint = listOfDoorWallLine1_StrtEndPoint[0];
                Point3d doorWallLine1_EndPoint = listOfDoorWallLine1_StrtEndPoint[1];
                listOfAllPoint.Add(doorWallLine1_StrtPoint);
                listOfAllPoint.Add(doorWallLine1_EndPoint);

                ObjectId doorWallLineId2 = listOfDoorPanelLine_Id[1];
                List<Point3d> listOfDoorWallLine2_StrtEndPoint = commonMod.getStartEndPointOfLine(doorWallLineId2);
                Point3d doorWallLine2_StrtPoint = listOfDoorWallLine2_StrtEndPoint[0];
                Point3d doorWallLine2_EndPoint = listOfDoorWallLine2_StrtEndPoint[1];
                listOfAllPoint.Add(doorWallLine2_StrtPoint);
                listOfAllPoint.Add(doorWallLine2_EndPoint);

                int indexOfDoorWallLine1 = listOfDoorPanelLine_ObjId.IndexOf(doorWallLineId1);
                int indexOfDoorWallLine2 = listOfDoorPanelLine_ObjId.IndexOf(doorWallLineId2);

                double angleOfLine = AEE_Utility.GetAngleOfLine(doorWallLineId1);

                bool flag_X_Axis = true;
                bool flag_Y_Axis = false;

                commonMod.checkAngleOfLine_Axis(angleOfLine, out flag_X_Axis, out flag_Y_Axis);

                List<Point3d> listOfDoorWallLine1_AllPoint = new List<Point3d>();
                List<Point3d> listOfDoorWallLine2_AllPoint = new List<Point3d>();

                List<double> listOfXY_ForCheck = new List<double>();
                for (int i = 0; i < listOfAllPoint.Count; i++)
                {
                    Point3d point = listOfAllPoint[i];
                    double X = point.X;
                    double Y = point.Y;
                    if (flag_X_Axis == true && (!listOfXY_ForCheck.Contains(X)))
                    {
                        bool flagOfAdd = false;
                        if (X >= doorWallLine1_StrtPoint.X && X <= doorWallLine1_EndPoint.X)
                        {
                            Point3d wallLine1_Point = new Point3d(X, doorWallLine1_StrtPoint.Y, 0);
                            listOfDoorWallLine1_AllPoint.Add(wallLine1_Point);
                            flagOfAdd = true;
                        }
                        if (X >= doorWallLine2_StrtPoint.X && X <= doorWallLine2_EndPoint.X)
                        {
                            Point3d wallLine2_Point = new Point3d(X, doorWallLine2_StrtPoint.Y, 0);
                            listOfDoorWallLine2_AllPoint.Add(wallLine2_Point);
                            flagOfAdd = true;
                        }
                        listOfDoorWallLine1_AllPoint = listOfDoorWallLine1_AllPoint.OrderBy(sortPoint => sortPoint.X).ToList();
                        listOfDoorWallLine2_AllPoint = listOfDoorWallLine2_AllPoint.OrderBy(sortPoint => sortPoint.X).ToList();

                        if (flagOfAdd == true)
                        {
                            listOfXY_ForCheck.Add(X);
                        }
                    }
                    else if (flag_Y_Axis == true && (!listOfXY_ForCheck.Contains(Y)))
                    {
                        bool flagOfAdd = false;
                        if (Y >= doorWallLine1_StrtPoint.Y && Y <= doorWallLine1_EndPoint.Y)
                        {
                            Point3d wallLine1_Point = new Point3d(doorWallLine1_StrtPoint.X, Y, 0);
                            listOfDoorWallLine1_AllPoint.Add(wallLine1_Point);
                            flagOfAdd = true;
                        }
                        if (Y >= doorWallLine2_StrtPoint.Y && Y <= doorWallLine2_EndPoint.Y)
                        {
                            Point3d wallLine2_Point = new Point3d(doorWallLine2_StrtPoint.X, Y, 0);
                            listOfDoorWallLine2_AllPoint.Add(wallLine2_Point);
                            flagOfAdd = true;
                        }
                        listOfDoorWallLine1_AllPoint = listOfDoorWallLine1_AllPoint.OrderBy(sortPoint => sortPoint.Y).ToList();
                        listOfDoorWallLine2_AllPoint = listOfDoorWallLine2_AllPoint.OrderBy(sortPoint => sortPoint.Y).ToList();

                        if (flagOfAdd == true)
                        {
                            listOfXY_ForCheck.Add(Y);
                        }
                    }
                }

                var doorWallLine1_Ent = AEE_Utility.GetEntityForRead(doorWallLineId1);
                var xDataRegAppName_Wall1 = AEE_Utility.GetXDataRegisterAppName(doorWallLineId1);
                List<ObjectId> listOfDoorWall1NewLineId = getNewDoorOrWindowWallLineId(doorId, doorWallLine1_Ent, xDataRegAppName_Wall1, listOfDoorWallLine1_AllPoint);
                listOfListOfDoorWallNewLineId.Add(listOfDoorWall1NewLineId);

                var doorWallLine2_Ent = AEE_Utility.GetEntityForRead(doorWallLineId2);
                var xDataRegAppName_Wall2 = AEE_Utility.GetXDataRegisterAppName(doorWallLineId2);
                List<ObjectId> listOfDoorWall2NewLineId = getNewDoorOrWindowWallLineId(doorId, doorWallLine2_Ent, xDataRegAppName_Wall2, listOfDoorWallLine2_AllPoint);
                listOfListOfDoorWallNewLineId.Add(listOfDoorWall2NewLineId);

                var doorWindowWallLine = AEE_Utility.GetEntityForRead(doorId);
                List<string> listOfBeamLyrName_InsctToDoorWall = new List<string>();
                List<double> listOfDistanceBtwDoorToBeam = new List<double>();
                List<ObjectId> listOfNearestBtwDoorToBeamWallLineId = new List<ObjectId>();
                List<ObjectId> listOfInsctDoor_BeamInsctId = new List<ObjectId>();
                List<bool> listOfDoorWall_SunkanSlabWallLineflag = new List<bool>();
                List<ObjectId> listOfDoorWall_SunkanSlabId = new List<ObjectId>();
                if (Door_UI_Helper.checkDoorLayerIsExist(doorWindowWallLine.Layer))
                {
                    listOfBeamLyrName_InsctToDoorWall = BeamHelper.listOfBeamName_InsctToDoorWall;
                    listOfDistanceBtwDoorToBeam = BeamHelper.listOfDistanceBtwDoorToBeam;
                    listOfNearestBtwDoorToBeamWallLineId = BeamHelper.listOfNearestBtwDoorToBeamBeamWallLineId;
                    listOfInsctDoor_BeamInsctId = BeamHelper.listOfInsctDoor_BeamInsctId;
                    listOfDoorWall_SunkanSlabWallLineflag = SunkanSlabHelper.listOfDoorWall_SunkanSlabWallLineflag;
                    listOfDoorWall_SunkanSlabId = SunkanSlabHelper.listOfDoorWall_SunkanSlabId;
                }
                //Added on 10/03/2023 by SDM
                else if (Beam_UI_Helper.checkBeamLayerIsExist(doorWindowWallLine.Layer))
                {
                    listOfBeamLyrName_InsctToDoorWall = BeamHelper.listOfBeamName_InsctToDoorWall;
                    listOfDistanceBtwDoorToBeam = BeamHelper.listOfDistanceBtwDoorToBeam;
                    listOfNearestBtwDoorToBeamWallLineId = BeamHelper.listOfNearestBtwDoorToBeamBeamWallLineId;
                    listOfInsctDoor_BeamInsctId = BeamHelper.listOfInsctDoor_BeamInsctId;
                    listOfDoorWall_SunkanSlabWallLineflag = SunkanSlabHelper.listOfDoorWall_SunkanSlabWallLineflag;
                    listOfDoorWall_SunkanSlabId = SunkanSlabHelper.listOfDoorWall_SunkanSlabId;
                }
                else if (Window_UI_Helper.checkWindowLayerIsExist(doorWindowWallLine.Layer))
                {
                    listOfBeamLyrName_InsctToDoorWall = BeamHelper.listOfBeamName_InsctToWindowWall;
                    listOfDistanceBtwDoorToBeam = BeamHelper.listOfDistanceBtwWindowToBeam;
                    listOfNearestBtwDoorToBeamWallLineId = BeamHelper.listOfNearestBtwWindowToBeamWallLineId;
                    listOfInsctDoor_BeamInsctId = BeamHelper.listOfInsctWindow_BeamInsctId;
                    listOfDoorWall_SunkanSlabWallLineflag = SunkanSlabHelper.listOfWindowWall_SunkanSlabWallLineflag;
                    listOfDoorWall_SunkanSlabId = SunkanSlabHelper.listOfWindowWall_SunkanSlabId;
                }

                listOfBeamLayerName_DoorWallInsct.Add(listOfBeamLyrName_InsctToDoorWall[indexOfDoorWallLine1]);
                listOfBeamLayerName_DoorWallInsct.Add(listOfBeamLyrName_InsctToDoorWall[indexOfDoorWallLine2]);

                listOfDoor_BeamInsctId.Add(listOfInsctDoor_BeamInsctId[indexOfDoorWallLine1]);
                listOfDoor_BeamInsctId.Add(listOfInsctDoor_BeamInsctId[indexOfDoorWallLine2]);

                listOfDistBtwDoorToBeam.Add(listOfDistanceBtwDoorToBeam[indexOfDoorWallLine1]);
                listOfDistBtwDoorToBeam.Add(listOfDistanceBtwDoorToBeam[indexOfDoorWallLine2]);

                listOfNrstBtwDoorToBeamWallLineId.Add(listOfNearestBtwDoorToBeamWallLineId[indexOfDoorWallLine1]);
                listOfNrstBtwDoorToBeamWallLineId.Add(listOfNearestBtwDoorToBeamWallLineId[indexOfDoorWallLine2]);

                listOfDoor_SunkanSlabWallLineflag.Add(listOfDoorWall_SunkanSlabWallLineflag[indexOfDoorWallLine1]);
                listOfDoor_SunkanSlabWallLineflag.Add(listOfDoorWall_SunkanSlabWallLineflag[indexOfDoorWallLine2]);

                listOfDoor_SunkanSlabId.Add(listOfDoorWall_SunkanSlabId[indexOfDoorWallLine1]);
                listOfDoor_SunkanSlabId.Add(listOfDoorWall_SunkanSlabId[indexOfDoorWallLine2]);
            }
            return listOfListOfDoorWallNewLineId;
        }

        private List<ObjectId> getNewDoorOrWindowWallLineId(ObjectId doorOrWindwId, Entity doorWallLineEnt, string xDataRegAppName, List<Point3d> listOfDoorWallLine_AllPoint)
        {
            List<ObjectId> listOfDoorWallNewLineId = new List<ObjectId>();
            for (int k = 0; k < listOfDoorWallLine_AllPoint.Count; k++)
            {
                if (k == (listOfDoorWallLine_AllPoint.Count - 1))
                {
                    break;
                }
                Point3d point1 = listOfDoorWallLine_AllPoint[k];
                Point3d point2 = listOfDoorWallLine_AllPoint[k + 1];
                var doorWallNewLine1_Lngth = AEE_Utility.GetLengthOfLine(point1, point2);
                if (doorWallNewLine1_Lngth > 1e-6)
                {
                    ObjectId doorWallNewLine1_Id = AEE_Utility.getLineId(doorWallLineEnt, point1, point2, false);

                    AEE_Utility.AttachXData(doorWallNewLine1_Id, xDataRegAppName, CommonModule.xDataAsciiName);

                    ObjectId doorWallNewOffstLine1_Id = new ObjectId();

                    ObjectId offsetLineId = AEE_Utility.OffsetLine(doorWallNewLine1_Id, 2, false);
                    var midPoint = AEE_Utility.GetMidPoint(offsetLineId);
                    var flagOfInside = AEE_Utility.GetPointIsInsideThePolyline(doorOrWindwId, midPoint);
                    if (flagOfInside == true)
                    {
                        doorWallNewOffstLine1_Id = AEE_Utility.OffsetLine(doorWallNewLine1_Id, -CommonModule.panelDepth, false);
                    }
                    else
                    {
                        doorWallNewOffstLine1_Id = AEE_Utility.OffsetLine(doorWallNewLine1_Id, CommonModule.panelDepth, false);
                    }
                    listOfDoorWallNewLineId.Add(doorWallNewLine1_Id);
                    listOfDoorWallNewLineId.Add(doorWallNewOffstLine1_Id);

                    AEE_Utility.deleteEntity(offsetLineId);
                }
            }
            return listOfDoorWallNewLineId;
        }

        private void getDoorWallText(string doorLayer, string beamLayer, ObjectId doorPanelLineId, ObjectId sunkanSlabId, out double doorWallXHeight)
        {
            doorWallXHeight = 0;
            Entity doorPanelLineEnt = AEE_Utility.GetEntityForRead(doorPanelLineId);
            double doorLintelLevel = Door_UI_Helper.getDoorLintelLevel(doorLayer);
            double beamBottom = Beam_UI_Helper.getOffsetBeamBottom(beamLayer);
            if (InternalWallSlab_UI_Helper.IsInternalWall(doorPanelLineEnt.Layer) || StairCase_UI_Helper.checkStairCaseLayerIsExist(doorPanelLineEnt.Layer))
            {
                doorWallXHeight = GeometryHelper.getHeightOfDoorWallPanelX_InternalWall(InternalWallSlab_UI_Helper.getSlabBottom(doorPanelLineEnt.Layer), beamBottom, doorLintelLevel, PanelLayout_UI.SL, PanelLayout_UI.RC, CommonModule.internalCorner, sunkanSlabId);
            }
            else if (doorPanelLineEnt.Layer == CommonModule.externalWallLayerName || doorPanelLineEnt.Layer == CommonModule.ductLayerName || Lift_UI_Helper.checkLiftLayerIsExist(doorPanelLineEnt.Layer))
            {
                doorWallXHeight = GeometryHelper.getHeightOfDoorWallPanelX_ExternalWall(InternalWallSlab_UI_Helper.getSlabBottom(doorPanelLineEnt.Layer), beamBottom, PanelLayout_UI.getSlabThickness(doorPanelLineEnt.Layer), doorLintelLevel, PanelLayout_UI.kickerHt, PanelLayout_UI.RC);
            }
        }

        private void drawWallPanels_In_DoorThick(Dictionary<Point3d, Tuple<double, bool>> dicCornerPoints)
        {
            WindowHelper windowHlp = new WindowHelper();

            CommonModule commonModule = new CommonModule();
            WallPanelHelper wallPanelHlpr = new WallPanelHelper();
            string doorThickWallPanelName = CommonModule.doorThickWallPanelText.Replace('X', ' ').Trim();// "DS";
            for (int k = 0; k < listOfListOfDoorThickWallPanelId_InsctToCorner.Count; k++)
            {
                ObjectId doorId = listOfDoorIdThickWallPanel_InsctToCorner[k];
                var doorEnt = AEE_Utility.GetEntityForRead(doorId);
                var listOfDoorThickWallPanelId_InsctToCorner = listOfListOfDoorThickWallPanelId_InsctToCorner[k];
                var listOffsetDoorThickWallPanelId_InsctToCorner = listOfListOffsetDoorThickWallPanelId_InsctToCorner[k];

                List<List<ObjectId>> listOfListRemainingDoorThickLineId = drawDoorThickPanel_NotIntersectToWallCorner(CommonModule.doorWallPanelType, doorId, listOfDoorId_NotInsctToCorner, listOfDoorThickWallPanelId_InsctToCorner, listOffsetDoorThickWallPanelId_InsctToCorner, listOfListOfDoorCornerId_NotInsctToWallCorner, listOfListOfDoorCornerTextId_NotInsctToWallCorner, CommonModule.doorThickWallPanelText);

                if (listOfListRemainingDoorThickLineId.Count != 0)
                {
                    // For Door End Panel is inline with Wall Panel because of that will use instead of "DS" to "WP"
                    //doorThickWallPanelName = PanelLayout_UI.wallPanelName;

                    listOfDoorThickWallPanelId_InsctToCorner = listOfListRemainingDoorThickLineId[0];
                    listOffsetDoorThickWallPanelId_InsctToCorner = listOfListRemainingDoorThickLineId[1];

                    for (int j = 0; j < listOfDoorThickWallPanelId_InsctToCorner.Count; j++)
                    {
                        ObjectId doorThickLineId = listOfDoorThickWallPanelId_InsctToCorner[j];
                        var xDataRegAppName = AEE_Utility.GetXDataRegisterAppName(doorThickLineId);
                        //Changes made on 04/06/2023 by SDM
                        var intersectToBeam = BeamHelper.IsBeamCornerIntersectsWithAnotherBeam(doorId, doorThickLineId);
                        if (intersectToBeam)
                            continue;

                        ObjectId offsetDoorThickLineId = listOffsetDoorThickWallPanelId_InsctToCorner[j];

                        var doorPanelLine = AEE_Utility.GetLine(doorThickLineId);
                        var wall1 = InternalWallHelper.GetInternalWallLineByxDataRegAppName(xDataRegAppName);
                        double doorThickPanelHght = GeometryHelper.getHeightOfDoorPanelThick(doorPanelLine.Layer, CommonModule.internalCorner, PanelLayout_UI.RC, PanelLayout_UI.flagOfPanelWithRC);
                        //By MT on 01/05/2022---------------------------------------------------------------
                        string suffix = string.Empty;
                        Point2d startPoint = new Point2d(doorPanelLine.StartPoint.X, doorPanelLine.StartPoint.Y);
                        Point2d endPoint = new Point2d(doorPanelLine.EndPoint.X, doorPanelLine.EndPoint.Y);
                        bool isCornerGroupCreate = InternalWallHelper.listOfNearestCornerPoint.Contains(startPoint) || InternalWallHelper.listOfNearestCornerPoint.Contains(endPoint);
                        ObjectId cornerID = new ObjectId();
                        List<ObjectId> cornerTextID = new List<ObjectId>();
                        List<ObjectId> listOfDoorCornerId = new List<ObjectId>();
                        List<List<ObjectId>> listOfDoorCornerTextId = new List<List<ObjectId>>();
                        if (isCornerGroupCreate)
                        {
                            // doorThickWallPanelName ="DSX";
                            if (PanelLayout_UI.flagOfExtendPanelAtExternalCornersOption)
                                doorThickWallPanelName += "X";//by SDM 2022-08-17
                            for (int n = 0; n < InternalWallHelper.listOfDoorCornerId.Count; n++)
                            {
                                var vertextPoint = AEE_Utility.GetPolylineVertexPoint(InternalWallHelper.listOfDoorCornerId[n]);
                                if (vertextPoint.Contains(startPoint) || vertextPoint.Contains(endPoint))
                                {
                                    cornerID = InternalWallHelper.listOfDoorCornerId[n];
                                    cornerTextID = InternalWallHelper.listOfDoorCornerTextId[n];
                                    break;
                                }
                            }
                            //var dicDoor = dicDoorToEALines[doorId];
                            var dicDoor = dicDoorToEALines.ContainsKey(doorId) ? dicDoorToEALines[doorId] : new List<Tuple<LineSegment3d, LineSegment3d>>();
                            int indexOfDoorExist = listOfDoorId_NotInsctToCorner.IndexOf(doorId);
                            //var drcrId= listOfListOfDoorCornerId_NotInsctToWallCorner.index
                            listOfDoorCornerId = new List<ObjectId>();
                            listOfDoorCornerTextId = new List<List<ObjectId>>();
                            //getDoorThickCornerId(doorThickLineId, listOfListOfDoorCornerId_NotInsctToWallCorner[indexOfDoorExist], listOfListOfDoorCornerTextId_NotInsctToWallCorner[indexOfDoorExist], out listOfDoorCornerId, out listOfDoorCornerTextId);
                            if (indexOfDoorExist != -1)//Changes made on 30/05/2023 by SDM
                                for (int m = 0; m < listOfListOfDoorCornerId_NotInsctToWallCorner[indexOfDoorExist].Count; m++)
                                {
                                    var vertextPoint = AEE_Utility.GetPolylineVertexPoint(listOfListOfDoorCornerId_NotInsctToWallCorner[indexOfDoorExist][m]);
                                    if (vertextPoint.Contains(startPoint) || vertextPoint.Contains(endPoint))
                                    {
                                        listOfDoorCornerId.Add(listOfListOfDoorCornerId_NotInsctToWallCorner[indexOfDoorExist][m]);
                                        listOfDoorCornerTextId.Add(listOfListOfDoorCornerTextId_NotInsctToWallCorner[indexOfDoorExist][m]);
                                        break;
                                    }
                                }

                            if (PanelLayout_UI.flagOfExtendPanelAtExternalCornersOption && listOfDoorCornerId.Count > 0)//Added by SDM 2022-08-17
                                foreach (var item in dicDoor)
                                {

                                    if (item.Item1.StartPoint.Equals(doorPanelLine.StartPoint) || (item.Item2.EndPoint.Equals(doorPanelLine.EndPoint)))
                                    {
                                        if (Math.Round(doorPanelLine.StartPoint.X, 6).Equals(Math.Round(doorPanelLine.EndPoint.X, 6)) && cornerTextID?.Count == 0)
                                        {
                                            var drCrPoints = AEE_Utility.GetPolylineVertexPoint(listOfDoorCornerId[0]).ToList();
                                            var Ypt = drCrPoints.Select(s => s.Y).ToList();

                                            /// Fix (R)/(L) SDM 12_May_2022
                                            // suffix = Ypt.Max() < doorPanelLine.GeometricExtents.MaxPoint.Y ? "(L)" : "(R)";

                                            var Xpt = drCrPoints.Select(s => s.X).ToList();

                                            //Direction : Left to Right
                                            if (Xpt.Min() < doorPanelLine.GeometricExtents.MaxPoint.X)
                                                suffix = Ypt.Max() > doorPanelLine.GeometricExtents.MaxPoint.Y ? "(L)" : "(R)";

                                            //Direction : Right to Left
                                            else
                                                suffix = Ypt.Max() < doorPanelLine.GeometricExtents.MaxPoint.Y ? "(L)" : "(R)";
                                        }
                                        else if (Math.Round(doorPanelLine.StartPoint.Y, 6).Equals(Math.Round(doorPanelLine.EndPoint.Y, 6)) && cornerTextID?.Count == 0)
                                        {
                                            var drCrPoints = AEE_Utility.GetPolylineVertexPoint(listOfDoorCornerId[0]).ToList();
                                            var Xpt = drCrPoints.Select(s => s.X).ToList();

                                            /// Fix (R)/(L) SDM 12_May_2022
                                            //suffix = Xpt.Max() < doorPanelLine.GeometricExtents.MaxPoint.X ? "(L)" : "(R)";

                                            var Ypt = drCrPoints.Select(s => s.Y).ToList();

                                            //Direction : Top to Bottom
                                            if (Ypt.Max() > doorPanelLine.GeometricExtents.MaxPoint.Y)
                                                suffix = Xpt.Max() > doorPanelLine.GeometricExtents.MaxPoint.X ? "(L)" : "(R)";

                                            //Direction : Bottom to Top
                                            else
                                                suffix = Xpt.Min() < doorPanelLine.GeometricExtents.MinPoint.X ? "(L)" : "(R)";
                                        }

                                        dicDoor.Remove(item);
                                        break;
                                    }
                                }
                        }
                        else
                        {
                            doorThickWallPanelName = PanelLayout_UI.wallPanelName;
                        }
                        //----------------------------------------------------------------------------------
                        List<List<ObjectId>> listOfListOfWallPanelRectAndTextId = new List<List<ObjectId>>();
                        drawDoorThickPanel(xDataRegAppName, doorId, CommonModule.doorWallPanelType, doorThickLineId, offsetDoorThickLineId, doorThickWallPanelName, out listOfListOfWallPanelRectAndTextId, suffix);
                        //By MT on 01/05/2022
                        if (isCornerGroupCreate)
                        {
                            if (cornerTextID.Count > 0)
                            {
                                listOfDoorCornerId.Add(cornerID);
                                listOfDoorCornerTextId.Add(cornerTextID);
                                InternalWallHelper.listOfDoorCornerId.Remove(cornerID);
                                InternalWallHelper.listOfDoorCornerTextId.Remove(cornerTextID);
                            }
                            createGroupInDoorThickPanel(listOfListOfWallPanelRectAndTextId, listOfDoorCornerId, listOfDoorCornerTextId, true, false);
                        }
                        //-----
                    }
                }
                ObjectId sunkanSlabId = new ObjectId();

                windowHlp.drawWindowDoorBottomPanel(doorId, doorEnt.Layer, sunkanSlabId, CommonModule.doorWallPanelType, dicCornerPoints);
            }
            AEE_Utility.deleteEntity(listOfDeleteIdOfDoorPanelThick);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xDataRegAppName"></param>
        /// <param name="doorId"></param>
        /// <param name="wallPanelType"></param>
        /// <param name="doorThickLineId"></param>
        /// <param name="offsetDoorThickLineId"></param>
        /// <param name="doorThickWallPanelName"></param>
        /// <param name="listOfListOfWallPanelRectAndTextId"></param>
        /// <param name="suffix"></param>
        private void drawDoorThickPanel(string xDataRegAppName, ObjectId doorId, string wallPanelType, ObjectId doorThickLineId, ObjectId offsetDoorThickLineId, string doorThickWallPanelName, out List<List<ObjectId>> listOfListOfWallPanelRectAndTextId, string suffix)
        {
            listOfListOfWallPanelRectAndTextId = new List<List<ObjectId>>();

            List<ObjectId> listOfWallPanelRect_Id = new List<ObjectId>();
            List<ObjectId> listOfWallTextId = new List<ObjectId>();
            List<ObjectId> listOfWallXTextId = new List<ObjectId>();

            CommonModule commonModule = new CommonModule();
            WallPanelHelper wallPanelHlpr = new WallPanelHelper();

            var doorPanelLine = AEE_Utility.GetLine(doorThickLineId);
            var listOfDoorPanelLineStrtEndPoint = commonModule.getStartEndPointOfLine(doorPanelLine);
            Point3d doorPanelLineStrtPoint = listOfDoorPanelLineStrtEndPoint[0];
            Point3d doorPanelLineEndPoint = listOfDoorPanelLineStrtEndPoint[1];

            var doorPanelOffsetLine = AEE_Utility.GetLine(offsetDoorThickLineId);
            var listOfDoorPanelOffsetLineStrtEndPoint = commonModule.getStartEndPointOfLine(doorPanelOffsetLine);
            Point3d doorPanelOffstLineStrtPoint = listOfDoorPanelOffsetLineStrtEndPoint[0];
            Point3d doorPanelOffstLineEndPoint = listOfDoorPanelOffsetLineStrtEndPoint[1];

            var lineSeg = new LineSegment3d(doorPanelLineStrtPoint, doorPanelLineEndPoint);
            var offsetLineSeg = new LineSegment3d(doorPanelOffstLineStrtPoint, doorPanelOffstLineEndPoint);
            double? wallLen = null;
            // if (dicDoorToEALines.ContainsKey(doorId))
            if (PanelLayout_UI.flagOfExtendPanelAtExternalCornersOption && dicDoorToEALines.ContainsKey(doorId))//Updated by SDM 2022-08-17
            {
                var t = 1e-6;
                var tol = new Tolerance(t, t);
                var lstLines = dicDoorToEALines[doorId];
                foreach (var item in lstLines)
                {
                    var line = item.Item1;
                    if (line.StartPoint.IsEqualTo(doorPanelLineStrtPoint, tol) &&
                        line.EndPoint.IsEqualTo(doorPanelLineEndPoint, tol))
                    {
                        deleteEAEntities(line, dicDoorToEACorners[doorId]);
                        wallLen = lineSeg.Length;
                        lineSeg = item.Item2;
                        var xline = offsetLineSeg.GetLine();
                        offsetLineSeg = new LineSegment3d(xline.GetClosestPointTo(lineSeg.StartPoint).GetPoint(), xline.GetClosestPointTo(lineSeg.EndPoint).GetPoint());
                    }
                    else if (line.EndPoint.IsEqualTo(doorPanelLineStrtPoint, tol) &&
                        line.StartPoint.IsEqualTo(doorPanelLineEndPoint, tol))
                    {
                        deleteEAEntities(line, dicDoorToEACorners[doorId]);
                        wallLen = lineSeg.Length;
                        lineSeg = item.Item2;
                        var xline = offsetLineSeg.GetLine();
                        offsetLineSeg = new LineSegment3d(xline.GetClosestPointTo(lineSeg.StartPoint).GetPoint(), xline.GetClosestPointTo(lineSeg.EndPoint).GetPoint());
                    }
                }
            }

            double angleOfLine = AEE_Utility.GetAngleOfLine(doorPanelLineStrtPoint.X, doorPanelLineStrtPoint.Y, doorPanelLineEndPoint.X, doorPanelLineEndPoint.Y);
            bool flag_X_Axis = false;
            bool flag_Y_Axis = false;

            commonModule.checkAngleOfLine_Axis(angleOfLine, out flag_X_Axis, out flag_Y_Axis);

            double doorThickPanelHght = GeometryHelper.getHeightOfDoorPanelThick(doorPanelLine.Layer, CommonModule.doorWindowTopCornerHt, PanelLayout_UI.RC, PanelLayout_UI.flagOfPanelWithRC);
            bool flagOfRCWrite = true;
            double sidePanelLevel = 0.0;

            if (Window_UI_Helper.checkWindowLayerIsExist(doorPanelLine.Layer))
            {
                flagOfRCWrite = false;
                double windowLintelLevel = Window_UI_Helper.getWindowLintelLevel(doorPanelLine.Layer);
                double windowSillLevel = Window_UI_Helper.getWindowSillLevel(doorPanelLine.Layer);
                doorThickPanelHght = GeometryHelper.getHeightOfWindow_InAtNotCornerSide_EC_In_InternalExternalWall(windowLintelLevel, windowSillLevel);
                sidePanelLevel = windowSillLevel + CommonModule.internalCorner;
                //Fix elevation when there is sunkan by SDM 2022-07-04
                if (SunkanSlabHelper.listOfWallName_SunkanSlabId.ContainsKey(xDataRegAppName))
                    sidePanelLevel -= PanelLayout_UI.RC;
            }
            if (Beam_UI_Helper.checkBeamLayerIsExist(doorPanelLine.Layer))
            {
                //doorThickPanelHght = Beam_UI_Helper.GetBeamLintelLevel(doorPanelLine.Layer, xDataRegAppName) - CommonModule.internalCorner;//Changes made on 12/04/2023 by SDM
                doorThickPanelHght = Beam_UI_Helper.GetBeamLintelLevel(doorPanelLine.Layer, xDataRegAppName) - CommonModule.doorWindowTopCornerHt;
            }//Changes made on 05/07/2023 by PRT

            double lengthOfLine = AEE_Utility.GetLengthOfLine(doorPanelLineStrtPoint.X, doorPanelLineStrtPoint.Y, doorPanelLineEndPoint.X, doorPanelLineEndPoint.Y);
            List<double> listOfWallPanelLength = wallPanelHlpr.getListOfWallPanelLength(lineSeg.Length, PanelLayout_UI.standardPanelWidth, PanelLayout_UI.minWidthOfPanel);
            // List<double> listOfWallPanelLength =WallPanelHelper.defineListOfWallPanelLengthForTwoCornerCase(new List<ObjectId> { doorThickLineId}, doorPanelLineStrtPoint, doorPanelLineEndPoint, PanelLayout_UI.standardPanelWidth, PanelLayout_UI.minWidthOfPanel);
            //slistOfWallPanelLength=listOfWallPanelLength.OrderByDescending(X=>X).ToList();

            //if (wallLen != null && listOfWallPanelLength?.Count == 1)
            //    wallLen = lineSeg.Length;

            ObjectId sunkanSlabId = new ObjectId();

            double doorThickPanelHghtWithRC = 0;
            string wallPanelNameWithRC = "";
            WallPanelHelper.getWallPanelHeight_PanelWithRC(sunkanSlabId, PanelLayout_UI.flagOfPanelWithRC, doorThickPanelHght, doorThickWallPanelName, PanelLayout_UI.RC, InternalWallSlab_UI_Helper.defaulLayerName, out doorThickPanelHghtWithRC, out wallPanelNameWithRC);
            wallPanelNameWithRC += suffix;

            if (wallLen != null)
            {
                if (wallPanelNameWithRC.Contains(PanelLayout_UI.wallPanelName))
                {
                    // For Door End Panel is inline with Wall Panel and one end is with External Corner
                    wallPanelNameWithRC = wallPanelNameWithRC.Replace(PanelLayout_UI.wallPanelName + "R", PanelLayout_UI.wallPanelName + "RX");

                    List<Point3d> listOfStartEndPoints = AEE_Utility.GetStartEndPointOfLine(doorThickLineId);
                    List<Point3d> listOfDoorBoundingPoints = AEE_Utility.GetBoundingBoxOfPolyline(doorId);
                    ObjectId tempLineObj = AEE_Utility.getLineId(listOfDoorBoundingPoints[0], listOfDoorBoundingPoints[1], false);
                    Point3d doorMidPt = AEE_Utility.GetMidPoint(tempLineObj);
                    AEE_Utility.deleteEntity(tempLineObj);

                    Point3d doorpanelStpt = listOfStartEndPoints[0];
                    Point3d doorpanelEndpt = listOfStartEndPoints[1];

                    double distToMidPtFromStart = doorpanelStpt.DistanceTo(doorMidPt);
                    double distToMidPtFromEnd = doorpanelEndpt.DistanceTo(doorMidPt);

                    //Change the xDataRegAppName to the right wall by SDM 06_June_2022
                    //start

                    var walllines = InternalWallHelper.listOfListOfInternalWallRoomLineId_With_WallName;
                    for (int r = 0; r < walllines.Count; r++)
                    {
                        for (int w = 0; w < walllines[r].Count; w++)
                        {
                            var wall = AEE_Utility.GetLine(walllines[r][w]);
                            var name = AEE_Utility.GetXDataRegisterAppName(walllines[r][w]);

                            double wall_angleOfLine = AEE_Utility.GetAngleOfLine(wall.StartPoint.X, wall.StartPoint.Y, wall.EndPoint.X, wall.EndPoint.Y);
                            bool wall_flag_X_Axis = false;
                            bool wall_flag_Y_Axis = false;

                            commonModule.checkAngleOfLine_Axis(wall_angleOfLine, out wall_flag_X_Axis, out wall_flag_Y_Axis);
                            var dist = Math.Round(Math.Min(wall.StartPoint.DistanceTo(wall.GetClosestPointTo(doorPanelLineStrtPoint, true)), wall.EndPoint.DistanceTo(wall.GetClosestPointTo(doorPanelLineStrtPoint, true))));
                            if (dist > doorPanelLine.Length)
                                continue;
                            if (flag_X_Axis == wall_flag_X_Axis && flag_X_Axis == true && Math.Round(wall.StartPoint.Y) == Math.Round(doorPanelLineStrtPoint.Y) && Math.Round(wall.EndPoint.Y) == Math.Round(doorPanelLineEndPoint.Y))
                            {
                                xDataRegAppName = name;
                                AEE_Utility.AttachXData(doorThickLineId, name, CommonModule.xDataAsciiName);
                                if (SunkanSlabHelper.listOfWallName_SunkanSlabId.ContainsKey(name))
                                {
                                    sunkanSlabId = SunkanSlabHelper.listOfWallName_SunkanSlabId[name];
                                    doorThickPanelHghtWithRC -= PanelLayout_UI.RC;
                                }
                                break;
                            }
                            else if (flag_Y_Axis == wall_flag_Y_Axis && flag_Y_Axis == true && Math.Round(wall.StartPoint.X) == Math.Round(doorPanelLineStrtPoint.X) && Math.Round(wall.EndPoint.X) == Math.Round(doorPanelLineEndPoint.X))
                            {
                                xDataRegAppName = name;
                                AEE_Utility.AttachXData(doorThickLineId, name, CommonModule.xDataAsciiName);
                                if (SunkanSlabHelper.listOfWallName_SunkanSlabId.ContainsKey(name))
                                {
                                    sunkanSlabId = SunkanSlabHelper.listOfWallName_SunkanSlabId[name];
                                    doorThickPanelHghtWithRC -= PanelLayout_UI.RC;
                                }
                                break;
                            }
                        }

                    }//end

                    if (flag_X_Axis)
                    {
                        if (doorMidPt.Y < doorpanelStpt.Y)
                        {
                            if (distToMidPtFromStart < distToMidPtFromEnd && doorpanelStpt.X < doorpanelEndpt.X)
                            {
                                wallPanelNameWithRC += "(L)";
                            }
                            else if (distToMidPtFromStart < distToMidPtFromEnd && doorpanelStpt.X > doorpanelEndpt.X)
                            {
                                wallPanelNameWithRC += "(R)";
                            }
                            else if (distToMidPtFromStart > distToMidPtFromEnd && doorpanelStpt.X < doorpanelEndpt.X)
                            {
                                wallPanelNameWithRC += "(R)";
                            }
                            else if (distToMidPtFromStart > distToMidPtFromEnd && doorpanelStpt.X > doorpanelEndpt.X)
                            {
                                wallPanelNameWithRC += "(L)";
                            }
                        }
                        else
                        {
                            if (distToMidPtFromStart < distToMidPtFromEnd && doorpanelStpt.X < doorpanelEndpt.X)
                            {
                                wallPanelNameWithRC += "(R)";
                            }
                            else if (distToMidPtFromStart < distToMidPtFromEnd && doorpanelStpt.X > doorpanelEndpt.X)
                            {
                                wallPanelNameWithRC += "(L)";
                            }
                            else if (distToMidPtFromStart > distToMidPtFromEnd && doorpanelStpt.X < doorpanelEndpt.X)
                            {
                                wallPanelNameWithRC += "(L)";
                            }
                            else if (distToMidPtFromStart > distToMidPtFromEnd && doorpanelStpt.X > doorpanelEndpt.X)
                            {
                                wallPanelNameWithRC += "(R)";
                            }
                        }
                    }
                    else if (flag_Y_Axis)
                    {
                        if (doorMidPt.X < doorpanelStpt.X)
                        {
                            if (distToMidPtFromStart < distToMidPtFromEnd && doorpanelStpt.Y > doorpanelEndpt.Y)
                            {
                                wallPanelNameWithRC += "(L)";
                            }
                            else if (distToMidPtFromStart < distToMidPtFromEnd && doorpanelStpt.Y < doorpanelEndpt.Y)
                            {
                                wallPanelNameWithRC += "(R)";
                            }
                            else if (distToMidPtFromStart > distToMidPtFromEnd && doorpanelStpt.Y > doorpanelEndpt.Y)
                            {
                                wallPanelNameWithRC += "(R)";
                            }
                            else if (distToMidPtFromStart > distToMidPtFromEnd && doorpanelStpt.Y < doorpanelEndpt.Y)
                            {
                                wallPanelNameWithRC += "(L)";
                            }
                        }
                        else
                        {
                            if (distToMidPtFromStart < distToMidPtFromEnd && doorpanelStpt.Y > doorpanelEndpt.Y)
                            {
                                wallPanelNameWithRC += "(R)";
                            }
                            else if (distToMidPtFromStart < distToMidPtFromEnd && doorpanelStpt.Y < doorpanelEndpt.Y)
                            {
                                wallPanelNameWithRC += "(L)";
                            }
                            else if (distToMidPtFromStart > distToMidPtFromEnd && doorpanelStpt.Y > doorpanelEndpt.Y)
                            {
                                wallPanelNameWithRC += "(L)";
                            }
                            else if (distToMidPtFromStart > distToMidPtFromEnd && doorpanelStpt.Y < doorpanelEndpt.Y)
                            {
                                wallPanelNameWithRC += "(R)";
                            }
                        }
                    }

                    //----------------------------------------------------------------------------------------------------------

                }
                else
                    wallPanelNameWithRC = wallPanelNameWithRC.Replace("DSR", "DSXR");
            }

            List<ObjectId> listOfWallPanelLine_ObjId = new List<ObjectId>();
            listOfWallPanelRect_Id = wallPanelHlpr.drawWallPanels(xDataRegAppName, doorThickLineId, lineSeg.StartPoint, lineSeg.EndPoint, offsetLineSeg.StartPoint, offsetLineSeg.EndPoint, flag_X_Axis, flag_Y_Axis, listOfWallPanelLength, out listOfWallPanelLine_ObjId);
            List<ObjectId> listOfCircleId = new List<ObjectId>();

            if (Door_UI_Helper.checkDoorLayerIsExist(doorPanelLine.Layer))
                sidePanelLevel = sunkanSlabId.IsValid ? -100 : 0;
            //Added on 10/03/2023 by SDM
            if (Beam_UI_Helper.checkBeamLayerIsExist(doorPanelLine.Layer))
                sidePanelLevel = sunkanSlabId.IsValid ? -100 : 0;

            string doorLayer = "";//RTJ 10-06-2021
            string windowLayer = ""; //RTJ 10-06-2021
            if (xDataRegAppName.Contains("_LF_") || xDataRegAppName.Contains("_DU_"))
                doorThickPanelHghtWithRC -= PanelLayout_UI.RC;
            wallPanelHlpr.writeTextInWallPanel(wallPanelType, sunkanSlabId, InternalWallSlab_UI_Helper.defaulLayerName, listOfWallPanelRect_Id, listOfWallPanelLine_ObjId, doorThickPanelHghtWithRC, wallPanelNameWithRC, 0, "", PanelLayout_UI.RC, CommonModule.wallPanelTextLyrName, CommonModule.wallPanelLyrColor, flagOfRCWrite, out listOfWallTextId, out listOfWallXTextId, out listOfCircleId, doorLayer, windowLayer, sidePanelLevel, wallLen, flag_X_Axis, flag_Y_Axis, wallDepth: lengthOfLine);//RTJ 10-06-2021

            listOfListOfWallPanelRectAndTextId.Add(listOfWallPanelRect_Id);
            listOfListOfWallPanelRectAndTextId.Add(listOfWallTextId);
            listOfListOfWallPanelRectAndTextId.Add(listOfWallXTextId);

            AEE_Utility.deleteEntity(listOfCircleId); // circle not create in Door Thickness panel
        }

        private void deleteEAEntities(LineSegment3d line, List<DoorEACorner> list)
        {
            foreach (var item in list)
            {
                if (line.StartPoint == item.WallCornerPointOnDoor ||
                    line.EndPoint == item.WallCornerPointOnDoor)
                {
                    AEE_Utility.deleteEntity(item.TextIds);
                    AEE_Utility.deleteEntity(item.CornerId);
                    return;
                }
            }
        }

        public List<List<ObjectId>> drawDoorThickPanel_NotIntersectToWallCorner(string wallPanelType, ObjectId doorId, List<ObjectId> listOfDoorId_NotInsctToCorner, List<ObjectId> listOfDoorThickWallPanelId, List<ObjectId> listOffsetDoorThickWallPanelId, List<List<ObjectId>> listOfListOfDoorCornerId, List<List<List<ObjectId>>> listOfListOfDoorCornerTextId, string doorThickWallPanelText)
        {
            doorThickWallPanelText = PanelLayout_UI.flagOfExtendPanelAtExternalCornersOption ? doorThickWallPanelText : doorThickWallPanelText?.Replace('X', ' ')?.Trim();//added by SDM 2022-08-17
            List<List<ObjectId>> listOfListRemainingDoorThickLineId = new List<List<ObjectId>>();
            List<ObjectId> listOfThickLineId = new List<ObjectId>();
            List<ObjectId> listOfOffsetThickLineId = new List<ObjectId>();
            List<ObjectId> listOfDeleteIdOfDoorCorner = new List<ObjectId>();//By MT on 23/04/2022
            int indexOfDoorExist = listOfDoorId_NotInsctToCorner.IndexOf(doorId);

            if (indexOfDoorExist != -1)
            {
                var listOfDoorCornerId_NotInsctToWallCrner = listOfListOfDoorCornerId[indexOfDoorExist];
                if (listOfDoorCornerId_NotInsctToWallCrner.Count >= 2)
                {
                    var listOfDoorCornerTextId_NotInsctToWallCorner = listOfListOfDoorCornerTextId[indexOfDoorExist];

                    bool flagOfDoorPanel_LR = false;
                    bool flagOfDoorPanel = false;

                    for (int index = 0; index < listOfDoorThickWallPanelId.Count; index++)
                    {
                        ObjectId doorThickWallPanelId = listOfDoorThickWallPanelId[index];
                        ObjectId doorOffsetThickWallPanelId = listOffsetDoorThickWallPanelId[index];

                        bool flagOfGreaterThan = false;
                        double doorThickLength = AEE_Utility.GetLengthOfLine(doorThickWallPanelId);

                        if (doorThickLength > CommonModule.maxPanelThick)
                        {
                            flagOfGreaterThan = true;
                        }
                        else if (doorThickLength >= CommonModule.minPanelThick && listOfDoorCornerId_NotInsctToWallCrner.Count < 4)
                        {
                            flagOfGreaterThan = true;
                        }
                        if (flagOfGreaterThan == true)
                        {
                            listOfThickLineId.Add(doorThickWallPanelId);
                            listOfOffsetThickLineId.Add(doorOffsetThickWallPanelId);
                        }
                        else
                        {
                            var xDataRegAppName = AEE_Utility.GetXDataRegisterAppName(doorThickWallPanelId);

                            List<ObjectId> listOfDoorCornerId = new List<ObjectId>();
                            List<List<ObjectId>> listOfDoorCornerTextId = new List<List<ObjectId>>();
                            getDoorThickCornerId(doorThickWallPanelId, listOfDoorCornerId_NotInsctToWallCrner, listOfDoorCornerTextId_NotInsctToWallCorner, out listOfDoorCornerId, out listOfDoorCornerTextId);
                            var flag = PanelLayout_UI.flagOfPanelWithRC;
                            if (listOfDoorCornerId.Count == 2)
                            {
                                string doorPanelThickText = "";
                                string suffix = "";
                                if (doorThickLength > CommonModule.minPanelThick && doorThickLength <= CommonModule.maxPanelThick)
                                {
                                    flagOfDoorPanel_LR = true;
                                    if (index == 0)
                                    {
                                        doorPanelThickText = doorThickWallPanelText + "(L)";
                                    }
                                    else
                                    {
                                        doorPanelThickText = doorThickWallPanelText + "(R)";
                                    }
                                }
                                else
                                {
                                    var ind = listOfDoorCornerId.FindIndex(o => listOverlapExternals.Where(o1 => o1.CornerId == o).Any());
                                    if (ind != -1)
                                    {
                                        flag = false;
                                        flagOfDoorPanel_LR = true;
                                        var txt = doorThickWallPanelText;
                                        if (txt.EndsWith("X"))
                                        {
                                            txt = txt.Substring(0, txt.Length - 1);
                                        }
                                        if (index == 0)
                                        {
                                            doorPanelThickText = txt;
                                            suffix = "(L)";
                                        }
                                        else
                                        {
                                            doorPanelThickText = txt;
                                            suffix = "(R)";
                                        }
                                    }
                                    else
                                    {
                                        doorPanelThickText = doorThickWallPanelText;
                                        flagOfDoorPanel = true;
                                    }
                                }

                                List<List<ObjectId>> listOfListOfWallPanelRectAndTextId = new List<List<ObjectId>>();
                                drawDoorThickPanel(xDataRegAppName, doorId, wallPanelType, doorThickWallPanelId, doorOffsetThickWallPanelId, doorPanelThickText, out listOfListOfWallPanelRectAndTextId, suffix);
                                createGroupInDoorThickPanel(listOfListOfWallPanelRectAndTextId, listOfDoorCornerId, listOfDoorCornerTextId, flagOfDoorPanel, flagOfDoorPanel_LR);
                            }
                            else
                            {
                                listOfThickLineId.Add(doorThickWallPanelId);
                                listOfOffsetThickLineId.Add(doorOffsetThickWallPanelId);
                                //By MT on 23/04/2022
                                listOfDeleteIdOfDoorCorner.AddRange(DeleteUnwantedCorners(doorId, listOfDoorCornerId_NotInsctToWallCrner, listOfDoorCornerTextId_NotInsctToWallCorner));
                                //--------------------------------------------------------------
                                //By MT opn 30/04/2022
                                //if (wallPanelType == "Door Wall Panel")
                                //{
                                //    var stEndAng = AEE_Utility.getStartEndPointWithAngle_Line(doorThickWallPanelId);
                                //    var drInd = DoorHelper.listOfDoorObjId.IndexOf(doorId);
                                //    var drVert = AEE_Utility.GetPolylineVertexPoint(doorId);
                                //    for (int i = drInd+1; i < DoorHelper.listOfDoorObjId.Count; i++)
                                //    {
                                //        var drVert_i = AEE_Utility.GetPolylineVertexPoint(DoorHelper.listOfDoorObjId[i]);
                                //        var intsct = drVert.Intersect(drVert_i).ToList();
                                //        if (intsct.Count() == 1)
                                //        {
                                //            foreach (var item in InternalWallHelper.listOfDoorCornerId)
                                //            {
                                //                var drCrVert = AEE_Utility.GetPolylineVertexPoint(item);
                                //                var drcrintsct = drCrVert.Contains(intsct[0]);
                                //                if (drcrintsct)
                                //                {

                                //                }
                                //            }

                                //        }

                                //    }
                                //}
                                //--------------------------------------------------------------
                            }
                        }
                    }
                    if (listOfThickLineId.Count != 0)
                    {
                        listOfListRemainingDoorThickLineId.Add(listOfThickLineId);
                        listOfListRemainingDoorThickLineId.Add(listOfOffsetThickLineId);
                    }
                }
                else
                {
                    listOfListRemainingDoorThickLineId.Add(listOfDoorThickWallPanelId);
                    listOfListRemainingDoorThickLineId.Add(listOffsetDoorThickWallPanelId);
                }
            }
            else
            {
                listOfListRemainingDoorThickLineId.Add(listOfDoorThickWallPanelId);
                listOfListRemainingDoorThickLineId.Add(listOffsetDoorThickWallPanelId);
            }
            AEE_Utility.deleteEntity(listOfDeleteIdOfDoorCorner); //By MT on 23/04/2022
            return listOfListRemainingDoorThickLineId;
        }

        //By MT on 23/04/2022 -----------------------------------------------------------------
        private List<ObjectId> DeleteUnwantedCorners(ObjectId doorId, List<ObjectId> lstOfDoorCornerId_NotInsctToWallCrner, List<List<ObjectId>> lstOfDoorCornerTextId_NotInsctToWallCorner)
        {
            List<ObjectId> listOfDeleteIdOfDoorCorner = new List<ObjectId>();
            var doorpl = AEE_Utility.GetEntityForRead(doorId) as Polyline;
            var drmin = doorpl.GeometricExtents.MinPoint;
            var drmax = doorpl.GeometricExtents.MaxPoint;
            double x1, x2, y1, y2;
            x1 = Math.Round(drmin.X, 4);
            y1 = Math.Round(drmin.Y, 4);
            x2 = Math.Round(drmax.X, 4);
            y2 = Math.Round(drmax.Y, 4);
            //check each door corner lies between door wall for each side
            foreach (var item in lstOfDoorCornerId_NotInsctToWallCrner)
            {
                var crPl = AEE_Utility.GetEntityForRead(item) as Polyline;
                var crmin = crPl.GeometricExtents.MinPoint;
                var crmax = crPl.GeometricExtents.MaxPoint;
                double cx1, cx2, cy1, cy2;
                cx1 = Math.Round(crmin.X, 4);
                cy1 = Math.Round(crmin.Y, 4);
                cx2 = Math.Round(crmax.X, 4);
                cy2 = Math.Round(crmax.Y, 4);
                if (((x1 < cx1 && x2 > cx2) && (y2 == cy1 || y1 == cy2)) || ((y1 < cy1 && y2 > cy2) && (x2 == cx1 || x1 == cx2)))
                {
                    var ind = lstOfDoorCornerId_NotInsctToWallCrner.IndexOf(item);
                    listOfDeleteIdOfDoorCorner.Add(item);
                    listOfDeleteIdOfDoorCorner.AddRange(lstOfDoorCornerTextId_NotInsctToWallCorner[ind]);
                }
            }
            return listOfDeleteIdOfDoorCorner;
        }
        //--------------------------------------------------------------------------------------------------------
        private void eraseOverlapCorner(List<OverlapExternalDoorCorners> listOverlapExternals)
        {
            foreach (var item in listOverlapExternals)
            {
                var vertexPoints = AEE_Utility.GetPolyLinePointList(item.CornerId);

                var refPt = new Point3d(item.WallCornerPointOnDoor.X, item.WallCornerPointOnDoor.Y, 0);
                var ray = new Ray3d(item.WallCornerBasePoint, refPt);
                var dist = -1.0;
                foreach (var pt in vertexPoints)
                {
                    var cPt = ray.GetClosestPointTo(pt).Point;
                    if (dist < 0 || dist < cPt.DistanceTo(item.WallCornerBasePoint))
                    {
                        dist = cPt.DistanceTo(item.WallCornerBasePoint);
                    }
                }
                var newPt = item.WallCornerBasePoint + (refPt - item.WallCornerBasePoint).GetNormal() * dist;

                AEE_Utility.deleteEntity(item.CornerId);
                AEE_Utility.deleteEntity(item.TextIds);
                CornerHelper helper = new CornerHelper();
                helper.stretchCorner(item, newPt);
            }
        }

        private void createGroupInDoorThickPanel(List<List<ObjectId>> listOfListOfWallPanelRectAndTextId, List<ObjectId> listOfDoorCornerId, List<List<ObjectId>> listOfDoorCornerTextId, bool flagOfDoorPanel, bool flagOfDoorPanel_LR)
        {
            if (!PanelLayout_UI.flagOfExtendPanelAtExternalCornersOption)//Added by SDM 2022-08-17
                return;
            List<ObjectId> listOfGroupObjId = new List<ObjectId>();

            if (flagOfDoorPanel == true)
            {
                for (int i = 0; i < listOfDoorCornerId.Count; i++)
                {
                    ObjectId doorCornerId = listOfDoorCornerId[i];
                    AEE_Utility.changeLayer(doorCornerId, CommonModule.wallPanelLyrName, CommonModule.wallPanelLyrColor);

                    listOfGroupObjId.Add(doorCornerId);
                }
                foreach (var cornerTextId in listOfDoorCornerTextId)
                {
                    listOfDeleteIdOfDoorPanelThick.AddRange(cornerTextId);
                }
            }
            if (flagOfDoorPanel_LR == true)
            {
                ObjectId doorCornerId = listOfDoorCornerId[0];
                AEE_Utility.changeLayer(doorCornerId, CommonModule.wallPanelLyrName, CommonModule.wallPanelLyrColor);

                listOfGroupObjId.Add(doorCornerId);

                listOfDeleteIdOfDoorPanelThick.AddRange(listOfDoorCornerTextId[0]);
            }

            for (int i = 0; i < listOfListOfWallPanelRectAndTextId.Count; i++)
            {
                var listOfWallPanelRectAndTextId = listOfListOfWallPanelRectAndTextId[i];
                for (int j = 0; j < listOfWallPanelRectAndTextId.Count; j++)
                {
                    ObjectId id = listOfWallPanelRectAndTextId[j];
                    listOfGroupObjId.Add(id);
                }
            }
            string grpName = doorThickPanelGroupName + Convert.ToString(doorThickPanelGroupNumber);
            var flagOfGroup = AEE_Utility.CreateGroup(grpName, listOfGroupObjId);
            doorThickPanelGroupNumber++;
        }
        private void getDoorThickCornerId(ObjectId doorThickWallPanelId, List<ObjectId> listOfDoorCornerId, List<List<ObjectId>> listOfDoorCornerTextId, out List<ObjectId> listOfCornerId, out List<List<ObjectId>> listOfCornerTextId)
        {
            listOfCornerId = new List<ObjectId>();
            listOfCornerTextId = new List<List<ObjectId>>();

            CommonModule commonMod = new CommonModule();
            var listOfStrtEndPoint = commonMod.getStartEndPointOfLine(doorThickWallPanelId);

            foreach (var point in listOfStrtEndPoint)
            {
                List<double> listOfLength = new List<double>();
                foreach (var cornerId in listOfDoorCornerId)
                {
                    //var en = AEE_Utility.GetEntityForRead(listOfDoorCornerTextId[2][0]) as MText;
                    var basePoint = AEE_Utility.GetBasePointOfPolyline(cornerId);
                    var length = AEE_Utility.GetLengthOfLine(point, basePoint);
                    listOfLength.Add(length);
                }
                double min = listOfLength.Min();
                if (min <= 1)
                {
                    int indexOfMin = listOfLength.IndexOf(min);
                    listOfCornerId.Add(listOfDoorCornerId[indexOfMin]);
                    listOfCornerTextId.Add(listOfDoorCornerTextId[indexOfMin]);
                }
            }
        }


    }
}
