using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using CADio.Configuration;
using CADio.SceneManagement.Surfaces;

namespace CADio.Geometry.Shapes.Dynamic
{
    public abstract class SurfacePatch : IDynamicShape
    {
        public virtual string Name => "Surface Patch";

        public IList<Vertex> Vertices { get; private set; }
        public IList<IndexedLine> Lines { get; private set; }
        public IList<Vertex> MarkerPoints { get; private set; }
        public IList<Vertex> RawPoints { get; private set; }
        public bool IsPolygonRenderingEnabled { get; set; }

        public Control GetEditorControl() => null;

        /// <summary>
        /// 16 Control Points
        /// </summary>
        public List<SurfaceControlPoint> ControlPoints { get; set; }

        public void UpdateGeometry(Func<Point3D, Point3D, double> estimateScreenDistanceWithoutClip, Predicate<Point3D> isInsideProjectiveCubePredicate)
        {
            RawPoints = new List<Vertex>();
            MarkerPoints = new List<Vertex>();
            Vertices = new List<Vertex>();
            Lines = new List<IndexedLine>();
            
            if (ControlPoints.Count != 16)
                return;

            // generate subdivisions
            var vertices = new List<Vertex>();
            var lines = new List<IndexedLine>();

            CreateDirectionalSurfaceSampling(
                estimateScreenDistanceWithoutClip, 
                (i, j) => new Tuple<int, int>(i, j), 
                GlobalSettings.QualitySettingsViewModel.SurfaceWSubdivisions, 
                vertices, 
                lines
            );

            CreateDirectionalSurfaceSampling(
                estimateScreenDistanceWithoutClip,
                (i, j) => new Tuple<int, int>(j, i),
                GlobalSettings.QualitySettingsViewModel.SurfaceHSubdivisions, 
                vertices, 
                lines
            );

            if (IsPolygonRenderingEnabled)
            {
                var additionalVertices = ControlPoints
                    .Select(t => new Vertex(t.Position, Colors.Gray))
                    .ToList();
                var additionalLines = new List<IndexedLine>();
                var baseVertex = vertices.Count;

                for (var x = 0; x < 4; ++x)
                {
                    for (var y = 0; y < 4; ++y)
                    {
                        if (x < 3)
                        {
                            additionalLines.Add(new IndexedLine(baseVertex + (4*y + x), baseVertex + (4*y + x + 1)));
                        }
                        if (y < 3)
                        {
                            additionalLines.Add(new IndexedLine(baseVertex + (4*y + x), baseVertex + (4*(y + 1) + x)));
                        }
                    }
                }

                vertices.AddRange(additionalVertices);
                lines.AddRange(additionalLines);
            }

            MarkerPoints = ControlPoints.Select(t => new Vertex(
                    t.Position, 
                    t.ColorOverride ?? Colors.White
                )).ToList();
            Vertices = vertices;//Vertices.Concat(additionalVertices).ToList();
            Lines = lines; //Lines.Concat(additionalLines).ToList();
        }

        protected abstract void CreateDirectionalSurfaceSampling(
            Func<Point3D, Point3D, double> estimateScreenDistanceWithoutClip,
            Func<int, int, Tuple<int, int>> mapping,
            int subdivisions, List<Vertex> vertices, List<IndexedLine> lines);
    }
}