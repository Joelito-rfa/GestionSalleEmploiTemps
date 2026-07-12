using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EMIT.Application.Interfaces;
using EMIT.Infrastructure.Data;
using EMIT.Domain.Enums;
using System.Security.Claims;

namespace GestionSalleEmploiTemps.Controllers;

[Authorize]
public class NotificationsController : Controller
{
    private readonly INotificationService _notificationService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        INotificationService notificationService,
        UserManager<ApplicationUser> userManager,
        ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var role = User.IsInRole("Admin") ? "Admin" :
                   User.IsInRole("Teacher") ? "Teacher" : "Student";

        string? userLevel = null;
        if (role == "Student")
        {
            userLevel = user.Role;
        }

        var notifications = await _notificationService.GetAllForUserAsync(user.Id, role, userLevel);
        return View(notifications);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        await _notificationService.MarkAsReadAsync(id);
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return Ok();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var role = User.IsInRole("Admin") ? "Admin" :
                   User.IsInRole("Teacher") ? "Teacher" : "Student";
        string? userLevel = null;
        if (role == "Student") userLevel = user.Role;

        await _notificationService.MarkAllAsReadAsync(user.Id, role, userLevel);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return Ok();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _notificationService.DeleteAsync(id);
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return Ok();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAll()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var role = User.IsInRole("Admin") ? "Admin" :
                   User.IsInRole("Teacher") ? "Teacher" : "Student";
        string? userLevel = null;
        if (role == "Student") userLevel = user.Role;

        await _notificationService.DeleteAllForUserAsync(user.Id, role, userLevel);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return Ok();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> GetUnreadCount()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Json(0);

        var role = User.IsInRole("Admin") ? "Admin" :
                   User.IsInRole("Teacher") ? "Teacher" : "Student";

        string? userLevel = null;
        if (role == "Student") userLevel = user.Role;

        var count = await _notificationService.GetUnreadCountAsync(user.Id, role, userLevel);
        return Json(count);
    }

    [HttpGet]
    public async Task<IActionResult> GetRecent()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Json(Array.Empty<object>());

        var role = User.IsInRole("Admin") ? "Admin" :
                   User.IsInRole("Teacher") ? "Teacher" : "Student";

        string? userLevel = null;
        if (role == "Student") userLevel = user.Role;

        var notifications = await _notificationService.GetAllForUserAsync(user.Id, role, userLevel);
        var recent = notifications.Take(5).Select(n => new
        {
            n.Id,
            n.Title,
            n.Message,
            n.Type,
            n.IsRead,
            n.TargetLevel,
            CreatedAt = n.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
        });
        return Json(recent);
    }
}
