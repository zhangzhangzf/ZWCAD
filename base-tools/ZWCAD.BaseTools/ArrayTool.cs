using ZWCAD.BaseTools.Extension;
using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.Geometry;
using System.Collections.Generic;
using ZwSoft.ZwCAD.Runtime;
using ZwSoft.ZwCAD.EditorInput;

namespace ZWCAD.BaseTools
{
    /// <summary>
    /// 阵列对象工具
    /// </summary>
    public class ArrayTool
    {



        #region Private Variables

        Document m_document;
        Database m_database;

        #endregion



        #region Default Constructor

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="document">文档对象</param>
        public ArrayTool(Document document)
        {
            m_document = document;
            m_database=m_document.Database;
        }


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="database">数据库对象</param>
        public ArrayTool(Database database)
        {
            m_database=database;
        }





        #endregion



        /// <summary>
        /// 对指定对象进行矩形阵列
        /// </summary>
        /// <param name="objectId">对象的ObjectId</param>
        /// <param name="basePoint">基点</param>
        /// <param name="rowCount">行数</param>
        /// <param name="rowSpacing">行间距</param>
        /// <param name="columnCount">列数</param>
        /// <param name="columnSpacing">列间距</param>
        /// <param name="RowElevation"></param>
        /// <param name="levelCount">z方向上的个数,如果为0 将报错：eMustBePositive， 应该是说z方向应该有一排</param>
        /// <param name="levelSpacing">z方向的间距</param>
        /// <param name="isDelete">是否删除原来的对象，默认是</param>
        /// <param name="spaceName">模型空间还是图纸空间名称，不分大小写，默认模型空间</param>
        /// <returns>阵列的ObjectId，如果失败，返回的ObjectId.Null</returns>
        public ObjectId ArrayByRectangle(ObjectId objectId, Point3d basePoint, int rowCount = 10, double rowSpacing = 10, int columnCount = 10, double columnSpacing = 10, double RowElevation = 0, int levelCount = 1, double levelSpacing = 10, bool isDelete = true, string spaceName = "MODELSPACE")
        {

            //返回值
            ObjectId arrayId = ObjectId.Null;

            //先获取空间
            SpaceTool spaceTool = new SpaceTool(m_database);
            ObjectId spaceId = spaceTool.GetSpaceByName(spaceName);

            if (spaceId.IsNull)
            {
                return arrayId;
            }

            arrayId= ArrayByRectangle(spaceId, objectId, basePoint, rowCount, rowSpacing, columnCount, columnSpacing, RowElevation, levelCount, levelSpacing, isDelete);

            return arrayId;
        }



        /// <summary>
        /// 对指定对象进行矩形阵列
        /// </summary>
        /// <param name="spaceId">空间对象的ObjectId</param>
        /// <param name="objectId">对象的ObjectId</param>
        /// <param name="basePoint">基点</param>
        /// <param name="rowCount">行数</param>
        /// <param name="rowSpacing">行间距</param>
        /// <param name="columnCount">列数</param>
        /// <param name="columnSpacing">列间距</param>
        /// <param name="RowElevation"></param>
        /// <param name="levelCount">z方向上的个数,如果为0 将报错：eMustBePositive， 应该是说z方向应该有一排</param>
        /// <param name="levelSpacing">z方向的间距</param>
        /// <param name="isDelete">是否删除原来的对象，默认是</param>
        /// <returns>阵列的ObjectId，如果失败，返回的ObjectId.Null</returns>
        public ObjectId ArrayByRectangle(ObjectId spaceId, ObjectId objectId, Point3d basePoint, int rowCount = 10, double rowSpacing = 10, int columnCount = 10, double columnSpacing = 10, double RowElevation = 0, int levelCount = 1, double levelSpacing = 10, bool isDelete = true)
        {
            //返回值
            ObjectId arrayId = ObjectId.Null;

            if (objectId.IsNull)
            {
                return arrayId;
            }

            ObjectId[] objectIdArr = new ObjectId[] { objectId };

            arrayId=ArrayByRectangle(spaceId, objectIdArr, basePoint, rowCount, rowSpacing, columnCount, columnSpacing, RowElevation, levelCount, levelSpacing, isDelete);

            return arrayId;

        }



