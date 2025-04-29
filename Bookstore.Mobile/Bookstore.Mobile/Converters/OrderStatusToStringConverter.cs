using Bookstore.Mobile.Enums;
using System.Globalization;

namespace Bookstore.Mobile.Converters
{
    public class OrderStatusToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is OrderStatus status)
            {
                // Thêm các case khác nếu cần
                return status switch
                {
                    OrderStatus.Pending => "Pending Payment",
                    OrderStatus.Confirmed => "Processing",
                    OrderStatus.Shipping => "Shipping",
                    OrderStatus.Completed => "Completed",
                    OrderStatus.Cancelled => "Cancelled",
                    _ => status.ToString()
                };
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}