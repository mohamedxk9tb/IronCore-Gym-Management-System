using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymMvc.Data;
using GymMvc.Models;
using GymMvc.Services;
using GymMvc.ViewModels;

namespace GymMvc.Controllers
{
    [Authorize(Roles = "Member")]
    [Route("TrainerReviews")]
    public class TrainerReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public TrainerReviewsController(ApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        [HttpGet("Create/{trainerId:int}")]
        public async Task<IActionResult> Create(int trainerId)
        {
            var trainer = await _context.Trainers.FindAsync(trainerId);
            if (trainer == null)
            {
                return NotFound();
            }

            var model = new TrainerReviewFormViewModel
            {
                TrainerId = trainer.Id,
                TrainerName = trainer.FullName
            };

            return View(model);
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TrainerReviewFormViewModel model)
        {
            var trainer = await _context.Trainers.FindAsync(model.TrainerId);
            if (trainer == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                model.TrainerName = trainer.FullName;
                return View(model);
            }

            var memberId = await _currentUser.GetCurrentMemberIdAsync();
            if (memberId == null)
            {
                return Forbid();
            }

            var alreadyReviewed = await _context.TrainerReviews
                .AnyAsync(r => r.TrainerId == model.TrainerId && r.MemberId == memberId);

            if (alreadyReviewed)
            {
                ModelState.AddModelError(string.Empty, "إنت قيّمت المدرب ده قبل كده");
                model.TrainerName = trainer.FullName;
                return View(model);
            }

            var review = new TrainerReview
            {
                TrainerId = model.TrainerId,
                MemberId = memberId.Value,
                Rating = model.Rating,
                Comment = model.Comment
            };

            _context.TrainerReviews.Add(review);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Re-check whether this was actually a duplicate (race condition against
                // the unique index) instead of assuming that's always the cause.
                var isDuplicateNow = await _context.TrainerReviews
                    .AnyAsync(r => r.TrainerId == model.TrainerId && r.MemberId == memberId);

                model.TrainerName = trainer.FullName;
                ModelState.AddModelError(string.Empty, isDuplicateNow
                    ? "إنت قيّمت المدرب ده قبل كده"
                    : "معلش، حصلت مشكلة أثناء حفظ التقييم، حاول تاني.");

                return View(model);
            }

            return RedirectToAction("Details", "Trainers", new { id = model.TrainerId });
        }
    }
}