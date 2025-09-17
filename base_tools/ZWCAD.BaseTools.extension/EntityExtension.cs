using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.Geometry;

namespace ZWCAD.BaseTools.Extension
{
    /// <summary>
    /// Entity类扩展
    /// </summary>
    public static class EntityExtension
    {



        /// <summary>
        /// 获取实体边界
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns>实体编辑,如果读取错误，返回null</returns>
        public static Extents3d? GetEntityExtents3d(this Entity entity)
        {
            Extents3d? extents = null;
            try
            {
                extents = entity.GeometricExtents; //mtext在这里可能会有问题，改成块也没有用

            }
            catch
            {

                try
                {
                    if (entity is BlockReference blockReference)
                    {
                        extents = blockReference.GeometryExtentsBestFit();
                    }

                }
                catch
                {
                    extents = null;
                }
            }

            return extents;
        }





        /// <summary>
        /// 获取实体边界的最小点
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns>最小点坐标Point3d,如果读取错误，返回null</returns>
        public static Point3d? GetEntityMinPoint(this Entity entity)
        {
            Extents3d? extents3D = GetEntityExtents3d(entity);

            if (extents3D == null)
            {
                return null;
            }

            Point3d minPoint = extents3D.Value.MinPoint;
            return minPoint;

        }


        /// <summary>
        /// 获取实体边界的最大点
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns>最大点坐标Point3d,如果读取错误，返回null</returns>
        public static Point3d? GetEntityMaxPoint(this Entity entity)
        {
            Extents3d? extents3D = GetEntityExtents3d(entity);

            if (extents3D == null)
            {
                return null;
            }

            Point3d maxPoint = extents3D.Value.MaxPoint;
            return maxPoint;
        }




        /// <summary>
        /// 获取实体边界的宽度和高度,有些对象会读取不出来，原因未知，返回ErrorStatus:NullObjectId,则返回null
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns>宽度和高度组成的数组double[],如果读取不成功，返回null</returns>
        public static double[] GetEntityBoundingWidthAndHeight(this Entity entity)
        {
            Extents3d? extents3D = entity. GetEntityExtents3d();

            if (extents3D == null)
            {
                return null;
            }


            Point3d minPoint = extents3D.Value.MinPoint;
            Point3d maxPoint = extents3D.Value.MaxPoint;

            double width = maxPoint.X - minPoint.X;
            double height = maxPoint.Y - minPoint.Y;
            return new double[] { width, height };

        }



        /// <summary>
        /// 获取实体边界的宽度,有些对象会读取不出来，原因未知，返回ErrorStatus:NullObjectId,则返回double.NaN
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns>宽度,如果读取不成功，返回double.NaN</returns>
        public static double GetEntityBoundingWidth(this Entity entity)
        {
            Extents3d? extents3D = entity. GetEntityExtents3d();

            if (extents3D == null)
            {
                return double.NaN;
            }


            Point3d minPoint = extents3D.Value.MinPoint;
            Point3d maxPoint = extents3D.Value.MaxPoint;


            double width = maxPoint.X - minPoint.X;
            return width;
        }



        /// <summary>
        /// 获取实体边界的高度,有些对象会读取不出来，原因未知，返回ErrorStatus:NullObjectId,则返回double.NaN
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns>高度,如果读取不成功，返回double.NaN</returns>
        public static double GetEntityBoundingHeight(this Entity entity)
        {
            Extents3d? extents3D = entity. GetEntityExtents3d();

            if (extents3D == null)
            {
                return double.NaN;
            }


            Point3d minPoint = extents3D.Value.MinPoint;
            Point3d maxPoint = extents3D.Value.MaxPoint;

            double height = maxPoint.Y - minPoint.Y;
            return height;
        }


    }
}
