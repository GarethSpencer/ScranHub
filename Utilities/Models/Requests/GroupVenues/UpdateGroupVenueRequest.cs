namespace Utilities.Models.Requests.GroupVenues;

public record UpdateGroupVenueRequest
{
    public required string VenueName { get; set; }
    public required bool Visited { get; set; }
    public Guid? FoodTypeOptionId { get; set; } = null;
    public Guid? VenueTypeOptionId { get; set; } = null;
    public string? GooglePlaceId { get; set; }
    public string? FormattedAddress { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}
