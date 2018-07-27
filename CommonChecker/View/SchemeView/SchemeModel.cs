using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonChecker
{
    public class DeleteCondition : ObservableObject, ICloneable
    {
        private CNode _child = null;
        public CNode Child
        {
            get { return _child; }
            set { _child = value; RaisePropertyChanged(nameof(Child)); }
        }

        private ConditionType _deleteConditionType = ConditionType.Equal;
        public ConditionType DeleteConditionType
        {
            get { return _deleteConditionType; }
            set { _deleteConditionType = value; }
        }

        private string _deleteConditionString = "等于";
        public string DeleteConditionString
        {
            get { return _deleteConditionString; }
            set
            {
                _deleteConditionString = value;
                RaisePropertyChanged(nameof(DeleteConditionString));
                if (_deleteConditionString == "等于")
                {
                    DeleteConditionType = ConditionType.Equal;
                }
                else if (_deleteConditionString == "小于")
                {
                    DeleteConditionType = ConditionType.Less;
                }
                else
                {
                    DeleteConditionType = ConditionType.More;
                }
            }
        }


        private string _value = string.Empty;
        public string Value
        {
            get { return _value; }
            set { _value = value; RaisePropertyChanged(nameof(Value)); }
        }

        public object Clone()
        {
            DeleteCondition result = new DeleteCondition();
            result.Child = this.Child.Clone() as CNode;
            result.DeleteConditionString = this.DeleteConditionString;
            result.DeleteConditionType = this.DeleteConditionType;
            return result as object;
        }
    }

    public enum ConditionType : byte
    {
        Equal = 1,
        More = 2,
        Less = 3,
    }
}
