using System.ComponentModel.DataAnnotations;

namespace EMIT.Domain.Entities;

public class Teacher : BaseEntity
{
    [Required]
    [StringLength(20)]
    public string Numero { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Subject { get; set; } = string.Empty;

    public string? UserId { get; set; }

    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}
