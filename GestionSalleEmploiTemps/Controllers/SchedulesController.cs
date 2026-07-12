using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using EMIT.Application.DTOs;
using EMIT.Application.Interfaces;
using EMIT.Domain.Enums;
using EMIT.Infrastructure.Data;

namespace GestionSalleEmploiTemps.Controllers;

[Authorize(Roles = "Admin")]
public class SchedulesController : Controller
{
    private readonly IScheduleService _scheduleService;
    private readonly IRoomService _roomService;
    private readonly ITeacherService _teacherService;
    private readonly INotificationService _notificationService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<SchedulesController> _logger;

    public SchedulesController(
        IScheduleService scheduleService,
        IRoomService roomService,
        ITeacherService teacherService,
        INotificationService notificationService,
        UserManager<ApplicationUser> userManager,
        ILogger<SchedulesController> logger)
    {
        _scheduleService = scheduleService;
        _roomService = roomService;
        _teacherService = teacherService;
        _notificationService = notificationService;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<IActionResult> Index(StudentLevel? level)
    {
        var schedules = await _scheduleService.GetByLevelAsync(level);
        ViewBag.Levels = Enum.GetValues<StudentLevel>().Select(l => new SelectListItem
        {
            Value = l.ToString(),
            Text = l.ToString(),
            Selected = level.HasValue && level.Value == l
        }).ToList();
        return View(schedules);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var schedule = await _scheduleService.GetByIdAsync(id.Value);
        if (schedule == null) return NotFound();
        return View(schedule);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateDropdowns();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateScheduleDto dto)
    {
        if (ModelState.IsValid)
        {
            if (dto.EndTime <= dto.StartTime)
            {
                ModelState.AddModelError("EndTime", "L'heure de fin doit être après l'heure de début.");
                await PopulateDropdowns();
                return View(dto);
            }

            try
            {
                var created = await _scheduleService.CreateAsync(dto);
                await NotifyScheduleChangeAsync("ScheduleCreated", "Nouvelle séance ajoutée",
                    $"Une nouvelle séance de {created.Subject} a été planifiée le {GetDayFrenchName(created.Day)} ({created.StartTime.ToString(@"hh\:mm")} - {created.EndTime.ToString(@"hh\:mm")}) pour {created.Level} en {created.RoomName}.",
                    created.Level);
                TempData["Success"] = "Séance planifiée avec succès.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateDropdowns();
                return View(dto);
            }
        }
        await PopulateDropdowns();
        return View(dto);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var schedule = await _scheduleService.GetByIdAsync(id.Value);
        if (schedule == null) return NotFound();
        await PopulateDropdowns();
        return View(new UpdateScheduleDto
        {
            Id = schedule.Id,
            Day = schedule.Day,
            StartTime = schedule.StartTime,
            EndTime = schedule.EndTime,
            Level = schedule.Level,
            RoomId = schedule.RoomId,
            TeacherId = schedule.TeacherId
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateScheduleDto dto)
    {
        if (id != dto.Id) return NotFound();
        if (ModelState.IsValid)
        {
            if (dto.EndTime <= dto.StartTime)
            {
                ModelState.AddModelError("EndTime", "L'heure de fin doit être après l'heure de début.");
                await PopulateDropdowns();
                return View(dto);
            }

            try
            {
                var updated = await _scheduleService.UpdateAsync(dto);
                await NotifyScheduleChangeAsync("ScheduleUpdated", "Séance modifiée",
                    $"La séance de {updated.Subject} le {GetDayFrenchName(updated.Day)} ({updated.StartTime.ToString(@"hh\:mm")} - {updated.EndTime.ToString(@"hh\:mm")}) pour {updated.Level} en {updated.RoomName} a été modifiée.",
                    updated.Level);
                TempData["Success"] = "Séance modifiée avec succès.";
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException) { return NotFound(); }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateDropdowns();
                return View(dto);
            }
        }
        await PopulateDropdowns();
        return View(dto);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var schedule = await _scheduleService.GetByIdAsync(id.Value);
        if (schedule == null) return NotFound();
        return View(schedule);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var schedule = await _scheduleService.GetByIdAsync(id);
        await _scheduleService.DeleteAsync(id);

        if (schedule != null)
        {
            await NotifyScheduleChangeAsync("ScheduleDeleted", "Séance supprimée",
                $"La séance de {schedule.Subject} le {GetDayFrenchName(schedule.Day)} ({schedule.StartTime.ToString(@"hh\:mm")} - {schedule.EndTime.ToString(@"hh\:mm")}) pour {schedule.Level} en {schedule.RoomName} a été supprimée.",
                schedule.Level);
        }

        TempData["Success"] = "Séance supprimée avec succès.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropdowns()
    {
        var rooms = await _roomService.GetAllAsync();
        var teachers = await _teacherService.GetAllAsync();
        ViewBag.RoomId = new SelectList(rooms, "Id", "Name");
        ViewBag.TeacherId = new SelectList(teachers, "Id", "FullName");
        ViewBag.Days = new SelectList(
            Enum.GetValues<DayOfWeek>().Select(d => new { Value = d, Text = GetDayFrenchName(d) }),
            "Value", "Text");
        ViewBag.Levels = new SelectList(Enum.GetValues<StudentLevel>());
    }

    private static string GetDayFrenchName(DayOfWeek day) => day switch
    {
        DayOfWeek.Monday => "Lundi",
        DayOfWeek.Tuesday => "Mardi",
        DayOfWeek.Wednesday => "Mercredi",
        DayOfWeek.Thursday => "Jeudi",
        DayOfWeek.Friday => "Vendredi",
        DayOfWeek.Saturday => "Samedi",
        DayOfWeek.Sunday => "Dimanche",
        _ => day.ToString()
    };

    private async Task NotifyScheduleChangeAsync(string type, string title, string message, StudentLevel level)
    {
        var user = await _userManager.GetUserAsync(User);
        var levelStr = level.ToString();

        await _notificationService.CreateAsync(new CreateNotificationDto
        {
            Title = title,
            Message = message,
            Type = type,
            TargetRole = "Admin",
            CreatedByUserId = user?.Id
        });

        await _notificationService.CreateAsync(new CreateNotificationDto
        {
            Title = title,
            Message = message,
            Type = type,
            TargetRole = "Teacher",
            TargetLevel = levelStr,
            CreatedByUserId = user?.Id
        });

        await _notificationService.CreateAsync(new CreateNotificationDto
        {
            Title = title,
            Message = message,
            Type = type,
            TargetRole = "Student",
            TargetLevel = levelStr,
            CreatedByUserId = user?.Id
        });
    }
}
