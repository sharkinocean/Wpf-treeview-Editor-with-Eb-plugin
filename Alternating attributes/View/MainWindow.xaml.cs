using Alternating_attributes.Model;
using Alternating_attributes.ViewModel;
using Aucotec.EngineeringBase.Client.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Alternating_attributes
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private ObjectItemTypeDefinationModel _childrenSelected;
        private ObjectItemTypeDefinationModel _parentSelected;
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var binding = (sender as CheckBox).GetBindingExpression(CheckBox.IsCheckedProperty);
            var currentObject = binding.DataItem as ObjectItemTypeDefinationModel;

            MainWindowViewModel dataContext = (MainWindowViewModel)this.DataContext;

            _parentSelected = dataContext.Tree.Where(q => q.Name.Equals(currentObject.Name)).FirstOrDefault();

            if (_parentSelected == null)
            {
                _childrenSelected = dataContext.Tree.Where(q => q.FolderChildren.Any(a => a.Name.Equals(currentObject.Name))).FirstOrDefault();
                if(_childrenSelected != null)
                    _childrenSelected.Checkbox = true;
               else
                    _childrenSelected = dataContext.Tree.Where(q => q.FolderChildren.Any(a => a.FolderChildren.Any(x => x.Name.Equals(currentObject.Name)))).FirstOrDefault();

                var ImidiateFolder = (from mainTree in dataContext.Tree
                        from subTree in mainTree.FolderChildren
                        from children in subTree.FolderChildren
                        where children.Name.Equals(currentObject.Name)
                        select subTree).FirstOrDefault();

                if (ImidiateFolder != null)
                    ImidiateFolder.Checkbox = true;
                if (_childrenSelected != null)
                    _childrenSelected.Checkbox = true;
               
            }

            if (_parentSelected!=null && _parentSelected.Source.Kind.Equals(ObjectKind.Folder)) { 
                var count = _parentSelected.FolderChildren.Where(x => x.Checkbox == true).Count();

                if (count <= 0)
                {
                    foreach (var item in _parentSelected.FolderChildren)
                    {
                        if (item.Checkbox != true)
                            item.Checkbox = true;
                    }
                }
            }
        }

     
    }
}
