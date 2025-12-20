using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace SuiviEntrainementSportif.Models
{
    public class ApplicationUser : IdentityUser
    {
        // extra properties if needed
        public string? Nom { get; set; }
        public string? Prenom { get; set; }

        // navigation
        public List<Entrainement>? Entrainements { get; set; }
    }
}   
