﻿using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using AcDb = Autodesk.AutoCAD.DatabaseServices;
//using Autodesk.AutoCAD.ExportLayout;
using AutoCADGraphicsInterface = Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.ExportLayout;
using Autodesk.AutoCAD.MacroRecorder;
using System.Windows;

namespace PanelLayout_App
{
    public enum eCustType //Added on 26/04/2023 by SDM
    {
        IHPanel, WNPanel
    }
    public static class AEE_Utility
    {
        public static bool ZoomObj = false;
        private static Document _activeDocument;
        private static Database _database;
        private static Editor _editor;
        private static Autodesk.AutoCAD.DatabaseServices.TransactionManager _transactionManager;

        public static eCustType CustType //Added on 26/04/2023 by SDM
        {
            get
            {
#if WNPANEL
                return eCustType.WNPanel;
#endif
                return eCustType.IHPanel;
            }
        }
        public static ObjectId createRectangle(double x1, double y1, double x2, double y2, string layerName, int colorIndex)
        {
            CreateLayer(layerName, colorIndex);
            // Get the current document and database
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
           
         
            ObjectId ObjectIDOfPolyline = new ObjectId();
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // Create Rectangle  
                using (Polyline polyLine = new Polyline())
                {
                    polyLine.AddVertexAt(0, new Point2d(x1, y1), 0.0, 0.0, 0.0);
                    polyLine.AddVertexAt(1, new Point2d(x2, y1), 0.0, 0.0, 0.0);
                    polyLine.AddVertexAt(2, new Point2d(x2, y2), 0.0, 0.0, 0.0);
                    polyLine.AddVertexAt(3, new Point2d(x1, y2), 0.0, 0.0, 0.0);

                    polyLine.ColorIndex = 256;
                    polyLine.Layer = layerName;
                    polyLine.TransformBy(ed.CurrentUserCoordinateSystem);
                    ObjectIDOfPolyline = acBlkTblRec.AppendEntity(polyLine);
                    acTrans.AddNewlyCreatedDBObject(polyLine, true);
                    polyLine.Closed = true;
                    ed.UpdateScreen();
                }
                // Save the new object to the database
                acTrans.Commit();
            }
            return ObjectIDOfPolyline;
        }

        internal static ObjectId CreateRectangle(Point3d p1, Point3d p2, Point3d p3, Point3d p4, string layerName, int colorIndex)
        {
            CreateLayer(layerName, colorIndex);
            ObjectId id = new ObjectId();

            InitializeTransaction();

            using (Transaction acTrans = _transactionManager.StartTransaction())
            {
                BlockTable blkTbl = acTrans.GetObject(_database.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord blkTblRec = acTrans.GetObject(blkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.AddVertexAt(0, new Point2d(p1.X, p1.Y), 0.0, 0.0, 0.0);
                    acPoly.AddVertexAt(1, new Point2d(p2.X, p2.Y), 0.0, 0.0, 0.0);
                    acPoly.AddVertexAt(2, new Point2d(p3.X, p3.Y), 0.0, 0.0, 0.0);
                    acPoly.AddVertexAt(3, new Point2d(p4.X, p4.Y), 0.0, 0.0, 0.0);

                    acPoly.Layer = layerName;
                    acPoly.ColorIndex = 256;
                    acPoly.Closed = true;

                    acPoly.TransformBy(_editor.CurrentUserCoordinateSystem);
                    id = blkTblRec.AppendEntity(acPoly);
                    acTrans.AddNewlyCreatedDBObject(acPoly, true);
                    _editor.UpdateScreen();
                }
                acTrans.Commit();
            }
            return id;
        }

        private static void InitializeTransaction()
        {
            _activeDocument = GetActiveDocument();
            _database = _activeDocument.Database;
            _editor = _activeDocument.Editor;
            _transactionManager = _database.TransactionManager;
        }
        private static Document GetActiveDocument() =>
           Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;

        internal static ObjectId CreateMultiLineText(string text, Point3d point3d, double textHeight, string layerName, int colorIndex, double rotationAngle, double fontSizeScale = 1)
        {
            ObjectId textId = new ObjectId();

            CreateLayer(layerName, colorIndex);

            //if (text == "
            //
            //WP 1950")
            //{
            //    MessageBox.Show("Test");
            //}

            InitializeTransaction();

            using (Transaction acTrans = _transactionManager.StartTransaction())
            {
                BlockTable acBlkTbl = acTrans.GetObject(_database.BlockTableId, OpenMode.ForRead) as BlockTable;

                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                using (MText acMText = new MText())
                {
                    // Open the current text style for write
                    TextStyleTableRecord acTextStyleTblRec;
                    acTextStyleTblRec = acTrans.GetObject(_database.Textstyle, OpenMode.ForWrite) as TextStyleTableRecord;

                    // Get the current font settings
                    AutoCADGraphicsInterface.FontDescriptor acFont = acTextStyleTblRec.Font;
                    acTextStyleTblRec.Font =
                        new AutoCADGraphicsInterface.FontDescriptor("Arial", acFont.Bold, acFont.Italic,
                       acFont.CharacterSet,
                        acFont.PitchAndFamily);

                    acMText.Location = point3d;
                    acMText.TextHeight = textHeight * fontSizeScale;
                    acMText.Attachment = AttachmentPoint.MiddleCenter;
                    acMText.Contents = text;
                    acMText.ColorIndex = 256;
                    acMText.Layer = layerName;
                    acMText.Rotation = (rotationAngle * Math.PI) / 180;

                    textId = acBlkTblRec.AppendEntity(acMText);
                    acTrans.AddNewlyCreatedDBObject(acMText, true);
                    _editor.UpdateScreen();
                }

                acTrans.Commit();
            }

            return textId;
        }
        public static bool IsLinesAreSame(ObjectId lineObj1, ObjectId lineObj2)
        {
            bool flag = false;
            int matchIndx;
            Line l1 = AEE_Utility.GetLine(lineObj1);

            List<Point3d> listOfPoints = GetStartEndPointOfLine(lineObj2);

            bool flagStartMatching = IsPoint3dPresentInPoint3dList(listOfPoints, l1.StartPoint, out matchIndx);

            bool flagEndMatching = IsPoint3dPresentInPoint3dList(listOfPoints, l1.EndPoint, out matchIndx);

            if (flagStartMatching && flagEndMatching)
                flag = true;

            return flag;
        }

        public static ObjectId createRectangle(Point3d p1, Point3d p2, Point3d p3, Point3d p4, string layerName, int colorIndex)
        {
            AEE_Utility.CreateLayer(layerName, colorIndex);
            ObjectId id = new ObjectId();

            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acDb = acDoc.Database;
            Editor ed = acDoc.Editor;

            using (Transaction acTrans = acDb.TransactionManager.StartTransaction())
            {
                BlockTable blkTbl = acTrans.GetObject(acDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord blkTblRec = acTrans.GetObject(blkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.AddVertexAt(0, new Point2d(p1.X, p1.Y), 0.0, 0.0, 0.0);
                    acPoly.AddVertexAt(1, new Point2d(p2.X, p2.Y), 0.0, 0.0, 0.0);
                    acPoly.AddVertexAt(2, new Point2d(p3.X, p3.Y), 0.0, 0.0, 0.0);
                    acPoly.AddVertexAt(3, new Point2d(p4.X, p4.Y), 0.0, 0.0, 0.0);

                    acPoly.Layer = layerName;
                    acPoly.ColorIndex = 256;
                    acPoly.Closed = true;

                    acPoly.TransformBy(ed.CurrentUserCoordinateSystem);
                    id = blkTblRec.AppendEntity(acPoly);
                    acTrans.AddNewlyCreatedDBObject(acPoly, true);
                    ed.UpdateScreen();
                }
                acTrans.Commit();
            }
            return id;
        }


        public static ObjectId createRectangle(List<Point3d> listOfVertexPoint, string layerName, int colorIndex)
        {
            CreateLayer(layerName, colorIndex);
            // Get the current document and database
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            ObjectId ObjectIDOfPolyline = new ObjectId();
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // Create Rectangle  
                using (Polyline polyLine = new Polyline())
                {
                    for (int index = 0; index < listOfVertexPoint.Count; index++)
                    {
                        Point3d vertexPoint = listOfVertexPoint[index];
                        polyLine.AddVertexAt(index, new Point2d(vertexPoint.X, vertexPoint.Y), 0.0, 0.0, 0.0);
                    }
                    polyLine.ColorIndex = 256;
                    polyLine.Layer = layerName;
                    polyLine.TransformBy(ed.CurrentUserCoordinateSystem);
                    ObjectIDOfPolyline = acBlkTblRec.AppendEntity(polyLine);
                    acTrans.AddNewlyCreatedDBObject(polyLine, true);
                    polyLine.Closed = true;
                    ed.UpdateScreen();
                }
                // Save the new object to the database
                acTrans.Commit();
            }
            return ObjectIDOfPolyline;
        }


        public static ObjectId createRectangleWithSameProperty(List<Point3d> listOfVertexPoint, Entity ent)
        {
            CreateLayer(ent.Layer, ent.ColorIndex);
            // Get the current document and database
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            ObjectId ObjectIDOfPolyline = new ObjectId();
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // Create Rectangle  
                using (Polyline polyLine = new Polyline())
                {
                    for (int index = 0; index < listOfVertexPoint.Count; index++)
                    {
                        Point3d vertexPoint = listOfVertexPoint[index];
                        polyLine.AddVertexAt(index, new Point2d(vertexPoint.X, vertexPoint.Y), 0.0, 0.0, 0.0);
                    }
                    polyLine.SetPropertiesFrom(ent);
                    polyLine.TransformBy(ed.CurrentUserCoordinateSystem);
                    ObjectIDOfPolyline = acBlkTblRec.AppendEntity(polyLine);
                    acTrans.AddNewlyCreatedDBObject(polyLine, true);
                    polyLine.Closed = true;
                    ed.UpdateScreen();
                }
                // Save the new object to the database
                acTrans.Commit();
            }
            return ObjectIDOfPolyline;
        }


