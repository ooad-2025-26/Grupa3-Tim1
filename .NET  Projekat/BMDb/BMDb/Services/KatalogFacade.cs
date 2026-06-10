using BMDb.Data;
using BMDb.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BMDb.Services
{
    // Facade pattern: details pages ask one class for related media details instead of coordinating many DbSets.
    public class KatalogFacade
    {
        private readonly ApplicationDbContext _context;
        private readonly ITrailerServis _trailerServis;
        private readonly IWatchlistService _watchlistService;

        public KatalogFacade(ApplicationDbContext context, ITrailerServis trailerServis, IWatchlistService watchlistService)
        {
            _context = context;
            _trailerServis = trailerServis;
            _watchlistService = watchlistService;
        }

        public async Task<MediaDetailsViewModel?> DohvatiSveDetaljeAsync(int id, int osobaId)
        {
            var entertainment = await _context.Entertainment.FirstOrDefaultAsync(x => x.Id == id);
            if (entertainment == null)
            {
                return null;
            }

            var uloge = await _context.Uloga.Where(x => x.EntertainmentId == id).ToListAsync();
            var glumacIds = uloge.Select(x => x.GlumacId).Distinct().ToList();
            var zanrIds = await _context.EntertainmentZanr
                .Where(x => x.EntertainmentId == id)
                .Select(x => x.ZanrId)
                .Distinct()
                .ToListAsync();

            return new MediaDetailsViewModel
            {
                Entertainment = entertainment,
                TrailerEmbedUrl = _trailerServis.PokreniTrailer(entertainment.YoutubeLink),
                Recenzije = await _context.Recenzija.Where(x => x.EntertainmentId == id).OrderByDescending(x => x.DatumObjave).ToListAsync(),
                Sezone = await _context.Sezona.Where(x => x.IdSerije == id).OrderBy(x => x.RedniBrojSezone).ToListAsync(),
                Uloge = uloge.OrderBy(x => x.Id).ToList(),
                Glumci = await _context.Glumac.Where(x => glumacIds.Contains(x.Id)).OrderBy(x => x.Ime).ThenBy(x => x.Prezime).ToListAsync(),
                Zanrovi = await _context.Zanr.Where(x => zanrIds.Contains(x.Id)).OrderBy(x => x.Naziv).ToListAsync(),
                Galerija = await _context.GalerijaSlika.Where(x => x.EntertainmentId == id).OrderBy(x => x.Id).ToListAsync(),
                JeGledao = osobaId > 0 && await _watchlistService.JeGledaoAsync(osobaId, id),
                JePlanirano = osobaId > 0 && await _watchlistService.JePlaniranoAsync(osobaId, id)
            };
        }
    }
}
