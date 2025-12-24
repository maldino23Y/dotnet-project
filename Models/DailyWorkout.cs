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
        // stored as a single serialized string in the DB (|| separated)
        public string? Exercises { get; set; }
        public int DurationMinutes { get; set; }
        public string? Intensity { get; set; }

        // helper to get exercises as list
        public List<string> GetExercisesList()
        {
            if (string.IsNullOrWhiteSpace(Exercises)) return new List<string>();
            return Exercises.Split(new[] { "||" }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
        }
    }
}
