using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace CommonChecker
{
    public class SchemeViewModel : ViewModelBase
    {
        private static readonly string xmlFilterString = "*.xml";
        private static readonly string jsonFilterString = "*.json";

        ISnackbarMessageQueue viewMessageQueue;

        #region Properties
        private ObservableCollection<CNode> _rootNodeCollection;
        public ObservableCollection<CNode> RootNodeCollection
        {
            get { return _rootNodeCollection; }
            set { _rootNodeCollection = value; RaisePropertyChanged(nameof(RootNodeCollection)); }
        }

        private FileNode _selectedNode;
        public FileNode SelectedNode
        {
            get { return _selectedNode; }
            set
            {
                if (_selectedNode != null)
                {
                    SaveEditOrDeleteCondition();
                }
                _selectedNode = value;
                if (_selectedNode?.ChildrenNode?.Count > 0)
                {
                    _selectedNode.HasChildren = true;
                }
                else
                {
                    if (_selectedNode != null)
                    {
                        _selectedNode.HasChildren = false;
                    }
                }
                RaisePropertyChanged(nameof(SelectedNode));
                SelectedChildrenNode = null;
                if (_selectedNode != null)
                {
                    SelectedChildrenNode = GetChildrenNodes(_selectedNode);
                    OperationType = _selectedNode.OperationType;
                }
            }
        }


        private CNode _child;
        public CNode Child
        {
            get { return _child; }
            set { _child = value; RaisePropertyChanged(nameof(Child)); }
        }

        private List<CNode> _selectedChildrenNode;
        public List<CNode> SelectedChildrenNode
        {
            get { return _selectedChildrenNode; }
            set { _selectedChildrenNode = value; RaisePropertyChanged(nameof(SelectedChildrenNode)); }
        }

        private OperationEnum _operationType = OperationEnum.None;
        /// <summary>
        /// true为Edit，false为Delete，null为初始化
        /// </summary>
        public OperationEnum OperationType
        {
            get { return _operationType; }
            set { _operationType = value; RaisePropertyChanged(nameof(OperationType)); }
        }

        private bool _isContentCountPanelShow = false;
        public bool IsContentCountPanelShow
        {
            get { return _isContentCountPanelShow; }
            set { _isContentCountPanelShow = value; RaisePropertyChanged(nameof(IsContentCountPanelShow)); }
        }

        private Dictionary<string, int> _specifyNodeContentCountMap;
        public Dictionary<string, int> SpecifyNodeContentCountMap
        {
            get { return _specifyNodeContentCountMap; }
            set { _specifyNodeContentCountMap = value; RaisePropertyChanged(nameof(SpecifyNodeContentCountMap)); }
        }

        public Dispatcher viewDispatcher { get; set; }

        public System.Windows.Controls.ListView listView { get; set; }

        public string CurrentDirectoryPath { get; set; }

        #endregion

        #region Command
        private RelayCommand _addDeleteConditionCommand;
        public RelayCommand AddDeleteConditionCommand
        {
            get
            {
                if (_addDeleteConditionCommand == null)
                {
                    _addDeleteConditionCommand = new RelayCommand(AddDeleteCondition);
                }
                return _addDeleteConditionCommand;
            }
        }

        private RelayCommand<object> _exportCommand;
        public RelayCommand<object> ExportCommand
        {
            get
            {
                if (_exportCommand == null)
                {
                    _exportCommand = new RelayCommand<object>(ExportFiles);
                }
                return _exportCommand;
            }
        }

        private RelayCommand _clearEditCommand;
        public RelayCommand ClearEditCommand
        {
            get
            {
                if (_clearEditCommand == null)
                {
                    _clearEditCommand = new RelayCommand(ClearEdit);
                }
                return _clearEditCommand;
            }
        }

        #endregion

        public SchemeViewModel(ISnackbarMessageQueue snackbarMessageQueue)
        {
            this.viewMessageQueue = snackbarMessageQueue ?? throw new ArgumentNullException(nameof(snackbarMessageQueue));
        }

        #region Methods

        public void SetNode(CNode root)
        {
            if (RootNodeCollection == null)
            {
                RootNodeCollection = new ObservableCollection<CNode>();
            }
            CNode node = RootNodeCollection.Where(fs => fs.Name == root.Name).FirstOrDefault();
            if (node != null)
            {
                node = root;
            }
            else
            {
                if (this.viewDispatcher != null)
                {
                    this.viewDispatcher.Invoke((Action)(() =>
                    {
                        RootNodeCollection.Add(root);
                    }));
                }
            }
        }

        internal void SetSelectedNode(FileNode value)
        {
            this.SelectedNode = value;
        }

        internal void SetCurrentPath(string dPath)
        {
            CurrentDirectoryPath = dPath;
        }

        public void CleanUp()
        {
            if (RootNodeCollection != null)
            {
                RootNodeCollection.Clear();
            }
        }

        private void AddDeleteCondition()
        {
            if (this.SelectedNode != null)
            {
                if (SelectedNode.DeleteConditionCollection == null)
                {
                    SelectedNode.DeleteConditionCollection = new ObservableCollection<DeleteCondition>(); 
                }
                SelectedNode.DeleteConditionCollection.Add(new DeleteCondition());
            }
        }

        internal void SetCurrentNodeStatus(OperationEnum status)
        {
            if (SelectedNode != null)
            {
                OperationType = status;
                SelectedNode.OperationType = status;
            }
        }

        internal void StatisticSpecifyNodeContent(FileNode specifyNode)
        {
            string currentDirectory = CurrentDirectoryPath;
            SpecifyNodeContentCountMap = null;
            IsContentCountPanelShow = true;
            Task.Run(() =>
            {
                List<string> list = new List<string>();
                string fileFilter = string.Empty;
                if (specifyNode is XmlCNode)
                {
                    fileFilter = xmlFilterString;
                }
                else
                {
                    fileFilter = jsonFilterString;
                }
                SearchFile(currentDirectory, fileFilter, list);
                Searcher searcher = new Searcher();
                try
                {
                    searcher.SearchSpecifyNode(specifyNode, list);
                    this.viewDispatcher.Invoke((Action)(() =>
                    {
                        SpecifyNodeContentCountMap = searcher.ContentCountMap;
                    }));
                }
                catch (ArgumentException ex)
                {
                    viewMessageQueue.Enqueue(ex.Message);
                    HideRightContent();
                }
                catch (FormatException ex)
                {
                    viewMessageQueue.Enqueue(ex.Message);
                    HideRightContent();
                }
                catch (Exception ex)
                {
                    //TO DO: log4net
                    HideRightContent();
                }

            });
        }

        private void HideRightContent()
        {
            this.viewDispatcher.Invoke((Action)(() =>
            {
                IsContentCountPanelShow = false;
            }));
        }

        private void ExportFiles(object obj)
        {
            string destPath = string.Empty;
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    destPath = dialog.SelectedPath;
                }
                else
                {
                    destPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output");
                    if (!Directory.Exists(destPath)) { Directory.CreateDirectory(destPath); }
                }
            }

            string cPath = CurrentDirectoryPath;
            FileNode node = (FileNode)(obj as FileNode).Clone();
            Task.Run(() =>
            {
                List<string> list = new List<string>();
                string fileFilter = string.Empty;
                if (obj is XmlCNode)
                {
                    fileFilter = xmlFilterString;
                }
                else
                {
                    fileFilter = jsonFilterString;
                }
                SearchFile(cPath, fileFilter, list);
                ModifyFiles(list, node, cPath, destPath);
                this.viewMessageQueue.Enqueue("导出完成！");
            });
        }

        private void ModifyFiles(List<string> list, FileNode node, string originalPath, string destPath)
        {
            foreach (string p in list)
            {
                string destFilePath = p.Replace(originalPath, destPath);
                string destDirectory = Path.GetDirectoryName(destFilePath);
                if (!Directory.Exists(destDirectory)) { Directory.CreateDirectory(destDirectory); }
                Editor editor = new Editor();
                if (node is XmlCNode)
                {
                    editor.Set_EditorInstance(new XmlEditer());
                }
                else
                {
                    editor.Set_EditorInstance(new JsonEditor());
                }
                if (editor.ParseFile(p, node, destFilePath))
                {
                    //TO DO
                }
            }
        }

        private List<CNode> GetChildrenNodes(CNode root)
        {
            List<CNode> tempList = new List<CNode>();
            SearchChildrenNode(root, tempList);
            return tempList;
        }

        private void SearchChildrenNode(CNode root, List<CNode> list)
        {
            if (root.ChildrenNode != null)
            {
                foreach (CNode child in root.ChildrenNode)
                {
                    list.Add(child);
                    SearchChildrenNode(child, list);
                }
            }
        }

        private void SaveEditOrDeleteCondition()
        {
            _selectedNode.OperationType = this.OperationType;
            if (this.OperationType == OperationEnum.Edit)
            {
                _selectedNode.DeleteConditionCollection = null;
                _selectedNode.AddContent = string.Empty;
                _selectedNode.AddName = string.Empty;
            }
            else if (this.OperationType == OperationEnum.Delete)
            {
                _selectedNode.EditValue = string.Empty;
                _selectedNode.AddContent = string.Empty;
                _selectedNode.AddName = string.Empty;
            }
            else if (this.OperationType == OperationEnum.Add)
            {
                _selectedNode.DeleteConditionCollection = null;
                _selectedNode.EditValue = string.Empty;
            }
            else
            {
                _selectedNode.EditValue = string.Empty;
                _selectedNode.DeleteConditionCollection = null;
                _selectedNode.AddContent = string.Empty;
                _selectedNode.AddName = string.Empty;
            }
        }

        private void SearchFile(string dPath, string searchFilter, List<string> list)
        {
            DirectoryInfo root = new DirectoryInfo(dPath);
            foreach (FileInfo fi in root.GetFiles(searchFilter))
            {
                list.Add(fi.FullName);
            }
            foreach (DirectoryInfo di in root.GetDirectories())
            {
                SearchFile(di.FullName, searchFilter, list);
            }
        }

        private void ClearEdit()
        {
            if (SelectedNode != null)
            {
                OperationType = SelectedNode.OperationType = OperationEnum.None;
                SelectedNode.EditValue = null;
                SelectedNode.DeleteConditionCollection = null;
            }
        }

        #endregion
    }
}
