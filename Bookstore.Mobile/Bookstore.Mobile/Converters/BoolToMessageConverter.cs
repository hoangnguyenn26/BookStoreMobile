using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Bookstore.Mobile.Converters
{
    public class BoolToMessageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string param)
            {
                var messages = param.Split(',');
                if (messages.Length == 2)
                {
                    var trueMsg = messages[0].Trim();
                    var falseMsg = messages[1].Trim();
                    if (value is bool b)
                        return b ? trueMsg : falseMsg;
                }
            }
            return string.Empty;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
} 