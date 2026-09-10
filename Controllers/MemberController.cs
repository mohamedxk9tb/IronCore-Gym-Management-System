using Microsoft.AspNetCore.Mvc;

namespace GymMvc.Controllers;

public class MemberController : Controller
{
    public IActionResult Dashboard() => View();
    public IActionResult Classes() => View();
    public IActionResult Bookings() => View();
    public IActionResult FitnessPlan() => View();
    public IActionResult Progress() => View();
    public IActionResult CheckIn() => View();
    public IActionResult Profile() => View();
    public IActionResult Assistant() => View();
}
