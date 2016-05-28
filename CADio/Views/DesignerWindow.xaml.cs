using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using CADio.ViewModels;

namespace CADio.Views
{
    public partial class DesignerWindow : Window
    {
        private readonly DesignerViewModel _viewModel;
        private bool _leftMouseButtonDown;
        private Point _lastMousePosition;

        public DesignerWindow()
        {
            InitializeComponent();

            _viewModel = new DesignerViewModel(this);
            DataContext = _viewModel;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _lastMousePosition = e.GetPosition(this);
                _leftMouseButtonDown = true;
            }

            if (e.RightButton == MouseButtonState.Pressed)
            {
                var multigrab = (Keyboard.GetKeyStates(Key.LeftShift) & KeyStates.Down) != 0;
                var aspect = RT.ActualWidth/RT.ActualHeight;
                var pixelPosition = e.GetPosition(RT);
                var xf = pixelPosition.X/RT.ActualWidth;
                var yf = pixelPosition.Y/RT.ActualHeight;
                aspect = 1.0;
                var screenSpaceClick = new Point(2*aspect*xf-aspect, -(2*yf-1));
                _viewModel.GrabUsingMouse(screenSpaceClick, multigrab);
            }
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

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _viewModel.ActiveRenderer.ForceRedraw();
        }

        private void OnSizeChange(object sender, SizeChangedEventArgs e)
        {
            _viewModel.ActiveRenderer.ForceRedraw();
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            int cursorMovementX = 0, cursorMovementY = 0, cursorMovementZ = 0;

            switch (e.Key)
            {
                case Key.Q: ++cursorMovementX; break;
                case Key.A: --cursorMovementX; break;
                case Key.W: ++cursorMovementY; break;
                case Key.S: --cursorMovementY; break;
                case Key.E: ++cursorMovementZ; break;
                case Key.D: --cursorMovementZ; break;
            }

            if (e.Key == Key.F)
                _viewModel.GrabNearestPoint(false);

            if (e.Key == Key.Escape)
                _viewModel.UngrabSelection();

            var moveStep = 0.05;
            _viewModel.MoveManipulator(moveStep * cursorMovementX, 
                moveStep * cursorMovementY, moveStep * cursorMovementZ);
        }
    }
}
