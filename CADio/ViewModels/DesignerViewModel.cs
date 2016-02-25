using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CADio.Rendering;

namespace CADio.ViewModels
{
    class DesignerViewModel
    {
        public DesignerViewModel()
        {
            ActiveRenderer = new Renderer();
        }

        public IRenderer ActiveRenderer { get; set; }
    }
}
