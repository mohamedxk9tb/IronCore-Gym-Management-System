using Microsoft.AspNetCore.Mvc;

namespace GymMvc.Controllers;

public class PlansController : Controller
{
    public IActionResult Index() => View();
}