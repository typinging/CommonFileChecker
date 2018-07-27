using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace CommonChecker
{
    public class JsonCNode : FileNode
    {
        private long _count;
        public long Count
        {
            get { return _count; }
            set { _count = value; RaisePropertyChanged(nameof(Count)); }
        }

        private bool _isArray;
        public bool IsArray
        {
            get { return _isArray; }
            set { _isArray = value; RaisePropertyChanged(nameof(IsArray)); }
        }

        public override object Clone()
        {
            JsonCNode result = new JsonCNode();
            result.Name = this.Name;
            result.IsArray = this.IsArray;
            result.HasChildren = this.HasChildren;
            result.IsEditNodeName = this.IsEditNodeName;
            result.Rank = this.Rank;
            result.OperationType = this.OperationType;
            result.AddContent = this.AddContent;
            result.AddName = this.AddName;
            result.Count = this.Count;
            if (this.DeleteConditionCollection == null)
            {
                result.DeleteConditionCollection = null;
            }
            else
            {
                List<DeleteCondition> tempDCList = new List<DeleteCondition>();
                foreach (DeleteCondition dc in this.DeleteConditionCollection)
                {
                    tempDCList.Add(dc.Clone() as DeleteCondition);
                }
                result.DeleteConditionCollection = new ObservableCollection<DeleteCondition>(tempDCList);
            }
            result.EditName = this.EditName;
            result.EditValue = this.EditValue;
            result.Parent = this.Parent;
            if (this.ChildrenNode == null)
            {
                result.ChildrenNode = null;
            }
            else
            {
                List<CNode> tempNodeList = new List<CNode>();
                foreach (CNode node in this.ChildrenNode)
                {
                    tempNodeList.Add(node.Clone() as CNode);
                }
                result.ChildrenNode = new ObservableCollection<CNode>(tempNodeList);
            }
            return result as object;
        }

    }
}
