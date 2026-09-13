using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymMvc.Data;
using GymMvc.ViewModels;

namespace GymMvc.Controllers
{
    // صفحة Trainer Dashboard: كلاسات المدرب اليوم + تسجيل حضور الأعضاء.
    // الحضور بيتسجل عن طريق تحديث Booking.Status (مفيش جدول Attendance منفصل).
    [Authorize(Roles = "Trainer")]
    public class TrainerDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrainerDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /TrainerDashboard/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var trainerId = GetCurrentTrainerId();

            var trainer = await _context.Trainers.FindAsync(trainerId);
            if (trainer == null)
            {
                return NotFound();
            }

            // ⚠️ افتراض: GymClass.DayOfWeek متخزن كنص مطابق لاسم اليوم بالإنجليزي ("Monday")
            var todayName = DateTime.Now.DayOfWeek.ToString();

            var todayClasses = await _context.GymClasses
                .Where(c => c.TrainerId == trainerId && c.DayOfWeek == todayName)
                .Include(c => c.Bookings)
                    .ThenInclude(b => b.Member)
                .ToListAsync();

            var model = new TrainerDashboardViewModel
            {
                TrainerName = trainer.FullName,
                Classes = todayClasses.Select(c => new ClassViewModel
                {
                    Id = c.Id,
                    ClassName = c.Name,
                    Time = todayName, // مفيش حقل وقت في GymClass دلوقتي - محتاج StartTime من مروان
                    Members = c.Bookings
                        // بنعرض الحجوزات الفعلية بس (مش الملغية)
                        .Where(b => b.Status != "Cancelled")
                        .Select(b => new MemberAttendanceViewModel
                        {
                            BookingId = b.Id,
                            MemberId = b.MemberId,
                            MemberName = b.Member.FullName,
                            IsPresent = b.Status == "Attended"
                        }).ToList()
                }).ToList()
            };

            return View(model);
        }

        // POST: /TrainerDashboard/MarkAttendance
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAttendance(int bookingId, bool isPresent)
        {
            var trainerId = GetCurrentTrainerId();

            var booking = await _context.Bookings
                .Include(b => b.GymClass)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
            {
                return NotFound();
            }

            // ownership check: المدرب مينفعش يعدّل حضور Booking لكلاس مدرب تاني
            if (booking.GymClass.TrainerId != trainerId)
            {
                return Forbid();
            }

            booking.Status = isPresent ? "Attended" : "Booked";
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Dashboard));
        }

        // ⚠️ placeholder - لازم يتظبط حسب نظام الـ Identity بتاع محمد
        // (إزاي المدرب المسجل دخوله مربوط بصف في جدول Trainer؟)
        private int GetCurrentTrainerId()
        {
            var claim = User.FindFirst("TrainerId")?.Value;
            return int.TryParse(claim, out var trainerId) ? trainerId : 0;
        }
    }
}