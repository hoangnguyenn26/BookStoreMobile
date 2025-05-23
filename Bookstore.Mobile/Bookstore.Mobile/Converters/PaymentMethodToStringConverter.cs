using Bookstore.Mobile.Enums;
using System.Globalization;

namespace Bookstore.Mobile.Converters
{
    public class PaymentMethodToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PaymentMethod paymentMethod)
            {
                return paymentMethod switch
                {
                    PaymentMethod.Cash => "Cash",
                    PaymentMethod.Card => "Credit/Debit Card",
                    PaymentMethod.Transfer => "Bank Transfer",
                    _ => paymentMethod.ToString()
                };
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}