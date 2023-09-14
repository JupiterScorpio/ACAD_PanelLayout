﻿using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using PanelLayout_App.CivilModel;
using PanelLayout_App.ElevationModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PanelLayout_App.RotationHelper;

namespace PanelLayout_App.WallModule
{
    public class KickerHelper
    {
        private List<List<LineSegment3d>> lstDoorLines = null;
        private static List<ObjectId> listOfKickerCornerId = new List<ObjectId>();
        private static List<Point3d> listOfKickerCornerBasePoint = new List<Point3d>();
        private static List<Point3d> listOfKickerCornerNrstBasePoint = new List<Point3d>();
        private static List<ObjectId> listOfKickerCornerLineId = new List<ObjectId>();
        private static List<double> listOfKickerRotationAngle = new List<double>();
        private static List<ObjectId> listOfKickerCornerId_ForText = new List<ObjectId>();
        private static List<string> listOfKickerCornerFlanges_ForText = new List<string>();
        public static List<ObjectId> listOfKickerCornerId_In_ConcaveAngle = new List<ObjectId>();

        public static List<ObjectId> listOfKickerWallLineId = new List<ObjectId>();
        public static List<ObjectId> listOfKickerOffsetWallLineId = new List<ObjectId>();
        public static List<double> listOfKickerHeight = new List<double>();

        Vector3d moveVector = CreateShellPlanHelper.moveVector_ForSunkanSlabLayout;
        internal KickerHelper(List<List<LineSegment3d>> lstDoorLines = null)
        {
            this.lstDoorLines = lstDoorLines;
        }

        public void callMethodOfInternalWallKickerCorner(int index, ObjectId prvsWallLineId, ObjectId lastWallLineId, ObjectId wallId, List<ObjectId> listOfWallLineId, ObjectId insideOffsetWallId, List<ObjectId> listOfInsideOffsetWallLineId, ObjectId outsideOffsetWallId, List<ObjectId> listOfOutsideOffsetWallLineId, ObjectId sunkanSlabId)
        {
            var wallEnt = AEE_Utility.GetEntityForRead(wallId);

            if (wallEnt.Layer == CommonModule.ductLayerName || Lift_UI_Helper.checkLiftLayerIsExist(wallEnt.Layer) || sunkanSlabId.IsValid == true)
            {
                if (index == 0)
                {
                    listOfKickerCornerId.Clear();
                    listOfKickerCornerBasePoint.Clear();
                    listOfKickerCornerNrstBasePoint.Clear();
                    listOfKickerCornerLineId.Clear();
                    listOfKickerRotationAngle.Clear();
                    listOfKickerCornerId_In_ConcaveAngle.Clear();
                    listOfKickerCornerId_ForText.Clear();
                    listOfKickerCornerFlanges_ForText.Clear();
                }
                bool flagOfGreater = false;
                if (sunkanSlabId.IsValid == true)
                {
                    var flagOfExternalSunkanSlabLayer = ExternalSunkanSlab_UI_Helper.checkExternalSunkanSlabLayerIsExist(sunkanSlabId);

                    flagOfGreater = SunkanSlabHelper.checkSunkanSlabLevelDiffrnce_Is_Greater(sunkanSlabId);
                    if (flagOfGreater == false)
                    {
                        return;
                    }
                }

                bool flagOfSlabJoint = false;

                double panelDepth = CommonModule.panelDepth;
                double kickerHeight = SunkanSlabHelper.getSunkanSlabLevelDifference(sunkanSlabId);

                if (sunkanSlabId.IsValid == true)
                {
                    kickerHeight = SunkanSlabHelper.getSunkanSlabLevelDifference(sunkanSlabId);

                    if (flagOfGreater == true)
                    {
                        kickerHeight = kickerHeight + PanelLayout_UI.RC;
                    }
                }
                else
                {
                    kickerHeight = PanelLayout_UI.kickerHt;
                }

                drawKickerCorner_In_InternalWall(index, prvsWallLineId, lastWallLineId, wallId, wallEnt, listOfWallLineId, insideOffsetWallId, listOfInsideOffsetWallLineId, outsideOffsetWallId, listOfOutsideOffsetWallLineId, kickerHeight, panelDepth, flagOfSlabJoint);

                if (index == (listOfWallLineId.Count - 1))
                {
                    createTextInKickerCorner(kickerHeight, listOfKickerCornerId, listOfKickerCornerLineId, listOfWallLineId, listOfKickerRotationAngle, listOfKickerCornerFlanges_ForText, moveVector, flagOfSlabJoint);
                }
            }
        }


        private void drawKickerCorner_In_InternalWall(int index, ObjectId prvsWallLineId, ObjectId lastWallLineId, ObjectId wallId, Entity wallEnt, List<ObjectId> listOfWallLineId, ObjectId insideOffsetWallId, List<ObjectId> listOfInsideOffsetWallLineId, ObjectId outsideOffsetWallId, List<ObjectId> listOfOutsideOffsetWallLineId, double kickerHeight, double panelDepth, bool flagOfSlabJoint)
        {
            ObjectId crrntLineId = listOfWallLineId[index];

            ObjectId prvsLineId = new ObjectId();
            ObjectId nextLineId = new ObjectId();
            if (index == 0)
            {
                prvsLineId = listOfWallLineId[(listOfWallLineId.Count - 1)];
            }
            else
            {
                prvsLineId = listOfWallLineId[(index - 1)];
            }

            if (index == (listOfWallLineId.Count - 1))
            {
                nextLineId = listOfWallLineId[0];
            }
            else
            {
                nextLineId = listOfWallLineId[index + 1];
            }

            double crrntLineAngle = AEE_Utility.GetAngleOfLine(crrntLineId);
            double crrntLineLngth = AEE_Utility.GetLengthOfLine(crrntLineId);
            var listOfCrrntStrtEndPoint = AEE_Utility.GetStartEndPointOfLine(crrntLineId);
            Point3d crrntStrtPoint = listOfCrrntStrtEndPoint[0];
            Point3d crrntEndPoint = listOfCrrntStrtEndPoint[1];

            double prvsLineAngle = AEE_Utility.GetAngleOfLine(prvsLineId);
            double prvsLineLngth = AEE_Utility.GetLengthOfLine(prvsLineId);
            var listOfPrvsStrtEndPoint = AEE_Utility.GetStartEndPointOfLine(prvsLineId);
            Point3d prvsStrtPoint = listOfPrvsStrtEndPoint[0];
            Point3d prvsEndPoint = listOfPrvsStrtEndPoint[1];

            double nextLineAngle = AEE_Utility.GetAngleOfLine(nextLineId);
            double nextLineLngth = AEE_Utility.GetLengthOfLine(nextLineId);
            var listOfNextStrtEndPoint = AEE_Utility.GetStartEndPointOfLine(nextLineId);
            Point3d nextStrtPoint = listOfNextStrtEndPoint[0];
            Point3d nextEndPoint = listOfNextStrtEndPoint[1];

            double kickerCornerFlange = CommonModule.minKickerCornrFlange;
            //double kickerCornr_Flange1 = kickerCornerFlange;
            //double kickerCornr_Flange2 = kickerCornerFlange;

            Point3d basePoint = crrntStrtPoint;

            ObjectId kickerCornerId = new ObjectId();

            var xDataRegAppName = AEE_Utility.GetXDataRegisterAppName(prvsWallLineId);

            PartHelper partHlp = new PartHelper();
            kickerCornerId = partHlp.drawExternalCorner(xDataRegAppName, basePoint.X, basePoint.Y, kickerCornerFlange, panelDepth, CommonModule.kickerCornerLayerName, CommonModule.kickerCornerLayerColor);
            AEE_Utility.AttachXData(kickerCornerId, xDataRegAppName, CommonModule.xDataAsciiName);
            AEE_Utility.AttachXData(kickerCornerId, xDataRegAppName + "_1", kickerCornerFlange.ToString() + "," +
                                                                            panelDepth.ToString());

            Point3d nrstOfBasePointCorner = getNearestPointOfKickerCorner(kickerCornerId, basePoint);
            double rotationAngle = 0;
            kickerCornerId = checkCornerIsInsideTheWall(index, kickerCornerId, crrntLineId, basePoint, crrntLineAngle, wallId, insideOffsetWallId, listOfInsideOffsetWallLineId, outsideOffsetWallId, listOfOutsideOffsetWallLineId, nrstOfBasePointCorner, out rotationAngle);

            basePoint = AEE_Utility.GetBasePointOfPolyline(kickerCornerId);
            nrstOfBasePointCorner = getNearestPointOfKickerCorner(kickerCornerId, basePoint);
            stretchKickerCorner(xDataRegAppName, listOfKickerCornerId, listOfKickerCornerBasePoint, listOfKickerCornerNrstBasePoint, kickerCornerId, basePoint, nrstOfBasePointCorner, prvsLineAngle, prvsLineLngth, kickerCornerFlange, wallEnt, kickerHeight, panelDepth, flagOfSlabJoint, outsideOffsetWallId);

            AEE_Utility.AttachXData(kickerCornerId, xDataRegAppName, CommonModule.xDataAsciiName);

            listOfKickerCornerBasePoint.Add(basePoint);
            listOfKickerCornerNrstBasePoint.Add(nrstOfBasePointCorner);
            listOfKickerCornerId.Add(kickerCornerId);
            listOfKickerCornerLineId.Add(crrntLineId);
            listOfKickerRotationAngle.Add(rotationAngle);

            if (index == (listOfWallLineId.Count - 1))
            {
                var last_XDataRegAppName = AEE_Utility.GetXDataRegisterAppName(lastWallLineId);

                ObjectId firstLineId = listOfWallLineId[0];
                double firstLineAngle = AEE_Utility.GetAngleOfLine(firstLineId);
                double firstLineLngth = AEE_Utility.GetLengthOfLine(firstLineId);

                ObjectId firstKickerCornerId = listOfKickerCornerId[0];
                Point3d firstBasePoint = listOfKickerCornerBasePoint[0];
                Point3d firstNrstOfBasePointCorner = listOfKickerCornerNrstBasePoint[0];
                stretchKickerCorner(last_XDataRegAppName, listOfKickerCornerId, listOfKickerCornerBasePoint, listOfKickerCornerNrstBasePoint, firstKickerCornerId, firstBasePoint, firstNrstOfBasePointCorner, crrntLineAngle, crrntLineLngth, kickerCornerFlange, wallEnt, kickerHeight, panelDepth, flagOfSlabJoint, outsideOffsetWallId);
            }
        }

