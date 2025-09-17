using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZWCAD.BaseTools
{
    /// <summary>
    /// 直线类工具
    /// </summary>
  public  class LineTool
    {


        Document m_document;
        Database m_database;


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="document">文档对象</param>
        public LineTool(Document document)
        {
            m_document = document;
            m_database = m_document.Database;

        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="database">图形数据库</param>
        public LineTool(Database database)
        {
            m_database = database;

        }







        /// <summary>
        /// 获取多段线的顶点三维坐标列表，判断是否为多段线，如果不是，返回空的列表
        /// </summary>
        /// <param name="objectId">多段线对象的ObjectId</param>
        /// <returns>顶点坐标列表，如果没有找到，返回空的列表</returns>
        public List<Point3d> GetLinePoint3dLst(ObjectId objectId)
        {
            //返回值
            List<Point3d> pointLst = new List<Point3d>();

            Database database = objectId.Database;

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                DBObject dBObject = transaction.GetObject(objectId, OpenMode.ForRead);
                if (dBObject is Line line)
                {
                    pointLst.Add(line.StartPoint);
                    pointLst.Add(line.EndPoint);
                }
                transaction.Commit();
            }

            return pointLst;

        }










    }
}
