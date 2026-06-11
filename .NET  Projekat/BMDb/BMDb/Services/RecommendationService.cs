using BMDb.Data;
using BMDb.Models;
using Microsoft.EntityFrameworkCore;

namespace BMDb.Services
{
    public interface IRecommendationService
    {
        Task<IReadOnlyList<Entertainment>> GlobalnePreporukeAsync(int count = 10);
        Task<IReadOnlyList<Entertainment>> PersonalizovanePreporukeAsync(string? osobaId, int gledaoSamOsobaId, int count = 10);
        Task<IReadOnlyList<Film>> PreporuceniFilmoviAsync(string? osobaId, int gledaoSamOsobaId, int count = 10);
        Task<IReadOnlyList<Serija>> PreporuceneSerijeAsync(string? osobaId, int gledaoSamOsobaId, int count = 10);
    }

    public class RecommendationService : IRecommendationService
    {
        private readonly ApplicationDbContext _context;

        public RecommendationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<Entertainment>> GlobalnePreporukeAsync(int count = 10)
        {
            var filmovi = await _context.Film
                .AsNoTracking()
                .Select(x => (Entertainment)x)
                .ToListAsync();

            var serije = await _context.Serija
                .AsNoTracking()
                .Select(x => (Entertainment)x)
                .ToListAsync();

            return filmovi.Concat(serije)
                .OrderByDescending(x => x.ProsjecnaOcjena)
                .ThenByDescending(x => x.GodinaIzlaska)
                .Take(count)
                .ToList();
        }

        public async Task<IReadOnlyList<Entertainment>> PersonalizovanePreporukeAsync(string? osobaId, int gledaoSamOsobaId, int count = 10)
        {
            if (string.IsNullOrWhiteSpace(osobaId))
            {
                return [];
            }

            var zanrIds = await _context.OsobaZanr
                .AsNoTracking()
                .Where(x => x.OsobaId == osobaId)
                .Select(x => x.ZanrId)
                .Distinct()
                .ToListAsync();

            if (zanrIds.Count == 0)
            {
                return [];
            }

            var gledaniIds = gledaoSamOsobaId > 0
                ? await _context.GledaoSam
                    .AsNoTracking()
                    .Where(x => x.OsobaId == gledaoSamOsobaId)
                    .Select(x => x.EntertainmentId)
                    .ToListAsync()
                : [];

            var relevantnostPoSadrzaju = await _context.EntertainmentZanr
                .AsNoTracking()
                .Where(x => zanrIds.Contains(x.ZanrId) && !gledaniIds.Contains(x.EntertainmentId))
                .GroupBy(x => x.EntertainmentId)
                .Select(g => new
                {
                    EntertainmentId = g.Key,
                    BrojPoklopljenihZanrova = g.Select(x => x.ZanrId).Distinct().Count()
                })
                .ToListAsync();

            if (relevantnostPoSadrzaju.Count == 0)
            {
                return [];
            }

            var relevantnost = relevantnostPoSadrzaju.ToDictionary(x => x.EntertainmentId, x => x.BrojPoklopljenihZanrova);
            var ids = relevantnost.Keys.ToList();

            var filmovi = await _context.Film
                .AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .Select(x => (Entertainment)x)
                .ToListAsync();

            var serije = await _context.Serija
                .AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .Select(x => (Entertainment)x)
                .ToListAsync();

            return filmovi.Concat(serije)
                .OrderByDescending(x => relevantnost[x.Id])
                .ThenByDescending(x => x.ProsjecnaOcjena)
                .ThenByDescending(x => x.GodinaIzlaska)
                .Take(count)
                .ToList();
        }

        public async Task<IReadOnlyList<Film>> PreporuceniFilmoviAsync(string? osobaId, int gledaoSamOsobaId, int count = 10)
        {
            var relevantnost = await GetRecommendationScoresAsync(osobaId, gledaoSamOsobaId);
            if (relevantnost.Count == 0)
            {
                return [];
            }

            var ids = relevantnost.Keys.ToList();
            return (await _context.Film
                    .AsNoTracking()
                    .Where(x => ids.Contains(x.Id))
                    .ToListAsync())
                .OrderByDescending(x => relevantnost[x.Id])
                .ThenByDescending(x => x.ProsjecnaOcjena)
                .ThenByDescending(x => x.DatumIzlaska)
                .ThenByDescending(x => x.GodinaIzlaska)
                .Take(count)
                .ToList();
        }

        public async Task<IReadOnlyList<Serija>> PreporuceneSerijeAsync(string? osobaId, int gledaoSamOsobaId, int count = 10)
        {
            var relevantnost = await GetRecommendationScoresAsync(osobaId, gledaoSamOsobaId);
            if (relevantnost.Count == 0)
            {
                return [];
            }

            var ids = relevantnost.Keys.ToList();
            return (await _context.Serija
                    .AsNoTracking()
                    .Where(x => ids.Contains(x.Id))
                    .ToListAsync())
                .OrderByDescending(x => relevantnost[x.Id])
                .ThenByDescending(x => x.ProsjecnaOcjena)
                .ThenByDescending(x => x.DatumIzlaska)
                .ThenByDescending(x => x.GodinaIzlaska)
                .Take(count)
                .ToList();
        }

        private async Task<Dictionary<int, int>> GetRecommendationScoresAsync(string? osobaId, int gledaoSamOsobaId)
        {
            if (string.IsNullOrWhiteSpace(osobaId))
            {
                return [];
            }

            var zanrIds = await _context.OsobaZanr
                .AsNoTracking()
                .Where(x => x.OsobaId == osobaId)
                .Select(x => x.ZanrId)
                .Distinct()
                .ToListAsync();

            if (zanrIds.Count == 0)
            {
                return [];
            }

            var gledaniIds = gledaoSamOsobaId > 0
                ? await _context.GledaoSam
                    .AsNoTracking()
                    .Where(x => x.OsobaId == gledaoSamOsobaId)
                    .Select(x => x.EntertainmentId)
                    .ToListAsync()
                : [];

            return await _context.EntertainmentZanr
                .AsNoTracking()
                .Where(x => zanrIds.Contains(x.ZanrId) && !gledaniIds.Contains(x.EntertainmentId))
                .GroupBy(x => x.EntertainmentId)
                .Select(g => new
                {
                    EntertainmentId = g.Key,
                    BrojPoklopljenihZanrova = g.Select(x => x.ZanrId).Distinct().Count()
                })
                .ToDictionaryAsync(x => x.EntertainmentId, x => x.BrojPoklopljenihZanrova);
        }
    }
}
