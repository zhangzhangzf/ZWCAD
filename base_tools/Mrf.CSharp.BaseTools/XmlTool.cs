using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Mrf.CSharp.BaseTools
{
    /// <summary>
    /// xml文件操作工具
    /// </summary>
    public class XmlTool
    {




        /// <summary>
        /// 创建Xml文件头部信息
        /// </summary>
        /// <returns>Xml文件对象</returns>
        public static XmlDocument CreateXmlDeclaration()
        {
            //创建一个XML文档对象
            XmlDocument xmlDocument = new XmlDocument();

            //声明XML头部信息
            XmlDeclaration dec = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);

            //添加进document对象子节点
            xmlDocument.AppendChild(dec);

            return xmlDocument;

        }




        /// <summary>
        /// 创建Xml文件根节点（第一级）
        /// </summary>
        /// <returns>根节点</returns>
        public static XmlNode CreateXmlRoot(XmlDocument xmlDocument, string rootName)
        {

            XmlNode xmlNode = CreateChildNode(xmlDocument, xmlDocument, rootName);

            //添加进document对象子节点
            return xmlDocument.AppendChild(xmlNode);

        }



        /// <summary>
        /// 创建子节点并添加子节点的值
        /// </summary>
        /// <param name="xmlDocument">xml文档对象</param>
        /// <param name="node">父节点</param>
        /// <param name="childName">子节点名称</param>
        /// <param name="childValue">子节点的值，如果为空，将不设置，默认为空</param>
        /// <returns>节点对象</returns>
        public static XmlNode CreateChildNode(XmlDocument xmlDocument, XmlNode node, string childName, string childValue = null)
        {

            //创建子节点 
            XmlElement element = xmlDocument.CreateElement(childName);
            if (!string.IsNullOrEmpty(childValue))
            {
                element.InnerText = childValue;
            }

            return node.AppendChild(element);
        }


        /// <summary>
        /// 创建子节点并添加子节点的属性
        /// </summary>
        /// <param name="xmlDocument">xml文档对象</param>
        /// <param name="node">父节点</param>
        /// <param name="childName">子节点名称</param>
        /// <param name="attributeName">子节点属性名称</param>
        /// <param name="childValue">子节点值</param>
        /// <param name="attributeValue">子节点属性值</param>
        /// <returns>节点对象</returns>
        public static XmlNode CreateAttribute(XmlDocument xmlDocument, XmlNode node, string childName, string attributeName, string childValue = null, string attributeValue = null)
        {
            //创建子节点 

            XmlNode childNode = CreateChildNode(xmlDocument, node, childName, childValue);

            XmlElement xmlElement = childNode as XmlElement;

            if (!string.IsNullOrEmpty(attributeValue))
            {
                xmlElement.SetAttribute(attributeName, attributeValue);
            }

            return childNode;

        }





        /// <summary>
        /// 创建子节点并添加子节点的属性列表
        /// </summary>
        /// <param name="xmlDocument">xml文档对象</param>
        /// <param name="node">父节点</param>
        /// <param name="childName">子节点名称</param>
        /// <param name="attributeNameAndValueMap">属性名称和值映射</param>
        /// <param name="childValue">子节点名称值，如果为空，将不设置，默认为空</param>
        /// <returns>节点对象</returns>
        public static XmlNode CreateAttributes(XmlDocument xmlDocument, XmlNode node, string childName, Dictionary<string, string> attributeNameAndValueMap, string childValue = null)
        {
            //创建子节点 

            XmlNode childNode = CreateChildNode(xmlDocument, node, childName, childValue);

            XmlElement xmlElement = childNode as XmlElement;

            foreach (KeyValuePair<string, string> kvp in attributeNameAndValueMap)
            {
                string key = kvp.Key;
                string value = kvp.Value;
                if (!string.IsNullOrEmpty(value))
                {
                    xmlElement.SetAttribute(key, value);
                }
            }

            return childNode;

        }









        /// <summary>
        /// 创建节点的注释
        /// </summary>
        /// <param name="xmlDocument">xml文档对象</param>
        /// <param name="node">节点</param>
        /// <param name="commnentText">注释内容</param>
        /// <returns>注释内容对象</returns>
        public static XmlText CreateComment(XmlDocument xmlDocument, XmlDocument node, string commnentText)
        {

            XmlText xmlText = xmlDocument.CreateTextNode(commnentText);
            node.AppendChild(xmlText);
            return xmlText;
        }





        /// <summary>
        /// 读取指定节点的所有指定属性列表的值
        /// </summary>
        /// <param name="xmlFileName">xml文件绝对路径，判断是否存在</param>
        /// <param name="nodeName">节点名称，如"student/name"</param>
        /// <param name="attributeNameLst">属性名称列表</param>
        /// <returns>属性值列表的列表，如果读取失败，返回空的列表</returns>
        public static List<List<string>> GetNodeAtrributeValueLst(string xmlFileName, string nodeName, List<string> attributeNameLst)
        {
            //返回值
            List<List<string>> allAttributeValueLst = new List<List<string>>();

            if (!File.Exists(xmlFileName)) //文件不存在，直接返回
            {
                return allAttributeValueLst;
            }



            //XmlDocument读取xml文件
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlFileName);

            //获取xml根节点
            XmlNode xmlRoot = xmlDoc.DocumentElement;


            //读取所有相关的节点
            foreach (XmlNode node in xmlRoot.SelectNodes(nodeName))
            {
                List<string> attributeValueLst = new List<string>();
                foreach (var item in attributeNameLst)
                {
                    string value = node.Attributes[item].InnerText;

                    if (string.IsNullOrEmpty(value))
                    {
                        value = "";
                    }
                    attributeValueLst.Add(value);
                }
                allAttributeValueLst.Add(attributeValueLst);
            }

            return allAttributeValueLst;

        }











        public void readXml(string xmlFileName)
        {
            //将XML文件加载进来
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlFileName);
            //获取根节点
            XmlElement root = doc.DocumentElement;
            //获取子节点
            XmlNodeList pnodes = root.GetElementsByTagName("attribute");
            //使用foreach循环读出集合
            foreach (XmlNode node in pnodes)
            {
                string name = ((XmlElement)node).GetAttribute("name");
                string x = ((XmlElement)node).GetElementsByTagName("X")[0].InnerText;
                string y = ((XmlElement)node).GetElementsByTagName("Y")[0].InnerText;
                string z = ((XmlElement)node).GetElementsByTagName("Z")[0].InnerText;
                Console.WriteLine("位置:{0},x:{1},y:{2},z:{3}", name, x, y, z);
            }

        }
    }
}
