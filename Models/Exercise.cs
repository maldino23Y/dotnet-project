using System.ComponentModel.DataAnnotations;

namespace SuiviEntrainementSportif.Models
{
    public class Exercise
n    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // Cardio, Strength, Mobility, Circuit
        public string Description { get; set; } = string.Empty;
    }
}
