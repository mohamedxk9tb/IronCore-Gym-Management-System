using System.ComponentModel.DataAnnotations;

namespace GymMvc.ViewModels;

public class MemberProgressViewModel
{
    public decimal CurrentWeight { get; set; }

    public decimal StartingWeight { get; set; }

    public DateTime? StartingDate { get; set; }

    public decimal? TotalChange { get; set; }

    public int TotalRecords { get; set; }

    public List<WeightProgressPoint> WeightHistory { get; set; }
        = new();

    [Required]
    [Range(0.1, 500)]
    [Display(Name = "Weight (kg)")]
    public decimal NewWeight { get; set; }

    [Required]
    [Display(Name = "Date")]
    public DateTime NewWeightDate { get; set; } = DateTime.Today;
}

public class WeightProgressPoint
{
    public DateTime Date { get; set; }

    public decimal Weight { get; set; }
}