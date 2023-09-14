using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using PanelLayout_App.WallModule;
using System;
using System.Collections.Generic;
namespace PanelLayout_App.ElevationModule
{
    internal sealed class CornerElevationHandler : ElevationBase
    {
        private const string BEAM_CORNER_ITEM_CODE = " CC";

        private readonly CornerHelper _cornerHelper;

        private readonly PartHelper _partHelper;

        internal CornerElevationHandler()
        {
            _cornerHelper = new CornerHelper();
            _partHelper = new PartHelper();
        }

        internal void Draw()
        {
            PanelElevationHandler.listOfCornerStartPtData.Clear();

            string cornerTextData;

            string[] cornerTextDataArray;
            string itemCode;
            string description;
            string wallType;
            double l1;
            double l2;
            double h;

            for (int index = 0; index < CornerHelper.listOfAllCornerId_ForBOM.Count; index++)
            {
                ObjectId cornerId = CornerHelper.listOfAllCornerId_ForBOM[index];

                if (cornerId.IsValid && !cornerId.IsErased)
                {
                    CornerElevation cornerElevation = CornerHelper.listOfAllCornerElevInfo[index];

                    if (cornerElevation == null)
                        continue;

                    cornerTextData = CornerHelper.listOfAllCornerText_ForBOM[index];

                    cornerTextDataArray = cornerTextData.Split('@');
                    itemCode = cornerTextDataArray[0];
                    description = cornerTextDataArray[1];
                    wallType = cornerTextDataArray[2];
                    l1 = Convert.ToDouble(cornerTextDataArray[3]);
                    l2 = Convert.ToDouble(cornerTextDataArray[4]);
                    h = Convert.ToDouble(cornerTextDataArray[5]);

                    if (itemCode.StartsWith(BEAM_CORNER_ITEM_CODE))
                    {
                        string descriptionBase = description;// + "\n" + index.ToString() + " ";

                        Draw_BeamCorner(cornerElevation, descriptionBase, l1, l2);
                    }
                    else if (itemCode.StartsWith(CommonModule.beamInternalCornerText))
                    {
                        Draw_BeamInternalCorner(cornerElevation, description, h, l1, l2);
                    }
                    else if (itemCode.StartsWith(CommonModule.doorWindowThickTopCrnrText) || itemCode.StartsWith(CommonModule.beamThickTopCrnrText))
                    {
                        Draw_DoorWindowThickTopCorner(cornerElevation, description, h, l1, l2, cornerId, wallType);//prt

                    }
                    else if (itemCode.StartsWith(CommonModule.externalCornerText))
                    {
                        Draw_ExternalCorner(cornerElevation, description, h, l1, l2);
                    }
                    else if (itemCode.StartsWith(CommonModule.internalCornerText))
                    {
                        Draw_InternalCorner(cornerElevation, description, h, l1, l2, cornerId);
                    }
                    else if (itemCode.StartsWith(CommonModule.kickerCornerText) ||
                             itemCode.StartsWith(CommonModule.externalKickerCornerText))
                    {
                        Draw_KickerCorner(cornerElevation, description, h, l1, l2, cornerId);
                    }
                    //////Fix ECSJ Placement SDM 21_May_2022
                    //// else if (itemCode.StartsWith(CommonModule.slabJointCornerText) || itemCode.StartsWith(CommonModule.slabJointCornerConcaveText))
                    else if (itemCode.StartsWith(CommonModule.slabJointCornerText) || itemCode.StartsWith(CommonModule.slabJointCornerConcaveText) || itemCode.StartsWith(CommonModule.slabJointCornerConvexText))//RTJ 29-06-2021 for SJ corner display
                    {
                        Draw_SlabJointCorner(cornerElevation, description, h, l1, l2);
                    }
                }
            }
        }

