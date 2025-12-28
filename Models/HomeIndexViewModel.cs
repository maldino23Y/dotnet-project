using System.Collections.Generic;
using SuiviEntrainementSportif.Models;

namespace SuiviEntrainementSportif.Models
{
    public class HomeIndexViewModel
    {
        public List<Entrainement> RecentWorkouts { get; set; } = new List<Entrainement>();
        public int TotalSessions { get; set; }
        public int TotalCalories { get; set; }
        public int CurrentStreak { get; set; }
    }
}
