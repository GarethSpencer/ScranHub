using DAL.Entities.Base;

namespace DAL.Entities;

public class GroupCost : AuditableEntity
{
    public required Guid GroupCostId { get; set; }
    public required string Costs { get; set; }
}
