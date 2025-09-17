using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Mrf.CSharp.BaseTools.Extension;
using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;

namespace Mrf.CSharp.BaseTools
{




    /// <summary>
    /// excel工具
    /// </summary>
    public class ExcelTool : IDisposable
    {
        private string fileName = null; //文件名
        private IWorkbook workbook = null;
        private FileStream fs = null;
        private bool disposed;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="fileName"></param>
        public ExcelTool(string fileName)//构造函数，读入文件名
        {
            this.fileName = fileName;
            disposed = false;
        }



        /// 将excel中的数据导入到DataTable中,判断文件是否存在
        /// <param name="sheetName">excel工作薄sheet的名称,如果为空，则取第一个sheet，默认为空</param>
        /// <param name="isFirstRowColumn">第一行是否是DataTable的列名,默认为否</param>
        /// <returns>返回的DataTable，如果出错，返回null，如果文件名为空或者不存在，返回null</returns>
        public DataTable ExcelToDataTable(string sheetName = null, bool isFirstRowColumn = false)
        {

            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
            {
                return null;
            }


            //创建临时文件，避免文件处于打开状态

            //string oldFileName = fileName;


            FileTool fileTool = new FileTool();
            string tmpNewFileName = fileTool.FindNewName(fileName);


            if (string.IsNullOrEmpty(tmpNewFileName))
            {
                return null;
            }
            //先复制一个文件，避免文件处于打开状态,之后记得删除
            File.Copy(fileName, tmpNewFileName);


            //fileName = tmpNewFileName;


            //返回值
            DataTable data = new DataTable();
            try
            {


                //用using，防止读错时 删除临时文件会报错
                //using (fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                using (fs = new FileStream(tmpNewFileName, FileMode.Open, FileAccess.Read))
                {

                    workbook = WorkbookFactory.Create(fs);

                    ISheet sheet;
                    if (string.IsNullOrEmpty(sheetName)) //如果为空，则取第一个sheet
                    {
                        sheet = workbook.GetSheetAt(0);
                    }
                    else
                    {
                        sheet = workbook.GetSheet(sheetName);
                        //如果没有找到指定的sheetName对应的sheet，则尝试获取第一个sheet
                        if (sheet == null)
                        {
                            sheet = workbook.GetSheetAt(0);
                        }
                    }


                    if (sheet != null)
                    {
                        IRow firstRow = sheet.GetRow(0);

                        //列的编号从0开始，会自动从非空的列开始，也就是说，如果 第一列不为空，则firstCellNum=0，第一列为空第二列不为空，firstCellNum=1
                        int firstCellNum = firstRow.FirstCellNum; //第一行第一个非空cell的编号
                        int lastCellNum = firstRow.LastCellNum; //第一行最后一个cell的编号,相当于列的长度
                        int startRow;
                        if (isFirstRowColumn)  //第一行为标题的情况
                        {
                            for (int i = firstCellNum; i < lastCellNum; ++i)
                            {
                                ICell cell = firstRow.GetCell(i);
                                if (cell != null)
                                {
                                    string cellValue = cell.StringCellValue;
                                    if (cellValue != null)
                                    {
                                        DataColumn column = new DataColumn(cellValue);
                                        data.Columns.Add(column);
                                    }
                                }
                            }
                            startRow = sheet.FirstRowNum + 1;//得到项标题后
                        }
                        else //第一行不为标题的情况
                        {
                            startRow = sheet.FirstRowNum;

                            //因为列名没有给，使用数字代替
                            for (int i = firstCellNum; i < lastCellNum; i++)
                            {
                                data.Columns.Add(i.ToString());
                            }


                        }
                        //最后一行的标号
                        int rowCount = sheet.LastRowNum;
                        for (int i = startRow; i <= rowCount; ++i)
                        {
                            IRow row = sheet.GetRow(i);
                            if (row == null) continue; //没有数据的行默认是null　　　　　　　

                            DataRow dataRow = data.NewRow();
                            for (int j = firstCellNum; j < lastCellNum; j++)
                            {
                                ICell iCell = row.GetCell(j);
                                if (iCell != null) //同理，没有数据的单元格都默认是null
                                {




                                    //string value = iCell.StringCellValue;
                                    //if (value != null)
                                    //{
                                    //    dataRow[j] = value;
                                    //}

                                    //不能用StringCellValue，因为有可能有纯数字的情况
                                    //value = iCell.StringCellValue;
                                    string value = iCell.ToString();
                                    if (string.IsNullOrEmpty(value))  //针对为空或null的情况，设置为""
                                    {
                                        value = "";
                                    }

                                    dataRow[j] = value;






                                }
                            }
                            data.Rows.Add(dataRow);
                        }
                    }



                    //删除
                    File.Delete(tmpNewFileName);

                    //fileName = oldFileName;

                }

                return data;
            }
            catch (Exception ex)//打印错误信息
            {

                //删除
                File.Delete(tmpNewFileName);

                //fileName = oldFileName;

                MessageBox.Show("Exception: " + ex.Message);
                return null;
            }

        }


