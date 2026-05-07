namespace Utilities.Models.Requests.QualityRatings;

public record class CreateQualityRatingRequest
{
    public required Guid GroupVenueId { get; set; }
    public required Guid QualityOptionId { get; set; }
}
