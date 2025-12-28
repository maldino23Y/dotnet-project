using System;
using System.Linq;
using System.Threading.Tasks;
using SuiviEntrainementSportif.Data;
using Microsoft.EntityFrameworkCore;

namespace SuiviEntrainementSportif.Services
{
    public class StreakService : IStreakService
    {
        private readonly ApplicationDbContext _db;
        public StreakService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<int> GetCurrentStreakAsync(string userId)
        {
            // compute consecutive days with at least one session ending today backwards
            var sessions = await _db.Entrainements.Where(e => e.ApplicationUserId == userId)
                .Select(e => e.Date.Date)
                .Distinct()
                .OrderByDescending(d => d)
                .ToListAsync();

            int streak = 0;
            var today = DateTime.UtcNow.Date;
            foreach (var day in sessions)
            {
                if (day == today.AddDays(-streak))
                {
                    streak++;
                }
                else if (day < today.AddDays(-streak))
                {
                    break;
                }
            }

            return streak;
        }
    }
}
