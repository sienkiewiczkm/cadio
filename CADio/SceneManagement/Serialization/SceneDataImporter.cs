using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Media.Media3D;
using CADio.Geometry.Shapes.Dynamic;
using CADio.Geometry.Shapes.Static;
using CADio.SceneManagement.Curves;
using CADio.SceneManagement.Points;
using CADio.SceneManagement.Surfaces;

namespace CADio.SceneManagement.Serialization
{
    public class SceneDataImporter
    {
        private readonly string _content;
        private int _contentCurrentIndex;
        private int _objectsNumber;

        private readonly List<SharedPoint3D> _referencePoints =
            new List<SharedPoint3D>();

        private int _referencePointsTotalCount;

        public int BaseIndex { get; private set; }

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
                var sharedPoint = new SharedPoint3D() { Data = point };

                _referencePoints.Add(sharedPoint);
            }

            _objectsNumber = ReadInt();
            for (var i = 0; i < _objectsNumber; ++i)
            {
                var objectTypeName = GetTextSegment();
                var objectName = GetToEndOfLine();

                var imported = MakeImportableObject(objectTypeName, objectName);
                scene.AttachObject(imported);

                var segment = GetTextSegment();
                if (segment != null && !segment.StartsWith("END"))
                    throw new ApplicationException(
                        "END expected and not found.");
            }

            // attach virutal points that are visible on list
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

        private WorldObject MakeImportableObject(
            string objectTypeName,
            string objectName)
        {
            objectTypeName = objectTypeName.Trim();
            objectName = objectName.Trim();

            if (objectTypeName == "BEZIERCURVE")
            {
                var beziercurve = new BezierC0WorldObject
                {
                    Name = objectName,
                    Shape = new BezierCurveC0()
                };

                var refPointsCount = ReadInt();
                for (var i = 0; i < refPointsCount; ++i)
                {
                    beziercurve.AttachObject(GetMarkerPoint(ReadInt()));
                }

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
                    bsplinecurve.AttachObject(GetMarkerPoint(ReadInt()));

                return bsplinecurve;
            }

            if (objectTypeName == "INTERP")
            {
                var interpolat = new InterpolatingBSplineObject
                {
                    Name = objectName,
                    Shape = new BezierCurveC2()
                };

                var refPointsCount = ReadInt();
                for (var i = 0; i < refPointsCount; ++i)
                    interpolat.AttachObject(GetMarkerPoint(ReadInt()));

                return interpolat;
            }

            if (objectTypeName == "BEZIERSURF" ||
                objectTypeName == "BSPLINESURF")
            {
                var controlPointsV = ReadInt();
                var controlPointsU = ReadInt();
                var folded = GetTextSegment()[0] == 'C';
                var correctDirection = GetTextSegment()[0] == 'H';

                var dataRows = controlPointsU;
                var dataColumns = controlPointsV;

                Func<int, int, Tuple<int, int>> mapper;
                if (correctDirection)
                {
                    mapper = (i, j) => new Tuple<int, int>(i, j);
                }
                else throw new ArgumentException("Only H option is supported");

                var data = new SharedPoint3D[dataRows, dataColumns];

                for (var row = 0; row < controlPointsU; ++row)
                {
                    for (var column = 0; column < controlPointsV; ++column)
                    {
                        var id = ReadInt();
                        var point = _referencePoints[id];
                        var mapped = mapper(row, column);
                        data[mapped.Item1, mapped.Item2] = point;
                    }
                }

                WorldObject surf;
                if (objectTypeName == "BEZIERSURF")
                {
                    var bezierSurf = new BezierSurfaceWorldObject
                    {
                        Name = objectName,
                        Shape = new BezierPatchGroup(),
                    };

                    bezierSurf.BuildFromExternalData(data, folded);
                    surf = bezierSurf;
                }
                else
                {
                    var bsplineSurf = new BSplineSurfaceWorldObject
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

            var segment = _content.Substring(
                _contentCurrentIndex,
                end - _contentCurrentIndex);
            _contentCurrentIndex = end + 1;
            return segment;
        }

        private void SkipWhitespace()
        {
            while (_contentCurrentIndex < _content.Length &&
                char.IsWhiteSpace(_content, _contentCurrentIndex))
                ++_contentCurrentIndex;
        }

        private string GetToEndOfLine()
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

            var segment = _content.Substring(
                _contentCurrentIndex,
                eol - _contentCurrentIndex);
            _contentCurrentIndex = eol + 1;
            return segment;
        }

        private WorldObject GetMarkerPoint(int refId)
        {
            return new WorldObject()
            {
                Name = "Point",
                Shape = new MarkerPoint(),
                Position = _referencePoints[refId].Data,
            };
        }
    }
}