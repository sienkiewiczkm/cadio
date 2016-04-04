using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CADio.Geometry
{
    public class Mesh
    {
        public IList<Vertex> Vertices { get; set; }
        public IList<IndexedLine> Indices { get; set; }
        public Color Color { get; set; }
    }
}
