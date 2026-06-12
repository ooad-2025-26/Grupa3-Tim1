using BMDb.Models;

namespace BMDb.ViewModels
{
    public class MediaIndexViewModel<T> where T : Entertainment
    {
        public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
        public IReadOnlyList<Zanr> Zanrovi { get; set; } = Array.Empty<Zanr>();
        public IReadOnlyDictionary<int, IReadOnlyList<Zanr>> ItemZanrovi { get; set; } =
            new Dictionary<int, IReadOnlyList<Zanr>>();
        public ContentSearchFilter Filter { get; set; } = new();
        public bool NemaRezultata => Items.Count == 0;
    }
}
