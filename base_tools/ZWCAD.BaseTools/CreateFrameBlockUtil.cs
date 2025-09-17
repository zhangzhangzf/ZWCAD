using ZWCAD.BaseTools.Extension;
using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.Geometry;
using Mrf.CSharp.BaseTools;
using Mrf.CSharp.BaseTools.Extension;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Point3dInCAD = ZwSoft.ZwCAD.Geometry.Point3d;

namespace ZWCAD.BaseTools
{
    /// <summary>
    /// 添加图框工具
    /// </summary>
    public class CreateFrameBlockUtil
    {



        #region Private Variables


        Document m_document;


        //图框信息列表
        List<FrameBlock> m_frameBlockLst;



        #endregion



        #region Default Constructor


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="document">文档对象</param>
        public CreateFrameBlockUtil(Document document)
        {
            m_document = document;

            //获取图框信息列表,如果失败，返回空的列表
            m_frameBlockLst = GetFrameBlockInformation();
        }










        #endregion




        #region CommandMethods



        /// <summary>
        /// 添加多个图框
        /// </summary>
        /// <returns>添加的图框对象的ObjectId列表，如果失败，返回空的列表</returns>
        public List<ObjectId> AddMultiFrameBlocks()
        {
            //返回值
            List<ObjectId> frameBlockIdLst = new List<ObjectId>();

            if (LeftInsertPoint == null)
            {
                LeftInsertPoint = Point3dInCAD.Origin;
            }

            if (MultiElementIdAndScales == null | MultiElementIdAndScales.Count == 0)
            {
                MessageBox.Show("请先设置图框内的对象参数：MultiElements", "Tips");
                return frameBlockIdLst;
            }



            foreach (var item in MultiElementIdAndScales)
            {

                ElementIds = item.Item1;
                Scale = item.Item2;

                var framBlockId = AddSingleFrameBlockByFindingFrame();
                if (framBlockId.IsNull) //失败，直接退出
                {
                    continue;
                }

                //以下为成功
                //下一个图框的基点
                LeftInsertPoint = RightInsertPoint + new Vector3d(XTolerance, YTolerance, 0);

            }

            return frameBlockIdLst;

        }




        /// <summary>
        /// 添加单个图框 比例指定，需要算出图框
        /// </summary>
        /// <returns>如果成功，返回图框对象的ObjectId，否则，返回ObjectId.Null</returns>
        public ObjectId AddSingleFrameBlockByFindingFrame()
        {
            //返回值
            ObjectId frameBlockId = ObjectId.Null;


            if (LeftInsertPoint == null)
            {
                MessageBox.Show("请先设置图框插入点属性：LeftInsertPoint", "Tips");
                return frameBlockId;
            }

            if (ElementIds == null | ElementIds.Count == 0)
            {
                MessageBox.Show("请先设置图框内的对象属性：ElementIds", "Tips");
                return frameBlockId;
            }


            if (m_frameBlockLst.Count == 0)  //获取失败
            {
                MessageBox.Show("获取图框信息失败", "Tips");
                return frameBlockId;
            }


            ////先计算能包住所有对象的最大长度和宽度，便于选择合适的图纸（单位：mm）  9:38 2023/6/25 可以用
            //GetSheetLengthAndWidth(out double maxLength, out double maxWidth);
            //if (double.IsNaN(maxLength))
            //{
            //    MessageBox.Show("获取对象最大长度失败", "Tips");
            //    return frameBlockId;
            //}

            //if (double.IsNaN(maxWidth))
            //{
            //    MessageBox.Show("获取对象最大宽度失败", "Tips");
            //    return frameBlockId;
            //}

            //9:38 2023/6/25 修改
            ObjectTool objectTool = new ObjectTool(m_document);
            double[] widthAndHeight = objectTool.GetDBObjectsWidthAndHeight(ElementIds);

            if (widthAndHeight == null)
            {
                MessageBox.Show("获取对象最大宽度和高度失败", "Tips");
                return frameBlockId;
            }

            double maxLength = widthAndHeight[0];
            double maxWidth = widthAndHeight[1];







            //考虑比例，先转换为1：1
            maxLength /= Scale;
            maxWidth /= Scale;



            //获取大小合适的图框,如果失败，返回null
            FrameBlock suitableFrameBlock = FindSuitableFrameBlock(maxLength, maxWidth);

            if (suitableFrameBlock == null)
            {
                MessageBox.Show("找不到合适的图框，长度: " + maxLength + " ,宽度: " + maxWidth, "Tips");
                return frameBlockId;
            }



            //以下为找到合适的图框

            FrameBlock = suitableFrameBlock;


            //插入图框
            frameBlockId = InsertFrameBlock();

            if (frameBlockId == ObjectId.Null) //创建失败
            {
                return frameBlockId;
            }




            //将所有对象移动到合适的位置，中心对中心

            //先获取图纸的中心  注意：考虑比例
            Point3dInCAD sheetCenterPoint = LeftInsertPoint + new Vector3d(suitableFrameBlock.CenterXOffset, suitableFrameBlock.CenterYOffset, 0) * Scale;



            //先计算能包住所有对象的中点坐标
            Point3dInCAD elementCenterPoint = GetViewportCenterPoint();


            MoveElementsToCenter(sheetCenterPoint, elementCenterPoint);


            //创建成功，计算右下角点坐标，便于后续使用
            RightInsertPoint = LeftInsertPoint.AddBy(new Point3dInCAD(suitableFrameBlock.Length / 2, suitableFrameBlock.Width / 2, 0));

            return frameBlockId;

        }


