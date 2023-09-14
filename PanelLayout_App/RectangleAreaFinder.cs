using System;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using System.Linq;

namespace PanelLayout_App
{
    internal class RectangleAreaFinder
    {
        private Point3dCollection pts;
        List<Point3d> lst = new List<Point3d>();
        private Polyline pl;

        HashSet<double> xCoOrds = new HashSet<double>(new DoubleComparer());
        HashSet<double> yCoOrds = new HashSet<double>(new DoubleComparer());
        
        private bool visible;
        private double gap;

        // Cuong - 20/12/2022 - DeckPanel issue - START
        private int[][] invalidData;
        private int maxKey;

        public int GetMaxKey()
        {
            return maxKey;
        }
        // Cuong - 20/12/2022 - DeckPanel issue - END

        public List<double> GetXCoOrds()
        {
            return new List<double>(xCoOrds);
        }

        public List<double> GetYCoOrds()
        {
            return new List<double>(yCoOrds);
        }

        public RectangleAreaFinder(Point3dCollection pts, String layer, int color, bool visible, double gap)
        {
            this.gap = gap;
            this.visible = visible;
            this.pts = pts;
        }

        // Cuong - 20/12/2022 - DeckPanel issue - START
        public int[][] FindInvalidData()
        {
            CreateRectangle1();
            return invalidData;
        }
        // Cuong - 20/12/2022 - DeckPanel issue - END

        internal static Dictionary<ObjectId, MaxAreaRelationInfo> CreateRecngle1(Point3dCollection pts, String layer, 
            int color, bool visible, double gap)
        {
            var obj = new RectangleAreaFinder(pts, layer, color, visible, gap);
            return obj.CreateRectangle1();
        }
        
        private Dictionary<ObjectId, MaxAreaRelationInfo> CreateRectangle1()
        {
            RemoveSameDirections();

            MakeStraightLines();

            CreateLineSegments();
                        
            return BuildNeighbours();
        }

        private Dictionary<ObjectId, MaxAreaRelationInfo> BuildNeighbours()
        {
            var lstX = new List<double>(xCoOrds);
            var lstY = new List<double>(yCoOrds);

            lstX.Sort();
            lstY.Sort();

            var obj = new BaseAreaFinder(lstX, lstY,pl,visible, gap);

            // Cuong - 20/12/2022 - DeckPanel issue - START
            //return obj.Find();
            var result = obj.Find();
            invalidData = obj.GetResult();
            maxKey = obj.GetMaxKey();
            // Cuong - 20/12/2022 - DeckPanel issue - END

            return result;
        }
        
        private void CreateLineSegments()
        {
            pl = new Polyline();
            for (var n = 0; n < lst.Count - 1; ++n)
            {
                pl.AddVertexAt(n, new Point2d(lst[n].X, lst[n].Y), 0, 0, 0);
                xCoOrds.Add(lst[n].X);
                yCoOrds.Add(lst[n].Y);
            }
            if (lst.Count > 0)
            {
                pl.Closed = true;
            }
        }

        private void MakeStraightLines()
        {
            for (var n = 1; n < lst.Count; ++n)
            {
                var prevPt = lst[n - 1];
                var pt = lst[n];
                if (Math.Abs(pt.X - prevPt.X) < Math.Abs(pt.Y - prevPt.Y))
                {
                    lst[n] = new Point3d(prevPt.X, pt.Y, 0);
                }
                else
                {
                    lst[n] = new Point3d(pt.X, prevPt.Y, 0);
                }
            }
        }

        private void RemoveSameDirections()
        {
            for(var n = 0; n < pts.Count; ++n)
            {
                if(n < 2)
                {
                    lst.Add(pts[n]);
                    continue;
                }
                var lastDir = lst[lst.Count - 1] - lst[lst.Count - 2];
                var curDir = pts[n] - lst[lst.Count - 1];
                if (lastDir.IsCodirectionalTo(curDir))
                {
                    lst[lst.Count - 1] = pts[n];
                    continue;
                }
                if(n == pts.Count - 1)
                {
                    var dir1 = pts[1] - pts[0];
                    if(dir1.IsCodirectionalTo(curDir) && pts[0] == pts[n])
                    {
                        lst[0] = lst[lst.Count - 1];
                        continue;
                    }
                }
                lst.Add(pts[n]);
            }
        }
    }
}