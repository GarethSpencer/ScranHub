namespace Utilities.Models.Requests.CostUserRatings;

public record class CreateCostUserRatingRequest
{
    public required Guid GroupVenueId { get; set; }
    public required Guid CostOptionId { get; set; }
}
