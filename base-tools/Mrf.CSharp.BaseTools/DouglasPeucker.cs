/*  *  File Name：             DouglasPeucker.cs  
 *  *  Author：                G.R.
 *  *  Create Date：           2021.12.13
 *  *  Function Description：  1、多段线简化工具—Douglas Peucker算法
 *  *  Revision Histroy：      Null
 *  *  Remarks：               Null
 *  */

using System;
using System.Collections.Generic;
using System.Linq;


namespace Mrf.CSharp.BaseTools
{
    /// <summary>
    /// Douglas Peucker算法  用于多段线简化
    /// </summary>
    public class DouglasPeucker
    {
        /// <summary>
        /// 实现Douglas Peucker算法
        /// </summary>
        /// <param name="PointList"></param>
        /// <param name="epsilon"></param>
        /// <param name="filteredPoints"></param>
        public static void MakeDouglasPeucker(IList<Point3d> PointList, double epsilon, ref List<Point3d> filteredPoints)
        {
            int length = PointList.Count;
            if (PointList == null || PointList.Count() <= 1)
            {
                return;
            }

            Point3d lineDirection = PointList[length - 1] - PointList[0];
            if (lineDirection.Length < 1.0E-8)
            {
                PointList.RemoveAt(0);
            }

            if (PointList.Count() > 1)
            {
                filteredPoints = new List<Point3d>();

                List<TYPoint> TYPointList = new List<TYPoint>();
                int i = 0;
                foreach (var poiny in PointList)
                {
                    TYPointList.Add(new TYPoint() { id = i, point = poiny });
                    i++;
                }

                List<TYPoint> filteredTYPoints = new List<TYPoint>();
                douglasPeuckerExcute(TYPointList, epsilon, ref filteredTYPoints);

                if (filteredTYPoints.Count() > 0)
                {
                    //按次序连接起点 - 分割点 - 终点
                    filteredTYPoints.Sort(delegate (TYPoint a, TYPoint b) { return a.id.CompareTo(b.id); });


                    filteredPoints.Add(PointList[0]);
                    foreach (var ele in filteredTYPoints)
                    {
                        filteredPoints.Add(PointList[ele.id]);
                    }
                    filteredPoints.Add(PointList[PointList.Count() - 1]);
                }
            }
        }


        /// <summary>
        /// 实现Douglas Peucker算法
        /// </summary>
        /// <param name="PointList"></param>
        /// <param name="epsilon"></param>
        /// <param name="filteredPoints"></param>
        public static void MakeDouglasPeucker2D(IList<Point3d> PointList, double epsilon, ref List<Point3d> filteredPoints)
        {
            int length = PointList.Count;
            if (PointList == null || PointList.Count() <= 1)
            {
                return;
            }

            Point3d lineDirection = PointList[length - 1] - PointList[0];
            if (lineDirection.Length < 1.0E-8)
            {
                PointList.RemoveAt(0);
            }

            if (PointList.Count() > 1)
            {
                filteredPoints = new List<Point3d>();

                List<TYPoint> TYPointList = new List<TYPoint>();
                int i = 0;
                foreach (var poiny in PointList)
                {
                    TYPointList.Add(new TYPoint() { id = i, point = new Point3d(poiny.X, poiny.Y, 0.0) });
                    i++;
                }

                //起点
                filteredPoints.Add(PointList[0]);

                //获取中间分割点
                List<TYPoint> filteredTYPoints = new List<TYPoint>();
                douglasPeuckerExcute(TYPointList, epsilon, ref filteredTYPoints);
                if (filteredTYPoints.Count() > 0)
                {
                    //按次序连接起点 - 分割点 - 终点
                    filteredTYPoints.Sort(delegate (TYPoint a, TYPoint b) { return a.id.CompareTo(b.id); });

                    foreach (var ele in filteredTYPoints)
                    {
                        filteredPoints.Add(PointList[ele.id]);
                    }
                }

                //终点
                filteredPoints.Add(PointList[PointList.Count() - 1]);
            }
        }

        /// <summary>
        /// 实现Douglas Peucker算法
        /// </summary>
        /// <param name="pointList"></param>
        /// <param name="epsilon"></param>
        /// <param name="filteredPoints"></param>
        private static void douglasPeuckerExcute(List<TYPoint> pointList, double epsilon, ref List<TYPoint> filteredPoints)
        {
            var dmax = 0d;
            int index = 0;
            int length = pointList.Count;

            if (pointList == null || pointList.Count() <= 1)
            {
                return;
            }

            Point3d lineDirection = pointList[length - 1].point - pointList[0].point;
            if (lineDirection.Length < 1.0E-8)
            {
                pointList.RemoveAt(0);
            }

            if (pointList.Count() > 1)
            {
                length = pointList.Count;
                lineDirection = pointList[length - 1].point - pointList[0].point;
                //找到2个点到倒数第2个点中距离首尾点构成的线段最远的点
                for (int i = 1; i < length - 1; i++)
                {
                    //获取第i个点距离首尾点构成的线段的距离

                    var d = d_pointtoface(pointList[i].point, new Point3d[] { pointList[0].point, lineDirection.GetNormal() }, 1);
                    //如果距离超过上一次记录的最大距离，那么index记录当前点，并更新最大距离
                    if (d > dmax)
                    {
                        index = i;
                        dmax = d;
                    }
                }

                //如果最远的点离首尾点构成的线段的距离超过了指定的公差，那么将这个点记录到简化后点集中
                if (dmax > epsilon)
                {
                    filteredPoints.Add(pointList[index]);

                    douglasPeuckerExcute(pointList.Take(index + 1).ToList(), epsilon, ref filteredPoints);
                    douglasPeuckerExcute(pointList.Skip(index + 1).Take(pointList.Count - index - 1).ToList(), epsilon, ref filteredPoints);
                }
            }
        }


        /// <summary>
        /// d_pointtoface 获取点到面（线）的最短距离
        /// ;index:0为点到面，1为点到线
        /// </summary>
        /// <param name="q">面（线）外一点（也可以是面（线）内）</param>
        /// <param name="f">面（线）（面（线）上一点，面（线）的法向(长度大于0.0)）</param>
        /// <param name="index">0为点到面，1为点到线</param>
        private static double d_pointtoface(Point3d q, Point3d[] f, int index)
        {
            double d = -1.0;
            if (index == 0)   //点到面
            {
                if (Math.Sqrt(f[1].DotProduct(f[1])) > 0.0)
                {
                    d = Math.Abs(f[1].DotProduct(q - f[0]) / Math.Sqrt(f[1].DotProduct(f[1])));
                }
                else
                {
                    d = double.MaxValue;
                    throw new NotImplementedException("法向量f[1]程度小于等于0");
                }
            }
            if (index == 1)    //点到线
            {
                if (f[1].Length > 0.0)
                {
                    d = ((q - f[0]).DotProduct(f[1] / f[1].Length) / f[1].Length * f[1] + f[0] - q).Length;
                }
                else
                {
                    d = double.MaxValue;
                    throw new NotImplementedException("方向向量f[1]程度小于等于0");
                }
            }



            if (index != 1 && index != 0)
            {
                d = double.MaxValue;
                throw new NotImplementedException("index值仅有0（面）或者1（线）");
            }
            return d;
        }
    }

    /// <summary>
    /// 用于 DouglasPeucker
    /// </summary>
    internal class TYPoint
    {
        public int id;

        public Point3d point = new Point3d();
    }
}
