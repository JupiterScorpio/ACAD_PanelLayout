﻿using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using PanelLayout_App.WallModule;
using System;
using System.Collections.Generic;
using PanelLayout_App.CivilModel;//RTJ 10-06-2021
using PanelLayout_App.Licensing;

namespace PanelLayout_App.ElevationModule
{
    internal sealed class PanelElevationHandler : ElevationBase
    {
        private Line _line;
        private Line _wallLine;

        private int _rw;
        private double _wo;
        private string _Ext_IntDoor;////RTJ 10-06-2021
        private bool bDoorWindowLeftInd = false; //RTJ 10-06-2021 for correcting right WSXR Panel
        private bool bDoorWindowPosition = false;//RTJ 17-06-2021
        private Tuple<Point3d, double, double, string> windowRectInfo;
        private Tuple<Point3d, double, double, string> windowPanelInfo;
        private bool bDoorWindowChk = false;//RTJ 17-06-2021
        public static List<Tuple<Point3d, double>> listOfCornerStartPtData = new List<Tuple<Point3d, double>>();//Added By RTJ 28-06-2021..

        internal void Draw()
        {
            string wallPanelData;

            string[] wallPanelDataArray;
            double wallWidth;
            double wallHeight;
            double wallLength;
            string itemCode;
            string description;
            string wallType;
            double elevation;
            bool flag_X_Axis;
            bool flag_Y_Axis;

            CommonModule.vert_cline_data = new List<Vert_Cline_data>();

            // Get Panel lines and draw elevation
            for (int index = 0; index < WallPanelHelper.listOfAllWallPanelRectId.Count; index++)
            {
                ObjectId panelLineId = WallPanelHelper.listOfAllWallPanelLineId[index];
                ObjectId panelRectId = WallPanelHelper.listOfAllWallPanelRectId[index];

                if (panelLineId.IsValid && !panelLineId.IsErased)
                {
                    wallPanelData = WallPanelHelper.listOfAllWallPanelData[index];

                    wallPanelDataArray = wallPanelData.Split('@');
                    wallWidth = Convert.ToDouble(wallPanelDataArray[0]);
                    wallHeight = Convert.ToDouble(wallPanelDataArray[1]);
                    wallLength = Convert.ToDouble(wallPanelDataArray[2]);
                    itemCode = wallPanelDataArray[3];
                    description = wallPanelDataArray[4];
                    wallType = wallPanelDataArray[5];
                    elevation = Convert.ToDouble(wallPanelDataArray[6]);
                    flag_X_Axis = Convert.ToBoolean(wallPanelDataArray[7]);
                    flag_Y_Axis = Convert.ToBoolean(wallPanelDataArray[8]);

                    if (itemCode == PanelLayout_UI.beamPanelName)//Chnages made on 03/05/2023 by SDM
                    {
                        //Commented by RTJ 16-06-2021
                        //Draw_BeamPanel(panelLineId, wallWidth, wallHeight, description, elevation);

                        //// Start.......RTJ 16-06-2021
                        if (wallType == CommonModule.wndowwallPanelType)//RTJ 16-06-2021
                        {
                            Draw_WindowVerticalTopPanel(panelLineId, wallHeight, wallWidth, description, elevation);
                        }
                        else
                        {
                            Draw_BeamPanel(panelLineId, wallWidth, wallHeight, description, elevation, wallType);
                        }
                        //// End.......RTJ 16-06-2021

                    }
                    //Added this line to draw the BeamBottomPanel by SDM 12-06-2022
                    //start
                    else if (itemCode.Contains("CC"))
                        Draw_BottomBeamPanel(panelLineId, wallWidth, wallHeight, description, elevation);
                    //end
                    else if (itemCode.StartsWith(PanelLayout_UI.doorTopPanelName) ||
                             itemCode.StartsWith(PanelLayout_UI.windowTopPanelName))
                    {
                        Draw_DoorOrWindowTopPanel(panelLineId, wallWidth, wallHeight, description, elevation);//To Change
                    }
                    else if (itemCode.StartsWith(CommonModule.doorThickWallPanelText) ||
                             itemCode.StartsWith(CommonModule.windowThickWallPanelText) || itemCode == "WS")
                    {
                        Draw_DoorThickWallPanelText_WindowThickWallPanelText(panelRectId, panelLineId, wallHeight, description, elevation, wallType, flag_X_Axis, flag_Y_Axis);//Added the Argument newly WallType...23-06-2021
                    }
                    else if (itemCode.StartsWith(PanelLayout_UI.doorWindowBottomPanelName) ||
                             itemCode.StartsWith(PanelLayout_UI.windowThickBottomWallPanelText) || itemCode.StartsWith(PanelLayout_UI.doorWindowBottomPanelNameStandard))
                    {
                        Draw_DoorWindowBottomPanel_WindowThickBottomWallPanelText(panelRectId, panelLineId, wallLength, description, elevation, itemCode);
                    }
                    else if (itemCode.StartsWith(CommonModule.doorWindowThickTopPropText) ||
                             itemCode.StartsWith(CommonModule.windowThickBottomPropText))
                    {
                        Draw_DoorWindowThickTopPropText_WindowThickBottomPropText(panelRectId, panelLineId, wallLength, description, elevation, itemCode, wallHeight);
                    }
                    else if (itemCode.StartsWith(PanelLayout_UI.kickerWallName))
                    {
                        Draw_KickerWall(panelRectId, panelLineId, wallWidth, wallHeight, description, elevation);
                    }
                    else if (itemCode.StartsWith(PanelLayout_UI.slabJointWallName))
                    {
                        Draw_SlabJointWall(panelRectId, panelLineId, wallWidth, wallHeight, description, elevation);
                    }
                    //Draw TP elevation by SDM 16-07-2022
                    else if (itemCode.StartsWith(PanelLayout_UI.wallTopPanelName))
                    {
                        Draw_WallOrWindowPanel(panelLineId, wallWidth, wallHeight, description, elevation);
                    }
                    else if (itemCode.StartsWith(PanelLayout_UI.wallPanelName) ||
                             itemCode.StartsWith(PanelLayout_UI.windowPanelName))
                    {
                        Draw_WallOrWindowPanel(panelLineId, wallWidth, wallHeight, description, wallType: wallType, wallDepth: wallLength);//WP
                    }
                    //Draw WS elevation by SDM 2022-07-20
                    //else if (itemCode == "WS")
                    //{
                    //    Draw_DoorThickWallPanelText_WindowThickWallPanelText(panelRectId, panelLineId, wallHeight, description, elevation, wallType, flag_X_Axis, flag_Y_Axis);
                    //}

                }
            }
        }
        //Added by SDM 15-06-2022s
        private void Draw_BottomBeamPanel(ObjectId panelLineId, double wallWidth, double wallHeight, string description, double elevation)
        {
            if (!PanelElevationDataInitialized(panelLineId, panelLineId))
                return;

            double distance = Math.Min(_wallLine.StartPoint.DistanceTo(_wallLine.GetClosestPointTo(_line.StartPoint - CreateShellPlanHelper.moveVector_ForBeamBottmLayout, true)),
                                     _wallLine.StartPoint.DistanceTo(_wallLine.GetClosestPointTo(_line.EndPoint - CreateShellPlanHelper.moveVector_ForBeamBottmLayout, true)));


            Point3d startPoint = BaseElevationPoint + new Vector3d(_wo + distance, elevation - _rw * WallHeight, 0);
            DrawDefaultPanelElevation(startPoint, wallWidth, wallHeight, description, false);

            Point3d endPoint = startPoint + new Vector3d(wallWidth, 0, 0);
            Vector3d vector3dOffset = Vector3d.YAxis * wallHeight;
            Point3d offsetStartPoint = startPoint + vector3dOffset;
            Point3d offsetEndPoint = endPoint + vector3dOffset;

            Vert_Cline_data data = new Vert_Cline_data();
            data.startpoint = startPoint;
            data.endpoint = endPoint;
            data.offsetstartpoint = offsetStartPoint;
            data.offsetendpoint = offsetEndPoint;
            data.paneltype = "DoororWindow";
            data.widh = wallWidth;
            data.ht = wallHeight;

            data.endpoint = startPoint + new Vector3d(wallWidth, 0, 0);

            vector3dOffset = Vector3d.YAxis * wallHeight;
            data.offsetstartpoint = startPoint + vector3dOffset;
            data.offsetendpoint = data.endpoint + vector3dOffset;
            CommonModule.vert_cline_data.Add(data);
        }
        private void Draw_BeamPanel(ObjectId panelLineId, double wallWidth, double wallHeight, string description, double elevation, string wallType)
        {
            if (!PanelElevationDataInitialized(panelLineId, panelLineId))
                return;
            //Fix BS panel for door horizontal top panel by SDM 23-06-2022
            var moveVector = wallType == CommonModule.doorWallPanelType ? CreateShellPlanHelper.moveVector_ForWindowDoorLayout : CreateShellPlanHelper.moveVector_ForBeamLayout;
            double distance = Math.Min(_wallLine.StartPoint.DistanceTo(_wallLine.GetClosestPointTo(_line.StartPoint - moveVector, true)),
                                       _wallLine.StartPoint.DistanceTo(_wallLine.GetClosestPointTo(_line.EndPoint - moveVector, true)));

            Point3d startPoint = BaseElevationPoint + new Vector3d(_wo + distance, elevation - _rw * WallHeight, 0);

            DrawDefaultPanelElevation(startPoint, wallWidth, wallHeight, description);


            Point3d endPoint = startPoint + new Vector3d(wallWidth, 0, 0);
            Vector3d vector3dOffset = Vector3d.YAxis * wallHeight;
            Point3d offsetStartPoint = startPoint + vector3dOffset;
            Point3d offsetEndPoint = endPoint + vector3dOffset;

            Vert_Cline_data data = new Vert_Cline_data();
            data.startpoint = startPoint;
            data.endpoint = endPoint;
            data.offsetstartpoint = offsetStartPoint;
            data.offsetendpoint = offsetEndPoint;
            data.paneltype = "DoororWindow";
            data.widh = wallWidth;
            data.ht = wallHeight;

            data.endpoint = startPoint + new Vector3d(wallWidth, 0, 0);

            vector3dOffset = Vector3d.YAxis * wallHeight;
            data.offsetstartpoint = startPoint + vector3dOffset;
            data.offsetendpoint = data.endpoint + vector3dOffset;
            CommonModule.vert_cline_data.Add(data);
        }

