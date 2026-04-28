using DAL.Entities.Base;

namespace DAL.Entities;

public class RatingUserRating : AuditableEntity
{
    public Guid RatingUserRatingId { get; set; }
    public required Guid GroupVenueId { get; set; }
    public required Guid UserId { get; set; }
    public required Guid RatingOptionId { get; set; }

    public GroupVenue? GroupVenue { get; set; }
    public User? User { get; set; }
    public RatingOption? RatingOption { get; set; }
}
