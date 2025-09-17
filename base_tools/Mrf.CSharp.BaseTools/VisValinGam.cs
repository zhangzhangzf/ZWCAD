/*  *  File Name：             GrahamScan.cs  
 *  *  Author：                G.R.
 *  *  Create Date：           2022.8.1
 *  *  Function Description：  1、VisValinGam算法实现
 *  *  Revision Histroy：      Null
 *  *  Remarks：               Null
 *  */

using System;
using System.Collections.Generic;
using System.Linq;


namespace Mrf.CSharp.BaseTools
{
    /// <summary>
    /// VisValinGam算法实现
    /// </summary>
    public class VisValinGam
    {
        #region 方法
        /// <summary>
        /// 垂直距离简化
        /// </summary>
        /// <param name="pointList"></param>
        /// <param name="epsilon"></param>
        /// <param name="filteredPoints"></param>
        /// <param name="index">0：点到线段；1：点到直线</param>
        public static void MakeVisValinGamByPerpendicularDistance(List<Point3d> pointList, double epsilon, ref List<Point3d> filteredPoints, int index = 0)
        {

            //如果为空或者点数少于1个，那么直接输出原来的点集，结束
            int length = pointList.Count;
            if (pointList == null || pointList.Count() <= 1)
            {
                filteredPoints = pointList;
                return;
            }

            //如果首尾太靠近，那么去除首个点
            Point3d lineDirection = pointList[length - 1] - pointList[0];
            if (lineDirection.Length < 1.0E-8)
            {
                pointList.RemoveAt(0);
            }

            //初始化 输出值
            filteredPoints = pointList;


            //如果点集合数量超过2个，进行处理，否则不进行处理
            if (pointList.Count() > 2)
            {
                //将点集合转换为TYPoint类型集合
                List<TYPoint> TYPointList = new List<TYPoint>();
                int i = 0;
                foreach (var poiny in pointList)
                {
                    TYPointList.Add(new TYPoint() { id = i, point = poiny });
                    i++;
                }

                //进行简化处理
                VisValinGamDistanceExcute(TYPointList, epsilon, index);


                //如果处理完了，剩余点集数量大于0，则输出剩余点集合，输出原来点的实际三维坐标，否则输出空
                if (TYPointList.Count() > 0)
                {
                    filteredPoints = new List<Point3d>();
                    foreach (var ele in TYPointList)
                    {
                        filteredPoints.Add(pointList[ele.id]);
                    }
                }
                else
                {
                    filteredPoints = null;
                }
            }
        }

        /// <summary>
        /// 垂直距离简化
        /// </summary>
        /// <param name="pointList"></param>
        /// <param name="epsilon"></param>
        /// <param name="filteredPoints"></param>
        /// <param name="index">0：点到线段；1：点到直线</param>
        public static void MakeVisValinGamByPerpendicularDistance2D(List<Point3d> pointList, double epsilon, ref List<Point3d> filteredPoints, int index = 0)
        {

            //如果为空或者点数少于1个，那么直接输出原来的点集，结束
            int length = pointList.Count;
            if (pointList == null || pointList.Count() <= 1)
            {
                filteredPoints = pointList;
                return;
            }

            //如果首尾太靠近，那么去除首个点
            Point3d lineDirection = pointList[length - 1] - pointList[0];
            if (lineDirection.Length < 1.0E-8)
            {
                pointList.RemoveAt(0);
            }

            //初始化 输出值
            filteredPoints = pointList;


            //如果点集合数量超过2个，进行处理，否则不进行处理
            if (pointList.Count() > 2)
            {
                //将点集合转换为TYPoint类型集合，将Z坐标统一设为0.0
                List<TYPoint> TYPointList = new List<TYPoint>();
                int i = 0;
                foreach (var poiny in pointList)
                {
                    TYPointList.Add(new TYPoint() { id = i, point = new Point3d(poiny.X, poiny.Y, 0.0) });
                    i++;
                }

                //进行简化处理
                VisValinGamDistanceExcute(TYPointList, epsilon, index);


                //如果处理完了，剩余点集数量大于0，则输出剩余点集合，输出原来点的实际三维坐标，否则输出空
                if (TYPointList.Count() > 0)
                {
                    filteredPoints = new List<Point3d>();
                    foreach (var ele in TYPointList)
                    {
                        filteredPoints.Add(pointList[ele.id]);
                    }
                }
                else
                {
                    filteredPoints = null;
                }
            }
        }