        /// <summary>
        /// 从Excel中读取数据列表,判断文件是否存在
        /// </summary>
        /// <param name="isFirstRowColumn">第一行是否是DataTable的列名,默认为否</param>
        /// <param name="isRestrictRow">是否限制读取空白行,默认为否</param>
        /// <returns>数据列表，如果出错，返回null，如果文件名为空或者不存在，返回null</returns>
        public Dictionary<string,List<List<string>>> ExcelToDictionary(bool isFirstRowColumn = false,bool isRestrictRow = false)
        {
            //返回值
            Dictionary<string, List<List<string>>> sheetNameAndDataLstMap = new Dictionary<string, List<List<string>>>();

            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
            {
                return null;
            }

            //创建临时文件，避免文件处于打开状态

            FileTool fileTool = new FileTool();
            string tmpNewFileName = fileTool.FindNewName(fileName);


            if (string.IsNullOrEmpty(tmpNewFileName))
            {
                return null;
            }
            //先复制一个文件，避免文件处于打开状态,之后记得删除
            File.Copy(fileName, tmpNewFileName);

            int startRow = 0;
            if (isFirstRowColumn)  //第一行为列名称
            {
                startRow = 1;
            }

            try
            {
                using (fs = new FileStream(tmpNewFileName, FileMode.Open, FileAccess.Read))
                {
                    workbook = WorkbookFactory.Create(fs);

                    for (int i = 0; i < workbook.NumberOfSheets; i++)
                    {
                        string sheetName = workbook.GetSheetName(i);

                        ISheet sheet = workbook.GetSheet(sheetName);

                        if (sheet != null)
                        {
                            List<List<string>> dataLstMap = new List<List<string>>();

                            IRow firstRow = sheet.GetRow(0);

                            //列的编号从0开始，会自动从非空的列开始，也就是说，如果 第一列不为空，则firstCellNum=0，第一列为空第二列不为空，firstCellNum=1
                            int firstCellNum = firstRow.FirstCellNum; //第一行第一个非空cell的编号
                            int lastCellNum = firstRow.LastCellNum; //第一行最后一个cell的编号,相当于列的长度


                            //最后一行的标号
                            int rowCount = sheet.LastRowNum;
                            for (int j = startRow; j <= rowCount; ++j)
                            {
                                IRow row = sheet.GetRow(j);
                                if (row == null) continue; //没有数据的行默认是null　　　　　　　

                                //记录每一行的值
                                List<string> rowData = new List<string>();

                                //string[] rowData = new string[lastCellNum - firstCellNum];
                                //int index = 0;
                                for (int k = firstCellNum; k < lastCellNum; k++)
                                {
                                    ICell iCell = row.GetCell(k);

                                    //默认为空
                                    string value = "";
                                    if (iCell != null) //同理，没有数据的单元格都默认是null
                                    {
                                        //不能用StringCellValue，因为有可能有纯数字的情况
                                        //value = iCell.StringCellValue;
                                        value = iCell.ToString().Trim();
                                        if (string.IsNullOrEmpty(value))  //针对为空或null的情况，设置为""
                                        {
                                            value = "";
                                        }
                                    }
                                    if (isRestrictRow)
                                    {
                                        if (!string.IsNullOrEmpty(value))
                                        {
                                            rowData.Add(value);
                                        }
                                    }
                                    else
                                    {
                                        rowData.Add(value);
                                    }
                                    //rowData[index] = value;
                                    //index++;
                                }
                                if (rowData.Count > 0)
                                {
                                    dataLstMap.Add(rowData);
                                }
                            }
                            sheetNameAndDataLstMap.Add(sheetName, dataLstMap);
                        }
                    }
                    //删除
                    File.Delete(tmpNewFileName);
                }
            }
            catch (Exception ex)
            {

                //删除
                File.Delete(tmpNewFileName);

                //fileName = oldFileName;


                MessageBox.Show("Exception: " + ex.Message);
                return null;
            }


            return sheetNameAndDataLstMap;
        }