        /// <summary>
        /// 对指定对象通过复制的形式形成矩形阵列的效果，返回的是多个独立的对象
        /// </summary>
        /// <param name="spaceId">空间对象的ObjectId</param>
        /// <param name="objectId">对象的ObjectId数组</param>
        /// <param name="basePoint">基点</param>
        /// <param name="rowCount">行数</param>
        /// <param name="rowSpacing">行间距</param>
        /// <param name="columnCount">列数</param>
        /// <param name="columnSpacing">列间距</param>
        /// <param name="isDelete">是否删除原来的对象，默认是</param>
        /// <returns>复制后的新对象ObjectId列表，如果失败，返回空的列表</returns>
        public List<ObjectId> ArrayByCopy(ObjectId spaceId, ObjectId objectId, Point3d basePoint, int rowCount = 10, double rowSpacing = 10, int columnCount = 10, double columnSpacing = 10, bool isDelete = true)
        {
            //返回值
            List<ObjectId> arrayIdLst = new List<ObjectId>();

            if (objectId.IsNull)
            {
                return arrayIdLst;
            }

            List<ObjectId> objectIdLst = new List<ObjectId> { objectId };

            arrayIdLst=ArrayByCopy(spaceId, objectIdLst, basePoint, rowCount, rowSpacing, columnCount, columnSpacing, isDelete);

            return arrayIdLst;

        }


        /// <summary>
        /// 对指定对象列表通过复制的形式形成矩形阵列的效果，返回的是多个独立的对象
        /// </summary>
        /// <param name="spaceId">空间对象的ObjectId</param>
        /// <param name="objectIdArr">对象的ObjectId数组</param>
        /// <param name="basePoint">基点</param>
        /// <param name="rowCount">行数</param>
        /// <param name="rowSpacing">行间距</param>
        /// <param name="columnCount">列数</param>
        /// <param name="columnSpacing">列间距</param>
        /// <param name="isDelete">是否删除原来的对象，默认是</param>
        /// <returns>复制后的新对象ObjectId列表，如果失败，返回空的列表</returns>
        public List<ObjectId> ArrayByCopy(ObjectId spaceId, ObjectId[] objectIdArr, Point3d basePoint, int rowCount = 10, double rowSpacing = 10, int columnCount = 10, double columnSpacing = 10, bool isDelete = true)
        {
            //返回值
            List<ObjectId> arrayIdLst = new List<ObjectId>();

            if (spaceId.IsNull || objectIdArr==null ||objectIdArr.Length==0)
            {
                return arrayIdLst;
            }

            List<ObjectId> objectIdLst = new List<ObjectId>(objectIdArr);


            arrayIdLst=ArrayByCopy(spaceId, objectIdLst, basePoint, rowCount, rowSpacing, columnCount, columnSpacing, isDelete);

            return arrayIdLst;

        }




        /// <summary>
        /// 对指定对象列表通过复制的形式形成矩形阵列的效果，返回的是多个独立的对象
        /// </summary>
        /// <param name="spaceId">空间对象的ObjectId</param>
        /// <param name="objectIdLst">对象的ObjectId列表</param>
        /// <param name="basePoint">基点</param>
        /// <param name="rowCount">行数</param>
        /// <param name="rowSpacing">行间距</param>
        /// <param name="columnCount">列数</param>
        /// <param name="columnSpacing">列间距</param>
        /// <param name="isDelete">是否删除原来的对象，默认是</param>
        /// <returns>复制后的新对象ObjectId列表，如果失败，返回空的列表</returns>
        public List<ObjectId> ArrayByCopy(ObjectId spaceId, List<ObjectId> objectIdLst, Point3d basePoint, int rowCount = 10, double rowSpacing = 10, int columnCount = 10, double columnSpacing = 10, bool isDelete = true)
        {
            //返回值
            List<ObjectId> arrayIdLst = new List<ObjectId>();

            if (spaceId.IsNull || objectIdLst==null ||objectIdLst.Count==0)
            {
                return arrayIdLst;
            }

            //计算位置
            List<Point3d> targetPointLst = new List<Point3d>();


            for (int row = 0; row < rowCount; row++)
            {

                for (int column = 0; column < columnCount; column++)
                {
                    double xDistance = column*columnSpacing;
                    double yDistance = row*rowSpacing;

                    Point3d vector = new Point3d(xDistance, yDistance, 0);

                    Point3d newPoint = basePoint.AddBy(vector);
                    targetPointLst.Add(newPoint);
                }
            }

            ObjectTool objectTool = new ObjectTool(m_database);

            arrayIdLst=objectTool.CopyEntities(spaceId, objectIdLst, basePoint, targetPointLst);

            //删除源对象
            if (isDelete)
            {
                objectTool.EraseDBObjects(objectIdLst);
            }


            return arrayIdLst;

        }




















