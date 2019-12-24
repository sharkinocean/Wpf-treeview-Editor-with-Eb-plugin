using Aucotec.EngineeringBase.Client.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alternating_attributes.ViewModel
{
    public class MyAttributeViewModel
    {
        public AttributeId Id { get; private set; }
        public string Name { get; private set; }
        public object Value { get; private set; }

        public MyAttributeViewModel(AttributeItem newAttribute)
        {
            Id = newAttribute.Id;
            Name = newAttribute.Name;
            Value = newAttribute.Value;
        }

        public MyAttributeViewModel(ObjectMaskDescription maskItem, object value)
        {
            Id = maskItem.ID;
            Name = maskItem.Name;
            Value = value;
        }
    }
}
