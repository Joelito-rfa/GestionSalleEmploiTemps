using System.ComponentModel.DataAnnotations;

namespace EMIT.Domain.Entities;

public class Room : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Range(1, 1000)]
    public int Capacity { get; set; }

    [Required]
    [StringLength(200)]
    public string Location { get; set; } = string.Empty;

    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}
