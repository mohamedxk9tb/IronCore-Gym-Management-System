using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymMvc.Data;
using GymMvc.ViewModels;

namespace GymMvc.Controllers
{
    // صفحة عامة: قايمة المدربين + بروفايل كل مدرب. متاحة لأي زائر.
    public class TrainersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrainersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Trainers
        public async Task<IActionResult> Index()
        {
            var trainers = await _context.Trainers
                .Select(t => new TrainerListItemViewModel
                {
                    Id = t.Id,
                    FullName = t.FullName,
                    Specialty = t.Specialty,
                    ReviewsCount = t.Reviews.Count,
                    AverageRating = t.Reviews.Any() ? t.Reviews.Average(r => r.Rating) : 0
                })
                .ToListAsync();

            return View(trainers);
        }

        // GET: /Trainers/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var trainer = await _context.Trainers
                .Include(t => t.Reviews)
                    .ThenInclude(r => r.Member)
                .Include(t => t.GymClasses)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trainer == null)
            {
                return NotFound();
            }

            var model = new TrainerProfileViewModel
            {
                Id = trainer.Id,
                FullName = trainer.FullName,
                Specialty = trainer.Specialty,
                Certificates = trainer.Certificates,
                AverageRating = trainer.Reviews.Any() ? trainer.Reviews.Average(r => r.Rating) : 0,
                ClassNames = trainer.GymClasses.Select(c => c.Name).ToList(),
                Reviews = trainer.Reviews.Select(r => new TrainerReviewItemViewModel
                {
                    MemberName = r.Member.FullName,
                    Rating = r.Rating,
                    Comment = r.Comment
                }).ToList()
            };

            return View(model);
        }
    }
}