        public double roundUpNumber(double number, double roundUpValue)
        {
            double roundUpNumber = 0;
            if ((number % roundUpValue) == 0)
            {
                roundUpNumber = number;
            }
            else
            {
                double division = number / roundUpValue;
                double truncate_DivisionValue = Math.Truncate(division);

                roundUpNumber = truncate_DivisionValue * roundUpValue;
            }

            return roundUpNumber;
        }
        private ObjectId checkCornerIsInsideTheWall(int index, ObjectId kickerCornerId, ObjectId crrntLineId, Point3d basePoint, double crrntLineAngle, ObjectId wallId, ObjectId insideOffsetWallId, List<ObjectId> listOfInsideOffsetWallLineId, ObjectId outsideOffsetWallId, List<ObjectId> listOfOutsideOffsetWallLineId, Point3d nrstOfBasePointCorner, out double rotationAngle)
        {
            rotationAngle = 0;

            var colon_KickerCornerId = AEE_Utility.createColonEntityInSamePoint(kickerCornerId, true);

            PartHelper partHelper = new PartHelper();
            CommonModule common_Obj = new CommonModule();

            ObjectId prvsInsideOffsetLineId = new ObjectId();
            ObjectId nextInsideOffsetLineId = new ObjectId();

            ObjectId prvsOutsideOffsetLineId = new ObjectId();
            ObjectId nextOutsideOffsetLineId = new ObjectId();

            ObjectId crrntInsideOffsetLineId = listOfInsideOffsetWallLineId[index];
            ObjectId crrntOutsideOffsetLineId = listOfOutsideOffsetWallLineId[index];

            int counter = 0;
            if (listOfOutsideOffsetWallLineId.Count() < listOfInsideOffsetWallLineId.Count())
                counter = listOfOutsideOffsetWallLineId.Count();
            else
            {
                counter = listOfInsideOffsetWallLineId.Count();
            }

            if (index == 0)
            {
                prvsInsideOffsetLineId = listOfInsideOffsetWallLineId[(listOfInsideOffsetWallLineId.Count - 1)];

                prvsOutsideOffsetLineId = listOfOutsideOffsetWallLineId[(listOfOutsideOffsetWallLineId.Count - 1)];
            }
            else
            {
                prvsInsideOffsetLineId = listOfInsideOffsetWallLineId[(index - 1)];

                prvsOutsideOffsetLineId = listOfOutsideOffsetWallLineId[(listOfOutsideOffsetWallLineId.Count - 1)];
            }

            if (index == (counter - 1))
            {
                nextInsideOffsetLineId = listOfInsideOffsetWallLineId[0];

                nextOutsideOffsetLineId = listOfOutsideOffsetWallLineId[0];
            }
            else
            {
                if (index < (counter - 1))
                {
                    nextInsideOffsetLineId = listOfInsideOffsetWallLineId[index + 1];
                    nextOutsideOffsetLineId = listOfOutsideOffsetWallLineId[index + 1];
                }

            }

            bool flagOfCornerInside = cornerRotate(kickerCornerId, outsideOffsetWallId, basePoint, crrntInsideOffsetLineId, prvsInsideOffsetLineId, false, crrntLineAngle, out rotationAngle);
            if (flagOfCornerInside == true)
            {
                AEE_Utility.deleteEntity(colon_KickerCornerId);
            }
            else
            {
                AEE_Utility.deleteEntity(kickerCornerId);
                drawCorner_In_ConcaveAngle(basePoint, nrstOfBasePointCorner, colon_KickerCornerId, outsideOffsetWallId, crrntOutsideOffsetLineId, prvsOutsideOffsetLineId, crrntLineAngle, out rotationAngle);
                kickerCornerId = colon_KickerCornerId;
                listOfKickerCornerId_In_ConcaveAngle.Add(kickerCornerId);
            }

            return kickerCornerId;
        }


        public bool cornerRotate(ObjectId kickerCornerId, ObjectId outsideOffsetWallId, Point3d basePoint, ObjectId crrntInsideOffsetLineId, ObjectId prvsInsideOffsetLineId, bool flagOfConcave, double crrntLineAngle, out double rotationAngle)
        {
            rotationAngle = 0;

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
                listOfObjId.Add(kickerCornerId);
                MoveRorationScaleJig.RotationScaleJig_Method(listOfObjId);

                var listOfCrrntLineInsct = AEE_Utility.InterSectionBetweenTwoEntity(crrntInsideOffsetLineId, kickerCornerId);
                var listOfPrvsLineInsct = AEE_Utility.InterSectionBetweenTwoEntity(prvsInsideOffsetLineId, kickerCornerId);

                rotationAngle = angle + crrntLineAngle;

                if (flagOfConcave == true && listOfCrrntLineInsct.Count == 0 && listOfPrvsLineInsct.Count == 0)
                {
                    flagOfCornerInside = true;
                    break;
                }

                if (flagOfConcave == false && listOfCrrntLineInsct.Count != 0 && listOfPrvsLineInsct.Count != 0)
                {
                    Point3d insctPointOfCrrntLine = listOfCrrntLineInsct[0];
                    Point3d insctPointOfPrvsLine = listOfPrvsLineInsct[0];

                    var lengthOfCrrtLineToBasePoint = AEE_Utility.GetLengthOfLine(basePoint, insctPointOfCrrntLine);
                    lengthOfCrrtLineToBasePoint = Math.Truncate(lengthOfCrrtLineToBasePoint);

                    var lengthOfPrvsLineToBasePoint = AEE_Utility.GetLengthOfLine(basePoint, insctPointOfPrvsLine);
                    lengthOfPrvsLineToBasePoint = Math.Truncate(lengthOfPrvsLineToBasePoint);
                    if (lengthOfCrrtLineToBasePoint <= 10 || lengthOfPrvsLineToBasePoint <= 10)
                    {
                        break;
                    }
                    flagOfCornerInside = true;
                    break;
                }
            }
            return flagOfCornerInside;
        }


        public Point3d getNearestPointOfKickerCorner(ObjectId kickerCornerId, Point3d basePoint)
        {
            Point3d nrstBasePointOfCorner = new Point3d();
            List<double> listOfAllLength = new List<double>();
            List<Point2d> listOfAllPoint = new List<Point2d>();

            var listOfCornerVertice = AEE_Utility.GetPolylineVertexPoint(kickerCornerId);

            for (int i = 0; i < listOfCornerVertice.Count; i++)
            {
                Point2d vertice = listOfCornerVertice[i];
                if (basePoint.X == vertice.X && basePoint.Y == vertice.Y)
                {
                }
                else
                {
                    var length = AEE_Utility.GetLengthOfLine(basePoint.X, basePoint.Y, 0, vertice.X, vertice.Y, 0);
                    listOfAllLength.Add(length);
                    listOfAllPoint.Add(vertice);
                }
            }
            if (listOfAllLength.Count != 0)
            {
                double minLength = listOfAllLength.Min();
                int indexOfMinLngth = listOfAllLength.IndexOf(minLength);
                Point2d minPoint = listOfAllPoint[indexOfMinLngth];
                nrstBasePointOfCorner = new Point3d(minPoint.X, minPoint.Y, 0);
            }
            return nrstBasePointOfCorner;
        }


        public void drawCorner_In_ConcaveAngle(Point3d basePoint, Point3d minPoint, ObjectId kickerCornerId, ObjectId outsideOffsetWallId, ObjectId crrntInsideOffsetLineId, ObjectId prvsInsideOffsetLineId, double currentLineAngle, out double rotationAngle)
        {
            rotationAngle = 0;
            Vector3d moveVector = new Vector3d((basePoint.X - minPoint.X), (basePoint.Y - minPoint.Y), 0);
            AEE_Utility.MoveEntity(kickerCornerId, moveVector);
            bool flagOfCornerInside = cornerRotate(kickerCornerId, outsideOffsetWallId, basePoint, crrntInsideOffsetLineId, prvsInsideOffsetLineId, true, currentLineAngle, out rotationAngle);
            if (flagOfCornerInside == false)
            {
                moveVector = new Vector3d((-moveVector.X), (-moveVector.Y), 0);
                AEE_Utility.MoveEntity(kickerCornerId, moveVector);
            }
        }


        public List<double> getCornerFlange(double crrntLineLngth, double panelDepth, out bool flagOfWallPanelCreate)
        {
            flagOfWallPanelCreate = false;

            crrntLineLngth = Math.Round(crrntLineLngth);
            List<double> listOfKickerCornerFlange = new List<double>();

            double kickerCornerFlange = 0;

            double roundUpValue = 5;

            double cornerLngth = (crrntLineLngth / 2);

            double cornerLength_RoundUp = roundUpNumber(cornerLngth, roundUpValue);

            if (cornerLength_RoundUp >= CommonModule.minKickerCornrFlange && cornerLength_RoundUp <= CommonModule.maxKickerCornrFlange)
            {
                if (cornerLength_RoundUp == CommonModule.maxKickerCornrFlange)
                {
                    kickerCornerFlange = (CommonModule.maxKickerCornrFlange) / 2;
                    listOfKickerCornerFlange.Add(kickerCornerFlange);
                    listOfKickerCornerFlange.Add(kickerCornerFlange);
                    flagOfWallPanelCreate = true;//Changes made on 01/06/2023 by SDM
                }
                else
                {
                    double diff = cornerLngth - (CommonModule.minKickerCornrFlange);
                    if (diff >= panelDepth)
                    {
                        listOfKickerCornerFlange.Add(cornerLength_RoundUp);
                        listOfKickerCornerFlange.Add((crrntLineLngth - cornerLength_RoundUp));
                    }
                    else
                    {
                        listOfKickerCornerFlange.Add(CommonModule.minKickerCornrFlange);
                        listOfKickerCornerFlange.Add(crrntLineLngth - CommonModule.minKickerCornrFlange);
                    }
                }
            }

            if (cornerLength_RoundUp > CommonModule.maxKickerCornrFlange)
            {
                flagOfWallPanelCreate = true;
                double diff = Math.Abs(crrntLineLngth - (2 * CommonModule.maxKickerCornrFlange));
                if (diff > PanelLayout_UI.minWidthOfPanel)
                {
                    kickerCornerFlange = CommonModule.maxKickerCornrFlange;
                    listOfKickerCornerFlange.Add(kickerCornerFlange);
                    listOfKickerCornerFlange.Add(kickerCornerFlange);
                }
                else
                {
                    kickerCornerFlange = cornerLength_RoundUp - (PanelLayout_UI.minWidthOfPanel / 2);
                    listOfKickerCornerFlange.Add(kickerCornerFlange);
                    listOfKickerCornerFlange.Add(kickerCornerFlange);
                }
            }

            if (listOfKickerCornerFlange.Count == 0)
            {
                listOfKickerCornerFlange.Add(cornerLngth);
                listOfKickerCornerFlange.Add(cornerLngth);
            }

            return listOfKickerCornerFlange;
        }

