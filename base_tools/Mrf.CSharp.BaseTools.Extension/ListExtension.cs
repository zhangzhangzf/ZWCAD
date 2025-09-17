using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Mrf.CSharp.BaseTools.Extension
{

    /// <summary>
    /// 列表扩展类
    /// </summary>
    public static class ListExtension
    {


        /// <summary>
        /// 交集
        /// </summary>
        /// <param name="list1"></param>
        /// <param name="list2"></param>
        /// <returns></returns>
        public static List<T> Intersection<T>(this List<T> list1, List<T> list2)
        {
            return list1.Intersect(list2).ToList();
        }


        /// <summary>
        /// 差集 list1-list2,将list1中去除跟list2相同的所有元素，返回剩下的元素列表
        /// </summary>
        /// <param name="list1"></param>
        /// <param name="list2"></param>
        /// <returns></returns>
        public static List<T> Exception<T>(this List<T> list1, List<T> list2)
        {
            return list1.Except(list2).ToList();
        }


        /// <summary>
        /// 并集，去重
        /// </summary>
        /// <param name="list1"></param>
        /// <param name="list2"></param>
        /// <returns></returns>
        public static List<T> Union<T>(this List<T> list1, List<T> list2)
        {
            return list1.Union(list2).ToList();
        }


        /// <summary>
        /// 并集，不去重
        /// </summary>
        /// <param name="list1"></param>
        /// <param name="list2"></param>
        /// <returns></returns>
        public static List<T> Contat<T>(this List<T> list1, List<T> list2)
        {
            return list1.Concat(list2).ToList();
        }



        /// <summary>
        /// 判断list1是否包含所有的list2
        /// </summary>
        /// <param name="list1"></param>
        /// <param name="list2"></param>
        /// <returns>true or false</returns>
        public static bool IsContainsAll<T>(this List<T> list1, List<T> list2)
        {
            return list2.All(b => list1.Any(a => a.Equals(b)));
        }



        /// <summary>
        /// 判断两个列表的元素是否全部相等，不考虑位置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list1"></param>
        /// <param name="list2"></param>
        /// <returns></returns>
        public static bool IsEquals<T>(this List<T> list1, List<T> list2)
        {
            //两者各自全部包含对方，说明相等，否则不相等
            return list1.IsContainsAll(list2) && list2.IsContainsAll(list1);
        }


        /// <summary>
        /// 判断两个列表的元素是否全部相等，考虑位置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list1"></param>
        /// <param name="list2"></param>
        /// <returns></returns>
        public static bool IsAbsoluteEquals<T>(this List<T> list1, List<T> list2)
        {
            //如果长度不一样，直接返回不相等
            if (list1.Count != list2.Count)
            {
                return false;
            }

            for (int i = 0; i < list1.Count; i++)
            {
                string list1ValueString = list1[i].ToString();
                string list2ValueString = list2[i].ToString();

                if (list1ValueString != list2ValueString)
                {
                    return false;
                }
            }


            //如果前面都没有返回，说明所有对象相等，返回true
            return true;

        }



        /// <summary>
        /// 判断是否连续包含于，跟位置有关
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list1"></param>
        /// <param name="list2"></param>
        /// <returns></returns>
        public static bool IsConsecutiveContains<T>(this List<T> list1, List<T> list2)
        {
            int length1 = list1.Count;
            int length2 = list2.Count;

            for (int i = 0; i < length1 - length2 + 1; i++)
            {
                List<T> subList1 = list1.GetRange(i, length2);

                //判断是否全部相等
                bool isRealEqual = subList1.IsAbsoluteEquals(list2);

                //如果找为true，直接返回
                if (isRealEqual)
                {
                    return true;
                }
            }

            //如果都没有返回true，则找不到，返回false
            return false;

        }


        /// <summary>
        /// 判断是否连续包含于，跟位置有关,如果顺序没有，再倒序判断
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list1"></param>
        /// <param name="list2"></param>
        /// <returns></returns>
        public static bool IsConsecutiveOrReverseContains<T>(this List<T> list1, List<T> list2)
        {
            bool isContain = list1.IsConsecutiveContains(list2);

            //如果不为true，再反向判断
            if (!isContain)
            {
                //如果直接将对象反序，因为比如为字符串时，直接改变了原来的值
                //list2.Reverse();
                List<T> tmpList2 = list2.GetRange(0, list2.Count);
                tmpList2.Reverse();
                isContain = list1.IsConsecutiveContains(tmpList2);
            }

            return isContain;
        }




        /// <summary>
        /// 子类继承父类，但是List 子类 不继承List 父类，这个功能实现这个效果
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="childList">子类列表</param>
        /// <returns>子类转换成的父类列表</returns>   
        public static List<T> ParentList<T>(this object childList)
        {
            //返回值
            List<T> result = new List<T>();

            if (!childList.GetType().IsGenericType) //是否为泛型类型
            {
                throw new Exception("非泛型类型");
            }

            if (childList as System.Collections.ICollection != null)
            {
                var list = (System.Collections.ICollection)childList;
                if (list.Count > 0)
                {
                    foreach (var item in list)
                    {
                        result.Add((T)item);
                    }
                }
            }

            return result;

        }





        /// <summary>
        /// 将列表转换为DataTable
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="listLink">列表的数据，按列表名称的长度自动分割</param>
        /// <param name="columnNameLst">列表名称</param>
        /// <returns>DataTable</returns>
        public static DataTable ToDataTable<T>(List<T> listLink, List<T> columnNameLst)
        {
            List<List<T>> listGroup = new List<List<T>>();
            int m = columnNameLst.Count;//这个是列的长度
            int j = m;
            //根据传过来的列名来对返回的数据进行划分成一组一组的数据
            for (int i = 0; i < listLink.Count; i += m)
            {
                List<T> cList = listLink.Take(j).Skip(i).ToList();
                j += m;
                listGroup.Add(cList);
            }
            DataTable dt = new DataTable();

            //让一组一组的数据填入到对应的列里
            for (int k = 0; k < columnNameLst.Count; k++)
            {
                dt.Columns.Add(columnNameLst[k].ToString(), typeof(T));
            }
            DataRow row;

            foreach (var item in listGroup)
            {
                row = dt.NewRow();
                for (int l = 0; l < columnNameLst.Count; l++)
                {
                    row[columnNameLst[l].ToString()] = item[l];
                }

                dt.Rows.Add(row);
            }
            return dt;
        }






        /// <summary>
        /// 将列表转换为DataTable
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="listLink">列表的数据，每个子列表为同一行</param>
        /// <param name="columnNameLst">列表名称，如果为null，使用默认的名称</param>
        /// <returns>DataTable</returns>
        public static DataTable ToDataTable<T>(List<List<T>> listLink, List<T> columnNameLst = null)
        {
            //计算最大的列数
            int columnNumber = 0;
            foreach (var item in listLink)
            {
                if (item.Count > columnNumber)
                {
                    columnNumber = item.Count;
                }
            }


            //处理列名
            List<string> tmpColumnNameLst = new List<string>();

            if (columnNameLst == null) //如果为空，则用默认
            {
                for (int i = 0; i < columnNumber; i++)
                {
                    tmpColumnNameLst.Add(i.ToString());

                }
            }
            else
            {
                foreach (var item in columnNameLst)
                {

                    tmpColumnNameLst.Add(item.ToString());
                }

            }



            DataTable dt = new DataTable();

            //让一组一组的数据填入到对应的列里
            foreach (var item in tmpColumnNameLst)
            {
                dt.Columns.Add(item, typeof(T));
            }




            DataRow row;

            foreach (var item in listLink)
            {
                row = dt.NewRow();
                for (int i = 0; i < item.Count; i++)
                {
                    row[tmpColumnNameLst[i].ToString()] = item[i];
                }

                dt.Rows.Add(row);
            }
            return dt;
        }





        /// <summary>
        /// 搜索同一列中是否有连续相同的值，如果有，返回行的首尾位置   最好只能比较int 和 string，其它不保证正确
        /// </summary>
        /// <param name="dataArrLst">数据列表</param>
        /// <returns>如果某列有连续相同值，列作为key，一个或多个连续相同的行的首尾位置组成的列表作为值</returns>
        public static Dictionary<int, List<List<int>>> GetTheSameInformationMap<T>(List<List<T>> dataArrLst)
        {
            //返回值
            Dictionary<int, List<List<int>>> theSameInformationMap = new Dictionary<int, List<List<int>>>();
            for (int i = 0; i < dataArrLst.FirstOrDefault().Count; i++)
            {
                List<List<int>> theSameLineLst = GetTheSameLineInformationInOneColumn(dataArrLst, i);
                if (theSameLineLst.Count> 0)
                {
                    theSameInformationMap.Add(i, theSameLineLst);
                }
            }
            return theSameInformationMap;

        }


        /// <summary>
        /// 从指定列中获取值连续相同的首尾行位置的列表  最好只能比较int 和 string，其它不保证正确
        /// </summary>
        /// <param name="dataArrLst">数据列表</param>
        /// <param name="columnIndex">指定列位置</param>
        /// <returns>如果不存在，返回空的列表</returns>
        public static List<List<int>> GetTheSameLineInformationInOneColumn<T>(List<List<T>> dataArrLst, int columnIndex)
        {

            //返回值
            List<List<int>> theSameLineLst = new List<List<int>>();

            if(dataArrLst==null || dataArrLst.Count==0)
            {
                return theSameLineLst;
            }

            //可能列超过了
            if (columnIndex>=dataArrLst.FirstOrDefault().Count)
            {
                return theSameLineLst;
            }

            //从第一行开始
            int startRow = 0;

            //至多在倒数第二个
            while(startRow < dataArrLst.Count-1)
            {
                var columnValue = dataArrLst[startRow][columnIndex];

                int endRow = startRow;//结束行
                //以下为查找结束行的位置
                for (int j = 1 + endRow; j < dataArrLst.Count; j++)
                {
                    if (columnValue.ToString()== dataArrLst[j][columnIndex].ToString()) //相等的情况
                    {
                        endRow = j;
                    }
                    else //不相等的情况
                    {
                        break;
                    }
                }

                if (endRow>startRow) //找到了
                {
                    List<int> rowLst = new List<int>
                    {
                        startRow,
                        endRow
                    };
                    theSameLineLst.Add(rowLst);
                   
                    //从下一行开始
                    startRow = endRow+1;
                }
                else //没找到
                {
                    startRow++;
                }
            }
            return theSameLineLst;
        }



        ///// <summary>
        ///// 从指定列中获取值连续相同的首尾行位置的列表  最好只能比较int 和 string，其它不保证正确  6:15 2023/8/25 可以用
        ///// </summary>
        ///// <param name="dataArrLst">数据列表</param>
        ///// <param name="columnIndex">指定列位置</param>
        ///// <returns>如果不存在，返回空的列表</returns>
        //public static List<List<int>> GetTheSameLineInformationInOneColumn<T>(List<List<T>> dataArrLst, int columnIndex)
        //{

        //    //返回值
        //    List<List<int>> theSameLineLst = new List<List<int>>();

         //if(dataArrLst==null || dataArrLst.Count==0)
         //   {
         //       return theSameLineLst;
         //   }

    //    for (int i = 0; i < dataArrLst.Count; i++)
    //    {
    //        int startRow = i;
    //        int endRow = 0;//结束行
    //        var columnValue = dataArrLst[i][columnIndex];
    //        for (int j = 1 + i; j < dataArrLst.Count; j++)
    //        {
    //            if (columnValue.ToString()== dataArrLst[j][columnIndex].ToString())
    //            {
    //                endRow = j;
    //            }
    //            else
    //            {
    //                i = --j;
    //                break;
    //            }
    //        }
    //        if (endRow != 0)
    //        {
    //            List<int> rowLst = new List<int>
    //            {
    //                startRow,
    //                endRow
    //            };
    //            theSameLineLst.Add(rowLst);
    //            if (endRow == dataArrLst.Count -1)
    //            {
    //                break;
    //            }
    //        }
    //    }
    //    return theSameLineLst;
    //}







}

}