        private void Draw_WindowVerticalTopPanel(ObjectId panelLineId, double wallWidth, double wallHeight, string description, double elevation)
        {
            if (!PanelElevationDataInitialized(panelLineId, panelLineId))
                return;

            Point3d startPoint = new Point3d();
            string[] strSplit = _Ext_IntDoor.Split('_');
            string windowLayer = WallPanelHelper.listOfWindowLineIdAndLayerName[panelLineId];
            double windowLintelLevel = Window_UI_Helper.getWindowLintelLevel(windowLayer);

            double distance = Math.Min(_wallLine.StartPoint.DistanceTo(_wallLine.GetClosestPointTo(_line.StartPoint - CreateShellPlanHelper.moveVector_ForWindowDoorLayout, true)),
                                       _wallLine.StartPoint.DistanceTo(_wallLine.GetClosestPointTo(_line.EndPoint - CreateShellPlanHelper.moveVector_ForWindowDoorLayout, true)));
            if (strSplit[1] == "EX")
            {
                startPoint = BaseElevationPoint + new Vector3d(_wo + distance, windowLintelLevel - _rw * WallHeight - PanelLayout_UI.RC, 0);
                //DrawDefaultPanelElevation(startPoint, wallWidth, wallHeight, description);

                bool isCheck = false;
                if (bDoorWindowPosition == false)
                {
                    windowRectInfo = new Tuple<Point3d, double, double, string>(startPoint, wallWidth, wallHeight, description);
                    bDoorWindowPosition = true;
                    isCheck = true;
                }
                if (isCheck == false && bDoorWindowPosition)
                {
                    if (windowRectInfo.Item3 < wallHeight)
                    {
                        DrawDefaultPanelElevation(startPoint, wallWidth, wallHeight, description);
                        Point3d modifiedPoint = new Point3d(windowRectInfo.Item1.X, windowRectInfo.Item1.Y + wallHeight, 0);
                        DrawDefaultPanelElevation(modifiedPoint, windowRectInfo.Item2, windowRectInfo.Item3, windowRectInfo.Item4);

                    }
                    else
                    {
                        DrawDefaultPanelElevation(windowRectInfo.Item1, windowRectInfo.Item2, windowRectInfo.Item3, windowRectInfo.Item4);
                        Point3d modifiedPoint = new Point3d(startPoint.X, startPoint.Y + wallHeight, 0);
                        DrawDefaultPanelElevation(modifiedPoint, wallWidth, wallHeight, description);
                    }
                    windowRectInfo = null;
                    bDoorWindowPosition = false;
                    isCheck = false;
                }
            }
            else
            {
                //Fix the elevation by SDM 23-06-2022
                // startPoint = BaseElevationPoint + new Vector3d(_wo + distance, windowLintelLevel - _rw * WallHeight, 0);
                startPoint = BaseElevationPoint + new Vector3d(_wo + distance, elevation - _rw * WallHeight, 0);
                DrawDefaultPanelElevation(startPoint, wallWidth, wallHeight, description);

            }
            Point3d endPoint = startPoint + new Vector3d(wallWidth, 0, 0);
            Vector3d vector3dOffset = Vector3d.YAxis * wallHeight;
            Point3d offsetStartPoint = startPoint + vector3dOffset;
            Point3d offsetEndPoint = endPoint + vector3dOffset;

            Vert_Cline_data data = new Vert_Cline_data();
            data.startpoint = startPoint;
            data.endpoint = endPoint;
            data.offsetstartpoint = offsetStartPoint;
            data.offsetendpoint = offsetEndPoint;
            data.paneltype = "DoororWindow";
            data.widh = wallWidth;
            data.ht = wallHeight;

            data.endpoint = startPoint + new Vector3d(wallWidth, 0, 0);

            vector3dOffset = Vector3d.YAxis * wallHeight;
            data.offsetstartpoint = startPoint + vector3dOffset;
            data.offsetendpoint = data.endpoint + vector3dOffset;
            CommonModule.vert_cline_data.Add(data);
        }

