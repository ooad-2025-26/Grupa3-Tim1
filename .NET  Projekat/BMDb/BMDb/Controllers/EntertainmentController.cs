using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BMDb.Data;
using BMDb.Models;
using BMDb.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace BMDb.Controllers
{
    public class EntertainmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EntertainmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Entertainment
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Index()
        {
            var filmovi = await _context.Film
                .Select(x => new EntertainmentListItemViewModel
                {
                    Id = x.Id,
                    Naziv = x.Naziv ?? string.Empty,
                    Tip = "Film",
                    GodinaIzlaska = x.GodinaIzlaska,
                    Ocjena = x.ProsjecnaOcjena,
                    PosterLink = x.PosterLink,
                    ControllerName = "Film"
                })
                .ToListAsync();

            var serije = await _context.Serija
                .Select(x => new EntertainmentListItemViewModel
                {
                    Id = x.Id,
                    Naziv = x.Naziv ?? string.Empty,
                    Tip = "Serija",
                    GodinaIzlaska = x.GodinaIzlaska,
                    Ocjena = x.ProsjecnaOcjena,
                    PosterLink = x.PosterLink,
                    ControllerName = "Serija"
                })
                .ToListAsync();

            return View(filmovi.Concat(serije).OrderBy(x => x.Naziv).ToList());
        }

        // GET: Entertainment/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entertainment = await _context.Entertainment
                .FirstOrDefaultAsync(m => m.Id == id);
            if (entertainment == null)
            {
                return NotFound();
            }

            return View(entertainment);
        }

        // GET: Entertainment/Create
        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Entertainment/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin,Moderator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Naziv,Opis,ProsjecnaOcjena,Reditelj,GodinaIzlaska,YoutubeLink,Trajanje,PosterLink")] Entertainment entertainment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(entertainment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(entertainment);
        }

        // GET: Entertainment/Edit/5
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entertainment = await _context.Entertainment.FindAsync(id);
            if (entertainment == null)
            {
                return NotFound();
            }
            return View(entertainment);
        }

        // POST: Entertainment/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin,Moderator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Naziv,Opis,ProsjecnaOcjena,Reditelj,GodinaIzlaska,YoutubeLink,Trajanje,PosterLink")] Entertainment entertainment)
        {
            if (id != entertainment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(entertainment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EntertainmentExists(entertainment.Id))
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
            return View(entertainment);
        }

        // GET: Entertainment/Delete/5
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entertainment = await _context.Entertainment
                .FirstOrDefaultAsync(m => m.Id == id);
            if (entertainment == null)
            {
                return NotFound();
            }

            return View(entertainment);
        }

        // POST: Entertainment/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin,Moderator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entertainment = await _context.Entertainment.FindAsync(id);
            if (entertainment != null)
            {
                _context.Entertainment.Remove(entertainment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EntertainmentExists(int id)
        {
            return _context.Entertainment.Any(e => e.Id == id);
        }
    }
}
