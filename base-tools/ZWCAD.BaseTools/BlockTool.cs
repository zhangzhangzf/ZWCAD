using ZWCAD.BaseTools.Extension;
using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;
using ZwSoft.ZwCAD.Geometry;
using Mrf.CSharp.BaseTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Point3d = ZwSoft.ZwCAD.Geometry.Point3d;
using System.Linq;
using ZwSoft.ZwCAD.Runtime;

namespace ZWCAD.BaseTools
{
    /// <summary>
    /// 块对象工具
    /// </summary>
    public class BlockTool
    {
        Document m_document;

        Database m_database;


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="document">文档对象</param>
        public BlockTool(Document document)
        {
            m_document = document;
            m_database = m_document.Database;
        }


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="database">图形数据库</param>
        public BlockTool(Database database)
        {
            m_database = database;
        }


        /// <summary>
        /// 判断块表记录是否已经存在，块参照不一定存在
        /// </summary>
        /// <param name="btrName">块名</param>
        /// <returns>true or false</returns>
        public bool IsBlockExist(string btrName)
        {

            Database database = m_database;

            bool hasBlock = false;
            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForRead);

                //块表中存在且没有被删除
                if (bt.Has(btrName) && !bt[btrName].IsErased)
                {
                    hasBlock = true;
                }

