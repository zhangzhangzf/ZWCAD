
using System;
using System.IO;
using System.Collections;
using System.Text;

namespace Mrf.CSharp.BaseTools
{
    /// <summary>
    /// 读取和解析INI文件工具
    /// </summary>
    public class InitFileTool
    {

        #region Private Variables


        private Hashtable m_keyPairs = new Hashtable();
        private string m_iniFilePath;

        private Encoding m_encoding = Encoding.Default;


        private struct SectionPair
        {
            public string Section;
            public string Key;
        }

        #endregion



        #region Default Constructor



        /// <summary>
        /// 构造函数 打开.ini文件并解析
        /// </summary>
        /// <param name="iniPath">.ini文件的全路径名称</param>
        public InitFileTool(string iniPath)
        {
            TextReader iniFile = null;
            string strLine = null;
            string currentRoot = null;
            string[] keyPair = null;
            m_iniFilePath = iniPath;
            if (File.Exists(iniPath))
            {

                try
                {

                    //获取文件的编码格式
                    m_encoding = FileTool.GetTextFileEncodingType(iniPath);

                    iniFile = new StreamReader(iniPath, m_encoding);

                    strLine = iniFile.ReadLine();
                    while (strLine != null)
                    {

                        //19:55 2022/5/26 都设置为大写 将会使原始的内容都变成大写
                        //strLine = strLine.Trim().ToUpper();
                        strLine = strLine.Trim();



                        if (strLine != "")
                        {
                            if (strLine.StartsWith("[") && strLine.EndsWith("]"))
                            {
                                currentRoot = strLine.Substring(1, strLine.Length - 2);
                            }
                            else
                            {
                                keyPair = strLine.Split(new char[] { '=' }, 2);
                                SectionPair sectionPair;
                                string value = null;
                                if (currentRoot == null)
                                    currentRoot = "ROOT";
                                sectionPair.Section = currentRoot;
                                sectionPair.Key = keyPair[0];
                                if (keyPair.Length > 1)
                                    value = keyPair[1];
                                m_keyPairs.Add(sectionPair, value);
                            }
                        }
                        strLine = iniFile.ReadLine();
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (iniFile != null)
                        iniFile.Close();
                }






            }
            else
                throw new FileNotFoundException("Unable to locate " + iniPath);
        }


        #endregion




        #region CommandMethods


        /// <summary>
        /// 返回指定段、键所对应的值
        /// </summary>
        /// <param name="sectionName">Section名字，区分大小写</param>
        /// <param name="settingName">Key 区分大小写</param>
        /// <returns>所对应的值，如果不存在，返回null</returns>
        public string GetSetting(string sectionName, string settingName)
        {
            SectionPair sectionPair;


            //区分大小写
            //sectionPair.Section = sectionName.ToUpper();
            //sectionPair.Key = settingName.ToUpper();

            sectionPair.Section = sectionName;
            sectionPair.Key = settingName;


            return (string)m_keyPairs[sectionPair];
        }



        /// <summary>
        /// 获取给定section的所有行的键组成的数组
        /// </summary>
        /// <param name="sectionName">Section名称</param>
        public string[] EnumSection(string sectionName)
        {
            ArrayList tmpArray = new ArrayList();
            foreach (SectionPair pair in m_keyPairs.Keys)
            {

                //if (pair.Section == sectionName.ToUpper())
                //    tmpArray.Add(pair.Key);

                if (pair.Section == sectionName)
                    tmpArray.Add(pair.Key);

            }
            return (string[])tmpArray.ToArray(typeof(string));
        }


        /// <summary>
        /// 添加或替换给定Section和Key所对应值的内容，暂不保存到文件
        /// </summary>
        /// <param name="sectionName">Section名字，区分大小写</param>
        /// <param name="settingName">Key名字，区分大小写</param>
        /// <param name="settingValue">key的值.</param>
        public void AddSetting(string sectionName, string settingName, string settingValue)
        {
            SectionPair sectionPair;


            //sectionPair.Section = sectionName.ToUpper();
            //sectionPair.Key = settingName.ToUpper();

            sectionPair.Section = sectionName;
            sectionPair.Key = settingName;



            if (m_keyPairs.ContainsKey(sectionPair))
                m_keyPairs.Remove(sectionPair);
            m_keyPairs.Add(sectionPair, settingValue);
        }
        /// <summary>
        /// 添加或替换给定Section和Key的值为null，暂不保存到文件
        /// </summary>
        /// <param name="sectionName">Section名字，不分大小写</param>
        /// <param name="settingName">Key名字，不分大小写</param>
        public void AddSetting(string sectionName, string settingName)
        {
            AddSetting(sectionName, settingName, null);
        }


        /// <summary>
        /// 删除一个给定Section和key的设置.
        /// </summary>
        /// <param name="sectionName">Section名字，区分大小写</param>
        /// <param name="settingName">Key名字，区分大小写</param>
        public void DeleteSetting(string sectionName, string settingName)
        {
            SectionPair sectionPair;


            //sectionPair.Section = sectionName.ToUpper();
            //sectionPair.Key = settingName.ToUpper();

            sectionPair.Section = sectionName;
            sectionPair.Key = settingName;



            if (m_keyPairs.ContainsKey(sectionPair))
                m_keyPairs.Remove(sectionPair);
        }


        /// <summary>
        /// 将设置保存到新的文件
        /// </summary>
        /// <param name="newFilePath">New file path.</param>
        public void SaveSettings(string newFilePath)
        {
            ArrayList sections = new ArrayList();
            string tmpValue = "";
            string strToSave = "";
            foreach (SectionPair sectionPair in m_keyPairs.Keys)
            {
                if (!sections.Contains(sectionPair.Section))
                    sections.Add(sectionPair.Section);
            }
            foreach (string section in sections)
            {
                strToSave += ("[" + section + "]\r\n");
                foreach (SectionPair sectionPair in m_keyPairs.Keys)
                {
                    if (sectionPair.Section == section)
                    {
                        tmpValue = (string)m_keyPairs[sectionPair];
                        if (tmpValue != null)
                            tmpValue = "=" + tmpValue;
                        strToSave += (sectionPair.Key + tmpValue + "\r\n");
                    }
                }
                strToSave += "\r\n";
            }
            try
            {
                TextWriter tw = new StreamWriter(newFilePath, false, m_encoding);
                tw.Write(strToSave);
                tw.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        /// <summary>
        /// 保存设置到原文件.
        /// </summary>
        /// <param name="isBackUp">是否将源文件备份</param>
        /// <param name="backUpFile">备份文件的全路径名称，如果为空，使用默认的</param>
        public void SaveSettings(bool isBackUp = true, string backUpFile = null)
        {
            if (isBackUp) //要备份
            {
                if (string.IsNullOrEmpty(backUpFile))
                {
                    string date = TimeTool.GetCurrentTimeByFormat();

                    string fileName = Path.GetFileNameWithoutExtension(m_iniFilePath) + date;

                    backUpFile = Path.GetDirectoryName(m_iniFilePath) + "\\" + fileName + Path.GetExtension(m_iniFilePath);

                }

                File.Copy(m_iniFilePath, backUpFile, true);
            }


            SaveSettings(m_iniFilePath);
        }


        #endregion




        #region Helper Methods


        #endregion


        #region Properties


        #endregion

    }
























}
