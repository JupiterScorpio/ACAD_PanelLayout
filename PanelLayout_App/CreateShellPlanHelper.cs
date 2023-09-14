using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using PanelLayout_App.CivilModel;
using PanelLayout_App.WallModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PanelLayout_App
{
    public class CreateShellPlanHelper
    {
        public static double start_X_NewLayout = 0;
        public static double start_Y_NewLayout = 0;

        int oldInternalWallColorIndex = 9;
        int oldExternalWallColorIndex = 8;

        private static double angleOf_X_Direction = double.NaN;
        private static double angleOf_Y_Direction = double.NaN;
        private static double remainingAngle = double.NaN;
        private static bool flagOfDeleteOld_ShellPlan = false;

        public static List<Point2d> listOfAllVerticesPoint = new List<Point2d>();
        public static List<Vector3d> listOfCopyVector = new List<Vector3d>();
        List<string> listOfAllLayoutName = new List<string>();

        public static Vector3d moveVector_WallPanelLayout = new Vector3d();
        public static Vector3d moveVector_ForWindowDoorLayout = new Vector3d();
        public static Vector3d moveVector_ForBeamLayout = new Vector3d();
        public static Vector3d moveVector_ForBeamBottmLayout = new Vector3d();
        public static Vector3d moveVector_ForSunkanSlabLayout = new Vector3d();
        public static Vector3d moveVector_ForSlabJointLayout = new Vector3d();
        public static Vector3d moveVector_ForSoffitLayout = new Vector3d();
        public static Vector3d moveVector_ForDeckPanelLayout = new Vector3d();

        double lengthOfShellPlan = 0;
        double heightOfShellPlan = 0;
        double gapBetweenTwoShellPlan = 0;
        public void commandMethodOfCreateShellPlan()
        {
            listOfAllLayoutName.Clear();
            moveVector_WallPanelLayout = new Vector3d();
            moveVector_ForWindowDoorLayout = new Vector3d();
            moveVector_ForBeamLayout = new Vector3d();
            moveVector_ForSoffitLayout = new Vector3d();
            moveVector_ForDeckPanelLayout = new Vector3d();
            moveVector_ForSunkanSlabLayout = new Vector3d();
            moveVector_ForBeamBottmLayout = new Vector3d();

            listOfCopyVector.Clear();     
            flagOfDeleteOld_ShellPlan = false;

            AEE_Utility.deleteEntity(CheckShellPlanHelper.listOfAllNonPerpendicularPoint_Circle_ObjId);

            if (CheckShellPlanHelper.listOfAllSelectedObjectIds.Count != 0)
            {
                RibbonHelper.createShellPlanButton.IsEnabled = false;

                var flagOfNewPlan = setNewLyoutPickPoint();
                if (flagOfNewPlan == false)
                {
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
                    return;
                }
                change_Z_Axis_In_Polyline(CheckShellPlanHelper.listOfAllSelectedObjectIds);

                setAllNewLayoutPoint();

                flagOfDeleteOld_ShellPlan = true;

                string progressBarMsg = "Shell plans are creating...";
                ProgressForm progressForm = new ProgressForm();
                progressForm.Show();

                if (CheckShellPlanHelper.listOfSelectedWindow_ObjId.Count != 0)
                {
                    List<ObjectId> listOfNewWindowId = drawNewWindowAndDoor_In_RightAngleCorner(progressForm, progressBarMsg, CheckShellPlanHelper.listOfSelectedWindow_ObjId);
                    CheckShellPlanHelper.listOfSelectedWindow_ObjId.Clear();
                    foreach (var newWindowId in listOfNewWindowId)
                    {
                        CheckShellPlanHelper.listOfSelectedWindow_ObjId.Add(newWindowId);
                    }
                }

                if (CheckShellPlanHelper.listOfSelectedDoor_ObjId.Count != 0)
                {
                    List<ObjectId> listOfNewDoorId = drawNewWindowAndDoor_In_RightAngleCorner(progressForm, progressBarMsg, CheckShellPlanHelper.listOfSelectedDoor_ObjId);
                    CheckShellPlanHelper.listOfSelectedDoor_ObjId.Clear();
                    foreach (var newDoorId in listOfNewDoorId)
                    {
                        CheckShellPlanHelper.listOfSelectedDoor_ObjId.Add(newDoorId);
                    }
                }
                //Added on 12/03/2023 by SDM
                if (CheckShellPlanHelper.listOfSelectedBeam_ObjId.Count != 0)
                {
                    List<ObjectId> listOfNewBeamId = drawNewWindowAndDoor_In_RightAngleCorner(progressForm, progressBarMsg, CheckShellPlanHelper.listOfSelectedBeam_ObjId);
                    CheckShellPlanHelper.listOfSelectedBeam_ObjId.Clear();
                    foreach (var newBeamId in listOfNewBeamId)
                    {
                        CheckShellPlanHelper.listOfSelectedBeam_ObjId.Add(newBeamId);
                    }
                }
                if (CheckShellPlanHelper.listOfSelectedOffsetBeam_ObjId.Count != 0)
                {
                    List<ObjectId> listOfNewBeamId = drawNewWindowAndDoor_In_RightAngleCorner(progressForm, progressBarMsg, CheckShellPlanHelper.listOfSelectedOffsetBeam_ObjId);
                    CheckShellPlanHelper.listOfSelectedOffsetBeam_ObjId.Clear();
                    foreach (var newBeamId in listOfNewBeamId)
                    {
                        CheckShellPlanHelper.listOfSelectedOffsetBeam_ObjId.Add(newBeamId);
                    }
                }

                progressForm.ReportProgress(1, progressBarMsg);
                InternalWallHelper.listOfInternalWallObjId = drawNewWalls(CheckShellPlanHelper.listOfSelectedInternalWall_ObjId);

                progressForm.ReportProgress(1, progressBarMsg);
                ExternalWallHelper.listOfExternalWallObjId = drawNewWalls(CheckShellPlanHelper.listOfSelectedExternalWall_ObjId);

                progressForm.ReportProgress(1, progressBarMsg);
                DuctHelper.listOfDuctObjId = drawNewWalls(CheckShellPlanHelper.listOfSelectedDuct_ObjId);

                progressForm.ReportProgress(1, progressBarMsg);
                StairCaseHelper.listOfStairCaseObjId = drawNewWalls(CheckShellPlanHelper.listOfSelectedStaircase_ObjId);

                progressForm.ReportProgress(1, progressBarMsg);
                LiftHelper.listOfLiftObjId = drawNewWalls(CheckShellPlanHelper.listOfSelectedLift_ObjId);

                progressForm.ReportProgress(1, progressBarMsg);
                SunkanSlabHelper.listOfSunkanSlab_ObjId = drawNewWalls(CheckShellPlanHelper.listOfSelectedSunkanSlab_ObjId);


                progressForm.ReportProgress(1, progressBarMsg);
                ParapetHelper.listOfParapetObjId = drawNewWalls(CheckShellPlanHelper.listOfSelectedParapetWall_ObjId);

                WindowHelper windowObj = new WindowHelper();
                windowObj.createNewWindowAsPerNewShellPlan(progressForm, progressBarMsg);

                DoorHelper doorHlpr = new DoorHelper();
                doorHlpr.createNewDoorAsPerNewShellPlan(progressForm, progressBarMsg);

                drawNewLayout_With_RoomIndex(progressForm, progressBarMsg, listOfCopyVector);

                string creatorMsg = "New shell plans is created.";

                progressForm.ReportProgress(100, creatorMsg);
                progressForm.Close();

                if (!CommandModule.dply)
                    MessageBox.Show(creatorMsg, CommonModule.headerText_messageBox, MessageBoxButtons.OK, MessageBoxIcon.Information);

                //Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.SendStringToExecute((String)RibbonHelper.insertCornerButton.CommandParameter, true, false, true);
             
                RibbonHelper.insertCornerButton.IsEnabled = true;           
            }
        }

        private List<ObjectId> GetInternalIds(List<ObjectId> listOfAllSelectObjId)
        {
            List<ObjectId> lst = new List<ObjectId>();;

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
                            Polyline acPoly = ent as Polyline;
                            
                            if (InternalSunkanSlab_UI_Helper.checkSunkanSlabLayerIsExist(ent.Layer) ||
                                    ExternalSunkanSlab_UI_Helper.checkExternalSunkanSlabLayerIsExist(ent.Layer))
                            {
                                if (acPoly.Closed == true)
                                {
                                    lst.Add(id);
                                }
                            }
                        }
                    }
                }
                acTrans.Abort();
            }
            return lst;
        }

        private List<ObjectId> drawNewWalls(List<ObjectId> listOfObjId)
        {
            List<ObjectId> listOfNewObjId = new List<ObjectId>();

            foreach (var objId in listOfObjId)
            {
                Entity acEntity = AEE_Utility.GetEntityForWrite(objId);
                
                ObjectId newShellPlan_ObjId = drawRightAnglePolyline(acEntity, objId);          

                listOfNewObjId.Add(newShellPlan_ObjId);

                if (flagOfDeleteOld_ShellPlan == true)
                {
                    AEE_Utility.deleteEntity(objId);
                }
                else
                {
                    //AEE_Utility.changeColor(objId, oldInternalWallColorIndex);
                }
            }
            return listOfNewObjId;
        }

        private ObjectId drawRightAnglePolyline(Entity ent, ObjectId id)
        {
            angleOf_X_Direction = double.NaN;
            angleOf_Y_Direction = double.NaN;
            remainingAngle = double.NaN;

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

                double outputAngle = 0;

                var flg = commonModule_Obj.checkAngleBetweenTwoLines(index, currntAngle, listOfExplodeIds);
                if (flg == false)
                {
                    //var circleId = AEE_Utility.CreateCircle(startPoint.X, startPoint.Y, 0, (CommonModule.intrnlCornr_Flange1 / 2), CommonModule.internalCornerLyrName, 1);
                    //AEE_Utility.changeColor(circleId, InternalWallHelper.nonStandardColorIndex);                              
                }

                var flagOfAngleBtw = checkAngleBetweenTwoLines_WithIntervalAngle(index, currntAngle, listOfExplodeIds);
                if (flagOfAngleBtw == true)
                {
                    setAngleOfLine(index, currntAngle, out outputAngle);
                }
                else
                {
                    angleOf_X_Direction = double.NaN;
                    angleOf_Y_Direction = double.NaN;
                    remainingAngle = double.NaN;

                    outputAngle = currntAngle;

                    //var circleId = AEE_Utility.CreateCircle(startPoint.X, startPoint.Y, 0, (CommonModule.intrnlCornr_Flange1 / 2), CommonModule.internalCornerLyrName, 1);
                    //AEE_Utility.changeColor(circleId, InternalWallHelper.nonStandardColorIndex);
                }

                if (index == 0)
                {
                    listOfNewVertexPoint.Add(startPoint);
                }
                else
                {
                    startPoint = listOfNewVertexPoint[(listOfNewVertexPoint.Count - 1)];
                }
                var point = AEE_Utility.get_XY(outputAngle, lengthOfCurrntLine);
                Point2d endPoint = new Point2d((startPoint.X + point.X), (startPoint.Y + point.Y));
                listOfNewVertexPoint.Add(endPoint);

                //AEE_Utility.CreateLine(startPoint.X, startPoint.Y, 0, endPoint.X, endPoint.Y, 0, "A", 3);
            }
            AEE_Utility.deleteEntity(listOfExplodeIds);

            listOfNewVertexPoint = filletFirstAndLastLine(listOfNewVertexPoint);
            ObjectId newShellPlan_ObjId = drawPerpendicularNewWall(listOfNewVertexPoint, ent);
            return newShellPlan_ObjId;
        }
        private void setAngleOfLine(int index, double angleOfLine, out double outputAngle)
        {
            outputAngle = 0;

            if (angleOfLine == CommonModule.angle_360)
            {
                angleOfLine = CommonModule.angle_0;
            }

            bool flagOf_X_Dirction = false;
            bool flagOf_Y_Dirction = false;

            if (angleOfLine >= CommonModule.angle_0 && angleOfLine < (CommonModule.angle_90 - CommonModule.intervalOfAngle))
            {
                outputAngle = CommonModule.angle_0;
                flagOf_X_Dirction = true;
            }
            else if (angleOfLine >= (CommonModule.angle_90 - CommonModule.intervalOfAngle) && angleOfLine < (CommonModule.angle_180 - CommonModule.intervalOfAngle))
            {
                outputAngle = CommonModule.angle_90;
                flagOf_Y_Dirction = true;
            }
            else if (angleOfLine >= (CommonModule.angle_180 - CommonModule.intervalOfAngle) && angleOfLine < (CommonModule.angle_270 - CommonModule.intervalOfAngle))
            {
                outputAngle = CommonModule.angle_180;
                flagOf_X_Dirction = true;
            }
            else if (angleOfLine >= (CommonModule.angle_270 - CommonModule.intervalOfAngle) && angleOfLine < (CommonModule.angle_360 - CommonModule.intervalOfAngle))
            {
                outputAngle = CommonModule.angle_270;
                flagOf_Y_Dirction = true;
            }
            else if (angleOfLine >= (CommonModule.angle_360 - CommonModule.intervalOfAngle) && angleOfLine <= CommonModule.angle_360)
            {
                outputAngle = CommonModule.angle_360;
                flagOf_X_Dirction = true;
            }

            if (flagOf_X_Dirction == true)
            {
                if (double.IsNaN(angleOf_X_Direction))
                {
                    angleOf_X_Direction = outputAngle;
                }
                if (!double.IsNaN(angleOf_X_Direction) && double.IsNaN(remainingAngle))
                {
                    remainingAngle = angleOfLine - Convert.ToDouble(angleOf_X_Direction);
                }

                outputAngle = outputAngle + remainingAngle;
            }

            if (flagOf_Y_Dirction == true)
            {
                if (double.IsNaN(angleOf_Y_Direction))
                {
                    angleOf_Y_Direction = outputAngle;
                }
                if (!double.IsNaN(angleOf_Y_Direction) && double.IsNaN(remainingAngle))
                {
                    remainingAngle = angleOfLine - Convert.ToDouble(angleOf_Y_Direction);
                }
                outputAngle = outputAngle + remainingAngle;
            }
        }


        private ObjectId drawPerpendicularNewWall(List<Point2d> listOfNewVertexPoint, Entity ent)
        {
            ObjectId id = new ObjectId();

            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acDb = acDoc.Database;
            Editor ed = acDoc.Editor;

            using (Transaction acTrans = acDb.TransactionManager.StartTransaction())
            {
                BlockTable blkTbl = acTrans.GetObject(acDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord blkTblRec = acTrans.GetObject(blkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                using (Polyline acPoly = new Polyline())
                {
                    for (int index = 0; index < listOfNewVertexPoint.Count; index++)
                    {
                        Point2d vertexPoint = listOfNewVertexPoint[index];
                        acPoly.AddVertexAt(index, vertexPoint, 0.0, 0.0, 0.0);
                    }
                    acPoly.Closed = true;
                    acPoly.SetPropertiesFrom(ent);
                    acPoly.TransformBy(ed.CurrentUserCoordinateSystem);
                    id = blkTblRec.AppendEntity(acPoly);
                    acTrans.AddNewlyCreatedDBObject(acPoly, true);
                }
                acTrans.Commit();
            }
            return id;
        }
        private List<Point2d> filletFirstAndLastLine(List<Point2d> listOfNewVertexPoint)
        {
            int listOfCount = listOfNewVertexPoint.Count;
            if (listOfCount != 0)
            {
                Point2d firstPoint = listOfNewVertexPoint[0];
                Point2d secondPoint = listOfNewVertexPoint[1];

                Point2d lastPoint = listOfNewVertexPoint[listOfCount - 1];
                Point2d secondLastPoint = listOfNewVertexPoint[listOfCount - 2];

                var id1 = AEE_Utility.getLineId(firstPoint.X, firstPoint.Y, 0, secondPoint.X, secondPoint.Y, 0, false);
                var id2 = AEE_Utility.getLineId(secondLastPoint.X, secondLastPoint.Y, 0, lastPoint.X, lastPoint.Y, 0, false);

                List<Point3d> listOfIntersctionPoint = AEE_Utility.GetIntrsctPntOfBtwnTwoLines_WithExtendBoth(id1, id2);
                if (listOfIntersctionPoint.Count != 0)
                {
                    var intsctPoint = listOfIntersctionPoint[0];
                    Point2d intersectPoint = new Point2d(intsctPoint.X, intsctPoint.Y);
                    listOfNewVertexPoint[0] = intersectPoint;

                    //listOfNewVertexPoint[listOfCount - 1] = intersectPoint;

                    listOfNewVertexPoint.RemoveAt(listOfCount - 1); // last vertex delete, and closed the polyline
                }
                AEE_Utility.deleteEntity(id1);
                AEE_Utility.deleteEntity(id2);
            }

            return listOfNewVertexPoint;
        }
        private bool checkAngleBetweenTwoLines_WithIntervalAngle(int index, double currntAngle, List<ObjectId> listOfExplodeIds)
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
            double prvsAngle = listOfPrvsLine_Points[4];

            double angleBtwTwoLines = Math.Round(Math.Abs(currntAngle - prvsAngle), 9);
            if (angleBtwTwoLines >= (CommonModule.angle_90 - CommonModule.intervalOfAngle) && angleBtwTwoLines <= (CommonModule.angle_90 + CommonModule.intervalOfAngle))
            {
                return true;
            }
            else if (angleBtwTwoLines >= (CommonModule.angle_270 - CommonModule.intervalOfAngle) && angleBtwTwoLines <= (CommonModule.angle_270 + CommonModule.intervalOfAngle))
            {
                return true;
            }
            else
            {
                //ObjectId nextLineId = new ObjectId();
                //if (index == (count - 1))
                //{
                //    nextLineId = listOfExplodeIds[0];
                //}
                //else
                //{
                //    nextLineId = listOfExplodeIds[index + 1];
                //}
                //List<double> listOfNextLine_Points = AEE_Utility.getStartEndPointWithAngle_Line(nextLineId);
                //double nextAngle = listOfNextLine_Points[4];

                //angleBtwTwoLines = Math.Round(Math.Abs(currntAngle - nextAngle), 9);
                //if (angleBtwTwoLines >= (CommonModule.angle_90 - CommonModule.intervalOfAngle) && angleBtwTwoLines <= (CommonModule.angle_90 + CommonModule.intervalOfAngle))
                //{
                //    angleOf_X_Direction = double.NaN;
                //    angleOf_Y_Direction = double.NaN;
                //    remainingAngle = double.NaN;
                //    return true;
                //}
                //else if (angleBtwTwoLines >= (CommonModule.angle_270 - CommonModule.intervalOfAngle) && angleBtwTwoLines <= (CommonModule.angle_270 + CommonModule.intervalOfAngle))
                //{
                //    angleOf_X_Direction = double.NaN;
                //    angleOf_Y_Direction = double.NaN;
                //    remainingAngle = double.NaN;
                //    return true;
                //}
                //else
                {
                    return false;
                }
            }
        }



        private List<ObjectId> drawNewWindowAndDoor_In_RightAngleCorner(ProgressForm progressForm, string progressBarMsg, List<ObjectId> listOfSelectedObjId)
        {
            List<ObjectId> listOfNewWindowId = new List<ObjectId>();

            WindowHelper windowHlp = new WindowHelper();
            for (int i = 0; i < listOfSelectedObjId.Count; i++)
            {
                var windowId = listOfSelectedObjId[i];
                if ((i % 5) == 0)
                {
                    progressForm.ReportProgress(1, progressBarMsg);
                }
                List<ObjectId> listOfExplodeIds = AEE_Utility.ExplodeEntity(windowId);
                if (listOfExplodeIds.Count != 0)
                {
                    List<ObjectId> listOfMaxLengthExplodeLine = windowHlp.getMaxLengthOfWindowLineSegment(listOfExplodeIds);
                    List<ObjectId> listOfMinLengthExplodeLine = windowHlp.getMinLengthOfWindowLineSegment(listOfExplodeIds);
                    double lengthOfMinLengthLine = AEE_Utility.GetLengthOfLine(listOfMinLengthExplodeLine[0]);

                    ObjectId maxLngthLine_Id1 = listOfMaxLengthExplodeLine[0];
                    ObjectId maxLngthLine_Id2 = listOfMaxLengthExplodeLine[1];
                    List<double> listOfStrtEndPoint_MaxLngth1 = AEE_Utility.getStartEndPointWithAngle_Line(maxLngthLine_Id1);
                    List<double> listOfStrtEndPoint_MaxLngth2 = AEE_Utility.getStartEndPointWithAngle_Line(maxLngthLine_Id2);

                    double minX_MaxLngth1 = Math.Min(listOfStrtEndPoint_MaxLngth1[0], listOfStrtEndPoint_MaxLngth1[2]);
                    double maxX_MaxLngth1 = Math.Max(listOfStrtEndPoint_MaxLngth1[0], listOfStrtEndPoint_MaxLngth1[2]);
                    double minY_MaxLngth1 = Math.Min(listOfStrtEndPoint_MaxLngth1[1], listOfStrtEndPoint_MaxLngth1[3]);
                    double maxY_MaxLngth1 = Math.Max(listOfStrtEndPoint_MaxLngth1[1], listOfStrtEndPoint_MaxLngth1[3]);
                    double center_X_MaxLngth1 = minX_MaxLngth1 + ((maxX_MaxLngth1 - minX_MaxLngth1) / 2);
                    double center_Y_MaxLngth1 = minY_MaxLngth1 + ((maxY_MaxLngth1 - minY_MaxLngth1) / 2);

                    double minX_MaxLngth2 = Math.Min(listOfStrtEndPoint_MaxLngth2[0], listOfStrtEndPoint_MaxLngth2[2]);
                    double maxX_MaxLngth2 = Math.Max(listOfStrtEndPoint_MaxLngth2[0], listOfStrtEndPoint_MaxLngth2[2]);
                    double minY_MaxLngth2 = Math.Min(listOfStrtEndPoint_MaxLngth2[1], listOfStrtEndPoint_MaxLngth2[3]);
                    double maxY_MaxLngth2 = Math.Max(listOfStrtEndPoint_MaxLngth2[1], listOfStrtEndPoint_MaxLngth2[3]);
                    double center_X_MaxLngth2 = minX_MaxLngth2 + ((maxX_MaxLngth2 - minX_MaxLngth2) / 2);
                    double center_Y_MaxLngth2 = minY_MaxLngth2 + ((maxY_MaxLngth2 - minY_MaxLngth2) / 2);

                    var centerLineId = AEE_Utility.getLineId(center_X_MaxLngth1, center_Y_MaxLngth1, 0, center_X_MaxLngth2, center_Y_MaxLngth2, 0, false);
                    ObjectId offsetMaxLngth_Id1 = new ObjectId();
                    var offsetId1 = AEE_Utility.OffsetLine(maxLngthLine_Id1, (lengthOfMinLengthLine / 2), false);
                    var listOfInsctPoint = AEE_Utility.InterSectionBetweenTwoEntity(centerLineId, offsetId1);
                    if (listOfInsctPoint.Count == 0)
                    {
                        offsetMaxLngth_Id1 = AEE_Utility.OffsetLine(maxLngthLine_Id1, -lengthOfMinLengthLine, false);
                    }
                    else
                    {
                        offsetMaxLngth_Id1 = AEE_Utility.OffsetLine(maxLngthLine_Id1, lengthOfMinLengthLine, false);
                    }

                    List<double> listOfStrtEndPoint_OffsetMaxLngth1 = AEE_Utility.getStartEndPointWithAngle_Line(offsetMaxLngth_Id1);

                    Point3d strtPoint_MaxLngth1 = new Point3d(listOfStrtEndPoint_MaxLngth1[0], listOfStrtEndPoint_MaxLngth1[1], 0);
                    Point3d endPoint_MaxLngth1 = new Point3d(listOfStrtEndPoint_MaxLngth1[2], listOfStrtEndPoint_MaxLngth1[3], 0);

                    Point3d strtPoint_OffstMaxLngth1 = new Point3d(listOfStrtEndPoint_OffsetMaxLngth1[0], listOfStrtEndPoint_OffsetMaxLngth1[1], 0);
                    Point3d endPoint_OffstMaxLngth1 = new Point3d(listOfStrtEndPoint_OffsetMaxLngth1[2], listOfStrtEndPoint_OffsetMaxLngth1[3], 0);

                    List<Point3d> listOfNewVertexPoint = new List<Point3d>();
                    listOfNewVertexPoint.Add(strtPoint_MaxLngth1);
                    listOfNewVertexPoint.Add(endPoint_MaxLngth1);
                    listOfNewVertexPoint.Add(endPoint_OffstMaxLngth1);
                    listOfNewVertexPoint.Add(strtPoint_OffstMaxLngth1);

                    var window_Ent = AEE_Utility.GetEntityForWrite(windowId);
                    var newWindowId = AEE_Utility.createRectangleWithSameProperty(listOfNewVertexPoint, window_Ent);
                    listOfNewWindowId.Add(newWindowId);

                    AEE_Utility.deleteEntity(centerLineId);
                    AEE_Utility.deleteEntity(offsetId1);
                    AEE_Utility.deleteEntity(offsetMaxLngth_Id1);
                    AEE_Utility.deleteEntity(listOfExplodeIds);
                }
                AEE_Utility.deleteEntity(windowId);
            }
            return listOfNewWindowId;
        }
        public void change_Z_Axis_In_Polyline(List<ObjectId> listOfObjId)
        {
            foreach (var id in listOfObjId)
            {
                Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
                try
                {
                    using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
                    {
                        Entity ent = tr.GetObject(id, OpenMode.ForWrite) as Entity;
                        if (ent is Polyline)
                        {
                            Polyline acPoly = ent as Polyline;
                            // Use a for loop to get each vertex, one by one
                            int vn = acPoly.NumberOfVertices;
                            for (int i = 0; i < vn; i++)
                            {
                                // Could also get the 3D point here
                                var pt = acPoly.GetPoint2dAt(i);                          
                                acPoly.Elevation = 0;
                            }
                        }
                        tr.Commit();
                    }
                }
                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                {

                }
            }
        }


        private void setAllNewLayoutPoint()
        {
            if (listOfAllVerticesPoint.Count != 0)
            {
                double minX = listOfAllVerticesPoint.Min(sortPoint => sortPoint.X);
                double minY = listOfAllVerticesPoint.Min(sortPoint => sortPoint.Y);

                double maxX = listOfAllVerticesPoint.Max(sortPoint => sortPoint.X);
                double maxY = listOfAllVerticesPoint.Max(sortPoint => sortPoint.Y);
                lengthOfShellPlan = Math.Abs(maxX - minX);
                heightOfShellPlan = Math.Abs(maxY - minY);
                gapBetweenTwoShellPlan = (20 * (lengthOfShellPlan + heightOfShellPlan)) / 100;

                double wallLayout_X = CreateShellPlanHelper.start_X_NewLayout - minX;
                double wallLayout_Y = CreateShellPlanHelper.start_Y_NewLayout - minY;
                moveVector_WallPanelLayout = new Vector3d(wallLayout_X, wallLayout_Y, 0);       
                drawNewShellPlanInPickPoint(moveVector_WallPanelLayout);

                double pasteVector_X = 0;
                double pasteVector_Y = 0;

                if (CheckShellPlanHelper.doorWindowCount_ForLayoutCreator != 0)
                {  
                    //// add x = 0, and y = 0 
                    pasteVector_X = 0 + ((lengthOfShellPlan + gapBetweenTwoShellPlan));
                    pasteVector_Y = 0;

                    moveVector_ForWindowDoorLayout = new Vector3d(pasteVector_X, pasteVector_Y, 0);
                    listOfCopyVector.Add(moveVector_ForWindowDoorLayout);
                    listOfAllLayoutName.Add("Door & Window\nPanel Layout");
                }
             

                if (CheckShellPlanHelper.listOfSelectedOffsetBeam_ObjId.Count != 0)
                {
                    pasteVector_X = pasteVector_X + ((lengthOfShellPlan + gapBetweenTwoShellPlan));
                    ////pasteVector_Y = pasteVector_Y;
                    moveVector_ForBeamLayout = new Vector3d(pasteVector_X, pasteVector_Y, 0);
                    listOfCopyVector.Add(moveVector_ForBeamLayout);
                    listOfAllLayoutName.Add("OffsetBeam Panel\nLayout");


                    pasteVector_X = pasteVector_X + ((lengthOfShellPlan + gapBetweenTwoShellPlan));
                    ////pasteVector_Y = pasteVector_Y;
                    moveVector_ForBeamBottmLayout = new Vector3d(pasteVector_X, pasteVector_Y, 0);
                    listOfCopyVector.Add(moveVector_ForBeamBottmLayout);
                    listOfAllLayoutName.Add("OffsetBeam Bottom\nPanel Layout");
                }

                if (CheckShellPlanHelper.kickerCount_ForLayoutCreator != 0)
                {
                    pasteVector_X = pasteVector_X + ((lengthOfShellPlan + gapBetweenTwoShellPlan));
                    ////pasteVector_Y = pasteVector_Y;
                    moveVector_ForSunkanSlabLayout = new Vector3d(pasteVector_X, pasteVector_Y, 0);
                    listOfCopyVector.Add(moveVector_ForSunkanSlabLayout);
                    listOfAllLayoutName.Add("Kicker Layout");
                }              

                if (CheckShellPlanHelper.slabJointCount_ForLayoutCreator != 0)
                {
                    pasteVector_X = pasteVector_X + ((lengthOfShellPlan + gapBetweenTwoShellPlan));
                    ////pasteVector_Y = pasteVector_Y;
                    moveVector_ForSlabJointLayout = new Vector3d(pasteVector_X, pasteVector_Y, 0);
                    listOfCopyVector.Add(moveVector_ForSlabJointLayout);
                    listOfAllLayoutName.Add("Slab Joint\nLayout");
                }
                                
                ////pasteVector_X = pasteVector_X + ((lengthOfShellPlan + gapBetweenTwoShellPlan));
                //////pasteVector_Y = pasteVector_Y;
                ////moveVector_ForSoffitLayout = new Vector3d(pasteVector_X, pasteVector_Y, 0);
                ////listOfCopyVector.Add(moveVector_ForSoffitLayout);
                ////listOfAllLayoutName.Add("Soffit Panel\nLayout");

                ////pasteVector_X = pasteVector_X + ((lengthOfShellPlan + gapBetweenTwoShellPlan));
                //////pasteVector_Y = pasteVector_Y;
                ////moveVector_ForDeckPanelLayout = new Vector3d(pasteVector_X, pasteVector_Y, 0);
                ////listOfCopyVector.Add(moveVector_ForDeckPanelLayout);
                ////listOfAllLayoutName.Add("Deck Panel\nLayout");
            }
        }

        private void drawNewShellPlanInPickPoint(Vector3d moveVector)
        {
            CheckShellPlanHelper.listOfSelectedInternalWall_ObjId = AEE_Utility.copyColonEntity(moveVector, CheckShellPlanHelper.listOfSelectedInternalWall_ObjId);
            CheckShellPlanHelper.listOfSelectedExternalWall_ObjId = AEE_Utility.copyColonEntity(moveVector, CheckShellPlanHelper.listOfSelectedExternalWall_ObjId);
            CheckShellPlanHelper.listOfSelectedDuct_ObjId = AEE_Utility.copyColonEntity(moveVector, CheckShellPlanHelper.listOfSelectedDuct_ObjId);
            CheckShellPlanHelper.listOfSelectedDoor_ObjId = AEE_Utility.copyColonEntity(moveVector, CheckShellPlanHelper.listOfSelectedDoor_ObjId);
            CheckShellPlanHelper.listOfSelectedWindow_ObjId =  AEE_Utility.copyColonEntity(moveVector, CheckShellPlanHelper.listOfSelectedWindow_ObjId);
            CheckShellPlanHelper.listOfSelectedOffsetBeam_ObjId = AEE_Utility.copyColonEntity(moveVector, CheckShellPlanHelper.listOfSelectedOffsetBeam_ObjId);
            CheckShellPlanHelper.listOfSelectedStaircase_ObjId = AEE_Utility.copyColonEntity(moveVector, CheckShellPlanHelper.listOfSelectedStaircase_ObjId);
            CheckShellPlanHelper.listOfSelectedLift_ObjId = AEE_Utility.copyColonEntity(moveVector, CheckShellPlanHelper.listOfSelectedLift_ObjId);
            CheckShellPlanHelper.listOfSelectedSunkanSlab_ObjId = AEE_Utility.copyColonEntity(moveVector, CheckShellPlanHelper.listOfSelectedSunkanSlab_ObjId);
            CheckShellPlanHelper.listOfSelectedParapetWall_ObjId = AEE_Utility.copyColonEntity(moveVector, CheckShellPlanHelper.listOfSelectedParapetWall_ObjId);

        }


        private void drawNewLayout_With_RoomIndex(ProgressForm progressForm, string progressBarMsg, List<Vector3d> listOfCopyVector)
        {
            double gapBtwTextAndWall = gapBetweenTwoShellPlan;
            double textHeight = (gapBetweenTwoShellPlan / 4);

            double shellPlanText_X = (start_X_NewLayout - moveVector_WallPanelLayout.X) + (lengthOfShellPlan / 2);
            double shellPlanText_Y = (start_Y_NewLayout - moveVector_WallPanelLayout.Y) - (gapBtwTextAndWall / 2);
            string shellPlanText = "Shell Plan";
            AEE_Utility.CreateMultiLineText(shellPlanText, shellPlanText_X, shellPlanText_Y, 0, textHeight, CommonModule.layoutLayerName, CommonModule.layoutLayerColor, 0);

            double wallPlanLayout_X = start_X_NewLayout + (lengthOfShellPlan / 2);
            double wallPlanLayout_Y = start_Y_NewLayout - gapBtwTextAndWall;
            string wallPlanLayoutName = "Wall Panel\nLayout";
            AEE_Utility.CreateMultiLineText(wallPlanLayoutName, wallPlanLayout_X, wallPlanLayout_Y, 0, textHeight, CommonModule.layoutLayerName, CommonModule.layoutLayerColor, 0);

            IndexHelper indexHelper = new IndexHelper();
            bool flagOfExternalWall = false;

            InternalWallHelper.listOfInternalWallObjId = indexHelper.setValueOfRoom(progressForm, progressBarMsg, InternalWallHelper.listOfInternalWallObjId, "R_", flagOfExternalWall, out InternalWallHelper.listOfListOfInternalWallRoomLineId_With_WallName, true);

            DuctHelper.listOfDuctObjId = indexHelper.setValueOfRoom(progressForm, progressBarMsg, DuctHelper.listOfDuctObjId, "DU_", flagOfExternalWall, out DuctHelper.listOfListOfDuctRoomLineId_With_WallName);

            StairCaseHelper.listOfStairCaseObjId = indexHelper.setValueOfRoom(progressForm, progressBarMsg, StairCaseHelper.listOfStairCaseObjId, "ST_", flagOfExternalWall, out StairCaseHelper.listOfListOfStaircaseRoomLineId_With_WallName);

            LiftHelper.listOfLiftObjId = indexHelper.setValueOfRoom(progressForm, progressBarMsg, LiftHelper.listOfLiftObjId, "LF_", flagOfExternalWall, out LiftHelper.listOfListOfLiftRoomLineId_With_WallName);

            flagOfExternalWall = true;
            ExternalWallHelper.listOfExternalWallObjId = indexHelper.setValueOfRoom(progressForm, progressBarMsg, ExternalWallHelper.listOfExternalWallObjId, "EX_", flagOfExternalWall, out ExternalWallHelper.listOfListOfExternalWallRoomLineId_With_WallName);
            var moveVector =new Vector3d();
            for (int index = 0; index < listOfCopyVector.Count; index++)
            {
                 moveVector = listOfCopyVector[index];              
                AEE_Utility.copyColonEntity(moveVector, WindowHelper.listOfWindowObjId);
                AEE_Utility.copyColonEntity(moveVector, DoorHelper.listOfDoorObjId);
                AEE_Utility.copyColonEntity(moveVector, BeamHelper.listOfBeamObjId);//Added on 12/03/2023 by SDM
                AEE_Utility.copyColonEntity(moveVector, CheckShellPlanHelper.listOfSelectedOffsetBeam_ObjId);          
                AEE_Utility.copyColonEntity(moveVector, SunkanSlabHelper.listOfSunkanSlab_ObjId);
                AEE_Utility.copyColonEntity(moveVector, ParapetHelper.listOfParapetObjId);

                string layoutName = listOfAllLayoutName[index];

                double text_X = start_X_NewLayout + moveVector.X + (lengthOfShellPlan / 2);
                double text_Y = start_Y_NewLayout - moveVector.Y - gapBtwTextAndWall;
                AEE_Utility.CreateMultiLineText(layoutName, text_X, text_Y, 0, textHeight, CommonModule.layoutLayerName, CommonModule.layoutLayerColor, 0);
            }
            //Changes made on 07/06/2023 by SDM
            double _text_X = start_X_NewLayout + (moveVector.X + lengthOfShellPlan ) / 2;
            double _text_Y = start_Y_NewLayout + moveVector.Y +heightOfShellPlan+ gapBtwTextAndWall;
            AEE_Utility.CreateMultiLineText(DateTime.Now.ToString("f"), _text_X, _text_Y, 0,2* textHeight, CommonModule.layoutLayerName, CommonModule.layoutLayerColor, 0);
        }









        private bool setNewLyoutPickPoint()
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            PromptPointResult pr = ed.GetPoint("\n\n\n\nEnter window and door shell plan point:  ");
            if (pr.Status == PromptStatus.OK)
            {
                Point3d point = pr.Value;
                start_X_NewLayout = pr.Value.X;
                start_Y_NewLayout = pr.Value.Y;

                return true;
            }
            return false;
        }



    }
}
