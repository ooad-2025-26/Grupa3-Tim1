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
using Microsoft.AspNetCore.Authorization;
using BMDb.ViewModels;

namespace BMDb.Controllers
{
    [Authorize]
    public class RecenzijaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IRecenzijaService _recenzijaService;
        private readonly IUserKeyService _userKeyService;

        public RecenzijaController(ApplicationDbContext context, IRecenzijaService recenzijaService, IUserKeyService userKeyService)
        {
            _context = context;
            _recenzijaService = recenzijaService;
            _userKeyService = userKeyService;
        }

        // GET: Recenzija
        public async Task<IActionResult> Index()
        {
            var osobaId = _userKeyService.GetCurrentUserKey(User);
            var recenzije = await _context.Recenzija
                .AsNoTracking()
                .Where(x => x.OsobaId == osobaId)
                .OrderByDescending(x => x.DatumObjave)
                .ToListAsync();

            var entertainmentIds = recenzije.Select(x => x.EntertainmentId).Distinct().ToList();
            var filmIds = await _context.Film
                .AsNoTracking()
                .Where(x => entertainmentIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync();
            var filmIdSet = filmIds.ToHashSet();

            var entertainment = await _context.Entertainment
                .AsNoTracking()
                .Where(x => entertainmentIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id);

            var model = recenzije
                .Where(x => entertainment.ContainsKey(x.EntertainmentId))
                .Select(x =>
                {
                    var item = entertainment[x.EntertainmentId];
                    return new ProfileReviewViewModel
                    {
                        Id = x.Id,
                        EntertainmentId = x.EntertainmentId,
                        Naziv = item.Naziv ?? string.Empty,
                        GodinaIzlaska = item.GodinaIzlaska,
                        PosterLink = item.PosterLink ?? string.Empty,
                        ControllerName = filmIdSet.Contains(x.EntertainmentId) ? "Film" : "Serija",
                        Ocjena = x.Ocjena,
                        Komentar = x.Komentar ?? string.Empty,
                        DatumObjave = x.DatumObjave
                    };
                })
                .ToList();

            return View(model);
        }

        // GET: Recenzija/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recenzija = await _context.Recenzija
                .FirstOrDefaultAsync(m => m.Id == id);
            if (recenzija == null)
            {
                return NotFound();
            }

            return View(recenzija);
        }

        // GET: Recenzija/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Recenzija/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Ocjena,Komentar,EntertainmentId")] Recenzija recenzija)
        {
            if (ModelState.IsValid)
            {
                var result = await _recenzijaService.DodajRecenzijuAsync(
                    _userKeyService.GetCurrentUserKey(User),
                    recenzija.EntertainmentId,
                    recenzija.Ocjena,
                    recenzija.Komentar);

                if (result.Success)
                {
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, result.Message);
            }

            return View(recenzija);
        }

        // GET: Recenzija/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recenzija = await _context.Recenzija.FindAsync(id);
            if (recenzija == null)
            {
                return NotFound();
            }
            return View(recenzija);
        }

        // POST: Recenzija/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Ocjena,Komentar,DatumObjave,OsobaId,EntertainmentId")] Recenzija recenzija)
        {
            if (id != recenzija.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(recenzija);
                    await _context.SaveChangesAsync();
                    await _recenzijaService.AzurirajProsjecnuOcjenuAsync(recenzija.EntertainmentId);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RecenzijaExists(recenzija.Id))
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
            return View(recenzija);
        }

        // GET: Recenzija/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recenzija = await _context.Recenzija
                .FirstOrDefaultAsync(m => m.Id == id);
            if (recenzija == null)
            {
                return NotFound();
            }

            return View(recenzija);
        }

        // POST: Recenzija/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var recenzija = await _context.Recenzija.FindAsync(id);
            if (recenzija != null)
            {
                var entertainmentId = recenzija.EntertainmentId;
                _context.Recenzija.Remove(recenzija);
                await _context.SaveChangesAsync();
                await _recenzijaService.AzurirajProsjecnuOcjenuAsync(entertainmentId);
                return RedirectToAction(nameof(Index));
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RecenzijaExists(int id)
        {
            return _context.Recenzija.Any(e => e.Id == id);
        }
    }
}
