using ZwSoft.ZwCAD.Geometry;
using ZwSoft.ZwCAD.DatabaseServices;
using Mrf.CSharp.BaseTools.Extension;
using System;
using System.Collections.Generic;

namespace ZWCAD.BaseTools.Extension
{

    /// <summary>
    /// 添加实体对象扩展
    /// </summary>
    public static partial class AddEntityExtension
    {



        /// <summary>
        /// 在模型空间或图纸空间将图形对象添加到图形文件中
        /// </summary>
        /// <param name="database">图形数据库</param>
        /// <param name="ent">图形对象</param>s
        /// <param name="spaceId">空间的ObjectId</param>
        /// <returns>图形的ObjectId,如果创建失败，返回ObjectId.Null</returns>
        public static ObjectId AddEntity(this Database database, ObjectId spaceId, Entity ent)
        {

            //声明ObjectId，用于返回
            ObjectId entId = ObjectId.Null;

            if (spaceId.IsNull)
            {
                return entId;
            }


            //开启事务处理
            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                try
                {
                    //打开块表记录
                    if (transaction.GetObject(spaceId, OpenMode.ForWrite) is BlockTableRecord btr)
                    {
                        //添加图形到块表记录
                        entId = btr.AppendEntity(ent);
                        //更新数据信息
                        transaction.AddNewlyCreatedDBObject(ent, true);
                        transaction.Commit();
                    }

                }
                catch
                {
                    transaction.Abort();
                }
            }
            return entId;
        }




        /// <summary>
        /// 在模型空间或图纸空间将图形对象添加到图形文件中
        /// </summary>
        /// <param name="database">图形数据库</param>
        /// <param name="ent">图形对象</param>s
        /// <param name="spaceName">模型空间或图纸空间，如果为"PAPERSPACE"（不分大小写），为图纸空间，如果指定的名称为其它的图纸空间名称，则为指定的图纸空间名称；为空或其它，为模型空间</param>
        /// <returns>图形的ObjectId,如果创建失败，返回ObjectId.Null</returns>
        public static ObjectId AddEntity(this Database database, Entity ent, string spaceName = "MODELSPACE")
        {
            //声明ObjectId，用于返回
            ObjectId entId = ObjectId.Null;
            //开启事务处理
            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                //打开块表
                BlockTable blockTable = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForRead);

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

                ObjectId spaceId = blockTable[spaceName];

                entId=database.AddEntity(spaceId, ent);

