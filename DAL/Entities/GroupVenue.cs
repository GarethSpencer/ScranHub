using DAL.Entities.Base;

namespace DAL.Entities;

public class GroupVenue : AuditableEntity
{
    public required Guid GroupVenueId { get; set; }
    public required Guid GroupId { get; set; }
    public required string VenueName { get; set; }
    public required bool Visited { get; set; }
    public int? VenueTypeId { get; set; }
    public int? FoodTypeId { get; set; }
    public Guid? GroupRatingOptionId { get; set; }
    public Guid? GroupCostOptionId { get; set; }

    public required Group Group { get; set; } = null!;
    public GroupRatingOption? GroupRatingOption { get; set; }
    public GroupCostOption? GroupCostOption { get; set; }
}