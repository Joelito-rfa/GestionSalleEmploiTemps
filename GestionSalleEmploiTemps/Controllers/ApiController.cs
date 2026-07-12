using Microsoft.AspNetCore.Mvc;
using EMIT.Application.Interfaces;
using EMIT.Domain.Enums;

namespace GestionSalleEmploiTemps.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ApiController : ControllerBase
{
    private readonly ITimetableService _timetableService;
    private readonly IDashboardService _dashboardService;

    public ApiController(ITimetableService timetableService, IDashboardService dashboardService)
    {
        _timetableService = timetableService;
        _dashboardService = dashboardService;
    }

    [HttpGet("timetable")]
    public async Task<IActionResult> GetTimetable([FromQuery] StudentLevel? level)
    {
        var schedules = await _timetableService.GetTimetableAsync(level);
        return Ok(schedules);
    }

    [HttpGet("timetable/search")]
    public async Task<IActionResult> SearchTimetable(
        [FromQuery] string? searchTerm,
        [FromQuery] StudentLevel? level,
        [FromQuery] DayOfWeek? day)
    {
        var schedules = await _timetableService.SearchAsync(searchTerm, level, day);
        return Ok(schedules);
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var dashboard = await _dashboardService.GetDashboardAsync();
        return Ok(dashboard);
    }
}
