/**********************************************************
Copyright(C) UIH 2011                                     *
描    述：                                                *
版    本：4.0.30319.42000                                 *
作    者：莫瑞芳                                          *
创建时间：2021/10/5 15:07:19                              *
**********************************************************/

using ZWCAD.BaseTools.Extension;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.Geometry;
using System.Collections.Generic;

namespace ZWCAD.BaseTools
{
    /// <summary>
    /// 错误信息工具
    /// </summary>
    public class ErrorMessageTool
    {

        /// <summary>
        /// 构造函数
        /// </summary>
        public ErrorMessageTool()
        {

        }


        /// <summary>
        /// 显示错误的对象的位置，判断列表是否为null或空
        /// </summary>
        /// <param name="failObjectIdLst">错误对象的ObjectId列表</param>
        /// <param name="isReWrite">如果图层"DEFPOINTS"存在，是否修改图层的颜色，默认不修改</param>
        public void ShowFailureObjects(List<ObjectId> failObjectIdLst, bool isReWrite = false)
        {

            if (failObjectIdLst == null || failObjectIdLst.Count == 0)
            {
                return;
            }

            Database database = failObjectIdLst[0].Database;

            //将添加的线和文字放到图层
            LayerTool layerTool = new LayerTool(database);

            string layerName = "DEFPOINTS";

            layerTool.AddLayerTableRecord(layerName, 1, isReWrite);


            ObjectTool objectTool = new ObjectTool(database);

            Point3d firstPoint = new Point3d(0, 0, 0);

            foreach (ObjectId objectId in failObjectIdLst)
            {

                Point3d? secondPointOrNull = objectTool.GetEntityBoundingBoxPoint(objectId, 0);


                if (secondPointOrNull == null) //读取有误
                {
                    continue;
                }

                Point3d secondPoint = (Point3d)secondPointOrNull;

                ObjectId lineObjectId = database.AddLine(firstPoint, secondPoint);
                layerTool.ChangeEntityLayer(lineObjectId, layerName);
            }



            DBText dBText = new DBText
            {
                Position = new Point3d(0, 0, 0),
                TextString = "需要手动修改",

                Height = 2000
            };

            ObjectId textObjectId = database.AddEntity(dBText);
            layerTool.ChangeEntityLayer(textObjectId, layerName);

        }
    }




}

