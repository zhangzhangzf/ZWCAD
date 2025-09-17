using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using Point3dInCAD = ZwSoft.ZwCAD.Geometry.Point3d;

namespace ZWCAD.BaseTools
{
    /// <summary>
    /// 图表对象工具
    /// </summary>
    public class TableTool
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
        public TableTool(Document document)
        {
            m_document = document;
            m_database = m_document.Database;
        }


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="database">数据库对象</param>
        public TableTool(Database database)
        {
            m_database = database;
        }



        #endregion


        /// <summary>
        /// 创建表格
        /// </summary>
        /// <param name="spaceId">所在空间的ObjectId</param>
        /// <param name="insertPoint">插入点</param>
        /// <param name="isTitleMerge">是否取消标题</param>
        /// <param name="rowsNum">行数</param>
        /// <param name="columnsNum">列数</param>
        /// <param name="rowHeight">行高</param>
        /// <param name="columnWidth">列宽</param>
        /// <returns>表格对象的ObjectId，如果失败，返回ObjectId.Null</returns>
        public ObjectId CreateTable(ObjectId spaceId, Point3dInCAD insertPoint, bool isTitleMerge = true, int rowsNum = 5, int columnsNum = 5, double rowHeight = 3, double columnWidth = 20)
        {
            //返回值
            ObjectId tableId = ObjectId.Null;

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                if (transaction.GetObject(spaceId, OpenMode.ForWrite) is BlockTableRecord btr)
                {

                    try
                    {
                        // create a table

                        Table tb = new Table
                        {
                            TableStyle = m_database.Tablestyle
                        };

                        tb.SetSize(rowsNum, columnsNum);

                        tb.SetRowHeight(rowHeight);

                        tb.SetColumnWidth(columnWidth);


                        if (isTitleMerge) //是否隐藏标题
                        {
                            //用"Data"会报错：eKeyNotFound 可能因为这个是中文版本的
                            tb.Rows[0].Style="数据";
                        }

                        // insert rows and columns

                        //tb.InsertRows(0, rowheight, rowsNum);

                        //tb.InsertColumns(0, columnwidth, columnsNum);


                        tb.Position = insertPoint;

                        tb.Cells.Alignment = CellAlignment.MiddleCenter;


                        tb.GenerateLayout();

                        tableId = btr.AppendEntity(tb);

                        transaction.AddNewlyCreatedDBObject(tb, true);

                        transaction.Commit();

                    }
                    catch
                    {
                        transaction.Abort();
                    }
                }


            }

