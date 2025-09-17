using ZwSoft.ZwCAD.Geometry;
using System.Collections.Generic;

namespace ZWCAD.BaseTools
{
    /// <summary>
    /// 图框对象
    /// </summary>
    public class FrameBlock
    {


        /// <summary>
        /// 构造函数
        /// </summary>
        public FrameBlock()
        {

        }


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="frameName">块名称</param>
        /// <param name="width">宽度（单位：mm）</param>
        /// <param name="height">高度（单位：mm）</param>
        public FrameBlock(string frameName, int width, int height)
        {
            FrameName = frameName;
            Length = width;
            Width = height;

        }

        /// <summary>
        /// 图框名称
        /// </summary>
        public string FrameName { get; set; }


        /// <summary>
        /// 图框宽度(单位：mm）
        /// </summary>
        public double Length { get; set; }

        /// <summary>
        /// 图框高度(单位：mm）
        /// </summary>
        public double Width { get; set; }




        /// <summary>
        /// 公司名称
        /// </summary>
        public string CompanyName { get; set; }




        /// <summary>
        /// 插入点坐标，因为有些文件所有对象的最小点没有放在原点，导致坐标偏了
        /// </summary>
        public Point3d InsertPoint { get; set; }




        /// <summary>
        /// 所有属性标记和属性值映射
        /// </summary>
        public Dictionary<string, string> AttributeMap { get; set; }



        /// <summary>
        /// 全路径文件名称
        /// </summary>
        public string AbsoluteFileName { get; set; }







        //以下为Revit中使用的属性




        /// <summary>
        /// 图框有效长度(单位：mm）
        /// </summary>
        public double ActuralLength { get; set; }




        /// <summary>
        /// 图框有效宽度(单位：mm）
        /// </summary>
        public double ActuralWidth { get; set; }






        /// <summary>
        /// 中心点距左下角点x偏移(单位：mm）
        /// </summary>
        public double CenterXOffset { get; set; }





        /// <summary>
        /// 中心点距左下角点y偏移(单位：mm）
        /// </summary>
        public double CenterYOffset { get; set; }






        ///// <summary>
        ///// 插入点距左下角点x偏移(单位：mm）
        ///// </summary>
        //public double InsertPointXOffset { get; set; }





        ///// <summary>
        ///// 插入点距左下角点y偏移(单位：mm）
        ///// </summary>
        //public double InsertPointYOffset { get; set; }













    }
}
