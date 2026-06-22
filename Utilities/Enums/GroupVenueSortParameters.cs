using System.Text.Json.Serialization;

namespace Utilities.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GroupVenueSortParameters
{
    VenueName = 0,
    Visited = 1,
    FoodType = 2,
    VenueType = 3,
}
