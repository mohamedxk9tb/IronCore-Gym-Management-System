using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GymMvc.Data;
using GymMvc.ViewModels;
using GymMvc.Models;

namespace GymMvc.Controllers;

public class ClassesController : Controller
{
    private const int PageSize = 10;

    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ClassesController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? search, string? day, string? sort, int page = 1)
    {
        var query = _context.GymClasses
            .Include(c => c.Trainer)
            .Include(c => c.Bookings)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => c.Name.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(day))
        {
            query = query.Where(c => c.DayOfWeek == day);
        }

        query = sort switch
        {
            "name" => query.OrderBy(c => c.Name),
            "name_desc" => query.OrderByDescending(c => c.Name),
            "time_desc" => query.OrderByDescending(c => c.StartTime),
            _ => query.OrderBy(c => c.DayOfWeek).ThenBy(c => c.StartTime)
        };

        var totalCount = await query.CountAsync();

        if (page < 1)
        {
            page = 1;
        }

        var classes = await query
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
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

        var viewModel = new ClassListViewModel
        {
            CurrentSearch = search,
            CurrentDay = day,
            CurrentSort = sort,
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize)
        };

        foreach (var gymClass in classes)
        {
            var bookedCount = gymClass.Bookings.Count(b => b.Status == "Booked");
            var availableSlots = gymClass.Capacity - bookedCount;

            var isBookedByCurrentMember = memberId != null &&
                gymClass.Bookings.Any(b => b.MemberId == memberId && b.Status == "Booked");

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