        /// <summary>
        /// 对指定对象数组进行矩形阵列
        /// </summary>
        /// <param name="objectIdLst">对象的数组</param>
        /// <param name="basePoint">基点</param>
        /// <param name="rowCount">行数</param>
        /// <param name="rowSpacing">行间距</param>
        /// <param name="columnCount">列数</param>
        /// <param name="columnSpacing">列间距</param>
        /// <param name="RowElevation"></param>
        /// <param name="levelCount">z方向上的个数,如果为0 将报错：eMustBePositive， 应该是说z方向应该有一排</param>
        /// <param name="levelSpacing">z方向的间距</param>
        /// <param name="isDelete">是否删除原来的对象，默认是</param>
        /// <param name="spaceName">模型空间还是图纸空间名称，不分大小写，默认模型空间</param>
        /// <returns>阵列的ObjectId，如果失败，返回的ObjectId.Null</returns>
        public ObjectId ArrayByRectangle(List<ObjectId> objectIdLst, Point3d basePoint, int rowCount = 10, double rowSpacing = 10, int columnCount = 10, double columnSpacing = 10, double RowElevation = 0, int levelCount = 1, double levelSpacing = 10, bool isDelete = true, string spaceName = "MODELSPACE")
        {
            //返回值
            ObjectId arrayId = ObjectId.Null;

            //先获取空间
            SpaceTool spaceTool = new SpaceTool(m_database);
            ObjectId spaceId = spaceTool.GetSpaceByName(spaceName);

            if (spaceId.IsNull)
            {
                return arrayId;
            }


            arrayId= ArrayByRectangle(spaceId, objectIdLst, basePoint, rowCount, rowSpacing, columnCount, columnSpacing, RowElevation, levelCount, levelSpacing, isDelete);

            return arrayId;
        }






        /// <summary>
        /// 对指定对象数组进行矩形阵列
        /// </summary>
        /// <param name="spaceId">空间对象的ObjectId</param>
        /// <param name="objectIdLst">对象的数组</param>
        /// <param name="basePoint">基点</param>
        /// <param name="rowCount">行数</param>
        /// <param name="rowSpacing">行间距</param>
        /// <param name="columnCount">列数</param>
        /// <param name="columnSpacing">列间距</param>
        /// <param name="RowElevation"></param>
        /// <param name="levelCount">z方向上的个数,如果为0 将报错：eMustBePositive， 应该是说z方向应该有一排</param>
        /// <param name="levelSpacing">z方向的间距</param>
        /// <param name="isDelete">是否删除原来的对象，默认是</param>
        /// <returns>阵列的ObjectId，如果失败，返回的ObjectId.Null</returns>
        public ObjectId ArrayByRectangle(ObjectId spaceId, List<ObjectId> objectIdLst, Point3d basePoint, int rowCount = 10, double rowSpacing = 10, int columnCount = 10, double columnSpacing = 10, double RowElevation = 0, int levelCount = 1, double levelSpacing = 10, bool isDelete = true)
        {
            //返回值
            ObjectId arrayId = ObjectId.Null;

            if (objectIdLst == null || objectIdLst.Count==0)
            {
                return arrayId;
            }

            ObjectId[] objectIdArr = objectIdLst.ToArray();

            arrayId=ArrayByRectangle(spaceId, objectIdArr, basePoint, rowCount, rowSpacing, columnCount, columnSpacing, RowElevation, levelCount, levelSpacing, isDelete);
            return arrayId;
        }







