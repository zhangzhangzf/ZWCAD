using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;
using ZwSoft.ZwCAD.Geometry;
using System.Collections.Generic;


namespace ZWCAD.BaseTools.Extension
{
    /// <summary>
    /// 模型空间和图纸空间相互转换的工具
    /// </summary>
    public static class ViewportExtension
    {


        /// <summary>
        ///获取模型空间的点到图纸空间的点的缩放矩阵
        /// </summary>
        /// <param name="viewport">视口对象</param>
        /// <returns>缩放矩阵</returns>
        public static Matrix3d GetTransformScaleToPaperSpace(this Viewport viewport)
        {
            //获取图纸空间中视口的自定义比例
            double scale = viewport.CustomScale;

            Matrix3d transformScale = Matrix3d.Scaling(scale, Point3d.Origin);

            return transformScale;
        }




        /// <summary>
        ///获取模型空间的点到图纸空间的点的移动向量矩阵
        /// </summary>
        /// <param name="viewport">视口对象</param>
        /// <returns>移动向量矩阵</returns>
        public static Matrix3d GetTransformVectorToPaperSpace(this Viewport viewport)
        {
            //缩放矩阵
            Matrix3d transformScale = viewport.GetTransformScaleToPaperSpace();

            //视口中心点在PS的WCS坐标点,与用户坐标系无关
            var centerPoint = viewport.CenterPoint;

            //视口中心点在MS中的对应点的WCS坐标点,与用户坐标系无关
            //这个ViewTarget不知道是什么，不是对应的点，应该是ViewCenter
            //var viewTarget = viewport.ViewTarget;
            var viewCenter = viewport.ViewCenter;
            Point3d viewCenter3d = new Point3d(viewCenter.X, viewCenter.Y, centerPoint.Z);

            Vector3d vector = viewCenter3d.TransformBy(transformScale)
              .GetVectorTo(centerPoint);


            Matrix3d transformVector = Matrix3d.Displacement(vector);

            return transformVector;
        }



        /// <summary>
        ///获取图纸空间的点到模型空间的点的缩放矩阵
        /// </summary>
        /// <param name="viewport">视口对象</param>
        /// <returns>缩放矩阵</returns>
        public static Matrix3d GetTransformScaleToModelSpace(this Viewport viewport)
        {
            //获取图纸空间中视口的自定义比例
            double scale = viewport.CustomScale;

            Matrix3d transformScale = Matrix3d.Scaling(1 / scale, Point3d.Origin);

            return transformScale;
        }





        /// <summary>
        ///获取图纸空间的点到模型空间的点的移动向量矩阵
        /// </summary>
        /// <param name="viewport">视口对象</param>
        /// <returns>移动向量矩阵</returns>
        public static Matrix3d GetTransformVectorToModelSpace(this Viewport viewport)
        {
            //缩放矩阵
            Matrix3d transformScale = viewport.GetTransformScaleToModelSpace();

            //视口中心点在PS的WCS坐标点,与用户坐标系无关
            var centerPoint = viewport.CenterPoint;

            //视口中心点在MS中的对应点的WCS坐标点,与用户坐标系无关
            //var viewTarget = viewport.ViewTarget;

            var viewCenter = viewport.ViewCenter;
            Point3d viewCenter3d = new Point3d(viewCenter.X, viewCenter.Y, centerPoint.Z);


            Vector3d vector = centerPoint.TransformBy(transformScale)
              .GetVectorTo(viewCenter3d);


            Matrix3d transformVector = Matrix3d.Displacement(vector);

            return transformVector;
        }




        /// <summary>
        ///获取模型空间某点在某一视口下的对应的图纸空间中的点坐标
        /// </summary>
        /// <param name="viewport">视口对象</param>
        /// <param name="pointInModelSpace">模型空间中某点的WCS坐标</param>
        /// <returns>相对某视口下对应的图纸空间中的点的WCS坐标</returns>
        public static Point3d GetPointInPaperSpace(this Viewport viewport, Point3d pointInModelSpace)
        {

            //缩放矩阵
            Matrix3d transformScale = viewport.GetTransformScaleToPaperSpace();

            //移动向量矩阵
            Matrix3d transformVector = viewport.GetTransformVectorToPaperSpace();


            Point3d pointInPaperSpace = pointInModelSpace
                .TransformBy(transformScale)
                .TransformBy(transformVector);

            return pointInPaperSpace;
        }


