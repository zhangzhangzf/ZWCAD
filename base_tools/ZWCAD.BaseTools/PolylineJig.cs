using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;
using ZwSoft.ZwCAD.Geometry;
using System.Activities.Statements;
using Point3d = ZwSoft.ZwCAD.Geometry.Point3d;

/*
 * https://codeantenna.com/a/vuIr1ZfdDK
 * 
 */





namespace ZWCAD.BaseTools
{
    /// <summary>
    /// 自定义创建多段线
    /// </summary>
    public class PolylineJig : EntityJig
    {



        #region Private Variables

        Document m_document;

        public static int color = 0;
        public static Point3dCollection m_pts;
        Point3d m_tempPoint;
        Plane m_plane;
        #endregion



        #region Default Constructor

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ucs"></param>
        public PolylineJig(Matrix3d ucs)
            : base(new Polyline())
        {
            m_pts = new Point3dCollection();
            Point3d origin = new Point3d(0, 0, 0);
            Vector3d normal = new Vector3d(0, 0, 1);
            normal = normal.TransformBy(ucs);
            m_plane = new Plane(origin, normal);
            Polyline pline = Entity as Polyline;
            pline.SetDatabaseDefaults();
            pline.Normal = normal;
            pline.ColorIndex = color;
            pline.AddVertexAt(0, new Point2d(0, 0), 0, 0, 0);
        }
        #endregion




        #region CommandMethods


        #endregion



        #region Helper Methods


        #endregion


        #region Properties


        #endregion


        #region 构造方法


        #endregion

        /// <summary>
        /// 画线
        /// </summary>
        /// <param name="colorIndex"></param>
        /// <returns></returns>
        public Point3dCollection DragLine(int colorIndex)
        {
            return PolyJig(colorIndex);
        }

        #region 画线方法
        /// <summary>
        /// 画线
        /// </summary>
        /// <param name="colorIndex">颜色</param>
        /// <returns></returns>
        public static Point3dCollection PolyJig(int colorIndex)
        {
            color = colorIndex;
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Matrix3d ucs = ed.CurrentUserCoordinateSystem;
            PolylineJig jig = new PolylineJig(ucs);
            bool bSuccess = true, bComplete = false;
            do
            {
                PromptResult res = ed.Drag(jig);
                bSuccess = (res.Status == PromptStatus.OK);


                if (bSuccess)

                    jig.AddLatestVertex();




                if (res.Status != PromptStatus.OK)
                {

                    //switch (res.Status)
                    //{
                    //    case Confirm.Operate.Continue:
                    //        bSuccess = true;
                    //        bComplete = false;
                    //        break;
                    //    case Confirm.Operate.Back:
                    //        if (m_pts.Count > 0)
                    //        {
                    //            m_pts.RemoveAt(m_pts.Count - 1);
                    //            bSuccess = true;
                    //            bComplete = false;
                    //            jig.RemoveLastVertex();
                    //        }
                    //        break;
                    //    case Confirm.Operate.Accept:
                    //        break;
                    //}

                       switch (res.Status)
                    {
                        case PromptStatus.None:
                            bSuccess = true;
                            bComplete = false;
                            break;
                        //case Confirm.Operate.Back:
                        //    if (m_pts.Count > 0)
                        //    {
                        //        m_pts.RemoveAt(m_pts.Count - 1);
                        //        bSuccess = true;
                        //        bComplete = false;
                        //        jig.RemoveLastVertex();
                        //    }
                        //    break;
                        case PromptStatus.Cancel:
                            break;
                    }





                }
            } while (bSuccess && !bComplete);
            return m_pts;
        }

        #endregion
        #region 用于在完成Drag后，移除最后个虚构的点
        /// <summary>
        /// 用于在完成Drag后，移除最后个虚构的点
        /// </summary>
        public void RemoveLastVertex()
        {
            Polyline pline = Entity as Polyline;
            if (pline.NumberOfVertices > 1)
            {
                pline.RemoveVertexAt(m_pts.Count);
            }
        }
        #endregion

