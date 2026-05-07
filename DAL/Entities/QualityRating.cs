using DAL.Entities.Base;

namespace DAL.Entities;

public class QualityRating : AuditableEntity
{
    public Guid QualityRatingId { get; set; }
    public required Guid GroupVenueId { get; set; }
    public required Guid UserId { get; set; }
    public required Guid QualityOptionId { get; set; }

    public GroupVenue? GroupVenue { get; set; }
    public User? User { get; set; }
    public QualityOption? QualityOption { get; set; }
}
