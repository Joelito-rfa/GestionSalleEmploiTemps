using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EMIT.Application.DTOs;
using EMIT.Infrastructure.Data;
using EMIT.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestionSalleEmploiTemps.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context,
        ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var session = new UserSession
                    {
                        UserId = user.Id,
                        LoginAt = DateTime.UtcNow,
                        LastActivityAt = DateTime.UtcNow,
                        IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                    };
                    _context.UserSessions.Add(session);
                    await _context.SaveChangesAsync();
                }
                _logger.LogInformation("User {Email} logged in.", model.Email);
                return await RedirectToLocal(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return View("Lockout");
            }
            ModelState.AddModelError(string.Empty, "Email ou mot de passe incorrect.");
        }
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var userId = _userManager.GetUserId(User);
        if (userId != null)
        {
            var activeSession = await _context.UserSessions
                .Where(s => s.UserId == userId && s.IsActive)
                .OrderByDescending(s => s.LoginAt)
                .FirstOrDefaultAsync();
            if (activeSession != null)
            {
                activeSession.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out.");
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterDto model)
    {
        if (ModelState.IsValid)
        {
            if (!await _roleManager.RoleExistsAsync(model.Role))
            {
                await _roleManager.CreateAsync(new IdentityRole(model.Role));
            }

            string? matchedUserId = null;

            if (model.Role == "Student")
            {
                var student = await _context.Students
                    .FirstOrDefaultAsync(s => s.Matricule == model.Identifier);
                if (student == null)
                {
                    ModelState.AddModelError(string.Empty, "Matricule incorrect ou étudiant introuvable.");
                    return View(model);
                }
                if (!string.IsNullOrEmpty(student.UserId))
                {
                    ModelState.AddModelError(string.Empty, "Cet étudiant a déjà un compte associé.");
                    return View(model);
                }
                matchedUserId = student.Id.ToString();
            }
            else if (model.Role == "Teacher")
            {
                var teacher = await _context.Teachers
                    .FirstOrDefaultAsync(t => t.Numero == model.Identifier);
                if (teacher == null)
                {
                    ModelState.AddModelError(string.Empty, "Numéro incorrect ou enseignant introuvable.");
                    return View(model);
                }
                if (!string.IsNullOrEmpty(teacher.UserId))
                {
                    ModelState.AddModelError(string.Empty, "Cet enseignant a déjà un compte associé.");
                    return View(model);
                }
                matchedUserId = teacher.Id.ToString();
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = $"{model.FirstName} {model.LastName}",
                Role = model.Role
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Role);

                if (model.Role == "Student" && matchedUserId != null)
                {
                    var studentId = int.Parse(matchedUserId);
                    var student = await _context.Students.FindAsync(studentId);
                    if (student != null)
                    {
                        student.UserId = user.Id;
                        await _context.SaveChangesAsync();
                    }
                }
                else if (model.Role == "Teacher" && matchedUserId != null)
                {
                    var teacherId = int.Parse(matchedUserId);
                    var teacher = await _context.Teachers.FindAsync(teacherId);
                    if (teacher != null)
                    {
                        teacher.UserId = user.Id;
                        await _context.SaveChangesAsync();
                    }
                }

                _logger.LogInformation("User created a new account with password. Role: {Role}", model.Role);
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction(nameof(HomeController.Timetable), "Home");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        return View(model);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Users()
    {
        var users = _userManager.Users.OrderBy(u => u.FullName).ToList();
        var items = new List<UserListItemDto>();
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            var isLocked = await _userManager.IsLockedOutAsync(u);
            items.Add(new UserListItemDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email ?? "",
                Role = roles.FirstOrDefault(),
                CreatedAt = u.CreatedAt,
                IsLockedOut = isLocked
            });
        }
        return View(items);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ResetPassword(string id)
    {
        if (string.IsNullOrEmpty(id)) return NotFound();

        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        ViewBag.UserFullName = user.FullName;
        ViewBag.UserEmail = user.Email;
        return View(new AdminResetPasswordDto { UserId = id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ResetPassword(AdminResetPasswordDto model)
    {
        if (!ModelState.IsValid)
        {
            var u = await _userManager.FindByIdAsync(model.UserId);
            ViewBag.UserFullName = u?.FullName;
            ViewBag.UserEmail = u?.Email;
            return View(model);
        }

        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null) return NotFound();

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            ViewBag.UserFullName = user.FullName;
            ViewBag.UserEmail = user.Email;
            return View(model);
        }

        await _userManager.SetLockoutEndDateAsync(user, null);
        _logger.LogInformation("Admin reset password for user {UserId}", user.Id);
        TempData["Success"] = $"Mot de passe réinitialisé pour {user.FullName}.";
        return RedirectToAction(nameof(Users));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ConnectedUsers()
    {
        var connectedSessions = await _context.UserSessions
            .Where(s => s.IsActive)
            .OrderByDescending(s => s.LoginAt)
            .ToListAsync();

        var items = new List<ConnectedUserDto>();
        foreach (var session in connectedSessions)
        {
            var user = await _userManager.FindByIdAsync(session.UserId);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                items.Add(new ConnectedUserDto
                {
                    FullName = user.FullName,
                    Email = user.Email ?? "",
                    Role = roles.FirstOrDefault() ?? "",
                    LoginAt = session.LoginAt,
                    LastActivityAt = session.LastActivityAt,
                    IpAddress = session.IpAddress
                });
            }
        }
        return View(items);
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private async Task<IActionResult> RedirectToLocal(string? returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            if (await _userManager.IsInRoleAsync(user, "Student"))
                return RedirectToAction(nameof(HomeController.Timetable), "Home");
        }

        return RedirectToAction(nameof(DashboardController.Index), "Dashboard");
    }
}
