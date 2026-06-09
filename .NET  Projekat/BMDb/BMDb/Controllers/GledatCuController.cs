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
    [Authorize]
    public class GledatCuController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GledatCuController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: GledatCu
        public async Task<IActionResult> Index()
        {
            return View(await _context.GledatCu.ToListAsync());
        }

        // GET: GledatCu/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gledatCu = await _context.GledatCu
                .FirstOrDefaultAsync(m => m.Id == id);
            if (gledatCu == null)
            {
                return NotFound();
            }

            return View(gledatCu);
        }

        // GET: GledatCu/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: GledatCu/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,OsobaId,EntertainmentId")] GledatCu gledatCu)
        {
            if (ModelState.IsValid)
            {
                _context.Add(gledatCu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(gledatCu);
        }

        // GET: GledatCu/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gledatCu = await _context.GledatCu.FindAsync(id);
            if (gledatCu == null)
            {
                return NotFound();
            }
            return View(gledatCu);
        }

        // POST: GledatCu/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,OsobaId,EntertainmentId")] GledatCu gledatCu)
        {
            if (id != gledatCu.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(gledatCu);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GledatCuExists(gledatCu.Id))
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
            return View(gledatCu);
        }

        // GET: GledatCu/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gledatCu = await _context.GledatCu
                .FirstOrDefaultAsync(m => m.Id == id);
            if (gledatCu == null)
            {
                return NotFound();
            }

            return View(gledatCu);
        }

        // POST: GledatCu/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var gledatCu = await _context.GledatCu.FindAsync(id);
            if (gledatCu != null)
            {
                _context.GledatCu.Remove(gledatCu);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GledatCuExists(int id)
        {
            return _context.GledatCu.Any(e => e.Id == id);
        }
    }
}
