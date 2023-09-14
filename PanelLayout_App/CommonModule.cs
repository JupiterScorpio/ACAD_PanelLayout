using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using static PanelLayout_App.RotationHelper;
using PanelLayout_App.Licensing;
using System.Windows.Forms;
using PanelLayout_App.ElevationModule;

namespace PanelLayout_App
{
    public class CommonModule
    {
        public static string symbolOfUnderScore = "_";  
        public static string xDataAsciiName = "PanelLayout";

        public static string nonStandardPanelLayerName = "AEE_Hightlight";
        public static int nonStandardPanelLayerColor = 1;

        public static string headerText_messageBox = "Panel Layout";
        public static string newDimensionStyleName = "AEE_Panel";      

        public static double rotateAngle = 0;
        public static Point3d basePoint = new Point3d();
        public static string internalWallLayerName1 = "AEE_IC_Wall";
        public static string externalWallLayerName = "AEE_EC_Wall";
        public static string ductLayerName = "AEE_Duct"; 

        public static string internalCornerLyrName = "AEE_IC";
        public static string internalCornerTextLyrName = "AEE_IC_Text";
        public static int internalCornerLyrColor = 140;
        public static string wallPanelLyrName = "AEE_WP";
        public static string wallPanelTextLyrName = "AEE_WP_Text";
        public static int wallPanelLyrColor = 197;
        public static string externalCornerLyrName = "AEE_EC";
        public static string externalCornerTextLyrName = "AEE_EC_Text";
        public static int externalCornerLyrColor = 173;
        public static string eleWallPanelLyrName = "AEE_EP";
        public static string baselineMWallPanelLyrName = "AEE_FFL";
        public static string clineWallPanelLyrName_hor = "WP_H_CT_Line_STD";
        public static string clineWallPanelLyrName_hor_SP = "WP_H_CT_Line_SP";
        public static string clineWallPanelLyrName_ver = "WP_V_CT_Line_STD";
        public static string clineWallPanelLyrName_ver_SP = "WP_V_CT_Line_SP";
        public static string clineWallPanelLyrName_sj = "SJ_CT_Line_STD";
        public static int eleWallPanelLyrColor = 111;
        public static int WallPanelLyrColor_sp = 1;

        public static string beamInternalCornerText = "BIAX";
        public static string internalCornerText = "IAX";
        public static string internalCornerText2 = "IA";
        public static string externalCornerText = "EA";
        public static string kickerCornerText = "KC";
        public static string externalKickerCornerText = "EKC";
        public static string slabJointCornerText = "SJ1";
        public static string slabJointCornerConcaveText = "CSJ";
        public static string slabJointCornerConvexText = "ECSJ";

        public static string doorWindowThickTopPropText = "BHX";
        public static string windowThickBottomPropText = "WBH";
        public static string doorWindowBottomPanelName = "BBX";
        public static string beamThickTopCrnrText = "BCCX";
        public static string windowThickWallPanelText = "WSX";
        public static string doorThickWallPanelText = "DSX";
        public static string doorWindowThickTopCrnrText = "CCX";
        public static string windowThickBottomCrnrText = "LCX";


        public static string internalCornerDescp = "Internal Corner Angle";
        public static string externalCornerDescp = "External Corner Angle";
        public static string kickerCornerDescp = "Kicker Corner";
        public static string slabJointCornerDescp = "Slab Joint Corner";

        public static string kickerCornerLayerName = "AEE_KickerCorner";
        public static int kickerCornerLayerColor = 112;
        public static string kickerCornerTextLayerName = "AEE_KickerCorner_Text";

        public static string beamCornerLayerName = "AEE_OffsetBeamCorner";
        public static int beamCornerLayerColor = 112;
        public static string beamCornerTextLayerName = "AEE_OffsetBeamCorner_Text";

        public static string kickerWallPanelLayerName = "KP_CT_Line_STD";
        public static int kickerWallPanelLayerColor = 163;
        public static string kickerWallPanelTextLayerName = "AEE_Kicker_WP_Text";

        public static string slabJointCornerLayerName = "AEE_SlabJointCorner";
        public static int slabJointCornerLayerColor = 41;
        public static string slabJointCornerTextLayerName = "AEE_SlabJointCorner_Text";

