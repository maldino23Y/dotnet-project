using System.Collections.Generic;
using System;

namespace SuiviEntrainementSportif.Models
{
    public class DailyWorkout
    {
        public int Id { get; set; }
        public int WorkoutPlanId { get; set; }
        public WorkoutPlan? WorkoutPlan { get; set; }
        public DateTime Date { get; set; }
        public string Title { get; set; } = "Workout";
        public List<string>? Exercises { get; set; }
        public int DurationMinutes { get; set; }
        public string? Intensity { get; set; }
    }
}
