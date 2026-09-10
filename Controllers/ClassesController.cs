using Microsoft.AspNetCore.Mvc;

namespace GymMvc.Controllers;

public class ClassesController : Controller
{
    public IActionResult Index() => View();
}