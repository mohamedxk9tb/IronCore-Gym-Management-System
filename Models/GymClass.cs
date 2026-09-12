using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymMvc.Models
{
    public class GymClass
    {
        public int Id { get; set; }

        [Required]
        public int TrainerId { get; set; }

        [ForeignKey("TrainerId")]
        public Trainer Trainer { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(20)]
        public string DayOfWeek { get; set; }

        [Required]
        [Range(1, 100)]
        public int Capacity { get; set; }

        public List<Booking> Bookings { get; set; } = new List<Booking>();
    }
}