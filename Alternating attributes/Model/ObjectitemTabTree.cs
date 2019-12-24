using Alternating_attributes.Model;
using Aucotec.EngineeringBase.Client.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Alternating_attributes
{
    public class ObjectitemTabTree : Helpers.VMBase
    {
        public MaskDescriptionModel Source;
        public ObjectMaskDescription Source2;

        private bool _Checkbox;
        private List<MaskDescriptionModel> treeView;
        private string _treeName;
        private string _addedString;
        private ImageSource _image;
        private List<ObjectitemTabTree> _treeChild;

        public bool TabTreeCheckbox
        {
            get { return _Checkbox; }
            set
            {
                _Checkbox = value;
                foreach (var child in TabTreeChildren)
                {
                    child.TabTreeCheckbox = value;
                    OnPropertyChanged(nameof(TabTreeCheckbox));
                }


            }
        }
        public string TabTreeName
        {
            get {
                //if(String.IsNullOrEmpty)
                //OnPropertyChanged(nameof(TabTreeName));
                return _treeName;
            }
            set
            {
                _treeName = value;
                OnPropertyChanged(nameof(TabTreeName));
            }
        }

        public ImageSource TabTreeIcon
        {
            get {
                return _image;
            }
            set
            {
                _image = value;
                OnPropertyChanged(nameof(TabTreeIcon));
            }

        }
        public List<ObjectitemTabTree> TabTreeChildren {
            get {
                return _treeChild;
            }
            set
            {
                _treeChild = value;
                OnPropertyChanged(nameof(TabTreeChildren));
            }
        }

        public string AddedId
        {
            get
            {
                return _addedString;
            }
            set
            {
                _addedString = value;
            }
        }

        public ObjectitemTabTree(MaskDescriptionModel source)
        {
            Source = source;
            ////TabTreeIcon = Source.Image;
            TabTreeChildren = new List<ObjectitemTabTree>();
            if(!String.IsNullOrEmpty(Source.FolderName))
                TabTreeName = Source.FolderName;

            else if (!String.IsNullOrEmpty(Source.Typename))
                TabTreeName = Source.Typename;
            else
                TabTreeName = source.TabName.Name;

            if (source.TabAttributes != null && source.TabAttributes.Count != 0)
            {
                foreach (var item in source.TabAttributes)
                    TabTreeChildren.Add(new ObjectitemTabTree(item));
            }

            if (Source.TreeView != null)
            {
                foreach (var child in Source.TreeView)
                {
                    TabTreeChildren.Add(new ObjectitemTabTree(child));
                }
            }
        }

        public ObjectitemTabTree(ObjectMaskDescription source)
        {
            Source2 = source;
            TabTreeName = Source2.Name;
        }

        
    }
}
