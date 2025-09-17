using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.Geometry;
using System;
using System.Collections.Generic;

namespace ZWCAD.BaseTools
{

    /// <summary>
    /// 填充对象工具
    /// </summary>
    public class HatchTool
    {

        Document m_document;

        Database m_database;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="document">文档对象</param>
        public HatchTool(Document document)
        {
            m_document = document;
            m_database = m_document.Database;
        }


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="database">数据库</param>
        public HatchTool(Database database)
        {
            m_database = database;
        }




        /// <summary>
        /// 获取填充对象的边界，判断是否为填充对象
        /// </summary>
        /// <param name="hatchId">填充对象的ObjectId</param>
        /// <param name="numSample">提取单位曲线中样本点的数目，程序会自动等分</param>
        /// <returns>边界线点集合列表,如果不为填充对象，或者如果获取不成功，返回空的列表</returns>
        public List<Point2dCollection> GetBorders(ObjectId hatchId, int numSample)
        {
            //返回值
            List<Point2dCollection> point2DCollections = new List<Point2dCollection>();

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {


                DBObject obj = transaction.GetObject(hatchId, OpenMode.ForRead, true);

                Hatch hatch = obj as Hatch;

                if (hatch == null)  //如果不是填充对象，返回空的列表
                {
                    return point2DCollections;
                }

                return GetBorders(hatch, numSample);
            }


        }








        /// <summary>
        /// 获取填充对象的边界
        /// </summary>
        /// <param name="hat">填充对象</param>
        /// <param name="numSample">提取单位曲线中样本点的数目，程序会自动等分</param>
        /// <returns>边界线点集合列表,如果获取不成功，返回空的列表</returns>
        public List<Point2dCollection> GetBorders(Hatch hat, int numSample)
        {

            //返回值
            List<Point2dCollection> point2DCollections = new List<Point2dCollection>();

            //取得边界数
            int loopNum = hat.NumberOfLoops;

            for (int i = 0; i < loopNum; i++)
            {
                Point2dCollection col_point2d = new Point2dCollection();

                HatchLoop hatLoop;
                try
                {
                    hatLoop = hat.GetLoopAt(i);
                }
                catch (System.Exception)
                {
                    continue;
                }

                //如果HatchLoop为PolyLine
                if (hatLoop.IsPolyline)
                {
                    BulgeVertexCollection col_ver = hatLoop.Polyline;
                    foreach (BulgeVertex vertex in col_ver)
                    {

                        //if (!col_point2d.Contains(vertex.Vertex))
                        col_point2d.Add(vertex.Vertex);
                    }
                }
                //不为PolyLine就为Curves
                else
                {
                    Curve2dCollection col_cur2d = hatLoop.Curves;
                    foreach (Curve2d item in col_cur2d)
                    {
                        Point2d[] M_point2d = item.GetSamplePoints(numSample);
                        foreach (Point2d point in M_point2d)
                        {
                            if (!col_point2d.Contains(point))  //这种情况，如果第一个点和最后一个点重复，会漏掉最后一个点
                                col_point2d.Add(point);
                        }
                    }
                }



                if (col_point2d.Count == 0)
                {
                    continue;
                }


                point2DCollections.Add(col_point2d);


            }

            return point2DCollections;

        }















        /// <summary>
        /// 创建填充图案的边界，判断是否为填充图案
        /// </summary>
        /// <param name="hatchObjectId">填充图案的ObjectId</param>
        /// <param name="numSample">提取单位曲线中样本点的数目</param>
        /// <param name="spaceName">空间名称，默认模型空间</param>
        /// <returns>如果对象不为填充图案，返回null，如果为填充图案，返回创建边界的ObjectId列表，如果创建不成功，返回空的列表</returns>
        public List<ObjectId> DrawBorder(ObjectId hatchObjectId, int numSample, string spaceName = "MODELSPACE")
        {

            ObjectTool objectTool = new ObjectTool(m_database);
            Hatch hatch = objectTool.GetObject(hatchObjectId) as Hatch;

            if (hatch == null)
            {
                //如果不是填充图案
                return null;
            }

            return DrawBorders(hatch, numSample, spaceName);


        }


