namespace Utilities.Models.Requests.GroupVenues;

public record UpdateGroupVenueRequest
{
    public required string VenueName { get; set; }
    public required bool Visited { get; set; }
    public required Guid FoodTypeOptionId { get; set; }
    public required Guid VenueTypeOptionId { get; set; }
}
