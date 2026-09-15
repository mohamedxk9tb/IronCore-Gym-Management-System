namespace GymMvc.ViewModels;

public class MemberDashboardViewModel
{
    public string FullName { get; set; } = string.Empty;

    public decimal Height { get; set; }

    public decimal CurrentWeight { get; set; }

    public decimal? PreviousWeight { get; set; }

    public string Goal { get; set; } = string.Empty;

    public int MonthlyCheckIns { get; set; }

    public int TotalWeightLogs { get; set; }

    public decimal? WeightChange { get; set; }

    public DateTime? LastCheckIn { get; set; }

    public DateTime? LastWeightLog { get; set; }

    // Active Subscription
    public string SubscriptionStatus { get; set; } =
        "No Active Subscription";

    public string? PlanName { get; set; }

    public DateTime? SubscriptionEndDate { get; set; }

    public int DaysRemaining { get; set; }

    // Next Booked Class
    public string? NextClassName { get; set; }

    public DateTime? NextClassDateTime { get; set; }

    public int? NextClassAvailableSpots { get; set; }
}