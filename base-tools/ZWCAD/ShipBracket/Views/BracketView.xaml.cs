using System.Windows;

namespace ZWCAD.ShipBracket.Views
{
    /// <summary>
    /// BracketView.xaml 的交互逻辑
    /// </summary>
    public partial class BracketView : Window
    {
        public BracketView()
        {
            InitializeComponent();
        }
        private void 取消_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
