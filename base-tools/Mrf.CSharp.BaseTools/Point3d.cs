
using Mrf.CSharp.BaseTools.Extension;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;


namespace Mrf.CSharp.BaseTools
{
    /// <summary>
    /// 三维点坐标
    /// </summary>
    public class Point3d
    {


        /// <summary>
        /// 构造函数
        /// </summary>
        public Point3d()
        {

        }


        /// <summary>
        /// 构造函数 
        /// </summary>
        /// <param name="arr">三维数组</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public Point3d(double[] arr)
        {
            if (arr == null)
            {
                throw new ArgumentNullException(nameof(arr));
            }

            if (arr.Length != 3)
            {
                throw new ArgumentException("数组的维数应该为3");
            }

            x = arr[0];
            y = arr[1];
            z = arr[2];


            _point3dArr[0] = arr[0];
            _point3dArr[1] = arr[1];
            _point3dArr[2] = arr[2];

            _count = 3;


            _length = GetLength();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="xValue">x坐标值</param>
        /// <param name="yValue">y坐标值</param>
        /// <param name="zValue">z坐标值</param>
        public Point3d(double xValue, double yValue, double zValue)
        {
            x = xValue;
            y = yValue;
            z = zValue;

            _point3dArr[0] = xValue;
            _point3dArr[1] = yValue;
            _point3dArr[2] = zValue;

            _count = 3;

            _length = GetLength();
        }







        private double[] _point3dArr = new double[3];




        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public double this[int i]
        {
            get => _point3dArr[i];

            set
            {
                _point3dArr[i] = value;

                switch (i)
                {
                    case 0:
                        x = value;
                        break;

                    case 1:
                        y = value;
                        break;

                    case 2:
                        z = value;
                        break;
                }

            }
        }




        /// <summary>
        /// 原点坐标
        /// </summary>
        public static Point3d Zero => new Point3d(0, 0, 0);

        /// <summary>
        /// x方向坐标
        /// </summary>
        public static Point3d BasisX => new Point3d(1, 0, 0);


        /// <summary>
        /// y方向坐标
        /// </summary>
        public static Point3d BasisY => new Point3d(0, 1, 0);


        /// <summary>
        /// z方向坐标
        /// </summary>
        public static Point3d BasisZ => new Point3d(0, 0, 1);




        private int _count;
        /// <summary>
        /// 元素总个数
        /// </summary>
        [JsonIgnore]
        public int Count
        {
            get { return _count; }

        }



        private double _length;
        /// <summary>
        /// 到原点的长度
        /// </summary>
        [JsonIgnore]
        public double Length
        {
            get { return _length; }

        }






        private double x;
        /// <summary>
        /// X坐标
        /// </summary>
        public double X
        {
            get { return x; }
            set
            {
                x = value;
                _point3dArr[0] = value;
            }
        }

        private double y;
        /// <summary>
        /// Y坐标
        /// </summary>
        public double Y
        {
            get { return y; }
            set
            {
                y = value;
                _point3dArr[1] = value;
            }
        }

        private double z;
        /// <summary>
        /// Z坐标
        /// </summary>
        public double Z
        {
            get { return z; }
            set
            {
                z = value;
                _point3dArr[2] = value;
            }
        }

        /// <summary>
        /// 获取距离
        /// </summary>
        public double DistanceTo(Point3d b)
        {
            double length = 0;
            if (b != null)
            {
                length = Math.Sqrt((X - b.X) * (X - b.X) + (Y - b.Y) * (Y - b.Y) + (Z - b.Z) * (Z - b.Z));
            }
            return length;
        }



        /// <summary>
        /// 相加
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns></returns>
        public static Point3d operator +(Point3d point1, Point3d point2)
        {
            return new Point3d(point1.X + point2.X, point1.Y + point2.Y, point1.Z + point2.Z);
        }



        /// <summary>
        /// 相减
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns></returns>
        public static Point3d operator -(Point3d point1, Point3d point2)
        {
            return new Point3d(point1.X - point2.X, point1.Y - point2.Y, point1.Z - point2.Z);
        }


        /// <summary>
        /// 取反方向
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Point3d operator -(Point3d point)
        {
            return new Point3d(-point.X, -point.Y, -point.Z);
        }





        /// <summary>
        /// 相乘
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns></returns>
        public static Point3d operator *(Point3d point1, Point3d point2)
        {
            return new Point3d(point1.X * point2.X, point1.Y * point2.Y, point1.Z * point2.Z);
        }




        /// <summary>
        /// 相除
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns></returns>
        public static Point3d operator /(Point3d point1, Point3d point2)
        {
            if (point1 == null)
            {
                throw new ArgumentNullException(nameof(point1));
            }

            if (point2 == null)
            {
                throw new ArgumentNullException(nameof(point2));
            }

            if (point2.X == 0 || point2.Y == 0 || point2.Z == 0)
            {
                new ArgumentException("除数不能为0", nameof(point2));
            }

            return new Point3d(point1.X / point2.X, point1.Y / point2.Y, point1.Z / point2.Z);
        }




        /// <summary>
        /// 相乘
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="scale">倍数</param>
        /// <returns></returns>
        public static Point3d operator *(Point3d point1, double scale)
        {
            return new Point3d(point1.X * scale, point1.Y * scale, point1.Z * scale);
        }


        /// <summary>
        /// 相乘
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="scale">倍数</param>
        /// <returns></returns>
        public static Point3d operator *(double scale, Point3d point1)
        {
            return new Point3d(point1.X * scale, point1.Y * scale, point1.Z * scale);
        }




        /// <summary>
        /// 相除
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static Point3d operator /(Point3d point1, double scale)
        {
            if (scale == 0)
            {
                new ArgumentException("除数不能为0", nameof(scale));
            }
            return new Point3d(point1.X / scale, point1.Y / scale, point1.Z / scale);
        }



        /// <summary>
        /// x值增加,不修改源对象 返回新的对象
        /// </summary>
        /// <param name="xValue"></param>
        /// <returns></returns>
        public Point3d AddByXValue(double xValue)
        {
            return new Point3d(X + xValue, Y, Z);
        }


        /// <summary>
        /// y值增加,不修改源对象 返回新的对象
        /// </summary>
        /// <param name="yValue"></param>
        /// <returns></returns>
        public Point3d AddByYValue(double yValue)
        {
            return new Point3d(X, Y + yValue, Z);
        }


        /// <summary>
        /// z值增加,不修改源对象 返回新的对象
        /// </summary>
        /// <param name="zValue"></param>
        /// <returns></returns>
        public Point3d AddByZValue(double zValue)
        {
            return new Point3d(X, Y, Z + zValue);
        }




        /// <summary>
        /// mm转换为英尺
        /// </summary>
        /// <returns></returns>
        public Point3d Millimeter2Foot()
        {
            return new Point3d(X.Millimeter2Foot(), Y.Millimeter2Foot(), Z.Millimeter2Foot());
        }


        /// <summary>
        /// 英尺转换为mm
        /// </summary>
        /// <returns></returns>
        public Point3d Foot2Millimeter()
        {
            return new Point3d(X.Foot2Millimeter(), Y.Foot2Millimeter(), Z.Foot2Millimeter());
        }


        /// <summary>
        /// mm转换为m
        /// </summary>
        /// <returns></returns>
        public Point3d Millimeter2Meter()
        {
            return new Point3d(X.Millimeter2Meter(), Y.Millimeter2Meter(), Z.Millimeter2Meter());
        }





        /// <summary>
        /// 获取极坐标
        /// </summary>
        /// <param name="distance">距离</param>
        /// <param name="angle">角度，弧度制</param>
        /// <returns></returns>
        public Point3d GetPolarPointInYZPlane(double distance, double angle)
        {
            return new Point3d(x, y + distance * Math.Cos(angle), z + distance * Math.Sin(angle));
        }



        /// <summary>
        /// 复制成新的点坐标
        /// </summary>
        /// <returns>新的点坐标，跟源对象没关系</returns>
        public Point3d ToCopy()
        {
            Point3d result = new Point3d(X, Y, Z);
            return result;
        }




        /// <summary>
        /// 获取单位方向向量
        /// </summary>
        /// <returns>如果失败，返回null</returns>
        public Point3d GetNormal()
        {
            double length = GetLength();
            if (length <= 0 || length.AreEqual(0)) //给一个误差
            {
                return null;
            }

            Point3d normal = new Point3d(x, y, z) / length;

            return normal;
        }


        /// <summary>
        /// 获取到原点的长度距离
        /// </summary>
        /// <returns></returns>
        public double GetLength()
        {
            double length = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2));
            return length;
        }



