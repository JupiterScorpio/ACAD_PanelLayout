using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using PanelLayout_App.CivilModel;
using PanelLayout_App.WallModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static PanelLayout_App.RotationHelper;


namespace PanelLayout_App.WallModule
{
    public class InternalWallHelper
    {
        public static int nonStandardColorIndex = 1;
        public static List<ObjectId> listOfInternalWallObjId = new List<ObjectId>();
        public static List<List<ObjectId>> listOfListOfInternalWallRoomLineId_With_WallName = new List<List<ObjectId>>();
        public static List<ObjectId> listOfNonPerpendiculareLine = new List<ObjectId>();

        private static List<Point3d> listOfInternalWallCorner_Points_EvenNo = new List<Point3d>();
        private static List<ObjectId> listOfInternalWallCorner_ObjId_EvenNo = new List<ObjectId>();
        private static List<List<ObjectId>> listOfInternalWallCornerText_ObjId_EvenNo = new List<List<ObjectId>>();

        private static List<Point3d> listOfInternalWallCorner_Points_OddNo = new List<Point3d>();
        private static List<ObjectId> listOfInternalWallCorner_ObjId_OddNo = new List<ObjectId>();
        private static List<List<ObjectId>> listOfInternalWallCornerText_ObjId_OddNo = new List<List<ObjectId>>();

        private static List<Point3d> listOfAllInternalWallCorner_Points = new List<Point3d>();
        private static List<ObjectId> listOfAllInternalWallCorner_ObjId = new List<ObjectId>();
        private static List<List<ObjectId>> listOfAllInternalWallCornerText_ObjId = new List<List<ObjectId>>();

        public static List<ObjectId> listOfAllInternalWallCornerId = new List<ObjectId>();//By MT on 24/04/2022
        public static List<List<ObjectId>> listOfAllInternalWallCornerTextId = new List<List<ObjectId>>();//By MT on 24/04/2022

        public static List<ObjectId> listOfAllCornerId1_ForInsctDoor = new List<ObjectId>();
        public static List<ObjectId> listOfAllCornerId2_ForInsctDoor = new List<ObjectId>();
        public static List<ObjectId> listOfAllCornerId_ForDoor_And_Beam = new List<ObjectId>();

        public static List<ObjectId> listOfLinesBtwTwoCrners_ObjId = new List<ObjectId>();
        public static List<ObjectId> listOfOffsetLinesBtwTwoCrners_ObjId = new List<ObjectId>();
        public static List<double> listOfDistnceBtwTwoCrners = new List<double>();
        public static List<string> listOfDistnceAndObjId_BtwTwoCrners = new List<string>();

        public static List<CornerData> lstCornerData = new List<CornerData>();
        public static List<ObjectId> listOfDoorCornerId = new List<ObjectId>();//By MT on 29/04/2022
        public static List<List<ObjectId>> listOfDoorCornerTextId = new List<List<ObjectId>>();//By MT on 29/04/2022
        public static bool flagForDoorCorner = false;//By MT on 29/04/2022
        public static List<Point2d> listOfNearestCornerPoint = new List<Point2d>();//By MT on 29/04/2022

        public void callMethodInternalWall(ProgressForm progressForm, string cornerCreationMsg, List<List<LineSegment3d>> listDoorLines)
        {
            listOfDoorCornerTextId.Clear();//By MT on 29/04/2022
            listOfDoorCornerId.Clear();//By MT on 29/04/2022
            listOfNearestCornerPoint.Clear(); //By MT on 29/04/2022
            if (listOfInternalWallObjId.Count != 0)
            {
                flagForDoorCorner = true;//By MT on 29/04/2022
                drawInternalCorner_In_InternalWall(progressForm, cornerCreationMsg, listOfInternalWallObjId, listOfListOfInternalWallRoomLineId_With_WallName, listDoorLines);
            }
            flagForDoorCorner = false;//By MT on 29/04/2022
            if (DuctHelper.listOfDuctObjId.Count != 0)
            {
                drawInternalCorner_In_InternalWall(progressForm, cornerCreationMsg, DuctHelper.listOfDuctObjId, DuctHelper.listOfListOfDuctRoomLineId_With_WallName, listDoorLines);
            }

            if (StairCaseHelper.listOfStairCaseObjId.Count != 0)
            {
                drawInternalCorner_In_InternalWall(progressForm, cornerCreationMsg, StairCaseHelper.listOfStairCaseObjId, StairCaseHelper.listOfListOfStaircaseRoomLineId_With_WallName, listDoorLines);
            }

            if (LiftHelper.listOfLiftObjId.Count != 0)
            {
                drawInternalCorner_In_InternalWall(progressForm, cornerCreationMsg, LiftHelper.listOfLiftObjId, LiftHelper.listOfListOfLiftRoomLineId_With_WallName, listDoorLines);
            }
        }