        public static string slabJointWallPanelLayerName = "AEE_SlabJoint_WP";
        public static int slabJointWallPanelLayerColor = 150;
        public static string slabJointWallPanelTextLayerName = "AEE_SlabJoint_WP_Text";

        public static string LCXPanelHorWallLayerName = "LCX_H_CT_Line_STD";
        public static string LCXPanelVerWallLayerName = "LCX_H_CT_Line_STD";

        public static string CCXPanelHorWallLayerName = "CCX_H_CT_Line_STD";
        public static string CCXPanelVerWallLayerName = "CCX_H_CT_Line_STD";

        public static string deckPanelWallLayerName = "DP_CT_Line_STD";
        public static int deckPanelWallLayerColor = 173;
        public static string deckPanelWallTextLayerName = "AEE_DeckPanel_Text";

        public static string beamBottomWallPanelLayerName = "AEE_OffsetBeamBottom_WP";
        public static int beamBottomWallPanelLayerColor = 163;
        public static string beamBottomWallPanelTextLayerName = "AEE_OffsetBeamBottom_WP_Text";

        public static string layoutLayerName = "AEE_Layout_Text";
        public static int layoutLayerColor = 42;

        public static List<Vert_Cline_data> vert_cline_data = new List<Vert_Cline_data>();
        public static List<WP_Cline_data> wallpanel_pnts = new List<WP_Cline_data>();
        public static List<cornerpnt> corner_pnts = new List<cornerpnt>();
        public static List<kIcker_panel_CLine> kIcker_panel_CLine = new List<kIcker_panel_CLine>();
        public static List<kIcker_corner_CLine> kIcker_corner_CLine = new List<kIcker_corner_CLine>();
        public static List<slapjoint_corner_CLine> slapjoint_corner_CLine = new List<slapjoint_corner_CLine>();
        public static List<LCX_CCX_corner_CLine> LCX_CCX_corner_CLine = new List<LCX_CCX_corner_CLine>();

        public static List<slapjoint_CLine> slapjoint_CLine = new List<slapjoint_CLine>();
        public static List<Deck_panel_CLine> Deck_panel_CLine = new List<Deck_panel_CLine>();

        public static string wallPanelType = "Wall Panel";
        public static string wndowwallPanelType = "Window Wall Panel";
        public static string doorWallPanelType = "Door Wall Panel";
        public static string beamWallPanelType = "Beam Wall Panel";
        public static string kickerPanelType = "Kicker Wall Panel";
        public static string slabJointPanelType = "Slab Joint Wall Panel";
        public static string rockerPanelType = "Rocker Wall Panel";
        public static string parapetPanelType = "Parapet Wall Panel";
        public static string deckPanelType = "Deck Wall Panel";
        public static string beamBottomCornerType = "Beam Bottom Corner";
        public static string beamBottomWallPanelType = "Beam Bottom Wall Panel";
        public static string beamBottomPropPanelType = "Beam Bottom Prop Panel";
        public static string beamBottomInternalPanelType = "Beam Bottom Internal Corner";
        public static string beamBottomExternalPanelType = "Beam Bottom External Corner";

        public static double deckPanel_SF_Lngth = 300;
        public static double deckPanel_MSP1_StndrdLngth = 1000;
        public static double deckPanel_MSP1_MaxLngth = 1050;
        public static double deckPanel_MSP1_MinLngth = 200;
        public static double deckPanel_ESP1_StndrdLngth = 1000;
        public static double deckPanel_ESP1_MaxLngth = 1050;
        public static double deckPanel_ESP1_MinLngth = 200;

        public static double deckPanelGap = 100;
        public static double deckPanel_Span_StndrdLngth = 1200;
        public static double deckPanel_Span_MaxLngth = 1050;
        public static double deckPanel_Span_MaxLngthForSLTGT175 = 1200;
        public static double deckPanel_Span_MaxLngthForSLTLTOrEqual175 = 1400;
        public static double deckPanel_Span_MinLngth = 200;

        //----------------------------
        public static double deckPanelMinLength = 0;
        public static double deckPanelMinWidth = 0;
        //----------------------Added on 10/07/2023 by PRT

