using System;
using System.Collections.Generic;

namespace PanelLayout_App.Comparers
{
    public sealed class DoubleComparer : IEqualityComparer<double>
    {
        public bool Equals(double x, double y) =>
            Math.Abs(x - y) < ComparerSettings.EPSILON;

        public int GetHashCode(double obj)
        {
            string str = GetString(obj);
            return str.GetHashCode();
        }

        private static string GetString(double o) =>
            o.ToString(ComparerSettings.DOUBLE_FORMAT);
    }
}
