using DAL.Entities.Base;

namespace DAL.Entities;

public class FoodTypeOption : AuditableEntity
{
    public Guid FoodTypeOptionId { get; set; }
    public Guid? GroupId { get; set; }
    public required string Label { get; set; }
    public required int DisplayOrder { get; set; }

    public Group? Group { get; set; }
    public ICollection<GroupVenue> GroupVenues { get; set; } = [];
}