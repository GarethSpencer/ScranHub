namespace DAL.Entities.Abstractions;

public interface IRatingOption
{
    public Guid OptionId { get; set; }
    public Guid? GroupId { get; set; }
    public string Label { get; set; }
    public int DisplayOrder { get; set; }
}
