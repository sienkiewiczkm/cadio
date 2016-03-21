// source: http://andyonwpf.blogspot.com/2006/10/dropdownbuttons-in-wpf.html

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace CADio.Views
{
    public class DropDownButton : ToggleButton
    {
        public static readonly DependencyProperty DropDownProperty
            = DependencyProperty.Register("DropDown", typeof (ContextMenu), typeof (DropDownButton),
                new UIPropertyMetadata(null));

        public DropDownButton()
        {
            var binding = new Binding("DropDown.IsOpen") {Source = this};
            SetBinding(IsCheckedProperty, binding);
        }

        public ContextMenu DropDown
        {
            get { return (ContextMenu) GetValue(DropDownProperty); }
            set { SetValue(DropDownProperty, value); }
        }

        protected override void OnClick()
        {
            if (DropDown == null) return;

            DropDown.PlacementTarget = this;
            DropDown.Placement = PlacementMode.Bottom;
            DropDown.IsOpen = true;
        }
    }
}
