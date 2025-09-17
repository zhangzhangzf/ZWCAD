using Mrf.CSharp.BaseTools;
using Mrf.CSharp.BaseTools.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json.Serialization;
using System.Reflection;

/*
 * https://blog.csdn.net/q__y__L/article/details/103566693?spm=1001.2101.3001.6661.1&utm_medium=distribute.pc_relevant_t0.none-task-blog-2%7Edefault%7ECTRLIST%7ERate-1-103566693-blog-108852227.pc_relevant_paycolumn_v3&depth_1-utm_source=distribute.pc_relevant_t0.none-task-blog-2%7Edefault%7ECTRLIST%7ERate-1-103566693-blog-108852227.pc_relevant_paycolumn_v3&utm_relevant_index=1
 * https://blog.csdn.net/wangtao19932008/article/details/108852227?ops_request_misc=%257B%2522request%255Fid%2522%253A%2522165365579016782391876326%2522%252C%2522scm%2522%253A%252220140713.130102334..%2522%257D&request_id=165365579016782391876326&biz_id=0&utm_medium=distribute.pc_search_result.none-task-blog-2~all~sobaiduend~default-2-108852227-null-null.142^v11^pc_search_result_control_group,157^v12^new_style2&utm_term=c%23+%E8%A7%A3%E6%9E%90+json&spm=1018.2226.3001.4187
 */


namespace Mrf.CSharp.BaseTools
{
    /// <summary>
    /// Json格式文件工具
    /// 使用前需引用开源项目类库：Newtonsoft.Json.dll
    /// </summary>
    public class JsonTool
    {




        /// <summary>
        /// 将对象序列化为json格式
        /// </summary>
        /// <param name="obj">序列化对象</param>
        /// <returns>json字符串</returns>
        public static string SerializeObject(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }





        /// <summary>
        /// 将任意一个类实例化后的数据保存为Json文件
        /// </summary>
        /// <param name="obj">任意一个类的实例化对象</param>
        /// <param name="jsonFileName">全路径文件，后缀不一定为.json，也可以为比如.dat</param>
        public static void ToJsonFile(object obj, string jsonFileName)
        {
            //string output = JsonConvert.SerializeObject(obj);
            string output = JsonConvert.SerializeObject(obj, Formatting.Indented); //Indented表示转化为的Json文件带缩进，这样输出会更加直观清晰。


            //将Json文件以字符串的形式保存
            StreamWriter sw = new StreamWriter(jsonFileName);
            sw.Write(output);
            sw.Close();
            //Console.WriteLine(output);
        }




        /// <summary>
        /// 输出包含的指定属性 名称列表的数据保存为Json文件
        /// </summary>
        /// <param name="obj">任意一个类的实例化对象</param>
        /// <param name="propertyNameLst">包含的指定的属性名称列表</param>
        /// <param name="jsonFileName">全路径文件，后缀不一定为.json，也可以为比如.dat</param>
        public static void IncludePropertyLstToJsonFile(object obj, List<string> propertyNameLst, string jsonFileName)
        {
            var options = new JsonSerializerSettings { ContractResolver = new IncludePropertyContractResolver(propertyNameLst) };

            string output = JsonConvert.SerializeObject(obj, Formatting.Indented, options); //Indented表示转化为的Json文件带缩进，这样输出会更加直观清晰。

            StreamWriter sw = new StreamWriter(jsonFileName);
            sw.Write(output);
            sw.Close();
        }


        

        /// <summary>
        /// 输出不包含的指定属性 名称列表的数据保存为Json文件
        /// </summary>
        /// <param name="obj">任意一个类的实例化对象</param>
        /// <param name="propertyNameLst">不包含的指定的属性名称列表</param>
        /// <param name="jsonFileName">全路径文件，后缀不一定为.json，也可以为比如.dat</param>
        public static void ExcludePropertyLstToJsonFile(object obj, List<string> propertyNameLst, string jsonFileName)
        {
            var options = new JsonSerializerSettings { ContractResolver = new ExcludePropertyContractResolver(propertyNameLst) };

            string output = JsonConvert.SerializeObject(obj, Formatting.Indented, options); //Indented表示转化为的Json文件带缩进，这样输出会更加直观清晰。

            StreamWriter sw = new StreamWriter(jsonFileName);
            sw.Write(output);
            sw.Close();
        }



        /// <summary>
        /// 输出不包含指定字符串结尾的属性 名称列表的数据保存为Json文件
        /// </summary>
        /// <param name="obj">任意一个类的实例化对象</param>
        /// <param name="excludeEndWithString">不包含指定字符串结尾</param>
        /// <param name="jsonFileName">全路径文件，后缀不一定为.json，也可以为比如.dat</param>
        public static void ExcludeEndWithPropertyLstToJsonFile(object obj, string excludeEndWithString, string jsonFileName)
        {
            var options = new JsonSerializerSettings { ContractResolver = new ExcludeEndWithPropertyContractResolver(excludeEndWithString) };

            string output = JsonConvert.SerializeObject(obj, Formatting.Indented, options); //Indented表示转化为的Json文件带缩进，这样输出会更加直观清晰。

            StreamWriter sw = new StreamWriter(jsonFileName);
            sw.Write(output);
            sw.Close();
        }







        





