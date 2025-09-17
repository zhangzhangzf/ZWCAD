/*
AutoCAD .NET: Detect Current Space (Model or Paper) Layout and Viewport
https://spiderinnet1.typepad.com/blog/2014/05/autocad-net-detect-current-space-model-or-paper-and-viewport.html
*/


using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;
using Mrf.CSharp.BaseTools;
using Mrf.CSharp.BaseTools.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace ZWCAD.BaseTools.Extension
{
    /// <summary>
    /// 空间（模型空间或图纸空间）的扩展
    /// </summary>
    public static class SpaceExtension
    {


        /// <summary>
        /// 获取是在模型空间中（返回0）、在图纸空间中，浮动视口没有被激活（返回1）
        /// 还是在图纸空间中，浮动视口被激活（返回值大于等于2，可能或当前视口的ViewPort.Number）
        /// </summary>
        /// <param name="database">数据库</param>
        /// <returns></returns>
        public static int InModelSpaceOrPaperSpace(this Database database)
        {
            //返回值
            int result;


            //不能这样转换为int 会报错
            //var tileMode = (int)Application.GetSystemVariable("TILEMODE");
            //var cvPort = (int)Application.GetSystemVariable("cvPort");

            var tileMode = Application.GetSystemVariable("TILEMODE").ToString();
            var cvPort = Application.GetSystemVariable("cvPort").ToString();

            if (tileMode == "1") //为在模型空间的模型空间中
            {
                result = 0;
            }
            else if (tileMode == "0" && cvPort == "1") //在图纸空间中，模型空间没有被激活
            {
                result = 1;
            }
            else
            {
                result = 2;

                if (int.TryParse(cvPort, out int cvPortInt))
                {

                    result = cvPortInt;
                    if (result < 2)
                    {
                        result = 2; //确保跟前面有区别
                    }

                }

            }

            return result;

        }




        /// <summary>
        /// 获取是在模型空间中（返回0）、在图纸空间中，浮动视口没有被激活（返回1） 
        /// 还是在图纸空间中，浮动视口被激活（返回值大于等于2，可能或当前视口的ViewPort.Number）
        /// </summary>
        /// <param name="document">文档对象</param>
        /// <returns></returns>
        public static int InModelSpaceOrPaperSpace(this Document document)
        {
            return document.Database.InModelSpaceOrPaperSpace();

        }






        /// <summary>
        /// 切换到模型空间中（输入0）、 图纸空间中，浮动视口没有被激活（输入1）,图纸空间中 浮动视口被激活(其他值,可为视口的Viewport.Number，将会设置该视口为当前激活视口）
        /// </summary>
        /// <param name="document">文档对象</param>
        /// <param name="spaceIndex">空间索引值</param>
        public static void SwitchToSpace(this Document document, int spaceIndex = 0)
        {
            //先判断当前是在模型空间中，图纸空间（没激活浮动视口）还是图纸空间中，激活活动视口

            Editor editor = document.Editor;

            string tileMode = Application.GetSystemVariable("TILEMODE").ToString();
            string cvPort = Application.GetSystemVariable("cvPort").ToString();

            switch (spaceIndex)
            {
                case 0: //模型空间中
                    if (tileMode != "1") //不在模型空间中，需要切换
                    {
                        Application.SetSystemVariable("TILEMODE", 1);
                    }
                    break;

                case 1: //图纸空间中，没有激活浮动视口 没有用系统变量控制，因为"CVPORT"如果输入值不当，可能会报错

                    if (tileMode == "1") //在模型空间中，需要切换，会自动切换到图纸空间状态
                    {
                        Application.SetSystemVariable("TILEMODE", 0);

                        //如果当前空间在模型空间中，但是图纸空间有激活的浮动视口，上一步结束之后，
                        //或进入到图纸空间的激活的浮动视口状态，需要切换一下

                        //需要再获取一下
                        cvPort = Application.GetSystemVariable("cvPort").ToString();
                        if (cvPort != "1")
                        {
                            editor.SwitchToPaperSpace();
                        }
                    }
                    else if (tileMode == "0" && cvPort != "1") //在图纸空间激活的浮动视口状态,需要切换
                    {
                        editor.SwitchToPaperSpace();
                    }

                    break;

                default: //图纸空间的激活的浮动视口中


                    if (tileMode == "1") //在模型空间中，需要先切换到图纸空间
                    {
                        Application.SetSystemVariable("TILEMODE", 0);

                    }

                    //再在图纸空间中激活模型空间
                    editor.SwitchToModelSpace();

                    //然后设置当前视口
                    //Application.SetSystemVariable("TILEMODE", 0);
                    Application.SetSystemVariable("CVPORT", spaceIndex); //这个应该是默认激活图纸空间当前视口中
                    break;

            }

        }






        /// <summary>
        /// 是否在模型空间 分为三种，在模型空间；在图纸空间，但是视口没有激活；在图纸空间，但是视口被激活
        /// </summary>
        /// <param name="document">文档对象</param>
        /// <returns>如果是，返回true，否则，返回false</returns>
        public static bool IsInModel(this Document document )
        {
            if (document.Database.TileMode)
                return true;
            else
                return false;
        }



        /// <summary>
        /// 是否在图纸空间 分为三种，在模型空间；在图纸空间，但是视口没有激活；在图纸空间，但是视口被激活
        /// </summary>
        /// <param name="document">文档对象</param>
        /// <returns>如果是，返回true，否则，返回false</returns>
        public static bool IsInLayout(this Document document)
        {
            return !document.IsInModel();
        }


        /// <summary>
        /// 是否在图纸空间，但是视口没有激活 分为三种，在模型空间；在图纸空间，但是视口没有激活；在图纸空间，但是视口被激活
        /// </summary>
        /// <param name="document">文档对象</param>
        /// <returns>如果是，返回true，否则，返回false</returns>
        public static bool IsInLayoutPaper(this Document document)
        {
            Database db = document.Database;
            Editor ed = document.Editor;

            if (db.TileMode)
                return false;
            else
            {
                if (db.PaperSpaceVportId == ObjectId.Null)
                    return false;
                else if (ed.CurrentViewportObjectId == ObjectId.Null)
                    return false;
                else if (ed.CurrentViewportObjectId == db.PaperSpaceVportId)
                    return true;
                else
                    return false;
            }
        }



        /// <summary>
        /// 是否在图纸空间，但是视口被激活 分为三种，在模型空间；在图纸空间，但是视口没有激活；在图纸空间，但是视口被激活
        /// </summary>
        /// <param name="document">文档对象</param>
        /// <returns>如果是，返回true，否则，返回false</returns>
        public static bool IsInLayoutViewport(this Document document)
        {
            return document. IsInLayout() && !document.IsInLayoutPaper();
        }



    }

}
