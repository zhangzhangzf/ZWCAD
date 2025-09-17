
using System;
using System.Diagnostics;
using System.Windows;

namespace Mrf.CSharp.BaseTools
{
    /// <summary>
    /// 时间测试工具
    /// </summary>
    public class TimingTool
    {



        #region Private Variables

        TimeSpan duration;

        #endregion



        #region Default Constructor


        /// <summary>
        /// 构造函数
        /// </summary>
        public TimingTool()
        {
            duration = new TimeSpan(0);
        }

        #endregion




        #region CommandMethods






        #endregion



        #region Helper Methods


        /// <summary>
        /// 测试开始
        /// </summary>
        public void StartTime()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        /// <summary>
        /// 测试结束
        /// </summary>
        public void StopTime()
        {
            duration = Process.GetCurrentProcess().TotalProcessorTime;
        }


        /// <summary>
        /// 显示测试时间，自动结束测试
        /// </summary>
        public void Show()
        {
            StopTime();
            MessageBox.Show(duration.ToString(),"Tips");
        }


        #endregion


        #region Properties


        #endregion




    }
}
