namespace EMIT.Domain.Entities;

public class UserSession
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime LoginAt { get; set; } = DateTime.UtcNow;
    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
    public bool IsActive { get; set; } = true;
}
