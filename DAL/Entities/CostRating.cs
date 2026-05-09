using DAL.Entities.Abstractions;
using DAL.Entities.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Entities;

public class CostRating : AuditableEntity, IRating
{
    public Guid CostRatingId { get; set; }
    public Guid GroupVenueId { get; set; }
    public Guid UserId { get; set; }
    public Guid CostOptionId { get; set; }

    [NotMapped]
    public Guid OptionId
    {
        get
        {
            return CostOptionId;
        }

        set
        {
            CostOptionId = value;
        }
    }

    [NotMapped]
    public Guid RatingId
    {
        get
        {
            return CostRatingId;
        }

        set
        {
            CostRatingId = value;
        }
    }

    public GroupVenue? GroupVenue { get; set; }
    public User? User { get; set; }
    public CostOption? CostOption { get; set; }
}
