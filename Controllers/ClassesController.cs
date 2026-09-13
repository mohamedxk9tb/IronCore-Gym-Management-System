using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymMvc.Data;

namespace GymMvc.Controllers;

public class ClassesController : Controller
{
    private readonly ApplicationDbContext _context;

    public ClassesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var classes = await _context.GymClasses
            .Include(c => c.Trainer)
            .Include(c => c.Bookings)
            .OrderBy(c => c.DayOfWeek)
            .ThenBy(c => c.StartTime)
            .ToListAsync();

        return View(classes);
    }
}