        private void Draw_DoorOrWindowTopPanel(ObjectId panelLineId, double wallWidth, double wallHeight, string description, double elevation)
        {
            if (!PanelElevationDataInitialized(panelLineId, panelLineId))
                return;

            double distance = GetDistanceByDefault();
            //--------------Srart //RTJ 10-06-2021
            string[] strSplit = _Ext_IntDoor.Split('_');
            Point3d startPoint = new Point3d();

            if ((WallPanelHelper.listOfDoorLineIdAndLayerName.Count > 0) && (WallPanelHelper.listOfDoorLineIdAndLayerName.ContainsKey(panelLineId)))
            {
                string doorLayer = WallPanelHelper.listOfDoorLineIdAndLayerName[panelLineId];
                double doorLintelLevel = Door_UI_Helper.getDoorLintelLevel(doorLayer);
                //Door Elevation...
                if (strSplit[1] == "EX")
                {
                    //External Door Top Panel location = DoorLintellevel - RC
                    startPoint = BaseElevationPoint + new Vector3d(_wo + distance, elevation - _rw * WallHeight + doorLintelLevel - PanelLayout_UI.RC, 0); //RTJ
                }
                else
                {
                    //Internal Door Top Panel location = DoorLintelLevel
                    startPoint = BaseElevationPoint + new Vector3d(_wo + distance, elevation - _rw * WallHeight + doorLintelLevel, 0); //RTJ
                }
            }
            //Window elevation..
            if ((WallPanelHelper.listOfWindowLineIdAndLayerName.Count > 0) && (WallPanelHelper.listOfWindowLineIdAndLayerName.ContainsKey(panelLineId)))
            {
                string windowLayer = WallPanelHelper.listOfWindowLineIdAndLayerName[panelLineId];
                double windowLintelLevel = Window_UI_Helper.getWindowLintelLevel(windowLayer);
                //Door Elevation...
                if (strSplit[1] == "EX")
                {
                    //External Door Top Panel location = DoorLintellevel - RC
                    startPoint = BaseElevationPoint + new Vector3d(_wo + distance, elevation - _rw * WallHeight + windowLintelLevel - PanelLayout_UI.RC, 0); //RTJ                    
                }
                else
                {
                    //Internal Door Top Panel location = DoorLintelLevel
                    startPoint = BaseElevationPoint + new Vector3d(_wo + distance, elevation - _rw * WallHeight + windowLintelLevel, 0); //RTJ
                }
            }
            //ENd----------//RTJ 10-06-2021

            DrawDefaultPanelElevation(startPoint, wallWidth, wallHeight, description, textRotationAngle: 90);


            Point3d endPoint = startPoint + new Vector3d(wallWidth, 0, 0);
            Vector3d vector3dOffset = Vector3d.YAxis * wallHeight;
            Point3d offsetStartPoint = startPoint + vector3dOffset;
            Point3d offsetEndPoint = endPoint + vector3dOffset;

            Vert_Cline_data data = new Vert_Cline_data();
            data.startpoint = startPoint;
            data.endpoint = endPoint;
            data.offsetstartpoint = offsetStartPoint;
            data.offsetendpoint = offsetEndPoint;
            data.paneltype = "DoororWindow";
            data.widh = wallWidth;
            data.ht = wallHeight;

            data.endpoint = startPoint + new Vector3d(wallWidth, 0, 0);

            vector3dOffset = Vector3d.YAxis * wallHeight;
            data.offsetstartpoint = startPoint + vector3dOffset;
            data.offsetendpoint = data.endpoint + vector3dOffset;
            CommonModule.vert_cline_data.Add(data);

        }
        private bool CheckPanelAndDistSame(Point3d startPoint, double elevation, double panelHeight)
        {
            bool isAddDist = false;

            for (int i = 0; i < listOfCornerStartPtData.Count; i++)
            {
                Point3d cornerStartPoint = listOfCornerStartPtData[i].Item1;
                double cornerAngle = listOfCornerStartPtData[i].Item2;

                if (Math.Round(startPoint.X, 0) == Math.Round(cornerStartPoint.X, 0) /*&& Math.Round(panelHeight, 0) == Math.Round(cornerStartPoint.Y - 100 - elevation, 0)*/)
                {
                    if (cornerAngle == 0 || cornerAngle == 90)
                    {
                        return isAddDist;
                    }
                    else if (cornerAngle == 270 || cornerAngle == 180)
                    {
                        isAddDist = true;
                        return isAddDist;
                    }
                }
            }
            //for (int index = 0; index < CornerHelper.listOfAllCornerId_ForBOM.Count; index++)
            //{
            //    ObjectId cornerId = CornerHelper.listOfAllCornerId_ForBOM[index];

            //    if (cornerId.IsValid && !cornerId.IsErased)
            //    {
            //        CornerElevation cornerElevation = CornerHelper.listOfAllCornerElevInfo[index];

            //        if (cornerElevation == null)
            //            continue;

            //        string name = cornerElevation.wall1;
            //        //if (!WallLinesContainName(name))
            //        //    return;

            //        double wo = WallLines[name].Item2 + cornerElevation.dist1;
            //        int rw = WallLines[name].Item3;

            //        Point3d cornerPoint = BaseElevationPoint + new Vector3d(wo, cornerElevation.elev - rw * WallHeight, 0);

            //        //cornerTextData = CornerHelper.listOfAllCornerText_ForBOM[index];
            //        //double elevationDist = cornerElevation.dist1;
            //        double rotateAngle = cornerElevation.dist2;

            //        if (startPoint.X == cornerPoint.X)
            //        {
            //            if (rotateAngle == 0)
            //            {
            //                return isAddDist;
            //            }
            //            else if (rotateAngle == 90)
            //            {
            //                return isAddDist;
            //            }
            //            else if (rotateAngle == 180)
            //            {
            //                isAddDist = true;
            //                return isAddDist;
            //            }
            //            else if (rotateAngle == 270)
            //            {
            //                isAddDist = true;
            //                return isAddDist;
            //            }
            //        }
            //        break;
            //    }
            //}
            return isAddDist;
        }

