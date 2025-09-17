using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Mrf.CSharp.BaseTools
{
    /// <summary>
    /// 
    /// </summary>
    public class StringTool
    {



        /// <summary>
        /// 构造函数
        /// </summary>
        public StringTool()
        {
        }




        /// <summary>
        /// 生成随机字符串
        ///1、支持自定义字符串长度
        ///2、支持自定义是否包含数字
        ///3、支持自定义是否包含小写字母
        ///4、支持自定义是否包含大写字母
        ///5、支持自定义是否包含特殊符号
        ///6、支持自定义字符集
        /// </summary>
        /// <param name="length">目标字符串的长度,默认4</param>
        /// <param name="useNum">是否包含数字，1=包含，默认为包含</param>
        /// <param name="useLow">是否包含小写字母，1=包含，默认为包含</param>
        /// <param name="useUpp">是否包含大写字母，1=包含，默认为包含</param>
        /// <param name="useSpe">是否包含特殊字符，1=包含，默认为不包含</param>
        /// <param name="custom">要包含的自定义字符，直接输入要包含的字符列表,默认为空</param>
        /// <returns>指定长度的随机字符串</returns>
        public static string GetRandomString(int length = 4, bool useNum = true, bool useLow = true, bool useUpp = true, bool useSpe = false, string custom = "")
        {

            byte[] b = new byte[4];
            new System.Security.Cryptography.RNGCryptoServiceProvider().GetBytes(b);
            Random r = new Random(BitConverter.ToInt32(b, 0));
            string s = null, str = custom;

            if (useNum == true) { str += "0123456789"; }
            if (useLow == true) { str += "abcdefghijklmnopqrstuvwxyz"; }
            if (useUpp == true) { str += "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; }
            if (useSpe == true) { str += "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~"; }

            for (int i = 0; i < length; i++)
            {
                s += str.Substring(r.Next(0, str.Length - 1), 1);
            }

            return s;
        }






    }
}
