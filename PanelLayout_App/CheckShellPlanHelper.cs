using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using PanelLayout_App.CivilModel;
using PanelLayout_App.Licensing;
using PanelLayout_App.WallModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace PanelLayout_App
{
    public class CheckShellPlanHelper
    {
        public static int escBtnCount = 0;
        public static int enterBtnCount = 0;

        public static List<ObjectId> listOfAllSelectedObjectIds = new List<ObjectId>();
    
        public static List<ObjectId> listOfSelectedInternalWall_ObjId = new List<ObjectId>();
        public static List<ObjectId> listOfSelectedExternalWall_ObjId = new List<ObjectId>();
        public static List<ObjectId> listOfSelectedDoor_ObjId = new List<ObjectId>();
        public static List<ObjectId> listOfSelectedWindow_ObjId = new List<ObjectId>();
        public static List<ObjectId> listOfSelectedChajja_ObjId = new List<ObjectId>();
        public static List<ObjectId> listOfSelectedOffsetBeam_ObjId = new List<ObjectId>();
        public static List<ObjectId> listOfSelectedBeam_ObjId = new List<ObjectId>();//Added on 12/03/2023 by SDM
        public static List<ObjectId> listOfSelectedDuct_ObjId = new List<ObjectId>();
        public static List<ObjectId> listOfSelectedStaircase_ObjId = new List<ObjectId>();
        public static List<ObjectId> listOfSelectedLift_ObjId = new List<ObjectId>();
        public static List<ObjectId> listOfSelectedSunkanSlab_ObjId = new List<ObjectId>();
        public static List<ObjectId> listOfSelectedExternalSunkanSlab_ObjId = new List<ObjectId>();

        public static List<ObjectId> listOfSelectedParapetWall_ObjId = new List<ObjectId>();

        public static List<ObjectId> listOfSelectedNonPerpendiculareLine = new List<ObjectId>();

        public static List<ObjectId> listOfAllNonPerpendicularPoint_Circle_ObjId = new List<ObjectId>();
        public static string checkPlanDocumentName = "";

        public static double doorWindowCount_ForLayoutCreator = 0;
        public static double slabJointCount_ForLayoutCreator = 0;
        public static double kickerCount_ForLayoutCreator = 0;
        public void commandMethodOfCheckShellPlan()
        {
            methodOfClearList();

            string promptSelMsg = "\n\nSelect panel layout\n\n";
            selectPanelLayoutWithWindowSelection(promptSelMsg);
            if (listOfAllSelectedObjectIds.Count != 0)
            {
                RibbonHelper.checkShellPlanButton.IsEnabled = false;

                setAllObjectIdFromLayer(listOfAllSelectedObjectIds);
                checkShellPlan_Is_Perpendicular();

                if (listOfSelectedNonPerpendiculareLine.Count != 0)
                {
                    string cornerCountInStr = Convert.ToString(listOfSelectedNonPerpendiculareLine.Count);
                    string msg = cornerCountInStr + " Corners are not at right angle, are marked with circle.";
                    if (!CommandModule.dply)
                        MessageBox.Show(msg, CommonModule.headerText_messageBox, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    string msg = "All Corners are at right angle.";
                    if (!CommandModule.dply)
                        MessageBox.Show(msg, CommonModule.headerText_messageBox, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                //Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.SendStringToExecute((String)RibbonHelper.createShellPlanButton.CommandParameter, true, false, true);

                RibbonHelper.createShellPlanButton.IsEnabled = true;
            }
        }


        private void methodOfClearList()
        {
            CreateShellPlanHelper.start_X_NewLayout = 0;
            CreateShellPlanHelper.start_Y_NewLayout = 0;

            checkPlanDocumentName = "";
            escBtnCount = 0;
            enterBtnCount = 0;
            doorWindowCount_ForLayoutCreator = 0;
            slabJointCount_ForLayoutCreator = 0;
            kickerCount_ForLayoutCreator = 0;

            AEE_Utility.RemoveXDataName(CommonModule.xDataAsciiName);

            CreateShellPlanHelper.listOfAllVerticesPoint.Clear();

            listOfAllSelectedObjectIds.Clear();
            listOfSelectedInternalWall_ObjId.Clear();
            listOfSelectedExternalWall_ObjId.Clear();
            listOfSelectedDoor_ObjId.Clear();
            listOfSelectedWindow_ObjId.Clear();
            listOfSelectedChajja_ObjId.Clear();
            listOfSelectedOffsetBeam_ObjId.Clear();
            listOfSelectedBeam_ObjId.Clear();//Added on 12/03/2023 by SDM
            listOfSelectedDuct_ObjId.Clear();
            listOfSelectedNonPerpendiculareLine.Clear();
            listOfAllNonPerpendicularPoint_Circle_ObjId.Clear();
            InternalWallHelper.listOfInternalWallObjId.Clear();
            InternalWallHelper.listOfListOfInternalWallRoomLineId_With_WallName.Clear();

            InternalWallHelper.listOfNonPerpendiculareLine.Clear();
            ExternalWallHelper.listOfExternalWallObjId.Clear();
            ExternalWallHelper.listOfListOfExternalWallRoomLineId_With_WallName.Clear();

            InternalWallHelper.listOfAllCornerId1_ForInsctDoor.Clear();
            InternalWallHelper.listOfAllCornerId2_ForInsctDoor.Clear();
            InternalWallHelper.listOfAllCornerId_ForDoor_And_Beam.Clear();
            InternalWallHelper.listOfLinesBtwTwoCrners_ObjId.Clear();
            InternalWallHelper.listOfOffsetLinesBtwTwoCrners_ObjId.Clear();
            InternalWallHelper.listOfDistnceBtwTwoCrners.Clear();
            InternalWallHelper.listOfDistnceAndObjId_BtwTwoCrners.Clear();
            CornerHelper.listOfWallLineId_OfExternalCrner_ForStretch.Clear();
            CornerHelper.listOfListOfExternalCrnerId_InsctToWallLine_ForStretch.Clear();

            CornerHelper.listOfInternalCornerTextId_ForStretch.Clear();
            CornerHelper.listOfInternalCornerId_ForStretch.Clear();
            CornerHelper.listOfOffstInternalCornerId_ForStretch.Clear();
            CornerHelper.listOfWallLineId_OfInternalCrner_ForStretch.Clear();
            CornerHelper.listOfListOfInternalCrnerId_InsctToWallLine_ForStretch.Clear();
            CornerHelper.listOfAllCornerId_ForBOM.Clear();
            CornerHelper.listOfAllCornerTextId_ForBOM.Clear();
            CornerHelper.listOfAllCornerText_ForBOM.Clear();
            CornerHelper.listOfAllCornerElevInfo.Clear();

            BeamHelper beamHlp = new BeamHelper();
            beamHlp.clearListOfBeamHelper();
            DoorHelper.clearListMethod();

            WindowHelper.listOfWindowObjId.Clear();
            WindowHelper.listOfWindowObjId_Str.Clear();
            WindowHelper.listOfWindowPanelLine_ObjId.Clear();
            WindowHelper.listOfWindowPanelOffsetLine_ObjId.Clear();
            WindowHelper.listOfWindowPanelLine_ObjId_InStr.Clear();

            DuctHelper.listOfDuctObjId.Clear();
            DuctHelper.listOfListOfDuctRoomLineId_With_WallName.Clear();

            ParapetHelper.listOfParapetObjId.Clear();
            StairCaseHelper.listOfStairCaseObjId.Clear();
            StairCaseHelper.listOfListOfStaircaseRoomLineId_With_WallName.Clear();

            listOfSelectedStaircase_ObjId.Clear();
            LiftHelper.listOfLiftObjId.Clear();
            LiftHelper.listOfListOfLiftRoomLineId_With_WallName.Clear();

            listOfSelectedLift_ObjId.Clear();
            listOfSelectedSunkanSlab_ObjId.Clear();
            listOfSelectedExternalSunkanSlab_ObjId.Clear();
            SunkanSlabHelper.listOfSunkanSlab_ObjId.Clear();
            SunkanSlabHelper.listOfLinesBtwTwoCrners_SunkanSlabWallLineId.Clear();
            SunkanSlabHelper.listOfLinesBtwTwoCrners_SunkanSlabWallLineflag.Clear();
            SunkanSlabHelper.listOfLinesBtwTwoCrners_SunkanSlabId.Clear();

            SunkanSlabHelper.listOfWindowWall_SunkanSlabWallLineId.Clear();
            SunkanSlabHelper.listOfWindowWall_SunkanSlabWallLineflag.Clear();
            SunkanSlabHelper.listOfWindowWall_SunkanSlabId.Clear();
            SunkanSlabHelper.listOfWallName_SunkanSlabId.Clear();

            SunkanSlabHelper.listOfDoorWall_SunkanSlabWallLineId.Clear();
            SunkanSlabHelper.listOfDoorWall_SunkanSlabWallLineflag.Clear();
            SunkanSlabHelper.listOfDoorWall_SunkanSlabId.Clear();

            SlabJointHelper.listOfSlabJointWallLineId.Clear();
            SlabJointHelper.listOfSlabJointOffsetWallLineId.Clear();
            SlabJointHelper.listOfSlabJointHeight.Clear();
            SlabJointHelper.listOfSlabJointElev.Clear();

            KickerHelper.listOfKickerWallLineId.Clear();
            KickerHelper.listOfKickerOffsetWallLineId.Clear();
            KickerHelper.listOfKickerHeight.Clear();

            listOfSelectedParapetWall_ObjId.Clear();
            ParapetHelper.listOfParapetId_WithWallLine.Clear();
            ParapetHelper.listOfWallLineId_InsctToParapet.Clear();
            ParapetHelper.listOfParapetId_WithWindowWallLine.Clear();
            ParapetHelper.listOfWindowWallLineId_InsctToParapet.Clear();
            ParapetHelper.listOfParapetId_WithDoorWallLine.Clear();
            ParapetHelper.listOfDoorWallLineId_InsctToParapet.Clear();

            ParapetHelper.listOfCornerId_InsctToTwoParapet.Clear();
            ParapetHelper.listOfListOfTwoParapetId_InsctToCommonCorner.Clear();
            ParapetHelper.listOfAllCornersId_InsctToParapet.Clear();
            ParapetHelper.listOfParapetId_InsctCorner.Clear();
            ParapetHelper.listOfSunkandId_AvailableInParapet_InsctCorner.Clear();
            ParapetHelper.listOfWallLineId_AvailableInParapet_InsctCorner.Clear();
            ParapetHelper.listOfTwoParapetId_InsctToCommonCorner_ForCheck.Clear();

            ParapetHelper.listOfParapetWallLineId_WithDoor.Clear();
            ParapetHelper.listOfParapetWallOffsetLineId_WithDoor.Clear();
            ParapetHelper.listOfParapetWallLineCornerId_WithDoor.Clear();
            CornerHelper.listOfAllCornerId_ForBOM.Clear();
            CornerHelper.listOfAllCornerText_ForBOM.Clear();

            BOMHelper bomHlp = new BOMHelper();
            bomHlp.clearMethodOfBOMHelper();

            DeckPanelHelper.lstMaxAreaRelations.Clear();
            DeckPanelHelper.listOfListOfDeckPanelWallId.Clear();
            DeckPanelHelper.listOfListOfDeckPanelSpanLngth.Clear();
            DeckPanelHelper.listOfListOfIntrvalLengthOfDeckPanel.Clear();
            DeckPanelHelper.listOfListOfDeckPlane_SF_Interval.Clear();
            // Cuong - 23/12/2022 - DeckPanel issue - START
            DeckPanelHelper.dicRelationInfo.Clear();
            DeckPanelHelper.listOfListOfRelationInfo.Clear();
            // Cuong - 23/12/2022 - DeckPanel issue - END

            SlabJointHelper.listOfNearestBeamLineId_ForSlabJoint.Clear();
            SlabJointHelper.listOfDistBtwBeamToWall_ForSlabJoint.Clear();
            SlabJointHelper.listOfWallId_ForSlabJoint.Clear();
        }

        public void selectPanelLayoutWithWindowSelection(string promptSelMsg)
        {
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = acDoc.Editor;
            Database db = HostApplicationServices.WorkingDatabase;
           
            checkPlanDocumentName = acDoc.Name;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                PromptSelectionResult acSSPrompt = default(PromptSelectionResult);
                PromptSelectionOptions acKeywordOpts = new PromptSelectionOptions();

                acKeywordOpts.MessageForAdding = (promptSelMsg);

                TypedValue[] filterlist = new TypedValue[1];
                filterlist.SetValue(new TypedValue((int)DxfCode.Start, "LWPOLYLINE"), 0);
                SelectionFilter filter = new SelectionFilter(filterlist);
                acSSPrompt = ed.GetSelection(acKeywordOpts, filter);

                //acSSPrompt = ed.GetSelection(acKeywordOpts);

                if (acSSPrompt.Status == PromptStatus.OK)
                {
                    // Start a transaction to open the selected objects
                    SelectionSet acSSet = acSSPrompt.Value;
                    // Iterate through the selections set
                    foreach (SelectedObject so in acSSet)
                    {
                        Entity ent = tr.GetObject(so.ObjectId, OpenMode.ForWrite) as Entity;
                        ObjectId id = so.ObjectId;
                        if (!listOfAllSelectedObjectIds.Contains(id))
                        {
                            listOfAllSelectedObjectIds.Add(id);
                        }
                    }
                }
                else if (acSSPrompt.Status == PromptStatus.Cancel)
                {
                    escBtnCount++;
                }
                else if (acSSPrompt.Status == PromptStatus.Error)
                {
                    enterBtnCount++;
                }

                ed.WriteMessage("\n\n");

                tr.Commit();
            }
        }
       
       
        private void setAllObjectIdFromLayer(List<ObjectId> listOfAllSelectObjId)
        {
            string errrMsg = "Selected polyline is not closed";

            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acDb = doc.Database;
            Editor ed = doc.Editor;
            using (Transaction acTrans = acDb.TransactionManager.StartTransaction())
            {
                foreach (var id in listOfAllSelectObjId)
                {
                    if (id.IsValid == true)
                    {
                        Entity ent = acTrans.GetObject(id, OpenMode.ForRead) as Entity;
                        if (ent is Polyline)
                        {
                            bool flagOfClosedPolyline = true;
                            Polyline acPoly = ent as Polyline;

                            if (InternalWallSlab_UI_Helper.IsInternalWall(ent.Layer))
                            {
                                if (acPoly.Closed == true)
                                {
                                    checkCollinearLine(id);
                                    listOfSelectedInternalWall_ObjId.Add(id);
                                    slabJointCount_ForLayoutCreator++;
                                }
                                else
                                {
                                    flagOfClosedPolyline = false;
                                }
                            }
                            else if (ent.Layer == CommonModule.externalWallLayerName)
                            {
                                if (acPoly.Closed == true)
                                {
                                    checkCollinearLine(id);
                                    listOfSelectedExternalWall_ObjId.Add(id);
                                    kickerCount_ForLayoutCreator++;
                                }
                                else
                                {
                                    flagOfClosedPolyline = false;
                                }
                            }
                            else if (Door_UI_Helper.checkDoorLayerIsExist(ent.Layer))
                            {
                                if (acPoly.Closed == true)
                                {
                                    checkCollinearLine(id);
                                    listOfSelectedDoor_ObjId.Add(id);
                                    doorWindowCount_ForLayoutCreator++;
                                }
                                else
                                {
                                    flagOfClosedPolyline = false;
                                }
                            }
                            else if (Beam_UI_Helper.checkBeamLayerIsExist(ent.Layer))
                            {
                                if (acPoly.Closed == true)
                                {
                                    checkCollinearLine(id);
                                    listOfSelectedOffsetBeam_ObjId.Add(id);
                                    //listOfSelectedDoor_ObjId.Add(id);
                                    //doorWindowCount_ForLayoutCreator++;
                                }
                                else
                                {
                                    flagOfClosedPolyline = false;
                                }
                            }
                            else if (Window_UI_Helper.checkWindowLayerIsExist(ent.Layer))
                            {
                                if (acPoly.Closed == true)
                                {
                                    checkCollinearLine(id);
                                    listOfSelectedWindow_ObjId.Add(id);
                                    doorWindowCount_ForLayoutCreator++;
                                }
                                else
                                {
                                    flagOfClosedPolyline = false;
                                }
                            }
                            else if (Window_UI_Helper.checkChajjaLayerIsExist(ent.Layer))
                            {
                                if (acPoly.Closed == true)
                                {
                                    checkCollinearLine(id);

                                    listOfSelectedChajja_ObjId.Add(id);
                                    // doorWindowCount_ForLayoutCreator++;
                                }
                                else
                                {
                                    flagOfClosedPolyline = false;
                                }
                            }

                            else if (Beam_UI_Helper.checkOffsetBeamLayerIsExist(ent.Layer))
                            {
                                if (acPoly.Closed == true)
                                {
                                    checkCollinearLine(id);
                                    listOfSelectedOffsetBeam_ObjId.Add(id);
                                }
                                else
                                {
                                    flagOfClosedPolyline = false;
                                }
                            }
                            else if (ent.Layer.Contains(CommonModule.ductLayerName))
                            {
                                if (acPoly.Closed == true)
                                {
                                    checkCollinearLine(id);
                                    listOfSelectedDuct_ObjId.Add(id);
                                    kickerCount_ForLayoutCreator++;
                                }
                                else
                                {
                                    flagOfClosedPolyline = false;
                                }
                            }
                            else if (StairCase_UI_Helper.checkStairCaseLayerIsExist(ent.Layer))
                            {
                                if (acPoly.Closed == true)
                                {
                                    checkCollinearLine(id);
                                    listOfSelectedStaircase_ObjId.Add(id);
                                }
                                else
                                {
                                    flagOfClosedPolyline = false;
                                }
                            }
                            else if (Lift_UI_Helper.checkLiftLayerIsExist(ent.Layer))
                            {
                                if (acPoly.Closed == true)
                                {
                                    checkCollinearLine(id);
                                    listOfSelectedLift_ObjId.Add(id);
                                    kickerCount_ForLayoutCreator++;
                                }
                                else
                                {
                                    flagOfClosedPolyline = false;
                                }
                            }
                            else if (InternalSunkanSlab_UI_Helper.checkSunkanSlabLayerIsExist(ent.Layer))
                            {
                                if (acPoly.Closed == true)
                                {
                                    checkCollinearLine(id);
                                    listOfSelectedSunkanSlab_ObjId.Add(id);

                                    var internalWallId = AEE_Utility.createColonEntityInSamePoint(ent, false, InternalWallSlab_UI_Helper.defaulLayerName, 1);
                                    listOfSelectedInternalWall_ObjId.Add(internalWallId);
                                    slabJointCount_ForLayoutCreator++;
                                    kickerCount_ForLayoutCreator++;
                                }
                                else
                                {
                                    flagOfClosedPolyline = false;
                                }
                            }
                            else if (ExternalSunkanSlab_UI_Helper.checkExternalSunkanSlabLayerIsExist(ent.Layer))
                            {
                                if (acPoly.Closed == true)
                                {
                                    checkCollinearLine(id);
                                    listOfSelectedSunkanSlab_ObjId.Add(id);

                                    var internalWallId = AEE_Utility.createColonEntityInSamePoint(ent, false, InternalWallSlab_UI_Helper.defaulLayerName, 1);
                                    listOfSelectedInternalWall_ObjId.Add(internalWallId);

                                    kickerCount_ForLayoutCreator++;

                                    //var externalWallId = AEE_Utility.createColonEntityInSamePoint(ent, false, CommonModule.externalWallLayerName, 1);
                                    //listOfSelectedExternalWall_ObjId.Add(externalWallId);
                                }
                                else
                                {
                                    flagOfClosedPolyline = false;
                                }
                            }
                            else if (Parapet_UI_Helper.checkParapetLayerIsExist(ent.Layer))
                            {
                                if (acPoly.Closed == true)
                                {
                                    checkCollinearLine(id);
                                    listOfSelectedParapetWall_ObjId.Add(id);
                                }
                                else
                                {
                                    flagOfClosedPolyline = false;
                                }
                            }
                            if (flagOfClosedPolyline == false)
                            {
                                ent.Highlight();
                                MessageBox.Show(errrMsg, CommonModule.headerText_messageBox, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
                acTrans.Commit();
            }
        }
        private void checkCollinearLine(ObjectId id)
        {
            var listOfExplodeIds = AEE_Utility.ExplodeEntity(id);
            var listOfPolylineVertex = AEE_Utility.GetPolylineVertexPoint(id);
            Point2d startPoint = new Point2d();
            int endLoopIndex = 5; // temporary value assign. set the value from out keyword
            List<Point2d> listOfCollinearPolylineVertex = new List<Point2d>();
            for (int index = 0; index < endLoopIndex; index++)
            {
                ObjectId crrntLine_Id = listOfExplodeIds[index];
                var listOfCrrntLineStrtEndPnt = AEE_Utility.getStartEndPointWithAngle_Line(crrntLine_Id);
                double currntLineAngle = listOfCrrntLineStrtEndPnt[4];

                if (index == (endLoopIndex - 1))
                {
                    break;
                }

                if (index == 0)
                {
                    checkPreviousVertexAngle(listOfCrrntLineStrtEndPnt, listOfExplodeIds, out startPoint, out endLoopIndex);
                    if (!listOfCollinearPolylineVertex.Contains(startPoint))
                    {
                        listOfCollinearPolylineVertex.Add(startPoint);
                    }
                }

                Point2d endPoint = new Point2d();
                int loopCount = 0;
                int nextIndex = index + 1;
                ObjectId nextLine_Id = listOfExplodeIds[nextIndex];
                checkNextVertexAngle(listOfCrrntLineStrtEndPnt, listOfExplodeIds, nextIndex, out endPoint, out loopCount);

                if (!listOfCollinearPolylineVertex.Contains(endPoint))
                {
                    listOfCollinearPolylineVertex.Add(endPoint);
                }

                index = index + loopCount;
            }

            if (listOfCollinearPolylineVertex.Count != listOfPolylineVertex.Count)
            {
                changeTheVertexOfPolyline(id, listOfCollinearPolylineVertex);
            }

            AEE_Utility.deleteEntity(listOfExplodeIds);
        }
        private void changeTheVertexOfPolyline(ObjectId id, List<Point2d> listOfPolylineVertex)
        {
            try
            {
                Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                Database acDb = acDoc.Database;
                using (Transaction acTrans = acDb.TransactionManager.StartTransaction())
                {
                    Entity ent = acTrans.GetObject(id, OpenMode.ForWrite) as Entity;
                    Polyline acPoly = ent as Polyline;
                    int numberOfVertex = acPoly.NumberOfVertices;
                    for (int i = (numberOfVertex - 1); i > 0; i--)
                    {
                        acPoly.RemoveVertexAt(i);
                    }
                    for (int j = 0; j < listOfPolylineVertex.Count; j++)
                    {
                        Point2d vertexPoint = listOfPolylineVertex[j];
                        acPoly.AddVertexAt(j, vertexPoint, 0, 0, 0);
                    }

                    numberOfVertex = acPoly.NumberOfVertices;
                    acPoly.RemoveVertexAt(numberOfVertex - 1);

                    acPoly.UpgradeOpen();
                    acTrans.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
        }
        private void checkPreviousVertexAngle(List<double> listOfCrrntLineStrtEndPnt, List<ObjectId> listOfExplodeIds, out Point2d startPoint, out int endLoopIndex)
        {
            endLoopIndex = 0;
            Point2d crrntLineStartPoint = new Point2d(listOfCrrntLineStrtEndPnt[0], listOfCrrntLineStrtEndPnt[1]);
            Point2d crrntLineEndPoint = new Point2d(listOfCrrntLineStrtEndPnt[2], listOfCrrntLineStrtEndPnt[3]);

            startPoint = new Point2d(listOfCrrntLineStrtEndPnt[0], listOfCrrntLineStrtEndPnt[1]);
            double firstLineAngle = listOfCrrntLineStrtEndPnt[4];

            int count = listOfExplodeIds.Count();
            for (int index = (count - 1); index >= 0; index--)
            {
                ObjectId prvsLine_Id = listOfExplodeIds[index];
                var listOfPrvsLineStrtEndPnt = AEE_Utility.getStartEndPointWithAngle_Line(prvsLine_Id);
                Point2d prvsLineStartPoint = new Point2d(listOfPrvsLineStrtEndPnt[0], listOfPrvsLineStrtEndPnt[1]);
                Point2d prvsLineEndPoint = new Point2d(listOfPrvsLineStrtEndPnt[2], listOfPrvsLineStrtEndPnt[3]);

                double prvsLineAngle = listOfPrvsLineStrtEndPnt[4];
                if (prvsLineAngle == firstLineAngle)
                {
                    startPoint = new Point2d(listOfPrvsLineStrtEndPnt[0], listOfPrvsLineStrtEndPnt[1]);
                }
                else
                {
                    endLoopIndex = (index + 1);
                    break;
                }
            }
        }
        private void checkNextVertexAngle(List<double> listOfCrrntLineStrtEndPnt, List<ObjectId> listOfExplodeIds, int index, out Point2d endPoint, out int loopCount)
        {
            loopCount = 0;

            Point2d crrntLineStartPoint = new Point2d(listOfCrrntLineStrtEndPnt[0], listOfCrrntLineStrtEndPnt[1]);
            Point2d crrntLineEndPoint = new Point2d(listOfCrrntLineStrtEndPnt[2], listOfCrrntLineStrtEndPnt[3]);

            endPoint = new Point2d(listOfCrrntLineStrtEndPnt[2], listOfCrrntLineStrtEndPnt[3]);
            double firstLineAngle = listOfCrrntLineStrtEndPnt[4];

            int count = listOfExplodeIds.Count();

            for (; index < listOfExplodeIds.Count; index++)
            {
                ObjectId nextLine_Id = listOfExplodeIds[index];
                var listOfNextLineStrtEndPnt = AEE_Utility.getStartEndPointWithAngle_Line(nextLine_Id);
                Point2d nextLineStartPoint = new Point2d(listOfNextLineStrtEndPnt[0], listOfNextLineStrtEndPnt[1]);
                Point2d nextLineEndPoint = new Point2d(listOfNextLineStrtEndPnt[2], listOfNextLineStrtEndPnt[3]);

                double nextLineAngle = listOfNextLineStrtEndPnt[4];
                if (nextLineAngle == firstLineAngle)
                {
                    loopCount++;
                    endPoint = new Point2d(listOfNextLineStrtEndPnt[2], listOfNextLineStrtEndPnt[3]);
                }
                else
                {
                    break;
                }
            }


        }


        private void checkShellPlan_Is_Perpendicular()
        {
            string progressBarMsg = "";
            ProgressForm progressForm = new ProgressForm();
            progressForm.Show();

            for (int index = 0; index < listOfSelectedInternalWall_ObjId.Count; index++)
            {
                ObjectId objId = listOfSelectedInternalWall_ObjId[index];
                checkCorner_Is_RightAngle(objId);

                if ((index % 5) == 0)
                {
                    progressForm.ReportProgress(1, progressBarMsg);
                }
            }

            for (int index = 0; index < listOfSelectedExternalWall_ObjId.Count; index++)
            {
                ObjectId objId = listOfSelectedExternalWall_ObjId[index];
                checkCorner_Is_RightAngle(objId);

                if ((index % 5) == 0)
                {
                    progressForm.ReportProgress(1, progressBarMsg);
                }
            }

            for (int index = 0; index < listOfSelectedDoor_ObjId.Count; index++)
            {
                ObjectId objId = listOfSelectedDoor_ObjId[index];
                checkCorner_Is_RightAngle(objId);

                if ((index % 5) == 0)
                {
                    progressForm.ReportProgress(1, progressBarMsg);
                }
            }
            //Added on 12/03/2023 by SDM
            for (int index = 0; index < listOfSelectedBeam_ObjId.Count; index++)
            {
                ObjectId objId = listOfSelectedBeam_ObjId[index];
                checkCorner_Is_RightAngle(objId);

                if ((index % 5) == 0)
                {
                    progressForm.ReportProgress(1, progressBarMsg);
                }
            }

            for (int index = 0; index < listOfSelectedWindow_ObjId.Count; index++)
            {
                ObjectId objId = listOfSelectedWindow_ObjId[index];
                checkCorner_Is_RightAngle(objId);

                if ((index % 5) == 0)
                {
                    progressForm.ReportProgress(1, progressBarMsg);
                }
            }

            for (int index = 0; index < listOfSelectedOffsetBeam_ObjId.Count; index++)
            {
                ObjectId objId = listOfSelectedOffsetBeam_ObjId[index];
                checkCorner_Is_RightAngle(objId);

                if ((index % 5) == 0)
                {
                    progressForm.ReportProgress(1, progressBarMsg);
                }
            }

            for (int index = 0; index < listOfSelectedDuct_ObjId.Count; index++)
            {
                ObjectId objId = listOfSelectedDuct_ObjId[index];
                checkCorner_Is_RightAngle(objId);

                if ((index % 5) == 0)
                {
                    progressForm.ReportProgress(1, progressBarMsg);
                }
            }

            for (int index = 0; index < listOfSelectedStaircase_ObjId.Count; index++)
            {
                ObjectId objId = listOfSelectedStaircase_ObjId[index];
                checkCorner_Is_RightAngle(objId);

                if ((index % 5) == 0)
                {
                    progressForm.ReportProgress(1, progressBarMsg);
                }
            }

            for (int index = 0; index < listOfSelectedLift_ObjId.Count; index++)
            {
                ObjectId objId = listOfSelectedLift_ObjId[index];
                checkCorner_Is_RightAngle(objId);

                if ((index % 5) == 0)
                {
                    progressForm.ReportProgress(1, progressBarMsg);
                }
            }

            for (int index = 0; index < listOfSelectedSunkanSlab_ObjId.Count; index++)
            {
                ObjectId objId = listOfSelectedSunkanSlab_ObjId[index];
                checkCorner_Is_RightAngle(objId);

                if ((index % 5) == 0)
                {
                    progressForm.ReportProgress(1, progressBarMsg);
                }
            }

            for (int index = 0; index < listOfSelectedExternalSunkanSlab_ObjId.Count; index++)
            {
                ObjectId objId = listOfSelectedExternalSunkanSlab_ObjId[index];
                checkCorner_Is_RightAngle(objId);

                if ((index % 5) == 0)
                {
                    progressForm.ReportProgress(1, progressBarMsg);
                }
            }

            for (int index = 0; index < listOfSelectedParapetWall_ObjId.Count; index++)
            {
                ObjectId objId = listOfSelectedParapetWall_ObjId[index];
                checkCorner_Is_RightAngle(objId);

                if ((index % 5) == 0)
                {
                    progressForm.ReportProgress(1, progressBarMsg);
                }
            }

            progressForm.ReportProgress(100, progressBarMsg);
            progressForm.Close();
        }

        public void checkCorner_Is_RightAngle(ObjectId id)
        {
            List<Point2d> listOfNewVertexPoint = new List<Point2d>();
            CommonModule commonModule_Obj = new CommonModule();
            var listOfExplodeIds = AEE_Utility.ExplodeEntity(id);
            for (int index = 0; index < listOfExplodeIds.Count; index++)
            {
                ObjectId currntLineId = listOfExplodeIds[index];
                double lengthOfCurrntLine = AEE_Utility.GetLengthOfLine(currntLineId);
                List<double> listOfCrrntLine_Points = AEE_Utility.getStartEndPointWithAngle_Line(currntLineId);
                Point2d startPoint = new Point2d(listOfCrrntLine_Points[0], listOfCrrntLine_Points[1]);
                double currntAngle = Math.Round(listOfCrrntLine_Points[4], 7);

                var flg = commonModule_Obj.checkAngleBetweenTwoLines(index, currntAngle, listOfExplodeIds);
                if (flg == false)
                {
                    var circleId = AEE_Utility.CreateCircle(startPoint.X, startPoint.Y, 0, (CommonModule.intrnlCornr_Flange1 / 2), CommonModule.internalCornerLyrName, 1);
                    AEE_Utility.changeColor(circleId, InternalWallHelper.nonStandardColorIndex);
                    listOfAllNonPerpendicularPoint_Circle_ObjId.Add(circleId);

                    listOfSelectedNonPerpendiculareLine.Add(currntLineId);
                }

                CreateShellPlanHelper.listOfAllVerticesPoint.Add(startPoint);
            }
            AEE_Utility.deleteEntity(listOfExplodeIds);
        }


        public bool checkShellPlanCommand_DocumentIsSame()
        {
            bool flag = true;
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = acDoc.Editor;
            Database db = HostApplicationServices.WorkingDatabase;
            string currentDocName = acDoc.Name;
            if (checkPlanDocumentName != currentDocName)
            {
                string message = "“Check shell plan” command was run on other document in this session; do you want to proceed with clearing the previously captured data? ";
                var dr = MessageBox.Show(message, CommonModule.headerText_messageBox, MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (dr == DialogResult.Yes)
                {
                    AEE_Utility.RemoveXDataName(CommonModule.xDataAsciiName);

                    RibbonHelper.panelLayoutOptionButton.IsEnabled = true;
                    RibbonHelper.checkShellPlanButton.IsEnabled = true;
                    RibbonHelper.createShellPlanButton.IsEnabled = false;
                    RibbonHelper.insertCornerButton.IsEnabled = false;
                    RibbonHelper.insertPanelButton.IsEnabled = false;
                    RibbonHelper.insertDeckPanelButton.IsEnabled = false;
                    RibbonHelper.createElevationPlanButton.IsEnabled = false;
                    RibbonHelper.createHolecenterlineButton.IsEnabled = false;
                    RibbonHelper.rebuildPanelButton.IsEnabled = false;
                    RibbonHelper.updateWallButton.IsEnabled = false;//Added on 10/06/2023 by SDM
                    RibbonHelper.createBOMButton.IsEnabled = false;
                }
                flag = false;
            }
            return flag;
        }
    }
}
