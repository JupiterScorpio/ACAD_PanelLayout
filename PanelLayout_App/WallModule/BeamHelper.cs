﻿using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using PanelLayout_App.CivilModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static PanelLayout_App.RotationHelper;
using PanelLayout_App.ElevationModule;

namespace PanelLayout_App.WallModule
{
    public class BeamHelper
    {
        public static List<ObjectId> listOfBeamLineId_ForMoveInBeamLayout = new List<ObjectId>();

        public static List<Point3d> listOfTwoBeamInsctPoint = new List<Point3d>();

        public static List<string> listOfLinesBtwTwoCrners_BeamInsctName = new List<string>();
        public static List<ObjectId> listOfLinesBtwTwoCrners_BeamInsctId = new List<ObjectId>();
        public static List<double> listOfDistanceBtwWallToBeam = new List<double>();
        public static List<ObjectId> listOfNearestBtwWallToBeamWallLineId = new List<ObjectId>();

        public static List<string> listOfBeamName_InsctToDoorWall = new List<string>();
        public static List<ObjectId> listOfInsctDoor_BeamInsctId = new List<ObjectId>();
        public static List<double> listOfDistanceBtwDoorToBeam = new List<double>();
        public static List<ObjectId> listOfNearestBtwDoorToBeamBeamWallLineId = new List<ObjectId>();


        public static List<string> listOfBeamName_InsctToWindowWall = new List<string>();
        public static List<ObjectId> listOfInsctWindow_BeamInsctId = new List<ObjectId>();
        public static List<double> listOfDistanceBtwWindowToBeam = new List<double>();
        public static List<ObjectId> listOfNearestBtwWindowToBeamWallLineId = new List<ObjectId>();

        public static List<List<ObjectId>> listOfListOfBeamSidePanelLineId = new List<List<ObjectId>>();
        public static List<ObjectId> listOfBeamSideWallId = new List<ObjectId>();
        public static List<ObjectId> listOfSunkanSlabObjId = new List<ObjectId>();

        private static List<ObjectId> listOfBeamBottomPanelLineId = new List<ObjectId>();
        private static List<double> listOfDistBtwBeamPanelLineToWallLine = new List<double>();
        private static List<ObjectId> listOfBeamId_ForBeamBotomPanel = new List<ObjectId>();
        private static List<ObjectId> listOfWallId_ForBeamBotomPanel = new List<ObjectId>();

        public static List<List<ObjectId>> listOfListOfBeamBottomPanelLineId = new List<List<ObjectId>>();
        public static List<List<double>> listOfListOfDistBtwBeamPanelLineToWallLine = new List<List<double>>();
        private static List<List<ObjectId>> listOfListOfBeamId_ForBeamBotomPanel = new List<List<ObjectId>>();
        private static List<List<ObjectId>> listOfListOfWallId_ForBeamBotomPanel = new List<List<ObjectId>>();
        public static List<ObjectId> listOfBeamId_BottomPanelLineDrawn = new List<ObjectId>();

        private static List<List<ObjectId>> listOfListOfBeamBottomCornerId = new List<List<ObjectId>>();
        private static List<List<ObjectId>> listOfListOfBeamCornerMaxLngthLineId = new List<List<ObjectId>>();

        private static List<List<ObjectId>> listOfListOfBeamBottomWallPanelId = new List<List<ObjectId>>();
        private static List<List<object>> listOfListOfBeamBottomPanelData = new List<List<object>>();

        public static List<ObjectId> listOfAllBeamCornerId_ForStretch = new List<ObjectId>();
        public static List<ObjectId> listOfAllBeamOffstCornerId_ForStretch = new List<ObjectId>();
        public static List<ObjectId> listOfAllBeamCornerTextId_ForStretch = new List<ObjectId>();
        public static List<ObjectId> listOfAllWallCornerId_WithBeam = new List<ObjectId>();

        public static List<ObjectId> listOfWallPanelLineId_WithBeamDoor = new List<ObjectId>();
        public static bool flagOfAdjacentWallLine_ToInsctToBeam = false;

        public static List<ObjectId> listOfAllStretchBeamCornerId = new List<ObjectId>();
        private static List<ObjectId> listOfAllBeamBottomCornerId = new List<ObjectId>();
        private static List<double> listOfListOfBeamlvls = new List<double>();
        public static Dictionary<Point3d, Point3d> dicBeamIntersectModifiedPoints = new Dictionary<Point3d, Point3d>(new PointComparer());

        public static List<Tuple<ObjectId, List<ObjectId>, List<ObjectId>, double, string>> listOfAllOffsetBeamCornerIdsWithBeamID = new List<Tuple<ObjectId, List<ObjectId>, List<ObjectId>, double, string>>();

        public static double maxWidthOfCornerLength = 0.0;//Modified by RTJ May-18 for Beam Max Length.

        public static List<Tuple<string, ObjectId, ObjectId>> listOfBeamBottomExternalWallPanelObj = new List<Tuple<string, ObjectId, ObjectId>>();
        public static List<Tuple<string, ObjectId, ObjectId>> listOfBeamBottomWallPanelObj = new List<Tuple<string, ObjectId, ObjectId>>();

        //Added on 12/03/2023 by SDM
        private static List<ObjectId> listOfIntersectBeamObjId = new List<ObjectId>();
        private static List<List<Point3d>> listOfListIntersectBeam_Points = new List<List<Point3d>>();

        private static List<ObjectId> listOfIntersectBeamId_ForTrimLineId = new List<ObjectId>();
        private static List<List<ObjectId>> listOfListIntersectBeam_LineId = new List<List<ObjectId>>();

        public static List<ObjectId> listOfBeamObjId = new List<ObjectId>();
        public static List<string> listOfBeamObjId_Str = new List<string>();
        public static List<List<LineSegment3d>> listBeamLines = new List<List<LineSegment3d>>();

        private static List<string> listOfExistBeamPanelLineObj = new List<string>();
        public static List<string> listOfDoorObjId_With_BeamPanelLine = new List<string>();
        public static Dictionary<ObjectId, List<Tuple<ObjectId, (Point3d, Point3d)>>> listOfBeamId_AllInsctPtsWithAnotherBeam = new Dictionary<ObjectId, List<Tuple<ObjectId, (Point3d, Point3d)>>>();//Changes made on 08/04/2023 by SDM
        public static Dictionary<ObjectId, List<(Point3d, Point3d)>> listOfBeamId_AllBASidePanelsPts = new Dictionary<ObjectId, List<(Point3d, Point3d)>>();//Changes made on 11/04/2023 by SDM

        public void clearListOfBeamHelper()
        {
            dicBeamIntersectModifiedPoints.Clear();
            listOfListOfBeamBottomCornerId.Clear();
            listOfListOfBeamlvls.Clear();
            listOfListOfBeamCornerMaxLngthLineId.Clear();
            BeamHelper.listOfLinesBtwTwoCrners_BeamInsctName.Clear();
            BeamHelper.listOfLinesBtwTwoCrners_BeamInsctId.Clear();
            BeamHelper.listOfNearestBtwWallToBeamWallLineId.Clear();
            BeamHelper.listOfDistanceBtwWallToBeam.Clear();
            BeamHelper.listOfBeamName_InsctToDoorWall.Clear();
            BeamHelper.listOfInsctDoor_BeamInsctId.Clear();
            BeamHelper.listOfDistanceBtwDoorToBeam.Clear();
            BeamHelper.listOfNearestBtwDoorToBeamBeamWallLineId.Clear();
            BeamHelper.listOfTwoBeamInsctPoint.Clear();
            BeamHelper.listOfBeamLineId_ForMoveInBeamLayout.Clear();
            BeamHelper.listOfWallPanelLineId_WithBeamDoor.Clear();
            BeamHelper.listOfAllBeamCornerId_ForStretch.Clear();
            BeamHelper.listOfAllBeamOffstCornerId_ForStretch.Clear();
            BeamHelper.listOfAllBeamCornerTextId_ForStretch.Clear();
            BeamHelper.listOfAllWallCornerId_WithBeam.Clear();
            BeamHelper.listOfAllStretchBeamCornerId.Clear();

            BeamHelper.listOfBeamName_InsctToWindowWall.Clear();
            BeamHelper.listOfInsctWindow_BeamInsctId.Clear();
            BeamHelper.listOfDistanceBtwWindowToBeam.Clear();
            BeamHelper.listOfNearestBtwWindowToBeamWallLineId.Clear();
            BeamHelper.listOfBeamBottomPanelLineId.Clear();
            BeamHelper.listOfBeamId_ForBeamBotomPanel.Clear();
            BeamHelper.listOfWallId_ForBeamBotomPanel.Clear();
            BeamHelper.listOfDistBtwBeamPanelLineToWallLine.Clear();
            BeamHelper.listOfListOfBeamSidePanelLineId.Clear();
            BeamHelper.listOfBeamSideWallId.Clear();
            BeamHelper.listOfSunkanSlabObjId.Clear();
            BeamHelper.listOfListOfBeamBottomWallPanelId.Clear();
            BeamHelper.listOfListOfBeamBottomPanelData.Clear();
            BeamHelper.listOfListOfBeamCornerMaxLngthLineId.Clear();
            BeamHelper.listOfAllBeamBottomCornerId.Clear();

            listOfListOfBeamBottomPanelLineId.Clear();
            listOfListOfDistBtwBeamPanelLineToWallLine.Clear();
            listOfListOfBeamId_ForBeamBotomPanel.Clear();
            listOfListOfWallId_ForBeamBotomPanel.Clear();
            listOfBeamId_BottomPanelLineDrawn.Clear();//Changes made on 02/04/2023 by SDM

            listOfAllOffsetBeamCornerIdsWithBeamID.Clear();

            listOfBeamBottomExternalWallPanelObj.Clear();
            listOfBeamBottomWallPanelObj.Clear();

            //Added on 12/03/2023 by SDM
            listOfIntersectBeamObjId.Clear();
            listOfIntersectBeamId_ForTrimLineId.Clear();
            listOfListIntersectBeam_Points.Clear();
            listOfListIntersectBeam_LineId.Clear();
            listOfBeamObjId.Clear();
            listOfBeamObjId_Str.Clear();
            listBeamLines.Clear();
            listOfExistBeamPanelLineObj.Clear();
            listOfDoorObjId_With_BeamPanelLine.Clear();

            listOfBeamId_AllInsctPtsWithAnotherBeam.Clear();//Changes made on 08/04/2023 by SDM for "Beam"
            listOfBeamId_AllBASidePanelsPts.Clear();//Changes made on 11/04/2023 by SDM for "Beam"
        }


        public void CheckIntersections(List<ObjectId> lstIds)
        {
            //foreach (ObjectId id in lstIds)
            //{
            //    var wallLineE = AEE_Utility.GetEntityForRead(id);
            //    if (!(wallLineE is Line))
            //    {
            //        continue;
            //    }
            //    var wallLine = wallLineE as Line;
            //    var wLSeg = new LineSegment3d(wallLine.StartPoint, wallLine.EndPoint);
            //    for (int index = 0; index < CheckShellPlanHelper.listOfSelectedBeam_ObjId.Count; index++)
            //    {
            //        ObjectId beamId = CheckShellPlanHelper.listOfSelectedBeam_ObjId[index];
            //        var lst = AEE_Utility.GetSegmentsFromPolyline(beamId);
            //        lst.Sort(delegate (LineSegment3d c1, LineSegment3d c2)
            //        {
            //            if (c1.Length > c2.Length)
            //                return 1;
            //            if (c1.Length < c2.Length)
            //                return -1;
            //            return 0;
            //        });
            //        if (lst.Last().Direction.IsParallelTo((wallLine.EndPoint - wallLine.StartPoint)))
            //        {
            //            continue;
            //        }

            //        foreach (var l in lst.Skip(2))
            //        {
            //            var ptsInter = l.IntersectWith(wLSeg);
            //            if (ptsInter == null || !ptsInter.Any())
            //            {
            //                continue;
            //            }
            //            var t = 1e-6;
            //            var tol = new Tolerance(t, t);
            //            if (ptsInter[0].IsEqualTo(wLSeg.EndPoint, tol) ||
            //                ptsInter[0].IsEqualTo(wLSeg.StartPoint, tol))
            //            {
            //                continue;
            //            }
            //            if (wLSeg.EndPoint.DistanceTo(ptsInter[0]) < wLSeg.StartPoint.DistanceTo(ptsInter[0]))
            //            {
            //                dicBeamIntersectModifiedPoints[wLSeg.EndPoint] = ptsInter[0];
            //            }
            //            else
            //            {
            //                dicBeamIntersectModifiedPoints[wLSeg.StartPoint] = ptsInter[0];
            //            }
            //        }
            //    }
            //}
            List<BoundBlock3d> lstBlks = new List<BoundBlock3d>();
            foreach (var item in CheckShellPlanHelper.listOfSelectedOffsetBeam_ObjId)
            {
                lstBlks.Add(null);
            }
            while (true)
            {
                List<ObjectId> lstFinalIds = new List<ObjectId>();
                foreach (ObjectId id in lstIds)
                {
                    var wallLineE = AEE_Utility.GetEntityForRead(id);
                    if (!(wallLineE is Line))
                    {
                        continue;
                    }
                    var hasIntersect = false;
                    var wallLine = wallLineE as Line;
                    var lstPts = isLineBetweenCorners(wallLine);
                    if (!lstPts.Any())
                    {
                        lstFinalIds.Add(id);
                        continue;
                    }
                    var wLSeg = new LineSegment3d(wallLine.StartPoint, wallLine.EndPoint);
                    for (int index = 0; index < CheckShellPlanHelper.listOfSelectedOffsetBeam_ObjId.Count; index++)
                    {
                        ObjectId beamId = CheckShellPlanHelper.listOfSelectedOffsetBeam_ObjId[index];
                        var lst = AEE_Utility.GetSegmentsFromPolyline(beamId);
                        if (lstBlks[index] == null)
                        {
                            if (lst.Any())
                            {
                                BoundBlock3d blk = new BoundBlock3d();
                                blk.Set(lst[0].StartPoint, lst[0].EndPoint);
                                for (var b = 1; b < lst.Count; ++b)
                                {
                                    blk.Extend(lst[b].StartPoint);
                                    blk.Extend(lst[b].EndPoint);
                                }
                                lstBlks[index] = blk;
                            }
                        }

                        if (lstBlks[index] != null)
                        {
                            if (!lstPts.Where(o => lstBlks[index].Contains(o)).Any())
                            {
                                continue;
                            }

                        }
                        lst.Sort(delegate (LineSegment3d c1, LineSegment3d c2)
                        {
                            if (c1.Length > c2.Length)
                                return 1;
                            if (c1.Length < c2.Length)
                                return -1;
                            return 0;
                        });
                        if (lst.Last().Direction.IsParallelTo((wallLine.EndPoint - wallLine.StartPoint)))
                        {
                            continue;
                        }

                        foreach (var l in lst.Skip(2))
                        {
                            var ptsInter = l.IntersectWith(wLSeg);
                            if (ptsInter == null || !ptsInter.Any())
                            {
                                continue;
                            }
                            var t = 1e-6;
                            var tol = new Tolerance(t, t);
                            if (ptsInter[0].IsEqualTo(wLSeg.EndPoint, tol) ||
                                ptsInter[0].IsEqualTo(wLSeg.StartPoint, tol))
                            {
                                continue;
                            }
                            hasIntersect = true;
                            lstFinalIds.Add(AEE_Utility.CreateLine(wLSeg.StartPoint, ptsInter[0], wallLine.Layer, wallLine.ColorIndex));
                            lstFinalIds.Add(AEE_Utility.CreateLine(ptsInter[0], wLSeg.EndPoint, wallLine.Layer, wallLine.ColorIndex));
                            if (wLSeg.EndPoint.DistanceTo(ptsInter[0]) < wLSeg.StartPoint.DistanceTo(ptsInter[0]))
                            {
                                dicBeamIntersectModifiedPoints[wLSeg.EndPoint] = ptsInter[0];
                            }
                            else
                            {
                                dicBeamIntersectModifiedPoints[wLSeg.StartPoint] = ptsInter[0];
                            }
                        }
                    }
                    if (!hasIntersect)
                    {
                        lstFinalIds.Add(id);
                    }
                }
                if (lstFinalIds.Count == lstIds.Count)
                {
                    break;
                }
                lstIds.Clear();
                lstIds.AddRange(lstFinalIds);
            }

            // Remove duplicate or same start and end point lines

            List<ObjectId> removeObjects = new List<ObjectId>();
            foreach (ObjectId l1 in lstIds)
            {
                foreach (ObjectId l2 in lstIds)
                {
                    if (l1 == l2 || removeObjects.Contains(l1))
                        continue;


                    if (AEE_Utility.IsLinesAreSame(l1, l2))
                    {
                        removeObjects.Add(l2);
                    }
                }
            }

            foreach (ObjectId lineobj in removeObjects)
            {
                lstIds.Remove(lineobj);
                AEE_Utility.deleteEntity(lineobj);
            }

        }

        private List<Point3d> isLineBetweenCorners(Line wallLine)
        {
            var t = 1e-6;
            var tol = new Tolerance(t, t);
            List<Point3d> lstCorners = new List<Point3d>();
            foreach (var corner in InternalWallHelper.lstCornerData)
            {
                if (DoorHelper.listOfMoveCornerIds_In_New_ShellPlan.Contains(corner.idCorner))
                {
                    continue;
                }
                if (corner.EndPoint1.IsEqualTo(wallLine.StartPoint, tol))
                {
                    lstCorners.Add(corner.EndPoint1 + (corner.EndPoint2 - corner.EndPoint1) / 2);
                }
                if (corner.EndPoint2.IsEqualTo(wallLine.StartPoint, tol))
                {
                    lstCorners.Add(corner.EndPoint1 + (corner.EndPoint2 - corner.EndPoint1) / 2);
                }
                if (corner.EndPoint1.IsEqualTo(wallLine.EndPoint, tol))
                {
                    lstCorners.Add(corner.EndPoint1 + (corner.EndPoint2 - corner.EndPoint1) / 2);
                }
                if (corner.EndPoint2.IsEqualTo(wallLine.EndPoint, tol))
                {
                    lstCorners.Add(corner.EndPoint1 + (corner.EndPoint2 - corner.EndPoint1) / 2);
                }
            }
            return lstCorners;
        }

