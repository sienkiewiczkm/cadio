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
        private bool _leftMouseButtonDown;
        private Point _lastMousePosition;

        public DesignerWindow()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            _lastMousePosition = e.GetPosition(this);
            _leftMouseButtonDown = true;
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            _leftMouseButtonDown = false;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_leftMouseButtonDown)
                return;

            var mousePosition = e.GetPosition(this);
            var movement = mousePosition - _lastMousePosition;

            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                _viewModel.TranslateSceneWithMouse(movement);
            }
            else
            {
                _viewModel.RotateSceneWithMouse(movement);
            }

            _lastMousePosition = mousePosition;
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            _viewModel.ScaleWithMouse(e.Delta);
        }
    }
}
