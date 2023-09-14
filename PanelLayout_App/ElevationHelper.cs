using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//namespace PanelLayout_App
namespace PanelLayout_App.WallModule
{
    class ElevationHelper
    {
        private Point3d mBasePoint;
        private const string beamCornerItemCode = " CC";

        public void CreateElevation(/*ProgressForm progressForm, string progressbarMsg*/)
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            PromptPointResult pr = ed.GetPoint("\n\n\n\nEnter elevation plan point:  ");
            if (pr.Status == PromptStatus.OK)
            {
                mBasePoint = pr.Value;
                RibbonHelper.createElevationPlanButton.IsEnabled = false;
                ElevationOfWall();
                RibbonHelper.rebuildPanelButton.IsEnabled = true;
            }
        }

        private void getDWTopDetails(ObjectId panelLineId, out int[] szs)
        {
            string sz = AEE_Utility.GetXDataRegisterAppName(panelLineId, 4);

            string[] array = sz.Split(',');
            szs = new int[array.Length];

            for (int i = 0; i < array.Length; i++)
                szs[i] = int.Parse(array[i]);
        }
       
        public static bool getWallDetails(string name, out int wallId, out int roomId, out bool intr, out bool ext, out bool lift, out bool stair)
        {
            string[] array = name.Split('_');
            bool ret = false;

            if (array.Length == 3)
            {
                wallId = Convert.ToInt32(array.GetValue(0).ToString().Substring(1));
                string roomText = Convert.ToString(array.GetValue(1));
                roomId = Convert.ToInt32(array.GetValue(2));
                intr = (roomText == "R");
                ext = (roomText == "EX");
                lift = (roomText == "LF");
                stair = (roomText == "ST");
                ret = true;
            }
            else
            {
                wallId = -1;
                roomId = -1;
                ext = false;
                intr = false;
                lift = false;
                stair = false;
            }

            return ret;
        }


        private void getWallDetails(ObjectId panelLineId, out string name, out int wallId, out int roomId, out bool ext)
        {
            bool intr, lift,stair;
            name = AEE_Utility.GetXDataRegisterAppName(panelLineId);
            getWallDetails(name, out wallId, out roomId,out intr, out ext,out lift,out stair);
        }


        private Dictionary<string, Tuple<Line, double, int, string>> GetWallLenghts(double dWallHt)
        {
            Dictionary<string, Tuple<Line, double, int, string>> mEleData = new Dictionary<string, Tuple<Line, double, int, string>>();
            int rm = 0;
            double SecTxtSz = 100;

            // Get External wall lines
            for (int index = 0; index < ExternalWallHelper.listOfListOfExternalWallRoomLineId_With_WallName.Count; index++)
            {
                List<ObjectId> listOfExternalWallLineId = ExternalWallHelper.listOfListOfExternalWallRoomLineId_With_WallName[index];
                double ln = 0.0;
                string zKey = AEE_Utility.GetXDataRegisterAppName(listOfExternalWallLineId[0]);
                string xDataRegAppName = zKey;

                for (int index1 = 0; index1 < listOfExternalWallLineId.Count; index1++)
                {
                    xDataRegAppName = AEE_Utility.GetXDataRegisterAppName(listOfExternalWallLineId[index1]);
                    Line line = AEE_Utility.GetLine(listOfExternalWallLineId[index1]);
                    Point3d startPoint = mBasePoint + new Vector3d(ln, -(rm * dWallHt + 500), 0);
                    Point3d endPoint = startPoint + new Vector3d(line.Length + 2.0 * CommonModule.externalCorner, 0, 0);

                    AEE_Utility.CreateLine(startPoint, endPoint, CommonModule.eleWallPanelLyrName, CommonModule.eleWallPanelLyrColor);
                    AEE_Utility.CreateTextWithAngle(xDataRegAppName, (startPoint.X + endPoint.X) / 2.0, endPoint.Y - SecTxtSz, endPoint.Z, SecTxtSz,
                                                    CommonModule.eleWallPanelLyrName, CommonModule.eleWallPanelLyrColor, 0);
                    ln += CommonModule.externalCorner;
                    mEleData.Add(xDataRegAppName, new Tuple<Line, double, int, string>(line, ln, rm, zKey));
                    ln += 1000 + line.Length + CommonModule.externalCorner;
                }

                Tuple<Line, double, int, string> t = mEleData[zKey];
                mEleData[zKey] = new Tuple<Line, double, int, string>(t.Item1, t.Item2, t.Item3, xDataRegAppName);
                rm++;
            }

            // Get Internal wall lines
            for (int index = 0; index < InternalWallHelper.listOfListOfInternalWallRoomLineId_With_WallName.Count; index++)
            {
                List<ObjectId> listOfInternalWallLineId = InternalWallHelper.listOfListOfInternalWallRoomLineId_With_WallName[index];
                double ln = 0.0;
                string zKey = AEE_Utility.GetXDataRegisterAppName(listOfInternalWallLineId[0]);
                string xDataRegAppName = zKey;

                for (int index1 = 0; index1 < listOfInternalWallLineId.Count; index1++)
                {
                    xDataRegAppName = AEE_Utility.GetXDataRegisterAppName(listOfInternalWallLineId[index1]);
                    Line line = AEE_Utility.GetLine(listOfInternalWallLineId[index1]);
                    mEleData.Add(xDataRegAppName, new Tuple<Line, double, int, string>(line, ln, rm, zKey));
                    Point3d startPoint = mBasePoint + new Vector3d(ln, -(rm * dWallHt + 500), 0);
                    Point3d endPoint = startPoint + new Vector3d(line.Length, 0, 0);

                    AEE_Utility.CreateLine(startPoint, endPoint, CommonModule.eleWallPanelLyrName, CommonModule.eleWallPanelLyrColor);
                    AEE_Utility.CreateTextWithAngle(xDataRegAppName, (startPoint.X + endPoint.X) / 2.0, endPoint.Y - SecTxtSz, endPoint.Z, SecTxtSz,
                                                    CommonModule.eleWallPanelLyrName, CommonModule.eleWallPanelLyrColor, 0);
                    ln += 1000 + line.Length;
                }

                Tuple<Line, double, int, string> t = mEleData[zKey];
                mEleData[zKey] = new Tuple<Line, double, int, string>(t.Item1, t.Item2, t.Item3, xDataRegAppName);
                rm++;
            }

            // Get Duct wall lines
            for (int index = 0; index < DuctHelper.listOfListOfDuctRoomLineId_With_WallName.Count; index++)
            {
                List<ObjectId> listOfWallLineId = DuctHelper.listOfListOfDuctRoomLineId_With_WallName[index];
                double ln = 0.0;
                string zKey = AEE_Utility.GetXDataRegisterAppName(listOfWallLineId[0]);
                string xDataRegAppName = zKey;

                for (int index1 = 0; index1 < listOfWallLineId.Count; index1++)
                {
                    xDataRegAppName = AEE_Utility.GetXDataRegisterAppName(listOfWallLineId[index1]);
                    Line line = AEE_Utility.GetLine(listOfWallLineId[index1]);
                    mEleData.Add(xDataRegAppName, new Tuple<Line, double, int, string>(line, ln, rm, zKey));
                    Point3d startPoint = mBasePoint + new Vector3d(ln, -(rm * dWallHt + 500), 0);
                    Point3d endPoint = startPoint + new Vector3d(line.Length, 0, 0);

                    AEE_Utility.CreateLine(startPoint, endPoint, CommonModule.eleWallPanelLyrName, CommonModule.eleWallPanelLyrColor);
                    AEE_Utility.CreateTextWithAngle(xDataRegAppName, (startPoint.X + endPoint.X) / 2.0, endPoint.Y - SecTxtSz, endPoint.Z, SecTxtSz,
                                                    CommonModule.eleWallPanelLyrName, CommonModule.eleWallPanelLyrColor, 0);
                    ln += 1000 + line.Length;
                }

                Tuple<Line, double, int, string> t = mEleData[zKey];
                mEleData[zKey] = new Tuple<Line, double, int, string>(t.Item1, t.Item2, t.Item3, xDataRegAppName);
                rm++;
            }

            // Get Lift wall lines
            for (int index = 0; index < LiftHelper.listOfListOfLiftRoomLineId_With_WallName.Count; index++)
            {
                List<ObjectId> listOfWallLineId = LiftHelper.listOfListOfLiftRoomLineId_With_WallName[index];
                double ln = 0.0;
                string zKey = AEE_Utility.GetXDataRegisterAppName(listOfWallLineId[0]);
                string xDataRegAppName = zKey;

                for (int index1 = 0; index1 < listOfWallLineId.Count; index1++)
                {
                    xDataRegAppName = AEE_Utility.GetXDataRegisterAppName(listOfWallLineId[index1]);
                    Line line = AEE_Utility.GetLine(listOfWallLineId[index1]);
                    mEleData.Add(xDataRegAppName, new Tuple<Line, double, int, string>(line, ln, rm, zKey));
                    Point3d startPoint = mBasePoint + new Vector3d(ln, -(rm * dWallHt + 500), 0);
                    Point3d endPoint = startPoint + new Vector3d(line.Length, 0, 0);

                    AEE_Utility.CreateLine(startPoint, endPoint, CommonModule.eleWallPanelLyrName, CommonModule.eleWallPanelLyrColor);
                    AEE_Utility.CreateTextWithAngle(xDataRegAppName, (startPoint.X + endPoint.X) / 2.0, endPoint.Y - SecTxtSz, endPoint.Z, SecTxtSz,
                                                    CommonModule.eleWallPanelLyrName, CommonModule.eleWallPanelLyrColor, 0);
                    ln += 1000 + line.Length;
                }

                Tuple<Line, double, int, string> t = mEleData[zKey];
                mEleData[zKey] = new Tuple<Line, double, int, string>(t.Item1, t.Item2, t.Item3, xDataRegAppName);
                rm++;
            }

            return mEleData;
        }

