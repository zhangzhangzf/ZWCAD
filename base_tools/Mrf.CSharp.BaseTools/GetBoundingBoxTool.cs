using System;
using System.Collections.Generic;
using System.Linq;

using XYZ = Mrf.CSharp.BaseTools.Point3d;
//using XYZ = ZwSoft.ZwCAD.Geometry.Point3d;


namespace Mrf.CSharp.BaseTools
{
    /// <summary>
    /// 获取汇流边界
    /// </summary>
    public class GetBoundingBoxTool
    {



        #region Private Variables


        #endregion



        #region Default Constructor


        /// <summary>
        /// 构造函数
        /// </summary>
        public GetBoundingBoxTool()
        {
        }




        #endregion




        #region CommandMethods
        /// <summary>
        /// 获取轮廓（方法1）
        /// </summary>
        /// <remarks>
        /// 获取一系列矩形对象的轮廓
        /// </remarks>
        /// <param name="rectangles">一些列矩形，存储每一个矩形的范围框</param>
        /// <param name="precision"></param>
        /// <returns>如果失败，返回null</returns>
        public List<XYZ> getRectanglesOutLine3(List<XYZ[]> rectangles, double precision = 1.0E-2)
        {
            List<XYZ> outLine = null;

            if (rectangles == null || rectangles.Count() == 0) return null;

            //获取Z坐标
            double zValue = rectangles.Max(x => Math.Max(x[0].Z, x[1].Z));


            List<List<XYZ>> leftRight = getRectanglesLeftRightBoundary1(rectangles, zValue, precision);

            if (leftRight == null || leftRight.Count() != 2) return null;


            XYZ left0 = leftRight[0][0];
            XYZ right0 = leftRight[1][0];

            XYZ left1 = leftRight[0][leftRight[0].Count() - 1];
            XYZ right1 = leftRight[1][leftRight[1].Count() - 1];


            //确定哪些矩形参加上下边寻找
            int i = 0;

            List<XYZ[]> bottomRectangles = new List<XYZ[]>();
            List<XYZ[]> topRectangles = new List<XYZ[]>();
            while (i < rectangles.Count())
            {
                XYZ[] rectangle = rectangles[i];

                if (rectangle[0].X >= left0.X && rectangle[1].X <= right0.X)
                {
                    bottomRectangles.Add(rectangle);
                }
                if (rectangle[0].X >= left1.X && rectangle[1].X <= right1.X)
                {
                    topRectangles.Add(rectangle);
                }
                i++;
            }

            List<XYZ> bottom = new List<XYZ>();
            List<XYZ> top = new List<XYZ>();


            if (bottomRectangles.Count() > 0)
            {
                bottom = getRectanglesBottomBoundary1(bottomRectangles, zValue, precision);
            }
            if (topRectangles.Count() > 0)
            {
                top = getRectanglesTopBoundary1(topRectangles, zValue, precision);
            }


            outLine = new List<XYZ>();
            if (leftRight[0] != null && leftRight[0].Count() > 1)
            {
                leftRight[0].Reverse();
                outLine.AddRange(leftRight[0]);
            }

            XYZ last = outLine[outLine.Count() - 1];

            if (bottom.Count() > 3)
            {
                if (Math.Abs(bottom[0].X - last.X) < precision &&
                    Math.Abs(bottom[0].Y - last.Y) < precision)
                    bottom.RemoveAt(0);

                if (Math.Abs(bottom[bottom.Count() - 1].X - last.X) < precision &&
                    Math.Abs(bottom[bottom.Count() - 1].Y - last.Y) < precision)
                    bottom.RemoveAt(bottom.Count() - 1);

                outLine.AddRange(bottom);
            }

            if (leftRight[1] != null && leftRight[1].Count() > 1)
            {
                outLine.AddRange(leftRight[1]);
            }

            last = outLine[outLine.Count() - 1];
            if (top.Count() > 3)
            {
                top.Reverse();
                if (Math.Abs(top[0].X - last.X) < precision &&
                    Math.Abs(top[0].Y - last.Y) < precision)
                    top.RemoveAt(0);

                if (Math.Abs(top[top.Count() - 1].X - last.X) < precision &&
                    Math.Abs(top[top.Count() - 1].Y - last.Y) < precision)
                    top.RemoveAt(top.Count() - 1);

                outLine.AddRange(top);
            }

            return outLine;
        }









