using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanelLayout_App
{
    public class RotationHelper
    {
        public class MoveRorationScaleJig : DrawJig
        {
            #region Fields
            Autodesk.AutoCAD.Geometry.Point3d mBase;
            private List<Entity> mEntities = new List<Entity>();

            private int mTotalJigFactorCount = 3;
            private int mCurJigFactorIndex = 1;  // Jig Factor Index
            public Autodesk.AutoCAD.Geometry.Point3d mLocation; // Jig Factor #1
            public Double mAngle; // Jig Factor #2
            public Double mScaleFactor; // Jig Factor #3
            #endregion
            #region Constructors
            public MoveRorationScaleJig(Point3d basePt)
            {
                mBase = basePt.TransformBy(UCS);
                //TODO: Initialize jig factors and transform them if necessary.
                mLocation = mBase;
                mAngle = 0;
                mScaleFactor = 1;
            }
            #endregion
            #region Properties
            public Autodesk.AutoCAD.Geometry.Point3d Location
            {
                get { return mLocation; }
                set { mLocation = value; }
            }
            public Double Angle
            {
                get { return mAngle; }
                set { mAngle = value; }
            }
            public Double ScaleFactor
            {
                get { return mScaleFactor; }
                set { mScaleFactor = value; }
            }
            public Autodesk.AutoCAD.Geometry.Point3d Base
            {
                get { return mBase; }
                set { mBase = value; }
            }
            public Editor AcEditor
            {
                get
                {
                    return Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
                }
            }
            public Matrix3d UCS
            {
                get
                {
                    return AcEditor.CurrentUserCoordinateSystem;
                }
            }
            public Matrix3d Transformation
            {
                get
                {
                    //return Matrix3d.Identity; //* Change it to anything else meaningful.
                    return Matrix3d.Scaling(mScaleFactor, CommonModule.basePoint).
                     PostMultiplyBy(Matrix3d.Rotation(CommonModule.rotateAngle, Vector3d.ZAxis.TransformBy(UCS), CommonModule.basePoint)).
                     PostMultiplyBy(Matrix3d.Displacement(CommonModule.basePoint.GetVectorTo(CommonModule.basePoint)));
                }
            }
            #endregion
            #region Methods
            public void AddEntity(Entity ent)
            {
                mEntities.Add(ent);
            }
            public void TransformEntities()
            {
                Matrix3d mat = Transformation;
                foreach (Entity ent in mEntities)
                {
                    ent.TransformBy(mat);
                }
            }
            #endregion
            #region Overrides
            protected override bool WorldDraw(Autodesk.AutoCAD.GraphicsInterface.WorldDraw draw)
            {
                Matrix3d mat = Transformation;
                WorldGeometry geo = draw.Geometry;
                if (geo != null)
                {
                    geo.PushModelTransform(mat);
                    foreach (Entity ent in mEntities)
                    {
                        geo.Draw(ent);
                    }
                    geo.PopModelTransform();
                }
                return true;
            }
            protected override SamplerStatus Sampler(JigPrompts prompts)
            {
                switch (mCurJigFactorIndex)
                {
                    case 1:
                    //JigPromptPointOptions prOptions1 = new JigPromptPointOptions("\nMove:");
                    //// Set properties such as UseBasePoint and BasePoint of the prompt options object if necessary here.
                    //prOptions1.UserInputControls = UserInputControls.GovernedByOrthoMode | UserInputControls.GovernedByUCSDetect;
                    //PromptPointResult prResult1 = prompts.AcquirePoint(prOptions1);
                    //if (prResult1.Status == PromptStatus.Cancel && prResult1.Status == PromptStatus.Error)
                    //    return SamplerStatus.Cancel;
                    //if (prResult1.Value.Equals(mLocation))  //Use better comparison method if necessary.
                    //{
                    //    return SamplerStatus.NoChange;
                    //}
                    //else
                    //{
                    //    mLocation = prResult1.Value;
                    //    return SamplerStatus.OK;
                    //}
                    case 2:
                    //JigPromptAngleOptions prOptions2 = new JigPromptAngleOptions("\nRotate:");
                    //prOptions2.UseBasePoint = true;
                    //prOptions2.BasePoint = mLocation;
                    //prOptions2.UserInputControls = UserInputControls.GovernedByOrthoMode | UserInputControls.GovernedByUCSDetect;
                    //PromptDoubleResult prResult2 = prompts.AcquireAngle(prOptions2);
                    //if (prResult2.Status == PromptStatus.Cancel && prResult2.Status == PromptStatus.Error)
                    //    return SamplerStatus.Cancel;
                    //if (prResult2.Value.Equals(mAngle))  //Use better comparison method if necessary.
                    //{
                    //    return SamplerStatus.NoChange;
                    //}
                    //else
                    //{
                    //    mAngle = prResult2.Value;
                    //    return SamplerStatus.OK;
                    //}
                    case 3:
                    //JigPromptDistanceOptions prOptions3 = new JigPromptDistanceOptions("\nScale:");
                    //prOptions3.UseBasePoint = true;
                    //prOptions3.BasePoint = mLocation;
                    //prOptions3.UserInputControls = UserInputControls.GovernedByOrthoMode | UserInputControls.GovernedByUCSDetect;
                    //PromptDoubleResult prResult3 = prompts.AcquireDistance(prOptions3);
                    //if (prResult3.Status == PromptStatus.Cancel && prResult3.Status == PromptStatus.Error)
                    //    return SamplerStatus.Cancel;
                    //if (prResult3.Value.Equals(mScaleFactor))  //Use better comparison method if necessary.
                    //{
                    //    return SamplerStatus.NoChange;
                    //}
                    //else
                    //{
                    //    mScaleFactor = prResult3.Value;
                    //    return SamplerStatus.OK;
                    //}
                    default:
                        break;
                }
                return SamplerStatus.OK;
            }
            #endregion
            #region Method to Call
            public bool Jig()
            {
                try
                {
                    //PromptResult pr;
                    //do
                    //{
                    //    pr = AcEditor.Drag(this);
                    //    if (pr.Status == PromptStatus.Keyword)
                    //    {
                    //        // Keyword handling code
                    //    }
                    //    else
                    //        this.mCurJigFactorIndex++;
                    //} while ((pr.Status != PromptStatus.Cancel && pr.Status != PromptStatus.Error)
                    // && this.mCurJigFactorIndex <= this.mTotalJigFactorCount);
                    //if (this.mCurJigFactorIndex == this.mTotalJigFactorCount + 1)
                    return true;
                    //else
                    //    return false;
                }
                catch { return false; }
            }

            public static void RotationScaleJig_Method(List<ObjectId> listOfId)
            {
                Database db = HostApplicationServices.WorkingDatabase;
                Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
                try
                {
                    MoveRorationScaleJig jigger = new MoveRorationScaleJig(CommonModule.basePoint);
                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        foreach (ObjectId id in listOfId)
                        {
                            Entity ent = (Entity)tr.GetObject(id, OpenMode.ForWrite);
                            jigger.AddEntity(ent);
                        }
                        if (jigger.Jig())
                        {
                            jigger.TransformEntities();
                            tr.Commit();
                        }
                        else
                            tr.Abort();
                    }
                }
                catch (System.Exception ex)
                {
                    ed.WriteMessage(ex.ToString());
                }
            }
            #endregion
        }

    }
}
