using System;
using System.Collections.Generic;

namespace SuiviEntrainementSportif.Models
{
    public class MealPlan
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public ApplicationUser? User { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime WeekStart { get; set; }
        // simple daily meals map: key = date (ISO), value = list of meals
        public List<DailyMeal>? Days { get; set; }
    }

    public class DailyMeal
    {
        public int Id { get; set; }
        public int MealPlanId { get; set; }
        public MealPlan? MealPlan { get; set; }
        public DateTime Date { get; set; }
        public string Breakfast { get; set; } = string.Empty;
        public string Lunch { get; set; } = string.Empty;
        public string Dinner { get; set; } = string.Empty;
        // add a simple Type to categorize the day's focus
        public string? Type { get; set; } // e.g. HighProtein, LowCarb, Balanced, Recovery
    }
}
