
using Aucotec.EngineeringBase.Client.Runtime;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
namespace Alternating_attributes.Model
{
    public class ObjectItemTypeDefinationModel : Helpers.VMBase
    {
        public ObjectItem Source;

        private bool _Checkbox;
        public bool Checkbox
        {
            get { return _Checkbox; }
            set
            {
                _Checkbox = value;
                foreach (var child in FolderChildren)
                {
                    child.Checkbox = value;
                    OnPropertyChanged(nameof(Checkbox));
                }
              
              
            }
        }
        public string Name { get; }
        public ImageSource Icon { get; }
        public List<ObjectItemTypeDefinationModel> FolderChildren { get; }

        public ObjectItemTypeDefinationModel(ObjectItem source)
        {
            Source = source;
            Name = Source.Name;
            Icon = Source.Image;

            FolderChildren = new List<ObjectItemTypeDefinationModel>();

            foreach (ObjectItem child in source.Children)
                
                FolderChildren.Add(new ObjectItemTypeDefinationModel(child));
        }
    }
}
