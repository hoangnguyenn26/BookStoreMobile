using System.Globalization;

namespace Bookstore.Mobile.Converters
{
    public class StockToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color returnColor = Colors.Gray;

            if (value is int stock)
            {
                if (stock == 0)
                {
                    // Hết hàng -> Màu Error
                    App.Current.Resources.TryGetValue("ErrorLight", out var errorColor);
                    returnColor = errorColor as Color ?? Colors.Red;
                }
                else if (stock <= 5)
                {
                    // Sắp hết -> Màu Warning
                    App.Current.Resources.TryGetValue("WarningLight", out var warningColor);
                    returnColor = warningColor as Color ?? Colors.OrangeRed;
                }
                else
                {
                    // Còn hàng -> Màu thành công hoặc màu mặc định
                    App.Current.Resources.TryGetValue("SuccessLight", out var successColor);
                    returnColor = successColor as Color ?? Colors.DarkGreen;
                }
            }
            return returnColor;
        }

        // Chuyển đổi ngược lại không cần thiết
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}