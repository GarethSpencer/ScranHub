namespace Utilities.Models.Requests.Ratings;

public record class UpdateRatingRequest
{
    public required Guid OptionId { get; set; }
}
