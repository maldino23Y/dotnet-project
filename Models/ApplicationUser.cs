using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace SuiviEntrainementSportif.Models
{
    public class ApplicationUser : IdentityUser
    {
        // extra properties if needed
        public string? Nom { get; set; }
        public string? Prenom { get; set; }
        // personal data used by AI coach
        public int? Age { get; set; }
        public decimal? HeightCm { get; set; }
        public decimal? WeightKg { get; set; }
        public string? Gender { get; set; }
        public string? FitnessGoal { get; set; } // e.g. lose weight, gain muscle, maintain
        public string? ActivityLevel { get; set; } // e.g. sedentary, light, moderate, active

        // navigation
        public List<Entrainement>? Entrainements { get; set; }
        public List<WorkoutPlan>? WorkoutPlans { get; set; }
        public List<MealPlan>? MealPlans { get; set; }
    }
}   
