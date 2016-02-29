using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Media3D;
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
            ActiveRenderer.Scene.Shapes.Add(new Torus());
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

        public void RotateSceneWithMouse(Vector mouseMovement)
        {
            var rotation = Transformations3D.RotationY(mouseMovement.X/50)
                * Transformations3D.RotationX(-mouseMovement.Y/50);
            ActiveRenderer.Scene.WorldTransformation = rotation * ActiveRenderer.Scene.WorldTransformation;
            ActiveRenderer.ForceRedraw();
        }

        public void ScaleWithMouse(int delta)
        {
            var scalingFactor = Math.Max(0, 1.0 + delta*0.002);
            var scalingMatrix = Transformations3D.Scaling(scalingFactor);
            ActiveRenderer.Scene.WorldTransformation = scalingMatrix*ActiveRenderer.Scene.WorldTransformation;
            ActiveRenderer.ForceRedraw();
        }

        public void TranslateSceneWithMouse(Vector mouseMovement)
        {
            var translation = Transformations3D.Translation(new Vector3D(mouseMovement.X/50, 0.0, mouseMovement.Y/50));
            ActiveRenderer.Scene.WorldTransformation = translation*ActiveRenderer.Scene.WorldTransformation;
            ActiveRenderer.ForceRedraw();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
