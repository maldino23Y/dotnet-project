using System;
using System.Collections.Generic;

namespace SuiviEntrainementSportif.Models
{
    public class Utilisateur
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime? DateNaissance { get; set; }
        public double Poids { get; set; }
        public double Taille { get; set; }
        public int? ObjectifId { get; set; }
        public Objectif? Objectif { get; set; }

        // Collection of trainings — prevents "does not contain a definition for 'Entrainements'"
        public List<Entrainement> Entrainements { get; set; } = new List<Entrainement>();
    }
}