        private LineSegment3d getCurLine(ObjectId KickerCornerId, Point3d basePoint, Point3d prvsBasePoint, double panelDepth, bool forExternal = false)
        {
            LineSegment3d lineSegment3d = null;
            List<Point3d> listOfStartpts = new List<Point3d>();

            ObjectId tempLine2d = AEE_Utility.getLineId(basePoint, prvsBasePoint, false);

            if (forExternal)
            {
                ObjectId offsetObjID = AEE_Utility.OffsetEntity(panelDepth, tempLine2d, false);

                List<Point3d> listOfInsctPts = AEE_Utility.InterSectionBetweenTwoEntity(offsetObjID, KickerCornerId);

                if (listOfInsctPts.Count == 0)
                {
                    AEE_Utility.deleteEntity(offsetObjID);
                    offsetObjID = AEE_Utility.OffsetEntity(-panelDepth, tempLine2d, false);
                }

                listOfStartpts = AEE_Utility.GetStartEndPointOfLine(offsetObjID);
                AEE_Utility.deleteEntity(offsetObjID);
            }
            else
            {
                listOfStartpts = AEE_Utility.GetStartEndPointOfLine(tempLine2d);
            }

            AEE_Utility.deleteEntity(tempLine2d);
            lineSegment3d = new LineSegment3d(listOfStartpts[0], listOfStartpts[1]);


            return lineSegment3d;
        }
        private void stretchKickerCorner(string xDataRegAppName, List<ObjectId> listOfAllKickerCornerId, List<Point3d> listOfAllKickerCornerBasePoint, List<Point3d> listOfAllKickerCornerNrstBasePoint, ObjectId crrntKickerCornerId, Point3d basePoint, Point3d nrstOfBasePointCorner, double stretchLineAngle, double stretchLineLength, double kickerCornerFlange, Entity wallEnt, double kickerHeight, double panelDepth, bool flagOfSlabJoint, ObjectId outsideOffsetWallId, bool forExternal = false)
        {
            if (listOfAllKickerCornerId.Count != 0)
            {
                int prvsDataIndex = (listOfAllKickerCornerId.Count - 1);
                ObjectId prvsKickerCornerId = listOfAllKickerCornerId[prvsDataIndex];
                Point3d prvsBasePoint = listOfAllKickerCornerBasePoint[prvsDataIndex];
                Point3d prvsNrstOfBasePointCorner = listOfAllKickerCornerNrstBasePoint[prvsDataIndex];

                var prvsCornerLayerName = AEE_Utility.GetLayerName(prvsKickerCornerId);
                var crrntCornerLayerName = AEE_Utility.GetLayerName(crrntKickerCornerId);
                if (crrntCornerLayerName == CommonModule.externalCornerLyrName || prvsCornerLayerName == CommonModule.externalCornerLyrName)
                {
                    if (crrntCornerLayerName == CommonModule.externalCornerLyrName)
                    {
                        setCornerText(crrntKickerCornerId, CommonModule.externalCorner);
                    }
                    else
                    {
                        setCornerText(crrntKickerCornerId, kickerCornerFlange);
                    }

                    if (prvsCornerLayerName == CommonModule.externalCornerLyrName)
                    {
                        setCornerText(prvsKickerCornerId, CommonModule.externalCorner);
                    }
                    else
                    {
                        setCornerText(prvsKickerCornerId, kickerCornerFlange);
                    }
                    setKickerWallPanelLine(wallEnt, xDataRegAppName, crrntKickerCornerId, basePoint, nrstOfBasePointCorner, prvsKickerCornerId, prvsBasePoint, prvsNrstOfBasePointCorner, stretchLineAngle, panelDepth, kickerHeight, flagOfSlabJoint, outsideOffsetWallId);
                }
                else
                {
                    if (listOfKickerCornerId_In_ConcaveAngle.Contains(crrntKickerCornerId))
                    {
                        //corner angle is concave, add the panel depth value in line length.
                        stretchLineLength = stretchLineLength + panelDepth;
                    }

                    if (listOfKickerCornerId_In_ConcaveAngle.Contains(prvsKickerCornerId))
                    {
                        //corner angle is concave, add the panel depth value in line length.
                        stretchLineLength = stretchLineLength + panelDepth;
                    }

                    bool flagOfWallPanelCreate = false;
                    List<double> listOfKickerCornerFlange = getCornerFlange(stretchLineLength, panelDepth, out flagOfWallPanelCreate);
                    double stretchValue = listOfKickerCornerFlange[0] - kickerCornerFlange;



                    if (AEE_Utility.CustType == eCustType.WNPanel && listOfKickerCornerId_In_ConcaveAngle.Contains(crrntKickerCornerId))
                    {
                        stretchValue = (listOfKickerCornerFlange[0] - kickerCornerFlange) + panelDepth;//Changes made on 29/06/2023 by PRT
                    }



                    //LineSegment3d curLine = new LineSegment3d(basePoint, prvsBasePoint);
                    LineSegment3d curLine = getCurLine(crrntKickerCornerId, basePoint, prvsBasePoint, panelDepth, forExternal);
                    LineSegment3d doorLine = null;


                    List<int> listOfCrrntVerticePointIndex_ForStretch = new List<int>();
                    List<List<Point3d>> listOfListKickerCornerLinePoint = getStretchLinePoint(stretchLineAngle, crrntKickerCornerId, basePoint, nrstOfBasePointCorner, panelDepth, out listOfCrrntVerticePointIndex_ForStretch);
                    if (lstDoorLines != null && lstDoorLines.Any())
                    {
                        doorLine = GetOverlapDoorLine(this.lstDoorLines, curLine);
                        if (doorLine != null)
                        {
                            List<double> distList = new List<double>() { doorLine.StartPoint.DistanceTo(curLine.StartPoint), doorLine.EndPoint.DistanceTo(curLine.StartPoint) };

                            double avaiLength = Math.Round(distList.Min());
                            if (avaiLength > 1)
                            {
                                if (avaiLength > CommonModule.extendKickerCornrFlangeUpto)
                                {
                                    avaiLength = listOfKickerCornerFlange[0];
                                }
                                stretchValue = avaiLength - kickerCornerFlange;
                                if (AEE_Utility.CustType == eCustType.WNPanel && listOfKickerCornerId_In_ConcaveAngle.Contains(crrntKickerCornerId))
                                {
                                    stretchValue = (avaiLength - kickerCornerFlange)+panelDepth;
                                }
                            }
                        }
                    }

                    stretchValue = stretch(crrntKickerCornerId, listOfCrrntVerticePointIndex_ForStretch, listOfListKickerCornerLinePoint, stretchValue, kickerCornerFlange, doorLine, curLine);
                    double kickerCrnrFlang = (kickerCornerFlange + stretchValue);
                    //kickerCrnrFlang = (kickerCornerFlange + stretchValue);
                    if (AEE_Utility.CustType == eCustType.WNPanel && listOfKickerCornerId_In_ConcaveAngle.Contains(crrntKickerCornerId))
                    {
                        kickerCrnrFlang = (kickerCornerFlange + stretchValue) - panelDepth;//Changes made on 29/06/2023 by PRT
                    }
                    setCornerText(crrntKickerCornerId, kickerCrnrFlang);
                    stretchValue = listOfKickerCornerFlange[1] - kickerCornerFlange;


                    if (AEE_Utility.CustType == eCustType.WNPanel && listOfKickerCornerId_In_ConcaveAngle.Contains(prvsKickerCornerId))
                    {
                        stretchValue = (listOfKickerCornerFlange[0] - kickerCornerFlange) + panelDepth;//Changes made on 29/06/2023 by PRT
                    }


                    List<int> listOfPrvsVerticePointIndex_ForStretch = new List<int>();
                    List<List<Point3d>> listOfListPrvsKickerCornerLinePoint = getStretchLinePoint(stretchLineAngle, prvsKickerCornerId, prvsBasePoint, prvsNrstOfBasePointCorner, panelDepth, out listOfPrvsVerticePointIndex_ForStretch);
                    doorLine = null;
                    //curLine = new LineSegment3d(prvsBasePoint, basePoint);

                    curLine = getCurLine(prvsKickerCornerId, prvsBasePoint, basePoint, panelDepth, forExternal);

                    if (lstDoorLines != null && lstDoorLines.Any())
                    {
                        doorLine = GetOverlapDoorLine(this.lstDoorLines, curLine);
                        if (doorLine != null)
                        {
                            List<double> distList = new List<double>() { doorLine.StartPoint.DistanceTo(curLine.StartPoint), doorLine.EndPoint.DistanceTo(curLine.StartPoint) };

                            double avaiLength = Math.Round(distList.Min());
                            if (avaiLength > 1)
                            {
                                if (avaiLength > CommonModule.extendKickerCornrFlangeUpto)
                                {
                                    avaiLength = listOfKickerCornerFlange[1];
                                }
                                stretchValue = avaiLength - kickerCornerFlange;
                                if ( AEE_Utility.CustType == eCustType.WNPanel && listOfKickerCornerId_In_ConcaveAngle.Contains(prvsKickerCornerId))
                                {
                                    stretchValue = (avaiLength - kickerCornerFlange) + panelDepth;
                                }
                            }
                        }
                    }
                    stretchValue = stretch(prvsKickerCornerId, listOfPrvsVerticePointIndex_ForStretch, listOfListPrvsKickerCornerLinePoint, stretchValue, kickerCornerFlange, doorLine, curLine);
                    kickerCrnrFlang = (kickerCornerFlange + stretchValue);

                    if (AEE_Utility.CustType == eCustType.WNPanel && listOfKickerCornerId_In_ConcaveAngle.Contains(prvsKickerCornerId))
                    {
                        kickerCrnrFlang = (kickerCornerFlange + stretchValue) - panelDepth;//Changes made on 29/06/2023 by PRT
                    }


                    setCornerText(prvsKickerCornerId, kickerCrnrFlang);

                    setKickerWallLine(xDataRegAppName, crrntKickerCornerId, listOfCrrntVerticePointIndex_ForStretch, prvsKickerCornerId, listOfPrvsVerticePointIndex_ForStretch, flagOfWallPanelCreate, wallEnt, kickerHeight, flagOfSlabJoint);
                }
            }
        }

