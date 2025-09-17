using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using Mrf.CSharp.BaseTools;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ZWCAD.BaseTools.Extension;
using Point3d = ZwSoft.ZwCAD.Geometry.Point3d;

namespace ZWCAD.BaseTools
{
    /// <summary>
    /// 组工具
    /// </summary>
    public class GroupTool
    {

        Document m_document;

        Database m_database;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="document">文档对象</param>
        public GroupTool(Document document)
        {
            m_document = document;
            m_database = m_document.Database;
        }


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="database">数据库</param>
        public GroupTool(Database database)
        {
            m_database = database;
        }



        /// <summary>
        /// 创建组,判断组名是否唯一，是否合法
        /// </summary>
        /// <param name="objectIdCollection">组成组的对象的ObjectId集合</param>
        /// <param name="groupName">组名称</param>
        /// <param name="isRename">如果组名已经存在，是否按默认规则重命名</param>
        /// <returns>组的ObjectId，如果失败，返回ObjectId.Null</returns>
        public ObjectId CreateGroup(ObjectIdCollection objectIdCollection, string groupName = "MyGroup", bool isRename = true)
        {

            //返回值
            ObjectId groupId = ObjectId.Null;

            Database database = m_database;

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                Group group = new Group();
                group.Append(objectIdCollection);

                DBDictionary groupDic = transaction.GetObject(database.GroupDictionaryId, OpenMode.ForWrite) as DBDictionary;

                //先验证组名

                string newGroupName = groupName;
                while (groupDic.Contains(newGroupName))
                {

                    if (isRename) //因为组名是唯一的，如果可以重命名
                    {
                        newGroupName = groupName + TimeTool.GetCurrentTimeByFormat();
                    }
                    else
                    {
                        MessageBox.Show("组名: " + newGroupName + " 已经存在", "Tips");
                        return groupId;
                    }

                }

                //没有返回，说明组名唯一
                groupName = newGroupName;

                try
                {
                    SymbolUtilityServices.ValidateSymbolName(groupName, false);
                }

                catch
                {
                    MessageBox.Show("无效的组名: " + groupName, "Tips");
                    return groupId;
                }


                //以上没有返回 说明组名合法

                try
                {
                    group.Selectable = true;
                    groupId = groupDic.SetAt(groupName, group);

                    transaction.AddNewlyCreatedDBObject(group, true);

                    transaction.Commit();

                }

                catch (Exception ex)
                {
                    transaction.Abort();
                    groupId = ObjectId.Null;
                }

                return groupId;

            }
        }



        /// <summary>
        /// 创建组,判断组名是否唯一，是否合法
        /// </summary>
        /// <param name="objectIds">组成组的对象的ObjectId列表</param>
        /// <param name="groupName">组名称</param>
        /// <param name="isRename">如果组名已经存在，是否按默认规则重命名</param>
        /// <returns>组的ObjectId，如果失败，返回ObjectId.Null</returns>
        public ObjectId CreateGroup(List<ObjectId> objectIds, string groupName = "MyGroup", bool isRename = true)
        {
            ObjectId objectId = ObjectId.Null;
            if (objectIds == null || objectIds.Count == 0)
            {
                return objectId;
            }


            ObjectIdCollection objectIdCollection = new ObjectIdCollection(objectIds.ToArray());



            //foreach (ObjectId objectId in objectIds)
            //{
            //    objectIdCollection.Add(objectId);
            //}

            return CreateGroup(objectIdCollection, groupName, isRename);

        }


        /// <summary>
        /// 创建组
        /// </summary>
        /// <param name="objectIds">组成组的对象的ObjectId数组</param>
        /// <param name="groupName">组名称</param>
        /// <returns>组的ObjectId</returns>
        public ObjectId CreateGroup(ObjectId[] objectIds, string groupName = "MyGroup")
        {
            ObjectIdCollection objectIdCollection = new ObjectIdCollection(objectIds);
            return CreateGroup(objectIdCollection, groupName);

        }



