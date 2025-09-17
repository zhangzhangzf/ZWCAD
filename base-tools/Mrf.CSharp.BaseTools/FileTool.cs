
using Mrf.CSharp.BaseTools.Extension;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace Mrf.CSharp.BaseTools
{

    /// <summary>
    /// 文件工具
    /// </summary>
    public class FileTool
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public FileTool()
        {

        }




        /// <summary>
        /// 将字符串列表输入txt文件，一个字符串一行
        /// </summary>
        /// <param name="pathName">完整路径</param>
        /// <param name="dataLst">字符串列表</param>
        /// <param name="fileMode">文件创建模式</param>
        /// <param name="encoding">写入的文件格式，如果为null，是使用默认的Encoding.UTF8 好像这个格式不起作用 ，哪怕指定了，也都用Encoding.UTF8</param>
        public void SendDataLstToTxtFile(string pathName, List<string> dataLst, FileMode fileMode = FileMode.Create, Encoding encoding = null)
        {

            FileStream fs = new FileStream(pathName, fileMode);
            StreamWriter sw;

            if (encoding == null)
            {
                sw = new StreamWriter(fs);
            }
            else
            {
                sw = new StreamWriter(fs, encoding);

            }

            //开始写入
            foreach (string each in dataLst)
            {
                sw.WriteLine(each);
            }


            //清空缓冲区
            sw.Flush();

            //关闭流
            sw.Close();
            fs.Close();

        }




        /// <summary>
        /// 将字符串入txt文件
        /// </summary>
        /// <param name="pathName">完整路径</param>
        /// <param name="data">字符串</param>
        /// <param name="fileMode">文件创建模式</param>
        /// <param name="encoding">写入的文件格式，如果为null，是使用默认的Encoding.UTF8 好像这个格式不起作用 ，哪怕指定了，也都用Encoding.UTF8</param>

        public void SendDataToTxtFile(string pathName, string data, FileMode fileMode = FileMode.Create, Encoding encoding = null)
        {
            List<string> dataLst = new List<string>
            {
                data
            };

            SendDataLstToTxtFile(pathName, dataLst, fileMode, encoding);
        }












        /// <summary>
        /// 读取txt文件内容,判断文件是否存在
        /// </summary>
        /// <param name="pathName">文件路径</param>
        /// <param name="encoding">读取编码的问题，因为有中文为乱码的情况，如果为null，则默认为Encoding.Default</param>
        /// <returns>每一行内容组成的列表,如果文件不存在，返回null， 如果文件存在但没有值，返回空的列表</returns>
        public List<string> GetDataLstFromTxtFile(string pathName, Encoding encoding = null)
        {


            if (!File.Exists(pathName))
            {
                return null;
            }

            if (encoding == null) //如果为null，则读取处文件保存的编码格式
            {
                //encoding = Encoding.Default;

                //获取保存的编码格式
                encoding = GetTextFileEncodingType(pathName);

            }


            //返回值
            List<string> dataLst = new List<string>();



            ////先读取txt文件的编码格式,如果读取不出来，默认Encoding.Default
            //encoding =     TxtFileEncoding.GetEncoding(pathName);


            //需要考虑内容为中文的情况，第一种方法会有乱码，用第二种方法
            //StreamReader sr = File.OpenText(pathName);
            StreamReader sr = new StreamReader(pathName, encoding);



            string nextLine;

            //开始读
            while ((nextLine = sr.ReadLine()) != null)
            {
                dataLst.Add(nextLine);
            }

            sr.Close();

            return dataLst;

        }



        /// <summary>
        /// 读取txt文件内容,返回每行数字作为一个列表的列表，判断文件是否存在
        /// </summary>
        /// <param name="pathName">文件路径</param>
        /// <param name="encoding">读取编码的问题，因为有中文为乱码的情况，如果为null，则默认为Encoding.Default</param>
        /// <returns>每一行内容数字组成的列表,如果文件不存在，返回null， 如果文件存在但没有值或读取失败，返回空的列表</returns>
        public List<List<double>> GetDataNumberLstFromTxtFile(string pathName, Encoding encoding = null)
        {

            List<string> dataLst = GetDataLstFromTxtFile(pathName, encoding);

            //返回值
            List<List<double>> dataNumberLst = dataLst.GetNumberLstFromStringLst();

            return dataNumberLst;

        }



        /// <summary>
        /// 获取文本文件的字符编码类型
        /// </summary>
        /// <param name="fileName">文件的全路径名称</param>
        /// <returns>文件编码类型，如果找不到，返回默认Encoding.Default</returns>
        public static Encoding GetTextFileEncodingType(string fileName)
        {
            Encoding encoding = Encoding.Default;
            FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream, encoding);
            byte[] buffer = binaryReader.ReadBytes((int)fileStream.Length);
            binaryReader.Close();
            fileStream.Close();
            if (buffer.Length >= 3 && buffer[0] == 239 && buffer[1] == 187 && buffer[2] == 191)
            {

                //UTF-8格式
                //对应记事本保存的编码选项 带有BOM的UTF-8
                encoding = Encoding.UTF8;
            }
            else if (buffer.Length >= 3 && buffer[0] == 254 && buffer[1] == 255 && buffer[2] == 0)
            {
                //UTF-16格式  大内位字节顺序  (big endian byte order) 
                //对应记事本保存的编码选项 UTF-16BE
                encoding = Encoding.BigEndianUnicode;
            }
            else if (buffer.Length >= 2 && buffer[0] == 255 && buffer[1] == 254)
            {
                //UTF-16格式  小内位字节顺序  (little endian byte order)
                //对应记事本保存的编码选项 UTF-16 LE
                encoding = Encoding.Unicode;
            }
            else if (IsUTF8Bytes(buffer))
            {
                //UTF-8格式
                //对应记事本保存的编码选项 UTF-8 
                encoding = Encoding.UTF8;
            }

            ////UTF-7格式
            //Encoding encoding7 = Encoding.UTF7;

            ////UTF-32格式 小内位字节顺序  (little endian byte order) 
            //Encoding encoding4 = Encoding.UTF32;

            ////使用这个.NET执行的格式
            //Encoding encoding2 = Encoding.Default;

            ////使用 ASCII (7-bit)
            //Encoding encoding5 = Encoding.ASCII;


            return encoding;
        }




        /// <summary>
        /// 判断是否是不带 BOM 的 UTF8 格式
        /// BOM（Byte Order Mark），字节顺序标记，出现在文本文件头部，Unicode编码标准中用于标识文件是采用哪种格式的编码。
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static bool IsUTF8Bytes(byte[] data)
        {
            int charByteCounter = 1; //计算当前正分析的字符应还有的字节数
            byte curByte; //当前分析的字节.
            for (int i = 0; i < data.Length; i++)
            {
                curByte = data[i];
                if (charByteCounter == 1)
                {
                    if (curByte >= 0x80)
                    {
                        //判断当前
                        while (((curByte <<= 1) & 0x80) != 0)
                        {
                            charByteCounter++;
                        }
                        //标记位首位若为非0 则至少以2个1开始 如:110XXXXX...........1111110X
                        if (charByteCounter == 1 || charByteCounter > 6)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    //若是UTF-8 此时第一位必须为1
                    if ((curByte & 0xC0) != 0x80)
                    {
                        return false;
                    }
                    charByteCounter--;
                }
            }
            if (charByteCounter > 1)
            {
                throw new Exception("非预期的byte格式");
            }
            return true;
        }





        /// <summary>
        /// 从对话框选择一个或多个文件类型的文件
        /// </summary>
        /// <param name="fileTitle"></param>
        /// <param name="fileFilter"></param>
        /// <param name="isMultiSelect"></param>
        /// <returns>文件绝对路径数组，如果没有选择，返回null</returns>
        public string[] SelectFilesFromDialog(string fileTitle = "请选择文件", string fileFilter = "所有文件(*.*)|*.*", bool isMultiSelect = true)
        {
            OpenFileDialog fileDialog = new OpenFileDialog
            {

                //选择文件标题
                Title = fileTitle,

                //设置过滤器
                Filter = fileFilter,

                //是否可以多选文件
                Multiselect = isMultiSelect
            };

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                return fileDialog.FileNames;
            }

            return null;

        }







        /// <summary>
        /// 另存为对话框
        /// </summary>
        /// <param name="defaultFileName">默认的文件名称</param>
        /// <param name="fileTitle"></param>
        /// <param name="fileFilter"></param>
        /// <returns>文件绝对路径，如果没有选择，返回null</returns>
        public static string SaveFileFromDialog(string defaultFileName = "", string fileTitle = "文件另存为", string fileFilter = "所有文件(*.txt)|*.txt")
        {

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = fileTitle,
                Filter = fileFilter,
                //saveFileDialog.FilterIndex = 2;
                RestoreDirectory = true
            };

            if (!string.IsNullOrEmpty(defaultFileName))
            {
                saveFileDialog.FileName = defaultFileName;
            }

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                return saveFileDialog.FileName;
            }

            return null;

        }






        /// <summary>
        /// 设置文件属性为正常，比如将只读设置为能写 判断文件是否存在
        /// </summary>
        /// <param name="fileName">全路径名称</param>
        /// <returns>如果成功，返回true，否则，返回false</returns>
        public static bool SetFileNormal(string fileName)
        {
            //返回值
            bool isSucceed = false;

            if (File.Exists(fileName))
            {
                File.SetAttributes(fileName, FileAttributes.Normal);
                isSucceed = true;
            }

            return isSucceed;

        }



        /// <summary>
        /// 设置文件属性为只读，判断文件是否存在
        /// </summary>
        /// <param name="fileName">全路径名称</param>
        /// <returns>如果成功，返回true，否则，返回false</returns>
        public static bool SetFileReadOnly(string fileName)
        {
            //返回值
            bool isSucceed = false;

            if (File.Exists(fileName))
            {
                File.SetAttributes(fileName, FileAttributes.ReadOnly);
                isSucceed = true;
            }

            return isSucceed;

        }




        /// <summary>
        /// 如果文件不存在，返回原来的文件名，如果存在，在后面添加后缀_1、_2、_3...,并返回新的文件名
        /// </summary>
        /// <param name="absoluteOldFileName">旧的绝对路径名</param>
        /// <returns>新的绝对路径名</returns>
        public string FindNewName(string absoluteOldFileName)
        {
            //获取路径
            string directory = Path.GetDirectoryName(absoluteOldFileName);

            //获取纯文件名
            string fileName = Path.GetFileNameWithoutExtension(absoluteOldFileName);

            //获取扩展名
            string extension = Path.GetExtension(absoluteOldFileName);

            int count = 1;

            string absoluteNewFileName = absoluteOldFileName;

            while (File.Exists(absoluteNewFileName))
            {
                absoluteNewFileName = directory + "\\" + fileName + "_" + count + extension;
                count++;
            }

            return absoluteNewFileName;
        }







        /// <summary>
        /// 备份文件，新文件名为添加后缀_1、_2、_3...
        /// </summary>
        /// <param name="absoluteFileName">文件绝对路径</param>
        /// <returns>返回新的绝对路径名，如果没有替换，返回null</returns>
        public string CopyFile(string absoluteFileName)
        {
            if (File.Exists(absoluteFileName))
            {
                //获取新的文件名
                string newFileName = FindNewName(absoluteFileName);

                //修改文件名称
                File.Copy(absoluteFileName, newFileName);

                return newFileName;
            }

            return null;

        }





        /// <summary>
        /// 判断文件名是否合法，可为绝对文件名或纯文件名，可带扩展名或否
        /// </summary>
        /// <param name="fileName">可为绝对文件名或纯文件名，可带扩展名或否</param>
        /// <returns>如果成功，返回true，否则，返回false</returns>
        private bool IsFileNameValid(string fileName)
        {
            //返回值
            bool isFilename = true;


            //考虑绝对文件名的情况，去掉路径
            fileName = Path.GetFileNameWithoutExtension(fileName);


            string[] errorStr = new string[] { "/", "\\", ":", ",", "*", "?", "\"", "<", ">", "|" };

            if (string.IsNullOrEmpty(fileName))
            {
                isFilename = false;
            }
            else
            {
                for (int i = 0; i < errorStr.Length; i++)
                {
                    if (fileName.Contains(errorStr[i]))
                    {
                        isFilename = false;
                        break;
                    }
                }
            }
            return isFilename;
        }








        /// <summary>
        /// 如果文件存在，备份，备份文件名为添加后缀_1、_2、_3...
        /// </summary>
        /// <param name="absoluteFileName">文件绝对路径</param>
        /// <returns>返回新的绝对路径名，如果没有替换，返回null</returns>
        public string BackUpSameFileName(string absoluteFileName)
        {
            if (File.Exists(absoluteFileName))
            {
                //获取新的文件名
                string backUpFileName = FindNewName(absoluteFileName);



                //修改文件名称
                File.Move(absoluteFileName, backUpFileName);




                return backUpFileName;
            }

            return null;

        }




        /// <summary>
        /// 保存指定文件到新的文件
        /// </summary>
        /// <param name="absoluteOldFileName">全路径源文件名</param>
        /// <param name="isMaintainOldFile">是否保留旧文件，默认否</param>
        /// <param name="absoluteNewFileName">全路径新文件名，如果文件名非法或者为null，则在源文件名基础上加时间戳，加时间戳后文件名还存在，则加后缀_1、_2、_3...</param>
        /// <returns>如果保存成功，返回新文件名称，否则，返回null</returns>
        public string SaveToNewFileName(string absoluteOldFileName, bool isMaintainOldFile = false, string absoluteNewFileName = null)
        {
            if (File.Exists(absoluteOldFileName))
            {

                //如果 文件名非法 或者为null 则在原来文件的基础上加入时间戳
                if (string.IsNullOrEmpty(absoluteNewFileName) || !IsFileNameValid(absoluteNewFileName))
                {
                    //获取文件修改时间
                    var modifyTime = File.GetLastWriteTime(absoluteOldFileName).ToString("yyyyMMddHHmmss");



                    //路径 最后不带"\\" 或"/",如："E:\章刘洋给的资料\光伏支架"
                    string directory = Path.GetDirectoryName(absoluteOldFileName);

                    //纯文件名
                    string fileName = Path.GetFileNameWithoutExtension(absoluteOldFileName);

                    //扩展名,带".",如：".docx"
                    string extension = Path.GetExtension(absoluteOldFileName);

                    absoluteNewFileName = directory + "\\" + fileName + "-" + modifyTime + extension;
                }


                //验证文件名是否已经存在，如果不存在，返回原来的文件名，如果不存在，加后缀_1、_2、_3...
                string backUpFileName = FindNewName(absoluteNewFileName);



                //先复制一个文件，避免文件处于打开状态,之后记得删除

                ////修改文件名称
                //File.Move(absoluteOldFileName, backUpFileName);

                File.Copy(absoluteOldFileName, backUpFileName);


                if (!isMaintainOldFile) //不保留旧文件
                {
                    try
                    {
                        File.Delete(absoluteOldFileName);
                    }
                    catch
                    { }
                }


                return backUpFileName;
            }

            return null;

        }





        /// <summary>
        /// 删除文件夹中的所有文件包括文件夹本身 
        /// </summary>
        /// <param name="file">文件夹绝对路径</param>
        public void DeleteFile(string file)
        {
            //去除文件夹和子文件的只读属性
            //去除文件夹的只读属性
            System.IO.DirectoryInfo fileInfo = new DirectoryInfo(file);
            fileInfo.Attributes = FileAttributes.Normal & FileAttributes.Directory;
            //去除文件的只读属性
            System.IO.File.SetAttributes(file, System.IO.FileAttributes.Normal);
            //判断文件夹是否还存在
            if (Directory.Exists(file))
            {
                foreach (string f in Directory.GetFileSystemEntries(file))
                {
                    if (File.Exists(f))
                    {
                        //如果有子文件删除文件
                        File.Delete(f);
                    }
                    else
                    {
                        //循环递归删除子文件夹
                        DeleteFile(f);
                    }
                }
                //删除空文件夹
                Directory.Delete(file);
            }
        }







        ///<summary>
        /// 清空指定文件夹下的所有文件和子文件夹，但不删除文件夹
        /// </summary>
        /// <param name="dir">文件夹绝对路径</param>
        public static void DeleteFolder(string dir)
        {
            foreach (string d in Directory.GetFileSystemEntries(dir))
            {
                if (File.Exists(d))
                {
                    FileInfo fi = new FileInfo(d);
                    if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                        fi.Attributes = FileAttributes.Normal;
                    File.Delete(d);//直接删除其中的文件
                }
                else
                {
                    DirectoryInfo d1 = new DirectoryInfo(d);
                    if (d1.GetFiles().Length != 0)
                    {
                        DeleteFolder(d1.FullName); //递归删除子文件夹
                    }
                    Directory.Delete(d);
                }
            }
        }





        ///<summary>
        /// 删除指定文件夹下的所有带指定前缀指定文件类型的文件，但不删除文件夹
        /// </summary>
        /// <param name="dir">文件夹绝对路径</param>
        /// <param name="startWith">文件名称前缀</param>
        /// <param name="extension">文件类型，如：.txt</param>
        public static void DeleteFilesStartWith(string dir, string startWith, string extension)
        {

            if (!Directory.Exists(dir)) //不存在，直接返回
            {
                return;
            }


            foreach (string d in Directory.GetFileSystemEntries(dir))
            {


                if (!File.Exists(d) || //文件不存在
                   !d.EndsWith(extension)
                    )
                {
                    continue;
                }


                //需要获取纯文件名
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(d);

                if (!fileNameWithoutExtension.StartsWith(startWith))
                {
                    continue;
                }



                FileInfo fi = new FileInfo(d);
                if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                    fi.Attributes = FileAttributes.Normal;
                File.Delete(d);//直接删除其中的文件


            }
        }






























        /// <summary>
        /// C# 删除指定文件或文件夹
        /// string strFilePath = @”c:\ttt\ttt.txt”;
        ///或者 string strFilePath = @”c:\ttt\”;
        /// </summary>
        /// <param name="fileFullPath"></param>
        public void DeleteDirectoryOrFile(string fileFullPath)
        {
            // 1、首先判断文件或者文件路径是否存在
            if (File.Exists(fileFullPath))
            {
                // 2、根据路径字符串判断是文件还是文件夹
                FileAttributes attr = File.GetAttributes(fileFullPath);
                // 3、根据具体类型进行删除
                if (attr == FileAttributes.Directory)
                {
                    // 3.1、删除文件夹
                    Directory.Delete(fileFullPath, true);
                }
                else
                {
                    // 3.2、删除文件
                    File.Delete(fileFullPath);
                }
            }
        }





        /// <summary>
        /// 打开文件
        /// </summary>
        /// <param name="lpPathName"></param>
        /// <param name="iReadWrite"></param>
        /// <returns></returns>

        [DllImport("kernel32.dll")]

        public static extern IntPtr _lopen(string lpPathName, int iReadWrite);  //_lopen这个名字不能改 可能是kernel32.dll里定义的名字

        /// <summary>
        /// 关闭句柄
        /// </summary>
        /// <param name="hObject"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr hObject);

        /// <summary>
        /// 
        /// </summary>
        public const int OF_READWRITE = 2;

        /// <summary>
        /// 
        /// </summary>
        public const int OF_SHARE_DENY_NONE = 0x40;

        /// <summary>
        /// 
        /// </summary>
        public static readonly IntPtr HFILE_ERROR = new IntPtr(-1);

        /// <summary>
        /// 判断文件是否打开  前面这个函数有错误，是因为将_lopen写成了open,现在已经可以使用，具体原因未知
        /// </summary>
        /// <param name="fileFullPath">文件绝对路径</param>
        /// <returns>如果打开，返回true,否则，返回false</returns>
        public static bool IsOpen(string fileFullPath)
        {
            if (!File.Exists(fileFullPath))
            {
                return false;
            }
            IntPtr vHandle = _lopen(fileFullPath, OF_READWRITE | OF_SHARE_DENY_NONE);
            if (vHandle == HFILE_ERROR)
            {
                return true;
            }
            CloseHandle(vHandle);
            return false;
        }





        /// <summary>
        /// 获取Assembly的运行路径
        /// </summary>
        ///<returns>返回dll运行时所在的路径，如"E:/revitcad/RevitCAD/bin/Debug/"</returns>
        public static string GetAssemblyPath()
        {
            string _CodeBase = Assembly.GetExecutingAssembly().CodeBase;

            _CodeBase = _CodeBase.Substring(8, _CodeBase.Length - 8);    // 8是file:// 的长度

            string[] arrSection = _CodeBase.Split(new char[] { '/' });

            string _FolderPath = "";
            for (int i = 0; i < arrSection.Length - 1; i++)
            {
                _FolderPath += arrSection[i] + "/";
            }

            return _FolderPath;

        }




        /// <summary>
        /// 获取Assembly的运行路径,貌似跟GetAssemblyPath()一样的结果
        /// </summary>
        ///<returns>返回dll运行时所在的路径，如"E:/revitcad/RevitCAD/bin/Debug/"</returns>
        public static string GetAssemblyPath2()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }





        /// <summary>
        /// 在txt文件中读取数据,第一列作为键，其它作为列表的值返回映射，判断文件是否存在,如果数据读取失败或只有一列，返回空的映射
        /// </summary>
        /// <param name="pathName">文件路径</param>
        /// <param name="encoding">读取编码的问题，因为有中文为乱码的情况，如果为null，则默认为Encoding.Default</param>
        /// <param name="isFirstRowColumn">第一行是否是列名,默认为否</param>
        /// <param name="splitSymbols">用于分割的字符</param>
        /// <returns>第一列作为键，其它作为列表的值返回映射，如果读取有误或只有一列，返回空的映射</returns>
        public Dictionary<string, List<string>> GetDataMapFromTxtFile(string pathName, Encoding encoding = null, bool isFirstRowColumn = false, string splitSymbols = ", \t")
        {
            //返回值
            Dictionary<string, List<string>> keyAndValueMap = new Dictionary<string, List<string>>();



            List<string> dataLst = GetDataLstFromTxtFile(pathName, encoding);
            if (dataLst == null || dataLst.Count == 0)  //读取失败,返回空的映射
            {
                return keyAndValueMap;
            }






            List<List<string>> dataStringLst = dataLst.GetDataStringLstFromStringLst(splitSymbols);



            if (dataStringLst == null || dataStringLst[0].Count == 1) //如果读取有问题或者只有一列，直接返回空的映射
            {
                return keyAndValueMap;
            }

            int index = 0;
            if (isFirstRowColumn)  //第一行为列名称
            {
                index = 1;
            }


            for (int i = index; i < dataStringLst.Count; i++)
            {
                List<string> keyAndValue = dataStringLst[i];
                string key = keyAndValue[0];
                List<string> value = keyAndValue.GetRange(1, keyAndValue.Count - 1);

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
        /// 使用MD5生成唯一的图片名称；对于同一个文件，不管保存在哪个路径，返回的值都是一样的，哪怕文件的文件名称改掉了，还是返回一样的值
        /// </summary>
        /// <param name="filePath">图片的全路径名称</param>
        /// <returns>MD5的字符串</returns>
      public  static string CalculateMD5(string filePath)
        {
            using (FileStream fs = File.OpenRead(filePath))
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                List<byte> retval = md5.ComputeHash(fs).ToList();
                StringBuilder strings = new StringBuilder();
                retval.ForEach(b => strings.Append(b.ToString("x2")));
                return strings.ToString();
            }
        }







    }
}
