using Utilities.Models.Results.Abstractions;

namespace Utilities.Models.Results;

public record FoodTypeOptionResult : IOptionResult
{
    public Guid FoodTypeOptionId { get; init; }
    public Guid? GroupId { get; init; }
    public string Label { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }

    public Guid OptionId
    {
        get
        {
            return FoodTypeOptionId;
        }

        init
        {
            FoodTypeOptionId = value;
        }
    }
}