        public static double doorWindowPropInterval = 200;
        public static double doorPropInterval = 100;
        public static double beamPropInterval = 100;
        public static double propRadius = 30;
        public static double propOffset = 50;
        public static double distBtwBeamCrnrToWall = 150;
        public static double beamBttmCornerChangeFlange = 250;
        public static double beamBttmCornerFixedFlange = 400;
        public static double minOfBeamOffset = 65;
        public static double beamBottomAddValueOfIntrnalCrnr = 100;

        public static double bottomPanel_StndrdLngth = 1200;
        public static double bottomPanel_MaxLngth = 1500;
        public static double bottomPanel_MinLngth = 200;  


        public static double minPanelThick = 200;
        public static double maxPanelThick = 325;

        public static double externalCornerThick = 6;
        public static double internalCornerThick = 6;
        
        public static double internalCorner_MinLngth = 75;
        public static double internalCorner_MaxLngth = 225;
        public static double internalCorner = 100;
        public static double externalCorner =60;

#if WNPANEL //Added on 15/05/2023 by SLW This value controls the IC Horizontal Extension for door & window corners
        public static double bottomPanelCorner = 65;
#else
         public static double bottomPanelCorner = 25;
#endif


        public static double cornerRotationAngle = 45;
        public static double doorWindowTopCornerHt = 100;

        public static double panelDepth = 0;
        public static double intrnlCornr_Flange1 = 0;
        public static double intrnlCornr_Flange2 = 0;
        public static double intrnlCornr_Flange3 = 0;
        public static double extrnlCornr_Flange = 0;
        public static double wallPanelThickness = 0;

        public static double slabJointPanelDepth = 100;

        public static double kickerCornr_Flange1 = 400;
        public static double kickerCornr_Flange2 = 400;
        public static double minKickerCornrFlange = 200;
        public static double maxKickerCornrFlange =0; 
        public static double extendKickerCornrFlangeUpto = 550; //Change to 550 PBAC
        public static double maxLngthOfKickerPanel = 1200;
        public static double minLngthOfKickerPanel = 200;
        public static double minKickerPanelLineLength = 0;

        public static double minValueOfSunkanLevelDiffrce = 50;

        public static double angle_0 = 0;
        public static double angle_90 = 90;
        public static double angle_180 = 180;
        public static double angle_270 = 270;
        public static double angle_360 = 360;

        public static double intervalOfAngle = 5;
        public static int groupNumber = 1;
        public static string groupName = "AEE_PanelLayout_";

        public static double wallPanelTextHght = 30;

        public static string blockName = "AEE_Block_";
        public static string roomLayer = "AEE_Room";
        public static int roomLayerColor = 42;
        public static int blockCount = 1;

        public static List<string> textvalue = new List<string>();

        public static double maxCornerExtension = internalCorner_MaxLngth - internalCorner;

        public static void setCornerVariableValue()
        {
            CommonModule.intrnlCornr_Flange1 = CommonModule.internalCorner;
            CommonModule.intrnlCornr_Flange2 = CommonModule.internalCorner;
            CommonModule.intrnlCornr_Flange3 = CommonModule.externalCorner;
            CommonModule.extrnlCornr_Flange = CommonModule.externalCorner;
            CommonModule.wallPanelThickness = CommonModule.externalCorner;
            CommonModule.panelDepth = CommonModule.externalCorner;
            maxKickerCornrFlange = Math.Max(kickerCornr_Flange1, kickerCornr_Flange2);
            maxCornerExtension = internalCorner_MaxLngth - internalCorner;

            minKickerPanelLineLength = 2 * (minLngthOfKickerPanel - panelDepth);
        }