        /// <summary>
        /// 实现VisValinGam算法
        /// </summary>
        /// <param name="pointList"></param>
        /// <param name="epsilon"></param>
        /// <param name="filteredPoints"></param>
        /// <param name="index">0：点到线段；1：点到直线</param>
        private static void VisValinGamDistanceExcute(List<TYPoint> pointList, double epsilon, int index)
        {
            //如果为空或者点数少于1个，那么不做任何处理，结束
            if (pointList == null || pointList.Count() <= 1)
            {
                return;
            }

            //如果首尾太靠近，那么去除首个点
            int length = pointList.Count;
            Point3d lineDirection = pointList[length - 1].point - pointList[0].point;
            if (lineDirection.Length < 1.0E-8)
            {
                pointList.RemoveAt(0);
            }

            //如果点集合数量超过2个，进行处理，否则不进行处理
            if (pointList.Count() > 2)
            {
                int record = 0;  //记录去除的点数
                int i = 1;
                while (i < pointList.Count() - 1)
                {
                    Point3d ipP = pointList[i - 1].point;
                    Point3d currentP = pointList[i].point;
                    Point3d nextP = pointList[i + 1].point;

                    double dis;
                    if (index == 0)
                        pointtofiniteface(currentP, new Point3d[] { ipP, nextP }, out dis, out Point3d fp, 1);
                    else
                        pointtoface(currentP, new Point3d[] { ipP, nextP - ipP }, out dis, out Point3d fp, 1);

                    if (dis < epsilon)
                    {
                        pointList.RemoveAt(i);
                        i--;
                        record++;
                    }
                    i++;
                }

                //如果本次去除的点数超过0，并且多段线点数大于2，继续去除点
                if (record > 0 && pointList.Count() > 2)
                {
                    VisValinGamDistanceExcute(pointList, epsilon, index);
                }
            }
        }


        /// <summary>
        /// 角度简化
        /// </summary>
        /// <param name="PointList"></param>
        /// <param name="lowerLimit"></param>
        /// <param name="upperLimit"></param>
        /// <param name="filteredPoints"></param>
        /// <param name="index">0：点到线段；1：点到直线</param>
        public static void MakeVisValinGamByAngle(List<Point3d> pointList, double lowerLimit, double upperLimit, ref List<Point3d> filteredPoints, int index = 0)
        {

            //如果为空或者点数少于1个，那么直接输出原来的点集，结束
            int length = pointList.Count;
            if (pointList == null || pointList.Count() <= 1)
            {
                filteredPoints = pointList;
                return;
            }

            //如果首尾太靠近，那么去除首个点
            Point3d lineDirection = pointList[length - 1] - pointList[0];
            if (lineDirection.Length < 1.0E-8)
            {
                pointList.RemoveAt(0);
            }

            //初始化 输出值
            filteredPoints = pointList;


            //如果点集合数量超过2个，进行处理，否则不进行处理
            if (pointList.Count() > 2)
            {
                //将点集合转换为TYPoint类型集合
                List<TYPoint> TYPointList = new List<TYPoint>();
                int i = 0;
                foreach (var poiny in pointList)
                {
                    TYPointList.Add(new TYPoint() { id = i, point = poiny });
                    i++;
                }

                //进行简化处理
                VisValinGamAngleExcute(TYPointList, lowerLimit, upperLimit, index);


                //如果处理完了，剩余点集数量大于0，则输出剩余点集合，输出原来点的实际三维坐标，否则输出空
                if (TYPointList.Count() > 0)
                {
                    filteredPoints = new List<Point3d>();
                    foreach (var ele in TYPointList)
                    {
                        filteredPoints.Add(pointList[ele.id]);
                    }
                }
                else
                {
                    filteredPoints = null;
                }
            }
        }

