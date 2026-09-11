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
}