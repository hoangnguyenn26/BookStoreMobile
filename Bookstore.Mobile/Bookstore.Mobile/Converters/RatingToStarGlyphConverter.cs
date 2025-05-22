using System.Globalization;

namespace Bookstore.Mobile.Converters
{
    public class RatingToStarGlyphConverter : IValueConverter
    {
        private const string StarFilledGlyph = "★"; // Hoặc "\u2605"
        private const string StarOutlineGlyph = "☆"; // Hoặc "\u2606"

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int currentRating && parameter is int starValue)
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