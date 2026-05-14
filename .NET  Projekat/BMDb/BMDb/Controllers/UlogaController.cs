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
    public class UlogaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UlogaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Uloga
        public async Task<IActionResult> Index()
        {
            return View(await _context.Uloga.ToListAsync());
        }

        // GET: Uloga/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var uloga = await _context.Uloga
                .FirstOrDefaultAsync(m => m.Id == id);
            if (uloga == null)
            {
                return NotFound();
            }

            return View(uloga);
        }

        // GET: Uloga/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Uloga/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ImeLika,GlumacId,EntertainmentId")] Uloga uloga)
        {
            if (ModelState.IsValid)
            {
                _context.Add(uloga);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(uloga);
        }

        // GET: Uloga/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var uloga = await _context.Uloga.FindAsync(id);
            if (uloga == null)
            {
                return NotFound();
            }
            return View(uloga);
        }

        // POST: Uloga/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ImeLika,GlumacId,EntertainmentId")] Uloga uloga)
        {
            if (id != uloga.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(uloga);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UlogaExists(uloga.Id))
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
            return View(uloga);
        }

        // GET: Uloga/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var uloga = await _context.Uloga
                .FirstOrDefaultAsync(m => m.Id == id);
            if (uloga == null)
            {
                return NotFound();
            }

            return View(uloga);
        }

        // POST: Uloga/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var uloga = await _context.Uloga.FindAsync(id);
            if (uloga != null)
            {
                _context.Uloga.Remove(uloga);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UlogaExists(int id)
        {
            return _context.Uloga.Any(e => e.Id == id);
        }
    }
}
