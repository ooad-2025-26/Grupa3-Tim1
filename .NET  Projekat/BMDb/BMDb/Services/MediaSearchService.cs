using BMDb.Data;
using BMDb.Models;
using BMDb.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BMDb.Services
{
    public interface IMediaSearchService
    {
        Task<IReadOnlyList<T>> SearchAsync<T>(IQueryable<T> source, ContentSearchFilter filter) where T : Entertainment;
    }

    public class MediaSearchService : IMediaSearchService
    {
        private readonly ApplicationDbContext _context;

        public MediaSearchService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<T>> SearchAsync<T>(IQueryable<T> source, ContentSearchFilter filter) where T : Entertainment
        {
            var query = source.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                query = query.Where(x => x.Naziv.Contains(filter.Search) || x.Opis.Contains(filter.Search));
            }

            if (filter.Godina.HasValue)
            {
                query = query.Where(x => x.GodinaIzlaska == filter.Godina.Value);
            }

            if (filter.MinimalnaOcjena.HasValue)
            {
                query = query.Where(x => x.ProsjecnaOcjena >= filter.MinimalnaOcjena.Value);
            }

            if (filter.ZanrId.HasValue)
            {
                var ids = await _context.EntertainmentZanr
                    .Where(x => x.ZanrId == filter.ZanrId.Value)
                    .Select(x => x.EntertainmentId)
                    .ToListAsync();
                query = query.Where(x => ids.Contains(x.Id));
            }

            query = filter.Sort switch
            {
                "rating_asc" => query.OrderBy(x => x.ProsjecnaOcjena),
                "year_desc" => query.OrderByDescending(x => x.GodinaIzlaska),
                "year_asc" => query.OrderBy(x => x.GodinaIzlaska),
                "title" => query.OrderBy(x => x.Naziv),
                _ => query.OrderByDescending(x => x.ProsjecnaOcjena)
            };

            return await query.ToListAsync();
        }
    }
}
