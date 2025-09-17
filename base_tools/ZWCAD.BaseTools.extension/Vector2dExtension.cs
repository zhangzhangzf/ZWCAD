using ZwSoft.ZwCAD.Geometry;

namespace ZWCAD.BaseTools.Extension
{
    /// <summary>
    /// Vector2d扩展
    /// </summary>
    public static class Vector2dExtension
    {
        /// <summary>
        /// 规范化
        /// </summary>
        /// <param name="vector2d"></param>
        /// <returns></returns>
        public static Vector2d Normalized(this Vector2d vector2d)
        {
            if (vector2d.IsZeroLength() || vector2d.IsUnitLength())
            {
                return vector2d;
            }

            double length = vector2d.Length;
            Vector2d vector2dNormalized = vector2d / length;

            return vector2dNormalized;
        }

    }
}
