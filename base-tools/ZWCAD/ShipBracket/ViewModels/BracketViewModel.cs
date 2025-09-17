using ZWCAD.BaseTools;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;
using ZwSoft.ZwCAD.Geometry;
using Mrf.CSharp.BaseTools.Extension;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using ZwSoft.ZwCAD.ApplicationServices;
using Application = ZwSoft.ZwCAD.ApplicationServices.Application;
using Point3dInCAD = ZwSoft.ZwCAD.Geometry.Point3d;
using ZWCAD.ShipBracket.Views;

namespace ZWCAD.ShipBracket.ViewModels
{
    /// <summary>
    /// 添加船舶肘板
    /// </summary>
    public partial class BracketViewModel : BindableBase
    {

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="document">文档对象</param>
        public BracketViewModel(Document document)
        {
            m_document = document;
        }


        /// <summary>
        /// 创建肘板 需要注意：不管第一条边和第二条边如何选，要处理为第一条边角度比第二条边小，从而在画肘板的时候能够逆时针，因为如果有通焊孔的时候，添加的凸度会按逆时针
        /// </summary>
        public void Run()
        {
            PolylineTool polylineTool = new PolylineTool(m_document);
            ObjectTool objectTool = new ObjectTool(m_document);
            CurveTool curveTool = new CurveTool(m_document);
            LayerTool layerTool = new LayerTool(m_document);
            PointTool pointTool = new PointTool();

            //选择第一条边
            string message = "请选择第一条边或 [设置(S)] <退出>:";
            (ObjectId firstSideId, Point3d? tmpFirstPickPoint) = SelectBracketSide(message);
            if (firstSideId.IsNull || tmpFirstPickPoint==null) return;
            var firstPickPoint = tmpFirstPickPoint.Value;


            var aa = curveTool.GetVectorAtPoint(firstSideId, firstPickPoint);



            //return;




            //选择第二条边
            message = "请选择第二条边或 [设置(S)] <退出>:";
            (ObjectId secondSideId, Point3d? tmpSecondPickPoint)= SelectBracketSide(message);
            if (secondSideId.IsNull || tmpSecondPickPoint==null) return;
            var secondPickPoint = tmpSecondPickPoint.Value;

            //计算两条边的交点
            var intersectPointLst = objectTool.GetIntersectWithPoints(firstSideId, secondSideId, Intersect.ExtendBoth);
            if (intersectPointLst.Count==0) return;
            var pointOnBoth = intersectPointLst.FirstOrDefault();

            //判断两条边的拾取点是在交点所在对应边上的负向还是正向，-1表示负向，1表示正向，2 表示两点重合，0表示判断有误
            var firstDirection = curveTool.GetPointDirection(firstSideId, pointOnBoth, firstPickPoint, true);
            if (firstDirection==0 || firstDirection==2) return;

            var secondDirection = curveTool.GetPointDirection(secondSideId, pointOnBoth, secondPickPoint, true);
            if (secondDirection==0 || secondDirection==2) return;

            //计算肘板在两条边上的点

            //此方法为考虑长度是沿着曲线方向的长度
            //double firstLengthToStartPoint = confirmedFirstLength*firstDirection;
            //var tmpPointOnFirstSide = curveTool.GetPoint(firstSideId, firstLengthToStartPoint, pointOnBoth, true, true);

            //改为到交点的直线距离
            var tmpPointOnFirstSide = curveTool.GetClosedPointByDistance(firstSideId, pointOnBoth, confirmedFirstLength, firstPickPoint, true);
            if (tmpPointOnFirstSide==null)
            {
                return;
            }
            var pointOnFirstSide = tmpPointOnFirstSide.Value;


            //double secondLengthToStartPoint = confirmedSecondLength*secondDirection;
            //var tmpPointOnSecondSide = curveTool.GetPoint(secondSideId, secondLengthToStartPoint, pointOnBoth, true, true);
            var tmpPointOnSecondSide = curveTool.GetClosedPointByDistance(secondSideId, pointOnBoth, confirmedSecondLength, secondPickPoint, true);
            if (tmpPointOnSecondSide==null)
            {
                return;
            }
            var pointOnSecondSide = tmpPointOnSecondSide.Value;

            //比较两条边角度， 确保第一条边角度小于第二条边，逆时针
            var firstAngle = pointTool.GetAngleToXAxis(pointOnBoth, pointOnFirstSide);
            var secondAngle = pointTool.GetAngleToXAxis(pointOnBoth, pointOnSecondSide);
            if ((firstAngle<secondAngle && secondAngle-firstAngle>=Math.PI) ||  //第一条边角度小于第二条边 且 两者夹角大于pi,
            (firstAngle>secondAngle && firstAngle -secondAngle<Math.PI) ////第一条边角度大于第二条边 且两者夹角小于pi
            )
            {
                //交换边对象
                ClassExtension.Swap(ref firstSideId, ref secondSideId);
                //交换方向
                ClassExtension.Swap(ref firstDirection, ref secondDirection);

                //交换肘板在两条边上的点
                ClassExtension.Swap(ref pointOnFirstSide, ref pointOnSecondSide);

                //交换角度
                ClassExtension.Swap(ref firstAngle, ref secondAngle);
            }


            //创建肘板


            //先记录当前图层
            string currentLayer = layerTool.GetCurrentLayer();


            var currentCmd = Application.GetSystemVariable("cmdecho");

            var currentOsmode = Application.GetSystemVariable("osmode");

            Application.SetSystemVariable("cmdecho", 0);

            //需要将捕捉关掉，不然可能会捕捉到附近的点
            Application.SetSystemVariable("osmode", 0);







            //创建多段线表示肘板
            double tolerance = 1E-6;

            //趾端高度所在的点

            Point3d? toesPointOnFirstSide = null;
            Point3d? toesPointOnSecondSide = null;
            if (confirmedToesLength>tolerance)
            {
                toesPointOnFirstSide=pointTool.GetPolarPoint(pointOnFirstSide, confirmedToesLength, firstAngle+Math.PI/2);
                toesPointOnSecondSide=pointTool.GetPolarPoint(pointOnSecondSide, confirmedToesLength, secondAngle-Math.PI/2);
            }



            if (confirmedHoleRadius>tolerance) //存在通焊孔
            {
                //通焊孔在第一条边上的点
                double firstHoleRadius = confirmedHoleRadius*firstDirection;
                var tmpHolePointOnFirstSide = curveTool.GetPoint(firstSideId, firstHoleRadius, pointOnBoth, true, true);
                if (!tmpHolePointOnFirstSide.HasValue) return;
                var holePointOnFirstSide = tmpHolePointOnFirstSide.Value;


                //通焊孔在第二条边上的点
                double secondHoleRadius = confirmedHoleRadius*secondDirection;
                var tmpHolePointOnSecondSide = curveTool.GetPoint(secondSideId, secondHoleRadius, pointOnBoth, true, true);
                if (!tmpHolePointOnSecondSide.HasValue) return;
                var holePointOnSecondSide = tmpHolePointOnSecondSide.Value;

                List<Point3dInCAD> pointLst = new List<Point3dInCAD> { pointOnSecondSide };
                if (toesPointOnFirstSide!=null && toesPointOnSecondSide!=null)
                {
                    pointLst.Add(toesPointOnSecondSide.Value);
                    pointLst.Add(toesPointOnFirstSide.Value);
                }
                pointLst.Add(pointOnFirstSide);
                polylineTool.CreatePolylineWithArc(holePointOnFirstSide, pointOnBoth, holePointOnSecondSide, pointLst);
            }
            else
            {
                //PolylineTool polylineTool = new PolylineTool(m_document);
                List<Point3dInCAD> pointLst = new List<Point3dInCAD>() { pointOnBoth, pointOnFirstSide };

                if (toesPointOnFirstSide!=null && toesPointOnSecondSide!=null)
                {
                    pointLst.Add(toesPointOnFirstSide.Value);
                    pointLst.Add(toesPointOnSecondSide.Value);
                }
                pointLst.Add(pointOnSecondSide);
                polylineTool.CreatePolyline(pointLst, true);
            }

            Application.SetSystemVariable("cmdecho", currentCmd);

            //恢复原来的捕捉
            Application.SetSystemVariable("osmode", currentOsmode);
        }

