using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuiviEntrainementSportif.Models
{
    public class Entrainement
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le nom est requis.")]
        [StringLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères.")]
        public string Nom { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le type est requis.")]
        [StringLength(50, ErrorMessage = "Le type ne peut pas dépasser 50 caractères.")]
        public string Type { get; set; } = string.Empty;

        [Range(0, 1440, ErrorMessage = "La durée doit être comprise entre 0 et 1440 minutes.")]
        public int Duree { get; set; }

        [Range(0, 100000, ErrorMessage = "Les calories doivent être un nombre positif.")]
        public int Calories { get; set; }

        // Views expect CaloriesBrulees — keep compatibility
        public int CaloriesBrulees
        {
            get => Calories;
            set => Calories = value;
        }

        [DataType(DataType.Date)]
        [Required(ErrorMessage = "La date est requise.")]
        public DateTime Date { get; set; }

        // ----- Link to ApplicationUser -----
        // foreign key to AspNetUsers table (Identity user Id is string)
        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        [ForeignKey(nameof(ApplicationUserId))]
        public ApplicationUser? ApplicationUser { get; set; }
    }
}
