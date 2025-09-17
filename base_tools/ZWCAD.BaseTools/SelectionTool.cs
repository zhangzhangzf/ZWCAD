using ZWCAD.BaseTools.Extension;
using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;
using ZwSoft.ZwCAD.Geometry;
using System.Collections.Generic;
using System.Windows.Forms;


namespace ZWCAD.BaseTools
{
    /// <summary>
    /// 选择工具
    /// </summary>
    public class SelectionTool
    {
        Document m_document;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="document">文档对象</param>
        public SelectionTool(Document document)
        {
            m_document = document;
        }






        ///// <summary>
        ///// 提示用户选择单个实体
        ///// </summary>
        ///// <param name="word">选择提示</param>
        ///// <param name="openMode">打开模式</param>
        ///// <returns>实体对象,如果选择不成功，返回null</returns>
        //public Entity SelectDBObject(string word = "\n选择对象:", OpenMode openMode = OpenMode.ForRead)
        //{


        //    Editor editor = m_document.Editor;

        //    //返回值
        //    Entity entity = null;
        //    PromptEntityResult result = editor.GetEntity(word);

        //    if (result.Status == PromptStatus.OK)
        //    {

        //        using (Transaction transaction = m_document.TransactionManager.StartTransaction())
        //        {
        //            entity = transaction.GetObject(result.ObjectId, openMode) as Entity;
        //            //transaction.Commit();
        //        }

        //    }

        //    return entity;

        //}





        /// <summary>
        /// 提示用户选择单个实体
        /// </summary>
        /// <param name="word">选择提示</param>
        /// <param name="openMode">打开模式</param>
        /// <param name="dxfNameLst">允许选择的实体对象的类型列表，如直线为"line"，不区分大小写，如果为null或空，将允许所有对象类型，默认允许所有对象类型</param>
        /// <returns>实体对象,如果选择不成功，返回null</returns>
        public Entity SelectDBObject(string word = "\n选择对象:", OpenMode openMode = OpenMode.ForRead, List<string> dxfNameLst = null)
        {


            Editor editor = m_document.Editor;

            //返回值
            Entity entity = null;
            PromptEntityResult result;

            if (dxfNameLst != null && dxfNameLst.Count > 0)
            {




                PromptEntityOptions options = new PromptEntityOptions(word);

                options.SetRejectMessage("\n选择的对象类型有误，请重新选择");
                foreach (var item in dxfNameLst)
                {

                    string itemLowercase = item.ToLower();
                    switch (itemLowercase)
                    {
                        case "line":
                            options.AddAllowedClass(typeof(Line), true);
                            break;

                        case "polyline":
                            options.AddAllowedClass(typeof(Polyline), true);
                            break;



                        case "insert":
                            options.AddAllowedClass(typeof(BlockReference), true);
                            break;


                        case "circle":
                            options.AddAllowedClass(typeof(Circle), true);
                            break;


                        case "arc":
                            options.AddAllowedClass(typeof(Arc), true);
                            break;


                        case "hatch":
                            options.AddAllowedClass(typeof(Hatch), true);
                            break;

                        case "text":
                            options.AddAllowedClass(typeof(DBText), true);
                            break;


                        case "mtext":
                            options.AddAllowedClass(typeof(MText), true);
                            break;

                        case "rasterimage":
                            options.AddAllowedClass(typeof(RasterImage), true);
                            break;



                        default:
                            MessageBox.Show("请添加 " + item + " 的对象类型");
                            break;

                    }

                }

                result = editor.GetEntity(options);
            }

            else
            {
                //这个方法不能选视口对象
                result = editor.GetEntity(word);

            }



            if (result.Status == PromptStatus.OK)
            {

                using (Transaction transaction = m_document.TransactionManager.StartTransaction())
                {
                    entity = transaction.GetObject(result.ObjectId, openMode) as Entity;
                }

            }

            return entity;

        }







