using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.Colors;
using ZwSoft.ZwCAD.DatabaseServices;
using Mrf.CSharp.BaseTools;
using System.Collections.Generic;
using System.IO;
using Point3dInCAD = ZwSoft.ZwCAD.Geometry.Point3d;

namespace ZWCAD.BaseTools
{
    /// <summary>
    /// 数据库工具
    /// </summary>
    public class DatabaseTool
    {

        Document m_document;

        Database m_database;


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="document">文档对象</param>
        public DatabaseTool(Document document)
        {
            m_document = document;
            m_database = m_document.Database;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="database">数据库对象</param>
        public DatabaseTool(Database database)
        {
            m_database = database;


        }






        /// <summary>
        /// 清理所有不需要的块
        /// </summary>
        public void PurgeAllUnreferencedBlockTables()
        {


            Database database = m_database;


            //BlockTool blockTool = new BlockTool(m_document);

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                BlockTable blockTable = transaction.GetObject(database.BlockTableId, OpenMode.ForRead) as BlockTable;

                ObjectIdCollection objectIdCollection = new ObjectIdCollection();
                foreach (ObjectId objectId in blockTable)
                {

                    //好像没什么用
                    ////先判断块表记录有没有块参照
                    //ObjectIdCollection collection = blockTool.GetBlockReferenceIds(objectId);

                    //if(collection.Count==0) //如果没有块参照，才会清理
                    //{
                    //objectIdCollection.Add(objectId);
                    //}

                    objectIdCollection.Add(objectId);


                }


                database.Purge(objectIdCollection);

                transaction.Commit();
            }
        }





        /// <summary>
        /// 清理所有未使用的块,需要初始化document
        /// </summary>
        public void PurgeAllUnusedBlockTableRecords()
        {
            //清理未使用的块
            string commandString = "-purge\nb\n\nn\n";
            m_document.SendStringToExecute(commandString, true, false, false);

        }

        /// <summary>
        /// 清理给定块表名称的对象，不判断是否还在数据库中使用，需要初始化document
        /// </summary>
        public void PurgeBlockTableRecordByName(string blockTableRecordName)
        {
            //清理未使用的块
            string commandString = "-purge\nb\n" + blockTableRecordName + "\nn\n";
            m_document.SendStringToExecute(commandString, true, false, false);

        }



        /// <summary>
        /// 清除块参照对应的块表记录，不判断是否还在数据库中使用
        /// </summary>
        /// <param name="blockReferenceId">块参照的ObjectId</param>
        public void PurgeBlockTableRecord(ObjectId blockReferenceId)
        {
            //先获取块表记录的名称

            BlockTool blockTool;
            if (m_database != null)
            {
                blockTool = new BlockTool(m_database);
            }

            else
            {
                blockTool = new BlockTool(m_document);
            }


            string recordName = blockTool.FindBlockTableRecordName(blockReferenceId);
            PurgeBlockTableRecordByName(recordName);

        }



