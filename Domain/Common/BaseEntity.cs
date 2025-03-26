namespace Domain.Common;

public abstract class BaseEntity
{
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? DeletedAt { get; set; }
    public bool IsActive { get; set; } = true;
}