        /// <summary>
        /// 对指定对象数组进行矩形阵列
        /// </summary>
        /// <param name="objectIdArr">对象的数组</param>
        /// <param name="basePoint">基点</param>
        /// <param name="rowCount">行数</param>
        /// <param name="rowSpacing">行间距</param>
        /// <param name="columnCount">列数</param>
        /// <param name="columnSpacing">列间距</param>
        /// <param name="RowElevation"></param>
        /// <param name="levelCount">z方向上的个数,如果为0 将报错：eMustBePositive， 应该是说z方向应该有一排</param>
        /// <param name="levelSpacing">z方向的间距</param>
        /// <param name="isDelete">是否删除原来的对象，默认是</param>
        /// <param name="spaceName">模型空间还是图纸空间名称，不分大小写，默认模型空间</param>
        /// <returns>阵列的ObjectId，如果失败，返回的ObjectId.Null</returns>
        public ObjectId ArrayByRectangle(ObjectId[] objectIdArr, Point3d basePoint, int rowCount = 10, double rowSpacing = 10, int columnCount = 10, double columnSpacing = 10, double RowElevation = 0, int levelCount = 1, double levelSpacing = 10, bool isDelete = true, string spaceName = "MODELSPACE")
        {

            //返回值
            ObjectId arrayId = ObjectId.Null;

            //先获取空间
            SpaceTool spaceTool = new SpaceTool(m_database);
            ObjectId spaceId = spaceTool.GetSpaceByName(spaceName);

            if (spaceId.IsNull)
            {
                return arrayId;
            }


            arrayId= ArrayByRectangle(spaceId, objectIdArr, basePoint, rowCount, rowSpacing, columnCount, columnSpacing, RowElevation, levelCount, levelSpacing, isDelete);

            return arrayId;
        }


        /// <summary>
        /// 对指定对象数组进行矩形阵列
        /// </summary>
        /// <param name="spaceId">空间对象的ObjectId</param>
        /// <param name="objectIdArr">对象的数组</param>
        /// <param name="basePoint">基点</param>
        /// <param name="rowCount">行数</param>
        /// <param name="rowSpacing">行间距</param>
        /// <param name="columnCount">列数</param>
        /// <param name="columnSpacing">列间距</param>
        /// <param name="rowElevation"></param>
        /// <param name="levelCount">z方向上的个数,如果为0 将报错：eMustBePositive， 应该是说z方向应该有一排</param>
        /// <param name="levelSpacing">z方向的间距</param>
        /// <param name="isDelete">是否删除原来的对象，默认是</param>
        /// <returns>阵列的ObjectId，如果失败，返回的ObjectId.Null</returns>
        public ObjectId ArrayByRectangle(ObjectId spaceId, ObjectId[] objectIdArr, Point3d basePoint, int rowCount = 10, double rowSpacing = 10, int columnCount = 10, double columnSpacing = 10, double rowElevation = 0, int levelCount = 1, double levelSpacing = 10, bool isDelete = true)
        {
            //返回值
            ObjectId arrayId = ObjectId.Null;

            if (spaceId.IsNull|| objectIdArr == null || objectIdArr.Length==0)
            {
                return arrayId;
            }


            LayoutTool layoutTool = new LayoutTool(m_database);


            //先获取当前的布局
            string currentLayout = layoutTool.GetCurrentLayout();

            //切换到指定的布局
            //layoutTool.SetCurrentLayoutBySpace(spaceId);
            layoutTool.SetCurrentLayout("Model");



            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    //中望CAD中没有对应的函数
                    //AssocArrayRectangularParameters rectParams = new AssocArrayRectangularParameters()
                    //{
                    //    ColumnCount = columnCount,
                    //    ColumnSpacing = columnSpacing,
                    //    RowCount = rowCount,
                    //    RowSpacing = rowSpacing,
                    //    RowElevation = rowElevation,
                    //    LevelCount = levelCount,
                    //    LevelSpacing = levelSpacing,
                    //    XAxisDirection = Vector3d.XAxis,
                    //    YAxisDirection = Vector3d.YAxis,
                    //    BasePoint = new VertexRef(basePoint),
                    //};

                    //ObjectIdCollection objectIdCollection = new ObjectIdCollection(objectIdArr);
                    //AssocArray assocArray = AssocArray.CreateArray(objectIdCollection, rectParams.BasePoint, rectParams);

                    ////这个EvaluateTopLevelNetwork需要 不然可能不会正常显示
                    //// Evaluate the array

                    //AssocManager.EvaluateTopLevelNetwork(m_database, null, 0);

                    //arrayId=assocArray.EntityId;

                    //删除原来的对象
                    if (isDelete)
                    {
                        foreach (var item in objectIdArr)
                        {
                            DBObject oldDbObject = transaction.GetObject(item, OpenMode.ForWrite);
                            oldDbObject.Erase(true);
                        }
                    }


                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();

                }

            }

