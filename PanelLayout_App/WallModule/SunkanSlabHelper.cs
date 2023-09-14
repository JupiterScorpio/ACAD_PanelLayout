using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using PanelLayout_App.CivilModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static PanelLayout_App.RotationHelper;

namespace PanelLayout_App.WallModule
{
    public class SunkanSlabHelper
    {
        public static List<ObjectId> listOfSunkanSlab_ObjId = new List<ObjectId>();

        public static List<ObjectId> listOfLinesBtwTwoCrners_SunkanSlabWallLineId = new List<ObjectId>();
        public static List<bool> listOfLinesBtwTwoCrners_SunkanSlabWallLineflag = new List<bool>();
        public static List<ObjectId> listOfLinesBtwTwoCrners_SunkanSlabId = new List<ObjectId>();
        public static Dictionary<string,ObjectId> listOfWallName_SunkanSlabId =new Dictionary<string, ObjectId>();

        public static List<ObjectId> listOfWindowWall_SunkanSlabWallLineId = new List<ObjectId>();
        public static List<bool> listOfWindowWall_SunkanSlabWallLineflag = new List<bool>();
        public static List<ObjectId> listOfWindowWall_SunkanSlabId = new List<ObjectId>();

        public static List<ObjectId> listOfDoorWall_SunkanSlabWallLineId = new List<ObjectId>();
        public static List<bool> listOfDoorWall_SunkanSlabWallLineflag = new List<bool>();
        public static List<ObjectId> listOfDoorWall_SunkanSlabId = new List<ObjectId>();   

        public static void changeWallPanelLayer_ForExternalSunkanSlab(ObjectId sunkanSlabId, ObjectId internalWall_Id, ObjectId offsetInterwall_ObjId, List<ObjectId> listOfInternalWallLineId)
        {
            if (sunkanSlabId.IsValid == true)
            {
                var sankanSlab = AEE_Utility.GetEntityForRead(sunkanSlabId);
                var flag = ExternalSunkanSlab_UI_Helper.checkExternalSunkanSlabLayerIsExist(sankanSlab.Layer);
                if (flag == true)
                {
                    AEE_Utility.changeLayer(internalWall_Id, CommonModule.externalWallLayerName, CommonModule.externalCornerLyrColor);
                    AEE_Utility.changeLayer(offsetInterwall_ObjId, CommonModule.externalWallLayerName, CommonModule.externalCornerLyrColor);

                    foreach (var id in listOfInternalWallLineId)
                    {
                        AEE_Utility.changeLayer(id, CommonModule.externalWallLayerName, CommonModule.externalCornerLyrColor);
                    }
                }
            }      
        }

        public static double getSunkanSlabLevelDifference_LessThan_RC__Corners(ObjectId sunkanSlabId)
        {
            double levelDifferenceHeight = 0;
            if (sunkanSlabId.IsValid == true)
            {
                levelDifferenceHeight = getSunkanSlabLevelDifference(sunkanSlabId);

                bool flagOfGreater = checkSunkanSlabLevelDiffrnce_Is_Greater(sunkanSlabId);
                if (flagOfGreater == true)
                {
                    levelDifferenceHeight = 0;
                }
            }
            return levelDifferenceHeight;
        }


        public static double getSunkanSlabLevelDifference_WallPanel(ObjectId sunkanSlabId)
        {
            double levelDifferenceHeight = getSunkanSlabLevelDifference(sunkanSlabId);
            bool flagOfGreater = checkSunkanSlabLevelDiffrnce_Is_Greater(sunkanSlabId);
            if (flagOfGreater == true)
            {
                levelDifferenceHeight = 0;
            }
            return levelDifferenceHeight;
        }


        public bool checkSunkanSlabIsAvailable_In_InternalWall(ObjectId wallObjId, out ObjectId sunkanSlabObjId)
        {
            sunkanSlabObjId = new ObjectId();

            var listOfVerticesOfWall = AEE_Utility.GetPolylineVertexPoint(wallObjId);

            double wallMin_X = listOfVerticesOfWall.Min(sortPoint => sortPoint.X);
            double wallMin_Y = listOfVerticesOfWall.Min(sortPoint => sortPoint.Y);

            double wallMax_X = listOfVerticesOfWall.Max(sortPoint => sortPoint.X);
            double wallMax_Y = listOfVerticesOfWall.Max(sortPoint => sortPoint.Y);

            double wallCntr_X = wallMin_X + ((wallMax_X - wallMin_X) / 2);
            double wallCntr_Y = wallMin_Y + ((wallMax_Y - wallMin_Y) / 2);

            for (int i = 0; i < listOfSunkanSlab_ObjId.Count; i++)
            {
                ObjectId sunkanSlabId = listOfSunkanSlab_ObjId[i];
                var listOfVerticesOfSunkanSlab = AEE_Utility.GetPolylineVertexPoint(sunkanSlabId);

                double sunkanMin_X = listOfVerticesOfSunkanSlab.Min(sortPoint => sortPoint.X);
                double sunkanMin_Y = listOfVerticesOfSunkanSlab.Min(sortPoint => sortPoint.Y);

                double sunkanMax_X = listOfVerticesOfSunkanSlab.Max(sortPoint => sortPoint.X);
                double sunkanMax_Y = listOfVerticesOfSunkanSlab.Max(sortPoint => sortPoint.Y);

                double sunkanCntr_X = sunkanMin_X + ((sunkanMax_X - sunkanMin_X) / 2);
                double sunkanCntr_Y = sunkanMin_Y + ((sunkanMax_Y - sunkanMin_Y) / 2);

                var length = AEE_Utility.GetLengthOfLine(wallCntr_X, wallCntr_Y, sunkanCntr_X, sunkanCntr_Y);
                if (length <= 1)
                {
                    sunkanSlabObjId = sunkanSlabId;
                    return true;
                }
            }
            return false;
        }

