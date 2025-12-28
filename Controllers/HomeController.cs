using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SuiviEntrainementSportif.Models;
using Microsoft.Extensions.Logging;
using SuiviEntrainementSportif.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using SuiviEntrainementSportif.Services;
using System.Threading.Tasks;


namespace SuiviEntrainementSportif.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;
        private readonly IStreakService _streak;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db, IStreakService streak)
        {
            _logger = logger;
            _db = db;
            _streak = streak;
        }

        public async Task<IActionResult> Index()
        {
            var model = new DashboardViewModel();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return View(model);
            }

            var recent = await _db.Entrainements
                .Where(e => e.ApplicationUserId == userId)
                .OrderByDescending(e => e.Date)
                .Take(10)
                .ToListAsync();

            model.RecentWorkouts = recent;
            model.TotalWorkouts = await _db.Entrainements.CountAsync(e => e.ApplicationUserId == userId);
            model.CaloriesBurned = await _db.Entrainements.Where(e => e.ApplicationUserId == userId).SumAsync(e => (int?)e.CaloriesBrulees) ?? 0;
            model.StreakDays = await _streak.GetCurrentStreakAsync(userId);

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
