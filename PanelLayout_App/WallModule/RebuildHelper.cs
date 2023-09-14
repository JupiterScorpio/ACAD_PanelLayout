﻿using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PanelLayout_App.WallModule
{
    public class RebuildHelper
    {
        public string wallNameForUpdate { get; private set; }

        public void callMethodOfRebuild()
        {
            RibbonHelper.rebuildPanelButton.IsEnabled = false;

            string rebuildCreationMsg = "Panels and corners are rebuilding....";
            ProgressForm progressForm = new ProgressForm();
            progressForm.Show();

            setBOMDataOfCornerAndWall(progressForm, rebuildCreationMsg);

            AEE_Utility.RemoveXDataName(CommonModule.xDataAsciiName);

            rebuildCreationMsg = "Panels and corners are rebuilt.";
            progressForm.ReportProgress(100, rebuildCreationMsg);
            progressForm.Close();

            MessageBox.Show(rebuildCreationMsg, CommonModule.headerText_messageBox, MessageBoxButtons.OK, MessageBoxIcon.Information);

            //Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.SendStringToExecute((String)RibbonHelper.createBOMButton.CommandParameter, true, false, true);

            RibbonHelper.createBOMButton.IsEnabled = true;
        }

        //Added on 10/06/2023 by SDM
        public void callMethodOfUpdate()
        {
           // RibbonHelper.updateWallButton.IsEnabled = false;

            string promptSelMsg = "\n\nSelect wall name\n\n";
            selectWallNameWithWindowSelection(promptSelMsg);

            //string rebuildCreationMsg = "Panels and corners are rebuilding....";
            //ProgressForm progressForm = new ProgressForm();
            //progressForm.Show();

            //setBOMDataOfCornerAndWall(progressForm, rebuildCreationMsg);

            //AEE_Utility.RemoveXDataName(CommonModule.xDataAsciiName);

            //rebuildCreationMsg = "Panels and corners are rebuilt.";
            //progressForm.ReportProgress(100, rebuildCreationMsg);
            //progressForm.Close();

            //MessageBox.Show(rebuildCreationMsg, CommonModule.headerText_messageBox, MessageBoxButtons.OK, MessageBoxIcon.Information);

          
            RibbonHelper.createBOMButton.IsEnabled = true;
            RibbonHelper.rebuildPanelButton.IsEnabled = true;
        }
        //Added on 11/07/2023 by SDM
        private void selectWallNameWithWindowSelection(string promptSelMsg)
        {
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = acDoc.Editor;
            Database db = HostApplicationServices.WorkingDatabase;



            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                PromptSelectionResult acSSPrompt = default(PromptSelectionResult);
                PromptSelectionOptions acKeywordOpts = new PromptSelectionOptions();
                acKeywordOpts.SingleOnly = true;
                acKeywordOpts.SinglePickInSpace = true;
                acKeywordOpts.MessageForAdding = (promptSelMsg);

                TypedValue[] filterlist = new TypedValue[1];
                filterlist.SetValue(new TypedValue((int)DxfCode.Start, "TEXT"), 0);
                SelectionFilter filter = new SelectionFilter(filterlist);
                acSSPrompt = ed.GetSelection(acKeywordOpts, filter);

                if (acSSPrompt.Status == PromptStatus.OK)
                {
                    // Start a transaction to open the selected objects
                    SelectionSet acSSet = acSSPrompt.Value;
                    // Iterate through the selections set
                    foreach (SelectedObject so in acSSet)
                    {
                        Entity ent = tr.GetObject(so.ObjectId, OpenMode.ForWrite) as Entity;
                        ObjectId id = so.ObjectId;
                        if (ent is DBText text)
                        {
                            ed.WriteMessage(text.TextString + " was selected");
                            ed.WriteMessage("\n\n");
                            ed.WriteMessage("select a Wall Panel or Corner with Text");
                            if (text.TextString.StartsWith("W"))
                            {
                                var wallNameForUpdate = text.TextString;
                                //-------select wall panels
                                acKeywordOpts.SingleOnly = false;
                                acKeywordOpts.SinglePickInSpace = false;
                                // filterlist.SetValue(new TypedValue((int)DxfCode.Start, "MTEXT"), 0);
                                acSSPrompt = ed.GetSelection(acKeywordOpts);

                                if (acSSPrompt.Status == PromptStatus.OK)
                                {
                                    acSSet = acSSPrompt.Value;
                                    var listOfAllWallEntities = new List<Entity>();
                                    Polyline polyline = null;
                                    MText mText1 = null;
                                    MText mText2 = null;
                                    foreach (SelectedObject _so in acSSet)
                                    {
                                        Entity _ent = tr.GetObject(_so.ObjectId, OpenMode.ForWrite) as Entity;
                                        ObjectId _id = _so.ObjectId;
                                        if (_ent is Polyline pl)
                                            polyline = pl;
                                        if (_ent is MText txt)
                                        {
                                            if (mText1 == null)
                                                mText1 = txt;
                                            else
                                                mText2 = txt;
                                        }
                                        listOfAllWallEntities.Add(_ent);
                                    }
                                    AddWallPanelOrCorner(wallNameForUpdate, polyline, mText1, mText2);

                                }
                            }

                        }
                    }
                }
                else
                    ed.WriteMessage("wrong selection, please select a wall name");
                ed.WriteMessage("\n\n");

                tr.Commit();
            }
        }
        //Added on 11/07/2023 by SDM
        private void AddWallPanelOrCorner(string wallname, Polyline polyline, MText mText1, MText mText2)
        {
            string panel_width = "";
            string panelType = "";
            if (polyline?.Layer == CommonModule.wallPanelLyrName)
                panelType = CommonModule.wallPanelType;
            else if (polyline?.Layer == CommonModule.kickerWallPanelLayerName)
                panelType = CommonModule.kickerPanelType;
            else if (polyline?.Layer == CommonModule.slabJointWallPanelLayerName)
                panelType = CommonModule.slabJointPanelType;
            if (panelType != "")
            {
                if (polyline != null)
                {
                    AEE_Utility.AttachXData(polyline.ObjectId, wallname, CommonModule.xDataAsciiName);
                    WallPanelHelper.listOfAllWallPanelRectId.Add(polyline.ObjectId);
                    if (mText2 != null)
                        WallPanelHelper.listOfAllWallPanelRectId.Add(polyline.ObjectId);




                    var wallline = InternalWallHelper.GetInternalWallLineByxDataRegAppName(wallname);
                    CommonModule commonMod = new CommonModule();
                    if (wallline != null && wallline.ObjectId.IsValid)
                    {
                        bool flagOfWallPnlLine_X_Axis = false;
                        bool flagOfWallPnlLine_Y_Axis = false;
                        commonMod.checkAngleOfLine_Axis(AEE_Utility.GetLineAngle(wallline), out flagOfWallPnlLine_X_Axis, out flagOfWallPnlLine_Y_Axis);

                        List<Point2d> listOfPanelRectVertice = AEE_Utility.GetPolylineVertexPoint(polyline.ObjectId);
                        for (int index = 0; index < listOfPanelRectVertice.Count; index++)
                        {
                            Point2d crrntPoint = listOfPanelRectVertice[index];
                            Point2d nextPoint = new Point2d();
                            if (index == (listOfPanelRectVertice.Count - 1))
                            {
                                nextPoint = listOfPanelRectVertice[0];
                            }
                            else
                            {
                                nextPoint = listOfPanelRectVertice[index + 1];
                            }

                            var wallPanelRectLineLength = AEE_Utility.GetLengthOfLine(crrntPoint.X, crrntPoint.Y, nextPoint.X, nextPoint.Y);
                            wallPanelRectLineLength = Math.Round(wallPanelRectLineLength);
                            if (wallPanelRectLineLength > 0)
                            {
                                var wallPanelRectLineAngle = AEE_Utility.GetAngleOfLine(crrntPoint.X, crrntPoint.Y, nextPoint.X, nextPoint.Y);

                                bool flagOfWallRectPnlLine_X_Axis = false;
                                bool flagOfWallRectPnlLine_Y_Axis = false;
                                commonMod.checkAngleOfLine_Axis(wallPanelRectLineAngle, out flagOfWallRectPnlLine_X_Axis, out flagOfWallRectPnlLine_Y_Axis);

                                bool flagOfWallPanel = false;
                                if (flagOfWallPnlLine_X_Axis == flagOfWallRectPnlLine_X_Axis && Math.Abs(crrntPoint.Y - wallline.StartPoint.Y) < 1)
                                {
                                    flagOfWallPanel = true;
                                }
                                else if (flagOfWallPnlLine_Y_Axis == flagOfWallRectPnlLine_Y_Axis && Math.Abs(crrntPoint.X - wallline.StartPoint.X) < 1)
                                {
                                    flagOfWallPanel = true;
                                }

                                if (flagOfWallPanel == true)
                                {
                                    panel_width = wallPanelRectLineLength.ToString("0.");
                                    var line = AEE_Utility.getLineId(crrntPoint, nextPoint, false);
                                    AEE_Utility.AttachXData(line, wallname, CommonModule.xDataAsciiName);

                                    WallPanelHelper.listOfAllWallPanelLineId.Add(line);
                                    if (mText2 != null)
                                        WallPanelHelper.listOfAllWallPanelLineId.Add(line);
                                    break;
                                }
                            }
                        }

                    }
                }

                if (mText1 != null)
                {
                    var code = mText1.Text.QuickSplit(" ", 1);
                    var ht = mText1.Text.QuickSplit(" ", 2);
                    var data = panel_width + "@" + ht + "@0@" + code + "@" + mText1.Text + "@" + panelType;
                    WallPanelHelper.listOfAllWallPanelData.Add(data);

                    WallPanelHelper.listOfAllWallPanelRectTextId.Add(mText1.ObjectId);
                }
                if (mText2 != null)
                {
                    var code = mText2.Text.QuickSplit(" ", 1);
                    var ht = mText2.Text.QuickSplit(" ", 2);
                    var data = panel_width + "@" + ht + "@0@" + code + "@" + mText2.Text + "@" + panelType;
                    WallPanelHelper.listOfAllWallPanelData.Add(data);

                    WallPanelHelper.listOfAllWallPanelRectTextId.Add(mText2.ObjectId);
                }
            }
            else
            {
                if (polyline?.Layer == CommonModule.internalCornerLyrName)
                    panelType = CommonModule.internalCornerDescp;
                else if (polyline?.Layer == CommonModule.slabJointCornerLayerName)
                    panelType = CommonModule.slabJointCornerDescp;
                else if (polyline?.Layer == CommonModule.kickerCornerLayerName)
                    panelType = CommonModule.kickerCornerText;
                else if (polyline?.Layer == CommonModule.beamCornerLayerName)
                    panelType = CommonModule.beamInternalCornerText;

                AEE_Utility.AttachXData(polyline.ObjectId, wallname, CommonModule.xDataAsciiName);
                CornerHelper.listOfAllCornerId_ForBOM.Add(polyline.ObjectId);
                CornerHelper.listOfAllCornerTextId_ForBOM.Add(mText1.ObjectId);
                string data = mText1.Text.Replace("\n", " ").Replace(" +", "");
                string itemCode = data.QuickSplit(" ", 0);
                string description = mText1.Text;
                string type = panelType;
                string flange1 = data.QuickSplit(" ", 1);
                string flange2 = data.QuickSplit(" ", 2);
                string cornerHeight = data.QuickSplit(" ", 3);

                CornerHelper.listOfAllCornerText_ForBOM.Add(itemCode + "@" + description + "@" + type + "@" + flange1 + "@" + flange2 + "@" + cornerHeight + "@" + mText1.ObjectId);
                if (mText2 != null)
                {
                    CornerHelper.listOfAllCornerId_ForBOM.Add(polyline.ObjectId);
                    CornerHelper.listOfAllCornerTextId_ForBOM.Add(mText2.ObjectId);

                    data = mText2.Text.Replace("\n", " ").Replace(" +", "");
                    itemCode = data.QuickSplit(" ", 0);
                    description = mText2.Text;
                    type = panelType;
                    flange1 = data.QuickSplit(" ", 1);
                    flange2 = data.QuickSplit(" ", 2);
                    cornerHeight = data.QuickSplit(" ", 3);
                    CornerHelper.listOfAllCornerText_ForBOM.Add(itemCode + "@" + description + "@" + type + "@" + flange1 + "@" + flange2 + "@" + cornerHeight + "@" + mText2.ObjectId);

                }
            }
        }

        public void setBOMDataOfCornerAndWall(ProgressForm progressForm, string progressbarMsg)
        {
            setBOMDataOfWall(progressForm, progressbarMsg);
            setBOMDataOfCorner(progressForm, progressbarMsg);
        }

        public void setBOMDataOfWall(ProgressForm progressForm, string progressbarMsg)
        {
            BOMHelper bomHlp = new BOMHelper();

            for (int index = 0; index < WallPanelHelper.listOfAllWallPanelRectId.Count; index++)
            {
                if ((index % 500) == 0)
                {
                    progressForm.ReportProgress(1, progressbarMsg);
                }

                var panelRectId = WallPanelHelper.listOfAllWallPanelRectId[index];
                var wallPanelTextId = WallPanelHelper.listOfAllWallPanelRectTextId[index];

                if (panelRectId.IsErased == false && panelRectId.IsValid == true && wallPanelTextId.IsErased == false && wallPanelTextId.IsValid == true)
                {
                    var panelLineId = WallPanelHelper.listOfAllWallPanelLineId[index];
                    double wallPanelAngle = AEE_Utility.GetAngleOfLine(panelLineId);
                    Entity panelRectEnt = AEE_Utility.GetEntityForRead(panelRectId);
                    string wallPanelRectLayer = panelRectEnt.Layer;

                    string data = WallPanelHelper.listOfAllWallPanelData[index];

                    var array = data.Split('@');
                    double wallWidth = Convert.ToDouble(array.GetValue(0));
                    double wallHeight = Convert.ToDouble(array.GetValue(1));
                    double wallLength = Convert.ToDouble(array.GetValue(2));
                    string itemCode = Convert.ToString(array.GetValue(3));
                    string description = Convert.ToString(array.GetValue(4));
                    string wallType = Convert.ToString(array.GetValue(5));

                    string newWallPanelText = "";
                    wallWidth = getWallPanelWidth(wallPanelAngle, panelRectId, wallPanelTextId, wallPanelRectLayer, wallWidth, out newWallPanelText);
                    if (newWallPanelText != "")
                    {
                        description = newWallPanelText;
                    }

                    bomHlp.addBOMDataOfWallPanelAndCorner(panelRectId, panelLineId, wallWidth, wallHeight, wallLength, itemCode, description, wallType);
                }
            }
        }

        private double getWallPanelWidth(double wallPanelAngle, ObjectId wallPanelRectId, ObjectId wallPanelTextId, string wallPanelRectLayer, double wallPanelWidth, out string newWallPanelText)
        {
            double newWallPanelWidth = wallPanelWidth;
            newWallPanelText = "";

            string wallPanelLyrName = CommonModule.wallPanelLyrName;
            string kickerWallPanelLayerName = CommonModule.kickerWallPanelLayerName;
            string slabJointCornerLayerName = CommonModule.slabJointCornerLayerName;

            CommonModule commonMod = new CommonModule();
            bool flagOfWallPnlLine_X_Axis = false;
            bool flagOfWallPnlLine_Y_Axis = false;
            commonMod.checkAngleOfLine_Axis(wallPanelAngle, out flagOfWallPnlLine_X_Axis, out flagOfWallPnlLine_Y_Axis);

            Entity panelRectEnt = AEE_Utility.GetEntityForRead(wallPanelRectId);
            if (wallPanelRectLayer == wallPanelLyrName || wallPanelRectLayer == kickerWallPanelLayerName || wallPanelRectLayer == slabJointCornerLayerName)
            {
                List<Point2d> listOfPanelRectVertice = AEE_Utility.GetPolylineVertexPoint(wallPanelRectId);
                for (int index = 0; index < listOfPanelRectVertice.Count; index++)
                {
                    Point2d crrntPoint = listOfPanelRectVertice[index];
                    Point2d nextPoint = new Point2d();
                    if (index == (listOfPanelRectVertice.Count - 1))
                    {
                        nextPoint = listOfPanelRectVertice[0];
                    }
                    else
                    {
                        nextPoint = listOfPanelRectVertice[index + 1];
                    }

                    var wallPanelRectLineLength = AEE_Utility.GetLengthOfLine(crrntPoint.X, crrntPoint.Y, nextPoint.X, nextPoint.Y);
                    wallPanelRectLineLength = Math.Round(wallPanelRectLineLength);
                    if (wallPanelRectLineLength > 0)
                    {
                        var wallPanelRectLineAngle = AEE_Utility.GetAngleOfLine(crrntPoint.X, crrntPoint.Y, nextPoint.X, nextPoint.Y);

                        bool flagOfWallRectPnlLine_X_Axis = false;
                        bool flagOfWallRectPnlLine_Y_Axis = false;
                        commonMod.checkAngleOfLine_Axis(wallPanelRectLineAngle, out flagOfWallRectPnlLine_X_Axis, out flagOfWallRectPnlLine_Y_Axis);

                        bool flagOfWallPanel = false;
                        if (flagOfWallPnlLine_X_Axis == flagOfWallRectPnlLine_X_Axis)
                        {
                            flagOfWallPanel = true;
                        }
                        else if (flagOfWallPnlLine_Y_Axis == flagOfWallRectPnlLine_Y_Axis)
                        {
                            flagOfWallPanel = true;
                        }

                        if (flagOfWallPanel == true)
                        {
                            newWallPanelWidth = wallPanelRectLineLength;
                            newWallPanelText = changeWidthOfWallPanelText(wallPanelRectLayer, wallPanelTextId, wallPanelWidth, newWallPanelWidth);
                            break;
                        }
                    }
                }
            }
            return newWallPanelWidth;
        }

        private string changeWidthOfWallPanelText(string wallPanelRectLayer, ObjectId wallPanelTextId, double wallPanelWidth, double newWallPanelWidth)
        {
            string newWallPanelText = "";
            if (wallPanelWidth != newWallPanelWidth)
            {
                var wallPanelText = AEE_Utility.GetTextOfMtext(wallPanelTextId);
                if (wallPanelText == "")
                {
                    wallPanelText = AEE_Utility.GetTextOfDBtext(wallPanelTextId);
                    if (wallPanelText != "")
                    {
                        newWallPanelText = getNewWallPanelText(wallPanelRectLayer, wallPanelText, newWallPanelWidth);
                        AEE_Utility.changeDBText(wallPanelTextId, newWallPanelText);
                    }
                }
                else
                {
                    newWallPanelText = getNewWallPanelText(wallPanelRectLayer, wallPanelText, newWallPanelWidth);
                    AEE_Utility.changeMText(wallPanelTextId, newWallPanelText);
                }
            }
            return newWallPanelText;
        }

        private string getNewWallPanelText(string wallPanelRectLayer, string wallPanelText, double newWallPanelWidth)
        {
            //// "450 WPR 2875"

            string newWallPanelText = "";

            string[] array = wallPanelText.Split(' ');
            for (int index = 0; index < array.Length; index++)
            {
                string text = array[index];
                if (index == 0)
                {
                    text = Convert.ToString(newWallPanelWidth);
                }
                if (index == (array.Length - 1))
                {
                    newWallPanelText = newWallPanelText + text;
                }
                else
                {
                    newWallPanelText = newWallPanelText + text + " ";
                }
            }
            return newWallPanelText;
        }

        private void getBeamBottomWallPanelText()
        {
            ////// "450 WPR 2875"

            //string newWallPanelText = "";

            //string[] array = wallPanelText.Split(' ');
            //for (int index = 0; index < array.Length; index++)
            //{
            //    string text = array[index];
            //    if (index == 0)
            //    {
            //        text = Convert.ToString(newWallPanelWidth);
            //    }
            //    if (index == (array.Length - 1))
            //    {
            //        newWallPanelText = newWallPanelText + text;
            //    }
            //    else
            //    {
            //        newWallPanelText = newWallPanelText + text + " ";
            //    }
            //}
            //return newWallPanelText;
        }
        public void setBOMDataOfCorner(ProgressForm progressForm, string progressbarMsg)
        {
            BOMHelper bomHlp = new BOMHelper();

            BOMHelper.flagOfCornerData = true;
            List<ObjectId> listOfCornerId = new List<ObjectId>();
            List<string> listOfCornerText = new List<string>();

            List<ObjectId> listOfDistinctAllCornerId_ForBOM = CornerHelper.listOfAllCornerId_ForBOM.Distinct().ToList();

            for (int index = 0; index < listOfDistinctAllCornerId_ForBOM.Count; index++)
            {
                if ((index % 100) == 0)
                {
                    progressForm.ReportProgress(1, progressbarMsg);
                }

                ObjectId cornerId = listOfDistinctAllCornerId_ForBOM[index];

                if (cornerId.IsValid == true && cornerId.IsErased == false)
                {
                    List<ObjectId> cornerTextIds = new List<ObjectId>();

                    for (int i = 0; i < CornerHelper.listOfAllCornerId_ForBOM.Count; i++)
                    {
                        if (cornerId == CornerHelper.listOfAllCornerId_ForBOM[i])
                        {
                            cornerTextIds.Add(CornerHelper.listOfAllCornerTextId_ForBOM[i]);
                        }
                    }


                    string cornerText = CornerHelper.listOfAllCornerText_ForBOM[index];

                    string[] array = cornerText.Split('@');
                    string itemCode = Convert.ToString(array.GetValue(0));
                    string description = Convert.ToString(array.GetValue(1));
                    string type = Convert.ToString(array.GetValue(2));
                    double flange1 = Convert.ToDouble(array.GetValue(3));
                    double flange2 = Convert.ToDouble(array.GetValue(4));
                    double cornerHeight = Convert.ToDouble(array.GetValue(5));

                    List<double> listOfFlange = changeCornerFlangeAsPerLayer(cornerId, cornerTextIds);
                    if (listOfFlange.Count != 0)
                    {
                        flange1 = listOfFlange[0];
                        flange2 = listOfFlange[1];
                    }

                    bomHlp.addBOMDataOfWallPanelAndCorner(cornerId, cornerId, flange1, cornerHeight, flange2, itemCode, description, type);
                }
            }
            BOMHelper.flagOfCornerData = false;
        }

        private List<double> changeCornerFlangeAsPerLayer(ObjectId cornerId, List<ObjectId> cornerTextIds)
        {
            string intrnlCrnrLyrName = CommonModule.internalCornerLyrName;
            string slabJointCrnrLyrName = CommonModule.slabJointCornerLayerName;
            string kickerCrnrLyrName = CommonModule.kickerCornerLayerName;
            string beamCrnrLyrName = CommonModule.beamCornerLayerName;

            CornerHelper cornerHlp = new CornerHelper();
            List<double> listOfFlange = new List<double>();

            var cornerEnt = AEE_Utility.GetEntityForRead(cornerId);
            string cornerLayer = cornerEnt.Layer;
            if (cornerLayer == intrnlCrnrLyrName || cornerLayer == slabJointCrnrLyrName || cornerLayer == kickerCrnrLyrName)
            {
                listOfFlange = cornerHlp.getInternalConerFlange(cornerId, cornerTextIds, cornerLayer);
            }
            else if (cornerLayer == beamCrnrLyrName)
            {
                listOfFlange = cornerHlp.getCornerFlangeLength(cornerId);
                if (listOfFlange.Count != 0)
                {
                    cornerHlp.changeBeamBottomCornerText(cornerTextIds, listOfFlange[0], listOfFlange[1]);
                }
            }

            return listOfFlange;
        }
    }
}
