using DAL.Entities.Abstractions;
using DAL.Entities.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Entities;

public class VenueTypeOption : AuditableEntity, ITypeOption
{
    public Guid VenueTypeOptionId { get; set; }
    public Guid? GroupId { get; set; }
    public string Label { get; set; } = string.Empty;

    [NotMapped]
    public Guid TypeOptionId
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