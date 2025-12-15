using System;

namespace SuiviEntrainementSportif.Models
{
    public class Entrainement
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Duree { get; set; }
        public int Calories { get; set; }

        // Views expect CaloriesBrulees — expose as a property
        public int CaloriesBrulees
        {
            get => Calories;
            set => Calories = value;
        }

        public DateTime Date { get; set; }
    }
}