        private void setKickerWallPanelLine(Entity wallEnt, string xDataRegAppName, ObjectId crrntKickerCornerId, Point3d crrntKickerCrnrBasePoint, Point3d crrntKickerCrnrNearstOfBasePoint, ObjectId prvsKickerCornerId, Point3d prvsKickerCrnrBasePoint, Point3d prvsKickerCrnrNearstOfBasePoint, double stretchLineAngle, double panelDepth, double kickerHeight, bool flagOfSlabJoint, ObjectId outsideOffsetWallId)
        {
            var prvsCornerLayerName = AEE_Utility.GetLayerName(prvsKickerCornerId);
            var crrntCornerLayerName = AEE_Utility.GetLayerName(crrntKickerCornerId);

            ObjectId kickerWallLineId = new ObjectId();
            ObjectId kickerOffsetWallLineId = new ObjectId();

            //Changes made on 01/06/2023 by SDM
            Point3d crrn_pt = crrntKickerCrnrBasePoint;
            Point3d prv_pt = prvsKickerCrnrBasePoint;

            var ent_SameProperty = AEE_Utility.GetEntityForWrite(outsideOffsetWallId);
            var lineId = AEE_Utility.getLineId(ent_SameProperty, crrn_pt.X, crrn_pt.Y, 0, prv_pt.X, prv_pt.Y, 0, false);
            if (crrntCornerLayerName == CommonModule.externalCornerLyrName)
            {

                var listOfInsctPoint1 = AEE_Utility.InterSectionBetweenTwoEntity(crrntKickerCornerId, lineId);
                foreach (var pnt in listOfInsctPoint1)
                {
                    if (pnt != crrn_pt)
                    {
                        crrntKickerCrnrBasePoint = pnt;
                        break;
                    }
                }
            }
            if (prvsCornerLayerName == CommonModule.externalCornerLyrName)
            {

                var listOfInsctPoint2 = AEE_Utility.InterSectionBetweenTwoEntity(prvsKickerCornerId, lineId);
                foreach (var pnt in listOfInsctPoint2)
                {
                    if (pnt != prv_pt)
                    {
                        prvsKickerCrnrBasePoint = pnt;
                        break;
                    }
                }
            }
            AEE_Utility.deleteEntity(lineId);
            //-------------------------------------
            if (crrntCornerLayerName == CommonModule.externalCornerLyrName && prvsCornerLayerName == CommonModule.externalCornerLyrName)
            {
                double len = AEE_Utility.GetLengthOfLine(crrntKickerCrnrBasePoint.X, crrntKickerCrnrBasePoint.Y, 0, prvsKickerCrnrBasePoint.X, prvsKickerCrnrBasePoint.Y, 0);
                //Changes made on 01/06/2023 by SDM
                if (Math.Round(len) <= 1)
                    return;

                kickerWallLineId = AEE_Utility.getLineId(wallEnt, crrntKickerCrnrBasePoint, prvsKickerCrnrBasePoint, false);
            }
            else
            {
                List<int> listOfVerticePointIndex_ForStretch = new List<int>();
                List<List<Point3d>> listOfListKickerCornerLinePoint = new List<List<Point3d>>();
                List<Point2d> listOfKickerVertice = new List<Point2d>();
                Point3d basePointOfCorner = new Point3d();
                if (crrntCornerLayerName != CommonModule.externalCornerLyrName)
                {
                    listOfListKickerCornerLinePoint = getStretchLinePoint(stretchLineAngle, crrntKickerCornerId, crrntKickerCrnrBasePoint, crrntKickerCrnrNearstOfBasePoint, panelDepth, out listOfVerticePointIndex_ForStretch);
                    listOfKickerVertice = AEE_Utility.GetPolylineVertexPoint(crrntKickerCornerId);
                    basePointOfCorner = prvsKickerCrnrBasePoint;
                }
                else if (prvsCornerLayerName != CommonModule.externalCornerLyrName)
                {
                    listOfListKickerCornerLinePoint = getStretchLinePoint(stretchLineAngle, prvsKickerCornerId, prvsKickerCrnrBasePoint, prvsKickerCrnrNearstOfBasePoint, panelDepth, out listOfVerticePointIndex_ForStretch);
                    listOfKickerVertice = AEE_Utility.GetPolylineVertexPoint(prvsKickerCornerId);
                    basePointOfCorner = crrntKickerCrnrBasePoint;
                }

                Point2d kickerPoint1 = listOfKickerVertice[listOfVerticePointIndex_ForStretch[0]];
                Point2d kickerPoint2 = listOfKickerVertice[listOfVerticePointIndex_ForStretch[1]];

                double length1 = AEE_Utility.GetLengthOfLine(kickerPoint1.X, kickerPoint1.Y, 0, basePointOfCorner.X, basePointOfCorner.Y, 0);
                double length2 = AEE_Utility.GetLengthOfLine(kickerPoint2.X, kickerPoint2.Y, 0, basePointOfCorner.X, basePointOfCorner.Y, 0);
                if (length1 > length2)
                {
                    //Changes made on 01/06/2023 by SDM
                    if (Math.Round(length2) <= 1)
                        return;

                    kickerWallLineId = AEE_Utility.getLineId(wallEnt, kickerPoint2.X, kickerPoint2.Y, 0, basePointOfCorner.X, basePointOfCorner.Y, 0, false);
                }
                else
                {
                    //Changes made on 01/06/2023 by SDM
                    if (Math.Round(length1) <= 1)
                        return;

                    kickerWallLineId = AEE_Utility.getLineId(wallEnt, kickerPoint1.X, kickerPoint1.Y, 0, basePointOfCorner.X, basePointOfCorner.Y, 0, false);
                }
            }

            AEE_Utility.AttachXData(kickerWallLineId, xDataRegAppName, CommonModule.xDataAsciiName);

            var angleOfLine = AEE_Utility.GetAngleOfLine(kickerWallLineId);
            var point = AEE_Utility.get_XY(angleOfLine, 50);
            var listOfKickerLineStrtEndPoint = AEE_Utility.GetStartEndPointOfLine(kickerWallLineId);

            Point3d strtPointKickerLine = listOfKickerLineStrtEndPoint[0];
            Point3d endPointKickerLine = listOfKickerLineStrtEndPoint[1];
            strtPointKickerLine = new Point3d((strtPointKickerLine.X - point.X), (strtPointKickerLine.Y - point.Y), 0);
            endPointKickerLine = new Point3d((endPointKickerLine.X + point.X), (endPointKickerLine.Y + point.Y), 0);

            var checkLineId = AEE_Utility.getLineId(strtPointKickerLine, endPointKickerLine, false);
            var offsetLineId = AEE_Utility.OffsetLine(checkLineId, WindowHelper.windowOffsetValue, false);
            var listOfCrrntCornrInsct = AEE_Utility.InterSectionBetweenTwoEntity(offsetLineId, crrntKickerCornerId);
            var listOfCrrntPrvsInsct = AEE_Utility.InterSectionBetweenTwoEntity(offsetLineId, prvsKickerCornerId);
            AEE_Utility.deleteEntity(checkLineId);
            AEE_Utility.deleteEntity(offsetLineId);
            if (listOfCrrntCornrInsct.Count != 0 || listOfCrrntPrvsInsct.Count != 0)
            {
                kickerOffsetWallLineId = AEE_Utility.OffsetLine(kickerWallLineId, panelDepth, false);
            }
            else
            {
                kickerOffsetWallLineId = AEE_Utility.OffsetLine(kickerWallLineId, -panelDepth, false);
            }

            KickerHelper.listOfKickerWallLineId.Add(kickerWallLineId);
            KickerHelper.listOfKickerOffsetWallLineId.Add(kickerOffsetWallLineId);
            KickerHelper.listOfKickerHeight.Add(kickerHeight);
        }


        public double stretch(ObjectId kickerCornerId, List<int> listOfVerticePointIndex, List<List<Point3d>> listOfListStretchLinePoint, double stretchValue, double kickerCornerFlange,
                LineSegment3d doorLine = null, LineSegment3d curLine = null)
        {
            for (int index = 0; index < listOfListStretchLinePoint.Count; index++)
            {
                var listOfStretchLinePoint = listOfListStretchLinePoint[index];
                Point3d stretchPoint1 = listOfStretchLinePoint[0];
                Point3d stretchPoint2 = listOfStretchLinePoint[1];

                var angleOfStrechLine = AEE_Utility.GetAngleOfLine(stretchPoint1, stretchPoint2);

                int strectVerticeIndex = listOfVerticePointIndex[index];

                var stretchPoint = AEE_Utility.get_XY(angleOfStrechLine, stretchValue);
                var orgPt = getStretchCorner(kickerCornerId, strectVerticeIndex);
                var vec = new Vector3d(stretchPoint.X, stretchPoint.Y, 0); ;
                var pt1 = orgPt + vec;
                if (doorLine != null)
                {
                    var td = 1e-6;
                    var tol = new Tolerance(td, td);
                    if (doorLine.IsOn(pt1, tol))
                    {
                        var newPt = (new LineSegment3d(curLine.StartPoint, pt1)).IsOn(doorLine.StartPoint) ? doorLine.StartPoint : doorLine.EndPoint;
                        if ((newPt - orgPt).IsCodirectionalTo(vec, tol))
                        {
                            stretchValue -= (newPt - pt1).Length;
                        }
                    }
                }
                stretchPoint = AEE_Utility.get_XY(angleOfStrechLine, stretchValue);
                Vector3d stretchVector = new Vector3d(stretchPoint.X, stretchPoint.Y, 0);
                stretchCorner(kickerCornerId, stretchVector, strectVerticeIndex);
            }
            return stretchValue;
        }


        public void setCornerText(ObjectId kickerCornerId, double kickerCrnrFlang)
        {
            string kickerCornerId_Str = Convert.ToString(kickerCornerId);
            int indexOfExist = listOfKickerCornerId_ForText.IndexOf(kickerCornerId);

            if (indexOfExist != -1)
            {
                string prvsData = listOfKickerCornerFlanges_ForText[indexOfExist];
                string newItem = prvsData + "," + Convert.ToString(kickerCrnrFlang);
                listOfKickerCornerFlanges_ForText[listOfKickerCornerFlanges_ForText.FindIndex(ind => ind.Equals(prvsData))] = newItem;
            }
            else
            {
                listOfKickerCornerId_ForText.Add(kickerCornerId);

                string kickerData = kickerCornerId_Str + "," + Convert.ToString(kickerCrnrFlang);
                listOfKickerCornerFlanges_ForText.Add(kickerData);
            }
        }

        public Point3d getStretchCorner(ObjectId id, int stretchPointIndex)
        {
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = acDoc.Editor;
            Database db = HostApplicationServices.WorkingDatabase;

            using (Transaction acTrans = db.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;

                Entity acEnt = acTrans.GetObject(id, OpenMode.ForRead) as Entity;
                Polyline acPoly = acEnt as Polyline;

                var pt = acPoly.GetPoint3dAt(stretchPointIndex);

                acTrans.Abort();

                return pt;
            }
        }


        public void stretchCorner(ObjectId id, Vector3d stretchVec, int stretchPointIndex)
        {
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = acDoc.Editor;
            Database db = HostApplicationServices.WorkingDatabase;

            using (Transaction acTrans = db.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                Entity acEnt = acTrans.GetObject(id, OpenMode.ForWrite) as Entity;
                Polyline acPoly = acEnt as Polyline;

                IntegerCollection indces = new IntegerCollection();
                indces.Add(stretchPointIndex);
                acPoly.MoveStretchPointsAt(indces, stretchVec);
                acPoly.UpgradeOpen();

                acTrans.Commit();
            }
        }