        private void Draw_BeamCorner(CornerElevation cornerElevation, string descriptionBase, double l1, double l2)
        {
            double h = CommonModule.internalCorner;

            // string descriptionStart = descriptionBase + cornerElevation.wall1 + " Start";
            DrawCorner(cornerElevation, descriptionBase, h, true, l1,
                       CommonModule.beamCornerLayerName, CommonModule.beamCornerLayerName, CommonModule.beamCornerLayerColor,
                       startPoint_Y_Offset_Modifier: cornerElevation.elev);

            //string descriptionEnd = descriptionBase + cornerElevation.wall2 + " End";

            DrawCorner(cornerElevation, descriptionBase, h, false, l2,
                       CommonModule.beamCornerLayerName, CommonModule.beamCornerLayerName, CommonModule.beamCornerLayerColor,
                       startPoint_Y_Offset_Modifier: cornerElevation.elev);
        }

        private void Draw_BeamInternalCorner(CornerElevation cornerElevation, string description, double h, double l1, double l2)
        {
            DrawCorner(cornerElevation, description, h, true, l1,
                       CommonModule.internalCornerLyrName, CommonModule.internalCornerTextLyrName, CommonModule.internalCornerLyrColor,
                       startPoint_Y_Offset_Modifier: cornerElevation.elev);

            DrawCorner(cornerElevation, description, h, false, l2,
                       CommonModule.internalCornerLyrName, CommonModule.internalCornerTextLyrName, CommonModule.internalCornerLyrColor,
                       startPoint_Y_Offset_Modifier: cornerElevation.elev);
        }

