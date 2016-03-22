using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using CADio.Geometry.Shapes;
using CADio.Mathematics;
using CADio.Rendering;
using CADio.SceneManagement;

namespace CADio.ViewModels
{
    internal class DesignerViewModel : INotifyPropertyChanged
    {
        private Renderer _renderer;

        public DesignerViewModel()
        {
            ActiveRenderer = new Renderer {Scene = new Scene()};
            ActiveRenderer.Scene.Objects.Add(new WorldObject() {Shape = new MarkerPoint()});
            ActiveRenderer.Scene.Objects.Add(new WorldObject() {Shape = new Cursor3D()});

            SceneTreeViewModel = new SceneTreeViewModel()
            {
                Scene = ActiveRenderer.Scene
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Renderer ActiveRenderer
        {
            get { return _renderer; }
            set
            {
                _renderer = value;
                OnPropertyChanged();
            }
        }

        public double EyeDistance
        {
            get {  return ActiveRenderer.EyeDistance; }
            set
            {
                ActiveRenderer.EyeDistance = value; // todo: notifiers
                ActiveRenderer.ForceRedraw();
                OnPropertyChanged();
            }
        }

        public SceneTreeViewModel SceneTreeViewModel { get; set; }

        public void RotateSceneWithMouse(Vector mouseMovement)
        {
            ActiveRenderer.Scene.Camera.XRotation += -mouseMovement.Y/50;
            ActiveRenderer.Scene.Camera.YRotation += -mouseMovement.X/50;
            ActiveRenderer.ForceRedraw();
        }

        public void ScaleWithMouse(int delta)
        {
            //ActiveRenderer.Scene.Camera.Zoom += delta*0.002;
            //ActiveRenderer.ForceRedraw();
        }

        public void TranslateSceneWithMouse(Vector mouseMovement)
        {
            ActiveRenderer.Scene.Camera.Move(new Vector3D(mouseMovement.X/50, 0, -mouseMovement.Y/50));
            ActiveRenderer.ForceRedraw();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
