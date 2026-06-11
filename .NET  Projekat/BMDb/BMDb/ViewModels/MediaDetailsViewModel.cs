using BMDb.Models;

namespace BMDb.ViewModels
{
    public class MediaDetailsViewModel
    {
        public Entertainment Entertainment { get; set; } = null!;
        public string? TrailerEmbedUrl { get; set; }
        public IReadOnlyList<Recenzija> Recenzije { get; set; } = Array.Empty<Recenzija>();
        public IReadOnlyList<Recenzija> RegularRecenzije { get; set; } = Array.Empty<Recenzija>();
        public IReadOnlyList<Recenzija> VerifikovaneRecenzije { get; set; } = Array.Empty<Recenzija>();
        public IReadOnlyDictionary<int, ReviewAuthorViewModel> Recenzenti { get; set; } =
            new Dictionary<int, ReviewAuthorViewModel>();
        public IReadOnlyList<Sezona> Sezone { get; set; } = Array.Empty<Sezona>();
        public IReadOnlyList<Uloga> Uloge { get; set; } = Array.Empty<Uloga>();
        public IReadOnlyList<Glumac> Glumci { get; set; } = Array.Empty<Glumac>();
        public IReadOnlyList<Zanr> Zanrovi { get; set; } = Array.Empty<Zanr>();
        public IReadOnlyList<GalerijaSlika> Galerija { get; set; } = Array.Empty<GalerijaSlika>();
        public bool JeGledao { get; set; }
        public bool JePlanirano { get; set; }
        public bool JePrijavljen { get; set; }
        public bool JeVecRecenzirao { get; set; }
        public bool MozeOcijeniti => JePrijavljen && !JeVecRecenzirao;
        public string? StatusPoruka { get; set; }
    }

    public class ReviewAuthorViewModel
    {
        public string DisplayName { get; set; } = "Korisnik";
        public string Avatar { get; set; } = "/images/uploads/user-img.png";
    }
}
