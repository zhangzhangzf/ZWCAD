using ZWCAD.BaseTools.Extension;
using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.Geometry;
using System.Collections.Generic;


namespace ZWCAD.BaseTools
{
    /// <summary>
    /// 
    /// </summary>
    public class DBPointTool
    {



        #region Private Variables

        Document m_document;
        Database m_database;


        #endregion



        #region Default Constructor


        /// <summary>
        /// 构造函数
        /// </summary>
        public DBPointTool()
        {
        }


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="document">文档对象</param>
        public DBPointTool(Document document)
        {
            m_document = document;
            m_database=m_document.Database;
        }



        #endregion

        /// <summary>
        ///给定点列表创建点对象列表
        /// </summary>
        /// <param name="spaceId">空间对象的ObjectId</param>
        /// <param name="point3DLst">点列表</param>
        /// <returns>如果失败，返回空的列表</returns>
        public List<ObjectId> CreatePoints(ObjectId spaceId, List<Point3d> point3DLst)
        {
            //返回值
            List<ObjectId> objectIdLst = new List<ObjectId>();

            if (point3DLst==null || point3DLst.Count==0)
            {
                return objectIdLst;
            }


            List<Entity> entLst = new List<Entity>();


            foreach (var item in point3DLst)
            {
                DBPoint dBPoint = new DBPoint(item);
                entLst.Add(dBPoint);
            }

            objectIdLst= m_database.AddEntities(entLst, spaceId);

            return objectIdLst;

        }


        /// <summary>
        ///给定点列表创建点对象列表
        /// </summary>
        /// <param name="point3DLst">点列表</param>
        /// <param name="spaceName">空间名称，默认模型空间</param>
        /// <returns>如果失败，返回空的列表</returns>
        public List<ObjectId> CreatePoints( List<Point3d> point3DLst, string spaceName = "MODELSPACE")

        {
            //返回值
            List<ObjectId> objectIdLst = new List<ObjectId>();

            if (point3DLst==null || point3DLst.Count==0)
            {
                return objectIdLst;
            }


            List<Entity> entLst = new List<Entity>();


            foreach (var item in point3DLst)
            {
                DBPoint dBPoint = new DBPoint(item);
                entLst.Add(dBPoint);
            }

            objectIdLst= m_database.AddEntities(entLst, spaceName);

            return objectIdLst;

        }


    }
}
