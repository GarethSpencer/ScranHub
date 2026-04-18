using DAL.Entities.Base;

namespace DAL.Entities;

public class GroupRatingOption : AuditableEntity
{
    public required Guid GroupRatingOptionId { get; set; }
    public Guid? GroupId { get; set; }
    public required string Label { get; set; }
    public required int DisplayOrder { get; set; }

    public Group? Group { get; set; }
}
