using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymMvc.Models
{
    public class WorkoutExercise
    {
        public int Id { get; set; }

        [ForeignKey(nameof(WorkoutPlan))]
        public int WorkoutPlanId { get; set; }
        public WorkoutPlan WorkoutPlan { get; set; } = null!;

        [Required, StringLength(100)]
        public string ExerciseName { get; set; } = string.Empty;

        [Range(1, 50)]
        public int Sets { get; set; }

        [Range(1, 100)]
        public int Reps { get; set; }
    }
}