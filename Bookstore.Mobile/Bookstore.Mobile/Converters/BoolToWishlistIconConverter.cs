using System.Globalization;

namespace Bookstore.Mobile.Converters
{
    public class BoolToWishlistIconConverter : IValueConverter
    {
        private const string FilledHeartGlyph = "\ue87d"; // Mã glyph cho icon "favorite" (trái tim đầy)
        private const string EmptyHeartGlyph = "\ue87e";  // Mã glyph cho icon "favorite_border" (trái tim rỗng)

        // Chuyển đổi từ bool (IsInWishlist) sang string (Mã Glyph Icon)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isInWishlist)
            {
                return isInWishlist ? FilledHeartGlyph : EmptyHeartGlyph;
            }
            return EmptyHeartGlyph;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Converting back from icon to boolean is not supported.");
        }
    }
}