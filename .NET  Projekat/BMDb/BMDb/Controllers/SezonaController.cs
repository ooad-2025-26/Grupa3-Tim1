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
    public class SezonaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SezonaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Sezona
        public async Task<IActionResult> Index()
        {
            return View(await _context.Sezona.ToListAsync());
        }

        // GET: Sezona/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sezona = await _context.Sezona
                .FirstOrDefaultAsync(m => m.IdSezone == id);
            if (sezona == null)
            {
                return NotFound();
            }

            return View(sezona);
        }

        // GET: Sezona/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Sezona/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdSezone,IdSerije,RedniBrojSezone,BrojEpizoda,DatumPremijere,PosterSezone")] Sezona sezona)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sezona);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sezona);
        }

        // GET: Sezona/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sezona = await _context.Sezona.FindAsync(id);
            if (sezona == null)
            {
                return NotFound();
            }
            return View(sezona);
        }

        // POST: Sezona/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdSezone,IdSerije,RedniBrojSezone,BrojEpizoda,DatumPremijere,PosterSezone")] Sezona sezona)
        {
            if (id != sezona.IdSezone)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sezona);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SezonaExists(sezona.IdSezone))
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
            return View(sezona);
        }

        // GET: Sezona/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sezona = await _context.Sezona
                .FirstOrDefaultAsync(m => m.IdSezone == id);
            if (sezona == null)
            {
                return NotFound();
            }

            return View(sezona);
        }

        // POST: Sezona/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sezona = await _context.Sezona.FindAsync(id);
            if (sezona != null)
            {
                _context.Sezona.Remove(sezona);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SezonaExists(int id)
        {
            return _context.Sezona.Any(e => e.IdSezone == id);
        }
    }
}
