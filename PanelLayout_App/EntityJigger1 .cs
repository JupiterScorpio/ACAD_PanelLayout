using System;
using System.Text;
using System.Linq;
using System.Xml;
using System.Reflection;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Windows;
using MgdAcApplication = Autodesk.AutoCAD.ApplicationServices.Application;
using MgdAcDocument = Autodesk.AutoCAD.ApplicationServices.Document;
using AcWindowsNS = Autodesk.AutoCAD.Windows;

namespace PanelLayout_App
{
    public class EntityJigger1 : EntityJig
    {
        #region Fields
        public int mCurJigFactorIndex = 1;  // Jig Factor Index
        public Autodesk.AutoCAD.Geometry.Point3d mBasePoint; // Jig Factor #1
        public Autodesk.AutoCAD.Geometry.Point3d mSecondPoint; // Jig Factor #2

        #endregion
        #region Constructors
        public EntityJigger1(Xline ent)
            : base(ent)
        {
            // Initialize and transform the Entity.
            Entity.SetDatabaseDefaults();
            Entity.TransformBy(UCS);
        }
        #endregion
        #region Properties
        private Editor Editor
        {
            get
            {
                return MgdAcApplication.DocumentManager.MdiActiveDocument.Editor;
            }
        }
        private Matrix3d UCS
        {
            get
            {
                return MgdAcApplication.DocumentManager.MdiActiveDocument.Editor.CurrentUserCoordinateSystem;
            }
        }
        #endregion
        #region Overrides
        public new Xline Entity  // Overload the Entity property for convenience.
        {
            get
            {
                return base.Entity as Xline;
            }
        }
        protected override bool Update()
        {
            switch (mCurJigFactorIndex)
            {
                case 1:
                    Entity.BasePoint = mBasePoint;
                    Entity.BasePoint.TransformBy(UCS); //Turn it on for UCS transformation or tweak it if not compile.
                    break;
                case 2:
                    Entity.SecondPoint = mSecondPoint;
                    Entity.SecondPoint.TransformBy(UCS); //Turn it on for UCS transformation or tweak it if not compile.
                    break;
                default:
                    return false;
            }
            return true;
        }
        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            switch (mCurJigFactorIndex)
            {
                case 1:
                    JigPromptPointOptions prOptions1 = new JigPromptPointOptions("\nBase point::");
                    // Set properties such as UseBasePoint and BasePoint of the prompt options object if necessary here.
                    prOptions1.UserInputControls = UserInputControls.Accept3dCoordinates | UserInputControls.GovernedByUCSDetect | UserInputControls.UseBasePointElevation;
                    PromptPointResult prResult1 = prompts.AcquirePoint(prOptions1);
                    if (prResult1.Status == PromptStatus.Cancel && prResult1.Status == PromptStatus.Error)
                        return SamplerStatus.Cancel;
                    if (prResult1.Value.Equals(mBasePoint))  //Use better comparison method if necessary.
                    {
                        return SamplerStatus.NoChange;
                    }
                    else
                    {
                        mBasePoint = prResult1.Value;
                        return SamplerStatus.OK;
                    }
                case 2:
                    JigPromptPointOptions prOptions2 = new JigPromptPointOptions("\nSecond point:");
                    // Set properties such as UseBasePoint and BasePoint of the prompt options object if necessary here.
                    prOptions2.UserInputControls = UserInputControls.Accept3dCoordinates | UserInputControls.GovernedByUCSDetect | UserInputControls.UseBasePointElevation;
                    PromptPointResult prResult2 = prompts.AcquirePoint(prOptions2);
                    if (prResult2.Status == PromptStatus.Cancel && prResult2.Status == PromptStatus.Error)
                        return SamplerStatus.Cancel;
                    if (prResult2.Value.Equals(mSecondPoint) || prResult2.Value.Equals(mBasePoint))
                    {
                        return SamplerStatus.NoChange;
                    }
                    else
                    {
                        mSecondPoint = prResult2.Value;
                        return SamplerStatus.OK;
                    }
                default:
                    break;
            }
            return SamplerStatus.OK;
        }

        #endregion
        #region Methods to Call
        public static Xline Jig()
        {
            EntityJigger1 jigger = null;
            try
            {
                jigger = new EntityJigger1(new Xline());
                PromptResult pr;
                do
                {
                    pr = MgdAcApplication.DocumentManager.MdiActiveDocument.Editor.Drag(jigger);
                    if (pr.Status == PromptStatus.Keyword)
                    {
                        // Keyword handling code
                    }
                    else
                    {
                        jigger.mCurJigFactorIndex++;
                    }
                } while (pr.Status != PromptStatus.Cancel && pr.Status != PromptStatus.Error && jigger.mCurJigFactorIndex <= 2);
                if (pr.Status == PromptStatus.Cancel || pr.Status == PromptStatus.Error)
                {
                    if (jigger != null && jigger.Entity != null)
                        jigger.Entity.Dispose();
                    return null;
                }
                else
                    return jigger.Entity;
            }
            catch
            {
                if (jigger != null && jigger.Entity != null)
                    jigger.Entity.Dispose();
                return null;
            }
        }
        #endregion
        #region Test Commands
        //[CommandMethod("test")]
        public static void TestEntityJigger1_Method()
        {
            try
            {
                Entity jigEnt = EntityJigger1.Jig();
                if (jigEnt != null)
                {
                    Database db = HostApplicationServices.WorkingDatabase;
                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        BlockTableRecord btr = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                        btr.AppendEntity(jigEnt);
                        tr.AddNewlyCreatedDBObject(jigEnt, true);
                        tr.Commit();
                    }
                }
            }
            catch (System.Exception ex)
            {
                MgdAcApplication.DocumentManager.MdiActiveDocument.Editor.WriteMessage(ex.ToString());
            }
        }
        #endregion
    }
}
