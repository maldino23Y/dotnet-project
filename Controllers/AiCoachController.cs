using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using SuiviEntrainementSportif.Services;
using System.Threading.Tasks;

namespace SuiviEntrainementSportif.Controllers
{
    [Authorize]
    public class AiCoachController : Controller
    {
        private readonly IAiFitnessService _ai;

        public AiCoachController(IAiFitnessService ai)
        {
            _ai = ai;
        }

        // GET: /AiCoach/GeneratePlan/{userId} - show form to collect user info
        [HttpGet]
        public async Task<IActionResult> GeneratePlan(string? userId)
        {
            var currentUserName = User.Identity?.Name;
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserName == null && currentUserId == null) return Challenge();

            // return the view model expected by the GeneratePlan view
            var model = new SuiviEntrainementSportif.Models.PlanRequestViewModel();
            // prefill some values if you like
            model.Age = 30;
            model.HeightCm = 175;
            model.WeightKg = 75;
            model.Gender = SuiviEntrainementSportif.Models.GenderEnum.Male;
            model.Level = SuiviEntrainementSportif.Models.FitnessLevel.Intermediate;
            model.DaysPerWeek = 4;

            return View(model);
        }

        // POST: /AiCoach/GeneratePlan
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GeneratePlan(SuiviEntrainementSportif.Models.PlanRequestViewModel request)
        {
            if (!ModelState.IsValid) return View(request);

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(currentUserId)) return Forbid();

            var user = await _ai.FindUserForControllerAsync(currentUserId);
            if (user == null) return NotFound();

            // map values
            user.Age = request.Age;
            user.HeightCm = request.HeightCm;
            user.WeightKg = request.WeightKg;
            user.Gender = request.Gender.ToString().ToLowerInvariant();
            user.ActivityLevel = request.Level.ToString();
            user.FitnessGoal = (request.Goals != null && request.Goals.Count > 0) ? request.Goals[0] : "maintain";

            await _ai.UpdateUserAsync(user);
            if (request.SelectedWeekDays != null && request.SelectedWeekDays.Any())
            {
                await _ai.GenerateWorkoutPlanAsync(user.Id, request.SelectedWeekDays);
            }
            else
            {
                await _ai.GenerateWorkoutPlanAsync(user.Id, request.DaysPerWeek);
            }
            await _ai.GenerateMealPlanAsync(user.Id);

            return RedirectToAction(nameof(ViewPlan));
        }

        // New POST for PlanRequest (accepts the ViewModel used by the form)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generate(Models.PlanRequestViewModel request)
        {
            if (!ModelState.IsValid)
            {
                // When invalid, return the same view model type the view expects
                return View("GeneratePlan", request);
            }

            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(currentUserId)) return Forbid();

            var user = await _ai.FindUserForControllerAsync(currentUserId);
            if (user == null) return Forbid();

            user.Age = request.Age;
            user.HeightCm = request.HeightCm;
            user.WeightKg = request.WeightKg;
            user.Gender = request.Gender.ToString().ToLowerInvariant();
            user.ActivityLevel = request.Level.ToString();
            user.FitnessGoal = (request.Goals != null && request.Goals.Count > 0) ? request.Goals[0] : "maintain";

            await _ai.UpdateUserAsync(user);
            if (request.SelectedWeekDays != null && request.SelectedWeekDays.Any())
            {
                await _ai.GenerateWorkoutPlanAsync(user.Id, request.SelectedWeekDays);
            }
            else
            {
                await _ai.GenerateWorkoutPlanAsync(user.Id, request.DaysPerWeek);
            }
            await _ai.GenerateMealPlanAsync(user.Id);

            return RedirectToAction(nameof(ViewPlan));
        }

        // GET: /AiCoach/ViewPlan/{userId}
        public async Task<IActionResult> ViewPlan(string userId)
        {
            var currentUserName = User.Identity?.Name;
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // if no userId provided, view current user's plan
            if (string.IsNullOrWhiteSpace(userId))
            {
                userId = currentUserId ?? currentUserName;
            }

            // security: only allow viewing own plans unless admin
            if (!User.IsInRole("Admin") && currentUserId != userId && currentUserName != userId)
            {
                return Forbid();
            }

            var workout = await _ai.GetWorkoutPlanAsync(userId);
            var meal = await _ai.GetMealPlanAsync(userId);
            var vm = new AiPlanViewModel { WorkoutPlan = workout, MealPlan = meal };
            return View(vm);
        }
    }

    public class AiPlanViewModel
    {
        public SuiviEntrainementSportif.Models.WorkoutPlan? WorkoutPlan { get; set; }
        public SuiviEntrainementSportif.Models.MealPlan? MealPlan { get; set; }
    }
}
