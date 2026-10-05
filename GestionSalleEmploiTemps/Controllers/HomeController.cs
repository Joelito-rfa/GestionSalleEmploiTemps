using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EMIT.Application.Interfaces;
using EMIT.Domain.Enums;
using EMIT.Application.DTOs;
using GestionSalleEmploiTemps.Models;

namespace GestionSalleEmploiTemps.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ITimetableService _timetableService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ITimetableService timetableService, ILogger<HomeController> logger)
    {
        _timetableService = timetableService;
        _logger = logger;
    }

    public async Task<IActionResult> Timetable(StudentLevel? level)
    {
        var schedules = await _timetableService.GetTimetableAsync(level);
        var grouped = schedules
            .GroupBy(s => s.Level)
            .Select(g => new LevelTimetableViewModel
            {
                Level = g.Key,
                Days = g.GroupBy(s => s.Day)
                    .Select(d => new DayScheduleViewModel
                    {
                        Day = d.Key,
                        Schedules = d.OrderBy(s => s.StartTime)
                            .Select(s => new ScheduleViewModel
                            {
                                Id = s.Id,
                                StartTime = s.StartTime,
                                EndTime = s.EndTime,
                                RoomName = s.RoomName,
                                TeacherName = s.TeacherName,
                                Subject = s.Subject
                            }).ToList()
                    })
                    .OrderBy(d => d.Day)
                    .ToList()
            })
            .OrderBy(g => g.Level)
            .ToList();

        ViewBag.Levels = Enum.GetValues<StudentLevel>().Select(l => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = l.ToString(),
            Text = l.ToString(),
            Selected = level.HasValue && level.Value == l
        }).ToList();
        ViewBag.CurrentLevel = level?.ToString();

        return View(grouped);
    }

    public async Task<IActionResult> ExportPdf(StudentLevel? level)
    {
        var schedules = await _timetableService.GetTimetableAsync(level);
        return View(schedules);
    }

    public IActionResult Privacy() => View();

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
