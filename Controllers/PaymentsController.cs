using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using GymMvc.Data;
using GymMvc.Models;

namespace GymMvc.Controllers;

[Authorize]
public class PaymentsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public PaymentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
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

        var payments = await _context.Payments
            .Include(p => p.Subscription)
                .ThenInclude(s => s.Plan)
            .Where(p => p.Subscription.MemberId == member.Id)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();

        return View(payments);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int subscriptionId)
    {
        var subscription = await _context.Subscriptions
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.Id == subscriptionId);

        if (subscription == null)
        {
            return NotFound();
        }

        return View(subscription);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int subscriptionId, decimal amount)
    {
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == subscriptionId);

        if (subscription == null)
        {
            return NotFound();
        }

        var payment = new Payment
        {
            SubscriptionId = subscription.Id,
            Amount = amount,
            PaymentDate = DateTime.Now
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}