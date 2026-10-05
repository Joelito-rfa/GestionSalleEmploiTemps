using EMIT.Application.Interfaces;
using EMIT.Application.Services;
using EMIT.Domain.Interfaces;
using EMIT.Infrastructure.Data;
using EMIT.Infrastructure.Repositories;
using EMIT.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EMIT.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Neon/Render fournissent une URL (postgresql://user:pass@host/db?sslmode=require)
        // mais Npgsql attend le format cle=valeur : on convertit si besoin.
        var raw = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        var connectionString = ConvertPostgresUrlIfNeeded(raw);

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 6;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Account/AccessDenied";
        });

        services.AddScoped<DbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<ITeacherService, TeacherService>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<IScheduleService, ScheduleService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<ITimetableService, TimetableService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<INotificationService, NotificationService>();

        services.AddScoped<SeedService>();

        services.AddTransient<IEmailSender<ApplicationUser>, EmailSender>();

        return services;
    }

    /// <summary>
    /// Convertit une URL Postgres (postgresql://user:pass@host:port/db?sslmode=require&channel_binding=require)
    /// en connection string Npgsql (Host=...;Port=...;Database=...;Username=...;Password=...;SslMode=Require).
    /// Retourne la valeur telle quelle si ce n'est pas une URL (ex. Host=localhost;... local).
    /// </summary>
    internal static string ConvertPostgresUrlIfNeeded(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        var trimmed = value.Trim();
        if (!trimmed.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)
            && !trimmed.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
            return trimmed;

        var uri = new Uri(trimmed);
        var userInfo = uri.UserInfo.Split(':', 2);
        var username = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
        var database = string.IsNullOrEmpty(uri.AbsolutePath) ? string.Empty : uri.AbsolutePath.TrimStart('/');

        // Query params Neon : sslmode=require, channel_binding=require, etc.
        var sslMode = trimmed.Contains("sslmode=disable", StringComparison.OrdinalIgnoreCase)
            ? "Disable"
            : "Require";

        var parts = new List<string>
        {
            $"Host={uri.Host}",
            $"Database={database}",
            $"Username={username}",
            $"Password={password}",
            $"SslMode={sslMode}"
        };
        if (!uri.IsDefaultPort && uri.Port > 0)
            parts.Add($"Port={uri.Port}");

        return string.Join(";", parts);
    }
}
