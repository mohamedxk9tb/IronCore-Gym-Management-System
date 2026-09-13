using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymMvc.Data;
using GymMvc.Services;
using GymMvc.ViewModels;

namespace GymMvc.Controllers
{
    [Authorize(Roles = "Trainer")]
    public class TrainerDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public TrainerDashboardController(ApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<IActionResult> Dashboard()
        {
            var trainerId = await _currentUser.GetCurrentTrainerIdAsync();
            if (trainerId == null)
            {
                return Forbid();
            }

            var trainer = await _context.Trainers.FindAsync(trainerId.Value);
            if (trainer == null)
            {
                return NotFound();
            }

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
                    // GymClass.StartTime is assumed TimeSpan - adjust format if it's DateTime
                    Time = c.StartTime.ToString(@"hh\:mm"),
                    Members = c.Bookings
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAttendance(int bookingId, bool isPresent)
        {
            var trainerId = await _currentUser.GetCurrentTrainerIdAsync();
            if (trainerId == null)
            {
                return Forbid();
            }

            var booking = await _context.Bookings
                .Include(b => b.GymClass)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
            {
                return NotFound();
            }

            if (booking.GymClass.TrainerId != trainerId)
            {
                return Forbid();
            }

            booking.Status = isPresent ? "Attended" : "Booked";
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Dashboard));
        }
    }
}