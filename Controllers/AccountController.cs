using Microsoft.AspNetCore.Mvc;

namespace GymMvc.Controllers;

public class AccountController : Controller
{
    public IActionResult Login() => View();
    public IActionResult SignUp() => View();
}