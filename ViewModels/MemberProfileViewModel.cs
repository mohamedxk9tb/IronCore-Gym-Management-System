using System.ComponentModel.DataAnnotations;

namespace GymMvc.ViewModels;

public class MemberProfileViewModel
{
    [Required]
    [StringLength(100)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Range(0, 300)]
    [Display(Name = "Height (cm)")]
    public decimal Height { get; set; }

    [Range(0, 500)]
    [Display(Name = "Weight (kg)")]
    public decimal Weight { get; set; }

    [StringLength(250)]
    [Display(Name = "Fitness goal")]
    public string Goal { get; set; } = string.Empty;
}