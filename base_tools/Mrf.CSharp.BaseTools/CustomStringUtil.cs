
using System;

namespace Mrf.CSharp.BaseTools
{
    /// <summary>
    /// 字符串工具
    /// </summary>
    public static class CustomStringUtil
    {


        /// <summary>
        /// 通过正则表达式提取字符串中的数字
        /// </summary>
        /// <param name="textString">字符串</param>
        /// <param name="resultInt">返回值，如果失败，则返回0</param>
        /// <returns>转换成功时，返回true，否则，返回false</returns>
        public static bool GetNumberByRegularExpressions(string textString, out int resultInt)
        {
            string result = System.Text.RegularExpressions.Regex.Replace(textString, @"[^0-9]+", "");

            return int.TryParse(result, out resultInt);
        }



        /// <summary>
        /// 通过ASCII码方法返回字符串中的数字
        /// </summary>
        /// <param name="textString"></param>
        /// <param name="resultInt"></param>
        /// <returns>如果是，返回true，否则，返回false</returns>
        public static bool GetNumberByASCII(string textString, out int resultInt)
        {
            string result = "";
            foreach (char c in textString)
            {
                if (Convert.ToInt32(c) >= 48 && Convert.ToInt32(c) <= 57)
                {

                    result += c;
                }
            }

            return int.TryParse(result, out resultInt);
        }


        /// <summary>
        ///字符串是否匹配某种模式
        /// </summary>
        /// <param name="inputContext">字符串</param>
        /// <param name="pattern">模式</param>
        /// <returns>如果是，返回true，否则，返回false</returns>
        public static bool IsMatch(this string inputContext, string pattern)
        {

            string[] patternLst = pattern.Split(new char[] { ' ', ',' });


            int startIndex = 0;
            int length = inputContext.Length;
            foreach (string each in patternLst)
            {
                if (startIndex >= length)
                {
                    return false;
                }

                int foundIndex = inputContext.IndexOf(each, startIndex);
                if (foundIndex == -1)
                {
                    return false;
                }

                startIndex = foundIndex + each.Length;

            }
            return true;

        }


    }
}
