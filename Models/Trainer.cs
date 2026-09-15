using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GymMvc.Models
{
    public class Trainer
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Specialty { get; set; } = string.Empty;

        [StringLength(250)]
        public string Certificates { get; set; } = string.Empty;

        // Links this Trainer row to the logged-in Identity user.
        // Required for reliable current-trainer resolution (no unverified claims).
        [StringLength(450)]
        public string? ApplicationUserId { get; set; }

        public ICollection<WorkoutPlan> WorkoutPlans { get; set; } = new List<WorkoutPlan>();
        public ICollection<TrainerReview> Reviews { get; set; } = new List<TrainerReview>();
        public ICollection<GymClass> GymClasses { get; set; } = new List<GymClass>();
    }
}