                transaction.Commit();
            }
            return hasBlock;
        }




        /// <summary>
        /// 判断块表记录是否存在某个内嵌块
        /// </summary>
        /// <param name="blockTableRecordId">块的ObjectId</param>
        /// <param name="innerBlockName">内嵌的块名</param>
        /// <returns>true or false</returns>
        public bool IsBlockHasInnerBlock(ObjectId blockTableRecordId, string innerBlockName)
        {

            Database database = m_database;

            bool hasBlock = false;
            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                BlockTableRecord btr = (BlockTableRecord)transaction.GetObject(blockTableRecordId, OpenMode.ForRead);

                foreach (ObjectId objectId in btr)
                {

                    DBObject dBObject = transaction.GetObject(objectId, OpenMode.ForRead, true);

                    if (dBObject is BlockReference blockReference) //块
                    {
                        string name = blockReference.Name;
                        if (name.ToUpper() == innerBlockName.ToUpper())
                        {
                            hasBlock = true;
                            break;
                        }
                    }
                    transaction.Commit();
                }
            }
            return hasBlock;
        }


        /// <summary>
        /// 判断块表记录是否存在某个内嵌块
        /// </summary>
        /// <param name="outBlockName">外部的块名</param>
        /// <param name="innerBlockName">内嵌的块名</param>
        /// <returns>true or false</returns>
        public bool IsBlockHasInnerBlock(string outBlockName, string innerBlockName)
        {
            //返回值
            bool hasBlock = false;

            //先获取块
            ObjectId blockTableRecordId = FindBlockTableRecordIdByName(outBlockName);

            if (blockTableRecordId == ObjectId.Null) //不存在
            {
                return hasBlock;
            }


            return IsBlockHasInnerBlock(blockTableRecordId, innerBlockName);
        }





        /// <summary>
        /// 如果块名不存在，则返回原来的名称，如果块名已经存在，那么增加后缀_1、_2、_3...
        /// </summary>
        /// <param name="btrOldName">原来的块表记录名称</param>
        /// <returns>名称string</returns>
        public string FindNewBlockTableRecordName(string btrOldName)
        {

            string btrNewName = btrOldName;
            int i = 1;
            while (IsBlockExist(btrNewName)) //块表记录已经存在
            {
                btrNewName = btrOldName + "_" + i;
                i++;
            }

            return btrNewName;

        }












        /// <summary>
        /// 将dwg文件作为外部参照导入当前文件的当前空间
        /// </summary>
        /// <param name="path">外部dwg文件的绝对路径</param>
        /// <param name="pos">插入点的世界坐标</param>
        /// <param name="name">外部参照的名称，如果不指定，将默认使用其文件名称</param>
        /// <param name="overlay">参照类型，如果为true，则为覆盖型，否则，为附着型，默认为附着型</param>
        /// <returns>外部参照的OjectId，如果失败，返回ObjectId.Null</returns>
        public ObjectId XrefAttachAndInsert(string path, Point3d pos, string name = null, bool overlay = false)
        {

            //返回值
            ObjectId xrefId = ObjectId.Null;

            Database database = m_database;

            if (database == null)
            {
                database = m_document.Database;

            }


            if (!File.Exists(path))

                return xrefId;



            if (string.IsNullOrEmpty(name))

                name = Path.GetFileNameWithoutExtension(path);



            // We'll collect any xref definitions that need reloading after our

            // transaction (there should be at most one)


            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                try
                {
                    var xId =
                  overlay ? database.OverlayXref(path, name) : database.AttachXref(path, name);

                    if (xId.IsValid)
                    {

                        // Open the newly created block, so we can get its units
                        var xbtr = (BlockTableRecord)transaction.GetObject(xId, OpenMode.ForRead);

                        // Determine the unit conversion between the xref and the target

                        // database
                        var sf = UnitsConverter.GetConversionFactor(xbtr.Units, database.Insunits);

                        // Create the block reference and scale it accordingly

                        var br = new BlockReference(pos, xId);

                        br.ScaleFactors = new Scale3d(sf);



                        // Add the block reference to the current space and the transaction



                        var btr = (BlockTableRecord)transaction.GetObject(database.CurrentSpaceId, OpenMode.ForWrite);

                        xrefId = btr.AppendEntity(br);

                        transaction.AddNewlyCreatedDBObject(br, true);

                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();

                }
            }

            return xrefId;

        }
















        /// <summary>
        /// 判断块表记录是否含有属性
        /// </summary>
        /// <param name="recordId">块表记录的ObjectId</param>
        /// <returns></returns>
        public bool IsBlockTableRecordHasAttribute(ObjectId recordId)
        {
            //返回值
            bool isSucceed = false;

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {

                //这个地方 有可能会报错 ，比如对象在视图中不可见 或一些匿名的块，原因未知
                try
                {
                    BlockTableRecord record = (BlockTableRecord)transaction.GetObject(recordId, OpenMode.ForRead);
                    isSucceed = record.HasAttributeDefinitions;
                }
                catch //报错
                {
                }

                transaction.Commit();
            }

            return isSucceed;
        }






        /// <summary>
        /// 判断块参照是否含有属性
        /// </summary>
        /// <param name="blockReferencId">块参照的ObjectId</param>
        /// <returns></returns>
        public bool IsBlockReferenceHasAttribute(ObjectId blockReferencId)
        {

            ObjectTool objectTool;

            if (m_database != null)
            {
                objectTool = new ObjectTool(m_database);
            }

            else
            {
                objectTool = new ObjectTool(m_document);
            }


            BlockReference blockReference = objectTool.GetObject(blockReferencId) as BlockReference;

            ObjectId recordId = blockReference.BlockTableRecord;
            return IsBlockTableRecordHasAttribute(recordId);


        }





        /// <summary>
        /// 从指定空间中获取所有含有特定属性标记的块对象
        /// </summary>
        /// <param name="tagName">属性标记名称</param>
        /// <param name="spaceName">模型空间还是图纸空间名称，不分大小写，默认模型空间</param>
        /// <returns>如果没有找到对象，List的长度为0</returns>
        public List<ObjectId> FindBlockReferencsByAttributeTag(string tagName, string spaceName = "MODELSPACE")
        {

            Database database = m_database;

            DatabaseTool databaseTool = new DatabaseTool(database);

            //获取模型空间所有的块
            List<ObjectId> frameObjectIdLst = databaseTool.GetAllEntityIds("INSERT", spaceName);

            if (frameObjectIdLst.Count == 0) //没有找到，直接返回
            {
                return frameObjectIdLst;
            }

            //把没有指定属性的图块移除
            frameObjectIdLst.RemoveAll(a => !IsBlockReferenceHasAttribute(a, tagName));

            return frameObjectIdLst;
        }




        /// <summary>
        /// 从指定空间中获取所有块名以指定字符串开头的块对象
        /// </summary>
        /// <param name="blockNameStartWith">块名前缀</param>
        /// <param name="spaceName">模型空间还是图纸空间名称，不分大小写，默认模型空间</param>
        /// <returns>如果没有找到对象，List的长度为0</returns>
        public List<ObjectId> FindBlockReferencsByNameStartWith(string blockNameStartWith, string spaceName = "MODELSPACE")
        {

            Database database = m_database;

            DatabaseTool databaseTool = new DatabaseTool(database);

            //获取模型空间所有的块
            List<ObjectId> frameObjectIdLst = databaseTool.GetAllEntityIds("INSERT", spaceName);

            if (frameObjectIdLst.Count == 0) //没有找到，直接返回
            {
                return frameObjectIdLst;
            }



            //把没有指定属性的图块移除
            frameObjectIdLst.RemoveAll(a => string.IsNullOrEmpty(GetBlockReferenceName(a)) || !GetBlockReferenceName(a).StartsWith(blockNameStartWith));


            return frameObjectIdLst;
        }





        /// <summary>
        /// 获取块参照的角度，弧度制
        /// </summary>
        /// <param name="blockReference">块参照对象</param>
        /// <returns>角度的弧度制double</returns>
        public double GetRotation(BlockReference blockReference)
        {
            return blockReference.Rotation;
        }




        /// <summary>
        /// 获取块参照的角度，弧度制
        /// </summary>
        /// <param name="objectId">图元对象的ObjectId</param>
        /// <returns>角度的弧度制double,如果不为块参照，返回double.NaN</returns>
        public double GetRotation(ObjectId objectId)
        {



            ObjectTool objectTool;

            if (m_database != null)
            {
                objectTool = new ObjectTool(m_database);
            }

            else
            {
                objectTool = new ObjectTool(m_document);
            }

            DBObject dBObject = objectTool.GetObject(objectId);
            if (dBObject is BlockReference blockReference)
            {
                return GetRotation(blockReference);
            }

            return double.NaN;
        }


        /// <summary>
        /// 获取块参照的比例,获取x方向的缩放比例
        /// </summary>
        /// <param name="objectId">图元对象的ObjectId</param>
        /// <returns>比例值,如果不为块参照，返回double.NaN</returns>
        public double GetScale(ObjectId objectId)
        {

            ObjectTool objectTool;

            if (m_database != null)
            {
                objectTool = new ObjectTool(m_database);
            }

            else
            {
                objectTool = new ObjectTool(m_document);
            }

            DBObject dBObject = objectTool.GetObject(objectId);
            if (dBObject is BlockReference blockReference)
            {
                return GetScale(blockReference);
            }

            return double.NaN;
        }





        /// <summary>
        /// 设置块的比例因子，判断是否为块参照
        /// </summary>
        /// <param name="blockRerefenceId">块参照的ObjectId</param>
        /// <param name="xScale">x方向比例因子</param>
        /// <param name="yScale">y方向比例因子</param>
        /// <param name="zScale">z方向比例因子</param>
        /// <returns>如果设置成功，返回true，如果不是块参照，返回false</returns>
        public bool SetScale(ObjectId blockRerefenceId, double xScale, double yScale, double zScale = 1)
        {
            //返回值
            bool isSucceed = false;

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                if (transaction.GetObject(blockRerefenceId, OpenMode.ForRead, true) is BlockReference br)
                {
                    br.UpgradeOpen();
                    Scale3d scale3D = new Scale3d(xScale, yScale, zScale);
                    br.ScaleFactors = scale3D;
                    isSucceed = true;
                }

                transaction.Commit();
            }

            return isSucceed;

        }







        /// <summary>
        /// 设置块的比例因子，判断是否为块参照
        /// </summary>
        /// <param name="blockRerefenceId">块参照的ObjectId</param>
        /// <param name="scale">比例因子</param>
        /// <returns>如果设置成功，返回true，如果不是块参照，返回false</returns>
        public bool SetScale(ObjectId blockRerefenceId, double scale)
        {
            return SetScale(blockRerefenceId, scale, scale);
        }






        /// <summary>
        /// 设置块的比例因子，判断是否为块参照
        /// </summary>
        /// <param name="blockReference">块参照图元对象</param>
        /// <param name="scale">比例因子</param>
        /// <returns>如果设置成功，返回true，如果不是块参照，返回false</returns>
        public bool SetScale(BlockReference blockReference, double scale)
        {
            return SetScale(blockReference.Id, scale);
        }




        /// <summary>
        /// 获取块参照的比例,获取x方向的缩放比例
        /// </summary>
        /// <param name="blockReference">块参照</param>
        /// <returns>比例值</returns>
        public double GetScale(BlockReference blockReference)
        {
            return blockReference.ScaleFactors.X;
        }



        /// <summary>
        /// 修改块参照的角度，弧度制
        /// </summary>
        /// <param name="blockReference">块参照</param>
        /// <param name="rotation">旋转角度，弧度制</param>
        /// <returns>如果修改成功，返回true，否则，返回false</returns>
        public bool SetRotation(BlockReference blockReference, double rotation)
        {
            return SetRotation(blockReference.Id, rotation);
        }



        /// <summary>
        /// 修改块参照的角度，弧度制
        /// </summary>
        /// <param name="blockRerefenceId">块参照的ObjectId</param>
        /// <param name="rotation">旋转角度，弧度制</param>
        /// <returns>如果修改成功，返回true，否则，返回false</returns>
        public bool SetRotation(ObjectId blockRerefenceId, double rotation)
        {
            //返回值
            bool isSucceed = false;

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {

                if (transaction.GetObject(blockRerefenceId, OpenMode.ForRead, true) is BlockReference br)
                {
                    br.UpgradeOpen();
                    br.Rotation = rotation;
                    isSucceed = true;
                }

                transaction.Commit();

                return isSucceed;
            }
        }





        /// <summary>
        /// 通过块参照的ObjectId查找其所在的块表记录的名称
        /// </summary>
        /// <param name="blockReferenceId"></param>
        /// <returns></returns>
        public string FindBlockTableRecordName(ObjectId blockReferenceId)
        {




            ObjectTool objectTool;

            if (m_database != null)
            {
                objectTool = new ObjectTool(m_database);
            }

            else
            {
                objectTool = new ObjectTool(m_document);
            }




            BlockReference blockReference = objectTool.GetObject(blockReferenceId) as BlockReference;

            string recordName = blockReference.Name;

            return recordName;
        }




        /// <summary>
        /// 通过块表记录名称查找其ObjectId，如果存在，返回OjbectId,否则，返回ObjectId.Null
        /// </summary>
        /// <param name="btrName">块名</param>
        /// <returns>ObjectId or  ObjectId.Null</returns>
        public ObjectId FindBlockTableRecordIdByName(string btrName)
        {
            //返回值
            ObjectId btrId = ObjectId.Null;

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {

                BlockTable bt = (BlockTable)transaction.GetObject(m_database.BlockTableId, OpenMode.ForRead);
                if (bt.Has(btrName))
                {
                    btrId = bt[btrName];
                }
                transaction.Commit();
            }

            return btrId;
        }



        /// <summary>
        /// 创建块对象，不判断块是否已经存在
        /// </summary>
        /// <param name="btrName">块名</param>
        /// <param name="ents">组成块的对象</param>
        /// <returns>块的ObjectId</returns>
        public ObjectId AddRecordToBlockTable(string btrName, List<Entity> ents)
        {
            ObjectId btrId = ObjectId.Null;

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {

                BlockTable bt = (BlockTable)transaction.GetObject(m_database.BlockTableId, OpenMode.ForWrite);


                BlockTableRecord btr = new BlockTableRecord
                {
                    Name = btrName
                };

                foreach (Entity entity in ents)
                {
                    btr.AppendEntity(entity);
                }

                btrId = bt.Add(btr);
                transaction.AddNewlyCreatedDBObject(btr, true);

                transaction.Commit();
            }

            return btrId;

        }







        /// <summary>
        /// 将块表记录添加到块表中，不判断块表记录是否已经存在
        /// </summary>
        /// <param name="record">块表记录</param>
        /// <returns>块表记录的ObjectId，如果失败，返回ObjectId.Null</returns>

        public ObjectId AddRecordToBlockTable(BlockTableRecord record)
        {
            ObjectId btrId = ObjectId.Null;

            Database database = m_database;

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {

                BlockTable bt = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForWrite);


                btrId = bt.Add(record);
                transaction.AddNewlyCreatedDBObject(record, true);

                transaction.Commit();
            }

            return btrId;

        }






        /// <summary>
        /// 创建一个块表记录 不判断块表是否存在
        /// </summary>
        /// <param name="btrName">块表记录名称</param>
        /// <param name="ents">组成块表记录的对象列表</param>
        /// <returns>块表记录BlockTableRecord</returns>
        public BlockTableRecord CreateBlockTableRecord(string btrName, List<Entity> ents)
        {
            BlockTableRecord btr = new BlockTableRecord
            {
                Name = btrName
            };

            foreach (Entity entity in ents)
            {
                btr.AppendEntity(entity);
            }

            return btr;
        }









        /// <summary>
        /// 添加块表记录，如果已经存在，直接返回
        /// </summary> 
        /// <param name="btrName">块表记录名称</param>
        /// <param name="ents">组成块表记录的对象</param>
        /// <returns>块表记录的ObjectId</returns>
        public ObjectId FindOrAddBlockTableRecord(string btrName, List<Entity> ents)
        {
            //找有没有块
            ObjectId btrId = FindBlockTableRecordIdByName(btrName);

            //如果没有，再创建
            if (btrId.IsNull)
            {

                //创建块表记录
                BlockTableRecord record = CreateBlockTableRecord(btrName, ents);

                //将块表记录添加到块表中
                btrId = AddRecordToBlockTable(record);

            }

            return btrId;
        }



        /// <summary>
        /// 添加块表记录，如果已经存在，直接返回
        /// </summary> 
        /// <param name="btrName">块表记录名称</param>
        /// <param name="entityIdLst">组成块表记录的对象的ObjectId列表</param>
        /// <param name="isRemain">是否保留原来的对象，默认不保留</param>
        /// <returns>块表记录的ObjectId</returns>
        public ObjectId FindOrAddBlockTableRecord(string btrName, List<ObjectId> entityIdLst, bool isRemain = false)
        {
            //找有没有块
            ObjectId btrId = FindBlockTableRecordIdByName(btrName);

            //如果没有，再创建
            if (btrId.IsNull)
            {
                BlockTableRecord btr = new BlockTableRecord
                {
                    Name = btrName
                };

                //将块表记录添加到块表中
                btrId = AddRecordToBlockTable(btr);


                List<ObjectId> toBeCopiedEntityIdLst = entityIdLst.ToList();

                if (isRemain) //需要保留原来的对象列表，那么需要复制成新的对象列表
                {
                    ObjectTool objectTool = new ObjectTool(m_database);
                    toBeCopiedEntityIdLst = objectTool.CopyEntities(toBeCopiedEntityIdLst, Point3d.Origin, Point3d.Origin);
                }


                MoveObjectsToBlock(btrId, toBeCopiedEntityIdLst);



            }

            return btrId;
        }



        /// <summary>
        /// 添加块参照到模型空间中
        /// </summary>
        /// <param name="recordId">块参照所在的块表记录的ObjectId</param>
        /// <param name="insertPoint">插入点</param>
        /// <param name="scale">块的比例，默认1</param>
        /// <param name="spaceName">模型空间或图纸空间，如果为"PAPERSPACE"（不分大小写），为图纸空间，如果指定的名称为其它的图纸空间名称，则为指定的图纸空间名称；为空或其它，为模型空间</param>
        /// <returns>返回新添加的块参照的ObjectId,如果失败，返回ObjectId.Null</returns>

        public ObjectId InsertBlockReference(ObjectId recordId, Point3d insertPoint, double scale = 1, string spaceName = null)
        {
            //返回值
            ObjectId blockReferenceId = ObjectId.Null;


            //先获取空间
            SpaceTool spaceTool = new SpaceTool(m_database);
            ObjectId spaceId = spaceTool.GetSpaceByName(spaceName);

            if (spaceId.IsNull)
            {
                return blockReferenceId;
            }


            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {

                //模型空间块表
                BlockTableRecord space = transaction.GetObject(spaceId, OpenMode.ForWrite) as BlockTableRecord;


                //通过块定义添加块参照
                BlockReference blockReference = new BlockReference(insertPoint, recordId);


                //添加比例信息
                if (scale != 1)
                {
                    Scale3d scale3D = new Scale3d(scale);
                    blockReference.ScaleFactors = scale3D;
                }


                //把块参照添加到模型空间块表记录
                blockReferenceId = space.AppendEntity(blockReference);

                //如果块中有属性，需要如下处理

                //通过ObjectId获取相关的块表记录
                BlockTableRecord record = transaction.GetObject(recordId, OpenMode.ForRead) as BlockTableRecord;

                //先判断是否为属性块，节省计算量
                if (record.HasAttributeDefinitions)
                {


                    foreach (ObjectId id in record)
                    {
                        //if (id.ObjectClass.Equals(RXClass.GetClass(typeof(AttributeDefinition))))
                        //{
                        //    AttributeDefinition ad = transaction.GetObject(id, OpenMode.ForRead) as AttributeDefinition;


                        //    //注意需要更改属性的位置
                        //    Point3d attributeInsertPoint = new Point3d(ad.Position.X + insertPoint.X, ad.Position.Y + insertPoint.Y, ad.Position.Z + insertPoint.Z);


                        //    //创建属性参照
                        //    AttributeReference ar = new AttributeReference(attributeInsertPoint, ad.TextString, ad.Tag, new ObjectId());

                        //    blockReference.AttributeCollection.AppendAttribute(ar);

                        //} 

                        DBObject obj = id.GetObject(OpenMode.ForRead);

                        if (obj is AttributeDefinition definition)
                        {
                            AttributeDefinition ad = transaction.GetObject(id, OpenMode.ForRead) as AttributeDefinition;


                            //注意需要更改属性的位置
                            //Point3d attributeInsertPoint = new Point3d(ad.Position.X + insertPoint.X, ad.Position.Y + insertPoint.Y, ad.Position.Z + insertPoint.Z);


                            //创建属性参照
                            AttributeReference attrRef = new AttributeReference();
                            attrRef.SetAttributeFromBlock(definition, blockReference.BlockTransform);
                            blockReference.AttributeCollection.AppendAttribute(attrRef);

                            //别忘了这个
                            transaction.AddNewlyCreatedDBObject(attrRef, true);

                        }
                    }
                }

                //通过事务添加块参照到数据库
                transaction.AddNewlyCreatedDBObject(blockReference, true);


                transaction.Commit();

            }

            return blockReferenceId;
        }















        /// <summary>
        /// 添加对象列表到已有块表记录中
        /// </summary>
        /// <param name="recordId">块表记录的ObjectId</param>
        /// <param name="entities">对象列表</param>
        public void InsertEntitysToExistedBlockTableRecord(ObjectId recordId, List<Entity> entities)
        {

            Database database = m_database;

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                //块表
                BlockTable bt = transaction.GetObject(database.BlockTableId, OpenMode.ForRead) as BlockTable;

                //块表记录
                BlockTableRecord record = transaction.GetObject(recordId, OpenMode.ForWrite) as BlockTableRecord;

                //往块表记录中和数据库中添加实体对象
                foreach (Entity entity in entities)
                {
                    record.AppendEntity(entity);
                    transaction.AddNewlyCreatedDBObject(entity, true);


                }

                transaction.Commit();

            }

        }
































        //public BlockReference InsertBlockReferenceByUser(string btrName)
        //{
        //    using (Transaction transaction = m_document.TransactionManager.StartTransaction())
        //    {
        //        Database database = m_document.Database;
        //        BlockTable bt = transaction.GetObject(database.BlockTableId, OpenMode.ForRead) as BlockTable;
        //        BlockTableRecord modelSpace = transaction.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

        //        //查找块的ObjectId，假设已经存在
        //        ObjectId btrId = FindBlockByName(btrName);

        //        //通过块定义创建块参照
        //        BlockReference blockReference = new BlockReference(Point3d.Origin, btrId);

        //        modelSpace.AppendEntity(blockReference); //把块参照添加到块表记录

        //        transaction.AddNewlyCreatedDBObject(blockReference, true); //通过事务添加块参照到数据库

        //        transaction.Commit();

        //        return blockReference;

        //    }
        //}




        /// <summary>
        /// 添加块参照到模型空间
        /// </summary>
        /// <param name="btrName">块参照所在块表的名称</param>
        /// <param name="ents">组成块表的对象列表</param>
        /// <param name="insertPoint">块参照的插入点</param>
        /// <param name="scale">图块比例</param>
        /// <returns></returns>
        public ObjectId InsertBlockReferenceToModelSpace(string btrName, List<Entity> ents, Point3d insertPoint, double scale = 1)
        {

            //先获取或添加块表记录
            ObjectId recordId = FindOrAddBlockTableRecord(btrName, ents);

            //再模型空间中添加块参照

            ObjectId blockReferenceId = InsertBlockReference(recordId, insertPoint, scale);

            return blockReferenceId;

        }






        /// <summary>
        /// 在模型空间或图纸空间以已存在块名添加块参照，默认在模型空间中添加
        /// </summary>
        /// <param name="btrName">块参照名称</param>
        /// <param name="insertPoint">插入点</param>
        /// <param name="scale">图块比例</param>
        /// <param name="spaceName">模型空间或图纸空间，如果为"PAPERSPACE"（不分大小写），为图纸空间，为空或其它，为模型空间</param>
        /// <returns>如果找到块名，添加块参照，并返回ObjectId，否则，返回ObjectId.Null</returns>
        public ObjectId InsertBlockReference(string btrName, Point3d insertPoint, double scale = 1, string spaceName = null)
        {

            //先找有没有块表记录
            ObjectId btrId = FindBlockTableRecordIdByName(btrName);

            //如果没有找到，直接退出
            if (btrId.IsNull)
            {
                return ObjectId.Null;
            }
            //如果找到，添加块参照

            ObjectId blockReferenceId = InsertBlockReference(btrId, insertPoint, scale, spaceName);

            return blockReferenceId;

        }




        /// <summary>
        /// 获取块的插入点
        /// </summary>
        /// <param name="blockReference">块参照对象</param>
        /// <returns>插入点Point3d</returns>
        public Point3d GetPosition(BlockReference blockReference)
        {
            return blockReference.Position;
        }









        /// <summary>
        /// 获取块的插入点，不判断是否为块
        /// </summary>
        /// <param name="blockReferenceId">块参照的ObjectId</param>
        /// <returns>插入点Point3d</returns>
        public Point3d GetPosition(ObjectId blockReferenceId)
        {

            //using (Transaction transaction = m_document.TransactionManager.StartTransaction())
            //{ 

            using (Transaction transaction = blockReferenceId.Database.TransactionManager.StartTransaction())
            {
                BlockReference blockReference = transaction.GetObject(blockReferenceId, OpenMode.ForRead) as BlockReference;

                return GetPosition(blockReference);
            }

        }





        /// <summary>
        /// 修改块的插入点,用这个方法的缺点：如果同步了块，属性还是会回到原来的地方，没有从根本上解决问题
        /// </summary>
        /// <param name="blockReferenceId">块参照的ObjectId</param>
        /// <param name="insertPoint">插入点</param>
        public void SetPosition(ObjectId blockReferenceId, Point3d insertPoint)
        {

            using (Transaction transaction = blockReferenceId.Database.TransactionManager.StartTransaction())
            {
                BlockReference blockReference = transaction.GetObject(blockReferenceId, OpenMode.ForWrite) as BlockReference;

                //块参照修改前插入点
                Point3d oldBlockReferencePoint = blockReference.Position;


                blockReference.Position = insertPoint;


                //BlockTableRecord block = transaction.GetObject(blockReference.BlockTableRecord, OpenMode.ForWrite) as BlockTableRecord; //根据块表记录编号获取的块表记录, 用于取得它的属性定义

                //Matrix3d mtr = Matrix3d.Displacement(block.Origin.GetVectorTo(insertPoint)); //Displacement 取代


                //PointTool pointTool = new PointTool();



                //foreach (ObjectId objectId in blockReference.AttributeCollection)
                //{


                //    AttributeReference attributeReference = transaction.GetObject(objectId, OpenMode.ForWrite) as AttributeReference;

                //    Point3d oldAttributeReferencePoint = attributeReference.Position;

                //    Point3d diff = pointTool.Subtract(oldBlockReferencePoint, oldAttributeReferencePoint);
                //    Point3d newAttributeReferencePoint = pointTool.Subtract(insertPoint, diff);


                //    attributeReference.Position = newAttributeReferencePoint;


                //    //attributeReference.Position = attributeReference.Position.TransformBy(mtr);


                //    attributeReference.AdjustAlignment(blockReferenceId.Database);


                //}

                transaction.Commit(); //提交事务


            }

        }























        /// <summary>
        /// 将图块的左下角点移动到插入点上
        /// </summary>
        /// <param name="blockReferenceId">图块的ObjectId</param>
        public bool LeftBottonPointToPosition(ObjectId blockReferenceId)
        {
            //返回值
            bool isSucceed = false;

            Database database = m_database;


            ObjectTool objectTool = new ObjectTool(database);
            Point3d? leftBottonPointOrNull = objectTool.GetEntityBoundingBoxPoint(blockReferenceId, 0);

            if (leftBottonPointOrNull == null) //读取有误
            {
                return isSucceed;
            }

            Point3d leftBottonPoint = (Point3d)leftBottonPointOrNull;

            Point3d position = GetPosition(blockReferenceId);

            Point3d newInsertPoint = new Point3d(2 * position.X - leftBottonPoint.X, 2 * position.Y - leftBottonPoint.Y, 2 * position.Z - leftBottonPoint.Z);
            SetPosition(blockReferenceId, newInsertPoint);

            isSucceed = true;
            return isSucceed;
        }








        /// <summary>
        /// 获取块表记录的名称
        /// </summary>
        /// <param name="recordId">块表记录的ObjectId</param>
        /// <returns>名称string,如果有错误，返回null</returns>
        public string GetRecordName(ObjectId recordId)
        {
            //返回值
            string recordName = null;

            //using (Transaction transaction = m_document.TransactionManager.StartTransaction())
            //{
            using (Transaction transaction = recordId.Database.TransactionManager.StartTransaction())
            {
                if (transaction.GetObject(recordId, OpenMode.ForRead) is BlockTableRecord blockTableRecord)
                {
                    recordName = blockTableRecord.Name;
                }
                transaction.Commit();
            }
            return recordName;
        }








        /// <summary>
        /// 获取块参照的名称
        /// </summary>
        /// <param name="blockReferenceId">块参照的ObjectId</param>
        /// <returns>名称string,如果有错误，返回null</returns>
        public string GetBlockReferenceName(ObjectId blockReferenceId)
        {
            //返回值
            string name = null;

            try
            {

                //这里曾经报错过：Unhandled Access Violation Reading 0x0000 Exception at f23ee74bh
                using (Transaction transaction = blockReferenceId.Database.TransactionManager.StartTransaction())
                {
                    if (transaction.GetObject(blockReferenceId, OpenMode.ForRead) is BlockReference blockReference)
                    {
                        name = blockReference.Name;
                    }
                    transaction.Commit();
                }
            }

            catch
            {

            }
            return name;
        }







        /// <summary>
        /// 将块参照中的所有属性值设置为""
        /// </summary>
        /// <param name="blockReferenceId">块参照的ObjectId</param>
        public void SetBlockAttributesEmpty(ObjectId blockReferenceId)
        {
            Dictionary<string, List<string>> attrNameValues = GetBlockAttributes(blockReferenceId);
            if (attrNameValues.Count != 0)
            {

                Dictionary<string, string> newAttrNameValues = new Dictionary<string, string>();

                foreach (string key in attrNameValues.Keys)
                {
                    newAttrNameValues[key] = "";
                }

                ChangeBlockAttributes(blockReferenceId, newAttrNameValues);

            }
        }









        /// <summary>
        /// 修改块参照的属性列表
        /// </summary>
        /// <param name="blockReferenceId">块参照的ObjectId</param>
        /// <param name="attrNameValues">属性字典</param>
        public void ChangeBlockAttributes(ObjectId blockReferenceId, Dictionary<string, string> attrNameValues)
        {
            if (blockReferenceId.IsNull)
            {
                return;
            }
            using (Transaction transaction = blockReferenceId.Database.TransactionManager.StartTransaction())
            {

                if (blockReferenceId.GetObject(OpenMode.ForRead) is BlockReference blockReference)
                {

                    foreach (ObjectId objectId in blockReference.AttributeCollection)
                    {


                        //7:53 2023/12/24 在这里，第三个参数为：openErased 因为这个情况有些属性已经被删除，需要设置为true才能打开，不然会报错：eWasErased
                        //AttributeReference attrRef = objectId.GetObject(OpenMode.ForRead) as AttributeReference;
                        AttributeReference attrRef = objectId.GetObject(OpenMode.ForRead, true, true) as AttributeReference;

                        if (attrRef.IsErased)
                        {
                            continue;
                        }



                        //标记名称
                        string tag = attrRef.Tag;

                        //判断属性字典中是否包含要更改的属性值
                        if (attrNameValues.ContainsKey(tag))
                        {
                            //升级为可写
                            attrRef.UpgradeOpen();

                            attrRef.TextString = attrNameValues[tag];


                            //降级为可读
                            attrRef.DowngradeOpen();
                        }
                    }

                }


                transaction.Commit();
            }
        }





        /// <summary>
        /// 修改块参照的属性
        /// </summary>
        /// <param name="blockReferenceId">块参照的ObjectId</param>
        /// <param name="tagName">属性标记名称</param>
        /// <param name="tagValue">属性值</param>
        /// <param name="height">属性文字高度，默认不修改</param>
        /// <param name="widthFactor">属性文字宽度因子，默认不修改</param>
        /// <returns>如果修改成功，返回true，否则，返回false（比如不存在这个属性）</returns>
        public bool ChangeBlockAttribute(ObjectId blockReferenceId, string tagName, string tagValue, double height = double.NaN, double widthFactor = double.NaN)
        {
            //返回值
            bool isSucceed = false;

            if (blockReferenceId.IsNull)
            {
                return isSucceed;
            }
            using (Transaction transaction = blockReferenceId.Database.TransactionManager.StartTransaction())
            {

                BlockReference blockReference = blockReferenceId.GetObject(OpenMode.ForRead) as BlockReference;

                foreach (ObjectId objectId in blockReference.AttributeCollection)
                {

                    //7:53 2023/12/24 在这里，第三个参数为：openErased 因为这个情况有些属性已经被删除，需要设置为true才能打开，不然会报错：eWasErased
                    //AttributeReference attrRef = objectId.GetObject(OpenMode.ForRead) as AttributeReference;

                    AttributeReference attrRef = objectId.GetObject(OpenMode.ForRead, true, true) as AttributeReference;

                    // 有时候会报错：eWasErased 可能是没加这个判断
                    if (attrRef.IsErased)
                    {
                        continue;
                    }


                    //标记名称
                    string tag = attrRef.Tag;

                    //如果找到有这个属性标记
                    if (tag == tagName)
                    {
                        //升级为可写
                        attrRef.UpgradeOpen();




                        attrRef.TextString = tagValue;


                        //修改属性文字高度
                        if (!double.IsNaN(height))
                        {
                            attrRef.Height = height;
                        }

                        if (!double.IsNaN(widthFactor))
                        {
                            attrRef.WidthFactor = widthFactor;
                        }




                        attrRef.Position.TransformBy(blockReference.BlockTransform);


                        //这个函数不知道怎么用，在这里用回报错无效的上下文环境

                        //attrRef.UpdateMTextAttribute();


                        //用这个无法重设属性

                        //blockReference.ResetBlock();

                        //降级为可读
                        attrRef.DowngradeOpen();
                        //改完，结束
                        isSucceed = true;
                        break;
                    }
                }
                transaction.Commit();

            }

            return isSucceed;
        }



        /// <summary>
        /// 修改块参照的属性可见性
        /// </summary>
        /// <param name="blockReferenceId">块参照的ObjectId</param>
        /// <param name="tagName">属性标记名称</param>
        /// <param name="isVisible">是否可见，默认为true</param>
        /// <returns>如果修改成功，返回true，否则，返回false（比如不存在这个属性）</returns>
        public bool ChangeBlockAttributeVisible(ObjectId blockReferenceId, string tagName, bool isVisible = true)
        {
            //返回值
            bool isSucceed = false;

            if (blockReferenceId.IsNull)
            {
                return isSucceed;
            }
            using (Transaction transaction = blockReferenceId.Database.TransactionManager.StartTransaction())
            {

                BlockReference blockReference = blockReferenceId.GetObject(OpenMode.ForRead) as BlockReference;

                foreach (ObjectId objectId in blockReference.AttributeCollection)
                {

                    //7:53 2023/12/24 在这里，第三个参数为：openErased 因为这个情况有些属性已经被删除，需要设置为true才能打开，不然会报错：eWasErased
                    //AttributeReference attrRef = objectId.GetObject(OpenMode.ForRead) as AttributeReference;
                    AttributeReference attrRef = objectId.GetObject(OpenMode.ForRead, true, true) as AttributeReference;

                    if (attrRef.IsErased)
                    {
                        continue;
                    }


                    //标记名称
                    string tag = attrRef.Tag;

                    //如果找到有这个属性标记
                    if (tag == tagName)
                    {
                        //升级为可写
                        attrRef.UpgradeOpen();

                        attrRef.Invisible = !isVisible;


                        //降级为可读
                        attrRef.DowngradeOpen();

                        isSucceed = true;
                        //改完，返回
                        break;

                    }
                }

                transaction.Commit();
            }

            return isSucceed;
        }


        /// <summary>
        /// 修改块参照的属性可见性
        /// </summary>
        /// <param name="blockReferenceId">块参照的ObjectId</param>
        /// <param name="tagNameLst">属性标记名称列表</param>
        /// <param name="isVisible">是否可见，默认为true</param>
        /// <returns>如果修改成功，返回true，否则，返回false（比如不存在这个属性）</returns>
        public bool ChangeBlockAttributeVisible(ObjectId blockReferenceId, List<string> tagNameLst, bool isVisible = true)
        {
            //返回值
            bool isSucceed = false;
            if (blockReferenceId.IsNull)
            {
                return isSucceed;
            }
            using (Transaction transaction = blockReferenceId.Database.TransactionManager.StartTransaction())
            {

                BlockReference blockReference = blockReferenceId.GetObject(OpenMode.ForWrite) as BlockReference;

                foreach (ObjectId objectId in blockReference.AttributeCollection)
                {

                    //7:53 2023/12/24 在这里，第三个参数为：openErased 因为这个情况有些属性已经被删除，需要设置为true才能打开，不然会报错：eWasErased
                    AttributeReference attrRef = objectId.GetObject(OpenMode.ForWrite, true, true) as AttributeReference;

                    //15:25 2023/12/22 报这个错： eWasErased 可能是没加这个判定
                    if (attrRef.IsErased)
                    {
                        continue;
                    }


                    //标记名称
                    string tag = attrRef.Tag;

                    //如果找到有这个属性标记
                    if (tagNameLst.Contains(tag))
                    {
                        attrRef.Invisible = !isVisible;
                        isSucceed = true;
                    }
                }

                transaction.Commit();

            }

            return isSucceed;

        }






        ///// <summary>
        ///// 修改块参照的属性
        ///// </summary>
        ///// <param name="blockReferenceId">块参照的ObjectId</param>
        ///// <param name="tagName">属性标记名称</param>
        ///// <param name="tagValue">属性值</param>
        ///// <param name="height">属性文字高度，默认不修改</param>
        ///// <param name="widthFactor">属性文字宽度因子，默认不修改</param>
        ///// <returns>如果修改成功，返回true，否则，返回false（比如不存在这个属性）</returns>
        //public bool ChangeBlockAttribute(ObjectId blockReferenceId, string tagName, string tagValue, double height = double.NaN, double widthFactor = double.NaN)
        //{
        //    if (blockReferenceId.IsNull)
        //    {
        //        return false;
        //    }
        //    using (Transaction transaction = blockReferenceId.Database.TransactionManager.StartTransaction())
        //    {

        //        BlockReference blockReference = blockReferenceId.GetObject(OpenMode.ForRead) as BlockReference;

        //        foreach (ObjectId objectId in blockReference.AttributeCollection)
        //        {
        //            AttributeReference attrRef = objectId.GetObject(OpenMode.ForRead) as AttributeReference;

        //            //标记名称
        //            string tag = attrRef.Tag;

        //            //如果找到有这个属性标记
        //            if (tag == tagName)
        //            {
        //                //升级为可写
        //                attrRef.UpgradeOpen();



        //                attrRef.Position.TransformBy(blockReference.BlockTransform);


        //                //这个函数不知道怎么用，在这里用回报错无效的上下文环境

        //                //attrRef.UpdateMTextAttribute();


        //                //用这个无法重设属性

        //                //blockReference.ResetBlock();



        //                attrRef.TextString = tagValue;


        //                //修改属性文字高度
        //                if (!double.IsNaN(height))
        //                {
        //                    attrRef.Height = height;
        //                }

        //                if (!double.IsNaN(widthFactor))
        //                {
        //                    attrRef.WidthFactor = widthFactor;
        //                }


        //                //降级为可读
        //                attrRef.DowngradeOpen();
        //                transaction.Commit();
        //                //改完，返回
        //                return true;
        //            }
        //        }
        //        return false;
        //    }
        //}





















        /// <summary>
        /// 修改块参照的属性的文字高度
        /// </summary>
        /// <param name="blockReferenceId">块参照的ObjectId</param>
        /// <param name="tagName">属性标记名称</param>
        /// <param name="height">文字高度</param>
        /// <returns>如果修改成功，返回true，否则，返回false（比如不存在这个属性）</returns>
        public bool ChangeBlockAttributeHeight(ObjectId blockReferenceId, string tagName, double height)
        {
            //返回值
            bool isSucceed = false;

            if (blockReferenceId.IsNull)
            {
                return isSucceed;
            }
            using (Transaction transaction = blockReferenceId.Database.TransactionManager.StartTransaction())
            {

                BlockReference blockReference = blockReferenceId.GetObject(OpenMode.ForRead) as BlockReference;

                foreach (ObjectId objectId in blockReference.AttributeCollection)
                {

                    //7:53 2023/12/24 在这里，第三个参数为：openErased 因为这个情况有些属性已经被删除，需要设置为true才能打开，不然会报错：eWasErased
                    //AttributeReference attrRef = objectId.GetObject(OpenMode.ForRead) as AttributeReference;

                    AttributeReference attrRef = objectId.GetObject(OpenMode.ForRead, true, true) as AttributeReference;

                    if (attrRef.IsErased)
                    {
                        continue;
                    }




                    //标记名称
                    string tag = attrRef.Tag;

                    //如果找到有这个属性标记
                    if (tag == tagName)
                    {
                        //升级为可写
                        attrRef.UpgradeOpen();

                        attrRef.Height = height;

                        //降级为可读
                        attrRef.DowngradeOpen();

                        //改完，返回
                        isSucceed = true;
                        break;

                    }
                }

                transaction.Commit();
            }

            return isSucceed;
        }




        /// <summary>
        /// 修改块参照的属性的文字高度
        /// </summary>
        /// <param name="blockReferenceId">块参照的ObjectId</param>
        /// <param name="tagName">属性标记名称</param>
        /// <param name="widthFactor">文字宽度因子</param>
        /// <returns>如果修改成功，返回true，否则，返回false（比如不存在这个属性）</returns>
        public bool ChangeBlockAttributeWidthFactor(ObjectId blockReferenceId, string tagName, double widthFactor)
        {
            //返回值
            bool isSucceed = false;

            if (blockReferenceId.IsNull)
            {
                return isSucceed;
            }
            using (Transaction transaction = blockReferenceId.Database.TransactionManager.StartTransaction())
            {

                BlockReference blockReference = blockReferenceId.GetObject(OpenMode.ForRead) as BlockReference;

                foreach (ObjectId objectId in blockReference.AttributeCollection)
                {

                    //7:53 2023/12/24 在这里，第三个参数为：openErased 因为这个情况有些属性已经被删除，需要设置为true才能打开，不然会报错：eWasErased
                    //AttributeReference attrRef = objectId.GetObject(OpenMode.ForRead) as AttributeReference;

                    AttributeReference attrRef = objectId.GetObject(OpenMode.ForRead, true, true) as AttributeReference;

                    if (attrRef.IsErased)
                    {
                        continue;
                    }




                    //标记名称
                    string tag = attrRef.Tag;

                    //如果找到有这个属性标记
                    if (tag == tagName)
                    {
                        //升级为可写
                        attrRef.UpgradeOpen();

                        attrRef.WidthFactor = widthFactor;

                        //降级为可读
                        attrRef.DowngradeOpen();

                        //改完，返回
                        isSucceed = true;
                        break;
                    }
                }

                transaction.Commit();
            }
            return isSucceed;
        }








        /// <summary>
        /// 获取块参照的属性字典，可能有多个属性名称有相同的情况
        /// </summary>
        /// <param name="blockReferenceId">块参照的ObjectId</param>
        /// <returns>块参照的属性字典,如果没有或失败，返回空的映射</returns>
        public Dictionary<string, List<string>> GetBlockAttributes(ObjectId blockReferenceId)
        {

            //返回值
            Dictionary<string, List<string>> attrNameValues = new Dictionary<string, List<string>>();

            if (blockReferenceId.IsNull)
            {
                return attrNameValues;
            }

            using (Transaction transaction = blockReferenceId.Database.TransactionManager.StartTransaction())
            {
                BlockReference blockReference = blockReferenceId.GetObject(OpenMode.ForRead) as BlockReference;

                foreach (ObjectId objectId in blockReference.AttributeCollection)
                {

                    //7:53 2023/12/24 在这里，第三个参数为：openErased 因为这个情况有些属性已经被删除，需要设置为true才能打开，不然会报错：eWasErased
                    //AttributeReference attrRef = objectId.GetObject(OpenMode.ForRead) as AttributeReference;

                    AttributeReference attrRef = objectId.GetObject(OpenMode.ForRead, true, true) as AttributeReference;

                    //15:25 2023/12/22 报这个错： eWasErased 可能是没加这个判定
                    if (attrRef.IsErased)
                    {
                        continue;
                    }





                    //9:08 2023/12/20 在块中有可能有属性名称相同的情况 这么用会出现键重复的的情况，改为list

                    //标记名称
                    //attrNameValues.Add(attrRef.Tag, attrRef.TextString);

                    string tag = attrRef.Tag;
                    if (attrNameValues.TryGetValue(tag, out var value)) //已经存在
                    {
                        value.Add(attrRef.TextString);
                    }
                    else //不存在
                    {

                        attrNameValues[attrRef.Tag]= new List<string>
                    {
                        attrRef.TextString
                    };
                    }


                }

                transaction.Commit();
            }

            return attrNameValues;

        }









        /// <summary>
        /// 获取块参照的属性定义列表
        /// </summary>
        /// <param name="blockReferenceId">块参照的ObjectId</param>
        /// <returns>块参照的属性定义列表</returns>
        public List<AttributeReference> GetBlockAttributeReferences(ObjectId blockReferenceId)
        {

            //返回值
            List<AttributeReference> attributeReferenceLst = new List<AttributeReference>();

            if (blockReferenceId.IsNull)
            {
                return attributeReferenceLst;
            }

            using (Transaction transaction = blockReferenceId.Database.TransactionManager.StartTransaction())
            {
                BlockReference blockReference = blockReferenceId.GetObject(OpenMode.ForRead) as BlockReference;

                foreach (ObjectId objectId in blockReference.AttributeCollection)
                {

                    //7:53 2023/12/24 在这里，第三个参数为：openErased 因为这个情况有些属性已经被删除，需要设置为true才能打开，不然会报错：eWasErased
                    //AttributeReference attrRef = objectId.GetObject(OpenMode.ForRead) as AttributeReference;

                    AttributeReference attrRef = objectId.GetObject(OpenMode.ForRead, true, true) as AttributeReference;

                    //15:25 2023/12/22 报这个错： eWasErased 可能是没加这个判定
                    if (attrRef.IsErased)
                    {
                        continue;
                    }


                    attributeReferenceLst.Add(attrRef);

                }

                transaction.Commit();
            }

            return attributeReferenceLst;

        }


















        /// <summary>
        /// 获取某个标记的属性值 可能有多个值，返回第一个或默认的值
        /// </summary>
        /// <param name="blockReferenceId">块参照的ObjectId </param>
        /// <param name="tagName">标记名称</param>
        /// <returns>属性值string，如果没有，返回null</returns>
        public string GetBlockAttributeValueByTagName(ObjectId blockReferenceId, string tagName)
        {
            Dictionary<string, List<string>> attrNameValues = GetBlockAttributes(blockReferenceId);

            string attributeValue = null;

            if (attrNameValues.ContainsKey(tagName))
            {
                attributeValue = attrNameValues[tagName].FirstOrDefault();
            }
            return attributeValue;
        }








        /// <summary>
        /// 判断块参照中是否含有某个属性
        /// </summary>
        /// <param name="blockReferenceId">块参照的ObjectId</param>
        /// <param name="tagName">属性标记名称，不分大小写</param>
        /// <returns></returns>
        public bool IsBlockReferenceHasAttribute(ObjectId blockReferenceId, string tagName)
        {
            bool hasAttribute = IsBlockReferenceHasAttribute(blockReferenceId);
            if (!hasAttribute)
            {
                return hasAttribute;
            }

            //因为以上的hasAttribute为true，下面还需要检测是否含有指定的属性，需要重新设置值
            hasAttribute = false;



            //不分大小写
            string tagNameUpper = tagName.ToUpper();

            using (Transaction transaction = blockReferenceId.Database.TransactionManager.StartTransaction())
            {
                BlockReference blockReference = blockReferenceId.GetObject(OpenMode.ForRead) as BlockReference;

                foreach (ObjectId objectId in blockReference.AttributeCollection)
                {
                    //7:53 2023/12/24 在这里，第三个参数为：openErased 因为这个情况有些属性已经被删除，需要设置为true才能打开，不然会报错：eWasErased
                    //AttributeReference attrRef = objectId.GetObject(OpenMode.ForRead) as AttributeReference;

                    AttributeReference attrRef = objectId.GetObject(OpenMode.ForRead, true, true) as AttributeReference;

                    //15:25 2023/12/22 报这个错： eWasErased 可能是没加这个判定
                    if (attrRef.IsErased)
                    {
                        continue;
                    }



                    //标记名称
                    if (attrRef.Tag.ToUpper() == tagNameUpper)
                    {
                        hasAttribute = true;
                        break;
                    }

                }
                transaction.Commit();

            }

            //遍历了 ，都没有返回，说明没有
            return hasAttribute;

        }







        /// <summary>
        /// 将一个块参数替换为另一个块
        /// </summary>
        /// <param name="blockReferenceId">旧的块参数对象</param>
        /// <param name="recordId">新块的块表记录对象</param>
        /// <returns>如果成功，返回新块的ObjectId,否则，返回ObjectId.Null</returns>
        public ObjectId ReplaceBlockReference(ObjectId blockReferenceId, ObjectId recordId)
        {
            using (Transaction transaction = blockReferenceId.Database.TransactionManager.StartTransaction())
            {

                BlockReference blockReference = transaction.GetObject(blockReferenceId, OpenMode.ForWrite) as BlockReference;

                //获取旧的块的插入点
                Point3d insertPoint = GetPosition(blockReferenceId);
                ObjectId newBlockReferenceId = InsertBlockReference(recordId, insertPoint);

                //删除旧的块
                ObjectTool objectTool = new ObjectTool(blockReferenceId.Database);
                objectTool.EraseDBObject(blockReferenceId);

                transaction.Commit();
                return newBlockReferenceId;

            }
        }






        /// <summary>
        /// 通过块参照获取块表记录
        /// </summary>
        /// <param name="referenceId">块参照的ObjectId</param>
        /// <returns>如果成功，返回块表记录的ObjectId,否则，返回ObjectId.Null</returns>
        public ObjectId GetRecordByReference(ObjectId referenceId)
        {
            //返回值
            ObjectId recordId = ObjectId.Null;

            if (referenceId.IsNull)
            {
                return recordId;
            }


            using (Transaction transaction = referenceId.Database.TransactionManager.StartTransaction())
            {
                try
                {
                    if (transaction.GetObject(referenceId, OpenMode.ForRead) is BlockReference blockReference)
                    {
                        recordId = blockReference.BlockTableRecord;
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();
                }
            }

            return recordId;
        }








        /// <summary>
        /// 通过块参照添加属性列表
        /// </summary>
        /// <param name="referenceId">块参照的ObjectId</param>
        /// <param name="attributeDefinitionLst">属性定义列表</param>
        /// <returns>如果成功，返回true，否则，返回false</returns>
        public bool AddAttributesToBlockByReference(ObjectId referenceId, List<AttributeDefinition> attributeDefinitionLst)
        {
            ObjectId recordId = GetRecordByReference(referenceId);

            return AddAttributesToBlockByRecord(recordId, attributeDefinitionLst);

        }



        /// <summary>
        /// 通过块表记录添加属性列表
        /// </summary>
        /// <param name="recordId">块表记录的ObjectId</param>
        /// <param name="attributeDefinitionLst">属性定义列表</param>
        /// <returns>如果成功，返回true，否则，返回false</returns>
        public bool AddAttributesToBlockByRecord(ObjectId recordId, List<AttributeDefinition> attributeDefinitionLst)
        {
            //返回值
            bool isSucceed = false;

            if (recordId.IsNull)
            {
                return isSucceed;
            }


            using (Transaction transaction = recordId.Database.TransactionManager.StartTransaction())
            {

                if (transaction.GetObject(recordId, OpenMode.ForWrite) is BlockTableRecord blockTableRecord)
                {

                    //先获取所有的属性标记 

                    Dictionary<string, string> existedTagAndValueMap = GetAttributesInBlock(recordId);

                    try
                    {
                        foreach (var item in attributeDefinitionLst)
                        {
                            if (existedTagAndValueMap.ContainsKey(item.Tag))//已经存在
                            {
                                continue;
                            }

                            AttributeDefinition attributeDefinition = new AttributeDefinition()
                            {
                                Position = item.Position,
                                Invisible = item.Invisible,
                                Verifiable = item.Verifiable,
                                Tag = item.Tag,
                                Prompt = item.Prompt,
                                TextString = item.TextString,
                                Height = item.Height,
                                Justify = item.Justify
                            };

                            blockTableRecord.AppendEntity(attributeDefinition);
                            transaction.AddNewlyCreatedDBObject(attributeDefinition, true);
                        }


                        transaction.Commit();

                        isSucceed = true;
                    }
                    catch (System.Exception)
                    {

                        transaction.Abort();
                    }
                }
            }


            if (isSucceed) //如果成功，记得同步属性，不然虽然添加成功了，用户还是看不到
            {
                AddAttributesSyncAttibuteReferences(recordId);
            }

            return isSucceed;
        }



        /// <summary>
        /// 通过块表记录删除所有属性
        /// </summary>
        /// <param name="recordId">块表记录的ObjectId</param>
        /// <param name="tagNameLst">属性名称列表，如果为null，则包括所有的属性</param>
        /// <returns>如果成功，返回true，否则，返回false</returns>
        public bool DeleteAttributesInBlockByRecord(ObjectId recordId, List<string> tagNameLst = null)
        {
            //返回值
            bool isSucceed = false;

            if (recordId.IsNull)
            {
                return isSucceed;
            }

            using (Transaction transaction = recordId.Database.TransactionManager.StartTransaction())
            {
                if (transaction.GetObject(recordId, OpenMode.ForRead) is BlockTableRecord blockTableRecord)
                {

                    try
                    {
                        if (blockTableRecord.HasAttributeDefinitions) //有属性的情况 有些没有属性 这个也会是true 没搞懂
                        {

                            foreach (ObjectId id in blockTableRecord)
                            {
                                if (transaction.GetObject(id, OpenMode.ForRead) is AttributeDefinition ad)
                                {

                                   
                                        

                                    if (tagNameLst == null) //对于全部属性都要操作
                                    {
                                        ad.UpgradeOpen();
                                        ad.Erase();
                                        isSucceed = true;
                                    }

                                    //只操作指定的属性
                                    else if (tagNameLst.Exists(x => x.Equals(ad.Tag, StringComparison.OrdinalIgnoreCase)))
                                    {
                                        ad.UpgradeOpen();
                                        ad.Erase();
                                        isSucceed = true;
                                    }

                                }
                            }

                            transaction.Commit();
                        }

                        else //没有属性的情况
                        {
                            isSucceed = true;
                            transaction.Commit();
                            return isSucceed;
                        }

                    }
                    catch (System.Exception)
                    {
                        isSucceed = false;
                        transaction.Abort();
                    }
                }
            }


            if (isSucceed) //如果成功，记得同步属性，不然虽然删除成功了，用户还是看不到
            {
                DeleteAttributesSyncAttibuteReferences(recordId, tagNameLst);
            }

            return isSucceed;
        }





        /// <summary>
        /// 通过块参照删除所有属性
        /// </summary>
        /// <param name="blockReferenceId">块参照的ObjectId</param>
        /// <param name="tagNameLst">属性名称列表，如果为null，则包括所有的属性</param>
        /// <returns>如果成功，返回true，否则，返回false</returns>
        public bool DeleteAttributesInBlockByReference(ObjectId blockReferenceId, List<string> tagNameLst = null)
        {
            ObjectId recordId = GetRecordByReference(blockReferenceId);
            return DeleteAttributesInBlockByRecord(recordId, tagNameLst);
        }






        /// <summary>
        /// 通过块表记录获取属性和属性值的映射
        /// </summary>
        /// <param name="recordId">块表记录对象</param>
        /// <returns>属性和属性值的映射，如果没有属性，将返回空的映射</returns>
        public Dictionary<string, string> GetAttributesInBlock(ObjectId recordId)
        {
            //返回值
            Dictionary<string, string> tagAndValueMap = new Dictionary<string, string>();
            if (recordId.IsNull)
            {
                return tagAndValueMap;
            }

            using (Transaction transaction = recordId.Database.TransactionManager.StartTransaction())
            {

                BlockTableRecord blockTableRecord = transaction.GetObject(recordId, OpenMode.ForRead) as BlockTableRecord;

                if (blockTableRecord != null)
                {

                    if (blockTableRecord.HasAttributeDefinitions)
                    {
                        var expectedIds =
                                        from ObjectId id in blockTableRecord
                                        where id.ObjectClass.IsDerivedFrom(RXClass.GetClass(typeof(AttributeDefinition)))
                                        select id;

                        if (expectedIds == null || expectedIds.Count() == 0)
                        {
                            return tagAndValueMap;
                        }

                        foreach (var item in expectedIds)
                        {
                            AttributeDefinition attributeDefinition = transaction.GetObject(item, OpenMode.ForRead) as AttributeDefinition;

                            string tag = attributeDefinition.Tag;
                            string textString = attributeDefinition.TextString;
                            tagAndValueMap[tag] = textString;
                        }

                    }

                }


            }

            return tagAndValueMap;
        }







        /// <summary>
        /// 将外部DWG文件所有DBObject组成一个块插入当前文件, 如果DWG文件中有属性, 则属性变为块属性，
        /// 注意，不能直接将当前文件作为外部文件直接插入，可以复制一个新的再插入
        /// </summary>
        /// <param name="document">文档对象</param>
        /// <param name="fileName">DWG文件全路径名称</param>
        /// <param name="insertPoint">插入点</param>
        /// <param name="blockName">块表记录名，如果为空，则自动按当前时间作为默认名称</param>
        /// <param name="scale">插入比例</param>
        /// <param name="spaceName">模型空间或图纸空间，如果为"PAPERSPACE"（不分大小写），为图纸空间，如果指定的名称为其它的图纸空间名称，则为指定的图纸空间名称；为空或其它，为模型空间</param>
        /// <returns>块参照的ObjectId，如果不成功，返回ObjectId.Null,需要用ObjectId.IsNull来判断</returns>
        public ObjectId InsertExternalDwgFileAsBlockReference(Document document, string fileName, Point3d insertPoint, string blockName = null, double scale = 1, string spaceName = null)
        {

            using (DocumentLock docLock = document.LockDocument()) //锁定文档
            {
                Database database = document.Database;
                return InsertExternalDwgFileAsBlockReference(database, fileName, insertPoint, blockName, scale, spaceName);

            }

        }









        /// <summary>
        /// 将外部DWG文件所有DBObject组成一个块插入当前文件, 如果DWG文件中有属性, 则属性变为块属性，
        /// 注意，不能直接将当前文件作为外部文件直接插入，可以复制一个新的再插入
        /// </summary>
        /// <param name="database">数据库对象</param>
        /// <param name="fileName">DWG文件全路径名称</param>
        /// <param name="insertPoint">插入点</param>
        /// <param name="blockName">块表记录名，如果为空，则自动按当前时间作为默认名称</param>
        /// <param name="scale">插入比例</param>
        /// <param name="spaceName">模型空间或图纸空间，如果为"PAPERSPACE"（不分大小写），为图纸空间，如果指定的名称为其它的图纸空间名称，则为指定的图纸空间名称；为空或其它，为模型空间</param>
        /// <returns>块参照的ObjectId，如果不成功，返回ObjectId.Null,需要用ObjectId.IsNull来判断</returns>
        public ObjectId InsertExternalDwgFileAsBlockReference(Database database, string fileName, Point3d insertPoint, string blockName = null, double scale = 1, string spaceName = null)
        {


            //默认的块名
            if (string.IsNullOrEmpty(blockName))
            {
                blockName = TimeTool.GetCurrentTimeByFormat();

            }




            using (Transaction trans = database.TransactionManager.StartTransaction()) //事务
            {
                try
                {
                    BlockTable curBlockTb = trans.GetObject(database.BlockTableId, OpenMode.ForRead) as BlockTable; //当前文档块表




                    //15:48 2023/6/20 还需要考虑到其它图纸空间的情况

                    //是模型空间还是图纸空间

                    if (!string.IsNullOrEmpty(spaceName) && spaceName.ToUpper() == "PAPERSPACE") //为图纸空间
                    {
                        spaceName = BlockTableRecord.PaperSpace;
                    }
                    else if (!string.IsNullOrEmpty(spaceName) && curBlockTb.Has(spaceName)) //其它图纸空间
                    {

                    }
                    else  //为模型空间
                    {
                        spaceName = BlockTableRecord.ModelSpace;

                    }




                    ObjectId blockObjId = new ObjectId(); //用于块表记录BlockTableRecord的编号,不是BlockReference的



                    if (!File.Exists(fileName)) //不存在该文件
                    {
                        trans.Abort(); //事务终止
                        return ObjectId.Null;
                    }


                    Database sourceDB = new Database(false, true);
                    sourceDB.ReadDwgFile(fileName, FileShare.Read, true, null); //后台读取DWG文件信息; 参数: 文件名, 打开方式, 是否允许转换版本, 密码
                    blockObjId = database.Insert(blockName, sourceDB, false); //将一个数据库插入到当前数据库的一个块中; 参数: 新创建的块表记录名, 资源数据库, 资源数据库是否保持原样
                    sourceDB.CloseInput(true); //是否关闭ReadDwgFile()方法之后打开的文件
                    sourceDB.Dispose();


                    //如果导入的时候有问题，直接返回空的

                    if (blockObjId == null)
                    {
                        return ObjectId.Null;
                    }



                    //string layoutName = LayoutManager.Current.CurrentLayout; //获得当前布局空间


                    BlockTableRecord block = trans.GetObject(blockObjId, OpenMode.ForWrite) as BlockTableRecord; //根据块表记录编号获取的块表记录, 用于取得它的属性定义
                    block.Explodable = true; //块参照是否能被炸开

                    BlockReference blockReference = new BlockReference(insertPoint, blockObjId); //新建块参照

                    //获取空间
                    BlockTableRecord layout = trans.GetObject(curBlockTb[spaceName], OpenMode.ForWrite) as BlockTableRecord;



                    //if (layoutName.Equals("Model"))
                    //    layout = trans.GetObject(curBlockTb[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord; //根据当前布局空间获取的表块记录, 用于将块插入布局空间中
                    //else
                    //    layout = trans.GetObject(curBlockTb[BlockTableRecord.PaperSpace], OpenMode.ForWrite) as BlockTableRecord;




                    layout.AppendEntity(blockReference); //在当前空间中追加此块参照
                    trans.AddNewlyCreatedDBObject(blockReference, true);
                    if (!block.HasAttributeDefinitions) //如果该块表记录中不包含任何属性定义
                        goto No_AttributeDefinitions; //直接去往No_AttributeDefinitions


                    //以下为处理块参照的属性
                    AttributeDefinition attriDef = null;
                    AttributeReference attriRefe = null;
                    Matrix3d mtr = Matrix3d.Displacement(block.Origin.GetVectorTo(insertPoint)); //Displacement 取代
                    foreach (ObjectId entityObjId in block) //遍历块表记录中的实体编号
                    {
                        attriDef = trans.GetObject(entityObjId, OpenMode.ForRead) as AttributeDefinition; //打开实体通过实体编号



                        if (attriDef == null) //若还是为null
                            continue;



                        attriRefe = new AttributeReference(); //每次循环new一次新的对象, 确保上次属性不会残留
                        attriRefe.SetPropertiesFrom(attriDef); //SetPropertiesFrom 设置属性来自


                        attriRefe.SetAttributeFromBlock(attriDef, mtr); //通过块设置属性 参数: 属性定义, 变形矩阵


                        attriRefe.TextString = ""; //设置属性值为""
                        blockReference.AttributeCollection.AppendAttribute(attriRefe); //块参照中添加此属性参照
                        trans.AddNewlyCreatedDBObject(attriRefe, true);
                        attriRefe = null;
                    }



                No_AttributeDefinitions:
                    blockReference.TransformBy(Matrix3d.Scaling(scale, insertPoint)); //缩放块; 修改块比例
                    trans.Commit(); //提交事务

                    //返回块参照的ObjectId
                    return blockReference.Id;

                }
                catch
                {

                    //9:18 2022/6/10 添加这个
                    trans.Abort(); //事务终止


                    return ObjectId.Null; //返回空的
                }
            }
        }









        /// <summary>
        /// 将外部DWG文件所有DBObject组成一个块插入当前文件, 如果DWG文件中有属性, 则属性变为块属性，
        /// 注意，不能直接将当前文件作为外部文件直接插入，可以复制一个新的再插入，默认插入模型空间中
        /// </summary>
        /// <param name="database">数据库对象</param>
        /// <param name="fileName">DWG文件全路径名称</param>
        /// <param name="insertPoint">插入点</param>
        /// <param name="blockName">块表记录名，如果为空，则自动按当前时间作为默认名称</param>
        /// <param name="scale">插入比例</param>
        /// <param name="spaceName">空间名称，如果为"PAPERSPACE" 不分大小写，则为图纸空间，其它情况为模型空间</param>
        /// <returns>块参照的ObjectId，如果不成功，返回ObjectId.Null,需要用ObjectId.IsNull来判断</returns>
        public ObjectId InsertExternalDwgFileAsBlockReference2(Database database, string fileName, Point3d insertPoint, string blockName = null, double scale = 1, string spaceName = null)
        {


            //默认的块名
            if (string.IsNullOrEmpty(blockName))
            {
                blockName = TimeTool.GetCurrentTimeByFormat();

            }






            using (Transaction trans = database.TransactionManager.StartTransaction()) //事务
            {
                try
                {
                    BlockTable curBlockTb = trans.GetObject(database.BlockTableId, OpenMode.ForRead) as BlockTable; //当前文档块表
                    ObjectId blockObjId = new ObjectId(); //用于块表记录BlockTableRecord的编号,不是BlockReference的



                    if (!File.Exists(fileName)) //不存在该文件
                    {
                        trans.Abort(); //事务终止
                        return ObjectId.Null;
                    }


                    Database sourceDB = new Database(false, true);
                    //Database sourceDB = new Database(true, false);



                    sourceDB.ReadDwgFile(fileName, FileShare.Read, true, null); //后台读取DWG文件信息; 参数: 文件名, 打开方式, 是否允许转换版本, 密码



                    blockObjId = database.Insert(blockName, sourceDB, false); //将一个数据库插入到当前数据库的一个块中; 参数: 新创建的块表记录名, 资源数据库, 资源数据库是否保持原样
                    sourceDB.CloseInput(true); //是否关闭ReadDwgFile()方法之后打开的文件
                    sourceDB.Dispose();


                    //如果导入的时候有问题，直接返回空的

                    if (blockObjId == null)
                    {
                        return ObjectId.Null;
                    }



                    //string layoutName = LayoutManager.Current.CurrentLayout; //获得当前布局空间


                    BlockTableRecord block = trans.GetObject(blockObjId, OpenMode.ForWrite) as BlockTableRecord; //根据块表记录编号获取的块表记录, 用于取得它的属性定义
                    block.Explodable = true; //块参照是否能被炸开

                    BlockReference blockReference = new BlockReference(insertPoint, blockObjId); //新建块参照







                    //15:48 2023/6/20 还需要考虑到其它图纸空间的情况

                    //是模型空间还是图纸空间

                    if (!string.IsNullOrEmpty(spaceName) && spaceName.ToUpper() == "PAPERSPACE") //为图纸空间
                    {
                        spaceName = BlockTableRecord.PaperSpace;
                    }
                    else if (!string.IsNullOrEmpty(spaceName) && curBlockTb.Has(spaceName)) //其它图纸空间
                    {

                    }
                    else  //为模型空间
                    {
                        spaceName = BlockTableRecord.ModelSpace;

                    }









                    //获取空间
                    BlockTableRecord layout = trans.GetObject(curBlockTb[spaceName], OpenMode.ForWrite) as BlockTableRecord;



                    //if (layoutName.Equals("Model"))
                    //    layout = trans.GetObject(curBlockTb[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord; //根据当前布局空间获取的表块记录, 用于将块插入布局空间中
                    //else
                    //    layout = trans.GetObject(curBlockTb[BlockTableRecord.PaperSpace], OpenMode.ForWrite) as BlockTableRecord;




                    layout.AppendEntity(blockReference); //在当前空间中追加此块参照







                    //if (!block.HasAttributeDefinitions) //如果该块表记录中不包含任何属性定义
                    //    goto No_AttributeDefinitions; //直接去往No_AttributeDefinitions


                    if (block.HasAttributeDefinitions) //如果该块表记录中包含属性定义

                    {


                        //以下为处理块参照的属性
                        //AttributeDefinition attriDef = null;
                        //AttributeReference attriRefe = null;



                        Matrix3d mtr = Matrix3d.Displacement(block.Origin.GetVectorTo(insertPoint)); //Displacement 取代


                        foreach (ObjectId entityObjId in block) //遍历块表记录中的实体编号
                        {
                            AttributeDefinition attriDef = trans.GetObject(entityObjId, OpenMode.ForRead) as AttributeDefinition; //打开实体通过实体编号



                            if (attriDef == null) //若还是为null
                                continue;



                            AttributeReference attriRefe = new AttributeReference(); //每次循环new一次新的对象, 确保上次属性不会残留

                            //attriRefe.SetPropertiesFrom(attriDef); //SetPropertiesFrom 设置属性来自

                            //attriRefe.SetAttributeFromBlock(attriDef, mtr); //通过块设置属性 参数: 属性定义, 变形矩阵


                            //attriRefe.Position = attriDef.Position.TransformBy(mtr);


                            attriRefe.SetAttributeFromBlock(attriDef, blockReference.BlockTransform);


                            //attriRefe.Position = attriDef.Position.TransformBy(blockReference.BlockTransform);

                            attriRefe.AdjustAlignment(database);

                            attriRefe.TextString = ""; //设置属性值为""


                            blockReference.AttributeCollection.AppendAttribute(attriRefe); //块参照中添加此属性参照
                            trans.AddNewlyCreatedDBObject(attriRefe, true);
                            //attriRefe = null;
                        }
                    }


                    //13:37 2022/8/19 不管是否有属性，都应该缩放块
                    //else //如果该块表记录中不包含任何属性定义
                    //{
                    //    blockReference.TransformBy(Matrix3d.Scaling(scale, insertPoint)); //缩放块; 修改块比例
                    //}


                    blockReference.TransformBy(Matrix3d.Scaling(scale, insertPoint)); //缩放块; 修改块比例



                    trans.AddNewlyCreatedDBObject(blockReference, true);





                    //No_AttributeDefinitions:
                    //    blockReference.TransformBy(Matrix3d.Scaling(scale, insertPoint)); //缩放块; 修改块比例





                    trans.Commit(); //提交事务

                    //返回块参照的ObjectId
                    return blockReference.Id;

                }
                catch
                {


                    //9:18 2022/6/10 添加这个
                    trans.Abort(); //事务终止


                    return ObjectId.Null; //返回空的
                }
            }
        }









        /// <summary>
        /// 获取块表记录的块参照的ObjectId列表
        /// </summary>
        /// <param name="blockReferenceName">块表记录名称或者快参数名称</param>
        /// <returns>块参照的ObjectId的列表，如果没有找到，null</returns>
        public List<ObjectId> GetBlockReferenceIds(string blockReferenceName)
        {

            //先查找块表记录
            ObjectId tableRecordId = FindBlockTableRecordIdByName(blockReferenceName);

            if (tableRecordId == ObjectId.Null)  //没找到，直接返回null
            {
                return null;
            }

            return GetBlockReferenceIds(tableRecordId);

        }











        /// <summary>
        /// 获取块表记录的块参照的ObjectId列表
        /// </summary>
        /// <param name="blockTableRecordId">块表记录的ObjectId</param>
        /// <returns>块参照的ObjectId的列表，如果没有找到，返回空的列表</returns>
        public List<ObjectId> GetBlockReferenceIds(ObjectId blockTableRecordId)
        {

            using (Transaction transaction = blockTableRecordId.Database.TransactionManager.StartTransaction())
            {

                //    using (Transaction transaction = m_document.TransactionManager.StartTransaction())
                //{
                BlockTableRecord record = transaction.GetObject(blockTableRecordId, OpenMode.ForRead) as BlockTableRecord;
                ObjectIdCollection collection = record.GetBlockReferenceIds(false, false);

                List<ObjectId> objectIds = new List<ObjectId>();
                foreach (ObjectId objectId in collection)
                {
                    objectIds.Add(objectId);
                }


                return objectIds;

            }
        }




































        /// <summary>
        /// 添加对象到块
        /// </summary>
        /// <param name="blockTableRecordId">块的ObjectId</param>
        /// <param name="ent">图元对象</param>
        /// <returns>对象的ObjectId</returns>
        public ObjectId AddEntityInBlock(ObjectId blockTableRecordId, Entity ent)
        {

            //声明ObjectId，用于返回
            ObjectId entId = ObjectId.Null;
            //开启事务处理
            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                //打开块表
                BlockTable blockTable = (BlockTable)transaction.GetObject(m_database.BlockTableId, OpenMode.ForRead);
                //打开块表记录
                BlockTableRecord btr = (BlockTableRecord)transaction.GetObject(blockTableRecordId, OpenMode.ForWrite);
                //添加图形到块表记录
                entId = btr.AppendEntity(ent);
                //更新数据信息
                transaction.AddNewlyCreatedDBObject(ent, true);
                transaction.Commit();
            }
            return entId;
        }



        //public void CreateBlockReference(SelectionSet selectionSet, string blockName)
        //{
        //    ObjectId[] objectIds = selectionSet.GetObjectIds();

        //    ObjectIdCollection objectIdCollection = new ObjectIdCollection(objectIds);


        //    Database db = m_document.Database;

        //    //BlockReference blockReference = new BlockReference();
        //    //blockReference.Bounds;

        //  Database database=  db.Wblock(objectIdCollection, Point3d.Origin);
        //}








        /// <summary>
        /// 将对象列表从一个块表记录移动到另一个块表记录，比如将模型空间中的对象移动到某个块中，注意，坐标原点对准坐标原点
        /// </summary>
        /// <param name="blockTableRecordId">块表记录的ObjectId</param>
        /// <param name="objectIds">被移动的对象的ObjectId列表</param>
        public void MoveObjectsToBlock(ObjectId blockTableRecordId, List<ObjectId> objectIds)
        {

            //using (Transaction transaction = m_document.TransactionManager.StartTransaction())
            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                BlockTableRecord btr = transaction.GetObject(blockTableRecordId, OpenMode.ForWrite) as BlockTableRecord;
                ObjectIdCollection objectIdCollection = new ObjectIdCollection();

                foreach (ObjectId objectId in objectIds)
                {
                    objectIdCollection.Add(objectId);

                }
                btr.AssumeOwnershipOf(objectIdCollection);

                transaction.Commit();

            }
        }






        /// <summary>
        /// 增加属性时，同步所有的块参考中的属性（包括匿名块）
        /// </summary>
        /// <param name="recordId">块表的ObjectId</param>
        /// <param name="tagNameLst">标记名称列表</param>
        public void AddAttributesSyncAttibuteReferences(ObjectId recordId, List<string> tagNameLst = null)
        {

            using (Transaction trans = m_database.TransactionManager.StartTransaction())
            {
                try
                {

                    BlockReference blockReference;
                    AttributeCollection attributeCollection;
                    AttributeReference attributeReference;
                    AttributeDefinition attributeDefinition;
                    BlockTableRecord blockTableRecord;
                    ObjectId AttributeID;


                    var attributeDefinitionMap = new Dictionary<string, AttributeDefinition>();
                    var BrefIDs = new ObjectIdCollection();
                    string txt;
                    var atRefTags = new System.Collections.Specialized.StringCollection();
                    blockTableRecord = trans.GetObject(recordId, OpenMode.ForRead, false, true) as BlockTableRecord;



                    foreach (ObjectId currentAttID in blockTableRecord)
                    {
                        AttributeID = currentAttID;

                        if (AttributeID.ObjectClass.Name == "AcDbAttributeDefinition")
                        {
                            attributeDefinition = trans.GetObject(AttributeID, OpenMode.ForRead, false, true) as AttributeDefinition;

                            if (tagNameLst == null || tagNameLst.Contains(attributeDefinition.Tag))
                            {
                                attributeDefinitionMap.Add(attributeDefinition.Tag, attributeDefinition);
                            }
                            else
                            {
                                attributeDefinition.Dispose();
                            }
                        }
                    }

                    if (blockTableRecord.IsDynamicBlock) //是动态块，从匿名块中获取
                    {

                        //13:24 2023/8/25 动态块也有可能含有该值
                        BrefIDs = blockTableRecord.GetBlockReferenceIds(true, true);

                        //获取所有的匿名块
                        ObjectIdCollection anonymousIds = blockTableRecord.GetAnonymousBlockIds();

                        foreach (ObjectId anonymousBtrId in anonymousIds)
                        {

                            //获取匿名块
                            BlockTableRecord anonymousBtr = (BlockTableRecord)trans.GetObject(anonymousBtrId, OpenMode.ForRead);

                            //获取块中的所有块参考
                            ObjectIdCollection blockRefIds = anonymousBtr.GetBlockReferenceIds(true, true);

                            foreach (ObjectId id in blockRefIds)
                            {

                                BrefIDs.Add(id);

                            }

                        }
                    }
                    else //非动态块
                    {

                        BrefIDs = blockTableRecord.GetBlockReferenceIds(true, true);


                        ////获取所有的块参考
                        //var BrefIDs1 = blockTableRecord.GetBlockReferenceIds(true, true);
                        //var BrefIDs2 = blockTableRecord.GetBlockReferenceIds(true, false);
                        //var BrefIDs3 = blockTableRecord.GetBlockReferenceIds(false, true);
                        //var BrefIDs4 = blockTableRecord.GetBlockReferenceIds(false, false);


                        //MessageBox.Show("BrefIDs1=" + BrefIDs1.Count + "\n" + "BrefIDs2=" + BrefIDs2.Count + "\n" +
                        //    "BrefIDs3=" + BrefIDs3.Count + "\n" + "BrefIDs4=" + BrefIDs4.Count);

                    }

                    //MessageBox.Show(BrefIDs.Count.ToString());


                    foreach (ObjectId BrefID in BrefIDs)
                    {
                        blockReference = trans.GetObject(BrefID, OpenMode.ForRead, false, true) as BlockReference;

                        //if (blockReference == null)
                        //{
                        //    continue;
                        //}

                        attributeCollection = blockReference.AttributeCollection;

                        //foreach (ObjectId currentAttID1 in attributeCollection)
                        //{
                        //    AttributeID = currentAttID1;
                        foreach (ObjectId currentAttID in attributeCollection)
                        {
                            if (currentAttID.ObjectClass.DxfName != "ATTRIB")
                            {
                                //7:53 2023/12/24 在这里，第三个参数为：openErased 因为这个情况有些属性已经被删除，需要设置为true才能打开，不然会报错：eWasErased
                                //attributeReference = trans.GetObject(currentAttID, OpenMode.ForWrite, false, true) as AttributeReference;
                                attributeReference = trans.GetObject(currentAttID, OpenMode.ForWrite, true, true) as AttributeReference;



                                //15:25 2023/12/22 报这个错： eWasErased 可能是没加这个判定
                                if (attributeReference.IsErased)
                                {
                                    continue;
                                }




                                atRefTags.Add(attributeReference.Tag);
                                if (attributeDefinitionMap.ContainsKey(attributeReference.Tag))
                                {
                                    txt = attributeReference.TextString;
                                    attributeReference.SetAttributeFromBlock(attributeDefinitionMap[attributeReference.Tag], blockReference.BlockTransform);
                                    attributeReference.TextString = txt;
                                }
                                attributeReference.Dispose();
                            }
                        }
                        foreach (KeyValuePair<string, AttributeDefinition> keyval in attributeDefinitionMap)
                        {

                            //之前不存在的 要添加
                            if (!atRefTags.Contains(keyval.Key))
                            {
                                if (!blockReference.IsWriteEnabled)
                                    blockReference.UpgradeOpen();
                                attributeReference = new AttributeReference();
                                attributeReference.SetAttributeFromBlock(keyval.Value, blockReference.BlockTransform);
                                blockReference.AttributeCollection.AppendAttribute(attributeReference);
                                trans.AddNewlyCreatedDBObject(attributeReference, true);
                                attributeReference.Dispose();
                            }
                        }
                        blockReference.Dispose();
                    }
                    foreach (KeyValuePair<string, AttributeDefinition> keyval in attributeDefinitionMap)
                        keyval.Value.Dispose();
                    trans.Commit();
                }
                catch (System.Exception ex)
                {
                    trans.Abort();
                    MessageBox.Show("同步块操作失败:" + ex.Message, "Tips");
                }
            }
        }




        /// <summary>
        /// 删除所有属性时，同步所有的块参考中的属性（包括匿名块）
        /// </summary>
        /// <param name="recordId">块表的ObjectId</param>
        /// <param name="tagNameLst">属性名称列表，如果为null，则包括所有的属性</param>
        public void DeleteAttributesSyncAttibuteReferences(ObjectId recordId, List<string> tagNameLst = null)
        {
            using (Transaction trans = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    BlockReference blockReference;
                    AttributeCollection attributeCollection;
                    AttributeReference attributeReference;

                    var BrefIDs = new ObjectIdCollection();

                    var atRefTags = new System.Collections.Specialized.StringCollection();
                    BlockTableRecord blockTableRecord = trans.GetObject(recordId, OpenMode.ForRead, false, true) as BlockTableRecord;


                    if (blockTableRecord.IsDynamicBlock) //是动态块，从匿名块中获取
                    {

                        //13:24 2023/8/25 动态块也有可能含有该值
                        BrefIDs = blockTableRecord.GetBlockReferenceIds(true, true);

                        //获取所有的匿名块
                        ObjectIdCollection anonymousIds = blockTableRecord.GetAnonymousBlockIds();



                        foreach (ObjectId anonymousBtrId in anonymousIds)
                        {

                            //获取匿名块
                            BlockTableRecord anonymousBtr = (BlockTableRecord)trans.GetObject(anonymousBtrId, OpenMode.ForRead);

                            //获取块中的所有块参考
                            ObjectIdCollection blockRefIds = anonymousBtr.GetBlockReferenceIds(true, true);

                            foreach (ObjectId id in blockRefIds)
                            {
                                BrefIDs.Add(id);
                            }
                        }

                    }
                    else //非动态块
                    {
                        BrefIDs = blockTableRecord.GetBlockReferenceIds(true, true);
                    }




                    //统一删除块参照中的所有或指定的属性对象
                    if (tagNameLst == null)
                    {
                        foreach (ObjectId BrefID in BrefIDs)
                        {
                            blockReference = trans.GetObject(BrefID, OpenMode.ForRead, false, true) as BlockReference;

                            attributeCollection = blockReference.AttributeCollection;

                            foreach (ObjectId currentAttID in attributeCollection)
                            {



                                //7:53 2023/12/24 在这里，第三个参数为：openErased 因为这个情况有些属性已经被删除，需要设置为true才能打开，不然会报错：eWasErased
                                //attributeReference = trans.GetObject(currentAttID, OpenMode.ForWrite, false, true) as AttributeReference;
                                attributeReference = trans.GetObject(currentAttID, OpenMode.ForWrite, true, true) as AttributeReference;





                                //15:25 2023/12/22 报这个错： eWasErased 可能是没加这个判定
                                if (attributeReference.IsErased)
                                {
                                    continue;
                                }



                                attributeReference.Erase();
                            }
                             
                        }
                    }
                    else
                    {
                        foreach (ObjectId BrefID in BrefIDs)
                        {


                            blockReference = trans.GetObject(BrefID, OpenMode.ForRead, false, true) as BlockReference;



                            attributeCollection = blockReference.AttributeCollection;

                            foreach (ObjectId currentAttID in attributeCollection)
                            {

                                //7:53 2023/12/24 在这里，第三个参数为：openErased 因为这个情况有些属性已经被删除，需要设置为true才能打开，不然会报错：eWasErased
                                //attributeReference = trans.GetObject(currentAttID, OpenMode.ForWrite, false, true) as AttributeReference;
                                attributeReference = trans.GetObject(currentAttID, OpenMode.ForWrite, true, true) as AttributeReference;




                                //15:25 2023/12/22 报这个错： eWasErased 可能是没加这个判定
                                if (attributeReference.IsErased)
                                {
                                    continue;
                                }

                                if (tagNameLst.Exists(x => x.Equals(attributeReference.Tag, StringComparison.OrdinalIgnoreCase)))
                                {
                                    attributeReference.Erase();
                                }
                            }

                        }
                    }

                    trans.Commit();
                }
                catch (System.Exception ex)
                {
                    trans.Abort();
                    MessageBox.Show("同步块操作失败:" + ex.Message, "Tips");
                }
            }
        }








        /// <summary>
        /// 删除块表记录
        /// </summary>
        /// <param name="blkId">块表记录的ObjectId</param>
        /// <returns>如果成功，返回true，否则，返回false</returns>
        public bool EraseBlockTableRecord(ObjectId blkId)
        {
            //返回值
            bool blkIsErased = false;

            if (blkId.IsNull)
                return blkIsErased;

            Database db = blkId.Database;
            if (db == null)
                return blkIsErased;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {

                BlockTableRecord blk = (BlockTableRecord)tr.GetObject(blkId, OpenMode.ForRead);
                var blkRefs = blk.GetBlockReferenceIds(true, true);

                if (blkRefs == null || blkRefs.Count == 0) //不存在
                {
                    blk.UpgradeOpen();
                    blk.Erase();
                    blkIsErased = true;
                }
                tr.Commit();
            }
            return blkIsErased;
        }



        /// <summary>
        /// 删除块表记录
        /// </summary>
        /// <param name="btrName">块表记录的名称</param>
        /// <returns>如果成功，返回true，否则，返回false</returns>
        public bool EraseBlockTableRecord(string btrName)
        {
            //返回值
            bool blkIsErased = false;


            bool isExist = IsBlockExist(btrName);
            if (!isExist) //不存在
            {
                return blkIsErased;
            }


            ObjectId blkId = FindBlockTableRecordIdByName(btrName);

            return EraseBlockTableRecord(blkId);
        }











        /// <summary>
        /// 删除块表记录里面指定的对象类型列表
        /// </summary>
        /// <param name="btrName">块表记录的名称</param>
        /// <param name="dxfNameLst">允许选择的实体对象的类型列表，如直线为"line"，不区分大小写，如果为null或空，将允许所有对象类型，默认允许所有对象类型</param>
        /// <returns>如果成功，返回true，否则，返回false</returns>
        public bool EraseObjectsInBlockTableRecord(string btrName, List<string> dxfNameLst = null)
        {
            //返回值
            bool blkIsErased = false;


            bool isExist = IsBlockExist(btrName);
            if (!isExist) //不存在
            {
                return blkIsErased;
            }


            ObjectId blkId = FindBlockTableRecordIdByName(btrName);

            return EraseObjectsInBlockTableRecord(blkId, dxfNameLst);
        }



        /// <summary>
        /// 删除块表记录里面指定的对象类型列表
        /// </summary>
        /// <param name="blkId">块表记录的ObjectId</param>
        /// <param name="dxfNameLst">允许选择的实体对象的类型列表，如直线为"line"，不区分大小写，如果为null或空，将允许所有对象类型，默认允许所有对象类型</param>
        /// <returns>如果成功，返回true，否则，返回false</returns> 
        public bool EraseObjectsInBlockTableRecord(ObjectId blkId, List<string> dxfNameLst = null)
        {
            //返回值
            bool succeed = false;

            if (blkId.IsNull)
                return succeed;

            Database db = blkId.Database;
            if (db == null)
                return succeed;


            List<string> copyDxfNameLst = new List<string>();

            if (dxfNameLst != null && dxfNameLst.Count > 0)
            {
                dxfNameLst.ForEach(x => copyDxfNameLst.Add(x.ToLower()));
            }


            using (Transaction tr = db.TransactionManager.StartTransaction())
            {

                BlockTableRecord blk = (BlockTableRecord)tr.GetObject(blkId, OpenMode.ForWrite);

                foreach (var id in blk)
                {
                    if (copyDxfNameLst.Count == 0 ||
                       (copyDxfNameLst.Count > 0 && copyDxfNameLst.Contains(id.ObjectClass.DxfName.ToLower()))
                        )
                    {
                        Entity entity = tr.GetObject(id, OpenMode.ForWrite) as Entity;
                        entity.Erase();
                        entity.Dispose();
                    }
                }

                db.TransactionManager.QueueForGraphicsFlush();

                //m_document.TransactionManager.FlushGraphics();
                //m_document.Editor.UpdateScreen();

                tr.Commit();

                //ed.Regen();
                succeed = true;
            }
            return succeed;
        }




        /// <summary>
        /// 炸开块参照并清理块表记录
        /// </summary>
        /// <param name="blockReferenceId">块参照的ObjectId</param>
        /// <returns>如果成功，返回true,否则，返回false</returns>
        public List<ObjectId> ExplodeBlockReferenceAndEraseRecord(ObjectId blockReferenceId)
        {
            //返回值
            List<ObjectId> subEntityIdLst = new List<ObjectId>();

            Database db = blockReferenceId.Database;
            if (db == null)
            {
                return subEntityIdLst;
            }

            ObjectId recordId = ObjectId.Null;

            using (Transaction transaction = db.TransactionManager.StartTransaction())
            {
                if (transaction.GetObject(blockReferenceId, OpenMode.ForRead) is BlockReference blockReference)
                {
                    recordId = blockReference.BlockTableRecord;
                }
            }

            if (recordId.IsNull)
            {
                return subEntityIdLst;
            }

            subEntityIdLst = ExplodeBlockReference(blockReferenceId);

            if (subEntityIdLst.Count == 0)
            {
                return subEntityIdLst;
            }

            //以上没有返回，说明炸开成功

            //清理
            EraseBlockTableRecord(recordId);

            return subEntityIdLst;
        }






        /// <summary>
        /// 炸开块参照并清理块表记录
        /// </summary>
        /// <param name="blockReferenceId">块参照的ObjectId</param>
        /// <returns>如果成功，返回true,否则，返回false</returns>
        //public bool ExplodeBlockReferenceAndEraseRecord(ObjectId blockReferenceId)
        //{
        //    //返回值
        //    bool isSucceed = false;

        //    Database db = blockReferenceId.Database;
        //    if (db == null)
        //    {
        //        return isSucceed;
        //    }

        //    ObjectId recordId = ObjectId.Null;

        //    using (Transaction transaction = db.TransactionManager.StartTransaction())
        //    {
        //        if (transaction.GetObject(blockReferenceId, OpenMode.ForRead) is BlockReference blockReference)
        //        {
        //            recordId = blockReference.BlockTableRecord;
        //        }
        //    }

        //    if (recordId.IsNull)
        //    {
        //        return isSucceed;
        //    }

        //    List<ObjectId> subEntityIdLst = ExplodeBlockReference(blockReferenceId);

        //    if (subEntityIdLst.Count == 0)
        //    {
        //        return isSucceed;
        //    }

        //    //以上没有返回，说明炸开成功

        //    //清理
        //    isSucceed = EraseBlockTableRecord(recordId);

        //    return isSucceed;
        //}


        /// <summary>
        /// 炸开块参照
        /// </summary>
        /// <param name="blockReferenceId">块参照的ObjectId</param>
        /// <returns>炸开之后的对象ObjectId列表，如果失败，返回空的列表</returns>
        public List<ObjectId> ExplodeBlockReference(ObjectId blockReferenceId)
        {
            //返回值
            List<ObjectId> subEntityIdLst = new List<ObjectId>();

            Database db = blockReferenceId.Database;
            if (db == null)
            {
                return subEntityIdLst;
            }

            using (Transaction transaction = db.TransactionManager.StartTransaction())
            {
                try
                {
                    if (transaction.GetObject(blockReferenceId, OpenMode.ForWrite) is BlockReference blockReference)
                    {
                        DBObjectCollection dBObjectCollection = new DBObjectCollection();

                        //这一步 只是获取块里面的对象，不会对块有任何影响
                        blockReference.Explode(dBObjectCollection);


                        BlockTable acBlkTbl = transaction.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;



                        //10:36 2023/6/20 用这个的话 炸开的对象会在模型空间中，需要改一下 炸开到块参照所有的空间
                        //BlockTableRecord acBlkTblRec = transaction.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                        BlockTableRecord acBlkTblRec = transaction.GetObject(blockReference.BlockId, OpenMode.ForWrite) as BlockTableRecord;




                        //只有这一步才会添加所有对象
                        foreach (Entity item in dBObjectCollection)
                        {
                            ObjectId objectId = acBlkTblRec.AppendEntity(item);
                            transaction.AddNewlyCreatedDBObject(item, true);

                            subEntityIdLst.Add(objectId);
                        }


                        //炸开  使用这个方法，如果块里面有块 可能会有点问题，嵌套块有时候可以看见 但是没法选择
                        //blockReference.ExplodeToOwnerSpace();

                        blockReference.Erase();

                        transaction.Commit();
                    }

                }
                catch
                {
                    transaction.Abort();
                }
            }

            return subEntityIdLst;
        }





        /// <summary>
        /// 获取在块对象边界范围内的所有实体对象的ObjectId列表
        /// 注意：不考虑是在模型空间还是图纸空间中，所以需要提前切换到块对象所在的空间
        /// </summary>
        /// <param name="blockReferenceId">块对象的ObjectId</param>
        /// <param name="filter">过滤器，如果为null，将不过滤，默认不过滤</param>
        /// <param name="tolerance">误差，如果为负值，将会往里面偏这个距离，如果为正值，将会往外面偏这个距离，为0时，不考虑误差，默认不考虑</param>
        /// <returns>实体对象的ObjectId列表，如果操作有误或没有对象，将返回空的列表</returns>
        public List<ObjectId> GetEntityIdLstInsideBlock(ObjectId blockReferenceId, SelectionFilter filter = null, double tolerance = 0)
        {
            //返回值
            List<ObjectId> entityIdLst = new List<ObjectId>();

            if (m_document == null)
            {
                return entityIdLst;
            }


            ObjectTool objectTool = new ObjectTool(m_document);

            //先获取边界点列表
            Point3d[] pointArr = objectTool.GetEntityBoundingBoxPoints(blockReferenceId);


            //如果没有三个点及以上，就没法构成封闭范围了
            if (pointArr == null || pointArr.Length < 3)
            {
                return entityIdLst;
            }



            List<Point3d> newPointLst = new List<Point3d>(pointArr);
            if (tolerance > 0.1 || tolerance < -0.1) //因为是double值，用0来比较基本都会为true
            {
                newPointLst = GetNewPointLst(newPointLst, tolerance);
            }


            Point3dCollection point3DCollection = new Point3dCollection();
            newPointLst.ForEach(x => point3DCollection.Add(x));


            Editor editor = m_document.Editor;

            //在视图缩放之前记录当前的视图
            ViewTableRecord currentView = editor.GetCurrentView();

            BlockReference blockReference = objectTool.GetObject(blockReferenceId) as BlockReference;

            //先判断块对象所在的空间
            string spaceName = blockReference.BlockName.ToUpper();
            if (spaceName.Contains("MODEL"))
            {
                spaceName = "MODELSPACE";
            }
            else
            {
                spaceName = "PAPERSPACE";
            }

            //需要先将视口切换到块所在的模型空间或图纸空间
            //在图纸空间中不会有该对象，在图纸空间激活的模型空间中缩放到的范围也不一定是理想的效果
            int spaceIndex = m_document.Database.InModelSpaceOrPaperSpace();

            //记录空间是否已经切换
            bool hasSpaceSwitch = false;

            if (spaceIndex != 0 && spaceName == "MODELSPACE") //块在模型空间中，且当前视图不在模型空间当中
            {
                //切换到模型空间的模型空间
                m_document.SwitchToSpace();
                hasSpaceSwitch = true;

            }
            else if (spaceIndex == 0 && spaceName == "PAPERSPACE") //块在模型空间中，且当前视图不在模型空间当中
            {
                //切换到图纸空间中，视口没有被激活 
                m_document.SwitchToSpace(1);
                hasSpaceSwitch = true;
            }




            //缩放到所有对象可见，有时候图形范围太大，反倒会选不出来一些对象，设置到框选范围，因为会自动留一些误差，更为可靠
            //设置视图范围，以便能获取所有对象
            //dynamic acadApp = Application.AcadApplication;
            //acadApp.ZoomExtents();

            //先以边界的点画封闭多段线，再删除
            ObjectId boundingPolylineId = m_document.Database.AddPolyLine(point3DCollection, true, 0, spaceName);

            if (boundingPolylineId != ObjectId.Null) //创建成功
            {
                editor.ZoomObject(boundingPolylineId);


                //这里要删除创建的对象

                boundingPolylineId.EraseEntity();

            }


            PromptSelectionResult selectionresult = null;

            if (filter == null) //没有过滤器
            {
                selectionresult = editor.SelectCrossingPolygon(point3DCollection);
            }
            else //有过滤器
            {
                selectionresult = editor.SelectCrossingPolygon(point3DCollection, filter);
            }



            if (hasSpaceSwitch) //说明空间切换过了，需要切换到原来的状态
            {
                m_document.SwitchToSpace(spaceIndex);
            }



            //好像不管用  加事务也不管用
            //恢复视图范围
            editor.SetCurrentView(currentView);



            if (selectionresult.Status != PromptStatus.OK) //没有选上，直接返回
            {
                return entityIdLst;
            }

            var objectIdArr = selectionresult.Value.GetObjectIds();

            entityIdLst = new List<ObjectId>(objectIdArr);

            //需要去掉块本身
            entityIdLst.Remove(blockReferenceId);


            return entityIdLst;
        }




        /// <summary>
        /// 以误差为距离，点的两个相邻直线的角平分线为方向向量，获取新的点列表
        /// </summary>
        /// <param name="pointLst"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        private List<Point3d> GetNewPointLst(List<Point3d> pointLst, double tolerance = 100)
        {
            List<Point3d> newPointLst = new List<Point3d>();

            int count = pointLst.Count;
            for (int i = 0; i < count; i++)
            {
                int lastIndex = i - 1;
                int nextIndex = i + 1;
                if (i == 0) //第一个点，上一个点在最后一个点
                {
                    lastIndex = count - 1;
                }
                else if (i == count - 1) //最后一个点 下一个点在第一个点
                {
                    nextIndex = 0;
                }


                Point3d firstLineStartPoint = pointLst[lastIndex];
                Point3d firstLineEndPoint = pointLst[i];
                Point3d secondLineStartPoint = pointLst[nextIndex];
                Point3d secondLineEndPoint = pointLst[i];


                var dividedVector = Vector3dExtension.GetDividedVector(firstLineStartPoint, firstLineEndPoint, secondLineStartPoint, secondLineEndPoint);

                var newPoint = firstLineEndPoint.Add(dividedVector * tolerance);
                newPointLst.Add(newPoint);

            }

            return newPointLst;
        }






        /// <summary>
        /// 获取块内指定类型对象,dxfName对大小写不敏感
        /// </summary>
        /// <param name="blockReferenceId">块对象Id</param>
        /// <param name="dxfName">类型名称</param>
        /// <returns>实体对象的ObjectId，如果操作有误或没有对象，ObjectId.Null</returns>
        public ObjectId GetSubEntiyByDxfName(ObjectId blockReferenceId, string dxfName)
        {
            List<ObjectId> objectIdLst = GetSubObjects(blockReferenceId);
            ObjectId objectId = objectIdLst.Find(x => x.ObjectClass.DxfName.ToUpper() == dxfName.ToUpper());
            return objectId;
        }



        /// <summary>
        /// 获取块内指定类型对象,dxfName对大小写不敏感
        /// </summary>
        /// <param name="blockReferenceId">块对象Id</param>
        /// <param name="dxfNameLst">类型集合</param>
        /// <returns>实体对象的ObjectId，如果操作有误或没有对象，将返回空对象</returns>
        public List<ObjectId> GetSubEntiyByDxfName(ObjectId blockReferenceId, List<string> dxfNameLst)
        {
            List<ObjectId> objectIdLst = GetSubObjects(blockReferenceId);
            List<ObjectId> expectedObjectIdLst = objectIdLst.FindAll(x => dxfNameLst.Exists(y => x.ObjectClass.DxfName.ToUpper() == y.ToUpper()));
            return expectedObjectIdLst;
        }







        /// <summary>
        /// 获取块内所有子对象集合
        /// </summary>
        /// <param name="blockReferenceId">块对象Id</param>
        /// <returns>实体对象的ObjectId列表，如果操作有误或没有对象，将返回空的列表</returns>
        public List<ObjectId> GetSubObjects(ObjectId blockReferenceId)
        {
            //返回值
            List<ObjectId> objectIdLst = new List<ObjectId>();

            if (blockReferenceId.IsNull)
            {
                return objectIdLst;
            }


            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {

                    if (!(transaction.GetObject(blockReferenceId, OpenMode.ForRead) is BlockReference blockReference))
                    {
                        return objectIdLst;
                    }

                    //处理属性对象
                    foreach (ObjectId id in blockReference.AttributeCollection)
                    {
                        objectIdLst.Add(id);
                    }



                    //处理子对象

                    BlockTableRecord btr = transaction.GetObject(blockReference.BlockTableRecord, OpenMode.ForRead) as BlockTableRecord;
                    foreach (var Id in btr)
                    {
                        var entiy = transaction.GetObject(Id, OpenMode.ForRead) as Entity;
                        if (entiy is BlockReference subBlockReference)
                        {

                            List<ObjectId> subObjectIdLst = GetSubObjects(subBlockReference.Id);

                            objectIdLst.AddRange(subObjectIdLst);

                        }
                        else
                        {
                            objectIdLst.Add(Id);
                        }
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();
                }
            }
            return objectIdLst;
        }






        /// <summary>
        /// 获取点的实际世界坐标
        /// </summary>
        /// <param name="blockReferenceId">块对象Id</param>
        /// <param name="point3dLstInBlock">点列表</param>
        /// <returns>点对象的实际坐标，如果有误或吗对象返回null</returns>
        public List<Point3d> GetPointsToWCS(ObjectId blockReferenceId, List<Point3d> point3dLstInBlock)
        {
            List<Point3d> WCSPointlLst = new List<Point3d>();
            if (blockReferenceId.IsNull || point3dLstInBlock == null)
            {
                return WCSPointlLst;
            }
            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    BlockReference blockReference = transaction.GetObject(blockReferenceId, OpenMode.ForRead) as BlockReference;
                    Matrix3d xform = blockReference.BlockTransform;
                    foreach (Point3d point3dInBlock in point3dLstInBlock)
                    {
                        Point3d point1 = point3dInBlock.TransformBy(xform);
                        WCSPointlLst.Add(point1);
                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();
                }
            }
            return WCSPointlLst;
        }









        /// <summary>
        /// 获取点的实际世界坐标
        /// </summary>
        /// <param name="blockReferenceId">块对象Id</param>
        /// <param name="point3dInBlock">点</param>
        /// <returns>点对象的实际坐标，如果有误或吗对象返回null</returns>
        public Point3d? GetPointToWCS(ObjectId blockReferenceId, Point3d point3dInBlock)
        {
            if (blockReferenceId.IsNull)
            {
                return null;
            }
            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    BlockReference blockReference = transaction.GetObject(blockReferenceId, OpenMode.ForRead) as BlockReference;
                    Matrix3d xform = blockReference.BlockTransform;
                    Point3d point1 = point3dInBlock.TransformBy(xform);
                    transaction.Commit();
                    return point1;
                }
                catch
                {
                    transaction.Abort();
                    return null;
                }

            }
        }





        /// <summary>
        /// 获取点的实际世界坐标
        /// </summary>
        /// <param name="blockReferenceId">块对象Id</param>
        /// <param name="pointId">点对象Id</param>
        /// <returns>点对象的实际坐标，如果有误或吗对象返回null</returns>
        public Point3d? GetPointCoordinateInBlock(ObjectId blockReferenceId, ObjectId pointId)
        {
            //返回值
            if (blockReferenceId.IsNull || pointId.IsNull)
            {
                return null;
            }
            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    BlockReference blockReference = transaction.GetObject(blockReferenceId, OpenMode.ForRead) as BlockReference;
                    Matrix3d xform = blockReference.BlockTransform;
                    DBPoint point = transaction.GetObject(pointId, OpenMode.ForRead) as DBPoint;
                    Point3d point1 = point.Position.TransformBy(xform);
                    transaction.Commit();
                    return point1;
                }
                catch
                {
                    transaction.Abort();
                    return null;
                }
            }
        }
    }
}
