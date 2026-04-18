using DAL.Entities.Base;

namespace DAL.Entities;

public class GroupCostOverride : AuditableEntity
{
    public required Guid GroupCostOverrideId { get; set; }
    public required Guid GroupId {  get; set; }
    public required string Costs { get; set; }

    public required Group Group { get; set; } = null!;
}