using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymMvc.Data;
using GymMvc.Models;
using GymMvc.ViewModels;

namespace GymMvc.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminTrainersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminTrainersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /AdminTrainers
        public async Task<IActionResult> Index()
        {
            var trainers = await _context.Trainers.ToListAsync();
            return View(trainers);
        }

        // GET: /AdminTrainers/Create
        public IActionResult Create()
        {
            return View(new TrainerFormViewModel());
        }

        // POST: /AdminTrainers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TrainerFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var trainer = new Trainer
            {
                FullName = model.FullName,
                Specialty = model.Specialty,
                Certificates = model.Certificates
            };

            _context.Trainers.Add(trainer);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /AdminTrainers/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var trainer = await _context.Trainers.FindAsync(id);
            if (trainer == null)
            {
                return NotFound();
            }

            var model = new TrainerFormViewModel
            {
                Id = trainer.Id,
                FullName = trainer.FullName,
                Specialty = trainer.Specialty,
                Certificates = trainer.Certificates
            };

            return View(model);
        }

        // POST: /AdminTrainers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TrainerFormViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var trainer = await _context.Trainers.FindAsync(id);
            if (trainer == null)
            {
                return NotFound();
            }

            trainer.FullName = model.FullName;
            trainer.Specialty = model.Specialty;
            trainer.Certificates = model.Certificates;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /AdminTrainers/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var trainer = await _context.Trainers.FindAsync(id);
            if (trainer == null)
            {
                return NotFound();
            }

            return View(trainer);
        }

        // POST: /AdminTrainers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trainer = await _context.Trainers.FindAsync(id);
            if (trainer == null)
            {
                return NotFound();
            }

            _context.Trainers.Remove(trainer);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}