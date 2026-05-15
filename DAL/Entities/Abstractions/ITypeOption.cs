namespace DAL.Entities.Abstractions;

public interface ITypeOption
{
    public Guid OptionId { get; set; }
    public Guid? GroupId { get; set; }
    public string Label { get; set; }
}
