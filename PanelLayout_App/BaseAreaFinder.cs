using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using System.Windows;
using System.Linq;
using Autodesk.AutoCAD.Geometry;

namespace PanelLayout_App
{
    internal class BaseAreaFinder
    {
        private List<double> lstX;
        private List<double> lstY;

        private int[][] data;
        private double[][] dataArea;
        private int[][] result;
        private int maxKey;

        Dictionary<ObjectId, MaxAreaRelationInfo> dicResult = new Dictionary<ObjectId, MaxAreaRelationInfo>();
        private bool visible;
        private Polyline pl;
        private Dictionary<int, MaxAreaExistingSide> relation;
        private double gap;

        // Cuong - 20/12/2022 - DeckPanel issue - START
        public int[][] GetResult()
        {
            return result;
        }
        // Cuong - 20/12/2022 - DeckPanel issue - END

        public int GetMaxKey()
        {
            return maxKey;
        }

        public BaseAreaFinder(List<double> lstX, List<double> lstY,Polyline pl, bool visible, double gap)
        {
            this.gap = gap;
            this.visible = visible;
            this.pl = pl;
            this.lstX = lstX;
            this.lstY = lstY;
            if(lstY.Count == 0 || lstX.Count == 0)
            {
                return;
            }
            data = new int[lstY.Count - 1][];
            dataArea = new double[lstY.Count - 1][];
            for(var i = 0; i < data.Length; ++i)
            {
                data[i] = new int[lstX.Count -1];
                dataArea[i] = new double[lstX.Count - 1];
                for(var j = 0; j < lstX.Count - 1; ++j)
                {
                    data[i][j] = insideOrOn(new Point3d(lstX[j], lstY[i],0), new Point3d(lstX[j+1], lstY[i+1], 0))? 0:-1;
                    dataArea[i][j] = (lstY[i + 1] - lstY[i]) * (lstX[j + 1] - lstX[j]);
                }
            }
        }


        private bool insideOrOn(Point3d pt1, Point3d pt2)
        {
            var midPoint = new LineSegment3d(pt1, pt2).MidPoint;
            var vecs = new Vector3d[] { new Vector3d(1, 0.3, 0), new Vector3d(-1, 0.3, 0),
                new Vector3d(1, -0.3, 0) , new Vector3d(-1, -0.3, 0) };
            var cl = pl.GetClosestPointTo(midPoint, false);
            if (cl.DistanceTo(midPoint) < 1e-6)
            {
                return true;
            }
            var inside = 0;
            foreach (var vec in vecs)
            {
                var ray = new Ray() { BasePoint = midPoint, UnitDir = vec };
                var pts = new Point3dCollection();
                pl.IntersectWith(ray, Intersect.OnBothOperands, pts, IntPtr.Zero, IntPtr.Zero);
                if (pts.Count % 2 == 1)
                {
                    inside++;
                }
            }
            return inside > 2;
        }

        internal Dictionary<ObjectId, MaxAreaRelationInfo> Find()
        {
            //  CreateArray();

            if (!SplitArea())
            {
                return dicResult;
            }

            FindRectangles();
            return dicResult;
        }

        private void FindRectangles()
        {
            var dicX = new Dictionary<int, List<int>>();
            var dicY = new Dictionary<int, List<int>>();
            for (var n = 0; n < result.Length; ++n)
            {
                for(var m= 0; m< result[n].Length; ++m)
                {
                    var val = result[n][m];
                    if (!dicX.ContainsKey(val))
                    {
                        dicX[val] = new List<int>();
                    }
                    dicX[val].Add(m);
                    if (!dicY.ContainsKey(val))
                    {
                        dicY[val] = new List<int>();
                    }
                    dicY[val].Add(n);
                }
            }
            ObjectId idFirst = ObjectId.Null;
            for(var k = 1; k <= maxKey; ++k)
            {
                var res = CreateRectangle(dicX, dicY, k);
                if (res != ObjectId.Null)
                {
                    if (k == 1)
                    {
                        idFirst = res;
                        dicResult[idFirst] = null;
                    }
                    else
                    {
                        var maxInfo = GetMaxAreaRelationInfo(dicX, dicY, k, idFirst);
                        dicResult[res] = maxInfo;
                    }
                }
            }
        }

