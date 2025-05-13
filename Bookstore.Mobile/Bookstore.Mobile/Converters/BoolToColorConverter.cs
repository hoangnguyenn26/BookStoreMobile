using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Bookstore.Mobile.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string colorParams)
            {
                var colors = colorParams.Split(',');
                if (colors.Length == 2)
                {
                    var trueColor = Color.FromArgb(colors[0].Trim());
                    var falseColor = Color.FromArgb(colors[1].Trim());
                    if (value is bool b)
                        return b ? trueColor : falseColor;
                }
            }
            return Colors.Transparent;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
} 