namespace DAL.Entities.Abstractions;

public interface IOption
{
    public Guid OptionId { get; set; }
    public Guid? GroupId { get; set; }
    public string Label { get; set; }
    public int DisplayOrder { get; set; }
}
