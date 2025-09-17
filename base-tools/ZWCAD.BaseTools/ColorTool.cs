using ZwSoft.ZwCAD.Colors;
using System;

namespace ZWCAD.BaseTools
{
    /// <summary>
    /// 颜色工具
    /// </summary>
    public class ColorTool
    {


        /// <summary>
        /// 获取RGB颜色
        /// </summary>
        /// <param name="R"></param>
        /// <param name="G"></param>
        /// <param name="B"></param>
        /// <returns>颜色对象</returns>
        public static Color GetColorRGB(int R, int G, int B)
        {
            var red = Convert.ToByte(R);
            var green = Convert.ToByte(G);
            var blue = Convert.ToByte(B);
            return Color.FromRgb(red, green, blue);
        }




        /// <summary>
        /// 获取颜色的索引
        /// </summary>
        /// <param name="R"></param>
        /// <param name="G"></param>
        /// <param name="B"></param>
        /// <returns>颜色的索引</returns>
        public static short GetIndexOfColor(int R, int G, int B)
        {
            Color color = GetColorRGB(R, G, B);
            short index = color.ColorIndex;
            return index;
        }



        /// <summary>
        /// 获取索引颜色
        /// </summary>
        /// <param name="R"></param>
        /// <param name="G"></param>
        /// <param name="B"></param>
        /// <returns>颜色对象</returns>
        public static Color GetColorIndex(int R, int G, int B)
        {
            short index = GetIndexOfColor(R, G, B);
            Color color = Color.FromColorIndex(ColorMethod.ByColor, index);
            return color;

        }

    }
}
