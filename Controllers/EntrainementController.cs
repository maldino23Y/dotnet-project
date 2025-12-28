using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuiviEntrainementSportif.Data;
using SuiviEntrainementSportif.Models;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace SuiviEntrainementSportif.Controllers
{
    [Authorize]
    public class EntrainementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EntrainementController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Entrainement
        public async Task<IActionResult> Index()
        {
            var list = await _context.Entrainements
                                     .OrderByDescending(e => e.Date)
                                     .ToListAsync();
            return View(list);
        }

        // GET: /Entrainement/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Entrainement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Entrainement e)
        {
            // require authenticated user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // attach current user id before validation
            e.ApplicationUserId = userId;

            // default date to today if none provided
            if (e.Date == default) e.Date = DateTime.UtcNow.Date;

            // ModelState was populated during model binding and may contain errors for
            // ApplicationUserId and Date (because they were empty during binding). Remove
            // those entries so validation runs against the updated values.
            ModelState.Remove(nameof(Entrainement.ApplicationUserId));
            ModelState.Remove(nameof(Entrainement.Date));

            if (!ModelState.IsValid) return View(e);

            _context.Entrainements.Add(e);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Entrainement/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var entr = await _context.Entrainements.FindAsync(id.Value);
            if (entr == null) return NotFound();

            return View(entr);
        }

        // POST: /Entrainement/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Entrainement e)
        {
            // Find existing first to preserve ApplicationUserId and avoid validation errors
            var existing = await _context.Entrainements.FindAsync(e.Id);
            if (existing == null) return NotFound();

            // Preserve user id
            e.ApplicationUserId = existing.ApplicationUserId;
            // If date not provided keep existing
            if (e.Date == default) e.Date = existing.Date;

            // Remove server-side validation entries that were empty during binding
            ModelState.Remove(nameof(Entrainement.ApplicationUserId));
            ModelState.Remove(nameof(Entrainement.Date));

            if (!ModelState.IsValid) return View(e);

            // Update fields
            existing.Nom = e.Nom;
            existing.Type = e.Type;
            existing.Duree = e.Duree;
            existing.Calories = e.Calories;
            existing.Date = e.Date;

            _context.Entrainements.Update(existing);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /Entrainement/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var entr = await _context.Entrainements.FindAsync(id.Value);
            if (entr != null)
            {
                _context.Entrainements.Remove(entr);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