        private void drawInternalCorner_In_InternalWall(ProgressForm progressForm, string cornerCreationMsg, List<ObjectId> listOfIntrnlWallObjId, List<List<ObjectId>> listOfListOfRoomLineId_With_WallName, List<List<LineSegment3d>> listDoorLines)
        {
            KickerHelper kickerHlp = new KickerHelper(listDoorLines);
            CommonModule common_Obj = new CommonModule();
            CornerHelper cornerHlpr = new CornerHelper();
            ExternalWallHelper externalWallHlp = new ExternalWallHelper();
            SunkanSlabHelper internalSunkanSlabHlp = new SunkanSlabHelper();
            SlabJointHelper slabJointHlp = new SlabJointHelper();
            listOfAllInternalWallCornerId = new List<ObjectId>();//By MT on 24/04/2022
            listOfAllInternalWallCornerTextId = new List<List<ObjectId>>();//By MT on 24/04/2022
            for (int index = 0; index < listOfIntrnlWallObjId.Count; index++)
            {
                int evenCount = 0;
                int oddCount = 0;
                listOfInternalWallCorner_ObjId_EvenNo.Clear();
                listOfInternalWallCornerText_ObjId_EvenNo.Clear();
                listOfInternalWallCorner_Points_EvenNo.Clear();
                listOfInternalWallCorner_Points_OddNo.Clear();
                listOfInternalWallCorner_ObjId_OddNo.Clear();
                listOfInternalWallCornerText_ObjId_OddNo.Clear();
                listOfAllInternalWallCorner_Points.Clear();
                listOfAllInternalWallCorner_ObjId.Clear();
                listOfAllInternalWallCornerText_ObjId.Clear();
                SlabJointHelper.listOfNearestBeamLineId_ForSlabJoint.Clear();
                SlabJointHelper.listOfDistBtwBeamToWall_ForSlabJoint.Clear();
                SlabJointHelper.listOfWallId_ForSlabJoint.Clear();

                ObjectId internalWall_Id = listOfIntrnlWallObjId[index];
                List<ObjectId> listOfInternalWallLineId = listOfListOfRoomLineId_With_WallName[index];

                ObjectId insideOffsetWallId = drawOffsetInternalWall(internalWall_Id, 1);
                ObjectId lastWallLineId_With_RoomName = new ObjectId();
                if (insideOffsetWallId.IsValid == true)
                {
                    var internalWallEnt = AEE_Utility.GetEntityForRead(internalWall_Id);
                    ObjectId sunkanSlabId = new ObjectId();

                    //Test Commented layer to be changed once again

                    bool flagOfSunkanSlab = internalSunkanSlabHlp.checkSunkanSlabIsAvailable_In_InternalWall(internalWall_Id, out sunkanSlabId);

                    SunkanSlabHelper.changeWallPanelLayer_ForExternalSunkanSlab(sunkanSlabId, internalWall_Id, insideOffsetWallId, listOfInternalWallLineId);

                    var outsideOffsetWallId = externalWallHlp.drawOffsetExternalWall(internalWall_Id);
                    var listOfOutsideOffsetWallLineId = AEE_Utility.ExplodeEntity(outsideOffsetWallId);

                    List<ObjectId> listOfInsideOffsetWallLineId = AEE_Utility.ExplodeEntity(insideOffsetWallId);

                    int listItemCount = listOfInternalWallLineId.Count;
                    for (int j = 0; j < listItemCount; j++)
                    {
                        ObjectId wallLineId_With_RoomName = getXDataRegisteredAppName_InternalWall(j, listOfInternalWallLineId);
                        var xDataRegAppName = AEE_Utility.GetXDataRegisterAppName(wallLineId_With_RoomName);

                        lastWallLineId_With_RoomName = listOfInternalWallLineId[listOfInternalWallLineId.Count - 1];

                        slabJointHlp.drawSlabJointCorner(j, wallLineId_With_RoomName, lastWallLineId_With_RoomName, internalWall_Id, listOfInternalWallLineId, insideOffsetWallId, listOfInsideOffsetWallLineId, outsideOffsetWallId, listOfOutsideOffsetWallLineId, sunkanSlabId);
                        kickerHlp.callMethodOfInternalWallKickerCorner(j, wallLineId_With_RoomName, lastWallLineId_With_RoomName, internalWall_Id, listOfInternalWallLineId, insideOffsetWallId, listOfInsideOffsetWallLineId, outsideOffsetWallId, listOfOutsideOffsetWallLineId, sunkanSlabId);

                        if ((j % 20) == 0)
                        {
                            progressForm.ReportProgress(1, cornerCreationMsg);
                        }

                        ObjectId lineId = listOfInternalWallLineId[j];

                        var listOfStrtEndPoint = AEE_Utility.getStartEndPointWithAngle_Line(lineId);
                        double intrnlCornr_StartX = listOfStrtEndPoint[0];
                        double intrnlCornr_StartY = listOfStrtEndPoint[1];
                        double angleOfLine = listOfStrtEndPoint[4];

                        var flagOfAngleBtw = common_Obj.checkAngleBetweenTwoLines(j, angleOfLine, listOfInternalWallLineId);
                        if (flagOfAngleBtw == true)
                        {
                            PartHelper partHelper = new PartHelper();

                            CommonModule.basePoint = new Point3d(intrnlCornr_StartX, intrnlCornr_StartY, 0);

                            bool flagOfInside = false;
                            double rotationAngle = 0;
                            common_Obj.checkInternalCornerIsInside_Innerwall(xDataRegAppName, internalWall_Id, CommonModule.basePoint, angleOfLine, insideOffsetWallId, out flagOfInside, out rotationAngle);
                            if (flagOfInside == true)
                            {
                                var intrnlCrnr_ObjId = partHelper.drawInternalCorner(xDataRegAppName, intrnlCornr_StartX, intrnlCornr_StartY, CommonModule.intrnlCornr_Flange1, CommonModule.intrnlCornr_Flange2, CommonModule.panelDepth, CommonModule.internalCornerThick, CommonModule.panelDepth, CommonModule.internalCornerLyrName, CommonModule.internalCornerLyrColor, rotationAngle);

                                var pts = AEE_Utility.GetPolyLinePointList(intrnlCrnr_ObjId);


                                AEE_Utility.AttachXData(intrnlCrnr_ObjId, xDataRegAppName, CommonModule.xDataAsciiName);
                                AEE_Utility.AttachXData(intrnlCrnr_ObjId, xDataRegAppName + "_1", CommonModule.intrnlCornr_Flange1.ToString() + "," +
                                                                                                  CommonModule.intrnlCornr_Flange2.ToString() + "," +
                                                                                                  CommonModule.panelDepth.ToString() + "," +
                                                                                                  CommonModule.internalCornerThick.ToString() + "," +
                                                                                                  CommonModule.panelDepth.ToString());


                                //CommonModule.rotateAngle = (rotationAngle * Math.PI) / 180; // variable assign for rotation             
                                List<ObjectId> listOfObjId = new List<ObjectId>();
                                listOfObjId.Add(intrnlCrnr_ObjId);
                                //MoveRorationScaleJig.RotationScaleJig_Method(listOfObjId);
                                CornerElevation oelev = new CornerElevation(xDataRegAppName, AEE_Utility.GetXDataRegisterAppName(lineId), AEE_Utility.GetLengthOfLine(wallLineId_With_RoomName), 0, 0);

                                var flagOfInternalCorner = common_Obj.checkInternalCornerIntersectTheTwoLines(j, intrnlCrnr_ObjId, listOfInsideOffsetWallLineId);
                                if (flagOfInternalCorner == false)
                                {
                                    AEE_Utility.deleteEntity(listOfObjId);
                                    drawExternalCorner_In_InternallWall(internalWallEnt.Layer, xDataRegAppName, internalWall_Id, CommonModule.basePoint, angleOfLine, insideOffsetWallId, j, listOfInsideOffsetWallLineId, sunkanSlabId);
                                }
                                else
                                {
                                    lstCornerData.Add(new CornerData(pts[1], pts[pts.Count - 3]) { idCorner = intrnlCrnr_ObjId, ptCorner = CommonModule.basePoint, angleOfLine = rotationAngle, Length1 = CommonModule.intrnlCornr_Flange1, Length2 = CommonModule.intrnlCornr_Flange2, });

                                    listOfInternalWallCorner_Points_EvenNo.Add(CommonModule.basePoint);
                                    listOfInternalWallCorner_ObjId_EvenNo.Add(intrnlCrnr_ObjId);

                                    listOfInternalWallCorner_Points_OddNo.Add(CommonModule.basePoint);
                                    listOfInternalWallCorner_ObjId_OddNo.Add(intrnlCrnr_ObjId);

                                    listOfAllInternalWallCorner_Points.Add(CommonModule.basePoint);
                                    listOfAllInternalWallCorner_ObjId.Add(intrnlCrnr_ObjId);

                                    double IC_CornerHght = IC_CornerHght = getCornerHeightOfInternallWall(internalWallEnt.Layer, sunkanSlabId);

                                    var lstTemp = new List<List<object>>();
                                    List<string> cornerTexts = CornerHelper.UpdateCornerText(lstTemp, IC_CornerHght, sunkanSlabId);
                                    double crnerRotationAngle = rotationAngle + CommonModule.cornerRotationAngle;
                                    var textId = cornerHlpr.writeMultipleTextInCorner(intrnlCrnr_ObjId, cornerTexts, crnerRotationAngle, CommonModule.internalCornerTextLyrName, CommonModule.internalCornerLyrColor);
                                    listOfInternalWallCornerText_ObjId_EvenNo.Add(textId);
                                    listOfInternalWallCornerText_ObjId_OddNo.Add(textId);
                                    listOfAllInternalWallCornerText_ObjId.Add(textId);

                                    //Fix elevation when wall Ht more than 3000 by SDM 2022-08-03
                                    //for (int i = 0; i < lstTemp.Count; i++)
                                    //{
                                    //    CornerHelper.setCornerDataForBOM(xDataRegAppName, intrnlCrnr_ObjId, textId[i], cornerTexts[i], CommonModule.internalCornerDescp, CornerHelper.getInternalCornerCode(), CommonModule.intrnlCornr_Flange1, CommonModule.intrnlCornr_Flange2, Convert.ToDouble(lstTemp[i][4]), oelev);
                                    //}
                                    double _elev = 0;//Fix elevation when wall Ht more than 3000 by SDM 2022-08-03
                                    for (int i = 0; i < lstTemp.Count; i++)
                                    {
                                        oelev.elev = _elev;//Fix elevation when wall Ht more than 3000 by SDM 2022-08-03
                                        CornerElevation elevation = new CornerElevation(xDataRegAppName, AEE_Utility.GetXDataRegisterAppName(lineId), AEE_Utility.GetLengthOfLine(wallLineId_With_RoomName), 0, _elev);//Fix elevation when wall Ht more than 3000 by SDM 2022-08-03

                                        CornerHelper.setCornerDataForBOM(xDataRegAppName, intrnlCrnr_ObjId, textId[i], cornerTexts[i], CommonModule.internalCornerDescp, CornerHelper.getInternalCornerCode(), CommonModule.intrnlCornr_Flange1, CommonModule.intrnlCornr_Flange2, Convert.ToDouble(lstTemp[i][4]), elevation);

                                        _elev += Convert.ToDouble(lstTemp[i][4]);//Fix elevation when wall Ht more than 3000 by SDM 2022-08-03
                                    }

                                    CheckandStretchInternalCornerWithWindowOrDoor(intrnlCrnr_ObjId, textId);

                                    cornerHlpr.setInternalCornerForStretch(intrnlCrnr_ObjId, textId);
                                }
                            }
                        }
                        else
                        {
                            var circleId = AEE_Utility.CreateCircle(intrnlCornr_StartX, intrnlCornr_StartY, 0, (CommonModule.intrnlCornr_Flange1 / 2), CommonModule.internalCornerLyrName, 1);
                            AEE_Utility.changeColor(circleId, nonStandardColorIndex);
                            listOfNonPerpendiculareLine.Add(lineId);

                            listOfInternalWallCorner_ObjId_EvenNo.Clear();
                            listOfInternalWallCornerText_ObjId_EvenNo.Clear();
                            listOfInternalWallCorner_Points_EvenNo.Clear();
                            listOfInternalWallCorner_Points_OddNo.Clear();
                            listOfInternalWallCorner_ObjId_OddNo.Clear();
                            listOfInternalWallCornerText_ObjId_OddNo.Clear();
                        }

                        evenCount++;
                        oddCount++;

                        setWallPanelPoint(index, wallLineId_With_RoomName, evenCount, listOfInternalWallCorner_ObjId_EvenNo, listOfInternalWallCornerText_ObjId_EvenNo, listOfInternalWallCorner_Points_EvenNo, oddCount, listOfInternalWallCorner_ObjId_OddNo, listOfInternalWallCornerText_ObjId_OddNo, listOfInternalWallCorner_Points_OddNo, insideOffsetWallId, internalWall_Id, flagOfSunkanSlab, sunkanSlabId);
                    }
                    setWallPanelPoint_In_LastVertex_Wall(lastWallLineId_With_RoomName, listOfAllInternalWallCorner_ObjId, listOfAllInternalWallCornerText_ObjId, listOfAllInternalWallCorner_Points, listOfInsideOffsetWallLineId, insideOffsetWallId, internalWall_Id, flagOfSunkanSlab, sunkanSlabId);
                    double elev = getCornerHeightOfInternallWall(internalWallEnt.Layer, sunkanSlabId);
                    slabJointHlp.moveSlabJointCornerInBeamLine(listOfInternalWallLineId, internalWall_Id, elev);

                    BeamHelper beamHlp = new BeamHelper();
                    beamHlp.setBeamBottomWallLineRoomWise();

                    AEE_Utility.deleteEntity(insideOffsetWallId);
                    AEE_Utility.deleteEntity(listOfInsideOffsetWallLineId);

                }
                listOfAllInternalWallCornerId.AddRange(listOfAllInternalWallCorner_ObjId);//BY MT on 24/04/2022
                listOfAllInternalWallCornerTextId.AddRange(listOfAllInternalWallCornerText_ObjId);//BY MT on 24/04/2022
            }
        }

