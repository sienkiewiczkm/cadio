using System.Windows;
using CADio.ViewModels;

namespace CADio.Views
{
    /// <summary>
    /// Interaction logic for DesignerWindow.xaml
    /// </summary>
    public partial class DesignerWindow : Window
    {
        public DesignerWindow()
        {
            InitializeComponent();
            DataContext = new DesignerViewModel();
        }
    }
}
