/*关于DllImport 的条件编译：
 * https://forums.autodesk.com/t5/net/conditional-dllimport/td-p/7934034
 * 
 * 关于多版本的条件编译：
 * https://www.keanw.com/2006/08/supporting_mult.html
 * 
 * 
 * 
 * 
 * 
 * 
 */


using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;
using ZwSoft.ZwCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using AcadApp = ZwSoft.ZwCAD.ApplicationServices.Application;

namespace ZWCAD.BaseTools
{


    /// <summary>
    /// 命令行帮助
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    public static class CommandLineHelper
    {


        private const string ACAD_EXE = "acad.exe";



        #region Result Type codes:前缀RT表示Result Type




        /// <summary>
        /// No result value
        /// </summary>
        private const short RTNONE = 5000;


        /// <summary>
        /// Real (floating-point) value
        /// </summary>
        private const short RTREAL = 5001;



        /// <summary>
        /// 2D point (X and Y; Z == 0.0
        /// </summary>
        private const short RTPOINT = 5002;


        /// <summary>
        /// Short (16-bit) integer
        /// </summary>
        private const short RTSHORT = 5003;


        /// <summary>
        /// Angle
        /// </summary>
        private const short RTANG = 5004;



        /// <summary>
        /// String
        /// </summary>
        private const short RTSTR = 5005;



        /// <summary>
        /// Entity name, 对应ObjectId
        /// </summary>
        private const short RTENAME = 5006;



        /// <summary>
        /// Selection set name
        /// </summary>
        private const short RTPICKS = 5007;



        /// <summary>
        /// Orientation
        /// </summary>
        private const short RTORINT = 5008;



        /// <summary>
        /// 3D point (X, Y, and Z)
        /// </summary>
        private const short RT3DPOINT = 5009;


        /// <summary>
        /// Long (32-bit) integer
        /// </summary>
        private const short RTLONG = 5010;



        /// <summary>
        /// Void (blank) symbol
        /// </summary>
        private const short RTVOID = 5014;



        /// <summary>
        /// List begin (for nested list)
        /// </summary>
        private const short RTLB = 5016;



        /// <summary>
        /// List end (for nested list)
        /// </summary>
        private const short RTLE = 5017;



        /// <summary>
        /// Dot (for dotted pair)
        /// </summary>
        private const short RTDOTE = 5018;



        /// <summary>
        /// AutoLISP nil
        /// </summary>
        private const short RTNIL = 5019;



        /// <summary>
        /// Group code zero for DXF lists (used only with acutBuildList())
        /// </summary>
        private const short RTDXFO = 5020;


        /// <summary>
        /// AutoLISP t (true)
        /// </summary>
        private const short RTT = 5021;


        /// <summary>
        /// Resbuf
        /// </summary>
        private const short RTRESBUF = 5023;


        /// <summary>
        /// Interrupted by modeless dialog
        /// </summary>
        private const short RTMODELESS = 5027;






        private const short RTNORM = 5100;



        #endregion




        private static Dictionary<Type, short> resTypes = new Dictionary<Type, short>();


        static CommandLineHelper()
        {
            resTypes[typeof(string)] = RTSTR;
            resTypes[typeof(double)] = RTREAL;
            resTypes[typeof(Point3d)] = RT3DPOINT;
            resTypes[typeof(ObjectId)] = RTENAME;
            resTypes[typeof(int)] = RTLONG;
            resTypes[typeof(short)] = RTSHORT;
            resTypes[typeof(Point2d)] = RTPOINT;
        }

        private static TypedValue TypedValueFromObject(object val)
        {
            if (val == null) throw new ArgumentException("null not permitted as command argument");
            short code = -1;

            if (resTypes.TryGetValue(val.GetType(), out code) && code > 0)
            {
                return new TypedValue(code, val);
            }
            throw new InvalidOperationException("Unsupported type in Command() method");
        }

        public static int Command(params object[] args)
        {
            if (AcadApp.DocumentManager.IsApplicationContext) throw new InvalidCastException("Invalid execution context");



            int stat = 0;



            int cnt = 0;
            using (ResultBuffer buffer = new ResultBuffer())
            {
                foreach (object o in args)
                {
                    buffer.Add(TypedValueFromObject(o));
                    ++cnt;
                }
                if (cnt > 0)
                {


                    ////获取cad版本
                    ////如2012 major=18,minor=2
                    //var version = AcadApp.Version;
                    //var major=version.Major;
                    //var minor=version.Minor;

#if autoCAD2018 || autoCAD2020 || autoCAD2022
                    stat = acedCmdS(buffer.UnmanagedObject);
#else


                    //#elif  autoCAD2012

                    stat = acedCmd(buffer.UnmanagedObject);

                    //MessageBox.Show(stat.ToString(), "Tips");

#endif


                }
            }
            return stat;
        }















        //2013及以上 应该都是"accore.dll"
#if autoCAD2018 || autoCAD2020 || autoCAD2022

        [SuppressUnmanagedCodeSecurity]
        //调用AutoCAD命令，ARX原型：int acedCmdS(const struct resbuf * rbp);
        [DllImport("accore.dll", EntryPoint = "acedCmdS", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        private extern static int acedCmdS(IntPtr rbp);




#else
        //#elif autoCAD2012
        [SuppressUnmanagedCodeSecurity]
        [DllImport("acad.exe", EntryPoint = "acedCmd", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Auto)]
        private static extern int acedCmd(IntPtr resbuf);

#endif



        //        //2013及以上 应该都是"accore.dll"
        //#if autoCAD2022
        //        const string acdbxx_dll = "accore.dll";
        //#else
        //        const string acdbxx_dll = "acad.exe";
        //#endif

        //        //[DllImport("acad.exe", CallingConvention = CallingConvention.Cdecl, EntryPoint = "?acedSetCurrentVPort@@YA?AW4ErrorStatus@Acad@@PBVAcDbViewport@@@Z")]
        //        //extern static private int acedCmd2013(IntPtr acDbVport);

        //        //[DllImport("acad.exe", CallingConvention = CallingConvention.Cdecl, EntryPoint = "?acedSetCurrentVPort@@YA?AW4ErrorStatus@Acad@@PBVAcDbViewport@@@Z")]
        //        //extern static private int acedCmd2008(IntPtr acDbVport);

        //        //[DllImport("acad.exe", CallingConvention = CallingConvention.Cdecl, EntryPoint = "?acedSetCurrentVPort@@YA?AW4ErrorStatus@Acad@@PEBVAcDbViewport@@@Z")]
        //        //extern static int acedCmd2011(IntPtr resbuf);


        //        [SuppressUnmanagedCodeSecurity]
        //        [DllImport(acdbxx_dll, EntryPoint = "acedCmd", CallingConvention = CallingConvention.Cdecl,
        //            CharSet = CharSet.Auto)]
        //        private static extern int acedCmd(IntPtr resbuf);
        //        //private static extern int acedCmd2012(IntPtr resbuf);


















        public static void ExecuteStringOverInvoke(string command)
        {
            try
            {
                object activeDocument = null;

                //#if autoCAD2012
                //                activeDocument =
                //                ZwSoft.ZwCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.AcadDocument;
                //#elif autoCAD2018 || autoCAD2020 || autoCAD2022
                //                activeDocument = ZwSoft.ZwCAD.ApplicationServices.DocumentExtension.GetAcadDocument(
                //                                  AcadApp.DocumentManager.MdiActiveDocument);
                //#endif


                //中望CAD中没有对应的函数
                //activeDocument = ZwSoft.ZwCAD.ApplicationServices.DocumentExtension.GetAcadDocument(AcadApp.DocumentManager.MdiActiveDocument);

                object[] data = { command };
                activeDocument.GetType()
                              .InvokeMember(
                                  "SendCommand", BindingFlags.InvokeMethod, null, activeDocument, data);
            }
            catch (ZwSoft.ZwCAD.Runtime.Exception exception)
            {
                //Logger.Error("Command line class error.", exception);
            }
        }

        static MethodInfo runCommand = typeof(Editor).GetMethod(
            "RunCommand", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        public static PromptStatus Command(this Editor ed, params object[] args)
        {
            if (AcadApp.DocumentManager.IsApplicationContext)
            {
                throw new InvalidOperationException("Invalid execution context for Command()");
            }
            if (ed.Document != AcadApp.DocumentManager.MdiActiveDocument)
            {
                throw new InvalidOperationException("Document is not active");
            }
            return (PromptStatus)runCommand.Invoke(ed, new object[] { args });
        }
    }

    public class CommandLineShortCuts
    {
        // Sample member functions that use the Command() method.

        public static int ZoomExtents()
        {
            return CommandLineHelper.Command("._ZOOM", "_E");
        }

        public static int ZoomCenter(Point3d center, double height)
        {
            return CommandLineHelper.Command("._ZOOM", "_C", center, height);
        }

        public static int ZoomWindow(Point3d corner1, Point3d corner2)
        {
            return CommandLineHelper.Command("._ZOOM", "_W", corner1, corner2);
        }

        public static int SetFilletRadius(double filletRadius)
        {
            return CommandLineHelper.Command("._FILLET", "_R", filletRadius);
        }

        public static int FilletPolyline(ObjectId polylineId)
        {
            return CommandLineHelper.Command("._FILLET", "_P", polylineId);
        }
        public static int Copy(ObjectId polylineId, Point3d originPoint, Point3d targetPoint)
        {
            return CommandLineHelper.Command("._COPY", polylineId, "", originPoint, targetPoint);
        }

        public static int Hatch(ObjectId objectId)
        {
            return CommandLineHelper.Command("._HATCH", "_S", objectId, "");
        }

        public static int Hatch(List<ObjectId> objectIdLst, string patternName = "ANSI31", double scale = 1, double angle = 0)
        {
            CommandLineHelper.Command("._HATCH", patternName, scale, angle, "_S");
            foreach (var item in objectIdLst)
            {
                CommandLineHelper.Command(item);
            }
            CommandLineHelper.Command("");
            return 1;
        }

        /// <summary>
        /// 创建多段线，第一个点和第二个点之间为指定半径的圆弧，其它点为线段  经过测试，没有成功
        /// </summary>
        /// <param name="firstPoint">第一个点</param>
        /// <param name="radius">圆弧的半径</param>
        /// <param name="secondPoint">第二个点</param>
        /// <param name="pointLst">其它点列表</param>
        /// <returns></returns>
        public static int Polyline(Point3d firstPoint, double radius, Point3d secondPoint, List<Point3d> pointLst)
        {
            CommandLineHelper.Command("._PLINE", firstPoint, "_A", "_R", radius, secondPoint, "_L");
            foreach (var item in pointLst)
            {
                CommandLineHelper.Command(item);
            }
            return CommandLineHelper.Command("_C");
        }
    }
}
