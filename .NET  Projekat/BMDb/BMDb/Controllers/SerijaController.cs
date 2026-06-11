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
    public class SerijaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMediaSearchService _mediaSearchService;
        private readonly KatalogFacade _katalogFacade;
        private readonly IUserKeyService _userKeyService;
        private readonly IWatchlistService _watchlistService;
        private readonly IRecenzijaService _recenzijaService;
        private readonly INotificationService _notificationService;
        private readonly IFileValidationService _fileValidationService;

        public SerijaController(
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

        // GET: Serija
        public async Task<IActionResult> Index(string? search, int? zanrId, int? godina, double? minimalnaOcjena, string? sort)
        {
            var filter = new ContentSearchFilterBuilder()
                .WithSearch(search)
                .WithGenre(zanrId)
                .WithYear(godina)
                .WithRating(minimalnaOcjena)
                .WithSort(sort)
                .Build();

            var items = await _mediaSearchService.SearchAsync(_context.Serija, filter);
            var model = new MediaIndexViewModel<Serija>
            {
                Items = items,
                Zanrovi = await _context.Zanr.OrderBy(x => x.Naziv).ToListAsync(),
                ItemZanrovi = await LoadItemGenresAsync(items.Select(x => x.Id).ToList()),
                Filter = filter
            };

            return View(model);
        }

        // GET: Serija/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _katalogFacade.DohvatiSveDetaljeAsync(id.Value, _userKeyService.GetCurrentUserKey(User));
            if (model == null || await _context.Serija.FindAsync(id.Value) == null)
            {
                return NotFound();
            }

            model.StatusPoruka = TempData["StatusPoruka"] as string;
            return View(model);
        }

        // GET: Serija/Create
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Create()
        {
            await PopulateCreateListsAsync();
            return View();
        }

        // POST: Serija/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin,Moderator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("BrojSezona,BrojEpizoda,ZavrsenoEmitovanje,Naziv,Opis,Reditelj,GodinaIzlaska,YoutubeLink,Trajanje,PosterLink")] Serija serija,
            int[] zanrIds,
            int[] glumacIds,
            string[] imenaLikova,
            string? galerijaUrls,
            int[] redniBrojeviSezona,
            int[] brojeviEpizodaSezona,
            int[] datumiPremijereSezona,
            int[] posteriSezona)
        {
            if (ModelState.IsValid)
            {
                if (!_fileValidationService.IsAllowedPoster(serija.PosterLink))
                {
                    ModelState.AddModelError(nameof(Serija.PosterLink), "Poster mora biti .jpg ili .png.");
                    await PopulateCreateListsAsync(zanrIds);
                    return View(serija);
                }

                await using var transaction = await _context.Database.BeginTransactionAsync();

                _context.Serija.Add(serija);
                await _context.SaveChangesAsync();
                AddRelatedData(serija.Id, zanrIds, glumacIds, imenaLikova, galerijaUrls);
                AddSeasonData(serija.Id, redniBrojeviSezona, brojeviEpizodaSezona, datumiPremijereSezona, posteriSezona);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await _notificationService.OnContentCreatedAsync(serija);
                return RedirectToAction(nameof(Index));
            }
            await PopulateCreateListsAsync(zanrIds);
            return View(serija);
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

        // GET: Serija/Edit/5
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serija = await _context.Serija.FindAsync(id);
            if (serija == null)
            {
                return NotFound();
            }
            await PopulateCreateListsAsync(await GetSelectedGenreIdsAsync(serija.Id));
            ViewBag.Sezone = await _context.Sezona
                .Where(x => x.IdSerije == serija.Id)
                .OrderBy(x => x.RedniBrojSezone)
                .ToListAsync();
            return View(serija);
        }

        // POST: Serija/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin,Moderator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("BrojSezona,BrojEpizoda,ZavrsenoEmitovanje,Naziv,Opis,Reditelj,GodinaIzlaska,YoutubeLink,Trajanje,PosterLink")] Serija serija,
            int[] zanrIds,
            int[] redniBrojeviSezona,
            int[] brojeviEpizodaSezona,
            int[] datumiPremijereSezona,
            int[] posteriSezona)
        {
            var existingSerija = await _context.Serija.FindAsync(id);
            if (existingSerija == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (!_fileValidationService.IsAllowedPoster(serija.PosterLink))
                {
                    ModelState.AddModelError(nameof(Serija.PosterLink), "Poster mora biti .jpg ili .png.");
                    await PopulateCreateListsAsync(zanrIds);
                    ViewBag.Sezone = await BuildSeasonPreviewAsync(redniBrojeviSezona, brojeviEpizodaSezona, datumiPremijereSezona, posteriSezona);
                    return View(existingSerija);
                }

                try
                {
                    existingSerija.Naziv = serija.Naziv;
                    existingSerija.Opis = serija.Opis;
                    existingSerija.Reditelj = serija.Reditelj;
                    existingSerija.GodinaIzlaska = serija.GodinaIzlaska;
                    existingSerija.YoutubeLink = serija.YoutubeLink;
                    existingSerija.Trajanje = serija.Trajanje;
                    existingSerija.PosterLink = serija.PosterLink;
                    existingSerija.BrojSezona = serija.BrojSezona;
                    existingSerija.BrojEpizoda = serija.BrojEpizoda;
                    existingSerija.ZavrsenoEmitovanje = serija.ZavrsenoEmitovanje;
                    await ReplaceGenresAsync(existingSerija.Id, zanrIds);
                    await ReplaceSeasonDataAsync(existingSerija.Id, redniBrojeviSezona, brojeviEpizodaSezona, datumiPremijereSezona, posteriSezona);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SerijaExists(id))
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
            ViewBag.Sezone = await BuildSeasonPreviewAsync(redniBrojeviSezona, brojeviEpizodaSezona, datumiPremijereSezona, posteriSezona);
            return View(existingSerija);
        }

        // GET: Serija/Delete/5
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serija = await _context.Serija
                .FirstOrDefaultAsync(m => m.Id == id);
            if (serija == null)
            {
                return NotFound();
            }

            return View(serija);
        }

        // POST: Serija/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin,Moderator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string? returnTo)
        {
            var serija = await _context.Serija.FindAsync(id);
            if (serija != null)
            {
                RemoveRelatedData(id);
                _context.Serija.Remove(serija);
            }

            await _context.SaveChangesAsync();
            return RedirectAfterDelete(returnTo);
        }

        private bool SerijaExists(int id)
        {
            return _context.Serija.Any(e => e.Id == id);
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

        private async Task<Dictionary<int, IReadOnlyList<Zanr>>> LoadItemGenresAsync(IReadOnlyList<int> entertainmentIds)
        {
            if (entertainmentIds.Count == 0)
            {
                return [];
            }

            var rows = await (
                    from ez in _context.EntertainmentZanr.AsNoTracking()
                    join z in _context.Zanr.AsNoTracking() on ez.ZanrId equals z.Id
                    where entertainmentIds.Contains(ez.EntertainmentId)
                    select new { ez.EntertainmentId, Zanr = z }
                )
                .ToListAsync();

            return rows
                .GroupBy(x => x.EntertainmentId)
                .ToDictionary(
                    x => x.Key,
                    x => (IReadOnlyList<Zanr>)x
                        .Select(g => g.Zanr)
                        .GroupBy(g => g.Id)
                        .Select(g => g.First())
                        .OrderBy(g => g.Naziv)
                        .ToList());
        }

        private void AddSeasonData(int serijaId, int[] redniBrojevi, int[] brojeviEpizoda, int[] datumiPremijere, int[] posteri)
        {
            for (var i = 0; i < redniBrojevi.Length; i++)
            {
                if (redniBrojevi[i] <= 0)
                {
                    continue;
                }

                _context.Sezona.Add(new Sezona
                {
                    IdSerije = serijaId,
                    RedniBrojSezone = redniBrojevi[i],
                    BrojEpizoda = i < brojeviEpizoda.Length ? brojeviEpizoda[i] : 0,
                    DatumPremijere = i < datumiPremijere.Length ? datumiPremijere[i] : 0,
                    PosterSezone = i < posteri.Length ? posteri[i] : 0
                });
            }
        }

        private async Task ReplaceSeasonDataAsync(int serijaId, int[] redniBrojevi, int[] brojeviEpizoda, int[] datumiPremijere, int[] posteri)
        {
            var existing = await _context.Sezona.Where(x => x.IdSerije == serijaId).ToListAsync();
            _context.Sezona.RemoveRange(existing);
            AddSeasonData(serijaId, redniBrojevi, brojeviEpizoda, datumiPremijere, posteri);
        }

        private static Task<List<Sezona>> BuildSeasonPreviewAsync(int[] redniBrojevi, int[] brojeviEpizoda, int[] datumiPremijere, int[] posteri)
        {
            var sezone = new List<Sezona>();
            for (var i = 0; i < redniBrojevi.Length; i++)
            {
                sezone.Add(new Sezona
                {
                    RedniBrojSezone = redniBrojevi[i],
                    BrojEpizoda = i < brojeviEpizoda.Length ? brojeviEpizoda[i] : 0,
                    DatumPremijere = i < datumiPremijere.Length ? datumiPremijere[i] : 0,
                    PosterSezone = i < posteri.Length ? posteri[i] : 0
                });
            }

            return Task.FromResult(sezone);
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
            _context.Sezona.RemoveRange(_context.Sezona.Where(x => x.IdSerije == entertainmentId));
        }
    }
}
