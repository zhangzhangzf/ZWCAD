using ZWCAD.BaseTools.Extension;
using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.Geometry;
using System;
using System.Collections.Generic;

namespace ZWCAD.BaseTools
{
    /// <summary>
    /// 多段线工具
    /// </summary>
    public class PolylineTool
    {

        Document m_document;
        Database m_database;


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="document">文档对象</param>
        public PolylineTool(Document document)
        {
            m_document = document;
            m_database = m_document.Database;

        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="database">图形数据库</param>
        public PolylineTool(Database database)
        {
            m_database = database;

        }







        /// <summary>
        /// 由三维点数组创建多段线
        /// </summary>
        /// <param name="spaceId">空间对象的OjectId</param>
        /// <param name="point3DLst">三维点数组</param>
        /// <param name="isClosed">是否封闭，默认不封闭</param>
        /// <param name="constantWidth">宽度，默认为0，即没有宽度</param>
        /// <returns>创建的多段线的ObjectId，如果创建不成功，返回ObjectId.Null</returns>
        public ObjectId CreatePolyline(ObjectId spaceId, List<Point3d> point3DLst, bool isClosed = false, double constantWidth = 0)
        {
            if (point3DLst == null || point3DLst.Count <= 1)
            {
                return ObjectId.Null;
            }

            Point3dCollection collection = new Point3dCollection(point3DLst.ToArray());

            return CreatePolyline(spaceId, collection, isClosed, constantWidth);

        }



        /// <summary>
        /// 由三维点数组创建多段线
        /// </summary>
        /// <param name="point3DLst">三维点数组</param>
        /// <param name="isClosed">是否封闭，默认不封闭</param>
        /// <param name="constantWidth">宽度，默认为0，即没有宽度</param>
        /// <param name="spaceName">空间名称，默认模型空间</param>
        /// <returns>创建的多段线的ObjectId，如果创建不成功，返回ObjectId.Null</returns>
        public ObjectId CreatePolyline(List<Point3d> point3DLst, bool isClosed = false, double constantWidth = 0, string spaceName = "MODELSPACE")
        {
            if (point3DLst == null || point3DLst.Count <= 1)
            {
                return ObjectId.Null;
            }

            Point3dCollection collection = new Point3dCollection(point3DLst.ToArray());

            return CreatePolyline(collection, isClosed, constantWidth, spaceName);

        }








        /// <summary>
        /// 创建多根多段线
        /// </summary>
        /// <param name="point3DLst">三维点列表</param>
        /// <param name="isClosed">是否封闭，默认不封闭</param>
        /// <param name="constantWidth">宽度，默认为0，即没有宽度</param>
        /// <param name="spaceName">空间名称，默认模型空间</param>
        /// <returns>创建的多段线的ObjectId列表，如果创建不成功，返回空的列表</returns>
        public List<ObjectId> CreatePolylines(List<List<Point3d>> point3DLst, bool isClosed = false, double constantWidth = 0, string spaceName = "MODELSPACE")
        {

            //返回值
            List<ObjectId> objectIdLst = new List<ObjectId>();


            if (point3DLst == null || point3DLst.Count == 0)
            {
                return objectIdLst;
            }

            List<Point3dCollection> point3DCollectionLst = new List<Point3dCollection>();

            foreach (var item in point3DLst)
            {

                if (item == null || item.Count <= 1)
                {
                    continue;
                }

                Point3dCollection collection = new Point3dCollection(item.ToArray());

                point3DCollectionLst.Add(collection);
            }

            if (point3DCollectionLst.Count == 0)
            {
                return objectIdLst;
            }


            using (DocumentLock documentLock = m_document.LockDocument())
            {
                objectIdLst= CreatePolylines(point3DCollectionLst, isClosed, constantWidth, spaceName);
            }

            return objectIdLst;

        }






        /// <summary>
        /// 创建多根多段线
        /// </summary>
        /// <param name="spaceId">空间对象的OjectId</param>
        /// <param name="point3DLst">三维点列表</param>
        /// <param name="isClosed">是否封闭，默认不封闭</param>
        /// <param name="constantWidth">宽度，默认为0，即没有宽度</param>
        /// <returns>创建的多段线的ObjectId列表，如果创建不成功，返回空的列表</returns>
        public List<ObjectId> CreatePolylines(ObjectId spaceId, List<List<Point3d>> point3DLst, bool isClosed = false, double constantWidth = 0)
        {

            //返回值
            List<ObjectId> objectIdLst = new List<ObjectId>();


            if (point3DLst == null || point3DLst.Count == 0)
            {
                return objectIdLst;
            }

            List<Point3dCollection> point3DCollectionLst = new List<Point3dCollection>();

            foreach (var item in point3DLst)
            {

                if (item == null || item.Count <= 1)
                {
                    continue;
                }

                Point3dCollection collection = new Point3dCollection(item.ToArray());

                point3DCollectionLst.Add(collection);
            }

            if (point3DCollectionLst.Count == 0)
            {
                return objectIdLst;
            }



            return CreatePolylines(spaceId, point3DCollectionLst, isClosed, constantWidth);

        }












        /// <summary>
        /// 由三维点数组创建多段线
        /// </summary>
        /// <param name="point3Ds">三维点数组</param>
        /// <param name="isClosed">是否封闭，默认不封闭</param>
        /// <param name="constantWidth">宽度，默认为0，即没有宽度</param>
        /// <param name="spaceName">空间名称，默认模型空间</param>
        /// <returns>创建的多段线的ObjectId，如果创建不成功，返回ObjectId.Null</returns>
        public ObjectId CreatePolyline(Point3d[] point3Ds, bool isClosed = false, double constantWidth = 0, string spaceName = "MODELSPACE")
        {
            if (point3Ds == null || point3Ds.Length <= 1)
            {
                return ObjectId.Null;
            }

            Point3dCollection collection = new Point3dCollection(point3Ds);

            return CreatePolyline(collection, isClosed, constantWidth, spaceName);

        }



        /// <summary>
        /// 由三维点数组创建多段线
        /// </summary>
        /// <param name="spaceId">空间对象的OjectId</param>
        /// <param name="point3Ds">三维点数组</param>
        /// <param name="isClosed">是否封闭，默认不封闭</param>
        /// <param name="constantWidth">宽度，默认为0，即没有宽度</param>
        /// <returns>创建的多段线的ObjectId，如果创建不成功，返回ObjectId.Null</returns>
        public ObjectId CreatePolyline(ObjectId spaceId, Point3d[] point3Ds, bool isClosed = false, double constantWidth = 0)
        {
            if (point3Ds == null || point3Ds.Length <= 1)
            {
                return ObjectId.Null;
            }

            Point3dCollection collection = new Point3dCollection(point3Ds);

            return CreatePolyline(spaceId, collection, isClosed, constantWidth);

        }







        /// <summary>
        /// 创建多段线
        /// </summary>
        /// <param name="polygon">三维点集合</param>
        /// <param name="isClosed">多段线是否闭合，默认不闭合</param>
        /// <param name="constantWidth">多段线的宽度，默认不带宽度</param>
        /// <param name="spaceName">空间名称，默认模型空间</param>
        /// <returns>创建的多段线的ObjectId，如果创建不成功，返回ObjectId.Null</returns>
        public ObjectId CreatePolyline(Point3dCollection polygon, bool isClosed = false, double constantWidth = 0, string spaceName = "MODELSPACE")
        {

            Database database = m_database;

            if (polygon.Count < 2)
            {
                return ObjectId.Null;
            }

            //声明一个多段线对象
            Polyline pLine = new Polyline(polygon.Count);
            for (int i = 0; i < polygon.Count; i++)
            {
                Point2d point2D = new Point2d(polygon[i].X, polygon[i].Y);
                pLine.AddVertexAt(i, point2D, 0, 0, 0);
            }

            //判断是否闭合
            if (isClosed)
            {
                pLine.Closed = isClosed;
            }

            //设置多段线的线宽
            pLine.ConstantWidth = constantWidth;


            return database.AddEntity(pLine, spaceName);

        }



        /// <summary>
        /// 创建多段线
        /// </summary>
        /// <param name="spaceId">空间对象的OjectId</param>
        /// <param name="polygon">三维点集合</param>
        /// <param name="isClosed">多段线是否闭合，默认不闭合</param>
        /// <param name="constantWidth">多段线的宽度，默认不带宽度</param>
        /// <returns>创建的多段线的ObjectId，如果创建不成功，返回ObjectId.Null</returns>
        public ObjectId CreatePolyline(ObjectId spaceId, Point3dCollection polygon, bool isClosed = false, double constantWidth = 0)
        {
            if (polygon.Count < 2)
            {
                return ObjectId.Null;
            }

            //声明一个多段线对象
            Polyline pLine = new Polyline(polygon.Count);
            for (int i = 0; i < polygon.Count; i++)
            {
                Point2d point2D = new Point2d(polygon[i].X, polygon[i].Y);
                pLine.AddVertexAt(i, point2D, 0, 0, 0);
            }

            //判断是否闭合
            if (isClosed)
            {
                pLine.Closed = isClosed;
            }

            //设置多段线的线宽
            pLine.ConstantWidth = constantWidth;


            return m_database.AddEntity(spaceId, pLine);

        }










        /// <summary>
        /// 创建多段线列表
        /// </summary>
        /// <param name="point3dCollectionLst">三维点集合列表</param>
        /// <param name="isClosed">多段线是否闭合，默认不闭合</param>
        /// <param name="constantWidth">多段线的宽度，默认不带宽度</param>
        /// <param name="spaceName">空间名称，默认模型空间</param>
        /// <returns>创建的多段线的ObjectId列表，如果创建不成功，返回空的列表</returns>
        public List<ObjectId> CreatePolylines(List<Point3dCollection> point3dCollectionLst, bool isClosed = false, double constantWidth = 0, string spaceName = "MODELSPACE")
        {

            //返回值
            List<ObjectId> objectIdLst = new List<ObjectId>();

            List<Entity> polylineLst = new List<Entity>();

            foreach (var polygon in point3dCollectionLst)
            {

                if (polygon.Count < 2)
                {
                    continue;
                }

                //声明一个多段线对象
                Polyline pLine = new Polyline(polygon.Count);
                for (int i = 0; i < polygon.Count; i++)
                {
                    Point2d point2D = new Point2d(polygon[i].X, polygon[i].Y);
                    pLine.AddVertexAt(i, point2D, 0, 0, 0);
                }

                //判断是否闭合
                if (isClosed)
                {
                    pLine.Closed = isClosed;
                }

                //设置多段线的线宽
                pLine.ConstantWidth = constantWidth;

                polylineLst.Add(pLine);

            }


            if (polylineLst.Count == 0)
            {
                return objectIdLst;
            }


            return m_database.AddEntities(polylineLst, spaceName);

        }


        /// <summary>
        /// 创建多段线列表
        /// </summary>
        /// <param name="spaceId">空间对象的OjectId</param>
        /// <param name="point3dCollectionLst">三维点集合列表</param>
        /// <param name="isClosed">多段线是否闭合，默认不闭合</param>
        /// <param name="constantWidth">多段线的宽度，默认不带宽度</param>
        /// <returns>创建的多段线的ObjectId列表，如果创建不成功，返回空的列表</returns>
        public List<ObjectId> CreatePolylines(ObjectId spaceId, List<Point3dCollection> point3dCollectionLst, bool isClosed = false, double constantWidth = 0)
        {
            //返回值
            List<ObjectId> objectIdLst = new List<ObjectId>();

            List<Entity> polylineLst = new List<Entity>();

            foreach (var polygon in point3dCollectionLst)
            {

                if (polygon.Count < 2)
                {
                    continue;
                }

                //声明一个多段线对象
                Polyline pLine = new Polyline(polygon.Count);
                for (int i = 0; i < polygon.Count; i++)
                {
                    Point2d point2D = new Point2d(polygon[i].X, polygon[i].Y);
                    pLine.AddVertexAt(i, point2D, 0, 0, 0);
                }

                //判断是否闭合
                if (isClosed)
                {
                    pLine.Closed = isClosed;
                }

                //设置多段线的线宽
                pLine.ConstantWidth = constantWidth;

                polylineLst.Add(pLine);
            }

            if (polylineLst.Count == 0)
            {
                return objectIdLst;
            }
            return m_database.AddEntities(polylineLst, spaceId);
        }









        /// <summary>
        /// 由二维点集合创建多段线
        /// </summary>
        /// <param name="polygon">二维点集合</param>
        /// <param name="isClosed">是否封闭，默认不封闭</param>
        /// <param name="constantWidth">宽度，默认为0，即没有宽度</param>
        /// <param name="spaceName">空间名称，默认模型空间</param>
        /// <returns>创建的多段线的ObjectId，如果创建不成功，返回ObjectId.Null</returns>
        public ObjectId CreatePolyline(Point2dCollection polygon, bool isClosed = false, double constantWidth = 0, string spaceName = "MODELSPACE")
        {

            Database database = m_database;


            if (polygon.Count < 2)
            {
                return ObjectId.Null;
            }

            //声明一个多段线对象
            Polyline pLine = new Polyline(polygon.Count);
            for (int i = 0; i < polygon.Count; i++)
            {
                Point2d point2D = new Point2d(polygon[i].X, polygon[i].Y);
                pLine.AddVertexAt(i, point2D, 0, 0, 0);
            }

            //判断是否闭合
            if (isClosed)
            {
                pLine.Closed = isClosed;
            }

            //设置多段线的线宽
            pLine.ConstantWidth = constantWidth;

            return database.AddEntity(pLine, spaceName);

        }


        /// <summary>
        /// 由二维点集合创建多段线
        /// </summary>
        /// <param name="spaceId">空间对象的OjectId</param>
        /// <param name="polygon">二维点集合</param>
        /// <param name="isClosed">是否封闭，默认不封闭</param>
        /// <param name="constantWidth">宽度，默认为0，即没有宽度</param>
        /// <returns>创建的多段线的ObjectId，如果创建不成功，返回ObjectId.Null</returns>
        public ObjectId CreatePolyline(ObjectId spaceId, Point2dCollection polygon, bool isClosed = false, double constantWidth = 0)
        {

            Database database = m_database;


            if (polygon.Count < 2)
            {
                return ObjectId.Null;
            }

            //声明一个多段线对象
            Polyline pLine = new Polyline(polygon.Count);
            for (int i = 0; i < polygon.Count; i++)
            {
                Point2d point2D = new Point2d(polygon[i].X, polygon[i].Y);
                pLine.AddVertexAt(i, point2D, 0, 0, 0);
            }

            //判断是否闭合
            if (isClosed)
            {
                pLine.Closed = isClosed;
            }

            //设置多段线的线宽
            pLine.ConstantWidth = constantWidth;

            return database.AddEntity(spaceId, pLine);
        }

        /// <summary>
        /// 计算给定中心点坐标的圆弧上第一个点和第二个点之间圆弧的凸度。凸度:在多段线中，用一个顶点与下一个顶点所形成的圆弧角度的四分之一的正切值表示。0表示直线，1表示半圆
        /// </summary>
        /// <param name="firstPoint">圆弧上第一个点坐标</param>
        /// <param name="centerPoint">第一个点和第二个点之间圆弧的中心点坐标</param>
        /// <param name="secondPoint">圆弧上第二个点坐标</param>
        /// <param name="getSmall">获取角度小的部分，也就是当夹角大于pi时，取2*pi-角度的部分</param>
        /// <returns>凸度</returns>
        public double GetBulge(Point3d firstPoint, Point3d centerPoint, Point3d secondPoint, bool getSmall = false)
        {
            // convert points to 2d points
            var plane = new Plane();
            var p1 = firstPoint.Convert2d(plane);
            var p2 = centerPoint.Convert2d(plane);
            var p3 = secondPoint.Convert2d(plane);

            // compute the bulge of the second segment
            var angle1 = p2.GetVectorTo(p1).Angle;
            var angle2 = p2.GetVectorTo(p3).Angle;

            var angleBetween = angle2-angle1;
            if (getSmall)
            {
                if (Math.Abs(angleBetween)>Math.PI)
                {
                    angleBetween =2* Math.PI-Math.Abs(angleBetween);
                }
            }
            var bulge = Math.Tan(angleBetween/4);
            return bulge;
        }


        /// <summary>
        /// 第一个点和第二个点之间为圆弧，其它为线段的多段线
        /// </summary>
        /// <param name="firstPoint">圆弧上第一个点坐标</param>
        /// <param name="centerPoint">第一个点和第二个点之间圆弧的中心点坐标</param>
        /// <param name="secondPoint">圆弧上第二个点坐标</param>
        /// <param name="otherPointLst">第二个点之后的坐标点列表</param>
        /// <param name="spaceName">空间名称，默认模型空间</param>
        /// <returns>创建的多段线的ObjectId</returns>
        public ObjectId CreatePolylineWithArc(Point3d firstPoint, Point3d centerPoint, Point3d secondPoint, List<Point3d> otherPointLst, string spaceName = "MODELSPACE")
        {
            Database database = m_database;

            //先获取凸度
            var bulge = GetBulge(firstPoint, centerPoint, secondPoint, true);

            //声明一个多段线对象
            Polyline pLine = new Polyline();

            // convert points to 2d points
            var plane = new Plane();
            var p1 = firstPoint.Convert2d(plane);
            var p3 = secondPoint.Convert2d(plane);
            pLine.AddVertexAt(0, p1, bulge, 0.0, 0.0);
            pLine.AddVertexAt(1, p3, 0.0, 0.0, 0.0);

            int index = 2;
            foreach (var item in otherPointLst)
            {
                var point2d = item.Convert2d(plane);
                pLine.AddVertexAt(index++, point2d, 0.0, 0.0, 0.0);
            }
            pLine.Closed = true;
            return database.AddEntity(pLine, spaceName);
        }

        /// <summary>
        /// 设置多根多段线的线宽
        /// </summary>
        /// <param name="objectIds">多段线的ObjectId列表，判断是否为多段线</param>
        /// <param name="constantWidth">多段线的宽度，默认不带宽度</param>
        /// <returns></returns>
        public void SetConstantWidth(ObjectId[] objectIds, double constantWidth = 0)
        {

            Database database = m_database;

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {

                foreach (var item in objectIds)
                {

                    //打开块表记录
                    if (transaction.GetObject(item, OpenMode.ForWrite) is Polyline polyline)
                    {
                        //设置多段线的线宽
                        polyline.ConstantWidth = constantWidth;
                    }
                }

                transaction.Commit();
            }

        }



        /// <summary>
        /// 设置多根多段线的线宽
        /// </summary>
        /// <param name="objectIdLst">多段线的ObjectId列表，判断是否为多段线</param>
        /// <param name="constantWidth">多段线的宽度，默认不带宽度</param>
        /// <returns></returns>
        public void SetConstantWidth(List<ObjectId> objectIdLst, double constantWidth = 0)
        {
            ObjectId[] objectIds = objectIdLst.ToArray();
            SetConstantWidth(objectIds, constantWidth);
        }

        /// <summary>
        /// 由二维点数组创建多段线
        /// </summary>
        /// <param name="point2Ds">二维点数组</param>
        /// <param name="isClosed">是否封闭，默认不封闭</param>
        /// <param name="constantWidth">宽度，默认为0，即没有宽度</param>
        /// <returns>创建的多段线的ObjectId，如果创建不成功，返回ObjectId.Null</returns>
        public ObjectId CreatePolyline(Point2d[] point2Ds, bool isClosed = false, double constantWidth = 0)
        {
            Point2dCollection collection = new Point2dCollection(point2Ds);
            return CreatePolyline(collection, isClosed, constantWidth);
        }


        /// <summary>
        /// 获取多段线的顶点二维坐标列表，判断是否为多段线，如果不是，返回空的列表
        /// </summary>
        /// <param name="objectId">多段线对象的ObjectId</param>
        /// <returns>顶点坐标列表，如果没有找到，返回空的列表</returns>
        public List<Point2d> GetPolylinePoint2dLst(ObjectId objectId)
        {
            //返回值
            List<Point2d> pointLst = new List<Point2d>();

            Database database = objectId.Database;

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                DBObject dBObject = transaction.GetObject(objectId, OpenMode.ForRead);
                if (dBObject is Polyline polyline)
                {
                    pointLst = polyline.GetPolylinePoint2dLst();
                }
                transaction.Commit();
            }

            return pointLst;
        }



