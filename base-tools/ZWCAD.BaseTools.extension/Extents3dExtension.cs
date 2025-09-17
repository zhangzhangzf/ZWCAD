using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.Geometry;
using System.Linq;


namespace ZWCAD.BaseTools.Extension
{
    /// <summary>
    /// Extents3d类扩展
    /// </summary>
    public static class Extents3dExtension
    {


        /// <summary>
        /// 判断边界是否有效，当最小坐标为正数，最大坐标为负数时无效
        /// </summary>
        /// <param name="extents3D">边界对象</param>
        /// <returns>当有效时，返回true，否则，返回false</returns>
        public static bool ValidDbExtents(Extents3d extents3D)
        {
            Point3d min=extents3D.MinPoint;
                Point3d max=extents3D.MaxPoint;
            return
              !(min.X > 0 && min.Y > 0 && min.Z > 0 &&
                max.X < 0 && max.Y < 0 && max.Z < 0);

        }

    }
}