        /// <summary>
        /// 添加单个图框 图框指定，需要算出比例 同时会修改对象中的文字高度
        /// </summary>
        /// <returns>如果成功，返回图框对象的ObjectId，否则，返回ObjectId.Null</returns>
        public ObjectId AddSingleFrameBlockByFindingScale()
        {
            //返回值
            ObjectId frameBlockId = ObjectId.Null;


            if (LeftInsertPoint == null)
            {
                MessageBox.Show("请先设置图框插入点属性：LeftInsertPoint", "Tips");
                return frameBlockId;
            }

            if (ElementIds == null | ElementIds.Count == 0)
            {
                MessageBox.Show("请先设置图框内的对象属性：ElementIds", "Tips");
                return frameBlockId;
            }


            if (m_frameBlockLst.Count == 0)  //获取失败
            {
                MessageBox.Show("获取图框信息失败", "Tips");
                return frameBlockId;
            }




            if (FrameBlock == null)
            {
                MessageBox.Show("请先设置图框属性：FrameBlock", "Tips");
                return frameBlockId;
            }

            if (string.IsNullOrEmpty(FrameBlock.FrameName))
            {
                MessageBox.Show("请先设置图框的名称：FrameBlock.FrameName", "Tips");
                return frameBlockId;
            }

            //通过图框名称搜索
            var framNameAndFrameBlockMap = m_frameBlockLst.ToDictionary(x => x.FrameName, x => x);

            if (!framNameAndFrameBlockMap.TryGetValue(FrameBlock.FrameName, out var result)) //不存在
            {
                MessageBox.Show("找不到图框名称："+FrameBlock.FrameName, "Tips");
                return frameBlockId;
            }

            FrameBlock = result;



            //以下为根据图框算出比例，同时文字对象的高度会修改 一定会有一个值

            Scale = GetSheetScale();



            //插入图框
            frameBlockId = InsertFrameBlock();

            if (frameBlockId == ObjectId.Null) //创建失败
            {
                return frameBlockId;
            }




            //将所有对象移动到合适的位置，中心对中心

            //先获取图纸的中心  注意：考虑比例
            Point3dInCAD sheetCenterPoint = LeftInsertPoint + new Vector3d(FrameBlock.CenterXOffset, FrameBlock.CenterYOffset, 0) * Scale;



            //先计算能包住所有对象的中点坐标
            Point3dInCAD elementCenterPoint = GetViewportCenterPoint();


            MoveElementsToCenter(sheetCenterPoint, elementCenterPoint);


            //创建成功，计算右下角点坐标，便于后续使用
            RightInsertPoint = LeftInsertPoint.AddBy(new Point3dInCAD(FrameBlock.Length / 2, FrameBlock.Width / 2, 0)*Scale);

            return frameBlockId;

        }




        /// <summary>
        ///将所有对象移动到合适的位置，中心对中心
        /// </summary>
        /// <param name="sheetCenterPoint"></param>
        /// <param name="elementCenterPoint"></param>
        private void MoveElementsToCenter(Point3dInCAD sheetCenterPoint, Point3dInCAD elementCenterPoint)
        {
            Vector3d centerVector = sheetCenterPoint - elementCenterPoint;

            ObjectTool elementUtil = new ObjectTool(m_document);

            elementUtil.MoveEntities(ElementIds, centerVector);

        }