        /// <summary>
        /// 遍历模型空间所有Entity对象的ObjectId
        /// </summary>
        /// <returns>对象的ObjectId列表</returns>
        public List<ObjectId> GetAllEntityIdsInModelSpace()
        {
            //返回值
            List<ObjectId> objectIds = new List<ObjectId>();

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {

                // 模型空间
                BlockTable blkTbl = transaction.GetObject(m_database.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modelSpace = transaction.GetObject(
                    blkTbl[BlockTableRecord.ModelSpace], OpenMode.ForRead)
                    as BlockTableRecord;


                // 遍历模型空间
                foreach (ObjectId id in modelSpace)
                {
                    DBObject dbobj = transaction.GetObject(id, OpenMode.ForRead);
                    if (dbobj is Entity)
                    {
                        objectIds.Add(id);
                    }
                }
                transaction.Commit();
            }

            return objectIds;
        }








        /// <summary>
        /// 将指定对象列表保存到外部文件 需要注意：如果对象保存到外部文件之后，当前文件中的会自动删除，还找不到办法做到不删除
        /// </summary>
        /// <param name="objectIdLst">要保存的对象ObjectId列表</param>
        /// <param name="outPutFileName">要保存的外部文件全路径</param>
        /// <returns>自动判断文件是否已经存在，如果存在，自动使用默认的文件，并返回，如果文件不存在，则返回原来指定的文件名</returns>
        private string WBlockToOutDrawing(List<ObjectId> objectIdLst, string outPutFileName)
        {
            ObjectIdCollection objIds = new ObjectIdCollection(objectIdLst.ToArray());

            FileTool fileTool = new FileTool();

            //获取新的文件名
            outPutFileName = fileTool.FindNewName(outPutFileName);


            //transaction对这段不管用

            using (Database newDb = new Database(true, false))
            {
                m_document.Database.Wblock(newDb, objIds, Point3dInCAD.Origin, DuplicateRecordCloning.Ignore);
                newDb.SaveAs(outPutFileName, DwgVersion.Current);
            }
            return outPutFileName;

        }





        /// <summary>
        /// 遍历模型空间或图纸空间中所有类型为typeName的对象列表，比如块为"INSERT"
        /// </summary>
        /// <param name="typeName">类型名称，不分大小写</param>
        /// <param name="spaceName">模型空间或图纸空间，如果为"PAPERSPACE"（不分大小写），为图纸空间，如果指定的名称为其它的图纸空间名称，则为指定的图纸空间名称；为空或其它，为模型空间</param>
        /// <returns>所有对象的List<Objectd></Objectd>,如果找不到值，返回空的列表</returns>
        public List<ObjectId> GetAllEntityIds(string typeName, string spaceName = "MODELSPACE")
        {
            //返回值
            List<ObjectId> objectIds = new List<ObjectId>();

            //先获取空间
            SpaceTool spaceTool = new SpaceTool(m_database);
            ObjectId spaceId = spaceTool.GetSpaceByName(spaceName);

            if (spaceId.IsNull)
            {
                return objectIds;
            }

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {

                BlockTableRecord space = transaction.GetObject(spaceId, OpenMode.ForRead) as BlockTableRecord;

                //变为大写，从而不分大小写
                string typeNameUpper = typeName.ToUpper();

                // 遍历空间
                foreach (ObjectId id in space)
                {

                    string dxfName = id.ObjectClass.DxfName.ToUpper();

                    if (dxfName == typeNameUpper)
                    {

                        objectIds.Add(id);
                    }

                }
                transaction.Commit();

            }

            return objectIds;
        }





        /// <summary>
        /// 遍历模型空间或图纸空间中所有类型在对象类型列表中的对象，比如块为"INSERT"
        /// </summary>
        /// <param name="typeNameLst">类型名称列表，不分大小写</param>
        /// <param name="spaceName">模型空间还是图纸空间名称，不分大小写，默认模型空间</param>
        /// <returns>所有对象的Objectd列表,如果找不到值，返回空的列表</returns>
        public List<ObjectId> GetAllEntityIds(List<string> typeNameLst, string spaceName = "MODELSPACE")
        {
            //返回值
            List<ObjectId> objectIds = new List<ObjectId>();

            //先获取空间
            SpaceTool spaceTool = new SpaceTool(m_database);
            ObjectId spaceId = spaceTool.GetSpaceByName(spaceName);

            if (spaceId.IsNull)
            {
                return objectIds;
            }



            List<string> typeNameUpperLst = new List<string>();

            typeNameLst.ForEach(x => typeNameUpperLst.Add(x.ToUpper()));


            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {

                BlockTableRecord space = transaction.GetObject(spaceId, OpenMode.ForRead) as BlockTableRecord;


                // 遍历空间
                foreach (ObjectId id in space)
                {

                    string dxfName = id.ObjectClass.DxfName.ToUpper();

                    if (typeNameUpperLst.Contains(dxfName))
                    {

                        objectIds.Add(id);
                    }

                }
                transaction.Commit();
            }

            return objectIds;
        }

























        /// <summary>
        ///获取模型空间中对象的个数
        /// </summary>
        /// <returns></returns>
        public int GetObjectNumberInModelSpace()
        {
            //返回值
            int objectNumber = 0;
            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                // 模型空间
                BlockTable blkTbl = transaction.GetObject(m_database.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modelSpace = transaction.GetObject(
                    blkTbl[BlockTableRecord.ModelSpace], OpenMode.ForRead)
                    as BlockTableRecord;



                //既然可以遍历，应该可以直接有获取总数的方法，暂时没找到
                foreach (ObjectId objectId in modelSpace)
                {
                    objectNumber++;
                }
                transaction.Commit();
            }

            return objectNumber;

        }



        /// <summary>
        /// 判断模型空间是否含有对象
        /// </summary>
        /// <returns>如果有，返回true，否则，返回false</returns>
        public bool HasObjectsInModelSpace()
        {

            int objectNumber = GetObjectNumberInModelSpace();
            if (objectNumber == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }




        /// <summary>
        /// 显示模型空间中所有的对象类型和对应的数量
        /// </summary>
        public void GetObjectTypeAndNumberInModelSpace()
        {
            Dictionary<string, int> objectTypeAndNumberMap = new Dictionary<string, int>();

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {

                // 模型空间
                BlockTable blkTbl = transaction.GetObject(m_database.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modelSpace = transaction.GetObject(
                    blkTbl[BlockTableRecord.ModelSpace], OpenMode.ForRead)
                    as BlockTableRecord;



                // 遍历模型空间
                foreach (ObjectId id in modelSpace)
                {

                    string dxfName = id.ObjectClass.DxfName;

                    if (objectTypeAndNumberMap.ContainsKey(dxfName))
                    {
                        objectTypeAndNumberMap[dxfName]++;
                    }
                    else
                    {
                        objectTypeAndNumberMap[dxfName] = 1;
                    }

                }
                transaction.Commit();
            }

            string showMessage;
            if (objectTypeAndNumberMap.Count == 0)
            {
                showMessage = "模型空间没有对象";
            }
            else
            {
                showMessage = "模型空间的对象类型和数量:\n";
                foreach (KeyValuePair<string, int> kvp in objectTypeAndNumberMap)
                {
                    showMessage += kvp.Key + " : " + kvp.Value + "\n";

                }
            }

            System.Windows.Forms.MessageBox.Show(showMessage);

        }





        /// <summary>
        /// 获取某个空间的所有块参照的ObjectId
        /// </summary>
        /// <param name="spaceName">空间名称，默认模型空间</param>
        /// <returns>块参照的ObjectId列表，如果找不到，返回空的列表</returns>
        public List<ObjectId> GetAllBlockReferenceIdsInSpace(string spaceName = "MODELSPACE")
        {


            return GetAllEntityIds("INSERT", spaceName);



            //using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            //{
            //    // 模型空间
            //    BlockTable blkTbl = transaction.GetObject(m_database.BlockTableId, OpenMode.ForRead) as BlockTable;

            //    if (spaceName.ToUpper() == "PAPERSPACE")
            //    {
            //        spaceName = BlockTableRecord.PaperSpace;
            //    }
            //    else
            //    {
            //        spaceName = BlockTableRecord.ModelSpace;
            //    }

            //    BlockTableRecord space = transaction.GetObject(
            //        blkTbl[spaceName], OpenMode.ForRead)
            //        as BlockTableRecord;



            //    //用GetBlockReferenceIds没法获取，返回0个对象，不知道怎么用，改用别的方法

            //    ////不知道这两个参数的具体作用
            //    //return space.GetBlockReferenceIds(true, true); 


        }





        /// <summary>
        /// 从外部dwg文件导入已有的块  需要添加事务，不然会报错，
        /// 注意：导进来的块单位会保留原来dwg中的单位，比如源文
        /// 件的的单位为英尺，目标文件的单位为毫米，那么块的单位将为英尺
        /// </summary>
        /// <param name="destdb"></param>
        /// <param name="externalFileName"></param>
        /// <param name="blockName"></param>
        /// <returns></returns>
        public bool ImportBlockFromExternalFile(Database destdb, string externalFileName, string blockName)
        {

            //返回值
            bool isSucceed = false;

            ObjectIdCollection ids = new ObjectIdCollection();

            //需要添加事务
            using (Transaction transaction = destdb.TransactionManager.StartTransaction())
            {

                using (Database sourseDatabase = new Database(false, true))
                {
                    try
                    {
                        sourseDatabase.ReadDwgFile(externalFileName, FileShare.ReadWrite, true, "");
                        sourseDatabase.CloseInput(true);
                    }

                    catch
                    {
                        transaction.Commit();
                        return isSucceed;
                    }


                    using (Transaction tr = sourseDatabase.TransactionManager.StartTransaction())
                    {

                        BlockTable bt = (BlockTable)tr.GetObject(sourseDatabase.BlockTableId, OpenMode.ForRead);

                        if (bt.Has(blockName))
                        {
                            transaction.Commit();
                            ids.Add(bt[blockName]);
                        }
                        else //没有找到块 直接返回
                        {
                            transaction.Commit();
                            return isSucceed;
                        }

                    }





                    //上面没有返回，说明成功
                    if (ids.Count > 0)
                    {

                        IdMapping iMap = new IdMapping();


                        try
                        {
                            destdb.WblockCloneObjects(ids, destdb.BlockTableId, iMap, DuplicateRecordCloning.Ignore, false);
                            transaction.Commit();
                            isSucceed = true;
                        }
                        catch
                        {
                            transaction.Abort();
                        }

                    }
                    else
                    {
                        transaction.Commit();
                    }

                }


            }

            return isSucceed;

        }





        /// <summary>
        /// 通过样式名称获取对象的样式对象的ObjectId
        /// </summary>
        /// <param name="styleName">样式名称</param>
        /// <returns>样式对象的ObjectId，如果失败，返回ObjectId.Null</returns>
        public ObjectId GetTextStyleByName(string styleName)
        {
            //返回值
            ObjectId objectId = ObjectId.Null;

            if (string.IsNullOrEmpty(styleName))
            {
                return objectId;
            }

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                // 获取文字样式表
                TextStyleTable styleTable = transaction.GetObject(m_database.TextStyleTableId, OpenMode.ForRead) as TextStyleTable;

                if (styleTable.Has(styleName))
                {
                    // 获取指定名称的文字样式对象
                    objectId= styleTable[styleName];
                }

                transaction.Commit();

            }

            return objectId;
        }



        /// <summary>
        /// 通过多重引线样式名称获取其ObjectId
        /// </summary>
        /// <param name="mLeaderStyleName">样式名称</param>
        /// <returns>样式对象的ObjectId，如果失败，返回ObjectId.Null</returns>
        public ObjectId GetMLeaderStyleByName(string mLeaderStyleName)
        {
            //返回值
            ObjectId objectId = ObjectId.Null;

            if (string.IsNullOrEmpty(mLeaderStyleName))
            {
                return objectId;
            }

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                DBDictionary dBDictionary = transaction.GetObject(m_database.MLeaderStyleDictionaryId, OpenMode.ForRead) as DBDictionary;

                //已经存在，直接返回
                if (dBDictionary.Contains(mLeaderStyleName))
                {
                    objectId= dBDictionary.GetAt(mLeaderStyleName);
                }

                transaction.Commit();
            }

            return objectId;
        }













        /// <summary>
        /// 设置当前的文字样式
        /// </summary>
        /// <param name="textStyleId">文字样式对象的ObjectId</param>
        /// <returns>如果成功，返回true，否则，返回false</returns>
        public bool SetCurrentTestStyle(ObjectId textStyleId)
        {
            //返回值
            bool isSucceed = false;

            if (textStyleId.IsNull)
            {
                return isSucceed;
            }

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    m_database.Textstyle=textStyleId;
                    isSucceed = true;
                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();
                }

            }