        public List<List<Point3d>> getStretchLinePoint(double crrntLineAngle, ObjectId kickerCornerId, Point3d basePoint, Point3d nearstOfBasePointOfCorner, double panelDepth, out List<int> listOfVerticePointIndex_ForStretch)
        {
            listOfVerticePointIndex_ForStretch = new List<int>();

            List<List<Point3d>> listOfListKickerCornerLinePoint = new List<List<Point3d>>();

            bool flag_X_Axis_CrrntLine = false;
            bool flag_Y_Axis_CrrntLine = false;

            List<ObjectId> listOfCornerLineId = new List<ObjectId>();

            CommonModule commonModule_Obj = new CommonModule();
            commonModule_Obj.checkAngleOfLine_Axis(crrntLineAngle, out flag_X_Axis_CrrntLine, out flag_Y_Axis_CrrntLine);

            var listOfVerticePoint = AEE_Utility.GetPolylineVertexPoint(kickerCornerId);

            List<Point3d> listOfBaseCornerPoint = new List<Point3d>();
            List<Point3d> listOfNearestBaseCornerPoint = new List<Point3d>();
            int basePointIndex_Stretch = 0;
            int baseNearstPointIndex_Stretch = 0;

            for (int index = 0; index < listOfVerticePoint.Count; index++)
            {
                int nextPoint_Index = 0;
                Point2d nxtPnt = new Point2d();
                if (index == (listOfVerticePoint.Count - 1))
                {
                    break;
                }
                else
                {
                    nextPoint_Index = index + 1;
                    nxtPnt = listOfVerticePoint[nextPoint_Index];
                }

                Point2d crrntPnt = listOfVerticePoint[index];
                Point3d currentPoint = new Point3d(crrntPnt.X, crrntPnt.Y, 0);
                Point3d nextPoint = new Point3d(nxtPnt.X, nxtPnt.Y, 0);

                ObjectId cornerLineId = AEE_Utility.getLineId(currentPoint, nextPoint, false);

                double cornerLineAngle = AEE_Utility.GetAngleOfLine(cornerLineId);

                bool flag_X_Axis_CrnrLine = false;
                bool flag_Y_Axis_CrnrLine = false;
                commonModule_Obj.checkAngleOfLine_Axis(cornerLineAngle, out flag_X_Axis_CrnrLine, out flag_Y_Axis_CrnrLine);

                bool flagOfAngle_Is_Same = false;
                if (flag_X_Axis_CrnrLine == true && flag_X_Axis_CrrntLine == true)
                {
                    flagOfAngle_Is_Same = true;
                }
                else if (flag_Y_Axis_CrnrLine == true && flag_Y_Axis_CrrntLine == true)
                {
                    flagOfAngle_Is_Same = true;
                }

                if (flagOfAngle_Is_Same == true)
                {
                    var strtBasePointLngth = AEE_Utility.GetLengthOfLine(currentPoint, basePoint);
                    var nextBasePointLngth = AEE_Utility.GetLengthOfLine(nextPoint, basePoint);

                    if (strtBasePointLngth == 0)
                    {
                        basePointIndex_Stretch = nextPoint_Index;

                        listOfBaseCornerPoint.Add(currentPoint);
                        listOfBaseCornerPoint.Add(nextPoint);
                    }
                    else if (nextBasePointLngth == 0)
                    {
                        basePointIndex_Stretch = index;

                        listOfBaseCornerPoint.Add(nextPoint);
                        listOfBaseCornerPoint.Add(currentPoint);
                    }

                    var strtNearstBasePointLngth = AEE_Utility.GetLengthOfLine(currentPoint, nearstOfBasePointOfCorner);
                    var nextNearstBasePointLngth = AEE_Utility.GetLengthOfLine(nextPoint, nearstOfBasePointOfCorner);

                    if (strtNearstBasePointLngth == 0)
                    {
                        baseNearstPointIndex_Stretch = nextPoint_Index;

                        listOfNearestBaseCornerPoint.Add(currentPoint);
                        listOfNearestBaseCornerPoint.Add(nextPoint);
                    }
                    else if (nextNearstBasePointLngth == 0)
                    {
                        baseNearstPointIndex_Stretch = index;

                        listOfNearestBaseCornerPoint.Add(nextPoint);
                        listOfNearestBaseCornerPoint.Add(currentPoint);
                    }

                    if (listOfBaseCornerPoint.Count != 0 && listOfNearestBaseCornerPoint.Count != 0)
                    {
                        break;
                    }
                }
                AEE_Utility.deleteEntity(cornerLineId);
            }

            if (listOfBaseCornerPoint.Count != 0 && listOfNearestBaseCornerPoint.Count != 0)
            {
                listOfListKickerCornerLinePoint.Add(listOfBaseCornerPoint);
                listOfListKickerCornerLinePoint.Add(listOfNearestBaseCornerPoint);

                listOfVerticePointIndex_ForStretch.Add(basePointIndex_Stretch);
                listOfVerticePointIndex_ForStretch.Add(baseNearstPointIndex_Stretch);
            }
            return listOfListKickerCornerLinePoint;
        }


        public void createTextInKickerCorner(double kickerHeight, List<ObjectId> listOfAllKickerCornerId, List<ObjectId> listOfAllKickerCornerLineId, List<ObjectId> listOfWallLineId,
                                             List<double> listOfKickerRotationAngle, List<string> listOfKickerCornerFlanges_ForText, Vector3d moveVector, bool flagOfSlabJoint)
        {
            CornerHelper cornerHlp = new CornerHelper();
            CommonModule commonMod = new CommonModule();


            for (int i = 0; i < listOfAllKickerCornerId.Count; i++)
            {
                ObjectId cornerLineId = listOfAllKickerCornerLineId[i];
                var cornerLine = AEE_Utility.GetEntityForRead(cornerLineId);
                string layerName = cornerLine.Layer;

                double cornerLineAngle = listOfKickerRotationAngle[i];
                //cornerLineAngle = cornerLineAngle + CommonModule.cornerRotationAngle;

                string kickerCornerId_With_Flange = listOfKickerCornerFlanges_ForText[i];
                string[] array = kickerCornerId_With_Flange.Split(',');
                if (array.Length == 3)
                {
                    double flange1 = Convert.ToDouble(array.GetValue(1));
                    double flange2 = Convert.ToDouble(array.GetValue(2));

                    ObjectId cornerId = listOfKickerCornerId_ForText[i];

                    List<double> listOfFlange = cornerHlp.getCornerFlangeLength(cornerId);
                    if (listOfFlange.Count != 0)
                    {
                        flange1 = listOfFlange[1];
                        flange2 = listOfFlange[0];
                    }

                    double crnerRotationAngle = getAngleOfCornerText(cornerId);
                    cornerLineAngle = crnerRotationAngle + CommonModule.cornerRotationAngle;
                    int indexOfCornerId = listOfAllKickerCornerId.IndexOf(cornerId);
                    string cornerLayerName = "";
                    string cornerTextLayerName = "";
                    int cornerLayerColor = 0;
                    if (indexOfCornerId != -1)
                    {
                        ObjectId cornerId_ForBOM = listOfAllKickerCornerId[indexOfCornerId];

                        double cornerHeight = kickerHeight;
                        string xDataRegAppName = AEE_Utility.GetXDataRegisterAppName(cornerId_ForBOM);

                        string cornerType = "";
                        string cornerDescpt = "";
                        if (flagOfSlabJoint == true)
                        {
                            cornerDescpt = CommonModule.slabJointCornerDescp;
                            cornerType = SlabJointHelper.getSJCornerText(cornerId);
                            cornerLayerName = CommonModule.slabJointCornerLayerName;
                            cornerTextLayerName = CommonModule.slabJointCornerTextLayerName;
                            cornerLayerColor = CommonModule.slabJointCornerLayerColor;
                        }
                        else
                        {
                            if (listOfKickerCornerId_In_ConcaveAngle.Contains(cornerId))
                            {
                                cornerType = CommonModule.externalKickerCornerText;
                            }
                            else
                            {
                                cornerType = CommonModule.kickerCornerText;
                                //Added to fix KC panels position by SDM 2022-07-17  //Updated to fix LF and DU case by SDM 2022-08-25
                                if (listOfFlange.Count != 0 && xDataRegAppName.Contains("_EX_"))
                                {
                                    flange1 = listOfFlange[0];
                                    flange2 = listOfFlange[1];
                                }
                            }
                            cornerDescpt = CommonModule.kickerCornerDescp;
                            cornerLayerName = CommonModule.kickerCornerLayerName;
                            cornerTextLayerName = CommonModule.kickerCornerTextLayerName;
                            cornerLayerColor = CommonModule.kickerCornerLayerColor;
                        }
                        string cornerText = CornerHelper.getCornerText(cornerType, flange1, flange2, cornerHeight);

                        //var crn_angle = listOfKickerRotationAngle[i];
                        //if (crn_angle%360 == 90)
                        //{
                        //    cornerText = CornerHelper.getCornerText(cornerType, flange2, flange1, cornerHeight);
                        //}

                        var cornerTextId = cornerHlp.writeTextInCorner(cornerId, cornerText, cornerLineAngle, cornerTextLayerName, cornerLayerColor);

                        var listOfCornerVertexPoint = AEE_Utility.GetPolylineVertexPoint(cornerId);
                        Point3d basePoint = AEE_Utility.GetBasePointOfPolyline(cornerId);


                        double[] val = getNearestPointOfKickerCorner(cornerId, basePoint).ToArray();
                        Point3d basepnt = new Point3d(Convert.ToDouble(val[0]) + moveVector.X, Convert.ToDouble(val[1]), Convert.ToDouble(val[2]));

                        double minX = listOfCornerVertexPoint.Min(point => point.X) + moveVector.X;
                        double maxX = listOfCornerVertexPoint.Max(point => point.X) + moveVector.X;
                        double minY = listOfCornerVertexPoint.Min(point => point.Y);
                        double maxY = listOfCornerVertexPoint.Max(point => point.Y);
                        double center_X = minX + ((maxX - minX) / 2);
                        double center_Y = minY + ((maxY - minY) / 2);

                        kIcker_corner_CLine kIcker_corner = new kIcker_corner_CLine();
                        kIcker_corner.basepnt = basepnt;
                        kIcker_corner.wallname = xDataRegAppName;
                        kIcker_corner.center_X = center_X;
                        kIcker_corner.center_Y = center_Y;
                        kIcker_corner.flange1 = flange1;
                        kIcker_corner.flange2 = flange2;

                        CommonModule.kIcker_corner_CLine.Add(kIcker_corner);



                        //Added to fix KC panels position by SDM 2022-07-17
                        int wall_idx = Convert.ToInt32(xDataRegAppName.Split('_').GetValue(0).ToString().Substring(1));
                        var xDataRegAppName1 = xDataRegAppName;// AEE_Utility.GetXDataRegisterAppName(listOfWallLineId[i]);
                        var xDataRegAppName2 = AEE_Utility.GetXDataRegisterAppName(listOfWallLineId[(wall_idx) % listOfWallLineId.Count]);
                        //double len = AEE_Utility.GetLengthOfLine(listOfWallLineId[i]);
                        double len = AEE_Utility.GetLengthOfLine(listOfWallLineId[wall_idx - 1]);

                        //Fix KC position in the Sunkan case by SDM 30-06-2022
                        //start
                        CornerElevation elev = null;
                        if (cornerType == CommonModule.kickerCornerText)
                            elev = new CornerElevation(xDataRegAppName1, xDataRegAppName2, len - flange1, 0, PanelLayout_UI.floorHeight - cornerHeight);
                        else
                            //Fix elevation when the floor Ht is more than 3000 by SDM 2022-07-14
                            // elev = new CornerElevation(xDataRegAppName1, xDataRegAppName2, len - flange1 + CommonModule.externalCorner, -CommonModule.externalCorner, PanelLayout_UI.maxHeightOfPanel - PanelLayout_UI.RC);
                            elev = new CornerElevation(xDataRegAppName1, xDataRegAppName2, len - flange1 + CommonModule.externalCorner, -CommonModule.externalCorner, PanelLayout_UI.floorHeight - cornerHeight);
                        //end

                        CornerHelper.setCornerDataForBOM(xDataRegAppName, cornerId, cornerTextId, cornerText, cornerDescpt, cornerType, flange1, flange2, cornerHeight, elev);
                        AEE_Utility.changeLayer(cornerId, cornerLayerName, cornerLayerColor);
                        AEE_Utility.MoveEntity(cornerId, moveVector);
                        AEE_Utility.MoveEntity(cornerTextId, moveVector);
                    }
                }
            }
        }


