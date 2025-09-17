using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.Geometry;

/// <summary>
/// 多重引线对象工具
/// </summary>
namespace ZWCAD.BaseTools
{
    /// <summary>
    /// 
    /// </summary>
    public class MLeaderTool
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
        public MLeaderTool(Document document)
        {
            m_document = document;
            m_database = m_document.Database;
        }


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="database">图形数据库</param>
        public MLeaderTool(Database database)
        {
            m_database = database;
        }


        #endregion


        /// <summary>
        /// 添加多重引线实例
        /// </summary>
        /// <param name="spaceId">空间对象的ObjectId</param>
        /// <param name="startPoint">引线的起点</param>
        /// <param name="lastPoint">引线的终点</param>
        /// <param name="txt">文字内容</param>
        /// <param name="txtHeight">文字高度</param>
        /// <param name="textStyleId">文字样式的ObjectId</param>
        /// <param name="mLeaderStyleId">多重引线样式的ObjectId</param>
        /// <returns>多重引线实例的ObjectId，如果失败，返回ObjectId.Null</returns>
        public ObjectId AddMLeader(ObjectId spaceId, Point3d startPoint, Point3d lastPoint, string txt, double txtHeight, ObjectId textStyleId, ObjectId mLeaderStyleId)
        {
            //返回值
            ObjectId mLeaderId = ObjectId.Null;

            if (spaceId.IsNull  || textStyleId.IsNull || mLeaderStyleId.IsNull)
            {
                return mLeaderId;
            }


            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {

                try
                {
                    if (transaction.GetObject(spaceId, OpenMode.ForWrite) is BlockTableRecord blkRef)
                    {
                        MLeader leader = new MLeader();
                        leader.SetDatabaseDefaults();
                        leader.MLeaderStyle = mLeaderStyleId;
                        var sn = leader.AddLeaderLine(startPoint);
                        leader.AddFirstVertex(sn, startPoint);
                        leader.SetFirstVertex(sn, startPoint);
                        leader.SetLastVertex(sn, lastPoint);

                        MText mText = new MText
                        {
                            Contents = txt,
                            TextHeight = txtHeight,
                            TextStyleId = textStyleId,
                            Location = lastPoint
                        };





                        leader.MText = mText;



                        mLeaderId=  blkRef.AppendEntity(leader);

                        transaction.AddNewlyCreatedDBObject(leader, true);

                        //如果在这里修改leader，会报错：AccessViolationException  可能是因为在这里leader只是只读模式
                        //改为到后面再修改
                        //leader.MoveMLeader(-Vector3d.XAxis, MoveType.MoveAllExceptArrowHeaderPoints);
                        //leader.MoveMLeader(Vector3d.XAxis, MoveType.MoveAllExceptArrowHeaderPoints);

                    }
                    transaction.Commit();
                }

                catch
                {
                    transaction.Abort();
                }

            }


            //因为以上的程序，生成的引线不会自动更新，也就是文字不会自适应到合适的位置，
            //只能通过移动、再移动回原来的位置，从而实现自动更新的目的

            RenewMleader(mLeaderId);

            //这个不管用 不需要
            //m_document.Editor.Regen();
            return mLeaderId;
        }



        /// <summary>
        /// 设置多重引线对象的文字对齐方式
        /// </summary>
        /// <param name="mLeaderId">空间对象的ObjectId</param>
        /// <param name="textAlignmentType"></param>
        /// <returns>如果成功，返回true，否则，返回false</returns>
        public bool SetTextAlignmentType(ObjectId mLeaderId,TextAlignmentType textAlignmentType=TextAlignmentType.LeftAlignment)
        {
            //返回值
            bool isSucceed = false;

            if (mLeaderId.IsNull )
            {
                return isSucceed;
            }


            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {

                try
                {
                    if (transaction.GetObject(mLeaderId, OpenMode.ForWrite) is MLeader mLeader)
                    {
                        mLeader.TextAlignmentType=textAlignmentType;
                        isSucceed=true;
                    }
                    transaction.Commit();
                }

                catch
                {
                    transaction.Abort();
                }

            }

            return isSucceed;
        }

        
        /// <summary>
        /// 设置多重引线对象的文字对齐方式
        /// </summary>
        /// <param name="mLeaderId">空间对象的ObjectId</param>
        /// <returns>如果成功，返回true，否则，返回false</returns>
        public bool RenewMleader(ObjectId mLeaderId)
        {
            //返回值
            bool isSucceed = false;

            if (mLeaderId.IsNull )
            {
                return isSucceed;
            }


            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {

                try
                {
                    if (transaction.GetObject(mLeaderId, OpenMode.ForWrite) is MLeader mLeader)
                    {
                        mLeader.MoveMLeader(-Vector3d.XAxis,MoveType.MoveContentAndDoglegPoints);
                        mLeader.MoveMLeader(Vector3d.XAxis,MoveType.MoveContentAndDoglegPoints);
                        // mLeader.MoveMLeader(-Vector3d.XAxis,MoveType.MoveAllExceptArrowHeaderPoints);
                        //mLeader.MoveMLeader(Vector3d.XAxis,MoveType.MoveAllExceptArrowHeaderPoints);
                        
                        
                        isSucceed=true;
                    }
                    transaction.Commit();
                }

                catch
                {
                    transaction.Abort();
                }

            }

            return isSucceed;
        }







        /// <summary>
        /// 添加多重引线实例
        /// </summary>
        /// <param name="startPoint">引线的起点</param>
        /// <param name="lastPoint">引线的终点</param>
        /// <param name="txt">文字内容</param>
        /// <param name="txtHeight">文字高度</param>
        /// <param name="textStyleId">文字样式的ObjectId</param>
        /// <param name="mLeaderStyleId">多重引线样式的ObjectId</param>
        /// <param name="spaceName">模型空间或图纸空间，如果为"PAPERSPACE"（不分大小写），为图纸空间，如果指定的名称为其它的图纸空间名称，则为指定的图纸空间名称；为空或其它，为模型空间</param>
        /// <returns>多重引线实例的ObjectId，如果失败，返回ObjectId.Null</returns>
        public ObjectId AddMLeader(Point3d startPoint, Point3d lastPoint, string txt, double txtHeight, ObjectId textStyleId, ObjectId mLeaderStyleId, string spaceName = "MODELSPACE")
        {
            //返回值
            ObjectId mLeaderId = ObjectId.Null;

            //先获取空间
            SpaceTool spaceTool = new SpaceTool(m_database);
            ObjectId spaceId = spaceTool.GetSpaceByName(spaceName);

            if (spaceId.IsNull)
            {
                return mLeaderId;
            }


            mLeaderId= AddMLeader(spaceId, startPoint, lastPoint, txt, txtHeight, textStyleId, mLeaderStyleId);

            return mLeaderId;

        }







    }
}
