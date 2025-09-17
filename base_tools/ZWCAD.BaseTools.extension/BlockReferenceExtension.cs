using ZwSoft.ZwCAD.DatabaseServices;


namespace ZWCAD.BaseTools.Extension
{
    /// <summary>
    /// 块参照扩展
    /// </summary>
    public static class BlockReferenceExtension
    {

        /// <summary>
        /// 获取文字属性的长度（单位：mm）
        /// </summary>
        /// <param name="blockReference">块参照</param>
        /// <param name="tagName">属性标记</param>
        /// <returns>文字属性的长度，如果不存在或失败，返回double.NaN</returns>
        public static double GetAttributeTextLength(this BlockReference blockReference, string tagName)
        {
            //返回值
            double length = double.NaN;

            foreach (ObjectId objectId in blockReference.AttributeCollection)
            {
                AttributeReference attrRef = objectId.GetObject(OpenMode.ForRead) as AttributeReference;

                if (attrRef.Tag == tagName)//为多行文字
                {
                    //if (attrRef.IsMTextAttribute)
                    //{
                    //    MText mText = attrRef.MTextAttribute;
                    //    length = mText.ActualWidth;
                    //    return length;
                    //}

                    try
                    {
                        Extents3d extents3d = attrRef.GeometricExtents;
                        if(extents3d != null)
                        {
                            length=extents3d.MaxPoint.X-extents3d.MinPoint.X;
                        }
                        return length;
                    }
                    catch
                    {

                    }

                }

            }

            return length;

        }





    }
}