        /// <summary>
        ///获取模型空间中的点列表在某一视口下的对应的图纸空间中的点坐标列表
        /// </summary>
        /// <param name="viewport">视口对象</param>
        /// <param name="pointLstInModelSpace">图纸空间中某点的WCS坐标列表</param>
        /// <returns>相对某视口下对应的图纸空间中的点的WCS坐标列表</returns>
        public static List<Point3d> GetPointLstInPaperSpace(this Viewport viewport, List<Point3d> pointLstInModelSpace)
        {

            //缩放矩阵
            Matrix3d transformScale = viewport.GetTransformScaleToPaperSpace();

            //移动向量矩阵
            Matrix3d transformVector = viewport.GetTransformVectorToPaperSpace();


            List<Point3d> pointLstInPaperSpace = new List<Point3d>();


            foreach (var point in pointLstInModelSpace)
            {

                Point3d pointInPaperSpace = point
                    .TransformBy(transformScale)
                    .TransformBy(transformVector);

                pointLstInPaperSpace.Add(pointInPaperSpace);
            }

            return pointLstInPaperSpace;
        }






        /// <summary>
        ///获取图纸空间某点在某一视口下的对应的模型空间中的点坐标
        /// </summary>
        /// <param name="viewport">视口对象</param>
        /// <param name="pointInPaperSpace">图纸空间中某点的WCS坐标</param>
        /// <returns>相对某视口下对应的模型空间中的点的WCS坐标</returns>
        public static Point3d GetPointInModelSpace(this Viewport viewport, Point3d pointInPaperSpace)
        {

            //缩放矩阵
            Matrix3d transformScale = viewport.GetTransformScaleToModelSpace();

            //移动向量矩阵
            Matrix3d transformVector = viewport.GetTransformVectorToModelSpace();


            Point3d pointInModelSpace = pointInPaperSpace
                .TransformBy(transformScale)
                .TransformBy(transformVector);

            return pointInModelSpace;
        }




        ///// <summary>
        ///// 获取图纸空间中的点在模型空间中的相应点
        ///// </summary>
        ///// <param name="viewport">视图对象</param>
        ///// <param name="pointInPaperSpace">图纸空间中的坐标点</param>
        ///// <returns>模型空间中对应的坐标点</returns>
        //public static Point3d GetPointInModelSpace(this Viewport viewport, Point3d pointInPaperSpace)
        //{
        //    double customScale = viewport.CustomScale;
        //    Point3d centerPoint = viewport.CenterPoint;
        //    Point2d viewCenter = viewport.ViewCenter;

        //    Point3d pointInModelSpace = new Point3d(viewCenter.X - (centerPoint.X - pointInPaperSpace.X) / customScale, viewCenter.Y - (centerPoint.Y - pointInPaperSpace.Y) / customScale, pointInPaperSpace.Z);
        //    return pointInModelSpace;
        //}




        /// <summary>
        ///获取图纸空间中的点列表在某一视口下的对应的模型空间中的点坐标列表
        /// </summary>
        /// <param name="viewport">视口对象</param>
        /// <param name="pointLstInPaperSpace">图纸空间中中的点的WCS坐标列表</param>
        /// <returns>相对某视口下对应的模型空间中的点的WCS坐标列表</returns>
        public static List<Point3d> GetPointLstInModelSpace(this Viewport viewport, List<Point3d> pointLstInPaperSpace)
        {

            //缩放矩阵
            Matrix3d transformScale = viewport.GetTransformScaleToModelSpace();

            //移动向量矩阵
            Matrix3d transformVector = viewport.GetTransformVectorToModelSpace();


            List<Point3d> pointLstInModelSpace = new List<Point3d>();

            foreach (var point in pointLstInPaperSpace)
            {

                Point3d pointInModelSpace = point
                    .TransformBy(transformScale)
                    .TransformBy(transformVector);

                pointLstInModelSpace.Add(pointInModelSpace);

            }

            return pointLstInModelSpace;
        }



