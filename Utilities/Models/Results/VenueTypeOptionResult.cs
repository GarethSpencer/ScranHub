using Utilities.Models.Results.Abstractions;

namespace Utilities.Models.Results;

public record VenueTypeOptionResult : IOptionResult
{
    public Guid VenueTypeOptionId { get; init; }
    public Guid? GroupId { get; init; }
    public string Label { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }

    public Guid OptionId
    {
        get
        {
            return VenueTypeOptionId;
        }

        init
        {
            VenueTypeOptionId = value;
        }
    }
}
