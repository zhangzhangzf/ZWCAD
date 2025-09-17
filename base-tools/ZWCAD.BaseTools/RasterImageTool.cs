using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.Geometry;
using Mrf.CSharp.BaseTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Point3d = ZwSoft.ZwCAD.Geometry.Point3d;

namespace ZWCAD.BaseTools
{
    /// <summary>
    /// 
    /// </summary>
    public class RasterImageTool
    {



        #region Private Variables

        Document m_document;
        Database m_database;

        #endregion



        #region Default Constructor



        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="document">文档对象</param>
        public RasterImageTool(Document document)
        {
            m_document = document;
            m_database = m_document.Database;
        }



        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="database">数据库对象</param>
        public RasterImageTool(Database database)
        {
            m_database = database;
        }



        #endregion



        /// <summary>
        /// 插入图片对象
        /// </summary>
        /// <param name="rasterImageDefId">光栅图像定义的ObjectId</param>
        /// <param name="spaceId">空间对象的ObjectId</param>
        /// <param name="insertPoint">插入点</param>
        /// <returns>插入 图片对象实例的ObjectId，如果失败，返回ObjectId.Null</returns>
        public ObjectId InsertRasterImage(ObjectId rasterImageDefId, ObjectId spaceId, Point3d insertPoint )
        {
            // 返回值
            ObjectId id = ObjectId.Null;

            if (rasterImageDefId.IsNull  || spaceId.IsNull)
            {
                return id;
            }


            using (Transaction trans = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    RasterImageDef imageDef = trans.GetObject(rasterImageDefId, OpenMode.ForWrite) as RasterImageDef;


                    // Now create the raster image that references the definition
                    RasterImage image = new RasterImage();
                    image.ImageDefId = rasterImageDefId;

                    // Prepare the orientation
                    Vector3d uCorner = new Vector3d(1.5, 0, 0);
                    Vector3d vOnPlane = new Vector3d(0, 1, 0);
                    CoordinateSystem3d coordinateSystem = new CoordinateSystem3d(insertPoint, uCorner, vOnPlane);
                    image.Orientation = coordinateSystem;

                    // And some other properties
                    image.ImageTransparency = true;
                    image.ShowImage = true;

                    // Add the image to ModelSpace
                    BlockTableRecord btr = trans.GetObject(spaceId, OpenMode.ForWrite) as BlockTableRecord;

                    id = btr.AppendEntity(image);

                    trans.AddNewlyCreatedDBObject(image, true);

                    // Create a reactor between the RasterImage
                    // and the RasterImageDef to avoid the "Unreferenced"
                    // warning the XRef palette
                    RasterImage.EnableReactors(true);
                    image.AssociateRasterDef(imageDef);

                    trans.Commit();
                }
                catch
                {
                    trans.Abort();
                }
            }
            return id;
        }