        /// <summary>
        /// 获取图纸空间中某一视口在图纸空间中的边界点的WCS坐标列表
        /// </summary>
        /// <param name="viewport">视口对象</param>
        /// <returns>视口在图纸空间中的边界点的WCS坐标列表</returns>
        public static List<Point3d> GetBoundingboxInPaperSpace(this Viewport viewport)
        {
            //返回值
            List<Point3d> pointLstInPaperSpace = new List<Point3d>();

            Point3dCollection psVpPnts = new Point3dCollection();

            viewport.GetGripPoints(psVpPnts, new IntegerCollection(),
                  new IntegerCollection());

            foreach (Point3d item in psVpPnts)
            {
                pointLstInPaperSpace.Add(item);
            }

            //经测试发现，第二个点为对角线上的点，需要转换一下，但是需要确保为三个点及以上

            if (pointLstInPaperSpace.Count < 3)
            {
                return pointLstInPaperSpace;
            }

            //先将第二个点和第三个点交换，确保为框交
            var tmp = pointLstInPaperSpace[1];
            pointLstInPaperSpace[1] = pointLstInPaperSpace[2];
            pointLstInPaperSpace[2] = tmp;

            return pointLstInPaperSpace;

        }




        /// <summary>
        /// 获取图纸空间中某一视口在模型空间中的边界点的WCS坐标列表
        /// </summary>
        /// <param name="viewport">视口对象</param>
        /// <returns>视口在图纸空间中的边界点的WCS坐标列表</returns>
        public static List<Point3d> GetBoundingboxInModelSpace(this Viewport viewport)
        {
            //获取在图纸空间中的边界点列表
            List<Point3d> pointLstInPaperSpace = viewport.GetBoundingboxInPaperSpace();

            //转换为在模型空间中的边界点列表
            List<Point3d> pointLstInModelSpace = viewport.GetPointLstInModelSpace(pointLstInPaperSpace);

            return pointLstInModelSpace;

        }



        /// <summary>
        /// 获取图纸空间中某一视口对应的模型空间的所有实体对象的ObjectId列表
        /// </summary>
        /// <param name="document">文档对象</param>
        /// <param name="viewport">视口对象</param>
        /// <param name="filter">过滤器，如果为null，将不过滤，默认不过滤</param>
        /// <param name="tolerance">误差，如果为负值，将会往里面偏这个距离，如果为正值，将会往外面偏这个距离，为0时，不考虑误差，默认不考虑</param>
        /// <returns>实体对象的ObjectId列表，如果操作有误或没有对象，将返回空的列表</returns>
        public static List<ObjectId> GetEntityIdLstInModelSpace(this Document document, Viewport viewport, SelectionFilter filter = null, double tolerance = 0)
        {

            //返回值
            List<ObjectId> entityIdLst = new List<ObjectId>();

            Editor editor = document.Editor;




            //先获取边界点列表
            List<Point3d> pointLst = viewport.GetBoundingboxInModelSpace();

            //如果没有三个点及以上，就没法构成封闭范围了
            if (pointLst == null || pointLst.Count < 3)
            {
                return entityIdLst;
            }


            List<Point3d> newPointLst = pointLst;
            if (tolerance > 0.1 || tolerance < -0.1) //因为是double值，用0来比较基本都会为true
            {
                newPointLst = GetNewPointLst(pointLst, tolerance);
            }


            Point3dCollection point3DCollection = new Point3dCollection();
            newPointLst.ForEach(x => point3DCollection.Add(x));



            //只能在模型空间中选择,因为下方会设置缩放范围为对象，
            //在图纸空间中不会有该对象，在图纸空间激活的模型空间中缩放到的范围也不一定是理想的效果
            int spaceIndex = viewport.Database.InModelSpaceOrPaperSpace();



            ////切换到模型空间时，会自动在图纸空间当前活动视口中切换，如果视口不是当前视口，会导致选择对象有问题
            ////需要设置为当前活动视口
            //bool isViewportOn = viewport.On;


            if (spaceIndex != 0) //不在 模型空间的模型空间中， 需要切换一下
            {

                //用这个，如果是在图纸空间状态，会激活图纸空间中的当前视口计入模型空间，需要改一下
                //editor.SwitchToModelSpace();

                //切换到模型空间的模型空间
                document.SwitchToSpace();

                //if (!isViewportOn) //不是当前活动视口，需要切换为当前活动视口
                //{
                //    viewport.On = true;
                //}



                //var oldTileMode = Application.GetSystemVariable("TILEMODE");
                //var oldcvPort = Application.GetSystemVariable("cvPort");

                //editor.SwitchToModelSpace();


                //var tileMode = Application.GetSystemVariable("TILEMODE");
                //var cvPort = Application.GetSystemVariable("cvPort");


                ////是否应该先设置当前活动视口，再切换到它的模型空间中
                //int number = viewport.Number;

                //Application.SetSystemVariable("CVPORT", number);



                //var newTileMode = Application.GetSystemVariable("TILEMODE");
                //var newcvPort = Application.GetSystemVariable("cvPort");

            }

            //在视图缩放之前记录当前的视图
            ViewTableRecord currentView = editor.GetCurrentView();


            //缩放到所有对象可见，有时候图形范围太大，反倒会选不出来一些对象，设置到框选范围，因为会自动留一些误差，更为可靠
            //设置视图范围，以便能获取所有对象
            //dynamic acadApp = Application.AcadApplication;
            //acadApp.ZoomExtents();



            //先以边界的点画封闭多段线，再删除
            //ObjectId boundingPolylineId = CreatePolyline(document.Database, point3DCollection, true);
            ObjectId boundingPolylineId = document.Database.AddPolyLine(point3DCollection, true);



            if (boundingPolylineId != ObjectId.Null) //创建成功
            {
                editor.ZoomObject(boundingPolylineId);


                //这里要删除创建的对象

                boundingPolylineId.EraseEntity();

            }



            PromptSelectionResult selectionresult = null;

            if (filter == null) //没有过滤器
            {
                selectionresult = editor.SelectCrossingPolygon(point3DCollection);
            }
            else //有过滤器
            {
                selectionresult = editor.SelectCrossingPolygon(point3DCollection, filter);
            }




            //好像不管用  加事务也不管用
            //恢复视图范围
            editor.SetCurrentView(currentView);


            if (spaceIndex != 0) //说明原来不在模型空间的模型空间中，需要切换到原来的状态
            {
                document.SwitchToSpace(spaceIndex);
            }



            if (selectionresult.Status != PromptStatus.OK) //没有选上，直接返回
            {
                return entityIdLst;
            }

            var objectIdArr = selectionresult.Value.GetObjectIds();

            entityIdLst = new List<ObjectId>(objectIdArr);

            return entityIdLst;
        }



