using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonChecker
{
    public class ViewItem : ObservableObject
    {
        private string _name = string.Empty;
        public string Name
        {
            get { return _name; }
            set { _name = value; RaisePropertyChanged(nameof(Name)); }
        }

        private object _view = null;
        public object View
        {
            get { return _view; }
            set { _view = value; RaisePropertyChanged(nameof(View)); }
        }

        public ViewItem(string name, object view)
        {
            this.Name = name;
            this.View = view;
        }
    }
}
