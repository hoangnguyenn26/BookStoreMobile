using System.Globalization;
namespace Bookstore.Mobile.Converters
{
    public class PositiveNegativeColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                if (intValue > 0) return Colors.Green;
                if (intValue < 0) return Colors.Red;
            }
            if (value is decimal decValue)
            {
                if (decValue > 0) return Colors.Green;
                if (decValue < 0) return Colors.Red;
            }
            if (Application.Current != null && Application.Current.Resources.TryGetValue("OnSurfaceColor", out var onSurface) && onSurface is Color defaultTextColor)
                return defaultTextColor;
            return Colors.Gray;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}