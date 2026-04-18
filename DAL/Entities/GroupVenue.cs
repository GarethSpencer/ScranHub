using DAL.Entities.Base;

namespace DAL.Entities;

public class GroupVenue : AuditableEntity
{
    public required Guid GroupVenueId { get; set; }
    public required Guid GroupId { get; set; }
    public required string VenueName { get; set; }
    public required bool Visited { get; set; }
    public Guid? CostOptionId { get; set; }
    public Guid? RatingOptionId { get; set; }
    public Guid? FoodTypeOptionId { get; set; }
    public Guid? VenueTypeOptionId { get; set; }

    public required Group Group { get; set; } = null!;
    public CostOption? CostOption { get; set; }
    public RatingOption? RatingOption { get; set; }
    public FoodTypeOption? FoodTypeOption { get; set; }
    public VenueTypeOption? VenueTypeOption { get; set; }
}