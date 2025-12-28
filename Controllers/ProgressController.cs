using Microsoft.AspNetCore.Mvc;
using SuiviEntrainementSportif.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using System.Collections.Generic;

namespace SuiviEntrainementSportif.Controllers
{
    public class ProgressController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ProgressController(ApplicationDbContext db)
        {
            _db = db;
        }

        // returns JSON with dates[] and values[] representing total minutes per day over last 30 days
        public async Task<JsonResult> WeeklyActivity(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return Json(new { dates = new string[0], values = new int[0] });
            var from = DateTime.UtcNow.Date.AddDays(-29);
            var data = await _db.Entrainements
                .Where(e => e.ApplicationUserId == userId && e.Date.Date >= from)
                .GroupBy(e => e.Date.Date)
                .Select(g => new { Date = g.Key, Minutes = g.Sum(x => x.Duree) })
                .ToListAsync();

            var dates = new List<string>();
            var vals = new List<int>();
            for (int i = 0; i < 30; i++)
            {
                var d = from.AddDays(i);
                dates.Add(d.ToString("yyyy-MM-dd"));
                var entry = data.FirstOrDefault(x => x.Date == d);
                vals.Add(entry?.Minutes ?? 0);
            }

            return Json(new { dates, values = vals });
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