        public static ObjectId GetRectangleId(List<Point3d> listOfVertexPoint, bool visible)
        {
            // Get the current document and database
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            ObjectId ObjectIDOfPolyline = new ObjectId();
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // Create Rectangle  
                using (Polyline polyLine = new Polyline())
                {
                    for (int index = 0; index < listOfVertexPoint.Count; index++)
                    {
                        Point3d vertexPoint = listOfVertexPoint[index];
                        polyLine.AddVertexAt(index, new Point2d(vertexPoint.X, vertexPoint.Y), 0.0, 0.0, 0.0);
                    }

                    polyLine.Visible = visible;
                    polyLine.TransformBy(ed.CurrentUserCoordinateSystem);
                    ObjectIDOfPolyline = acBlkTblRec.AppendEntity(polyLine);
                    acTrans.AddNewlyCreatedDBObject(polyLine, true);
                    polyLine.Closed = true;
                    ed.UpdateScreen();
                }
                // Save the new object to the database
                acTrans.Commit();
            }
            return ObjectIDOfPolyline;
        }


        public static ObjectId GetRectangleId(Point3d p1, Point3d p2, Point3d p3, Point3d p4, string layerName, int colorIndex, bool visible)
        {
            AEE_Utility.CreateLayer(layerName, colorIndex);
            ObjectId id = new ObjectId();

            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acDb = acDoc.Database;
            Editor ed = acDoc.Editor;

            using (Transaction acTrans = acDb.TransactionManager.StartTransaction())
            {
                BlockTable blkTbl = acTrans.GetObject(acDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord blkTblRec = acTrans.GetObject(blkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.Visible = visible;
                    acPoly.AddVertexAt(0, new Point2d(p1.X, p1.Y), 0.0, 0.0, 0.0);
                    acPoly.AddVertexAt(1, new Point2d(p2.X, p2.Y), 0.0, 0.0, 0.0);
                    acPoly.AddVertexAt(2, new Point2d(p3.X, p3.Y), 0.0, 0.0, 0.0);
                    acPoly.AddVertexAt(3, new Point2d(p4.X, p4.Y), 0.0, 0.0, 0.0);

                    acPoly.Layer = layerName;
                    acPoly.ColorIndex = 256;
                    acPoly.Closed = true;

                    acPoly.TransformBy(ed.CurrentUserCoordinateSystem);
                    id = blkTblRec.AppendEntity(acPoly);
                    acTrans.AddNewlyCreatedDBObject(acPoly, true);
                    ed.UpdateScreen();
                }
                acTrans.Commit();
            }
            return id;
        }


        public static ObjectId GetRectangleId(Point3d p1, Point3d p2, string layerName, int colorIndex)
        {
            AEE_Utility.CreateLayer(layerName, colorIndex);
            ObjectId id = new ObjectId();

            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acDb = acDoc.Database;
            Editor ed = acDoc.Editor;

            using (Transaction acTrans = acDb.TransactionManager.StartTransaction())
            {
                BlockTable blkTbl = acTrans.GetObject(acDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord blkTblRec = acTrans.GetObject(blkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                var x1 = p1.X;
                var x2 = p2.X;
                var y1 = p1.Y;
                var y2 = p2.Y;
                using (Polyline acPoly = new Polyline())
                {
                    acPoly.Visible = true;
                    acPoly.AddVertexAt(0, new Point2d(x1, y1), 0.0, 0.0, 0.0);
                    acPoly.AddVertexAt(1, new Point2d(x2, y1), 0.0, 0.0, 0.0);
                    acPoly.AddVertexAt(2, new Point2d(x2, y2), 0.0, 0.0, 0.0);
                    acPoly.AddVertexAt(3, new Point2d(x1, y2), 0.0, 0.0, 0.0);

                    acPoly.Layer = layerName;
                    acPoly.ColorIndex = 256;
                    acPoly.Closed = true;

                    acPoly.TransformBy(ed.CurrentUserCoordinateSystem);
                    id = blkTblRec.AppendEntity(acPoly);
                    acTrans.AddNewlyCreatedDBObject(acPoly, true);
                    ed.UpdateScreen();
                }
                acTrans.Commit();
            }
            return id;
        }


        public static Entity GetEntityOfRectangle(double x1, double y1, double x2, double y2)
        {
            Polyline polyLine = new Polyline();
            polyLine.AddVertexAt(0, new Point2d(x1, y1), 0.0, 0.0, 0.0);
            polyLine.AddVertexAt(1, new Point2d(x2, y1), 0.0, 0.0, 0.0);
            polyLine.AddVertexAt(2, new Point2d(x2, y2), 0.0, 0.0, 0.0);
            polyLine.AddVertexAt(3, new Point2d(x1, y2), 0.0, 0.0, 0.0);
            polyLine.Closed = true;

            return polyLine;
        }


        public static void CreateLayer(string sLayerName, int colorIndex)
        {
            short colorInderInShort = Convert.ToInt16(colorIndex);

            Autodesk.AutoCAD.ApplicationServices.Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;

            Database acCurDb = doc.Database;

            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                LayerTable acLyrTbl = acTrans.GetObject(acCurDb.LayerTableId, OpenMode.ForRead) as LayerTable;

                if (acLyrTbl.Has(sLayerName) == false)
                {
                    using (LayerTableRecord acLyrTblRec = new LayerTableRecord())
                    {
                        acLyrTblRec.Color = Autodesk.AutoCAD.Colors.Color.FromColorIndex(ColorMethod.ByAci, colorInderInShort);
                        acLyrTblRec.Name = sLayerName;
                        if (sLayerName == "WP_H_CT_Line_STD" || sLayerName == "WP_V_CT_Line_STD" || sLayerName == "WP_H_CT_Line_SP" || sLayerName == "SJ_CT_Line_STD")
                        {
                            LinetypeTable linetypetable = acTrans.GetObject(acCurDb.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;

                            if (linetypetable.Has("Center") == true)
                            {
                                acLyrTblRec.LinetypeObjectId = linetypetable["Center"];
                            }
                        }

                        acLyrTbl.UpgradeOpen();
                        acLyrTbl.Add(acLyrTblRec);
                        acTrans.AddNewlyCreatedDBObject(acLyrTblRec, true);
                    }
                }
                acTrans.Commit();
            }
        }

        public static void CreateLayerFromUI(string sLayerName, int colorIndex)
        {
            try
            {
                short colorInderInShort = Convert.ToInt16(colorIndex);
                Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                Database db = doc.Database;
                Editor ed = doc.Editor;
                //lock the document
                using (DocumentLock docLock = doc.LockDocument())
                {
                    using (Transaction acTrans = db.TransactionManager.StartTransaction())
                    {
                        ObjectId ModelSpaceId = SymbolUtilityServices.GetBlockModelSpaceId(db);

                        BlockTableRecord acBlkTblRec = acTrans.GetObject(ModelSpaceId, OpenMode.ForWrite) as BlockTableRecord;

                        LayerTable acLyrTbl = acTrans.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;

                        if (acLyrTbl.Has(sLayerName) == false)
                        {
                            using (LayerTableRecord acLyrTblRec = new LayerTableRecord())
                            {
                                acLyrTblRec.Color = Autodesk.AutoCAD.Colors.Color.FromColorIndex(ColorMethod.ByAci, colorInderInShort);
                                acLyrTblRec.Name = sLayerName;
                                acLyrTbl.UpgradeOpen();
                                acLyrTbl.Add(acLyrTblRec);
                                acTrans.AddNewlyCreatedDBObject(acLyrTblRec, true);
                            }
                        }
                        acTrans.Commit();
                    }
                }
            }
            catch (System.Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
        }


        public static ObjectId CreateLine(double X1, double Y1, double Z1, double X2, double Y2, double Z2, string layerName, int colorIndex)
        {
            CreateLayer(layerName, colorIndex);
            // Get the current document and database
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            ObjectId lineId = new ObjectId();
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // Create a line that starts at 5,5 and ends at 12,3
                using (Line acLine = new Line(new Point3d(X1, Y1, Z1), new Point3d(X2, Y2, Z2)))
                {
                    acLine.ColorIndex = 256;
                    acLine.Layer = layerName;
                    lineId = acBlkTblRec.AppendEntity(acLine);
                    acTrans.AddNewlyCreatedDBObject(acLine, true);
                    ed.UpdateScreen();
                }

                // Save the new object to the database
                acTrans.Commit();
            }
            return lineId;
        }
        public static Entity GetLineEntity(double X1, double Y1, double Z1, double X2, double Y2, double Z2, string layerName, int colorIndex)
        {
            AEE_Utility.CreateLayer(layerName, colorIndex);
            Line acLine = new Line(new Point3d(X1, Y1, Z1), new Point3d(X2, Y2, Z2));
            acLine.Layer = layerName;
            acLine.ColorIndex = 256;
            return acLine;
        }

        public static ObjectId createLineWithPolyline(Point2d point1, Point2d point2, string layerName, int colorIndex)
        {
            CreateLayer(layerName, colorIndex);
            // Get the current document and database
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            ObjectId ObjectIDOfPolyline = new ObjectId();
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // Create Rectangle  
                using (Polyline polyLine = new Polyline())
                {
                    polyLine.AddVertexAt(0, point1, 0.0, 0.0, 0.0);
                    polyLine.AddVertexAt(1, point2, 0.0, 0.0, 0.0);
                    polyLine.ColorIndex = 256;
                    polyLine.Layer = layerName;
                    polyLine.TransformBy(ed.CurrentUserCoordinateSystem);
                    ObjectIDOfPolyline = acBlkTblRec.AppendEntity(polyLine);
                    acTrans.AddNewlyCreatedDBObject(polyLine, true);
                    ed.UpdateScreen();
                }
                // Save the new object to the database
                acTrans.Commit();
            }
            return ObjectIDOfPolyline;
        }


        public static Entity GetEntityLine(double X1, double Y1, double Z1, double X2, double Y2, double Z2)
        {
            Line acLine = new Line(new Point3d(X1, Y1, Z1), new Point3d(X2, Y2, Z2));
            return acLine;
        }
        public static ObjectId CreateLine(Point3d startPoint, Point3d endPoint, string layerName, int colorIndex)
        {
            CreateLayer(layerName, colorIndex);
            // Get the current document and database
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            ObjectId lineObjId = new ObjectId();
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // Create a line that starts at 5,5 and ends at 12,3
                using (Line acLine = new Line(startPoint, endPoint))
                {
                    // // Add the new object to the block table record and the transaction

                    acLine.ColorIndex = 256;
                    acLine.Layer = layerName;
                    lineObjId = acBlkTblRec.AppendEntity(acLine);
                    acTrans.AddNewlyCreatedDBObject(acLine, true);
                    ed.UpdateScreen();
                }
                // Save the new object to the database
                acTrans.Commit();
            }
            return lineObjId;
        }

        public static ObjectId CreateArc(double X, double Y, double Z, double radian, double startAngle, double endAngle, string layerName, int colorIndex)
        {
            CreateLayer(layerName, colorIndex);
            // Get the current document and database
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            ObjectId ObjectIDLIne;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                startAngle = (startAngle * Math.PI) / 180;
                endAngle = (endAngle * Math.PI) / 180;

                using (Arc acArc = new Arc(new Point3d(X, Y, Z), radian, startAngle, endAngle))
                {
                    // Add the new object to the block table record and the transaction
                    acArc.ColorIndex = 256;
                    acArc.Layer = layerName;
                    ObjectIDLIne = acBlkTblRec.AppendEntity(acArc);
                    acTrans.AddNewlyCreatedDBObject(acArc, true);
                    ed.UpdateScreen();
                }
                // Save the new line to the database
                acTrans.Commit();
            }
            return ObjectIDLIne;
        }

        public static double GetLengthOfLine(double X1, double Y1, double Z1, double X2, double Y2, double Z2)
        {
            double length = 0;
            using (Line acLine = new Line(new Point3d(X1, Y1, Z1), new Point3d(X2, Y2, Z2)))
            {
                length = acLine.Length;
            }

            return length;
        }
        public static double GetLengthOfLine(Point3d startPoint, Point3d endPoint)
        {
            double length = 0;
            using (Line acLine = new Line(startPoint, endPoint))
            {
                length = acLine.Length;
            }

            return length;
        }
        public static double GetLengthOfLine(Point2d startPoint, Point2d endPoint)
        {
            double length = 0;
            using (Line acLine = new Line(new Point3d(startPoint.X, startPoint.Y, 0), new Point3d(endPoint.X, endPoint.Y, 0)))
            {
                length = acLine.Length;
            }

            return length;
        }

        public static ObjectId CreateLine(double X1, double Y1, double Z1, double X2, double Y2, double Z2, string layerName, int colorIndex, string sLineTypName, int lineTypeScale)
        {
            CreateLayer(layerName, colorIndex);
            // Get the current document and database
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            ObjectId ObjectIDLIne;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Linetype table for read              
                LinetypeTable acLineTypTbl = acTrans.GetObject(acCurDb.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;

                if (acLineTypTbl.Has(sLineTypName) == false)
                {
                    acCurDb.LoadLineTypeFile(sLineTypName, "acad.lin");
                }

                // Open the Block table for read           
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // Create a line that starts at 5,5 and ends at 12,3
                using (Line acLine = new Line(new Point3d(X1, Y1, Z1), new Point3d(X2, Y2, Z2)))
                {
                    acLine.Linetype = sLineTypName;
                    acLine.LinetypeScale = lineTypeScale;
                    acLine.ColorIndex = 256;
                    acLine.Layer = layerName;
                    ObjectIDLIne = acBlkTblRec.AppendEntity(acLine);
                    acTrans.AddNewlyCreatedDBObject(acLine, true);
                    ed.UpdateScreen();
                }

                // Save the new object to the database
                acTrans.Commit();
            }
            return ObjectIDLIne;
        }

        public static ObjectId CreateLine(Line acLine, string layerName, int colorIndex, string sLineTypName, int lineTypeScale)
        {
            CreateLayer(layerName, colorIndex);
            // Get the current document and database
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;

            ObjectId ObjectIDLIne;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Linetype table for read              
                LinetypeTable acLineTypTbl = acTrans.GetObject(acCurDb.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;

                if (acLineTypTbl.Has(sLineTypName) == false)
                {
                    acCurDb.LoadLineTypeFile(sLineTypName, "acad.lin");
                }

                // Open the Block table for read           
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                acLine.Linetype = sLineTypName;
                acLine.LinetypeScale = lineTypeScale;
                acLine.ColorIndex = 256;
                acLine.Layer = layerName;
                ObjectIDLIne = acBlkTblRec.AppendEntity(acLine);
                acTrans.AddNewlyCreatedDBObject(acLine, true);
                ed.UpdateScreen();
                // Save the new object to the database
                acTrans.Commit();

            }
            return ObjectIDLIne;
        }

        public static ObjectId CreateArc(double X, double Y, double Z, double radian, double startAngle, double endAngle, string layerName, int colorIndex, string sLineTypName, int lineTypeScale)
        {
            CreateLayer(layerName, colorIndex);
            // Get the current document and database
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            ObjectId ObjectIDLIne;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Linetype table for read
                LinetypeTable acLineTypTbl = acTrans.GetObject(acCurDb.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;

                if (acLineTypTbl.Has(sLineTypName) == false)
                {
                    acCurDb.LoadLineTypeFile(sLineTypName, "acad.lin");
                }

                // Open the Block table for read           
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                startAngle = (startAngle * Math.PI) / 180;
                endAngle = (endAngle * Math.PI) / 180;

                using (Arc acArc = new Arc(new Point3d(X, Y, Z), radian, startAngle, endAngle))
                {
                    // Add the new object to the block table record and the transaction
                    acArc.Linetype = sLineTypName;
                    acArc.LinetypeScale = lineTypeScale;
                    acArc.ColorIndex = 256;
                    acArc.Layer = layerName;
                    ObjectIDLIne = acBlkTblRec.AppendEntity(acArc);
                    acTrans.AddNewlyCreatedDBObject(acArc, true);
                    ed.UpdateScreen();
                }
                // Save the new line to the database
                acTrans.Commit();
            }
            return ObjectIDLIne;
        }
        public static ObjectId CreateArc(Arc acArc, string layerName, int colorIndex, string sLineTypName, int lineTypeScale)
        {
            CreateLayer(layerName, colorIndex);
            // Get the current document and database
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;

            ObjectId ObjectIDLIne;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Linetype table for read
                LinetypeTable acLineTypTbl = acTrans.GetObject(acCurDb.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;

                if (acLineTypTbl.Has(sLineTypName) == false)
                {
                    acCurDb.LoadLineTypeFile(sLineTypName, "acad.lin");
                }

                // Open the Block table for read           
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // Add the new object to the block table record and the transaction
                acArc.Linetype = sLineTypName;
                acArc.LinetypeScale = lineTypeScale;
                acArc.ColorIndex = 256;
                acArc.Layer = layerName;
                ObjectIDLIne = acBlkTblRec.AppendEntity(acArc);
                acTrans.AddNewlyCreatedDBObject(acArc, true);
                ed.UpdateScreen();

                // Save the new line to the database
                acTrans.Commit();
            }
            return ObjectIDLIne;
        }
        public static ObjectId GetArcId(Arc acArc)
        {
            // Get the current document and database
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            ObjectId ObjectIDLIne;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read           
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                ObjectIDLIne = acBlkTblRec.AppendEntity(acArc);
                acTrans.AddNewlyCreatedDBObject(acArc, true);

                // Save the new line to the database
                acTrans.Commit();
            }
            return ObjectIDLIne;
        }

        public static ObjectId GetArcId(double X, double Y, double Z, double radian, double startAngle, double endAngle, bool visible)
        {
            // Get the current document and database
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            ObjectId ObjectIDLIne;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read           
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                startAngle = (startAngle * Math.PI) / 180;
                endAngle = (endAngle * Math.PI) / 180;

                using (Arc acArc = new Arc(new Point3d(X, Y, Z), radian, startAngle, endAngle))
                {
                    acArc.Visible = visible;
                    ObjectIDLIne = acBlkTblRec.AppendEntity(acArc);
                    acTrans.AddNewlyCreatedDBObject(acArc, true);
                }
                // Save the new line to the database
                acTrans.Commit();
            }
            return ObjectIDLIne;
        }
        public static ObjectId GetRectangleId(double x1, double y1, double x2, double y2, bool visible)
        {
            // Get the current document and database
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            ObjectId ObjectIDOfPolyline;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // Create Rectangle  
                using (Polyline polyLine = new Polyline())
                {
                    polyLine.Visible = visible;
                    polyLine.AddVertexAt(0, new Point2d(x1, y1), 0.0, 0.0, 0.0);
                    polyLine.AddVertexAt(1, new Point2d(x2, y1), 0.0, 0.0, 0.0);
                    polyLine.AddVertexAt(2, new Point2d(x2, y2), 0.0, 0.0, 0.0);
                    polyLine.AddVertexAt(3, new Point2d(x1, y2), 0.0, 0.0, 0.0);

                    polyLine.TransformBy(ed.CurrentUserCoordinateSystem);
                    ObjectIDOfPolyline = acBlkTblRec.AppendEntity(polyLine);
                    acTrans.AddNewlyCreatedDBObject(polyLine, true);
                    polyLine.Closed = true;
                }
                // Save the new object to the database
                acTrans.Commit();
            }
            return ObjectIDOfPolyline;
        }
        public static ObjectId CreateArcWithoutByLayer(Point3d centerArcPoint, Point3d startAnglePoint, Point3d endAnglePoint, double radius, string layerName, int colorIndex, string sLineTypName, int lineTypeScale)
        {
            CreateLayer(layerName, colorIndex);
            // Get the current document and database
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            ObjectId ObjectIDLIne = new ObjectId();
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Linetype table for read
                LinetypeTable acLineTypTbl;
                acLineTypTbl = acTrans.GetObject(acCurDb.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;

                if (acLineTypTbl.Has(sLineTypName) == false)
                {
                    acCurDb.LoadLineTypeFile(sLineTypName, "acad.lin");
                }

                // Open the Block table for read           
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                double startAngle = getAngleInRadianOfLine(centerArcPoint.X, centerArcPoint.Y, startAnglePoint.X, startAnglePoint.Y);
                double endAngle = getAngleInRadianOfLine(centerArcPoint.X, centerArcPoint.Y, endAnglePoint.X, endAnglePoint.Y);

                using (Arc acArcEnt = new Arc(centerArcPoint, radius, startAngle, endAngle))
                {
                    Point2d startPoint = new Point2d(startAnglePoint.X, startAnglePoint.Y);
                    Point2d endPoint = new Point2d(endAnglePoint.X, endAnglePoint.Y);
                    Point3d midPoint = GetMidPoint(acArcEnt);
                    Point2d pointOnArc = new Point2d(midPoint.X, midPoint.Y);
                    CircularArc2d arc2d = new CircularArc2d(startPoint, pointOnArc, endPoint);

                    var plane = new Plane(Point3d.Origin, Vector3d.ZAxis);

                    Point3d cntrPoint = new Point3d(arc2d.Center.X, arc2d.Center.Y, 0);
                    double angle = new Vector3d(plane, arc2d.ReferenceVector).AngleOnPlane(plane);
                    using (Arc acArc = new Arc(new Point3d(plane, arc2d.Center), arc2d.Radius, arc2d.StartAngle + angle, arc2d.EndAngle + angle))
                    {
                        // Add the new object to the block table record and the transaction
                        acArc.Linetype = sLineTypName;
                        acArc.LinetypeScale = lineTypeScale;
                        acArc.ColorIndex = colorIndex;
                        acArc.Layer = layerName;
                        ObjectIDLIne = acBlkTblRec.AppendEntity(acArc);
                        acTrans.AddNewlyCreatedDBObject(acArc, true);
                        ed.UpdateScreen();
                    }
                }
                // Save the new line to the database
                acTrans.Commit();
            }
            return ObjectIDLIne;
        }
        public static ObjectId CreateLineWithoutByLayer(double X1, double Y1, double Z1, double X2, double Y2, double Z2, string layerName, int colorIndex, string sLineTypName, int lineTypeScale)
        {
            CreateLayer(layerName, colorIndex);
            // Get the current document and database
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            ObjectId ObjectIDLIne;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Linetype table for read              
                LinetypeTable acLineTypTbl = acTrans.GetObject(acCurDb.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;

                if (acLineTypTbl.Has(sLineTypName) == false)
                {
                    acCurDb.LoadLineTypeFile(sLineTypName, "acad.lin");
                }

                // Open the Block table for read           
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // Create a line that starts at 5,5 and ends at 12,3
                using (Line acLine = new Line(new Point3d(X1, Y1, Z1), new Point3d(X2, Y2, Z2)))
                {
                    acLine.Linetype = sLineTypName;
                    acLine.LinetypeScale = lineTypeScale;
                    acLine.ColorIndex = colorIndex;
                    acLine.Layer = layerName;
                    ObjectIDLIne = acBlkTblRec.AppendEntity(acLine);
                    acTrans.AddNewlyCreatedDBObject(acLine, true);
                    ed.UpdateScreen();
                }

                // Save the new object to the database
                acTrans.Commit();
            }
            return ObjectIDLIne;
        }
        public static ObjectId CreateArcWithoutByLayer(double X, double Y, double Z, double radian, double startAngle, double endAngle, string layerName, int colorIndex, string sLineTypName, int lineTypeScale)
        {
            CreateLayer(layerName, colorIndex);
            // Get the current document and database
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            ObjectId ObjectIDLIne;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Linetype table for read
                LinetypeTable acLineTypTbl;
                acLineTypTbl = acTrans.GetObject(acCurDb.LinetypeTableId,
                                                    OpenMode.ForRead) as LinetypeTable;

                if (acLineTypTbl.Has(sLineTypName) == false)
                {
                    acCurDb.LoadLineTypeFile(sLineTypName, "acad.lin");
                }

                // Open the Block table for read           
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                startAngle = (startAngle * Math.PI) / 180;
                endAngle = (endAngle * Math.PI) / 180;

                using (Arc acArc = new Arc(new Point3d(X, Y, Z), radian, startAngle, endAngle))
                {
                    // Add the new object to the block table record and the transaction
                    acArc.Linetype = sLineTypName;
                    acArc.LinetypeScale = lineTypeScale;
                    acArc.ColorIndex = colorIndex;
                    acArc.Layer = layerName;
                    ObjectIDLIne = acBlkTblRec.AppendEntity(acArc);
                    acTrans.AddNewlyCreatedDBObject(acArc, true);
                    ed.UpdateScreen();
                }
                // Save the new line to the database
                acTrans.Commit();
            }
            return ObjectIDLIne;
        }

        public static ObjectId CreateArc(Point3d centerArcPoint, Point3d startAnglePoint, Point3d endAnglePoint, double radius, string layerName, int colorIndex, string sLineTypName, double lineTypeScale)
        {
            CreateLayer(layerName, colorIndex);
            // Get the current document and database
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;

            ObjectId ObjectIDLIne = new ObjectId();
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Linetype table for read
                LinetypeTable acLineTypTbl;
                acLineTypTbl = acTrans.GetObject(acCurDb.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;

                if (acLineTypTbl.Has(sLineTypName) == false)
                {
                    acCurDb.LoadLineTypeFile(sLineTypName, "acad.lin");
                }

                // Open the Block table for read           
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                double startAngle = getAngleInRadianOfLine(centerArcPoint.X, centerArcPoint.Y, startAnglePoint.X, startAnglePoint.Y);
                double endAngle = getAngleInRadianOfLine(centerArcPoint.X, centerArcPoint.Y, endAnglePoint.X, endAnglePoint.Y);

                using (Arc acArcEnt = new Arc(centerArcPoint, radius, startAngle, endAngle))
                {
                    Point2d startPoint = new Point2d(startAnglePoint.X, startAnglePoint.Y);
                    Point2d endPoint = new Point2d(endAnglePoint.X, endAnglePoint.Y);
                    Point3d midPoint = GetMidPoint(acArcEnt);
                    Point2d pointOnArc = new Point2d(midPoint.X, midPoint.Y);
                    CircularArc2d arc2d = new CircularArc2d(startPoint, pointOnArc, endPoint);

                    var plane = new Plane(Point3d.Origin, Vector3d.ZAxis);

                    Point3d cntrPoint = new Point3d(arc2d.Center.X, arc2d.Center.Y, 0);
                    double angle = new Vector3d(plane, arc2d.ReferenceVector).AngleOnPlane(plane);
                    using (Arc acArc = new Arc(new Point3d(plane, arc2d.Center), arc2d.Radius, arc2d.StartAngle + angle, arc2d.EndAngle + angle))
                    {
                        // Add the new object to the block table record and the transaction
                        acArc.Linetype = sLineTypName;
                        acArc.LinetypeScale = lineTypeScale;
                        acArc.ColorIndex = 256;
                        acArc.Layer = layerName;
                        ObjectIDLIne = acBlkTblRec.AppendEntity(acArc);
                        acTrans.AddNewlyCreatedDBObject(acArc, true);
                        ed.UpdateScreen();
                    }
                }
                // Save the new line to the database
                acTrans.Commit();
            }
            return ObjectIDLIne;
        }

        public static ObjectId CreateArc(Point3d centerArcPoint, Point3d startAnglePoint, Point3d endAnglePoint, double radius, string layerName, int colorIndex)
        {
            CreateLayer(layerName, colorIndex);
            // Get the current document and database
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;

            ObjectId ObjectIDLIne = new ObjectId();
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read           
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                double startAngle = getAngleInRadianOfLine(centerArcPoint.X, centerArcPoint.Y, startAnglePoint.X, startAnglePoint.Y);
                double endAngle = getAngleInRadianOfLine(centerArcPoint.X, centerArcPoint.Y, endAnglePoint.X, endAnglePoint.Y);

                using (Arc acArcEnt = new Arc(centerArcPoint, radius, startAngle, endAngle))
                {
                    Point2d startPoint = new Point2d(startAnglePoint.X, startAnglePoint.Y);
                    Point2d endPoint = new Point2d(endAnglePoint.X, endAnglePoint.Y);
                    Point3d midPoint = GetMidPoint(acArcEnt);
                    Point2d pointOnArc = new Point2d(midPoint.X, midPoint.Y);
                    CircularArc2d arc2d = new CircularArc2d(startPoint, pointOnArc, endPoint);

                    var plane = new Plane(Point3d.Origin, Vector3d.ZAxis);

                    Point3d cntrPoint = new Point3d(arc2d.Center.X, arc2d.Center.Y, 0);
                    double angle = new Vector3d(plane, arc2d.ReferenceVector).AngleOnPlane(plane);
                    using (Arc acArc = new Arc(new Point3d(plane, arc2d.Center), arc2d.Radius, arc2d.StartAngle + angle, arc2d.EndAngle + angle))
                    {
                        // Add the new object to the block table record and the transaction                       
                        acArc.ColorIndex = 256;
                        acArc.Layer = layerName;
                        ObjectIDLIne = acBlkTblRec.AppendEntity(acArc);
                        acTrans.AddNewlyCreatedDBObject(acArc, true);
                        ed.UpdateScreen();
                    }
                }
                // Save the new line to the database
                acTrans.Commit();
            }
            return ObjectIDLIne;
        }

        internal static void setPolyLinePts(List<Point3d> lstPts, ObjectId cornerId)
        {
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = acDoc.Editor;
            Database db = HostApplicationServices.WorkingDatabase;

            using (Transaction acTrans = db.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                Entity acEnt = acTrans.GetObject(cornerId, OpenMode.ForWrite) as Entity;
                Polyline acPoly = acEnt as Polyline;
                while (acPoly.NumberOfVertices > 1)
                {
                    acPoly.RemoveVertexAt(1);
                }
                for (var n = 1; n < lstPts.Count; ++n)
                {
                    var pt = lstPts[n];
                    acPoly.AddVertexAt(n, new Point2d(pt.X, pt.Y), 0, 0, 0);
                }

                acTrans.Commit();
            }

        }

        public static Entity GetArcEntity(Point3d centerArcPoint, Point3d startAnglePoint, Point3d endAnglePoint, double radius, string layerName, int colorIndex)
        {
            CreateLayer(layerName, colorIndex);

            double startAngle = getAngleInRadianOfLine(centerArcPoint.X, centerArcPoint.Y, startAnglePoint.X, startAnglePoint.Y);
            double endAngle = getAngleInRadianOfLine(centerArcPoint.X, centerArcPoint.Y, endAnglePoint.X, endAnglePoint.Y);

            Arc acArcEnt = new Arc(centerArcPoint, radius, startAngle, endAngle);

            Point2d startPoint = new Point2d(startAnglePoint.X, startAnglePoint.Y);
            Point2d endPoint = new Point2d(endAnglePoint.X, endAnglePoint.Y);
            Point3d midPoint = GetMidPoint(acArcEnt);
            Point2d pointOnArc = new Point2d(midPoint.X, midPoint.Y);
            CircularArc2d arc2d = new CircularArc2d(startPoint, pointOnArc, endPoint);

            var plane = new Plane(Point3d.Origin, Vector3d.ZAxis);

            Point3d cntrPoint = new Point3d(arc2d.Center.X, arc2d.Center.Y, 0);
            double angle = new Vector3d(plane, arc2d.ReferenceVector).AngleOnPlane(plane);
            Arc acArc = new Arc(new Point3d(plane, arc2d.Center), arc2d.Radius, arc2d.StartAngle + angle, arc2d.EndAngle + angle);

            // Add the new object to the block table record and the transaction                       
            acArc.ColorIndex = 256;
            acArc.Layer = layerName;

            return acArc;
        }


        public static ObjectId GetArcId(Point3d centerArcPoint, Point3d startAnglePoint, Point3d endAnglePoint, double radius, bool visible)
        {
            // Get the current document and database
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            ObjectId ObjectIDLIne = new ObjectId();
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read           
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                double startAngle = getAngleInRadianOfLine(centerArcPoint.X, centerArcPoint.Y, startAnglePoint.X, startAnglePoint.Y);
                double endAngle = getAngleInRadianOfLine(centerArcPoint.X, centerArcPoint.Y, endAnglePoint.X, endAnglePoint.Y);

                using (Arc acArcEnt = new Arc(centerArcPoint, radius, startAngle, endAngle))
                {
                    Point2d startPoint = new Point2d(startAnglePoint.X, startAnglePoint.Y);
                    Point2d endPoint = new Point2d(endAnglePoint.X, endAnglePoint.Y);
                    Point3d midPoint = GetMidPoint(acArcEnt);
                    Point2d pointOnArc = new Point2d(midPoint.X, midPoint.Y);
                    CircularArc2d arc2d = new CircularArc2d(startPoint, pointOnArc, endPoint);

                    var plane = new Plane(Point3d.Origin, Vector3d.ZAxis);

                    Point3d cntrPoint = new Point3d(arc2d.Center.X, arc2d.Center.Y, 0);
                    double angle = new Vector3d(plane, arc2d.ReferenceVector).AngleOnPlane(plane);
                    using (Arc acArc = new Arc(new Point3d(plane, arc2d.Center), arc2d.Radius, arc2d.StartAngle + angle, arc2d.EndAngle + angle))
                    {
                        acArc.Visible = visible;
                        ObjectIDLIne = acBlkTblRec.AppendEntity(acArc);
                        acTrans.AddNewlyCreatedDBObject(acArc, true);
                    }
                }
                // Save the new line to the database
                acTrans.Commit();
            }
            return ObjectIDLIne;
        }
        public static double GetArcLength(Point3d centerArcPoint, Point3d startAnglePoint, Point3d endAnglePoint, double radius)
        {
            double length = 0;
            double startAngle = getAngleInRadianOfLine(centerArcPoint.X, centerArcPoint.Y, startAnglePoint.X, startAnglePoint.Y);
            double endAngle = getAngleInRadianOfLine(centerArcPoint.X, centerArcPoint.Y, endAnglePoint.X, endAnglePoint.Y);

            using (Arc acArc = new Arc(centerArcPoint, radius, startAngle, endAngle))
            {
                length = acArc.Length;
            }
            return length;
        }
        public static double GetArcLength(double X, double Y, double Z, double radian, double startAngle, double endAngle)
        {
            double length = 0;
            startAngle = (startAngle * Math.PI) / 180;
            endAngle = (endAngle * Math.PI) / 180;

            using (Arc acArc = new Arc(new Point3d(X, Y, Z), radian, startAngle, endAngle))
            {
                length = acArc.Length;
            }
            return length;
        }

        public static Point3dCollection GetPolyLinePoints(ObjectId id)
        {
            var pts = new Point3dCollection();
            var obj = id.Open(OpenMode.ForRead);
            if (obj != null)
            {
                var pl = obj as Polyline;

                for (var n = 0; n < pl.NumberOfVertices; ++n)
                {
                    var pt = pl.GetPoint3dAt(n);
                    pts.Add(pt);
                }
                if (pl.Closed)
                {
                    pts.Add(pl.GetPoint3dAt(0));
                }
                obj.Close();
            }
            return pts;
        }
        public static List<Point3d> GetPolyLinePointList(ObjectId id)
        {
            var pts = new List<Point3d>();
            var obj = id.Open(OpenMode.ForRead);
            if (obj != null)
            {
                var pl = obj as Polyline;
                GetPolylinePoints(pl, pts);
                obj.Close();
            }
            return pts;
        }

        public static void GetPolylinePoints(Polyline pl, List<Point3d> pts)
        {
            for (var n = 0; n < pl.NumberOfVertices; ++n)
            {
                var pt = pl.GetPoint3dAt(n);
                pts.Add(pt);
            }
            if (pl.Closed)
            {
                pts.Add(pl.GetPoint3dAt(0));
            }
        }

        public static ObjectId GetPointId(double X, double Y, double Z, string layerName, int colorIndex, bool visible)
        {
            CreateLayer(layerName, colorIndex);

            ObjectId objId = new ObjectId();
            // Get the current document and database
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // Create a point at (4, 3, 0) in Model space
                using (DBPoint dbPoint = new DBPoint(new Point3d(X, Y, Z)))
                {
                    // Add the new object to the block table record and the transaction
                    dbPoint.ColorIndex = 256;
                    dbPoint.Layer = layerName;
                    dbPoint.Visible = visible;
                    objId = acBlkTblRec.AppendEntity(dbPoint);
                    acTrans.AddNewlyCreatedDBObject(dbPoint, true);
                }
                // Save the new object to the database
                acTrans.Commit();
            }
            return objId;
        }

        public static double getAngleInRadianOfLine(double X1, double Y1, double X2, double Y2)
        {
            double angle = 0;

            using (Line acLine = new Line(new Point3d(X1, Y1, 0), new Point3d(X2, Y2, 0)))
            {
                angle = acLine.Angle;
            }
            return angle;
        }
        public static ObjectId CreateCircle(double X, double Y, double Z, double radian, string layerName, int colorIndex)
        {
            CreateLayer(layerName, colorIndex);
            // Get the current document and database
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            ObjectId ObjectIDLIne;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // Create a line that starts at 5,5 and ends at 12,3
                using (Circle acCircle = new Circle())
                {
                    acCircle.Center = new Point3d(X, Y, Z);
                    acCircle.Radius = radian;
                    acCircle.ColorIndex = 256;
                    acCircle.Layer = layerName;
                    ObjectIDLIne = acBlkTblRec.AppendEntity(acCircle);
                    acTrans.AddNewlyCreatedDBObject(acCircle, true);
                    ed.UpdateScreen();
                }
                // Save the new object to the database
                acTrans.Commit();
            }
            return ObjectIDLIne;
        }
        public static Entity GetCircleEntity(double X, double Y, double Z, double radian, string layerName, int colorIndex)
        {
            // Get the current document and database
            AEE_Utility.CreateLayer(layerName, colorIndex);

            Circle acCircle = new Circle();
            acCircle.Center = new Point3d(X, Y, Z);
            acCircle.Radius = radian;
            acCircle.Layer = layerName;
            acCircle.ColorIndex = 256;

            return acCircle;
        }
        public static ObjectId GetCircleId(double X, double Y, double Z, double radian, bool visible)
        {
            // Get the current document and database
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;

            ObjectId ObjectIDLIne;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // Create a line that starts at 5,5 and ends at 12,3
                using (Circle acCircle = new Circle())
                {
                    acCircle.Center = new Point3d(X, Y, Z);
                    acCircle.Radius = radian;
                    acCircle.Visible = visible;

                    ObjectIDLIne = acBlkTblRec.AppendEntity(acCircle);
                    acTrans.AddNewlyCreatedDBObject(acCircle, true);
                }
                // Save the new object to the database
                acTrans.Commit();
            }
            return ObjectIDLIne;
        }

        public static bool IntersectPointBetweenTwoLines(Point2d p1, Point2d p2, Point2d p3, Point2d p4)
        {
            bool lines_intersect = false;

            Point2d intersection = new Point2d();

            // Get the segments' parameters.
            double dx12 = p2.X - p1.X;
            double dy12 = p2.Y - p1.Y;
            double dx34 = p4.X - p3.X;
            double dy34 = p4.Y - p3.Y;

            // Solve for t1 and t2
            double denominator = (dy12 * dx34 - dx12 * dy34);

            double t1 = ((p1.X - p3.X) * dy34 + (p3.Y - p1.Y) * dx34) / denominator;

            if (double.IsInfinity(t1))
            {
                // The lines are parallel (or close enough to it).
                lines_intersect = false;
            }
            else
            {
                lines_intersect = true;
                double t2 = ((p3.X - p1.X) * dy12 + (p1.Y - p3.Y) * dx12) / -denominator;
                // Find the point of intersection.
                intersection = new Point2d(p1.X + dx12 * t1, p1.Y + dy12 * t1);
            }
            return lines_intersect;
        }
        //  Returns Point2d of intersection if do intersect otherwise default Point2d (null)
        public static Point2d? FindIntersection(Line lineA, Line lineB, double tolerance = 0.001)
        {
            double x1 = lineA.StartPoint.X, y1 = lineA.StartPoint.Y;
            double x2 = lineA.EndPoint.X, y2 = lineA.EndPoint.Y;

            double x3 = lineB.StartPoint.X, y3 = lineB.StartPoint.Y;
            double x4 = lineB.EndPoint.X, y4 = lineB.EndPoint.Y;

            // equations of the form x=c (two vertical lines) with overlapping
            if (Math.Abs(x1 - x2) < tolerance && Math.Abs(x3 - x4) < tolerance && Math.Abs(x1 - x3) < tolerance)
            {
                //throw new Exception("Both lines overlap vertically, ambiguous intersection points.");
            }

            //equations of the form y=c (two horizontal lines) with overlapping
            if (Math.Abs(y1 - y2) < tolerance && Math.Abs(y3 - y4) < tolerance && Math.Abs(y1 - y3) < tolerance)
            {
                //throw new Exception("Both lines overlap horizontally, ambiguous intersection points.");
            }

            //equations of the form x=c (two vertical parallel lines)
            if (Math.Abs(x1 - x2) < tolerance && Math.Abs(x3 - x4) < tolerance)
            {
                //return default (no intersection)
                return null;
            }

            //equations of the form y=c (two horizontal parallel lines)
            if (Math.Abs(y1 - y2) < tolerance && Math.Abs(y3 - y4) < tolerance)
            {
                //return default (no intersection)
                return null;
            }

            //general equation of line is y = mx + c where m is the slope
            //assume equation of line 1 as y1 = m1x1 + c1 
            //=> -m1x1 + y1 = c1 ----(1)
            //assume equation of line 2 as y2 = m2x2 + c2
            //=> -m2x2 + y2 = c2 -----(2)
            //if line 1 and 2 intersect then x1=x2=x & y1=y2=y where (x,y) is the intersection point
            //so we will get below two equations 
            //-m1x + y = c1 --------(3)
            //-m2x + y = c2 --------(4)

            double x, y;

            //lineA is vertical x1 = x2
            //slope will be infinity
            //so lets derive another solution
            if (Math.Abs(x1 - x2) < tolerance)
            {
                //compute slope of line 2 (m2) and c2
                double m2 = (y4 - y3) / (x4 - x3);
                double c2 = -m2 * x3 + y3;

                //equation of vertical line is x = c
                //if line 1 and 2 intersect then x1=c1=x
                //subsitute x=x1 in (4) => -m2x1 + y = c2
                // => y = c2 + m2x1 
                x = x1;
                y = c2 + m2 * x1;
            }
            //lineB is vertical x3 = x4
            //slope will be infinity
            //so lets derive another solution
            else if (Math.Abs(x3 - x4) < tolerance)
            {
                //compute slope of line 1 (m1) and c2
                double m1 = (y2 - y1) / (x2 - x1);
                double c1 = -m1 * x1 + y1;

                //equation of vertical line is x = c
                //if line 1 and 2 intersect then x3=c3=x
                //subsitute x=x3 in (3) => -m1x3 + y = c1
                // => y = c1 + m1x3 
                x = x3;
                y = c1 + m1 * x3;
            }
            //lineA & lineB are not vertical 
            //(could be horizontal we can handle it with slope = 0)
            else
            {
                //compute slope of line 1 (m1) and c2
                double m1 = (y2 - y1) / (x2 - x1);
                double c1 = -m1 * x1 + y1;

                //compute slope of line 2 (m2) and c2
                double m2 = (y4 - y3) / (x4 - x3);
                double c2 = -m2 * x3 + y3;

                //solving equations (3) & (4) => x = (c1-c2)/(m2-m1)
                //plugging x value in equation (4) => y = c2 + m2 * x
                x = (c1 - c2) / (m2 - m1);
                y = c2 + m2 * x;

                //verify by plugging intersection point (x, y)
                //in orginal equations (1) & (2) to see if they intersect
                //otherwise x,y values will not be finite and will fail this check
                if (!(Math.Abs(-m1 * x + y - c1) < tolerance
                    && Math.Abs(-m2 * x + y - c2) < tolerance))
                {
                    //return default (no intersection)
                    return null;
                }
            }

            //x,y can intersect outside the line segment since line is infinitely long
            //so finally check if x, y is within both the line segments
            if (IsInsideLine(lineA, x, y) &&
                IsInsideLine(lineB, x, y))
            {
                return new Point2d(x, y);
            }

            //return default (no intersection)
            return null;

        }

        // Returns true if given point(x,y) is inside the given line segment
        public static bool IsInsideLine(Line line, double x, double y)
        {
            //x = Math.Round(x); 

            bool result = ((x.FGE(line.StartPoint.X) && x.FLE(line.EndPoint.X))
                        || (x.FGE(line.EndPoint.X) && x.FLE(line.StartPoint.X)))
                   && ((y.FGE(line.StartPoint.Y) && y.FLE(line.EndPoint.Y))
                        || (y.FGE(line.EndPoint.Y) && y.FLE(line.StartPoint.Y)));
            return result;
        }
        public static bool IsInsideLine(Line line, Point3d pt)
        {
            var x = pt.X;
            var y = pt.Y;
            bool result = ((x.FGE(line.StartPoint.X) && x.FLE(line.EndPoint.X))
                       || (x.FGE(line.EndPoint.X) && x.FLE(line.StartPoint.X)))
                  && ((y.FGE(line.StartPoint.Y) && y.FLE(line.EndPoint.Y))
                       || (y.FGE(line.EndPoint.Y) && y.FLE(line.StartPoint.Y)));
            return result;
        }
        public static bool FLE(this double val1, double val2, double tol = 0.1)
        {
            return val1 <= val2 || Math.Abs(val1 - val2) <= tol;
        }
        public static bool FGE(this double val1, double val2, double tol = 0.1)
        {
            return val1 >= val2 || Math.Abs(val1 - val2) <= tol;
        }

        public static List<Point2d> IntersectPointBetweenLineAndCircle(double circle_X, double circle_Y, double radius, Point2d point1, Point2d point2)
        {
            List<Point2d> listOfPoints = new List<Point2d>();

            double dx, dy, A, B, C, det, t;
            Point2d intersection1 = new Point2d();
            Point2d intersection2 = new Point2d();
            dx = point2.X - point1.X;
            dy = point2.Y - point1.Y;

            A = dx * dx + dy * dy;
            B = 2 * (dx * (point1.X - circle_X) + dy * (point1.Y - circle_Y));
            C = (point1.X - circle_X) * (point1.X - circle_X) + (point1.Y - circle_Y) * (point1.Y - circle_Y) - radius * radius;

            det = B * B - 4 * A * C;
            if ((A <= 0.0000001) || (det < 0))
            {
                // No real solutions.
                intersection1 = new Point2d(double.NaN, double.NaN);
                intersection2 = new Point2d(double.NaN, double.NaN);
            }
            else if (det == 0)
            {
                // One solution.
                t = -B / (2 * A);
                intersection1 = new Point2d(point1.X + t * dx, point1.Y + t * dy);
                intersection2 = new Point2d(double.NaN, double.NaN);
            }
            else
            {
                // Two solutions.
                t = (double)((-B + Math.Sqrt(det)) / (2 * A));
                intersection1 = new Point2d(point1.X + t * dx, point1.Y + t * dy);
                t = (double)((-B - Math.Sqrt(det)) / (2 * A));
                intersection2 = new Point2d(point1.X + t * dx, point1.Y + t * dy);
                listOfPoints.Add(intersection1);
                listOfPoints.Add(intersection2);
            }
            return listOfPoints;
        }
        public static ObjectId CreateTextWithAngle(string text, double x, double y, double z, double h, string layerName, int colorIndex, double angel)
        {
            ObjectId textId = new ObjectId();

            CreateLayer(layerName, colorIndex);

            // ' Get the current document and database
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;

            using (DocumentLock locDoc = ed.Document.LockDocument())
            {
                // ' Start a transaction
                using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
                {

                    // ' Open the Block table for read
                    BlockTable acBlkTbl = (BlockTable)acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead);

                    // ' Open the Block table record Model space for write
                    BlockTableRecord acBlkTblRec = (BlockTableRecord)acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                    string nm = acBlkTblRec.Name;

                    // ' Create a single-line text object
                    using (DBText acText = new DBText())
                    {
                        // Open the current text style for write
                        TextStyleTableRecord acTextStyleTblRec;
                        acTextStyleTblRec = acTrans.GetObject(acCurDb.Textstyle, OpenMode.ForWrite) as TextStyleTableRecord;
                        // Get the current font settings
                        Autodesk.AutoCAD.GraphicsInterface.FontDescriptor acFont;
                        acFont = acTextStyleTblRec.Font;
                        // Update the text style's typeface with "Arial Narrow"
                        Autodesk.AutoCAD.GraphicsInterface.FontDescriptor acNewFont;
                        acNewFont = new Autodesk.AutoCAD.GraphicsInterface.FontDescriptor("Arial", acFont.Bold, acFont.Italic,
                                                                            acFont.CharacterSet, acFont.PitchAndFamily);
                        acTextStyleTblRec.Font = acNewFont;
                        acText.Position = new Point3d(x, y, z);
                        acText.Height = h;
                        acText.ColorIndex = 256;
                        acText.TextString = text;
                        acText.Layer = layerName;
                        angel = (angel * 3.141592) / 180;
                        acText.Rotation = angel;
                        acText.VerticalMode = TextVerticalMode.TextVerticalMid;
                        acText.HorizontalMode = TextHorizontalMode.TextCenter;
                        acText.AlignmentPoint = new Point3d(x, y, z);
                        textId = acBlkTblRec.AppendEntity(acText);
                        acTrans.AddNewlyCreatedDBObject(acText, true);
                        ed.UpdateScreen();
                    }
                    // ' Save the changes and dispose of the transaction
                    acTrans.Commit();
                }
            }
            return textId;
        }
        public static ObjectId CreateTextWithoutAligment(string text, double x, double y, double z, double h, string layerName, int colorIndex, double angel)
        {
            ObjectId textId = new ObjectId();

            CreateLayer(layerName, colorIndex);

            // ' Get the current document and database
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            Editor ed = acDoc.Editor;

            using (DocumentLock locDoc = ed.Document.LockDocument())
            {
                // ' Start a transaction
                using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
                {

                    // ' Open the Block table for read

                    BlockTable acBlkTbl = (BlockTable)acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead);

                    // ' Open the Block table record Model space for write
                    BlockTableRecord acBlkTblRec = (BlockTableRecord)acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                    string nm = acBlkTblRec.Name;

                    // ' Create a single-line text object
                    using (DBText acText = new DBText())
                    {
                        // Open the current text style for write
                        TextStyleTableRecord acTextStyleTblRec;
                        acTextStyleTblRec = acTrans.GetObject(acCurDb.Textstyle, OpenMode.ForWrite) as TextStyleTableRecord;
                        // Get the current font settings
                        Autodesk.AutoCAD.GraphicsInterface.FontDescriptor acFont;
                        acFont = acTextStyleTblRec.Font;
                        // Update the text style's typeface with "Arial Narrow"
                        Autodesk.AutoCAD.GraphicsInterface.FontDescriptor acNewFont;
                        acNewFont = new Autodesk.AutoCAD.GraphicsInterface.FontDescriptor("Arial", acFont.Bold, acFont.Italic,
                                                                            acFont.CharacterSet, acFont.PitchAndFamily);
                        acTextStyleTblRec.Font = acNewFont;
                        acText.Position = new Point3d(x, y, z);
                        acText.Height = h;
                        acText.ColorIndex = 256;
                        acText.TextString = text;
                        acText.Layer = layerName;
                        angel = (angel * 3.141592) / 180;
                        acText.Rotation = angel;
                        //acText.VerticalMode = TextVerticalMode.TextVerticalMid;
                        //acText.HorizontalMode = TextHorizontalMode.TextCenter;
                        //acText.AlignmentPoint = new Point3d(x, y, z);
                        textId = acBlkTblRec.AppendEntity(acText);
                        acTrans.AddNewlyCreatedDBObject(acText, true);
                        ed.UpdateScreen();
                    }
                    // ' Save the changes and dispose of the transaction
                    acTrans.Commit();
                }
            }
            return textId;
        }
        public static ObjectId CreateTextWithAlignInRight(string text, double x, double y, double z, double h, string layerName, int colorIndex, double angel)
        {
            ObjectId textId = new ObjectId();

            CreateLayer(layerName, colorIndex);

            // ' Get the current document and database
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;

            using (DocumentLock locDoc = ed.Document.LockDocument())
            {
                // ' Start a transaction
                using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
                {
                    // ' Open the Block table for read
                    BlockTable acBlkTbl = (BlockTable)acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead);

                    // ' Open the Block table record Model space for write
                    BlockTableRecord acBlkTblRec = (BlockTableRecord)acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                    string nm = acBlkTblRec.Name;

                    // ' Create a single-line text object
                    using (DBText acText = new DBText())
                    {
                        // Open the current text style for write
                        TextStyleTableRecord acTextStyleTblRec;
                        acTextStyleTblRec = acTrans.GetObject(acCurDb.Textstyle, OpenMode.ForWrite) as TextStyleTableRecord;
                        // Get the current font settings
                        Autodesk.AutoCAD.GraphicsInterface.FontDescriptor acFont;
                        acFont = acTextStyleTblRec.Font;
                        // Update the text style's typeface with "Arial Narrow"
                        Autodesk.AutoCAD.GraphicsInterface.FontDescriptor acNewFont;
                        acNewFont = new Autodesk.AutoCAD.GraphicsInterface.FontDescriptor("Arial", acFont.Bold, acFont.Italic,
                                                                            acFont.CharacterSet, acFont.PitchAndFamily);
                        acTextStyleTblRec.Font = acNewFont;
                        acText.Position = new Point3d(x, y, z);
                        acText.Height = h;
                        acText.ColorIndex = 256;
                        acText.TextString = text;
                        acText.Layer = layerName;
                        angel = (angel * 3.141592) / 180;
                        acText.Rotation = angel;
                        acText.VerticalMode = TextVerticalMode.TextVerticalMid;
                        acText.HorizontalMode = TextHorizontalMode.TextRight;
                        acText.AlignmentPoint = new Point3d(x, y, z);
                        textId = acBlkTblRec.AppendEntity(acText);
                        acTrans.AddNewlyCreatedDBObject(acText, true);
                        ed.UpdateScreen();
                    }
                    // ' Save the changes and dispose of the transaction
                    acTrans.Commit();
                }
            }
            return textId;
        }
        public static ObjectId CreateTextWithAlignInLeft(string text, double x, double y, double z, double h, string layerName, int colorIndex, double angel)
        {
            ObjectId textId;

            CreateLayer(layerName, colorIndex);

            // ' Get the current document and database
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;

            using (DocumentLock locDoc = ed.Document.LockDocument())
            {
                // ' Start a transaction
                using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
                {
                    // ' Open the Block table for read
                    BlockTable acBlkTbl = (BlockTable)acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead);

                    // ' Open the Block table record Model space for write
                    BlockTableRecord acBlkTblRec = (BlockTableRecord)acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                    string nm = acBlkTblRec.Name;

                    // ' Create a single-line text object
                    using (DBText acText = new DBText())
                    {
                        // Open the current text style for write
                        TextStyleTableRecord acTextStyleTblRec;
                        acTextStyleTblRec = acTrans.GetObject(acCurDb.Textstyle, OpenMode.ForWrite) as TextStyleTableRecord;
                        // Get the current font settings
                        Autodesk.AutoCAD.GraphicsInterface.FontDescriptor acFont;
                        acFont = acTextStyleTblRec.Font;
                        // Update the text style's typeface with "Arial Narrow"
                        Autodesk.AutoCAD.GraphicsInterface.FontDescriptor acNewFont;
                        acNewFont = new Autodesk.AutoCAD.GraphicsInterface.FontDescriptor("Arial", acFont.Bold, acFont.Italic,
                                                                            acFont.CharacterSet, acFont.PitchAndFamily);
                        acTextStyleTblRec.Font = acNewFont;
                        acText.Position = new Point3d(x, y, z);
                        acText.Height = h;
                        acText.ColorIndex = 256;
                        acText.TextString = text;
                        acText.Layer = layerName;
                        angel = (angel * 3.141592) / 180;
                        acText.Rotation = angel;
                        acText.VerticalMode = TextVerticalMode.TextVerticalMid;
                        acText.HorizontalMode = TextHorizontalMode.TextLeft;
                        acText.AlignmentPoint = new Point3d(x, y, z);
                        textId = acBlkTblRec.AppendEntity(acText);
                        acTrans.AddNewlyCreatedDBObject(acText, true);
                        ed.UpdateScreen();
                    }
                    // ' Save the changes and dispose of the transaction
                    acTrans.Commit();
                }
            }
            return textId;
        }
        public static Entity GetDBTextEntity(string text, double x, double y, double z, double h, string layerName, int colorIndex, double angel)
        {
            AEE_Utility.CreateLayer(layerName, colorIndex);

            DBText acText = new DBText();
            // Open the current text style for write                                       
            acText.Position = new Point3d(x, y, z);
            acText.Height = h;
            acText.TextString = text;
            angel = (angel * 3.141592) / 180;
            acText.Rotation = angel;
            acText.VerticalMode = TextVerticalMode.TextVerticalMid;
            acText.HorizontalMode = TextHorizontalMode.TextCenter;
            acText.AlignmentPoint = new Point3d(x, y, z);
            acText.Layer = layerName;
            acText.ColorIndex = 256;

            return acText;
        }

        private static void setDataOfDimension(Dimension dim, double textHeight, Database db)
        {
            dim.SetDatabaseDefaults();

            dim.Dimtxt = textHeight;
            dim.Dimasz = textHeight / 2;
            dim.Dimdli = textHeight / 2;
            dim.Dimgap = 1;
            dim.Dimexo = textHeight / 4;

            // acLinAngDim.DimensionStyle = acCurDb.Dimstyle;
            dim.DimensionStyle = db.CurrentSpaceId;
        }
        public static ObjectId crtRotatedDimWithPosition(double X1, double Y1, double X2, double Y2, double height, double x_dimPosition, double y_dimPosition, double angle, string layerName, int colorIndex, string sLineTypName, int lineTypeScale)
        {
            ObjectId id = new ObjectId();

            CreateLayer(layerName, colorIndex);
            // Get the current database
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Linetype table for read              
                LinetypeTable acLineTypTbl = acTrans.GetObject(acCurDb.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;

                if (acLineTypTbl.Has(sLineTypName) == false)
                {
                    acCurDb.LoadLineTypeFile(sLineTypName, "acad.lin");
                }

                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // Create an aligned dimension
                using (RotatedDimension acLinAngDim = new RotatedDimension())
                {
                    acLinAngDim.XLine1Point = new Point3d(X1, Y1, 0);
                    acLinAngDim.XLine2Point = new Point3d(X2, Y2, 0);
                    acLinAngDim.Layer = layerName;
                    acLinAngDim.ColorIndex = 256;
                    acLinAngDim.TextPosition = new Point3d(x_dimPosition, y_dimPosition, 0);
                    acLinAngDim.Linetype = sLineTypName;
                    acLinAngDim.LinetypeScale = lineTypeScale;
                    angle = (angle * Math.PI) / 180;
                    acLinAngDim.Rotation = angle;

                    setDataOfDimension(acLinAngDim, height, acCurDb);

                    //  acLinAngDim.Dimdec = 0; // Precision decimal number ignore

                    // Add the new object to Model space and the transaction
                    id = acBlkTblRec.AppendEntity(acLinAngDim);
                    acTrans.AddNewlyCreatedDBObject(acLinAngDim, true);
                    ed.UpdateScreen();
                }

                // Commit the changes and dispose of the transaction
                acTrans.Commit();
            }
            return id;
        }
        public static ObjectId crtAlignedDimWithText(string text, double X1, double Y1, double X2, double Y2, double height, double x_dimPosition, double y_dimPosition, string layerName, int colorIndex)
        {
            ObjectId id = new ObjectId();

            CreateLayer(layerName, colorIndex);
            // Get the current database
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // Create an aligned dimension
                using (AlignedDimension acLinAngDim = new AlignedDimension())
                {
                    acLinAngDim.XLine1Point = new Point3d(X1, Y1, 0);
                    acLinAngDim.XLine2Point = new Point3d(X2, Y2, 0);
                    acLinAngDim.Layer = layerName;
                    acLinAngDim.ColorIndex = 256;
                    // Override the dimension text
                    acLinAngDim.DimensionText = text;

                    acLinAngDim.TextPosition = new Point3d(x_dimPosition, y_dimPosition, 0);
                    //   acLinAngDim.Dimdec = 0; // Precision decimal number ignore              

                    setDataOfDimension(acLinAngDim, height, acCurDb);

                    // Add the new object to Model space and the transaction
                    id = acBlkTblRec.AppendEntity(acLinAngDim);
                    acTrans.AddNewlyCreatedDBObject(acLinAngDim, true);
                    ed.UpdateScreen();
                }

                // Commit the changes and dispose of the transaction
                acTrans.Commit();
            }
            return id;
        }
        public static ObjectId crtAlignedDimWithText(string text, double X1, double Y1, double X2, double Y2, double height, double x_dimPosition, double y_dimPosition, string layerName, int colorIndex, string sLineTypName, int lineTypeScale)
        {
            ObjectId id = new ObjectId();

            CreateLayer(layerName, colorIndex);
            // Get the current database
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Linetype table for read              
                LinetypeTable acLineTypTbl = acTrans.GetObject(acCurDb.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;

                if (acLineTypTbl.Has(sLineTypName) == false)
                {
                    acCurDb.LoadLineTypeFile(sLineTypName, "acad.lin");
                }

                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // Create an aligned dimension
                using (AlignedDimension acLinAngDim = new AlignedDimension())
                {
                    acLinAngDim.XLine1Point = new Point3d(X1, Y1, 0);
                    acLinAngDim.XLine2Point = new Point3d(X2, Y2, 0);
                    acLinAngDim.Layer = layerName;
                    acLinAngDim.ColorIndex = 256;
                    // Override the dimension text               
                    acLinAngDim.DimensionText = text;

                    acLinAngDim.TextPosition = new Point3d(x_dimPosition, y_dimPosition, 0);
                    //   acLinAngDim.Dimdec = 0; // Precision decimal number ignore              
                    acLinAngDim.Linetype = sLineTypName;
                    acLinAngDim.LinetypeScale = lineTypeScale;

                    setDataOfDimension(acLinAngDim, height, acCurDb);

                    // Add the new object to Model space and the transaction
                    id = acBlkTblRec.AppendEntity(acLinAngDim);
                    acTrans.AddNewlyCreatedDBObject(acLinAngDim, true);
                    ed.UpdateScreen();
                }

                // Commit the changes and dispose of the transaction
                acTrans.Commit();
            }
            return id;
        }

        public static ObjectId crtAlignedDimWithPosition(double X1, double Y1, double X2, double Y2, double height, double x_dimPosition, double y_dimPosition, string layerName, int colorIndex, string sLineTypName, int lineTypeScale)
        {
            ObjectId id = new ObjectId();

            CreateLayer(layerName, colorIndex);
            // Get the current database
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Linetype table for read              
                LinetypeTable acLineTypTbl = acTrans.GetObject(acCurDb.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;

                if (acLineTypTbl.Has(sLineTypName) == false)
                {
                    acCurDb.LoadLineTypeFile(sLineTypName, "acad.lin");
                }

                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // Create an aligned dimension
                using (AlignedDimension acLinAngDim = new AlignedDimension())
                {
                    acLinAngDim.XLine1Point = new Point3d(X1, Y1, 0);
                    acLinAngDim.XLine2Point = new Point3d(X2, Y2, 0);
                    acLinAngDim.Layer = layerName;
                    acLinAngDim.ColorIndex = 256;
                    acLinAngDim.TextPosition = new Point3d(x_dimPosition, y_dimPosition, 0);
                    //   acLinAngDim.Dimdec = 0; // Precision decimal number ignore
                    acLinAngDim.Linetype = sLineTypName;
                    acLinAngDim.LinetypeScale = lineTypeScale;

                    setDataOfDimension(acLinAngDim, height, acCurDb);

                    // Add the new object to Model space and the transaction
                    id = acBlkTblRec.AppendEntity(acLinAngDim);
                    acTrans.AddNewlyCreatedDBObject(acLinAngDim, true);
                    ed.UpdateScreen();
                }

                // Commit the changes and dispose of the transaction
                acTrans.Commit();
            }
            return id;
        }
        public static ObjectId crtAlignedDimWithPosition(ObjectId lineId, Point3d dimTextPoint, double textHeight, string layerName, int colorIndex, string sLineTypName, int lineTypeScale)
        {
            ObjectId id = new ObjectId();

            CreateLayer(layerName, colorIndex);
            // Get the current database
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Linetype table for read              
                LinetypeTable acLineTypTbl = acTrans.GetObject(acCurDb.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;

                if (acLineTypTbl.Has(sLineTypName) == false)
                {
                    acCurDb.LoadLineTypeFile(sLineTypName, "acad.lin");
                }

                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                var listOfDim = getStartEndPointWithAngle_Line(lineId);
                Point3d startPoint = new Point3d(listOfDim[0], listOfDim[1], 0);
                Point3d endPoint = new Point3d(listOfDim[2], listOfDim[3], 0);

                var acLinAngDim = new AlignedDimension(startPoint, endPoint, dimTextPoint, null, acCurDb.Dimstyle);

                acLinAngDim.Layer = layerName;
                acLinAngDim.ColorIndex = 256;
                //////   acLinAngDim.Dimdec = 0; // Precision decimal number ignore
                acLinAngDim.Linetype = sLineTypName;
                acLinAngDim.LinetypeScale = lineTypeScale;

                setDataOfDimension(acLinAngDim, textHeight, acCurDb);

                // Add the new object to Model space and the transaction
                id = acBlkTblRec.AppendEntity(acLinAngDim);
                acTrans.AddNewlyCreatedDBObject(acLinAngDim, true);
                ed.UpdateScreen();

                // Commit the changes and dispose of the transaction
                acTrans.Commit();
            }
            return id;
        }
        public static ObjectId crtAlignedDimWithPosition(ObjectId lineId, Point3d dimTextPoint, double textHeight, string layerName, int colorIndex)
        {
            ObjectId id = new ObjectId();

            CreateLayer(layerName, colorIndex);
            // Get the current database
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                var listOfDim = getStartEndPointWithAngle_Line(lineId);
                Point3d startPoint = new Point3d(listOfDim[0], listOfDim[1], 0);
                Point3d endPoint = new Point3d(listOfDim[2], listOfDim[3], 0);

                var acLinAngDim = new AlignedDimension(startPoint, endPoint, dimTextPoint, null, acCurDb.Dimstyle);

                acLinAngDim.Layer = layerName;
                acLinAngDim.ColorIndex = 256;
                //////   acLinAngDim.Dimdec = 0; // Precision decimal number ignore              

                setDataOfDimension(acLinAngDim, textHeight, acCurDb);

                // Add the new object to Model space and the transaction
                id = acBlkTblRec.AppendEntity(acLinAngDim);
                acTrans.AddNewlyCreatedDBObject(acLinAngDim, true);
                ed.UpdateScreen();

                // Commit the changes and dispose of the transaction
                acTrans.Commit();
            }
            return id;
        }

        public static ObjectId crtRotatedDimWithPosition(ObjectId lineId, Point3d dimTextPoint, double textHeight, double angle, string layerName, int colorIndex, string sLineTypName, int lineTypeScale)
        {
            ObjectId id = new ObjectId();

            CreateLayer(layerName, colorIndex);
            // Get the current database
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Linetype table for read              
                LinetypeTable acLineTypTbl = acTrans.GetObject(acCurDb.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;

                if (acLineTypTbl.Has(sLineTypName) == false)
                {
                    acCurDb.LoadLineTypeFile(sLineTypName, "acad.lin");
                }

                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                var listOfDim = getStartEndPointWithAngle_Line(lineId);
                Point3d startPoint = new Point3d(listOfDim[0], listOfDim[1], 0);
                Point3d endPoint = new Point3d(listOfDim[2], listOfDim[3], 0);

                angle = (angle * Math.PI) / 180;
                //acLinAngDim.Rotation = angle;

                var acLinAngDim = new RotatedDimension(angle, startPoint, endPoint, dimTextPoint, null, acCurDb.Dimstyle);

                acLinAngDim.Layer = layerName;
                acLinAngDim.ColorIndex = 256;
                //////   acLinAngDim.Dimdec = 0; // Precision decimal number ignore
                acLinAngDim.Linetype = sLineTypName;
                acLinAngDim.LinetypeScale = lineTypeScale;

                setDataOfDimension(acLinAngDim, textHeight, acCurDb);

                // Add the new object to Model space and the transaction
                id = acBlkTblRec.AppendEntity(acLinAngDim);
                acTrans.AddNewlyCreatedDBObject(acLinAngDim, true);
                ed.UpdateScreen();

                // Commit the changes and dispose of the transaction
                acTrans.Commit();
            }
            return id;
        }

        public static ObjectId crtAlignedDim_Arc(ObjectId arcId, Point3d dimTextPoint, double textHeight, string layerName, int colorIndex, string sLineTypName, int lineTypeScale)
        {
            ObjectId id = new ObjectId();

            CreateLayer(layerName, colorIndex);
            // Get the current database
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Linetype table for read              
                LinetypeTable acLineTypTbl = acTrans.GetObject(acCurDb.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;

                if (acLineTypTbl.Has(sLineTypName) == false)
                {
                    acCurDb.LoadLineTypeFile(sLineTypName, "acad.lin");
                }

                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                var listOfDim = getDimensionOfArc(arcId);
                Point3d startPoint = new Point3d(listOfDim[5], listOfDim[6], 0);
                Point3d endPoint = new Point3d(listOfDim[7], listOfDim[8], 0);

                var acLinAngDim = new AlignedDimension(startPoint, endPoint, dimTextPoint, null, acCurDb.Dimstyle);

                acLinAngDim.Layer = layerName;
                acLinAngDim.ColorIndex = 256;
                //////   acLinAngDim.Dimdec = 0; // Precision decimal number ignore
                acLinAngDim.Linetype = sLineTypName;
                acLinAngDim.LinetypeScale = lineTypeScale;

                setDataOfDimension(acLinAngDim, textHeight, acCurDb);

                // Add the new object to Model space and the transaction
                id = acBlkTblRec.AppendEntity(acLinAngDim);
                acTrans.AddNewlyCreatedDBObject(acLinAngDim, true);
                ed.UpdateScreen();
                // Commit the changes and dispose of the transaction
                acTrans.Commit();
            }
            return id;
        }

        public static ObjectId crtAlignedDim_CenterArc(ObjectId arcId, Point3d dimTextPoint, double textHeight, string layerName, int colorIndex, string sLineTypName, int lineTypeScale)
        {
            ObjectId id = new ObjectId();

            CreateLayer(layerName, colorIndex);
            // Get the current database
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Linetype table for read              
                LinetypeTable acLineTypTbl = acTrans.GetObject(acCurDb.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;

                if (acLineTypTbl.Has(sLineTypName) == false)
                {
                    acCurDb.LoadLineTypeFile(sLineTypName, "acad.lin");
                }

                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                var listOfDim = getDimensionOfArc(arcId);
                Point3d startPoint = new Point3d(listOfDim[5], listOfDim[6], 0);
                Point3d endPoint = new Point3d(listOfDim[7], listOfDim[8], 0);
                var centerPoint = GetMidPoint(arcId);

                var acLinAngDim = new AlignedDimension(startPoint, centerPoint, dimTextPoint, null, acCurDb.Dimstyle);

                acLinAngDim.Layer = layerName;
                acLinAngDim.ColorIndex = 256;
                //////   acLinAngDim.Dimdec = 0; // Precision decimal number ignore
                acLinAngDim.Linetype = sLineTypName;
                acLinAngDim.LinetypeScale = lineTypeScale;

                setDataOfDimension(acLinAngDim, textHeight, acCurDb);

                // Add the new object to Model space and the transaction
                id = acBlkTblRec.AppendEntity(acLinAngDim);
                acTrans.AddNewlyCreatedDBObject(acLinAngDim, true);
                ed.UpdateScreen();
                // Commit the changes and dispose of the transaction
                acTrans.Commit();
            }
            return id;
        }

        public static ObjectId crtRotatedDim_CenterArc(ObjectId arcId, Point3d dimTextPoint, double angle, double textHeight, string layerName, int colorIndex, string sLineTypName, int lineTypeScale)
        {
            ObjectId id = new ObjectId();

            CreateLayer(layerName, colorIndex);
            // Get the current database
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Linetype table for read              
                LinetypeTable acLineTypTbl = acTrans.GetObject(acCurDb.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;

                if (acLineTypTbl.Has(sLineTypName) == false)
                {
                    acCurDb.LoadLineTypeFile(sLineTypName, "acad.lin");
                }

                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                var listOfDim = getDimensionOfArc(arcId);
                Point3d startPoint = new Point3d(listOfDim[5], listOfDim[6], 0);
                Point3d endPoint = new Point3d(listOfDim[7], listOfDim[8], 0);
                var centerPoint = GetMidPoint(arcId);

                angle = (angle * 3.141592) / 180;
                var acLinAngDim = new RotatedDimension(angle, startPoint, centerPoint, dimTextPoint, null, acCurDb.Dimstyle);

                acLinAngDim.Layer = layerName;
                acLinAngDim.ColorIndex = 256;
                //////   acLinAngDim.Dimdec = 0; // Precision decimal number ignore
                acLinAngDim.Linetype = sLineTypName;
                acLinAngDim.LinetypeScale = lineTypeScale;

                setDataOfDimension(acLinAngDim, textHeight, acCurDb);

                // Add the new object to Model space and the transaction
                id = acBlkTblRec.AppendEntity(acLinAngDim);
                acTrans.AddNewlyCreatedDBObject(acLinAngDim, true);
                ed.UpdateScreen();
                // Commit the changes and dispose of the transaction
                acTrans.Commit();
            }
            return id;
        }

        public static ObjectId crtAngularDim(ObjectId arcId, Point3d dimTextPoint, double textHeight, string layerName, int colorIndex, string sLineTypName, int lineTypeScale)
        {
            ObjectId id = new ObjectId();

            CreateLayer(layerName, colorIndex);
            // Get the current database
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Linetype table for read              
                LinetypeTable acLineTypTbl = acTrans.GetObject(acCurDb.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;

                if (acLineTypTbl.Has(sLineTypName) == false)
                {
                    acCurDb.LoadLineTypeFile(sLineTypName, "acad.lin");
                }

                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                var listOfDim = getDimensionOfArc(arcId);
                Point3d startPoint = new Point3d(listOfDim[5], listOfDim[6], 0);
                Point3d endPoint = new Point3d(listOfDim[7], listOfDim[8], 0);

                Point3d centerPoint_Arc = new Point3d(listOfDim[0], listOfDim[1], 0);

                Point3AngularDimension acAngluarDim = new Point3AngularDimension(centerPoint_Arc, startPoint, endPoint, dimTextPoint, null, acCurDb.Dimstyle);
                acAngluarDim.Layer = layerName;
                acAngluarDim.ColorIndex = colorIndex;
                acAngluarDim.Linetype = sLineTypName;
                acAngluarDim.LinetypeScale = lineTypeScale;
                id = acBlkTblRec.AppendEntity(acAngluarDim);
                acTrans.AddNewlyCreatedDBObject(acAngluarDim, true);
                ed.UpdateScreen();
                // Commit the changes and dispose of the transaction
                acTrans.Commit();
            }
            return id;
        }
        public static ObjectId createAngularDimension(Point3d startPoint, Point3d endPoint, Point3d centerPoint_Arc, Point3d dimTextPoint, double textHeight, string layerName, int colorIndex, string sLineTypName, int lineTypeScale)
        {
            ObjectId id = new ObjectId();

            CreateLayer(layerName, colorIndex);
            // Get the current database
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Linetype table for read              
                LinetypeTable acLineTypTbl = acTrans.GetObject(acCurDb.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;

                if (acLineTypTbl.Has(sLineTypName) == false)
                {
                    acCurDb.LoadLineTypeFile(sLineTypName, "acad.lin");
                }

                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                Point3AngularDimension acAngluarDim = new Point3AngularDimension(centerPoint_Arc, startPoint, endPoint, dimTextPoint, null, acCurDb.Dimstyle);

                acAngluarDim.Layer = layerName;
                acAngluarDim.ColorIndex = colorIndex;
                acAngluarDim.Linetype = sLineTypName;
                acAngluarDim.LinetypeScale = lineTypeScale;
                id = acBlkTblRec.AppendEntity(acAngluarDim);
                acTrans.AddNewlyCreatedDBObject(acAngluarDim, true);
                ed.UpdateScreen();
                // Commit the changes and dispose of the transaction
                acTrans.Commit();
            }
            return id;
        }

        public static ObjectId CreateDiametricDimension(double centerX, double centerY, double radius, double leaderLength, double height, double angle, string layerName, int colorINdex, string sLineTypName, int lineTypeScale)
        {
            ObjectId id = new ObjectId();

            CreateLayer(layerName, colorINdex);
            // Get the current database
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Linetype table for read              
                LinetypeTable acLineTypTbl = acTrans.GetObject(acCurDb.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;

                if (acLineTypTbl.Has(sLineTypName) == false)
                {
                    acCurDb.LoadLineTypeFile(sLineTypName, "acad.lin");
                }

                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                double degree = (angle * 3.141592) / 180;
                double cosTheta = Math.Cos(degree);
                double sinTheta = Math.Sin(degree);

                double x = cosTheta * radius;
                double y = sinTheta * radius;
                double x2 = centerX + x;
                double y2 = centerY + y;

                // Create the diametric dimension
                using (DiametricDimension acDia = new DiametricDimension())
                {
                    acDia.FarChordPoint = new Point3d((centerX - x), (centerY - y), 0);
                    acDia.ChordPoint = new Point3d(x2, y2, 0);
                    acDia.LeaderLength = leaderLength;
                    acDia.Layer = layerName;
                    acDia.ColorIndex = 256;
                    acDia.Linetype = sLineTypName;
                    acDia.LinetypeScale = lineTypeScale;

                    setDataOfDimension(acDia, height, acCurDb);

                    // Add the new object to Model space and the transaction
                    id = acBlkTblRec.AppendEntity(acDia);
                    acTrans.AddNewlyCreatedDBObject(acDia, true);
                    ed.UpdateScreen();
                }
                // Commit the changes and dispose of the transaction
                acTrans.Commit();
            }
            return id;
        }
        public static ObjectId CreateRadialDimension(double centerX, double centerY, double radius, double leaderLength, double height, double angle, string layerName, int colorINdex, string sLineTypName, int lineTypeScale)
        {
            ObjectId id = new ObjectId();

            CreateLayer(layerName, colorINdex);
            // Get the current database
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Linetype table for read              
                LinetypeTable acLineTypTbl = acTrans.GetObject(acCurDb.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;

                if (acLineTypTbl.Has(sLineTypName) == false)
                {
                    acCurDb.LoadLineTypeFile(sLineTypName, "acad.lin");
                }

                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                double degree = (angle * 3.141592) / 180;
                double cosTheta = Math.Cos(degree);
                double sinTheta = Math.Sin(degree);

                double x = cosTheta * radius;
                double y = sinTheta * radius;
                double x2 = centerX - x;
                double y2 = centerY - y;

                // Create the diametric dimension
                using (RadialDimension acRad = new RadialDimension())
                {
                    acRad.ChordPoint = new Point3d(centerX, centerY, 0);
                    acRad.Center = new Point3d(x2, y2, 0);
                    acRad.LeaderLength = leaderLength;

                    acRad.Layer = layerName;
                    acRad.ColorIndex = 256;
                    acRad.Linetype = sLineTypName;
                    acRad.LinetypeScale = lineTypeScale;

                    setDataOfDimension(acRad, height, acCurDb);

                    // Add the new object to Model space and the transaction
                    id = acBlkTblRec.AppendEntity(acRad);
                    acTrans.AddNewlyCreatedDBObject(acRad, true);
                    ed.UpdateScreen();
                }
                // Commit the changes and dispose of the transaction
                acTrans.Commit();
            }
            return id;
        }

        public static ObjectId CreateRadialDimension(ObjectId arcId, double leaderLength, double height, string addSub_Operator, string layerName, int colorIndex, string sLineTypName, int lineTypeScale)
        {
            ObjectId id = new ObjectId();

            CreateLayer(layerName, colorIndex);
            // Get the current database
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                var listOfArcDim = getDimensionOfArc(arcId);
                Point3d centerPoint = new Point3d(listOfArcDim[0], listOfArcDim[1], 0);
                double radius = listOfArcDim[2];
                double maxAngle = Math.Max(listOfArcDim[3], listOfArcDim[4]);
                double minAngle = Math.Min(listOfArcDim[3], listOfArcDim[4]);
                double angle = maxAngle - ((maxAngle - minAngle) / 4);

                double degree = (angle * 3.141592) / 180;
                double cosTheta = Math.Cos(degree);
                double sinTheta = Math.Sin(degree);

                double x = cosTheta * radius;
                double y = sinTheta * radius;
                double x2 = centerPoint.X + x;
                double y2 = centerPoint.Y + y;

                double txt_X = 0;
                double txt_Y = 0;

                if (addSub_Operator == "+") // text position assign
                {
                    txt_X = cosTheta * (radius + leaderLength);
                    txt_Y = sinTheta * (radius + leaderLength);
                }
                else if (addSub_Operator == "-")  // text position assign
                {
                    txt_X = cosTheta * (radius - leaderLength);
                    txt_Y = sinTheta * (radius - leaderLength);
                }
                // Create the diametric dimension
                using (RadialDimension acRad = new RadialDimension())
                {
                    acRad.Center = new Point3d(centerPoint.X, centerPoint.Y, 0);
                    acRad.ChordPoint = new Point3d(x2, y2, 0);

                    acRad.TextPosition = new Point3d((centerPoint.X + txt_X), (centerPoint.Y + txt_Y), 0);

                    acRad.Dimtxt = height;
                    acRad.Dimasz = height;
                    acRad.Layer = layerName;
                    acRad.ColorIndex = 256;
                    acRad.Dimdli = height;
                    acRad.Dimgap = 1;

                    //  acRad.DimensionStyle = acCurDb.Dimstyle;
                    acRad.DimensionStyle = acCurDb.CurrentSpaceId;

                    //    setDataOfDimension(acRad, height, acCurDb);

                    // Add the new object to Model space and the transaction
                    id = acBlkTblRec.AppendEntity(acRad);
                    acTrans.AddNewlyCreatedDBObject(acRad, true);
                    ed.UpdateScreen();
                }
                // Commit the changes and dispose of the transaction
                acTrans.Commit();
            }
            return id;
        }

        public static ObjectId CreateRadialDimension(ObjectId arcId, Point3d textposition, double leaderLength, double height, string layerName, int colorIndex, string sLineTypName, int lineTypeScale)
        {
            ObjectId id = new ObjectId();

            CreateLayer(layerName, colorIndex);
            // Get the current database
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                var listOfArcDim = getDimensionOfArc(arcId);
                Point3d centerPoint = new Point3d(listOfArcDim[0], listOfArcDim[1], 0);
                double radius = listOfArcDim[2];

                var midPointOfArc = AEE_Utility.GetMidPoint(arcId);

                // Create the diametric dimension
                using (RadialDimension acRad = new RadialDimension())
                {
                    acRad.Center = new Point3d(centerPoint.X, centerPoint.Y, 0);
                    acRad.ChordPoint = new Point3d(midPointOfArc.X, midPointOfArc.Y, 0);

                    acRad.TextPosition = new Point3d(textposition.X, textposition.Y, 0);

                    acRad.Dimtxt = height;
                    acRad.Dimasz = height;
                    acRad.Layer = layerName;
                    acRad.ColorIndex = 256;
                    acRad.Dimdli = height;
                    acRad.Dimgap = 1;

                    //  acRad.DimensionStyle = acCurDb.Dimstyle;
                    acRad.DimensionStyle = acCurDb.CurrentSpaceId;
                    //    setDataOfDimension(acRad, height, acCurDb);

                    // Add the new object to Model space and the transaction
                    id = acBlkTblRec.AppendEntity(acRad);
                    acTrans.AddNewlyCreatedDBObject(acRad, true);
                    ed.UpdateScreen();
                }
                // Commit the changes and dispose of the transaction
                acTrans.Commit();
            }
            return id;
        }

        public static LineSegment3d OffsetLine(double offsetValue, Point3d start, Point3d end)
        {
            var line = new Line(start, end);
            var objs = line.GetOffsetCurves(offsetValue);
            foreach (var ent in objs)
            {
                var lineO = ent as Line;
                if (lineO != null)
                {
                    return new LineSegment3d(lineO.StartPoint, lineO.EndPoint);
                }
            }
            return null;
        }

        public static ObjectId OffsetEntity(double offsetValue, ObjectId id, bool visible)
        {
            ObjectId ids = new ObjectId();

            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            Database acCurDb = doc.Database;
           
            using (Transaction tr = doc.TransactionManager.StartTransaction())
            {
                // Open the Block table for read          
                BlockTable acBlkTbl = tr.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write               
                BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                Entity ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
                DBObjectCollection acDbObjColl = new DBObjectCollection();
                if (ent is Arc)
                {
                    Arc acArc = ent as Arc;
                    // Offset the polyline a given distance
                    acDbObjColl = acArc.GetOffsetCurves(offsetValue);
                }
                else if (ent is Polyline)
                {
                    Polyline acPoly = ent as Polyline;
                    // Offset the polyline a given distance
                    acDbObjColl = acPoly.GetOffsetCurves(offsetValue);
                }
                else if (ent is Line)
                {
                    Line acLine = ent as Line;
                    // Offset the polyline a given distance
                    acDbObjColl = acLine.GetOffsetCurves(offsetValue);
                }
                else if (ent is Circle)
                {
                    Circle acCircle = ent as Circle;
                    // Offset the polyline a given distance
                    acDbObjColl = acCircle.GetOffsetCurves(offsetValue);
                }
                // Step through the new objects created
                foreach (Entity acEnt in acDbObjColl)
                {
                    // Add each offset object   
                    acEnt.Visible = visible;  //Testing-123 // true;//
                    ids = acBlkTblRec.AppendEntity(acEnt);
                    tr.AddNewlyCreatedDBObject(acEnt, true);
                }
                tr.Commit();
            }
            return ids;
        }
        public static ObjectId OffsetEntity_WithoutLine(double offsetValue, Point3d offsetPointDir, ObjectId id, bool visible)
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            ObjectId ids = new ObjectId();
            using (Transaction tr = doc.TransactionManager.StartTransaction())
            {
                Curve curve = tr.GetObject(id, OpenMode.ForRead) as Curve;
                if (curve != null)
                {
                    BlockTableRecord btr = tr.GetObject(curve.BlockId, OpenMode.ForWrite) as BlockTableRecord;
                    if (btr != null)
                    {
                        Point3d pDir = (Point3d)(Autodesk.AutoCAD.ApplicationServices.Application.GetSystemVariable("VIEWDIR"));
                        if (pDir != null)
                        {
                            Point3d pWCS = offsetPointDir.TransformBy(ed.CurrentUserCoordinateSystem);
                            double offset = IsRightDirection(curve, pWCS, pDir.GetAsVector()) ? offsetValue : -offsetValue;
                            DBObjectCollection curvCols = curve.GetOffsetCurves(offset);
                            if (curvCols == null || curvCols.Count == 0)
                            {
                                curvCols = curve.GetOffsetCurves(-offset);
                            }
                            foreach (DBObject obj in curvCols)
                            {
                                Curve subCurv = obj as Curve;
                                if (subCurv != null)
                                {
                                    subCurv.Visible = visible; // visible; Testing-123
                                    ids = btr.AppendEntity(subCurv);
                                    tr.AddNewlyCreatedDBObject(subCurv, true);
                                }
                            }
                        }
                    }
                }
                tr.Commit();
            }
            return ids;
        }
        private static bool IsRightDirection(Curve pCurv, Point3d p, Vector3d vDir)
        {
            Vector3d vNormal = Vector3d.ZAxis;
            if (pCurv.IsPlanar)
            {
                Plane plane = pCurv.GetPlane();
                vNormal = plane.Normal;
                p = p.Project(plane, vDir);
            }
            Point3d pNear = pCurv.GetClosestPointTo(p, true);
            Vector3d vSide = p - pNear;
            Vector3d vDeriv = pCurv.GetFirstDerivative(pNear);
            if (vNormal.CrossProduct(vDeriv).DotProduct(vSide) < 0.0)
                return true;
            else
                return false;
        }
        public static List<ObjectId> ExplodeEntity(ObjectId explodeId)
        {
            List<ObjectId> listOfExplodes_Id = new List<ObjectId>();

            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database acCurDb = doc.Database;

            using (Transaction tr = doc.TransactionManager.StartTransaction())
            {
                // Open the Block table for read          
                BlockTable acBlkTbl = tr.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write               
                BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                Entity ent = tr.GetObject(explodeId, OpenMode.ForRead) as Entity;
                if (ent is Polyline)
                {
                    // Explodes the polyline
                    DBObjectCollection acDBObjColl = new DBObjectCollection();
                    ent.Explode(acDBObjColl);

                    // Step through the new objects created
                    foreach (Entity acEnt in acDBObjColl)
                    {
                        acEnt.Visible = false;
                        ObjectId id = acBlkTblRec.AppendEntity(acEnt);
                        listOfExplodes_Id.Add(id);
                        tr.AddNewlyCreatedDBObject(acEnt, true);
                    }
                }

                tr.Commit();
            }
            return listOfExplodes_Id;
        }
        public static List<double> getDimensionOfArc_FromEntity(List<Entity> listOfEntity, string inputLayerName)
        {
            List<double> listOfArc = new List<double>();
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            try
            {
                using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
                {
                    foreach (var ent in listOfEntity)
                    {
                        if (ent is Arc)
                        {
                            Arc arc = ent as Arc;
                            string layerName = arc.Layer;
                            if (inputLayerName == layerName)
                            {
                                listOfArc.Add(arc.Center.X);
                                listOfArc.Add(arc.Center.Y);
                                listOfArc.Add(arc.Radius);
                                double startAngle = ConvertRadiansToDegrees(arc.StartAngle);
                                listOfArc.Add(startAngle);
                                double endAngle = ConvertRadiansToDegrees(arc.EndAngle);
                                listOfArc.Add(endAngle);
                                listOfArc.Add(arc.StartPoint.X);
                                listOfArc.Add(arc.StartPoint.Y);
                                listOfArc.Add(arc.EndPoint.X);
                                listOfArc.Add(arc.EndPoint.Y);
                                break;
                            }
                        }
                    }
                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
            return listOfArc;
        }
        public static List<double> getDimensionOfArc(List<ObjectId> listOfId, string inputLayerName)
        {
            List<double> listOfArc = new List<double>();
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            try
            {
                using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
                {
                    foreach (var id in listOfId)
                    {
                        Entity ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
                        if (ent is Arc)
                        {
                            Arc arc = ent as Arc;
                            string layerName = arc.Layer;
                            if (inputLayerName == layerName)
                            {
                                listOfArc.Add(arc.Center.X);
                                listOfArc.Add(arc.Center.Y);
                                listOfArc.Add(arc.Radius);
                                double startAngle = ConvertRadiansToDegrees(arc.StartAngle);
                                listOfArc.Add(startAngle);
                                double endAngle = ConvertRadiansToDegrees(arc.EndAngle);
                                listOfArc.Add(endAngle);
                                listOfArc.Add(arc.StartPoint.X);
                                listOfArc.Add(arc.StartPoint.Y);
                                listOfArc.Add(arc.EndPoint.X);
                                listOfArc.Add(arc.EndPoint.Y);
                                break;
                            }
                        }
                    }
                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
            return listOfArc;
        }

        public static List<double> getDimensionOfArc(ObjectId arcId)
        {
            List<double> listOfArc = new List<double>();
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            try
            {
                using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
                {
                    Entity ent = tr.GetObject(arcId, OpenMode.ForRead) as Entity;
                    if (ent is Arc)
                    {
                        Arc arc = ent as Arc;
                        listOfArc.Add(arc.Center.X);
                        listOfArc.Add(arc.Center.Y);
                        listOfArc.Add(arc.Radius);
                        double startAngle = ConvertRadiansToDegrees(arc.StartAngle);
                        listOfArc.Add(startAngle);
                        double endAngle = ConvertRadiansToDegrees(arc.EndAngle);
                        listOfArc.Add(endAngle);
                        listOfArc.Add(arc.StartPoint.X);
                        listOfArc.Add(arc.StartPoint.Y);
                        listOfArc.Add(arc.EndPoint.X);
                        listOfArc.Add(arc.EndPoint.Y);
                    }
                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
            return listOfArc;
        }
        public static double ConvertRadiansToDegrees(double radians)
        {
            double degrees = (180 / Math.PI) * radians;
            degrees = Math.Round(degrees, 1);
            return (degrees);
        }
        public static double GetAngleOfLine(double X1, double Y1, double X2, double Y2)
        {
            double angle = 0;
            using (Line acLine = new Line(new Point3d(X1, Y1, 0), new Point3d(X2, Y2, 0)))
            {
                double radians = acLine.Angle;
                double degrees = (180 / Math.PI) * radians;
                angle = Math.Round(degrees, 3);
            }
            return angle;
        }
        public static double GetAngleOfLine(Line acLine)
        {
            double radians = acLine.Angle;
            double degrees = (180 / Math.PI) * radians;
            double angle = Math.Round(degrees, 3);

            return angle;
        }
        public static double GetAngleOfLine(Point3d startPoint, Point3d endPoint)
        {
            double angle = 0;
            using (Line acLine = new Line(startPoint, endPoint))
            {
                double radians = acLine.Angle;
                double degrees = (180 / Math.PI) * radians;
                angle = Math.Round(degrees, 3);
            }
            return angle;
        }

        public static double GetLengthOfLine(ObjectId lineID)
        {
            double length = 0;
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;

            try
            {
                using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
                {
                    Entity ent = tr.GetObject(lineID, OpenMode.ForRead) as Entity;
                    if (ent is Line)
                    {
                        Line line = ent as Line;
                        length = line.Length;
                    }
                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
            return length;
        }
        public static List<double> getStartEndPointWithAngle_Line(ObjectId lineID)
        {
            List<double> listOfPointWithAngle = new List<double>();
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;

            try
            {
                using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
                {
                    Entity ent = tr.GetObject(lineID, OpenMode.ForRead) as Entity;
                    if (ent is Line)
                    {
                        Line line = ent as Line;
                        Point3d startPoint = line.StartPoint;
                        Point3d endPoint = line.EndPoint;
                        double radians = line.Angle;
                        double angle = (180 / Math.PI) * radians;
                        listOfPointWithAngle.Add(startPoint.X);
                        listOfPointWithAngle.Add(startPoint.Y);
                        listOfPointWithAngle.Add(endPoint.X);
                        listOfPointWithAngle.Add(endPoint.Y);
                        listOfPointWithAngle.Add(angle);
                    }
                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
            return listOfPointWithAngle;
        }
        public static List<double> GetStartEndPointOfLine_WithEntity(Entity ent)
        {
            List<double> listOfPointWithAngle = new List<double>();
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;

            try
            {
                using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
                {
                    if (ent is Line)
                    {
                        Line line = ent as Line;
                        Point3d startPoint = line.StartPoint;
                        Point3d endPoint = line.EndPoint;
                        double radians = line.Angle;
                        double angle = (180 / Math.PI) * radians;
                        listOfPointWithAngle.Add(startPoint.X);
                        listOfPointWithAngle.Add(startPoint.Y);
                        listOfPointWithAngle.Add(endPoint.X);
                        listOfPointWithAngle.Add(endPoint.Y);
                        listOfPointWithAngle.Add(angle);
                    }
                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
            return listOfPointWithAngle;
        }
        public static List<Point3d> GetStartEndPointOfLine(ObjectId lineID)
        {
            List<Point3d> listOfPoint = new List<Point3d>();
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;

            try
            {
                using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
                {
                    Entity ent = tr.GetObject(lineID, OpenMode.ForRead) as Entity;
                    if (ent is Line)
                    {
                        Line line = ent as Line;
                        //Point2d startPoint = new Point2d(line.StartPoint.X, line.StartPoint.Y);
                        //Point2d endPoint = new Point2d(line.EndPoint.X, line.EndPoint.Y);
                        listOfPoint.Add(line.StartPoint);
                        listOfPoint.Add(line.EndPoint);
                    }
                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
            return listOfPoint;
        }

        public static double GetAngleOfLine(ObjectId lineID)
        {
            double angle = 0;
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;

            try
            {
                using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
                {
                    Entity ent = tr.GetObject(lineID, OpenMode.ForRead) as Entity;
                    if (ent is Line)
                    {
                        angle = GetLineAngle(ent);
                    }
                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
            return angle;
        }

        public static double GetLineAngle(Entity ent)
        {
            double angle;
            Line line = ent as Line;
            angle = line.Angle;
            double degrees = (180 / Math.PI) * angle;
            angle = Math.Round(degrees, 3);
            return angle;
        }

        public static Point3d getEndPointOfLineWithID(ObjectId lineID)
        {
            Point3d endPoint = new Point3d(); ;
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;

            try
            {
                using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
                {
                    Entity ent = tr.GetObject(lineID, OpenMode.ForRead) as Entity;
                    if (ent is Line)
                    {
                        Line line = ent as Line;
                        endPoint = new Point3d(line.EndPoint.X, line.EndPoint.Y, 0);
                    }
                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
            return endPoint;
        }
        public static List<Point3d> getPointOfLineWithID(ObjectId lineID)
        {
            List<Point3d> listOfPoint = new List<Point3d>();
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;

            try
            {
                using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
                {
                    Entity ent = tr.GetObject(lineID, OpenMode.ForRead) as Entity;
                    if (ent is Line)
                    {
                        Line line = ent as Line;
                        var strtPoint = new Point3d(line.StartPoint.X, line.StartPoint.Y, 0);
                        var endPoint = new Point3d(line.EndPoint.X, line.EndPoint.Y, 0);
                        listOfPoint.Add(strtPoint);
                        listOfPoint.Add(endPoint);
                    }
                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
            return listOfPoint;
        }
        public static double getLengthOfArc(ObjectId arcId)
        {
            double length = 0;
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;

            try
            {
                using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
                {
                    Entity ent = tr.GetObject(arcId, OpenMode.ForRead) as Entity;
                    if (ent is Arc)
                    {
                        Arc arc = ent as Arc;
                        length = arc.Length;
                    }
                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
            return length;
        }
        public static List<double> GetAngleOfArc(ObjectId arcId)
        {
            List<double> listOfAngle = new List<double>(); ;
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            try
            {
                using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
                {
                    Entity ent = tr.GetObject(arcId, OpenMode.ForRead) as Entity;
                    if (ent is Arc)
                    {
                        Arc arc = ent as Arc;
                        double startAngle = arc.StartAngle;
                        startAngle = ConvertRadiansToDegrees(startAngle);

                        double endAngle = arc.EndAngle;
                        endAngle = ConvertRadiansToDegrees(endAngle);

                        listOfAngle.Add(startAngle);
                        listOfAngle.Add(endAngle);
                    }
                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
            return listOfAngle;
        }
        public static ObjectId getLineId(double X1, double Y1, double Z1, double X2, double Y2, double Z2, string layerName, int colorIndex, bool visible)
        {
            CreateLayer(layerName, colorIndex);
            // Get the current document and database
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;

            ObjectId ObjectIDLIne;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // Create a line that starts at 5,5 and ends at 12,3
                using (Line acLine = new Line(new Point3d(X1, Y1, Z1), new Point3d(X2, Y2, Z2)))
                {
                    acLine.Visible = visible;
                    acLine.ColorIndex = 256;
                    acLine.Layer = layerName;
                    ObjectIDLIne = acBlkTblRec.AppendEntity(acLine);
                    acTrans.AddNewlyCreatedDBObject(acLine, true);
                }
                // Save the new object to the database
                acTrans.Commit();
            }
            return ObjectIDLIne;
        }


        public static ObjectId getLineId(Point3d startPoint, Point3d endPoint, bool visible)
        {
            // Get the current document and database
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;

            ObjectId lineObjId = new ObjectId();
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // Create a line that starts at 5,5 and ends at 12,3
                using (Line acLine = new Line(startPoint, endPoint))
                {
                    acLine.Visible = visible; // visible; Testing-123
                    lineObjId = acBlkTblRec.AppendEntity(acLine);
                    acTrans.AddNewlyCreatedDBObject(acLine, true);
                }

                // Save the new object to the database
                acTrans.Commit();
            }
            return lineObjId;
        }


        public static ObjectId getLineId(Point2d startPoint, Point2d endPoint, bool visible)
        {
            // Get the current document and database
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;

            ObjectId lineObjId = new ObjectId();
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // Create a line that starts at 5,5 and ends at 12,3
                using (Line acLine = new Line(new Point3d(startPoint.X, startPoint.Y, 0), new Point3d(endPoint.X, endPoint.Y, 0)))
                {
                    acLine.Visible = visible;
                    lineObjId = acBlkTblRec.AppendEntity(acLine);
                    acTrans.AddNewlyCreatedDBObject(acLine, true);
                }

                // Save the new object to the database
                acTrans.Commit();
            }
            return lineObjId;
        }

        public static ObjectId getLineId(ObjectId lineId, string layerName, int layerColor, bool visible)
        {
            AEE_Utility.CreateLayer(layerName, layerColor);
            // Get the current document and database
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;

            ObjectId lineObjId = new ObjectId();
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                Entity ent = acTrans.GetObject(lineId, OpenMode.ForRead) as Entity;
                Line line = ent as Line;
                // Create a line that starts at 5,5 and ends at 12,3
                using (Line acLine = new Line(line.StartPoint, line.EndPoint))
                {
                    acLine.Layer = layerName;
                    acLine.ColorIndex = 256;
                    acLine.Visible = visible;
                    lineObjId = acBlkTblRec.AppendEntity(acLine);
                    acTrans.AddNewlyCreatedDBObject(acLine, true);
                }

                // Save the new object to the database
                acTrans.Commit();
            }
            return lineObjId;
        }


        public static ObjectId getLineId(Entity ent_SameProperty, Point3d startPoint, Point3d endPoint, bool visible)
        {
            // Get the current document and database
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;

            ObjectId lineObjId = new ObjectId();
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                using (Line acLine = new Line(startPoint, endPoint))
                {
                    acLine.SetPropertiesFrom(ent_SameProperty);
                    acLine.Visible = visible;
                    lineObjId = acBlkTblRec.AppendEntity(acLine);
                    acTrans.AddNewlyCreatedDBObject(acLine, true);
                }

                // Save the new object to the database
                acTrans.Commit();
            }
            return lineObjId;
        }


        public static ObjectId getLineId(Point3d startPoint, Point3d endPoint, string layerName, int layerColor, bool visible)
        {
            CreateLayer(layerName, layerColor);

            // Get the current document and database
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;

            ObjectId lineObjId = new ObjectId();
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // Create a line that starts at 5,5 and ends at 12,3
                using (Line acLine = new Line(startPoint, endPoint))
                {
                    acLine.Layer = layerName;
                    acLine.ColorIndex = 256;
                    acLine.Visible = visible;
                    lineObjId = acBlkTblRec.AppendEntity(acLine);
                    acTrans.AddNewlyCreatedDBObject(acLine, true);
                }

                // Save the new object to the database
                acTrans.Commit();
            }
            return lineObjId;
        }

        public static ObjectId getLineId(double X1, double Y1, double Z1, double X2, double Y2, double Z2, bool visible)
        {
            // Get the current document and database
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;

            ObjectId ObjectIDLIne;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // Create a line that starts at 5,5 and ends at 12,3
                using (Line acLine = new Line(new Point3d(X1, Y1, Z1), new Point3d(X2, Y2, Z2)))
                {
                    acLine.Visible = visible;// visible; Testing-123
                    ObjectIDLIne = acBlkTblRec.AppendEntity(acLine);
                    acTrans.AddNewlyCreatedDBObject(acLine, true);
                }
                // Save the new object to the database
                acTrans.Commit();
            }
            return ObjectIDLIne;
        }
        public static ObjectId getLineId(Entity ent_SameProperty, double X1, double Y1, double Z1, double X2, double Y2, double Z2, bool visible)
        {
            // Get the current document and database
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;

            ObjectId ObjectIDLIne;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // Create a line that starts at 5,5 and ends at 12,3
                using (Line acLine = new Line(new Point3d(X1, Y1, Z1), new Point3d(X2, Y2, Z2)))
                {
                    acLine.SetPropertiesFrom(ent_SameProperty);
                    acLine.Visible = visible;
                    ObjectIDLIne = acBlkTblRec.AppendEntity(acLine);
                    acTrans.AddNewlyCreatedDBObject(acLine, true);
                }
                // Save the new object to the database
                acTrans.Commit();
            }
            return ObjectIDLIne;
        }


        public static ObjectId addEntity(Entity ent)
        {
            // Get the current document and database
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;

            ObjectId ObjectIDLIne;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;


                ObjectIDLIne = acBlkTblRec.AppendEntity(ent);
                acTrans.AddNewlyCreatedDBObject(ent, true);
                // Save the new object to the database
                acTrans.Commit();
            }
            return ObjectIDLIne;
        }

        public static bool IsPointOnEntity(ObjectId polyLineId, Point3d pt)
        {
            var isOn = false;

            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            Entity ent = null;
            using (Transaction tx = db.TransactionManager.StartTransaction())
            {
                //Get the polyline entity
                ent = (Entity)tx.GetObject(polyLineId, OpenMode.ForRead);
                if (ent is Curve)
                {
                    var cur = ent as Curve;
                    var clsPt = cur.GetClosestPointTo(pt, false);
                    isOn = clsPt.DistanceTo(pt) < 1e-6;
                }
                tx.Abort();
            }

            return isOn;
        }



        public static bool IsPointOnPolylineOnSegmentExtend(ObjectId polyLineId, Point3d pt)
        {
            var isOn = false;

            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            Entity ent = null;
            using (Transaction tx = db.TransactionManager.StartTransaction())
            {
                //Get the polyline entity
                ent = (Entity)tx.GetObject(polyLineId, OpenMode.ForRead);
                if (ent is Polyline)
                {
                    var cur = ent as Polyline;

                    for (var n = 0; n < cur.NumberOfVertices; ++n)
                    {
                        Line3d line = null;
                        if (n == (cur.NumberOfVertices - 1))
                        {
                            line = new Line3d(cur.GetPoint3dAt(n), cur.GetPoint3dAt(0));
                        }
                        else
                        {
                            line = new Line3d(cur.GetPoint3dAt(n), cur.GetPoint3dAt(n + 1));
                        }
                        if (line.IsOn(pt))
                        {
                            isOn = true;
                            break;
                        }
                    }
                }
                tx.Abort();
            }

            return false;
        }
        public static List<Point3d> InterSectionPointBetweenPolyLineAndLine(ObjectId polyLineId, ObjectId lineId, Intersect intersectOption = Intersect.ExtendArgument)
        {
            List<Point3d> listOfInsectPoint = new List<Point3d>();

            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            Autodesk.AutoCAD.DatabaseServices.Polyline pl1 = null;
            Autodesk.AutoCAD.DatabaseServices.Line pl2 = null;
            Entity ent = null;
            using (Transaction tx = db.TransactionManager.StartTransaction())
            {
                //Get the polyline entity
                ent = (Entity)tx.GetObject(polyLineId, OpenMode.ForRead);
                if (ent is Autodesk.AutoCAD.DatabaseServices.Polyline)
                {
                    pl1 = ent as Autodesk.AutoCAD.DatabaseServices.Polyline;
                }

                //Get the line entity
                ent = (Entity)tx.GetObject(lineId, OpenMode.ForRead);
                if (ent is Autodesk.AutoCAD.DatabaseServices.Line)
                {
                    pl2 = ent as Autodesk.AutoCAD.DatabaseServices.Line;
                }
                Point3dCollection pts3D = new Point3dCollection();

                //Get the intersection Points between line 1 and line 2
                pl1.IntersectWith(pl2, intersectOption, pts3D, IntPtr.Zero, IntPtr.Zero);

                foreach (Point3d pt in pts3D)
                {
                    listOfInsectPoint.Add(pt);

                    //Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog("\n Intersection Point: " + "\nX = " + pt.X + "\nY = " + pt.Y + "\nZ = " + pt.Z);
                }
                tx.Commit();
            }

            return listOfInsectPoint;
        }
        public static double GetLengthOfLine(double X1, double Y1, double X2, double Y2)
        {
            double length = 0;
            using (Line acLine = new Line(new Point3d(X1, Y1, 0), new Point3d(X2, Y2, 0)))
            {
                length = acLine.Length;
            }
            return length;
        }

        public static double getAngleOfPolyLines(Polyline acPoly, int vertexIndex)
        {
            LineSegment2d lineSegment = acPoly.GetLineSegment2dAt(vertexIndex);
            Vector2d vec = lineSegment.Direction;
            double ang = vec.Angle; // angle in radians
            double angleOfVertex = 0;

            if (ang == 0)
            {
                angleOfVertex = 90.0;
            }

            if (ang > 0 && ang < Math.PI / 2)
            {
                angleOfVertex = 90 - convertRadianToDegree(ang);
            }

            if (ang == Math.PI / 2)
            {
                angleOfVertex = 0.0;
            }
            if (ang > Math.PI / 2 && ang < Math.PI)
            {
                angleOfVertex = convertRadianToDegree(ang) + 180;
            }
            if (ang == Math.PI)
            {
                angleOfVertex = 270.0;
            }
            if (ang > Math.PI && ang < Math.PI * 1.5)
            {
                angleOfVertex = convertRadianToDegree(ang);
            }
            if (ang == Math.PI * 1.5)
            {
                angleOfVertex = 180.0;
            }
            if (ang > Math.PI * 1.5 && ang < Math.PI * 2)
            {
                angleOfVertex = convertRadianToDegree(ang) - 180.0;
            }
            if (ang == Math.PI * 2)
            {
                angleOfVertex = 90;
            }
            angleOfVertex = Math.Round(angleOfVertex);
            return angleOfVertex;
        }
        public static double convertRadianToDegree(double rads)
        {
            return rads * 180 / Math.PI;
        }
        public static List<double> getMinMaxPoint(List<Point3d> listOfPoints_X)
        {
            List<double> listOfPoint = new List<double>();

            double intersect_X1_X_Dir = listOfPoints_X.Min(point => point.X);
            double intersect_X2_X_Dir = listOfPoints_X.Max(point => point.X);

            double intersect_Y1_X_Dir = listOfPoints_X.Min(point => point.Y);
            double intersect_Y2_X_Dir = listOfPoints_X.Max(point => point.Y);

            listOfPoint.Add(intersect_X1_X_Dir);
            listOfPoint.Add(intersect_Y1_X_Dir);
            listOfPoint.Add(intersect_X2_X_Dir);
            listOfPoint.Add(intersect_Y2_X_Dir);

            return listOfPoint;
        }
        public static void deleteEntitiesOnLayer(string layerName)
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            TypedValue[] tvs = new TypedValue[1] { new TypedValue(Convert.ToInt32(DxfCode.LayerName), layerName) };
            SelectionFilter sf = new SelectionFilter(tvs);
            PromptSelectionResult psr = ed.SelectAll(sf);
            if (psr.Status == PromptStatus.OK)
            {
                SelectionSet selset = psr.Value;
                ObjectId[] ids = selset.GetObjectIds();
                List<ObjectId> spaceIds = new List<ObjectId>();

                for (int i = 0; i <= ids.Length - 1; i++)
                {
                    spaceIds.Add(ids[i]);
                    if (TryGetLayer(spaceIds[i], layerName))
                    {
                        try
                        {
                            Erase(spaceIds[i]);
                        }
                        catch (System.Exception ex)
                        {
                            ed.WriteMessage($"\nError: {ex.Message}");
                        }

                    }
                }
            }

        }
        public static void EraseLayer(string sLayerName)
        {
            // Get the current document and database
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Layer table for read
                LayerTable acLyrTbl;
                acLyrTbl = acTrans.GetObject(acCurDb.LayerTableId, OpenMode.ForRead) as LayerTable;

                if (acLyrTbl.Has(sLayerName) == true)
                {
                    // Check to see if it is safe to erase layer
                    ObjectIdCollection acObjIdColl = new ObjectIdCollection();
                    acObjIdColl.Add(acLyrTbl[sLayerName]);
                    acCurDb.Purge(acObjIdColl);
                    LayerTableRecord acLyrTblRec;
                    acLyrTblRec = acTrans.GetObject(acObjIdColl[0], OpenMode.ForWrite) as LayerTableRecord;
                    try
                    {
                        deleteEntitiesOnLayer(sLayerName);
                        // Erase the unreferenced layer
                        acLyrTblRec.Erase(true);
                        // Save the changes and dispose of the transaction
                        acTrans.Commit();
                    }
                    catch (Autodesk.AutoCAD.Runtime.Exception Ex)
                    {
                        // Layer could not be deleted
                        Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog("Error:\n" + Ex.Message);
                    }
                }
            }
        }
        public static bool TryGetLayer(ObjectId id, string layerName)
        {
            if (id.ObjectClass.IsDerivedFrom(RXObject.GetClass(typeof(Entity))))
                using (var tr = id.Database.TransactionManager.StartOpenCloseTransaction())
                {
                    layerName = ((Entity)tr.GetObject(id, OpenMode.ForRead)).Layer;
                    return true;
                }
            layerName = null;
            return false;
        }
        public static void Erase(ObjectId id)
        {
            try
            {
                if (!id.IsValid)
                {
                    return;
                }
                if (id.Database.TransactionManager.TopTransaction != null)
                    id.GetObject(OpenMode.ForWrite).Erase();
                else
#pragma warning disable CS0618
                    using (var obj = id.Open(OpenMode.ForWrite))
                        obj.Erase();
#pragma warning restore CS0618
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
        }
        public static void ChangeLayerAsPerDifferentEntity(string oldLayerName, string dimensionLayer, string textLayerName)
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            // We won't validate whether the layer exists -
            // we'll just see what's returned by the selection.
            try
            {
                TypedValue[] tvs = new TypedValue[1];
                tvs[0] = new TypedValue((int)DxfCode.LayerName, oldLayerName);
                SelectionFilter sf = new SelectionFilter(tvs);
                PromptSelectionResult psr = ed.SelectAll(sf);

                if (psr.Value == null)
                {
                    return;
                }
                Transaction tr = db.TransactionManager.StartTransaction();
                using (tr)
                {
                    // This time we do check whether
                    // the layer exists
                    LayerTable lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);

                    // change Rotated and Aligned Dimension entity from layer
                    ChangeLayerOfDimension(ed, psr, tr, lt, dimensionLayer, textLayerName);

                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }

        }
        private static void ChangeLayerOfDimension(Editor ed, PromptSelectionResult psr, Transaction tr, LayerTable lt, string dimensionLayer, string textLayerName)
        {
            if (!lt.Has(dimensionLayer))
                ed.WriteMessage("\nLayer not found.");
            else
            {
                // We have the layer table open, so let's
                // get the layer ID and use that
                ObjectId lid = lt[dimensionLayer];
                foreach (ObjectId id in psr.Value.GetObjectIds())
                {
                    Entity ent = (Entity)tr.GetObject(id, OpenMode.ForWrite);

                    if (ent.GetType() == typeof(RotatedDimension))
                    {
                        ent.LayerId = lid;
                    }
                    else if (ent.GetType() == typeof(AlignedDimension))
                    {
                        ent.LayerId = lid;
                    }
                    else if (ent.GetType() == typeof(RadialDimension))
                    {
                        ent.LayerId = lid;
                    }
                    else if (ent.GetType() == typeof(DiametricDimension))
                    {
                        ent.LayerId = lid;
                    }
                    else if (ent.GetType() == typeof(Point3AngularDimension))
                    {
                        ent.LayerId = lid;
                    }
                    else if (ent.GetType() == typeof(LineAngularDimension2))
                    {
                        ent.LayerId = lid;
                    }
                    else if (ent.GetType() == typeof(MLeader))
                    {
                        ent.LayerId = lid;
                    }
                    else if (ent.GetType() == typeof(DBText))
                    {
                        ent.LayerId = lid;
                    }
                }
            }
            if (!lt.Has(textLayerName))
                ed.WriteMessage("\nLayer not found.");
            else
            {
                // We have the layer table open, so let's
                // get the layer ID and use that
                ObjectId lid = lt[textLayerName];
                foreach (ObjectId id in psr.Value.GetObjectIds())
                {
                    Entity ent = (Entity)tr.GetObject(id, OpenMode.ForWrite);

                    if (ent.GetType() == typeof(DBText))
                    {
                        ent.LayerId = lid;
                    }
                }
            }

        }
        public static void deleteDimension(List<ObjectId> listOfId)
        {
            if (listOfId.Count == 0)
            {
                return;
            }
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            Database acCurDb = doc.Database;

            try
            {
                using (Transaction tr = doc.TransactionManager.StartTransaction())
                {
                    // Open the Block table for read          
                    BlockTable acBlkTbl = tr.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                    // Open the Block table record Model space for write               
                    BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                    foreach (var id in listOfId)
                    {
                        Entity ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
                        if (ent is Dimension)
                        {
                            Erase(id);
                        }
                        else if (ent is MLeader)
                        {
                            Erase(id);
                        }
                        else if (ent is DBText)
                        {
                            Erase(id);
                        }
                    }
                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
        }
        public static void deleteEntity(List<ObjectId> listOfId, string inputLayerName)
        {
            if (listOfId.Count == 0)
            {
                return;
            }
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            Database acCurDb = doc.Database;

            try
            {
                using (Transaction tr = doc.TransactionManager.StartTransaction())
                {
                    // Open the Block table for read          
                    BlockTable acBlkTbl = tr.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                    // Open the Block table record Model space for write               
                    BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                    foreach (var id in listOfId)
                    {
                        Entity ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
                        var layerName = ent.Layer;
                        if (inputLayerName == layerName)
                        {
                            Erase(id);
                        }
                    }
                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
        }
        public static void deleteEntity(List<ObjectId> listOfId)
        {
            if (listOfId.Count == 0)
            {
                return;
            }
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            Database acCurDb = doc.Database;

            try
            {
                using (Transaction tr = doc.TransactionManager.StartTransaction())
                {
                    // Open the Block table for read          
                    BlockTable acBlkTbl = tr.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                    // Open the Block table record Model space for write               
                    BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                    foreach (var id in listOfId)
                    {
                        if(id.IsErased)
                            continue;
                        Erase(id);
                    }
                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
        }

        public static void deleteEntity(ObjectId id)
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database acCurDb = doc.Database;

            try
            {
                if (id.IsValid == false)
                {
                    return;
                }
                using (Transaction tr = doc.TransactionManager.StartTransaction())
                {
                    // Open the Block table for read          
                    BlockTable acBlkTbl = tr.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                    // Open the Block table record Model space for write               
                    BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                    try
                    {
                        Erase(id);
                    }
                    catch (Autodesk.AutoCAD.Runtime.Exception ex)
                    {

                    }
                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
        }

        public static Point2d get_XY(double angle, double length)
        {
            double degree = (angle * Math.PI) / 180;
            double cosTheta = Math.Cos(degree);
            double sinTheta = Math.Sin(degree);

            double x = cosTheta * length;
            double y = sinTheta * length;

            Point2d point = new Point2d(x, y);
            return point;
        }

        public static double getRadiusOfArc(ObjectId id)
        {
            double radius = 0;

            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
            {
                Entity ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
                if (ent is Arc)
                {
                    Arc acArc = ent as Arc;
                    radius = acArc.Radius;
                }
                tr.Commit();
            }
            return radius;
        }
        public static double getRadiusOfCircle(ObjectId id)
        {
            double radius = 0;

            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
            {
                Entity ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
                if (ent is Circle)
                {
                    Circle acCirc = ent as Circle;
                    radius = acCirc.Radius;
                }
                tr.Commit();
            }
            return radius;
        }

        public static Point3d getCenterPointOfArc(ObjectId id)
        {
            Point3d centerPoint = new Point3d();

            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
            {
                Entity ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
                if (ent is Arc)
                {
                    Arc acArc = ent as Arc;
                    centerPoint = new Point3d(acArc.Center.X, acArc.Center.Y, acArc.Center.Z);
                }
                tr.Commit();
            }
            return centerPoint;
        }
        public static Point3d getCenterPointOfCircle(ObjectId id)
        {
            Point3d centerPoint = new Point3d();

            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
            {
                Entity ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
                if (ent is Circle)
                {
                    Circle acCirc = ent as Circle;
                    centerPoint = new Point3d(acCirc.Center.X, acCirc.Center.Y, 0);
                }
                tr.Commit();
            }
            return centerPoint;
        }

        public static List<double> getDimensionOfCircle(ObjectId id)
        {
            List<double> listOfCircleDim = new List<double>();

            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
            {
                Entity ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
                if (ent is Circle)
                {
                    Circle acCir = ent as Circle;
                    listOfCircleDim.Add(acCir.Center.X);
                    listOfCircleDim.Add(acCir.Center.Y);
                    listOfCircleDim.Add(acCir.Center.Z);
                    listOfCircleDim.Add(acCir.Radius);
                }
                tr.Commit();
            }

            return listOfCircleDim;
        }

        public static List<Point3d> InterSectionBetweenTwoEntity(ObjectId ObjId1, ObjectId ObjId2, bool skipOverlap = false)
        {
            List<Point3d> listOfInsectPoint = new List<Point3d>();
            if (ObjId1 == null || ObjId2 == null || ObjId1.IsNull || ObjId2.IsNull)
            {
                return listOfInsectPoint;
            }
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            using (Transaction tx = db.TransactionManager.StartTransaction())
            {
                Entity ent1 = (Entity)tx.GetObject(ObjId1, OpenMode.ForRead);

                Entity ent2 = (Entity)tx.GetObject(ObjId2, OpenMode.ForRead);

                Point3dCollection pts3D = new Point3dCollection();
                //Get the intersection Points between Entity 1 and Entity 2
                ent2.IntersectWith(ent1, Intersect.OnBothOperands, pts3D, IntPtr.Zero, IntPtr.Zero);

                if (pts3D.Count != 0)
                {
                    if (ent1 is Polyline && ent2 is Polyline)
                    {
                        var p1 = ent1 as Polyline;
                        var p2 = ent2 as Polyline;
                        try
                        {

                            var p1_pts = new Point2dCollection();
                            var p2_pts = new Point2dCollection();
                            for (int i = 0; i < p1.NumberOfVertices; i++)
                            {
                                p1_pts.Add(p1.GetPoint2dAt(i));
                            }
                            for (int i = 0; i < p2.NumberOfVertices; i++)
                            {
                                p2_pts.Add(p2.GetPoint2dAt(i));
                            }
                            //var pp = new Point3dCollection();
                            //for (int i = 0; i < pts3D.Count; i++)
                            //{
                            //    var _p = pts3D[i];
                            //    if (p1_pts.Contains(new Point2d(_p.X, _p.Y)) || p2_pts.Contains(new Point2d(_p.X, _p.Y)))
                            //        pp.Add(_p);
                            //}
                            //for (int i = 0; i < pp.Count; i++)
                            //{
                            //    pts3D.Remove(pp[i]);
                            //}

                        }
                        catch (System.Exception)
                        {
                        }
                    }
                }

                foreach (Point3d pt in pts3D)
                {
                    listOfInsectPoint.Add(pt);

                }
                if (skipOverlap)
                {
                    DBObjectCollection ents1 = new DBObjectCollection();
                    DBObjectCollection ents2 = new DBObjectCollection();
                    ent1.Explode(ents1);
                    ent2.Explode(ents2);
                    Entity me1 = null;
                    Entity me2 = null;
                    foreach (Entity e1 in ents1)
                    {
                        if (e1 == null)
                        {
                            continue;
                        }
                        var allExists = isAllExist(e1, pts3D);
                        if (allExists)
                        {
                            me1 = e1;
                            break;
                        }
                    }
                    foreach (Entity e2 in ents2)
                    {
                        if (e2 == null)
                        {
                            continue;
                        }
                        var allExists = isAllExist(e2, pts3D);
                        if (allExists)
                        {
                            me2 = e2;
                            break;
                        }
                    }
                    if (me1 != null && me2 != null && me1 is Line && me2 is Line)
                    {
                        var l1 = me1 as Line;
                        var l2 = me2 as Line;
                        var xline = new Line3d(l1.StartPoint, l1.EndPoint);
                        if (xline.IsOn(l2.StartPoint) && xline.IsOn(l2.EndPoint))
                        {
                            listOfInsectPoint.Clear();
                        }
                    }
                }
                tx.Commit();
            }
            return listOfInsectPoint;
        }


        private static bool isAllExist(Entity e1, Point3dCollection pts3D)
        {
            if (pts3D.Count == 0)
            {
                return false;
            }
            if (e1 is Curve)
            {
                var c = e1 as Curve;
                foreach (Point3d pt in pts3D)
                {

                    var d = c.GetDistAtPoint(pt);
                    if (d > 1e-6)
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public static List<Point3d> InterSectionBetweenTwoEntity_WithExtend(ObjectId ObjId1, ObjectId ObjId2)
        {
            List<Point3d> listOfInsectPoint = new List<Point3d>();

            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            using (Transaction tx = db.TransactionManager.StartTransaction())
            {
                Entity ent1 = (Entity)tx.GetObject(ObjId1, OpenMode.ForRead);

                Entity ent2 = (Entity)tx.GetObject(ObjId2, OpenMode.ForRead);

                Point3dCollection pts3D = new Point3dCollection();

                //Get the intersection Points between Entity 1 and Entity 2
                ent1.IntersectWith(ent2, Intersect.OnBothOperands, pts3D, IntPtr.Zero, IntPtr.Zero);

                foreach (Point3d pt in pts3D)
                {
                    listOfInsectPoint.Add(pt);
                }
                if (listOfInsectPoint.Count == 0)
                {
                    Point3dCollection pts1 = new Point3dCollection();
                    ent1.IntersectWith(ent2, Intersect.ExtendThis, pts1, IntPtr.Zero, IntPtr.Zero);

                    foreach (Point3d pt in pts1)
                    {
                        listOfInsectPoint.Add(pt);
                    }
                }
                tx.Commit();
            }
            return listOfInsectPoint;
        }

        public static List<Point3d> InterSectionBetweenTwoEntity(Entity ent1, Entity ent2)
        {
            List<Point3d> listOfInsectPoint = new List<Point3d>();

            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            using (Transaction tx = db.TransactionManager.StartTransaction())
            {
                Point3dCollection pts3D = new Point3dCollection();

                //Get the intersection Points between Entity 1 and Entity 2
                ent1.IntersectWith(ent2, Intersect.OnBothOperands, pts3D, IntPtr.Zero, IntPtr.Zero);

                foreach (Point3d pt in pts3D)
                {
                    listOfInsectPoint.Add(pt);
                }
                tx.Commit();
            }
            return listOfInsectPoint;
        }

        public static double GetDistanceBetweenTwoEntity(ObjectId id1, ObjectId id2)
        {
            double distance = double.NaN;
            try
            {
                var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                var db = doc.Database;
                var ed = doc.Editor;

                using (var tr = db.TransactionManager.StartTransaction())
                {
                    var curve1 = (Curve)tr.GetObject(id1, OpenMode.ForRead);
                    var curve2 = (Curve)tr.GetObject(id2, OpenMode.ForRead);
                    distance = curve1.GetGeCurve().GetDistanceTo(curve2.GetGeCurve());

                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
            return distance;
        }
        public static double GetDistanceBetweenTwoLines(ObjectId id1, ObjectId id2)
        {
            double distanceBetweenTwoCurves = 0;
            try
            {
                Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                Database db = doc.Database;
                Editor ed = doc.Editor;

                List<ObjectId> ids = new List<ObjectId>();

                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    // use the transaction to open these objects.
                    // the transaction will automatically dispose these objects when done
                    // so we don't have to worry about manually disposing them.

                    BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord ms = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    var ent1 = (Entity)tr.GetObject(id1, OpenMode.ForRead);
                    var ent2 = (Entity)tr.GetObject(id2, OpenMode.ForRead);
                    Line curve = ent1 as Line;
                    Line handrailLine = ent2 as Line;

                    //using (Line curve = new Line(new Point3d(0, 20, 0), new Point3d(200, 20, 0)))
                    {
                        //using (Line handrailLine = new Line(new Point3d(-500, 50, 0), new Point3d(500, 50, 0)))
                        {
                            PointOnCurve3d[] pointOnCurve3d = curve.GetGeCurve().GetClosestPointTo(handrailLine.GetGeCurve());

                            // check how many points you have here
                            Point3d pointOnCurveClosestToHandrailLine = pointOnCurve3d.First().Point;

                            Point3d pointOnHandrailLineClosestToCurve = handrailLine.GetClosestPointTo(pointOnCurveClosestToHandrailLine, false);

                            // distance should be 30;
                            distanceBetweenTwoCurves = pointOnCurveClosestToHandrailLine.DistanceTo(pointOnHandrailLineClosestToCurve);
                            //MessageBox.Show(Convert.ToString(distanceBetweenTwoCurves));
                        }

                    }

                    // always remember to either commit or abort your transaction.
                    // this is to improve performance.
                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
            return distanceBetweenTwoCurves;
        }

        public static Point3d GetMidPoint(ObjectId id)
        {
            Point3d midPoint = new Point3d(double.NaN, double.NaN, double.NaN);

            var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var ed = doc.Editor;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var curve = (Curve)tr.GetObject(id, OpenMode.ForRead);

                double d1 = curve.GetDistanceAtParameter(curve.StartParam);
                double d2 = curve.GetDistanceAtParameter(curve.EndParam);
                if (d1 == 0 && d2 == 0)
                {
                    return midPoint;
                }
                midPoint = curve.GetPointAtDist(d1 + ((d2 - d1) / 2.0));

                tr.Commit();
            }
            return midPoint;
        }

        public static Point3d GetMidPoint(Entity ent)
        {
            Point3d midPoint = new Point3d(double.NaN, double.NaN, double.NaN);

            var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var ed = doc.Editor;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                //var curve = (Curve)tr.GetObject(id, OpenMode.ForRead);
                var curve = ent as Curve;
                double d1 = curve.GetDistanceAtParameter(curve.StartParam);
                double d2 = curve.GetDistanceAtParameter(curve.EndParam);
                if (d1 == 0 && d2 == 0)
                {
                    return midPoint;
                }
                midPoint = curve.GetPointAtDist(d1 + ((d2 - d1) / 2.0));

                tr.Commit();
            }
            return midPoint;
        }

        public static void TurnLayerOff(string sLayerName)
        {
            // Get the current document and database
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Layer table for read
                LayerTable acLyrTbl;
                acLyrTbl = acTrans.GetObject(acCurDb.LayerTableId, OpenMode.ForRead) as LayerTable;

                if (acLyrTbl.Has(sLayerName) == false)
                {
                    using (LayerTableRecord acLyrTblRec = new LayerTableRecord())
                    {
                        // Assign the layer a name
                        acLyrTblRec.Name = sLayerName;

                        // Upgrade the Layer table for write
                        acLyrTbl.UpgradeOpen();

                        // Append the new layer to the Layer table and the transaction
                        acLyrTbl.Add(acLyrTblRec);
                        acTrans.AddNewlyCreatedDBObject(acLyrTblRec, true);

                        // Turn the layer off
                        acLyrTblRec.IsOff = true;
                    }
                }
                else
                {
                    LayerTableRecord acLyrTblRec = acTrans.GetObject(acLyrTbl[sLayerName], OpenMode.ForWrite) as LayerTableRecord;
                    // Turn the layer off
                    acLyrTblRec.IsOff = true;
                }
                // Save the changes and dispose of the transaction
                acTrans.Commit();
            }
        }
        public static void TurnLayerOn(List<string> layerList)
        {
            // Get the current document and database
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Layer table for read
                LayerTable acLyrTbl;
                acLyrTbl = acTrans.GetObject(acCurDb.LayerTableId, OpenMode.ForRead) as LayerTable;

                foreach (string sLayerName in layerList)
                {
                    if (acLyrTbl.Has(sLayerName) == false)
                    {
                        using (LayerTableRecord acLyrTblRec = new LayerTableRecord())
                        {
                            // Assign the layer a name
                            acLyrTblRec.Name = sLayerName;

                            // Upgrade the Layer table for write
                            acLyrTbl.UpgradeOpen();

                            // Append the new layer to the Layer table and the transaction
                            acLyrTbl.Add(acLyrTblRec);
                            acTrans.AddNewlyCreatedDBObject(acLyrTblRec, true);

                            // Turn the layer on
                            acLyrTblRec.IsOff = false;
                        }
                    }
                    else
                    {
                        LayerTableRecord acLyrTblRec = acTrans.GetObject(acLyrTbl[sLayerName], OpenMode.ForWrite) as LayerTableRecord;
                        // Turn the layer on
                        acLyrTblRec.IsOff = false;
                    }
                }
                // Save the changes and dispose of the transaction
                acTrans.Commit();
            }
        }
        public static void changeColor(ObjectId entId, long newColor)
        {
            Database db = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Database;

            Transaction tr = db.TransactionManager.StartTransaction();
            try
            {
                Entity ent = (Entity)tr.GetObject(entId, OpenMode.ForWrite);
                var colorIndex = Convert.ToInt16(newColor);
                ent.ColorIndex = colorIndex;
                tr.Commit();
            }
            catch
            {
                Console.WriteLine("Error in setting the color for the entity");
            }
            finally
            {
                tr.Dispose();
            }
        }
        public static void changeVisibility(ObjectId entId, bool visible)
        {
            Database db = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Database;

            Transaction tr = db.TransactionManager.StartTransaction();
            try
            {
                Entity ent = (Entity)tr.GetObject(entId, OpenMode.ForWrite);
                ent.Visible = visible;
                tr.Commit();
            }
            catch
            {

            }
            finally
            {
                tr.Dispose();
            }
        }

        public static ObjectId CreateMultiLeaderDimension(Point3d firstPoint, double textHeight, double length, double angle, string text, string layerName, int colorIndex)
        {
            ObjectId id = new ObjectId();

            AEE_Utility.CreateLayer(layerName, colorIndex);

            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            using (Transaction Tx = db.TransactionManager.StartTransaction())
            {
                BlockTable table = Tx.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord model = Tx.GetObject(table[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                if (!table.Has("_TagCircle"))
                {
                    ed.WriteMessage(
                        "\nYou need to define a \"BlkLeader\" first...");
                    return id;
                }

                MLeader leader = new MLeader();
                leader.SetDatabaseDefaults();

                leader.ContentType = ContentType.BlockContent;
                leader.BlockContentId = table["_TagCircle"];

                double degree = (angle * 3.141592) / 180;
                double cosTheta = Math.Cos(degree);
                double sinTheta = Math.Sin(degree);
                double x = cosTheta * length;
                double y = sinTheta * length;
                double X = firstPoint.X + (cosTheta * length);
                double Y = firstPoint.Y + (sinTheta * length);

                int leaderNumber = leader.AddLeader();
                int leaderLineNum = leader.AddLeaderLine(leaderNumber);
                leader.ArrowSize = textHeight;
                leader.AddFirstVertex(leaderLineNum, new Point3d(firstPoint.X, firstPoint.Y, firstPoint.Z));
                leader.AddLastVertex(leaderLineNum, new Point3d(x, y, 0));

                Scale3d scale1 = new Scale3d(20, 20, 1);

                leader.BlockPosition = new Point3d(X, Y, 0);
                leader.BlockScale = scale1;
                //Handle Block Attributes

                BlockTableRecord blkLeader = Tx.GetObject(leader.BlockContentId, OpenMode.ForRead) as BlockTableRecord;

                //Doesn't take in consideration oLeader.BlockRotation
                Matrix3d Transfo = Matrix3d.Displacement(leader.BlockPosition.GetAsVector());

                foreach (ObjectId blkEntId in blkLeader)
                {
                    AttributeDefinition AttributeDef = Tx.GetObject(blkEntId, OpenMode.ForRead) as AttributeDefinition;

                    if (AttributeDef != null)
                    {
                        AttributeReference AttributeRef = new AttributeReference();

                        AttributeRef.SetAttributeFromBlock(AttributeDef, Transfo);

                        AttributeRef.Position = AttributeDef.Position.TransformBy(Transfo);

                        AttributeRef.TextString = text;

                        leader.Layer = layerName;
                        leader.BlockColor = leader.Color;  // ByLayer Color Assign               

                        leader.SetBlockAttribute(blkEntId, AttributeRef);
                    }
                }

                id = model.AppendEntity(leader);
                Tx.AddNewlyCreatedDBObject(leader, true);
                ed.UpdateScreen();
                Tx.Commit();
            }
            return id;
        }

        public static ObjectId CreateMultiLeader(Point3d firstPoint, double textHeight, double length, double angle, string text, string layerName, int colorIndex, string sLineTypName, int lineTypeScale)
        {
            ObjectId id = new ObjectId();

            AEE_Utility.CreateLayer(layerName, colorIndex);
            // Get the current database
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            AcDb.Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Linetype table for read              
                LinetypeTable acLineTypTbl = acTrans.GetObject(acCurDb.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;

                if (acLineTypTbl.Has(sLineTypName) == false)
                {
                    acCurDb.LoadLineTypeFile(sLineTypName, "acad.lin");
                }

                // Open the Block table for read                
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write                
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // Create the leader
                using (MLeader acLdr = new MLeader())
                {
                    int leaderNumber = acLdr.AddLeader();
                    int leaderLineNum = acLdr.AddLeaderLine(leaderNumber);
                    acLdr.ArrowSize = textHeight;

                    double degree = (angle * 3.141592) / 180;
                    double cosTheta = Math.Cos(degree);
                    double sinTheta = Math.Sin(degree);
                    double x = cosTheta * length;
                    double y = sinTheta * length;
                    double X = firstPoint.X + (cosTheta * length);
                    double Y = firstPoint.Y + (sinTheta * length);

                    acLdr.AddFirstVertex(leaderLineNum, new Point3d(firstPoint.X, firstPoint.Y, firstPoint.Z));
                    acLdr.AddLastVertex(leaderLineNum, new Point3d(X, Y, 0));

                    MText mt = new MText();
                    mt.Location = new Point3d(0, 0, 0);
                    mt.TextHeight = textHeight;
                    mt.Contents = text;

                    acLdr.MText = mt;
                    acLdr.Layer = layerName;
                    acLdr.ColorIndex = 256;
                    acLdr.Linetype = sLineTypName;
                    acLdr.LinetypeScale = lineTypeScale;
                    acLdr.TextStyleId = acCurDb.CurrentSpaceId;
                    acLdr.MLeaderStyle = acCurDb.MLeaderstyle;
                    acLdr.LandingGap = 2;
                    acBlkTbl.UpgradeOpen();
                    // Add the new object to Model space and the transaction
                    id = acBlkTblRec.AppendEntity(acLdr);
                    acTrans.AddNewlyCreatedDBObject(acLdr, true);
                    ed.UpdateScreen();
                }
                // Commit the changes and dispose of the transaction
                acTrans.Commit();
            }
            return id;
        }

        public static void CreatingMleaderStyle()
        {
            var acad = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager;
            Document doc = acad.MdiActiveDocument;
            Editor ed = doc.Editor;
            AcDb.Database db = doc.Database;
            using (AcDb.Transaction tr = db.TransactionManager.StartTransaction())
            {
                AcDb.DBDictionary mleaderStylesDict = tr.GetObject(db.MLeaderStyleDictionaryId,
                   AcDb.OpenMode.ForRead) as AcDb.DBDictionary;
                String mleaderStyleName = "My Mleader Style";
                AcDb.MLeaderStyle mleaderStyle;
                if (mleaderStylesDict.Contains(mleaderStyleName))
                    mleaderStyle = (AcDb.MLeaderStyle)tr.GetObject(
                      (AcDb.ObjectId)mleaderStylesDict[mleaderStyleName], AcDb.OpenMode.ForWrite);
                else
                {
                    mleaderStyle = new AcDb.MLeaderStyle();
                    AcDb.ObjectId mleaderStyleId = mleaderStyle.PostMLeaderStyleToDb(db, mleaderStyleName);
                    tr.AddNewlyCreatedDBObject(mleaderStyle, true);
                }

                // Below the detailed settings of the "Modify Multileader Style" dialog box

                // "Leader Format" tab ***

                // "General" group:

                mleaderStyle.LeaderLineType = AcDb.LeaderType.StraightLeader; // Type
                mleaderStyle.LeaderLineColor = Color.FromColorIndex(ColorMethod.None, 100); // Color
                mleaderStyle.LeaderLineTypeId = db.ContinuousLinetype; // Linetype
                mleaderStyle.LeaderLineWeight = AcDb.LineWeight.LineWeight013; // Lineweight

                // "Arrowhead" group:

                //make the arrow head as DOT.
                AcDb.BlockTable blockTable = tr.GetObject(db.BlockTableId,
                                   AcDb.OpenMode.ForRead) as AcDb.BlockTable;
                if (!blockTable.Has("_DOT"))
                {
                    /* load the "_DOT" block definition */
                    Autodesk.AutoCAD.ApplicationServices.Application.SetSystemVariable("DIMBLK", "_DOT");
                }
                mleaderStyle.ArrowSymbolId = blockTable["_DOT"]; // Symbol
                mleaderStyle.ArrowSize = 5.5; // Size

                // "Leader break" group:
                mleaderStyle.BreakSize = 4.4; // Break size

                // "Leader Structure" tab ***

                // "Constrains" group:

                mleaderStyle.MaxLeaderSegmentsPoints = 4; // Maximum leader points
                mleaderStyle.FirstSegmentAngleConstraint = AcDb.AngleConstraint.Degrees045; // First segment angle  
                                                                                            // OR:
                                                                                            // mleaderStyle.FirstSegmentAngleConstraint = AcDb.AngleConstraint.DegreesHorz; // Checked, and value is 0
                                                                                            // mleaderStyle.FirstSegmentAngleConstraint = AcDb.AngleConstraint.DegreesAny; // Unchecked, and value is 0

                mleaderStyle.SecondSegmentAngleConstraint = AcDb.AngleConstraint.Degrees060; // Second segment angle
                                                                                             // OR:
                                                                                             // mleaderStyle.SecondSegmentAngleConstraint = AcDb.AngleConstraint.DegreesHorz; // Checked, and value is 0
                                                                                             // mleaderStyle.SecondSegmentAngleConstraint = AcDb.AngleConstraint.DegreesAny; // Unchecked, and value is 0

                // "Landing settings" group:

                // I can't to check\uncheck the "Automatically include landing" and "Set landing distance" 
                // (and it's value) on "Leading settings" group on "Leader Structure" tab through .NET, or COM:
                // AutoCAD API does not provide direct access to its UI.             

                // "Scale" group:
                mleaderStyle.Annotative = AcDb.AnnotativeStates.False; // Annotative
                                                                       // mleaderStyle.Scale = 0; // Scale multileaders to layout
                mleaderStyle.Scale = 1.55; // Specify scale


                // "Content" tab ***               

                // *** if Multileader type is MText ***
                mleaderStyle.ContentType = AcDb.ContentType.MTextContent; // Multileader type

                // "Text options" group:
                AcDb.MText defaultMText = new AcDb.MText();
                defaultMText.SetDatabaseDefaults();
                defaultMText.Contents = "Hello World";
                mleaderStyle.DefaultMText = defaultMText; // Default text
                mleaderStyle.TextStyleId = db.Textstyle; // Text style
                mleaderStyle.TextAngleType = AcDb.TextAngleType.InsertAngle; // Text angle
                mleaderStyle.TextColor = Color.FromColorIndex(ColorMethod.None, 100); // Text color
                mleaderStyle.TextHeight = 3.55; // Text height
                mleaderStyle.TextAlignAlwaysLeft = true; // Alwais left justify
                mleaderStyle.EnableFrameText = true; // Frame text

                // "Leader connection" group:
                mleaderStyle.TextAttachmentType = AcDb.TextAttachmentType.AttachmentMiddle;

                mleaderStyle.SetTextAttachmentType(AcDb.TextAttachmentType.AttachmentMiddleOfTop,
                   AcDb.LeaderDirectionType.LeftLeader); // Left attachment
                mleaderStyle.SetTextAttachmentType(AcDb.TextAttachmentType.AttachmentMiddleOfBottom,
                    AcDb.LeaderDirectionType.RightLeader); // Right attachment
                mleaderStyle.LandingGap = 7.7; // Landing gap  
#if !Acad2009
                // AutoCAD 2009 has not such properties:
                // mleaderStyle.TextAttachmentDirection = AcDb.TextAttachmentDirection.AttachmentHorizontal; // Horisontal attachment
                mleaderStyle.TextAttachmentDirection = AcDb.TextAttachmentDirection.AttachmentVertical; // Vertical attachment
                mleaderStyle.ExtendLeaderToText = false; // Extend leader to text
#endif
                // ***

                // *** if Multileader type is Block ***
                mleaderStyle.ContentType = AcDb.ContentType.BlockContent; // Multileader type

                // "Block options" group:     

                // Create some block definition
                String blockName = "My Block";
                try
                {
                    AcDb.SymbolUtilityServices.ValidateSymbolName(blockName, false);
                    AcDb.BlockTable bt = tr.GetObject(db.BlockTableId, AcDb.OpenMode.ForWrite) as AcDb.BlockTable;
                    AcDb.ObjectId blockId = AcDb.ObjectId.Null;

                    if (bt.Has(blockName))
                        blockId = bt[blockName];
                    else
                    {
                        AcDb.BlockTableRecord block = new AcDb.BlockTableRecord();
                        block.Name = blockName;
                        block.Annotative = AcDb.AnnotativeStates.True;
                        bt.Add(block);
                        tr.AddNewlyCreatedDBObject(block, true);
                        // Circle
                        AcDb.Circle circle = new AcDb.Circle();
                        circle.Center = new Point3d(0, 0, 0);
                        circle.Radius = 10.0;
                        circle.SetDatabaseDefaults();
                        block.AppendEntity(circle);
                        tr.AddNewlyCreatedDBObject(circle, true);
                        // Attribute definition
                        AcDb.AttributeDefinition attDef = new AcDb.AttributeDefinition();
                        attDef.Position = new Point3d(0, 0, 0);
                        attDef.Tag = "My attribute";
#if Acad2009
                         attDef.TextStyle = db.Textstyle;
#else
                        attDef.TextStyleId = db.Textstyle;
#endif
                        attDef.Prompt = "Attribute value";
                        attDef.Preset = true;
                        attDef.TextString = "123";
                        attDef.Justify = AcDb.AttachmentPoint.MiddleCenter;
                        attDef.Height = 5;
                        attDef.Annotative = AcDb.AnnotativeStates.True;
                        block.AppendEntity(attDef);
                        tr.AddNewlyCreatedDBObject(attDef, true);
                        blockId = block.ObjectId;
                    }
                    mleaderStyle.BlockId = blockId; // Source block
                    Scale3d scale3d = new Scale3d(3.35, 3.35, 3.35);
                    mleaderStyle.BlockScale = scale3d; // Scale                
                    mleaderStyle.BlockConnectionType = AcDb.BlockConnectionType.ConnectBase; // Attachment
                    mleaderStyle.BlockColor = Color.FromColorIndex(ColorMethod.None, 100); // Block color
                                                                                           // ***
                }
                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                {
                    ed.WriteMessage("Exception: {0}\n", ex.Message);
                }
                ed.UpdateScreen();
                tr.Commit();
            }
        }

        public static ObjectId CreateMultiLeader(Point3d firstPoint, Point3d textLocPoint, double textHeight, double length, double angle, string text, string layerName, int colorIndex, string sLineTypName, int lineTypeScale)
        {
            ObjectId id = new ObjectId();

            AEE_Utility.CreateLayer(layerName, colorIndex);
            // Get the current database
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Linetype table for read              
                LinetypeTable acLineTypTbl = acTrans.GetObject(acCurDb.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;

                if (acLineTypTbl.Has(sLineTypName) == false)
                {
                    acCurDb.LoadLineTypeFile(sLineTypName, "acad.lin");
                }

                // Open the Block table for read                
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write                
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // Create the leader
                using (MLeader acLdr = new MLeader())
                {
                    int leaderNumber = acLdr.AddLeader();
                    int leaderLineNum = acLdr.AddLeaderLine(leaderNumber);
                    acLdr.ArrowSize = textHeight;

                    double degree = (angle * 3.141592) / 180;
                    double cosTheta = Math.Cos(degree);
                    double sinTheta = Math.Sin(degree);
                    double x = cosTheta * length;
                    double y = sinTheta * length;
                    double X = firstPoint.X + (cosTheta * length);
                    double Y = firstPoint.Y + (sinTheta * length);

                    //acLdr.LandingGap = length / 2;

                    acLdr.AddFirstVertex(leaderLineNum, new Point3d(firstPoint.X, firstPoint.Y, firstPoint.Z));
                    acLdr.AddLastVertex(leaderLineNum, new Point3d(textLocPoint.X, textLocPoint.Y, textLocPoint.Z));


                    MText mt = new MText();
                    mt.SetContentsRtf("MLeader");
                    mt.Location = new Point3d(textLocPoint.X, textLocPoint.Y, textLocPoint.Z);

                    mt.TextHeight = textHeight;
                    mt.Contents = text;
                    //mt.Attachment = AttachmentPoint.BaseLeft;
                    //mt.SetAttachmentMovingLocation(AttachmentPoint.BottomLeft);

                    acLdr.MText = mt;
                    acLdr.Layer = layerName;
                    acLdr.ColorIndex = 256;

                    acLdr.Linetype = sLineTypName;
                    acLdr.LinetypeScale = lineTypeScale;

                    acBlkTbl.UpgradeOpen();
                    // Add the new object to Model space and the transaction
                    id = acBlkTblRec.AppendEntity(acLdr);
                    acTrans.AddNewlyCreatedDBObject(acLdr, true);
                    ed.UpdateScreen();
                }
                // Commit the changes and dispose of the transaction
                acTrans.Commit();
            }
            return id;
        }

        public static ObjectId CreateMultiLeader(Point3d firstPoint, Point3d secondPoint, Point3d textLocPoint, double textHeight, double length, double angle, string text, string layerName, int colorIndex, string sLineTypName, int lineTypeScale)
        {
            ObjectId id = new ObjectId();

            AEE_Utility.CreateLayer(layerName, colorIndex);
            // Get the current database
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Linetype table for read              
                LinetypeTable acLineTypTbl = acTrans.GetObject(acCurDb.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;

                if (acLineTypTbl.Has(sLineTypName) == false)
                {
                    acCurDb.LoadLineTypeFile(sLineTypName, "acad.lin");
                }

                // Open the Block table for read                
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write                
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // Create the leader
                using (MLeader acLdr = new MLeader())
                {
                    int leaderNumber = acLdr.AddLeader();
                    int leaderLineNum = acLdr.AddLeaderLine(leaderNumber);
                    acLdr.ArrowSize = textHeight;

                    double degree = (angle * 3.141592) / 180;
                    double cosTheta = Math.Cos(degree);
                    double sinTheta = Math.Sin(degree);
                    double x = cosTheta * length;
                    double y = sinTheta * length;
                    double X = firstPoint.X + (cosTheta * length);
                    double Y = firstPoint.Y + (sinTheta * length);

                    //acLdr.LandingGap = length / 2;

                    acLdr.AddFirstVertex(leaderLineNum, new Point3d(firstPoint.X, firstPoint.Y, firstPoint.Z));
                    acLdr.AddLastVertex(leaderLineNum, new Point3d(secondPoint.X, secondPoint.Y, secondPoint.Z));


                    MText mt = new MText();
                    mt.SetContentsRtf("MLeader");
                    mt.Location = new Point3d(textLocPoint.X, textLocPoint.Y, textLocPoint.Z);

                    mt.TextHeight = textHeight;
                    mt.Contents = text;
                    //mt.Attachment = AttachmentPoint.MiddleCenter;
                    //mt.SetAttachmentMovingLocation(AttachmentPoint.MiddleCenter);

                    acLdr.MText = mt;
                    acLdr.Layer = layerName;
                    acLdr.ColorIndex = 256;

                    acLdr.Linetype = sLineTypName;
                    acLdr.LinetypeScale = lineTypeScale;

                    acBlkTbl.UpgradeOpen();
                    // Add the new object to Model space and the transaction
                    id = acBlkTblRec.AppendEntity(acLdr);
                    acTrans.AddNewlyCreatedDBObject(acLdr, true);
                    ed.UpdateScreen();
                }
                // Commit the changes and dispose of the transaction
                acTrans.Commit();
            }
            return id;
        }


        public static List<Point2d> GetPolylineVertexPoint(ObjectId id)
        {
            List<Point2d> listOfVertPoints = new List<Point2d>();
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            try
            {
                Transaction tr = db.TransactionManager.StartTransaction();
                using (tr)
                {
                    var obj = tr.GetObject(id, OpenMode.ForRead);
                    if (obj == null)
                    {
                        return listOfVertPoints;
                    }
                    Entity ent = obj as Entity;
                    if (ent is Polyline)
                    {
                        Polyline lwp = ent as Polyline;
                        if (lwp != null)
                        {
                            // Use a for loop to get each vertex, one by one
                            int vn = lwp.NumberOfVertices;
                            for (int i = 0; i < vn; i++)
                            {
                                // Could also get the 3D point here
                                var pt = lwp.GetPoint2dAt(i);
                                listOfVertPoints.Add(pt);
                            }
                        }
                    }
                    // Committing is cheaper than aborting
                    tr.Commit();
                }
            }
            catch (System.Exception)
            {


            }

            return listOfVertPoints;
        }


        public static List<Point2d> GetPolylineVertexPoint(Entity polylineEnt)
        {
            List<Point2d> listOfVertPoints = new List<Point2d>();
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            Transaction tr = db.TransactionManager.StartTransaction();
            using (tr)
            {
                if (polylineEnt is Polyline)
                {
                    Polyline lwp = polylineEnt as Polyline;
                    if (lwp != null)
                    {
                        // Use a for loop to get each vertex, one by one
                        int vn = lwp.NumberOfVertices;
                        for (int i = 0; i < vn; i++)
                        {
                            // Could also get the 3D point here
                            var pt = lwp.GetPoint2dAt(i);
                            listOfVertPoints.Add(pt);
                        }
                    }
                }
                // Committing is cheaper than aborting
                tr.Commit();
            }
            return listOfVertPoints;
        }


        public static ObjectIdCollection TraceBoundary(Point3d insideBoundaryPoint)
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            ObjectIdCollection ids = new ObjectIdCollection();

            // Select a seed point for our boundary
            // Get the objects making up our boundary

            DBObjectCollection objs = ed.TraceBoundary(insideBoundaryPoint, true);

            if (objs.Count > 0)
            {
                Transaction tr = doc.TransactionManager.StartTransaction();
                using (tr)
                {
                    // We'll add the objects to the model space

                    BlockTable bt = (BlockTable)tr.GetObject(doc.Database.BlockTableId, OpenMode.ForRead);

                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                    // Add our boundary objects to the drawing and
                    // collect their ObjectIds for later use


                    foreach (DBObject obj in objs)
                    {
                        Entity ent = obj as Entity;
                        if (ent != null)
                        {
                            // Set our boundary objects to be of
                            // our auto-incremented colour index

                            ent.ColorIndex = 3;

                            // Set the lineweight of our object

                            ent.LineWeight = LineWeight.LineWeight050;

                            // Add each boundary object to the modelspace
                            // and add its ID to a collection

                            ids.Add(btr.AppendEntity(ent));
                            tr.AddNewlyCreatedDBObject(ent, true);
                        }
                    }
                    // Commit the transaction

                    tr.Commit();
                }
            }
            return ids;
        }

        public static bool getPointIsInside_ClosedPolyline(Point3d point, ObjectId objId)
        {
            bool isInside = false;
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Entity ent = (Entity)tr.GetObject(objId, OpenMode.ForRead);

                if (ent is Polyline)
                {
                    Polyline poly = ent as Polyline;
                    if (poly != null)
                    {
                        int num = poly.NumberOfVertices;
                        double[] polyX = new double[num];
                        double[] polyY = new double[num];
                        for (int i = 0; i < num; i++)
                        {
                            Point2d p = poly.GetPoint2dAt(i);
                            polyX[i] = p.X;
                            polyY[i] = p.Y;
                        }
                        isInside = pointInSidePolyLine(num, polyX, polyY, point);
                    }
                }
                tr.Commit();
            }
            return isInside;
        }


        private static bool pointInSidePolyLine(int polySides, double[] polyX, double[] polyY, Point3d inputPoint)
        {
            int i, j = polySides - 1;
            bool oddNodes = false;
            double x = inputPoint.X;
            double y = inputPoint.Y;
            for (i = 0; i < polySides; i++)
            {
                if ((polyY[i] < y && polyY[j] >= y) || (polyY[j] < y && polyY[i] >= y))
                {
                    if (polyX[i] + (y - polyY[i]) / (polyY[j] - polyY[i]) * (polyX[j] - polyX[i]) < x)
                    {
                        oddNodes = true;
                        break;
                    }
                }
                j = i;
            }
            return oddNodes;
        }


        public static ObjectId CreateRegion(List<ObjectId> listOfObjectId, bool visible)
        {
            ObjectId regionId = new ObjectId();
            // Add all the objects to clone
            DBObjectCollection acDBObjColl = new DBObjectCollection();

            if (listOfObjectId.Count != 0)
            {
                Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
                Editor ed = acDoc.Editor;
                try
                {
                    using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
                    {
                        // Open the Block table for read              
                        BlockTable acBlkTbl = tr.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                        // Open the Block table record Model space for write            
                        BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                        List<Curve3d> listofCurve = new List<Curve3d>();
                        foreach (var id in listOfObjectId)
                        {
                            Entity ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
                            acDBObjColl.Add(ent);
                        }
                        ////   Calculate the regions based on each closed loop
                        DBObjectCollection myRegionColl = new DBObjectCollection();
                        myRegionColl = Region.CreateFromCurves(acDBObjColl);

                        Region acRegion = myRegionColl[0] as Region;

                        ////   Add the new object to the block table record and the transaction
                        acRegion.Visible = visible;
                        regionId = acBlkTblRec.AppendEntity(acRegion);
                        tr.AddNewlyCreatedDBObject(acRegion, true);
                        ed.UpdateScreen();
                        tr.Commit();
                    }
                }
                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                {

                }
            }
            return regionId;
        }


        public static ObjectId CreateRegion(ObjectId objId, bool visible)
        {
            ObjectId regionId = new ObjectId();
            // Add all the objects to clone
            DBObjectCollection acDBObjColl = new DBObjectCollection();
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            try
            {
                using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
                {
                    // Open the Block table for read              
                    BlockTable acBlkTbl = tr.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                    // Open the Block table record Model space for write            
                    BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    Entity ent = tr.GetObject(objId, OpenMode.ForRead) as Entity;
                    acDBObjColl.Add(ent);

                    ////   Calculate the regions based on each closed loop
                    DBObjectCollection myRegionColl = new DBObjectCollection();
                    myRegionColl = Region.CreateFromCurves(acDBObjColl);
                    Region acRegion = myRegionColl[0] as Region;

                    ////   Add the new object to the block table record and the transaction
                    acRegion.Visible = visible;
                    regionId = acBlkTblRec.AppendEntity(acRegion);
                    tr.AddNewlyCreatedDBObject(acRegion, true);
                    ed.UpdateScreen();
                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
            return regionId;
        }


        public static double GetAreaOfRegion(ObjectId regionId)
        {
            double Area = 0;
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            try
            {
                using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
                {
                    Entity ent = tr.GetObject(regionId, OpenMode.ForRead) as Entity;
                    if (ent is Region)
                    {
                        Region acRegion = ent as Region;
                        Area = acRegion.Area;
                    }
                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
            return Area;
        }


        public static void SetUCSInWorld()
        {
            // Get the current document and database, and start a transaction
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table record for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                ed.CurrentUserCoordinateSystem = new Matrix3d(new double[16]{ 1.0, 0.0, 0.0, 0.0,
                                                                              0.0, 1.0, 0.0, 0.0,
                                                                              0.0, 0.0, 1.0, 0.0,
                                                                              0.0, 0.0, 0.0, 1.0});

                // Save the new objects to the database
                acTrans.Commit();
            }
        }


        public static List<ObjectId> GetObjects(string layerName, string className)
        {
            Autodesk.AutoCAD.ApplicationServices.Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;

            if (doc == null)
                return null;

            Editor ed = doc.Editor;

            List<ObjectId> spaceIds = new List<ObjectId>();

            Database database = HostApplicationServices.WorkingDatabase;

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                TypedValue[] filterlist = new TypedValue[2];
                filterlist.SetValue(new TypedValue((int)DxfCode.Start, className), 0);
                filterlist.SetValue(new TypedValue((int)DxfCode.LayerName, layerName), 1);

                SelectionFilter filter = new SelectionFilter(filterlist);

                PromptSelectionResult res = ed.SelectAll(filter);

                if (res.Status != PromptStatus.OK)
                {
                    return spaceIds;
                }

                SelectionSet selSet = res.Value;
                ObjectId[] ids = selSet.GetObjectIds();

                for (int i = 0; i < ids.Length; i++)
                {
                    spaceIds.Add(ids[i]);
                }

                transaction.Commit();
            }

            return spaceIds;
        }


        public static Entity GetEntityOfLine(ObjectId id)
        {
            Entity output;
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
            {
                Entity ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
                Line acLine = ent as Line;
                output = acLine;
                tr.Commit();
            }
            return output;
        }


        public static Line GetLine(ObjectId id)
        {
            Line acLine;
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
            {
                Entity ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
                acLine = ent as Line;
                tr.Commit();
            }
            return acLine;
        }


        public static Entity GetEntityForRead(ObjectId id)
        {
            Entity output;
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
            {
                Entity ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
                output = ent;

                tr.Commit();
            }
            return output;
        }


        public static Entity GetEntityForWrite(ObjectId id)
        {
            Entity output;
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
            {
                Entity ent = tr.GetObject(id, OpenMode.ForWrite) as Entity;
                output = ent;

                tr.Commit();
            }
            return output;
        }


        public static string GetLayerName(ObjectId id)
        {
            string output;
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
            {
                Entity ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
                output = ent.Layer;

                tr.Abort();
            }
            return output;
        }


        public static bool CreateGroup(string groupName, List<ObjectId> listObjId)
        {
            try
            {
                Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                Database db = doc.Database;
                Editor ed = doc.Editor;

                Transaction tr = db.TransactionManager.StartTransaction();
                using (tr)
                {
                    // Get the group dictionary from the drawing
                    DBDictionary gd = (DBDictionary)tr.GetObject(db.GroupDictionaryId, OpenMode.ForRead);

                    // Create our new group...
                    Group grp = new Group("AEE_Group", true);

                    gd.UpgradeOpen();
                    ObjectId grpId = gd.SetAt(groupName, grp);
                    tr.AddNewlyCreatedDBObject(grp, true);

                    BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);

                    BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                    ObjectIdCollection ids = new ObjectIdCollection();
                    foreach (var id in listObjId)
                    {
                        ids.Add(id);
                    }
                    grp.Append(ids);
                    ed.UpdateScreen();
                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                return false;
            }
            return true;
        }
        public static List<CircularArc2d> GetArcFromPolyLine(Polyline acPoly)
        {
            List<CircularArc2d> listOfCircularArc2d = new List<CircularArc2d>();

            for (int i = 0; i < acPoly.NumberOfVertices; i++)
            {
                Curve3d seg = null;

                SegmentType segType = acPoly.GetSegmentType(i);

                if (segType == SegmentType.Arc)
                {
                    seg = acPoly.GetArcSegmentAt(i);
                    var acArc = acPoly.GetArcSegment2dAt(i);
                    listOfCircularArc2d.Add(acArc);
                }
                else if (segType == SegmentType.Line)
                {
                    seg = acPoly.GetLineSegmentAt(i);
                }
            }
            return listOfCircularArc2d;
        }

        public static List<Point3d> GetBoundingBoxOfRegion(ObjectId id)
        {
            List<Point3d> listOfBoundPoint = new List<Point3d>();
            try
            {
                // Get the current document and database
                Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                Database acCurDb = acDoc.Database;

                // Start a transaction
                using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
                {
                    // Open the Block table for read
                    BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                    // Open the Block table record Model space for write
                    BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    Entity ent = acTrans.GetObject(id, OpenMode.ForWrite) as Entity;
                    if (ent is Region)
                    {
                        Region acRegion = ent as Region;
                        var bound = acRegion.Bounds;
                        var minPoint = bound.Value.MinPoint;
                        var maxPoint = bound.Value.MaxPoint;
                        listOfBoundPoint.Add(minPoint);
                        listOfBoundPoint.Add(maxPoint);
                    }
                    // Save the new objects to the database
                    acTrans.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
            return listOfBoundPoint;
        }
        public static List<Point3d> GetBoundingBoxOfPolyline(ObjectId id)
        {
            List<Point3d> listOfBoundPoint = new List<Point3d>();
            try
            {
                // Get the current document and database
                Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                Database acCurDb = acDoc.Database;

                // Start a transaction
                using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
                {
                    // Open the Block table for read
                    BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                    // Open the Block table record Model space for write
                    BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    Entity ent = acTrans.GetObject(id, OpenMode.ForWrite) as Entity;
                    if (ent is Polyline)
                    {
                        Polyline acPoly = ent as Polyline;
                        var bound = acPoly.Bounds;
                        var minPoint = bound.Value.MinPoint;
                        var maxPoint = bound.Value.MaxPoint;
                        listOfBoundPoint.Add(minPoint);
                        listOfBoundPoint.Add(maxPoint);
                    }
                    // Save the new objects to the database
                    acTrans.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
            return listOfBoundPoint;
        }

        public static ObjectId CreateMultiLineText(string text, double x, double y, double z, double h, string layerName, int colorIndex, double rotationAngle, double fontSizeScale = 1)
        {
            ObjectId textId = new ObjectId();
            //https://adndevblog.typepad.com/autocad/2017/09/dissecting-mtext-format-codes.html

            //if (text == "200 WP 1950")
            //{
            //    MessageBox.Show("Test");
            //}

            CreateLayer(layerName, colorIndex);
            // Get the current document and database
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // Create a multiline text object
                using (MText acMText = new MText())
                {
                    // Open the current text style for write
                    TextStyleTableRecord acTextStyleTblRec;
                    acTextStyleTblRec = acTrans.GetObject(acCurDb.Textstyle, OpenMode.ForWrite) as TextStyleTableRecord;
                    // Get the current font settings
                    Autodesk.AutoCAD.GraphicsInterface.FontDescriptor acFont;
                    acFont = acTextStyleTblRec.Font;
                    // Update the text style's typeface with "Arial Narrow"
                    Autodesk.AutoCAD.GraphicsInterface.FontDescriptor acNewFont;
                    acNewFont = new Autodesk.AutoCAD.GraphicsInterface.FontDescriptor("Arial", acFont.Bold, acFont.Italic,
                                                                   acFont.CharacterSet,
                                                                        acFont.PitchAndFamily);
                    acTextStyleTblRec.Font = acNewFont;


                    acMText.Location = new Point3d(x, y, z);
                    acMText.TextHeight = h * fontSizeScale;
                    acMText.Attachment = AttachmentPoint.MiddleCenter;
                    acMText.Contents = text;

                    acMText.ColorIndex = 256;
                    acMText.Layer = layerName;
                    rotationAngle = (rotationAngle * Math.PI) / 180;
                    acMText.Rotation = rotationAngle;

                    textId = acBlkTblRec.AppendEntity(acMText);
                    acTrans.AddNewlyCreatedDBObject(acMText, true);
                    ed.UpdateScreen();
                }

                // Save the changes and dispose of the transaction
                acTrans.Commit();
            }
            return textId;
        }


        public static ObjectId CreateMultiLineText(string text, double x, double y, double z, double h, string layerName, int colorIndex, double rotationAngle, out Extents3d bb)
        {
            ObjectId textId = new ObjectId();
            //https://adndevblog.typepad.com/autocad/2017/09/dissecting-mtext-format-codes.html

            CreateLayer(layerName, colorIndex);
            // Get the current document and database
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // Create a multiline text object
                using (MText acMText = new MText())
                {
                    // Open the current text style for write
                    TextStyleTableRecord acTextStyleTblRec;
                    acTextStyleTblRec = acTrans.GetObject(acCurDb.Textstyle, OpenMode.ForWrite) as TextStyleTableRecord;
                    // Get the current font settings
                    Autodesk.AutoCAD.GraphicsInterface.FontDescriptor acFont;
                    acFont = acTextStyleTblRec.Font;
                    // Update the text style's typeface with "Arial Narrow"
                    Autodesk.AutoCAD.GraphicsInterface.FontDescriptor acNewFont;
                    acNewFont = new Autodesk.AutoCAD.GraphicsInterface.FontDescriptor("Arial", acFont.Bold, acFont.Italic,
                                                                        acFont.CharacterSet,
                                                                        acFont.PitchAndFamily);
                    acTextStyleTblRec.Font = acNewFont;


                    acMText.Location = new Point3d(x, y, z);
                    acMText.TextHeight = h;
                    acMText.Attachment = AttachmentPoint.MiddleCenter;
                    acMText.Contents = text;

                    acMText.ColorIndex = 256;
                    acMText.Layer = layerName;
                    rotationAngle = (rotationAngle * Math.PI) / 180;
                    acMText.Rotation = rotationAngle;

                    textId = acBlkTblRec.AppendEntity(acMText);
                    acTrans.AddNewlyCreatedDBObject(acMText, true);
                    ed.UpdateScreen();
                    bb = acMText.Bounds.Value;
                }

                // Save the changes and dispose of the transaction
                acTrans.Commit();
            }

            return textId;
        }


        public static ObjectId RotateEntity(ObjectId id, double rotAngle, Point3d basePt)
        {
            try
            {
                // Get the current document and database
                Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                Database acCurDb = acDoc.Database;
                Editor ed = acDoc.Editor;
                // Start a transaction
                using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
                {
                    // Open the Block table for read
                    BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                    // Open the Block table record Model space for write
                    BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    Entity ent = acTrans.GetObject(id, OpenMode.ForWrite) as Entity;

                    ent.TransformBy(Matrix3d.Rotation(rotAngle, Vector3d.ZAxis, basePt));
                    //ed.UpdateScreen();
                    // Save the new objects to the database
                    acTrans.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            { }
            return id;
        }


        public static ObjectId MoveEntity(ObjectId id, Vector3d moveVector)
        {
            try
            {
                // Get the current document and database
                Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                Database acCurDb = acDoc.Database;
                Editor ed = acDoc.Editor;
                // Start a transaction
                using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
                {
                    // Open the Block table for read
                    BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                    // Open the Block table record Model space for write
                    BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    Entity ent = acTrans.GetObject(id, OpenMode.ForWrite) as Entity;

                    ent.TransformBy(Matrix3d.Displacement(moveVector));
                    //ed.UpdateScreen();
                    // Save the new objects to the database
                    acTrans.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            { }
            return id;
        }


        public static void MoveEntity(List<ObjectId> listOfMoveObjectId, Vector3d moveVector)
        {
            if (listOfMoveObjectId.Count == 0)
            {
                return;
            }
            try
            {
                // delete same objectid from list
                listOfMoveObjectId = listOfMoveObjectId.Distinct().ToList();

                // Get the current document and database
                Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                Database acCurDb = acDoc.Database;
                Editor ed = acDoc.Editor;
                // Start a transaction
                using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
                {
                    // Open the Block table for read
                    BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                    // Open the Block table record Model space for write
                    BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    foreach (var id in listOfMoveObjectId)
                    {
                        Entity ent = acTrans.GetObject(id, OpenMode.ForWrite) as Entity;

                        ent.TransformBy(Matrix3d.Displacement(moveVector));
                        //ed.UpdateScreen();
                    }

                    // Save the new objects to the database
                    acTrans.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            { }
        }

        public static List<ObjectId> copyColonEntity(Vector3d pastePoint, List<ObjectId> listOfCopyPasteObjectId)
        {
            List<ObjectId> listOfObjId = new List<ObjectId>();
            // Add all the objects to clone
            DBObjectCollection acDBObjColl = new DBObjectCollection();

            if (listOfCopyPasteObjectId.Count != 0)
            {
                Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
                Editor ed = acDoc.Editor;
                try
                {
                    using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
                    {
                        // Open the Block table for read              
                        BlockTable acBlkTbl = tr.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                        // Open the Block table record Model space for write            
                        BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                        foreach (var id in listOfCopyPasteObjectId)
                        {
                            Entity ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
                            acDBObjColl.Add(ent);
                        }

                        foreach (Entity acEnt in acDBObjColl)
                        {
                            Entity acEntClone = acEnt.Clone() as Entity;

                            // Create a matrix and move each copied entity in selection point
                            acEntClone.TransformBy(Matrix3d.Displacement(new Vector3d(pastePoint.X, pastePoint.Y, pastePoint.Z)));

                            if (pastePoint.X == CreateShellPlanHelper.moveVector_ForSlabJointLayout.X && acEntClone.Layer.ToLower().Contains("room"))
                            {
                                CommonModule.textvalue.Add(((Autodesk.AutoCAD.DatabaseServices.DBText)acEntClone).TextString + "|" + ((Autodesk.AutoCAD.DatabaseServices.DBText)acEntClone).Position.X.ToString() + "," + ((Autodesk.AutoCAD.DatabaseServices.DBText)acEntClone).Position.Y.ToString() + "," + ((Autodesk.AutoCAD.DatabaseServices.DBText)acEntClone).Position.Z.ToString());
                            }

                            // Add the cloned object
                            var id = acBlkTblRec.AppendEntity(acEntClone);
                            tr.AddNewlyCreatedDBObject(acEntClone, true);
                            listOfObjId.Add(id);
                        }
                        ed.UpdateScreen();
                        tr.Commit();
                    }
                }
                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                {

                }
            }
            return listOfObjId;
        }
        public static ObjectId copyColonEntity(Vector3d pastePoint, ObjectId objId)
        {
            ObjectId colonObjId = new ObjectId();
            // Add all the objects to clone
            DBObjectCollection acDBObjColl = new DBObjectCollection();

            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;

            try
            {
                using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
                {
                    // Open the Block table for read              
                    BlockTable acBlkTbl = tr.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                    // Open the Block table record Model space for write            
                    BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    Entity ent = tr.GetObject(objId, OpenMode.ForRead) as Entity;
                    acDBObjColl.Add(ent);

                    foreach (Entity acEnt in acDBObjColl)
                    {
                        Entity acEntClone = acEnt.Clone() as Entity;

                        // Create a matrix and move each copied entity in selection point
                        acEntClone.TransformBy(Matrix3d.Displacement(new Vector3d(pastePoint.X, pastePoint.Y, pastePoint.Z)));

                        // Add the cloned object
                        colonObjId = acBlkTblRec.AppendEntity(acEntClone);
                        tr.AddNewlyCreatedDBObject(acEntClone, true);
                    }
                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
            return colonObjId;
        }
        public static ObjectId copyColonEntity(Vector3d pastePoint, ObjectId objId, bool visible)
        {
            ObjectId colonObjId = new ObjectId();
            // Add all the objects to clone
            DBObjectCollection acDBObjColl = new DBObjectCollection();

            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;

            try
            {
                using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
                {
                    // Open the Block table for read              
                    BlockTable acBlkTbl = tr.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                    // Open the Block table record Model space for write            
                    BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    Entity ent = tr.GetObject(objId, OpenMode.ForRead) as Entity;
                    acDBObjColl.Add(ent);

                    foreach (Entity acEnt in acDBObjColl)
                    {
                        Entity acEntClone = acEnt.Clone() as Entity;
                        acEntClone.Visible = visible;
                        // Create a matrix and move each copied entity in selection point
                        acEntClone.TransformBy(Matrix3d.Displacement(new Vector3d(pastePoint.X, pastePoint.Y, pastePoint.Z)));

                        // Add the cloned object
                        colonObjId = acBlkTblRec.AppendEntity(acEntClone);
                        tr.AddNewlyCreatedDBObject(acEntClone, true);
                    }
                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
            return colonObjId;
        }

        public static ObjectId createColonEntityInSamePoint(Entity ent)
        {
            ObjectId colonRegionId = new ObjectId();
            // Add all the objects to clone
            DBObjectCollection acDBObjColl = new DBObjectCollection();

            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            try
            {
                using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
                {
                    // Open the Block table for read              
                    BlockTable acBlkTbl = tr.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                    // Open the Block table record Model space for write            
                    BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    acDBObjColl.Add(ent);

                    foreach (Entity acEnt in acDBObjColl)
                    {
                        Entity acEntClone = acEnt.Clone() as Entity;
                        Vector3d pastePoint = new Vector3d(0, 0, 0);
                        // Create a matrix and move each copied entity in selection point
                        acEntClone.TransformBy(Matrix3d.Displacement(new Vector3d(pastePoint.X, pastePoint.Y, pastePoint.Z)));
                        acEntClone.Visible = false;
                        //// Add the cloned object
                        colonRegionId = acBlkTblRec.AppendEntity(acEntClone);
                        tr.AddNewlyCreatedDBObject(acEntClone, true);
                    }
                    ed.UpdateScreen();
                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
            return colonRegionId;
        }
        public static ObjectId createColonEntityInSamePoint(Entity ent, bool visible)
        {
            ObjectId colonRegionId = new ObjectId();
            // Add all the objects to clone
            DBObjectCollection acDBObjColl = new DBObjectCollection();

            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            try
            {
                using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
                {
                    // Open the Block table for read              
                    BlockTable acBlkTbl = tr.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                    // Open the Block table record Model space for write            
                    BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    acDBObjColl.Add(ent);

                    foreach (Entity acEnt in acDBObjColl)
                    {
                        Entity acEntClone = acEnt.Clone() as Entity;
                        Vector3d pastePoint = new Vector3d(0, 0, 0);
                        // Create a matrix and move each copied entity in selection point
                        acEntClone.TransformBy(Matrix3d.Displacement(new Vector3d(pastePoint.X, pastePoint.Y, pastePoint.Z)));
                        acEntClone.Visible = visible;
                        //// Add the cloned object
                        colonRegionId = acBlkTblRec.AppendEntity(acEntClone);
                        tr.AddNewlyCreatedDBObject(acEntClone, true);
                    }
                    ed.UpdateScreen();
                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
            return colonRegionId;
        }
        public static ObjectId createColonEntityInSamePoint(Entity ent, bool visible, string layerName, int colorIndex)
        {
            AEE_Utility.CreateLayer(layerName, colorIndex);
            ObjectId colonRegionId = new ObjectId();
            // Add all the objects to clone
            DBObjectCollection acDBObjColl = new DBObjectCollection();

            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            try
            {
                using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
                {
                    // Open the Block table for read              
                    BlockTable acBlkTbl = tr.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                    // Open the Block table record Model space for write            
                    BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    acDBObjColl.Add(ent);

                    foreach (Entity acEnt in acDBObjColl)
                    {
                        Entity acEntClone = acEnt.Clone() as Entity;
                        Vector3d pastePoint = new Vector3d(0, 0, 0);
                        // Create a matrix and move each copied entity in selection point
                        acEntClone.TransformBy(Matrix3d.Displacement(new Vector3d(pastePoint.X, pastePoint.Y, pastePoint.Z)));
                        acEntClone.Visible = visible;
                        acEntClone.Layer = layerName;
                        //// Add the cloned object
                        colonRegionId = acBlkTblRec.AppendEntity(acEntClone);
                        tr.AddNewlyCreatedDBObject(acEntClone, true);
                    }
                    ed.UpdateScreen();
                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
            return colonRegionId;
        }
        public static ObjectId changeLayer(ObjectId id, string layerName, int colorIndex)
        {
            AEE_Utility.CreateLayer(layerName, colorIndex);

            ObjectId colonRegionId = new ObjectId();

            DBObjectCollection acDBObjColl = new DBObjectCollection();

            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            try
            {
                using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
                {
                    // Open the Block table for read              
                    BlockTable acBlkTbl = tr.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                    // Open the Block table record Model space for write            
                    BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    Entity acEnt = tr.GetObject(id, OpenMode.ForWrite) as Entity;
                    acEnt.Layer = layerName;
                    acEnt.ColorIndex = 256;
                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
            return colonRegionId;
        }

        public static ObjectId createColonEntityInSamePoint(ObjectId objId, bool visible)
        {
            ObjectId colonRegionId = new ObjectId();
            // Add all the objects to clone
            DBObjectCollection acDBObjColl = new DBObjectCollection();

            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            try
            {
                using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
                {
                    // Open the Block table for read              
                    BlockTable acBlkTbl = tr.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                    // Open the Block table record Model space for write            
                    BlockTableRecord acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    Entity ent = tr.GetObject(objId, OpenMode.ForWrite) as Entity;
                    acDBObjColl.Add(ent);

                    foreach (Entity acEnt in acDBObjColl)
                    {
                        Entity acEntClone = acEnt.Clone() as Entity;
                        Vector3d pastePoint = new Vector3d(0, 0, 0);
                        // Create a matrix and move each copied entity in selection point
                        acEntClone.TransformBy(Matrix3d.Displacement(new Vector3d(pastePoint.X, pastePoint.Y, pastePoint.Z)));
                        acEntClone.Visible = visible;// visible; Testing-123
                        //// Add the cloned object
                        colonRegionId = acBlkTblRec.AppendEntity(acEntClone);
                        tr.AddNewlyCreatedDBObject(acEntClone, true);
                    }
                    ed.UpdateScreen();
                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
            return colonRegionId;
        }

        public static ObjectId OffsetLine(Entity ent, double offsetDistance, string layerName, int colorIndex)
        {
            AEE_Utility.CreateLayer(layerName, colorIndex);

            ObjectId id = new ObjectId();
            // Get the current document and database
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                if (ent is Line)
                {
                    Line acLine = ent as Line;
                    // Offset the line a given distance
                    DBObjectCollection acDbObjColl = acLine.GetOffsetCurves(offsetDistance);

                    // Step through the new objects created
                    foreach (Entity acEnt in acDbObjColl)
                    {
                        // Add each offset object
                        acEnt.Layer = layerName;
                        acEnt.ColorIndex = 256;
                        id = acBlkTblRec.AppendEntity(acEnt);
                        acTrans.AddNewlyCreatedDBObject(acEnt, true);
                    }
                    ed.UpdateScreen();
                }
                // Save the new objects to the database
                acTrans.Commit();
            }
            return id;
        }

        public static ObjectId OffsetLine(ObjectId lineId, double offsetDistance, bool visible)
        {
            ObjectId id = new ObjectId();
            // Get the current document and database
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                Entity ent = acTrans.GetObject(lineId, OpenMode.ForWrite) as Entity;

                if (ent is Line)
                {
                    Line acLine = ent as Line;
                    // Offset the line a given distance
                    DBObjectCollection acDbObjColl = acLine.GetOffsetCurves(offsetDistance);

                    // Step through the new objects created
                    foreach (Entity acEnt in acDbObjColl)
                    {
                        acEnt.Visible = visible;// visible; Testing-123
                        // Add each offset object                       
                        id = acBlkTblRec.AppendEntity(acEnt);
                        acTrans.AddNewlyCreatedDBObject(acEnt, true);
                    }
                    ed.UpdateScreen();
                }
                // Save the new objects to the database
                acTrans.Commit();
            }
            return id;
        }

        public static ObjectId OffsetLine(Entity samePropertyEnt, ObjectId lineId, double offsetDistance, bool visible)
        {
            ObjectId id = new ObjectId();
            // Get the current document and database
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                Entity ent = acTrans.GetObject(lineId, OpenMode.ForWrite) as Entity;

                if (ent is Line)
                {
                    Line acLine = ent as Line;
                    // Offset the line a given distance
                    DBObjectCollection acDbObjColl = acLine.GetOffsetCurves(offsetDistance);

                    // Step through the new objects created
                    foreach (Entity acEnt in acDbObjColl)
                    {
                        acEnt.SetPropertiesFrom(samePropertyEnt);
                        acEnt.Visible = visible;
                        // Add each offset object                       
                        id = acBlkTblRec.AppendEntity(acEnt);
                        acTrans.AddNewlyCreatedDBObject(acEnt, true);
                    }
                    ed.UpdateScreen();
                }
                // Save the new objects to the database
                acTrans.Commit();
            }
            return id;
        }

        public static Entity OffsetLineEntity(Entity ent, double offsetDistance)
        {
            Line acLine = ent as Line;
            // Offset the polyline a given distance
            DBObjectCollection acDbObjColl = acLine.GetOffsetCurves(offsetDistance);
            Entity entOfLine = acDbObjColl[0] as Entity;

            return entOfLine;
        }

        public static double GetAreaOfHatch(ObjectId hatchId)
        {
            double Area = 0;
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            try
            {
                using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
                {
                    Entity ent = tr.GetObject(hatchId, OpenMode.ForRead) as Entity;
                    if (ent is Hatch)
                    {
                        Hatch acHatch = ent as Hatch;
                        Area = acHatch.Area;
                    }
                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }

            return Area;
        }

        public static ObjectId CreateArcWithPolyline(Point3d centerPoint, Point3d startAnglePoint, Point3d endAnglePoint, double radius, string layerName, int colorIndex, string lineType, double lineTypeScale)
        {
            ObjectId objId = new ObjectId();
            AEE_Utility.CreateLayer(layerName, colorIndex);

            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                // Open the Linetype table for read
                LinetypeTable acLineTypTbl = tr.GetObject(db.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;

                if (acLineTypTbl.Has(lineType) == false)
                {
                    db.LoadLineTypeFile(lineType, "acad.lin");
                }

                BlockTable bt = (BlockTable)db.BlockTableId.GetObject(OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);

                Line acLine = new Line(new Point3d(centerPoint.X, centerPoint.Y, 0), new Point3d(startAnglePoint.X, startAnglePoint.Y, 0));
                double startAngle = acLine.Angle;

                acLine = new Line(new Point3d(centerPoint.X, centerPoint.Y, 0), new Point3d(endAnglePoint.X, endAnglePoint.Y, 0));
                double endAngle = acLine.Angle;
                double deltaAng = endAngle - startAngle;

                if (deltaAng < 0)
                {
                    deltaAng += 2 * Math.PI;
                }

                double bulge = Math.Tan(deltaAng * 0.25);

                Polyline poly = new Polyline();
                poly.AddVertexAt(0, new Point2d(startAnglePoint.X, startAnglePoint.Y), bulge, 0, 0);
                poly.AddVertexAt(1, new Point2d(endAnglePoint.X, endAnglePoint.Y), 0, 0, 0);
                poly.Linetype = lineType;
                poly.LinetypeScale = lineTypeScale;
                poly.ColorIndex = 256;
                poly.Layer = layerName;
                objId = btr.AppendEntity(poly);
                tr.AddNewlyCreatedDBObject(poly, true);
                ed.UpdateScreen();
                tr.Commit();
            }

            return objId;
        }

        public static List<string> GetAllLayerName()
        {
            List<string> listOfAllLayer = new List<string>();
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            using (Transaction acTrans = db.TransactionManager.StartTransaction())
            {
                LayerTable lyrTbl = acTrans.GetObject(db.LayerTableId, OpenMode.ForNotify) as LayerTable;
                foreach (ObjectId layerId in lyrTbl)
                {
                    LayerTableRecord lyrTblRec = acTrans.GetObject(layerId, OpenMode.ForWrite) as LayerTableRecord;
                    string layerName = lyrTblRec.Name;
                    listOfAllLayer.Add(layerName);
                }
            }

            return listOfAllLayer;
        }

        public static void LockUnLockLayer(string sLayerName, bool lockValue)
        {
            //// Get the current document and database
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            //// Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                //// Open the Layer table for read
                LayerTable acLyrTbl = acTrans.GetObject(acCurDb.LayerTableId, OpenMode.ForRead) as LayerTable;

                if (acLyrTbl.Has(sLayerName) == true)
                {
                    LayerTableRecord acLyrTblRec = acTrans.GetObject(acLyrTbl[sLayerName], OpenMode.ForWrite) as LayerTableRecord;
                    // Turn the layer off
                    acLyrTblRec.IsLocked = lockValue;
                }

                //// Save the changes and dispose of the transaction
                acTrans.Commit();
            }
        }

        public static ObjectId CreatePolyLine(Polyline polyline, double basePoint_X, double basePoint_Y, string layerName, int colorIndex)
        {
            AEE_Utility.CreateLayer(layerName, colorIndex);

            ObjectId polyLineId;
            var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var ed = doc.Editor;

            // start a transaction
            using (var tr = db.TransactionManager.StartTransaction())
            {
                // open the current space for write
                var curSpace = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                // create a new Polyline
                using (var pline = polyline)
                {
                    Point3d basePoint = new Point3d(basePoint_X, basePoint_Y, 0);
                    // transform the polyline about current coordinate system and specified instertion point
                    pline.TransformBy(ed.CurrentUserCoordinateSystem * Matrix3d.Displacement(basePoint.GetAsVector()));
                    pline.Layer = layerName;
                    pline.ColorIndex = 256;
                    // append the polyline to the current space
                    polyLineId = curSpace.AppendEntity(pline);
                    // add the polyline to the transaction
                    tr.AddNewlyCreatedDBObject(pline, true);
                    ed.UpdateScreen();
                }
                // save the changes to the database
                tr.Commit();
            } // dispose of the transaction

            return polyLineId;
        }

        public static List<ObjectId> GetEntitiesOnLayer(string layerName)
        {
            List<ObjectId> listOfObjIds = new List<ObjectId>();
            try
            {
                Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                Editor ed = doc.Editor;

                //// Build a filter list so that only entities on the specified layer are selected

                TypedValue[] tvs = new TypedValue[1] { new TypedValue((int)DxfCode.LayerName, layerName) };
                SelectionFilter sf = new SelectionFilter(tvs);
                PromptSelectionResult psr = ed.SelectAll(sf);
                if (psr.Status != PromptStatus.OK)
                {
                    return listOfObjIds;
                }

                SelectionSet selSet = psr.Value;
                ObjectId[] ids = selSet.GetObjectIds();

                for (int i = 0; i < ids.Length; i++)
                {
                    listOfObjIds.Add(ids[i]);
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
            return listOfObjIds;
        }
        public static void ArrangeStartEndPointOfLine(ObjectId objId)
        {
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            try
            {
                using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
                {
                    Entity ent = tr.GetObject(objId, OpenMode.ForWrite) as Entity;
                    if (ent is Line)
                    {
                        Line line = ent as Line;

                        Point3d startPoint = new Point3d(line.StartPoint.X, line.StartPoint.Y, 0);
                        Point3d endPoint = new Point3d(line.EndPoint.X, line.EndPoint.Y, 0);

                        Point3d strtPnt = new Point3d();
                        Point3d endPnt = new Point3d();
                        double minX = Math.Min(startPoint.X, endPoint.X);
                        if (minX == startPoint.X)
                        {
                            strtPnt = startPoint;
                            endPnt = endPoint;
                        }
                        else
                        {
                            strtPnt = endPoint;
                            endPnt = startPoint;
                        }

                        line.StartPoint = strtPnt;
                        line.EndPoint = endPnt;
                    }
                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
        }
        public static List<CircularArc2d> GetArcFromPolyLine(ObjectId polyLineId)
        {
            List<CircularArc2d> listOfCircularArc2d = new List<CircularArc2d>();

            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            using (Transaction acTrans = db.TransactionManager.StartTransaction())
            {
                Entity ent = acTrans.GetObject(polyLineId, OpenMode.ForRead) as Entity;
                if (ent is Polyline)
                {
                    Polyline acPoly = ent as Polyline;
                    for (int i = 0; i < acPoly.NumberOfVertices; i++)
                    {
                        Curve3d seg = null;

                        SegmentType segType = acPoly.GetSegmentType(i);

                        if (segType == SegmentType.Arc)
                        {
                            seg = acPoly.GetArcSegmentAt(i);
                            var acArc = acPoly.GetArcSegment2dAt(i);
                            listOfCircularArc2d.Add(acArc);
                        }
                    }
                }
            }
            return listOfCircularArc2d;
        }
        public static List<LineSegment2d> GetSegmentOfLineFromPolyline(ObjectId polyLineId)
        {
            List<LineSegment2d> listOfLineSegment2d = new List<LineSegment2d>();

            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            using (Transaction acTrans = db.TransactionManager.StartTransaction())
            {
                Entity ent = acTrans.GetObject(polyLineId, OpenMode.ForRead) as Entity;
                if (ent is Polyline)
                {
                    Polyline acPoly = ent as Polyline;
                    for (int i = 0; i < acPoly.NumberOfVertices; i++)
                    {
                        Curve3d seg = null;

                        SegmentType segType = acPoly.GetSegmentType(i);

                        if (segType == SegmentType.Line)
                        {
                            seg = acPoly.GetLineSegmentAt(i);
                            var acLine = acPoly.GetLineSegment2dAt(i);
                            listOfLineSegment2d.Add(acLine);
                        }
                    }
                }
            }
            return listOfLineSegment2d;
        }

        public static List<LineSegment3d> GetSegmentsFromPolyline(ObjectId polyLineId)
        {
            List<LineSegment3d> listOfLineSegment3d = new List<LineSegment3d>();

            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            using (Transaction acTrans = db.TransactionManager.StartTransaction())
            {
                Entity ent = acTrans.GetObject(polyLineId, OpenMode.ForRead) as Entity;
                if (ent is Polyline)
                {
                    Polyline acPoly = ent as Polyline;
                    for (int i = 0; i < acPoly.NumberOfVertices; i++)
                    {
                        Curve3d seg = null;

                        SegmentType segType = acPoly.GetSegmentType(i);

                        if (segType == SegmentType.Line)
                        {
                            seg = acPoly.GetLineSegmentAt(i);
                            var acLine = acPoly.GetLineSegment2dAt(i);
                            listOfLineSegment3d.Add(new LineSegment3d(new Point3d(acLine.StartPoint.X, acLine.StartPoint.Y, 0),
                                    new Point3d(acLine.EndPoint.X, acLine.EndPoint.Y, 0)));
                        }
                    }
                }
            }
            return listOfLineSegment3d;
        }

        public static ObjectId ConvertToPolyline(ObjectId objId)
        {
            ObjectId polyLineId = new ObjectId();
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;
            try
            {
                using (Transaction acTrans = db.TransactionManager.StartTransaction())
                {
                    Curve curve = acTrans.GetObject(objId, OpenMode.ForRead) as Curve;
                    if (!(curve is Line || curve is Arc))
                    {
                        return polyLineId;
                    }
                    BlockTable blkTb = acTrans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord blkTblRec = acTrans.GetObject(blkTb[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                    using (Polyline acPoly = new Polyline())
                    {
                        Entity ent = acTrans.GetObject(objId, OpenMode.ForRead) as Entity;
                        acPoly.SetPropertiesFrom(ent);
                        acPoly.TransformBy(curve.Ecs);
                        Point3d start = curve.StartPoint.TransformBy(acPoly.Ecs.Inverse());
                        acPoly.AddVertexAt(0, new Point2d(start.X, start.Y), 0, 0, 0);
                        acPoly.JoinEntity(curve);
                        polyLineId = blkTblRec.AppendEntity(acPoly);
                        acTrans.AddNewlyCreatedDBObject(acPoly, true);
                        AEE_Utility.deleteEntity(objId);
                    }
                    acTrans.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
            return polyLineId;
        }
        public static void JoinPolylines(ObjectId sourcePolyId, List<ObjectId> listOfJoinPolyIds)
        {
            Document document = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = document.Editor;
            Database db = document.Database;
            try
            {
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    foreach (ObjectId joinPolyId in listOfJoinPolyIds)
                    {
                        JoinPolylines(sourcePolyId, joinPolyId);
                    }
                    ed.UpdateScreen();
                    trans.Commit();
                }
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage(ex.Message);
            }
        }
        public static void JoinPolylines(ObjectId sourcePolyId, ObjectId joinPolyId)
        {
            Document document = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = document.Editor;
            Database db = document.Database;
            try
            {
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    Entity srcPLine = trans.GetObject(sourcePolyId, OpenMode.ForRead) as Entity;

                    Entity addPLine = trans.GetObject(joinPolyId, OpenMode.ForRead) as Entity;

                    srcPLine.UpgradeOpen();
                    srcPLine.JoinEntity(addPLine);
                    addPLine.UpgradeOpen();
                    addPLine.Erase();
                    ed.UpdateScreen();
                    trans.Commit();
                }
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage(ex.Message);
            }
        }

        public static Point3d GetBasePointOfPolyline(ObjectId polylineId)
        {
            // Get the current document and database
            Point3d basePoint = new Point3d();
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;

            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                Entity ent = acTrans.GetObject(polylineId, OpenMode.ForRead) as Entity;

                if (ent is Polyline)
                {
                    Polyline acPoly = ent as Polyline;
                    basePoint = acPoly.StartPoint;
                }
                // Save the new object to the database
                acTrans.Commit();
            }

            return basePoint;
        }

        public static bool GetPointIsInsideThePolyline(ObjectId polylineId, Point3d checkPoint)
        {
            try
            {
                Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                Database db = doc.Database;
                Editor ed = doc.Editor;

                using (Transaction tr = db.TransactionManager.StartOpenCloseTransaction())
                {
                    Polyline pline = (Polyline)tr.GetObject(polylineId, OpenMode.ForRead);

                    double tolerance =Tolerance.Global.EqualPoint;
                    using (MPolygon mpg = new MPolygon())
                    {
                        mpg.AppendLoopFromBoundary(pline, true, tolerance);
                        return mpg.IsPointInsideMPolygon(checkPoint, tolerance).Count == 1;
                    }
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                return false;
            }

        }

        public static void ModifyDimensionStyle()
        {
            try
            {
                Database db = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Database;
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    // Open the DimStyle table for read
                    DimStyleTable DimTabb = trans.GetObject(db.DimStyleTableId, OpenMode.ForRead) as DimStyleTable;
                    ObjectId dimId = db.Dimstyle;
                    DimStyleTableRecord DimTabRecord = (DimStyleTableRecord)trans.GetObject(dimId, OpenMode.ForWrite);
                    var dimStyleName = DimTabRecord.Name;

                    var dimtmove_Value = DimTabRecord.Dimtmove;
                    if (dimtmove_Value != 0)
                    {
                        DimTabRecord.Dimtmove = 0;
                    }
                    DimTabRecord.UpgradeOpen();
                    trans.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
        }
        public static ObjectId Fillet_BetweenTwoLines(ObjectId lineId1, ObjectId lineId2, double radius)
        {
            ObjectId id = new ObjectId();

            var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var ed = doc.Editor;

            try
            {
                var midPoint1 = AEE_Utility.GetMidPoint(lineId1);
                //var pp1 = ed.Snap("_near", midPoint1).TransformBy(ed.CurrentUserCoordinateSystem);
                var pp1 = AEE_Utility.GetMidPoint(lineId1);

                var midPoint2 = AEE_Utility.GetMidPoint(lineId1);
                //var pp2 = ed.Snap("_near", midPoint2).TransformBy(ed.CurrentUserCoordinateSystem);
                var pp2 = AEE_Utility.GetMidPoint(lineId1);
                using (var tr = db.TransactionManager.StartTransaction())
                {
                    // open the lines
                    var line1 = (Line)tr.GetObject(lineId1, OpenMode.ForRead);
                    var line2 = (Line)tr.GetObject(lineId2, OpenMode.ForRead);

                    // get the intersection
                    var pts = new Point3dCollection();
                    line1.IntersectWith(line2, Intersect.ExtendBoth, pts, IntPtr.Zero, IntPtr.Zero);
                    if (pts.Count != 1)
                    {
                        ed.WriteMessage("\nSelected lines do not intersect.");
                        return id;
                    }
                    var inters = pts[0];

                    var sp1 = line1.StartPoint;
                    var ep1 = line1.EndPoint;
                    var sp2 = line2.StartPoint;
                    var ep2 = line2.EndPoint;

                    // get the farest points from intersection on the picked side of both lines
                    Func<Point3d, Point3d, Point3d, Point3d> getFarest = (sp, ep, pp) =>
                    {
                        var dir = inters.GetVectorTo(pp);
                        if (!inters.GetVectorTo(sp).IsCodirectionalTo(dir)) return ep;
                        if (!inters.GetVectorTo(ep).IsCodirectionalTo(dir)) return sp;
                        if (inters.DistanceTo(sp) < inters.DistanceTo(ep)) return ep;
                        return sp;
                    };
                    var fp1 = getFarest(sp1, ep1, pp1);
                    var fp2 = getFarest(sp2, ep2, pp2);

                    // if radius == 0, just trim/extend the lines
                    if (radius == 0.0)
                    {
                        line1.UpgradeOpen();
                        if (sp1.IsEqualTo(line1.StartPoint))
                            line1.EndPoint = inters;
                        else
                            line1.StartPoint = inters;

                        line2.UpgradeOpen();
                        if (sp2.IsEqualTo(line2.StartPoint))
                            line2.EndPoint = inters;
                        else
                            line2.StartPoint = inters;
                    }

                    // compute the fillet
                    else
                    {
                        // 2D work in the plane defined by the two lines
                        var normal = (fp1 - inters).CrossProduct(fp2 - inters);
                        var plane = new Plane(inters, normal);
                        var v1 = fp1.Convert2d(plane).GetAsVector();
                        var v2 = fp2.Convert2d(plane).GetAsVector();
                        double angle = v1.GetAngleTo(v2) / 2.0;
                        var dist = radius / Math.Tan(angle);
                        if (v1.Length <= dist || v2.Length <= dist)
                        {
                            ed.WriteMessage("\nRadius too large to fillet the selected lines.");
                            return id;
                        }

                        double hyp = radius / Math.Sin(angle);
                        var center = new Point2d(hyp * Math.Cos(angle + v1.Angle), hyp * Math.Sin(angle + v1.Angle));
                        var p1 = Point2d.Origin + v1.GetNormal() * dist;
                        var p2 = Point2d.Origin + v2.GetNormal() * dist;
                        var a1 = center.GetVectorTo(p1).Angle;
                        var a2 = center.GetVectorTo(p2).Angle;

                        // back to 3D
                        Func<Point2d, Point3d> convert3d = (pt) =>
                            new Point3d(pt.X, pt.Y, 0.0).TransformBy(Matrix3d.PlaneToWorld(plane));
                        var arc = new Arc(new Point3d(center.X, center.Y, 0.0), radius, a2, a1);

                        arc.Layer = line1.Layer;
                        arc.ColorIndex = 256;
                        arc.Linetype = line1.Linetype;
                        arc.LinetypeScale = line1.LinetypeScale;

                        arc.TransformBy(Matrix3d.PlaneToWorld(plane));
                        var curSpace = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                        id = curSpace.AppendEntity(arc);
                        tr.AddNewlyCreatedDBObject(arc, true);

                        line1.UpgradeOpen();
                        line1.StartPoint = convert3d(p1);
                        line1.EndPoint = fp1;
                        line2.UpgradeOpen();
                        line2.StartPoint = fp2;
                        line2.EndPoint = convert3d(p2);
                    }
                    ed.UpdateScreen();
                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
            return id;
        }
        public static List<Point3d> GetIntrsctPntOfBtwnTwoLines_WithExtendBoth(ObjectId lineId1, ObjectId lineId2)
        {
            List<Point3d> listOfIntersctionPoint = new List<Point3d>();

            var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var ed = doc.Editor;
            try
            {
                using (var tr = db.TransactionManager.StartTransaction())
                {
                    // open the lines
                    var line1 = (Line)tr.GetObject(lineId1, OpenMode.ForRead);
                    var line2 = (Line)tr.GetObject(lineId2, OpenMode.ForRead);

                    // get the intersection
                    var pts = new Point3dCollection();
                    line1.IntersectWith(line2, Intersect.ExtendBoth, pts, IntPtr.Zero, IntPtr.Zero);
                    if (pts.Count != 1)
                    {
                        //ed.WriteMessage("\nSelected lines do not intersect.");
                        return listOfIntersctionPoint;
                    }
                    foreach (Point3d insctPoint in pts)
                    {
                        listOfIntersctionPoint.Add(insctPoint);
                    }
                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
            return listOfIntersctionPoint;
        }
        public static List<Point3d> GetIntrsctPntOfBtwnTwoLines_WithExtendArgument(ObjectId lineId1This, ObjectId lineId2Argument)
        {
            List<Point3d> listOfIntersctionPoint = new List<Point3d>();

            var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var ed = doc.Editor;
            try
            {
                using (var tr = db.TransactionManager.StartTransaction())
                {
                    // open the lines
                    var line1 = (Line)tr.GetObject(lineId1This, OpenMode.ForRead);
                    var line2 = (Line)tr.GetObject(lineId2Argument, OpenMode.ForRead);

                    // get the intersection
                    var pts = new Point3dCollection();
                    line1.IntersectWith(line2, Intersect.ExtendArgument, pts, IntPtr.Zero, IntPtr.Zero);
                    if (pts.Count != 1)
                    {
                        //ed.WriteMessage("\nSelected lines do not intersect.");
                        return listOfIntersctionPoint;
                    }
                    foreach (Point3d insctPoint in pts)
                    {
                        listOfIntersctionPoint.Add(insctPoint);
                    }
                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
            return listOfIntersctionPoint;
        }
        public static List<double> GetParallelLinePoints_In_UpperDir(double x1, double y1, double x2, double y2, double offsetValue)
        {
            List<double> listOfPoint = new List<double>();
            double L = Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));

            double offsetPixels = -offsetValue;

            // This is the second line
            double x1p = x1 + offsetPixels * (y2 - y1) / L;
            double x2p = x2 + offsetPixels * (y2 - y1) / L;
            double y1p = y1 + offsetPixels * (x1 - x2) / L;
            double y2p = y2 + offsetPixels * (x1 - x2) / L;

            double line_Max_Y = Math.Max(y1, y2);
            double offSet_Max_Y = Math.Max(y1p, y2p);
            if (line_Max_Y > offSet_Max_Y)
            {
                offsetPixels = offsetValue;

                // This is the second line
                x1p = x1 + offsetPixels * (y2 - y1) / L;
                x2p = x2 + offsetPixels * (y2 - y1) / L;
                y1p = y1 + offsetPixels * (x1 - x2) / L;
                y2p = y2 + offsetPixels * (x1 - x2) / L;
            }

            listOfPoint.Add(x1p);
            listOfPoint.Add(y1p);
            listOfPoint.Add(x2p);
            listOfPoint.Add(y2p);

            return listOfPoint;
        }
        public static List<double> GetParallelLinePoints_In_DownDir(double x1, double y1, double x2, double y2, double offsetValue)
        {
            List<double> listOfPoint = new List<double>();

            double L = Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));

            double offsetPixels = offsetValue;

            // This is the second line
            double x1p = x1 + offsetPixels * (y2 - y1) / L;
            double x2p = x2 + offsetPixels * (y2 - y1) / L;
            double y1p = y1 + offsetPixels * (x1 - x2) / L;
            double y2p = y2 + offsetPixels * (x1 - x2) / L;

            double line_Max_Y = Math.Max(y1, y2);
            double offSet_Max_Y = Math.Max(y1p, y2p);
            if (offSet_Max_Y > line_Max_Y)
            {
                offsetPixels = -offsetValue;

                // This is the second line
                x1p = x1 + offsetPixels * (y2 - y1) / L;
                x2p = x2 + offsetPixels * (y2 - y1) / L;
                y1p = y1 + offsetPixels * (x1 - x2) / L;
                y2p = y2 + offsetPixels * (x1 - x2) / L;
            }

            listOfPoint.Add(x1p);
            listOfPoint.Add(y1p);
            listOfPoint.Add(x2p);
            listOfPoint.Add(y2p);

            return listOfPoint;
        }

        public static List<ObjectId> TrimBetweenTwoClosedPolylines(ObjectId boundary_Id, ObjectId trim_Id)
        {
            /// This code requires a reference to AcExportLayoutEx.dll:
            //using Autodesk.AutoCAD.ExportLayout;

            List<ObjectId> listOfObjId = new List<ObjectId>();

            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;
            try
            {
                using (Transaction tr = doc.TransactionManager.StartTransaction())
                {
                    var boundary = AEE_Utility.GetEntityForWrite(boundary_Id);
                    var entityToTrim = AEE_Utility.GetEntityForWrite(trim_Id);

                    using (Trimmer trimmer = new Autodesk.AutoCAD.ExportLayout.Trimmer())
                    {
                        var btr = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);

                        trimmer.Trim(entityToTrim, boundary);
                        if (trimmer.HasAccurateResults)
                        {
                            foreach (Entity ent in trimmer.TrimResultObjects)
                            {
                                ent.SetPropertiesFrom(entityToTrim);
                                var id = btr.AppendEntity(ent);
                                tr.AddNewlyCreatedDBObject(ent, true);
                                listOfObjId.Add(id);
                            }

                            //if (trimmer.EntityCompletelyOutside || trimmer.EntityOnBoundary)
                            //{
                            //    entityToTrim.Erase();
                            //}
                        }
                    }

                    tr.Commit();
                }
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage("\nOperation failed ({0})", ex.Message);
            }
            return listOfObjId;
        }
        public static ObjectId CreateAlignedDimWithTextWithStyle(string newDimensionStyleName, string dimensionText, ObjectId lineId, Point3d dimTextPoint, double textHeight, string layerName, int colorIndex)
        {
            ObjectId id = new ObjectId();
            CreateLayer(layerName, colorIndex);
            // Get the current database
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                var dimId = AEE_Utility.CreateNewDimensionStyle(newDimensionStyleName);
                acCurDb.Dimstyle = dimId;

                var listOfDim = getStartEndPointWithAngle_Line(lineId);
                Point3d startPoint = new Point3d(listOfDim[0], listOfDim[1], 0);
                Point3d endPoint = new Point3d(listOfDim[2], listOfDim[3], 0);

                var acLinAngDim = new AlignedDimension(startPoint, endPoint, dimTextPoint, null, acCurDb.Dimstyle);
                acLinAngDim.Dimtxt = textHeight;
                acLinAngDim.DimensionText = dimensionText;
                acLinAngDim.Layer = layerName;
                acLinAngDim.ColorIndex = 256;

                // Add the new object to Model space and the transaction
                id = acBlkTblRec.AppendEntity(acLinAngDim);
                acTrans.AddNewlyCreatedDBObject(acLinAngDim, true);
                ed.UpdateScreen();

                // Commit the changes and dispose of the transaction
                acTrans.Commit();
            }
            return id;
        }
        public static ObjectId CreateNewDimensionStyle(string dimStyleName)
        {
            ObjectId dimId = ObjectId.Null;

            Database db = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Database;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                DimStyleTable DimTabb = (DimStyleTable)trans.GetObject(db.DimStyleTableId, OpenMode.ForRead);

                if (!DimTabb.Has(dimStyleName))
                {
                    DimTabb.UpgradeOpen();

                    DimStyleTableRecord dstr = new DimStyleTableRecord();

                    // Set all the available dimension style properties                   
                    dstr.Dimadec = 0;
                    dstr.Dimdli = 0;
                    dstr.Dimalt = false;
                    dstr.Dimaltd = 0;

                    dstr.Dimaltf = 25.4;
                    dstr.Dimaltrnd = 0;

                    dstr.Dimalttz = 0;
                    dstr.Dimaltu = 2;
                    dstr.Dimaltz = 0;
                    dstr.Dimapost = "";

                    dstr.Dimatfit = 0;
                    dstr.Dimaunit = 0;

                    dstr.Dimcen = 0;
                    dstr.Dimdec = 0;
                    dstr.Dimdle = 0;

                    dstr.Dimfrac = 0;

                    dstr.DimfxlenOn = false;

                    dstr.Dimjust = 0;
                    dstr.Dimldrblk = ObjectId.Null;

                    dstr.Dimlim = false;

                    dstr.Dimpost = "";
                    dstr.Dimrnd = 0;
                    dstr.Dimsah = false;
                    dstr.Dimscale = 1;
                    dstr.Dimsd1 = true;
                    dstr.Dimsd2 = true;
                    dstr.Dimse1 = true;
                    dstr.Dimse2 = true;
                    dstr.Dimsoxd = false;

                    dstr.Dimtdec = 0;

                    dstr.Dimtfill = 0;
                    dstr.Dimtih = false;
                    dstr.Dimtix = false;
                    dstr.Dimtm = 0;
                    dstr.Dimtmove = 0;
                    dstr.Dimtofl = false;
                    dstr.Dimtoh = false;
                    dstr.Dimtol = false;

                    dstr.Dimtp = 0;
                    dstr.Dimtsz = 0;
                    dstr.Dimtvp = 0;

                    dstr.Dimtxtdirection = false;
                    dstr.Dimtzin = 0;
                    dstr.Dimupt = false;
                    dstr.Dimzin = 0;

                    dstr.Name = dimStyleName;
                    dimId = DimTabb.Add(dstr);
                    trans.AddNewlyCreatedDBObject(dstr, true);
                }
                else
                {
                    dimId = DimTabb[dimStyleName];
                }
                trans.Commit();
            }
            return dimId;
        }

        public static bool checkBlockExist(string blkName)
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            Transaction tr = db.TransactionManager.StartTransaction();
            using (tr)
            {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);  // Get the block table from the drawing         

                BlockTableRecord btr = new BlockTableRecord(); // Create our new block table record...
                if (!bt.Has(blkName))
                {
                    return false;
                }
            }
            return true;
        }
        public static ObjectId createBlock(string blkName, DBObjectCollection colEntity, string blockLayer, int blockLayerColorIndex)
        {
            AEE_Utility.CreateLayer(blockLayer, blockLayerColorIndex);
            ObjectId id = new ObjectId();
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            Transaction tr = db.TransactionManager.StartTransaction();
            using (tr)
            {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);  // Get the block table from the drawing         

                BlockTableRecord btr = new BlockTableRecord(); // Create our new block table record...
                if (!bt.Has(blkName))
                {
                    btr.Name = blkName; // ... and set its properties

                    bt.UpgradeOpen(); // Add the new block to the block table

                    ObjectId btrId = bt.Add(btr);
                    tr.AddNewlyCreatedDBObject(btr, true);

                    foreach (Entity ent in colEntity)
                    {
                        btr.AppendEntity(ent);
                        tr.AddNewlyCreatedDBObject(ent, true);
                    }
                    //// Add a block reference to the model space
                    BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                    BlockReference br = new BlockReference(Point3d.Origin, btrId);
                    br.Visible = true;
                    br.Layer = blockLayer;
                    br.ColorIndex = 256;
                    id = ms.AppendEntity(br);
                    tr.AddNewlyCreatedDBObject(br, true);

                    ed.UpdateScreen();
                    // Commit the transaction
                    tr.Commit();
                }
            }
            return id;
        }
        public static List<string> GetAllBlockNameName()
        {
            List<string> listOfAllBlockName = new List<string>();
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            using (Transaction acTrans = db.TransactionManager.StartTransaction())
            {
                BlockTable blkTable = (BlockTable)acTrans.GetObject(db.BlockTableId, OpenMode.ForRead);  // Get the block table from the drawing         
                foreach (ObjectId blockId in blkTable)
                {
                    BlockTableRecord blkTblRec = acTrans.GetObject(blockId, OpenMode.ForWrite) as BlockTableRecord;
                    string blockName = blkTblRec.Name;
                    listOfAllBlockName.Add(blockName);
                }
            }
            return listOfAllBlockName;
        }


        public static void AttachXData(ObjectId id, string XDataRegisterAppName, string xDataAsciiName)
        {
            if (string.IsNullOrEmpty(xDataAsciiName) || string.IsNullOrWhiteSpace(xDataAsciiName) || string.IsNullOrEmpty(XDataRegisterAppName) || string.IsNullOrWhiteSpace(XDataRegisterAppName) ||
                CheckXDataRegisterAppName(id, XDataRegisterAppName))
            {
                return;
            }
            try
            {
                // Get the current database and start a transaction
                Database acCurDb = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Database;

                Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;

                using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
                {
                    // Open the Registered Applications table for read
                    RegAppTable acRegAppTbl = acTrans.GetObject(acCurDb.RegAppTableId, OpenMode.ForRead) as RegAppTable;

                    // Check to see if the Registered Applications table record for the custom app exists
                    if (acRegAppTbl.Has(XDataRegisterAppName) == false)
                    {
                        using (RegAppTableRecord acRegAppTblRec = new RegAppTableRecord())
                        {
                            acRegAppTblRec.Name = XDataRegisterAppName;

                            acRegAppTbl.UpgradeOpen();
                            acRegAppTbl.Add(acRegAppTblRec);
                            acTrans.AddNewlyCreatedDBObject(acRegAppTblRec, true);
                        }
                    }
                    // Define the Xdata to add to each selected object
                    using (ResultBuffer rb = new ResultBuffer())
                    {
                        rb.Add(new TypedValue((int)DxfCode.ExtendedDataRegAppName, XDataRegisterAppName));
                        rb.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString, xDataAsciiName));
                        //foreach (var id in listOfXData)
                        {
                            // Open the selected object for write
                            Entity acEnt = acTrans.GetObject(id, OpenMode.ForWrite) as Entity;
                            // Append the extended data to each object
                            acEnt.XData = rb;
                        }
                    }

                    // Save the new object to the database
                    acTrans.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
        }
        public static void RemoveXDataName(string XDataName) // This method can have any name
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Transaction tr = db.TransactionManager.StartTransaction();
            using (tr)
            {
                Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
                try
                {
                    List<ObjectId> listOfXDataIds = GetObjectIdFromXDataName(XDataName);
                    foreach (var id in listOfXDataIds)
                    {
                        Entity ent = (Entity)tr.GetObject(id, OpenMode.ForRead);
                        string xDataRegisterAppName = "";
                        ResultBuffer buffer = GetResultBufferOfXDataName(id, XDataName, out xDataRegisterAppName);
                        // This call would ensure that the
                        //Xdata of the entity associated with application name only would be removed                       
                        if (buffer != null)
                        {
                            ent.UpgradeOpen();
                            ent.XData = new ResultBuffer(new TypedValue((int)DxfCode.ExtendedDataRegAppName, xDataRegisterAppName));
                            buffer.Dispose();
                        }
                    }
                    tr.Commit();
                }
                catch
                {
                    tr.Abort();
                }
            }
        }

        public static List<ObjectId> GetObjectIdFromXDataName(string xDataName)
        {
            List<ObjectId> listOfXDataIds = new List<ObjectId>();
            try
            {
                Autodesk.AutoCAD.ApplicationServices.Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;

                if (doc == null)
                    return listOfXDataIds;

                Editor ed = doc.Editor;

                Database db = HostApplicationServices.WorkingDatabase;

                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    RegAppTable rat = (RegAppTable)tr.GetObject(db.RegAppTableId, OpenMode.ForRead, false);
                    RegAppTableRecord ratr = new RegAppTableRecord();
                    //if (rat.Has(xDataAppName))
                    {
                        TypedValue[] filterlist = new TypedValue[1];
                        filterlist.SetValue(new TypedValue((int)DxfCode.ExtendedDataAsciiString, xDataName), 0);

                        SelectionFilter filter = new SelectionFilter(filterlist);

                        PromptSelectionResult res = ed.SelectAll(filter);

                        if (res.Status != PromptStatus.OK)
                        {
                            return listOfXDataIds;
                        }

                        SelectionSet selSet = res.Value;

                        ObjectId[] ids = selSet.GetObjectIds();

                        for (int i = 0; i < ids.Length; i++)
                        {
                            listOfXDataIds.Add(ids[i]);
                        }
                    }
                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
            return listOfXDataIds;
        }

        private static ResultBuffer GetResultBufferOfXDataName(ObjectId id, string xDataName, out string xDataRegisterAppName)
        {
            ResultBuffer resultBuff = new ResultBuffer();
            xDataRegisterAppName = "";
            try
            {
                Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                Editor ed = doc.Editor;
                Database db = doc.Database;
                Transaction tr = doc.TransactionManager.StartTransaction();
                using (tr)
                {
                    Entity ent = tr.GetObject(id, OpenMode.ForRead, false) as Entity;
                    ResultBuffer resultBuffer = ent.XData;

                    int count = 0;
                    foreach (TypedValue typeVal in resultBuffer)
                    {
                        count++;
                        try
                        {
                            string typeValAppName = Convert.ToString(typeVal.Value);
                            if (count == 1)
                            {
                                xDataRegisterAppName = typeValAppName;
                            }
                            if (typeValAppName == xDataName)
                            {
                                resultBuff = resultBuffer;
                                break;
                            }
                        }
                        catch (Autodesk.AutoCAD.Runtime.Exception ex)
                        {
                        }
                    }
                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }

            return resultBuff;
        }

        public static string GetXDataRegisterAppName(ObjectId id, int loc = 1)
        {
            string xDataRegisterAppName = "";
            try
            {
                Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                Editor ed = doc.Editor;
                Database db = doc.Database;
                Transaction tr = doc.TransactionManager.StartTransaction();
                using (tr)
                {
                    Entity ent = tr.GetObject(id, OpenMode.ForRead, false) as Entity;
                    ResultBuffer resultBuffer = ent.XData;

                    if (resultBuffer == null)
                    {
                        return xDataRegisterAppName;
                    }

                    int count = 0;
                    foreach (TypedValue typeVal in resultBuffer)
                    {
                        count++;
                        try
                        {
                            string typeValAppName = Convert.ToString(typeVal.Value);
                            if (count == loc)
                            {
                                xDataRegisterAppName = typeValAppName;
                            }
                        }
                        catch (Autodesk.AutoCAD.Runtime.Exception ex)
                        {

                        }
                    }
                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
            return xDataRegisterAppName;
        }

        public static bool CheckXDataRegisterAppName(ObjectId id, string xDataRegisterAppName)
        {
            bool flagOfExist = false;
            try
            {
                Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                Editor ed = doc.Editor;
                Database db = doc.Database;
                Transaction tr = doc.TransactionManager.StartTransaction();
                using (tr)
                {
                    Entity ent = tr.GetObject(id, OpenMode.ForRead, false) as Entity;
                    var resultBuffer = ent.XData;

                    if (resultBuffer == null)
                    {
                        return flagOfExist;
                    }
                    foreach (TypedValue typeVal in resultBuffer)
                    {
                        try
                        {
                            string typeValAppName = Convert.ToString(typeVal.Value);
                            if (typeValAppName == xDataRegisterAppName)
                            {
                                flagOfExist = true;
                                break;
                            }
                        }
                        catch (Autodesk.AutoCAD.Runtime.Exception ex)
                        {

                        }
                    }
                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
            return flagOfExist;
        }

        public static string GetTextOfMtext(ObjectId mtextId)
        {
            string mtext = "";
            try
            {
                Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;

                using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
                {
                    Entity ent = tr.GetObject(mtextId, OpenMode.ForWrite) as Entity;
                    if (ent is MText)
                    {
                        MText mText = ent as MText;
                        mtext = mText.Contents;
                    }
                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
            return mtext;
        }

        public static string GetTextOfDBtext(ObjectId mtextId)
        {
            string dbtext = "";
            try
            {
                Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;

                using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
                {
                    Entity ent = tr.GetObject(mtextId, OpenMode.ForWrite) as Entity;
                    if (ent is DBText)
                    {
                        DBText dbText = ent as DBText;
                        dbtext = dbText.TextString;
                    }
                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
            return dbtext;
        }

        public static void changeDBText(ObjectId textId, string text)
        {
            try
            {
                Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;

                using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
                {
                    Entity ent = tr.GetObject(textId, OpenMode.ForWrite) as Entity;
                    if (ent is DBText)
                    {
                        //var dbText_Before = AEE_Utility.GetTextOfDBtext(textId);

                        DBText dbText = ent as DBText;
                        dbText.TextString = text;

                        //var dbText_After = AEE_Utility.GetTextOfDBtext(textId);
                    }
                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
        }

        public static void changeMText(ObjectId textId, string text)
        {
            try
            {
                Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;

                using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
                {
                    Entity ent = tr.GetObject(textId, OpenMode.ForWrite) as Entity;
                    if (ent is MText)
                    {
                        //var mtext_Before = AEE_Utility.GetTextOfMtext(textId);

                        MText mtext = ent as MText;
                        mtext.Contents = text;

                        //var mtext_After = AEE_Utility.GetTextOfMtext(textId);
                    }
                    tr.Commit();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

            }
        }
        // Testing-123

        public static bool IsPointsAreEqual(Point3d point1, Point3d point2)
        {
            bool flag = false;
            //double accuracy = 0.000000001;
            //Vector3d p1 = point1.GetAsVector();
            //Vector3d p2 = point2.GetAsVector();

            //flag = (Math.Abs(p1.X - p2.X) < accuracy &&
            //        Math.Abs(p1.Y - p2.Y) < accuracy &&
            //        Math.Abs(p1.Z - p2.Z) < accuracy);

            var t = 1e-3;
            var tol = new Tolerance(t, t);
            if (point1.IsEqualTo(point2, tol))
            {
                flag = true;
            }
            return flag;
        }

        public static bool IsPoint3dPresentInPoint3dList(List<Point3d> listOfPoint3d, Point3d checkPt, out int matchIndx)
        {
            bool flag = false;
            matchIndx = -1;
            var t = 1e-6;
            var tol = new Tolerance(t, t);

            foreach (Point3d point in listOfPoint3d)
            {
                if (point.IsEqualTo(checkPt, tol))
                {
                    flag = true;
                    matchIndx = listOfPoint3d.IndexOf(point);
                    break;
                }

            }
            return flag;
        }

        public static bool AddToSelection(ObjectId obj, bool clearselection = false)
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            if (clearselection)
            {
                ed.SetImpliedSelection(new ObjectId[0]);
            }
            TypedValue[] tvs = new TypedValue[1] { new TypedValue((int)DxfCode.Handle, obj.Handle) };
            SelectionFilter sf = new SelectionFilter(tvs);
            PromptSelectionResult psr = ed.SelectAll(sf);
            if (psr.Status != PromptStatus.OK)
                return true;
            else
                return false;
        }
        public static ObjectId DCircle(double x, double y)
        {
            return DCircle(new Point3d(x, y, 0));
        }
        public static ObjectId DCircle(Point3d point)
        {
            // Get the current document and database
            Autodesk.AutoCAD.ApplicationServices.Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Autodesk.AutoCAD.DatabaseServices.Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;
            ObjectId ObjectIDLIne;
            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // Create a line that starts at 5,5 and ends at 12,3
                using (Circle acCircle = new Circle())
                {
                    acCircle.Center = point;
                    acCircle.Radius = 10;
                    acCircle.ColorIndex = 256;
                    //acCircle.Layer = layerName;
                    ObjectIDLIne = acBlkTblRec.AppendEntity(acCircle);
                    acTrans.AddNewlyCreatedDBObject(acCircle, true);
                    ed.UpdateScreen();
                }
                // Save the new object to the database
                acTrans.Commit();
            }

            ZoomToEntity(ObjectIDLIne);
            return ObjectIDLIne;
        }

        static public void ZoomWindow()

        {

            Document doc =

              Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;

            Database db = doc.Database;

            Editor ed = doc.Editor;


            // Get the window coordinates


            PromptPointOptions ppo =

              new PromptPointOptions(

                "\nSpecify first corner:"

              );


            PromptPointResult ppr =

              ed.GetPoint(ppo);


            if (ppr.Status != PromptStatus.OK)

                return;


            Point3d min = ppr.Value;


            PromptCornerOptions pco =

              new PromptCornerOptions(

                "\nSpecify opposite corner: ",

                ppr.Value

              );


            ppr = ed.GetCorner(pco);


            if (ppr.Status != PromptStatus.OK)

                return;


            Point3d max = ppr.Value;


            // Call out helper function

            // [Change this to ZoomWin2 or WoomWin3 to

            // use different zoom techniques]


            ZoomWin(ed, min, max);

        }


        // Zoom to the extents of an entity

        static public void ZoomToEntity()

        {

            Document doc =

           Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;

            Database db = doc.Database;

            Editor ed = doc.Editor;


            // Get the entity to which we'll zoom


            PromptEntityOptions peo =

              new PromptEntityOptions(

                "\nSelect an entity:"

              );


            PromptEntityResult per = ed.GetEntity(peo);


            if (per.Status != PromptStatus.OK)

                return;


            // Extract its extents


            Extents3d ext;


            Transaction tr =

              db.TransactionManager.StartTransaction();

            using (tr)

            {

                Entity ent =

                  (Entity)tr.GetObject(

                    per.ObjectId,

                    OpenMode.ForRead

                  );

                ext =

                  ent.GeometricExtents;

                tr.Commit();

            }


            ext.TransformBy(

              ed.CurrentUserCoordinateSystem.Inverse()

            );


            // Call our helper function

            // [Change this to ZoomWin2 or WoomWin3 to

            // use different zoom techniques]


            ZoomWin(ed, ext.MinPoint, ext.MaxPoint);

        }

        static public void ZoomToEntity(ObjectId objID)

        {

            if (!ZoomObj) return;

            Document doc =

           Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;

            Database db = doc.Database;

            Editor ed = doc.Editor;



            // Extract its extents

            Extents3d ext;
            Transaction tr =

              db.TransactionManager.StartTransaction();
            Entity ent;
            int oldColorIndx;
            using (tr)
            {
                ent = GetEntityForWrite(objID);
                oldColorIndx = ent.ColorIndex;
                ent.ColorIndex = 256;
                ext = ent.GeometricExtents;
                tr.Commit();
            }

            ext.TransformBy(ed.CurrentUserCoordinateSystem.Inverse());

            // Call our helper function
            // [Change this to ZoomWin2 or WoomWin3 to
            // use different zoom techniques]
            ZoomWin(ed, ext.MinPoint, ext.MaxPoint);

            AddToSelection(objID);

            ZoomWindow();

            tr = db.TransactionManager.StartTransaction();
            using (tr)
            {
                ent = GetEntityForWrite(objID);
                ent.ColorIndex = oldColorIndx;
                tr.Commit();
            }
        }

        // Helper functions to zoom using different techniques


        // Zoom using a view object


        private static void ZoomWin(

          Editor ed, Point3d min, Point3d max

        )

        {

            Point2d min2d = new Point2d(min.X, min.Y);

            Point2d max2d = new Point2d(max.X, max.Y);


            ViewTableRecord view =

              new ViewTableRecord();


            view.CenterPoint =

              min2d + ((max2d - min2d) / 2.0);

            view.Height = max2d.Y - min2d.Y;

            view.Width = max2d.X - min2d.X;

            ed.SetCurrentView(view);

        }

        public static Point2d ConvertToPoint2d(this Point3d point)
        {
            return new Point2d(point.X, point.Y);
        }
        public static Point ConvertToPoint(this Point3d point)
        {
            return new Point(point.X, point.Y);
        }
        public static bool IsLinesAreParallel(Line line1, Line line2, out double Offset, out double ClosestPtDist)
        {
            bool result = false;
            Offset = 0;
            ClosestPtDist = 0;

            Point2d l1Stpt = ConvertToPoint2d(line1.StartPoint);
            Point2d l1Endpt = ConvertToPoint2d(line1.EndPoint);
            Point2d l2Stpt = ConvertToPoint2d(line2.StartPoint);
            Point2d l2Endpt = ConvertToPoint2d(line2.EndPoint);


            // reject lines with coincident endpoints
            // if(l1Stpt.IsEqualTo(l2Stpt) || l1Stpt.IsEqualTo(l2Endpt) || l1Endpt.IsEqualTo(l2Stpt) || l1Endpt.IsEqualTo(l2Endpt))
            //    return result;

            var lineSeg1 = new LineSegment2d(l1Stpt, l1Endpt);
            var lineSeg2 = new LineSegment2d(l2Stpt, l2Endpt);

            // reject non-parallel lines
            if (!lineSeg1.IsParallelTo(lineSeg2))
                return result;

            //// reject colinear lines
            //if (lineSeg1.IsColinearTo(lineSeg2))
            //    return result;

            var cp = lineSeg1.GetClosestPointTo(lineSeg2);
            Point2d cp1 = cp[0].Point;
            Point2d cp2 = cp[1].Point;

            Vector2d vx = cp1.GetVectorTo(cp2);

            double lineSeg1Angle = Math.Round(AEE_Utility.convertRadianToDegree(lineSeg1.Direction.Angle));

            if (lineSeg1Angle == 90 || lineSeg1Angle == 270)
            {
                Offset = vx.X;
                ClosestPtDist = vx.Y;
            }
            else if (lineSeg1Angle == 0 || lineSeg1Angle == 180 || lineSeg1Angle == 360)
            {
                Offset = vx.Y;
                ClosestPtDist = vx.X;
            }
            //// reject non-adjacent lines:
            //if (vx.IsPerpendicularTo(lineSeg1.Direction) && vx.X == 0)
            //    Offset = vx.LengthSqrd;

            return true;
        }

        // Cuong - 23/12/2022 - DeckPanel issue - START
        public static void AdjustDeckPanel(ObjectId deckPanelWallId, Point3d centerPt, List<int> segments, double panelDepth)
        {
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            var ct = centerPt;

            foreach (int index in segments)
            {
                // Start a transaction
                using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
                {
                    var ent = acTrans.GetObject(deckPanelWallId, OpenMode.ForWrite) as Polyline;
                    var segment = ent.GetLineSegmentAt(index);

                    Point3d midPoint = segment.MidPoint;
                    Vector3d direction = ct.GetVectorTo(midPoint).GetNormal().Negate();
                    Point3d pt1 = segment.StartPoint.Add(direction * panelDepth);
                    Point3d pt2 = segment.EndPoint.Add(direction * panelDepth);

                    ent.SetPointAt(index, new Point2d(pt1.X, pt1.Y));
                    if (index == ent.NumberOfVertices - 1)
                        ent.SetPointAt(0, new Point2d(pt2.X, pt2.Y));
                    else
                        ent.SetPointAt(index + 1, new Point2d(pt2.X, pt2.Y));

                    ct = centerPt.Add(direction * panelDepth * 0.5);

                    // Save the new object to the database
                    acTrans.Commit();
                }
            }
        }

        public static List<MaxAreaRelationInfo> AdjustRelationInfo(ObjectId deckPanelWallId, Point3d centerPt, List<int> segments, double panelDepth)
        {
            var infoList = new List<MaxAreaRelationInfo>();

            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var ent = acTrans.GetObject(deckPanelWallId, OpenMode.ForRead) as Polyline;
                foreach (int index in segments)
                {
                    var segment = ent.GetLineSegmentAt(index);

                    Point3d midPoint = segment.MidPoint;
                    Vector3d direction = centerPt.GetVectorTo(midPoint).GetNormal().Negate();
                    Point3d pt1 = segment.StartPoint.Add(direction * panelDepth);
                    Point3d pt2 = segment.EndPoint.Add(direction * panelDepth);

                    var info = new MaxAreaRelationInfo();
                    var gapBaseLineSegment = new LineSegment3d(segment.StartPoint, pt1);
                    var directionLine = new LineSegment3d(segment.StartPoint, segment.EndPoint);
                    if (direction.IsCodirectionalTo(Vector3d.YAxis.Negate()) || direction.IsCodirectionalTo(Vector3d.XAxis))
                    {
                        gapBaseLineSegment = new LineSegment3d(pt2, segment.EndPoint);
                        directionLine = new LineSegment3d(segment.EndPoint, segment.StartPoint);
                    }

                    info.GapBaseLineSegment = gapBaseLineSegment;
                    info.DirectionLine = directionLine;

                    infoList.Add(info);
                }

                // Save the new object to the database
                acTrans.Commit();
            }

            return infoList;
        }

        //Added on 11/04/2023 by SDM
        public static bool IsBetween(this Point3d myPoint, Point3d pt1, Point3d pt2, bool flag_X_Axis, bool flag_Y_Axis)
        {
            var min = pt1;
            var max = pt2;

            if (flag_X_Axis)
            {
                min = (pt1.X < pt2.X) ? pt1 : pt2;
                max = !(pt1.X < pt2.X) ? pt1 : pt2;

                if (myPoint.X > min.X && myPoint.X < max.X)
                    return true;
            }
            {
                min = (pt1.Y < pt2.Y) ? pt1 : pt2;
                max = !(pt1.Y < pt2.Y) ? pt1 : pt2;

                if (myPoint.Y > min.Y && myPoint.Y < max.Y)
                    return true;
            }
            return false;
        }
        // Cuong - 23/12/2022 - DeckPanel issue - END
    }
}
