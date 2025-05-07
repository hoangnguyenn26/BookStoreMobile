using System.Globalization;

namespace Bookstore.Mobile.Converters
{
    public class BoolToStatusTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isActive)
            {
                return isActive ? "Active" : "Inactive";
            }
            return "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string statusText)
            {
                return statusText.Equals("Active", StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }
    }
}