        /// <summary>
        /// 在屏幕中选择对象，并返回选择到的第一个对象和拾取点
        /// </summary>
        /// <param name="message">提示信息</param>
        /// <returns>对象的ObjectId和拾取点，如果失败，返回(ObjectId.Null,null）</returns>
        private (ObjectId, Point3d?) SelectBracketSide(string message = "请选择第一条边:")
        {
            //选择直线和多段线
            TypedValue[] values = new TypedValue[] { new TypedValue((int)DxfCode.Start, "LINE,LWPOLYLINE") };

            SelectionFilter selectionFilter = new SelectionFilter(values);

            PromptSelectionOptions options = new PromptSelectionOptions
            {
                MessageForAdding = message,

                SingleOnly=true //只能选择单个对象
            };
            options.Keywords.Add("Setting");
            string keyWords = options.Keywords.GetDisplayString(true);

            // 输入关键字后执行的操作
            options.KeywordInput +=
              delegate (object sender, SelectionTextInputEventArgs e)
              {
                  BracketView view = new BracketView
                  {
                      DataContext=this
                  };
                  view.ShowDialog();
              };

            SelectionTool selectEntityTool = new SelectionTool(m_document);
            SelectionSet selectionSet = selectEntityTool.GetSelection(options, selectionFilter);
            if (selectionSet==null || selectionSet.Count==0) return (ObjectId.Null, null);

            SelectionSetTool selectionSetTool = new SelectionSetTool(m_document);
            var pickPointLst = selectionSetTool.GetPickPoints(selectionSet);
            if (pickPointLst.Count==0) return (ObjectId.Null, null);

            return (selectionSet.GetObjectIds().FirstOrDefault(), pickPointLst.FirstOrDefault());
        }

        private void ConfirmCommandRun()
        {
            confirmedFirstLength=FirstLength;
            confirmedSecondLength=SecondLength;
            confirmedHoleRadius=HoleRadius;
            confirmedToesLength=ToesLength;
        }



















    }

}





