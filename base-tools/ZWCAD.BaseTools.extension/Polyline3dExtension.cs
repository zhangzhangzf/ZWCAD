using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace ZWCAD.BaseTools.Extension
{
    /// <summary>
    /// Polyline2d扩展
    /// </summary>
    public static class Polyline3dExtension
    {


        /// <summary>
        /// 获取所有顶点对象
        /// </summary>
        /// <param name="polyline3D">三维多段线</param>
        /// <returns>如果失败，返回空的列表</returns>
        public static List<PolylineVertex3d> GetVertices(this Polyline3d polyline3D)
        {
            //返回值
            List<PolylineVertex3d> vertices = new List<PolylineVertex3d>();

            using (Transaction transaction = polyline3D.Database.TransactionManager.StartTransaction())
            {
                try
                {
                    foreach (ObjectId objectId in polyline3D)
                    {
                        PolylineVertex3d vx = (PolylineVertex3d)transaction.GetObject(objectId, OpenMode.ForRead);
                        if (vx.VertexType != Vertex3dType.ControlVertex)
                            vertices.Add(vx);
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();
                }
            }

            return vertices;
        }


        /// <summary>
        /// 获取所有顶点的坐标点
        /// </summary>
        /// <param name="polyline3D">二维多段线</param>
        /// <returns>如果失败，返回空的列表</returns>
        public static List<Point3d> GetPoint3ds(this Polyline3d polyline3D)
        {
            //返回值
            List<Point3d> point3Ds = new List<Point3d>();

            List<PolylineVertex3d> vertices = polyline3D.GetVertices();
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
        /// <param name="polyline3D">多段线对象</param>
        /// <param name="tolerance">误差</param>
        /// <returns>如果闭合，返回true，否则，返回false</returns>
        public static bool IsClosed(this Polyline3d polyline3D, double tolerance = 1E-6)
        {
            //返回值
            bool isSucceed = false;
            if (polyline3D.Closed)
            {
                isSucceed = true;
            }
            else
            {

                List<Point3d> point3DLst= polyline3D.GetPoint3ds();

                if (point3DLst.Count > 2)
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
