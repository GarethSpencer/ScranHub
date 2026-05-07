using DAL.Entities.Base;

namespace DAL.Entities;

public class QualityOption : AuditableEntity
{
    public Guid QualityOptionId { get; set; }
    public Guid? GroupId { get; set; }
    public required string Label { get; set; }
    public required int DisplayOrder { get; set; }

    public Group? Group { get; set; }
    public ICollection<QualityRating> QualityRatings { get; set; } = [];
}