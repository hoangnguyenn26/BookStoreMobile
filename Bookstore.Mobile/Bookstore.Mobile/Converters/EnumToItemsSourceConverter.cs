using System.Globalization;

namespace Bookstore.Mobile.Converters
{
    public class EnumToItemsSourceConverter : IValueConverter
    {
        public required Type EnumType { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (EnumType == null || !EnumType.IsEnum)
                return new List<string>();

            var items = Enum.GetValues(EnumType)
                          .Cast<object>()
                          .Select(e => e.ToString())
                          .ToList();

            if (parameter is string allText && !string.IsNullOrWhiteSpace(allText))
            {
                items.Insert(0, allText);
            }

            return items;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString();
        }
    }
}