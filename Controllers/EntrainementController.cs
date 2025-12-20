using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuiviEntrainementSportif.Data;
using SuiviEntrainementSportif.Models;
using System.Threading.Tasks;
using System.Linq;

namespace SuiviEntrainementSportif.Controllers
{
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
            if (!ModelState.IsValid) return View(e);

            var existing = await _context.Entrainements.FindAsync(e.Id);
            if (existing == null) return NotFound();

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