        private void Draw_DoorThickWallPanelText_WindowThickWallPanelText(ObjectId panelRectId, ObjectId panelLineId, double wallHeight, string description, double elevation, string wallType, bool flag_X_Axis, bool flag_Y_Axis)//Added the Argument newly WallType...23-06-2021
        {
            // TODO: incorrect right WSXR panel
            if (!PanelElevationDataInitialized(panelRectId, panelLineId))
                return;



            //Draw the elevation panels twice when the door/window is INDOOR by SDM 2022-07-05
            KeyValuePair<string, Tuple<Line, double, int, string>> backWall = new KeyValuePair<string, Tuple<Line, double, int, string>>();
            double offset = 0;
            bool isInDoor = CheckWallIsIndoor(_Ext_IntDoor, Convert.ToDouble(description.ToSplit(' ', 0)), panelLineId, out backWall, new Point3d(), out offset);
            Point3d startPoint = new Point3d();//RTJ 10-06-2021B
            double distance = GetDistanceByDefault();
            startPoint = BaseElevationPoint + new Vector3d(_wo + distance, elevation - _rw * WallHeight, 0);
            if (wallType == "Door Wall Panel")
            {

                //bool isCheck = false;
                //if (bDoorWindowChk == false)
                //{
                // windowPanelInfo = new Tuple<Point3d, double, double, string>(startPoint, wallHeight, distance, description);
                //    bDoorWindowChk = true;
                //    isCheck = true;
                //}
                //if (isCheck == false && bDoorWindowChk)
                //{
                //    if (windowPanelInfo.Item3 < distance)
                //    {
                //        DrawDefaultPanelElevation(windowPanelInfo.Item1, CommonModule.externalCorner, windowPanelInfo.Item2, windowPanelInfo.Item4, drawInternalThickCorner: false, textRotationAngle: 90);
                //        Point3d modifiedPoint = new Point3d(startPoint.X - CommonModule.externalCorner, startPoint.Y, 0);
                //        DrawDefaultPanelElevation(modifiedPoint, CommonModule.externalCorner, wallHeight, description, drawInternalThickCorner: false, textRotationAngle: 90);
                //    }
                //    else
                //    {
                //        DrawDefaultPanelElevation(startPoint, CommonModule.externalCorner, wallHeight, description, drawInternalThickCorner: false, textRotationAngle: 90);
                //        Point3d modifiedPoint = new Point3d(windowPanelInfo.Item1.X-CommonModule.externalCorner, windowPanelInfo.Item1.Y , 0);
                //        DrawDefaultPanelElevation(modifiedPoint, CommonModule.externalCorner, windowPanelInfo.Item2, windowPanelInfo.Item4, drawInternalThickCorner: false, textRotationAngle: 90);
                //    }
                //    windowPanelInfo = null;
                //    bDoorWindowChk = false;
                //    isCheck = false;
                //}

                bool isAddDist = CheckPanelAndDistSame(startPoint, elevation, BaseElevationPoint.Y + wallHeight - WallHeight);
                if (isAddDist)
                {
                    startPoint = BaseElevationPoint + new Vector3d(_wo + distance - CommonModule.externalCorner, elevation - _rw * WallHeight, 0);
                }
                else
                {
                    startPoint = BaseElevationPoint + new Vector3d(_wo + distance /*- CommonModule.externalCorner*/, elevation - _rw * WallHeight, 0);
                }
                DrawDefaultPanelElevation(startPoint, CommonModule.externalCorner, wallHeight, description, drawInternalThickCorner: false, textRotationAngle: 90);

                //Draw the door elevation panels twice when the door is INDOOR by SDM 2022-07-05
                if (isInDoor)
                {
                    var _wo2 = backWall.Value.Item2;
                    var _rw2 = backWall.Value.Item3;
                    var _wallLine2 = backWall.Value.Item1;
                    double distance2 = Math.Min(_wallLine2.StartPoint.DistanceTo(_wallLine2.GetClosestPointTo(_line.StartPoint, true)),
                     _wallLine2.StartPoint.DistanceTo(_wallLine2.GetClosestPointTo(_line.EndPoint, true)));

                    if (SunkanSlabHelper.listOfWallName_SunkanSlabId.ContainsKey(backWall.Key))
                        wallHeight -= PanelLayout_UI.RC;
                    //else if (SunkanSlabHelper.listOfWallName_SunkanSlabId.ContainsKey(_Ext_IntDoor))
                    //    elevation += PanelLayout_UI.RC;//Commented by SDM 2022-08-23

                    var startPoint2 = BaseElevationPoint + new Vector3d(_wo2 + distance2, elevation - _rw2 * WallHeight, 0);
                    if (isAddDist)
                    {
                        startPoint2 = BaseElevationPoint + new Vector3d(_wo2 + distance2 /*- CommonModule.externalCorner*/, elevation - _rw2 * WallHeight, 0);
                    }
                    else
                    {
                        startPoint2 = BaseElevationPoint + new Vector3d(_wo2 + distance2 - CommonModule.externalCorner, elevation - _rw2 * WallHeight, 0);
                    }
                    DrawDefaultPanelElevation(startPoint2, CommonModule.externalCorner, wallHeight, description, drawInternalThickCorner: false, textRotationAngle: 90);
                }
            }
            else
            {
                bool isAddDist = CheckPanelAndDistSame(startPoint, elevation, BaseElevationPoint.Y + wallHeight - WallHeight);
                if (isAddDist)
                {
                    startPoint = BaseElevationPoint + new Vector3d(_wo + distance - CommonModule.externalCorner, elevation - _rw * WallHeight, 0);
                }
                else
                {
                    startPoint = BaseElevationPoint + new Vector3d(_wo + distance /*- CommonModule.externalCorner*/, elevation - _rw * WallHeight, 0);
                }
                DrawDefaultPanelElevation(startPoint, CommonModule.externalCorner, wallHeight, description, drawInternalThickCorner: false, textRotationAngle: 90);


                //Draw the door elevation panels twice when the window is INDOOR by SDM 2022-07-20
                if (isInDoor)
                {
                    var _wo2 = backWall.Value.Item2;
                    var _rw2 = backWall.Value.Item3;
                    var _wallLine2 = backWall.Value.Item1;
                    double distance2 = Math.Min(_wallLine2.StartPoint.DistanceTo(_wallLine2.GetClosestPointTo(_line.StartPoint, true)),
                     _wallLine2.StartPoint.DistanceTo(_wallLine2.GetClosestPointTo(_line.EndPoint, true)));
                    if (SunkanSlabHelper.listOfWallName_SunkanSlabId.ContainsKey(backWall.Key))
                        elevation -= PanelLayout_UI.RC;
                    else if (SunkanSlabHelper.listOfWallName_SunkanSlabId.ContainsKey(_Ext_IntDoor))
                        elevation += PanelLayout_UI.RC;
                    var startPoint2 = BaseElevationPoint + new Vector3d(_wo2 + distance2, elevation - _rw2 * WallHeight, 0);
                    if (isAddDist)
                    {
                        startPoint2 = BaseElevationPoint + new Vector3d(_wo2 + distance2 /*- CommonModule.externalCorner*/, elevation - _rw2 * WallHeight, 0);
                    }
                    else
                    {
                        startPoint2 = BaseElevationPoint + new Vector3d(_wo2 + distance2 - CommonModule.externalCorner, elevation - _rw2 * WallHeight, 0);
                    }

                    DrawDefaultPanelElevation(startPoint2, CommonModule.externalCorner, wallHeight, description, drawInternalThickCorner: false, textRotationAngle: 90);
                }



                //startPoint = BaseElevationPoint + new Vector3d(_wo + distance/* - CommonModule.externalCorner*/, elevation - _rw * WallHeight, 0);

                //bool isCheck = false;
                //if (bDoorWindowChk == false)
                //{
                //    windowPanelInfo = new Tuple<Point3d, double, double, string>(startPoint, wallHeight, distance, description);
                //    bDoorWindowChk = true;
                //    isCheck = true;
                //}
                //if (isCheck == false && bDoorWindowChk)
                //{
                //    if (windowPanelInfo.Item3 < distance)
                //    {
                //        DrawDefaultPanelElevation(windowPanelInfo.Item1, CommonModule.externalCorner, windowPanelInfo.Item2, windowPanelInfo.Item4, drawInternalThickCorner: false, textRotationAngle: 90);
                //        Point3d modifiedPoint = new Point3d(startPoint.X - CommonModule.externalCorner, startPoint.Y, 0);
                //        DrawDefaultPanelElevation(modifiedPoint, CommonModule.externalCorner, wallHeight, description, drawInternalThickCorner: false, textRotationAngle: 90);
                //    }
                //    else
                //    {
                //        DrawDefaultPanelElevation(startPoint, CommonModule.externalCorner, wallHeight, description, drawInternalThickCorner: false, textRotationAngle: 90);
                //        Point3d modifiedPoint = new Point3d(windowPanelInfo.Item1.X-CommonModule.externalCorner, windowPanelInfo.Item1.Y , 0);
                //        DrawDefaultPanelElevation(modifiedPoint, CommonModule.externalCorner, windowPanelInfo.Item2, windowPanelInfo.Item4, drawInternalThickCorner: false, textRotationAngle: 90);
                //    }
                //    windowPanelInfo = null;
                //    bDoorWindowChk = false;
                //    isCheck = false;
                //}

                // DrawDefaultPanelElevation(startPoint, CommonModule.externalCorner, wallHeight, description, drawInternalThickCorner: false, textRotationAngle: 90);
            }
            //RTJ 10-06-2021 ends for correcting right WSXR Panel 
        }

