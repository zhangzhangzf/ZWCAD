
using System;
using System.Collections.Generic;


namespace Mrf.CSharp.BaseTools.Extension
{
    /// <summary>
    /// dictionary的扩展
    /// </summary>
    public static class DictionaryExtension
    {

        /// <summary>
        /// 合并一个dictionary到另一个dictionary中，判断两者是否为null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="S"></typeparam>
        /// <param name="target">目标对象</param>
        /// <param name="source">要添加的对象</param>
        /// <param name="isReplaceOLd"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void AddRange<T, S>(this Dictionary<T, S> target, Dictionary<T, S> source,bool isReplaceOLd=false)
        {

            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            foreach (var item in source)
            {
                if (!target.ContainsKey(item.Key))  //不存在
                {
                    target.Add(item.Key, item.Value);
                }
                else  //已存在
                {
                    if (isReplaceOLd) //要替换
                    {
                        target[item.Key] = item.Value;
                    }

                }
            }
        }


    }
}
