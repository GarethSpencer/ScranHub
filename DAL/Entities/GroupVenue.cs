using DAL.Entities.Base;

namespace DAL.Entities;

public class GroupVenue : AuditableEntity
{
    public Guid GroupVenueId { get; set; }
    public required Guid GroupId { get; set; }
    public required string VenueName { get; set; }
    public required bool Visited { get; set; }
    public Guid? FoodTypeOptionId { get; set; }
    public Guid? VenueTypeOptionId { get; set; }

    public Group? Group { get; set; }
    public ICollection<CostUserRating> CostUserRatings { get; set; } = [];
    public ICollection<RatingUserRating> RatingUserRatings { get; set; } = [];
    public FoodTypeOption? FoodTypeOption { get; set; }
    public VenueTypeOption? VenueTypeOption { get; set; }
}