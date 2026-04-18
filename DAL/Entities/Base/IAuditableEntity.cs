namespace DAL.Entities.Base;

public interface IAuditableEntity
{
    DateTime CreatedOn { get; set; }
    Guid CreatedBy { get; set; }
    DateTime? UpdatedOn { get; set; }
    Guid? UpdatedBy { get; set; }
}
