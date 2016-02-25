using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace CADio.Rendering
{
    public class Renderer : IRenderer
    {
        public IList<Line2D> GetRenderedPrimitives()
        {
            return new List<Line2D>()
            {
                new Line2D(new Point(0, 0), new Point(32, 32), Colors.Green),
                new Line2D(new Point(32, 32), new Point(48, 64), Colors.Aquamarine),
            };
        }
    }
}
