using DAL.Entities.Base;

namespace DAL.Entities;

public class UserGroup: AuditableEntity
{
    public Guid UserGroupId { get; set; }
    public required Guid UserId { get; set; }
    public required Guid GroupId { get; set; }

    public User? User { get; set; }
    public Group? Group { get; set; }
}