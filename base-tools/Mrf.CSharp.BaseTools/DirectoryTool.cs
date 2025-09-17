using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;


namespace Mrf.CSharp.BaseTools
{

    /// <summary>
    /// 文件夹工具
    /// </summary>
    public class DirectoryTool
    {



        /// <summary>
        /// 选择文件夹
        /// </summary>
        /// <param name="defaultTitle">默认标题</param>
        /// <param name="defaultPath">默认路径</param>
        /// <returns>返回选择的路径，如"E:\\AutoCAD"，如果取消，返回null</returns>
        public static string SelectDirectory(string defaultTitle = "选择文件所在文件夹", string defaultPath = "")
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowserDialog.Description = defaultTitle;
            folderBrowserDialog.SelectedPath = defaultPath;//默认路径
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return folderBrowserDialog.SelectedPath;//
            }
            else
                return null;


        }



        /// <summary>
        /// 选择文件夹对话框，并且能复制路径到文件夹对话框中
        /// </summary>
        /// <param name="defaultTitle">默认对话框提示</param>
        /// <param name="defaultPath">默认路径</param>
        /// <returns>如果操作成功，返回获取的文件夹全路径名称，否则，返回null</returns>
        public static string SelectDirectoryAndCanCopy(string defaultTitle = "选择文件所在文件夹", string defaultPath = "")
        {


            Ookii.Dialogs.VistaFolderBrowserDialog folderBrowser = new Ookii.Dialogs.VistaFolderBrowserDialog();
            folderBrowser.SelectedPath = defaultPath;
            folderBrowser.Description = defaultTitle;
            //folderBrowser.ShowNewFolderButton = true;
            if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return folderBrowser.SelectedPath;
            }

            else
            {
                return null;
            }
        }












        /// <summary>
        /// 获文件夹下的所有文件绝对路径列表
        /// </summary>
        /// <param name="strPath">文件夹路径</param>
        /// <param name="fileType">文件类型，默认为空，则返回所有的文件类型路径</param>
        /// <returns>文件路径数组,如果文件夹不存在，返回null</returns>
        public static string[] GetAllFiles(string strPath, string fileType = null)
        {
            //如果文件夹不存在

            if (!Directory.Exists(strPath))
            {
                return null;
            }

            //如果fileType为空，则默认获取所有的文件类型
            if (string.IsNullOrEmpty(fileType))
            {
                return Directory.GetFiles(strPath,"*",SearchOption.AllDirectories);
            }

            else
            {

                //获得目录下文件完整路径
                return Directory.GetFiles(strPath, fileType, SearchOption.AllDirectories);//参数1：目录；参数2：文件类型；参数3：是否包含子级目录文件
            }

        }




        /// <summary>
        /// 递归生成所有的子文件夹
        /// </summary>
        /// <param name="strPath">文件夹绝对路径</param>
        /// <param name="isReCreate">如果文件夹已经存在，是否允许给新的名字，在后面加_1、_2、_3...，默认false</param>
        /// <returns>创建的文件夹名字，如果没有给新的名字，则返回原来的名字</returns>
        public static string CreateDirectory(string strPath, bool isReCreate = false)
        {

            if (Directory.Exists(strPath)) //如果文件夹存在
            {
                if (isReCreate) //如果存在，允许给新的名字，在后面加_1、_2、_3...
                {
                    strPath = FindNewName(strPath);

                    //创建文件夹
                    Directory.CreateDirectory(strPath);
                }

            }
            else //如果文件夹不存在
            {

                //string dirpath = strPath.Substring(0, strPath.LastIndexOf('\\'));
                string[] pathes = strPath.Split('\\');
                if (pathes.Length > 1)
                {
                    string path = pathes[0];
                    for (int i = 1; i < pathes.Length; i++)
                    {
                        path += "\\" + pathes[i];
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                    }
                }
            }

            return strPath;
        }





        /// <summary>
        /// 如果文件夹不存在，返回原来的文件夹名，如果存在，在后面添加后缀_1、_2、_3...,并返回新的文件夹名
        /// </summary>
        /// <param name="absoluteOldDirectoryName">旧的绝对路径名</param>
        /// <returns>新的绝对路径名</returns>
        public static string FindNewName(string absoluteOldDirectoryName)
        {

            string absoluteNewDirectoryName = absoluteOldDirectoryName;

            int count = 1;

            //计算count
            while (Directory.Exists(absoluteNewDirectoryName)) //如果文件夹存在
            {
                absoluteNewDirectoryName = absoluteOldDirectoryName + "_" + count;
                count++;
            }

            return absoluteNewDirectoryName;
        }
















        /// <summary>
        /// 删除所有空的子文件夹，如果文件夹为空，也会被删除
        /// </summary>
        /// <param name="strPath">文件夹的绝对路径</param>
        public static void DeleteEmptyDirectory(string strPath)
        {
            if (!Directory.Exists(strPath))
            {
                return;
            }

            DeleteSubEmptyDirectories(strPath);

            //如果为空文件夹，删除

            if (Directory.GetFileSystemEntries(strPath).Length == 0)
            {
                Directory.Delete(strPath);
            }

        }










        /// <summary>
        /// 删除指定文件夹内的所有子空文件夹 该程序暂时没看懂
        /// </summary>
        /// <param name="parentFolder">文件夹绝对路径</param>
        public static void DeleteSubEmptyDirectories(string parentFolder)
        {

            if (!Directory.Exists(parentFolder))
            {
                return;
            }

            var dir = new DirectoryInfo(parentFolder);
            var subdirs = dir.GetDirectories("*.*", SearchOption.AllDirectories);

            foreach (var subdir in subdirs)
            {
                if (!Directory.Exists(subdir.FullName)) continue;

                var subFiles = subdir.GetFileSystemInfos("*.*", SearchOption.AllDirectories);

                var findFile = false;
                foreach (var sub in subFiles)
                {

                    //这个地方可能是判断sub是否为文件夹，如果是，返回false，继续下一轮循环判断，如果否，返回true，结束循环
                    findFile = (sub.Attributes & FileAttributes.Directory) == 0;

                    if (findFile) break;
                }

                if (!findFile) subdir.Delete(true);
            }
        }







        /// <summary>
        /// 获取文件夹下的所有子文件夹
        /// </summary>
        /// <param name="strPath">文件夹的绝对路径</param>
        /// <returns>子文件夹绝对路径数组，如果文件夹不存在，返回null，如果没有子文件夹，返回空数组</returns>
        public static string[] GetSubDirectories(string strPath)
        {

            if (!Directory.Exists(strPath)) //判断存在性
            {
                return null;
            }
            return Directory.GetDirectories(strPath, "*", SearchOption.AllDirectories);
        }







        /// <summary>
        /// 删除文件夹下的所有子文件和子文件夹，不删除本文件夹
        /// </summary>
        /// <param name="strPath">文件夹绝对路径</param>
        /// <returns>成功删除，返回true，如果有子文件打开，提示，并中断，返回false</returns>
        public static bool DeleteSubFolder(string strPath)
        {
            foreach (string d in Directory.GetFileSystemEntries(strPath))
            {
                if (File.Exists(d))  //为文件的情况
                {
                    FileInfo fi = new FileInfo(d);
                    if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                        fi.Attributes = FileAttributes.Normal;


                    ////需要先判断文件是否处于打开的状态
                    //if (FileTool.IsOpen(d))
                    //{
                    //    MessageBox.Show("文件: " + d + " 已经打开，需要手动关闭.");
                    //    return false;

                    //}

                    File.Delete(d);//直接删除其中的文件  
                }
                else  //为文件夹的情况
                {


                    if (Directory.GetFileSystemEntries(d).Length != 0)
                    {
                        DeleteSubFolder(d);
                    }


                    //对错误的处理
                    try
                    {

                        Directory.Delete(d);
                    }
                    catch
                    {
                        return false;
                    }



                }
            }


            return true;
        }



        /// <summary>
        /// 删除文件夹下的所有子文件和子文件夹，以及本文件夹
        /// </summary>
        /// <param name="strPath">文件夹绝对路径</param>
        /// <returns>成功删除，返回true，如果有子文件打开，提示，并中断，返回false，如果文件夹不存在，也返回false</returns>
        public static bool DeleteFolder(string strPath)
        {

            //先判断是否存在,如果不存在，返回false
            if (!Directory.Exists(strPath))
            {
                return false;
            }

            bool isSucceed = DeleteSubFolder(strPath);
            if (!isSucceed)  //如果删除失败
            {
                return false;
            }

            //删除成功

            //对错误的处理
            try
            {

                Directory.Delete(strPath, true);
            }

            catch
            {
                return false;
            }
            return true;

        }





        /// <summary>
        /// 复制文件到文件夹，如果有同名文件，自动覆盖
        /// </summary>
        /// <param name="fileName">文件的绝对路径</param>
        /// <param name="destPath">文件夹的绝对路径</param>
        public static void CopyFileToDirectory(string fileName, string destPath)
        {
            try
            {
                //如果目标文件不存在，先创建
                if (!Directory.Exists(destPath))
                {
                    Directory.CreateDirectory(destPath);
                }

                if (File.Exists(fileName))  //为文件的情况
                {
                    FileInfo fileInfo = new FileInfo(fileName);

                    string newFileName = destPath + "\\" + fileInfo.Name;
                    File.Copy(fileName, newFileName, true);      //复制文件，true表示可以覆盖同名文件
                    return;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }








        /// <summary>
        /// 将源文件夹下的所有文件和文件夹复制到目标文件夹下
        /// </summary>
        /// <param name="srcPath">源文件</param>
        /// <param name="destPath">目标文件</param>
        public static void CopyDirectory(string srcPath, string destPath)
        {
            try
            {


                //如果目标文件不存在，先创建
                if (!Directory.Exists(destPath))
                {
                    Directory.CreateDirectory(destPath);
                }


                DirectoryInfo dir = new DirectoryInfo(srcPath);
                FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();  //获取目录下（不包含子目录）的文件和子目录
                foreach (FileSystemInfo i in fileinfo)
                {
                    if (i is DirectoryInfo)     //判断是否文件夹
                    {
                        //if (!Directory.Exists(destPath + "\\" + i.Name))
                        //{
                        //    Directory.CreateDirectory(destPath + "\\" + i.Name);   //目标目录下不存在此文件夹即创建子文件夹
                        //}
                        CopyDirectory(i.FullName, destPath + "\\" + i.Name);    //递归调用复制子文件夹
                    }
                    else
                    {
                        File.Copy(i.FullName, destPath + "\\" + i.Name, true);      //不是文件夹即复  m,制文件，true表示可以覆盖同名文件
                    }
                }



            }
            catch (Exception e)
            {
                throw e;
            }
        }







        /// <summary>
        /// 将源文件或源文件夹复制到目标文件夹下
        /// </summary>
        /// <param name="srcPath">源文件</param>
        /// <param name="destPath">目标文件</param>
        public static void CopyIntoDirectory(string srcPath, string destPath)
        {
            try
            {

                //如果目标文件不存在，先创建
                if (!Directory.Exists(destPath))
                {
                    Directory.CreateDirectory(destPath);
                }



                if (File.Exists(srcPath))  //为文件的情况
                {
                    FileInfo fileInfo = new FileInfo(srcPath);

                    string newFileName = destPath + "\\" + fileInfo.Name;
                    File.Copy(srcPath, newFileName, true);      //复制文件，true表示可以覆盖同名文件
                }

                else   //为文件夹的情况
                {
                    //srcPath为绝对路径，先获取文件夹名称
                    DirectoryInfo directoryInfo = new DirectoryInfo(srcPath);
                    string directoryName = directoryInfo.Name;

                    //如果目标文件夹下的源文件夹名称不存在，先创建
                    string newDestPath = destPath + "\\" + directoryName;


                    if (!Directory.Exists(newDestPath))
                    {
                        Directory.CreateDirectory(newDestPath);
                    }

                    CopyDirectory(srcPath, newDestPath);

                }
            }

            catch (Exception e)
            {
                throw e;
            }

        }




        /// <summary>
        /// 对于完整文件路径：E:\svn\A\B\C\D\E\2020-06-29,资源管理器定位至 E:\svn\A\B\C\D\E 同时选中 2020-06-29文件夹
        /// </summary>
        static public class Win32Helper
        {

            /// <summary>
            /// 释放命令行管理程序分配的ITEMIDLIST结构
            /// Frees an ITEMIDLIST structure allocated by the Shell.
            /// </summary>
            /// <param name="pidlList"></param>
            [DllImport("shell32.dll", ExactSpelling = true)]
            public static extern void ILFree(IntPtr pidlList);
            /// <summary>
            /// 返回与指定文件路径关联的ITEMIDLIST结构。
            /// Returns the ITEMIDLIST structure associated with a specified file path.
            /// </summary>
            /// <param name="pszPath"></param>
            /// <returns></returns>
            [DllImport("shell32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
            public static extern IntPtr ILCreateFromPathW(string pszPath);
            /// <summary>
            /// 打开一个Windows资源管理器窗口，其中选择了特定文件夹中的指定项目。
            /// Opens a Windows Explorer window with specified items in a particular folder selected.
            /// </summary>
            /// <param name="pidlList"></param>
            /// <param name="cild"></param>
            /// <param name="children"></param>
            /// <param name="dwFlags"></param>
            /// <returns></returns>
            [DllImport("shell32.dll", ExactSpelling = true)]
            public static extern int SHOpenFolderAndSelectItems(IntPtr pidlList, uint cild, IntPtr children, uint dwFlags);

        }


        /// <summary>
        /// 打开目录并选中相应文件
        /// </summary>
        /// <param name="fileFullName"></param>
        static public void OpenFolderAndSelectFile(string fileFullName)
        {
            if (string.IsNullOrEmpty(fileFullName))
                throw new ArgumentNullException(nameof(fileFullName));
            fileFullName = Path.GetFullPath(fileFullName);
            var pidlList = Win32Helper.ILCreateFromPathW(fileFullName);
            if (pidlList == IntPtr.Zero) return;
            try
            {
                Marshal.ThrowExceptionForHR(Win32Helper.SHOpenFolderAndSelectItems(pidlList, 0, IntPtr.Zero, 0));
            }
            finally
            {
                Win32Helper.ILFree(pidlList);
            }

        }





        /// <summary>
        /// 打开文件或文件夹，判断是否存在，不判断是否已经打开
        /// </summary>
        /// <param name="pathName"></param>
        public static void OpenDirectory(string pathName)
        {
            if (!Directory.Exists(pathName) && !File.Exists(pathName))
            {
                return;
            }

            System.Diagnostics.Process.Start("Explorer", pathName);
        }














        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);



        /// <summary>
        /// 打开文件或文件夹，并前置 目前发现这个前置没有用处，如果之前已经打开，不会用之前的，而是新打开一个
        /// </summary>
        /// <param name="pathName"></param>
       public  static void OpenDirectoryAndSetForeground(string pathName)
        {

            // 获取文件夹的所有进程
            Process[] processes = Process.GetProcessesByName("explorer");

            foreach (Process process in processes)
            {
                try
                {
                    // 获取进程打开的文件夹路径
                    string processPath = process.MainModule.FileName;





                    // 这里假设打开的是文件夹而不是文件
                    // 实际情况可能需要更复杂的逻辑来判断
                    string commandLine = process.StartInfo.Arguments;
                    if (commandLine.StartsWith("/n,") && commandLine.Length > 3)
                    {
                        string fi= commandLine.Substring(3);
                    }







                    // 检查文件夹路径是否匹配
                    if (processPath.Equals(pathName, StringComparison.OrdinalIgnoreCase))
                    {
                        // 将文件夹窗口前置
                        IntPtr hWnd = process.MainWindowHandle;
                        SetForegroundWindow(hWnd);
                        return ;
                    }
                }
                catch (Exception ex)
                {
                    // 处理异常，可能是由于没有足够权限等原因
                    //Console.WriteLine($"Error: {ex.Message}");
                }
            }


            //如果以上没有返回，说明之前没有打开

            OpenDirectory( pathName);

        }
 









}

}