        /// <summary>
        /// 获取多段线的在指定顶点上的凸度
        /// </summary>
        /// <param name="objectId">多段线对象的ObjectId</param>
        /// <param name="index">顶点索引，第一个点为0</param>
        /// <returns>凸度，如果失败，返回0</returns>
        public double GetBulge(ObjectId objectId, int index = 0)
        {
            //返回值
            double bulge = 0;
            if (objectId.IsNull)
            {
                return bulge;
            }
            Database database = objectId.Database;
            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                try
                {
                    DBObject dBObject = transaction.GetObject(objectId, OpenMode.ForRead);
                    if (dBObject is Polyline polyline)
                    {
                        bulge = polyline.GetBulgeAt(index);
                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();
                }
            }
            return bulge;
        }


        /// <summary>
        /// 获取多段线的在指定顶点上的凸度
        /// </summary>
        /// <param name="objectId">多段线对象的ObjectId</param>
        /// <param name="index">顶点索引，第一个点为0</param>
        /// <returns>凸度，如果失败，返回0</returns>
        public double SetBulge(ObjectId objectId, int index = 0)
        {
            //返回值
            double bulge = 0;
            if (objectId.IsNull)
            {
                return bulge;
            }
            Database database = objectId.Database;
            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                try
                {
                    DBObject dBObject = transaction.GetObject(objectId, OpenMode.ForRead);
                    if (dBObject is Polyline polyline)
                    {
                        bulge = polyline.GetBulgeAt(index);
                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();
                }
            }
            return bulge;
        }