        /// <summary>
        /// 获取左右边
        /// </summary>
        /// <remarks>
        /// 获取一系列矩形对象的左右边（从下往上）
        /// </remarks>
        /// <param name="rectangles">一些列矩形，存储每一个矩形的范围框</param>
        /// <param name="zValue"></param>
        /// <param name="precision"></param>
        /// <returns>如果失败，返回null</returns>
        private List<List<XYZ>> getRectanglesLeftRightBoundary1(List<XYZ[]> rectangles, double zValue, double precision = 1.0E-2)
        {
            List<List<XYZ>> outLine = new List<List<XYZ>>();

            if (rectangles == null || rectangles.Count() == 0) return null;

            //获取每一个左右边界集合
            List<XYZ[]> boundaries = new List<XYZ[]>();
            int i = 0;
            while (i < rectangles.Count)
            {
                XYZ[] boundingBox = rectangles[i];

                if (boundingBox == null || boundingBox.Length != 2)
                {
                    i++;
                    continue;
                }

                boundaries.Add(new XYZ[2] { new XYZ(boundingBox[0].X, boundingBox[0].Y, zValue), new XYZ(boundingBox[0].X, boundingBox[1].Y, zValue) });
                boundaries.Add(new XYZ[2] { new XYZ(boundingBox[1].X, boundingBox[0].Y, zValue), new XYZ(boundingBox[1].X, boundingBox[1].Y, zValue) });

                i++;
            }

            if (boundaries == null || boundaries.Count() < 2) return null;

            //分组
            var boundaryGroups = boundaries.GroupBy(x => x[0].Y);

            List<List<XYZ[]>> boundaryGroupsList = new List<List<XYZ[]>>();
            foreach (var boundaryGroup in boundaryGroups)
            {
                if (boundaryGroup == null || boundaryGroup.Count() == 0) continue;

                List<XYZ[]> boundaryGroupList = boundaryGroup.ToList();

                boundaryGroupList.Sort(delegate (XYZ[] p1, XYZ[] p2) { return p1[0].X.CompareTo(p2[0].X); });

                boundaryGroupsList.Add(boundaryGroupList);
            }

            //排序组
            boundaryGroupsList.Sort(delegate (List<XYZ[]> p1, List<XYZ[]> p2)
            {
                return p1[0][0].Y.CompareTo(p2[0][0].Y);
            });

            //搜集范围
            List<double[]> ranges = new List<double[]>();
            boundaryGroupsList.ForEach(x => ranges.Add(new double[2] { x.Min(y => y[0].Y), x.Max(y => y[1].Y) }));


            //合并组
            //根据轮廓
            List<List<XYZ[]>> finalBoundaryGroupsList = new List<List<XYZ[]>>();

            //取每一行的两边
            List<XYZ[]> leftBoundary = new List<XYZ[]>();
            List<XYZ[]> rightBoundary = new List<XYZ[]>();

            i = 0;
            int j = 0;
            while (i < boundaryGroupsList.Count())
            {
                double[] rangei = ranges[i];

                List<XYZ[]> iboundaryGroup = boundaryGroupsList[i];
                double minX = iboundaryGroup[0][0].X;
                double maxX = iboundaryGroup[iboundaryGroup.Count() - 1][1].X;

                j = i + 1;
                while (j < boundaryGroupsList.Count())
                {
                    double[] rangej = ranges[j];

                    List<XYZ[]> jboundaryGroup = boundaryGroupsList[j];

                    //两个范围没有交集，则直接退出
                    if (rangei[1] < rangej[0] || rangei[0] > rangej[1])
                    {
                        break;
                    }

                    //归并
                    //将j组归并到i组中
                    iboundaryGroup.AddRange(jboundaryGroup.ToList());

                    //重置rangei
                    rangei[0] = Math.Min(rangei[0], rangej[0]);
                    rangei[1] = Math.Max(rangei[1], rangej[1]);

                    //重置minX和maxX
                    minX = Math.Min(minX, jboundaryGroup[0][0].X);
                    maxX = Math.Max(maxX, jboundaryGroup[jboundaryGroup.Count() - 1][1].X);

                    //删除j组
                    boundaryGroupsList.RemoveAt(j);
                    ranges.RemoveAt(j);
                }

                finalBoundaryGroupsList.Add(iboundaryGroup);

                leftBoundary.Add(new XYZ[] { new XYZ(minX, rangei[0], zValue), new XYZ(minX, rangei[1], zValue) });
                rightBoundary.Add(new XYZ[] { new XYZ(maxX, rangei[0], zValue), new XYZ(maxX, rangei[1], zValue) });

                i++;
            }

            //构建轮廓
            List<XYZ> left = new List<XYZ>();
            List<XYZ> right = new List<XYZ>();

            left.Add(leftBoundary[0][0]);
            left.Add(leftBoundary[0][1]);
            double xCurrent = left[1].X;
            double yCurrent = left[1].Y;

            //左边
            i = 0;
            while (i < leftBoundary.Count() - 1)
            {
                XYZ[] leftBound = leftBoundary[i + 1];
                XYZ leftLower = leftBound[0];
                XYZ leftUpper = leftBound[1];

                if (Math.Abs(leftLower.X - xCurrent) >= precision)
                {
                    if (Math.Abs(leftLower.Y - yCurrent) >= precision)
                    {
                        if (leftLower.X < xCurrent)
                        {
                            left.Add(new XYZ(xCurrent, leftLower.Y, zValue));
                            left.Add(leftLower);
                            left.Add(leftUpper);
                        }
                        else
                        {
                            left.Add(new XYZ(leftLower.X, yCurrent, zValue));
                            left.Add(leftLower);
                            left.Add(leftUpper);
                        }
                    }
                    else
                    {
                        left.Add(leftLower);
                        left.Add(leftUpper);
                    }
                }
                else if (Math.Abs(leftLower.Y - yCurrent) >= precision)
                {
                    if (Math.Abs(leftLower.X - xCurrent) >= precision)
                    {
                        if (leftLower.X < xCurrent)
                        {
                            left.Add(new XYZ(xCurrent, leftLower.Y, zValue));
                            left.Add(leftLower);
                            left.Add(leftUpper);
                        }
                        else
                        {
                            left.Add(new XYZ(leftLower.X, yCurrent, zValue));
                            left.Add(leftLower);
                            left.Add(leftUpper);
                        }
                    }
                    else
                    {
                        left.Add(leftLower);
                        left.Add(leftUpper);
                    }
                }
                else
                {
                    left.Add(leftUpper);
                }

                xCurrent = leftUpper.X;
                yCurrent = leftUpper.Y;

                i++;
            }

            //left.Reverse();


            right.Add(rightBoundary[0][0]);
            right.Add(rightBoundary[0][1]);
            xCurrent = right[1].X;
            yCurrent = right[1].Y;
            //右边

            i = 0;
            while (i < rightBoundary.Count() - 1)
            {
                XYZ[] rightBound = rightBoundary[i + 1];
                XYZ rightLower = rightBound[0];
                XYZ rightUpper = rightBound[1];

                if (Math.Abs(rightLower.X - xCurrent) >= precision)
                {
                    if (Math.Abs(rightLower.Y - yCurrent) >= precision)
                    {
                        if (rightLower.X > xCurrent)
                        {
                            right.Add(new XYZ(xCurrent, rightLower.Y, zValue));
                            right.Add(rightLower);
                            right.Add(rightUpper);
                        }
                        else
                        {
                            right.Add(new XYZ(rightLower.X, yCurrent, zValue));
                            right.Add(rightLower);
                            right.Add(rightUpper);
                        }
                    }
                    else
                    {
                        right.Add(rightLower);
                        right.Add(rightUpper);
                    }
                }
                else if (Math.Abs(rightLower.Y - yCurrent) >= precision)
                {
                    if (Math.Abs(rightLower.X - xCurrent) >= precision)
                    {
                        if (rightLower.X > xCurrent)
                        {
                            right.Add(new XYZ(xCurrent, rightLower.Y, zValue));
                            right.Add(rightLower);
                            right.Add(rightUpper);
                        }
                        else
                        {
                            right.Add(new XYZ(rightLower.X, yCurrent, zValue));
                            right.Add(rightLower);
                            right.Add(rightUpper);
                        }
                    }
                    else
                    {
                        right.Add(rightLower);
                        right.Add(rightUpper);
                    }
                }
                else
                {
                    right.Add(rightUpper);
                }

                xCurrent = rightUpper.X;
                yCurrent = rightUpper.Y;

                i++;
            }



            outLine = new List<List<XYZ>>();
            outLine.Add(left);
            outLine.Add(right);

            return outLine;
        }

