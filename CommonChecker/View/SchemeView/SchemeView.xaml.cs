using System.Windows;
using System.Windows.Controls;

namespace CommonChecker
{
    /// <summary>
    /// SchemeView.xaml 的交互逻辑
    /// </summary>
    public partial class SchemeView : UserControl
    {
        public SchemeView()
        {
            InitializeComponent();

            this.Loaded += SchemeView_Loaded;
        }

        private void SchemeView_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= SchemeView_Loaded;
            if (this.DataContext != null)
            {
                (this.DataContext as SchemeViewModel).viewDispatcher = this.Dispatcher;
                (this.DataContext as SchemeViewModel).listView = this.ContentCountListView;
            }
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            (this.DataContext as SchemeViewModel).SetSelectedNode(e.NewValue as FileNode);
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            switch (byte.Parse((sender as FrameworkElement).Tag.ToString()))
            {
                case (byte)OperationEnum.Edit:
                    (this.DataContext as SchemeViewModel).SetCurrentNodeStatus(OperationEnum.Edit);
                    break;
                case (byte)OperationEnum.Delete:
                    (this.DataContext as SchemeViewModel).SetCurrentNodeStatus(OperationEnum.Delete);
                    break;
                case (byte)OperationEnum.Add:
                    (this.DataContext as SchemeViewModel).SetCurrentNodeStatus(OperationEnum.Add);
                    break;
            }
        }

        private void NameEditButton_Click(object sender, RoutedEventArgs e)
        {
            if (NameEditButton.Content.ToString() == "修改名称")
            {
                (this.DataContext as SchemeViewModel).SelectedNode.IsEditNodeName = true;
                (this.DataContext as SchemeViewModel).SelectedNode.EditValue = null;
            }
            else
            {
                (this.DataContext as SchemeViewModel).SelectedNode.IsEditNodeName = false;
                (this.DataContext as SchemeViewModel).SelectedNode.EditName = null;
            }
        }

        private void TypeCountMenuItem_Click(object sender, RoutedEventArgs e)
        {
            FileNode currentNode = (sender as FrameworkElement).DataContext as FileNode;
            (this.DataContext as SchemeViewModel).StatisticSpecifyNodeContent(currentNode);
        }
    }

}
