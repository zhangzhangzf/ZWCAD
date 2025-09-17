using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace ZWCAD.BaseTools.Extension
{
    /// <summary>
    /// Polyline2d扩展
    /// </summary>
    public static class Polyline2dExtension
    {


        /// <summary>
        /// 获取所有顶点对象
        /// </summary>
        /// <param name="polyline2D">二维多段线</param>
        /// <returns>如果失败，返回空的列表</returns>
        public static List<Vertex2d> GetVertices(this Polyline2d polyline2D)
        {
            //返回值
            List<Vertex2d> vertices = new List<Vertex2d>();

            using (Transaction transaction = polyline2D.Database.TransactionManager.StartTransaction())
            {
                try
                {
                    foreach (ObjectId objectId in polyline2D)
                    {
                        Vertex2d vx = (Vertex2d)transaction.GetObject(objectId, OpenMode.ForRead);
                        if (vx.VertexType != Vertex2dType.SplineControlVertex)
                            vertices.Add(vx);
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();
                    transaction.Abort();

                }
            }

            return vertices;
        }


        /// <summary>
        /// 获取所有顶点的坐标点
        /// </summary>
        /// <param name="polyline2D">二维多段线</param>
        /// <returns>如果失败，返回空的列表</returns>
        public static List<Point3d> GetPoint3ds(this Polyline2d polyline2D)
        {
            //返回值
            List<Point3d> point3Ds = new List<Point3d>();

            List<Vertex2d> vertices = polyline2D.GetVertices();
            if (vertices.Count == 0)
            {
                return point3Ds;
            }
            
            vertices.ForEach(x => point3Ds.Add(x.Position));

            return point3Ds;
        }





        /// <summary>
        /// 判断多段线是否闭合，如果闭合，或者不闭合，但是收尾点相距在误差之内，也可以认为是闭合的
        /// </summary>
        /// <param name="polyline2D">多段线对象</param>
        /// <param name="tolerance">误差</param>
        /// <returns>如果闭合，返回true，否则，返回false</returns>
        public static bool IsClosed(this Polyline2d polyline2D, double tolerance = 1E-6)
        {
            //返回值
            bool isSucceed = false;
            if (polyline2D.Closed)
            {
                isSucceed = true;
            }
            else
            {

                List<Point3d> point3DLst = polyline2D.GetPoint3ds();

                if (point3DLst.Count >2 )
                {
                    Point3d startPoint = point3DLst[0];
                    Point3d endPoint = point3DLst.LastOrDefault();

                    double distance = startPoint.DistanceTo(endPoint);
                    if (distance < tolerance)
                    {
                        isSucceed = true;
                    }
                }
            }

            return isSucceed;
        }






    }
}
