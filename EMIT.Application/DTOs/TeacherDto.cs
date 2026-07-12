using System.ComponentModel.DataAnnotations;

namespace EMIT.Application.DTOs;

public class TeacherDto
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public int ScheduleCount { get; set; }
}

public class CreateTeacherDto
{
    [Required(ErrorMessage = "Le nom complet est requis.")]
    [StringLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Subject { get; set; } = string.Empty;
}

public class UpdateTeacherDto
{
    public int Id { get; set; }

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
}
