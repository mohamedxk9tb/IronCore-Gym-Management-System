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

        // ------ Navigation Properties ------
        public ICollection<WorkoutPlan> WorkoutPlans { get; set; } = new List<WorkoutPlan>();
        public ICollection<TrainerReview> Reviews { get; set; } = new List<TrainerReview>();

        // GymClass نفسه بتاع مروان - إحنا محتاجين الـ navigation بس عشان علاقة TEACHES
        public ICollection<GymClass> GymClasses { get; set; } = new List<GymClass>();
    }
}