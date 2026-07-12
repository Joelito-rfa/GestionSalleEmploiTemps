using System.ComponentModel.DataAnnotations;
using EMIT.Domain.Enums;

namespace EMIT.Domain.Entities;

public class Student : BaseEntity
{
    [Required]
    [StringLength(20)]
    public string Matricule { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public StudentLevel Level { get; set; }

    public string? UserId { get; set; }

    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}
