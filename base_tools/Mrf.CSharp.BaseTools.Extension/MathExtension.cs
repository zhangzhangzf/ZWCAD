using System;
using System.Collections.Generic;

namespace Mrf.CSharp.BaseTools.Extension
{
    /// <summary>
    /// 数学工具扩展
    /// </summary>
    public static partial class MathExtension
    {

        // Fields 浮点型的误差
        private const double DOUBLE_DELTA = 1E-06;


        /// <summary>
        /// 判断两个浮点数是否相等
        /// </summary>
        /// <param name="value1">浮点数1</param>
        /// <param name="value2">浮点数2</param>
        /// <param name="tolerance">误差</param>
        /// <returns>true or false</returns>
        public static bool AreEqual(this double value1, double value2, double tolerance = DOUBLE_DELTA)
        {
            return value1 == value2
                || Math.Abs(value1 - value2) < tolerance;
        }


        /// <summary>
        /// 第一个浮点数是否大于第二个浮点数
        /// </summary>
        /// <param name="value1">第一个浮点数</param>
        /// <param name="value2">第二个浮点数</param>
        /// <param name="tolerance">误差</param>
        /// <returns>true or false</returns>
        public static bool GreaterThan(this double value1, double value2, double tolerance = DOUBLE_DELTA)
        {
            return value1 > value2 && !AreEqual(value1, value2, tolerance);
        }


        /// <summary>
        /// 判断第一个浮点数是否大于等于第二个浮点数
        /// </summary>
        /// <param name="value1">第一个浮点数</param>
        /// <param name="value2">第二个浮点数</param>
        /// <param name="tolerance">误差</param>
        /// <returns>true or false</returns>
        public static bool GreaterThanOrEqual(this double value1, double value2, double tolerance = DOUBLE_DELTA)
        {
            return value1 > value2 || AreEqual(value1, value2, tolerance);
        }


        /// <summary>
        /// 判断浮点数是否为0
        /// </summary>
        /// <param name="value">浮点数</param>
        /// <param name="tolerance">误差</param>
        /// <returns>true or false</returns>
        public static bool IsZero(this double value, double tolerance = DOUBLE_DELTA)
        {
            return Math.Abs(value) < tolerance;
        }


        /// <summary>
        /// 判断第一个浮点数是否小于第二个浮点数
        /// </summary>
        /// <param name="value1">第一个浮点数</param>
        /// <param name="value2">第二个浮点数</param>
        /// <param name="tolerance">误差</param>
        /// <returns>true or false</returns>
        public static bool LessThan(this double value1, double value2, double tolerance = DOUBLE_DELTA)
        {
            return value1 < value2 && !AreEqual(value1, value2, tolerance);
        }


        /// <summary>
        /// 判断第一个浮点数是否小于等于第二个浮点数
        /// </summary>
        /// <param name="value1">第一个浮点数</param>
        /// <param name="value2">第二个浮点数</param>
        /// <param name="tolerance">误差</param>
        /// <returns>true or false</returns>
        public static bool LessThanOrEqual(this double value1, double value2, double tolerance = DOUBLE_DELTA)
        {
            return value1 < value2 || AreEqual(value1, value2, tolerance);
        }







        /// <summary>
        /// 角度转换为弧度
        /// </summary>
        /// <param name="degree">角度值</param>
        /// <returns>弧度值</returns>
        public static double DegreeToAngle(this double degree)
        {
            return degree * Math.PI / 180;
        }

        /// <summary>
        /// 弧度转换为角度值
        /// </summary>
        /// <param name="angle">弧度值</param>
        /// <returns>角度值</returns>
        public static double AngleToDegree(this double angle)
        {
            return angle * 180 / Math.PI;
        }






        /// <summary>
        /// 求二维向量与x轴的夹角（单位：弧度），范围0~2pi
        /// </summary>
        /// <param name="vector2DX">二维向量的x值</param>
        /// <param name="vector2DY">二维向量的y值</param>
        /// <returns>角度弧度制</returns>
        public static double Vector2DToAngle(double vector2DX, double vector2DY)
        {
            double angle = Math.Atan2(vector2DY, vector2DX);

            if (angle < 0)  //负数，转换为整数
            {
                angle = Math.PI * 2 + angle;
            }

            return angle;
        }


