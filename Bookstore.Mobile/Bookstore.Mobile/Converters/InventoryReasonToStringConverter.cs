// Bookstore.Mobile/Converters/InventoryReasonToStringConverter.cs
using Bookstore.Mobile.Enums;
using System.Globalization;

namespace Bookstore.Mobile.Converters
{
    public class InventoryReasonToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is InventoryReason reason)
            {
                return reason switch
                {
                    InventoryReason.StockReceipt => "Stock Receipt",
                    InventoryReason.OnlineSale => "Online Sale",
                    InventoryReason.InStoreSale => "In-Store Sale",
                    InventoryReason.OrderCancellation => "Order Cancellation",
                    InventoryReason.Return => "Customer Return",
                    InventoryReason.Adjustment => "Adjustment",
                    InventoryReason.InitialStock => "Initial Stock",
                    _ => reason.ToString()
                };
            }
            return string.Empty;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}