        /// <summary>
        /// 插入图框
        /// </summary>
        /// <returns>如果失败，返回ObjectId.Null</returns>
        private ObjectId InsertFrameBlock()
        {
            //返回值
            ObjectId elementId = ObjectId.Null;



            string blockName = FrameBlock.FrameName;


            BlockTool blockUtil = new BlockTool(m_document);

            //先判断块表是否存在
            if (blockUtil.IsBlockExist(blockName)) //已经存在
            {
                elementId = blockUtil.InsertBlockReference(blockName, LeftInsertPoint, Scale, SpaceName);
            }
            else //不存在，从外部插入
            {

                //先获取族
                //调试时的路径跟安装程序后的路径可能会不一样，需要专门处理

                //dll所在的文件夹路径
                string dllPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);


                //获取图框信息表
                string relativeFileName = @"\blocks\图框\" + blockName;


                //处理是否有后缀.dwg
                if (!relativeFileName.ToUpper().Contains(".DWG"))
                {
                    relativeFileName += ".dwg";
                }



                //先获取同一文件夹路径下
                string absoluteFileName = dllPath + relativeFileName;



                if (!File.Exists(absoluteFileName)) // 不存在，再上一层找
                {
                    //上一级路径
                    dllPath = Path.GetDirectoryName(dllPath);
                    absoluteFileName = dllPath + relativeFileName;


                    //找不到，直接返回
                    if (!File.Exists(absoluteFileName))
                    {
                        return elementId;
                    }
                }


                //以下为成功
                elementId = blockUtil.InsertExternalDwgFileAsBlockReference2(m_document.Database, absoluteFileName, LeftInsertPoint, blockName, Scale, SpaceName);
            }
            return elementId;

        }







        /// <summary>
        ///获取大小合适的图框
        /// </summary>
        /// <param name="maxLength"></param>
        /// <param name="maxWidth"></param>
        /// <returns>如果失败，返回null</returns>
        public  FrameBlock FindSuitableFrameBlock(double maxLength, double maxWidth)
        {
            //返回值
            FrameBlock frameBlock = null;

            //先保留高度合适的图框
            var expectFrameBlockLst = m_frameBlockLst.FindAll(x => x.ActuralWidth >= maxWidth);

            if (expectFrameBlockLst.Count == 0)
            {
                return frameBlock;
            }

            //先对图框按有效高度升序
            expectFrameBlockLst.Sort((x, y) => x.ActuralWidth.CompareTo(y.ActuralWidth));


            //将同一有效高度的放在一个list中，并按有效长度升序
            List<List<FrameBlock>> sortedFrameBlockLst = new List<List<FrameBlock>>();

            while (expectFrameBlockLst.Count > 0)
            {
                //至少有一个
                var tmpLst = expectFrameBlockLst.FindAll(x => x.ActuralWidth.AreEqual(expectFrameBlockLst[0].ActuralWidth));

                tmpLst.Sort((x, y) => x.ActuralLength.CompareTo(y.ActuralLength));
                sortedFrameBlockLst.Add(tmpLst);
                expectFrameBlockLst.RemoveAll(x => tmpLst.Contains(x));
            }



            //获取满足有效长度的图框

            foreach (var item in sortedFrameBlockLst)
            {
                if (maxWidth <= item[0].ActuralWidth)
                {
                    var expectedFrameBlock = item.Find(x => maxLength <= x.ActuralLength);

                    if (expectedFrameBlock != null) //存在
                    {
                        frameBlock = expectedFrameBlock;
                        break;
                    }
                }

            }

            return frameBlock;

        }



        /// <summary>
        ///获取图框信息列表
        /// </summary>
        /// <returns>如果失败，返回空的列表</returns>
        private List<FrameBlock> GetFrameBlockInformation()
        {
            //返回值
            List<FrameBlock> frameBlockLst = new List<FrameBlock>();

            //从外部文件读入

            //调试时的路径跟安装程序后的路径可能会不一样，需要专门处理

            //dll所在的文件夹路径
            string dllPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);


            //获取图框信息表
            string relativeFileName = @"\blocks\图框\图框信息.xls";


            //先获取同一文件夹路径下
            string absoluteFileName = dllPath + relativeFileName;



