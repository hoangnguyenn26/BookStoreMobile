using System.Globalization;

namespace Bookstore.Mobile.Converters
{
    public class RatingToStarIconConverter : IValueConverter
    {
        private const string StarFilledGlyph = "\ue838"; // star
        private const string StarOutlineGlyph = "\ue83a"; // star_outline

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int starValue && parameter is int currentRating)
            {
                return starValue <= currentRating ? StarFilledGlyph : StarOutlineGlyph;
            }
            return StarOutlineGlyph;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}