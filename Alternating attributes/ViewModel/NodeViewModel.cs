using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Alternating_attributes.View;
using Aucotec.EngineeringBase.Client.Runtime;
using MessageBox = System.Windows.MessageBox;

namespace Alternating_attributes.ViewModel
{
    public class NodeViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public Application _app { get; private set; }
        //public ObservableCollection<MyAttribute> MyAttributes { get; set; }

        public ObservableCollection<MyMaskViewModel> MyAttributes2
        {
            get { return _myAttributes2; }
            set
            {
                _myAttributes2 = value;
                OnPropertyChanged("MyAttributes2");
            }
        }

        private ObjectItem _root;
        private MyNode _rootNode;
        private ObservableCollection<MyMaskViewModel> _myAttributes2;


        public MyNode RootNode
        {
            get { return _rootNode; }
            set
            {
                _rootNode = value;
                OnPropertyChanged("RootNode");
            }
        }

        public NodeViewModel(Application app)
        {
            _app = app;
            _root = _app.RootObject;
            _rootNode = new MyNode("Root", this);
            //MyAttributes    = new ObservableCollection<MyAttribute>();

            _myAttributes2 = new ObservableCollection<MyMaskViewModel>();
        }

        public void Start()
        {
            foreach (ObjectItem rootChild in _root.Children)
                _rootNode.ToDragTreeChildren.Add(new MyNode(rootChild, _rootNode, this));

            _rootNode.IsLoaded = true;
        }

        private void OnPropertyChanged(String propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