        public double getAngleOfCornerText(ObjectId cornerId)
        {
            var colonCornerId = AEE_Utility.createColonEntityInSamePoint(cornerId, false);
            var listOfCornerExplodeId = AEE_Utility.ExplodeEntity(colonCornerId);
            ObjectId firstLineId = listOfCornerExplodeId[0];
            ObjectId lastLineId = listOfCornerExplodeId[(listOfCornerExplodeId.Count - 1)];

            double firstLineAngle = AEE_Utility.GetAngleOfLine(firstLineId);
            double lastLineAngle = AEE_Utility.GetAngleOfLine(lastLineId);

            double maxAngle = Math.Max(firstLineAngle, lastLineAngle);
            double minAngle = Math.Min(firstLineAngle, lastLineAngle);

            double centerAngle = minAngle + ((maxAngle - minAngle) / 2);

            var basePoint = AEE_Utility.GetBasePointOfPolyline(cornerId);
            var point = AEE_Utility.get_XY(centerAngle, 1000);

            Point3d strtPoint = new Point3d((basePoint.X - point.X), (basePoint.Y - point.Y), 0);
            Point3d endPoint = new Point3d((basePoint.X + point.X), (basePoint.Y + point.Y), 0);

            //var lineId = AEE_Utility.getLineId(strtPoint, endPoint, true);


            AEE_Utility.deleteEntity(listOfCornerExplodeId);

            return centerAngle;
        }


        private void setKickerWallLine(string xDataRegAppName, ObjectId crrntKickerCornerId, List<int> listOfCrrntVerticePointIndex_ForStretch, ObjectId prvsKickerCornerId, List<int> listOfPrvsVerticePointIndex_ForStretch, bool flagOfWallPanelCreate, Entity wallEnt, double kickerHeight, bool flagOfSlabJoint)
        {
            if (flagOfWallPanelCreate == true)
            {
                if (listOfCrrntVerticePointIndex_ForStretch.Count == 2 && listOfPrvsVerticePointIndex_ForStretch.Count == 2)
                {
                    var listOfCrrntKickerVertices = AEE_Utility.GetPolylineVertexPoint(crrntKickerCornerId);
                    var listOfPrvsKickerVertices = AEE_Utility.GetPolylineVertexPoint(prvsKickerCornerId);

                    Point2d kickerPoint1 = listOfCrrntKickerVertices[listOfCrrntVerticePointIndex_ForStretch[0]];
                    Point2d kickerPoint2 = listOfCrrntKickerVertices[listOfCrrntVerticePointIndex_ForStretch[1]];

                    Point2d prvsKickerPoint1 = listOfPrvsKickerVertices[listOfPrvsVerticePointIndex_ForStretch[0]];
                    Point2d prvsKickerPoint2 = listOfPrvsKickerVertices[listOfPrvsVerticePointIndex_ForStretch[1]];

                    var baseWallLineId = AEE_Utility.getLineId(wallEnt, kickerPoint1.X, kickerPoint1.Y, 0, prvsKickerPoint1.X, prvsKickerPoint1.Y, 0, false);

                    var baseWallOffsetLineId = AEE_Utility.getLineId(wallEnt, kickerPoint2.X, kickerPoint2.Y, 0, prvsKickerPoint2.X, prvsKickerPoint2.Y, 0, false);

                    var listOfInsct = AEE_Utility.InterSectionBetweenTwoEntity(baseWallLineId, baseWallOffsetLineId);
                    if (listOfInsct.Count != 0)
                    {
                        AEE_Utility.deleteEntity(baseWallLineId);
                        AEE_Utility.deleteEntity(baseWallOffsetLineId);

                        baseWallLineId = AEE_Utility.getLineId(wallEnt, kickerPoint1.X, kickerPoint1.Y, 0, prvsKickerPoint2.X, prvsKickerPoint2.Y, 0, false);

                        baseWallOffsetLineId = AEE_Utility.getLineId(wallEnt, kickerPoint2.X, kickerPoint2.Y, 0, prvsKickerPoint1.X, prvsKickerPoint1.Y, 0, false);
                    }

                    AEE_Utility.AttachXData(baseWallLineId, xDataRegAppName, CommonModule.xDataAsciiName);

                    KickerHelper.listOfKickerWallLineId.Add(baseWallLineId);
                    KickerHelper.listOfKickerOffsetWallLineId.Add(baseWallOffsetLineId);
                    KickerHelper.listOfKickerHeight.Add(kickerHeight);
                }
            }
        }


        public void drawKickerWallPanel(ProgressForm progressForm, string wallCreationMsg, List<List<LineSegment3d>> listDoorLines)
        {
            CommonModule commonModule = new CommonModule();
            WindowHelper windowHlp = new WindowHelper();
            WallPanelHelper wallPanelHlp = new WallPanelHelper();
            CommonModule.kIcker_panel_CLine = new List<kIcker_panel_CLine>();

            for (int index = 0; index < listOfKickerWallLineId.Count; index++)
            {
                if ((index % 50) == 0)
                {
                    progressForm.ReportProgress(1, wallCreationMsg);
                }

                ObjectId wallLineId = KickerHelper.listOfKickerWallLineId[index];
                ObjectId offsetWallLineId = KickerHelper.listOfKickerOffsetWallLineId[index];

                var xDataRegAppName = AEE_Utility.GetXDataRegisterAppName(wallLineId);

                double kickerHeight = KickerHelper.listOfKickerHeight[index];

                var wallPanelLine = AEE_Utility.GetLine(wallLineId);
                var listOfWallPanelLineStrtEndPoint = commonModule.getStartEndPointOfLine(wallPanelLine);
                Point3d wallPanelLineStrtPoint = listOfWallPanelLineStrtEndPoint[0];
                Point3d wallPanelLineEndPoint = listOfWallPanelLineStrtEndPoint[1];

                double wallPanelLength = AEE_Utility.GetLengthOfLine(wallPanelLineStrtPoint, wallPanelLineEndPoint);
                double wallPanelLineAngle = AEE_Utility.GetAngleOfLine(wallPanelLineStrtPoint, wallPanelLineEndPoint);
                wallPanelLength = Math.Round(wallPanelLength);
                if (wallPanelLength == 0)
                {
                    continue;
                }
                var wallPanelOffsetLine = AEE_Utility.GetLine(offsetWallLineId);
                var listOfWallPanelOffsetLineStrtEndPoint = commonModule.getStartEndPointOfLine(wallPanelOffsetLine);
                Point3d wallPanelOffstLineStrtPoint = listOfWallPanelOffsetLineStrtEndPoint[0];
                Point3d wallPanelOffstLineEndPoint = listOfWallPanelOffsetLineStrtEndPoint[1];

                bool flag_X_Axis = false;
                bool flag_Y_Axis = false;
                commonModule.checkAngleOfLine_Axis(wallPanelLineAngle, out flag_X_Axis, out flag_Y_Axis);

                var baseLine = new LineSegment3d(wallPanelLineStrtPoint, wallPanelLineEndPoint);
                var offsetLine = new LineSegment3d(wallPanelOffstLineStrtPoint, wallPanelOffstLineEndPoint);
                var doorLine = GetOverlapDoorLine(listDoorLines, new LineSegment3d(wallPanelOffstLineStrtPoint, wallPanelOffstLineEndPoint));
                var offsetVec = wallPanelLineStrtPoint - wallPanelOffstLineStrtPoint;
                var offsetVecs = new Vector3d[] { offsetVec, new Vector3d(0, 0, 0) };
                LineSegment3d[] splittedLines = null;
                if (doorLine == null)
                {
                    doorLine = GetOverlapDoorLine(listDoorLines, new LineSegment3d(wallPanelLineStrtPoint, wallPanelOffstLineEndPoint + offsetVec));
                    if (doorLine == null)
                    {
                        createWallPanelFromPoints(wallPanelHlp, wallLineId, xDataRegAppName, kickerHeight, wallPanelLineStrtPoint, wallPanelLength,
                            wallPanelLineAngle, wallPanelOffstLineStrtPoint, flag_X_Axis, flag_Y_Axis);
                        continue;
                    }
                    else
                    {
                        offsetVecs = new Vector3d[] { new Vector3d(0, 0, 0), -offsetVec };
                        splittedLines = GetSplittedLines(baseLine, doorLine);
                    }
                }
                else
                {
                    splittedLines = GetSplittedLines(offsetLine, doorLine);
                }
                if (splittedLines == null || splittedLines.Length == 0)
                {
                    continue;
                }
                foreach (var line in splittedLines)
                {
                    var sp = line.StartPoint + offsetVecs[0];
                    var offsetSP = line.StartPoint + offsetVecs[1];
                    createWallPanelFromPoints(wallPanelHlp, wallLineId, xDataRegAppName, kickerHeight, sp, line.Length,
                        wallPanelLineAngle, offsetSP, flag_X_Axis, flag_Y_Axis);
                }
            }
        }

