namespace DAL.Entities.Base;

public abstract class AuditableEntity : IAuditableEntity
{
    public DateTime CreatedOn { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public Guid? UpdatedBy { get; set; }
}