        /// <summary>
        /// 将与x轴的夹角（单位：弧度）转换为二维单位向量
        /// </summary>
        /// <param name="angle">与x轴的夹角（单位：弧度）</param>
        /// <returns>二维单位向量</returns>
        public static double[] AngleToVector2D(double angle)
        {

            double vector2DX = Math.Cos(angle);
            double vector2DY = Math.Sin(angle);

            double[] vector2D = new double[] { vector2DX, vector2DY };

            return vector2D;
        }



        /// <summary>
        /// 将与x轴的夹角（单位：弧度）转换为三维单位向量
        /// </summary>
        /// <param name="angle">与x轴的夹角（单位：弧度）</param>
        /// <param name="planeName">平面的名称，不分大小写</param>
        /// <returns>三维单位向量</returns>
        public static double[] AngleToVector3D(double angle, string planeName = "XY")
        {

            double[] vector3D;

            double[] vector2D = AngleToVector2D(angle);

            if (!string.IsNullOrEmpty(planeName))
            {
                planeName = planeName.ToUpper();
            }

            switch (planeName)
            {
                case "XZ":
                    vector3D = new double[] { vector2D[0], 0, vector2D[1] };
                    break;

                case "YZ":
                    vector3D = new double[] { 0, vector2D[0], vector2D[1] };
                    break;
                default:
                    vector3D = new double[] { vector2D[0], vector2D[1], 0 };
                    break;
            }

            return vector3D;
        }














        /// <summary>
        /// 获取起点二维点指向终点二维点的角度（单位：弧度），范围0~2pi
        /// </summary>
        /// <param name="startPoint2D">起点二维点</param>
        /// <param name="endPoint2D">终点二维点</param>
        /// <returns>角度弧度制，如果不为二维点，返回double.NaN</returns>
        public static double GetPoint2DAngle(double[] startPoint2D, double[] endPoint2D)
        {
            double angle = double.NaN;
            if (startPoint2D.Length != 2 || endPoint2D.Length != 2) //不符合要求
            {
                return angle;
            }

            double vector2DX = endPoint2D[0] - startPoint2D[0];
            double vector2DY = endPoint2D[1] - startPoint2D[1];

            angle = Vector2DToAngle(vector2DX, vector2DY);
            return angle;

        }








        /// <summary>
        /// 求三维向量与指定平面的夹角（单位：弧度），范围0~2pi
        /// </summary>
        /// <param name="vector3DX">三维向量的x值</param>
        /// <param name="vector3DY">三维向量的y值</param>
        /// <param name="vector3DZ">三维向量的z值</param>
        /// <param name="planeName">平面名称，忽略大小写</param>
        /// <returns>角度弧度制</returns>
        public static double Vector3DToAngleInPlane(double vector3DX, double vector3DY, double vector3DZ, string planeName = "XY")
        {
            //返回值
            double angle;

            string planeNameToUpper = planeName;
            //处理大小写的问题
            if (!string.IsNullOrEmpty(planeNameToUpper))
            {
                planeNameToUpper = planeNameToUpper.ToUpper();
            }


            switch (planeNameToUpper)
            {
                case "XZ": //XZ平面
                    angle = Vector2DToAngle(vector3DX, vector3DZ);
                    break;

                case "YZ": //YZ平面
                    angle = Vector2DToAngle(vector3DY, vector3DZ);
                    break;

                default://默认XY平面
                    angle = Vector2DToAngle(vector3DX, vector3DY);
                    break;
            }

            return angle;
        }






        /// <summary>
        /// 获取起点二维点指向终点二维点的角度（单位：弧度），范围0~2pi
        /// </summary>
        /// <param name="startPoint3D">起点三维点</param>
        /// <param name="endPoint3D">终点三维点</param>
        /// <param name="planeName">平面名称，忽略大小写</param>
        /// <returns>角度弧度制，如果不为三维点，返回double.NaN</returns>
        public static double GetPoint3DAngleInPlane(double[] startPoint3D, double[] endPoint3D, string planeName = "XY")
        {
            double angle = double.NaN;
            if (startPoint3D.Length != 3 || endPoint3D.Length != 3) //不符合要求
            {
                return angle;
            }

            double vector3DX = endPoint3D[0] - startPoint3D[0];
            double vector3DY = endPoint3D[1] - startPoint3D[1];
            double vector3DZ = endPoint3D[2] - startPoint3D[2];

            angle = Vector3DToAngleInPlane(vector3DX, vector3DY, vector3DZ, planeName);
            return angle;

        }





