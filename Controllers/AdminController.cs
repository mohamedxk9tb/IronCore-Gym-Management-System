using Microsoft.AspNetCore.Mvc;

namespace GymMvc.Controllers;

public class AdminController : Controller
{
    public IActionResult Dashboard() => View();
    public IActionResult Plans() => View("Plans/Index");
    public IActionResult PlansCreate() => View("Plans/Create");
    public IActionResult PlansEdit(int id) => View("Plans/Edit");
    public IActionResult PlansDetails(int id) => View("Plans/Details");
    public IActionResult PlansDelete(int id) => View("Plans/Delete");
    public IActionResult Classes() => View("Classes/Index");
    public IActionResult ClassesCreate() => View("Classes/Create");
    public IActionResult ClassesEdit(int id) => View("Classes/Edit");
    public IActionResult ClassesDetails(int id) => View("Classes/Details");
    public IActionResult ClassesDelete(int id) => View("Classes/Delete");
    public IActionResult Trainers() => View("Trainers/Index");
    public IActionResult TrainersCreate() => View("Trainers/Create");
    public IActionResult TrainersEdit(int id) => View("Trainers/Edit");
    public IActionResult TrainersDetails(int id) => View("Trainers/Details");
    public IActionResult TrainersDelete(int id) => View("Trainers/Delete");
    public IActionResult Members() => View("Members/Index");
    public IActionResult MembersCreate() => View("Members/Create");
    public IActionResult MembersEdit(int id) => View("Members/Edit");
    public IActionResult MembersDetails(int id) => View("Members/Details");
    public IActionResult MembersDelete(int id) => View("Members/Delete");
    public IActionResult Subscriptions() => View("Subscriptions/Index");
    public IActionResult Payments() => View("Payments/Index");
    public IActionResult Reports() => View("Reports/Index");
}