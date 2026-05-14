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
    public class GledaoSamController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GledaoSamController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: GledaoSam
        public async Task<IActionResult> Index()
        {
            return View(await _context.GledaoSam.ToListAsync());
        }

        // GET: GledaoSam/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gledaoSam = await _context.GledaoSam
                .FirstOrDefaultAsync(m => m.Id == id);
            if (gledaoSam == null)
            {
                return NotFound();
            }

            return View(gledaoSam);
        }

        // GET: GledaoSam/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: GledaoSam/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,OsobaId,EntertainmentId")] GledaoSam gledaoSam)
        {
            if (ModelState.IsValid)
            {
                _context.Add(gledaoSam);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(gledaoSam);
        }

        // GET: GledaoSam/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gledaoSam = await _context.GledaoSam.FindAsync(id);
            if (gledaoSam == null)
            {
                return NotFound();
            }
            return View(gledaoSam);
        }

        // POST: GledaoSam/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,OsobaId,EntertainmentId")] GledaoSam gledaoSam)
        {
            if (id != gledaoSam.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(gledaoSam);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GledaoSamExists(gledaoSam.Id))
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
            return View(gledaoSam);
        }

        // GET: GledaoSam/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gledaoSam = await _context.GledaoSam
                .FirstOrDefaultAsync(m => m.Id == id);
            if (gledaoSam == null)
            {
                return NotFound();
            }

            return View(gledaoSam);
        }

        // POST: GledaoSam/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var gledaoSam = await _context.GledaoSam.FindAsync(id);
            if (gledaoSam != null)
            {
                _context.GledaoSam.Remove(gledaoSam);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GledaoSamExists(int id)
        {
            return _context.GledaoSam.Any(e => e.Id == id);
        }
    }
}