        #region 在添加一个点时激发事件
        /// <summary>
        /// 事件委托
        /// </summary>
        /// <param name="e">事件参数</param>
        public delegate void AddHandle(EventArgs e);

        public class EventArgs
        {
            public EventArgs(Point3dCollection currentPoints, int index)
            {
                this.currentPoints = currentPoints;
                this.index = index;
            }
            /// <summary>
            /// 当前点集合
            /// </summary>
            private Point3dCollection currentPoints;
            /// <summary>
            /// 当前点集合
            /// </summary>
            public Point3dCollection CurrentPoints
            {
                get { return currentPoints; }
            }
            /// <summary>
            /// 当前点索引
            /// </summary>
            private int index;
            /// <summary>
            /// 当前点索引
            /// </summary>
            public int Index
            {
                get { return index; }
            }
            /// <summary>
            /// 是否取消点绘制
            /// </summary>
            private bool isCancel = false;
            /// <summary>
            /// 是否取消点绘制
            /// </summary>
            public bool IsCancel
            {
                get { return isCancel; }
                set { isCancel = value; }
            }
        }

        /// <summary>
        /// 添加一个点时激发事件
        /// </summary>
        public static event AddHandle AddPoint;
        #endregion
        #region 总是设置polyline为一个虚构的点，在完成Drag后，此点会被移除
        /// <summary>
        /// 总是设置polyline为一个虚构的点，在完成Drag后，此点会被移除
        /// </summary>
        public void AddLatestVertex()
        {
            m_pts.Add(m_tempPoint);
            Polyline pline = Entity as Polyline;
            pline.AddVertexAt(pline.NumberOfVertices, new Point2d(m_tempPoint.X, m_tempPoint.Y), 0, 0, 0);
            if (AddPoint != null)
            {
                EventArgs e = new EventArgs(m_pts, m_pts.Count - 1);
                e.IsCancel = false;
                AddPoint(e);
                if (e.IsCancel)
                {
                    RemoveLastVertex();
                }
            }
        }
        #endregion

        #region 取样
        /// <summary>
        /// 取样
        /// </summary>
        /// <param name="prompts"></param>
        /// <returns></returns>
        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            JigPromptPointOptions jigOpts = new JigPromptPointOptions
            {
                UserInputControls = 
                
                
                //(UserInputControls.Accept3dCoordinates |
                //UserInputControls.NullResponseAccepted |
                //UserInputControls.AcceptOtherInputString |
                //UserInputControls.NoNegativeResponseAccepted)

                 UserInputControls.NullResponseAccepted | UserInputControls.Accept3dCoordinates |
                UserInputControls.GovernedByUCSDetect | UserInputControls.GovernedByOrthoMode |
                UserInputControls.AcceptMouseUpAsPoint



        };
            if (m_pts.Count == 0)
            {
                jigOpts.Message = "\n请输入起点坐标 ";
            }
            else if (m_pts.Count > 0)
            {
                jigOpts.BasePoint = m_pts[m_pts.Count - 1];
                jigOpts.UseBasePoint = true;
                jigOpts.Message = "\n请输入下一个点[或按ESC退出] ";
            }
            else
                return SamplerStatus.Cancel;
            PromptPointResult res = prompts.AcquirePoint(jigOpts);
            if (m_tempPoint == res.Value)
            {
                return SamplerStatus.NoChange;
            }
            else if (res.Status == PromptStatus.OK)
            {
                m_tempPoint = res.Value;
                return SamplerStatus.OK;
            }
            return SamplerStatus.Cancel;
        }
        #endregion


        #region 更新
        /// <summary>
        /// 更新
        /// </summary>
        /// <returns></returns>
        protected override bool Update()
        {
            Polyline pline = Entity as Polyline;
            pline.SetPointAt(pline.NumberOfVertices - 1, m_tempPoint.Convert2d(m_plane));
            Matrix3d m = Matrix3d.Displacement(new Vector3d(0, 0, 1000));
            pline.TransformBy(m);
            return true;
        }
        #endregion


    }

  

    }
