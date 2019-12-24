using Aucotec.EngineeringBase.Client.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Alternating_attributes
{
    public class ObjectitemToDragToTree : Helpers.VMBase
    {
        public ObjectItem Source;
        public ObjectItem Folder;
        private bool _Checkbox;
        private List<ObjectitemToDragToTree> _toDragChildren;
        private ObjectKind _objectKind;
        public bool ToDragTreeCheckbox
        {
            get { return _Checkbox; }
            set
            {
                _Checkbox = value;
                foreach (var child in ToDragTreeChildren)
                {
                    child.ToDragTreeCheckbox = value;
                    OnPropertyChanged(nameof(ToDragTreeCheckbox));
                }


            }
        }
        public ObjectKind ObjectKind { get; set; }
        public string ToDragTreeName { get; }
        public ImageSource ToDragTreeIcon { get; }
        public List<ObjectitemToDragToTree> ToDragTreeChildren {
            get { return _toDragChildren; }

            set
            {
                _toDragChildren = value;
                OnPropertyChanged(nameof(ToDragTreeChildren));
            }
        }

        public ObjectitemToDragToTree(ObjectItem source)
        {
            Source = source;
            ToDragTreeName = Source.Name;
            ToDragTreeIcon = Source.Image;
            ObjectKind = source.Kind;

            ToDragTreeChildren = new List<ObjectitemToDragToTree>();

            foreach (ObjectItem child in source.Children)

                ToDragTreeChildren.Add(new ObjectitemToDragToTree(child));
        }
    }
}