        /// <summary>
        /// 获取上下边
        /// </summary>
        /// <remarks>
        /// 获取一系列矩形对象的上下边（从左往右）
        /// </remarks>
        /// <param name="rectangles">一些列矩形，存储每一个矩形的范围框</param>
        /// <param name="zValue"></param>
        /// <param name="precision"></param>
        /// <returns></returns>
        private List<List<XYZ>> getRectanglesTopBottomBoundary1(List<XYZ[]> rectangles, double zValue, double precision = 1.0E-2)
        {
            List<List<XYZ>> outLine = new List<List<XYZ>>();

            if (rectangles == null || rectangles.Count() == 0) return null;

            //获取每一个上下边界集合
            List<XYZ[]> boundaries = new List<XYZ[]>();
            int i = 0;
            while (i < rectangles.Count)
            {
                XYZ[] boundingBox = rectangles[i];

                if (boundingBox == null || boundingBox.Length != 2)
                {
                    i++;
                    continue;
                }

                //下边
                boundaries.Add(new XYZ[2] { new XYZ(boundingBox[0].X, boundingBox[0].Y, zValue), new XYZ(boundingBox[1].X, boundingBox[0].Y, zValue) });
                //上边
                boundaries.Add(new XYZ[2] { new XYZ(boundingBox[0].X, boundingBox[1].Y, zValue), new XYZ(boundingBox[1].X, boundingBox[1].Y, zValue) });

                i++;
            }

            if (boundaries == null || boundaries.Count() < 2) return null;

            //分组
            var boundaryGroups = boundaries.GroupBy(x => x[0].X);

            List<List<XYZ[]>> boundaryGroupsList = new List<List<XYZ[]>>();
            foreach (var boundaryGroup in boundaryGroups)
            {
                if (boundaryGroup == null || boundaryGroup.Count() == 0) continue;

                List<XYZ[]> boundaryGroupList = boundaryGroup.ToList();

                boundaryGroupList.Sort(delegate (XYZ[] p1, XYZ[] p2) { return p1[0].Y.CompareTo(p2[0].Y); });

                boundaryGroupsList.Add(boundaryGroupList);
            }

            //排序组
            boundaryGroupsList.Sort(delegate (List<XYZ[]> p1, List<XYZ[]> p2)
            {
                return p1[0][0].X.CompareTo(p2[0][0].X);
            });

            //搜集范围
            List<double[]> ranges = new List<double[]>();
            boundaryGroupsList.ForEach(x => ranges.Add(new double[2] { x.Min(y => y[0].X), x.Max(y => y[1].X) }));


            //合并组
            //根据轮廓
            List<List<XYZ[]>> finalBoundaryGroupsList = new List<List<XYZ[]>>();

            //取每一行的两边
            List<XYZ[]> bottomBoundary = new List<XYZ[]>();
            List<XYZ[]> topBoundary = new List<XYZ[]>();

            i = 0;
            int j = 0;
            while (i < boundaryGroupsList.Count())
            {
                double[] rangei = ranges[i];

                List<XYZ[]> iboundaryGroup = boundaryGroupsList[i];
                double minY = iboundaryGroup[0][0].Y;
                double maxY = iboundaryGroup[iboundaryGroup.Count() - 1][1].Y;

                j = i + 1;
                while (j < boundaryGroupsList.Count())
                {
                    double[] rangej = ranges[j];

                    List<XYZ[]> jboundaryGroup = boundaryGroupsList[j];

                    //两个范围没有交集，则直接退出
                    if (rangei[1] < rangej[0] || rangei[0] > rangej[1])
                    {
                        break;
                    }

                    //归并
                    //将j组归并到i组中
                    iboundaryGroup.AddRange(jboundaryGroup.ToList());

                    //重置rangei
                    rangei[0] = Math.Min(rangei[0], rangej[0]);
                    rangei[1] = Math.Max(rangei[1], rangej[1]);

                    //重置minY和maxY
                    minY = Math.Min(minY, jboundaryGroup[0][0].Y);
                    maxY = Math.Max(maxY, jboundaryGroup[jboundaryGroup.Count() - 1][1].Y);

                    //删除j组
                    boundaryGroupsList.RemoveAt(j);
                    ranges.RemoveAt(j);
                }

                finalBoundaryGroupsList.Add(iboundaryGroup);

                bottomBoundary.Add(new XYZ[] { new XYZ(rangei[0], minY, zValue), new XYZ(rangei[1], minY, zValue) });
                topBoundary.Add(new XYZ[] { new XYZ(rangei[0], maxY, zValue), new XYZ(rangei[1], maxY, zValue) });

                i++;
            }

            //构建轮廓
            List<XYZ> bottom = new List<XYZ>();
            List<XYZ> top = new List<XYZ>();

            bottom.Add(bottomBoundary[0][0]);
            bottom.Add(bottomBoundary[0][1]);
            double xCurrent = bottom[1].X;
            double yCurrent = bottom[1].Y;

            //下边
            i = 0;
            while (i < bottomBoundary.Count() - 1)
            {
                XYZ[] bottomBound = bottomBoundary[i + 1];
                XYZ bottomLower = bottomBound[0];
                XYZ bottomUpper = bottomBound[1];

                if (Math.Abs(bottomLower.X - xCurrent) >= precision)
                {
                    if (Math.Abs(bottomLower.Y - yCurrent) >= precision)
                    {
                        if (bottomLower.Y < yCurrent)
                        {
                            bottom.Add(new XYZ(bottomLower.X, yCurrent, zValue));
                            bottom.Add(bottomLower);
                            bottom.Add(bottomUpper);
                        }
                        else
                        {
                            bottom.Add(new XYZ(xCurrent, bottomLower.Y, zValue));
                            bottom.Add(bottomLower);
                            bottom.Add(bottomUpper);
                        }
                    }
                    else
                    {
                        bottom.Add(bottomLower);
                        bottom.Add(bottomUpper);
                    }
                }
                else if (Math.Abs(bottomLower.Y - yCurrent) >= precision)
                {
                    if (Math.Abs(bottomLower.X - xCurrent) >= precision)
                    {
                        if (bottomLower.Y < yCurrent)
                        {
                            bottom.Add(new XYZ(bottomLower.X, yCurrent, zValue));
                            bottom.Add(bottomLower);
                            bottom.Add(bottomUpper);
                        }
                        else
                        {
                            bottom.Add(new XYZ(xCurrent, bottomLower.Y, zValue));
                            bottom.Add(bottomLower);
                            bottom.Add(bottomUpper);
                        }
                    }
                    else
                    {
                        bottom.Add(bottomLower);
                        bottom.Add(bottomUpper);
                    }
                }
                else
                {
                    bottom.Add(bottomUpper);
                }

                xCurrent = bottomUpper.X;
                yCurrent = bottomUpper.Y;

                i++;
            }


            top.Add(topBoundary[0][0]);
            top.Add(topBoundary[0][1]);
            xCurrent = top[1].X;
            yCurrent = top[1].Y;
            //上边

            i = 0;
            while (i < topBoundary.Count() - 1)
            {
                XYZ[] topBound = topBoundary[i + 1];
                XYZ topLower = topBound[0];
                XYZ topUpper = topBound[1];

                if (Math.Abs(topLower.X - xCurrent) >= precision)
                {
                    if (Math.Abs(topLower.Y - yCurrent) >= precision)
                    {
                        if (topLower.Y > yCurrent)
                        {
                            top.Add(new XYZ(topLower.X, yCurrent, zValue));
                            top.Add(topLower);
                            top.Add(topUpper);
                        }
                        else
                        {
                            top.Add(new XYZ(xCurrent, topLower.Y, zValue));
                            top.Add(topLower);
                            top.Add(topUpper);
                        }
                    }
                    else
                    {
                        top.Add(topLower);
                        top.Add(topUpper);
                    }
                }
                else if (Math.Abs(topLower.Y - yCurrent) >= precision)
                {
                    if (Math.Abs(topLower.X - xCurrent) >= precision)
                    {
                        if (topLower.Y > yCurrent)
                        {
                            top.Add(new XYZ(topLower.X, yCurrent, zValue));
                            top.Add(topLower);
                            top.Add(topUpper);
                        }
                        else
                        {
                            top.Add(new XYZ(xCurrent, topLower.Y, zValue));
                            top.Add(topLower);
                            top.Add(topUpper);
                        }
                    }
                    else
                    {
                        top.Add(topLower);
                        top.Add(topUpper);
                    }
                }
                else
                {
                    top.Add(topUpper);
                }

                xCurrent = topUpper.X;
                yCurrent = topUpper.Y;

                i++;
            }



            outLine = new List<List<XYZ>>();
            outLine.Add(bottom);
            outLine.Add(top);

            return outLine;
        }

