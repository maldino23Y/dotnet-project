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
        Task<WorkoutPlan> GenerateWorkoutPlanAsync(string userId, int daysPerWeek);
        Task<WorkoutPlan> GenerateWorkoutPlanAsync(string userId, System.Collections.Generic.List<int>? selectedWeekDays);
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

        public async Task<ApplicationUser?> FindUserForControllerAsync(string identifier)
        {
            return await FindUserAsync(identifier);
        }

        public async Task UpdateUserAsync(ApplicationUser user)
        {
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
        }

        // Basal metabolic rate (Mifflin-St Jeor) and TDEE estimate
        private (decimal? Bmr, decimal? Tdee) EstimateCalories(ApplicationUser user)
        {
            if (user?.WeightKg == null || user?.HeightCm == null || user?.Age == null || string.IsNullOrWhiteSpace(user.Gender)) return (null, null);

            // Mifflin-St Jeor
            var weight = (decimal)user.WeightKg.Value;
            var height = (decimal)user.HeightCm.Value;
            var age = (decimal)user.Age.Value;
            decimal bmr;
            if (user.Gender.ToLowerInvariant().StartsWith("f"))
            {
                bmr = 10m * weight + 6.25m * height - 5m * age - 161m;
            }
            else
            {
                bmr = 10m * weight + 6.25m * height - 5m * age + 5m;
            }

            // activity multiplier
            var level = (user.ActivityLevel ?? "moderate").ToLowerInvariant();
            decimal multiplier = 1.375m; // light
            if (level.Contains("sedent") || level.Contains("low")) multiplier = 1.2m;
            else if (level.Contains("moderate")) multiplier = 1.55m;
            else if (level.Contains("high") || level.Contains("very active")) multiplier = 1.725m;

            var tdee = Math.Round(bmr * multiplier);
            return (Math.Round(bmr), tdee);
        }

        // Build an exercise bank grouped by focus
        private List<(string Focus, List<string> Exercises)> BuildExerciseBank(IEnumerable<Exercise>? fromDb)
        {
            var bank = new List<(string, List<string>)>();
            if (fromDb != null && fromDb.Any())
            {
                bank = fromDb.GroupBy(e => e.Type ?? "General")
                    .Select(g => (g.Key, g.Select(x => x.Name + (string.IsNullOrWhiteSpace(x.Description) ? string.Empty : $": {x.Description}" )).ToList()))
                    .ToList();
            }

            if (!bank.Any())
            {
                bank = new List<(string, List<string>)>
                {
                    ("Upper", new List<string>{ "Push-ups 3x12", "Dumbbell bench press 4x8", "Bent-over rows 4x8", "Overhead press 3x10" }),
                    ("Lower", new List<string>{ "Back squats 4x6-8", "Romanian deadlift 3x8", "Walking lunges 3x12", "Leg press 3x10" }),
                    ("Core", new List<string>{ "Plank 3x45s", "Hanging leg raises 3x12", "Russian twists 3x20" }),
                    ("Cardio", new List<string>{ "Interval run 20min", "Bike 30min steady", "Rowing 20min intervals" }),
                    ("FullBody", new List<string>{ "Kettlebell swings 4x15", "Burpees 3x12", "Thrusters 3x10" }),
                    ("Recovery", new List<string>{ "Yoga / mobility 30min", "Brisk walk 30-45min" })
                };
            }

            return bank;
        }

        // Generate a personalized 7-day workout plan (wrapper)
        public async Task<WorkoutPlan> GenerateWorkoutPlanAsync(string userId)
        {
            return await GenerateWorkoutPlanAsync(userId, 7);
        }

        // Generate a personalized workout plan for a specified number of training days per week
        public async Task<WorkoutPlan> GenerateWorkoutPlanAsync(string userId, int daysPerWeek)
        {
            var user = await FindUserAsync(userId);
            if (user == null) throw new ArgumentException("User not found");
            var goal = (user.FitnessGoal ?? "maintain").ToLowerInvariant();
            var rnd = new Random();

            // load exercises when available
            List<Exercise> exercisesFromDb = new List<Exercise>();
            try { exercisesFromDb = await _db.Exercises.ToListAsync(); } catch { exercisesFromDb = new List<Exercise>(); }
            var bank = BuildExerciseBank(exercisesFromDb);

            var (bmr, tdee) = EstimateCalories(user);

            var plan = new WorkoutPlan
            {
                UserId = user.Id,
                WeekStart = DateTime.UtcNow.Date,
                Created = DateTime.UtcNow,
                Days = new List<DailyWorkout>()
            };

            // Typical weekly split to ensure specific focus each day
            var weeklySchedule = new[]
            {
                (Title: "Upper Body Strength", Focus: "Upper", IntensityBase: "High", DurationBase: 50),
                (Title: "Lower Body Strength", Focus: "Lower", IntensityBase: "High", DurationBase: 50),
                (Title: "Cardio & Core", Focus: "Cardio", IntensityBase: "Moderate", DurationBase: 35),
                (Title: "Full Body Hypertrophy", Focus: "FullBody", IntensityBase: "High", DurationBase: 45),
                (Title: "Active Recovery / Mobility", Focus: "Recovery", IntensityBase: "Light", DurationBase: 30),
                (Title: "Lower Power / Plyometrics", Focus: "Lower", IntensityBase: "High", DurationBase: 40),
                (Title: "Mixed Cardio & Conditioning", Focus: "Cardio", IntensityBase: "Moderate", DurationBase: 30)
            };

            // determine which weekday indices to populate based on desired daysPerWeek
            var chosenIndices = new List<int>();
            daysPerWeek = Math.Max(1, Math.Min(7, daysPerWeek));
            for (int k = 0; k < daysPerWeek; k++)
            {
                int idx = (int)Math.Floor(k * 7.0 / daysPerWeek);
                if (idx < 0) idx = 0;
                if (idx > 6) idx = 6;
                if (!chosenIndices.Contains(idx)) chosenIndices.Add(idx);
            }

            // fill remaining days (if any duplicates occurred) by adding next available weekdays
            int fillCandidate = 0;
            while (chosenIndices.Count < daysPerWeek)
            {
                if (!chosenIndices.Contains(fillCandidate)) chosenIndices.Add(fillCandidate);
                fillCandidate = (fillCandidate + 1) % 7;
            }

            foreach (var i in chosenIndices)
            {
                var date = plan.WeekStart.AddDays(i);
                var daySpec = weeklySchedule[i % weeklySchedule.Length];
                var day = new DailyWorkout
                {
                    Date = date,
                    Title = daySpec.Title,
                    Type = daySpec.Focus,
                    Intensity = daySpec.IntensityBase,
                    DurationMinutes = daySpec.DurationBase + rnd.Next(-5, 11)
                };

                var focusList = bank.FirstOrDefault(b => string.Equals(b.Focus, daySpec.Focus, StringComparison.OrdinalIgnoreCase)).Exercises;
                if (focusList == null || !focusList.Any())
                {
                    focusList = bank.SelectMany(x => x.Exercises).ToList();
                }

                var picks = focusList.OrderBy(x => rnd.Next()).Take(Math.Min(6, Math.Max(3, focusList.Count))).ToList();
                for (int j = 0; j < picks.Count; j++)
                {
                    var ex = picks[j];
                    if (!System.Text.RegularExpressions.Regex.IsMatch(ex, "\\d+x\\d+"))
                    {
                        if (daySpec.Focus == "Upper" || daySpec.Focus == "Lower" || daySpec.Focus == "FullBody")
                            picks[j] = ex + " - 3 sets of 8-12";
                    }
                }

                day.Exercises = string.Join("||", picks);

                if (goal.Contains("lose"))
                {
                    day.Intensity = day.Intensity == "Light" ? "Light" : "Moderate";
                    day.DurationMinutes = (int)(day.DurationMinutes * 1.05);
                }
                else if (goal.Contains("gain"))
                {
                    if (day.Type == "Cardio") day.Intensity = "Moderate";
                    if (day.Type == "Upper" || day.Type == "Lower" || day.Type == "FullBody") day.Intensity = "High";
                    day.DurationMinutes = (int)(day.DurationMinutes * 1.1);
                }

                plan.Days.Add(day);
            }

            _db.WorkoutPlans.Add(plan);
            await _db.SaveChangesAsync();
            return plan;
        }

        // Generate plan using explicit selected weekdays (0=Sunday..6=Saturday)
        public async Task<WorkoutPlan> GenerateWorkoutPlanAsync(string userId, System.Collections.Generic.List<int>? selectedWeekDays)
        {
            if (selectedWeekDays == null || !selectedWeekDays.Any())
            {
                return await GenerateWorkoutPlanAsync(userId, 7);
            }

            // sanitize indices
            var days = selectedWeekDays.Distinct().Select(d => Math.Max(0, Math.Min(6, d))).OrderBy(d => d).ToList();
            var user = await FindUserAsync(userId);
            if (user == null) throw new ArgumentException("User not found");

            var goal = (user.FitnessGoal ?? "maintain").ToLowerInvariant();
            var rnd = new Random();

            List<Exercise> exercisesFromDb = new List<Exercise>();
            try { exercisesFromDb = await _db.Exercises.ToListAsync(); } catch { exercisesFromDb = new List<Exercise>(); }
            var bank = BuildExerciseBank(exercisesFromDb);

            var plan = new WorkoutPlan { UserId = user.Id, WeekStart = DateTime.UtcNow.Date, Created = DateTime.UtcNow, Days = new List<DailyWorkout>() };

            var weeklySchedule = new[]
            {
                (Title: "Upper Body Strength", Focus: "Upper", IntensityBase: "High", DurationBase: 50),
                (Title: "Lower Body Strength", Focus: "Lower", IntensityBase: "High", DurationBase: 50),
                (Title: "Cardio & Core", Focus: "Cardio", IntensityBase: "Moderate", DurationBase: 35),
                (Title: "Full Body Hypertrophy", Focus: "FullBody", IntensityBase: "High", DurationBase: 45),
                (Title: "Active Recovery / Mobility", Focus: "Recovery", IntensityBase: "Light", DurationBase: 30),
                (Title: "Lower Power / Plyometrics", Focus: "Lower", IntensityBase: "High", DurationBase: 40),
                (Title: "Mixed Cardio & Conditioning", Focus: "Cardio", IntensityBase: "Moderate", DurationBase: 30)
            };

            foreach (var i in days)
            {
                var date = plan.WeekStart.AddDays(i);
                var daySpec = weeklySchedule[i % weeklySchedule.Length];
                var day = new DailyWorkout { Date = date, Title = daySpec.Title, Type = daySpec.Focus, Intensity = daySpec.IntensityBase, DurationMinutes = daySpec.DurationBase + rnd.Next(-5, 11) };

                var focusList = bank.FirstOrDefault(b => string.Equals(b.Focus, daySpec.Focus, StringComparison.OrdinalIgnoreCase)).Exercises;
                if (focusList == null || !focusList.Any()) focusList = bank.SelectMany(x => x.Exercises).ToList();

                var picks = focusList.OrderBy(x => rnd.Next()).Take(Math.Min(6, Math.Max(3, focusList.Count))).ToList();
                for (int j = 0; j < picks.Count; j++)
                {
                    var ex = picks[j];
                    if (!System.Text.RegularExpressions.Regex.IsMatch(ex, "\\d+x\\d+"))
                    {
                        if (daySpec.Focus == "Upper" || daySpec.Focus == "Lower" || daySpec.Focus == "FullBody") picks[j] = ex + " - 3 sets of 8-12";
                    }
                }

                day.Exercises = string.Join("||", picks);
                if (goal.Contains("lose")) { day.Intensity = day.Intensity == "Light" ? "Light" : "Moderate"; }
                if (goal.Contains("gain")) { if (day.Type == "Cardio") day.Intensity = "Moderate"; if (day.Type == "Upper" || day.Type == "Lower" || day.Type == "FullBody") day.Intensity = "High"; }

                plan.Days.Add(day);
            }

            _db.WorkoutPlans.Add(plan);
            await _db.SaveChangesAsync();
            return plan;
        }

        // Generate a richer weekly meal plan using TDEE and workout intensity
        public async Task<MealPlan> GenerateMealPlanAsync(string userId)
        {
            var user = await FindUserAsync(userId);
            if (user == null) throw new ArgumentException("User not found");
            var (bmr, tdee) = EstimateCalories(user);
            var goal = (user.FitnessGoal ?? "maintain").ToLowerInvariant();
            decimal dailyCalories = tdee ?? 2000;
            if (goal.Contains("lose")) dailyCalories = (tdee ?? 2000) - 300;
            if (goal.Contains("gain")) dailyCalories = (tdee ?? 2000) + 300;

            var workoutPlan = await GetWorkoutPlanAsync(userId);

            // base meal building blocks
            var breakfasts = new[]
            {
                "Oatmeal with milk, banana and walnuts",
                "Greek yogurt, berries and granola",
                "Three-egg omelette with spinach and tomato",
                "Smoothie: milk, protein, oats, fruit",
                "Wholegrain toast, avocado and poached egg"
            };

            var lunches = new[]
            {
                "Grilled chicken breast, quinoa, mixed vegetables",
                "Salmon fillet, sweet potato, green beans",
                "Turkey & avocado wholegrain wrap, salad",
                "Buddha bowl: brown rice, chickpeas, veg, tahini",
                "Beef stir-fry with mixed vegetables and rice"
            };

            var dinners = new[]
            {
                "Baked white fish, roasted vegetables, small potato",
                "Stir-fried tofu with broccoli and brown rice",
                "Lean steak, salad and steamed veg",
                "Pasta with tomato sauce, lean protein and salad",
                "Chicken curry with basmati rice and veg"
            };

            var snacks = new[]
            {
                "Apple with peanut butter",
                "Handful of mixed nuts",
                "Cottage cheese with pineapple",
                "Protein bar or shake",
                "Carrot sticks with hummus"
            };

            var plan = new MealPlan
            {
                UserId = user.Id,
                WeekStart = workoutPlan?.WeekStart ?? DateTime.UtcNow.Date,
                Created = DateTime.UtcNow,
                Days = new List<DailyMeal>()
            };

            var rnd = new Random();

            if (workoutPlan != null && workoutPlan.Days != null && workoutPlan.Days.Any())
            {
                // Create meal entries only for days present in the workout plan
                foreach (var w in workoutPlan.Days.OrderBy(d => d.Date))
                {
                    var date = w.Date.Date;
                    var intensity = (w.Intensity ?? "moderate").ToLowerInvariant();

                    var b = breakfasts[rnd.Next(breakfasts.Length)];
                    var l = lunches[rnd.Next(lunches.Length)];
                    var d = dinners[rnd.Next(dinners.Length)];
                    var s1 = snacks[rnd.Next(snacks.Length)];
                    var s2 = snacks[rnd.Next(snacks.Length)];

                    decimal breakfastShare = 0.25m, lunchShare = 0.4m, dinnerShare = 0.3m;
                    if (intensity.Contains("high")) { breakfastShare += 0.03m; lunchShare += 0.02m; }
                    if (goal.Contains("lose")) { breakfastShare -= 0.02m; dinnerShare -= 0.03m; lunchShare += 0.01m; }

                    var meal = new DailyMeal
                    {
                        Date = date,
                        Breakfast = b + " - Snack: " + s1 + $" (~{(int)(dailyCalories * breakfastShare)} kcal)",
                        Lunch = l + " - Snack: " + s2 + $" (~{(int)(dailyCalories * lunchShare)} kcal)",
                        Dinner = d + $" (~{(int)(dailyCalories * dinnerShare)} kcal)",
                        Type = intensity + (goal.Contains("gain") ? "-HighProtein" : goal.Contains("lose") ? "-LowCal" : "-Balanced")
                    };

                    plan.Days.Add(meal);
                }
            }
            else
            {
                // Fallback: generate a full 7-day meal plan when no workout plan exists
                for (int i = 0; i < 7; i++)
                {
                    var date = plan.WeekStart.AddDays(i);
                    var intensity = "moderate";

                    var b = breakfasts[(rnd.Next(breakfasts.Length) + i) % breakfasts.Length];
                    var l = lunches[(rnd.Next(lunches.Length) + i + 1) % lunches.Length];
                    var d = dinners[(rnd.Next(dinners.Length) + i + 2) % dinners.Length];
                    var s1 = snacks[(rnd.Next(snacks.Length) + i + 3) % snacks.Length];
                    var s2 = snacks[(rnd.Next(snacks.Length) + i + 4) % snacks.Length];

                    decimal breakfastShare = 0.25m, lunchShare = 0.4m, dinnerShare = 0.3m;
                    if (goal.Contains("lose")) { breakfastShare -= 0.02m; dinnerShare -= 0.03m; lunchShare += 0.01m; }

                    var meal = new DailyMeal { Date = date };
                    meal.Breakfast = b + " - Snack: " + s1 + $" (~{(int)(dailyCalories * breakfastShare)} kcal)";
                    meal.Lunch = l + " - Snack: " + s2 + $" (~{(int)(dailyCalories * lunchShare)} kcal)";
                    meal.Dinner = d + $" (~{(int)(dailyCalories * dinnerShare)} kcal)";
                    meal.Type = intensity + (goal.Contains("gain") ? "-HighProtein" : goal.Contains("lose") ? "-LowCal" : "-Balanced");

                    plan.Days.Add(meal);
                }
            }

            _db.MealPlans.Add(plan);
            await _db.SaveChangesAsync();
            return plan;
        }

        public async Task<WorkoutPlan?> GetWorkoutPlanAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return null;
            var user = await FindUserAsync(userId);
            if (user == null) return null;
            return await _db.WorkoutPlans.Include(w => w.Days).Where(w => w.UserId == user.Id).OrderByDescending(w => w.Created).FirstOrDefaultAsync();
        }

        public async Task<MealPlan?> GetMealPlanAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return null;
            var user = await FindUserAsync(userId);
            if (user == null) return null;
            return await _db.MealPlans.Include(m => m.Days).Where(m => m.UserId == user.Id).OrderByDescending(m => m.Created).FirstOrDefaultAsync();
        }
    }
}
