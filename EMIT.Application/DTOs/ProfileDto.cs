using System.ComponentModel.DataAnnotations;

namespace EMIT.Application.DTOs;

public class ProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Role { get; set; }
    public string? ProfilePicturePath { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalSchedules { get; set; }
    public int TotalAttendances { get; set; }
}

public class UpdateProfileDto
{
    [Required(ErrorMessage = "Le prénom est requis.")]
    [StringLength(75)]
    [Display(Name = "Prénom")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le nom est requis.")]
    [StringLength(75)]
    [Display(Name = "Nom")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    public string? ProfilePicturePath { get; set; }
}

public class UserListItemDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsLockedOut { get; set; }
}
