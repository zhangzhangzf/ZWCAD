using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;
using ZwSoft.ZwCAD.Geometry;
using System.Collections.Generic;

namespace ZWCAD.BaseTools.Extension
{

    /// <summary>
    ///  视图缩放扩展
    /// </summary>
    public static class ZoomExtension
    {

        /// <summary>
        /// 实现视图的比例缩放
        /// </summary>
        /// <param name="editor"></param>
        /// <param name="scale">缩放比例</param>
        public static void ZoomScaled(this Editor editor, double scale)
        {
            //得到当前视图
            ViewTableRecord view = editor.GetCurrentView();
            //修改视图的宽度和高度
            view.Width /= scale;
            view.Height /= scale;
            //更新当前视图
            editor.SetCurrentView(view);
        }


        /// <summary>
        /// 实现视图的窗口缩放
        /// </summary>
        /// <param name="editor"></param>
        /// <param name="pt1">窗口角点</param>
        /// <param name="pt2">窗口角点</param>
        public static void ZoomWindow(this Editor editor, Point3d pt1, Point3d pt2)
        {
            //创建一临时的直线用于获取两点表示的范围
            using (Line line = new Line(pt1, pt2))
            {
                //获取两点表示的范围
                Extents3d extents = new Extents3d(line.GeometricExtents.MinPoint, line.GeometricExtents.MaxPoint);
                //获取范围内的最小值点及最大值点
                Point2d minPt = new Point2d(extents.MinPoint.X, extents.MinPoint.Y);
                Point2d maxPt = new Point2d(extents.MaxPoint.X, extents.MaxPoint.Y);
                //得到当前视图
                ViewTableRecord view = editor.GetCurrentView();
                //设置视图的中心点、高度和宽度
                view.CenterPoint = minPt + (maxPt - minPt) / 2;
                view.Height = maxPt.Y - minPt.Y;
                view.Width = maxPt.X - minPt.X;
                //更新当前视图
                editor.SetCurrentView(view);
            }
        }






        /// <summary>
        /// 以centerPoint点为中心，半径radius缩放视图
        /// </summary>
        /// <param name="editor"></param>
        /// <param name="centerPoint">中心点</param>
        /// <param name="radius">半径</param>
        public static void ZoomWindow(this Editor editor, Point3d centerPoint, double radius)
        {

            //找出最小点和最大点
            Point3d minPoint = new Point3d(centerPoint.X - radius, centerPoint.Y - radius, centerPoint.Z);
            Point3d maxPoint = new Point3d(centerPoint.X + radius, centerPoint.Y + radius, centerPoint.Z);

            editor.ZoomWindow(minPoint, maxPoint);

        }




        ///// <summary>
        ///// 根据图形边界显示视图
        ///// </summary>
        //public void ZoomExtens()
        //{
        //    Database db = m_editor.Document.Database;
        //    //更新当前模型空间的范围
        //    db.UpdateExt(true);
        //    //根据当前图形的界限范围对视图进行缩放
        //    if (db.Extmax.X < db.Extmin.X)
        //    {
        //        Plane plane = new Plane();
        //        Point3d pt1 = new Point3d(plane, db.Limmin);
        //        Point3d pt2 = new Point3d(plane, db.Limmax);
        //        ZoomWindow(pt1, pt2);
        //    }
        //    else
        //    {
        //        ZoomWindow(db.Extmin, db.Extmax);
        //    }
        //}


        /// <summary>
        /// 根据图形边界显示视图 用这个更简洁
        /// </summary>
        public static void ZoomExtens()
        {
            dynamic acadApp = ZwSoft.ZwCAD.ApplicationServices.Application.ZcadApplication;
            acadApp.ZoomExtents();
        }







