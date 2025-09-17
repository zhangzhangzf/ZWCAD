using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Mrf.CSharp.BaseTools.Extension
{

    /// <summary>
    /// 枚举扩展
    /// </summary>
    public static class EnumExtension
    {



        /// <summary>
        /// 获取字符串对应的枚举值
        /// </summary>
        /// <typeparam name="T">泛型，枚举类型</typeparam>
        /// <param name="stringValue">枚举值的字符串形式</param>
        /// <returns>枚举值,如果不存在，返回默认值，比如如果为引用类型，返回null，如果为值类型，返回0</returns>
        public static T GetEnumValue<T>(this string stringValue)
        {
            //调用的时候发现有错误，使用Try
            try
            {


                //不一定存在，先判断
                if (Enum.IsDefined(typeof(T), stringValue)) //存在的情况
                {
                    //如果不先判断是否存在，会抛出异常
                    return (T)Enum.Parse(typeof(T), stringValue);
                }
                else //不存在的情况
                {

                    //返回0所对应的枚举值，如果枚举值中没有定义整数0，则返回第一个值
                    return default;
                }
            }
            catch
            {
                return default;
            }

        }


        /// <summary>
        /// 获取枚举对应的字符串名称
        /// </summary>
        /// <typeparam name="T">泛型，枚举类型</typeparam>
        /// <param name="value">枚举值</param>
        /// <returns>字符串</returns>
        public static string GetEnumName<T>(object value)
        {
            return Enum.GetName(typeof(T), value);
        }


        /// <summary>
        /// 获取枚举的所有名称字符串数组
        /// </summary>
        /// <returns></returns>
        public static string[] GetEnumNames<T>()
        {
            return Enum.GetNames(typeof(T));
        }



        /// <summary>
        /// 获取所有的枚举值
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <returns></returns>
        public static List<T> GetEnumValues<T>()
        {
            //返回值
            List<T> values = new List<T>();

            foreach (T each in Enum.GetValues(typeof(T)))
            {
                values.Add(each);
            }

            return values;
        }



        /// <summary>
        /// 获取枚举值的属性（或定义描述）
        /// </summary>
        /// <param name="val">枚举对象</param>
        /// <returns>属性（或定义描述），如果没有，就把当前枚举值的对应名称返回</returns>
        public static string GetDescription(this Enum val)
        {
            var type = val.GetType();
            var memberInfo = type.GetMember(val.ToString());
            var attributes = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            //如果没有定义描述，就把当前枚举值的对应名称返回
            if (attributes == null || attributes.Length != 1) return val.ToString();

            return (attributes.Single() as DescriptionAttribute).Description;
        }




        /// <summary>
        /// 从属性获取对应的枚举值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="description"></param>
        /// <returns></returns>
        public static T GetValueFromDescription<T>(this string description) where T : Enum
        {
            foreach (var field in typeof(T).GetFields())
            {
                if (Attribute.GetCustomAttribute(field,
                typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                {
                    if (attribute.Description == description)
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name == description)
                        return (T)field.GetValue(null);
                }
            }

            //throw new ArgumentException("Not found.", nameof(description));
             return default(T);
        }



    }
}
