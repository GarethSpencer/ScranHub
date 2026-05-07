namespace Utilities.Models.Requests.QualityRatings;

public record class UpdateQualityRatingRequest
{
    public required Guid QualityOptionId { get; set; }
}