        /// <summary>
        /// 根据对象的范围显示视图
        /// </summary>
        /// <param name="editor"></param>
        /// <param name="objectId">实体ID</param>
        public static void ZoomObject(this Editor editor, ObjectId objectId)
        {

            //先判断是否为空值
            if (objectId.IsNull || objectId.IsErased)
            {
                return;
            }


            Database db = editor.Document.Database;



            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                //获取实体对象
                Entity entity = trans.GetObject(objectId, OpenMode.ForRead) as Entity;
                if (entity == null)
                {
                    return;
                }



                //16:25 2021/9/15  有些对象并没有得到相应的视图，还有可能是跟坐标转换有问题，并没有得到想要的视图 原因未知 需要重新考虑



                //17:07 2022 / 6 / 10

                //根据实体的范围对视图进行缩放
                //Extents3d extents3 = entity.GeometricExtents;


                Extents3d? extents3d = null;
                try
                {
                    extents3d = entity.GeometricExtents;
                }
                catch
                {

                    try
                    {
                        if (entity is BlockReference blockReference)
                        {
                            extents3d = blockReference.GeometryExtentsBestFit();
                        }

                    }
                    catch
                    {
                        extents3d = null;
                    }
                }

                if(extents3d == null)
                {
                    return;
                }


                Extents3d extents3 = extents3d.Value;


                extents3.TransformBy(editor.CurrentUserCoordinateSystem.Inverse());
                editor.ZoomWindow(extents3.MinPoint, extents3.MaxPoint);
                trans.Commit();
            }
        }





        /// <summary>
        /// 根据对象的范围显示视图
        /// </summary>
        /// <param name="editor"></param>
        /// <param name="objectIds">实体ID</param>
        public static void ZoomObjects(this Editor editor, List<ObjectId> objectIds)
        {
            Database db = editor.Document.Database;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                //用来放最小点的数组
                Point3d[] minPoints = new Point3d[objectIds.Count];

                //用来放最大点的数组
                Point3d[] maxPoints = new Point3d[objectIds.Count];

                int i = 0;

                foreach (ObjectId objectId in objectIds)
                {
                    //获取实体对象
                    Entity entity = trans.GetObject(objectId, OpenMode.ForRead) as Entity;



                    //17:07 2022 / 6 / 10


                    ////根据实体的范围对视图进行缩放
                    //Extents3d extents3 = entity.GeometricExtents;


                    Extents3d? extents3d = null;
                    try
                    {
                        extents3d = entity.GeometricExtents;
                    }
                    catch
                    {

                        try
                        {
                            if (entity is BlockReference blockReference)
                            {
                                extents3d = blockReference.GeometryExtentsBestFit();
                            }

                        }
                        catch
                        {
                            extents3d = null;
                        }
                    }

                    if (extents3d == null)
                    {
                        return;
                    }


                    Extents3d extents3 = extents3d.Value;


                    extents3.TransformBy(editor.CurrentUserCoordinateSystem.Inverse());

                    minPoints[i] = extents3.MinPoint;
                    maxPoints[i] = extents3.MaxPoint;

                    i++;
                }




                //获取最小的点
                Point3d minimumPoint = FindMinimumPoint(minPoints);

                //获取最大的点
                Point3d maximumPoint = FindMaximumPoint(maxPoints);

                //以最小点和最大点缩放视图
                editor.ZoomWindow(minimumPoint, maximumPoint);
                trans.Commit();
            }
        }






        /// <summary>
        /// 从点数组中分别找出最小的x值、y值和z值，组成最小的点
        /// </summary>
        /// <param name="points">点数组</param>
        /// <returns>最小的点</returns>
        private static Point3d FindMinimumPoint(Point3d[] points)
        {
            double minX = points[0].X;
            double minY = points[0].Y;
            double minZ = points[0].Z;

            foreach (Point3d each in points)
            {
                if (minX > each.X)
                {
                    minX = each.X;
                }
                if (minY > each.Y)
                {
                    minY = each.Y;
                }
                if (minZ > each.Z)
                {
                    minZ = each.Z;
                }

            }

            return new Point3d(minX, minY, minZ);

        }


        /// <summary>
        /// 从点数组中分别找出最大的x值、y值和z值，组成最小的点
        /// </summary>
        /// <param name="points">点列表</param>
        /// <returns>最大的点</returns>
        private static Point3d FindMaximumPoint(Point3d[] points)
        {
            double maxX = points[0].X;
            double maxY = points[0].Y;
            double maxZ = points[0].Z;

            foreach (Point3d each in points)
            {
                if (maxX < each.X)
                {
                    maxX = each.X;
                }
                if (maxY < each.Y)
                {
                    maxY = each.Y;
                }
                if (maxZ < each.Z)
                {
                    maxZ = each.Z;
                }

            }

            return new Point3d(maxX, maxY, maxZ);

        }


    }
}


