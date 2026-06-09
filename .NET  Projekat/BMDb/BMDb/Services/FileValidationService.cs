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

            return AllowedExtensions.Contains(Path.GetExtension(fileName));
        }
    }
}