        public bool checkInternalCornerIntersectTheTwoLines(int index, ObjectId internalCornrId, List<ObjectId> listOfExplodeIds)
        {
            ObjectId prvsLineId = new ObjectId();
            int count = listOfExplodeIds.Count();
            if (index >= listOfExplodeIds.Count)
            {
                return false;
            }
            ObjectId currentLineId = listOfExplodeIds[index];
            if (index == 0)
            {
                prvsLineId = listOfExplodeIds[count - 1];
            }
            else
            {
                prvsLineId = listOfExplodeIds[index - 1];
            }

            int countOfInsct = 0;

            List<double> listOfCurrntLine_Points = AEE_Utility.getStartEndPointWithAngle_Line(currentLineId);
            double currntAngle = listOfCurrntLine_Points[4];
            var currntId = AEE_Utility.getLineId(listOfCurrntLine_Points[0], listOfCurrntLine_Points[1], 0, listOfCurrntLine_Points[2], listOfCurrntLine_Points[3], 0, false);
            AEE_Utility.changeColor(currntId, 10);

            List<double> listOfPrvsLine_Points = AEE_Utility.getStartEndPointWithAngle_Line(prvsLineId);
            double prvsAngle = listOfPrvsLine_Points[4];
            var prvsId = AEE_Utility.getLineId(listOfPrvsLine_Points[0], listOfPrvsLine_Points[1], 0, listOfPrvsLine_Points[2], listOfPrvsLine_Points[3], 0, false);
            AEE_Utility.changeColor(prvsId, 11);


            double angleBtwTwoLines = Math.Abs(currntAngle - prvsAngle);

            var listOfInsctPointFromCurrentLine = AEE_Utility.InterSectionBetweenTwoEntity(internalCornrId, currntId);
            if (listOfInsctPointFromCurrentLine.Count >= 1)
            {
                countOfInsct++;
            }

            var listOfInsctPointFromPrvsLine = AEE_Utility.InterSectionBetweenTwoEntity(internalCornrId, prvsId);
            if (listOfInsctPointFromPrvsLine.Count >= 1)
            {
                countOfInsct++;
            }

            AEE_Utility.deleteEntity(currntId);
            AEE_Utility.deleteEntity(prvsId);

            if (countOfInsct <= 1)
            {
                return false;
            }
            return true;
        }

