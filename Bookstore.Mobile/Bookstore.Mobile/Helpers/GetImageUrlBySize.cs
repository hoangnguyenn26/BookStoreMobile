public static class ImageUrlHelper
{
    private const string ThumbnailSuffix = "_T";
    private const string MediumSuffix = "_M";

    public static string GetSizedImageUrl(string? baseUrl, string sizeSuffix) // sizeSuffix là "_T", "_M" hoặc "" (cho gốc)
    {
        if (string.IsNullOrWhiteSpace(baseUrl)) return "dotnet_bot.png";

        try
        {
            var uri = new Uri(baseUrl);
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(uri.LocalPath);
            var extension = Path.GetExtension(uri.LocalPath);

            // Xóa các suffix cũ nếu có
            if (fileNameWithoutExt.EndsWith(MediumSuffix))
                fileNameWithoutExt = fileNameWithoutExt.Substring(0, fileNameWithoutExt.Length - MediumSuffix.Length);
            else if (fileNameWithoutExt.EndsWith(ThumbnailSuffix))
                fileNameWithoutExt = fileNameWithoutExt.Substring(0, fileNameWithoutExt.Length - ThumbnailSuffix.Length);

            // Tạo tên file mới
            var newFileName = $"{fileNameWithoutExt}{sizeSuffix}{extension}";

            // Lấy phần đường dẫn thư mục
            var directoryPath = Path.GetDirectoryName(uri.LocalPath)?.Replace('\\', '/').TrimStart('/') ?? "";

            // Tạo URL mới
            var builder = new UriBuilder(uri.Scheme, uri.Host, uri.Port, Path.Combine(directoryPath, newFileName).Replace('\\', '/'));
            return builder.ToString();

        }
        catch (Exception)
        {
            return baseUrl;
        }
    }

    public static string GetThumbnailUrl(string? baseUrl) => GetSizedImageUrl(baseUrl, ThumbnailSuffix);
    public static string GetMediumUrl(string? baseUrl) => GetSizedImageUrl(baseUrl, MediumSuffix);
    public static string GetOriginalUrl(string? baseUrl) => GetSizedImageUrl(baseUrl, "");
}