        ///// <summary>
        ///// 创建多段线
        ///// </summary>
        ///// <param name="database">数据库对象</param>
        ///// <param name="polygon">三维点集合</param>
        ///// <param name="isClosed">多段线是否闭合，默认不闭合</param>
        ///// <param name="constantWidth">多段线的宽度，默认不带宽度</param>
        ///// <returns>创建的多段线的ObjectId，如果创建不成功，返回ObjectId.Null</returns>
        //private static ObjectId CreatePolyline(Database database, Point3dCollection polygon, bool isClosed = false, double constantWidth = 0)
        //{

        //    if (polygon.Count < 2)
        //    {
        //        return ObjectId.Null;
        //    }

        //    //声明一个多段线对象
        //    Polyline pLine = new Polyline(polygon.Count);
        //    for (int i = 0; i < polygon.Count; i++)
        //    {
        //        Point2d point2D = new Point2d(polygon[i].X, polygon[i].Y);
        //        pLine.AddVertexAt(i, point2D, 0, 0, 0);
        //    }

        //    //判断是否闭合
        //    if (isClosed)
        //    {
        //        pLine.Closed = isClosed;
        //    }

        //    //设置多段线的线宽
        //    pLine.ConstantWidth = constantWidth;


        //    return database.AddEntity(pLine);

        //}




        /// <summary>
        /// 以误差为距离，点的两个相邻直线的角平分线为方向向量，获取新的点列表
        /// </summary>
        /// <param name="pointLst"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        private static List<Point3d> GetNewPointLst(List<Point3d> pointLst, double tolerance = 100)
        {
            List<Point3d> newPointLst = new List<Point3d>();

            int count = pointLst.Count;
            for (int i = 0; i < count; i++)
            {
                int lastIndex = i - 1;
                int nextIndex = i + 1;
                if (i == 0) //第一个点，上一个点在最后一个点
                {
                    lastIndex = count - 1;
                }
                else if (i == count - 1) //最后一个点 下一个点在第一个点
                {
                    nextIndex = 0;
                }


                Point3d firstLineStartPoint = pointLst[lastIndex];
                Point3d firstLineEndPoint = pointLst[i];
                Point3d secondLineStartPoint = pointLst[nextIndex];
                Point3d secondLineEndPoint = pointLst[i];


                var dividedVector = Vector3dExtension.GetDividedVector(firstLineStartPoint, firstLineEndPoint, secondLineStartPoint, secondLineEndPoint);

                var newPoint = firstLineEndPoint.Add(dividedVector * tolerance);
                newPointLst.Add(newPoint);

            }

            return newPointLst;
        }