            //结束之后，切换回原来的布局
            layoutTool.SetCurrentLayout(currentLayout);



            //////以上经过测试之后，发现直接切换到指定空间中，不一定能生成成功，所以改为先切换到模型空间，生成好对象之后，再将对象移动到指定空间

            //if (!arrayId.IsNull)
            //{

            //    SpaceTool spaceTool = new SpaceTool(m_database);
            //    arrayId=  spaceTool.MoveToNewSpace(spaceId, arrayId);
            //}


            return arrayId;
        }














        /// <summary>
        /// 对指定对象进行环形阵列
        /// </summary>
        /// <param name="objectId">对象的ObjectId</param>
        /// <param name="basePoint">基点</param>
        /// <param name="radius">半径</param>
        /// <param name="startAngle">起始角度（角度制）</param>
        /// <param name="endAngle">终始角度（角度制）</param>
        /// <param name="angleBetweenItems">相邻两对象之间的角度（角度制）</param>
        /// <param name="itemCount">对象的个数</param>
        /// <param name="rowCount">行数</param>
        /// <param name="rowSpacing">行间距</param>
        /// <param name="rowElevation"></param>
        /// <param name="rotateItems">是否旋转对象</param>
        /// <param name="isDelete">是否删除原来的对象，默认是</param>
        /// <param name="spaceName">模型空间还是图纸空间名称，不分大小写，默认模型空间</param>
        /// <returns>阵列的ObjectId，如果失败，返回的ObjectId.Null</returns>
        public ObjectId ArrayByPolar(ObjectId objectId, Point3d basePoint, double radius = 30, double startAngle = 0, double endAngle = 360, double angleBetweenItems = 10, int itemCount = 36, int rowCount = 10, double rowSpacing = 10, double rowElevation = 10, bool rotateItems = true, bool isDelete = true, string spaceName = "MODELSPACE")
        {
            //返回值
            ObjectId arrayId = ObjectId.Null;

            //先获取空间
            SpaceTool spaceTool = new SpaceTool(m_database);
            ObjectId spaceId = spaceTool.GetSpaceByName(spaceName);

            if (spaceId.IsNull)
            {
                return arrayId;
            }


            arrayId=ArrayByPolar(spaceId, objectId, basePoint, radius, startAngle, endAngle, angleBetweenItems, itemCount, rowCount, rowSpacing, rowElevation, rotateItems, isDelete);

            return arrayId;

        }





        /// <summary>
        /// 对指定对象进行环形阵列
        /// </summary>
        /// <param name="spaceId">空间对象的ObjectId</param>
        /// <param name="objectId">对象的ObjectId</param>
        /// <param name="basePoint">基点</param>
        /// <param name="radius">半径</param>
        /// <param name="startAngle">起始角度（角度制）</param>
        /// <param name="endAngle">终始角度（角度制）</param>
        /// <param name="angleBetweenItems">相邻两对象之间的角度（角度制）</param>
        /// <param name="itemCount">对象的个数</param>
        /// <param name="rowCount">行数</param>
        /// <param name="rowSpacing">行间距</param>
        /// <param name="rowElevation"></param>
        /// <param name="rotateItems">是否旋转对象</param>
        /// <param name="isDelete">是否删除原来的对象，默认是</param>
        /// <returns>阵列的ObjectId，如果失败，返回的ObjectId.Null</returns>
        public ObjectId ArrayByPolar(ObjectId spaceId, ObjectId objectId, Point3d basePoint, double radius = 30, double startAngle = 0, double endAngle = 360, double angleBetweenItems = 10, int itemCount = 36, int rowCount = 10, double rowSpacing = 10, double rowElevation = 10, bool rotateItems = true, bool isDelete = true)
        {
            //返回值
            ObjectId arrayId = ObjectId.Null;

            if (objectId.IsNull)
            {
                return arrayId;
            }

            ObjectId[] objectIdArr = new ObjectId[]
            {
                objectId
            };

            arrayId=ArrayByPolar(spaceId, objectIdArr, basePoint, radius, startAngle, endAngle, angleBetweenItems, itemCount, rowCount, rowSpacing, rowElevation, rotateItems, isDelete);
            return arrayId;
        }





        /// <summary>
        /// 对指定对象列表进行环形阵列
        /// </summary>
        /// <param name="objectIdLst">对象的列表</param>
        /// <param name="basePoint">基点</param>
        /// <param name="radius">半径</param>
        /// <param name="startAngle">起始角度（角度制）</param>
        /// <param name="endAngle">终始角度（角度制）</param>
        /// <param name="angleBetweenItems">相邻两对象之间的角度（角度制）</param>
        /// <param name="itemCount">对象的个数</param>
        /// <param name="rowCount">行数</param>
        /// <param name="rowSpacing">行间距</param>
        /// <param name="rowElevation"></param>
        /// <param name="rotateItems">是否旋转对象</param>
        /// <param name="isDelete">是否删除原来的对象，默认是</param>
        /// <param name="spaceName">模型空间还是图纸空间名称，不分大小写，默认模型空间</param>
        /// <returns>阵列的ObjectId，如果失败，返回的ObjectId.Null</returns>
        public ObjectId ArrayByPolar(List<ObjectId> objectIdLst, Point3d basePoint, double radius = 30, double startAngle = 0, double endAngle = 360, double angleBetweenItems = 10, int itemCount = 36, int rowCount = 10, double rowSpacing = 10, double rowElevation = 10, bool rotateItems = true, bool isDelete = true, string spaceName = "MODELSPACE")
        {
            //返回值
            ObjectId arrayId = ObjectId.Null;

            //先获取空间
            SpaceTool spaceTool = new SpaceTool(m_database);
            ObjectId spaceId = spaceTool.GetSpaceByName(spaceName);

            if (spaceId.IsNull)
            {
                return arrayId;
            }


            arrayId= ArrayByPolar(spaceId, objectIdLst, basePoint, radius, startAngle, endAngle, angleBetweenItems, itemCount, rowCount, rowSpacing, rowElevation, rotateItems, isDelete);

            return arrayId;

        }



        /// <summary>
        /// 对指定对象列表进行环形阵列
        /// </summary>
        /// <param name="spaceId">空间对象的ObjectId</param>
        /// <param name="objectIdLst">对象的列表</param>
        /// <param name="basePoint">基点</param>
        /// <param name="radius">半径</param>
        /// <param name="startAngle">起始角度（角度制）</param>
        /// <param name="endAngle">终始角度（角度制）</param>
        /// <param name="angleBetweenItems">相邻两对象之间的角度（角度制）</param>
        /// <param name="itemCount">对象的个数</param>
        /// <param name="rowCount">行数</param>
        /// <param name="rowSpacing">行间距</param>
        /// <param name="rowElevation"></param>
        /// <param name="rotateItems">是否旋转对象</param>
        /// <param name="isDelete">是否删除原来的对象，默认是</param>
        /// <returns>阵列的ObjectId，如果失败，返回的ObjectId.Null</returns>
        public ObjectId ArrayByPolar(ObjectId spaceId, List<ObjectId> objectIdLst, Point3d basePoint, double radius = 30, double startAngle = 0, double endAngle = 360, double angleBetweenItems = 10, int itemCount = 36, int rowCount = 10, double rowSpacing = 10, double rowElevation = 10, bool rotateItems = true, bool isDelete = true)
        {

            //返回值
            ObjectId arrayId = ObjectId.Null;

            if (objectIdLst == null || objectIdLst.Count==0)
            {
                return arrayId;
            }

            ObjectId[] objectIdArr = objectIdLst.ToArray();
            arrayId=ArrayByPolar(spaceId, objectIdArr, basePoint, radius, startAngle, endAngle, angleBetweenItems, itemCount, rowCount, rowSpacing, rowElevation, rotateItems, isDelete);
            return arrayId;
        }










        /// <summary>
        /// 对指定对象数组进行环形阵列
        /// </summary>
        /// <param name="objectIdArr">对象的数组</param>
        /// <param name="basePoint">基点</param>
        /// <param name="radius">半径</param>
        /// <param name="startAngle">起始角度（角度制）</param>
        /// <param name="endAngle">终始角度（角度制）</param>
        /// <param name="angleBetweenItems">相邻两对象之间的角度（角度制）</param>
        /// <param name="itemCount">对象的个数</param>
        /// <param name="rowCount">行数</param>
        /// <param name="rowSpacing">行间距</param>
        /// <param name="rowElevation"></param>
        /// <param name="rotateItems">是否旋转对象</param>
        /// <param name="isDelete">是否删除原来的对象，默认是</param>
        /// <param name="spaceName">模型空间还是图纸空间名称，不分大小写，默认模型空间</param>
        /// <returns>阵列的ObjectId，如果失败，返回的ObjectId.Null</returns>
        public ObjectId ArrayByPolar(ObjectId[] objectIdArr, Point3d basePoint, double radius = 30, double startAngle = 0, double endAngle = 360, double angleBetweenItems = 10, int itemCount = 36, int rowCount = 10, double rowSpacing = 10, double rowElevation = 10, bool rotateItems = true, bool isDelete = true, string spaceName = "MODELSPACE")
        {
            //返回值
            ObjectId arrayId = ObjectId.Null;

            //先获取空间
            SpaceTool spaceTool = new SpaceTool(m_database);
            ObjectId spaceId = spaceTool.GetSpaceByName(spaceName);

            if (spaceId.IsNull)
            {
                return arrayId;
            }

            arrayId=ArrayByPolar(spaceId, objectIdArr, basePoint, radius, startAngle, endAngle, angleBetweenItems, itemCount, rowCount, rowSpacing, rowElevation, rotateItems, isDelete);

            return arrayId;
        }






        /// <summary>
        /// 对指定对象数组进行环形阵列
        /// </summary>
        /// <param name="spaceId">空间对象的ObjectId</param>
        /// <param name="objectIdArr">对象的数组</param>
        /// <param name="basePoint">基点</param>
        /// <param name="radius">半径</param>
        /// <param name="startAngle">起始角度（角度制）</param>
        /// <param name="endAngle">终始角度（角度制）</param>
        /// <param name="angleBetweenItems">相邻两对象之间的角度（角度制）</param>
        /// <param name="itemCount">对象的个数</param>
        /// <param name="rowCount">行数</param>
        /// <param name="rowSpacing">行间距</param>
        /// <param name="rowElevation"></param>
        /// <param name="rotateItems">是否旋转对象</param>
        /// <param name="isDelete">是否删除原来的对象，默认是</param>
        /// <returns>阵列的ObjectId，如果失败，返回的ObjectId.Null</returns>
        public ObjectId ArrayByPolar(ObjectId spaceId, ObjectId[] objectIdArr, Point3d basePoint, double radius = 30, double startAngle = 0, double endAngle = 360, double angleBetweenItems = 10, int itemCount = 36, int rowCount = 10, double rowSpacing = 10, double rowElevation = 10, bool rotateItems = true, bool isDelete = true)
        {
            //返回值
            ObjectId arrayId = ObjectId.Null;

            if (spaceId.IsNull|| objectIdArr == null || objectIdArr.Length==0)
            {
                return arrayId;
            }


            LayoutTool layoutTool = new LayoutTool(m_database);


            //先获取当前的布局
            string currentLayout = layoutTool.GetCurrentLayout();

            //切换到指定的布局
            layoutTool.SetCurrentLayoutBySpace(spaceId);


            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    //中望CAD中没有对应的函数
                    //AssocArrayPolarParameters polarParams = new AssocArrayPolarParameters()
                    //{
                    //    FillAngle = endAngle,
                    //    AngleBetweenItems = angleBetweenItems,
                    //    ItemCount = itemCount,
                    //    RowCount = rowCount,
                    //    RowSpacing = rowSpacing,
                    //    RowElevation = rowElevation,
                    //    Radius = radius,
                    //    Direction = AssocArrayPolarParameters.ArcDirection.CounterClockwise,
                    //    StartAngle = startAngle,
                    //    RotateItems = rotateItems
                    //};

                    //ObjectIdCollection objectIdCollection = new ObjectIdCollection(objectIdArr);

                    //VertexRef vertexRef = new VertexRef(basePoint);


                    //AssocArray assocArray = AssocArray.CreateArray(objectIdCollection, vertexRef, polarParams);
                    //// Evaluate the array

                    //AssocManager.EvaluateTopLevelNetwork(m_database, null, 0);

                    //arrayId=assocArray.EntityId;


                    //删除原来的对象
                    if (isDelete)
                    {
                        foreach (var item in objectIdArr)
                        {
                            DBObject oldDbObject = transaction.GetObject(item, OpenMode.ForWrite);
                            oldDbObject.Erase(true);
                        }
                    }


                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();

                }


            }


            //结束之后，切换回原来的布局
            layoutTool.SetCurrentLayout(currentLayout);

            return arrayId;

        }




    }
}