        /// <summary>
        /// 垂直距离简化
        /// </summary>
        /// <param name="PointList"></param>
        /// <param name="lowerLimit"></param>
        /// <param name="upperLimit"></param>
        /// <param name="filteredPoints"></param>
        /// <param name="index">0：点到线段；1：点到直线</param>
        public static void MakeVisValinGamByAngle2D(List<Point3d> pointList, double lowerLimit, double upperLimit, ref List<Point3d> filteredPoints, int index = 0)
        {

            //如果为空或者点数少于1个，那么直接输出原来的点集，结束
            int length = pointList.Count;
            if (pointList == null || pointList.Count() <= 1)
            {
                filteredPoints = pointList;
                return;
            }

            //如果首尾太靠近，那么去除首个点
            Point3d lineDirection = pointList[length - 1] - pointList[0];
            if (lineDirection.Length < 1.0E-8)
            {
                pointList.RemoveAt(0);
            }

            //初始化 输出值
            filteredPoints = pointList;


            //如果点集合数量超过2个，进行处理，否则不进行处理
            if (pointList.Count() > 2)
            {
                //将点集合转换为TYPoint类型集合，将Z坐标统一设为0.0
                List<TYPoint> TYPointList = new List<TYPoint>();
                int i = 0;
                foreach (var poiny in pointList)
                {
                    TYPointList.Add(new TYPoint() { id = i, point = new Point3d(poiny.X, poiny.Y, 0.0) });
                    i++;
                }

                //进行简化处理
                VisValinGamAngleExcute(TYPointList, lowerLimit, upperLimit, index);


                //如果处理完了，剩余点集数量大于0，则输出剩余点集合，输出原来点的实际三维坐标，否则输出空
                if (TYPointList.Count() > 0)
                {
                    filteredPoints = new List<Point3d>();
                    foreach (var ele in TYPointList)
                    {
                        filteredPoints.Add(pointList[ele.id]);
                    }
                }
                else
                {
                    filteredPoints = null;
                }
            }
        }


        /// <summary>
        /// 实现VisValinGam算法
        /// </summary>
        /// <param name="pointList"></param>
        /// <param name="lowerLimit"></param>
        /// <param name="upperLimit"></param>
        /// <param name="filteredPoints"></param>
        /// <param name="index">0：点到线段；1：点到直线</param>
        private static void VisValinGamAngleExcute(List<TYPoint> pointList, double lowerLimit, double upperLimit, int index)
        {
            //如果为空或者点数少于1个，那么不做任何处理，结束
            if (pointList == null || pointList.Count() <= 1)
            {
                return;
            }

            //如果首尾太靠近，那么去除首个点
            int length = pointList.Count;
            Point3d lineDirection = pointList[length - 1].point - pointList[0].point;
            if (lineDirection.Length < 1.0E-8)
            {
                pointList.RemoveAt(0);
            }

            //如果点集合数量超过2个，进行处理，否则不进行处理
            if (pointList.Count() > 2)
            {
                int record = 0;  //记录去除的点数
                int i = 1;
                while (i < pointList.Count() - 1)
                {
                    Point3d ipP = pointList[i - 1].point;
                    Point3d currentP = pointList[i].point;
                    Point3d nextP = pointList[i + 1].point;

                    Point3d vector1 = ipP - currentP;
                    Point3d vector2 = nextP - currentP;

                    if (vector1.IsZeroLength() || vector2.IsZeroLength())
                    {
                        pointList.RemoveAt(i);
                        i--;
                    }

                    vector1 = vector1.GetNormal();
                    vector2 = vector2.GetNormal();

                    double angle = Math.Acos(vector1.DotProduct(vector2));


                    if (angle < lowerLimit || angle > upperLimit)
                    {
                        pointList.RemoveAt(i);
                        i--;
                        record++;
                    }
                    i++;
                }

                //如果本次去除的点数超过0，并且多段线点数大于2，继续去除点
                if (record > 0 && pointList.Count() > 2)
                {
                    VisValinGamAngleExcute(pointList, lowerLimit, upperLimit, index);
                }
            }
        }
        #endregion


