using DAL.Entities.Base;

namespace DAL.Entities;

public class GroupCostOption : AuditableEntity
{
    public required Guid GroupCostOptionId { get; set; }
    public Guid? GroupId { get; set; }
    public required string Label { get; set; }
    public required int DisplayOrder { get; set; }

    public Group? Group { get; set; }
}
