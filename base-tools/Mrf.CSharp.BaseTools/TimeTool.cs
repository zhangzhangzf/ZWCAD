using System;

namespace Mrf.CSharp.BaseTools
{

    /// <summary>
    /// 时间工具
    /// </summary>
    public class TimeTool
    {

/// <summary>
/// 构造函数
/// </summary>
        public TimeTool()
        {

        }


        /// <summary>
        /// 获取当前时间的某种格式，默认格式为：202012040859334620
        /// </summary>
        /// <param name="format">格式</param>
        /// <returns>字符串</returns>
        public static string GetCurrentTimeByFormat(string format = null)
        {
            if (string.IsNullOrEmpty(format))
            {
                format = "{0:yyyyMMddHHmmssffff}";
            }

            DateTime currentDateTime = DateTime.Now;

            string formatedDateTime = string.Format(format, currentDateTime); //202012040859334620

            return formatedDateTime;

        }







    }
}