        /// <summary>
        /// 将excel中读取数据列表,判断文件是否存在
        /// </summary>
        /// <param name="sheetName">excel工作薄sheet的名称,如果为空，则取第一个sheet，默认为空</param>
        /// <param name="isFirstRowColumn">第一行是否是DataTable的列名,默认为否</param>
        /// <returns>数据列表，如果出错，返回null，如果文件名为空或者不存在，返回null</returns>
        public List<string[]> ExcelToList(string sheetName = null, bool isFirstRowColumn = false)
        {

            //返回值
            List<string[]> dataLst = new List<string[]>();



            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
            {
                return null;
            }





            //创建临时文件，避免文件处于打开状态

            //string oldFileName = fileName;


            FileTool fileTool = new FileTool();
            string tmpNewFileName = fileTool.FindNewName(fileName);


            if (string.IsNullOrEmpty(tmpNewFileName))
            {
                return null;
            }
            //先复制一个文件，避免文件处于打开状态,之后记得删除
            File.Copy(fileName, tmpNewFileName);


            //fileName = tmpNewFileName;





            ////先让用户手动关闭文件，如果关闭了 就往下执行，否则结束

            ////先判断文件是否已经打开
            //bool isOpened = IsFileOpened();

            //if (isOpened) //已经打开，操作失败
            //{
            //    return null;
            //}


            int startRow = 0;
            if (isFirstRowColumn)  //第一行为列名称
            {
                startRow = 1;
            }

            //int startRow = 0;


            try
            {

                //用using，防止读错时 删除临时文件会报错
                //using (fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                using (fs = new FileStream(tmpNewFileName, FileMode.Open, FileAccess.Read))
                {

                    //fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                    workbook = WorkbookFactory.Create(fs);


                    ISheet sheet;
                    if (string.IsNullOrEmpty(sheetName)) //如果为空，则取第一个sheet
                    {
                        sheet = workbook.GetSheetAt(0);
                    }
                    else
                    {
                        sheet = workbook.GetSheet(sheetName);
                        //如果没有找到指定的sheetName对应的sheet，则尝试获取第一个sheet
                        if (sheet == null)
                        {
                            sheet = workbook.GetSheetAt(0);
                        }
                    }


                    if (sheet != null)
                    {
                        IRow firstRow = sheet.GetRow(0);

                        //列的编号从0开始，会自动从非空的列开始，也就是说，如果 第一列不为空，则firstCellNum=0，第一列为空第二列不为空，firstCellNum=1
                        int firstCellNum = firstRow.FirstCellNum; //第一行第一个非空cell的编号
                        int lastCellNum = firstRow.LastCellNum; //第一行最后一个cell的编号,相当于列的长度


                        //最后一行的标号
                        int rowCount = sheet.LastRowNum;
                        for (int i = startRow; i <= rowCount; ++i)
                        {
                            IRow row = sheet.GetRow(i);
                            if (row == null) continue; //没有数据的行默认是null　　　　　　　

                            //记录每一行的值
                            string[] rowData = new string[lastCellNum - firstCellNum];
                            int index = 0;
                            for (int j = firstCellNum; j < lastCellNum; j++)
                            {
                                ICell iCell = row.GetCell(j);

                                //默认为空
                                string value = "";
                                if (iCell != null) //同理，没有数据的单元格都默认是null
                                {
                                    //不能用StringCellValue，因为有可能有纯数字的情况
                                    //value = iCell.StringCellValue;
                                    value = iCell.ToString();
                                    if (string.IsNullOrEmpty(value))  //针对为空或null的情况，设置为""
                                    {
                                        value = "";
                                    }
                                }

                                rowData[index] = value;
                                index++;

                            }
                            dataLst.Add(rowData);
                        }
                    }


                    //删除
                    File.Delete(tmpNewFileName);

                    //fileName = oldFileName;

                }
                return dataLst;
            }
            catch (Exception ex)//打印错误信息
            {

                //删除
                File.Delete(tmpNewFileName);

                //fileName = oldFileName;


                MessageBox.Show("Exception: " + ex.Message);
                return null;
            }
        }








        /// <summary>
        /// 将excel中读取数据,第一列作为键，其它作为列表的值返回映射，判断文件是否存在,如果数据读取失败或只有一列，返回空的映射
        /// </summary>
        /// <param name="sheetName">excel工作薄sheet的名称,如果为空，则取第一个sheet，默认为空</param>
        /// <param name="isFirstRowColumn">第一行是否是DataTable的列名,默认为否</param>
        /// <returns>第一列作为键，其它作为列表的值返回映射，如果读取有误或只有一列，返回空的映射</returns>
        public Dictionary<string, List<string>> ExcelToMap(string sheetName = null, bool isFirstRowColumn = false)
        {
            //返回值
            Dictionary<string, List<string>> keyAndValueMap = new Dictionary<string, List<string>>();

            List<string[]> dataStringLst = ExcelToList(sheetName, isFirstRowColumn);
            if (dataStringLst == null || dataStringLst[0].Length == 1) //如果读取有问题或者只有一列，直接返回空的映射
            {
                return keyAndValueMap;
            }

            int index = 0;

            //上面已经考虑了
            //if (isFirstRowColumn)  //第一行为列名称
            //{
            //    index = 1;
            //}


            for (int i = index; i < dataStringLst.Count; i++)
            {
                string[] keyAndValue = dataStringLst[i];
                string key = keyAndValue[0];
                List<string> value = new List<string>();
                for (int j = 1; j < keyAndValue.Length; j++)
                {
                    value.Add(keyAndValue[j]);
                }

                if (keyAndValueMap.ContainsKey(key)) //重复,清空映射，并返回
                {
                    MessageBox.Show("第 " + i + " 行键值\" " + key + " \"重复,请检查.", "Tips");

                    keyAndValueMap.Clear();
                    return keyAndValueMap;
                }
                keyAndValueMap[key] = value;

            }


            return keyAndValueMap;

        }





        /// <summary>
        /// 先判断文件是否已经打开,如果已经打开，会一直让用户关闭，除非用户选择，取消，会返回true
        /// </summary>
        /// <returns>如果打开，返回true，否则，返回false</returns>
        private bool IsFileOpened()
        {

            bool isOpened = FileTool.IsOpen(fileName);

            while (isOpened)
            {

                string message = "文件：\n" + fileName + "\n已打开,请先手动关闭，再按 确定，否则按 取消";

                DialogResult dialogResult = MessageBox.Show(message, "tips", MessageBoxButtons.OKCancel);

                if (dialogResult == DialogResult.Cancel)  //如果按了取消按钮，直接返回
                {
                    return true;
                }

                //按了确定，继续判断
                isOpened = FileTool.IsOpen(fileName);

            }

            //到这一步了，说明没有打开或者已经手动关闭掉了
            return false;
        }




