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
    public class GlumacController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GlumacController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Glumac
        public async Task<IActionResult> Index()
        {
            return View(await _context.Glumac.ToListAsync());
        }

        // GET: Glumac/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var glumac = await _context.Glumac
                .FirstOrDefaultAsync(m => m.Id == id);
            if (glumac == null)
            {
                return NotFound();
            }

            return View(glumac);
        }

        // GET: Glumac/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Glumac/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Ime,Prezime,Slika")] Glumac glumac)
        {
            if (ModelState.IsValid)
            {
                _context.Add(glumac);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(glumac);
        }

        // GET: Glumac/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var glumac = await _context.Glumac.FindAsync(id);
            if (glumac == null)
            {
                return NotFound();
            }
            return View(glumac);
        }

        // POST: Glumac/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Ime,Prezime,Slika")] Glumac glumac)
        {
            if (id != glumac.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(glumac);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GlumacExists(glumac.Id))
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
            return View(glumac);
        }

        // GET: Glumac/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var glumac = await _context.Glumac
                .FirstOrDefaultAsync(m => m.Id == id);
            if (glumac == null)
            {
                return NotFound();
            }

            return View(glumac);
        }

        // POST: Glumac/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var glumac = await _context.Glumac.FindAsync(id);
            if (glumac != null)
            {
                _context.Glumac.Remove(glumac);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GlumacExists(int id)
        {
            return _context.Glumac.Any(e => e.Id == id);
        }
    }
}
