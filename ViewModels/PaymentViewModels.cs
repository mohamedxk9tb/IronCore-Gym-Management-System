using GymMvc.Models;

namespace GymMvc.ViewModels
{
    public class PaymentRowViewModel
    {
        public Payment Payment { get; set; }
        public string PlanName { get; set; }
    }

    public class PaymentListViewModel
    {
        public List<PaymentRowViewModel> Payments { get; set; } = new List<PaymentRowViewModel>();
    }

    public class PaymentCreateViewModel
    {
        public Subscription Subscription { get; set; }
        public string PlanName { get; set; }
        public decimal PlanPrice { get; set; }
    }
}