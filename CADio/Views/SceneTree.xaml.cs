using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CADio.Geometry.Shapes.Static;
using CADio.SceneManagement;
using CADio.SceneManagement.Interfaces;
using CADio.ViewModels;
using CADio.Views.DragDropSupport;

namespace CADio.Views
{
    public partial class SceneTree : UserControl
    {
        private const string DragDropFormat = "BasicDragDropFormat";

        private Point _startPoint;

        public SceneTree()
        {
            InitializeComponent();
        }

        private void ListDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // todo: viewmodel
            var viewModel = DataContext as SceneTreeViewModel;

            var selectable = viewModel?.SelectedObject as ISceneSelectable;
            if (selectable != null)
                viewModel.Scene.GrabObject(selectable);
        }

        private void OnTreeViewPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
        }

        private void OnTreeViewPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var mousePosition = e.GetPosition(null);
                var movement = _startPoint - mousePosition;

                if (Math.Abs(movement.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(movement.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    var senderTreeView = sender as TreeView;
                    var treeViewItem = FindAncestor<TreeViewItem>((DependencyObject)e.OriginalSource);

                    if (senderTreeView == null || treeViewItem == null)
                        return;

                    var sceneObject = senderTreeView.SelectedItem as IUIDragable;
                    if (sceneObject == null)
                        return;

                    var dragData = new DataObject(DragDropFormat, sceneObject);
                    DragDrop.DoDragDrop(treeViewItem, dragData, DragDropEffects.Move);
                }
            }
        }

        private void OnTreeViewDrop(object sender, DragEventArgs e)
        {
            var treeViewItem = FindAncestor<TreeViewItem>((DependencyObject)e.OriginalSource);
            var dropzone = treeViewItem?.Header as IDropzone;

            if (dropzone != null && e.Data.GetDataPresent(DragDropFormat))
            {
                var dragable = e.Data.GetData(DragDropFormat) as IUIDragable;
                if (dragable != null)
                    dropzone.Drop(dragable);
            }
        }

        private void OnTreeViewDragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DragDropFormat))
                e.Effects = DragDropEffects.None;
        }

        private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            do
            {
                if (current is T)
                    return (T)current;

                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);

            return null;
        }
    }
}