        /// <summary>
        /// 创建填充图案的边界，判断是否为填充图案
        /// </summary>
        /// <param name="spaceId">空间对象的OjectId</param>
        /// <param name="hatchObjectId">填充图案的ObjectId</param>
        /// <param name="numSample">提取单位曲线中样本点的数目</param>
        /// <returns>如果对象不为填充图案，返回null，如果为填充图案，返回创建边界的ObjectId列表，如果创建不成功，返回空的列表</returns>
        public List<ObjectId> DrawBorder(ObjectId spaceId, ObjectId hatchObjectId, int numSample)
        {
            SpaceTool spaceTool = new SpaceTool(m_database);
            string spaceName = spaceTool.GetSpaceName(spaceId);
            return DrawBorder(hatchObjectId, numSample, spaceName);
        }



        /// <summary>
        /// 获取填充对象的边界，并创建相应的多段线
        /// </summary>
        /// <param name="hat">填充对象</param>
        /// <param name="numSample">提取单位曲线中样本点的数目，程序会自动等分</param>
        /// <param name="spaceName">空间名称，默认模型空间</param>
        /// <returns>创建的多段线的列表,如果创建都不成功，返回空的列表</returns>
        public List<ObjectId> DrawBorders(Hatch hat, int numSample, string spaceName = "MODELSPACE")
        {

            //返回值
            List<ObjectId> objectIds = new List<ObjectId>();

            //这一步如果第一点和最后一点重复，将会漏掉最后一个点，将在下面创建边界线时让其闭合
            List<Point2dCollection> point2DCollections = GetBorders(hat, numSample);

            if (point2DCollections.Count == 0)
            {
                return objectIds;
            }


            PolylineTool polylineTool = new PolylineTool(m_database);

            foreach (Point2dCollection col_point2d in point2DCollections)
            {

                //有些边界线不闭合，让它闭合
                ObjectId objectId = polylineTool.CreatePolyline(col_point2d, true, 0, spaceName);

                if (!objectId.IsNull) //创建成功
                {
                    objectIds.Add(objectId);

                }

            }

            return objectIds;
        }



        /// <summary>
        /// 获取填充对象的边界，并创建相应的多段线
        /// </summary>
        /// <param name="spaceId">空间对象的OjectId</param>
        /// <param name="hat">填充对象</param>
        /// <param name="numSample">提取单位曲线中样本点的数目，程序会自动等分</param>
        /// <returns>创建的多段线的列表,如果创建都不成功，返回空的列表</returns>
        public List<ObjectId> DrawBorders(ObjectId spaceId, Hatch hat, int numSample)
        {
            SpaceTool spaceTool = new SpaceTool(m_database);
            string spaceName = spaceTool.GetSpaceName(spaceId);

            List<ObjectId> objectIds = DrawBorders(hat, numSample, spaceName);

            return objectIds;

        }




        /// <summary>
        /// 根据边界创建填充图案
        /// </summary>
        /// <param name="objectId">边界对象的ObjectId</param>
        /// <param name="patternType">图案类型，默认HatchPatternType.PreDefined</param>
        /// <param name="patternName">图案名称，默认"SOLID"</param>
        /// <param name="patternScale">图案比例，默认1</param>
        /// <param name="isAssociative">是否关联，默认关联</param>
        /// <param name="spaceName">空间名称，默认模型空间</param>
        /// <returns>如果创建成功，返回填充图案的ObjectId，否则，返回ObjectId.Null</returns>
        public ObjectId CreateHatch(ObjectId objectId, HatchPatternType patternType = HatchPatternType.PreDefined, string patternName = "SOLID", double patternScale = 1, bool isAssociative = true, string spaceName = "MODELSPACE")
        {
            //返回值
            ObjectId hatchId = ObjectId.Null;
            if (objectId.IsNull)
            {
                return hatchId;
            }

            List<ObjectId> objectIds = new List<ObjectId> { objectId };

            hatchId= CreateHatch(objectIds, patternType, patternName, patternScale, isAssociative, spaceName);
            return hatchId;
        }


