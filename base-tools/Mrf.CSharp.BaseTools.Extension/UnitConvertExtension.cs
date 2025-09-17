using System;

namespace Mrf.CSharp.BaseTools.Extension
{

    /// <summary>
    /// 单位转换工具
    /// </summary>
    public static class UnitConvertExtension
    {


        /// <summary>
        /// 毫米转换为英尺
        /// </summary>
        /// <param name="value">要转换的浮点数</param>
        /// <returns>转换为英尺后的浮点数</returns>
        public static double Millimeter2Foot(this double value)
        {
            return value / 304.8;
        }



        /// <summary>
        /// 毫米转换为米
        /// </summary>
        /// <param name="value">要转换的浮点数</param>
        /// <returns>转换为米后的浮点数</returns>
        public static double Millimeter2Meter(this double value)
        {
            return value * 0.001;
        }

        
        /// <summary>
        /// 米转换为毫米
        /// </summary>
        /// <param name="value">要转换的浮点数</param>
        /// <returns>转换为米后的浮点数</returns>
        public static double Meter2Millimeter(this double value)
        {
            return value * 1000;
        }



        /// <summary>
        /// 英尺转换为毫米
        /// </summary>
        /// <param name="value">要转换的浮点数</param>
        /// <returns>转换为毫米后的浮点数</returns>
        public static double Foot2Millimeter(this double value)
        {
            return value * 304.8;
        }






        /// <summary>
        /// 米转换为英尺
        /// </summary>
        /// <param name="value">要转换的浮点数</param>
        /// <returns>转换为英尺后的浮点数</returns>
        public static double Meter2Foot(this double value)
        {
            return value / 0.3048;
        }



        /// <summary>
        /// 英尺转换为米
        /// </summary>
        /// <param name="value">要转换的浮点数</param>
        /// <returns>转换为米后的浮点数</returns>
        public static double Foot2Meter(this double value)
        {
            return value * 0.3048;
        }


        /// <summary>
        /// 平方英尺转换为平方米
        /// </summary>
        /// <param name="value">要转换的浮点数</param>
        /// <returns>转换为平方米后的浮点数</returns>
        public static double SquareFoot2SquareMeter(this double value)
        {
            return value * 0.3048 * 0.3048;
        }



        /// <summary>
        /// 立方英尺转换为立方米
        /// </summary>
        /// <param name="value">要转换的浮点数</param>
        /// <returns>转换为立方米后的浮点数</returns>
        public static double CubicFoot2CubicMeter(this double value)
        {
            return value * 0.3048 * 0.3048 * 0.3048;
        }



        /// <summary>
        /// 角度值转换为弧度值
        /// </summary>
        /// <param name="value">要转换的浮点数</param>
        /// <returns>转换为弧度值后的浮点数</returns>
        public static double Angle2Radian(this double value)
        {
            return value * Math.PI / 180;
        }



        /// <summary>
        /// 弧度值转换为角度值
        /// </summary>
        /// <param name="value">要转换的浮点数</param>
        /// <returns>转换为角度值后的浮点数</returns>
        public static double Radian2Angle(this double value)
        {
            return value * 180 / Math.PI;
        }







    }
}
