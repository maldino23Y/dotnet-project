using System;

namespace SuiviEntrainementSportif.Models
{
    public class Objectif
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public int CibleCalories { get; set; }
        public int CibleDuree { get; set; }
    }
}
