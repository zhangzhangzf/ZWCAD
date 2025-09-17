using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;
using ZwSoft.ZwCAD.Geometry;
using System;
using System.Collections.Generic;

namespace ZWCAD.BaseTools
{

    /// <summary>
    /// 选择集工具
    /// </summary>
    public class SelectionSetTool
    {

        Document m_document;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="document">文档对象</param>
        public SelectionSetTool(Document document)
        {
            m_document = document;

        }


        /// <summary>
        /// 获取上一次选择集的宽度和高度
        /// </summary>
        public void GetPreviousSelectionSetWidthAndHeight()
        {

            //使用时间戳作为块名
            // string blockName = DateTime.Now.ToLongDateString();


            //将里面的空格删除
            // blockName =  blockName.Replace(" ", "");

            //string commandSring = "-block " + blockName + "\n0,0 p\n-insert blockName ";



            m_document.Editor.SelectImplied();





            string commandSring = "copybase 0,0 p \n" + "pasteblock 0,0\n";
            m_document.SendStringToExecute(commandSring, true, false, true);

            SelectionSet selectionSet = m_document.Editor.SelectLast().Value;

            ObjectTool objectTool = new ObjectTool(m_document);
            ObjectId blockObjectId = selectionSet[0].ObjectId;
            Entity entity = objectTool.GetObject(blockObjectId) as Entity;

            BlockReference blockReference = objectTool.GetObject(blockObjectId) as BlockReference;

            // double[] widthAndHeight = objectTool.GetEntityBoundingWidthAndHeight(entity);

            //string blockName= blockReference.Name;

            // entity.Erase(true);

            // //将块从文件中清除

            // commandSring = "-purge b " + blockName + "n\n";
            // m_document.SendStringToExecute(commandSring, true, false, true);


            // return widthAndHeight; 

        }


/// <summary>
/// 获取选择集选择时的拾取点列表
/// </summary>
/// <param name="selectionSet">选择集</param>
/// <returns>拾取点列表，如果失败，返回空的列表</returns>
        public List<Point3d> GetPickPoints(SelectionSet selectionSet)
        {
            //返回值
            List<Point3d> pickPointLst = new List<Point3d>();
            Database db = m_document.Database;
            Editor ed = m_document.Editor;

            foreach (SelectedObject ssItem in selectionSet)
            {
                switch (ssItem.SelectionMethod)
                {
                    case SelectionMethod.PickPoint:
                        PickPointSelectedObject ppSelObj = ssItem as PickPointSelectedObject;
                        Point3d pickedPoint = ppSelObj.PickPoint.PointOnLine;
                        pickPointLst.Add(pickedPoint);
                        break;

                    case SelectionMethod.Crossing:
                        CrossingOrWindowSelectedObject crossSelObj = ssItem as CrossingOrWindowSelectedObject;
                        PickPointDescriptor[] crossSelPickedPoints = crossSelObj.GetPickPoints();
                        foreach (var item in crossSelPickedPoints)
                        {
                            pickPointLst.Add(item.PointOnLine);
                        }
                        break;

                    case SelectionMethod.Window:
                        CrossingOrWindowSelectedObject windSelObj = ssItem as CrossingOrWindowSelectedObject;
                        PickPointDescriptor[] winSelPickedPoints = windSelObj.GetPickPoints();
                        foreach (var item in winSelPickedPoints)
                        {
                            pickPointLst.Add(item.PointOnLine);
                        }
                        break;
                }
            }
            return pickPointLst;
        }

        /// <summary>
        /// 用于测试选择集的类型以及获取选择时的拾取点坐标
        /// </summary>
        public void TestSelection()
        {
            Database db = null;
            Editor ed = null;

            try
            {
                db = m_document.Database;
                ed = m_document.Editor;

                //Select Objects, use ed.GetSelection to benefit from Window/Crossing selection visual feedback
                PromptSelectionOptions pso = new PromptSelectionOptions();
                pso.MessageForAdding = "Select  Objects";
                pso.SingleOnly = false;
                PromptSelectionResult psr = ed.GetSelection(pso);
                if (psr.Status != PromptStatus.OK)
                    return;

                SelectionSetDelayMarshalled ssMarshal = (SelectionSetDelayMarshalled)psr.Value;
                ZdsName name = ssMarshal.Name;
                var selObjIds = ssMarshal.GetObjectIds();
                ed.WriteMessage("\n {0} Objects selected", selObjIds.Length);
                for (int i = 0; i < selObjIds.Length; i++)
                    ed.WriteMessage("\n\t [{0}]: {1} ({2})", i, selObjIds[i].ObjectClass.DxfName, selObjIds[i].Handle);

                SelectionSet selSet = (SelectionSet)ssMarshal;
                ed.WriteMessage("\n {0} Selections in SelectionSet", selSet.Count);

                foreach (SelectedObject ssItem in selSet)
                {
                    ed.WriteMessage("\n\t {0} Selected: {1} ({2})", Enum.GetName(typeof(SelectionMethod), ssItem.SelectionMethod), ssItem.ObjectId.ObjectClass.DxfName, ssItem.ObjectId.Handle);
                    switch (ssItem.SelectionMethod)
                    {
                        case SelectionMethod.PickPoint:
                            PickPointSelectedObject ppSelObj = ssItem as PickPointSelectedObject;
                            Point3d pickedPoint = ppSelObj.PickPoint.PointOnLine;
                            ed.WriteMessage("\n\t\t Selected at: {0}", pickedPoint.ToString());
                            break;

                        case SelectionMethod.Crossing:
                            CrossingOrWindowSelectedObject crossSelObj = ssItem as CrossingOrWindowSelectedObject;
                            PickPointDescriptor[] crossSelPickedPoints = crossSelObj.GetPickPoints();
                            ed.WriteMessage("\n\t\t Crossing at: {0}..{1}", crossSelPickedPoints[0].PointOnLine.ToString(), crossSelPickedPoints[1].PointOnLine.ToString());
                            break;

                        case SelectionMethod.Window:
                            CrossingOrWindowSelectedObject windSelObj = ssItem as CrossingOrWindowSelectedObject;
                            PickPointDescriptor[] winSelPickedPoints = windSelObj.GetPickPoints();
                            ed.WriteMessage("\n\t\t Window at: {0}..{1}", winSelPickedPoints[0].PointOnLine.ToString(), winSelPickedPoints[1].PointOnLine.ToString());
                            break;

                    }
                }
            }
            catch (System.Exception ex)
            {
                if (ed != null)
                    ed.WriteMessage(ex.Message);
                else
                    System.Windows.Forms.MessageBox.Show(ex.Message,
                                "TestSelection",
                                System.Windows.Forms.MessageBoxButtons.OK,
                                System.Windows.Forms.MessageBoxIcon.Error);
            }
        }










    }
}
