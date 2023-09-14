using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using PanelLayout_App.Licensing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace PanelLayout_App.ElevationModule
{
    internal abstract class ElevationBase
    {
        protected static Point3d BaseElevationPoint;

        protected static double WallHeight = 2 * PanelLayout_UI.maxHeightOfPanel;

        protected static Dictionary<string, Tuple<Line, double, int, string>> WallLines;

        protected Point3d GetTextPoint(Point3d startPoint, double dW, double dH) =>
            startPoint + new Vector3d(dW / 2d, dH / 2d, 0);

        protected bool WallLinesContainName(string name) =>
            !string.IsNullOrEmpty(name) && WallLines.ContainsKey(name);

        //Added For draw the elevation panels twice when the door/window is INDOOR by SDM 2022-07-07
        protected static bool CheckWallIsIndoor(string wallLineName, double maxOffset, ObjectId panelLineId, out KeyValuePair<string, Tuple<Line, double, int, string>> backWall, Point3d cornerBasePt,out double offset, bool insct_flag = false)
        {
            bool isInDoor = false;
            var rw = Convert.ToInt64(wallLineName.ToSplit('_', 2));
            backWall = new KeyValuePair<string, Tuple<Line, double, int, string>>();
            offset = 0;//Added to fix an issue when two doors share the same corner by SDM 2022-08-25
            foreach (var wall in WallLines)
            {
                var _rw=Convert.ToInt64(wall.Key.ToSplit('_',2));
                if (!wall.Key.Contains("_R_") || wall.Value.Item4 == wallLineName||_rw==rw )
                    continue;
                var wallline = wall.Value.Item1;
                double ClosestPtDist = 0;
                double Offset = 0;
                bool isParalllel = AEE_Utility.IsLinesAreParallel(WallLines[wallLineName].Item1, wallline, out Offset, out ClosestPtDist);

                if (Math.Round(Math.Abs(Offset)) != 0 && Math.Round(Math.Abs(Offset)) <= maxOffset && isParalllel && ClosestPtDist <= maxOffset)
                {
                    //Added to fix case when wall has more than one wall by SDM 2022-08-15
                    //start
                    var obj = panelLineId.Open(OpenMode.ForRead);
                    var cmnmodule = new CommonModule();
                    bool flag_X_Axis = false;
                    bool flag_Y_Axis = false;
                    cmnmodule.checkAngleOfLine_Axis(AEE_Utility.GetLineAngle(wallline), out flag_X_Axis, out flag_Y_Axis);
                    if (obj is Line)
                    {
                        var panelline = AEE_Utility.GetLine(panelLineId);
                        if (insct_flag)
                        {
                            var pts = AEE_Utility.InterSectionBetweenTwoEntity(panelLineId, wallline.ObjectId);
                            var p = AEE_Utility.FindIntersection(wallline, panelline);
                             if (pts.Count == 0 && p ==null)
                                continue;
                        }
                        else
                        {
                            if (flag_X_Axis && !((panelline.StartPoint.X >= wallline.StartPoint.X && panelline.StartPoint.X <= wallline.EndPoint.X) ||
                                    (panelline.StartPoint.X <= wallline.StartPoint.X && panelline.StartPoint.X >= wallline.EndPoint.X)))
                                continue;
                            else if (flag_Y_Axis && !((panelline.StartPoint.Y >= wallline.StartPoint.Y && panelline.StartPoint.Y <= wallline.EndPoint.Y) ||
                                    (panelline.StartPoint.Y <= wallline.StartPoint.Y && panelline.StartPoint.Y >= wallline.EndPoint.Y)))
                                continue;
                        }
                    }
                    else
                    {
                        if (flag_X_Axis && !((Math.Round(cornerBasePt.X) >= Math.Round(wallline.StartPoint.X) && Math.Round(cornerBasePt.X) <= Math.Round(wallline.EndPoint.X)) ||
                                  (Math.Round(cornerBasePt.X) <= Math.Round(wallline.StartPoint.X) && Math.Round(cornerBasePt.X) >= Math.Round(wallline.EndPoint.X))))
                            continue;
                        else if (flag_Y_Axis && !((Math.Round(cornerBasePt.Y) >= Math.Round(wallline.StartPoint.Y) && Math.Round(cornerBasePt.Y) <= Math.Round(wallline.EndPoint.Y)) ||
                                (Math.Round(cornerBasePt.Y) <= Math.Round(wallline.StartPoint.Y) && Math.Round(cornerBasePt.Y) >= Math.Round(wallline.EndPoint.Y))))
                            continue;
                    }
                    //end
                    isInDoor = true;
                    backWall = wall;
                    offset =Math.Abs(Offset);//Added to fix an issue when two doors share the same corner by SDM 2022-08-25
                }

            }
            ////Fix case when indoor wall has more than one backwall by SDM 2022-07-21
            //if (isInDoor && backWalls.Count > 1 && panelLineId.IsValid)
            //{
            //    List<double> lens = new List<double>();
            //    var obj = panelLineId.Open(OpenMode.ForRead);
            //    var panelline = AEE_Utility.GetLine(panelLineId);
            //    foreach (var wall in backWalls)
            //    {
            //        if (obj is Line)
            //        {
            //            double distance1 = Math.Min(panelline.StartPoint.DistanceTo(wall.Value.Item1.StartPoint), panelline.StartPoint.DistanceTo((wall.Value.Item1.EndPoint)));
            //            double distance2 = Math.Min(panelline.EndPoint.DistanceTo(wall.Value.Item1.StartPoint), panelline.EndPoint.DistanceTo(wall.Value.Item1.EndPoint));
            //            lens.Add(Math.Min(distance1, distance2));
            //        }
            //        else
            //        {
            //            double distance1 = Math.Min(cornerBasePt.DistanceTo(wall.Value.Item1.StartPoint), cornerBasePt.DistanceTo((wall.Value.Item1.EndPoint)));
            //            lens.Add(distance1);
            //        }
            //    }
            //    double minDist = lens.Min();
            //    backWall = backWalls[lens.IndexOf(minDist)];
            //}
            //else if (isInDoor && backWalls.Count == 1)
            //    backWall = backWalls[0];
            return isInDoor;
        }
    }
}
