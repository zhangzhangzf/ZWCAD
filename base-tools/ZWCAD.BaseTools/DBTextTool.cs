

using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace ZWCAD.BaseTools
{
    /// <summary>
    /// 单行文字对象工具
    /// </summary>
    public class DBTextTool
    {
        Document m_document;
        Database m_database;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="document">文档对象</param>
        public DBTextTool(Document document)
        {
            m_document = document;
            m_database = m_document.Database;
        }


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="database">数据库对象</param>
        public DBTextTool(Database database)
        {
            m_database = database;
        }






        /// <summary>
        /// 获取单行文字的插入点
        /// </summary>
        /// <param name="textObjectId">单行文字的ObjectId</param>
        /// <returns>插入点Point3d</returns>
        public Point3d GetTextInsertPoint(ObjectId textObjectId)
        {

            Database database = m_database;

            ObjectTool objectTool = new ObjectTool(database);
            DBText text = objectTool.GetObject(textObjectId) as DBText;

            return text.Position;
        }





        /// <summary>
        /// 获取单行文字的内容
        /// </summary>
        /// <param name="textObjectId">单行文字的ObjectId</param>
        /// <returns>内容string,如果不为单行文字，返回null</returns>
        public string GetTextString(ObjectId textObjectId)
        {
            Database database = m_database;

            ObjectTool objectTool = new ObjectTool(database);

            DBText text = objectTool.GetObject(textObjectId) as DBText;
            if (text == null)
            {
                return null;
            }
            return text.TextString;
        }






        /// <summary>
        /// 设置单行文字的内容
        /// </summary>
        /// <param name="textObjectId">单行文字的ObjectId</param>
        /// <param name="textString">文字内容</param>
        /// <returns>如果成功，返回true,如果不为单行文字或失败，返回false</returns>
        public bool SetTextString(ObjectId textObjectId, string textString)
        {
            //返回值
            bool result = false;

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    if (transaction.GetObject(textObjectId, OpenMode.ForWrite, true) is DBText text)
                    {

                        text.TextString = textString;
                        transaction.Commit();
                        result = true;
                    }
                }
                catch
                {
                    transaction.Abort();
                }

            }

            return result;

        }


        /// <summary>
        /// 设置单行文字的对齐方式
        /// </summary>
        /// <param name="textObjectId">单行文字的ObjectId</param>
        /// <param name="attachmentPoint">对齐方式 第一个单词表示Vertical mode,第二个单词表示Horizontal mode，两者组合</param>
        /// AttachmentPoint.TopMid,中上
        ///AttachmentPoint.MiddleMid,正中
        ///  AttachmentPoint.BottomMid, 中下
        ///  AttachmentPoint.BaseMid,中间
        ///  AttachmentPoint.TopFit,右上，但是是垂直的
        ///  AttachmentPoint.MiddleFit,右中，但是是垂直的
        ///   AttachmentPoint.BottomFit,右下，但是是垂直的
        ///  AttachmentPoint.BaseFit,布满，但是是垂直的
        ///  AttachmentPoint.TopAlign,左上，但是是垂直的
        ///  AttachmentPoint.MiddleAlign,左中，但是是垂直的
        ///  AttachmentPoint.BottomAlign, 左下，但是是垂直的
        ///  AttachmentPoint.BaseAlign,  对齐，但是是垂直的
        ///   AttachmentPoint.BaseRight,右对齐
        ///  AttachmentPoint.BaseCenter,居中
        ///  AttachmentPoint.BaseLeft, 左对齐
        ///  AttachmentPoint.BottomRight,右下
        ///  AttachmentPoint.BottomCenter, 中下
        ///  AttachmentPoint.BottomLeft,左下
        ///   AttachmentPoint.MiddleRight,右中
        ///  AttachmentPoint.MiddleCenter, 正中
        ///  AttachmentPoint.MiddleLeft,左中
        ///  AttachmentPoint.TopRight, 右上
        ///  AttachmentPoint.TopCenter,中上
        ///  AttachmentPoint.TopLeft 左上
        /// <returns>如果成功，返回true,如果不为单行文字或失败，返回false</returns>
        public bool SetJustify(ObjectId textObjectId, AttachmentPoint attachmentPoint)
        {
            //返回值
            bool result = false;

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    if (transaction.GetObject(textObjectId, OpenMode.ForWrite) is DBText text)
                    {
                        text.Justify = attachmentPoint;
                        text.AlignmentPoint = text.Position;
                        transaction.Commit();
                        result = true;
                    }
                }
                catch
                {
                    transaction.Abort();
                }

            }

            return result;

        }















        /// <summary>
        /// 获取单行文字的高度（单位：mm）
        /// </summary>
        /// <param name="textObjectId">单行文字的ObjectId</param>
        /// <returns>返回高度值double，如果不为单行文字，返回double.NaN</returns>
        public double GetTextHeight(ObjectId textObjectId)
        {
            Database database = m_database;

            ObjectTool objectTool = new ObjectTool(database);
            DBText text = objectTool.GetObject(textObjectId) as DBText;
            if (text == null)
            {
                return double.NaN;
            }
            return text.Height;
        }



        /// <summary>
        /// 修改单行文字的高度（单位：mm）
        /// </summary>
        /// <param name="textObjectId">单行文字的ObjectId</param>
        /// <param name="height">单行文字的高度（单位：mm）</param>
        /// <returns>如果成功，返回true，否则，返回false</returns>
        public bool SetTextHeight(ObjectId textObjectId, double height)
        {
            //返回值
            bool isSucceed = false;

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    if (transaction.GetObject(textObjectId, OpenMode.ForWrite) is DBText dBText)
                    {
                        dBText.Height = height;
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
        /// 获取单行文字的宽度因子
        /// </summary>
        /// <param name="textId">单行文字的ObjectId</param>
        /// <returns>如果成功，返回宽度因子，否则，返回double.NaN</returns>
        public double GetWidthFactor(ObjectId textId)
        {
            //返回值
            double widthFactor = double.NaN;

            Database database = m_database;

            ObjectTool objectTool = new ObjectTool(database);
            DBText dBText = objectTool.GetObject(textId) as DBText;

            if (dBText != null)
            {
                widthFactor = dBText.WidthFactor;
            }

            return widthFactor;
        }






        /// <summary>
        /// 选择框内的所有单行文字对象
        /// </summary>
        /// <param name="minPoint">选择框的最小点</param>
        /// <param name="maxPoint">选择框的最大点</param>
        /// <param name="spaceName">空间名称</param>
        /// <returns>文字对象的ObjectId列表,如果不存在，返回空的列表</returns>
        public List<ObjectId> GetTextsInBoundingBox(Point3d minPoint, Point3d maxPoint, string spaceName = "MODELSPACE")
        {

            Database database = m_database;


            DatabaseTool databaseTool = new DatabaseTool(database);
            List<ObjectId> objectIds = databaseTool.GetAllEntityIds("TEXT", spaceName);

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






        /// <summary>
        /// 获取字符串在特定文字样式、高度和宽度因子下的长度和高度（单位：mm）
        /// </summary>
        /// <param name="textContext">字符串内容</param>
        /// <param name="styleName">样式名称</param>
        /// <param name="textHeight">文字高度</param>
        /// <param name="scaleFactor">文字宽度因子</param>
        /// <returns>长度和高度组成的数组，如果失败，返回null</returns>
        public double[] GetTextLengthAndHeight(string textContext, string styleName, double textHeight, double scaleFactor)
        {
            //返回值
            double[] lengthAndHeight = null;

            Database database = m_database;

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                try
                {
                    SymbolTable symTable = (SymbolTable)transaction.GetObject(database.TextStyleTableId, OpenMode.ForRead);

                    if (!symTable.Has(styleName)) //不存在 直接返回
                    {
                        transaction.Commit();
                        return lengthAndHeight;
                    }


                    //以下为存在
                    TextStyleTableRecord textStyleTableRecord = (TextStyleTableRecord)transaction.GetObject(symTable[styleName], OpenMode.ForRead);


                    var font = textStyleTableRecord.Font;

                    string bigFontFileName = textStyleTableRecord.BigFontFileName;

                    bool isVertical = textStyleTableRecord.IsVertical;

                    var ts = new ZwSoft.ZwCAD.GraphicsInterface.TextStyle
                    {
                        Font = font,
                        BigFontFileName = bigFontFileName,
                        StyleName = styleName,
                        Vertical = isVertical,
                        TextSize = textHeight,
                        XScale = scaleFactor
                    };

                    //RMo:需要重新考虑
                    //Extents2d extents2D = ts.ExtentsBox(textContext, true, false, null);
                    Extents2d extents2D = new Extents2d();

                    //var minPoint = extents2D.MinPoint;
                    var maxPoint = extents2D.MaxPoint;

                    lengthAndHeight = new double[]
                    {
                        maxPoint.X,
                        maxPoint.Y
                    };

                }
                catch
                {

                }

                transaction.Commit();
            }

            return lengthAndHeight;
        }





        /// <summary>
        /// 获取字符串在特定文字样式、高度和宽度因子下的长度（单位：mm）
        /// </summary>
        /// <param name="textContext">字符串内容</param>
        /// <param name="styleName">样式名称</param>
        /// <param name="textHeight">文字高度</param>
        /// <param name="scaleFactor">文字宽度因子</param>
        /// <returns>长度，如果失败，返回double.NaN</returns>
        public double GetTextLength(string textContext, string styleName, double textHeight, double scaleFactor)
        {
            //返回值
            double length = double.NaN;

            double[] lengthAndHeight = GetTextLengthAndHeight(textContext, styleName, textHeight, scaleFactor);

            if (lengthAndHeight != null)
            {
                length = lengthAndHeight[0];
            }

            return length;

        }






        /// <summary>
        /// 获取字符串列表在特定文字样式、高度和宽度因子下的长度列表（单位：mm）
        /// </summary>
        /// <param name="textContextLst">字符串内容列表</param>
        /// <param name="styleName">样式名称</param>
        /// <param name="textHeight">文字高度</param>
        /// <param name="scaleFactor">文字宽度因子</param>
        /// <returns>长度列表，如果失败，返回空的列表</returns>
        public List<double> GetTextLengthLst(List<string> textContextLst, string styleName, double textHeight, double scaleFactor)
        {
            //返回值
            List<double> lengthLst = new List<double>();

            foreach (var textContext in textContextLst)
            {
                double length = GetTextLength(textContext, styleName, textHeight, scaleFactor);
                if (!double.IsNaN(length))
                {
                    lengthLst.Add(length);
                }

            }

            return lengthLst;
        }





        /// <summary>
        /// 获取字符串列表在特定文字样式、高度和宽度因子下的最大长度（单位：mm）
        /// </summary>
        /// <param name="textContextLst">字符串内容列表</param>
        /// <param name="styleName">样式名称</param>
        /// <param name="textHeight">文字高度</param>
        /// <param name="scaleFactor">文字宽度因子</param>
        /// <returns>最大长度，如果失败，返回double.NaN</returns>
        public double GetTextLstMaxLength(List<string> textContextLst, string styleName, double textHeight, double scaleFactor)
        {
            //返回值
            double maxLength = double.NaN;

            List<double> lengthLst = GetTextLengthLst(textContextLst, styleName, textHeight, scaleFactor);

            if (lengthLst.Count > 0)
            {
                lengthLst.Sort();
                maxLength = lengthLst.LastOrDefault();
            }

            return maxLength;

        }



        /// <summary>
        /// 创建单行文字对象
        /// </summary>
        /// <param name="textString">文字内容</param>
        /// <param name="insertPoint">插入点</param>
        /// <param name="textHeight">文字高度</param>
        /// <param name="scaleFactor">宽度因子</param>
        /// <param name="styleName">文字样式名称，如果为null，将用当前的文字样式</param>
        /// <param name="spaceName">空间名称，默认模型空间</param>
        /// <returns>如果失败，返回ObjectId.Null</returns>
        public ObjectId CreateText(string textString, Point3d insertPoint, double textHeight = 200, double scaleFactor = 0.8, string styleName = null, string spaceName = "MODELSPACE")
        {
            //返回值
            ObjectId objectId = ObjectId.Null;


            //先获取空间
            SpaceTool spaceTool = new SpaceTool(m_database);
            ObjectId spaceId = spaceTool.GetSpaceByName(spaceName);

            if (spaceId.IsNull)
            {
                return objectId;
            }

            objectId = CreateText(spaceId, textString, insertPoint, textHeight, scaleFactor, styleName);

            return objectId;
        }




        /// <summary>
        /// 创建单行文字对象
        /// </summary>
        /// <param name="spaceId">空间对象的ObjectId</param>
        /// <param name="textString">文字内容</param>
        /// <param name="insertPoint">插入点</param>
        /// <param name="textHeight">文字高度</param>
        /// <param name="scaleFactor">宽度因子</param>
        /// <param name="styleName">文字样式名称，如果为null，将用当前的文字样式</param>
        /// <returns>如果失败，返回ObjectId.Null</returns>
        public ObjectId CreateText(ObjectId spaceId, string textString, Point3d insertPoint, double textHeight = 200, double scaleFactor = 0.8, string styleName = null)
        {
            //返回值
            ObjectId objectId = ObjectId.Null;


            DatabaseTool databaseTool = new DatabaseTool(m_database);

            //获取文字样式
            ObjectId textStyleId = databaseTool.GetTextStyleByName(styleName);


            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {

                    if (transaction.GetObject(spaceId, OpenMode.ForWrite) is BlockTableRecord blockTableRecord)
                    {

                        DBText acText = new DBText();
                        acText.SetDatabaseDefaults();
                        acText.Position = insertPoint;
                        acText.Height = textHeight;
                        acText.TextString = textString;
                        acText.WidthFactor = scaleFactor;

                        if (!textStyleId.IsNull)
                        {
                            acText.TextStyleId=textStyleId;
                        }

                        objectId = blockTableRecord.AppendEntity(acText);
                        transaction.AddNewlyCreatedDBObject(acText, true);
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();
                }
            }

            return objectId;
        }



        /// <summary>
        /// 创建单行文字对象
        /// </summary>
        /// <param name="spaceId">空间对象的ObjectId</param>
        /// <param name="textStringAndinsertPointMap">文字内容和插入点的映射</param>
        /// <param name="textHeight">文字高度</param>
        /// <param name="scaleFactor">宽度因子</param>
        /// <param name="styleName">文字样式名称，如果为null，将用当前的文字样式</param>
        /// <returns>如果失败，返回空的列表</returns>
        public List<ObjectId> CreateTexts(ObjectId spaceId, Dictionary<string, Point3d> textStringAndinsertPointMap, double textHeight = 200, double scaleFactor = 0.8, string styleName = null)
        {
            //返回值
            List<ObjectId> textIdLst = new List<ObjectId>();


            DatabaseTool databaseTool = new DatabaseTool(m_database);

            //获取文字样式
            ObjectId textStyleId = databaseTool.GetTextStyleByName(styleName);


            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {

                    if (transaction.GetObject(spaceId, OpenMode.ForWrite) is BlockTableRecord blockTableRecord)
                    {

                        foreach (var item in textStringAndinsertPointMap)
                        {
                            DBText acText = new DBText();
                            acText.SetDatabaseDefaults();
                            acText.Position =item.Value;
                            acText.Height = textHeight;
                            acText.TextString = item.Key;
                            acText.WidthFactor = scaleFactor;

                            if (!textStyleId.IsNull)
                            {
                                acText.TextStyleId=textStyleId;
                            }

                            ObjectId objectId = blockTableRecord.AppendEntity(acText);
                            transaction.AddNewlyCreatedDBObject(acText, true);

                            textIdLst.Add(objectId);

                        }

                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();
                    textIdLst.Clear();
                }
            }

            return textIdLst;
        }


















    }
}