        /// <summary>
        /// 将double类型数据四舍五入取整
        /// </summary>
        /// <param name="doubleToBeRound">double数据</param>
        /// <param name="numberRound">取整的数，比如为5时，取整5</param>
        /// <returns>取整后的double数据</returns>
        public static double GetDoubleRound(this double doubleToBeRound, int numberRound = 5)
        {
            //先四舍五入为整数
            int doubleToBeRoundInt = (int)Math.Round(doubleToBeRound);

            //先判断余数
            int remain = doubleToBeRoundInt % numberRound;

            if (remain != 0)
            {
                doubleToBeRoundInt = doubleToBeRoundInt / numberRound * numberRound + numberRound;
            }

            return doubleToBeRoundInt;
        }




        /// <summary>
        /// 将double类型数据向上取整
        /// </summary>
        /// <param name="doubleToBeCeiling">double数据</param>
        /// <param name="numberCeiling">取整的数，比如为5时，取整5</param>
        /// <returns>取整后的double数据</returns>
        public static double GetDoubleCeiling(this double doubleToBeCeiling, int numberCeiling = 5)
        {
            //先向上取整数
            int doubleToBeCeilingInt = (int)Math.Ceiling(doubleToBeCeiling);

            //先判断余数
            int remain = doubleToBeCeilingInt % numberCeiling;

            if (remain != 0)
            {
                doubleToBeCeilingInt = doubleToBeCeilingInt / numberCeiling * numberCeiling + numberCeiling;
            }

            return doubleToBeCeilingInt;
        }


        /// <summary>
        /// 将double类型数据向上取整
        /// </summary>
        /// <param name="doubleToBeCeiling">double数据</param>
        /// <param name="numberFloor">取整的数，比如为5时，取整5</param>
        /// <returns>取整后的double数据</returns>
        public static double GetDoubleFloor(this double doubleToBeCeiling, int numberFloor = 5)
        {
            //先向下取整数
            int doubleToBeFloorInt = (int)Math.Floor(doubleToBeCeiling);

            //先判断余数
            int remain = doubleToBeFloorInt % numberFloor;

            if (remain != 0)
            {
                doubleToBeFloorInt = doubleToBeFloorInt / numberFloor * numberFloor - numberFloor;
            }

            return doubleToBeFloorInt;
        }



        /// <summary>
        /// 一次函数或者线性插值，如果在两头，那直接返回两头的值
        /// </summary>
        /// <param name="dataArr">x值按从小到大排序</param>
        /// <param name="xValue">要获取值对应的x值</param>
        /// <param name="interMode">插入的模式，0：如果在两头，那直接返回两头的值，其它值：外插法</param>
        /// <returns>要获取的y值，如果计算失败，返回bouble.NaN</returns>
        public static double LinearInterpolation(double[,] dataArr, double xValue, int interMode = 0)
        {
            //返回值
            double yValue = double.NaN;

            if (dataArr == null || dataArr.Length <= 1)
            {
                return yValue;
            }

            //注意：这个dataArr.GetLength(0)为i+j 就是为各维位数相加的值

            int lastIndex = dataArr.GetUpperBound(0);


            for (int i = 0; i <= lastIndex; i++)
            {

                if (i == 0 && (xValue < dataArr[i, 0] || xValue.AreEqual(dataArr[i, 0], DOUBLE_DELTA))) //小于等于第一项
                {


                    if (interMode == 0)
                    {
                        yValue = dataArr[i, 1];
                    }
                    else //外插法
                    {
                        double x0 = dataArr[0, 0];
                        double y0 = dataArr[0, 1];

                        double x1 = dataArr[1, 0];
                        double y1 = dataArr[1, 1];

                        yValue = (y1 - y0) / (x1 - x0) * (xValue - x0) + y0;
                    }

                    break;
                }


                else if (i == lastIndex && (xValue > dataArr[i, 0] || xValue.AreEqual(dataArr[i, 0], DOUBLE_DELTA))) //大于等于最后一项
                {
                    if (interMode == 0)
                    {
                        yValue = dataArr[i, 1];
                    }

                    else //外插法
                    {
                        double x0 = dataArr[i - 1, 0];
                        double y0 = dataArr[i - 1, 1];

                        double x1 = dataArr[i, 0];
                        double y1 = dataArr[i, 1];

                        yValue = (y1 - y0) / (x1 - x0) * (xValue - x0) + y0;

                        break;
                    }

                    break;
                }

                else if (xValue < dataArr[i, 0])    //其它项
                {
                    double x0 = dataArr[i - 1, 0];
                    double y0 = dataArr[i - 1, 1];

                    double x1 = dataArr[i, 0];
                    double y1 = dataArr[i, 1];

                    yValue = (y1 - y0) / (x1 - x0) * (xValue - x0) + y0;

                    break;
                }

            }

            return yValue;
        }


