using System.Globalization;

namespace Bookstore.Mobile.Converters
{
    public class ImageUrlConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            string? originalUrl = value as string;
            string sizeKey = parameter as string ?? "Medium";

            switch (sizeKey.ToUpperInvariant())
            {
                case "THUMB":
                case "T":
                    return ImageUrlHelper.GetThumbnailUrl(originalUrl);
                case "MEDIUM":
                case "M":
                    return ImageUrlHelper.GetMediumUrl(originalUrl);
                case "ORIGINAL":
                    return ImageUrlHelper.GetOriginalUrl(originalUrl);
                default:
                    return ImageUrlHelper.GetMediumUrl(originalUrl);
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}