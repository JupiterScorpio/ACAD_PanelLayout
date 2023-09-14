using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using PanelLayout_App.ElevationModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PanelLayout_App.WallModule
{
    public class DeckPanelHelper
    {
        public static List<List<ObjectId>> listOfListOfDeckPanelWallId = new List<List<ObjectId>>();
        public static List<MaxAreaRelationInfo> lstMaxAreaRelations = new List<MaxAreaRelationInfo>();
        public static List<List<double>> listOfListOfDeckPanelSpanLngth = new List<List<double>>();
        public static List<List<double>> listOfListOfIntrvalLengthOfDeckPanel = new List<List<double>>();
        public static List<List<double>> listOfListOfDeckPlane_SF_Interval = new List<List<double>>();

        // Cuong - 23/12/2022 - DeckPanel issue - START
        public static Dictionary<ObjectId, List<MaxAreaRelationInfo>> dicRelationInfo = new Dictionary<ObjectId, List<MaxAreaRelationInfo>>();
        public static List<List<MaxAreaRelationInfo>> listOfListOfRelationInfo = new List<List<MaxAreaRelationInfo>>();
        // Cuong - 23/12/2022 - DeckPanel issue - END

        public void setDeckPanelWall(ObjectId wallId, double panelDepth)
        {
            AEE_Utility.changeVisibility(wallId, false);

            string xDataRegAppName = AEE_Utility.GetXDataRegisterAppName(wallId);

            var listOfDeckPanelId = getDeckPanel(wallId);

            // Cuong - 23/12/2022 - DeckPanel issue - START
            var segmentDic = new Dictionary<ObjectId, List<int>>();
            foreach (KeyValuePair<ObjectId, MaxAreaRelationInfo> deckPanelWallId in listOfDeckPanelId)
            {
                segmentDic.Add(deckPanelWallId.Key, ValidateDeckPanel(deckPanelWallId.Key, wallId, panelDepth, listOfDeckPanelId));
            }

            AdjustDeckPanelInfo(segmentDic, panelDepth);
            // Cuong - 23/12/2022 - DeckPanel issue - END

            foreach (KeyValuePair<ObjectId, MaxAreaRelationInfo> deckPanelWallId in listOfDeckPanelId)
            {
                AEE_Utility.AttachXData(deckPanelWallId.Key, xDataRegAppName, CommonModule.xDataAsciiName);
                setDeckPanelId(deckPanelWallId.Key, xDataRegAppName, deckPanelWallId.Value);
            }
        }

        // Cuong - 23/12/2022 - DeckPanel issue - START
        private List<int> ValidateDeckPanel(ObjectId deckPanelWallId, ObjectId wallId, double panelDepth, Dictionary<ObjectId, MaxAreaRelationInfo> listOfDeckPanelId)
        {
            var segmentList = new List<int>();

            var deckPanel = AEE_Utility.GetEntityForRead(deckPanelWallId) as Polyline;
            var insideWallEnt = AEE_Utility.GetEntityForRead(wallId) as Polyline;

            Point3d centerPt = WallPanelHelper.getCenterPointOfPanelRectangle(deckPanelWallId);
            segmentList = GetLinesToBeAdjusted(deckPanel, insideWallEnt, centerPt, panelDepth, listOfDeckPanelId);

            return segmentList;
        }

        private List<int> GetLinesToBeAdjusted(Polyline deckPanel, Polyline insideWallEnt, Point3d centerPt, double panelDepth, Dictionary<ObjectId, MaxAreaRelationInfo> listOfDeckPanelId)
        {
            var segments = new List<int>();
            if (deckPanel != null && deckPanel.NumberOfVertices == 4)
            {
                for (int i = 0; i < deckPanel.NumberOfVertices; i++)
                {
                    if (IsSegmentNeedToBeAdjusted(centerPt, deckPanel.GetLineSegmentAt(i), insideWallEnt, panelDepth) &&
                        IsSegmentUnique(deckPanel.GetLineSegmentAt(i), deckPanel.ObjectId, listOfDeckPanelId, insideWallEnt, panelDepth))
                        segments.Add(i);
                }
            }

            return segments;
        }

        private bool IsSegmentUnique(LineSegment3d segment, ObjectId id, Dictionary<ObjectId, MaxAreaRelationInfo> listOfDeckPanelId, Polyline insideWallEnt, double panelDepth)
        {
            bool result = true;

            foreach (KeyValuePair<ObjectId, MaxAreaRelationInfo> deckPanelWallId in listOfDeckPanelId)
            {
                if (deckPanelWallId.Key != id)
                {
                    var deckPanel = AEE_Utility.GetEntityForRead(deckPanelWallId.Key) as Polyline;
                    Point3d centerPt = WallPanelHelper.getCenterPointOfPanelRectangle(deckPanelWallId.Key);

                    if (deckPanel != null && deckPanel.NumberOfVertices == 4)
                    {
                        for (int i = 0; i < deckPanel.NumberOfVertices; i++)
                        {
                            if (IsSegmentNeedToBeAdjusted(centerPt, deckPanel.GetLineSegmentAt(i), insideWallEnt, panelDepth))
                            {
                                var segmentTmp = deckPanel.GetLineSegmentAt(i);
                                if (segmentTmp.IsOn(segment.StartPoint) && segmentTmp.IsOn(segment.EndPoint))
                                    result = false;
                            }
                        }
                    }
                }
            }

            return result;
        }

        private bool IsSegmentNeedToBeAdjusted(Point3d centerPt, LineSegment3d segment, Polyline insideWallEnt, double maxLen)
        {
            bool result = false;

            Point3d midPoint = segment.MidPoint;
            Vector3d direction = centerPt.GetVectorTo(midPoint).GetNormal();
            Point3d pt1 = segment.StartPoint.Add(direction * maxLen);
            Point3d pt2 = segment.EndPoint.Add(direction * maxLen);

            if (InsideOrOn(pt1, insideWallEnt) && InsideOrOn(pt2, insideWallEnt))
                result = true;

            return result;
        }

        private bool InsideOrOn(Point3d pt, Polyline pl)
        {
            var vecs = new Vector3d[] { new Vector3d(1, 0.3, 0), new Vector3d(-1, 0.3, 0),
                                        new Vector3d(1, -0.3, 0), new Vector3d(-1, -0.3, 0) };
            var cl = pl.GetClosestPointTo(pt, false);
            if (cl.DistanceTo(pt) < 1e-6)
            {
                return true;
            }
            var inside = 0;
            foreach (var vec in vecs)
            {
                var ray = new Ray() { BasePoint = pt, UnitDir = vec };
                var pts = new Point3dCollection();
                pl.IntersectWith(ray, Intersect.OnBothOperands, pts, IntPtr.Zero, IntPtr.Zero);
                if (pts.Count % 2 == 1)
                {
                    inside++;
                }
            }
            return inside > 2;
        }

        private void AdjustDeckPanelInfo(Dictionary<ObjectId, List<int>> segmentDic, double panelDepth)
        {
            foreach (var dic in segmentDic)
            {
                Point3d centerPt = WallPanelHelper.getCenterPointOfPanelRectangle(dic.Key);

                var infoList = AEE_Utility.AdjustRelationInfo(dic.Key, centerPt, dic.Value, panelDepth);
                dicRelationInfo.Add(dic.Key, infoList);

                AEE_Utility.AdjustDeckPanel(dic.Key, centerPt, dic.Value, panelDepth);
            }
        }

        // Cuong - 23/12/2022 - DeckPanel issue - END

        private Dictionary<ObjectId, MaxAreaRelationInfo> getDeckPanel(ObjectId insideWallId)
        {
            var listOfDeckPanelId = new Dictionary<ObjectId, MaxAreaRelationInfo>();

            var listOfDeckPanelExplodeId = AEE_Utility.ExplodeEntity(insideWallId);

            if (listOfDeckPanelExplodeId.Count == 4)
            {
                listOfDeckPanelId[insideWallId] = null;
            }

            //if (listOfDeckPanelExplodeId.Count == 6)
            //{
            //    double offsetValue = 10;

            //    List<double> listOfDeckWallLength = new List<double>();
            //    foreach (var wallLineId in listOfDeckPanelExplodeId)
            //    {
            //        double wallLineLngth = AEE_Utility.GetLengthOfLine(wallLineId);
            //        listOfDeckWallLength.Add(wallLineLngth);
            //    }

            //    double minLength = listOfDeckWallLength.Min();
            //    int indexOfMinLngth = listOfDeckWallLength.IndexOf(minLength);
            //    ObjectId minLngthWallLineId = listOfDeckPanelExplodeId[indexOfMinLngth];

            //    ObjectId prvsWallLineId = new ObjectId();
            //    ObjectId nextWallLineId = new ObjectId();
            //    getMinLengthLineId(indexOfMinLngth, listOfDeckPanelExplodeId, out minLngthWallLineId, out prvsWallLineId, out nextWallLineId);

            //    ObjectId minLengthRectId = getMinLengthRectId(indexOfMinLngth, insideWallId, prvsWallLineId, nextWallLineId, minLngthWallLineId, offsetValue);
            //    ObjectId maxLengthRectId = getMaxLengthRectId(listOfDeckPanelExplodeId, insideWallId, minLengthRectId, minLngthWallLineId, prvsWallLineId, nextWallLineId, offsetValue);
            //    if (minLengthRectId.IsValid == true)
            //    {
            //        listOfDeckPanelId[minLengthRectId]=maxLengthRectId;
            //    }

            //    if (maxLengthRectId.IsValid == true)
            //    {
            //        listOfDeckPanelId[maxLengthRectId] = ObjectId.Null;
            //    }
            //}

            // Cuong - 20/12/2022 - DeckPanel issue - START
            //if (listOfDeckPanelExplodeId.Count > 4)
            //{
            //    var pts = AEE_Utility.GetPolyLinePoints(insideWallId);
            //    var insideWallEnt = AEE_Utility.GetEntityForRead(insideWallId);

            //    var dic = RectangleAreaFinder.CreateRecngle1(pts, insideWallEnt.Layer, insideWallEnt.ColorIndex, false, CommonModule.deckPanelGap);
            //    foreach (KeyValuePair<ObjectId, MaxAreaRelationInfo> pair in dic)
            //    {
            //        listOfDeckPanelId[pair.Key] = pair.Value;
            //    }
            //    //createDeckPanelRect(insideWallId, listOfDeckPanelExplodeId);
            //}
            // Cuong - 20/12/2022 - DeckPanel issue - END

            // Cuong - 20/12/2022 - DeckPanel issue - START
            if (listOfDeckPanelExplodeId.Count > 4)
            {
                var pts = AEE_Utility.GetPolyLinePoints(insideWallId);
                var insideWallEnt = AEE_Utility.GetEntityForRead(insideWallId);

                var obj = new RectangleAreaFinder(pts, insideWallEnt.Layer, insideWallEnt.ColorIndex, false, CommonModule.deckPanelGap);
                var invalidData = obj.FindInvalidData();
                var maxKey = obj.GetMaxKey();

                var xc = obj.GetXCoOrds();
                var yc = obj.GetYCoOrds();
                xc.Sort();
                yc.Sort();

                var rectangles = GetSameKeyRectangles(invalidData, maxKey, xc, yc);
                foreach (int k in rectangles.Keys)
                {
                    var points = rectangles[k];
                    var pt1 = points[0];
                    var pt2 = points[points.Count - 1];

                    var newPts = new Point3dCollection();
                    newPts.Add(pt1);
                    newPts.Add(new Point3d(pt1.X, pt2.Y, 0));
                    newPts.Add(pt2);
                    newPts.Add(new Point3d(pt2.X, pt1.Y, 0));
                    newPts.Add(pt1);

                    var dic = RectangleAreaFinder.CreateRecngle1(newPts, insideWallEnt.Layer, insideWallEnt.ColorIndex, false, CommonModule.deckPanelGap);
                    foreach (KeyValuePair<ObjectId, MaxAreaRelationInfo> pair in dic)
                    {
                        listOfDeckPanelId[pair.Key] = pair.Value;
                    }
                }
            }
            // Cuong - 20/12/2022 - DeckPanel issue - END

            AEE_Utility.deleteEntity(listOfDeckPanelExplodeId);

            return listOfDeckPanelId;
        }

        // Cuong - 20/12/2022 - DeckPanel issue - START
        private Dictionary<int, List<Point3d>> GetSameKeyRectangles(int[][] data, int maxKey, List<double> xc, List<double> yc)
        {
            var rectangles = new Dictionary<int, List<Point3d>>();

            for (int k = 1; k <= maxKey; k++)
            {
                var points = new List<Point3d>();
                for (int i = 0; i < data.Length; i++)
                {
                    for (int j = 0; j < data[i].Length; j++)
                    {
                        if (data[i][j] == k)
                        {
                            points.Add(new Point3d(xc[j], yc[i], 0));
                            points.Add(new Point3d(xc[j + 1], yc[i + 1], 0));
                        }
                    }
                }
                rectangles.Add(k, points);
            }

            return rectangles;
        }
        // Cuong - 20/12/2022 - DeckPanel issue - END

        private void createDeckPanelRect(ObjectId insideWallId, List<ObjectId> listOfDeckPlateExplodeId)
        {
            var listOfVerticePoint = AEE_Utility.GetPolylineVertexPoint(insideWallId);
            double minX = listOfVerticePoint.Min(sortPoint => sortPoint.X);
            double minY = listOfVerticePoint.Min(sortPoint => sortPoint.Y);
            double maxX = listOfVerticePoint.Max(sortPoint => sortPoint.X);
            double maxY = listOfVerticePoint.Max(sortPoint => sortPoint.Y);

            double length1 = Math.Abs(minX - maxX);
            double length2 = Math.Abs(minY - maxY);
            double maxLength = Math.Max(length1, length2);
            maxLength = maxLength + (maxLength / 2);

            List<string> listOfPointWithAngle_X = new List<string>();
            List<string> listOfPointWithAngle_Y = new List<string>();

            CommonModule commonMod = new CommonModule();

            foreach (var wallLineId in listOfDeckPlateExplodeId)
            {
                List<Point3d> listOfStrtEndPoint = commonMod.getStartEndPointOfLine(wallLineId);

                Point3d startPnt = listOfStrtEndPoint[0];
                Point3d endPoint = listOfStrtEndPoint[1];
                var lineId = AEE_Utility.getLineId(startPnt, endPoint, false);
                listOfStrtEndPoint = commonMod.getStartEndPointOfLine(lineId);
                startPnt = listOfStrtEndPoint[0];
                endPoint = listOfStrtEndPoint[1];

                double wallLineAngle = AEE_Utility.GetAngleOfLine(lineId);
                var point = AEE_Utility.get_XY(wallLineAngle, maxLength);

                Point3d startPoint = new Point3d((startPnt.X - point.X), (startPnt.Y - point.Y), 0);
                Point3d endPnt = new Point3d((endPoint.X + point.X), (endPoint.Y + point.Y), 0);

                AEE_Utility.deleteEntity(lineId);
            }
        }

        private void getMinLengthLineId(int indexOfMinLngth, List<ObjectId> listOfDeckPanelExplodeId, out ObjectId minLngthWallLineId, out ObjectId prvsWallLineId, out ObjectId nextWallLineId)
        {
            minLngthWallLineId = listOfDeckPanelExplodeId[indexOfMinLngth];

            prvsWallLineId = new ObjectId();
            nextWallLineId = new ObjectId();
            if (indexOfMinLngth == 0)
            {
                prvsWallLineId = listOfDeckPanelExplodeId[(listOfDeckPanelExplodeId.Count - 1)];
            }
            else
            {
                prvsWallLineId = listOfDeckPanelExplodeId[(indexOfMinLngth - 1)];
            }

            if (indexOfMinLngth == (listOfDeckPanelExplodeId.Count - 1))
            {
                nextWallLineId = listOfDeckPanelExplodeId[0];
            }
            else
            {
                nextWallLineId = listOfDeckPanelExplodeId[(indexOfMinLngth + 1)];
            }
        }
        private ObjectId getMinLengthRectId(int indexOfMinLngth, ObjectId insideWallId, ObjectId prvsWallLineId, ObjectId nextWallLineId, ObjectId minLngthWallLineId, double offsetValue)
        {
            ObjectId minLengthRectId = new ObjectId();

            var minLngthWallOffstLineId = AEE_Utility.OffsetLine(minLngthWallLineId, offsetValue, false);
            var minLngthWallOffstLineMidPoint = AEE_Utility.GetMidPoint(minLngthWallOffstLineId);
            var flagOfInside = AEE_Utility.GetPointIsInsideThePolyline(insideWallId, minLngthWallOffstLineMidPoint);
            if (flagOfInside == false)
            {
                AEE_Utility.deleteEntity(minLngthWallOffstLineId);
                minLngthWallOffstLineId = AEE_Utility.OffsetLine(minLngthWallLineId, -offsetValue, false);
            }

            var listOfInsctPointOfPrvsLine = AEE_Utility.InterSectionBetweenTwoEntity(minLngthWallOffstLineId, prvsWallLineId);
            var listOfInsctPointOfNewLine = AEE_Utility.InterSectionBetweenTwoEntity(minLngthWallOffstLineId, nextWallLineId);

            AEE_Utility.deleteEntity(minLngthWallOffstLineId);

            ObjectId minLengthAdjacentLineId = new ObjectId();
            if (listOfInsctPointOfPrvsLine.Count != 0 && listOfInsctPointOfNewLine.Count != 0)
            {
                double prvsWallLineLngth = AEE_Utility.GetLengthOfLine(prvsWallLineId);
                double nextWallLineLngth = AEE_Utility.GetLengthOfLine(nextWallLineId);
                if (prvsWallLineLngth > nextWallLineLngth)
                {
                    minLengthAdjacentLineId = nextWallLineId;
                }
                else
                {
                    minLengthAdjacentLineId = prvsWallLineId;
                }
            }
            else if (listOfInsctPointOfPrvsLine.Count != 0 && listOfInsctPointOfNewLine.Count == 0)
            {
                minLengthAdjacentLineId = prvsWallLineId;
            }
            else if (listOfInsctPointOfNewLine.Count != 0 && listOfInsctPointOfPrvsLine.Count == 0)
            {
                minLengthAdjacentLineId = nextWallLineId;
            }

            if (minLengthAdjacentLineId.IsValid == true)
            {
                double minLengthAdjacentLineLngth = AEE_Utility.GetLengthOfLine(minLengthAdjacentLineId);

                minLngthWallOffstLineId = AEE_Utility.OffsetLine(minLngthWallLineId, offsetValue, false);
                minLngthWallOffstLineMidPoint = AEE_Utility.GetMidPoint(minLngthWallOffstLineId);
                flagOfInside = AEE_Utility.GetPointIsInsideThePolyline(insideWallId, minLngthWallOffstLineMidPoint);
                if (flagOfInside == false)
                {
                    AEE_Utility.deleteEntity(minLngthWallOffstLineId);
                    minLngthWallOffstLineId = AEE_Utility.OffsetLine(minLngthWallLineId, -minLengthAdjacentLineLngth, false);
                }
                else
                {
                    minLngthWallOffstLineId = AEE_Utility.OffsetLine(minLngthWallLineId, minLengthAdjacentLineLngth, false);
                }
                minLengthRectId = drawMinLengthOfRectangle(insideWallId, minLngthWallLineId, minLngthWallOffstLineId);

                AEE_Utility.deleteEntity(minLngthWallOffstLineId);
            }
            return minLengthRectId;
        }
        private ObjectId drawMinLengthOfRectangle(ObjectId insideWallId, ObjectId lineId1, ObjectId lineId2)
        {
            var insideWallEnt = AEE_Utility.GetEntityForRead(insideWallId);
            List<Point3d> listOfLine_1_StrtEndPoint = AEE_Utility.GetStartEndPointOfLine(lineId1);
            Point3d deckPnl_1_StrtPnt = listOfLine_1_StrtEndPoint[0];
            Point3d deckPnl_1_EndPnt = listOfLine_1_StrtEndPoint[1];

            List<Point3d> listOfLine_2_StrtEndPoint = AEE_Utility.GetStartEndPointOfLine(lineId2);
            Point3d deckPnl_2_StrtPnt = listOfLine_2_StrtEndPoint[0];
            Point3d deckPnl_2_EndPnt = listOfLine_2_StrtEndPoint[1];

            ObjectId deckPanelRectId = AEE_Utility.GetRectangleId(deckPnl_1_StrtPnt, deckPnl_1_EndPnt, deckPnl_2_EndPnt, deckPnl_2_StrtPnt, insideWallEnt.Layer, insideWallEnt.ColorIndex, false);

            return deckPanelRectId;
        }

        private ObjectId getMaxLengthRectId(List<ObjectId> listOfDeckPanelExplodeId, ObjectId insideWallId, ObjectId minLengthRectId, ObjectId minLngthWallLineId, ObjectId prvsWallLineId, ObjectId nextWallLineId, double offsetValue)
        {
            Point3d offsetRectPoint = new Point3d(123456, 123456, 0);
            ObjectId minLengthOffstRectId = AEE_Utility.OffsetEntity_WithoutLine(offsetValue, offsetRectPoint, minLengthRectId, false);

            List<ObjectId> listOfDeckPanelLineId = new List<ObjectId>();
            List<double> listOfDeckPanelLineLngth = new List<double>();

            foreach (var lineId in listOfDeckPanelExplodeId)
            {
                if (lineId == minLngthWallLineId || lineId == prvsWallLineId || lineId == nextWallLineId)
                {

                }
                else
                {
                    var listOfInsctPoint = AEE_Utility.InterSectionBetweenTwoEntity(minLengthOffstRectId, lineId);
                    if (listOfInsctPoint.Count == 0)
                    {
                        listOfDeckPanelLineId.Add(lineId);
                        double lineLngth = AEE_Utility.GetLengthOfLine(lineId);
                        listOfDeckPanelLineLngth.Add(lineLngth);
                    }
                }
            }

            double minLength = listOfDeckPanelLineLngth.Min();
            int indexOfMinLngth = listOfDeckPanelLineLngth.IndexOf(minLength);
            minLngthWallLineId = listOfDeckPanelLineId[indexOfMinLngth];

            double maxLength = listOfDeckPanelLineLngth.Max();

            var minLngthWallOffstLineId = AEE_Utility.OffsetLine(minLngthWallLineId, offsetValue, false);
            var minLngthWallOffstLineMidPoint = AEE_Utility.GetMidPoint(minLngthWallOffstLineId);
            var flagOfInside = AEE_Utility.GetPointIsInsideThePolyline(insideWallId, minLngthWallOffstLineMidPoint);
            if (flagOfInside == false)
            {
                AEE_Utility.deleteEntity(minLngthWallOffstLineId);
                minLngthWallOffstLineId = AEE_Utility.OffsetLine(minLngthWallLineId, -maxLength, false);
            }
            else
            {
                minLngthWallOffstLineId = AEE_Utility.OffsetLine(minLngthWallLineId, maxLength, false);
            }

            ObjectId maxLengthRectId = drawMinLengthOfRectangle(insideWallId, minLngthWallLineId, minLngthWallOffstLineId);

            AEE_Utility.deleteEntity(minLngthWallLineId);
            AEE_Utility.deleteEntity(minLngthWallOffstLineId);
            AEE_Utility.deleteEntity(minLengthOffstRectId);

            return maxLengthRectId;
        }
        private void setDeckPanelId(ObjectId deckPanelWallId, string xDataRegAppName, MaxAreaRelationInfo nearestRectId)
        {
            var listOfDeckPanelExplodeId = AEE_Utility.ExplodeEntity(deckPanelWallId);

            WindowHelper windowHlp = new WindowHelper();
            List<ObjectId> listOfMinLengthExplodeLine = windowHlp.getMinLengthOfWindowLineSegment(listOfDeckPanelExplodeId);
            List<ObjectId> listOfMaxLengthExplodeLine = windowHlp.getMaxLengthOfWindowLineSegment(listOfDeckPanelExplodeId);

            var spanLength = AEE_Utility.GetLengthOfLine(listOfMaxLengthExplodeLine[0]);
            spanLength = Math.Round(spanLength);
            ObjectId minLengthId = getMinLengthId(listOfMinLengthExplodeLine);

            bool enableSF = false;
            var stdLength = spanLength + 2 * CommonModule.internalCorner;
            if (CommonModule.deckPanel_MSP1_MaxLngth > stdLength)
            {
                spanLength = AEE_Utility.GetLengthOfLine(listOfMinLengthExplodeLine[0]);
                spanLength = Math.Round(spanLength);
                minLengthId = getMinLengthId(listOfMaxLengthExplodeLine);
                //enableSF = true;
            }

            setDeckPanelLineId(xDataRegAppName, deckPanelWallId, minLengthId, spanLength, nearestRectId, enableSF);

            AEE_Utility.deleteEntity(listOfDeckPanelExplodeId);
        }

        private ObjectId getMinLengthId(List<ObjectId> listOfMinLengthExplodeLine)
        {
            List<double> listOfMinCoOrdinate = new List<double>();

            ObjectId minLengthId1 = listOfMinLengthExplodeLine[0];
            var listOfMinLengthEnt1 = AEE_Utility.GetStartEndPointOfLine(minLengthId1);

            var minLength1_Angle = AEE_Utility.GetAngleOfLine(minLengthId1);

            ObjectId minLengthId2 = listOfMinLengthExplodeLine[1];
            var listOfMinLengthEnt2 = AEE_Utility.GetStartEndPointOfLine(minLengthId2);

            bool flag_X_Axis = true;
            bool flag_Y_Axis = false;
            CommonModule commonMod = new CommonModule();
            commonMod.checkAngleOfLine_Axis(minLength1_Angle, out flag_X_Axis, out flag_Y_Axis);

            int indexOfLngth = 0;
            ObjectId lengthLineId = new ObjectId();
            if (flag_X_Axis == true)
            {
                double minLngth_1_Y = listOfMinLengthEnt1.Min(sortPoint => sortPoint.Y);
                double minLngth_2_Y = listOfMinLengthEnt2.Min(sortPoint => sortPoint.Y);

                listOfMinCoOrdinate.Add(minLngth_1_Y);
                listOfMinCoOrdinate.Add(minLngth_2_Y);

                double minLength = listOfMinCoOrdinate.Min();
                indexOfLngth = listOfMinCoOrdinate.IndexOf(minLength);
                lengthLineId = listOfMinLengthExplodeLine[indexOfLngth];
            }
            else if (flag_Y_Axis == true)
            {
                double minLngth_1_X = listOfMinLengthEnt1.Min(sortPoint => sortPoint.X);
                double minLngth_2_X = listOfMinLengthEnt2.Min(sortPoint => sortPoint.X);

                listOfMinCoOrdinate.Add(minLngth_1_X);
                listOfMinCoOrdinate.Add(minLngth_2_X);

                double minLength = listOfMinCoOrdinate.Min();
                indexOfLngth = listOfMinCoOrdinate.IndexOf(minLength);
                lengthLineId = listOfMinLengthExplodeLine[indexOfLngth];
            }
            return lengthLineId;
        }

        private void setDeckPanelLineId(string xDataRegAppName, ObjectId deckPlateWallId, ObjectId minLengthId, double spanLength, MaxAreaRelationInfo maxAreaRelation, bool enableSF)
        {
            List<double> listOfIntervalPanelLength = getListOfDeckPanelInterval(spanLength, PanelLayout_UI.standardPanelWidth);
            listOfListOfIntrvalLengthOfDeckPanel.Add(listOfIntervalPanelLength);

            List<double> listOfDeckPlane_SF_Interval = getListOfSP1SpanLength(spanLength);// getListOfDeckPanelSpanLength(spanLength, CommonModule.deckPanel_MSP1_StndrdLngth, CommonModule.deckPanel_MSP1_MaxLngth, CommonModule.deckPanel_MSP1_MinLngth, CommonModule.deckPanel_SF_Lngth);//Changes made on 08/06/2023 by SDM
            listOfListOfDeckPlane_SF_Interval.Add(listOfDeckPlane_SF_Interval);

            List<ObjectId> listOfDeckPanelWallId = new List<ObjectId>();

            CommonModule commonMod = new CommonModule();
            var listOfStrtEndPoint = commonMod.getStartEndPointOfLine(minLengthId);
            var minLengthEnt = AEE_Utility.GetEntityForRead(minLengthId);

            var startPt = listOfStrtEndPoint[0];
            var endPt = listOfStrtEndPoint[1];

            var newLineId = AEE_Utility.getLineId(minLengthEnt, startPt, endPt, false);
            listOfDeckPanelWallId.Add(newLineId);
            AEE_Utility.AttachXData(newLineId, xDataRegAppName, CommonModule.xDataAsciiName);

            double newLineLngth = AEE_Utility.GetLengthOfLine(newLineId);
            newLineLngth = Math.Round(newLineLngth);

            var lyrName = AEE_Utility.GetLayerName(deckPlateWallId);

            var maxLen = PanelLayout_UI.getSlabThickness(lyrName) <= 175 ? CommonModule.deckPanel_Span_MaxLngthForSLTLTOrEqual175 :
                CommonModule.deckPanel_Span_MaxLngthForSLTGT175;
            List<double> listOfWallSpanLength = getListOfDeckPanelStandardSpanLength(newLineLngth, CommonModule.deckPanel_Span_StndrdLngth, maxLen, CommonModule.deckPanel_Span_MinLngth, CommonModule.deckPanelGap, CommonModule.slabJointPanelDepth);
            listOfListOfDeckPanelSpanLngth.Add(listOfWallSpanLength);

            Point3d offsetPoint = new Point3d(1234521, 1234521, 0);
            ObjectId deckPlateOffstOutsideWallId = AEE_Utility.OffsetEntity_WithoutLine(5, offsetPoint, deckPlateWallId, false);

            double totalIntervalLength = 0;
            for (int index = 0; index < listOfIntervalPanelLength.Count; index++)
            {
                double intervalLength = listOfIntervalPanelLength[index];
                totalIntervalLength = totalIntervalLength + intervalLength;

                var offsetId = AEE_Utility.OffsetLine(newLineId, totalIntervalLength, false);
                var midPoint = AEE_Utility.GetMidPoint(offsetId);
                var flagOfInside = AEE_Utility.GetPointIsInsideThePolyline(deckPlateOffstOutsideWallId, midPoint);
                if (flagOfInside == false)
                {
                    AEE_Utility.deleteEntity(offsetId);
                    offsetId = AEE_Utility.OffsetLine(newLineId, -totalIntervalLength, false);
                    listOfDeckPanelWallId.Add(offsetId);
                }
                else
                {
                    listOfDeckPanelWallId.Add(offsetId);
                }
                AEE_Utility.AttachXData(offsetId, xDataRegAppName, CommonModule.xDataAsciiName);
            }

            AEE_Utility.deleteEntity(deckPlateOffstOutsideWallId);
            lstMaxAreaRelations.Add(maxAreaRelation);
            listOfListOfDeckPanelWallId.Add(listOfDeckPanelWallId);

            // Cuong - 23/12/2022 - DeckPanel issue - START
            listOfListOfRelationInfo.Add(dicRelationInfo[deckPlateWallId]);
            // Cuong - 23/12/2022 - DeckPanel issue - END
        }

        private List<double> getListOfDeckPanelInterval(double lengthOfLine, double standardWallLngth)
        {
            List<double> listOfWallPanelLength = new List<double>();
            if (lengthOfLine > standardWallLngth)
            {
                double qtyOfPanelWall = lengthOfLine / standardWallLngth;
                double qtyOfStandardPanelWall = Math.Truncate(qtyOfPanelWall);
                if ((qtyOfPanelWall % 1) != 0)
                {
                    double totalLength = qtyOfStandardPanelWall * standardWallLngth;
                    double difference = Math.Abs(lengthOfLine - totalLength);
                    double subtractLength = (standardWallLngth + difference) - PanelLayout_UI.minWidthOfPanel;
                    if (difference < PanelLayout_UI.minWidthOfPanel)
                    {
                        for (int i = 0; i < qtyOfStandardPanelWall; i++)
                        {
                            if (i == (qtyOfStandardPanelWall - 1))
                            {
                                listOfWallPanelLength.Add((subtractLength));
                            }
                            else
                            {
                                listOfWallPanelLength.Add(standardWallLngth);
                            }
                        }
                        listOfWallPanelLength.Add(PanelLayout_UI.minWidthOfPanel);
                    }
                    else
                    {
                        for (int i = 0; i < qtyOfStandardPanelWall; i++)
                        {
                            listOfWallPanelLength.Add(standardWallLngth);
                        }
                        if (AEE_Utility.CustType == eCustType.WNPanel)
                        {
                            double valueToMinus = CommonModule.deckPanelMinWidth - difference;
                            if (difference < CommonModule.deckPanelMinWidth)
                            {
                                difference = CommonModule.deckPanelMinWidth;
                                double diff = standardWallLngth - valueToMinus;
                                double ind = qtyOfStandardPanelWall;
                                listOfWallPanelLength.RemoveAt(Convert.ToInt32(ind) - 1);
                                listOfWallPanelLength.Insert(Convert.ToInt32(ind) - 1, diff);


                            }
                        }//Changes made on 10/07/2023 by PRT

                        listOfWallPanelLength.Add(difference);
                    }
                }
                else
                {
                    for (int i = 0; i < qtyOfStandardPanelWall; i++)
                    {
                        listOfWallPanelLength.Add(standardWallLngth);
                    }
                }
            }
            else
            {
                listOfWallPanelLength.Add(lengthOfLine);
            }

            if (listOfWallPanelLength.Count > 1)
            {
                var len = listOfWallPanelLength.Last() + listOfWallPanelLength[listOfWallPanelLength.Count - 2];
                if (len > 450 && len <= 600)
                {
                    listOfWallPanelLength[listOfWallPanelLength.Count - 2] = 300;
                    listOfWallPanelLength[listOfWallPanelLength.Count - 1] = len - 300;
                }
            }
            return listOfWallPanelLength;
        }

        public List<double> getListOfDeckPanelSpanLength(double lengthOfLine, double span_StndrdLngth, double span_MaxLngth, double span_MinLngth, double deckPanelGap)
        {
            List<double> listOfDeckPanelSpanLength = new List<double>();

            if (lengthOfLine > span_MaxLngth)
            {
                double qtyOfWallPanel = lengthOfLine / span_MaxLngth;
                double propQty = Math.Truncate(qtyOfWallPanel);

                if ((qtyOfWallPanel % 1) != 0)
                {
                    propQty = Math.Truncate(qtyOfWallPanel);
                }
                else
                {
                    propQty = qtyOfWallPanel - 1;
                }

                double remainingLength = lengthOfLine - (propQty * deckPanelGap);
                double splitStdLengthApprox = remainingLength / (propQty + 1);
                var splitStdLength = (splitStdLengthApprox % 50) != 0 ? (Math.Truncate(splitStdLengthApprox / 50) + 1) * 50 : splitStdLengthApprox;
                var curLen = remainingLength;
                while (curLen > 1e-06)
                {
                    var len = curLen > splitStdLength ? splitStdLength : curLen;
                    curLen -= splitStdLength;
                    listOfDeckPanelSpanLength.Add(len);
                }
            }
            else
            {
                listOfDeckPanelSpanLength.Add(lengthOfLine);
            }

            return listOfDeckPanelSpanLength;
        }

        //Added on 08/06/2023 by SDM
        public List<double> getListOfSP1SpanLength(double lengthOfLine)
        {
            List<double> listOfDeckPanelSpanLength = new List<double>();
            double span_MaxLngth = CommonModule.deckPanel_MSP1_MaxLngth;
            double deckPanelGap = CommonModule.deckPanel_SF_Lngth;
            double splitStdLengthApprox = 0;
            if (lengthOfLine > span_MaxLngth)
            {
                double qtyOfWallPanel = lengthOfLine / span_MaxLngth;
                double propQty = Math.Truncate(qtyOfWallPanel);

                if ((qtyOfWallPanel % 1) != 0)
                {
                    propQty = Math.Truncate(qtyOfWallPanel);
                }
                else
                {
                    propQty = qtyOfWallPanel - 1;
                }

                double remainingLength = lengthOfLine - (propQty * deckPanelGap);

                var end_panel_length = CommonModule.deckPanel_ESP1_MinLngth;
                if (propQty >= 2 && remainingLength - 2 * end_panel_length >= CommonModule.deckPanel_MSP1_MinLngth)
                {
                    var _remain_l = remainingLength - 2 * end_panel_length;

                    var nbr = propQty - 1;
                    listOfDeckPanelSpanLength = BetterSP1Split(_remain_l, nbr, out splitStdLengthApprox);
                    if (splitStdLengthApprox >= CommonModule.deckPanel_MSP1_MinLngth && splitStdLengthApprox <= CommonModule.deckPanel_MSP1_MaxLngth && listOfDeckPanelSpanLength.Any(X => X < CommonModule.deckPanel_MSP1_MinLngth))
                    {
                        double small_panel = listOfDeckPanelSpanLength.FirstOrDefault2(X => X < CommonModule.deckPanel_MSP1_MinLngth);
                        listOfDeckPanelSpanLength.Remove(small_panel);

                        listOfDeckPanelSpanLength.AddRange(BetterSP1Split(small_panel + 2 * end_panel_length, 2, out splitStdLengthApprox));
                    }
                    else if ((splitStdLengthApprox > CommonModule.deckPanel_MSP1_MaxLngth && nbr == 1) || (splitStdLengthApprox < CommonModule.deckPanel_MSP1_MinLngth && nbr == 2))
                    {
                        var diff = _remain_l - CommonModule.deckPanel_MSP1_MaxLngth;
                        listOfDeckPanelSpanLength = new List<double> { CommonModule.deckPanel_MSP1_MaxLngth };
                        var ends = BetterSP1Split(diff + end_panel_length * 2, 2, out end_panel_length);
                        if (ends.Count >= 2)
                        {
                            listOfDeckPanelSpanLength.Insert(0, ends[0]);
                            listOfDeckPanelSpanLength.Add(ends[1]);
                        }
                    }
                    else if (splitStdLengthApprox > CommonModule.deckPanel_MSP1_MaxLngth)
                    {
                        var diff = (splitStdLengthApprox - CommonModule.deckPanel_MSP1_MaxLngth) * nbr;
                        if (listOfDeckPanelSpanLength.Any(X => X < CommonModule.deckPanel_MSP1_MinLngth))
                        {
                            double small_panel = listOfDeckPanelSpanLength.FirstOrDefault2(X => X < CommonModule.deckPanel_MSP1_MinLngth);
                            diff += small_panel;
                        }
                        listOfDeckPanelSpanLength = BetterSP1Split(_remain_l - diff, nbr, out splitStdLengthApprox);
                        var ends = BetterSP1Split(diff + end_panel_length * 2, 2, out end_panel_length);
                        if (ends.Count >= 2)
                        {
                            listOfDeckPanelSpanLength.Insert(0, ends[0]);
                            listOfDeckPanelSpanLength.Add(ends[1]);
                        }
                    }
                    else
                    {
                        listOfDeckPanelSpanLength.Insert(0, end_panel_length);
                        listOfDeckPanelSpanLength.Add(end_panel_length);
                    }
                }
                else
                {
                    double _remain_l = lengthOfLine - deckPanelGap;
                    listOfDeckPanelSpanLength = BetterSP1Split(_remain_l, 2, out splitStdLengthApprox);
                }
            }
            else if (lengthOfLine > CommonModule.deckPanel_ESP1_MaxLngth)
            {
                double _remain_l = lengthOfLine - deckPanelGap;
                listOfDeckPanelSpanLength = BetterSP1Split(_remain_l, 2, out splitStdLengthApprox);
            }
            else
            {
                listOfDeckPanelSpanLength.Add(lengthOfLine);
            }

            return listOfDeckPanelSpanLength;
        }

        //Added on 08/06/2023 by SDM
        private static List<double> BetterSP1Split(double lengthOfLine, double nbr, out double splitStdLength)
        {
            splitStdLength = 0;
            List<double> listOfDeckPanelSpanLength = new List<double>();
            if (nbr <= 0)
                return new List<double> { lengthOfLine };

            var splitStdLengthApprox = lengthOfLine / nbr;
            splitStdLength = (splitStdLengthApprox % 50) != 0 ? (Math.Truncate(splitStdLengthApprox / 50) + 1) * 50 : splitStdLengthApprox;

            if (splitStdLength > CommonModule.deckPanel_MSP1_MaxLngth && nbr > 1)
            {
                splitStdLength -= 50;
            }
            var curLen = lengthOfLine;
            while (curLen > 1e-06)
            {
                var len = curLen > splitStdLength ? splitStdLength : curLen;
                curLen -= splitStdLength;
                listOfDeckPanelSpanLength.Add(len);
            }
            return listOfDeckPanelSpanLength;
        }

        private List<double> getListOfDeckPanelStandardSpanLength(double lengthOfLine, double span_StndrdLngth, double span_MaxLngth, double span_MinLngth, double deckPanelGap, double slabJointPanelDepth)
        {
            List<double> listOfDeckPanelSpanLength = new List<double>();

            if (AEE_Utility.CustType == eCustType.IHPanel)
            {
                if (lengthOfLine > span_MaxLngth)
                {
                    double qtyOfWallPanel = lengthOfLine / span_MaxLngth;
                    double propQty = Math.Truncate(qtyOfWallPanel);

                    if ((qtyOfWallPanel % 1) != 0)
                    {
                        propQty = Math.Truncate(qtyOfWallPanel);
                    }
                    else
                    {
                        propQty = qtyOfWallPanel - 1;
                    }

                    double remainingLength = lengthOfLine - (propQty * deckPanelGap);
                    double splitStdLengthApprox = remainingLength / (propQty + 1);
                    var splitStdLength = (splitStdLengthApprox % 50) != 0 ? (Math.Truncate(splitStdLengthApprox / 50) + 1) * 50 : splitStdLengthApprox;
                    var curLen = remainingLength;
                    while (curLen > 1e-06)
                    {
                        var len = curLen > splitStdLength ? splitStdLength : curLen;
                        curLen -= splitStdLength;
                        listOfDeckPanelSpanLength.Add(len);
                    }
                }
                else
                {
                    listOfDeckPanelSpanLength.Add(lengthOfLine);
                }
            }
            else if (AEE_Utility.CustType == eCustType.WNPanel)
            {
                if (lengthOfLine > CommonModule.deckPanel_Span_StndrdLngth)
                {
                    double qtyOfWallPanel = lengthOfLine / CommonModule.deckPanel_Span_StndrdLngth;
                    double propQty = Math.Truncate(qtyOfWallPanel);

                    if ((qtyOfWallPanel % 1) != 0)
                    {
                        propQty = Math.Truncate(qtyOfWallPanel);
                    }
                    else
                    {
                        propQty = qtyOfWallPanel - 1;
                    }

                    double remainingLength = lengthOfLine - (propQty * deckPanelGap);
                    double splitStdLengthApprox = remainingLength / (propQty + 1);
                    var splitStdLength = (splitStdLengthApprox % 50) != 0 ? (Math.Truncate(splitStdLengthApprox / 50) + 1) * 50 : splitStdLengthApprox;
                    var curLen = remainingLength;
                    while (curLen > 1e-06)
                    {
                        var len = curLen > CommonModule.deckPanel_Span_StndrdLngth ? CommonModule.deckPanel_Span_StndrdLngth : curLen;

                        double valueToBeMinus = CommonModule.deckPanel_Span_MinLngth - len;
                        if (len < CommonModule.deckPanel_Span_MinLngth)
                        {
                            len = CommonModule.deckPanel_Span_MinLngth;
                            double diff = CommonModule.deckPanel_Span_StndrdLngth - valueToBeMinus;
                            listOfDeckPanelSpanLength.RemoveAt(listOfDeckPanelSpanLength.Count - 1);
                            listOfDeckPanelSpanLength.Insert(listOfDeckPanelSpanLength.Count, diff);
                        }
                        //Changes made on 10/07/2023 by PRT

                        curLen -= CommonModule.deckPanel_Span_StndrdLngth;
                        listOfDeckPanelSpanLength.Add(len);
                    }
                }
                else
                {
                    listOfDeckPanelSpanLength.Add(lengthOfLine);
                }//Changes made on 07/07/2023 by PRT
            }

            return listOfDeckPanelSpanLength;
        }

        private double checkLastDeckPlane_SpanLength(double totalLngthOfDeckPanel, double lengthOfLine, double deckPanelWallLngth, double deckPanelGap)
        {
            double deckPanelSpanLngth = deckPanelWallLngth;
            totalLngthOfDeckPanel = totalLngthOfDeckPanel + deckPanelWallLngth + deckPanelGap;

            double remainingLngth = lengthOfLine - totalLngthOfDeckPanel;

            if (remainingLngth < 0)
            {
                double diff = deckPanelWallLngth + (deckPanelGap - Math.Abs(remainingLngth));
                double equal = Math.Truncate(diff / 2);
                if ((equal % 1) != 0)
                {
                    deckPanelSpanLngth = Math.Truncate(equal) + 1;
                }
                else
                {
                    deckPanelSpanLngth = equal;
                }
            }
            return deckPanelSpanLngth;
        }

        //private List<double> getListOfDeckPlane_SF_Interval(double lengthOfLine, double sp1_StndrdLngth, double sp1_MaxLngth, double sp1_MinLngth, double deckPanel_SF_Lngth)
        //{
        //    List<double> listOfDeckPlane_SF_Interval = new List<double>();

        //    if (lengthOfLine > sp1_StndrdLngth)
        //    {
        //        for (double totalLngthOfDeckPanel = 0; ;)
        //        {
        //            double remainingLngth = lengthOfLine - totalLngthOfDeckPanel;

        //            if (totalLngthOfDeckPanel <= (lengthOfLine - sp1_StndrdLngth))
        //            {
        //                if (remainingLngth <= sp1_MaxLngth)
        //                {
        //                    listOfDeckPlane_SF_Interval.Add(remainingLngth);
        //                    totalLngthOfDeckPanel = totalLngthOfDeckPanel + remainingLngth + deckPanel_SF_Lngth;
        //                }
        //                else if (remainingLngth > sp1_MaxLngth)
        //                {
        //                    listOfDeckPlane_SF_Interval.Add(sp1_MaxLngth);
        //                    totalLngthOfDeckPanel = totalLngthOfDeckPanel + sp1_MaxLngth + deckPanel_SF_Lngth;
        //                }
        //                else if (remainingLngth <= sp1_StndrdLngth)
        //                {
        //                    listOfDeckPlane_SF_Interval.Add(sp1_StndrdLngth);
        //                    totalLngthOfDeckPanel = totalLngthOfDeckPanel + sp1_StndrdLngth + deckPanel_SF_Lngth;
        //                }
        //            }
        //            else
        //            {
        //                if (remainingLngth > 0)
        //                {
        //                    if (listOfDeckPlane_SF_Interval.Count != 0)
        //                    {
        //                        int lastIndex = listOfDeckPlane_SF_Interval.Count - 1;
        //                        double lastSpanLength = listOfDeckPlane_SF_Interval[lastIndex];

        //                        //double diff = (sp1_MinLngth - remainingLngth);
        //                        double totalSF_Intval = lastSpanLength + remainingLngth;

        //                        double SF_Interval1 = 0;
        //                        double SF_Interval2 = 0;
        //                        double equalToSFIntrval = totalSF_Intval / 2;
        //                        if ((totalSF_Intval % 1) == 0)
        //                        {
        //                            SF_Interval2 = Math.Truncate(equalToSFIntrval);
        //                            SF_Interval1 = SF_Interval2 + 1;
        //                        }
        //                        else
        //                        {
        //                            SF_Interval2 = equalToSFIntrval;
        //                            SF_Interval1 = equalToSFIntrval;
        //                        }
        //                        listOfDeckPlane_SF_Interval[lastIndex] = SF_Interval1;

        //                        listOfDeckPlane_SF_Interval.Add(SF_Interval2);
        //                    }
        //                    else
        //                    {
        //                        listOfDeckPlane_SF_Interval.Add(remainingLngth);
        //                    }
        //                }
        //                break;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        listOfDeckPlane_SF_Interval.Add(lengthOfLine);
        //    }

        //    return listOfDeckPlane_SF_Interval;
        //}

        public List<double> getListOfDeckPlane_SF_Interval(double lengthOfLine, double sp1_StndrdLngth, double sp1_MaxLngth, double sp1_MinLngth, double deckPanel_SF_Lngth)
        {
            List<double> listOfDeckPlane_SF_Interval = new List<double>();

            if (lengthOfLine > sp1_StndrdLngth)
            {
                for (double totalLngthOfDeckPanel = 0; ;)
                {
                    double remainingLngth = lengthOfLine - totalLngthOfDeckPanel;

                    if (totalLngthOfDeckPanel <= (lengthOfLine - sp1_StndrdLngth))
                    {
                        if (remainingLngth <= sp1_MaxLngth)
                        {
                            double deckPanelSFLngth = checkLastDeckPlane_SF_Interval(totalLngthOfDeckPanel, lengthOfLine, remainingLngth, deckPanel_SF_Lngth);
                            listOfDeckPlane_SF_Interval.Add(deckPanelSFLngth);
                            totalLngthOfDeckPanel = totalLngthOfDeckPanel + deckPanelSFLngth + deckPanel_SF_Lngth;
                        }
                        else if (remainingLngth > sp1_MaxLngth)
                        {
                            double deckPanelSFLngth = checkLastDeckPlane_SF_Interval(totalLngthOfDeckPanel, lengthOfLine, sp1_MaxLngth, deckPanel_SF_Lngth);
                            listOfDeckPlane_SF_Interval.Add(deckPanelSFLngth);
                            totalLngthOfDeckPanel = totalLngthOfDeckPanel + deckPanelSFLngth + deckPanel_SF_Lngth;
                        }
                        else if (remainingLngth <= sp1_StndrdLngth)
                        {
                            double deckPanelSFLngth = checkLastDeckPlane_SF_Interval(totalLngthOfDeckPanel, lengthOfLine, sp1_StndrdLngth, deckPanel_SF_Lngth);

                            listOfDeckPlane_SF_Interval.Add(deckPanelSFLngth);
                            totalLngthOfDeckPanel = totalLngthOfDeckPanel + deckPanelSFLngth + deckPanel_SF_Lngth;
                        }
                    }
                    else
                    {
                        if (remainingLngth > 0)
                        {
                            if (remainingLngth <= sp1_MinLngth && listOfDeckPlane_SF_Interval.Count != 0)
                            {
                                int lastIndex = listOfDeckPlane_SF_Interval.Count - 1;
                                double lastSpanLength = listOfDeckPlane_SF_Interval[lastIndex];

                                double diff = sp1_MinLngth - remainingLngth;
                                listOfDeckPlane_SF_Interval[lastIndex] = (lastSpanLength - diff);

                                listOfDeckPlane_SF_Interval.Add(sp1_MinLngth);
                            }
                            else
                            {
                                listOfDeckPlane_SF_Interval.Add(remainingLngth);
                            }
                        }
                        break;
                    }
                }
            }
            else
            {
                listOfDeckPlane_SF_Interval.Add(lengthOfLine);
            }

            return listOfDeckPlane_SF_Interval;
        }

        private double checkLastDeckPlane_SF_Interval(double totalLngthOfDeckPanel, double lengthOfLine, double deckPanelWallLngth, double deckPanel_SF_Lngth)
        {
            double deckPanelSFLngth = deckPanelWallLngth;
            totalLngthOfDeckPanel = totalLngthOfDeckPanel + deckPanelWallLngth + deckPanel_SF_Lngth;

            double remainingLngth = lengthOfLine - totalLngthOfDeckPanel;

            if (remainingLngth < 0)
            {
                double diff = deckPanelWallLngth + (deckPanel_SF_Lngth - Math.Abs(remainingLngth));
                double equal = Math.Truncate(diff / 2);
                if ((equal % 1) != 0)
                {
                    deckPanelSFLngth = Math.Truncate(equal) + 1;
                }
                else
                {
                    deckPanelSFLngth = equal;
                }
            }

            return deckPanelSFLngth;
        }

        public void drawDeckPlatePanel()
        {
            RibbonHelper.insertDeckPanelButton.IsEnabled = false;

            string deckPanelCreationMsg = "Deck panels are creating....";
            ProgressForm progressForm = new ProgressForm();
            progressForm.Show();

            CommonModule.Deck_panel_CLine = new List<ElevationModule.Deck_panel_CLine>();

            for (int index = 0; index < listOfListOfDeckPanelWallId.Count; index++)
            {
                if ((index % 5) == 0)
                {
                    progressForm.ReportProgress(1, deckPanelCreationMsg);
                }
                List<double> listOfIntervalLngthDeckPanel = listOfListOfIntrvalLengthOfDeckPanel[index];
                List<ObjectId> listOfDeckPanelWallId = listOfListOfDeckPanelWallId[index];
                List<double> listOfDeckPanelSpanLngth = listOfListOfDeckPanelSpanLngth[index];
                List<double> listOfDeckPlane_SF_Interval = listOfListOfDeckPlane_SF_Interval[index];

                MaxAreaRelationInfo relation = lstMaxAreaRelations[index];

                // Cuong - 23/12/2022 - DeckPanel issue - START
                List<MaxAreaRelationInfo> listOfRelation = listOfListOfRelationInfo[index];
                // Cuong - 23/12/2022 - DeckPanel issue - END

                drawDeckPlate(listOfDeckPanelWallId, listOfDeckPanelSpanLngth, listOfIntervalLngthDeckPanel, listOfDeckPlane_SF_Interval, relation, listOfRelation);
            }

            deckPanelCreationMsg = "Deck panels are created.";
            progressForm.ReportProgress(100, deckPanelCreationMsg);
            progressForm.Close();

            if (!CommandModule.dply)
                MessageBox.Show(deckPanelCreationMsg, CommonModule.headerText_messageBox, MessageBoxButtons.OK, MessageBoxIcon.Information);
            //Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.SendStringToExecute((String)RibbonHelper.rebuildPanelButton.CommandParameter, true, false, true);

            if (Licensing.LicenseUtilities.hasElevation)
                RibbonHelper.createElevationPlanButton.IsEnabled = true;
            else
            {
                RibbonHelper.rebuildPanelButton.IsEnabled = true;
                RibbonHelper.updateWallButton.IsEnabled = true;//Added on 10/06/2023 by SDM
            }
        }

        private void drawDeckPlate(List<ObjectId> listOfDeckPanelWallId, List<double> listOfDeckPanelSpanLngth,
            List<double> listOfIntervalLngthDeckPanel, List<double> listOfDeckPlane_SF_Interval, MaxAreaRelationInfo relation, List<MaxAreaRelationInfo> listOfRelation)
        {
            //Vector3d moveVector = CreateShellPlanHelper.moveVector_ForSlabJointLayout;
            Vector3d moveVector = new Vector3d(0, 0, 0);
            for (int j = 0; j < listOfDeckPanelWallId.Count; j++)
            {
                if (j == (listOfDeckPanelWallId.Count - 1))
                {
                    break;
                }
                double deckPanelInterval = listOfIntervalLngthDeckPanel[j];

                ObjectId deckPanelId1 = listOfDeckPanelWallId[j];
                ObjectId deckPanelId2 = listOfDeckPanelWallId[(j + 1)];

                string xDataRegAppName = AEE_Utility.GetXDataRegisterAppName(deckPanelId1);

                drawDeckPanelRectangle(xDataRegAppName, j, deckPanelId1, deckPanelId2, listOfDeckPanelSpanLngth, listOfDeckPlane_SF_Interval, deckPanelInterval, moveVector, relation, listOfRelation);
            }
        }



        private void drawDeckPanelRectangle(string xDataRegAppName, int indexOfDeckPanelId, ObjectId deckPanelId1, ObjectId deckPanelId2, List<double> listOfDeckPanelSpanLngth, List<double> listOfDeckPlane_SF_Interval,
            double deckPanelInterval, Vector3d moveVector, MaxAreaRelationInfo relation, List<MaxAreaRelationInfo> listOfRelation)
        {
            var deckPanelEnt = AEE_Utility.GetEntityForRead(deckPanelId1);
            List<Point3d> listOfDeckPanel_1_StrtEndPoint = AEE_Utility.GetStartEndPointOfLine(deckPanelId1);
            Point3d deckPnl_1_StrtPnt = listOfDeckPanel_1_StrtEndPoint[0];
            Point3d deckPnl_1_EndPnt = listOfDeckPanel_1_StrtEndPoint[1];

            List<Point3d> listOfDeckPanel_2_StrtEndPoint = AEE_Utility.GetStartEndPointOfLine(deckPanelId2);
            Point3d deckPnl_2_StrtPnt = listOfDeckPanel_2_StrtEndPoint[0];
            Point3d deckPnl_2_EndPnt = listOfDeckPanel_2_StrtEndPoint[1];

            Point3d strtPoint1 = deckPnl_1_StrtPnt;
            Point3d strtPoint2 = deckPnl_2_StrtPnt;

            WallPanelHelper wallPanelHlp = new WallPanelHelper();

            for (int j = 0; j < listOfDeckPanelSpanLngth.Count; j++)
            {
                double deckPanelSpanLength = listOfDeckPanelSpanLngth[j];
                bool skipEntCreation = false;
                if (deckPanelSpanLength < 0)
                {
                    deckPanelSpanLength = 0;
                    skipEntCreation = true;
                }
                var angleOfLine = AEE_Utility.GetAngleOfLine(deckPnl_1_StrtPnt, deckPnl_1_EndPnt);
                Point2d lengthPoint = AEE_Utility.get_XY(angleOfLine, deckPanelSpanLength);
                Point3d endPoint1 = new Point3d((strtPoint1.X + lengthPoint.X), (strtPoint1.Y + lengthPoint.Y), 0);
                Point3d endPoint2 = new Point3d((strtPoint2.X + lengthPoint.X), (strtPoint2.Y + lengthPoint.Y), 0);

                if (!skipEntCreation)
                {

                    ObjectId deckWallLineId = AEE_Utility.getLineId(deckPanelEnt, strtPoint1, endPoint1, false);
                    double deckWallLineAngle = AEE_Utility.GetAngleOfLine(deckWallLineId);

                    List<ObjectId> listOfMoveId = new List<ObjectId>();

                    var deckPanelRectId = AEE_Utility.createRectangle(strtPoint1, endPoint1, endPoint2, strtPoint2, CommonModule.deckPanelWallLayerName, CommonModule.deckPanelWallLayerColor);
                    AEE_Utility.AttachXData(deckPanelRectId, xDataRegAppName, CommonModule.xDataAsciiName);
                    listOfMoveId.Add(deckPanelRectId);

                    Point3d dimTextPoint = WallPanelHelper.getCenterPointOfPanelRectangle(deckPanelRectId);

                    string deckPanelText = getWallPanelText(deckPanelInterval, PanelLayout_UI.deckPanelName, deckPanelSpanLength);
                    string wallDescp = deckPanelText;
                    ObjectId dimTextId2 = wallPanelHlp.writeDimensionTextInWallPanel(deckPanelText, deckWallLineId, dimTextPoint, deckWallLineAngle, CommonModule.deckPanelWallTextLayerName, CommonModule.deckPanelWallLayerColor);
                    listOfMoveId.Add(dimTextId2);

                    wallPanelHlp.setBOMDataOfWallPanel(dimTextId2, deckPanelRectId, deckWallLineId, deckPanelInterval, 0, deckPanelSpanLength, PanelLayout_UI.deckPanelName, wallDescp, CommonModule.deckPanelType);

                    AEE_Utility.MoveEntity(listOfMoveId, moveVector);

                    Deck_panel_CLine Deck_panel_CLine = new Deck_panel_CLine();
                    Deck_panel_CLine.startpoint1 = strtPoint1;
                    Deck_panel_CLine.endpoint1 = endPoint1;
                    Deck_panel_CLine.startpoint2 = strtPoint2;
                    Deck_panel_CLine.endpoint2 = endPoint2;

                    Deck_panel_CLine.pitch = 100;
                    if (deckWallLineAngle == 0)
                    {
                        Deck_panel_CLine.angle = 0;

                    }
                    else if (deckWallLineAngle == 90)
                    {

                        Deck_panel_CLine.angle = 90;

                    }

                    CommonModule.Deck_panel_CLine.Add(Deck_panel_CLine);


                }

                Point3d gapStrtPoint1 = endPoint1;
                Point3d gapStrtPoint2 = endPoint2;

                Point2d gapPoint = AEE_Utility.get_XY(angleOfLine, CommonModule.deckPanelGap);
                Point3d gapEndPoint1 = new Point3d((gapStrtPoint1.X + gapPoint.X), (gapStrtPoint1.Y + gapPoint.Y), 0);
                Point3d gapEndPoint2 = new Point3d((gapStrtPoint2.X + gapPoint.X), (gapStrtPoint2.Y + gapPoint.Y), 0);

                if (j != (listOfDeckPanelSpanLngth.Count - 1))
                {
                    double angleOfSFDeckPanel = AEE_Utility.GetAngleOfLine(gapStrtPoint1, gapStrtPoint2);
                    drawGapDeckPanelRectangle(xDataRegAppName, deckPanelEnt, indexOfDeckPanelId, listOfDeckPlane_SF_Interval, gapStrtPoint1, gapEndPoint1, angleOfSFDeckPanel, moveVector);



                }

                strtPoint1 = gapEndPoint1;
                strtPoint2 = gapEndPoint2;
            }
            //if (relation != null && indexOfDeckPanelId == 0)
            //{
            //    var deckPlane_SF_Intervals = getListOfDeckPanelSpanLength(Math.Round(relation.DirectionLine.Length), CommonModule.deckPanel_SP1_StndrdLngth, CommonModule.deckPanel_SP1_MaxLngth, CommonModule.deckPanel_SP1_MinLngth, CommonModule.deckPanel_SF_Lngth);

            //    var l = new Line(relation.DirectionLine.StartPoint, relation.DirectionLine.EndPoint);
            //    var angle = l.Angle;
            //    double degrees = (180 / Math.PI) * angle;
            //    angle = Math.Round(degrees, 3);
            //    //draw_SF_DeckPanelRectangle(xDataRegAppName, deckPanelEnt, gapEndPoint1, gapStrtPoint1, gapStrtPoint2, gapEndPoint2, angleOfSFDeckPanel, moveVector);
            //    drawGapDeckPanelRectangle(xDataRegAppName, deckPanelEnt, indexOfDeckPanelId, deckPlane_SF_Intervals,
            //        relation.GapBaseLineSegment.StartPoint, relation.GapBaseLineSegment.EndPoint, angle, new Vector3d(0, 0, 0));
            //}

            // Cuong - 23/12/2022 - DeckPanel issue - START
            if (indexOfDeckPanelId == 0)
            {
                foreach (var info in listOfRelation)
                {
                    var deckPlane_SF_Intervals = getListOfSP1SpanLength(Math.Round(info.DirectionLine.Length));// getListOfDeckPanelSpanLength(Math.Round(info.DirectionLine.Length), CommonModule.deckPanel_MSP1_StndrdLngth, CommonModule.deckPanel_MSP1_MaxLngth, CommonModule.deckPanel_MSP1_MinLngth, CommonModule.deckPanel_SF_Lngth);//Changes made on 08/06/2023 by SDM

                    var l = new Line(info.DirectionLine.StartPoint, info.DirectionLine.EndPoint);
                    var angle = l.Angle;
                    double degrees = (180 / Math.PI) * angle;
                    angle = Math.Round(degrees, 3);

                    drawGapDeckPanelRectangle(xDataRegAppName, deckPanelEnt, deckPlane_SF_Intervals,
                        info.GapBaseLineSegment.StartPoint, info.GapBaseLineSegment.EndPoint, angle, new Vector3d(0, 0, 0));
                }
            }
            // Cuong - 23/12/2022 - DeckPanel issue - END
        }

        private void drawGapDeckPanelRectangle(string xDataRegAppName, Entity deckPanelEnt, List<double> listOfDeckPlane_SF_Interval, Point3d gapStartPoint1, Point3d gapEndPnt1, double angleOfSFDeckPanel, Vector3d moveVector)
        {
            WallPanelHelper wallPanelHlp = new WallPanelHelper();
            Point3d gapStrtPoint1 = gapStartPoint1;
            Point3d gapEndPoint1 = gapEndPnt1;
            for (int index = 0; index < listOfDeckPlane_SF_Interval.Count; index++)
            {
                List<ObjectId> listOfMoveId = new List<ObjectId>();

                double SF_Interval = listOfDeckPlane_SF_Interval[index];
                Point2d SF_Point = AEE_Utility.get_XY(angleOfSFDeckPanel, SF_Interval);

                Point3d gapStrtPoint2 = new Point3d((gapStrtPoint1.X + SF_Point.X), (gapStrtPoint1.Y + SF_Point.Y), 0);
                Point3d gapEndPoint2 = new Point3d((gapEndPoint1.X + SF_Point.X), (gapEndPoint1.Y + SF_Point.Y), 0);

                ObjectId deckWallLineId = AEE_Utility.getLineId(deckPanelEnt, gapStrtPoint1, gapStrtPoint2, false);

                var deckPanelRectId = AEE_Utility.createRectangle(gapStrtPoint1, gapEndPoint1, gapEndPoint2, gapStrtPoint2, CommonModule.deckPanelWallLayerName, CommonModule.deckPanelWallLayerColor);
                AEE_Utility.AttachXData(deckPanelRectId, xDataRegAppName, CommonModule.xDataAsciiName);
                listOfMoveId.Add(deckPanelRectId);


                Deck_panel_CLine Deck_panel_CLine = new Deck_panel_CLine();
                Deck_panel_CLine.startpoint1 = gapStrtPoint1;
                Deck_panel_CLine.endpoint1 = gapEndPoint1;
                Deck_panel_CLine.startpoint2 = gapStrtPoint2;
                Deck_panel_CLine.endpoint2 = gapEndPoint2;

                Deck_panel_CLine.angle = 0;
                Deck_panel_CLine.pitch = 50;

                CommonModule.Deck_panel_CLine.Add(Deck_panel_CLine);


                Point3d dimTextPoint = WallPanelHelper.getCenterPointOfPanelRectangle(deckPanelRectId);
                double deckPanelGap = CommonModule.deckPanelGap;

                //Changes made on 08/06/2023 by SDM
                string deckPanel_SP1_Name = PanelLayout_UI.deckPanel_MSP1_Name;
                if (index == 0 || index == listOfDeckPlane_SF_Interval.Count - 1)
                    deckPanel_SP1_Name = PanelLayout_UI.deckPanel_ESP1_Name;

                string deckPanelText = getWallPanelText(deckPanelGap, deckPanel_SP1_Name, SF_Interval);
                string wallDescp = deckPanelText;
                ObjectId dimTextId2 = wallPanelHlp.writeDimensionTextInWallPanel(deckPanelText, deckWallLineId, dimTextPoint, angleOfSFDeckPanel, CommonModule.deckPanelWallTextLayerName, CommonModule.deckPanelWallLayerColor);
                listOfMoveId.Add(dimTextId2);

                wallPanelHlp.setBOMDataOfWallPanel(dimTextId2, deckPanelRectId, deckWallLineId, deckPanelGap, 0, SF_Interval, deckPanel_SP1_Name, wallDescp, CommonModule.deckPanelType);
                AEE_Utility.MoveEntity(listOfMoveId, moveVector);

                Point2d SF_GapPoint = AEE_Utility.get_XY(angleOfSFDeckPanel, CommonModule.deckPanel_SF_Lngth);
                gapStrtPoint1 = new Point3d((gapStrtPoint2.X + SF_GapPoint.X), (gapStrtPoint2.Y + SF_GapPoint.Y), 0);
                gapEndPoint1 = new Point3d((gapEndPoint2.X + SF_GapPoint.X), (gapEndPoint2.Y + SF_GapPoint.Y), 0);

                if (index != (listOfDeckPlane_SF_Interval.Count - 1))
                {
                    draw_SF_DeckPanelRectangle(xDataRegAppName, deckPanelEnt, gapEndPoint1, gapStrtPoint1, gapStrtPoint2, gapEndPoint2, angleOfSFDeckPanel, moveVector);

                    Deck_panel_CLine Deck_panel_CLine_1 = new Deck_panel_CLine();
                    Deck_panel_CLine_1.startpoint1 = gapStrtPoint2;
                    Deck_panel_CLine_1.endpoint1 = gapEndPoint2;
                    Deck_panel_CLine_1.startpoint2 = gapStrtPoint1;
                    Deck_panel_CLine_1.endpoint2 = gapEndPoint1;

                    Deck_panel_CLine_1.angle = 0;
                    Deck_panel_CLine_1.pitch = 50;

                    CommonModule.Deck_panel_CLine.Add(Deck_panel_CLine_1);
                }
            }
        }

        private void drawGapDeckPanelRectangle(string xDataRegAppName, Entity deckPanelEnt, int indexOfDeckPanelId, List<double> listOfDeckPlane_SF_Interval, Point3d gapStartPoint1, Point3d gapEndPnt1, double angleOfSFDeckPanel, Vector3d moveVector)
        {
            if (indexOfDeckPanelId == 0)
            {
                WallPanelHelper wallPanelHlp = new WallPanelHelper();
                Point3d gapStrtPoint1 = gapStartPoint1;
                Point3d gapEndPoint1 = gapEndPnt1;
                for (int index = 0; index < listOfDeckPlane_SF_Interval.Count; index++)
                {
                    List<ObjectId> listOfMoveId = new List<ObjectId>();

                    double SF_Interval = listOfDeckPlane_SF_Interval[index];
                    Point2d SF_Point = AEE_Utility.get_XY(angleOfSFDeckPanel, SF_Interval);

                    Point3d gapStrtPoint2 = new Point3d((gapStrtPoint1.X + SF_Point.X), (gapStrtPoint1.Y + SF_Point.Y), 0);
                    Point3d gapEndPoint2 = new Point3d((gapEndPoint1.X + SF_Point.X), (gapEndPoint1.Y + SF_Point.Y), 0);

                    ObjectId deckWallLineId = AEE_Utility.getLineId(deckPanelEnt, gapStrtPoint1, gapStrtPoint2, false);

                    var deckPanelRectId = AEE_Utility.createRectangle(gapStrtPoint1, gapEndPoint1, gapEndPoint2, gapStrtPoint2, CommonModule.deckPanelWallLayerName, CommonModule.deckPanelWallLayerColor);
                    AEE_Utility.AttachXData(deckPanelRectId, xDataRegAppName, CommonModule.xDataAsciiName);
                    listOfMoveId.Add(deckPanelRectId);


                    Deck_panel_CLine Deck_panel_CLine = new Deck_panel_CLine();
                    Deck_panel_CLine.startpoint1 = gapStrtPoint1;
                    Deck_panel_CLine.endpoint1 = gapEndPoint1;
                    Deck_panel_CLine.startpoint2 = gapStrtPoint2;
                    Deck_panel_CLine.endpoint2 = gapEndPoint2;

                    Deck_panel_CLine.angle = 0;
                    Deck_panel_CLine.pitch = 50;

                    CommonModule.Deck_panel_CLine.Add(Deck_panel_CLine);


                    Point3d dimTextPoint = WallPanelHelper.getCenterPointOfPanelRectangle(deckPanelRectId);
                    double deckPanelGap = CommonModule.deckPanelGap;

                    //Changes made on 08/06/2023 by SDM
                    string deckPanel_SP1_Name = PanelLayout_UI.deckPanel_MSP1_Name;
                    if (index == 0 || index == listOfDeckPlane_SF_Interval.Count - 1)
                        deckPanel_SP1_Name = PanelLayout_UI.deckPanel_ESP1_Name;

                    string deckPanelText = getWallPanelText(deckPanelGap, deckPanel_SP1_Name, SF_Interval);
                    string wallDescp = deckPanelText;
                    ObjectId dimTextId2 = wallPanelHlp.writeDimensionTextInWallPanel(deckPanelText, deckWallLineId, dimTextPoint, angleOfSFDeckPanel, CommonModule.deckPanelWallTextLayerName, CommonModule.deckPanelWallLayerColor);
                    listOfMoveId.Add(dimTextId2);

                    wallPanelHlp.setBOMDataOfWallPanel(dimTextId2, deckPanelRectId, deckWallLineId, deckPanelGap, 0, SF_Interval, deckPanel_SP1_Name, wallDescp, CommonModule.deckPanelType);
                    AEE_Utility.MoveEntity(listOfMoveId, moveVector);

                    Point2d SF_GapPoint = AEE_Utility.get_XY(angleOfSFDeckPanel, CommonModule.deckPanel_SF_Lngth);
                    gapStrtPoint1 = new Point3d((gapStrtPoint2.X + SF_GapPoint.X), (gapStrtPoint2.Y + SF_GapPoint.Y), 0);
                    gapEndPoint1 = new Point3d((gapEndPoint2.X + SF_GapPoint.X), (gapEndPoint2.Y + SF_GapPoint.Y), 0);

                    if (index != (listOfDeckPlane_SF_Interval.Count - 1))
                    {
                        draw_SF_DeckPanelRectangle(xDataRegAppName, deckPanelEnt, gapEndPoint1, gapStrtPoint1, gapStrtPoint2, gapEndPoint2, angleOfSFDeckPanel, moveVector);

                        Deck_panel_CLine Deck_panel_CLine_1 = new Deck_panel_CLine();
                        Deck_panel_CLine_1.startpoint1 = gapStrtPoint2;
                        Deck_panel_CLine_1.endpoint1 = gapEndPoint2;
                        Deck_panel_CLine_1.startpoint2 = gapStrtPoint1;
                        Deck_panel_CLine_1.endpoint2 = gapEndPoint1;

                        Deck_panel_CLine_1.angle = 0;
                        Deck_panel_CLine_1.pitch = 50;

                        CommonModule.Deck_panel_CLine.Add(Deck_panel_CLine_1);
                    }
                }
            }
        }

        private void draw_SF_DeckPanelRectangle(string xDataRegAppName, Entity deckPanelEnt, Point3d gapEndPoint1, Point3d gapStrtPoint1, Point3d gapStrtPoint2, Point3d gapEndPoint2, double angleOfSFDeckPanel, Vector3d moveVector)
        {
            WallPanelHelper wallPanelHlp = new WallPanelHelper();
            List<ObjectId> listOfMoveId = new List<ObjectId>();

            ObjectId deckWallLineId = AEE_Utility.getLineId(deckPanelEnt, gapStrtPoint1, gapStrtPoint2, false);
            ObjectId deckPanelRectId = AEE_Utility.createRectangle(gapStrtPoint2, gapEndPoint2, gapEndPoint1, gapStrtPoint1, CommonModule.deckPanelWallLayerName, CommonModule.deckPanelWallLayerColor);
            AEE_Utility.AttachXData(deckPanelRectId, xDataRegAppName, CommonModule.xDataAsciiName);
            listOfMoveId.Add(deckPanelRectId);

            Point3d dimTextPoint = WallPanelHelper.getCenterPointOfPanelRectangle(deckPanelRectId);

            double deckPanel_SF_Lngth = CommonModule.deckPanel_SF_Lngth;
            double deckPanelGap = CommonModule.deckPanelGap;
            string deckPanel_SF_Name = PanelLayout_UI.deckPanel_SF_Name;
            string deckPanelText = getWallPanelText(deckPanelGap, deckPanel_SF_Name, deckPanel_SF_Lngth);
            string wallDescp = deckPanelText;
            ObjectId dimTextId2 = wallPanelHlp.writeDimensionTextInWallPanel(deckPanelText, deckWallLineId, dimTextPoint, angleOfSFDeckPanel, CommonModule.deckPanelWallTextLayerName, CommonModule.deckPanelWallLayerColor);
            listOfMoveId.Add(dimTextId2);

            wallPanelHlp.setBOMDataOfWallPanel(dimTextId2, deckPanelRectId, deckWallLineId, deckPanelGap, 0, deckPanel_SF_Lngth, deckPanel_SF_Name, wallDescp, CommonModule.deckPanelType);

            AEE_Utility.MoveEntity(listOfMoveId, moveVector);
        }

        public static string getWallPanelText(double length, string wallPanelName, double height)
        {
            ///Change 'Length WBH Height' to 'Height WBH Length' in SDM 11_May_2022
            //string deckPanelText = Convert.ToString(length) + " " + wallPanelName + " " + Convert.ToString(height);
            string deckPanelText = "";
            if (wallPanelName == CommonModule.windowThickBottomPropText)
                deckPanelText = Convert.ToString(height) + " " + wallPanelName + " " + Convert.ToString(length);
            else
                deckPanelText = Convert.ToString(length) + " " + wallPanelName + " " + Convert.ToString(height);
            return deckPanelText;
        }
    }
}
