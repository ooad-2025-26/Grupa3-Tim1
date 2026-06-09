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
        public IActionResult Create()
        {
            return View();
        }

        public IActionResult ComingSoon()
        {
            return View();
        }

        // POST: Film/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IDFilma,BoxOffice,Id,Naziv,Opis,ProsjecnaOcjena,Reditelj,GodinaIzlaska,YoutubeLink,Trajanje,PosterLink")] Film film)
        {
            if (ModelState.IsValid)
            {
                if (!_fileValidationService.IsAllowedPoster(film.PosterLink))
                {
                    ModelState.AddModelError(nameof(Film.PosterLink), "Poster mora biti .jpg ili .png.");
                    return View(film);
                }

                _context.Add(film);
                await _context.SaveChangesAsync();
                await _notificationService.OnContentCreatedAsync(film);
                return RedirectToAction(nameof(Index));
            }
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
            return View(film);
        }

        // POST: Film/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IDFilma,BoxOffice,Id,Naziv,Opis,ProsjecnaOcjena,Reditelj,GodinaIzlaska,YoutubeLink,Trajanje,PosterLink")] Film film)
        {
            if (id != film.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (!_fileValidationService.IsAllowedPoster(film.PosterLink))
                {
                    ModelState.AddModelError(nameof(Film.PosterLink), "Poster mora biti .jpg ili .png.");
                    return View(film);
                }

                try
                {
                    _context.Update(film);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FilmExists(film.Id))
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
            return View(film);
        }

        // GET: Film/Delete/5
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var film = await _context.Film.FindAsync(id);
            if (film != null)
            {
                _context.Film.Remove(film);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FilmExists(int id)
        {
            return _context.Film.Any(e => e.Id == id);
        }
    }
}
