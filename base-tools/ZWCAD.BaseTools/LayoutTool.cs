using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ZWCAD.BaseTools
{
    /// <summary>
    /// 
    /// </summary>
    public class LayoutTool
    {

        Document m_document;
        Database m_database;


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="document">文档对象</param>
        public LayoutTool(Document document)
        {
            m_document = document;
            m_database = m_document.Database;
        }


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="database">图形数据库</param>
        public LayoutTool(Database database)
        {
            m_database = database;
        }





        /// <summary>
        ///获取指定名称的布局对象
        /// </summary>
        /// <param name="layoutName">布局的名称</param>
        /// <returns>布局对象，如果失败，返回ObjectId.Null</returns>
        public ObjectId GetLayoutByName(string layoutName)
        {
            //返回值
            ObjectId layoutId = ObjectId.Null;

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                // First try to get the layout
                LayoutManager lm = LayoutManager.Current;

                var id = lm.GetLayoutId(layoutName);

                // If it doesn't exist, we create it

                if (id.IsValid) //已经存在
                {
                    layoutId = id;
                }

                transaction.Commit();
            }

            return layoutId;
        }

















        /// <summary>
        ///创建布局对象 （图纸空间可以当作一种布局）,如果已经存在，则返回已经存在的
        /// </summary>
        /// <param name="layoutName">布局的名称</param>
        /// <param name="select">是否设置为当前布局</param>
        /// <returns>布局对象，如果失败，返回ObjectId.Null</returns>
        public ObjectId CreateAndMakeLayoutCurrent(string layoutName, bool select = true)
        {
            //返回值
            ObjectId layoutId = ObjectId.Null;

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    // First try to get the layout
                    LayoutManager lm = LayoutManager.Current;

                    var id = lm.GetLayoutId(layoutName);

                    // If it doesn't exist, we create it

                    if (id.IsValid) //已经存在
                    {
                        layoutId = id;
                    }
                    else  //不存在，则创建
                    {
                        layoutId = lm.CreateLayout(layoutName);
                    }

                    // And finally we select it

                    if (select)
                    {
                        lm.CurrentLayout = layoutName;
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();

                }

            }

            return layoutId;

        }

        public bool setViewPortLayerAndOn(ObjectId layoutId, string layerName)
        {
            //返回值
            bool isSucceed = false;

            if (layoutId.IsNull)
            {
                return isSucceed; 
            }

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    Layout layout = transaction.GetObject(layoutId, OpenMode.ForRead) as Layout;
                    ObjectIdCollection idCollection = layout.GetViewports();
                    for (int i = 1; i < idCollection.Count; i++)
                    {
                        Viewport viewport = transaction.GetObject(idCollection[i], OpenMode.ForWrite) as Viewport;
                        viewport.On = false;
                        viewport.Layer = layerName;
                    }
                    transaction.Commit();
                    isSucceed = true;
                }
                catch(Exception ex)
                {
                    transaction.Abort();
                }
            }

            return isSucceed;
        }






        /// <summary>
        ///创建布局对象列表 ，如果指定名称已经存在，则保留原来的
        /// </summary>
        /// <param name="layoutNameLst">布局的名称列表</param>
        /// <returns>布局对象列表，如果失败，返回空的列表</returns>
        public List<ObjectId> CreateLayouts(List<string> layoutNameLst)
        {
            //返回值
            List<ObjectId> layoutIdLst = new List<ObjectId>();

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    // First try to get the layout
                    LayoutManager lm = LayoutManager.Current;

                    foreach (var layoutName in layoutNameLst)
                    {
                        var layoutId = lm.GetLayoutId(layoutName);

                        // If it doesn't exist, we create it

                        if (!layoutId.IsValid) //已经存在
                        {
                            layoutId = lm.CreateLayout(layoutName);
                        }

                    }

                    transaction.Commit();
                }
                catch
                {
                    layoutIdLst.Clear();
                    transaction.Abort();

                }

            }

            return layoutIdLst;

        }



        /// <summary>
        /// 获取所有布局的名称列表，排除掉模型空间，大小写不敏感
        /// </summary>
        /// <returns>布局的名称列表，如果失败，返回空的列表</returns>
        public List<string> GetAllLayoutNames()
        {
            //返回值
            List<string> layoutNameLst = new List<string>();

            List<string> excludeLayoutNameLst = new List<string>
            {
                "模型",
                "Model"
            };


            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    DBDictionary layoutDic = transaction.GetObject(m_database.LayoutDictionaryId, OpenMode.ForRead, false) as DBDictionary;

                    foreach (DBDictionaryEntry entry in layoutDic)
                    {
                        ObjectId layoutId = entry.Value;
                        Layout layout = transaction.GetObject(layoutId, OpenMode.ForRead) as Layout;

                        string layoutName = layout.LayoutName;

                        if (!excludeLayoutNameLst.Contains(layoutName))
                        {
                            layoutNameLst.Add(layoutName);
                        }
                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();

                }

            }

            return layoutNameLst;

        }





        /// <summary>
        ///删除指定布局
        /// </summary>
        /// <param name="layoutName">布局的名称列表</param>
        /// <returns>布局对象列表，如果失败，返回空的列表</returns>
        public bool EraseLayout(string layoutName)
        {
            //返回值
            bool isSucceed = false;

            // First try to get the layout
            LayoutManager lm = LayoutManager.Current;

            var id = lm.GetLayoutId(layoutName);

            if (!id.IsValid) //不存在
            {
                return isSucceed;
            }

            //以下为存在的情况

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    ////需要排除掉为模型空间的情况 这个情况不能删除
                    //Layout layout = transaction.GetObject(id, OpenMode.ForRead) as Layout;

                    //if (layout.IsWriteEnabled)
                    //{



                    //layout.UpgradeOpen();
                    //layout.Erase();

                    lm.DeleteLayout(layoutName);
                    isSucceed = true;
                    //}
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
        ///删除指定布局列表
        /// </summary>
        /// <param name="layoutIdLst">布局对象的ObjectId列表</param>
        public void EraseLayouts(List<ObjectId> layoutIdLst)
        {
            // First try to get the layout
            LayoutManager lm = LayoutManager.Current;

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    foreach (var objectId in layoutIdLst)
                    {

                        //需要排除掉为模型空间的情况 这个情况不能删除
                        if (transaction.GetObject(objectId, OpenMode.ForWrite) is Layout layout)
                        {
                            string layoutName = layout.LayoutName;

                            lm.DeleteLayout(layoutName);


                            //if (layout.IsWriteEnabled)
                            //{
                            //layout.UpgradeOpen();
                            //layout.Erase();
                            //}

                        }

                    }
                    transaction.Commit();

                }
                catch
                {
                    transaction.Abort();

                }

            }


        }




        /// <summary>
        /// 获取布局空间所有对象
        /// </summary>
        /// <param name="layoutName">布局对象名称</param>
        /// <returns>如果成功，返回实体ObjectId对象列表，否则，返回空列表对象</returns>
        public List<ObjectId> GetAllEntitiesInLayout(string layoutName)
        {
            //返回值
            List<ObjectId> entiyIdLst = new List<ObjectId>();

            ObjectId layoutId = GetLayoutByName(layoutName);

            if (layoutId == ObjectId.Null)
            {
                return entiyIdLst;
            }

            entiyIdLst = GetAllEntitiesInLayout(layoutId);


            return entiyIdLst;
        }



        /// <summary>
        /// 获取布局空间所有对象
        /// </summary>
        /// <param name="layoutId">布局对象的ObjectId</param>
        /// <returns>如果成功，返回实体ObjectId对象列表，否则，返回空列表对象</returns>
        public List<ObjectId> GetAllEntitiesInLayout(ObjectId layoutId)
        {
            //返回值
            List<ObjectId> entiyIdLst = new List<ObjectId>();

            if (layoutId.IsNull)
            {
                return entiyIdLst;
            }

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    if (transaction.GetObject(layoutId, OpenMode.ForRead) is Layout layout)
                    {
                        //获取布局所在的图纸空间
                        ObjectId spaceId = layout.BlockTableRecordId;

                        BlockTableRecord blockTableRecord = transaction.GetObject(spaceId, OpenMode.ForWrite) as BlockTableRecord;

                        foreach (ObjectId objId in blockTableRecord)
                        {
                            entiyIdLst.Add(objId);
                        }
                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();
                }
            }

            return entiyIdLst;
        }








        /// <summary>
        /// 删除布局对象所在的空间中的所有对象 会自动判断布局对象是否存在
        /// </summary>
        /// <param name="layoutName">布局对象的名称</param>
        /// <returns>如果成功，返回true，否则，返回false</returns>
        public bool EraseAllEntitiesInLayout(string layoutName)
        {
            //返回值
            bool isSucceed = false;

            ObjectId layoutId = GetLayoutByName(layoutName);

            if (layoutId == ObjectId.Null)
            {
                return isSucceed;
            }

            isSucceed= EraseAllEntitiesInLayout(layoutId);


            return isSucceed;
        }



        /// <summary>
        /// 删除布局对象所在的空间中的所有对象 需要排除掉默认生成的视口对象，不然显示会有问题
        /// </summary>
        /// <param name="layoutId">布局对象的ObjectId</param>
        /// <returns>如果成功，返回true，否则，返回false</returns>
        public bool EraseAllEntitiesInLayout(ObjectId layoutId)
        {
            //返回值
            bool isSucceed = false;

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {

                    //用来判断是否为第一个视口
                    bool isDefaultViewport = true;

                    if (transaction.GetObject(layoutId, OpenMode.ForRead) is Layout layout)
                    {
                        //获取布局所在的图纸空间
                        ObjectId spaceId = layout.BlockTableRecordId;

                        BlockTableRecord blockTableRecord = transaction.GetObject(spaceId, OpenMode.ForWrite) as BlockTableRecord;

                        foreach (ObjectId objId in blockTableRecord)
                        {
                            DBObject obj = transaction.GetObject(objId, OpenMode.ForWrite);


                            //第一个视口对象不删除
                            if(obj is Viewport && isDefaultViewport)
                            {
                                isDefaultViewport=false;
                                continue;
                            }


                            obj.Erase();
                        }
                    }

                    transaction.Commit();
                    isSucceed=true;
                }
                catch
                {
                    transaction.Abort();
                }

            }
            return isSucceed;

        }




        /// <summary>
        /// 通过布局对象名称获取其所在的空间对象
        /// </summary>
        /// <param name="layoutName">布局对象的名称</param>
        /// <returns>空间对象的ObjectId，如果失败，返回ObjectId.Null</returns>
        public ObjectId GetSpaceId(string layoutName)
        {
            ObjectId layoutId = GetLayoutByName(layoutName);
            return GetSpaceId(layoutId);
        }




        /// <summary>
        /// 获取布局对象所在的空间
        /// </summary>
        /// <param name="layoutId">布局对象的ObjectId</param>
        /// <returns>空间对象的ObjectId，如果失败，返回ObjectId.Null</returns>
        public ObjectId GetSpaceId(ObjectId layoutId)
        {
            //返回值
            ObjectId spaceId = ObjectId.Null;
            if (layoutId.IsNull)
            {
                return spaceId;
            }

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {

                if (transaction.GetObject(layoutId, OpenMode.ForRead) is Layout layout)
                {
                    //获取布局所在的图纸空间
                    spaceId = layout.BlockTableRecordId;

                }

                transaction.Commit();
            }

            return spaceId;
        }




        /// <summary>
        /// 获取空间对象的布局对象
        /// </summary>
        /// <param name="spaceId">空间对象的ObjectId</param>
        /// <returns>布局对象的ObjectId，如果失败，返回ObjectId.Null</returns>
        public ObjectId GetLayoutId(ObjectId spaceId)
        {
            //返回值
            ObjectId layoutId = ObjectId.Null;
            if (spaceId.IsNull)
            {
                return layoutId;
            }

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {

                if (transaction.GetObject(spaceId, OpenMode.ForRead) is BlockTableRecord blockTableRecord)
                {
                    //获取空间的布局
                    layoutId = blockTableRecord.LayoutId;

                }

                transaction.Commit();
            }

            return layoutId;
        }










        /// <summary>
        /// 获取所有的布局对象列表 不包括模型空间
        /// </summary>
        /// <returns>所有布局对象的ObjectId列表，如果没有，返回空的列表</returns>
        public List<ObjectId> GetAllLayoutsWithoutModelSpace()
        {
            //返回值
            List<ObjectId> layoutIdLst = new List<ObjectId>();

            List<string> excludeLayoutNameLst = new List<string>
            {
                "模型",
                "Model"
            };


            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    DBDictionary layoutDic = transaction.GetObject(m_database.LayoutDictionaryId, OpenMode.ForRead, false) as DBDictionary;

                    foreach (DBDictionaryEntry entry in layoutDic)
                    {
                        ObjectId layoutId = entry.Value;
                        Layout layout = transaction.GetObject(layoutId, OpenMode.ForRead) as Layout;



                        string layoutName = layout.LayoutName;

                        if (!excludeLayoutNameLst.Contains(layoutName))
                        {
                            layoutIdLst.Add(layoutId);
                        }

                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();

                }

            }

            return layoutIdLst;

        }



        /// <summary>
        /// 获取所有的布局对象列表包括模型空间
        /// </summary>
        /// <returns>所有布局对象的ObjectId列表，如果没有，返回空的列表</returns>
        public List<ObjectId> GetAllLayouts()
        {
            //返回值
            List<ObjectId> layoutIdLst = new List<ObjectId>();


            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    DBDictionary layoutDic = transaction.GetObject(m_database.LayoutDictionaryId, OpenMode.ForRead, false) as DBDictionary;

                    foreach (DBDictionaryEntry entry in layoutDic)
                    {
                        ObjectId layoutId = entry.Value;
                        Layout layout = transaction.GetObject(layoutId, OpenMode.ForRead) as Layout;

                        layoutIdLst.Add(layoutId);
                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();

                }

            }

            return layoutIdLst;

        }








        /// <summary>
        /// 删除所有布局对象 最后会自动创建一个新的布局对象
        /// </summary>
        public void EraseAllLayouts()
        {
            List<ObjectId> layoutIdLst = GetAllLayoutsWithoutModelSpace();
            if (layoutIdLst.Count == 0)
            {
                return;
            }

            EraseLayouts(layoutIdLst);

        }






        /// <summary>
        ///设置当前的布局对象
        /// </summary>
        /// <param name="layoutId">布局对象的ObjectId</param>
        /// <returns>如果成功，返回true，否则，返回false</returns>
        public bool SetCurrentLayout(ObjectId layoutId)
        {
            //返回值
            bool isSucceed = false;

            if (layoutId.IsNull)
            {
                return isSucceed;
            }

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {

                    if (transaction.GetObject(layoutId, OpenMode.ForRead) is Layout layout)
                    {
                        string layoutName = layout.LayoutName;
                        LayoutManager.Current.CurrentLayout=layoutName;
                        isSucceed=true;

                    }

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
        ///设置当前的布局对象
        /// </summary>
        /// <param name="layoutName">布局对象的名称</param>
        /// <returns>如果成功，返回true，否则，返回false</returns>
        public bool SetCurrentLayout(string layoutName)
        {
            ObjectId layoutId = GetLayoutByName(layoutName);

            //返回值
            bool isSucceed = SetCurrentLayout(layoutId);
            return isSucceed;

        }







        /// <summary>
        ///设置当前的布局对象
        /// </summary>
        /// <param name="spaceId">空间对象的ObjectId</param>
        /// <returns>如果成功，返回true，否则，返回false</returns>
        public bool SetCurrentLayoutBySpace(ObjectId spaceId)
        {
            ObjectId layoutId = GetLayoutId(spaceId);

            //返回值
            bool isSucceed = SetCurrentLayout(layoutId);
            return isSucceed;
        }




        /// <summary>
        ///获取当前的布局对象名称
        /// </summary>
        /// <returns>如果失败，返回null</returns>
        public string GetCurrentLayout()
        {
            string layoutName = null;
            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    layoutName = LayoutManager.Current.CurrentLayout;
                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();
                }

            }
            return layoutName;

        }





        /// <summary>
        /// 获取指定实体对象所在的布局对象
        /// </summary>
        /// <param name="entityId">实体对象的ObjectId</param>
        /// <returns>布局对象的ObjectId，如果失败，返回ObjectId.Null</returns>
        public ObjectId GetLayoutByEntity(ObjectId entityId)
        {
            //返回值
            ObjectId layoutId = ObjectId.Null;

            if (entityId.IsNull)
            {
                return layoutId;
            }

            Database database = entityId.Database;

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                try
                {
                    if (transaction.GetObject(entityId, OpenMode.ForRead) is Entity entity)
                    {
                        var spaceId = entity.BlockId;

                        var space = transaction.GetObject(spaceId, OpenMode.ForRead) as BlockTableRecord;

                        if (space.IsLayout)
                        {
                            layoutId = space.LayoutId;
                        }

                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();
                }
            }

            return layoutId;

        }





        /// <summary>
        /// 初始化指定的布局
        /// </summary>
        /// <param name="layoutId">布局对象的Id</param>
        /// <returns>如果成功，返回true，否则，返回false</returns>
        public bool SetLayoutInitialize(ObjectId layoutId)
        {
            bool isSucceed = SetCurrentLayout(layoutId);
            if (isSucceed)
            {
                using (Transaction trans = m_database.TransactionManager.StartTransaction())
                {
                    try
                    {
                        Layout layout = trans.GetObject(layoutId, OpenMode.ForRead) as Layout;
                        ObjectIdCollection idCollection = layout.GetViewports();
                        for (int i = 0; i < idCollection.Count; i++)
                        {
                            Viewport viewport = trans.GetObject(idCollection[i], OpenMode.ForWrite) as Viewport;
                            viewport.UpgradeOpen();
                            double scale = viewport.CustomScale;
                            viewport.CustomScale = scale;
                            //viewport.UpdateDisplay();
                            viewport.Locked = true;
                            viewport.On = true;
                        }
                        trans.Commit();
                    }
                    catch
                    {
                        isSucceed = false;
                        trans.Abort();
                    }
                }
                ViewTool.ZoomExtens();
            }
            return isSucceed;
        }



        /// <summary>
        /// 初始化指定的布局
        /// </summary>
        /// <param name="layoutName">布局对象的名称</param>
        /// <returns>如果成功，返回true，否则，返回false</returns>
        public bool SetLayoutInitialize(string layoutName)
        {
            ObjectId layoutId = GetLayoutByName(layoutName);

            //返回值
            bool isSucceed = SetLayoutInitialize(layoutId);
            
            return isSucceed;
        }


        /// <summary>
        /// 初始化所有布局
        /// </summary>
        public void SetAllLayoutsInitialize()
        {
            List<ObjectId> layoutLst = GetAllLayoutsWithoutModelSpace();

            layoutLst.ForEach(x => SetLayoutInitialize(x));
         
        }

    }
}

