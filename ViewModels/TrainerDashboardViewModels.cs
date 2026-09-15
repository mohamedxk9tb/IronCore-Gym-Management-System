using System.Collections.Generic;

namespace GymMvc.ViewModels
{
    public class TrainerDashboardViewModel
    {
        public string TrainerName { get; set; } = string.Empty;

        public List<ClassViewModel> Classes { get; set; } = new();
    }

    public class ClassViewModel
    {
        public int Id { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;

        public List<MemberAttendanceViewModel> Members { get; set; } = new();
    }

    public class MemberAttendanceViewModel
    {
        public int BookingId { get; set; }
        public int MemberId { get; set; }
        public string MemberName { get; set; } = string.Empty;
        public bool IsPresent { get; set; }
    }
}