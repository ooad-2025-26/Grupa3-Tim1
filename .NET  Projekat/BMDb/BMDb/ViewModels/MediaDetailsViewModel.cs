using BMDb.Models;

namespace BMDb.ViewModels
{
    public class MediaDetailsViewModel
    {
        public Entertainment Entertainment { get; set; } = null!;
        public string? TrailerEmbedUrl { get; set; }
        public IReadOnlyList<Recenzija> Recenzije { get; set; } = Array.Empty<Recenzija>();
        public IReadOnlyList<Sezona> Sezone { get; set; } = Array.Empty<Sezona>();
        public IReadOnlyList<Uloga> Uloge { get; set; } = Array.Empty<Uloga>();
        public IReadOnlyList<Glumac> Glumci { get; set; } = Array.Empty<Glumac>();
        public IReadOnlyList<Zanr> Zanrovi { get; set; } = Array.Empty<Zanr>();
        public IReadOnlyList<GalerijaSlika> Galerija { get; set; } = Array.Empty<GalerijaSlika>();
        public bool JeGledao { get; set; }
        public bool JePlanirano { get; set; }
        public bool MozeOcijeniti => JeGledao;
        public string? StatusPoruka { get; set; }
    }
}
