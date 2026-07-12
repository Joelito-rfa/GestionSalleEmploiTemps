using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EMIT.Application.DTOs;
using EMIT.Application.Interfaces;
using EMIT.Infrastructure.Data;

namespace GestionSalleEmploiTemps.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly IProfileService _profileService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(
        IProfileService profileService,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<ProfileController> logger)
    {
        _profileService = profileService;
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        var profile = await _profileService.GetProfileAsync(userId);
        return View(profile);
    }

    public async Task<IActionResult> Edit()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        var names = user.FullName.Split(' ', 2);
        var dto = new UpdateProfileDto
        {
            FirstName = names.Length > 0 ? names[0] : "",
            LastName = names.Length > 1 ? names[1] : "",
            Email = user.Email ?? ""
        };

        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateProfileDto dto, IFormFile? profilePicture)
    {
        if (!ModelState.IsValid) return View(dto);

        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        try
        {
            if (profilePicture is { Length: > 0 })
            {
                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
                Directory.CreateDirectory(uploadsDir);

                var fileName = $"{Guid.NewGuid()}_{profilePicture.FileName}";
                var filePath = Path.Combine(uploadsDir, fileName);

                await using var stream = new FileStream(filePath, FileMode.Create);
                await profilePicture.CopyToAsync(stream);

                dto.ProfilePicturePath = $"/uploads/profiles/{fileName}";
            }

            await _profileService.UpdateProfileAsync(userId, dto);
            TempData["Success"] = "Profil mis à jour avec succès.";
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto model)
    {
        if (!ModelState.IsValid) return View(model);

        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        var changeResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!changeResult.Succeeded)
        {
            foreach (var error in changeResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        await _signInManager.RefreshSignInAsync(user);
        TempData["Success"] = "Mot de passe modifié avec succès.";
        return RedirectToAction(nameof(Index));
    }
}