        private void Draw_DoorWindowBottomPanel_WindowThickBottomWallPanelText(ObjectId panelRectId, ObjectId panelLineId, double wallLength, string description, double elevation, string itemCode)
        {
            if (!PanelElevationDataInitialized(panelRectId, panelLineId))
                return;

            double distance = GetDistanceByDefault();
            double sill = GetSill(itemCode);


            Point3d startPoint = BaseElevationPoint + new Vector3d(_wo + distance, elevation - sill - _rw * WallHeight, 0);

            DrawDefaultPanelElevation(startPoint, wallLength, CommonModule.externalCorner, description, drawInternalThickCorner: false);

            //Draw the elevation panels twice when the door/window is INDOOR by SDM 2022-07-05
            KeyValuePair<string, Tuple<Line, double, int, string>> backWall = new KeyValuePair<string, Tuple<Line, double, int, string>>();
            double offset = 0;
            double max_offset = Convert.ToDouble(description.ToSplit(' ', 0));
            bool isInDoor = CheckWallIsIndoor(_Ext_IntDoor, max_offset, panelLineId, out backWall, new Point3d(), out offset);
            if (isInDoor)
            {
                var _wo2 = backWall.Value.Item2;
                var _rw2 = backWall.Value.Item3;
                var _wallLine2 = backWall.Value.Item1;
                double distance2 = Math.Min(_wallLine2.StartPoint.DistanceTo(_wallLine2.GetClosestPointTo(_line.StartPoint, true)),
                 _wallLine2.StartPoint.DistanceTo(_wallLine2.GetClosestPointTo(_line.EndPoint, true)));
                sill = GetSill(itemCode);
                if (SunkanSlabHelper.listOfWallName_SunkanSlabId.ContainsKey(backWall.Key))
                    sill += PanelLayout_UI.RC;
                else if (SunkanSlabHelper.listOfWallName_SunkanSlabId.ContainsKey(_Ext_IntDoor))
                    sill -= PanelLayout_UI.RC;
                var startPoint2 = BaseElevationPoint + new Vector3d(_wo2 + distance2, elevation - sill - _rw2 * WallHeight, 0);
                DrawDefaultPanelElevation(startPoint2, wallLength, CommonModule.externalCorner, description, drawInternalThickCorner: false);

            }

        }

        private void Draw_DoorWindowThickTopPropText_WindowThickBottomPropText(ObjectId panelRectId, ObjectId panelLineId, double wallLength, string description, double elevation, string itemCode, double wallHeight)
        {
            if (!PanelElevationDataInitialized(panelRectId, panelLineId))
                return;

            Vector3d lv = (_line.StartPoint - _line.EndPoint).GetPerpendicularVector();
            lv *= wallLength / lv.Length;
            Point3d npt = _wallLine.GetClosestPointTo(_line.StartPoint, true);
            Point3d ept = npt - lv;
            ///Fix overlapping SDM 2022-07-27
            //double distance = Math.Min(_wallLine.StartPoint.DistanceTo(npt),
            //                           _wallLine.StartPoint.DistanceTo(ept));

            //Updated by SDM 2022-08-20
            double d = GetDistanceByDefault();
            double distance = d + wallHeight + (Math.Round(d) <= CommonModule.internalCorner ? 0 : CommonModule.internalCorner);

            double sill = GetSill(itemCode);

            //Changes made on 13/04/2023 by SDM
            if (Beam_UI_Helper.checkBeamLayerIsExist(_line.Layer))
                distance = d;

            Point3d startPoint = BaseElevationPoint + new Vector3d(_wo + distance, elevation - sill - _rw * WallHeight, 0);

            DrawDefaultPanelElevation(startPoint, wallLength, CommonModule.externalCorner, description, drawInternalThickCorner: false);

            //Draw the elevation panels twice when the door/window is INDOOR by SDM 2022-07-05
            KeyValuePair<string, Tuple<Line, double, int, string>> backWall = new KeyValuePair<string, Tuple<Line, double, int, string>>();
            double offset = 0;
            bool isInDoor = CheckWallIsIndoor(_Ext_IntDoor, Convert.ToDouble(description.ToSplit(' ', 2)), panelLineId, out backWall, new Point3d(), out offset);
            if (isInDoor)
            {
                var _wo2 = backWall.Value.Item2;
                var _rw2 = backWall.Value.Item3;
                var _wallLine2 = backWall.Value.Item1;
                d = Math.Min(_wallLine2.StartPoint.DistanceTo(_wallLine2.GetClosestPointTo(_line.StartPoint, true)),
                 _wallLine2.StartPoint.DistanceTo(_wallLine2.GetClosestPointTo(_line.EndPoint, true)));
                double distance2 = d + wallHeight + (Math.Round(d) <= CommonModule.internalCorner ? 0 : CommonModule.internalCorner); //Updated by SDM 2022-08-20

                sill = GetSill(itemCode);

                //Changes made on 13/04/2023 by SDM
                if (Beam_UI_Helper.checkBeamLayerIsExist(_line.Layer))
                    distance2 = d;

                if (SunkanSlabHelper.listOfWallName_SunkanSlabId.ContainsKey(backWall.Key))
                    sill += PanelLayout_UI.RC;
                else if (SunkanSlabHelper.listOfWallName_SunkanSlabId.ContainsKey(_Ext_IntDoor))
                    sill -= PanelLayout_UI.RC;

                Point3d startPoint2 = BaseElevationPoint + new Vector3d(_wo2 + distance2, elevation - sill - _rw2 * WallHeight, 0);
                DrawDefaultPanelElevation(startPoint2, wallLength, CommonModule.externalCorner, description, drawInternalThickCorner: false);

            }

        }

