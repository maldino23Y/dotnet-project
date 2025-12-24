using System;
using System;
using System.Collections.Generic;
using System.Linq;
using SuiviEntrainementSportif.Data;
using SuiviEntrainementSportif.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SuiviEntrainementSportif.Services
{
    public interface IAiFitnessService
    {
        Task<WorkoutPlan> GenerateWorkoutPlanAsync(string userId);
        Task<MealPlan> GenerateMealPlanAsync(string userId);
        Task<WorkoutPlan?> GetWorkoutPlanAsync(string userId);
        Task<MealPlan?> GetMealPlanAsync(string userId);
        Task<ApplicationUser?> FindUserForControllerAsync(string identifier);
        Task UpdateUserAsync(ApplicationUser user);
    }

    public class AiFitnessService : IAiFitnessService
    {
        private readonly ApplicationDbContext _db;

        public AiFitnessService(ApplicationDbContext db)
        {
            _db = db;
        }

        private async Task<ApplicationUser?> FindUserAsync(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier)) return null;
            return await _db.Users.FirstOrDefaultAsync(u => u.Id == identifier || u.UserName == identifier || u.Email == identifier);
        }

        // Helper methods exposed via interface for controller usage
        public async Task<ApplicationUser?> FindUserForControllerAsync(string identifier)
        {
            return await FindUserAsync(identifier);
        }

        public async Task UpdateUserAsync(ApplicationUser user)
        {
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
        }

        // Simple rule-based BMI calculation
        private decimal? CalculateBmi(ApplicationUser user)
        {
            if (user?.HeightCm == null || user?.WeightKg == null) return null;
            var h = (decimal)user.HeightCm / 100m;
            return Math.Round(user.WeightKg.Value / (h * h), 1);
        }

        // Generate a 7-day workout plan
        public async Task<WorkoutPlan> GenerateWorkoutPlanAsync(string userId)
        {
            var user = await FindUserAsync(userId);
            if (user == null) throw new ArgumentException("User not found");

            var bmi = CalculateBmi(user);
            var activity = (user.ActivityLevel ?? "moderate").ToLowerInvariant();
            var goal = (user.FitnessGoal ?? "maintain").ToLowerInvariant();

            var plan = new WorkoutPlan
            {
                UserId = user.Id,
                WeekStart = DateTime.UtcNow.Date,
                Days = new List<DailyWorkout>()
            };

            // Basic rules
            var rnd = new Random();
            var exerciseOptions = new List<(string Type, List<string> Exercises)>
            {
                ("Cardio", new List<string>{ "Warm-up 10min", "Interval run 20min", "Bodyweight circuit: pushups/squats/planks 3 rounds", "Cool-down" }),
                ("Cardio", new List<string>{ "Warm-up 10min", "Hill sprints or cycling 20min", "Core + mobility 15min" }),
                ("Strength", new List<string>{ "Warm-up 10min", "Strength: squats / deadlifts 4x6", "Accessory: lunges / rows" }),
                ("Strength", new List<string>{ "Warm-up 10min", "Strength: bench / overhead press 4x6", "Accessory: pullups / dips" }),
                ("Recovery", new List<string>{ "Active recovery: long walk or yoga 30-45min" }),
                ("Circuit", new List<string>{ "Circuit: kettlebell swings, burpees, box jumps 4 rounds" }),
                ("Mixed", new List<string>{ "Mixed cardio: bike/run + stretching" })
            };

            for (int i = 0; i < 7; i++)
            {
                var date = plan.WeekStart.AddDays(i);
                var day = new DailyWorkout { Date = date };

                // choose intensity
                if (goal == "lose weight")
                {
                    day.Intensity = "Moderate";
                    day.DurationMinutes = 40 + rnd.Next(-5, 10);
                    var pick = exerciseOptions[rnd.Next(exerciseOptions.Count)];
                    day.Type = pick.Type;
                    day.Exercises = string.Join("||", pick.Exercises);
                }
                else if (goal == "gain muscle")
                {
                    day.Intensity = "High";
                    day.DurationMinutes = 45 + rnd.Next(0, 20);
                    var pick2 = exerciseOptions[rnd.Next(exerciseOptions.Count)];
                    day.Type = pick2.Type;
                    day.Exercises = string.Join("||", pick2.Exercises);
                }
                else
                {
                    // maintain
                    day.Intensity = "Light-Moderate";
                    day.DurationMinutes = 25 + rnd.Next(0, 20);
                    var pick3 = exerciseOptions[rnd.Next(exerciseOptions.Count)];
                    day.Type = pick3.Type;
                    day.Exercises = string.Join("||", pick3.Exercises);
                }

                // Add a rest/light day every 3rd day for high intensity
                if (i % 3 == 2 && day.Intensity == "High")
                {
                    day.Intensity = "Light";
                    day.DurationMinutes = 25;
                    day.Exercises = string.Join("||", new List<string>{ "Active recovery: walking or stretching" });
                }

                plan.Days.Add(day);
            }

            // Save plan
            _db.WorkoutPlans.Add(plan);
            await _db.SaveChangesAsync();
            return plan;
        }

        // Generate a simple weekly meal plan
        public async Task<MealPlan> GenerateMealPlanAsync(string userId)
        {
            var user = await FindUserAsync(userId);
            if (user == null) throw new ArgumentException("User not found");

            var goal = (user.FitnessGoal ?? "maintain").ToLowerInvariant();

            // simple calorie heuristic
            decimal calories = 2000;
            if (goal == "lose weight") calories = 1800;
            if (goal == "gain muscle") calories = 2500;

            var plan = new MealPlan
            {
                UserId = user.Id,
                WeekStart = DateTime.UtcNow.Date,
                Days = new List<DailyMeal>()
            };

            var mealOptions = new List<(string Type, string Breakfast, string Lunch, string Dinner)>
            {
                ("Balanced", "Oatmeal + fruit + egg", "Grilled chicken, quinoa, veg", "Salmon + salad"),
                ("HighProtein", "Greek yogurt + nuts", "Steak, sweet potato, veg", "Grilled chicken + veg"),
                ("LowCarb", "Omelette + spinach", "Salad with tuna", "Grilled fish + broccoli"),
                ("Recovery", "Smoothie + oats", "Soup + wholegrain bread", "Light protein + salad"),
                ("Balanced", "Porridge + banana", "Turkey wrap + salad", "Veggie stir-fry + tofu"),
                ("HighProtein", "Cottage cheese + fruit", "Chicken bowl + brown rice", "Beef stir-fry"),
                ("LowCal", "Fruit salad + yogurt", "Large salad + eggs", "Soup + salad")
            };

            for (int i = 0; i < 7; i++)
            {
                var date = plan.WeekStart.AddDays(i);
                var meal = new DailyMeal { Date = date };
                var pick = mealOptions[rnd.Next(mealOptions.Count)];
                meal.Breakfast = pick.Breakfast + $" (~{(int)(calories * 0.25m)} kcal)";
                meal.Lunch = pick.Lunch + $" (~{(int)(calories * 0.4m)} kcal)";
                meal.Dinner = pick.Dinner + $" (~{(int)(calories * 0.35m)} kcal)";
                meal.Type = pick.Type;
                plan.Days.Add(meal);
            }

            _db.MealPlans.Add(plan);
            await _db.SaveChangesAsync();
            return plan;
        }

        public async Task<WorkoutPlan?> GetWorkoutPlanAsync(string userId)
        {
            var user = await FindUserAsync(userId);
            if (user == null) return null;
            return await _db.WorkoutPlans.Include(w => w.Days).Where(w => w.UserId == user.Id).OrderByDescending(w => w.Created).FirstOrDefaultAsync();
        }

        public async Task<MealPlan?> GetMealPlanAsync(string userId)
        {
            var user = await FindUserAsync(userId);
            if (user == null) return null;
            return await _db.MealPlans.Include(m => m.Days).Where(m => m.UserId == user.Id).OrderByDescending(m => m.Created).FirstOrDefaultAsync();
        }
    }
}
