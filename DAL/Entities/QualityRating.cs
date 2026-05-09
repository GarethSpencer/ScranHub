using DAL.Entities.Abstractions;
using DAL.Entities.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Entities;

public class QualityRating : AuditableEntity, IRating
{
    public Guid QualityRatingId { get; set; }
    public Guid GroupVenueId { get; set; }
    public Guid UserId { get; set; }
    public Guid QualityOptionId { get; set; }

    [NotMapped]
    public Guid OptionId
    {
        get
        {
            return QualityOptionId;
        }

        set
        {
            QualityOptionId = value;
        }
    }

    [NotMapped]
    public Guid RatingId
    {
        get
        {
            return QualityRatingId;
        }

        set
        {
            QualityRatingId = value;
        }
    }

    public GroupVenue? GroupVenue { get; set; }
    public User? User { get; set; }
    public QualityOption? QualityOption { get; set; }
}
