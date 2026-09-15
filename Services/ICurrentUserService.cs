using System.Threading.Tasks;

namespace GymMvc.Services
{
    // Resolves the logged-in Identity user to their Trainer/Member row.
    // Replaces unverified "TrainerId"/"MemberId" claims used previously.
    public interface ICurrentUserService
    {
        Task<int?> GetCurrentTrainerIdAsync();
        Task<int?> GetCurrentMemberIdAsync();
    }
}