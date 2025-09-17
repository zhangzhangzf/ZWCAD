using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Mrf.CSharp.BaseTools.Extension
{
    /// <summary>
    /// 字符串工具
    /// </summary>
    public static class StringExtension
    {


        //备注 数字，字母的ASCII码对照表
        /*
        0~9数字对应十进制48－57 
        a~z字母对应的十进制97－122十六进制61－7A 
        A~Z字母对应的十进制65－90十六进制41－5A

        */

        /// <summary>
        /// 判断字符串是否为纯数字
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>

        public static bool IsNumeric(this string str)
        {
            if (str == null || str.Length == 0)    //验证这个参数是否为空
                return false;                           //是，就返回False
            ASCIIEncoding ascii = new ASCIIEncoding();//new ASCIIEncoding 的实例
            byte[] bytestr = ascii.GetBytes(str);         //把string类型的参数保存到数组里

            foreach (byte c in bytestr)                   //遍历这个数组里的内容
            {
                if (c < 48 || c > 57)                          //判断是否为数字
                {
                    return false;                              //不是，就返回False
                }
            }
            return true;                                        //是，就返回True
        }




        /// <summary>  
        /// 验证数字(double类型)  
        /// [可以包含负号和小数点]  
        /// </summary>  
        /// <param name="input">待验证的字符串</param>  
        /// <returns>是否匹配</returns>  
        public static bool IsNumber(this string input)
        {
            //string pattern = @"^-?\d+$|^(-?\d+)(\.\d+)?$";  
            //return IsMatch(input, pattern);  
            if (double.TryParse(input, out _))
                return true;
            else
                return false;
        }




        /// <summary>  
        /// 验证整数  
        /// </summary>  
        /// <param name="input">待验证的字符串</param>  
        /// <returns>是否匹配</returns>  
        public static bool IsInteger(this string input)
        {
            //string pattern = @"^-?\d+$";  
            //return IsMatch(input, pattern);  
            if (int.TryParse(input, out _))
                return true;
            else
                return false;
        }






        /// <summary>  
        /// 验证非负整数  
        /// </summary>  
        /// <param name="input">待验证的字符串</param>  
        /// <returns>是否匹配</returns>  
        public static bool IsIntegerNotNagtive(this string input)
        {
            //string pattern = @"^\d+$";  
            //return IsMatch(input, pattern);  
            if (int.TryParse(input, out int i) && i >= 0)
                return true;
            else
                return false;
        }

        /// <summary>  
        /// 验证正整数  
        /// </summary>  
        /// <param name="input">待验证的字符串</param>  
        /// <returns>是否匹配</returns>  
        public static bool IsIntegerPositive(this string input)
        {
            //string pattern = @"^[0-9]*[1-9][0-9]*$";  
            //return IsMatch(input, pattern);  
            if (int.TryParse(input, out int i) && i >= 1)
                return true;
            else
                return false;
        }




        /// <summary>  
        /// 验证日期  
        /// </summary>  
        /// <param name="input">待验证的字符串</param>  
        /// <returns>是否匹配</returns>  
        public static bool IsDateTime(this string input)
        {
            if (DateTime.TryParse(input, out _))
                return true;
            else
                return false;
        }



        #region 匹配方法
        /// <summary>  
        /// 验证字符串是否匹配正则表达式描述的规则  
        /// </summary>  
        /// <param name="inputStr">待验证的字符串</param>  
        /// <param name="patternStr">正则表达式字符串</param>  
        /// <returns>是否匹配</returns>  
        public static bool IsMatch(this string inputStr, string patternStr)
        {
            return IsMatch(inputStr, patternStr, false, false);
        }

        /// <summary>  
        /// 验证字符串是否匹配正则表达式描述的规则  
        /// </summary>  
        /// <param name="inputStr">待验证的字符串</param>  
        /// <param name="patternStr">正则表达式字符串</param>  
        /// <param name="ifIgnoreCase">匹配时是否不区分大小写</param>  
        /// <returns>是否匹配</returns>  
        public static bool IsMatch(this string inputStr, string patternStr, bool ifIgnoreCase)
        {
            return IsMatch(inputStr, patternStr, ifIgnoreCase, false);
        }



        /// <summary>  
        /// 验证字符串是否匹配正则表达式描述的规则  
        /// </summary>  
        /// <param name="inputStr">待验证的字符串</param>  
        /// <param name="patternStr">正则表达式字符串</param>  
        /// <param name="ifIgnoreCase">匹配时是否不区分大小写</param>  
        /// <param name="ifValidateWhiteSpace">是否验证空白字符串</param>  
        /// <returns>是否匹配</returns>  
        public static bool IsMatch(this string inputStr, string patternStr, bool ifIgnoreCase, bool ifValidateWhiteSpace)
        {
            if (!ifValidateWhiteSpace && string.IsNullOrWhiteSpace(inputStr))//.NET 4.0 新增IsNullOrWhiteSpace 方法，便于对用户做处理
                return false;//如果不要求验证空白字符串而此时传入的待验证字符串为空白字符串，则不匹配  
            Regex regex = null;
            if (ifIgnoreCase)
                regex = new Regex(patternStr, RegexOptions.IgnoreCase);//指定不区分大小写的匹配  
            else
                regex = new Regex(patternStr);
            return regex.IsMatch(inputStr);
        }
        #endregion




        /// <summary>  
        /// 验证小数  
        /// </summary>  
        /// <param name="input">待验证的字符串</param>  
        /// <returns>是否匹配</returns>  
        public static bool IsDecimal(this string input)
        {
            string pattern = @"^([-+]?[1-9]\d*\.\d+|-?0\.\d*[1-9]\d*)$";
            return IsMatch(input, pattern);
        }

        /// <summary>  
        /// 验证只包含英文字母  
        /// </summary>  
        /// <param name="input">待验证的字符串</param>  
        /// <returns>是否匹配</returns>  
        public static bool IsEnglishCharacter(this string input)
        {
            string pattern = @"^[A-Za-z]+$";
            return IsMatch(input, pattern);
        }

        /// <summary>  
        /// 验证只包含数字和英文字母  
        /// </summary>  
        /// <param name="input">待验证的字符串</param>  
        /// <returns>是否匹配</returns>  
        public static bool IsIntegerAndEnglishCharacter(this string input)
        {
            string pattern = @"^[0-9A-Za-z]+$";
            return IsMatch(input, pattern);
        }

        /// <summary>  
        /// 验证只包含汉字  
        /// </summary>  
        /// <param name="input">待验证的字符串</param>  
        /// <returns>是否匹配</returns>  
        public static bool IsChineseCharacter(this string input)
        {
            string pattern = @"^[\u4e00-\u9fa5]+$";
            return IsMatch(input, pattern);
        }




        /// <summary>
        /// 从字符串中提取数字字符串列表
        /// </summary>
        /// <param name="containNumberString">字符串</param>
        /// <param name="includedSymbols">可以允许的字符组成的字符串，默认"+-.",那么将会允许数字中包含这些字符，如果都不允许，使用""</param>
        /// <returns>数字字符串列表，如果不存在数字，返回空的列表</returns>
        public static List<string> GetDataLstFromString(this string containNumberString, string includedSymbols = "+-.")
        {
            //返回值
            List<string> returnDataLst = new List<string>();

            List<char> includedSymbolLst = new List<char>(includedSymbols);

            string data = "";

            foreach (char c in containNumberString)
            {
                if (Convert.ToInt32(c) >= 48 && Convert.ToInt32(c) <= 57 ||
                    includedSymbolLst.Contains(c))
                {
                    data += c;
                }
                else
                {
                    if (!string.IsNullOrEmpty(data))
                    {
                        returnDataLst.Add(data);
                        data = "";
                    }
                }

            }

            //可能剩下最后的一组数字
            if (!string.IsNullOrEmpty(data))
            {
                returnDataLst.Add(data);
            }

            return returnDataLst;

        }




        /// <summary>
        /// 从字符串中提取字符串列表
        /// </summary>
        /// <param name="stringToBeSplit">字符串</param>
        /// <param name="splitSymbols">可以允许的字符组成的字符串，默认"+-.",那么将会允许数字中包含这些字符，如果都不允许，使用""</param>
        /// <returns>数字字符串列表，如果不存在数字，返回空的列表</returns>
        public static List<string> GetDataStringLstFromString(this string stringToBeSplit, string splitSymbols = " \t")
        {
            List<char> splitSymbolLst = new List<char>(splitSymbols);
            char[] splitSymbolArr = splitSymbolLst.ToArray();


            string[] splitResult = stringToBeSplit.Split(splitSymbolArr);

            //返回值
            List<string> returnStringLst = new List<string>(splitResult);

            return returnStringLst;

        }


        /// <summary>
        /// 从字符串中提取双精度列表
        /// </summary>
        /// <param name="containNumberString">字符串</param>
        /// <param name="includedSymbols">可以允许的字符组成的字符串，默认"+-.",那么将会允许数字中包含这些字符，如果都不允许，使用""</param>
        /// <returns>双精度列表，如果读取失败，返回null，如果没有数字，返回空的列表</returns>
        public static List<double> GetDataNumberLstFromString(this string containNumberString, string includedSymbols = "+-.")
        {
            List<double> dataNumberLst = new List<double>();

            List<string> dataLst = GetDataLstFromString(containNumberString, includedSymbols);

            foreach (string data in dataLst)
            {
                if (double.TryParse(data, out double result))
                {
                    dataNumberLst.Add(result);
                }
                else
                {
                    return null;
                }
            }


            //如果上面没有返回，说明成功
            return dataNumberLst;

        }


        /// <summary>
        /// 将每个字符串分解为一个列表
        /// </summary>
        /// <param name="stringLst">字符串列表</param>
        /// <param name="splitSymbols">可以包含的字符，默认", \t"</param>
        /// <returns>如果stringLst为null，返回null，如果stringLst为空的列表，返回空的列表，如果读取数据失败，返回null，其它情况，一行字符串返回一个列表</returns>
        public static List<List<string>> GetDataStringLstFromStringLst(this List<string> stringLst, string splitSymbols = ", \t")
        {

            //返回值
            List<List<string>> dataLst = new List<List<string>>();


            if (stringLst == null)
            {
                return null;
            }


            if (stringLst.Count == 0)
            {
                return dataLst;
            }

            foreach (string each in stringLst)
            {
                List<string> eachLineLst = GetDataStringLstFromString(each, splitSymbols);
                if (eachLineLst == null)  //读取失败,直接返回空的列表
                {
                    dataLst.Clear();
                    return dataLst;
                }

                if (eachLineLst.Count > 0) //空的列表不要
                {
                    dataLst.Add(eachLineLst);
                }

            }

            return dataLst;

        }








        /// <summary>
        /// 将每个字符串分解为一个只包含数字的列表
        /// </summary>
        /// <param name="stringLst">字符串列表</param>
        /// <param name="includedSymbols">可以包含的字符，默认", \t"</param>
        /// <returns>如果stringLst为null，返回null，如果stringLst为空的列表，返回空的列表，如果读取数据失败，返回null，其它情况，一行字符串返回一个列表</returns>
        public static List<List<string>> GetDataLstFromstringLst(this List<string> stringLst, string includedSymbols = ", \t")
        {

            //返回值
            List<List<string>> dataLst = new List<List<string>>();


            if (stringLst == null)
            {
                return null;
            }


            if (stringLst.Count == 0)
            {
                return dataLst;
            }

            foreach (string each in stringLst)
            {
                List<string> eachLineLst = GetDataLstFromString(each, includedSymbols);
                if (eachLineLst == null)  //读取失败,直接返回空的列表
                {
                    dataLst.Clear();
                    return dataLst;
                }

                if (eachLineLst.Count > 0) //空的列表不要
                {
                    dataLst.Add(eachLineLst);
                }

            }

            return dataLst;

        }





        /// <summary>
        /// 从字符串列表中获取数字列表，每个字符串返回一个数字列表
        /// </summary>
        /// <param name="dataLst">字符串列表</param>
        /// <param name="includedSymbols">可以包含的字符，默认"+-."</param>
        /// <returns>如果dataLst为null，返回null，如果dataLst为空的列表，返回空的列表，如果读取数据失败，返回null，其它情况，一行字符串返回一个数字列表</returns>
        public static List<List<double>> GetNumberLstFromStringLst(this List<string> dataLst, string includedSymbols = "+-.")
        {

            //返回值
            List<List<double>> dataNumberLst = new List<List<double>>();


            if (dataLst == null)
            {
                return null;
            }


            if (dataLst.Count == 0)
            {
                return dataNumberLst;
            }

            foreach (string each in dataLst)
            {
                List<double> eachLineLst = GetDataNumberLstFromString(each, includedSymbols);
                if (eachLineLst == null)  //读取失败,直接返回空的列表
                {
                    dataNumberLst.Clear();
                    return dataNumberLst;
                }

                if (eachLineLst.Count > 0) //空的列表不要
                {
                    dataNumberLst.Add(eachLineLst);
                }

            }

            return dataNumberLst;

        }






        /// <summary>
        /// 从字符串列表中获取数字列表，每个字符串返回一个数字列表
        /// </summary>
        /// <param name="dataLst">字符串列表</param>
        /// <param name="includedSymbols">可以包含的字符，默认"+-."</param>
        /// <returns>如果dataLst为null，返回null，如果dataLst为空的列表，返回空的列表，如果读取数据失败，返回null，其它情况，一行字符串返回一个数字列表</returns>
        public static List<List<double>> GetNumberLstFromStringLst(this List<string[]> dataLst, string includedSymbols = "+-.")
        {

            //返回值
            List<List<double>> dataNumberLst = new List<List<double>>();


            if (dataLst == null)
            {
                return null;
            }


            if (dataLst.Count == 0)
            {
                return dataNumberLst;
            }

            foreach (string[] eachLine in dataLst)
            {

                List<double> eachLineLst = new List<double>();

                foreach (string each in eachLine)
                {

                    List<double> eachLst = GetDataNumberLstFromString(each, includedSymbols);
                    if (eachLst == null || eachLst.Count == 0)  //读取失败或没有数据,直接返回空的列表
                    {
                        dataNumberLst.Clear();
                        return dataNumberLst;
                    }

                    //只取第一个数值
                    eachLineLst.Add(eachLst[0]);
                }



                if (eachLineLst.Count > 0) //空的列表不要
                {
                    dataNumberLst.Add(eachLineLst);
                }


            }

            return dataNumberLst;

        }




        /// <summary>
        /// 如果字符串后缀不是数字，则加上数字1，如果以数字结尾，则数字加1后，作为新的字符串返回。主要应用于名字已经存在，获取新的名字的情况
        /// </summary>
        /// <param name="oldName">旧的字符串</param>
        /// <returns>新的字符串</returns>
        public static string GetNewName(this string oldName)
        {

            //返回值
            string newName;

            //是否以数字结尾,正则表达式
            string regularExpression = @"[0-9]$";
            Regex rg = new Regex(regularExpression);

            if (rg.IsMatch(oldName))  //以文字结尾，递增
            {

                List<double> dataNumberLst = oldName.GetDataNumberLstFromString();
                double lastNumber = dataNumberLst.LastOrDefault();
                int index = oldName.LastIndexOf(lastNumber.ToString());
                newName = oldName.Substring(0, index) + (lastNumber + 1).ToString();
            }
            else  //如果不是以字母开头，直接加1
            {
                newName = oldName + 1;
            }

            return newName;

        }


        /// <summary>
        /// 获取指定替换次数的新字符串
        /// </summary>
        /// <param name="input">字符串</param>
        /// <param name="oldString">被替换的旧字符串</param>
        /// <param name="newString">要替换的新字符串</param>
        /// <param name="count">替换的次数</param>
        /// <returns>被替换后的新字符串</returns>
        public static string StringReplace(this string input, string oldString, string newString, int count = 1)
        {
            Regex regex = new Regex(oldString);
            string output = regex.Replace(input, newString, count);

            return output;
        }





    }
}