        /// <summary>
        /// 返回用户选择点的二维点坐标
        /// </summary>
        /// <param name="word">选择提示</param>
        /// <returns>点的二维坐标,如果选择不成功，返回new Point2d(double.NaN, double.NaN)</returns>
        public Point2d SelectPoint2d(string word = "\n选择点:")
        {

            Point2d point2D = new Point2d(double.NaN, double.NaN);


            Editor editor = m_document.Editor;


            PromptPointResult result = editor.GetPoint(word);

            if (result.Status == PromptStatus.OK)
            {

                Point3d point3D = result.Value;
                point2D = new Point2d(point3D.X, point3D.Y);

            }

            return point2D;

        }






        /// <summary>
        /// 获取所有的对象
        /// </summary>
        /// <param name="selectionFilter">过滤器</param>
        /// <param name="isInModelSpace">是否是在模型空间中（包括在模型空间的模型空间中，或者图纸空间中激活的模型空间中)，默认在模型空间中</param>
        /// <returns>选择集,如果没有选到，返回null</returns>
        public SelectionSet SelectAllInSpace(SelectionFilter selectionFilter = null, bool isInModelSpace = true)
        {
            //返回值
            SelectionSet resultValue = null;

            Editor editor = m_document.Editor;



            //需要先判断是在模型空间中还是图纸空间中，需要切换到需要的空间中才能获取所有的对象，如果已经切换，选择完对象之后要切换回来

            int spaceIndex = m_document.InModelSpaceOrPaperSpace();


            bool isSwitch = false;

            if (isInModelSpace) //到模型空间中选择,可以在模型空间的模型空间 或者图纸空间中激活的模型空间中
            {
                if (spaceIndex == 1) //当前在图纸空间中，没有激活模型空间
                {
                    //15:06 2023/6/5  如果是在图纸空间中，要切换到模型空间中，可能会报错 ：eCannotChangeActiveViewport
                    editor.SwitchToModelSpace(); //在图纸空间中激活模型空间
                    isSwitch = true;
                }
            }
            else //到图纸空间中选择
            {
                if (spaceIndex != 1) //不在图纸空间中
                {
                    editor.SwitchToPaperSpace(); //激活图纸空间
                    isSwitch = true;
                }
            }


            try
            {

                PromptSelectionResult result;

                if (selectionFilter == null)
                {
                    result = editor.SelectAll();
                }
                else
                {
                    result = editor.SelectAll(selectionFilter);

                }

                if (result.Status == PromptStatus.OK)
                {
                    resultValue = result.Value;

                }
            }
            catch  //如果选择过程发生错误，什么也不做，保留值null
            {
            }

            //结束之前先返回原来的空间
            if (isSwitch) //需要切换
            {
                m_document.SwitchToSpace(spaceIndex);//如果为图纸空间中激活的模型空间，会自动切换到当前激活视口
            }
            return resultValue;

        }













        /// <summary>
        /// 获取指定名称的所有动态快的ObjectId数组
        /// </summary>
        /// <param name="blkName">块的名称</param>
        /// <param name="isInModelSpace">是否是在模型空间中（包括在模型空间的模型空间中，或者图纸空间中激活的模型空间中)，默认在模型空间中</param>
        /// <returns>所有对象id列表ObjectId[],如果无，则返回null</returns>
        public ObjectId[] SelectAllDynamicBlocks(string blkName, bool isInModelSpace = true)
        {
            //返回值
            ObjectId[] dynamicBlockIdLst = null;

            List<string> blkNames = new List<string>
            {
                blkName
            };


            //开启事务处理
            using (Transaction transaction = m_document.TransactionManager.StartTransaction())
            {
                //打开块表
                BlockTable blockTable = (BlockTable)transaction.GetObject(m_document.Database.BlockTableId, OpenMode.ForRead);

                if (!blockTable.Has(blkName))
                {
                    return dynamicBlockIdLst;
                }


                // Get the anonymous block names
                var btr = (BlockTableRecord)transaction.GetObject(blockTable[blkName], OpenMode.ForRead);

                if (!btr.IsDynamicBlock)
                {
                    return dynamicBlockIdLst;
                }


                // Get the anonymous blocks and add them to our list
                var anonBlks = btr.GetAnonymousBlockIds();

                foreach (ObjectId bid in anonBlks)
                {
                    var btr2 = (BlockTableRecord)transaction.GetObject(bid, OpenMode.ForRead);
                    blkNames.Add(btr2.Name);
                }
                transaction.Commit();
            }


            // Build a conditional filter list so that only
            // entities with the specified properties are
            // selected

            SelectionFilter selectionFilter = new SelectionFilter(CreateFilterListForBlocks(blkNames));

            return SelectAllInSpaceToObjectIds(selectionFilter, isInModelSpace);
        }


