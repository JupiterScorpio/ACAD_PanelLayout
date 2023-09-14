using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System.Linq;
using System.IO;

namespace PanelLayout_App
{
    internal class NeighbourFinder
    {
        private List<Region> filteredRectRegions;
        Dictionary<Tuple<Point3d, Point3d>, RectangleInfo> dicPtsToRect = new Dictionary<Tuple<Point3d, Point3d>, RectangleInfo>(new TupleComparer());
        private List<RectangleInfo> rectBlocks = new List<RectangleInfo>();
        
        public NeighbourFinder(List<Region> filteredRectRegions)
        {
            this.filteredRectRegions = filteredRectRegions;
        }

        internal List<RectangleInfo> Find()
        {
            BuildRectangles();

            BuildNeighbours();

            return rectBlocks;
        }

        private void BuildRectangles()
        {
            foreach (var reg in filteredRectRegions)
            {
                var rec = new RectangleInfo(reg.GeometricExtents);
                rectBlocks.Add(rec);
                dicPtsToRect[new Tuple<Point3d, Point3d>(rec.LeftLine.EndPoint, rec.LeftLine.StartPoint)] = rec;
                dicPtsToRect[new Tuple<Point3d, Point3d>(rec.RightLine.EndPoint, rec.RightLine.StartPoint)] = rec;
                dicPtsToRect[new Tuple<Point3d, Point3d>(rec.TopLine.EndPoint, rec.TopLine.StartPoint)] = rec;
                dicPtsToRect[new Tuple<Point3d, Point3d>(rec.BottomLine.EndPoint, rec.BottomLine.StartPoint)] = rec;
            }
        }

        private void BuildNeighbours()
        {
            foreach (var recCur in rectBlocks)
            {
                var tup = new Tuple<Point3d, Point3d>(recCur.LeftLine.StartPoint, recCur.LeftLine.EndPoint);
                if (dicPtsToRect.ContainsKey(tup))
                {
                    var rec = dicPtsToRect[tup];
                    rec.Left = recCur;
                    recCur.Right = rec;
                }
                tup = new Tuple<Point3d, Point3d>(recCur.RightLine.StartPoint, recCur.RightLine.EndPoint);
                if (dicPtsToRect.ContainsKey(tup))
                {
                    var rec = dicPtsToRect[tup];
                    rec.Right = recCur;
                    recCur.Left = rec;
                }
                tup = new Tuple<Point3d, Point3d>(recCur.TopLine.StartPoint, recCur.TopLine.EndPoint);
                if (dicPtsToRect.ContainsKey(tup))
                {
                    var rec = dicPtsToRect[tup];
                    rec.Top = recCur;
                    recCur.Bottom = rec;
                }
                tup = new Tuple<Point3d, Point3d>(recCur.BottomLine.StartPoint, recCur.BottomLine.EndPoint);
                if (dicPtsToRect.ContainsKey(tup))
                {
                    var rec = dicPtsToRect[tup];
                    rec.Bottom = recCur;
                    recCur.Top = rec;
                }
            }
        }
    }

    public class TupleComparer : IEqualityComparer<Tuple<Point3d, Point3d>>
    {
        public bool Equals(Tuple<Point3d, Point3d> x, Tuple<Point3d, Point3d> y)
        {
            double t = 1e-6;
            Tolerance tol = new Tolerance(t, t);
            return x.Item1.IsEqualTo(y.Item1, tol) && x.Item2.IsEqualTo(y.Item2, tol);
        }

        public int GetHashCode(Tuple<Point3d, Point3d> obj)
        {
            string str = getString(obj);
            return str.GetHashCode();
        }

        private static string getString(Tuple<Point3d, Point3d> obj)
        {
            var dblArray = new List<double>();
            dblArray.AddRange(obj.Item1.ToArray());
            dblArray.AddRange(obj.Item2.ToArray());
            var lst = dblArray.Select(o => o.ToString("0.000000"));
            var str = string.Join(",", lst);
            return str;
        }
    }

    public class PointComparer : IEqualityComparer<Point3d>
    {
        public bool Equals(Point3d x, Point3d y)
        {
            double t = 1e-6;
            Tolerance tol = new Tolerance(t, t);
            return x.IsEqualTo(y, tol);
        }

        public int GetHashCode(Point3d obj)
        {
            string str = getString(obj);
            return str.GetHashCode();
        }

        private static string getString(Point3d obj)
        {
            var dblArray = new List<double>();
            dblArray.AddRange(obj.ToArray());
            var lst = dblArray.Select(o => o.ToString("0.000000"));
            var str = string.Join(",", lst);
            return str;
        }
    }

    public class RectangleInfo
    {
        public LineSegment3d LeftLine { get; private set; }

        public LineSegment3d RightLine { get; private set; }

        public LineSegment3d TopLine { get; private set; }

        public LineSegment3d BottomLine { get; private set; }

        public Extents3d Extents { get; private set; }
        public RectangleInfo Left { get; internal set; }
        public RectangleInfo Right { get; internal set; }
        public RectangleInfo Top { get; internal set; }
        public RectangleInfo Bottom { get; internal set; }

        public int NeighbourCount
        {
            get
            {
                var neighbours = new RectangleInfo[] { Left, Right, Top.Bottom };
                return neighbours.Where(o => o != null).Count();
            }
        }

        public RectangleInfo(Extents3d ext)
        {
            Extents = ext;
            var ptMin = Extents.MinPoint;
            var ptMax = Extents.MaxPoint;
            LeftLine = new LineSegment3d(ptMin, new Point3d(ptMin.X, ptMax.Y, 0));
            RightLine = new LineSegment3d(ptMax, new Point3d(ptMax.X, ptMin.Y, 0));
            TopLine = new LineSegment3d(new Point3d(ptMin.X, ptMax.Y, 0),ptMax);
            BottomLine = new LineSegment3d(new Point3d(ptMax.X, ptMin.Y, 0), ptMin);
        }
    }
}