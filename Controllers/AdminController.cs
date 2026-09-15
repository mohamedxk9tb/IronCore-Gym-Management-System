using GymMvc.Data;
using GymMvc.Models;
using GymMvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymMvc.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private const int PageSize = 10;

    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // =========================================================
    // Dashboard
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var now = DateTime.Now;

        var activeMembers = await _context.Subscriptions
            .Where(s =>
                s.Status == "Active" &&
                s.StartDate <= now &&
                s.EndDate >= now)
            .Select(s => s.MemberId)
            .Distinct()
            .CountAsync();

        var monthlyIncome = await _context.Payments
            .Where(p =>
                p.PaymentDate.Year == now.Year &&
                p.PaymentDate.Month == now.Month)
            .SumAsync(p => (decimal?)p.Amount) ?? 0m;

        var expiringSoon = await _context.Subscriptions
            .CountAsync(s =>
                s.Status == "Active" &&
                s.EndDate >= now &&
                s.EndDate <= now.AddDays(7));

        var classesCount = await _context.GymClasses
            .CountAsync();

        ViewBag.ActiveMembers = activeMembers;
        ViewBag.MonthlyIncome = monthlyIncome;
        ViewBag.ExpiringSoon = expiringSoon;
        ViewBag.ClassesCount = classesCount;

        return View();
    }


    // =========================================================
    // Plans
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Plans(
        string? search,
        int page = 1)
    {
        if (page < 1)
        {
            page = 1;
        }

        var query = _context.Plans
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(p =>
                p.Name.Contains(search));
        }

        var totalPlans = await query.CountAsync();

        var totalPages =
            (int)Math.Ceiling(totalPlans / (double)PageSize);

        if (totalPages > 0 && page > totalPages)
        {
            page = totalPages;
        }

        var plans = await query
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        ViewBag.Search = search;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalPlans = totalPlans;

        return View("Plans/Index", plans);
    }


    [HttpGet]
    public IActionResult PlansCreate()
    {
        return View("Plans/Create", new Plan());
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlansCreate(Plan model)
    {
        if (!ModelState.IsValid)
        {
            return View("Plans/Create", model);
        }

        var nameExists = await _context.Plans
            .AnyAsync(p => p.Name == model.Name);

        if (nameExists)
        {
            ModelState.AddModelError(
                nameof(model.Name),
                "A plan with this name already exists.");

            return View("Plans/Create", model);
        }

        _context.Plans.Add(model);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Plan created successfully.";

        return RedirectToAction(nameof(Plans));
    }


    [HttpGet]
    public async Task<IActionResult> PlansEdit(int id)
    {
        var plan = await _context.Plans
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (plan is null)
        {
            return NotFound();
        }

        return View("Plans/Edit", plan);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlansEdit(
        int id,
        Plan model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View("Plans/Edit", model);
        }

        var plan = await _context.Plans
            .FirstOrDefaultAsync(p => p.Id == id);

        if (plan is null)
        {
            return NotFound();
        }

        var duplicateName = await _context.Plans
            .AnyAsync(p =>
                p.Id != id &&
                p.Name == model.Name);

        if (duplicateName)
        {
            ModelState.AddModelError(
                nameof(model.Name),
                "Another plan already uses this name.");

            return View("Plans/Edit", model);
        }

        plan.Name = model.Name;
        plan.DurationMonths = model.DurationMonths;
        plan.Price = model.Price;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Plan updated successfully.";

        return RedirectToAction(
            nameof(PlansDetails),
            new { id });
    }


    [HttpGet]
    public async Task<IActionResult> PlansDetails(int id)
    {
        var plan = await _context.Plans
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (plan is null)
        {
            return NotFound();
        }

        return View("Plans/Details", plan);
    }


    [HttpGet]
    public async Task<IActionResult> PlansDelete(int id)
    {
        var plan = await _context.Plans
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (plan is null)
        {
            return NotFound();
        }

        return View("Plans/Delete", plan);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlansDeleteConfirmed(int id)
    {
        var plan = await _context.Plans
            .FirstOrDefaultAsync(p => p.Id == id);

        if (plan is null)
        {
            return NotFound();
        }

        var hasSubscriptions = await _context.Subscriptions
            .AnyAsync(s => s.PlanId == id);

        if (hasSubscriptions)
        {
            TempData["ErrorMessage"] =
                "This plan cannot be deleted because it has subscription records.";

            return RedirectToAction(
                nameof(PlansDelete),
                new { id });
        }

        _context.Plans.Remove(plan);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Plan deleted successfully.";

        return RedirectToAction(nameof(Plans));
    }


    // =========================================================
    // Classes
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Classes(
        string? search,
        string? day,
        string? sort,
        int page = 1)
    {
        if (page < 1)
        {
            page = 1;
        }

        var query = _context.GymClasses
            .AsNoTracking()
            .Include(c => c.Trainer)
            .Include(c => c.Bookings)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(c =>
                c.Name.Contains(search) ||
                c.Trainer.FullName.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(day))
        {
            query = query.Where(c =>
                c.DayOfWeek == day);
        }

        query = sort switch
        {
            "name" =>
                query.OrderBy(c => c.Name),

            "name_desc" =>
                query.OrderByDescending(c => c.Name),

            "time" =>
                query.OrderBy(c => c.StartTime),

            "time_desc" =>
                query.OrderByDescending(c => c.StartTime),

            "capacity" =>
                query.OrderBy(c => c.Capacity),

            "capacity_desc" =>
                query.OrderByDescending(c => c.Capacity),

            _ =>
                query
                    .OrderBy(c => c.DayOfWeek)
                    .ThenBy(c => c.StartTime)
        };

        var totalClasses = await query.CountAsync();

        var totalPages =
            (int)Math.Ceiling(
                totalClasses / (double)PageSize);

        if (totalPages > 0 && page > totalPages)
        {
            page = totalPages;
        }

        var classes = await query
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        ViewBag.Search = search;
        ViewBag.Day = day;
        ViewBag.Sort = sort;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalClasses = totalClasses;

        return View("Classes/Index", classes);
    }


    [HttpGet]
    public async Task<IActionResult> ClassesCreate()
    {
        await LoadTrainersAsync();

        return View(
            "Classes/Create",
            new GymClass());
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClassesCreate(
        GymClass model)
    {
        var trainerExists = await _context.Trainers
            .AnyAsync(t => t.Id == model.TrainerId);

        if (!trainerExists)
        {
            ModelState.AddModelError(
                nameof(model.TrainerId),
                "Selected trainer does not exist.");
        }

        if (!ModelState.IsValid)
        {
            await LoadTrainersAsync(model.TrainerId);

            return View("Classes/Create", model);
        }

        _context.GymClasses.Add(model);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Class created successfully.";

        return RedirectToAction(nameof(Classes));
    }


    [HttpGet]
    public async Task<IActionResult> ClassesEdit(int id)
    {
        var gymClass = await _context.GymClasses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (gymClass is null)
        {
            return NotFound();
        }

        await LoadTrainersAsync(gymClass.TrainerId);

        return View("Classes/Edit", gymClass);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClassesEdit(
        int id,
        GymClass model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var trainerExists = await _context.Trainers
            .AnyAsync(t => t.Id == model.TrainerId);

        if (!trainerExists)
        {
            ModelState.AddModelError(
                nameof(model.TrainerId),
                "Selected trainer does not exist.");
        }

        if (!ModelState.IsValid)
        {
            await LoadTrainersAsync(model.TrainerId);

            return View("Classes/Edit", model);
        }

        var gymClass = await _context.GymClasses
            .FirstOrDefaultAsync(c => c.Id == id);

        if (gymClass is null)
        {
            return NotFound();
        }

        gymClass.Name = model.Name;
        gymClass.TrainerId = model.TrainerId;
        gymClass.DayOfWeek = model.DayOfWeek;
        gymClass.StartTime = model.StartTime;
        gymClass.DurationMinutes = model.DurationMinutes;
        gymClass.Capacity = model.Capacity;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Class updated successfully.";

        return RedirectToAction(
            nameof(ClassesDetails),
            new { id });
    }


    [HttpGet]
    public async Task<IActionResult> ClassesDetails(int id)
    {
        var gymClass = await _context.GymClasses
            .AsNoTracking()
            .Include(c => c.Trainer)
            .Include(c => c.Bookings)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (gymClass is null)
        {
            return NotFound();
        }

        return View("Classes/Details", gymClass);
    }


    [HttpGet]
    public async Task<IActionResult> ClassesDelete(int id)
    {
        var gymClass = await _context.GymClasses
            .AsNoTracking()
            .Include(c => c.Trainer)
            .Include(c => c.Bookings)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (gymClass is null)
        {
            return NotFound();
        }

        return View("Classes/Delete", gymClass);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClassesDeleteConfirmed(
        int id)
    {
        var gymClass = await _context.GymClasses
            .FirstOrDefaultAsync(c => c.Id == id);

        if (gymClass is null)
        {
            return NotFound();
        }

        var hasBookings = await _context.Bookings
            .AnyAsync(b => b.GymClassId == id);

        if (hasBookings)
        {
            TempData["ErrorMessage"] =
                "This class cannot be deleted because it has booking records.";

            return RedirectToAction(
                nameof(ClassesDelete),
                new { id });
        }

        _context.GymClasses.Remove(gymClass);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Class deleted successfully.";

        return RedirectToAction(nameof(Classes));
    }


    // =========================================================
    // Trainers
    // =========================================================
    //
    // Ziad owns the real Admin trainer CRUD through
    // AdminTrainersController.
    //
    // These actions preserve the existing Admin URLs used
    // by the Admin Views/sidebar while forwarding the request
    // to Ziad's controller.
    // =========================================================

    [HttpGet]
    public IActionResult Trainers()
    {
        return RedirectToAction(
            "Index",
            "AdminTrainers");
    }


    [HttpGet]
    public IActionResult TrainersCreate()
    {
        return RedirectToAction(
            "Create",
            "AdminTrainers");
    }


    [HttpGet]
    public IActionResult TrainersEdit(int id)
    {
        return RedirectToAction(
            "Edit",
            "AdminTrainers",
            new { id });
    }


    [HttpGet]
    public IActionResult TrainersDelete(int id)
    {
        return RedirectToAction(
            "Delete",
            "AdminTrainers",
            new { id });
    }


    [HttpGet]
    public IActionResult TrainersDetails(int id)
    {
        TempData["Info"] =
            "Trainer details are managed from the trainer administration area.";

        return RedirectToAction(
            "Edit",
            "AdminTrainers",
            new { id });
    }


    // =========================================================
    // Members
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Members(
        string? search,
        int page = 1)
    {
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
            (int)Math.Ceiling(
                totalMembers / (double)PageSize);

        if (totalPages > 0 && page > totalPages)
        {
            page = totalPages;
        }

        var members = await query
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
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
    public IActionResult MembersCreate()
    {
        return View(
            "Members/Create",
            new Member());
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MembersCreate(
        Member model)
    {
        if (!ModelState.IsValid)
        {
            return View(
                "Members/Create",
                model);
        }

        var existingMember = await _context.Members
            .AnyAsync(member =>
                member.Email == model.Email);

        if (existingMember)
        {
            ModelState.AddModelError(
                nameof(model.Email),
                "A member with this email already exists.");

            return View(
                "Members/Create",
                model);
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

        return View(
            "Members/Edit",
            member);
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
            return View(
                "Members/Edit",
                model);
        }

        var member = await _context.Members
            .FirstOrDefaultAsync(
                member => member.Id == id);

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

            return View(
                "Members/Edit",
                model);
        }

        member.FullName = model.FullName;
        member.Email = model.Email;
        member.Height = model.Height;
        member.Weight = model.Weight;
        member.Goal = model.Goal;
        member.FitnessLevel = model.FitnessLevel;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Member updated successfully.";

        return RedirectToAction(
            nameof(MembersDetails),
            new { id });
    }


    [HttpGet]
    public async Task<IActionResult> MembersDelete(int id)
    {
        var member = await _context.Members
            .AsNoTracking()
            .FirstOrDefaultAsync(member =>
                member.Id == id);

        if (member is null)
        {
            return NotFound();
        }

        return View(
            "Members/Delete",
            member);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MembersDeleteConfirmed(
        int id)
    {
        var member = await _context.Members
            .FirstOrDefaultAsync(member =>
                member.Id == id);

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


    // =========================================================
    // Subscriptions
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Subscriptions()
    {
        var subscriptions = await _context.Subscriptions
            .AsNoTracking()
            .Include(s => s.Member)
            .Include(s => s.Plan)
            .OrderByDescending(s => s.StartDate)
            .ToListAsync();

        var now = DateTime.Now;

        var viewModel =
            new SubscriptionListViewModel();

        foreach (var subscription in subscriptions)
        {
            var daysRemaining =
                (subscription.EndDate - now).Days;

            viewModel.Subscriptions.Add(
                new SubscriptionRowViewModel
                {
                    Subscription = subscription,
                    PlanName =
                        subscription.Plan?.Name
                        ?? "Unknown plan",
                    PlanPrice =
                        subscription.Plan?.Price
                        ?? 0m,
                    DaysRemaining =
                        daysRemaining > 0
                            ? daysRemaining
                            : 0,
                    IsExpired =
                        subscription.EndDate < now
                });
        }

        return View(
            "Subscriptions/Index",
            viewModel);
    }


    // =========================================================
    // Payments
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Payments()
    {
        var payments = await _context.Payments
            .AsNoTracking()
            .Include(p => p.Subscription)
                .ThenInclude(s => s.Plan)
            .Include(p => p.Subscription)
                .ThenInclude(s => s.Member)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();

        var viewModel =
            new PaymentListViewModel();

        foreach (var payment in payments)
        {
            viewModel.Payments.Add(
                new PaymentRowViewModel
                {
                    Payment = payment,
                    PlanName =
                        payment.Subscription?.Plan?.Name
                        ?? "Unknown plan"
                });
        }

        return View(
            "Payments/Index",
            viewModel);
    }


    // =========================================================
    // Reports
    // =========================================================
    //
    // Ziad owns ReportsController.
    // Do not duplicate report calculations here.
    // =========================================================

    [HttpGet]
    public IActionResult Reports()
    {
        return RedirectToAction(
            "Index",
            "Reports");
    }


    // =========================================================
    // Helpers
    // =========================================================

    private async Task LoadTrainersAsync(
        int? selectedTrainerId = null)
    {
        var trainers = await _context.Trainers
            .AsNoTracking()
            .OrderBy(t => t.FullName)
            .ToListAsync();

        ViewBag.Trainers = trainers;
        ViewBag.SelectedTrainerId =
            selectedTrainerId;
    }
}