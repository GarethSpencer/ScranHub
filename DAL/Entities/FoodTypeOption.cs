using DAL.Entities.Abstractions;
using DAL.Entities.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Entities;

public class FoodTypeOption : AuditableEntity, ITypeOption
{
    public Guid FoodTypeOptionId { get; set; }
    public Guid? GroupId { get; set; }
    public string Label { get; set; } = string.Empty;

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