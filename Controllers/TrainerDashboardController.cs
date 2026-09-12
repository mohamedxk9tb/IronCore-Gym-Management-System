using GymMvc.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GymMvc.Controllers
{
    public class TrainerDashboardController : Controller
    {
        private static readonly List<ClassViewModel> Classes = new()
        {
            new ClassViewModel
            {
                Id = 1,
                ClassName = "Strength Fundamentals",
                Time = "06:00 PM",
                Members = new List<MemberAttendanceViewModel>
                {
                    new MemberAttendanceViewModel
                    {
                        MemberId = 1,
                        MemberName = "Ahmed Ali"
                    },
                    new MemberAttendanceViewModel
                    {
                        MemberId = 2,
                        MemberName = "Mohamed Hassan"
                    }
                }
            },

            new ClassViewModel
            {
                Id = 2,
                ClassName = "Bodybuilding",
                Time = "08:00 PM",
                Members = new List<MemberAttendanceViewModel>
                {
                    new MemberAttendanceViewModel
                    {
                        MemberId = 3,
                        MemberName = "Omar Khaled"
                    }
                }
            }
        };

        public IActionResult Index()
        {
            var model = new TrainerDashboardViewModel
            {
                TrainerName = "Ahmed Ali",
                Classes = Classes
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult MarkAttendance(int classId, int memberId)
        {
            var gymClass = Classes.FirstOrDefault(x => x.Id == classId);

            var member = gymClass?.Members
                .FirstOrDefault(x => x.MemberId == memberId);

            if (member != null)
            {
                member.IsPresent = true;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}