        /// <summary>
        /// 根据边界创建填充图案
        /// </summary>
        /// <param name="spaceId">空间对象的OjectId</param>
        /// <param name="objectId">边界对象的ObjectId</param>
        /// <param name="patternType">图案类型，默认HatchPatternType.PreDefined</param>
        /// <param name="patternName">图案名称，默认"SOLID"</param>
        /// <param name="patternScale">图案比例，默认1</param>
        /// <param name="isAssociative">是否关联，默认关联</param>
        /// <returns>如果创建成功，返回填充图案的ObjectId，否则，返回ObjectId.Null</returns>
        public ObjectId CreateHatch(ObjectId spaceId, ObjectId objectId, HatchPatternType patternType = HatchPatternType.PreDefined, string patternName = "SOLID", double patternScale = 1, bool isAssociative = true)
        {

            //返回值
            ObjectId hatchId=ObjectId.Null;
            if(spaceId.IsNull || objectId.IsNull)
            {
                return hatchId ;
            }

            List<ObjectId> objectIdLst = new List<ObjectId>
            {
                objectId
            };


            return CreateHatch(spaceId, objectIdLst, patternType, patternName,patternScale, isAssociative);
        }





        /// <summary>
        /// 根据边界创建填充图案
        /// </summary>
        /// <param name="objectIds">边界对象的ObjectId列表</param>
        /// <param name="patternType">图案类型，默认HatchPatternType.PreDefined</param>
        /// <param name="patternName">图案名称，默认"SOLID"</param>
        /// <param name="patternScale">图案比例，默认1</param>
        /// <param name="isAssociative">是否关联，默认关联</param>
        /// <param name="spaceName">空间名称，默认模型空间</param>
        /// <returns>如果创建成功，返回填充图案的ObjectId，否则，返回ObjectId.Null</returns>
        public ObjectId CreateHatch(List<ObjectId> objectIds, HatchPatternType patternType = HatchPatternType.PreDefined, string patternName = "SOLID", double patternScale = 1, bool isAssociative = true, string spaceName = "MODELSPACE")
        {
            //返回值
            ObjectId objectId = ObjectId.Null;

            using (Transaction acTrans = m_database.TransactionManager.StartTransaction())
            {
                // Open the Block table for read以读打开块表
                BlockTable acBlkTbl = acTrans.GetObject(m_database.BlockTableId, OpenMode.ForRead) as BlockTable;


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

                ObjectId spaceId = acBlkTbl[spaceName];

                objectId= CreateHatch(spaceId, objectIds, patternType, patternName,patternScale, isAssociative);

                acTrans.Commit();
            }

            return objectId;

        }






