using GymMvc.Models;
using Microsoft.AspNetCore.Mvc;

namespace GymMvc.Controllers
{
    public class AdminTrainersController : Controller
    {
        private static readonly List<Trainer> Trainers = new()
        {
            new Trainer
            {
                Id = 1,
                FullName = "Ahmed Hassan",
                Specialty = "Strength Training"
            },
            new Trainer
            {
                Id = 2,
                FullName = "Omar Ali",
                Specialty = "Bodybuilding"
            },
            new Trainer
            {
                Id = 3,
                FullName = "Youssef Mohamed",
                Specialty = "Fitness & Cardio"
            }
        };

        public IActionResult Index()
        {
            return View(Trainers);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Trainer trainer)
        {
            if (!ModelState.IsValid)
                return View(trainer);

            trainer.Id = Trainers.Count == 0
                ? 1
                : Trainers.Max(t => t.Id) + 1;

            Trainers.Add(trainer);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var trainer = Trainers.FirstOrDefault(t => t.Id == id);

            if (trainer == null)
                return NotFound();

            return View(trainer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Trainer trainer)
        {
            if (id != trainer.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(trainer);

            var existingTrainer =
                Trainers.FirstOrDefault(t => t.Id == id);

            if (existingTrainer == null)
                return NotFound();

            existingTrainer.FullName = trainer.FullName;
            existingTrainer.Specialty = trainer.Specialty;

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            var trainer = Trainers.FirstOrDefault(t => t.Id == id);

            if (trainer == null)
                return NotFound();

            return View(trainer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var trainer = Trainers.FirstOrDefault(t => t.Id == id);

            if (trainer != null)
                Trainers.Remove(trainer);

            return RedirectToAction(nameof(Index));
        }
    }
}