        /// <summary>
        /// 获取过滤器
        /// </summary>
        /// <param name="blkNames"></param>
        /// <returns>如果blkNames为空，返回null</returns>
        private TypedValue[] CreateFilterListForBlocks(List<string> blkNames)
        {
            // If we don't have any block names, return null
            if (blkNames.Count == 0)
                return null;

            // If we only have one, return an array of a single value


            if (blkNames.Count == 1)

                return new TypedValue[] {
          new TypedValue(
            (int)DxfCode.BlockName,blkNames[0]
          )
        };

            // We have more than one block names to search for...
            // Create a list big enough for our block names plus
            // the containing "or" operators

            List<TypedValue> tvl =
              new List<TypedValue>(blkNames.Count + 2)
              {
                  // Add the initial operator
                  new TypedValue(
                (int)DxfCode.Operator,"<or"
              )
              };

            // Add an entry for each block name, prefixing the
            // anonymous block names with a reverse apostrophe

            foreach (var blkName in blkNames)
            {
                tvl.Add(
                  new TypedValue(
                    (int)DxfCode.BlockName,
                    blkName.StartsWith("*") ? "`" + blkName : blkName
                  )
                );
            }

            // Add the final operator

            tvl.Add(
              new TypedValue(
                (int)DxfCode.Operator, "or>"
              )
            );

            // Return an array from the list
            return tvl.ToArray();
        }





        /// <summary>
        /// 选择所有对象，并返回所有对象的id列表
        /// </summary>
        /// <param name="selectionFilter">过滤器</param>
        /// <param name="isInModelSpace">是否是在模型空间中（包括在模型空间的模型空间中，或者图纸空间中激活的模型空间中)，默认在模型空间中</param>
        /// <returns>所有对象id列表ObjectId[],如果无，则返回null</returns>
        public ObjectId[] SelectAllInSpaceToObjectIds(SelectionFilter selectionFilter = null, bool isInModelSpace = true)
        {

            //如果没有过滤器，直接遍历
            if (selectionFilter == null)
            {

                List<ObjectId> objectIdLst = new List<ObjectId>();


                using (Transaction transaction = m_document.TransactionManager.StartTransaction())
                {
                    try
                    {

                        BlockTableRecord btr;

                        if (isInModelSpace) //模型空间
                        {
                            btr =(BlockTableRecord)transaction.GetObject(
                           SymbolUtilityServices.GetBlockModelSpaceId(m_document.Database),
                           OpenMode.ForRead
                         );
                        }
                        else //图纸空间
                        {
                            btr =(BlockTableRecord)transaction.GetObject(
                                               SymbolUtilityServices.GetBlockPaperSpaceId(m_document.Database),
                                               OpenMode.ForRead
                                             );
                        }


                        foreach (var item in btr)
                        {
                            objectIdLst.Add(item);
                        }


                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Abort();
                    }
                }

                return objectIdLst.ToArray();

            }


            SelectionSet selectionSet = SelectAllInSpace(selectionFilter, isInModelSpace);


            if (selectionSet == null)
            {
                return null;
            }
            return selectionSet.GetObjectIds();

        }



