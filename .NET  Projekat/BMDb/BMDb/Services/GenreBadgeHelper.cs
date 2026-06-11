namespace BMDb.Services
{
    public static class GenreBadgeHelper
    {
        private const int ColorCount = 8;

        public static string CssClass(int genreId)
        {
            var index = ((genreId % ColorCount) + ColorCount) % ColorCount;
            return $"bmd-genre-{index}";
        }

        public static string CssClass(string? genreName)
        {
            if (string.IsNullOrWhiteSpace(genreName))
            {
                return CssClass(0);
            }

            var hash = 0;
            foreach (var character in genreName.Trim().ToUpperInvariant())
            {
                hash = (hash * 31) + character;
            }

            return CssClass(hash);
        }
    }
}
