namespace Utilities.Models.Requests.GroupVenues;

public record UpdateGroupVenueRequest
{
    public required string VenueName { get; set; }
    public required bool Visited { get; set; }
    public Guid? FoodTypeOptionId { get; set; } = null;
    public Guid? VenueTypeOptionId { get; set; } = null;
}
