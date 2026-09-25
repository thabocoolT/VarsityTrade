namespace VarsityTrade.Web.Helpers
{
    // Helper for resolving image URLs from the API
    public static class ImageHelper
    {
        // API base URL — matches appsettings.json ApiSettings:BaseUrl
        private const string ApiBaseUrl = "https://localhost:7019";

        // Resolves a relative image path from the API to a full URL
        // If already a full URL returns as-is
        public static string ResolveUrl(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return string.Empty;

            if (imageUrl.StartsWith("http://") || imageUrl.StartsWith("https://"))
                return imageUrl;

            // Relative path — prefix with API base URL
            return $"{ApiBaseUrl}{imageUrl}";
        }

        // Returns a resolved URL or empty string if null
        public static string? ResolveNullable(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return null;

            return ResolveUrl(imageUrl);
        }
    }
}