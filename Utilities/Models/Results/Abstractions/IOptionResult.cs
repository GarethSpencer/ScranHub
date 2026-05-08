namespace Utilities.Models.Results.Abstractions;

public interface IOptionResult
{
    public Guid OptionId { get; init; }
    public Guid? GroupId { get; init; }
    public string Label { get; init; }
    public int DisplayOrder { get; init; }
}
