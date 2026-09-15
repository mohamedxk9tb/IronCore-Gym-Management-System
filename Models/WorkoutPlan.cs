using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymMvc.Models
{
    public class WorkoutPlan
    {
        public int Id { get; set; }

        [ForeignKey(nameof(Member))]
        public int MemberId { get; set; }
        public Member Member { get; set; } = null!; // بتاع محمد

        [ForeignKey(nameof(Trainer))]
        public int TrainerId { get; set; }
        public Trainer Trainer { get; set; } = null!;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // ------ Navigation Properties ------
        public ICollection<WorkoutExercise> Exercises { get; set; } = new List<WorkoutExercise>();
        public ICollection<DietMeal> DietMeals { get; set; } = new List<DietMeal>();
    }
}