using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        // POST: /AiCoach/GeneratePlan/{userId}
        [HttpPost]
        public async Task<IActionResult> GeneratePlan(string userId)
        {
            // allow users to generate their own plan; admins can generate for anyone
            if (User.Identity?.Name != null && (User.IsInRole("Admin") || User.Identity.Name == userId || User.FindFirst("sub")?.Value == userId))
            {
                var workout = await _ai.GenerateWorkoutPlanAsync(userId);
                var meal = await _ai.GenerateMealPlanAsync(userId);
                return RedirectToAction(nameof(ViewPlan), new { userId });
            }
            return Forbid();
        }

        // GET: /AiCoach/ViewPlan/{userId}
        public async Task<IActionResult> ViewPlan(string userId)
        {
            // security: only allow viewing own plans unless admin
            if (!User.IsInRole("Admin") && User.FindFirst("sub")?.Value != userId && User.Identity?.Name != userId)
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
