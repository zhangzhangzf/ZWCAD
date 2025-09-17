/**********************************************************
Copyright(C) UIH 2011                                     *
描    述：                                                *
版    本：4.0.30319.42000                                 *
作    者：莫瑞芳                                          *
创建时间：2021/10/14 17:15:36                             *
**********************************************************/

using ZwSoft.ZwCAD.DatabaseServices;
using System;
using System.Globalization;

namespace ZWCAD.BaseTools.Extension
{
    /// <summary>
    /// ObjectId扩展
    /// </summary>
    public static class ObjectIdExtension
    {

        /// <summary>
        /// 通过ObjectId的数字字符串返回ObjectId
        /// </summary>
        /// <param name="Int64String">ObjectId的数字字符串</param>
        /// <returns>ObjectId,格式如(1112385),不判断模型中是否含有该对象</returns>
        public static ObjectId GetObjectIdByString(string Int64String)
        {

            ObjectId objectId = new ObjectId(new IntPtr(Convert.ToInt64(Int64String))); //需要转换为int64, int32和int都会报错
            
            return objectId;
        }



        /// <summary>
        /// 将handle转换为objectId
        /// </summary>
        /// <param name="db">database</param>
        /// <param name="handle">handle字符串</param>
        /// <returns>ObjectId,格式如(1112385),不判断模型中是否含有该对象,如果失败，返回ObjectId.Null</returns>
        public static ObjectId GetObjectIdByHandle(this Database db, string handle)
        {

            //15:57 2023/12/20 用这个有报错的情况 修改

            //Handle h = new Handle(long.Parse(handle, NumberStyles.AllowHexSpecifier));

            // Convert hexadecimal string to 64-bit integer
            long ln = Convert.ToInt64(handle, 16);

            // Not create a Handle from the long integer
            Handle hn = new Handle(ln);

         

            if (!  db.TryGetObjectId(hn, out var  id))
            {
                id=ObjectId.Null;
            }
          
            return id;
        }



        /// <summary>
        /// 删除DBObject对象
        /// </summary>
        /// <param name="dbObjectId">DBObject对象的ObjectId</param>
        /// <param name="isRegenScreen">是否重生成屏幕，虽然删除掉对象后，不会再存在于数据库中，但是只有重生成屏幕后，才不会显示。重生成需要耗费时间，默认重生成</param>
        public static void EraseEntity(this ObjectId dbObjectId, bool isRegenScreen = true)
        {

            using (Transaction transaction = dbObjectId.Database.TransactionManager.StartTransaction())
            {

                DBObject dBObject = transaction.GetObject(dbObjectId, OpenMode.ForWrite, true);
                dBObject.Erase(true);
                if (isRegenScreen && dBObject is Entity entity)
                {
                    entity.RecordGraphicsModified(true);
                }
                transaction.Commit();
            }

        }





        /// <summary>
        ///泛型  获取对象 如：ObjectId.GetObject<Circle>()
        /// </summary>
        /// <param name="id">对象的ObjectId</param>
        /// <returns>对象,如果对象类型不匹配，则返回null</returns>
        public static T GetObject<T>(this ObjectId id) where T : class
        {
            //返回值
            T obj = null;

            using (Transaction transaction = id.Database.TransactionManager.StartTransaction())
            {
                obj = transaction.GetObject(id, OpenMode.ForRead, true) as T;
                transaction.Commit();
            }
            return obj;
        }







    }
}
