using System.ComponentModel.DataAnnotations;

namespace GymMvc.Models;

public class Member
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Range(0, 300)]
    public decimal Height { get; set; }

    [Range(0, 500)]
    public decimal Weight { get; set; }

    [StringLength(250)]
    public string Goal { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string FitnessLevel { get; set; } = string.Empty;

    public string? ApplicationUserId { get; set; }

    public ApplicationUser? ApplicationUser { get; set; }

    public ICollection<WeightLog> WeightLogs { get; set; }
        = new List<WeightLog>();

    public ICollection<CheckIn> CheckIns { get; set; }
        = new List<CheckIn>();
}