        private void Draw_DoorWindowThickTopCorner(CornerElevation cornerElevation, string description, double h, double l1, double l2, ObjectId cornerId, string wallPanelType)
        {
            double ht = 0;
            double smallHt = 0;
            //if (AEE_Utility.CustType == eCustType.WNPanel)
            //{
            if (l1 > l2)
            {
                ht = l1;
                smallHt = l2;

                if (cornerElevation.dist2 != 0)
                {
                    ht = l2;
                    smallHt = l1;
                }
            }
            else
            {
                ht = l2;
                smallHt = l1;

                if (cornerElevation.dist2 != 0)
                {
                    ht = l1;
                    smallHt = l2;
                }
            }//Changes made on 06/07/2023 by PRT


            

            string name = cornerElevation.wall1;

            if (!WallLinesContainName(name))
                return;

            double wo = WallLines[name].Item2 + cornerElevation.dist1;
            int rw = WallLines[name].Item3;

            Point3d startPoint = BaseElevationPoint + new Vector3d(wo, cornerElevation.elev - rw * WallHeight, 0);
            Tuple<Point3d, double> tuple = new Tuple<Point3d, double>(startPoint, cornerElevation.dist2);
            PanelElevationHandler.listOfCornerStartPtData.Add(tuple);
            //ObjectId internalCorner_ObjectId =
            //    _partHelper.drawInternalCorner("", startPoint.X, startPoint.Y, l1, l2, CommonModule.panelDepth, CommonModule.internalCornerThick,
            //                                   CommonModule.panelDepth, CommonModule.internalCornerLyrName, CommonModule.internalCornerLyrColor, cornerElevation.dist2);



            ObjectId internalCorner_ObjectId =
                _partHelper.drawInternalCorner("", startPoint.X, startPoint.Y, ht, smallHt, CommonModule.panelDepth, CommonModule.internalCornerThick,
                                               CommonModule.panelDepth, CommonModule.internalCornerLyrName, CommonModule.internalCornerLyrColor, cornerElevation.dist2);

            double cornerRotationAngle = cornerElevation.dist2 + CommonModule.cornerRotationAngle;
            _cornerHelper.writeTextInCorner(internalCorner_ObjectId, description, cornerRotationAngle, CommonModule.internalCornerTextLyrName, CommonModule.internalCornerLyrColor, 1.5);

            //Draw the elevation panels twice when the door/window is INDOOR by SDM 2022-07-05
            var _wallLine1 = WallLines[name].Item1;
            var cornerBasePoint = _wallLine1.StartPoint;
            if (_wallLine1.StartPoint.Y == _wallLine1.EndPoint.Y)
                cornerBasePoint += new Vector3d(-cornerElevation.dist1, 0, 0);
            else
                cornerBasePoint += new Vector3d(0, (_wallLine1.StartPoint.Y > _wallLine1.EndPoint.Y ? -1 : 1) * cornerElevation.dist1, 0);


            // For Door End Corner is inline with Wall Panel because of that will add a CCX above the WPRX  by SDM 05-06-2022
            if (cornerElevation.wall2 != "" && !InternalWallHelper.listOfNearestCornerPoint.Contains(new Point2d(cornerBasePoint.X, cornerBasePoint.Y)))//Updated to fix an issue when two doors share the same corner by SDM 2022-08-25
            {
                string name2 = cornerElevation.wall2;

                if (!WallLinesContainName(name2))
                    return;


                var nextWall = name;
                var prevWall = name;
                InternalWallHelper.GetPreviousNextWallName(name, out nextWall, out prevWall);

                // double wo2 = WallLines[name2].Item2 + (cornerElevation.dist2 == 0 ? WallLines[name2].Item1.Length : (2 * l2) - h);
                var wallLine2 = WallLines[name2].Item1;
                bool isBegining = (nextWall == name2 || prevWall == name2) ? nextWall == name2 : wallLine2.StartPoint.DistanceTo(cornerBasePoint) < wallLine2.EndPoint.DistanceTo(cornerBasePoint);
                double wo2 = WallLines[name2].Item2 + (!isBegining ? WallLines[name2].Item1.Length - CommonModule.internalCorner : -(h - CommonModule.internalCorner));
                int rw2 = WallLines[name2].Item3;

                //Vector3d vector3dOffset = Vector3d.YAxis * l2;//Commented on 05/07/2023 by PRT
                Vector3d vector3dOffset = Vector3d.YAxis * CommonModule.doorWindowTopCornerHt;  //Changes made on 05/07/2023 by PRT
                double startPoint_Y_Offset = cornerElevation.elev - rw2 * WallHeight;

                //Point3d startPoint2 = BaseElevationPoint + new Vector3d(wo2, startPoint_Y_Offset - l2, 0);//Commented on 05/07/2023 by PRT
                Point3d startPoint2 = BaseElevationPoint + new Vector3d(wo2, startPoint_Y_Offset - CommonModule.doorWindowTopCornerHt, 0);
                Point3d offsetStartPoint = startPoint2 + vector3dOffset;


                Point3d endPoint = startPoint2 + new Vector3d(h, 0, 0);
                Point3d offsetEndPoint = endPoint + vector3dOffset;

                ObjectId cornerRec = AEE_Utility.CreateRectangle(startPoint2, endPoint, offsetEndPoint, offsetStartPoint, CommonModule.internalCornerLyrName, CommonModule.internalCornerLyrColor);

                //Point3d textPoint = GetTextPoint(startPoint2, h, l2);//Commented on 05/07/2023 by PRT
                Point3d textPoint = GetTextPoint(startPoint2, h, CommonModule.doorWindowTopCornerHt);
                AEE_Utility.CreateMultiLineText(description, textPoint, 20, CommonModule.internalCornerTextLyrName, CommonModule.internalCornerLyrColor, 0, 1.5);

                //Changes made on 05/07/2023 by PRT

            }



            KeyValuePair<string, Tuple<Line, double, int, string>> backWall = new KeyValuePair<string, Tuple<Line, double, int, string>>();
            double offset = 0;//Added to fix an issue when two doors share the same corner by SDM 2022-08-25
            bool isInDoor = CheckWallIsIndoor(name, Convert.ToDouble(description.Split(' ')[4]), cornerId, out backWall, cornerBasePoint, out offset);
            if (isInDoor)
            {
                var wo2 = backWall.Value.Item2;
                var rw2 = backWall.Value.Item3;
                var _wallLine2 = backWall.Value.Item1;

                double distance = _wallLine2.StartPoint.DistanceTo(_wallLine2.GetClosestPointTo(cornerBasePoint, true));
                //Math.Min(_wallLine2.StartPoint.DistanceTo(_wallLine2.GetClosestPointTo(_line.StartPoint, true)),                 _wallLine2.StartPoint.DistanceTo(_wallLine2.GetClosestPointTo(_line.EndPoint, true)));

                double angleShift = cornerElevation.dist2 == 0 || cornerElevation.dist2 == 180 ? -90 : 90;

                if (l1 > l2)
                {
                   

                    if (cornerElevation.dist2 + angleShift == -cornerElevation.dist2 + angleShift)
                    {
                        ht = l2;
                        smallHt = l1;
                    }
                    else
                    {
                        ht = l1;
                        smallHt = l2;
                    }
                }
                else
                {


                    if (cornerElevation.dist2 + angleShift == -cornerElevation.dist2 + angleShift)
                    {
                        ht = l1;
                        smallHt = l2;
                    }
                    else
                    {
                        ht = l2;
                        smallHt = l1;
                    }
                }//Changes made on 06/07/2023 by PRT

                double rc = 0;
                if (SunkanSlabHelper.listOfWallName_SunkanSlabId.ContainsKey(backWall.Key))
                    rc = PanelLayout_UI.RC;
                else if (SunkanSlabHelper.listOfWallName_SunkanSlabId.ContainsKey(name))
                    rc = -PanelLayout_UI.RC;
                Point3d startPoint2 = BaseElevationPoint + new Vector3d(wo2 + distance, cornerElevation.elev - rc - rw2 * WallHeight, 0);
                ObjectId internalCorner_ObjectId2 =
                _partHelper.drawInternalCorner("", startPoint2.X, startPoint2.Y, ht, smallHt, CommonModule.panelDepth, CommonModule.internalCornerThick,
                                               CommonModule.panelDepth, CommonModule.internalCornerLyrName, CommonModule.internalCornerLyrColor, cornerElevation.dist2 + angleShift);

                //double cornerRotationAngle = cornerElevation.dist2 + CommonModule.cornerRotationAngle;
                _cornerHelper.writeTextInCorner(internalCorner_ObjectId2, description, cornerRotationAngle + angleShift, CommonModule.internalCornerTextLyrName, CommonModule.internalCornerLyrColor, 1.5);

                //Added to fix an issue when two doors share the same corner by SDM 2022-08-25
                bool insctDoors = false;
                for (int i = 0; i < InternalWallHelper.listOfNearestCornerPoint.Count; i++)
                {
                    var pt = InternalWallHelper.listOfNearestCornerPoint[i];
                    if (pt.GetDistanceTo(new Point2d(cornerBasePoint.X, cornerBasePoint.Y)) <= offset)
                    {
                        insctDoors = true;
                        break;
                    }
                }
                //Added to fix an issue when two doors share the same corner by SDM 2022-08-25
                if (!insctDoors && h > CommonModule.minPanelThick && wallPanelType == CommonModule.doorWallPanelType && (Math.Round(distance) == 0 || Math.Round(distance) == Math.Round(_wallLine2.Length)))
                {
                    var nextWall = name;
                    var prevWall = name;
                    InternalWallHelper.GetPreviousNextWallName(backWall.Key, out nextWall, out prevWall);

                    bool isBegining = Math.Round(distance) == 0;
                    var name4 = isBegining ? prevWall : nextWall;
                    var wallLine4 = WallLines[name4].Item1;
                    double wo4 = WallLines[name4].Item2 + (isBegining ? WallLines[name4].Item1.Length - CommonModule.internalCorner : -(h - CommonModule.internalCorner));
                    int rw4 = WallLines[name4].Item3;

                    Vector3d vector3dOffset = Vector3d.YAxis * CommonModule.doorWindowTopCornerHt;
                    double startPoint_Y_Offset = cornerElevation.elev - rw4 * WallHeight;

                    Point3d startPoint4 = BaseElevationPoint + new Vector3d(wo4, startPoint_Y_Offset - CommonModule.doorWindowTopCornerHt, 0);
                    Point3d offsetStartPoint = startPoint4 + vector3dOffset;


                    Point3d endPoint = startPoint4 + new Vector3d(h, 0, 0);
                    Point3d offsetEndPoint = endPoint + vector3dOffset;

                    ObjectId cornerRec = AEE_Utility.CreateRectangle(startPoint4, endPoint, offsetEndPoint, offsetStartPoint, CommonModule.internalCornerLyrName, CommonModule.internalCornerLyrColor);

                    Point3d textPoint = GetTextPoint(startPoint4, h, CommonModule.doorWindowTopCornerHt);
                    AEE_Utility.CreateMultiLineText(description, textPoint, 20, CommonModule.internalCornerTextLyrName, CommonModule.internalCornerLyrColor, 0, 1.5);
                }

            }
        }



