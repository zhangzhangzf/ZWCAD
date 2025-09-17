
using ZwSoft.ZwCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Point3dInCAD = ZwSoft.ZwCAD.Geometry.Point3d;
using Point3d = Mrf.CSharp.BaseTools.Point3d;




namespace ZWCAD.BaseTools.Extension
{
    /// <summary>
    /// 三维点扩展
    /// </summary>
    public static class Point3dExtension
    {


        /// <summary>
        /// 两个点坐标相加
        /// </summary>
        /// <param name="firstPoint">第一个点坐标</param>
        /// <param name="secondPoint">第二个点坐标</param>
        /// <returns>两个点坐标相加</returns>
        public static Point3dInCAD AddBy(this Point3dInCAD firstPoint, Point3dInCAD secondPoint)
        {
            Point3dInCAD result = new Point3dInCAD(firstPoint.X + secondPoint.X, firstPoint.Y + secondPoint.Y, firstPoint.Z + secondPoint.Z);
            return result;
        }


        /// <summary>
        /// 两个点坐标相减
        /// </summary>
        /// <param name="firstPoint">第一个点坐标</param>
        /// <param name="secondPoint">第二个点坐标</param>
        /// <returns>两个点坐标相减</returns>
        public static Point3dInCAD SubtractBy(this Point3dInCAD firstPoint, Point3dInCAD secondPoint)
        {
            Point3dInCAD result = new Point3dInCAD(firstPoint.X - secondPoint.X, firstPoint.Y - secondPoint.Y, firstPoint.Z - secondPoint.Z);

            return result;
        }









        /// <summary>
        /// 忽略三维点坐标的z值.
        /// </summary>
        /// <param name="pt">三维点坐标</param>
        /// <returns>二维点坐标</returns>
        public static Point2d Strip(this Point3dInCAD pt)
        {
            return new Point2d(pt.X, pt.Y);
        }




        /// <summary>
        /// 自定义的Point3d转换为cad中的Pointe3d
        /// </summary>
        /// <param name="point3D">自定义的Point3d</param>
        /// <returns>cad中的Point3d</returns>
        public static Point3dInCAD Point3dToPoint3dInCAD(this Point3d point3D)
        {
            Point3dInCAD point3dInCAD = new Point3dInCAD(point3D.X, point3D.Y, point3D.Z);
            return point3dInCAD;
        }






        /// <summary>
        /// cad中的Pointe3d转换为自定义的Point3d
        /// </summary>
        /// <param name="point3DInCAD">cad中的Point3d</param>
        /// <returns>自定义的Point3d</returns>
        public static Point3d Point3dInCADToPoint3d(this Point3dInCAD point3DInCAD)
        {
            Point3d point3d = new Point3d(point3DInCAD.X, point3DInCAD.Y, point3DInCAD.Z);
            return point3d;
        }





        /// <summary>
        /// 自定义的Point3d列表转换为cad中的Pointe3d列表
        /// </summary>
        /// <param name="point3DLst">自定义的Point3d列表</param>
        /// <returns>cad中的Point3d列表</returns>
        public static List<Point3dInCAD> Point3dLstToPoint3dInCADLst(List<Point3d> point3DLst)
        {

            //返回值
            List<Point3dInCAD> Point3dInCADLst = new List<Point3dInCAD>();

            point3DLst.ForEach(x => Point3dInCADLst.Add(Point3dToPoint3dInCAD(x)));

            return Point3dInCADLst;
        }




        /// <summary>
        /// cad中的Pointe3d列表转换为自定义的Point3d列表
        /// </summary>
        /// <param name="point3DInCADLst">cad中的Point3d列表</param>
        /// <returns>自定义的Point3d列表</returns>
        public static List<Point3d> Point3dInCADLstToPoint3dLst(List<Point3dInCAD> point3DInCADLst)
        {

            //返回值
            List<Point3d> point3dLst = new List<Point3d>();

            point3DInCADLst.ForEach(x => point3dLst.Add(Point3dInCADToPoint3d(x)));

            return point3dLst;

        }






        /// <summary>
        /// x值增加,不修改源对象 返回新的对象
        /// </summary>
        /// <param name="point3DInCAD"></param>
        /// <param name="xValue"></param>
        /// <returns></returns>
        public static Point3dInCAD AddByXValue(this Point3dInCAD point3DInCAD, double xValue)
        {
            return new Point3dInCAD(point3DInCAD.X+xValue,point3DInCAD.Y,point3DInCAD.Z);
        }


        /// <summary>
        /// y值增加,不修改源对象 返回新的对象
        /// </summary>
        /// <param name="point3DInCAD"></param>
        /// <returns></returns>

        public static Point3dInCAD AddByYValue(this Point3dInCAD point3DInCAD, double yValue)
        {
            return new Point3dInCAD(point3DInCAD.X, point3DInCAD.Y+yValue, point3DInCAD.Z);
        }

        /// <summary>
        /// z值增加,不修改源对象 返回新的对象
        /// </summary>
        /// <param name="point3DInCAD"></param>
        /// <param name="zValue"></param>
        /// <returns></returns>
        public static Point3dInCAD AddByZValue(this Point3dInCAD point3DInCAD, double zValue)
        {
            return new Point3dInCAD(point3DInCAD.X, point3DInCAD.Y, point3DInCAD.Z+zValue);
        }


    }
}
