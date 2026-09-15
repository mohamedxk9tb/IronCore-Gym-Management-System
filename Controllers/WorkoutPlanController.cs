using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GymMvc.Data;
using GymMvc.Models;
using GymMvc.Services;
using GymMvc.ViewModels;

namespace GymMvc.Controllers
{
    public class WorkoutPlanController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public WorkoutPlanController(ApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        [Authorize(Roles = "Trainer")]
        public async Task<IActionResult> Create()
        {
            var trainerId = await _currentUser.GetCurrentTrainerIdAsync();
            if (trainerId == null)
            {
                return Forbid();
            }

            var model = new WorkoutDietPlanFormViewModel
            {
                TrainerId = trainerId.Value,
                Members = await GetMembersSelectListAsync()
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Trainer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WorkoutDietPlanFormViewModel model)
        {
            var trainerId = await _currentUser.GetCurrentTrainerIdAsync();
            if (trainerId == null)
            {
                return Forbid();
            }

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
                // TrainerId resolved server-side - never trusted from the form.
                TrainerId = trainerId.Value,
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

            if (User.IsInRole("Member"))
            {
                var memberId = await _currentUser.GetCurrentMemberIdAsync();
                if (memberId == null || plan.MemberId != memberId)
                {
                    return Forbid();
                }
            }

            if (User.IsInRole("Trainer"))
            {
                var trainerId = await _currentUser.GetCurrentTrainerIdAsync();
                if (trainerId == null || plan.TrainerId != trainerId)
                {
                    return Forbid();
                }
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

            return View("Index", model);
        }

        private async Task<System.Collections.Generic.List<SelectListItem>> GetMembersSelectListAsync()
        {
            return await _context.Members
                .Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.FullName })
                .ToListAsync();
        }
    }
}