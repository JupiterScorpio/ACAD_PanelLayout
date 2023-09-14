using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using PanelLayout_App.CivilModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanelLayout_App.WallModule
{
    public class ParapetHelper
    {
        public static List<ObjectId> listOfParapetObjId = new List<ObjectId>();

        public static List<ObjectId> listOfParapetId_WithWallLine = new List<ObjectId>();
        public static List<ObjectId> listOfWallLineId_InsctToParapet = new List<ObjectId>();

        public static List<ObjectId> listOfParapetId_WithWindowWallLine = new List<ObjectId>();
        public static List<ObjectId> listOfWindowWallLineId_InsctToParapet = new List<ObjectId>();

        public static List<ObjectId> listOfParapetId_WithDoorWallLine = new List<ObjectId>();
        public static List<ObjectId> listOfDoorWallLineId_InsctToParapet = new List<ObjectId>();

        public static List<ObjectId> listOfCornerId_InsctToTwoParapet = new List<ObjectId>();
        public static List<List<ObjectId>> listOfListOfTwoParapetId_InsctToCommonCorner = new List<List<ObjectId>>();
        public static List<string> listOfTwoParapetId_InsctToCommonCorner_ForCheck = new List<string>();

        public static List<ObjectId> listOfAllCornersId_InsctToParapet = new List<ObjectId>();
        public static List<ObjectId> listOfParapetId_InsctCorner = new List<ObjectId>();
        public static List<ObjectId> listOfWallLineId_AvailableInParapet_InsctCorner = new List<ObjectId>();
        public static List<ObjectId> listOfSunkandId_AvailableInParapet_InsctCorner = new List<ObjectId>();

  
        public static List<ObjectId> listOfParapetWallLineId_WithDoor = new List<ObjectId>();
        public static List<ObjectId> listOfParapetWallOffsetLineId_WithDoor = new List<ObjectId>();
        public static List<ObjectId> listOfParapetWallLineCornerId_WithDoor = new List<ObjectId>();

        public ObjectId checkParapetIsAvailableInWallPanel(ObjectId wallPanelLineId, ObjectId cornerId1, ObjectId cornerId2, List<ObjectId> cornerId1_TextId, List<ObjectId> cornerId2_TextId, ObjectId anotherParapetId_InsctToParapet, ObjectId sunkanSlabId)
        {
            anotherParapetId_InsctToParapet = new ObjectId();
            ObjectId parapetWallId = new ObjectId();  
            for (int k = 0; k < listOfParapetObjId.Count; k++)
            {
                ObjectId parapetId = listOfParapetObjId[k];
                BeamHelper beamHlp = new BeamHelper();
                var flagOfParapet = beamHlp.checkPointIsInsideTheBoundary(wallPanelLineId, parapetId);
                if (flagOfParapet == true)
                {
                    parapetWallId = parapetId;
                    checkParapetIsIntersectToAnotherParapet(parapetWallId, listOfParapetObjId, cornerId1, cornerId2, out anotherParapetId_InsctToParapet);
                    break;
                }
            }

            if (parapetWallId.IsValid == true)
            {
                changeHeightOfCornerWithParapet(wallPanelLineId, parapetWallId, anotherParapetId_InsctToParapet, cornerId1, cornerId2, cornerId1_TextId, cornerId2_TextId, sunkanSlabId);
            }

            return parapetWallId;
        }


        private void checkParapetIsIntersectToAnotherParapet(ObjectId crrntParapetId, List<ObjectId> listOfAllParapetId, ObjectId cornerId1, ObjectId cornerId2, out ObjectId anotherParapetId_InsctToParapet)
        {
            anotherParapetId_InsctToParapet = new ObjectId();
       
            double offsetValue = WindowHelper.windowOffsetValue;
            var listOfCrrntParapetVertices = AEE_Utility.GetPolylineVertexPoint(crrntParapetId);
            double crrntParapet_MaxX = listOfCrrntParapetVertices.Max(sortPoint => sortPoint.X);
            double crrntParapet_MaxY = listOfCrrntParapetVertices.Max(sortPoint => sortPoint.Y);
            Point3d offsetPoint = new Point3d((offsetValue + crrntParapet_MaxX), (offsetValue + crrntParapet_MaxY), 0);
            var offsetCrrntParapetId = AEE_Utility.OffsetEntity_WithoutLine(offsetValue, offsetPoint, crrntParapetId, false);

            for (int i = 0; i < listOfAllParapetId.Count; i++)
            {
                ObjectId parapetId = listOfAllParapetId[i];
                if (parapetId != crrntParapetId)
                {
                    var listOfParapetVertices = AEE_Utility.GetPolylineVertexPoint(parapetId);
                    double parapet_MaxX = listOfParapetVertices.Max(sortPoint => sortPoint.X);
                    double parapet_MaxY = listOfParapetVertices.Max(sortPoint => sortPoint.Y);
                    Point3d parapetOffsetPoint = new Point3d((offsetValue + parapet_MaxX), (offsetValue + parapet_MaxY), 0);
                    var offsetParapetId = AEE_Utility.OffsetEntity_WithoutLine(offsetValue, parapetOffsetPoint, parapetId, false);

                    var listOfInsctPoint = AEE_Utility.InterSectionBetweenTwoEntity(cornerId1, offsetParapetId);
                    if (listOfInsctPoint.Count != 0)
                    {
                        anotherParapetId_InsctToParapet = parapetId;
                        break;
                    }
                    else
                    {
                        listOfInsctPoint = AEE_Utility.InterSectionBetweenTwoEntity(cornerId2, offsetParapetId);
                        if (listOfInsctPoint.Count != 0)
                        {
                            anotherParapetId_InsctToParapet = parapetId;
                            break;
                        }
                    }
                    AEE_Utility.deleteEntity(offsetParapetId);
                }
            }
            AEE_Utility.deleteEntity(offsetCrrntParapetId);
        }

        private void changeHeightOfCornerWithParapet(ObjectId wallPanelLineId, ObjectId parapetWallId, ObjectId anotherParapetId_InsctToParapet, ObjectId cornerId1, ObjectId cornerId2,List<ObjectId> cornerId1_TextId,List<ObjectId> cornerId2_TextId, ObjectId sunkanSlabId)
        {
            var wallPanelLine = AEE_Utility.GetEntityForRead(wallPanelLineId);
            string parapetId_InStr1 = Convert.ToString(parapetWallId) + "," + Convert.ToString(anotherParapetId_InsctToParapet);
            string parapetId_InStr2 = Convert.ToString(anotherParapetId_InsctToParapet) + "," + Convert.ToString(parapetWallId);

            CornerHelper cornerHelper = new CornerHelper();

            var parapetWallHeight = ParapetHelper.getParapetCornerHeight(parapetWallId, sunkanSlabId, wallPanelLine.Layer);

            bool flagOfNearestCornerId1 = false;
            bool flagOfNearestCornerId2 = false;
            getNearestCorner(anotherParapetId_InsctToParapet, cornerId1, cornerId2, out flagOfNearestCornerId1, out flagOfNearestCornerId2);

            List<ObjectId> listOfParapetId_InsctToCommonCorner = new List<ObjectId>();
            if (flagOfNearestCornerId1 == true)
            {
                var anotherParapetWallHeight = ParapetHelper.getParapetCornerHeight(anotherParapetId_InsctToParapet, sunkanSlabId, wallPanelLine.Layer);
                parapetWallHeight = Math.Min(parapetWallHeight, anotherParapetWallHeight);

                if (anotherParapetId_InsctToParapet.IsValid == true)
                {
                    if (listOfTwoParapetId_InsctToCommonCorner_ForCheck.Contains(parapetId_InStr1) || listOfTwoParapetId_InsctToCommonCorner_ForCheck.Contains(parapetId_InStr2))
                    {

                    }
                    else
                    {
                        listOfTwoParapetId_InsctToCommonCorner_ForCheck.Add(parapetId_InStr1);
                        ParapetHelper.listOfCornerId_InsctToTwoParapet.Add(cornerId1);
                        listOfParapetId_InsctToCommonCorner.Add(parapetWallId);
                        listOfParapetId_InsctToCommonCorner.Add(anotherParapetId_InsctToParapet);
                        ParapetHelper.listOfListOfTwoParapetId_InsctToCommonCorner.Add(listOfParapetId_InsctToCommonCorner);
                    }     
                }
            }
            List<List<object>> listOfCornerData_ForBOM = new List<List<object>>();
            List<String> corner1_Texts = getCornerHeightOfParapet(cornerId1, parapetWallHeight, out listOfCornerData_ForBOM);

            for (int i=0;i< corner1_Texts.Count;i++)//Chnages made on 07/05/2023 by SDM
            {
                AEE_Utility.changeMText(cornerId1_TextId[i], corner1_Texts[i]);
            }

 
            if (flagOfNearestCornerId2 == true)
            {
                var anotherParapetWallHeight = ParapetHelper.getParapetCornerHeight(anotherParapetId_InsctToParapet, sunkanSlabId, wallPanelLine.Layer);
                parapetWallHeight = Math.Min(parapetWallHeight, anotherParapetWallHeight);

                if (anotherParapetId_InsctToParapet.IsValid == true)
                {
                    if (listOfTwoParapetId_InsctToCommonCorner_ForCheck.Contains(parapetId_InStr1) || listOfTwoParapetId_InsctToCommonCorner_ForCheck.Contains(parapetId_InStr2))
                    {

                    }
                    else
                    {
                        listOfTwoParapetId_InsctToCommonCorner_ForCheck.Add(parapetId_InStr1);
                        ParapetHelper.listOfCornerId_InsctToTwoParapet.Add(cornerId2);
                        listOfParapetId_InsctToCommonCorner.Add(parapetWallId);
                        listOfParapetId_InsctToCommonCorner.Add(anotherParapetId_InsctToParapet);
                        ParapetHelper.listOfListOfTwoParapetId_InsctToCommonCorner.Add(listOfParapetId_InsctToCommonCorner);
                    }                
                }                
            }

            List<String> corner2_Texts = getCornerHeightOfParapet(cornerId2, parapetWallHeight, out listOfCornerData_ForBOM);

            for (int i = 0; i < corner2_Texts.Count; i++)//Chnages made on 07/05/2023 by SDM
            {
                AEE_Utility.changeMText(cornerId2_TextId[i], corner2_Texts[i]);
            }

            listOfAllCornersId_InsctToParapet.Add(cornerId1);
            listOfParapetId_InsctCorner.Add(parapetWallId);
            listOfSunkandId_AvailableInParapet_InsctCorner.Add(sunkanSlabId);
            listOfWallLineId_AvailableInParapet_InsctCorner.Add(wallPanelLineId);

            listOfAllCornersId_InsctToParapet.Add(cornerId2);
            listOfParapetId_InsctCorner.Add(parapetWallId);
            listOfSunkandId_AvailableInParapet_InsctCorner.Add(sunkanSlabId);
            listOfWallLineId_AvailableInParapet_InsctCorner.Add(wallPanelLineId);
        }

        private void getNearestCorner(ObjectId anotherParapetId_InsctToParapet, ObjectId cornerId1, ObjectId cornerId2, out bool flagOfNearestCornerId1, out bool flagOfNearestCornerId2)
        {
            flagOfNearestCornerId1 = false;
            flagOfNearestCornerId2 = false;
            if (anotherParapetId_InsctToParapet.IsValid == true)
            {
                var corner1_BasePoint = AEE_Utility.GetBasePointOfPolyline(cornerId1);
                var corner2_BasePoint = AEE_Utility.GetBasePointOfPolyline(cornerId2);

                List<double> listOfCorner1Lngth = new List<double>();
                List<double> listOfCorner2Lngth = new List<double>();

                var listOfPolyLineVertice = AEE_Utility.GetPolylineVertexPoint(anotherParapetId_InsctToParapet);
                foreach (var vertice in listOfPolyLineVertice)
                {
                    var length1 = AEE_Utility.GetLengthOfLine(vertice.X, vertice.Y, corner1_BasePoint.X, corner1_BasePoint.Y);
                    listOfCorner1Lngth.Add(length1);

                    var length2 = AEE_Utility.GetLengthOfLine(vertice.X, vertice.Y, corner2_BasePoint.X, corner2_BasePoint.Y);
                    listOfCorner2Lngth.Add(length2);
                }

                double minLengthOfCorner1 = listOfCorner1Lngth.Min();

                double minLengthOfCorner2 = listOfCorner2Lngth.Min();

                double minLength = Math.Min(minLengthOfCorner1, minLengthOfCorner2);

                if (minLengthOfCorner1 == minLength)
                {
                    flagOfNearestCornerId1 = true;
                }
                if (minLengthOfCorner2 == minLength)
                {
                    flagOfNearestCornerId2 = true;
                }
            }
        }

        public void changeHeightOfWindowOrDoorCornerWithParapet(string xDataRegAppName, ObjectId parapetWallId, ObjectId cornerId,List<ObjectId> cornerTextIds, ObjectId sunkanSlabId, string wallPanelLine, out bool flagOfParapet)
        {
            flagOfParapet = false;
            CornerHelper cornerHelper = new CornerHelper();

            var parapetWallHeight = ParapetHelper.getParapetCornerHeight(parapetWallId, sunkanSlabId, wallPanelLine);
            if (parapetWallHeight > 0)
            {
                List<List<object>> listOfCornerData_ForBOM = new List<List<object>>();

                List<string> corner1_Texts = getCornerHeightOfParapet(cornerId, parapetWallHeight, out listOfCornerData_ForBOM);

                for (int i = 0; i < listOfCornerData_ForBOM.Count; i++)
                {
                    string cornerDescp = Convert.ToString(listOfCornerData_ForBOM[0]);
                    string cornerType = Convert.ToString(listOfCornerData_ForBOM[1]);
                    double flange1 = Convert.ToDouble(listOfCornerData_ForBOM[2]);
                    double flange2 = Convert.ToDouble(listOfCornerData_ForBOM[3]);
                    double cornerWallHeight = parapetWallHeight;

                    AEE_Utility.changeMText(cornerTextIds[i], corner1_Texts[i]);

                    CornerHelper.setCornerDataForBOM(xDataRegAppName, cornerId, cornerTextIds[i], corner1_Texts[i], cornerDescp, cornerType, flange1, flange2, cornerWallHeight/*, 0.0/*elev*/);
                    flagOfParapet = true;
                }
            }
        }

        private List<string> getCornerHeightOfParapet(ObjectId cornerId, double cornerWallHeight, out List<List<object>> listOfCornerData_ForBOM)
        {
            listOfCornerData_ForBOM = new List<List<object>>();

            List<string> cornerTexts = new List<string>();

            var cornerEnt = AEE_Utility.GetEntityForRead(cornerId);

            if (cornerEnt.Layer == CommonModule.internalCornerLyrName)
            {
                cornerTexts = CornerHelper.UpdateCornerText(listOfCornerData_ForBOM, cornerWallHeight, new ObjectId());
            }
            else if (cornerEnt.Layer == CommonModule.externalCornerLyrName)
            {
                cornerTexts = CornerHelper.UpdateCornerTextExternal(listOfCornerData_ForBOM, cornerWallHeight);               
            }

            return cornerTexts;
        }

        public List<ObjectId> splitWallPanelLineWithParapet(List<ObjectId> listOfWallId, ObjectId parapetId, out List<ObjectId> listOfWallLineWithoutParapetId)
        {
            List<ObjectId> listOfAllWallLineId = new List<ObjectId>();
            listOfWallLineWithoutParapetId = new List<ObjectId>();

            if (parapetId.IsValid == true)
            {              
                foreach (var wallLineId in listOfWallId)
                {
                    var listOfInsctPoint = AEE_Utility.InterSectionBetweenTwoEntity(parapetId, wallLineId);
                    if (listOfInsctPoint.Count != 0)
                    {
                        Entity wallLineEnt = AEE_Utility.GetEntityForRead(wallLineId);

                        List<Point3d> listOfAllPoint = new List<Point3d>();
                        foreach (var insctPoint in listOfInsctPoint)
                        {
                            listOfAllPoint.Add(insctPoint);
                        }

                        List<Point3d> listOfStrtEndPoint = AEE_Utility.GetStartEndPointOfLine(wallLineId);
                        foreach (var point in listOfStrtEndPoint)
                        {
                            listOfAllPoint.Add(point);
                        }

                        listOfAllPoint = listOfAllPoint.Distinct().ToList();                  

                        var wallLineAngle = AEE_Utility.GetAngleOfLine(wallLineId);

                        bool flag_X_Axis = false;
                        bool flag_Y_Axis = false;

                        CommonModule commonMod = new CommonModule();
                        commonMod.checkAngleOfLine_Axis(wallLineAngle, out flag_X_Axis, out flag_Y_Axis);

                        if (flag_X_Axis == true)
                        {
                            listOfAllPoint = listOfAllPoint.OrderBy(sortPoint => sortPoint.X).ToList();
                        }
                        else if (flag_Y_Axis == true)
                        {
                            listOfAllPoint = listOfAllPoint.OrderBy(sortPoint => sortPoint.Y).ToList();
                        }

                        for (int index = 0; index < listOfAllPoint.Count; index++)
                        {
                            if (index == (listOfAllPoint.Count - 1))
                            {
                                break;
                            }

                            var lineId = AEE_Utility.getLineId(wallLineEnt, listOfAllPoint[index], listOfAllPoint[index + 1], false);
                            double lineLngth = AEE_Utility.GetLengthOfLine(lineId);
                            if (lineLngth > 0)
                            {
                                var midPoint = AEE_Utility.GetMidPoint(lineId);
                                var flagOfInside = AEE_Utility.GetPointIsInsideThePolyline(parapetId, midPoint);
                                if (flagOfInside == true)
                                {
                                    listOfAllWallLineId.Add(lineId);
                                }
                                else
                                {
                                    listOfWallLineWithoutParapetId.Add(lineId);
                                    listOfAllWallLineId.Add(lineId);
                                }
                            }
                        }
                    }
                    else
                    {
                        listOfAllWallLineId.Add(wallLineId);
                    }
                }
            }
            else
            {
                listOfAllWallLineId = listOfWallId;
            }
            return listOfAllWallLineId;
        }

        public void setWallDataInSameCornerOfParapetOrDoor(ObjectId doorId, ObjectId cornerId, double maxLengthVertice, ObjectId maxLengthVerticeLineId)
        {
            var offsetMaxLengthVerticeLineId = AEE_Utility.OffsetEntity(WindowHelper.windowOffsetValue, maxLengthVerticeLineId, false);
            var listOfInsct = AEE_Utility.InterSectionBetweenTwoEntity(cornerId, offsetMaxLengthVerticeLineId);
            if (listOfInsct.Count == 0)
            {
                AEE_Utility.deleteEntity(offsetMaxLengthVerticeLineId);
                offsetMaxLengthVerticeLineId = AEE_Utility.OffsetEntity(-CommonModule.panelDepth, maxLengthVerticeLineId, false);
            }
            else
            {
                offsetMaxLengthVerticeLineId = AEE_Utility.OffsetEntity(CommonModule.panelDepth, maxLengthVerticeLineId, false);
            }

            ParapetHelper.listOfParapetWallLineId_WithDoor.Add(maxLengthVerticeLineId);
            ParapetHelper.listOfParapetWallOffsetLineId_WithDoor.Add(offsetMaxLengthVerticeLineId);
            ParapetHelper.listOfParapetWallLineCornerId_WithDoor.Add(cornerId);        
        }

        public void drawDrawPanelInParapetThickWall(ProgressForm progressForm, string wallCreationMsg)
        {
            for (int index = 0; index < listOfParapetObjId.Count; index++)
            {
                if ((index % 20) == 0)
                {
                    progressForm.ReportProgress(1, wallCreationMsg);
                }

                ObjectId parapetId = listOfParapetObjId[index];
                var listOfExplodeParapetId = AEE_Utility.ExplodeEntity(parapetId);

                WindowHelper windowHlp = new WindowHelper();
                List<ObjectId> listOfMinLengthExplodeLine = windowHlp.getMinLengthOfWindowLineSegment(listOfExplodeParapetId);
                foreach (var parapetThickLineId in listOfMinLengthExplodeLine)
                {
                    var parapetThickLine = AEE_Utility.GetLine(parapetThickLineId);
                    CommonModule commonMod = new CommonModule();
                    var listOfStrtEndPoint = commonMod.getStartEndPointOfLine(parapetThickLine);
                    Point3d newStrtPoint = listOfStrtEndPoint[0];
                    Point3d newEndPoint = listOfStrtEndPoint[1];
                    ObjectId newParapetThickLineId = AEE_Utility.getLineId(parapetThickLine, newStrtPoint, newEndPoint, false);

                    var angleOfNewLine = AEE_Utility.GetAngleOfLine(newStrtPoint, newEndPoint);
                    Entity wallLineEnt = null;
                    List<ObjectId> listOfCornerId = getCornerId_In_ParapetThickWallLine(newParapetThickLineId, angleOfNewLine, out wallLineEnt);
                    if (listOfCornerId.Count != 0)
                    {
                        getParapetThickWallLineWithCorner(parapetThickLine, listOfCornerId, newStrtPoint, newEndPoint, angleOfNewLine, wallLineEnt);
                    }
                }
            }
        }



        private List<ObjectId> getCornerId_In_ParapetThickWallLine(ObjectId parapetThickLineId, double angleOfNewLine, out  Entity wallLineEnt)
        {
            List<ObjectId> listOfCornerId = new List<ObjectId>();
            wallLineEnt = null;

            var listOfStrtEndPoint = AEE_Utility.GetStartEndPointOfLine(parapetThickLineId);
            Point3d newStrtPoint = listOfStrtEndPoint[0];
            Point3d newEndPoint = listOfStrtEndPoint[1];
         
            var point = AEE_Utility.get_XY(angleOfNewLine, WindowHelper.windowOffsetValue);

            newStrtPoint = new Point3d((newStrtPoint.X - point.X), (newStrtPoint.Y - point.Y), 0);
            newEndPoint = new Point3d((newEndPoint.X + point.X), (newEndPoint.Y + point.Y), 0);

            var newParapetThickLineId = AEE_Utility.getLineId(newStrtPoint, newEndPoint, false);
            var newParapetThickLineId_Positive = AEE_Utility.OffsetLine(newParapetThickLineId, WindowHelper.windowOffsetValue, false);
            var newParapetThickLineId_Negative = AEE_Utility.OffsetLine(newParapetThickLineId, -WindowHelper.windowOffsetValue, false);

            for(int index = 0; index < listOfAllCornersId_InsctToParapet.Count; index++)
            {
                var cornerId = listOfAllCornersId_InsctToParapet[index];
                ObjectId wallLineId = listOfWallLineId_AvailableInParapet_InsctCorner[index];
                var wallLine = AEE_Utility.GetEntityForRead(wallLineId);

                bool flagOfInsct = false;
                var listOfInsct = AEE_Utility.InterSectionBetweenTwoEntity(cornerId, newParapetThickLineId);
                if (listOfInsct.Count != 0)
                {
                    flagOfInsct = true;
                }
                else
                {
                    listOfInsct = AEE_Utility.InterSectionBetweenTwoEntity(cornerId, newParapetThickLineId_Positive);
                    if (listOfInsct.Count != 0)
                    {
                        flagOfInsct = true;
                    }
                    else
                    {
                        listOfInsct = AEE_Utility.InterSectionBetweenTwoEntity(cornerId, newParapetThickLineId_Negative);
                        if (listOfInsct.Count != 0)
                        {
                            flagOfInsct = true;
                        }                       
                    }
                }

                if (flagOfInsct == true)
                {
                    if (!listOfCornerId.Contains(cornerId))
                    {
                        listOfCornerId.Add(cornerId);
                    }
                    wallLineEnt = wallLine;
                }              

                if (listOfCornerId.Count == 2)
                {
                    break;
                }
            }

            AEE_Utility.deleteEntity(newParapetThickLineId);
            AEE_Utility.deleteEntity(newParapetThickLineId_Positive);
            AEE_Utility.deleteEntity(newParapetThickLineId_Negative);

            return listOfCornerId;
        }

        private void getParapetThickWallLineWithCorner(Line parapetThickLine, List<ObjectId> listOfCornerId, Point3d strtPoint, Point3d endPoint, double angleOfNewLine, Entity wallLineEnt)
        {
            DoorHelper doorHlp = new DoorHelper();

            double lengthOfStrtPoint = 0;
            double lengthOfEndPoint = 0;
            ObjectId startPointCornerId = new ObjectId();
            ObjectId endPointCornerId = new ObjectId();

            int externalCornerCount = 0;
            for (int i = 0; i < listOfCornerId.Count; i++)
            {
                ObjectId cornerId = listOfCornerId[i];
                var basePoint = AEE_Utility.GetBasePointOfPolyline(cornerId);

                var cornerEnt = AEE_Utility.GetEntityForRead(cornerId);
                ObjectId maxLengthCornerLineId = new ObjectId();
                var maxLengthCornerLine = doorHlp.getMaximumLengthOfCornerVertex(angleOfNewLine, cornerId, out maxLengthCornerLineId);

              
                double strtPointLength = AEE_Utility.GetLengthOfLine(basePoint, strtPoint);
                double endPointLength = AEE_Utility.GetLengthOfLine(basePoint, endPoint);

                double minLength = Math.Min(strtPointLength, endPointLength);
                if (minLength == strtPointLength)
                {
                    lengthOfStrtPoint = minLength + maxLengthCornerLine;
                    startPointCornerId = cornerId;
                    if (cornerEnt.Layer == CommonModule.externalCornerLyrName)
                    {
                        externalCornerCount++;
                    }
                }
                if (minLength == endPointLength)
                {
                    lengthOfEndPoint = minLength + maxLengthCornerLine;
                    endPointCornerId = cornerId;
                    if (cornerEnt.Layer == CommonModule.externalCornerLyrName)
                    {
                        externalCornerCount++;
                    }
                }
            }

            if (externalCornerCount >= 2)
            {
                return;
            }

            bool flagOfTwoParapetInsct = false;

            var indexOfStrtPointCorner = ParapetHelper.listOfCornerId_InsctToTwoParapet.IndexOf(startPointCornerId);
            var indexOfEndPointCorner = ParapetHelper.listOfCornerId_InsctToTwoParapet.IndexOf(endPointCornerId);

            double diffrnceHghtBtwTwoParapet = 0;
            if (indexOfStrtPointCorner != -1)
            {
                flagOfTwoParapetInsct = true;

                var listOfTwoParapetId_InsctToCommonCorner = listOfListOfTwoParapetId_InsctToCommonCorner[indexOfStrtPointCorner];
                ObjectId parapet1Id = listOfTwoParapetId_InsctToCommonCorner[0];    
                var parapet1 = AEE_Utility.GetEntityForRead(parapet1Id);
                var parapet1Heigth = Parapet_UI_Helper.getParapetTopHeight(parapet1.Layer);

                ObjectId parapet2Id = listOfTwoParapetId_InsctToCommonCorner[1];
                var parapet2 = AEE_Utility.GetEntityForRead(parapet2Id);
                var parapet2Heigth = Parapet_UI_Helper.getParapetTopHeight(parapet2.Layer);

                if (parapet1Heigth == parapet2Heigth)
                {
                    return;
                }
                diffrnceHghtBtwTwoParapet = Math.Abs(parapet1Heigth - parapet2Heigth);
            }

            if (indexOfEndPointCorner != -1)
            {
                flagOfTwoParapetInsct = true;

                var listOfTwoParapetId_InsctToCommonCorner = listOfListOfTwoParapetId_InsctToCommonCorner[indexOfEndPointCorner];
                ObjectId parapet1Id = listOfTwoParapetId_InsctToCommonCorner[0];
                var parapet1 = AEE_Utility.GetEntityForRead(parapet1Id);
                var parapet1Heigth = Parapet_UI_Helper.getParapetTopHeight(parapet1.Layer);


                ObjectId parapet2Id = listOfTwoParapetId_InsctToCommonCorner[1];    
                var parapet2 = AEE_Utility.GetEntityForRead(parapet2Id);
                var parapet2Heigth = Parapet_UI_Helper.getParapetTopHeight(parapet2.Layer);

                if (parapet1Heigth == parapet2Heigth)
                {
                    return;
                }
                diffrnceHghtBtwTwoParapet = Math.Abs(parapet1Heigth - parapet2Heigth);
            }

            Point3d newStrtPoint = new Point3d();
            Point3d newEndPoint = new Point3d();

            if (lengthOfStrtPoint != 0)
            {
                var point = AEE_Utility.get_XY(angleOfNewLine, lengthOfStrtPoint);
                newStrtPoint = new Point3d((strtPoint.X - point.X), (strtPoint.Y - point.Y), 0);
            }
            else
            {
                newStrtPoint = strtPoint;              
            }

            if (lengthOfEndPoint != 0)
            {
                var point = AEE_Utility.get_XY(angleOfNewLine, lengthOfEndPoint);
                newEndPoint = new Point3d((endPoint.X + point.X), (endPoint.Y + point.Y), 0);
            }
            else
            {
                newEndPoint = endPoint;
            }

            ObjectId newParapetLineId = new ObjectId();

            if (flagOfTwoParapetInsct == false)
            {
                newParapetLineId = AEE_Utility.getLineId(wallLineEnt, newStrtPoint, newEndPoint, false);
                drawParapetWall(newParapetLineId, listOfCornerId, flagOfTwoParapetInsct, diffrnceHghtBtwTwoParapet);
            }
            else
            {
                newParapetLineId = AEE_Utility.getLineId(parapetThickLine, newStrtPoint, newEndPoint, false);
                drawParapetWall(newParapetLineId, listOfCornerId, flagOfTwoParapetInsct, diffrnceHghtBtwTwoParapet);
            }         
        }

        private void drawParapetWall(ObjectId parapetWallLineId, List<ObjectId> listOfCornerId, bool flagOfTwoParapetInsct, double parapetHeight)
        {
            var parapetWallLine = AEE_Utility.GetEntityForRead(parapetWallLineId);
            ObjectId cornerId = listOfCornerId[0];

            var offsetParapetWallLineId = AEE_Utility.OffsetLine(parapetWallLineId, 1, false);
            var listOfInsct = AEE_Utility.InterSectionBetweenTwoEntity(offsetParapetWallLineId, cornerId);

            AEE_Utility.deleteEntity(offsetParapetWallLineId);
            if (listOfInsct.Count == 0)
            {
                offsetParapetWallLineId = AEE_Utility.OffsetLine(parapetWallLineId, -CommonModule.panelDepth, false);
            }
            else
            {
                offsetParapetWallLineId = AEE_Utility.OffsetLine(parapetWallLineId,CommonModule.panelDepth, false);
            }

            int indexOfExistCornerId = listOfAllCornersId_InsctToParapet.IndexOf(cornerId);
            var parapetId = listOfParapetId_InsctCorner[indexOfExistCornerId];
            var sunkanSlabId = listOfSunkandId_AvailableInParapet_InsctCorner[indexOfExistCornerId];   
            if (parapetHeight == 0)
            {
                var parapet = AEE_Utility.GetEntityForRead(parapetId);
                parapetHeight = Parapet_UI_Helper.getParapetTopHeight(parapet.Layer);
            }
            drawPanelInParapetWall(parapetWallLineId, offsetParapetWallLineId, cornerId, flagOfTwoParapetInsct, parapetHeight, sunkanSlabId);
        }

        public void drawParapetPanel_With_Door()
        {
            for (int index = 0; index < listOfParapetWallLineId_WithDoor.Count; index++)
            {
                var parapetWallLineId = listOfParapetWallLineId_WithDoor[index];
                var offsetParapetWallLineId = listOfParapetWallOffsetLineId_WithDoor[index];
                var cornerId = listOfParapetWallLineCornerId_WithDoor[index];

                int indexOfExistCornerId = listOfAllCornersId_InsctToParapet.IndexOf(cornerId);           
                var parapetId = listOfParapetId_InsctCorner[indexOfExistCornerId];
                var sunkanSlabId = listOfSunkandId_AvailableInParapet_InsctCorner[indexOfExistCornerId];
              
                var parapet = AEE_Utility.GetEntityForRead(parapetId);
                var parapetWallHeight = Parapet_UI_Helper.getParapetTopHeight(parapet.Layer);

                drawPanelInParapetWall(parapetWallLineId, offsetParapetWallLineId, cornerId, false, parapetWallHeight, sunkanSlabId);
            }
        }

        private void drawPanelInParapetWall(ObjectId parapetWallLineId, ObjectId offsetParapetWallLineId, ObjectId cornerId, bool flagOfTwoParapetInsct, double parapetWallHeight, ObjectId sunkanSlabId)
        {
            sunkanSlabId = new ObjectId();
            var parapetWallLine = AEE_Utility.GetLine(parapetWallLineId);

            CommonModule commonMod = new CommonModule();
            var listOfStrtEndPoint = commonMod.getStartEndPointOfLine(parapetWallLine);
            Point3d parapetWallStrtPoint = listOfStrtEndPoint[0];
            Point3d parapetWallEndPoint = listOfStrtEndPoint[1];

            var offsetParapetWallLine = AEE_Utility.GetLine(offsetParapetWallLineId);
            var listOfOffsetStrtEndPoint = commonMod.getStartEndPointOfLine(offsetParapetWallLine);
            Point3d parapetOffsetWallStrtPoint = listOfOffsetStrtEndPoint[0];
            Point3d parapetOffsetWallEndPoint = listOfOffsetStrtEndPoint[1];

            double angleOfParapetWallLine = AEE_Utility.GetAngleOfLine(parapetWallStrtPoint.X, parapetWallStrtPoint.Y, parapetWallEndPoint.X, parapetWallEndPoint.Y);

            bool flag_X_Axis = false;
            bool flag_Y_Axis = false;
            commonMod.checkAngleOfLine_Axis(angleOfParapetWallLine, out flag_X_Axis, out flag_Y_Axis);

            List<ObjectId> listOfWallPanelLine_ObjId = new List<ObjectId>();

            WallPanelHelper wallPanelHlpr = new WallPanelHelper();
            var xDataRegAppName = AEE_Utility.GetXDataRegisterAppName(parapetWallLineId);
            double lengthOfLine = AEE_Utility.GetLengthOfLine(parapetWallStrtPoint.X, parapetWallStrtPoint.Y, parapetWallEndPoint.X, parapetWallEndPoint.Y);
            List<double> listOfWallPanelLength = wallPanelHlpr.getListOfWallPanelLength(lengthOfLine, PanelLayout_UI.standardPanelWidth, PanelLayout_UI.minWidthOfPanel);

            var listOfWallPanelRect_Id = wallPanelHlpr.drawWallPanels(xDataRegAppName, parapetWallLineId, parapetWallStrtPoint, parapetWallEndPoint, parapetOffsetWallStrtPoint, parapetOffsetWallEndPoint, flag_X_Axis, flag_Y_Axis, listOfWallPanelLength, out listOfWallPanelLine_ObjId);

            int indexOfExistCornerId = listOfAllCornersId_InsctToParapet.IndexOf(cornerId);          

            ObjectId wallLineId = listOfWallLineId_AvailableInParapet_InsctCorner[indexOfExistCornerId];
            var wallLine = AEE_Utility.GetEntityForRead(wallLineId);

            double beamBottom = 0;
            string beamLayerNameInsctToWall = "";
            bool flagOfTopPanel = false;
            var standardWallHeight = GeometryHelper.getHeightOfWall(wallLine.Layer, PanelLayout_UI.maxHeightOfPanel, InternalWallSlab_UI_Helper.getSlabBottom(wallLine.Layer), beamBottom, PanelLayout_UI.SL, PanelLayout_UI.RC, PanelLayout_UI.getSlabThickness(wallLine.Layer), PanelLayout_UI.kickerHt, PanelLayout_UI.floorHeight, CommonModule.internalCorner, sunkanSlabId, out flagOfTopPanel);
            var wall_X_Height = wallPanelHlpr.getHeightOfWall_X(sunkanSlabId, PanelLayout_UI.flagOfPanelWithRC, PanelLayout_UI.RC, wallLine.Layer, beamLayerNameInsctToWall, standardWallHeight, flagOfTopPanel);

            double standardWallHeightWithRC = 0;
            string wallPanelNameWithRC = "";
            WallPanelHelper.getWallPanelHeight_PanelWithRC(sunkanSlabId, PanelLayout_UI.flagOfPanelWithRC, standardWallHeight, PanelLayout_UI.wallPanelName, PanelLayout_UI.RC, wallLine.Layer, out standardWallHeightWithRC, out wallPanelNameWithRC);

            List<ObjectId> listOfTextId = new List<ObjectId>();
            List<ObjectId> listOfWallXTextId = new List<ObjectId>();
            List<ObjectId> listOfCircleId = new List<ObjectId>();

            if (flagOfTwoParapetInsct == true)
            {
                standardWallHeightWithRC = parapetWallHeight;
                wall_X_Height = 0;
            }
            else
            {
                standardWallHeightWithRC = standardWallHeightWithRC - parapetWallHeight;
            }

            string doorLayer = ""; //RTJ 10-06-2021
            string windowLayer = ""; //RTJ 10-06-2021
            wallPanelHlpr.writeTextInWallPanel(CommonModule.parapetPanelType, sunkanSlabId, "", listOfWallPanelRect_Id, listOfWallPanelLine_ObjId, standardWallHeightWithRC, wallPanelNameWithRC, wall_X_Height, PanelLayout_UI.wallTopPanelName, PanelLayout_UI.RC, CommonModule.wallPanelTextLyrName, CommonModule.wallPanelLyrColor, true, out listOfTextId, out listOfWallXTextId, out listOfCircleId, doorLayer, windowLayer);  //RTJ 10-06-2021
        }




        public static double getParapetCornerHeight(ObjectId parapetId, ObjectId sunkanSlabId, string wallLayer)
        {
            double parapetWallHght = 0;

            if (parapetId.IsValid == true)
            {
                double levelDifferenceOfSunkanSlb = SunkanSlabHelper.getSunkanSlabLevelDifference_LessThan_RC__Corners(sunkanSlabId);

                var parapet = AEE_Utility.GetEntityForRead(parapetId);
                var parapetTopHght = Parapet_UI_Helper.getParapetTopHeight(parapet.Layer);
                parapetWallHght = parapetTopHght;
                parapetWallHght = parapetWallHght + levelDifferenceOfSunkanSlb;
                
            }

            return parapetWallHght;
        }
        public static double getParapetWallHeight(ObjectId parapetId, ObjectId sunkanSlabId,ObjectId wallId, string wallPanelLayer)
        {
            double parapetWallHght = 0;

            if (parapetId.IsValid == true)
            {
                double levelDifferenceOfSunkanSlb = SunkanSlabHelper.getSunkanSlabLevelDifference_LessThan_RC__Corners(sunkanSlabId);

                parapetWallHght = getParapetWallHeightWithoutLevelDiffOfSunkanSlab(parapetId, sunkanSlabId, wallId, wallPanelLayer);

                 var flagOfGreater = SunkanSlabHelper.checkSunkanSlabLevelDiffrnce_Is_Greater(sunkanSlabId);
                if (flagOfGreater == false)
                {
                    parapetWallHght = parapetWallHght + levelDifferenceOfSunkanSlb;
                }
            }

            return parapetWallHght;
        }
        private static bool checkSunkanSlabExistForWallId(ObjectId wallId,out ObjectId sunkanSlabId)
        {
            bool sunkanSlabIsExist = false;
            bool flag_X_Axis = false;
            bool flag_Y_Axis = false;
            Point3d midPt = new Point3d();
            ObjectId perpendicularLineId = new ObjectId(); 

            Point2d startPt;
            Point2d endPt;

            sunkanSlabId = new ObjectId();
            CommonModule commonMod = new CommonModule();
            commonMod.checkAngleOfLine_Axis(AEE_Utility.GetAngleOfLine(wallId), out flag_X_Axis, out flag_Y_Axis);

            midPt = AEE_Utility.GetMidPoint(wallId);

            if (flag_X_Axis)
            {
                startPt = new Point2d(midPt.X , midPt.Y - PanelLayout_UI.maxWallThickness);
                endPt = new Point2d(midPt.X, midPt.Y+ PanelLayout_UI.maxWallThickness);
            }
            else
            {
                startPt = new Point2d(midPt.X - PanelLayout_UI.maxWallThickness, midPt.Y );
                endPt = new Point2d(midPt.X + PanelLayout_UI.maxWallThickness, midPt.Y );
            }

            perpendicularLineId = AEE_Utility.getLineId(startPt, endPt, true);

            foreach (var sunkanId in SunkanSlabHelper.listOfSunkanSlab_ObjId)
            {
                var points3d = AEE_Utility.InterSectionBetweenTwoEntity(perpendicularLineId, sunkanId);

                if (points3d.Count > 0)
                {
                    sunkanSlabId = sunkanId;
                    sunkanSlabIsExist = true;
                    break;
                }
            }

            AEE_Utility.deleteEntity(perpendicularLineId);
            return sunkanSlabIsExist;
        }

        //// New Method Added By PBAC 22-11-2021 
        public static double getParapetWallHeightWithoutLevelDiffOfSunkanSlab(ObjectId parapetId, ObjectId sunkanSlabId, ObjectId wallId, string wallPanelLayer)
        {
            double parapetWallHght = 0;
    
            if (parapetId.IsValid == true)
            {              
                var parapet = AEE_Utility.GetEntityForRead(parapetId);
                var parapetTopHght = Parapet_UI_Helper.getParapetTopHeight(parapet.Layer);
                parapetWallHght = parapetTopHght;

                if (sunkanSlabId.IsValid == true)
                {
                    parapetWallHght = parapetWallHght - PanelLayout_UI.RC;
                }
                else if (InternalWallSlab_UI_Helper.IsInternalWall(wallPanelLayer))
                {
                    if (PanelLayout_UI.flagOfPanelWithRC == false )
                    {
                        parapetWallHght = parapetWallHght - PanelLayout_UI.RC;
                    }
                }             
                else 
                {
                    ObjectId tempSunkanSlabId = new ObjectId();
                    if (checkSunkanSlabExistForWallId(wallId,out tempSunkanSlabId) == true)
                    {
                        parapetWallHght = parapetWallHght + PanelLayout_UI.getSlabThickness(AEE_Utility.GetEntityForRead(tempSunkanSlabId).Layer);
                    }
                    else
                    {
                        parapetWallHght = parapetWallHght - PanelLayout_UI.RC;
                    }                    
                }

            }

            return parapetWallHght;
        }
        //Commented on 22/11/2021 by PBAC (This replaced with getParapetWallHeightWithoutLevelDiffOfSunkanSlab method)

        //public static double getParapetWallHeight(ObjectId parapetId, string wallPanelLayer)
        //{
        //    double parapetWallHght = 0;

        //    if (parapetId.IsValid == true)
        //    {
        //        var parapet = AEE_Utility.GetEntityForRead(parapetId);
        //        var parapetTopHght = Parapet_UI_Helper.getParapetTopHeight(parapet.Layer);
        //        parapetWallHght = parapetTopHght;
        //        if (InternalWallSlab_UI_Helper.IsInternalWall(wallPanelLayer))
        //        {
        //            if (PanelLayout_UI.flagOfPanelWithRC == false)
        //            {
        //                parapetWallHght = parapetWallHght - PanelLayout_UI.RC;
        //            }
        //        }
        //        else if (wallPanelLayer == CommonModule.externalWallLayerName)
        //        {
        //            parapetWallHght = parapetWallHght - PanelLayout_UI.RC;
        //        }
        //    }

        //    return parapetWallHght;
        //}

    }
}
