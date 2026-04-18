using DAL.Entities.Base;

namespace DAL.Entities;

public class GroupRating : AuditableEntity
{
    public required Guid GroupRatingId { get; set; }
    public required string Ratings { get; set; }
}