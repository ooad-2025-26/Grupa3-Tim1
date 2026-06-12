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

namespace BMDb.Controllers
{
    [Authorize(Roles = "Admin,Moderator")]
    public class GalerijaSlikaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GalerijaSlikaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: GalerijaSlika
        public async Task<IActionResult> Index()
        {
            return View(await _context.GalerijaSlika.ToListAsync());
        }

        // GET: GalerijaSlika/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var galerijaSlika = await _context.GalerijaSlika
                .FirstOrDefaultAsync(m => m.Id == id);
            if (galerijaSlika == null)
            {
                return NotFound();
            }

            return View(galerijaSlika);
        }

        // GET: GalerijaSlika/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: GalerijaSlika/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EntertainmentId,Url")] GalerijaSlika galerijaSlika)
        {
            if (ModelState.IsValid)
            {
                _context.Add(galerijaSlika);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(galerijaSlika);
        }

        // GET: GalerijaSlika/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var galerijaSlika = await _context.GalerijaSlika.FindAsync(id);
            if (galerijaSlika == null)
            {
                return NotFound();
            }
            return View(galerijaSlika);
        }

        // POST: GalerijaSlika/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,EntertainmentId,Url")] GalerijaSlika galerijaSlika)
        {
            if (id != galerijaSlika.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(galerijaSlika);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GalerijaSlikaExists(galerijaSlika.Id))
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
            return View(galerijaSlika);
        }

        // GET: GalerijaSlika/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var galerijaSlika = await _context.GalerijaSlika
                .FirstOrDefaultAsync(m => m.Id == id);
            if (galerijaSlika == null)
            {
                return NotFound();
            }

            return View(galerijaSlika);
        }

        // POST: GalerijaSlika/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var galerijaSlika = await _context.GalerijaSlika.FindAsync(id);
            if (galerijaSlika != null)
            {
                _context.GalerijaSlika.Remove(galerijaSlika);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GalerijaSlikaExists(int id)
        {
            return _context.GalerijaSlika.Any(e => e.Id == id);
        }
    }
}
