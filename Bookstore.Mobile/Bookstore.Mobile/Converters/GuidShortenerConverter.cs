using System.Globalization;

namespace Bookstore.Mobile.Converters
{
    public class GuidShortenerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Guid guid)
            {
                // Lấy 8 ký tự đầu chẳng hạn
                return guid.ToString("N").Substring(0, 8).ToUpperInvariant();
            }
            return value?.ToString() ?? string.Empty;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}