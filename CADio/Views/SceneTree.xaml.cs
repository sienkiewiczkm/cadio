using System.Windows.Controls;
using System.Windows.Input;
using CADio.ViewModels;

namespace CADio.Views
{
    public partial class SceneTree : UserControl
    {
        public SceneTree()
        {
            InitializeComponent();
        }

        private void ListDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // todo: viewmodel
            var viewModel = DataContext as SceneTreeViewModel;
            if (viewModel == null) return;
            viewModel.Scene.GrabbedObject = viewModel.SelectedObject;
        }
    }
}
