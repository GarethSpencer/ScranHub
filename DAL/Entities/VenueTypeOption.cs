using DAL.Entities.Abstractions;
using DAL.Entities.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Entities;

public class VenueTypeOption : AuditableEntity, IOption
{
    public Guid VenueTypeOptionId { get; set; }
    public Guid? GroupId { get; set; }
    public required string Label { get; set; }
    public required int DisplayOrder { get; set; }

    [NotMapped]
    public Guid OptionId
    {
        get
        {
            return VenueTypeOptionId;
        }

        set
        {
            VenueTypeOptionId = value;
        }
    }

    public Group? Group { get; set; }
    public ICollection<GroupVenue> GroupVenues { get; set; } = [];
}