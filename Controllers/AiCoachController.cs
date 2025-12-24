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
        public async Task<IActionResult> GeneratePlan(string userId)
        {
            var currentUserName = User.Identity?.Name;
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserName == null && currentUserId == null) return Challenge();

            var model = new SuiviEntrainementSportif.Models.GeneratePlanModel();
            model.UserId = string.IsNullOrWhiteSpace(userId) ? (currentUserId ?? currentUserName) : userId;
            return View(model);
        }

        // POST: /AiCoach/GeneratePlan
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GeneratePlan(SuiviEntrainementSportif.Models.GeneratePlanModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // store provided info into user profile (if current user)
            var user = await _ai.FindUserForControllerAsync(model.UserId);
            if (user == null) return NotFound();

            // update user profile values
            user.Age = model.Age;
            user.HeightCm = model.HeightCm;
            user.WeightKg = model.WeightKg;
            user.Gender = model.Gender;
            user.FitnessGoal = model.FitnessGoal;
            user.ActivityLevel = model.ActivityLevel;

            // save and generate plans
            await _ai.UpdateUserAsync(user);
            await _ai.GenerateWorkoutPlanAsync(user.Id);
            await _ai.GenerateMealPlanAsync(user.Id);
            return RedirectToAction(nameof(ViewPlan), new { userId = user.Id });
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
