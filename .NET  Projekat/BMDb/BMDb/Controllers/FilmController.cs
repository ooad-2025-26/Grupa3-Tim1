using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BMDb.Data;
using BMDb.Models;
using BMDb.Services;
using BMDb.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace BMDb.Controllers
{
    public class FilmController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMediaSearchService _mediaSearchService;
        private readonly KatalogFacade _katalogFacade;
        private readonly IUserKeyService _userKeyService;
        private readonly IWatchlistService _watchlistService;
        private readonly IRecenzijaService _recenzijaService;
        private readonly INotificationService _notificationService;
        private readonly IFileValidationService _fileValidationService;

        public FilmController(
            ApplicationDbContext context,
            IMediaSearchService mediaSearchService,
            KatalogFacade katalogFacade,
            IUserKeyService userKeyService,
            IWatchlistService watchlistService,
            IRecenzijaService recenzijaService,
            INotificationService notificationService,
            IFileValidationService fileValidationService)
        {
            _context = context;
            _mediaSearchService = mediaSearchService;
            _katalogFacade = katalogFacade;
            _userKeyService = userKeyService;
            _watchlistService = watchlistService;
            _recenzijaService = recenzijaService;
            _notificationService = notificationService;
            _fileValidationService = fileValidationService;
        }

        // GET: Film
        public async Task<IActionResult> Index(string? search, int? zanrId, int? godina, double? minimalnaOcjena, string? sort)
        {
            var filter = new ContentSearchFilterBuilder()
                .WithSearch(search)
                .WithGenre(zanrId)
                .WithYear(godina)
                .WithRating(minimalnaOcjena)
                .WithSort(sort)
                .Build();

            var model = new MediaIndexViewModel<Film>
            {
                Items = await _mediaSearchService.SearchAsync(_context.Film, filter),
                Zanrovi = await _context.Zanr.OrderBy(x => x.Naziv).ToListAsync(),
                Filter = filter
            };

            return View(model);
        }

        // GET: Film/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _katalogFacade.DohvatiSveDetaljeAsync(id.Value, _userKeyService.GetCurrentUserKey(User));
            if (model == null || await _context.Film.FindAsync(id.Value) == null)
            {
                return NotFound();
            }

            model.StatusPoruka = TempData["StatusPoruka"] as string;
            return View(model);
        }

        // GET: Film/Create
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Create()
        {
            await PopulateCreateListsAsync();
            return View();
        }

        public async Task<IActionResult> ComingSoon()
        {
            var trenutnaGodina = DateTime.UtcNow.Year;
            var filmovi = await _context.Film
                .AsNoTracking()
                .Where(x => x.GodinaIzlaska > trenutnaGodina)
                .OrderBy(x => x.GodinaIzlaska)
                .ThenBy(x => x.Naziv)
                .ToListAsync();

            var filmIds = filmovi.Select(x => x.Id).ToList();
            var zanrovi = await (
                    from ez in _context.EntertainmentZanr.AsNoTracking()
                    join z in _context.Zanr.AsNoTracking() on ez.ZanrId equals z.Id
                    where filmIds.Contains(ez.EntertainmentId)
                    select new { ez.EntertainmentId, z.Naziv }
                )
                .ToListAsync();

            ViewBag.Zanrovi = zanrovi
                .GroupBy(x => x.EntertainmentId)
                .ToDictionary(
                    x => x.Key,
                    x => x.Select(z => z.Naziv).Where(z => !string.IsNullOrWhiteSpace(z)).Distinct().ToList());

            return View(filmovi);
        }

        // POST: Film/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin,Moderator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("BoxOffice,Naziv,Opis,Reditelj,GodinaIzlaska,YoutubeLink,Trajanje,PosterLink")] Film film,
            int[] zanrIds,
            int[] glumacIds,
            string[] imenaLikova,
            string? galerijaUrls)
        {
            if (ModelState.IsValid)
            {
                if (!_fileValidationService.IsAllowedPoster(film.PosterLink))
                {
                    ModelState.AddModelError(nameof(Film.PosterLink), "Poster mora biti .jpg ili .png.");
                    await PopulateCreateListsAsync(zanrIds);
                    return View(film);
                }

                await using var transaction = await _context.Database.BeginTransactionAsync();

                _context.Film.Add(film);
                await _context.SaveChangesAsync();
                AddRelatedData(film.Id, zanrIds, glumacIds, imenaLikova, galerijaUrls);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await _notificationService.OnContentCreatedAsync(film);
                return RedirectToAction(nameof(Index));
            }
            await PopulateCreateListsAsync(zanrIds);
            return View(film);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OznaciGledao(int id)
        {
            await _watchlistService.OznaciGledaoAsync(_userKeyService.GetCurrentUserKey(User), id);
            TempData["StatusPoruka"] = "Sadržaj je označen kao 'Gledao sam'.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OznaciPlanirano(int id)
        {
            await _watchlistService.OznaciPlaniranoAsync(_userKeyService.GetCurrentUserKey(User), id);
            TempData["StatusPoruka"] = "Sadržaj je označen kao 'Gledat ću'.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DodajRecenziju(int id, int ocjena, string? komentar)
        {
            var result = await _recenzijaService.DodajRecenzijuAsync(_userKeyService.GetCurrentUserKey(User), id, ocjena, komentar);
            TempData["StatusPoruka"] = result.Message;
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Film/Edit/5
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var film = await _context.Film.FindAsync(id);
            if (film == null)
            {
                return NotFound();
            }
            await PopulateCreateListsAsync(await GetSelectedGenreIdsAsync(film.Id));
            return View(film);
        }

        // POST: Film/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin,Moderator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("BoxOffice,Naziv,Opis,Reditelj,GodinaIzlaska,YoutubeLink,Trajanje,PosterLink")] Film film,
            int[] zanrIds)
        {
            var existingFilm = await _context.Film.FindAsync(id);
            if (existingFilm == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (!_fileValidationService.IsAllowedPoster(film.PosterLink))
                {
                    ModelState.AddModelError(nameof(Film.PosterLink), "Poster mora biti .jpg ili .png.");
                    await PopulateCreateListsAsync(zanrIds);
                    return View(existingFilm);
                }

                try
                {
                    existingFilm.Naziv = film.Naziv;
                    existingFilm.Opis = film.Opis;
                    existingFilm.Reditelj = film.Reditelj;
                    existingFilm.GodinaIzlaska = film.GodinaIzlaska;
                    existingFilm.YoutubeLink = film.YoutubeLink;
                    existingFilm.Trajanje = film.Trajanje;
                    existingFilm.PosterLink = film.PosterLink;
                    existingFilm.BoxOffice = film.BoxOffice;
                    await ReplaceGenresAsync(existingFilm.Id, zanrIds);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FilmExists(id))
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
            await PopulateCreateListsAsync(zanrIds);
            return View(existingFilm);
        }

        // GET: Film/Delete/5
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var film = await _context.Film
                .FirstOrDefaultAsync(m => m.Id == id);
            if (film == null)
            {
                return NotFound();
            }

            return View(film);
        }

        // POST: Film/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin,Moderator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string? returnTo)
        {
            var film = await _context.Film.FindAsync(id);
            if (film != null)
            {
                RemoveRelatedData(id);
                _context.Film.Remove(film);
            }

            await _context.SaveChangesAsync();
            return RedirectAfterDelete(returnTo);
        }

        private bool FilmExists(int id)
        {
            return _context.Film.Any(e => e.Id == id);
        }

        private IActionResult RedirectAfterDelete(string? returnTo)
        {
            return returnTo switch
            {
                "Entertainment" => RedirectToAction("Index", "Entertainment"),
                _ => RedirectToAction(nameof(Index))
            };
        }

        private async Task PopulateCreateListsAsync(int[]? selectedZanrIds = null)
        {
            ViewBag.Zanrovi = new MultiSelectList(
                await _context.Zanr.OrderBy(x => x.Naziv).ToListAsync(),
                nameof(Zanr.Id),
                nameof(Zanr.Naziv),
                selectedZanrIds ?? Array.Empty<int>());

            ViewBag.Glumci = await _context.Glumac.OrderBy(x => x.Ime).ThenBy(x => x.Prezime).ToListAsync();
        }

        private void AddRelatedData(int entertainmentId, int[] zanrIds, int[] glumacIds, string[] imenaLikova, string? galerijaUrls)
        {
            AddGenres(entertainmentId, zanrIds);

            for (var i = 0; i < glumacIds.Length; i++)
            {
                if (glumacIds[i] <= 0)
                {
                    continue;
                }

                _context.Uloga.Add(new Uloga
                {
                    EntertainmentId = entertainmentId,
                    GlumacId = glumacIds[i],
                    ImeLika = i < imenaLikova.Length ? imenaLikova[i]?.Trim() ?? string.Empty : string.Empty
                });
            }

            foreach (var url in SplitLines(galerijaUrls))
            {
                _context.GalerijaSlika.Add(new GalerijaSlika
                {
                    EntertainmentId = entertainmentId,
                    Url = url
                });
            }
        }

        private void AddGenres(int entertainmentId, int[] zanrIds)
        {
            foreach (var zanrId in zanrIds.Distinct().Where(x => x > 0))
            {
                _context.EntertainmentZanr.Add(new EntertainmentZanr
                {
                    EntertainmentId = entertainmentId,
                    ZanrId = zanrId
                });
            }
        }

        private async Task ReplaceGenresAsync(int entertainmentId, int[] zanrIds)
        {
            var existing = await _context.EntertainmentZanr
                .Where(x => x.EntertainmentId == entertainmentId)
                .ToListAsync();
            _context.EntertainmentZanr.RemoveRange(existing);
            AddGenres(entertainmentId, zanrIds);
        }

        private async Task<int[]> GetSelectedGenreIdsAsync(int entertainmentId)
        {
            return await _context.EntertainmentZanr
                .Where(x => x.EntertainmentId == entertainmentId)
                .Select(x => x.ZanrId)
                .ToArrayAsync();
        }

        private static IEnumerable<string> SplitLines(string? value)
        {
            return (value ?? string.Empty)
                .Split(new[] { "\r\n", "\n", ";", "," }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(x => !string.IsNullOrWhiteSpace(x));
        }

        private void RemoveRelatedData(int entertainmentId)
        {
            _context.EntertainmentZanr.RemoveRange(_context.EntertainmentZanr.Where(x => x.EntertainmentId == entertainmentId));
            _context.GalerijaSlika.RemoveRange(_context.GalerijaSlika.Where(x => x.EntertainmentId == entertainmentId));
            _context.Uloga.RemoveRange(_context.Uloga.Where(x => x.EntertainmentId == entertainmentId));
            _context.Recenzija.RemoveRange(_context.Recenzija.Where(x => x.EntertainmentId == entertainmentId));
            _context.GledaoSam.RemoveRange(_context.GledaoSam.Where(x => x.EntertainmentId == entertainmentId));
            _context.GledatCu.RemoveRange(_context.GledatCu.Where(x => x.EntertainmentId == entertainmentId));
        }
    }
}
