using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Bookstore.Mobile.Converters
{
    public class BoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string param)
            {
                var texts = param.Split(',');
                if (texts.Length == 2)
                {
                    var trueText = texts[0].Trim();
                    var falseText = texts[1].Trim();
                    if (value is bool b)
                        return b ? trueText : falseText;
                }
            }
            return string.Empty;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
} 