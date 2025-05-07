using System.Collections;
using System.Globalization;

namespace Bookstore.Mobile.Converters
{
    public class ListToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable collection)
            {
                var stringList = collection.OfType<object>()
                                         .Select(item => item?.ToString())
                                         .Where(s => !string.IsNullOrEmpty(s));
                return string.Join(", ", stringList);
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}