


using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;

namespace ZWCAD.BaseTools
{
    /// <summary>
    /// 线形对象工具
    /// </summary>
    public class LineTypeTool
    {



        #region Private Variables

        Document m_document;

        Database m_database;

        #endregion



        #region Default Constructor


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="document">文档对象</param>
        public LineTypeTool(Document document)
        {
            m_document = document;
            m_database = m_document.Database;
        }



        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="database">数据库对象</param>
        public LineTypeTool(Database database)
        {
            m_database = database;
        }


        #endregion




        /// <summary>
        /// 获取或加载指定线形
        /// </summary>
        /// <param name="ltname">线形的名称</param>
        /// <param name="ltFileName">线形文件名称，全路径，默认系统自带</param>
        /// <returns>如果存在，返回已存在线形的ObjectId，否则加载，如果成功，返回新加载对象的ObjectId,否则，返回ObjectId.Null</returns>
        public ObjectId GetOrLoadLineType(string ltname, string ltFileName = "acad.lin")
        {
            //返回值
            ObjectId lineTypeId = ObjectId.Null;


            using (Transaction tr = m_database.TransactionManager.StartTransaction())
            {
                LinetypeTable tbl = (LinetypeTable)tr.GetObject(m_database.LinetypeTableId, OpenMode.ForRead);

                if (tbl.Has(ltname))  //已经存在
                {
                    lineTypeId= tbl[ltname];
                }

                else //不存在，加载
                {
                    try
                    {
                        m_database.LoadLineTypeFile(ltname, ltFileName);// imperic
                        tr.Commit();
                        lineTypeId= tbl[ltname];
                    }
                    catch
                    {
                        tr.Abort();
                    }
                }
            }

            return lineTypeId;
        }







    }
}