        /// <summary>
        /// 获取下边
        /// </summary>
        /// <remarks>
        /// 获取一系列矩形对象的下边（从左往右）
        /// </remarks>
        /// <param name="rectangles">一些列矩形，存储每一个矩形的范围框</param>
        /// <param name="zValue"></param>
        /// <param name="precision"></param>
        /// <returns></returns>
        private List<XYZ> getRectanglesBottomBoundary1(List<XYZ[]> rectangles, double zValue, double precision = 1.0E-2)
        {
            if (rectangles == null || rectangles.Count() == 0) return null;

            //获取每一个上下边界集合
            List<XYZ[]> boundaries = new List<XYZ[]>();
            int i = 0;
            while (i < rectangles.Count)
            {
                XYZ[] boundingBox = rectangles[i];

                if (boundingBox == null || boundingBox.Length != 2)
                {
                    i++;
                    continue;
                }

                //下边
                boundaries.Add(new XYZ[2] { new XYZ(boundingBox[0].X, boundingBox[0].Y, zValue), new XYZ(boundingBox[1].X, boundingBox[0].Y, zValue) });
                //上边
                boundaries.Add(new XYZ[2] { new XYZ(boundingBox[0].X, boundingBox[1].Y, zValue), new XYZ(boundingBox[1].X, boundingBox[1].Y, zValue) });

                i++;
            }

            if (boundaries == null || boundaries.Count() < 2) return null;

            //分组
            var boundaryGroups = boundaries.GroupBy(x => x[0].X);

            List<List<XYZ[]>> boundaryGroupsList = new List<List<XYZ[]>>();
            foreach (var boundaryGroup in boundaryGroups)
            {
                if (boundaryGroup == null || boundaryGroup.Count() == 0) continue;

                List<XYZ[]> boundaryGroupList = boundaryGroup.ToList();

                boundaryGroupList.Sort(delegate (XYZ[] p1, XYZ[] p2) { return p1[0].Y.CompareTo(p2[0].Y); });

                boundaryGroupsList.Add(boundaryGroupList);
            }

            //排序组
            boundaryGroupsList.Sort(delegate (List<XYZ[]> p1, List<XYZ[]> p2)
            {
                return p1[0][0].X.CompareTo(p2[0][0].X);
            });

            //搜集范围
            List<double[]> ranges = new List<double[]>();
            boundaryGroupsList.ForEach(x => ranges.Add(new double[2] { x.Min(y => y[0].X), x.Max(y => y[1].X) }));


            //合并组
            //根据轮廓
            List<List<XYZ[]>> finalBoundaryGroupsList = new List<List<XYZ[]>>();

            //取每一行的两边
            List<XYZ[]> bottomBoundary = new List<XYZ[]>();
            List<XYZ[]> topBoundary = new List<XYZ[]>();

            i = 0;
            int j = 0;
            while (i < boundaryGroupsList.Count())
            {
                double[] rangei = ranges[i];

                List<XYZ[]> iboundaryGroup = boundaryGroupsList[i];
                double minY = iboundaryGroup[0][0].Y;
                double maxY = iboundaryGroup[iboundaryGroup.Count() - 1][1].Y;

                j = i + 1;
                while (j < boundaryGroupsList.Count())
                {
                    double[] rangej = ranges[j];

                    List<XYZ[]> jboundaryGroup = boundaryGroupsList[j];

                    //两个范围没有交集，则直接退出
                    if (rangei[1] < rangej[0] || rangei[0] > rangej[1])
                    {
                        break;
                    }

                    //归并
                    //将j组归并到i组中
                    iboundaryGroup.AddRange(jboundaryGroup.ToList());

                    //重置rangei
                    rangei[0] = Math.Min(rangei[0], rangej[0]);
                    rangei[1] = Math.Max(rangei[1], rangej[1]);

                    //重置minY和maxY
                    minY = Math.Min(minY, jboundaryGroup[0][0].Y);
                    maxY = Math.Max(maxY, jboundaryGroup[jboundaryGroup.Count() - 1][1].Y);

                    //删除j组
                    boundaryGroupsList.RemoveAt(j);
                    ranges.RemoveAt(j);
                }

                finalBoundaryGroupsList.Add(iboundaryGroup);

                bottomBoundary.Add(new XYZ[] { new XYZ(rangei[0], minY, zValue), new XYZ(rangei[1], minY, zValue) });
                topBoundary.Add(new XYZ[] { new XYZ(rangei[0], maxY, zValue), new XYZ(rangei[1], maxY, zValue) });

                i++;
            }

            //构建轮廓
            List<XYZ> bottom = new List<XYZ>();

            bottom.Add(bottomBoundary[0][0]);
            bottom.Add(bottomBoundary[0][1]);
            double xCurrent = bottom[1].X;
            double yCurrent = bottom[1].Y;

            //下边
            i = 0;
            while (i < bottomBoundary.Count() - 1)
            {
                XYZ[] bottomBound = bottomBoundary[i + 1];
                XYZ bottomLower = bottomBound[0];
                XYZ bottomUpper = bottomBound[1];

                if (Math.Abs(bottomLower.X - xCurrent) >= precision)
                {
                    if (Math.Abs(bottomLower.Y - yCurrent) >= precision)
                    {
                        if (bottomLower.Y < yCurrent)
                        {
                            bottom.Add(new XYZ(bottomLower.X, yCurrent, zValue));
                            bottom.Add(bottomLower);
                            bottom.Add(bottomUpper);
                        }
                        else
                        {
                            bottom.Add(new XYZ(xCurrent, bottomLower.Y, zValue));
                            bottom.Add(bottomLower);
                            bottom.Add(bottomUpper);
                        }
                    }
                    else
                    {
                        bottom.Add(bottomLower);
                        bottom.Add(bottomUpper);
                    }
                }
                else if (Math.Abs(bottomLower.Y - yCurrent) >= precision)
                {
                    if (Math.Abs(bottomLower.X - xCurrent) >= precision)
                    {
                        if (bottomLower.Y < yCurrent)
                        {
                            bottom.Add(new XYZ(bottomLower.X, yCurrent, zValue));
                            bottom.Add(bottomLower);
                            bottom.Add(bottomUpper);
                        }
                        else
                        {
                            bottom.Add(new XYZ(xCurrent, bottomLower.Y, zValue));
                            bottom.Add(bottomLower);
                            bottom.Add(bottomUpper);
                        }
                    }
                    else
                    {
                        bottom.Add(bottomLower);
                        bottom.Add(bottomUpper);
                    }
                }
                else
                {
                    bottom.Add(bottomUpper);
                }

                xCurrent = bottomUpper.X;
                yCurrent = bottomUpper.Y;

                i++;
            }

            return bottom;
        }