        /// <summary>
        /// 在指定视口中冻结指定的图层名称
        /// </summary>
        /// <param name="viewport">视口对象</param>
        /// <param name="layerName">图层名称，大小写不敏感</param>
        /// <returns>如果成功，返回true,否则，返回false</returns>
        public static bool FreezeLayer(this Viewport viewport, string layerName)
        {
            //返回值
            bool isSucceed = false;

            Database database = viewport.Database;

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                try
                {
                    LayerTable layerTable = transaction.GetObject(database.LayerTableId, OpenMode.ForRead) as LayerTable;
                    //图层已经存在的情况
                    if (layerTable.Has(layerName))
                    {
                        ObjectId objectId = layerTable[layerName];

                        //还没有
                        if (!viewport.IsLayerFrozenInViewport(objectId))
                        {
                            List<ObjectId> objectIdLst = new List<ObjectId> { objectId };



                            Viewport tmpViewport = transaction.GetObject(viewport.Id, OpenMode.ForWrite) as Viewport;

                            tmpViewport.FreezeLayersInViewport(objectIdLst.GetEnumerator());


                            isSucceed = true;
                        }

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
        /// <param name="viewport">视口对象</param>
        /// <param name="layerNameLst">图层名称列表，大小写不敏感</param>
        public static void FreezeLayers(this Viewport viewport, List<string> layerNameLst)
        {

            Database database = viewport.Database;

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                try
                {
                    LayerTable layerTable = transaction.GetObject(database.LayerTableId, OpenMode.ForRead) as LayerTable;

                    //获取已经存在的图层
                    List<string> existedLayerNameLst = layerNameLst.FindAll(x => layerTable.Has(x));

                    if (existedLayerNameLst.Count > 0)
                    {
                        List<ObjectId> objectIdLst = new List<ObjectId>();

                        foreach (var layerName in existedLayerNameLst)
                        {
                            ObjectId objectId = layerTable[layerName];

                            //还没有
                            if (!viewport.IsLayerFrozenInViewport(objectId))
                            {
                                objectIdLst.Add(objectId);
                            }
                        }

                        if (objectIdLst.Count > 0)
                        {
                            Viewport tmpViewport = transaction.GetObject(viewport.Id, OpenMode.ForWrite) as Viewport;
                            tmpViewport.FreezeLayersInViewport(objectIdLst.GetEnumerator());
                        }

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
        /// <param name="viewport">视口对象</param>
        /// <param name="layerName">图层名称，大小写不敏感</param>
        /// <returns>如果成功，返回true,否则，返回false</returns>
        public static bool ThawLayer(this Viewport viewport, string layerName)
        {
            //返回值
            bool isSucceed = false;

            Database database = viewport.Database;

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                try
                {
                    LayerTable layerTable = transaction.GetObject(database.LayerTableId, OpenMode.ForRead) as LayerTable;
                    //图层已经存在的情况
                    if (layerTable.Has(layerName))
                    {
                        ObjectId objectId = layerTable[layerName];

                        //已经冻结
                        if (viewport.IsLayerFrozenInViewport(objectId))
                        {
                            List<ObjectId> objectIdLst = new List<ObjectId> { objectId };

                            Viewport tmpViewport = transaction.GetObject(viewport.Id, OpenMode.ForWrite) as Viewport;

                            tmpViewport.ThawLayersInViewport(objectIdLst.GetEnumerator());


                            isSucceed = true;
                        }

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
        /// <param name="viewport">视口对象</param>
        /// <param name="layerNameLst">图层名称列表，大小写不敏感</param>
        public static void ThawLayers(this Viewport viewport, List<string> layerNameLst)
        {

            Database database = viewport.Database;

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                try
                {
                    LayerTable layerTable = transaction.GetObject(database.LayerTableId, OpenMode.ForRead) as LayerTable;

                    //获取已经存在的图层
                    List<string> existedLayerNameLst = layerNameLst.FindAll(x => layerTable.Has(x));

                    if (existedLayerNameLst.Count > 0)
                    {
                        List<ObjectId> objectIdLst = new List<ObjectId>();

                        foreach (var layerName in existedLayerNameLst)
                        {
                            ObjectId objectId = layerTable[layerName];

                            if (viewport.IsLayerFrozenInViewport(objectId))
                            {
                                objectIdLst.Add(objectId);
                            }
                        }

                        if (objectIdLst.Count > 0)
                        {
                            Viewport tmpViewport = transaction.GetObject(viewport.Id, OpenMode.ForWrite) as Viewport;
                            tmpViewport.ThawLayersInViewport(objectIdLst.GetEnumerator());
                        }

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
        /// 设置视口的尺寸为给定的区域范围 Sets the size of the viewport according to the provided extents.
        /// </summary>
        /// <param name="vp">视口对象</param>
        /// <param name="ext">视图的范围</param>
        /// <param name="fac">放大或缩小的倍数</param>
        public static void ResizeViewport(this Viewport vp, Extents2d ext, double fac = 1.0)
        {
            vp.Width = (ext.MaxPoint.X - ext.MinPoint.X) * fac;
            vp.Height = (ext.MaxPoint.Y - ext.MinPoint.Y) * fac;
            vp.CenterPoint =(Point2d.Origin + (ext.MaxPoint - ext.MinPoint) * 0.5).Pad();
        }





        /// <summary>
        ///将视口设置为指定模型的范围
        /// </summary>
        /// <param name="vp">视口对象</param>
        /// <param name="ext">指定内容的范围</param>
        /// <param name="fac">放大或缩小的倍数</param>
        public static void FitContentToViewport(this Viewport vp, Extents3d ext, double fac = 1.0)
        {
            // Let's zoom to just larger than the extents
            vp.ViewCenter =(ext.MinPoint + ((ext.MaxPoint - ext.MinPoint) * 0.5)).Strip();

            // Get the dimensions of our view from the database extents
            var hgt = ext.MaxPoint.Y - ext.MinPoint.Y;
            var wid = ext.MaxPoint.X - ext.MinPoint.X;

            // We'll compare with the aspect ratio of the viewport itself
            // (which is derived from the page size)
            var aspect = vp.Width / vp.Height;

            // If our content is wider than the aspect ratio, make sure we
            // set the proposed height to be larger to accommodate the
            // content

            if (wid / hgt > aspect)
            {
                hgt = wid / aspect;
            }

            // Set the height so we're exactly at the extents
            vp.ViewHeight = hgt;

            // Set a custom scale to zoom out slightly (could also
            // vp.ViewHeight *= 1.1, for instance)
            vp.CustomScale *= fac;

        }




        /// <summary>
        /// 获取视口的宽度
        /// </summary>
        /// <param name="viewport">视口对象</param>
        /// <returns>视口的宽度</returns>
        public static double GetWidth(this Viewport viewport)
        {
            return viewport.Width;
        }


        /// <summary>
        /// 获取视口的高度
        /// </summary>
        /// <param name="viewport">视口对象</param>
        /// <returns>视口的高度</returns>
        public static double GetHeight(this Viewport viewport)
        {
            return viewport.Height;
        }


        /// <summary>
        /// 获取视口的自定义比例
        /// </summary>
        /// <param name="viewport">视口对象</param>
        /// <returns>视口的自定义比例</returns>
        public static double GetCustomScale(this Viewport viewport)
        {
            return viewport.CustomScale;
        }



        /// <summary>
        /// 获取视口对应的模型空间的中心点坐标
        /// </summary>
        /// <param name="viewport">视口对象</param>
        /// <returns>视口的对应的模型空间的中心点坐标</returns>
        public static Point2d GetViewCenter(this Viewport viewport)
        {
            return viewport.ViewCenter;
        }



        /// <summary>
        /// 设置视口的视图中心点
        /// </summary>
        /// <param name="viewport">视口对象</param>
        /// <param name="viewCenter">视图中心点</param>
        public static void SetViewCenter(this Viewport viewport, Point2d viewCenter)
        {
            using (Transaction transaction = viewport.Database.TransactionManager.StartTransaction())
            {
                try
                {

                Viewport vp = transaction.GetObject(viewport.Id, OpenMode.ForWrite, true) as Viewport;
                vp.ViewCenter = viewCenter;
                transaction.Commit();
                }
                catch
                {
                    transaction.Abort();
                }
            }
        }



        /// <summary>
        /// 设置视口的自定义比例
        /// </summary>
        /// <param name="viewport">视口对象</param>
        /// <param name="customScale">自定义比例</param>
        public  static void SetCustomScale(this Viewport viewport, double customScale)
        {
            using (Transaction transaction = viewport.Database.TransactionManager.StartTransaction())
            {
                Viewport vp = transaction.GetObject(viewport.Id, OpenMode.ForWrite, true) as Viewport;
                vp.CustomScale = customScale;
                transaction.Commit();
            }
        }




        /// <summary>
        /// 旋转视口对象
        /// </summary>
        /// <param name="viewport">视口对象</param>
        /// <param name="rotateAngle">旋转角度，弧度制</param>
        public static void RotateViewPort(this Viewport viewport, double rotateAngle)
        {

            ObjectId viewportId = viewport.ObjectId;


            if (!viewportId.IsValid)
            {
                return;
            }



            using (Transaction tr = viewport.Database.TransactionManager.StartTransaction())
            {
                Viewport vp = tr.GetObject(viewportId, OpenMode.ForWrite) as Viewport;

                if (vp.NonRectClipEntityId.IsNull)
                {
                    //说明视口是矩形视口
                    ObjectId plId = ObjectId.Null;
                    using (BlockTableRecord btr = tr.GetObject(vp.BlockId, OpenMode.ForWrite) as BlockTableRecord)
                    {
                        Point3d centerPoint = vp.CenterPoint;
                        double width = vp.Width;
                        double height = vp.Height;

                        //生成视口裁剪区域
                        Point2d ptLeftBottom = new Point2d(centerPoint.X - width * 0.5, centerPoint.Y - height * 0.5);
                        Point2d ptRightBottom = new Point2d(centerPoint.X + width * 0.5, centerPoint.Y - height * 0.5);
                        Point2d ptRightTop = new Point2d(centerPoint.X + width * 0.5, centerPoint.Y + height * 0.5);
                        Point2d ptLeftTop = new Point2d(centerPoint.X - width * 0.5, centerPoint.Y + height * 0.5);
                        Polyline pl = new Polyline();
                        pl.AddVertexAt(0, ptLeftBottom, 0, 0, 0);
                        pl.AddVertexAt(1, ptRightBottom, 0, 0, 0);
                        pl.AddVertexAt(2, ptRightTop, 0, 0, 0);
                        pl.AddVertexAt(3, ptLeftTop, 0, 0, 0);
                        pl.Closed = true;

                        plId = btr.AppendEntity(pl);
                        tr.AddNewlyCreatedDBObject(pl, true);
                    }
                    vp.NonRectClipEntityId = plId;
                    vp.NonRectClipOn = true;
                }

                vp.TwistAngle += rotateAngle;
                vp.ViewCenter = vp.ViewCenter.RotateBy(rotateAngle, Point2d.Origin);
                Entity ent = tr.GetObject(vp.NonRectClipEntityId, OpenMode.ForWrite) as Entity;
                ent.TransformBy(Matrix3d.Rotation(rotateAngle, Vector3d.ZAxis, vp.CenterPoint));

                tr.Commit();
            }
        }



      



        /// <summary>
        /// 获取在图纸空间中的视口的中心点
        /// </summary>
        /// <param name="viewport">视口对象</param>
        /// <returns>中心点坐标</returns>
        public static Point3d GetCenterPoint(this Viewport viewport)
        {
            return viewport.CenterPoint;
        }



        /// <summary>
        /// 获取视口左下方的坐标点
        /// </summary>
        /// <param name="viewport"></param>
        /// <returns></returns>
        public static Point3d GetLeftBottomPoint(this Viewport viewport)
        {
            Point3d centerPoint = viewport.CenterPoint;
            double width = viewport.Width;
            double height = viewport.Height;
            return new Point3d(centerPoint.X - width / 2, centerPoint.Y - height / 2, centerPoint.Z);
        }




    }
}









