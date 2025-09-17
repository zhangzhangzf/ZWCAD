using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;
using ZwSoft.ZwCAD.Geometry;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace ZWCAD.BaseTools
{
    /// <summary>
    /// 多行文字对象工具
    /// </summary>
    public class MTextTool
    {
        Document m_document;
        Database m_database;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="document">文档对象</param>
        public MTextTool(Document document)
        {
            m_document = document;
            m_database = m_document.Database;

        }


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="database">数据库</param>
        public MTextTool(Database database)
        {
            m_database = database;
        }


        /// <summary>
        /// 修改当前文字样式
        /// </summary>
        public void ChangeCurrentTextStyleFontFile()
        {


            //dll所在的文件夹路径
            string dllPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);


            string relativeFileName = @"\fonts\arialbi.ttf";


            //先获取同一文件夹路径下
            string absoluteFileName = dllPath + relativeFileName;



            if (!File.Exists(absoluteFileName)) // 不存在，再上一层找
            {
                //上一级路径
                dllPath = Path.GetDirectoryName(dllPath);
                absoluteFileName = dllPath + relativeFileName;


                //找不到，直接返回
                if (!File.Exists(absoluteFileName))
                {
                    return;
                }
            }


            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                //以写模式打开当前文字样式
                TextStyleTableRecord textStyleRecord = transaction.GetObject(m_database.Textstyle, OpenMode.ForWrite) as TextStyleTableRecord;

                //改用字体
                textStyleRecord.FileName = absoluteFileName;

                transaction.Commit();
            }
        }



        /// <summary>
        /// 选择模型空间的所有多行文字样式，然后以其中一个的文字样式作为当前的文字样式,需要初始化m_document
        /// </summary>
        public void ChangeCurrentTextStyleToExistTextStyle()
        {
            if (m_document == null)
            {

                MessageBox.Show("文档对象为空", "Tips");
                return;
            }


            //选择图块内的文字
            TypedValue[] typedValues = new TypedValue[] { new TypedValue((int)DxfCode.Start, "MTEXT") };
            SelectionFilter selectionFilter = new SelectionFilter(typedValues);

            SelectionTool selectEntityTool = new SelectionTool(m_document);
            ObjectId[] objectIds = selectEntityTool.SelectAllInSpaceToObjectIds(selectionFilter);

            //说明选到了对象，可以修改
            if (objectIds != null && objectIds.Length > 0)
            {
                ObjectTool objectTool = new ObjectTool(m_database);
                MText mText = objectTool.GetObject(objectIds[0]) as MText;
                ObjectId textStyleId = mText.TextStyleId;

                //设置为当前文字样式
                using (Transaction transaction = m_database.TransactionManager.StartTransaction())
                {
                    m_database.Textstyle = textStyleId;
                    transaction.Commit();
                }


            }

        }



        /// <summary>
        /// 获取多行文字的插入点
        /// </summary>
        /// <param name="mTextObjectId">多行文字的ObjectId</param>
        /// <returns>插入点Point3d</returns>
        public Point3d GetTextInsertPoint(ObjectId mTextObjectId)
        {

            Database database = m_database;

            ObjectTool objectTool = new ObjectTool(database);
            MText mText = objectTool.GetObject(mTextObjectId) as MText;
            return mText.Location;
        }





        /// <summary>
        /// 获取多行文字的内容
        /// </summary>
        /// <param name="textObjectId">多行文字的ObjectId</param>
        /// <returns>内容string,如果不为多行文字，返回null</returns>
        public string GetTextString(ObjectId textObjectId)
        {
            Database database = m_database;

            ObjectTool objectTool = new ObjectTool(database);

            MText mText = objectTool.GetObject(textObjectId) as MText;
            if (mText == null)
            {
                return null;
            }
            return mText.Text;
        }






        /// <summary>
        /// 设置多行文字的内容
        /// </summary>
        /// <param name="textObjectId">多行文字的ObjectId</param>
        /// <param name="textString">文字内容</param>
        /// <returns>如果成功，返回true,如果不为多行文字或失败，返回false</returns>
        public bool SetTextString(ObjectId textObjectId, string textString)
        {
            //返回值
            bool result = false;

            Database database = m_database;

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                try
                {

                    MText text = transaction.GetObject(textObjectId, OpenMode.ForWrite, true) as MText;

                    if (text == null)
                    {
                        return result;
                    }

                    text.Contents = textString;
                    transaction.Commit();
                    result = true;
                }
                catch
                {
                    transaction.Abort();
                }

            }

            return result;

        }







        /// <summary>
        /// 获取多行文字的高度
        /// </summary>
        /// <param name="mTextObjectId">多行文字的ObjectId</param>
        /// <returns>返回高度值double，如果不为多行文字，返回double.NaN</returns>
        public double GetTextHeight(ObjectId mTextObjectId)
        {
            Database database = m_database;

            ObjectTool objectTool = new ObjectTool(database);
            MText mText = objectTool.GetObject(mTextObjectId) as MText;
            if (mText == null)
            {
                return double.NaN;
            }
            return mText.TextHeight;
        }





        /// <summary>
        /// 修改多行文字的高度（单位：mm）
        /// </summary>
        /// <param name="textObjectId">多行文字的ObjectId</param>
        /// <param name="height">多行文字的高度（单位：mm）</param>
        /// <returns>如果成功，返回true，否则，返回false</returns>
        public bool SetTextHeight(ObjectId textObjectId, double height)
        {
            //返回值
            bool isSucceed = false;

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    if (transaction.GetObject(textObjectId, OpenMode.ForWrite) is MText text)
                    {
                        text.TextHeight = height;
                        transaction.Commit();
                        isSucceed = true;
                    }
                }
                catch
                {
                    transaction.Abort();
                }
            }
            return isSucceed;
        }
















        /// <summary>
        /// 将多行文字转换为单行文字
        /// </summary>
        /// <param name="mTextId">多行文字的ObjectId</param>
        /// <param name="eraseMtext">是否删除多行文字，默认不删除</param>
        /// <returns>如果转换成功，返回单行文字的ObjectId，否则，返回ObjectId.Null</returns>
        public ObjectId ConvertToText(ObjectId mTextId, bool eraseMtext = false)
        {
            Database database = m_database;

            ObjectTool objectTool = new ObjectTool(database);
            List<ObjectId> objectIds = objectTool.ExplodeEntity(mTextId, eraseMtext);

            //如果操作没有成功，返回ObjectId.Null
            if (objectIds == null)
            {
                return ObjectId.Null;
            }

            return objectIds[0];



            //using (Transaction transaction = mTextId.Database.TransactionManager.StartTransaction())
            //{

            //    MText mText = transaction.GetObject(mTextId, OpenMode.ForWrite) as MText;

            //    //这个函数不知道干什么用的，但是不是炸开多行文字的函数
            //    //mText.ConvertFieldToText();
            //    DBObjectCollection dBObjectCollection = new DBObjectCollection();
            //    mText.Explode(dBObjectCollection);

            //    transaction.Commit();
            //}



        }



        /// <summary>
        /// 获取多行文字的宽度因子
        /// </summary>
        /// <param name="mTextId">多行文字的ObjectId</param>
        /// <returns>如果成功，返回宽度因子，否则，返回double.NaN</returns>
        public double GetWidthFactor(ObjectId mTextId)
        {
            //返回值
            double widthFactor = double.NaN;

            //转换为单行文字，不删除源对象
            ObjectId newTextId = ConvertToText(mTextId);

            //转换不成功
            if (newTextId == ObjectId.Null)
            {
                return widthFactor;
            }


            Database database = m_database;

            ObjectTool objectTool = new ObjectTool(database);
            DBText dBText = objectTool.GetObject(newTextId) as DBText;

            if (dBText != null)
            {
                widthFactor = dBText.WidthFactor;
            }


            //删除新生成的单行文字对象
            objectTool.EraseDBObject(newTextId);

            return widthFactor;
        }








        ///// <summary>
        ///// 选择框内的所有文字对象
        ///// </summary>
        ///// <param name="minPoint">选择框的最小点</param>
        ///// <param name="maxPoint">选择框的最大点</param>
        ///// <returns>文字对象的ObjectId列表,如果不存在，返回空的列表</returns>
        //public List<ObjectId> GetTextsInBoundingBox(Point3d minPoint, Point3d maxPoint)
        //{

        //    //返回值
        //    List<ObjectId> foundTextLst = new List<ObjectId>();

        //    //选择图块内的文字
        //    TypedValue[] typedValues = new TypedValue[] { new TypedValue((int)DxfCode.Start, "MTEXT") };
        //    SelectionFilter selectionFilter = new SelectionFilter(typedValues);

        //    SelectionTool selectEntityTool = new SelectionTool(m_document);


        //    ObjectId[] objectIds = selectEntityTool.SelectAllInModelSpaceToObjectIds(selectionFilter);

        //    if (objectIds != null)
        //    {
        //        PointTool pointTool = new PointTool();

        //        foreach (ObjectId objectId in objectIds)
        //        {
        //            Point3d insertPoint = GetTextInsertPoint(objectId);

        //            //判断文字的插入点是否在边界框中
        //            if (pointTool.IsPointInsideBoundingBox(insertPoint, minPoint, maxPoint))
        //            {
        //                foundTextLst.Add(objectId);
        //            }
        //        }
        //    }



        //    return foundTextLst;
        //}




        /// <summary>
        /// 选择框内的所有多行文字对象
        /// </summary>
        /// <param name="minPoint">选择框的最小点</param>
        /// <param name="maxPoint">选择框的最大点</param>
        /// <param name="spaceName">空间名称</param>
        /// <returns>文字对象的ObjectId列表,如果不存在，返回空的列表</returns>
        public List<ObjectId> GetTextsInBoundingBox(Point3d minPoint, Point3d maxPoint, string spaceName = "MODELSPACE")
        {

            Database database = m_database;


            DatabaseTool databaseTool = new DatabaseTool(database);
            List<ObjectId> objectIds = databaseTool.GetAllEntityIds("MTEXT", spaceName);

            //返回值
            List<ObjectId> foundTextLst = new List<ObjectId>();


            PointTool pointTool = new PointTool();

            foreach (ObjectId objectId in objectIds)
            {
                Point3d insertPoint = GetTextInsertPoint(objectId);

                //判断文字的插入点是否在边界框中
                if (pointTool.IsPointInsideBoundingBox(insertPoint, minPoint, maxPoint))
                {
                    foundTextLst.Add(objectId);
                }
            }




            return foundTextLst;
        }













    }
}
