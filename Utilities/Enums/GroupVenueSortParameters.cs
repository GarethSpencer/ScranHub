using System.Text.Json.Serialization;

namespace Utilities.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GroupVenueSortParameters
{
    VenueName = 0,
    Visited = 1,
    FoodType = 2,
    VenueType = 3,
    AvgCostRating = 4,
    AvgQualityRating = 5,
    MyCostRating = 6,
    MyQualityRating = 7,
    CostRatingVotes = 8,
    QualityRatingVotes = 9
}
