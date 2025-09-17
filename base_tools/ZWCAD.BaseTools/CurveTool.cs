using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.Geometry;
using Mrf.CSharp.BaseTools.Extension;
using System.Collections.Generic;
using ZWCAD.BaseTools.Extension;
using System;

namespace ZWCAD.BaseTools
{
    /// <summary>
    /// 曲线类工具
    /// </summary>
    public class CurveTool
    {

        #region Private Variables

        Document m_document;
        Database m_database;

        #endregion



        #region Default Constructor
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="document">文档对象</param>
        public CurveTool(Document document)
        {
            m_document = document;
            m_database=document.Database;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="database">图形数据库</param>
        public CurveTool(Database database)
        {
            m_database = database;
        }
        #endregion




        #region CommandMethods

        /// <summary>
        /// 获取curve对象上从指定起始长度开始，距离指定起始长度的长度对应的点坐标
        /// </summary>
        /// <param name="curveId">curve的ObjectId</param>
        /// <param name="lengthToStartLength">距离指定起始长度的长度</param>
        /// <param name="startLenght">起始长度</param>
        /// <param name="extend">如果长度超过曲线长度，是否延长</param>
        /// <returns>点坐标，如果失败，返回null</returns>
        public Point3d? GetPoint(ObjectId curveId, double lengthToStartLength, double startLenght = 0, bool extend = false)
        {
            double totalLength = startLenght+ lengthToStartLength;
            if (curveId.IsNull || totalLength<0)
            {
                return null;
            }
            var curveLength = GetLength(curveId);
            if (double.IsNaN(curveLength)) return null;

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    if (transaction.GetObject(curveId, OpenMode.ForRead) is Curve curve)
                    {
                        Point3d point;
                        if (totalLength<=curveLength)
                        {
                            point= curve.GetPointAtDist(totalLength);
                        }
                        else
                        {
                            if (extend)
                            {
                                curve.UpgradeOpen();
                                curve.Extend(totalLength);
                                curve.DowngradeOpen();
                                point=curve.GetPointAtDist(totalLength);
                            }
                            else
                            {
                                point=curve.GetPointAtDist(curveLength);
                            }
                        }
                        //因为上面有修改curve的情况，需要取消
                        transaction.Abort();
                        return point;
                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();
                }
            }
            return null;
        }


        /// <summary>
        /// 获取curve对象上从指定点的斜率
        /// </summary>
        /// <param name="curveId">curve的ObjectId</param>
        /// <param name="point">指定点</param>
        /// <param name="extend">如果长度超过曲线长度，是否延长</param>
        /// <returns>点坐标，如果失败，返回null</returns>
        public Vector2d? GetVectorAtPoint(ObjectId curveId, Point3d point, bool extend = false)
        {
            Point3d? tmpPointOnCurve = GetClosestPointOnCurve(curveId, point, extend);
            if (tmpPointOnCurve == null)
            {
                return null;
            }
            Point3d pointOnCurve = tmpPointOnCurve.Value;

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    if (transaction.GetObject(curveId, OpenMode.ForRead) is Curve curve)
                    {
                        var a1 = curve.GetFirstDerivative(pointOnCurve);
                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();
                }
            }
            return null;
        }


