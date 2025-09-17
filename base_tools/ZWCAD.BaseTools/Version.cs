using ZwSoft.ZwCAD.DatabaseServices;
using System.Text.RegularExpressions;


namespace ZWCAD.BaseTools
{
    /// <summary>
    /// 版本
    /// </summary>
    public class Version
    {

        /// <summary>
        /// 获取版本信息
        /// </summary>
        /// <param name="infoType">哪种版本信息，大小写不敏感，如为"release"，则返回时间，如"2020"；
        /// 如为"productId"，则返回产品类型，如"AutoCAD"、"Autodesk Civil 3d"等；如为"localeId",则为"English"、"Simplified Chinese" </param>
        /// <returns></returns>
        public static string GetAcadVersionInfo(string infoType)
        {
            // credit Gile: https://forums.autodesk.com/t5/net/which-autocad-vertical-am-i-using/m-p/6861378#M51975

//#if autoCAD2012
//            //var productKey = HostApplicationServices.Current.RegistryProductRootKey;
//            //var productKey = HostApplicationServices.Current.MachineRegistryProductRootKey;
//            var productKey = HostApplicationServices.Current.UserRegistryProductRootKey;
//#else
            var productKey = HostApplicationServices.Current.UserRegistryProductRootKey;
//#endif


            var groups = Regex.Match(productKey, @"ACAD-([0-9A-F])\d(\d{2}):([0-9A-F]{3})").Groups;
            string release, localeId, productId;
            switch (groups[1].Value)
            {
                case "5": release = "2007"; break;
                case "6": release = "2008"; break;
                case "7": release = "2009"; break;
                case "8": release = "2010"; break;
                case "9": release = "2011"; break;
                case "A": release = "2012"; break;
                case "B": release = "2013"; break;
                case "D": release = "2014"; break;
                case "E": release = "2015"; break;
                case "F": release = "2016"; break;
                case "0": release = "2017"; break;
                case "1": release = "2018"; break;
                case "2": release = "2019"; break;
                case "3": release = "2020"; break;
                case "4": release = "2021"; break;
                default: release = "unknown"; break;
            }
            switch (groups[2].Value)
            {
                case "00": productId = "Autodesk Civil 3d"; break;
                case "01": productId = "AutoCAD"; break;
                case "0A": productId = "AutoCAD OEM"; break;
                case "02": productId = "AutoCAD Map"; break;
                case "04": productId = "AutoCAD Architecture"; break;
                case "05": productId = "AutoCAD Mechanical"; break;
                case "06": productId = "AutoCAD MEP"; break;
                case "07": productId = "AutoCAD Electrical"; break;
                case "16": productId = "AutoCAD P & ID"; break;
                case "17": productId = "AutoCAD Plant 3d"; break;
                case "29": productId = "AutoCAD ecscad"; break;
                case "30": productId = "AutoCAD Structural Detailing"; break;
                default: productId = "unknown"; break;
            }
            switch (groups[3].Value)
            {
                case "409": localeId = "English"; break;
                case "407": localeId = "German"; break;
                case "40C": localeId = "French"; break;
                case "410": localeId = "Italian"; break;
                case "40A": localeId = "Spanish"; break;
                case "415": localeId = "Polish"; break;
                case "40E": localeId = "Hungarian"; break;
                case "405": localeId = "Czech"; break;
                case "416": localeId = "Brasilian Portuguese"; break;
                case "804": localeId = "Simplified Chinese"; break;
                case "404": localeId = "Traditional Chinese"; break;
                case "412": localeId = "Korean"; break;
                case "411": localeId = "Japanese"; break;
                default: localeId = "unknown"; break;
            }

            // return the requested info

            //先处理大小写的问题

            infoType = infoType.ToLower();

            switch (infoType)
            {
                case "release": return release;
                case "productid": return productId;
                case "localeid": return localeId;
                default: return "unknown request type";
            }
        }

    }
}
