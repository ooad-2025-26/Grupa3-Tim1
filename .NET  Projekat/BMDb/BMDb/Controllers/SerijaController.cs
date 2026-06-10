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

            var model = new MediaIndexViewModel<Serija>
            {
                Items = await _mediaSearchService.SearchAsync(_context.Serija, filter),
                Zanrovi = await _context.Zanr.OrderBy(x => x.Naziv).ToListAsync(),
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
            [Bind("IDSerije,BrojSezona,BrojEpizoda,ZavrsenoEmitovanje,Id,Naziv,Opis,ProsjecnaOcjena,Reditelj,GodinaIzlaska,YoutubeLink,Trajanje,PosterLink")] Serija serija,
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
            return View(serija);
        }

        // POST: Serija/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin,Moderator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IDSerije,BrojSezona,BrojEpizoda,ZavrsenoEmitovanje,Id,Naziv,Opis,ProsjecnaOcjena,Reditelj,GodinaIzlaska,YoutubeLink,Trajanje,PosterLink")] Serija serija)
        {
            if (id != serija.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (!_fileValidationService.IsAllowedPoster(serija.PosterLink))
                {
                    ModelState.AddModelError(nameof(Serija.PosterLink), "Poster mora biti .jpg ili .png.");
                    return View(serija);
                }

                try
                {
                    _context.Update(serija);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SerijaExists(serija.Id))
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
            return View(serija);
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
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var serija = await _context.Serija.FindAsync(id);
            if (serija != null)
            {
                _context.Serija.Remove(serija);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SerijaExists(int id)
        {
            return _context.Serija.Any(e => e.Id == id);
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
            foreach (var zanrId in zanrIds.Distinct().Where(x => x > 0))
            {
                _context.EntertainmentZanr.Add(new EntertainmentZanr
                {
                    EntertainmentId = entertainmentId,
                    ZanrId = zanrId
                });
            }

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

        private static IEnumerable<string> SplitLines(string? value)
        {
            return (value ?? string.Empty)
                .Split(new[] { "\r\n", "\n", ";", "," }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(x => !string.IsNullOrWhiteSpace(x));
        }
    }
}
