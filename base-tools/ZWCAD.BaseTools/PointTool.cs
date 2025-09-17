using ZWCAD.BaseTools.Extension;
using ZwSoft.ZwCAD.Geometry;
using Mrf.CSharp.BaseTools.Extension;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZWCAD.BaseTools
{

    /// <summary>
    /// 点坐标工具
    /// </summary>
    public class PointTool
    {

        /// <summary>
        /// 构造函数
        /// </summary>
        public PointTool()
        {

        }


        /// <summary>
        /// 获取以centerPoint点为中心，radius为半径的均分点的集合
        /// </summary>
        /// <param name="centerPoint">中心点</param>
        /// <param name="radius">半径</param>
        /// <param name="pointNumber">点的数目</param>
        /// <returns>三维点的集合Point3dCollection</returns>
        public Point3dCollection GetPolygonSurroundPoint(Point3d centerPoint, double radius = 1500, int pointNumber = 10)
        {

            //组成多段线的点数
            double step = 2 * Math.PI / pointNumber;

            //包围点的集合
            Point3d[] values = new Point3d[10];
            for (int i = 0; i < 10; i++)
            {
                double angle = step * i;
                Point3d point = new Point3d(centerPoint.X + radius * Math.Cos(angle), centerPoint.Y + radius * Math.Sin(angle), centerPoint.Z);
                values[i] = point;
            }

            Point3dCollection polygon = new Point3dCollection(values);

            return polygon;
        }



        /// <summary>
        /// 获取两个点的中点
        /// </summary>
        /// <param name="startPoint">起点</param>
        /// <param name="endPoint">终点</param>
        /// <returns>中点Point3d</returns>
        public Point3d GetMidPoint(Point3d startPoint, Point3d endPoint)
        {
            return new Point3d((startPoint.X + endPoint.X) / 2, (startPoint.Y + endPoint.Y) / 2, (startPoint.Z + endPoint.Z) / 2);
        }







        /// <summary>
        /// 从点数组中分别找出最小的x值、y值和z值，组成最小的点
        /// </summary>
        /// <param name="points">点数组</param>
        /// <returns>最小的点,即左下角的点</returns>
        public Point3d GetMinPoint(Point3d[] points)
        {
            double minX = points.Min(p => p.X);
            double minY = points.Min(p => p.Y);
            double minZ = points.Min(p => p.Z);

            //double minX = double.MaxValue;
            //double minY = double.MaxValue;
            //double minZ = double.MaxValue;

            //foreach (Point3d each in points)
            //{
            //    minX =Math.Min(minX, each.X);
            //    minY =Math.Min(minY, each.Y);
            //    minZ =Math.Min(minZ, each.Z);

            //}

            return new Point3d(minX, minY, minZ);

        }



        /// <summary>
        /// 从点列表中分别找出最小的x值、y值和z值，组成最小的点
        /// </summary>
        /// <param name="points">点列表</param>
        /// <returns>最小的点,即左下角的点</returns>
        public Point3d GetMinPoint(List<Point3d> points)
        {

            double minX = points.Min(p => p.X);
            double minY = points.Min(p => p.Y);
            double minZ = points.Min(p => p.Z);

            //double minX = double.MaxValue;
            //double minY = double.MaxValue;
            //double minZ = double.MaxValue;

            //foreach (Point3d each in points)
            //{
            //    minX =Math.Min(minX, each.X);
            //    minY =Math.Min(minY, each.Y);
            //    minZ =Math.Min(minZ, each.Z);

            //}

            return new Point3d(minX, minY, minZ);

        }





        /// <summary>
        /// 从点数组中分别找出最大的x值、y值和z值，组成最大的点
        /// </summary>
        /// <param name="points">点数组</param>
        /// <returns>最大的点,即右上角的点</returns>
        public Point3d GetMaxPoint(Point3d[] points)
        {
            double maxX = points.Max(p => p.X);
            double maxY = points.Max(p => p.Y);
            double maxZ = points.Max(p => p.Z);


            //double maxX = double.MinValue;
            //double maxY = double.MinValue;
            //double maxZ = double.MinValue;

            //foreach (Point3d each in points)
            //{

            //    maxX =Math.Max(maxX, each.X);
            //    maxY =Math.Max(maxY, each.Y);
            //    maxZ =Math.Max(maxZ, each.Z);
            //}

            return new Point3d(maxX, maxY, maxZ);

        }





        /// <summary>
        /// 从点列表中分别找出最大的x值、y值和z值，组成最大的点
        /// </summary>
        /// <param name="points">点列表</param>
        /// <returns>最大的点,即右上角的点</returns>
        public Point3d GetMaxPoint(List<Point3d> points)
        {
            double maxX = points.Max(p => p.X);
            double maxY = points.Max(p => p.Y);
            double maxZ = points.Max(p => p.Z);

            //double maxX = double.MinValue;
            //double maxY = double.MinValue;
            //double maxZ = double.MinValue;

            //foreach (Point3d each in points)
            //{

            //    maxX =Math.Max(maxX, each.X);
            //    maxY =Math.Max(maxY, each.Y);
            //    maxZ =Math.Max(maxZ, each.Z);
            //}




            return new Point3d(maxX, maxY, maxZ);

        }






        /// <summary>
        /// 包住所有点的垂直方向的最大高度
        /// </summary>
        /// <param name="points">点列表</param>
        /// <returns>最大的高度</returns>
        public double GetMaxHeight(List<Point3d> points)
        {
            Point3d minPoint = GetMinPoint(points);
            Point3d maxPoint = GetMaxPoint(points);

            double maxHeight = maxPoint.Y-minPoint.Y;

            return maxHeight;
        }


        /// <summary>
        /// 包住所有点的水平方向的最大长度
        /// </summary>
        /// <param name="points">点列表</param>
        /// <returns>最大的长度</returns>
        public double GetMaxLength(List<Point3d> points)
        {
            Point3d minPoint = GetMinPoint(points);
            Point3d maxPoint = GetMaxPoint(points);

            double maxLength = maxPoint.X-minPoint.X;

            return maxLength;
        }






        /// <summary>
        /// 包住所有点的最大边界
        /// </summary>
        /// <param name="pointArrLst">点列表</param>
        /// <returns>点列表，分别为左下角点、左上角点、右上角点、右下角点</returns>
        public List<Point3d> GetBoundingBox(List<List<Point3d>> pointArrLst)
        {
            List<Point3d> pointLst = new List<Point3d>();

            pointArrLst.ForEach(x => pointLst.AddRange(x));

            return GetBoundingBox(pointLst);

        }




        /// <summary>
        /// 包住所有点的最大边界
        /// </summary>
        /// <param name="pointLst">点列表</param>
        /// <returns>点列表，分别为左下角点、左上角点、右上角点、右下角点</returns>
        public List<Point3d> GetBoundingBox(List<Point3d> pointLst)
        {
            //左下角的点
            Point3d minPoint = GetMinPoint(pointLst);

            //右上角的点
            Point3d maxPoint = GetMaxPoint(pointLst);


            //左上角的点
            Point3d leftUpPoint = new Point3d(minPoint.X, maxPoint.Y, minPoint.Z);

            //右下角的点
            Point3d rightDownPoint = new Point3d(maxPoint.X, minPoint.Y, minPoint.Z);


            List<Point3d> boundingBox = new List<Point3d>
         {
             minPoint,
             leftUpPoint,
             maxPoint,
             rightDownPoint
         };

            return boundingBox;
        }




        /// <summary>
        /// 由边界向外偏移一定的距离，得到的新的边界
        /// </summary>
        /// <param name="boundingBox">原边界点列表，分别为左下角点、左上角点、右上角点、右下角点，会自动判断是否符合要求</param>
        /// <param name="tolerance">偏移的水平距离</param>
        /// <returns>新边界点列表，分别为左下角点、左上角点、右上角点、右下角点，如果原边界点列表有问题，返回null</returns>
        public List<Point3d> GetNewBoundingBox(List<Point3d> boundingBox, double tolerance)
        {
            List<Point3d> newBoundingBox = null;


            if (boundingBox == null || boundingBox.Count!=4)
            {
                return newBoundingBox;
            }

            //左下角
            Point3d leftDownPoint = boundingBox[0];
            //左上角
            Point3d leftUpPoint = boundingBox[1];
            //右上角
            Point3d rightUpPoint = boundingBox[2];
            //右下角
            Point3d rightDownPoint = boundingBox[3];



            //这个tolerance是水平偏移，需要找到极轴半径
            double distance = tolerance/Math.Sin(Math.PI/4);



            //左下角
            double angle = Math.PI*5/4;
            Point3d newLeftDownPoint = GetPolarPoint(leftDownPoint, distance, angle);

            //左上角
            angle = Math.PI*3/4;
            Point3d newLeftUpPoint = GetPolarPoint(leftUpPoint, distance, angle);

            //右上角
            angle = Math.PI/4;

            Point3d newRightUpPoint = GetPolarPoint(rightUpPoint, distance, angle);

            //右下角
            angle =-Math.PI/4;
            Point3d newRightDownPoint = GetPolarPoint(rightDownPoint, distance, angle);

            newBoundingBox=new List<Point3d>
            {
                newLeftDownPoint,
                newLeftUpPoint,
                newRightUpPoint,
                newRightDownPoint
            };

            return newBoundingBox;

        }


        /// <summary>
        /// 由边界向外偏移一定的距离，得到的新的边界
        /// </summary>
        /// <param name="boundingBox">原边界点列表，分别为左下角点、左上角点、右上角点、右下角点，会自动判断是否符合要求</param>
        /// <param name="xTolerance">水平方向的偏移距离</param>
        /// <param name="yTolerance">垂直方向的偏移距离</param>
        /// <returns>新边界点列表，分别为左下角点、左上角点、右上角点、右下角点，如果原边界点列表有问题，返回null</returns>
        public List<Point3d> GetNewBoundingBox(List<Point3d> boundingBox, double xTolerance, double yTolerance)
        {
            List<Point3d> newBoundingBox = null;


            if (boundingBox == null || boundingBox.Count!=4)
            {
                return newBoundingBox;
            }

            //左下角
            Point3d leftDownPoint = boundingBox[0];
            //左上角
            Point3d leftUpPoint = boundingBox[1];
            //右上角
            Point3d rightUpPoint = boundingBox[2];
            //右下角
            Point3d rightDownPoint = boundingBox[3];



            //这个tolerance是水平偏移，需要找到极轴半径
            double distance = xTolerance/Math.Sin(Math.PI/4);



            //左下角
            Point3d newLeftDownPoint = leftDownPoint.AddBy(new Point3d(-xTolerance, -yTolerance, 0));

            //左上角
            Point3d newLeftUpPoint = leftUpPoint.AddBy(new Point3d(-xTolerance, yTolerance, 0));

            //右上角
            Point3d newRightUpPoint = rightUpPoint.AddBy(new Point3d(xTolerance, yTolerance, 0));

            //右下角
            Point3d newRightDownPoint = rightDownPoint.AddBy(new Point3d(xTolerance, -yTolerance, 0));

            newBoundingBox=new List<Point3d>
            {
                newLeftDownPoint,
                newLeftUpPoint,
                newRightUpPoint,
                newRightDownPoint
            };

            return newBoundingBox;

        }













        /// <summary>
        /// 判断点是否在边界框中
        /// </summary>
        /// <param name="point">点</param>
        /// <param name="boundingBoxMinPoint">边界框的最小点</param>
        /// <param name="boundingBoxMaxPoint">边界框的最大点</param>
        /// <returns>true or false</returns>
        public bool IsPointInsideBoundingBox(Point3d point, Point3d boundingBoxMinPoint, Point3d boundingBoxMaxPoint)
        {
            double pointX = point.X;
            double pointY = point.Y;

            double minX = boundingBoxMinPoint.X;
            double minY = boundingBoxMinPoint.Y;

            double maxX = boundingBoxMaxPoint.X;
            double maxY = boundingBoxMaxPoint.Y;


            if (pointX.GreaterThanOrEqual(minX) &&  //大于等于最小值
                 pointY.GreaterThanOrEqual(minY) &&  //大于等于最小值
                 pointX.LessThanOrEqual(maxX) && //小于等于最大值
                 pointY.LessThanOrEqual(maxY)  //小于等于最大值
                 )
            {
                return true;
            }

            {
                return false;
            }


        }




        /// <summary>
        /// 获取极坐标
        /// </summary>
        /// <param name="point">起始点的坐标</param>
        /// <param name="distance">距离</param>
        /// <param name="angle">角度，弧度制</param>
        /// <returns></returns>
        public Point3d GetPolarPointInYZPlane(Point3d point, double distance, double angle)
        {
            return new Point3d(point.X, point.Y + distance * Math.Cos(angle), point.Z + distance * Math.Sin(angle));
        }


        /// <summary>
        /// 在xy平面获取极坐标
        /// </summary>
        /// <param name="point">起始点的坐标</param>
        /// <param name="distance">距离</param>
        /// <param name="angle">角度，弧度制</param>
        /// <returns></returns>
        public Point3d GetPolarPoint(Point3d point, double distance, double angle)
        {
            return new Point3d(point.X + distance * Math.Cos(angle), point.Y + distance * Math.Sin(angle), point.Z);
        }



        /// <summary>
        /// 求两个点的减法
        /// </summary>
        /// <param name="firstPoint">第一个点</param>
        /// <param name="secondPoint">第二个点</param>
        /// <returns></returns>
        public Point3d Subtract(Point3d firstPoint, Point3d secondPoint)
        {
            return new Point3d(firstPoint.X - secondPoint.X, firstPoint.Y - secondPoint.Y, firstPoint.Z - secondPoint.Z);
        }



        /// <summary>
        /// 求两个点的加法法
        /// </summary>
        /// <param name="firstPoint">第一个点</param>
        /// <param name="secondPoint">第二个点</param>
        /// <returns></returns>
        public Point3d Add(Point3d firstPoint, Point3d secondPoint)
        {
            return new Point3d(firstPoint.X + secondPoint.X, firstPoint.Y + secondPoint.Y, firstPoint.Z + secondPoint.Z);
        }





        /// <summary>
        /// 获取三维点列表的质心
        /// </summary>
        /// <param name="point3dLst">三维点坐标列表</param>
        /// <returns>表示质心的三维点坐标</returns>
        public Point3d GetCentroid(List<Point3d> point3dLst)
        {
            double totalX = 0;
            double totalY = 0;
            double totalZ = 0;

            foreach (var point in point3dLst)
            {
                totalX += point.X;
                totalY += point.Y;
                totalZ += point.Z;
            }

            double centerX = totalX / point3dLst.Count;
            double centerY = totalY / point3dLst.Count;
            double centerZ = totalZ / point3dLst.Count;

            return new Point3d(centerX, centerY, centerZ);
        }




        /// <summary>
        /// 根据两点坐标返回角度
        /// </summary>
        /// <param name="startpoint"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public double Getrotation(Point3d startpoint, Point3d endpoint)
        {
            return Math.Atan2(endpoint.Y - startpoint.Y, endpoint.X - startpoint.X) * 180 / Math.PI;
        }

        /// <summary>
        /// 计算起点到终点向量和x轴正向的角度
        /// </summary>
        /// <param name="startPoint">起点</param>
        /// <param name="endPoint">终点</param>
        /// <returns>角度（弧度制0~2pi）</returns>
        public double GetAngleToXAxis(Point3d startPoint, Point3d endPoint)
        {
            var vector = startPoint.GetVectorTo(endPoint);
            return MathExtension.Vector2DToAngle(vector.X, vector.Y);
        }
    }

}