        /// <summary>
        /// 点乘
        /// </summary>
        /// <returns></returns>
        public double DotProduct(Point3d secondPoint)
        {
            double result = x * secondPoint.X + y * secondPoint.Y + z * secondPoint.Z;
            return result;
        }




        /// <summary>
        /// 判断是否是0长度
        /// </summary>
        /// <returns>如果是，返回true,否则，返回false</returns>
        public bool IsZeroLength()
        {
            //返回值
            bool isZero = false;

            double length = GetLength();
            if (length <= 0 || length.AreEqual(0))
            {
                isZero = true;
            }

            return isZero;
        }







        /*
         * https://www.geeksforgeeks.org/check-if-two-vectors-are-collinear-or-not/?ref=gcse
         * 
         */



        /// <summary>
        /// Function to calculate cross product of two vectors 计算两个向量的叉乘
        /// </summary>
        /// <param name="vect_A"></param>
        /// <param name="vect_B"></param>
        /// <param name="cross_P"></param>
        private void CrossProduct(double[] vect_A,
                                 double[] vect_B,
                                 double[] cross_P)
        {

            // Update cross_P[0]
            cross_P[0] = vect_A[1] * vect_B[2] -
                         vect_A[2] * vect_B[1];

            // Update cross_P[1]
            cross_P[1] = vect_A[2] * vect_B[0] -
                         vect_A[0] * vect_B[2];

            // Update cross_P[2]
            cross_P[2] = vect_A[0] * vect_B[1] -
                         vect_A[1] * vect_B[0];
        }







