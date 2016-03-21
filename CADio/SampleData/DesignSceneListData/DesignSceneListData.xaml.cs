﻿//      *********    DO NOT MODIFY THIS FILE     *********
//      This file is regenerated by a design tool. Making
//      changes to this file can cause errors.
namespace Expression.Blend.SampleData.DesignSceneListData
{
    using System; 
    using System.ComponentModel;

// To significantly reduce the sample data footprint in your production application, you can set
// the DISABLE_SAMPLE_DATA conditional compilation constant and disable sample data at runtime.
#if DISABLE_SAMPLE_DATA
    internal class DesignSceneListData { }
#else

    public class DesignSceneListData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public DesignSceneListData()
        {
            try
            {
                Uri resourceUri = new Uri("/CADio;component/SampleData/DesignSceneListData/DesignSceneListData.xaml", UriKind.RelativeOrAbsolute);
                System.Windows.Application.LoadComponent(this, resourceUri);
            }
            catch
            {
            }
        }

        private Scene _Scene = new Scene();

        public Scene Scene
        {
            get
            {
                return this._Scene;
            }

            set
            {
                if (this._Scene != value)
                {
                    this._Scene = value;
                    this.OnPropertyChanged("Scene");
                }
            }
        }

        private string _SelectedShape = string.Empty;

        public string SelectedShape
        {
            get
            {
                return this._SelectedShape;
            }

            set
            {
                if (this._SelectedShape != value)
                {
                    this._SelectedShape = value;
                    this.OnPropertyChanged("SelectedShape");
                }
            }
        }
    }

    public class Scene : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private Shapes _Shapes = new Shapes();

        public Shapes Shapes
        {
            get
            {
                return this._Shapes;
            }
        }
    }

    public class ShapesItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string _Name = string.Empty;

        public string Name
        {
            get
            {
                return this._Name;
            }

            set
            {
                if (this._Name != value)
                {
                    this._Name = value;
                    this.OnPropertyChanged("Name");
                }
            }
        }
    }

    public class Shapes : System.Collections.ObjectModel.ObservableCollection<ShapesItem>
    { 
    }
#endif
}