using System.Windows;
using System.Windows.Controls;


namespace CommonChecker
{
    /// <summary>
    /// SchemeConvert.xaml 的交互逻辑
    /// </summary>
    public partial class SchemeConvert : UserControl
    {
        public SchemeConvert(SchemeConvertViewModel cvm)
        {
            InitializeComponent();
            this.DataContext = cvm;
            (this.DataContext as SchemeConvertViewModel).Init(this.Dispatcher);
        }

        private void AddChildItem_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as SchemeConvertViewModel).AddChildNode((sender as FrameworkElement).DataContext as ConvertNode);
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            (this.DataContext as SchemeConvertViewModel).SetSelectedNode(e.NewValue as ConvertNode);
        }

    }
}
