using DAL.Entities.Base;

namespace DAL.Entities;

public class UserGroup: AuditableEntity
{
    public required Guid UserGroupId { get; set; }
    public required Guid UserId { get; set; }
    public required Guid GroupId { get; set; }

    public required User User { get; set; } = null!;
    public required Group Group { get; set; } = null!;
}