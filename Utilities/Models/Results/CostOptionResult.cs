using Utilities.Models.Results.Abstractions;

namespace Utilities.Models.Results;

public record CostOptionResult: IOptionResult
{
    public Guid CostOptionId { get; init; }
    public Guid? GroupId { get; init; }
    public string Label { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }
    public Guid OptionId
    {
        get
        {
            return CostOptionId;
        }

        init
        {
            CostOptionId = value;
        }
    }
}
