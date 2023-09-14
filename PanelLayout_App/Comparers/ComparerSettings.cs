using Autodesk.AutoCAD.Geometry;

namespace PanelLayout_App.Comparers
{
    internal sealed class ComparerSettings
    {
        internal const string DOUBLE_FORMAT = "0.000000";

        internal const double EPSILON = 1e-06;

        internal static readonly Tolerance TOLERANCE = new Tolerance(EPSILON, EPSILON);
    }
}
