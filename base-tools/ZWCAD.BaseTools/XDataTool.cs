using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using System.Collections.Generic;

namespace ZWCAD.BaseTools
{
    /// <summary>
    /// 扩展数据工具
    /// </summary>
    public class XDataTool
    {
        Document m_document;

        Database m_database;


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="document">文档对象</param>
        public XDataTool(Document document)
        {
            m_document = document;
            m_database = m_document.Database;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="database">图形数据库</param>
        public XDataTool(Database database)
        {
            m_database = database;
        }



        /// <summary>
        /// 注册应用程序名称
        /// </summary>
        /// <param name="regAppName">应用程序名称</param>
        public void AddRegApp(string regAppName)
        {

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {

                RegAppTable regAppTable = transaction.GetObject(m_database.RegAppTableId, OpenMode.ForWrite) as RegAppTable;
                if (!regAppTable.Has(regAppName)) //如果不存在名为regAppName的记录，则创建
                {
                    RegAppTableRecord regRecord = new RegAppTableRecord();
                    regRecord.Name = regAppName;//设置扩展数据的名字

                    regAppTable.Add(regRecord);
                    transaction.AddNewlyCreatedDBObject(regRecord, true);

                    transaction.Commit();

                    //为了安全，将应用程序注册表切换为读的状态
                    //regAppTable.DowngradeOpen();
                }
            }

        }



        /// <summary>
        /// 某个应用程序的扩展数据列表
        /// </summary>
        /// <param name="dbObject">对象</param>
        /// <param name="applicationName">应用程序名称</param>
        /// <returns>扩展数据列表</returns>
        public List<string[]> GetXData(DBObject dbObject, string applicationName)
        {
            List<string[]> xDataLst = new List<string[]>();

            ResultBuffer resultBuffer = dbObject.GetXDataForApplication(applicationName);
            if (resultBuffer != null)
            {
                TypedValue[] typedValues = resultBuffer.AsArray();
                foreach (TypedValue typedValue in typedValues)
                {

                    string typeCode = typedValue.TypeCode.ToString();
                    string value = typedValue.Value.ToString();
                    xDataLst.Add(new string[] { typeCode, value });
                    xDataLst.Add(new string[] { typeCode, value });
                }
            }

            return xDataLst;

        }









        /// <summary>
        /// 获取外部数据数组
        /// </summary>
        /// <param name="objectid">对象的objectid</param>
        /// <returns>TypedValue[],如果没有外部数据，，则返回null</returns>
        public TypedValue[] GetXDataTypedValueArr(ObjectId objectid)
        {


            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                Entity ent = (Entity)transaction.GetObject(objectid, OpenMode.ForRead);
                ResultBuffer resultBuffer = ent.XData;

                if (resultBuffer != null)
                {
                    TypedValue[] typedValues = resultBuffer.AsArray();
                    return typedValues;
                }
                else
                {
                    return null;
                }
            }


        }












        /// <summary>
        /// 获取外部数据的列表
        /// </summary>
        /// <param name="objectId">对象的objectid</param>
        /// <returns>数组的列表</returns>
        public List<string[]> GetXData(ObjectId objectId)
        {
            List<string[]> xDataLst = new List<string[]>();

            TypedValue[] typedValues = GetXDataTypedValueArr(objectId);
            if (typedValues != null)
            {

                foreach (TypedValue typedValue in typedValues)
                {

                    string typeCode = typedValue.TypeCode.ToString();
                    string value = typedValue.Value.ToString();
                    xDataLst.Add(new string[] { typeCode, value });
                }

            }


            return xDataLst;

        }


        /// <summary>
        /// 查找外部文件的注册名称
        /// </summary>
        /// <param name="objectId">对象的objectid</param>
        /// <returns>注册名称</returns>
        public string GetXDataRegisterName(ObjectId objectId)
        {
            List<string[]> xDataLst = GetXData(objectId);

            //不存在时，返回null
            if (xDataLst.Count == 0)
            {
                return null;

            }


            foreach (string[] xData in xDataLst)
            {
                if (xData[0] == "1001")
                {
                    return xData[1];
                }
            }

            //不存在时，返回null
            return null;

        }


        /// <summary>
        /// 获取外部数据列表
        /// </summary>
        /// <param name="objectId">对象的objectid</param>
        /// <returns>外部数据数组的列表，如果没有，返回空的列表</returns>
        public List<string[]> GetXDataInformationLst(ObjectId objectId)
        {
            List<string[]> xDataLst = GetXData(objectId);

            List<string[]> xDataInformationLst = new List<string[]>();

            int count = xDataLst.Count;

            int i = 0;

            while (i < count)
            {
                if (xDataLst[i][1] == "{")
                {
                    xDataInformationLst.Add(new string[] { xDataLst[i + 1][1], xDataLst[i + 2][1] });
                    i = i + 3;
                }
                i++;
            }

            return xDataInformationLst;

        }



