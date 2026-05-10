using DAL.Entities.Abstractions;
using DAL.Entities.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Entities;

public class QualityOption : AuditableEntity, IRatingOption
{
    public Guid QualityOptionId { get; set; }
    public Guid? GroupId { get; set; }
    public string Label { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }

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

    public Group? Group { get; set; }
    public ICollection<QualityRating> QualityRatings { get; set; } = [];
}