        public void checkInternalCornerIsInside_Innerwall(string xDataRegAppName, ObjectId internalWallId, Point3d basePoint, double angleOfLine, ObjectId offsetInterwall_ObjId, out bool flagOfInside, out double rotationAngle)
        {
            flagOfInside = false;
            rotationAngle = 0;

            PartHelper partHelper = new PartHelper();

            List<double> listOfAngle = new List<double>();
            listOfAngle.Add(angleOfLine);
            listOfAngle.Add(angleOfLine + angle_90);
            listOfAngle.Add(angleOfLine + CommonModule.angle_180);
            listOfAngle.Add(angleOfLine + CommonModule.angle_270);

            int colorIndex = 10;
            for (int k = 0; k < listOfAngle.Count; k++)
            {
                var internalCornrId = partHelper.drawInternalCorner(xDataRegAppName, basePoint.X, basePoint.Y, intrnlCornr_Flange1, intrnlCornr_Flange2, intrnlCornr_Flange3,
                                                                    internalCornerThick, panelDepth, internalCornerLyrName, internalCornerLyrColor, listOfAngle[k]);
                AEE_Utility.AttachXData(internalCornrId, xDataRegAppName, xDataAsciiName);
                AEE_Utility.AttachXData(internalCornrId, xDataRegAppName + "_1", intrnlCornr_Flange1.ToString() + "," +
                                                                                 intrnlCornr_Flange2.ToString() + "," +
                                                                                 intrnlCornr_Flange3.ToString() + "," +
                                                                                 internalCornerThick.ToString() + "," +
                                                                                 panelDepth.ToString());
                double angle = listOfAngle[k];
                /*CommonModule.rotateAngle = (angle * Math.PI) / 180; // variable assign for rotation             
                CommonModule.basePoint = new Point3d(basePoint.X, basePoint.Y, 0);
                List<ObjectId> listOfObjId = new List<ObjectId>();
                listOfObjId.Add(internalCornrId);
                MoveRorationScaleJig.RotationScaleJig_Method(listOfObjId);*/

                //var colonId = AEE_Utility.createColonEntityInSamePoint(internalCornrId, true);
                //AEE_Utility.changeColor(colonId, 5);

                var id = internalCornrId; // listOfObjId[0];
                var listOfIntsct = AEE_Utility.TrimBetweenTwoClosedPolylines(offsetInterwall_ObjId, id);

                if (listOfIntsct.Count != 0)
                {
                    flagOfInside = true;
                    AEE_Utility.deleteEntity(internalCornrId);
                    AEE_Utility.deleteEntity(listOfIntsct);
                    rotationAngle = angle;
                    break;
                }
                else
                {
                    listOfIntsct = AEE_Utility.TrimBetweenTwoClosedPolylines(id, offsetInterwall_ObjId);
                    if (listOfIntsct.Count != 0)
                    {
                        flagOfInside = true;
                        AEE_Utility.deleteEntity(internalCornrId);
                        AEE_Utility.deleteEntity(listOfIntsct);
                        rotationAngle = angle;
                        break;
                    }
                    AEE_Utility.deleteEntity(listOfIntsct);
                    AEE_Utility.deleteEntity(internalCornrId);
                }

                AEE_Utility.changeColor(internalCornrId, colorIndex);
                colorIndex++;
            }
        }
        public void getRectangleWidthLength(double refWidthSideAngle, ObjectId beamId, out double Width, out double Length)
        {

            bool flagOfRefWidthSide_X_Axis = false;
            bool flagOfRefWidthSide_Y_Axis = false;
            checkAngleOfLine_Axis(refWidthSideAngle, out flagOfRefWidthSide_X_Axis, out flagOfRefWidthSide_Y_Axis);
            Width = 0;
            Length = 0;

            Entity panelRectEnt = AEE_Utility.GetEntityForRead(beamId);
            Point2d previousPoint = new Point2d();

            List<Point2d> listOfRectVertice = AEE_Utility.GetPolylineVertexPoint(beamId);
            for (int index = 0; index < listOfRectVertice.Count; index++)
            {
                Point2d crrntPoint = listOfRectVertice[index];
                Point2d nextPoint = new Point2d();
                if (index == 0)
                {
                    previousPoint = listOfRectVertice[listOfRectVertice.Count - 1];
                }


                if (index == (listOfRectVertice.Count - 1))
                {
                    nextPoint = listOfRectVertice[0];
                }
                else
                {
                    nextPoint = listOfRectVertice[index + 1];
                }

                var rectLineWidth = AEE_Utility.GetLengthOfLine(crrntPoint.X, crrntPoint.Y, nextPoint.X, nextPoint.Y);
                rectLineWidth = Math.Round(rectLineWidth);
                if (rectLineWidth > 0)
                {
                    var rectLineAngle = AEE_Utility.GetAngleOfLine(crrntPoint.X, crrntPoint.Y, nextPoint.X, nextPoint.Y);

                    bool flagOfRectLine_X_Axis = false;
                    bool flagOfRectLine_Y_Axis = false;
                    checkAngleOfLine_Axis(rectLineAngle, out flagOfRectLine_X_Axis, out flagOfRectLine_Y_Axis);

                    bool flagOfWidth = false;
                    if (flagOfRefWidthSide_X_Axis == flagOfRectLine_X_Axis)
                    {
                        flagOfWidth = true;
                    }
                    else if (flagOfRefWidthSide_Y_Axis == flagOfRectLine_Y_Axis)
                    {
                        flagOfWidth = true;
                    }

                    if (flagOfWidth == true)
                    {
                        Width = rectLineWidth;
                        var rectLineLength = AEE_Utility.GetLengthOfLine(crrntPoint.X, crrntPoint.Y, previousPoint.X, previousPoint.Y);
                        Length = Math.Round(rectLineLength);
                        break;
                    }
                }
                previousPoint = crrntPoint;
            }
        }


