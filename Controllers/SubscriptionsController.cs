using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using GymMvc.Data;
using GymMvc.Models;
using GymMvc.ViewModels;

namespace GymMvc.Controllers;

[Authorize(Roles = "Member")]
public class SubscriptionsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public SubscriptionsController(
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

        var subscriptions = await _context.Subscriptions
            .Include(s => s.Plan)
            .Where(s => s.MemberId == member.Id)
            .OrderByDescending(s => s.StartDate)
            .ToListAsync();

        var viewModel = new SubscriptionListViewModel();

        foreach (var subscription in subscriptions)
        {
            var daysRemaining = (subscription.EndDate - DateTime.Now).Days;

            viewModel.Subscriptions.Add(new SubscriptionRowViewModel
            {
                Subscription = subscription,
                PlanName = subscription.Plan.Name,
                PlanPrice = subscription.Plan.Price,
                DaysRemaining = daysRemaining > 0 ? daysRemaining : 0,
                IsExpired = subscription.EndDate < DateTime.Now
            });
        }

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int planId)
    {
        var plan = await _context.Plans.FindAsync(planId);

        if (plan == null)
        {
            return NotFound();
        }

        var viewModel = new SubscriptionCreateViewModel
        {
            Plan = plan
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int planId)
    {
        var member = await GetCurrentMemberAsync();

        if (member == null)
        {
            return NotFound();
        }

        var plan = await _context.Plans.FindAsync(planId);

        if (plan == null)
        {
            return NotFound();
        }

        var now = DateTime.Now;

        var hasActiveSubscription = await _context.Subscriptions
            .AnyAsync(s => s.MemberId == member.Id
                && s.Status == "Active"
                && s.StartDate <= now
                && s.EndDate >= now);

        if (hasActiveSubscription)
        {
            TempData["Error"] = "You already have an active subscription.";
            return RedirectToAction(nameof(Index));
        }

        var subscription = new Subscription
        {
            MemberId = member.Id,
            PlanId = plan.Id,
            StartDate = now,
            EndDate = now.AddMonths(plan.DurationMonths),
            Status = "Active"
        };

        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Subscription created successfully.";
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
}