                transaction.Commit();
            }
            return entId;
        }



        /// <summary>
        /// 在模型空间或图纸空间将图形对象添加到图形文件中
        /// </summary>
        /// <param name="database">图形数据库</param>
        /// <param name="entLst">图形对象列表</param>s
        /// <param name="spaceId">空间对象的ObjectId</param>
        /// <returns>图形的ObjectId列表,如果创建失败，返回空的列表</returns>
        public static List<ObjectId> AddEntities(this Database database, List<Entity> entLst, ObjectId spaceId)
        {
            //返回值
            List<ObjectId> entIdLst = new List<ObjectId>();

            //开启事务处理
            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                //打开块表
                BlockTable blockTable = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForRead);

                //打开块表记录
                if (transaction.GetObject(spaceId, OpenMode.ForWrite) is BlockTableRecord btr)
                {
                    try
                    {
                        foreach (var ent in entLst)
                        {
                            //添加图形到块表记录
                            var entId = btr.AppendEntity(ent);
                            entIdLst.Add(entId);
                            //更新数据信息
                            transaction.AddNewlyCreatedDBObject(ent, true);
                        }
                        transaction.Commit();
                    }

                    catch (Exception ex)
                    {
                        transaction.Abort();
                        entIdLst.Clear();
                    }
                }
            }
            return entIdLst;
        }


        /// <summary>
        /// 在模型空间或图纸空间将图形对象添加到图形文件中
        /// </summary>
        /// <param name="database">图形数据库</param>
        /// <param name="entLst">图形对象列表</param>s
        /// <param name="spaceName">空间名称，默认模型空间</param>
        /// <returns>图形的ObjectId列表,如果创建失败，返回空的列表</returns>
        public static List<ObjectId> AddEntities(this Database database, List<Entity> entLst, string spaceName = "MODELSPACE")
        {

            //返回值
            List<ObjectId> entIdLst = new List<ObjectId>();


            //开启事务处理
            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                //打开块表
                BlockTable blockTable = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForRead);


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


                ObjectId spaceId = blockTable[spaceName];

                entIdLst= database.AddEntities(entLst, spaceId);

                transaction.Commit();
            }
            return entIdLst;
        }







        /// <summary>
        /// 将图形对象添加到图形文件中
        /// </summary>
        /// <param name="database">图形数据库</param>
        /// <param name="ents">图形对象列表</param>
        /// <param name="spaceName">空间名称，默认模型空间</param>
        /// <returns>图形的ObjectId数组</returns>
        public static ObjectId[] AddEntity(this Database database, List<Entity> ents, string spaceName = "MODELSPACE")
        {

            Entity[] entities = ents.ToArray();
            return database.AddEntity(spaceName, entities);

        }





        /// <summary>
        /// 将图形对象添加到图形文件中
        /// </summary>
        /// <param name="database">图形数据库</param>
        /// <param name="ent">图形对象，可变参数</param>
        /// <param name="spaceId">空间对象的ObjectId</param>
        /// <returns>图形的ObjectId,数组返回</returns>
        public static ObjectId[] AddEntity(this Database database, ObjectId spaceId, params Entity[] ent)
        {

            //声明ObjectId，用于返回
            ObjectId[] entId = new ObjectId[ent.Length];
            //开启事务处理
            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                //打开块表
                BlockTable blockTable = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForRead);

                //打开块表记录
                if (transaction.GetObject(spaceId, OpenMode.ForWrite) is BlockTableRecord btr)
                {
                    try
                    {
                        for (int i = 0; i < ent.Length; i++)
                        {
                            //添加图形到块表记录
                            entId[i] = btr.AppendEntity(ent[i]);
                            transaction.AddNewlyCreatedDBObject(ent[i], true);
                        }

                        transaction.Commit();

                    }

                    catch (Exception ex)
                    {
                        transaction.Abort();
                    }
                }
            }

            return entId;
        }



        /// <summary>
        /// 将图形对象添加到图形文件中
        /// </summary>
        /// <param name="database">图形数据库</param>
        /// <param name="ent">图形对象，可变参数</param>
        /// <param name="spaceName">空间名称，默认模型空间</param>
        /// <returns>图形的ObjectId,数组返回</returns>
        public static ObjectId[] AddEntity(this Database database, string spaceName, params Entity[] ent)
        {


            //声明ObjectId，用于返回
            ObjectId[] entId = new ObjectId[ent.Length];
            //开启事务处理
            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                //打开块表
                BlockTable blockTable = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForRead);

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


                ObjectId spaceId = blockTable[spaceName];


                entId= database.AddEntity(spaceId, ent);
                transaction.Commit();
            }
            return entId;
        }





        /// <summary>
        /// 绘制直线，自动将z值设置为0
        /// </summary>
        /// <param name="database">图形数据库</param>
        /// <param name="startPoint">起点二维坐标</param>
        /// <param name="endPoint">终点二维坐标</param>
        /// <param name="spaceName">模型空间还是图纸空间名称，不分大小写，默认模型空间</param>
        /// <returns>图形的ObjectId,如果创建失败，返回ObjectId.Null</returns>
        public static ObjectId AddLine(this Database database, Point2d startPoint, Point2d endPoint, string spaceName = "MODELSPACE")
        {
            Point3d startPoint3d = new Point3d(startPoint.X, startPoint.Y, 0);
            Point3d endPoint3d = new Point3d(endPoint.X, endPoint.Y, 0);
            return database.AddLine(startPoint3d, endPoint3d, spaceName);
        }


        /// <summary>
        /// 绘制直线，自动将z值设置为0
        /// </summary>
        /// <param name="database">图形数据库</param>
        /// <param name="spaceId">空间对象的OjectId</param>
        /// <param name="startPoint">起点二维坐标</param>
        /// <param name="endPoint">终点二维坐标</param>
        /// <param name="spaceName">模型空间还是图纸空间名称，不分大小写，默认模型空间</param>
        /// <returns>图形的ObjectId,如果创建失败，返回ObjectId.Null</returns>
        public static ObjectId AddLine(this Database database, ObjectId spaceId, Point2d startPoint, Point2d endPoint, string spaceName = "MODELSPACE")
        {
            Point3d startPoint3d = new Point3d(startPoint.X, startPoint.Y, 0);
            Point3d endPoint3d = new Point3d(endPoint.X, endPoint.Y, 0);
            return database.AddLine(spaceId, startPoint3d, endPoint3d);
        }







        /// <summary>
        /// 绘制直线
        /// </summary>
        /// <param name="database">图形数据库</param>
        /// <param name="startPoint">起点坐标</param>
        /// <param name="endPoint">终点坐标</param>
        /// <param name="spaceName">模型空间还是图纸空间名称，不分大小写，默认模型空间</param>
        /// <returns>图形的ObjectId,如果创建失败，返回ObjectId.Null</returns>
        public static ObjectId AddLine(this Database database, Point3d startPoint, Point3d endPoint, string spaceName = "MODELSPACE")
        {
            return database.AddEntity(new Line(startPoint, endPoint), spaceName);
        }

        /// <summary>
        /// 绘制直线
        /// </summary>
        /// <param name="database">图形数据库</param>
        /// <param name="spaceId">空间对象的OjectId</param>
        /// <param name="startPoint">起点坐标</param>
        /// <param name="endPoint">终点坐标</param>
        /// <returns>图形的ObjectId,如果创建失败，返回ObjectId.Null</returns>
        public static ObjectId AddLine(this Database database, ObjectId spaceId, Point3d startPoint, Point3d endPoint)
        {
            return database.AddEntity(spaceId, new Line(startPoint, endPoint));
        }





        /// <summary>
        /// 绘制直线
        /// </summary>
        /// <param name="database">图形数据库</param>
        /// <param name="spaceId">空间对象的OjectId</param>
        /// <param name="startPoint">起点坐标</param>
        /// <param name="length">直线长度</param>
        /// <param name="degree">与X轴正方向的夹角</param>
        /// <returns>图形的ObjectId,如果创建失败，返回ObjectId.Null</returns>
        public static ObjectId AddLine(this Database database, ObjectId spaceId, Point3d startPoint, double length, double degree)
        {
            //计算终点坐标
            double X = startPoint.X + length * Math.Cos(degree.DegreeToAngle());
            double Y = startPoint.Y + length * Math.Sin(degree.DegreeToAngle());
            Point3d endPoint = new Point3d(X, Y, 0);
            return database.AddEntity(spaceId, new Line(startPoint, endPoint));
        }





        /// <summary>
        /// 绘制直线
        /// </summary>
        /// <param name="database">图形数据库</param>
        /// <param name="startPoint">起点坐标</param>
        /// <param name="length">直线长度</param>
        /// <param name="degree">与X轴正方向的夹角</param>
        /// <param name="spaceName">模型空间还是图纸空间名称，不分大小写，默认模型空间</param>
        /// <returns>图形的ObjectId,如果创建失败，返回ObjectId.Null</returns>
        public static ObjectId AddLine(this Database database, Point3d startPoint, double length, double degree, string spaceName = "MODELSPACE")
        {
            //计算终点坐标
            double X = startPoint.X + length * Math.Cos(degree.DegreeToAngle());
            double Y = startPoint.Y + length * Math.Sin(degree.DegreeToAngle());
            Point3d endPoint = new Point3d(X, Y, 0);
            return database.AddEntity(new Line(startPoint, endPoint), spaceName);
        }



        /// <summary>
        /// 通过圆弧上的三个点画圆弧
        /// </summary>
        /// <param name="database">图形数据库</param>
        /// <param name="startPoint">起始点</param>
        /// <param name="pointOnArc">椭圆上的一点</param>
        /// <param name="endPoint">终止点</param>
        /// <param name="spaceName">模型空间还是图纸空间名称，不分大小写，默认模型空间</param>
        /// <returns>图形的ObjectId对象,如果创建失败，返回ObjectId.Null</returns>
        public static ObjectId AddArc(this Database database, Point3d startPoint, Point3d pointOnArc, Point3d endPoint, string spaceName = "MODELSPACE")
        {
            //先判断三个点是否在同一条直线上
            if (startPoint.IsOnOneLine(pointOnArc, endPoint))
            {
                return ObjectId.Null;
            }

            //创建几何类对象
            CircularArc3d cArc = new CircularArc3d(startPoint, pointOnArc, endPoint);

            //通过几何类对象获取其属性
            Point3d center = cArc.Center;//圆心
            double radius = cArc.Radius;//半径

            //圆弧的起始角度
            double startAngle = center.GetAngleToXAxis(startPoint);
            //圆弧的终止角度
            double endAngle = center.GetAngleToXAxis(endPoint);

            //创建圆弧对象
            Arc arc = new Arc(center, radius, startAngle, endAngle);

            //加入图形数据库
            return database.AddEntity(arc, spaceName);
        }






        /// <summary>
        /// 通过圆弧上的三个点画圆弧
        /// </summary>
        /// <param name="database">图形数据库</param>
        /// <param name="spaceId">空间对象的OjectId</param>
        /// <param name="startPoint">起始点</param>
        /// <param name="pointOnArc">椭圆上的一点</param>
        /// <param name="endPoint">终止点</param>
        /// <returns>图形的ObjectId对象,如果创建失败，返回ObjectId.Null</returns>
        public static ObjectId AddArc(this Database database, ObjectId spaceId, Point3d startPoint, Point3d pointOnArc, Point3d endPoint)
        {

            //先判断三个点是否在同一条直线上
            if (startPoint.IsOnOneLine(pointOnArc, endPoint))
            {
                return ObjectId.Null;
            }

            //创建几何类对象
            CircularArc3d cArc = new CircularArc3d(startPoint, pointOnArc, endPoint);

            //通过几何类对象获取其属性
            Point3d center = cArc.Center;//圆心
            double radius = cArc.Radius;//半径

            //圆弧的起始角度
            double startAngle = center.GetAngleToXAxis(startPoint);
            //圆弧的终止角度
            double endAngle = center.GetAngleToXAxis(endPoint);

            //创建圆弧对象
            Arc arc = new Arc(center, radius, startAngle, endAngle);

            //加入图形数据库
            return database.AddEntity(spaceId, arc);
        }








        /// <summary>
        /// 通过圆心、起点、角度绘制圆弧
        /// </summary>
        /// <param name="database">图形数据库</param>
        /// <param name="center">圆心</param>
        /// <param name="startPoint">起点</param>
        /// <param name="degree">角度值</param>
        /// <param name="spaceName">模型空间还是图纸空间名称，不分大小写，默认模型空间</param>
        /// <returns>对象的ObjectId,如果创建失败，返回ObjectId.Null</returns>
        public static ObjectId AddArc(this Database database, Point3d center, Point3d startPoint, double degree, string spaceName = "MODELSPACE")
        {
            //获取半径
            double radius = center.DistanceTo(startPoint);

            //获取起始点角度
            double startAngle = center.GetAngleToXAxis(startPoint);

            //声明圆弧对象
            Arc arc = new Arc(center, radius, startAngle, startAngle + degree.DegreeToAngle());

            return database.AddEntity(arc, spaceName);
        }


        /// <summary>
        /// 通过圆心、起点、角度绘制圆弧
        /// </summary>
        /// <param name="database">图形数据库</param>
        /// <param name="spaceId">空间对象的OjectId</param>
        /// <param name="center">圆心</param>
        /// <param name="startPoint">起点</param>
        /// <param name="degree">角度值</param>
        /// <returns>对象的ObjectId,如果创建失败，返回ObjectId.Null</returns>
        public static ObjectId AddArc(this Database database, ObjectId spaceId, Point3d center, Point3d startPoint, double degree)
        {
            //获取半径
            double radius = center.DistanceTo(startPoint);

            //获取起始点角度
            double startAngle = center.GetAngleToXAxis(startPoint);

            //声明圆弧对象
            Arc arc = new Arc(center, radius, startAngle, startAngle + degree.DegreeToAngle());

            return database.AddEntity(spaceId, arc);
        }








        /// <summary>
        /// 通过圆心、半径绘制圆
        /// </summary>
        /// <param name="database">图形数据库</param>
        /// <param name="spaceName">模型空间还是图纸空间名称，不分大小写，默认模型空间</param>
        /// <param name="center">中心</param>
        /// <param name="radius">半径</param>
        /// <returns>图形ObjectId,如果创建失败，返回ObjectId.Null</returns>
        public static ObjectId AddCircle(this Database database, Point3d center, double radius, string spaceName = "MODELSPACE")
        {
            return database.AddEntity(new Circle(center, Vector3d.ZAxis, radius), spaceName);
        }


        /// <summary>
        /// 通过圆心、半径绘制圆
        /// </summary>
        /// <param name="database">图形数据库</param>
        /// <param name="spaceId">空间对象的OjectId</param>
        /// <param name="center">中心</param>
        /// <param name="radius">半径</param>
        /// <returns>图形ObjectId,如果创建失败，返回ObjectId.Null</returns>
        public static ObjectId AddCircle(this Database database, ObjectId spaceId, Point3d center, double radius)
        {
            return database.AddEntity(spaceId, new Circle(center, Vector3d.ZAxis, radius));
        }






        /// <summary>
        /// 通过两点绘制圆
        /// </summary>
        /// <param name="database">图形数据库</param>
        /// <param name="point1">第一个点</param>
        /// <param name="point2">第二个点</param>
        /// <param name="spaceName">模型空间还是图纸空间名称，不分大小写，默认模型空间</param>
        /// <returns>图形ObjectId,如果创建失败，返回ObjectId.Null</returns>
        public static ObjectId AddCircle(this Database database, Point3d point1, Point3d point2, string spaceName = "MODELSPACE")
        {
            //获取中心点
            Point3d center = point1.GetCenterPointBetweenTwoPoints(point2);
            //获取半径
            double radius = center.DistanceTo(point1);
            return database.AddCircle(center, radius, spaceName);
        }




        /// <summary>
        /// 通过两点绘制圆
        /// </summary>
        /// <param name="database">图形数据库</param>
        /// <param name="spaceId">空间对象的OjectId</param>
        /// <param name="point1">第一个点</param>
        /// <param name="point2">第二个点</param>
        /// <returns>图形ObjectId,如果创建失败，返回ObjectId.Null</returns>
        public static ObjectId AddCircle(this Database database, ObjectId spaceId, Point3d point1, Point3d point2)
        {
            //获取中心点
            Point3d center = point1.GetCenterPointBetweenTwoPoints(point2);
            //获取半径
            double radius = center.DistanceTo(point1);
            return database.AddCircle(spaceId, center, radius);
        }



        /// <summary>
        /// 通过三点绘制圆
        /// </summary>
        /// <param name="database">图形数据库</param>
        /// <param name="point1">第一个点</param>
        /// <param name="point2">第二个点</param>
        /// <param name="point3">第三个点</param>
        /// <param name="spaceName">模型空间还是图纸空间名称，不分大小写，默认模型空间</param>
        /// <returns>图形的ObjectId,如果创建失败，返回ObjectId.Null</returns>
        public static ObjectId AddCircle(this Database database, Point3d point1, Point3d point2, Point3d point3, string spaceName = "MODELSPACE")
        {
            //先判断三个点是否在同一条直线上
            if (point1.IsOnOneLine(point2, point3))
            {
                return ObjectId.Null;

            }

            //声明几何类的CircularArc3d对象
            CircularArc3d cArc = new CircularArc3d(point1, point2, point3);
            return database.AddCircle(cArc.Center, cArc.Radius, spaceName);
        }


        /// <summary>
        /// 通过三点绘制圆
        /// </summary>
        /// <param name="database">图形数据库</param>
        /// <param name="spaceId">空间对象的OjectId</param>
        /// <param name="point1">第一个点</param>
        /// <param name="point2">第二个点</param>
        /// <param name="point3">第三个点</param>
        /// <returns>图形的ObjectId,如果创建失败，返回ObjectId.Null</returns>
        public static ObjectId AddCircle(this Database database, ObjectId spaceId, Point3d point1, Point3d point2, Point3d point3)
        {
            //先判断三个点是否在同一条直线上
            if (point1.IsOnOneLine(point2, point3))
            {
                return ObjectId.Null;

            }

            //声明几何类的CircularArc3d对象
            CircularArc3d cArc = new CircularArc3d(point1, point2, point3);
            return database.AddCircle(spaceId, cArc.Center, cArc.Radius);
        }








        /// <summary>
        /// 绘制折线多段线
        /// </summary>
        /// <param name="database">图形数据库</param>
        /// <param name="isClosed">是否闭合</param>
        /// <param name="constantWidth">线宽</param>
        /// <param name="spaceName">模型空间还是图纸空间名称，不分大小写，默认模型空间</param>
        /// <param name="vertics">多段线的顶点，可变参数</param>
        /// <returns>图形的ObjectId,如果创建失败，返回ObjectId.Null</returns>
        public static ObjectId AddPolyLine(this Database database, bool isClosed = true, double constantWidth = 0, string spaceName = "MODELSPACE", params Point2d[] vertics)
        {
            if (vertics.Length < 2)
            {
                return ObjectId.Null;
            }

            //声明一个多段线对象
            Polyline pLine = new Polyline();
            for (int i = 0; i < vertics.Length; i++)
            {
                pLine.AddVertexAt(i, vertics[i], 0, 0, 0);
            }

            //判断是否闭合
            if (isClosed)
            {
                pLine.Closed = isClosed;
            }

            //设置多段线的线宽
            pLine.ConstantWidth = constantWidth;

            return database.AddEntity(pLine, spaceName);

        }


        /// <summary>
        /// 绘制折线多段线
        /// </summary>
        /// <param name="database">图形数据库</param>
        /// <param name="spaceId">空间对象的OjectId</param>
        /// <param name="isClosed">是否闭合</param>
        /// <param name="constantWidth">线宽</param>
        /// <param name="vertics">多段线的顶点，可变参数</param>
        /// <returns>图形的ObjectId,如果创建失败，返回ObjectId.Null</returns>
        public static ObjectId AddPolyLine(this Database database, ObjectId spaceId, bool isClosed = true, double constantWidth = 0, params Point2d[] vertics)
        {
            if (vertics.Length < 2)
            {
                return ObjectId.Null;
            }

            //声明一个多段线对象
            Polyline pLine = new Polyline();
            for (int i = 0; i < vertics.Length; i++)
            {
                pLine.AddVertexAt(i, vertics[i], 0, 0, 0);
            }

            //判断是否闭合
            if (isClosed)
            {
                pLine.Closed = isClosed;
            }

            //设置多段线的线宽
            pLine.ConstantWidth = constantWidth;

            return database.AddEntity(spaceId, pLine);

        }





        /// <summary>
        /// 绘制折线多段线
        /// </summary>
        /// <param name="database">图形数据库</param>
        /// <param name="isClosed">是否闭合</param>
        /// <param name="constantWidth">线宽</param>
        /// <param name="vertics">多段线的顶点，可变参数</param>
        /// <param name="spaceName">空间名称，默认模型空间</param>
        /// <returns>图形的ObjectId,如果创建失败，返回ObjectId.Null</returns>
        public static ObjectId AddPolyLine(this Database database, List<Point2d> vertics, bool isClosed = true, double constantWidth = 0, string spaceName = "MODELSPACE")
        {
            if (vertics.Count < 2)
            {
                return ObjectId.Null;
            }

            //声明一个多段线对象
            Polyline pLine = new Polyline();
            for (int i = 0; i < vertics.Count; i++)
            {
                pLine.AddVertexAt(i, vertics[i], 0, 0, 0);
            }

            //判断是否闭合
            if (isClosed)
            {
                pLine.Closed = isClosed;
            }
            else
            {
                pLine.Closed = false;

            }

            //设置多段线的线宽
            pLine.ConstantWidth = constantWidth;

            return database.AddEntity(pLine, spaceName);

        }











        /// <summary>
        /// 绘制折线多段线
        /// </summary>
        /// <param name="database">图形数据库</param>
        /// <param name="spaceId">空间对象的OjectId</param>
        /// <param name="isClosed">是否闭合</param>
        /// <param name="constantWidth">线宽</param>
        /// <param name="vertics">多段线的顶点，可变参数</param>
        /// <returns>图形的ObjectId,如果创建失败，返回ObjectId.Null</returns>
        public static ObjectId AddPolyLine(this Database database, ObjectId spaceId, List<Point2d> vertics, bool isClosed = true, double constantWidth = 0)
        {
            if (vertics.Count < 2)
            {
                return ObjectId.Null;
            }

            //声明一个多段线对象
            Polyline pLine = new Polyline();
            for (int i = 0; i < vertics.Count; i++)
            {
                pLine.AddVertexAt(i, vertics[i], 0, 0, 0);
            }

            //判断是否闭合
            if (isClosed)
            {
                pLine.Closed = isClosed;
            }
            else
            {
                pLine.Closed = false;

            }

            //设置多段线的线宽
            pLine.ConstantWidth = constantWidth;

            return database.AddEntity(spaceId, pLine);

        }











        /// <summary>
        /// 绘制折线多段线
        /// </summary>
        /// <param name="database">图形数据库</param>
        /// <param name="spaceId">空间对象的OjectId</param>
        /// <param name="isClosed">是否闭合</param>
        /// <param name="constantWidth">线宽</param>
        /// <param name="vertics">多段线的顶点，可变参数</param>
        /// <returns>图形的ObjectId,如果创建失败，返回ObjectId.Null</returns>
        public static ObjectId AddPolyLine(this Database database, ObjectId spaceId, List<Point3d> vertics, bool isClosed = true, double constantWidth = 0)
        {
            if (vertics.Count < 2)
            {
                return ObjectId.Null;
            }

            //先转换为二维多段线
            List<Point2d> vertics2D = new List<Point2d>();
            foreach (var item in vertics)
            {
                vertics2D.Add(new Point2d(item.X, item.Y));
            }

            return database.AddPolyLine(spaceId, vertics2D, isClosed, constantWidth);

        }














        /// <summary>
        /// 创建多段线
        /// </summary>
        /// <param name="database">数据库对象</param>
        /// <param name="polygon">三维点集合</param>
        /// <param name="isClosed">多段线是否闭合，默认不闭合</param>
        /// <param name="constantWidth">多段线的宽度，默认不带宽度</param>
        /// <param name="spaceName">空间名称，默认模型空间</param>
        /// <returns>创建的多段线的ObjectId，如果创建不成功，返回ObjectId.Null</returns>
        public static ObjectId AddPolyLine(this Database database, Point3dCollection polygon, bool isClosed = false, double constantWidth = 0, string spaceName = "MODELSPACE")
        {

            if (polygon.Count < 2)
            {
                return ObjectId.Null;
            }

            //声明一个多段线对象
            Polyline pLine = new Polyline(polygon.Count);
            for (int i = 0; i < polygon.Count; i++)
            {
                Point2d point2D = new Point2d(polygon[i].X, polygon[i].Y);
                pLine.AddVertexAt(i, point2D, 0, 0, 0);
            }

            //判断是否闭合
            if (isClosed)
            {
                pLine.Closed = isClosed;
            }

            //设置多段线的线宽
            pLine.ConstantWidth = constantWidth;


            return database.AddEntity(pLine, spaceName);

        }



        /// <summary>
        /// 创建多段线
        /// </summary>
        /// <param name="database">数据库对象</param>
        /// <param name="spaceId">空间对象的OjectId</param>
        /// <param name="polygon">三维点集合</param>
        /// <param name="isClosed">多段线是否闭合，默认不闭合</param>
        /// <param name="constantWidth">多段线的宽度，默认不带宽度</param>
        /// <returns>创建的多段线的ObjectId，如果创建不成功，返回ObjectId.Null</returns>
        public static ObjectId AddPolyLine(this Database database, ObjectId spaceId, Point3dCollection polygon, bool isClosed = false, double constantWidth = 0)
        {

            if (polygon.Count < 2)
            {
                return ObjectId.Null;
            }

            //声明一个多段线对象
            Polyline pLine = new Polyline(polygon.Count);
            for (int i = 0; i < polygon.Count; i++)
            {
                Point2d point2D = new Point2d(polygon[i].X, polygon[i].Y);
                pLine.AddVertexAt(i, point2D, 0, 0, 0);
            }

            //判断是否闭合
            if (isClosed)
            {
                pLine.Closed = isClosed;
            }

            //设置多段线的线宽
            pLine.ConstantWidth = constantWidth;


            return database.AddEntity(spaceId, pLine);

        }



        /// <summary>
        /// 绘制矩形
        /// </summary>
        /// <param name="database">图形数据库</param>
        /// <param name="spaceId">空间对象的OjectId</param>
        /// <param name="point1">第一个角点</param>
        /// <param name="point2">第二个角点</param>
        /// <returns>图形ObjectId,如果创建失败，返回ObjectId.Null</returns>
        public static ObjectId AddRectangle(this Database database, ObjectId spaceId, Point2d point1, Point2d point2)
        {

            //声明多段线
            Polyline polyline = new Polyline();
            //计算矩形的四个顶点坐标
            double minX = Math.Min(point1.X, point2.X);
            double maxX = Math.Max(point1.X, point2.X);
            double minY = Math.Min(point1.Y, point2.Y);
            double maxY = Math.Max(point1.Y, point2.Y);
            Point2d p1 = new Point2d(minX, minY);
            Point2d p2 = new Point2d(maxX, minY);
            Point2d p3 = new Point2d(maxX, maxY);
            Point2d p4 = new Point2d(minX, maxY);
            //添加多段线的顶点
            polyline.AddVertexAt(0, p1, 0, 0, 0);
            polyline.AddVertexAt(1, p2, 0, 0, 0);
            polyline.AddVertexAt(2, p3, 0, 0, 0);
            polyline.AddVertexAt(3, p4, 0, 0, 0);
            polyline.Closed = true;
            return database.AddEntity(spaceId, polyline);

        }



        /// <summary>
        /// 绘制矩形
        /// </summary>
        /// <param name="database">图形数据库</param>
        /// <param name="point1">第一个角点</param>
        /// <param name="point2">第二个角点</param>
        /// <param name="spaceName">模型空间还是图纸空间名称，不分大小写，默认模型空间</param>
        /// <returns>图形ObjectId,如果创建失败，返回ObjectId.Null</returns>
        public static ObjectId AddRectangle(this Database database, Point2d point1, Point2d point2, string spaceName = "MODELSPACE")
        {

            //声明多段线
            Polyline polyline = new Polyline();
            //计算矩形的四个顶点坐标
            double minX = Math.Min(point1.X, point2.X);
            double maxX = Math.Max(point1.X, point2.X);
            double minY = Math.Min(point1.Y, point2.Y);
            double maxY = Math.Max(point1.Y, point2.Y);
            Point2d p1 = new Point2d(minX, minY);
            Point2d p2 = new Point2d(maxX, minY);
            Point2d p3 = new Point2d(maxX, maxY);
            Point2d p4 = new Point2d(minX, maxY);
            //添加多段线的顶点
            polyline.AddVertexAt(0, p1, 0, 0, 0);
            polyline.AddVertexAt(1, p2, 0, 0, 0);
            polyline.AddVertexAt(2, p3, 0, 0, 0);
            polyline.AddVertexAt(3, p4, 0, 0, 0);
            polyline.Closed = true;
            return database.AddEntity(polyline, spaceName);

        }



        /// <summary>
        /// 绘制矩形
        /// </summary>
        /// <param name="database">图形数据库</param>
        /// <param name="width">矩形宽度</param>
        /// <param name="height">矩形高度</param>
        /// <param name="centerPoint">矩形中心点</param>
        /// <param name="spaceName">模型空间还是图纸空间名称，不分大小写，默认模型空间</param>
        /// <returns>图形ObjectId,如果创建失败，返回ObjectId.Null</returns>
        public static ObjectId AddRectangle(this Database database, double width, double height, Point2d centerPoint, string spaceName = "MODELSPACE")
        {
            Point2d point1 = new Point2d(centerPoint.X - width / 2, centerPoint.Y - height / 2);
            Point2d point2 = new Point2d(centerPoint.X + width / 2, centerPoint.Y + height / 2);
            return database.AddRectangle(point1, point2, spaceName);
        }


        /// <summary>
        /// 绘制矩形
        /// </summary>
        /// <param name="database">图形数据库</param>
        /// <param name="spaceId">空间对象的OjectId</param>
        /// <param name="width">矩形宽度</param>
        /// <param name="height">矩形高度</param>
        /// <param name="centerPoint">矩形中心点</param>
        /// <returns>图形ObjectId,如果创建失败，返回ObjectId.Null</returns>
        public static ObjectId AddRectangle(this Database database, ObjectId spaceId, double width, double height, Point2d centerPoint)
        {
            Point2d point1 = new Point2d(centerPoint.X - width / 2, centerPoint.Y - height / 2);
            Point2d point2 = new Point2d(centerPoint.X + width / 2, centerPoint.Y + height / 2);
            return database.AddRectangle(spaceId, point1, point2);
        }




        /// <summary>
        /// 绘制内接正多边形
        /// </summary>
        /// <param name="database">图形数据库</param>
        /// <param name="center">多边形所在圆的圆心</param>
        /// <param name="radius">所在圆的半径</param>
        /// <param name="sideNum">边数</param>
        /// <param name="startDegree">起始角度，角度值</param>
        /// <param name="spaceName">模型空间还是图纸空间名称，不分大小写，默认模型空间</param>
        /// <returns>图形ObjectId,如果创建失败，返回ObjectId.Null</returns>
        public static ObjectId AddPolygon(this Database database, Point2d center, double radius, int sideNum = 3, double startDegree = 0, string spaceName = "MODELSPACE")
        {
            //声明多段线对象
            Polyline polyline = new Polyline();

            //判断边数是否符合要求
            if (sideNum < 3)
            {
                return ObjectId.Null;

            }
            //计算每个顶点的坐标
            Point2d[] points = new Point2d[sideNum];
            double angle = startDegree.DegreeToAngle();
            for (int i = 0; i < sideNum; i++)
            {
                points[i] = new Point2d(center.X + radius * Math.Cos(angle), center.Y + radius * Math.Sin(angle));
                polyline.AddVertexAt(i, points[i], 0, 0, 0);
                angle += 2 * Math.PI / sideNum;

            }

            //闭合多段线
            polyline.Closed = true;
            return database.AddEntity(polyline, spaceName);
        }



        /// <summary>
        /// 绘制内接正多边形
        /// </summary>
        /// <param name="database">图形数据库</param>
        /// <param name="spaceId">空间对象的ObjectId</param>
        /// <param name="center">多边形所在圆的圆心</param>
        /// <param name="radius">所在圆的半径</param>
        /// <param name="sideNum">边数</param>
        /// <param name="startDegree">起始角度，角度值</param>
        /// <returns>图形ObjectId,如果失败，返回ObjectId.Null</returns>
        public static ObjectId AddPolygon(this Database database, ObjectId spaceId, Point2d center, double radius, int sideNum = 3, double startDegree = 0)
        {
            //声明多段线对象
            Polyline polyline = new Polyline();

            //判断边数是否符合要求
            if (sideNum < 3)
            {
                return ObjectId.Null;

            }
            //计算每个顶点的坐标
            Point2d[] points = new Point2d[sideNum];
            double angle = startDegree.DegreeToAngle();
            for (int i = 0; i < sideNum; i++)
            {
                points[i] = new Point2d(center.X + radius * Math.Cos(angle), center.Y + radius * Math.Sin(angle));
                polyline.AddVertexAt(i, points[i], 0, 0, 0);
                angle += 2 * Math.PI / sideNum;

            }

            //闭合多段线
            polyline.Closed = true;
            return database.AddEntity(spaceId, polyline);
        }



    }


}
