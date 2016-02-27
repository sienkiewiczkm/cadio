using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CADio.Rendering;

namespace CADio.ViewModels
{
    internal class DesignerViewModel : INotifyPropertyChanged
    {
        private Renderer _renderer;

        public DesignerViewModel()
        {
            ActiveRenderer = new Renderer();
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
            ActiveRenderer.XRotation += mouseMovement.Y/50;
            ActiveRenderer.YRotation += mouseMovement.X/50;
            ActiveRenderer.ForceRedraw();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
