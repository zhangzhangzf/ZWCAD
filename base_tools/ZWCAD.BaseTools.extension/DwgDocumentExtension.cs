using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;

namespace ZWCAD.BaseTools.Extension
{
    /// <summary>
    /// dwg文件扩展
    /// </summary>
    public static class DwgDocumentExtension
    {


        /// <summary>
        /// 获取打开的文件，可以用来判断文件是否已经打开，只对于本打开的cad程序有效
        /// </summary>
        /// <param name="fileName">文件全路径名称</param>
        /// <returns>如果找到，返回Document，否则，返回null</returns>
        public static Document GetDocument(this string fileName)
        {

            string fileNameUpper = fileName.ToUpper();
            DocumentCollection documentCollection = Application.DocumentManager;

            foreach (Document document in documentCollection)
            {

                //图形是否已经命名
                object hasName = Application.GetSystemVariable("DWGTITLED");

                //图形命名了吗？0-没呢
                if (System.Convert.ToInt16(hasName) == 0)
                {
                    continue;  //这种情况为新创建cad，但是没有保存的情况，不可能是已存在文件
                }


                //不管文件有没有保存，只是返回名称，没有路径
                //string Name = Application.GetSystemVariable("DWGNAME").ToString();


                //如果文件没有保存，则返回名称，不带路径，如果为已经保存的文件，则返回全路径名称
                string dwgName = document.Name;

                //返回路径，不带名称
                //string dwgPath = Application.GetSystemVariable("DWGPREFIX").ToString();


                if (dwgName.ToUpper() == fileNameUpper)
                {
                    return document;
                }



            }

            //没有找到
            return null;

        }





        /// <summary>
        /// 关闭文档
        /// </summary>
        /// <param name="fileName">文档的绝对路径</param>
        /// <param name="isSaved">是否保存，默认不保存</param>
        /// <returns>如果成功，返回true，否则，返回false</returns>
        public static bool CloseDocument(this string fileName, bool isSaved = false)
        {
            Document document = fileName.GetDocument();
            if (document == null)  //文件没找到
            {
                return false;
            }

            return document.CloseDocument(isSaved);

        }


        /// <summary>
        /// 关闭文档
        /// </summary>
        /// <param name="document">文档对象</param>
        /// <param name="isSaved">是否保存，默认不保存</param>
        /// <returns>如果成功，返回true,否则，返回false</returns>
        public static bool CloseDocument(this Document document, bool isSaved = false)
        {
            string dwgName = document.Name;

            if (isSaved)  //保存关闭
            {
                document.CloseAndSave(dwgName);
            }

            else  //不保存关闭
            {
                document.CloseAndDiscard();
            }
            return true;

        }




        public static void CopyBetweenDwgFiles(this string fromFilePathName)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                using (Database srcDb = new Database(false, false))
                {
                    srcDb.ReadDwgFile(fromFilePathName, FileOpenMode.OpenForReadAndReadShare, true, "");

                    ObjectIdCollection objectIdCollection = srcDb.GetDbModelSpaceEntities();

                    if (objectIdCollection.Count > 0)
                    {
                        IdMapping idMap = new IdMapping();
                        srcDb.WblockCloneObjects(objectIdCollection, db.CurrentSpaceId, idMap, DuplicateRecordCloning.Ignore, false);
                    }
                }

                tr.Commit();

            }
        }


        /// <summary>
        /// 获取数据库模型空间的图形对象集合
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <returns></returns>
        public static ObjectIdCollection GetDbModelSpaceEntities(this Database db)
        {
            ObjectIdCollection objectIdCollection = new ObjectIdCollection();

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modelSpace = tr.GetObject(
                    bt[BlockTableRecord.ModelSpace],
                    OpenMode.ForRead) as BlockTableRecord;

                foreach (ObjectId objectId in modelSpace)
                {
                    DBObject dbobj = tr.GetObject(objectId, OpenMode.ForRead);
                    if (dbobj is Entity)
                    {
                        objectIdCollection.Add(objectId);
                    }
                }
            }

            return objectIdCollection;
        }





    }
}
