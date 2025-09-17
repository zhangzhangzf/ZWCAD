using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.Geometry;
using ZwSoft.ZwCAD.Runtime;
using System;
using AcRx = ZwSoft.ZwCAD.Runtime;

namespace Mrf.CSharp.BaseTools.Extension
{

    /// <summary>
    /// 曲线扩展
    /// </summary>
    public static class CurveExtension
    {



        /// <summary>
        /// 将数据库中存留的直线或圆弧替换为多段线，同时删除直线或圆弧，多段线保留直线或圆弧的属性，如果事务为空，将不替换
        /// </summary>
        /// <param name="curve">直线或圆弧，如果不为两者，将会提示</param>
        /// <param name="trans">事务，默认空</param>
        /// <returns>替换后的多段线</returns>
        //public static Polyline ReplaceWithPolyline(this Curve curve, OpenCloseTransaction trans = null)
        public static Polyline ReplaceWithPolyline(this Curve curve, Transaction trans = null)
        {
            return Convert(curve, trans);
        }




        /// <summary>
        /// Creates a Polyline that is equivalent to a given Arc or Line,
        /// and optionally replaces the Arc or Line with the Polyline in
        /// the Database where the Arc or Line resides.
        /// </summary>
        /// <remarks>The curve argument must be a Line or Arc. If the curve
        /// argument is database-resident and the second transaction argument 
        /// is provided, the curve MUST be opened via that transaction. 
        /// 
        /// If the curve argument is database-resident and a transaction is
        /// provided, the new Polyline will replace the curve in the database
        /// and inherit the common entity properties, handle, and application 
        /// data of the curve. The curve argument will no longer be usable 
        /// upon return. If the curve argument is not database-resident, the 
        /// resulting polyline will not be database-resident and <em>must</em> 
        /// be disposed or added to a database. The curve argument will be 
        /// unchanged and still be usable.
        /// </remarks>
        /// <param name="curve">The Line or Arc to convert/replace</param>
        /// <param name="trans">The OpenCloseTransaction which the database-
        /// resident curve was obtained from.</param>
        /// <returns>The Polyline equivalent of the given curve</returns>


        //public static Polyline Convert(Curve curve, OpenCloseTransaction trans = null)
        public static Polyline Convert(Curve curve, Transaction trans = null)
        {
            if (curve == null)
                throw new ArgumentNullException("曲线");
            if (!(curve is Line || curve is Arc))
                ErrorStatus.WrongObjectType.Throw("必须为直线或圆弧");
            if (curve.IsTransactionResident)
                ErrorStatus.InvalidInput.Throw("曲线必须来自一个OpenCloseTransaction");
            Polyline pline = new Polyline(1);
            try
            {
                var start = curve.StartPoint.TransformBy(curve.Ecs.Inverse());
                if (curve is Arc)
                    pline.TransformBy(curve.Ecs);
                pline.Elevation = start.Z;
                pline.Thickness = curve.GetThickness();
                pline.AddVertexAt(0, new Point2d(start.X, start.Y), 0, 0, 0);
                pline.JoinEntity(curve);
                pline.SetPropertiesFrom(curve);
                if (curve.Database != null && trans != null)
                {
                    if (!curve.IsWriteEnabled)
                        curve.UpgradeOpen();
                    curve.HandOverTo(pline, true, true);
                    trans.AddNewlyCreatedDBObject(pline, true);
                    trans.AddNewlyCreatedDBObject(curve, false);
                    curve.Dispose();
                }
                return pline;
            }
            catch
            {
                pline.Dispose();
                throw;
            }
        }

        public static void Throw(this AcRx.ErrorStatus es, string message = null)
        {
            throw new AcRx.Exception(es, message);
        }

        public static double GetThickness(this Curve curve)
        {
            Line line = curve as Line;
            return line != null ? line.Thickness : ((Arc)curve).Thickness;
        }
    }








}
