using GymMvc.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GymMvc.Controllers
{
    public class WorkoutDietPlanController : Controller
    {
        private static readonly List<WorkoutPlanItem> WorkoutPlans = new();

        private static readonly List<DietPlanItem> DietPlans = new();

        public IActionResult Index()
        {
            var model = new WorkoutDietPlanViewModel
            {
                MemberId = 1,
                MemberName = "Ahmed Ali",
                WorkoutPlans = WorkoutPlans,
                DietPlans = DietPlans
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult AddWorkoutPlan(
            int memberId,
            string memberName,
            string planDetails)
        {
            WorkoutPlans.Add(new WorkoutPlanItem
            {
                Id = WorkoutPlans.Count + 1,
                MemberName = memberName,
                PlanDetails = planDetails,
                CreatedAt = DateTime.Now
            });

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult AddDietPlan(
            int memberId,
            string memberName,
            string planDetails)
        {
            DietPlans.Add(new DietPlanItem
            {
                Id = DietPlans.Count + 1,
                MemberName = memberName,
                PlanDetails = planDetails,
                CreatedAt = DateTime.Now
            });

            return RedirectToAction(nameof(Index));
        }
    }
}