        /// <summary>
        /// 解析JSON字符串生成对象实体
        /// </summary>
        /// <typeparam name="T">实体类</typeparam>
        /// <param name="json">JSON字符串</param>
        /// <returns></returns>
        public static T JsonConvertObject<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }







        /// <summary>
        /// 解析JSON字符串生成对象实体 注意：如果对象T里面有几何对象，集合对象将会以数组的形式存储
        /// 比如下面的类：
        /// public class Acount
        ///{
        ///    public string Email { get; set; }
        ///        public Ilist/<string/> Roles { get; set}
        ///    }
        ///    集合对象 Ilist/<string/> Roles 会以数组的形式存储
        ///    如
        ///        List/<string/> videogames = new List/<string/>
        /// {
        ///  "Starcraft",
        /// "Halo",
        ///    "Legend of Zelda"
        /// };
        /// </summary>
        /// <typeparam name="T">实体类</typeparam>
        /// <param name="jsonFileName">JSON文件，不一定后缀为.json，比如为 .dat，不知道这里是否需要考虑文件的保存格式的问题，需要待确认</param>
        /// <returns></returns>
        public static T FromJsonFile<T>(string jsonFileName)
        {

            StreamReader sr = new StreamReader(jsonFileName);

            //一直读完 不知道这里是否需要考虑文件的保存格式的问题，需要待确认
            string json = sr.ReadToEnd();

            sr.Close();

            return JsonConvert.DeserializeObject<T>(json);
        }




        /// <summary>
        /// 解析JSON字符串生成对象实体
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="json">json字符串</param>
        /// <returns></returns>
        public static T DeserializeJsonToObject<T>(string json) where T : class
        {
            JsonSerializer serializer = new JsonSerializer();
            StringReader sr = new StringReader(json);
            object obj = serializer.Deserialize(new JsonTextReader(sr), typeof(T));
            T t = obj as T;
            return t;
        }





        //序列化集合和字典:

        //List集合被转化为了数组，当然List里面可以是复杂的类型,如使用我们之前定义的Product：

        //Product product1 = new Product()
        //{
        //    Name = "Apple",
        //    ExpiryDate = new DateTime(2020, 12, 30),
        //    Price = 2.99M,
        //    Sizes = new string[] { "small", "medium", "large" }

        //};
        //Product product2 = new Product()
        //{
        //    Name = "cup",
        //    ExpiryDate = new DateTime(2099, 1, 1),
        //    Price = 9.99M,
        //    Sizes = new string[] { "s", "L", "M", "xL" }

        //};

        //List<Product> list = new List<Product>() { product1, product2 };
        //string json = JsonConvert.SerializeObject(list);
        //Console.WriteLine(json);



        //返回：

        //        [{"Name":"Apple","ExpiryDate":"2020-12-30T00:00:00","Price":2.99,"Sizes":["small","medium","large"]
        //    },{"Name":"cup","ExpiryDate":"2099-01-01T00:00:00","Price":9.99,"Sizes":["s","L","M","xL"]
        //}]




        //        Dictionary<string, int> points = new Dictionary<string, int>
        //{
        //    { "James", 9001 },
        //    { "Jo", 3474 },
        //    { "Jess", 11926 }
        //};

        //        string json = JsonConvert.SerializeObject(points, Formatting.Indented); //Indented表示转化为的Json文件带缩进，这样输出会更加直观清晰。

        //        Console.WriteLine(json);
        //// {
        ////   "James": 9001,
        ////   "Jo": 3474,
        ////   "Jess": 11926
        //// }












        /// <summary>
        /// 解析JSON数组生成对象实体集合  反序列化集合一般转为List类型，如果需要转化为其它类型，后续再处理。
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="json">json数组</param>
        /// <returns>对象实体集合</returns>
        public static List<T> DeserializeJsonToList<T>(string json) where T : class
        {
            JsonSerializer serializer = new JsonSerializer();
            StringReader sr = new StringReader(json);
            object obj = serializer.Deserialize(new JsonTextReader(sr), typeof(List<T>));
            List<T> list = obj as List<T>;
            return list;
        }




        /// <summary>
        /// 将JSON转数组
        /// 用法:jsonArr[0]["xxxx"]
        /// </summary>
        /// <param name="json">json字符串</param>
        /// <returns></returns>
        public static JArray GetToJsonList(string json)
        {
            JArray jsonArr = (JArray)JsonConvert.DeserializeObject(json);

            return jsonArr;
        }






    //    Book book = new Book
    //    {
    //        BookName = "The Gathering Storm",
    //        BookPrice = 16.19m,
    //        AuthorName = "Brandon Sanderson",
    //        AuthorAge = 34,
    //        AuthorCountry = "United States of America"
    //    };

    //    string startingWithA = JsonConvert.SerializeObject(book, Formatting.Indented,
    //        new JsonSerializerSettings { ContractResolver = new DynamicContractResolver('A') });

