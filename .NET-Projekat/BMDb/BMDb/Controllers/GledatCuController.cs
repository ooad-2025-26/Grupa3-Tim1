using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BMDb.Data;
using BMDb.Models;
using Microsoft.AspNetCore.Authorization;
using BMDb.Services;
using BMDb.ViewModels;

namespace BMDb.Controllers
{
    [Authorize]
    public class GledatCuController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserKeyService _userKeyService;

        public GledatCuController(ApplicationDbContext context, IUserKeyService userKeyService)
        {
            _context = context;
            _userKeyService = userKeyService;
        }

        // GET: GledatCu
        public async Task<IActionResult> Index(string? sort)
        {
            var osobaId = _userKeyService.GetCurrentUserKey(User);
            var sortOrder = NormalizeSort(sort);
            var watchlistRows = await _context.GledatCu
                .AsNoTracking()
                .Where(x => x.OsobaId == osobaId)
                .Select(x => new { x.Id, x.EntertainmentId })
                .ToListAsync();

            var entertainmentIds = watchlistRows.Select(x => x.EntertainmentId).ToList();
            var addedOrder = watchlistRows
                .GroupBy(x => x.EntertainmentId)
                .ToDictionary(x => x.Key, x => x.Max(row => row.Id));

            var items = await BuildProfileItemsAsync(entertainmentIds, addedOrder, sortOrder);
            ViewBag.Sort = sortOrder;
            return View(items);
        }

        // GET: GledatCu/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gledatCu = await _context.GledatCu
                .FirstOrDefaultAsync(m => m.Id == id);
            if (gledatCu == null)
            {
                return NotFound();
            }

            return View(gledatCu);
        }

        // GET: GledatCu/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: GledatCu/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,OsobaId,EntertainmentId")] GledatCu gledatCu)
        {
            if (ModelState.IsValid)
            {
                _context.Add(gledatCu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(gledatCu);
        }

        // GET: GledatCu/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gledatCu = await _context.GledatCu.FindAsync(id);
            if (gledatCu == null)
            {
                return NotFound();
            }
            return View(gledatCu);
        }

        // POST: GledatCu/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,OsobaId,EntertainmentId")] GledatCu gledatCu)
        {
            if (id != gledatCu.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(gledatCu);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GledatCuExists(gledatCu.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(gledatCu);
        }

        // GET: GledatCu/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gledatCu = await _context.GledatCu
                .FirstOrDefaultAsync(m => m.Id == id);
            if (gledatCu == null)
            {
                return NotFound();
            }

            return View(gledatCu);
        }

        // POST: GledatCu/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var gledatCu = await _context.GledatCu.FindAsync(id);
            if (gledatCu != null)
            {
                _context.GledatCu.Remove(gledatCu);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GledatCuExists(int id)
        {
            return _context.GledatCu.Any(e => e.Id == id);
        }

        private async Task<IReadOnlyList<ProfileMediaItemViewModel>> BuildProfileItemsAsync(
            IReadOnlyList<int> entertainmentIds,
            IReadOnlyDictionary<int, int> addedOrder,
            string sortOrder)
        {
            if (entertainmentIds.Count == 0)
            {
                return Array.Empty<ProfileMediaItemViewModel>();
            }

            var filmIds = await _context.Film
                .AsNoTracking()
                .Where(x => entertainmentIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync();
            var filmIdSet = filmIds.ToHashSet();

            var items = await _context.Entertainment
                .AsNoTracking()
                .Where(x => entertainmentIds.Contains(x.Id))
                .Select(x => new ProfileMediaItemViewModel
                {
                    EntertainmentId = x.Id,
                    Naziv = x.Naziv ?? string.Empty,
                    Opis = x.Opis ?? string.Empty,
                    ProsjecnaOcjena = x.ProsjecnaOcjena,
                    Reditelj = x.Reditelj ?? string.Empty,
                    GodinaIzlaska = x.GodinaIzlaska,
                    Trajanje = x.Trajanje,
                    PosterLink = x.PosterLink ?? string.Empty,
                    ControllerName = filmIdSet.Contains(x.Id) ? "Film" : "Serija"
                })
                .ToListAsync();

            var genres = await (
                    from ez in _context.EntertainmentZanr.AsNoTracking()
                    join z in _context.Zanr.AsNoTracking() on ez.ZanrId equals z.Id
                    where entertainmentIds.Contains(ez.EntertainmentId)
                    select new { ez.EntertainmentId, z.Naziv }
                )
                .ToListAsync();

            var genresByItem = genres
                .GroupBy(x => x.EntertainmentId)
                .ToDictionary(
                    x => x.Key,
                    x => (IReadOnlyList<string>)x
                        .Select(g => g.Naziv)
                        .Where(g => !string.IsNullOrWhiteSpace(g))
                        .Distinct()
                        .OrderBy(g => g)
                        .ToList());

            foreach (var item in items)
            {
                item.Genres = genresByItem.TryGetValue(item.EntertainmentId, out var itemGenres)
                    ? itemGenres
                    : Array.Empty<string>();
            }

            return sortOrder == "rating"
                ? items
                    .OrderByDescending(x => x.ProsjecnaOcjena)
                    .ThenBy(x => x.Naziv)
                    .ToList()
                : items
                    .OrderByDescending(x => addedOrder.TryGetValue(x.EntertainmentId, out var id) ? id : 0)
                    .ThenBy(x => x.Naziv)
                    .ToList();
        }

        private static string NormalizeSort(string? sort)
        {
            return string.Equals(sort, "rating", StringComparison.OrdinalIgnoreCase)
                ? "rating"
                : "newest";
        }
    }
}
