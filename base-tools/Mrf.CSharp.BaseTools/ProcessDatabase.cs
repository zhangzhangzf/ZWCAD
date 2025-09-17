using System.Runtime.InteropServices;

namespace Sepd.RevitTools.Helper
{
    /// <summary>
    /// NetConnection
    /// </summary>
    public class NetConnection
    {
        /// <summary>
        /// 检查服务器状态
        /// </summary>
        /// <param name="urls"></param>
        /// <param name="state"></param>
        /// <param name="IsConnected"></param>
        /// <param name="connectedInfo"></param>
        public static void CheckServeStatus(string[] urls, out string state, out bool[] IsConnected, out string[] connectedInfo)
        {
            var errCount = 0;
            IsConnected = new bool[urls.Length];
            state = "网络稳定";
            for (var i = 0; i < urls.Length; i++)
            {
                IsConnected[i] = true;
            }
            connectedInfo = new string[urls.Length];
            for (var i = 0; i < urls.Length; i++)
            {
                connectedInfo[i] = "网络稳定";
            }
            if (!LocalConnectionStatus())
            {
                //网络异常-无连接
                state = "网络异常-无连接";
                for (var i = 0; i < urls.Length; i++)
                {
                    IsConnected[i] = false;
                }
                for (var i = 0; i < urls.Length; i++)
                {
                    connectedInfo[i] = "网络异常-无连接";
                }
            }
            else
            {

                MyPing(urls, out errCount, out IsConnected, out connectedInfo);
                if ((double)errCount / urls.Length == 0.0)
                {
                    //网络稳定
                    state = "网络稳定";
                }
                else if ((double)errCount / urls.Length >= 0.3)
                {
                    //网络异常-连接多次无响应
                    state = "网络异常-连接多次无响应";
                }
                else
                {
                    //网络不稳定
                    state = "网络不稳定";
                }
            }
        }

        private const int INTERNET_CONNECTION_MODEM = 1;
        private const int INTERNET_CONNECTION_LAN = 2;
        [DllImport("wininet.dll")]
        extern static bool InternetGetConnectedState(out int connectionDescription, int reservedValue);

        private bool IsConnected()
        {
            var I = 0;
            var state = InternetGetConnectedState(out I, 0);
            return state;
        }

        /// <summary>
        /// 判断本地的连接状态
        /// </summary>
        /// <returns></returns>
        private static bool LocalConnectionStatus()
        {
            var dwFlag = new int();
            if (!InternetGetConnectedState(out dwFlag, 0))
            {
                //未联网
                return false;
            }
            else
            {
                if ((dwFlag & INTERNET_CONNECTION_MODEM) != 0)
                {
                    //采用调制解调器上网
                    return true;
                }
                else if ((dwFlag & INTERNET_CONNECTION_LAN) != 0)
                {
                    //采用网卡上网
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Ping命令检测网络是否顺畅
        /// </summary>
        /// <param name="urls">URL数据</param>
        /// <param name="errorCount">ping时连接失败个数</param>
        /// <param name="IsConnected"></param>
        /// <param name="connectedInfo"></param>
        /// <returns></returns>
        private static void MyPing(string[] urls, out int errorCount, out bool[] IsConnected, out string[] connectedInfo)
        {
            var ping = new System.Net.NetworkInformation.Ping();
            errorCount = 0;
            IsConnected = new bool[urls.Length];
            for (var i = 0; i < urls.Length; i++)
            {
                IsConnected[i] = true;
            }
            connectedInfo = new string[urls.Length];
            for (var i = 0; i < urls.Length; i++)
            {
                connectedInfo[i] = "连接正常";
            }
            try
            {
                System.Net.NetworkInformation.PingReply pr;
                for (var i = 0; i < urls.Length; i++)
                {
                    try
                    {
                        pr = ping.Send(urls[i]);
                        if (pr.Status != System.Net.NetworkInformation.IPStatus.Success)
                        {
                            IsConnected[i] = false;
                            connectedInfo[i] = "网络异常";
                            errorCount++;
                        }
                    }
                    catch
                    {
                        IsConnected[i] = false;
                        connectedInfo[i] = "网络异常";
                        errorCount++;
                    }
                }
            }
            catch
            {
                for (var i = 0; i < urls.Length; i++)
                {
                    IsConnected[i] = false;
                }
                for (var i = 0; i < urls.Length; i++)
                {
                    connectedInfo[i] = "网络异常-无连接！";
                }
                errorCount = urls.Length;
            }
        }


        /// <summary>
        /// Get local IP
        /// </summary>
        /// <returns></returns>
        public static string GetlocalIP()
        {
            string localIP = null;
            foreach (var _ipAddress in System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList)
            {
                if (_ipAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    localIP = _ipAddress.ToString();
                    break;
                }
            }
            return localIP;
        }
    }
}
