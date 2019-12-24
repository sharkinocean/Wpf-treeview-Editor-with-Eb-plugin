using Aucotec.EngineeringBase.Client.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alternating_attributes.Model
{
    public class MaskDescriptionModel
    {
        public List<MaskDescriptionModel> TreeView { get; set; }
        public ObjectMaskDescription TabName { get; set; }
        public List<ObjectMaskDescription> TabAttributes { get; set; }
        public string TabNameString { get; set; }
        public List<ExcelAttributeMapping> AttibuteNameAndID { get; set; }
        public string Typename { get; set; }
        public string FolderName { get; set; }
    }
}
