using System.Text.RegularExpressions;

namespace BMDb.Services
{
    public class YouTubeTrailerServis : ITrailerServis
    {
        public string PokreniTrailer(string link)
        {
            if (string.IsNullOrWhiteSpace(link))
            {
                return string.Empty;
            }

            var trimmed = link.Trim();
            var id = ExtractVideoId(trimmed);
            return string.IsNullOrWhiteSpace(id)
                ? string.Empty
                : $"https://www.youtube.com/embed/{id}?rel=0";
        }

        private static string ExtractVideoId(string link)
        {
            if (Regex.IsMatch(link, "^[a-zA-Z0-9_-]{11}$"))
            {
                return link;
            }

            if (!Uri.TryCreate(link, UriKind.Absolute, out var uri))
            {
                return string.Empty;
            }

            if (uri.Host.Contains("youtu.be"))
            {
                return uri.AbsolutePath.Trim('/').Split('/').FirstOrDefault() ?? string.Empty;
            }

            if (uri.Host.Contains("youtube.com"))
            {
                if (uri.AbsolutePath.StartsWith("/embed/", StringComparison.OrdinalIgnoreCase))
                {
                    return uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries).Skip(1).FirstOrDefault() ?? string.Empty;
                }

                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                return query["v"] ?? string.Empty;
            }

            return string.Empty;
        }
    }
}