        public string getBeamName_With_WallPanelLineIsInsideTheBeam(ObjectId wall_ObjId, ObjectId wallLineId, ObjectId crrntWallLineId, out ObjectId beamId_InsctWall, out ObjectId nearestWallToBeamWallId, out double distBtwWallToBeam, out List<Point3d> listOfAllInsctPointOfTwoBeam, out ObjectId adjacentOfBeamId, out Point3d adjacentInsidePointOfBeam)
        {
            flagOfAdjacentWallLine_ToInsctToBeam = false;
            adjacentInsidePointOfBeam = new Point3d();
            adjacentOfBeamId = new ObjectId();

            listOfAllInsctPointOfTwoBeam = new List<Point3d>();

            distBtwWallToBeam = 0;
            nearestWallToBeamWallId = new ObjectId();
            string beamLayerNameInsct = "";
            beamId_InsctWall = new ObjectId();
            try
            {
                if (wallLineId.IsValid == true)
                {
                    var offsetInPost_Dir = AEE_Utility.OffsetEntity(WindowHelper.windowOffsetValue, wallLineId, false);
                    var offsetInNegt_Dir = AEE_Utility.OffsetEntity(-WindowHelper.windowOffsetValue, wallLineId, false);

                    var midPointOfWallLine = AEE_Utility.GetMidPoint(wallLineId);
                    var midPointOfWallLine_Post_Dir = AEE_Utility.GetMidPoint(offsetInPost_Dir);
                    var midPointOfWallLine_Negt_Dir = AEE_Utility.GetMidPoint(offsetInNegt_Dir);

                    for (int index = 0; index < CheckShellPlanHelper.listOfSelectedOffsetBeam_ObjId.Count; index++)
                    {
                        ObjectId beamId = CheckShellPlanHelper.listOfSelectedOffsetBeam_ObjId[index];

                        var beamEntity = AEE_Utility.GetEntityForRead(beamId);
                        bool pointIsInsideTheBeam = AEE_Utility.GetPointIsInsideThePolyline(beamId, midPointOfWallLine);
                        bool distBtwTwoLine = checkDistanceBetweenBeamAndWallPanelLines(wall_ObjId, beamId, wallLineId, pointIsInsideTheBeam, out nearestWallToBeamWallId, out distBtwWallToBeam, out listOfAllInsctPointOfTwoBeam);
                        if (pointIsInsideTheBeam == false)
                        {
                            pointIsInsideTheBeam = AEE_Utility.GetPointIsInsideThePolyline(beamId, midPointOfWallLine_Post_Dir);
                            distBtwTwoLine = checkDistanceBetweenBeamAndWallPanelLines(wall_ObjId, beamId, offsetInPost_Dir, pointIsInsideTheBeam, out nearestWallToBeamWallId, out distBtwWallToBeam, out listOfAllInsctPointOfTwoBeam);
                        }
                        if (pointIsInsideTheBeam == false)
                        {
                            pointIsInsideTheBeam = AEE_Utility.GetPointIsInsideThePolyline(beamId, midPointOfWallLine_Negt_Dir);
                            distBtwTwoLine = checkDistanceBetweenBeamAndWallPanelLines(wall_ObjId, beamId, offsetInNegt_Dir, pointIsInsideTheBeam, out nearestWallToBeamWallId, out distBtwWallToBeam, out listOfAllInsctPointOfTwoBeam);
                        }
                        if (pointIsInsideTheBeam == true && distBtwTwoLine == true)
                        {
                            beamLayerNameInsct = beamEntity.Layer;
                            beamId_InsctWall = beamId;
                        }

                        if (flagOfAdjacentWallLine_ToInsctToBeam == false && pointIsInsideTheBeam == true)
                        {
                            checkBeamIsIntersectToWall(wall_ObjId, wallLineId, crrntWallLineId, beamId, out adjacentOfBeamId, out adjacentInsidePointOfBeam);
                        }

                        if (pointIsInsideTheBeam == true)
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return beamLayerNameInsct;
        }


        private bool checkDistanceBetweenBeamAndWallPanelLines(ObjectId wall_ObjId, ObjectId beamId, ObjectId wallLineId, bool pointIsInsideTheBeam, out ObjectId nearestWallToBeamWallId, out double distBtwWallToBeam, out List<Point3d> listOfAllInsctPointOfTwoBeam)
        {
            listOfAllInsctPointOfTwoBeam = new List<Point3d>();
            distBtwWallToBeam = 0;
            nearestWallToBeamWallId = new ObjectId();

            if (pointIsInsideTheBeam == true)
            {
                WallPanelHelper wallPanelHlp = new WallPanelHelper();
                List<double> listDifference = new List<double>();
                List<ObjectId> listBeamLineId = new List<ObjectId>();
                string beamLayer = AEE_Utility.GetLayerName(beamId);//Changes made on 13/03/2023 by SDM
                string data = AEE_Utility.GetXDataRegisterAppName(wallLineId);
                var listOfBeamExplode = AEE_Utility.ExplodeEntity(beamId);
                foreach (var beamLineId in listOfBeamExplode)
                {
                    var flag = WallPanelHelper.checkAngleOfBaseLine(beamLineId, wallLineId);
                    if (flag == true)
                    {
                        var listOfInsctPoint = AEE_Utility.InterSectionBetweenTwoEntity(wall_ObjId, beamLineId);
                        if (listOfInsctPoint.Count != 0)
                        {
                            var diff = AEE_Utility.GetDistanceBetweenTwoLines(beamLineId, wallLineId);
                            listDifference.Add(diff);
                            listBeamLineId.Add(beamLineId);
                        }
                    }
                }

                if (listDifference.Count != 0)
                {
                    double minDiff = listDifference.Min();
                    //Changes made on 12/03/2023 by SDM for "Beam" 
                    if (minDiff <= WindowHelper.windowOffsetValue && beamLayer.Contains(Beam_UI_Helper.offsetBeamStrtText))
                    {
                        return false;
                    }
                    else
                    {
                        distBtwWallToBeam = minDiff;
                        int indexOfMin = listDifference.IndexOf(minDiff);
                        nearestWallToBeamWallId = AEE_Utility.createColonEntityInSamePoint(listBeamLineId[indexOfMin], false);

                        checkBeamIsIntersectToAnotherBeamNew(wall_ObjId, beamId, nearestWallToBeamWallId, distBtwWallToBeam, CheckShellPlanHelper.listOfSelectedOffsetBeam_ObjId, out listOfAllInsctPointOfTwoBeam);

                        //Changes made on 08/04/2023 by SDM for "Beam"
                        checkBeamIsIntersectToAnotherBeam2(wall_ObjId, beamId, nearestWallToBeamWallId, CheckShellPlanHelper.listOfSelectedOffsetBeam_ObjId);

                        return true;
                    }
                }
                AEE_Utility.deleteEntity(listOfBeamExplode);
            }
            return false;
        }
        private void checkBeamIsIntersectToAnotherBeam(ObjectId wall_ObjId, ObjectId crrntBeamId, ObjectId crrntBeamWallLineId, double distBtwWallToBeam, List<ObjectId> listOfAllBeamId, out List<Point3d> listOfAllInsctPointOfTwoBeam)
        {
            listOfAllInsctPointOfTwoBeam = new List<Point3d>();

            double offsetValue = 0;
            var listOfCrrntBeamVertices = AEE_Utility.GetPolylineVertexPoint(crrntBeamId);
            double crrntBeam_MaxX = listOfCrrntBeamVertices.Max(sortPoint => sortPoint.X);
            double crrntBeam_MaxY = listOfCrrntBeamVertices.Max(sortPoint => sortPoint.Y);
            Point3d offsetPoint = new Point3d((offsetValue + crrntBeam_MaxX), (offsetValue + crrntBeam_MaxY), 0);
            var offsetCrrntBeamId = AEE_Utility.OffsetEntity_WithoutLine(offsetValue, offsetPoint, crrntBeamId, false);

            for (int i = 0; i < listOfAllBeamId.Count; i++)
            {
                ObjectId beamId = listOfAllBeamId[i];
                if (beamId != crrntBeamId)
                {
                    var listOfBeamVertices = AEE_Utility.GetPolylineVertexPoint(beamId);
                    double beam_MaxX = listOfBeamVertices.Max(sortPoint => sortPoint.X);
                    double beam_MaxY = listOfBeamVertices.Max(sortPoint => sortPoint.Y);
                    Point3d beamOffsetPoint = new Point3d((offsetValue + beam_MaxX), (offsetValue + beam_MaxY), 0);
                    var offsetBeamId = AEE_Utility.OffsetEntity_WithoutLine(offsetValue, beamOffsetPoint, beamId, false);

                    var listOfInsctPoint = AEE_Utility.InterSectionBetweenTwoEntity(offsetCrrntBeamId, offsetBeamId);
                    if (listOfInsctPoint.Count != 0)
                    {
                        //AEE_Utility.DCircle(listOfBeamVertices[3][0], listOfBeamVertices[3][1]);

                        //AEE_Utility.DCircle(AEE_Utility.GetLine(crrntBeamWallLineId).EndPoint);
                        for (int k = 0; k < listOfInsctPoint.Count; k++)
                        {
                            Point3d insctPoint = listOfInsctPoint[k];
                            bool flagOfBeamInsctToAnotherBeam = AEE_Utility.GetPointIsInsideThePolyline(wall_ObjId, insctPoint);
                            if (flagOfBeamInsctToAnotherBeam == true)
                            {
                                listOfAllInsctPointOfTwoBeam.Add(insctPoint);
                                break;
                            }
                        }
                    }
                    AEE_Utility.deleteEntity(offsetBeamId);
                }
            }

            AEE_Utility.deleteEntity(offsetCrrntBeamId);
        }

        private void checkBeamIsIntersectToAnotherBeamNew(ObjectId wall_ObjId, ObjectId crrntBeamId, ObjectId crrntBeamWallLineId, double distBtwWallToBeam, List<ObjectId> listOfAllBeamId, out List<Point3d> listOfAllInsctPointOfTwoBeam)
        {
            listOfAllInsctPointOfTwoBeam = new List<Point3d>();
            WallPanelHelper wallPanelHlp = new WallPanelHelper();

            double offsetValue = 0;
            var listOfCrrntBeamVertices = AEE_Utility.GetPolylineVertexPoint(crrntBeamId);
            double crrntBeam_MaxX = listOfCrrntBeamVertices.Max(sortPoint => sortPoint.X);
            double crrntBeam_MaxY = listOfCrrntBeamVertices.Max(sortPoint => sortPoint.Y);
            Point3d offsetPoint = new Point3d((offsetValue + crrntBeam_MaxX), (offsetValue + crrntBeam_MaxY), 0);
            var offsetCrrntBeamId = AEE_Utility.OffsetEntity_WithoutLine(offsetValue, offsetPoint, crrntBeamId, false);

            for (int i = 0; i < listOfAllBeamId.Count; i++)
            {
                ObjectId beamId = listOfAllBeamId[i];
                if (beamId != crrntBeamId)
                {
                    var listOfBeamVertices = AEE_Utility.GetPolylineVertexPoint(beamId);
                    double beam_MaxX = listOfBeamVertices.Max(sortPoint => sortPoint.X);
                    double beam_MaxY = listOfBeamVertices.Max(sortPoint => sortPoint.Y);
                    Point3d beamOffsetPoint = new Point3d((offsetValue + beam_MaxX), (offsetValue + beam_MaxY), 0);
                    var offsetBeamId = AEE_Utility.OffsetEntity_WithoutLine(offsetValue, beamOffsetPoint, beamId, false);

                    List<Point3d> listOfInsctPoint;

                    List<ObjectId> offsetBeamExplodedObj = AEE_Utility.ExplodeEntity(beamId);
                    for (int j = 0; j < offsetBeamExplodedObj.Count; j++)
                    {
                        ObjectId offsetBeamLineObj = offsetBeamExplodedObj[j];

                        var flag = WallPanelHelper.checkAngleOfBaseLine(crrntBeamWallLineId, offsetBeamLineObj);

                        if (!flag)
                        {
                            //listOfInsctPoint = AEE_Utility.GetIntrsctPntOfBtwnTwoLines_WithExtendBoth(crrntBeamWallLineId, offsetBeamLineObj);
                            listOfInsctPoint = AEE_Utility.InterSectionBetweenTwoEntity(crrntBeamWallLineId, offsetBeamLineObj);
                            if (listOfInsctPoint.Count != 0)
                            {
                                Point3d insctPoint = listOfInsctPoint[0];
                                bool flagOfBeamInsctToAnotherBeam = AEE_Utility.GetPointIsInsideThePolyline(wall_ObjId, insctPoint);

                                ////double lengthOffsetBeamLine = Math.Round(AEE_Utility.GetLengthOfLine(offsetBeamLineObj));

                                ////List<Point3d> listOffsetBeamLineStrtAndEndPoints = AEE_Utility.GetStartEndPointOfLine(offsetBeamLineObj);
                                ////bool intersectPtIsMatchToLinePoints = true;

                                ////List<double> distFromInsctPtToLineEndpts = new List<double> { 0, lengthOffsetBeamLine };

                                ////for (int m = 0; m < listOffsetBeamLineStrtAndEndPoints.Count; m++)
                                ////{
                                ////    double len = Math.Round(AEE_Utility.GetLengthOfLine(listOffsetBeamLineStrtAndEndPoints[m], insctPoint));
                                ////    intersectPtIsMatchToLinePoints = distFromInsctPtToLineEndpts.Contains(len);
                                ////    if (!intersectPtIsMatchToLinePoints)
                                ////        break;
                                ////}

                                //if (flagOfBeamInsctToAnotherBeam == true && intersectPtIsMatchToLinePoints == true)
                                if (flagOfBeamInsctToAnotherBeam == true)
                                {
                                    listOfAllInsctPointOfTwoBeam.Add(insctPoint);
                                    break;
                                }
                            }
                        }
                    }

                    AEE_Utility.deleteEntity(offsetBeamExplodedObj);
                    AEE_Utility.deleteEntity(offsetBeamId);
                }
            }

            AEE_Utility.deleteEntity(offsetCrrntBeamId);
        }

        //Added made on 08/04/2023 by SDM for "Beam"
        private void checkBeamIsIntersectToAnotherBeam2(ObjectId wall_ObjId, ObjectId crrntBeamId, ObjectId crrntBeamWallLineId, List<ObjectId> listOfAllBeamId)
        {

            for (int i = 0; i < listOfAllBeamId.Count; i++)
            {
                ObjectId beamId = listOfAllBeamId[i];
                if (beamId != crrntBeamId)
                {

                    List<Point3d> listOfInsctPoint;
                    listOfInsctPoint = AEE_Utility.InterSectionBetweenTwoEntity(crrntBeamId, beamId);
                    if (listOfInsctPoint.Count != 0)
                    {
                        var pt1 = listOfInsctPoint[0];
                        var pt2 = listOfInsctPoint.Count == 2 ? listOfInsctPoint[1] : new Point3d();

                        if (!listOfBeamId_AllInsctPtsWithAnotherBeam.ContainsKey(crrntBeamId))
                            listOfBeamId_AllInsctPtsWithAnotherBeam.Add(crrntBeamId, new List<Tuple<ObjectId, (Point3d, Point3d)>> { new Tuple<ObjectId, (Point3d, Point3d)>(beamId, (pt1, pt2)) });
                        else if (listOfBeamId_AllInsctPtsWithAnotherBeam.ContainsKey(crrntBeamId) && !listOfBeamId_AllInsctPtsWithAnotherBeam[crrntBeamId].Any(X => X.Item1 == beamId))
                            listOfBeamId_AllInsctPtsWithAnotherBeam[crrntBeamId].AddRange(new List<Tuple<ObjectId, (Point3d, Point3d)>> { new Tuple<ObjectId, (Point3d, Point3d)>(beamId, (pt1, pt2)) });
                    }

                }
            }

        }

        private void checkBeamIsIntersectToWall(ObjectId wall_ObjId, ObjectId wallLineId, ObjectId crrntWallLineId, ObjectId beamId, out ObjectId adjacenttOfBeamId, out Point3d adjacentInsidePointOfBeam)
        {
            adjacentInsidePointOfBeam = new Point3d();
            adjacenttOfBeamId = new ObjectId();

            var listOfStrtEndPoint = AEE_Utility.GetStartEndPointOfLine(crrntWallLineId);
            for (int k = 0; k < listOfStrtEndPoint.Count; k++)
            {
                Point3d point = listOfStrtEndPoint[k];
                var flagOfInside = AEE_Utility.GetPointIsInsideThePolyline(beamId, point);
                if (flagOfInside == true)
                {
                    flagOfAdjacentWallLine_ToInsctToBeam = true;
                    adjacentInsidePointOfBeam = point;
                    adjacenttOfBeamId = beamId;
                    break;
                }
            }
        }

        public bool CheckAnyVerticesPointOfCornerIdIsInsidePolyLineObj(ObjectId cornerId, ObjectId polylineObj, bool offset)
        {
            bool flagOfInside = false;

            ObjectId offsetCrnrId = cornerId;

            if (offset)
            {
                offsetCrnrId = AEE_Utility.OffsetEntity(1, cornerId, true);
            }

            List<Point3d> listOfCornerVertexPts = AEE_Utility.GetPolyLinePointList(offsetCrnrId);

            for (int h = 0; h < listOfCornerVertexPts.Count; h++)
            {
                flagOfInside = AEE_Utility.GetPointIsInsideThePolyline(polylineObj, listOfCornerVertexPts[h]);
                if (flagOfInside == true)
                {
                    break;
                }
            }
            if (offset)
                AEE_Utility.deleteEntity(offsetCrnrId);

            return flagOfInside;
        }
        public void setBeamLine_ForCorner(string xDataRegAppNameWallLine, ObjectId wall_ObjId, List<Point3d> listOfAllInsctPointOfTwoBeam, ObjectId nearestWallToBeamWallId,
                                          double distBtwWallToBeam, ObjectId wallLineId, ObjectId prvsWallLineId, ObjectId wallCornerId1, ObjectId wallCornerId2, ObjectId beamId_InsctWall,
                                          string beamLayerNameInsct, Point3d adjacentInsidePointOfBeam, ObjectId adjacentOfBeamId,
                                          bool flagOfSunkanSlab, ObjectId sunkanSlabId, out ObjectId beamCornerId1, out ObjectId beamCornerId2)
        {
            beamCornerId1 = new ObjectId();
            beamCornerId2 = new ObjectId();

            if (beamId_InsctWall.IsValid == true)
            {
                setBeamPanelThickLineData(xDataRegAppNameWallLine, wall_ObjId, wallLineId, prvsWallLineId, wallCornerId1, wallCornerId2, beamId_InsctWall, distBtwWallToBeam, adjacentInsidePointOfBeam, adjacentOfBeamId);

                //Changes made on 13/03/2023 by SDM
                double heightOfBeamSidePanel = 0;
                double beamBottom = 0;
                if (beamLayerNameInsct.Contains(Beam_UI_Helper.offsetBeamStrtText))
                {
                    beamBottom = Beam_UI_Helper.getOffsetBeamBottom(beamLayerNameInsct);
                    heightOfBeamSidePanel = GeometryHelper.getHeightOfBeamSidePanel(InternalWallSlab_UI_Helper.getSlabBottom(AEE_Utility.GetLayerName(wallLineId)), beamBottom, PanelLayout_UI.SL, sunkanSlabId.IsValid, sunkanSlabId);
                }
                else if (beamLayerNameInsct.Contains(Beam_UI_Helper.beamStrtText))
                {
                    heightOfBeamSidePanel = Beam_UI_Helper.getBeamSidePanelHeight(beamLayerNameInsct, AEE_Utility.GetLayerName(wallLineId));
                    beamBottom = Beam_UI_Helper.GetBeamLintelLevel(beamLayerNameInsct, xDataRegAppNameWallLine);
                }
                //-------------------

                if (heightOfBeamSidePanel > 0)
                {
                    double angleOfLine = AEE_Utility.GetAngleOfLine(wallLineId);
                    double crnerRotationAngle = angleOfLine + CommonModule.cornerRotationAngle;

                    var basePointOfCorner1 = AEE_Utility.GetBasePointOfPolyline(wallCornerId1);
                    var basePointOfCorner2 = AEE_Utility.GetBasePointOfPolyline(wallCornerId2);
                    var lineId = AEE_Utility.getLineId(basePointOfCorner1.X, basePointOfCorner1.Y, 0, basePointOfCorner2.X, basePointOfCorner2.Y, 0, false);

                    var offsetLineId = AEE_Utility.OffsetLine(lineId, distBtwWallToBeam, false);
                    double distance = nearestWallToBeamWallId.IsValid ? AEE_Utility.GetDistanceBetweenTwoLines(nearestWallToBeamWallId, offsetLineId) : 0;

                    if (distance > distBtwWallToBeam)
                    {
                        AEE_Utility.deleteEntity(offsetLineId);
                        offsetLineId = AEE_Utility.OffsetLine(lineId, -distBtwWallToBeam, false);
                    }

                    var listOfStrtEndPoint = AEE_Utility.GetStartEndPointOfLine(offsetLineId);
                    Line offLn = AEE_Utility.GetLine(offsetLineId);

                    List<double> listOfLengthCorner1 = new List<double>();
                    List<double> listOfLengthCorner2 = new List<double>();

                    listOfLengthCorner1.Add(offLn.StartPoint.DistanceTo(offLn.GetClosestPointTo(basePointOfCorner1, false)));
                    listOfLengthCorner2.Add(offLn.StartPoint.DistanceTo(offLn.GetClosestPointTo(basePointOfCorner2, false)));
                    listOfLengthCorner1.Add(offLn.EndPoint.DistanceTo(offLn.GetClosestPointTo(basePointOfCorner1, false)));
                    listOfLengthCorner2.Add(offLn.EndPoint.DistanceTo(offLn.GetClosestPointTo(basePointOfCorner2, false)));

                    /*foreach (var point in listOfStrtEndPoint)
                    {
                        var length1 = AEE_Utility.GetLengthOfLine(point.X, point.Y, basePointOfCorner1.X, basePointOfCorner1.Y);
                        listOfLengthCorner1.Add(length1);

                        var length2 = AEE_Utility.GetLengthOfLine(point.X, point.Y, basePointOfCorner2.X, basePointOfCorner2.Y);
                        listOfLengthCorner2.Add(length2);
                    }*/

                    List<Point2d> pnts = AEE_Utility.GetPolylineVertexPoint(wall_ObjId);

                    CornerHelper crnerHlpr = new CornerHelper();
                    string beamInternalCornerText = CommonModule.beamInternalCornerText;
                    string cornerText = CornerHelper.getCornerText(beamInternalCornerText, CommonModule.intrnlCornr_Flange1, CommonModule.intrnlCornr_Flange2, heightOfBeamSidePanel);

                    List<ObjectId> listOfBeamCornerMoveId = new List<ObjectId>();
                    List<ObjectId> listOfBeamCornerTextMoveId = new List<ObjectId>();

                    ObjectId newCorner1TextId = new ObjectId();
                    ObjectId newCorner2TextId = new ObjectId();
                    List<ObjectId> listOfBeamCornerId = new List<ObjectId>();
                    List<Point3d> listOfBeamCornerBasePoint = drawCornerBtwTwoBeam(xDataRegAppNameWallLine, wallCornerId1, wallCornerId2, listOfAllInsctPointOfTwoBeam, cornerText, crnerRotationAngle, heightOfBeamSidePanel, distBtwWallToBeam, beamBottom, out beamCornerId1, out beamCornerId2, out newCorner1TextId, out newCorner2TextId, out listOfBeamCornerId);

                    if (beamCornerId1.IsValid == false)
                    {
                        double minLengthCorner1 = listOfLengthCorner1.Min();
                        int indexOfMinLengthCorner1 = listOfLengthCorner1.IndexOf(minLengthCorner1);
                        Point3d insertionPointCorner1 = listOfStrtEndPoint[indexOfMinLengthCorner1];

                        Vector3d copyPasteVector_Corner1 = new Vector3d((insertionPointCorner1.X - basePointOfCorner1.X), (insertionPointCorner1.Y - basePointOfCorner1.Y), 0);
                        var beamCrnrId1 = AEE_Utility.copyColonEntity(copyPasteVector_Corner1, wallCornerId1);

                        bool flagInsideBeam = CheckAnyVerticesPointOfCornerIdIsInsidePolyLineObj(beamCrnrId1, beamId_InsctWall, true);
                        if (flagInsideBeam)
                        {

                            listOfBeamCornerMoveId.Add(beamCrnrId1);

                            var beamCornerId1_Text = crnerHlpr.writeTextInCorner(beamCrnrId1, cornerText, crnerRotationAngle, CommonModule.internalCornerTextLyrName, CommonModule.internalCornerLyrColor);
                            listOfBeamCornerTextMoveId.Add(beamCornerId1_Text);

                            listOfBeamCornerBasePoint.Add(AEE_Utility.GetBasePointOfPolyline(beamCrnrId1));
                            listOfBeamCornerId.Add(beamCrnrId1);

                            string w1 = AEE_Utility.GetXDataRegisterAppName(wallCornerId1);
                            string w2 = AEE_Utility.GetXDataRegisterAppName(wallCornerId2);
                            //Fix BIAX 2nd position by SDM 12-06-2022
                            // CornerElevation elev = new CornerElevation(w1, w2, distBtwWallToBeam, listOfLengthCorner1[0], beamBottom);
                            //start
                            double dist1 = 0;
                            double dist2 = 0;
                            double wall1length = 0;
                            double wall2length = 0;
                            int wall1idx = 0;
                            int wall2idx = 0;
                            var walllines = InternalWallHelper.listOfListOfInternalWallRoomLineId_With_WallName;
                            for (int r = 0; r < walllines.Count; r++)
                            {
                                for (int w = 0; w < walllines[r].Count; w++)
                                {
                                    var name = AEE_Utility.GetXDataRegisterAppName(walllines[r][w]);

                                    if (name == w2)
                                    {
                                        var wall = AEE_Utility.GetLine(walllines[r][w]);
                                        wall2length = wall.Length;
                                        wall2idx = w;
                                    }
                                    if (name == w1)
                                    {
                                        wall1length = AEE_Utility.GetLengthOfLine(walllines[r][w]);
                                        wall1idx = w;
                                    }
                                }
                            }
                            if (w1 == xDataRegAppNameWallLine)
                            {
                                if (wall1idx < wall2idx)
                                {

                                    dist1 = wall1length - CommonModule.intrnlCornr_Flange1;
                                    dist2 = distBtwWallToBeam;
                                }
                                else
                                {
                                    bool flag_btwnTowBeams = false;
                                    for (int i = 0; i < listOfBeamCornerBasePoint.Count; i++)
                                    {
                                        for (int j = 0; j < listOfAllInsctPointOfTwoBeam.Count; j++)
                                        {
                                            if (listOfAllInsctPointOfTwoBeam[j] == listOfBeamCornerBasePoint[i])
                                                flag_btwnTowBeams = true;
                                        }
                                    }

                                    dist1 = flag_btwnTowBeams ? distBtwWallToBeam : 0;
                                    dist2 = wall2length - CommonModule.intrnlCornr_Flange2 - distBtwWallToBeam;
                                }
                            }
                            else if (w2 == xDataRegAppNameWallLine)
                            {
                                dist1 = wall1length - CommonModule.intrnlCornr_Flange1 - distBtwWallToBeam;
                                dist2 = 0;
                            }



                            CornerElevation elev = new CornerElevation(w1, w2, dist1, dist2, beamBottom);
                            //end
                            CornerHelper.setCornerDataForBOM(xDataRegAppNameWallLine, beamCrnrId1, beamCornerId1_Text, cornerText, CommonModule.internalCornerDescp, beamInternalCornerText, CommonModule.intrnlCornr_Flange1, CommonModule.intrnlCornr_Flange2, heightOfBeamSidePanel, elev);
                            setBeamCornerId_CornerTextId(beamCrnrId1, beamCornerId1_Text, wallCornerId1);
                        }
                        else
                        {
                            AEE_Utility.deleteEntity(beamCrnrId1);
                        }


                    }
                    else
                    {
                        if (newCorner1TextId.IsErased == false)
                        {
                            listOfBeamCornerMoveId.Add(beamCornerId1);
                            listOfBeamCornerTextMoveId.Add(newCorner1TextId);
                            setBeamCornerId_CornerTextId(beamCornerId1, newCorner1TextId, wallCornerId1);
                        }
                    }

                    if (beamCornerId2.IsValid == false)
                    {
                        double minLengthCorner2 = listOfLengthCorner2.Min();
                        int indexOfMinLengthCorner2 = listOfLengthCorner2.IndexOf(minLengthCorner2);
                        Point3d insertionPointCorner2 = listOfStrtEndPoint[indexOfMinLengthCorner2];

                        Vector3d copyPasteVector_Corner2 = new Vector3d((insertionPointCorner2.X - basePointOfCorner2.X), (insertionPointCorner2.Y - basePointOfCorner2.Y), 0);
                        var beamCrnrId2 = AEE_Utility.copyColonEntity(copyPasteVector_Corner2, wallCornerId2);


                        bool flagInsideBeam = CheckAnyVerticesPointOfCornerIdIsInsidePolyLineObj(beamCrnrId2, beamId_InsctWall, true);
                        if (flagInsideBeam)
                        {
                            listOfBeamCornerMoveId.Add(beamCrnrId2);

                            var beamCornerId2_Text = crnerHlpr.writeTextInCorner(beamCrnrId2, cornerText, crnerRotationAngle, CommonModule.internalCornerTextLyrName, CommonModule.internalCornerLyrColor);
                            listOfBeamCornerTextMoveId.Add(beamCornerId2_Text);

                            listOfBeamCornerBasePoint.Add(AEE_Utility.GetBasePointOfPolyline(beamCrnrId2));
                            listOfBeamCornerId.Add(beamCrnrId2);
                            int wallId, roomId;
                            bool ext, ret, intr, lift, stair,duct;
                            ret = ElevationHelper.getWallDetails(xDataRegAppNameWallLine, out wallId, out roomId, out intr, out ext, out lift, out stair,out duct);
                            string w1 = "W" + ((wallId + 1) % pnts.Count).ToString() + "_" + (ext ? "EX" : "R") + "_" + roomId.ToString();


                            //Fix BIAX 2nd position by SDM 12-06-2022
                            //start
                            double dist1 = 0;
                            double dist2 = 0;
                            double wall1length = 0;
                            var walllines = InternalWallHelper.listOfListOfInternalWallRoomLineId_With_WallName;
                            for (int r = 0; r < walllines.Count; r++)
                            {
                                for (int w = 0; w < walllines[r].Count; w++)
                                {
                                    var name = AEE_Utility.GetXDataRegisterAppName(walllines[r][w]);

                                    if (name == xDataRegAppNameWallLine)
                                        wall1length = AEE_Utility.GetLengthOfLine(walllines[r][w]);
                                }
                            }
                            bool flag_btwnTowBeams = false;

                            for (int j = 0; j < listOfAllInsctPointOfTwoBeam.Count; j++)
                            {
                                if (listOfAllInsctPointOfTwoBeam[j] == listOfBeamCornerBasePoint[1])
                                    flag_btwnTowBeams = true;
                            }

                            dist1 = wall1length - CommonModule.intrnlCornr_Flange1 - (flag_btwnTowBeams ? distBtwWallToBeam : 0);
                            dist2 = distBtwWallToBeam;

                            CornerElevation elev = new CornerElevation(xDataRegAppNameWallLine, w1, dist1, dist2, beamBottom);
                            //end

                            // CornerElevation elev = new CornerElevation(xDataRegAppNameWallLine, w1, listOfLengthCorner2[0] - CommonModule.intrnlCornr_Flange2, distBtwWallToBeam, beamBottom);
                            CornerHelper.setCornerDataForBOM(xDataRegAppNameWallLine, beamCrnrId2, beamCornerId2_Text, cornerText, CommonModule.internalCornerDescp, beamInternalCornerText, CommonModule.intrnlCornr_Flange1, CommonModule.intrnlCornr_Flange2, heightOfBeamSidePanel, elev);

                            setBeamCornerId_CornerTextId(beamCrnrId2, beamCornerId2_Text, wallCornerId2);
                        }
                        else
                        {
                            AEE_Utility.deleteEntity(beamCrnrId2);
                        }
                    }
                    else
                    {
                        if (newCorner2TextId.IsErased == false)
                        {
                            listOfBeamCornerMoveId.Add(beamCornerId2);
                            listOfBeamCornerTextMoveId.Add(newCorner2TextId);
                            setBeamCornerId_CornerTextId(beamCornerId2, newCorner2TextId, wallCornerId2);
                        }
                    }
                    if (beamLayerNameInsct.Contains(Beam_UI_Helper.offsetBeamStrtText))//Added on 19/03/2023 by SDM for "Beam" implementation
                        listOfAllOffsetBeamCornerIdsWithBeamID.Add(new Tuple<ObjectId, List<ObjectId>, List<ObjectId>, double, string>(beamId_InsctWall, listOfBeamCornerMoveId, listOfBeamCornerTextMoveId, distBtwWallToBeam, beamLayerNameInsct));

                    AEE_Utility.MoveEntity(listOfBeamCornerMoveId, CreateShellPlanHelper.moveVector_ForBeamLayout);
                    AEE_Utility.MoveEntity(listOfBeamCornerTextMoveId, CreateShellPlanHelper.moveVector_ForBeamLayout);
                    AEE_Utility.deleteEntity(offsetLineId);
                }
            }
        }


        public void checkBeamWallPanelLength(string xDataRegAppName, ObjectId beamId, ObjectId wallPanelLineId, ObjectId nearestBeamWallLineId, double distanceBtwWallToBeam, ObjectId beamCornerId1, ObjectId beamCornerId2,
            bool flagOfSunkanSlab, ObjectId sunkanSlabId)
        {
            List<ObjectId> listOfBeamSidePanelsId = new List<ObjectId>();

            bool flagOfMove = false;
            var moveVectorOfBeamLine = getMoveVectorOfBeamWallLine(wallPanelLineId, nearestBeamWallLineId, distanceBtwWallToBeam, out flagOfMove);
            if (flagOfMove == true)
            {
                bool flagOfNewBeamWallPnlCreate = false;
                double beamWallPanelLngth = getBeamWallPanelLength(beamId, out flagOfNewBeamWallPnlCreate, AEE_Utility.GetLayerName(wallPanelLineId), flagOfSunkanSlab, sunkanSlabId);
                if (flagOfNewBeamWallPnlCreate == true)
                {
                    var wallLine = AEE_Utility.GetLine(wallPanelLineId);
                    var beam_line = wallPanelLineId;
                    var exploded_lines = AEE_Utility.ExplodeEntity(beamId);
                    //foreach (var exp_lineId in exploded_lines)
                    //{
                    //    var exp_line = AEE_Utility.GetLine(exp_lineId);
                    //    if(Utils.IsSubline(exp_line.StartPoint.ConvertToPoint(),exp_line.EndPoint.ConvertToPoint(), wallLine.StartPoint.ConvertToPoint(), wallLine.EndPoint.ConvertToPoint()))
                    //    {
                    //        beam_line = exp_lineId;
                    //    }
                    //}

                    var beamSidePanelLineId = AEE_Utility.createColonEntityInSamePoint(beam_line, false);
                    AEE_Utility.AttachXData(beamSidePanelLineId, xDataRegAppName, CommonModule.xDataAsciiName);

                    //AEE_Utility.changeColor(beamSidePanelLineId, 3);

                    AEE_Utility.MoveEntity(beamSidePanelLineId, moveVectorOfBeamLine);
                    listOfBeamSidePanelsId.Add(beamSidePanelLineId);

                    var offsetLineId = AEE_Utility.OffsetLine(beamSidePanelLineId, 10, false);
                    var midPointOfOffstLine = AEE_Utility.GetMidPoint(offsetLineId);

                    ObjectId offsetBeamSidePanelLineId = new ObjectId();
                    var flagOfInside = AEE_Utility.GetPointIsInsideThePolyline(beamId, midPointOfOffstLine);
                    if (flagOfInside == true)
                    {
                        offsetBeamSidePanelLineId = AEE_Utility.OffsetLine(beamSidePanelLineId, -CommonModule.panelDepth, false);
                    }
                    else
                    {
                        offsetBeamSidePanelLineId = AEE_Utility.OffsetLine(beamSidePanelLineId, CommonModule.panelDepth, false);
                    }
                    listOfBeamSidePanelsId.Add(offsetBeamSidePanelLineId);

                    listOfListOfBeamSidePanelLineId.Add(listOfBeamSidePanelsId);

                    listOfBeamSideWallId.Add(beamId);

                    listOfSunkanSlabObjId.Add(sunkanSlabId);

                    AEE_Utility.deleteEntity(offsetLineId);
                }
            }
        }


        public double getBeamWallPanelLength(ObjectId beamId, out bool flagOfNewBeamWallPnlCreate, String layerName, bool flagOfSunkanSlab, ObjectId sunkanSlabId)
        {
            flagOfNewBeamWallPnlCreate = false;

            double beamWallPanelLngth = 0;
            if (beamId.IsValid == true)
            {
                //Changes made on 13/03/2023 by SDM
                double heightOfBeamSidePanel = 0;
                string beamLayer = AEE_Utility.GetLayerName(beamId);
                if (beamLayer.Contains(Beam_UI_Helper.offsetBeamStrtText))
                {
                    double beamBottom = 0;
                    beamBottom = Beam_UI_Helper.getOffsetBeamBottom(beamId);
                    heightOfBeamSidePanel = GeometryHelper.getHeightOfBeamSidePanel(InternalWallSlab_UI_Helper.getSlabBottom(layerName), beamBottom, PanelLayout_UI.SL, sunkanSlabId.IsValid, sunkanSlabId);
                }
                else
                    heightOfBeamSidePanel = Beam_UI_Helper.getBeamHeight(beamId);
                //---------------

                //Note:Beam Side horz panel is not working for IH when beam side ht is less than 750.
                if (heightOfBeamSidePanel <= PanelLayout_UI.standardPanelWidth)
                {
                    flagOfNewBeamWallPnlCreate = true;
                    //beamWallPanelLngth = 1;//Commented on 13/07/2023 by PRT
                    beamWallPanelLngth = PanelLayout_UI.standardPanelWidth;
                }
                else if ((PanelLayout_UI.standardPanelWidth + 1) > PanelLayout_UI.standardPanelWidth && beamWallPanelLngth <= PanelLayout_UI.maxWidthOfPanel)
                {
                    flagOfNewBeamWallPnlCreate = true;
                    //beamWallPanelLngth = 1000;//Commented on 13/07/2023 by PRT
                    beamWallPanelLngth = PanelLayout_UI.beamSideMaxPanelLength;
                }
                else
                {

                    flagOfNewBeamWallPnlCreate = false;
#if WNPANEL
                    flagOfNewBeamWallPnlCreate = true;
#endif
                    beamWallPanelLngth = PanelLayout_UI.standardPanelWidth;
                }
            }
            return beamWallPanelLngth;
        }


        public List<ObjectId> getNewWallPanelLineWithAdjacentBeam(List<ObjectId> listOfWallPanelLineWithDoor_ObjId, ObjectId adjacentOfBeamId, Point3d adjacentInsidePointOfBeam, string beamLayerNameInsctToWall, out ObjectId newWallLineIdWithBeam)
        {
            newWallLineIdWithBeam = new ObjectId();
            if (beamLayerNameInsctToWall == "")
            {
                if (adjacentOfBeamId.IsValid == true)
                {
                    List<double> listOfLength = new List<double>();
                    List<ObjectId> listOfWallLineId = new List<ObjectId>();
                    List<Point3d> listOfPoint = new List<Point3d>();
                    foreach (var wallLineId in listOfWallPanelLineWithDoor_ObjId)
                    {
                        var listOfStrtEndPoint = AEE_Utility.GetStartEndPointOfLine(wallLineId);
                        for (int k = 0; k < listOfStrtEndPoint.Count; k++)
                        {
                            Point3d point = listOfStrtEndPoint[k];
                            var length = Math.Round(AEE_Utility.GetLengthOfLine(point, adjacentInsidePointOfBeam));//Changes made on 29/05/2023 by SDM
                            listOfLength.Add(length);
                            listOfWallLineId.Add(wallLineId);
                            listOfPoint.Add(point);
                        }
                    }
                    
                    if (listOfLength.Count != 0)
                    {
                        double minLength = listOfLength.Min();
                        if (minLength != 0)
                        {
                            int indexOfMinLngth = listOfLength.IndexOf(minLength);
                            ObjectId nearestBeamWallLineId = listOfWallLineId[indexOfMinLngth];
                            Point3d nearestWallPoint = listOfPoint[indexOfMinLngth];

                            var nearestBeamWallLine = AEE_Utility.GetLine(nearestBeamWallLineId);
                            CommonModule commonMod = new CommonModule();
                            var listOfStrtEndPoint = commonMod.getStartEndPointOfLine(nearestBeamWallLine);
                            Point3d newStrtPoint = listOfStrtEndPoint[0];
                            Point3d newEndPoint = listOfStrtEndPoint[1];

                            var listOfInsctPoint = AEE_Utility.InterSectionBetweenTwoEntity(adjacentOfBeamId, nearestBeamWallLineId);
                            if (listOfInsctPoint.Count != 0)
                            {
                                var angleOfNewLine = AEE_Utility.GetAngleOfLine(newStrtPoint, newEndPoint);
                                var diffPoint = AEE_Utility.get_XY(angleOfNewLine, CommonModule.internalCorner);
                                Point3d intersectPointOfBeam = listOfInsctPoint[0];

                                Point3d beamCornerEndPoint = new Point3d();
                                Point3d firstPoint = new Point3d();
                                Point3d lastPoint = new Point3d();
                                var flafOfInside = AEE_Utility.GetPointIsInsideThePolyline(adjacentOfBeamId, newStrtPoint);
                                if (flafOfInside == true)
                                {
                                    beamCornerEndPoint = new Point3d((intersectPointOfBeam.X + diffPoint.X), (intersectPointOfBeam.Y + diffPoint.Y), 0);
                                    firstPoint = newStrtPoint;
                                    lastPoint = newEndPoint;
                                }
                                else
                                {
                                    flafOfInside = AEE_Utility.GetPointIsInsideThePolyline(adjacentOfBeamId, newEndPoint);
                                    beamCornerEndPoint = new Point3d((intersectPointOfBeam.X - diffPoint.X), (intersectPointOfBeam.Y - diffPoint.Y), 0);
                                    firstPoint = newEndPoint;
                                    lastPoint = newStrtPoint;
                                }

                                newWallLineIdWithBeam = AEE_Utility.getLineId(nearestBeamWallLine, firstPoint, beamCornerEndPoint, false);
                                var wallLineIdWithoutBeam = AEE_Utility.getLineId(nearestBeamWallLine, beamCornerEndPoint, lastPoint, false);
                                listOfWallPanelLineWithDoor_ObjId[listOfWallPanelLineWithDoor_ObjId.FindIndex(ind => ind.Equals(nearestBeamWallLineId))] = wallLineIdWithoutBeam;

                                listOfWallPanelLineWithDoor_ObjId.Add(newWallLineIdWithBeam);
                            }
                        }
                        //Changes made on 29/05/2023 by SDM
                        else if (listOfPoint.All(X => AEE_Utility.IsPointOnEntity(adjacentOfBeamId, X)))
                        {
                            newWallLineIdWithBeam = listOfWallPanelLineWithDoor_ObjId.FirstOrDefault();
                        }
                    }
                }
            }
            return listOfWallPanelLineWithDoor_ObjId;
        }


        //private List<Point3d> drawCornerBtwTwoBeam(string xDataRegAppNameWallLine, ObjectId cornerId1, ObjectId cornerId2, List<Point3d> listOfAllInsctPointOfTwoBeam, string cornerText, double crnerRotationAngle, out ObjectId newCornerId1, out ObjectId newCornerId2, out ObjectId newCorner1TextId, out ObjectId newCorner2TextId, out List<ObjectId> listOfBeamCornerId)
        private List<Point3d> drawCornerBtwTwoBeam(string xDataRegAppNameWallLine, ObjectId cornerId1, ObjectId cornerId2, List<Point3d> listOfAllInsctPointOfTwoBeam, string cornerText, double crnerRotationAngle, double heightOfBeamSidePanel, double distBtwWallToBeam, double beamBottom, out ObjectId newCornerId1, out ObjectId newCornerId2, out ObjectId newCorner1TextId, out ObjectId newCorner2TextId, out List<ObjectId> listOfBeamCornerId)
        {
            List<Point3d> listOfBeamCornerBasePoint = new List<Point3d>();
            listOfBeamCornerId = new List<ObjectId>();

            newCornerId1 = new ObjectId();
            newCornerId2 = new ObjectId();
            newCorner1TextId = new ObjectId();
            newCorner2TextId = new ObjectId();
            Vector3d copyPasteVector = new Vector3d();
            if (listOfAllInsctPointOfTwoBeam.Count != 0)
            {
                CornerHelper crnerHlpr = new CornerHelper();

                var basePointOfCorner1 = AEE_Utility.GetBasePointOfPolyline(cornerId1);
                var basePointOfCorner2 = AEE_Utility.GetBasePointOfPolyline(cornerId2);

                for (int i = 0; i < listOfAllInsctPointOfTwoBeam.Count; i++)
                {
                    Point3d insctPointOfTwoBeam = listOfAllInsctPointOfTwoBeam[i];

                    //if (listOfTwoBeamInsctPoint.Contains(insctPointOfTwoBeam))
                    //{
                    //    continue;
                    //}
                    var lengthOfCorner1 = AEE_Utility.GetLengthOfLine(insctPointOfTwoBeam.X, insctPointOfTwoBeam.Y, basePointOfCorner1.X, basePointOfCorner1.Y);

                    var lengthOfCorner2 = AEE_Utility.GetLengthOfLine(insctPointOfTwoBeam.X, insctPointOfTwoBeam.Y, basePointOfCorner2.X, basePointOfCorner2.Y);

                    double minLength = Math.Min(lengthOfCorner1, lengthOfCorner2);

                    if (minLength == lengthOfCorner1)
                    {
                        copyPasteVector = new Vector3d((insctPointOfTwoBeam.X - basePointOfCorner1.X), (insctPointOfTwoBeam.Y - basePointOfCorner1.Y), 0);
                        newCornerId1 = AEE_Utility.copyColonEntity(copyPasteVector, cornerId1);

                        newCorner1TextId = crnerHlpr.writeTextInCorner(newCornerId1, cornerText, crnerRotationAngle, CommonModule.internalCornerTextLyrName, CommonModule.internalCornerLyrColor);

                        if (listOfTwoBeamInsctPoint.Contains(insctPointOfTwoBeam))
                        {
                            AEE_Utility.changeVisibility(newCornerId1, false);
                            AEE_Utility.deleteEntity(newCorner1TextId);
                        }
                        else
                        {
                            listOfTwoBeamInsctPoint.Add(insctPointOfTwoBeam);
                        }
                    }
                    else
                    {
                        copyPasteVector = new Vector3d((insctPointOfTwoBeam.X - basePointOfCorner2.X), (insctPointOfTwoBeam.Y - basePointOfCorner2.Y), 0);
                        newCornerId2 = AEE_Utility.copyColonEntity(copyPasteVector, cornerId2);
                        newCorner2TextId = crnerHlpr.writeTextInCorner(newCornerId2, cornerText, crnerRotationAngle, CommonModule.internalCornerTextLyrName, CommonModule.internalCornerLyrColor);

                        if (listOfTwoBeamInsctPoint.Contains(insctPointOfTwoBeam))
                        {
                            AEE_Utility.changeVisibility(newCornerId2, false);
                            AEE_Utility.deleteEntity(newCorner2TextId);
                        }
                        else
                        {
                            listOfTwoBeamInsctPoint.Add(insctPointOfTwoBeam);
                        }
                    }

                    //if (listOfTwoBeamInsctPoint.Contains(insctPointOfTwoBeam))
                    //{
                    //    AEE_Utility.changeVisibility(newCornerId1, false);
                    //    AEE_Utility.changeVisibility(newCornerId2, false);
                    //    //////AEE_Utility.deleteEntity(newCornerId1);
                    //    //////AEE_Utility.deleteEntity(newCornerId2);
                    //    AEE_Utility.deleteEntity(newCorner1TextId);
                    //    AEE_Utility.deleteEntity(newCorner2TextId);
                    //}
                    //listOfTwoBeamInsctPoint.Add(insctPointOfTwoBeam);
                }

                if (newCornerId1.IsValid == true)
                {
                    var beamCornerbasePoint = AEE_Utility.GetBasePointOfPolyline(newCornerId1);
                    listOfBeamCornerBasePoint.Add(beamCornerbasePoint);
                    listOfBeamCornerId.Add(newCornerId1);

                    //Add BIAX elevation between two beams by SDM 28-06-2022
                    //start
                    string w1 = "";
                    string w2 = "";
                    double dist1 = 0;
                    double dist2 = 0;
                    int room_idx = Convert.ToInt32(xDataRegAppNameWallLine.Split('_')[2]) - 1;
                    var walllines = InternalWallHelper.listOfListOfInternalWallRoomLineId_With_WallName[room_idx];

                    for (int w = 0; w < walllines.Count; w++)
                    {
                        var name = AEE_Utility.GetXDataRegisterAppName(walllines[w]);

                        if (name == xDataRegAppNameWallLine)
                        {
                            var wallline2 = AEE_Utility.GetLine(walllines[w]);
                            if (newCornerId1.IsValid == true)
                            {
                                var wallline1 = AEE_Utility.GetLine(walllines[(w == 0 ? walllines.Count : w) - 1]);
                                if (wallline1.StartPoint.X == wallline1.EndPoint.X)
                                    dist2 = Math.Abs(copyPasteVector.X);
                                else
                                    dist2 = Math.Abs(copyPasteVector.Y);


                                dist1 = wallline1.Length - CommonModule.intrnlCornr_Flange1 - distBtwWallToBeam;
                                w1 = AEE_Utility.GetXDataRegisterAppName(walllines[(w == 0 ? walllines.Count : w) - 1]);
                                w2 = xDataRegAppNameWallLine;
                            }
                        }
                    }
                    //end

                    CornerElevation elev = new CornerElevation(w1, w2, dist1, dist2, beamBottom);
                    CornerHelper.setCornerDataForBOM(xDataRegAppNameWallLine, newCornerId1, newCorner1TextId, cornerText, CommonModule.internalCornerDescp, CommonModule.beamInternalCornerText, CommonModule.intrnlCornr_Flange1, CommonModule.intrnlCornr_Flange2, heightOfBeamSidePanel, elev);
                }
                if (newCornerId2.IsValid == true)
                {
                    listOfBeamCornerBasePoint.Add(AEE_Utility.GetBasePointOfPolyline(newCornerId2));
                    listOfBeamCornerId.Add(newCornerId2);
                }
            }
            return listOfBeamCornerBasePoint;
        }


        public ObjectId changeWallPanelLine_With_TwoBeamIntersct(string xDataRegAppNameOfWallLine, ObjectId wallCornerId1, ObjectId wallCornerId2, ObjectId wallLineId, List<Point3d> listOfAllInsctPointOfTwoBeam, double distBtwWallToBeam, ObjectId nearestWallToBeamWallId, out List<ObjectId> listOfBeamInsideWallLinesId)
        {
            listOfBeamInsideWallLinesId = new List<ObjectId>();

            var basePointCorner1 = AEE_Utility.GetBasePointOfPolyline(wallCornerId1);
            var basePointCorner2 = AEE_Utility.GetBasePointOfPolyline(wallCornerId2);
            var cornerLineId = AEE_Utility.getLineId(basePointCorner1, basePointCorner2, false);
            var cornerLine = AEE_Utility.GetLine(cornerLineId);

            CommonModule commonModule = new CommonModule();
            var listOfStrtEndPointOfCornerLine = commonModule.getStartEndPointOfLine(cornerLine);

            Point3d startPointOfCornerLine = listOfStrtEndPointOfCornerLine[0];
            Point3d endPointOfCornerLine = listOfStrtEndPointOfCornerLine[1];

            var wallLine = AEE_Utility.GetLine(wallLineId);
            var listOfStrtEndPointOfWallLine = commonModule.getStartEndPointOfLine(wallLine);
            Point3d startPointOfWallLine = listOfStrtEndPointOfWallLine[0];
            Point3d endPointOfWallLine = listOfStrtEndPointOfWallLine[1];

            double lengthOfStrtPointToBeamCornerEnd = 0;
            double lengthOfEndPointToBeamCornerEnd = 0;

            //for (int i = 0; i < listOfAllInsctPointOfTwoBeam.Count; i++)
            //{
            //    Point3d insctPoint = listOfAllInsctPointOfTwoBeam[i];
            //    double lngthStrtPnt = AEE_Utility.GetLengthOfLine(startPointOfCornerLine.X, startPointOfCornerLine.Y, insctPoint.X, insctPoint.Y);
            //    double lngthEndPnt = AEE_Utility.GetLengthOfLine(endPointOfCornerLine.X, endPointOfCornerLine.Y, insctPoint.X, insctPoint.Y);
            //    double minLength = Math.Min(lngthStrtPnt, lngthEndPnt);
            //    if (minLength == lngthStrtPnt)
            //    {
            //        lengthOfStrtPointToBeamCornerEnd = getLength(startPointOfCornerLine, endPointOfCornerLine, insctPoint, distBtwWallToBeam, nearestWallToBeamWallId, true, false);
            //    }
            //    else
            //    {
            //        lengthOfEndPointToBeamCornerEnd = getLength(startPointOfCornerLine, endPointOfCornerLine, insctPoint, distBtwWallToBeam, nearestWallToBeamWallId, false, true);
            //    }
            //}

            double angleOfWallLine = AEE_Utility.GetAngleOfLine(startPointOfCornerLine.X, startPointOfCornerLine.Y, endPointOfCornerLine.X, endPointOfCornerLine.Y);
            Point3d newStrtPoint = new Point3d();
            Point3d newEndPoint = new Point3d();
            if (lengthOfStrtPointToBeamCornerEnd != 0)
            {
                var beamCornerEndPoint = AEE_Utility.get_XY(angleOfWallLine, lengthOfStrtPointToBeamCornerEnd);
                newStrtPoint = new Point3d((startPointOfCornerLine.X + beamCornerEndPoint.X), (startPointOfCornerLine.Y + beamCornerEndPoint.Y), 0);

                var remainingStrtPointLineId = AEE_Utility.getLineId(wallLine, startPointOfWallLine, newStrtPoint, false);
                listOfBeamInsideWallLinesId.Add(remainingStrtPointLineId);

                AEE_Utility.AttachXData(remainingStrtPointLineId, xDataRegAppNameOfWallLine, CommonModule.xDataAsciiName);

                listOfBeamLineId_ForMoveInBeamLayout.Add(remainingStrtPointLineId);
            }
            else
            {
                newStrtPoint = startPointOfCornerLine;
            }

            if (lengthOfEndPointToBeamCornerEnd != 0)
            {
                var beamCornerEndPoint = AEE_Utility.get_XY(angleOfWallLine, lengthOfEndPointToBeamCornerEnd);
                newEndPoint = new Point3d((endPointOfCornerLine.X - beamCornerEndPoint.X), (endPointOfCornerLine.Y - beamCornerEndPoint.Y), 0);

                var remainingEndPointLineId = AEE_Utility.getLineId(wallLine, newEndPoint, endPointOfWallLine, false);
                listOfBeamInsideWallLinesId.Add(remainingEndPointLineId);

                AEE_Utility.AttachXData(remainingEndPointLineId, xDataRegAppNameOfWallLine, CommonModule.xDataAsciiName);

                listOfBeamLineId_ForMoveInBeamLayout.Add(remainingEndPointLineId);

            }
            else
            {
                newEndPoint = endPointOfCornerLine;
            }

            var newWallLineId = AEE_Utility.getLineId(wallLine, newStrtPoint, newEndPoint, false);
            AEE_Utility.AttachXData(newWallLineId, xDataRegAppNameOfWallLine, CommonModule.xDataAsciiName);

            return newWallLineId;
        }


        private double getLength(Point3d startPointOfWallLine, Point3d endPointOfWallLine, Point3d insctPoint, double distBtwWallToBeam, ObjectId nearestWallToBeamWallId, bool flagOfStrtPoint, bool flagOfEndPoint)
        {
            var lineId = AEE_Utility.getLineId(startPointOfWallLine, endPointOfWallLine, false);

            var offsetLineId = AEE_Utility.OffsetLine(lineId, distBtwWallToBeam, false);
            double distance = AEE_Utility.GetDistanceBetweenTwoLines(nearestWallToBeamWallId, offsetLineId);

            if (distance > distBtwWallToBeam)
            {
                AEE_Utility.deleteEntity(offsetLineId);
                offsetLineId = AEE_Utility.OffsetLine(lineId, -distBtwWallToBeam, false);
            }

            var listOfBeamLineStrtEndPoint = AEE_Utility.GetStartEndPointOfLine(offsetLineId);

            Point3d beamLineStrtPoint = listOfBeamLineStrtEndPoint[0];
            Point3d beamLineEndPoint = listOfBeamLineStrtEndPoint[1];

            double gapBtwWallToBeamCornerEnd = 0;
            if (flagOfStrtPoint == true)
            {
                gapBtwWallToBeamCornerEnd = AEE_Utility.GetLengthOfLine(insctPoint.X, insctPoint.Y, beamLineStrtPoint.X, beamLineStrtPoint.Y);
            }
            else if (flagOfEndPoint == true)
            {
                gapBtwWallToBeamCornerEnd = AEE_Utility.GetLengthOfLine(insctPoint.X, insctPoint.Y, beamLineEndPoint.X, beamLineEndPoint.Y);
            }

            gapBtwWallToBeamCornerEnd = gapBtwWallToBeamCornerEnd + CommonModule.internalCorner;

            AEE_Utility.deleteEntity(lineId);
            AEE_Utility.deleteEntity(offsetLineId);

            return gapBtwWallToBeamCornerEnd;
        }


        public void checkRemainingWallBeamLineIsInsideTheDoorOrWindow(List<ObjectId> listOfBeamInsideWallLinesId, List<ObjectId> listOfDoorObjId_With_WallInsct)
        {
            List<ObjectId> listOfWallLineInsideTheDoorOrWindow = new List<ObjectId>();

            foreach (var wallLineId in listOfBeamInsideWallLinesId)
            {
                var length = AEE_Utility.GetLengthOfLine(wallLineId);
                if (length > 0)
                {
                    foreach (var doorId in listOfDoorObjId_With_WallInsct)
                    {
                        if (!listOfWallLineInsideTheDoorOrWindow.Contains(wallLineId))
                        {
                            bool pointIsInsideTheWindow = checkPointIsInsideTheBoundary(wallLineId, doorId);
                            if (pointIsInsideTheWindow == true)
                            {
                                listOfWallLineInsideTheDoorOrWindow.Add(wallLineId);
                            }
                        }
                    }
                }
                else
                {
                    listOfWallLineInsideTheDoorOrWindow.Add(wallLineId);
                }
            }

            List<ObjectId> listOfWallLineNotInsideTheDoorOrWindow = new List<ObjectId>();

            foreach (var wallId in listOfBeamInsideWallLinesId)
            {
                if (!listOfWallLineInsideTheDoorOrWindow.Contains(wallId))
                {
                    listOfWallLineNotInsideTheDoorOrWindow.Add(wallId);
                }
            }
        }


        public List<ObjectId> checkRemainingWallBeamLineIsInsideTheDoorOrWindow(List<ObjectId> listOfBeamInsideWallLinesId, List<ObjectId> listOfWindowObjId_With_WallInsct, List<ObjectId> listOfDoorObjId_With_WallInsct)
        {
            List<ObjectId> listOfWallLineInsideTheDoorOrWindow = new List<ObjectId>();

            foreach (var wallLineId in listOfBeamInsideWallLinesId)
            {
                var length = AEE_Utility.GetLengthOfLine(wallLineId);
                if (length > 0)
                {
                    foreach (var windowId in listOfWindowObjId_With_WallInsct)
                    {
                        if (!listOfWallLineInsideTheDoorOrWindow.Contains(wallLineId))
                        {
                            bool pointIsInsideTheWindow = checkPointIsInsideTheBoundary(wallLineId, windowId);
                            if (pointIsInsideTheWindow == true)
                            {
                                listOfWallLineInsideTheDoorOrWindow.Add(wallLineId);
                            }
                        }
                    }

                    foreach (var doorId in listOfDoorObjId_With_WallInsct)
                    {
                        if (!listOfWallLineInsideTheDoorOrWindow.Contains(wallLineId))
                        {
                            bool pointIsInsideTheWindow = checkPointIsInsideTheBoundary(wallLineId, doorId);
                            if (pointIsInsideTheWindow == true)
                            {
                                listOfWallLineInsideTheDoorOrWindow.Add(wallLineId);
                            }
                        }
                    }
                }
                else
                {
                    listOfWallLineInsideTheDoorOrWindow.Add(wallLineId);
                }
            }

            List<ObjectId> listOfWallLineNotInsideTheDoorOrWindow = new List<ObjectId>();

            foreach (var wallId in listOfBeamInsideWallLinesId)
            {
                if (!listOfWallLineInsideTheDoorOrWindow.Contains(wallId))
                {
                    listOfWallLineNotInsideTheDoorOrWindow.Add(wallId);
                }
            }

            return listOfWallLineNotInsideTheDoorOrWindow;
        }


        public bool checkPointIsInsideTheBoundary(ObjectId wallLineId, ObjectId polylineId)
        {
            var offsetInPost_Dir = AEE_Utility.OffsetEntity(WindowHelper.windowOffsetValue, wallLineId, false);
            var offsetInNegt_Dir = AEE_Utility.OffsetEntity(-WindowHelper.windowOffsetValue, wallLineId, false);

            var midPointOfWallLine = AEE_Utility.GetMidPoint(wallLineId);
            var midPointOfWallLine_Post_Dir = AEE_Utility.GetMidPoint(offsetInPost_Dir);
            var midPointOfWallLine_Negt_Dir = AEE_Utility.GetMidPoint(offsetInNegt_Dir);

            bool pointIsInsideTheBeam = AEE_Utility.GetPointIsInsideThePolyline(polylineId, midPointOfWallLine);
            if (pointIsInsideTheBeam == false)
            {
                pointIsInsideTheBeam = AEE_Utility.GetPointIsInsideThePolyline(polylineId, midPointOfWallLine_Post_Dir);
            }
            if (pointIsInsideTheBeam == false)
            {
                pointIsInsideTheBeam = AEE_Utility.GetPointIsInsideThePolyline(polylineId, midPointOfWallLine_Negt_Dir);
            }

            AEE_Utility.deleteEntity(offsetInPost_Dir);
            AEE_Utility.deleteEntity(offsetInNegt_Dir);

            if (pointIsInsideTheBeam == true)
            {
                return true;
            }
            return false;
        }


        public void drawBeamWallPanel(ObjectId sunkanSlabId, string beamLayerNameInsctToWall, ObjectId beamId, double distanceBtwWallToBeam, ObjectId nearestBeamWallLineId, List<ObjectId> listOfWallPanelRect_ObjId, List<ObjectId> listOfWallPanelLine_ObjId, ObjectId wallPanelLineId)
        {
            if (string.IsNullOrEmpty(beamLayerNameInsctToWall) || string.IsNullOrWhiteSpace(beamLayerNameInsctToWall))
            {

            }
            else
            {
                if (distanceBtwWallToBeam == 0 || beamId.IsValid == false || nearestBeamWallLineId.IsValid == false)
                {
                    return;
                }
                if (BeamHelper.listOfWallPanelLineId_WithBeamDoor.Contains(wallPanelLineId))
                {
                    return;
                }
                var xDataRegAppName = AEE_Utility.GetXDataRegisterAppName(wallPanelLineId);
                var indexOfXDataNameExist = listOfBeamSideWallId.IndexOf(beamId);
                if (indexOfXDataNameExist == -1)
                {
                    bool flagOfMove = false;
                    var lyrName1 = AEE_Utility.GetLayerName(nearestBeamWallLineId);
                    var lyrName2 = AEE_Utility.GetLayerName(wallPanelLineId);
                    var moveVectorOfBeamLine = getMoveVectorOfBeamWallLine(wallPanelLineId, nearestBeamWallLineId, distanceBtwWallToBeam, out flagOfMove);

                    var lyrName = InternalWallSlab_UI_Helper.getWallLayer(new string[] { lyrName1, lyrName2, beamLayerNameInsctToWall });
                    drawBeamPanel(sunkanSlabId, beamLayerNameInsctToWall, listOfWallPanelRect_ObjId, listOfWallPanelLine_ObjId, moveVectorOfBeamLine, beamLayerNameInsctToWall);
                }
            }
        }


        private void getInsctPointWithBeamToWallPanel(ObjectId newWallPanelLineId, ObjectId nearestBeamWallLineId, double distanceBtwWallToBeam, out List<Point3d> listOfStartPointInsct, out List<Point3d> listOfEndPointInsct)
        {
            listOfStartPointInsct = new List<Point3d>();
            listOfEndPointInsct = new List<Point3d>();

            double addtionValue = 20;

            var nearestBeamWallLine = AEE_Utility.GetLine(nearestBeamWallLineId);
            CommonModule commonModule = new CommonModule();
            var listOfNearstPoint = commonModule.getStartEndPointOfLine(nearestBeamWallLine);
            Point3d nearstStartPoint = listOfNearstPoint[0];
            Point3d nearstEndPoint = listOfNearstPoint[1];

            var nearestWallLineAngle = AEE_Utility.GetAngleOfLine(nearstStartPoint.X, nearstStartPoint.Y, nearstEndPoint.X, nearstEndPoint.Y);
            var point = AEE_Utility.get_XY(nearestWallLineAngle, addtionValue);
            var checkNearestBeamWallLineId = AEE_Utility.getLineId((nearstStartPoint.X - point.X), (nearstStartPoint.Y - point.Y), 0, (nearstEndPoint.X + point.X), (nearstEndPoint.Y + point.Y), 0, false);

            distanceBtwWallToBeam = distanceBtwWallToBeam + addtionValue;

            var newOffsetWallPanelLineId = AEE_Utility.OffsetLine(newWallPanelLineId, distanceBtwWallToBeam, false);

            double distance = AEE_Utility.GetDistanceBetweenTwoLines(checkNearestBeamWallLineId, newOffsetWallPanelLineId);

            if (distance > distanceBtwWallToBeam)
            {
                AEE_Utility.deleteEntity(newOffsetWallPanelLineId);
                newOffsetWallPanelLineId = AEE_Utility.OffsetLine(newWallPanelLineId, -distanceBtwWallToBeam, false);
            }

            var newWallPanelLine = AEE_Utility.GetLine(newWallPanelLineId);
            var newOffsetWallPanelLine = AEE_Utility.GetLine(newOffsetWallPanelLineId);

            ObjectId strtPointLineId = AEE_Utility.getLineId(newWallPanelLine.StartPoint, newOffsetWallPanelLine.StartPoint, false);
            listOfStartPointInsct = AEE_Utility.InterSectionBetweenTwoEntity(checkNearestBeamWallLineId, strtPointLineId);

            ObjectId endPointLineId = AEE_Utility.getLineId(newWallPanelLine.EndPoint, newOffsetWallPanelLine.EndPoint, false);
            listOfEndPointInsct = AEE_Utility.InterSectionBetweenTwoEntity(checkNearestBeamWallLineId, endPointLineId);

            AEE_Utility.deleteEntity(strtPointLineId);
            AEE_Utility.deleteEntity(endPointLineId);
            AEE_Utility.deleteEntity(newOffsetWallPanelLineId);
            AEE_Utility.deleteEntity(checkNearestBeamWallLineId);

            //if (listOfStartPointInsct.Count == 0 && listOfEndPointInsct.Count == 0)
            //{
            //    MessageBox.Show("Intersection point is zero");
            //}
        }


        private void setBeamPanelThickLineData(string xDataRegAppNameWallLine, ObjectId wallObjId, ObjectId wallLineId, ObjectId prvsWallLineId, ObjectId wallCornerId1, ObjectId wallCornerId2, ObjectId beamId_InsctWall, double distBtwWallToBeam, Point3d adjacentInsidePointOfBeam, ObjectId adjacentOfBeamId)
        {
            var listBeamExplodeIds = AEE_Utility.ExplodeEntity(beamId_InsctWall);
            WindowHelper windowHlp = new WindowHelper();
            var listOfMinLngthIds = windowHlp.getMinLengthOfWindowLineSegment(listBeamExplodeIds);
            double beamThick = AEE_Utility.GetLengthOfLine(listOfMinLngthIds[0]);
            AEE_Utility.deleteEntity(listOfMinLngthIds);
            var beamEnt = AEE_Utility.GetEntityForRead(beamId_InsctWall);
            CommonModule commonMod = new CommonModule();

            List<Point3d> listOfStrtEndPoint = AEE_Utility.GetStartEndPointOfLine(prvsWallLineId);
            //////Point3d wallLineStrtPoint = AEE_Utility.GetBasePointOfPolyline(wallCornerId1);
            //////Point3d wallLineEndPoint = AEE_Utility.GetBasePointOfPolyline(wallCornerId2);

            Point3d wallLineStrtPoint = listOfStrtEndPoint[0];
            Point3d wallLineEndPoint = listOfStrtEndPoint[1];

            var length = AEE_Utility.GetLengthOfLine(wallLineStrtPoint, wallLineEndPoint);
            length = Math.Truncate(length);
            if (length > 0)
            {
                var beamThickLineId = AEE_Utility.getLineId(beamEnt, wallLineStrtPoint, wallLineEndPoint, false);
                AEE_Utility.AttachXData(beamThickLineId, xDataRegAppNameWallLine, CommonModule.xDataAsciiName);

                listOfBeamBottomPanelLineId.Add(beamThickLineId);
                listOfDistBtwBeamPanelLineToWallLine.Add(Math.Round(distBtwWallToBeam, 1));
                listOfBeamId_ForBeamBotomPanel.Add(beamId_InsctWall);
                listOfWallId_ForBeamBotomPanel.Add(wallObjId);
            }
        }


        public void setBeamBottomWallLineRoomWise()
        {
            if (listOfBeamBottomPanelLineId.Count != 0)
            {
                List<ObjectId> listOfBeamBttmPanelLineId = new List<ObjectId>();
                List<double> listOfListOfDistBtwBeamBttmPanelLineToWallLine = new List<double>();
                List<ObjectId> listOfBeamId_ForBeamBttmPanel = new List<ObjectId>();
                List<ObjectId> listOfWallId_ForBeamBttmPanel = new List<ObjectId>();

                for (int k = 0; k < listOfBeamBottomPanelLineId.Count; k++)
                {
                    listOfBeamBttmPanelLineId.Add(listOfBeamBottomPanelLineId[k]);
                    listOfListOfDistBtwBeamBttmPanelLineToWallLine.Add(listOfDistBtwBeamPanelLineToWallLine[k]);
                    listOfBeamId_ForBeamBttmPanel.Add(listOfBeamId_ForBeamBotomPanel[k]);
                    listOfWallId_ForBeamBttmPanel.Add(listOfWallId_ForBeamBotomPanel[k]);
                }
                listOfListOfBeamBottomPanelLineId.Add(listOfBeamBttmPanelLineId);
                listOfListOfDistBtwBeamPanelLineToWallLine.Add(listOfListOfDistBtwBeamBttmPanelLineToWallLine);
                listOfListOfBeamId_ForBeamBotomPanel.Add(listOfBeamId_ForBeamBttmPanel);
                listOfListOfWallId_ForBeamBotomPanel.Add(listOfWallId_ForBeamBttmPanel);
            }

            listOfBeamBottomPanelLineId.Clear();
            listOfDistBtwBeamPanelLineToWallLine.Clear();
            listOfBeamId_ForBeamBotomPanel.Clear();
            listOfWallId_ForBeamBotomPanel.Clear();
        }


        private void setBeamCornerId_CornerTextId(ObjectId beamCornerId, ObjectId beamCornerTextId, ObjectId wallCornerId)
        {
            CornerHelper cornrHlp = new CornerHelper();

            int indexOfExist = BeamHelper.listOfAllBeamCornerId_ForStretch.IndexOf(beamCornerId);
            if (indexOfExist == -1)
            {
                var beamOffstCornerId = cornrHlp.getOffsetInternalCornerInOutside(beamCornerId);
                BeamHelper.listOfAllWallCornerId_WithBeam.Add(wallCornerId);
                BeamHelper.listOfAllBeamCornerId_ForStretch.Add(beamCornerId);
                BeamHelper.listOfAllBeamOffstCornerId_ForStretch.Add(beamOffstCornerId);
                BeamHelper.listOfAllBeamCornerTextId_ForStretch.Add(beamCornerTextId);
            }
            else
            {
                var beamOffstCornerId = cornrHlp.getOffsetInternalCornerInOutside(beamCornerId);
                //BeamHelper.listOfAllWallCornerId_WithBeam[indexOfExist] = (wallCornerId);
                BeamHelper.listOfAllBeamCornerId_ForStretch[indexOfExist] = beamCornerId;
                BeamHelper.listOfAllBeamOffstCornerId_ForStretch[indexOfExist] = beamOffstCornerId;
                BeamHelper.listOfAllBeamCornerTextId_ForStretch[indexOfExist] = beamCornerTextId;
            }
        }

        //Modified by RTJ May-18 for Beam Max Length.
        //Creating Global Variable....For checking
        List<List<ObjectId>> m_ListOfConnectedBeams = new List<List<ObjectId>>();
        double m_MaxCornerLengthValue = 0;

        public List<ObjectId> ConnectedLinesForBeam(List<ObjectId> beamLines, ObjectId idObj)
        {
            List<ObjectId> idsObjs = new List<ObjectId>();
            SelectConnectedLinesForBeams(beamLines, idsObjs, idObj);
            return idsObjs;
        }
        private void SelectConnectedLinesForBeams(List<ObjectId> beamLines, List<ObjectId> idsObjs, ObjectId idObj)
        {
            // Get the current document and database
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;

            try
            {
                if (idObj != null)
                {
                    using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
                    {
                        Entity entLine = acTrans.GetObject(idObj, OpenMode.ForRead, false) as Entity;

                        Line oLine = entLine as Line;

                        if (oLine != null)
                        {
                            foreach (var iObjId in beamLines)
                            {
                                Line lineEnt = acTrans.GetObject(iObjId, OpenMode.ForRead, false) as Line;
                                // if (IsEqual(oLine.StartPoint, lineEnt.StartPoint) || IsEqual(oLine.StartPoint, lineEnt.EndPoint) ||
                                //IsEqual(oLine.EndPoint, lineEnt.StartPoint) || IsEqual(oLine.EndPoint, lineEnt.EndPoint))
                                // {
                                if (AEE_Utility.IsPointsAreEqual(oLine.StartPoint, lineEnt.StartPoint) || AEE_Utility.IsPointsAreEqual(oLine.StartPoint, lineEnt.EndPoint) ||
                                    AEE_Utility.IsPointsAreEqual(oLine.EndPoint, lineEnt.StartPoint) || AEE_Utility.IsPointsAreEqual(oLine.EndPoint, lineEnt.EndPoint))
                                {
                                    if (!idsObjs.Contains(iObjId))
                                    {
                                        idsObjs.Add(iObjId);
                                        SelectConnectedLinesForBeams(beamLines, idsObjs, iObjId);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            { }
        }

        public void GetMaxCornerLength(int nValue, List<double> listOfDistBtwBeamPanelLineToWallLine, List<ObjectId> iListOfBeamsObj, out double fixedIntrnalCrnrDepth, out double changeIntrnalCrnrDepth)
        {
            fixedIntrnalCrnrDepth = 0.0;
            changeIntrnalCrnrDepth = 0.0;
            m_MaxCornerLengthValue = 0.0;
            if (iListOfBeamsObj.Count != 0)
            {
                if (iListOfBeamsObj.Count == 1)
                {
                    for (int index = 0; index < iListOfBeamsObj.Count; index++)
                    {
                        double distBtwWallToBeam = listOfDistBtwBeamPanelLineToWallLine[nValue];
                        var listOfStrtEndPoint = AEE_Utility.GetStartEndPointOfLine(iListOfBeamsObj[index]);
                        for (int i = 0; i < listOfStrtEndPoint.Count; i++)
                        {
                            Point3d basePoint = listOfStrtEndPoint[i];
                            double distBtwBeamAdjctBttmLine = getDistanceOfAdjacentBeamToWall(i, basePoint, iListOfBeamsObj[index], listOfBeamBottomPanelLineId, listOfDistBtwBeamPanelLineToWallLine, listOfBeamId_ForBeamBotomPanel);
                            List<object> listOfBeamBottomFlangeData = getListOfBeamBottomFlange(distBtwWallToBeam, distBtwBeamAdjctBttmLine, i);
                        }
                    }
                    fixedIntrnalCrnrDepth = m_MaxCornerLengthValue;
                    changeIntrnalCrnrDepth = m_MaxCornerLengthValue;
                }
                else
                {
                    for (int index = 0; index < iListOfBeamsObj.Count; index++)
                    {
                        double distBtwWallToBeam = listOfDistBtwBeamPanelLineToWallLine[index];
                        var listOfStrtEndPoint = AEE_Utility.GetStartEndPointOfLine(iListOfBeamsObj[index]);
                        for (int i = 0; i < listOfStrtEndPoint.Count; i++)
                        {
                            Point3d basePoint = listOfStrtEndPoint[i];
                            double distBtwBeamAdjctBttmLine = getDistanceOfAdjacentBeamToWall(i, basePoint, iListOfBeamsObj[index], listOfBeamBottomPanelLineId, listOfDistBtwBeamPanelLineToWallLine, listOfBeamId_ForBeamBotomPanel);
                            List<object> listOfBeamBottomFlangeData = getListOfBeamBottomFlange(distBtwWallToBeam, distBtwBeamAdjctBttmLine, i);
                        }
                    }
                    fixedIntrnalCrnrDepth = m_MaxCornerLengthValue;
                    changeIntrnalCrnrDepth = m_MaxCornerLengthValue;
                }
            }

        }
        //End...Modified by RTJ May-18 for Beam Max Length.
        public void drawBeamBottomCorner()
        {
            DoorHelper doorHelper = new DoorHelper();

            WindowHelper windowHlp = new WindowHelper();
            ExternalWallHelper externalWallHlp = new ExternalWallHelper();
            KickerHelper kickerHlp = new KickerHelper();
            //Below line added by by RTJ May-18 for Beam Max Length.
            List<List<ObjectId>> listOFBeams = new List<List<ObjectId>>();

            for (int k = 0; k < listOfListOfBeamBottomPanelLineId.Count; k++)
            {
                var listOfBeamBottomPanelLineId = listOfListOfBeamBottomPanelLineId[k];
                var listOfDistBtwBeamPanelLineToWallLine = listOfListOfDistBtwBeamPanelLineToWallLine[k];
                var listOfBeamId_ForBeamBotomPanel = listOfListOfBeamId_ForBeamBotomPanel[k];
                var listOfWallId_ForBeamBotomPanel = listOfListOfWallId_ForBeamBotomPanel[k];
                //Below Code added by by RTJ May-18 for Beam Max Length.
                int nValue = 0;

                for (int nIndex = 0; nIndex < listOfBeamBottomPanelLineId.Count; nIndex++)
                {
                    ObjectId beambottomLineId = listOfBeamBottomPanelLineId[nIndex];
                    bool bContinue = true;
                    foreach (var iObjId in listOFBeams)
                    {
                        List<ObjectId> listOfLines = iObjId;
                        if (listOfLines.Contains(beambottomLineId))
                        {
                            bContinue = false;
                            break;
                        }
                    }
                    if (bContinue == false)
                        continue;

                    List<ObjectId> listOfConnectedBeams = ConnectedLinesForBeam(listOfBeamBottomPanelLineId, beambottomLineId);
                    listOFBeams.Add(listOfConnectedBeams);
                    double fixedIntrnalCrnrDepth = 0.0;
                    double changeIntrnalCrnrDepth = 0.0;
                    GetMaxCornerLength(nValue, listOfDistBtwBeamPanelLineToWallLine, listOfConnectedBeams, out fixedIntrnalCrnrDepth, out changeIntrnalCrnrDepth);
                    if (listOfConnectedBeams.Count == 1)
                    {
                        nValue++;
                    }
                    for (int index = 0; index < listOfConnectedBeams.Count; index++)
                    {
                        beambottomLineId = listOfConnectedBeams[index];
                        //End..Modified  by RTJ May-18 for Beam Max Length.
                        var beambottomLineEnt = AEE_Utility.GetEntityForRead(beambottomLineId);

                        int beamIndx = -1;
                        for (int m = 0; m < listOfBeamId_ForBeamBotomPanel.Count; m++)
                        {
                            if (AEE_Utility.GetPointIsInsideThePolyline(listOfBeamId_ForBeamBotomPanel[m], AEE_Utility.GetMidPoint(beambottomLineEnt)))
                            {
                                beamIndx = m;
                                break;
                            }
                        }
                        if (beamIndx == -1)
                            continue;
                        double distBtwWallToBeam = listOfDistBtwBeamPanelLineToWallLine[beamIndx];

                        //if (distBtwWallToBeam == 0)//Added on 13/03/2023 by SDM to not draw corners for Beam
                        //    continue;
                        ObjectId beamId = listOfBeamId_ForBeamBotomPanel[beamIndx];
                        ObjectId wallId = listOfWallId_ForBeamBotomPanel[beamIndx];
                        ObjectId outsideOffstWallId = externalWallHlp.drawOffsetExternalWall(wallId);
                        double beamThickLineAngle = AEE_Utility.GetAngleOfLine(beambottomLineId);
                        var listOfStrtEndPoint = AEE_Utility.GetStartEndPointOfLine(beambottomLineId);
                        double dLen = listOfStrtEndPoint[0].DistanceTo(listOfStrtEndPoint[1]);
                        double dBeamBottom = Beam_UI_Helper.getOffsetBeamBottom(beambottomLineEnt.Layer);
                        var xDataRegAppName = AEE_Utility.GetXDataRegisterAppName(beambottomLineId);
                        string beamLayer = AEE_Utility.GetLayerName(beamId);

                        //Added on 20/03/2023 by SDM
                        bool isOffsetBeam = beamLayer.Contains(Beam_UI_Helper.offsetBeamStrtText);
                        if (!isOffsetBeam)
                        {
                            dBeamBottom = Beam_UI_Helper.GetBeamLintelLevel(beamLayer, xDataRegAppName);
                            distBtwWallToBeam = Beam_UI_Helper.GetBeamDepth(beamId, beambottomLineId);
                        }//--------------------

                        List<ObjectId> listOfBeamCornerId = new List<ObjectId>();
                        List<ObjectId> listOfBeamCornerMaxLngthLineId = new List<ObjectId>();

                        double flangeDepthOfWallPanelDepth = 0;

                        List<object> listOfBeamBottomPanelData = new List<object>();
                        //Below line added by by RTJ May-18 for Beam Max Length.
                        maxWidthOfCornerLength = 0.0;
                        if (isOffsetBeam)
                            for (int i = 0; i < listOfStrtEndPoint.Count; i++)
                            {
                                Point3d basePoint = listOfStrtEndPoint[i];

                                double distBtwBeamAdjctBttmLine = getDistanceOfAdjacentBeamToWall(i, basePoint, beambottomLineId, listOfBeamBottomPanelLineId, listOfDistBtwBeamPanelLineToWallLine, listOfBeamId_ForBeamBotomPanel);
                                //Below line added by by RTJ May-18 for Beam Max Length.
                                List<object> listOfBeamBottomFlangeData = getListOfBeamBottomFlange(distBtwWallToBeam, distBtwBeamAdjctBttmLine, i);

                                listOfBeamBottomFlangeData[6] = distBtwWallToBeam - changeIntrnalCrnrDepth;

                                double beamBttmCrnrChangeFlange = Convert.ToDouble(listOfBeamBottomFlangeData[0]);
                                double beamBttmCrnrFixedFlange = Convert.ToDouble(listOfBeamBottomFlangeData[1]);
                                //Below lines Commented  by RTJ May-18 for Beam Max Length.
                                //double fixedIntrnalCrnrDepth = Convert.ToDouble(listOfBeamBottomFlangeData[2]);
                                //double changeIntrnalCrnrDepth = Convert.ToDouble(listOfBeamBottomFlangeData[3]);
                                bool flagOfBeamBottomPanel = Convert.ToBoolean(listOfBeamBottomFlangeData[4]);
                                bool flagOfSmallestBeamDist = Convert.ToBoolean(listOfBeamBottomFlangeData[5]);
                                double beamBottomPanelDeth = Convert.ToDouble(listOfBeamBottomFlangeData[6]);

                                if (i == 0)
                                {
                                    flangeDepthOfWallPanelDepth = changeIntrnalCrnrDepth;
                                    listOfBeamBottomPanelData.Add(fixedIntrnalCrnrDepth);
                                    listOfBeamBottomPanelData.Add(changeIntrnalCrnrDepth);
                                    listOfBeamBottomPanelData.Add(beamBttmCrnrFixedFlange);
                                    listOfBeamBottomPanelData.Add(beamBttmCrnrChangeFlange);
                                    listOfBeamBottomPanelData.Add(beamBottomPanelDeth);
                                    listOfBeamBottomPanelData.Add(distBtwWallToBeam);
                                    listOfBeamBottomPanelData.Add(flagOfBeamBottomPanel);
                                    listOfBeamBottomPanelData.Add(flagOfSmallestBeamDist);
                                }

                                ObjectId maxLengthCrnrLineId = new ObjectId();
                                ObjectId beamBttmCornerId = getInternalCornerIdIsInside(basePoint, xDataRegAppName, beamBttmCrnrChangeFlange, beamBttmCrnrFixedFlange, changeIntrnalCrnrDepth, fixedIntrnalCrnrDepth, outsideOffstWallId, beamThickLineAngle, out maxLengthCrnrLineId);

                                AEE_Utility.MoveEntity(beamBttmCornerId, CreateShellPlanHelper.moveVector_ForBeamBottmLayout);

                                ObjectId existBeamBttmCrnrId = new ObjectId();
                                bool flagOfCornerExist = checkBeamBottomCornerExistInSamePoint(beamBttmCornerId, beamId, out existBeamBttmCrnrId);

                                if (flagOfCornerExist == false)
                                {
                                    beamBttmCrnrChangeFlange = changeBeamBottomCornerFlange(distBtwBeamAdjctBttmLine, beamBttmCornerId, beamBttmCrnrChangeFlange, changeIntrnalCrnrDepth);

                                    //Determine the adjacent wall by SDM 07-06-2022
                                    //start
                                    string wall1 = "";
                                    string wall2 = "";
                                    double dist2 = dLen;
                                    var walllines = InternalWallHelper.listOfListOfInternalWallRoomLineId_With_WallName;
                                    for (int r = 0; r < walllines.Count; r++)
                                    {
                                        for (int w = 0; w < walllines[r].Count; w++)
                                        {
                                            var wall = AEE_Utility.GetLine(walllines[r][w]);
                                            var name = AEE_Utility.GetXDataRegisterAppName(walllines[r][w]);

                                            if (name != xDataRegAppName)
                                            {
                                                if (wall.StartPoint.IsEqualTo(basePoint))
                                                {
                                                    wall1 = name;
                                                    wall2 = xDataRegAppName;
                                                    dist2 = dLen - beamBttmCrnrFixedFlange;
                                                }
                                                else if (wall.EndPoint.IsEqualTo(basePoint))
                                                {
                                                    wall1 = xDataRegAppName;
                                                    wall2 = name;
                                                    dist2 = wall.Length - beamBttmCrnrChangeFlange;
                                                }
                                            }

                                        }
                                    }
                                    //end
                                    CornerElevation elev = new CornerElevation(wall1, wall2, 0, dist2, dBeamBottom - CommonModule.internalCorner);
                                    writeBeamBottomInternalCornerText(xDataRegAppName, beamBttmCornerId, maxLengthCrnrLineId, beamThickLineAngle, beamBttmCrnrChangeFlange, beamBttmCrnrFixedFlange, changeIntrnalCrnrDepth, fixedIntrnalCrnrDepth, flagOfSmallestBeamDist, elev);
                                }

                                else
                                {
                                    AEE_Utility.MoveEntity(maxLengthCrnrLineId, CreateShellPlanHelper.moveVector_ForBeamBottmLayout);
                                    AEE_Utility.changeVisibility(beamBttmCornerId, false);
                                    //AEE_Utility.deleteEntity(beamBttmCornerId);
                                    //beamBttmCornerId = existBeamBttmCrnrId;
                                }

                                // Modify the wall panel line with Beam Bottom Corner size

                                if (AEE_Utility.GetEntityForRead(beamBttmCornerId).Visible == true)
                                {
                                    AEE_Utility.MoveEntity(beamBttmCornerId, CreateShellPlanHelper.moveVector_ForBeamBottmLayout * -1);

                                    ObjectId modifiedLineId = new ObjectId();
                                    ObjectId modifiedNxtLineId = new ObjectId();

                                    ObjectId modifiedOffsetLineId = new ObjectId();
                                    ObjectId modifiedOffsetNxtLineId = new ObjectId();
                                    double modifiedLineIdLength = 0;
                                    double modifiedNxtLineIdLength = 0;

                                    Entity lineIdEntity = null;
                                    Entity nxtLineIdEntity = null;

                                    List<Point3d> listOfOffsetLineStrtEndpts = new List<Point3d>();
                                    Vector3d diffpoint3d = new Vector3d();

                                    for (int kl = 0; kl < InternalWallHelper.listOfLinesBtwTwoCrners_ObjId.Count; kl++)
                                    {
                                        ObjectId lineId = InternalWallHelper.listOfLinesBtwTwoCrners_ObjId[kl];

                                        List<Point3d> listOfLineStrtEndPts3d = AEE_Utility.GetStartEndPointOfLine(lineId);

                                        if (AEE_Utility.GetPointIsInsideThePolyline(beamBttmCornerId, listOfLineStrtEndPts3d[0]) &&
                                            AEE_Utility.GetPointIsInsideThePolyline(beamBttmCornerId, listOfLineStrtEndPts3d[1]) && AEE_Utility.GetXDataRegisterAppName(lineId)==xDataRegAppName)//Changes made on 29/05/2023 by SDM
                                        {
                                            bool isEndptsMatching = false;

                                            for (int mn = 0; mn < InternalWallHelper.listOfLinesBtwTwoCrners_ObjId.Count; mn++)
                                            {
                                                ObjectId nxtLineId = InternalWallHelper.listOfLinesBtwTwoCrners_ObjId[mn];

                                                if (lineId != nxtLineId)
                                                {
                                                    lineIdEntity = AEE_Utility.GetEntityForRead(lineId);
                                                    nxtLineIdEntity = AEE_Utility.GetEntityForRead(nxtLineId);
                                                    List<Point3d> listOfInsctPts = new List<Point3d>();
                                                    List<Point3d> listOfNxtLineStrtEndPts3d = AEE_Utility.GetStartEndPointOfLine(nxtLineId);

                                                    if (listOfLineStrtEndPts3d[0].IsEqualTo(listOfNxtLineStrtEndPts3d[0]))
                                                    {
                                                        isEndptsMatching = true;

                                                        listOfOffsetLineStrtEndpts = AEE_Utility.GetStartEndPointOfLine(InternalWallHelper.listOfOffsetLinesBtwTwoCrners_ObjId[kl]);

                                                        diffpoint3d = (listOfOffsetLineStrtEndpts[1] - listOfLineStrtEndPts3d[1]);

                                                        listOfInsctPts = AEE_Utility.InterSectionBetweenTwoEntity(beamBttmCornerId, nxtLineId);
                                                        if (listOfInsctPts.Count == 0)
                                                            continue;
                                                        modifiedLineId = AEE_Utility.CreateLine(listOfInsctPts[0], listOfLineStrtEndPts3d[1], lineIdEntity.Layer, lineIdEntity.ColorIndex);
                                                        modifiedNxtLineId = AEE_Utility.CreateLine(listOfInsctPts[0], listOfNxtLineStrtEndPts3d[1], nxtLineIdEntity.Layer, nxtLineIdEntity.ColorIndex);

                                                        modifiedOffsetLineId = AEE_Utility.CreateLine(listOfInsctPts[0] + diffpoint3d, listOfLineStrtEndPts3d[1] + diffpoint3d, lineIdEntity.Layer, lineIdEntity.ColorIndex);
                                                        modifiedOffsetNxtLineId = AEE_Utility.CreateLine(listOfInsctPts[0] + diffpoint3d, listOfNxtLineStrtEndPts3d[1] + diffpoint3d, nxtLineIdEntity.Layer, nxtLineIdEntity.ColorIndex);

                                                    }
                                                    else if (listOfLineStrtEndPts3d[1].IsEqualTo(listOfNxtLineStrtEndPts3d[0]))
                                                    {
                                                        isEndptsMatching = true;

                                                        listOfOffsetLineStrtEndpts = AEE_Utility.GetStartEndPointOfLine(InternalWallHelper.listOfOffsetLinesBtwTwoCrners_ObjId[kl]);

                                                        diffpoint3d = (listOfOffsetLineStrtEndpts[1] - listOfLineStrtEndPts3d[1]);

                                                        listOfInsctPts = AEE_Utility.InterSectionBetweenTwoEntity(beamBttmCornerId, nxtLineId);
                                                        if (listOfInsctPts.Count == 0)
                                                            continue;
                                                        modifiedLineId = AEE_Utility.CreateLine(listOfLineStrtEndPts3d[0], listOfInsctPts[0], lineIdEntity.Layer, lineIdEntity.ColorIndex);
                                                        modifiedNxtLineId = AEE_Utility.CreateLine(listOfInsctPts[0], listOfNxtLineStrtEndPts3d[1], nxtLineIdEntity.Layer, nxtLineIdEntity.ColorIndex);

                                                        modifiedOffsetLineId = AEE_Utility.CreateLine(listOfLineStrtEndPts3d[0] + diffpoint3d, listOfInsctPts[0] + diffpoint3d, lineIdEntity.Layer, lineIdEntity.ColorIndex);
                                                        modifiedOffsetNxtLineId = AEE_Utility.CreateLine(listOfInsctPts[0] + diffpoint3d, listOfNxtLineStrtEndPts3d[1] + diffpoint3d, nxtLineIdEntity.Layer, nxtLineIdEntity.ColorIndex);

                                                    }
                                                    else if (listOfLineStrtEndPts3d[0].IsEqualTo(listOfNxtLineStrtEndPts3d[1]))
                                                    {
                                                        isEndptsMatching = true;

                                                        listOfOffsetLineStrtEndpts = AEE_Utility.GetStartEndPointOfLine(InternalWallHelper.listOfOffsetLinesBtwTwoCrners_ObjId[kl]);

                                                        diffpoint3d = (listOfOffsetLineStrtEndpts[1] - listOfLineStrtEndPts3d[1]);

                                                        listOfInsctPts = AEE_Utility.InterSectionBetweenTwoEntity(beamBttmCornerId, nxtLineId);
                                                        if (listOfInsctPts.Count == 0)
                                                            continue;
                                                        modifiedLineId = AEE_Utility.CreateLine(listOfInsctPts[0], listOfLineStrtEndPts3d[1], lineIdEntity.Layer, lineIdEntity.ColorIndex);
                                                        modifiedNxtLineId = AEE_Utility.CreateLine(listOfNxtLineStrtEndPts3d[0], listOfInsctPts[0], nxtLineIdEntity.Layer, nxtLineIdEntity.ColorIndex);

                                                        modifiedOffsetLineId = AEE_Utility.CreateLine(listOfInsctPts[0] + diffpoint3d, listOfLineStrtEndPts3d[1] + diffpoint3d, lineIdEntity.Layer, lineIdEntity.ColorIndex);
                                                        modifiedOffsetNxtLineId = AEE_Utility.CreateLine(listOfNxtLineStrtEndPts3d[0] + diffpoint3d, listOfInsctPts[0] + diffpoint3d, nxtLineIdEntity.Layer, nxtLineIdEntity.ColorIndex);


                                                    }
                                                    else if (listOfLineStrtEndPts3d[1].IsEqualTo(listOfNxtLineStrtEndPts3d[1]))
                                                    {
                                                        isEndptsMatching = true;

                                                        listOfOffsetLineStrtEndpts = AEE_Utility.GetStartEndPointOfLine(InternalWallHelper.listOfOffsetLinesBtwTwoCrners_ObjId[kl]);

                                                        diffpoint3d = (listOfOffsetLineStrtEndpts[1] - listOfLineStrtEndPts3d[1]);

                                                        listOfInsctPts = AEE_Utility.InterSectionBetweenTwoEntity(beamBttmCornerId, nxtLineId);
                                                        if (listOfInsctPts.Count == 0)
                                                            continue;
                                                        modifiedLineId = AEE_Utility.CreateLine(listOfLineStrtEndPts3d[0], listOfInsctPts[0], lineIdEntity.Layer, lineIdEntity.ColorIndex);
                                                        modifiedNxtLineId = AEE_Utility.CreateLine(listOfNxtLineStrtEndPts3d[0], listOfInsctPts[0], nxtLineIdEntity.Layer, nxtLineIdEntity.ColorIndex);

                                                        modifiedOffsetLineId = AEE_Utility.CreateLine(listOfLineStrtEndPts3d[0] + diffpoint3d, listOfInsctPts[0] + diffpoint3d, lineIdEntity.Layer, lineIdEntity.ColorIndex);
                                                        modifiedOffsetNxtLineId = AEE_Utility.CreateLine(listOfNxtLineStrtEndPts3d[0] + diffpoint3d, listOfInsctPts[0] + diffpoint3d, nxtLineIdEntity.Layer, nxtLineIdEntity.ColorIndex);
                                                    }

                                                    if (isEndptsMatching == true && modifiedLineId.IsValid)
                                                    {
                                                        string wallName = AEE_Utility.GetXDataRegisterAppName(lineId);
                                                        AEE_Utility.AttachXData(modifiedLineId, wallName, CommonModule.xDataAsciiName);

                                                        wallName = AEE_Utility.GetXDataRegisterAppName(nxtLineId);
                                                        AEE_Utility.AttachXData(modifiedNxtLineId, wallName, CommonModule.xDataAsciiName);

                                                        AEE_Utility.deleteEntity(lineId);
                                                        AEE_Utility.deleteEntity(nxtLineId);

                                                        AEE_Utility.deleteEntity(InternalWallHelper.listOfOffsetLinesBtwTwoCrners_ObjId[kl]);
                                                        AEE_Utility.deleteEntity(InternalWallHelper.listOfOffsetLinesBtwTwoCrners_ObjId[mn]);

                                                        InternalWallHelper.listOfLinesBtwTwoCrners_ObjId[kl] = modifiedLineId;
                                                        InternalWallHelper.listOfLinesBtwTwoCrners_ObjId[mn] = modifiedNxtLineId;

                                                        InternalWallHelper.listOfOffsetLinesBtwTwoCrners_ObjId[kl] = modifiedOffsetLineId;
                                                        InternalWallHelper.listOfOffsetLinesBtwTwoCrners_ObjId[mn] = modifiedOffsetNxtLineId;


                                                        modifiedLineIdLength = Math.Round(AEE_Utility.GetLengthOfLine(modifiedLineId), 1);
                                                        modifiedNxtLineIdLength = Math.Round(AEE_Utility.GetLengthOfLine(modifiedNxtLineId), 1);

                                                        InternalWallHelper.listOfDistnceAndObjId_BtwTwoCrners[kl] = Convert.ToString(modifiedLineIdLength) + "," + Convert.ToString(modifiedLineId);
                                                        InternalWallHelper.listOfDistnceAndObjId_BtwTwoCrners[mn] = Convert.ToString(modifiedNxtLineIdLength) + "," + Convert.ToString(modifiedNxtLineId);

                                                        InternalWallHelper.listOfDistnceBtwTwoCrners[kl] = modifiedLineIdLength;
                                                        InternalWallHelper.listOfDistnceBtwTwoCrners[mn] = modifiedNxtLineIdLength;

                                                        break;
                                                    }
                                                }
                                            }
                                            //break;//Fix WP split by SDM 2022-09-13
                                        }
                                    }

                                    AEE_Utility.MoveEntity(beamBttmCornerId, CreateShellPlanHelper.moveVector_ForBeamBottmLayout);
                                }
                                listOfBeamCornerId.Add(beamBttmCornerId);
                                listOfBeamCornerMaxLngthLineId.Add(maxLengthCrnrLineId);

                            }
                        else if (!listOfBeamId_BottomPanelLineDrawn.Contains(beamId))//Added on 20/03/2023 by SDM
                        {
                            AEE_Utility.AttachXData(beamId, xDataRegAppName, CommonModule.xDataAsciiName);
                            List<ObjectId> listOfExplodeLineId = AEE_Utility.ExplodeEntity(beamId);
                            List<ObjectId> listOfMinLengthExplodeLine = windowHlp.getMinLengthOfWindowLineSegment(listOfExplodeLineId);
                            for (int m = 0; m < listOfMinLengthExplodeLine.Count; m++)
                            {
                                ObjectId doorWallThicknessId = listOfMinLengthExplodeLine[m];
                                bool flagOfDoorWallPanelCreate = doorHelper.drawDoorWallPanel_In_DoorThick_With_Corner(beamId, doorWallThicknessId, CornerHelper.listOfCornerId_InsctToBeam);
                                if (flagOfDoorWallPanelCreate == false)
                                {
                                    var newDoorThickLineId = AEE_Utility.createColonEntityInSamePoint(doorWallThicknessId, false);
                                    var newOffsetDoorThickLineId = doorHelper.getOffsetDoorWallThickLineId(newDoorThickLineId, beamId);
                                    doorHelper.setDoorWallThickLineId(xDataRegAppName, newDoorThickLineId, newOffsetDoorThickLineId, beamId);

                                }
                                listOfBeamId_BottomPanelLineDrawn.Add(beamId);
                            }
                            AEE_Utility.deleteEntity(listOfExplodeLineId);

                        }//-------------------

                        List<ObjectId> listOfBeamBottomWallPanelLineId = getBeamBottomWallPanelLine(xDataRegAppName, beambottomLineEnt, listOfBeamCornerId, flangeDepthOfWallPanelDepth);
                        if (listOfBeamCornerId.Count == 2)
                        {
                            listOfListOfBeamBottomCornerId.Add(listOfBeamCornerId);
                            listOfListOfBeamBottomWallPanelId.Add(listOfBeamBottomWallPanelLineId);
                            listOfListOfBeamBottomPanelData.Add(listOfBeamBottomPanelData);
                            listOfListOfBeamCornerMaxLngthLineId.Add(listOfBeamCornerMaxLngthLineId);
                            listOfListOfBeamlvls.Add(dBeamBottom);
                        }
                    }

                }
            }

        }
        private List<ObjectId> getBeamBottomWallPanelLine(string xDataRegAppName, Entity beambottomLineEnt, List<ObjectId> listOfBeamCornerId, double offsetValue)
        {
            List<ObjectId> listOfBeamBottomPanelLineId = new List<ObjectId>();

            if (listOfBeamCornerId.Count == 2)
            {
                ObjectId beamCrnrId1 = listOfBeamCornerId[0];
                var beamCrnr1_BasePnt = AEE_Utility.GetBasePointOfPolyline(beamCrnrId1);

                ObjectId beamCrnrId2 = listOfBeamCornerId[1];
                var beamCrnr2_BasePnt = AEE_Utility.GetBasePointOfPolyline(beamCrnrId2);

                var lineId = AEE_Utility.getLineId(beamCrnr1_BasePnt, beamCrnr2_BasePnt, false);

                var listOfLine1_InsctPnt = AEE_Utility.InterSectionBetweenTwoEntity(beamCrnrId1, lineId);

                List<double> listOfBeamCrnr1_Lngth = new List<double>();
                foreach (var insct1 in listOfLine1_InsctPnt)
                {
                    var length = AEE_Utility.GetLengthOfLine(insct1, beamCrnr1_BasePnt);
                    listOfBeamCrnr1_Lngth.Add(length);
                }
                if (listOfBeamCrnr1_Lngth.Count != 0)
                {
                    double maxLngthCrnr1 = listOfBeamCrnr1_Lngth.Max();
                    int indexOfMaxLngthCrnr1 = listOfBeamCrnr1_Lngth.IndexOf(maxLngthCrnr1);
                    beamCrnr1_BasePnt = listOfLine1_InsctPnt[indexOfMaxLngthCrnr1];
                }

                var listOfLine2_InsctPnt = AEE_Utility.InterSectionBetweenTwoEntity(beamCrnrId2, lineId);
                List<double> listOfBeamCrnr2_Lngth = new List<double>();
                foreach (var insct2 in listOfLine2_InsctPnt)
                {
                    var length = AEE_Utility.GetLengthOfLine(insct2, beamCrnr2_BasePnt);
                    listOfBeamCrnr2_Lngth.Add(length);
                }
                if (listOfBeamCrnr2_Lngth.Count != 0)
                {
                    double maxLngthCrnr2 = listOfBeamCrnr2_Lngth.Max();
                    int indexOfMaxLngthCrnr2 = listOfBeamCrnr2_Lngth.IndexOf(maxLngthCrnr2);
                    beamCrnr2_BasePnt = listOfLine2_InsctPnt[indexOfMaxLngthCrnr2];
                }

                var beamBottomWallLineId = AEE_Utility.getLineId(beambottomLineEnt, beamCrnr1_BasePnt, beamCrnr2_BasePnt, false);
                AEE_Utility.AttachXData(beamBottomWallLineId, xDataRegAppName, CommonModule.xDataAsciiName);

                ObjectId beamBottomWallOffstLineId = new ObjectId();
                var offsetLineId = AEE_Utility.OffsetLine(lineId, 5, false);
                var listOfInsct = AEE_Utility.InterSectionBetweenTwoEntity(beamCrnrId1, offsetLineId);
                if (listOfInsct.Count == 0)
                {
                    beamBottomWallOffstLineId = AEE_Utility.OffsetLine(beamBottomWallLineId, -offsetValue, false);
                }
                else
                {
                    beamBottomWallOffstLineId = AEE_Utility.OffsetLine(beamBottomWallLineId, -offsetValue, false);
                }
                listOfBeamBottomPanelLineId.Add(beamBottomWallLineId);
                listOfBeamBottomPanelLineId.Add(beamBottomWallOffstLineId);

                AEE_Utility.deleteEntity(lineId);
                AEE_Utility.deleteEntity(offsetLineId);
            }
            return listOfBeamBottomPanelLineId;
        }


        private bool checkBeamBottomCornerExistInSamePoint(ObjectId beamBttmCornerId, ObjectId adjacentOfBeamId, out ObjectId existBeamBttmCrnrId)
        {
            bool flagOfCornerExist = false;
            existBeamBttmCrnrId = new ObjectId();
            if (adjacentOfBeamId.IsValid == true)
            {
                for (int index = 0; index < listOfAllBeamBottomCornerId.Count; index++)
                {
                    ObjectId prvsBeamBttmCornerId = listOfAllBeamBottomCornerId[index];
                    var listOfInsctPoint = AEE_Utility.InterSectionBetweenTwoEntity(beamBttmCornerId, prvsBeamBttmCornerId);
                    if (listOfInsctPoint.Count != 0)
                    {
                        flagOfCornerExist = true;
                        existBeamBttmCrnrId = prvsBeamBttmCornerId;
                        break;
                    }
                }
            }

            if (flagOfCornerExist == false)
            {
                listOfAllBeamBottomCornerId.Add(beamBttmCornerId);
            }
            return flagOfCornerExist;
        }


        private double getDistanceOfAdjacentBeamToWall(int index, Point3d beamBttmLinePoint, ObjectId beambottomLineId, List<ObjectId> listOfBeamBottomPanelLineId, List<double> listOfDistBtwBeamPanelLineToWallLine, List<ObjectId> listOfBeamId_ForBeamBotomPanel)
        {
            double distBtwBeamAdjctBttmLine = 0;
            ObjectId adjacentBeamId = new ObjectId();

            // if index = 0, beamBttmLinePoint = start point
            // else  beamBttmLinePoint = end point

            double difference = 1;
            for (int k = 0; k < listOfBeamBottomPanelLineId.Count; k++)
            {
                bool flagOfAdjacentBeam = false;
                ObjectId beamAdjctBttmLineId = listOfBeamBottomPanelLineId[k];
                if (beambottomLineId != beamAdjctBttmLineId)
                {
                    var listOfAdjctBeamLinePoint = AEE_Utility.GetStartEndPointOfLine(beamAdjctBttmLineId);
                    if (index == 0)
                    {
                        var adjacentBeamPoint = listOfAdjctBeamLinePoint[1];
                        var length = AEE_Utility.GetLengthOfLine(adjacentBeamPoint, beamBttmLinePoint);
                        if (length <= difference)
                        {
                            //AEE_Utility.CreateCircle(adjacentBeamPoint.X, adjacentBeamPoint.Y, 0, 30, "A", 2);
                            flagOfAdjacentBeam = true;
                        }
                    }
                    else
                    {
                        var adjacentBeamPoint = listOfAdjctBeamLinePoint[0];
                        var length = AEE_Utility.GetLengthOfLine(adjacentBeamPoint, beamBttmLinePoint);
                        if (length <= difference)
                        {
                            //AEE_Utility.CreateCircle(adjacentBeamPoint.X, adjacentBeamPoint.Y, 0, 30, "A", 2);
                            flagOfAdjacentBeam = true;
                        }
                    }
                    if (flagOfAdjacentBeam == true)
                    {
                        distBtwBeamAdjctBttmLine = listOfDistBtwBeamPanelLineToWallLine[k];
                        adjacentBeamId = listOfBeamId_ForBeamBotomPanel[k];
                        break;
                    }
                }
            }
            return distBtwBeamAdjctBttmLine;
        }


        private double changeBeamBottomCornerFlange(double distBtwBeamAdjctBttmLine, ObjectId beamBttmCornerId, double beamBttmCrnrChangeFlange, double changeIntrnalCrnrDepth)
        {
            if (distBtwBeamAdjctBttmLine != 0)
            {
                double beamBttmCornerFixedFlange = CommonModule.beamBttmCornerFixedFlange;
                double diff = beamBttmCornerFixedFlange - beamBttmCrnrChangeFlange;

                if (Math.Abs(diff) > 0)
                {
                    List<ObjectId> listOfCornerExplodeId = AEE_Utility.ExplodeEntity(beamBttmCornerId);
                    ObjectId flange2_Id = listOfCornerExplodeId[listOfCornerExplodeId.Count - 1];
                    double flange2 = AEE_Utility.GetLengthOfLine(flange2_Id);

                    ObjectId thickCornerLineId = new ObjectId();
                    ObjectId thickCornerOffstLineId = new ObjectId();

                    Point3d crnrPt3d = AEE_Utility.GetBasePointOfPolyline(beamBttmCornerId);

                    for (int k = 0; k < listOfCornerExplodeId.Count; k++)
                    {
                        flange2_Id = listOfCornerExplodeId[k];
                        flange2 =Math.Round(AEE_Utility.GetLengthOfLine(flange2_Id));

                        List<Point3d> listOfFlangePoints = AEE_Utility.GetStartEndPointOfLine(flange2_Id);

                        int matchIndx = -1;

                        if (flange2 ==Math.Round(beamBttmCrnrChangeFlange) && AEE_Utility.IsPoint3dPresentInPoint3dList(listOfFlangePoints, crnrPt3d, out matchIndx))
                        {
                            if (k <= 1)
                                thickCornerLineId = listOfCornerExplodeId[(k + 1)];
                            else
                                thickCornerLineId = listOfCornerExplodeId[(k - 1)];

                            break;
                        }
                    }
                    if (!thickCornerLineId.IsValid)
                        return beamBttmCornerFixedFlange;
                    var offsetId = AEE_Utility.OffsetLine(thickCornerLineId, 3, false);
                    var midPoint = AEE_Utility.GetMidPoint(offsetId);
                    var flagOfInside = AEE_Utility.GetPointIsInsideThePolyline(beamBttmCornerId, midPoint);

                    if (flagOfInside == true)
                        thickCornerOffstLineId = AEE_Utility.OffsetLine(thickCornerLineId, -diff, false);
                    else
                        thickCornerOffstLineId = AEE_Utility.OffsetLine(thickCornerLineId, diff, false);

                    List<Point3d> listOfLinePoint = AEE_Utility.GetStartEndPointOfLine(thickCornerLineId);
                    List<Point3d> listOfOffsetLinePoint = AEE_Utility.GetStartEndPointOfLine(thickCornerOffstLineId);
                    List<Point2d> listOfCornerVertice = AEE_Utility.GetPolylineVertexPoint(beamBttmCornerId);

                    for (int i = 0; i < listOfCornerVertice.Count; i++)
                    {
                        Point2d crnrPoint = listOfCornerVertice[i];
                        for (int j = 0; j < listOfLinePoint.Count; j++)
                        {
                            Point3d linePoint = listOfLinePoint[j];
                            Point3d offstLinePoint = listOfOffsetLinePoint[j];
                            var length = AEE_Utility.GetLengthOfLine(crnrPoint.X, crnrPoint.Y, linePoint.X, linePoint.Y);
                            if (length <= 2)
                            {
                                Point2d offsetLinePnt = new Point2d(offstLinePoint.X, offstLinePoint.Y);
                                listOfCornerVertice[i] = offsetLinePnt;
                                break;
                            }
                        }
                    }

                    CornerHelper cornerHlp = new CornerHelper();
                    cornerHlp.reDrawCorner(beamBttmCornerId, listOfCornerVertice);

                    AEE_Utility.deleteEntity(listOfCornerExplodeId);
                    AEE_Utility.deleteEntity(offsetId);
                    AEE_Utility.deleteEntity(thickCornerOffstLineId);
                }

                return beamBttmCornerFixedFlange;
            }

            return beamBttmCrnrChangeFlange;
        }

        //Below Function is modified to pass one more parameter added  by RTJ May-18 for Beam Max Length.
        private List<object> getListOfBeamBottomFlange(double distBtwWallToBeam, double distBtwBeamAdjctBttmLine, int nIndex)
        {
            List<object> listOfBeamBottomFlangeData = new List<object>();

            double beamBttmCrnrChangeFlange = CommonModule.beamBttmCornerChangeFlange;
            double beamBttmCrnrFixedFlange = CommonModule.beamBttmCornerFixedFlange;

            double fixedIntrnalCrnrDepth = CommonModule.internalCorner;
            double changeIntrnalCrnrDepth = 0;


            bool flagOfBeamBottomPanel = false;
            bool flagOfSmallestBeamDist = false;

            double beamBottomPanelDeth = 0;
            if (distBtwWallToBeam > CommonModule.internalCorner_MaxLngth)
            {
                changeIntrnalCrnrDepth = CommonModule.internalCorner;

                flagOfBeamBottomPanel = true;
                beamBottomPanelDeth = distBtwWallToBeam - CommonModule.internalCorner;
            }
            else if (distBtwWallToBeam <= CommonModule.minOfBeamOffset)
            {
                flagOfSmallestBeamDist = true;
                changeIntrnalCrnrDepth = distBtwWallToBeam + CommonModule.externalCorner;
            }
            else
            {
                changeIntrnalCrnrDepth = distBtwWallToBeam;
            }

            if (distBtwWallToBeam > CommonModule.distBtwBeamCrnrToWall)
            {
                beamBttmCrnrChangeFlange = distBtwWallToBeam + CommonModule.beamBottomAddValueOfIntrnalCrnr;
            }

            if (distBtwBeamAdjctBttmLine != 0)
            {
                if (distBtwBeamAdjctBttmLine > CommonModule.internalCorner_MaxLngth)
                {
                    fixedIntrnalCrnrDepth = CommonModule.internalCorner;
                }
                else if (distBtwBeamAdjctBttmLine <= CommonModule.minOfBeamOffset)
                {
                    fixedIntrnalCrnrDepth = distBtwBeamAdjctBttmLine + CommonModule.externalCorner;
                }
                else
                {
                    fixedIntrnalCrnrDepth = distBtwBeamAdjctBttmLine;
                }

                //beamBttmCrnrChangeFlange = CommonModule.beamBttmCornerFixedFlange;
                ////if (distBtwBeamAdjctBttmLine > CommonModule.distBtwBeamCrnrToWall)
                ////{
                ////    beamBttmCrnrChangeFlange = beamBttmCrnrChangeFlange + CommonModule.beamBottomAddValueOfIntrnalCrnr;
                ////}
            }
            //Plan width dimention on both sides is same [Fixed Below]12-05-2021 fixed by RTJ
            if (changeIntrnalCrnrDepth > fixedIntrnalCrnrDepth)
            {
                fixedIntrnalCrnrDepth = changeIntrnalCrnrDepth;
                //Below lines  added by by RTJ May-18 for Beam Max Length.
                if (nIndex == 0)
                {
                    maxWidthOfCornerLength = changeIntrnalCrnrDepth;
                    if (m_MaxCornerLengthValue < changeIntrnalCrnrDepth)
                    {
                        m_MaxCornerLengthValue = changeIntrnalCrnrDepth;
                    }
                }
            }
            else
            {
                changeIntrnalCrnrDepth = fixedIntrnalCrnrDepth;
                if (nIndex == 0)
                {
                    maxWidthOfCornerLength = changeIntrnalCrnrDepth;
                    if (m_MaxCornerLengthValue < changeIntrnalCrnrDepth)
                    {
                        m_MaxCornerLengthValue = changeIntrnalCrnrDepth;
                    }
                }
            }
            //Check the condition if the the FixedInternalCrnrDepth and changeInternalCrnrDepth..
            //if (nIndex == 1)
            //{
            //    if (maxWidthOfCornerLength > fixedIntrnalCrnrDepth && maxWidthOfCornerLength > changeIntrnalCrnrDepth)
            //    {
            //        fixedIntrnalCrnrDepth = maxWidthOfCornerLength;
            //        changeIntrnalCrnrDepth = maxWidthOfCornerLength;
            //    }
            //}

            if (m_MaxCornerLengthValue < fixedIntrnalCrnrDepth || m_MaxCornerLengthValue < changeIntrnalCrnrDepth)
            {
                if (fixedIntrnalCrnrDepth > changeIntrnalCrnrDepth)
                {
                    m_MaxCornerLengthValue = fixedIntrnalCrnrDepth;
                }
                else
                {
                    m_MaxCornerLengthValue = changeIntrnalCrnrDepth;
                }
                //End....  added by by RTJ May-18 for Beam Max Length.
            }
            //Plan width dimention on both sides is same [Fixed Below]12-05-2021 fixed by RTJ

            listOfBeamBottomFlangeData.Add(beamBttmCrnrChangeFlange);
            listOfBeamBottomFlangeData.Add(beamBttmCrnrFixedFlange);
            listOfBeamBottomFlangeData.Add(fixedIntrnalCrnrDepth);
            listOfBeamBottomFlangeData.Add(changeIntrnalCrnrDepth);
            listOfBeamBottomFlangeData.Add(flagOfBeamBottomPanel);
            listOfBeamBottomFlangeData.Add(flagOfSmallestBeamDist);
            listOfBeamBottomFlangeData.Add(beamBottomPanelDeth);
            return listOfBeamBottomFlangeData;
        }

        private ObjectId getInternalCornerIdIsInside(Point3d basePoint, string xDataRegAppName, double beamBttmCrnrChangeFlange, double beamBttmCrnrFixedFlange, double changeIntrnalCrnrDepth, double fixedIntrnalCrnrDepth, ObjectId outsideOffstWallId, double beamThickLineAngle, out ObjectId maxLengthCrnrLineId)
        {
            maxLengthCrnrLineId = new ObjectId();
            PartHelper partHlp = new PartHelper();

            ObjectId externalCornerId = partHlp.drawBeamBottomInternalCorner(xDataRegAppName, basePoint.X, basePoint.Y, beamBttmCrnrChangeFlange, beamBttmCrnrFixedFlange, fixedIntrnalCrnrDepth, changeIntrnalCrnrDepth, CommonModule.beamCornerLayerName, CommonModule.beamCornerLayerColor);
            AEE_Utility.AttachXData(externalCornerId, xDataRegAppName, CommonModule.xDataAsciiName);
            AEE_Utility.AttachXData(externalCornerId, xDataRegAppName + "_1", beamBttmCrnrFixedFlange.ToString() + "," +
                                                                              beamBttmCrnrChangeFlange.ToString() + "," +
                                                                              changeIntrnalCrnrDepth.ToString() + "," +
                                                                              fixedIntrnalCrnrDepth.ToString());
            AEE_Utility.AttachXData(externalCornerId, xDataRegAppName, CommonModule.xDataAsciiName);

            drawBeamBottomInternalCorner(basePoint, externalCornerId, outsideOffstWallId);

            DoorHelper doorHlp = new DoorHelper();
            ObjectId maxLengthVerticeLineId_Corner = new ObjectId();
            double maxLengthVerticeLine = doorHlp.getMaximumLengthOfCornerVertex(beamThickLineAngle, externalCornerId, out maxLengthVerticeLineId_Corner);
            if (maxLengthVerticeLine != beamBttmCrnrFixedFlange)
            {
                AEE_Utility.deleteEntity(externalCornerId);
                externalCornerId = partHlp.drawBeamBottomInternalCorner(xDataRegAppName, basePoint.X, basePoint.Y, beamBttmCrnrFixedFlange, beamBttmCrnrChangeFlange, changeIntrnalCrnrDepth, fixedIntrnalCrnrDepth, CommonModule.beamCornerLayerName, CommonModule.beamCornerLayerColor);
                AEE_Utility.AttachXData(externalCornerId, xDataRegAppName, CommonModule.xDataAsciiName);
                AEE_Utility.AttachXData(externalCornerId, xDataRegAppName + "_1", beamBttmCrnrFixedFlange.ToString() + "," +
                                                                                  beamBttmCrnrChangeFlange.ToString() + "," +
                                                                                  changeIntrnalCrnrDepth.ToString() + "," +
                                                                                  fixedIntrnalCrnrDepth.ToString());
                AEE_Utility.AttachXData(externalCornerId, xDataRegAppName, CommonModule.xDataAsciiName);

                drawBeamBottomInternalCorner(basePoint, externalCornerId, outsideOffstWallId);

                maxLengthVerticeLineId_Corner = new ObjectId();
                maxLengthVerticeLine = doorHlp.getMaximumLengthOfCornerVertex(beamThickLineAngle, externalCornerId, out maxLengthVerticeLineId_Corner);
            }
            maxLengthCrnrLineId = maxLengthVerticeLineId_Corner;
            AEE_Utility.AttachXData(maxLengthCrnrLineId, xDataRegAppName, CommonModule.xDataAsciiName);

            return externalCornerId;
        }


        private ObjectId drawBeamBottomInternalCorner(Point3d basePoint, ObjectId externalCornerId, ObjectId outsideOffstWallId)
        {
            bool flagOfCornerInside = false;
            List<double> listOfAngle = new List<double>();
            listOfAngle.Add(0);
            listOfAngle.Add(CommonModule.angle_90);
            listOfAngle.Add(CommonModule.angle_180);
            listOfAngle.Add(CommonModule.angle_270);
            for (int m = 0; m < listOfAngle.Count; m++)
            {
                double angle = listOfAngle[m];
                CommonModule.basePoint = new Point3d(basePoint.X, basePoint.Y, 0);
                CommonModule.rotateAngle = (angle * Math.PI) / 180; // variable assign for rotation    

                List<ObjectId> listOfObjId = new List<ObjectId>();
                listOfObjId.Add(externalCornerId);
                MoveRorationScaleJig.RotationScaleJig_Method(listOfObjId);

                var listOfVertice = AEE_Utility.GetPolylineVertexPoint(externalCornerId);
                foreach (var vertice in listOfVertice)
                {
                    Point3d point = new Point3d(vertice.X, vertice.Y, 0);
                    flagOfCornerInside = AEE_Utility.GetPointIsInsideThePolyline(outsideOffstWallId, point);
                    if (flagOfCornerInside == false)
                    {
                        break;
                    }
                }
                if (flagOfCornerInside == true)
                {
                    break;
                }
            }

            ////if (flagOfCornerInside == false)
            ////{
            ////    AEE_Utility.deleteEntity(externalCornerId);
            ////}
            return externalCornerId;
        }


        private void writeBeamBottomInternalCornerText(string xDataRegAppName, ObjectId internalCornerId, ObjectId maxLengthCrnrLineId, double beamThickLineAngle, double beamBttmCrnrChangeFlange, double beamBttmCrnrFixedFlange, double changeIntrnalCrnrDepth, double fixedIntrnalCrnrDepth, bool flagOfSmallestBeamDist, CornerElevation elev)
        {
            AEE_Utility.MoveEntity(maxLengthCrnrLineId, CreateShellPlanHelper.moveVector_ForBeamBottmLayout);

            ObjectId maxLengthCrnrOffstLineId = new ObjectId();
            var offsetId = AEE_Utility.OffsetEntity(10, maxLengthCrnrLineId, false);
            var listOfInsct = AEE_Utility.InterSectionBetweenTwoEntity(internalCornerId, offsetId);

            if (listOfInsct.Count == 0)
            {
                maxLengthCrnrOffstLineId = AEE_Utility.OffsetEntity(-(changeIntrnalCrnrDepth / 2), maxLengthCrnrLineId, false);
            }
            else
            {
                maxLengthCrnrOffstLineId = AEE_Utility.OffsetEntity((changeIntrnalCrnrDepth / 2), maxLengthCrnrLineId, false);
            }

            var midPoint = AEE_Utility.GetMidPoint(maxLengthCrnrOffstLineId);
            CommonModule commonMod = new CommonModule();
            var listOfStrtEndPoint = commonMod.getStartEndPointOfLine(maxLengthCrnrOffstLineId);
            double textAngle = AEE_Utility.GetAngleOfLine(listOfStrtEndPoint[0], listOfStrtEndPoint[1]);

            var internalCornerEnt = AEE_Utility.GetEntityForRead(internalCornerId);
            if (internalCornerEnt.Layer == CommonModule.beamCornerLayerName)
            {
                CornerHelper cornerHlp = new CornerHelper();
                var listOfCrnrFlange = cornerHlp.getCornerFlangeLength(internalCornerId);
                if (listOfCrnrFlange.Count != 0)
                {
                    beamBttmCrnrFixedFlange = listOfCrnrFlange[0];
                    beamBttmCrnrChangeFlange = listOfCrnrFlange[1];
                }
            }

            List<double> listOfCornerFlangeWithDepth = getCornerFlangeAsPerDirction(internalCornerId, beamBttmCrnrFixedFlange, beamBttmCrnrChangeFlange, changeIntrnalCrnrDepth, fixedIntrnalCrnrDepth);
            double rightCrnrFlange = listOfCornerFlangeWithDepth[0];
            double rightCrnrDepth = listOfCornerFlangeWithDepth[1];
            double leftCrnrFlange = listOfCornerFlangeWithDepth[2];
            double leftCrnrDepth = listOfCornerFlangeWithDepth[3];

            string cornerText = "";
            string beamCornerItemCode = "";

            //X = Beam Bottom - internalCorner - Lintel Level
            //If X < 74
            //Then
            //internalCorner = internalCorner + X

            //Plan width dimention on both sides is same [Fixed Below]12-05-2021 fixed by RTJ
            double internalCorner = 0.0;
            double dInternalCorner = Beam_UI_Helper.getOffsetBeamBottom(0) - CommonModule.internalCorner - Window_UI_Helper.listOfWindowLintelLevel[0];
            if (dInternalCorner < 74)
            {
                dInternalCorner = CommonModule.internalCorner + dInternalCorner;
            }
            else
            {
                dInternalCorner = CommonModule.internalCorner;
            }
            //Plan width dimention on both sides is same [Fixed Below]12-05-2021 fixed by RTJ
            if (flagOfSmallestBeamDist == false)
            {
                beamCornerItemCode = " CC ";
                //cornerText = Convert.ToString(rightCrnrDepth) + " x " + Convert.ToString(leftCrnrDepth) + beamCornerItemCode + Convert.ToString(rightCrnrFlange) + " x " + Convert.ToString(leftCrnrFlange);
                cornerText = Convert.ToString(rightCrnrDepth) + " x " + Convert.ToString(dInternalCorner) + beamCornerItemCode + Convert.ToString(rightCrnrFlange) + " x " + Convert.ToString(leftCrnrFlange);

            }
            else
            {
                beamCornerItemCode = " CC(BH)(SP1) ";
                //cornerText = Convert.ToString(rightCrnrDepth) + " x " + Convert.ToString(leftCrnrDepth) + beamCornerItemCode + Convert.ToString(rightCrnrFlange) + " x " + Convert.ToString(leftCrnrFlange);
                cornerText = Convert.ToString(rightCrnrDepth) + " x " + Convert.ToString(dInternalCorner) + beamCornerItemCode + Convert.ToString(rightCrnrFlange) + " x " + Convert.ToString(leftCrnrFlange);
            }
            //Plan width dimention on both sides is same [Fixed Below]12-05-2021 fixed by RTJ

            ObjectId internalCornerTextId = AEE_Utility.CreateTextWithAngle(cornerText, midPoint.X, midPoint.Y, 0, CommonModule.wallPanelTextHght, CommonModule.beamCornerLayerName, CommonModule.beamCornerLayerColor, textAngle);
            //CornerElevation elev = new CornerElevation(xDataRegAppName, "", 0, 0, 0);
            CornerHelper.setCornerDataForBOM(xDataRegAppName, internalCornerId, internalCornerTextId, cornerText, CommonModule.beamBottomCornerType, beamCornerItemCode, rightCrnrFlange, leftCrnrFlange, 0, elev);

            AEE_Utility.deleteEntity(offsetId);
            AEE_Utility.deleteEntity(maxLengthCrnrOffstLineId);
        }


        private List<double> getCornerFlangeAsPerDirction(ObjectId internalCornerId, double beamBttmCrnrFixedFlange, double beamBttmCrnrChangeFlange, double changeIntrnalCrnrDepth, double fixedIntrnalCrnrDepth)
        {
            List<double> listOfCornerFlangeWithDepth = new List<double>();

            double rightCrnrFlange = beamBttmCrnrFixedFlange;
            double rightCrnrDepth = changeIntrnalCrnrDepth;
            double leftCrnrFlange = beamBttmCrnrChangeFlange;
            double leftCrnrDepth = fixedIntrnalCrnrDepth;

            var internalCornerEnt = AEE_Utility.GetEntityForRead(internalCornerId);
            if (internalCornerEnt.Layer == CommonModule.beamCornerLayerName)
            {
                CornerHelper cornerHlp = new CornerHelper();
                var listOfCrnrFlange = cornerHlp.getCornerFlangeLength(internalCornerId);
                if (listOfCrnrFlange.Count != 0)
                {
                    double rghtFlange = listOfCrnrFlange[0];
                    double leftFlange = listOfCrnrFlange[1];
                    if (rghtFlange == leftCrnrFlange)
                    {
                        leftCrnrFlange = beamBttmCrnrFixedFlange;
                        leftCrnrDepth = changeIntrnalCrnrDepth;

                        rightCrnrFlange = beamBttmCrnrChangeFlange;
                        rightCrnrDepth = fixedIntrnalCrnrDepth;
                    }
                }
            }
            listOfCornerFlangeWithDepth.Add(rightCrnrFlange);
            listOfCornerFlangeWithDepth.Add(rightCrnrDepth);
            listOfCornerFlangeWithDepth.Add(leftCrnrFlange);
            listOfCornerFlangeWithDepth.Add(leftCrnrDepth);

            return listOfCornerFlangeWithDepth;
        }
        private void drawBeamBottomPanelInternalCorner(Point3d strtPnt, Point3d endPnt, Point3d offstStrtPnt, Point3d offstEndPnt, ObjectId beamRectId)
        {
            var startLineId = AEE_Utility.getLineId(strtPnt, offstStrtPnt, true);
            var startOffstLineId = getOffsetLineInSide(beamRectId, startLineId, CommonModule.internalCorner);

            var endLineId = AEE_Utility.getLineId(endPnt, offstEndPnt, true);
            var endOffstLineId = getOffsetLineInSide(beamRectId, endLineId, CommonModule.internalCorner);

            List<Point3d> listOfStrtLinePoints = AEE_Utility.GetStartEndPointOfLine(startLineId);
            List<Point3d> listOfStrtOffstLinePoints = AEE_Utility.GetStartEndPointOfLine(startOffstLineId);

            List<Point3d> listOfEndLinePoints = AEE_Utility.GetStartEndPointOfLine(endLineId);
            List<Point3d> listOfEndOffstLinePoints = AEE_Utility.GetStartEndPointOfLine(endOffstLineId);

            Point3d strtLine_StrtPoint = listOfStrtLinePoints[0];
            Point3d strtLine_EndPoint = listOfStrtLinePoints[1];

            Point3d strtOffstLine_StrtPoint = listOfStrtOffstLinePoints[0];
            Point3d strtOffstLine_EndPoint = listOfStrtOffstLinePoints[1];


            Point3d endLine_StrtPoint = listOfEndLinePoints[0];
            Point3d endLine_EndPoint = listOfEndLinePoints[1];

            Point3d endOffstLine_StrtPoint = listOfEndOffstLinePoints[0];
            Point3d endOffstLine_EndPoint = listOfEndOffstLinePoints[1];

            var thickWallPanelRectId1 = AEE_Utility.createRectangle(strtLine_StrtPoint, strtLine_EndPoint, strtOffstLine_EndPoint, strtOffstLine_StrtPoint, CommonModule.wallPanelLyrName, CommonModule.wallPanelLyrColor);
            var thickWallPanelRectId2 = AEE_Utility.createRectangle(endLine_StrtPoint, endLine_EndPoint, endOffstLine_EndPoint, endOffstLine_StrtPoint, CommonModule.wallPanelLyrName, CommonModule.wallPanelLyrColor);
        }


        public static ObjectId getOffsetLineInSide(ObjectId polylineId, ObjectId lineId, double offsetValue)
        {
            var polylineEnt = AEE_Utility.GetEntityForRead(polylineId);

            ObjectId offsetLineId = AEE_Utility.OffsetLine(polylineEnt, lineId, offsetValue, false);

            var midPoint = AEE_Utility.GetMidPoint(offsetLineId);
            var flagOfInside = AEE_Utility.GetPointIsInsideThePolyline(polylineId, midPoint);

            if (flagOfInside == false)
            {
                AEE_Utility.deleteEntity(offsetLineId);
                offsetLineId = AEE_Utility.OffsetLine(polylineEnt, lineId, -offsetValue, false);
            }
            return offsetLineId;
        }


        public void drawBeamBottomPanel(ProgressForm progressForm, string wallCreationMsg)
        {
            for (int i = 0; i < listOfListOfBeamBottomCornerId.Count; i++)
            {
                List<ObjectId> listOfBeamBottomCornerId = listOfListOfBeamBottomCornerId[i];
                List<ObjectId> listOfBeamCornerMaxLngthLineId = listOfListOfBeamCornerMaxLngthLineId[i];
                List<object> listOfBeamBottomPanelData = listOfListOfBeamBottomPanelData[i];
                if (listOfBeamBottomCornerId.Count == 2)
                {
                    ObjectId cornerId1 = listOfBeamBottomCornerId[0];
                    if (cornerId1.IsValid == true && cornerId1.IsErased == false)
                    {
                        ObjectId corner1MaxLngthLineId = listOfBeamCornerMaxLngthLineId[0];
                        var corner1_BasePoint = AEE_Utility.GetBasePointOfPolyline(cornerId1);
                        string xDataRegAppName_Corner1 = AEE_Utility.GetXDataRegisterAppName(cornerId1);
                        string xDataRegAppName = AEE_Utility.GetXDataRegisterAppName(corner1MaxLngthLineId);

                        ObjectId cornerId2 = listOfBeamBottomCornerId[1];
                        ObjectId corner2MaxLngthLineId = listOfBeamCornerMaxLngthLineId[1];
                        var corner2_BasePoint = AEE_Utility.GetBasePointOfPolyline(cornerId2);

                        string xDataRegAppName_Corner2 = AEE_Utility.GetXDataRegisterAppName(cornerId2);

                        xDataRegAppName = AEE_Utility.GetXDataRegisterAppName(corner2MaxLngthLineId);

                        double fixedIntrnalCrnrDepth = Convert.ToDouble(listOfBeamBottomPanelData[0]);
                        double changeIntrnalCrnrDepth = Convert.ToDouble(listOfBeamBottomPanelData[1]);
                        double beamBttmCrnrFixedFlange = Convert.ToDouble(listOfBeamBottomPanelData[2]);
                        double beamBttmCrnrChangeFlange = Convert.ToDouble(listOfBeamBottomPanelData[3]);
                        double beamBottomPanelDeth = Convert.ToDouble(listOfBeamBottomPanelData[4]);
                        double distBtwWallToBeam = Convert.ToDouble(listOfBeamBottomPanelData[5]);
                        bool flagOfBeamBottomPanel = Convert.ToBoolean(listOfBeamBottomPanelData[6]);
                        bool flagOfSmallestBeamDist = Convert.ToBoolean(listOfBeamBottomPanelData[7]);

                        List<Point3d> listOfBeamBottomPanelLineEndPoint_Cornr1 = getOffsetMaxLengthLineId(corner1MaxLngthLineId, cornerId1, corner1_BasePoint, changeIntrnalCrnrDepth);
                        Point3d maxLngthEndPoint_Cornr1 = listOfBeamBottomPanelLineEndPoint_Cornr1[0];
                        Point3d offsetMaxLngthEndPoint_Cornr1 = listOfBeamBottomPanelLineEndPoint_Cornr1[1];

                        List<Point3d> listOfBeamBottomPanelLineEndPoint_Cornr2 = getOffsetMaxLengthLineId(corner2MaxLngthLineId, cornerId2, corner2_BasePoint, changeIntrnalCrnrDepth);
                        Point3d maxLngthEndPoint_Cornr2 = listOfBeamBottomPanelLineEndPoint_Cornr2[0];
                        Point3d offsetMaxLngthEndPoint_Cornr2 = listOfBeamBottomPanelLineEndPoint_Cornr2[1];


                        List<Point3d> listOfBeamBottomPanelBALineEndPoint_Cornr1 = getOffsetMaxLengthLineId(corner1MaxLngthLineId, cornerId1, corner1_BasePoint, distBtwWallToBeam);

                        List<Point3d> listOfBeamBottomPanelBALineEndPoint_Cornr2 = getOffsetMaxLengthLineId(corner2MaxLngthLineId, cornerId2, corner2_BasePoint, distBtwWallToBeam);

                        drawBeamBottomInternalCornerPanel(xDataRegAppName, flagOfBeamBottomPanel, flagOfSmallestBeamDist, cornerId1, cornerId2, listOfBeamBottomPanelLineEndPoint_Cornr1, listOfBeamBottomPanelLineEndPoint_Cornr2, listOfBeamBottomPanelBALineEndPoint_Cornr1, listOfBeamBottomPanelBALineEndPoint_Cornr2, changeIntrnalCrnrDepth, fixedIntrnalCrnrDepth, beamBottomPanelDeth, listOfListOfBeamlvls[i]);
                    }
                }
            }
        }


        private void drawBeamBottomInternalCornerPanel(string xDataRegAppName, bool flagOfBeamBottomPanel, bool flagOfSmallestBeamDist, ObjectId cornerId1, ObjectId cornerId2,
                                                       List<Point3d> listOfBeamBttmLineEndPoint_Cornr1, List<Point3d> listOfBeamBttmLineEndPoint_Cornr2, List<Point3d> listOfBeamBttmBALineEndPoint_Cornr1, List<Point3d> listOfBeamBttmBALineEndPoint_Cornr2,
                                                       double changeIntrnalCrnrDepth, double fixedIntrnalCrnrDepth, double beamBottomPanelDeth, double beamBottomLvl)
        {
            CommonModule commonMod = new CommonModule();
            WallPanelHelper wallPanelHlp = new WallPanelHelper();

            Point3d maxLngthEndPoint_Cornr1 = listOfBeamBttmLineEndPoint_Cornr1[0];
            Point3d offsetMaxLngthEndPoint_Cornr1 = listOfBeamBttmLineEndPoint_Cornr1[1];

            Point3d maxLngthEndPoint_Cornr2 = listOfBeamBttmLineEndPoint_Cornr2[0];
            Point3d offsetMaxLngthEndPoint_Cornr2 = listOfBeamBttmLineEndPoint_Cornr2[1];

            Point3d offsetMaxLngthBAPanelEndPoint_Cornr1 = listOfBeamBttmBALineEndPoint_Cornr1[1];
            Point3d offsetMaxLngthBAPanelEndPoint_Cornr2 = listOfBeamBttmBALineEndPoint_Cornr2[1];


            var beamSidePanelLineId = AEE_Utility.getLineId(maxLngthEndPoint_Cornr1, maxLngthEndPoint_Cornr2, false);
            var listOfStrtEndPoint = commonMod.getStartEndPointOfLine(beamSidePanelLineId);
            var beamLineStrtPoint = listOfStrtEndPoint[0];
            var beamLineEndPoint = listOfStrtEndPoint[1];

            var beamSidePanelOffstLineId = AEE_Utility.getLineId(offsetMaxLngthEndPoint_Cornr1, offsetMaxLngthEndPoint_Cornr2, false);
            var listOfOffstStrtEndPoint = commonMod.getStartEndPointOfLine(beamSidePanelOffstLineId);
            var beamOffstLineStrtPoint = listOfOffstStrtEndPoint[0];
            var beamOffstLineEndPoint = listOfOffstStrtEndPoint[1];

            var lengthOfLine = AEE_Utility.GetLengthOfLine(maxLngthEndPoint_Cornr1, maxLngthEndPoint_Cornr2);
            lengthOfLine = Math.Round(lengthOfLine);

            List<double> listOfWallPanelLength = wallPanelHlp.getListOfWallPanelLength(lengthOfLine, CommonModule.bottomPanel_StndrdLngth, CommonModule.bottomPanel_MinLngth);

            double angleOfLine = AEE_Utility.GetAngleOfLine(beamLineStrtPoint.X, beamLineStrtPoint.Y, beamLineEndPoint.X, beamLineEndPoint.Y);
            bool flag_X_Axis = false;
            bool flag_Y_Axis = false;

            commonMod.checkAngleOfLine_Axis(angleOfLine, out flag_X_Axis, out flag_Y_Axis);

            List<ObjectId> listOfWallPanelLine_ObjId = new List<ObjectId>();
            var listOfWallPanelRect_ObjId = wallPanelHlp.drawWallPanels(xDataRegAppName, beamSidePanelLineId, beamLineStrtPoint, beamLineEndPoint, beamOffstLineStrtPoint, beamOffstLineEndPoint, flag_X_Axis, flag_Y_Axis, listOfWallPanelLength, out listOfWallPanelLine_ObjId);
            string wallPanelText = "";


            //if (flagOfSmallestBeamDist == false && flagOfBeamBottomPanel == false) //commented on 07-06-2021 by PBAC
            if (flagOfSmallestBeamDist == false)
            {

                var beamSideBAPanelLineId = AEE_Utility.getLineId(offsetMaxLngthBAPanelEndPoint_Cornr1, offsetMaxLngthBAPanelEndPoint_Cornr2, false);

                List<ObjectId> listOfCrnerId_And_PanelLineId = new List<ObjectId>();
                listOfCrnerId_And_PanelLineId.Add(cornerId1);
                listOfCrnerId_And_PanelLineId.Add(cornerId2);
                listOfCrnerId_And_PanelLineId.Add(beamSidePanelLineId);
                listOfCrnerId_And_PanelLineId.Add(beamSideBAPanelLineId);
                drawBeamBottomExternalCornerPanel(xDataRegAppName, listOfCrnerId_And_PanelLineId, changeIntrnalCrnrDepth, listOfWallPanelLength, angleOfLine, flag_X_Axis, flag_Y_Axis, offsetMaxLngthBAPanelEndPoint_Cornr1, offsetMaxLngthBAPanelEndPoint_Cornr2, beamBottomLvl);
            }
            //Below line added by by RTJ May-18 for Wall Text modification.
            //X = Beam Bottom - internalCorner - Lintel Level
            //If X < 74
            //Then
            //internalCorner = internalCorner + X

            //Plan width dimention on both sides is same [Fixed Below]18-05-2021 fixed by RTJ
            double internalCorner = 0.0;

            double dInternalCorner = Beam_UI_Helper.getOffsetBeamBottom(0) - CommonModule.internalCorner - Window_UI_Helper.listOfWindowLintelLevel[0];
            if (dInternalCorner < 74)
            {
                dInternalCorner = CommonModule.internalCorner + dInternalCorner;
            }
            else
            {
                dInternalCorner = CommonModule.internalCorner;
            }
            if (flagOfSmallestBeamDist == false)
            {
                //wallPanelText = Convert.ToString(changeIntrnalCrnrDepth) + " x " + Convert.ToString(fixedIntrnalCrnrDepth) + " CC";
                wallPanelText = Convert.ToString(changeIntrnalCrnrDepth) + " x " + Convert.ToString(dInternalCorner) + " CC";

                //Added this line to fix the BeamBottomPanel elevation by SDM 12-06-2022
                beamBottomLvl -= dInternalCorner;
            }
            else
            {
                wallPanelText = Convert.ToString(changeIntrnalCrnrDepth) + " x " + Convert.ToString(dInternalCorner) + " CC(BH)(SP1)";
            }
            //End...  RTJ May-18 for Wall Modification.
            writeBeamBottomPanel(xDataRegAppName, wallPanelText, listOfWallPanelRect_ObjId, listOfWallPanelLine_ObjId, angleOfLine, CommonModule.beamBottomInternalPanelType, beamBottomLvl, dInternalCorner);

            if (flagOfBeamBottomPanel == true)
            {
                drawBeamBottomWallPanel(xDataRegAppName, beamBottomPanelDeth, cornerId1, cornerId2, angleOfLine, beamBottomLvl);
            }
        }


        public List<ObjectId> writeBeamBottomPanel(string xDataRegAppName, string wallPanelText, List<ObjectId> listOfWallPanelRect_ObjId, List<ObjectId> listOfWallPanelLine_ObjId,
                                          double angleOfLine, string wallType, double beamBottomLvl,
                                          //Added this param to fix the BeamBottomPanel height by SDM 12-06-2022
                                          double wallheight = 0)
        {
            List<ObjectId> listOfDimTextIds = new List<ObjectId>();//Changes made on 11/04/2023 by SDM
            WallPanelHelper wallPanelHlp = new WallPanelHelper();
            for (int k = 0; k < listOfWallPanelRect_ObjId.Count; k++)
            {
                ObjectId wallPanelRect_Id = listOfWallPanelRect_ObjId[k];
                ObjectId wallPanelLineId = listOfWallPanelLine_ObjId[k];
                double wallPanelLineLngth = AEE_Utility.GetLengthOfLine(wallPanelLineId);
                wallPanelLineLngth = Math.Round(wallPanelLineLngth);
                Point3d dimTextPoint = WallPanelHelper.getCenterPointOfPanelRectangle(wallPanelRect_Id);

                AEE_Utility.changeLayer(wallPanelRect_Id, CommonModule.beamBottomWallPanelLayerName, CommonModule.beamBottomWallPanelLayerColor);

                string wallText = wallPanelText + " " + Convert.ToString(wallPanelLineLngth);
                ObjectId dimTextId2 = wallPanelHlp.writeDimensionTextInWallPanel(wallText, wallPanelLineId, dimTextPoint, angleOfLine, CommonModule.beamBottomWallPanelTextLayerName, CommonModule.beamBottomWallPanelLayerColor);
                AEE_Utility.changeLayer(dimTextId2, CommonModule.beamBottomWallPanelTextLayerName, CommonModule.beamBottomWallPanelLayerColor);

                string wallDescp = wallText;
                string wallItemCode = wallPanelText;
                double externalCornrHeight = wallPanelText.Contains("CC") ? wallheight : 0;
                wallPanelHlp.setBOMDataOfWallPanel(dimTextId2, wallPanelRect_Id, wallPanelLineId, wallPanelLineLngth, externalCornrHeight, 0, wallItemCode, wallDescp, wallType, beamBottomLvl.ToString());

                listOfDimTextIds.Add(dimTextId2);
                //var circleId = wallPanelHlp.createCircleInNonStandardWallPanel(dimTextPoint.X, dimTextPoint.Y, wallPnlLngth);
                //if (circleId.IsValid == true)
                //{
                //    listOfMoveId.Add(circleId);
                //}
            }
            return listOfDimTextIds;
        }


        public void drawBeamBottomExternalCornerPanel(string xDataRegAppName, List<ObjectId> listOfCrnerId_And_PanelLineId, double changeIntrnalCrnrDepth,
                                                       List<double> listOfWallPanelLength, double angleOfLine, bool flag_X_Axis, bool flag_Y_Axis,
                                                       Point3d offsetMaxLngthEndPoint_Cornr1, Point3d offsetMaxLngthEndPoint_Cornr2, double beamBottomLvl)
        {
            ObjectId cornerId1 = listOfCrnerId_And_PanelLineId[0];
            ObjectId cornerId2 = listOfCrnerId_And_PanelLineId[1];
            ObjectId beamSidePanelLineId = listOfCrnerId_And_PanelLineId[2];
            ObjectId beamSideBAPanelLineId = listOfCrnerId_And_PanelLineId[3];

            CommonModule commonMod = new CommonModule();
            WallPanelHelper wallPanelHlpr = new WallPanelHelper();

            double externalCorner = CommonModule.externalCorner;
            ObjectId offsetExternalCrnrLineId = getOffsetLineForBeamBottomExternalCorner(beamSideBAPanelLineId, cornerId1, externalCorner, flag_X_Axis, flag_Y_Axis);

            var listOfStrtEndPoint = commonMod.getStartEndPointOfLine(beamSideBAPanelLineId);
            var beamLineStrtPoint = listOfStrtEndPoint[0];
            var beamLineEndPoint = listOfStrtEndPoint[1];

            var listOfOffstStrtEndPoint = commonMod.getStartEndPointOfLine(offsetExternalCrnrLineId);
            var beamOffstLineStrtPoint = listOfOffstStrtEndPoint[0];
            var beamOffstLineEndPoint = listOfOffstStrtEndPoint[1];


            List<ObjectId> listOfWallPanelLine_ObjId = new List<ObjectId>();
            var listOfWallPanelRect_ObjId = wallPanelHlpr.drawWallPanels(xDataRegAppName, beamSideBAPanelLineId, beamLineStrtPoint, beamLineEndPoint, beamOffstLineStrtPoint, beamOffstLineEndPoint, flag_X_Axis, flag_Y_Axis, listOfWallPanelLength, out listOfWallPanelLine_ObjId);

            AddBeamBottomExternalWallPanelObjs(xDataRegAppName, listOfWallPanelLine_ObjId, listOfWallPanelRect_ObjId);

            string wallPanelText = PanelLayout_UI.beamSidePanelName;
            writeBeamBottomPanel(xDataRegAppName, wallPanelText, listOfWallPanelRect_ObjId, listOfWallPanelLine_ObjId, angleOfLine, CommonModule.beamBottomExternalPanelType, beamBottomLvl);
            drawBeamBottomPanelExternalCornerPanel(xDataRegAppName, wallPanelText, cornerId1, cornerId2, offsetMaxLngthEndPoint_Cornr1, offsetMaxLngthEndPoint_Cornr2, flag_X_Axis, flag_Y_Axis, angleOfLine, beamBottomLvl);
        }
        public void AddBeamBottomExternalWallPanelObjs(string xDataRegAppName, List<ObjectId> listOfPanelLineObjectIds, List<ObjectId> listOfPanelObjectIds)
        {

            for (int i = 0; i < listOfPanelLineObjectIds.Count; i++)
            {
                listOfBeamBottomExternalWallPanelObj.Add(new Tuple<string, ObjectId, ObjectId>(xDataRegAppName, listOfPanelLineObjectIds[i], listOfPanelObjectIds[i]));
            }

        }
        private void ValidateAndUpdateBeamBottomExternalPanel(string xDataRegAppName, ref List<ObjectId> listOfPanelObj, ref List<double> listOfPanelLength, ref List<ObjectId> listOfPanelLineObj)
        {
            string[] wallnameSplit = xDataRegAppName.Split('_');
            string roomName = wallnameSplit[1] + "_" + wallnameSplit[2];

            List<Tuple<string, ObjectId, ObjectId>> roomSpecificPanels = listOfBeamBottomExternalWallPanelObj.Where(x => x.Item1.EndsWith(roomName)).ToList();


            double angleOfPanelLine = 0;
            double angleOfExistingPanelLine = 0;
            bool flag_X_Axis = false;
            bool flag_Y_Axis = false;
            bool flag_X_Axis_Existing = false;
            bool flag_Y_Axis_Existing = false;
            ObjectId modifiedPanelLineObj = new ObjectId();
            ObjectId modifiedPanelObj = new ObjectId();

            CommonModule commonMod = new CommonModule();

            for (int i = 0; i < listOfPanelLineObj.Count; i++)
            {
                angleOfPanelLine = AEE_Utility.GetAngleOfLine(listOfPanelLineObj[i]);
                commonMod.checkAngleOfLine_Axis(angleOfPanelLine, out flag_X_Axis, out flag_Y_Axis);

                for (int j = 0; j < roomSpecificPanels.Count; j++)
                {

                    angleOfExistingPanelLine = AEE_Utility.GetAngleOfLine(roomSpecificPanels[j].Item2);
                    commonMod.checkAngleOfLine_Axis(angleOfExistingPanelLine, out flag_X_Axis_Existing, out flag_Y_Axis_Existing);
                    if (flag_X_Axis != flag_X_Axis_Existing)
                    {
                        List<Point3d> intersectPts = AEE_Utility.InterSectionPointBetweenPolyLineAndLine(roomSpecificPanels[j].Item3, listOfPanelLineObj[i], Intersect.OnBothOperands);
                        if (intersectPts.Count == 2)
                        {
                            double recWidth = 0;
                            double recLength = 0;
                            commonMod.getRectangleWidthLength(angleOfPanelLine, listOfPanelObj[i], out recLength, out recWidth);

                            List<Point3d> panelLinePts = AEE_Utility.GetStartEndPointOfLine(listOfPanelLineObj[i]);

                            Point3d panelLineStartPt = new Point3d();
                            Point3d panelLineEndPt = new Point3d();
                            Point3d movePt = new Point3d();

                            if (intersectPts[0].IsEqualTo(panelLinePts[0]))
                            {
                                panelLineStartPt = intersectPts[1];
                                panelLineEndPt = panelLinePts[1];
                                movePt = intersectPts[1];
                            }
                            else if (intersectPts[0].IsEqualTo(panelLinePts[1]))
                            {
                                panelLineStartPt = panelLinePts[0];
                                panelLineEndPt = intersectPts[1];
                                movePt = intersectPts[1];
                            }
                            else if (intersectPts[1].IsEqualTo(panelLinePts[0]))
                            {
                                panelLineStartPt = intersectPts[0];
                                panelLineEndPt = panelLinePts[1];
                                movePt = intersectPts[0];
                            }
                            else if (intersectPts[1].IsEqualTo(panelLinePts[1]))
                            {
                                panelLineStartPt = panelLinePts[0];
                                panelLineEndPt = intersectPts[0];
                                movePt = intersectPts[0];
                            }

                            modifiedPanelLineObj = AEE_Utility.getLineId(AEE_Utility.GetEntityOfLine(listOfPanelLineObj[i]), panelLineStartPt, panelLineEndPt, false);
                            ObjectId offsetLineId = AEE_Utility.OffsetLine(modifiedPanelLineObj, recWidth / 2, false);
                            ObjectId offsetLineId1 = AEE_Utility.OffsetLine(modifiedPanelLineObj, recWidth, false);

                            var midPointOfOffstLine = AEE_Utility.GetMidPoint(offsetLineId);

                            var flagOfInside = AEE_Utility.GetPointIsInsideThePolyline(listOfPanelObj[i], midPointOfOffstLine);
                            if (flagOfInside == false)
                            {
                                AEE_Utility.deleteEntity(offsetLineId);
                                AEE_Utility.deleteEntity(offsetLineId1);
                                offsetLineId = AEE_Utility.OffsetLine(modifiedPanelLineObj, -recWidth / 2, false);
                                offsetLineId1 = AEE_Utility.OffsetLine(modifiedPanelLineObj, -recWidth, false);
                            }

                            List<Point3d> OffsetPointsList = AEE_Utility.GetStartEndPointOfLine(offsetLineId1);

                            Entity panelObjEnt = AEE_Utility.GetEntityForRead(listOfPanelObj[i]);

                            modifiedPanelObj = AEE_Utility.createRectangle(panelLineStartPt, panelLineEndPt, OffsetPointsList[1], OffsetPointsList[0], panelObjEnt.Layer, panelObjEnt.ColorIndex);

                            AEE_Utility.deleteEntity(offsetLineId);
                            AEE_Utility.deleteEntity(offsetLineId1);

                            AEE_Utility.deleteEntity(listOfPanelLineObj[i]);
                            AEE_Utility.deleteEntity(listOfPanelObj[i]);

                            listOfPanelLineObj[i] = modifiedPanelLineObj;
                            listOfPanelObj[i] = modifiedPanelObj;
                            listOfPanelLength[i] = Math.Round(AEE_Utility.GetLengthOfLine(modifiedPanelLineObj), 1);
                            break;
                        }
                    }
                }
            }
        }
        private void ValidateAndUpdateBeamBottomPanel(string xDataRegAppName, ref Point3d p1, ref Point3d p2, ref Point3d p3, ref Point3d p4, ref ObjectId wallPanelObj, ref ObjectId wallPanelLineObj)
        {
            string[] wallnameSplit = xDataRegAppName.Split('_');
            string roomName = wallnameSplit[1] + "_" + wallnameSplit[2];

            List<Tuple<string, ObjectId, ObjectId>> roomSpecificPanels = listOfBeamBottomWallPanelObj.Where(x => x.Item1.EndsWith(roomName)).ToList();


            double angleOfPanelLine = 0;
            double angleOfExistingPanelLine = 0;
            bool flag_X_Axis = false;
            bool flag_Y_Axis = false;
            bool flag_X_Axis_Existing = false;
            bool flag_Y_Axis_Existing = false;
            ObjectId modifiedPanelLineObj = new ObjectId();
            ObjectId modifiedPanelObj = new ObjectId();

            CommonModule commonMod = new CommonModule();

            angleOfPanelLine = AEE_Utility.GetAngleOfLine(wallPanelLineObj);
            commonMod.checkAngleOfLine_Axis(angleOfPanelLine, out flag_X_Axis, out flag_Y_Axis);

            for (int j = 0; j < roomSpecificPanels.Count; j++)
            {

                angleOfExistingPanelLine = AEE_Utility.GetAngleOfLine(roomSpecificPanels[j].Item2);
                commonMod.checkAngleOfLine_Axis(angleOfExistingPanelLine, out flag_X_Axis_Existing, out flag_Y_Axis_Existing);
                if (flag_X_Axis != flag_X_Axis_Existing)
                {
                    List<Point3d> intersectPts = AEE_Utility.InterSectionPointBetweenPolyLineAndLine(roomSpecificPanels[j].Item3, wallPanelLineObj, Intersect.OnBothOperands);
                    if (intersectPts.Count == 2)
                    {
                        double recWidth = 0;
                        double recLength = 0;
                        commonMod.getRectangleWidthLength(angleOfPanelLine, wallPanelObj, out recLength, out recWidth);

                        List<Point3d> panelLinePts = AEE_Utility.GetStartEndPointOfLine(wallPanelLineObj);

                        Point3d panelLineStartPt = new Point3d();
                        Point3d panelLineEndPt = new Point3d();
                        Point3d movePt = new Point3d();

                        if (intersectPts[0].IsEqualTo(panelLinePts[0], new Tolerance(0.1, 0.1)))
                        {
                            panelLineStartPt = intersectPts[1];
                            panelLineEndPt = panelLinePts[1];
                            movePt = intersectPts[1];
                        }
                        else if (intersectPts[0].IsEqualTo(panelLinePts[1], new Tolerance(0.1, 0.1)))
                        {
                            panelLineStartPt = panelLinePts[0];
                            panelLineEndPt = intersectPts[1];
                            movePt = intersectPts[1];
                        }
                        else if (intersectPts[1].IsEqualTo(panelLinePts[0], new Tolerance(0.1, 0.1)))
                        {
                            panelLineStartPt = intersectPts[0];
                            panelLineEndPt = panelLinePts[1];
                            movePt = intersectPts[0];
                        }
                        else if (intersectPts[1].IsEqualTo(panelLinePts[1],new Tolerance(0.1,0.1)))
                        {
                            panelLineStartPt = panelLinePts[0];
                            panelLineEndPt = intersectPts[0];
                            movePt = intersectPts[0];
                        }
                        //Added on 04/06/2023 by SDM to prevent OffsetLine bug when line_length =0
                        if (panelLineStartPt.DistanceTo(panelLineEndPt) <= 1)
                            continue;

                        modifiedPanelLineObj = AEE_Utility.getLineId(AEE_Utility.GetEntityOfLine(wallPanelLineObj), panelLineStartPt, panelLineEndPt, false);
                        ObjectId offsetLineId = AEE_Utility.OffsetLine(modifiedPanelLineObj, recWidth / 2, false);
                        ObjectId offsetLineId1 = AEE_Utility.OffsetLine(modifiedPanelLineObj, recWidth, false);

                        var midPointOfOffstLine = AEE_Utility.GetMidPoint(offsetLineId);

                        var flagOfInside = AEE_Utility.GetPointIsInsideThePolyline(wallPanelObj, midPointOfOffstLine);
                        if (flagOfInside == false)
                        {
                            AEE_Utility.deleteEntity(offsetLineId);
                            AEE_Utility.deleteEntity(offsetLineId1);
                            offsetLineId = AEE_Utility.OffsetLine(modifiedPanelLineObj, -recWidth / 2, false);
                            offsetLineId1 = AEE_Utility.OffsetLine(modifiedPanelLineObj, -recWidth, false);
                        }

                        List<Point3d> OffsetPointsList = AEE_Utility.GetStartEndPointOfLine(offsetLineId1);

                        Entity panelObjEnt = AEE_Utility.GetEntityForRead(wallPanelObj);

                        p1 = panelLineStartPt;
                        p2 = panelLineEndPt;
                        p3 = OffsetPointsList[1];
                        p4 = OffsetPointsList[0];

                        modifiedPanelObj = AEE_Utility.createRectangle(panelLineStartPt, panelLineEndPt, OffsetPointsList[1], OffsetPointsList[0], panelObjEnt.Layer, panelObjEnt.ColorIndex);

                        AEE_Utility.deleteEntity(offsetLineId);
                        AEE_Utility.deleteEntity(offsetLineId1);

                        AEE_Utility.deleteEntity(wallPanelLineObj);
                        AEE_Utility.deleteEntity(wallPanelObj);

                        wallPanelLineObj = modifiedPanelLineObj;
                        wallPanelObj = modifiedPanelObj;
                        break;
                    }
                }
            }
        }

        private void drawBeamBottomPanelExternalCornerPanel(string xDataRegAppName, string wallPanelText, ObjectId cornerId1, ObjectId cornerId2,
                                                            Point3d offsetMaxLngthEndPoint_Cornr1, Point3d offsetMaxLngthEndPoint_Cornr2, bool flag_X_Axis, bool flag_Y_Axis,
                                                            double angleOfLine, double beamBottomLvl)
        {
            KickerHelper kickerHlp = new KickerHelper();
            CommonModule commonMod = new CommonModule();
            WallPanelHelper wallPanelHlp = new WallPanelHelper();

            var corner1_BasePoint = AEE_Utility.GetBasePointOfPolyline(cornerId1);
            var corner1_NrstBasePoint = kickerHlp.getNearestPointOfKickerCorner(cornerId1, corner1_BasePoint);

            if (flag_X_Axis == true)
            {
                corner1_NrstBasePoint = new Point3d(corner1_NrstBasePoint.X, offsetMaxLngthEndPoint_Cornr1.Y, corner1_NrstBasePoint.Z);
            }
            else
            {
                corner1_NrstBasePoint = new Point3d(offsetMaxLngthEndPoint_Cornr1.X, corner1_NrstBasePoint.Y, corner1_NrstBasePoint.Z);
            }

            ObjectId corner1_LineId = AEE_Utility.getLineId(corner1_NrstBasePoint, offsetMaxLngthEndPoint_Cornr1, false);

            List<Point3d> beamIntersectPts = new List<Point3d>();
            if (checkOffsetLineIntersectWithBeam(corner1_LineId, out beamIntersectPts) == true)
            {
                AEE_Utility.deleteEntity(corner1_LineId);
                corner1_LineId = AEE_Utility.getLineId(beamIntersectPts[0], offsetMaxLngthEndPoint_Cornr1, false);
            }

            ObjectId corner1_OffstLineId = checkOffsetLineInOutsideTheCorner(corner1_LineId, cornerId1, CommonModule.externalCorner, flag_X_Axis, flag_Y_Axis);
            var listOfStrtEndPoint_Crnr1 = commonMod.getStartEndPointOfLine(corner1_LineId);
            var beamLineStrtPoint_Crnr1 = listOfStrtEndPoint_Crnr1[0];
            var beamLineEndPoint_Crnr1 = listOfStrtEndPoint_Crnr1[1];
            var lengthOfLine_Crnr1 = AEE_Utility.GetLengthOfLine(beamLineStrtPoint_Crnr1, beamLineEndPoint_Crnr1);
            lengthOfLine_Crnr1 = Math.Round(lengthOfLine_Crnr1);

            var listOfOffstStrtEndPoint_Crnr1 = commonMod.getStartEndPointOfLine(corner1_OffstLineId);
            var beamOffstLineStrtPoint_Crnr1 = listOfOffstStrtEndPoint_Crnr1[0];
            var beamOffstLineEndPoint_Crnr1 = listOfOffstStrtEndPoint_Crnr1[1];

            List<double> listOfWallPanelLength_Crnr1 = wallPanelHlp.getListOfWallPanelLength(lengthOfLine_Crnr1, CommonModule.bottomPanel_StndrdLngth, CommonModule.bottomPanel_MinLngth);

            List<ObjectId> listOfWallPanelLine_ObjId_Crnr1 = new List<ObjectId>();
            var listOfWallPanelRect_ObjId_Crnr1 = wallPanelHlp.drawWallPanels(xDataRegAppName, corner1_LineId, beamLineStrtPoint_Crnr1, beamLineEndPoint_Crnr1, beamOffstLineStrtPoint_Crnr1, beamOffstLineEndPoint_Crnr1, flag_X_Axis, flag_Y_Axis, listOfWallPanelLength_Crnr1, out listOfWallPanelLine_ObjId_Crnr1);

            ValidateAndUpdateBeamBottomExternalPanel(xDataRegAppName, ref listOfWallPanelRect_ObjId_Crnr1, ref listOfWallPanelLength_Crnr1, ref listOfWallPanelLine_ObjId_Crnr1);

            MarkNonStandardPanelSize(listOfWallPanelRect_ObjId_Crnr1, listOfWallPanelLength_Crnr1);

            AddBeamBottomExternalWallPanelObjs(xDataRegAppName, listOfWallPanelLine_ObjId_Crnr1, listOfWallPanelRect_ObjId_Crnr1);

            writeBeamBottomPanel(xDataRegAppName, wallPanelText, listOfWallPanelRect_ObjId_Crnr1, listOfWallPanelLine_ObjId_Crnr1, angleOfLine, CommonModule.beamBottomExternalPanelType, beamBottomLvl);

            var corner2_BasePoint = AEE_Utility.GetBasePointOfPolyline(cornerId2);
            var corner2_NrstBasePoint = kickerHlp.getNearestPointOfKickerCorner(cornerId2, corner2_BasePoint);

            if (flag_X_Axis == true)
            {
                corner2_NrstBasePoint = new Point3d(corner2_NrstBasePoint.X, offsetMaxLngthEndPoint_Cornr2.Y, corner2_NrstBasePoint.Z);
            }
            else
            {
                corner2_NrstBasePoint = new Point3d(offsetMaxLngthEndPoint_Cornr2.X, corner2_NrstBasePoint.Y, corner2_NrstBasePoint.Z);
            }

            ObjectId corner2_LineId = AEE_Utility.getLineId(corner2_NrstBasePoint, offsetMaxLngthEndPoint_Cornr2, true);

            beamIntersectPts = new List<Point3d>();
            if (checkOffsetLineIntersectWithBeam(corner2_LineId, out beamIntersectPts) == true)
            {
                AEE_Utility.deleteEntity(corner2_LineId);
                corner2_LineId = AEE_Utility.getLineId(beamIntersectPts[0], offsetMaxLngthEndPoint_Cornr2, false);
            }

            ObjectId corner2_OffstLineId = checkOffsetLineInOutsideTheCorner(corner2_LineId, cornerId2, CommonModule.externalCorner, flag_X_Axis, flag_Y_Axis);
            var listOfStrtEndPoint_Crnr2 = commonMod.getStartEndPointOfLine(corner2_LineId);
            var beamLineStrtPoint_Crnr2 = listOfStrtEndPoint_Crnr2[0];
            var beamLineEndPoint_Crnr2 = listOfStrtEndPoint_Crnr2[1];
            var lengthOfLine_Crnr2 = AEE_Utility.GetLengthOfLine(beamLineStrtPoint_Crnr2, beamLineEndPoint_Crnr2);
            lengthOfLine_Crnr2 = Math.Round(lengthOfLine_Crnr2);

            var listOfOffstStrtEndPoint_Crnr2 = commonMod.getStartEndPointOfLine(corner2_OffstLineId);
            var beamOffstLineStrtPoint_Crnr2 = listOfOffstStrtEndPoint_Crnr2[0];
            var beamOffstLineEndPoint_Crnr2 = listOfOffstStrtEndPoint_Crnr2[1];

            List<double> listOfWallPanelLength_Crnr2 = wallPanelHlp.getListOfWallPanelLength(lengthOfLine_Crnr2, CommonModule.bottomPanel_StndrdLngth, CommonModule.bottomPanel_MinLngth);

            List<ObjectId> listOfWallPanelLine_ObjId_Crnr2 = new List<ObjectId>();
            var listOfWallPanelRect_ObjId_Crnr2 = wallPanelHlp.drawWallPanels(xDataRegAppName, corner2_LineId, beamLineStrtPoint_Crnr2, beamLineEndPoint_Crnr2, beamOffstLineStrtPoint_Crnr2, beamOffstLineEndPoint_Crnr2, flag_X_Axis, flag_Y_Axis, listOfWallPanelLength_Crnr2, out listOfWallPanelLine_ObjId_Crnr2);

            AddBeamBottomExternalWallPanelObjs(xDataRegAppName, listOfWallPanelLine_ObjId_Crnr2, listOfWallPanelRect_ObjId_Crnr2);

            ValidateAndUpdateBeamBottomExternalPanel(xDataRegAppName, ref listOfWallPanelRect_ObjId_Crnr2, ref listOfWallPanelLength_Crnr2, ref listOfWallPanelLine_ObjId_Crnr2);

            MarkNonStandardPanelSize(listOfWallPanelRect_ObjId_Crnr2, listOfWallPanelLength_Crnr2);

            writeBeamBottomPanel(xDataRegAppName, wallPanelText, listOfWallPanelRect_ObjId_Crnr2, listOfWallPanelLine_ObjId_Crnr2, angleOfLine, CommonModule.beamBottomExternalPanelType, beamBottomLvl);
        }

        private void MarkNonStandardPanelSize(List<ObjectId> listOfPanelObj, List<double> listOfPanelLength)
        {

            for (int i = 0; i < listOfPanelLength.Count; i++)
            {
                if (listOfPanelLength[i] < CommonModule.internalCorner_MinLngth)
                {

                    List<Point3d> listOfBoundingPts = AEE_Utility.GetBoundingBoxOfPolyline(listOfPanelObj[i]);
                    ObjectId lineId = AEE_Utility.getLineId(listOfBoundingPts[0], listOfBoundingPts[1], false);
                    Point3d midPt = AEE_Utility.GetMidPoint(lineId);
                    AEE_Utility.deleteEntity(lineId);

                    var circleId = AEE_Utility.CreateCircle(midPt.X, midPt.Y, 0, (CommonModule.intrnlCornr_Flange1 / 2), CommonModule.internalCornerLyrName, 1);
                    AEE_Utility.changeColor(circleId, InternalWallHelper.nonStandardColorIndex);

                }
            }
        }
        private void drawBeamBottomWallPanel(string xDataRegAppName, double beamBottomPanelDeth, ObjectId cornerId1, ObjectId cornerId2, double angleOfLine, double beamBottomLvl)
        {
            KickerHelper kickerHlp = new KickerHelper();
            CommonModule commonMod = new CommonModule();
            WallPanelHelper wallPanelHlp = new WallPanelHelper();
            DeckPanelHelper deckPanelHlp = new DeckPanelHelper();

            var corner1_BasePoint = AEE_Utility.GetBasePointOfPolyline(cornerId1);
            var corner1_NrstBasePoint = kickerHlp.getNearestPointOfKickerCorner(cornerId1, corner1_BasePoint);
            var corner2_BasePoint = AEE_Utility.GetBasePointOfPolyline(cornerId2);
            var corner2_NrstBasePoint = kickerHlp.getNearestPointOfKickerCorner(cornerId2, corner2_BasePoint);

            var beamWallLineId = AEE_Utility.getLineId(corner1_NrstBasePoint, corner2_NrstBasePoint, false);
            var beamWallOffstLineId = getOffsetLineInOutsideTheCorner(beamWallLineId, cornerId1, beamBottomPanelDeth);

            double newLineLngth = AEE_Utility.GetLengthOfLine(beamWallLineId);
            newLineLngth = Math.Round(newLineLngth);
            List<double> listOfDeckPanelSpanLngth = deckPanelHlp.getListOfDeckPanelSpanLength(newLineLngth, CommonModule.bottomPanel_StndrdLngth, CommonModule.bottomPanel_MaxLngth, CommonModule.bottomPanel_MinLngth, CommonModule.beamPropInterval);

            double offsetDistOfExtrnalCrnrLine = (beamBottomPanelDeth); // Made changes by PBAC 05-06-2021 (beamBottomPanelDeth+ CommonModule.externalCorner)
            ObjectId cornerLineId = beamWallOffstLineId;
            var cornerOffstLineId = getOffsetLineInOutsideTheCorner(beamWallLineId, cornerId1, offsetDistOfExtrnalCrnrLine);

            double offsetDistOfPropLine = offsetDistOfExtrnalCrnrLine - 10;
            var propOffstLineId = getOffsetLineInOutsideTheCorner(beamWallLineId, cornerId1, offsetDistOfPropLine);

            List<ObjectId> listOfBeamWallPanelId = new List<ObjectId>();
            listOfBeamWallPanelId.Add(beamWallLineId);
            listOfBeamWallPanelId.Add(cornerOffstLineId);
            listOfBeamWallPanelId.Add(propOffstLineId);

            bool flagOfRightPanel = false;
            bool flagOfLeftPanel = false;
            checkWallIsInRightOrLeft(beamWallLineId, beamWallOffstLineId, out flagOfLeftPanel, out flagOfRightPanel);

            List<List<ObjectId>> listOfListOfBeamPanelAndCrnrPanelId = new List<List<ObjectId>>();
            listOfListOfBeamPanelAndCrnrPanelId.Add(listOfBeamWallPanelId);

            Entity beamWallLineEnt = AEE_Utility.GetEntityForRead(beamWallLineId);

            for (int index = 0; index < listOfListOfBeamPanelAndCrnrPanelId.Count; index++)
            {
                List<ObjectId> listOfBeamPanelAndCrnrPanelId = listOfListOfBeamPanelAndCrnrPanelId[index];
                List<Point3d> listOfBttmPanelLineStrtEndPoint = commonMod.getStartEndPointOfLine(listOfBeamPanelAndCrnrPanelId[0]);
                List<Point3d> listOfBttmPanelOffstLineStrtEndPoint = commonMod.getStartEndPointOfLine(listOfBeamPanelAndCrnrPanelId[1]);
                List<Point3d> listOfPromPanelOffstLineStrtEndPoint = commonMod.getStartEndPointOfLine(listOfBeamPanelAndCrnrPanelId[2]);

                List<Point3d> listOfRectPoint = new List<Point3d>();
                listOfRectPoint.Add(listOfBttmPanelLineStrtEndPoint[0]);
                listOfRectPoint.Add(listOfBttmPanelLineStrtEndPoint[1]);
                listOfRectPoint.Add(listOfBttmPanelOffstLineStrtEndPoint[0]);
                listOfRectPoint.Add(listOfBttmPanelOffstLineStrtEndPoint[1]);
                listOfRectPoint.Add(listOfPromPanelOffstLineStrtEndPoint[0]);
                listOfRectPoint.Add(listOfPromPanelOffstLineStrtEndPoint[1]);

                double panelWidth = offsetDistOfExtrnalCrnrLine;
                string beamBottomWallPanelType = CommonModule.beamBottomWallPanelType;
                string beamBottomPropPanelType = CommonModule.beamBottomPropPanelType;
                drawBeamBottomWallPanel(beamWallLineEnt, index, xDataRegAppName, listOfRectPoint, listOfDeckPanelSpanLngth, panelWidth, CommonModule.beamPropInterval,
                                        offsetDistOfPropLine, beamBottomWallPanelType, beamBottomPropPanelType, flagOfRightPanel, flagOfLeftPanel, beamBottomLvl);
            }
        }
        public static bool IsBeamCornerIntersectsWithAnotherBeam(ObjectId doorId, ObjectId doorCornerLineId)
        {
            bool intersectToBeam = false;
            if (BeamHelper.listOfBeamId_AllInsctPtsWithAnotherBeam.ContainsKey(doorId))
            {
                var tup = BeamHelper.listOfBeamId_AllInsctPtsWithAnotherBeam[doorId];
                var doorCornerLine = AEE_Utility.GetLine(doorCornerLineId);
                foreach (var t in tup)
                {
                    if (AEE_Utility.IsPointOnEntity(doorCornerLineId, t.Item2.Item1) || AEE_Utility.IsPointOnEntity(doorCornerLineId, t.Item2.Item2))
                    {
                        intersectToBeam = true;
                        break;
                    }
                }
            }

            return intersectToBeam;
        }


        private void drawBeamBottomWallPanel(Entity wallEnt, int indexOfLoop, string xDataRegAppName, List<Point3d> listOfRectPoint, List<double> listOfDeckPanelSpanLngth,
                                             double panelWidth, double propInterval, double propWidth, string wallPanelType, string propWallPanelType, bool flagOfRightPanel,
                                             bool flagOfLeftPanel, double beamBottomLvl)
        {
            Point3d beamPnl_1_StrtPnt = listOfRectPoint[0];
            Point3d beamPnl_1_EndPnt = listOfRectPoint[1];
            Point3d beamPnl_2_StrtPnt = listOfRectPoint[2];
            Point3d beamPnl_2_EndPnt = listOfRectPoint[3];

            Point3d propStartPoint1 = beamPnl_1_StrtPnt;
            Point3d propEndPoint1 = beamPnl_1_EndPnt;
            Point3d propStartPoint2 = listOfRectPoint[4];
            Point3d propEndPoint2 = listOfRectPoint[5];

            var deckPanelEnt = wallEnt;

            Point3d strtPoint1 = beamPnl_1_StrtPnt;
            Point3d strtPoint2 = beamPnl_2_StrtPnt;

            var lngth = AEE_Utility.GetLengthOfLine(beamPnl_1_StrtPnt, beamPnl_2_StrtPnt);
            var lngth1 = AEE_Utility.GetLengthOfLine(beamPnl_1_EndPnt, beamPnl_2_EndPnt);

            WallPanelHelper wallPanelHlp = new WallPanelHelper();

            var angleOfLine = AEE_Utility.GetAngleOfLine(beamPnl_1_StrtPnt, beamPnl_1_EndPnt);

            for (int j = 0; j < listOfDeckPanelSpanLngth.Count; j++)
            {
                double deckPanelSpanLength = listOfDeckPanelSpanLngth[j];

                Point2d lengthPoint = AEE_Utility.get_XY(angleOfLine, deckPanelSpanLength);
                Point3d endPoint1 = new Point3d((strtPoint1.X + lengthPoint.X), (strtPoint1.Y + lengthPoint.Y), 0);
                Point3d endPoint2 = new Point3d((strtPoint2.X + lengthPoint.X), (strtPoint2.Y + lengthPoint.Y), 0);

                propEndPoint1 = new Point3d((propStartPoint1.X + lengthPoint.X), (propStartPoint1.Y + lengthPoint.Y), 0);
                propEndPoint2 = new Point3d((propStartPoint2.X + lengthPoint.X), (propStartPoint2.Y + lengthPoint.Y), 0);

                ObjectId beamPanelLineId = AEE_Utility.getLineId(deckPanelEnt, strtPoint1, endPoint1, false);
                double beamPanelLineAngle = AEE_Utility.GetAngleOfLine(beamPanelLineId);

                var beamPanelRectId = AEE_Utility.createRectangle(strtPoint1, endPoint1, endPoint2, strtPoint2, CommonModule.beamBottomWallPanelLayerName, CommonModule.beamBottomWallPanelLayerColor);

                ValidateAndUpdateBeamBottomPanel(xDataRegAppName, ref strtPoint1, ref endPoint1, ref endPoint2, ref strtPoint2, ref beamPanelRectId, ref beamPanelLineId);

                deckPanelSpanLength = AEE_Utility.GetLengthOfLine(beamPanelLineId);

                listOfBeamBottomWallPanelObj.Add(new Tuple<string, ObjectId, ObjectId>(xDataRegAppName, beamPanelLineId, beamPanelRectId));

                AEE_Utility.AttachXData(beamPanelRectId, xDataRegAppName, CommonModule.xDataAsciiName);

                Point3d dimTextPoint = WallPanelHelper.getCenterPointOfPanelRectangle(beamPanelRectId);

                string beamBottomPanelName = PanelLayout_UI.beamBottomPanelName;
                if (flagOfRightPanel == true)
                {
                    beamBottomPanelName = beamBottomPanelName + "(R)";
                }
                if (flagOfLeftPanel == true)
                {
                    beamBottomPanelName = beamBottomPanelName + "(L)";
                }

                string deckPanelText = DeckPanelHelper.getWallPanelText(panelWidth, beamBottomPanelName, deckPanelSpanLength);

                string wallDescp = deckPanelText;
                ObjectId dimTextId2 = wallPanelHlp.writeDimensionTextInWallPanel(deckPanelText, beamPanelLineId, dimTextPoint, beamPanelLineAngle, CommonModule.beamBottomWallPanelTextLayerName, CommonModule.beamBottomWallPanelLayerColor);

                wallPanelHlp.setBOMDataOfWallPanel(dimTextId2, beamPanelRectId, beamPanelLineId, panelWidth, 0, deckPanelSpanLength, beamBottomPanelName, wallDescp, wallPanelType, beamBottomLvl.ToString());

                Point3d gapStrtPoint1 = endPoint1;
                Point3d gapStrtPoint2 = endPoint2;
                propStartPoint1 = propEndPoint1;
                propStartPoint2 = propEndPoint2;

                Point2d gapPoint = AEE_Utility.get_XY(angleOfLine, propInterval);
                Point3d gapEndPoint1 = new Point3d((gapStrtPoint1.X + gapPoint.X), (gapStrtPoint1.Y + gapPoint.Y), 0);
                Point3d gapEndPoint2 = new Point3d((gapStrtPoint2.X + gapPoint.X), (gapStrtPoint2.Y + gapPoint.Y), 0);

                propEndPoint1 = new Point3d((propStartPoint1.X + gapPoint.X), (propStartPoint1.Y + gapPoint.Y), 0);
                propEndPoint2 = new Point3d((propStartPoint2.X + gapPoint.X), (propStartPoint2.Y + gapPoint.Y), 0);

                if (j != (listOfDeckPanelSpanLngth.Count - 1) && indexOfLoop == 0)
                {
                    List<ObjectId> listOfPropObjId = drawBeamBottom_PropRect(xDataRegAppName, deckPanelEnt, propStartPoint1, propEndPoint1, propStartPoint2, propEndPoint2,
                                                                             beamPanelLineAngle, propWidth, propInterval, propWallPanelType, beamBottomLvl);
                }

                strtPoint1 = gapEndPoint1;
                strtPoint2 = gapEndPoint2;
                propStartPoint1 = propEndPoint1;
                propStartPoint2 = propEndPoint2;
            }
        }


        private List<ObjectId> drawBeamBottom_PropRect(string xDataRegAppName, Entity deckPanelEnt, Point3d gapStrtPoint1, Point3d gapEndPoint1, Point3d gapStrtPoint2,
                                                       Point3d gapEndPoint2, double angleOfPropPanel, double panelWidth, double propInterval, string wallPanelType, double beamBottomLvl)
        {
            WallPanelHelper wallPanelHlp = new WallPanelHelper();

            List<ObjectId> listOfPropObjId = new List<ObjectId>();

            ObjectId deckWallLineId = AEE_Utility.getLineId(deckPanelEnt, gapStrtPoint1, gapStrtPoint2, false);

            ObjectId propRectId = AEE_Utility.createRectangle(gapStrtPoint2, gapEndPoint2, gapEndPoint1, gapStrtPoint1, CommonModule.beamBottomWallPanelLayerName, CommonModule.beamBottomWallPanelLayerColor);
            AEE_Utility.AttachXData(propRectId, xDataRegAppName, CommonModule.xDataAsciiName);
            listOfPropObjId.Add(propRectId);

            Point3d dimTextPoint = WallPanelHelper.getCenterPointOfPanelRectangle(propRectId);

            ////double propRadius = CommonModule.propRadius;
            ////var circleId = AEE_Utility.CreateCircle(dimTextPoint.X, dimTextPoint.Y, 0, propRadius, CommonModule.beamBottomWallPanelTextLayerName, CommonModule.beamBottomWallPanelLayerColor);
            ////listOfPropObjId.Add(circleId);

            string beamBottomPropPanelName = PanelLayout_UI.beamBottomPropPanelName;
            string deckPanelText = DeckPanelHelper.getWallPanelText(panelWidth, beamBottomPropPanelName, propInterval);
            string wallDescp = deckPanelText;
            ObjectId dimTextId2 = wallPanelHlp.writeDimensionTextInWallPanel(deckPanelText, deckWallLineId, dimTextPoint, angleOfPropPanel, CommonModule.beamBottomWallPanelTextLayerName, CommonModule.beamBottomWallPanelLayerColor);
            listOfPropObjId.Add(dimTextId2);

            wallPanelHlp.setBOMDataOfWallPanel(dimTextId2, propRectId, deckWallLineId, panelWidth, 0, propInterval, beamBottomPropPanelName, wallDescp, wallPanelType, beamBottomLvl.ToString());
            return listOfPropObjId;
        }


        private List<Point3d> getOffsetMaxLengthLineId(ObjectId maxLngthLineId, ObjectId cornerId, Point3d cornerBasePoint, double offsetValue)
        {
            ObjectId maxLengthCrnrOffstLineId = new ObjectId();

            var offsetId = AEE_Utility.OffsetEntity(10, maxLngthLineId, false);
            var listOfInsct = AEE_Utility.InterSectionBetweenTwoEntity(cornerId, offsetId);
            if (listOfInsct.Count == 0)
            {
                maxLengthCrnrOffstLineId = AEE_Utility.OffsetEntity(-offsetValue, maxLngthLineId, false);
            }
            else
            {
                maxLengthCrnrOffstLineId = AEE_Utility.OffsetEntity(offsetValue, maxLngthLineId, false);
            }

            List<Point3d> listOfMaxLngthStrtEndPoint = AEE_Utility.GetStartEndPointOfLine(maxLngthLineId);
            List<double> listOfLength = new List<double>();
            for (int m = 0; m < listOfMaxLngthStrtEndPoint.Count; m++)
            {
                var point = listOfMaxLngthStrtEndPoint[m];
                var length = AEE_Utility.GetLengthOfLine(point, cornerBasePoint);
                listOfLength.Add(length);
            }
            double minLngth = listOfLength.Min();
            int indexOfMinLngth = listOfLength.IndexOf(minLngth);
            Point3d minLngthPoint = listOfMaxLngthStrtEndPoint[indexOfMinLngth];

            List<Point3d> listOfOffstMaxLngthStrtEndPoint = AEE_Utility.GetStartEndPointOfLine(maxLengthCrnrOffstLineId);

            Point3d maxLngthEndPoint = new Point3d();
            Point3d offsetMaxLngthEndPoint = new Point3d();


            if (indexOfMinLngth == 0)
            {
                maxLngthEndPoint = listOfMaxLngthStrtEndPoint[1];
                offsetMaxLngthEndPoint = listOfOffstMaxLngthStrtEndPoint[1];
            }
            else
            {
                maxLngthEndPoint = listOfMaxLngthStrtEndPoint[0];
                offsetMaxLngthEndPoint = listOfOffstMaxLngthStrtEndPoint[0];
            }
            List<Point3d> listOfBeamBottomPanelLineEndPoint = new List<Point3d>();
            listOfBeamBottomPanelLineEndPoint.Add(maxLngthEndPoint);
            listOfBeamBottomPanelLineEndPoint.Add(offsetMaxLngthEndPoint);

            AEE_Utility.deleteEntity(maxLengthCrnrOffstLineId);
            AEE_Utility.deleteEntity(offsetId);

            return listOfBeamBottomPanelLineEndPoint;
        }


        private ObjectId getOffsetLineInOutsideTheCorner(ObjectId lineId, ObjectId cornerId, double offsetValue)
        {
            ObjectId offsetLineId = new ObjectId();
            var angleOfLine = AEE_Utility.GetAngleOfLine(lineId);
            var point = AEE_Utility.get_XY(angleOfLine, 5);
            List<Point3d> listOfStrtEndPoint = AEE_Utility.GetStartEndPointOfLine(lineId);
            Point3d strtPoint = listOfStrtEndPoint[0];
            Point3d endPoint = listOfStrtEndPoint[1];
            strtPoint = new Point3d((strtPoint.X + point.X), (strtPoint.Y + point.Y), 0);
            endPoint = new Point3d((endPoint.X - point.X), (endPoint.Y - point.Y), 0);
            ObjectId lineId_ForInsct = AEE_Utility.getLineId(strtPoint, endPoint, false);

            var offsetId = AEE_Utility.OffsetEntity(10, lineId_ForInsct, false);
            var listOfInsct = AEE_Utility.InterSectionBetweenTwoEntity(cornerId, offsetId);
            if (listOfInsct.Count == 0)
            {
                offsetLineId = AEE_Utility.OffsetEntity(offsetValue, lineId, false);
            }
            else
            {
                offsetLineId = AEE_Utility.OffsetEntity(-offsetValue, lineId, false);
            }
            AEE_Utility.deleteEntity(offsetId);
            AEE_Utility.deleteEntity(lineId_ForInsct);

            return offsetLineId;
        }


        private ObjectId getOffsetLineForBeamBottomExternalCorner(ObjectId lineId, ObjectId cornerId, double offsetValue, bool flag_X_Axis, bool flag_Y_Axis)
        {
            ObjectId offsetLineId = new ObjectId();
            var angleOfLine = AEE_Utility.GetAngleOfLine(lineId);
            var point = AEE_Utility.get_XY(angleOfLine, 5);
            List<Point3d> listOfStrtEndPoint = AEE_Utility.GetStartEndPointOfLine(lineId);
            Point3d strtPoint = listOfStrtEndPoint[0];
            Point3d endPoint = listOfStrtEndPoint[1];
            strtPoint = new Point3d((strtPoint.X - point.X), (strtPoint.Y - point.Y), 0);
            endPoint = new Point3d((endPoint.X + point.X), (endPoint.Y + point.Y), 0);
            ObjectId lineId_ForInsct = AEE_Utility.getLineId(strtPoint, endPoint, false);

            Point3d cornerIdBasePt = AEE_Utility.GetBasePointOfPolyline(cornerId);
            double checkOffsetDist = 0;

            if (flag_X_Axis == true)
            {
                checkOffsetDist = Math.Abs(cornerIdBasePt.Y - listOfStrtEndPoint[0][1]) - 5;
            }
            else
            {
                checkOffsetDist = Math.Abs(cornerIdBasePt.X - listOfStrtEndPoint[0][0]) - 5;
            }

            var offsetId = AEE_Utility.OffsetEntity(checkOffsetDist, lineId_ForInsct, false);
            var listOfInsct = AEE_Utility.InterSectionBetweenTwoEntity(cornerId, offsetId);
            if (listOfInsct.Count == 0)
            {
                offsetLineId = AEE_Utility.OffsetEntity(offsetValue, lineId, false);
            }
            else
            {
                offsetLineId = AEE_Utility.OffsetEntity(-offsetValue, lineId, false);
            }
            AEE_Utility.deleteEntity(offsetId);
            AEE_Utility.deleteEntity(lineId_ForInsct);

            return offsetLineId;
        }


        private ObjectId checkOffsetLineInOutsideTheCorner(ObjectId lineId, ObjectId cornerId, double offsetValue, bool flag_X_Axis, bool flag_Y_Axis)
        {
            ObjectId offsetLineId = new ObjectId();

            List<Point3d> listOfStrtEndPoint = AEE_Utility.GetStartEndPointOfLine(lineId);

            Point3d cornerIdBasePt = AEE_Utility.GetBasePointOfPolyline(cornerId);
            double checkOffsetDist = 0;

            if (flag_X_Axis == true)
            {
                checkOffsetDist = Math.Abs(cornerIdBasePt.Y - listOfStrtEndPoint[0][1]) - 5;
            }
            else
            {
                checkOffsetDist = Math.Abs(cornerIdBasePt.X - listOfStrtEndPoint[0][0]) - 5;
            }

            var offsetId = AEE_Utility.OffsetEntity(checkOffsetDist, lineId, false);
            var midPoint = AEE_Utility.GetMidPoint(offsetId);

            var flagOfInside = AEE_Utility.GetPointIsInsideThePolyline(cornerId, midPoint);
            if (flagOfInside == true)
            {
                offsetLineId = AEE_Utility.OffsetEntity(-offsetValue, lineId, false);
            }
            else
            {
                offsetLineId = AEE_Utility.OffsetEntity(offsetValue, lineId, false);
            }
            AEE_Utility.deleteEntity(offsetId);

            return offsetLineId;
        }

        private bool checkOffsetLineIntersectWithBeam(ObjectId lineId, out List<Point3d> InsctPts)
        {
            ObjectId beamObj = new ObjectId();

            InsctPts = new List<Point3d>();

            for (int i = 0; i < CheckShellPlanHelper.listOfSelectedOffsetBeam_ObjId.Count; i++)
            {
                beamObj = CheckShellPlanHelper.listOfSelectedOffsetBeam_ObjId[i];
                AEE_Utility.MoveEntity(beamObj, CreateShellPlanHelper.moveVector_ForBeamBottmLayout);
                InsctPts = AEE_Utility.InterSectionPointBetweenPolyLineAndLine(beamObj, lineId, Intersect.OnBothOperands);
                AEE_Utility.MoveEntity(beamObj, CreateShellPlanHelper.moveVector_ForBeamBottmLayout * -1);
                if (InsctPts.Count > 0)
                {
                    return true;
                }
            }

            return false;
        }
        private void checkWallIsInRightOrLeft(ObjectId beamBttmPanelLineId, ObjectId beamBttmPanelOffstLineId, out bool flagOfLeftPanel, out bool flagOfRightPanel)
        {
            flagOfRightPanel = false;
            flagOfLeftPanel = false;

            var listOfBeamLineStrtEndPoint = AEE_Utility.GetStartEndPointOfLine(beamBttmPanelLineId);
            var listOfBeamOffstLineStrtEndPoint = AEE_Utility.GetStartEndPointOfLine(beamBttmPanelOffstLineId);
            Point3d beamLineStrtPoint = listOfBeamLineStrtEndPoint[0];
            Point3d beamOffstLineStrtPoint = listOfBeamOffstLineStrtEndPoint[0];

            double beamBttmPanelLineAngle = AEE_Utility.GetAngleOfLine(beamBttmPanelLineId);

            CommonModule commonMod = new CommonModule();
            bool flag_X_Axis = true;
            bool flag_Y_Axis = false;
            commonMod.checkAngleOfLine_Axis(beamBttmPanelLineAngle, out flag_X_Axis, out flag_Y_Axis);
            if (flag_X_Axis == true)
            {
                if (beamLineStrtPoint.Y > beamOffstLineStrtPoint.Y)
                {
                    flagOfLeftPanel = true;
                }
                else
                {
                    flagOfRightPanel = true;
                }
            }
            else if (flag_Y_Axis == true)
            {
                if (beamLineStrtPoint.X > beamOffstLineStrtPoint.X)
                {
                    flagOfLeftPanel = true;
                }
                else
                {
                    flagOfRightPanel = true;
                }
            }
        }


        public void drawBeamSidePanel(ProgressForm progressForm, string wallCreationMsg)
        {
            WallPanelHelper wallPanelHlpr = new WallPanelHelper();
            CommonModule commonMod = new CommonModule();
            for (int index = 0; index < listOfListOfBeamSidePanelLineId.Count; index++)
            {
                if ((index % 50) == 0)
                {
                    progressForm.ReportProgress(1, wallCreationMsg);
                }

                List<ObjectId> listOfBeamSidePanelLineId = listOfListOfBeamSidePanelLineId[index];

                ObjectId sunkanSlabId = listOfSunkanSlabObjId[index];
                ObjectId beamSidePanelLineId = listOfBeamSidePanelLineId[0];
                ObjectId offstBeamSidePanelLineId = listOfBeamSidePanelLineId[1];
                ObjectId beamId = listOfBeamSideWallId[index];//Changes made on 12/03/2023 by SDM

                var xDataRegAppName = AEE_Utility.GetXDataRegisterAppName(beamSidePanelLineId);

                double beamSidePanelLineLngth = AEE_Utility.GetLengthOfLine(beamSidePanelLineId);
                bool flagOfNewBeamWallPnlCreate = false;
                var layerName = AEE_Utility.GetLayerName(beamSidePanelLineId);
                double maxBeamWallPanelLength = getBeamWallPanelLength(beamId, out flagOfNewBeamWallPnlCreate, layerName, sunkanSlabId.IsValid, sunkanSlabId);

                //Changes made on 08/04/2023 by SDM
                var beamline = AEE_Utility.GetLine(beamSidePanelLineId);
                var offsetbeamline = AEE_Utility.GetLine(offstBeamSidePanelLineId);
                var beamEnt = AEE_Utility.GetEntityForRead(beamId);
                var intersections = listOfBeamId_AllInsctPtsWithAnotherBeam.ContainsKey(beamId) ? listOfBeamId_AllInsctPtsWithAnotherBeam[beamId] : new List<Tuple<ObjectId, (Point3d, Point3d)>>();
                //--------------------------------------

                if (flagOfNewBeamWallPnlCreate == true)
                {
                    beamSidePanelLineLngth = Math.Round(beamSidePanelLineLngth);
                    var listOfBeamLineStrtEndPoint = commonMod.getStartEndPointOfLine(beamSidePanelLineId);
                    var listOfBeamOffstLineStrtEndPoint = commonMod.getStartEndPointOfLine(offstBeamSidePanelLineId);

                    Point3d beamLineStrtPoint = listOfBeamLineStrtEndPoint[0];
                    Point3d beamLineEndPoint = listOfBeamLineStrtEndPoint[1];
                    Point3d beamOffstLineStrtPoint = listOfBeamOffstLineStrtEndPoint[0];
                    Point3d beamOffstLineEndPoint = listOfBeamOffstLineStrtEndPoint[1];

                    double angleOfLine = AEE_Utility.GetAngleOfLine(beamLineStrtPoint.X, beamLineStrtPoint.Y, beamLineEndPoint.X, beamLineEndPoint.Y);
                    bool flag_X_Axis = false;
                    bool flag_Y_Axis = false;

                    commonMod.checkAngleOfLine_Axis(angleOfLine, out flag_X_Axis, out flag_Y_Axis);

                    //Changes made on 08/04/2023 by SDM fix side panel partitions when there is T beam intersection 
                    intersections = intersections.Where(insct => (flag_Y_Axis && insct.Item2.Item1.Y <= beamLineEndPoint.Y && insct.Item2.Item2.Y >= beamLineStrtPoint.Y) || (flag_X_Axis && insct.Item2.Item1.X <= beamLineEndPoint.X && insct.Item2.Item2.X >= beamLineStrtPoint.X)).ToList();

                    List<double> listOfWallPanelLength = new List<double>();
                    List<ObjectId> listOfWallPanelLine_ObjId = new List<ObjectId>();
                    List<ObjectId> listOfWallPanelRect_ObjId = new List<ObjectId>();
                    if (Beam_UI_Helper.checkBeamLayerIsExist(beamEnt.Layer))
                    {

                        var panel_parts = new List<(Point3d, Point3d)>();
                        var all_pts = new List<Point3d>() { beamLineStrtPoint, beamLineEndPoint };
                        if (intersections.Count() > 0)
                        {
                            var _intersections = intersections.Select(X => X.Item2).ToList();
                            _intersections.ForEach(insct =>
                            {
                                var pt1 = insct.Item1;
                                var pt2 = insct.Item2;
                                Vector3d shift = pt2 - pt1;
                                shift *= CommonModule.internalCorner / shift.Length;
                                pt1 -= shift;
                                pt2 += shift;
                                if (flag_X_Axis)
                                {
                                    insct.Item1 = new Point3d(pt1.X, beamLineStrtPoint.Y, 0);
                                    insct.Item2 = new Point3d(pt2.X, beamLineStrtPoint.Y, 0);
                                }
                                else
                                {
                                    insct.Item1 = new Point3d(beamLineStrtPoint.X, pt1.Y, 0);
                                    insct.Item2 = new Point3d(beamLineStrtPoint.X, pt2.Y, 0);
                                }
                                all_pts.Add(insct.Item1);
                                all_pts.Add(insct.Item2);
                            });
                            all_pts = all_pts.OrderBy(X => flag_X_Axis ? X.X : X.Y).ToList();
                        }

                        for (int i = 0; i < all_pts.Count - 1; i++)
                        {
                            var st = all_pts[i];
                            var end = all_pts[i + 1];
                            if (Math.Round(st.DistanceTo(end)) > beamSidePanelLineLngth)
                                continue;

                            var offset_st = st + new Vector3d(beamOffstLineStrtPoint.X - beamLineStrtPoint.X, beamOffstLineStrtPoint.Y - beamLineStrtPoint.Y, 0);
                            var offset_end = end + new Vector3d(beamOffstLineStrtPoint.X - beamLineStrtPoint.X, beamOffstLineStrtPoint.Y - beamLineStrtPoint.Y, 0);

                            var sidePanelLineId = AEE_Utility.getLineId(beamline, st, end, false);
                            var length = Math.Round(AEE_Utility.GetLengthOfLine(sidePanelLineId));
                            listOfWallPanelLength = wallPanelHlpr.getListOfWallPanelLength(length, maxBeamWallPanelLength, PanelLayout_UI.minWidthOfPanel);

                            List<ObjectId> _listOfWallPanelLine_ObjId = new List<ObjectId>();
                            listOfWallPanelRect_ObjId.AddRange(wallPanelHlpr.drawWallPanels(xDataRegAppName, sidePanelLineId, st, end, offset_st, offset_end, flag_X_Axis, flag_Y_Axis, listOfWallPanelLength, out _listOfWallPanelLine_ObjId));
                            listOfWallPanelLine_ObjId.AddRange(_listOfWallPanelLine_ObjId);
                        }
                    }//-----------------------------------------------
                    else
                    {
                        listOfWallPanelLength = wallPanelHlpr.getListOfWallPanelLength(beamSidePanelLineLngth, maxBeamWallPanelLength, PanelLayout_UI.minWidthOfPanel);
                        listOfWallPanelRect_ObjId = wallPanelHlpr.drawWallPanels(xDataRegAppName, beamSidePanelLineId, beamLineStrtPoint, beamLineEndPoint, beamOffstLineStrtPoint, beamOffstLineEndPoint, flag_X_Axis, flag_Y_Axis, listOfWallPanelLength, out listOfWallPanelLine_ObjId);
                    }
                    Vector3d moveVectorOfBeamLine = new Vector3d(0, 0, 0);

                    drawBeamPanel(sunkanSlabId, beamEnt.Layer, listOfWallPanelRect_ObjId, listOfWallPanelLine_ObjId, moveVectorOfBeamLine, layerName);

                    AEE_Utility.deleteEntity(listOfWallPanelRect_ObjId);
                }
            }
        }


        private void drawBeamPanel(ObjectId sunkanSlabId, string beamLayerNameInsctToWall, List<ObjectId> listOfWallPanelRect_ObjId, List<ObjectId> listOfWallPanelLine_ObjId, Vector3d moveVectorOfBeamLine, String wallLayer)
        {
            //Changes made on 13/03/2023 by SDM
            double heightOfBeamSidePanel = 0;
            double beamBottom = 0;
            if (beamLayerNameInsctToWall.Contains(Beam_UI_Helper.offsetBeamStrtText))
            {
                beamBottom = Beam_UI_Helper.getOffsetBeamBottom(beamLayerNameInsctToWall);
                heightOfBeamSidePanel = GeometryHelper.getHeightOfBeamSidePanel(InternalWallSlab_UI_Helper.getSlabBottom(wallLayer), beamBottom, PanelLayout_UI.SL, sunkanSlabId.IsValid, sunkanSlabId);
            }
            else if (beamLayerNameInsctToWall.Contains(Beam_UI_Helper.beamStrtText))
            {
                double levelDiffSunkan = 0;
                if (sunkanSlabId.IsValid == true)
                {
                    levelDiffSunkan = SunkanSlabHelper.getSunkanSlabLevelDifference(sunkanSlabId);
                }
                heightOfBeamSidePanel = Beam_UI_Helper.getBeamSidePanelHeight(beamLayerNameInsctToWall, wallLayer);
                beamBottom = Beam_UI_Helper.GetBeamLintelLevelWithoutSunkan(beamLayerNameInsctToWall, wallLayer) - levelDiffSunkan;
            }
            //-------------------
            if (heightOfBeamSidePanel > 0)
            {
                var listOfBeamPanelRectId = AEE_Utility.copyColonEntity(CreateShellPlanHelper.moveVector_ForBeamLayout, listOfWallPanelRect_ObjId);
                AEE_Utility.MoveEntity(listOfBeamPanelRectId, moveVectorOfBeamLine);

                var listOfBeamPanelLineId = AEE_Utility.copyColonEntity(CreateShellPlanHelper.moveVector_ForBeamLayout, listOfWallPanelLine_ObjId);
                AEE_Utility.MoveEntity(listOfBeamPanelLineId, moveVectorOfBeamLine);

                listOfBeamPanelRectId = checkDistBtwBeamWallPanelToBeamCorner(listOfBeamPanelRectId, listOfBeamPanelLineId);

                WallPanelHelper wallPanelHlpr = new WallPanelHelper();
                List<ObjectId> listOfWallTextId = new List<ObjectId>();
                List<ObjectId> listOfWallXTextId = new List<ObjectId>();
                List<ObjectId> listOfCircleId = new List<ObjectId>();
                string doorLayer = ""; //RTJ 10-06-2021
                string windowLayer = ""; //RTJ 10-06-2021
                wallPanelHlpr.writeTextInWallPanel(CommonModule.beamWallPanelType, sunkanSlabId, beamLayerNameInsctToWall, listOfBeamPanelRectId, listOfBeamPanelLineId,
                                                   heightOfBeamSidePanel, PanelLayout_UI.beamPanelName, 0, "", PanelLayout_UI.RC, CommonModule.wallPanelTextLyrName,
                                                   CommonModule.wallPanelLyrColor, true, out listOfWallTextId, out listOfWallXTextId, out listOfCircleId, doorLayer, windowLayer, beamBottom); //RTJ 10-06-2021
            }
        }


        private List<ObjectId> checkDistBtwBeamWallPanelToBeamCorner(List<ObjectId> listOfBeamPanelRectId, List<ObjectId> listOfBeamPanelLineId)
        {
            for (int j = 0; j < listOfAllBeamCornerId_ForStretch.Count; j++)
            {
                ObjectId beamCrnrId = listOfAllBeamCornerId_ForStretch[j];
                if (beamCrnrId.IsErased == false)
                {
                    for (int k = 0; k < listOfBeamPanelRectId.Count; k++)
                    {
                        ObjectId beamRectId = listOfBeamPanelRectId[k];
                        if (beamRectId.IsErased == false)
                        {
                            ObjectId beamLineId = listOfBeamPanelLineId[k];
                            if (AEE_Utility.GetLengthOfLine(beamLineId) == 0)
                                continue;
                            ObjectId beamInsideOffstRectId = getBeamRectOffsetInInside(beamRectId);
                            var listOfInsct = AEE_Utility.InterSectionBetweenTwoEntity(beamCrnrId, beamInsideOffstRectId);
                            if (listOfInsct.Count != 0)
                            {
                                var listOfInsctPointOfLine = AEE_Utility.InterSectionBetweenTwoEntity(beamCrnrId, beamLineId);
                                if (listOfInsctPointOfLine.Count != 0)
                                {
                                    ObjectId newBeamPanelRectId = changeBeamWallPanel(beamLineId, beamRectId, beamCrnrId, listOfInsctPointOfLine);
                                    if (newBeamPanelRectId.IsValid == false)
                                        continue;

                                    listOfBeamPanelRectId[k] = newBeamPanelRectId;
                                    AEE_Utility.deleteEntity(beamRectId);
                                    break;
                                }
                            }
                            AEE_Utility.deleteEntity(beamInsideOffstRectId);
                        }
                    }
                }
            }
            return listOfBeamPanelRectId;
        }


        private ObjectId changeBeamWallPanel(ObjectId beamLineId, ObjectId beamRectId, ObjectId beamCrnrId, List<Point3d> listOfInsctPointOfLine)
        {
            List<Point3d> listOfBeamPanelLineStrtEndPoint = AEE_Utility.GetStartEndPointOfLine(beamLineId);
            Point3d beamCrnrBasePnt = AEE_Utility.GetBasePointOfPolyline(beamCrnrId);

            List<double> listOfLengthBtwCrnrToWallLine = new List<double>();
            List<double> listOfLengthBtwCrnrToInsctPoint = new List<double>();

            for (int j = 0; j < listOfBeamPanelLineStrtEndPoint.Count; j++)
            {
                Point3d linePoint = listOfBeamPanelLineStrtEndPoint[j];
                var lngthBtwCrnrToWallLine = AEE_Utility.GetLengthOfLine(beamCrnrBasePnt, linePoint);
                listOfLengthBtwCrnrToWallLine.Add(lngthBtwCrnrToWallLine);

                foreach (var insctPoint in listOfInsctPointOfLine)
                {
                    var lngthBtwCrnrToInsctPoint = AEE_Utility.GetLengthOfLine(beamCrnrBasePnt, insctPoint);
                    listOfLengthBtwCrnrToInsctPoint.Add(lngthBtwCrnrToInsctPoint);
                }
            }
            double maxLengthBtwCrnrToWallLine = listOfLengthBtwCrnrToInsctPoint.Max();
            int indexOfMaxLngthBtwCrnrToWallLine = listOfLengthBtwCrnrToInsctPoint.IndexOf(maxLengthBtwCrnrToWallLine);
            Point3d maxLngthInsctPoint = listOfInsctPointOfLine[indexOfMaxLngthBtwCrnrToWallLine];

            double minLengthBtwCrnrToWallLine = listOfLengthBtwCrnrToWallLine.Min();
            int indexOfMinLngthBtwCrnrToWallLine = listOfLengthBtwCrnrToWallLine.IndexOf(minLengthBtwCrnrToWallLine);
            Point3d inSideLinePoint = listOfBeamPanelLineStrtEndPoint[indexOfMinLngthBtwCrnrToWallLine];

            Point3d strtPointOfLine = new Point3d();
            Point3d endPointOfLine = new Point3d();
            int indexOfOutsidePoint = listOfBeamPanelLineStrtEndPoint.IndexOf(inSideLinePoint);
            if (indexOfOutsidePoint == 0)
            {
                strtPointOfLine = maxLngthInsctPoint;
                endPointOfLine = listOfBeamPanelLineStrtEndPoint[1];
            }
            else
            {
                strtPointOfLine = listOfBeamPanelLineStrtEndPoint[0];
                endPointOfLine = maxLngthInsctPoint;
            }
            if (strtPointOfLine.DistanceTo(endPointOfLine) < 1)
                return new ObjectId();
            InternalWallHelper internalWallHlp = new InternalWallHelper();
            internalWallHlp.changeLinePoint(beamLineId, strtPointOfLine, endPointOfLine);

            ObjectId beamOffstLineId = new ObjectId();
            var offstId = AEE_Utility.OffsetEntity((CommonModule.panelDepth / 2), beamLineId, false);
            var listOfInsct = AEE_Utility.InterSectionBetweenTwoEntity(beamCrnrId, offstId);
            if (listOfInsct.Count != 0)
            {
                beamOffstLineId = AEE_Utility.OffsetEntity(CommonModule.panelDepth, beamLineId, false);
            }
            else
            {
                beamOffstLineId = AEE_Utility.OffsetEntity(-CommonModule.panelDepth, beamLineId, false);
            }

            var listOfStrtEndPnt = AEE_Utility.GetStartEndPointOfLine(beamOffstLineId);

            var beamRectEnt = AEE_Utility.GetEntityForRead(beamRectId);
            ObjectId newBeamPanelRectId = AEE_Utility.createRectangle(strtPointOfLine, endPointOfLine, listOfStrtEndPnt[1], listOfStrtEndPnt[0], beamRectEnt.Layer, beamRectEnt.ColorIndex);
            var xDataRegAppName = AEE_Utility.GetXDataRegisterAppName(newBeamPanelRectId);
            AEE_Utility.AttachXData(newBeamPanelRectId, xDataRegAppName, CommonModule.xDataAsciiName);

            AEE_Utility.deleteEntity(offstId);
            AEE_Utility.deleteEntity(beamOffstLineId);

            return newBeamPanelRectId;
        }


        private ObjectId getBeamRectOffsetInInside(ObjectId beamRectId)
        {
            double offsetValue = 2;

            var listOfBeamRectExplodeId = AEE_Utility.ExplodeEntity(beamRectId);
            ObjectId beamExplodeLineId = listOfBeamRectExplodeId[0];
            ObjectId beamExplodeOffstLineId = AEE_Utility.OffsetLine(beamExplodeLineId, offsetValue, false);
            var offsetMidPoint = AEE_Utility.GetMidPoint(beamExplodeOffstLineId);

            var flagOfInside = AEE_Utility.GetPointIsInsideThePolyline(beamRectId, offsetMidPoint);
            if (flagOfInside == false)
            {
                AEE_Utility.deleteEntity(beamExplodeOffstLineId);

                beamExplodeOffstLineId = AEE_Utility.OffsetLine(beamExplodeLineId, -offsetValue, false);
                offsetMidPoint = AEE_Utility.GetMidPoint(beamExplodeOffstLineId);
            }
            ObjectId beamOffstRectId = AEE_Utility.OffsetEntity_WithoutLine(offsetValue, offsetMidPoint, beamRectId, false);

            AEE_Utility.deleteEntity(listOfBeamRectExplodeId);
            AEE_Utility.deleteEntity(beamExplodeOffstLineId);

            return beamOffstRectId;
        }


        public Vector3d getMoveVectorOfBeamWallLine(ObjectId wallPanelLineId, ObjectId nearestBeamWallLineId, double distanceBtwWallToBeam, out bool flagOfMove)
        {
            flagOfMove = false;
            Vector3d moveVectorOfBeamLine = new Vector3d();

            var wallPanelLine = AEE_Utility.GetLine(wallPanelLineId);
            if (wallPanelLine.StartPoint == wallPanelLine.EndPoint)
            {
                return moveVectorOfBeamLine;
            }

            CommonModule commonMod = new CommonModule();
            var listOfStrtEndPoint = commonMod.getStartEndPointOfLine(wallPanelLine);
            Point3d strtPoint = listOfStrtEndPoint[0];
            Point3d endPoint = listOfStrtEndPoint[1];

            var newWallPanelLineId = AEE_Utility.getLineId(wallPanelLine, strtPoint, endPoint, true);


            List<Point3d> listOfStartPointInsct = new List<Point3d>();
            List<Point3d> listOfEndPointInsct = new List<Point3d>();

            getInsctPointWithBeamToWallPanel(newWallPanelLineId, nearestBeamWallLineId, distanceBtwWallToBeam, out listOfStartPointInsct, out listOfEndPointInsct);
            if (listOfStartPointInsct.Count != 0)
            {
                Point3d strtInsctPoint = listOfStartPointInsct[0];
                double wallLayout_X = strtInsctPoint.X - strtPoint.X;
                double wallLayout_Y = strtInsctPoint.Y - strtPoint.Y;
                moveVectorOfBeamLine = new Vector3d(wallLayout_X, wallLayout_Y, 0);
            }
            else
            {
                if (listOfEndPointInsct.Count != 0)
                {
                    Point3d endInsctPoint = listOfEndPointInsct[0];
                    double wallLayout_X = endInsctPoint.X - endPoint.X;
                    double wallLayout_Y = endInsctPoint.Y - endPoint.Y;
                    moveVectorOfBeamLine = new Vector3d(wallLayout_X, wallLayout_Y, 0);
                }
            }

            flagOfMove = true;
            return moveVectorOfBeamLine;
        }
    }
}
