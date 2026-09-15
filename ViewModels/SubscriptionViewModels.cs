using GymMvc.Models;

namespace GymMvc.ViewModels
{
    public class SubscriptionRowViewModel
    {
        public Subscription Subscription { get; set; }
        public string PlanName { get; set; }
        public decimal PlanPrice { get; set; }
        public int DaysRemaining { get; set; }
        public bool IsExpired { get; set; }
    }

    public class SubscriptionListViewModel
    {
        public List<SubscriptionRowViewModel> Subscriptions { get; set; } = new List<SubscriptionRowViewModel>();
    }

    public class SubscriptionCreateViewModel
    {
        public Plan Plan { get; set; }
    }
}