        /// <summary>
        /// 根据边界创建填充图案
        /// </summary>
        /// <param name="spaceId">空间对象的OjectId</param>
        /// <param name="objectIds">边界对象的ObjectId列表</param>
        /// <param name="patternType">图案类型，默认HatchPatternType.PreDefined</param>
        /// <param name="patternName">图案名称，默认"SOLID"</param>
        /// <param name="patternScale">图案比例，默认1</param>
        /// <param name="isAssociative">是否关联，默认关联</param>
        /// <returns>如果创建成功，返回填充图案的ObjectId，否则，返回ObjectId.Null</returns>
        public ObjectId CreateHatch(ObjectId spaceId, List<ObjectId> objectIds, HatchPatternType patternType = HatchPatternType.PreDefined, string patternName = "SOLID",double patternScale=1, bool isAssociative = true)
        {
            //返回值
            ObjectId objectId = ObjectId.Null;

            //将边界面积按大到小排列,确保第一个为外边界
            ObjectTool objectTool = new ObjectTool(m_database);
            objectIds.Sort((x, y) => -objectTool.GetEntityBoundingArea(x).CompareTo(objectTool.GetEntityBoundingArea(y)));


            try
            {
                using (Transaction acTrans = m_database.TransactionManager.StartTransaction())
                {
                    // Open the Block table record Model space for write以写打开块表记录模型空间
                    if (acTrans.GetObject(spaceId, OpenMode.ForWrite) is BlockTableRecord acBlkTblRec)
                    {

                        // Adds the arc and line to an object id collection将圆弧和直线添加到ObjectIdCollection
                        //ObjectIdCollection acObjIdColl = new ObjectIdCollection(objectIds.ToArray());
                        ObjectIdCollection acObjIdColl = new ObjectIdCollection();


                        // Create the hatch object and append it to the block table record创建Hatch对象
                        Hatch acHatch = new Hatch();
                        acBlkTblRec.AppendEntity(acHatch);
                        acTrans.AddNewlyCreatedDBObject(acHatch, true);

                        // Set the properties of the hatch object设置填充对象的属性
                        // Associative must be set after the hatch object is appended to the
                        // block table record and before AppendLoop关联属性必须在将填充对象添加到块表记录之后、执行AppendLoop之前设置
                        acHatch.SetHatchPattern(patternType, patternName);

                        acHatch.PatternScale = patternScale;

                        acHatch.Associative = isAssociative;
                        //acHatch.AppendLoop(HatchLoopTypes.Outermost, acObjIdColl);

                        //先添加第一个，后续的再一个个添加
                        acObjIdColl.Add(objectIds[0]);

                        try
                        {
                            acHatch.AppendLoop(HatchLoopTypes.Outermost, acObjIdColl);
                        }
                        catch (Exception e)  //创建失败，直接返回
                        {
                            string massage = e.Message;
                            acTrans.Abort();
                            return objectId;
                        }

                        for (int i = 1; i < objectIds.Count; i++)
                        {
                            acObjIdColl.Clear();
                            acObjIdColl.Add(objectIds[i]);

                            acHatch.AppendLoop(HatchLoopTypes.Default, acObjIdColl);
                        }


                        // Append the circle as the inner loop of the hatch and evaluate it追加圆为内部边界环并对填充对象取值
                        //acHatch.AppendLoop(HatchLoopTypes.Default, acObjIdColl);
                        acHatch.EvaluateHatch(true);

                        objectId = acHatch.Id;

                    }
                    // Save the new object to the database保存到数据库
                    acTrans.Commit();
                }
            }
            catch  //错误，返回ObjectId.Null
            {
                return objectId;
            }

            return objectId;
        }








        /// <summary>
        /// 替换填充图案,判断是否为填充图案
        /// </summary>
        /// <param name="hatchObjectId">填充图案的ObjectId</param>
        /// <returns>新创建图案的ObjectId，如果创建不成功，返回ObjectId.Null</returns>
        public ObjectId ReplaceHatch(ObjectId hatchObjectId)
        {

            ObjectTool objectTool = new ObjectTool(m_database);
            Hatch hatch = objectTool.GetObject(hatchObjectId) as Hatch;

            if (hatch == null)
            {
                return ObjectId.Null;
            }
            return ReplaceHatch(hatch);

        }




        /// <summary>
        /// 替换填充图案
        /// </summary>
        /// <param name="hatch">填充图案</param>
        /// <returns>新创建图案的ObjectId，如果创建不成功，返回ObjectId.Null</returns>
        public ObjectId ReplaceHatch(Hatch hatch)
        {
            //创建边界
            List<ObjectId> objectIds = DrawBorders(hatch, 3);


            //如果创建边界对象不成功
            if (objectIds.Count == 0)
            {
                return ObjectId.Null;
            }










            ////创建填充图案
            //HatchPatternType patternType = hatch.PatternType;
            //string patternName = hatch.PatternName;
            //ObjectId objectId = CreateHatch(objectIds, patternType, patternName);
            ObjectId objectId = CreateHatch(objectIds);


            //如果创建图案不成功
            if (objectId.IsNull)
            {
                return ObjectId.Null;
            }



            //修改图层
            objectIds.Add(objectId);
            LayerTool layerTool = new LayerTool(m_database);
            layerTool.ChangeEntitysLayer(objectIds, hatch.Layer);




            //删除原来的填充对象
            ObjectTool objectTool = new ObjectTool(m_database);
            objectTool.EraseDBObject(hatch.ObjectId);


            //返回新创建对象的ObjectId
            return objectId;

        }