        /// <summary>
        /// 将字符串列表输出到excel 如果文件不存在，自动创建
        /// </summary>
        /// <param name="stringDataLst">字符串列表</param>
        /// <param name="sheetName">表格名称</param>
        /// <returns>如果失败，返回-1 如果成功，>=0,如果为0，成功，但是是个空的excel文件</returns>
        public int StringDataLstToExcel(List<List<string>> stringDataLst, string sheetName = "Sheet1")
        {

            #region list转换为datatable


            //DataTable table = new DataTable();


            ////先获取最大的列数
            //int columnMaxNumber = 1;
            //foreach (var item in stringDataLst)
            //{
            //    if (item.Count > columnMaxNumber)
            //    {
            //        columnMaxNumber = item.Count;
            //    }
            //}


            //for (int i = 0; i < columnMaxNumber; i++)
            //{

            //    //设置标题
            //    DataColumn column = new DataColumn
            //    {
            //        DataType = typeof(string) //列数据类型
            //    };
            //    table.Columns.Add(column);//将列添加到表中
            //}


            //foreach (var item in stringDataLst)  //每一行
            //{

            //    DataRow dataRow = table.NewRow();

            //    for (int i = 0; i < item.Count; i++)
            //    {
            //        if (string.IsNullOrEmpty(item[i]))
            //        {
            //            dataRow[i] = "";

            //        }
            //        else
            //        {
            //            dataRow[i] = item[i];

            //        }

            //    }

            //    table.Rows.Add(dataRow);

            //}

            #endregion


            //list转换为datatable

            var table = ListExtension.ToDataTable<string>(stringDataLst);


            return DataTableToExcel(table, sheetName);
        }



        ///将列表的列表数据导入到excel中 ,如果文件不存在，会自动创建
        ///<param name="sheetNameAndDataLstMap">要导入的excel的sheet的名称和要导入的数据映射</param>
        ///<param name="isColumnWritten">DataTable的列名是否要导入,默认不导入</param>
        ///<returns>导入数据行数(包含列名那一行)，如果返回-1，则操作失败</returns>
        public int StringDataLstToExcel(Dictionary<string, List<List<string>>> sheetNameAndDataLstMap, bool isColumnWritten = false)
        {

            Dictionary<string, DataTable> sheetNameAndDataTableMap = new Dictionary<string, DataTable>();

            foreach (var item in sheetNameAndDataLstMap)
            {
                var table = ListExtension.ToDataTable<string>(item.Value);
                sheetNameAndDataTableMap[item.Key] = table;
            }


            return DataTableToExcel(sheetNameAndDataTableMap, isColumnWritten);

        }




        /// <summary>
        /// 创建一个sheet
        /// </summary>
        ///<param name="data">要导入的数据</param>
        ///<param name="sheetName">要导入的excel的sheet的名称，默认名字为Sheet1</param>
        ///<param name="isColumnWritten">DataTable的列名是否要导入,默认不导入</param>
        ///<returns>创建的sheet</returns>
        private ISheet CreateSheet(DataTable data, string sheetName = "Sheet1", bool isColumnWritten = false)
        {

            ISheet sheet = workbook.CreateSheet(sheetName);


            int count;
            if (isColumnWritten == true) //写入DataTable的列名
            {
                IRow row = sheet.CreateRow(0);
                for (int j = 0; j < data.Columns.Count; ++j)
                {
                    row.CreateCell(j).SetCellValue(data.Columns[j].ColumnName);
                }
                count = 1;
            }
            else
            {
                count = 0;
            }

            for (int i = 0; i < data.Rows.Count; ++i)
            {
                IRow row = sheet.CreateRow(count);
                for (int j = 0; j < data.Columns.Count; ++j)
                {

                    var cellValue = data.Rows[i][j];

                    //判断一下是否为字符串，如果为字符串，且以"="开头，则为公式，需要专门考虑
                    if (cellValue is string cellValueString && cellValueString.StartsWith("="))
                    {
                        //需要将"="号去掉
                        row.CreateCell(j).SetCellFormula(cellValueString.Replace("=", ""));
                    }
                    else if (cellValue is int cellValueInt)  //为整数
                    {
                        row.CreateCell(j).SetCellValue(cellValueInt);

                    }

                    else if (cellValue is double cellValueDouble) //为浮点数
                    {
                        row.CreateCell(j).SetCellValue(cellValueDouble);

                    }

                    else if (cellValue is bool cellValueBool) //为浮点数
                    {
                        row.CreateCell(j).SetCellValue(cellValueBool);

                    }

                    else if (cellValue is DateTime cellValueDateTime) //为浮点数
                    {
                        row.CreateCell(j).SetCellValue(cellValueDateTime);

                    }

                    else  //其它全部当字符串处理
                    {
                        row.CreateCell(j).SetCellValue(cellValue.ToString());

                    }

                }
                ++count;
            }

            return sheet;

        }

















