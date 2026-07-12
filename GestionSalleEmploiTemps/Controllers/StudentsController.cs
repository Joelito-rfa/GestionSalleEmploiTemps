using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using EMIT.Application.DTOs;
using EMIT.Application.Interfaces;
using EMIT.Domain.Enums;

namespace GestionSalleEmploiTemps.Controllers;

[Authorize(Roles = "Admin")]
public class StudentsController : Controller
{
    private readonly IStudentService _studentService;
    private readonly ILogger<StudentsController> _logger;

    public StudentsController(IStudentService studentService, ILogger<StudentsController> logger)
    {
        _studentService = studentService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(StudentLevel? level)
    {
        var students = level.HasValue
            ? await _studentService.GetByLevelAsync(level.Value)
            : await _studentService.GetAllAsync();

        ViewBag.Levels = Enum.GetValues<StudentLevel>().Select(l => new SelectListItem
        {
            Value = l.ToString(),
            Text = l.ToString(),
            Selected = level.HasValue && level.Value == l
        }).ToList();

        return View(students);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var student = await _studentService.GetByIdAsync(id.Value);
        if (student == null) return NotFound();
        return View(student);
    }

    public IActionResult Create()
    {
        ViewBag.Levels = new SelectList(Enum.GetValues<StudentLevel>());
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateStudentDto dto)
    {
        if (ModelState.IsValid)
        {
            await _studentService.CreateAsync(dto);
            TempData["Success"] = "Étudiant créé avec succès.";
            return RedirectToAction(nameof(Index));
        }
        ViewBag.Levels = new SelectList(Enum.GetValues<StudentLevel>());
        return View(dto);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var student = await _studentService.GetByIdAsync(id.Value);
        if (student == null) return NotFound();
        ViewBag.Levels = new SelectList(Enum.GetValues<StudentLevel>(), student.Level);
        return View(new UpdateStudentDto { Id = student.Id, FullName = student.FullName, Email = student.Email, Level = student.Level });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateStudentDto dto)
    {
        if (id != dto.Id) return NotFound();
        if (ModelState.IsValid)
        {
            try
            {
                await _studentService.UpdateAsync(dto);
                TempData["Success"] = "Étudiant modifié avec succès.";
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException) { return NotFound(); }
        }
        ViewBag.Levels = new SelectList(Enum.GetValues<StudentLevel>());
        return View(dto);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var student = await _studentService.GetByIdAsync(id.Value);
        if (student == null) return NotFound();
        return View(student);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _studentService.DeleteAsync(id);
        TempData["Success"] = "Étudiant supprimé avec succès.";
        return RedirectToAction(nameof(Index));
    }
}