        /// <summary>
        /// 添加指定顶点
        /// </summary>
        /// <param name="objectId">多段线对象的ObjectId</param>
        /// <param name="point">指定的插入点坐标</param>
        /// <param name="extend">如果点在多段线外部，是否延伸</param>
        /// <returns>如果成功，返回true，否则，返回false</returns>
        public bool AddVertex(ObjectId objectId, Point3d point, bool extend = false)
        {
            //返回值
            bool isSucceed = false;
            if (objectId.IsNull)
            {
                return isSucceed;
            }

            double tolerance = 1E-6;

            Database database = objectId.Database;
            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                try
                {
                    DBObject dBObject = transaction.GetObject(objectId, OpenMode.ForWrite);
                    if (dBObject is Polyline polyline)
                    {
                        point = polyline.GetClosestPointTo(point, extend);        // point on curve
                        double parameter = polyline.GetParameterAtPoint(point);  // parameter at point
                        int index = (int)parameter;                           // segment index

                        // do not add a new vertex if point is on an existing one
                        if (Math.Abs(parameter - index)>tolerance)
                        {
                            double bulge = polyline.GetBulgeAt(index);               // segment bulge
                            var plane = new Plane(Point3d.Origin, polyline.Normal);  // polyline OCS plane
                            if (bulge <=tolerance) // linear segment
                            {
                                polyline.AddVertexAt(index + 1, point.Convert2d(plane), 0.0, 0.0, 0.0);
                            }
                            else // arc segment
                            {
                                double angle = Math.Atan(bulge);              // quarter of total arc angle
                                double angle1 = angle * (parameter - index);  // quarter of first arc angle
                                double angle2 = angle - angle1;               // quarter of second arc angle
                                                                              // add the new vertex and set it bulge
                                polyline.AddVertexAt(index + 1, point.Convert2d(plane), Math.Tan(angle2), 0.0, 0.0);
                                // set the bulge of the fist arc segment
                                polyline.SetBulgeAt(index, Math.Tan(angle1));
                            }
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
        /// 获取多段线的顶点三维坐标列表，判断是否为多段线，如果不是，返回空的列表
        /// </summary>
        /// <param name="objectId">多段线对象的ObjectId</param>
        /// <returns>顶点坐标列表，如果没有找到，返回空的列表</returns>
        public List<Point3d> GetPolylinePoint3dLst(ObjectId objectId)
        {
            //返回值
            List<Point3d> pointLst = new List<Point3d>();

            Database database = objectId.Database;

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                DBObject dBObject = transaction.GetObject(objectId, OpenMode.ForRead);
                if (dBObject is Polyline polyline)
                {
                    pointLst = polyline.GetPolylinePoint3dLst();
                }
                transaction.Commit();
            }

            return pointLst;

        }




        /// <summary>
        /// 判断多段线是否闭合，如果闭合，或者不闭合，但是收尾点相距在误差之内，也可以认为是闭合的
        /// https://www.keanw.com/2015/08/getting-the-centroid-of-an-autocad-region-using-net.html
        /// https://forums.autodesk.com/t5/net/region-centroid/td-p/3193544
        /// </summary>
        /// <param name="objectId">对象的ObjectId，会自动判断是否为多段线</param>
        /// <param name="tolerance">误差</param>
        /// <returns>如果不是多段线，返回false,如果闭合，返回true，否则，返回false</returns>
        public bool IsClosed(ObjectId objectId, double tolerance = 1E-6)
        {
            //返回值
            bool isSucceed = false;

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    DBObject dBObject = transaction.GetObject(objectId, OpenMode.ForRead);
                    if (dBObject is Polyline polyline)
                    {
                        isSucceed = polyline.IsClosed(tolerance);
                    }
                    else if (dBObject is Polyline3d polyline3d)
                    {
                        isSucceed = polyline3d.IsClosed(tolerance);
                    }
                    else if (dBObject is Polyline2d polyline2d)
                    {
                        isSucceed = polyline2d.IsClosed(tolerance);
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
        /// 获取多多段线的质心 2012版本通过拉伸三维对象去计算，高版本通过创建面域去计算，但不会创建实体的面域
        /// </summary>
        /// <param name="polylineId">多段线的ObjectId</param>
        /// <returns>多段线的质心，如果失败，返回null</returns>
        public Point3d? GetCentroid(ObjectId polylineId)
        {
            //var tid = ObjectId.Null;
            //返回值
            Point3d centroid = Point3d.Origin;

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    if (transaction.GetObject(polylineId, OpenMode.ForRead) is Entity pline)
                    {

                        //Convert entity to in-memory Region.
                        using (DBObjectCollection segments = new DBObjectCollection())
                        {
                            pline.Explode(segments);

                            if (segments.Count == 0)
                            {
                                return null;
                            }

                            DBObjectCollection regions = Region.CreateFromCurves(segments);

                            if (regions.Count == 0)
                            {
                                return null;
                            }

                            //只考虑第一个面域
                            Region region = regions[0] as Region;

#if autoCAD2012 //2013及以下版本，Region没有AreaProperties这个方法，需要创建实体对象去获得

                            using (Solid3d solid = new Solid3d())
                            {
                                solid.Extrude(region, 2.0, 0.0);
                                Point3d solidCentroid = solid.MassProperties.Centroid;
                                centroid = solidCentroid.TransformBy(Matrix3d.Displacement(region.Normal.Negate()));


                            }


#else
                            var cs = pline.GetPlane().GetCoordinateSystem();

                            //to get the centroid of a region lying on the WCS XY plane:

                            var origin = cs.Origin;
                            var xAxis = cs.Xaxis;
                            var yAxis = cs.Yaxis;
                          var  tmpCentroid = region.AreaProperties(ref origin, ref xAxis, ref yAxis).Centroid;
                            var pl = new Plane(origin, xAxis, yAxis);

                            //这一行不知道有什么用？
                            centroid= pl.EvaluatePoint(tmpCentroid);
#endif

                        }
                        transaction.Commit();
                    }
                    else
                    {
                        return null;
                    }


                }
                catch
                {
                }
            }

            return centroid;


        }














    }
}