        private void Draw_ExternalCorner(CornerElevation cornerElevation, string description, double h, double l1, double l2)
        {
            description = description.Replace("\n", "");
            DrawCorner(cornerElevation, description, h, true, l1,
                       CommonModule.externalCornerLyrName, CommonModule.externalCornerTextLyrName, CommonModule.externalCornerLyrColor,
                       startPoint_Y_Offset_Modifier: cornerElevation.elev); //Fix elevation when wall Ht more than 3000 by SDM 2022-08-06

            DrawCorner(cornerElevation, description, h, false, l2,
                       CommonModule.externalCornerLyrName, CommonModule.externalCornerTextLyrName, CommonModule.externalCornerLyrColor,
                       woModifier: -CommonModule.externalCorner,
                       startPoint_Y_Offset_Modifier: cornerElevation.elev); //Fix elevation when wall Ht more than 3000 by SDM 2022-08-06
        }

        private void Draw_InternalCorner(CornerElevation cornerElevation, string description, double h, double l1, double l2, ObjectId cornerId)
        {
            //DrawCorner(cornerElevation, description, h, true, l1,
            //           CommonModule.internalCornerLyrName, CommonModule.internalCornerTextLyrName, CommonModule.internalCornerLyrColor,
            //           woModifier: -l1,
            //           startPoint_Y_Offset_Modifier: cornerElevation.elev);//Commented by RTJ 18-06-21

            bool flagOfWindowInsideCorner1 = false;
            bool flagOfWindowInsideCorner2 = false;
            List<object> intersectionInfo = new List<object>();
            if (CornerHelper.listOfInternalCornerId_InsctToWindow.ContainsKey(cornerId))
            {
                intersectionInfo = CornerHelper.listOfInternalCornerId_InsctToWindow[cornerId];
                flagOfWindowInsideCorner1 = intersectionInfo[0].ToString() == cornerElevation.wall1;
                flagOfWindowInsideCorner2 = intersectionInfo[0].ToString() == cornerElevation.wall2;
            }

            if (flagOfWindowInsideCorner1)
            {
                if (cornerElevation.elev == 0)// Fix IAX elevation when the floor height more than 3000 by SDM 2022 - 08 - 08
                {

                    double bottomPanelHt = Convert.ToDouble(intersectionInfo[1]);
                    double topPanelHt = Convert.ToDouble(intersectionInfo[2]);
                    double elev = Convert.ToDouble(intersectionInfo[3]);

                    string desc1 = description.Replace(description.Split(' ')[4], bottomPanelHt.ToString());
                    DrawCorner(cornerElevation, desc1, bottomPanelHt, true, l1,
                              CommonModule.internalCornerLyrName, CommonModule.internalCornerTextLyrName, CommonModule.internalCornerLyrColor,
                              woModifier: -l1,
                              startPoint_Y_Offset_Modifier: cornerElevation.elev);

                    string desc2 = description.Replace(description.Split(' ')[4], topPanelHt.ToString());
                    DrawCorner(cornerElevation, desc2, topPanelHt, true, l1,
                             CommonModule.internalCornerLyrName, CommonModule.internalCornerTextLyrName, CommonModule.internalCornerLyrColor,
                             woModifier: -l1,
                             startPoint_Y_Offset_Modifier: elev);
                }
            }
            else
                DrawCorner(cornerElevation, description, h, true, l1,
                           CommonModule.internalCornerLyrName, CommonModule.internalCornerTextLyrName, CommonModule.internalCornerLyrColor,
                           woModifier: -l1,
                           startPoint_Y_Offset_Modifier: cornerElevation.elev); //Modified by RTJ 18 - 06 - 21

            if (flagOfWindowInsideCorner2)
            {
                if (cornerElevation.elev == 0)//Fix IAX elevation when the floor height more than 3000 by SDM 2022-08-08
                {
                    double bottomPanelHt = Convert.ToDouble(intersectionInfo[1]);
                    double topPanelHt = Convert.ToDouble(intersectionInfo[2]);
                    double elev = Convert.ToDouble(intersectionInfo[3]);

                    string desc1 = description.Replace(description.Split(' ')[4], bottomPanelHt.ToString());
                    DrawCorner(cornerElevation, desc1, bottomPanelHt, false, l2,
                           CommonModule.internalCornerLyrName, CommonModule.internalCornerTextLyrName, CommonModule.internalCornerLyrColor,
                           startPoint_Y_Offset_Modifier: cornerElevation.elev);

                    string desc2 = description.Replace(description.Split(' ')[4], topPanelHt.ToString());
                    DrawCorner(cornerElevation, desc2, topPanelHt, false, l2,
                           CommonModule.internalCornerLyrName, CommonModule.internalCornerTextLyrName, CommonModule.internalCornerLyrColor,
                           startPoint_Y_Offset_Modifier: elev);
                }
            }
            else
                DrawCorner(cornerElevation, description, h, false, l2,
                       CommonModule.internalCornerLyrName, CommonModule.internalCornerTextLyrName, CommonModule.internalCornerLyrColor,
                       startPoint_Y_Offset_Modifier: cornerElevation.elev);

        }

