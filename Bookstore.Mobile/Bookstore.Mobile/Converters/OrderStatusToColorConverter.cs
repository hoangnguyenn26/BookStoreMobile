using Bookstore.Mobile.Enums;
using System.Globalization;

namespace Bookstore.Mobile.Converters
{
    public class OrderStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is OrderStatus status)
            {
                // Lấy màu từ ResourceDictionary hoặc định nghĩa trực tiếp
                Color defaultColor = Application.Current.RequestedTheme == AppTheme.Dark ? Colors.LightGray : Colors.DarkGray;
                if (Application.Current.Resources.TryGetValue("Gray600Light", out var colorValue))
                {
                    defaultColor = (Color)colorValue;
                }

                return status switch
                {
                    OrderStatus.Completed => Colors.Green,
                    OrderStatus.Cancelled => Colors.Red,
                    OrderStatus.Shipping => Colors.Orange,
                    OrderStatus.Confirmed => Colors.Blue,
                    OrderStatus.Pending => defaultColor,
                    _ => defaultColor,
                };
            }
            return Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}