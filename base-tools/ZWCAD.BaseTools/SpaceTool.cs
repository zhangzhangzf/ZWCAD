using ZWCAD.BaseTools.Extension;
using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.Geometry;
//using Mrf.CSharp.BaseTools;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ZWCAD.BaseTools
{
    /// <summary>
    /// 空间（模型、图纸）工具
    /// </summary>
    public class SpaceTool
    {

        Document m_document;

        Database m_database;


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="document">文档对象</param>
        public SpaceTool(Document document)
        {
            m_document = document;
            m_database = m_document.Database;
        }


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="database">数据库</param>
        public SpaceTool(Database database)
        {
            m_database = database;
        }







        /// <summary>
        /// 将指定对象复制到指定空间中，坐标平移
        /// </summary>
        /// <param name="newSpaceId">新空间对象的ObjectId</param>
        /// <param name="objectId">对象的ObjectId</param>
        /// <param name="isDelete">是否删除源对象，默认删除</param>
        /// <returns>返回新生成对象的ObjectId，如果创建失败，返回ObjectId.Null，程序中将旧对象的属性（包括ObjectId）都指定到了新对象上</returns>
        public ObjectId MoveToNewSpace(ObjectId newSpaceId, ObjectId objectId, bool isDelete = true)
        {
            //返回值
            ObjectId newObjectId = ObjectId.Null;

            if (newSpaceId.IsNull || objectId.IsNull)
            {
                return newObjectId;
            }

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    if (transaction.GetObject(newSpaceId, OpenMode.ForWrite) is BlockTableRecord space)
                    {
                        if (transaction.GetObject(objectId, OpenMode.ForRead) is Entity entity)
                        {

                            Entity clone = entity.Clone() as Entity;

                            newObjectId= space.AppendEntity(clone);
                            transaction.AddNewlyCreatedDBObject(clone, true);

                            if (isDelete)
                            {
                                entity.UpgradeOpen();
                                entity.Erase(true);
                            }
                        }

                    }
                    transaction.Commit();

                }
                catch (System.Exception e)
                {
                    MessageBox.Show(e.Message);
                    transaction.Abort();
                }

            }

            return newObjectId;

        }




        /// <summary>
        /// 将指定对象复制到指定空间中，坐标平移
        /// </summary>
        /// <param name="objectId">对象的ObjectId</param>
        /// <param name="paperSpaceId">指定空间的ObjectId</param>
        /// <returns>返回新生成对象的ObjectId，如果创建失败，返回ObjectId.Null，程序中将旧对象的属性（包括ObjectId）都指定到了新对象上</returns>
        public ObjectId CopyToNewSpace(ObjectId objectId, ObjectId paperSpaceId)
        {
            //返回值
            ObjectId newObjectId = ObjectId.Null;

            if (m_document == null) //因为下面要用到
            {
                MessageBox.Show("图形对象为空", "Tips");
                return newObjectId;
            }


            //需要先切换到模型空间，配合entity.RecordGraphicsModified(true)一起用，不然更新的对象会不会立即显示在屏幕上，用editor.Regen()也不管用
            m_document.SwitchToSpace();


            using (Transaction transaction = m_document.TransactionManager.StartTransaction())
            {
                BlockTableRecord paperSpace = transaction.GetObject(paperSpaceId, OpenMode.ForWrite) as BlockTableRecord;

                try
                {
                    Entity entity = transaction.GetObject(objectId, OpenMode.ForRead) as Entity;

                    if (entity != null)
                    {

                        Entity clone = entity.Clone() as Entity;

                        paperSpace.AppendEntity(clone);
                        transaction.AddNewlyCreatedDBObject(clone, true);

                        clone.RecordGraphicsModified(true);
                        newObjectId = clone.Id;
                    }
                    transaction.Commit();

                }
                catch (System.Exception e)
                {
                    MessageBox.Show(e.Message);
                    transaction.Abort();
                }

            }

            return newObjectId;

        }


        /// <summary>
        /// 将指定对象列表复制到指定空间中，坐标平移
        /// </summary>
        /// <param name="objectIds">对象的ObjectId数组</param>
        /// <param name="paperSpaceId">指定空间的ObjectId</param>
        /// <returns>返回新生成对象的ObjectId列表，如果创建失败，返回空的列表，程序中将旧对象的属性（包括ObjectId）都指定到了新对象上</returns>
        public List<ObjectId> CopyToNewSpace(ObjectId[] objectIds, ObjectId paperSpaceId)
        {
            //返回值
            List<ObjectId> newObjectIdLst = new List<ObjectId>();

            if (m_document == null) //因为下面要用到
            {

                MessageBox.Show("图形对象为空", "Tips");
                return newObjectIdLst;
            }


            //需要先切换到模型空间，配合entity.RecordGraphicsModified(true)一起用，不然更新的对象会不会立即显示在屏幕上，用editor.Regen()也不管用
            m_document.SwitchToSpace();


            using (Transaction transaction = m_document.TransactionManager.StartTransaction())
            {
                BlockTableRecord paperSpace = transaction.GetObject(paperSpaceId, OpenMode.ForWrite) as BlockTableRecord;

                try
                {

                    foreach (var objectId in objectIds)
                    {
                        Entity entity = transaction.GetObject(objectId, OpenMode.ForRead) as Entity;

                        if (entity == null)
                        {
                            continue;
                        }

                        Entity clone = entity.Clone() as Entity;

                        paperSpace.AppendEntity(clone);
                        transaction.AddNewlyCreatedDBObject(clone, true);

                        newObjectIdLst.Add(clone.Id);

                        clone.RecordGraphicsModified(true);
                    }
                    transaction.Commit();

                }
                catch (System.Exception e)
                {
                    newObjectIdLst.Clear();
                    MessageBox.Show(e.Message);
                    transaction.Abort();
                }

            }

            return newObjectIdLst;

        }



        /// <summary>
        /// 将图纸空间中的对象移动到模型空间中，坐标平移
        /// </summary>
        /// <param name="objectIds">对象的ObjectId数组</param>
        /// <returns>返回新生成对象的ObjectId列表，如果创建失败，返回空的列表，程序中将旧对象的属性（包括ObjectId）都指定到了新对象上</returns>
        public List<ObjectId> MoveFromPaperSpaceToModelSpace(ObjectId[] objectIds)
        {
            //返回值
            List<ObjectId> newObjectIdLst = new List<ObjectId>();

            if (m_document == null) //因为下面要用到
            {

                MessageBox.Show("图形对象为空", "Tips");
                return newObjectIdLst;
            }



            Database database = m_database;


            //需要先切换到模型空间，配合entity.RecordGraphicsModified(true)一起用，不然更新的对象会不会立即显示在屏幕上，用editor.Regen()也不管用
            m_document.SwitchToSpace();


            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                ObjectId paperSpaceId = SymbolUtilityServices.GetBlockModelSpaceId(database);
                BlockTableRecord paperSpace = transaction.GetObject(paperSpaceId, OpenMode.ForWrite) as BlockTableRecord;

                try
                {

                    foreach (var objectId in objectIds)
                    {
                        Entity entity = transaction.GetObject(objectId, OpenMode.ForWrite) as Entity;

                        if (entity == null)
                        {
                            continue;
                        }

                        Entity clone = entity.Clone() as Entity;

                        paperSpace.AppendEntity(clone);
                        transaction.AddNewlyCreatedDBObject(clone, true);

                        newObjectIdLst.Add(clone.Id);

                        clone.RecordGraphicsModified(true);

                        entity.Erase();

                    }
                    transaction.Commit();

                }
                catch (System.Exception e)
                {
                    newObjectIdLst.Clear();
                    MessageBox.Show(e.Message);
                    transaction.Abort();
                }

            }


            return newObjectIdLst;

        }




        /// <summary>
        /// 将图纸空间中的对象移动到模型空间中，坐标平移
        /// </summary>
        /// <param name="objectIdLst">对象的ObjectId列表</param>
        /// <returns>返回新生成对象的ObjectId列表，如果创建失败，返回空的列表，程序中将旧对象的属性（包括ObjectId）都指定到了新对象上</returns>
        public List<ObjectId> MoveFromPaperSpaceToModelSpace(List<ObjectId> objectIdLst)
        {
            ObjectId[] objectIds = objectIdLst.ToArray();
            return MoveFromPaperSpaceToModelSpace(objectIds);
        }



        /// <summary>
        /// 获取某个空间中的所有指定类型的对象的ObjectId列表
        /// </summary>
        /// <param name="dxfNameLst">要获取的对象类型列表，不分大小写，如直线为"LINE"，如果为空，则获取所有类型</param>
        /// <param name="spaceName">模型空间或图纸空间，如果为"PAPERSPACE"（不分大小写），为图纸空间，如果指定的名称为其它的图纸空间名称，则为指定的图纸空间名称；为空或其它，为模型空间</param>
        /// <returns>获取对象的ObjectId列表，如果没有找到，返回空的列表</returns>
        public List<ObjectId> GetAllEntitiesInSpace(List<string> dxfNameLst = null, string spaceName = null)
        {
            //返回值
            List<ObjectId> returnObjectIdLst = new List<ObjectId>();

            Database database = m_database;


            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {

                // Open the Block table for read以读打开块表
                BlockTable acBlkTbl = transaction.GetObject(database.BlockTableId, OpenMode.ForRead) as BlockTable;


                //15:48 2023/6/20 还需要考虑到其它图纸空间的情况

                //是模型空间还是图纸空间

                if (!string.IsNullOrEmpty(spaceName) && spaceName.ToUpper() == "PAPERSPACE") //为图纸空间
                {
                    spaceName = BlockTableRecord.PaperSpace;
                }
                else if (!string.IsNullOrEmpty(spaceName) && acBlkTbl.Has(spaceName)) //其它图纸空间
                {

                }
                else  //为模型空间
                {
                    spaceName = BlockTableRecord.ModelSpace;

                }

                returnObjectIdLst=GetAllEntitiesInSpace(acBlkTbl[spaceName], dxfNameLst);


                transaction.Commit();

            }
            return returnObjectIdLst;

        }



        /// <summary>
        /// 获取某个空间中的所有指定类型的对象的ObjectId列表
        /// </summary>
        /// <param name="spaceId">空间对象的ObjectId</param>
        /// <param name="dxfNameLst">要获取的对象类型列表，不分大小写，如直线为"LINE"，如果为空，则获取所有类型</param>
        /// <returns>获取对象的ObjectId列表，如果没有找到，返回空的列表</returns>
        public List<ObjectId> GetAllEntitiesInSpace(ObjectId spaceId, List<string> dxfNameLst = null)
        {
            //返回值
            List<ObjectId> returnObjectIdLst = new List<ObjectId>();

            Database database = m_database;


            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {


                // Open the Block table record Modelspace for write
                // 以写打开块表记录模型空间
                BlockTableRecord acBlkTblRec = transaction.GetObject(spaceId, OpenMode.ForRead) as BlockTableRecord;


                if (acBlkTblRec == null)
                {
                    return returnObjectIdLst;
                }


                if (dxfNameLst == null || dxfNameLst.Count == 0)  //没有过滤器
                {

                    foreach (ObjectId objectId in acBlkTblRec)
                    {
                        returnObjectIdLst.Add(objectId);
                    }

                }

                else //含有对象过滤器
                {
                    //全部变为大写
                    List<string> tmpDxfNameLst = new List<string>();

                    dxfNameLst.ForEach(x => tmpDxfNameLst.Add(x.ToUpper()));

                    foreach (ObjectId objectId in acBlkTblRec)
                    {
                        string dxfName = objectId.ObjectClass.DxfName.ToUpper();
                        if (tmpDxfNameLst.Contains(dxfName))
                        {
                            returnObjectIdLst.Add(objectId);
                        }
                    }
                }


                transaction.Commit();

            }
            return returnObjectIdLst;

        }




        /// <summary>
        /// 获取某个空间中的指定类型的对象列表
        /// </summary>
        /// <param name="spaceName">空间名称，如果包含"PAPERSPACE"（不分大小写）为图纸空间，其它情况为模型空间</param>
        /// <returns>指定类型对象的列表，如果没有找到，返回空的列表</returns>
        public List<T> GetEntitiesInSpace<T>(string spaceName = null)
        {
            //返回值
            List<T> returnObjectIdLst = new List<T>();

            Database database = m_database;


            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {


                // Open the Block table for read以读打开块表
                BlockTable acBlkTbl = transaction.GetObject(database.BlockTableId, OpenMode.ForRead) as BlockTable;



                //15:48 2023/6/20 还需要考虑到其它图纸空间的情况

                //是模型空间还是图纸空间

                if (!string.IsNullOrEmpty(spaceName) && spaceName.ToUpper() == "PAPERSPACE") //为图纸空间
                {
                    spaceName = BlockTableRecord.PaperSpace;
                }
                else if (!string.IsNullOrEmpty(spaceName) && acBlkTbl.Has(spaceName)) //其它图纸空间
                {

                }
                else  //为模型空间
                {
                    spaceName = BlockTableRecord.ModelSpace;

                }

                returnObjectIdLst= GetEntitiesInSpace<T>(acBlkTbl[spaceName]);


                transaction.Commit();

            }
            return returnObjectIdLst;

        }



        /// <summary>
        /// 获取某个空间中的指定类型的对象列表
        /// </summary>
        /// <param name="spaceId">空间对象的ObjectId</param>
        /// <returns>指定类型对象的列表，如果没有找到，返回空的列表</returns>
        public List<T> GetEntitiesInSpace<T>(ObjectId spaceId)
        {
            //返回值
            List<T> returnObjectIdLst = new List<T>();

            Database database = m_database;


            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {

                // Open the Block table record Modelspace for write
                // 以写打开块表记录模型空间
                if (transaction.GetObject(spaceId, OpenMode.ForRead) is BlockTableRecord acBlkTblRec)
                {

                    foreach (ObjectId objectId in acBlkTblRec)
                    {

                        DBObject dbobj = transaction.GetObject(objectId, OpenMode.ForRead);
                        if (dbobj is T entity)
                        {
                            returnObjectIdLst.Add(entity);
                        }

                    }
                }

                transaction.Commit();

            }
            return returnObjectIdLst;

        }





        /// <summary>
        /// 获取某个空间中的所有不包含指定类型的对象
        /// </summary>
        /// <param name="exceptDxfNameLst">要排除的对象类型列表，不分大小写，如直线为"LINE"，如果为空，则获取所有类型</param>
        /// <param name="spaceName">空间名称，如果包含"PAPERSPACE"（不分大小写）为图纸空间，其它情况为模型空间</param>
        /// <returns>获取对象的ObjectId列表，如果没有找到，返回空的列表</returns>
        public List<ObjectId> GetAllEntitiesInSpaceExcept(List<string> exceptDxfNameLst, string spaceName = null)
        {
            //返回值
            List<ObjectId> returnObjectIdLst = new List<ObjectId>();


            if (exceptDxfNameLst == null || exceptDxfNameLst.Count == 0)  //没有过滤器,直接返回空的
            {
                return returnObjectIdLst;
            }



            Database database = m_database;


            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {

                // Open the Block table for read以读打开块表
                BlockTable acBlkTbl = transaction.GetObject(database.BlockTableId, OpenMode.ForRead) as BlockTable;



                //15:48 2023/6/20 还需要考虑到其它图纸空间的情况

                //是模型空间还是图纸空间

                if (!string.IsNullOrEmpty(spaceName) && spaceName.ToUpper() == "PAPERSPACE") //为图纸空间
                {
                    spaceName = BlockTableRecord.PaperSpace;
                }
                else if (!string.IsNullOrEmpty(spaceName) && acBlkTbl.Has(spaceName)) //其它图纸空间
                {

                }
                else  //为模型空间
                {
                    spaceName = BlockTableRecord.ModelSpace;

                }




                // Open the Block table record Modelspace for write
                // 以写打开块表记录模型空间
                BlockTableRecord acBlkTblRec = transaction.GetObject(acBlkTbl[spaceName],
                    OpenMode.ForRead) as BlockTableRecord;




                //全部变为大写
                List<string> tmpDxfNameLst = new List<string>();

                exceptDxfNameLst.ForEach(x => tmpDxfNameLst.Add(x.ToUpper()));

                foreach (ObjectId objectId in acBlkTblRec)
                {
                    string dxfName = objectId.ObjectClass.DxfName.ToUpper();
                    if (!tmpDxfNameLst.Contains(dxfName))  //不包含
                    {
                        returnObjectIdLst.Add(objectId);
                    }
                }


            }
            return returnObjectIdLst;

        }






        /// <summary>
        /// 获取空间边界的最小点和最大点 
        /// </summary>
        /// <param name="spaceId">所有对象的ObjectId集合</param>
        /// <returns>最小点和最大点组成的数组Point3d[],如果不存在，会返回null</returns>
        public Point3d[] GetSpaceMinPointAndMaxPoint(ObjectId spaceId)
        {

            //返回值
            Point3d[] minAndMaxPointArr = null;

            //获取空间所有对象
            List<ObjectId> allEntityIdLst = GetAllEntitiesInSpace(spaceId);

            if (allEntityIdLst.Count == 0)
            {
                return minAndMaxPointArr;
            }


            //获取所有对象的边界
            ObjectTool objectTool = new ObjectTool(m_database);

            minAndMaxPointArr=objectTool.GetDBObjectsMinPointAndMaxPoint(allEntityIdLst);

            return minAndMaxPointArr;

        }











        /// <summary>
        /// 获取空间边界的四个点的坐标，分为为左下、左上、右上、右下
        /// </summary>
        /// <param name="spaceId">实体的ObjectId</param>
        /// <returns> Point3d[],如果读取错误，返回null</returns>
        public Point3d[] GetSpaceBoundingBoxPoints(ObjectId spaceId)
        {
            //返回值
            Point3d[] boundingBoxPoints = new Point3d[4];

            Point3d[] minAndMaxPointArr = GetSpaceMinPointAndMaxPoint(spaceId);
            if (minAndMaxPointArr==null)
            {
                return null;
            }



            Point3d minPoint = minAndMaxPointArr[0];
            Point3d maxPoint = minAndMaxPointArr[1];
            Point3d leftTopPoint = new Point3d(minPoint.X, maxPoint.Y, minPoint.Z);
            Point3d rightBottomPoint = new Point3d(maxPoint.X, minPoint.Y, minPoint.Z);

            boundingBoxPoints[0] = minPoint;
            boundingBoxPoints[1] = leftTopPoint;
            boundingBoxPoints[2] = maxPoint;
            boundingBoxPoints[3] = rightBottomPoint;
            return boundingBoxPoints;


        }







        /// <summary>
        /// 返回空间边界的某个点坐标
        /// </summary>
        /// <param name="spaceId">空间对象的ObjectId</param>
        /// <param name="index">index=0 左下; 1 左上; 2 右上; 3 右下</param>
        /// <returns>点坐标，如果读取错误，返回null</returns>
        public Point3d? GetSpaceBoundingBoxPoint(ObjectId spaceId, int index)
        {
            Point3d[] boundingBoxPoints = GetSpaceBoundingBoxPoints(spaceId);

            if (boundingBoxPoints == null)
            {
                return null;
            }

            return boundingBoxPoints[index];


        }





        /// <summary>
        /// 获取空间边界的最大宽度和最大高度
        /// </summary>
        /// <param name="spaceId">空间对象的ObjectId</param>
        /// <returns>宽和高组成的double[]，如果读取不成功，返回null</returns>
        public double[] GetSpaceBoundingWidthAndHeight(ObjectId spaceId)
        {

            Point3d[] minAndMaxPointArr = GetSpaceMinPointAndMaxPoint(spaceId);

            if (minAndMaxPointArr == null)
            {
                return null;
            }

            Point3d minPoint = minAndMaxPointArr[0];
            Point3d maxPoint = minAndMaxPointArr[1];


            double width = maxPoint.X - minPoint.X;
            double height = maxPoint.Y - minPoint.Y;
            return new double[] { width, height };

        }




        /// <summary>
        /// 获取空间对象的宽度
        /// </summary>
        /// <param name="spaceId">空间对象的ObjectId</param>
        /// <returns>宽度，如果读取不成功，返回double.NaN</returns>
        public double GetSpaceBoundingWidth(ObjectId spaceId)
        {
            double[] widthAndHeight = GetSpaceBoundingWidthAndHeight(spaceId);
            if (widthAndHeight == null)
            {
                return double.NaN;
            }

            return widthAndHeight[0];
        }





        /// <summary>
        /// 获取空间对象的高度
        /// </summary>
        /// <param name="spaceId">空间对象的ObjectId</param>
        /// <returns>高度，如果读取不成功，返回double.NaN</returns>
        public double GetSpaceBoundingHeight(ObjectId spaceId)
        {
            double[] widthAndHeight = GetSpaceBoundingWidthAndHeight(spaceId);
            if (widthAndHeight == null)
            {
                return double.NaN;
            }

            return widthAndHeight[1];

        }




        /// <summary>
        /// 获取空间的名称
        /// </summary>
        /// <param name="spaceId">空间对象的ObjectId</param>
        /// <returns>空间对象的名称，如果没有找到，返回""</returns>
        public string GetSpaceName(ObjectId spaceId)
        {
            //返回值
            string spaceName = "";
            Database database = m_database;


            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {

                // Open the Block table record Modelspace for write
                // 以写打开块表记录模型空间
                if (transaction.GetObject(spaceId, OpenMode.ForRead) is BlockTableRecord acBlkTblRec)
                {
                    spaceName=     acBlkTblRec.Name;

                }

                transaction.Commit();
            }
            return spaceName;
        }








        /// <summary>
        /// 获取模型空间、图纸空间或指定的其它图纸空间
        /// </summary>
        /// <param name="spaceName">模型空间或图纸空间，如果为"PAPERSPACE"（不分大小写），为图纸空间，如果指定的名称为其它的图纸空间名称，则为指定的图纸空间名称；为空或其它，为模型空间</param>
        /// <returns>空间的ObjectId,如果创建失败，返回ObjectId.Null</returns>
        public ObjectId GetSpaceByName(string spaceName = "MODELSPACE")
        {
            //返回值
            ObjectId spaceId = ObjectId.Null;

            //开启事务处理
            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                //打开块表
                BlockTable blockTable = (BlockTable)transaction.GetObject(m_database.BlockTableId, OpenMode.ForRead);

                //15:48 2023/6/20 还需要考虑到其它图纸空间的情况

                //是模型空间还是图纸空间

                if (!string.IsNullOrEmpty(spaceName) && spaceName.ToUpper() == "PAPERSPACE") //为图纸空间
                {
                    spaceName = BlockTableRecord.PaperSpace;
                }
                else if (!string.IsNullOrEmpty(spaceName) && blockTable.Has(spaceName)) //其它图纸空间
                {

                }
                else  //为模型空间
                {
                    spaceName = BlockTableRecord.ModelSpace;

                }

                spaceId = blockTable[spaceName];


                transaction.Commit();
            }
            return spaceId;
        }



        /// <summary> 
        /// 获取所有的空间对象名称列表 包括模型空间、图纸空间和所有的块
        /// </summary>
        /// <returns>所有布局对象的名称列表，如果没有，返回空的列表</returns>
        public List<string> GetAllSpaceNames()
        {
            //返回值
            List<string> spaceNameLst = new List<string>();


            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)transaction.GetObject(m_database.BlockTableId, OpenMode.ForRead);

                foreach (ObjectId objectId in bt)
                {
                    BlockTableRecord record = transaction.GetObject(objectId, OpenMode.ForRead) as BlockTableRecord;

                    spaceNameLst.Add(record.Name);
                }

                transaction.Commit();
            }


            //MessageBox.Show(string.Join(",", spaceNameLst));

            return spaceNameLst;

        }
        /// <summary> 
        /// 获取所有的空间对象列表 包括模型空间、图纸空间和所有的块
        /// </summary>
        /// <returns>所有布局对象的ObjectId列表，如果没有，返回空的列表</returns>
        public List<ObjectId> GetAllSpaces()
        {
            //返回值
            List<ObjectId> objectIdLst = new List<ObjectId>();


            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)transaction.GetObject(m_database.BlockTableId, OpenMode.ForRead);

                foreach (ObjectId objectId in bt)
                {
                    objectIdLst.Add(objectId);
                }

                transaction.Commit();
            }

            return objectIdLst;

        }






        /// <summary>
        /// 获取指定实体对象所在的空间对象
        /// </summary>
        /// <param name="entityId">实体对象的ObjectId</param>
        /// <returns>空间对象的ObjectId，如果失败，返回ObjectId.Null</returns>
        public ObjectId GetSpaceByEntity(ObjectId entityId)
        {
            //返回值
            ObjectId spaceId = ObjectId.Null;

            if (entityId.IsNull)
            {
                return spaceId;
            }

            Database database = entityId.Database;

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                try
                {
                    if (transaction.GetObject(entityId, OpenMode.ForRead) is Entity entity)
                    {
                        spaceId=entity.BlockId;
                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();
                }
            }

            return spaceId;

        }

        
        /// <summary>
        /// 获取指定实体对象所在的空间对象
        /// </summary>
        /// <param name="entityId">实体对象的ObjectId</param>
        /// <returns>空间对象的ObjectId，如果失败，返回ObjectId.Null</returns>
        public string GetSpaceNameByEntity(ObjectId entityId)
        {
            //返回值
            string spaceName = "";

            if (entityId.IsNull)
            {
                return spaceName;
            }

            Database database = entityId.Database;

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                try
                {
                    if (transaction.GetObject(entityId, OpenMode.ForRead) is Entity entity)
                    {
                        spaceName=entity.BlockName;
                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();
                }
            }

            return spaceName;

        }







    }


}
