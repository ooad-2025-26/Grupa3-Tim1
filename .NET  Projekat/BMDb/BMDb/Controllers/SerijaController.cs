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
        public IActionResult Create()
        {
            return View();
        }

        // POST: Serija/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IDSerije,BrojSezona,BrojEpizoda,ZavrsenoEmitovanje,Id,Naziv,Opis,ProsjecnaOcjena,Reditelj,GodinaIzlaska,YoutubeLink,Trajanje,PosterLink")] Serija serija)
        {
            if (ModelState.IsValid)
            {
                if (!_fileValidationService.IsAllowedPoster(serija.PosterLink))
                {
                    ModelState.AddModelError(nameof(Serija.PosterLink), "Poster mora biti .jpg ili .png.");
                    return View(serija);
                }

                _context.Add(serija);
                await _context.SaveChangesAsync();
                await _notificationService.OnContentCreatedAsync(serija);
                return RedirectToAction(nameof(Index));
            }
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
    }
}
