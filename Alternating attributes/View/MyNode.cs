 
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Alternating_attributes.ViewModel;
using Aucotec.EngineeringBase.Client.Runtime;
using MessageBox = System.Windows.MessageBox;

namespace Alternating_attributes.View
{
    public class MyNode : INotifyPropertyChanged
    {
        private ExApplication _exApp;
        private NodeViewModel _vm;
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isLoaded;
        private bool _isExpanded;
        private bool _isSelected;
        private ImageSource _dbIcon;

        private ObservableCollection<MyNode> _todragtreechildren;
        private ObservableCollection<MyMaskViewModel> _masks;
        private List<MyAttributeViewModel> _myAttributes;

        public ObservableCollection<MyNode> ToDragTreeChildren
        {
            get { return _todragtreechildren; }
            set
            {
                _todragtreechildren = value;
                OnPropertyChanged("ChildrenNodes");
            }
        }

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (!_isLoaded)
                    LoadChild(Id);

                _isExpanded = value;
                OnPropertyChanged("IsExpanded");
            }
        }

        public bool IsLoaded
        {
            get { return _isLoaded; }
            set
            {
                _isLoaded = value;
                OnPropertyChanged("IsLoaded");
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (!_isSelected && value)
                {
                    GetAttributes(Id);
                }
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        public ImageSource DbIcon
        {
            get { return _dbIcon; }
            set
            {
                _dbIcon = value;
                OnPropertyChanged("DbIcon");
            }
        }


        public bool IsDummy { get; private set; }
        public string Id { get; private set; }
        public string Name { get; private set; }
        public ImageSource ImageSource { get; private set; }
        public MyNode ParentNode { get; private set; }

        private void Init(NodeViewModel vm, bool isLoaded, bool isExpanded, bool isSelected, bool isDummy, string id, string name, ImageSource imageSource, MyNode parentNode)
        {
            _exApp = vm?._app as ExApplication;
            _vm = vm;
            _isLoaded = isLoaded;
            _isExpanded = isExpanded;
            _isSelected = isSelected;
            IsDummy = isDummy;
            Id = id;
            Name = name;
            DbIcon = imageSource;
            ParentNode = parentNode;

            if (isDummy)
            {
                _todragtreechildren = null;
                _masks = null;
            }

            else
            {
                _todragtreechildren = new ObservableCollection<MyNode>();
                _masks = new ObservableCollection<MyMaskViewModel>();
            }

        }

        //Dummy
        public MyNode(bool isDummy, string name)
        {
            Init(null, false, false, false, isDummy, null, name, null, null);
        }

        //Root
        public MyNode(string name, NodeViewModel vm)
        {
            Init(vm, false, true, false, false, null, name, null, null);
        }

        public MyNode(ObjectItem objectItem, MyNode parentNode, NodeViewModel vm)
        {
            Init(vm, false, false, false, false, objectItem.Id, objectItem.Name, objectItem.Image, parentNode);

            //add Dummy
            _todragtreechildren.Add(new MyNode(true, "Dummy"));
        }

        private void LoadChild(string id)
        {
            _todragtreechildren.Clear();
            ObjectItem itemInEB = _vm._app.Utils.GetSingleObjectByID(id);

            foreach (ObjectItem EB_childItem in itemInEB.Children)
                _todragtreechildren.Add(new MyNode(EB_childItem, ParentNode, _vm));

            _isLoaded = true;
        }

        private void GetAttributes(string itemID)
        {
            if (!_masks.Any())
            {
                MyMaskViewModel actualItem = null;
                IList<ObjectMaskDescription> descriptions = new List<ObjectMaskDescription>();
                _exApp.ExtendedUtils.GetObjectsMaskDescription(_exApp.CurrentCulture.LCID, itemID, ref descriptions);

                foreach (ObjectMaskDescription maskItem in descriptions)
                {
                    if (maskItem.ID == AttributeId.Unspecified)
                    {
                        actualItem = new MyMaskViewModel(maskItem);
                        _masks.Add(actualItem);
                    }
                    else
                    {
                        _vm._app.Utils.GetSingleObjectByID(itemID).Attributes.TryFindById(maskItem.ID, out AttributeItem attributeItem);
                        actualItem.Attributes.Add(new MyAttributeViewModel(maskItem, attributeItem?.Value));
                    }
                }
            }
            _vm.MyAttributes2 = _masks;
        }

        private void OnPropertyChanged(String propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
