using GymMvc.Data;
using GymMvc.Models;
using GymMvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymMvc.Controllers;

[Authorize(Roles = "Member")]
public class MemberController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public MemberController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // =========================
    // Dashboard
    // =========================

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return Challenge();
        }

        var member = await _context.Members
            .Include(member => member.WeightLogs)
            .Include(member => member.CheckIns)
            .FirstOrDefaultAsync(
                member => member.ApplicationUserId == user.Id);

        if (member is null)
        {
            return NotFound();
        }

        var now = DateTime.Now;
        var today = now.Date;

        // =========================
        // Check-ins
        // =========================

        var monthlyCheckIns = member.CheckIns
            .Count(checkIn =>
                checkIn.CheckInTime.Year == now.Year &&
                checkIn.CheckInTime.Month == now.Month);

        // =========================
        // Weight
        // =========================

        var latestWeightLog = member.WeightLogs
            .OrderByDescending(weightLog => weightLog.Date)
            .FirstOrDefault();

        var previousWeightLog = member.WeightLogs
            .OrderByDescending(weightLog => weightLog.Date)
            .Skip(1)
            .FirstOrDefault();

        var currentWeight =
            latestWeightLog?.Weight ?? member.Weight;

        decimal? previousWeight =
            previousWeightLog?.Weight;

        decimal? weightChange = null;

        if (latestWeightLog is not null &&
            previousWeightLog is not null)
        {
            weightChange =
                latestWeightLog.Weight -
                previousWeightLog.Weight;
        }

        // =========================
        // Active Subscription
        // =========================

        var activeSubscription = await _context.Subscriptions
            .Include(subscription => subscription.Plan)
            .Where(subscription =>
                subscription.MemberId == member.Id &&
                subscription.Status == "Active" &&
                subscription.StartDate <= now &&
                subscription.EndDate >= now)
            .OrderByDescending(subscription => subscription.EndDate)
            .FirstOrDefaultAsync();

        var subscriptionStatus =
            activeSubscription is not null
                ? "Active"
                : "No Active Subscription";

        var daysRemaining = 0;

        if (activeSubscription is not null)
        {
            daysRemaining = Math.Max(
                0,
                (activeSubscription.EndDate.Date - today).Days);
        }

        // =========================
        // Next Booked Class
        // =========================

        var bookedClasses = await _context.Bookings
            .Include(booking => booking.GymClass)
            .Where(booking =>
                booking.MemberId == member.Id &&
                booking.Status == "Booked")
            .ToListAsync();

        var upcomingClasses = bookedClasses
            .Select(booking => new
            {
                Booking = booking,
                ClassDateTime = GetNextOccurrence(
                    booking.GymClass.DayOfWeek,
                    booking.GymClass.StartTime)
            })
            .Where(item => item.ClassDateTime != DateTime.MaxValue)
            .OrderBy(item => item.ClassDateTime)
            .ToList();

        var nextBookedClass = upcomingClasses.FirstOrDefault();

        int? nextClassAvailableSpots = null;

        if (nextBookedClass is not null)
        {
            var gymClassId = nextBookedClass.Booking.GymClassId;

            var bookedCount = await _context.Bookings
                .CountAsync(booking =>
                    booking.GymClassId == gymClassId &&
                    booking.Status == "Booked");

            nextClassAvailableSpots = Math.Max(
                0,
                nextBookedClass.Booking.GymClass.Capacity - bookedCount);
        }

        // =========================
        // View Model
        // =========================

        var model = new MemberDashboardViewModel
        {
            FullName = member.FullName,
            Height = member.Height,
            CurrentWeight = currentWeight,
            PreviousWeight = previousWeight,
            Goal = member.Goal,

            MonthlyCheckIns = monthlyCheckIns,
            TotalWeightLogs = member.WeightLogs.Count,

            WeightChange = weightChange,

            LastCheckIn = member.CheckIns
                .OrderByDescending(checkIn => checkIn.CheckInTime)
                .Select(checkIn => (DateTime?)checkIn.CheckInTime)
                .FirstOrDefault(),

            LastWeightLog = latestWeightLog?.Date,

            SubscriptionStatus = subscriptionStatus,
            PlanName = activeSubscription?.Plan?.Name,
            SubscriptionEndDate = activeSubscription?.EndDate,
            DaysRemaining = daysRemaining,

            NextClassName =
                nextBookedClass?.Booking.GymClass.Name,

            NextClassDateTime =
                nextBookedClass?.ClassDateTime,

            NextClassAvailableSpots =
                nextClassAvailableSpots
        };

        return View(model);
    }

    // =========================
    // Profile
    // =========================

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return Challenge();
        }

        var member = await _context.Members
            .FirstOrDefaultAsync(
                member => member.ApplicationUserId == user.Id);

        if (member is null)
        {
            return NotFound();
        }

        var model = new MemberProfileViewModel
        {
            FullName = member.FullName,
            Email = member.Email,
            Height = member.Height,
            Weight = member.Weight,
            Goal = member.Goal,
            FitnessLevel = member.FitnessLevel
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(
        MemberProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return Challenge();
        }

        var member = await _context.Members
            .FirstOrDefaultAsync(
                member => member.ApplicationUserId == user.Id);

        if (member is null)
        {
            return NotFound();
        }

        member.FullName = model.FullName;
        member.Email = model.Email;
        member.Height = model.Height;
        member.Weight = model.Weight;
        member.Goal = model.Goal;
        member.FitnessLevel = model.FitnessLevel;

        user.FullName = model.FullName;
        user.Email = model.Email;
        user.UserName = model.Email;

        var updateUserResult =
            await _userManager.UpdateAsync(user);

        if (!updateUserResult.Succeeded)
        {
            foreach (var error in updateUserResult.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }

            return View(model);
        }

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Your profile has been updated.";

        return RedirectToAction(nameof(Profile));
    }

    // =========================
    // Change Password
    // =========================

    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(
        ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return Challenge();
        }

        var result = await _userManager.ChangePasswordAsync(
            user,
            model.CurrentPassword,
            model.NewPassword);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }

            return View(model);
        }

        await _userManager.UpdateSecurityStampAsync(user);

        TempData["SuccessMessage"] =
            "Your password has been changed successfully.";

        return RedirectToAction(nameof(Profile));
    }

    // =========================
    // Progress
    // =========================

    [HttpGet]
    public async Task<IActionResult> Progress()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return Challenge();
        }

        var member = await _context.Members
            .Include(member => member.WeightLogs)
            .FirstOrDefaultAsync(
                member => member.ApplicationUserId == user.Id);

        if (member is null)
        {
            return NotFound();
        }

        var history = member.WeightLogs
            .OrderBy(weightLog => weightLog.Date)
            .Select(weightLog => new WeightProgressPoint
            {
                Date = weightLog.Date,
                Weight = weightLog.Weight
            })
            .ToList();

        var firstWeight = history.FirstOrDefault();
        var latestWeight = history.LastOrDefault();

        var currentWeight =
            latestWeight?.Weight ?? member.Weight;

        var startingWeight =
            firstWeight?.Weight ?? member.Weight;

        decimal? totalChange = null;

        if (history.Count > 1)
        {
            totalChange =
                latestWeight!.Weight -
                firstWeight!.Weight;
        }

        var model = new MemberProgressViewModel
        {
            CurrentWeight = currentWeight,
            StartingWeight = startingWeight,
            StartingDate = firstWeight?.Date,
            TotalChange = totalChange,
            TotalRecords = history.Count,
            WeightHistory = history,
            NewWeight = currentWeight,
            NewWeightDate = DateTime.Today
        };

        return View(model);
    }

    // =========================
    // Log Weight
    // =========================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LogWeight(
        MemberProgressViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return await Progress();
        }

        var user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return Challenge();
        }

        var member = await _context.Members
            .FirstOrDefaultAsync(
                member => member.ApplicationUserId == user.Id);

        if (member is null)
        {
            return NotFound();
        }

        var weightLog = new WeightLog
        {
            MemberId = member.Id,
            Weight = model.NewWeight,
            Date = model.NewWeightDate
        };

        _context.WeightLogs.Add(weightLog);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Your weight progress has been recorded.";

        return RedirectToAction(nameof(Progress));
    }

    // =========================
    // Check In
    // =========================

    [HttpGet]
    public async Task<IActionResult> CheckIn()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return Challenge();
        }

        var member = await _context.Members
            .FirstOrDefaultAsync(
                member => member.ApplicationUserId == user.Id);

        if (member is null)
        {
            return NotFound();
        }

        var now = DateTime.Now;

        var activeSubscription = await _context.Subscriptions
            .Include(subscription => subscription.Plan)
            .Where(subscription =>
                subscription.MemberId == member.Id &&
                subscription.Status == "Active" &&
                subscription.StartDate <= now &&
                subscription.EndDate >= now)
            .OrderByDescending(subscription => subscription.EndDate)
            .FirstOrDefaultAsync();

        ViewBag.HasActiveSubscription =
            activeSubscription is not null;

        ViewBag.ActivePlanName =
            activeSubscription?.Plan?.Name;

        ViewBag.SubscriptionEndDate =
            activeSubscription?.EndDate;

        ViewBag.LastCheckIn = await _context.CheckIns
            .Where(checkIn => checkIn.MemberId == member.Id)
            .OrderByDescending(checkIn => checkIn.CheckInTime)
            .Select(checkIn => (DateTime?)checkIn.CheckInTime)
            .FirstOrDefaultAsync();

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckInNow()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return Challenge();
        }

        var member = await _context.Members
            .FirstOrDefaultAsync(
                member => member.ApplicationUserId == user.Id);

        if (member is null)
        {
            return NotFound();
        }

        // =========================
        // Active Subscription Check
        // =========================

        var now = DateTime.Now;

        var hasActiveSubscription = await _context.Subscriptions
            .AnyAsync(subscription =>
                subscription.MemberId == member.Id &&
                subscription.Status == "Active" &&
                subscription.StartDate <= now &&
                subscription.EndDate >= now);

        if (!hasActiveSubscription)
        {
            TempData["ErrorMessage"] =
                "You need an active subscription to check in.";

            return RedirectToAction(nameof(CheckIn));
        }

        // =========================
        // Create Check-in
        // =========================

        var checkIn = new CheckIn
        {
            MemberId = member.Id,
            CheckInTime = now
        };

        _context.CheckIns.Add(checkIn);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Check-in completed successfully.";

        return RedirectToAction(nameof(CheckIn));
    }

    // =========================
    // Helpers
    // =========================

    private static DateTime GetNextOccurrence(
        string dayOfWeekName,
        TimeSpan startTime)
    {
        if (!Enum.TryParse<DayOfWeek>(
                dayOfWeekName,
                true,
                out var targetDay))
        {
            return DateTime.MaxValue;
        }

        var now = DateTime.Now;
        var today = now.Date;

        var daysUntilTarget =
            ((int)targetDay - (int)today.DayOfWeek + 7) % 7;

        var candidateDate =
            today.AddDays(daysUntilTarget);

        var candidateDateTime =
            candidateDate.Add(startTime);

        if (daysUntilTarget == 0 &&
            candidateDateTime <= now)
        {
            candidateDateTime =
                candidateDateTime.AddDays(7);
        }

        return candidateDateTime;
    }
}