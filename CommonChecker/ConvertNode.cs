using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonChecker
{
    public class ConvertNode : CNode
    {
        private FileType _schemeType = FileType.XML;
        public FileType SchemeType
        {
            get { return _schemeType; }
            set { _schemeType = value; RaisePropertyChanged(nameof(SchemeType)); }
        }

        private string _originalNodeName;
        public string OriginalNodeName
        {
            get { return _originalNodeName; }
            set { _originalNodeName = value; RaisePropertyChanged(nameof(OriginalNodeName)); }
        }

        private string _originalNodePath;
        public string OriginalNodePath
        {
            get { return _originalNodePath; }
            set { _originalNodePath = value; RaisePropertyChanged(nameof(OriginalNodePath)); }
        }

        private bool _isArray = false;
        public bool IsArray
        {
            get { return _isArray; }
            set { _isArray = value; RaisePropertyChanged(nameof(IsArray)); }
        }

        public ConvertNode Root { get; set; } = null;

        public ConvertNode(string name, ConvertNode parent)
        {
            this.Name = name;
            this.Rank = 0;
            this.Parent = parent;
            if (parent != null)
            {
                this.SchemeType = parent.SchemeType;
            }
        }
        public ConvertNode(CNode node)
        {
            this.Name = node.Name;
            this.Rank = node.Rank;
            this.Parent = node.Parent;
            this.ChildrenNode = node.ChildrenNode;
            if (node is JsonCNode)
            {
                this.SchemeType = FileType.JSON;
            }
            else
            {
                this.SchemeType = FileType.XML;
            }
        }

        public void SetChildrenNodeType(ConvertNode root)
        {
            for (int i = 0; i < ChildrenNode.Count; i++)
            {
                ConvertNode tempNode = new ConvertNode(ChildrenNode[i]);
                tempNode.SchemeType = root.SchemeType;
                if (ChildrenNode[i] is JsonCNode)
                {
                    tempNode.IsArray = (ChildrenNode[i] as JsonCNode).IsArray;
                }
                tempNode.Parent = this;
                tempNode.Root = root;
                ChildrenNode[i] = tempNode;
                (ChildrenNode[i] as ConvertNode).SetChildrenNodeType(root);
            }
        }
    }

    public enum FileType : byte
    {
        XML = 0,
        JSON = 1,
    }
}
