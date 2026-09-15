using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymMvc.Models;

public class WeightLog
{
    public int Id { get; set; }

    public int MemberId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Range(0, 500)]
    public decimal Weight { get; set; }

    [ForeignKey(nameof(MemberId))]
    public Member Member { get; set; } = null!;
}