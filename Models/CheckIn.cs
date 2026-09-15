using System.ComponentModel.DataAnnotations.Schema;

namespace GymMvc.Models;

public class CheckIn
{
    public int Id { get; set; }

    public int MemberId { get; set; }

    public DateTime CheckInTime { get; set; }

    [ForeignKey(nameof(MemberId))]
    public Member Member { get; set; } = null!;
}