        /// <param name="hat">需要转化的Hatch对象</param>    
        /// <param name="shp">shp=false:常规坐标集合列表！随机方向；首尾点不重合！默认为 false
        /// shp=true:shp文件坐标集合列表！外环正向，内环逆向；首尾点重合！
        /// </param>    
        /// <param name="pointDisdance">取点间隔：当子曲线类型不等于ZwSoft.ZwCAD.Geometry.LineSegment2d 时，按给定边长取点,默认为1</param>    
        public List<Point2dCollection> BoundaryPoint2dCollectionLst(Hatch hat, bool shp = false, double pointDisdance = 1)
        {

            //返回值
            List<Point2dCollection> ptss = new List<Point2dCollection>();
            for (int i = 0; i < hat.NumberOfLoops; i++)  //历遍边界（环）
            {
                Point2dCollection col_point2d = new Point2dCollection();
                HatchLoop hatLoop = hat.GetLoopAt(i);
                Curve2dCollection col_cur2d = hatLoop.Curves;
                foreach (Curve2d item in col_cur2d)  //历遍曲线段
                {
                    int pointNumber = 2;

                    //if (item.GetType().ToString() != "ZwSoft.ZwCAD.Geometry.LineSegment2d")  //直线
                    if (!(item is LineSegment2d))  //直线

                    {
                        Interval interval = item.GetInterval();   //获取曲线的间隔（上、下界）
                        double curveLength = item.GetLength(interval.LowerBound, interval.UpperBound);
                        pointNumber = (int)(curveLength / pointDisdance);
                    }
                    Point2d[] M_point2d = item.GetSamplePoints(pointNumber);
                    foreach (Point2d point in M_point2d)
                    {
                        if (!col_point2d.Contains(point))
                            col_point2d.Add(point);
                    }
                }


                //这段代码的作用是是否考虑首尾点重合，如果shp，那么首尾点重合，否则，首尾点不重合
                if (shp)   //shp == true shp文件坐标数组，面对象中环线坐标需回到起点,并且外环正向，内环逆向
                {
                    //是否为第一个环  不明白这个的作用
                    int j = i == 0 ? 1 : -1;

                    //环是顺时针还是逆时针
                    int n = PointCollectionArea(col_point2d) > 0 ? 1 : -1;

                    Point2dCollection spt;


                    if (j * n == -1)    //外环正向，内环逆向
                        spt = ReversePointCollection(col_point2d);
                    else
                        spt = col_point2d;
                    Point2d point = spt[0];
                    spt.Add(point);
                    ptss.Add(spt);
                }

                else //获取常规坐标数组
                {
                    ptss.Add(col_point2d);

                }



            }
            return ptss;
        }



        /// <summary>
        /// 判断点集合为顺时针还是逆时针，还不是很理解这个方法
        /// </summary>
        /// <param name="pts">点集合</param>
        /// <returns>当面积为正值，多边形为顺时针；当面积为负值，多边形为逆时针。</returns>
        private double PointCollectionArea(Point2dCollection pts)
        {
            double area = 0;
            for (int i = 0; i < pts.Count; i++)
            {
                //判断i是否为最后一个点，如果是，返回第一点，如果不是，返回该点的下一点
                int v = (i == pts.Count - 1) ? 0 : i + 1;

                area += pts[i].X * pts[v].Y - pts[i].Y * pts[v].X;
            }
            return area / (-2);
        }




        /// <summary>
        /// 将点集合反向
        /// </summary>
        /// <param name="pts">点集合</param>
        /// <returns>反向后的点集合</returns>
        private Point2dCollection ReversePointCollection(Point2dCollection pts)
        {
            Point2dCollection ptss = new Point2dCollection();
            for (int i = pts.Count - 1; i > -1; i--)
            {
                Point2d point = pts[i];
                ptss.Add(point);
            }
            return ptss;
        }



    }
}
