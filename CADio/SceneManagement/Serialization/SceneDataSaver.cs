using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Media.Media3D;

namespace CADio.SceneManagement.Serialization
{
    public class SceneDataSaver
    {
        private Dictionary<WorldObject, int> WorldObjectIdMap { get; set; } 
            = new Dictionary<WorldObject, int>();
        public List<Point3D> ReferencePoints { get; set; } 
            = new List<Point3D>();
        public StringBuilder ObjectDataSet { get; set; } = new StringBuilder();

        private string TranslateWorldObjectType(Scene.WorldObjectType type)
        {
            switch (type)
            {
                case Scene.WorldObjectType.BezierCurve:
                    return "BEZIERCURVE";
                case Scene.WorldObjectType.BSplineCurve:
                    return "BSPLINECURVE";
                case Scene.WorldObjectType.Interpolation:
                    return "INTERP";
                case Scene.WorldObjectType.BezierSurface:
                    return "BEZIERSURF";
                case Scene.WorldObjectType.BSplineSurface:
                    return "BSPLINESURF";
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(type),
                        type,
                        null
                    );
            }
        }

        public void EmitObjectInfo(Scene.WorldObjectType type, string name)
        {
            ObjectDataSet.AppendFormat(
                "{0} {1}\n",
                TranslateWorldObjectType(type),
                name
            );
        }

        public void EmitInt(int i)
        {
            ObjectDataSet.AppendFormat("{0} ", i);
        }

        public void EmitDouble(double i)
        {
            ObjectDataSet.AppendFormat(
                "{0} ",
                i.ToString(CultureInfo.InvariantCulture)
            );
        }

        public void EmitChar(char c)
        {
            ObjectDataSet.AppendFormat("{0} ", c);
        }

        public void EmitObjectDataEnd()
        {
            ObjectDataSet.Append("\nEND\n");
        }

        public void EmitEndOfLine()
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
                file.AppendFormat(
                    "{0} {1} {2}\n",
                    rp.X.ToString(CultureInfo.InvariantCulture), 
                    rp.Y.ToString(CultureInfo.InvariantCulture),
                    rp.Z.ToString(CultureInfo.InvariantCulture)
                );
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
}