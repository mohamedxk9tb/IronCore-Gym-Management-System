using GymMvc.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GymMvc.Controllers
{
    public class ReportsController : Controller
    {
        public IActionResult Index()
        {
            var model = new ReportsViewModel
            {
                MonthlyIncome = 25000,

                ActiveMembers = 120,

                Months = new List<string>
                {
                    "January",
                    "February",
                    "March",
                    "April",
                    "May",
                    "June"
                },

                IncomeData = new List<decimal>
                {
                    18000,
                    21000,
                    19500,
                    23000,
                    25000,
                    27000
                },

                MembersData = new List<int>
                {
                    85,
                    92,
                    98,
                    105,
                    112,
                    120
                }
            };

            return View(model);
        }
    }
}