        private void Draw_KickerWall(ObjectId panelRectId, ObjectId panelLineId, double wallWidth, double wallHeight, string description, double elevation)
        {
            if (!PanelElevationDataInitialized(panelRectId, panelLineId))
                return;

            double distance = Math.Min(_wallLine.StartPoint.DistanceTo(_wallLine.GetClosestPointTo(_line.StartPoint - CreateShellPlanHelper.moveVector_ForSunkanSlabLayout, true)),
                                       _wallLine.StartPoint.DistanceTo(_wallLine.GetClosestPointTo(_line.EndPoint - CreateShellPlanHelper.moveVector_ForSunkanSlabLayout, true)));

           

            Point3d startPoint = BaseElevationPoint + new Vector3d(_wo + distance, elevation - wallHeight - _rw * WallHeight, 0);

            DrawCustomPanelElevation(startPoint, wallWidth, wallHeight, description,
                                     CommonModule.kickerWallPanelLayerName, CommonModule.kickerWallPanelTextLayerName, CommonModule.kickerWallPanelLayerColor);
        }

        private void Draw_SlabJointWall(ObjectId panelRectId, ObjectId panelLineId, double wallWidth, double wallHeight, string description, double elevation)
        {
            if (!PanelElevationDataInitialized(panelRectId, panelLineId))
                return;

            double distance = Math.Min(_wallLine.StartPoint.DistanceTo(_wallLine.GetClosestPointTo(_line.StartPoint - CreateShellPlanHelper.moveVector_ForSlabJointLayout, true)),
                                       _wallLine.StartPoint.DistanceTo(_wallLine.GetClosestPointTo(_line.EndPoint - CreateShellPlanHelper.moveVector_ForSlabJointLayout, true)));

            Point3d startPoint = BaseElevationPoint + new Vector3d(_wo + distance, elevation - _rw * WallHeight, 0);

            DrawCustomPanelElevation(startPoint, wallWidth, wallHeight, description,
                                     CommonModule.slabJointCornerLayerName, CommonModule.slabJointWallPanelTextLayerName, CommonModule.slabJointCornerLayerColor);
        }

