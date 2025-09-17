using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using System;

namespace ZWCAD.BaseTools.Extension
{
    /// <summary>
    /// 布局类扩展
    /// </summary>
    public static class LayoutExtension
    {

        /// <summary>
        ///获取布局的最大可能尺寸 
        /// </summary>
        /// <returns>布局的最大范围</returns>
        public static Extents2d GetMaximumExtents(this Layout lay)
        {
            // If the drawing template is imperial, we need to divide by
            // 1" in mm (25.4)

            var div = lay.PlotPaperUnits == PlotPaperUnit.Inches ? 25.4 : 1.0;
            // We need to flip the axes if the plot is rotated by 90 or 270 deg

            var doIt = lay.PlotRotation == PlotRotation.Degrees090 ||
                lay.PlotRotation == PlotRotation.Degrees270;

            // Get the extents in the correct units and orientation
            var min = lay.PlotPaperMargins.MinPoint.Swap(doIt) / div;
            var max = (lay.PlotPaperSize.Swap(doIt) - lay.PlotPaperMargins.MaxPoint.Swap(doIt).GetAsVector()) / div;

            return new Extents2d(min, max);
        }



        /// <summary>
        ///设置布局对象的打印设置 
        ///如： lay.SetPlotSettings(                      //"ISO_full_bleed_2A0_(1189.00_x_1682.00_MM)", // Try this big boy!
        ///            "ANSI_B_(11.00_x_17.00_Inches)",
        ///           "monochrome.ctb",
        ///           "DWF6 ePlot.pc3"
        ///        );
        /// </summary>
        /// <param name="lay">布局对象</param>
        /// <param name="pageSize">图纸尺寸</param>
        /// <param name="styleSheet">打印样式表（画笔指定） (ctb or stb).</param>
        /// <param name="device">输出设备名称</param>
        public static void SetPlotSettings(this Layout lay, string pageSize, string styleSheet, string device)
        {
            Database database = lay.Database;

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                try
                {
                    using (var ps = new PlotSettings(lay.ModelType))
                    {
                        ps.CopyFrom(lay);

                        var psv = PlotSettingsValidator.Current;

                        // Set the device
                        var devs = psv.GetPlotDeviceList();

                        if (devs.Contains(device))
                        {
                            psv.SetPlotConfigurationName(ps, device, null);
                            psv.RefreshLists(ps);
                        }


                        // Set the media name/size
                        var mns = psv.GetCanonicalMediaNameList(ps);
                        if (mns.Contains(pageSize))
                        {
                            psv.SetCanonicalMediaName(ps, pageSize);
                        }

                        // Set the pen settings
                        var ssl = psv.GetPlotStyleSheetList();
                        if (ssl.Contains(styleSheet))
                        {
                            psv.SetCurrentStyleSheet(ps, styleSheet);
                        }


                        // Copy the PlotSettings data back to the Layout

                        var upgraded = false;

                        if (!lay.IsWriteEnabled)
                        {
                            lay.UpgradeOpen();
                            upgraded = true;
                        }

                        lay.CopyFrom(ps);

                        if (upgraded)
                        {
                            lay.DowngradeOpen();
                        }

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
        ///  在布局中对指定视口实施某个函数
        ///  比如：lay.ApplyToViewport( 2,vp => { vp.ResizeViewport(ext, 0.8); })
        /// </summary>
        /// <param name="lay"></param>
        /// <param name="vpNum">指定视口的代号，如果不存在，创建新的视口</param>
        /// <param name="f">用于指定视口的函数</param>
        public static void ApplyToViewport(this Layout lay, int vpNum, Action<Viewport> f)
        {
            Database database = lay.Database;

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                try
                {
                    var vpIds = lay.GetViewports();
                    Viewport vp = null;

                    foreach (ObjectId vpId in vpIds)
                    {
                        var vp2 = transaction.GetObject(vpId, OpenMode.ForWrite) as Viewport;
                        if (vp2 != null && vp2.Number == vpNum)  //已经存在
                        {
                            vp = vp2;
                            break;
                        }

                    }

                    if (vp == null) //不存在 创建一个
                    {
                        var btr = (BlockTableRecord)transaction.GetObject(lay.BlockTableRecordId, OpenMode.ForWrite);
                        vp = new Viewport();
                        btr.AppendEntity(vp);
                        transaction.AddNewlyCreatedDBObject(vp, true);

                        // Turn it - and its grid - on
                        vp.On = true;
                        vp.GridOn = true;
                    }

                    // Finally we call our function on it

                    f(vp);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();
                }

            }
        }
    }
}
