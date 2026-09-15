using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using GymMvc.Data;
using GymMvc.Models;
using GymMvc.ViewModels;

namespace GymMvc.Controllers;

[Authorize(Roles = "Member")]
public class BookingController : Controller
{
    private const int CancellationCutoffHours = 2;

    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public BookingController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var member = await GetCurrentMemberAsync();

        if (member == null)
        {
            return NotFound();
        }

        var bookings = await _context.Bookings
            .Include(b => b.GymClass)
                .ThenInclude(c => c.Trainer)
            .Where(b => b.MemberId == member.Id)
            .OrderByDescending(b => b.BookedAt)
            .ToListAsync();

        var viewModel = new BookingListViewModel();

        foreach (var booking in bookings)
        {
            var classDateTime = GetNextOccurrence(booking.GymClass.DayOfWeek, booking.GymClass.StartTime);
            var canCancel = booking.Status == "Booked"
                && classDateTime > DateTime.Now.AddHours(CancellationCutoffHours);

            viewModel.Bookings.Add(new BookingRowViewModel
            {
                Booking = booking,
                ClassName = booking.GymClass.Name,
                TrainerName = booking.GymClass.Trainer.FullName,
                DayOfWeek = booking.GymClass.DayOfWeek,
                StartTime = booking.GymClass.StartTime,
                IsCancelled = booking.Status == "Cancelled",
                IsAttended = booking.Status == "Attended",
                ClassDateTime = classDateTime,
                CanCancel = canCancel
            });
        }

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int gymClassId)
    {
        var member = await GetCurrentMemberAsync();

        if (member == null)
        {
            return NotFound();
        }

        var now = DateTime.Now;

        var hasActiveSubscription = await _context.Subscriptions
            .AnyAsync(s => s.MemberId == member.Id
                && s.Status == "Active"
                && s.StartDate <= now
                && s.EndDate >= now);

        if (!hasActiveSubscription)
        {
            TempData["Error"] = "You need an active subscription to book a class.";
            return RedirectToAction("Index", "Classes");
        }

        var gymClass = await _context.GymClasses
            .FirstOrDefaultAsync(c => c.Id == gymClassId);

        if (gymClass == null)
        {
            return NotFound();
        }

        var activeBookingsCount = await _context.Bookings
            .CountAsync(b => b.GymClassId == gymClass.Id && b.Status == "Booked");

        if (activeBookingsCount >= gymClass.Capacity)
        {
            TempData["Error"] = "Sorry, this class has reached its maximum capacity.";
            return RedirectToAction("Index", "Classes");
        }

        var alreadyBooked = await _context.Bookings
            .AnyAsync(b => b.GymClassId == gymClass.Id
                && b.MemberId == member.Id
                && b.Status == "Booked");

        if (alreadyBooked)
        {
            TempData["Error"] = "You are already booked in this class.";
            return RedirectToAction("Index", "Classes");
        }

        using var transaction = await _context.Database.BeginTransactionAsync();

        var booking = new Booking
        {
            MemberId = member.Id,
            GymClassId = gymClass.Id,
            Status = "Booked",
            BookedAt = now
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        var confirmedBookingsCount = await _context.Bookings
            .CountAsync(b => b.GymClassId == gymClass.Id && b.Status == "Booked");

        if (confirmedBookingsCount > gymClass.Capacity)
        {
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
            await transaction.RollbackAsync();

            TempData["Error"] = "Sorry, this class just reached its maximum capacity.";
            return RedirectToAction("Index", "Classes");
        }

        await transaction.CommitAsync();

        TempData["Success"] = "Booking confirmed successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var member = await GetCurrentMemberAsync();

        if (member == null)
        {
            return NotFound();
        }

        var booking = await _context.Bookings
            .Include(b => b.GymClass)
            .FirstOrDefaultAsync(b => b.Id == id && b.MemberId == member.Id);

        if (booking == null)
        {
            return NotFound();
        }

        if (booking.Status == "Cancelled")
        {
            TempData["Error"] = "This booking is already cancelled.";
            return RedirectToAction(nameof(Index));
        }

        if (booking.Status == "Attended")
        {
            TempData["Error"] = "This booking cannot be cancelled because it has already been attended.";
            return RedirectToAction(nameof(Index));
        }

        var classDateTime = GetNextOccurrence(booking.GymClass.DayOfWeek, booking.GymClass.StartTime);

        if (classDateTime <= DateTime.Now.AddHours(CancellationCutoffHours))
        {
            TempData["Error"] = $"Cancellation is not allowed within {CancellationCutoffHours} hours of the class.";
            return RedirectToAction(nameof(Index));
        }

        booking.Status = "Cancelled";
        await _context.SaveChangesAsync();

        TempData["Success"] = "Booking cancelled successfully.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<Member?> GetCurrentMemberAsync()
    {
        var currentUserId = _userManager.GetUserId(User);

        if (currentUserId == null)
        {
            return null;
        }

        return await _context.Members
            .FirstOrDefaultAsync(m => m.ApplicationUserId == currentUserId);
    }

    private static DateTime GetNextOccurrence(string dayOfWeekName, TimeSpan startTime)
    {
        if (!Enum.TryParse<DayOfWeek>(dayOfWeekName, true, out var targetDay))
        {
            return DateTime.MaxValue;
        }

        var now = DateTime.Now;
        var today = now.Date;

        var daysUntilTarget = ((int)targetDay - (int)today.DayOfWeek + 7) % 7;
        var candidateDate = today.AddDays(daysUntilTarget);
        var candidateDateTime = candidateDate.Add(startTime);

        if (daysUntilTarget == 0 && candidateDateTime <= now)
        {
            candidateDateTime = candidateDateTime.AddDays(7);
        }

        return candidateDateTime;
    }
}