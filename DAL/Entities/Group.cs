using DAL.Entities.Base;

namespace DAL.Entities;

public class Group : AuditableEntity
{
    public required Guid GroupId { get; set; }
    public required string GroupName { get; set; }
    public required bool Active { get; set; }

    public ICollection<UserGroup> UserGroups { get; set; } = [];
    public GroupCostOverride GroupCostOverride { get; set; } = null!;
    public GroupRatingOverride GroupRatingOverride { get; set; } = null!;
}