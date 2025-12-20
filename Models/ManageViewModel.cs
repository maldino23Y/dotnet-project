using System.ComponentModel.DataAnnotations;

namespace SuiviEntrainementSportif.Models
{
    public class ManageViewModel
    {
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Last Name")]
        public string Nom { get; set; } = string.Empty;

        [Display(Name = "First Name")]
        public string Prenom { get; set; } = string.Empty;

        [Display(Name = "Status message")]
        public string? StatusMessage { get; set; }
    }
}
