namespace Mrf.CSharp.BaseTools
{
    /// <summary>
    /// 任意数据类型的类
    /// </summary>
    public class AnyDataType
    {

        /// <summary>
        /// 构造函数
        /// </summary>
        public AnyDataType()
        {

        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dataType">数据类型</param>
        /// <param name="intValue">整数</param>
        public AnyDataType(DataType dataType, int intValue)
        {
            DataType = dataType;
            IntValue = intValue;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dataType">数据类型</param>
        /// <param name="doubleValue">双精度</param>
        public AnyDataType(DataType dataType, double doubleValue)
        {
            DataType = dataType;
            DoubleValue = doubleValue;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dataType">数据类型</param>
        /// <param name="stringValue">字符串</param>
        public AnyDataType(DataType dataType, string stringValue)
        {
            DataType = dataType;
            StringValue = stringValue;
        }





        /// <summary>
        ///数据类型
        /// </summary>
        public DataType DataType { get; set; }


        /// <summary>
        /// 整数值
        /// </summary>
        public int IntValue { get; set; }


        /// <summary>
        /// 双精度值
        /// </summary>
        public double DoubleValue { get; set; }


        /// <summary>
        /// 字符串值
        /// </summary>
        public string StringValue { get; set; }










    }


    /// <summary>
    /// 数据类型枚举
    /// </summary>
    public enum DataType
    {
        /// <summary>
        /// 整数
        /// </summary>
        Int,

        /// <summary>
        /// 双精度
        /// </summary>
        Double,

        /// <summary>
        /// 字符串
        /// </summary>
        String


    }




}
