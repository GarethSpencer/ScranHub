namespace Utilities.Models.Requests.CostRatings;

public record class UpdateCostRatingRequest
{
    public required Guid CostOptionId { get; set; }
}