        /// <summary>
        /// pointtoface 获取点到面(线)的最短距离以及面上的最近点
        /// </summary>
        /// <param name="q">面（线）外一点（也可以是面（线）内）</param>
        /// <param name="f">面（线）（面（线）上一点，面（线）的法向(长度大于0.0)）</param>
        /// <param name="d">点到面（线）的最短距离(如果为负值，点位于面的背面)</param>
        /// <param name="fp">面（线）上最近点</param>
        /// <param name="index">0为点到面，1为点到线</param>
        public static void pointtoface(Point3d q, Point3d[] f, out double d, out Point3d fp, int index)
        {
            d = 0;
            fp = default;
            if (index == 0) //点到面的距离
            {
                if (Math.Sqrt(f[1].DotProduct(f[1])) > 0.0)
                {
                    d = f[1].DotProduct(q - f[0]) / Math.Sqrt(f[1].DotProduct(f[1]));
                    fp = q - d * f[1] / Math.Sqrt(f[1].DotProduct(f[1]));
                }
                else
                {
                    d = double.MaxValue;
                    fp = default;
                    throw new NotImplementedException("法向量f[1]程度小于等于0");
                }
            }
            if (index == 1) //点到线的距离
            {
                if (f[1].Length > 0.0)
                {
                    d = ((q - f[0]).DotProduct(f[1] / f[1].Length) / f[1].Length * f[1] + f[0] - q).Length;
                    fp = (q - f[0]).DotProduct(f[1] / f[1].Length) / f[1].Length * f[1] + f[0];
                }
                else
                {
                    d = double.MaxValue;
                    fp = default;
                    throw new NotImplementedException("方向向量f[1]程度小于等于0");
                }
            }
            if (index != 1 && index != 0)
            {
                d = double.MaxValue;
                fp = default;
                throw new NotImplementedException("index值仅有0（平行四边形体）或者1（平行四边形）");
            }
        }
        /// <summary>
        /// pointtofiniteface 获取点到有限面(线段)的最短距离以及面上的最近点
        /// !!!!!!!!!!!!!!!!问题是有限面的问题还没有解决
        /// </summary>
        /// <param name="q">面（线）外一点（也可以是面（线）内）</param>
        /// <param name="f">面：由一系列点构成按照右手螺旋规则布置顶点；线：线段两点）</param>
        /// <param name="d">点到面（线）的最短距离(如果为负值，点位于面的背面)</param>
        /// <param name="fp">面（线）上最近点</param>
        /// <param name="index">0为点到面，1为点到线</param>
        private static void pointtofiniteface(Point3d q, Point3d[] f, out double d, out Point3d fp, int index)
        {
            d = 0;
            fp = default;
            if (index == 0) //点到面的距离
            {
                if (Math.Sqrt(f[1].DotProduct(f[1])) > 0.0)
                {
                    d = f[1].DotProduct(q - f[0]) / Math.Sqrt(f[1].DotProduct(f[1]));
                    fp = q - d * f[1] / Math.Sqrt(f[1].DotProduct(f[1]));
                }
                else
                {
                    d = double.MaxValue;
                    fp = default;
                    throw new NotImplementedException("法向量f[1]程度小于等于0");
                }
            }
            if (index == 1) //点到线的距离
            {
                if ((f[1] - f[0]).Length > 0.0)
                {
                    d = ((q - f[0]).DotProduct((f[1] - f[0]).GetNormal()) * (f[1] - f[0]).GetNormal() + f[0] - q).Length;
                    fp = (q - f[0]).DotProduct((f[1] - f[0]).GetNormal()) * (f[1] - f[0]).GetNormal() + f[0];
                    if ((fp - f[0]).DotProduct(f[1] - f[0]) < 0 || (fp - f[1]).DotProduct(f[0] - f[1]) < 0)
                    {
                        double d1 = (q - f[0]).Length;
                        double d2 = (q - f[1]).Length;
                        if (d1 <= d2)
                        {
                            d = d1;
                            fp = f[0];
                        }
                        else
                        {
                            d = d2;
                            fp = f[1];
                        }
                    }
                }
                else
                {
                    d = double.MaxValue;
                    fp = default;
                    throw new NotImplementedException("方向向量f[1]程度小于等于0");
                }
            }
            if (index != 1 && index != 0)
            {
                d = double.MaxValue;
                fp = default;
                throw new NotImplementedException("index值仅有0（平行四边形体）或者1（平行四边形）");
            }
        }
    }
}
