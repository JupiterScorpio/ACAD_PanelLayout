using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using PanelLayout_App.ElevationModule;
using PanelLayout_App.WallModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanelLayout_App
{

    internal sealed class HoleCenterlineCreation : ElevationBase
    {
        private void DECK_inecreation(Point3d startpnt, Point3d endpnt, double angle, double value, double pitch)
        {

            bool flag_X_Axis = false;
            bool flag_Y_Axis = false;
            CommonModule commonModule = new CommonModule();
            commonModule.checkAngleOfLine_Axis(angle, out flag_X_Axis, out flag_Y_Axis);


            if (flag_X_Axis)
            {




                double linecount = (endpnt.X - startpnt.X) / pitch;
                if (value == 3)
                {
                    linecount = (startpnt.X - endpnt.X) / pitch;
                }
                int max_line = Convert.ToInt32(Math.Floor(linecount));
                double remian_dist = (endpnt.X - startpnt.X) - ((pitch * max_line));

                if (remian_dist < pitch)
                {
                    max_line--;
                }

                for (int i = 0; i < max_line; i++)
                {

                    double[] stpnts = new double[3];
                    stpnts = startpnt.ToArray();
                    Point3d stpnt = new Point3d((stpnts[0] + (pitch * (i + 1))), (stpnts[1]), (stpnts[2]));


                    double[] edpnts = new double[3];
                    Point3d endpoint = new Point3d();

                    if (value == 1)
                    {
                        endpoint = new Point3d((stpnts[0] + (pitch * (i + 1))), (stpnts[1] + pitch), (stpnts[2]));
                    }
                    else
                    {
                        stpnts = endpnt.ToArray();
                        stpnt = new Point3d((stpnts[0] + (pitch * (i + 1))), (stpnts[1]), (stpnts[2]));
                        endpoint = new Point3d((stpnts[0] + (pitch * (i + 1))), (stpnts[1] - pitch), (stpnts[2]));
                    }




                    AEE_Utility.CreateLine(stpnt, endpoint, CommonModule.deckPanelWallLayerName, CommonModule.eleWallPanelLyrColor);
                }

            }
            else
            {




                double linecount = (endpnt.Y - startpnt.Y) / pitch;
                if (value == 4)
                {
                    linecount = (startpnt.Y - endpnt.Y) / pitch;
                }
                int max_line = Convert.ToInt32(Math.Floor(linecount));
                double remian_dist = (endpnt.Y - startpnt.Y) - ((pitch * max_line));
                if (value == 4)
                {
                    remian_dist = (startpnt.Y - endpnt.Y) - ((pitch * max_line));
                }

                if (remian_dist < pitch)
                {
                    max_line--;
                }

                for (int i = 0; i < max_line; i++)
                {

                    double[] stpnts = new double[3];
                    stpnts = startpnt.ToArray();
                    Point3d stpnt = new Point3d((stpnts[0]), (stpnts[1] + (pitch * (i + 1))), (stpnts[2]));

                    double[] edpnts = new double[3];
                    Point3d endpoint = new Point3d();

                    if (value == 2)
                    {
                        endpoint = new Point3d((stpnts[0] - pitch), (stpnts[1] + (pitch * (i + 1))), (stpnts[2]));
                    }
                    else
                    {
                        stpnts = endpnt.ToArray();
                        stpnt = new Point3d((stpnts[0]), (stpnts[1] + (pitch * (i + 1))), (stpnts[2]));
                        endpoint = new Point3d((stpnts[0] + pitch), (stpnts[1] + (pitch * (i + 1))), (stpnts[2]));
                    }




                    AEE_Utility.CreateLine(stpnt, endpoint, CommonModule.deckPanelWallLayerName, CommonModule.eleWallPanelLyrColor);
                }

            }
        }

        private void DECK_inecreation(Point3d startpnt, Point3d endpnt, double angle, double value, double pitch, bool val)
        {

            bool flag_X_Axis = false;
            bool flag_Y_Axis = false;
            CommonModule commonModule = new CommonModule();
            commonModule.checkAngleOfLine_Axis(angle, out flag_X_Axis, out flag_Y_Axis);


            if (flag_X_Axis)
            {




                double linecount = (endpnt.X - startpnt.X) / pitch;
                if (value == 3 || value == 6)
                {
                    linecount = (startpnt.X - endpnt.X) / pitch;
                }
                int max_line = Convert.ToInt32(Math.Floor(linecount));
                double remian_dist = (endpnt.X - startpnt.X) - ((pitch * max_line));

                if (remian_dist < pitch)
                {
                    max_line--;
                }

                for (int i = 0; i < max_line; i++)
                {

                    double[] stpnts = new double[3];
                    stpnts = startpnt.ToArray();
                    double yvalue = 0;
                    double endyvalue = 0;
                    if (val)
                    {
                        yvalue = (stpnts[1]) + 25;
                        endyvalue = (stpnts[1] - 50);
                        if (value == 3 || value == 5)
                        {
                            yvalue = (stpnts[1]) - 25;
                            endyvalue = (stpnts[1] + 50);

                        }

                    }
                    else
                    {
                        endyvalue = (stpnts[1] - pitch);
                        yvalue = (stpnts[1]);
                    }
                    Point3d stpnt = new Point3d((stpnts[0] + (pitch * (i + 1))), yvalue, (stpnts[2]));


                    double[] edpnts = new double[3];
                    Point3d endpoint = new Point3d();

                    if (value == 1)
                    {
                        endpoint = new Point3d((stpnts[0] + (pitch * (i + 1))), endyvalue, (stpnts[2]));
                    }
                    else
                    {
                        stpnts = endpnt.ToArray();

                        yvalue = 0;
                        if (val)
                        {
                            yvalue = (stpnts[1]) + 25;
                            endyvalue = (stpnts[1] - 50);
                            if (value == 3 || value == 5)
                            {
                                yvalue = (stpnts[1]) - 25;
                                endyvalue = (stpnts[1] + 50);

                            }

                        }
                        else
                        {
                            endyvalue = (stpnts[1] + pitch);
                            if (value == 6)
                            {
                                endyvalue = (stpnts[1] - pitch);
                            }
                            yvalue = (stpnts[1]);
                        }
                        stpnt = new Point3d((stpnts[0] + (pitch * (i + 1))), yvalue, (stpnts[2]));
                        endpoint = new Point3d((stpnts[0] + (pitch * (i + 1))), endyvalue, (stpnts[2]));

                        if (value == 5)
                        {
                            stpnt = new Point3d((startpnt[0] + (pitch * (i + 1))), yvalue, (startpnt[2]));
                            endpoint = new Point3d((startpnt[0] + (pitch * (i + 1))), endyvalue, (startpnt[2]));
                        }
                        else if (value == 6)
                        {

                            endpoint = new Point3d((stpnts[0] + (pitch * (i + 1))), endyvalue, (stpnts[2]));
                        }

                    }


                    if (pitch == 100)
                    {
                        //bool isbetween = false;
                        //for (int n = 0; n < SJpnts.Count; n++)
                        //{
                        //    if (SJpnts[n] < stpnt.X && (((SJpnts[n] + 50) > stpnt.X)) || (SJpnts[n] > stpnt.X && ((SJpnts[n] - 50) < stpnt.X)))
                        //    {
                        //        isbetween = true;
                        //    }
                        //}

                        //if (isbetween == false)
                        //{

                        AEE_Utility.CreateLine(stpnt, endpoint, CommonModule.clineWallPanelLyrName_sj, CommonModule.eleWallPanelLyrColor);
                        // }
                    }
                    else
                    {

                        AEE_Utility.CreateLine(stpnt, endpoint, CommonModule.clineWallPanelLyrName_ver, CommonModule.eleWallPanelLyrColor);
                    }
                }

            }
            else
            {




                double linecount = (endpnt.Y - startpnt.Y) / pitch;
                if (linecount < 0)
                {
                    linecount = (startpnt.Y - endpnt.Y) / pitch;
                }

                int max_line = Convert.ToInt32(Math.Floor(linecount));
                double remian_dist = (endpnt.Y - startpnt.Y) - ((pitch * max_line));

                remian_dist = (startpnt.Y - endpnt.Y) - ((pitch * max_line));



                if (remian_dist < pitch)
                {
                    max_line--;
                }

                for (int i = 0; i < max_line; i++)
                {

                    double[] stpnts = new double[3];
                    stpnts = startpnt.ToArray();
                    double xvalue = 0;
                    double endxvalue = 0;
                    if (val)
                    {
                        xvalue = (stpnts[0]) + 25;
                        endxvalue = (stpnts[0] - 50);
                        if (value == 3 || value == 7 || value == 2)
                        {
                            xvalue = (stpnts[0]) - 25;
                            endxvalue = (stpnts[0] + 50);
                        }
                    }
                    else
                    {
                        endxvalue = (stpnts[0] + pitch);
                        if (value == 4)
                        {
                            endxvalue = (stpnts[0] - pitch);

                        }
                        xvalue = (stpnts[0]);
                    }
                    Point3d stpnt = new Point3d(xvalue, (stpnts[1] + (pitch * (i + 1))), (stpnts[2]));

                    double[] edpnts = new double[3];
                    Point3d endpoint = new Point3d();

                    if (value == 7)
                    {

                        stpnt = new Point3d(xvalue, (stpnts[1] - (pitch * (i + 1))), (stpnts[2]));
                        endpoint = new Point3d(endxvalue, (stpnts[1] - (pitch * (i + 1))), (stpnts[2]));
                    }
                    else if (value == 4)
                    {
                        endpoint = new Point3d(endxvalue, (stpnts[1] + (pitch * (i + 1))), (stpnts[2]));
                    }
                    else
                    {

                        endpoint = new Point3d(endxvalue, (stpnts[1] + (pitch * (i + 1))), (stpnts[2]));
                    }

                    if (pitch == 100)
                    {
                        //bool isbetween = false;
                        //for (int n = 0; n < SJpnts.Count; n++)
                        //{
                        //    if (SJpnts[n] < stpnt.Y && (((SJpnts[n] + 50) > stpnt.Y)) || (SJpnts[n] > stpnt.Y && ((SJpnts[n] - 50) < stpnt.Y)))
                        //    {
                        //        isbetween = true;
                        //    }
                        //}

                        //if (isbetween == false)
                        //{

                        AEE_Utility.CreateLine(stpnt, endpoint, CommonModule.clineWallPanelLyrName_sj, CommonModule.eleWallPanelLyrColor);
                        //}
                    }
                    else
                    {


                        AEE_Utility.CreateLine(stpnt, endpoint, CommonModule.clineWallPanelLyrName_ver, CommonModule.eleWallPanelLyrColor);
                    }
                }

            }
        }

        private void DECK_inecreation(Point3d startpnt, Point3d endpnt, double angle, double value, double pitch, bool val, string xDataRegAppName)
        {

            bool flag_X_Axis = false;
            bool flag_Y_Axis = false;
            CommonModule commonModule = new CommonModule();
            commonModule.checkAngleOfLine_Axis(angle, out flag_X_Axis, out flag_Y_Axis);


            if (flag_X_Axis)
            {


                double linecount = (endpnt.X - startpnt.X) / pitch;
                if (value == 3 || value == 6)
                {
                    linecount = (startpnt.X - endpnt.X) / pitch;
                }
                int max_line = Convert.ToInt32(Math.Floor(linecount));
                double remian_dist = (endpnt.X - startpnt.X) - ((pitch * max_line));
                if (value == 66)
                {
                    linecount = (startpnt.X - endpnt.X) / pitch;
                    max_line = Convert.ToInt32(Math.Floor(linecount));
                    remian_dist = (endpnt.X - startpnt.X) - ((pitch * max_line));
                }
                if (remian_dist < pitch)
                {
                    max_line--;
                }

                for (int i = 0; i < max_line; i++)
                {

                    double[] stpnts = new double[3];
                    stpnts = startpnt.ToArray();
                    double yvalue = 0;
                    double endyvalue = 0;
                    if (val)
                    {
                        yvalue = (stpnts[1]);
                        endyvalue = (stpnts[1] - 50);
                        if (value == 3 || value == 5 || value == 11)
                        {
                            yvalue = (stpnts[1]);
                            endyvalue = (stpnts[1] + 50);

                        }

                    }
                    else
                    {
                        endyvalue = (stpnts[1] - 50);
                        if (value == 11)
                        {
                            endyvalue = (stpnts[1] + 50);
                        }
                        yvalue = (stpnts[1]);
                    }
                    Point3d stpnt = new Point3d((stpnts[0] + (pitch * (i + 1))), yvalue, (stpnts[2]));


                    double[] edpnts = new double[3];
                    Point3d endpoint = new Point3d();

                    if (value == 1 || value == 11)
                    {
                        endpoint = new Point3d((stpnts[0] + (pitch * (i + 1))), endyvalue, (stpnts[2]));
                    }
                    else
                    {
                        stpnts = endpnt.ToArray();

                        yvalue = 0;
                        if (val)
                        {
                            yvalue = (stpnts[1]);
                            endyvalue = (stpnts[1] - 50);
                            if (value == 3 || value == 5 || value == 11)
                            {
                                yvalue = (stpnts[1]);
                                endyvalue = (stpnts[1] + 50);

                            }

                        }
                        else
                        {
                            endyvalue = (stpnts[1] + 50);
                            if (value == 6 || value == 3 || value == 5)
                            {
                                endyvalue = (stpnts[1] - 50);
                            }
                            yvalue = (stpnts[1]);
                        }
                        stpnt = new Point3d((stpnts[0] + (pitch * (i + 1))), yvalue, (stpnts[2]));
                        endpoint = new Point3d((stpnts[0] + (pitch * (i + 1))), endyvalue, (stpnts[2]));

                        if (value == 5)
                        {
                            stpnt = new Point3d((startpnt[0] + (pitch * (i + 1))), yvalue, (startpnt[2]));
                            endpoint = new Point3d((startpnt[0] + (pitch * (i + 1))), endyvalue, (startpnt[2]));
                        }
                        else if (value == 6)
                        {

                            endpoint = new Point3d((stpnts[0] + (pitch * (i + 1))), endyvalue, (stpnts[2]));
                        }

                    }





                    if (pitch == 50)
                    {
                        //bool isbetween = false;
                        //for (int n = 0; n < kickerpnts.Count; n++)
                        //{
                        //    if (kickerpnts[n] < stpnt.X && (((kickerpnts[n] + 50) > stpnt.X)) || (kickerpnts[n] > stpnt.X && ((kickerpnts[n] - 50) < stpnt.X)))
                        //    {
                        //        isbetween = true;
                        //    }
                        //}

                        //if (isbetween == false)
                        //{

                        ObjectId id = AEE_Utility.CreateLine(stpnt, endpoint, CommonModule.clineWallPanelLyrName_ver, CommonModule.eleWallPanelLyrColor);
                        //}
                    }
                    else
                    {
                        ObjectId id = AEE_Utility.CreateLine(stpnt, endpoint, CommonModule.kickerWallPanelLayerName, CommonModule.eleWallPanelLyrColor);
                    }
                }

            }
            else
            {




                double linecount = (endpnt.Y - startpnt.Y) / pitch;
                if (linecount < 0)
                {
                    linecount = (startpnt.Y - endpnt.Y) / pitch;
                }

                int max_line = Convert.ToInt32(Math.Floor(linecount));
                double remian_dist = (endpnt.Y - startpnt.Y) - ((pitch * max_line));

                remian_dist = (startpnt.Y - endpnt.Y) - ((pitch * max_line));



                if (remian_dist < pitch)
                {
                    max_line--;
                }

                for (int i = 0; i < max_line; i++)
                {

                    double[] stpnts = new double[3];
                    stpnts = startpnt.ToArray();
                    double xvalue = 0;
                    double endxvalue = 0;
                    if (val)
                    {
                        xvalue = (stpnts[0]);
                        endxvalue = (stpnts[0] - 50);
                        if (value == 3 || value == 7 || value == 2 || value == 44)
                        {
                            xvalue = (stpnts[0]);
                            endxvalue = (stpnts[0] + 50);
                        }
                    }
                    else
                    {
                        endxvalue = (stpnts[0] + 50);
                        if (value == 4 || value == 7 || value == 2)
                        {
                            endxvalue = (stpnts[0] - 50);

                        }
                        xvalue = (stpnts[0]);
                    }
                    Point3d stpnt = new Point3d(xvalue, (stpnts[1] + (pitch * (i + 1))), (stpnts[2]));

                    double[] edpnts = new double[3];
                    Point3d endpoint = new Point3d();

                    if (value == 7)
                    {

                        stpnt = new Point3d(xvalue, (stpnts[1] - (pitch * (i + 1))), (stpnts[2]));
                        endpoint = new Point3d(endxvalue, (stpnts[1] - (pitch * (i + 1))), (stpnts[2]));
                    }
                    else if (value == 4)
                    {
                        endpoint = new Point3d(endxvalue, (stpnts[1] + (pitch * (i + 1))), (stpnts[2]));
                    }
                    else
                    {

                        endpoint = new Point3d(endxvalue, (stpnts[1] + (pitch * (i + 1))), (stpnts[2]));
                    }





                    if (pitch == 50)
                    {
                        //bool isbetween = false;
                        //for (int n = 0; n < kickerpnts.Count; n++)
                        //{
                        //    if (kickerpnts[n] < stpnt.Y && (((kickerpnts[n] + 50) > stpnt.Y)) || (kickerpnts[n] > stpnt.Y && ((kickerpnts[n] - 50) < stpnt.Y)))
                        //    {
                        //        isbetween = true;
                        //    }
                        //}

                        //if (isbetween == false)
                        //{

                        ObjectId id = AEE_Utility.CreateLine(stpnt, endpoint, CommonModule.clineWallPanelLyrName_ver, CommonModule.eleWallPanelLyrColor);
                        //}
                    }
                    else
                    {
                        ObjectId id = AEE_Utility.CreateLine(stpnt, endpoint, CommonModule.kickerWallPanelLayerName, CommonModule.eleWallPanelLyrColor);
                    }
                }

            }
        }
        private void deckpanel_CLine_Creation()
        {
            for (int i = 0; i < CommonModule.Deck_panel_CLine.Count; i++)
            {

                Point3d strtPoint1 = CommonModule.Deck_panel_CLine[i].startpoint1;
                Point3d strtPoint2 = CommonModule.Deck_panel_CLine[i].startpoint2;
                Point3d endPoint1 = CommonModule.Deck_panel_CLine[i].endpoint1;
                Point3d endPoint2 = CommonModule.Deck_panel_CLine[i].endpoint2;

                if (CommonModule.Deck_panel_CLine[i].pitch == 100)
                {
                    if (CommonModule.Deck_panel_CLine[i].angle == 0)
                    {

                        DECK_inecreation(strtPoint1, endPoint1, 0, 1, 150);
                        DECK_inecreation(endPoint1, endPoint2, 90, 2, 100);
                        DECK_inecreation(endPoint2, strtPoint2, 0, 3, 150);
                        DECK_inecreation(strtPoint2, strtPoint1, 90, 4, 100);
                    }
                    else if (CommonModule.Deck_panel_CLine[i].angle == 90)
                    {

                        DECK_inecreation(strtPoint1, strtPoint2, 0, 1, 100);
                        DECK_inecreation(strtPoint2, endPoint2, 90, 2, 150);
                        DECK_inecreation(endPoint2, endPoint1, 0, 3, 100);
                        DECK_inecreation(endPoint1, strtPoint1, 90, 4, 150);
                    }
                }
                else if (CommonModule.Deck_panel_CLine[i].pitch == 50)
                {
                    DECK_inecreation(strtPoint1, endPoint1, 0, 1, 50);
                    DECK_inecreation(endPoint1, endPoint2, 90, 2, 50);
                    DECK_inecreation(endPoint2, strtPoint2, 0, 3, 50);
                    DECK_inecreation(strtPoint2, strtPoint1, 90, 4, 50);
                }
            }
        }

        private void slpjoint_cline_creation()
        {
            List<string> lstpnts = new List<string>();
            for (int cline = 0; cline < CommonModule.slapjoint_CLine.Count; cline++)
            {
                string xDataRegAppName = CommonModule.slapjoint_CLine[cline].wallname;
                bool flag_X_Axis = CommonModule.slapjoint_CLine[cline].x_axis;
                double length = CommonModule.slapjoint_CLine[cline].length;
                Point3d offsetStartPoint = CommonModule.slapjoint_CLine[cline].offsetpoint;
                Point3d startPoint = CommonModule.slapjoint_CLine[cline].startpoint;
                Vector3d moveVector = CommonModule.slapjoint_CLine[cline].movevector;
                List<double> listOfWallPanelLength = CommonModule.slapjoint_CLine[cline].listOfWallPanelLength;
                double wallPanelLineAngle = CommonModule.slapjoint_CLine[cline].wallPanelLineAngle;

                string txt = CommonModule.textvalue.Where(x => x.StartsWith(xDataRegAppName)).Select(y => y).First();
                if (txt != null)
                {
                    if (flag_X_Axis)
                    {
                        double y_value = Convert.ToDouble(txt.Split('|')[1].Split(',')[1]);

                        double pitch = 50;

                        double linecount = length / pitch;
                        int max_line = Convert.ToInt32(Math.Floor(linecount));
                        double remian_dist = length - ((pitch * max_line));

                        if (remian_dist < pitch)
                        {
                            max_line--;
                        }

                        int count = 0;
                        List<string> pnts = new List<string>();

                        for (int i = 0; i < max_line; i++)
                        {

                            double[] stpnts = new double[3];
                            stpnts = offsetStartPoint.ToArray();
                            Point3d stpnt = new Point3d((stpnts[0] + (pitch * (i + 1))), (stpnts[1]), (stpnts[2]));

                            double[] edpnts = new double[3];
                            Point3d endpnt = new Point3d();
                            bool isleft = false;
                            if (y_value > offsetStartPoint.Y)
                            {
                                if (offsetStartPoint.Y < startPoint.Y)
                                {
                                    stpnts = startPoint.ToArray();
                                    stpnt = new Point3d((stpnts[0] + (pitch * (i + 1))), (stpnts[1]), (stpnts[2]));
                                }
                                endpnt = new Point3d((stpnts[0] + (pitch * (i + 1))), (stpnts[1] - pitch), (stpnts[2]));
                            }
                            else
                            {
                                endpnt = new Point3d((stpnts[0] + (pitch * (i + 1))), (stpnts[1] + pitch), (stpnts[2]));
                            }


                            // AEE_Utility.CreateLine(stpnt, endpnt, CommonModule.clineWallPanelLyrName_sj, CommonModule.eleWallPanelLyrColor);

                            if (count == 0)
                            {
                                List<WP_Cline_data> wallpnaelpnt = CommonModule.wallpanel_pnts.Where(x => x.panelname.ToLower().Contains(xDataRegAppName.ToLower())).Select(y => y).ToList();
                                bool isccreated = false;

                                if (wallpnaelpnt != null && wallpnaelpnt.Count > 0)
                                {
                                    if (y_value < wallpnaelpnt[0].startpoint.Y)
                                    {
                                        wallpnaelpnt.Reverse();
                                    }

                                    bool exit = false;
                                    for (int j = 0; j < wallpnaelpnt.Count; j++)
                                    {
                                        if (wallpnaelpnt[j].panelname.Contains("_EX_") || (wallpnaelpnt[j].xaxis != true))
                                        {
                                            continue;
                                        }

                                        double pitch1 = 0;
                                        bool inbetval = false;
                                        double dW1 = wallpnaelpnt[j].startpoint.X - wallpnaelpnt[j].endpoint.X;

                                        if (dW1 < 0)
                                        {
                                            dW1 = wallpnaelpnt[j].endpoint.X - wallpnaelpnt[j].startpoint.X;
                                        }


                                        if (wallpnaelpnt[j].startpoint.X < stpnt.X && wallpnaelpnt[j].endpoint.X > stpnt.X)
                                        {
                                            inbetval = true;
                                        }

                                        //double x = wallpnaelpnt[j].startpoint.X + 60 + moveVector.X;
                                        //if (x != stpnt.X)
                                        //{
                                        //    continue;
                                        //}


                                        if (dW1 >= 250)
                                        {
                                            pitch1 = PanelLayout_UI.ver_max_pitch;
                                        }
                                        else if (dW1 < 250)
                                        {
                                            pitch1 = PanelLayout_UI.ver_min_pitch;
                                        }
                                        double linecount1 = dW1 / pitch1;
                                        double max_line1 = Convert.ToInt32(Math.Floor(linecount1));
                                        if (max_line1 < 0)
                                        {
                                            max_line1 = Math.Abs(max_line1);

                                            if (wallpnaelpnt[j].endpoint.X < stpnt.X && wallpnaelpnt[j].startpoint.X > stpnt.X)
                                            {
                                                inbetval = true;
                                            }

                                        }

                                        //if (inbetval == false)
                                        //{
                                        //    continue;
                                        //}
                                        double remian_dist1 = dW1 - ((pitch1 * max_line1));
                                        double[] stpnts1 = new double[3];
                                        stpnts1 = wallpnaelpnt[j].startpoint.ToArray();
                                        if (remian_dist1 < 50)
                                        {
                                            max_line1--;
                                            if (dW1 == 75)
                                            {

                                                //stpnts1[0] = stpnts[0];
                                                stpnts1[0] = stpnts1[0] + moveVector.X;

                                                Point3d stpnt1 = new Point3d();
                                                Point3d endpnt1 = new Point3d();
                                                if (y_value > offsetStartPoint.Y)
                                                {
                                                    stpnt1 = new Point3d((stpnts1[0] + (dW1 / 2)), (stpnt.Y), stpnt.Z);
                                                    endpnt1 = new Point3d((stpnts1[0] + (dW1 / 2)), (endpnt.Y - 50), endpnt.Z);
                                                }
                                                else
                                                {


                                                    stpnt1 = new Point3d((stpnts1[0] + (dW1 / 2)), (stpnt.Y), stpnt.Z);
                                                    endpnt1 = new Point3d((stpnts1[0] + (dW1 / 2)), (endpnt.Y + 50), endpnt.Z);
                                                }
                                                AEE_Utility.CreateLine(stpnt1, endpnt1, CommonModule.clineWallPanelLyrName_ver, CommonModule.eleWallPanelLyrColor);
                                            }
                                        }



                                        for (int l = 0; l < max_line1; l++)
                                        {

                                            stpnts1 = wallpnaelpnt[j].startpoint.ToArray();
                                            if (y_value < wallpnaelpnt[j].startpoint.Y)
                                            {
                                                stpnts1 = wallpnaelpnt[j].endpoint.ToArray();
                                            }

                                            stpnts1[0] = stpnts1[0] + moveVector.X;



                                            Point3d stpnt1 = new Point3d((stpnts1[0] + (pitch1 * (l + 1))), (stpnts1[1]), (stpnts1[2]));

                                            if (y_value < wallpnaelpnt[j].startpoint.Y)
                                            {
                                                stpnt1 = new Point3d((stpnts1[0] - (pitch1 * (l + 1))), (stpnts1[1]), (stpnts1[2]));
                                            }


                                            double[] edpnts1 = new double[3];
                                            Point3d endpnt1 = new Point3d();
                                            if (y_value > wallpnaelpnt[j].startpoint.Y)
                                            {
                                                if (wallpnaelpnt[j].startpoint.Y > wallpnaelpnt[j].endpoint.Y)
                                                {
                                                    endpnt1 = new Point3d((stpnts1[0] + pitch1), (stpnts1[1] + (pitch1 * (l + 1))), (stpnts1[2]));
                                                }
                                                else
                                                {
                                                    endpnt1 = new Point3d((stpnts1[0] - pitch1), (stpnts1[1] + (pitch1 * (l + 1))), (stpnts1[2]));
                                                }
                                            }
                                            else
                                            {
                                                endpnt1 = new Point3d((stpnts1[0] - pitch1), (stpnts1[1] + (pitch1 * (l + 1))), (stpnts1[2]));
                                            }

                                            //if ((stpnt.Y < stpnt1.Y && ((stpnt.Y + 50) > stpnt1.Y)) || (stpnt.Y > stpnt1.Y && ((stpnt.Y - 50) < stpnt1.Y)))
                                            {

                                                double x_value = stpnt1.X;
                                                //if (xDataRegAppName.ToLower().Contains("w3_r_1"))
                                                //{
                                                //    if (lstpnts.Count > 0)
                                                //    {
                                                //        double pntx = Math.Abs(stpnt1.X - lstpnts[lstpnts.Count - 1]);

                                                //        if (pntx == 150 && l == 0)
                                                //        {
                                                //            x_value = lstpnts[lstpnts.Count - 1] + pitch1;
                                                //        }
                                                //    }
                                                //}


                                                Point3d stpnt_1 = new Point3d(x_value, stpnt.Y, stpnt.Z);
                                                Point3d endpnt_1 = new Point3d(x_value, endpnt.Y, endpnt.Z);

                                                if (!lstpnts.Contains(x_value + "|" + xDataRegAppName))
                                                {

                                                    //AEE_Utility.CreateLine(stpnt_1, endpnt_1, CommonModule.clineWallPanelLyrName_sj, CommonModule.eleWallPanelLyrColor);
                                                    // SJpnts.Add(stpnt_1.X);
                                                    Point3d tempstpnt = new Point3d(startPoint.X, startPoint.Y, startPoint.Z); ;
                                                    for (int len = 0; len < listOfWallPanelLength.Count; len++)
                                                    {
                                                        double wallPnlLngth_1 = listOfWallPanelLength[len];
                                                        var point_1 = AEE_Utility.get_XY(wallPanelLineAngle, wallPnlLngth_1);
                                                        Point3d endPoint_1 = new Point3d((tempstpnt.X + point_1.X), (tempstpnt.Y + point_1.Y), 0);


                                                        if ((tempstpnt.X < stpnt_1.X) && (endPoint_1.X > stpnt_1.X))
                                                        {
                                                            AEE_Utility.CreateLine(stpnt_1, endpnt_1, CommonModule.clineWallPanelLyrName_ver, CommonModule.eleWallPanelLyrColor);
                                                        }

                                                        tempstpnt = endPoint_1;
                                                    }

                                                }
                                                lstpnts.Add(x_value + "|" + xDataRegAppName);
                                                count++;
                                                exit = true;
                                            }


                                        }
                                    }
                                }
                            }

                            if (lstpnts.Count > 0)
                            {
                                bool isbetween = false;
                                for (int m = 0; m < lstpnts.Count; m++)
                                {
                                    double val = Convert.ToDouble(lstpnts[m].Split('|')[0]);
                                    if ((stpnt.X < val && ((stpnt.X + 100) > val)) || (stpnt.X > val && ((stpnt.X - 100) < val)))
                                    {
                                        isbetween = true;
                                    }
                                }

                                if (isbetween == false)
                                {
                                    SJpnts.Add(stpnt.X);
                                    AEE_Utility.CreateLine(stpnt, endpnt, CommonModule.clineWallPanelLyrName_sj, CommonModule.eleWallPanelLyrColor);
                                }
                            }
                            else
                            {
                                SJpnts.Add(stpnt.X);
                                AEE_Utility.CreateLine(stpnt, endpnt, CommonModule.clineWallPanelLyrName_sj, CommonModule.eleWallPanelLyrColor);
                            }
                        }








                        try
                        {
                            pitch = 0;
                            double dW = length;
                            if (dW >= 250)
                            {
                                pitch = PanelLayout_UI.ver_max_pitch;
                            }
                            else if (dW < 250)
                            {
                                pitch = PanelLayout_UI.ver_min_pitch;
                            }
                            linecount = dW / pitch;
                            max_line = Convert.ToInt32(Math.Floor(linecount));
                            remian_dist = dW - ((pitch * max_line));

                            if (remian_dist < 50)
                            {
                                max_line--;
                                if (dW == 75)
                                {
                                    double[] stpnts = new double[3];
                                    stpnts = startPoint.ToArray();

                                    Point3d stpnt = new Point3d();
                                    Point3d endpnt = new Point3d();

                                    if (y_value > offsetStartPoint.Y)
                                    {
                                        if (offsetStartPoint.Y < startPoint.Y)
                                        {
                                            stpnts = offsetStartPoint.ToArray();
                                            stpnt = new Point3d((stpnts[0] + (dW / 2)), (stpnts[1] - 50), (stpnts[2]));
                                            endpnt = new Point3d((stpnts[0] + (dW / 2)), (stpnts[1] + 25), (stpnts[2]));
                                        }
                                        else
                                        {
                                            stpnt = new Point3d((stpnts[0] + (dW / 2)), (stpnts[1] - 50), (stpnts[2]));
                                            endpnt = new Point3d((stpnts[0] + (dW / 2)), (stpnts[1] + 25), (stpnts[2]));
                                        }

                                    }
                                    else
                                    {
                                        stpnts = startPoint.ToArray();
                                        stpnt = new Point3d((stpnts[0] + (dW / 2)), (stpnts[1] + 50), (stpnts[2]));

                                        endpnt = new Point3d((stpnts[0] + (dW / 2)), (stpnts[1] - 25), (stpnts[2]));
                                    }
                                    AEE_Utility.CreateLine(stpnt, endpnt, CommonModule.clineWallPanelLyrName_sj, CommonModule.eleWallPanelLyrColor);
                                }
                            }

                            for (int i = 0; i < max_line; i++)
                            {

                                double[] stpnts = new double[3];
                                stpnts = startPoint.ToArray();

                                Point3d stpnt = new Point3d();
                                Point3d endpnt = new Point3d();

                                if (y_value > offsetStartPoint.Y)
                                {
                                    if (offsetStartPoint.Y < startPoint.Y)
                                    {
                                        stpnts = offsetStartPoint.ToArray();
                                        stpnt = new Point3d((stpnts[0] + (pitch * (i + 1))), (stpnts[1] - 50), (stpnts[2]));
                                        endpnt = new Point3d((stpnts[0] + (pitch * (i + 1))), (stpnts[1] + 25), (stpnts[2]));
                                    }
                                    else
                                    {
                                        stpnt = new Point3d((stpnts[0] + (pitch * (i + 1))), (stpnts[1] - 50), (stpnts[2]));
                                        endpnt = new Point3d((stpnts[0] + (pitch * (i + 1))), (stpnts[1] + 25), (stpnts[2]));
                                    }
                                }
                                else
                                {

                                    stpnts = startPoint.ToArray();
                                    stpnt = new Point3d((stpnts[0] + (pitch * (i + 1))), (stpnts[1] + 50), (stpnts[2]));
                                    endpnt = new Point3d((stpnts[0] + (pitch * (i + 1))), (stpnts[1] - 25), (stpnts[2]));
                                }
                                AEE_Utility.CreateLine(stpnt, endpnt, CommonModule.clineWallPanelLyrName_sj, CommonModule.eleWallPanelLyrColor);



                            }
                        }
                        catch { }

                    }
                    else
                    {
                        double y_value = Convert.ToDouble(txt.Split('|')[1].Split(',')[0]);

                        double pitch = 50;

                        double linecount = length / pitch;
                        int max_line = Convert.ToInt32(Math.Floor(linecount));
                        double remian_dist = length - ((pitch * max_line));

                        if (remian_dist < pitch)
                        {
                            max_line--;
                        }

                        List<string> pnts = new List<string>();


                        int count = 0;

                        for (int i = 0; i < max_line; i++)
                        {

                            double[] stpnts = new double[3];
                            stpnts = offsetStartPoint.ToArray();
                            Point3d stpnt = new Point3d((stpnts[0]), (stpnts[1] + (pitch * (i + 1))), (stpnts[2]));

                            double[] edpnts = new double[3];
                            Point3d endpnt = new Point3d();
                            bool isleft = false;
                            if (y_value > offsetStartPoint.X)
                            {
                                isleft = true;
                                endpnt = new Point3d((stpnts[0] - pitch), (stpnts[1] + (pitch * (i + 1))), (stpnts[2]));
                            }
                            else
                            {
                                endpnt = new Point3d((stpnts[0] + pitch), (stpnts[1] + (pitch * (i + 1))), (stpnts[2]));
                            }

                            //  AEE_Utility.CreateLine(stpnt, endpnt, CommonModule.clineWallPanelLyrName_sj, CommonModule.eleWallPanelLyrColor);
                            if (count == 0)
                            {
                                List<WP_Cline_data> wallpnaelpnt = CommonModule.wallpanel_pnts.Where(x => x.panelname.ToLower().Contains(xDataRegAppName.ToLower())).Select(y => y).ToList();
                                bool isccreated = false;

                                if (wallpnaelpnt != null && wallpnaelpnt.Count > 0)
                                {

                                    bool exit = false;
                                    for (int j = 0; j < wallpnaelpnt.Count; j++)
                                    {
                                        if (wallpnaelpnt[j].panelname.Contains("_EX_") || (wallpnaelpnt[j].xaxis == true))
                                        {
                                            continue;
                                        }

                                        double pitch1 = 0;
                                        bool inbetval = false;
                                        double dW1 = wallpnaelpnt[j].startpoint.Y - wallpnaelpnt[j].endpoint.Y;

                                        if (dW1 < 0)
                                        {
                                            dW1 = wallpnaelpnt[j].endpoint.Y - wallpnaelpnt[j].startpoint.Y;
                                        }


                                        if (wallpnaelpnt[j].startpoint.Y < stpnt.Y && wallpnaelpnt[j].endpoint.Y > stpnt.Y)
                                        {
                                            inbetval = true;
                                        }

                                        //double x = wallpnaelpnt[j].startpoint.X + 60 + moveVector.X;
                                        //if (x != stpnt.X)
                                        //{
                                        //    continue;
                                        //}


                                        if (dW1 >= 250)
                                        {
                                            pitch1 = PanelLayout_UI.ver_max_pitch;
                                        }
                                        else if (dW1 < 250)
                                        {
                                            pitch1 = PanelLayout_UI.ver_min_pitch;
                                        }
                                        double linecount1 = dW1 / pitch1;
                                        double max_line1 = Convert.ToInt32(Math.Floor(linecount1));
                                        if (max_line1 < 0)
                                        {
                                            max_line1 = Math.Abs(max_line1);

                                            if (wallpnaelpnt[j].endpoint.Y < stpnt.Y && wallpnaelpnt[j].startpoint.Y > stpnt.Y)
                                            {
                                                inbetval = true;
                                            }

                                        }

                                        //if (inbetval == false)
                                        //{
                                        //    continue;
                                        //}
                                        double remian_dist1 = dW1 - ((pitch1 * max_line1));
                                        double[] stpnts1 = new double[3];
                                        stpnts1 = wallpnaelpnt[j].startpoint.ToArray();
                                        if (remian_dist1 < 50)
                                        {
                                            max_line1--;
                                            if (dW1 == 75)
                                            {

                                                //stpnts1[0] = stpnts[0];


                                                Point3d stpnt1 = new Point3d();
                                                Point3d endpnt1 = new Point3d();
                                                if (y_value > offsetStartPoint.X)
                                                {
                                                    stpnt1 = new Point3d((stpnt.X), (stpnts1[1] + (dW1 / 2)), stpnt.Z);
                                                    endpnt1 = new Point3d((endpnt.X - 50), (stpnts1[1] + (dW1 / 2)), endpnt.Z);
                                                }
                                                else
                                                {
                                                    stpnt1 = new Point3d((stpnt.X), (stpnts1[1] + (dW1 / 2)), stpnt.Z);
                                                    endpnt1 = new Point3d((endpnt.X + 50), (stpnts1[1] + (dW1 / 2)), endpnt.Z);
                                                }
                                                AEE_Utility.CreateLine(stpnt1, endpnt1, CommonModule.clineWallPanelLyrName_ver, CommonModule.eleWallPanelLyrColor);
                                            }
                                        }



                                        for (int l = 0; l < max_line1; l++)
                                        {

                                            stpnts1 = wallpnaelpnt[j].endpoint.ToArray();
                                            if (isleft == true)
                                            {
                                                stpnts1 = wallpnaelpnt[j].startpoint.ToArray();
                                            }

                                            stpnts1[0] = stpnts1[0];// + moveVector.X;

                                            Point3d stpnt1 = new Point3d((stpnts1[0]), (stpnts1[1] - (pitch1 * (l + 1))), (stpnts1[2]));

                                            if (isleft == true)
                                            {
                                                stpnt1 = new Point3d((stpnts1[0]), (stpnts1[1] + (pitch1 * (l + 1))), (stpnts1[2]));
                                            }

                                            double[] edpnts1 = new double[3];
                                            Point3d endpnt1 = new Point3d();
                                            if (y_value > wallpnaelpnt[j].startpoint.X)
                                            {
                                                if (wallpnaelpnt[j].startpoint.X > wallpnaelpnt[j].endpoint.X)
                                                {
                                                    endpnt1 = new Point3d((stpnts1[0] + pitch1), (stpnts1[1] + (pitch1 * (l + 1))), (stpnts1[2]));
                                                }
                                                else
                                                {
                                                    endpnt1 = new Point3d((stpnts1[0] - pitch1), (stpnts1[1] + (pitch1 * (l + 1))), (stpnts1[2]));
                                                }
                                            }
                                            else
                                            {
                                                endpnt1 = new Point3d((stpnts1[0] - pitch1), (stpnts1[1] + (pitch1 * (l + 1))), (stpnts1[2]));
                                            }

                                            //if ((stpnt.Y < stpnt1.Y && ((stpnt.Y + 50) > stpnt1.Y)) || (stpnt.Y > stpnt1.Y && ((stpnt.Y - 50) < stpnt1.Y)))
                                            {

                                                Point3d stpnt_1 = new Point3d(stpnt.X, stpnt1.Y, stpnt.Z);
                                                Point3d endpnt_1 = new Point3d(endpnt.X, stpnt1.Y, endpnt.Z);
                                                if (!lstpnts.Contains(stpnt1.Y + "|" + xDataRegAppName))
                                                {

                                                    //AEE_Utility.CreateLine(stpnt_1, endpnt_1, CommonModule.clineWallPanelLyrName_sj, CommonModule.eleWallPanelLyrColor);
                                                    //SJpnts.Add(stpnt_1.Y);
                                                    Point3d tempstpnt = startPoint;
                                                    for (int len = 0; len < listOfWallPanelLength.Count; len++)
                                                    {
                                                        double wallPnlLngth_1 = listOfWallPanelLength[len];
                                                        var point_1 = AEE_Utility.get_XY(wallPanelLineAngle, wallPnlLngth_1);
                                                        Point3d endPoint_1 = new Point3d((tempstpnt.X + point_1.X), (tempstpnt.Y + point_1.Y), 0);


                                                        if ((startPoint.Y < stpnt_1.Y) && (endPoint_1.Y > stpnt_1.Y))
                                                        {
                                                            AEE_Utility.CreateLine(stpnt_1, endpnt_1, CommonModule.clineWallPanelLyrName_ver, CommonModule.eleWallPanelLyrColor);
                                                        }
                                                        else if ((startPoint.Y > stpnt_1.Y) && (endPoint_1.Y < stpnt_1.Y))
                                                        {
                                                            AEE_Utility.CreateLine(stpnt_1, endpnt_1, CommonModule.clineWallPanelLyrName_ver, CommonModule.eleWallPanelLyrColor);
                                                        }


                                                        tempstpnt = endPoint_1;
                                                    }

                                                }
                                                lstpnts.Add(stpnt1.Y + "|" + xDataRegAppName);
                                                count++;
                                                exit = true;
                                            }


                                        }
                                    }
                                }
                            }

                            if (lstpnts.Count > 0)
                            {
                                bool isbetween = false;
                                for (int m = 0; m < lstpnts.Count; m++)
                                {
                                    double val = Convert.ToDouble(lstpnts[m].Split('|')[0]);
                                    if ((stpnt.Y < val && ((stpnt.Y + 100) > val)) || (stpnt.Y > val && ((stpnt.Y - 100) < val)))
                                    {
                                        isbetween = true;
                                    }
                                }

                                if (isbetween == false)
                                {
                                    SJpnts.Add(stpnt.Y);
                                    AEE_Utility.CreateLine(stpnt, endpnt, CommonModule.clineWallPanelLyrName_sj, CommonModule.eleWallPanelLyrColor);
                                }
                            }
                            else
                            {
                                SJpnts.Add(stpnt.Y);
                                AEE_Utility.CreateLine(stpnt, endpnt, CommonModule.clineWallPanelLyrName_sj, CommonModule.eleWallPanelLyrColor);
                            }
                        }




                        try
                        {
                            pitch = 0;
                            double dW = length;
                            if (dW >= 250)
                            {
                                pitch = PanelLayout_UI.ver_max_pitch;
                            }
                            else if (dW < 250)
                            {
                                pitch = PanelLayout_UI.ver_min_pitch;
                            }
                            linecount = dW / pitch;
                            max_line = Convert.ToInt32(Math.Floor(linecount));
                            remian_dist = dW - ((pitch * max_line));

                            if (remian_dist < 50)
                            {
                                max_line--;
                                if (dW == 75)
                                {
                                    double[] stpnts = new double[3];
                                    stpnts = offsetStartPoint.ToArray();

                                    Point3d stpnt = new Point3d();
                                    Point3d endpnt = new Point3d();

                                    if (y_value > offsetStartPoint.X)
                                    {
                                        stpnt = new Point3d((stpnts[0] + 25), (stpnts[1] + (dW / 2)), (stpnts[2]));
                                        endpnt = new Point3d((stpnts[0] - 50), (stpnts[1] + (dW / 2)), (stpnts[2]));
                                    }
                                    else
                                    {
                                        stpnt = new Point3d((stpnts[0] - 25), (stpnts[1] + (dW / 2)), (stpnts[2]));
                                        endpnt = new Point3d((stpnts[0] + 50), (stpnts[1] + (dW / 2)), (stpnts[2]));
                                    }
                                    AEE_Utility.CreateLine(stpnt, endpnt, CommonModule.clineWallPanelLyrName_sj, CommonModule.eleWallPanelLyrColor);
                                }
                            }

                            for (int i = 0; i < max_line; i++)
                            {

                                double[] stpnts = new double[3];
                                stpnts = startPoint.ToArray();
                                Point3d stpnt = new Point3d();
                                Point3d endpnt = new Point3d();
                                bool isleft = false;
                                if (y_value > offsetStartPoint.X)
                                {
                                    isleft = true;
                                    stpnt = new Point3d((stpnts[0] + 25), (stpnts[1] + (pitch * (i + 1))), (stpnts[2]));
                                    endpnt = new Point3d((stpnts[0] - 50), (stpnts[1] + (pitch * (i + 1))), (stpnts[2]));

                                }
                                else
                                {
                                    stpnt = new Point3d((stpnts[0] - 25), (stpnts[1] + (pitch * (i + 1))), (stpnts[2]));
                                    endpnt = new Point3d((stpnts[0] + 50), (stpnts[1] + (pitch * (i + 1))), (stpnts[2]));
                                }

                                AEE_Utility.CreateLine(stpnt, endpnt, CommonModule.clineWallPanelLyrName_sj, CommonModule.eleWallPanelLyrColor);


                            }
                        }
                        catch { }

                    }
                }
            }
        }

        private void slapjoint_corner_cline_creation()
        {
            for (int corner = 0; corner < CommonModule.slapjoint_corner_CLine.Count; corner++)
            {

                Point3d basepnt = CommonModule.slapjoint_corner_CLine[corner].basepnt;
                double center_X = CommonModule.slapjoint_corner_CLine[corner].center_X;
                double center_Y = CommonModule.slapjoint_corner_CLine[corner].center_Y;
                string xDataRegAppName = CommonModule.slapjoint_corner_CLine[corner].wallname;
                double flange1 = CommonModule.slapjoint_corner_CLine[corner].flange1;
                double flange2 = CommonModule.slapjoint_corner_CLine[corner].flange2;

                if (basepnt.X < center_X && basepnt.Y < center_Y)
                {
                    Point3d strtPoint1 = basepnt;
                    Point3d endPoint1 = new Point3d(basepnt.X + flange1 - CommonModule.slabJointPanelDepth, basepnt.Y, 0);

                    DECK_inecreation(strtPoint1, endPoint1, 0, 1, 50, false);

                    strtPoint1 = basepnt;
                    endPoint1 = new Point3d(basepnt.X, basepnt.Y + flange1 - CommonModule.slabJointPanelDepth, 0);
                    DECK_inecreation(strtPoint1, endPoint1, 90, 4, 50, false);


                    strtPoint1 = new Point3d(basepnt.X - CommonModule.slabJointPanelDepth, basepnt.Y - CommonModule.slabJointPanelDepth, basepnt.Z);
                    endPoint1 = new Point3d(basepnt.X + flange1 - CommonModule.slabJointPanelDepth, basepnt.Y, 0);

                    DECK_inecreation(strtPoint1, endPoint1, 0, 1, 100, true);

                    strtPoint1 = new Point3d(basepnt.X - CommonModule.slabJointPanelDepth, basepnt.Y - CommonModule.slabJointPanelDepth, basepnt.Z);
                    endPoint1 = new Point3d(basepnt.X, basepnt.Y + flange1 - CommonModule.slabJointPanelDepth, 0);
                    DECK_inecreation(strtPoint1, endPoint1, 90, 4, 100, true);
                }
                else if (basepnt.X > center_X && basepnt.Y > center_Y)
                {
                    Point3d strtPoint1 = basepnt;
                    Point3d endPoint1 = new Point3d(basepnt.X - flange1 + CommonModule.slabJointPanelDepth, basepnt.Y, 0);

                    DECK_inecreation(strtPoint1, endPoint1, 0, 3, 50, false);

                    strtPoint1 = basepnt;
                    endPoint1 = new Point3d(basepnt.X, basepnt.Y - flange1 + CommonModule.slabJointPanelDepth, 0);
                    DECK_inecreation(strtPoint1, endPoint1, 90, 7, 50, false);



                    strtPoint1 = new Point3d(basepnt.X + CommonModule.slabJointPanelDepth, basepnt.Y + CommonModule.slabJointPanelDepth, basepnt.Z);
                    endPoint1 = new Point3d(basepnt.X - flange1 + CommonModule.slabJointPanelDepth, basepnt.Y + CommonModule.slabJointPanelDepth, 0);

                    DECK_inecreation(strtPoint1, endPoint1, 0, 3, 100, true);

                    strtPoint1 = new Point3d(basepnt.X + CommonModule.slabJointPanelDepth, basepnt.Y + CommonModule.slabJointPanelDepth, basepnt.Z);
                    endPoint1 = new Point3d(basepnt.X + CommonModule.slabJointPanelDepth, basepnt.Y - flange1 + CommonModule.slabJointPanelDepth, 0);
                    DECK_inecreation(strtPoint1, endPoint1, 90, 7, 100, true);
                }
                else if (basepnt.X < center_X && basepnt.Y > center_Y)
                {
                    Point3d strtPoint1 = basepnt;
                    Point3d endPoint1 = new Point3d(basepnt.X + flange1 - CommonModule.slabJointPanelDepth, basepnt.Y, 0);

                    DECK_inecreation(strtPoint1, endPoint1, 0, 5, 50, false);

                    strtPoint1 = new Point3d(basepnt.X, basepnt.Y - flange1 + CommonModule.slabJointPanelDepth, 0);
                    endPoint1 = basepnt;
                    DECK_inecreation(strtPoint1, endPoint1, 90, 4, 50, false);


                    strtPoint1 = new Point3d(basepnt.X - CommonModule.slabJointPanelDepth, basepnt.Y + CommonModule.slabJointPanelDepth, basepnt.Z); ;
                    endPoint1 = new Point3d(basepnt.X + flange1 - CommonModule.slabJointPanelDepth, basepnt.Y + CommonModule.slabJointPanelDepth, 0);

                    DECK_inecreation(strtPoint1, endPoint1, 0, 5, 100, true);

                    strtPoint1 = new Point3d(basepnt.X - CommonModule.slabJointPanelDepth, basepnt.Y - flange1 + CommonModule.slabJointPanelDepth, 0);
                    endPoint1 = new Point3d(basepnt.X - CommonModule.slabJointPanelDepth, basepnt.Y + CommonModule.slabJointPanelDepth, basepnt.Z); ;
                    DECK_inecreation(strtPoint1, endPoint1, 90, 4, 100, true);
                }
                else if (basepnt.X > center_X && basepnt.Y < center_Y)
                {
                    Point3d strtPoint1 = basepnt;
                    Point3d endPoint1 = new Point3d(basepnt.X - flange1 + CommonModule.slabJointPanelDepth, basepnt.Y, 0);

                    DECK_inecreation(strtPoint1, endPoint1, 0, 6, 50, false);

                    strtPoint1 = basepnt;
                    endPoint1 = new Point3d(basepnt.X, basepnt.Y + flange1 - CommonModule.slabJointPanelDepth, 0);
                    DECK_inecreation(strtPoint1, endPoint1, 90, 2, 50, false);


                    strtPoint1 = new Point3d(basepnt.X + CommonModule.slabJointPanelDepth, basepnt.Y - CommonModule.slabJointPanelDepth, basepnt.Z); ;
                    endPoint1 = new Point3d(basepnt.X - flange1 + CommonModule.slabJointPanelDepth, basepnt.Y - CommonModule.slabJointPanelDepth, 0);

                    DECK_inecreation(strtPoint1, endPoint1, 0, 6, 100, true);

                    strtPoint1 = new Point3d(basepnt.X + CommonModule.slabJointPanelDepth, basepnt.Y - CommonModule.slabJointPanelDepth, basepnt.Z); ;
                    endPoint1 = new Point3d(basepnt.X + CommonModule.slabJointPanelDepth, basepnt.Y + flange1 - CommonModule.slabJointPanelDepth, 0);
                    DECK_inecreation(strtPoint1, endPoint1, 90, 2, 100, true);
                }
            }
        }

        private void kickerpanel_CLine_Creation()
        {
            List<string> lstpnts = new List<string>();
            for (int cline = 0; cline < CommonModule.kIcker_panel_CLine.Count; cline++)
            {
                string xDataRegAppName = CommonModule.kIcker_panel_CLine[cline].wallname;
                bool flag_X_Axis = CommonModule.kIcker_panel_CLine[cline].x_axis;
                double length = CommonModule.kIcker_panel_CLine[cline].length;
                Point3d startPoint = CommonModule.kIcker_panel_CLine[cline].startpoint;
                Vector3d moveVector = CommonModule.kIcker_panel_CLine[cline].movevector;
                Point3d offsetStartPoint = CommonModule.kIcker_panel_CLine[cline].offsetpoint;
                List<double> listOfWallPanelLength = CommonModule.kIcker_panel_CLine[cline].listOfWallPanelLength;
                double wallPanelLineAngle = CommonModule.kIcker_panel_CLine[cline].angle;


                string txt = CommonModule.textvalue.Where(x => x.StartsWith(xDataRegAppName)).Select(y => y).First();
                if (txt != null)
                {
                    if (flag_X_Axis)
                    {
                        double y_value = Convert.ToDouble(txt.Split('|')[1].Split(',')[1]);

                        double pitch = 50;

                        double linecount = length / pitch;
                        int max_line = Convert.ToInt32(Math.Floor(linecount));
                        double remian_dist = length - ((pitch * max_line));

                        if (remian_dist < pitch)
                        {
                            max_line--;
                        }


                        int count = 0;

                        List<string> pnts = new List<string>();
                        for (int i = 0; i < max_line; i++)
                        {

                            double[] stpnts = new double[3];
                            stpnts = startPoint.ToArray();
                            stpnts[0] = stpnts[0] + moveVector.X;
                            Point3d stpnt = new Point3d((stpnts[0] + (pitch * (i + 1))), (stpnts[1]), (stpnts[2]));

                            double[] edpnts = new double[3];
                            Point3d endpnt = new Point3d();
                            bool isleft = false;
                            if (y_value > offsetStartPoint.Y)
                            {
                                if (offsetStartPoint.Y < startPoint.Y)
                                {
                                    stpnts = startPoint.ToArray();
                                    stpnts[0] = stpnts[0] + moveVector.X;
                                    stpnt = new Point3d((stpnts[0] + (pitch * (i + 1))), (stpnts[1]), (stpnts[2]));
                                }
                                endpnt = new Point3d((stpnts[0] + (pitch * (i + 1))), (stpnts[1] + pitch), (stpnts[2]));
                            }
                            else
                            {
                                endpnt = new Point3d((stpnts[0] + (pitch * (i + 1))), (stpnts[1] - pitch), (stpnts[2]));
                            }


                            if (count == 0)
                            {
                                List<WP_Cline_data> wallpnaelpnt = CommonModule.wallpanel_pnts.Where(x => x.panelname.ToLower().Contains(xDataRegAppName.ToLower())).Select(y => y).ToList();
                                bool isccreated = false;

                                if (wallpnaelpnt != null && wallpnaelpnt.Count > 0)
                                {
                                    if (y_value < wallpnaelpnt[0].startpoint.Y)
                                    {
                                        wallpnaelpnt.Reverse();
                                    }
                                    bool exit = false;
                                    for (int j = 0; j < wallpnaelpnt.Count; j++)
                                    {
                                        if (wallpnaelpnt[j].panelname.Contains("_R_") || (wallpnaelpnt[j].xaxis != true))
                                        {
                                            continue;
                                        }

                                        double pitch1 = 0;
                                        bool reverse = false;
                                        double dW1 = wallpnaelpnt[j].startpoint.X - wallpnaelpnt[j].endpoint.X;

                                        if (dW1 < 0)
                                        {
                                            dW1 = wallpnaelpnt[j].endpoint.X - wallpnaelpnt[j].startpoint.X;
                                            reverse = true;
                                        }
                                        if (dW1 >= 250)
                                        {
                                            pitch1 = PanelLayout_UI.ver_max_pitch;
                                        }
                                        else if (dW1 < 250)
                                        {
                                            pitch1 = PanelLayout_UI.ver_min_pitch;
                                        }
                                        double linecount1 = dW1 / pitch1;
                                        double max_line1 = Convert.ToInt32(Math.Floor(linecount1));
                                        if (max_line1 < 0)
                                        {
                                            max_line1 = Math.Abs(max_line1);
                                        }
                                        double remian_dist1 = dW1 - ((pitch1 * max_line1));
                                        double[] stpnts1 = new double[3];
                                        stpnts1 = wallpnaelpnt[j].startpoint.ToArray();
                                        if (remian_dist1 < 50)
                                        {
                                            max_line1--;
                                            if (dW1 == 75)
                                            {
                                                stpnts1[0] = stpnts1[0] + moveVector.X;

                                                Point3d stpnt1 = new Point3d();
                                                Point3d endpnt1 = new Point3d();
                                                if (y_value > offsetStartPoint.Y)
                                                {
                                                    stpnt1 = new Point3d((stpnts1[0] + (dW1 / 2)), (stpnt.Y), stpnt.Z);
                                                    endpnt1 = new Point3d((stpnts1[0] + (dW1 / 2)), (endpnt.Y - 50), endpnt.Z);
                                                }
                                                else
                                                {


                                                    stpnt1 = new Point3d((stpnts1[0] + (dW1 / 2)), (stpnt.Y), stpnt.Z);
                                                    endpnt1 = new Point3d((stpnts1[0] + (dW1 / 2)), (endpnt.Y + 50), endpnt.Z);
                                                }
                                                AEE_Utility.CreateLine(stpnt1, endpnt1, CommonModule.clineWallPanelLyrName_ver, CommonModule.eleWallPanelLyrColor);
                                            }
                                        }



                                        for (int l = 0; l < max_line1; l++)
                                        {

                                            stpnts1 = wallpnaelpnt[j].startpoint.ToArray();
                                            if (y_value < wallpnaelpnt[j].startpoint.Y)
                                            {
                                                stpnts1 = wallpnaelpnt[j].endpoint.ToArray();
                                            }

                                            stpnts1[0] = stpnts1[0] + moveVector.X;



                                            Point3d stpnt1 = new Point3d((stpnts1[0] + (pitch1 * (l + 1))), (stpnts1[1]), (stpnts1[2]));

                                            if (y_value < wallpnaelpnt[j].startpoint.Y)
                                            {
                                                stpnt1 = new Point3d((stpnts1[0] - (pitch1 * (l + 1))), (stpnts1[1]), (stpnts1[2]));
                                            }

                                            double[] edpnts1 = new double[3];
                                            Point3d endpnt1 = new Point3d();
                                            if (y_value > wallpnaelpnt[j].startpoint.Y)
                                            {
                                                if (wallpnaelpnt[j].startpoint.Y > wallpnaelpnt[j].endpoint.Y)
                                                {
                                                    endpnt1 = new Point3d((stpnts1[0] + pitch1), (stpnts1[1] + (pitch1 * (l + 1))), (stpnts1[2]));
                                                }
                                                else
                                                {
                                                    endpnt1 = new Point3d((stpnts1[0] - pitch1), (stpnts1[1] + (pitch1 * (l + 1))), (stpnts1[2]));
                                                }
                                            }
                                            else
                                            {
                                                endpnt1 = new Point3d((stpnts1[0] - pitch1), (stpnts1[1] + (pitch1 * (l + 1))), (stpnts1[2]));
                                            }

                                            //if ((stpnt.Y < stpnt1.Y && ((stpnt.Y + 50) > stpnt1.Y)) || (stpnt.Y > stpnt1.Y && ((stpnt.Y - 50) < stpnt1.Y)))
                                            {
                                                double x_value = stpnt1.X;
                                                //if (xDataRegAppName.ToLower().Contains("w3_ex_"))
                                                //{
                                                //    if (lstpnts.Count > 0)
                                                //    {
                                                //        //double pntx = Math.Abs(stpnt1.X - lstpnts[lstpnts.Count - 1]);

                                                //        //if (pntx > 100 && l == 0)
                                                //        //{
                                                //        //    x_value = lstpnts[lstpnts.Count - 1] + pitch1;
                                                //        //}
                                                //        if (l == 0)
                                                //        {

                                                //            x_value = lstpnts[lstpnts.Count - 1] + pitch1 + remian_dist1;
                                                //        }
                                                //    }
                                                //}

                                                Point3d stpnt_1 = new Point3d(x_value, stpnt.Y, stpnt.Z);
                                                Point3d endpnt_1 = new Point3d(x_value, endpnt.Y, endpnt.Z);
                                                if (!lstpnts.Contains(x_value.ToString() + "|" + xDataRegAppName))
                                                {
                                                    //AEE_Utility.CreateLine(stpnt_1, endpnt_1, CommonModule.kickerWallPanelLayerName, CommonModule.eleWallPanelLyrColor);
                                                    //kickerpnts.Add(stpnt_1.X);
                                                    Point3d tempstpnt = new Point3d(startPoint.X + moveVector.X, startPoint.Y, startPoint.Z); ;
                                                    for (int len = 0; len < listOfWallPanelLength.Count; len++)
                                                    {
                                                        double wallPnlLngth_1 = listOfWallPanelLength[len];
                                                        var point_1 = AEE_Utility.get_XY(wallPanelLineAngle, wallPnlLngth_1);
                                                        Point3d endPoint_1 = new Point3d((tempstpnt.X + point_1.X), (tempstpnt.Y + point_1.Y), 0);


                                                        if ((tempstpnt.X < stpnt_1.X) && (endPoint_1.X > stpnt_1.X))
                                                        {
                                                            AEE_Utility.CreateLine(stpnt_1, endpnt_1, CommonModule.clineWallPanelLyrName_ver, CommonModule.eleWallPanelLyrColor);
                                                        }

                                                        tempstpnt = endPoint_1;
                                                    }
                                                }
                                                lstpnts.Add(x_value.ToString() + "|" + xDataRegAppName);
                                                //List<cornerpnt> corpnt = CommonModule.corner_pnts.Where(x => x.wallname.ToLower().Contains(xDataRegAppName.ToLower())).Select(y => y).ToList();
                                                //for (int n = 0; n < corpnt.Count; n++)
                                                //{
                                                //    if (corpnt[n].x_axis == true)
                                                //    {
                                                //        if (corpnt[n].startpoint.X < stpnt_1.X && (((corpnt[n].startpoint.X + 50) > stpnt_1.X)) || (corpnt[n].startpoint.X > stpnt_1.X && ((corpnt[n].startpoint.X - 50) < stpnt_1.X)))
                                                //        {
                                                //            AEE_Utility.deleteEntity(corpnt[n].objid);
                                                //        }
                                                //    }
                                                //}

                                                count++;
                                                exit = true;
                                            }


                                        }
                                    }
                                }
                            }

                            if (lstpnts.Count > 0)
                            {
                                bool isbetween = false;
                                for (int m = 0; m < lstpnts.Count; m++)
                                {
                                    double val = Convert.ToDouble(lstpnts[m].Split('|')[0]);

                                    if ((stpnt.X < val && ((stpnt.X + 50) > val)) || (stpnt.X > val && ((stpnt.X - 50) < val)))
                                    {
                                        isbetween = true;
                                    }
                                }

                                if (isbetween == false)
                                {
                                    kickerpnts.Add(stpnt.X);
                                    AEE_Utility.CreateLine(stpnt, endpnt, CommonModule.kickerWallPanelLayerName, CommonModule.eleWallPanelLyrColor);
                                }
                            }
                            else
                            {
                                kickerpnts.Add(stpnt.X);
                                AEE_Utility.CreateLine(stpnt, endpnt, CommonModule.kickerWallPanelLayerName, CommonModule.eleWallPanelLyrColor);
                            }
                        }


                        try
                        {
                            pitch = 0;
                            double dW = length;
                            if (dW >= 250)
                            {
                                pitch = PanelLayout_UI.ver_max_pitch;
                            }
                            else if (dW < 250)
                            {
                                pitch = PanelLayout_UI.ver_min_pitch;
                            }
                            linecount = dW / pitch;
                            max_line = Convert.ToInt32(Math.Floor(linecount));
                            remian_dist = dW - ((pitch * max_line));

                            if (remian_dist < 50)
                            {
                                max_line--;
                                if (dW == 75)
                                {
                                    double[] stpnts = new double[3];
                                    stpnts = startPoint.ToArray();
                                    stpnts[0] = stpnts[0] + moveVector.X;

                                    Point3d stpnt = new Point3d();
                                    Point3d endpnt = new Point3d();

                                    if (y_value > offsetStartPoint.Y)
                                    {
                                        if (offsetStartPoint.Y < startPoint.Y)
                                        {
                                            stpnts = offsetStartPoint.ToArray();
                                            stpnts[0] = stpnts[0] + moveVector.X;
                                            stpnt = new Point3d((stpnts[0] + (dW / 2)), (stpnts[1] - 50), (stpnts[2]));
                                            endpnt = new Point3d((stpnts[0] + (dW / 2)), (stpnts[1]), (stpnts[2]));
                                        }
                                        else
                                        {
                                            stpnt = new Point3d((stpnts[0] + (dW / 2)), (stpnts[1] - 50), (stpnts[2]));
                                            endpnt = new Point3d((stpnts[0] + (dW / 2)), (stpnts[1]), (stpnts[2]));
                                        }

                                    }
                                    else
                                    {
                                        stpnts = startPoint.ToArray();
                                        stpnts[0] = stpnts[0] + moveVector.X;
                                        stpnt = new Point3d((stpnts[0] + (dW / 2)), (stpnts[1] + 50), (stpnts[2]));

                                        endpnt = new Point3d((stpnts[0] + (dW / 2)), (stpnts[1]), (stpnts[2]));
                                    }
                                    AEE_Utility.CreateLine(stpnt, endpnt, CommonModule.kickerWallPanelLayerName, CommonModule.eleWallPanelLyrColor);
                                }
                            }

                            for (int i = 0; i < max_line; i++)
                            {

                                double[] stpnts = new double[3];
                                stpnts = startPoint.ToArray();
                                stpnts[0] = stpnts[0] + moveVector.X;

                                Point3d stpnt = new Point3d();
                                Point3d endpnt = new Point3d();

                                if (y_value > offsetStartPoint.Y)
                                {
                                    if (offsetStartPoint.Y < startPoint.Y)
                                    {
                                        stpnts = startPoint.ToArray();
                                        stpnts[0] = stpnts[0] + moveVector.X;
                                        stpnt = new Point3d((stpnts[0] + (pitch * (i + 1))), (stpnts[1] - 50), (stpnts[2]));
                                        endpnt = new Point3d((stpnts[0] + (pitch * (i + 1))), (stpnts[1]), (stpnts[2]));
                                    }
                                    else
                                    {
                                        stpnt = new Point3d((stpnts[0] + (pitch * (i + 1))), (stpnts[1] + 50), (stpnts[2]));
                                        endpnt = new Point3d((stpnts[0] + (pitch * (i + 1))), (stpnts[1]), (stpnts[2]));
                                    }
                                }
                                else
                                {

                                    stpnts = startPoint.ToArray();
                                    stpnts[0] = stpnts[0] + moveVector.X;
                                    stpnt = new Point3d((stpnts[0] + (pitch * (i + 1))), (stpnts[1] + 50), (stpnts[2]));
                                    endpnt = new Point3d((stpnts[0] + (pitch * (i + 1))), (stpnts[1]), (stpnts[2]));
                                }

                                AEE_Utility.CreateLine(stpnt, endpnt, CommonModule.kickerWallPanelLayerName, CommonModule.eleWallPanelLyrColor);
                            }
                        }
                        catch { }

                    }
                    else
                    {
                        double y_value = Convert.ToDouble(txt.Split('|')[1].Split(',')[0]);

                        double pitch = 50;

                        double linecount = length / pitch;
                        int max_line = Convert.ToInt32(Math.Floor(linecount));
                        double remian_dist = length - ((pitch * max_line));

                        if (remian_dist < pitch)
                        {
                            max_line--;
                        }

                        List<string> pnts = new List<string>();
                        List<double> xpnt = new List<double>();
                        int count = 0;

                        for (int i = 0; i < max_line; i++)
                        {

                            double[] stpnts = new double[3];
                            stpnts = startPoint.ToArray();
                            stpnts[0] = stpnts[0] + moveVector.X;
                            Point3d stpnt = new Point3d((stpnts[0]), (stpnts[1] + (pitch * (i + 1))), (stpnts[2]));

                            double[] edpnts = new double[3];
                            Point3d endpnt = new Point3d();
                            bool isleft = false;
                            if (y_value > startPoint.X)
                            {
                                if (startPoint.X > offsetStartPoint.X)
                                {
                                    endpnt = new Point3d((stpnts[0] + pitch), (stpnts[1] + (pitch * (i + 1))), (stpnts[2]));
                                }
                                else
                                {
                                    isleft = true;
                                    endpnt = new Point3d((stpnts[0] - pitch), (stpnts[1] + (pitch * (i + 1))), (stpnts[2]));
                                }
                            }
                            else
                            {
                                isleft = true;
                                endpnt = new Point3d((stpnts[0] - pitch), (stpnts[1] + (pitch * (i + 1))), (stpnts[2]));
                            }

                            //AEE_Utility.CreateLine(stpnt, endpnt, CommonModule.kickerWallPanelLayerName, CommonModule.eleWallPanelLyrColor);


                            if (count == 0)
                            {
                                List<WP_Cline_data> wallpnaelpnt = CommonModule.wallpanel_pnts.Where(x => x.panelname.ToLower().Contains(xDataRegAppName.ToLower())).Select(y => y).ToList();
                                bool isccreated = false;

                                if (wallpnaelpnt != null && wallpnaelpnt.Count > 0)
                                {

                                    bool exit = false;
                                    for (int j = 0; j < wallpnaelpnt.Count; j++)
                                    {
                                        if (wallpnaelpnt[j].panelname.Contains("_R_") || (wallpnaelpnt[j].xaxis == true))
                                        {
                                            continue;
                                        }

                                        double pitch1 = 0;
                                        bool inbetval = false;
                                        double dW1 = wallpnaelpnt[j].startpoint.Y - wallpnaelpnt[j].endpoint.Y;

                                        if (dW1 < 0)
                                        {
                                            dW1 = wallpnaelpnt[j].endpoint.Y - wallpnaelpnt[j].startpoint.Y;
                                        }


                                        if (wallpnaelpnt[j].startpoint.Y < stpnt.Y && wallpnaelpnt[j].endpoint.Y > stpnt.Y)
                                        {
                                            inbetval = true;
                                        }

                                        //double x = wallpnaelpnt[j].startpoint.X + 60 + moveVector.X;
                                        //if (x != stpnt.X)
                                        //{
                                        //    continue;
                                        //}


                                        if (dW1 >= 250)
                                        {
                                            pitch1 = PanelLayout_UI.ver_max_pitch;
                                        }
                                        else if (dW1 < 250)
                                        {
                                            pitch1 = PanelLayout_UI.ver_min_pitch;
                                        }
                                        double linecount1 = dW1 / pitch1;
                                        double max_line1 = Convert.ToInt32(Math.Floor(linecount1));
                                        if (max_line1 < 0)
                                        {
                                            max_line1 = Math.Abs(max_line1);

                                            if (wallpnaelpnt[j].endpoint.Y < stpnt.Y && wallpnaelpnt[j].startpoint.Y > stpnt.Y)
                                            {
                                                inbetval = true;
                                            }

                                        }

                                        //if (inbetval == false)
                                        //{
                                        //    continue;
                                        //}
                                        double remian_dist1 = dW1 - ((pitch1 * max_line1));
                                        double[] stpnts1 = new double[3];
                                        stpnts1 = wallpnaelpnt[j].startpoint.ToArray();
                                        if (remian_dist1 < 50)
                                        {
                                            max_line1--;
                                            if (dW1 == 75)
                                            {

                                                //stpnts1[0] = stpnts[0];


                                                Point3d stpnt1 = new Point3d();
                                                Point3d endpnt1 = new Point3d();
                                                if (y_value > offsetStartPoint.X)
                                                {
                                                    stpnt1 = new Point3d((stpnt.X), (stpnts1[1] + (dW1 / 2)), stpnt.Z);
                                                    endpnt1 = new Point3d((endpnt.X - 50), (stpnts1[1] + (dW1 / 2)), endpnt.Z);
                                                }
                                                else
                                                {
                                                    stpnt1 = new Point3d((stpnt.X), (stpnts1[1] + (dW1 / 2)), stpnt.Z);
                                                    endpnt1 = new Point3d((endpnt.X + 50), (stpnts1[1] + (dW1 / 2)), endpnt.Z);
                                                }
                                                AEE_Utility.CreateLine(stpnt1, endpnt1, CommonModule.clineWallPanelLyrName_ver, CommonModule.eleWallPanelLyrColor);
                                            }
                                        }



                                        for (int l = 0; l < max_line1; l++)
                                        {

                                            stpnts1 = wallpnaelpnt[j].endpoint.ToArray();
                                            if (isleft == true)
                                            {
                                                stpnts1 = wallpnaelpnt[j].startpoint.ToArray();
                                            }

                                            stpnts1[0] = stpnts1[0];// + moveVector.X;

                                            Point3d stpnt1 = new Point3d((stpnts1[0]), (stpnts1[1] - (pitch1 * (l + 1))), (stpnts1[2]));

                                            if (isleft == true)
                                            {
                                                stpnt1 = new Point3d((stpnts1[0]), (stpnts1[1] + (pitch1 * (l + 1))), (stpnts1[2]));
                                            }

                                            double[] edpnts1 = new double[3];
                                            Point3d endpnt1 = new Point3d();
                                            if (y_value > wallpnaelpnt[j].startpoint.X)
                                            {
                                                if (wallpnaelpnt[j].startpoint.X > wallpnaelpnt[j].endpoint.X)
                                                {
                                                    endpnt1 = new Point3d((stpnts1[0] + pitch1), (stpnts1[1] + (pitch1 * (l + 1))), (stpnts1[2]));
                                                }
                                                else
                                                {
                                                    endpnt1 = new Point3d((stpnts1[0] - pitch1), (stpnts1[1] + (pitch1 * (l + 1))), (stpnts1[2]));
                                                }
                                            }
                                            else
                                            {
                                                endpnt1 = new Point3d((stpnts1[0] - pitch1), (stpnts1[1] + (pitch1 * (l + 1))), (stpnts1[2]));
                                            }

                                            //if ((stpnt.Y < stpnt1.Y && ((stpnt.Y + 50) > stpnt1.Y)) || (stpnt.Y > stpnt1.Y && ((stpnt.Y - 50) < stpnt1.Y)))
                                            {

                                                Point3d stpnt_1 = new Point3d(stpnt.X, stpnt1.Y, stpnt.Z);
                                                Point3d endpnt_1 = new Point3d(endpnt.X, stpnt1.Y, endpnt.Z);
                                                if (!lstpnts.Contains(stpnt1.Y.ToString() + "|" + xDataRegAppName))
                                                {

                                                    //AEE_Utility.CreateLine(stpnt_1, endpnt_1, CommonModule.kickerWallPanelLayerName, CommonModule.eleWallPanelLyrColor);
                                                    //kickerpnts.Add(stpnt_1.Y);
                                                    Point3d tempstpnt = startPoint;
                                                    for (int len = 0; len < listOfWallPanelLength.Count; len++)
                                                    {
                                                        double wallPnlLngth_1 = listOfWallPanelLength[len];
                                                        var point_1 = AEE_Utility.get_XY(wallPanelLineAngle, wallPnlLngth_1);
                                                        Point3d endPoint_1 = new Point3d((tempstpnt.X + point_1.X), (tempstpnt.Y + point_1.Y), 0);


                                                        if ((startPoint.Y < stpnt_1.Y) && (endPoint_1.Y > stpnt_1.Y))
                                                        {
                                                            AEE_Utility.CreateLine(stpnt_1, endpnt_1, CommonModule.clineWallPanelLyrName_ver, CommonModule.eleWallPanelLyrColor);
                                                        }
                                                        else if ((startPoint.Y > stpnt_1.Y) && (endPoint_1.Y < stpnt_1.Y))
                                                        {
                                                            AEE_Utility.CreateLine(stpnt_1, endpnt_1, CommonModule.clineWallPanelLyrName_ver, CommonModule.eleWallPanelLyrColor);
                                                        }


                                                        tempstpnt = endPoint_1;
                                                    }
                                                }
                                                lstpnts.Add(stpnt1.Y.ToString() + "|" + xDataRegAppName);
                                                //List<cornerpnt> corpnt = CommonModule.corner_pnts.Where(x => x.wallname.ToLower().Contains(xDataRegAppName.ToLower())).Select(y => y).ToList();
                                                //for (int n = 0; n < corpnt.Count; n++)
                                                //{
                                                //    if (corpnt[n].x_axis == false)
                                                //    {
                                                //        if (corpnt[n].startpoint.Y < stpnt_1.Y && (((corpnt[n].startpoint.Y + 50) > stpnt_1.Y)) || (corpnt[n].startpoint.Y > stpnt_1.Y && ((corpnt[n].startpoint.Y - 50) < stpnt_1.Y)))
                                                //        {
                                                //            AEE_Utility.deleteEntity(corpnt[n].objid);
                                                //        }
                                                //    }
                                                //}

                                                count++;
                                                exit = true;
                                            }


                                        }
                                    }
                                }
                            }

                            if (lstpnts.Count > 0)
                            {
                                bool isbetween = false;
                                for (int m = 0; m < lstpnts.Count; m++)
                                {
                                    double val = Convert.ToDouble(lstpnts[m].Split('|')[0]);
                                    if ((stpnt.Y < val && ((stpnt.Y + 50) > val)) || (stpnt.Y > val && ((stpnt.Y - 50) < val)))
                                    {
                                        isbetween = true;
                                    }
                                }

                                if (isbetween == false)
                                {
                                    kickerpnts.Add(stpnt.Y);
                                    AEE_Utility.CreateLine(stpnt, endpnt, CommonModule.kickerWallPanelLayerName, CommonModule.eleWallPanelLyrColor);
                                }
                            }
                            else
                            {
                                kickerpnts.Add(stpnt.Y);
                                AEE_Utility.CreateLine(stpnt, endpnt, CommonModule.kickerWallPanelLayerName, CommonModule.eleWallPanelLyrColor);
                            }
                        }

                        try
                        {
                            pitch = 0;
                            double dW = length;
                            if (dW >= 250)
                            {
                                pitch = PanelLayout_UI.ver_max_pitch;
                            }
                            else if (dW < 250)
                            {
                                pitch = PanelLayout_UI.ver_min_pitch;
                            }
                            linecount = dW / pitch;
                            max_line = Convert.ToInt32(Math.Floor(linecount));
                            remian_dist = dW - ((pitch * max_line));

                            if (remian_dist < 50)
                            {
                                max_line--;
                                if (dW == 75)
                                {
                                    double[] stpnts = new double[3];
                                    stpnts = offsetStartPoint.ToArray();
                                    stpnts[0] = stpnts[0] + moveVector.X;

                                    Point3d stpnt = new Point3d();
                                    Point3d endpnt = new Point3d();

                                    if (y_value > offsetStartPoint.X)
                                    {
                                        stpnt = new Point3d((stpnts[0]), (stpnts[1] + (dW / 2)), (stpnts[2]));
                                        endpnt = new Point3d((stpnts[0] - 50), (stpnts[1] + (dW / 2)), (stpnts[2]));
                                    }
                                    else
                                    {
                                        stpnt = new Point3d((stpnts[0]), (stpnts[1] + (dW / 2)), (stpnts[2]));
                                        endpnt = new Point3d((stpnts[0] + 50), (stpnts[1] + (dW / 2)), (stpnts[2]));
                                    }
                                    AEE_Utility.CreateLine(stpnt, endpnt, CommonModule.kickerWallPanelLayerName, CommonModule.eleWallPanelLyrColor);
                                }
                            }

                            for (int i = 0; i < max_line; i++)
                            {

                                double[] stpnts = new double[3];
                                stpnts = startPoint.ToArray();
                                stpnts[0] = stpnts[0] + moveVector.X;
                                Point3d stpnt = new Point3d();
                                Point3d endpnt = new Point3d();

                                if (y_value > offsetStartPoint.X)
                                {
                                    if (startPoint.X < offsetStartPoint.X)
                                    {
                                        stpnt = new Point3d((stpnts[0]), (stpnts[1] + (pitch * (i + 1))), (stpnts[2]));
                                        endpnt = new Point3d((stpnts[0] + 50), (stpnts[1] + (pitch * (i + 1))), (stpnts[2]));
                                    }
                                    else
                                    {
                                        stpnt = new Point3d((stpnts[0]), (stpnts[1] + (pitch * (i + 1))), (stpnts[2]));
                                        endpnt = new Point3d((stpnts[0] - 50), (stpnts[1] + (pitch * (i + 1))), (stpnts[2]));
                                    }
                                }
                                else
                                {
                                    stpnt = new Point3d((stpnts[0]), (stpnts[1] + (pitch * (i + 1))), (stpnts[2]));
                                    endpnt = new Point3d((stpnts[0] + 50), (stpnts[1] + (pitch * (i + 1))), (stpnts[2]));
                                }

                                AEE_Utility.CreateLine(stpnt, endpnt, CommonModule.kickerWallPanelLayerName, CommonModule.eleWallPanelLyrColor);
                            }
                        }
                        catch { }

                    }
                }
            }
        }

        private void kickerpanel_corner_cline_creation()
        {
            for (int corner = 0; corner < CommonModule.kIcker_corner_CLine.Count; corner++)
            {

                Point3d basepnt = CommonModule.kIcker_corner_CLine[corner].basepnt;
                double center_X = CommonModule.kIcker_corner_CLine[corner].center_X;
                double center_Y = CommonModule.kIcker_corner_CLine[corner].center_Y;
                string xDataRegAppName = CommonModule.kIcker_corner_CLine[corner].wallname;
                double flange1 = CommonModule.kIcker_corner_CLine[corner].flange1;
                double flange2 = CommonModule.kIcker_corner_CLine[corner].flange2;

                if (basepnt.X < center_X && basepnt.Y < center_Y)
                {
                    Point3d strtPoint1 = new Point3d(basepnt.X - CommonModule.panelDepth, basepnt.Y - CommonModule.panelDepth, basepnt.Z); ;
                    Point3d endPoint1 = new Point3d(basepnt.X + flange1, basepnt.Y, 0);

                    DECK_inecreation(strtPoint1, endPoint1, 0, 11, 100, false, xDataRegAppName);

                    strtPoint1 = new Point3d(basepnt.X - CommonModule.panelDepth, basepnt.Y - CommonModule.panelDepth, basepnt.Z); ;
                    endPoint1 = new Point3d(basepnt.X, basepnt.Y + flange2, 0);
                    DECK_inecreation(strtPoint1, endPoint1, 90, 44, 100, false, xDataRegAppName);


                    strtPoint1 = new Point3d(basepnt.X - CommonModule.panelDepth, basepnt.Y - CommonModule.panelDepth, basepnt.Z);
                    endPoint1 = new Point3d(basepnt.X + flange1 - CommonModule.panelDepth, basepnt.Y, 0);

                    DECK_inecreation(strtPoint1, endPoint1, 0, 1, 50, true, xDataRegAppName);

                    strtPoint1 = new Point3d(basepnt.X - CommonModule.panelDepth, basepnt.Y - CommonModule.panelDepth, basepnt.Z);
                    endPoint1 = new Point3d(basepnt.X, basepnt.Y + flange2 - CommonModule.panelDepth, 0);
                    DECK_inecreation(strtPoint1, endPoint1, 90, 4, 50, true, "W4_EX_1");
                }
                else if (basepnt.X > center_X && basepnt.Y > center_Y)
                {
                    Point3d strtPoint1 = new Point3d(basepnt.X + CommonModule.panelDepth, basepnt.Y + CommonModule.panelDepth, basepnt.Z); ;
                    Point3d endPoint1 = new Point3d(basepnt.X - flange1 + CommonModule.panelDepth, basepnt.Y + CommonModule.panelDepth, 0);

                    DECK_inecreation(strtPoint1, endPoint1, 0, 3, 100, false, xDataRegAppName);

                    strtPoint1 = new Point3d(basepnt.X + CommonModule.panelDepth, basepnt.Y + CommonModule.panelDepth, basepnt.Z); ;
                    endPoint1 = new Point3d(basepnt.X + CommonModule.panelDepth, basepnt.Y - flange2 + CommonModule.panelDepth, 0);
                    DECK_inecreation(strtPoint1, endPoint1, 90, 7, 100, false, xDataRegAppName);



                    strtPoint1 = new Point3d(basepnt.X + CommonModule.panelDepth, basepnt.Y + CommonModule.panelDepth, basepnt.Z);
                    endPoint1 = new Point3d(basepnt.X - flange1 + CommonModule.panelDepth, basepnt.Y + CommonModule.panelDepth, 0);

                    DECK_inecreation(strtPoint1, endPoint1, 0, 3, 50, true, xDataRegAppName);

                    strtPoint1 = new Point3d(basepnt.X + CommonModule.panelDepth, basepnt.Y + CommonModule.panelDepth, basepnt.Z);
                    endPoint1 = new Point3d(basepnt.X + CommonModule.panelDepth, basepnt.Y - flange2 + CommonModule.panelDepth, 0);
                    DECK_inecreation(strtPoint1, endPoint1, 90, 7, 50, true, "W2_EX_1");
                }
                else if (basepnt.X < center_X && basepnt.Y > center_Y)
                {
                    Point3d strtPoint1 = new Point3d(basepnt.X - CommonModule.panelDepth, basepnt.Y + CommonModule.panelDepth, basepnt.Z); ; ;
                    Point3d endPoint1 = new Point3d(basepnt.X + flange1 - CommonModule.panelDepth, basepnt.Y + CommonModule.panelDepth, 0);

                    DECK_inecreation(strtPoint1, endPoint1, 0, 5, 100, false, xDataRegAppName);

                    strtPoint1 = new Point3d(basepnt.X - CommonModule.panelDepth, basepnt.Y - flange2 + CommonModule.panelDepth, 0);
                    endPoint1 = new Point3d(basepnt.X - CommonModule.panelDepth, basepnt.Y + CommonModule.panelDepth, basepnt.Z); ; ;
                    DECK_inecreation(strtPoint1, endPoint1, 90, 44, 100, false, xDataRegAppName);


                    strtPoint1 = new Point3d(basepnt.X - CommonModule.panelDepth, basepnt.Y + CommonModule.panelDepth, basepnt.Z); ;
                    endPoint1 = new Point3d(basepnt.X + flange1 - CommonModule.panelDepth, basepnt.Y + CommonModule.panelDepth, 0);

                    DECK_inecreation(strtPoint1, endPoint1, 0, 5, 50, true, "W1_EX_1");

                    strtPoint1 = new Point3d(basepnt.X - CommonModule.panelDepth, basepnt.Y - flange2 + CommonModule.panelDepth, 0);
                    endPoint1 = new Point3d(basepnt.X - CommonModule.panelDepth, basepnt.Y + CommonModule.panelDepth, basepnt.Z); ;
                    DECK_inecreation(strtPoint1, endPoint1, 90, 4, 50, true, xDataRegAppName);
                }
                else if (basepnt.X > center_X && basepnt.Y < center_Y)
                {
                    Point3d strtPoint1 = new Point3d(basepnt.X + CommonModule.panelDepth, basepnt.Y - CommonModule.panelDepth, basepnt.Z);
                    Point3d endPoint1 = new Point3d(basepnt.X - flange1 + CommonModule.panelDepth, basepnt.Y - CommonModule.panelDepth, 0);

                    DECK_inecreation(strtPoint1, endPoint1, 0, 66, 100, false, xDataRegAppName);

                    strtPoint1 = new Point3d(basepnt.X + CommonModule.panelDepth, basepnt.Y - CommonModule.panelDepth, basepnt.Z);
                    endPoint1 = new Point3d(basepnt.X, basepnt.Y + flange2 - CommonModule.panelDepth, 0);
                    DECK_inecreation(strtPoint1, endPoint1, 90, 2, 100, false, xDataRegAppName);


                    strtPoint1 = new Point3d(basepnt.X + CommonModule.panelDepth, basepnt.Y - CommonModule.panelDepth, basepnt.Z); ;
                    endPoint1 = new Point3d(basepnt.X - flange1 + CommonModule.panelDepth, basepnt.Y - CommonModule.panelDepth, 0);

                    DECK_inecreation(strtPoint1, endPoint1, 0, 6, 50, true, "W3_EX_1");

                    strtPoint1 = new Point3d(basepnt.X + CommonModule.panelDepth, basepnt.Y - CommonModule.panelDepth, basepnt.Z); ;
                    endPoint1 = new Point3d(basepnt.X + CommonModule.panelDepth, basepnt.Y + flange2 - CommonModule.panelDepth, 0);
                    DECK_inecreation(strtPoint1, endPoint1, 90, 2, 50, true, xDataRegAppName);
                }
            }
        }

        //Vertical hole centerline creation
        private void draw_vertical_Cline(Point3d startPoint, double dW, double dH)
        {
            try
            {
                Vector3d vector3dOffset = Vector3d.YAxis * dH;
                Point3d offsetStartPoint = startPoint + vector3dOffset;


                double pitch = 0;
                if (dW >= 250)
                {
                    pitch = PanelLayout_UI.ver_max_pitch;
                }
                else if (dW < 250)
                {
                    pitch = PanelLayout_UI.ver_min_pitch;
                }
                double linecount = dW / pitch;
                int max_line = Convert.ToInt32(Math.Floor(linecount));
                double remian_dist = dW - ((pitch * max_line));

                if (remian_dist < 50)
                {
                    max_line--;
                    if (dW == 75)
                    {
                        double[] stpnts = new double[3];
                        stpnts = startPoint.ToArray();
                        Point3d stpnt = new Point3d((stpnts[0] + (dW / 2)), (stpnts[1] - 100), (stpnts[2]));

                        double[] edpnts = new double[3];
                        edpnts = offsetStartPoint.ToArray();
                        Point3d endpnt = new Point3d((edpnts[0] + (dW / 2)), (edpnts[1] + 100), (edpnts[2]));

                        AEE_Utility.CreateLine(stpnt, endpnt, CommonModule.clineWallPanelLyrName_ver, CommonModule.eleWallPanelLyrColor);
                    }
                }

                for (int i = 0; i < max_line; i++)
                {

                    double[] stpnts = new double[3];
                    stpnts = startPoint.ToArray();
                    Point3d stpnt = new Point3d((stpnts[0] + (pitch * (i + 1))), (stpnts[1] - 100), (stpnts[2]));

                    double[] edpnts = new double[3];
                    edpnts = offsetStartPoint.ToArray();
                    Point3d endpnt = new Point3d((edpnts[0] + (pitch * (i + 1))), (edpnts[1] + 100), (edpnts[2]));

                    AEE_Utility.CreateLine(stpnt, endpnt, CommonModule.clineWallPanelLyrName_ver, CommonModule.eleWallPanelLyrColor);
                }
            }
            catch { }
        }

        List<Point3d> lst_cline_dist = new List<Point3d>();
        //Creating Centerline for the wall panel
        private void datum_Cline_creation(Point3d startPoint, Point3d endPoint, bool isinternal)
        {
            try
            {
                double initial_dist = 450;
                double first_dist = PanelLayout_UI.f_dist;
                double top_dist = 100;
                double wall_hgt = PanelLayout_UI.int_wall_hgt;

                if (isinternal)
                {
                    initial_dist = 500;
                }


                double[] stpnts = new double[3];
                stpnts = startPoint.ToArray();
                Point3d stpnt = new Point3d((stpnts[0] - 100), (stpnts[1] + initial_dist), (stpnts[2]));

                double[] edpnts = new double[3];
                edpnts = endPoint.ToArray();
                Point3d endpnt = new Point3d((edpnts[0] + 100), (edpnts[1] + initial_dist), (edpnts[2]));
                AEE_Utility.CreateLine(stpnt, endpnt, CommonModule.clineWallPanelLyrName_hor, CommonModule.eleWallPanelLyrColor);

                lst_cline_dist.Add(stpnt);



                stpnt = new Point3d((stpnts[0] - 100), (stpnts[1] + initial_dist + PanelLayout_UI.f_dist), (stpnts[2]));
                endpnt = new Point3d((edpnts[0] + 100), (edpnts[1] + initial_dist + PanelLayout_UI.f_dist), (edpnts[2]));
                AEE_Utility.CreateLine(stpnt, endpnt, CommonModule.clineWallPanelLyrName_hor, CommonModule.eleWallPanelLyrColor);

                lst_cline_dist.Add(stpnt);



                double line_count = (wall_hgt - first_dist) / PanelLayout_UI.sec_dist;
                int max_line = Convert.ToInt32(Math.Floor(line_count));

                double remian_dist = wall_hgt - ((PanelLayout_UI.sec_dist * max_line) + first_dist);
                if (remian_dist <= 50)
                {
                    max_line--;
                }

                for (int i = 0; i < max_line; i++)
                {
                    stpnt = new Point3d((stpnts[0] - 100), (stpnts[1] + initial_dist + PanelLayout_UI.f_dist + PanelLayout_UI.sec_dist * (i + 1)), (stpnts[2]));
                    endpnt = new Point3d((edpnts[0] + 100), (edpnts[1] + initial_dist + PanelLayout_UI.f_dist + PanelLayout_UI.sec_dist * (i + 1)), (edpnts[2]));
                    AEE_Utility.CreateLine(stpnt, endpnt, CommonModule.clineWallPanelLyrName_hor, CommonModule.eleWallPanelLyrColor);

                    lst_cline_dist.Add(stpnt);





                    //if (max_line > i +1)
                    //{

                    //   Point3d stpnt_1 = new Point3d((stpnts[0] - 100), (stpnts[1] + initial_dist + PanelLayout_UI.f_dist + PanelLayout_UI.sec_dist * (i + 1 + 1)), (stpnts[2]));
                    //    Vert_Cline_data data = CommonModule.vert_cline_data.Where(x => x.startpoint.Y > stpnt.Y && x.startpoint.Y < stpnt_1.Y).Select(y => y).FirstOrDefault();

                    //    double pnt = stpnt.Y;

                    //    if (data != null)
                    //    {
                    //        double value = data.startpoint.Y - pnt;

                    //        if (value > 200)
                    //        {
                    //            double X = data.startpoint.X;
                    //            double Y = data.startpoint.Y - top_dist;
                    //            stpnt = new Point3d((X - 100), Y, (stpnts[2]));
                    //            endpnt = new Point3d((X + 100), Y, (edpnts[2]));
                    //            AEE_Utility.CreateLine(stpnt, endpnt, CommonModule.clineWallPanelLyrName_hor_SP, CommonModule.eleWallPanelLyrColor);
                    //        }
                    //    }

                    //    stpnt_1 = new Point3d((stpnts[0] - 100), (stpnts[1] + initial_dist + PanelLayout_UI.f_dist + PanelLayout_UI.sec_dist * (i - 1)), (stpnts[2]));
                    //    data = CommonModule.vert_cline_data.Where(x => x.offsetendpoint.Y < stpnt.Y && x.offsetendpoint.Y > stpnt_1.Y).Select(y => y).FirstOrDefault();


                    //    if (data != null)
                    //    {
                    //        double value = pnt - data.startpoint.Y;

                    //        if (value > 200)
                    //        {
                    //            double X = data.offsetendpoint.X;
                    //            double Y = data.offsetendpoint.Y + top_dist;
                    //            stpnt = new Point3d((X - 100), Y, (stpnts[2]));
                    //            endpnt = new Point3d((X + 100), Y, (edpnts[2]));
                    //            AEE_Utility.CreateLine(stpnt, endpnt, CommonModule.clineWallPanelLyrName_hor_SP, CommonModule.eleWallPanelLyrColor);
                    //        }
                    //    }
                    //}
                }

                bool isfromtop = false;


                if (remian_dist > 200)
                {
                    stpnt = new Point3d((stpnts[0] - 100), (stpnts[1] + initial_dist + (wall_hgt - top_dist)), (stpnts[2]));
                    endpnt = new Point3d((edpnts[0] + 100), (edpnts[1] + initial_dist + (wall_hgt - top_dist)), (edpnts[2]));
                    AEE_Utility.CreateLine(stpnt, endpnt, CommonModule.clineWallPanelLyrName_hor, CommonModule.eleWallPanelLyrColor);
                    isfromtop = true;

                    lst_cline_dist.Add(stpnt);

                }
                else if (remian_dist <= 50)
                {
                    stpnt = new Point3d((stpnts[0] - 100), (stpnts[1] + initial_dist + (wall_hgt - top_dist)), (stpnts[2]));
                    endpnt = new Point3d((edpnts[0] + 100), (edpnts[1] + initial_dist + (wall_hgt - top_dist)), (edpnts[2]));
                    AEE_Utility.CreateLine(stpnt, endpnt, CommonModule.clineWallPanelLyrName_hor, CommonModule.eleWallPanelLyrColor);
                    isfromtop = true;

                    lst_cline_dist.Add(stpnt);

                }


                if (!isinternal)
                {
                    double inside_wall_gap = 0;
                    if (isfromtop)
                    {
                        inside_wall_gap = remian_dist - 100;
                    }

                    double val = PanelLayout_UI.ext_wall_hgt - (PanelLayout_UI.f_dist - 50) - (PanelLayout_UI.sec_dist * max_line) - inside_wall_gap;


                    if (val > 200)
                    {
                        stpnt = new Point3d((stpnts[0] - 100), (stpnts[1] + initial_dist + 50 + (PanelLayout_UI.ext_wall_hgt - top_dist)), (stpnts[2]));
                        endpnt = new Point3d((edpnts[0] + 100), (edpnts[1] + initial_dist + 50 + (PanelLayout_UI.ext_wall_hgt - top_dist)), (edpnts[2]));
                        AEE_Utility.CreateLine(stpnt, endpnt, CommonModule.clineWallPanelLyrName_hor, CommonModule.eleWallPanelLyrColor);
                    }

                    double endht = (stpnts[1] + initial_dist + 50 + (PanelLayout_UI.ext_wall_hgt)) + PanelLayout_UI.kickerHt;

                    double remain = endht - ((stpnts[1] + initial_dist + 50 + (PanelLayout_UI.ext_wall_hgt - top_dist)) + PanelLayout_UI.f_dist);

                    if (remain >= 50)
                    {
                        stpnt = new Point3d((stpnts[0] - 100), ((stpnts[1] + initial_dist + 50 + (PanelLayout_UI.ext_wall_hgt - top_dist)) + PanelLayout_UI.f_dist), (stpnts[2]));
                        endpnt = new Point3d((edpnts[0] + 100), ((stpnts[1] + initial_dist + 50 + (PanelLayout_UI.ext_wall_hgt - top_dist)) + PanelLayout_UI.f_dist), (edpnts[2]));
                        AEE_Utility.CreateLine(stpnt, endpnt, CommonModule.clineWallPanelLyrName_hor, CommonModule.eleWallPanelLyrColor);
                    }
                    else
                    {
                        stpnt = new Point3d((stpnts[0] - 100), ((stpnts[1] + initial_dist + 50 + (PanelLayout_UI.ext_wall_hgt)) + (PanelLayout_UI.kickerHt / 2)), (stpnts[2]));
                        endpnt = new Point3d((edpnts[0] + 100), ((stpnts[1] + initial_dist + 50 + (PanelLayout_UI.ext_wall_hgt)) + (PanelLayout_UI.kickerHt / 2)), (edpnts[2]));
                        AEE_Utility.CreateLine(stpnt, endpnt, CommonModule.clineWallPanelLyrName_hor, CommonModule.eleWallPanelLyrColor);
                    }
                }
                else
                {


                    double endht = (stpnts[1] + initial_dist + (wall_hgt)) + PanelLayout_UI.SL;

                    double remain = endht - ((stpnts[1] + initial_dist + (wall_hgt - top_dist)) + PanelLayout_UI.f_dist);

                    if (remain >= 50)
                    {
                        stpnt = new Point3d((stpnts[0] - 100), ((stpnts[1] + initial_dist + (wall_hgt - top_dist)) + PanelLayout_UI.f_dist), (stpnts[2]));
                        endpnt = new Point3d((edpnts[0] + 100), ((stpnts[1] + initial_dist + (wall_hgt - top_dist)) + PanelLayout_UI.f_dist), (edpnts[2]));
                        AEE_Utility.CreateLine(stpnt, endpnt, CommonModule.clineWallPanelLyrName_hor, CommonModule.eleWallPanelLyrColor);

                    }
                    else
                    {
                        stpnt = new Point3d((stpnts[0] - 100), ((stpnts[1] + initial_dist + (wall_hgt) + (PanelLayout_UI.SL / 2))), (stpnts[2]));
                        endpnt = new Point3d((edpnts[0] + 100), ((stpnts[1] + initial_dist + (wall_hgt)) + (PanelLayout_UI.SL / 2)), (edpnts[2]));
                        AEE_Utility.CreateLine(stpnt, endpnt, CommonModule.clineWallPanelLyrName_hor, CommonModule.eleWallPanelLyrColor);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void elevation_hor_clien_creation()
        {
            int rm = 0;
            double SecTxtSz = 100;
            // Get External wall lines
            for (int index = 0; index < ExternalWallHelper.listOfListOfExternalWallRoomLineId_With_WallName.Count; index++)
            {
                List<ObjectId> listOfExternalWallLineId = ExternalWallHelper.listOfListOfExternalWallRoomLineId_With_WallName[index];
                double ln = 0.0;


                for (int index1 = 0; index1 < listOfExternalWallLineId.Count; index1++)
                {

                    Line line = AEE_Utility.GetLine(listOfExternalWallLineId[index1]);
                    Point3d startPoint = BaseElevationPoint + new Vector3d(ln, -(rm * WallHeight + 500), 0);
                    Point3d endPoint = startPoint + new Vector3d(line.Length + 2.0 * CommonModule.externalCorner, 0, 0);


                    datum_Cline_creation(startPoint, endPoint, false);

                    ln += CommonModule.externalCorner;

                    ln += 1000 + line.Length + CommonModule.externalCorner;
                }


                rm++;
            }


            // Get Internal wall lines
            for (int index = 0; index < InternalWallHelper.listOfListOfInternalWallRoomLineId_With_WallName.Count; index++)
            {
                List<ObjectId> listOfInternalWallLineId = InternalWallHelper.listOfListOfInternalWallRoomLineId_With_WallName[index];
                double ln = 0.0;



                for (int index1 = 0; index1 < listOfInternalWallLineId.Count; index1++)
                {

                    Line line = AEE_Utility.GetLine(listOfInternalWallLineId[index1]);
                    Point3d startPoint = BaseElevationPoint + new Vector3d(ln, -(rm * WallHeight + 500), 0);
                    Point3d endPoint = startPoint + new Vector3d(line.Length, 0, 0);

                    datum_Cline_creation(startPoint, endPoint, true);

                    ln += 1000 + line.Length;

                }


                rm++;
            }
        }

        private void elevation_ver_clien_creation()
        {
            for (int i = 0; i < CommonModule.vert_cline_data.Count; i++)
            {
                draw_vertical_Cline(CommonModule.vert_cline_data[i].startpoint, CommonModule.vert_cline_data[i].widh, CommonModule.vert_cline_data[i].ht);

                Point3d stpnt = new Point3d(0, 0, 0);
                Point3d endpnt = new Point3d(0, 0, 0);

                if (CommonModule.vert_cline_data[i].ht < PanelLayout_UI.int_wall_hgt)
                {
                    if (CommonModule.vert_cline_data[i].ht == 150)
                    {
                        stpnt = new Point3d((CommonModule.vert_cline_data[i].offsetstartpoint.X - 100), (CommonModule.vert_cline_data[i].offsetstartpoint.Y - 100), (CommonModule.vert_cline_data[i].offsetstartpoint.Z));
                        endpnt = new Point3d((CommonModule.vert_cline_data[i].offsetendpoint.X + 100), (CommonModule.vert_cline_data[i].offsetendpoint.Y - 100), (CommonModule.vert_cline_data[i].offsetendpoint.Z));

                        var less = lst_cline_dist.Aggregate((x, y) => Math.Abs(x.Y - stpnt.Y) < Math.Abs(y.Y - stpnt.Y) ? x : y);

                        bool isline = true;

                        if (((less.Y - 50) < stpnt.Y) && ((less.Y + 50) > stpnt.Y))
                        {
                            isline = false;
                        }

                        if (isline)
                        {
                            AEE_Utility.CreateLine(stpnt, endpnt, CommonModule.clineWallPanelLyrName_hor_SP, CommonModule.WallPanelLyrColor_sp);
                        }
                    }
                    else if (CommonModule.vert_cline_data[i].ht <= 450 && CommonModule.vert_cline_data[i].ht > 150)
                    {
                        stpnt = new Point3d((CommonModule.vert_cline_data[i].startpoint.X - 100), (CommonModule.vert_cline_data[i].startpoint.Y + PanelLayout_UI.f_dist), (CommonModule.vert_cline_data[i].startpoint.Z));
                        endpnt = new Point3d((CommonModule.vert_cline_data[i].endpoint.X + 100), (CommonModule.vert_cline_data[i].endpoint.Y + PanelLayout_UI.f_dist), (CommonModule.vert_cline_data[i].endpoint.Z));

                        var less = lst_cline_dist.Aggregate((x, y) => Math.Abs(x.Y - stpnt.Y) < Math.Abs(y.Y - stpnt.Y) ? x : y);

                        bool isline = true;

                        if (((less.Y - 50) < stpnt.Y) && ((less.Y + 50) > stpnt.Y))
                        {
                            isline = false;
                        }

                        if (isline)
                        {
                            AEE_Utility.CreateLine(stpnt, endpnt, CommonModule.clineWallPanelLyrName_hor_SP, CommonModule.WallPanelLyrColor_sp);
                        }

                        stpnt = new Point3d((CommonModule.vert_cline_data[i].offsetstartpoint.X - 100), (CommonModule.vert_cline_data[i].offsetstartpoint.Y - 100), (CommonModule.vert_cline_data[i].offsetstartpoint.Z));
                        endpnt = new Point3d((CommonModule.vert_cline_data[i].offsetendpoint.X + 100), (CommonModule.vert_cline_data[i].offsetendpoint.Y - 100), (CommonModule.vert_cline_data[i].offsetendpoint.Z));
                        less = lst_cline_dist.Aggregate((x, y) => Math.Abs(x.Y - stpnt.Y) < Math.Abs(y.Y - stpnt.Y) ? x : y);

                        isline = true;

                        if (((less.Y - 50) < stpnt.Y) && ((less.Y + 50) > stpnt.Y))
                        {
                            isline = false;
                        }

                        if (isline)
                        {
                            AEE_Utility.CreateLine(stpnt, endpnt, CommonModule.clineWallPanelLyrName_hor_SP, CommonModule.WallPanelLyrColor_sp);
                        }
                    }
                    else if (CommonModule.vert_cline_data[i].ht <= 600 && CommonModule.vert_cline_data[i].ht > 450)
                    {
                        stpnt = new Point3d((CommonModule.vert_cline_data[i].startpoint.X - 100), (CommonModule.vert_cline_data[i].startpoint.Y + PanelLayout_UI.f_dist), (CommonModule.vert_cline_data[i].startpoint.Z));
                        endpnt = new Point3d((CommonModule.vert_cline_data[i].endpoint.X + 100), (CommonModule.vert_cline_data[i].endpoint.Y + PanelLayout_UI.f_dist), (CommonModule.vert_cline_data[i].endpoint.Z));
                        var less = lst_cline_dist.Aggregate((x, y) => Math.Abs(x.Y - stpnt.Y) < Math.Abs(y.Y - stpnt.Y) ? x : y);

                        bool isline = true;

                        if (((less.Y - 50) < stpnt.Y) && ((less.Y + 50) > stpnt.Y))
                        {
                            isline = false;
                        }

                        if (isline)
                        {
                            AEE_Utility.CreateLine(stpnt, endpnt, CommonModule.clineWallPanelLyrName_hor_SP, CommonModule.WallPanelLyrColor_sp);
                        }

                        stpnt = new Point3d((CommonModule.vert_cline_data[i].startpoint.X - 100), (CommonModule.vert_cline_data[i].startpoint.Y + PanelLayout_UI.f_dist + PanelLayout_UI.sec_dist), (CommonModule.vert_cline_data[i].startpoint.Z));
                        endpnt = new Point3d((CommonModule.vert_cline_data[i].endpoint.X + 100), (CommonModule.vert_cline_data[i].endpoint.Y + PanelLayout_UI.f_dist + PanelLayout_UI.sec_dist), (CommonModule.vert_cline_data[i].endpoint.Z));
                        less = lst_cline_dist.Aggregate((x, y) => Math.Abs(x.Y - stpnt.Y) < Math.Abs(y.Y - stpnt.Y) ? x : y);

                        isline = true;

                        if (((less.Y - 50) < stpnt.Y) && ((less.Y + 50) > stpnt.Y))
                        {
                            isline = false;
                        }

                        if (isline)
                        {
                            AEE_Utility.CreateLine(stpnt, endpnt, CommonModule.clineWallPanelLyrName_hor_SP, CommonModule.WallPanelLyrColor_sp);
                        }


                        stpnt = new Point3d((CommonModule.vert_cline_data[i].offsetstartpoint.X - 100), (CommonModule.vert_cline_data[i].offsetstartpoint.Y - 100), (CommonModule.vert_cline_data[i].offsetstartpoint.Z));
                        endpnt = new Point3d((CommonModule.vert_cline_data[i].offsetendpoint.X + 100), (CommonModule.vert_cline_data[i].offsetendpoint.Y - 100), (CommonModule.vert_cline_data[i].offsetendpoint.Z));
                        less = lst_cline_dist.Aggregate((x, y) => Math.Abs(x.Y - stpnt.Y) < Math.Abs(y.Y - stpnt.Y) ? x : y);

                        isline = true;

                        if (((less.Y - 50) < stpnt.Y) && ((less.Y + 50) > stpnt.Y))
                        {
                            isline = false;
                        }

                        if (isline)
                        {
                            AEE_Utility.CreateLine(stpnt, endpnt, CommonModule.clineWallPanelLyrName_hor_SP, CommonModule.WallPanelLyrColor_sp);
                        }
                    }
                    else
                    {
                        stpnt = new Point3d((CommonModule.vert_cline_data[i].startpoint.X - 100), (CommonModule.vert_cline_data[i].startpoint.Y + PanelLayout_UI.f_dist), (CommonModule.vert_cline_data[i].startpoint.Z));
                        endpnt = new Point3d((CommonModule.vert_cline_data[i].endpoint.X + 100), (CommonModule.vert_cline_data[i].endpoint.Y + PanelLayout_UI.f_dist), (CommonModule.vert_cline_data[i].endpoint.Z));
                        var less = lst_cline_dist.Aggregate((x, y) => Math.Abs(x.Y - stpnt.Y) < Math.Abs(y.Y - stpnt.Y) ? x : y);

                        bool isline = true;

                        if (((less.Y - 50) < stpnt.Y) && ((less.Y + 50) > stpnt.Y))
                        {
                            isline = false;
                        }

                        if (isline)
                        {
                            AEE_Utility.CreateLine(stpnt, endpnt, CommonModule.clineWallPanelLyrName_hor_SP, CommonModule.WallPanelLyrColor_sp);
                        }

                        double line_count = (CommonModule.vert_cline_data[i].ht - PanelLayout_UI.f_dist) / PanelLayout_UI.sec_dist;
                        int max_line = Convert.ToInt32(Math.Floor(line_count));

                        double remian_dist = CommonModule.vert_cline_data[i].ht - ((PanelLayout_UI.sec_dist * max_line) + PanelLayout_UI.f_dist);
                        if (remian_dist <= 50)
                        {
                            max_line--;
                        }

                        for (int j = 0; j < max_line; j++)
                        {
                            stpnt = new Point3d((CommonModule.vert_cline_data[i].startpoint.X - 100), (CommonModule.vert_cline_data[i].startpoint.Y + PanelLayout_UI.f_dist + PanelLayout_UI.sec_dist * (j + 1)), (CommonModule.vert_cline_data[i].startpoint.Z));
                            endpnt = new Point3d((CommonModule.vert_cline_data[i].endpoint.X + 100), (CommonModule.vert_cline_data[i].endpoint.Y + PanelLayout_UI.f_dist + PanelLayout_UI.sec_dist * (j + 1)), (CommonModule.vert_cline_data[i].endpoint.Z));
                            less = lst_cline_dist.Aggregate((x, y) => Math.Abs(x.Y - stpnt.Y) < Math.Abs(y.Y - stpnt.Y) ? x : y);

                            isline = true;

                            if (((less.Y - 50) < stpnt.Y) && ((less.Y + 50) > stpnt.Y))
                            {
                                isline = false;
                            }

                            if (isline)
                            {
                                AEE_Utility.CreateLine(stpnt, endpnt, CommonModule.clineWallPanelLyrName_hor_SP, CommonModule.WallPanelLyrColor_sp);
                            }
                        }

                        if (remian_dist > 200)
                        {
                            stpnt = new Point3d((CommonModule.vert_cline_data[i].offsetstartpoint.X - 100), (CommonModule.vert_cline_data[i].offsetstartpoint.Y - 100), (CommonModule.vert_cline_data[i].offsetstartpoint.Z));
                            endpnt = new Point3d((CommonModule.vert_cline_data[i].offsetendpoint.X + 100), (CommonModule.vert_cline_data[i].offsetendpoint.Y - 100), (CommonModule.vert_cline_data[i].offsetendpoint.Z));
                            less = lst_cline_dist.Aggregate((x, y) => Math.Abs(x.Y - stpnt.Y) < Math.Abs(y.Y - stpnt.Y) ? x : y);

                            isline = true;

                            if (((less.Y - 50) < stpnt.Y) && ((less.Y + 50) > stpnt.Y))
                            {
                                isline = false;
                            }

                            if (isline)
                            {
                                AEE_Utility.CreateLine(stpnt, endpnt, CommonModule.clineWallPanelLyrName_hor_SP, CommonModule.WallPanelLyrColor_sp);
                            }
                        }
                        else if (remian_dist <= 50)
                        {
                            stpnt = new Point3d((CommonModule.vert_cline_data[i].offsetstartpoint.X - 100), (CommonModule.vert_cline_data[i].offsetstartpoint.Y - 100), (CommonModule.vert_cline_data[i].offsetstartpoint.Z));
                            endpnt = new Point3d((CommonModule.vert_cline_data[i].offsetendpoint.X + 100), (CommonModule.vert_cline_data[i].offsetendpoint.Y - 100), (CommonModule.vert_cline_data[i].offsetendpoint.Z));
                            less = lst_cline_dist.Aggregate((x, y) => Math.Abs(x.Y - stpnt.Y) < Math.Abs(y.Y - stpnt.Y) ? x : y);

                            isline = true;

                            if (((less.Y - 50) < stpnt.Y) && ((less.Y + 50) > stpnt.Y))
                            {
                                isline = false;
                            }

                            if (isline)
                            {
                                AEE_Utility.CreateLine(stpnt, endpnt, CommonModule.clineWallPanelLyrName_hor_SP, CommonModule.WallPanelLyrColor_sp);
                            }
                        }
                    }

                }

            }
        }

        List<double> kickerpnts = new List<double>();
        List<double> SJpnts = new List<double>();



        private void LCX_CCX_lince_creation()
        {
            for (int i = 0; i < CommonModule.LCX_CCX_corner_CLine.Count; i++)
            {
                Point3d startpoint1 = CommonModule.LCX_CCX_corner_CLine[i].startpoint1;
                Point3d endpoint1 = CommonModule.LCX_CCX_corner_CLine[i].endpoint1;
                Point3d startpoint2 = CommonModule.LCX_CCX_corner_CLine[i].startpoint2;
                Point3d endpoint2 = CommonModule.LCX_CCX_corner_CLine[i].endpoint2;
                double angle = CommonModule.LCX_CCX_corner_CLine[i].angle;
                Vector3d bottommove = CommonModule.LCX_CCX_corner_CLine[i].bottommove;
                Vector3d topmove = CommonModule.LCX_CCX_corner_CLine[i].topmove;
                string panelname = CommonModule.LCX_CCX_corner_CLine[i].panellayername;


                if (angle == 90 || angle == 270)
                {
                    List<ObjectId> objids = new List<ObjectId>();

                    if (true)
                    {
                        Point3d STpoint1 = startpoint1;
                        Point3d STpoint2 = startpoint2;

                        upp:
                        double dist = STpoint1.Y - STpoint2.Y;
                        bool isneg = false;
                        if (dist <= 50)
                        {
                            dist = Math.Abs(STpoint2.Y - STpoint1.Y);
                            isneg = true;
                        }

                        if (dist > 50)
                        {
                            if (isneg == false)
                            {
                                Point3d stpnt = new Point3d(STpoint1.X - 25 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint1.Y - 50, STpoint1.Z);
                                Point3d endpoint = new Point3d(STpoint1.X + 25 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint1.Y - 50, STpoint1.Z);

                                dist = stpnt.Y - STpoint2.Y;
                                if (dist <= 50)
                                {
                                    dist = Math.Abs(STpoint2.Y - stpnt.Y);
                                }
                                if (dist >= 50)
                                {
                                    ObjectId objid = AEE_Utility.CreateLine(stpnt, endpoint, panelname, CommonModule.eleWallPanelLyrColor);
                                    objids.Add(objid);
                                    STpoint1 = new Point3d(STpoint1.X, STpoint1.Y - 50, STpoint1.Z);
                                }
                            }
                            else
                            {
                                Point3d stpnt = new Point3d(STpoint1.X - 25 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint1.Y + 50, STpoint1.Z);
                                Point3d endpoint = new Point3d(STpoint1.X + 25 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint1.Y + 50, STpoint1.Z);

                                dist = stpnt.Y - STpoint2.Y;
                                if (dist <= 50)
                                {
                                    dist = Math.Abs(STpoint2.Y - stpnt.Y);
                                }
                                if (dist >= 50)
                                {
                                    ObjectId objid = AEE_Utility.CreateLine(stpnt, endpoint, panelname, CommonModule.eleWallPanelLyrColor);
                                    objids.Add(objid);
                                    STpoint1 = new Point3d(STpoint1.X, STpoint1.Y + 50, STpoint1.Z);
                                }
                            }
                        }

                        if (dist > 50)
                        {

                            if (isneg == false)
                            {
                                Point3d stpnt = new Point3d(STpoint1.X - 25 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint2.Y + 50, STpoint2.Z);
                                Point3d endpoint = new Point3d(STpoint1.X + 25 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint2.Y + 50, STpoint2.Z);
                                dist = stpnt.Y - STpoint2.Y;
                                if (dist <= 50)
                                {
                                    dist = Math.Abs(STpoint2.Y - stpnt.Y);
                                }
                                if (dist >= 50)
                                {
                                    ObjectId objid = AEE_Utility.CreateLine(stpnt, endpoint, panelname, CommonModule.eleWallPanelLyrColor);
                                    objids.Add(objid);
                                    STpoint2 = new Point3d(STpoint1.X, STpoint2.Y + 50, STpoint2.Z);
                                    goto upp;
                                }
                            }
                            else
                            {
                                Point3d stpnt = new Point3d(STpoint1.X - 25 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint2.Y - 50, STpoint2.Z);
                                Point3d endpoint = new Point3d(STpoint1.X + 25 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint2.Y - 50, STpoint2.Z);
                                dist = stpnt.Y - STpoint2.Y;
                                if (dist <= 50)
                                {
                                    dist = Math.Abs(STpoint2.Y - stpnt.Y);
                                }
                                if (dist >= 50)
                                {
                                    ObjectId objid = AEE_Utility.CreateLine(stpnt, endpoint, panelname, CommonModule.eleWallPanelLyrColor);
                                    objids.Add(objid);
                                    STpoint2 = new Point3d(STpoint1.X, STpoint2.Y - 50, STpoint2.Z);
                                    goto upp;
                                }
                            }
                        }
                    }
                    if (true)
                    {
                        Point3d STpoint1 = endpoint1;
                        Point3d STpoint2 = endpoint2;

                        upp:
                        double dist = STpoint1.Y - STpoint2.Y;
                        bool isneg = false;
                        if (dist <= 50)
                        {
                            dist = Math.Abs(STpoint2.Y - STpoint1.Y);
                            isneg = true;
                        }
                        if (dist > 50)
                        {
                            if (isneg == false)
                            {
                                Point3d stpnt = new Point3d(STpoint1.X - 25 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint1.Y - 50, STpoint1.Z);
                                Point3d endpoint = new Point3d(STpoint1.X + 25 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint1.Y - 50, STpoint1.Z);
                                dist = stpnt.Y - STpoint2.Y;
                                if (dist <= 50)
                                {
                                    dist = Math.Abs(STpoint2.Y - stpnt.Y);
                                }
                                if (dist >= 50)
                                {
                                    ObjectId objid = AEE_Utility.CreateLine(stpnt, endpoint, panelname, CommonModule.eleWallPanelLyrColor);
                                    objids.Add(objid);
                                    STpoint1 = new Point3d(STpoint1.X, STpoint1.Y - 50, STpoint1.Z);
                                }
                            }
                            else
                            {
                                Point3d stpnt = new Point3d(STpoint1.X - 25 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint1.Y + 50, STpoint1.Z);
                                Point3d endpoint = new Point3d(STpoint1.X + 25 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint1.Y + 50, STpoint1.Z);
                                dist = stpnt.Y - STpoint2.Y;
                                if (dist <= 50)
                                {
                                    dist = Math.Abs(STpoint2.Y - stpnt.Y);
                                }
                                if (dist >= 50)
                                {
                                    ObjectId objid = AEE_Utility.CreateLine(stpnt, endpoint, panelname, CommonModule.eleWallPanelLyrColor);
                                    objids.Add(objid);
                                    STpoint1 = new Point3d(STpoint1.X, STpoint1.Y + 50, STpoint1.Z);
                                }
                            }
                        }

                        if (dist > 50)
                        {
                            if (isneg == false)
                            {
                                Point3d stpnt = new Point3d(STpoint1.X - 25 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint2.Y + 50, STpoint2.Z);
                                Point3d endpoint = new Point3d(STpoint1.X + 25 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint2.Y + 50, STpoint2.Z);
                                dist = stpnt.Y - STpoint2.Y;
                                if (dist <= 50)
                                {
                                    dist = Math.Abs(STpoint2.Y - stpnt.Y);
                                }
                                if (dist >= 50)
                                {
                                    ObjectId objid = AEE_Utility.CreateLine(stpnt, endpoint, panelname, CommonModule.eleWallPanelLyrColor);
                                    objids.Add(objid);
                                    STpoint2 = new Point3d(STpoint1.X, STpoint2.Y + 50, STpoint2.Z);
                                    goto upp;
                                }
                            }
                            else
                            {
                                Point3d stpnt = new Point3d(STpoint1.X - 25 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint2.Y - 50, STpoint2.Z);
                                Point3d endpoint = new Point3d(STpoint1.X + 25 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint2.Y - 50, STpoint2.Z);
                                dist = stpnt.Y - STpoint2.Y;
                                if (dist <= 50)
                                {
                                    dist = Math.Abs(STpoint2.Y - stpnt.Y);
                                }
                                if (dist >= 50)
                                {
                                    ObjectId objid = AEE_Utility.CreateLine(stpnt, endpoint, panelname, CommonModule.eleWallPanelLyrColor);
                                    objids.Add(objid);
                                    STpoint2 = new Point3d(STpoint1.X, STpoint2.Y - 50, STpoint2.Z);
                                    goto upp;
                                }
                            }
                        }
                       
                    }
                    if (CommonModule.LCX_CCX_corner_CLine[i].panelype == "Window Wall Panel")
                    {
                        AEE_Utility.MoveEntity(objids, bottommove);
                    }

                    if (CommonModule.LCX_CCX_corner_CLine[i].panelype.ToLower().Contains("door") || panelname.ToLower().Contains("ccx"))
                    {
                        AEE_Utility.MoveEntity(objids, topmove);

                        if (panelname.ToLower().Contains("ccx") && CommonModule.LCX_CCX_corner_CLine[i].panelype == "Window Wall Panel")
                        {
                            AEE_Utility.MoveEntity(objids, new Vector3d(0, -150, 0));
                        }

                    }
                }
                else
                {
                    List<ObjectId> objids = new List<ObjectId>();

                    if (true)
                    {
                        Point3d STpoint1 = startpoint1;
                        Point3d STpoint2 = startpoint2;

                        upp:
                        double dist = STpoint1.X - STpoint2.X;
                        bool isneg = false;
                        if (dist <= 50)
                        {
                            dist = Math.Abs(STpoint2.X - STpoint1.X);
                            isneg = true;
                        }

                        if (dist > 50)
                        {
                            if (isneg == false)
                            {
                                Point3d stpnt = new Point3d(STpoint1.X - 50 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint1.Y - 25, STpoint1.Z);
                                Point3d endpoint = new Point3d(STpoint1.X - 50 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint1.Y + 25, STpoint1.Z);

                                dist = stpnt.X - (STpoint2.X + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X);
                                if (dist <= 50)
                                {
                                    dist = Math.Abs((STpoint2.X + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X) - stpnt.X);
                                }
                                if (dist >= 50)
                                {
                                    ObjectId objid = AEE_Utility.CreateLine(stpnt, endpoint, panelname, CommonModule.eleWallPanelLyrColor);
                                    objids.Add(objid);
                                    STpoint1 = new Point3d(STpoint1.X - 50, STpoint1.Y, STpoint1.Z);
                                }
                            }
                            else
                            {
                                Point3d stpnt = new Point3d(STpoint1.X + 50 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint1.Y - 25, STpoint1.Z);
                                Point3d endpoint = new Point3d(STpoint1.X + 50 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint1.Y + 25, STpoint1.Z);

                                dist = stpnt.X - (STpoint2.X + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X);
                                if (dist <= 50)
                                {
                                    dist = Math.Abs((STpoint2.X + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X) - stpnt.X);
                                }
                                if (dist >= 50)
                                {
                                    ObjectId objid = AEE_Utility.CreateLine(stpnt, endpoint, panelname, CommonModule.eleWallPanelLyrColor);
                                    objids.Add(objid);
                                    STpoint1 = new Point3d(STpoint1.X + 50, STpoint1.Y, STpoint1.Z);
                                }
                            }
                        }

                        if (dist > 50)
                        {

                            if (isneg == false)
                            {
                                Point3d stpnt = new Point3d(STpoint2.X + 50 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint2.Y - 25, STpoint2.Z);
                                Point3d endpoint = new Point3d(STpoint2.X + 50 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint2.Y + 25, STpoint2.Z);

                                dist = stpnt.X - (STpoint2.X + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X);
                                if (dist <= 50)
                                {
                                    dist = Math.Abs((STpoint2.X + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X) - stpnt.X);
                                }
                                if (dist >= 50)
                                {
                                    ObjectId objid = AEE_Utility.CreateLine(stpnt, endpoint, panelname, CommonModule.eleWallPanelLyrColor);
                                    objids.Add(objid);
                                    STpoint2 = new Point3d(STpoint2.X + 50, STpoint1.Y, STpoint2.Z);
                                    goto upp;
                                }
                            }
                            else
                            {
                                Point3d stpnt = new Point3d(STpoint2.X - 50 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint2.Y - 25, STpoint2.Z);
                                Point3d endpoint = new Point3d(STpoint2.X - 50 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint2.Y + 25, STpoint2.Z);

                                dist = stpnt.X - (STpoint2.X + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X);
                                if (dist <= 50)
                                {
                                    dist = Math.Abs((STpoint2.X + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X) - stpnt.X);
                                }
                                if (dist >= 50)
                                {
                                    ObjectId objid = AEE_Utility.CreateLine(stpnt, endpoint, panelname, CommonModule.eleWallPanelLyrColor);
                                    objids.Add(objid);
                                    STpoint2 = new Point3d(STpoint2.X - 50, STpoint1.Y, STpoint2.Z);
                                    goto upp;
                                }
                            }
                        }
                    }
                    if (true)
                    {
                        Point3d STpoint1 = endpoint1;
                        Point3d STpoint2 = endpoint2;

                        upp:
                        double dist = STpoint1.X - STpoint2.X;
                        bool isneg = false;
                        if (dist <= 50)
                        {
                            dist = Math.Abs(STpoint2.X - STpoint1.X);
                            isneg = true;
                        }
                        if (dist > 50)
                        {
                            if (isneg == false)
                            {
                                Point3d stpnt = new Point3d(STpoint1.X - 50 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint1.Y - 25, STpoint1.Z);
                                Point3d endpoint = new Point3d(STpoint1.X - 50 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint1.Y + 25, STpoint1.Z);

                                dist = stpnt.X - (STpoint2.X + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X);
                                if (dist <= 50)
                                {
                                    dist = Math.Abs((STpoint2.X + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X) - stpnt.X);
                                }
                                if (dist >= 50)
                                {
                                    ObjectId objid = AEE_Utility.CreateLine(stpnt, endpoint, panelname, CommonModule.eleWallPanelLyrColor);
                                    objids.Add(objid);
                                    STpoint1 = new Point3d(STpoint1.X - 50, STpoint1.Y, STpoint1.Z);
                                }
                            }
                            else
                            {
                                Point3d stpnt = new Point3d(STpoint1.X + 50 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint1.Y - 25, STpoint1.Z);
                                Point3d endpoint = new Point3d(STpoint1.X + 50 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint1.Y + 25, STpoint1.Z);

                                dist = stpnt.X - (STpoint2.X + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X);
                                if (dist <= 50)
                                {
                                    dist = Math.Abs((STpoint2.X + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X) - stpnt.X);
                                }
                                if (dist >= 50)
                                {
                                    ObjectId objid = AEE_Utility.CreateLine(stpnt, endpoint, panelname, CommonModule.eleWallPanelLyrColor);
                                    objids.Add(objid);
                                    STpoint1 = new Point3d(STpoint1.X + 50, STpoint1.Y, STpoint1.Z);
                                }
                            }
                        }

                        if (dist > 50)
                        {

                            if (isneg == false)
                            {

                                Point3d stpnt = new Point3d(STpoint2.X + 50 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint2.Y - 25, STpoint2.Z);
                                Point3d endpoint = new Point3d(STpoint2.X + 50 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint2.Y + 25, STpoint2.Z);

                                dist = stpnt.X - (STpoint2.X + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X);
                                if (dist <= 50)
                                {
                                    dist = Math.Abs((STpoint2.X + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X) - stpnt.X);
                                }
                                if (dist >= 50)
                                {
                                    ObjectId objid = AEE_Utility.CreateLine(stpnt, endpoint, panelname, CommonModule.eleWallPanelLyrColor);
                                    objids.Add(objid);
                                    STpoint2 = new Point3d(STpoint2.X + 50, STpoint1.Y, STpoint2.Z);
                                    goto upp;
                                }
                            }
                            else
                            {
                                Point3d stpnt = new Point3d(STpoint2.X - 50 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint2.Y - 25, STpoint2.Z);
                                Point3d endpoint = new Point3d(STpoint2.X - 50 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint2.Y + 25, STpoint2.Z);

                                dist = stpnt.X - (STpoint2.X + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X);
                                if (dist <= 50)
                                {
                                    dist = Math.Abs((STpoint2.X + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X) - stpnt.X);
                                }
                                if (dist >= 50)
                                {
                                    ObjectId objid = AEE_Utility.CreateLine(stpnt, endpoint, panelname, CommonModule.eleWallPanelLyrColor);
                                    objids.Add(objid);
                                    STpoint2 = new Point3d(STpoint2.X - 50, STpoint1.Y, STpoint2.Z);
                                    goto upp;
                                }
                            }
                        }
                    }
                    if (CommonModule.LCX_CCX_corner_CLine[i].panelype == "Window Wall Panel")
                    {
                        AEE_Utility.MoveEntity(objids, bottommove);
                    }

                    if (CommonModule.LCX_CCX_corner_CLine[i].panelype.ToLower().Contains("door") || panelname.ToLower().Contains("ccx"))
                    {
                        AEE_Utility.MoveEntity(objids, topmove);
                        if (panelname.ToLower().Contains("ccx")  && CommonModule.LCX_CCX_corner_CLine[i].panelype == "Window Wall Panel")
                        {
                            AEE_Utility.MoveEntity(objids, new Vector3d(-150, 0, 0));
                        }
                    }
                }



                //for (int j = 0; j < CommonModule.LCX_CCX_corner_CLine[i].LCX_CCX_mid_CLine.Count; j++)
                //{
                //    if (angle == 270)
                //    {
                //        if (Math.Round(CommonModule.LCX_CCX_corner_CLine[i].LCX_CCX_mid_CLine[j].startpoint1.X, 4) == Math.Round(endpoint1.X, 4))
                //        {
                //            Point3d STpoint1 = CommonModule.LCX_CCX_corner_CLine[i].LCX_CCX_mid_CLine[j].startpoint1;
                //            if ((CommonModule.LCX_CCX_corner_CLine[i].LCX_CCX_mid_CLine[j].endpoint1.X == startpoint2.X))
                //            {
                //                STpoint1 = CommonModule.LCX_CCX_corner_CLine[i].LCX_CCX_mid_CLine[j].endpoint1;
                //            }
                //            Point3d STpoint2 = CommonModule.LCX_CCX_corner_CLine[i].LCX_CCX_mid_CLine[j].startpoint2;

                //            upp:
                //            double dist = STpoint1.Y - STpoint2.Y;
                //            if (dist <= 50)
                //            {
                //                dist = STpoint2.Y - STpoint1.Y;
                //                if (dist <= 50)
                //                {
                //                    continue;
                //                }

                //            }

                //            Point3d stpnt = new Point3d(STpoint1.X - 25 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint1.Y - 50, STpoint1.Z);
                //            Point3d endpoint = new Point3d(STpoint1.X + 25 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint1.Y - 50, STpoint1.Z);
                //            ObjectId objid = AEE_Utility.CreateLine(stpnt, endpoint, CommonModule.LCXPanelWallLayerName, CommonModule.eleWallPanelLyrColor);
                //            objids.Add(objid);
                //            STpoint1 = new Point3d(STpoint1.X, STpoint1.Y - 50, STpoint1.Z);


                //            stpnt = new Point3d(STpoint1.X - 25 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint2.Y + 50, STpoint2.Z);
                //            endpoint = new Point3d(STpoint1.X + 25 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint2.Y + 50, STpoint2.Z);
                //            objid = AEE_Utility.CreateLine(stpnt, endpoint, CommonModule.LCXPanelWallLayerName, CommonModule.eleWallPanelLyrColor);
                //            objids.Add(objid);
                //            STpoint2 = new Point3d(STpoint1.X, STpoint2.Y + 50, STpoint2.Z);
                //            goto upp;
                //        }

                //        if (Math.Round(CommonModule.LCX_CCX_corner_CLine[i].LCX_CCX_mid_CLine[j].endpoint1.X, 4) == Math.Round(startpoint2.X, 4))
                //        {
                //            Point3d STpoint1 = CommonModule.LCX_CCX_corner_CLine[i].LCX_CCX_mid_CLine[j].endpoint1;
                //            if ((CommonModule.LCX_CCX_corner_CLine[i].LCX_CCX_mid_CLine[j].endpoint1.X == startpoint2.X))
                //            {
                //                STpoint1 = CommonModule.LCX_CCX_corner_CLine[i].LCX_CCX_mid_CLine[j].endpoint1;
                //            }
                //            Point3d STpoint2 = CommonModule.LCX_CCX_corner_CLine[i].LCX_CCX_mid_CLine[j].startpoint2;

                //            upp:
                //            double dist = STpoint1.Y - STpoint2.Y;
                //            if (dist <= 50)
                //            {
                //                dist = STpoint2.Y - STpoint1.Y;
                //                if (dist <= 50)
                //                {
                //                    continue;
                //                }

                //            }

                //            Point3d stpnt = new Point3d(STpoint1.X - 25 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint1.Y - 50, STpoint1.Z);
                //            Point3d endpoint = new Point3d(STpoint1.X + 25 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint1.Y - 50, STpoint1.Z);
                //            ObjectId objid = AEE_Utility.CreateLine(stpnt, endpoint, CommonModule.LCXPanelWallLayerName, CommonModule.eleWallPanelLyrColor);
                //            objids.Add(objid);
                //            STpoint1 = new Point3d(STpoint1.X, STpoint1.Y - 50, STpoint1.Z);


                //            stpnt = new Point3d(STpoint1.X - 25 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint2.Y + 50, STpoint2.Z);
                //            endpoint = new Point3d(STpoint1.X + 25 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint2.Y + 50, STpoint2.Z);
                //            objid = AEE_Utility.CreateLine(stpnt, endpoint, CommonModule.LCXPanelWallLayerName, CommonModule.eleWallPanelLyrColor);
                //            objids.Add(objid);
                //            STpoint2 = new Point3d(STpoint1.X, STpoint2.Y + 50, STpoint2.Z);
                //            goto upp;
                //        }
                //    }
                //}

                //if (panelname.ToLower().Contains("ccx"))
                //{
                //    AEE_Utility.MoveEntity(objids, topmove);
                //}




                //if (angle == 90 || angle == 270)
                //{
                //    for (int j = 0; j < CommonModule.LCX_CCX_corner_CLine[i].LCX_CCX_mid_CLine.Count; j++)
                //    {

                //        if ((CommonModule.LCX_CCX_corner_CLine[i].LCX_CCX_mid_CLine[j].startpoint1.X == endpoint1.X) || (CommonModule.LCX_CCX_corner_CLine[i].LCX_CCX_mid_CLine[j].endpoint1.X == startpoint2.X))
                //        {
                //            Point3d STpoint1 = CommonModule.LCX_CCX_corner_CLine[i].LCX_CCX_mid_CLine[j].startpoint1;
                //            if ((CommonModule.LCX_CCX_corner_CLine[i].LCX_CCX_mid_CLine[j].endpoint1.X == startpoint2.X))
                //            {
                //                STpoint1 = CommonModule.LCX_CCX_corner_CLine[i].LCX_CCX_mid_CLine[j].endpoint1;
                //            }
                //            Point3d STpoint2 = CommonModule.LCX_CCX_corner_CLine[i].LCX_CCX_mid_CLine[j].startpoint2;

                //            upp:
                //            double dist = STpoint1.Y - STpoint2.Y;
                //            if (dist <= 50 )
                //            {
                //                dist = STpoint2.Y - STpoint1.Y;
                //                if (dist <= 50)
                //                {
                //                    continue;
                //                }

                //            }

                //            Point3d stpnt = new Point3d(STpoint1.X-25 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint1.Y - 50 + CommonModule.LCX_CCX_corner_CLine[i].moveangle, STpoint1.Z);
                //            Point3d endpoint = new Point3d(STpoint1.X + 25 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint1.Y - 50+ CommonModule.LCX_CCX_corner_CLine[i].moveangle, STpoint1.Z);
                //            AEE_Utility.CreateLine(stpnt, endpoint, CommonModule.LCXPanelWallLayerName, CommonModule.eleWallPanelLyrColor);
                //            STpoint1 = new Point3d(STpoint1.X, STpoint1.Y - 50, STpoint1.Z);


                //            stpnt = new Point3d(STpoint1.X - 25 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint2.Y + 50+ CommonModule.LCX_CCX_corner_CLine[i].moveangle, STpoint2.Z);
                //            endpoint = new Point3d(STpoint1.X + 25 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint2.Y + 50+ CommonModule.LCX_CCX_corner_CLine[i].moveangle, STpoint2.Z);
                //            AEE_Utility.CreateLine(stpnt, endpoint, CommonModule.LCXPanelWallLayerName, CommonModule.eleWallPanelLyrColor);
                //            STpoint2 = new Point3d(STpoint1.X, STpoint2.Y + 50, STpoint2.Z);
                //            goto upp;

                //        }
                //    }
                //}
                //else
                //{
                //    for (int j = 0; j < CommonModule.LCX_CCX_corner_CLine[i].LCX_CCX_mid_CLine.Count; j++)
                //    {
                //        if ((CommonModule.LCX_CCX_corner_CLine[i].LCX_CCX_mid_CLine[j].startpoint1.Y == endpoint1.Y) || (CommonModule.LCX_CCX_corner_CLine[i].LCX_CCX_mid_CLine[j].endpoint1.Y == startpoint2.Y))
                //        {
                //            Point3d STpoint1 = CommonModule.LCX_CCX_corner_CLine[i].LCX_CCX_mid_CLine[j].startpoint1;
                //            if ((CommonModule.LCX_CCX_corner_CLine[i].LCX_CCX_mid_CLine[j].endpoint1.Y == startpoint2.Y))
                //            {
                //                STpoint1 = CommonModule.LCX_CCX_corner_CLine[i].LCX_CCX_mid_CLine[j].endpoint1;
                //            }
                //            Point3d STpoint2 = CommonModule.LCX_CCX_corner_CLine[i].LCX_CCX_mid_CLine[j].endpoint1;

                //            upp:
                //            double dist = STpoint1.X - STpoint2.X;
                //            if (dist <= 50 )
                //            {
                //                dist = STpoint2.X - STpoint1.X;
                //                if (dist <= 50)
                //                {
                //                    continue;
                //                }
                //            }

                //            Point3d stpnt = new Point3d(STpoint1.X + 50 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint1.Y - 25, STpoint1.Z);
                //            Point3d endpoint = new Point3d(STpoint1.X + 50 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint1.Y + 25, STpoint1.Z);
                //            AEE_Utility.CreateLine(stpnt, endpoint, CommonModule.LCXPanelWallLayerName, CommonModule.eleWallPanelLyrColor);
                //            STpoint1 = new Point3d(STpoint1.X + 50, STpoint1.Y, STpoint1.Z);


                //            stpnt = new Point3d(STpoint2.X - 50 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint2.Y - 25, STpoint2.Z);
                //            endpoint = new Point3d(STpoint2.X - 50 + CreateShellPlanHelper.moveVector_ForWindowDoorLayout.X, STpoint2.Y + 25, STpoint2.Z);
                //            AEE_Utility.CreateLine(stpnt, endpoint, CommonModule.LCXPanelWallLayerName, CommonModule.eleWallPanelLyrColor);
                //            STpoint2 = new Point3d(STpoint2.X - 50, STpoint2.Y, STpoint2.Z);
                //            goto upp;

                //        }
                //    }
                //}

            }
        }
        public void createCline()
        {
            WallLines = new Dictionary<string, Tuple<Line, double, int, string>>();
            lst_cline_dist = new List<Point3d>();

            try
            {

                //Task task1 = new Task(elevation_hor_clien_creation); task1.Start();
                //Task task2 = new Task(kickerpanel_CLine_Creation); task2.Start();
                //Task task3 = new Task(deckpanel_CLine_Creation); task3.Start();
                //Task task4 = new Task(elevation_ver_clien_creation); task4.Start();

                elevation_hor_clien_creation();
                elevation_ver_clien_creation();

                deckpanel_CLine_Creation();
                kickerpnts = new List<double>();
                SJpnts = new List<double>();

                kickerpanel_CLine_Creation();
                kickerpanel_corner_cline_creation();

                slpjoint_cline_creation();
                slapjoint_corner_cline_creation();

                LCX_CCX_lince_creation();

            }
            catch (Exception ex)
            {

            }
        }
    }
}
