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

        var currentUserId = _userManager.GetUserId(User);
        int? memberId = null;

        if (currentUserId != null)
        {
            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.ApplicationUserId == currentUserId);

            if (member != null)
            {
                memberId = member.Id;
            }
        }

        var viewModel = new PlanListViewModel();

        foreach (var plan in plans)
        {
            var isSubscribed = false;

            if (memberId != null)
            {
                isSubscribed = await _context.Subscriptions
                    .AnyAsync(s => s.PlanId == plan.Id
                        && s.MemberId == memberId
                        && s.Status == "Active");
            }

            var activeCount = await _context.Subscriptions
                .CountAsync(s => s.PlanId == plan.Id && s.Status == "Active");

            viewModel.Plans.Add(new PlanCardViewModel
            {
                Plan = plan,
                IsSubscribed = isSubscribed,
                ActiveSubscribersCount = activeCount
            });
        }

        return View(viewModel);
    }
}