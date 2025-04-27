namespace Bookstore.Mobile.Helpers
{
    public static class ImageUrlHelper
    {
        private const string ThumbnailSuffix = "_T";
        private const string MediumSuffix = "_M";

        private static string GetSizedImageUrlInternal(string? baseImageUrl, string requestedSuffix)
        {
            // Ảnh mặc định nếu URL gốc không hợp lệ
            const string defaultImage = "dotnet_bot.png";

            if (string.IsNullOrWhiteSpace(baseImageUrl))
            {
                System.Diagnostics.Debug.WriteLine("[ImageUrlHelper] BaseImageUrl is null or empty, returning default.");
                return defaultImage;
            }

            try
            {
                // Phân tích URL gốc
                var uri = new Uri(baseImageUrl);
                var fileName = Path.GetFileName(uri.LocalPath);
                var extension = Path.GetExtension(fileName);

                // Lấy tên file không có đuôi
                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);

                // --- Logic xác định tên file gốc (không có suffix kích thước) ---
                string baseName = fileNameWithoutExt;

                if (baseName.EndsWith(MediumSuffix, StringComparison.OrdinalIgnoreCase))
                {
                    baseName = baseName.Substring(0, baseName.Length - MediumSuffix.Length);
                }
                else if (baseName.EndsWith(ThumbnailSuffix, StringComparison.OrdinalIgnoreCase))
                {
                    baseName = baseName.Substring(0, baseName.Length - ThumbnailSuffix.Length);
                }
                var newFileName = $"{baseName}{requestedSuffix}{extension}";

                var directoryPath = Path.GetDirectoryName(uri.AbsolutePath)?.Replace('\\', '/').TrimStart('/') ?? "";

                var builder = new UriBuilder(uri.Scheme, uri.Host, uri.Port);
                builder.Path = !string.IsNullOrEmpty(directoryPath) ? $"/{directoryPath}/{newFileName}" : $"/{newFileName}";


                string finalUrl = builder.ToString();
                System.Diagnostics.Debug.WriteLine($"[ImageUrlHelper] Original: {baseImageUrl}, RequestedSuffix: {requestedSuffix}, Generated: {finalUrl}");
                return finalUrl;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ImageUrlHelper] Error processing URL '{baseImageUrl}': {ex.Message}");
                return baseImageUrl;
            }
        }
        public static string GetThumbnailUrl(string? baseUrl)
        {
            return GetSizedImageUrlInternal(baseUrl, ThumbnailSuffix);
        }

        public static string GetMediumUrl(string? baseUrl)
        {
            return GetSizedImageUrlInternal(baseUrl, MediumSuffix);
        }

        public static string GetOriginalUrl(string? baseUrl)
        {
            return GetSizedImageUrlInternal(baseUrl, "");
        }
    }
}