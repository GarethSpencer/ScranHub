using DAL.Entities.Abstractions;
using DAL.Entities.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Entities;

public class FoodTypeOption : AuditableEntity, IOption
{
    public Guid FoodTypeOptionId { get; set; }
    public Guid? GroupId { get; set; }
    public required string Label { get; set; }
    public required int DisplayOrder { get; set; }

    [NotMapped]
    public Guid OptionId
    {
        get
        {
            return FoodTypeOptionId;
        }

        set
        {
            FoodTypeOptionId = value;
        }
    }

    public Group? Group { get; set; }
    public ICollection<GroupVenue> GroupVenues { get; set; } = [];
}