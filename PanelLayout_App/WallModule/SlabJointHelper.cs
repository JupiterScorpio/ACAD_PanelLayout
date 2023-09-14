using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using PanelLayout_App.CivilModel;
using PanelLayout_App.ElevationModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanelLayout_App.WallModule
{
    public class SlabJointHelper
    {
        private static List<ObjectId> listOfSlabJointCornerId = new List<ObjectId>();
        private static List<Point3d> listOfSlabJointCornerBasePoint = new List<Point3d>();
        private static List<Point3d> listOfSlabJointCornerNrstBasePoint = new List<Point3d>();
        private static List<ObjectId> listOfSlabJointCornerLineId = new List<ObjectId>();
        private static List<ObjectId> listOfSlabJointCornerTextId = new List<ObjectId>();
        private static List<double> listOfSlabJointRotationAngle = new List<double>();
        private static List<ObjectId> listOfSlabJointCornerId_ForText = new List<ObjectId>();
        private static List<string> listOfSlabJointCornerFlanges_ForText = new List<string>();
        private static List<ObjectId> listOfSlabJointCornerId_In_ConcaveAngle = new List<ObjectId>();

        public static List<ObjectId> listOfNearestBeamLineId_ForSlabJoint = new List<ObjectId>();
        public static List<double> listOfDistBtwBeamToWall_ForSlabJoint = new List<double>();
        public static List<ObjectId> listOfWallId_ForSlabJoint = new List<ObjectId>();

        public static List<ObjectId> listOfSlabJointWallLineId = new List<ObjectId>();
        public static List<ObjectId> listOfSlabJointOffsetWallLineId = new List<ObjectId>();
        public static List<double> listOfSlabJointHeight = new List<double>();
        public static List<double> listOfSlabJointElev = new List<double>();

        private static List<Point3d> listOfDeckPanelWallPoint = new List<Point3d>();
        public void drawSlabJointCorner(int index, ObjectId prvsWallLineId, ObjectId lastWallLineId, ObjectId wallId, List<ObjectId> listOfWallLineId, ObjectId insideOffsetWallId, List<ObjectId> listOfInsideOffsetWallLineId, ObjectId outsideOffsetWallId, List<ObjectId> listOfOutsideOffsetWallLineId, ObjectId sunkanSlabId)
        {
            if (index == 0)
            {
                listOfSlabJointCornerId.Clear();
                listOfSlabJointCornerBasePoint.Clear();
                listOfSlabJointCornerNrstBasePoint.Clear();
                listOfSlabJointCornerLineId.Clear();
                listOfSlabJointCornerTextId.Clear();
                listOfSlabJointRotationAngle.Clear();
                listOfSlabJointCornerId_In_ConcaveAngle.Clear();
                listOfSlabJointCornerId_ForText.Clear();
                listOfSlabJointCornerFlanges_ForText.Clear();
                listOfDeckPanelWallPoint.Clear();
            }
            var flagOfExternalSunkanSlabLayer = ExternalSunkanSlab_UI_Helper.checkExternalSunkanSlabLayerIsExist(sunkanSlabId);
            if (flagOfExternalSunkanSlabLayer == true)
            {
            //    return;
            }

            var wallEnt = AEE_Utility.GetEntityForRead(wallId);

            if (flagOfExternalSunkanSlabLayer || InternalWallSlab_UI_Helper.IsInternalWall(wallEnt.Layer))
            {
                bool flagOfSlabJoint = true;

                Vector3d moveVector = CreateShellPlanHelper.moveVector_ForSlabJointLayout;
                double panelDepth = CommonModule.slabJointPanelDepth;
                double slabJointHeight = PanelLayout_UI.SL;     

                InternalWallHelper internalWallHlp = new InternalWallHelper();

                drawSlabJointCorner_In_InternalWall(index, prvsWallLineId, lastWallLineId, wallId, wallEnt, listOfWallLineId, insideOffsetWallId, listOfInsideOffsetWallLineId, outsideOffsetWallId, listOfOutsideOffsetWallLineId, slabJointHeight, panelDepth, flagOfSlabJoint);
            }
        }

        private void drawSlabJointCorner_In_InternalWall(int index, ObjectId prvsWallLineId, ObjectId lastWallLineId, ObjectId wallId, Entity wallEnt, List<ObjectId> listOfWallLineId, ObjectId insideOffsetWallId, List<ObjectId> listOfInsideOffsetWallLineId, ObjectId outsideOffsetWallId, List<ObjectId> listOfOutsideOffsetWallLineId, double slabJointHeight, double panelDepth, bool flagOfSlabJoint)
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

            double slabJointCornerFlange = CommonModule.minKickerCornrFlange;          

            Point3d basePoint = crrntStrtPoint;

            ObjectId slabJointCornerId = new ObjectId();

            var xDataRegAppName = AEE_Utility.GetXDataRegisterAppName(prvsWallLineId);
    
            PartHelper partHlp = new PartHelper();
            slabJointCornerId = partHlp.drawExternalCorner(xDataRegAppName, basePoint.X, basePoint.Y, slabJointCornerFlange, panelDepth, CommonModule.kickerCornerLayerName, CommonModule.kickerCornerLayerColor);

            AEE_Utility.AttachXData(slabJointCornerId, xDataRegAppName, CommonModule.xDataAsciiName);
            AEE_Utility.AttachXData(slabJointCornerId, xDataRegAppName + "_1", slabJointCornerFlange.ToString() + "," +
                                                                               panelDepth.ToString());

            KickerHelper kickerHlp = new KickerHelper();
            Point3d nrstOfBasePointCorner = kickerHlp.getNearestPointOfKickerCorner(slabJointCornerId, basePoint);
            double rotationAngle = 0;
            slabJointCornerId = checkCornerIsInsideTheWall(index, slabJointCornerId, crrntLineId, basePoint, crrntLineAngle, wallId, insideOffsetWallId, listOfInsideOffsetWallLineId, outsideOffsetWallId, listOfOutsideOffsetWallLineId, nrstOfBasePointCorner, out rotationAngle);
            AEE_Utility.MoveEntity(slabJointCornerId, CreateShellPlanHelper.moveVector_ForSlabJointLayout);
       
            basePoint = AEE_Utility.GetBasePointOfPolyline(slabJointCornerId);
            nrstOfBasePointCorner = kickerHlp.getNearestPointOfKickerCorner(slabJointCornerId, basePoint);

            //AEE_Utility.AttachXData(slabJointCornerId, xDataRegAppName, CommonModule.xDataAsciiName);
            //AEE_Utility.AttachXData(slabJointCornerId, xDataRegAppName + "_1", slabJointCornerFlange.ToString() + "," +
            //                                                                   panelDepth.ToString());

            listOfSlabJointCornerBasePoint.Add(basePoint);
            listOfSlabJointCornerNrstBasePoint.Add(nrstOfBasePointCorner);
            listOfSlabJointCornerId.Add(slabJointCornerId);
            listOfSlabJointCornerLineId.Add(crrntLineId);
            listOfSlabJointRotationAngle.Add(rotationAngle);

        }

        private ObjectId checkCornerIsInsideTheWall(int index, ObjectId slabJointCornerId, ObjectId crrntLineId, Point3d basePoint, double crrntLineAngle, ObjectId wallId, ObjectId insideOffsetWallId, List<ObjectId> listOfInsideOffsetWallLineId, ObjectId outsideOffsetWallId, List<ObjectId> listOfOutsideOffsetWallLineId, Point3d nrstOfBasePointCorner, out double rotationAngle)
        {
            rotationAngle = 0;

            var colon_KickerCornerId = AEE_Utility.createColonEntityInSamePoint(slabJointCornerId, true);

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

            if (index == (listOfInsideOffsetWallLineId.Count - 1))
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
            KickerHelper kickerHlp = new KickerHelper();
            bool flagOfCornerInside = kickerHlp.cornerRotate(slabJointCornerId, outsideOffsetWallId, basePoint, crrntInsideOffsetLineId, prvsInsideOffsetLineId, false, crrntLineAngle, out rotationAngle);
            if (flagOfCornerInside == true)
            {
                AEE_Utility.deleteEntity(colon_KickerCornerId);
            }
            else
            {
                AEE_Utility.deleteEntity(slabJointCornerId);
                kickerHlp.drawCorner_In_ConcaveAngle(basePoint, nrstOfBasePointCorner, colon_KickerCornerId, outsideOffsetWallId, crrntOutsideOffsetLineId, prvsOutsideOffsetLineId, crrntLineAngle, out rotationAngle);
                slabJointCornerId = colon_KickerCornerId;
                listOfSlabJointCornerId_In_ConcaveAngle.Add(slabJointCornerId);
            }

            return slabJointCornerId;
        }

        private void stretchSlabJointCorner(string xDataRegAppName, List<ObjectId> listOfSlabJointCornerId, List<Point3d> listOfAllSlabJointCornerBasePoint, List<Point3d> listOfAllSlabJointCornerNrstBasePoint, 
                                            ObjectId crrntSlabJointCornerId, Point3d basePoint, Point3d nrstOfBasePointCorner, double stretchLineAngle, double stretchLineLength, double slabJointCornerFlange, 
                                            Entity wallEnt, double slabJointHeight, double panelDepth, bool flagOfSlabJoint, double elev)
        {
            KickerHelper kickerHlp = new KickerHelper();
            if (listOfSlabJointCornerId.Count != 0)
            {
                int prvsDataIndex = (listOfSlabJointCornerId.Count - 1);           

                ObjectId prvsSlabJointCornerId = listOfSlabJointCornerId[prvsDataIndex];
                Point3d prvsBasePoint = listOfAllSlabJointCornerBasePoint[prvsDataIndex];
                Point3d prvsNrstOfBasePointCorner = listOfAllSlabJointCornerNrstBasePoint[prvsDataIndex];
               
                var prvsCornerLayerName = AEE_Utility.GetLayerName(prvsSlabJointCornerId);
                var crrntCornerLayerName = AEE_Utility.GetLayerName(crrntSlabJointCornerId);
                if (crrntCornerLayerName == CommonModule.externalCornerLyrName || prvsCornerLayerName == CommonModule.externalCornerLyrName)
                {
                    if (crrntCornerLayerName == CommonModule.externalCornerLyrName)
                    {
                        setCornerText(crrntSlabJointCornerId, CommonModule.externalCorner);
                    }
                    else
                    {
                        setCornerText(crrntSlabJointCornerId, slabJointCornerFlange);
                    }

                    if (prvsCornerLayerName == CommonModule.externalCornerLyrName)
                    {
                        setCornerText(prvsSlabJointCornerId, CommonModule.externalCorner);
                    }
                    else
                    {
                        setCornerText(prvsSlabJointCornerId, slabJointCornerFlange);
                    }
                    setSlabJointWallPanelLine(wallEnt, xDataRegAppName, crrntSlabJointCornerId, basePoint, nrstOfBasePointCorner, prvsSlabJointCornerId, prvsBasePoint, prvsNrstOfBasePointCorner, stretchLineAngle, panelDepth, slabJointHeight, flagOfSlabJoint, elev);
                }
                else
                {
                    if (listOfSlabJointCornerId_In_ConcaveAngle.Contains(crrntSlabJointCornerId))
                    {
                        // corner angle is concave, add the panel depth value in line length.
                        stretchLineLength = stretchLineLength + panelDepth;
                    }

                    if (listOfSlabJointCornerId_In_ConcaveAngle.Contains(prvsSlabJointCornerId))
                    {
                        // corner angle is concave, add the panel depth value in line length.
                        stretchLineLength = stretchLineLength + panelDepth;
                    }

                    bool flagOfWallPanelCreate = false;
                    List<double> listOfSlabJointCornerFlange = kickerHlp.getCornerFlange(stretchLineLength, panelDepth, out flagOfWallPanelCreate);
                    double stretchValue = listOfSlabJointCornerFlange[0] - slabJointCornerFlange;

                    //Changes made on 12/07/2023 by SDM
                    double convex_offst = 0;
                    if (AEE_Utility.CustType == eCustType.WNPanel && listOfSlabJointCornerId_In_ConcaveAngle.Contains(crrntSlabJointCornerId))
                        convex_offst = panelDepth;

                    List<int> listOfCrrntVerticePointIndex_ForStretch = new List<int>();
                    List<List<Point3d>> listOfListSlabJointCornerLinePoint = kickerHlp.getStretchLinePoint(stretchLineAngle, crrntSlabJointCornerId, basePoint, nrstOfBasePointCorner, panelDepth, out listOfCrrntVerticePointIndex_ForStretch);

                    kickerHlp.stretch(crrntSlabJointCornerId, listOfCrrntVerticePointIndex_ForStretch, listOfListSlabJointCornerLinePoint, stretchValue+convex_offst, slabJointCornerFlange);
                    double SlabJointCrnrFlang = slabJointCornerFlange + stretchValue;
                    setCornerText(crrntSlabJointCornerId, SlabJointCrnrFlang);

                    stretchValue = listOfSlabJointCornerFlange[1] - slabJointCornerFlange;
                    List<int> listOfPrvsVerticePointIndex_ForStretch = new List<int>();
                    List<List<Point3d>> listOfListPrvsSlabJointCornerLinePoint = kickerHlp.getStretchLinePoint(stretchLineAngle, prvsSlabJointCornerId, prvsBasePoint, prvsNrstOfBasePointCorner, panelDepth, out listOfPrvsVerticePointIndex_ForStretch);

                    //Changes made on 12/07/2023 by SDM
                    convex_offst = 0;
                    if (AEE_Utility.CustType == eCustType.WNPanel && listOfSlabJointCornerId_In_ConcaveAngle.Contains(prvsSlabJointCornerId))
                        convex_offst = panelDepth;
                    
                    kickerHlp.stretch(prvsSlabJointCornerId, listOfPrvsVerticePointIndex_ForStretch, listOfListPrvsSlabJointCornerLinePoint, stretchValue+convex_offst, slabJointCornerFlange);

                    SlabJointCrnrFlang = slabJointCornerFlange + stretchValue;
                    setCornerText(prvsSlabJointCornerId, SlabJointCrnrFlang);

                    setSlabJointWallLine(xDataRegAppName, crrntSlabJointCornerId, listOfCrrntVerticePointIndex_ForStretch, prvsSlabJointCornerId, listOfPrvsVerticePointIndex_ForStretch, flagOfWallPanelCreate, wallEnt, slabJointHeight, flagOfSlabJoint, elev);
                }
            }
        }

        private void setSlabJointWallLine(string xDataRegAppName, ObjectId crrntSlabJointCornerId, List<int> listOfCrrntVerticePointIndex_ForStretch, ObjectId prvsSlabJointCornerId, List<int> listOfPrvsVerticePointIndex_ForStretch, 
                                          bool flagOfWallPanelCreate, Entity wallEnt, double slabJointHeight, bool flagOfSlabJoint, double elev)
        {
            if (flagOfWallPanelCreate == true)
            {
                if (listOfCrrntVerticePointIndex_ForStretch.Count == 2 && listOfPrvsVerticePointIndex_ForStretch.Count == 2)
                {
                    var listOfCrrntSlabJointVertices = AEE_Utility.GetPolylineVertexPoint(crrntSlabJointCornerId);
                    var listOfPrvsSlabJointVertices = AEE_Utility.GetPolylineVertexPoint(prvsSlabJointCornerId);

                    Point2d slabJointPoint1 = listOfCrrntSlabJointVertices[listOfCrrntVerticePointIndex_ForStretch[0]];
                    Point2d slabJointPoint2 = listOfCrrntSlabJointVertices[listOfCrrntVerticePointIndex_ForStretch[1]];

                    Point2d prvsSlabJointPoint1 = listOfPrvsSlabJointVertices[listOfPrvsVerticePointIndex_ForStretch[0]];
                    Point2d prvsSlabJointPoint2 = listOfPrvsSlabJointVertices[listOfPrvsVerticePointIndex_ForStretch[1]];

                    var baseWallLineId = AEE_Utility.getLineId(wallEnt, slabJointPoint1.X, slabJointPoint1.Y, 0, prvsSlabJointPoint1.X, prvsSlabJointPoint1.Y, 0, false);

                    var baseWallOffsetLineId = AEE_Utility.getLineId(wallEnt, slabJointPoint2.X, slabJointPoint2.Y, 0, prvsSlabJointPoint2.X, prvsSlabJointPoint2.Y, 0, false);

                    var listOfInsct = AEE_Utility.InterSectionBetweenTwoEntity(baseWallLineId, baseWallOffsetLineId);
                    if (listOfInsct.Count != 0)
                    {
                        AEE_Utility.deleteEntity(baseWallLineId);
                        AEE_Utility.deleteEntity(baseWallOffsetLineId);

                        baseWallLineId = AEE_Utility.getLineId(wallEnt, slabJointPoint1.X, slabJointPoint1.Y, 0, prvsSlabJointPoint2.X, prvsSlabJointPoint2.Y, 0, false);

                        baseWallOffsetLineId = AEE_Utility.getLineId(wallEnt, slabJointPoint2.X, slabJointPoint2.Y, 0, prvsSlabJointPoint1.X, prvsSlabJointPoint1.Y, 0, false);
                    }

                    AEE_Utility.AttachXData(baseWallLineId, xDataRegAppName, CommonModule.xDataAsciiName);

                    SlabJointHelper.listOfSlabJointWallLineId.Add(baseWallLineId);
                    SlabJointHelper.listOfSlabJointOffsetWallLineId.Add(baseWallOffsetLineId);
                    SlabJointHelper.listOfSlabJointHeight.Add(slabJointHeight);
                    SlabJointHelper.listOfSlabJointElev.Add(elev);
                }
            }
        }

        private void setSlabJointWallPanelLine(Entity wallEnt, string xDataRegAppName, ObjectId crrntSlabJointCornerId, Point3d crrntSlabJointCrnrBasePoint, Point3d crrntSlabJointCrnrNearstOfBasePoint, ObjectId prvsSlabJointCornerId, 
                                               Point3d prvsSlabJointCrnrBasePoint, Point3d prvsSlabJointCrnrNearstOfBasePoint, double stretchLineAngle, double panelDepth, double slabJointHeight, bool flagOfSlabJoint, double elev)
        {
            KickerHelper kickerHlp = new KickerHelper();
            var prvsCornerLayerName = AEE_Utility.GetLayerName(prvsSlabJointCornerId);
            var crrntCornerLayerName = AEE_Utility.GetLayerName(crrntSlabJointCornerId);

            ObjectId slabJointWallLineId = new ObjectId();
            ObjectId slabJointOffsetWallLineId = new ObjectId();

            if (crrntCornerLayerName == CommonModule.externalCornerLyrName && prvsCornerLayerName == CommonModule.externalCornerLyrName)
            {
                slabJointWallLineId = AEE_Utility.getLineId(wallEnt, crrntSlabJointCrnrBasePoint, prvsSlabJointCrnrBasePoint, false);
            }
            else
            {
                List<int> listOfVerticePointIndex_ForStretch = new List<int>();
                List<List<Point3d>> listOfListSlabJointCornerLinePoint = new List<List<Point3d>>();
                List<Point2d> listOfSlabJointVertice = new List<Point2d>();
                Point3d basePointOfCorner = new Point3d();
                if (crrntCornerLayerName != CommonModule.externalCornerLyrName)
                {
                    listOfListSlabJointCornerLinePoint = kickerHlp.getStretchLinePoint(stretchLineAngle, crrntSlabJointCornerId, crrntSlabJointCrnrBasePoint, crrntSlabJointCrnrNearstOfBasePoint, panelDepth, out listOfVerticePointIndex_ForStretch);
                    listOfSlabJointVertice = AEE_Utility.GetPolylineVertexPoint(crrntSlabJointCornerId);
                    basePointOfCorner = prvsSlabJointCrnrBasePoint;
                }
                else if (prvsCornerLayerName != CommonModule.externalCornerLyrName)
                {
                    listOfListSlabJointCornerLinePoint = kickerHlp.getStretchLinePoint(stretchLineAngle, prvsSlabJointCornerId, prvsSlabJointCrnrBasePoint, prvsSlabJointCrnrNearstOfBasePoint, panelDepth, out listOfVerticePointIndex_ForStretch);
                    listOfSlabJointVertice = AEE_Utility.GetPolylineVertexPoint(prvsSlabJointCornerId);
                    basePointOfCorner = crrntSlabJointCrnrBasePoint;
                }

                Point2d slabJointPoint1 = listOfSlabJointVertice[listOfVerticePointIndex_ForStretch[0]];
                Point2d slabJointPoint2 = listOfSlabJointVertice[listOfVerticePointIndex_ForStretch[1]];

                double length1 = AEE_Utility.GetLengthOfLine(slabJointPoint1.X, slabJointPoint1.Y, 0, basePointOfCorner.X, basePointOfCorner.Y, 0);
                double length2 = AEE_Utility.GetLengthOfLine(slabJointPoint2.X, slabJointPoint2.Y, 0, basePointOfCorner.X, basePointOfCorner.Y, 0);
                if (length1 > length2)
                {
                    slabJointWallLineId = AEE_Utility.getLineId(wallEnt, slabJointPoint2.X, slabJointPoint2.Y, 0, basePointOfCorner.X, basePointOfCorner.Y, 0, false);
                }
                else
                {
                    slabJointWallLineId = AEE_Utility.getLineId(wallEnt, slabJointPoint1.X, slabJointPoint1.Y, 0, basePointOfCorner.X, basePointOfCorner.Y, 0, false);
                }
            }

            AEE_Utility.AttachXData(slabJointWallLineId, xDataRegAppName, CommonModule.xDataAsciiName);
            /*AEE_Utility.AttachXData(slabJointWallLineId, xDataRegAppName + "_1", slabJointCornerFlange.ToString() + "," +
                                                                                 panelDepth.ToString());*/

            var angleOfLine = AEE_Utility.GetAngleOfLine(slabJointWallLineId);
            var point = AEE_Utility.get_XY(angleOfLine, 50);
            var listOfSlabJointLineStrtEndPoint = AEE_Utility.GetStartEndPointOfLine(slabJointWallLineId);

            Point3d strtPointSlabJointLine = listOfSlabJointLineStrtEndPoint[0];
            Point3d endPointSlabJointLine = listOfSlabJointLineStrtEndPoint[1];
            strtPointSlabJointLine = new Point3d((strtPointSlabJointLine.X - point.X), (strtPointSlabJointLine.Y - point.Y), 0);
            endPointSlabJointLine = new Point3d((endPointSlabJointLine.X + point.X), (endPointSlabJointLine.Y + point.Y), 0);

            var checkLineId = AEE_Utility.getLineId(strtPointSlabJointLine, endPointSlabJointLine, false);
            var offsetLineId = AEE_Utility.OffsetLine(checkLineId, WindowHelper.windowOffsetValue, false);
            var listOfCrrntCornrInsct = AEE_Utility.InterSectionBetweenTwoEntity(offsetLineId, crrntSlabJointCornerId);
            var listOfCrrntPrvsInsct = AEE_Utility.InterSectionBetweenTwoEntity(offsetLineId, prvsSlabJointCornerId);
            AEE_Utility.deleteEntity(checkLineId);
            AEE_Utility.deleteEntity(offsetLineId);
            if (listOfCrrntCornrInsct.Count != 0 || listOfCrrntPrvsInsct.Count != 0)
            {
                slabJointOffsetWallLineId = AEE_Utility.OffsetLine(slabJointWallLineId, panelDepth, false);
            }
            else
            {
                slabJointOffsetWallLineId = AEE_Utility.OffsetLine(slabJointWallLineId, -panelDepth, false);
            }

            listOfSlabJointWallLineId.Add(slabJointWallLineId);
            listOfSlabJointOffsetWallLineId.Add(slabJointOffsetWallLineId);
            listOfSlabJointHeight.Add(slabJointHeight);
            listOfSlabJointElev.Add(elev);
        }


        public void setCornerText(ObjectId slabJointCornerId, double slabJointCrnrFlang)
        {
            string slabJointCornerId_Str = Convert.ToString(slabJointCornerId);
            int indexOfExist = listOfSlabJointCornerId_ForText.IndexOf(slabJointCornerId);

            if (indexOfExist != -1)
            {
                string prvsData = listOfSlabJointCornerFlanges_ForText[indexOfExist];
                string newItem = prvsData + "," + Convert.ToString(slabJointCrnrFlang);
                listOfSlabJointCornerFlanges_ForText[listOfSlabJointCornerFlanges_ForText.FindIndex(ind => ind.Equals(prvsData))] = newItem;
            }
            else
            {
                listOfSlabJointCornerId_ForText.Add(slabJointCornerId);

                string slabJointData = slabJointCornerId_Str + "," + Convert.ToString(slabJointCrnrFlang);
                listOfSlabJointCornerFlanges_ForText.Add(slabJointData);
            }
        }


        public void createTextInSlabJointCorner(double slabJointHeight, List<ObjectId> listOfSlabJointCornerId, List<ObjectId> listOfAllSlabJointCornerLineId, List<ObjectId> listOfInternalWallLineId,
                                                List<double> listOfSlabJointRotationAngle, List<string> listOfSlabJointCornerFlanges_ForText, bool flagOfSlabJoint, double elev, List<string> listOfSlabJointCornerNrstBasePoint)
        {
            CornerHelper cornerHlp = new CornerHelper();
            CommonModule commonMod = new CommonModule();
            KickerHelper kickerHlp = new KickerHelper();
            for (int i = 0; i < listOfSlabJointCornerId.Count; i++)
            {
                ObjectId cornerLineId = listOfAllSlabJointCornerLineId[i];
                var cornerLine = AEE_Utility.GetEntityForRead(cornerLineId);
                string layerName = cornerLine.Layer;

                double cornerLineAngle = listOfSlabJointRotationAngle[i];
                //cornerLineAngle = cornerLineAngle + CommonModule.cornerRotationAngle;

                string slabJointCornerId_With_Flange = listOfSlabJointCornerFlanges_ForText[i];
                string[] array = slabJointCornerId_With_Flange.Split(',');
                if (array.Length == 3)
                {
                    double flange1 = Convert.ToDouble(array.GetValue(1));
                    double flange2 = Convert.ToDouble(array.GetValue(2));

                    ObjectId cornerId = listOfSlabJointCornerId_ForText[i];
                    ObjectId cornerId1 = listOfSlabJointCornerId_ForText[(i + 1) % listOfSlabJointCornerId.Count];

                    List<double> listOfFlange = cornerHlp.getCornerFlangeLength(cornerId);
                    if (listOfFlange.Count != 0)
                    {
                        flange1 = listOfFlange[0];
                        flange2 = listOfFlange[1];
                    }

                    double crnerRotationAngle = kickerHlp.getAngleOfCornerText(cornerId);
                    cornerLineAngle = crnerRotationAngle + CommonModule.cornerRotationAngle;

                    int indexOfCornerId = listOfSlabJointCornerId.IndexOf(cornerId);
                    int indexOfCornerId1 = listOfSlabJointCornerId.IndexOf(cornerId1);

                    if (indexOfCornerId != -1)
                    {
                        ObjectId cornerId_ForBOM = listOfSlabJointCornerId[indexOfCornerId];
                        ObjectId cornerId_ForBOM1 = listOfSlabJointCornerId[indexOfCornerId1];

                        double cornerHeight = slabJointHeight;

                        var cornerDescpt = CommonModule.slabJointCornerDescp;
                        var cornerType = getSJCornerText(cornerId);
                        var cornerLayerName = CommonModule.slabJointCornerLayerName;
                        var cornerTextLayerName = CommonModule.slabJointCornerTextLayerName;
                        var cornerLayerColor = CommonModule.slabJointCornerLayerColor;

                        string cornerText = CornerHelper.getCornerText(cornerType, flange1, flange2, cornerHeight);
                        string xDataRegAppName = AEE_Utility.GetXDataRegisterAppName(cornerId_ForBOM);
                        string xDataRegAppName1 = AEE_Utility.GetXDataRegisterAppName(cornerId_ForBOM1);

                        var cornerTextId = cornerHlp.writeTextInCorner(cornerId, cornerText, cornerLineAngle, cornerTextLayerName, cornerLayerColor);
                        listOfSlabJointCornerTextId.Add(cornerTextId);

                        var listOfCornerVertexPoint = AEE_Utility.GetPolylineVertexPoint(cornerId);
                        string nrstOfBasePointCorner = listOfSlabJointCornerNrstBasePoint.Where(x => x.Split('|')[0] == cornerId.ToString()).Select(y => y).First();

                        string[] val = nrstOfBasePointCorner.Split('|')[1].Split(',').ToArray();
                        Point3d basepnt = new Point3d(Convert.ToDouble(val[0]), Convert.ToDouble(val[1]), Convert.ToDouble(val[2]));

                        double minX = listOfCornerVertexPoint.Min(point => point.X);
                        double maxX = listOfCornerVertexPoint.Max(point => point.X);
                        double minY = listOfCornerVertexPoint.Min(point => point.Y);
                        double maxY = listOfCornerVertexPoint.Max(point => point.Y);
                        double center_X = minX + ((maxX - minX) / 2);
                        double center_Y = minY + ((maxY - minY) / 2);


                        slapjoint_corner_CLine corner = new slapjoint_corner_CLine();
                        corner.basepnt = basepnt;
                        corner.wallname = xDataRegAppName;
                        corner.center_X = center_X;
                        corner.center_Y = center_Y;
                        corner.flange1 = flange1;
                        corner.flange2 = flange2;

                        CommonModule.slapjoint_corner_CLine.Add(corner);



                        int nextno = (indexOfCornerId + 1) % listOfSlabJointCornerId.Count;
                        int len_no = int.Parse(xDataRegAppName.Split('_')[0].Replace("W", "")) - 1;
                        string next_regappname = AEE_Utility.GetXDataRegisterAppName(listOfSlabJointCornerId[nextno]);
                        //double len = AEE_Utility.GetLengthOfLine(listOfInternalWallLineId[len_no]);
                        Line ln = AEE_Utility.GetLine(listOfInternalWallLineId[len_no]);
                        double len1 = Math.Max(ln.StartPoint.DistanceTo(ln.GetClosestPointTo(listOfSlabJointCornerBasePoint[indexOfCornerId], false)),
                                              ln.EndPoint.DistanceTo(ln.GetClosestPointTo(listOfSlabJointCornerBasePoint[indexOfCornerId], false)));
                        Line ln1 = AEE_Utility.GetLine(listOfInternalWallLineId[(len_no + 1) % listOfInternalWallLineId.Count]);
                        double len2 = Math.Min(ln1.StartPoint.DistanceTo(ln1.GetClosestPointTo(listOfSlabJointCornerBasePoint[indexOfCornerId], false)),
                                               ln1.EndPoint.DistanceTo(ln1.GetClosestPointTo(listOfSlabJointCornerBasePoint[indexOfCornerId], false)));



                        CornerElevation oElev = new CornerElevation(xDataRegAppName, next_regappname, len1 - flange1, len2, elev);
                        CornerHelper.setCornerDataForBOM(xDataRegAppName, cornerId, cornerTextId, cornerText, cornerDescpt, cornerType, flange1, flange2, cornerHeight, oElev);
                        AEE_Utility.changeLayer(cornerId, cornerLayerName, cornerLayerColor);
                    }
                }
            }
        }

        public static string getSJCornerText(ObjectId idCorner)
        {
            if (!listOfSlabJointCornerId_In_ConcaveAngle.Contains(idCorner))
            {
                return CommonModule.slabJointCornerConcaveText;
            }
            return CommonModule.slabJointCornerConvexText;
        }


        public void setSlabJointBeamData_ForCorner(ObjectId nearestWallToBeamWallId, double distBtwWallToBeam, ObjectId prvsWallLineId)
        {
            nearestWallToBeamWallId = AEE_Utility.createColonEntityInSamePoint(nearestWallToBeamWallId, false);
            prvsWallLineId = AEE_Utility.createColonEntityInSamePoint(prvsWallLineId, false);
            AEE_Utility.MoveEntity(nearestWallToBeamWallId, CreateShellPlanHelper.moveVector_ForSlabJointLayout);
            AEE_Utility.MoveEntity(prvsWallLineId, CreateShellPlanHelper.moveVector_ForSlabJointLayout);

            listOfNearestBeamLineId_ForSlabJoint.Add(nearestWallToBeamWallId);
            listOfDistBtwBeamToWall_ForSlabJoint.Add(distBtwWallToBeam);
            listOfWallId_ForSlabJoint.Add(prvsWallLineId);
        }


        public void moveSlabJointCornerInBeamLine(List<ObjectId> listOfInternalWallLineId, ObjectId wallId, double elev)
        {
            BeamHelper beamHlp = new BeamHelper();
            KickerHelper kickerHlp = new KickerHelper();
            for (int index = 0; index < listOfNearestBeamLineId_ForSlabJoint.Count; index++)
            {
                ObjectId nearestBeamLineId = listOfNearestBeamLineId_ForSlabJoint[index];
                if (nearestBeamLineId.IsValid == true)
                {
                    double distBtwBeamToWall = listOfDistBtwBeamToWall_ForSlabJoint[index];
                    ObjectId wallId_ForSlabJoint = listOfWallId_ForSlabJoint[index];

                    for (int j = 0; j < listOfSlabJointCornerId.Count; j++)
                    {
                        ObjectId slabJointCrnrId = listOfSlabJointCornerId[j];
                        var listOfInsctPoint = AEE_Utility.InterSectionBetweenTwoEntity(slabJointCrnrId, wallId_ForSlabJoint);
                        if (listOfInsctPoint.Count != 0)
                        {
                            bool flagOfMove = false;
                            var moveVectorOfBeamLine = beamHlp.getMoveVectorOfBeamWallLine(wallId_ForSlabJoint, nearestBeamLineId, distBtwBeamToWall, out flagOfMove);
                            AEE_Utility.MoveEntity(slabJointCrnrId, moveVectorOfBeamLine);
                            var basePoint = AEE_Utility.GetBasePointOfPolyline(slabJointCrnrId);
                            Point3d nrstOfBasePointCorner = kickerHlp.getNearestPointOfKickerCorner(slabJointCrnrId, basePoint);
                            listOfSlabJointCornerBasePoint[j] = basePoint;
                            listOfSlabJointCornerNrstBasePoint[j] = nrstOfBasePointCorner;
                        }
                    }
                }
            }

            drawSlabCornerAfterCornerMoveInBeamLine(listOfInternalWallLineId, wallId, elev);
        }


        private void drawSlabCornerAfterCornerMoveInBeamLine(List<ObjectId> listOfInternalWallLineId, ObjectId wallId, double elev)
        {
            bool flagOfSlabJoint = true;
            double panelDepth = CommonModule.slabJointPanelDepth;
            double slabJointHeight = PanelLayout_UI.SL;
            double slabJointCornerFlange = CommonModule.minKickerCornrFlange;

            var wallEnt = AEE_Utility.GetEntityForRead(wallId);
            ObjectId lastWallLineId = listOfInternalWallLineId[(listOfInternalWallLineId.Count - 1)];

            List<Point3d> listOfSlabCornerBasePoint = new List<Point3d>();
            List<Point3d> listOfSlabCornerNrstBasePoint = new List<Point3d>();
            List<ObjectId> listOfSlabCornerId = new List<ObjectId>();
            List<ObjectId> listOfSlabCornerLineId = new List<ObjectId>();
            List<double> listOfSlabRotationAngle = new List<double>();
            List<string> listOfsjcline = new List<string>();
            for (int k = 0; k < listOfSlabJointCornerBasePoint.Count; k++)
            {
                listOfSlabCornerBasePoint.Add(listOfSlabJointCornerBasePoint[k]);
                listOfSlabCornerNrstBasePoint.Add(listOfSlabJointCornerNrstBasePoint[k]);
                listOfSlabCornerId.Add(listOfSlabJointCornerId[k]);
                listOfSlabCornerLineId.Add(listOfSlabJointCornerLineId[k]);
                listOfSlabRotationAngle.Add(listOfSlabJointRotationAngle[k]);
            }

            listOfSlabJointCornerBasePoint.Clear();
            listOfSlabJointCornerNrstBasePoint.Clear();
            listOfSlabJointCornerId.Clear();
            listOfSlabJointCornerLineId.Clear();
            listOfSlabJointRotationAngle.Clear();


            for (int index = 0; index < listOfSlabCornerLineId.Count; index++)
            {
                ObjectId slabJointCornerId = listOfSlabCornerId[index];
                ObjectId crrntLineId = listOfSlabCornerLineId[index];
                double crrntLineAngle = AEE_Utility.GetAngleOfLine(crrntLineId);
                double crrntLineLngth = AEE_Utility.GetLengthOfLine(crrntLineId);

                double rotationAngle = listOfSlabRotationAngle[index];
                Point3d basePoint = listOfSlabCornerBasePoint[index];
                Point3d nrstOfBasePointCorner = listOfSlabCornerNrstBasePoint[index];
                listOfsjcline.Add(slabJointCornerId.ToString() + "|" + nrstOfBasePointCorner.X.ToString() + "," + nrstOfBasePointCorner.Y.ToString() + "," + nrstOfBasePointCorner.Z.ToString());

                ObjectId prvsLineId = new ObjectId();
                if (index == 0)
                {
                    prvsLineId = listOfSlabCornerLineId[(listOfSlabCornerLineId.Count - 1)];
                }
                else
                {
                    prvsLineId = listOfSlabCornerLineId[(index - 1)];
                }

                double prvsLineAngle = AEE_Utility.GetAngleOfLine(prvsLineId);
                double prvsLineLngth = AEE_Utility.GetLengthOfLine(prvsLineId);

                KickerHelper kickerHlp = new KickerHelper();

                var xDataRegAppName = AEE_Utility.GetXDataRegisterAppName(slabJointCornerId);

                stretchSlabJointCorner(xDataRegAppName, listOfSlabJointCornerId, listOfSlabJointCornerBasePoint, listOfSlabJointCornerNrstBasePoint, slabJointCornerId, basePoint, nrstOfBasePointCorner, prvsLineAngle, prvsLineLngth, slabJointCornerFlange, wallEnt, slabJointHeight, panelDepth, flagOfSlabJoint, elev);

                AEE_Utility.AttachXData(slabJointCornerId, xDataRegAppName, CommonModule.xDataAsciiName);

                listOfSlabJointCornerBasePoint.Add(basePoint);
                listOfSlabJointCornerNrstBasePoint.Add(nrstOfBasePointCorner);
                listOfSlabJointCornerId.Add(slabJointCornerId);
                listOfSlabJointCornerLineId.Add(crrntLineId);
                listOfSlabJointRotationAngle.Add(rotationAngle);

                setDeckPanelWallPoint(slabJointCornerId, basePoint, nrstOfBasePointCorner);
                if (index == (listOfInternalWallLineId.Count - 1))
                {
                    var last_XDataRegAppName = AEE_Utility.GetXDataRegisterAppName(lastWallLineId);

                    ObjectId firstLineId = listOfInternalWallLineId[0];
                    double firstLineAngle = AEE_Utility.GetAngleOfLine(firstLineId);
                    double firstLineLngth = AEE_Utility.GetLengthOfLine(firstLineId);

                    ObjectId firstSlabJointCornerId = listOfSlabCornerId[0];
                    Point3d firstBasePoint = listOfSlabJointCornerBasePoint[0];
                    Point3d firstNrstOfBasePointCorner = listOfSlabJointCornerNrstBasePoint[0];
                    stretchSlabJointCorner(last_XDataRegAppName, listOfSlabCornerId, listOfSlabJointCornerBasePoint, listOfSlabJointCornerNrstBasePoint, firstSlabJointCornerId, firstBasePoint, firstNrstOfBasePointCorner, crrntLineAngle, crrntLineLngth, slabJointCornerFlange, wallEnt, slabJointHeight, panelDepth, flagOfSlabJoint, elev);

                    createTextInSlabJointCorner(slabJointHeight, listOfSlabJointCornerId, listOfSlabJointCornerLineId, listOfInternalWallLineId, listOfSlabJointRotationAngle, listOfSlabJointCornerFlanges_ForText, flagOfSlabJoint, elev, listOfsjcline);

                    var deckPanelWallId = AEE_Utility.createRectangle(listOfDeckPanelWallPoint, wallEnt.Layer, wallEnt.ColorIndex);
                    xDataRegAppName = AEE_Utility.GetXDataRegisterAppName(wallId);
                    AEE_Utility.AttachXData(deckPanelWallId, xDataRegAppName, CommonModule.xDataAsciiName);

                    DeckPanelHelper deckPanelHlp = new DeckPanelHelper();
                    deckPanelHlp.setDeckPanelWall(deckPanelWallId, panelDepth);



                }
            }
        }


        private void setDeckPanelWallPoint(ObjectId cornerId, Point3d basePointCrnr, Point3d nearestBasePointCrnr)
        {
            if (listOfSlabJointCornerId_In_ConcaveAngle.Contains(cornerId))
            {
                listOfDeckPanelWallPoint.Add(basePointCrnr);
            }
            else
            {
                listOfDeckPanelWallPoint.Add(nearestBasePointCrnr);
            }
        }


        public void drawSlabJointWallPanel(ProgressForm progressForm, string wallCreationMsg)
        {
            CommonModule commonModule = new CommonModule();
            WindowHelper windowHlp = new WindowHelper();
            WallPanelHelper wallPanelHlp = new WallPanelHelper();
            SunkanSlabHelper internalSunkanSlabHlp = new SunkanSlabHelper();
            List<double> lstpnts = new List<double>();
            CommonModule.slapjoint_CLine = new List<slapjoint_CLine>();

            for (int index = 0; index < listOfSlabJointWallLineId.Count; index++)
            {
                if ((index % 50) == 0)
                {
                    progressForm.ReportProgress(1, wallCreationMsg);
                }

                ObjectId wallLineId = listOfSlabJointWallLineId[index];
                ObjectId offsetWallLineId = listOfSlabJointOffsetWallLineId[index];

                var xDataRegAppName = AEE_Utility.GetXDataRegisterAppName(wallLineId);

                double slabJointHeight = listOfSlabJointHeight[index];

                string wallText = PanelLayout_UI.slabJointWallName;
                string wallItemCode = PanelLayout_UI.slabJointWallName;
                Vector3d moveVector = CreateShellPlanHelper.moveVector_ForSlabJointLayout;

                string wallPanelLayerName = CommonModule.slabJointCornerLayerName;
                string wallPanelTextLayerName = CommonModule.slabJointWallPanelTextLayerName;
                int wallPanelLayerColor = CommonModule.slabJointWallPanelLayerColor;
                string wallType = CommonModule.slabJointPanelType;

                var wallPanelLine = AEE_Utility.GetLine(wallLineId);
                var listOfWallPanelLineStrtEndPoint = commonModule.getStartEndPointOfLine(wallPanelLine);
                Point3d wallPanelLineStrtPoint = listOfWallPanelLineStrtEndPoint[0];
                Point3d wallPanelLineEndPoint = listOfWallPanelLineStrtEndPoint[1];

                double wallPanelLength = AEE_Utility.GetLengthOfLine(wallPanelLineStrtPoint, wallPanelLineEndPoint);
                wallPanelLength = Math.Round(wallPanelLength);
                double wallPanelLineAngle = AEE_Utility.GetAngleOfLine(wallPanelLineStrtPoint, wallPanelLineEndPoint);
                double elev = listOfSlabJointElev[index];

                if (wallPanelLength != 0)
                {
                    var wallPanelOffsetLine = AEE_Utility.GetLine(offsetWallLineId);
                    var listOfWallPanelOffsetLineStrtEndPoint = commonModule.getStartEndPointOfLine(wallPanelOffsetLine);
                    Point3d wallPanelOffstLineStrtPoint = listOfWallPanelOffsetLineStrtEndPoint[0];
                    Point3d wallPanelOffstLineEndPoint = listOfWallPanelOffsetLineStrtEndPoint[1];

                    bool flag_X_Axis = false;
                    bool flag_Y_Axis = false;
                    commonModule.checkAngleOfLine_Axis(wallPanelLineAngle, out flag_X_Axis, out flag_Y_Axis);

                    List<double> listOfWallPanelLength = wallPanelHlp.getListOfWallPanelLength(wallPanelLength, PanelLayout_UI.maxSJWidthOfPanel, PanelLayout_UI.minSJWidthOfPanel);

                    Point3d startPoint = wallPanelLineStrtPoint;
                    Point3d offsetStartPoint = wallPanelOffstLineStrtPoint;

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

                            string sunkanSlabPanelText = DeckPanelHelper.getWallPanelText(wallPnlLngth, wallText, slabJointHeight);
                            string wallDescp = sunkanSlabPanelText;

                            ObjectId dimTextId2 = wallPanelHlp.writeDimensionTextInWallPanel(sunkanSlabPanelText, wallLineId, dimTextPoint, wallPanelLineAngle, wallPanelTextLayerName, wallPanelLayerColor);
                            listOfMoveId.Add(dimTextId2);
                            wallPanelHlp.setBOMDataOfWallPanel(dimTextId2, wallPanelRect_Id, wallPanelLine_Id /*wallLineId*/, wallPnlLngth, slabJointHeight, 0, wallItemCode, wallDescp, wallType, elev.ToString());

                            var circleId = wallPanelHlp.createCircleInNonStandardWallPanel(dimTextPoint.X, dimTextPoint.Y, wallPnlLngth, PanelLayout_UI.minSJWidthOfPanel);
                            if (circleId.IsValid == true)
                            {
                                listOfMoveId.Add(circleId);
                            }
                            ////AEE_Utility.MoveEntity(listOfMoveId, moveVector);
                            slapjoint_CLine cline = new slapjoint_CLine();
                            cline.wallname = xDataRegAppName;
                            cline.x_axis = flag_X_Axis;
                            cline.length = length;
                            cline.offsetpoint = offsetStartPoint;
                            cline.startpoint = startPoint;
                            cline.movevector = moveVector;
                            cline.wallPanelLineAngle = wallPanelLineAngle;
                            cline.listOfWallPanelLength = listOfWallPanelLength;

                            CommonModule.slapjoint_CLine.Add(cline);

                        }

                        startPoint = endPoint;
                        offsetStartPoint = offstEndPoint;
                    }
                }
            }
        }
    }
}
