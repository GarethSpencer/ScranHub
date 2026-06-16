using System.Text.Json.Serialization;

namespace Utilities.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FriendshipStatus
{
    Pending = 0,
    Accepted = 1,
    Declined = 2,
}