        private void drawCornerElevation(double dWallHt, Dictionary<string, Tuple<Line, double, int, string>> mEleData)
        {
            string name;
            PartHelper partHelper = new PartHelper();
            CornerHelper cornerHlpr = new CornerHelper();
            Extents3d bb;
            string desc = "";

            for (int index = 0; index < CornerHelper.listOfAllCornerId_ForBOM.Count; index++)
            {
                var cornerId = CornerHelper.listOfAllCornerId_ForBOM[index];

                if (cornerId.IsErased == false && cornerId.IsValid == true)
                {
                    string data = CornerHelper.listOfAllCornerText_ForBOM[index];
                    CornerElevation cnr_elev = CornerHelper.listOfAllCornerElevInfo[index];
                    if (cnr_elev == null)
                        continue;

                    var array = data.Split('@');
                    string itemCode = Convert.ToString(array.GetValue(0));
                    string description = Convert.ToString(array.GetValue(1));
                    string wallType = Convert.ToString(array.GetValue(2));
                    double l1 = Convert.ToDouble(array.GetValue(3));
                    double l2 = Convert.ToDouble(array.GetValue(4));
                    double h = Convert.ToDouble(array.GetValue(5));

                    if (itemCode.StartsWith(CommonModule.externalCornerText))
                    {
                        int rw = -1;
                        double wo = -1;

                        name = cnr_elev.wall1;

                        if (name != "" && mEleData.ContainsKey(name))
                        {
                            rw = mEleData[name].Item3;
                            wo = mEleData[name].Item2 + cnr_elev.dist1;
                        }
                        else
                            continue;

                        Point3d startPoint = mBasePoint + new Vector3d(wo, -rw * dWallHt, 0);
                        Point3d textPoint = startPoint + new Vector3d(l1 / 2.0, h / 2.0, 0);
                        Point3d endPoint = startPoint + new Vector3d(l1, 0, 0);
                        Point3d offsetEndPoint = endPoint + Vector3d.YAxis * h;
                        Point3d offsetStartPoint = startPoint + Vector3d.YAxis * h;
                        AEE_Utility.createRectangle(startPoint, endPoint, offsetEndPoint, offsetStartPoint, CommonModule.externalCornerLyrName, CommonModule.externalCornerLyrColor);
                        var objId = AEE_Utility.CreateMultiLineText(description, textPoint.X, textPoint.Y, textPoint.Z, 20,
                                                                    CommonModule.externalCornerTextLyrName, CommonModule.externalCornerLyrColor, 90, out bb);

                        name = cnr_elev.wall2;

                        if (name != "" && mEleData.ContainsKey(name))
                        {
                            rw = mEleData[name].Item3;
                            wo = mEleData[name].Item2 + cnr_elev.dist2 - CommonModule.externalCorner;
                        }
                        else
                            continue;

                        startPoint = mBasePoint + new Vector3d(wo, -rw * dWallHt, 0);
                        textPoint = startPoint + new Vector3d(l2 / 2.0, h / 2.0, 0);
                        endPoint = startPoint + new Vector3d(l2, 0, 0);
                        offsetEndPoint = endPoint + Vector3d.YAxis * h;
                        offsetStartPoint = startPoint + Vector3d.YAxis * h;
                        AEE_Utility.createRectangle(startPoint, endPoint, offsetEndPoint, offsetStartPoint, CommonModule.externalCornerLyrName, CommonModule.externalCornerLyrColor);
                        var objId2 = AEE_Utility.CreateMultiLineText(description, textPoint.X, textPoint.Y, textPoint.Z, 20,
                                                                     CommonModule.externalCornerTextLyrName, CommonModule.externalCornerLyrColor, 90, out bb);
                    }
                    else if (itemCode.StartsWith(CommonModule.internalCornerText))
                    {
                        int rw = -1;
                        double wo = -1;

                        name = cnr_elev.wall1;

                        if (name != "" && mEleData.ContainsKey(name))
                        {
                            rw = mEleData[name].Item3;
                            wo = mEleData[name].Item2 + cnr_elev.dist1 - CommonModule.internalCorner;
                        }
                        else
                            continue;

                        Point3d startPoint = mBasePoint + new Vector3d(wo, cnr_elev.elev - rw * dWallHt, 0);
                        Point3d textPoint = startPoint + new Vector3d(l1 / 2.0, h / 2.0, 0);
                        Point3d endPoint = startPoint + new Vector3d(l1, 0, 0);
                        Point3d offsetEndPoint = endPoint + Vector3d.YAxis * h;
                        Point3d offsetStartPoint = startPoint + Vector3d.YAxis * h;
                        AEE_Utility.createRectangle(startPoint, endPoint, offsetEndPoint, offsetStartPoint, CommonModule.internalCornerLyrName, CommonModule.internalCornerLyrColor);
                        var objId = AEE_Utility.CreateMultiLineText(description, textPoint.X, textPoint.Y, textPoint.Z, 20,
                                                                    CommonModule.internalCornerTextLyrName, CommonModule.internalCornerLyrColor, 90, out bb);

                        name = cnr_elev.wall2;

                        if (name != "" && mEleData.ContainsKey(name))
                        {
                            rw = mEleData[name].Item3;
                            wo = mEleData[name].Item2 + cnr_elev.dist2;
                        }
                        else
                            continue;

                        startPoint = mBasePoint + new Vector3d(wo, cnr_elev.elev - rw * dWallHt, 0);
                        textPoint = startPoint + new Vector3d(l2 / 2.0, h / 2.0, 0);
                        endPoint = startPoint + new Vector3d(l2, 0, 0);
                        offsetEndPoint = endPoint + Vector3d.YAxis * h;
                        offsetStartPoint = startPoint + Vector3d.YAxis * h;
                        AEE_Utility.createRectangle(startPoint, endPoint, offsetEndPoint, offsetStartPoint, CommonModule.internalCornerLyrName, CommonModule.internalCornerLyrColor);
                        var objId2 = AEE_Utility.CreateMultiLineText(description, textPoint.X, textPoint.Y, textPoint.Z, 20,
                                                                     CommonModule.internalCornerTextLyrName, CommonModule.internalCornerLyrColor, 90, out bb);
                    }
                    else if (itemCode.StartsWith(CommonModule.beamInternalCornerText))
                    {
                        int rw = -1;
                        double wo = -1;

                        name = cnr_elev.wall1;

                        if (name != "" && mEleData.ContainsKey(name))
                        {
                            rw = mEleData[name].Item3;
                            wo = mEleData[name].Item2 + cnr_elev.dist1;
                        }
                        else
                            continue;

                        Point3d startPoint = mBasePoint + new Vector3d(wo, cnr_elev.elev - rw * dWallHt, 0);
                        Point3d textPoint = startPoint + new Vector3d(l1 / 2.0, h / 2.0, 0);
                        Point3d endPoint = startPoint + new Vector3d(l1, 0, 0);
                        Point3d offsetEndPoint = endPoint + Vector3d.YAxis * h;
                        Point3d offsetStartPoint = startPoint + Vector3d.YAxis * h;
                        AEE_Utility.createRectangle(startPoint, endPoint, offsetEndPoint, offsetStartPoint, CommonModule.internalCornerLyrName, CommonModule.internalCornerLyrColor);
                        var objId = AEE_Utility.CreateMultiLineText(description, textPoint.X, textPoint.Y, textPoint.Z, 20,
                                                                    CommonModule.internalCornerTextLyrName, CommonModule.internalCornerLyrColor, 90, out bb);

                        name = cnr_elev.wall2;

                        if (name != "" && mEleData.ContainsKey(name))
                        {
                            rw = mEleData[name].Item3;
                            wo = mEleData[name].Item2 + cnr_elev.dist2;
                        }
                        else
                            continue;

                        startPoint = mBasePoint + new Vector3d(wo, cnr_elev.elev - rw * dWallHt, 0);
                        textPoint = startPoint + new Vector3d(l2 / 2.0, h / 2.0, 0);
                        endPoint = startPoint + new Vector3d(l2, 0, 0);
                        offsetEndPoint = endPoint + Vector3d.YAxis * h;
                        offsetStartPoint = startPoint + Vector3d.YAxis * h;
                        AEE_Utility.createRectangle(startPoint, endPoint, offsetEndPoint, offsetStartPoint, CommonModule.internalCornerLyrName, CommonModule.internalCornerLyrColor);
                        var objId2 = AEE_Utility.CreateMultiLineText(description, textPoint.X, textPoint.Y, textPoint.Z, 20,
                                                                     CommonModule.internalCornerTextLyrName, CommonModule.internalCornerLyrColor, 90, out bb);
                    }
                    else if (itemCode.StartsWith(CommonModule.kickerCornerText) ||
                             itemCode.StartsWith(CommonModule.externalKickerCornerText))
                    {
                        int rw = -1;
                        double wo = -1;

                        name = cnr_elev.wall1;

                        if (name != "" && mEleData.ContainsKey(name))
                        {
                            rw = mEleData[name].Item3;
                            wo = mEleData[name].Item2 + cnr_elev.dist1;
                        }
                        else
                            continue;

                        Point3d startPoint = mBasePoint + new Vector3d(wo, -h - rw * dWallHt, 0);
                        Point3d textPoint = startPoint + new Vector3d(l1 / 2.0, h / 2.0, 0);
                        Point3d endPoint = startPoint + new Vector3d(l1, 0, 0);
                        Point3d offsetEndPoint = endPoint + Vector3d.YAxis * h;
                        Point3d offsetStartPoint = startPoint + Vector3d.YAxis * h;
                        AEE_Utility.createRectangle(startPoint, endPoint, offsetEndPoint, offsetStartPoint, CommonModule.kickerCornerLayerName, CommonModule.kickerCornerLayerColor);
                        var objId1 = AEE_Utility.CreateMultiLineText(description, textPoint.X, textPoint.Y, textPoint.Z, 20,
                                                                    CommonModule.kickerCornerTextLayerName, CommonModule.kickerCornerLayerColor, 90, out bb);

                        Vector3d topkickerlvl = new Vector3d(0, cnr_elev.elev + h, 0);
                        startPoint += topkickerlvl; endPoint += topkickerlvl; offsetEndPoint += topkickerlvl; offsetStartPoint += topkickerlvl;
                        AEE_Utility.createRectangle(startPoint, endPoint, offsetEndPoint, offsetStartPoint, CommonModule.kickerCornerLayerName, CommonModule.kickerCornerLayerColor);
                        var objId11 = AEE_Utility.CreateMultiLineText(description, textPoint.X, textPoint.Y + cnr_elev.elev + h, textPoint.Z, 20,
                                                                      CommonModule.kickerCornerTextLayerName, CommonModule.kickerCornerLayerColor, 90, out bb);
                        name = cnr_elev.wall2;

                        if (name != "" && mEleData.ContainsKey(name))
                        {
                            rw = mEleData[name].Item3;
                            wo = mEleData[name].Item2 + cnr_elev.dist2;
                        }
                        else
                            continue;

                        startPoint = mBasePoint + new Vector3d(wo, -h - rw * dWallHt, 0);
                        textPoint = startPoint + new Vector3d(l2 / 2.0, h / 2.0, 0);
                        endPoint = startPoint + new Vector3d(l2, 0, 0);
                        offsetEndPoint = endPoint + Vector3d.YAxis * h;
                        offsetStartPoint = startPoint + Vector3d.YAxis * h;
                        AEE_Utility.createRectangle(startPoint, endPoint, offsetEndPoint, offsetStartPoint, CommonModule.externalCornerLyrName, CommonModule.externalCornerLyrColor);
                        var objId2 = AEE_Utility.CreateMultiLineText(description, textPoint.X, textPoint.Y, textPoint.Z, 20,
                                                                     CommonModule.externalCornerTextLyrName, CommonModule.externalCornerLyrColor, 90, out bb);

                        startPoint += topkickerlvl; endPoint += topkickerlvl; offsetEndPoint += topkickerlvl; offsetStartPoint += topkickerlvl;
                        AEE_Utility.createRectangle(startPoint, endPoint, offsetEndPoint, offsetStartPoint, CommonModule.kickerCornerLayerName, CommonModule.kickerCornerLayerColor);
                        var objId31 = AEE_Utility.CreateMultiLineText(description, textPoint.X, textPoint.Y + cnr_elev.elev + h, textPoint.Z, 20,
                                                                      CommonModule.kickerCornerTextLayerName, CommonModule.kickerCornerLayerColor, 90, out bb);
                    }
                    else if (itemCode.StartsWith(CommonModule.slabJointCornerText))
                    {
                        int rw = -1;
                        double wo = -1;

                        name = cnr_elev.wall1;

                        if (name != "" && mEleData.ContainsKey(name))
                        {
                            rw = mEleData[name].Item3;
                            wo = mEleData[name].Item2 + cnr_elev.dist1;
                        }
                        else
                            continue;

                        Point3d startPoint = mBasePoint + new Vector3d(wo, cnr_elev.elev - rw * dWallHt, 0);
                        Point3d textPoint = startPoint + new Vector3d(l1 / 2.0, h / 2.0, 0);
                        Point3d endPoint = startPoint + new Vector3d(l1, 0, 0);
                        Point3d offsetEndPoint = endPoint + Vector3d.YAxis * h;
                        Point3d offsetStartPoint = startPoint + Vector3d.YAxis * h;
                        AEE_Utility.createRectangle(startPoint, endPoint, offsetEndPoint, offsetStartPoint, CommonModule.slabJointCornerLayerName, CommonModule.slabJointCornerLayerColor);
                        var objId1 = AEE_Utility.CreateMultiLineText(description, textPoint.X, textPoint.Y, textPoint.Z, 20,
                                                                    CommonModule.slabJointCornerTextLayerName, CommonModule.slabJointCornerLayerColor, 90, out bb);

                        name = cnr_elev.wall2;

                        if (name != "" && mEleData.ContainsKey(name))
                        {
                            rw = mEleData[name].Item3;
                            wo = mEleData[name].Item2 + cnr_elev.dist2;
                        }
                        else
                            continue;

                        startPoint = mBasePoint + new Vector3d(wo, cnr_elev.elev - rw * dWallHt, 0);
                        textPoint = startPoint + new Vector3d(l2 / 2.0, h / 2.0, 0);
                        endPoint = startPoint + new Vector3d(l2, 0, 0);
                        offsetEndPoint = endPoint + Vector3d.YAxis * h;
                        offsetStartPoint = startPoint + Vector3d.YAxis * h;
                        AEE_Utility.createRectangle(startPoint, endPoint, offsetEndPoint, offsetStartPoint, CommonModule.slabJointCornerLayerName, CommonModule.slabJointCornerLayerColor);
                        var objId2 = AEE_Utility.CreateMultiLineText(description, textPoint.X, textPoint.Y, textPoint.Z, 20,
                                                                     CommonModule.slabJointCornerTextLayerName, CommonModule.slabJointCornerLayerColor, 90, out bb);
                    }
                    else if (itemCode.StartsWith(CommonModule.doorWindowThickTopCrnrText))
                    {
                        int rw = -1;
                        double wo = -1;

                        name = cnr_elev.wall1;

                        if (name != "" && mEleData.ContainsKey(name))
                        {
                            rw = mEleData[name].Item3;
                            wo = mEleData[name].Item2 + cnr_elev.dist1;
                        }
                        else
                            continue;

                        Point3d startPoint = mBasePoint + new Vector3d(wo, cnr_elev.elev - rw * dWallHt, 0);
                        Point3d textPoint = startPoint + new Vector3d(l1 / 2.0, h / 2.0, 0);
                        Point3d endPoint = startPoint + new Vector3d(l1, 0, 0);
                        Point3d offsetEndPoint = endPoint + Vector3d.YAxis * h;
                        Point3d offsetStartPoint = startPoint + Vector3d.YAxis * h;
                        ObjectId intrnlCrnr_ObjId = partHelper.drawInternalCorner("", startPoint.X, startPoint.Y, l1, l2, CommonModule.panelDepth, CommonModule.internalCornerThick, CommonModule.panelDepth, CommonModule.internalCornerLyrName, CommonModule.internalCornerLyrColor, cnr_elev.dist2);
                        double crnerRotationAngle = cnr_elev.dist2 + CommonModule.cornerRotationAngle;
                        var textId = cornerHlpr.writeTextInCorner(intrnlCrnr_ObjId, description, crnerRotationAngle, CommonModule.internalCornerTextLyrName, CommonModule.internalCornerLyrColor);
                    }
                    else if (itemCode.StartsWith(CommonModule.slabJointCornerText))
                    {
                        int rw = -1;
                        double wo = -1;

                        name = cnr_elev.wall1;

                        if (name != "" && mEleData.ContainsKey(name))
                        {
                            rw = mEleData[name].Item3;
                            wo = mEleData[name].Item2 + cnr_elev.dist1;
                        }
                        else
                            continue;

                        Point3d startPoint = mBasePoint + new Vector3d(wo, cnr_elev.elev - rw * dWallHt, 0);
                        Point3d textPoint = startPoint + new Vector3d(l1 / 2.0, h / 2.0, 0);
                        Point3d endPoint = startPoint + new Vector3d(l1, 0, 0);
                        Point3d offsetEndPoint = endPoint + Vector3d.YAxis * h;
                        Point3d offsetStartPoint = startPoint + Vector3d.YAxis * h;
                        AEE_Utility.createRectangle(startPoint, endPoint, offsetEndPoint, offsetStartPoint, CommonModule.slabJointCornerLayerName, CommonModule.slabJointCornerLayerColor);
                        var objId1 = AEE_Utility.CreateMultiLineText(description, textPoint.X, textPoint.Y, textPoint.Z, 20,
                                                                    CommonModule.slabJointCornerTextLayerName, CommonModule.slabJointCornerLayerColor, 90, out bb);

                        name = cnr_elev.wall2;

                        if (name != "" && mEleData.ContainsKey(name))
                        {
                            rw = mEleData[name].Item3;
                            wo = mEleData[name].Item2 + cnr_elev.dist2;
                        }
                        else
                            continue;

                        startPoint = mBasePoint + new Vector3d(wo, cnr_elev.elev - rw * dWallHt, 0);
                        textPoint = startPoint + new Vector3d(l2 / 2.0, h / 2.0, 0);
                        endPoint = startPoint + new Vector3d(l2, 0, 0);
                        offsetEndPoint = endPoint + Vector3d.YAxis * h;
                        offsetStartPoint = startPoint + Vector3d.YAxis * h;
                        AEE_Utility.createRectangle(startPoint, endPoint, offsetEndPoint, offsetStartPoint, CommonModule.slabJointCornerLayerName, CommonModule.slabJointCornerLayerColor);
                        var objId2 = AEE_Utility.CreateMultiLineText(description, textPoint.X, textPoint.Y, textPoint.Z, 20,
                                                                     CommonModule.slabJointCornerTextLayerName, CommonModule.slabJointCornerLayerColor, 90, out bb);
                    }
                    else if (itemCode.StartsWith(beamCornerItemCode))
                    {
                        int rw = -1;
                        double wo = -1;
                        h = CommonModule.internalCorner;
                        name = cnr_elev.wall1;

                        if (name != "" && mEleData.ContainsKey(name))
                        {
                            rw = mEleData[name].Item3;
                            wo = mEleData[name].Item2 + cnr_elev.dist1;
                        }
                        else
                            continue;

                        Point3d startPoint = mBasePoint + new Vector3d(wo, cnr_elev.elev - rw * dWallHt, 0);
                        Point3d textPoint = startPoint + new Vector3d(l1 / 2.0, h / 2.0, 0);
                        Point3d endPoint = startPoint + new Vector3d(l1, 0, 0);
                        Point3d offsetEndPoint = endPoint + Vector3d.YAxis * h;
                        Point3d offsetStartPoint = startPoint + Vector3d.YAxis * h;
                        AEE_Utility.createRectangle(startPoint, endPoint, offsetEndPoint, offsetStartPoint, CommonModule.beamBottomWallPanelLayerName, CommonModule.beamBottomWallPanelLayerColor);
                        desc = description + "\n" + index.ToString() + " " + name + " Start";
                        var objId1 = AEE_Utility.CreateMultiLineText(desc, textPoint.X, textPoint.Y, textPoint.Z, 20,
                                                                    CommonModule.beamBottomWallPanelTextLayerName, CommonModule.beamBottomWallPanelLayerColor, 90, out bb);

                        name = cnr_elev.wall2;

                        if (name != "" && mEleData.ContainsKey(name))
                        {
                            rw = mEleData[name].Item3;
                            wo = mEleData[name].Item2 + cnr_elev.dist2;
                        }
                        else
                            continue;

                        startPoint = mBasePoint + new Vector3d(wo, cnr_elev.elev - rw * dWallHt, 0);
                        textPoint = startPoint + new Vector3d(l2 / 2.0, h / 2.0, 0);
                        endPoint = startPoint + new Vector3d(l2, 0, 0);
                        offsetEndPoint = endPoint + Vector3d.YAxis * h;
                        offsetStartPoint = startPoint + Vector3d.YAxis * h;
                        AEE_Utility.createRectangle(startPoint, endPoint, offsetEndPoint, offsetStartPoint, CommonModule.beamBottomWallPanelLayerName, CommonModule.beamBottomWallPanelLayerColor);
                        desc = description + "\n" + index.ToString() + " " + name + " End";
                        var objId2 = AEE_Utility.CreateMultiLineText(desc, textPoint.X, textPoint.Y, textPoint.Z, 20,
                                                                     CommonModule.beamBottomWallPanelTextLayerName, CommonModule.beamBottomWallPanelLayerColor, 90, out bb);
                    }
                }
            }
        }


