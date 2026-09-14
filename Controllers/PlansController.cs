using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GymMvc.Data;
using GymMvc.ViewModels;

namespace GymMvc.Controllers;

public class PlansController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public PlansController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var plans = await _context.Plans
            .OrderBy(p => p.Price)
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

        var activeSubscriptions = await _context.Subscriptions
            .Where(s => s.Status == "Active")
            .ToListAsync();

        var activeCountByPlan = activeSubscriptions
            .GroupBy(s => s.PlanId)
            .ToDictionary(g => g.Key, g => g.Count());

        var subscribedPlanIds = memberId != null
            ? activeSubscriptions
                .Where(s => s.MemberId == memberId)
                .Select(s => s.PlanId)
                .ToHashSet()
            : new HashSet<int>();

        var viewModel = new PlanListViewModel();

        foreach (var plan in plans)
        {
            viewModel.Plans.Add(new PlanCardViewModel
            {
                Plan = plan,
                IsSubscribed = subscribedPlanIds.Contains(plan.Id),
                ActiveSubscribersCount = activeCountByPlan.TryGetValue(plan.Id, out var count) ? count : 0
            });
        }

        return View(viewModel);
    }
}