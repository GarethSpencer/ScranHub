namespace Utilities.Helpers;

public static class RegexConstants
{
    public const string AlphanumericPlus = @"^[\w\s\-'&.!?]+$";

    public const string GooglePlaceId = @"^[A-Za-z0-9_-]+$";

    public const string FormattedAddress = @"^[^<>{}\\]*$";
}
