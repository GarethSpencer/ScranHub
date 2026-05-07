using DAL.Entities.Base;

namespace DAL.Entities;

public class CostRating : AuditableEntity
{
    public Guid CostRatingId { get; set; }
    public required Guid GroupVenueId { get; set; }
    public required Guid UserId { get; set; }
    public required Guid CostOptionId { get; set; }

    public GroupVenue? GroupVenue { get; set; }
    public User? User { get; set; }
    public CostOption? CostOption { get; set; }
}
