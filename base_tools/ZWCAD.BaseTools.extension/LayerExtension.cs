using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using System.Collections;
using System.Collections.Generic;

namespace ZWCAD.BaseTools.Extension
{
    /// <summary>
    /// 
    /// </summary>
    public static class LayerExtension
    {



       





        /// <summary>
        /// 锁定指定图层
        /// </summary>
        /// <param name="doc">文档对象</param>
        /// <param name="layers">图层集合，如果为null，则针对所有的图层</param>
        /// <param name="ignoreCurrent">是否忽略当前图层</param>
        /// <param name="lockZero">是否锁定当前图层</param>
        /// <returns>锁定成功的图层的ObjectId列表,如果失败，返回空的列表</returns>
        public static List<ObjectId> LockLayers(this Document doc, ObjectIdCollection layers = null, bool ignoreCurrent = true, bool lockZero = false)
        {
            //返回值
            List<ObjectId> objectIdLst = new List<ObjectId>();


            var db = doc.Database;

            var ed = doc.Editor;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var layerIds = (layers != null ? (IEnumerable)layers : (IEnumerable)tr.GetObject(db.LayerTableId, OpenMode.ForRead));

                foreach (ObjectId ltrId in layerIds)
                {
                    // Don't try to lock/unlock either the current layer or layer 0

                    // (depending on whether lockZero == true for the latter)

                    if ((!ignoreCurrent || ltrId != db.Clayer) &&
                      (lockZero || ltrId != db.LayerZero)
                    )
                    {
                        // Open the layer for write and lock/unlock it
                        var ltr = (LayerTableRecord)tr.GetObject(ltrId, OpenMode.ForWrite);


                        if (!ltr.IsLocked)
                        {
                            objectIdLst.Add(ltrId);
                            ltr.IsLocked = true;
                            ltr.IsOff = ltr.IsOff; // This is needed to force a graphics update
                        }
                    }


                }

                tr.Commit();
            }

            // These two calls will result in the layer's geometry fading/unfading
            // appropriately




            //ed.ApplyCurDwgLayerTableChanges();


            //先注销这个 因为暂时不需要考虑
            //ed.Regen();

            return objectIdLst;

        }





            /// <summary>
            /// 解锁指定图层
            /// </summary>
            /// <param name="doc">文档对象</param>
            /// <param name="layers">图层集合，如果为null，则针对所有的图层</param>
            /// <param name="ignoreCurrent">是否忽略当前图层</param>
            /// <param name="lockZero">是否锁定当前图层</param>
            /// <returns>解锁成功的图层的ObjectId列表,如果失败，返回空的列表</returns>
            public static List<ObjectId> UnlockLayers(this Document doc, ObjectIdCollection layers = null, bool ignoreCurrent = true, bool lockZero = false)
        {
            //返回值
            List<ObjectId> objectIdLst = new List<ObjectId>();


            var db = doc.Database;

            var ed = doc.Editor;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var layerIds = (layers != null ? (IEnumerable)layers : (IEnumerable)tr.GetObject(db.LayerTableId, OpenMode.ForRead));

                foreach (ObjectId ltrId in layerIds)
                {
                    // Don't try to lock/unlock either the current layer or layer 0

                    // (depending on whether lockZero == true for the latter)

                    if ((!ignoreCurrent || ltrId != db.Clayer) &&
                      (lockZero || ltrId != db.LayerZero)
                    )
                    {
                        // Open the layer for write and lock/unlock it
                        var ltr = (LayerTableRecord)tr.GetObject(ltrId, OpenMode.ForWrite);


                        if (ltr.IsLocked)
                        {
                            objectIdLst.Add(ltrId);
                            ltr.IsLocked = false;
                            ltr.IsOff = ltr.IsOff; // This is needed to force a graphics update
                        }
                    }


                }

                tr.Commit();
            }

            // These two calls will result in the layer's geometry fading/unfading
            // appropriately




            //ed.ApplyCurDwgLayerTableChanges();


            //先注销这个 因为暂时不需要考虑
            //ed.Regen();
            return objectIdLst;

        }




    }
}
