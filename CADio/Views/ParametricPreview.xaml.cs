using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CADio.ViewModels;

namespace CADio.Views
{
    /// <summary>
    /// Interaction logic for ParametricPreview.xaml
    /// </summary>
    public partial class ParametricPreview : Window
    {
        public ParametricPreviewViewModel ViewModel
            = new ParametricPreviewViewModel();

        public ParametricPreview()
        {
            InitializeComponent();
            DataContext = ViewModel;
        }
    }
}
