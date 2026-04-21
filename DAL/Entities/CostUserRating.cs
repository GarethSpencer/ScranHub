using DAL.Entities.Base;

namespace DAL.Entities;

public class CostUserRating : AuditableEntity
{
    public required Guid CostUserRatingId { get; set; }
    public required Guid GroupVenueId { get; set; }
    public required Guid UserId { get; set; }
    public required Guid CostOptionId { get; set; }

    public GroupVenue GroupVenue { get; set; } = null!;
    public User User { get; set; } = null!;
    public CostOption CostOption { get; set; } = null!;
}