        /// <summary>
        /// 通过索引获取外边数据对应的值
        /// </summary>
        /// <param name="objectId">对象的objectid</param>
        /// <param name="codeString"></param>
        /// <returns></returns>
        public string GetDataValueByCodeString(ObjectId objectId, string codeString)
        {
            List<string[]> xDataInformationLst = GetXDataInformationLst(objectId);
            return GetDataValueByCodeString(xDataInformationLst, codeString);
        }



        /// <summary>
        /// 通过索引获取外边数据对应的值
        /// </summary>
        /// <param name="xDataInformationLst">外部数据列表</param>
        /// <param name="codeString">索引字符串</param>
        /// <returns>值string</returns>
        public string GetDataValueByCodeString(List<string[]> xDataInformationLst, string codeString)
        {
            foreach (string[] xDataInformation in xDataInformationLst)
            {
                if (xDataInformation[0] == codeString)
                {
                    return xDataInformation[1];
                }
            }

            return null;
        }








        /// <summary>
        /// 删除某个应用程序的扩展数据
        /// </summary>
        /// <param name="objectId">对象的ObjectId</param>
        /// <param name="applicationName">应用程序名称</param>
        public void DeleteXData(ObjectId objectId, string applicationName)
        {

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                TypedValue[] values = new TypedValue[] { new TypedValue((int)DxfCode.ExtendedDataRegAppName, applicationName) };

                ResultBuffer resultBuffer = new ResultBuffer(values);


                DBObject dbObject = transaction.GetObject(objectId, OpenMode.ForWrite);


                //删除应用程序abc的扩展数据，只需要进行一个这样的赋值，新值里只添加一个应用程序名，
                //不添加其他值，这样XData中，原有的abc的扩展数据就会被删除
                dbObject.XData = resultBuffer;
                transaction.Commit();

            }

        }




        /// <summary>
        /// 添加某个应用程序的扩展数据
        /// </summary>
        /// <param name="objectId">对象的ObjectId</param>
        /// <param name="applicationName">应用程序名称</param>
        /// <param name="dataLst">数据列表</param>
        public void AddXData(ObjectId objectId, string applicationName, List<string[]> dataLst)
        {

            TypedValue[] values = new TypedValue[dataLst.Count];

            int i = 0;

            foreach (string[] each in dataLst)
            {
                values[i] = new TypedValue(int.Parse(each[0]), each[1]);
                i++;
            }

            AddXData(objectId, applicationName, values);

        }




        /// <summary>
        /// 添加某个应用程序的扩展数据
        /// </summary>
        /// <param name="objectId">对象的ObjectId</param>
        /// <param name="applicationName">应用程序名称</param>
        /// <param name="values">数据列表</param>
        public void AddXData(ObjectId objectId, string applicationName, TypedValue[] values)
        {
            //先注册
            AddRegApp(applicationName);


            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {

                ResultBuffer resultBuffer = new ResultBuffer();

                //添加数据
                resultBuffer.Add(new TypedValue((int)DxfCode.ExtendedDataRegAppName, applicationName));

                foreach (TypedValue value in values)
                {
                    resultBuffer.Add(value);
                }

                DBObject dbObject = transaction.GetObject(objectId, OpenMode.ForWrite);

                dbObject.XData = resultBuffer;

                transaction.Commit();
            }

        }




        /// <summary>
        /// 将一个对象的外部数据复制到另一个对象中
        /// </summary>
        /// <param name="sourceObjectId">源对象的ObjectId</param>
        /// <param name="targetObjectId">目标对象的ObjectId</param>
        public void CopyXData(ObjectId sourceObjectId, ObjectId targetObjectId)
        {

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                //源对象的外部数据
                Entity sourceObject = (Entity)transaction.GetObject(sourceObjectId, OpenMode.ForRead);
                ResultBuffer sourceObjectXData = sourceObject.XData;

                //更改目标对象的外部数据
                Entity targetObject = (Entity)transaction.GetObject(targetObjectId, OpenMode.ForWrite);
                targetObject.XData = sourceObjectXData;

                transaction.Commit();

            }
        }



        /// <summary>
        /// 修改或添加新的扩展数据，该方法会删除其它的扩展数据，需要重新考虑
        /// </summary>
        /// <param name="objectId">对象的ObjectId</param>
        /// <param name="applicationName">程序文件名称</param>
        /// <param name="codeString">索引字符串</param>
        /// <param name="codeValue">扩展数据的值</param>
        public void ChangeOrAddDataValue(ObjectId objectId, string applicationName, string codeString, string codeValue)
        {

            TypedValue[] values = new TypedValue[] {
                    new TypedValue((int)DxfCode.ExtendedDataRegAppName, applicationName) ,
                    new TypedValue((int)DxfCode.ExtendedDataControlString, "{") ,
                    new TypedValue((int)DxfCode.ExtendedDataInteger16, int.Parse(codeString)) ,
                    new TypedValue((int)DxfCode.ExtendedDataAsciiString, codeValue) ,
                    new TypedValue((int)DxfCode.ExtendedDataControlString, "}")
            };

            AddXData(objectId, applicationName, values);
        }







    }
}