        private void CheckandStretchInternalCornerWithWindowOrDoor(ObjectId internalCorner_id, List<ObjectId> internalCorner_textId)
        {

            CornerHelper cornerHlp = new CornerHelper();
            ObjectId tempRecId;
            double cornerIdStretchLen = 0;

            foreach (ObjectId windowId in WindowHelper.listOfWindowObjId)
            {
                var listOfInsctPoint = AEE_Utility.InterSectionBetweenTwoEntity(windowId, internalCorner_id);

                if (listOfInsctPoint.Count == 2)
                {
                    tempRecId = internalCorner_id;

                    ObjectId tempLineId = AEE_Utility.getLineId(listOfInsctPoint[0], listOfInsctPoint[1], true);

                    ObjectId tempOffsetLineId = AEE_Utility.OffsetLine(tempLineId, 1, true);

                    bool pointIsInsideWindow = AEE_Utility.GetPointIsInsideThePolyline(windowId, AEE_Utility.GetLine(tempOffsetLineId).StartPoint);

                    AEE_Utility.deleteEntity(tempOffsetLineId);

                    if (pointIsInsideWindow == true)
                        tempOffsetLineId = AEE_Utility.OffsetLine(tempLineId, -CommonModule.panelDepth, true);
                    else
                        tempOffsetLineId = AEE_Utility.OffsetLine(tempLineId, CommonModule.panelDepth, true);

                    tempRecId = AEE_Utility.createRectangle(listOfInsctPoint[0], listOfInsctPoint[1], AEE_Utility.GetLine(tempOffsetLineId).EndPoint, AEE_Utility.GetLine(tempOffsetLineId).StartPoint, AEE_Utility.GetLayerName(windowId), 1);

                    cornerIdStretchLen = AEE_Utility.GetLine(tempOffsetLineId).Length;

                    AEE_Utility.deleteEntity(tempLineId);
                    AEE_Utility.deleteEntity(tempOffsetLineId);

                    bool flag = cornerHlp.stretchInternalCorner_BeHalfOfWindowDoor(internalCorner_id, internalCorner_textId, tempRecId, -cornerIdStretchLen);

                    AEE_Utility.deleteEntity(tempRecId);


                }
            }
        }

        private double getCornerHeightOfInternallWall(string wallLayer, ObjectId sunkanSlabId)
        {
            double beamBottom = 0;
            double IC_CornerHght = 0;
            if (CommonModule.ductLayerName == wallLayer || Lift_UI_Helper.checkLiftLayerIsExist(wallLayer))
            {
                IC_CornerHght = GeometryHelper.getHeightOfWall_EC_IC_ExternallWall(wallLayer, InternalWallSlab_UI_Helper.getSlabBottom(wallLayer), beamBottom, PanelLayout_UI.getSlabThickness(wallLayer), PanelLayout_UI.RC, PanelLayout_UI.kickerHt, CommonModule.internalCorner, sunkanSlabId);

            }
            else
            {
                IC_CornerHght = GeometryHelper.getHeightOfWall_EC_IC_InternallWall(wallLayer, InternalWallSlab_UI_Helper.getSlabBottom(wallLayer), beamBottom, PanelLayout_UI.SL, CommonModule.internalCorner, sunkanSlabId);
            }
            // commented on 03/03/2020
            //double levelDifferenceOfSunkanSlb = SunkanSlabHelper.getSunkanSlabLevelDifference_LessThan_RC__Corners(sunkanSlabId);
            //IC_CornerHght = IC_CornerHght + levelDifferenceOfSunkanSlb;

            return IC_CornerHght;
        }

        private void drawExternalCorner_In_InternallWall(string wallLayerName, string xDataRegAppName, ObjectId internalWall_Id, Point3d basepoint, double angleOfLine, ObjectId offsetInterwall_ObjId, int index, List<ObjectId> listOfOffsetWall_Ids, ObjectId sunkanSlabId)
        {
            CornerHelper cornerHlpr = new CornerHelper();
            bool flagOfExternalDwg = false;
            double rotationAngle = 0;
            CommonModule common_Obj = new CommonModule();
            common_Obj.checkExternalCornerIsInside_Innerwall(xDataRegAppName, internalWall_Id, basepoint, angleOfLine, offsetInterwall_ObjId, index, listOfOffsetWall_Ids, out flagOfExternalDwg, out rotationAngle);
            if (flagOfExternalDwg == true)
            {
                PartHelper partHelper = new PartHelper();
                CommonModule.rotateAngle = (rotationAngle * Math.PI) / 180; // variable assign for rotation    
                List<ObjectId> listOfObjId = new List<ObjectId>();
                var externalCornerId = partHelper.drawExternalCorner(xDataRegAppName, basepoint.X, basepoint.Y, CommonModule.extrnlCornr_Flange, CommonModule.externalCornerThick, CommonModule.externalCornerLyrName, CommonModule.externalCornerLyrColor);

                listOfObjId.Add(externalCornerId);
                MoveRorationScaleJig.RotationScaleJig_Method(listOfObjId);

                listOfInternalWallCorner_Points_EvenNo.Add(CommonModule.basePoint);
                listOfInternalWallCorner_ObjId_EvenNo.Add(externalCornerId);

                listOfInternalWallCorner_Points_OddNo.Add(CommonModule.basePoint);
                listOfInternalWallCorner_ObjId_OddNo.Add(externalCornerId);

                listOfAllInternalWallCorner_Points.Add(CommonModule.basePoint);
                listOfAllInternalWallCorner_ObjId.Add(externalCornerId);

                double EC_CornerHght = getCornerHeightOfInternallWall(wallLayerName, sunkanSlabId);

                List<List<object>> lstTemp = new List<List<object>>();
                List<string> cornerTexts = CornerHelper.UpdateCornerTextExternal(lstTemp, EC_CornerHght);
                double crnerRotationAngle = rotationAngle + CommonModule.cornerRotationAngle;
                List<ObjectId> textIds = cornerHlpr.writeMultipleTextInCorner(externalCornerId, cornerTexts, crnerRotationAngle, CommonModule.externalCornerTextLyrName, CommonModule.externalCornerLyrColor);
                listOfInternalWallCornerText_ObjId_EvenNo.Add(textIds);
                listOfInternalWallCornerText_ObjId_OddNo.Add(textIds);
                listOfAllInternalWallCornerText_ObjId.Add(textIds);

                cornerHlpr.setExternalCornerInConcave(externalCornerId, textIds);

                for (int i = 0; i < lstTemp.Count; i++)
                {
                    CornerHelper.setCornerDataForBOM(xDataRegAppName, externalCornerId, textIds[i], cornerTexts[i], CommonModule.externalCornerDescp, CommonModule.externalCornerText, CommonModule.extrnlCornr_Flange, CommonModule.extrnlCornr_Flange, Convert.ToDouble(lstTemp[i][4])/*, 0.0/*elev*/);
                }
            }
        }
        public ObjectId drawOffsetInternalWall(ObjectId internalWall_ObjId, double offsetValue)
        {
            // get inside offset externall wall.
            ObjectId offsetInterwall_ObjId = new ObjectId();

            List<Point2d> listOfInternallWall_Vertex = AEE_Utility.GetPolylineVertexPoint(internalWall_ObjId);

            double intrnalWallMinX = listOfInternallWall_Vertex.Min(point => point.X);
            double intrnalWallMinY = listOfInternallWall_Vertex.Min(point => point.Y);
            double intrnalWallMaxX = listOfInternallWall_Vertex.Max(point => point.X);
            double intrnalWallMaxY = listOfInternallWall_Vertex.Max(point => point.Y);
            var actualWallLength = AEE_Utility.GetLengthOfLine(intrnalWallMinX, intrnalWallMinY, 0, intrnalWallMaxX, intrnalWallMaxY, 0);

            var positiveOffsetIntrnlWall_Id = AEE_Utility.OffsetEntity(offsetValue, internalWall_ObjId, false);
            if (positiveOffsetIntrnlWall_Id.IsValid == false)
            {
                offsetInterwall_ObjId = new ObjectId();
                return offsetInterwall_ObjId;
            }

            List<Point2d> listOfPstvOffstInternallWall_Vertex = AEE_Utility.GetPolylineVertexPoint(positiveOffsetIntrnlWall_Id);
            double pstvOffstWallMinX = listOfPstvOffstInternallWall_Vertex.Min(point => point.X);
            double pstvOffstWallMinY = listOfPstvOffstInternallWall_Vertex.Min(point => point.Y);
            double pstvOffstWallMaxX = listOfPstvOffstInternallWall_Vertex.Max(point => point.X);
            double pstvOffstWallMaxY = listOfPstvOffstInternallWall_Vertex.Max(point => point.Y);
            var pstvOffstWallLength = AEE_Utility.GetLengthOfLine(pstvOffstWallMinX, pstvOffstWallMinY, 0, pstvOffstWallMaxX, pstvOffstWallMaxY, 0);

            var negativeOffsetIntrnlWall_Id = AEE_Utility.OffsetEntity(-offsetValue, internalWall_ObjId, false);
            if (negativeOffsetIntrnlWall_Id.IsValid == false)
            {
                offsetInterwall_ObjId = new ObjectId();
                AEE_Utility.deleteEntity(positiveOffsetIntrnlWall_Id);
                return offsetInterwall_ObjId;
            }

            List<Point2d> listOfNgtvOffstInternallWall_Vertex = AEE_Utility.GetPolylineVertexPoint(negativeOffsetIntrnlWall_Id);
            double ngtvOffstWallMinX = listOfNgtvOffstInternallWall_Vertex.Min(point => point.X);
            double ngtvOffstWallMinY = listOfNgtvOffstInternallWall_Vertex.Min(point => point.Y);
            double ngtvOffstWallMaxX = listOfNgtvOffstInternallWall_Vertex.Max(point => point.X);
            double ngtvOffstWallMaxY = listOfNgtvOffstInternallWall_Vertex.Max(point => point.Y);
            var ngtvOffstWallLength = AEE_Utility.GetLengthOfLine(ngtvOffstWallMinX, ngtvOffstWallMinY, 0, ngtvOffstWallMaxX, ngtvOffstWallMaxY, 0);

            if (actualWallLength > pstvOffstWallLength)
            {
                offsetInterwall_ObjId = positiveOffsetIntrnlWall_Id;
                AEE_Utility.deleteEntity(negativeOffsetIntrnlWall_Id);
            }
            else
            {
                offsetInterwall_ObjId = negativeOffsetIntrnlWall_Id;
                AEE_Utility.deleteEntity(positiveOffsetIntrnlWall_Id);
            }

            //var colonId = AEE_Utility.createColonEntityInSamePoint(offsetInterwall_ObjId, true);
            //AEE_Utility.changeColor(colonId, 4);

            AEE_Utility.changeColor(offsetInterwall_ObjId, 3);

            return offsetInterwall_ObjId;
        }

