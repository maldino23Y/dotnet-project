using System;

namespace SuiviEntrainementSportif.Models
{
    public class ObjectifSportif
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public int CaloriesCible { get; set; }
        public int DureeCible { get; set; }
    }
}
