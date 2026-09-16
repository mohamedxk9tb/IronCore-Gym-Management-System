using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using GymMvc.Data;
using GymMvc.Models;
using GymMvc.ViewModels;

namespace GymMvc.Controllers;

[Authorize(Roles = "Member")]
public class PaymentsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public PaymentsController(
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

        var payments = await _context.Payments
            .Include(p => p.Subscription)
                .ThenInclude(s => s.Plan)
            .Where(p => p.Subscription.MemberId == member.Id)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();

        var viewModel = new PaymentListViewModel();

        foreach (var payment in payments)
        {
            viewModel.Payments.Add(new PaymentRowViewModel
            {
                Payment = payment,
                PlanName = payment.Subscription.Plan.Name
            });
        }

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int subscriptionId)
    {
        var member = await GetCurrentMemberAsync();

        if (member == null)
        {
            return NotFound();
        }

        var subscription = await _context.Subscriptions
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.Id == subscriptionId);

        if (subscription == null)
        {
            return NotFound();
        }

        if (subscription.MemberId != member.Id)
        {
            return NotFound();
        }

        var viewModel = new PaymentCreateViewModel
        {
            Subscription = subscription,
            PlanName = subscription.Plan.Name,
            PlanPrice = subscription.Plan.Price
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePayment(int subscriptionId)
    {
        var member = await GetCurrentMemberAsync();

        if (member == null)
        {
            return NotFound();
        }

        var subscription = await _context.Subscriptions
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.Id == subscriptionId);

        if (subscription == null)
        {
            return NotFound();
        }

        if (subscription.MemberId != member.Id)
        {
            return NotFound();
        }

        var payment = new Payment
        {
            SubscriptionId = subscription.Id,
            Amount = subscription.Plan.Price,
            PaymentDate = DateTime.Now
        };

        subscription.Status = "Active";

        _context.Payments.Add(payment);

        await _context.SaveChangesAsync();

        TempData["Success"] = "Payment completed successfully.";

        return RedirectToAction("Dashboard", "Member");
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