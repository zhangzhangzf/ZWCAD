using ZWCAD.BaseTools.Extension;
using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.Geometry;
using Mrf.CSharp.BaseTools.Extension;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZWCAD.BaseTools
{
    /// <summary>
    /// 视口对象工具
    /// </summary>
    public class ViewPortTool
    {


        Document m_document;
        Database m_database;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="document">文档对象</param>
        public ViewPortTool(Document document)
        {
            m_document = document;
            m_database = m_document.Database;

        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="database">数据库</param>
        public ViewPortTool(Database database)
        {
            m_database = database;

        }




        /// <summary>
        /// 创建视口
        /// </summary>
        /// <param name="centerPoint">视口的中心点</param>
        /// <param name="width">视口的宽度</param>
        /// <param name="height">视口的高度</param>
        /// <param name="spaceName">如果指定的名称为其它的图纸空间名称且存在，则为指定的图纸空间名称；为空或其它，则为图纸空间</param>
        /// <param name="customScale">视口的自定义比例</param>
        /// <returns>返回创建的视口对象的ObjectId，如果没有初始化document，或者创建失败，返回返回ObjectId.Null</returns>
        public ObjectId CreateDefaultViewport(Point3d centerPoint, double width = 297, double height = 210, string spaceName = null, double customScale = 1)
        {
            return CreateFloatingViewport(centerPoint, width, height, spaceName, customScale);
        }



        /// <summary>
        /// 创建视口
        /// </summary>
        /// <param name="centerPoint">视口的中心点</param>
        /// <param name="width">视口的宽度</param>
        /// <param name="height">视口的高度</param>
        /// <param name="spaceId">空间对象的ObjectId</param>
        /// <param name="customScale">视口的自定义比例</param>
        /// <returns>返回创建的视口对象的ObjectId，如果没有初始化document，或者创建失败，返回返回ObjectId.Null</returns>
        public ObjectId CreateDefaultViewport(ObjectId spaceId, Point3d centerPoint, double width = 297, double height = 210, double customScale = 1)
        {
            return CreateFloatingViewport(centerPoint, width, height, spaceId, customScale);
        }










        /// <summary>
        /// 创建视口
        /// </summary>
        /// <param name="centerPoint">视口的中心点</param>
        /// <param name="width">视口的宽度</param>
        /// <param name="height">视口的高度</param>
        /// <param name="spaceName">如果指定的名称为其它的图纸空间名称且存在，则为指定的图纸空间名称；为空或其它，则为图纸空间</param>
        /// <param name="customScale">视口的自定义比例</param>
        /// <returns>返回创建的视口对象的ObjectId，如果没有初始化document，或者创建失败，返回返回ObjectId.Null</returns>
        public ObjectId CreateFloatingViewport(Point3d centerPoint, double width, double height, string spaceName = null, double customScale = 1)
        {

            ObjectId spaceId;

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                //块表
                BlockTable bt = transaction.GetObject(m_database.BlockTableId, OpenMode.ForRead) as BlockTable;

                if (!string.IsNullOrEmpty(spaceName) && bt.Has(spaceName)) //别的图纸空间
                {

                }
                else //默认图纸空间
                {
                    spaceName = BlockTableRecord.PaperSpace;
                }

                spaceId = bt[spaceName];
            }

            ObjectId viewportId = CreateFloatingViewport(centerPoint, width, height, spaceId, customScale);

            return viewportId;

        }



        //本例设置图纸空间为活动空间，
        //然后创建一个浮动视口，定义视口的视图，并激活该视口。
        //导入外部函数acedSetCurrentVPort。对2012版，该函数在acad.exe内。
        //       [DllImport("acad.exe", CallingConvention = CallingConvention.Cdecl,
        //EntryPoint = "?acedSetCurrentVPort@@YA?AW4ErrorStatus@Acad@@PBVAcDbViewport@@@Z")]
        //       extern static private int acedSetCurrentVPort(IntPtr AcDbVport);


        /// <summary>
        /// 创建视口
        /// </summary>
        /// <param name="centerPoint">视口的中心点</param>
        /// <param name="width">视口的宽度</param>
        /// <param name="height">视口的高度</param>
        /// <param name="spaceId">空间对象的ObjectId</param>
        /// <param name="customScale">视口的自定义比例</param>
        /// <returns>返回创建的视口对象的ObjectId，如果创建失败，则返回ObjectId.Null</returns>
        public ObjectId CreateFloatingViewport(Point3d centerPoint, double width, double height, ObjectId spaceId, double customScale = 1)
        {
            //返回值
            ObjectId viewportId = ObjectId.Null;

            //// 获取数据库，启动事务

            //if (m_document == null)
            //{
            //    return viewportId;
            //}


            using (Transaction acTrans = m_database.TransactionManager.StartTransaction())
            {

                // 以写模式打开块表记录Paper空间
                if (acTrans.GetObject(spaceId, OpenMode.ForWrite) is BlockTableRecord acBlkTblRec)
                {
                    try
                    {

                        //// 切换到Paper空间布局
                        //Application.SetSystemVariable("TILEMODE", 0);
                        //m_document.Editor.SwitchToPaperSpace();

                        // 创建一个视口
                        Viewport acVport = new Viewport
                        {
                            CenterPoint = centerPoint,
                            Width = width,
                            Height = height,
                            CustomScale = customScale
                        };

                        // 添加新对象到块表记录和事务
                        viewportId= acBlkTblRec.AppendEntity(acVport);
                        acTrans.AddNewlyCreatedDBObject(acVport, true);

                        // 修改观察方向
                        acVport.ViewDirection = new Vector3d(0, 0, 1);


                        // 激活视口
                        acVport.On = true;


                        //// 激活视口的模型空间
                        //m_document.Editor.SwitchToModelSpace();


                        // 使用导入的ObjectARX函数设置新视口为当前视口
                        //acedSetCurrentVPort(acVport.UnmanagedObject);

                        // 将新对象保存到数据库
                        acTrans.Commit();

                    }
                    catch
                    {
                        acTrans.Abort();
                    }

                }

            }

            return viewportId;

        }





        /// <summary>
        /// 创建视口
        /// </summary>
        /// <param name="leftBottomPt">左下角的点坐标</param>
        /// <param name="rightTopPt">右上角的点坐标</param>
        /// <param name="spaceName">如果指定的名称为其它的图纸空间名称且存在，则为指定的图纸空间名称；为空或其它，则为图纸空间</param>
        /// <param name="customScale">视口的自定义比例</param>
        /// <returns>返回创建的视口对象的ObjectId，如果失败，返回ObjectId.Null</returns>
        public ObjectId CreateFloatingViewport(Point3d leftBottomPt, Point3d rightTopPt, string spaceName = null, double customScale = 1)
        {
            PointTool pointTool = new PointTool();
            Point3d centerPoint = pointTool.GetMidPoint(leftBottomPt, rightTopPt);
            double width = Math.Abs(leftBottomPt.X - rightTopPt.X);
            double height = Math.Abs(leftBottomPt.Y - rightTopPt.Y);
            return CreateFloatingViewport(centerPoint, width, height, spaceName, customScale);
        }


        /// <summary>
        /// 创建视口
        /// </summary>
        /// <param name="leftBottomPt">左下角的点坐标</param>
        /// <param name="rightTopPt">右上角的点坐标</param>
        /// <param name="spaceId">空间对象的ObjectId</param>
        /// <param name="customScale">视口的自定义比例</param>
        /// <returns>返回创建的视口对象的ObjectId，如果失败，返回ObjectId.Null</returns>
        public ObjectId CreateFloatingViewport(Point3d leftBottomPt, Point3d rightTopPt, ObjectId spaceId, double customScale = 1)
        {
            PointTool pointTool = new PointTool();
            Point3d centerPoint = pointTool.GetMidPoint(leftBottomPt, rightTopPt);
            double width = Math.Abs(leftBottomPt.X - rightTopPt.X);
            double height = Math.Abs(leftBottomPt.Y - rightTopPt.Y);
            return CreateFloatingViewport(centerPoint, width, height, spaceId, customScale);
        }





        /// <summary>
        /// 通过已经存在的实体对象边界创建视口，视口在实体对象所在的布局中，如果为模型空间，将失败；如果成功，实体对象将成为视口的边界对象，不单独存在
        /// </summary>
        /// <param name="objectId">实体对象的ObjectId，边界可以不封闭，但是会自动按封闭的效果</param>
        /// <param name="customScale">视口的自定义比例</param>
        /// <returns>返回创建的视口对象的ObjectId，如果失败，返回ObjectId.Null</returns>
        public ObjectId CreateFloatingViewport(ObjectId objectId, double customScale = 1)
        {
            //返回值
            ObjectId viewportId = ObjectId.Null;

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    Entity entity = transaction.GetObject(objectId, OpenMode.ForRead) as Entity;
                    ObjectId btrtId = entity.BlockId;

                    BlockTableRecord blockTableRecord = transaction.GetObject(btrtId, OpenMode.ForRead) as BlockTableRecord;
                    //是布局空间
                    if (blockTableRecord.IsLayout)
                    {
                        Viewport viewport = new Viewport
                        {
                            // Set the boundary entity and turn the
                            // viewport/clipping on
                            NonRectClipEntityId = objectId,
                            NonRectClipOn = true,
                            On = true,
                            CustomScale = customScale
                        };

                        viewportId=   blockTableRecord.AppendEntity(viewport);
                        transaction.AddNewlyCreatedDBObject(viewport, true);
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();

                }

            }

            return viewportId;

        }







        /// <summary>
        /// 通过边界创建视口
        /// </summary>
        /// <param name="point3DLst">边界的点列表</param>
        /// <param name="spaceName">如果指定的名称为其它的图纸空间名称且存在，则为指定的图纸空间名称；为空或其它，则为图纸空间</param>
        /// <param name="customScale">视口的自定义比例</param>
        /// <returns>返回创建的视口对象的ObjectId，如果失败，返回ObjectId.Null</returns>
        public ObjectId CreateFloatingViewport(List<Point3d> point3DLst, string spaceName = null, double customScale = 1)
        {

            ObjectId spaceId;

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                //块表
                BlockTable bt = transaction.GetObject(m_database.BlockTableId, OpenMode.ForRead) as BlockTable;

                if (!string.IsNullOrEmpty(spaceName) && bt.Has(spaceName)) //别的图纸空间
                {

                }
                else //默认图纸空间
                {
                    spaceName = BlockTableRecord.PaperSpace;
                }

                spaceId = bt[spaceName];
            }


            return CreateFloatingViewport(spaceId, point3DLst, customScale);

        }













        /// <summary>
        /// 通过边界创建视口
        /// </summary>
        /// <param name="spaceId">空间对象的ObjectId</param>
        /// <param name="point3DLst">边界的点列表</param>
        /// <param name="customScale">视口的自定义比例</param>
        /// <returns>返回创建的视口对象的ObjectId，如果失败，返回ObjectId.Null</returns>
        public ObjectId CreateFloatingViewport(ObjectId spaceId, List<Point3d> point3DLst, double customScale = 1)
        {
            //返回值
            ObjectId viewportId = ObjectId.Null;

            if (point3DLst == null || point3DLst.Count <= 1)
            {
                return viewportId;
            }

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    BlockTableRecord ps = (BlockTableRecord)transaction.GetObject(spaceId, OpenMode.ForWrite);

                    // A closed polyline...
                    Polyline pl = new Polyline(point3DLst.Count);
                    for (int i = 0; i < point3DLst.Count; i++)
                    {
                        var item = point3DLst[i];
                        pl.AddVertexAt(i, new Point2d(item.X, item.Y), 0, 0, 0);
                    }

                    pl.Closed = true;


                    // Add our boundary to paperspace and the
                    // transaction
                    ObjectId objectId = ps.AppendEntity(pl);
                    transaction.AddNewlyCreatedDBObject(pl, true);


                    //viewport = new Viewport
                    //{
                    //    // Set the boundary entity and turn the
                    //    // viewport/clipping on
                    //    NonRectClipEntityId = objectId,
                    //NonRectClipOn = true,
                    //    On = true,
                    //    CustomScale = customScale
                    //};


                    Viewport viewport = new Viewport();


                    viewportId=  ps.AppendEntity(viewport);
                    transaction.AddNewlyCreatedDBObject(viewport, true);




                    // Set the boundary entity and turn the
                    // viewport/clipping on
                    viewport.NonRectClipEntityId = objectId;
                    viewport.NonRectClipOn = true;
                    viewport.On = true;


                    //自定义比例设置好像在这里不管用
                    //viewport.CustomScale = customScale;



                    //将对象删除
                    //Entity entity=transaction.GetObject(objectId,OpenMode.ForWrite) as Entity;
                    //entity.Erase(true);


                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();

                }

            }


            //改为在这里设置
            SetCustomScale(viewportId, customScale);

            //设置视口的视图中心点 使跟模型空间的点完全对应
            SetViewCenterAsCenterPoint(viewportId);

            return viewportId;
        }


















        /// <summary>
        /// 设置视口的边界
        /// </summary>
        /// <param name="viewportId">视口对象的ObjectId</param>
        /// <param name="point3DLst">边界的点列表</param>
        /// <returns>返回创建的视口对象的ObjectId，如果失败，返回ObjectId.Null</returns>
        public bool SetBoundingBox(ObjectId viewportId, List<Point3d> point3DLst)
        {
            //返回值
            bool isSucceed = false;

            if (point3DLst == null || point3DLst.Count <= 1)
            {
                return isSucceed;
            }

            //先获取视口所在的空间
            SpaceTool spaceTool = new SpaceTool(m_database);
            var spaceId = spaceTool.GetSpaceByEntity(viewportId);
            if (spaceId.IsNull)
            {
                return isSucceed;
            }

            //创建边界对象
            PolylineTool polylineTool = new PolylineTool(m_database);
            ObjectId polylineId = polylineTool.CreatePolyline(spaceId, point3DLst, true);
            if (polylineId.IsNull)
            {
                return isSucceed;
            }


            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    if (transaction.GetObject(viewportId, OpenMode.ForWrite) is Viewport viewport) //是视口，则修改视口的边界
                    {
                        // Set the boundary entity and turn the
                        // viewport/clipping on
                        viewport.NonRectClipEntityId = polylineId;

                        isSucceed=true;
                    }
                    else //不是视口，则需要把原来创建的边界删除
                    {
                        Entity entity = transaction.GetObject(polylineId, OpenMode.ForWrite) as Entity;
                        entity.Erase();
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();
                }

            }

            return isSucceed;
        }



















        /// <summary>
        /// 设置视口开或关
        /// </summary>
        /// <param name="viewPortIds">视口的ObjectId列表</param>
        /// <param name="setOn">开或关</param>
        public void SetViewOn(List<ObjectId> viewPortIds, bool setOn = true)
        {
            Database database = m_database;

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {

                foreach (ObjectId objectId in viewPortIds)
                {

                    Viewport vp = transaction.GetObject(objectId, OpenMode.ForWrite, true) as Viewport;
                    if (vp == null)
                    {
                        continue;
                    }
                    vp.On = setOn;
                }
                transaction.Commit();
            }
        }




        /// <summary>
        /// 设置视口的视图中心点
        /// </summary>
        /// <param name="viewportId">视口对象的ObjectId</param>
        /// <param name="viewCenter">视图中心点</param>
        /// <returns>如果成功，返回true，否则，返回false</returns>
        public bool SetViewCenter(ObjectId viewportId, Point2d viewCenter)
        {
            //返回值
            bool isSucceed = false;

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    Viewport vp = transaction.GetObject(viewportId, OpenMode.ForWrite, true) as Viewport;
                    vp.ViewCenter = viewCenter;
                    transaction.Commit();
                    isSucceed = true;
                }
                catch
                {
                    transaction.Abort();
                }
            }

            return isSucceed;
        }



        /// <summary>
        /// 设置视口的视图中心点 CenterPoint为图纸空间中视口中心点在图纸空间中的坐标值， 
        /// ViewCenter为CenterPoint对应的模型空间中的点,将ViewCenter的值设置为CenterPoint的值，
        /// 这样就可以使视口中的点跟模型空间中的点完全对应起来
        /// </summary>
        /// <param name="viewportId">视口对象的ObjectId</param>
        /// <returns>如果成功，返回true，否则，返回false</returns>
        public bool SetViewCenterAsCenterPoint(ObjectId viewportId)
        {
            //返回值
            bool isSucceed = false;

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    if (transaction.GetObject(viewportId, OpenMode.ForWrite, true) is Viewport vp)
                    {
                        Point3d centerPoint = vp.CenterPoint;
                        vp.ViewCenter = new Point2d(centerPoint.X, centerPoint.Y);
                        isSucceed = true;
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();
                }
            }

            return isSucceed;
        }



        /// <summary>
        /// 获取视口的视图中心点 CenterPoint为图纸空间中视口中心点在图纸空间中的坐标值， 
        /// </summary>
        /// <param name="viewportId">视口对象的ObjectId</param>
        /// <returns>如果失败，返回null</returns>
        public Point3d? GetCenterPoint(ObjectId viewportId)
        {
            //返回值
            Point3d? centerPoint = null;

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    if (transaction.GetObject(viewportId, OpenMode.ForWrite, true) is Viewport vp)
                    {
                        centerPoint = vp.CenterPoint;
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();
                }
            }

            return centerPoint;
        }




        /// <summary>
        /// 获取视口的视图中心点
        /// </summary>
        /// <param name="viewportId">视口对象的ObjectId</param>
        /// <returns>视口的视图中心点，如果失败，返回null</returns>
        public Point2d? GetViewCenter(ObjectId viewportId)
        {
            //返回值
            Point2d viewCenter = Point2d.Origin;

            if (viewportId.IsNull)
            {
                return null;
            }

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    if (transaction.GetObject(viewportId, OpenMode.ForRead, true) is Viewport viewport)
                    {
                        viewCenter= viewport.ViewCenter;
                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();
                }
            }

            return viewCenter;
        }















        /// <summary>
        /// 设置视口的自定义比例
        /// </summary>
        /// <param name="viewportId">视口对象的ObjectId</param>
        /// <param name="customScale">自定义比例</param>
        /// <returns>如果成功，返回true，否则，返回false</returns>
        public bool SetCustomScale(ObjectId viewportId, double customScale)
        {
            //返回值
            bool isSucceed = false;


            if (viewportId.IsNull)
            {
                return isSucceed;
            }

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {

                    if (transaction.GetObject(viewportId, OpenMode.ForWrite, true) is Viewport vp)
                    {
                        vp.CustomScale = customScale;
                        isSucceed = true;
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();
                }
            }

            return isSucceed;
        }




        /// <summary>
        /// 获取视口的自定义比例
        /// </summary>
        /// <param name="viewportId">视口对象的ObjectId</param>
        /// <returns>如果成功，返回视口比例，否则，返回默认值1</returns>
        public double GetCustomScale(ObjectId viewportId)
        {
            //返回值
            double customScale = 1;


            if (viewportId.IsNull)
            {
                return customScale;
            }

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {

                    if (transaction.GetObject(viewportId, OpenMode.ForWrite, true) is Viewport vp)
                    {
                        customScale= vp.CustomScale ;
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();
                }
            }

            return customScale;
        }













        /// <summary>
        /// 缩放视口，同时设置视口的自定义比例 需要注意的是，这个scale不是自定义比例的实际值，而是在原来的基础上缩放这个倍数
        /// </summary>
        /// <param name="viewportId">视口对象的ObjectId</param>
        /// <param name="scale">需要缩放的比例</param>
        /// <returns>如果成功，返回true，否则，返回false</returns>
        public bool ScaleViewport(ObjectId viewportId, double scale)
        {
            //返回值
            bool isSucceed = false;


            if (viewportId.IsNull)
            {
                return isSucceed;
            }

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {

                    if (transaction.GetObject(viewportId, OpenMode.ForWrite, true) is Viewport vp)
                    {
                        Point3d centerPoint = vp.CenterPoint;

                        ObjectId boundingBoxId = vp.NonRectClipEntityId;

                        if (boundingBoxId.IsNull)
                        {
                            //这个缩放，vp.CustomScale 会自动改掉，不需要专门设置了 如果设置了 那么就重复缩放了 结果是缩放了两次
                            vp.TransformBy(Matrix3d.Scaling(scale, centerPoint));
                        }
                        else
                        {
                            vp.CustomScale *= scale;
                            Entity boundingBox = transaction.GetObject(boundingBoxId, OpenMode.ForWrite, true) as Entity;

                            boundingBox.TransformBy(Matrix3d.Scaling(scale, centerPoint));

                        }



                        isSucceed = true;
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();
                }
            }

            return isSucceed;
        }






        /// <summary>
        /// 移除集合中的视口和视口对象边界的对象
        /// </summary>
        /// <param name="objectIdLst">实体对象集合</param>
        /// <param name="viewportId">视口对象的ObjectId</param>
        /// <returns>如果存在，返回边界对象的ObjectId,如果不存在（如矩形），则返回ObjectId.Null</returns>
        public void RemoveViewport(List<ObjectId> objectIdLst, ObjectId viewportId)
        {
            if (!viewportId.IsNull)
            {
                objectIdLst.Remove(viewportId);
                ObjectId boundingBoxId = GetNonRectClipEntityId(viewportId);
                if (!boundingBoxId.IsNull)
                {
                    objectIdLst.Remove(boundingBoxId);
                }
            }
        }







        /// <summary>
        /// 获取用于定义视口对象边界的对象
        /// </summary>
        /// <param name="viewportId">视口对象的ObjectId</param>
        /// <returns>如果存在，返回边界对象的ObjectId,如果不存在（如矩形），则返回ObjectId.Null</returns>
        public ObjectId GetNonRectClipEntityId(ObjectId viewportId)
        {
            //返回值
            ObjectId boundingBoxId = ObjectId.Null;


            if (viewportId.IsNull)
            {
                return boundingBoxId;
            }

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {

                    if (transaction.GetObject(viewportId, OpenMode.ForRead, true) is Viewport vp)
                    {
                        //如果有用对象定义边界，则返回对象的ObjectId，否则返回Object.Null
                         boundingBoxId = vp.NonRectClipEntityId;
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();
                }
            }

            return boundingBoxId;
        }























        /// <summary>
        /// 缩放视口，同时设置视口的自定义比例 需要注意的是，这个scale不是自定义比例的实际值，而是在原来的基础上缩放这个倍数
        /// </summary>
        /// <param name="viewportId">视口对象的ObjectId</param>
        /// <param name="subEntityIdLst">视口内部的对象列表，不包括视口本身</param>
        /// <param name="scale">需要缩放的比例</param>
        /// <returns>如果成功，返回true，否则，返回false</returns>
        public bool ScaleViewportAndSubEntities(ObjectId viewportId, List<ObjectId> subEntityIdLst, double scale)
        {
            //返回值
            bool isSucceed = ScaleViewport(viewportId, scale);

            //如果成功
            if (isSucceed)
            {
                Point3d? centerPoint = GetCenterPoint(viewportId);
                if (centerPoint.HasValue)
                {
                    ObjectTool objectTool = new ObjectTool(m_database);
                    isSucceed=  objectTool.ScaleEntities(subEntityIdLst, centerPoint.Value, scale);
                }

            }

            return isSucceed;
        }








        /// <summary>
        /// 在指定视口中冻结指定的图层名称
        /// </summary>
        /// <param name="viewportId">视口对象的ObjectId</param>
        /// <param name="layerName">图层名称，大小写不敏感</param>
        /// <returns>如果成功，返回true,否则，返回false</returns>
        public bool FreezeLayer(ObjectId viewportId, string layerName)
        {

            //返回值
            bool isSucceed = false;

            using (Transaction transaction = m_document.TransactionManager.StartTransaction())
            {
                try
                {
                    if (transaction.GetObject(viewportId, OpenMode.ForWrite, true) is Viewport viewport)
                    {
                        isSucceed=viewport.FreezeLayer(layerName);
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();

                }

            }

            return isSucceed;

        }




        /// <summary>
        /// 在指定视口中冻结指定的图层名称列表
        /// </summary>
        /// <param name="viewportId">视口对象的ObjectId</param>
        /// <param name="layerNameLst">图层名称列表，大小写不敏感</param>
        public void FreezeLayers(ObjectId viewportId, List<string> layerNameLst)
        {
            using (Transaction transaction = m_document.TransactionManager.StartTransaction())
            {
                try
                {
                    if (transaction.GetObject(viewportId, OpenMode.ForWrite, true) is Viewport viewport)
                    {
                        viewport.FreezeLayers(layerNameLst);
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();

                }

            }

        }








        /// <summary>
        /// 在指定视口中解冻指定的图层名称
        /// </summary>
        /// <param name="viewportId">视口对象的ObjectId</param>
        /// <param name="layerName">图层名称，大小写不敏感</param>
        /// <returns>如果成功，返回true,否则，返回false</returns>
        public bool ThawLayer(ObjectId viewportId, string layerName)
        {

            //返回值
            bool isSucceed = false;

            using (Transaction transaction = m_document.TransactionManager.StartTransaction())
            {
                try
                {
                    if (transaction.GetObject(viewportId, OpenMode.ForWrite, true) is Viewport viewport)
                    {
                        isSucceed=viewport.ThawLayer(layerName);
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();

                }

            }

            return isSucceed;

        }




        /// <summary>
        /// 在指定视口中解冻指定的图层名称列表
        /// </summary>
        /// <param name="viewportId">视口对象的ObjectId</param>
        /// <param name="layerNameLst">图层名称列表，大小写不敏感</param>
        public void ThawLayers(ObjectId viewportId, List<string> layerNameLst)
        {
            using (Transaction transaction = m_document.TransactionManager.StartTransaction())
            {
                try
                {
                    if (transaction.GetObject(viewportId, OpenMode.ForWrite, true) is Viewport viewport)
                    {
                        viewport.ThawLayers(layerNameLst);
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();

                }

            }

        }
















        /// <summary>
        /// 在指定视口中除了指定图层的名称列表外， 冻结所有其它的图层名称列表
        /// </summary>
        /// <param name="viewportId">视口对象的ObjectId</param>
        /// <param name="exceptLayerNameLst">图层名称，大小写不敏感</param>
        public void FreezeAllLayerExcept(ObjectId viewportId, List<string> exceptLayerNameLst)
        {

            //先获取所有的图层
            LayerTool layerTool = new LayerTool(m_database);

            List<string> allLayerNameLst = layerTool.GetAllLayerNames();


            //复制一份
            var tmpExceptLayerNameLst = exceptLayerNameLst.ToList();

            //先删除掉所有本来就不存在的图层
            tmpExceptLayerNameLst.RemoveAll(x => !allLayerNameLst.Exists(y => y.Equals(x, StringComparison.OrdinalIgnoreCase)));

            //获取需要冻结的所有图层列表
            List<string> freezeLayerLst = allLayerNameLst.Exception(tmpExceptLayerNameLst);

            //冻结图层
            FreezeLayers(viewportId, freezeLayerLst);

            //解冻图层
            ThawLayers(viewportId, tmpExceptLayerNameLst);

        }





        /// <summary>
        /// 获取指定空间所有的视口对象列表
        /// </summary>
        /// <param name="spaceId">空间对象的ObjectId</param>
        /// <returns>所有的视口对象列表，如果失败，返回空的列表</returns>
        public List<ObjectId> GetAllViewports(ObjectId spaceId)
        {
            //返回值
            List<ObjectId> viewportIdLst = new List<ObjectId>();

            if (spaceId.IsNull)
            {
                return viewportIdLst;
            }

            SpaceTool spaceTool = new SpaceTool(m_database);

            List<string> dxfNameLst = new List<string> { "viewport" };
           viewportIdLst = spaceTool.GetAllEntitiesInSpace(spaceId, dxfNameLst);

            return viewportIdLst;
        }




    }
}
