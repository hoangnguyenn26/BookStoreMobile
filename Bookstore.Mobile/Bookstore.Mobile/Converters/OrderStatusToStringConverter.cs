using Bookstore.Mobile.Enums;
using System.Globalization;

namespace Bookstore.Mobile.Converters
{
    public class OrderStatusToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Kiểm tra xem giá trị đầu vào có phải là OrderStatus không
            if (value is OrderStatus status)
            {
                // Sử dụng switch expression để trả về chuỗi tương ứng
                return status switch
                {
                    OrderStatus.Pending => "Pending Payment",
                    OrderStatus.Confirmed => "Processing",
                    OrderStatus.Shipping => "Shipping",
                    OrderStatus.Completed => "Completed",
                    OrderStatus.Cancelled => "Cancelled",
                    _ => status.ToString() // Mặc định trả về tên của Enum nếu không khớp case nào
                };
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Converting back from string to OrderStatus is not implemented.");
        }
    }
}