        public void UnGroup(ObjectId groupId)
        {

            Database database = m_database;

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                Group group = transaction.GetObject(groupId, OpenMode.ForWrite) as Group;

                ObjectId[] objectIds = group.GetAllEntityIds();

                foreach (ObjectId objectId in objectIds)
                {

                    group.Remove(objectId);
                }

                transaction.Commit();
            }
        }




        /// <summary>
        /// 将组里面的对象分解，并删除组对象
        /// </summary>
        /// <param name="groupId">组对象的ObjectId</param>
        public void UnGroupAndDelete(ObjectId groupId)
        {
            //先把对象从组中分解出来
            UnGroup(groupId);



            //然后将组对象删除
            ObjectTool objectTool;
            if (m_database != null)
            {
                objectTool = new ObjectTool(m_database);
            }

            else
            {
                objectTool = new ObjectTool(m_document);
            }


            objectTool.EraseDBObject(groupId);
        }




        /// <summary>
        /// 获取组边界的最小点和最大点
        /// </summary>
        /// <param name="groupId">组的ObjectId</param>
        /// <returns>最小点和最大点组成的数组Point3d[]</returns>
        public Point3d[] GetGroupBoundingBoxPoint(ObjectId groupId)
        {
            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                Group group = transaction.GetObject(groupId, OpenMode.ForRead) as Group;
                Extents3d extents3D = transaction.GetExtents(group.GetAllEntityIds());

                Point3d minPoint = extents3D.MinPoint;
                Point3d maxPoint = extents3D.MaxPoint;
                return new Point3d[] { minPoint, maxPoint };
            }
        }




        /// <summary>
        /// 获取实体边界的最小点
        /// </summary>
        /// <param name="groupId">组的ObjectId</param>
        /// <returns>最小点坐标Point3d</returns>
        public Point3d GetGroupMinPoint(ObjectId groupId)
        {
            Point3d[] minPointAndMaxPoint = GetGroupBoundingBoxPoint(groupId);
            return minPointAndMaxPoint[0];
        }



        /// <summary>
        /// 获取实体边界的最大点
        /// </summary>
        /// <param name="groupId">组的ObjectId</param>
        /// <returns>最大点坐标Point3d</returns>
        public Point3d GetGroupMaxPoint(ObjectId groupId)
        {
            Point3d[] minPointAndMaxPoint = GetGroupBoundingBoxPoint(groupId);
            return minPointAndMaxPoint[1];
        }




        /// <summary>
        /// 通过组内的子对象的ObjectId获得组的ObjectId
        /// https://adndevblog.typepad.com/autocad/2012/04/how-to-detect-whether-entity-is-belong-to-any-group-or-not.html
        /// </summary>
        /// <param name="subEntityId">对象的ObjectId</param>
        /// <returns>如果对象不在组内，返回ObjectId.Null</returns>
        public ObjectId GetGroupBySubEntityId(ObjectId subEntityId)
        {
            //返回值
            ObjectId groupId = ObjectId.Null;

            using (Transaction transaction = m_database.TransactionManager.StartTransaction())
            {
                try
                {
                    DBObject dBObject = transaction.GetObject(subEntityId, OpenMode.ForRead);
                    if (dBObject is Entity ent)
                    {
                        ObjectIdCollection ids = ent.GetPersistentReactorIds();

                        foreach (ObjectId id in ids)
                        {
                            DBObject obj = transaction.GetObject(id, OpenMode.ForRead);
                            if (obj is Group group)
                            {
                                groupId = group.ObjectId;
                                break;
                            }
                        }

                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();

                }

            }

            return groupId;
        }



        /// <summary>
        /// 获取组的名称
        /// </summary>
        /// <param name="groupId">组的ObjectId</param>
        /// <returns>组的名称，如果失败，返回null</returns>
        public string GetGroupName(ObjectId groupId)
        {
            string groupName = null;

            if (groupId.IsNull)
            {
                return groupName;
            }



            using (Transaction transaction = m_document.TransactionManager.StartTransaction())
            {
                try
                {
                    if (transaction.GetObject(groupId, OpenMode.ForRead) is Group group)
                    {
                        groupName=group.Name;
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Abort();

                }

                return groupName;

            }

        }
    }
}
