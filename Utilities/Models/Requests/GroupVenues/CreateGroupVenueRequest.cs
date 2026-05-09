namespace Utilities.Models.Requests.GroupVenues;

public record CreateGroupVenueRequest
{
    public required string VenueName { get; set; }
    public required Guid GroupId { get; set; }
    public Guid? FoodTypeOptionId { get; set; } = null;
    public Guid? VenueTypeOptionId { get; set; } = null;
}
