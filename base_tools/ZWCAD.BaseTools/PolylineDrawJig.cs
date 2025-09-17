/*
 * https://spiderinnet1.typepad.com/blog/2012/05/autocad-net-drawjig-poly-lines.html
 */
#region Namespaces

using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;
using ZwSoft.ZwCAD.Geometry;
using ZwSoft.ZwCAD.GraphicsInterface;
using ZwSoft.ZwCAD.Runtime;
using System;
using MgdAcApplication = ZwSoft.ZwCAD.ApplicationServices.Application;

#endregion

namespace ZWCAD.BaseTools
{
    public class PolylineDrawJig : DrawJig
    {
        #region Fields

        public Point3dCollection mAllVertexes = new Point3dCollection();
        public Point3d mLastVertex;

        #endregion

        #region Constructors

        public PolylineDrawJig()
        {
        }



        #endregion

        #region Properties

        public Point3d LastVertex
        {
            get { return mLastVertex; }
            set { mLastVertex = value; }
        }

        private Editor Editor
        {
            get
            {
                return MgdAcApplication.DocumentManager.MdiActiveDocument.Editor;
            }
        }

        public Matrix3d UCS
        {
            get
            {
                return MgdAcApplication.DocumentManager.MdiActiveDocument.Editor.CurrentUserCoordinateSystem;
            }
        }

        #endregion

        #region Overrides

        protected override bool WorldDraw(WorldDraw draw)
        {
            WorldGeometry geo = draw.Geometry;
            if (geo != null)
            {
                geo.PushModelTransform(UCS);

                Point3dCollection tempPts = new Point3dCollection();
                foreach (Point3d pt in mAllVertexes)
                {
                    tempPts.Add(pt);
                }
                if (mLastVertex != null)
                    tempPts.Add(mLastVertex);
                if (tempPts.Count > 0)
                    geo.Polyline(tempPts, Vector3d.ZAxis, IntPtr.Zero);

                geo.PopModelTransform();
            }

            return true;
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            JigPromptPointOptions prOptions1 = new JigPromptPointOptions("\nVertex(Enter to finish)");
            prOptions1.UseBasePoint = false;
            prOptions1.UserInputControls =
                UserInputControls.NullResponseAccepted | UserInputControls.Accept3dCoordinates |
                UserInputControls.GovernedByUCSDetect | UserInputControls.GovernedByOrthoMode |
                UserInputControls.AcceptMouseUpAsPoint;

            PromptPointResult prResult1 = prompts.AcquirePoint(prOptions1);
            if (prResult1.Status == PromptStatus.Cancel || prResult1.Status == PromptStatus.Error)
                return SamplerStatus.Cancel;

            Point3d tempPt = prResult1.Value.TransformBy(UCS.Inverse());
            mLastVertex = tempPt;

            return SamplerStatus.OK;
        }

        #endregion


    }

    public static class SamplerStatus1
    {

        #region Commands


        [CommandMethod("TestDrawJigger6")]
        public static void TestDrawJigger6_Method()
        {
            try
            {



                Database db = HostApplicationServices.WorkingDatabase;
                PolylineDrawJig jigger = new PolylineDrawJig();
                PromptResult jigRes;
                do
                {
                    jigRes = MgdAcApplication.DocumentManager.MdiActiveDocument.Editor.Drag(jigger);
                    if (jigRes.Status == PromptStatus.OK)
                        jigger.mAllVertexes.Add(jigger.mLastVertex);
                } while (jigRes.Status == PromptStatus.OK);

                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);

                    ZwSoft.ZwCAD.DatabaseServices.Polyline ent = new ZwSoft.ZwCAD.DatabaseServices.Polyline();
                    ent.SetDatabaseDefaults();
                    for (int i = 0; i < jigger.mAllVertexes.Count; i++)
                    {
                        Point3d pt3d = jigger.mAllVertexes[i];
                        Point2d pt2d = new Point2d(pt3d.X, pt3d.Y);
                        ent.AddVertexAt(i, pt2d, 0, db.Plinewid, db.Plinewid);
                    }
                    ent.TransformBy(jigger.UCS);
                    btr.AppendEntity(ent);
                    tr.AddNewlyCreatedDBObject(ent, true);

                    tr.Commit();
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