using BMDb.Data;
using BMDb.Models;
using Microsoft.EntityFrameworkCore;

namespace BMDb.Services
{
    public interface IRecenzijaService
    {
        Task<(bool Success, string Message)> DodajRecenzijuAsync(int osobaId, int entertainmentId, int ocjena, string? komentar);
        Task AzurirajProsjecnuOcjenuAsync(int entertainmentId);
    }

    public class RecenzijaService : IRecenzijaService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWatchlistService _watchlistService;

        public RecenzijaService(ApplicationDbContext context, IWatchlistService watchlistService)
        {
            _context = context;
            _watchlistService = watchlistService;
        }

        public async Task<(bool Success, string Message)> DodajRecenzijuAsync(int osobaId, int entertainmentId, int ocjena, string? komentar)
        {
            if (ocjena < 1 || ocjena > 10)
            {
                return (false, "Ocjena mora biti između 1 i 10.");
            }

            if (!await _watchlistService.JeGledaoAsync(osobaId, entertainmentId))
            {
                return (false, "Sadržaj mora biti označen kao 'Gledao sam' prije ocjenjivanja.");
            }

            _context.Recenzija.Add(new Recenzija
            {
                EntertainmentId = entertainmentId,
                OsobaId = osobaId,
                Ocjena = ocjena,
                Komentar = komentar?.Trim() ?? string.Empty,
                DatumObjave = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            await AzurirajProsjecnuOcjenuAsync(entertainmentId);
            return (true, "Recenzija je sačuvana.");
        }

        public async Task AzurirajProsjecnuOcjenuAsync(int entertainmentId)
        {
            var entertainment = await _context.Entertainment.FindAsync(entertainmentId);
            if (entertainment == null)
            {
                return;
            }

            var prosjek = await _context.Recenzija
                .Where(x => x.EntertainmentId == entertainmentId)
                .AverageAsync(x => (double?)x.Ocjena);

            entertainment.ProsjecnaOcjena = Math.Round(prosjek ?? 0, 1);
            await _context.SaveChangesAsync();
        }
    }
}
