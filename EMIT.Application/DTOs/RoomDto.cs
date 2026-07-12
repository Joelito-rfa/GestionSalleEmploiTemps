using System.ComponentModel.DataAnnotations;

namespace EMIT.Application.DTOs;

public class RoomDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Le nom est requis.")]
    [StringLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "La capacité est requise.")]
    [Range(1, 1000, ErrorMessage = "La capacité doit être comprise entre 1 et 1000.")]
    public int Capacity { get; set; }

    [Required(ErrorMessage = "L'emplacement est requis.")]
    [StringLength(200, ErrorMessage = "L'emplacement ne peut pas dépasser 200 caractères.")]
    public string Location { get; set; } = string.Empty;
}

public class CreateRoomDto
{
    [Required(ErrorMessage = "Le nom est requis.")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Range(1, 1000)]
    public int Capacity { get; set; }

    [Required]
    [StringLength(200)]
    public string Location { get; set; } = string.Empty;
}

public class UpdateRoomDto
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Range(1, 1000)]
    public int Capacity { get; set; }

    [Required]
    [StringLength(200)]
    public string Location { get; set; } = string.Empty;
}
