using BMDb.Data;
using BMDb.Models;
using Microsoft.EntityFrameworkCore;

namespace BMDb.Services
{
    public interface IWatchlistService
    {
        Task<bool> JeGledaoAsync(int osobaId, int entertainmentId);
        Task<bool> JePlaniranoAsync(int osobaId, int entertainmentId);
        Task OznaciGledaoAsync(int osobaId, int entertainmentId);
        Task OznaciPlaniranoAsync(int osobaId, int entertainmentId);
    }

    public class WatchlistService : IWatchlistService
    {
        private readonly ApplicationDbContext _context;

        public WatchlistService(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<bool> JeGledaoAsync(int osobaId, int entertainmentId)
        {
            return _context.GledaoSam.AnyAsync(x => x.OsobaId == osobaId && x.EntertainmentId == entertainmentId);
        }

        public Task<bool> JePlaniranoAsync(int osobaId, int entertainmentId)
        {
            return _context.GledatCu.AnyAsync(x => x.OsobaId == osobaId && x.EntertainmentId == entertainmentId);
        }

        public async Task OznaciGledaoAsync(int osobaId, int entertainmentId)
        {
            if (!await JeGledaoAsync(osobaId, entertainmentId))
            {
                _context.GledaoSam.Add(new GledaoSam { OsobaId = osobaId, EntertainmentId = entertainmentId });
            }

            var planirani = await _context.GledatCu
                .Where(x => x.OsobaId == osobaId && x.EntertainmentId == entertainmentId)
                .ToListAsync();
            _context.GledatCu.RemoveRange(planirani);

            await _context.SaveChangesAsync();
        }

        public async Task OznaciPlaniranoAsync(int osobaId, int entertainmentId)
        {
            if (await JeGledaoAsync(osobaId, entertainmentId))
            {
                return;
            }

            if (!await JePlaniranoAsync(osobaId, entertainmentId))
            {
                _context.GledatCu.Add(new GledatCu { OsobaId = osobaId, EntertainmentId = entertainmentId });
                await _context.SaveChangesAsync();
            }
        }
    }
}