        /// <summary>
        /// Function to check if two given  vectors are collinear or not 判断两个向量是否共线,如果两者的叉乘为null，及叉乘的x、y和z都为0，则共线
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="z1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="z2"></param>
        /// <param name="tolerance">误差</param>
        /// <returns></returns>
        private bool CheckCollinearity(double x1, double y1,
                                      double z1, double x2,
                                      double y2, double z2
            , double tolerance = 1E-06)
        {

            //返回值
            bool isCollinear = false;

            // Store the first and second vectors
            double[] A = { x1, y1, z1 };
            double[] B = { x2, y2, z2 };

            // Store their cross product
            double[] cross_P = new double[3];

            // Calculate their cross product
            CrossProduct(A, B, cross_P);

            // Check if their cross product
            // is a NULL Vector or not
            if (cross_P[0].AreEqual(0, tolerance) && cross_P[1].AreEqual(0, tolerance) &&
                cross_P[2].AreEqual(0, tolerance))
                isCollinear = true;

            return isCollinear;

        }




        /// <summary>
        /// 判断两个向量是否共线
        /// </summary>
        /// <param name="secondVector">指定向量</param>
        /// <param name="tolerance">误差</param>
        /// <returns>如果共线，返回true，否则，返回false</returns>
        public bool CheckCollinearity(Point3d secondVector, double tolerance = 1E-06)
        {

            //返回值
            bool isCollinear = CheckCollinearity(x, y, z, secondVector.X, secondVector.Y, secondVector.Z, tolerance);

            return isCollinear;

        }


        public override string ToString()
        {
            return base.ToString();
        }




        /// <summary>
        /// 使用射线法判断一个点是否在一个封闭的区域内：计算通过点的射线与多段线边界的交点个数，通过判断交点个数的奇偶性来确定点是否在多段线内。如果交点个数是奇数，那么点在多段线内，否则在多段线外。
        /// </summary>
        /// <param name="boundingBoxPointLst"></param>
        /// <returns></returns>
        public bool IsInsideBoundingBox(List<Point3d> boundingBoxPointLst)
        {
            int numCrossings = 0;
            int numVertices = boundingBoxPointLst.Count;

            for (int i = 0; i < numVertices; i++)
            {
                int j = (i + 1) % numVertices;
                Point3d vertex1 = boundingBoxPointLst[i];
                Point3d vertex2 = boundingBoxPointLst[j];

                if (((vertex1.Y > Y) != (vertex2.Y > Y)) &&
                    (X < (vertex2.X - vertex1.X) * (Y - vertex1.Y) / (vertex2.Y - vertex1.Y) + vertex1.X))
                {
                    numCrossings++;
                }
            }

            return (numCrossings % 2 != 0);
        }






        /// <summary>
        /// 判断两个点是否相等
        /// </summary>
        /// <param name="secondPoint">第二个点</param>
        /// <param name="tolerance">误差</param>
        /// <returns>如果相等，返回true，否则，返回false</returns>
        public bool AreEqual(Point3d secondPoint, double tolerance = 1E-06)
        {
            return X.AreEqual(secondPoint.X, tolerance)&& Y.AreEqual(secondPoint.Y, tolerance)&& Z.AreEqual(secondPoint.Z, tolerance);
        }




    }




    /// <summary>
    /// Point3d类的自定义比较器 这个比较器的结果待检查
    /// </summary>
    public class Point3dComparer : IEqualityComparer<Point3d>
    {
        // Products are equal if their names and product numbers are equal.

        /// <summary>
        /// 如果两者指向同一个引用，或者x、y和z分别相等，则两者相等
        /// </summary>
        /// <param name="firstPoint"></param>
        /// <param name="secondPoint"></param>
        /// <returns></returns>
        public bool Equals(Point3d firstPoint, Point3d secondPoint)
        {

            //Check whether the compared objects reference the same data.
            if (ReferenceEquals(firstPoint, secondPoint)) return true;

            //Check whether any of the compared objects is null.
            if (firstPoint is null || secondPoint is null)
                return false;

            //Check whether the products' properties are equal.
            return firstPoint.X.AreEqual(secondPoint.X) &&
             firstPoint.Y.AreEqual(secondPoint.Y) &&
             firstPoint.Z.AreEqual(secondPoint.Z);

        }

        // If Equals() returns true for a pair of objects
        // then GetHashCode() must return the same value for these objects.

        public int GetHashCode(Point3d point)
        {
            //Check whether the object is null
            if (point is null) return 0;

            int hashProductCode = point.ToString().GetHashCode();
            //Calculate the hash code for the product.
            return hashProductCode;
        }




    }








}