        public void setWallPanelPoint(int index, ObjectId wallLineId_With_RoomName, int evenCount, List<ObjectId> listOfWallCorner_ObjId_EvenNo, List<List<ObjectId>> listOfWallCornerText_ObjId_EvenNo, List<Point3d> listOfWallCorner_Points_EvenNo, int oddCount, List<ObjectId> listOfWallCorner_ObjId_OddNo, List<List<ObjectId>> listOfWallCornerText_ObjId_OddNo, List<Point3d> listOfWallCorner_Points_OddNo, ObjectId offsetWall_ObjId, ObjectId wall_ObjId, bool flagOfSunkanSlab, ObjectId sunkanSlabId)
        {
            if ((evenCount % 2) == 0)
            {
                if (listOfWallCorner_ObjId_EvenNo.Count == 2)
                {
                    ObjectId cornerId1 = listOfWallCorner_ObjId_EvenNo[0];
                    ObjectId cornerId2 = listOfWallCorner_ObjId_EvenNo[1];

                    Point3d point1 = listOfWallCorner_Points_EvenNo[0];
                    Point3d point2 = listOfWallCorner_Points_EvenNo[1];

                    List<ObjectId> corner1TextId = listOfWallCornerText_ObjId_EvenNo[0];
                    List<ObjectId> corner2TextId = listOfWallCornerText_ObjId_EvenNo[1];

                    drawLineBtwnTwoCorners(wallLineId_With_RoomName, cornerId1, cornerId2, corner1TextId, corner2TextId, point1, point2, offsetWall_ObjId, wall_ObjId, flagOfSunkanSlab, sunkanSlabId);
                }
                listOfWallCorner_ObjId_EvenNo.Clear();
                listOfWallCorner_Points_EvenNo.Clear();
                listOfWallCornerText_ObjId_EvenNo.Clear();
            }

            if ((oddCount % 2) == 0)
            {
                return;
            }
            if ((oddCount % 1) == 0)
            {
                if (listOfWallCorner_ObjId_OddNo.Count == 2)
                {
                    ObjectId cornerId1 = listOfWallCorner_ObjId_OddNo[0];
                    ObjectId cornerId2 = listOfWallCorner_ObjId_OddNo[1];

                    List<ObjectId> corner1TextId = listOfWallCornerText_ObjId_OddNo[0];
                    List<ObjectId> corner2TextId = listOfWallCornerText_ObjId_OddNo[1];

                    Point3d point1 = listOfWallCorner_Points_OddNo[0];
                    Point3d point2 = listOfWallCorner_Points_OddNo[1];

                    drawLineBtwnTwoCorners(wallLineId_With_RoomName, cornerId1, cornerId2, corner1TextId, corner2TextId, point1, point2, offsetWall_ObjId, wall_ObjId, flagOfSunkanSlab, sunkanSlabId);
                }
                listOfWallCorner_ObjId_OddNo.Clear();
                listOfWallCorner_Points_OddNo.Clear();
                listOfWallCornerText_ObjId_OddNo.Clear();
            }
        }

        public void setWallPanelPoint_In_LastVertex_Wall(ObjectId lastWallLineId_With_RoomName, List<ObjectId> listOfAllWallCorner_ObjId, List<List<ObjectId>> listOfAllWallCornerText_ObjId, List<Point3d> listOfAllWallCorner_Points, List<ObjectId> listOfOffsetInterWallExplode_Id, ObjectId offsetWall_ObjId, ObjectId wall_ObjId, bool flagOfSunkanSlab, ObjectId sunkanSlabId)
        {
            if (listOfAllWallCorner_ObjId.Count >= 2 && listOfAllWallCorner_Points.Count >= 2)
            {
                ObjectId firstCrnr_ObjId = listOfAllWallCorner_ObjId[0];
                ObjectId lastCrnr_ObjId = listOfAllWallCorner_ObjId[(listOfAllWallCorner_ObjId.Count - 1)];

                List<ObjectId> firstCrnrText_ObjId = listOfAllWallCornerText_ObjId[0];
                List<ObjectId> lastCrnrText_ObjId = listOfAllWallCornerText_ObjId[(listOfAllWallCornerText_ObjId.Count - 1)];

                var firstCrnr_Point = listOfAllWallCorner_Points[0];
                var lastCrnr_Point = listOfAllWallCorner_Points[listOfAllWallCorner_Points.Count - 1];

                ObjectId firstLine_ObjId = listOfOffsetInterWallExplode_Id[0];
                ObjectId lastLine_ObjId = listOfOffsetInterWallExplode_Id[(listOfOffsetInterWallExplode_Id.Count - 1)];

                var listOfFirstCrner_FirstLine_Insct = AEE_Utility.InterSectionBetweenTwoEntity(firstLine_ObjId, firstCrnr_ObjId);
                var listOfLastCrner_LastLine_Insct = AEE_Utility.InterSectionBetweenTwoEntity(lastLine_ObjId, lastCrnr_ObjId);
                if (listOfFirstCrner_FirstLine_Insct.Count != 0 && listOfLastCrner_LastLine_Insct.Count != 0)
                {
                    //Fix the corners order for the last wall by SDM 28-06-2022
                    // drawLineBtwnTwoCorners(lastWallLineId_With_RoomName, firstCrnr_ObjId, lastCrnr_ObjId, firstCrnrText_ObjId, lastCrnrText_ObjId, firstCrnr_Point, lastCrnr_Point, offsetWall_ObjId, wall_ObjId, flagOfSunkanSlab, sunkanSlabId);

                    drawLineBtwnTwoCorners(lastWallLineId_With_RoomName, lastCrnr_ObjId, firstCrnr_ObjId, lastCrnrText_ObjId, firstCrnrText_ObjId, lastCrnr_Point, firstCrnr_Point, offsetWall_ObjId, wall_ObjId, flagOfSunkanSlab, sunkanSlabId);
                }
            }
        }

