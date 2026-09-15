using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymMvc.Data;
using GymMvc.ViewModels;

namespace GymMvc.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int MonthsToShow = 6;
        private const int TopClassesCount = 5;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;
            var rangeStart = now.AddMonths(-(MonthsToShow - 1)).Date;

            var months = Enumerable.Range(0, MonthsToShow)
                .Select(i => now.AddMonths(-(MonthsToShow - 1 - i)))
                .ToList();

            var paymentsInRange = await _context.Payments
                .Where(p => p.PaymentDate >= rangeStart)
                .Select(p => new { p.Amount, p.PaymentDate })
                .ToListAsync();

            var subscriptionsInRange = await _context.Subscriptions
                .Where(s => s.EndDate >= rangeStart)
                .Select(s => new { s.MemberId, s.StartDate, s.EndDate, s.Status })
                .ToListAsync();

            var model = new ReportsViewModel
            {
                Months = months.Select(m => m.ToString("MMM yyyy")).ToList(),

                IncomeData = months
                    .Select(m => paymentsInRange
                        .Where(p => p.PaymentDate.Year == m.Year && p.PaymentDate.Month == m.Month)
                        .Sum(p => p.Amount))
                    .ToList(),

                MembersData = months
                    .Select(m => subscriptionsInRange
                        .Where(s => s.StartDate <= m && s.EndDate >= m && s.Status == "Active")
                        .Select(s => s.MemberId)
                        .Distinct()
                        .Count())
                    .ToList(),

                MonthlyIncome = await _context.Payments
                    .Where(p => p.PaymentDate.Year == now.Year && p.PaymentDate.Month == now.Month)
                    .SumAsync(p => p.Amount),

                // "Active member" = Status == "Active" AND StartDate <= now AND EndDate >= now.
                // Previous version was missing the StartDate check.
                ActiveMembers = await _context.Subscriptions
                    .Where(s => s.Status == "Active" && s.StartDate <= now && s.EndDate >= now)
                    .Select(s => s.MemberId)
                    .Distinct()
                    .CountAsync(),

                ExpiringMembershipsThisWeek = await _context.Subscriptions
                    .Include(s => s.Member)
                    .Include(s => s.Plan)
                    .Where(s => s.Status == "Active" && s.EndDate >= now && s.EndDate <= now.AddDays(7))
                    .OrderBy(s => s.EndDate)
                    .Select(s => new ExpiringMembershipViewModel
                    {
                        MemberName = s.Member.FullName,
                        PlanName = s.Plan.Name,
                        EndDate = s.EndDate
                    })
                    .ToListAsync(),

                MostPopularClasses = await _context.GymClasses
                    .Select(c => new PopularClassViewModel
                    {
                        ClassName = c.Name,
                        BookingsCount = c.Bookings.Count(b => b.Status != "Cancelled")
                    })
                    .OrderByDescending(c => c.BookingsCount)
                    .Take(TopClassesCount)
                    .ToListAsync()
            };

            return View(model);
        }
    }
}