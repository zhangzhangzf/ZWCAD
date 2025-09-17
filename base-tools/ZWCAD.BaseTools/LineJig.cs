using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;
using ZwSoft.ZwCAD.Geometry;
using System;


namespace ZWCAD.BaseTools
{
    /// <summary>
    /// 实体对象实时显示工具
    /// </summary>
    public class LineJig : EntityJig
    {



        #region Private Variables


        /// <summary>
        /// 直线的起点
        /// </summary>
        private Point3d m_jStartPoint;

        /// <summary>
        /// 直线的终点
        /// </summary>
        private Point3d m_jEndPoint;

        /// <summary>
        /// 提示信息
        /// </summary>
        private string m_jPrompt;


        /// <summary>
        /// 交互关键字
        /// </summary>
        private string[] m_jKeywords;

        #endregion



        #region Default Constructor


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="startPoint">起点</param>
        /// <param name="prompt">提示信息</param>
        /// <param name="keywords">交互关键字</param>
        public LineJig(Point3d startPoint, string prompt, string[] keywords) : base(new Line())
        {

            m_jStartPoint = startPoint;
            m_jPrompt = prompt;
            m_jKeywords = keywords;

            ((Line)Entity).StartPoint = m_jStartPoint;

        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="prompts"></param>
        /// <returns></returns>
        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            //声明提示信息类
            JigPromptPointOptions options = new JigPromptPointOptions(m_jPrompt);

            //添加关键字
            foreach (var item in m_jKeywords)
            {
                options.Keywords.Add(item);
            }

            //不将关键字信息添加到提示信息中
            options.AppendKeywordsToMessage = false;

            //添加空格键

            char space = (char)32;
            options.Keywords.Add(space.ToString());

            //设置获取的信息类型
            options.UserInputControls = UserInputControls.Accept3dCoordinates;
            PromptPointResult pr = prompts.AcquirePoint(options);

            m_jEndPoint = pr.Value;
            return SamplerStatus.NoChange;

        }




        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override bool Update()
        {
            ((Line)Entity).EndPoint = m_jEndPoint;
            return true;
        }


        /// <summary>
        /// 返回图形对象
        /// </summary>
        /// <returns></returns>
        public Entity GetEntity()
        {
            return Entity;
        }
        #endregion




        #region CommandMethods


        #endregion



        #region Helper Methods


        #endregion


        #region Properties


        #endregion




    }
}
