using ZwSoft.ZwCAD.Geometry;


namespace ZWCAD.BaseTools.Extension
{
    /// <summary>
    /// Vector3d扩展
    /// </summary>
    public class Vector3dExtension
    {

        /// <summary>
        /// 获取两根线的角平分线的单位向量
        /// </summary>
        /// <param name="firstLineStartPoint">第一根线的起点</param>
        /// <param name="firstLineEndPoint">第一根线的终点</param>
        /// <param name="secondLineStartPoint">第二根线的起点</param>
        /// <param name="secondLineEndPoint">第二根线的终点</param>
        /// <returns>角平分线的单位向量</returns>
        public  static Vector3d GetDividedVector(Point3d firstLineStartPoint, Point3d firstLineEndPoint, Point3d secondLineStartPoint, Point3d secondLineEndPoint)
        {
            //获取单位向量
            Vector3d firstLineVector = (firstLineEndPoint - firstLineStartPoint).GetNormal();
            Vector3d secondLineVector = (secondLineEndPoint - secondLineStartPoint).GetNormal();

            Vector3d dividedVector = (firstLineVector + secondLineVector).GetNormal();

            return dividedVector;

        }

    }
}