        ///将DataTable数据导入到excel中 ,如果文件不存在，会自动创建 ,因为是加数据到excel中，如果同名文件已经存在，不能创建临时文件，所以只能判断文件有没有打开
        ///<param name="sheetNameAndDataTableMap">要导入的excel的sheet的名称和要导入的数据映射</param>
        ///<param name="isColumnWritten">DataTable的列名是否要导入,默认不导入</param>
        ///<returns>导入数据行数(包含列名那一行)，如果返回-1，则操作失败</returns>
        public int DataTableToExcel(Dictionary<string, DataTable> sheetNameAndDataTableMap, bool isColumnWritten = false)
        {
            //fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);

            if (string.IsNullOrEmpty(fileName))
            {
                return -1;
            }

            if (File.Exists(fileName))  //如果存在，就要判断是否已经打开
            {

                ////先让用户手动关闭文件，如果关闭了 就往下执行，否则结束

                //先判断文件是否已经打开
                bool isOpened = IsFileOpened();

                if (isOpened) //已经打开， 并且选择了取消 操作失败
                {
                    return -1;
                }

            }


            else //因为传入文件，如果没有，需要先创建
            {

                //先判断路径是否存在，如果不存在，先创建路径
                string directoryName = Path.GetDirectoryName(fileName);
                if (!Directory.Exists(directoryName))
                {
                    //会递增的创建所有子目录
                    Directory.CreateDirectory(directoryName);

                }




                //再创建文件

                //File.Create(fileName).Close();

                //再创建文件 要用using或close 这个是开启了内存，不然文件会处于打开状态，没法后续操作
                using (File.Create(fileName))
                {

                };
            }

            try
            {
                if (fileName.IndexOf(".xlsx") > 0) // 2007版本


                    workbook = new XSSFWorkbook();  //这里报过错 ，原因未知
                else if (fileName.IndexOf(".xls") > 0) // 2003版本
                    workbook = new HSSFWorkbook();


                if (workbook == null)
                {
                    ////删除
                    //File.Delete(tmpNewFileName);

                    //fileName = oldFileName;


                    return -1;
                }

                foreach (KeyValuePair<string, DataTable> kvp in sheetNameAndDataTableMap)
                {
                    ISheet sheet = CreateSheet(kvp.Value, kvp.Key, isColumnWritten);
                }



                fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);


                workbook.Write(fs); //写入到excel

                fs.Close();

                ////删除
                //File.Delete(tmpNewFileName);

                //fileName = oldFileName;


                return 1;
            }
            catch (Exception ex)
            {


                ////删除
                //File.Delete(tmpNewFileName);

                //fileName = oldFileName;

                Console.WriteLine("Exception: " + ex.Message);
                return -1;
            }
        }






