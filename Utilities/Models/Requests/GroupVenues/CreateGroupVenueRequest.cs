namespace Utilities.Models.Requests.GroupVenues;

public record CreateGroupVenueRequest
{
    public required string VenueName { get; set; }
    public required Guid GroupId { get; set; }
    public required Guid FoodTypeOptionId { get; set; }
    public required Guid VenueTypeOptionId { get; set; }
}
