using Microsoft.AspNetCore.Mvc;
using SuiviEntrainementSportif.Services;
using System.Threading.Tasks;

namespace SuiviEntrainementSportif.ViewComponents
{
    public class StreakCounterViewComponent : ViewComponent
    {
        private readonly IStreakService _streak;
        public StreakCounterViewComponent(IStreakService streak)
        {
            _streak = streak;
        }

        public async Task<IViewComponentResult> InvokeAsync(string userId)
        {
            var count = await _streak.GetCurrentStreakAsync(userId);
            return View(count);
        }
    }
}
