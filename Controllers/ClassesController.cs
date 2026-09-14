using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GymMvc.Data;
using GymMvc.ViewModels;

namespace GymMvc.Controllers;

public class ClassesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ClassesController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var classes = await _context.GymClasses
            .Include(c => c.Trainer)
            .Include(c => c.Bookings)
            .OrderBy(c => c.DayOfWeek)
            .ThenBy(c => c.StartTime)
            .ToListAsync();

        int? memberId = null;
        var currentUserId = _userManager.GetUserId(User);

        if (currentUserId != null)
        {
            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.ApplicationUserId == currentUserId);

            if (member != null)
            {
                memberId = member.Id;
            }
        }

        var viewModel = new ClassListViewModel();

        foreach (var gymClass in classes)
        {
            var bookedCount = gymClass.Bookings.Count(b => b.Status == "Booked");
            var availableSlots = gymClass.Capacity - bookedCount;

            var isBookedByCurrentMember = memberId != null && gymClass.Bookings.Any(b => b.MemberId == memberId && b.Status == "Booked");

            viewModel.Classes.Add(new ClassCardViewModel
            {
                GymClass = gymClass,
                TrainerName = gymClass.Trainer.FullName,
                BookedCount = bookedCount,
                AvailableSlots = availableSlots > 0 ? availableSlots : 0,
                IsFull = availableSlots <= 0,
                IsBookedByCurrentMember = isBookedByCurrentMember
            });
        }

        return View(viewModel);
    }
}