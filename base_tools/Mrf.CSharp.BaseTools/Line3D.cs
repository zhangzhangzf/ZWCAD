using Mrf.CSharp.BaseTools.Extension;
using System;

namespace Mrf.CSharp.BaseTools
{
    /// <summary>
    /// 几何三维线
    /// </summary>
    public class Line3D
    {


        #region Fields

        Point3d m_endPnt; //end point
        double m_length; //length of line
        Point3d m_normal; //normal
        Point3d m_startPnt; //start point

        #endregion Fields

        #region Constructors

        /// <summary>
        /// The default constructor
        /// </summary>
        public Line3D()
        {
            m_startPnt = Point3d.Zero;
            m_endPnt = Point3d.BasisX;
            m_length = 1.0;
            m_normal = Point3d.BasisX;
        }

        /// <summary>
        /// The default constructor
        /// </summary>
        /// <param name="startPnt">start point of line</param>
        /// <param name="endPnt">enn point of line</param>
        public Line3D(Point3d startPnt, Point3d endPnt)
        {
            m_startPnt = startPnt;
            m_endPnt = endPnt;
            CalculateDirection();
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Property to get and set End Point of line
        /// </summary>
        public Point3d EndPoint
        {
            get
            {
                return m_endPnt;
            }
            set
            {
                if (m_endPnt == value)
                {
                    return;
                }
                m_endPnt = value;
                CalculateDirection();
            }
        }

        //property
        /// <summary>
        /// Property to get and set length of line
        /// </summary>
        public double Length
        {
            get
            {
                return m_length;
            }
            set
            {
                if (m_length == value)
                {
                    return;
                }
                m_length = value;
                CalculateEndPoint();
            }
        }

        /// <summary>
        /// Property to get and set Normal of line
        /// </summary>
        public Point3d Normal
        {
            get
            {
                return m_normal;
            }
            set
            {
                if (m_normal == value)
                {
                    return;
                }
                m_normal = value;
                CalculateEndPoint();
            }
        }

        /// <summary>
        /// Property to get and set Start Point of line
        /// </summary>
        public Point3d StartPoint
        {
            get
            {
                return m_startPnt;
            }
            set
            {
                if (m_startPnt == value)
                {
                    return;
                }
                m_startPnt = value;
                CalculateDirection();
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// calculate Direction by StartPoint and EndPoint
        /// </summary>
        private void CalculateDirection()
        {
            CalculateLength();
            m_normal = (m_endPnt - m_startPnt) / m_length;
        }

        /// <summary>
        /// calculate EndPoint by StartPoint, Length and Direction
        /// </summary>
        private void CalculateEndPoint()
        {
            m_endPnt = m_startPnt + m_normal * m_length;
        }

        /// <summary>
        /// calculate length by StartPoint and EndPoint
        /// </summary>
        private void CalculateLength()
        {
            m_length = m_startPnt.DistanceTo(m_endPnt);
        }





        ///// <summary>
        ///// 判断空间两条直线（有界的），是否共线（即是否在同一条无限长的直线上）
        ///// </summary>
        ///// <param name="lineA"></param>
        ///// <param name="lineB"></param>
        ///// <param name="tolerance">误差</param>
        ///// <returns>如果是，返回true，否则，返回false</returns>
        //public static bool IsInOneLine(this Line lineA, Line lineB, double tolerance = DOUBLE_DELTA)
        //{

        //    //2.先将lineA复制一根，然后使其无限长
        //    var unboundLine = lineA.Clone();
        //    unboundLine.MakeUnbound();

        //    //3. 看看lineB是否在unboundLine上，
        //    //用unboundLine.Intersect(lineB)== SetComparisonResult.Superset来判断，控制不了误差
        //    var projectP0 = unboundLine.Project(lineB.GetEndPoint(0))?.XYZPoint;
        //    var projectP1 = unboundLine.Project(lineB.GetEndPoint(1))?.XYZPoint;

        //    var dis0 = projectP0?.DistanceTo(lineB.GetEndPoint(0));
        //    var dis1 = projectP1?.DistanceTo(lineB.GetEndPoint(1));
        //    if (dis0 < tolerance && dis1 < tolerance)
        //    {
        //        return true;
        //    }
        //    else
        //    {



        //        return false;
        //    }
        //}










        /// <summary>
        /// 判断两根线是否共线
        /// </summary>
        /// <param name="secondLine">指定的线</param>
        /// <returns>如果共线，返回true，否则，返回false</returns>
        public bool IsCollinear(Line3D secondLine)
        {
            return Normal.CheckCollinearity(secondLine.Normal);
        }



        #endregion Methods
    }
}