    //    // {
    //    //   "AuthorName": "Brandon Sanderson",
    //    //   "AuthorAge": 34,
    //    //   "AuthorCountry": "United States of America"
    //    // }

    //    string startingWithB = JsonConvert.SerializeObject(book, Formatting.Indented,
    //        new JsonSerializerSettings { ContractResolver = new DynamicContractResolver('B') });

    //    // {
    //    //   "BookName": "The Gathering Storm",
    //    //   "BookPrice": 16.19
    //    // }




    //    /// <summary>
    //    /// 过滤出属性名带特定首字母的类
    //    /// </summary>
    //    public class DynamicContractResolver : DefaultContractResolver
    //    {
    //        private readonly char _startingWithChar;

    //        public DynamicContractResolver(char startingWithChar)
    //        {
    //            _startingWithChar = startingWithChar;
    //        }

    //        /// <summary>
    //        /// 针对多个属性进行筛选
    //        /// </summary>
    //        /// <param name="type"></param>
    //        /// <param name="memberSerialization"></param>
    //        /// <returns></returns>
    //        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    //        {
    //            IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);

    //            // only serializer properties that start with the specified character
    //            //只需要在这里添加属性筛选
    //            properties =
    //                properties.Where(p => p.PropertyName.StartsWith(_startingWithChar.ToString())).ToList();

    //            return properties;
    //        }
    //    }






    //    string json = JsonConvert.SerializeObject(new[] { joe, mike }, Formatting.Indented,
    //new JsonSerializerSettings { ContractResolver = ShouldSerializeContractResolver.Instance });


    //    /// <summary>
    //    /// 只想对Manage成员进行筛选
    //    /// </summary>
    //    public class ShouldSerializeContractResolver : DefaultContractResolver
    //    {
    //        public static readonly ShouldSerializeContractResolver Instance = new ShouldSerializeContractResolver();

    //        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    //        {
    //            JsonProperty property = base.CreateProperty(member, memberSerialization);

    //            if (property.DeclaringType == typeof(Employee) && property.PropertyName == "Manager")
    //            {
    //                property.ShouldSerialize =
    //                    instance =>
    //                    {
    //                        Employee e = (Employee)instance;
    //                        return e.Manager != e;
    //                    };
    //            }

    //            return property;
    //        }
    //    }







        /// <summary>
        /// 过滤出包含指定的属性名称列表的类
        /// </summary>
        public class IncludePropertyContractResolver : DefaultContractResolver
        {
            private readonly List<string> _includePropertyNameLst;


            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="includePropertyNameLst">包含的属性名称列表，区分大小写</param>
            public IncludePropertyContractResolver(List<string> includePropertyNameLst)
            {
                _includePropertyNameLst = includePropertyNameLst;
            }

            /// <summary>
            /// 针对多个属性进行筛选
            /// </summary>
            /// <param name="type"></param>
            /// <param name="memberSerialization"></param>
            /// <returns></returns>
            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);

                // only serializer properties that start with the specified character
                //只需要在这里添加属性筛选
                properties =
                    properties.Where(p => _includePropertyNameLst.Contains( p.PropertyName)).ToList();

                return properties;
            }
        }




        



        /// <summary>
        /// 过滤出不包含指定的属性名称列表的类
        /// </summary>
        public class ExcludePropertyContractResolver : DefaultContractResolver
        {
            private readonly List<string> _excludePropertyNameLst;


            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="excludePropertyNameLst">不包含的属性名称列表，区分大小写</param>
            public ExcludePropertyContractResolver(List<string> excludePropertyNameLst)
            {
                _excludePropertyNameLst = excludePropertyNameLst;
            }

            /// <summary>
            /// 针对多个属性进行筛选
            /// </summary>
            /// <param name="type"></param>
            /// <param name="memberSerialization"></param>
            /// <returns></returns>
            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);

                // only serializer properties that start with the specified character
                //只需要在这里添加属性筛选
                properties =
                    properties.Where(p =>! _excludePropertyNameLst.Contains( p.PropertyName)).ToList();

                return properties;
            }
        }



        

        /// <summary>
        /// 过滤出不包含指定字符串结尾的属性名称列表的类
        /// </summary>
        public class ExcludeEndWithPropertyContractResolver : DefaultContractResolver
        {
            private readonly string _excludeEndWithString;


            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="excludeEndWithString">不包含指定字符串结尾，区分大小写</param>
            public ExcludeEndWithPropertyContractResolver(string excludeEndWithString)
            {
                _excludeEndWithString = excludeEndWithString;
            }

            /// <summary>
            /// 针对多个属性进行筛选
            /// </summary>
            /// <param name="type"></param>
            /// <param name="memberSerialization"></param>
            /// <returns></returns>
            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);

                // only serializer properties that start with the specified character
                //只需要在这里添加属性筛选
                properties =
                    properties.Where(p =>!p.PropertyName.EndsWith(_excludeEndWithString)).ToList();

                return properties;
            }
        }









    }
}
