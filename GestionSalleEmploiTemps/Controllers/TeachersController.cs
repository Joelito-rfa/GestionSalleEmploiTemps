using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EMIT.Application.DTOs;
using EMIT.Application.Interfaces;

namespace GestionSalleEmploiTemps.Controllers;

[Authorize(Roles = "Admin")]
public class TeachersController : Controller
{
    private readonly ITeacherService _teacherService;
    private readonly IScheduleService _scheduleService;
    private readonly ILogger<TeachersController> _logger;

    public TeachersController(ITeacherService teacherService, IScheduleService scheduleService, ILogger<TeachersController> logger)
    {
        _teacherService = teacherService;
        _scheduleService = scheduleService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var teachers = await _teacherService.GetAllAsync();
        return View(teachers);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var teacher = await _teacherService.GetByIdAsync(id.Value);
        if (teacher == null) return NotFound();
        return View(teacher);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTeacherDto dto)
    {
        if (ModelState.IsValid)
        {
            await _teacherService.CreateAsync(dto);
            TempData["Success"] = "Enseignant créé avec succès.";
            return RedirectToAction(nameof(Index));
        }
        return View(dto);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var teacher = await _teacherService.GetByIdAsync(id.Value);
        if (teacher == null) return NotFound();
        return View(new UpdateTeacherDto { Id = teacher.Id, FullName = teacher.FullName, Email = teacher.Email, Subject = teacher.Subject });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateTeacherDto dto)
    {
        if (id != dto.Id) return NotFound();
        if (ModelState.IsValid)
        {
            try
            {
                await _teacherService.UpdateAsync(dto);
                TempData["Success"] = "Enseignant modifié avec succès.";
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException) { return NotFound(); }
        }
        return View(dto);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var teacher = await _teacherService.GetByIdAsync(id.Value);
        if (teacher == null) return NotFound();
        return View(teacher);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _teacherService.DeleteAsync(id);
        TempData["Success"] = "Enseignant supprimé avec succès.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Timetable(int? id)
    {
        if (id == null) return NotFound();
        var teacher = await _teacherService.GetByIdAsync(id.Value);
        if (teacher == null) return NotFound();

        var schedules = await _scheduleService.GetByTeacherAsync(id.Value);
        ViewBag.Teacher = teacher;
        return View(schedules);
    }
}