        public void drawLineBtwnTwoCorners(ObjectId wallLineId_With_RoomName, ObjectId cornerId1, ObjectId cornerId2, List<ObjectId> cornerId1_TextId, List<ObjectId> cornerId2_TextId, Point3d point1, Point3d point2, ObjectId offsetWall_ObjId, ObjectId wall_ObjId, bool flagOfSunkanSlab, ObjectId sunkanSlabId)
        {
            var xDataRegAppName = AEE_Utility.GetXDataRegisterAppName(wallLineId_With_RoomName);

            ObjectId wallPanelLineId = new ObjectId();
            double wallPanelLineLength = 0;
            double wallPanelLineAngle = 0;
            getWallPanelLineId(point1, point2, cornerId1, cornerId2, offsetWall_ObjId, xDataRegAppName, out wallPanelLineId, out wallPanelLineLength, out wallPanelLineAngle);

            wallPanelLineAngle = AEE_Utility.GetAngleOfLine(wallLineId_With_RoomName);

            BeamHelper beamHlp = new BeamHelper();
            ObjectId beamId_InsctWall = new ObjectId();
            ObjectId nearestWallToBeamWallId = new ObjectId();
            double distBtwWallToBeam = 0;

            List<Point3d> listOfAllInsctPointOfTwoBeam = new List<Point3d>();

            ObjectId anotherParapetId_InsctToParapet = new ObjectId();
            ParapetHelper parapetHlp = new ParapetHelper();
            ObjectId parapetId = parapetHlp.checkParapetIsAvailableInWallPanel(wallPanelLineId, cornerId1, cornerId2, cornerId1_TextId, cornerId2_TextId, anotherParapetId_InsctToParapet, sunkanSlabId);

            Point3d adjacentInsidePointOfBeam = new Point3d();
            ObjectId adjacentOfBeamId = new ObjectId();
            string beamLayerNameInsctToWall = beamHlp.getBeamName_With_WallPanelLineIsInsideTheBeam(wall_ObjId, wallPanelLineId, wallLineId_With_RoomName, out beamId_InsctWall, out nearestWallToBeamWallId, out distBtwWallToBeam, out listOfAllInsctPointOfTwoBeam, out adjacentOfBeamId, out adjacentInsidePointOfBeam);
            
            ObjectId beamCornerId1 = new ObjectId();
            ObjectId beamCornerId2 = new ObjectId();
            List<ObjectId> listOfBeamInsideWallLinesId = new List<ObjectId>();
            bool isBeam = beamLayerNameInsctToWall.Contains(Beam_UI_Helper.beamStrtText);
            if (beamId_InsctWall.IsValid == true)
            {
                beamHlp.setBeamLine_ForCorner(xDataRegAppName, wall_ObjId, listOfAllInsctPointOfTwoBeam, nearestWallToBeamWallId, distBtwWallToBeam, wallPanelLineId, wallLineId_With_RoomName, cornerId1, cornerId2, beamId_InsctWall, beamLayerNameInsctToWall, adjacentInsidePointOfBeam, adjacentOfBeamId, flagOfSunkanSlab, sunkanSlabId, out beamCornerId1, out beamCornerId2);
                //if (listOfAllInsctPointOfTwoBeam.Count != 0)
                //{
                //    wallPanelLineId = beamHlp.changeWallPanelLine_With_TwoBeamIntersct(xDataRegAppName, cornerId1, cornerId2, wallPanelLineId, listOfAllInsctPointOfTwoBeam, distBtwWallToBeam, nearestWallToBeamWallId, out listOfBeamInsideWallLinesId);
                //    wallPanelLineLength = AEE_Utility.GetLengthOfLine(wallPanelLineId);
                //}
                beamHlp.checkBeamWallPanelLength(xDataRegAppName, beamId_InsctWall, wallPanelLineId, nearestWallToBeamWallId, distBtwWallToBeam, beamCornerId1, beamCornerId2, flagOfSunkanSlab, sunkanSlabId);
            }
            SlabJointHelper slabJointHlp = new SlabJointHelper();
            slabJointHlp.setSlabJointBeamData_ForCorner(nearestWallToBeamWallId, distBtwWallToBeam, wallLineId_With_RoomName);

            CornerHelper cornerHlpr = new CornerHelper();
            cornerHlpr.checkCornerInsctToBeam(beamId_InsctWall, wallPanelLineId, cornerId1, cornerId2, cornerId1_TextId, cornerId2_TextId, sunkanSlabId);

            if (beamId_InsctWall.IsValid && isBeam)
                return;
            List<ObjectId> listOfWindowPanelLine_ObjId = new List<ObjectId>();
            List<ObjectId> listOfWindowObjId_With_WallInsct = new List<ObjectId>();
            List<ObjectId> listOfWallPanelLineWithWindowInsct_ObjId = new List<ObjectId>();

            WindowHelper windowHlp = new WindowHelper();
            windowHlp.checkWindowIsIntersectToWallPanelLine(xDataRegAppName, wallPanelLineId, out listOfWindowPanelLine_ObjId, out listOfWallPanelLineWithWindowInsct_ObjId, out listOfWindowObjId_With_WallInsct);
            windowHlp.checkWindowIsIntersectToChajjaLine();//Added on 22/06/2023 by PRT
            windowHlp.setWindowPanelLines(xDataRegAppName, nearestWallToBeamWallId, distBtwWallToBeam, beamId_InsctWall, beamLayerNameInsctToWall, listOfWindowPanelLine_ObjId, cornerId1, cornerId2, wallPanelLineLength, flagOfSunkanSlab, sunkanSlabId, parapetId);
            windowHlp.createCornersInWindow(xDataRegAppName, beamLayerNameInsctToWall, listOfWindowObjId_With_WallInsct, listOfWindowPanelLine_ObjId, WindowHelper.listOfWindowPanelOffsetLine_ObjId, offsetWall_ObjId, cornerId1, cornerId2, cornerId1_TextId, cornerId2_TextId, sunkanSlabId, parapetId);

            //List<ObjectId> listOfWallId = new List<ObjectId>();
            //listOfWallId.Add(wallPanelLineId);
            //DoorAndWindowHelper doorAndWndowHlp = new DoorAndWindowHelper();
            //doorAndWndowHlp.checkDoorIsIntersectToWallPanelLine(xDataRegAppName, WindowHelper.listOfWindowObjId, wallPanelLineId, cornerId1, cornerId2, listOfWallId, out listOfWindowPanelLine_ObjId, out listOfWallPanelLineWithWindowInsct_ObjId, out listOfWindowObjId_With_WallInsct);
            //doorAndWndowHlp.setDoorPanelLines(xDataRegAppName, nearestWallToBeamWallId, distBtwWallToBeam, beamId_InsctWall, beamLayerNameInsctToWall, listOfWindowPanelLine_ObjId, cornerId1, cornerId2, wallPanelLineLength, flagOfSunkanSlab, sunkanSlabId, parapetId);
            //doorAndWndowHlp.createCornersInDoor(xDataRegAppName, WindowHelper.listOfWindowObjId, WindowHelper.listOfWindowObjId_Str, beamLayerNameInsctToWall, listOfWindowObjId_With_WallInsct,  offsetWall_ObjId, cornerId1, cornerId2, cornerId1_TextId, cornerId2_TextId, sunkanSlabId, parapetId);


            List<ObjectId> listOfDoorPanelLine_ObjId = new List<ObjectId>();
            List<ObjectId> listOfWallPanelLineWithDoor_ObjId = new List<ObjectId>();
            List<ObjectId> listOfDoorObjId_With_WallInsct = new List<ObjectId>();

            DoorHelper doorHlpr = new DoorHelper();
            doorHlpr.checkDoorIsIntersectToWallPanelLine(xDataRegAppName, wallPanelLineId, cornerId1, cornerId2, listOfWallPanelLineWithWindowInsct_ObjId, out listOfDoorPanelLine_ObjId, out listOfWallPanelLineWithDoor_ObjId, out listOfDoorObjId_With_WallInsct);
            List<List<ObjectId>> listOfListOfDoorPanelsId = doorHlpr.setDoorPanelLines(xDataRegAppName, nearestWallToBeamWallId, distBtwWallToBeam, beamId_InsctWall, beamLayerNameInsctToWall, listOfDoorPanelLine_ObjId, cornerId1, cornerId2, wallPanelLineLength, flagOfSunkanSlab, sunkanSlabId, parapetId);

            doorHlpr.createCornersInDoor(xDataRegAppName, beamId_InsctWall, beamLayerNameInsctToWall, listOfListOfDoorPanelsId, offsetWall_ObjId, cornerId1, cornerId2, cornerId1_TextId, cornerId2_TextId, sunkanSlabId, parapetId);

            listOfBeamInsideWallLinesId = beamHlp.checkRemainingWallBeamLineIsInsideTheDoorOrWindow(listOfBeamInsideWallLinesId, listOfWindowObjId_With_WallInsct, listOfDoorObjId_With_WallInsct);
            foreach (var id in listOfBeamInsideWallLinesId)
            {
                listOfWallPanelLineWithDoor_ObjId.Add(id);
            }

            ObjectId newWallLineIdWithBeam = new ObjectId();
            beamHlp.getNewWallPanelLineWithAdjacentBeam(listOfWallPanelLineWithDoor_ObjId, adjacentOfBeamId, adjacentInsidePointOfBeam, beamLayerNameInsctToWall, out newWallLineIdWithBeam);

            List<ObjectId> listOfWallLineWithoutParapetId = new List<ObjectId>();
            listOfWallPanelLineWithDoor_ObjId = parapetHlp.splitWallPanelLineWithParapet(listOfWallPanelLineWithDoor_ObjId, parapetId, out listOfWallLineWithoutParapetId);

            List<ObjectId> listOfLineBtwTwoCrners_ObjId = new List<ObjectId>();
            List<double> listOfDistBtwTwoCrners = new List<double>();
            List<string> listOfDistAndObjId_BtwTwoCrners = new List<string>();
            List<ObjectId> listOfOffstLinesBtwTwoCrners_ObjId = new List<ObjectId>();
            beamHlp.CheckIntersections(listOfWallPanelLineWithDoor_ObjId);
            getPanelLine(listOfWallPanelLineWithDoor_ObjId, cornerId1, cornerId2, wallPanelLineLength, out listOfLineBtwTwoCrners_ObjId, out listOfDistBtwTwoCrners, out listOfDistAndObjId_BtwTwoCrners, out listOfOffstLinesBtwTwoCrners_ObjId);

            CornerHelper cornerHlp = new CornerHelper();
            cornerHlp.checkWallLineIntersctToExternalCornerId(cornerId1, cornerId2, wallPanelLineId, listOfLineBtwTwoCrners_ObjId);
            cornerHlp.checkWallLineIntersctToInternalCornerId(cornerId1, cornerId2, wallPanelLineId, listOfLineBtwTwoCrners_ObjId);
            setWallPanelLineData(xDataRegAppName, cornerId1, cornerId2, listOfWallLineWithoutParapetId, listOfLineBtwTwoCrners_ObjId, listOfOffstLinesBtwTwoCrners_ObjId, listOfDistBtwTwoCrners, listOfDistAndObjId_BtwTwoCrners, newWallLineIdWithBeam, nearestWallToBeamWallId, adjacentOfBeamId, beamLayerNameInsctToWall, beamId_InsctWall, distBtwWallToBeam, flagOfSunkanSlab, sunkanSlabId, parapetId);
        }

        private void getWallPanelLineId(Point3d point1, Point3d point2, ObjectId cornerId1, ObjectId cornerId2, ObjectId offsetWall_ObjId, string xDataRegAppName, out ObjectId wallPanelLineId, out double wallPanelLineLength, out double wallPanelLineAngle)
        {
            var ent_SameProperty = AEE_Utility.GetEntityForWrite(offsetWall_ObjId);
            var lineId = AEE_Utility.getLineId(ent_SameProperty, point1.X, point1.Y, 0, point2.X, point2.Y, 0, false);

            var listOfInsctPoint1 = AEE_Utility.InterSectionBetweenTwoEntity(cornerId1, lineId);
            foreach (var pnt in listOfInsctPoint1)
            {
                
                if (!AEE_Utility.IsPointsAreEqual(pnt,point1))//Changes made on 12/07/2023 by SDM
                {
                    point1 = pnt;
                    break;
                }
            }

            var listOfInsctPoint2 = AEE_Utility.InterSectionBetweenTwoEntity(cornerId2, lineId);
            foreach (var pnt in listOfInsctPoint2)
            {
                if (!AEE_Utility.IsPointsAreEqual(pnt, point2))//Changes made on 12/07/2023 by SDM
                {
                    point2 = pnt;
                    break;
                }
            }

            AEE_Utility.deleteEntity(lineId);

            wallPanelLineId = AEE_Utility.getLineId(ent_SameProperty, point1.X, point1.Y, 0, point2.X, point2.Y, 0, false);

            wallPanelLineLength = AEE_Utility.GetLengthOfLine(wallPanelLineId);
            wallPanelLineAngle = AEE_Utility.GetAngleOfLine(wallPanelLineId);
            AEE_Utility.AttachXData(wallPanelLineId, xDataRegAppName, CommonModule.xDataAsciiName);
        }

