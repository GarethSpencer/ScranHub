namespace Utilities.Models.Requests.CostRatings;

public record class CreateCostRatingRequest
{
    public required Guid GroupVenueId { get; set; }
    public required Guid CostOptionId { get; set; }
}
