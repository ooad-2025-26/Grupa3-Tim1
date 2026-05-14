using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BMDb.Data;
using BMDb.Models;

namespace BMDb.Controllers
{
    public class SerijaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SerijaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Serija
        public async Task<IActionResult> Index()
        {
            return View(await _context.Serija.ToListAsync());
        }

        // GET: Serija/Details/5
        public async Task<IActionResult> Details(int? id)
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
                _context.Add(serija);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(serija);
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
