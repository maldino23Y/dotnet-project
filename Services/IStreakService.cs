using System.Threading.Tasks;

namespace SuiviEntrainementSportif.Services
{
    public interface IStreakService
    {
        Task<int> GetCurrentStreakAsync(string userId);
    }
}
