using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymMvc.Data;

namespace GymMvc.Controllers;

public class PlansController : Controller
{
    private readonly ApplicationDbContext _context;

    public PlansController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var plans = await _context.Plans
            .OrderBy(p => p.Price)
            .ToListAsync();

        return View(plans);
    }
}