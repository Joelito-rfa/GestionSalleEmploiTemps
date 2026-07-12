using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using EMIT.Application.DTOs;
using EMIT.Application.Interfaces;
using EMIT.Domain.Entities;
using EMIT.Domain.Enums;
using EMIT.Domain.Interfaces;

namespace GestionSalleEmploiTemps.Controllers;

[Authorize(Roles = "Admin,Teacher")]
public class AttendanceController : Controller
{
    private readonly IAttendanceService _attendanceService;
    private readonly IScheduleService _scheduleService;
    private readonly IStudentService _studentService;
    private readonly IRepository<Student> _studentRepo;
    private readonly ILogger<AttendanceController> _logger;

    public AttendanceController(
        IAttendanceService attendanceService,
        IScheduleService scheduleService,
        IStudentService studentService,
        IUnitOfWork unitOfWork,
        ILogger<AttendanceController> logger)
    {
        _attendanceService = attendanceService;
        _scheduleService = scheduleService;
        _studentService = studentService;
        _studentRepo = unitOfWork.Repository<Student>();
        _logger = logger;
    }

    public async Task<IActionResult> Index(int? scheduleId, int? studentId, StudentLevel? level)
    {
        if (scheduleId.HasValue)
        {
            var records = await _attendanceService.GetByScheduleAsync(scheduleId.Value);
            var schedule = await _scheduleService.GetByIdAsync(scheduleId.Value);
            ViewBag.Schedule = schedule;
            ViewBag.Levels = GetLevelSelectList(null);
            return View(records);
        }
        if (studentId.HasValue)
        {
            var records = await _attendanceService.GetByStudentAsync(studentId.Value);
            ViewBag.Levels = GetLevelSelectList(null);
            return View(records);
        }

        var students = level.HasValue
            ? await _studentService.GetByLevelAsync(level.Value)
            : await _studentService.GetAllAsync();

        var todayRecords = await _attendanceService.GetTodayAllAsync();
        var todayDict = todayRecords.ToDictionary(r => r.StudentId);

        var result = new List<AttendanceDto>();
        foreach (var s in students)
        {
            if (todayDict.TryGetValue(s.Id, out var record))
            {
                record.StudentName = s.FullName;
                result.Add(record);
            }
            else
            {
                result.Add(new AttendanceDto
                {
                    StudentId = s.Id,
                    StudentName = s.FullName,
                    IsPresent = false,
                    Date = DateTime.UtcNow,
                    IsNotMarked = true
                });
            }
        }

        ViewBag.SelectedLevel = level;
        ViewBag.Levels = GetLevelSelectList(level);
        return View(result);
    }

    private static List<SelectListItem> GetLevelSelectList(StudentLevel? selected)
    {
        return Enum.GetValues<StudentLevel>().Select(l => new SelectListItem
        {
            Value = l.ToString(),
            Text = l.ToString(),
            Selected = selected.HasValue && selected.Value == l
        }).ToList();
    }

    public async Task<IActionResult> Mark(int scheduleId)
    {
        var schedule = await _scheduleService.GetByIdAsync(scheduleId);
        if (schedule == null) return NotFound();

        var students = await _studentService.GetAllAsync();
        var existing = await _attendanceService.GetByScheduleAsync(scheduleId);

        var bulkDto = new BulkAttendanceDto
        {
            ScheduleId = scheduleId,
            Date = DateTime.UtcNow
        };

        var studentNames = new Dictionary<int, string>();
        foreach (var student in students)
        {
            var existingRecord = existing.FirstOrDefault(e => e.StudentId == student.Id);
            bulkDto.Students.Add(new StudentAttendanceItem
            {
                StudentId = student.Id,
                IsPresent = existingRecord?.IsPresent ?? false
            });
            studentNames[student.Id] = student.FullName;
        }

        ViewBag.Schedule = schedule;
        ViewBag.StudentNames = studentNames;
        return View(bulkDto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Mark(BulkAttendanceDto dto)
    {
        dto.Date = DateTime.SpecifyKind(dto.Date, DateTimeKind.Utc);
        await _attendanceService.MarkBulkAsync(dto);
        TempData["Success"] = "Présences enregistrées.";
        _logger.LogInformation("Attendance marked for schedule {ScheduleId}", dto.ScheduleId);
        return RedirectToAction(nameof(Index), new { scheduleId = dto.ScheduleId });
    }

    public async Task<IActionResult> SelectSchedule(StudentLevel? level)
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
}
