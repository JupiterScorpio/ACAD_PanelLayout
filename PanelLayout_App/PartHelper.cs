using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanelLayout_App
{
    public class PartHelper
    {       
        public ObjectId drawInternalCorner(string xDataRegAppName, double startX, double startY, double flange1, double flange2, double flange3, double thick, double panelDepth, string layerName, int colorIndex, double rotAng)
        {

            var pline = new Polyline();
            var pt = Point2d.Origin; // initialize the start point to (0,0)           
            pline.AddVertexAt(0, pt, 0.0, 0.0, 0.0); // add the first vertex

            pt += new Vector2d(flange2, 0);  // flange_2 outer side y Straight line  
            pline.AddVertexAt(0, pt, 0.0, 0.0, 0.0);

            pt += new Vector2d(0, -panelDepth);  // panel depth right upper-side y straight line 
            pline.AddVertexAt(0, pt, 0.0, 0.0, 0.0);

            pt += new Vector2d(-thick, 0);  // thick right-side x straight line 
            pline.AddVertexAt(0, pt, 0.0, 0.0, 0.0);

            pt += new Vector2d(0, (panelDepth - thick));  // panel depth right upper-side y straight line 
            pline.AddVertexAt(0, pt, 0.0, 0.0, 0.0);

            pt += new Vector2d(-(flange2 - (2 * thick)), 0);  // panel depth right upper-side y straight line 
            pline.AddVertexAt(0, pt, 0.0, 0.0, 0.0);

            pt += new Vector2d(0, -(flange1 - (2 * thick))); //flange_1 inner side y Straight line
            pline.AddVertexAt(0, pt, 0.0, 0.0, 0.0);

            pt += new Vector2d((panelDepth - thick), 0);  // panel depth inner lower-side x straight line 
            pline.AddVertexAt(0, pt, 0.0, 0.0, 0.0);

            pt += new Vector2d(0, -thick);  // panel depth inner-thick, lower-side y straight line 
            pline.AddVertexAt(0, pt, 0.0, 0.0, 0.0);

            pt += new Vector2d(-panelDepth, 0);  // panel depth outer lower-side x straight line 
            pline.AddVertexAt(0, pt, 0.0, 0.0, 0.0);

            pt += new Vector2d(0, flange1); //flange_1 outer side y Straight line
            pline.AddVertexAt(0, pt, 0.0, 0.0, 0.0);

            pline.Closed = true;

            ObjectId intrnalCorner_Id = AEE_Utility.CreatePolyLine(pline, startX, startY, layerName, colorIndex);



            if (rotAng != 0)
                AEE_Utility.RotateEntity(intrnalCorner_Id, rotAng * Math.PI / 180, new Point3d(startX, startY, 0.0));

            return intrnalCorner_Id;
        }

        public ObjectId drawExternalCorner(string xDataRegAppName, double startX, double startY, double flange, double thick, string layerName, int colorIndex)
        {
            var pline = new Polyline();
            var pt = Point2d.Origin; // initialize the start point to (0,0)           
            pline.AddVertexAt(0, pt, 0.0, 0.0, 0.0); // add the first vertex

            pt += new Vector2d(0, -flange);  // flange outer side y Straight line  
            pline.AddVertexAt(0, pt, 0.0, 0.0, 0.0);

            pt += new Vector2d(thick, 0);  // flange thick lower-side x Straight line  
            pline.AddVertexAt(0, pt, 0.0, 0.0, 0.0);

            pt += new Vector2d(0, (flange - thick));  // flange inner side y Straight line  
            pline.AddVertexAt(0, pt, 0.0, 0.0, 0.0);

            pt += new Vector2d((flange - thick), 0);  // flange inner side x Straight line  
            pline.AddVertexAt(0, pt, 0.0, 0.0, 0.0);

            pt += new Vector2d(0, thick);  // flange thick upper-side y Straight line  
            pline.AddVertexAt(0, pt, 0.0, 0.0, 0.0);

            pt += new Vector2d(-(flange), 0);  // flange upper Y outer side x Straight line  
            pline.AddVertexAt(0, pt, 0.0, 0.0, 0.0);

            pline.Closed = true;

             ObjectId extrnalCorner_Id = AEE_Utility.CreatePolyLine(pline, startX, startY, layerName, colorIndex);

            

            Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor.UpdateScreen();
            //var ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
            //var res = ed.GetEntity("Select Entity : ");
            //if (res.Status != PromptStatus.OK)
            //{
                
            //}

            return extrnalCorner_Id;
        }
        
        public ObjectId drawBeamBottomInternalCorner(string xDataRegAppName, double startX, double startY, double flange1, double flange2, double thick1, double thick2, string layerName, int colorIndex)
        {
            var pline = new Polyline();
            var pt = Point2d.Origin; // initialize the start point to (0,0)           
            pline.AddVertexAt(0, pt, 0.0, 0.0, 0.0); // add the first vertex

            pt += new Vector2d(0, -flange1);  // flange outer side y Straight line  
            pline.AddVertexAt(0, pt, 0.0, 0.0, 0.0);

            pt += new Vector2d(thick1, 0);  // flange thick lower-side x Straight line  
            pline.AddVertexAt(0, pt, 0.0, 0.0, 0.0);

            pt += new Vector2d(0, (flange1 - thick2));  // flange inner side y Straight line  
            pline.AddVertexAt(0, pt, 0.0, 0.0, 0.0);

            pt += new Vector2d((flange2 - thick1), 0);  // flange inner side x Straight line  
            pline.AddVertexAt(0, pt, 0.0, 0.0, 0.0);

            pt += new Vector2d(0, thick2);  // flange thick upper-side y Straight line  
            pline.AddVertexAt(0, pt, 0.0, 0.0, 0.0);

            pt += new Vector2d(-(flange2), 0);  // flange upper Y outer side x Straight line  
            pline.AddVertexAt(0, pt, 0.0, 0.0, 0.0);

            pline.Closed = true;

            ObjectId extrnalCorner_Id = AEE_Utility.CreatePolyLine(pline, startX, startY, layerName, colorIndex);

            return extrnalCorner_Id;
        }
    }
}
