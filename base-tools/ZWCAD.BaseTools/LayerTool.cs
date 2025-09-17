using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.Colors;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ZWCAD.BaseTools
{

    /// <summary>
    /// 图层工具
    /// </summary>
    public class LayerTool
    {
        Document m_document;
        Database m_database;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="document">文档对象</param>
        public LayerTool(Document document)
        {
            m_document = document;
            m_database = m_document.Database;
        }


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="database">图形数据库</param>
        public LayerTool(Database database)
        {
            m_database = database;
        }


        /// <summary>
        /// 删除图层
        /// </summary>
        /// <param name="layerName">图层名称，不分大小写</param>
        /// <returns>如果删除成功，返回true，否则，返回false</returns>
        public bool RemoveLayerTableRecord(string layerName)
        {

            //因为需要用到Editor，所以需要用到m_document
            if (m_document == null)
            {
                MessageBox.Show("找不到图形对象", "CAD");
                return false;
            }

            Database database = m_document.Database;
            Editor editor = m_document.Editor;
            using (Transaction transaction = m_document.TransactionManager.StartTransaction())
            {
                LayerTable layerTable = (LayerTable)transaction.GetObject(database.LayerTableId, OpenMode.ForWrite);

                //当前图层
                LayerTableRecord currentLayer = (LayerTableRecord)transaction.GetObject(database.Clayer, OpenMode.ForRead);


                //为当前图层的情况
                if (currentLayer.Name.ToLower() == layerName.ToLower())
                {
                    editor.WriteMessage("\n" + layerName + "为当前图层，不能删除.");
                    return false;
                }


                //没有此图层的情况
                if (!layerTable.Has(layerName))
                {
                    editor.WriteMessage("\n没有此图层:" + layerName);
                    return false;
                }

                LayerTableRecord record = transaction.GetObject(layerTable[layerName], OpenMode.ForWrite) as LayerTableRecord;

                //图层已经被删除，但是没有从数据库中清除的情况
                if (record.IsErased)
                {
                    editor.WriteMessage("此图层已经被删除:" + layerName);
                    return false;

                }

                ObjectIdCollection objectIdCollection = new ObjectIdCollection();
                objectIdCollection.Add(record.ObjectId);

                //从数据库中清理
                database.Purge(objectIdCollection);

                //图层包含对象的情况，不能删除
                if (objectIdCollection.Count == 0)
                {
                    editor.WriteMessage("\n不能删除包含对象的图层:" + layerName);
                    return false;
                }

                //能删除的情况
                record.Erase();
                transaction.Commit();
                return true;

            }
        }




        /// <summary>
        /// 添加图层
        /// </summary>
        /// <param name="layerName">图层名</param>
        /// <param name="colorIndex">图层颜色索引,颜色默认白色</param>
        /// <param name="isReWrite">如果图层存在，是否修改图层的颜色，默认不修改</param>
        /// <returns>图层的ObjectId,如果创建不成功，返回ObjectId.Null</returns>
        public ObjectId AddLayerTableRecord(string layerName, short colorIndex = 7, bool isReWrite = false)
        {
            //防止输入的颜色超出256
            colorIndex %= 256;


            Color color = Color.FromColorIndex(ColorMethod.ByColor, colorIndex);

            return AddLayerTableRecord(layerName, color, isReWrite);
        }





        /// <summary>
        /// 添加图层
        /// </summary>
        /// <param name="layerName">图层名</param>
        /// <param name="color">图层颜色</param>
        /// <param name="isReWrite">如果图层存在，是否修改图层的颜色，默认不修改</param>
        /// <returns>图层的ObjectId,如果创建不成功，返回ObjectId.Null</returns>
        public ObjectId AddLayerTableRecord(string layerName, Color color, bool isReWrite = false)
        {

            Database database = m_database;

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                LayerTable layerTable = (LayerTable)transaction.GetObject(database.LayerTableId, OpenMode.ForWrite);
                ObjectId objectId = ObjectId.Null;

                //图层已经存在的情况
                if (layerTable.Has(layerName))
                {
                    objectId = layerTable[layerName];
                    //如果接受重新修改
                    if (isReWrite)
                    {
                        LayerTableRecord record = (LayerTableRecord)transaction.GetObject(objectId, OpenMode.ForWrite);
                        record.Color = color;
                    }
                }

                //如果图层不存在
                else
                {
                    LayerTableRecord record = new LayerTableRecord();
                    record.Name = layerName;
                    record.Color = color;

                    //添加到图层记录表中
                    layerTable.Add(record);

                    //添加到数据库中
                    transaction.AddNewlyCreatedDBObject(record, true);

                    objectId = record.Id;
                }

                transaction.Commit();

                return objectId;
            }
        }




        /// <summary>
        /// 设置图层的颜色
        /// </summary>
        /// <param name="layerName">图层名</param>
        /// <param name="colorIndex">图层颜色索引,颜色默认白色</param>
        /// <returns>如果成功，返回true，否则，返回false</returns>
        public bool SetColor(string layerName, short colorIndex = 7)
        {
            //防止输入的颜色超出256
            colorIndex %= 256;

            Color color = Color.FromColorIndex(ColorMethod.ByColor, colorIndex);

            return SetColor(layerName, color);
        }



        /// <summary>
        /// 设置图层的颜色
        /// </summary>
        /// <param name="layerName">图层名</param>
        /// <param name="color">图层颜色</param>
        /// <returns>如果成功，返回true，否则，返回false</returns>
        public bool SetColor(string layerName, Color color)
        {
            //返回值
            bool isSucceed = false;

            ObjectId objectId = ObjectId.Null;

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                LayerTable layerTable = (LayerTable)transaction.GetObject(m_database.LayerTableId, OpenMode.ForRead);

                //图层不存在的情况 直接返回
                if (layerTable.Has(layerName))
                {
                    objectId = layerTable[layerName];
                }

                transaction.Commit();
            }

            if (!objectId.IsNull)
            {
                isSucceed=SetColor(objectId, color);
            }


            return isSucceed;
        }




        /// <summary>
        /// 设置图层的颜色
        /// </summary>
        /// <param name="layerId">图层对象的OBjectId</param>
        /// <param name="color">图层颜色</param>
        /// <returns>如果成功，返回true，否则，返回false</returns>
        public bool SetColor(ObjectId layerId, Color color)
        {
            //返回值
            bool isSucceed = false;

            if (layerId.IsNull)
            {
                return isSucceed;
            }

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                if (transaction.GetObject(layerId, OpenMode.ForWrite) is LayerTableRecord record)
                {
                    record.Color = color;
                    transaction.Commit();

                    isSucceed = true;
                }

            }


            return isSucceed;
        }




        /// <summary>
        /// 设置图层的线形
        /// </summary>
        /// <param name="layerId">图层对象的OBjectId</param>
        /// <param name="lineTypeId">线形对象的ObjectId</param>
        /// <returns>如果成功，返回true，否则，返回false</returns>
        public bool SetLineType(ObjectId layerId, ObjectId lineTypeId)
        {
            //返回值
            bool isSucceed = false;

            if (layerId.IsNull || lineTypeId.IsNull)
            {
                return isSucceed;
            }

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                if (transaction.GetObject(layerId, OpenMode.ForWrite) is LayerTableRecord record)
                {
                    record.LinetypeObjectId = lineTypeId;
                    transaction.Commit();

                    isSucceed = true;
                }

            }


            return isSucceed;
        }








        /// <summary>
        /// 设置图层的线形
        /// </summary>
        /// <param name="layerId">图层对象的OBjectId</param>
        /// <param name="ltname">线形的名称</param>
        /// <param name="ltFileName">线形文件名称，全路径，默认系统自带</param>
        /// <returns>如果成功，返回true，否则，返回false</returns>
        public bool SetLineType(ObjectId layerId, string ltname, string ltFileName = "acad.lin")
        {
            //返回值
            bool isSucceed = false;

            if (layerId.IsNull)
            {
                return isSucceed;
            }


            //先获取或加载线形
            LineTypeTool lineTypeTool = new LineTypeTool(m_database);

            ObjectId lineTypeId = lineTypeTool.GetOrLoadLineType(ltname, ltFileName);
            //如果加载不成功 直接结束
            if (lineTypeId.IsNull)
            {
                return isSucceed;
            }

            isSucceed= SetLineType(layerId, lineTypeId);

            return isSucceed;
        }






        /// <summary>
        /// 修改多个对象的图层，不判断图层是否存在
        /// </summary>
        /// <param name="objectIds">对象的ObjectId的数组</param>
        /// <param name="layerName">图层名称</param>
        public void ChangeEntitysLayer(ObjectId[] objectIds, string layerName)
        {

            Database database = m_database;

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {

                //分别替换
                foreach (ObjectId objectId in objectIds)
                {

                    Entity entity = transaction.GetObject(objectId, OpenMode.ForWrite) as Entity;

                    entity.Layer = layerName;

                }

                transaction.Commit();

            }
        }




        /// <summary>
        /// 修改对象列表的图层，不判断图层是否存在
        /// </summary>
        /// <param name="objectIds">对象的ObjectId的列表</param>
        /// <param name="layerName">图层名称</param>
        public void ChangeEntitysLayer(List<ObjectId> objectIds, string layerName)
        {
            Database database = m_database;

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {

                //分别替换
                foreach (ObjectId objectId in objectIds)
                {

                    Entity entity = transaction.GetObject(objectId, OpenMode.ForWrite) as Entity;

                    entity.Layer = layerName;

                }

                transaction.Commit();

            }
        }












        /// <summary>
        /// 修改对象的图层，不判断图层是否存在
        /// </summary>
        /// <param name="objectId">对象的ObjectId的列表</param>
        /// <param name="layerName">图层名称</param>
        public void ChangeEntityLayer(ObjectId objectId, string layerName)
        {
            Database database = m_database;

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {

                Entity entity = transaction.GetObject(objectId, OpenMode.ForWrite) as Entity;
                entity.Layer = layerName;
                transaction.Commit();

            }
        }




        /// <summary>
        /// 获取实体的图层名称
        /// </summary>
        /// <param name="objectId">实体的ObjectId</param>
        /// <returns>图层名称string</returns>
        public string GetEntityLayerName(ObjectId objectId)
        {
            Database database = m_database;

            ObjectTool objectTool = new ObjectTool(database);
            Entity entity = objectTool.GetObject(objectId) as Entity;

            return entity.Layer;
        }




        /// <summary>
        /// 设置当前图层
        /// </summary>
        /// <param name="layerName">图层名</param>
        /// <returns>如果成功，返回true，否则，返回false</returns>
        public bool SetCurrentLayer(string layerName)
        {

            //返回值
            bool isSucceed = false;

            Database database = m_database;

            using (Transaction acTrans = database.TransactionManager.StartTransaction())
            {
                LayerTable acLyrTbl = acTrans.GetObject(database.LayerTableId,
                                                   OpenMode.ForRead) as LayerTable;

                if (acLyrTbl.Has(layerName))
                {
                    database.Clayer = acLyrTbl[layerName];
                    acTrans.Commit();
                    isSucceed = true;
                }
            }

            return isSucceed;
        }






        /// <summary>
        /// 获取当前图层名
        /// </summary>
        /// <returns>图层名</returns>
        public string GetCurrentLayer()
        {

            using (Transaction acTrans = m_database.TransactionManager.StartTransaction())
            {
                ObjectId layerId = m_database.Clayer;
                LayerTableRecord record = acTrans.GetObject(layerId, OpenMode.ForRead) as LayerTableRecord;
                return record.Name;

            }
        }





        /// <summary>
        /// 获取所有的图层名称列表
        /// </summary>
        /// <returns>如果失败，返回空的列表</returns>
        public List<string> GetAllLayerNames()
        {
            //返回值
            List<string> layerNameLst = new List<string>();

            //using (Transaction transaction = m_database.TransactionManager.StartOpenCloseTransaction())
            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                LayerTable lt = transaction.GetObject(m_database.LayerTableId, OpenMode.ForRead) as LayerTable;
                foreach (ObjectId layerId in lt)
                {
                    LayerTableRecord layer = transaction.GetObject(layerId, OpenMode.ForRead) as LayerTableRecord;
                    layerNameLst.Add(layer.Name);
                }

                transaction.Commit();
            }
            return layerNameLst;
        }







    }
}