        /// <summary>
        /// 选择单个或多个对象，有过滤器的情况，暂时不考虑先选择后执行的情况，因为还不知道怎么过滤
        /// </summary>
        /// <param name="options"></param>
        /// <param name="selectionFilter"></param>
        /// <returns>如果没有选到对象，返回null</returns>
        public SelectionSet GetSelection(PromptSelectionOptions options = null, SelectionFilter selectionFilter = null)
        {
            Editor editor = m_document.Editor;

            PromptSelectionResult result;


            if (options == null)
            {

                if (selectionFilter == null)
                {
                    //先选择，后执行
                    result = editor.SelectImplied();

                    //没有提前选择对象
                    if (result.Status != PromptStatus.OK)
                    {

                        result = editor.GetSelection();
                    }
                }
                else
                {
                    result = editor.GetSelection(selectionFilter);

                }
            }

            else
            {
                if (selectionFilter == null)
                {

                    //先选择，后执行
                    result = editor.SelectImplied();

                    //没有提前选择对象
                    if (result.Status != PromptStatus.OK)
                    {
                        result = editor.GetSelection(options);
                    }

                }
                else
                {
                    result = editor.GetSelection(options, selectionFilter);

                }
            }


            if (result.Status == PromptStatus.OK)
            {

                return result.Value;

            }

            return null;

        }



        /// <summary>
        /// 选择单个或多个对象，考虑了先选择后执行的情况
        /// </summary>
        /// <param name="dxfNameLst">要选择的对象类型，比如直线为"line",不分大小写，如果为null或空，则选择所有的对象类型，默认选择的所有对象类型</param>
        /// <returns>如果没有选到对象，返回空的列表</returns>
        public List<ObjectId> GetSelectionToList(List<string> dxfNameLst = null)
        {
            //返回值
            List<ObjectId> objectIdLst = new List<ObjectId>();

            SelectionSet selectionSet = GetSelection(dxfNameLst);
            if (selectionSet != null)
            {
                objectIdLst = new List<ObjectId>(selectionSet.GetObjectIds());
            }

            return objectIdLst;
        }



        /// <summary>
        /// 选择单个或多个对象，考虑了先选择后执行的情况
        /// </summary>
        /// <param name="dxfNameLst">要选择的对象类型，比如直线为"line",不分大小写，如果为null或空，则选择所有的对象类型，默认选择的所有对象类型</param>
        /// <returns>如果没有选到对象，返回null</returns>
        public SelectionSet GetSelection(List<string> dxfNameLst = null)
        {
            Editor editor = m_document.Editor;

            PromptSelectionResult result;



            //先选择，后执行
            result = editor.SelectImplied();

            //没有提前选择对象
            if (result.Status != PromptStatus.OK)
            {

                if (dxfNameLst != null && dxfNameLst.Count > 0)
                {

                    string dxfName = string.Join(",", dxfNameLst);

                    //string dxfName = dxfNameLst[0];
                    //for (int i = 1; i < dxfNameLst.Count - 1; i++)
                    //{
                    //    dxfName += "," + dxfNameLst[i];
                    //}



                    TypedValue[] values = new TypedValue[] {
                    new TypedValue((int)DxfCode.Start,dxfName) //类型

                    };

                    SelectionFilter selectionFilter = new SelectionFilter(values);

                    result = editor.GetSelection(selectionFilter);

                }
                else
                {

                    result = editor.GetSelection();
                }
            }




            if (result.Status == PromptStatus.OK)
            {


                var value = result.Value;

                if (value != null && value.Count > 0 && dxfNameLst != null && dxfNameLst.Count > 0)
                {

                    List<string> copyDxfNameLst = new List<string>();

                    dxfNameLst.ForEach(x => copyDxfNameLst.Add(x.ToLower()));

                    //考虑后先选择后执行的情况没有过滤，重新过滤一遍
                    List<ObjectId> objectIdLst = new List<ObjectId>(value.GetObjectIds());
                    objectIdLst.RemoveAll(x => !copyDxfNameLst.Contains(x.ObjectClass.DxfName.ToLower()));


                    value = SelectionSet.FromObjectIds(objectIdLst.ToArray());
                }

                return value;

            }

            return null;

        }


