using GymMvc.Models;

namespace GymMvc.ViewModels
{
    public class ClassCardViewModel
    {
        public GymClass GymClass { get; set; }
        public string TrainerName { get; set; }
        public int BookedCount { get; set; }
        public int AvailableSlots { get; set; }
        public bool IsFull { get; set; }
        public bool IsBookedByCurrentMember { get; set; }
    }

    public class ClassListViewModel
    {
        public List<ClassCardViewModel> Classes { get; set; } = new List<ClassCardViewModel>();

        public string? CurrentSearch { get; set; }
        public string? CurrentDay { get; set; }
        public string? CurrentSort { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
    }
}