        /// <summary>
        /// 获取上边
        /// </summary>
        /// <remarks>
        /// 获取一系列矩形对象的上边（从左往右）
        /// </remarks>
        /// <param name="rectangles">一些列矩形，存储每一个矩形的范围框</param>
        /// <param name="zValue"></param>
        /// <param name="precision"></param>
        /// <returns></returns>
        private List<XYZ> getRectanglesTopBoundary1(List<XYZ[]> rectangles, double zValue, double precision = 1.0E-2)
        {
            if (rectangles == null || rectangles.Count() == 0) return null;

            //获取每一个上下边界集合
            List<XYZ[]> boundaries = new List<XYZ[]>();
            int i = 0;
            while (i < rectangles.Count)
            {
                XYZ[] boundingBox = rectangles[i];

                if (boundingBox == null || boundingBox.Length != 2)
                {
                    i++;
                    continue;
                }

                //下边
                boundaries.Add(new XYZ[2] { new XYZ(boundingBox[0].X, boundingBox[0].Y, zValue), new XYZ(boundingBox[1].X, boundingBox[0].Y, zValue) });
                //上边
                boundaries.Add(new XYZ[2] { new XYZ(boundingBox[0].X, boundingBox[1].Y, zValue), new XYZ(boundingBox[1].X, boundingBox[1].Y, zValue) });

                i++;
            }

            if (boundaries == null || boundaries.Count() < 2) return null;

            //分组
            var boundaryGroups = boundaries.GroupBy(x => x[0].X);

            List<List<XYZ[]>> boundaryGroupsList = new List<List<XYZ[]>>();
            foreach (var boundaryGroup in boundaryGroups)
            {
                if (boundaryGroup == null || boundaryGroup.Count() == 0) continue;

                List<XYZ[]> boundaryGroupList = boundaryGroup.ToList();

                boundaryGroupList.Sort(delegate (XYZ[] p1, XYZ[] p2) { return p1[0].Y.CompareTo(p2[0].Y); });

                boundaryGroupsList.Add(boundaryGroupList);
            }

            //排序组
            boundaryGroupsList.Sort(delegate (List<XYZ[]> p1, List<XYZ[]> p2)
            {
                return p1[0][0].X.CompareTo(p2[0][0].X);
            });

            //搜集范围
            List<double[]> ranges = new List<double[]>();
            boundaryGroupsList.ForEach(x => ranges.Add(new double[2] { x.Min(y => y[0].X), x.Max(y => y[1].X) }));


            //合并组
            //根据轮廓
            List<List<XYZ[]>> finalBoundaryGroupsList = new List<List<XYZ[]>>();

            //取每一行的两边
            List<XYZ[]> bottomBoundary = new List<XYZ[]>();
            List<XYZ[]> topBoundary = new List<XYZ[]>();

            i = 0;
            int j = 0;
            while (i < boundaryGroupsList.Count())
            {
                double[] rangei = ranges[i];

                List<XYZ[]> iboundaryGroup = boundaryGroupsList[i];
                double minY = iboundaryGroup[0][0].Y;
                double maxY = iboundaryGroup[iboundaryGroup.Count() - 1][1].Y;

                j = i + 1;
                while (j < boundaryGroupsList.Count())
                {
                    double[] rangej = ranges[j];

                    List<XYZ[]> jboundaryGroup = boundaryGroupsList[j];

                    //两个范围没有交集，则直接退出
                    if (rangei[1] < rangej[0] || rangei[0] > rangej[1])
                    {
                        break;
                    }

                    //归并
                    //将j组归并到i组中
                    iboundaryGroup.AddRange(jboundaryGroup.ToList());

                    //重置rangei
                    rangei[0] = Math.Min(rangei[0], rangej[0]);
                    rangei[1] = Math.Max(rangei[1], rangej[1]);

                    //重置minY和maxY
                    minY = Math.Min(minY, jboundaryGroup[0][0].Y);
                    maxY = Math.Max(maxY, jboundaryGroup[jboundaryGroup.Count() - 1][1].Y);

                    //删除j组
                    boundaryGroupsList.RemoveAt(j);
                    ranges.RemoveAt(j);
                }

                finalBoundaryGroupsList.Add(iboundaryGroup);

                bottomBoundary.Add(new XYZ[] { new XYZ(rangei[0], minY, zValue), new XYZ(rangei[1], minY, zValue) });
                topBoundary.Add(new XYZ[] { new XYZ(rangei[0], maxY, zValue), new XYZ(rangei[1], maxY, zValue) });

                i++;
            }

            //构建轮廓
            List<XYZ> top = new List<XYZ>();

            top.Add(topBoundary[0][0]);
            top.Add(topBoundary[0][1]);
            double xCurrent = top[1].X;
            double yCurrent = top[1].Y;
            //上边

            i = 0;
            while (i < topBoundary.Count() - 1)
            {
                XYZ[] topBound = topBoundary[i + 1];
                XYZ topLower = topBound[0];
                XYZ topUpper = topBound[1];

                if (Math.Abs(topLower.X - xCurrent) >= precision)
                {
                    if (Math.Abs(topLower.Y - yCurrent) >= precision)
                    {
                        if (topLower.Y > yCurrent)
                        {
                            top.Add(new XYZ(topLower.X, yCurrent, zValue));
                            top.Add(topLower);
                            top.Add(topUpper);
                        }
                        else
                        {
                            top.Add(new XYZ(xCurrent, topLower.Y, zValue));
                            top.Add(topLower);
                            top.Add(topUpper);
                        }
                    }
                    else
                    {
                        top.Add(topLower);
                        top.Add(topUpper);
                    }
                }
                else if (Math.Abs(topLower.Y - yCurrent) >= precision)
                {
                    if (Math.Abs(topLower.X - xCurrent) >= precision)
                    {
                        if (topLower.Y > yCurrent)
                        {
                            top.Add(new XYZ(topLower.X, yCurrent, zValue));
                            top.Add(topLower);
                            top.Add(topUpper);
                        }
                        else
                        {
                            top.Add(new XYZ(xCurrent, topLower.Y, zValue));
                            top.Add(topLower);
                            top.Add(topUpper);
                        }
                    }
                    else
                    {
                        top.Add(topLower);
                        top.Add(topUpper);
                    }
                }
                else
                {
                    top.Add(topUpper);
                }

                xCurrent = topUpper.X;
                yCurrent = topUpper.Y;

                i++;
            }

            return top;
        }

