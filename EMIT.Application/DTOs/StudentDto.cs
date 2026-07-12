using System.ComponentModel.DataAnnotations;
using EMIT.Domain.Enums;

namespace EMIT.Application.DTOs;

public class StudentDto
{
    public int Id { get; set; }
    public string Matricule { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public StudentLevel Level { get; set; }
    public string? UserId { get; set; }
}

public class CreateStudentDto
{
    [Required(ErrorMessage = "Le nom complet est requis.")]
    [StringLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public StudentLevel Level { get; set; }
}

public class UpdateStudentDto
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
    public StudentLevel Level { get; set; }
}