        public bool checkAngleBetweenTwoLines(int index, double currntAngle, List<ObjectId> listOfExplodeIds)
        {
            ObjectId prvsLineId = new ObjectId();

            int count = listOfExplodeIds.Count();
            if (index >= listOfExplodeIds.Count)
            {
                return false;
            }

            if (index == 0)
            {
                prvsLineId = listOfExplodeIds[count - 1];
            }
            else
            {
                prvsLineId = listOfExplodeIds[index - 1];
            }

            List<double> listOfPrvsLine_Points = AEE_Utility.getStartEndPointWithAngle_Line(prvsLineId);
            double prvsLineAngle = listOfPrvsLine_Points[4];

            double angleBtwTwoLines = Math.Round(Math.Abs(currntAngle - prvsLineAngle), 9);
            if (angleBtwTwoLines == angle_90)
            {
                return true;
            }
            else if (angleBtwTwoLines == angle_270)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public void checkExternalCornerIsInside_Innerwall(string xDataRegAppName, ObjectId internalWallId, Point3d basePoint, double angleOfLine, ObjectId offsetInterwall_ObjId, int index, List<ObjectId> listOfOffsetWall_Ids, out bool flagOfExternalDwg, out double rotationAngle)
        {
            flagOfExternalDwg = false;
            rotationAngle = 0;

            PartHelper partHelper = new PartHelper();

            List<double> listOfAngle = new List<double>();
            listOfAngle.Add(angleOfLine);
            listOfAngle.Add(angleOfLine + angle_90);
            listOfAngle.Add(angleOfLine + CommonModule.angle_180);
            listOfAngle.Add(angleOfLine + CommonModule.angle_270);

            //int colorIndex = 10;
            for (int k = 0; k < listOfAngle.Count; k++)
            {
                var externalCornerId = partHelper.drawExternalCorner(xDataRegAppName, basePoint.X, basePoint.Y, CommonModule.panelDepth, CommonModule.externalCornerThick, CommonModule.externalCornerLyrName, CommonModule.externalCornerLyrColor);
                AEE_Utility.AttachXData(externalCornerId, xDataRegAppName, CommonModule.xDataAsciiName);
                AEE_Utility.AttachXData(externalCornerId, xDataRegAppName + "_1", panelDepth.ToString() + "," +
                                                                                  externalCornerThick.ToString());

                double angle = listOfAngle[k];
                CommonModule.rotateAngle = (angle * Math.PI) / 180; // variable assign for rotation             
                CommonModule.basePoint = new Point3d(basePoint.X, basePoint.Y, 0);
                List<ObjectId> listOfObjId = new List<ObjectId>();
                listOfObjId.Add(externalCornerId);
                MoveRorationScaleJig.RotationScaleJig_Method(listOfObjId);

                var flag = checkExternalCornerIntersectTheTwoLines(index, externalCornerId, listOfOffsetWall_Ids);
                if (flag == false)
                {
                    AEE_Utility.deleteEntity(externalCornerId);
                }
                else
                {
                    AEE_Utility.deleteEntity(externalCornerId);
                    flagOfExternalDwg = flag;
                    rotationAngle = angle;
                    break;
                }

                //AEE_Utility.changeColor(externalCornerId, colorIndex);
                //colorIndex++;
            }
        }

        public bool checkExternalCornerIntersectTheTwoLines(int index, ObjectId internalCornrId, List<ObjectId> listOfExplodeIds)
        {
            ObjectId prvsLineId = new ObjectId();
            int count = listOfExplodeIds.Count();
            if (index >= listOfExplodeIds.Count)
            {
                return false;
            }
            ObjectId currentLineId = listOfExplodeIds[index];
            if (index == 0)
            {
                prvsLineId = listOfExplodeIds[count - 1];
            }
            else
            {
                prvsLineId = listOfExplodeIds[index - 1];
            }

            int countOfInsct = 0;

            List<double> listOfCurrntLine_Points = AEE_Utility.getStartEndPointWithAngle_Line(currentLineId);
            var currntId = AEE_Utility.getLineId(listOfCurrntLine_Points[0], listOfCurrntLine_Points[1], 0, listOfCurrntLine_Points[2], listOfCurrntLine_Points[3], 0, false);
            //AEE_Utility.changeColor(currntId, 10);

            List<double> listOfPrvsLine_Points = AEE_Utility.getStartEndPointWithAngle_Line(prvsLineId);
            var prvsId = AEE_Utility.getLineId(listOfPrvsLine_Points[0], listOfPrvsLine_Points[1], 0, listOfPrvsLine_Points[2], listOfPrvsLine_Points[3], 0, false);
            //AEE_Utility.changeColor(prvsId, 11);

            var listOfInsctPointFromCurrentLine = AEE_Utility.InterSectionBetweenTwoEntity(internalCornrId, currntId);
            if (listOfInsctPointFromCurrentLine.Count >= 1)
            {
                countOfInsct++;
            }

            var listOfInsctPointFromPrvsLine = AEE_Utility.InterSectionBetweenTwoEntity(internalCornrId, prvsId);
            if (listOfInsctPointFromPrvsLine.Count >= 1)
            {
                countOfInsct++;
            }

            AEE_Utility.deleteEntity(currntId);
            AEE_Utility.deleteEntity(prvsId);

            if (countOfInsct <= 1)
            {
                return false;
            }
            return true;
        }

        public void checkAngleOfLine_Axis(double lineAngle, out bool flag_X_Axis, out bool flag_Y_Axis)
        {
            flag_X_Axis = false;
            flag_Y_Axis = false;

            if (lineAngle >= (CommonModule.angle_90 - CommonModule.intervalOfAngle) && lineAngle <= (CommonModule.angle_90 + CommonModule.intervalOfAngle))
            {
                flag_Y_Axis = true;
                flag_X_Axis = false;
            }
            else if (lineAngle >= (CommonModule.angle_270 - CommonModule.intervalOfAngle) && lineAngle <= (CommonModule.angle_270 + CommonModule.intervalOfAngle))
            {
                flag_Y_Axis = true;
                flag_X_Axis = false;
            }
            else
            {
                // X- Axis is true
                flag_X_Axis = true;
                flag_Y_Axis = false;
            }
        }
        public List<Point3d> getStartEndPointOfLine(ObjectId lineId)
        {
            Line acLine = AEE_Utility.GetLine(lineId);

            List<Point3d> listOfStrtEndPoint = getStartEndPointOfLine(acLine);

            return listOfStrtEndPoint;
        }

        public List<Point3d> getStartEndPointOfLine(Line acLine)
        {
            List<Point3d> listOfStrtEndPoint = new List<Point3d>();
            double lineAngle = AEE_Utility.GetAngleOfLine(acLine);

            bool flag_X_Axis = true;
            bool flag_Y_Axis = false;

            checkAngleOfLine_Axis(lineAngle, out flag_X_Axis, out flag_Y_Axis);

            Point3d strtPnt = acLine.StartPoint;
            Point3d endPnt = acLine.EndPoint;

            Point3d startPoint = new Point3d();
            Point3d endPoint = new Point3d();

            if (flag_X_Axis == true)
            {
                if (strtPnt.X > endPnt.X)
                {
                    startPoint = endPnt;
                    endPoint = strtPnt;
                }
                else
                {
                    startPoint = strtPnt;
                    endPoint = endPnt;
                }
            }
            else if (flag_Y_Axis == true)
            {
                if (strtPnt.Y > endPnt.Y)
                {
                    startPoint = endPnt;
                    endPoint = strtPnt;
                }
                else
                {
                    startPoint = strtPnt;
                    endPoint = endPnt;
                }
            }
            listOfStrtEndPoint.Add(startPoint);
            listOfStrtEndPoint.Add(endPoint);

            return listOfStrtEndPoint;
        }

        public void createGroup(List<ObjectId> listOfObjectId)
        {
            //string grpName = groupName + Convert.ToString(groupNumber);
            //var flagOfGroup = AEE_Utility.CreateGroup(grpName, listOfObjectId);
            //groupNumber++;   
        }

        public bool checkLicenseFile()
        {
            if (CommandModule.licFileNameWithPath == "")
            {
                var appPath = LicenseUtilities.getAppPath();
                CommandModule.licFileNameWithPath = appPath + LicenseUtilities.licenseFileName;
            }

            if (MacAddressUtility.IsLicenseCheckNeeded())
            {
                return true;
            }
            else
            {
                LicenseData licData = Licensing.Utilities.Deserialize<LicenseData>(CommandModule.licFileNameWithPath, true);
                if (licData == null)
                {
                    string msg1 = "License file does not exist. ";
                    msg1 = msg1 + LicenseUtilities.lnceErrorMsg;
                    System.Windows.Forms.MessageBox.Show(msg1, LicenseUtilities.licenseErrorMsg, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                else
                {
                    if (LicenseUtilities.IsValidLicense(CommandModule.licFileNameWithPath) < 0)
                    {
                        return false;
                    }
                }
            }     
            return true;
        }
    }
}
