using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GymMvc.ViewModels
{
    public class TrainerListItemViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public int ReviewsCount { get; set; }
    }

    public class TrainerProfileViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public string Certificates { get; set; } = string.Empty;
        public double AverageRating { get; set; }

        public List<TrainerReviewItemViewModel> Reviews { get; set; } = new();
        public List<string> ClassNames { get; set; } = new();
    }

    public class TrainerReviewItemViewModel
    {
        public string MemberName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
    }

    public class TrainerFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "الاسم مطلوب"), StringLength(100)]
        [Display(Name = "اسم المدرب")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "التخصص مطلوب"), StringLength(100)]
        [Display(Name = "التخصص")]
        public string Specialty { get; set; } = string.Empty;

        [StringLength(250)]
        [Display(Name = "الشهادات")]
        public string Certificates { get; set; } = string.Empty;
    }

    public class TrainerReviewFormViewModel
    {
        public int TrainerId { get; set; }
        public string TrainerName { get; set; } = string.Empty;

        [Range(1, 5, ErrorMessage = "التقييم من 1 لـ 5")]
        public int Rating { get; set; }

        [StringLength(500)]
        public string Comment { get; set; } = string.Empty;
    }
}