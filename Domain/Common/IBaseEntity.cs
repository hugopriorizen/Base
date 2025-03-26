namespace Domain.Common;

public interface IBaseEntity
{
    DateTimeOffset CreatedAt { get; set; }
    DateTimeOffset? DeletedAt { get; set; }
    bool IsActive { get; set; }
}
