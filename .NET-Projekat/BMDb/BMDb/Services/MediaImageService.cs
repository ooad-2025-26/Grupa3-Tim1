using Microsoft.AspNetCore.Hosting;

namespace BMDb.Services
{
    public interface IMediaImageService
    {
        string ResolvePosterUrl(string? posterPath);
    }

    public class MediaImageService : IMediaImageService
    {
        private const string DefaultPosterUrl = "/images/uploads/interstellar.jpg";
        private readonly IWebHostEnvironment _environment;

        public MediaImageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public string ResolvePosterUrl(string? posterPath)
        {
            if (string.IsNullOrWhiteSpace(posterPath))
            {
                return DefaultPosterUrl;
            }

            var normalized = posterPath.Trim().Replace('\\', '/');
            if (normalized.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                normalized.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return normalized;
            }

            var wwwrootIndex = normalized.IndexOf("wwwroot/", StringComparison.OrdinalIgnoreCase);
            if (wwwrootIndex >= 0)
            {
                normalized = normalized[(wwwrootIndex + "wwwroot".Length)..];
            }

            if (Path.IsPathRooted(normalized))
            {
                return DefaultPosterUrl;
            }

            normalized = normalized.TrimStart('~');
            if (normalized.StartsWith('/'))
            {
                return normalized;
            }

            if (!normalized.Contains('/'))
            {
                var uploadsCandidate = $"/images/uploads/{normalized}";
                return StaticFileExists(uploadsCandidate) ? uploadsCandidate : DefaultPosterUrl;
            }

            return "/" + normalized.TrimStart('/');
        }

        private bool StaticFileExists(string urlPath)
        {
            var relativePath = urlPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            return File.Exists(Path.Combine(_environment.WebRootPath, relativePath));
        }
    }
}
