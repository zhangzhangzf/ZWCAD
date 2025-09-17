using ZwSoft.ZwCAD.Geometry;

namespace ZWCAD.BaseTools.Extension
{
    /// <summary>
    /// Point2d类扩展
    /// </summary>
    public static class Point2dExtension
    {

        /// <summary>
        /// 翻转x和y的值，返回新的对象.
        /// </summary>
        /// <param name="pt">二维点坐标</param>
        /// <param name="flip">是否需要翻转</param>
        /// <returns>反转后的新对象，不修改原对象</returns>
        public static Point2d Swap(this Point2d pt,bool flip=true)
        {
            return flip? new Point2d(pt.Y, pt.X):pt;
        }


        /// <summary>
        /// 通过指定z值，将二维点坐标返回三维点坐标.
        /// </summary>
        /// <param name="pt">二维点坐标</param>
        /// <param name="zValue">指定的z值</param>
        /// <returns>三维点坐标.</returns>
        public static Point3d Pad(this Point2d pt,double zValue=0)
        {
            return new Point3d(pt.X, pt.Y, zValue);
        }
    }
}
