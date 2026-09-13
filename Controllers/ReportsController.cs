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

            var payments = await _context.Payments.ToListAsync();
            var subscriptions = await _context.Subscriptions
                .Include(s => s.Member)
                .Include(s => s.Plan)
                .ToListAsync();

            var months = Enumerable.Range(0, MonthsToShow)
                .Select(i => now.AddMonths(-(MonthsToShow - 1 - i)))
                .ToList();

            var model = new ReportsViewModel
            {
                Months = months.Select(m => m.ToString("MMM yyyy")).ToList(),

                IncomeData = months
                    .Select(m => payments
                        .Where(p => p.PaymentDate.Year == m.Year && p.PaymentDate.Month == m.Month)
                        .Sum(p => p.Amount))
                    .ToList(),

                MembersData = months
                    .Select(m => subscriptions
                        .Where(s => s.StartDate <= m && s.EndDate >= m && s.Status == "Active")
                        .Select(s => s.MemberId)
                        .Distinct()
                        .Count())
                    .ToList(),

                MonthlyIncome = payments
                    .Where(p => p.PaymentDate.Year == now.Year && p.PaymentDate.Month == now.Month)
                    .Sum(p => p.Amount),

                // Distinct members, not subscription rows - a member can have more than one.
                ActiveMembers = subscriptions
                    .Where(s => s.Status == "Active")
                    .Select(s => s.MemberId)
                    .Distinct()
                    .Count(),

                ExpiringMembershipsThisWeek = subscriptions
                    .Where(s => s.Status == "Active" && s.EndDate >= now && s.EndDate <= now.AddDays(7))
                    .Select(s => new ExpiringMembershipViewModel
                    {
                        MemberName = s.Member.FullName,
                        PlanName = s.Plan.Name,
                        EndDate = s.EndDate
                    })
                    .OrderBy(e => e.EndDate)
                    .ToList()
            };

            model.MostPopularClasses = await _context.GymClasses
                .Select(c => new PopularClassViewModel
                {
                    ClassName = c.Name,
                    // Cancelled bookings don't count as popularity.
                    BookingsCount = c.Bookings.Count(b => b.Status != "Cancelled")
                })
                .OrderByDescending(c => c.BookingsCount)
                .Take(TopClassesCount)
                .ToListAsync();

            return View(model);
        }
    }
}