        /// <summary>
        /// 获取以centerPoint为中心点、radius为半径的范围内或与这个范围相交的对象集，注意：因为用到窗口选择，这些对象必须在当前窗口可见，否则找不出来
        /// </summary>
        /// <param name="centerPoint">中心点</param>
        /// <param name="radius">半径</param>
        /// <param name="isInsided">是否为在范围内</param>
        /// <param name="selectionFilter">过滤器</param>
        /// <returns>对象集SelectionSet</returns>
        public SelectionSet SelectAllNearPoint(Point3d centerPoint, double radius = 5000, bool isInsided = true, SelectionFilter selectionFilter = null)
        {

            //获取包围点的点集合
            PointTool pointTool = new PointTool();

            Point3dCollection polygon = pointTool.GetPolygonSurroundPoint(centerPoint, radius);


            Editor editor = m_document.Editor;

            PromptSelectionResult result;


            //先得到当前视图，等到修改视图范围之后，再恢复到该视图
            ViewTableRecord currentView = editor.GetCurrentView();

            ViewTool viewTool = new ViewTool(editor);




            //先设置视图，避免不在当前视图看不到而选择不到的情况
            viewTool.ZoomWindow(centerPoint, radius);




            //选择集
            if (isInsided) //包含在内的情况，为窗交
            {

                if (selectionFilter == null) //无过滤器的情况
                {
                    result = editor.SelectWindowPolygon(polygon);
                }
                else  //有过滤器的情况
                {
                    result = editor.SelectWindowPolygon(polygon, selectionFilter);

                }
            }

            else //相交的情况，为交叉相交
            {
                if (selectionFilter == null) //无过滤器的情况
                {
                    result = editor.SelectCrossingPolygon(polygon); //窗交
                }
                else  //有过滤器的情况
                {
                    result = editor.SelectCrossingPolygon(polygon, selectionFilter);

                }
            }


            //返回前恢复视图
            editor.SetCurrentView(currentView);

            //选择结果

            if (result.Status == PromptStatus.OK)
            {



                return result.Value;

            }

            return null;

        }




        /// <summary>
        /// 通过提示用户选择单个对象来获取对象所在的组列表
        /// </summary>
        /// <param name="message">提示信息</param>
        /// <returns>对象所在的所有组的ObjectId列表，如果不在任何组，返回空的列表</returns>
        public List<ObjectId> SelectGroupByEntity(string message = "请选择组:")
        {
            //返回值
            List<ObjectId> groupIdLst = new List<ObjectId>();

            Database m_database = m_document.Database;

            Editor ed = m_document.Editor;

            PromptEntityResult acSSPrompt = ed.GetEntity(message);


            if (acSSPrompt.Status != PromptStatus.OK)
            {
                return groupIdLst;
            }


            ObjectTool objectTool = new ObjectTool(m_document);
            groupIdLst=objectTool.GetGroups(acSSPrompt.ObjectId);

            return groupIdLst;

        }





        /// <summary>
        /// 选择单个或多个对象，通过选择对象获取对象所在的所有的组的列表，排除重复的情况。考虑了先选择后执行的情况
        /// </summary>
        /// <returns>如果没有选到对象，或者所选的对象不在任何组内，返回空的列表</returns>
        public List<ObjectId> SelectGroupsByEntities()
        {
            //返回值
            List<ObjectId> groupIdLst = new List<ObjectId>();

            List<ObjectId> objectIdLst = GetSelectionToList();
            if (objectIdLst.Count==0)
            {
                return groupIdLst;
            }


            ObjectTool objectTool = new ObjectTool(m_document);
            groupIdLst=objectTool.GetGroups(objectIdLst);

            return groupIdLst;

        }
    }

}
