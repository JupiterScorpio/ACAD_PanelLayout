using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace PanelLayout_App.Comparers
{
    public sealed class PointComparer : IEqualityComparer<Point3d>
    {
        public bool Equals(Point3d x, Point3d y) =>
            x.IsEqualTo(y, ComparerSettings.TOLERANCE);

        public int GetHashCode(Point3d obj)
        {
            string str = GetString(obj);
            return str.GetHashCode();
        }

        private static string GetString(Point3d obj)
        {
            var dblArray = new List<double>();
            dblArray.AddRange(obj.ToArray());
            var lst = dblArray.Select(o => o.ToString(ComparerSettings.DOUBLE_FORMAT));
            var str = string.Join(",", lst);
            return str;
        }
    }
}
