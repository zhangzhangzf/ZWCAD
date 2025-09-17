using ZwSoft.ZwCAD.ApplicationServices;
using Prism.Commands;

namespace ZWCAD.ShipBracket.ViewModels
{
    /// <summary>
    /// 添加船舶肘板
    /// </summary>
    public partial class BracketViewModel
    {



        #region Private Variables
        Document m_document;
        static double confirmedFirstLength = 300;
        static double confirmedSecondLength = 300;
        static double confirmedHoleRadius = 50;
        static double confirmedToesLength = 25;


        #endregion



        #region Default Constructor


        #endregion




        #region CommandMethods


        #endregion



        #region Helper Methods


        #endregion


        #region Properties

        private double _firstLength = confirmedFirstLength;
        /// <summary>
        /// 第一条边长度
        /// </summary>
        public double FirstLength
        {
            get { return _firstLength; }
            set
            {
                _firstLength = value;
                RaisePropertyChanged(nameof(FirstLength));  //双向绑定
            }
        }

        private double _secondLength = confirmedSecondLength;
        /// <summary>
        /// 第二条边长度
        /// </summary>
        public double SecondLength
        {
            get { return _secondLength; }
            set
            {
                _secondLength = value;
                RaisePropertyChanged(nameof(SecondLength));  //双向绑定
            }
        }

        private double _holeRadius = confirmedHoleRadius;
        /// <summary>
        /// 通焊孔半径
        /// </summary>
        public double HoleRadius
        {
            get { return _holeRadius; }
            set
            {
                _holeRadius = value;
                RaisePropertyChanged(nameof(HoleRadius));  //双向绑定
            }
        }

        private double _toesLength = confirmedToesLength;
        /// <summary>
        /// 脚趾高度
        /// </summary>
        public double ToesLength
        {
            get { return _toesLength; }
            set
            {
                _toesLength = value;
                RaisePropertyChanged(nameof(ToesLength));  //双向绑定
            }
        }

        /// <summary>
        /// 确认按钮对应的命令
        /// </summary>
        public DelegateCommand ConfirmCommand => new DelegateCommand(ConfirmCommandRun);
        #endregion

    }
}
