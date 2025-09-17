using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.Runtime;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.Geometry;
namespace TEST
{
    public class SHMain
    {
        [CommandMethod("AddCircle")]
        public void AddCircle()
        {
            Document zcDoc = Application.DocumentManager.MdiActiveDocument;
            Database zcDB = zcDoc.Database;
            Transaction ZcTran = zcDoc.TransactionManager.StartTransaction();
            using (ZcTran)
            {
                BlockTable zcBLT = (BlockTable)ZcTran.GetObject(zcDB.BlockTableId, OpenMode.ForRead);
                BlockTableRecord zcBLTR = (BlockTableRecord)ZcTran.GetObject(zcBLT[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                Circle zcCircle = new Circle();
                zcCircle.Center = new Point3d(2, 3, 0);
                zcCircle.Radius = 30;
                zcCircle.ColorIndex = 1;
                zcBLTR.AppendEntity(zcCircle);
                ZcTran.AddNewlyCreatedDBObject(zcCircle, true);
                ZcTran.Commit();
            }
            zcDoc.SendStringToExecute("_ZOOM E ", false, false, false);
        }
    }
}
