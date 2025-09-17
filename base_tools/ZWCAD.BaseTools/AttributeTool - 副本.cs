using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AutoCAD.BaseTools
{
    /// <summary>
    /// 属性工具
    /// </summary>
    public class AttributeTool
    {

        /// <summary>
        /// 定义属性
        /// </summary>
        /// <param name="label">标记名</param>
        /// <param name="prompt">提示</param>
        /// <param name="value">属性值</param>
        /// <param name="insertPoint">属性插入点位置</param>
        /// <param name="textHeight">文字高度</param>
        /// <returns>属性</returns>
        public AttributeDefinition AttributeDefinition(string label, string prompt, string value, Point3d insertPoint, double textHeight = 1000)
        {
            AttributeDefinition ad = new AttributeDefinition
            {
                Constant = false,
                Tag = label,
                Prompt = prompt,
                TextString = value,
                Position = insertPoint,
                Height = textHeight
            };
            return ad;
        }




    }
}
