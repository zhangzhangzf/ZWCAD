using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.Geometry;
using System.Collections.Generic;


namespace ZWCAD.BaseTools.Extension
{
    /// <summary>
    /// 多段线的扩展
    /// </summary>
    public static class PolylineExtension
    {
        /// <summary>
        /// 获取多段线的顶点二维坐标列表
        /// </summary>
        /// <param name="polyline">多段线对象</param>
        /// <returns>顶点坐标列表，如果没有找到，返回空的列表</returns>
        public static List<Point2d> GetPolylinePoint2dLst(this Polyline polyline)
        {
            //返回值
            List<Point2d> pointLst = new List<Point2d>();

            for (int i = 0; i < polyline.NumberOfVertices; i++)
            {
                Point2d point = polyline.GetPoint2dAt(i);
                pointLst.Add(point);
            }

            return pointLst;
        }


        /// <summary>
        /// 获取多段线的顶点三维坐标列表
        /// </summary>
        /// <param name="polyline">多段线对象</param>
        /// <returns>顶点坐标列表，如果没有找到，返回空的列表</returns>
        public static List<Point3d> GetPolylinePoint3dLst(this Polyline polyline)
        {
            //返回值
            List<Point3d> pointLst = new List<Point3d>();

            for (int i = 0; i < polyline.NumberOfVertices; i++)
            {
                Point3d point = polyline.GetPoint3dAt(i);
                pointLst.Add(point);
            }

            return pointLst;
        }


        /// <summary>
        /// 判断多段线是否闭合，如果闭合，或者不闭合，但是收尾点相距在误差之内，也可以认为是闭合的
        /// </summary>
        /// <param name="polyline">多段线对象</param>
        /// <param name="tolerance">误差</param>
        /// <returns>如果闭合，返回true，否则，返回false</returns>
        public static bool IsClosed(this Polyline polyline,double tolerance=1E-6)
        {
            //返回值
            bool isSucceed = false;
            if (polyline.Closed)
            {
                isSucceed = true;
            }
            else
            {
                if (polyline.NumberOfVertices > 2)
                {
                    Point3d startPoint=polyline.GetPoint3dAt(0);
                    Point3d endPoint=polyline.GetPoint3dAt(polyline.NumberOfVertices-1);

                    double distance=startPoint.DistanceTo(endPoint);
                    if(distance< tolerance)
                    {
                        isSucceed=true;
                    }

                }
            }

            return isSucceed;
        }


    }
}