        public static double getSunkanSlabLevelDifference(ObjectId sunkanSlabId)
        {
            double levelDifferenceOfSunkanSlab = 0;
            if (sunkanSlabId.IsValid == true)
            {
                var sunkanSlab = AEE_Utility.GetEntityForRead(sunkanSlabId);
                levelDifferenceOfSunkanSlab = InternalSunkanSlab_UI_Helper.getLevelDifferenceOfSunkanSlab(sunkanSlab.Layer);

                if (levelDifferenceOfSunkanSlab == 0)
                {
                    levelDifferenceOfSunkanSlab = ExternalSunkanSlab_UI_Helper.getExternalLevelDifferenceOfSunkanSlab(sunkanSlab.Layer);
                }
            }     

            return levelDifferenceOfSunkanSlab;
        }
        public static bool checkSunkanSlabLevelDiffrnce_Is_Greater(ObjectId sunkanSlabId)
        {
            bool flag = false;
            if (sunkanSlabId.IsValid == true)
            {
                double levelDifferenceHeight = getSunkanSlabLevelDifference(sunkanSlabId);

                if (levelDifferenceHeight > CommonModule.minValueOfSunkanLevelDiffrce)
                {
                    flag = true;
                }
            }        
            return flag;
        }
        public void drawSunkanSlabWallPanel(bool flagOfSunkanSlab, ObjectId sunkanSlabId, List<ObjectId> listOfWallPanelRect_ObjId, List<ObjectId> listOfWallPanelLine_ObjId, ObjectId wallPanelLineId)
        {
            //if (sunkanSlabId.IsValid == true)
            //{
            //    double levelDifferenceOfSunkanSlab = getSunkanSlabLevelDifference(sunkanSlabId);             

            //    bool flagOfGreater = getSunkanSlabLevelDiff_Is_LessThan_RC(levelDifferenceOfSunkanSlab);

            //    if (flagOfGreater == true)
            //    {
            //        var listOfSunkanSlabPanelRectId = AEE_Utility.copyColonEntity(CreateShellPlanHelper.moveVector_ForSunkanSlabLayout, listOfWallPanelRect_ObjId);

            //        WallPanelHelper wallPanelHlpr = new WallPanelHelper();

            //        List<ObjectId> listOfTextId = new List<ObjectId>();
            //        List<ObjectId> listOfCircleId = new List<ObjectId>();

            //        for (int i = 0; i < listOfSunkanSlabPanelRectId.Count; i++)
            //        {
            //            ObjectId panelLineId = listOfWallPanelLine_ObjId[i];
            //            ObjectId panelRectId = listOfSunkanSlabPanelRectId[i];

            //            double lineAngle = AEE_Utility.GetAngleOfLine(panelLineId);
            //            double lengthOfLine = AEE_Utility.GetLengthOfLine(panelLineId);

            //            lengthOfLine = Math.Round(lengthOfLine);

            //            var listOfVerticesOfPanelRectId = AEE_Utility.GetPolylineVertexPoint(panelRectId);
            //            double panelMin_X = listOfVerticesOfPanelRectId.Min(sortPoint => sortPoint.X);
            //            double panelMin_Y = listOfVerticesOfPanelRectId.Min(sortPoint => sortPoint.Y);
            //            double panelMax_X = listOfVerticesOfPanelRectId.Max(sortPoint => sortPoint.X);
            //            double panelMax_Y = listOfVerticesOfPanelRectId.Max(sortPoint => sortPoint.Y);

            //            double panelCntr_X = panelMin_X + ((panelMax_X - panelMin_X) / 2);
            //            double panelCntr_Y = panelMin_Y + ((panelMax_Y - panelMin_Y) / 2);
            //            var wallPanel_X_MidPoint = new Point3d(panelCntr_X, panelCntr_Y, 0);

            //            string sunkanSlabPanelText = Convert.ToString(lengthOfLine) + " " + "Kicker" + " " + Convert.ToString(levelDifferenceOfSunkanSlab);


            //            ObjectId dimTextId2 = wallPanelHlpr.writeDimensionTextInWallPanel(sunkanSlabPanelText, panelLineId, wallPanel_X_MidPoint, lineAngle, CommonModule.wallPanelTextLyrName, CommonModule.wallPanelLyrColor);
            //            listOfTextId.Add(dimTextId2);

            //            var circleId = wallPanelHlpr.createCircleInNonStandardWallPanel(panelCntr_X, panelCntr_Y, lengthOfLine);
            //            if (circleId.IsValid == true)
            //            {
            //                listOfCircleId.Add(circleId);
            //            }
            //        }
            //    }            
            //}
        }





    }
}
