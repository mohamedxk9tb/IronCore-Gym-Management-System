using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GymMvc.ViewModels
{
    public class WorkoutExerciseInputViewModel
    {
        [Required(ErrorMessage = "اسم التمرين مطلوب"), StringLength(100)]
        public string ExerciseName { get; set; } = string.Empty;

        [Range(1, 50, ErrorMessage = "عدد الـ Sets لازم يكون بين 1 و 50")]
        public int Sets { get; set; }

        [Range(1, 100, ErrorMessage = "عدد الـ Reps لازم يكون بين 1 و 100")]
        public int Reps { get; set; }
    }

    public class DietMealInputViewModel
    {
        [Required(ErrorMessage = "اليوم مطلوب"), StringLength(20)]
        public string DayOfWeek { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم الوجبة مطلوب"), StringLength(100)]
        public string MealName { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
    }

    public class WorkoutDietPlanFormViewModel
    {
        // بياخد قيمته من اليوزر المسجل دخوله كمدرب، مش من الفورم (server-side فقط)
        public int TrainerId { get; set; }

        [Required(ErrorMessage = "لازم تختار العضو")]
        [Display(Name = "العضو")]
        public int MemberId { get; set; }

        public List<SelectListItem>? Members { get; set; }

        public List<WorkoutExerciseInputViewModel> Exercises { get; set; } = new();
        public List<DietMealInputViewModel> DietMeals { get; set; } = new();
    }

    public class WorkoutDietPlanDetailsViewModel
    {
        public int Id { get; set; }
        public string TrainerName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }

        public List<WorkoutExerciseInputViewModel> Exercises { get; set; } = new();
        public List<DietMealInputViewModel> DietMeals { get; set; } = new();
    }
}