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

        public int Sets { get; set; }
        public int Reps { get; set; }
    }
}