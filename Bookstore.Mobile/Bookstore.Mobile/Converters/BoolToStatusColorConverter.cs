using System.Globalization;

namespace Bookstore.Mobile.Converters
{
    public class BoolToStatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color activeColor = Colors.Green;
            Color inactiveColor = Colors.Red;
            Color defaultColor = Colors.Gray;

            if (Application.Current != null)
            {
                if (Application.Current.Resources.TryGetValue("SuccessColor", out var activeRes) && activeRes is Color ac)
                    activeColor = ac;
                if (Application.Current.Resources.TryGetValue("ErrorColor", out var inactiveRes) && inactiveRes is Color ic)
                    inactiveColor = ic;
                if (Application.Current.Resources.TryGetValue("Gray500", out var defaultRes) && defaultRes is Color dc) // Giả sử có Gray500
                    defaultColor = dc;
            }


            if (value is bool isActive)
            {
                return isActive ? activeColor : inactiveColor;
            }
            return defaultColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}