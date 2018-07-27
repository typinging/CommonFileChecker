using GalaSoft.MvvmLight;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace CommonChecker
{
    public class SchemeConvertViewModel : ViewModelBase
    {
        ISnackbarMessageQueue viewMessageQueue;
        Dispatcher viewDispatcher;

        #region Properties
        private List<ConvertNode> _viewRootNode = new List<ConvertNode>() { new ConvertNode("xml", null), };
        public List<ConvertNode> ViewRootNode
        {
            get { return _viewRootNode; }
            set { _viewRootNode = value; RaisePropertyChanged(nameof(ViewRootNode)); }
        }

        private List<ConvertNode> _rootNodeList;
        public List<ConvertNode> RootNodeList
        {
            get { return _rootNodeList; }
            set { _rootNodeList = value; RaisePropertyChanged(nameof(RootNodeList)); }
        }

        private ConvertNode _selectedRootNode;
        public ConvertNode SelectedRootNode
        {
            get { return _selectedRootNode; }
            set
            {
                _selectedRootNode = value;
                RaisePropertyChanged(nameof(SelectedRootNode));
            }
        }

        private ConvertNode _selectedNode;
        public ConvertNode SelectedNode
        {
            get { return _selectedNode; }
            set { _selectedNode = value; RaisePropertyChanged(nameof(SelectedNode)); }
        }

        private Dictionary<string, Dictionary<string, string>> _originalNodeMap;
        public Dictionary<string, Dictionary<string, string>> OriginalNodeMap
        {
            get { return _originalNodeMap; }
            set { _originalNodeMap = value; RaisePropertyChanged(nameof(OriginalNodeMap)); }
        }

        private Dictionary<string, string> _selectedOriginalNodeDic;
        public Dictionary<string, string> SelectedOriginalNodeDic
        {
            get { return _selectedOriginalNodeDic; }
            set { _selectedOriginalNodeDic = value; RaisePropertyChanged(nameof(SelectedOriginalNodeDic)); }
        }

        private string currentDerictoryPath = string.Empty; 

        #endregion

        #region Construction
        public SchemeConvertViewModel(ISnackbarMessageQueue snackbarMessageQueue)
        {
            this.viewMessageQueue = snackbarMessageQueue ?? throw new ArgumentNullException(nameof(snackbarMessageQueue));
        }

        #endregion

        #region Methods

        internal void Init(Dispatcher dispatcher)
        {
            this.viewDispatcher = dispatcher;
        }

        internal void AddChildNode(ConvertNode node)
        {
            node.ChildrenNode.Add(new ConvertNode("NewNode", node));
        }

        internal void SetSelectedNode(ConvertNode node)
        {
            SelectedNode = node;
        }

        internal void SetCurrentPath(string dPath)
        {
            currentDerictoryPath = dPath;
        }

        internal void CleanUp()
        {
            if (ViewRootNode != null)
            {
                ViewRootNode = null;
            }
            if(RootNodeList != null)
            {
                RootNodeList = null;
            }
            if (SelectedNode != null)
            {
                SelectedNode = null;
            }
        }

        internal void ParseFileDone(List<CNode> cNodes)
        {
            Task.Run(() =>
            {
                List<ConvertNode> tempList = new List<ConvertNode>();
                foreach (CNode node in cNodes)
                {
                    ConvertNode tempConvertn = new ConvertNode((CNode)node.Clone());
                    tempConvertn.SetChildrenNodeType(tempConvertn);
                    tempList.Add(tempConvertn);
                }
                Dictionary<string, Dictionary<string, string>> tempMap = new Dictionary<string, Dictionary<string, string>>();
                foreach (CNode rootNode in tempList)
                {
                    Dictionary<string, string> tempDic = new Dictionary<string, string>();
                    GetChildrenNodePath(rootNode, tempDic);
                    tempMap.Add(rootNode.Name, tempDic);
                }
                this.viewDispatcher.BeginInvoke((Action)(() =>
                {
                    RootNodeList = tempList;
                    OriginalNodeMap = tempMap;
                    if (SelectedRootNode == null)
                    {
                        SelectedRootNode = RootNodeList.FirstOrDefault();
                    }
                    if (SelectedRootNode != null)
                    {
                        SelectedOriginalNodeDic = OriginalNodeMap[SelectedRootNode.Name];
                    }
                }));
            });
        }

        private void GetChildrenNodePath(CNode rootNode, Dictionary<string, string> tempDic)
        {
            foreach (CNode childNode in rootNode.ChildrenNode)
            {
                tempDic.Add((childNode as ConvertNode).CreateNameString(), (childNode as ConvertNode).CreateSearchString(string.Empty));
                GetChildrenNodePath(childNode, tempDic);
            }
        }
        #endregion
    }
}