        /// <summary>
        /// 一次函数或者线性插值，如果在两头，那直接返回两头的值
        /// </summary>
        /// <param name="xValueArr">x值序列 x值按从小到大排序</param>
        /// <param name="yValueArr">y值按从小到大排序</param>
        /// <param name="xValue">要获取值对应的x值</param>
        /// <param name="interMode">插入的模式，0：如果在两头，那直接返回两头的值，其它值：外插法</param>
        /// <returns>要获取的y值，如果计算失败，返回bouble.NaN</returns>
        public static double LinearInterpolation(double[] xValueArr, double[] yValueArr, double xValue, int interMode = 0)
        {
            //返回值
            double yValue = double.NaN;

            if (xValueArr == null || xValueArr.Length <= 1)
            {
                return yValue;
            }
            if (yValueArr == null || yValueArr.Length <= 1)
            {
                return yValue;
            }

            if (xValueArr.Length != yValueArr.Length)
            {
                return yValue;
            }

            double[,] dataArr = new double[xValueArr.Length, 2];

            for (int i = 0; i < xValueArr.Length; i++)
            {
                dataArr[i, 0] = xValueArr[i];
            }

            for (int i = 0; i < yValueArr.Length; i++)
            {
                dataArr[i, 1] = yValueArr[i];
            }


            return LinearInterpolation(dataArr, xValue, interMode);

        }







        /// <summary>
        /// 通过起始值、终止值和步长获取数据列表
        /// </summary>
        /// <param name="startValue">起始值</param>
        /// <param name="endValue">终止值</param>
        /// <param name="distance">步长</param>
        /// <returns>如果失败，返回空的列表</returns>
        public static List<double> GetValueLst(double startValue, double endValue, double distance)
        {
            //返回值
            List<double> valueLst = new List<double>();

            if (distance.AreEqual(0)) //等于0
            {
                return valueLst;
            }


            int count = (int)((endValue - startValue) / distance + 1);

            if (count <= 0)
            {
                return valueLst;
            }


            //以下为正常

            for (int i = 0; i < count; i++)
            {
                double value = startValue + i * distance;
                valueLst.Add(value);
            }


            return valueLst;

        }









        /// <summary>
        /// 将数字转换为中文
        /// </summary>
        /// <param name="number">数字</param>
        /// <returns>对应的中文</returns>
        public static string NumberToChinese(int number)
        {

            string[] chineseNumbers = { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九" };
            string[] chineseUnits = { "", "十", "百", "千", "万", "亿" };




            if (number < 0)
            {
                return "负" + NumberToChinese(-number);
            }
            else if (number < 10)
            {
                return chineseNumbers[number];
            }







            //17:49 2023/8/28 修改这个 因为当为10时返回的的是："十零" 对于10来说，是不对的 ，应该返回的是："十"
            else if (number == 10)
            {
                return "十";
            }





            else if (number < 20)
            {
                return "十" + chineseNumbers[number % 10];
            }
            else
            {
                int unitIndex = 0;
                string chineseNumber = "";
                while (number > 0)
                {
                    int digit = number % 10;
                    if (digit > 0)
                    {
                        chineseNumber = chineseNumbers[digit] + chineseUnits[unitIndex] + chineseNumber;
                    }
                    else if (chineseNumber.Length > 0 && chineseNumber[0] != '零')
                    {
                        chineseNumber = chineseNumbers[digit] + chineseNumber;
                    }
                    else if (unitIndex == 4)  // 十万以及更高的单位
                    {
                        chineseNumber = chineseUnits[unitIndex] + chineseNumber;
                    }
                    number /= 10;
                    unitIndex++;
                }
                return chineseNumber;
            }
        }

         



}
}
