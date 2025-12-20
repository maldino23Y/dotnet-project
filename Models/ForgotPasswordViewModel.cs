using System.ComponentModel.DataAnnotations;

namespace SuiviEntrainementSportif.Models
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
    }
}
