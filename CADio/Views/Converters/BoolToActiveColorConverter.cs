using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Brushes = System.Drawing.Brushes;

namespace CADio.Views.Converters
{
    public class BoolToActiveColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool))
                return null;

            var boolValue = (bool) value;
            return boolValue 
                ? new SolidColorBrush(Colors.LightGreen) 
                : new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}