        /// <summary>
        /// 插入图片对象
        /// </summary>
        /// <param name="rasterImageDefId">光栅图像定义的ObjectId</param>
        /// <param name="insertPoint">插入点</param>
        /// <param name="spaceName">模型空间或图纸空间，如果为"PAPERSPACE"（不分大小写），为图纸空间，如果指定的名称为其它的图纸空间名称，则为指定的图纸空间名称；为空或其它，为模型空间</param>
        /// <returns>插入 图片对象实例的ObjectId，如果失败，返回ObjectId.Null</returns>
        public ObjectId InsertRasterImage(ObjectId rasterImageDefId,Point3d insertPoint, string spaceName = "MODELSPACE")
        {
            ObjectId imgId = ObjectId.Null;

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                //打开块表
                BlockTable blockTable = (BlockTable)transaction.GetObject(m_database.BlockTableId, OpenMode.ForRead);

                //还需要考虑到其它图纸空间的情况

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

                imgId = InsertRasterImage(rasterImageDefId, spaceId, insertPoint);

                transaction.Commit();
            }
            return imgId;

        }



















            /// <summary>
            /// 插入图片对象
            /// </summary>
            /// <param name="imgPath">图片对象的全路径名称</param>
            /// <param name="spaceId">空间对象的ObjectId</param>
            /// <param name="insertPoint">插入点</param>
            /// <returns>插入 图片对象实例的ObjectId，如果失败，返回ObjectId.Null</returns>
            public ObjectId InsertRasterImage(string imgPath, ObjectId spaceId, Point3d insertPoint )
        {
            // 返回值
            ObjectId id = ObjectId.Null;

            if (spaceId.IsNull)
            {
                return id;
            }




            ////16:31 2023/12/31 在云设计院中，路径用的是"\"，而不是"\\"，比如正确为:"P:\上海奉贤区无人机航拍的TIFF图片--样品(1).tif",错误为："P:\\731321d7fcbde42eae95b4df4557674f.png",
            ////这样，可能跟本地的路径不太一样，需要注意
            ////增加这种情况的判断
            //if (!File.Exists(imgPath))
            //{
            //    string tmpImgPath = imgPath.Replace(@"\\", @"\");
            //    if (File.Exists(tmpImgPath))
            //    {
            //        imgPath = tmpImgPath;

            //        MessageBox.Show("找到文件11:\n"+imgPath, "Tips");

            //    }
            //    else
            //    {

            //        MessageBox.Show("找不到文件:\n"+imgPath, "Tips");
            //        return id;

            //    }
            //}
            //else
            //{
            //    MessageBox.Show("找到文件22:\n"+imgPath, "Tips");

            //}







            // Image dictionary name constant
            string dictName = Path.GetFileNameWithoutExtension(imgPath);

            RasterImageDef imageDef;
            ObjectId imageDefId;


            using (Transaction trans = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    ObjectId imageDictId = RasterImageDef.GetImageDictionary(m_database);

                    if (imageDictId.IsNull)
                    {
                        // Image dictionary doesn't exist, create new
                        imageDictId = RasterImageDef.CreateImageDictionary(m_database);
                    }

                    // Open the image dictionary
                    DBDictionary imageDict = trans.GetObject(imageDictId, OpenMode.ForRead) as DBDictionary;

                    // See if our raster def in the image dict exists,
                    // if not, create new
                    if (imageDict.Contains(dictName))
                    {
                        // Get the raster image def
                        imageDefId = imageDict.GetAt(dictName);
                        imageDef = trans.GetObject(imageDefId, OpenMode.ForWrite) as RasterImageDef;

                    }
                    else
                    {
                        // Create a new image definition
                        imageDef = new RasterImageDef();
                        // And set its source image


                        MessageBox.Show("1", "Tips");





                        //23:46 2023/12/31 这个地方报错：eFileAccessErr
                        imageDef.SourceFileName = imgPath;







                        MessageBox.Show("2", "Tips");



                        // finally load it
                        imageDef.Load();



                        MessageBox.Show("3", "Tips");



                        imageDict.UpgradeOpen();


                        MessageBox.Show("4", "Tips");


                        imageDefId = imageDict.SetAt(dictName, imageDef);


                        MessageBox.Show("5", "Tips");


                        trans.AddNewlyCreatedDBObject(imageDef, true);


                        MessageBox.Show("6", "Tips");

                    }

                    int numberDef = imageDef.GetEntityCount(out bool lockLayer); // 返回的值 lockLayer 用不到

                    if (numberDef >= 1)
                    {
                        MessageBox.Show("名称不同，但是内容相同的图片已经插入");
                        trans.Abort();
                        return id;
                    }

                    // Now create the raster image that references the definition
                    RasterImage image = new RasterImage();


                    MessageBox.Show("7", "Tips");



                    image.ImageDefId = imageDefId;

                    // Prepare the orientation
                    Vector3d uCorner = new Vector3d(1.5, 0, 0);
                    Vector3d vOnPlane = new Vector3d(0, 1, 0);
                    CoordinateSystem3d coordinateSystem = new CoordinateSystem3d(insertPoint, uCorner, vOnPlane);
                    image.Orientation = coordinateSystem;

                    // And some other properties
                    image.ImageTransparency = true;
                    image.ShowImage = true;




                    MessageBox.Show("8", "Tips");





                    // Add the image to ModelSpace
                    BlockTable bt = trans.GetObject(m_database.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord btr = trans.GetObject(spaceId, OpenMode.ForWrite) as BlockTableRecord;



                    MessageBox.Show("9", "Tips");




                    id = btr.AppendEntity(image);



                    MessageBox.Show("10", "Tips");



                    trans.AddNewlyCreatedDBObject(image, true);



                    MessageBox.Show("11", "Tips");



                    // Create a reactor between the RasterImage
                    // and the RasterImageDef to avoid the "Unreferenced"
                    // warning the XRef palette
                    RasterImage.EnableReactors(true);


                    MessageBox.Show("12", "Tips");




                    image.AssociateRasterDef(imageDef);




                    MessageBox.Show("13", "Tips");





                    trans.Commit();




                    MessageBox.Show("插入图片成功", "Tips");


                }
                catch(Exception     e)
                {

                    

                    MessageBox.Show("失败"+e.Message, "Tips");


                }
            }
            return id;
        }


















        /// <summary>
        /// 插入图片对象
        /// </summary>
        /// <param name="imgPath">图片对象的全路径名称</param>
        /// <param name="insertPoint">插入点</param>
        /// <param name="spaceName">模型空间或图纸空间，如果为"PAPERSPACE"（不分大小写），为图纸空间，如果指定的名称为其它的图纸空间名称，则为指定的图纸空间名称；为空或其它，为模型空间</param>
        /// <returns>插入 图片对象实例的ObjectId，如果失败，返回ObjectId.Null</returns>
        public ObjectId InsertRasterImage(string imgPath, Point3d insertPoint, string spaceName = "MODELSPACE")
        {
            ObjectId imgId = ObjectId.Null;

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                //打开块表
                BlockTable blockTable = (BlockTable)transaction.GetObject(m_database.BlockTableId, OpenMode.ForRead);

                //还需要考虑到其它图纸空间的情况

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

                imgId = InsertRasterImage(imgPath, spaceId, insertPoint);

                transaction.Commit();
            }
            return imgId;
        }





        /// <summary>
        /// 获取或创建光栅图像定义对象
        /// </summary>
        /// <param name="imgPath">图片对象的全路径名称</param>
        /// <returns>插入 图片对象定义的ObjectId，如果失败，返回ObjectId.Null</returns>
        public ObjectId GetOrCreateRasterImageDef(string imgPath)
        {
            // 返回值
            ObjectId imageDefId = ObjectId.Null;

            // Image dictionary name constant
            string dictName = Path.GetFileNameWithoutExtension(imgPath);


            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    ObjectId imageDictId = RasterImageDef.GetImageDictionary(m_database);

                    if (imageDictId.IsNull)
                    {
                        // Image dictionary doesn't exist, create new
                        imageDictId = RasterImageDef.CreateImageDictionary(m_database);
                    }

                    // Open the image dictionary
                    DBDictionary imageDict = transaction.GetObject(imageDictId, OpenMode.ForRead) as DBDictionary;

                    // See if our raster def in the image dict exists,
                    // if not, create new
                    if (imageDict.Contains(dictName))
                    {
                        // Get the raster image def
                        imageDefId = imageDict.GetAt(dictName);

                    }
                    else
                    {
                        // Create a new image definition
                        RasterImageDef imageDef = new RasterImageDef();
                        // And set its source image
                        imageDef.SourceFileName = imgPath;
                        // finally load it
                        imageDef.Load();
                        imageDict.UpgradeOpen();
                        imageDefId = imageDict.SetAt(dictName, imageDef);
                        transaction.AddNewlyCreatedDBObject(imageDef, true);
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();
                }
            }
            return imageDefId;
        }





        /// <summary>
        /// 通过指定路径获取光栅图像定义对象
        /// </summary>
        /// <param name="imgPath">图片对象的全路径名称</param>
        /// <returns>获取图片对象定义的ObjectId，如果失败，返回ObjectId.Null</returns>
        public ObjectId GetRasterImageDef(string imgPath)
        {
            // 返回值
            ObjectId imageDefId = ObjectId.Null;

            // Image dictionary name constant
            string dictName = Path.GetFileNameWithoutExtension(imgPath);


            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    ObjectId imageDictId = RasterImageDef.GetImageDictionary(m_database);

                    if (!imageDictId.IsNull)
                    {

                        // Open the image dictionary
                        DBDictionary imageDict = transaction.GetObject(imageDictId, OpenMode.ForRead) as DBDictionary;

                        // See if our raster def in the image dict exists,
                        // if not, create new
                        if (imageDict.Contains(dictName))
                        {
                            // Get the raster image def
                            imageDefId = imageDict.GetAt(dictName);

                        }
                    }
                    transaction.Commit();

                }

                catch
                {
                    transaction.Abort();
                }
            }

            return imageDefId;
        }





        /// <summary>
        /// 通过指定路径获取光栅图像对象实例的集合
        /// </summary>
        /// <param name="imgPath">图片对象的全路径名称</param>
        /// <param name="spaceName">模型空间或图纸空间，如果为"PAPERSPACE"（不分大小写），为图纸空间，如果指定的名称为其它的图纸空间名称，则为指定的图纸空间名称；为空或其它，为模型空间</param>
        /// <returns>如果不存在或失败，返回空的列表</returns>
        public List<ObjectId> GetRasterImages(string imgPath, string spaceName = "MODELSPACE")
        {

            //返回值
            List<ObjectId> inageLst = new List<ObjectId>();


            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                //打开块表
                BlockTable blockTable = (BlockTable)transaction.GetObject(m_database.BlockTableId, OpenMode.ForRead);

                //还需要考虑到其它图纸空间的情况

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

                inageLst = GetRasterImages(imgPath, spaceId);

                transaction.Commit();
            }
            return inageLst;
        }






        /// <summary>
        /// 通过指定路径获取光栅图像对象实例的集合
        /// </summary>
        /// <param name="imgPath">图片对象的全路径名称</param>
        /// <param name="spaceId">空间对象的ObjectId</param>
        /// <returns>如果不存在或失败，返回空的列表</returns>
        public List<ObjectId> GetRasterImages(string imgPath, ObjectId spaceId)
        {

            //返回值
            List<ObjectId> imageLst = new List<ObjectId>();

            if (spaceId.IsNull)
            {
                return imageLst;
            }


            ObjectId imageDefId = GetRasterImageDef(imgPath);
            if (imageDefId.IsNull)
            {
                return imageLst;
            }

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    BlockTableRecord btr = transaction.GetObject(spaceId, OpenMode.ForWrite) as BlockTableRecord;
                    foreach (var objectid in btr)
                    {
                        Entity entity = transaction.GetObject(objectid, OpenMode.ForRead) as Entity;
                        if (entity is RasterImage image)
                        {

                            if (image.ImageDefId == imageDefId)
                            {
                                imageLst.Add(image.Id);
                            }
                        }
                    }
                    transaction.Commit();
                }
                catch (System.Exception)
                {
                    transaction.Abort();
                }
            }
            return imageLst;
        }





        /// <summary>
        /// 通过指定光栅图像定义获取光栅图像对象实例的集合
        /// </summary>
        /// <param name="rasterImageDefId">光栅图像定义的ObjectId</param>
        /// <param name="spaceName">模型空间或图纸空间，如果为"PAPERSPACE"（不分大小写），为图纸空间，如果指定的名称为其它的图纸空间名称，则为指定的图纸空间名称；为空或其它，为模型空间</param>
        /// <returns>如果不存在或失败，返回空的列表</returns>
        public List<ObjectId> GetRasterImages(ObjectId rasterImageDefId, string spaceName = "MODELSPACE")
        {

            //返回值
            List<ObjectId> imageLst = new List<ObjectId>();


            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                //打开块表
                BlockTable blockTable = (BlockTable)transaction.GetObject(m_database.BlockTableId, OpenMode.ForRead);

                //还需要考虑到其它图纸空间的情况

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

                imageLst = GetRasterImages(rasterImageDefId, spaceId);

                transaction.Commit();
            }
            return imageLst;

        }






            /// <summary>
            /// 通过指定光栅图像定义获取光栅图像对象实例的集合
            /// </summary>
            /// <param name="rasterImageDefId">光栅图像定义的ObjectId</param>
            /// <param name="spaceId">空间对象的ObjectId</param>
            /// <returns>如果不存在或失败，返回空的列表</returns>
            public List<ObjectId> GetRasterImages(ObjectId rasterImageDefId, ObjectId spaceId)
        {

            //返回值
            List<ObjectId> imageLst = new List<ObjectId>();

            if (rasterImageDefId.IsNull|| spaceId.IsNull)
            {
                return imageLst;
            }


            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    BlockTableRecord btr = transaction.GetObject(spaceId, OpenMode.ForWrite) as BlockTableRecord;
                    foreach (var objectid in btr)
                    {
                        Entity entity = transaction.GetObject(objectid, OpenMode.ForRead) as Entity;
                        if (entity is RasterImage image)
                        {

                            if (image.ImageDefId == rasterImageDefId)
                            {
                                imageLst.Add(image.Id);
                            }
                        }
                    }
                    transaction.Commit();
                }
                catch (System.Exception)
                {
                    transaction.Abort();
                }
            }
            return imageLst;
        }
   
    
    
    
    
    }
}
