using GalaSoft.MvvmLight;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Command;

namespace CommonChecker
{
    public delegate void SetNodeDelegate(CNode node);
    public delegate void ParseFileDoneDelegate(List<CNode> cNodes);
    public delegate void SetCurrentDirectoryPath(string dPath);
    public delegate void CleanUpDelegate();

    public class CCheckerViewModel : ViewModelBase
    {
        private static readonly string jsonFilterString = "*.json";
        private static readonly string xmlFilterString = "*.xml";

        private List<string> jsonPathList = new List<string>();
        private List<string> xmlPathList = new List<string>();

        private CNode DirectoryRoot = null;

        private Dispatcher mainDispatcher = null;

        public event SetNodeDelegate SetNodeEvent;
        public event ParseFileDoneDelegate ParseFileDoneEvent;
        public event SetCurrentDirectoryPath SetCurrentPathEvent;
        public event CleanUpDelegate CleanUpEvent;

        public List<CNode> FileSchemeList = new List<CNode>();

        #region Properties
        public ViewItem[] ViewItems { get; }

        private string _currentDirectoryPath = string.Empty;
        public string CurrentDirectoryPath
        {
            get { return _currentDirectoryPath; }
            set
            {
                _currentDirectoryPath = value;
                RaisePropertyChanged(nameof(CurrentDirectoryPath));
                SetCurrentPathEvent(_currentDirectoryPath);
            }
        }

        private CNode _currentDirectoryNode;
        public CNode CurrentDirectoryNode
        {
            get { return _currentDirectoryNode; }
            set
            {
                _currentDirectoryNode = value;
                RaisePropertyChanged(nameof(CurrentDirectoryNode));
                CurrentDirectoryPath = (_currentDirectoryNode as DirectoryCNode).Path;
                if (CurrentDirectoryNode == DirectoryRoot)
                {
                    ReturnVisibility = false;
                }
                else
                {
                    ReturnVisibility = true;
                }
            }
        }

        private bool _returnVisibility = false;
        public bool ReturnVisibility
        {
            get { return _returnVisibility; }
            set
            {
                _returnVisibility = value;
                RaisePropertyChanged(nameof(ReturnVisibility));
            }
        }

        private RelayCommand _returnParentDirectoryCommand;
        public RelayCommand ReturnParentDirectoryCommand
        {
            get
            {
                if (_returnParentDirectoryCommand == null)
                {
                    _returnParentDirectoryCommand = new RelayCommand(ReturnParentDirectory);
                }
                return _returnParentDirectoryCommand;
            }
        }

        #endregion

        public CCheckerViewModel(ISnackbarMessageQueue snackbarMessageQueue, Dispatcher dispatcher)
        {
            SchemeViewModel svm = new SchemeViewModel(snackbarMessageQueue);
            SchemeConvertViewModel cvm = new SchemeConvertViewModel(snackbarMessageQueue);

            this.SetNodeEvent += svm.SetNode;
            this.ParseFileDoneEvent += cvm.ParseFileDone;
            this.SetCurrentPathEvent += svm.SetCurrentPath;
            this.SetCurrentPathEvent += cvm.SetCurrentPath;
            this.CleanUpEvent += svm.CleanUp;
            this.CleanUpEvent += cvm.CleanUp;
            ViewItems = new[]
                {
                    new ViewItem("统计和修改", new SchemeView()
                        {
                            DataContext = svm,
                        }),
                    new ViewItem("转换", new SchemeConvert(cvm)),
                };
            this.mainDispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        #region Methods
        internal void LoadDirectory(string dPath)
        {
            FileSchemeList.Clear();
            int startRank = 0;
            CNode rootNode = new DirectoryCNode(startRank, Path.GetDirectoryName(dPath), null, dPath);
            GetDirectories(new DirectoryInfo(dPath), rootNode, startRank);
            DirectoryRoot = rootNode;
            this.mainDispatcher.Invoke((Action)(() =>
            {
                CurrentDirectoryNode = rootNode;
            }));

            Parser psr = new Parser();
            psr.Set_ParserInstance(new XmlParser());
            ParseFile(psr, xmlPathList);

            CNode node = psr.Get_FileScheme();
            if (node != null && node.ChildrenNode.Count > 0)
            {
                FileSchemeList.Add(node);
            }
            psr.Set_ParserInstance(new JsonParser());
            ParseFile(psr, jsonPathList);

            node = psr.Get_FileScheme();
            if (node != null && node.ChildrenNode.Count > 0)
            {
                FileSchemeList.Add(node);
            }
            ParseFileDoneEvent(FileSchemeList);
        }

        internal void SetCurrentDirectoryNode(CNode dNode)
        {
            CurrentDirectoryNode = dNode;
        }

        private void ParseFile(Parser psr, List<string> filePathList)
        {
            if (filePathList.Count > 0)
            {
                foreach (string filePath in filePathList)
                {
                    if (psr.ParseFile(filePath))
                    {
                        SetNodeEvent((CNode)psr.Get_FileScheme());
                    }
                    else
                    {
                        //TODO: log4net
                        continue;
                    }
                }
            }
        }

        private void GetDirectories(DirectoryInfo root, CNode rNode, int rank)
        {
            rank++;
            foreach (FileInfo fi in root.GetFiles(jsonFilterString))
            {
                jsonPathList.Add(fi.FullName);
            }
            foreach (FileInfo fi in root.GetFiles(xmlFilterString))
            {
                xmlPathList.Add(fi.FullName);
            }

            foreach (DirectoryInfo dir in root.GetDirectories())
            {
                CNode subNode = new DirectoryCNode(rank, dir.Name, rNode, dir.FullName);
                rNode.ChildrenNode.Add(subNode);
                GetDirectories(dir, subNode, rank);
            }
        }

        public override void Cleanup()
        {
            jsonPathList.Clear();
            xmlPathList.Clear();
            CurrentDirectoryPath = string.Empty;
            DirectoryRoot = null;
            CleanUpEvent();
        }

        private void ReturnParentDirectory()
        {
            if (CurrentDirectoryNode != DirectoryRoot)
            {
                CurrentDirectoryNode = CurrentDirectoryNode.Parent;
            }
        }

        private void Dispose()
        {
            SchemeViewModel svm = (ViewItems[0].View as FrameworkElement).DataContext as SchemeViewModel;
            SchemeConvertViewModel cvm = (ViewItems[1].View as FrameworkElement).DataContext as SchemeConvertViewModel;
            this.SetNodeEvent -= svm.SetNode;
            this.ParseFileDoneEvent -= cvm.ParseFileDone;
            this.SetCurrentPathEvent -= svm.SetCurrentPath;
            this.SetCurrentPathEvent -= cvm.SetCurrentPath;
            this.CleanUpEvent -= svm.CleanUp;
            this.CleanUpEvent -= cvm.CleanUp;
            Cleanup();
        }
        #endregion
    }
}