        private void ElevationOfWall(/*ProgressForm progressForm, string progressbarMsg*/)
        {
            double x = mBasePoint.X - CreateShellPlanHelper.start_X_NewLayout;
            double y = mBasePoint.Y - CreateShellPlanHelper.start_Y_NewLayout;
            Vector3d v = new Vector3d(x, y, 0.0);
            Vector3d vh = new Vector3d(0.0, 1.0, 0.0);
            double dWallHt = PanelLayout_UI.maxHeightOfPanel * 2;

            Dictionary<string, Tuple<Line, double, int, string>> mEleData = GetWallLenghts(dWallHt);
            drawCornerElevation(dWallHt, mEleData);
            drawPanelElev(dWallHt, mEleData);
        }

        void drawPanelElev(double dWallHt, Dictionary<string, Tuple<Line, double, int, string>> mEleData)
        {
            string name;
            int wallId, roomId;
            bool ext;

            // Get Panel lines and draw elevation
            for (int index = 0; index < WallPanelHelper.listOfAllWallPanelRectId.Count; index++)
            {
                var panelLineId = WallPanelHelper.listOfAllWallPanelLineId[index];
                var panelRectId = WallPanelHelper.listOfAllWallPanelRectId[index];
                if (panelLineId.IsErased == false && panelLineId.IsValid == true)
                {
                    string data = WallPanelHelper.listOfAllWallPanelData[index];

                    var array = data.Split('@');
                    double wallWidth = Convert.ToDouble(array.GetValue(0));
                    double wallHeight = Convert.ToDouble(array.GetValue(1));
                    double wallLength = Convert.ToDouble(array.GetValue(2));
                    string itemCode = Convert.ToString(array.GetValue(3));
                    string description = Convert.ToString(array.GetValue(4));
                    string wallType = Convert.ToString(array.GetValue(5));
                    double elev = Convert.ToDouble(array.GetValue(6));

                    if (itemCode.StartsWith(PanelLayout_UI.wallPanelName) ||
                        itemCode.StartsWith(PanelLayout_UI.windowPanelName))
                    {
                        List<Point3d> lnpts = AEE_Utility.GetStartEndPointOfLine(panelLineId);

                        getWallDetails(panelLineId, out name, out wallId, out roomId, out ext);
                        double l = AEE_Utility.GetLengthOfLine(panelLineId);
                        Line ln = AEE_Utility.GetLine(panelLineId);
                        Entity en = AEE_Utility.GetEntityForRead(panelRectId);
                        int rw = -1;
                        double wo = -1, d = 0;
                        if (name != "" && mEleData.ContainsKey(name))
                        {
                            Line lnw = mEleData[name].Item1;
                            rw = mEleData[name].Item3;
                            wo = mEleData[name].Item2;
                            d = Math.Min(lnw.StartPoint.DistanceTo(lnw.GetClosestPointTo(ln.StartPoint, true)),
                                         lnw.StartPoint.DistanceTo(lnw.GetClosestPointTo(ln.EndPoint, true)));
                        }
                        else
                            continue;

                        double dW = wallWidth, dH = wallHeight;
                        double extoffset = 0.0;

                        if (description.Contains("(R)"))
                        {
                            dW += CommonModule.externalCorner;
                            d -= CommonModule.externalCorner;
                            extoffset = -CommonModule.externalCorner - 1000;

                            Tuple<Line, double, int, string> t = mEleData[mEleData[name].Item4];
                            if (t.Item4 == name && mEleData[name].Item2 < t.Item2)
                                extoffset = t.Item2 + t.Item1.Length - wo + CommonModule.externalCorner;
                        }
                        else if (description.Contains("(L)"))
                        {
                            dW += CommonModule.externalCorner;
                            extoffset = dW + 1000;

                            Tuple<Line, double, int, string> t = mEleData[mEleData[name].Item4];
                            if (t.Item4 == name && mEleData[name].Item2 > t.Item2)
                                extoffset += t.Item2 + t.Item1.Length - wo;
                        }

                        Point3d startPoint = mBasePoint + new Vector3d(wo + d, -rw * dWallHt, 0);
                        Point3d textPoint = startPoint + new Vector3d(dW / 2.0, dH / 2.0, 0);

                        DrawPanelElev(startPoint, dW, dH, CommonModule.wallPanelLyrName, CommonModule.eleWallPanelLyrColor);
                        AEE_Utility.CreateMultiLineText(description, textPoint.X, textPoint.Y, textPoint.Z, 20,
                                                        CommonModule.wallPanelTextLyrName, CommonModule.eleWallPanelLyrColor, 90);
                        if (extoffset != 0.0)
                        {
                            dW = CommonModule.externalCorner;
                            startPoint += new Vector3d(extoffset, 0, 0);
                            textPoint = startPoint + new Vector3d(dW / 2.0, dH / 2.0, 0);
                            Point3d endPoint = startPoint + new Vector3d(dW, 0, 0);
                            Point3d offsetEndPoint = endPoint + Vector3d.YAxis * dH;
                            Point3d offsetStartPoint = startPoint + Vector3d.YAxis * dH;

                            AEE_Utility.createRectangle(startPoint, endPoint, offsetEndPoint, offsetStartPoint, CommonModule.wallPanelLyrName, CommonModule.eleWallPanelLyrColor);
                            Extents3d bb;
                            var objId = AEE_Utility.CreateMultiLineText(description, textPoint.X, textPoint.Y, textPoint.Z, 20,
                                                                        CommonModule.wallPanelTextLyrName, CommonModule.eleWallPanelLyrColor, 90, out bb);
                        }
                    }
                    else if (itemCode.StartsWith(PanelLayout_UI.windowTopPanelName) ||
                             itemCode.StartsWith(PanelLayout_UI.doorTopPanelName))
                    {
                        List<Point3d> lnpts = AEE_Utility.GetStartEndPointOfLine(panelLineId);

                        getWallDetails(panelLineId, out name, out wallId, out roomId, out ext);
                        double l = AEE_Utility.GetLengthOfLine(panelLineId);
                        Line ln = AEE_Utility.GetLine(panelLineId);
                        Entity en = AEE_Utility.GetEntityForRead(panelRectId);
                        int rw = -1;
                        double wo = -1, d = 0;
                        if (name != "" && mEleData.ContainsKey(name))
                        {
                            Line lnw = mEleData[name].Item1;
                            rw = mEleData[name].Item3;
                            wo = mEleData[name].Item2;
                            d = Math.Min(lnw.StartPoint.DistanceTo(lnw.GetClosestPointTo(ln.StartPoint - CreateShellPlanHelper.moveVector_ForWindowDoorLayout, true)),
                                         lnw.StartPoint.DistanceTo(lnw.GetClosestPointTo(ln.EndPoint - CreateShellPlanHelper.moveVector_ForWindowDoorLayout, true)));
                        }
                        else
                            continue;

                        double dW = wallHeight, dH = wallWidth;
                        Point3d startPoint = mBasePoint + new Vector3d(wo + d, elev - rw * dWallHt, 0);
                        Point3d textPoint = startPoint + new Vector3d(dW / 2.0, dH / 2.0, 0);

                        DrawPanelElev(startPoint, dW, dH, CommonModule.wallPanelLyrName, CommonModule.eleWallPanelLyrColor);
                        Extents3d bb;
                        var objId = AEE_Utility.CreateMultiLineText(description, textPoint.X, textPoint.Y, textPoint.Z, 20,
                                                                    CommonModule.wallPanelTextLyrName, CommonModule.eleWallPanelLyrColor, 0, out bb);
                        //bb.MinPoint 
                    }
                    else if (itemCode.StartsWith(PanelLayout_UI.beamPanelName))
                    {
                        List<Point3d> lnpts = AEE_Utility.GetStartEndPointOfLine(panelLineId);

                        getWallDetails(panelLineId, out name, out wallId, out roomId, out ext);
                        double l = AEE_Utility.GetLengthOfLine(panelLineId);
                        Line ln = AEE_Utility.GetLine(panelLineId);
                        Entity en = AEE_Utility.GetEntityForRead(panelRectId);
                        int rw = -1;
                        double wo = -1, d = 0;
                        if (name != "" && mEleData.ContainsKey(name))
                        {
                            Line lnw = mEleData[name].Item1;
                            rw = mEleData[name].Item3;
                            wo = mEleData[name].Item2;
                            d = Math.Min(lnw.StartPoint.DistanceTo(lnw.GetClosestPointTo(ln.StartPoint - CreateShellPlanHelper.moveVector_ForBeamLayout, true)),
                                         lnw.StartPoint.DistanceTo(lnw.GetClosestPointTo(ln.EndPoint - CreateShellPlanHelper.moveVector_ForBeamLayout, true)));
                        }
                        else
                            continue;

                        double dW = wallWidth, dH = wallHeight;
                        Point3d startPoint = mBasePoint + new Vector3d(wo + d, elev - rw * dWallHt, 0);
                        Point3d textPoint = startPoint + new Vector3d(dW / 2.0, dH / 2.0, 0);

                        DrawPanelElev(startPoint, dW, dH, CommonModule.wallPanelLyrName, CommonModule.eleWallPanelLyrColor);
                        Extents3d bb;
                        var objId = AEE_Utility.CreateMultiLineText(description, textPoint.X, textPoint.Y, textPoint.Z, 20,
                                                                    CommonModule.wallPanelTextLyrName, CommonModule.eleWallPanelLyrColor, 0, out bb);
                        //bb.MinPoint 
                    }
                    else if (itemCode.StartsWith(PanelLayout_UI.doorWindowBottomPanelName) ||
                             itemCode.StartsWith(PanelLayout_UI.windowThickBottomWallPanelText) ||
                             itemCode.StartsWith(CommonModule.windowThickBottomPropText) ||
                             itemCode.StartsWith(CommonModule.doorWindowThickTopPropText))
                    {
                        List<Point3d> lnpts = AEE_Utility.GetStartEndPointOfLine(panelLineId);

                        getWallDetails(panelRectId, out name, out wallId, out roomId, out ext);
                        double l = AEE_Utility.GetLengthOfLine(panelLineId);/**/
                        Line ln = AEE_Utility.GetLine(panelLineId);
                        Entity en = AEE_Utility.GetEntityForRead(panelRectId);
                        int rw = -1;
                        double wo = -1, d = 0;
                        if (name != "" && mEleData.ContainsKey(name))
                        {
                            Line lnw = mEleData[name].Item1;
                            rw = mEleData[name].Item3;
                            wo = mEleData[name].Item2;

                            if (itemCode.StartsWith(CommonModule.windowThickBottomPropText) ||
                                itemCode.StartsWith(CommonModule.doorWindowThickTopPropText))
                            {
                                Vector3d lv = (ln.StartPoint - ln.EndPoint).GetPerpendicularVector();
                                Vector3d v1 = lv.GetNormal();
                                lv *= wallLength / lv.Length;
                                Point3d npt = lnw.GetClosestPointTo(ln.StartPoint, true);
                                Point3d ept = npt - lv;
                                d = Math.Min(lnw.StartPoint.DistanceTo(npt),
                                             lnw.StartPoint.DistanceTo(ept));
                            }
                            else
                            {
                                d = Math.Min(lnw.StartPoint.DistanceTo(lnw.GetClosestPointTo(ln.StartPoint, true)),
                                             lnw.StartPoint.DistanceTo(lnw.GetClosestPointTo(ln.EndPoint, true)));
                            }
                        }
                        else
                            continue;

                        double dW = wallLength, dH = CommonModule.externalCorner;
                        double sill = itemCode.StartsWith(PanelLayout_UI.windowThickBottomWallPanelText) ||
                                      itemCode.StartsWith(CommonModule.windowThickBottomPropText)
                                      ? CommonModule.externalCorner : 0.0;
                        Point3d startPoint = mBasePoint + new Vector3d(wo + d, elev - sill - rw * dWallHt, 0);
                        Point3d textPoint = startPoint + new Vector3d(dW / 2.0, dH / 2.0, 0);
                        Point3d endPoint = startPoint + new Vector3d(dW, 0, 0);
                        Point3d offsetEndPoint = endPoint + Vector3d.YAxis * dH;
                        Point3d offsetStartPoint = startPoint + Vector3d.YAxis * dH;

                        AEE_Utility.createRectangle(startPoint, endPoint, offsetEndPoint, offsetStartPoint, CommonModule.wallPanelLyrName, CommonModule.eleWallPanelLyrColor);
                        Extents3d bb;
                        var objId = AEE_Utility.CreateMultiLineText(description, textPoint.X, textPoint.Y, textPoint.Z, 20,
                                                                    CommonModule.wallPanelTextLyrName, CommonModule.eleWallPanelLyrColor, 0, out bb);
                        //bb.MinPoint 
                    }
                    else if (itemCode.StartsWith(CommonModule.doorThickWallPanelText) ||
                             itemCode.StartsWith(CommonModule.windowThickWallPanelText))
                    {
                        List<Point3d> lnpts = AEE_Utility.GetStartEndPointOfLine(panelLineId);

                        getWallDetails(panelRectId, out name, out wallId, out roomId, out ext);
                        double l = AEE_Utility.GetLengthOfLine(panelLineId);/**/
                        Line ln = AEE_Utility.GetLine(panelLineId);
                        Entity en = AEE_Utility.GetEntityForRead(panelRectId);
                        int rw = -1;
                        double wo = -1, d = 0;
                        if (name != "" && mEleData.ContainsKey(name))
                        {
                            Line lnw = mEleData[name].Item1;
                            rw = mEleData[name].Item3;
                            wo = mEleData[name].Item2;
                            d = Math.Min(lnw.StartPoint.DistanceTo(lnw.GetClosestPointTo(ln.StartPoint, true)),
                                            lnw.StartPoint.DistanceTo(lnw.GetClosestPointTo(ln.EndPoint, true)));
                        }
                        else
                            continue;

                        double dW = CommonModule.externalCorner, dH = wallHeight;
                        Point3d startPoint = mBasePoint + new Vector3d(wo + d, elev - rw * dWallHt, 0);
                        Point3d textPoint = startPoint + new Vector3d(dW / 2.0, dH / 2.0, 0);
                        Point3d endPoint = startPoint + new Vector3d(dW, 0, 0);
                        Point3d offsetEndPoint = endPoint + Vector3d.YAxis * dH;
                        Point3d offsetStartPoint = startPoint + Vector3d.YAxis * dH;

                        AEE_Utility.createRectangle(startPoint, endPoint, offsetEndPoint, offsetStartPoint, CommonModule.wallPanelLyrName, CommonModule.eleWallPanelLyrColor);
                        Extents3d bb;
                        var objId = AEE_Utility.CreateMultiLineText(description, textPoint.X, textPoint.Y, textPoint.Z, 20,
                                                                    CommonModule.wallPanelTextLyrName, CommonModule.eleWallPanelLyrColor, 90, out bb);
                        //bb.MinPoint 
                    }
                    else if (itemCode.StartsWith(PanelLayout_UI.slabJointWallName))
                    {
                        List<Point3d> lnpts = AEE_Utility.GetStartEndPointOfLine(panelLineId);

                        getWallDetails(panelRectId, out name, out wallId, out roomId, out ext);
                        double l = AEE_Utility.GetLengthOfLine(panelLineId);/**/
                        Line ln = AEE_Utility.GetLine(panelLineId);
                        Entity en = AEE_Utility.GetEntityForRead(panelRectId);
                        int rw = -1;
                        double wo = -1, d = 0;
                        if (name != "" && mEleData.ContainsKey(name))
                        {
                            Line lnw = mEleData[name].Item1;
                            rw = mEleData[name].Item3;
                            wo = mEleData[name].Item2;
                            d = Math.Min(lnw.StartPoint.DistanceTo(lnw.GetClosestPointTo(ln.StartPoint - CreateShellPlanHelper.moveVector_ForSlabJointLayout, true)),
                                         lnw.StartPoint.DistanceTo(lnw.GetClosestPointTo(ln.EndPoint - CreateShellPlanHelper.moveVector_ForSlabJointLayout, true)));
                        }
                        else
                            continue;

                        double dW = wallWidth, dH = wallHeight;
                        Point3d startPoint = mBasePoint + new Vector3d(wo + d, elev - rw * dWallHt, 0);
                        Point3d textPoint = startPoint + new Vector3d(dW / 2.0, dH / 2.0, 0);
                        Point3d endPoint = startPoint + new Vector3d(dW, 0, 0);
                        Point3d offsetEndPoint = endPoint + Vector3d.YAxis * dH;
                        Point3d offsetStartPoint = startPoint + Vector3d.YAxis * dH;

                        AEE_Utility.createRectangle(startPoint, endPoint, offsetEndPoint, offsetStartPoint, CommonModule.slabJointCornerLayerName, CommonModule.slabJointCornerLayerColor);
                        Extents3d bb;
                        var objId = AEE_Utility.CreateMultiLineText(description, textPoint.X, textPoint.Y, textPoint.Z, 20,
                                                                    CommonModule.slabJointWallPanelTextLayerName, CommonModule.slabJointWallPanelLayerColor,
                                                                    0, out bb);
                    }
                    else if (itemCode.StartsWith(PanelLayout_UI.kickerWallName))
                    {
                        List<Point3d> lnpts = AEE_Utility.GetStartEndPointOfLine(panelLineId);

                        getWallDetails(panelRectId, out name, out wallId, out roomId, out ext);
                        double l = AEE_Utility.GetLengthOfLine(panelLineId);/**/
                        Line ln = AEE_Utility.GetLine(panelLineId);
                        Entity en = AEE_Utility.GetEntityForRead(panelRectId);
                        int rw = -1;
                        double wo = -1, d = 0;
                        if (name != "" && mEleData.ContainsKey(name))
                        {
                            Line lnw = mEleData[name].Item1;
                            rw = mEleData[name].Item3;
                            wo = mEleData[name].Item2;
                            d = Math.Min(lnw.StartPoint.DistanceTo(lnw.GetClosestPointTo(ln.StartPoint - CreateShellPlanHelper.moveVector_ForSunkanSlabLayout, true)),
                                         lnw.StartPoint.DistanceTo(lnw.GetClosestPointTo(ln.EndPoint - CreateShellPlanHelper.moveVector_ForSunkanSlabLayout, true)));
                        }
                        else
                            continue;

                        double dW = wallWidth, dH = wallHeight;
                        Point3d startPoint = mBasePoint + new Vector3d(wo + d, elev - dH - rw * dWallHt, 0);
                        Point3d textPoint = startPoint + new Vector3d(dW / 2.0, dH / 2.0, 0);
                        Point3d endPoint = startPoint + new Vector3d(dW, 0, 0);
                        Point3d offsetEndPoint = endPoint + Vector3d.YAxis * dH;
                        Point3d offsetStartPoint = startPoint + Vector3d.YAxis * dH;

                        AEE_Utility.createRectangle(startPoint, endPoint, offsetEndPoint, offsetStartPoint, CommonModule.kickerWallPanelLayerName, CommonModule.kickerWallPanelLayerColor);
                        Extents3d bb;
                        var objId = AEE_Utility.CreateMultiLineText(description, textPoint.X, textPoint.Y, textPoint.Z, 20,
                                                                    CommonModule.kickerWallPanelTextLayerName,
                                                                    CommonModule.kickerWallPanelLayerColor,
                                                                    0, out bb);
                        //bb.MinPoint kickerWallPanelLayerColor
                    }
                    else
                    {
                        string d = itemCode;
                    }
                }
            }
        }

        void DrawPanelElev(Point3d p1, double dW, double dH, string layerName, int colorIndex)
        {
            Point3d endPoint = p1 + new Vector3d(dW, 0, 0);
            Point3d offsetEndPoint = endPoint + Vector3d.YAxis * dH;
            Point3d offsetStartPoint = p1 + Vector3d.YAxis * dH;
            Vector3d vne = new Vector3d(CommonModule.internalCornerThick, CommonModule.internalCornerThick, 0);
            Vector3d vnw = new Vector3d(CommonModule.internalCornerThick, -CommonModule.internalCornerThick, 0);
            var objId1 = AEE_Utility.createRectangle(p1, endPoint, offsetEndPoint, offsetStartPoint, layerName, colorIndex);
            var objId2 = AEE_Utility.createRectangle(p1 + vne, endPoint - vnw, offsetEndPoint - vne, offsetStartPoint + vnw, layerName, colorIndex);
        }
    }
}
