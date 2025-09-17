/**********************************************************
Copyright(C) UIH 2011                                     *
描    述：                                                *
版    本：4.0.30319.42000                                 *
作    者：莫瑞芳                                          *
创建时间：2021/9/29 14:33:55                              *
**********************************************************/

using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Text;

namespace Mrf.CSharp.BaseTools.Extension
{
    /// <summary>
    /// 字符串到公式扩展类
    /// </summary>
    public static class StringToFormulaExtension
    {

        /// <summary>
        /// string 转换成计算公式
        /// </summary>
        /// <param name="expression"></param>
        /// <returns>计算结果字符串,如果计算失败，返回""</returns>
        public static string Calculate(this string expression)
        {
            string className = "CalcQ";
            string methodName = "RunW";
            expression = expression.Replace("/ ", "*1.0/ ");

            //创建编译器实例。 
            ICodeCompiler complier = new CSharpCodeProvider().CreateCompiler();

            //设置编译参数。     
            CompilerParameters paras = new CompilerParameters();
            paras.GenerateExecutable = false;
            paras.GenerateInMemory = true;

            //创建动态代码。     
            StringBuilder classSource = new StringBuilder();
            classSource.Append("public     class     " + className + "\n ");
            classSource.Append("{\n ");
            classSource.Append("                 public     object     " + methodName + "()\n ");
            classSource.Append("                 {\n ");
            classSource.Append("                                 return     " + expression + ";\n ");
            classSource.Append("                 }\n ");
            classSource.Append("} ");

            //System.Diagnostics.Debug.WriteLine(classSource.ToString());     

            //编译代码。     
            CompilerResults result = complier.CompileAssemblyFromSource(paras, classSource.ToString());


            try
            {

                //获取编译后的程序集。     
                Assembly assembly = result.CompiledAssembly;

                //动态调用方法。     
                object eval = assembly.CreateInstance(className);
                MethodInfo method = eval.GetType().GetMethod(methodName);
                object reobj = method.Invoke(eval, null);
                GC.Collect();
                return reobj.ToString();
            }
            catch
            {
                return "";
            }

        }






        /// <summary>
        /// string 转换成计算公式
        /// </summary>
        /// <param name="expression"></param>
        /// <returns>计算结果字符串,如果计算失败，返回""</returns>
        public static string Calculate2(this string expression)
        {
            try
            {

                var result = new System.Data.DataTable().Compute(expression, "");
                return result.ToString();
            }
            catch
            {
                return "";
            }
        }





    }
}
