using System.Globalization;

namespace Bookstore.Mobile.Converters
{
    public class RatingToStarColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int position && parameter is int currentRating)
            {
                // Return gold for filled stars, gray for empty stars
                return position <= currentRating ? Colors.Gold : Colors.DarkGray;
            }
            // Default color
            return Colors.DarkGray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}