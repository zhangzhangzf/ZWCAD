using ZwSoft.ZwCAD.DatabaseServices;
using System.Collections.Generic;

namespace ZWCAD.BaseTools.Extension
{
    /// <summary>
    /// 事务的扩展类
    /// </summary>
    public static class TransactionExtension
    {


        /// <summary>
        /// 获取多个对象组成的边界
        /// </summary>
        /// <param name="transaction">事务对象</param>
        /// <param name="idArr">对象组成的数组</param>
        /// <returns>边界，如果失败，返回默认的边界对象</returns>
        public static Extents3d GetExtents(this Transaction transaction, ObjectId[] idArr)
        {
            var ext = new Extents3d();
            foreach (ObjectId id in idArr)
            {
                if (transaction.GetObject(id, OpenMode.ForRead) is Entity ent)
                {
                    ext.AddExtents(ent.GeometricExtents);
                }
            }
            return ext;
        }



        /// <summary>
        /// 获取多个对象组成的边界
        /// </summary>
        /// <param name="transaction">事务对象</param>
        /// <param name="idLst">对象组成的列表</param>
        /// <returns>边界，如果失败，返回默认的边界对象</returns>
        public static Extents3d GetExtents(this Transaction transaction,List< ObjectId> idLst)
        {
            ObjectId[] idArr=idLst.ToArray();
            return GetExtents(transaction,idArr);
        }







        }
}