        private void Draw_KickerCorner(CornerElevation cornerElevation, string description, double h, double l1, double l2, ObjectId kickerId)
        {
            string name = cornerElevation.wall1;

            if (!WallLinesContainName(name))
                return;

            double wo = WallLines[name].Item2 + cornerElevation.dist1;
            int rw = WallLines[name].Item3;

            Point3d startPoint = BaseElevationPoint + new Vector3d(wo, -h - rw * WallHeight, 0);
            Point3d endPoint = startPoint + new Vector3d(l1, 0, 0);

            if (AEE_Utility.CustType == eCustType.WNPanel && (KickerHelper.listOfKickerCornerId_In_ConcaveAngle.Contains(kickerId)))
            {

                startPoint = BaseElevationPoint + new Vector3d(wo - 65, -h - rw * WallHeight, 0);
                endPoint = startPoint + new Vector3d(l1 + 65, 0, 0);//Changes made on 03/07/2023 by PRT
            }


            Vector3d vector3dOffset = Vector3d.YAxis * h;
            Point3d offsetStartPoint = startPoint + vector3dOffset;
            Point3d offsetEndPoint = endPoint + vector3dOffset;

            AEE_Utility.CreateRectangle(startPoint, endPoint, offsetEndPoint, offsetStartPoint, CommonModule.kickerCornerLayerName, CommonModule.kickerCornerLayerColor);

            Point3d textPoint = GetTextPoint(startPoint, l1, h);
            //Scale text size by 1.5 by SDM 31_May_2022
            AEE_Utility.CreateMultiLineText(description, textPoint, 20, CommonModule.kickerCornerTextLayerName, CommonModule.kickerCornerLayerColor, 0, 1.5);

            Vector3d topkickerlvl = new Vector3d(0, cornerElevation.elev + h, 0);

            //Remove the top kicker panels for Internal Sunkan by SDM 2022-07-17
            if (!name.Contains("_R_"))
            {
                startPoint += topkickerlvl;
                offsetStartPoint += topkickerlvl;
                endPoint += topkickerlvl;
                offsetEndPoint += topkickerlvl;
                AEE_Utility.CreateRectangle(startPoint, endPoint, offsetEndPoint, offsetStartPoint, CommonModule.kickerCornerLayerName, CommonModule.kickerCornerLayerColor);

                textPoint = new Point3d(textPoint.X, textPoint.Y + cornerElevation.elev + h, textPoint.Z);
                //Scale text size by 1.5 by SDM 31_May_2022
                AEE_Utility.CreateMultiLineText(description, textPoint, 20, CommonModule.kickerCornerTextLayerName, CommonModule.kickerCornerLayerColor, 0, 1.5);
            }

            name = cornerElevation.wall2;

            if (!WallLinesContainName(name))
                return;

            wo = WallLines[name].Item2 + cornerElevation.dist2;
            rw = WallLines[name].Item3;

            startPoint = BaseElevationPoint + new Vector3d(wo, -h - rw * WallHeight, 0);
            endPoint = startPoint + new Vector3d(l2, 0, 0);

            if (AEE_Utility.CustType == eCustType.WNPanel && (KickerHelper.listOfKickerCornerId_In_ConcaveAngle.Contains(kickerId)))
            {
                endPoint = startPoint + new Vector3d(l2 + 65, 0, 0);//Changes made on 03/07/2023 by PRT
            }

            vector3dOffset = Vector3d.YAxis * h;
            offsetEndPoint = endPoint + vector3dOffset;
            offsetStartPoint = startPoint + vector3dOffset;

            AEE_Utility.CreateRectangle(startPoint, endPoint, offsetEndPoint, offsetStartPoint, CommonModule.kickerCornerLayerName, CommonModule.kickerCornerLayerColor);

            textPoint = GetTextPoint(startPoint, l2, h);
            //Scale text size by 1.5 by SDM 31_May_2022
            AEE_Utility.CreateMultiLineText(description, textPoint, 20, CommonModule.kickerCornerLayerName, CommonModule.kickerCornerLayerColor, 0, 1.5);

            //Remove the top kicker panels for Internal Sunkan by SDM 2022-07-17
            if (!name.Contains("_R_"))
            {
                startPoint += topkickerlvl;
                offsetStartPoint += topkickerlvl;
                endPoint += topkickerlvl;
                offsetEndPoint += topkickerlvl;
                AEE_Utility.CreateRectangle(startPoint, endPoint, offsetEndPoint, offsetStartPoint, CommonModule.kickerCornerLayerName, CommonModule.kickerCornerLayerColor);

                textPoint = new Point3d(textPoint.X, textPoint.Y + cornerElevation.elev + h, textPoint.Z);
                //Scale text size by 1.5 by SDM 31_May_2022
                AEE_Utility.CreateMultiLineText(description, textPoint, 20, CommonModule.kickerCornerTextLayerName, CommonModule.kickerCornerLayerColor, 0, 1.5);
            }
        }

