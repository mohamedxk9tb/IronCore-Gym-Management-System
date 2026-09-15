using GymMvc.Models;

namespace GymMvc.ViewModels
{
    public class PlanCardViewModel
    {
        public Plan Plan { get; set; }
        public bool IsSubscribed { get; set; }
        public int ActiveSubscribersCount { get; set; }
    }

    public class PlanListViewModel
    {
        public List<PlanCardViewModel> Plans { get; set; } = new List<PlanCardViewModel>();
    }
}
