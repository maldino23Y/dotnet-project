using System.ComponentModel.DataAnnotations;

namespace SuiviEntrainementSportif.Models
{
    public class EditProfileViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Last Name")]
        public string? Nom { get; set; }

        [Display(Name = "First Name")]
        public string? Prenom { get; set; }
    }
}
