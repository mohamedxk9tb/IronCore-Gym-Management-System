using Microsoft.AspNetCore.Mvc;
using GymMvc.Models;

namespace GymMvc.Controllers;

public class TrainersController : Controller
{
    public IActionResult Index() => View(TrainerProfiles.All);

    public IActionResult Details(int id)
    {
        var trainer = TrainerProfiles.All.FirstOrDefault(trainer => trainer.Id == id);

        return trainer is null ? NotFound() : View(trainer);
    }
}
