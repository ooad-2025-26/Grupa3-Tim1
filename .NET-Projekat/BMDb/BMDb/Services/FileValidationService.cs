namespace BMDb.Services
{
    public interface IFileValidationService
    {
        bool IsAllowedPoster(string? fileName);
    }

    public class FileValidationService : IFileValidationService
    {
        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".png" };

        public bool IsAllowedPoster(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return true;
            }

            var value = fileName.Trim();
            if (Uri.TryCreate(value, UriKind.Absolute, out var uri))
            {
                return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
            }

            if (Path.IsPathRooted(value))
            {
                return false;
            }

            return AllowedExtensions.Contains(Path.GetExtension(value));
        }
    }
}
