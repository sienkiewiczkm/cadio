using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using CADio.Rendering;
using CADio.ViewModels;

namespace CADio.Views
{
    public partial class DesignerWindow : Window
    {
        private readonly DesignerViewModel _viewModel;
        private bool _leftMouseButtonDown;
        private bool _boxGrab;
        private Point _startMousePosition;
        private Point _lastMousePosition;

        public DesignerWindow()
        {
            InitializeComponent();

            _viewModel = new DesignerViewModel(this);
            DataContext = _viewModel;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            _startMousePosition = _lastMousePosition = e.GetPosition(RT);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _leftMouseButtonDown = true;
            }

            if (e.RightButton == MouseButtonState.Pressed)
            {
                _boxGrab =
                    (Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) != 0;
            }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            var rightButtonChanged = e.ChangedButton == MouseButton.Right;

            if (_boxGrab && rightButtonChanged)
            {
                var screenSpaceStart = GetScreenSpacePoint(_startMousePosition);
                var screenSpaceEnd = GetScreenSpacePoint(e.GetPosition(RT));
                var grabRect = new Rect(screenSpaceStart, screenSpaceEnd);
                _viewModel.GrabUsingMouseRect(grabRect, false);
            }
            else if (rightButtonChanged)
            {
                var multigrab = (Keyboard.GetKeyStates(Key.LeftShift) & 
                    KeyStates.Down) != 0;
                var screenSpaceClick = GetScreenSpacePoint(e.GetPosition(RT));
                _viewModel.GrabUsingMouse(screenSpaceClick, multigrab);
            }           

            _leftMouseButtonDown = false;
        }

        private Point GetScreenSpacePoint(Point pixelPosition)
        {
            var aspect = RT.ActualWidth/RT.ActualHeight;
            var xf = pixelPosition.X/RT.ActualWidth;
            var yf = pixelPosition.Y/RT.ActualHeight;
            aspect = 1.0; // todo: wtf? aspekt brany pod uwage w macierzy?
            return new Point(2*aspect*xf - aspect, -(2*yf - 1));
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_leftMouseButtonDown)
                return;

            var mousePosition = e.GetPosition(RT);
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
            int rotationStep = 0, scalingStep = 0;

            switch (e.Key)
            {
                case Key.Q: ++cursorMovementX; break;
                case Key.A: --cursorMovementX; break;
                case Key.W: ++cursorMovementY; break;
                case Key.S: --cursorMovementY; break;
                case Key.E: ++cursorMovementZ; break;
                case Key.D: --cursorMovementZ; break;
                case Key.U: ++rotationStep; break;
                case Key.J: --rotationStep; break;
                case Key.O: ++scalingStep; break;
                case Key.K: --scalingStep; break;
            }

            if (e.Key == Key.F)
                _viewModel.GrabNearestPoint(false);

            if (e.Key == Key.Escape)
                _viewModel.UngrabSelection();

            var moveStep = 0.05;
            _viewModel.MoveManipulator(moveStep * cursorMovementX, 
                moveStep * cursorMovementY, moveStep * cursorMovementZ);

            if (rotationStep != 0)
                _viewModel.RotateSelectionZAxis(rotationStep);
            if (scalingStep != 0)
                _viewModel.ScaleSelection(scalingStep);
        }

        private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var renderTarget = sender as RenderTarget;
            Debug.Assert(renderTarget != null, "renderTarget != null");
            renderTarget.Focus();
        }
    }
}
