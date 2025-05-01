using System.Globalization;

namespace Bookstore.Mobile.Converters
{
    public class GuidNotNullBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Guid guid)
            {
                return guid != Guid.Empty;
            }
            if (value is Guid?)
            {
                return ((Guid?)value).HasValue && ((Guid?)value).Value != Guid.Empty;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}