        /// <summary>
        /// 获取下边
        /// </summary>
        /// <remarks>
        /// 获取一系列矩形对象的下边（从左往右）
        /// </remarks>
        /// <param name="rectangles">一些列矩形，存储每一个矩形的范围框</param>
        /// <param name="line">投影边</param>
        /// <param name="zValue"></param>
        /// <param name="precision"></param>
        /// <returns></returns>
        private List<XYZ> getRectanglesBottomBoundary2(List<XYZ[]> rectangles, XYZ[] line, double zValue, double precision = 1.0E-2)
        {
            if (rectangles == null || rectangles.Count() == 0) return null;

            //获取每一个上下边界集合
            List<XYZ> points = new List<XYZ>();
            List<XYZ[]> bottomBoundaries = new List<XYZ[]>();
            List<XYZ[]> btopBoundaries = new List<XYZ[]>();
            int i = 0;
            while (i < rectangles.Count)
            {
                XYZ[] boundingBox = rectangles[i];

                if (boundingBox == null || boundingBox.Length != 2)
                {
                    i++;
                    continue;
                }

                XYZ bottomLeft = new XYZ(boundingBox[0].X, boundingBox[0].Y, zValue);
                XYZ bottomRight = new XYZ(boundingBox[1].X, boundingBox[0].Y, zValue);

                XYZ topLeft = new XYZ(boundingBox[0].X, boundingBox[1].Y, zValue);
                XYZ topRight = new XYZ(boundingBox[1].X, boundingBox[1].Y, zValue);

                //下边
                bottomBoundaries.Add(new XYZ[2] { bottomLeft, bottomRight });

                //上边
                btopBoundaries.Add(new XYZ[2] { topLeft, topRight });

                points.Add(bottomLeft);
                points.Add(bottomRight);
                i++;
            }

            points.Sort(delegate (XYZ p1, XYZ p2) { return p1.X.CompareTo(p2.X); });

            i = 0;
            int j = 0;
            while (i < points.Count)
            {
                XYZ point = points[i];

                j = 0;
                while (j < bottomBoundaries.Count)
                {
                    XYZ[] bottomBoundary = bottomBoundaries[j];




                    i++;
                }


                i++;
            }





            return null;
        }








        #endregion



        #region Helper Methods


        #endregion


        #region Properties


        #endregion




    }
}
