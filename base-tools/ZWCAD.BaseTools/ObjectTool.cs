using ZWCAD.BaseTools.Extension;
using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.Colors;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;
using ZwSoft.ZwCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZWCAD.BaseTools
{
    /// <summary>
    /// 实体对象工具
    /// </summary>
    public class ObjectTool
    {
        Document m_document;

        Database m_database;


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="document">文档对象</param>
        public ObjectTool(Document document)
        {
            m_document = document;
            m_database = m_document.Database;

        }


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="database">数据库对象</param>
        public ObjectTool(Database database)
        {
            m_database = database;
        }


        /// <summary>
        /// 获取对象
        /// </summary>
        /// <param name="id">对象的ObjectId</param>
        /// <param name="openMode">打开模式</param>
        /// <returns>对象DBobject</returns>
        public DBObject GetObject(ObjectId id, OpenMode openMode = OpenMode.ForRead)
        {
            DBObject obj = null;




            //using (Transaction transaction = m_document.TransactionManager.StartTransaction())
            //{

            using (Transaction transaction = id.Database.TransactionManager.StartTransaction())
            {


                obj = transaction.GetObject(id, openMode, true);


                //transaction.Commit();
            }
            return obj;
        }


















        /// <summary>
        /// 从对象的ObjectdId列表中获取获取其中某种类型的对象列表
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="objectIdLst">对象的ObjectId列表</param>
        /// <returns>指定类型的对象列表，如果没有，返回空的列表</returns>
        public List<T> GetObjectLst<T>(List<ObjectId> objectIdLst)
        {
            //返回值
            List<T> objectLst = new List<T>();


            Database database = m_database;

            using (Transaction tr = database.TransactionManager.StartTransaction())
            {
                foreach (ObjectId objectId in objectIdLst)
                {
                    DBObject dbobj = tr.GetObject(objectId, OpenMode.ForRead);
                    if (dbobj is T entity)
                    {
                        objectLst.Add(entity);
                    }
                }
            }

            return objectLst;
        }







        /// <summary>
        /// 将对象设置亮显
        /// </summary>
        /// <param name="objectIds">对象的ObjectId列表</param>
        public void SetHighLight(List<ObjectId> objectIds)
        {
            foreach (ObjectId objectId in objectIds)
            {
                SetHighLight(objectId);
            }
        }



        /// <summary>
        /// 将对象设置亮显
        /// </summary>
        /// <param name="objectIds">对象的ObjectId列表</param>
        public void SetHighLight(ObjectId[] objectIds)
        {
            foreach (ObjectId objectId in objectIds)
            {
                SetHighLight(objectId);
            }
        }





        /// <summary>
        /// 将对象设置亮显
        /// </summary>
        /// <param name="objectId">对象的ObjectId</param>
        public void SetHighLight(ObjectId objectId)
        {

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                DBObject dBObject = transaction.GetObject(objectId, OpenMode.ForWrite);

                try
                {

                    if (dBObject is Entity entity)
                    {
                        entity.Highlight();
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
        /// 将对象设置为亮显
        /// </summary>
        /// <param name="entity">对象实体</param>
        public void SetHighLight(Entity entity)
        {
            entity.Highlight();
        }



        /// <summary>
        /// 将对象设置为不亮显
        /// </summary>
        /// <param name="entity">对象实体</param>
        public void SetUnHighLight(Entity entity)
        {
            entity.Unhighlight();
        }




        /// <summary>
        /// 获取数据库模型空间的所有图元
        /// </summary>
        /// <returns>ObjectIdCollection</returns>
        public ObjectIdCollection GetDbModelSpaceEntities()
        {
            //返回值
            ObjectIdCollection objectIdCollection = new ObjectIdCollection();

            Database database = m_database;

            using (Transaction tr = database.TransactionManager.StartTransaction())
            {
                BlockTable bt = tr.GetObject(database.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modelSpace = tr.GetObject(
                    bt[BlockTableRecord.ModelSpace],
                    OpenMode.ForRead) as BlockTableRecord;

                foreach (ObjectId objectId in modelSpace)
                {
                    DBObject dbobj = tr.GetObject(objectId, OpenMode.ForRead);
                    if (dbobj is Entity)
                    {
                        objectIdCollection.Add(objectId);
                    }
                }
            }

            return objectIdCollection;
        }




        /// <summary>
        /// 获取实体边界的最小点和最大点
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns>最小点和最大点组成的数组Point3d,如果读取错误，返回null</returns>
        public Point3d[] GetEntityMinPointAndMaxPoint(Entity entity)
        {

            Extents3d? extents3D = entity.GetEntityExtents3d();

            if (extents3D == null)
            {
                return null;
            }


            Point3d minPoint = extents3D.Value.MinPoint;
            Point3d maxPoint = extents3D.Value.MaxPoint;
            return new Point3d[] { minPoint, maxPoint };

        }




        /// <summary>
        /// 获取实体边界的最小点和最大点
        /// </summary>
        /// <param name="objectId">对象的ObjectId</param>
        /// <returns>最小点和最大点组成的数组Point3d[],如果读取错误，返回null</returns>
        public Point3d[] GetEntityMinPointAndMaxPoint(ObjectId objectId)
        {
            Entity entity = GetObject(objectId) as Entity;

            return GetEntityMinPointAndMaxPoint(entity);
        }

















        /// <summary>
        /// 获取实体边界的最小点
        /// </summary>
        /// <param name="objectId">对象的ObjectId</param>
        /// <returns>最小点Point3d,如果读取错误，返回null</returns>
        public Point3d? GetEntityMinPoint(ObjectId objectId)
        {
            Entity entity = GetObject(objectId) as Entity;

            return entity.GetEntityMinPoint();
        }




        /// <summary>
        /// 获取实体边界的最大点
        /// </summary>
        /// <param name="objectId">对象的ObjectId</param>
        /// <returns>最大点Point3d,如果读取错误，返回null</returns>
        public Point3d? GetEntityMaxPoint(ObjectId objectId)
        {
            Entity entity = GetObject(objectId) as Entity;

            return entity.GetEntityMaxPoint();
        }







        /// <summary>
        /// 获取实体边界的质心
        /// </summary>
        /// <param name="objectId">实体的ObjectId</param>
        /// <returns> Point3d,如果读取错误，返回null</returns>
        public Point3d? GetEntityCentroid(ObjectId objectId)
        {
            Point3d[] minPointAndMaxPoint = GetEntityMinPointAndMaxPoint(objectId);

            if (minPointAndMaxPoint == null) //读取错误
            {
                return null;
            }

            Point3d minPoint = minPointAndMaxPoint[0];
            Point3d maxPoint = minPointAndMaxPoint[1];

            Point3d centroid = new Point3d((minPoint.X+maxPoint.X)/2, (minPoint.Y+maxPoint.Y)/2, (minPoint.Z+maxPoint.Z)/2);

            return centroid;
        }











        /// <summary>
        /// 获取实体边界的四个点的坐标，分为为左下、左上、右上、右下
        /// </summary>
        /// <param name="objectId">实体的ObjectId</param>
        /// <returns> Point3d[],如果读取错误，返回null</returns>
        public Point3d[] GetEntityBoundingBoxPoints(ObjectId objectId)
        {
            Point3d[] minPointAndMaxPoint = GetEntityMinPointAndMaxPoint(objectId);

            if (minPointAndMaxPoint == null) //读取错误
            {
                return null;
            }



            Point3d minPoint = minPointAndMaxPoint[0];
            Point3d maxPoint = minPointAndMaxPoint[1];
            Point3d leftTopPoint = new Point3d(minPoint.X, maxPoint.Y, minPoint.Z);
            Point3d rightBottomPoint = new Point3d(maxPoint.X, minPoint.Y, minPoint.Z);

            //返回值
            Point3d[] boundingBoxPoints = new Point3d[4];
            boundingBoxPoints[0] = minPoint;
            boundingBoxPoints[1] = leftTopPoint;
            boundingBoxPoints[2] = maxPoint;
            boundingBoxPoints[3] = rightBottomPoint;
            return boundingBoxPoints;

        }




        /// <summary>
        /// 返回实体边界的某个点坐标
        /// </summary>
        /// <param name="objectId">实体的ObjectId</param>
        /// <param name="index">index=0 左下; 1 左上; 2 右上; 3 右下</param>
        /// <returns>点坐标，如果读取错误，返回null</returns>
        public Point3d? GetEntityBoundingBoxPoint(ObjectId objectId, int index)
        {
            Point3d[] boundingBoxPoints = GetEntityBoundingBoxPoints(objectId);

            if (boundingBoxPoints == null)
            {
                return null;
            }

            return boundingBoxPoints[index];


        }


        /// <summary>
        /// 获取DBObject对象的最小点和最大点
        /// </summary>
        /// <param name="objectId">DBOject对象的ObjectId</param>
        /// <returns>最小点和最大点组成的数组Point3d[]，如果不存在，会返回null</returns>
        public Point3d[] GetDBObjectMinPointAndMaxPoint(ObjectId objectId)
        {
            DBObject dBObject = GetObject(objectId);

            Extents3d? extents3d;

            if (dBObject is Entity entity)
            {
                extents3d = entity.GetEntityExtents3d();
            }
            else
            {
                extents3d = dBObject.Bounds;
            }


            //extents3d可能会为null

            if (extents3d == null)
            {
                return null;
            }

            Point3d minPoint = ((Extents3d)extents3d).MinPoint;
            Point3d maxPoint = ((Extents3d)extents3d).MinPoint;

            return new Point3d[] { minPoint, maxPoint };
        }





        /// <summary>
        /// 获取能包住实体边界的宽度和高度
        /// </summary>
        /// <param name="objectIdLst">所有对象的ObjectId列表</param>
        /// <returns>宽度和高度组成的数组double[],如果不存在，会返回null</returns>
        public double[] GetDBObjectsWidthAndHeight(List<ObjectId> objectIdLst)
        {
            //返回值
            double[] widthAndHeight = new double[2];

            Point3d[] minPointAndMaxPoint = GetDBObjectsMinPointAndMaxPoint(objectIdLst);

            if (minPointAndMaxPoint == null)
            {
                return null;
            }


            Point3d minPoint = minPointAndMaxPoint[0];
            Point3d maxPoint = minPointAndMaxPoint[1];

            double width = maxPoint.X - minPoint.X;
            double height = maxPoint.Y - minPoint.Y;

            widthAndHeight[0] = width;
            widthAndHeight[1] = height;

            return widthAndHeight;
        }





        /// <summary>
        /// 获取实体边界的最小点
        /// </summary>
        /// <param name="objectIdLst">所有对象的ObjectId列表</param>
        /// <returns>最小点，如果不存在，会返回null</returns>
        public Point3d? GetDBObjectsMinPoint(List<ObjectId> objectIdLst)
        {
            Point3d[] minPointAndMaxPoint = GetDBObjectsMinPointAndMaxPoint(objectIdLst);
            if (minPointAndMaxPoint == null)
            {
                return null;
            }

            return minPointAndMaxPoint[0];
        }

        /// <summary>
        /// 获取实体边界的最大点
        /// </summary>
        /// <param name="objectIdLst">所有对象的ObjectId列表</param>
        /// <returns>最大点，如果不存在，会返回null</returns>
        public Point3d? GetDBObjectsMaxPoint(List<ObjectId> objectIdLst)
        {
            Point3d[] minPointAndMaxPoint = GetDBObjectsMinPointAndMaxPoint(objectIdLst);
            if (minPointAndMaxPoint == null)
            {
                return null;
            }

            return minPointAndMaxPoint[1];
        }





        /// <summary>
        /// 获取实体边界的最小点和最大点
        /// </summary>
        /// <param name="objectIdLst">所有对象的ObjectId列表</param>
        /// <returns>最小点和最大点组成的数组Point3d[],如果不存在，会返回null</returns>
        public Point3d[] GetDBObjectsMinPointAndMaxPoint(List<ObjectId> objectIdLst)
        {
            ObjectIdCollection objectIdCollection = new ObjectIdCollection();
            objectIdLst.ForEach(x => objectIdCollection.Add(x));

            return GetDBObjectsMinPointAndMaxPoint(objectIdCollection);
        }




        /// <summary>
        /// 获取实体列表边界的某个点坐标，分为为左下、左上、右上、右下
        /// </summary>
        /// <param name="objectIdLst">所有对象的ObjectId列表</param>
        /// <param name="index">index=0 左下; 1 左上; 2 右上; 3 右下</param>
        /// <returns>点坐标，如果读取错误，返回null</returns>

        public Point3d? GetDBObjectsBoundingBoxPoint(List<ObjectId> objectIdLst, int index)
        {
            Point3d[] boundingBoxPoints = GetDBObjectsBoundingBoxPoints(objectIdLst);
            if (boundingBoxPoints == null)
            {
                return null;
            }
            return boundingBoxPoints[index];
        }





        /// <summary>
        /// 获取实体列表边界的四个点的坐标，分为为左下、左上、右上、右下
        /// </summary>
        /// <param name="objectIdLst">所有对象的ObjectId列表</param>
        /// <returns>Point3d[],如果读取错误，返回null</returns>
        public Point3d[] GetDBObjectsBoundingBoxPoints(List<ObjectId> objectIdLst)
        {

            Point3d[] minPointAndMaxPoint = GetDBObjectsMinPointAndMaxPoint(objectIdLst);

            if (minPointAndMaxPoint == null) //读取错误
            {
                return null;
            }



            Point3d minPoint = minPointAndMaxPoint[0];
            Point3d maxPoint = minPointAndMaxPoint[1];
            Point3d leftTopPoint = new Point3d(minPoint.X, maxPoint.Y, minPoint.Z);
            Point3d rightBottomPoint = new Point3d(maxPoint.X, minPoint.Y, minPoint.Z);

            //返回值
            Point3d[] boundingBoxPoints = new Point3d[4];
            boundingBoxPoints[0] = minPoint;
            boundingBoxPoints[1] = leftTopPoint;
            boundingBoxPoints[2] = maxPoint;
            boundingBoxPoints[3] = rightBottomPoint;
            return boundingBoxPoints;



        }





        /// <summary>
        /// 获取实体列表边界的质心
        /// </summary>
        /// <param name="objectIdLst">所有对象的ObjectId列表</param>
        /// <returns>Point3d,如果读取错误，返回null</returns>
        public Point3d? GetDBObjectsCentroid(List<ObjectId> objectIdLst)
        {

            Point3d[] minPointAndMaxPoint = GetDBObjectsMinPointAndMaxPoint(objectIdLst);

            if (minPointAndMaxPoint == null) //读取错误
            {
                return null;
            }


            Point3d minPoint = minPointAndMaxPoint[0];
            Point3d maxPoint = minPointAndMaxPoint[1];

            Point3d centroid = new Point3d((minPoint.X+maxPoint.X)/2, (minPoint.Y+maxPoint.Y)/2, (minPoint.Z+maxPoint.Z)/2);

            return centroid;

        }







            /// <summary>
            /// 获取实体边界的最小点和最大点
            /// </summary>
            /// <param name="objectIdArr">所有对象的ObjectId数组</param>
            /// <returns>最小点和最大点组成的数组Point3d[],如果不存在，会返回null</returns>
            public Point3d[] GetDBObjectsMinPointAndMaxPoint(ObjectId[] objectIdArr)
        {
            ObjectIdCollection objectIdCollection = new ObjectIdCollection(objectIdArr);

            return GetDBObjectsMinPointAndMaxPoint(objectIdCollection);
        }



        /// <summary>
        /// 获取实体边界的最小点和最大点   15:59 2023/6/5 可以用
        /// </summary>
        /// <param name="objectIdCollection">所有对象的ObjectId集合</param>
        /// <returns>最小点和最大点组成的数组Point3d[],如果不存在，会返回null</returns>
        public Point3d[] GetDBObjectsMinPointAndMaxPoint(ObjectIdCollection objectIdCollection)
        {
            //返回值
            Point3d[] minAndMaxPointArr = null;


            Extents3d extents3D = new Extents3d();

            bool flag = false;

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    foreach (ObjectId item in objectIdCollection)
                    {

                        DBObject dBObject = transaction.GetObject(item, OpenMode.ForRead);

                        if (dBObject is Entity entity)
                        {

                            Extents3d? entityExtents3d = entity.GetEntityExtents3d();

                            if (entityExtents3d.HasValue)
                            {
                                extents3D.AddExtents(entityExtents3d.Value);
                                flag = true;
                            }

                        }
                        else
                        {

                        }
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();
                }

            }


            if (flag)
            {
                Point3d minPoint = extents3D.MinPoint;
                Point3d maxPoint = extents3D.MaxPoint;
                minAndMaxPointArr = new Point3d[] { minPoint, maxPoint };
            }

            return minAndMaxPointArr;

        }




        ///// <summary>
        ///// 获取实体边界的最小点和最大点   15:59 2023/6/5 可以用
        ///// </summary>
        ///// <param name="objectIdCollection">所有对象的ObjectId集合</param>
        ///// <returns>最小点和最大点组成的数组Point3d[],如果不存在，会返回null</returns>
        //public Point3d[] GetDBObjectsMinPointAndMaxPoint(ObjectIdCollection objectIdCollection)
        //{

        //    //先创建组
        //    GroupTool groupTool = new GroupTool(m_database);


        //    ObjectId objectId = groupTool.CreateGroup(objectIdCollection);

        //    //获取组的边界点
        //    Point3d[] point3Ds = GetDBObjectMinPointAndMaxPoint(objectId);

        //    //将对象集合从组中分解，并删掉组

        //    groupTool.UnGroupAndDelete(objectId);

        //    return point3Ds;

        //}











        /// <summary>
        /// 获取实体对象的宽度和高度
        /// </summary>
        /// <param name="objectId">对象的ObjectId</param>
        /// <returns>宽和高组成的double[]，如果读取不成功，返回null</returns>
        public double[] GetEntityBoundingWidthAndHeight(ObjectId objectId)
        {
            Entity entity = GetObject(objectId) as Entity;
            return entity.GetEntityBoundingWidthAndHeight();

        }




        /// <summary>
        /// 获取实体对象的宽度
        /// </summary>
        /// <param name="objectId">对象的ObjectId</param>
        /// <returns>宽度，如果读取不成功，返回double.NaN</returns>
        public double GetEntityBoundingWidth(ObjectId objectId)
        {
            Entity entity = GetObject(objectId) as Entity;
            return entity.GetEntityBoundingWidth();

        }





        /// <summary>
        /// 获取实体对象的高度
        /// </summary>
        /// <param name="objectId">对象的ObjectId</param>
        /// <returns>高度，如果读取不成功，返回double.NaN</returns>
        public double GetEntityBoundingHeight(ObjectId objectId)
        {
            Entity entity = GetObject(objectId) as Entity;
            return entity.GetEntityBoundingHeight();

        }




        /// <summary>
        /// 获取实体对象边界所包围的最大面积
        /// </summary>
        /// <param name="objectId">对象的ObjectId</param>
        /// <returns>面积值，如果读取不成功，返回double.NaN</returns>
        public double GetEntityBoundingArea(ObjectId objectId)
        {
            double[] widthAndHeight = GetEntityBoundingWidthAndHeight(objectId);

            if (widthAndHeight == null)  //没有获取成功
            {

                return double.NaN;
            }

            double area = widthAndHeight[0] * widthAndHeight[1];
            return area;

        }













        /// <summary>
        /// 获取图元对象的中点
        /// </summary>
        /// <param name="entity">图元对象</param>
        /// <returns>中点坐标Point3d</returns>
        public Point3d GetEntityMidPoint(Entity entity)
        {
            Point3d[] points = GetEntityMinPointAndMaxPoint(entity);
            Point3d minPoint = points[0];
            Point3d maxPoint = points[1];
            return new Point3d((minPoint.X + maxPoint.X) / 2, (minPoint.Y + maxPoint.Y) / 2, (minPoint.Z + maxPoint.Z) / 2);
        }



        /// <summary>
        /// 获取图元对象的中点
        /// </summary>
        /// <param name="objectId">图元对象的ObjectId</param>
        /// <returns>中点坐标Point3d</returns>
        public Point3d GetEntityMidPoint(ObjectId objectId)
        {
            Entity entity = GetObject(objectId) as Entity;
            return GetEntityMidPoint(entity);
        }







        /// <summary>
        /// 删除DBObject对象
        /// </summary>
        /// <param name="dbObjectId">DBObject对象的ObjectId</param>
        /// <param name="isRegenScreen">是否重生成屏幕，虽然删除掉对象后，不会再存在于数据库中，但是只有重生成屏幕后，才不会显示。重生成需要耗费时间，默认重生成</param>
        public void EraseDBObject(ObjectId dbObjectId, bool isRegenScreen = true)
        {




            //using (Transaction transaction = m_document.TransactionManager.StartTransaction())
            //{

            using (Transaction transaction = dbObjectId.Database.TransactionManager.StartTransaction())
            {




                DBObject dBObject = transaction.GetObject(dbObjectId, OpenMode.ForWrite, true);
                dBObject.Erase(true);
                transaction.Commit();
            }






            //如果没有通过ObjectTool(Document document)实例化，m_document将为null

            if (isRegenScreen && m_document != null)
            {

                //删除对象的时候可能比较慢，可能是因为这个刷屏幕造成的
                Editor editor = m_document.Editor;
                editor.Regen();
            }


        }



        /// <summary>
        /// 设置实体的颜色
        /// </summary>
        /// <param name="objectId">Entity对象的ObjectId</param>
        /// <param name="color">颜色对象</param>
        /// <param name="isRegenScreen">是否刷新屏幕</param>
        public void SetColor(ObjectId objectId, Color color, bool isRegenScreen = false)
        {

            if (m_document == null)
            {
                return;
            }


            using (DocumentLock docLock = m_document.LockDocument()) //锁定文档
            {

                using (Transaction transaction = m_document.TransactionManager.StartTransaction())
                {

                    Entity entity = transaction.GetObject(objectId, OpenMode.ForWrite, true) as Entity;

                    entity.Color = color;
                    transaction.Commit();
                }
            }


            //颜色修改后 屏幕不会自动更新，需要处理
            //如果没有通过ObjectTool(Document document)实例化，m_document将为null
            if (isRegenScreen && m_document != null)
            {

                //刷屏幕可能会比较慢
                Editor editor = m_document.Editor;
                editor.Regen();
            }


        }




        /// <summary>
        /// 设置实体列表的颜色
        /// </summary>
        /// <param name="objectIds">Entity对象的ObjectId列表</param>
        /// <param name="color">颜色对象</param>
        /// <param name="isRegenScreen">是否刷新屏幕</param>
        /// 
        public void SetColor(ObjectId[] objectIds, Color color, bool isRegenScreen = false)
        {

            if (objectIds == null || objectIds.Length == 0)
            {
                return;
            }


            if (m_document == null)
            {
                return;
            }

            using (DocumentLock docLock = m_document.LockDocument()) //锁定文档
            {

                using (Transaction transaction = m_document.TransactionManager.StartTransaction())
                {
                    foreach (var item in objectIds)
                    {

                        Entity entity = transaction.GetObject(item, OpenMode.ForWrite, true) as Entity;
                        entity.Color = color;

                    }

                    transaction.Commit();
                }

            }




            //颜色修改后 屏幕不会自动更新，需要处理
            //如果没有通过ObjectTool(Document document)实例化，m_document将为null
            if (isRegenScreen && m_document != null)
            {

                //刷屏幕可能会比较慢
                Editor editor = m_document.Editor;
                editor.Regen();
            }





        }






        /// <summary>
        /// 设置实体列表的颜色
        /// </summary>
        /// <param name="objectIdLst">Entity对象的ObjectId列表</param>
        /// <param name="color">颜色</param>
        public void SetColor(List<ObjectId> objectIdLst, Color color)
        {
            ObjectId[] objectIds = objectIdLst.ToArray();
            SetColor(objectIds, color);
        }






        /// <summary>
        /// 设置实体列表的颜色
        /// </summary>
        /// <param name="objectIds">Entity对象的ObjectId列表</param>
        /// <param name="lineTypeName">线形名称</param>
        public void SetLineType(ObjectId[] objectIds, string lineTypeName)
        {

            if (objectIds == null || objectIds.Length == 0)
            {
                return;
            }

            if (m_document == null)
            {
                return;
            }

            Database database = m_database;

            //是否存在线形或者加载成功
            LineTypeTool lineTypeTool = new LineTypeTool(database);

            ObjectId lineTypeId = lineTypeTool.GetOrLoadLineType(lineTypeName);
            if (lineTypeId.IsNull) //失败
            {
                return;
            }

            using (DocumentLock docLock = m_document.LockDocument()) //锁定文档
            {
                using (Transaction transaction = m_document.TransactionManager.StartTransaction())
                {
                    foreach (var item in objectIds)
                    {
                        Entity entity = transaction.GetObject(item, OpenMode.ForWrite, true) as Entity;
                        entity.Linetype = lineTypeName;

                    }

                    transaction.Commit();
                }

            }




            ////颜色修改后 屏幕不会自动更新，需要处理
            ////如果没有通过ObjectTool(Document document)实例化，m_document将为null
            //if (isRegenScreen && m_document != null)
            //{

            //    //刷屏幕可能会比较慢
            //    Editor editor = m_document.Editor;
            //    editor.Regen();
            //}

        }







        /// <summary>
        /// 设置实体列表的线型
        /// </summary>
        /// <param name="objectIds">Entity对象的ObjectId列表</param>
        /// <param name="lineTypeName">线形名称</param>
        public void SetLineType(List<ObjectId> objectIds, string lineTypeName)
        {

            if (objectIds == null || objectIds.Count == 0)
            {
                return;
            }

            var objectIdArr = objectIds.ToArray();

            SetLineType(objectIdArr, lineTypeName);
        }


        /// <summary>
        /// 设置实体列表的线型比例
        /// </summary>
        /// <param name="objectIdLst">Entity对象的ObjectId列表</param>
        /// <param name="lineScale">线型比例</param>
        public void SetLineTypeScale(List<ObjectId> objectIdLst, double lineScale)
        {
            if (objectIdLst == null || objectIdLst.Count == 0)
            {
                return;
            }

            var objectIdArr = objectIdLst.ToArray();

            SetLineTypeScale(objectIdArr, lineScale);
        }





        /// <summary>
        /// 设置实体列表的线型比例
        /// </summary>
        /// <param name="objectIds">Entity对象的ObjectId列表</param>
        /// <param name="lineScale">线型比例</param>
        public void SetLineTypeScale(ObjectId[] objectIds, double lineScale)
        {
            if (objectIds == null || objectIds.Length == 0)
            {
                return;
            }


            var ltscale = Application.GetSystemVariable("LTSCALE");


            double scale = Convert.ToDouble(ltscale);

            lineScale /= scale;


            using (Transaction transaction = m_document.TransactionManager.StartTransaction())
            {
                try
                {
                    foreach (var item in objectIds)
                    {
                        Entity entity = transaction.GetObject(item, OpenMode.ForWrite, true) as Entity;
                        entity.LinetypeScale = lineScale;
                    }

                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Abort();
                }
            }
        }





        /// <summary>
        /// 删除多个对象
        /// </summary>
        /// <param name="objectIds">对象的ObjectId列表</param>
        /// <param name="isRegenScreen">是否刷新屏幕</param>
        public void EraseDBObjects(List<ObjectId> objectIds, bool isRegenScreen = true)
        {

            foreach (ObjectId objectId in objectIds)
            {

                EraseDBObject(objectId, isRegenScreen);
            }

        }


        /// <summary>
        /// 删除多个对象
        /// </summary>
        /// <param name="objectIds">对象的ObjectId数组</param>
        /// <param name="isRegenScreen">是否刷新屏幕</param>
        public void EraseDBObjects(ObjectId[] objectIds, bool isRegenScreen = true)
        {

            if (objectIds == null || objectIds.Length == 0)
            {
                return;
            }


            Database database = objectIds[0].Database;

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {

                foreach (ObjectId objectId in objectIds)
                {

                    DBObject dBObject = transaction.GetObject(objectId, OpenMode.ForWrite, true);
                    dBObject.Erase(true);
                }

                transaction.Commit();
            }


            //如果没有实例化，m_document将为null

            if (isRegenScreen && m_document != null)
            {

                //删除对象的时候可能比较慢，可能是因为这个刷屏幕造成的
                Editor editor = m_document.Editor;
                editor.Regen();
            }

        }





        /// <summary>
        /// 删除多个对象
        /// </summary>
        /// <param name="selectionSet">DBObject对象集合</param>
        public void EraseDBObjects(SelectionSet selectionSet)
        {

            ObjectId[] objectIds = selectionSet.GetObjectIds();

            EraseDBObjects(objectIds);

        }





        /// <summary>
        /// 旋转图元对象
        /// </summary>
        /// <param name="objectId">图元对象的ObjectId</param>
        /// <param name="centerPoint">旋转中心点</param>
        /// <param name="rotateAngle">旋转角度</param>
        /// <returns>如果旋转成功，返回true，否则，返回false</returns>
        public bool RotateEntity(ObjectId objectId, Point3d centerPoint, double rotateAngle)
        {


            //using (Transaction transaction = m_document.TransactionManager.StartTransaction())
            //{

            using (Transaction transaction = objectId.Database.TransactionManager.StartTransaction())
            {



                Entity ent = transaction.GetObject(objectId, OpenMode.ForWrite) as Entity;
                if (ent == null)
                {
                    return false;
                }

                ent.TransformBy(Matrix3d.Rotation(rotateAngle, Vector3d.ZAxis, centerPoint));

                transaction.Commit();

                return true;
            }

        }



        /// <summary>
        /// 旋转图元对象
        /// </summary>
        /// <param name="entity">图元对象</param>
        /// <param name="centerPoint">旋转中心点</param>
        /// <param name="rotateAngle">旋转角度</param>
        /// <returns>如果旋转成功，返回true，否则，返回false</returns>
        public bool RotateEntity(Entity entity, Point3d centerPoint, double rotateAngle)
        {
            return RotateEntity(entity.Id, centerPoint, rotateAngle);
        }








        /// <summary>
        /// 旋转多个图元对象
        /// </summary>
        /// <param name="objectIds">图元对象数组</param>
        /// <param name="centerPoint">旋转中心点</param>
        /// <param name="rotateAngle">旋转角度</param>
        /// <returns>如果旋转成功，返回true，否则，返回false</returns>  
        public bool RotateEntities(ObjectId[] objectIds, Point3d centerPoint, double rotateAngle)
        {


            //using (Transaction transaction = m_document.TransactionManager.StartTransaction())
            //{

            Database database = m_database;

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {

                foreach (ObjectId objectId in objectIds)
                {

                    Entity ent = transaction.GetObject(objectId, OpenMode.ForWrite) as Entity;
                    if (ent == null)
                    {
                        return false;
                    }

                    ent.TransformBy(Matrix3d.Rotation(rotateAngle, Vector3d.ZAxis, centerPoint));
                }

                transaction.Commit();

                return true;
            }

        }






        /// <summary>
        /// 旋转多个图元对象
        /// </summary>
        /// <param name="objectIds">图元对象数组</param>
        /// <param name="centerPoint">旋转中心点</param>
        /// <param name="rotateAngle">旋转角度</param>
        /// <returns>如果旋转成功，返回true，否则，返回false</returns>  
        public bool RotateEntities(List<ObjectId> objectIds, Point3d centerPoint, double rotateAngle)
        {

            ObjectId[] objectIdsArray = objectIds.ToArray();
            return RotateEntities(objectIdsArray, centerPoint, rotateAngle);

        }


        /// <summary>
        /// 移动多个图元对象
        /// </summary>
        /// <param name="objectIdLst">图元对象列表</param>
        /// <param name="oldPoint">旧的点</param>
        /// <param name="newPoint">新的点</param>
        /// <returns>如果移动成功，返回true，否则，返回false</returns>  
        public bool MoveEntities(List<ObjectId> objectIdLst, Point3d oldPoint, Point3d newPoint)
        {
            Vector3d moveVector = newPoint - oldPoint;

            return MoveEntities(objectIdLst, moveVector);
        }





        /// <summary>
        /// 移动多个图元对象
        /// </summary>
        /// <param name="objectIdLst">图元对象列表</param>
        /// <param name="moveVector">移动的向量</param>
        /// <returns>如果移动成功，返回true，否则，返回false</returns>  
        public bool MoveEntities(List<ObjectId> objectIdLst, Vector3d moveVector)
        {
            //返回值
            bool isSucceed = false;
            if (objectIdLst == null || objectIdLst.Count == 0)
            {
                return isSucceed;
            }

            ObjectId[] objectIds = objectIdLst.ToArray();
            isSucceed = MoveEntities(objectIds, moveVector);
            return isSucceed;
        }



        /// <summary>
        /// 移动多个图元对象
        /// </summary>
        /// <param name="objectIds">图元对象数组</param>
        /// <param name="oldPoint">旧的点</param>
        /// <param name="newPoint">新的点</param>
        /// <returns>如果移动成功，返回true，否则，返回false</returns>  
        public bool MoveEntities(ObjectId[] objectIds, Point3d oldPoint, Point3d newPoint)
        {
            Vector3d moveVector = newPoint - oldPoint;

            return MoveEntities(objectIds, moveVector);
        }





        /// <summary>
        /// 移动多个图元对象
        /// </summary>
        /// <param name="objectIds">图元对象数组</param>
        /// <param name="moveVector">移动的向量</param>
        /// <returns>如果移动成功，返回true，否则，返回false</returns>  
        public bool MoveEntities(ObjectId[] objectIds, Vector3d moveVector)
        {
            Database database = m_database;

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {

                List<Entity> allEntityLst = new List<Entity>();

                foreach (ObjectId objectId in objectIds)
                {

                    Entity ent = transaction.GetObject(objectId, OpenMode.ForWrite) as Entity;
                    if (ent == null)
                    {
                        return false;
                    }

                    //视口需要特殊考虑，如果边界用别的对象来定义的话，需要移动的是这个边界对象，而不是视口对象
                    if (ent is Viewport vp)
                    {
                        //如果有用对象定义边界，则返回对象的ObjectId，否则返回Object.Null
                        var boundingBoxId = vp.NonRectClipEntityId;

                        if (boundingBoxId.IsNull)
                        {
                            if (!allEntityLst.Contains(ent))
                            {
                                allEntityLst.Add(ent);
                            }
                        }
                        else
                        {
                            Entity boundingBox = transaction.GetObject(boundingBoxId, OpenMode.ForWrite, true) as Entity;
                            if (!allEntityLst.Contains(boundingBox))
                            {
                                allEntityLst.Add(boundingBox);
                            }
                        }
                    }
                    else
                    {
                        if (!allEntityLst.Contains(ent))
                        {
                            allEntityLst.Add(ent);
                        }
                    }
                }

                allEntityLst.ForEach(ent => ent.TransformBy(Matrix3d.Displacement(moveVector)));

                //    Entity ent = transaction.GetObject(objectId, OpenMode.ForWrite) as Entity;
                //if (ent == null)
                //{
                //    return false;
                //}

                ////视口需要特殊考虑，如果边界用别的对象来定义的话，需要移动的是这个边界对象，而不是视口对象
                //if (ent is Viewport vp)
                //{
                //    //如果有用对象定义边界，则返回对象的ObjectId，否则返回Object.Null
                //    var boundingBoxId = vp.NonRectClipEntityId;

                //    if (boundingBoxId.IsNull)
                //    {
                //        ent.TransformBy(Matrix3d.Displacement(moveVector));
                //    }
                //    else
                //    {
                //        Entity boundingBox = transaction.GetObject(boundingBoxId, OpenMode.ForWrite, true) as Entity;
                //        boundingBox.TransformBy(Matrix3d.Displacement(moveVector));
                //    }
                //}
                //else
                //{
                //    ent.TransformBy(Matrix3d.Displacement(moveVector));
                //}
                //}
                transaction.Commit();

                return true;
            }

        }


        /// <summary>
        /// 移动单个图元对象
        /// </summary>
        /// <param name="objectId">图元对象的ObjectId</param>
        /// <param name="moveVector">移动的向量</param>
        /// <returns>如果移动成功，返回true，否则，返回false</returns>  
        public bool MoveEntity(ObjectId objectId, Vector3d moveVector)
        {
            //返回值
            bool isSucceed = false;


            Database database = m_database;

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                try
                {
                    if (transaction.GetObject(objectId, OpenMode.ForWrite) is Entity ent)
                    {
                        //视口需要特殊考虑，如果边界用别的对象来定义的话，需要移动的是这个边界对象，而不是视口对象
                        if (ent is Viewport vp)
                        {
                            //如果有用对象定义边界，则返回对象的ObjectId，否则返回Object.Null
                            var boundingBoxId = vp.NonRectClipEntityId;

                            if (boundingBoxId.IsNull)
                            {
                                ent.TransformBy(Matrix3d.Displacement(moveVector));
                            }
                            else
                            {
                                Entity boundingBox = transaction.GetObject(boundingBoxId, OpenMode.ForWrite, true) as Entity;
                                boundingBox.TransformBy(Matrix3d.Displacement(moveVector));
                            }
                        }

                        else
                        {
                            ent.TransformBy(Matrix3d.Displacement(moveVector));
                        }


                        isSucceed = true;
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
        /// 移动单个图元对象
        /// </summary>
        /// <param name="objectId">图元对象的ObjectId</param>
        /// <param name="oldPoint">旧的点</param>
        /// <param name="newPoint">新的点</param>
        /// <returns>如果移动成功，返回true，否则，返回false</returns>  
        public bool MoveEntity(ObjectId objectId, Point3d oldPoint, Point3d newPoint)
        {

            Vector3d moveVector = newPoint - oldPoint;

            return MoveEntity(objectId, moveVector);
        }


        /// <summary>
        /// 按照指定的对齐方式，将包围实体对象的四个边界点中的一个，移动到指定的新位置
        /// </summary>
        /// <param name="objectId">图元对象的ObjectId</param>
        /// <param name="newPoint">新的点</param>
        /// <param name="alignment">块的对齐方式，其它：左下角对齐；1：左上角对齐；2：右上角对齐；3：右下角对齐</param>
        /// <returns>如果移动成功，返回true，否则，返回false</returns>  
        public bool MoveEntity(ObjectId objectId, Point3d newPoint, int alignment = 0)
        {

            var boundingboxPointArr = GetEntityBoundingBoxPoints(objectId);

            if (boundingboxPointArr == null)
            {
                return false;
            }

            if (alignment >= 4)
            {
                alignment = 0;
            }

            Point3d point = boundingboxPointArr[alignment];

            return MoveEntity(objectId, point, newPoint);
        }


        /// <summary>
        /// 按照指定的对齐方式，将包围实体对象的四个边界点中的一个，移动到指定的新位置
        /// </summary>
        /// <param name="objectIdLst">图元对象的ObjectId列表</param>
        /// <param name="newPoint">新的点</param>
        /// <param name="alignment">块的对齐方式，其它：左下角对齐；1：左上角对齐；2：右上角对齐；3：右下角对齐</param>
        /// <returns>如果移动成功，返回true，否则，返回false</returns>  
        public bool MoveEntities(List<ObjectId> objectIdLst, Point3d newPoint, int alignment = 0)
        {
            //返回值
            bool isSucceed = false;
            if (objectIdLst == null || objectIdLst.Count == 0)
            {
                return isSucceed;
            }

            var boundingboxPointArr = GetDBObjectsBoundingBoxPoints(objectIdLst);

            if (boundingboxPointArr == null)
            {
                return isSucceed;
            }

            if (alignment >= 4)
            {
                alignment = 0;
            }

            Point3d point = boundingboxPointArr[alignment];

            return MoveEntities(objectIdLst, point, newPoint);
        }







        /// <summary>
        /// 缩放多个图元对象
        /// </summary>
        /// <param name="objectIds">图元对象数组</param>
        /// <param name="centerPoint">缩放的中心点坐标（WCS坐标）</param>
        /// <param name="scaleFactor">缩放比例</param>
        /// <returns>如果缩放成功，返回true，否则，返回false</returns>  
        public bool ScaleEntities(ObjectId[] objectIds, Point3d centerPoint, double scaleFactor = 1)
        {
            //返回值
            bool isSucceed = false;
            if (objectIds == null || objectIds.Length == 0)
            {
                return isSucceed;
            }


            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {

                foreach (ObjectId objectId in objectIds)
                {

                    Entity ent = transaction.GetObject(objectId, OpenMode.ForWrite) as Entity;
                    if (ent == null)
                    {
                        transaction.Abort();
                        return isSucceed;
                    }

                    ent.TransformBy(Matrix3d.Scaling(scaleFactor, centerPoint));
                }
                isSucceed = true;
                transaction.Commit();

            }

            return isSucceed;

        }



        /// <summary>
        /// 缩放多个图元对象
        /// </summary>
        /// <param name="objectIds">图元对象数组</param>
        /// <param name="centerPoint">缩放的中心点坐标（WCS坐标）</param>
        /// <param name="scaleFactor">缩放比例</param>
        /// <returns>如果缩放成功，返回true，否则，返回false</returns>  
        public bool ScaleEntities(List<ObjectId> objectIds, Point3d centerPoint, double scaleFactor = 1)
        {
            //返回值
            bool isSucceed = false;
            if (objectIds == null || objectIds.Count == 0)
            {
                return isSucceed;
            }
            ObjectId[] objectIdsArray = objectIds.ToArray();
            return ScaleEntities(objectIdsArray, centerPoint, scaleFactor);
        }




        ///// <summary>
        ///// 移动多个图元对象
        ///// </summary>
        ///// <param name="objectIds">图元对象数组</param>
        ///// <param name="moveVector">移动的向量</param>
        ///// <returns>如果旋转成功，返回true，否则，返回false</returns>  
        //public bool MoveEntities(List<ObjectId> objectIds, Vector3d moveVector)
        //{
        //    ObjectId[] objectIdsArray = objectIds.ToArray();
        //    return MoveEntities(objectIdsArray, moveVector);
        //}










        /// <summary>
        /// 炸开实体
        /// </summary>
        /// <param name="objectId">实体对象的ObjectId</param>
        /// <param name="eraseEntity">是否删除实体，默认不删除</param>
        /// <returns>炸开后生成的新对象ObjectId列表,如果操作没有成功，返回null</returns>
        public List<ObjectId> ExplodeEntity(ObjectId objectId, bool eraseEntity = false)
        {
            //返回值
            List<ObjectId> newEntityIds = new List<ObjectId>();
            using (Transaction transaction = objectId.Database.TransactionManager.StartTransaction())
            {

                Entity entity = transaction.GetObject(objectId, OpenMode.ForWrite) as Entity;
                if (entity == null)
                {
                    return null;
                }



                DBObjectCollection dBObjectCollection = new DBObjectCollection();

                entity.Explode(dBObjectCollection);

                if (dBObjectCollection.Count == 0)
                {
                    return null;
                }



                string spaceName = entity.BlockName;
                if (spaceName.ToUpper().Contains("PAPER_SPACE"))
                {
                    spaceName = "PAPERSPACE";
                }
                else
                {
                    spaceName = "MODELSPACE";
                }

                List<Entity> newEntities = new List<Entity>();

                foreach (DBObject dBObject in dBObjectCollection)
                {
                    Entity newEntity = dBObject as Entity;
                    if (newEntity != null)
                    {
                        newEntities.Add(newEntity);
                    }
                }


                if (newEntities.Count == 0)
                {
                    return null;
                }

                ObjectId[] newEntityIdsArr = AddEntityExtension.AddEntity(objectId.Database, newEntities, spaceName);


                //是否删除源对象
                if (eraseEntity)
                {
                    entity.Erase();
                }


                transaction.Commit();


                newEntityIds = new List<ObjectId>(newEntityIdsArr);
            }
            return newEntityIds;


        }




        /// <summary>
        /// 沿着z轴复制多个对象
        /// </summary>
        /// <param name="objectIds">对象的id列表</param>
        /// <param name="xDistance">复制距离</param>
        /// <param name="spaceName">模型空间还是图纸空间名称，不分大小写，默认模型空间</param>
        /// <returns>如果成功，返回新创建的所有对象的ObjectId列表，否则，返回空的列表</returns>
        public List<ObjectId> CopyEntitiesAlongXaxis(ObjectId[] objectIds, double xDistance, string spaceName = "MODELSPACE")
        {
            List<ObjectId> objectIdLst = new List<ObjectId>(objectIds);
            return CopyEntitiesAlongXaxis(objectIdLst, xDistance, spaceName);
        }







        /// <summary>
        /// 沿着z轴复制多个对象
        /// </summary>
        /// <param name="objectIds">对象的id列表</param>
        /// <param name="xDistance">复制距离</param>
        /// <param name="spaceName">模型空间还是图纸空间名称，不分大小写，默认模型空间</param>
        /// <returns>如果成功，返回新创建的所有对象的ObjectId列表，否则，返回空的列表</returns>
        public List<ObjectId> CopyEntitiesAlongXaxis(List<ObjectId> objectIds, double xDistance, string spaceName = "MODELSPACE")
        {

            SpaceTool spaceTool = new SpaceTool(m_database);
            ObjectId spaceId = spaceTool.GetSpaceByName(spaceName);


            return CopyEntitiesAlongXaxis(spaceId, objectIds, xDistance);
        }




        /// <summary>
        /// 沿着x轴复制多个对象
        /// </summary>
        /// <param name="spaceId">空间对象的ObjectId</param>
        /// <param name="objectIds">对象的id列表</param>
        /// <param name="xDistance">复制距离</param>
        /// <returns>如果成功，返回新创建的所有对象的ObjectId列表，否则，返回空的列表</returns>
        public List<ObjectId> CopyEntitiesAlongXaxis(ObjectId spaceId, List<ObjectId> objectIds, double xDistance)
        {
            //返回值
            List<ObjectId> newObjectLst = new List<ObjectId>();

            if (spaceId.IsNull)
            {
                return newObjectLst;
            }


            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    // Open the Block table record Modelspace for write
                    // 以写打开块表记录模型空间
                    if (transaction.GetObject(spaceId, OpenMode.ForWrite) is BlockTableRecord space)
                    {
                        //变换矩阵
                        var transform = Matrix3d.Displacement(new Vector3d(xDistance, 0, 0));


                        foreach (ObjectId objectId in objectIds)
                        {

                            Entity ent = transaction.GetObject(objectId, OpenMode.ForRead) as Entity;
                            if (ent == null)
                            {
                                //前面操作过的都撤销
                                transaction.Abort();
                                newObjectLst.Clear();

                                break;
                            }


                            Entity entClone = ent.Clone() as Entity;

                            // 创建一个变换矩阵，移动每个复本实体
                            entClone.TransformBy(transform);

                            // Add the cloned object
                            // 将克隆对象添加到块表记录和事务w
                            ObjectId newObjectId = space.AppendEntity(entClone);
                            transaction.AddNewlyCreatedDBObject(entClone, true);

                            newObjectLst.Add(newObjectId);
                        }

                    }

                    transaction.Commit();
                }
                catch
                {
                    //前面操作过的都撤销
                    transaction.Abort();
                    newObjectLst.Clear();
                }

            }

            return newObjectLst;

        }





        /// <summary>
        /// 复制多个对象一个新的地方，源对象不删除
        /// </summary>
        /// <param name="objectIds">对象的id列表</param>
        /// <param name="sourcePoint">被复制对象的基点</param>
        /// <param name="targetPoint">目标点</param>
        /// <param name="spaceName">模型空间还是图纸空间名称，不分大小写，默认模型空间</param>
        /// <returns>如果成功，返回新创建的所有对象的ObjectId列表，否则，返回空的列表</returns>
        public List<ObjectId> CopyEntities(List<ObjectId> objectIds, Point3d sourcePoint, Point3d targetPoint, string spaceName = "MODELSPACE")
        {
            SpaceTool spaceTool = new SpaceTool(m_database);
            ObjectId spaceId = spaceTool.GetSpaceByName(spaceName);

            return CopyEntities(spaceId, objectIds, sourcePoint, targetPoint);
        }








        /// <summary>
        /// 复制多个对象一个新的地方，源对象不删除
        /// </summary>
        /// <param name="spaceId">空间对象的ObjectId</param>
        /// <param name="objectIds">对象的id列表</param>
        /// <param name="sourcePoint">被复制对象的基点</param>
        /// <param name="targetPoint">目标点</param>
        /// <returns>如果成功，返回新创建的所有对象的ObjectId列表，否则，返回空的列表</returns>
        public List<ObjectId> CopyEntities(ObjectId spaceId, List<ObjectId> objectIds, Point3d sourcePoint, Point3d targetPoint)
        {
            //返回值
            List<ObjectId> newObjectLst = new List<ObjectId>();

            if (spaceId.IsNull)
            {
                return newObjectLst;
            }


            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    // Open the Block table record Modelspace for write
                    // 以写打开块表记录模型空间
                    if (transaction.GetObject(spaceId, OpenMode.ForWrite) is BlockTableRecord space)
                    {
                        //变换矩阵
                        var transform = Matrix3d.Displacement(targetPoint - sourcePoint);

                        foreach (ObjectId objectId in objectIds)
                        {

                            Entity ent = transaction.GetObject(objectId, OpenMode.ForRead) as Entity;
                            if (ent == null)
                            {
                                //前面操作过的都撤销
                                transaction.Abort();
                                newObjectLst.Clear();

                                break;
                            }


                            Entity entClone = ent.Clone() as Entity;

                            // 创建一个变换矩阵，移动每个复本实体
                            entClone.TransformBy(transform);

                            // Add the cloned object
                            // 将克隆对象添加到块表记录和事务w
                            ObjectId newObjectId = space.AppendEntity(entClone);
                            transaction.AddNewlyCreatedDBObject(entClone, true);

                            newObjectLst.Add(newObjectId);
                        }

                    }

                    transaction.Commit();
                }
                catch
                {
                    //前面操作过的都撤销
                    transaction.Abort();
                    newObjectLst.Clear();
                }

            }

            return newObjectLst;

        }







        /// <summary>
        /// 复制多个对象多个新的地方，源对象不删除
        /// </summary>
        /// <param name="objectIds">对象的id列表</param>
        /// <param name="sourcePoint">被复制对象的基点</param>
        /// <param name="targetPointLst">目标点列表</param>
        /// <param name="spaceName">模型空间还是图纸空间名称，不分大小写，默认模型空间</param>
        /// <returns>如果成功，返回新创建的所有对象的ObjectId列表，否则，返回空的列表</returns>
        public List<ObjectId> CopyEntities(List<ObjectId> objectIds, Point3d sourcePoint, List<Point3d> targetPointLst, string spaceName = "MODELSPACE")
        {
            SpaceTool spaceTool = new SpaceTool(m_database);
            ObjectId spaceId = spaceTool.GetSpaceByName(spaceName);

            return CopyEntities(spaceId, objectIds, sourcePoint, targetPointLst);
        }







        /// <summary>
        /// 复制多个对象多个新的地方，源对象不删除
        /// </summary>
        /// <param name="spaceId">空间对象的ObjectId</param>
        /// <param name="objectIds">对象的id列表</param>
        /// <param name="sourcePoint">被复制对象的基点</param>
        /// <param name="targetPointLst">目标点列表</param>
        /// <returns>如果成功，返回新创建的所有对象的ObjectId列表，否则，返回空的列表</returns>
        public List<ObjectId> CopyEntities(ObjectId spaceId, List<ObjectId> objectIds, Point3d sourcePoint, List<Point3d> targetPointLst)
        {

            //返回值
            List<ObjectId> newObjectLst = new List<ObjectId>();

            if (spaceId.IsNull)
            {
                return newObjectLst;
            }


            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    // Open the Block table record Modelspace for write
                    // 以写打开块表记录模型空间
                    if (transaction.GetObject(spaceId, OpenMode.ForWrite) is BlockTableRecord space)
                    {


                        foreach (var targetPoint in targetPointLst)
                        {

                            //变换矩阵
                            var transform = Matrix3d.Displacement(targetPoint - sourcePoint);

                            foreach (ObjectId objectId in objectIds)
                            {

                                Entity ent = transaction.GetObject(objectId, OpenMode.ForRead) as Entity;
                                if (ent == null)
                                {

                                    //前面操作过的都撤销
                                    transaction.Abort();
                                    newObjectLst.Clear();

                                    break;
                                }


                                Entity entClone = ent.Clone() as Entity;

                                // 创建一个变换矩阵，移动每个复本实体15个单位
                                entClone.TransformBy(transform);

                                // Add the cloned object
                                // 将克隆对象添加到块表记录和事务
                                ObjectId newObjectId = space.AppendEntity(entClone);
                                transaction.AddNewlyCreatedDBObject(entClone, true);

                                newObjectLst.Add(newObjectId);
                            }

                        }
                    }

                    transaction.Commit();
                }
                catch
                {
                    //前面操作过的都撤销
                    transaction.Abort();
                    newObjectLst.Clear();
                }

            }

            return newObjectLst;

        }




        /// <summary>
        /// 将对象列表复制到某个空间，位置不变，源对象不删除
        /// </summary>
        /// <param name="objectIds">对象的id列表</param>
        /// <param name="spaceName">空间名称，默认模型空间</param>
        /// <returns>新创建对象的ObjectId列表，如果复制不成功，返回空的列表</returns>
        public List<ObjectId> CopyEntitiesToSpace(List<ObjectId> objectIds, string spaceName = "MODELSPACE")
        {
            SpaceTool spaceTool = new SpaceTool(m_database);
            ObjectId spaceId = spaceTool.GetSpaceByName(spaceName);

            return CopyEntitiesToSpace(spaceId, objectIds);
        }






        /// <summary>
        /// 将对象列表复制到某个空间，位置不变，源对象不删除
        /// </summary>
        /// <param name="spaceId">空间对象的ObjectId</param>
        /// <param name="objectIds">对象的id列表</param>
        /// <returns>新创建对象的ObjectId列表，如果复制不成功，返回空的列表</returns>
        public List<ObjectId> CopyEntitiesToSpace(ObjectId spaceId, List<ObjectId> objectIds)
        {
            //返回值
            List<ObjectId> newObjectLst = new List<ObjectId>();

            if (spaceId.IsNull)
            {
                return newObjectLst;
            }

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    // Open the Block table record Modelspace for write
                    // 以写打开块表记录模型空间
                    if (transaction.GetObject(spaceId, OpenMode.ForWrite) is BlockTableRecord space)
                    {

                        foreach (ObjectId objectId in objectIds)
                        {

                            Entity ent = transaction.GetObject(objectId, OpenMode.ForRead) as Entity;
                            if (ent == null)
                            {

                                //前面操作过的都撤销
                                transaction.Abort();
                                newObjectLst.Clear();

                                break;
                            }

                            Entity entClone = ent.Clone() as Entity;

                            // Add the cloned object
                            // 将克隆对象添加到块表记录和事务
                            ObjectId newObjectId = space.AppendEntity(entClone);
                            transaction.AddNewlyCreatedDBObject(entClone, true);

                            newObjectLst.Add(newObjectId);

                        }
                    }

                    transaction.Commit();
                }
                catch
                {
                    //前面操作过的都撤销
                    transaction.Abort();
                    newObjectLst.Clear();
                }

            }

            return newObjectLst;

        }




        /// <summary>
        /// 对象的类型
        /// </summary>
        /// <param name="objectId">对象的objectId</param>
        /// <returns>对象的类型，基本上字母都为大写</returns>
        public string GetObjectType(ObjectId objectId)
        {
            string dxfName = objectId.ObjectClass.DxfName;

            return dxfName;
        }





        /// <summary>
        /// 获取对象所在的组列表
        /// </summary>
        /// <param name="objectId">对象的ObjectId</param>
        /// <returns>对象所在的所有组的ObjectId列表，如果不在任何组，返回空的列表</returns>
        public List<ObjectId> GetGroups(ObjectId objectId)
        {
            //返回值
            List<ObjectId> groupIdLst = new List<ObjectId>();

            if (objectId.IsNull)
            {
                return groupIdLst;
            }


            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                DBObject dBObject = transaction.GetObject(objectId, OpenMode.ForRead);
                ObjectIdCollection ids = dBObject.GetPersistentReactorIds();

                foreach (ObjectId id in ids)
                {
                    if (transaction.GetObject(id, OpenMode.ForRead) is Group group)
                    {
                        groupIdLst.Add(id);
                    }

                }

                transaction.Commit();
            }

            return groupIdLst;

        }



        /// <summary>
        /// 获取对象列表所在的组列表，自动排除重复
        /// </summary>
        /// <param name="objectIdLst">对象的ObjectId列表</param>
        /// <returns>对象所在的所有组的ObjectId列表，如果不在任何组，返回空的列表</returns>
        public List<ObjectId> GetGroups(List<ObjectId> objectIdLst)
        {
            //返回值
            List<ObjectId> groupIdLst = new List<ObjectId>();

            if (objectIdLst == null || objectIdLst.Count == 0)
            {
                return groupIdLst;
            }

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                foreach (var objectId in objectIdLst)
                {
                    DBObject dBObject = transaction.GetObject(objectId, OpenMode.ForRead);
                    ObjectIdCollection ids = dBObject.GetPersistentReactorIds();

                    foreach (ObjectId id in ids)
                    {
                        if (transaction.GetObject(id, OpenMode.ForRead) is Group group)
                        {
                            if (!groupIdLst.Contains(id)) //排除掉重复
                            {
                                groupIdLst.Add(id);
                            }
                        }

                    }
                }

                transaction.Commit();
            }

            return groupIdLst;

        }



        /// <summary>
        /// 获取两个实体对象的交点列表
        /// </summary>
        /// <param name="firstEntityId">第一个实体对象的ObjectId</param>
        /// <param name="secondEntityId">第一个实体对象的ObjectId</param>
        /// <param name="intersectType">两个对象延伸的类型。OnBothOperands：两者都不延伸；ExtendThis：只延伸第一个对象；ExtendArgument：只延伸第二个对象；ExtendBoth两者都延伸</param>
        /// <returns>交点列表，如果失败，返回空的列表</returns>
        public List<Point3d> GetIntersectWithPoints(ObjectId firstEntityId, ObjectId secondEntityId,Intersect intersectType= Intersect.OnBothOperands)
        {
            //返回值
            List<Point3d> intersectWithPointLst = new List<Point3d>();
            if (firstEntityId.IsNull || secondEntityId.IsNull)
            {
                return intersectWithPointLst;
            }


            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    if (transaction.GetObject(firstEntityId, OpenMode.ForRead) is Entity firstEntity)
                    {
                        if (transaction.GetObject(secondEntityId, OpenMode.ForRead) is Entity secondEntity)
                        {

                            Point3dCollection point3dCollection = new Point3dCollection();



                            if (firstEntity is BlockReference firstBlockReference)
                            {

                                var firstBoundingBox = GetEntityBoundingBoxPoints(firstEntityId);

                                //如果失败
                                if (firstBoundingBox == null)
                                {
                                    return intersectWithPointLst;
                                }


                                ObjectId firstSpaceId = firstEntity.BlockId;

                                //创建边界
                                ObjectId firstBoundingBoxId = m_database.AddPolyLine(firstSpaceId, firstBoundingBox.ToList());

                                //创建失败
                                if (firstBoundingBoxId.IsNull)
                                {
                                    return intersectWithPointLst;
                                }


                                Entity firstBoundingBoxEntity = transaction.GetObject(firstBoundingBoxId, OpenMode.ForRead) as Entity;

                                if (secondEntity is BlockReference secondBlockReference)
                                {

                                    var secondBoundingBox = GetEntityBoundingBoxPoints(secondEntityId);

                                    //如果失败
                                    if (secondBoundingBox == null)
                                    {
                                        return intersectWithPointLst;
                                    }


                                    ObjectId secondSpaceId = secondEntity.BlockId;

                                    //创建边界
                                    ObjectId secondBoundingBoxId = m_database.AddPolyLine(secondSpaceId, secondBoundingBox.ToList());

                                    //创建失败
                                    if (secondBoundingBoxId.IsNull)
                                    {
                                        return intersectWithPointLst;
                                    }


                                    Entity secondBoundingBoxEntity = transaction.GetObject(secondBoundingBoxId, OpenMode.ForWrite) as Entity;


                                    firstBoundingBoxEntity.IntersectWith(secondBoundingBoxEntity, intersectType, point3dCollection, IntPtr.Zero, IntPtr.Zero);

                                    //记得删除
                                    secondBoundingBoxEntity.Erase();
                                }
                                else
                                {
                                    firstBoundingBoxEntity.IntersectWith(secondEntity, intersectType, point3dCollection, IntPtr.Zero, IntPtr.Zero);

                                }

                                firstBoundingBoxEntity.Erase();

                            }
                            else
                            {

                                if (secondEntity is BlockReference secondBlockReference)
                                {

                                    var secondBoundingBox = GetEntityBoundingBoxPoints(secondEntityId);

                                    //如果失败
                                    if (secondBoundingBox == null)
                                    {
                                        return intersectWithPointLst;
                                    }


                                    ObjectId secondSpaceId = secondEntity.BlockId;

                                    //创建边界
                                    ObjectId secondBoundingBoxId = m_database.AddPolyLine(secondSpaceId, secondBoundingBox.ToList());

                                    //创建失败
                                    if (secondBoundingBoxId.IsNull)
                                    {
                                        return intersectWithPointLst;
                                    }


                                    Entity secondBoundingBoxEntity = transaction.GetObject(secondBoundingBoxId, OpenMode.ForWrite) as Entity;


                                    firstEntity.IntersectWith(secondBoundingBoxEntity, intersectType, point3dCollection, IntPtr.Zero, IntPtr.Zero);

                                    //记得删除
                                    secondBoundingBoxEntity.Erase();

                                }
                                else
                                {
                                    firstEntity.IntersectWith(secondEntity, intersectType, point3dCollection, IntPtr.Zero, IntPtr.Zero);
                                    //firstEntity.BoundingBoxIntersectWith(secondEntity, Intersect.OnBothOperands, point3dCollection, IntPtr.Zero, IntPtr.Zero);

                                }

                            }


                            for (int i = 0; i < point3dCollection.Count; i++)
                            {
                                intersectWithPointLst.Add(point3dCollection[i]);
                            }
                        }
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();
                }

            }

            return intersectWithPointLst;

        }




        /// <summary>
        /// 判断两个实体对象是否相交 两个对象都不延伸
        /// </summary>
        /// <param name="firstEntityId">第一个实体对象的ObjectId</param>
        /// <param name="secondEntityId">第一个实体对象的ObjectId</param>
        /// <param name="intersectType">两个对象延伸的类型。OnBothOperands：两者都不延伸；ExtendThis：只延伸第一个对象；ExtendArgument：只延伸第二个对象；ExtendBoth两者都延伸</param>
        /// <returns>如果相交，返回true，否则，返回false</returns>
        public bool IsIntersectWith(ObjectId firstEntityId, ObjectId secondEntityId, Intersect intersectType = Intersect.OnBothOperands)
        {

            List<Point3d> intersectPointLst = GetIntersectWithPoints(firstEntityId, secondEntityId, intersectType);
            if (intersectPointLst.Count == 0)
            {
                return false;
            }
            else
            {
                return true;
            }

        }





        /// <summary>
        /// 获取指定实体对象跟同一个空间中相交的所有实体对象列表
        /// </summary>
        /// <param name="entityId">实体对象的ObjectId</param>
        /// <param name="dxfNameLst">要获取的对象类型列表，不分大小写，如直线为"LINE"，如果为空，则获取所有类型</param>
        /// <returns>实体对象列表，如果失败，返回空的列表</returns>        /// <param name="intersectType">两个对象延伸的类型。OnBothOperands：两者都不延伸；ExtendThis：只延伸第一个对象；ExtendArgument：只延伸第二个对象；ExtendBoth两者都延伸</param>
        public List<ObjectId> GetAllIntersectWithEntities(ObjectId entityId, List<string> dxfNameLst = null, Intersect intersectType = Intersect.OnBothOperands)
        {
            //返回值
            List<ObjectId> allIntersectWithEntityIdLst = new List<ObjectId>();

            if (entityId.IsNull)
            {
                return allIntersectWithEntityIdLst;
            }



            SpaceTool spaceTool = new SpaceTool(m_database);

            ObjectId spaceId = spaceTool.GetSpaceByEntity(entityId);
            if (spaceId.IsNull)
            {
                return allIntersectWithEntityIdLst;
            }

            List<ObjectId> allEntityObjectIdLst = spaceTool.GetAllEntitiesInSpace(spaceId, dxfNameLst);

            if (allEntityObjectIdLst.Count == 0)
            {
                return allIntersectWithEntityIdLst;
            }


            //以上可能包含自身，需要排除掉
            allEntityObjectIdLst.Remove(entityId);


            //获取所有跟指定实体对象相交的对象列表

            allIntersectWithEntityIdLst = allEntityObjectIdLst.FindAll(x => IsIntersectWith(entityId, x, intersectType));

            //视口的边界可能是指定了某个对象 这个情况下边界会包含在以上列表中，需要特殊考虑 排除掉
            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    if (transaction.GetObject(entityId, OpenMode.ForRead, true) is Viewport vp)
                    {

                        ObjectId boundingBoxId = vp.NonRectClipEntityId;

                        if (!boundingBoxId.IsNull)
                        {
                            allIntersectWithEntityIdLst.Remove(boundingBoxId);
                        }
                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();
                }
            }


            return allIntersectWithEntityIdLst;

        }




    }

}
