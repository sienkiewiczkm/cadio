﻿using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CADio.Helpers.MVVM;

namespace CADio.SceneManagement
{
    public class ControlPoint : INotifyPropertyChanged
    {
        private ICommand _unlinkCommand;
        public BezierWorldObject Owner { get; set; }
        public WorldObject Reference { get; set; }

        public ICommand UnlinkCommand
        {
            get { return _unlinkCommand ?? (_unlinkCommand = new RelayCommand(Unlink)); }
            set { _unlinkCommand = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Unlink()
        {
            Owner.DetachObject(this);
        }
    }
}