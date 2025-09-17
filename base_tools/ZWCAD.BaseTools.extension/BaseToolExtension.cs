using ZwSoft.ZwCAD.Geometry;
using System;

namespace ZWCAD.BaseTools.Extension
{
    /// <summary>
    /// 基础工具扩展
    /// </summary>
    public static partial class BaseToolExtension
    {




        /// <summary>
        /// 判断三个点是否在一条直线上
        /// </summary>
        /// <param name="firstPoint">第一个点</param>
        /// <param name="secondPoint">第二个点</param>
        /// <param name="thirdPoint">第三个点</param>
        /// <returns>true or false</returns>
        public static bool IsOnOneLine(this Point3d firstPoint, Point3d secondPoint, Point3d thirdPoint)
        {
            Vector3d v21 = secondPoint.GetVectorTo(firstPoint);
            Vector3d v31 = secondPoint.GetVectorTo(thirdPoint);
            if (v21.GetAngleTo(v31) == 0 || v21.GetAngleTo(v31) == Math.PI)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 获取起始点到终止点的向量与x轴正向的夹角弧度
        /// </summary>
        /// <param name="startPoint">起始点</param>
        /// <param name="endPoint">终止点</param>
        /// <returns>弧度值</returns>
        public static double GetAngleToXAxis(this Point3d startPoint, Point3d endPoint)
        {
            //X轴正方向的向量
            Vector3d xVector = Vector3d.XAxis;
            //获取起点到终点的向量
            Vector3d VsToe = startPoint.GetVectorTo(endPoint);
            return VsToe.Y > 0 ? xVector.GetAngleTo(VsToe) : -xVector.GetAngleTo(VsToe);
        }


        /// <summary>
        /// 求两个点的中点
        /// </summary>
        /// <param name="point1">第一个点</param>
        /// <param name="point2">第二个点</param>
        /// <returns>中点坐标</returns>
        public static Point3d GetCenterPointBetweenTwoPoints(this Point3d point1, Point3d point2)
        {
            return new Point3d((point1.X + point2.X) / 2, (point1.Y + point2.Y) / 2, (point1.Z + point2.Z) / 2);
        }


        /// <summary>
        /// 获取绝对坐标点的相对坐标
        /// </summary>
        /// <param name="absolutePoint">绝对坐标点</param>
        /// <param name="originPoint">原点</param>
        /// <returns>Point2d相对坐标点</returns>
        public static Point2d GetRelativePoint(this Point2d absolutePoint, Point2d originPoint)
        {
            return new Point2d(absolutePoint.X - originPoint.X, absolutePoint.Y - originPoint.Y);
        }


    }


}
