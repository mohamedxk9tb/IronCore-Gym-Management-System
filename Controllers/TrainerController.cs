using Microsoft.AspNetCore.Mvc;

namespace GymMvc.Controllers;

public class TrainerController : Controller
{
    public IActionResult Dashboard() => View();
    public IActionResult ClassRoster() => View();
    public IActionResult Attendance() => View();
}