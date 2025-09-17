using System;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Sepd.RevitTools.Helper
{
    /// <summary>
    /// 操作日志记录类
    /// </summary>
    public class LogHelper
    {


        /// <summary>
        /// 操作日志记录
        /// </summary>
        /// <param name="opType">需要记录的信息</param>
        /// <param name="platformName">平台的名称，莫瑞芳自己修改的</param>
        /// <param name="time">时间</param>
        /// <param name="excludeUserId">不需要记录的用户的Id，比如开发人员自己的Id</param>
        public static void LogMonitor(string opType, string platformName, DateTime time,string excludeUserId="430725")
        {
            string opDate = null;
            string currentUser = null;
            var userVerifyFile = @"C:\ProgramData\Autodesk\Revit\Addins\UserCode.txt";
            try
            {
                if (File.Exists(userVerifyFile))
                {
                    var sr = new StreamReader(userVerifyFile, Encoding.Default);
                    var line = sr.ReadLine();
                    var VerifyInfo = line.Split('/');
                    currentUser = VerifyInfo[0];
                }
                else
                {
                    currentUser = "无工号";
                }

                /*
                //本机当前使用的IPv4地址
                string opIP = null;
                IPAddress ipAddr = Dns.GetHostEntry(Dns.GetHostName()).AddressList[1];//获得当前IP地址
                opIP = ipAddr.ToString();
                */
                //当前操作时间
                //opDate =  DateTime.Now.ToString();
                //DateTime time = Convert.ToDateTime(opDate);
                //开发记录不写入数据库

                //10:57 2021/10/29  莫瑞芳 把自己的记录去掉
                //if (currentUser != "430661")
                //{

                if (currentUser != excludeUserId)
                {


                    string state = null;
                    bool[] IsConnected = null;
                    string[] connectedInfo = null;
                    var checkurls = "10.193.40.11";
                    NetConnection.CheckServeStatus(new string[1] { checkurls }, out state, out IsConnected, out connectedInfo);
                    if (state == "网络稳定")
                    {
                        //写入数据库
                        //MessageBox.Show("HH");
                        #region
                        //string department = GetDepartment(currentUser);


                        //10:57 2021/10/29  莫瑞芳 改为其他部门
                        //var department = "测试部门";
                        var department = "其他部门";



                        state = null;
                        IsConnected = null;
                        connectedInfo = null;
                        checkurls = "10.193.217.38";
                        NetConnection.CheckServeStatus(new string[1] { checkurls }, out state, out IsConnected, out connectedInfo);
                        if (state == "网络稳定")
                        {
                            var sqlConnection = new SqlConnection("Data Source=10.193.217.38;Database=sepd;Uid=Sa;Pwd=SanWei2209");
                            sqlConnection.Open();

                            //MessageBox.Show(UserCode);
                            var sql = $"INSERT INTO Plugin_pluginusageinfo VALUES('{currentUser}','{department}','{platformName}'," +
                                         $"'{opType}','{time}')";
                            var sqlCommand = new SqlCommand(sql, sqlConnection);
                            var i = sqlCommand.ExecuteNonQuery();

                            sqlConnection.Close();
                        }
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }


        /// <summary>
        /// 获取部门
        /// </summary>
        /// <param name="UserCode"></param>
        /// <returns></returns>
        public static string GetDepartment(string UserCode)
        {
            var DP = "";

            var _SqlConnection = new SqlConnection("Server = 10.193.40.11; DataBase=LZMISPM;uid=sa;pwd=sepdkxb_2212");

            _SqlConnection.Open();

            //MessageBox.Show(UserCode);
            var sql = @"SELECT UG_UserGrpName AS GroupName From [dbo].[LZ_MISUser] left join  [dbo].[LZ_MISDepartmentUser] ON [LZ_MISUser].SU_UserID=[LZ_MISDepartmentUser].SU_UserID left join [dbo].[LZ_MISDepartment] ON [LZ_MISDepartmentUser].UG_UserGrpID=[LZ_MISDepartment].UG_UserGrpID WHERE SU_UserCode='" + UserCode + "' ORDER BY UG_UserGrpUpdateTime DESC";
            var _SqlCommand = new SqlCommand(sql, _SqlConnection);
            var _SqlDataReader = _SqlCommand.ExecuteReader();
            var tempDP = "";
            while (_SqlDataReader.Read())
            {
                tempDP = _SqlDataReader["GroupName"].ToString();
                break;
            }
            _SqlConnection.Close();
            if (tempDP.Contains("配网"))
            {
                DP = "配网部";
            }
            else if (tempDP.Contains("电网") || tempDP.Contains("变电"))
            {
                DP = "电网部";
            }
            else if (tempDP.Contains("新能源"))
            {
                DP = "新能源部";
            }
            else
            {
                DP = "其他部门";
            }
            return DP;
        }
    }
}
