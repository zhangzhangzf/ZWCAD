using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Mrf.CSharp.BaseTools.Extension
{
    /// <summary>
    /// 
    /// </summary>
    public static class ExeExtension
    {

        /// <summary>
        /// 使用编译好的工具，将TIFF格式的图片转为png格式的图片
        /// 返回PNG图片名称
        /// 返回tiff坐标信息
        /// </summary>
        /// <param name="exePath">exe文件执行地址</param>
        /// <param name="arguments">传递到exe中的参数列表，可以为空</param>
        /// <returns>程序的返回值，如果exe不存在 或者不为exe文件，返回null，如果运行失败，则返回的数据跟具体的exe返回结果相关</returns>
        public static string[] CallExeFile(string exePath,params string[] arguments)
        {

            //返回值
            string[] tifInfo=null;

            if(!File.Exists(exePath)) //文件不存在
            {
                MessageBox.Show("找不到应用程序:\n"+exePath, "Tips");
                return tifInfo;
            }

            if (!exePath.EndsWith("exe", StringComparison.OrdinalIgnoreCase)) //不为exe文件
            {
                MessageBox.Show("文件:\n"+exePath+"\n不是应用程序", "Tips");
                return tifInfo;
            }



            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = exePath,                   //可执行程序路径
                //Arguments = tiffPath + " " + pngDir,  //参数以空格分隔，如果某个参数为空，可以传入""

                //对于cad来说，这个必须设置为false，不然会报错
                UseShellExecute = false,              //是否使用操作系统shell启动
                CreateNoWindow = false,               //显示程序窗口
                RedirectStandardOutput = true,   //由调用程序获取输出信息
                RedirectStandardInput = true,    //接受来自调用程序的输入信息
                RedirectStandardError = true,    //重定向标准错误输出
            };


            if(arguments != null && arguments.Length>0)
            {
                startInfo.Arguments=string.Join(" ", arguments);
            }



            using (Process process = new Process())
            {
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();

                string output = process.StandardOutput.ReadToEnd();
                tifInfo = output.Split('\n');
            }

            return tifInfo;
        }


    }
}
