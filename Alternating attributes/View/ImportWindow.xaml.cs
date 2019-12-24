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
using System.Windows.Shapes;

namespace Alternating_attributes.View
{
    /// <summary>
    /// Interaction logic for ImportWindow.xaml
    /// </summary>{}
    public partial class ImportWindow : Window
    {
        public ImportWindowViewModel ViewModel { get; set; }
        public ObjectitemToDragToTree node;
        public ObjectitemTabTree nodeDrop;

        private string _name2;
        public string Name2
        {
            get { return _name2; }
            set { _name2 = value; }
        }


        public ImportWindow()
        {
            InitializeComponent();
        }

        private void TreeView_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
        }

        private void TreeView_MouseMove(object sender, MouseEventArgs e)
        {
            if (node != null || nodeDrop != null)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                    DragDrop.DoDragDrop(this, node, DragDropEffects.Move);
            }
        }

        private void TreeView_Drop(object sender, DragEventArgs e)
        {

            //string str = (string)e.Data.GetData(typeof(string));
            ObjectitemToDragToTree DroopedItem = e.Data.GetData(typeof(ObjectitemToDragToTree)) as ObjectitemToDragToTree;
            TreeViewItem treeViewItem = FindAncestor<TreeViewItem>((DependencyObject)e.OriginalSource);

            if (treeViewItem != null)
            {
                nodeDrop = treeViewItem.DataContext as ObjectitemTabTree;
                var droppedNode = (ObjectitemTabTree)treeViewItem.Header;

                if (nodeDrop.Source != null)
                {
                    MaskDescriptionModel mask = new MaskDescriptionModel();
                    ObjectMaskDescription listMask = new ObjectMaskDescription();
                    listMask.Name = DroopedItem.ToDragTreeName;
                    mask.TabName = listMask;
                    object idFromValue = new object();
                    try
                    {
                        idFromValue = DroopedItem.Source.Attributes.Where(x => x.Id.Equals(AttributeId.Aid)).Select(x => x.Value).FirstOrDefault();
                    }
                    catch { MessageBox.Show("Id cant befound for this attribute, please check value at TId."); }
                    ObjectitemTabTree obj = new ObjectitemTabTree(mask);
                    obj.AddedId = idFromValue.ToString();
                    nodeDrop.TabTreeChildren.Add(obj);
                    treeViewItem.Items.Refresh();
                    if (nodeDrop != null)
                    {
                        MessageBox.Show("You have added item to " + nodeDrop.TabTreeName);
                    }

                }
                else { MessageBox.Show("Please drag to an attribute tab."); }
            }
            else { MessageBox.Show("Please select a proper drop off point."); }
        }



        private void Tree_Selected(object sender, RoutedEventArgs e)
        {

            //string str = (string)e.Data.GetData(typeof(string));
            TreeViewItem treeViewItemDrop = e.OriginalSource as TreeViewItem;
            nodeDrop = treeViewItemDrop.DataContext as ObjectitemTabTree;

            if (nodeDrop != null)
            {
                MessageBox.Show(nodeDrop.TabTreeName);
            }


        }

        public void OnItemSelected(object sender, RoutedEventArgs e)
        {
            TreeViewItem treeViewItem = e.OriginalSource as TreeViewItem;
            node = treeViewItem.DataContext as ObjectitemToDragToTree;
            //  MessageBox.Show(node.ToDragTreeName);


        }

        private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            // Search the VisualTree for specified type
            while (current != null)
            {
                if (current is T)
                    return (T)current;

                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }

        public class AdvancedTreeViewItem<T> : TreeViewItem
        {
            public T ParentNodeValue { get; set; }
            public T RootParentNodeValue { get; set; }
        }

        private void ContextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        //    var le = node;
        //    var f = nodeDrop;
        //    try
        //    {
        //        TreeViewItem treeViewItem = e.OriginalSource as TreeViewItem;
        //        TreeViewItem treeViewItemw = FindAncestor<TreeViewItem>((DependencyObject)e.OriginalSource);
        //        node = treeViewItem.DataContext as ObjectitemToDragToTree;
        //        //node = treeViewItemDrop.DataContext as ObjectitemToDragToTree;
        //        var test = ToTakeWords.Text;

        //        //blocks repainting tree till all objects loaded
        //        var tes = node.ToDragTreeChildren.Where(x => x.ToDragTreeName.Equals(test));
        //    }
        //    catch { }
        //    //if (test != string.Empty)
        //    //{
        //    //    foreach (ObjectitemToDragToTree _parentNode in node.)
        //    //    {
        //    //        foreach (TreeNode _childNode in _parentNode.Nodes)
        //    //        {
        //    //            if (_childNode.Text.StartsWith(this.fieldFilterTxtBx.Text))
        //    //            {
        //    //                this.fieldsTree.Nodes.Add((TreeNode)_childNode.Clone());
        //    //            }
        //    //        }
        //    //    }
        //    //}
        //    //else
        //    //{
        //    //    foreach (TreeNode _node in this._fieldsTreeCache.Nodes)
        //    //    {
        //    //        fieldsTree.Nodes.Add((TreeNode)_node.Clone());
        //    //    }
        //    //}
        //    ////enables redrawing tree after all objects have been added
        //    //this.fieldsTree.EndUpdate();
        }

    }
}
