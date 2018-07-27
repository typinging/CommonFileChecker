using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace CommonChecker
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CCheckerMainWindow : Window
    {
        CCheckerViewModel CCheckerVM;

        public CCheckerMainWindow()
        {
            InitializeComponent();
            this.DataContext = CCheckerVM = new CCheckerViewModel(this.MainSnackbar.MessageQueue, this.Dispatcher);
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string dPath = (e.Data.GetData(DataFormats.FileDrop) as Array).GetValue(0).ToString();
                if (Directory.Exists(dPath))
                {
                    CCheckerVM.Cleanup();
                    Task.Run(() =>
                    {
                        CCheckerVM.LoadDirectory(dPath);
                    });
                }
                else
                {
                    this.MainSnackbar.MessageQueue.Enqueue("必须拖入文件夹！");
                    //TO DO: log4net
                }
            }
        }

        private void ViewItemListBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var dependencyObject = Mouse.Captured as DependencyObject;
            while (dependencyObject != null)
            {
                if (dependencyObject is ScrollBar) return;
                dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
            }
            this.MenuToggleButton.IsChecked = false;
        }

        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            (this.DataContext as CCheckerViewModel).SetCurrentDirectoryNode(((sender as FrameworkElement).DataContext) as CNode);
        }
    }
}
