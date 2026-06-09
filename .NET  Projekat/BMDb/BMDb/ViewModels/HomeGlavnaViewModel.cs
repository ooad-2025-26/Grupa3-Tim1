namespace BMDb.ViewModels
{
    public class HomeGlavnaViewModel
    {
        public IReadOnlyList<HomeMediaItemViewModel> TopRatedFilms { get; set; } = [];
        public IReadOnlyList<HomeMediaItemViewModel> TopRatedSeries { get; set; } = [];
        public IReadOnlyList<HomeMediaItemViewModel> RecommendedItems { get; set; } = [];
        public IReadOnlyList<HomeMediaItemViewModel> ComingSoonFilms { get; set; } = [];
    }

    public class HomeMediaItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ControllerName { get; set; } = string.Empty;
        public string PosterUrl { get; set; } = string.Empty;
        public double Rating { get; set; }
        public int? Year { get; set; }
        public string TrailerEmbedUrl { get; set; } = string.Empty;
        public IReadOnlyList<string> Genres { get; set; } = [];
    }
}