        private void Draw_SlabJointCorner(CornerElevation cornerElevation, string description, double h, double l1, double l2)
        {
            //Fix CSJ position when intersects with beam by SDM 06-06-2022
            //start
            double woModifier1 = 0;
            double woModifier2 = 0;

            double distBeamToWall1 = 0;//Added to fix CSJ position by SDM 2022-08-14
            double distBeamToWall2 = 0;//Added to fix CSJ position by SDM 2022-08-14
            bool isWall1HasBeam = false;
            bool isWall2HasBeam = false;
            //if (Math.Abs(int.Parse(cornerElevation.wall2.Split('_')[0].Replace("W", "")) - int.Parse(cornerElevation.wall1.Split('_')[0].Replace("W", ""))) != 1)
            for (int j = 0; j < BeamHelper.listOfListOfBeamBottomPanelLineId.Count; j++)
            {
                for (int k = 0; k < BeamHelper.listOfListOfBeamBottomPanelLineId[j].Count; k++)
                {
                    var beamWall = AEE_Utility.GetXDataRegisterAppName(BeamHelper.listOfListOfBeamBottomPanelLineId[j][k]);
                    if (!isWall1HasBeam && beamWall == cornerElevation.wall1)
                    {
                        isWall1HasBeam = true;
                        distBeamToWall1 = BeamHelper.listOfListOfDistBtwBeamPanelLineToWallLine[j][k];//Added to fix CSJ position by SDM 2022-08-14
                    }
                    if (!isWall2HasBeam && beamWall == cornerElevation.wall2)
                    {
                        isWall2HasBeam = true;
                        distBeamToWall2 = BeamHelper.listOfListOfDistBtwBeamPanelLineToWallLine[j][k];//Added to fix CSJ position by SDM 2022-08-14
                    }
                }
            }

            if (isWall2HasBeam && isWall1HasBeam)
            {
                if (cornerElevation.dist2 == 0)
                    woModifier2 = distBeamToWall1;
                else
                    woModifier1 = -distBeamToWall2;
            }
            else if (isWall1HasBeam && cornerElevation.dist2 == 0)
            {
                woModifier2 = distBeamToWall1;
            }
            else if (isWall2HasBeam)
            {
                for (int i = 0; i < InternalWallHelper.listOfListOfInternalWallRoomLineId_With_WallName.Count; i++)
                {
                    for (int j = 0; j < InternalWallHelper.listOfListOfInternalWallRoomLineId_With_WallName[i].Count; j++)
                    {
                        var name = AEE_Utility.GetXDataRegisterAppName(InternalWallHelper.listOfListOfInternalWallRoomLineId_With_WallName[i][j]);
                        if (name == cornerElevation.wall1)
                        {
                            var line1 = AEE_Utility.GetLengthOfLine(InternalWallHelper.listOfListOfInternalWallRoomLineId_With_WallName[i][j]);
                            if (cornerElevation.dist1 != line1 - l1 - distBeamToWall2)
                                woModifier1 = -distBeamToWall2;
                        }
                    }
                }

            }
            //end
            DrawCorner(cornerElevation, description, h, true, l1,
                       CommonModule.slabJointCornerLayerName, CommonModule.slabJointCornerTextLayerName, CommonModule.slabJointCornerLayerColor, woModifier: woModifier1,
                       startPoint_Y_Offset_Modifier: cornerElevation.elev);

            DrawCorner(cornerElevation, description, h, false, l2,
                       CommonModule.slabJointCornerLayerName, CommonModule.slabJointCornerTextLayerName, CommonModule.slabJointCornerLayerColor, woModifier: woModifier2,
                       startPoint_Y_Offset_Modifier: cornerElevation.elev);
        }