        private LineSegment3d[] GetSplittedLines(LineSegment3d refLine, LineSegment3d doorLine)
        {
            var dT = 1e-6;
            var tol = new Tolerance(dT, dT);
            var param = new List<double>();
            var pts = new Point3d[] { doorLine.StartPoint, doorLine.EndPoint };

            if (refLine.IsOn(doorLine.StartPoint, tol) && refLine.IsOn(doorLine.EndPoint, tol))
            {
                if (refLine.StartPoint.DistanceTo(doorLine.StartPoint) < refLine.StartPoint.DistanceTo(doorLine.EndPoint))
                {
                    return new LineSegment3d[]
                    {
                        new LineSegment3d(refLine.StartPoint, doorLine.StartPoint),
                        new LineSegment3d(doorLine.EndPoint, refLine.EndPoint)
                    };
                }
                return new LineSegment3d[]
                {
                    new LineSegment3d(refLine.StartPoint, doorLine.EndPoint),
                    new LineSegment3d(doorLine.StartPoint, refLine.EndPoint)
                };
            }
            if (refLine.IsOn(doorLine.StartPoint))
            {
                if (refLine.Direction.IsCodirectionalTo(doorLine.Direction, tol))
                {
                    return new LineSegment3d[]
                    {
                        new LineSegment3d(refLine.StartPoint, doorLine.StartPoint)
                    };
                }
                return new LineSegment3d[]
                {
                        new LineSegment3d(doorLine.StartPoint, refLine.EndPoint)
                };
            }
            if (refLine.IsOn(doorLine.EndPoint))
            {
                if (refLine.Direction.IsCodirectionalTo(doorLine.Direction, tol))
                {
                    return new LineSegment3d[]
                    {
                        new LineSegment3d(doorLine.EndPoint, refLine.EndPoint)
                    };
                }
                return new LineSegment3d[]
                {
                        new LineSegment3d(refLine.StartPoint, doorLine.EndPoint)
                };
            }
            return new LineSegment3d[] { };
        }

