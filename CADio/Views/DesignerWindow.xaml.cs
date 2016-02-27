using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using CADio.ViewModels;

namespace CADio.Views
{
    /// <summary>
    /// Interaction logic for DesignerWindow.xaml
    /// </summary>
    public partial class DesignerWindow : Window
    {
        private readonly DesignerViewModel _viewModel = new DesignerViewModel();
        private bool _rotationActive;
        private Point _lastMousePosition;

        public DesignerWindow()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            _lastMousePosition = e.GetPosition(this);
            _rotationActive = true;
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            _rotationActive = false;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_rotationActive)
                return;

            var mousePosition = e.GetPosition(this);
            var movement = mousePosition - _lastMousePosition;
            _viewModel.RotateSceneWithMouse(movement);
            _lastMousePosition = mousePosition;
        }
    }
}
