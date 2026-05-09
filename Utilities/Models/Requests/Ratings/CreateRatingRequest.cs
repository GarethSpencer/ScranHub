namespace Utilities.Models.Requests.Ratings;

public record class CreateRatingRequest
{
    public required Guid GroupVenueId { get; set; }
    public required Guid OptionId { get; set; }
}