        private void setWallPanelLineData(string xDataRegAppName, ObjectId cornerId1, ObjectId cornerId2, List<ObjectId> listOfWallLineWithoutParapetId, List<ObjectId> listOfLineBtwTwoCrners_ObjId, List<ObjectId> listOfOffstLinesBtwTwoCrners_ObjId, List<double> listOfDistBtwTwoCrners, List<string> listOfDistAndObjId_BtwTwoCrners, ObjectId newWallLineIdWithBeam, ObjectId nearestWallToBeamWallId, ObjectId adjacentOfBeamId, string beamLayerNameInsctToWall, ObjectId beamId_InsctWall, double distBtwWallToBeam, bool flagOfSunkanSlab, ObjectId sunkanSlabId, ObjectId parapetId)
        {
            for (int p = 0; p < listOfDistBtwTwoCrners.Count; p++)
            {
                listOfAllCornerId1_ForInsctDoor.Add(cornerId1);
                listOfAllCornerId2_ForInsctDoor.Add(cornerId2);
                listOfAllCornerId_ForDoor_And_Beam.Add(beamId_InsctWall);

                ObjectId wallPanelLineId_ForPanel = listOfLineBtwTwoCrners_ObjId[p];

                AEE_Utility.AttachXData(wallPanelLineId_ForPanel, xDataRegAppName, CommonModule.xDataAsciiName);

                if (wallPanelLineId_ForPanel == newWallLineIdWithBeam && newWallLineIdWithBeam.IsValid == true)
                {
                    var adjacentOfBeam = AEE_Utility.GetEntityForRead(adjacentOfBeamId);
                    var distOfWallLineToBeam = AEE_Utility.GetLengthOfLine(newWallLineIdWithBeam);
                    BeamHelper.listOfLinesBtwTwoCrners_BeamInsctName.Add(adjacentOfBeam.Layer);
                    BeamHelper.listOfLinesBtwTwoCrners_BeamInsctId.Add(adjacentOfBeamId);
                    BeamHelper.listOfDistanceBtwWallToBeam.Add(distOfWallLineToBeam);
                    BeamHelper.listOfNearestBtwWallToBeamWallLineId.Add(nearestWallToBeamWallId);
                }
                else
                {
                    BeamHelper.listOfLinesBtwTwoCrners_BeamInsctName.Add(beamLayerNameInsctToWall);
                    BeamHelper.listOfLinesBtwTwoCrners_BeamInsctId.Add(beamId_InsctWall);
                    BeamHelper.listOfDistanceBtwWallToBeam.Add(distBtwWallToBeam);
                    BeamHelper.listOfNearestBtwWallToBeamWallLineId.Add(nearestWallToBeamWallId);
                }
                listOfLinesBtwTwoCrners_ObjId.Add(wallPanelLineId_ForPanel);
                listOfDistnceBtwTwoCrners.Add(listOfDistBtwTwoCrners[p]);
                listOfDistnceAndObjId_BtwTwoCrners.Add(listOfDistAndObjId_BtwTwoCrners[p]);
                listOfOffsetLinesBtwTwoCrners_ObjId.Add(listOfOffstLinesBtwTwoCrners_ObjId[p]);


                SunkanSlabHelper.listOfLinesBtwTwoCrners_SunkanSlabWallLineId.Add(wallPanelLineId_ForPanel);
                SunkanSlabHelper.listOfLinesBtwTwoCrners_SunkanSlabWallLineflag.Add(flagOfSunkanSlab);
                SunkanSlabHelper.listOfLinesBtwTwoCrners_SunkanSlabId.Add(sunkanSlabId);
                //Fix window panels elevation when there is sunkan by SDM 2022-07-04
                if (flagOfSunkanSlab && !SunkanSlabHelper.listOfWallName_SunkanSlabId.ContainsKey(xDataRegAppName))
                    SunkanSlabHelper.listOfWallName_SunkanSlabId.Add(xDataRegAppName, sunkanSlabId);

                ParapetHelper.listOfWallLineId_InsctToParapet.Add(wallPanelLineId_ForPanel);

                if (listOfWallLineWithoutParapetId.Contains(wallPanelLineId_ForPanel))
                {
                    ObjectId parpetId = new ObjectId();
                    ParapetHelper.listOfParapetId_WithWallLine.Add(parpetId);
                }
                else
                {
                    ParapetHelper.listOfParapetId_WithWallLine.Add(parapetId);
                }
            }
        }

        public void getPanelLine(List<ObjectId> listOfPanelLine_ObjId, ObjectId cornerId1, ObjectId cornerId2, double panelLineLength, out List<ObjectId> listOfLineBtwTwoCrners_ObjId, out List<double> listOfDistBtwTwoCrners, out List<string> listOfDistAndObjId_BtwTwoCrners, out List<ObjectId> listOfOffstLinesBtwTwoCrners_ObjId)
        {
            listOfLineBtwTwoCrners_ObjId = new List<ObjectId>();
            listOfDistBtwTwoCrners = new List<double>();
            listOfDistAndObjId_BtwTwoCrners = new List<string>();
            listOfOffstLinesBtwTwoCrners_ObjId = new List<ObjectId>();

            for (int j = 0; j < listOfPanelLine_ObjId.Count; j++)
            {
                ObjectId windowAndWall_LineId = listOfPanelLine_ObjId[j];

                var lengthOfLine = Math.Round((AEE_Utility.GetLengthOfLine(windowAndWall_LineId)), 1);
                if (lengthOfLine >= 1)
                {
                    var listOfStrtEndPoint = AEE_Utility.getStartEndPointWithAngle_Line(windowAndWall_LineId);
                    Point3d strtPnt = new Point3d(listOfStrtEndPoint[0], listOfStrtEndPoint[1], 0);
                    Point3d endPnt = new Point3d(listOfStrtEndPoint[2], listOfStrtEndPoint[3], 0);
                    double angleOflin = listOfStrtEndPoint[4];
                    var extraLngthPoint = AEE_Utility.get_XY(angleOflin, (1.5 * panelLineLength));

                    var lineIdForInsct = AEE_Utility.getLineId((strtPnt.X - extraLngthPoint.X), (strtPnt.Y - extraLngthPoint.Y), 0, (endPnt.X + extraLngthPoint.X), (endPnt.Y + extraLngthPoint.Y), 0, false);
                    var lengthOfLineForInsct = AEE_Utility.GetLengthOfLine(lineIdForInsct);
                    if (lengthOfLineForInsct >= 1)
                    {
                        var offsetLineIdForInsct = AEE_Utility.OffsetLine(lineIdForInsct, CommonModule.panelDepth, false);
                        var listOfInsctPoint1_OffstLine = AEE_Utility.InterSectionBetweenTwoEntity(cornerId1, offsetLineIdForInsct);
                        var listOfInsctPoint2_OffstLine = AEE_Utility.InterSectionBetweenTwoEntity(cornerId2, offsetLineIdForInsct);

                        ObjectId offsetWindowAndWall_LineId = new ObjectId();
                        if (listOfInsctPoint1_OffstLine.Count == 0 && listOfInsctPoint2_OffstLine.Count == 0)
                        {
                            AEE_Utility.deleteEntity(offsetLineIdForInsct);
                            offsetLineIdForInsct = AEE_Utility.OffsetLine(lineIdForInsct, -CommonModule.panelDepth, false);

                            offsetWindowAndWall_LineId = AEE_Utility.OffsetLine(windowAndWall_LineId, -CommonModule.panelDepth, false);

                            AEE_Utility.deleteEntity(offsetLineIdForInsct);
                        }
                        else
                        {
                            offsetWindowAndWall_LineId = AEE_Utility.OffsetLine(windowAndWall_LineId, CommonModule.panelDepth, false);
                        }

                        listOfOffstLinesBtwTwoCrners_ObjId.Add(offsetWindowAndWall_LineId);
                        listOfLineBtwTwoCrners_ObjId.Add(windowAndWall_LineId);
                        listOfDistBtwTwoCrners.Add(lengthOfLine);
                        listOfDistAndObjId_BtwTwoCrners.Add(Convert.ToString(lengthOfLine) + "," + Convert.ToString(windowAndWall_LineId));
                    }
                    AEE_Utility.deleteEntity(lineIdForInsct);
                }
            }
        }
        public void sortAllLinesBtwnTwoCorners()
        {
            if (listOfDistnceBtwTwoCrners.Count == 0)
            {
                return;
            }

            List<string> listOfDistAndObjId = new List<string>();
            List<ObjectId> listOfLineBtwnTwoCnrs_ObjId = new List<ObjectId>();
            List<ObjectId> listOfOffsetLineBtwnTwoCnrs_ObjId = new List<ObjectId>();
            List<double> listOfDistBtwnTwoCnrs = new List<double>();
            List<string> listOfBeamNameWithInsctWallLine = new List<string>();
            List<ObjectId> listOfBeamIdWithInsctWallLine = new List<ObjectId>();
            List<double> listOfDistBtwBeamToWall = new List<double>();
            List<ObjectId> listOfNearestBeamWallPanelId = new List<ObjectId>();

            List<ObjectId> listOfLinesBtwTwoCrners_SunkanSlabWallLineId = new List<ObjectId>();
            List<bool> listOfLinesBtwTwoCrners_SunkanSlabWallLineflag = new List<bool>();
            List<ObjectId> listOfLinesBtwTwoCrners_SunkanSlabId = new List<ObjectId>();

            List<ObjectId> listOfWallLineId_InsctToParapet = new List<ObjectId>();
            List<ObjectId> listOfParapetId_WithWallLine = new List<ObjectId>();

            List<double> listOfDistBtwnTwoCnrs_DescdningOrder = listOfDistnceBtwTwoCrners.OrderByDescending(n => n).ToList();

            for (int index = 0; index < listOfDistBtwnTwoCnrs_DescdningOrder.Count; index++)
            {
                double length1 = listOfDistBtwnTwoCnrs_DescdningOrder[index];
                for (int j = 0; j < listOfDistnceAndObjId_BtwTwoCrners.Count; j++)
                {
                    string data = listOfDistnceAndObjId_BtwTwoCrners[j];
                    if (!listOfDistAndObjId.Contains(data))
                    {
                        string[] array = data.Split(',');
                        double length2 = Convert.ToDouble(array.GetValue(0));
                        if (length1 == length2)
                        {
                            listOfDistAndObjId.Add(data);
                            listOfDistBtwnTwoCnrs.Add(listOfDistnceBtwTwoCrners[j]);
                            listOfLineBtwnTwoCnrs_ObjId.Add(listOfLinesBtwTwoCrners_ObjId[j]);
                            listOfOffsetLineBtwnTwoCnrs_ObjId.Add(listOfOffsetLinesBtwTwoCrners_ObjId[j]);
                            listOfBeamNameWithInsctWallLine.Add(BeamHelper.listOfLinesBtwTwoCrners_BeamInsctName[j]);
                            listOfBeamIdWithInsctWallLine.Add(BeamHelper.listOfLinesBtwTwoCrners_BeamInsctId[j]);
                            listOfDistBtwBeamToWall.Add(BeamHelper.listOfDistanceBtwWallToBeam[j]);
                            listOfNearestBeamWallPanelId.Add(BeamHelper.listOfNearestBtwWallToBeamWallLineId[j]);

                            listOfLinesBtwTwoCrners_SunkanSlabWallLineId.Add(SunkanSlabHelper.listOfLinesBtwTwoCrners_SunkanSlabWallLineId[j]);
                            listOfLinesBtwTwoCrners_SunkanSlabWallLineflag.Add(SunkanSlabHelper.listOfLinesBtwTwoCrners_SunkanSlabWallLineflag[j]);
                            listOfLinesBtwTwoCrners_SunkanSlabId.Add(SunkanSlabHelper.listOfLinesBtwTwoCrners_SunkanSlabId[j]);

                            listOfWallLineId_InsctToParapet.Add(ParapetHelper.listOfWallLineId_InsctToParapet[j]);
                            listOfParapetId_WithWallLine.Add(ParapetHelper.listOfParapetId_WithWallLine[j]);
                            break;
                        }
                    }
                }
            }

            listOfDistnceAndObjId_BtwTwoCrners.Clear();
            listOfDistnceBtwTwoCrners.Clear();
            listOfLinesBtwTwoCrners_ObjId.Clear();
            listOfOffsetLinesBtwTwoCrners_ObjId.Clear();
            BeamHelper.listOfLinesBtwTwoCrners_BeamInsctName.Clear();
            BeamHelper.listOfLinesBtwTwoCrners_BeamInsctId.Clear();
            BeamHelper.listOfDistanceBtwWallToBeam.Clear();
            BeamHelper.listOfNearestBtwWallToBeamWallLineId.Clear();

            SunkanSlabHelper.listOfLinesBtwTwoCrners_SunkanSlabWallLineId.Clear();
            SunkanSlabHelper.listOfLinesBtwTwoCrners_SunkanSlabWallLineflag.Clear();
            SunkanSlabHelper.listOfLinesBtwTwoCrners_SunkanSlabId.Clear();

            ParapetHelper.listOfWallLineId_InsctToParapet.Clear();
            ParapetHelper.listOfParapetId_WithWallLine.Clear();

            for (int k = 0; k < listOfDistAndObjId.Count; k++)
            {
                listOfDistnceAndObjId_BtwTwoCrners.Add(listOfDistAndObjId[k]);
                listOfDistnceBtwTwoCrners.Add(listOfDistBtwnTwoCnrs[k]);
                listOfLinesBtwTwoCrners_ObjId.Add(listOfLineBtwnTwoCnrs_ObjId[k]);
                listOfOffsetLinesBtwTwoCrners_ObjId.Add(listOfOffsetLineBtwnTwoCnrs_ObjId[k]);
                BeamHelper.listOfLinesBtwTwoCrners_BeamInsctName.Add(listOfBeamNameWithInsctWallLine[k]);
                BeamHelper.listOfLinesBtwTwoCrners_BeamInsctId.Add(listOfBeamIdWithInsctWallLine[k]);
                BeamHelper.listOfDistanceBtwWallToBeam.Add(listOfDistBtwBeamToWall[k]);
                BeamHelper.listOfNearestBtwWallToBeamWallLineId.Add(listOfNearestBeamWallPanelId[k]);

                SunkanSlabHelper.listOfLinesBtwTwoCrners_SunkanSlabWallLineId.Add(listOfLinesBtwTwoCrners_SunkanSlabWallLineId[k]);
                SunkanSlabHelper.listOfLinesBtwTwoCrners_SunkanSlabWallLineflag.Add(listOfLinesBtwTwoCrners_SunkanSlabWallLineflag[k]);
                SunkanSlabHelper.listOfLinesBtwTwoCrners_SunkanSlabId.Add(listOfLinesBtwTwoCrners_SunkanSlabId[k]);

                ParapetHelper.listOfWallLineId_InsctToParapet.Add(listOfWallLineId_InsctToParapet[k]);
                ParapetHelper.listOfParapetId_WithWallLine.Add(listOfParapetId_WithWallLine[k]);
            }
        }

