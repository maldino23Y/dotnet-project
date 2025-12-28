using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SuiviEntrainementSportif.Models
{
    public enum FitnessLevel { Beginner, Intermediate, Advanced }
    public enum Goal { LoseWeight, GainMuscle, Maintain }

    public class PlanRequest
    {
        [Required]
        [Range(10, 100)]
        public int Age { get; set; }

        [Required]
        [Range(100, 250)]
        public double HeightCm { get; set; }

        [Required]
        [Range(30, 300)]
        public double WeightKg { get; set; }

        [Required]
        public string Gender { get; set; } = "male";

        [Required]
        public FitnessLevel Level { get; set; } = FitnessLevel.Intermediate;

        [Required]
        public List<Goal> Goals { get; set; } = new List<Goal>();

        [Required]
        [Range(1,7)]
        public int DaysPerWeek { get; set; } = 4;
    }
}
