using DAL.Entities.Base;

namespace DAL.Entities;

public class RatingUserRating : AuditableEntity
{
    public required Guid RatingUserRatingId { get; set; }
    public required Guid GroupVenueId { get; set; }
    public required Guid UserId { get; set; }
    public required Guid RatingOptionId { get; set; }

    public GroupVenue GroupVenue { get; set; } = null!;
    public User User { get; set; } = null!;
    public RatingOption RatingOption { get; set; } = null!;
}