        private void Draw_WallOrWindowPanel(ObjectId panelLineId, double wallWidth, double wallHeight, string description, double elevation = 0, string wallType = "", double wallDepth = 0)
        {
            // TODO: here correct WWPR panels
            if (!PanelElevationDataInitialized(panelLineId, panelLineId))
                return;

            string name = GetPanelName(panelLineId);

            double distance = GetDistanceByDefault();

            double dW = wallWidth;
            double dH = wallHeight;
            double extOffset = 0d;
            string wp_name = PanelLayout_UI.wallPanelName;
            string tp_name = PanelLayout_UI.wallTopPanelName;
            if (description.Contains("(R)"))
            {
                dW += CommonModule.externalCorner;
                ///Fix WPRX(R) overlaping SDM 21_May_2022
               // distance -= CommonModule.externalCorner;
                extOffset = -CommonModule.externalCorner - 1000;

                Tuple<Line, double, int, string> t = WallLines[WallLines[name].Item4];
                if (t.Item4 == name && WallLines[name].Item2 < t.Item2)
                    extOffset = t.Item2 + t.Item1.Length - _wo + CommonModule.externalCorner;

                //Fix WPX(R) position by SDM 22-06-2022 //added TPX condition by SDM 2022-08-04
                if ((name.Contains("EX") && (description.Contains(wp_name + "X") || description.Contains(tp_name + "X"))))
                    distance -= CommonModule.externalCorner;
            }
            else if (description.Contains("(L)"))
            {
                dW += CommonModule.externalCorner;
                extOffset = dW + 1000;

                Tuple<Line, double, int, string> t = WallLines[WallLines[name].Item4];
                if (t.Item4 == name && WallLines[name].Item2 > t.Item2)
                    //extOffset +=t.Item2 + t.Item1.Length - _wo;
                    extOffset = -(_wo + distance);//Fixed by SDM 2022-08-10

                //Fix WPRX(L) overlapping when it comes in the begining by SDM 06_June_2022
                if (distance == 100 && description.Contains(wp_name + "RX"))
                    distance -= dW;
            }
            //Fix WPR position when door intersects internal corner by SDM 2022-07-28
            bool flagOfBegining = _wallLine.StartPoint.DistanceTo(_line.StartPoint) < _wallLine.EndPoint.DistanceTo(_line.StartPoint);
            if (wallType == CommonModule.doorWallPanelType && !description.Contains(wp_name+"X"))
            {
                var nextWall = name;
                var prevWall = name;
                InternalWallHelper.GetPreviousNextWallName(name, out nextWall, out prevWall);

                //Draw the elevation panels twice when the door/window is INDOOR by SDM 2022-07-28
                KeyValuePair<string, Tuple<Line, double, int, string>> backWall = new KeyValuePair<string, Tuple<Line, double, int, string>>();
                double offset = 0;
                bool isInDoor = CheckWallIsIndoor(description.Contains(wp_name+"RX") ? flagOfBegining ? prevWall : nextWall : _Ext_IntDoor, _line.Length, panelLineId, out backWall, new Point3d(), out offset, true);

                if (description.Contains(" "+wp_name+"R"+" "))
                {
                    //Changes made on 18/04/2023 by SDM  fix when the "Beam/Door" thickline is >450 so we have 2 panels, one of them isn't drawn in the back door
                    if (!isInDoor)
                        isInDoor = CheckWallIsIndoor(_Ext_IntDoor, wallDepth, panelLineId, out backWall, new Point3d(), out offset, true);
                    if (!isInDoor)
                    {
                        var stretch_vec = ((_line.EndPoint - _line.StartPoint) / _line.Length) * Math.Abs(wallDepth - _line.Length);
                        var new_panel_lineId = AEE_Utility.CreateLine(_line.StartPoint, _line.EndPoint + stretch_vec, _line.Layer, 0);
                        isInDoor = CheckWallIsIndoor(_Ext_IntDoor, wallDepth, new_panel_lineId, out backWall, new Point3d(), out offset, true);
                        AEE_Utility.deleteEntity(new_panel_lineId);
                        if (!isInDoor)
                        {
                            new_panel_lineId = AEE_Utility.CreateLine(_line.StartPoint - stretch_vec, _line.EndPoint, _line.Layer, 0);
                            isInDoor = CheckWallIsIndoor(_Ext_IntDoor, wallDepth, new_panel_lineId, out backWall, new Point3d(), out offset, true);
                            AEE_Utility.deleteEntity(new_panel_lineId);
                        }
                    }//-----------------------------------------------------------

                    string wall1 = flagOfBegining ? prevWall : nextWall;
                    var wallline1 = WallLines[wall1].Item1;
                    double distance1 = Math.Min(wallline1.StartPoint.DistanceTo(wallline1.GetClosestPointTo(_line.StartPoint, true)),
                         wallline1.StartPoint.DistanceTo(wallline1.GetClosestPointTo(_line.EndPoint, true)));// - (flagOfBegining ? 0 : wallWidth); //Changes made on 17/04/2023

                    bool flagToDraw=false;//Added on 17/07/2023 by PRT

                    //Changes made on 17/04/2023 by SDM fix position
                    if (!flagOfBegining)
                        if ((!AEE_Utility.IsInsideLine(wallline1, _line.StartPoint) && !AEE_Utility.IsInsideLine(wallline1, _line.EndPoint)))
                        {
                            flagToDraw=true; 
                            distance1 = -(wallline1.StartPoint - _line.StartPoint).Length - wallWidth;
                        }
                        else if (!AEE_Utility.IsInsideLine(wallline1, _line.StartPoint) || !AEE_Utility.IsInsideLine(wallline1, _line.EndPoint))
                            distance1 -= wallWidth;

                    if (flagToDraw ==false)//Added on 17/07/2023 by PRT
                    {
                        Point3d startPoint1 = BaseElevationPoint + new Vector3d(WallLines[wall1].Item2 + distance1, elevation - WallLines[wall1].Item3 * WallHeight, 0);
                        DrawDefaultPanelElevation(startPoint1, dW, dH, description, textRotationAngle: 90);
                    }
                  

                    //Draw on the backwall by SDM 2022-08-15
                    if (isInDoor)
                    {
                        var _nextWall = name;
                        var _prevWall = name;
                        InternalWallHelper.GetPreviousNextWallName(backWall.Key, out _nextWall, out _prevWall);

                        string wall2 = !flagOfBegining ? _prevWall : _nextWall;
                        var wallline2 = WallLines[wall2].Item1;
                        double distance2 = Math.Min(wallline2.StartPoint.DistanceTo(wallline2.GetClosestPointTo(_line.StartPoint, true)),
                             wallline2.StartPoint.DistanceTo(wallline2.GetClosestPointTo(_line.EndPoint, true)));// - (!flagOfBegining ? 0 : wallWidth);

                        //Changes made on 18/04/2023 by SDM fix position
                        if (flagOfBegining)
                            if ((!AEE_Utility.IsInsideLine(wallline2, _line.StartPoint) && !AEE_Utility.IsInsideLine(wallline2, _line.EndPoint)))
                            {
                                distance2 = -(wallline2.StartPoint - _line.StartPoint).Length - wallWidth;
                            }
                            else if (!AEE_Utility.IsInsideLine(wallline2, _line.StartPoint) || !AEE_Utility.IsInsideLine(wallline2, _line.EndPoint))
                                distance2 -= wallWidth;

                        Point3d startPoint2 = BaseElevationPoint + new Vector3d(WallLines[wall2].Item2 + distance2, elevation - WallLines[wall2].Item3 * WallHeight, 0);
                        DrawDefaultPanelElevation(startPoint2, dW, dH, description, textRotationAngle: 90);

                    }


                    dW = CommonModule.externalCorner;
                    if (!flagOfBegining)
                        distance -= dW;

                    //Changes made on 17/04/2023 by SDM do not draw "Door/Beam" thickLine panel if there 2 panels along the thickline, we should draw only one
                    if (AEE_Utility.FindIntersection(_wallLine, _line) == null)
                        return;
                }
                else if (description.Contains(wp_name + "RX"))
                {
                    string wall1 = flagOfBegining ? prevWall : nextWall;
                    var wallline1 = WallLines[wall1].Item1;
                    double distance1 = Math.Min(wallline1.StartPoint.DistanceTo(wallline1.GetClosestPointTo(_line.StartPoint, true)),
                         wallline1.StartPoint.DistanceTo(wallline1.GetClosestPointTo(_line.EndPoint, true))) + (flagOfBegining ? -1 : 0) * CommonModule.externalCorner;

                    Point3d startPoint1 = BaseElevationPoint + new Vector3d(WallLines[wall1].Item2 + distance1, elevation - WallLines[wall1].Item3 * WallHeight, 0);
                    DrawDefaultPanelElevation(startPoint1, CommonModule.externalCorner, dH, description, textRotationAngle: 90, drawInternalThickCorner: false);
                }


                if (isInDoor)
                {
                    double rc = 0;
                    var wallline2 = backWall.Value.Item1;
                    double distance1 = Math.Min(wallline2.StartPoint.DistanceTo(wallline2.GetClosestPointTo(_line.StartPoint, true)),
                         wallline2.StartPoint.DistanceTo(wallline2.GetClosestPointTo(_line.EndPoint, true)));
                    if ((flagOfBegining && description.Contains(" " + wp_name + "R" + " ")) || (!flagOfBegining && description.Contains(wp_name + "RX")))
                        distance1 -= CommonModule.externalCorner;
                    if (SunkanSlabHelper.listOfWallName_SunkanSlabId.ContainsKey(backWall.Key))
                    {
                        rc -= PanelLayout_UI.RC;
                    }
                    else if (SunkanSlabHelper.listOfWallName_SunkanSlabId.ContainsKey(name))
                        rc = PanelLayout_UI.RC;

                    Point3d startPoint1 = BaseElevationPoint + new Vector3d(backWall.Value.Item2 + distance1, elevation - backWall.Value.Item3 * WallHeight, 0);
                    DrawDefaultPanelElevation(startPoint1, CommonModule.externalCorner, dH + rc, description, textRotationAngle: 90, drawInternalThickCorner: false);
                }
            }



            //Draw TP elevation by SDM 16-07-2022
            // Point3d startPoint = BaseElevationPoint + new Vector3d(_wo + distance, -_rw * WallHeight, 0);
            Point3d startPoint = BaseElevationPoint + new Vector3d(_wo + distance, elevation - _rw * WallHeight, 0);
            DrawDefaultPanelElevation(startPoint, dW, dH, description, textRotationAngle: 90);


            Point3d endPoint = startPoint + new Vector3d(dW, 0, 0);
            Vector3d vector3dOffset = Vector3d.YAxis * dH;
            Point3d offsetStartPoint = startPoint + vector3dOffset;
            Point3d offsetEndPoint = endPoint + vector3dOffset;

            Vert_Cline_data data = new Vert_Cline_data();
            data.startpoint = startPoint;
            data.endpoint = endPoint;
            data.offsetstartpoint = offsetStartPoint;
            data.offsetendpoint = offsetEndPoint;
            data.paneltype = "DoororWindow";
            data.widh = dW;
            data.ht = dH;

            data.endpoint = startPoint + new Vector3d(dW, 0, 0);

            vector3dOffset = Vector3d.YAxis * dH;
            data.offsetstartpoint = startPoint + vector3dOffset;
            data.offsetendpoint = data.endpoint + vector3dOffset;
            CommonModule.vert_cline_data.Add(data);

            ///Fix WPRX(R) duplicated panel SDM 21_May_2022
           // if (extOffset != 0d)
            if (extOffset != 0d && this._Ext_IntDoor.Contains("_EX_"))
            {
                dW = CommonModule.externalCorner;
                startPoint += new Vector3d(extOffset, 0, 0);

                DrawDefaultPanelElevation(startPoint, dW, dH, description, drawInternalThickCorner: false, textRotationAngle: 90);
            }



        }



