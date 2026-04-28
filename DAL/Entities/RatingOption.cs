using DAL.Entities.Base;

namespace DAL.Entities;

public class RatingOption : AuditableEntity
{
    public Guid RatingOptionId { get; set; }
    public Guid? GroupId { get; set; }
    public required string Label { get; set; }
    public required int DisplayOrder { get; set; }

    public Group? Group { get; set; }
    public ICollection<RatingUserRating> RatingUserRatings { get; set; } = [];
}