            return tableId;
        }



        /// <summary>
        /// 往已存在的表格中添加数据列表 如果行或列超过了，会自动添加
        /// </summary>
        /// <param name="tableId">表格的ObjectId</param>
        /// <param name="dataArrLst">数据列表</param>
        /// <param name="startRow">开始添加的起始行</param>
        public bool AddDataToTable(ObjectId tableId, List<List<string>> dataArrLst, int startRow = 0)
        {
            //返回值
            bool isSucceed = false;

            if (tableId.IsNull)
            {
                return isSucceed;
            }

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {

                    DBObject dBObject = transaction.GetObject(tableId, OpenMode.ForWrite);
                    if (dBObject is Table table)
                    {

                        int tableRowNumber = table.Rows.Count;
                        int tableColumnNumber = table.Columns.Count;



                        //如果要添加的数据行数和列数大于表格的行数和列数，则自动扩充表格
                        int restRowNumber = dataArrLst.Count-tableRowNumber+startRow;



                        //添加行
                        if (restRowNumber>0)
                        {

                            //16:58 2023/8/31 直接用继承某一行的属性，不然会用默认的格式，导致格式不符合
                            //最后一行的高度
                            //double height = table.Rows[tableRowNumber-1].Height;

                            //table.InsertRows(tableRowNumber-1, height, restRowNumber);
                            //table.InsertRows(tableRowNumber, height, restRowNumber);

                            //这个会在tableRowNumber的前面加一行，对于tableRowNumber为最后一行的情况，
                            //直接用tableRowNumber+1 就会在tableRowNumber的后面加一行，不会报超过范围这种报错
                            table.InsertRowsAndInherit(tableRowNumber, tableRowNumber-1, restRowNumber);
                        }


                        //添加列

                        int dataColumnNumber = int.MinValue;

                        dataArrLst.ForEach(x => dataColumnNumber=Math.Max(dataColumnNumber, x.Count));

                        int restColumnNumber = dataColumnNumber-tableColumnNumber;


                        if (restColumnNumber>0)
                        {
                            //double width = table.Columns[tableColumnNumber-1].Width;
                            //table.InsertColumns(tableColumnNumber-1, width, restColumnNumber);

                            table.InsertColumnsAndInherit(tableColumnNumber, tableColumnNumber-1, restColumnNumber);

                        }




                        for (int row = startRow; row <startRow+ dataArrLst.Count; row++)
                        {

                            for (int column = 0; column <dataArrLst[row-startRow].Count; column++)
                            {
                                table.Cells[row, column].TextString=dataArrLst[row-startRow][column];
                            }
                        }

                        isSucceed=true;

                    }

                    transaction.Commit();
                }
                catch
                {
                    isSucceed=false;
                    transaction.Abort();

                }

            }

            return isSucceed;
        }





        /// <summary>
        /// 往已存在的表格最后数据列表，自动添加行和列
        /// </summary>
        /// <param name="tableId">表格的ObjectId</param>
        /// <param name="dataArrLst">数据列表</param>
        /// <returns>如果成功，返回true,否则，返回false</returns>
        public bool AppendDataToTable(ObjectId tableId, List<List<string>> dataArrLst)
        {
            //返回值
            bool isSucceed = false;

            int startRow = GetTableRowNumber(tableId);
            if (startRow<0)
            {
                return isSucceed;
            }

            return AddDataToTable(tableId, dataArrLst, startRow);

        }







        /// <summary>
        /// 获取表格对象的行数
        /// </summary>
        /// <param name="tableId">表格的ObjectId</param>
        /// <returns>表格对象的行数，如果失败，返回-1</returns>
        public int GetTableRowNumber(ObjectId tableId)
        {
            //返回值
            int tableRowNumber = -1;

            if (tableId.IsNull)
            {
                return tableRowNumber;
            }

            using (Transaction transaction = m_document.TransactionManager.StartTransaction())
            {
                try
                {
                    DBObject dBObject = transaction.GetObject(tableId, OpenMode.ForWrite);
                    if (dBObject is Table table)
                    {
                        tableRowNumber = table.Rows.Count;
                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();
                }
            }
            return tableRowNumber;
        }





        ///// <summary>
        ///// 往已存在的表格中添加数据列表   22:00 2023/8/29 可以用
        ///// </summary>
        ///// <param name="tableId">表格的ObjectId</param>
        ///// <param name="dataArrLst">数据列表</param>
        ///// <param name="startRow">开始添加的其实行</param>
        //public void AddDataToTable(ObjectId tableId, List<List<string>> dataArrLst, int startRow = 0)
        //{

        //    using (Transaction transaction = m_document.TransactionManager.StartTransaction())
        //    {
        //        try
        //        {

        //            DBObject dBObject = transaction.GetObject(tableId, OpenMode.ForWrite);
        //            if (dBObject is Table table)
        //            {

        //                int tableRowNumber = table.Rows.Count;
        //                int tableColumnNumber = table.Columns.Count;



        //                //如果要添加的数据行数和列数大于表格的行数和列数，则自动扩充表格




        //                for (int row = startRow; row < (dataArrLst.Count<tableRowNumber ? dataArrLst.Count : tableRowNumber); row++)
        //                {

        //                    for (int column = 0; column <(dataArrLst[row].Count<tableColumnNumber ? dataArrLst[row].Count : tableColumnNumber); column++)
        //                    {
        //                        table.Cells[row, column].TextString=dataArrLst[row][column];
        //                    }
        //                }

        //            }

        //            transaction.Commit();
        //        }
        //        catch
        //        {
        //            transaction.Abort();

        //        }


        //    }
        //}







        /// <summary>
        /// 设置表格的文字样式
        /// </summary>
        /// <param name="tableId">表格对象的ObjectId</param>
        /// <param name="styleName">文字样式名称</param>
        /// <returns>如果成功，返回true,否则，返回false</returns>
        public bool SetTextStyle(ObjectId tableId, string styleName)
        {
            //返回值
            bool isSucceed = false;

            DatabaseTool databaseTool = new DatabaseTool(m_database);
            ObjectId styleId = databaseTool.GetTextStyleByName(styleName);

            if (!styleId.IsNull)
            {
                using (Transaction transaction = m_database.TransactionManager.StartTransaction())
                {
                    try
                    {
                        if (transaction.GetObject(tableId, OpenMode.ForWrite) is Table table)
                        {
                            table.Cells.TextStyleId=styleId;
                            isSucceed = true;
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Abort();
                    }
                }
            }

            return isSucceed;
        }

        /// <summary>
        /// 设置表格的文字高度
        /// </summary>
        /// <param name="tableId">表格对象的ObjectId</param>
        /// <param name="height">文字高度</param>
        /// <returns>如果成功，返回true,否则，返回false</returns>
        public bool SetTextHeight(ObjectId tableId, double height)
        {
            //返回值
            bool isSucceed = false;

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    if (transaction.GetObject(tableId, OpenMode.ForWrite) is Table table)
                    {
                        table.Cells.TextHeight=height;
                        isSucceed = true;
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();
                }
            }

            return isSucceed;
        }


        /// <summary>
        /// 设置表格某一列的文字宽度
        /// </summary>
        /// <param name="tableId">表格对象的ObjectId</param>
        /// <param name="column">列数</param>
        /// <param name="width">文字宽度</param>
        /// <returns>如果成功，返回true,否则，返回false</returns>
        public bool SetColumnWidth(ObjectId tableId, int column, double width)
        {
            //返回值
            bool isSucceed = false;

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    if (transaction.GetObject(tableId, OpenMode.ForWrite) is Table table)
                    {
                        table.Columns[column].Width=width;
                        isSucceed = true;
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();
                }
            }

            return isSucceed;
        }




        /// <summary>
        /// 合并单元格范围
        /// </summary>
        /// <param name="tableId">表格对象的ObjectId</param>
        /// <param name="topRow">上部所在的行号</param>
        /// <param name="leftColumn">左侧所在的列号</param>
        /// <param name="bottomRow">下部所在的行号</param>
        /// <param name="rightColumn">右侧所在的行号</param>
        /// <returns>如果成功，返回true,否则，返回false</returns>
        public bool MergeCellRange(ObjectId tableId, int topRow, int leftColumn, int bottomRow, int rightColumn)
        {
            //返回值
            bool isSucceed = false;

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    if (transaction.GetObject(tableId, OpenMode.ForWrite) is Table table)
                    {
                        CellRange cellRange = CellRange.Create(table, topRow, leftColumn, bottomRow, rightColumn);
                        table.MergeCells(cellRange);
                        isSucceed = true;
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();
                }
            }

            return isSucceed;
        }



        /// <summary>
        /// 向表格中指定行指定列添加块
        /// </summary>
        /// <param name="tableId">表格的ObjectId</param>
        /// <param name="blockRecordId">块表记录的ObjectId</param>
        /// <param name="rowindex">行号</param>
        /// <param name="columnindex">列号</param>
        /// <returns>如果成功，返回true,否则，返回false</returns>
        public bool AddBlockToTable(ObjectId tableId, ObjectId blockRecordId, int rowindex = 0, int columnindex = 0)
        {
            //返回值
            bool isSucceed = false;

            if (tableId.IsNull || blockRecordId.IsNull)
            {
                return isSucceed;
            }


            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    if (transaction.GetObject(tableId, OpenMode.ForWrite) is Table table)
                    {

                        if (table.Rows.Count > rowindex && table.Columns.Count > columnindex)
                        {
                            if (table.Cells[rowindex, columnindex].Contents.Count > 0)
                            {
                                table.Cells[rowindex, columnindex].Contents[0].BlockTableRecordId = blockRecordId;
                            }
                            else
                            {
                                table.Cells[rowindex, columnindex].Contents.Add();
                                table.Cells[rowindex, columnindex].Contents[0].BlockTableRecordId = blockRecordId;
                            }

                            isSucceed = true;
                        }
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();
                }
            }

            return isSucceed;
        }





        /// <summary>
        /// 向表格中指定行指定列添加块
        /// </summary>
        /// <param name="tableId">表格的ObjectId</param>
        /// <param name="blockName">块表记录的名称</param>
        /// <param name="rowindex">行号</param>
        /// <param name="columnindex">列号</param>
        /// <returns>如果成功，返回true,否则，返回false</returns>
        public bool AddBlockToTable(ObjectId tableId, string blockName, int rowindex = 0, int columnindex = 0)
        {
            BlockTool blockTool = new BlockTool(m_database);
            ObjectId blockRecordId = blockTool.FindBlockTableRecordIdByName(blockName);

            return AddBlockToTable(tableId, blockRecordId, rowindex, columnindex);
        }


        ////https://adndevblog.typepad.com/autocad/2012/05/how-to-create-a-table-and-fill-in-its-cells-with-net.html

        //[CommandMethod("testaddtable")]

        //public static void testaddtable()

        //{

        //    Database db =

        //        HostApplicationServices.WorkingDatabase;





        //    using (Transaction tr =

        //        db.TransactionManager.StartTransaction())

        //    {

        //        BlockTable bt =

        //            (BlockTable)tr.GetObject(db.BlockTableId,

        //                                    OpenMode.ForRead);

        //        ObjectId msId =

        //            bt[BlockTableRecord.ModelSpace];



        //        BlockTableRecord btr =

        //            (BlockTableRecord)tr.GetObject(msId,

        //                                OpenMode.ForWrite);



        //        // create a table

        //        Table tb = new Table();

        //        tb.TableStyle = db.Tablestyle;



        //        // row number

        //        Int32 RowsNum = 5;

        //        // column number

        //        Int32 ColumnsNum = 5;



        //        // row height

        //        double rowheight = 3;

        //        // column width

        //        double columnwidth = 20;



        //        // insert rows and columns

        //        tb.InsertRows(0,

        //                    rowheight,

        //                    RowsNum);

        //        tb.InsertColumns(0,

        //                    columnwidth,

        //                    ColumnsNum);



        //        tb.SetRowHeight(rowheight);

        //        tb.SetColumnWidth(columnwidth);



        //        Point3d eMax = db.Extmax;

        //        Point3d eMin = db.Extmin;

        //        double CenterY =

        //            (eMax.Y + eMin.Y) * 0.5;

        //        tb.Position =

        //            new Point3d(10, 10, 0);



        //        // fill in the cell one by one

        //        for (int i = 0;

        //            i < RowsNum;

        //            i++)

        //        {

        //            for (int j = 0;

        //                j < ColumnsNum;

        //                j++)

        //            {

        //                tb.Cells[i, j].TextHeight = 1;

        //                if (i == 0 && j == 0)

        //                    tb.Cells[i, j].TextString =

        //                        "The Title";

        //                else

        //                    tb.Cells[i, j].TextString =

        //                        i.ToString() + "," + j.ToString();



        //                tb.Cells[i, j].Alignment =

        //                    CellAlignment.MiddleCenter;

        //            }

        //        }



        //        tb.GenerateLayout();

        //        btr.AppendEntity(tb);

        //        tr.AddNewlyCreatedDBObject(tb, true);

        //        tr.Commit();

        //    }



        //}





    }
}