            if (!File.Exists(absoluteFileName)) // 不存在，再上一层找
            {
                //上一级路径
                dllPath = Path.GetDirectoryName(dllPath);
                absoluteFileName = dllPath + relativeFileName;


                //找不到，直接返回
                if (!File.Exists(absoluteFileName))
                {
                    return frameBlockLst;
                }
            }


            ExcelTool excelHelper = new ExcelTool(absoluteFileName);

            List<string[]> frameBlockInformationLst = excelHelper.ExcelToList(null, true);

            if (frameBlockInformationLst.Count == 0)  //获取失败
            {
                return frameBlockLst;
            }



            foreach (var item in frameBlockInformationLst)
            {
                if (item.Count() != 7)
                {
                    continue;
                }

                string blockName = item[0];

                if (!double.TryParse(item[1], out var length))
                {
                    continue;
                }

                if (!double.TryParse(item[2], out var width))
                {
                    continue;
                }


                if (!double.TryParse(item[3], out var acturalLength))
                {
                    continue;
                }

                if (!double.TryParse(item[4], out var acturalWidth))
                {
                    continue;
                }


                if (!double.TryParse(item[5], out var centerXOffset))
                {
                    continue;
                }

                if (!double.TryParse(item[6], out var centerYOffset))
                {
                    continue;
                }



                FrameBlock frameBlock = new FrameBlock
                {
                    FrameName = blockName,
                    Length = length,
                    Width = width,
                    ActuralLength = acturalLength,
                    ActuralWidth = acturalWidth,
                    CenterXOffset = centerXOffset,
                    CenterYOffset = centerYOffset
                };

                frameBlockLst.Add(frameBlock);

            }

