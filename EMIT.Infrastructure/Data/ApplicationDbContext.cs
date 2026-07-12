using EMIT.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EMIT.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Schedule>(entity =>
        {
            entity.HasIndex(s => new { s.RoomId, s.Day, s.StartTime, s.EndTime })
                .HasDatabaseName("IX_Schedule_Room_Time")
                .IsUnique();

            entity.HasIndex(s => new { s.TeacherId, s.Day, s.StartTime, s.EndTime })
                .HasDatabaseName("IX_Schedule_Teacher_Time")
                .IsUnique();

            entity.HasOne(s => s.Room)
                .WithMany(r => r.Schedules)
                .HasForeignKey(s => s.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.Teacher)
                .WithMany(t => t.Schedules)
                .HasForeignKey(s => s.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(s => s.Attendances)
                .WithOne(a => a.Schedule)
                .HasForeignKey(a => a.ScheduleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Attendance>(entity =>
        {
            entity.HasOne(a => a.Student)
                .WithMany(s => s.Attendances)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(a => new { a.ScheduleId, a.StudentId, a.Date })
                .HasDatabaseName("IX_Attendance_Unique")
                .IsUnique();
        });

        builder.Entity<Room>(entity =>
        {
            entity.Property(r => r.Name).HasMaxLength(100).IsRequired();
            entity.Property(r => r.Location).HasMaxLength(200).IsRequired();
        });

        builder.Entity<Teacher>(entity =>
        {
            entity.HasIndex(t => t.Numero).IsUnique();
            entity.Property(t => t.FullName).HasMaxLength(150).IsRequired();
            entity.Property(t => t.Email).HasMaxLength(200).IsRequired();
            entity.Property(t => t.Subject).HasMaxLength(200).IsRequired();
        });

        builder.Entity<Student>(entity =>
        {
            entity.HasIndex(s => s.Matricule).IsUnique();
            entity.Property(s => s.FullName).HasMaxLength(150).IsRequired();
            entity.Property(s => s.Email).HasMaxLength(200).IsRequired();
        });

        builder.Entity<UserSession>(entity =>
        {
            entity.HasIndex(s => s.UserId);
            entity.HasIndex(s => s.IsActive);
            entity.Property(s => s.IpAddress).HasMaxLength(45);
        });

        builder.Entity<Notification>(entity =>
        {
            entity.HasIndex(n => new { n.TargetRole, n.IsRead });
            entity.HasIndex(n => n.CreatedAt);
            entity.Property(n => n.Title).HasMaxLength(200).IsRequired();
            entity.Property(n => n.Message).HasMaxLength(500).IsRequired();
            entity.Property(n => n.Type).HasMaxLength(50).IsRequired();
            entity.Property(n => n.TargetRole).HasMaxLength(50);
            entity.Property(n => n.TargetLevel).HasMaxLength(10);
        });
    }
}
