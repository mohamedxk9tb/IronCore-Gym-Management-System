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
    // ⚠️ الصفحة دي بتعتمد على Models بتاعة مروان (Payment, Subscription, Booking, GymClass)
    // القيم المفترضة (Status == "Active"/"Attended") لازم تتأكد منها معاه.
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

        // GET: /Reports
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

                // ⚠️ افتراض: "نشط في شهر معين" = فيه اشتراك حالته Active وبيغطي الشهر ده
                MembersData = months
                    .Select(m => subscriptions
                        .Count(s => s.StartDate <= m && s.EndDate >= m && s.Status == "Active"))
                    .ToList(),

                MonthlyIncome = payments
                    .Where(p => p.PaymentDate.Year == now.Year && p.PaymentDate.Month == now.Month)
                    .Sum(p => p.Amount),

                ActiveMembers = subscriptions.Count(s => s.Status == "Active"),

                // الاشتراكات اللي هتنتهي خلال 7 أيام قادمة ولسه Active
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

            // أكتر الكلاسات حجزًا (بغض النظر عن حالة الحجز)
            model.MostPopularClasses = await _context.GymClasses
                .Select(c => new PopularClassViewModel
                {
                    ClassName = c.Name,
                    BookingsCount = c.Bookings.Count
                })
                .OrderByDescending(c => c.BookingsCount)
                .Take(TopClassesCount)
                .ToListAsync();

            return View(model);
        }
    }
}