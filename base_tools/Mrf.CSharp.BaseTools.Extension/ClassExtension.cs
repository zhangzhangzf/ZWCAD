namespace Mrf.CSharp.BaseTools.Extension
{
    /// <summary>
    /// 一般类扩展
    /// </summary>
    public static class ClassExtension
    {
        /// <summary>
        /// 交换两个对象
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="firstClass">第一个对象</param>
        /// <param name="secondClass">第二个对象</param>
        public static void Swap<T>(ref T firstClass, ref T secondClass)
        {
            var tmp = firstClass;
            firstClass=secondClass;
            secondClass=tmp;
        }
    }
}
