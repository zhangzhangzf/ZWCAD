
using System;

namespace Mrf.CSharp.BaseTools
{
    /// <summary>
    /// 转换工具
    /// </summary>
    public static class ConvertUtil
    {

        /// <summary>
        /// 浮点数转换为字符串
        /// </summary>
        /// <param name="data">浮点数</param>
        /// <param name="reservedNumber">保留小数位数</param>
        /// <param name="deleteZero">是否删除末尾的0，默认否</param>
        /// <returns>字符串</returns>
        public static string Double2Sring(double data, int reservedNumber = 3, bool deleteZero = false)
        {
            int realReservedNumber = reservedNumber;

            //如果默认删除末尾的0，需要判断实际要保留的位数
            if (deleteZero && realReservedNumber != 0)
            {
                int rem;
                do
                {

                    int intData = (int)(data * Math.Pow(10, realReservedNumber));

                    //求余数
                    rem = intData % 10;
                    if (rem == 0)
                    {
                        realReservedNumber--;
                    }

                    if (realReservedNumber == 0)
                    {
                        break;
                    }
                } while (rem == 0);


            }

            return data.ToString("f" + realReservedNumber);
        }







        /// <summary>
        /// 获取二维点坐标转换一定角度后新的二维坐标点坐标
        /// </summary>
        /// <param name="xyValueArr">二维点坐标组成的数组</param>
        /// <param name="rotateAngle">旋转角度，逆时针为正，顺时针为负（单位：弧度）</param>
        /// <returns>新的二维点坐标组成的数组</returns>
        public static double[] GetNewPositionByRotate(double[] xyValueArr, double rotateAngle)
        {
            double x0Value = xyValueArr[0];
            double y0Value = xyValueArr[1];

            double x1Value = x0Value * Math.Cos(rotateAngle) - y0Value * Math.Sin(rotateAngle);
            double y1Value = x0Value * Math.Sin(rotateAngle) + y0Value * Math.Cos(rotateAngle);

            double[] newXYValueArr = new double[] { x1Value, y1Value };
            return newXYValueArr;
        }










    }
}