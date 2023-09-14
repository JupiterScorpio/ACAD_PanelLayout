﻿using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using PanelLayout_App.CivilModel;
using PanelLayout_App.WallModule;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PanelLayout_App
{

    public class CornerElevation
    {
        public string wall1;
        public string wall2;
        public double dist1;
        public double dist2;
        public double elev;
        public CornerElevation(string pwall1,
                               string pwall2,
                               double pdist1,
                               double pdist2,
                               double pelev)
        {
            wall1 = pwall1;
            wall2 = pwall2;
            dist1 = pdist1;
            dist2 = pdist2;
            elev = pelev;
        }
    }

    public class CornerHelper
    {
        public static List<ObjectId> listOfAllCornerId_ForBOM = new List<ObjectId>();
        public static List<ObjectId> listOfAllCornerTextId_ForBOM = new List<ObjectId>();
        public static List<CornerElevation> listOfAllCornerElevInfo = new List<CornerElevation>();

        public static List<string> listOfAllCornerText_ForBOM = new List<string>();

        public static List<List<ObjectId>> listOfExternalCornerTextId_ForStretch = new List<List<ObjectId>>();
        public static List<ObjectId> listOfExternalCornerId_ForStretch = new List<ObjectId>();
        public static List<ObjectId> listOfOffstExternalCornerId_ForStretch = new List<ObjectId>();
        public static List<ObjectId> listOfWallLineId_OfExternalCrner_ForStretch = new List<ObjectId>();
        public static List<List<ObjectId>> listOfListOfExternalCrnerId_InsctToWallLine_ForStretch = new List<List<ObjectId>>();

        public static List<List<ObjectId>> listOfInternalCornerTextId_ForStretch = new List<List<ObjectId>>();
        public static List<ObjectId> listOfInternalCornerId_ForStretch = new List<ObjectId>();
        public static List<ObjectId> listOfOffstInternalCornerId_ForStretch = new List<ObjectId>();
        public static List<ObjectId> listOfWallLineId_OfInternalCrner_ForStretch = new List<ObjectId>();
        public static List<List<ObjectId>> listOfListOfInternalCrnerId_InsctToWallLine_ForStretch = new List<List<ObjectId>>();

        public static Dictionary<ObjectId, List<object>> listOfInternalCornerId_InsctToWindow = new Dictionary<ObjectId, List<object>>(); //Save info about Splited IAX by SDM 2022-07-19
        public static List<ObjectId> listOfCornerId_InsctToBeam = new List<ObjectId>();//Added on 18/03/2023 by SDM
        public static List<List<ObjectId>> listOfCornerTextId_InsctToBeam = new List<List<ObjectId>>();//Added on 18/03/2023 by SDM
        public void callMethodOfInsertCornerHelper()
        {
            listOfOffstExternalCornerId_ForStretch.Clear();
            listOfExternalCornerId_ForStretch.Clear();
            listOfExternalCornerTextId_ForStretch.Clear();
            listOfInternalCornerId_InsctToWindow.Clear();
            InternalWallHelper.lstCornerData.Clear();
            CommonModule.kIcker_corner_CLine = new List<ElevationModule.kIcker_corner_CLine>();
            CommonModule.slapjoint_corner_CLine = new List<ElevationModule.slapjoint_corner_CLine>();
            listOfCornerId_InsctToBeam.Clear();//Added on 18/03/2023 by SDM
            listOfCornerTextId_InsctToBeam.Clear();//Added on 18/03/2023 by SDM

            if (CheckShellPlanHelper.listOfAllSelectedObjectIds.Count != 0)
            {
                RibbonHelper.insertCornerButton.IsEnabled = false;
                string cornerCreationMsg = "Corners are creating....";
                ProgressForm progressForm = new ProgressForm();
                progressForm.Show();
                progressForm.ReportProgress(1, cornerCreationMsg);
                InternalWallHelper internalWall_Obj = new InternalWallHelper();
                internalWall_Obj.callMethodInternalWall(progressForm, cornerCreationMsg, DoorHelper.listDoorLines);
                ExternalWallHelper externalWall_Obj = new ExternalWallHelper();
                externalWall_Obj.callMethodExternalWall(progressForm, cornerCreationMsg);
                internalWall_Obj.getListOfWallId_InsctToDoorCorner();
                DoorHelper doorHelper = new DoorHelper();
                doorHelper.drawDoorThicknessWallPanel();
                BeamHelper beamHlp = new BeamHelper();
                beamHlp.drawBeamBottomCorner();
                AEE_Utility.MoveEntity(DoorHelper.listOfMoveCornerIds_In_New_ShellPlan, CreateShellPlanHelper.moveVector_ForWindowDoorLayout);
                internalWall_Obj.sortAllLinesBtwnTwoCorners();
                cornerCreationMsg = "Corner is created....";
                progressForm.ReportProgress(100, cornerCreationMsg);
                progressForm.Close();

                if (InternalWallHelper.listOfNonPerpendiculareLine.Count != 0)
                {
                    string cornerCountInStr = Convert.ToString(InternalWallHelper.listOfNonPerpendiculareLine.Count);
                    string msg = cornerCountInStr + " Corners are not at right angle are marked with a circle.";
                    MessageBox.Show(msg, CommonModule.headerText_messageBox, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                string dwgMsg = "Corners are created.";

                if (!CommandModule.dply)
                { MessageBox.Show(dwgMsg, CommonModule.headerText_messageBox, MessageBoxButtons.OK, MessageBoxIcon.Information); }

                //Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.SendStringToExecute((String)RibbonHelper.insertPanelButton.CommandParameter, true, false, true);
                RibbonHelper.insertPanelButton.IsEnabled = true;
            }
        }


        public ObjectId writeTextInCorner(ObjectId cornerId, string cornerText, double cornerAngle, string layerName, int layerColor, double textScale = 1)
        {

            var listOfCornerVertexPoint = AEE_Utility.GetPolylineVertexPoint(cornerId);
            double minX = listOfCornerVertexPoint.Min(point => point.X);
            double maxX = listOfCornerVertexPoint.Max(point => point.X);
            double minY = listOfCornerVertexPoint.Min(point => point.Y);
            double maxY = listOfCornerVertexPoint.Max(point => point.Y);
            double center_X = minX + ((maxX - minX) / 2);
            double center_Y = minY + ((maxY - minY) / 2);
            double angle = cornerAngle;


            var textId = AEE_Utility.CreateMultiLineText(cornerText, center_X, center_Y, 0, CommonModule.wallPanelTextHght, layerName, layerColor, angle, textScale);

            return textId;
        }
        public List<ObjectId> writeMultipleTextInCorner(ObjectId cornerId, List<string> listOfcornerTexts, double cornerAngle, string layerName, int layerColor)
        {

            List<ObjectId> textIds = new List<ObjectId>();

            var listOfCornerVertexPoint = AEE_Utility.GetPolylineVertexPoint(cornerId);
            double minX = listOfCornerVertexPoint.Min(point => point.X);
            double maxX = listOfCornerVertexPoint.Max(point => point.X);
            double minY = listOfCornerVertexPoint.Min(point => point.Y);
            double maxY = listOfCornerVertexPoint.Max(point => point.Y);
            double center_X = minX + ((maxX - minX) / 2);
            double center_Y = minY + ((maxY - minY) / 2);
            double angle = cornerAngle;

            int i = 1;
            foreach (string textStr in listOfcornerTexts)
            {
                ObjectId textId;

                if (i == 1)
                {
                    textId = AEE_Utility.CreateMultiLineText(textStr, center_X, center_Y, 0, CommonModule.wallPanelTextHght, layerName, layerColor, angle);
                }
                else
                {
                    Point3d basePt = AEE_Utility.GetBasePointOfPolyline(cornerId);

                    double x_Diff = center_X - basePt.X;
                    double y_Diff = center_Y - basePt.Y;

                    double length = Math.Sqrt(x_Diff * x_Diff + y_Diff * y_Diff);

                    double offsetDist = length + 50;
                    double factor = -offsetDist / length;

                    if (textStr.StartsWith(CommonModule.externalCornerText))
                    {
                        factor *= -1;
                    }

                    Point3d newPt = new Point3d(center_X + x_Diff * factor, center_Y + y_Diff * factor, 0);

                    textId = AEE_Utility.CreateMultiLineText(textStr, newPt.X, newPt.Y, 0, CommonModule.wallPanelTextHght, layerName, layerColor, angle);
                }
                textIds.Add(textId);
                i++;
            }
            return textIds;
        }

        public static string getCornerText(string cornerText, double flange1, double flange2, double cornerWallHeight)
        {
#if WNPANEL
            string outputText = cornerText + " " + Convert.ToString(flange2) + " + " + Convert.ToString(flange1) + " \n" + Convert.ToString(cornerWallHeight);
#else
            string outputText = cornerText + " " + Convert.ToString(flange1) + " X " + Convert.ToString(flange2) + " \n" + Convert.ToString(cornerWallHeight);
#endif
            return outputText;
        }


        public void checkCornerInsctToBeam(ObjectId beamId_InsctWall, ObjectId wallPanelLineId, ObjectId cornerId1, ObjectId cornerId2, List<ObjectId> cornerId1_TextId, List<ObjectId> cornerId2_TextId, ObjectId sunkanSlabId)
        {
            if (beamId_InsctWall.IsValid == true)
            {
                Entity wallEnt = AEE_Utility.GetEntityForRead(wallPanelLineId);
                string wallLayerName = wallEnt.Layer;
                Entity beamEnt = AEE_Utility.GetEntityForRead(beamId_InsctWall);
                string beamLayerName = beamEnt.Layer;
                WindowHelper windowHlp = new WindowHelper();
                DoorHelper doorHlp = new DoorHelper();
                ExternalWallHelper externalWallHlp = new ExternalWallHelper();
                if (!DoorHelper.listOfDoorCornerTextId_ForBeamTextChange.Contains(cornerId1_TextId))
                {
                    var listOfCorner1Vertex = AEE_Utility.GetPolylineVertexPoint(cornerId1);

                    for (int i = 0; i < listOfCorner1Vertex.Count; i++)
                    {
                        Point2d point = listOfCorner1Vertex[i];
                        Point3d point3d = new Point3d(point.X, point.Y, 0);
                        var flag = AEE_Utility.GetPointIsInsideThePolyline(beamId_InsctWall, point3d);

                        if (flag == true)
                        {
                            List<List<object>> listOfCornerData_ForBOM = new List<List<object>>();
                            List<string> cornerTexts = getCornerTextWithWall(wallLayerName, beamLayerName, sunkanSlabId, out listOfCornerData_ForBOM);

                            var _oelev = DeleteCornerData_ForBOM(cornerId1); //Added to Fix IAX when there is a Beam by SDM 2022-08-13
                            if (beamLayerName.Contains(Beam_UI_Helper.offsetBeamStrtText))
                                for (int j = 0; j < listOfCornerData_ForBOM.Count; j++)
                                {
                                    AEE_Utility.changeMText(cornerId1_TextId[j], cornerTexts[j]);
                                    var xDataRegAppName = AEE_Utility.GetXDataRegisterAppName(cornerId1);
                                    string cornerDescp = Convert.ToString(listOfCornerData_ForBOM[j][0]);
                                    string cornerType = Convert.ToString(listOfCornerData_ForBOM[j][1]);
                                    double flange1 = Convert.ToDouble(listOfCornerData_ForBOM[j][2]);
                                    double flange2 = Convert.ToDouble(listOfCornerData_ForBOM[j][3]);
                                    double cornerWallHeight = Convert.ToDouble(listOfCornerData_ForBOM[j][4]);
                                    Line ln = AEE_Utility.GetLine(wallPanelLineId);
                                    double len = Math.Max(ln.StartPoint.DistanceTo(ln.GetClosestPointTo(point3d, false)),
                                                          ln.EndPoint.DistanceTo(ln.GetClosestPointTo(point3d, false)));
                                    //Fix IAX elevation by SDM 2022-08-13
                                    //CornerElevation oelev = new CornerElevation(xDataRegAppName, AEE_Utility.GetXDataRegisterAppName(cornerId2), len, 0, 0);
                                    if (_oelev == null)
                                        _oelev = new CornerElevation(xDataRegAppName, AEE_Utility.GetXDataRegisterAppName(cornerId2), 0, 0, 0);
                                    var wall1 = InternalWallHelper.GetInternalWallLineByxDataRegAppName(xDataRegAppName);
                                    CornerElevation oelev = new CornerElevation(_oelev.wall1, _oelev.wall2, wall1.Length, 0, 0);

                                    setCornerDataForBOM(xDataRegAppName, cornerId1, cornerId1_TextId[j], cornerTexts[j], cornerDescp, cornerType, flange1, flange2, cornerWallHeight, oelev);
                                }
                            else
                            {
                                listOfCornerId_InsctToBeam.Add(cornerId1);
                                listOfCornerTextId_InsctToBeam.Add(cornerId1_TextId);
                            }
                            break;
                        }
                    }
                }

                if (!DoorHelper.listOfDoorCornerTextId_ForBeamTextChange.Contains(cornerId2_TextId))
                {
                    var listOfCorner2Vertex = AEE_Utility.GetPolylineVertexPoint(cornerId2);

                    for (int i = 0; i < listOfCorner2Vertex.Count; i++)
                    {
                        Point2d point = listOfCorner2Vertex[i];
                        Point3d point3d = new Point3d(point.X, point.Y, 0);
                        var flag = AEE_Utility.GetPointIsInsideThePolyline(beamId_InsctWall, point3d);

                        if (flag == true)
                        {
                            var xDataRegAppName = AEE_Utility.GetXDataRegisterAppName(cornerId2);
                            List<List<object>> listOfCornerData_ForBOM = new List<List<object>>();
                            List<string> cornerTexts = getCornerTextWithWall(wallLayerName, beamLayerName, sunkanSlabId, out listOfCornerData_ForBOM);

                            var _oelev = DeleteCornerData_ForBOM(cornerId2); //Added to Fix IAX when there is a Beam by SDM 2022-08-13
                            if (beamLayerName.Contains(Beam_UI_Helper.offsetBeamStrtText))
                                for (int j = 0; j < listOfCornerData_ForBOM.Count; j++)
                                {
                                    AEE_Utility.changeMText(cornerId2_TextId[j], cornerTexts[j]);
                                    string cornerDescp = Convert.ToString(listOfCornerData_ForBOM[j][0]);
                                    string cornerType = Convert.ToString(listOfCornerData_ForBOM[j][1]);
                                    double flange1 = Convert.ToDouble(listOfCornerData_ForBOM[j][2]);
                                    double flange2 = Convert.ToDouble(listOfCornerData_ForBOM[j][3]);
                                    double cornerWallHeight = Convert.ToDouble(listOfCornerData_ForBOM[j][4]);
                                    Line ln = AEE_Utility.GetLine(wallPanelLineId);
                                    double len = Math.Max(ln.StartPoint.DistanceTo(ln.GetClosestPointTo(point3d, false)),
                                                          ln.EndPoint.DistanceTo(ln.GetClosestPointTo(point3d, false)));
                                    //Fix IAX elevation by SDM 2022-08-13
                                    //CornerElevation oelev = new CornerElevation(xDataRegAppName, AEE_Utility.GetXDataRegisterAppName(cornerId1), len, 0, 0);
                                    if (_oelev == null)
                                        _oelev = new CornerElevation(xDataRegAppName, AEE_Utility.GetXDataRegisterAppName(cornerId1), 0, 0, 0);
                                    var wall1 = InternalWallHelper.GetInternalWallLineByxDataRegAppName(xDataRegAppName);
                                    CornerElevation oelev = new CornerElevation(_oelev.wall1, _oelev.wall2, wall1.Length, 0, 0);

                                    setCornerDataForBOM(xDataRegAppName, cornerId2, cornerId2_TextId[j], cornerTexts[j], cornerDescp, cornerType, flange1, flange2, cornerWallHeight, oelev);
                                }
                            else
                            {
                                listOfCornerId_InsctToBeam.Add(cornerId2);
                                listOfCornerTextId_InsctToBeam.Add(cornerId2_TextId);
                            }
                            break;
                        }
                    }
                }
            }
        }

        //Added by SDM 2022-08-13
        public static CornerElevation DeleteCornerData_ForBOM(ObjectId cornerId)
        {
            CornerElevation elev = null;
            while (CornerHelper.listOfAllCornerId_ForBOM.IndexOf(cornerId) != -1)
            {
                int del_idx = CornerHelper.listOfAllCornerId_ForBOM.IndexOf(cornerId);
                elev = CornerHelper.listOfAllCornerElevInfo[del_idx] != null ? CornerHelper.listOfAllCornerElevInfo[del_idx] : elev;
                CornerHelper.listOfAllCornerId_ForBOM.RemoveAt(del_idx);
                CornerHelper.listOfAllCornerElevInfo.RemoveAt(del_idx);
                CornerHelper.listOfAllCornerText_ForBOM.RemoveAt(del_idx);
                CornerHelper.listOfAllCornerTextId_ForBOM.RemoveAt(del_idx);
            }
            return elev;
        }

        private List<string> getCornerTextWithWall(string wallLayerName, string beamLayerName, ObjectId sunkanSlabId, out List<List<object>> listOfCornerData_ForBOM)
        {
            listOfCornerData_ForBOM = new List<List<object>>();
            List<string> listOfCornerTexts = new List<string>();

            double beamBottom = Beam_UI_Helper.getOffsetBeamBottom(beamLayerName);

            if (InternalWallSlab_UI_Helper.IsInternalWall(wallLayerName) || StairCase_UI_Helper.checkStairCaseLayerIsExist(wallLayerName))
            {
                double cornerWallHeight = GeometryHelper.getHeightOfWall_EC_IC_InternallWall(wallLayerName, InternalWallSlab_UI_Helper.getSlabBottom(wallLayerName), beamBottom, PanelLayout_UI.SL, CommonModule.internalCorner, sunkanSlabId);
                // commented on 03/03/2020
                //double levelDifferenceOfSunkanSlb = SunkanSlabHelper.getSunkanSlabLevelDifference_LessThan_RC__Corners(sunkanSlabId);
                //cornerWallHeight = cornerWallHeight + levelDifferenceOfSunkanSlb;
                listOfCornerTexts = UpdateCornerText(listOfCornerData_ForBOM, cornerWallHeight, sunkanSlabId);
            }

            else if (wallLayerName == CommonModule.externalWallLayerName || wallLayerName == CommonModule.ductLayerName || Lift_UI_Helper.checkLiftLayerIsExist(wallLayerName))
            {
                double cornerWallHeight = GeometryHelper.getHeightOfWall_EC_IC_ExternallWall(wallLayerName, InternalWallSlab_UI_Helper.getSlabBottom(wallLayerName), beamBottom, PanelLayout_UI.getSlabThickness(wallLayerName), PanelLayout_UI.RC, PanelLayout_UI.kickerHt, CommonModule.internalCorner, sunkanSlabId);
                listOfCornerTexts = CornerHelper.UpdateCornerTextExternal(listOfCornerData_ForBOM, cornerWallHeight);
            }

            return listOfCornerTexts;
        }

        public static string getInternalCornerCode()
        {
            return CommonModule.intrnlCornr_Flange1 == 100 && CommonModule.intrnlCornr_Flange2 == 100 ? CommonModule.internalCornerText : CommonModule.internalCornerText2;
        }

        public static List<string> UpdateCornerText(List<List<object>> listOfCornerData_ForBOM,
            double cornerWallHeight, ObjectId sunkanSlabId)
        {
            // List<object> tempListOfCornerData_ForBOM = new List<object>();
            List<string> listOfCornerTexts = new List<string>();

            var code = getInternalCornerCode();
            List<double> cornerHeights = new List<double>();

            double RC_Value = 0;

            if (sunkanSlabId.IsValid == false)
            {
                RC_Value = PanelLayout_UI.RC;
            }

                if (cornerWallHeight > PanelLayout_UI.maxHeightOfCorner)
            {
                if (PanelLayout_UI.flagOfPanelWithRC == true)
                {
                    cornerHeights.Add(PanelLayout_UI.bottomSplitHeight + RC_Value);
                    cornerHeights.Add(cornerWallHeight - (PanelLayout_UI.bottomSplitHeight + RC_Value));
                }
                else
                {
                    cornerHeights.Add(PanelLayout_UI.bottomSplitHeight);
                    cornerHeights.Add(cornerWallHeight - PanelLayout_UI.bottomSplitHeight);
                }

            }
            else
            {
                cornerHeights.Add(cornerWallHeight);
            }

            for (int i = 0; i < cornerHeights.Count; i++)
            {
                string tempCornerText = CornerHelper.getCornerText(code, CommonModule.intrnlCornr_Flange1, CommonModule.intrnlCornr_Flange2, cornerHeights[i]);
                listOfCornerTexts.Add(tempCornerText);

                List<object> tempListOfCornerData_ForBOM = new List<object>();
                tempListOfCornerData_ForBOM.Add(CommonModule.internalCornerDescp);
                tempListOfCornerData_ForBOM.Add(code);
                tempListOfCornerData_ForBOM.Add(CommonModule.intrnlCornr_Flange1);
                tempListOfCornerData_ForBOM.Add(CommonModule.intrnlCornr_Flange2);
                tempListOfCornerData_ForBOM.Add(cornerHeights[i]);

                listOfCornerData_ForBOM.Add(tempListOfCornerData_ForBOM);

            }

            return listOfCornerTexts;
        }


        public static List<string> UpdateCornerTextExternal(List<List<object>> listOfCornerData_ForBOM,
            double cornerWallHeight)
        {
            List<string> listOfCornerTexts = new List<string>();
            List<double> cornerHeights = new List<double>();

            if (cornerWallHeight > PanelLayout_UI.maxHeightOfCorner)
            {
                cornerHeights.Add(PanelLayout_UI.bottomSplitHeight);
                cornerHeights.Add(cornerWallHeight - PanelLayout_UI.bottomSplitHeight);
            }
            else
            {
                cornerHeights.Add(cornerWallHeight);
            }

            for (int i = 0; i < cornerHeights.Count; i++)
            {
                List<object> tempListOfCornerData_ForBOM = new List<object>();
                string tempCornerText = CornerHelper.getCornerText(CommonModule.externalCornerText, CommonModule.extrnlCornr_Flange, CommonModule.extrnlCornr_Flange, cornerHeights[i]);
                listOfCornerTexts.Add(tempCornerText);

                tempListOfCornerData_ForBOM.Add(CommonModule.externalCornerDescp);
                tempListOfCornerData_ForBOM.Add(CommonModule.externalCornerText);
                tempListOfCornerData_ForBOM.Add(CommonModule.extrnlCornr_Flange);
                tempListOfCornerData_ForBOM.Add(CommonModule.extrnlCornr_Flange);
                tempListOfCornerData_ForBOM.Add(cornerHeights[i]);

                listOfCornerData_ForBOM.Add(tempListOfCornerData_ForBOM);
            }

            return listOfCornerTexts;
        }
        public void setExternalCornerInConcave(ObjectId externalCornerId, List<ObjectId> externalCornerTextId)
        {
            listOfExternalCornerId_ForStretch.Add(externalCornerId);
            listOfExternalCornerTextId_ForStretch.Add(externalCornerTextId);
            double offsetValue = 10;
            var listOfCornerExplode = AEE_Utility.ExplodeEntity(externalCornerId);
            var explodeCornerLineId = listOfCornerExplode[0];
            var offsetLineId = AEE_Utility.OffsetLine(explodeCornerLineId, 1, false);
            var midPoint = AEE_Utility.GetMidPoint(offsetLineId);
            var flagOfInside = AEE_Utility.GetPointIsInsideThePolyline(externalCornerId, midPoint);
            ObjectId externalCrnrId_ForStretch = new ObjectId();

            if (flagOfInside == true)
            {
                AEE_Utility.deleteEntity(offsetLineId);
                offsetLineId = AEE_Utility.OffsetLine(explodeCornerLineId, -1, false);
                midPoint = AEE_Utility.GetMidPoint(offsetLineId);
                externalCrnrId_ForStretch = AEE_Utility.OffsetEntity_WithoutLine(offsetValue, midPoint, externalCornerId, false);
            }

            else
            {
                externalCrnrId_ForStretch = AEE_Utility.OffsetEntity_WithoutLine(offsetValue, midPoint, externalCornerId, false);
            }

            listOfOffstExternalCornerId_ForStretch.Add(externalCrnrId_ForStretch);
            AEE_Utility.deleteEntity(offsetLineId);
            AEE_Utility.deleteEntity(listOfCornerExplode);
        }


        public void setCornerIdRemainingWallLineId_ExternalCrnr(ObjectId wallLineId, ObjectId remainingWallLineId)
        {
            int indexOfExistLine = listOfWallLineId_OfExternalCrner_ForStretch.IndexOf(wallLineId);

            if (indexOfExistLine != -1)
            {
                listOfWallLineId_OfExternalCrner_ForStretch.Add(remainingWallLineId);
                listOfListOfExternalCrnerId_InsctToWallLine_ForStretch.Add(listOfListOfExternalCrnerId_InsctToWallLine_ForStretch[indexOfExistLine]);
            }
        }


        public void checkWallLineIntersctToExternalCornerId(ObjectId cornerId1, ObjectId cornerId2, ObjectId wallLineId, List<ObjectId> listOfWallLineId)
        {
            int indexOfExistCornrId1 = listOfExternalCornerId_ForStretch.IndexOf(cornerId1);
            int indexOfExistCornrId2 = listOfExternalCornerId_ForStretch.IndexOf(cornerId2);

            if (indexOfExistCornrId1 == -1 && indexOfExistCornrId2 == -1)
            {
                return;
            }

            List<ObjectId> listOfOffsetExternalCornerId = new List<ObjectId>();

            if (indexOfExistCornrId1 != -1)
            {
                listOfOffsetExternalCornerId.Add(listOfOffstExternalCornerId_ForStretch[indexOfExistCornrId1]);
            }

            if (indexOfExistCornrId2 != -1)
            {
                listOfOffsetExternalCornerId.Add(listOfOffstExternalCornerId_ForStretch[indexOfExistCornrId2]);
            }

            for (int i = 0; i < listOfWallLineId.Count; i++)
            {
                ObjectId lineId = listOfWallLineId[i];

                foreach (var offstExtrnlCrnrId in listOfOffsetExternalCornerId)
                {
                    List<ObjectId> listOfOffsetExternalCrnrId = new List<ObjectId>();
                    var listOfInsctPoint = AEE_Utility.InterSectionBetweenTwoEntity(offstExtrnlCrnrId, lineId);

                    if (listOfInsctPoint.Count != 0)
                    {
                        int indexOfLineExist = listOfWallLineId_OfExternalCrner_ForStretch.IndexOf(lineId);

                        if (indexOfLineExist == -1)
                        {
                            listOfWallLineId_OfExternalCrner_ForStretch.Add(lineId);
                            listOfOffsetExternalCrnrId.Add(offstExtrnlCrnrId);
                            listOfListOfExternalCrnerId_InsctToWallLine_ForStretch.Add(listOfOffsetExternalCrnrId);
                        }

                        else
                        {
                            listOfOffsetExternalCrnrId = listOfListOfExternalCrnerId_InsctToWallLine_ForStretch[indexOfLineExist];
                            listOfOffsetExternalCrnrId.Add(offstExtrnlCrnrId);
                            listOfListOfExternalCrnerId_InsctToWallLine_ForStretch[indexOfLineExist] = listOfOffsetExternalCrnrId;
                        }
                    }
                }
            }
        }


        public void setInternalCornerForStretch(ObjectId internalCornerId, List<ObjectId> internalCornerTextId)
        {
            listOfInternalCornerId_ForStretch.Add(internalCornerId);
            listOfInternalCornerTextId_ForStretch.Add(internalCornerTextId);
            ObjectId offstInternalCrnrId_ForStretch = getOffsetInternalCornerInOutside(internalCornerId);
            //AEE_Utility.changeVisibility(internalCrnrId_ForStretch, true);
            listOfOffstInternalCornerId_ForStretch.Add(offstInternalCrnrId_ForStretch);
        }


        public void setCornerIdRemainingWallLineId_InternalCrnr(ObjectId wallLineId, ObjectId remainingWallLineId)
        {
            int indexOfExistLine = listOfWallLineId_OfInternalCrner_ForStretch.IndexOf(wallLineId);

            if (indexOfExistLine != -1)
            {
                listOfWallLineId_OfInternalCrner_ForStretch.Add(remainingWallLineId);
                listOfListOfInternalCrnerId_InsctToWallLine_ForStretch.Add(listOfListOfInternalCrnerId_InsctToWallLine_ForStretch[indexOfExistLine]);
            }
        }


        public void checkWallLineIntersctToInternalCornerId(ObjectId cornerId1, ObjectId cornerId2, ObjectId wallLineId, List<ObjectId> listOfWallLineId)
        {
            int indexOfExistCornrId1 = listOfInternalCornerId_ForStretch.IndexOf(cornerId1);
            int indexOfExistCornrId2 = listOfInternalCornerId_ForStretch.IndexOf(cornerId2);

            if (indexOfExistCornrId1 == -1 && indexOfExistCornrId2 == -1)
            {
                return;
            }

            List<ObjectId> listOfOffsetInternalCornerId = new List<ObjectId>();

            if (indexOfExistCornrId1 != -1)
            {
                listOfOffsetInternalCornerId.Add(listOfOffstInternalCornerId_ForStretch[indexOfExistCornrId1]);
            }

            if (indexOfExistCornrId2 != -1)
            {
                listOfOffsetInternalCornerId.Add(listOfOffstInternalCornerId_ForStretch[indexOfExistCornrId2]);
            }

            for (int i = 0; i < listOfWallLineId.Count; i++)
            {
                ObjectId lineId = listOfWallLineId[i];

                //AEE_Utility.createColonEntityInSamePoint(lineId, true);

                foreach (var offstIntrnlCrnrId in listOfOffsetInternalCornerId)
                {
                    List<ObjectId> listOfOffsetInternalCrnrId = new List<ObjectId>();
                    var listOfInsctPoint = AEE_Utility.InterSectionBetweenTwoEntity(offstIntrnlCrnrId, lineId);

                    if (listOfInsctPoint.Count != 0)
                    {
                        int indexOfLineExist = listOfWallLineId_OfInternalCrner_ForStretch.IndexOf(lineId);

                        if (indexOfLineExist == -1)
                        {
                            listOfWallLineId_OfInternalCrner_ForStretch.Add(lineId);
                            listOfOffsetInternalCrnrId.Add(offstIntrnlCrnrId);
                            listOfListOfInternalCrnerId_InsctToWallLine_ForStretch.Add(listOfOffsetInternalCrnrId);
                        }

                        else
                        {
                            listOfOffsetInternalCrnrId = listOfListOfInternalCrnerId_InsctToWallLine_ForStretch[indexOfLineExist];
                            listOfOffsetInternalCrnrId.Add(offstIntrnlCrnrId);
                            listOfListOfInternalCrnerId_InsctToWallLine_ForStretch[indexOfLineExist] = listOfOffsetInternalCrnrId;
                        }
                    }
                }
            }
        }


        public void getInternalCornerId_ForStretch(ObjectId wallLineId, List<ObjectId> listOfPanelRectId, List<ObjectId> listOfPanelLineId, List<ObjectId> listOfWallPanelTextId, List<ObjectId> listOfTopPanelTextId, List<ObjectId> listOfCircleId, double lineLenDiff = 0)
        {
            int indexOfExist = CornerHelper.listOfWallLineId_OfInternalCrner_ForStretch.IndexOf(wallLineId);

            if (indexOfExist != -1)
            {
                var listOfInternalCrnerId_InsctToWallLine = CornerHelper.listOfListOfInternalCrnerId_InsctToWallLine_ForStretch[indexOfExist];

                foreach (var offsetIntrnalCrnrId in listOfInternalCrnerId_InsctToWallLine)
                {
                    for (int index = 0; index < listOfPanelRectId.Count; index++)
                    {
                        ObjectId wallPanelLineId = listOfPanelLineId[index];
                        var wallRectId = listOfPanelRectId[index];

                        bool flagOfDistIsGreater = getDistBtwCrnrBasePointToWallPanel(offsetIntrnalCrnrId, wallPanelLineId, wallRectId);

                        if (flagOfDistIsGreater == false)
                        {
                            var listOfInsct = AEE_Utility.InterSectionBetweenTwoEntity(offsetIntrnalCrnrId, wallRectId);

                            if (listOfInsct.Count != 0)
                            {
                                double wallPanelLineLength = AEE_Utility.GetLengthOfLine(wallPanelLineId);
                                if (wallPanelLineLength + lineLenDiff > 0)
                                {
                                    wallPanelLineLength += lineLenDiff;
                                    wallPanelLineLength = Math.Round(wallPanelLineLength);
                                }
                                bool flagOfStretchCorner = stretchInternalCorner_BeHalfOfWallLength(offsetIntrnalCrnrId, wallRectId, wallPanelLineLength);

                                ////AEE_Utility.changeColor(wallRectId, 2);
                                if (flagOfStretchCorner == true)
                                {
                                    deletetWallPanelsData(index, wallRectId, wallPanelLineId, listOfWallPanelTextId, listOfTopPanelTextId, listOfCircleId);
                                }

                                break;
                            }
                        }
                    }
                }
            }
        }


        private void deletetWallPanelsData(int index, ObjectId wallRectId, ObjectId wallPanelLineId, List<ObjectId> listOfWallPanelTextId, List<ObjectId> listOfTopPanelTextId, List<ObjectId> listOfCircleId)
        {
            if (index < listOfWallPanelTextId.Count)
            {
                AEE_Utility.deleteEntity(listOfWallPanelTextId[index]);
            }

            if (index < listOfTopPanelTextId.Count)
            {
                AEE_Utility.deleteEntity(listOfTopPanelTextId[index]);
            }

            foreach (var circleId in listOfCircleId)
            {
                var centerPoinOfCirc = AEE_Utility.getCenterPointOfCircle(circleId);
                var flafOfInside = AEE_Utility.GetPointIsInsideThePolyline(wallRectId, centerPoinOfCirc);

                if (flafOfInside == true)
                {
                    AEE_Utility.deleteEntity(circleId);
                    break;
                }
            }

            AEE_Utility.deleteEntity(wallRectId);
            AEE_Utility.deleteEntity(wallPanelLineId);
        }

        private bool stretchInternalCorner_BeHalfOfWallLength(ObjectId offsetIntrnalCrnrId, ObjectId wallRectId, double wallPanelLineLength)
        {
            bool flagOfStretchCorner = false;
            int indexOfOffstCrnrId = listOfOffstInternalCornerId_ForStretch.IndexOf(offsetIntrnalCrnrId);

            if (indexOfOffstCrnrId != -1)
            {
                ObjectId internalCornerId = listOfInternalCornerId_ForStretch[indexOfOffstCrnrId];
                List<ObjectId> internalCornerTextId = listOfInternalCornerTextId_ForStretch[indexOfOffstCrnrId];
                ////AEE_Utility.changeColor(internalCornerId, 2);
                Point3d basePointOfIntrnlCrnr = AEE_Utility.GetBasePointOfPolyline(internalCornerId);
                ////KickerHelper kickerHlp = new KickerHelper();
                ////Point3d nrstPointFromBasePointOfIntrnlCrnr = kickerHlp.getNearestPointOfKickerCorner(internalCornerId, basePointOfIntrnlCrnr);
                ObjectId offstWallRectId = getOffsetWallPanelRectInOutside(wallRectId);
                List<int> listOfStretchPointIndex = new List<int>();

                List<Point3d> listOfStretchPoint = getStretchPointOfCornerWithIndex(offstWallRectId, internalCornerId, basePointOfIntrnlCrnr, out listOfStretchPointIndex);

                if (listOfStretchPoint.Count != 0)
                {
                    double angleOfCrnrToWallLine = getAngleBtwCornerToWallPanel(wallRectId, basePointOfIntrnlCrnr);
                    var point = AEE_Utility.get_XY(angleOfCrnrToWallLine, wallPanelLineLength);
                    Point3d stretchPoint = new Point3d(point.X, point.Y, 0);
                    Point3d endPointOfIntrnlCrnr = new Point3d((basePointOfIntrnlCrnr.X + stretchPoint.X), (basePointOfIntrnlCrnr.Y + stretchPoint.Y), 0);
                    flagOfStretchCorner = stretchCorner(internalCornerId, stretchPoint, listOfStretchPointIndex, listOfStretchPoint);

                    if (flagOfStretchCorner == true)
                    {
                        List<double> listOfFlange = getInternalConerFlange(internalCornerId, internalCornerTextId, CommonModule.internalCornerLyrName);
                        checkBeamCornerExistInWallCorner(internalCornerId);
                    }

                    ////AEE_Utility.getLineId(basePointOfIntrnlCrnr, endPointOfIntrnlCrnr, true);
                }

                AEE_Utility.deleteEntity(offstWallRectId);
            }

            return flagOfStretchCorner;
        }
        public bool stretchInternalCorner_BeHalfOfWindowDoor(ObjectId intrnalCrnrId, List<ObjectId> internalCorner_textId, ObjectId tempRectId, double stretchLen)
        {
            bool flagOfStretchCorner = false;

            Point3d basePointOfIntrnlCrnr = AEE_Utility.GetBasePointOfPolyline(intrnalCrnrId);

            ObjectId offstTempRectId = getOffsetWallPanelRectInOutside(tempRectId);
            List<int> listOfStretchPointIndex = new List<int>();

            List<Point3d> listOfStretchPoint = getStretchPointOfCornerWithIndex(offstTempRectId, intrnalCrnrId, basePointOfIntrnlCrnr, out listOfStretchPointIndex);

            if (listOfStretchPoint.Count != 0)
            {
                double angleOfCrnrToWallLine = getAngleBtwCornerToWallPanel(tempRectId, basePointOfIntrnlCrnr);
                var point = AEE_Utility.get_XY(angleOfCrnrToWallLine, stretchLen);
                Point3d stretchPoint = new Point3d(point.X, point.Y, 0);
                Point3d endPointOfIntrnlCrnr = new Point3d((basePointOfIntrnlCrnr.X + stretchPoint.X), (basePointOfIntrnlCrnr.Y + stretchPoint.Y), 0);
                flagOfStretchCorner = stretchCorner(intrnalCrnrId, stretchPoint, listOfStretchPointIndex, listOfStretchPoint);

                if (flagOfStretchCorner == true)
                {
                    List<double> listOfFlange = getInternalConerFlange(intrnalCrnrId, internalCorner_textId, CommonModule.internalCornerLyrName);

                    if (listOfFlange[0] < CommonModule.internalCorner_MinLngth || listOfFlange[1] < CommonModule.internalCorner_MinLngth)
                    {
                        Point3d cornerBasePt = AEE_Utility.GetBasePointOfPolyline(intrnalCrnrId);

                        var circleId = AEE_Utility.CreateCircle(cornerBasePt.X, cornerBasePt.Y, 0, (CommonModule.intrnlCornr_Flange1 / 2), CommonModule.internalCornerLyrName, 1);
                        AEE_Utility.changeColor(circleId, InternalWallHelper.nonStandardColorIndex);
                    }
                    //checkBeamCornerExistInWallCorner(intrnalCrnrId);
                }
                AEE_Utility.deleteEntity(offstTempRectId);
            }
            return flagOfStretchCorner;
        }

        public ObjectId getOffsetWallPanelRectInOutside(ObjectId wallRectId)
        {
            double offsetValue = CommonModule.internalCornerThick + 2;
            Point3d offsetPoint = new Point3d(1234578, 6598751, 0);
            ObjectId offstWallRectId = AEE_Utility.OffsetEntity_WithoutLine(offsetValue, offsetPoint, wallRectId, false);
            return offstWallRectId;
        }


        public ObjectId getOffsetInternalCornerInOutside(ObjectId internalCornerId)
        {
            double offsetValue = 5;
            Point3d offsetPoint = new Point3d(1231451, 5245861, 0);
            ObjectId offstInternalCrnrId_ForStretch = AEE_Utility.OffsetEntity_WithoutLine(offsetValue, offsetPoint, internalCornerId, false);
            return offstInternalCrnrId_ForStretch;
        }


        public bool stretchCorner(ObjectId id, Point3d stretchPoint, List<int> listOfStretchPointIndex, List<Point3d> listOfStretchPoint)
        {
            bool flagOfStretchCorner = false;
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

                for (int index = 0; index < listOfStretchPointIndex.Count; index++)
                {
                    int stretchPointIndex = listOfStretchPointIndex[index];
                    Vector3d stretchVec = new Vector3d(stretchPoint.X, stretchPoint.Y, 0);
                    IntegerCollection indces = new IntegerCollection();
                    indces.Add(stretchPointIndex);
                    acPoly.MoveStretchPointsAt(indces, stretchVec);
                    flagOfStretchCorner = true;
                }

                acPoly.UpgradeOpen();
                acTrans.Commit();
            }

            return flagOfStretchCorner;
        }


        private void checkBeamCornerExistInWallCorner(ObjectId wallInternalCornerId)
        {
            return;
            int indexOfCrnerExistInBeam = BeamHelper.listOfAllWallCornerId_WithBeam.IndexOf(wallInternalCornerId);

            if (indexOfCrnerExistInBeam != -1)
            {
                ObjectId beamCornrId = BeamHelper.listOfAllBeamCornerId_ForStretch[indexOfCrnerExistInBeam];
                BeamHelper.listOfAllStretchBeamCornerId.Add(beamCornrId);
                var listOfWallCrnrVertice = AEE_Utility.GetPolylineVertexPoint(wallInternalCornerId);
                var wallCrnrBasePoint = AEE_Utility.GetBasePointOfPolyline(wallInternalCornerId);
                var beamCrnrBasePoint = AEE_Utility.GetBasePointOfPolyline(beamCornrId);
                Vector3d stretchVector = new Vector3d((beamCrnrBasePoint.X - wallCrnrBasePoint.X), (0), 0);
                var colonId = AEE_Utility.copyColonEntity(stretchVector, wallInternalCornerId, false);
                AEE_Utility.GetPolylineVertexPoint(colonId);
                listOfWallCrnrVertice = AEE_Utility.GetPolylineVertexPoint(colonId);
                AEE_Utility.deleteEntity(colonId);
                reDrawCorner(beamCornrId, listOfWallCrnrVertice);
            }
        }


        public void reDrawCorner(ObjectId beamCornrId, List<Point2d> listOfWallCrnrVertice)
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
                Entity acEnt = acTrans.GetObject(beamCornrId, OpenMode.ForWrite) as Entity;
                Polyline acPoly = acEnt as Polyline;
                var noVertx = acPoly.NumberOfVertices;

                for (int index = (noVertx - 1); index > 0; index--)
                {
                    acPoly.RemoveVertexAt(index);
                }

                for (int index = 1; index < listOfWallCrnrVertice.Count; index++)
                {
                    Point2d point = listOfWallCrnrVertice[index];
                    acPoly.AddVertexAt(index, new Point2d(point.X, point.Y), 0.0, 0.0, 0.0);
                }

                acPoly.UpgradeOpen();
                acTrans.Commit();
            }
        }


        private bool getDistBtwCrnrBasePointToWallPanel(ObjectId cornerId, ObjectId wallPanelId, ObjectId wallRectId)
        {
            bool flagOfDistIsGreater = true;
            var basePoint = AEE_Utility.GetBasePointOfPolyline(cornerId);
            var listOfStrtEndPoint = AEE_Utility.GetStartEndPointOfLine(wallPanelId);

            if (listOfStrtEndPoint.Count == 0)
            {
                return flagOfDistIsGreater;
            }

            List<double> listOfDistance = new List<double>();

            for (int k = 0; k < listOfStrtEndPoint.Count; k++)
            {
                Point3d point = listOfStrtEndPoint[k];
                var lengthBtwCrnrToWallPanel = AEE_Utility.GetLengthOfLine(basePoint, point);
                listOfDistance.Add(lengthBtwCrnrToWallPanel);
            }

            double maxLength = listOfDistance.Max();

            if (maxLength <= CommonModule.internalCorner_MaxLngth)
            {
                flagOfDistIsGreater = false;
            }
            var wallPanelLineLength = AEE_Utility.GetLengthOfLine(wallPanelId);
            if (wallPanelLineLength <= CommonModule.maxCornerExtension)
            {
                double angleOfCrnrToWallLine = getAngleBtwCornerToWallPanel(wallRectId, basePoint);
                var _point = AEE_Utility.get_XY(angleOfCrnrToWallLine, wallPanelLineLength);
                listOfDistance = new List<double> { Math.Abs(Math.Round(_point.X)), Math.Abs(Math.Round(_point.Y)) };
                if (listOfDistance.Max() <= CommonModule.internalCorner_MaxLngth)
                {
                    flagOfDistIsGreater = false;
                }
            }
            return flagOfDistIsGreater;
        }


        private List<Point3d> getStretchPointOfCornerWithIndex(ObjectId offstWallRectId, ObjectId internalCornerId, Point3d basePointOfIntrnlCrnr, out List<int> listOfStretchPointIndex)
        {
            List<Point3d> listOfStretchPoint = new List<Point3d>();
            listOfStretchPointIndex = new List<int>();
            List<Point2d> listOfCornerVertices = AEE_Utility.GetPolylineVertexPoint(internalCornerId);

            for (int index = 0; index < listOfCornerVertices.Count; index++)
            {
                Point2d vertPoint = listOfCornerVertices[index];
                Point3d point = new Point3d(vertPoint.X, vertPoint.Y, 0);
                var flagOfInside = AEE_Utility.GetPointIsInsideThePolyline(offstWallRectId, point);

                if (flagOfInside == true)
                {
                    if (basePointOfIntrnlCrnr == point)
                    {
                        listOfStretchPoint.Clear();
                        listOfStretchPointIndex.Clear();
                        break;
                    }

                    listOfStretchPoint.Add(point);
                    listOfStretchPointIndex.Add(index);
                }
            }

            return listOfStretchPoint;
        }


        private double getAngleBtwCornerToWallPanel(ObjectId wallRectId, Point3d basePointOfCrnr)
        {
            List<double> listOfLength = new List<double>();
            List<Point3d> listOfVerticesPoint = new List<Point3d>();
            var listOfRectVerticesPoint = AEE_Utility.GetPolylineVertexPoint(wallRectId);

            for (int index = 0; index < listOfRectVerticesPoint.Count; index++)
            {
                Point2d vertPoint = listOfRectVerticesPoint[index];
                Point3d point = new Point3d(vertPoint.X, vertPoint.Y, 0);
                var length = AEE_Utility.GetLengthOfLine(point, basePointOfCrnr);
                listOfLength.Add(length);
                listOfVerticesPoint.Add(point);
            }

            double minLength = listOfLength.Min();
            int indexOfMinLength = listOfLength.IndexOf(minLength);
            var minLngthVerticePoint = listOfVerticesPoint[indexOfMinLength];
            double angleOfCrnrToWallLine = AEE_Utility.GetAngleOfLine(basePointOfCrnr, minLngthVerticePoint);
            return angleOfCrnrToWallLine;
        }


        public static void setCornerDataForBOM(string xDataRegAppName, ObjectId cornerId, ObjectId cornerTextId, string cornerDescp, string type, string itemCode, double flange1, double flange2, double height, CornerElevation elev = null)
        {
            string outputText = itemCode + BOMHelper.symbol + cornerDescp + BOMHelper.symbol + type + BOMHelper.symbol + Convert.ToString(flange1) + BOMHelper.symbol + Convert.ToString(flange2) + BOMHelper.symbol + Convert.ToString(height) + BOMHelper.symbol + cornerTextId.ToString();
            //outputText += BOMHelper.symbol + Convert.ToString(elev);
            int indexOfExistCornerId = CornerHelper.listOfAllCornerId_ForBOM.IndexOf(cornerId);

            if (indexOfExistCornerId == -1)
            {
                if (!AEE_Utility.CheckXDataRegisterAppName(cornerId, xDataRegAppName))
                {
                    AEE_Utility.AttachXData(cornerId, xDataRegAppName, CommonModule.xDataAsciiName);
                }

                listOfAllCornerId_ForBOM.Add(cornerId);
                listOfAllCornerTextId_ForBOM.Add(cornerTextId);
                listOfAllCornerText_ForBOM.Add(outputText);
                listOfAllCornerElevInfo.Add(elev);
            }
            else
            {
                //Added to fix IAX when the wall height more than 3000 by SDM 2022-07-18
                if (itemCode == CommonModule.internalCornerText || itemCode == CommonModule.externalCornerText)
                {
                    listOfAllCornerId_ForBOM.Add(cornerId);
                    listOfAllCornerTextId_ForBOM.Add(cornerTextId);
                    listOfAllCornerText_ForBOM.Add(outputText);
                    listOfAllCornerElevInfo.Add(elev);
                }
                else
                {
                    listOfAllCornerText_ForBOM[indexOfExistCornerId] = outputText;
                    listOfAllCornerTextId_ForBOM[indexOfExistCornerId] = cornerTextId;

                    if (listOfAllCornerElevInfo[indexOfExistCornerId] == null)
                        listOfAllCornerElevInfo[indexOfExistCornerId] = elev;

                }
            }
        }


        public List<double> getCornerFlangeLength(ObjectId cornerId)
        {
            List<ObjectId> listOfCornerExplodeId = AEE_Utility.ExplodeEntity(cornerId);
            List<double> listOfFlange = new List<double>();
            ObjectId flange1_Id = listOfCornerExplodeId[0];
            ObjectId flange2_Id = listOfCornerExplodeId[listOfCornerExplodeId.Count - 1];
            double flange1 = AEE_Utility.GetLengthOfLine(flange1_Id);
            double flange2 = AEE_Utility.GetLengthOfLine(flange2_Id);

            for (int k = (listOfCornerExplodeId.Count - 1); k > 0; k--)
            {
                flange2_Id = listOfCornerExplodeId[k];
                flange2 = AEE_Utility.GetLengthOfLine(flange2_Id);

                if (flange2 != 0)
                {
                    break;
                }
            }

            flange1 = Math.Round(flange1);
            flange2 = Math.Round(flange2);

            if (flange1 == flange2)
            {
                return listOfFlange;
            }

            ObjectId minLngthCornerLineId = new ObjectId();
            ObjectId maxLngthCornerLineId = new ObjectId();
            double maxFlange = 0;
            double minFlange = 0;

            if (flange1 > flange2)
            {
                maxLngthCornerLineId = flange1_Id;
                minLngthCornerLineId = flange2_Id;
                maxFlange = flange1;
                minFlange = flange2;
            }
            else
            {
                maxLngthCornerLineId = flange2_Id;
                minLngthCornerLineId = flange1_Id;
                maxFlange = flange2;
                minFlange = flange1;
            }

            CommonModule commonMod = new CommonModule();
            var listOfMaxStrtEndPoint = commonMod.getStartEndPointOfLine(maxLngthCornerLineId);
            maxLngthCornerLineId = AEE_Utility.getLineId(listOfMaxStrtEndPoint[0], listOfMaxStrtEndPoint[1], false);
            var listOfMinStrtEndPoint = commonMod.getStartEndPointOfLine(minLngthCornerLineId);
            minLngthCornerLineId = AEE_Utility.getLineId(listOfMinStrtEndPoint[0], listOfMinStrtEndPoint[1], false);
            bool flagOfLeft = false;
            bool flagOfRight = false;
            getCornerPanelDirection(minLngthCornerLineId, maxLngthCornerLineId, out flagOfLeft, out flagOfRight);
            var maxLngthMidPoint = AEE_Utility.GetMidPoint(maxLngthCornerLineId);
            var minLngthMidPoint = AEE_Utility.GetMidPoint(minLngthCornerLineId);
            double rightFlange = 0;
            double leftFlange = 0;

            if (flagOfLeft == true)
            {
                leftFlange = maxFlange;
                rightFlange = minFlange;
                //AEE_Utility.CreateTextWithAngle("Left", maxLngthMidPoint.X, maxLngthMidPoint.Y, 0, 30, "A", 3, 0);
                //AEE_Utility.CreateTextWithAngle("Right", minLngthMidPoint.X, minLngthMidPoint.Y, 0, 30, "A", 3, 0);
            }
            else
            {
                leftFlange = minFlange;
                rightFlange = maxFlange;
                //AEE_Utility.CreateTextWithAngle("Right", maxLngthMidPoint.X, maxLngthMidPoint.Y, 0, 30, "A", 3, 0);
                //AEE_Utility.CreateTextWithAngle("Left", minLngthMidPoint.X, minLngthMidPoint.Y, 0, 30, "A", 3, 0);
            }

            AEE_Utility.deleteEntity(minLngthCornerLineId);
            AEE_Utility.deleteEntity(maxLngthCornerLineId);
            listOfFlange.Add(rightFlange);
            listOfFlange.Add(leftFlange);
            AEE_Utility.deleteEntity(listOfCornerExplodeId);
            return listOfFlange;
        }


        private void getCornerPanelDirection(ObjectId minLngthCornerLineId, ObjectId maxLngthCornerLineId, out bool flagOfLeft, out bool flagOfRight)
        {
            flagOfRight = false;
            flagOfLeft = false;
            var listOfMaxLngthLineStrtEndPoint = AEE_Utility.GetStartEndPointOfLine(maxLngthCornerLineId);
            var listOfStretchWallLineStrtEndPoint = AEE_Utility.GetStartEndPointOfLine(minLngthCornerLineId);
            var stretchWallLineAngle = AEE_Utility.GetAngleOfLine(minLngthCornerLineId);
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
                        flagOfLeft = true;
                    }

                    else
                    {
                        flagOfRight = true;
                    }
                }

                else
                {
                    double diff = minLngthPointOfStretchWall.Y - maxLngthWallPoint2.Y;

                    if (diff <= 0)
                    {
                        flagOfLeft = true;
                    }

                    else
                    {
                        flagOfRight = true;
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
                        flagOfLeft = true;
                    }

                    else
                    {
                        flagOfRight = true;
                    }
                }

                else
                {
                    double diff = maxLngthWallPoint2.X - minLngthPointOfStretchWall.X;

                    if (diff <= 0)
                    {
                        flagOfLeft = true;
                    }

                    else
                    {
                        flagOfRight = true;
                    }
                }
            }
        }


        public List<double> getInternalConerFlange(ObjectId cornerId, List<ObjectId> cornerTextIds, string cornerLayerName)
        {
            List<double> listOfFlange = new List<double>();
            CommonModule commonMod = new CommonModule();
            CornerHelper cornerHlp = new CornerHelper();
            var cornerEnt = AEE_Utility.GetEntityForRead(cornerId);

            if (cornerEnt.Layer == cornerLayerName)
            {
                listOfFlange = cornerHlp.getCornerFlangeLength(cornerId);

                if (listOfFlange.Count != 0)
                {
                    double rightFlange = listOfFlange[0];
                    double leftFlange = listOfFlange[1];
                    changeWallCornerText(cornerTextIds, rightFlange, leftFlange);
                }
            }

            return listOfFlange;
        }


        private void changeWallCornerText(List<ObjectId> cornerTextIds, double flange1, double flange2)
        {
            ////IAX 100 X 100 2875
            foreach (ObjectId crnrTextId in cornerTextIds)
            {
                var crrntCornerText = AEE_Utility.GetTextOfMtext(crnrTextId);
                string[] array = crrntCornerText.Split(' ');
                string newCornerText = "";

                var lstTxt = new List<string>();

                for (int i = 0; i < array.Length; i++)
                {
                    string text = Convert.ToString(array.GetValue(i));

                    if (i == 1)
                    {
#if WNPANEL
                        text = Convert.ToString(flange2);
#else
                        text = Convert.ToString(flange1);
#endif
                    }

                    else
                        if (i == 3)
                    {
#if WNPANEL
                        text = Convert.ToString(flange1);
#else
                        text = Convert.ToString(flange2);
#endif
                    }
                    lstTxt.Add(text);
                }
                if (array.Length > 3 && array[0] == "IAX")
                {
                    if (lstTxt[1] != "100" || lstTxt[3] != "100")
                    {
                        lstTxt[0] = "IA";
                    }
                }
                else if (array.Length > 3 && array[0] == "IA")
                {
                    if (lstTxt[1] == "100" && lstTxt[3] == "100")
                    {
                        lstTxt[0] = "IAX";
                    }
                }

                newCornerText = string.Join(" ", lstTxt);
                //Added by RTJ 18-06-21
                ChangeTheBOMCollection(crnrTextId.ToString(), newCornerText, lstTxt[1], lstTxt[3]);
                AEE_Utility.changeMText(crnrTextId, newCornerText);
            }
        }
        //Added the function by RTJ 18-06-21
        private void ChangeTheBOMCollection(string crnrTextId, string newCornerText, string newl1, string newl2)
        {
            //
            foreach (var iBomInfo in listOfAllCornerText_ForBOM)
            {
                string[] strSpltData = iBomInfo.Split('@');
                if (strSpltData[6] == crnrTextId)
                {
                    string appendText = strSpltData[0] + "@" + newCornerText + "@" + strSpltData[2] + "@" + newl2 + "@" + newl1 + "@" + strSpltData[5] + "@" + strSpltData[6];

                    listOfAllCornerText_ForBOM[listOfAllCornerText_ForBOM.FindIndex(ind => ind.Equals(iBomInfo))] = appendText;
                    break;
                }
            }
        }
        //Added the function by RTJ 18-06-21


        public void changeBeamBottomCornerText(List<ObjectId> cornerTextIds, double rightFlange, double leftFlange)
        {
            ////140 x 100 CC 400 x 250
            ////140 x 100 CC(BH)(SP1) 400 x 250

            foreach (ObjectId crnrTextId in cornerTextIds)
            {
                var crrntCornerText = AEE_Utility.GetTextOfDBtext(crnrTextId);
                string[] array = crrntCornerText.Split(' ');
                string newCornerText = "";
                int lastCount = (array.Length - 1);

                for (int i = lastCount; i >= 0; i--)
                {
                    string text = Convert.ToString(array.GetValue(i));

                    if (i == lastCount)
                    {
                        text = Convert.ToString(leftFlange);
                    }

                    else
                        if (i == (lastCount - 2))
                    {
                        text = Convert.ToString(rightFlange);
                    }

                    if (i == lastCount)
                    {
                        newCornerText = text + newCornerText;
                    }

                    else
                    {
                        newCornerText = text + " " + newCornerText;
                    }
                }

                AEE_Utility.changeDBText(crnrTextId, newCornerText);
            }
        }

        internal void stretchCorner(OverlapExternalDoorCorners item, Point3d newPt)
        {
            double t = 1e-6;
            Tolerance tol = new Tolerance(t, t);
            List<Point3d> lstPts = AEE_Utility.GetPolyLinePointList(item.WallCornerId);
            List<LineSegment2d> lst = AEE_Utility.GetSegmentOfLineFromPolyline(item.WallCornerId);
            if (lst.Count < 2)
            {
                return;
            }
            var minLen = lst.Min(o => o.Length);
            if (lst.First().IsOn(item.WallCornerPointOnDoor, tol) && lst.Count > 5)
            {
                var dir = new Vector3d(lst[1].Direction.X, lst[1].Direction.Y, 0);
                var thirdPt = newPt + (minLen * dir.GetNormal());
                lstPts[1] = newPt;
                lstPts[2] = thirdPt;
                lstPts.RemoveAt(4);
                lstPts.RemoveAt(3);
                AEE_Utility.setPolyLinePts(lstPts, item.WallCornerId);
            }
            if (lst.Last().IsOn(item.WallCornerPointOnDoor, tol) && lst.Count > 5)
            {
                var ldir = lst[lst.Count - 2].Direction;
                var dir = new Vector3d(ldir.X, ldir.Y, 0);
                var thirdPt = newPt - (minLen * dir.GetNormal());
                lstPts[lst.Count - 1] = newPt;
                lstPts[lst.Count - 2] = thirdPt;
                lstPts.RemoveAt(lst.Count - 3);
                lstPts.RemoveAt(lst.Count - 4);
                AEE_Utility.setPolyLinePts(lstPts, item.WallCornerId);
            }

            List<double> listOfFlange = getInternalConerFlange(item.WallCornerId, item.WallCornerTextId, CommonModule.internalCornerLyrName);
            checkBeamCornerExistInWallCorner(item.WallCornerId);
        }
    }



    public class IndexHelper
    {
        public static List<string> listOfAllRoomText_ForXData = new List<string>();
        public static List<string> listOfAllRoomWallText_ForXData = new List<string>();

        List<Point3d> listOfWallPointForSorting = new List<Point3d>();
        List<Point3d> listOfCenterWallPointForSorting = new List<Point3d>();

        List<string> listOfWallPoint_Str = new List<string>();
        List<ObjectId> listOfWallObjId_ForIndex = new List<ObjectId>();

        List<string> listOfExistWallPoint_Check = new List<string>();

        public List<ObjectId> setValueOfRoom(ProgressForm progressForm, string progressBarMsg, List<ObjectId> listOfWallObjId,
            string roomType, bool flagOfExternalWall, out List<List<ObjectId>> listOfListAllRoomWallLineId, bool needToAddSlabThickness = false)
        {
            listOfListAllRoomWallLineId = new List<List<ObjectId>>();

            if (listOfWallObjId.Count == 0)
            {
                return listOfWallObjId;
            }

            listOfExistWallPoint_Check.Clear();
            listOfWallPoint_Str.Clear();
            listOfCenterWallPointForSorting.Clear();
            listOfWallPointForSorting.Clear();
            listOfWallObjId_ForIndex.Clear();

            for (int index = 0; index < listOfWallObjId.Count; index++)
            {
                if ((index % 10) == 0)
                {
                    progressForm.ReportProgress(1, progressBarMsg);
                }

                ObjectId wallId = listOfWallObjId[index];
                var listOfPolylineVertex = AEE_Utility.GetPolylineVertexPoint(wallId);
                double minX = Math.Round(listOfPolylineVertex.Min(sortPoint => sortPoint.X));
                double minY = Math.Round(listOfPolylineVertex.Min(sortPoint => sortPoint.Y));
                double maxX = Math.Round(listOfPolylineVertex.Max(sortPoint => sortPoint.X));
                double maxY = Math.Round(listOfPolylineVertex.Max(sortPoint => sortPoint.Y));
                minX = Math.Round(minX);
                minY = Math.Round(minY);
                maxX = Math.Round(maxX);
                maxY = Math.Round(maxY);
                double centerX = minX + ((maxX - minX) / 2);
                double centerY = minY + ((maxY - minY) / 2);
                Point3d minX_maxY = new Point3d(minX, maxY, 0);
                Point3d cntrX_cntrY = new Point3d(centerX, centerY, 0);
                string pointInStr = Convert.ToString(minX_maxY) + "," + Convert.ToString(cntrX_cntrY);
                listOfWallPoint_Str.Add(pointInStr);
                listOfWallObjId_ForIndex.Add(wallId);
                listOfWallPointForSorting.Add(minX_maxY);
                listOfCenterWallPointForSorting.Add(cntrX_cntrY);
            }

            listOfWallPointForSorting = listOfWallPointForSorting.OrderBy(sortPoint => sortPoint.X).ToList();
            listOfWallPointForSorting = listOfWallPointForSorting.OrderByDescending(sortPoint => sortPoint.Y).ToList();
            listOfCenterWallPointForSorting = listOfCenterWallPointForSorting.OrderBy(sortPoint => sortPoint.X).ToList();
            listOfCenterWallPointForSorting = listOfCenterWallPointForSorting.OrderByDescending(sortPoint => sortPoint.Y).ToList();
            List<ObjectId> listOfNewWallId_With_RoomIdentity = new List<ObjectId>();
            createIndexingInRoom(roomType, flagOfExternalWall, out listOfNewWallId_With_RoomIdentity, out listOfListAllRoomWallLineId, needToAddSlabThickness);
            AEE_Utility.deleteEntity(listOfWallObjId);
            return listOfNewWallId_With_RoomIdentity;
        }


        private List<ObjectId> createIndexingInRoom(string roomType, bool flagOfExternalWall,
            out List<ObjectId> listOfNewWallId_With_RoomIdentity, out List<List<ObjectId>> listOfListAllRoomWallLineId,
            bool needToAddSlabThickness)
        {
            listOfListAllRoomWallLineId = new List<List<ObjectId>>();
            listOfNewWallId_With_RoomIdentity = new List<ObjectId>();
            int roomCount = 1;

            for (int i = 0; i < listOfWallPointForSorting.Count; i++)
            {
                Point3d minX_maxY = listOfWallPointForSorting[i];

                for (int j = 0; j < listOfCenterWallPointForSorting.Count; j++)
                {
                    Point3d cntrX_cntrY = listOfCenterWallPointForSorting[j];
                    string pointInStr = Convert.ToString(minX_maxY) + "," + Convert.ToString(cntrX_cntrY);
                    int index = listOfWallPoint_Str.IndexOf(pointInStr);

                    if (index != -1 && !listOfExistWallPoint_Check.Contains(pointInStr))
                    {
                        listOfExistWallPoint_Check.Add(pointInStr);
                        ObjectId wallId = listOfWallObjId_ForIndex[index];
                        var wallEnt = AEE_Utility.GetEntityForRead(wallId);
                        Point3d centerPoint = WallPanelHelper.getCenterPointOfPanelRectangle(wallId);
                        string roomText = roomType + Convert.ToString(roomCount);
                        ObjectId blockId = new ObjectId();

                        String slabText = "";
                        if (needToAddSlabThickness)
                        {
                            slabText = "ST-" + PanelLayout_UI.getSlabThickness(wallEnt.Layer);
                        }

                        if (flagOfExternalWall == false)
                        {
                            blockId = createBlock(roomText, centerPoint.X, centerPoint.Y, slabText);
                        }

                        List<Point2d> listOfSortPoint = sortPolyLineVertexInClockWise(wallId);
                        ObjectId newWallId_WithRoomIndex = new ObjectId();
                        List<ObjectId> listOfRoomTextId = new List<ObjectId>();
                        List<ObjectId> listOfRoomWallLineId = new List<ObjectId>();
                        createIndexingInRoomWall(listOfSortPoint, roomText, wallId, flagOfExternalWall, out newWallId_WithRoomIndex, out listOfRoomTextId, out listOfRoomWallLineId);

                        for (int p = 0; p < CreateShellPlanHelper.listOfCopyVector.Count; p++)
                        {
                            var pasteVector = CreateShellPlanHelper.listOfCopyVector[p];

                            if (flagOfExternalWall == false)
                            {
                                AEE_Utility.copyColonEntity(pasteVector, blockId);
                            }

                            AEE_Utility.copyColonEntity(pasteVector, listOfRoomTextId);
                            AEE_Utility.copyColonEntity(pasteVector, newWallId_WithRoomIndex);
                        }

                        listOfNewWallId_With_RoomIdentity.Add(newWallId_WithRoomIndex);
                        listOfListAllRoomWallLineId.Add(listOfRoomWallLineId);
                        roomCount++;
                        break;
                    }
                }
            }

            return listOfNewWallId_With_RoomIdentity;
        }


        private ObjectId createBlock(string roomText, double centerX, double centerY, string slabText)
        {
            CommonModule.blockCount = GetMaxBlockNameIndex(CommonModule.blockName);
            CommonModule.blockCount++;
            double insideCircleRadius = 175;
            double gapRadiusValue = 25;
            double centerCircleRadius = insideCircleRadius + gapRadiusValue;
            var centerCircleId = AEE_Utility.GetCircleId(centerX, centerY, 0, centerCircleRadius, false);
            double outsideCircleRadius = centerCircleRadius + 100;
            var outsideCircleId = AEE_Utility.GetCircleId(centerX, centerY, 0, outsideCircleRadius, false);
            DBObjectCollection listOfAllEntity_Block = new DBObjectCollection();
            List<double> listOfAngle = new List<double>();
            listOfAngle.Add(0);
            listOfAngle.Add(90);
            listOfAngle.Add(180);
            listOfAngle.Add(270);

            foreach (var angle in listOfAngle)
            {
                List<Entity> listOfEntity = GetEntityForBlock(centerX, centerY, centerCircleRadius, outsideCircleRadius, angle, centerCircleId);

                foreach (var ent in listOfEntity)
                {
                    listOfAllEntity_Block.Add(ent);
                }
            }

            var insideCircleEnt = AEE_Utility.GetCircleEntity(centerX, centerY, 0, insideCircleRadius, CommonModule.roomLayer, CommonModule.roomLayerColor);
            listOfAllEntity_Block.Add(insideCircleEnt);
            var txtY = centerY;
            var txtH = 100;
            if (!string.IsNullOrEmpty(slabText))
            {
                txtY += insideCircleRadius / 2;
                txtH = 90;
                var dbSlabTextEnt = AEE_Utility.GetDBTextEntity(slabText, centerX, centerY - insideCircleRadius / 2, 0, txtH * .75, CommonModule.roomLayer, CommonModule.roomLayerColor, 0);
                listOfAllEntity_Block.Add(dbSlabTextEnt);
            }
            var dbTextEnt = AEE_Utility.GetDBTextEntity(roomText, centerX, txtY, 0, txtH, CommonModule.roomLayer, CommonModule.roomLayerColor, 0);
            listOfAllEntity_Block.Add(dbTextEnt);
            string blockName = CommonModule.blockName + Convert.ToString(CommonModule.blockCount);
            ObjectId blockId = AEE_Utility.createBlock(blockName, listOfAllEntity_Block, CommonModule.roomLayer, CommonModule.roomLayerColor);
            CommonModule.blockCount++;
            AEE_Utility.deleteEntity(centerCircleId);
            AEE_Utility.deleteEntity(outsideCircleId);
            return blockId;
        }


        private List<Entity> GetEntityForBlock(double centerX, double centerY, double centerCircleRadius, double outsideCircleRadius, double angle, ObjectId centerCircleId)
        {
            var distancePointOfOutsideCircle = AEE_Utility.get_XY(angle, outsideCircleRadius);
            double offsetLineValue = 60;
            Point3d outsideCircleTangentPoint = new Point3d((centerX + distancePointOfOutsideCircle.X), (centerY + distancePointOfOutsideCircle.Y), 0);
            var midLineId = AEE_Utility.getLineId(centerX, centerY, 0, outsideCircleTangentPoint.X, outsideCircleTangentPoint.Y, 0, false);
            var offsetLineId_Positive = AEE_Utility.OffsetLine(midLineId, offsetLineValue, false);
            var offsetLineId_Negative = AEE_Utility.OffsetLine(midLineId, -offsetLineValue, false);
            var listOfInsctPoint_Positive = AEE_Utility.InterSectionBetweenTwoEntity(centerCircleId, offsetLineId_Positive);
            Point3d insctPoint_Positive = listOfInsctPoint_Positive[0];
            var listOfInsctPoint_Negative = AEE_Utility.InterSectionBetweenTwoEntity(centerCircleId, offsetLineId_Negative);
            Point3d insctPoint_Negative = listOfInsctPoint_Negative[0];
            AEE_Utility.deleteEntity(midLineId);
            AEE_Utility.deleteEntity(offsetLineId_Positive);
            AEE_Utility.deleteEntity(offsetLineId_Negative);
            Point3d centerPoint = new Point3d(centerX, centerY, 0);
            var arcLength1 = AEE_Utility.GetArcLength(centerPoint, insctPoint_Positive, insctPoint_Negative, centerCircleRadius);
            var arcLength2 = AEE_Utility.GetArcLength(centerPoint, insctPoint_Negative, insctPoint_Positive, centerCircleRadius);
            Point3d startPointOfArc = new Point3d();
            Point3d endPointOfArc = new Point3d();

            if (arcLength1 > arcLength2)
            {
                startPointOfArc = insctPoint_Negative;
                endPointOfArc = insctPoint_Positive;
            }

            else
            {
                startPointOfArc = insctPoint_Positive;
                endPointOfArc = insctPoint_Negative;
            }

            List<Entity> listOfEntity = new List<Entity>();
            var lineEnt1 = AEE_Utility.GetLineEntity(insctPoint_Positive.X, insctPoint_Positive.Y, 0, outsideCircleTangentPoint.X, outsideCircleTangentPoint.Y, 0, CommonModule.roomLayer, CommonModule.roomLayerColor);
            listOfEntity.Add(lineEnt1);
            var lineEnt2 = AEE_Utility.GetLineEntity(insctPoint_Negative.X, insctPoint_Negative.Y, 0, outsideCircleTangentPoint.X, outsideCircleTangentPoint.Y, 0, CommonModule.roomLayer, CommonModule.roomLayerColor);
            listOfEntity.Add(lineEnt2);
            var arcEnt = AEE_Utility.GetArcEntity(centerPoint, startPointOfArc, endPointOfArc, centerCircleRadius, CommonModule.roomLayer, CommonModule.roomLayerColor);
            listOfEntity.Add(arcEnt);
            return listOfEntity;
        }


        private List<Point2d> sortPolyLineVertexInClockWise(ObjectId wallId)
        {
            var listOfPolylineVertex = AEE_Utility.GetPolylineVertexPoint(wallId);
            List<Point2d> listOfSortPoint = new List<Point2d>();
            List<double> listOfAll_X = new List<double>();
            double maxY = listOfPolylineVertex.Max(sortPoint => sortPoint.Y);

            foreach (var vertexPoint in listOfPolylineVertex)
            {
                if (vertexPoint.Y == maxY)
                {
                    listOfAll_X.Add(vertexPoint.X);
                }
            }

            if (listOfAll_X.Count != 0)
            {
                double minX = listOfAll_X.Min();
                Point2d point = new Point2d(minX, maxY);
                int indexOfPoint = listOfPolylineVertex.IndexOf(point);

                if (indexOfPoint != -1)
                {
                    bool flagOfClockWise = true;

                    for (int p = indexOfPoint; p < listOfPolylineVertex.Count; p++)
                    {
                        Point2d sortPoint = listOfPolylineVertex[p];
                        listOfSortPoint.Add(sortPoint);

                        if (listOfSortPoint.Count == 2)
                        {
                            flagOfClockWise = checkVerticesIsClockWise(listOfSortPoint);

                            if (flagOfClockWise == false)
                            {
                                break;
                            }
                        }
                    }

                    for (int q = 0; q < indexOfPoint; q++)
                    {
                        Point2d sortPoint = listOfPolylineVertex[q];
                        listOfSortPoint.Add(sortPoint);

                        if (listOfSortPoint.Count == 2)
                        {
                            flagOfClockWise = checkVerticesIsClockWise(listOfSortPoint);

                            if (flagOfClockWise == false)
                            {
                                break;
                            }
                        }
                    }

                    if (flagOfClockWise == false)
                    {
                        listOfSortPoint.Clear();

                        for (int p = indexOfPoint; p >= 0; p--)
                        {
                            Point2d sortPoint = listOfPolylineVertex[p];
                            listOfSortPoint.Add(sortPoint);
                        }

                        for (int q = listOfPolylineVertex.Count - 1; q > indexOfPoint; q--)
                        {
                            Point2d sortPoint = listOfPolylineVertex[q];
                            listOfSortPoint.Add(sortPoint);
                        }
                    }
                }
            }

            return listOfSortPoint;
        }


        private bool checkVerticesIsClockWise(List<Point2d> listOfSortPoint)
        {
            Point2d point1 = listOfSortPoint[0];
            Point2d point2 = listOfSortPoint[1];
            var angleOfLine = AEE_Utility.GetAngleOfLine(point1.X, point1.Y, point2.X, point2.Y);
            CommonModule commonMod = new CommonModule();
            bool flag_X_Axis = false;
            bool flag_Y_Axis = false;
            commonMod.checkAngleOfLine_Axis(angleOfLine, out flag_X_Axis, out flag_Y_Axis);

            if (flag_X_Axis == true)
            {
                return true;
            }

            else
            {
                return false;
            }
        }


        private void createIndexingInRoomWall(List<Point2d> listOfSortPoint, string roomText, ObjectId wallId, bool flagOfExternalWall, out ObjectId newWallId_WithRoomIndex, out List<ObjectId> listOfRoomTextId, out List<ObjectId> listOfRoomWallLineId)
        {
            newWallId_WithRoomIndex = new ObjectId();
            double roomTextHght = 80;
            var wallEnt = AEE_Utility.GetEntityForRead(wallId);
            listOfRoomTextId = new List<ObjectId>();
            listOfRoomWallLineId = new List<ObjectId>();
            CommonModule commonModl = new CommonModule();
            int wallCount = 1;
            List<Point3d> listOfNewVerticePoint = new List<Point3d>();

            for (int i = 0; i < listOfSortPoint.Count; i++)
            {
                Point2d point1 = listOfSortPoint[i];
                Point2d point2 = new Point2d();

                if (i == (listOfSortPoint.Count - 1))
                {
                    point2 = listOfSortPoint[0];
                }

                else
                {
                    point2 = listOfSortPoint[i + 1];
                }

                var lineId = AEE_Utility.getLineId(wallEnt, point1.X, point1.Y, 0, point2.X, point2.Y, 0, false);
                listOfNewVerticePoint.Add(new Point3d(point1.X, point1.Y, 0));
                //listOfNewVerticePoint.Add(new Point3d(point2.X, point2.Y, 0));
                var length = AEE_Utility.GetLengthOfLine(lineId);

                if (length >= 1)
                {
                    double offsetValue = 150;
                    ////Point3d centerPoint = AEE_Utility.GetMidPoint(lineId);
                    var offsetLineId = AEE_Utility.OffsetLine(lineId, offsetValue, false);
                    Point3d centerPoint = AEE_Utility.GetMidPoint(offsetLineId);
                    var flagOfPointInside = AEE_Utility.GetPointIsInsideThePolyline(wallId, centerPoint);

                    if (flagOfPointInside == false && flagOfExternalWall == false)
                    {
                        AEE_Utility.deleteEntity(offsetLineId);
                        offsetLineId = AEE_Utility.OffsetLine(lineId, -offsetValue, false);
                        centerPoint = AEE_Utility.GetMidPoint(offsetLineId);
                    }

                    else
                        if (flagOfExternalWall == true)
                    {
                        if (flagOfPointInside == true)
                        {
                            AEE_Utility.deleteEntity(offsetLineId);
                            offsetLineId = AEE_Utility.OffsetLine(lineId, -offsetValue, false);
                            centerPoint = AEE_Utility.GetMidPoint(offsetLineId);
                        }
                    }

                    List<Point3d> listOfStrtEndPoint = commonModl.getStartEndPointOfLine(lineId);
                    Point3d strtPoint = listOfStrtEndPoint[0];
                    Point3d endPoint = listOfStrtEndPoint[1];
                    double angleOfLine = AEE_Utility.GetAngleOfLine(strtPoint.X, strtPoint.Y, endPoint.X, endPoint.Y);
                    string wallName = "W" + Convert.ToString(wallCount) + CommonModule.symbolOfUnderScore + roomText;
                    var wallTextId = AEE_Utility.CreateTextWithAngle(wallName, centerPoint.X, centerPoint.Y, 0, roomTextHght, CommonModule.roomLayer, CommonModule.roomLayerColor, angleOfLine);
                    AEE_Utility.AttachXData(lineId, wallName, CommonModule.xDataAsciiName);
                    listOfAllRoomWallText_ForXData.Add(wallName);
                    wallCount++;
                    listOfRoomTextId.Add(wallTextId);
                    listOfRoomWallLineId.Add(lineId);
                    AEE_Utility.deleteEntity(offsetLineId);
                }

                //AEE_Utility.deleteEntity(lineId);
            }

            listOfNewVerticePoint = listOfNewVerticePoint.Distinct().ToList();
            newWallId_WithRoomIndex = AEE_Utility.createRectangleWithSameProperty(listOfNewVerticePoint, wallEnt);
            AEE_Utility.AttachXData(newWallId_WithRoomIndex, roomText, CommonModule.xDataAsciiName);
            listOfAllRoomText_ForXData.Add(roomText);
        }


        private int GetMaxBlockNameIndex(string inputBlockName)
        {
            int maxIndex = 0;
            List<int> listOfAllBlockIndex = new List<int>();
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            using (Transaction acTrans = db.TransactionManager.StartTransaction())
            {
                BlockTable blkTable = (BlockTable)acTrans.GetObject(db.BlockTableId, OpenMode.ForRead);  // Get the block table from the drawing

                foreach (ObjectId blockId in blkTable)
                {
                    BlockTableRecord blkTblRec = acTrans.GetObject(blockId, OpenMode.ForWrite) as BlockTableRecord;
                    string blockName = blkTblRec.Name;

                    if (blockName.Contains(inputBlockName))
                    {
                        string[] nameArray = blockName.Split('_');

                        if (nameArray.Length == 3)
                        {
                            string blkName = Convert.ToString(nameArray.GetValue(0)) + CommonModule.symbolOfUnderScore + Convert.ToString(nameArray.GetValue(1)) + CommonModule.symbolOfUnderScore;

                            if (blkName == inputBlockName)
                            {
                                listOfAllBlockIndex.Add(Convert.ToInt32(nameArray.GetValue(2)));
                            }
                        }
                    }
                }
            }

            if (listOfAllBlockIndex.Count != 0)
            {
                maxIndex = listOfAllBlockIndex.Max();
            }

            return maxIndex;
        }
    }
}