        public ObjectId getXDataRegisteredAppName_InternalWall(int index, List<ObjectId> listOfInternalWallLineId)
        {
            ObjectId wallLineId_With_RoomName = new ObjectId();

            if (index != 0)
            {
                wallLineId_With_RoomName = listOfInternalWallLineId[index - 1];
            }
            else
            {
                wallLineId_With_RoomName = listOfInternalWallLineId[listOfInternalWallLineId.Count - 1];
            }

            return wallLineId_With_RoomName;
        }




        public void getListOfWallId_InsctToDoorCorner()
        {
            //WindowHelper windowHlp = new WindowHelper();
            //DoorHelper doorHlp = new DoorHelper();

            //foreach (var listOfListOfExistDoorId_And_CornerId in listOfListOfListOfExistDoorId_CornerId)
            //{
            //    foreach (var listOfExistDoorId_And_CornerId in listOfListOfExistDoorId_And_CornerId)
            //    {
            //        if (listOfExistDoorId_And_CornerId.Count == 2)
            //        {
            //            ObjectId existDoorCornerId = listOfExistDoorId_And_CornerId[0];
            //            ObjectId doorId = listOfExistDoorId_And_CornerId[1];

            //            List<ObjectId> listOfMinLengthExplodeLine = windowHlp.getMinLengthOfWindowLineSegment(doorId);
            //            double wallPanelLineAngle = AEE_Utility.GetAngleOfLine(listOfMinLengthExplodeLine[0]);
            //            AEE_Utility.deleteEntity(listOfMinLengthExplodeLine);

            //            var basePointOfCorner = AEE_Utility.GetBasePointOfPolyline(existDoorCornerId);
            //            ObjectId maxLengthVerticeLineId_Corner = new ObjectId();
            //            double maxLengthOfCornerVertex = doorHlp.getMaximumLengthOfCornerVertex(wallPanelLineAngle, existDoorCornerId, out maxLengthVerticeLineId_Corner);
            //            if (maxLengthVerticeLineId_Corner.IsValid == true)
            //            {
            //                var maxLengthVerticeOffstLineId_Corner = AEE_Utility.OffsetLine(maxLengthVerticeLineId_Corner, CommonModule.panelDepth, false);
            //                var listOfInsct = AEE_Utility.InterSectionBetweenTwoEntity(doorId, maxLengthVerticeOffstLineId_Corner);
            //                if (listOfInsct.Count == 0)
            //                {
            //                    AEE_Utility.deleteEntity(maxLengthVerticeOffstLineId_Corner);
            //                    maxLengthVerticeOffstLineId_Corner = AEE_Utility.OffsetLine(maxLengthVerticeLineId_Corner, -CommonModule.panelDepth, false);
            //                }
            //                //AEE_Utility.changeVisibility(maxLengthVerticeLineId_Corner, true);
            //                //AEE_Utility.changeVisibility(maxLengthVerticeOffstLineId_Corner, true);

            //                var listOfAllCornerId1_Index = Enumerable.Range(0, listOfAllCornerId1_ForInsctDoor.Count).Where(i => listOfAllCornerId1_ForInsctDoor[i] == existDoorCornerId).ToList();
            //                var listOfAllCornerId2_Index = Enumerable.Range(0, listOfAllCornerId2_ForInsctDoor.Count).Where(i => listOfAllCornerId2_ForInsctDoor[i] == existDoorCornerId).ToList();

            //                List<int> listOfAllIndex = new List<int>();
            //                foreach (int index in listOfAllCornerId1_Index)
            //                {
            //                    listOfAllIndex.Add(index);
            //                }
            //                foreach (int index in listOfAllCornerId2_Index)
            //                {
            //                    listOfAllIndex.Add(index);
            //                }
            //                addWallLineIdOfDoorCornerInsct(listOfAllIndex, basePointOfCorner, maxLengthVerticeLineId_Corner, maxLengthVerticeOffstLineId_Corner, maxLengthOfCornerVertex);
            //            }
            //        }
            //    }
            //}
        }