        ///将DataTable数据导入到excel中 ,如果文件不存在，会自动创建
        ///<param name="data">要导入的数据</param>
        ///<param name="sheetName">要导入的excel的sheet的名称，默认名字为Sheet1</param>
        ///<param name="isColumnWritten">DataTable的列名是否要导入,默认不导入</param>
        ///<returns>导入数据行数(包含列名那一行)，如果返回-1，则操作失败</returns>
        public int DataTableToExcel(DataTable data, string sheetName = "Sheet1", bool isColumnWritten = false)
        {
            //fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);

            if (string.IsNullOrEmpty(fileName))
            {
                return -1;
            }


            if (File.Exists(fileName))  //如果存在，就要判断是否已经打开
            {

                ////先让用户手动关闭文件，如果关闭了 就往下执行，否则结束

                //先判断文件是否已经打开
                bool isOpened = IsFileOpened();

                if (isOpened) //已经打开， 并且选择了取消 操作失败
                {
                    return -1;
                }

            }


            else //因为传入文件，如果没有，需要先创建
            {

                //先判断路径是否存在，如果不存在，先创建路径
                string directoryName = Path.GetDirectoryName(fileName);
                if (!Directory.Exists(directoryName))
                {
                    //会递增的创建所有子目录
                    Directory.CreateDirectory(directoryName);

                }

                //再创建文件 要用using或close 这个是开启了内存，不然文件会处于打开状态，没法后续操作
                using (File.Create(fileName))
                {

                };


            }


            try
            {
                if (fileName.IndexOf(".xlsx") > 0) // 2007版本


                    workbook = new XSSFWorkbook();  //这里报过错 ，原因未知
                else if (fileName.IndexOf(".xls") > 0) // 2003版本
                    workbook = new HSSFWorkbook();


                if (workbook == null)
                {

                    return -1;
                }


                ISheet sheet = CreateSheet(data, sheetName, isColumnWritten);

                fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);


                workbook.Write(fs); //写入到excel




                fs.Close();

                return 1;
            }
            catch (Exception ex)
            {

                Console.WriteLine("Exception: " + ex.Message);
                return -1;
            }
        }








        /////将DataTable数据导入到excel中 ,如果文件不存在，会自动创建    14:17 2022/5/11 可以用
        /////<param name="data">要导入的数据</param>
        /////<param name="sheetName">要导入的excel的sheet的名称，默认名字为Sheet1</param>
        /////<param name="isColumnWritten">DataTable的列名是否要导入,默认不导入</param>
        /////<returns>导入数据行数(包含列名那一行)，如果返回-1，则操作失败</returns>
        //public int DataTableToExcel(DataTable data, string sheetName = "Sheet1", bool isColumnWritten = false)
        //{
        //    //fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);

        //    if (string.IsNullOrEmpty(fileName))
        //    {
        //        return -1;
        //    }

        //    if (!File.Exists(fileName))  //因为传入文件，如果没有，需要先创建
        //    {

        //        //先判断路径是否存在，如果不存在，先创建路径
        //        string directoryName = Path.GetDirectoryName(fileName);
        //        if (!Directory.Exists(directoryName))
        //        {
        //            //会递增的创建所有子目录
        //            Directory.CreateDirectory(directoryName);

        //        }

        //        //再创建文件
        //        File.Create(fileName);

        //    }


        //    ////先让用户手动关闭文件，如果关闭了 就往下执行，否则结束

        //    ////先判断文件是否已经打开
        //    //bool isOpened = IsFileOpened();

        //    //if (isOpened) //已经打开，操作失败
        //    //{
        //    //    return -1;
        //    //}


        //    ////创建临时文件，避免文件处于打开状态

        //    //string oldFileName = fileName;


        //    //FileTool fileTool = new FileTool();
        //    //string tmpNewFileName = fileTool.FindNewName(fileName);


        //    //if (string.IsNullOrEmpty(tmpNewFileName))
        //    //{
        //    //    return -1;
        //    //}
        //    ////先复制一个文件，避免文件处于打开状态,之后记得删除
        //    //File.Copy(fileName, tmpNewFileName);


        //    //fileName = tmpNewFileName;






        //    try
        //    {
        //        if (fileName.IndexOf(".xlsx") > 0) // 2007版本


        //            workbook = new XSSFWorkbook();  //这里报过错 ，原因未知
        //        else if (fileName.IndexOf(".xls") > 0) // 2003版本
        //            workbook = new HSSFWorkbook();


        //        if (workbook == null)
        //        {
        //            ////删除
        //            //File.Delete(tmpNewFileName);

        //            //fileName = oldFileName;


        //            return -1;
        //        }





        //        ISheet sheet = workbook.CreateSheet(sheetName);



        //        int count;
        //        if (isColumnWritten == true) //写入DataTable的列名
        //        {
        //            IRow row = sheet.CreateRow(0);
        //            for (int j = 0; j < data.Columns.Count; ++j)
        //            {
        //                row.CreateCell(j).SetCellValue(data.Columns[j].ColumnName);
        //            }
        //            count = 1;
        //        }
        //        else
        //        {
        //            count = 0;
        //        }

        //        for (int i = 0; i < data.Rows.Count; ++i)
        //        {
        //            IRow row = sheet.CreateRow(count);
        //            for (int j = 0; j < data.Columns.Count; ++j)
        //            {

        //                var cellValue = data.Rows[i][j];

        //                //判断一下是否为字符串，如果为字符串，且以"="开头，则为公式，需要专门考虑
        //                if (cellValue is string cellValueString && cellValueString.StartsWith("="))
        //                {
        //                    //需要将"="号去掉
        //                    row.CreateCell(j).SetCellFormula(cellValueString.Replace("=", ""));
        //                }
        //                else if (cellValue is int cellValueInt)  //为整数
        //                {
        //                    row.CreateCell(j).SetCellValue(cellValueInt);

        //                }

        //                else if (cellValue is double cellValueDouble) //为浮点数
        //                {
        //                    row.CreateCell(j).SetCellValue(cellValueDouble);

        //                }

        //                else if (cellValue is bool cellValueBool) //为浮点数
        //                {
        //                    row.CreateCell(j).SetCellValue(cellValueBool);

        //                }

        //                else if (cellValue is DateTime cellValueDateTime) //为浮点数
        //                {
        //                    row.CreateCell(j).SetCellValue(cellValueDateTime);

        //                }

        //                else  //其它全部当字符串处理
        //                {
        //                    row.CreateCell(j).SetCellValue(cellValue.ToString());

        //                }

        //            }
        //            ++count;
        //        }

        //        fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);


        //        workbook.Write(fs); //写入到excel



        //        //设置最大列宽 最大行高还不知道怎么弄

        //        //int maxColumn = data.Columns.Count;
        //        //AutoSizeColumn(sheet, maxColumn);
        //        AutoSizeColumn(sheet);


        //        fs.Close();




        //        ////删除
        //        //File.Delete(tmpNewFileName);

        //        //fileName = oldFileName;


        //        return count;
        //    }
        //    catch (Exception ex)
        //    {


        //        ////删除
        //        //File.Delete(tmpNewFileName);

        //        //fileName = oldFileName;

        //        Console.WriteLine("Exception: " + ex.Message);
        //        return -1;
        //    }
        //}

















        /// <summary>
        /// 自动设置最大列宽
        /// </summary>
        /// <returns>如果成功，返回true，否则，返回false</returns>
        public bool AutoSizeColumn()
        {
            //返回值
            bool isSucceed = false;


            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
            {
                return isSucceed;
            }


            ////先让用户手动关闭文件，如果关闭了 就往下执行，否则结束

            //先判断文件是否已经打开
            bool isOpened = IsFileOpened();

            if (isOpened) //已经打开， 并且选择了取消 操作失败
            {
                return isSucceed;
            }


            // 打开Excel文件
            Excel.Application excel = new Excel.Application();

            try
            {

                Excel.Workbook workbook = excel.Workbooks.Open(fileName);

                // 调整所有单元格的内容为完全可见
                foreach (Excel.Worksheet worksheet in workbook.Worksheets)
                {
                    worksheet.Cells.EntireColumn.AutoFit();
                    worksheet.Cells.EntireRow.AutoFit();
                }

                // 保存并关闭Excel文件
                workbook.Save();
                workbook.Close();

                // 释放COM对象
                Marshal.ReleaseComObject(workbook);
                Marshal.ReleaseComObject(excel);

                isSucceed = true;
            }
            catch
            {
                workbook.Close();

                // 释放COM对象
                Marshal.ReleaseComObject(workbook);
                Marshal.ReleaseComObject(excel);
            }


            return isSucceed;
        }





        /// <summary>
        /// 修改指定行列表的颜色
        /// </summary>
        /// <returns>如果成功，返回true，否则，返回false</returns>
        public bool ChangeRowsColor(List<string> sheetNameLst, List<int> rowNumberLst, System.Drawing.Color color)
        {
            //返回值
            bool isSucceed = false;


            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
            {
                return isSucceed;
            }


            ////先让用户手动关闭文件，如果关闭了 就往下执行，否则结束

            //先判断文件是否已经打开
            bool isOpened = IsFileOpened();

            if (isOpened) //已经打开， 并且选择了取消 操作失败
            {
                return isSucceed;
            }


            // 打开Excel文件
            Excel.Application excel = new Excel.Application();

            try
            {


                Excel.Workbook workbook = excel.Workbooks.Open(fileName);


                // 调整所有单元格的内容为完全可见
                foreach (Excel.Worksheet worksheet in workbook.Worksheets)
                {
                    string sheetName = worksheet.Name;

                    if (!sheetNameLst.Contains(sheetName))
                    {
                        continue;
                    }


                    Excel.Range usedRange = worksheet.UsedRange;

                    Excel.Range rows = usedRange.Rows;

                    int count = 0;

                    foreach (Excel.Range row in rows)
                    {

                        if (rowNumberLst.Contains(count))
                        {
                            Excel.Range firstCell = row.Cells[1];

                            string firstCellValue = firstCell.Value as String;

                            if (!string.IsNullOrEmpty(firstCellValue))
                            {
                                row.Interior.Color = color;
                            }
                        }

                        count++;
                    }
                }

                // 保存并关闭Excel文件
                workbook.Save();
                workbook.Close();

                // 释放COM对象
                Marshal.ReleaseComObject(workbook);
                Marshal.ReleaseComObject(excel);

                isSucceed = true;
            }
            catch
            {
                workbook.Close();

                // 释放COM对象
                Marshal.ReleaseComObject(workbook);
                Marshal.ReleaseComObject(excel);
            }

            return isSucceed;

        }






        /// <summary>
        /// 将AnyDataType数据导入到excel中
        /// </summary>
        /// <param name="dataArr">要导入的数据</param>
        /// <param name="sheetName">要导入的excel的sheet的名称，默认名字为Sheet1</param>
        /// <returns>如果成功，返回dataArr的总长度，否则，返回-1</returns>
        public int DataToExcel(AnyDataType[,] dataArr, string sheetName = "Sheet1")
        {


            if (string.IsNullOrEmpty(fileName))
            {
                return -1;
            }


            if (File.Exists(fileName))  //如果存在，就要判断是否已经打开
            {

                ////先让用户手动关闭文件，如果关闭了 就往下执行，否则结束

                //先判断文件是否已经打开
                bool isOpened = IsFileOpened();

                if (isOpened) //已经打开， 并且选择了取消 操作失败
                {
                    return -1;
                }

            }


            else //因为传入文件，如果没有，需要先创建
            {

                //先判断路径是否存在，如果不存在，先创建路径
                string directoryName = Path.GetDirectoryName(fileName);
                if (!Directory.Exists(directoryName))
                {
                    //会递增的创建所有子目录
                    Directory.CreateDirectory(directoryName);

                }

                ////再创建文件
                //File.Create(fileName);

                //再创建文件 要用using或close 这个是开启了内存，不然文件会处于打开状态，没法后续操作
                using (File.Create(fileName))
                {

                };
            }







            try
            {
                if (fileName.IndexOf(".xlsx") > 0) // 2007版本

                    workbook = new XSSFWorkbook();  //这里报过错 ，原因未知
                else if (fileName.IndexOf(".xls") > 0) // 2003版本
                    workbook = new HSSFWorkbook();


                ISheet sheet;
                if (workbook != null)
                {
                    sheet = workbook.CreateSheet(sheetName);
                }
                else
                {

                    return -1;
                }


                //行数
                int rowLength = dataArr.GetLength(0);

                //列数
                int colLength = dataArr.GetLength(1);

                for (int i = 0; i < rowLength; i++)
                {

                    IRow row = sheet.CreateRow(i);
                    for (int j = 0; j < colLength; j++)
                    {

                        AnyDataType anyDataType = dataArr[i, j];
                        DataType dataType = anyDataType.DataType;

                        switch (dataType)
                        {
                            case DataType.Int: //为整数的情况
                                row.CreateCell(j).SetCellValue(anyDataType.IntValue);
                                break;

                            case DataType.Double: //为双精度的情况
                                row.CreateCell(j).SetCellValue(anyDataType.DoubleValue);
                                break;

                            case DataType.String:  //为字符串的情况
                                string stringValue = anyDataType.StringValue;

                                //判断一下是否为字符串，如果为字符串，且以"="开头，则为公式，需要专门考虑
                                if (stringValue.StartsWith("="))
                                {
                                    //需要将"="号去掉
                                    row.CreateCell(j).SetCellFormula(stringValue.Replace("=", ""));
                                }
                                else
                                {
                                    row.CreateCell(j).SetCellValue(stringValue);
                                }

                                break;
                        }

                    }
                }

                fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);

                workbook.Write(fs); //写入到excel

                fs.Close();

                return rowLength * colLength;
            }
            catch (Exception ex)
            {

                Console.WriteLine("Exception: " + ex.Message);
                return -1;
            }
        }

















        /// <summary>
        /// 读取excel中某一列的数据，判断文件是否存在
        /// </summary>
        /// <param name="columnName">列名</param>
        /// <param name="sheetName">excel工作薄sheet的名称,如果为空，则取第一个sheet，默认为空</param>
        /// <param name="isFirstRowColumn">第一行是否是DataTable的列名,默认为否</param>
        /// <returns>列内容的字符串列表，如果读取失败，返回空的列表</returns>
        public List<string> GetColunmDateToStringLst(string columnName, string sheetName = null, bool isFirstRowColumn = false)
        {

            //返回值
            List<string> dataStringLst = new List<string>();

            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
            {
                return dataStringLst;
            }



            ////先让用户手动关闭文件，如果关闭了 就往下执行，否则结束

            ////先判断文件是否已经打开
            //bool isOpened = IsFileOpened();

            //if (isOpened) //已经打开，操作失败
            //{
            //    return dataStringLst;
            //}



            //创建临时文件，避免文件处于打开状态

            //string oldFileName = fileName;


            FileTool fileTool = new FileTool();
            string tmpNewFileName = fileTool.FindNewName(fileName);


            if (string.IsNullOrEmpty(tmpNewFileName))
            {
                return dataStringLst;
            }
            //先复制一个文件，避免文件处于打开状态,之后记得删除
            File.Copy(fileName, tmpNewFileName);


            //fileName = tmpNewFileName;




            try
            {

                //用using，防止读错时 删除临时文件会报错
                //using (fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                using (fs = new FileStream(tmpNewFileName, FileMode.Open, FileAccess.Read))
                {


                    //fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);

                    workbook = WorkbookFactory.Create(fs);

                    ISheet sheet;
                    if (string.IsNullOrEmpty(sheetName)) //如果为空，则取第一个sheet
                    {
                        sheet = workbook.GetSheetAt(0);
                    }
                    else
                    {
                        sheet = workbook.GetSheet(sheetName);
                        //如果没有找到指定的sheetName对应的sheet，则尝试获取第一个sheet
                        if (sheet == null)
                        {
                            sheet = workbook.GetSheetAt(0);
                        }
                    }


                    if (sheet != null)
                    {
                        IRow firstRow = sheet.GetRow(0);

                        //列的编号从0开始，会自动从非空的列开始，也就是说，如果 第一列不为空，则firstCellNum=0，第一列为空第二列不为空，firstCellNum=1
                        int firstCellNum = firstRow.FirstCellNum; //第一行第一个非空cell的编号
                        int lastCellNum = firstRow.LastCellNum; //第一行最后一个cell的编号,相当于列的长度
                        int startRow;

                        int foundColumnNum = -1;

                        if (isFirstRowColumn)  //第一行为标题的情况
                        {
                            for (int i = firstCellNum; i < lastCellNum; ++i)
                            {
                                ICell cell = firstRow.GetCell(i);
                                if (cell != null)
                                {
                                    string cellValue = cell.StringCellValue;
                                    if (cellValue != null && cellValue == columnName)
                                    {
                                        foundColumnNum = i;
                                        break;

                                    }
                                }
                            }

                            startRow = sheet.FirstRowNum + 1;//得到项标题后
                        }
                        else //第一行不为标题的情况
                        {
                            startRow = sheet.FirstRowNum;

                            //因为列名没有给，使用数字代替
                            for (int i = firstCellNum; i < lastCellNum; i++)
                            {
                                if (i.ToString() == columnName)
                                {
                                    foundColumnNum = i;
                                    break;
                                }
                            }
                        }


                        if (foundColumnNum == -1)  //没有找到所找的列，直接结束
                        {

                            //删除
                            File.Delete(tmpNewFileName);

                            //fileName = oldFileName;


                            return dataStringLst;
                        }




                        //最后一行的标号
                        int rowCount = sheet.LastRowNum;


                        for (int i = startRow; i <= rowCount; ++i)
                        {
                            IRow row = sheet.GetRow(i);

                            string value = "";
                            if (row != null) //没有数据的行默认是""
                            {

                                ICell iCell = row.GetCell(foundColumnNum);

                                if (iCell != null)//同理，没有数据的单元格都默认是""
                                {

                                    ////不能用StringCellValue，因为有可能有纯数字的情况
                                    //value = iCell.StringCellValue;
                                    //value = iCell.ToString(); //用这个，如果由公式，会返回公式


                                    bool done = false;
                                    int index = 1;
                                    while (!done)
                                    {
                                        try
                                        {
                                            switch (index)
                                            {

                                                case 1:
                                                    string stringValue = iCell.StringCellValue;
                                                    value = stringValue;
                                                    break;

                                                case 2:
                                                    double intValue = iCell.NumericCellValue; //如果没有报错，且为空的，将返回0
                                                    value = intValue.ToString();
                                                    break;

                                                case 3:
                                                    bool boolValue = iCell.BooleanCellValue;
                                                    value = boolValue.ToString();
                                                    break;

                                                case 4:
                                                    DateTime dateTimeValue = iCell.DateCellValue;
                                                    value = dateTimeValue.ToString();
                                                    break;

                                                default:
                                                    value = iCell.ToString(); //用这个，如果由公式，会返回公式，适合所有的情况，不会报错
                                                    done = true;
                                                    break;
                                            }
                                        }
                                        catch
                                        { }

                                        if (!string.IsNullOrEmpty(value))
                                        {
                                            break;
                                        }

                                        index++;
                                    }

                                }
                            }
                            dataStringLst.Add(value);

                        }
                    }


                    //删除
                    File.Delete(tmpNewFileName);

                    //fileName = oldFileName;

                }

                return dataStringLst;
            }
            catch (Exception ex)//打印错误信息
            {

                //删除
                File.Delete(tmpNewFileName);

                //fileName = oldFileName;

                MessageBox.Show("Exception: " + ex.Message);
                dataStringLst.Clear();
                return dataStringLst;
            }
        }






        /// <summary>
        /// 回收方法
        /// </summary>
        public void Dispose()//IDisposable为垃圾回收相关的东西，用来显式释放非托管资源,这部分目前还不是非常了解
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 回收方法
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (fs != null)
                        fs.Close();
                }
                fs = null;
                disposed = true;
            }
        }

    }






















}
