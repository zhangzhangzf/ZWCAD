using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.Geometry;
using System.Collections.Generic;

namespace ZWCAD.BaseTools.Extension
{
    /// <summary>
    /// 直线扩展
    /// </summary>
    public static class LineExtension
    {





        /// <summary>
        /// 直线起点的三维坐标,判断是否为直线，如果不是，返回null
        /// </summary>
        /// <param name="objectId">对象的ObjectId</param>
        /// <returns>起点的三维坐标，如果不是直线，返回null</returns>
        public static Point3d? GetStartPoint3d(this ObjectId objectId)
        {

            Database database = objectId.Database;

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {

                DBObject dBObject = transaction.GetObject(objectId, OpenMode.ForRead, true);
                if (dBObject is Line line)
                {
                    Point3d point3d = line.StartPoint;
                    return point3d;

                }
                transaction.Commit();
            }

            return null;

        }



        /// <summary>
        /// 直线终点的三维坐标,判断是否为直线，如果不是，返回null
        /// </summary>
        /// <param name="objectId">对象的ObjectId</param>
        /// <returns>终点的三维坐标，如果不是直线，返回null</returns>
        public static Point3d? GetEndPoint3d(this ObjectId objectId)
        {

            Database database = objectId.Database;

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {

                DBObject dBObject = transaction.GetObject(objectId, OpenMode.ForRead, true);
                if (dBObject is Line line)
                {
                    Point3d point3d = line.EndPoint;
                    return point3d;

                }
                transaction.Commit();
            }

            return null;

        }








        /// <summary>
        /// 起点的二维坐标
        /// </summary>
        /// <param name="line">直线对象</param>
        /// <returns>起点的二维坐标</returns>
        public static Point2d GetStartPoint2d(this Line line)
        {
            Point3d point3d = line.StartPoint;
            Point2d point2d = new Point2d(point3d.X, point3d.Y);

            return point2d;
        }






        /// <summary>
        /// 终点的二维坐标
        /// </summary>
        /// <param name="line">直线对象</param>
        /// <returns>终点的二维坐标</returns>
        public static Point2d GetEndPoint2d(this Line line)
        {
            Point3d point3d = line.EndPoint;
            Point2d point2d = new Point2d(point3d.X, point3d.Y);

            return point2d;
        }






        /// <summary>
        /// 起点的二维坐标,判断是否为直线，如果不是，返回null
        /// </summary>
        /// <param name="objectId">直线对象</param>
        /// <returns>起点的二维坐标，如果不是直线，返回null</returns>
        public static Point2d? GetStartPoint2d(this ObjectId objectId)
        {

            var result = objectId.GetStartPoint3d();

            if (result == null)  //不是直线，直接返回null
            {
                return null;
            }
            else  //是直线
            {
                Point3d point3d = (Point3d)result;
                Point2d point2d = new Point2d(point3d.X, point3d.Y);
                return point2d;
            }

        }


        /// <summary>
        /// 终点的二维坐标,判断是否为直线，如果不是，返回null
        /// </summary>
        /// <param name="objectId">直线对象</param>
        /// <returns>终点的二维坐标，如果不是直线，返回null</returns>
        public static Point2d? GetEndPoint2d(this ObjectId objectId)
        {

            var result = objectId.GetEndPoint3d();

            if (result == null)  //不是直线，直接返回null
            {
                return null;
            }
            else  //是直线
            {
                Point3d point3d = (Point3d)result;
                Point2d point2d = new Point2d(point3d.X, point3d.Y);
                return point2d;
            }

        }






        /// <summary>
        /// 获取直线的起点和终点二维坐标列表，判断是否为直线，如果不是，返回空的列表
        /// </summary>
        /// <param name="objectId">对象的ObjectId</param>
        /// <returns>起点和终点二维坐标列表，如果不是直线，返回空的列表</returns>
        public static List<Point2d> GetPoint2dLst(this ObjectId objectId)
        {
            //返回值
            List<Point2d> pointLst = new List<Point2d>();

            var startPoint2d = objectId.GetStartPoint2d();
            if (startPoint2d == null)  //不是直线，直接返回
            {
                return pointLst;
            }

            var endPoint2d = objectId.GetEndPoint2d();
            if (endPoint2d == null)  //不是直线，直接返回
            {
                return pointLst;
            }

            //以下为直线
            pointLst.Add((Point2d)startPoint2d);
            pointLst.Add((Point2d)endPoint2d);

            return pointLst;

        }






        /// <summary>
        /// 获取直线的起点和终点二维坐标列表
        /// </summary>
        /// <param name="line">直线对象</param>
        /// <returns>起点和终点二维坐标列表</returns>
        public static List<Point2d> GetPoint2dLst(this Line line)
        {
            Point2d startPoint2d = line.GetStartPoint2d();
            Point2d endPoint2d = line.GetEndPoint2d();
            List<Point2d> pointLst = new List<Point2d>
            {
                startPoint2d,
                endPoint2d
            };

            return pointLst;

        }

        
        /// <summary>
        /// 获取直线的起点和终点三维坐标列表
        /// </summary>
        /// <param name="line">直线对象</param>
        /// <returns>起点和终点三维坐标列表</returns>
        public static List<Point3d> GetPoint3dLst(this Line line)
        {
            Point3d startPoint3d = line.StartPoint;
            Point3d endPoint3d = line.EndPoint;
            List<Point3d> pointLst = new List<Point3d>
            {
                startPoint3d,
                endPoint3d
            };

            return pointLst;

        }



    }
}
