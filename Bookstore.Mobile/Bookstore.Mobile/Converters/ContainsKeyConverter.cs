using System.Collections;
using System.Globalization;

namespace Bookstore.Mobile.Converters
{
    public class ContainsKeyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            // Trường hợp cho Dictionary<string, string>
            if (value is Dictionary<string, string> stringDict)
            {
                return stringDict.ContainsKey(parameter.ToString());
            }

            // Trường hợp cho Dictionary<string, object>
            if (value is Dictionary<string, object> objDict)
            {
                return objDict.ContainsKey(parameter.ToString());
            }

            // Trường hợp cho IDictionary (interface tổng quát hơn)
            if (value is IDictionary dict)
            {
                return dict.Contains(parameter);
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("ConvertBack không được hỗ trợ cho ContainsKeyConverter");
        }
    }
}