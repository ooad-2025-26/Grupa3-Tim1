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
    public class EntertainmentZanrController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EntertainmentZanrController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: EntertainmentZanr
        public async Task<IActionResult> Index()
        {
            return View(await _context.EntertainmentZanr.ToListAsync());
        }

        // GET: EntertainmentZanr/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entertainmentZanr = await _context.EntertainmentZanr
                .FirstOrDefaultAsync(m => m.IDVeze == id);
            if (entertainmentZanr == null)
            {
                return NotFound();
            }

            return View(entertainmentZanr);
        }

        // GET: EntertainmentZanr/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: EntertainmentZanr/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IDVeze,EntertainmentId,ZanrId")] EntertainmentZanr entertainmentZanr)
        {
            if (ModelState.IsValid)
            {
                _context.Add(entertainmentZanr);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(entertainmentZanr);
        }

        // GET: EntertainmentZanr/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entertainmentZanr = await _context.EntertainmentZanr.FindAsync(id);
            if (entertainmentZanr == null)
            {
                return NotFound();
            }
            return View(entertainmentZanr);
        }

        // POST: EntertainmentZanr/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IDVeze,EntertainmentId,ZanrId")] EntertainmentZanr entertainmentZanr)
        {
            if (id != entertainmentZanr.IDVeze)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(entertainmentZanr);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EntertainmentZanrExists(entertainmentZanr.IDVeze))
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
            return View(entertainmentZanr);
        }

        // GET: EntertainmentZanr/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entertainmentZanr = await _context.EntertainmentZanr
                .FirstOrDefaultAsync(m => m.IDVeze == id);
            if (entertainmentZanr == null)
            {
                return NotFound();
            }

            return View(entertainmentZanr);
        }

        // POST: EntertainmentZanr/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entertainmentZanr = await _context.EntertainmentZanr.FindAsync(id);
            if (entertainmentZanr != null)
            {
                _context.EntertainmentZanr.Remove(entertainmentZanr);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EntertainmentZanrExists(int id)
        {
            return _context.EntertainmentZanr.Any(e => e.IDVeze == id);
        }
    }
}