            return frameBlockLst;

        }




        /// <summary>
        /// 设置特定的块参照的属性"日期"的宽度因为为0.8，使得其不超出框范围
        /// </summary>
        /// <param name="blockReferenceId"></param>
        public void SetAttibuteScaleForDate(ObjectId blockReferenceId)
        {
            BlockTool blockTool = new BlockTool(m_document);

            string blockName = blockTool.GetBlockReferenceName(blockReferenceId);

            if (string.IsNullOrEmpty(blockName))
            {
                return ;
            }

            List<string> blockNameLst = new List<string>
            {

                //长度都为20.15
                "a0横单校_17_3",
                "a0竖单校_17_3",
                "a01横单校_17_3",
                "A01竖单校_17_3",
                "a1横单校_17_3",
                "a1竖单校_17_3",
                "A02横单校_17_3",
                "a02竖单校_17_3",
                "A03横单校_17_3",
                "A04横单校_17_3",
                "a11横单校_17_3",
                "A11竖单校_17_3",
                "A12横单校_17_3",
                  "A12竖单校_17_3",
                "A13横单校_17_3",
                "A14横单校_17_3"
            };

            if (blockNameLst.Contains(blockName))
            {
                blockTool.ChangeBlockAttributeWidthFactor(blockReferenceId, "日期", 0.8);
            }

        }










        ///// <summary>
        /////先计算能包住所有视口的最大长度和宽度，便于选择合适的图纸（单位：mm） 9:38 2023/6/25  可以用
        ///// </summary>
        ///// <param name="maxLength">最大的长度（单位：mm），如果失败，返回double.NaN</param>
        ///// <param name="maxWidth">最大的宽度（单位：mm），如果失败，返回double.NaN</param>
        //private void GetSheetLengthAndWidth(out double maxLength, out double maxWidth)
        //{
        //    maxLength = double.NaN;
        //    maxWidth = double.NaN;


        //    double minXValue = double.MaxValue;
        //    double minYValue = double.MaxValue;

        //    double maxXValue = double.MinValue;
        //    double maxYValue = double.MinValue;

        //    ObjectTool objectTool = new ObjectTool(m_document);

        //    foreach (var item in ElementIds)
        //    {
        //        Point3dInCAD[] minPointAndMaxPoint = objectTool.GetEntityMinPointAndMaxPoint(item);
        //        if (minPointAndMaxPoint == null)
        //        {
        //            continue;
        //        }


        //        var minPoint = minPointAndMaxPoint[0];
        //        var maxPoint = minPointAndMaxPoint[1];


        //        minXValue = minXValue < minPoint.X ? minXValue : minPoint.X;

        //        minYValue = minYValue < minPoint.Y ? minYValue : minPoint.Y;

        //        maxXValue = maxXValue > maxPoint.X ? maxXValue : maxPoint.X;
        //        maxYValue = maxYValue > maxPoint.Y ? maxYValue : maxPoint.Y;

        //    }


        //    if (!minXValue.AreEqual(double.MaxValue) && !maxXValue.AreEqual(double.MinValue))
        //    {
        //        maxLength = maxXValue - minXValue;

        //    }

        //    if (!minYValue.AreEqual(double.MaxValue) && !maxYValue.AreEqual(double.MinValue))
        //    {
        //        maxWidth = maxYValue - minYValue;
        //    }

        //}














        /// <summary>
        ///先计算能包住所有视口最大长度和宽度的图纸比例
        /// </summary>
        private double GetSheetScale()
        {

            //可以选择的比例
            List<double> scaleLst = new List<double>
            {
                1,
                2,
                5,
                10,
                20,
                25,
                50,
                100,
                150,
                200
            };

            //先获取对象中的单行文字和多行文字，尺寸标注先不考虑
            List<ObjectId> dbTextIdLst = new List<ObjectId>();
            List<ObjectId> mTextIdLst = new List<ObjectId>();


            ObjectTool objectTool = new ObjectTool(m_document);
            foreach (var item in ElementIds)
            {

                var dBObject = objectTool.GetObject(item);
                if (dBObject is DBText)
                {
                    dbTextIdLst.Add(item);
                }

                else if (dBObject is MText)
                {
                    mTextIdLst.Add(item);
                }
            }


            foreach (var suitableScale in scaleLst)
            {
                //先修改文字的高度
                ChangeDBTextHeight(dbTextIdLst, suitableScale);
                ChangeMTextHeight(mTextIdLst, suitableScale);


                double minXValue = double.MaxValue;
                double minYValue = double.MaxValue;

                double maxXValue = double.MinValue;
                double maxYValue = double.MinValue;


                double maxLength, maxWidth;

                foreach (var item in ElementIds)
                {
                    Point3dInCAD[] minPointAndMaxPoint = objectTool.GetDBObjectMinPointAndMaxPoint(item);
                    if (minPointAndMaxPoint == null)
                    {
                        continue;
                    }


                    var minPoint = minPointAndMaxPoint[0];
                    var maxPoint = minPointAndMaxPoint[1];


                    minXValue = minXValue < minPoint.X ? minXValue : minPoint.X;

                    minYValue = minYValue < minPoint.Y ? minYValue : minPoint.Y;

                    maxXValue = maxXValue > maxPoint.X ? maxXValue : maxPoint.X;
                    maxYValue = maxYValue > maxPoint.Y ? maxYValue : maxPoint.Y;

                }


                if (!minXValue.AreEqual(double.MaxValue) && !maxXValue.AreEqual(double.MinValue))
                {
                    maxLength = maxXValue - minXValue;
                }
                else
                {
                    continue;
                }


                if (!minYValue.AreEqual(double.MaxValue) && !maxYValue.AreEqual(double.MinValue))
                {
                    maxWidth = maxYValue - minYValue;
                }
                else
                {
                    continue;
                }

                //以下为成功
                if (maxLength<=FrameBlock.ActuralLength*suitableScale && maxWidth<=FrameBlock.ActuralWidth * suitableScale)
                {
                    return suitableScale;
                }

            }

            //如果以上没有返回，则返回最后一个
            return scaleLst.LastOrDefault();

        }



        /// <summary>
        /// 修改单行文字的高度
        /// </summary>
        /// <param name="textIdLst"></param>
        /// <param name="scale"></param>
        private void ChangeDBTextHeight(List<ObjectId> textIdLst, double scale)
        {
            //修改的文字高度
            double textheight = TextHeight * scale;
            DBTextTool textTool = new DBTextTool(m_document);
            foreach (var item in textIdLst)
            {
                textTool.SetTextHeight(item, textheight);
            }
        }




        /// <summary>
        /// 修改多行文字的高度
        /// </summary>
        /// <param name="textIdLst"></param>
        /// <param name="scale"></param>
        private void ChangeMTextHeight(List<ObjectId> textIdLst, double scale)
        {
            //修改的文字高度
            double textheight = TextHeight * scale;
            MTextTool textTool = new MTextTool(m_document);
            foreach (var item in textIdLst)
            {
                textTool.SetTextHeight(item, textheight);
            }
        }



















        /// <summary>
        ///先计算能包住所有对象的中点坐标(单位：mm）
        /// </summary>
        /// <returns>如果失败，返回Point3dInCAD.Origin</returns>
        private Point3dInCAD GetViewportCenterPoint()
        {
            //返回值
            Point3dInCAD centerPoint = Point3dInCAD.Origin;

            ObjectTool objectTool = new ObjectTool(m_document);

            Point3dInCAD[] minPointAndMaxPoint = objectTool.GetDBObjectsMinPointAndMaxPoint(ElementIds);

            if (minPointAndMaxPoint != null)
            {
                Point3dInCAD minPoint = minPointAndMaxPoint[0];
                Point3dInCAD maxPoint = minPointAndMaxPoint[1];

                centerPoint = new Point3dInCAD((minPoint.X+maxPoint.X)/2, (minPoint.Y+maxPoint.Y)/2, 0);
            }

            return centerPoint;

        }



        ///// <summary>
        /////先计算能包住所有对象的中点坐标(单位：mm）  11:18 2023/6/25 可以用
        ///// </summary>
        ///// <returns>如果失败，返回Point3dInCAD.Origin</returns>
        //private Point3dInCAD GetViewportCenterPoint()
        //{
        //    //返回值
        //    Point3dInCAD centerPoint = Point3dInCAD.Origin;


        //    double minXValue = double.MaxValue;
        //    double minYValue = double.MaxValue;

        //    double maxXValue = double.MinValue;
        //    double maxYValue = double.MinValue;

        //    ObjectTool objectTool = new ObjectTool(m_document);

        //    foreach (var item in ElementIds)
        //    {
        //        Point3dInCAD[] minPointAndMaxPoint = objectTool.GetEntityMinPointAndMaxPoint(item);
        //        if (minPointAndMaxPoint == null)
        //        {
        //            continue;
        //        }


        //        var minPoint = minPointAndMaxPoint[0];
        //        var maxPoint = minPointAndMaxPoint[1];


        //        minXValue = minXValue < minPoint.X ? minXValue : minPoint.X;

        //        minYValue = minYValue < minPoint.Y ? minYValue : minPoint.Y;

        //        maxXValue = maxXValue > maxPoint.X ? maxXValue : maxPoint.X;
        //        maxYValue = maxYValue > maxPoint.Y ? maxYValue : maxPoint.Y;

        //    }


        //    if (!minXValue.AreEqual(double.MaxValue) &&
        //        !maxXValue.AreEqual(double.MinValue) &&
        //        !minYValue.AreEqual(double.MaxValue) &&
        //        !maxYValue.AreEqual(double.MinValue)
        //             )
        //    {
        //        double midXValue = (minXValue + maxXValue) / 2;
        //        double midYValue = (minYValue + maxYValue) / 2;

        //        centerPoint = new Point3dInCAD(midXValue, midYValue, 0);
        //    }

        //    return centerPoint;

        //}




        #endregion



        #region Helper Methods


        #endregion


        #region Properties



        /// <summary>
        /// 图框左下角插入点（单位：mm）
        /// </summary>
        public Point3dInCAD LeftInsertPoint { get; set; }




        /// <summary>
        /// 图框插入点（单位：mm）
        /// </summary>
        public Point3dInCAD InsertPoint { get; set; }




        /// <summary>
        /// 图框右下角插入点（单位：mm）
        /// </summary>
        public Point3dInCAD RightInsertPoint { get; set; }




        /// <summary>
        /// 一个图框内的对象列表
        /// </summary>
        public List<ObjectId> ElementIds { get; set; }



        /// <summary>
        /// 多个图框内的对象列表，以及对应的图框比例
        /// </summary>
        public List<Tuple<List<ObjectId>, double>> MultiElementIdAndScales { get; set; }







        /// <summary>
        /// 选择的图框对象
        /// </summary>
        public FrameBlock FrameBlock { get; set; }



        /// <summary>
        /// 图框的比例列表
        /// </summary>
        public double Scale { get; set; } = 1;



        /// <summary>
        /// 空间的名称
        /// </summary>
        public string SpaceName { get; set; } = null;











        /// <summary>
        /// 文字的指定比例，包括单行文字 多行文字 和尺寸标注
        /// </summary>
        public double TextHeight { get; set; } = 5;



        /// <summary>
        /// 两个相邻图框水平（x方向）间距（单位：mm）
        /// </summary>
        public double XTolerance { get; set; } = 100;



        /// <summary>
        /// 两个相邻图框垂直（y方向）间距（单位：mm）
        /// </summary>
        public double YTolerance { get; set; }





        #endregion




    }
}