        /// <summary>
        /// 获取curve对象上从指定起始长度开始，距离指定起始长度的长度对应的点坐标
        /// </summary>
        /// <param name="curveId">curve的ObjectId</param>
        /// <param name="lengthToCertainPoint">距离指定点的相对长度，如果为负数，则为在指定点的负方向</param>
        /// <param name="certainPoint">指定点</param>
        /// <param name="extendPoint">如果指定点超过曲线长度，是否延长</param>
        /// <param name="extendLength">如果长度超过曲线长度，是否延长</param>
        /// <returns>点坐标，如果失败，返回null</returns>
        public Point3d? GetPoint(ObjectId curveId, double lengthToCertainPoint, Point3d certainPoint, bool extendPoint = false, bool extendLength = false)
        {
            if (curveId.IsNull)
            {
                return null;
            }
            Point3d? tmpStartPoint = GetClosestPointOnCurve(curveId, certainPoint, extendPoint);
            if (tmpStartPoint == null)
            {
                return null;
            }
            certainPoint=tmpStartPoint.Value;
            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    if (transaction.GetObject(curveId, OpenMode.ForRead) is Curve curve)
                    {
                        double startLength = curve.GetDistAtPoint(certainPoint);

                        Point3d? point = GetPoint(curveId, lengthToCertainPoint, startLength, extendLength);

                        transaction.Commit();
                        return point;
                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();
                }
            }
            return null;
        }

        /// <summary>
        /// 判断给定点在参考点所在曲线的负向还是正向
        /// </summary>
        /// <param name="curveId">曲线的ObjectId</param>
        /// <param name="referencePoint">参考点</param>
        /// <param name="point">给定点</param>
        /// <param name="extend">如果点超过曲线长度，是否延长</param>
        /// <returns>-1表示负向，1表示正向，2 表示两点重合，0表示判断有误</returns>
        public int GetPointDirection(ObjectId curveId, Point3d referencePoint, Point3d point, bool extend = false)
        {
            double tolerance = 1E-6;

            //两者相等
            if (point.X.AreEqual(referencePoint.X, tolerance) &&
            point.Y.AreEqual(referencePoint.Y, tolerance) &&
            point.Z.AreEqual(referencePoint.Z, tolerance)
            )
            {
                return 2;
            }

            //返回值
            int direction = 0;

            double referenceValue = GetDistance(curveId, referencePoint, extend);
            if (double.IsNaN(referenceValue))
            {
                return direction;
            }

            double pointValue = GetDistance(curveId, point, extend);
            if (double.IsNaN(pointValue))
            {
                return direction;
            }

            if (referenceValue<pointValue)
            {
                direction = 1;
            }
            else
            {
                direction=-1;
            }
            return direction;
        }

        /// <summary>
        /// 获取curve对象上距离指定点最近的点
        /// </summary>
        /// <param name="curveId">curve的ObjectId</param>
        /// <param name="point">指定点</param>
        /// <param name="extend">如果点超过曲线长度，是否延长</param>
        /// <returns>点坐标，如果失败，返回null</returns>
        public Point3d? GetClosestPointOnCurve(ObjectId curveId, Point3d point, bool extend = false)
        {
            if (curveId.IsNull)
            {
                return null;
            }
            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    if (transaction.GetObject(curveId, OpenMode.ForRead) is Curve curve)
                    {
                        Point3d closestPoint = curve.GetClosestPointTo(point, extend);
                        transaction.Commit();
                        return closestPoint;
                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();
                }
            }
            return null;
        }


        /// <summary>
        /// 获取curve对象上距离参考点一定距离的所有点的列表
        /// </summary>
        /// <param name="curveId">curve的ObjectId</param>
        /// <param name="fromPoint">以参考点作为基点</param>
        /// <param name="distance">距离参考点的距离</param>
        /// <param name="extendCurve">如果点超过曲线长度，是否延长</param>
        /// <returns>所有点坐标列表，如果失败，返回空的列表</returns>
        public List<Point3d> GetPointsByDistance(ObjectId curveId, Point3d fromPoint, double distance, bool extendCurve = false)
        {
            //return value
            List<Point3d> allPointLst = new List<Point3d>();
            if (curveId.IsNull)
            {
                return allPointLst;
            }
            double tolerance = 1e-6;
            if (distance<0 && Math.Abs(distance)>tolerance)
            {
                return allPointLst;
            }
            Point3d? closestPoint = GetClosestPointOnCurve(curveId, fromPoint, extendCurve);
            if (closestPoint==null || Math.Abs(distance)<=tolerance)
            {
                return allPointLst;
            }

            //创建圆对象
            ObjectId circleId = m_database.AddCircle(fromPoint, distance);
            if (circleId.IsNull)
            {
                return allPointLst;
            }
            //计算两者的所有交点

            ObjectTool objectTool = new ObjectTool(m_database);
            Intersect intersectType = extendCurve ? Intersect.ExtendThis : Intersect.OnBothOperands;
            allPointLst = objectTool.GetIntersectWithPoints(curveId, circleId, intersectType);

            //删除圆对象
            objectTool.EraseDBObject(circleId);
            return allPointLst;
        }

        /// <summary>
        /// 获取curve对象上距离参考点一定距离的所有点中距离制定点最近的点
        /// </summary>
        /// <param name="curveId">curve的ObjectId</param>
        /// <param name="fromPoint">以参考点作为基点</param>
        /// <param name="distance">距离参考点的距离</param>
        /// <param name="closedPoint">指定的距离最近的点</param>
        /// <param name="extendCurve">如果点超过曲线长度，是否延长</param>
        /// <returns>距离指定的点最近的点，如果失败，返回null</returns>
        public Point3d? GetClosedPointByDistance(ObjectId curveId, Point3d fromPoint, double distance, Point3d closedPoint, bool extendCurve = false)
        {
            List<Point3d> allPointLst = GetPointsByDistance(curveId, fromPoint, distance, extendCurve);
            if (allPointLst==null)
            {
                return null;
            }
            double minDistance = double.MaxValue;
            Point3d expectedPoint = allPointLst[0];
            foreach (var item in allPointLst)
            {
                double distanceToPoint = closedPoint.DistanceTo(item);
                if (distanceToPoint<minDistance)
                {
                    expectedPoint = item;
                    minDistance = distanceToPoint;
                }
            }
            return expectedPoint;
        }

        /// <summary>
        /// 获取curve对象上指定点的参数
        /// </summary>
        /// <param name="curveId">curve的ObjectId</param>
        /// <param name="point">指定点</param>
        /// <param name="extend">如果点超过曲线长度，是否延长</param>
        /// <returns>参数，如果失败，返回double.NaN</returns>
        public double GetParameter(ObjectId curveId, Point3d point, bool extend = false)
        {
            //返回值
            double value = double.NaN;
            if (curveId.IsNull)
            {
                return value;
            }

            Point3d? tmpClosestPoint = GetClosestPointOnCurve(curveId, point, extend);
            if (!tmpClosestPoint.HasValue)
            {
                return value;
            }
            var closestPoint = tmpClosestPoint.Value;

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    if (transaction.GetObject(curveId, OpenMode.ForRead) is Curve curve)
                    {
                        value = curve.GetParameterAtPoint(closestPoint);
                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();
                }
            }
            return value;
        }


        /// <summary>
        /// 获取curve对象上指定点的距离起点的距离
        /// </summary>
        /// <param name="curveId">curve的ObjectId</param>
        /// <param name="point">指定点</param>
        /// <param name="extend">如果点超过曲线长度，是否延长</param>
        /// <returns>参数，如果失败，返回double.NaN</returns>
        public double GetDistance(ObjectId curveId, Point3d point, bool extend = false)
        {
            //返回值
            double value = double.NaN;
            if (curveId.IsNull)
            {
                return value;
            }

            Point3d? tmpClosestPoint = GetClosestPointOnCurve(curveId, point, extend);
            if (!tmpClosestPoint.HasValue)
            {
                return value;
            }
            var closestPoint = tmpClosestPoint.Value;

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    if (transaction.GetObject(curveId, OpenMode.ForRead) is Curve curve)
                    {
                        value = curve.GetDistAtPoint(closestPoint);
                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();
                }
            }
            return value;
        }

        /// <summary>
        /// 获取曲线对象的长度
        /// </summary>
        /// <param name="curveId">曲线对象的ObjectId</param>
        /// <returns>长度，如果失败，返回double.NaN</returns>
        public double GetLength(ObjectId curveId)
        {
            //返回值
            double length = double.NaN;
            if (curveId.IsNull)
            {
                return length;
            }
            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    if (transaction.GetObject(curveId, OpenMode.ForRead) is Curve curve)
                    {
                        var endPoint = curve.EndPoint;
                        length = curve.GetDistAtPoint(endPoint);
                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();
                }
            }
            return length;
        }



        /// <summary>
        /// 获取curve对象上从指定起始长度开始，距离指定起始长度的长度对应的点坐标
        /// </summary>
        /// <param name="curveId">curve的ObjectId</param>
        /// <param name="lengthToStartLength">距离指定起始长度的长度</param>
        /// <param name="startLenght">起始长度</param>
        /// <returns>点坐标，如果失败，返回null</returns>
        public Point3d? GetPoint(ObjectId curveId, double lengthToStartLength, double startLenght = 0)
        {
            if (curveId.IsNull)
            {
                return null;
            }
            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    if (transaction.GetObject(curveId, OpenMode.ForRead) is Curve curve)
                    {
                        Point3d point = curve.GetPointAtDist(lengthToStartLength+startLenght);
                        transaction.Commit();
                        return point;
                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();
                }
            }
            return null;
        }





        #endregion



        #region Helper Methods


        #endregion


        #region Properties


        #endregion




    }
}
