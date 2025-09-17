using System.Text;

namespace Mrf.CSharp.BaseTools
{

    /// <summary>
    /// 当执行例子时存储提醒/错误信息
    /// </summary>
    public static class MessageManager
    {
        static StringBuilder m_messageBuff = new StringBuilder();
        /// <summary>
        /// 存储提醒/错误信息
        /// </summary>
        public static StringBuilder MessageBuff
        {
            get
            {
                return m_messageBuff;   
            }
            set
            {
                m_messageBuff = value;
            }
        }
    }


}
