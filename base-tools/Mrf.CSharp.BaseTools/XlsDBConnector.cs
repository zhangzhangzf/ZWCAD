
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;

namespace Mrf.CSharp.BaseTools
{
    /// <summary>
    ///连接xls文件，检索、更新数据
    /// </summary>
    public class XlsDBConnector : IDisposable
    {
        #region Class Memeber Variables
        // 创建的连接
        private OleDbConnection m_objConn;

        // 连接命令
        private OleDbCommand m_command;

        // 连接字符串
        private String m_connectStr;

        //xls文件的所有可用的表格（工作表）
        private List<String> m_tables = new List<String>();
        #endregion


        #region 构造函数/析构函数
        /// <summary>
        /// 构造函数，从xls文件中检索数据
        /// </summary>
        /// <param name="strXlsFile">要连接的.xls文件
        ///该文件应该存在且可读</param>
        public XlsDBConnector(String strXlsFile)
        {
            // 验证文件的有效性
            if (!ValidateFile(strXlsFile))
            {
                throw new ArgumentException("文件不存在或者仅可读", strXlsFile);
            }

            // 创建连接到文件
            m_connectStr = "Provider = Microsoft.Jet.OLEDB.4.0; Data Source = \"" + strXlsFile +
                "\"; Extended Properties = \"Excel 8.0;HDR=YES;\"";

            // 创建.xls连接
            m_objConn = new OleDbConnection(m_connectStr);
            m_objConn.Open();
        }

        /// <summary>
        /// 关闭OleDb连接
        /// </summary>
        public void Dispose()
        {
            if (null != m_objConn)
            {
                //关闭OleDb连接
                m_objConn.Close();
                m_objConn = null;
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        ///终结器，需要确认连接已经关闭
        ///这个析构函数仅在没有调用Dispose方法时运行
        /// </summary>
        ~XlsDBConnector()
        {
            Dispose();
        }
        #endregion


        #region Class Member Methods
        /// <summary>
        ///从.xls文件中获取所有可用的表格名称
        /// </summary>
        public List<string> RetrieveAllTables()
        {
            // 首先清理所有的旧表格列表
            m_tables.Clear();

            // 从数据源中获取所有的表格名称
            DataTable schemaTable = m_objConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables,
                new object[] { null, null, null, "TABLE" });
            for (int i = 0; i < schemaTable.Rows.Count; i++)
            {
                m_tables.Add(schemaTable.Rows[i].ItemArray[2].ToString().TrimEnd('$'));
            }


            return m_tables;
        }





        ////必须存在的五个列常量，以下检查
        //String[] constantNames = { RoomsData.RoomID, RoomsData.RoomName,
        //            RoomsData.RoomNumber, RoomsData.RoomArea, RoomsData.RoomComments };


        /// <summary>
        ///通过某一表格名称，从xls数据源中获取DataTable数据
        /// </summary>
        /// <param name="tableName">要被检索的表格名称 </param>
        /// <param name="constantNames">表格中列的名称列表</param>
        /// <returns>从工作表获取的DataTable</returns>
        public DataTable GenDataTable(String tableName, String[] constantNames)
        {
            //通过命令获取所有的数据，然后将数据添加到表格中
            string strCom = "Select * From [" + tableName + "$]";
            OleDbDataAdapter myCommand = new OleDbDataAdapter(strCom, m_objConn);
            DataSet myDataSet = new DataSet();
            myCommand.Fill(myDataSet, "[" + tableName + "$]");

            try
            {
                //验证表格中是否存在列常量（在类RoomData中定义)
                // 当更新表格时，需要这些列

                //定义一个标示变量来记录是否找到列
                // 不允许在表格中复制列
                bool[] bHasColumn = new bool[constantNames.Length];
                Array.Clear(bHasColumn, 0, constantNames.Length); // 将变量设置为false


                //记住所有重复的列，用来弹出错误信息
                String duplicateColumns = String.Empty;
                for (int i = 0; i < myDataSet.Tables[0].Columns.Count; i++)
                {
                    //获取每个列并检查
                    String columnName = myDataSet.Tables[0].Columns[i].ColumnName;

                    // 一个个检查是否有需要的列
                    for (int col = 0; col < bHasColumn.Length; col++)
                    {
                        bool bDupliate = CheckSameColName(columnName, constantNames[col]);
                        if (bDupliate)
                        {
                            //这种情况为第一次相等的时候，说明该列存在，且不算重复
                            if (false == bHasColumn[col])
                            {
                                bHasColumn[col] = true;
                            }

                            //这种情况为第二次以上出现的时候，为重复的情况
                            else
                            {
                                //这个列是复制的，保存下来
                                duplicateColumns += String.Format("[{0}], ", constantNames[col]);
                            }
                        }
                    }
                }

                //检查是否有重复的列
                if (duplicateColumns.Length > 0)
                {
                    //不允许复制列
                    String message = String.Format("表格中存在重复的列: {0}.", duplicateColumns);
                    throw new Exception(message);
                }


                // 检查是否存在所有要找的列
                String missingColumns = String.Empty; //保存所有丢失的列
                for (int col = 0; col < bHasColumn.Length; col++)
                {
                    //当bHasColumn为false时，说明不存在
                    if (bHasColumn[col] == false)
                    {
                        missingColumns += String.Format("[{0}], ", constantNames[col]);
                    }
                }

                //检查是否有丢失的列
                if (missingColumns.Length != 0)
                {
                    // 弹出丢失列的名字
                    String message = String.Format("要找的列丢失: {0}.", missingColumns);
                    throw new Exception(message);
                }

                //如果没有异常，直接返回数据集表格
                return myDataSet.Tables[0];
            }
            catch (Exception ex)
            {
                // 抛出异常
                throw new Exception(ex.Message);
            }
        }


        /// <summary>
        ///执行SQL命令，比如更新和插入
        /// </summary>
        /// <param name="strCmd">要执行的命令</param>
        /// <returns>被这个命令影响的行数</returns>
        public int ExecuteCommnand(String strCmd)
        {
            try
            {
                if (null == m_command)
                {
                    m_command = m_objConn.CreateCommand();
                }
                m_command.CommandText = strCmd;
                return m_command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString() + strCmd);
            }
        }
        #endregion


        #region Class Implementation
        /// <summary>
        ///这个方法将要验证和更新指定文件的属性
        /// 这个表格应该存在且具有可写属性
        ///如果仅可读，这个方法将会尝试设置其属性未可写
        /// </summary>
        /// <param name="strFile"></param>
        /// <returns></returns>
        private bool ValidateFile(String strFile)
        {
            // 检查是否存在
            if (!File.Exists(strFile))
            {
                return false;
            }
            //
            // 设置可写属性
            File.SetAttributes(strFile, FileAttributes.Normal);
            return (FileAttributes.Normal == File.GetAttributes(strFile));
        }
        /// <summary>
        ///检查两个列的名字是否一样
        /// </summary>
        /// <param name="baseName">第一个名字</param>
        /// <param name="compName">第二个名字</param>
        /// <returns>如果一样，返回true，否则，返回false</returns>
        private static bool CheckSameColName(String baseName, String compName)
        {
            if (String.Compare(baseName, compName) == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion
    };
}
