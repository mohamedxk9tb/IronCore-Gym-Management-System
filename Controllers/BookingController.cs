using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using GymMvc.Data;
using GymMvc.Models;

namespace GymMvc.Controllers;

[Authorize]
public class BookingController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public BookingController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var currentUserId = _userManager.GetUserId(User);

        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.ApplicationUserId == currentUserId);

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

        return View(bookings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int gymClassId)
    {
        var currentUserId = _userManager.GetUserId(User);

        var member = await _context.Members
           .FirstOrDefaultAsync(m => m.ApplicationUserId == currentUserId);

        if (member == null)
        {
            return NotFound();
        }

        var gymClass = await _context.GymClasses
            .Include(c => c.Bookings)
            .FirstOrDefaultAsync(c => c.Id == gymClassId);

        if (gymClass == null)
        {
            return NotFound();
        }

        var activeBookingsCount = gymClass.Bookings
            .Count(b => b.Status == "Booked");

        if (activeBookingsCount >= gymClass.Capacity)
        {
            TempData["Error"] = "Sorry, this class has reached its maximum capacity.";
            return RedirectToAction("Index", "Classes");
        }

        var alreadyBooked = gymClass.Bookings
            .Any(b => b.MemberId == member.Id && b.Status == "Booked");

        if (alreadyBooked)
        {
            TempData["Error"] = "You are already booked in this class.";
            return RedirectToAction("Index", "Classes");
        }

        var booking = new Booking
        {
            MemberId = member.Id,
            GymClassId = gymClass.Id,
            Status = "Booked",
            BookedAt = DateTime.Now
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Booking confirmed successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var currentUserId = _userManager.GetUserId(User);

        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.ApplicationUserId == currentUserId);

        if (member == null)
        {
            return NotFound();
        }

        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == id && b.MemberId == member.Id);

        if (booking == null)
        {
            return NotFound();
        }

        booking.Status = "Cancelled";
        await _context.SaveChangesAsync();

        TempData["Success"] = "Booking cancelled successfully.";
        return RedirectToAction(nameof(Index));
    }
}