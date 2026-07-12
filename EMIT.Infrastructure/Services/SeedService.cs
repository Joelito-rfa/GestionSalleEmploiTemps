using EMIT.Domain.Entities;
using EMIT.Domain.Enums;
using EMIT.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EMIT.Infrastructure.Services;

public class SeedService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<SeedService> _logger;

    public SeedService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<SeedService> logger)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        await _context.Database.MigrateAsync();

        await SeedRolesAsync();
        await SeedAdminAsync();
        await SeedRoomsAsync();
        await SeedTeachersAsync();
        await SeedStudentsAsync();
        await SeedSchedulesAsync();
        await SeedUserAccountsAsync();
    }

    private async Task SeedRolesAsync()
    {
        var roles = new[] { "Admin", "Teacher", "Student" };
        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
                _logger.LogInformation("Role created: {Role}", role);
            }
        }
    }

    private async Task SeedAdminAsync()
    {
        if (await _userManager.FindByEmailAsync("admin@emit.com") == null)
        {
            var admin = new ApplicationUser
            {
                UserName = "admin@emit.com",
                Email = "admin@emit.com",
                FullName = "Administrateur EMIT",
                Role = "Admin"
            };
            var result = await _userManager.CreateAsync(admin, "Admin@123");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(admin, "Admin");
                _logger.LogInformation("Admin user created");
            }
        }
    }

    private async Task SeedRoomsAsync()
    {
        if (!await _context.Rooms.AnyAsync())
        {
            var rooms = new List<Room>
            {
                new() { Name = "Amphi A", Capacity = 200, Location = "Bâtiment Principal, RDC" },
                new() { Name = "Salle 101", Capacity = 30, Location = "Bâtiment A, 1er étage" },
                new() { Name = "Salle 102", Capacity = 25, Location = "Bâtiment A, 1er étage" },
                new() { Name = "Salle 201", Capacity = 35, Location = "Bâtiment A, 2ème étage" },
                new() { Name = "Labo Info", Capacity = 20, Location = "Bâtiment B, RDC" },
                new() { Name = "Salle TD 1", Capacity = 40, Location = "Bâtiment B, 1er étage" }
            };
            _context.Rooms.AddRange(rooms);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Rooms seeded: {Count}", rooms.Count);
        }
    }

    private async Task SeedTeachersAsync()
    {
        if (!await _context.Teachers.AnyAsync())
        {
            var year = DateTime.UtcNow.Year;
            var teachers = new List<Teacher>
            {
                new() { Numero = $"PROF-{year}-001", FullName = "Dr. Ahmed Benali", Email = "a.benali@emit.tn", Subject = "Mathématiques" },
                new() { Numero = $"PROF-{year}-002", FullName = "Dr. Sarra Karray", Email = "s.karray@emit.tn", Subject = "Informatique" },
                new() { Numero = $"PROF-{year}-003", FullName = "M. Karim Jellali", Email = "k.jellali@emit.tn", Subject = "Réseaux" },
                new() { Numero = $"PROF-{year}-004", FullName = "Dr. Ines Trabelsi", Email = "i.trabelsi@emit.tn", Subject = "Algorithmique" },
                new() { Numero = $"PROF-{year}-005", FullName = "M. Mohamed Sassi", Email = "m.sassi@emit.tn", Subject = "Base de données" },
                new() { Numero = $"PROF-{year}-006", FullName = "Dr. Leila Mansour", Email = "l.mansour@emit.tn", Subject = "Anglais Technique" }
            };
            _context.Teachers.AddRange(teachers);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Teachers seeded: {Count}", teachers.Count);
        }
    }

    private async Task SeedStudentsAsync()
    {
        if (!await _context.Students.AnyAsync())
        {
            var year = DateTime.UtcNow.Year;
            var levels = Enum.GetValues<StudentLevel>();
            var students = new List<Student>();
            var count = 1;

            foreach (var level in levels)
            {
                for (int i = 1; i <= 5; i++)
                {
                    students.Add(new Student
                    {
                        Matricule = $"STU-{year}-{count:D3}",
                        FullName = $"Étudiant {level} - {i}",
                        Email = $"student.{level.ToString().ToLower()}.{i}@emit.tn",
                        Level = level
                    });
                    count++;
                }
            }

            _context.Students.AddRange(students);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Students seeded: {Count}", students.Count);
        }
    }

    private async Task SeedSchedulesAsync()
    {
        if (!await _context.Schedules.AnyAsync())
        {
            var rooms = await _context.Rooms.ToListAsync();
            var teachers = await _context.Teachers.ToListAsync();

            var schedules = new List<Schedule>();
            var days = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };
            var levels = Enum.GetValues<StudentLevel>();
            var times = new[] { new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0), new TimeSpan(14, 0, 0), new TimeSpan(16, 0, 0) };

            int offset = 0;
            foreach (var day in days)
            {
                foreach (var time in times)
                {
                    for (int li = 0; li < levels.Length; li++)
                    {
                        var room = rooms[(li + offset) % rooms.Count];
                        var teacher = teachers[(li + offset + 1) % teachers.Count];

                        schedules.Add(new Schedule
                        {
                            Day = day,
                            StartTime = time,
                            EndTime = time.Add(new TimeSpan(1, 45, 0)),
                            Level = levels[li],
                            RoomId = room.Id,
                            TeacherId = teacher.Id
                        });
                    }
                    offset++;
                }
            }

            _context.Schedules.AddRange(schedules);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Schedules seeded: {Count}", schedules.Count);
        }
    }

    private async Task SeedUserAccountsAsync()
    {
        var teachers = await _context.Teachers.ToListAsync();
        foreach (var teacher in teachers)
        {
            if (string.IsNullOrEmpty(teacher.UserId))
            {
                if (await _userManager.FindByEmailAsync(teacher.Email) != null)
                {
                    var existing = await _userManager.FindByEmailAsync(teacher.Email);
                    if (existing != null)
                    {
                        await _userManager.DeleteAsync(existing);
                        _logger.LogInformation("Deleted old teacher account: {Email}", teacher.Email);
                    }
                }

                var user = new ApplicationUser
                {
                    UserName = teacher.Email,
                    Email = teacher.Email,
                    FullName = teacher.FullName,
                    Role = "Teacher"
                };
                var result = await _userManager.CreateAsync(user, "Teacher@123");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Teacher");
                    teacher.UserId = user.Id;
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Teacher account created: {Email} (UserId: {UserId})", teacher.Email, user.Id);
                }
                else
                {
                    _logger.LogWarning("Failed to create teacher account for {Email}: {Errors}",
                        teacher.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }

        var students = await _context.Students.ToListAsync();
        foreach (var student in students)
        {
            if (string.IsNullOrEmpty(student.UserId))
            {
                if (await _userManager.FindByEmailAsync(student.Email) != null)
                {
                    var existing = await _userManager.FindByEmailAsync(student.Email);
                    if (existing != null)
                    {
                        await _userManager.DeleteAsync(existing);
                        _logger.LogInformation("Deleted old student account: {Email}", student.Email);
                    }
                }

                var user = new ApplicationUser
                {
                    UserName = student.Email,
                    Email = student.Email,
                    FullName = student.FullName,
                    Role = "Student"
                };
                var result = await _userManager.CreateAsync(user, "Student@123");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Student");
                    student.UserId = user.Id;
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Student account created: {Email} (UserId: {UserId})", student.Email, user.Id);
                }
                else
                {
                    _logger.LogWarning("Failed to create student account for {Email}: {Errors}",
                        student.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}
