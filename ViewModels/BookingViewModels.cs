using GymMvc.Models;

namespace GymMvc.ViewModels
{
    public class BookingRowViewModel
    {
        public Booking Booking { get; set; }
        public string ClassName { get; set; }
        public string TrainerName { get; set; }
        public string DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public bool IsCancelled { get; set; }
        public bool IsAttended { get; set; }
    }

    public class BookingListViewModel
    {
        public List<BookingRowViewModel> Bookings { get; set; } = new List<BookingRowViewModel>();
    }
}