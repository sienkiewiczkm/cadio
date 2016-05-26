using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Media3D;
using CADio.Geometry.Shapes;
using CADio.Geometry.Shapes.Dynamic;
using CADio.Geometry.Shapes.Static;
using CADio.Mathematics;
using CADio.Rendering;

namespace CADio.SceneManagement
{
    public class Scene : INotifyPropertyChanged
    {
        private ISceneSelectable _grabbedObject;
        private WorldObject _manipulator;
        private ObservableCollection<WorldObject> _objects;
        private ICamera _camera = new ArcBallCamera();
        private ICollectionView _manageableObjects;
        private WorldObject _firstSelectedObject;
        private ISmartEditTarget _smartEditTarget;

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

        public ISceneSelectable GrabbedObject
        {
            get { return _grabbedObject; }
            set
            {
                if (_grabbedObject != null)
                    _grabbedObject.IsGrabbed = false;
                _grabbedObject = value;
                if (_grabbedObject != null)
                    _grabbedObject.IsGrabbed = true;
                OnPropertyChanged();
            }
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
            if (GrabbedObject == worldObject)
                GrabbedObject = null;
            worldObject.DetachFromCompositors();
            // todo: fix hack
            Objects.Remove((WorldObject)worldObject);
            worldObject.SceneManager = null;
            worldObject.PropertyChanged -= OnWorldObjectChange;
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

        public class SceneDataGatherer
        {
            private Dictionary<WorldObject, int> WorldObjectIdMap { get; set; } = new Dictionary<WorldObject, int>();
            public List<Point3D> ReferencePoints { get; set; } = new List<Point3D>();
            public StringBuilder ObjectDataSet { get; set; } = new StringBuilder();

            private string TranslateWorldObjectType(WorldObjectType type)
            {
                switch (type)
                {
                    case WorldObjectType.BezierCurve:
                        return "BEZIERCURVE";
                    case WorldObjectType.BSplineCurve:
                        return "BSPLINECURVE";
                    case WorldObjectType.Interpolation:
                        return "INTERP";
                    case WorldObjectType.BezierSurface:
                        return "BEZIERSURF";
                    case WorldObjectType.BSplineSurface:
                        return "BSPLINESURF";
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            }

            public void EmitObjectInfo(WorldObjectType type, string name)
            {
                ObjectDataSet.AppendFormat("{0} {1}\n", TranslateWorldObjectType(type), name);
            }

            public void EmitInt(int i)
            {
                ObjectDataSet.AppendFormat("{0} ", i);
            }

            public void EmitDouble(double i)
            {
                ObjectDataSet.AppendFormat("{0} ", i.ToString(CultureInfo.InvariantCulture));
            }

            public void EmitChar(char c)
            {
                ObjectDataSet.AppendFormat("{0} ", c);
            }

            public void EmitObjectDataEnd()
            {
                ObjectDataSet.Append("\nEND\n");
            }

            public void EmitEOL()
            {
                ObjectDataSet.AppendLine();
            }

            public int GetWorldObjectId(WorldObject worldObject)
            {
                int value;
                if (WorldObjectIdMap.TryGetValue(worldObject, out value))
                    return value;

                value = ReferencePoints.Count;
                WorldObjectIdMap.Add(worldObject, value);
                ReferencePoints.Add(worldObject.Position);
                return value;
            }

            public string Build()
            {
                var file = new StringBuilder();
                file.AppendFormat("{0}\n", ReferencePoints.Count);
                foreach (var rp in ReferencePoints)
                    file.AppendFormat("{0} {1} {2}\n", rp.X.ToString(CultureInfo.InvariantCulture), 
                        rp.Y.ToString(CultureInfo.InvariantCulture), rp.Z.ToString(CultureInfo.InvariantCulture));
                file.AppendFormat(ObjectDataSet.ToString());
                return file.ToString();
            }

            public int CreateReferencePoint(Point3D pos)
            {
                var id = ReferencePoints.Count;
                ReferencePoints.Add(pos);
                return id;
            }
        }

        public void Save()
        {
            if (string.IsNullOrWhiteSpace(Filename))
                throw new ArgumentException("Filename is empty.");

            var gatherer = new SceneDataGatherer();
            gatherer.EmitInt(Objects.Count(t => t is ISaveable));
            gatherer.EmitEOL();

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

        public class SceneDataImporter
        {
            private readonly string _content;
            private int _referencePointsTotalCount;
            private List<WorldObject> _referencePoints = new List<WorldObject>();
            private List<WorldObject> _redundantPoints = new List<WorldObject>();
            private int _contentCurrentIndex;
            private int _objectsNumber;

            public int BaseIndex { get; private set; } = 0;

            private void SkipWhitespace()
            {
                while (_contentCurrentIndex < _content.Length && char.IsWhiteSpace(_content, _contentCurrentIndex))
                    ++_contentCurrentIndex;
            }

            public int ReadInt()
            {
                SkipWhitespace();
                var segment = GetTextSegment();
                return int.Parse(segment, CultureInfo.InvariantCulture);
            }

            public double ReadDouble()
            {
                SkipWhitespace();
                var segment = GetTextSegment().Replace(',', '.');
                return double.Parse(segment, CultureInfo.InvariantCulture);
            }

            private string GetTextSegment()
            {
                SkipWhitespace();
                if (_contentCurrentIndex >= _content.Length)
                    return null;

                var end = Math.Min(
                    _content.IndexOf(' ', _contentCurrentIndex), 
                    _content.IndexOf('\n', _contentCurrentIndex)
                );

                if (end == -1)
                {
                    var rest = _content.Substring(_contentCurrentIndex);
                    _contentCurrentIndex = _content.Length;
                    return rest;
                }

                var segment = _content.Substring(_contentCurrentIndex, end - _contentCurrentIndex);
                _contentCurrentIndex = end + 1;
                return segment;
            }

            private string GetToEOL()
            {
                SkipWhitespace();
                if (_contentCurrentIndex >= _content.Length)
                    return null;

                var eol = _content.IndexOf('\n', _contentCurrentIndex);
                if (eol == -1)
                {
                    var rest = _content.Substring(_contentCurrentIndex);
                    _contentCurrentIndex = _content.Length;
                    return rest;
                }

                var segment = _content.Substring(_contentCurrentIndex, eol - _contentCurrentIndex);
                _contentCurrentIndex = eol + 1;
                return segment;
            }

            public SceneDataImporter(string filename)
            {
                var reader = new StreamReader(filename);
                _content = reader.ReadToEnd();
                reader.Close();
            }

            public void Import(Scene scene)
            {
                BaseIndex = scene.Objects.Count;

                _referencePointsTotalCount = ReadInt();
                for (var i = 0; i < _referencePointsTotalCount; ++i)
                {
                    var x = ReadDouble();
                    var y = ReadDouble();
                    var z = ReadDouble();
                    var point = new Point3D(x, y, z);

                    var pointObject = new WorldObject()
                    {
                        Name = $"Point {i}",
                        Position = point,
                        Shape = new MarkerPoint(),
                    };

                    _referencePoints.Add(pointObject);
                    scene.AttachObject(pointObject);
                }

                _objectsNumber = ReadInt();
                for (var i = 0; i < _objectsNumber; ++i)
                {
                    var objectTypeName = GetTextSegment();
                    var objectName = GetToEOL();

                    var imported = MakeImportableObject(objectTypeName, objectName);
                    scene.AttachObject(imported);

                    var segment = GetTextSegment();
                    if (segment != null && !segment.StartsWith("END"))
                        throw new ApplicationException("END expected and not found.");
                }

                foreach (var pt in _redundantPoints)
                    scene.DetachObject(pt);
            }

            private WorldObject MakeImportableObject(string objectTypeName, string objectName)
            {
                if (objectTypeName == "BEZIERCURVE")
                {
                    var beziercurve = new BezierC0WorldObject
                    {
                        Name = objectName,
                        Shape = new BezierCurveC0()
                    };

                    var refPointsCount = ReadInt();
                    for (var i = 0; i < refPointsCount; ++i)
                        beziercurve.AttachObject(_referencePoints[ReadInt()]);

                    return beziercurve;
                }

                if (objectTypeName == "BSPLINECURVE")
                {
                    var bsplinecurve = new BezierC2WorldObject
                    {
                        Name = objectName,
                        Shape = new BezierCurveC2()
                    };

                    var refPointsCount = ReadInt();
                    for (var i = 0; i < refPointsCount; ++i)
                        bsplinecurve.AttachObject(_referencePoints[ReadInt()]);

                    return bsplinecurve;
                }

                if (objectTypeName == "INTERP")
                {
                    var interpolat = new InterpolatingBSplineObject()
                    {
                        Name = objectName,
                        Shape = new BezierCurveC2()
                    };

                    var refPointsCount = ReadInt();
                    for (var i = 0; i < refPointsCount; ++i)
                        interpolat.AttachObject(_referencePoints[ReadInt()]);

                    return interpolat;
                }

                if (objectTypeName == "BEZIERSURF" ||
                    objectTypeName == "BSPLINESURF")
                {
                    var rows = ReadInt();
                    var columns = ReadInt();
                    var folded = GetTextSegment()[0] == 'C';
                    var flipDirection = GetTextSegment()[0] == 'H' && false;

                    var dataRows = !flipDirection ? rows : columns;
                    var dataColumns = !flipDirection ? columns : rows;

                    Func<int, int, Tuple<int, int>> mapper;
                    if (!flipDirection)
                    {
                        mapper = (i, j) => new Tuple<int, int>(i, j);
                    }
                    else
                    {
                        mapper = (i, j) => new Tuple<int, int>(j, i);
                    }

                    var data = new Point3D[dataRows, dataColumns];

                    for (var row = 0; row < rows; ++row)
                    {
                        for (var column = 0; column < columns; ++column)
                        {
                            var id = ReadInt();
                            var point = _referencePoints[id];
                            _redundantPoints.Add(point);

                            var mapped = mapper(row, column);
                            data[mapped.Item1, mapped.Item2] = point.Position;
                        }
                    }

                    WorldObject surf;
                    if (objectTypeName == "BEZIERSURF")
                    {
                        var bezierSurf = new BezierSurfaceWorldObject()
                        {
                            Name = objectName,
                            Shape = new BezierPatchGroup(),
                        };

                        bezierSurf.BuildFromExternalData(data, folded);
                        surf = bezierSurf;
                    }
                    else
                    {
                        var bsplineSurf = new BSplineSurfaceWorldObject()
                        {
                            Name = objectName,
                            Shape = new BSplinePatchGroup(),
                        };

                        bsplineSurf.BuildFromExternalData(data, folded);
                        surf = bsplineSurf;
                    }

                    return surf;
                }

                return null;
            }
        }

        public void Import(string sceneFile)
        {
            GlobalSettings.FreezeDrawing = true; //todo: fix hack
            var importer = new SceneDataImporter(sceneFile);
            importer.Import(this);
            GlobalSettings.FreezeDrawing = false;
        }
    }
}