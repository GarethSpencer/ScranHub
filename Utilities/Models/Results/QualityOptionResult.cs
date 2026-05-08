using Utilities.Models.Results.Abstractions;

namespace Utilities.Models.Results;

public record QualityOptionResult : IOptionResult
{
    public Guid QualityOptionId { get; init; }
    public Guid? GroupId { get; init; }
    public string Label { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }

    public Guid OptionId
    {
        get
        {
            return QualityOptionId;
        }

        init
        {
            QualityOptionId = value;
        }
    }
}
