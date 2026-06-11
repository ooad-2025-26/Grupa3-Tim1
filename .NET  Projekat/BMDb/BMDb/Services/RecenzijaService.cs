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

        public RecenzijaService(ApplicationDbContext context, IWatchlistService watchlistService)
        {
            _context = context;
        }

        public async Task<(bool Success, string Message)> DodajRecenzijuAsync(int osobaId, int entertainmentId, int ocjena, string? komentar)
        {
            if (osobaId <= 0)
            {
                return (false, "Morate biti prijavljeni da biste napisali recenziju.");
            }

            if (ocjena < 1 || ocjena > 10)
            {
                return (false, "Ocjena mora biti izmedju 1 i 10.");
            }

            var vecPostoji = await _context.Recenzija
                .AnyAsync(x => x.OsobaId == osobaId && x.EntertainmentId == entertainmentId);
            if (vecPostoji)
            {
                return (false, "Vec ste napisali recenziju za ovaj sadrzaj.");
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
            return (true, "Recenzija je sacuvana.");
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