        private static void createWallPanelFromPoints(WallPanelHelper wallPanelHlp, ObjectId wallLineId, string xDataRegAppName, double kickerHeight, Point3d wallPanelLineStrtPoint, double wallPanelLength, double wallPanelLineAngle, Point3d wallPanelOffstLineStrtPoint, bool flag_X_Axis, bool flag_Y_Axis)
        {
            List<double> listOfWallPanelLength = wallPanelHlp.getListOfWallPanelLength(wallPanelLength, CommonModule.maxLngthOfKickerPanel, PanelLayout_UI.minWidthOfPanel);

            Point3d startPoint = wallPanelLineStrtPoint;
            Point3d offsetStartPoint = wallPanelOffstLineStrtPoint;
            string wallText = PanelLayout_UI.kickerWallName;
            string wallItemCode = PanelLayout_UI.kickerWallName;
            Vector3d moveVector = CreateShellPlanHelper.moveVector_ForSunkanSlabLayout;

            string wallPanelLayerName = CommonModule.kickerWallPanelLayerName;
            string wallPanelTextLayerName = CommonModule.kickerWallPanelTextLayerName;
            int wallPanelLayerColor = CommonModule.kickerWallPanelLayerColor;

            string wallType = CommonModule.kickerPanelType;
            for (int k = 0; k < listOfWallPanelLength.Count; k++)
            {
                double wallPnlLngth = listOfWallPanelLength[k];
                var point = AEE_Utility.get_XY(wallPanelLineAngle, wallPnlLngth);
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

                    var wallPanelRect_Id = AEE_Utility.createRectangle(startPoint, endPoint, offstEndPoint, offsetStartPoint, wallPanelLayerName, wallPanelLayerColor);
                    AEE_Utility.AttachXData(wallPanelRect_Id, xDataRegAppName, CommonModule.xDataAsciiName);
                    listOfMoveId.Add(wallPanelRect_Id);

                    var wallPanelLine_Id = AEE_Utility.getLineId(startPoint, endPoint, false);
                    AEE_Utility.AttachXData(wallPanelLine_Id, xDataRegAppName, CommonModule.xDataAsciiName);
                    listOfMoveId.Add(wallPanelLine_Id);

                    Point3d dimTextPoint = WallPanelHelper.getCenterPointOfPanelRectangle(wallPanelRect_Id);
                    string sunkanSlabPanelText = DeckPanelHelper.getWallPanelText(wallPnlLngth, wallText, kickerHeight);
                    string wallDescp = sunkanSlabPanelText;

                    ObjectId dimTextId2 = wallPanelHlp.writeDimensionTextInWallPanel(sunkanSlabPanelText, wallLineId, dimTextPoint, wallPanelLineAngle, wallPanelTextLayerName, wallPanelLayerColor);
                    listOfMoveId.Add(dimTextId2);

                    wallPanelHlp.setBOMDataOfWallPanel(dimTextId2, wallPanelRect_Id, wallPanelLine_Id /*wallLineId*/, wallPnlLngth, kickerHeight, 0, wallItemCode, wallDescp, wallType);

                    //Remove the top kicker panels for Internal Sunkan by SDM 2022-07-17 
                    //double elev = 0;
                    //if (xDataRegAppName.Contains("_R_"))
                    //{
                    //   // wallPanelHlp.setBOMDataOfWallPanel(dimTextId2, wallPanelRect_Id, wallPanelLine_Id /*wallLineId*/, wallPnlLngth, kickerHeight, 0, wallItemCode, wallDescp, wallType);

                    //    //Added for the Sunkan case by SDM 30-06-2022
                    //    //start
                    //    elev = PanelLayout_UI.maxHeightOfPanel;// + kickerHeight - PanelLayout_UI.RC;
                    //}
                    //else
                    //    //Fix elevation when the floor Ht is more than 3000 by SDM 2022-07-14
                    //   // elev = PanelLayout_UI.maxHeightOfPanel + kickerHeight - PanelLayout_UI.RC;
                    //    elev = PanelLayout_UI.defaultFloorHeight;
                    ////end
                    //if (xDataRegAppName.Contains("_R_"))
                    //{
                    //    wallPanelHlp.setBOMDataOfWallPanel(dimTextId2, wallPanelRect_Id, wallPanelLine_Id /*wallLineId*/, wallPnlLngth, kickerHeight, 0, wallItemCode, wallDescp, wallType, elev.ToString());
                    //}
                    if (!xDataRegAppName.Contains("_R_"))
                        wallPanelHlp.setBOMDataOfWallPanel(dimTextId2, wallPanelRect_Id, wallPanelLine_Id /*wallLineId*/, wallPnlLngth, kickerHeight, 0, wallItemCode, wallDescp, wallType, PanelLayout_UI.floorHeight.ToString());


                    var circleId = wallPanelHlp.createCircleInNonStandardWallPanel(dimTextPoint.X, dimTextPoint.Y, wallPnlLngth);

                    if (circleId.IsValid == true)
                    {
                        listOfMoveId.Add(circleId);
                    }
                    AEE_Utility.MoveEntity(listOfMoveId, moveVector);
                    if (!xDataRegAppName.Contains("_R_"))
                    {
                        kIcker_panel_CLine kicker_cline = new kIcker_panel_CLine();
                        kicker_cline.startpoint = startPoint;
                        kicker_cline.length = length;
                        kicker_cline.movevector = moveVector;
                        kicker_cline.wallname = xDataRegAppName;
                        kicker_cline.offsetpoint = offsetStartPoint;
                        kicker_cline.x_axis = flag_X_Axis;
                        kicker_cline.listOfWallPanelLength = listOfWallPanelLength;
                        kicker_cline.angle = wallPanelLineAngle;

                        CommonModule.kIcker_panel_CLine.Add(kicker_cline);
                    }
                }

                startPoint = endPoint;
                offsetStartPoint = offstEndPoint;
            }
        }

        private bool IsOverlap(List<List<LineSegment3d>> listDoorLines, LineSegment3d lineSegment3d)
        {
            var line = GetOverlapDoorLine(listDoorLines, lineSegment3d);
            return line != null;
        }

        private LineSegment3d GetOverlapDoorLine(List<List<LineSegment3d>> listDoorLines, LineSegment3d lineSegment3d)
        {
            var line = GetDoorOverlapInternal(listDoorLines, lineSegment3d);
            if (line != null)
            {
                return line;
            }
            return GetKickerOverlapDoorLine(listDoorLines, lineSegment3d);
        }

        private LineSegment3d GetKickerOverlapDoorLine(List<List<LineSegment3d>> listDoorLines, LineSegment3d lineSegment3d)
        {
            var dT = 1e-6;
            var tol = new Tolerance(dT, dT);
            foreach (var lst in listDoorLines)
            {
                foreach (var line in lst)
                {
                    if (Math.Round(Math.Abs(line.Direction.X)) == Math.Round(Math.Abs(lineSegment3d.Direction.X)) || Math.Round(Math.Abs(line.Direction.Y)) == Math.Round(Math.Abs(lineSegment3d.Direction.Y)))
                    {
                        var xline = new LineSegment3d(lineSegment3d.StartPoint, lineSegment3d.EndPoint);
                        var isOn = xline.IsOn(line.StartPoint, tol) || xline.IsOn(line.EndPoint, tol) ||
                                xline.IsOn(line.MidPoint, tol);
                        if (isOn)
                        {
                            return line;
                        }
                    }
                }
            }
            return null;
        }

        private LineSegment3d GetDoorOverlapInternal(List<List<LineSegment3d>> listDoorLines, LineSegment3d lineSegment3d)
        {
            var dT = 1e-6;
            var tol = new Tolerance(dT, dT);
            foreach (var lst in listDoorLines)
            {
                foreach (var line in lst)
                {
                    if (Math.Round(Math.Abs(line.Direction.X)) == Math.Round(Math.Abs(lineSegment3d.Direction.X)) || Math.Round(Math.Abs(line.Direction.Y)) == Math.Round(Math.Abs(lineSegment3d.Direction.Y)))
                    {
                        var isOn = line.IsOn(lineSegment3d.StartPoint, tol) || line.IsOn(lineSegment3d.EndPoint, tol) ||
                       line.IsOn(lineSegment3d.MidPoint, tol);
                        if (isOn)
                        {
                            return line;
                        }
                    }
                }
            }
            return null;
        }

        public void callMethodOfExternalKickerCorner(int index, ObjectId prvsWallLineId, ObjectId lastWallLineId, ObjectId wallId, List<ObjectId> listOfWallLineId, ObjectId insideOffsetWallId, List<ObjectId> listOfInsideOffsetWallLineId, ObjectId outsideOffsetWallId, List<ObjectId> listOfOutsideOffsetWallLineId, ObjectId sunkanSlabId)
        {
            if (sunkanSlabId.IsValid == true)
            {
                var flagOfGreater = SunkanSlabHelper.checkSunkanSlabLevelDiffrnce_Is_Greater(sunkanSlabId);
                if (flagOfGreater == false)
                {
                    return;
                }
            }

            if (index == 0)
            {
                listOfKickerCornerId.Clear();
                listOfKickerCornerBasePoint.Clear();
                listOfKickerCornerNrstBasePoint.Clear();
                listOfKickerCornerLineId.Clear();
                listOfKickerRotationAngle.Clear();
                listOfKickerCornerId_In_ConcaveAngle.Clear();
                listOfKickerCornerId_ForText.Clear();
                listOfKickerCornerFlanges_ForText.Clear();
            }
            bool flagOfSlabJoint = false;

            var wallEnt = AEE_Utility.GetEntityForRead(wallId);

            double panelDepth = CommonModule.panelDepth;
            double kickerHeight = PanelLayout_UI.kickerHt;
            Vector3d moveVector = CreateShellPlanHelper.moveVector_ForSunkanSlabLayout;
            if (sunkanSlabId.IsValid == true)
            {
                kickerHeight = SunkanSlabHelper.getSunkanSlabLevelDifference(sunkanSlabId);
            }
            else if (sunkanSlabId.IsValid == false && InternalWallSlab_UI_Helper.IsInternalWall(wallEnt.Layer))
            {
                flagOfSlabJoint = true;
                moveVector = CreateShellPlanHelper.moveVector_ForSlabJointLayout;
                panelDepth = CommonModule.slabJointPanelDepth;
                kickerHeight = PanelLayout_UI.SL;
            }

            drawKickerCorner_In_ExternalWall(index, prvsWallLineId, lastWallLineId, wallId, wallEnt, listOfWallLineId, insideOffsetWallId, listOfInsideOffsetWallLineId, outsideOffsetWallId, listOfOutsideOffsetWallLineId, kickerHeight, panelDepth, flagOfSlabJoint);//PRT

            if (index == (listOfWallLineId.Count - 1))
            {
                createTextInKickerCorner(kickerHeight, listOfKickerCornerId, listOfKickerCornerLineId, listOfWallLineId, listOfKickerRotationAngle, listOfKickerCornerFlanges_ForText, moveVector, flagOfSlabJoint);
            }
        }

        private void drawKickerCorner_In_ExternalWall(int index, ObjectId prvsWallLineId, ObjectId lastWallLineId, ObjectId wallId, Entity wallEnt, List<ObjectId> listOfWallLineId, ObjectId insideOffsetWallId, List<ObjectId> listOfInsideOffsetWallLineId, ObjectId outsideOffsetWallId, List<ObjectId> listOfOutsideOffsetWallLineId, double kickerHeight, double panelDepth, bool flagOfSlabJoint)
        {
            PartHelper partHelper = new PartHelper();

            ObjectId crrntLineId = listOfWallLineId[index];

            ObjectId prvsLineId = new ObjectId();
            ObjectId nextLineId = new ObjectId();
            if (index == 0)
            {
                prvsLineId = listOfWallLineId[(listOfWallLineId.Count - 1)];
            }
            else
            {
                prvsLineId = listOfWallLineId[(index - 1)];
            }

            if (index == (listOfWallLineId.Count - 1))
            {
                nextLineId = listOfWallLineId[0];
            }
            else
            {
                nextLineId = listOfWallLineId[index + 1];
            }

            double crrntLineAngle = AEE_Utility.GetAngleOfLine(crrntLineId);
            double crrntLineLngth = AEE_Utility.GetLengthOfLine(crrntLineId);
            var listOfCrrntStrtEndPoint = AEE_Utility.GetStartEndPointOfLine(crrntLineId);
            Point3d crrntStrtPoint = listOfCrrntStrtEndPoint[0];
            Point3d crrntEndPoint = listOfCrrntStrtEndPoint[1];

            double prvsLineAngle = AEE_Utility.GetAngleOfLine(prvsLineId);
            double prvsLineLngth = AEE_Utility.GetLengthOfLine(prvsLineId);
            var listOfPrvsStrtEndPoint = AEE_Utility.GetStartEndPointOfLine(prvsLineId);
            Point3d prvsStrtPoint = listOfPrvsStrtEndPoint[0];
            Point3d prvsEndPoint = listOfPrvsStrtEndPoint[1];

            double nextLineAngle = AEE_Utility.GetAngleOfLine(nextLineId);
            double nextLineLngth = AEE_Utility.GetLengthOfLine(nextLineId);
            var listOfNextStrtEndPoint = AEE_Utility.GetStartEndPointOfLine(nextLineId);
            Point3d nextStrtPoint = listOfNextStrtEndPoint[0];
            Point3d nextEndPoint = listOfNextStrtEndPoint[1];

            double kickerCornerFlange = CommonModule.minKickerCornrFlange;
            double kickerCornr_Flange1 = kickerCornerFlange;
            double kickerCornr_Flange2 = kickerCornerFlange;

            Point3d basePoint = crrntStrtPoint;

            ObjectId kickerCornerId = new ObjectId();

            var xDataRegAppName = AEE_Utility.GetXDataRegisterAppName(prvsWallLineId);

            //var polyline = getPolylineOfKickerCorner(kickerCornr_Flange1, kickerCornr_Flange2, panelDepth, crrntLineAngle, prvsLineAngle, nextLineAngle);
            //kickerCornerId = AEE_Utility.CreatePolyLine(polyline, basePoint.X, basePoint.Y, CommonModule.kickerCornerLayerName, CommonModule.kickerCornerLayerColor);

            PartHelper partHlp = new PartHelper();
            kickerCornerId = partHlp.drawExternalCorner(xDataRegAppName, basePoint.X, basePoint.Y, kickerCornerFlange, panelDepth, CommonModule.kickerCornerLayerName, CommonModule.kickerCornerLayerColor);
            AEE_Utility.AttachXData(kickerCornerId, xDataRegAppName, CommonModule.xDataAsciiName);
            AEE_Utility.AttachXData(kickerCornerId, xDataRegAppName + "_1", kickerCornerFlange.ToString() + "," +
                                                                            panelDepth.ToString());

            Point3d nrstOfBasePointCorner = getNearestPointOfKickerCorner(kickerCornerId, basePoint);
            double rotationAngle = 0;
            kickerCornerId = checkCornerIsInsideTheWall(index, kickerCornerId, crrntLineId, basePoint, crrntLineAngle, wallId, outsideOffsetWallId, listOfOutsideOffsetWallLineId, insideOffsetWallId, listOfInsideOffsetWallLineId, nrstOfBasePointCorner, out rotationAngle);
            if (listOfKickerCornerId_In_ConcaveAngle.Contains(kickerCornerId))
            {
                if (crrntLineLngth < CommonModule.minKickerPanelLineLength || prvsLineLngth < CommonModule.minKickerPanelLineLength)
                {
                    bool flagOfExternalDwg = false;
                    CommonModule common_Obj = new CommonModule();
                    rotationAngle = 0;
                    common_Obj.checkExternalCornerIsInside_Innerwall(xDataRegAppName, wallId, basePoint, crrntLineAngle, outsideOffsetWallId, index, listOfOutsideOffsetWallLineId, out flagOfExternalDwg, out rotationAngle);
                    if (flagOfExternalDwg == true)
                    {
                        CommonModule.rotateAngle = (rotationAngle * Math.PI) / 180; // variable assign for rotation    
                        List<ObjectId> listOfObjId = new List<ObjectId>();
                        var externalCornerId = partHelper.drawExternalCorner(xDataRegAppName, basePoint.X, basePoint.Y, CommonModule.extrnlCornr_Flange, CommonModule.externalCornerThick, CommonModule.externalCornerLyrName, CommonModule.externalCornerLyrColor);
                        AEE_Utility.AttachXData(externalCornerId, xDataRegAppName, CommonModule.xDataAsciiName);
                        AEE_Utility.AttachXData(externalCornerId, xDataRegAppName + "_1", CommonModule.extrnlCornr_Flange.ToString() + "," +
                                                                                          CommonModule.externalCornerThick.ToString());
                        nrstOfBasePointCorner = getNearestPointOfKickerCorner(externalCornerId, basePoint);
                        listOfObjId.Add(externalCornerId);
                        MoveRorationScaleJig.RotationScaleJig_Method(listOfObjId);
                        AEE_Utility.deleteEntity(kickerCornerId);
                        kickerCornerId = externalCornerId;
                        listOfKickerCornerId_In_ConcaveAngle.Add(kickerCornerId);
                    }
                }
            }

            basePoint = AEE_Utility.GetBasePointOfPolyline(kickerCornerId);
            nrstOfBasePointCorner = getNearestPointOfKickerCorner(kickerCornerId, basePoint);
            stretchKickerCorner(xDataRegAppName, listOfKickerCornerId, listOfKickerCornerBasePoint, listOfKickerCornerNrstBasePoint, kickerCornerId, basePoint, nrstOfBasePointCorner, prvsLineAngle, prvsLineLngth, kickerCornerFlange, wallEnt, kickerHeight, panelDepth, flagOfSlabJoint, outsideOffsetWallId, true);

            AEE_Utility.AttachXData(kickerCornerId, xDataRegAppName, CommonModule.xDataAsciiName);

            listOfKickerCornerBasePoint.Add(basePoint);
            listOfKickerCornerNrstBasePoint.Add(nrstOfBasePointCorner);
            listOfKickerCornerId.Add(kickerCornerId);
            listOfKickerCornerLineId.Add(crrntLineId);
            listOfKickerRotationAngle.Add(rotationAngle);

            if (index == (listOfWallLineId.Count - 1))
            {
                var last_XDataRegAppName = AEE_Utility.GetXDataRegisterAppName(lastWallLineId);

                ObjectId firstLineId = listOfWallLineId[0];
                double firstLineAngle = AEE_Utility.GetAngleOfLine(firstLineId);
                double firstLineLngth = AEE_Utility.GetLengthOfLine(firstLineId);

                ObjectId firstKickerCornerId = listOfKickerCornerId[0];
                Point3d firstBasePoint = listOfKickerCornerBasePoint[0];
                Point3d firstNrstOfBasePointCorner = listOfKickerCornerNrstBasePoint[0];
                stretchKickerCorner(last_XDataRegAppName, listOfKickerCornerId, listOfKickerCornerBasePoint, listOfKickerCornerNrstBasePoint, firstKickerCornerId, firstBasePoint, firstNrstOfBasePointCorner, crrntLineAngle, crrntLineLngth, kickerCornerFlange, wallEnt, kickerHeight, panelDepth, flagOfSlabJoint, outsideOffsetWallId, true);
            }
        }
    }
}
