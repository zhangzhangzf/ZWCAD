using System.Collections.Generic;
using System.Reflection;


namespace Mrf.CSharp.BaseTools.Extension
{
    /// <summary>
    /// 类反射扩展
    /// </summary>
    public class ClassReflectionExtension
    {



        #region Private Variables

        #endregion



        #region Default Constructor

        #endregion




        #region CommandMethods


        #endregion



        #region Helper Methods



        /// <summary>
        /// 获取所有公共、实例属性名称列表
        /// </summary>
        /// <typeparam name="T">类的类型</typeparam>
        /// <param name="t">类的名称</param>
        /// <returns>如果失败，返回空的列表</returns>
        public static List<string> GetPropertyNameLst<T>(T t)
        {
            List<string> result = new List<string>();
            if (t == null)
            {
                return result;
            }
            PropertyInfo[] properties = t.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            if (properties.Length <= 0)
            {
                return result;
            }
            foreach (PropertyInfo item in properties)
            {
                string name = item.Name; //名称
                object value = item.GetValue(t, null);  //值

                if (item.PropertyType.IsValueType || item.PropertyType.Name.StartsWith("String"))
                {
                    result.Add(name);
                }
                else
                {
                    GetProperties(value);
                }
            }
            return result;
        }




        /// <summary>
        /// 获取所有公共、实例属性对象列表
        /// </summary>
        /// <typeparam name="T">类的类型</typeparam>
        /// <param name="t">类的名称</param>
        /// <returns>如果失败，返回空的列表</returns>
        public static List<PropertyInfo> GetProperties<T>(T t)
        {
            List<PropertyInfo> result = new List<PropertyInfo>();
            if (t == null)
            {
                return result;
            }
            PropertyInfo[] properties = t.GetType().GetProperties(BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (properties.Length <= 0)
            {
                return result;
            }
            foreach (PropertyInfo item in properties)
            {
                string name = item.Name; //名称

                //报错 暂时不要
                //object value = item.GetValue(t, null);  //值

                if (item.PropertyType.IsValueType || item.PropertyType.Name.StartsWith("String"))
                {
                    result.Add(item);
                }


                //报错 暂时不要

                //else
                //{
                //    GetProperties(value);
                //}
            }
            return result;
        }




        /// <summary>
        /// 获取所有公共、实例属性名称和对象列表 区分大小写
        /// </summary>
        /// <typeparam name="T">类的类型</typeparam>
        /// <param name="t">类的名称</param>
        /// <returns>如果失败，返回空的映射</returns>
        public static Dictionary<string, PropertyInfo> GetNameAndPropertyMap<T>(T t)
        {
            //返回值
            Dictionary<string, PropertyInfo> nameAndPropertyMap = new Dictionary<string, PropertyInfo>();

            List<PropertyInfo> result = GetProperties(t);
            result.ForEach(x => nameAndPropertyMap.Add(x.Name, x));

            return nameAndPropertyMap;

        }







        public static List<string> GetFields<T>(T t)
        {
            List<string> ListStr = new List<string>();
            if (t == null)
            {
                return ListStr;
            }
            FieldInfo[] fields = t.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (fields.Length <= 0)
            {
                return ListStr;
            }
            foreach (FieldInfo item in fields)
            {
                string name = item.Name; //名称
                object value = item.GetValue(t);  //值

                if (item.FieldType.IsValueType || item.FieldType.Name.StartsWith("String"))
                {
                    ListStr.Add(name);
                }
                else
                {
                    GetFields(value);
                }
            }
            return ListStr;
        }





        #endregion


        #region Properties


        #endregion




    }
}