        private bool PanelElevationDataInitialized(ObjectId panelId, ObjectId panelLineId)
        {
            string name = GetPanelName(panelId);

            if (!WallLinesContainName(name))
                return false;

            _line = AEE_Utility.GetLine(panelLineId);

            _wallLine = WallLines[name].Item1;
            _wo = WallLines[name].Item2;
            _rw = WallLines[name].Item3;
            _Ext_IntDoor = name;//RTJ 10-06-2021
            return true;
        }

        private void DrawDefaultPanelElevation(Point3d startPoint, double width, double height, string description, bool drawInternalThickCorner = true, double textRotationAngle = 0)
        {
            Point3d endPoint = startPoint + new Vector3d(width, 0, 0);

            Vector3d vector3dOffset = Vector3d.YAxis * height;
            Point3d offsetStartPoint = startPoint + vector3dOffset;
            Point3d offsetEndPoint = endPoint + vector3dOffset;
            var layername = CommonModule.wallPanelLyrName;
            var layercolor = CommonModule.eleWallPanelLyrColor;

            if (description.Contains(" CC "))
            {
                layername = CommonModule.beamCornerLayerName;
                layercolor = CommonModule.beamCornerLayerColor;
            }

            AEE_Utility.CreateRectangle(startPoint, endPoint, offsetEndPoint, offsetStartPoint, layername, layercolor);

            if (drawInternalThickCorner)
            {
                Vector3d vne = new Vector3d(CommonModule.internalCornerThick, CommonModule.internalCornerThick, 0);
                Vector3d vnw = new Vector3d(CommonModule.internalCornerThick, -CommonModule.internalCornerThick, 0);

                if (AEE_Utility.CustType == eCustType.IHPanel)
                {
                    AEE_Utility.CreateRectangle(startPoint + vne, endPoint - vnw, offsetEndPoint - vne, offsetStartPoint + vnw, CommonModule.wallPanelLyrName, CommonModule.eleWallPanelLyrColor);//Commented on 30/06/2023 by PRT
                }
                   
            }

            Point3d textPoint = GetTextPoint(startPoint, width, height);
#if !WNPANEL
            if (PanelLayout_UI.int_wall_hgt > height)
            {
                description = description + " (S)";
            }
#endif
            AEE_Utility.CreateMultiLineText(description, textPoint, 20, layername, layercolor, textRotationAngle, 1.5);
        }

        private void DrawCustomPanelElevation(Point3d startPoint, double width, double height, string description, string layerName, string textLayerName, int layerColor, double textRotationAngle = 0d)
        {
            Point3d endPoint = startPoint + new Vector3d(width, 0, 0);

            Vector3d vector3dOffset = Vector3d.YAxis * height;
            Point3d offsetStartPoint = startPoint + vector3dOffset;
            Point3d offsetEndPoint = endPoint + vector3dOffset;

            AEE_Utility.CreateRectangle(startPoint, endPoint, offsetEndPoint, offsetStartPoint, layerName, layerColor);

            Point3d textPoint = GetTextPoint(startPoint, width, height);
            AEE_Utility.CreateMultiLineText(description, textPoint, 20, textLayerName, layerColor, textRotationAngle, 1.5);
        }

        private double GetDistanceByDefault() =>
            Math.Min(_wallLine.StartPoint.DistanceTo(_wallLine.GetClosestPointTo(_line.StartPoint, true)),
                     _wallLine.StartPoint.DistanceTo(_wallLine.GetClosestPointTo(_line.EndPoint, true)));

        private string GetPanelName(ObjectId panelId) =>
            AEE_Utility.GetXDataRegisterAppName(panelId);

        private double GetSill(string itemCode) =>
            itemCode.StartsWith(PanelLayout_UI.windowThickBottomWallPanelText) ||
            itemCode.StartsWith(CommonModule.windowThickBottomPropText)
            ? CommonModule.externalCorner : 0d;
    }

    public class kIcker_corner_CLine
    {
        public Point3d basepnt = new Point3d();
        public double center_X = new double();
        public double center_Y = new double();
        public string wallname = string.Empty;
        public double flange1 = new double();
        public double flange2 = new double();
    }

    public class LCX_CCX_corner_CLine
    {
        public Point3d startpoint1 = new Point3d();
        public Point3d endpoint1 = new Point3d();
        public Point3d startpoint2 = new Point3d();
        public Point3d endpoint2 = new Point3d();
        public double angle = 0;
        public Vector3d bottommove = new Vector3d();
        public Vector3d topmove = new Vector3d();
        public string panelype = string.Empty;
        public String panellayername = string.Empty;
        public List<LCX_CCX_mid_CLine> LCX_CCX_mid_CLine = new List<ElevationModule.LCX_CCX_mid_CLine>();
    }

    public class LCX_CCX_mid_CLine
    {
        public Point3d startpoint1 = new Point3d();
        public Point3d endpoint1 = new Point3d();
        public Point3d startpoint2 = new Point3d();
        public Point3d endpoint2 = new Point3d();
        public double angle = 0;
    }

    public class slapjoint_corner_CLine
    {
        public Point3d basepnt = new Point3d();
        public double center_X = new double();
        public double center_Y = new double();
        public string wallname = string.Empty;
        public double flange1 = new double();
        public double flange2 = new double();
    }

    public class slapjoint_CLine
    {
        public string wallname = string.Empty;
        public bool x_axis = true;
        public double length = 0;
        public Point3d offsetpoint = new Point3d();
        public Point3d startpoint = new Point3d();
        public Vector3d movevector = new Vector3d();
        public List<double> listOfWallPanelLength = new List<double>();
        public double wallPanelLineAngle = 0;
    }

    public class Deck_panel_CLine
    {
        public Point3d startpoint1 = new Point3d();
        public Point3d endpoint1 = new Point3d();
        public Point3d startpoint2 = new Point3d();
        public Point3d endpoint2 = new Point3d();
        public double angle = 0;
        public double pitch = 0;
    }





    public class kIcker_panel_CLine
    {
        public Point3d startpoint = new Point3d();
        public Point3d offsetpoint = new Point3d();
        public Vector3d movevector = new Vector3d();
        public string wallname = string.Empty;
        public double length = 0;
        public bool x_axis = true;
        public List<double> listOfWallPanelLength = new List<double>();
        public double angle = 0;
    }

    public class cornerpnt
    {
        public Point3d startpoint = new Point3d();
        public string wallname = string.Empty;
        public ObjectId objid = new ObjectId();
        public bool x_axis = true;
    }
    public class WP_Cline_data
    {
        public Point3d startpoint = new Point3d();
        public Point3d endpoint = new Point3d();
        public Point3d offsetstartpoint = new Point3d();
        public Point3d offsetendpoint = new Point3d();
        public string panelname = string.Empty;
        public string layoutname = string.Empty;
        public bool xaxis = true;
    }
    public class Vert_Cline_data
    {
        public Point3d startpoint = new Point3d();
        public Point3d endpoint = new Point3d();
        public Point3d offsetstartpoint = new Point3d();
        public Point3d offsetendpoint = new Point3d();
        public string paneltype = string.Empty;
        public double widh = 0;
        public double ht = 0;
    }
}
