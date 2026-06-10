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
using System.Security.Claims;

namespace BMDb.Controllers
{
    public class NotifikacijaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotifikacijaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Notifikacija
        [Authorize]
        public async Task<IActionResult> Index()
        {
            if (!await CurrentUserHasNotificationsEnabledAsync())
            {
                return View(Array.Empty<Notifikacija>());
            }

            var notifikacije = await _context.Notifikacija
                .AsNoTracking()
                .OrderByDescending(x => x.DatumObjave)
                .ToListAsync();

            return View(notifikacije);
        }

        // GET: Notifikacija/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            if (!await CurrentUserHasNotificationsEnabledAsync())
            {
                return NotFound();
            }

            var notifikacija = await _context.Notifikacija
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (notifikacija == null)
            {
                return NotFound();
            }

            return View(notifikacija);
        }

        // GET: Notifikacija/Create
        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Notifikacija/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin,Moderator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Tekst,DatumObjave,Slika")] Notifikacija notifikacija)
        {
            if (ModelState.IsValid)
            {
                _context.Add(notifikacija);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(notifikacija);
        }

        // GET: Notifikacija/Edit/5
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notifikacija = await _context.Notifikacija.FindAsync(id);
            if (notifikacija == null)
            {
                return NotFound();
            }
            return View(notifikacija);
        }

        // POST: Notifikacija/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin,Moderator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Tekst,DatumObjave,Slika")] Notifikacija notifikacija)
        {
            if (id != notifikacija.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(notifikacija);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NotifikacijaExists(notifikacija.Id))
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
            return View(notifikacija);
        }

        // GET: Notifikacija/Delete/5
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notifikacija = await _context.Notifikacija
                .FirstOrDefaultAsync(m => m.Id == id);
            if (notifikacija == null)
            {
                return NotFound();
            }

            return View(notifikacija);
        }

        // POST: Notifikacija/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin,Moderator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var notifikacija = await _context.Notifikacija.FindAsync(id);
            if (notifikacija != null)
            {
                _context.Notifikacija.Remove(notifikacija);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NotifikacijaExists(int id)
        {
            return _context.Notifikacija.Any(e => e.Id == id);
        }

        private async Task<bool> CurrentUserHasNotificationsEnabledAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return false;
            }

            return await _context.Users
                .AsNoTracking()
                .Where(x => x.Id == userId)
                .Select(x => x.NotifikacijeUkljucene)
                .FirstOrDefaultAsync();
        }
    }
}
