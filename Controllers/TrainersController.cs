using GymMvc.Models;
using Microsoft.AspNetCore.Mvc;

namespace GymMvc.Controllers
{
    public class TrainersController : Controller
    {
        private static readonly List<Trainer> Trainers = new()
        {
            new Trainer
            {
                Id = 1,
                FullName = "Ahmed Ali",
                Specialty = "Strength Training",
                
            },

            new Trainer
            {
                Id = 2,
                FullName = "Mohamed Hassan",
                Specialty = "Fitness & Cardio",
                
            },

            new Trainer
            {
                Id = 3,
                FullName = "Omar Khaled",
                Specialty = "Bodybuilding",
                
            }
        };

        public IActionResult Index()
        {
            return View(Trainers);
        }

        public IActionResult Details(int id)
        {
            var trainer = Trainers.FirstOrDefault(x => x.Id == id);

            if (trainer == null)
                return NotFound();

            return View(trainer);
        }
    }
}