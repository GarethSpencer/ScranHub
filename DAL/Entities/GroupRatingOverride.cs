using DAL.Entities.Base;

namespace DAL.Entities;

public class GroupRatingOverride : AuditableEntity
{
    public required Guid GroupRatingOverrideId { get; set; }
    public required Guid GroupId {  get; set; }
    public required string Ratings { get; set; }

    public required Group Group { get; set; } = null!;
}