using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using GymMvc.Data;
using GymMvc.Models;

namespace GymMvc.Controllers;

[Authorize]
public class SubscriptionsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public SubscriptionsController(ApplicationDbContext context,UserManager<ApplicationUser> userManager)
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

        var subscriptions = await _context.Subscriptions
            .Include(s => s.Plan)
            .Where(s => s.MemberId == member.Id)
            .OrderByDescending(s => s.StartDate)
            .ToListAsync();

        return View(subscriptions);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int planId)
    {
        var plan = await _context.Plans.FindAsync(planId);

        if (plan == null)
        {
            return NotFound();
        }

        return View(plan);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int planId, string status)
    {
        var currentUserId = _userManager.GetUserId(User);

        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.ApplicationUserId == currentUserId);

        if (member == null)
        {
            return NotFound();
        }

        var plan = await _context.Plans.FindAsync(planId);

        if (plan == null)
        {
            return NotFound();
        }

        var subscription = new Subscription
        {
            MemberId = member.Id,
            PlanId = plan.Id,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddMonths(plan.DurationMonths),
            Status = "Active"
        };

        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}