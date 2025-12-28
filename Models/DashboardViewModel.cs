using System.Collections.Generic;

namespace SuiviEntrainementSportif.Models
{
    public class DashboardViewModel
    {
        public List<Entrainement> RecentWorkouts { get; set; } = new List<Entrainement>();
        public int StreakDays { get; set; }
        public int TotalWorkouts { get; set; }
        public int CaloriesBurned { get; set; }
    }
}
