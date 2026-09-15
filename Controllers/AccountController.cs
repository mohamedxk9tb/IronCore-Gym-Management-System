using GymMvc.Data;
using GymMvc.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GymMvc.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _context;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        string email,
        string password,
        string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError(
                string.Empty,
                "Email and password are required.");

            return View();
        }

        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
        {
            ModelState.AddModelError(
                string.Empty,
                "Invalid email or password.");

            return View();
        }

        var result = await _signInManager.PasswordSignInAsync(
            user.UserName!,
            password,
            isPersistent: false,
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            // Keep the original requested URL if it is local.
            if (!string.IsNullOrWhiteSpace(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // Redirect users according to their role.
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return RedirectToAction(
                    "Dashboard",
                    "Admin");
            }

            if (await _userManager.IsInRoleAsync(user, "Trainer"))
            {
                return RedirectToAction(
                    "Dashboard",
                    "TrainerDashboard");
            }

            if (await _userManager.IsInRoleAsync(user, "Member"))
            {
                return RedirectToAction(
                    "Dashboard",
                    "Member");
            }

            // Fallback for users without a recognized role.
            return RedirectToAction(
                "Index",
                "Home");
        }

        ModelState.AddModelError(
            string.Empty,
            "Invalid email or password.");

        return View();
    }

    [HttpGet]
    public IActionResult SignUp()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SignUp(
        string fullName,
        string email,
        string password,
        string confirmPassword,
        string fitnessLevel)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            ModelState.AddModelError(
                nameof(fullName),
                "Full name is required.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            ModelState.AddModelError(
                nameof(email),
                "Email is required.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError(
                nameof(password),
                "Password is required.");
        }

        if (string.IsNullOrWhiteSpace(confirmPassword))
        {
            ModelState.AddModelError(
                nameof(confirmPassword),
                "Please confirm your password.");
        }
        else if (password != confirmPassword)
        {
            ModelState.AddModelError(
                nameof(confirmPassword),
                "Passwords do not match.");
        }

        if (string.IsNullOrWhiteSpace(fitnessLevel))
        {
            ModelState.AddModelError(
                nameof(fitnessLevel),
                "Please select your fitness level.");
        }

        if (!ModelState.IsValid)
        {
            return View();
        }

        var existingUser =
            await _userManager.FindByEmailAsync(email);

        if (existingUser is not null)
        {
            ModelState.AddModelError(
                nameof(email),
                "An account with this email already exists.");

            return View();
        }

        var user = new ApplicationUser
        {
            UserName = email.Trim(),
            Email = email.Trim(),
            FullName = fullName.Trim()
        };

        var createResult =
            await _userManager.CreateAsync(
                user,
                password);

        if (!createResult.Succeeded)
        {
            foreach (var error in createResult.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }

            return View();
        }

        var member = new Member
        {
            FullName = fullName.Trim(),
            Email = email.Trim(),
            FitnessLevel = fitnessLevel
        };

        _context.Members.Add(member);

        try
        {
            await _context.SaveChangesAsync();

            member.ApplicationUserId = user.Id;

            await _context.SaveChangesAsync();
        }
        catch
        {
            _context.Members.Remove(member);
            await _context.SaveChangesAsync();

            await _userManager.DeleteAsync(user);

            ModelState.AddModelError(
                string.Empty,
                "Account creation failed. Please try again.");

            return View();
        }

        var roleResult =
            await _userManager.AddToRoleAsync(
                user,
                "Member");

        if (!roleResult.Succeeded)
        {
            foreach (var error in roleResult.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }

            _context.Members.Remove(member);
            await _context.SaveChangesAsync();

            await _userManager.DeleteAsync(user);

            return View();
        }

        await _signInManager.SignInAsync(
            user,
            isPersistent: false);

        // New accounts are Members, so send them
        // directly to the Member dashboard.
        return RedirectToAction(
            "Dashboard",
            "Member");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();

        return RedirectToAction(
            "Index",
            "Home");
    }
}