        private bool getWallPaneLineChangeWithBeam(List<int> listOfAllIndex, ObjectId cornerWallLineId, ObjectId cornerWallOffstLineId)
        {
            bool flagOfWallLineStretch = true;

            BeamHelper beamHlp = new BeamHelper();
            int indexOfExist = 0;
            for (int i = 0; i < listOfAllIndex.Count; i++)
            {
                indexOfExist = listOfAllIndex[i];
                ObjectId beamId = listOfAllCornerId_ForDoor_And_Beam[indexOfExist];
                if (beamId.IsValid == true)
                {
                    bool flagOfNewBeamWallPnlCreate = false;
                    double beamWallPanelLngth = beamHlp.getBeamWallPanelLength(beamId, out flagOfNewBeamWallPnlCreate, AEE_Utility.GetLayerName(cornerWallLineId), false, new ObjectId());
                    if (flagOfNewBeamWallPnlCreate == false)
                    {
                        flagOfWallLineStretch = flagOfNewBeamWallPnlCreate;
                        break;
                    }
                }
            }


            if (flagOfWallLineStretch == false)
            {
                ObjectId wallPanelLineId = listOfLinesBtwTwoCrners_ObjId[indexOfExist];
                ObjectId wallPanelLineOffstId = listOfOffsetLinesBtwTwoCrners_ObjId[indexOfExist];

                var wallPanelLineLayer = AEE_Utility.GetLayerName(wallPanelLineId);
                var xDataRegAppName = AEE_Utility.GetXDataRegisterAppName(wallPanelLineId);

                var lengthOfLine = Math.Round((AEE_Utility.GetLengthOfLine(cornerWallLineId)), 1);
                if (lengthOfLine >= 1)
                {
                    var cornerColonWallLineId = AEE_Utility.createColonEntityInSamePoint(cornerWallLineId, false);
                    var cornerColonWallOffstLineId = AEE_Utility.createColonEntityInSamePoint(cornerWallOffstLineId, false);

                    AEE_Utility.changeLayer(cornerColonWallLineId, wallPanelLineLayer, 256);
                    AEE_Utility.changeLayer(cornerColonWallOffstLineId, wallPanelLineLayer, 256);
                    AEE_Utility.AttachXData(cornerColonWallLineId, xDataRegAppName, CommonModule.xDataAsciiName);
                    AEE_Utility.AttachXData(cornerColonWallOffstLineId, xDataRegAppName, CommonModule.xDataAsciiName);

                    BeamHelper.listOfWallPanelLineId_WithBeamDoor.Add(cornerColonWallLineId);

                    listOfDistnceBtwTwoCrners.Add(lengthOfLine);
                    listOfDistnceAndObjId_BtwTwoCrners.Add(Convert.ToString(lengthOfLine) + "," + Convert.ToString(cornerColonWallLineId));

                    listOfLinesBtwTwoCrners_ObjId.Add(cornerColonWallLineId);
                    listOfOffsetLinesBtwTwoCrners_ObjId.Add(cornerColonWallOffstLineId);

                    BeamHelper.listOfLinesBtwTwoCrners_BeamInsctName.Add(BeamHelper.listOfLinesBtwTwoCrners_BeamInsctName[indexOfExist]);
                    BeamHelper.listOfLinesBtwTwoCrners_BeamInsctId.Add(BeamHelper.listOfLinesBtwTwoCrners_BeamInsctId[indexOfExist]);
                    BeamHelper.listOfDistanceBtwWallToBeam.Add(BeamHelper.listOfDistanceBtwWallToBeam[indexOfExist]);
                    BeamHelper.listOfNearestBtwWallToBeamWallLineId.Add(BeamHelper.listOfNearestBtwWallToBeamWallLineId[indexOfExist]);

                    SunkanSlabHelper.listOfLinesBtwTwoCrners_SunkanSlabWallLineId.Add(SunkanSlabHelper.listOfLinesBtwTwoCrners_SunkanSlabWallLineId[indexOfExist]);
                    SunkanSlabHelper.listOfLinesBtwTwoCrners_SunkanSlabWallLineflag.Add(SunkanSlabHelper.listOfLinesBtwTwoCrners_SunkanSlabWallLineflag[indexOfExist]);
                    SunkanSlabHelper.listOfLinesBtwTwoCrners_SunkanSlabId.Add(SunkanSlabHelper.listOfLinesBtwTwoCrners_SunkanSlabId[indexOfExist]);

                    ParapetHelper.listOfWallLineId_InsctToParapet.Add(ParapetHelper.listOfWallLineId_InsctToParapet[indexOfExist]);
                    ParapetHelper.listOfParapetId_WithWallLine.Add(ParapetHelper.listOfParapetId_WithWallLine[indexOfExist]);
                }
            }
            return flagOfWallLineStretch;
        }
        private void addWallLineIdOfDoorCornerInsct(List<int> listOfAllIndex, Point3d basePointOfCorner, ObjectId cornerWallLineId, ObjectId cornerWallOffstLineId, double cornerWallLineLngth)
        {
            if (listOfAllIndex.Count == 0)
            {
                return;
            }

            bool flagOfWallLineStretch = getWallPaneLineChangeWithBeam(listOfAllIndex, cornerWallLineId, cornerWallOffstLineId);
            if (flagOfWallLineStretch == false)
            {
                return;
            }

            List<double> listOfMinLength = new List<double>();
            List<Point3d> listOfMinLengthPoint = new List<Point3d>();
            List<List<Point3d>> listOfListStrtEndPoint = new List<List<Point3d>>();
            List<ObjectId> listOfPrvsLineId = new List<ObjectId>();
            List<ObjectId> listOfPrvsOffstLineId = new List<ObjectId>();

            for (int k = 0; k < listOfAllIndex.Count; k++)
            {
                int indexOfExist = listOfAllIndex[k];
                var prvsWallPanlLineId = listOfLinesBtwTwoCrners_ObjId[indexOfExist];
                var prvsWallPanlOffstLineId = listOfOffsetLinesBtwTwoCrners_ObjId[indexOfExist];
                List<Point3d> listOfStrtEndPoint = AEE_Utility.GetStartEndPointOfLine(prvsWallPanlLineId);

                List<double> listOfLength = new List<double>();
                for (int m = 0; m < listOfStrtEndPoint.Count; m++)
                {
                    var point = listOfStrtEndPoint[m];
                    var length = AEE_Utility.GetLengthOfLine(point, basePointOfCorner);
                    listOfLength.Add(length);
                }
                double minLngth = listOfLength.Min();
                int indexOfMinLngth = listOfLength.IndexOf(minLngth);
                Point3d minLngthPoint = listOfStrtEndPoint[indexOfMinLngth];
                listOfMinLength.Add(minLngth);
                listOfMinLengthPoint.Add(minLngthPoint);
                listOfListStrtEndPoint.Add(listOfStrtEndPoint);
                listOfPrvsLineId.Add(prvsWallPanlLineId);
                listOfPrvsOffstLineId.Add(prvsWallPanlOffstLineId);
            }

            double minLength = listOfMinLength.Min();
            int indexOfMinLength = listOfMinLength.IndexOf(minLength);
            Point3d minLengthPoint = listOfMinLengthPoint[indexOfMinLength];
            ObjectId wallPanelLineId = listOfPrvsLineId[indexOfMinLength];
            ObjectId wallPanelOffstLineId = listOfPrvsOffstLineId[indexOfMinLength];

            List<Point3d> listOfStartEndPoint = listOfListStrtEndPoint[indexOfMinLength];
            int indexOfMinLngthPoint = listOfStartEndPoint.IndexOf(minLengthPoint);
            Point3d startPoint = basePointOfCorner;
            Point3d endPoint = new Point3d();
            if (indexOfMinLngthPoint == 0)
            {
                endPoint = listOfStartEndPoint[1];
            }
            else
            {
                endPoint = listOfStartEndPoint[0];
            }

            changeLinePoint(wallPanelLineId, startPoint, endPoint);

            ObjectId newWallPanelOffstLineId = new ObjectId();
            ObjectId offsetLineId = AEE_Utility.OffsetLine(wallPanelLineId, 20, false);
            var dist = AEE_Utility.GetDistanceBetweenTwoLines(wallPanelOffstLineId, offsetLineId);
            if (dist <= CommonModule.panelDepth)
            {
                newWallPanelOffstLineId = AEE_Utility.OffsetLine(wallPanelLineId, CommonModule.panelDepth, false);
            }
            else
            {
                newWallPanelOffstLineId = AEE_Utility.OffsetLine(wallPanelLineId, -CommonModule.panelDepth, false);
            }

            List<Point3d> listOfOffstStrtEndPoint = AEE_Utility.GetStartEndPointOfLine(newWallPanelOffstLineId);
            changeLinePoint(wallPanelOffstLineId, listOfOffstStrtEndPoint[0], listOfOffstStrtEndPoint[1]);

            AEE_Utility.deleteEntity(newWallPanelOffstLineId);
            AEE_Utility.deleteEntity(offsetLineId);
        }

        public void changeLinePoint(ObjectId lineId, Point3d startPoint, Point3d endPoint)
        {
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            try
            {
                using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
                {
                    Entity ent = tr.GetObject(lineId, OpenMode.ForWrite) as Entity;
                    if (ent is Line)
                    {
                        Line line = ent as Line;
                        line.StartPoint = startPoint;
                        line.EndPoint = endPoint;
                    }
                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
        }

        //Added by SDM 2022-07-28
        public static void GetPreviousNextWallName(string name, out string nextWall, out string prevWall)
        {
            nextWall = "";
            prevWall = "";
            for (int idx = 0; idx < InternalWallHelper.listOfListOfInternalWallRoomLineId_With_WallName.Count; idx++)
            {
                List<ObjectId> listOfInternalWallLineId = InternalWallHelper.listOfListOfInternalWallRoomLineId_With_WallName[idx];
                string wallname = AEE_Utility.GetXDataRegisterAppName(listOfInternalWallLineId[0]);
                if (listOfInternalWallLineId.Count != 0 && wallname.Split('_')[2] == name.Split('_')[2])
                {
                    int w = int.Parse(name.Split('_')[0].Replace("W", ""));
                    nextWall = "W" + (listOfInternalWallLineId.Count == w ? 1 : w + 1) + name.Replace(name.Split('_')[0], "");
                    prevWall = "W" + (w == 1 ? listOfInternalWallLineId.Count : w - 1) + name.Replace(name.Split('_')[0], "");
                    break;
                }
            }
        }

        public static Line GetInternalWallLineByxDataRegAppName(string xDataName)
        {
            for (int i = 0; i < InternalWallHelper.listOfListOfInternalWallRoomLineId_With_WallName.Count; i++)
            {
                for (int j = 0; j < InternalWallHelper.listOfListOfInternalWallRoomLineId_With_WallName[i].Count; j++)
                {
                    var name = AEE_Utility.GetXDataRegisterAppName(InternalWallHelper.listOfListOfInternalWallRoomLineId_With_WallName[i][j]);
                    if (name == xDataName)
                    {
                        return AEE_Utility.GetLine(InternalWallHelper.listOfListOfInternalWallRoomLineId_With_WallName[i][j]);

                    }
                }
            }
            return new Line();
        }
        public static Line GetInternalBaseLineByxDataRegAppName(string xDataName)
        {
            for (int i = 0; i < InternalWallHelper.listOfLinesBtwTwoCrners_ObjId.Count; i++)
            {

                var name = AEE_Utility.GetXDataRegisterAppName(InternalWallHelper.listOfLinesBtwTwoCrners_ObjId[i]);
                if (name == xDataName)
                {
                    return AEE_Utility.GetLine(InternalWallHelper.listOfLinesBtwTwoCrners_ObjId[i]);

                }
            }
            return new Line();
        }
    }

    public class CornerData
    {
        public Point3d ptCorner { get; set; }

        public double angleOfLine { get; set; }

        public double Length1 { get; set; }
        public double Length2 { get; set; }
        public Point3d EndPoint2 { get; private set; }
        public Point3d EndPoint1 { get; private set; }

        public CornerData(Point3d pt1, Point3d pt2)
        {
            this.EndPoint1 = pt1;
            this.EndPoint2 = pt2;
        }

        public ObjectId idCorner { get; set; }
    }
}
