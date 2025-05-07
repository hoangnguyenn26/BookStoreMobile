using System.Globalization;

namespace Bookstore.Mobile.Converters
{

    public class ObjectNotNullBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Trả về true nếu đối tượng binding khác null
            return value != null;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Converting from boolean back to object is not supported.");
        }
    }
}