using GymMvc.Data;
using GymMvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymMvc.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Dashboard
    [HttpGet]
    public IActionResult Dashboard()
    {
        return View();
    }

    // Plans
    [HttpGet]
    public IActionResult Plans()
    {
        return View("Plans/Index");
    }

    [HttpGet]
    public IActionResult PlansCreate()
    {
        return View("Plans/Create");
    }

    [HttpGet]
    public IActionResult PlansEdit(int id)
    {
        return View("Plans/Edit");
    }

    [HttpGet]
    public IActionResult PlansDetails(int id)
    {
        return View("Plans/Details");
    }

    [HttpGet]
    public IActionResult PlansDelete(int id)
    {
        return View("Plans/Delete");
    }

    // Classes
    [HttpGet]
    public IActionResult Classes()
    {
        return View("Classes/Index");
    }

    [HttpGet]
    public IActionResult ClassesCreate()
    {
        return View("Classes/Create");
    }

    [HttpGet]
    public IActionResult ClassesEdit(int id)
    {
        return View("Classes/Edit");
    }

    [HttpGet]
    public IActionResult ClassesDetails(int id)
    {
        return View("Classes/Details");
    }

    [HttpGet]
    public IActionResult ClassesDelete(int id)
    {
        return View("Classes/Delete");
    }

    // Trainers
    [HttpGet]
    public IActionResult Trainers()
    {
        return View("Trainers/Index");
    }

    [HttpGet]
    public IActionResult TrainersCreate()
    {
        return View("Trainers/Create");
    }

    [HttpGet]
    public IActionResult TrainersEdit(int id)
    {
        return View("Trainers/Edit");
    }

    [HttpGet]
    public IActionResult TrainersDetails(int id)
    {
        return View("Trainers/Details");
    }

    [HttpGet]
    public IActionResult TrainersDelete(int id)
    {
        return View("Trainers/Delete");
    }

    // Members
    [HttpGet]
    public async Task<IActionResult> Members(
        string? search,
        int page = 1)
    {
        const int pageSize = 10;

        if (page < 1)
        {
            page = 1;
        }

        var query = _context.Members
            .AsNoTracking()
            .OrderByDescending(member => member.Id)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(member =>
                member.FullName.Contains(search) ||
                member.Email.Contains(search));
        }

        var totalMembers = await query.CountAsync();

        var totalPages =
            (int)Math.Ceiling(totalMembers / (double)pageSize);

        if (totalPages > 0 && page > totalPages)
        {
            page = totalPages;
        }

        var members = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Search = search;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalMembers = totalMembers;

        return View("Members/Index", members);
    }

    [HttpGet]
    public async Task<IActionResult> MembersDetails(int id)
    {
        var member = await _context.Members
            .AsNoTracking()
            .Include(member => member.WeightLogs)
            .Include(member => member.CheckIns)
            .FirstOrDefaultAsync(member => member.Id == id);

        if (member is null)
        {
            return NotFound();
        }

        return View("Members/Details", member);
    }

    [HttpGet]
    public async Task<IActionResult> MembersCreate()
    {
        return View("Members/Create");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MembersCreate(Member model)
    {
        if (!ModelState.IsValid)
        {
            return View("Members/Create", model);
        }

        var existingMember = await _context.Members
            .AnyAsync(member => member.Email == model.Email);

        if (existingMember)
        {
            ModelState.AddModelError(
                nameof(model.Email),
                "A member with this email already exists.");

            return View("Members/Create", model);
        }

        _context.Members.Add(model);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Member created successfully.";

        return RedirectToAction(nameof(Members));
    }

    [HttpGet]
    public async Task<IActionResult> MembersEdit(int id)
    {
        var member = await _context.Members
            .FindAsync(id);

        if (member is null)
        {
            return NotFound();
        }

        return View("Members/Edit", member);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MembersEdit(
        int id,
        Member model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View("Members/Edit", model);
        }

        var member = await _context.Members
            .FirstOrDefaultAsync(member => member.Id == id);

        if (member is null)
        {
            return NotFound();
        }

        var emailExists = await _context.Members
            .AnyAsync(otherMember =>
                otherMember.Email == model.Email &&
                otherMember.Id != id);

        if (emailExists)
        {
            ModelState.AddModelError(
                nameof(model.Email),
                "Another member already uses this email.");

            return View("Members/Edit", model);
        }

        member.FullName = model.FullName;
        member.Email = model.Email;
        member.Height = model.Height;
        member.Weight = model.Weight;
        member.Goal = model.Goal;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Member updated successfully.";

        return RedirectToAction(nameof(MembersDetails), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> MembersDelete(int id)
    {
        var member = await _context.Members
            .AsNoTracking()
            .FirstOrDefaultAsync(member => member.Id == id);

        if (member is null)
        {
            return NotFound();
        }

        return View("Members/Delete", member);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MembersDeleteConfirmed(int id)
    {
        var member = await _context.Members
            .FirstOrDefaultAsync(member => member.Id == id);

        if (member is null)
        {
            return NotFound();
        }

        _context.Members.Remove(member);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Member deleted successfully.";

        return RedirectToAction(nameof(Members));
    }

    // Subscriptions
    [HttpGet]
    public IActionResult Subscriptions()
    {
        return View("Subscriptions/Index");
    }

    // Payments
    [HttpGet]
    public IActionResult Payments()
    {
        return View("Payments/Index");
    }

    // Reports
    [HttpGet]
    public IActionResult Reports()
    {
        return View("Reports/Index");
    }
}