using Bookstore.Mobile.Enums;
using System.Globalization;

namespace Bookstore.Mobile.Converters
{
    public class OrderStatusToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;

            return value switch
            {
                OrderStatus.Pending => "Chờ xử lý",
                OrderStatus.Confirmed => "Đã xác nhận",
                OrderStatus.Shipping => "Đang giao hàng",
                OrderStatus.Completed => "Giao thành công",
                OrderStatus.Cancelled => "Đã hủy",
                _ => value.ToString()
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}