            return isSucceed;

        }



        /// <summary>
        /// 设置当前的文字样式
        /// </summary>
        /// <param name="mLeaderStyleName">文字样式对象的名称</param>
        /// <returns>如果成功，返回true，否则，返回false</returns>
        public bool SetCurrentTextStyle(string styleName)
        {
            ObjectId textStyleId = GetTextStyleByName(styleName);
            return SetCurrentTestStyle(textStyleId);
        }








        /// <summary>
        /// 设置当前的多重引线样式
        /// </summary>
        /// <param name="mLeaserStyleId">多重引线样式对象的ObjectId</param>
        /// <returns>如果成功，返回true，否则，返回false</returns>
        public bool SetCurrentMLeaderStyle(ObjectId mLeaserStyleId)
        {
            //返回值
            bool isSucceed = false;

            if (mLeaserStyleId.IsNull)
            {
                return isSucceed;
            }

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    m_database.MLeaderstyle=mLeaserStyleId;
                    isSucceed = true;
                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();
                }

            }

            return isSucceed;

        }














        /// <summary>
        /// 获取当前默认的颜色
        /// </summary>
        /// <returns></returns>
        public Color GetCurrentColor()
        {

            return m_database.Cecolor;
        }



        /// <summary>
        /// 设置当前的颜色
        /// </summary>
        /// <param name="color">如果为null，则设置为随层</param>
        /// <returns></returns>
        public bool SetCurrentColor(Color color = null)
        {
            //返回值
            bool isSucceed = false;

            if (color==null)
            {
                color = Color.FromColorIndex(ColorMethod.ByLayer, 256);
            }


            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    m_database.Cecolor = color;
                    isSucceed = true;
                    transaction.Commit();

                }
                catch
                {
                    transaction.Abort();
                }
            }

            return isSucceed;
        }





        /// <summary>
        /// 获取当前的线形
        /// </summary>
        /// <returns>当前线形的ObjectId</returns>
        public ObjectId GetCurrentLineTyle()
        {
            return m_database.Celtype;
        }




        /// <summary>
        /// 设置当前的线形
        /// </summary>
        /// <param name="lineTypeId">如果为null，则设置为随层</param>
        /// <returns></returns>
        public bool SetCurrentLineTyle(ObjectId lineTypeId)
        {
            //返回值
            bool isSucceed = false;


            if (lineTypeId.IsNull)
            {
                return isSucceed;
            }


            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    m_database.Celtype = lineTypeId;
                    isSucceed = true;
                    transaction.Commit();

                }
                catch
                {
                    transaction.Abort();
                }
            }

            return isSucceed;
        }

















        /// <summary>
        /// 获取获取创建指定形式的字体样式对象
        /// </summary>
        /// <param name="mLeaderStyleName">样式名称</param>
        /// <param name="fontName">字体</param>
        /// <param name="bigFontName">大字体</param>
        /// <param name="scale">宽度因子</param>
        /// <returns>如果已经存在，直接返回已经存在的，不做任何修改，如果不存在，按指定样式创建一个新的样式并返回，如果失败，返回ObjectId.Null</returns>
        public ObjectId GetOrCreateTextStyle(string styleName, string fontName, string bigFontName, double scale)
        {
            //返回值
            ObjectId objectId = ObjectId.Null;


            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {

                try
                {
                    TextStyleTable ts = (TextStyleTable)transaction.GetObject(m_database.TextStyleTableId, OpenMode.ForRead);

                    //已经存在
                    if (ts.Has(styleName))
                    {
                        objectId=ts[styleName];
                    }
                    else //不存在，创建
                    {
                        ts.UpgradeOpen();

                        TextStyleTableRecord tstr = new TextStyleTableRecord
                        {
                            Name = styleName,
                            FileName = fontName,
                            BigFontFileName = bigFontName,
                            TextSize = 0,
                            XScale = scale
                        };

                        objectId=  ts.Add(tstr);
                        transaction.AddNewlyCreatedDBObject(tstr, true);
                        ts.DowngradeOpen();
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
        /// 获取与名称对应的箭头块的ObjectId
        /// </summary>
        /// <param name="arrowName">箭头名</param>
        /// <returns>指定箭头块的ObjectId,如果失败，返回ObjectId.Null</returns>
        public ObjectId GetArrowObjectId(string arrowName)
        {
            //返回值
            ObjectId result = ObjectId.Null;

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    BlockTable blockTable = (BlockTable)transaction.GetObject(m_database.BlockTableId, OpenMode.ForRead);
                    if (blockTable.Has(arrowName))
                    {
                        result = blockTable[arrowName];
                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();

                }

            }

            return result;
        }












    }

}



