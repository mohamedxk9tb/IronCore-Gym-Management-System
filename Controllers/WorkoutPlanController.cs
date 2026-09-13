using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GymMvc.Data;
using GymMvc.Models;
using GymMvc.ViewModels;

namespace GymMvc.Controllers
{
    // صفحة Workout & Diet Plan: المدرب يكتب خطة تمرين/غذاء لعضو، والعضو يشوفها
    public class WorkoutPlanController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WorkoutPlanController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /WorkoutPlan/Create
        [Authorize(Roles = "Trainer")]
        public async Task<IActionResult> Create()
        {
            var model = new WorkoutDietPlanFormViewModel
            {
                TrainerId = GetCurrentTrainerId(),
                Members = await GetMembersSelectListAsync()
            };

            return View(model);
        }

        // POST: /WorkoutPlan/Create
        [HttpPost]
        [Authorize(Roles = "Trainer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WorkoutDietPlanFormViewModel model)
        {
            // التحقق إن العضو المختار فعلاً موجود - منمنعش الـ ID من غير تحقق
            var memberExists = await _context.Members.AnyAsync(m => m.Id == model.MemberId);
            if (!memberExists)
            {
                ModelState.AddModelError(nameof(model.MemberId), "العضو المختار غير موجود");
            }

            if (!ModelState.IsValid)
            {
                model.Members = await GetMembersSelectListAsync();
                return View(model);
            }

            var plan = new WorkoutPlan
            {
                // TrainerId من اليوزر المسجل دخوله، مش من الفورم
                TrainerId = GetCurrentTrainerId(),
                MemberId = model.MemberId,
                Exercises = model.Exercises.Select(e => new WorkoutExercise
                {
                    ExerciseName = e.ExerciseName,
                    Sets = e.Sets,
                    Reps = e.Reps
                }).ToList(),
                DietMeals = model.DietMeals.Select(d => new DietMeal
                {
                    DayOfWeek = d.DayOfWeek,
                    MealName = d.MealName,
                    Description = d.Description
                }).ToList()
            };

            _context.WorkoutPlans.Add(plan);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = plan.Id });
        }

        // GET: /WorkoutPlan/Details/5
        [Authorize(Roles = "Member,Trainer")]
        public async Task<IActionResult> Details(int id)
        {
            var plan = await _context.WorkoutPlans
                .Include(p => p.Trainer)
                .Include(p => p.Exercises)
                .Include(p => p.DietMeals)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (plan == null)
            {
                return NotFound();
            }

            // ownership check: عضو مينفعش يشوف خطة عضو تاني
            if (User.IsInRole("Member") && plan.MemberId != GetCurrentMemberId())
            {
                return Forbid();
            }

            var model = new WorkoutDietPlanDetailsViewModel
            {
                Id = plan.Id,
                TrainerName = plan.Trainer.FullName,
                CreatedDate = plan.CreatedDate,
                Exercises = plan.Exercises.Select(e => new WorkoutExerciseInputViewModel
                {
                    ExerciseName = e.ExerciseName,
                    Sets = e.Sets,
                    Reps = e.Reps
                }).ToList(),
                DietMeals = plan.DietMeals.Select(d => new DietMealInputViewModel
                {
                    DayOfWeek = d.DayOfWeek,
                    MealName = d.MealName,
                    Description = d.Description
                }).ToList()
            };

            return View(model);
        }

        private async Task<System.Collections.Generic.List<SelectListItem>> GetMembersSelectListAsync()
        {
            return await _context.Members
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.FullName
                })
                .ToListAsync();
        }

        // ⚠️ placeholders - نفس نقطة التنسيق المطلوبة مع محمد بخصوص ربط اليوزر بـ Trainer/Member
        private int GetCurrentTrainerId()
        {
            var claim = User.FindFirst("TrainerId")?.Value;
            return int.TryParse(claim, out var trainerId) ? trainerId : 0;
        }

        private int GetCurrentMemberId()
        {
            var claim = User.FindFirst("MemberId")?.Value;
            return int.TryParse(claim, out var memberId) ? memberId : 0;
        }
    }
}