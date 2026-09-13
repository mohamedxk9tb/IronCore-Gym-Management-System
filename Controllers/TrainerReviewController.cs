using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymMvc.Data;
using GymMvc.Models;
using GymMvc.ViewModels;

namespace GymMvc.Controllers
{
    // ⚠️ ده الاسم الرسمي (جمع). لو عندك TrainerReviewController.cs بالمفرد، امسحه.
    [Authorize(Roles = "Member")]
    [Route("TrainerReviews")]
    public class TrainerReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrainerReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /TrainerReviews/Create/5  (5 = TrainerId)
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

        // POST: /TrainerReviews/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TrainerReviewFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // MemberId من اليوزر المسجل دخوله - مش من الفورم
            var memberId = GetCurrentMemberId();

            var alreadyReviewed = await _context.TrainerReviews
                .AnyAsync(r => r.TrainerId == model.TrainerId && r.MemberId == memberId);

            if (alreadyReviewed)
            {
                ModelState.AddModelError(string.Empty, "إنت قيّمت المدرب ده قبل كده");
                model.TrainerName = (await _context.Trainers.FindAsync(model.TrainerId))?.FullName ?? model.TrainerName;
                return View(model);
            }

            var review = new TrainerReview
            {
                TrainerId = model.TrainerId,
                MemberId = memberId,
                Rating = model.Rating,
                Comment = model.Comment
            };

            _context.TrainerReviews.Add(review);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Trainers", new { id = model.TrainerId });
        }

        // ⚠️ placeholder - نفس نقطة التنسيق المطلوبة مع محمد
        private int GetCurrentMemberId()
        {
            var claim = User.FindFirst("MemberId")?.Value;
            return int.TryParse(claim, out var memberId) ? memberId : 0;
        }
    }
}