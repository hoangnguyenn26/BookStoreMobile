using System.Globalization;

namespace Bookstore.Mobile.Converters
{
    public class StringToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str && double.TryParse(str, out double result))
                return result;
            return 1.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double num)
                return num.ToString(CultureInfo.InvariantCulture);
            return "1";
        }
    }
}