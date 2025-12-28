using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SuiviEntrainementSportif.Models
{
    public enum GenderEnum { Male, Female, Other }

    public class PlanRequestViewModel
    {
        [Required]
        [Range(10, 100)]
        public int Age { get; set; }

        [Required]
        [Range(100, 250)]
        public int HeightCm { get; set; }

        [Required]
        [Range(30, 300)]
        public int WeightKg { get; set; }

        [Required]
        public GenderEnum Gender { get; set; }

        [Required]
        public FitnessLevel Level { get; set; }

        [Required]
        public List<string> Goals { get; set; } = new List<string>();

        [Required]
        [Range(1,7)]
        public int DaysPerWeek { get; set; } = 4;

        // Optional: specific weekdays selected by the user (0=Sunday..6=Saturday)
        public List<int> SelectedWeekDays { get; set; } = new List<int>();
    }
}
