﻿using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using PanelLayout_App.CivilModel;
using PanelLayout_App.ElevationModule;
using PanelLayout_App.WallModule;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PanelLayout_App.WallModule
{
    public class WallPanelHelper
    {
        public static List<ObjectId> listOfAllWallPanelRectId = new List<ObjectId>();
        public static List<ObjectId> listOfAllWallPanelRectTextId = new List<ObjectId>();
        public static List<ObjectId> listOfAllWallPanelLineId = new List<ObjectId>();
        public static List<string> listOfAllWallPanelData = new List<string>();

        //public static List<Line> listOfExistsClosestWallPanelLine = new List<Line>();
        //public static List<ObjectId> listOfExistsClosestWallPanelLine_ObjId = new List<ObjectId>();

        //public static List<Line> listOfExistsBaseWallPanelLine = new List<Line>();
        //public static List<ObjectId> listOfExistsBaseWallPanelLine_ObjId = new List<ObjectId>();

        public static List<Line> listOfAllExistsWallPanelLine = new List<Line>();
        //public static List<ObjectId> listOfAllExistsWallPanelLine_ObjId = new List<ObjectId>();

       public static List<Line> listOfAllWallLines = new List<Line>();

        private static List<List<ObjectId>> listOfListClosestLineWallPanelRect_ObjId = new List<List<ObjectId>>();
        private static List<List<ObjectId>> listOfListClosestLineWallPanelText_ObjId = new List<List<ObjectId>>();
        private static List<List<ObjectId>> listOfListClosestLineWallXText_ObjId = new List<List<ObjectId>>();

        private static List<List<ObjectId>> listOfListBaseLineWallPanelRect_ObjId = new List<List<ObjectId>>();
        private static List<List<ObjectId>> listOfListBaseLineWallPanelText_ObjId = new List<List<ObjectId>>();
        private static List<List<ObjectId>> listOfListBaseLineWallXText_ObjId = new List<List<ObjectId>>();

        private static List<List<List<ObjectId>>> listOfListOfListWallPanelId_ForStretch = new List<List<List<ObjectId>>>();
        private static List<ObjectId> listOfWallOffsetCrnrId_ForCheck_ForStretch = new List<ObjectId>();

        private static List<ObjectId> listOfClosestLineObj_With_Index_ForGroup = new List<ObjectId>();

        private static List<ObjectId> listOfStretchWallPanelCornerAndTextId = new List<ObjectId>();

        private static Dictionary<Point3d, Point3d> dicModifiedPoints = new Dictionary<Point3d, Point3d>(new PointComparer());

        internal static Dictionary<ObjectId, string> listOfDoorLineIdAndLayerName = new Dictionary<ObjectId, string>();//RTJ 10-06-2021
        internal static Dictionary<ObjectId, string> listOfBeamLineIdAndLayerName = new Dictionary<ObjectId, string>();//Added on 10/03/2023 by SDM
        internal static Dictionary<ObjectId, string> listOfWindowLineIdAndLayerName = new Dictionary<ObjectId, string>(); //RTJ 10-06-2021

        private void clearList()
        {
            listOfStretchWallPanelCornerAndTextId.Clear();
            dicModifiedPoints.Clear();
            //listOfExistsClosestWallPanelLine.Clear();
            //listOfExistsClosestWallPanelLine_ObjId.Clear();
            //listOfExistsBaseWallPanelLine.Clear();
            //listOfExistsBaseWallPanelLine_ObjId.Clear();
            listOfAllExistsWallPanelLine.Clear();
            //listOfAllExistsWallPanelLine_ObjId.Clear();
            listOfAllWallLines.Clear();
            listOfListClosestLineWallPanelRect_ObjId.Clear();
            listOfListBaseLineWallPanelRect_ObjId.Clear();
            listOfClosestLineObj_With_Index_ForGroup.Clear();
            listOfListClosestLineWallPanelText_ObjId.Clear();
            listOfListClosestLineWallXText_ObjId.Clear();
            listOfListBaseLineWallPanelText_ObjId.Clear();
            listOfListBaseLineWallXText_ObjId.Clear();
            listOfAllWallPanelRectId.Clear();
            listOfAllWallPanelRectTextId.Clear();
            listOfAllWallPanelLineId.Clear();
            listOfAllWallPanelData.Clear();
            listOfListOfListWallPanelId_ForStretch.Clear();
            listOfWallOffsetCrnrId_ForCheck_ForStretch.Clear();
            listOfDoorLineIdAndLayerName.Clear();//RTJ 10-06-2021
            listOfWindowLineIdAndLayerName.Clear(); //RTJ 10-06-2021
            listOfBeamLineIdAndLayerName.Clear();//Added on 10/03/2023 by SDM
        }
        public void callMethodOfWallPanel()
        {
            RibbonHelper.insertPanelButton.IsEnabled = false;

            clearList();

            string wallCreationMsg = "Wall panels are creating....";
            ProgressForm progressForm = new ProgressForm();
            progressForm.Show();

            getWallPanels(progressForm, wallCreationMsg);
            if (PanelLayout_UI.flagOfExtendPanelAtExternalCornersOption) //Added by SDM 2022-08-06
            {

                AEE_Utility.deleteEntity(DoorHelper.listOfCornerIfDoorXHeightIsLessThanZero);
                AEE_Utility.deleteEntity(CornerHelper.listOfOffstExternalCornerId_ForStretch);
                AEE_Utility.deleteEntity(listOfStretchWallPanelCornerAndTextId);
            }
            //Added by SDM 20/03/2023
            AEE_Utility.deleteEntity(CornerHelper.listOfCornerId_InsctToBeam);
            AEE_Utility.deleteEntity(CornerHelper.listOfCornerTextId_InsctToBeam.SelectMany(X => X).ToList());


            wallCreationMsg = "Wall panel are created.";
            progressForm.ReportProgress(100, wallCreationMsg);
            progressForm.Close();

            if (!CommandModule.dply)
                MessageBox.Show(wallCreationMsg, CommonModule.headerText_messageBox, MessageBoxButtons.OK, MessageBoxIcon.Information);

            if (CheckShellPlanHelper.slabJointCount_ForLayoutCreator != 0)
            {
                //Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.SendStringToExecute((String)RibbonHelper.insertDeckPanelButton.CommandParameter, true, false, true);
                RibbonHelper.insertDeckPanelButton.IsEnabled = true;
            }
            else
            {
                //Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.SendStringToExecute((String)RibbonHelper.rebuildPanelButton.CommandParameter, true, false, true);
                RibbonHelper.rebuildPanelButton.IsEnabled = true;
                RibbonHelper.updateWallButton.IsEnabled = true;//Added on 10/06/2023 by SDM
            }
        }
        private void getWallPanels(ProgressForm progressForm, string wallCreationMsg)
        {

            CommonModule.wallpanel_pnts = new List<WP_Cline_data>();
            if (InternalWallHelper.listOfLinesBtwTwoCrners_ObjId.Count != 0)
            {
                foreach (var lineId in InternalWallHelper.listOfLinesBtwTwoCrners_ObjId)
                {
                    listOfAllWallLines.Add(AEE_Utility.GetLine(lineId));

                    //System.Diagnostics.Debug.WriteLine(listOfAllWallLines.Count +  " -- " + listOfAllWallLines[listOfAllWallLines.Count - 1].Handle);
                }
                progressForm.ReportProgress(1, wallCreationMsg);

                WindowHelper winHelp = new WindowHelper();

                for (int index = 0; index < InternalWallHelper.listOfLinesBtwTwoCrners_ObjId.Count; index++)
                {
                    if ((index % 20) == 0)
                    {
                        progressForm.ReportProgress(1, wallCreationMsg);
                    }

                    ObjectId baseLineId = InternalWallHelper.listOfLinesBtwTwoCrners_ObjId[index];
                    var baseLine = AEE_Utility.GetLine(baseLineId);

                    //Testing - 123
                    string wallName = AEE_Utility.GetXDataRegisterAppName(baseLineId);
                    //List<string> wallNames = new List<string>() { "W9_EX_1" };

                    //if (string.IsNullOrWhiteSpace(wallName))
                    //{
                    //    MessageBox.Show("Empty WallName");
                    //}

                    //{ "W12_EX_1", "W10_EX_1", "W4_R_20", "W3_LF_10", "W1_R_20", "W7_EX_1", "W3_R_21", "W12_EX_1", "W4_R_20", "W3_R_10", "W3_R_6", "W12_R_6", "W2_R_17", "W2_R_15", "W8_R_13", "W18_EX_1", "W28_EX_1" };

                    //if (wallNames.Contains(wallName) || string.IsNullOrWhiteSpace(wallName))
                    //{
                    //    AEE_Utility.ZoomObj = true;
                    //    AEE_Utility.ZoomToEntity(baseLineId);
                    //    AEE_Utility.DCircle(baseLine.StartPoint);
                    //    AEE_Utility.DCircle(baseLine.EndPoint);
                    //}


                    string beamLayerNameInsctToBaseLine = BeamHelper.listOfLinesBtwTwoCrners_BeamInsctName[index];

                    ObjectId beamId_BaseLine = BeamHelper.listOfLinesBtwTwoCrners_BeamInsctId[index];

                    double distanceBtwWallToBeam_BaseLine = BeamHelper.listOfDistanceBtwWallToBeam[index];

                    ObjectId nearestBeamWallLineId_BaseLine = BeamHelper.listOfNearestBtwWallToBeamWallLineId[index];

                    bool flagOfSunkanSlab_BaseLine = SunkanSlabHelper.listOfLinesBtwTwoCrners_SunkanSlabWallLineflag[index];
                    ObjectId sunkanSlabId_BaseLine = SunkanSlabHelper.listOfLinesBtwTwoCrners_SunkanSlabId[index];

                    ObjectId parapetObjId_BaseLine = ParapetHelper.listOfParapetId_WithWallLine[index];

                    Point3d startPoint = baseLine.StartPoint;
                    Point3d endPoint = baseLine.EndPoint;
                    if (!listOfAllExistsWallPanelLine.Contains(baseLine) 
                        && !Beam_UI_Helper.checkBeamLayerIsExist(beamLayerNameInsctToBaseLine))//Changes made on 03/04/2023 by SDM
                    {

                        List<Line> listOfClosestLine = GetAllClosestLines_In_SameDirection(baseLine, listOfAllWallLines);

                        if (listOfClosestLine.Count != 0)
                        {
                            drawAllWallPanel(beamLayerNameInsctToBaseLine, beamId_BaseLine, distanceBtwWallToBeam_BaseLine, nearestBeamWallLineId_BaseLine, baseLineId, baseLine, listOfClosestLine, flagOfSunkanSlab_BaseLine, sunkanSlabId_BaseLine, parapetObjId_BaseLine);
                        }
                        else
                        {
                            drawBaseLineWall_IfClosestLineCountIsZero(beamLayerNameInsctToBaseLine, beamId_BaseLine, distanceBtwWallToBeam_BaseLine, nearestBeamWallLineId_BaseLine, baseLineId, baseLine, flagOfSunkanSlab_BaseLine, sunkanSlabId_BaseLine, parapetObjId_BaseLine);
                        }

                        Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor.UpdateScreen();

                    }
                }
                if (PanelLayout_UI.flagOfExtendPanelAtExternalCornersOption) //Added by SDM 2022-08-06
                    stretchWallPanelRect_In_ConcaveAngle();
                
                BeamHelper beamHlp = new BeamHelper();
                beamHlp.drawBeamBottomPanel(progressForm, wallCreationMsg);

                beamHlp.drawBeamSidePanel(progressForm, wallCreationMsg);


                var dicCornerPoints = WindowHelper.FindCorners();
                CommonModule.LCX_CCX_corner_CLine = new List<LCX_CCX_corner_CLine>();

                WindowHelper windowHlp = new WindowHelper();
                windowHlp.drawWindowPanels(progressForm, wallCreationMsg, dicCornerPoints);

                DoorHelper doorHlp = new DoorHelper();
                doorHlp.drawDoorPanels(progressForm, wallCreationMsg, dicCornerPoints);

                ParapetHelper parapetHlp = new ParapetHelper();
                parapetHlp.drawParapetPanel_With_Door();
                parapetHlp.drawDrawPanelInParapetThickWall(progressForm, wallCreationMsg);

                KickerHelper kickerHlp = new KickerHelper();
                kickerHlp.drawKickerWallPanel(progressForm, wallCreationMsg, DoorHelper.listDoorLines);

                SlabJointHelper slabJointHlp = new SlabJointHelper();
                slabJointHlp.drawSlabJointWallPanel(progressForm, wallCreationMsg);
            }
        }

        public static List<Line> GetAllClosestLines_In_SameDirection(Line baseLine, List<Line> listOfLines)
        {
            List<Line> listOfClosestLine = new List<Line>();

            List<ObjectId> listOfOffsetBaseLine_ObjId = getOffsetLineId(baseLine);
            var offsetBaseLineId1 = listOfOffsetBaseLine_ObjId[0];
            var offsetBaseLineId2 = listOfOffsetBaseLine_ObjId[1];

            //foreach (var line in listOfLines)
            //{
            //    MessageBox.Show(line.Layer);
            //}


            foreach (var line in listOfLines)
            {
                if (!listOfAllExistsWallPanelLine.Contains(line))
                {
                    //Testing-123
                    //string wallName = AEE_Utility.GetXDataRegisterAppName(line.ObjectId);
                    //List<string> wallNames = new List<string>() { "W2_LF_16", "W2_LF_9", "W2_LF_20", "W2_LF_5", "W2_R_1" }; // For "W12_R_6" baseline

                    //if (wallNames.Contains(wallName))
                    //{
                    //    AEE_Utility.ZoomObj = true;
                    //    AEE_Utility.ZoomToEntity(line.ObjectId);
                    //    AEE_Utility.DCircle(line.StartPoint);
                    //    AEE_Utility.DCircle(line.EndPoint);
                    //}

                    var flagOfBaseLineAngle = checkAngleOfBaseLine(baseLine.ObjectId, line.ObjectId);
                    if (line != baseLine && flagOfBaseLineAngle == true)
                    {
                        var dist = AEE_Utility.GetDistanceBetweenTwoLines(baseLine.ObjectId, line.ObjectId);

                        if (dist <= PanelLayout_UI.maxWallThickness)
                        {
                            bool flagOfInsct = checkBaseLine_Is_Intersect(baseLine, line, offsetBaseLineId1, offsetBaseLineId2);
                            if (flagOfInsct == true)
                            {
                                listOfClosestLine.Add(line);
                            }
                        }
                    }
                }
            }

            AEE_Utility.deleteEntity(listOfOffsetBaseLine_ObjId);

            return listOfClosestLine;
        }

        public static bool checkAngleOfBaseLine(ObjectId baseLineId, ObjectId prllel_LineId)
        {
            // parallel condition false, because angle of base and parallel line is not equal. So get difference of both angle.
            // check difference angle is less than interval of angle. If condition is false, substract 180 - difference angle.

            double baseLineAngle = AEE_Utility.GetAngleOfLine(baseLineId);
            double prllelLineAngle = AEE_Utility.GetAngleOfLine(prllel_LineId);

            if (prllelLineAngle == 360)
            {
                prllelLineAngle = 0;
            }
            if (baseLineAngle == 360)
            {
                baseLineAngle = 0;
            }

            double diffAngle = Math.Abs(baseLineAngle - prllelLineAngle);
            if (diffAngle <= CommonModule.intervalOfAngle)
            {
                return true;
            }
            else
            {
                double diffAngleWith_180 = Math.Abs(180 - diffAngle);
                if (diffAngleWith_180 <= CommonModule.intervalOfAngle)
                {
                    return true;
                }
            }

            return false;
        }
        public static bool checkBaseLine_Is_Intersect(Line baseLine, Line prllel_Line, ObjectId offsetBaseLineId1, ObjectId offsetBaseLineId2)
        {
            // create offset lines of base and parallel. check offset base line is intersect to parallel line.
            bool flagOfInsct = false;

            var listOfBaseLineInsctPoint1 = AEE_Utility.InterSectionBetweenTwoEntity(offsetBaseLineId1, prllel_Line.ObjectId);
            var listOfBaseLineInsctPoint2 = AEE_Utility.InterSectionBetweenTwoEntity(offsetBaseLineId2, prllel_Line.ObjectId);
            if (listOfBaseLineInsctPoint1.Count == 0 && listOfBaseLineInsctPoint2.Count == 0)
            {
                // check offset parallel line is intersect to base line.
                List<ObjectId> listOfOffsetPrllelLine_ObjId = getOffsetLineId(prllel_Line);
                var offsetPrllelLineId1 = listOfOffsetPrllelLine_ObjId[0];
                var offsetPrllelLineId2 = listOfOffsetPrllelLine_ObjId[1];

                var listOfPrllelLineInsctPoint1 = AEE_Utility.InterSectionBetweenTwoEntity(offsetPrllelLineId1, baseLine.ObjectId);
                var listOfPrllelILinensctPoint2 = AEE_Utility.InterSectionBetweenTwoEntity(offsetPrllelLineId2, baseLine.ObjectId);

                AEE_Utility.deleteEntity(listOfOffsetPrllelLine_ObjId);
                if (listOfPrllelLineInsctPoint1.Count != 0)
                {
                    flagOfInsct = true;
                }
                if (listOfPrllelLineInsctPoint1.Count != 0)
                {
                    flagOfInsct = true;
                }
            }
            else
            {
                flagOfInsct = true;
            }
            return flagOfInsct;
        }

        public static List<ObjectId> getOffsetLineId(Line baseLine)
        {
            List<ObjectId> listOfOffset_ObjId = new List<ObjectId>();

            var offset_In_Pstive = AEE_Utility.OffsetLineEntity(baseLine, PanelLayout_UI.maxWallThickness);
            var offset_In_Ngtive = AEE_Utility.OffsetLineEntity(baseLine, (-PanelLayout_UI.maxWallThickness));

            List<double> listOfOffsetPstive_LinePoint = AEE_Utility.GetStartEndPointOfLine_WithEntity(offset_In_Pstive);
            List<double> listOfOffsetNgtive_LinePoint = AEE_Utility.GetStartEndPointOfLine_WithEntity(offset_In_Ngtive);

            var offsetLineId1 = AEE_Utility.getLineId(listOfOffsetPstive_LinePoint[0], listOfOffsetPstive_LinePoint[1], 0, listOfOffsetNgtive_LinePoint[0], listOfOffsetNgtive_LinePoint[1], 0, false);
            var offsetLineId2 = AEE_Utility.getLineId(listOfOffsetPstive_LinePoint[2], listOfOffsetPstive_LinePoint[3], 0, listOfOffsetNgtive_LinePoint[2], listOfOffsetNgtive_LinePoint[3], 0, false);
            listOfOffset_ObjId.Add(offsetLineId1);
            listOfOffset_ObjId.Add(offsetLineId2);

            return listOfOffset_ObjId;
        }


        private void drawAllWallPanel(string beamLayerNameInsctToBaseLine, ObjectId beamId_BaseLine, double distanceBtwWallToBeam_BaseLine, ObjectId nearestBeamWallLineId_BaseLine, ObjectId baseLineId, Line baseLine, List<Line> listOfClosestLine, bool flagOfSunkanSlab_BaseLine, ObjectId sunkanSlabId_BaseLine, ObjectId parapetObjId_BaseLine)
        {
            listOfListClosestLineWallPanelRect_ObjId.Clear();
            listOfListBaseLineWallPanelRect_ObjId.Clear();
            listOfClosestLineObj_With_Index_ForGroup.Clear();
            listOfListClosestLineWallPanelText_ObjId.Clear();
            listOfListClosestLineWallXText_ObjId.Clear();
            listOfListBaseLineWallPanelText_ObjId.Clear();
            listOfListBaseLineWallXText_ObjId.Clear();


            bool flag_X_Axis = false;
            bool flag_Y_Axis = false;

            double baseLineAngle = AEE_Utility.GetAngleOfLine(baseLine);
            CommonModule commonModule_Obj = new CommonModule();
            commonModule_Obj.checkAngleOfLine_Axis(baseLineAngle, out flag_X_Axis, out flag_Y_Axis);

            List<Point3d> listOfStrtEndPoint = commonModule_Obj.getStartEndPointOfLine(baseLine);
            Point3d baseLineStartPoint = listOfStrtEndPoint[0];
            Point3d baseLineEndPoint = listOfStrtEndPoint[1];

            List<ObjectId> listOfClosestBaseline_ObjId = new List<ObjectId>();

            listOfClosestLine = arrangeClosestLineAsInAscending(listOfClosestLine, flag_X_Axis, flag_Y_Axis);

            foreach (var closestLine in listOfClosestLine)
            {
                if (Math.Round(closestLine.Length) == 0)
                    continue;
                string beamLayerNameInsctToClosestLine = "";
                ObjectId beamId = new ObjectId();
                double distanceBtwWallToBeam = 0;
                ObjectId nearestBeamWallLineId = new ObjectId();
                bool flagOfSunkanSlab = false;
                ObjectId sunkanSlabId = new ObjectId();
                ObjectId parapetId = new ObjectId();

                //Testing - 123
                //string wallName = AEE_Utility.GetXDataRegisterAppName(closestLine.ObjectId);
                //List<string> wallNames = new List<string>() { "W9_EX_1" };//, "W4_R_8" };

                //{ "W12_EX_1", "W10_EX_1", "W4_R_20", "W3_LF_10", "W1_R_20", "W7_EX_1", "W3_R_21", "W12_EX_1", "W4_R_20", "W3_R_10", "W3_R_6", "W12_R_6", "W2_R_17", "W2_R_15", "W8_R_13", "W18_EX_1", "W28_EX_1" };
                //if (string.IsNullOrWhiteSpace(wallName))
                //{
                //    MessageBox.Show("Empty WallName");
                ////}

                //if (wallNames.Contains(wallName) || string.IsNullOrWhiteSpace(wallName))
                //{
                //    AEE_Utility.ZoomObj = true;
                //    AEE_Utility.ZoomToEntity(baseLineId);
                //    AEE_Utility.DCircle(baseLine.StartPoint);
                //    AEE_Utility.DCircle(baseLine.EndPoint);
                //}

                int indexOfClosestLineId = InternalWallHelper.listOfLinesBtwTwoCrners_ObjId.IndexOf(closestLine.ObjectId);
                if (indexOfClosestLineId != -1)
                {
                    beamLayerNameInsctToClosestLine = BeamHelper.listOfLinesBtwTwoCrners_BeamInsctName[indexOfClosestLineId];
                    beamId = BeamHelper.listOfLinesBtwTwoCrners_BeamInsctId[indexOfClosestLineId];
                    distanceBtwWallToBeam = BeamHelper.listOfDistanceBtwWallToBeam[indexOfClosestLineId];
                    nearestBeamWallLineId = BeamHelper.listOfNearestBtwWallToBeamWallLineId[indexOfClosestLineId];
                    flagOfSunkanSlab = SunkanSlabHelper.listOfLinesBtwTwoCrners_SunkanSlabWallLineflag[indexOfClosestLineId];
                    sunkanSlabId = SunkanSlabHelper.listOfLinesBtwTwoCrners_SunkanSlabId[indexOfClosestLineId];
                    parapetId = ParapetHelper.listOfParapetId_WithWallLine[indexOfClosestLineId];
                }
                ObjectId closestLine_ObjId = new ObjectId();

                getPointOfClosestWallPanel(beamLayerNameInsctToClosestLine, beamId, distanceBtwWallToBeam, nearestBeamWallLineId, closestLine, baseLineStartPoint, baseLineEndPoint, flag_X_Axis, flag_Y_Axis, out closestLine_ObjId, flagOfSunkanSlab, sunkanSlabId, parapetId);

                if (closestLine_ObjId.IsValid == true)
                {
                    listOfClosestBaseline_ObjId.Add(closestLine_ObjId);

                    //listOfExistsClosestWallPanelLine.Add(closestLine);
                    //listOfExistsClosestWallPanelLine_ObjId.Add(closestLine.ObjectId);
                    listOfAllExistsWallPanelLine.Add(closestLine);
                    //listOfAllExistsWallPanelLine_ObjId.Add(closestLine.ObjectId);                   
                }
            }

            getAllPointOfBaseWallPanel(beamLayerNameInsctToBaseLine, beamId_BaseLine, distanceBtwWallToBeam_BaseLine, nearestBeamWallLineId_BaseLine, baseLine, baseLineStartPoint, baseLineEndPoint, listOfClosestBaseline_ObjId, flag_X_Axis, flag_Y_Axis, flagOfSunkanSlab_BaseLine, sunkanSlabId_BaseLine, parapetObjId_BaseLine);

            createGroupOfClosesAndBaseLine(listOfClosestBaseline_ObjId);

            AEE_Utility.deleteEntity(listOfClosestBaseline_ObjId);
            listOfClosestBaseline_ObjId.Clear();

            //listOfExistsBaseWallPanelLine.Add(baseLine);
            //listOfExistsBaseWallPanelLine_ObjId.Add(baseLineId);
            listOfAllExistsWallPanelLine.Add(baseLine);
            //listOfAllExistsWallPanelLine_ObjId.Add(baseLineId);
        }
        public static List<Line> arrangeClosestLineAsInAscending(List<Line> listOfClosestLine, bool flag_X_Axis, bool flag_Y_Axis)
        {
            List<Point3d> listOfStrtPoint = new List<Point3d>();
            foreach (var line in listOfClosestLine)
            {
                listOfStrtPoint.Add(line.StartPoint);
            }

            if (flag_X_Axis == true)
            {
                listOfStrtPoint = listOfStrtPoint.OrderBy(sortPoint => sortPoint.X).ToList();
            }
            else if (flag_Y_Axis == true)
            {
                listOfStrtPoint = listOfStrtPoint.OrderBy(sortPoint => sortPoint.Y).ToList();
            }

            List<Line> listOfOutputLine = new List<Line>();

            for (int i = 0; i < listOfClosestLine.Count; i++)
            {
                var line = listOfClosestLine[i];
                int index = listOfStrtPoint.IndexOf(line.StartPoint);
                listOfOutputLine.Add(listOfClosestLine[index]);
            }
            return listOfOutputLine;
        }

        public void getPointOfClosestWallPanel(string beamLayerNameInsctToWall, ObjectId beamId, double distanceBtwWallToBeam, ObjectId nearestBeamWallLineId, Line closestLine, Point3d baseLineStartPoint, Point3d baseLineEndPoint, bool flag_X_Axis, bool flag_Y_Axis, out ObjectId closestLine_ObjId, bool flagOfSunkanSlab, ObjectId sunkanSlabId, ObjectId parapetId)
        {
            closestLine_ObjId = new ObjectId();

            var xDataRegAppName = AEE_Utility.GetXDataRegisterAppName(closestLine.Id);

            CommonModule commonModule_Obj = new CommonModule();

            List<Point3d> listOfStrtEndPoint = commonModule_Obj.getStartEndPointOfLine(closestLine);
            Point3d actulClosestStartPoint = listOfStrtEndPoint[0];
            Point3d actulClosestEndPoint = listOfStrtEndPoint[1];

            List<Point3d> listOfInsctClosestLineStrtEndPoint = checkClosestLine_Intesect_To_BaseLine(closestLine, actulClosestStartPoint, actulClosestEndPoint, baseLineStartPoint, baseLineEndPoint, PanelLayout_UI.maxWallThickness);
            Point3d insctClosestStartPoint = listOfInsctClosestLineStrtEndPoint[0];
            Point3d insctClosestEndPoint = listOfInsctClosestLineStrtEndPoint[1];

            var indexOfLine = InternalWallHelper.listOfLinesBtwTwoCrners_ObjId.IndexOf(closestLine.ObjectId);
            var offsetLineId = InternalWallHelper.listOfOffsetLinesBtwTwoCrners_ObjId[indexOfLine];
            var offsetLineEnt = AEE_Utility.GetLine(offsetLineId);

            List<Point3d> listOfOffsetLineStrtEndPoint = commonModule_Obj.getStartEndPointOfLine(offsetLineEnt);
            Point3d actulClosestOffstStartPoint = listOfOffsetLineStrtEndPoint[0];
            Point3d actulClosestOffstEndPoint = listOfOffsetLineStrtEndPoint[1];

            List<Point3d> listOfInsctClosestOffstLineStrtEndPoint = checkClosestLine_Intesect_To_BaseLine(offsetLineEnt, actulClosestOffstStartPoint, actulClosestOffstEndPoint, baseLineStartPoint, baseLineEndPoint, (PanelLayout_UI.maxWallThickness + CommonModule.panelDepth));
            Point3d insctClosestOffstStartPoint = listOfInsctClosestOffstLineStrtEndPoint[0];
            Point3d insctClosestOffstEndPoint = listOfInsctClosestOffstLineStrtEndPoint[1];

            var lstNewSegmets = new List<LineSegment3d>();
            var lineDiff = 0.0;
            double lengthOfLineTemp = AEE_Utility.GetLengthOfLine(insctClosestStartPoint.X, insctClosestStartPoint.Y, insctClosestEndPoint.X, insctClosestEndPoint.Y);
            List<double> listOfWallPanelLengthTemp = getListOfWallPanelLength(lengthOfLineTemp, PanelLayout_UI.standardPanelWidth, PanelLayout_UI.minWidthOfPanel);

            if (isBaseLineConnectToCorner(new LineSegment3d(baseLineStartPoint, baseLineEndPoint),
                new LineSegment3d(insctClosestStartPoint, insctClosestEndPoint),
                new LineSegment3d(insctClosestOffstStartPoint, insctClosestOffstEndPoint), listOfWallPanelLengthTemp, lstNewSegmets))
            {
                var diff = insctClosestStartPoint.DistanceTo(insctClosestEndPoint) - lstNewSegmets[0].Length;
                if (diff > 0 || listOfWallPanelLengthTemp.Count == 1)
                {
                    lineDiff = diff;
                    lineDiff = Math.Round(lineDiff);
                    insctClosestStartPoint = lstNewSegmets[0].StartPoint;
                    insctClosestEndPoint = lstNewSegmets[0].EndPoint;

                    insctClosestOffstStartPoint = lstNewSegmets[1].StartPoint;
                    insctClosestOffstEndPoint = lstNewSegmets[1].EndPoint;
                    if (listOfWallPanelLengthTemp.Count > 1 && listOfWallPanelLengthTemp.Last() <= CommonModule.maxCornerExtension
                        && listOfWallPanelLengthTemp.Last() > 0)
                    {
                        var offsetVec = (insctClosestStartPoint - insctClosestEndPoint).GetNormal() * listOfWallPanelLengthTemp.Last();
                        dicModifiedPoints[insctClosestEndPoint] = insctClosestEndPoint + offsetVec;
                        dicModifiedPoints[insctClosestOffstEndPoint] = insctClosestOffstEndPoint + offsetVec;
                        insctClosestEndPoint = insctClosestEndPoint + offsetVec;
                        insctClosestOffstEndPoint = insctClosestOffstEndPoint + offsetVec;
                    }
                }
            }

            double tempLengthOfLine = AEE_Utility.GetLengthOfLine(insctClosestStartPoint.X, insctClosestStartPoint.Y, insctClosestEndPoint.X, insctClosestEndPoint.Y);
            List<double> tempListOfWallPanelLength = getListOfWallPanelLength(tempLengthOfLine, PanelLayout_UI.standardPanelWidth, PanelLayout_UI.minWidthOfPanel);
            var offsetVector = new Vector3d(0, 0, 0);

            if (tempListOfWallPanelLength.Count > 1 && tempListOfWallPanelLength.Last() <= CommonModule.maxCornerExtension)
            {
                offsetVector = (insctClosestStartPoint - insctClosestEndPoint).GetNormal() * tempListOfWallPanelLength.Last();
                insctClosestEndPoint = insctClosestEndPoint + offsetVector;
                insctClosestOffstEndPoint = insctClosestOffstEndPoint + offsetVector;
                listOfInsctClosestLineStrtEndPoint[1] = insctClosestEndPoint;
                listOfInsctClosestOffstLineStrtEndPoint[1] = insctClosestOffstEndPoint;
            }


            if (tempListOfWallPanelLength.Count == 1 && tempListOfWallPanelLength.Last() <= CommonModule.maxCornerExtension && closestLine.Length <= PanelLayout_UI.standardPanelWidth)
            {
                //AEE_Utility.ZoomToEntity(closestLine.ObjectId);
                return;
            }

            var closestEnt = AEE_Utility.GetEntityForRead(closestLine.ObjectId);
            closestLine_ObjId = AEE_Utility.getLineId(closestEnt, insctClosestStartPoint.X, insctClosestStartPoint.Y, 0, insctClosestEndPoint.X, insctClosestEndPoint.Y, 0, false);

            List<Point3d> listOfClosestStrtEndPt = AEE_Utility.GetStartEndPointOfLine(closestLine.ObjectId);

            double beamBottom = 0;

            if (beamLayerNameInsctToWall == "")
            {
                foreach (ObjectId item in CheckShellPlanHelper.listOfSelectedOffsetBeam_ObjId)
                {
                    Entity insctBeamEnt = AEE_Utility.GetEntityForRead(item);
                    if (AEE_Utility.GetPointIsInsideThePolyline(item, listOfClosestStrtEndPt[0]) || AEE_Utility.GetPointIsInsideThePolyline(item, listOfClosestStrtEndPt[1]))
                    {
                        List<Point3d> listOfBeamInsctPts = AEE_Utility.InterSectionPointBetweenPolyLineAndLine(item, closestLine.ObjectId, Intersect.OnBothOperands);
                        if (listOfBeamInsctPts.Count > 0)
                        {
                            // This checking for beam and corner are intersectin case 100 mm from IA case
                            if (listOfBeamInsctPts[0].DistanceTo(listOfClosestStrtEndPt[0]) < 1 || listOfBeamInsctPts[0].DistanceTo(listOfClosestStrtEndPt[1]) < 1)
                            {
                                break;
                            }
                        }
                        //Fix the Panel Ht when there is a beam in the backwallby SDM 2022-09-21
                        //beamLayerNameInsctToWall = insctBeamEnt.Layer;
                        //beamBottom = Beam_UI_Helper.getBeamBottom(insctBeamEnt.Layer);
                        //break;
                        if (AEE_Utility.GetDistanceBetweenTwoEntity(closestLine.ObjectId, item) > 1)
                        {
                            beamLayerNameInsctToWall = insctBeamEnt.Layer;
                            beamBottom = Beam_UI_Helper.getOffsetBeamBottom(insctBeamEnt.Layer);
                            break;
                        }
                    }
                }
            }
            else if (distanceBtwWallToBeam > 0)//Fix the Panel Ht when there is a beam in the backwall by SDM 2022-09-21
            {
                beamBottom = Beam_UI_Helper.getOffsetBeamBottom(beamLayerNameInsctToWall);
            }

            bool flagOfTopPanel = false;
            var standardWallHeight = GeometryHelper.getHeightOfWall(closestEnt.Layer, PanelLayout_UI.maxHeightOfPanel, InternalWallSlab_UI_Helper.getSlabBottom(closestEnt.Layer), beamBottom, PanelLayout_UI.SL, PanelLayout_UI.RC, PanelLayout_UI.getSlabThickness(closestEnt.Layer), PanelLayout_UI.kickerHt, PanelLayout_UI.floorHeight, CommonModule.internalCorner, sunkanSlabId, out flagOfTopPanel);


            var wall_X_Height = getHeightOfWall_X(sunkanSlabId, PanelLayout_UI.flagOfPanelWithRC, PanelLayout_UI.RC, closestEnt.Layer, beamLayerNameInsctToWall, standardWallHeight, flagOfTopPanel);


            List<ObjectId> listOfWallPanelLine_ObjId = new List<ObjectId>();
            double lengthOfLine = AEE_Utility.GetLengthOfLine(insctClosestStartPoint.X, insctClosestStartPoint.Y, insctClosestEndPoint.X, insctClosestEndPoint.Y);
            List<double> listOfWallPanelLength = getListOfWallPanelLength(lengthOfLine, PanelLayout_UI.standardPanelWidth, PanelLayout_UI.minWidthOfPanel);

            List<ObjectId> listOfWallPanelRect_ObjId = drawWallPanels(xDataRegAppName, closestLine.ObjectId, insctClosestStartPoint, insctClosestEndPoint, insctClosestOffstStartPoint, insctClosestOffstEndPoint, flag_X_Axis, flag_Y_Axis, listOfWallPanelLength, out listOfWallPanelLine_ObjId);

            var indexOfBeamLineExist = BeamHelper.listOfBeamLineId_ForMoveInBeamLayout.IndexOf(closestLine.ObjectId);
            if (indexOfBeamLineExist == -1)
            {
                BeamHelper beamHelper = new BeamHelper();
                beamHelper.drawBeamWallPanel(sunkanSlabId, beamLayerNameInsctToWall, beamId, distanceBtwWallToBeam, nearestBeamWallLineId, listOfWallPanelRect_ObjId, listOfWallPanelLine_ObjId, closestLine.ObjectId);
            }

            SunkanSlabHelper internalSunkanSlabHelper = new SunkanSlabHelper();
            internalSunkanSlabHelper.drawSunkanSlabWallPanel(flagOfSunkanSlab, sunkanSlabId, listOfWallPanelRect_ObjId, listOfWallPanelLine_ObjId, closestLine_ObjId);

            List<ObjectId> listOfTextId = new List<ObjectId>();
            List<ObjectId> listOfWallXTextId = new List<ObjectId>();
            List<ObjectId> listOfCircleId = new List<ObjectId>();

            string wallPanelNameWithRC = "";
            double standardWallHeightWithRC = 0;

            if (parapetId.IsValid == true)
            {
                standardWallHeight = ParapetHelper.getParapetWallHeight(parapetId, sunkanSlabId, closestEnt.ObjectId, closestEnt.Layer);
                wall_X_Height = 0;
            }

            getWallPanelHeight_PanelWithRC(sunkanSlabId, PanelLayout_UI.flagOfPanelWithRC, standardWallHeight, PanelLayout_UI.wallPanelName, PanelLayout_UI.RC, closestEnt.Layer, out standardWallHeightWithRC, out wallPanelNameWithRC);

            double RC_WithLevelDiff = PanelLayout_UI.RC;
            string doorLayer = ""; //RTJ 10-06-2021
            string windowLayer = ""; //RTJ 10-06-2021
            
            writeTextInWallPanel(CommonModule.wallPanelType, sunkanSlabId, closestEnt.Layer, listOfWallPanelRect_ObjId, listOfWallPanelLine_ObjId, standardWallHeightWithRC, wallPanelNameWithRC, wall_X_Height, PanelLayout_UI.wallTopPanelName, RC_WithLevelDiff, CommonModule.wallPanelTextLyrName, CommonModule.wallPanelLyrColor, true, out listOfTextId, out listOfWallXTextId, out listOfCircleId, doorLayer, windowLayer); //RTJ 10-06-2021

            listOfListClosestLineWallPanelRect_ObjId.Add(listOfWallPanelRect_ObjId);
            listOfListClosestLineWallPanelText_ObjId.Add(listOfTextId);
            listOfListClosestLineWallXText_ObjId.Add(listOfWallXTextId);

            getExternalCornerWallId_ForStretch(closestLine.ObjectId, listOfWallPanelRect_ObjId, listOfWallPanelLine_ObjId, listOfTextId, listOfWallXTextId);

            CornerHelper cornerHlp = new CornerHelper();
            cornerHlp.getInternalCornerId_ForStretch(closestLine.ObjectId, listOfWallPanelRect_ObjId, listOfWallPanelLine_ObjId, listOfTextId, listOfWallXTextId, listOfCircleId, lineDiff);
            getRemainingLine(xDataRegAppName, closestLine.ObjectId, beamLayerNameInsctToWall, beamId, distanceBtwWallToBeam, nearestBeamWallLineId, closestEnt, actulClosestStartPoint, actulClosestEndPoint, listOfInsctClosestLineStrtEndPoint, actulClosestOffstStartPoint, actulClosestOffstEndPoint, listOfInsctClosestOffstLineStrtEndPoint, flag_X_Axis, flag_Y_Axis, flagOfSunkanSlab, sunkanSlabId, parapetId);
        }

        private bool isBaseLineConnectToCorner(LineSegment3d baseLine,
            LineSegment3d line1, LineSegment3d line2, List<double> listOfWallPanelLength, List<LineSegment3d> lstNewSegmets)
        {
            var t = 1e-6;
            var tol = new Tolerance(t, t);
            CornerData connectingCorner = null;
            foreach (var corner in InternalWallHelper.lstCornerData)
            {
                if (corner.EndPoint1.IsEqualTo(baseLine.StartPoint, tol))
                {
                    var dist = corner.ptCorner.DistanceTo(baseLine.StartPoint) + listOfWallPanelLength.First();
                    if (dist < CommonModule.internalCorner_MaxLngth)
                    {
                        var st = line1.GetLine().GetClosestPointTo(baseLine.StartPoint);
                        lstNewSegmets.Add(new LineSegment3d(st.Point, line1.EndPoint));
                        var st2 = line2.GetLine().GetClosestPointTo(baseLine.StartPoint);
                        lstNewSegmets.Add(new LineSegment3d(st2.Point, line2.EndPoint));
                        connectingCorner = corner;
                        break;
                    }
                    return false;
                }
                if (corner.EndPoint2.IsEqualTo(baseLine.StartPoint, tol))
                {
                    var dist = corner.ptCorner.DistanceTo(baseLine.StartPoint) + listOfWallPanelLength.First();
                    if (dist < CommonModule.internalCorner_MaxLngth)
                    {
                        var st = line1.GetLine().GetClosestPointTo(baseLine.StartPoint);
                        lstNewSegmets.Add(new LineSegment3d(st.Point, line1.EndPoint));
                        var st2 = line2.GetLine().GetClosestPointTo(baseLine.StartPoint);
                        lstNewSegmets.Add(new LineSegment3d(st2.Point, line2.EndPoint));
                        connectingCorner = corner;
                        break;
                    }
                    return false;
                }

                if (corner.EndPoint1.IsEqualTo(baseLine.EndPoint, tol))
                {
                    var dist = corner.ptCorner.DistanceTo(baseLine.EndPoint) + listOfWallPanelLength.Last();
                    if (dist < CommonModule.internalCorner_MaxLngth)
                    {
                        var st = line1.GetLine().GetClosestPointTo(baseLine.EndPoint);
                        lstNewSegmets.Add(new LineSegment3d(line1.StartPoint, st.Point));
                        var st2 = line2.GetLine().GetClosestPointTo(baseLine.EndPoint);
                        lstNewSegmets.Add(new LineSegment3d(line2.StartPoint, st2.Point));
                        connectingCorner = corner;
                        break;
                    }
                    return false;
                }
                if (corner.EndPoint2.IsEqualTo(baseLine.EndPoint, tol))
                {
                    var dist = corner.ptCorner.DistanceTo(baseLine.EndPoint) + listOfWallPanelLength.Last();
                    if (dist < CommonModule.internalCorner_MaxLngth)
                    {
                        var st = line1.GetLine().GetClosestPointTo(baseLine.EndPoint);
                        lstNewSegmets.Add(new LineSegment3d(line1.StartPoint, st.Point));
                        var st2 = line2.GetLine().GetClosestPointTo(baseLine.EndPoint);
                        lstNewSegmets.Add(new LineSegment3d(line2.StartPoint, st2.Point));
                        connectingCorner = corner;
                        break;
                    }
                    return false;
                }
            }
            if (!lstNewSegmets.Any())
            {
                return false;
            }
            foreach (var item in lstNewSegmets)
            {
                foreach (var corner in InternalWallHelper.lstCornerData)
                {
                    if (item.IsOn(corner.ptCorner, tol))
                    {
                        lstNewSegmets.Clear();
                        return false;
                    }
                }
            }
            return true;
        }

        private List<Point3d> checkClosestLine_Intesect_To_BaseLine(Line closestLine, Point3d closestLineStartPoint, Point3d closestLineEndPoint, Point3d baseLineStartPoint, Point3d baseLineEndPoint, double offsetValue)
        {
            List<Point3d> listOfClosestStrtEndPoint = new List<Point3d>();
            List<double> listOfOutPoint = new List<double>();

            var listOfUpperDir_Point = AEE_Utility.GetParallelLinePoints_In_UpperDir(baseLineStartPoint.X, baseLineStartPoint.Y, baseLineEndPoint.X, baseLineEndPoint.Y, offsetValue);

            var listOfLowerDir_Point = AEE_Utility.GetParallelLinePoints_In_DownDir(baseLineStartPoint.X, baseLineStartPoint.Y, baseLineEndPoint.X, baseLineEndPoint.Y, offsetValue);

            var lowerLineId = AEE_Utility.getLineId(listOfLowerDir_Point[0], listOfLowerDir_Point[1], 0, listOfLowerDir_Point[2], listOfLowerDir_Point[3], 0, false);

            var upperLineId = AEE_Utility.getLineId(listOfUpperDir_Point[0], listOfUpperDir_Point[1], 0, listOfUpperDir_Point[2], listOfUpperDir_Point[3], 0, false);

            double upperLength = AEE_Utility.GetDistanceBetweenTwoLines(closestLine.ObjectId, upperLineId);
            double lowerLength = AEE_Utility.GetDistanceBetweenTwoLines(closestLine.ObjectId, lowerLineId);

            if (upperLength > lowerLength)
            {
                listOfOutPoint = listOfLowerDir_Point;
                AEE_Utility.deleteEntity(upperLineId);
            }
            else
            {
                listOfOutPoint = listOfUpperDir_Point;
                AEE_Utility.deleteEntity(lowerLineId);
            }

            var minX_LineId = AEE_Utility.getLineId(baseLineStartPoint.X, baseLineStartPoint.Y, 0, listOfOutPoint[0], listOfOutPoint[1], 0, false);
            var maxX_LineId = AEE_Utility.getLineId(baseLineEndPoint.X, baseLineEndPoint.Y, 0, listOfOutPoint[2], listOfOutPoint[3], 0, false);

            var listOfMinX_InsctPoint = AEE_Utility.InterSectionBetweenTwoEntity(closestLine.ObjectId, minX_LineId);
            var listOfMaxX_InsctPoint = AEE_Utility.InterSectionBetweenTwoEntity(closestLine.ObjectId, maxX_LineId);

            if (listOfMinX_InsctPoint.Count != 0)
            {
                listOfClosestStrtEndPoint.Add(listOfMinX_InsctPoint[0]);
            }
            else
            {
                listOfClosestStrtEndPoint.Add(closestLineStartPoint);
            }

            if (listOfMaxX_InsctPoint.Count != 0)
            {
                listOfClosestStrtEndPoint.Add(listOfMaxX_InsctPoint[0]);
            }
            else
            {
                listOfClosestStrtEndPoint.Add(closestLineEndPoint);
            }

            AEE_Utility.deleteEntity(minX_LineId);
            AEE_Utility.deleteEntity(maxX_LineId);

            return listOfClosestStrtEndPoint;
        }


        private void getAllPointOfBaseWallPanel(string beamLayerNameInsctToWall, ObjectId beamId, double distanceBtwWallToBeam, ObjectId nearestBeamWallLineId, Line baseLine, Point3d baseLineStartPoint, Point3d baseLineEndPoint, List<ObjectId> listOfClosestBaseline_ObjId, bool flag_X_Axis, bool flag_Y_Axis, bool flagOfSunkanSlab, ObjectId sunkanSlabId, ObjectId parapetId)
        {
            List<Point3d> listOfAllLinesStartAndEndPoint = new List<Point3d>();

            listOfAllLinesStartAndEndPoint.Add(baseLineStartPoint);
            listOfAllLinesStartAndEndPoint.Add(baseLineEndPoint);

            double baseLineAngle = AEE_Utility.GetAngleOfLine(baseLineStartPoint.X, baseLineStartPoint.Y, baseLineEndPoint.X, baseLineEndPoint.Y);

            CommonModule commonModule_Obj = new CommonModule();
            var indexOfLine = InternalWallHelper.listOfLinesBtwTwoCrners_ObjId.IndexOf(baseLine.ObjectId);
            var offsetBaseLineId = InternalWallHelper.listOfOffsetLinesBtwTwoCrners_ObjId[indexOfLine];
            var offsetBaseLineEnt = AEE_Utility.GetLine(offsetBaseLineId);

            List<Point3d> listOfOffsetBaseLineStrtEndPoint = commonModule_Obj.getStartEndPointOfLine(offsetBaseLineEnt);
            Point3d actulOffstBaseStartPoint = listOfOffsetBaseLineStrtEndPoint[0];
            Point3d actulOffstBaseEndPoint = listOfOffsetBaseLineStrtEndPoint[1];

            List<Point3d> listOfAllOffsetLinesStartAndEndPoint = new List<Point3d>();

            listOfAllOffsetLinesStartAndEndPoint.Add(actulOffstBaseStartPoint);
            listOfAllOffsetLinesStartAndEndPoint.Add(actulOffstBaseEndPoint);

            for (int i = 0; i < listOfClosestBaseline_ObjId.Count; i++)
            {
                ObjectId closestLineId = listOfClosestBaseline_ObjId[i];
                var listOfClosestLinePoint = AEE_Utility.GetStartEndPointOfLine(closestLineId);
                Point3d closestStartPoint = listOfClosestLinePoint[0];
                Point3d closestEndPoint = listOfClosestLinePoint[1];

                List<List<Point3d>> listOfParallelPoint = getNearestParallelBaseLinePoint(baseLine, baseLineStartPoint, baseLineEndPoint, closestLineId, closestStartPoint, closestEndPoint, PanelLayout_UI.maxWallThickness);

                if (listOfParallelPoint.Count == 2)
                {
                    if ((BeamHelper.dicBeamIntersectModifiedPoints.ContainsKey(closestStartPoint) &&
                        BeamHelper.dicBeamIntersectModifiedPoints[closestStartPoint].IsEqualTo(closestEndPoint)) ||
                        (BeamHelper.dicBeamIntersectModifiedPoints.ContainsKey(closestEndPoint) &&
                        BeamHelper.dicBeamIntersectModifiedPoints[closestEndPoint].IsEqualTo(closestStartPoint)))
                    {
                        continue;
                    }
                    var strtLine_Perpend_InsctPoint = listOfParallelPoint[0];
                    var endLine_Perpend_InsctPoint = listOfParallelPoint[1];
                    Point3d baseStrtPoint = strtLine_Perpend_InsctPoint[0];
                    Point3d baseEndPoint = endLine_Perpend_InsctPoint[0];

                    var length = baseStrtPoint.DistanceTo(baseEndPoint);
                    var offsetVec = new Vector3d(0, 0, 0);
                    List<double> listOfWallPanelLength = getListOfWallPanelLength(length, PanelLayout_UI.standardPanelWidth, PanelLayout_UI.minWidthOfPanel);
                    if (listOfWallPanelLength.Count > 1 && listOfWallPanelLength.Last() <= CommonModule.maxCornerExtension)
                    {
                        offsetVec = (baseStrtPoint - baseEndPoint).GetNormal() * listOfWallPanelLength.Last();
                        baseEndPoint = baseEndPoint + offsetVec;
                    }

                    if (listOfWallPanelLength.Count == 1 && listOfWallPanelLength.Last() <= CommonModule.maxCornerExtension)
                    {
                        return;
                    }

                    List<List<Point3d>> listOfOffsetBaseLinePoint = getNearestParallelBaseLinePoint(offsetBaseLineEnt, actulOffstBaseStartPoint, actulOffstBaseEndPoint, closestLineId, closestStartPoint, closestEndPoint, (PanelLayout_UI.maxWallThickness + CommonModule.panelDepth));
                    if (listOfOffsetBaseLinePoint.Count == 2)
                    {
                        var strtOffsetLine_Perpend_InsctPoint = listOfOffsetBaseLinePoint[0];
                        var endOffsetLine_Perpend_InsctPoint = listOfOffsetBaseLinePoint[1];

                        var offsetBaseStrtPoint = strtOffsetLine_Perpend_InsctPoint[0];
                        var offsetBaseEndPoint = endOffsetLine_Perpend_InsctPoint[0];
                        if (double.IsNaN(offsetBaseStrtPoint.X) || double.IsNaN(offsetBaseStrtPoint.Y) || double.IsNaN(offsetBaseEndPoint.X) || double.IsNaN(offsetBaseEndPoint.Y))
                        {

                        }
                        else
                        {
                            listOfAllLinesStartAndEndPoint.Add(baseStrtPoint);
                            listOfAllLinesStartAndEndPoint.Add(baseEndPoint);
                            listOfAllOffsetLinesStartAndEndPoint.Add(offsetBaseStrtPoint);
                            listOfAllOffsetLinesStartAndEndPoint.Add(offsetBaseEndPoint + offsetVec);
                        }
                        ////////////drawWallPanel(baseStrtPoint, baseEndPoint, offsetBaseStrtPoint, offsetBaseEndPoint);
                    }
                }
            }

            drawBaseLineWallPanel(beamLayerNameInsctToWall, beamId, distanceBtwWallToBeam, nearestBeamWallLineId, baseLine.ObjectId, listOfAllLinesStartAndEndPoint, listOfAllOffsetLinesStartAndEndPoint, flag_X_Axis, flag_Y_Axis, listOfClosestBaseline_ObjId, flagOfSunkanSlab, sunkanSlabId, parapetId);

        }

        public static List<double> defineListOfWallPanelLengthForTwoCornerCase(List<ObjectId> listOfClosestBaseline_ObjId, Point3d baseStrtPoint, Point3d baseEndPoint, double standardPanelWidth, double minWidthOfPanel)
        {
            List<double> listOfWallPanelLength = new List<double>();

            double lengthOfLine = Math.Round(AEE_Utility.GetLengthOfLine(baseStrtPoint, baseEndPoint));

            CommonModule commonMod = new CommonModule();
            bool flag_X_Axis = false;
            bool flag_Y_Axis = false;
            commonMod.checkAngleOfLine_Axis(AEE_Utility.GetAngleOfLine(baseStrtPoint, baseEndPoint), out flag_X_Axis, out flag_Y_Axis);

            if (lengthOfLine >= PanelLayout_UI.standardPanelWidth * 2 || lengthOfLine <= PanelLayout_UI.standardPanelWidth)
                return listOfWallPanelLength;

            int noOfIntersectingCorners = 0;
            List<double> listOfCornerLengths = new List<double>();

            for (int ii = 0; ii < listOfClosestBaseline_ObjId.Count; ii++)
            {
                for (int jj = 0; jj < CornerHelper.listOfInternalCornerId_ForStretch.Count; jj++)
                {
                    List<Point3d> listOfInterSectPoints = AEE_Utility.InterSectionBetweenTwoEntity(listOfClosestBaseline_ObjId[ii], CornerHelper.listOfInternalCornerId_ForStretch[jj]);
                    if (listOfInterSectPoints.Count > 0)
                    {

                        Point3d cornerPt = AEE_Utility.GetBasePointOfPolyline(CornerHelper.listOfInternalCornerId_ForStretch[jj]);

                        ObjectId tempPolylineId = new ObjectId();
                        Point3d tempCornerPt = new Point3d();
                        if (flag_Y_Axis)
                        {
                            tempPolylineId = AEE_Utility.createLineWithPolyline(new Point2d(0, baseStrtPoint.Y), new Point2d(0, baseEndPoint.Y), CommonModule.wallPanelLyrName, 125);
                            tempCornerPt = new Point3d(0, cornerPt.Y, 0);
                        }
                        if (flag_X_Axis)
                        {
                            tempPolylineId = AEE_Utility.createLineWithPolyline(new Point2d(baseStrtPoint.X, 0), new Point2d(baseEndPoint.X, 0), CommonModule.wallPanelLyrName, 125);
                            tempCornerPt = new Point3d(cornerPt.X, 0, 0);
                        }

                        if (AEE_Utility.IsPointOnEntity(tempPolylineId, tempCornerPt))
                        {
                            double cornerLength = Math.Round(AEE_Utility.GetLengthOfLine(listOfInterSectPoints[0], cornerPt));
                            listOfCornerLengths.Add(cornerLength);
                            noOfIntersectingCorners++;
                        }

                        AEE_Utility.deleteEntity(tempPolylineId);
                    }
                }
            }

            if (noOfIntersectingCorners == 2)
            {
                double gapBwCorners = lengthOfLine - listOfCornerLengths.Sum();
                double firstPanelLength = 0;
                if (listOfCornerLengths[0] <= listOfCornerLengths[1])
                {
                    firstPanelLength = gapBwCorners + listOfCornerLengths[0];
                    if (firstPanelLength > PanelLayout_UI.standardPanelWidth)
                    {
                        listOfWallPanelLength.Add(firstPanelLength);
                        listOfWallPanelLength.Add(gapBwCorners);
                        listOfWallPanelLength.Add(listOfCornerLengths[1]);
                    }
                    else
                    {
                        listOfWallPanelLength.Add(firstPanelLength);
                        listOfWallPanelLength.Add(listOfCornerLengths[1]);
                    }

                }
                else
                {
                    firstPanelLength = gapBwCorners + listOfCornerLengths[1];
                    if (firstPanelLength > PanelLayout_UI.standardPanelWidth)
                    {
                        listOfWallPanelLength.Add(listOfCornerLengths[0]);
                        listOfWallPanelLength.Add(gapBwCorners);
                        listOfWallPanelLength.Add(firstPanelLength);
                    }
                    else
                    {
                        listOfWallPanelLength.Add(listOfCornerLengths[0]);
                        listOfWallPanelLength.Add(firstPanelLength);
                    }
                }
            }

            return listOfWallPanelLength;
        }

        private void drawBaseLineWallPanel(string beamLayerNameInsctToWall, ObjectId beamId, double distanceBtwWallToBeam, ObjectId nearestBeamWallLineId, ObjectId baseLineId, List<Point3d> listOfAllLinesStartAndEndPoint, List<Point3d> listOfAllOffsetLinesStartAndEndPoint, bool flag_X_Axis, bool flag_Y_Axis, List<ObjectId> listOfClosestBaseline_ObjId, bool flagOfSunkanSlab, ObjectId sunkanSlabId, ObjectId parapetId)
        {
            if (flag_X_Axis == true)
            {
                listOfAllLinesStartAndEndPoint = listOfAllLinesStartAndEndPoint.OrderBy(sortPoint => sortPoint.X).ToList();
                listOfAllOffsetLinesStartAndEndPoint = listOfAllOffsetLinesStartAndEndPoint.OrderBy(sortPoint => sortPoint.X).ToList();
            }
            else if (flag_Y_Axis == true)
            {
                listOfAllLinesStartAndEndPoint = listOfAllLinesStartAndEndPoint.OrderBy(sortPoint => sortPoint.Y).ToList();
                listOfAllOffsetLinesStartAndEndPoint = listOfAllOffsetLinesStartAndEndPoint.OrderBy(sortPoint => sortPoint.Y).ToList();
            }
            var xDataRegAppName = AEE_Utility.GetXDataRegisterAppName(baseLineId);

            for (int index = 0; index < listOfAllLinesStartAndEndPoint.Count; index++)
            {

                if (index == (listOfAllLinesStartAndEndPoint.Count - 1))
                {
                    break;
                }
                Point3d baseStrtPoint = listOfAllLinesStartAndEndPoint[index];
                Point3d baseEndPoint = listOfAllLinesStartAndEndPoint[index + 1];

                Point3d offsetBaseStrtPoint = listOfAllOffsetLinesStartAndEndPoint[index];
                Point3d offsetBaseEndPoint = listOfAllOffsetLinesStartAndEndPoint[index + 1];

                // Check next Panel Gap
                if ((index + 2) <= (listOfAllLinesStartAndEndPoint.Count - 1))
                {
                    Point3d baseNxtStrtPoint = listOfAllLinesStartAndEndPoint[index + 1];
                    Point3d baseNxtEndPoint = listOfAllLinesStartAndEndPoint[index + 2];

                    double currentPanelLength = Math.Round(AEE_Utility.GetLengthOfLine(baseStrtPoint, baseEndPoint));
                    double nxtPanelLength = Math.Round(AEE_Utility.GetLengthOfLine(baseNxtStrtPoint, baseNxtEndPoint));

                    double totalLength = Math.Truncate(currentPanelLength + nxtPanelLength);

                    //if ((totalLength <= PanelLayout_UI.standardPanelWidth) && (nxtPanelLength <= CommonModule.maxCornerExtension) && (nxtPanelLength != 0) && (currentPanelLength != 0))
                    //{
                    if ((totalLength <= (PanelLayout_UI.maxWallThickness + CommonModule.internalCorner_MaxLngth * 2)) && (nxtPanelLength <= CommonModule.maxCornerExtension) && (nxtPanelLength != 0) && (currentPanelLength != 0))
                    {
                        index++;
                        baseEndPoint = listOfAllLinesStartAndEndPoint[index + 1];
                        offsetBaseEndPoint = listOfAllOffsetLinesStartAndEndPoint[index + 1];

                    }
                }

                baseStrtPoint = dicModifiedPoints.ContainsKey(baseStrtPoint) ? dicModifiedPoints[baseStrtPoint] : baseStrtPoint;
                baseEndPoint = dicModifiedPoints.ContainsKey(baseEndPoint) ? dicModifiedPoints[baseEndPoint] : baseEndPoint;
                double lengthOfBaseLine = AEE_Utility.GetLengthOfLine(baseStrtPoint.X, baseStrtPoint.Y, baseEndPoint.X, baseEndPoint.Y);
                lengthOfBaseLine = Math.Truncate(lengthOfBaseLine);
                if (lengthOfBaseLine != 0)
                {
                    int closestLineObj_Index = 0;
                    ObjectId closestLineObjId = new ObjectId();
                    bool flagOfExistPoint = checkBaseLineIntersectToClosestLine_ForGroup(listOfClosestBaseline_ObjId, baseStrtPoint, baseEndPoint, out closestLineObj_Index, out closestLineObjId);

                    offsetBaseStrtPoint = dicModifiedPoints.ContainsKey(offsetBaseStrtPoint) ? dicModifiedPoints[offsetBaseStrtPoint] : offsetBaseStrtPoint;
                    offsetBaseEndPoint = dicModifiedPoints.ContainsKey(offsetBaseEndPoint) ? dicModifiedPoints[offsetBaseEndPoint] : offsetBaseEndPoint;

                    List<Point3d> listOfClosestStrtEndPt = AEE_Utility.GetStartEndPointOfLine(baseLineId);
                    double beamBottom = 0;

                    if (beamLayerNameInsctToWall == "")
                    {
                        foreach (ObjectId item in CheckShellPlanHelper.listOfSelectedOffsetBeam_ObjId)
                        {
                            Entity insctBeamEnt = AEE_Utility.GetEntityForRead(item);
                            if (AEE_Utility.GetPointIsInsideThePolyline(item, listOfClosestStrtEndPt[0]) || AEE_Utility.GetPointIsInsideThePolyline(item, listOfClosestStrtEndPt[1]))
                            {
                                List<Point3d> listOfBeamInsctPts = AEE_Utility.InterSectionPointBetweenPolyLineAndLine(item, baseLineId, Intersect.OnBothOperands);
                                if (listOfBeamInsctPts.Count > 0)
                                {
                                    // This checking for beam and corner are intersecting case 100 mm from IA case
                                    if (listOfBeamInsctPts[0].DistanceTo(listOfClosestStrtEndPt[0]) < 1 || listOfBeamInsctPts[0].DistanceTo(listOfClosestStrtEndPt[1]) < 1)
                                    {
                                        break;
                                    }
                                }
                                //Added on 14/03/2023 by SDM to check the distance btw wallLine and offsetBeam
                                List<double> listDifference = new List<double>();
                                List<ObjectId> listBeamLineId = new List<ObjectId>();
                                var listOfBeamExplode = AEE_Utility.ExplodeEntity(item);
                                foreach (var beamLineId in listOfBeamExplode)
                                {
                                    var flag = checkAngleOfBaseLine(beamLineId, baseLineId);
                                    if (flag == true)
                                    {
                                        var diff = AEE_Utility.GetDistanceBetweenTwoLines(beamLineId, baseLineId);
                                        listDifference.Add(diff);
                                        listBeamLineId.Add(beamLineId);

                                    }
                                }
                                if (listDifference.Count > 0 && listDifference.Min() <= WindowHelper.windowOffsetValue)
                                {
                                    AEE_Utility.deleteEntity(listOfBeamExplode);
                                    break;
                                }
                                AEE_Utility.deleteEntity(listOfBeamExplode);
                                //---------------------------------
                                beamLayerNameInsctToWall = insctBeamEnt.Layer;
                                beamBottom = Beam_UI_Helper.getOffsetBeamBottom(insctBeamEnt.Layer);
                                break;
                            }
                        }
                    }
                    else
                    {
                        beamBottom = Beam_UI_Helper.getOffsetBeamBottom(beamLayerNameInsctToWall);
                    }

                    List<ObjectId> listOfWallPanelLine_ObjId = new List<ObjectId>();
                    List<ObjectId> listOfTextId = new List<ObjectId>();
                    List<ObjectId> listOfWallXTextId = new List<ObjectId>();
                    List<ObjectId> listOfCircleId = new List<ObjectId>();

                    bool flagOfTopPanel = false;
                    var baseLineEnt = AEE_Utility.GetEntityForRead(baseLineId);
                    var standardWallHeight = GeometryHelper.getHeightOfWall(baseLineEnt.Layer, PanelLayout_UI.maxHeightOfPanel, InternalWallSlab_UI_Helper.getSlabBottom(baseLineEnt.Layer), beamBottom, PanelLayout_UI.SL, PanelLayout_UI.RC, PanelLayout_UI.getSlabThickness(baseLineEnt.Layer), PanelLayout_UI.kickerHt, PanelLayout_UI.floorHeight, CommonModule.internalCorner, sunkanSlabId, out flagOfTopPanel);
                    var wall_X_Height = getHeightOfWall_X(sunkanSlabId, PanelLayout_UI.flagOfPanelWithRC, PanelLayout_UI.RC, baseLineEnt.Layer, beamLayerNameInsctToWall, standardWallHeight, flagOfTopPanel);

                    if (parapetId.IsValid == true)
                    {
                        // templLine added by PBAC 22-11-2021
                        ObjectId tempLineId = AEE_Utility.getLineId(baseStrtPoint, baseEndPoint, false);
                        standardWallHeight = ParapetHelper.getParapetWallHeight(parapetId, sunkanSlabId, tempLineId, baseLineEnt.Layer);
                        wall_X_Height = 0;
                        AEE_Utility.deleteEntity(tempLineId);
                    }

                    double standardWallHeightWithRC = 0;
                    string wallPanelNameWithRC = "";
                    getWallPanelHeight_PanelWithRC(sunkanSlabId, PanelLayout_UI.flagOfPanelWithRC, standardWallHeight, PanelLayout_UI.wallPanelName, PanelLayout_UI.RC, baseLineEnt.Layer, out standardWallHeightWithRC, out wallPanelNameWithRC);

                    //double levelDifferenceOfSunkanSlb = SunkanSlabHelper.getSunkanSlabLevelDifference_WallPanel(sunkanSlabId);
                    //double RC_WithLevelDiff = levelDifferenceOfSunkanSlb + PanelLayout_UI.RC;
                    double RC_WithLevelDiff = PanelLayout_UI.RC;

                    if (flagOfExistPoint == false)
                    {
                        double lengthOfLine = AEE_Utility.GetLengthOfLine(baseStrtPoint.X, baseStrtPoint.Y, baseEndPoint.X, baseEndPoint.Y);
                        List<double> listOfWallPanelLength = defineListOfWallPanelLengthForTwoCornerCase(listOfClosestBaseline_ObjId, baseStrtPoint, baseEndPoint, PanelLayout_UI.standardPanelWidth, PanelLayout_UI.minWidthOfPanel);

                        if (listOfWallPanelLength.Count == 0)
                            listOfWallPanelLength = getListOfWallPanelLength(lengthOfLine, PanelLayout_UI.standardPanelWidth, PanelLayout_UI.minWidthOfPanel);

                        var listOfWallPanelRectId = drawWallPanels(xDataRegAppName, baseLineId, baseStrtPoint, baseEndPoint, offsetBaseStrtPoint, offsetBaseEndPoint, flag_X_Axis, flag_Y_Axis, listOfWallPanelLength, out listOfWallPanelLine_ObjId);
                        string doorLayer = ""; //RTJ 10-06-2021
                        string windowLayer = ""; //RTJ 10-06-2021

                        writeTextInWallPanel(CommonModule.wallPanelType, sunkanSlabId, baseLineEnt.Layer, listOfWallPanelRectId, listOfWallPanelLine_ObjId, standardWallHeightWithRC, wallPanelNameWithRC, wall_X_Height, PanelLayout_UI.wallTopPanelName, RC_WithLevelDiff, CommonModule.wallPanelTextLyrName, CommonModule.wallPanelLyrColor, true, out listOfTextId, out listOfWallXTextId, out listOfCircleId, doorLayer, windowLayer); //RTJ 10-06-2021
                      

                        getExternalCornerWallId_ForStretch(baseLineId, listOfWallPanelRectId, listOfWallPanelLine_ObjId, listOfTextId, listOfWallXTextId);

                        CornerHelper cornerHlp = new CornerHelper();
                        cornerHlp.getInternalCornerId_ForStretch(baseLineId, listOfWallPanelRectId, listOfWallPanelLine_ObjId, listOfTextId, listOfWallXTextId, listOfCircleId);

                        var indexOfBeamLineExist = BeamHelper.listOfBeamLineId_ForMoveInBeamLayout.IndexOf(baseLineId);
                        if (indexOfBeamLineExist == -1)
                        {
                            BeamHelper beamHelper = new BeamHelper();
                            beamHelper.drawBeamWallPanel(sunkanSlabId, beamLayerNameInsctToWall, beamId, distanceBtwWallToBeam, nearestBeamWallLineId, listOfWallPanelRectId, listOfWallPanelLine_ObjId, baseLineId);
                        }

                        SunkanSlabHelper internalSunkanSlabHelper = new SunkanSlabHelper();
                        internalSunkanSlabHelper.drawSunkanSlabWallPanel(flagOfSunkanSlab, sunkanSlabId, listOfWallPanelRectId, listOfWallPanelLine_ObjId, baseLineId);
                    }
                    else
                    {
                        double lengthOfLine = AEE_Utility.GetLengthOfLine(baseStrtPoint.X, baseStrtPoint.Y, baseEndPoint.X, baseEndPoint.Y);

                        List<double> listOfWallPanelLength = defineListOfWallPanelLengthForTwoCornerCase(listOfClosestBaseline_ObjId, baseStrtPoint, baseEndPoint, PanelLayout_UI.standardPanelWidth, PanelLayout_UI.minWidthOfPanel);

                        if (listOfWallPanelLength.Count == 0)
                            listOfWallPanelLength = getListOfWallPanelLength(lengthOfLine, PanelLayout_UI.standardPanelWidth, PanelLayout_UI.minWidthOfPanel);

                        //// add object id for closest parallel line
                        var listOfWallPanelRectId = drawWallPanels(xDataRegAppName, baseLineId, baseStrtPoint, baseEndPoint, offsetBaseStrtPoint, offsetBaseEndPoint, flag_X_Axis, flag_Y_Axis, listOfWallPanelLength, out listOfWallPanelLine_ObjId);
                        string doorLayer = ""; //RTJ 10-06-2021
                        string windowLayer = ""; //RTJ 10-06-2021

                        writeTextInWallPanel(CommonModule.wallPanelType, sunkanSlabId, baseLineEnt.Layer, listOfWallPanelRectId, listOfWallPanelLine_ObjId, standardWallHeightWithRC, wallPanelNameWithRC, wall_X_Height, PanelLayout_UI.wallTopPanelName, RC_WithLevelDiff, CommonModule.wallPanelTextLyrName, CommonModule.wallPanelLyrColor, true, out listOfTextId, out listOfWallXTextId, out listOfCircleId, doorLayer, windowLayer); //RTJ 10-06-2021
                        

                        getExternalCornerWallId_ForStretch(baseLineId, listOfWallPanelRectId, listOfWallPanelLine_ObjId, listOfTextId, listOfWallXTextId);

                        CornerHelper cornerHlp = new CornerHelper();
                        cornerHlp.getInternalCornerId_ForStretch(baseLineId, listOfWallPanelRectId, listOfWallPanelLine_ObjId, listOfTextId, listOfWallXTextId, listOfCircleId);

                        listOfListBaseLineWallPanelRect_ObjId.Add(listOfWallPanelRectId);
                        listOfListBaseLineWallPanelText_ObjId.Add(listOfTextId);
                        listOfListBaseLineWallXText_ObjId.Add(listOfWallXTextId);
                        listOfClosestLineObj_With_Index_ForGroup.Add(closestLineObjId);

                        var indexOfBeamLineExist = BeamHelper.listOfBeamLineId_ForMoveInBeamLayout.IndexOf(baseLineId);
                        if (indexOfBeamLineExist == -1)
                        {
                            BeamHelper beamHelper = new BeamHelper();
                            beamHelper.drawBeamWallPanel(sunkanSlabId, beamLayerNameInsctToWall, beamId, distanceBtwWallToBeam, nearestBeamWallLineId, listOfWallPanelRectId, listOfWallPanelLine_ObjId, baseLineId);
                        }

                        SunkanSlabHelper internalSunkanSlabHlp = new SunkanSlabHelper();
                        internalSunkanSlabHlp.drawSunkanSlabWallPanel(flagOfSunkanSlab, sunkanSlabId, listOfWallPanelRectId, listOfWallPanelLine_ObjId, baseLineId);
                    }
                }
            }
        }


        public List<List<Point3d>> getNearestParallelBaseLinePoint(Line baseLine, Point3d baseLineStartPoint, Point3d baseLineEndPoint, ObjectId closestLineId, Point3d closestStartPoint, Point3d closestEndPoint, double offsetValue)
        {
            List<List<Point3d>> listOfParallelPoint = new List<List<Point3d>>();

            List<double> listOfOutPoint = new List<double>();

            var listOfUpperDir_Point = AEE_Utility.GetParallelLinePoints_In_UpperDir(closestStartPoint.X, closestStartPoint.Y, closestEndPoint.X, closestEndPoint.Y, offsetValue);

            var listOfLowerDir_Point = AEE_Utility.GetParallelLinePoints_In_DownDir(closestStartPoint.X, closestStartPoint.Y, closestEndPoint.X, closestEndPoint.Y, offsetValue);

            var upperLineId = AEE_Utility.getLineId(listOfUpperDir_Point[0], listOfUpperDir_Point[1], 0, listOfUpperDir_Point[2], listOfUpperDir_Point[3], 0, false);
            var lowerLineId = AEE_Utility.getLineId(listOfLowerDir_Point[0], listOfLowerDir_Point[1], 0, listOfLowerDir_Point[2], listOfLowerDir_Point[3], 0, false);
            double upperLength = AEE_Utility.GetDistanceBetweenTwoLines(baseLine.ObjectId, upperLineId);
            double lowerLength = AEE_Utility.GetDistanceBetweenTwoLines(baseLine.ObjectId, lowerLineId);

            AEE_Utility.deleteEntity(upperLineId);
            AEE_Utility.deleteEntity(lowerLineId);

            if (upperLength > lowerLength)
            {
                listOfOutPoint = listOfLowerDir_Point;
            }
            else
            {
                listOfOutPoint = listOfUpperDir_Point;
            }

            var strtLine_Perpend_Id = AEE_Utility.getLineId(closestStartPoint.X, closestStartPoint.Y, 0, listOfOutPoint[0], listOfOutPoint[1], 0, false);
            var endLine_Perpend_Id = AEE_Utility.getLineId(closestEndPoint.X, closestEndPoint.Y, 0, listOfOutPoint[2], listOfOutPoint[3], 0, false);
            var strtLine_Perpend_InsctPoint = AEE_Utility.InterSectionBetweenTwoEntity(strtLine_Perpend_Id, baseLine.ObjectId);
            var endLine_Perpend_InsctPoint = AEE_Utility.InterSectionBetweenTwoEntity(endLine_Perpend_Id, baseLine.ObjectId);

            if (strtLine_Perpend_InsctPoint.Count != 0)
            {
                listOfParallelPoint.Add(strtLine_Perpend_InsctPoint);
            }
            else
            {
                List<Point3d> listOfStrtPoint = new List<Point3d>();
                listOfStrtPoint.Add(new Point3d(listOfOutPoint[0], listOfOutPoint[1], 0));
                //listOfStrtPoint.Add(closestStartPoint);
                listOfParallelPoint.Add(listOfStrtPoint);
            }
            if (endLine_Perpend_InsctPoint.Count != 0)
            {
                listOfParallelPoint.Add(endLine_Perpend_InsctPoint);
            }
            else
            {
                List<Point3d> listOfEndPoint = new List<Point3d>();
                listOfEndPoint.Add(new Point3d(listOfOutPoint[2], listOfOutPoint[3], 0));
                //listOfEndPoint.Add(closestEndPoint);
                listOfParallelPoint.Add(listOfEndPoint);
            }

            AEE_Utility.deleteEntity(strtLine_Perpend_Id);
            AEE_Utility.deleteEntity(endLine_Perpend_Id);

            return listOfParallelPoint;
        }


        private bool checkBaseLineIntersectToClosestLine_ForGroup(List<ObjectId> listOfClosestBaseline_ObjId, Point3d baseLineStrtPoint, Point3d baseLineEndPoint, out int closestLineObj_Index, out ObjectId closestLineObjId)
        {
            closestLineObj_Index = 0;
            closestLineObjId = new ObjectId();
            var baseLineId = AEE_Utility.getLineId(baseLineStrtPoint.X, baseLineStrtPoint.Y, 0, baseLineEndPoint.X, baseLineEndPoint.Y, 0, false);

            var offsetUpperLineId = AEE_Utility.OffsetLine(baseLineId, PanelLayout_UI.maxWallThickness, false);
            var offsetLowerLineId = AEE_Utility.OffsetLine(baseLineId, -PanelLayout_UI.maxWallThickness, false);
            List<double> listOfUpperLineStrtEndPoint = AEE_Utility.getStartEndPointWithAngle_Line(offsetUpperLineId);
            List<double> listOfLowerLineStrtEndPoint = AEE_Utility.getStartEndPointWithAngle_Line(offsetLowerLineId);

            AEE_Utility.deleteEntity(baseLineId);
            AEE_Utility.deleteEntity(offsetUpperLineId);
            AEE_Utility.deleteEntity(offsetLowerLineId);

            double upperMinX = Math.Min(listOfUpperLineStrtEndPoint[0], listOfUpperLineStrtEndPoint[2]);
            double upperMaxX = Math.Max(listOfUpperLineStrtEndPoint[0], listOfUpperLineStrtEndPoint[2]);
            double upperMinY = Math.Min(listOfUpperLineStrtEndPoint[1], listOfUpperLineStrtEndPoint[3]);
            double upperMaxY = Math.Max(listOfUpperLineStrtEndPoint[1], listOfUpperLineStrtEndPoint[3]);
            double upperCenter_X = upperMinX + ((upperMaxX - upperMinX) / 2);
            double upperCenter_Y = upperMinY + ((upperMaxY - upperMinY) / 2);

            double lowerMinX = Math.Min(listOfLowerLineStrtEndPoint[0], listOfLowerLineStrtEndPoint[2]);
            double lowerMaxX = Math.Max(listOfLowerLineStrtEndPoint[0], listOfLowerLineStrtEndPoint[2]);
            double lowerMinY = Math.Min(listOfLowerLineStrtEndPoint[1], listOfLowerLineStrtEndPoint[3]);
            double lowerMaxY = Math.Max(listOfLowerLineStrtEndPoint[1], listOfLowerLineStrtEndPoint[3]);
            double lowerCenter_X = lowerMinX + ((lowerMaxX - lowerMinX) / 2);
            double lowerCenter_Y = lowerMinY + ((lowerMaxY - lowerMinY) / 2);

            var centerBaseLineId = AEE_Utility.getLineId(lowerCenter_X, lowerCenter_Y, 0, upperCenter_X, upperCenter_Y, 0, false);

            bool flagOfExistPoint = false;
            for (int m = 0; m < listOfClosestBaseline_ObjId.Count; m++)
            {
                var closestLineId = listOfClosestBaseline_ObjId[m];
                var listOfInsct = AEE_Utility.InterSectionBetweenTwoEntity(centerBaseLineId, closestLineId);
                if (listOfInsct.Count != 0)
                {
                    closestLineObj_Index = m;
                    closestLineObjId = closestLineId;
                    flagOfExistPoint = true;
                    break;
                }
            }

            AEE_Utility.deleteEntity(centerBaseLineId);

            return flagOfExistPoint;
        }
        private void drawBaseLineWall_IfClosestLineCountIsZero(string beamLayerNameInsctToWall, ObjectId beamId_BaseLine, double distanceBtwWallToBeam_BaseLine, ObjectId nearestBeamWallLineId_BaseLine, ObjectId baseLineId, Line baseLine, bool flagOfSunkanSlab, ObjectId sunkanSlabId, ObjectId parapetObjId_BaseLine)
        {
            bool flag_X_Axis = false;
            bool flag_Y_Axis = false;

            double baseLineAngle = AEE_Utility.GetAngleOfLine(baseLine);
            CommonModule commonModule_Obj = new CommonModule();
            commonModule_Obj.checkAngleOfLine_Axis(baseLineAngle, out flag_X_Axis, out flag_Y_Axis);

            List<Point3d> listOfStrtEndPoint = commonModule_Obj.getStartEndPointOfLine(baseLine);
            Point3d baseLineStartPoint = listOfStrtEndPoint[0];
            Point3d baseLineEndPoint = listOfStrtEndPoint[1];

            List<ObjectId> listOfClosestLine_ObjId = new List<ObjectId>();

            getAllPointOfBaseWallPanel(beamLayerNameInsctToWall, beamId_BaseLine, distanceBtwWallToBeam_BaseLine, nearestBeamWallLineId_BaseLine, baseLine, baseLineStartPoint, baseLineEndPoint, listOfClosestLine_ObjId, flag_X_Axis, flag_Y_Axis, flagOfSunkanSlab, sunkanSlabId, parapetObjId_BaseLine);

            //listOfExistsBaseWallPanelLine.Add(baseLine);
            //listOfExistsBaseWallPanelLine_ObjId.Add(baseLineId);
            listOfAllExistsWallPanelLine.Add(baseLine);
            //listOfAllExistsWallPanelLine_ObjId.Add(baseLineId);
        }

        private void getRemainingLine(string xDataRegAppName, ObjectId wallLineId, string beamLayerNameInsctToWall, ObjectId beamId, double distanceBtwWallToBeam, ObjectId nearestBeamWallLineId, Entity closestEnt_ForSameProprty, Point3d actualClosestStartPoint, Point3d actualClosestEndPoint, List<Point3d> listOfInsctClosestLineStrtEndPoint, Point3d actualClosestOffstStartPoint, Point3d actualClosestOffstEndPoint, List<Point3d> listOfInsctClosestOffstLineStrtEndPoint, bool flag_X_Axis, bool flag_Y_Axis, bool flagOfSunkanSlab, ObjectId sunkanSlabId, ObjectId parapetObjId)
        {
            CornerHelper cornerHlp = new CornerHelper();

            Point3d insctClosestStartPoint = listOfInsctClosestLineStrtEndPoint[0];
            Point3d insctClosestEndPoint = listOfInsctClosestLineStrtEndPoint[1];

            Point3d insctClosestOffstStartPoint = listOfInsctClosestOffstLineStrtEndPoint[0];
            Point3d insctClosestOffstEndPoint = listOfInsctClosestOffstLineStrtEndPoint[1];

            if (actualClosestStartPoint != insctClosestStartPoint)
            {
                var strtLineId = AEE_Utility.getLineId(closestEnt_ForSameProprty, actualClosestStartPoint.X, actualClosestStartPoint.Y, 0, insctClosestStartPoint.X, insctClosestStartPoint.Y, 0, false);
                var offsetStrtLineId = AEE_Utility.getLineId(closestEnt_ForSameProprty, actualClosestOffstStartPoint.X, actualClosestOffstStartPoint.Y, 0, insctClosestOffstStartPoint.X, insctClosestOffstStartPoint.Y, 0, false);

                double length = 0;
                if (flag_X_Axis == true)
                {
                    length = Math.Truncate(Math.Abs(actualClosestStartPoint.X - insctClosestStartPoint.X));
                }
                else if (flag_Y_Axis == true)
                {
                    length = Math.Truncate(Math.Abs(actualClosestStartPoint.Y - insctClosestStartPoint.Y));
                }
                if (length != 0)
                {
                    AEE_Utility.AttachXData(strtLineId, xDataRegAppName, CommonModule.xDataAsciiName);
                    listOfAllWallLines.Add(AEE_Utility.GetLine(strtLineId));
                    InternalWallHelper.listOfLinesBtwTwoCrners_ObjId.Add(strtLineId);
                    InternalWallHelper.listOfOffsetLinesBtwTwoCrners_ObjId.Add(offsetStrtLineId);
                    BeamHelper.listOfLinesBtwTwoCrners_BeamInsctName.Add(beamLayerNameInsctToWall);
                    BeamHelper.listOfLinesBtwTwoCrners_BeamInsctId.Add(beamId);
                    BeamHelper.listOfDistanceBtwWallToBeam.Add(distanceBtwWallToBeam);
                    BeamHelper.listOfNearestBtwWallToBeamWallLineId.Add(nearestBeamWallLineId);

                    SunkanSlabHelper.listOfLinesBtwTwoCrners_SunkanSlabWallLineId.Add(strtLineId);
                    SunkanSlabHelper.listOfLinesBtwTwoCrners_SunkanSlabWallLineflag.Add(flagOfSunkanSlab);
                    SunkanSlabHelper.listOfLinesBtwTwoCrners_SunkanSlabId.Add(sunkanSlabId);
                    if (flagOfSunkanSlab && !SunkanSlabHelper.listOfWallName_SunkanSlabId.ContainsKey(xDataRegAppName))
                        SunkanSlabHelper.listOfWallName_SunkanSlabId.Add(xDataRegAppName, sunkanSlabId);

                    ParapetHelper.listOfWallLineId_InsctToParapet.Add(strtLineId);
                    ParapetHelper.listOfParapetId_WithWallLine.Add(parapetObjId);

                    cornerHlp.setCornerIdRemainingWallLineId_ExternalCrnr(wallLineId, strtLineId);
                    cornerHlp.setCornerIdRemainingWallLineId_InternalCrnr(wallLineId, strtLineId);
                }
            }
            if (actualClosestEndPoint != insctClosestEndPoint)
            {
                var endLineId = AEE_Utility.getLineId(closestEnt_ForSameProprty, actualClosestEndPoint.X, actualClosestEndPoint.Y, 0, insctClosestEndPoint.X, insctClosestEndPoint.Y, 0, false);
                var offsetEndLineId = AEE_Utility.getLineId(closestEnt_ForSameProprty, actualClosestOffstEndPoint.X, actualClosestOffstEndPoint.Y, 0, insctClosestOffstEndPoint.X, insctClosestOffstEndPoint.Y, 0, false);

                double length = 0;
                if (flag_X_Axis == true)
                {
                    length = Math.Truncate(Math.Abs(actualClosestEndPoint.X - insctClosestEndPoint.X));
                }
                else if (flag_Y_Axis == true)
                {
                    length = Math.Truncate(Math.Abs(actualClosestEndPoint.Y - insctClosestEndPoint.Y));
                }
                if (length != 0)
                {
                    AEE_Utility.AttachXData(endLineId, xDataRegAppName, CommonModule.xDataAsciiName);
                    listOfAllWallLines.Add(AEE_Utility.GetLine(endLineId));
                    InternalWallHelper.listOfLinesBtwTwoCrners_ObjId.Add(endLineId);
                    InternalWallHelper.listOfOffsetLinesBtwTwoCrners_ObjId.Add(offsetEndLineId);
                    BeamHelper.listOfLinesBtwTwoCrners_BeamInsctName.Add(beamLayerNameInsctToWall);
                    BeamHelper.listOfLinesBtwTwoCrners_BeamInsctId.Add(beamId);
                    BeamHelper.listOfDistanceBtwWallToBeam.Add(distanceBtwWallToBeam);
                    BeamHelper.listOfNearestBtwWallToBeamWallLineId.Add(nearestBeamWallLineId);

                    SunkanSlabHelper.listOfLinesBtwTwoCrners_SunkanSlabWallLineId.Add(endLineId);
                    SunkanSlabHelper.listOfLinesBtwTwoCrners_SunkanSlabWallLineflag.Add(flagOfSunkanSlab);
                    SunkanSlabHelper.listOfLinesBtwTwoCrners_SunkanSlabId.Add(sunkanSlabId);
                    if (flagOfSunkanSlab && !SunkanSlabHelper.listOfWallName_SunkanSlabId.ContainsKey(xDataRegAppName))
                        SunkanSlabHelper.listOfWallName_SunkanSlabId.Add(xDataRegAppName, sunkanSlabId);

                    ParapetHelper.listOfWallLineId_InsctToParapet.Add(endLineId);
                    ParapetHelper.listOfParapetId_WithWallLine.Add(parapetObjId);

                    cornerHlp.setCornerIdRemainingWallLineId_ExternalCrnr(wallLineId, endLineId);
                    cornerHlp.setCornerIdRemainingWallLineId_InternalCrnr(wallLineId, endLineId);
                }
            }
        }

        private void createGroupOfClosesAndBaseLine(List<ObjectId> listOfClosestBaseline_ObjId)
        {
            for (int i = 0; i < listOfListClosestLineWallPanelRect_ObjId.Count; i++)
            {
                ObjectId closestLineId = listOfClosestBaseline_ObjId[i];
                var listOfClosestLinePanelRect_Obj = listOfListClosestLineWallPanelRect_ObjId[i];
                var listOfClosestLinePanelText_Obj = listOfListClosestLineWallPanelText_ObjId[i];
                var listClosestLineWallXText_ObjId = listOfListClosestLineWallXText_ObjId[i];

                if (listOfListBaseLineWallPanelRect_ObjId.Count == i)
                {
                    break;
                }
                int indexOfClosestLineId = listOfClosestLineObj_With_Index_ForGroup.IndexOf(closestLineId);
                if (indexOfClosestLineId != -1)
                {
                    var listOfBaseLinePanelRect_Obj = listOfListBaseLineWallPanelRect_ObjId[indexOfClosestLineId];
                    var listOfBaseLinePanelText_Obj = listOfListBaseLineWallPanelText_ObjId[indexOfClosestLineId];
                    var listBaseLineWallXText_ObjId = listOfListBaseLineWallXText_ObjId[indexOfClosestLineId];
                    for (int j = 0; j < listOfClosestLinePanelRect_Obj.Count; j++)
                    {
                        if (listOfBaseLinePanelRect_Obj.Count == j)
                        {
                            break;
                        }

                        List<ObjectId> listOfObjId_ForGroupCreator = new List<ObjectId>();
                        if (j < listClosestLineWallXText_ObjId.Count)
                        {
                            var closestLineXText_Obj = listClosestLineWallXText_ObjId[j];
                            listOfObjId_ForGroupCreator.Add(closestLineXText_Obj);
                        }
                        if (j < listOfClosestLinePanelRect_Obj.Count)
                        {
                            var closestLinePanelRect_Obj = listOfClosestLinePanelRect_Obj[j];
                            listOfObjId_ForGroupCreator.Add(closestLinePanelRect_Obj);
                        }
                        if (j < listOfClosestLinePanelText_Obj.Count)
                        {
                            var closestLinePanelText_Obj = listOfClosestLinePanelText_Obj[j];
                            listOfObjId_ForGroupCreator.Add(closestLinePanelText_Obj);
                        }

                        if (j < listOfBaseLinePanelRect_Obj.Count)
                        {
                            var baseLinePanelRect_Obj = listOfBaseLinePanelRect_Obj[j];
                            listOfObjId_ForGroupCreator.Add(baseLinePanelRect_Obj);
                        }
                        if (j < listOfBaseLinePanelText_Obj.Count)
                        {
                            var baseLinePanelText_Obj = listOfBaseLinePanelText_Obj[j];
                            listOfObjId_ForGroupCreator.Add(baseLinePanelText_Obj);
                        }
                        if (j < listBaseLineWallXText_ObjId.Count)
                        {
                            var baseLineXText_Obj = listBaseLineWallXText_ObjId[j];
                            listOfObjId_ForGroupCreator.Add(baseLineXText_Obj);
                        }

                        if (listOfObjId_ForGroupCreator.Count != 0)
                        {
                            CommonModule commonMod = new CommonModule();
                            commonMod.createGroup(listOfObjId_ForGroupCreator);
                        }
                    }
                }
            }
        }
        public List<ObjectId> drawWallPanels(string xDataRegAppName, ObjectId wallLineId, Point3d baseStrtPoint, Point3d baseEndPoint, Point3d offsetBaseStrtPoint, Point3d offsetBaseEndPoint, bool flag_X_Axis, bool flag_Y_Axis, List<double> listOfWallPanelLength, out List<ObjectId> listOfWallPanelLine_ObjId)
        {
            List<ObjectId> listOfWallPanelRect_ObjId = new List<ObjectId>();
            listOfWallPanelLine_ObjId = new List<ObjectId>();

            double lineAngle = AEE_Utility.GetAngleOfLine(baseStrtPoint.X, baseStrtPoint.Y, baseEndPoint.X, baseEndPoint.Y);

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
                    if (length > 100)
                    {

                        var commonModule = new CommonModule();
                        var wallLine = AEE_Utility.GetLine(wallLineId);
                        var name = AEE_Utility.GetXDataRegisterAppName(wallLineId);
                        double angleOfLine = AEE_Utility.GetAngleOfLine(wallLine.StartPoint.X, wallLine.StartPoint.Y, wallLine.EndPoint.X, wallLine.EndPoint.Y);
                        bool _flag_X_Axis = false;
                        bool _flag_Y_Axis = false;
                        commonModule.checkAngleOfLine_Axis(angleOfLine, out _flag_X_Axis, out _flag_Y_Axis);
                        if (flag_X_Axis != _flag_X_Axis)
                        {
                            xDataRegAppName = "W1_R_1";
                        }

                    }
                    var objId = AEE_Utility.createRectangle(startPoint, endPoint, offstEndPoint, offsetStartPoint, CommonModule.wallPanelLyrName, CommonModule.wallPanelLyrColor);
                    AEE_Utility.AttachXData(objId, xDataRegAppName, CommonModule.xDataAsciiName);


                    WP_Cline_data data = new WP_Cline_data();
                    data.panelname = xDataRegAppName;
                    if (xDataRegAppName.Contains("_EX_"))
                    {
                        if (flag_X_Axis)
                        {
                            data.startpoint = new Point3d(startPoint.X, startPoint.Y, startPoint.Z);
                            data.endpoint = new Point3d(endPoint.X, endPoint.Y, endPoint.Z);

                        }
                        else
                        {
                            data.startpoint = new Point3d(startPoint.X, startPoint.Y, startPoint.Z);
                            data.endpoint = new Point3d(endPoint.X, endPoint.Y, endPoint.Z);
                        }

                    }
                    else
                    {
                        if (flag_X_Axis)
                        {
                            data.startpoint = new Point3d(startPoint.X, startPoint.Y, startPoint.Z);
                            data.endpoint = new Point3d(endPoint.X, endPoint.Y, endPoint.Z);

                        }
                        else
                        {
                            data.startpoint = new Point3d(startPoint.X, startPoint.Y, startPoint.Z);
                            data.endpoint = new Point3d(endPoint.X, endPoint.Y, endPoint.Z);
                        }
                    }
                    data.xaxis = flag_X_Axis;
                    CommonModule.wallpanel_pnts.Add(data);





                    var wallPanelLineId = AEE_Utility.getLineId(startPoint.X, startPoint.Y, 0, endPoint.X, endPoint.Y, 0, false);
                    AEE_Utility.AttachXData(wallPanelLineId, xDataRegAppName, CommonModule.xDataAsciiName);

                    listOfWallPanelRect_ObjId.Add(objId);
                    listOfWallPanelLine_ObjId.Add(wallPanelLineId);
                    startPoint = endPoint;
                    offsetStartPoint = offstEndPoint;

                }
            }

            if (listOfWallPanelLength.Count == 0)
            {
                double length = 0;
                if (flag_X_Axis == true)
                {
                    length = Math.Truncate(Math.Abs(startPoint.X - baseEndPoint.X));
                }
                else if (flag_Y_Axis == true)
                {
                    length = Math.Truncate(Math.Abs(startPoint.Y - baseEndPoint.Y));
                }
                if (length != 0)
                {
                    var wallPanelLineId = AEE_Utility.getLineId(startPoint.X, startPoint.Y, 0, baseEndPoint.X, baseEndPoint.Y, 0, false);
                    AEE_Utility.AttachXData(wallPanelLineId, xDataRegAppName, CommonModule.xDataAsciiName);

                    var objId = AEE_Utility.createRectangle(startPoint, baseEndPoint, offsetBaseEndPoint, offsetStartPoint, CommonModule.wallPanelLyrName, CommonModule.wallPanelLyrColor);
                    AEE_Utility.AttachXData(objId, xDataRegAppName, CommonModule.xDataAsciiName);


                    WP_Cline_data data = new WP_Cline_data();
                    data.panelname = xDataRegAppName;
                    if (xDataRegAppName.Contains("_EX_"))
                    {

                        data.startpoint = new Point3d(startPoint.X, startPoint.Y, startPoint.Z);
                        data.endpoint = new Point3d(baseEndPoint.X, baseEndPoint.Y, baseEndPoint.Z);

                        data.startpoint = new Point3d(offsetBaseEndPoint.X, offsetBaseEndPoint.Y, offsetBaseEndPoint.Z);
                        data.endpoint = new Point3d(baseEndPoint.X, baseEndPoint.Y, baseEndPoint.Z);



                    }
                    else
                    {

                        data.startpoint = new Point3d(startPoint.X, startPoint.Y, startPoint.Z);
                        data.endpoint = new Point3d(baseEndPoint.X, baseEndPoint.Y, baseEndPoint.Z);

                        data.startpoint = new Point3d(offsetBaseEndPoint.X, offsetBaseEndPoint.Y, offsetBaseEndPoint.Z);
                        data.endpoint = new Point3d(baseEndPoint.X, baseEndPoint.Y, baseEndPoint.Z);





                    }
                    data.xaxis = flag_X_Axis;
                    CommonModule.wallpanel_pnts.Add(data);





                    listOfWallPanelRect_ObjId.Add(objId);
                    listOfWallPanelLine_ObjId.Add(wallPanelLineId);
                }
            }
            return listOfWallPanelRect_ObjId;
        }

        public List<double> getListOfWallPanelLength(double lengthOfLine, double standardPanelWidth, double minWidthOfPanel)
        {
            lengthOfLine = Math.Round(lengthOfLine);

            List<double> listOfWallPanelLength = new List<double>();
            if (lengthOfLine > standardPanelWidth)
            {
                bool flagOfLastWall = false;
                if (lengthOfLine >= ((2 * standardPanelWidth) + minWidthOfPanel))
                {
                    flagOfLastWall = true;
                    lengthOfLine = lengthOfLine - standardPanelWidth;
                }
                double qtyOfPanelWall = lengthOfLine / standardPanelWidth;
                double qtyOfStandardPanelWall = Math.Truncate(qtyOfPanelWall);
                if ((qtyOfPanelWall % 1) != 0)
                {
                    double totalLength = qtyOfStandardPanelWall * standardPanelWidth;
                    double difference = Math.Abs(lengthOfLine - totalLength);
                    double subtractLength = (standardPanelWidth + difference) - minWidthOfPanel;
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
                                listOfWallPanelLength.Add(standardPanelWidth);
                            }
                        }
                        listOfWallPanelLength.Add(minWidthOfPanel);
                    }
                    else
                    {
                        for (int i = 0; i < qtyOfStandardPanelWall; i++)
                        {
                            listOfWallPanelLength.Add(standardPanelWidth);
                        }
                        listOfWallPanelLength.Add(difference);
                    }
                }
                else
                {
                    for (int i = 0; i < qtyOfStandardPanelWall; i++)
                    {
                        listOfWallPanelLength.Add(standardPanelWidth);
                    }
                }
                if (flagOfLastWall == true)
                {
                    listOfWallPanelLength.Add(standardPanelWidth);
                }
            }
            else
            {
                listOfWallPanelLength.Add(lengthOfLine);
            }
            return listOfWallPanelLength;
        }

        public void writeTextInWallPanel(string wallType, ObjectId sunkanId, string wallPanelLayerName,
                                         List<ObjectId> listOfWallPanelRect_Id, List<ObjectId> listOfWallPanelLine_ObjId,
                                         double wallHeight, string wallHeightText, double wallXHeight, string wallXHeightText,
                                         double RC_WithLevelDiff, string layerName, int layerColor, bool flagOfRCWrite,
                                         out List<ObjectId> listOfTextId, out List<ObjectId> listOfWallXTextId,
                                         out List<ObjectId> listOfCircleId, string doorLayer, string windowLayer, double ele = 0.0, double? wallLen = null, bool flag_X_Axis = false, bool flag_Y_Axis = false,double wallDepth=0)//RTJ 10-06-2021 //Added argument bool flag_Y_Axis ,bool flag_X_Axis.
        {
            WallPanelHelper wallPanelHlp = new WallPanelHelper();

            listOfCircleId = new List<ObjectId>();

            listOfTextId = new List<ObjectId>();
            listOfWallXTextId = new List<ObjectId>();
            for (int index = 0; index < listOfWallPanelRect_Id.Count; index++)
            {
                ObjectId wallPanelRect_Id = listOfWallPanelRect_Id[index];
                if (wallPanelRect_Id.IsErased == false)
                {
                    ObjectId wallPanelLine_Id = listOfWallPanelLine_ObjId[index];
                    double lengthOfWallPanel = AEE_Utility.GetLengthOfLine(wallPanelLine_Id);
                    if (wallLen != null)
                    {
                        lengthOfWallPanel = wallLen.Value;
                    }
                    lengthOfWallPanel = Math.Round(lengthOfWallPanel);
                    wallHeight = Math.Round(wallHeight);
                    wallXHeight = Math.Round(wallXHeight);
                    double angleOfWallPanel = AEE_Utility.GetAngleOfLine(wallPanelLine_Id);
                    List<string> listOfWallPanelText_With_RC = new List<string>();
                    string wallPanelText = getWallPanelText(wallPanelLayerName, wallHeight, lengthOfWallPanel, wallHeightText, RC_WithLevelDiff, PanelLayout_UI.flagOfPanelWithRC,
                                                            sunkanId, flagOfRCWrite, out listOfWallPanelText_With_RC);
                    string wallDescp = listOfWallPanelText_With_RC[0];
                    string wallTopPanelText = getWallTopPanelText(wallXHeight, lengthOfWallPanel, wallXHeightText);
                    var listOfPanelRectVertex = AEE_Utility.GetPolylineVertexPoint(wallPanelRect_Id);
                    Point3d dimTextPoint = getCenterPointOfPanelRectangle(wallPanelRect_Id);

                    var textId = writeDimensionTextInWallPanel(wallPanelText, wallPanelLine_Id, dimTextPoint, angleOfWallPanel, layerName, layerColor);
                    if (textId.IsValid == true)
                    {
                        string itemCode = wallHeightText;
                        //RTJ Start 10-06-2021
                        if (windowLayer != "")
                        {
                            listOfWindowLineIdAndLayerName.Add(wallPanelLine_Id, windowLayer); //RTJ 10-06-2021
                        }
                        //RTJ End 10-06-2021

                        wallPanelHlp.setBOMDataOfWallPanel(textId, wallPanelRect_Id, wallPanelLine_Id, lengthOfWallPanel, wallHeight, wallDepth, itemCode, wallDescp, wallType, ele.ToString(), flag_X_Axis, flag_Y_Axis);
                        listOfTextId.Add(textId);

                        var circleId = createCircleInNonStandardWallPanel(dimTextPoint.X, dimTextPoint.Y, lengthOfWallPanel);
                        //listOfCircleId.Add(circleId);

                        if (circleId.IsValid == true)
                        {
                            listOfCircleId.Add(circleId);
                        }
                        if (listOfWallPanelText_With_RC.Count == 2)
                        {
                            string RC_Descp = listOfWallPanelText_With_RC[1];
                            string itemCode_RC = "RC";
                            //RTJ Start 10-06-2021
                            if (doorLayer != "")
                            {
                                listOfDoorLineIdAndLayerName.Add(wallPanelLine_Id, doorLayer);//RTJ 10-06-2021
                            }
                            //RTJ end 10-06-2021
                            wallPanelHlp.setBOMDataOfWallPanel(textId, wallPanelRect_Id, wallPanelLine_Id, lengthOfWallPanel, PanelLayout_UI.RC, 0, itemCode_RC, RC_Descp,
                                                               CommonModule.rockerPanelType);
                        }
                    }

                    Point3d wallPanel_X_MidPoint = getTextPosition_HeightOfWallX(listOfPanelRectVertex, angleOfWallPanel, wallPanelRect_Id, lengthOfWallPanel);
                    textId = writeDimensionTextInWallPanel(wallTopPanelText, wallPanelLine_Id, wallPanel_X_MidPoint, angleOfWallPanel, layerName, layerColor);
                    if (textId.IsValid == true)
                    {
                        wallDescp = wallTopPanelText;
                        string itemCode = wallXHeightText;
                        //This collection of Retriving the doorLayer Names..
                        //RTJ Start 10-06-2021
                        if (doorLayer != "")
                        {
                            listOfDoorLineIdAndLayerName.Add(wallPanelLine_Id, doorLayer);//RTJ 10-06-2021
                        }
                        else if (windowLayer != "")
                        {
                            if (!listOfWindowLineIdAndLayerName.ContainsKey(wallPanelLine_Id))
                            {
                                listOfWindowLineIdAndLayerName.Add(wallPanelLine_Id, windowLayer); //RTJ 10-06-2021
                            }
                        }
                        //Fix TP panels elevation By SDM 16-07-2022
                        else
                            ele = wallHeight;
                        //RTJ Ends 10-06-2021
                        // TODO: added elevation parameter
                        wallPanelHlp.setBOMDataOfWallPanel(textId, wallPanelRect_Id, wallPanelLine_Id, lengthOfWallPanel, wallXHeight, 0, itemCode, wallDescp, wallType, ele.ToString());
                        listOfWallXTextId.Add(textId);
                    }
                }
            }
        }


        public static Point3d getCenterPointOfPanelRectangle(ObjectId wallPanelRect_Id)
        {
            var listOfPanelRectVertex = AEE_Utility.GetPolylineVertexPoint(wallPanelRect_Id);
            double minX = listOfPanelRectVertex.Min(point => point.X);
            double maxX = listOfPanelRectVertex.Max(point => point.X);
            double minY = listOfPanelRectVertex.Min(point => point.Y);
            double maxY = listOfPanelRectVertex.Max(point => point.Y);

            double center_X = minX + ((maxX - minX) / 2);
            double center_Y = minY + ((maxY - minY) / 2);
            Point3d dimTextPoint = new Point3d(center_X, center_Y, 0);

            return dimTextPoint;
        }


        public ObjectId writeDimensionTextInWallPanel(string text, ObjectId wallPanelLine_Id, Point3d dimTextPoint, double angleOfWallPanel, string layerName, int layerColor)
        {
            ObjectId textId = new ObjectId();

            if (string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text))
            {
                return textId;
            }
            else
            {
                //textId = AEE_Utility.CreateTextWithAngle(text, dimTextPoint.X, dimTextPoint.Y, 0, CommonModule.wallPanelTextHght, layerName, layerColor, angleOfWallPanel);

                textId = AEE_Utility.CreateMultiLineText(text, dimTextPoint.X, dimTextPoint.Y, 0, CommonModule.wallPanelTextHght, layerName, layerColor, angleOfWallPanel);

                ////////string dimText = "<>" + " " + wallHeightText + " " + Convert.ToString(wallHeight);
                //////textId = AEE_Utility.CreateAlignedDimWithTextWithStyle(CommonModule.newDimensionStyleName, text, wallPanelLine_Id, dimTextPoint, CommonModule.wallPanelTextHght, layerName, layerColor);
            }
            return textId;
        }


        public Point3d getTextPosition_HeightOfWallX(List<Point2d> listOfPanelRectVertex, double angleOfWallPanel, ObjectId wallPanelRect_Id, double lengthOfWallPanel)
        {
            double minX = listOfPanelRectVertex.Min(point => point.X);
            double maxX = listOfPanelRectVertex.Max(point => point.X);
            double minY = listOfPanelRectVertex.Min(point => point.Y);
            double maxY = listOfPanelRectVertex.Max(point => point.Y);

            double center_X = minX + ((maxX - minX) / 2);
            double center_Y = minY + ((maxY - minY) / 2);

            var newPoint = AEE_Utility.get_XY(angleOfWallPanel, lengthOfWallPanel);
            Point2d strtPoint = new Point2d((center_X - newPoint.X), (center_Y - newPoint.Y));
            Point2d endPoint = new Point2d((center_X + newPoint.X), (center_Y + newPoint.Y));

            var id1 = AEE_Utility.getLineId(strtPoint.X, strtPoint.Y, 0, endPoint.X, endPoint.Y, 0, false);

            var offsetId1 = AEE_Utility.OffsetEntity(5, id1, false);
            var listOfInsctLine = AEE_Utility.InterSectionBetweenTwoEntity(offsetId1, wallPanelRect_Id);

            ObjectId outSideRectLineId = new ObjectId();
            if (listOfInsctLine.Count != 0)
            {
                outSideRectLineId = AEE_Utility.OffsetEntity(CommonModule.panelDepth, id1, false);
            }
            else
            {
                outSideRectLineId = AEE_Utility.OffsetEntity(-CommonModule.panelDepth, id1, false);
            }

            Point3d wallPanel_X_MidPoint = AEE_Utility.GetMidPoint(outSideRectLineId);

            AEE_Utility.deleteEntity(id1);
            AEE_Utility.deleteEntity(offsetId1);
            AEE_Utility.deleteEntity(outSideRectLineId);

            return wallPanel_X_MidPoint;
        }


        public double getHeightOfWall_X(ObjectId sunkanSlabId, bool flagOfPanelWithRC, double RC, string layerName, string beamLayerNameInsctToWall, double standardWallHeight, bool flagOfTopPanel)
        {
            double wall_X_Height = 0;
            if (flagOfTopPanel == false)
            {
                return wall_X_Height;
            }

            var beamBottom = Beam_UI_Helper.getOffsetBeamBottom(beamLayerNameInsctToWall);

            bool flagOfExternalSlab = ExternalSunkanSlab_UI_Helper.checkExternalSunkanSlabLayerIsExist(sunkanSlabId);

            if (InternalWallSlab_UI_Helper.IsInternalWall(layerName) || StairCase_UI_Helper.checkStairCaseLayerIsExist(layerName))
            {
                wall_X_Height = GeometryHelper.getHeightOfWallPanelX_InternalWall(InternalWallSlab_UI_Helper.getSlabBottom(layerName), beamBottom, PanelLayout_UI.SL, standardWallHeight, PanelLayout_UI.RC, CommonModule.internalCorner, sunkanSlabId, flagOfTopPanel);
            }
            else if (layerName == CommonModule.externalWallLayerName || layerName == CommonModule.ductLayerName || Lift_UI_Helper.checkLiftLayerIsExist(layerName))
            {
                wall_X_Height = GeometryHelper.getHeightOfWallPanelX_ExternalWall(InternalWallSlab_UI_Helper.getSlabBottom(layerName), beamBottom, PanelLayout_UI.getSlabThickness(layerName), standardWallHeight, PanelLayout_UI.kickerHt, PanelLayout_UI.RC, sunkanSlabId, flagOfTopPanel);
            }

            if (InternalWallSlab_UI_Helper.IsInternalWall(layerName) || StairCase_UI_Helper.checkStairCaseLayerIsExist(layerName) || flagOfExternalSlab == true)
            {
                if (flagOfPanelWithRC == true)
                {
                    double levelDifferenceOfSunkanSlb = SunkanSlabHelper.getSunkanSlabLevelDifference_WallPanel(sunkanSlabId);

                    //wall_X_Height = wall_X_Height + levelDifferenceOfSunkanSlb;
                }
            }

            return wall_X_Height;
        }


        public static string getWallPanelText(string layerName, double wallHeight, double lengthOfWallPanel, string wallHeightText, double RC, bool flagOfPanelWithRC, ObjectId sunkanSlabId, bool flagOfRCWrite, out List<string> listOfWallPanelText_With_RC)
        {
            listOfWallPanelText_With_RC = new List<string>();
            string wallPanelText = "";

            wallHeight = Math.Round(wallHeight);
            lengthOfWallPanel = Math.Round(lengthOfWallPanel);

            if (wallHeight != 0)
            {
                bool flagOfExternalSunkanSlb = ExternalSunkanSlab_UI_Helper.checkExternalSunkanSlabLayerIsExist(sunkanSlabId);

                if (flagOfPanelWithRC == false)
                {
                    //if (sunkanSlabId.IsValid == true)
                    //{
                    //    var flagOfGreater = SunkanSlabHelper.checkSunkanSlabLevelDiffrnce_Is_Greater(sunkanSlabId);
                    //    if (flagOfGreater == false)
                    //    {
                    //        wallPanelText = Convert.ToString(lengthOfWallPanel) + " " + wallHeightText + " " + Convert.ToString(wallHeight);
                    //        listOfWallPanelText_With_RC.Add(wallPanelText);
                    //        return wallPanelText;
                    //    }                      
                    //}

                    if (InternalWallSlab_UI_Helper.IsInternalWall(layerName) || StairCase_UI_Helper.checkStairCaseLayerIsExist(layerName) || flagOfExternalSunkanSlb == true || Window_UI_Helper.checkWindowLayerIsExist(layerName))
                    {
                        var flagOfGreater = SunkanSlabHelper.checkSunkanSlabLevelDiffrnce_Is_Greater(sunkanSlabId);
                        if (flagOfRCWrite == true && flagOfGreater == false)
                        {
                            string wallpanelTextWithoutRC = Convert.ToString(lengthOfWallPanel) + " " + wallHeightText + " " + Convert.ToString(wallHeight);
                            listOfWallPanelText_With_RC.Add(wallpanelTextWithoutRC);

                            string RC_Text = Convert.ToString(RC) + " R " + Convert.ToString(lengthOfWallPanel);
                            listOfWallPanelText_With_RC.Add(RC_Text);

                            RC_Text = "\n                         {\\H0.75x;" + RC_Text + "}";
                            wallPanelText = wallpanelTextWithoutRC + RC_Text;
                        }
                        else
                        {
                            wallPanelText = Convert.ToString(lengthOfWallPanel) + " " + wallHeightText + " " + Convert.ToString(wallHeight);
                            listOfWallPanelText_With_RC.Add(wallPanelText);
                        }
                    }
                    else
                    {
                        wallPanelText = Convert.ToString(lengthOfWallPanel) + " " + wallHeightText + " " + Convert.ToString(wallHeight);
                        listOfWallPanelText_With_RC.Add(wallPanelText);
                    }
                }
                else
                {
                    wallPanelText = Convert.ToString(lengthOfWallPanel) + " " + wallHeightText + " " + Convert.ToString(wallHeight);
                    listOfWallPanelText_With_RC.Add(wallPanelText);
                }
            }
            if (listOfWallPanelText_With_RC.Count == 0)
            {
                listOfWallPanelText_With_RC.Add(wallPanelText);
            }

            return wallPanelText;
        }


        public static string getWallTopPanelText(double wallHeight, double lengthOfWallPanel, string wallHeightText)
        {
            string wallTopPanelText = "";

            wallHeight = Math.Round(wallHeight);
            lengthOfWallPanel = Math.Round(lengthOfWallPanel);

            if (wallHeight > 0)
            {
                wallTopPanelText = Convert.ToString(lengthOfWallPanel) + " " + wallHeightText + " " + Convert.ToString(wallHeight);
            }

            return wallTopPanelText;
        }

        public static void getWallPanelHeight_PanelWithRC(ObjectId sunkanSlabId, bool flagOfPanelWithRC, double wallHeight, string wallPanelText, double RC, string layerName, out double wallHeightWithRC, out string wallPanelTextWithRC)
        {
            wallHeightWithRC = 0;
            wallPanelTextWithRC = "";

            bool flagOfExternalSlab = ExternalSunkanSlab_UI_Helper.checkExternalSunkanSlabLayerIsExist(sunkanSlabId);
            if (InternalWallSlab_UI_Helper.IsInternalWall(layerName) || StairCase_UI_Helper.checkStairCaseLayerIsExist(layerName) || flagOfExternalSlab == true)
            {
                if (flagOfPanelWithRC == true)
                {
                    wallHeightWithRC = wallHeight;

                    if (sunkanSlabId.IsValid == true)
                    {
                        var flagOfGreater = SunkanSlabHelper.checkSunkanSlabLevelDiffrnce_Is_Greater(sunkanSlabId);
                        if (flagOfGreater == true)
                        {
                            wallPanelTextWithRC = wallPanelText;
                            return;
                        }
                    }
                    wallPanelTextWithRC = wallPanelText;
                    if(!(AEE_Utility.CustType==eCustType.WNPanel && (wallPanelText==CommonModule.doorThickWallPanelText || wallPanelText==CommonModule.windowThickWallPanelText)))
                    wallPanelTextWithRC = wallPanelText + "R";

                    if (sunkanSlabId.IsValid == true)
                    {
                        // sunkan slab is available, but kicker layout is not create
                        wallPanelTextWithRC = wallPanelTextWithRC + "1";
                    }
                }
                else
                {
                    wallHeightWithRC = wallHeight;
                    wallPanelTextWithRC = wallPanelText;
                }
            }
            else
            {
                wallHeightWithRC = wallHeight;
                wallPanelTextWithRC = wallPanelText;
            }
        }

        public ObjectId createCircleInNonStandardWallPanel(double centerX, double centerY, double wallLength, double minLength = -1)
        {
            ObjectId circleId = new ObjectId();
            var minLen = minLength < 0 ? PanelLayout_UI.minWidthOfPanel : minLength;
            if (wallLength < minLen)
            {
                circleId = AEE_Utility.CreateCircle(centerX, centerY, 0, (1 * CommonModule.panelDepth), CommonModule.nonStandardPanelLayerName, CommonModule.nonStandardPanelLayerColor);
                AEE_Utility.changeColor(circleId, 1);
            }
            return circleId;
        }






        public void getExternalCornerWallId_ForStretch(ObjectId wallLineId, List<ObjectId> listOfPanelRectId, List<ObjectId> listOfPanelLineId, List<ObjectId> listOfWallPanelTextId, List<ObjectId> listOfTopPanelTextId)
        {
            int indexOfExist = CornerHelper.listOfWallLineId_OfExternalCrner_ForStretch.IndexOf(wallLineId);
            if (indexOfExist != -1)
            {
                var listOfExternalCrnerId_InsctToWallLine = CornerHelper.listOfListOfExternalCrnerId_InsctToWallLine_ForStretch[indexOfExist];
                foreach (var offsetExtrnalCrnrId in listOfExternalCrnerId_InsctToWallLine)
                {
                    for (int index = 0; index < listOfPanelRectId.Count; index++)
                    {
                        var wallRectId = listOfPanelRectId[index];
                        var listOfInsct = AEE_Utility.InterSectionBetweenTwoEntity(offsetExtrnalCrnrId, wallRectId);
                        if (listOfInsct.Count != 0)
                        {
                            setWallPanelRect_Stretch(index, offsetExtrnalCrnrId, wallRectId, listOfPanelLineId, listOfWallPanelTextId, listOfTopPanelTextId);
                            ////AEE_Utility.changeColor(wallRectId, 2);
                            break;
                        }
                    }
                }
            }
        }
        private void setWallPanelRect_Stretch(int index, ObjectId offsetExtrnalCrnrId, ObjectId wallRectId, List<ObjectId> listOfPanelLineId, List<ObjectId> listOfWallPanelTextId, List<ObjectId> listOfTopPanelTextId)
        {
            ObjectId panelLineId = new ObjectId();
            if (index < listOfPanelLineId.Count)
            {
                panelLineId = listOfPanelLineId[index];
            }

            ObjectId wallPanelTextId = new ObjectId();
            if (index < listOfWallPanelTextId.Count)
            {
                wallPanelTextId = listOfWallPanelTextId[index];
            }

            ObjectId topPanelTextId = new ObjectId();
            if (index < listOfTopPanelTextId.Count)
            {
                topPanelTextId = listOfTopPanelTextId[index];
            }

            List<ObjectId> listOfWallPanelId = new List<ObjectId>();
            listOfWallPanelId.Add(wallRectId);
            listOfWallPanelId.Add(panelLineId);
            listOfWallPanelId.Add(wallPanelTextId);
            listOfWallPanelId.Add(topPanelTextId);

            int indexOfExist = listOfWallOffsetCrnrId_ForCheck_ForStretch.IndexOf(offsetExtrnalCrnrId);
            if (indexOfExist == -1)
            {
                listOfWallOffsetCrnrId_ForCheck_ForStretch.Add(offsetExtrnalCrnrId);
                List<List<ObjectId>> listOfListWallPanelId = new List<List<ObjectId>>();
                listOfListWallPanelId.Add(listOfWallPanelId);

                listOfListOfListWallPanelId_ForStretch.Add(listOfListWallPanelId);
            }
            else
            {
                var listOfListWallPanelId = listOfListOfListWallPanelId_ForStretch[indexOfExist];

                listOfListWallPanelId.Add(listOfWallPanelId);
                listOfListOfListWallPanelId_ForStretch[indexOfExist] = listOfListWallPanelId;
            }
        }
        private void stretchWallPanelRect_In_ConcaveAngle()
        {

            for (int index = 0; index < listOfWallOffsetCrnrId_ForCheck_ForStretch.Count; index++)
            {
                ObjectId offstExternalCrnerId = listOfWallOffsetCrnrId_ForCheck_ForStretch[index];
                int indexOfExistCornerId = CornerHelper.listOfOffstExternalCornerId_ForStretch.IndexOf(offstExternalCrnerId);
                if (indexOfExistCornerId != -1)
                {
                    ObjectId externalCrnerId = CornerHelper.listOfExternalCornerId_ForStretch[indexOfExistCornerId];
                    List<ObjectId> externalCrnrTextId = CornerHelper.listOfExternalCornerTextId_ForStretch[indexOfExistCornerId];

                    var listOfListWallPanelId = listOfListOfListWallPanelId_ForStretch[index];
                    if (listOfListWallPanelId.Count == 2 && externalCrnerId.IsErased == false)
                    {
                        var listWallPanelId1 = listOfListWallPanelId[0];
                        ObjectId wallPanelLine1Id = listWallPanelId1[1];
                        double wallPanelLine1_Lngth = AEE_Utility.GetLengthOfLine(wallPanelLine1Id);

                        var listWallPanelId2 = listOfListWallPanelId[1];
                        ObjectId wallPanelLine2Id = listWallPanelId2[1];
                        double wallPanelLine2_Lngth = AEE_Utility.GetLengthOfLine(wallPanelLine2Id);

                        double minLength = Math.Min(wallPanelLine1_Lngth, wallPanelLine2_Lngth);
                        if (minLength < CommonModule.maxPanelThick)
                        {
                            List<ObjectId> listOfMinLengthPanelDataId = new List<ObjectId>();
                            ObjectId maxLngthWallPanelLineId = new ObjectId();

                            var olddata1 = AEE_Utility.GetXDataRegisterAppName(wallPanelLine1Id);
                            var olddata2 = AEE_Utility.GetXDataRegisterAppName(wallPanelLine2Id);
                            if (minLength == wallPanelLine1_Lngth)
                            {
                                listOfMinLengthPanelDataId = listWallPanelId1;
                                maxLngthWallPanelLineId = wallPanelLine2Id;
                            }
                            else
                            {
                                listOfMinLengthPanelDataId = listWallPanelId2;
                                maxLngthWallPanelLineId = wallPanelLine1Id;
                            }
                            stretchWallPanelRectangle(externalCrnerId, externalCrnrTextId, listOfMinLengthPanelDataId, maxLngthWallPanelLineId);

                            olddata1 = AEE_Utility.GetXDataRegisterAppName(wallPanelLine1Id);
                            olddata2 = AEE_Utility.GetXDataRegisterAppName(wallPanelLine2Id);
                        }
                    }

                    Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor.UpdateScreen();

                    //var ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
                    //var res = ed.GetEntity("Select Entity : ");
                    //if (res.Status != PromptStatus.OK)
                    //{
                    //    return;
                    //}
                }
            }


        }
        private void stretchWallPanelRectangle(ObjectId externalCrnerId, List<ObjectId> externalCrnrTextId, List<ObjectId> listOfPanelDataId, ObjectId maxLngthWallPanelLineId)
        {
            try
            {
                var basePointOfExternalCrner = AEE_Utility.GetBasePointOfPolyline(externalCrnerId);

                ObjectId wallPanelRectId = listOfPanelDataId[0];
                ObjectId wallPanelLineId = listOfPanelDataId[1];
                ObjectId wallPanelTextId = listOfPanelDataId[2];
                ObjectId topPanelTextId = listOfPanelDataId[3];

                var wallPanelLineLngth = AEE_Utility.GetLengthOfLine(wallPanelLineId);
                var wallPanelLineAngle = AEE_Utility.GetAngleOfLine(wallPanelLineId);

                List<List<Point3d>> listOfStretchPoint = new List<List<Point3d>>();
                List<int> listOfStretchPointIndex = new List<int>();
                double stretchValue = 0;
                List<ObjectId> listOfStretchLineId = getExternalCornerLineId(externalCrnerId, basePointOfExternalCrner, wallPanelLineId, wallPanelLineAngle, wallPanelRectId, out listOfStretchPoint, out listOfStretchPointIndex, out stretchValue);

                bool flagOfLeft = false;
                bool flagOfRight = false;
                getWallPanelType(wallPanelLineId, maxLngthWallPanelLineId, out flagOfLeft, out flagOfRight);

                stretch(wallPanelRectId, listOfStretchPointIndex, listOfStretchPoint, stretchValue);

                changeWallPanelText_In_ConcaveAngle(wallPanelRectId, wallPanelLineId, wallPanelTextId, topPanelTextId, stretchValue, flagOfLeft, flagOfRight);

                listOfStretchWallPanelCornerAndTextId.Add(externalCrnerId);
                listOfStretchWallPanelCornerAndTextId.AddRange(externalCrnrTextId);
            }
            catch (Exception)
            {


            }

        }

        private void getWallPanelType(ObjectId stretchWallLineId, ObjectId maxLngthWallPanelLineId, out bool flagOfLeft, out bool flagOfRight)
        {
            flagOfLeft = false;
            flagOfRight = false;

            var listOfMaxLngthLineStrtEndPoint = AEE_Utility.GetStartEndPointOfLine(maxLngthWallPanelLineId);
            var listOfStretchWallLineStrtEndPoint = AEE_Utility.GetStartEndPointOfLine(stretchWallLineId);

            double textHeight = 50;
            var stretchWallLineAngle = AEE_Utility.GetAngleOfLine(stretchWallLineId);
            CommonModule commonMod = new CommonModule();
            bool flag_X_Axis = false;
            bool flag_Y_Axis = false;
            commonMod.checkAngleOfLine_Axis(stretchWallLineAngle, out flag_X_Axis, out flag_Y_Axis);

            List<double> listOfWallLineLngth = new List<double>();
            List<Point3d> listOfStretchWallLinePoint = new List<Point3d>();

            List<double> listOfMaxLngthWallLineLngth = new List<double>();
            List<Point3d> listOfMaxLngthWallLinePoint = new List<Point3d>();

            foreach (var maxLngthPoint in listOfMaxLngthLineStrtEndPoint)
            {
                foreach (var strectchWallPoint in listOfStretchWallLineStrtEndPoint)
                {
                    var length = AEE_Utility.GetLengthOfLine(maxLngthPoint, strectchWallPoint);
                    listOfWallLineLngth.Add(length);
                    listOfStretchWallLinePoint.Add(strectchWallPoint);
                    listOfMaxLngthWallLinePoint.Add(maxLngthPoint);
                }
            }

            double minLength = listOfWallLineLngth.Min();
            int indexOfMinLngth = listOfWallLineLngth.IndexOf(minLength);

            Point3d minLngthPointOfStretchWall = listOfStretchWallLinePoint[indexOfMinLngth];
            int indexOfStrectchWallPoint = listOfStretchWallLineStrtEndPoint.IndexOf(minLngthPointOfStretchWall);
            Point3d stretchWallPoint2 = new Point3d();

            bool flagOfStrt = false;
            if (indexOfStrectchWallPoint == 0)
            {
                stretchWallPoint2 = listOfStretchWallLineStrtEndPoint[1];
                flagOfStrt = true;
            }
            else
            {
                stretchWallPoint2 = listOfStretchWallLineStrtEndPoint[0];
                flagOfStrt = false;
            }


            Point3d minLngthPointOfMaxLngthWall = listOfMaxLngthWallLinePoint[indexOfMinLngth];
            int indexOfMaxLngthWallPoint = listOfMaxLngthLineStrtEndPoint.IndexOf(minLngthPointOfStretchWall);
            Point3d maxLngthWallPoint2 = new Point3d();
            if (indexOfMaxLngthWallPoint == 0)
            {
                maxLngthWallPoint2 = listOfMaxLngthLineStrtEndPoint[1];
            }
            else
            {
                maxLngthWallPoint2 = listOfMaxLngthLineStrtEndPoint[0];
            }

            if (flag_X_Axis == true)
            {
                double textY = listOfStretchWallLineStrtEndPoint.Max(sortPoint => sortPoint.Y);
                if (flagOfStrt == true)
                {
                    double diff = maxLngthWallPoint2.Y - minLngthPointOfStretchWall.Y;

                    if (diff <= 0)
                    {
                        flagOfRight = true;
                        //AEE_Utility.CreateTextWithAngle("Right", minLngthPointOfStretchWall.X, textY, 0, textHeight, "A", 3, stretchWallLineAngle);
                    }
                    else
                    {
                        flagOfLeft = true;
                        //AEE_Utility.CreateTextWithAngle("Left", minLngthPointOfStretchWall.X, textY, 0, textHeight, "A", 3, stretchWallLineAngle);
                    }
                }
                else
                {
                    double diff = minLngthPointOfStretchWall.Y - maxLngthWallPoint2.Y;

                    if (diff <= 0)
                    {
                        flagOfRight = true;
                        //AEE_Utility.CreateTextWithAngle("Right", minLngthPointOfStretchWall.X, textY, 0, textHeight, "A", 3, stretchWallLineAngle);
                    }
                    else
                    {
                        flagOfLeft = true;
                        //AEE_Utility.CreateTextWithAngle("Left", minLngthPointOfStretchWall.X, textY, 0, textHeight, "A", 3, stretchWallLineAngle);
                    }
                }
            }




            if (flag_Y_Axis == true)
            {
                double textX = listOfStretchWallLineStrtEndPoint.Max(sortPoint => sortPoint.X);
                if (flagOfStrt == true)
                {
                    double diff = minLngthPointOfStretchWall.X - maxLngthWallPoint2.X;

                    if (diff <= 0)
                    {
                        flagOfRight = true;
                        //AEE_Utility.CreateTextWithAngle("Right", textX, minLngthPointOfStretchWall.Y, 0, textHeight, "A", 3, stretchWallLineAngle);
                    }
                    else
                    {
                        flagOfLeft = true;
                        //AEE_Utility.CreateTextWithAngle("Left", textX, minLngthPointOfStretchWall.Y, 0, textHeight, "A", 3, stretchWallLineAngle);
                    }
                }
                else
                {
                    double diff = maxLngthWallPoint2.X - minLngthPointOfStretchWall.X;

                    if (diff <= 0)
                    {
                        flagOfRight = true;
                        //AEE_Utility.CreateTextWithAngle("Right", textX, minLngthPointOfStretchWall.Y, 0, textHeight, "A", 3, stretchWallLineAngle);
                    }
                    else
                    {
                        flagOfLeft = true;
                        //AEE_Utility.CreateTextWithAngle("Left", textX, minLngthPointOfStretchWall.Y, 0, textHeight, "A", 3, stretchWallLineAngle);
                    }
                }
            }

            //AEE_Utility.getLineId(stretchWallPoint2, minLngthPointOfStretchWall, "StretchLength", 4, true);

            //AEE_Utility.getLineId(minLngthPointOfMaxLngthWall, maxLngthWallPoint2, "MaxLength", 2, true);
        }


        public void stretch(ObjectId wallPanelRectId, List<int> listOfVerticePointIndex, List<List<Point3d>> listOfListStretchLinePoint, double stretchValue)
        {
            KickerHelper kickerHlp = new KickerHelper();
            for (int index = 0; index < listOfListStretchLinePoint.Count; index++)
            {
                var listOfStretchLinePoint = listOfListStretchLinePoint[index];
                Point3d stretchPoint1 = listOfStretchLinePoint[0];
                Point3d stretchPoint2 = listOfStretchLinePoint[1];

                var angleOfStrechLine = AEE_Utility.GetAngleOfLine(stretchPoint1, stretchPoint2);

                int strectVerticeIndex = listOfVerticePointIndex[index];


                var stretchPoint = AEE_Utility.get_XY(angleOfStrechLine, stretchValue);
                Vector3d stretchVector = new Vector3d(stretchPoint.X, stretchPoint.Y, 0);
                kickerHlp.stretchCorner(wallPanelRectId, stretchVector, strectVerticeIndex);
            }
        }
        private List<ObjectId> getExternalCornerLineId(ObjectId externalCornerId, Point3d basePointOfExternalCrner, ObjectId wallPanelLineId, double wallPanelLineAngle, ObjectId wallPanelRectId, out List<List<Point3d>> listOfOfListStretchPoint, out List<int> listOfStretchPointIndex, out double stretchValue)
        {
            stretchValue = 0;
            listOfStretchPointIndex = new List<int>();
            listOfOfListStretchPoint = new List<List<Point3d>>();

            List<ObjectId> listOfStretchLineId = new List<ObjectId>();

            var listOfCrnerExplodeId = AEE_Utility.ExplodeEntity(externalCornerId);

            List<ObjectId> listOfAllCrnrLineId = new List<ObjectId>();
            List<double> listOfAllCrnrLineLngth = new List<double>();

            for (int index = 0; index < listOfCrnerExplodeId.Count; index++)
            {
                ObjectId crnerLineId = listOfCrnerExplodeId[index];

                var flg = checkAngleOfBaseLine(wallPanelLineId, crnerLineId);
                if (flg == true)
                {
                    listOfAllCrnrLineId.Add(crnerLineId);
                    var crnerLineLngth = AEE_Utility.GetLengthOfLine(crnerLineId);
                    listOfAllCrnrLineLngth.Add(crnerLineLngth);
                }
            }

            double maxCrnrLineLngth = listOfAllCrnrLineLngth.Max();
            int indexOfMaxLngth = listOfAllCrnrLineLngth.IndexOf(maxCrnrLineLngth);
            var externalCrnrLineId = listOfAllCrnrLineId[indexOfMaxLngth];

            externalCrnrLineId = AEE_Utility.createColonEntityInSamePoint(externalCrnrLineId, false);

            var offsetLineId = AEE_Utility.OffsetLine(externalCrnrLineId, 1, false);
            var midPoint = AEE_Utility.GetMidPoint(offsetLineId);
            var flagOfInside = AEE_Utility.GetPointIsInsideThePolyline(externalCornerId, midPoint);

            if (flagOfInside == true)
            {
                AEE_Utility.deleteEntity(offsetLineId);
                offsetLineId = AEE_Utility.OffsetLine(externalCrnrLineId, CommonModule.panelDepth, false);
            }
            else
            {
                AEE_Utility.deleteEntity(offsetLineId);
                offsetLineId = AEE_Utility.OffsetLine(externalCrnrLineId, -CommonModule.panelDepth, false);
            }
            listOfStretchLineId.Add(externalCrnrLineId);
            listOfStretchLineId.Add(offsetLineId);

            AEE_Utility.deleteEntity(listOfCrnerExplodeId);

            var listOfStrtEndExtrnCrnrPoint = AEE_Utility.GetStartEndPointOfLine(externalCrnrLineId);
            var listOfOffstStrtEndExtrnCrnrPoint = AEE_Utility.GetStartEndPointOfLine(offsetLineId);

            List<double> listOfLngth = new List<double>();
            foreach (var point in listOfStrtEndExtrnCrnrPoint)
            {
                var length = AEE_Utility.GetLengthOfLine(point, basePointOfExternalCrner);
                listOfLngth.Add(length);
            }

            List<Point3d> listStretchPoint1 = new List<Point3d>();
            double maxLngth = listOfLngth.Max();
            double minLngth = listOfLngth.Min();
            stretchValue = maxLngth;

            int minLengthIndex = listOfLngth.IndexOf(minLngth);
            int maxLengthIndex = listOfLngth.IndexOf(maxLngth);
            listStretchPoint1.Add(listOfStrtEndExtrnCrnrPoint[minLengthIndex]);
            listStretchPoint1.Add(listOfStrtEndExtrnCrnrPoint[maxLengthIndex]);

            listOfOfListStretchPoint.Add(listStretchPoint1);

            List<Point3d> listStretchPoint2 = new List<Point3d>();
            listStretchPoint2.Add(listOfOffstStrtEndExtrnCrnrPoint[minLengthIndex]);
            listStretchPoint2.Add(listOfOffstStrtEndExtrnCrnrPoint[maxLengthIndex]);

            listOfOfListStretchPoint.Add(listStretchPoint2);

            List<double> listOfExternalCrnrLineLngth = new List<double>();
            List<double> listOfOffsetExternalCrnrLineLngth = new List<double>();

            var listOfVerticePoint = AEE_Utility.GetPolylineVertexPoint(wallPanelRectId);
            for (int index = 0; index < listOfVerticePoint.Count; index++)
            {
                var verticePoint = listOfVerticePoint[index];
                Point3d vrtcPoint = new Point3d(verticePoint.X, verticePoint.Y, 0);

                List<double> listOfExtrnCrnrLineLngth = new List<double>();
                foreach (var point in listOfStrtEndExtrnCrnrPoint)
                {
                    var length = AEE_Utility.GetLengthOfLine(point, vrtcPoint);
                    listOfExtrnCrnrLineLngth.Add(length);
                }
                listOfExternalCrnrLineLngth.Add(listOfExtrnCrnrLineLngth.Min());

                List<double> listOfOffsetExtrnCrnrLineLngth = new List<double>();

                foreach (var point in listOfOffstStrtEndExtrnCrnrPoint)
                {
                    var length = AEE_Utility.GetLengthOfLine(point, vrtcPoint);
                    listOfOffsetExtrnCrnrLineLngth.Add(length);
                }
                listOfOffsetExternalCrnrLineLngth.Add(listOfOffsetExtrnCrnrLineLngth.Min());
            }

            double minLengthOfExtrnlCrnr = listOfExternalCrnrLineLngth.Min();
            int indexOfMinLengthOfExtrnlCrnr = listOfExternalCrnrLineLngth.IndexOf(minLengthOfExtrnlCrnr);
            listOfStretchPointIndex.Add(indexOfMinLengthOfExtrnlCrnr);

            double minLengthOfOffstExtrnlCrnr = listOfOffsetExternalCrnrLineLngth.Min();
            int indexOfMinLengthOfOffstExtrnlCrnr = listOfOffsetExternalCrnrLineLngth.IndexOf(minLengthOfOffstExtrnlCrnr);
            listOfStretchPointIndex.Add(indexOfMinLengthOfOffstExtrnlCrnr);

            return listOfStretchLineId;
        }

        private void changeWallPanelText_In_ConcaveAngle(ObjectId wallPanelRectId, ObjectId wallPanelLineId, ObjectId wallPanelTextId, ObjectId topPanelTextId, double stretchValue, bool flagOfLeft, bool flagOfRight)
        {
            double wallPanelLineLngth = AEE_Utility.GetLengthOfLine(wallPanelLineId);
            wallPanelLineLngth = Math.Round(wallPanelLineLngth);
            //double newWallPanelLineLngth = wallPanelLineLngth + stretchValue;
            double newWallPanelLineLngth = wallPanelLineLngth;

            string oldWallTextName = "";
            string newWallTextName = "";
            changeWallPanelText(wallPanelTextId, wallPanelLineLngth, newWallPanelLineLngth, flagOfLeft, flagOfRight, out oldWallTextName, out newWallTextName);

            string oldTopWallTextName = "";
            string newTopWallTextName = "";
            changeWallPanelText(topPanelTextId, wallPanelLineLngth, newWallPanelLineLngth, flagOfLeft, flagOfRight, out oldTopWallTextName, out newTopWallTextName);

            if (wallPanelTextId.IsValid == true)
            {
                var listOfAllIndexOfWallPanelTextId = Enumerable.Range(0, listOfAllWallPanelRectTextId.Count).Where(i => listOfAllWallPanelRectTextId[i] == wallPanelTextId).ToList();

                changeBOMDataOfWallPanel_Stretch(listOfAllIndexOfWallPanelTextId, wallPanelLineLngth, newWallPanelLineLngth, oldWallTextName, newWallTextName);
            }
            if (topPanelTextId.IsValid == true)
            {
                var listOfAllIndexOfWallPanelTextId = Enumerable.Range(0, listOfAllWallPanelRectTextId.Count).Where(i => listOfAllWallPanelRectTextId[i] == topPanelTextId).ToList();

                changeBOMDataOfWallPanel_Stretch(listOfAllIndexOfWallPanelTextId, wallPanelLineLngth, newWallPanelLineLngth, oldTopWallTextName, newTopWallTextName);
            }
        }

        private void changeWallPanelText(ObjectId cornerTextId, double wallLength, double newWallLngth, bool flagOfLeft, bool flagOfRight, out string oldWallTextName, out string newWallTextName)
        {
            newWallTextName = "";
            oldWallTextName = "";
            if (cornerTextId.IsErased == true || cornerTextId.IsValid == false)
            {
                return;
            }
            try
            {
                string oldLngth = Convert.ToString(wallLength);
                string newLngth = Convert.ToString(newWallLngth);

                Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;

                using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
                {
                    Entity ent = tr.GetObject(cornerTextId, OpenMode.ForWrite) as Entity;
                    if (ent is MText)
                    {
                        MText mtext = ent as MText;
                        string wallText = mtext.Contents;
                        string newText = getNewWallPanelTextAfterStretch(wallText, wallLength, newWallLngth, flagOfLeft, flagOfRight, out oldWallTextName, out newWallTextName);
                        mtext.Contents = newText;
                    }
                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
        }

        private string getNewWallPanelTextAfterStretch(string wallText, double wallLength, double newWallLngth, bool flagOfLeft, bool flagOfRight, out string oldWallTextName, out string newWallTextName)
        {
            newWallTextName = "";
            oldWallTextName = "";
            string oldLngth = Convert.ToString(wallLength);
            string newLngth = Convert.ToString(newWallLngth);

            //if (wallText=="200 WP 1950")
            //{
            //    MessageBox.Show("Test");
            //}
            //string newText = wallText.Replace(oldLngth, newLngth);
            string[] array = wallText.Split(' ');
            string wallTextName = Convert.ToString(array.GetValue(1));
            oldWallTextName = wallTextName;

            if (flagOfLeft == true)
            {
                newWallTextName = wallTextName + "X(L)";
            }
            else if (flagOfRight == true)
            {
                newWallTextName = wallTextName + "X(R)";
            }

            string newText = wallText.Replace(wallTextName, newWallTextName);
            if (newText.Contains("(L)X(R)"))
            {
                newText = newText.Replace("(L)X(R)", "");
            }
            if (newText.Contains("(R)X(L)"))
            {
                newText = newText.Replace("(R)X(L)", "");
            }

            //if (newText.Contains("WPRX(L)"))
            //{
            //    MessageBox.Show("Test");
            //}

            return newText;

        }

        private void changeBOMDataOfWallPanel_Stretch(List<int> listOfAllIndex, double wallLength, double newWallLngth, string oldWallTextName, string newWallTextName)
        {
            //string oldLngth = Convert.ToString(wallLength);
            //string newLngth = Convert.ToString(newWallLngth);

            for (int i = 0; i < listOfAllIndex.Count; i++)
            {
                int index = listOfAllIndex[i];
                string data = listOfAllWallPanelData[index];

                string newText = data;
                if (!string.IsNullOrEmpty(oldWallTextName))
                {
                    newText = data.Replace(oldWallTextName, newWallTextName);
                }
                listOfAllWallPanelData[index] = newText;
            }
        }

        public void setBOMDataOfWallPanel(ObjectId wallPanelTextId, ObjectId panelRectId, ObjectId panelLineId, double wallWidth, double wallHeight, double wallLength, string itemCode, string description, string wallType, string elev = "0", bool flag_X_Axis = false, bool flag_Y_Axis = false)
        {
            listOfAllWallPanelRectId.Add(panelRectId);
            listOfAllWallPanelRectTextId.Add(wallPanelTextId);
            listOfAllWallPanelLineId.Add(panelLineId);

#if !WNPANEL
            if (itemCode.StartsWith(CommonModule.doorThickWallPanelText) ||
                          itemCode.StartsWith(CommonModule.windowThickWallPanelText))
            {
                description = description + " (S)";
            }
            else if (itemCode.StartsWith(PanelLayout_UI.doorWindowBottomPanelName) ||
                     itemCode.StartsWith(PanelLayout_UI.windowThickBottomWallPanelText))
            {
                description = description + " (S)";
            }
            else if (itemCode.StartsWith(CommonModule.doorWindowThickTopPropText) ||
                     itemCode.StartsWith(CommonModule.windowThickBottomPropText))
            {
                description = description + " (S)";
            }
#endif
            string data = Convert.ToString(wallWidth) + BOMHelper.symbol + Convert.ToString(wallHeight) + BOMHelper.symbol + Convert.ToString(wallLength) + BOMHelper.symbol + itemCode + BOMHelper.symbol + description + BOMHelper.symbol + wallType + BOMHelper.symbol + elev + BOMHelper.symbol + flag_X_Axis + BOMHelper.symbol + flag_Y_Axis;
            listOfAllWallPanelData.Add(data);
        }
    }
}