using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SuiviEntrainementSportif.Models
{
    public class WorkoutPlan
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public ApplicationUser? User { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime WeekStart { get; set; }
        public List<DailyWorkout>? Days { get; set; }
    }
}