        private MaxAreaRelationInfo GetMaxAreaRelationInfo(Dictionary<int, List<int>> dicX, 
            Dictionary<int, List<int>> dicY, int k, ObjectId idFirst)
        {
            var xMin = dicX[k].Min();
            var xMax = dicX[k].Max();
            var yMin = dicY[k].Min();
            var yMax = dicY[k].Max();
            
            var side = relation[k];
            var lGap = side == MaxAreaExistingSide.Left ? gap : 0;
            var rGap = side == MaxAreaExistingSide.Right ? -gap : 0;
            var bGap = side == MaxAreaExistingSide.Bottom ? gap : 0;
            var tGap = side == MaxAreaExistingSide.Top ? -gap : 0;

            MaxAreaRelationInfo info = new MaxAreaRelationInfo()
            {
                MaxAreaObjectId = idFirst,
            };
            switch (side)
            {
                case MaxAreaExistingSide.Left:
                    info.GapBaseLineSegment = new LineSegment3d(new Point3d(lstX[xMin], lstY[yMin], 0),
                                                    new Point3d(lstX[xMin] + gap, lstY[yMin], 0));
                    info.DirectionLine = new LineSegment3d(new Point3d(lstX[xMin], lstY[yMin], 0),
                                                    new Point3d(lstX[xMin], lstY[yMax + 1], 0));
                    break;
                case MaxAreaExistingSide.Right:
                    info.GapBaseLineSegment = new LineSegment3d(new Point3d(lstX[xMax + 1] - gap, lstY[yMin], 0),
                                                    new Point3d(lstX[xMax + 1], lstY[yMin], 0));
                    info.DirectionLine = new LineSegment3d(new Point3d(lstX[xMax + 1], lstY[yMin], 0),
                                                    new Point3d(lstX[xMax + 1], lstY[yMax + 1], 0));
                    break;
                case MaxAreaExistingSide.Bottom:
                    info.GapBaseLineSegment = new LineSegment3d(new Point3d(lstX[xMin], lstY[yMin], 0),
                                                    new Point3d(lstX[xMin], lstY[yMin] + gap, 0));
                    info.DirectionLine = new LineSegment3d(new Point3d(lstX[xMin], lstY[yMin], 0),
                                                    new Point3d(lstX[xMax + 1], lstY[yMin], 0));
                    break;
                case MaxAreaExistingSide.Top:
                    info.GapBaseLineSegment = new LineSegment3d(new Point3d(lstX[xMin], lstY[yMax + 1] - gap, 0),
                                                    new Point3d(lstX[xMin], lstY[yMax + 1] , 0));
                    info.DirectionLine = new LineSegment3d(new Point3d(lstX[xMin], lstY[yMax + 1], 0),
                                                    new Point3d(lstX[xMax + 1], lstY[yMax + 1], 0));
                    break;
            }
            return info;
        }

        private ObjectId CreateRectangle(Dictionary<int, List<int>> dicX, Dictionary<int, List<int>> dicY, int k)
        {
            var xMin = dicX[k].Min();
            var xMax = dicX[k].Max();
            var yMin = dicY[k].Min();
            var yMax = dicY[k].Max();

            var side = relation[k];
            var lGap = side == MaxAreaExistingSide.Left ? gap : 0;
            var rGap = side == MaxAreaExistingSide.Right ? -gap : 0;
            var bGap = side == MaxAreaExistingSide.Bottom ? gap : 0;
            var tGap = side == MaxAreaExistingSide.Top ? -gap : 0;


            return AEE_Utility.GetRectangleId(lstX[xMin] + lGap, lstY[yMin] + bGap, lstX[xMax + 1] + rGap, lstY[yMax + 1] + tGap, visible);
        }

        private bool SplitArea()
        {
            var obj = new SplittedAreaFinder(data, dataArea);
            if (!obj.Find())
            {
                // Cuong - 20/12/2022 - DeckPanel issue - START
                if (obj.lstInvalidDataBasedOnArea != null && obj.lstInvalidDataBasedOnArea.Count > 0)
                    result = obj.lstInvalidDataBasedOnArea[0];
                maxKey = obj.maxKey();
                // Cuong - 20/12/2022 - DeckPanel issue - END

                return false;
            }
            result = obj.Result;

            // Cuong - 20/12/2022 - DeckPanel issue - START
            if (obj.lstInvalidDataBasedOnArea != null && obj.lstInvalidDataBasedOnArea.Count > 0)
                result = obj.lstInvalidDataBasedOnArea[0];
            // Cuong - 20/12/2022 - DeckPanel issue - END

            relation = obj.Relation;
            maxKey = obj.maxKey();
            return result != null;
        }
    }

    public class MaxAreaRelationInfo
    {
        public ObjectId MaxAreaObjectId { get; set; }

        public LineSegment3d GapBaseLineSegment { get; set; }

        public LineSegment3d DirectionLine { get; set; }
    } 
}