using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CommonChecker
{
    public class CNode : ObservableObject, ICloneable
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; RaisePropertyChanged(nameof(Name)); }
        }

        private int _rank;
        public int Rank
        {
            get { return _rank; }
            set { _rank = value; RaisePropertyChanged(nameof(Rank)); }
        }

        private CNode _parent;
        public CNode Parent
        {
            get { return _parent; }
            set { _parent = value; RaisePropertyChanged(nameof(Parent)); }
        }

        private ObservableCollection<CNode> _childrenNode = new ObservableCollection<CNode>();
        public ObservableCollection<CNode> ChildrenNode
        {
            get { return _childrenNode; }
            set { _childrenNode = value; RaisePropertyChanged(nameof(ChildrenNode)); }
        }

        public virtual object Clone()
        {
            return null;
        }
    }

    public class FileNode : CNode
    {
        private OperationEnum _operationType = OperationEnum.None;
        public OperationEnum OperationType
        {
            get { return _operationType; }
            set
            {
                _operationType = value;
                RaisePropertyChanged(nameof(OperationType));
            }
        }

        private ObservableCollection<DeleteCondition> _deleteConditionCollection = null;
        public ObservableCollection<DeleteCondition> DeleteConditionCollection
        {
            get { return _deleteConditionCollection; }
            set
            {
                _deleteConditionCollection = value;
                RaisePropertyChanged(nameof(DeleteConditionCollection));
            }
        }

        private string _editValue;
        public string EditValue
        {
            get { return _editValue; }
            set { _editValue = value; RaisePropertyChanged(nameof(EditValue)); }
        }

        private string _addName;
        public string AddName
        {
            get { return _addName; }
            set { _addName = value; RaisePropertyChanged(nameof(AddName)); }
        }

        private string _addContent;
        public string AddContent
        {
            get { return _addContent; }
            set { _addContent = value; RaisePropertyChanged(nameof(AddContent)); }
        }

        private bool _hasChildren = false;
        public bool HasChildren
        {
            get { return _hasChildren; }
            set { _hasChildren = value; RaisePropertyChanged(nameof(HasChildren)); }
        }

        private bool _isEditNodeName = false;
        public bool IsEditNodeName
        {
            get { return _isEditNodeName; }
            set { _isEditNodeName = value; RaisePropertyChanged(nameof(IsEditNodeName)); }
        }

        private string _editName;
        public string EditName
        {
            get { return _editName; }
            set { _editName = value; RaisePropertyChanged(nameof(EditName)); }
        }
    }

    public class DirectoryCNode : CNode
    {
        private string _path;
        public string Path
        {
            get { return _path; }
            set { _path = value; RaisePropertyChanged(nameof(Path)); }
        }

        private List<CNode> _currentNodeList;
        public List<CNode> CurrentNodeList
        {
            get { return _currentNodeList; }
            set { _currentNodeList = value; RaisePropertyChanged(nameof(CurrentNodeList)); }
        }

        public DirectoryCNode(int rank, string name, CNode parent, string path)
        {
            this.Rank = rank;
            this.Name = name;
            this.Parent = parent;
            this.Path = path;
            this.ChildrenNode = new ObservableCollection<CNode>();
        }

        public override object Clone()
        {
            Type t = this.GetType();
            object o = Activator.CreateInstance(t);
            PropertyInfo[] piArray = t.GetProperties();
            for (int i = 0; i < piArray.Length; i++)
            {
                PropertyInfo p = piArray[i];
                p.SetValue(o, p.GetValue(this));
            }
            return o;
        }


    }

}