        private void DrawCorner(
            CornerElevation cornerElevation,
            string description,
            double h,
            bool isWall1, // true => isWall1; false => isWall2
            double l,
            string cornerLayerName,
            string cornerTextLayerName,
            int cornerLayerColor,
            double woModifier = 0d,
            double startPoint_Y_Offset_Modifier = 0d
        )
        {
            string name = isWall1 ? cornerElevation.wall1 : cornerElevation.wall2;

            if (!WallLinesContainName(name))
                return;

            woModifier += isWall1 ? cornerElevation.dist1 : cornerElevation.dist2;
            double wo = WallLines[name].Item2 + woModifier;
            ////Fix ECSJ Placement SDM 21_May_2022
            //start
            if (description.StartsWith(CommonModule.slabJointCornerConvexText))
            {
                if (isWall1)
                    wo += CommonModule.slabJointPanelDepth;
                else
                    l -= CommonModule.slabJointPanelDepth;
            }
            //end

            int rw = WallLines[name].Item3;

            Vector3d vector3dOffset = Vector3d.YAxis * h;
            double startPoint_Y_Offset = startPoint_Y_Offset_Modifier - rw * WallHeight;

            Point3d startPoint = BaseElevationPoint + new Vector3d(wo, startPoint_Y_Offset, 0);
            Point3d offsetStartPoint = startPoint + vector3dOffset;

            Point3d endPoint = startPoint + new Vector3d(l, 0, 0);
            Point3d offsetEndPoint = endPoint + vector3dOffset;

            ObjectId cornerRec = AEE_Utility.CreateRectangle(startPoint, endPoint, offsetEndPoint, offsetStartPoint, cornerLayerName, cornerLayerColor);

            Point3d textPoint = GetTextPoint(startPoint, l, h);
            double textAngle = description.Contains(CommonModule.internalCornerText) || description.Contains(CommonModule.externalCornerText) || description.Contains(CommonModule.beamInternalCornerText) ? 90 : 0;
            AEE_Utility.CreateMultiLineText(description, textPoint, 20, cornerTextLayerName, cornerLayerColor, textAngle, 1.5);
        }
    }
}
