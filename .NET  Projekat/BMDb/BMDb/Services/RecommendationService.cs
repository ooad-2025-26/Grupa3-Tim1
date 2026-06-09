using BMDb.Data;
using BMDb.Models;
using Microsoft.EntityFrameworkCore;

namespace BMDb.Services
{
    public interface IRecommendationService
    {
        Task<IReadOnlyList<Entertainment>> GlobalnePreporukeAsync(int count = 10);
        Task<IReadOnlyList<Entertainment>> PersonalizovanePreporukeAsync(int osobaId, int count = 10);
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
            return await _context.Entertainment
                .OrderByDescending(x => x.ProsjecnaOcjena)
                .ThenByDescending(x => x.GodinaIzlaska)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Entertainment>> PersonalizovanePreporukeAsync(int osobaId, int count = 10)
        {
            var gledaniIds = await _context.GledaoSam
                .Where(x => x.OsobaId == osobaId)
                .Select(x => x.EntertainmentId)
                .ToListAsync();

            var zanrIds = await _context.EntertainmentZanr
                .Where(x => gledaniIds.Contains(x.EntertainmentId))
                .Select(x => x.ZanrId)
                .Distinct()
                .ToListAsync();

            if (zanrIds.Count == 0)
            {
                return await GlobalnePreporukeAsync(count);
            }

            var preporuceniIds = await _context.EntertainmentZanr
                .Where(x => zanrIds.Contains(x.ZanrId) && !gledaniIds.Contains(x.EntertainmentId))
                .Select(x => x.EntertainmentId)
                .Distinct()
                .ToListAsync();

            return await _context.Entertainment
                .Where(x => preporuceniIds.Contains(x.Id))
                .OrderByDescending(x => x.ProsjecnaOcjena)
                .ThenByDescending(x => x.GodinaIzlaska)
                .Take(count)
                .ToListAsync();
        }
    }
}
