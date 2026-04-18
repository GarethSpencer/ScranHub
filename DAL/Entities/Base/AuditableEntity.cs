namespace DAL.Entities.Base;

public abstract class AuditableEntity
{
    public required DateTime CreatedOn { get; set; }
    public required Guid CreatedBy { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public Guid? UpdatedBy { get; set; }
}