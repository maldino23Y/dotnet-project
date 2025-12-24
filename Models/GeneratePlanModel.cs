using System.ComponentModel.DataAnnotations;

namespace SuiviEntrainementSportif.Models
{
    public class GeneratePlanModel
    {
        public string? UserId { get; set; }

        [Required]
        [Range(10, 120)]
        public int Age { get; set; }

        [Required]
        [Range(50, 250)]
        public decimal HeightCm { get; set; }

        [Required]
        [Range(20, 500)]
        public decimal WeightKg { get; set; }

        [Required]
        public string Gender { get; set; } = "male";

        [Required]
        public string FitnessGoal { get; set; } = "maintain"; // lose weight, gain muscle, maintain

        [Required]
        public string ActivityLevel { get; set; } = "moderate"; // sedentary, light, moderate, active
    }
}
