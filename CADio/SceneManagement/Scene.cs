using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Media.Media3D;
using CADio.Configuration;
using CADio.Geometry.Shapes;
using CADio.Mathematics;
using CADio.Rendering;
using CADio.SceneManagement.Interfaces;
using CADio.SceneManagement.Points;
using CADio.SceneManagement.Serialization;

namespace CADio.SceneManagement
{
    public class Scene : INotifyPropertyChanged
    {
        private WorldObject _manipulator;
        private ObservableCollection<WorldObject> _objects;
        private ICamera _camera = new ArcBallCamera();
        private ICollectionView _manageableObjects;
        private WorldObject _firstSelectedObject;
        private ISmartEditTarget _smartEditTarget;
        private readonly List<ISceneSelectable> _grabbedObjects = new List<ISceneSelectable>();

        public ICamera Camera
        {
            get { return _camera; }
            set { _camera = value; OnPropertyChanged(); }
        }

        public ISmartEditTarget SmartEditTarget
        {
            get { return _smartEditTarget; }
            set
            {
                var oldEditTarget = _smartEditTarget;
                _smartEditTarget = value;

                oldEditTarget?.NotifyAboutStateChange();
                _smartEditTarget?.NotifyAboutStateChange();

                OnPropertyChanged();
            }
        }

        public ObservableCollection<WorldObject> Objects
        {
            get { return _objects; }
            set
            {
                _objects = value;
                OnPropertyChanged();
                CreateObjectCollectionView();
            }
        }

        public IReadOnlyList<ISceneSelectable> GrabbedObjects => _grabbedObjects;

        private void CreateObjectCollectionView()
        {
            var collectionView = CollectionViewSource.GetDefaultView(Objects);
            collectionView.Filter = o => ((WorldObject) o).IsGrabable;
            ManageableObjects = collectionView;
        }

        public ICollectionView ManageableObjects
        {
            get { return _manageableObjects; }
            protected set { _manageableObjects = value; OnPropertyChanged(); }
        }

        public WorldObject Manipulator
        {
            get { return _manipulator; }
            set { _manipulator = value; OnPropertyChanged(); }
        }

        public string Filename { get; set; }

        public WorldObject FirstSelectedObject
        {
            get { return _firstSelectedObject; }
            set { _firstSelectedObject = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Scene()
        {
            Objects = new ObservableCollection<WorldObject>();
        }

        public void AttachObject(WorldObject worldObject)
        {
            if (worldObject == null) return;
            worldObject.SceneManager?.DetachObject(worldObject);
            worldObject.SceneManager = this;
            worldObject.PropertyChanged += OnWorldObjectChange;
            Objects.Add(worldObject);
        }

        public void DetachObject(IWorldObject worldObject)
        {
            if (worldObject?.SceneManager != this) return;
            if (worldObject.IsGrabbed)
                UngrabObject(worldObject as ISceneSelectable);
            worldObject.DetachFromCompositors();
            // todo: fix hack
            Objects.Remove((WorldObject)worldObject);
            worldObject.SceneManager = null;
            worldObject.PropertyChanged -= OnWorldObjectChange;
        }

        public void ToggleObjectGrab(ISceneSelectable selectable)
        {
            if (_grabbedObjects.Contains(selectable))
                UngrabObject(selectable);
            else GrabObject(selectable);
        }

        public void GrabObject(ISceneSelectable selectable)
        {
            if (selectable.IsGrabbed) return;
            selectable.IsGrabbed = true;
            _grabbedObjects.Add(selectable);
            OnPropertyChanged(nameof(GrabbedObjects));
        }

        public void UngrabObject(ISceneSelectable selectable)
        {
            if (!selectable.IsGrabbed) return;
            selectable.IsGrabbed = false;
            _grabbedObjects.Remove(selectable);
            OnPropertyChanged(nameof(GrabbedObjects));
        }

        public void UngrabAllObjects()
        {
            foreach (var grabbed in _grabbedObjects)
                grabbed.IsGrabbed = false;
            _grabbedObjects.Clear();
        }

        private void OnWorldObjectChange(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(WorldObject.IsSelected):
                    // todo: fix this workaround, need to pass OnPropertyChanged to parent somehow
                    FirstSelectedObject = Objects.FirstOrDefault(t => t.IsSelected);
                    break;
                default:
                    break;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public enum WorldObjectType
        {
            BezierCurve,
            BSplineCurve,
            Interpolation,
            BezierSurface,
            BSplineSurface,
        };

        public void Save()
        {
            if (string.IsNullOrWhiteSpace(Filename))
                throw new ArgumentException("Filename is empty.");

            var gatherer = new SceneDataSaver();
            gatherer.EmitInt(Objects.Count(t => t is ISaveable));
            gatherer.EmitEndOfLine();

            foreach (var worldObject in Objects)
            {
                var saveable = worldObject as ISaveable;
                saveable?.Save(gatherer);
            }

            var fileData = gatherer.Build();

            var writer = new StreamWriter(Filename);
            writer.Write(fileData);
            writer.Close();
        }

        public void Import(string sceneFile)
        {
            GlobalSettings.FreezeDrawing = true; //todo: fix hack
            var importer = new SceneDataImporter(sceneFile);
            importer.Import(this);
            GlobalSettings.FreezeDrawing = false;
        }

        public void SetObjectGrab(ISceneSelectable closestObject)
        {
            UngrabAllObjects();
            GrabObject(closestObject);
        }

        public void CollapseSelection()
        {
            var collapsables = _grabbedObjects
                .Where(t => t is VirtualPoint)
                .Cast<VirtualPoint>()
                .ToList();

            if (collapsables.Count < 2)
                throw new ApplicationException(string.Concat(
                    "There is not enough collapsables contained in selection ",
                    "to perform collapse operation."
                    ));

            var firstCollapsable = collapsables.First();
            var resultantPosition = (Vector3D) firstCollapsable.Position;

            foreach (var collapsable in collapsables.Skip(1))
            {
                resultantPosition += (Vector3D) collapsable.Position;
                collapsable.MergeInto(firstCollapsable);
            }

            firstCollapsable.Position =
                (Point3D) (resultantPosition/collapsables.Count);

            SetObjectGrab(firstCollapsable);
            OnPropertyChanged(nameof(GrabbedObjects));
        }
    }
}