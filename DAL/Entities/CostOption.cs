using DAL.Entities.Abstractions;
using DAL.Entities.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Entities;

public class CostOption : AuditableEntity, IRatingOption
{
    public Guid CostOptionId { get; set; }
    public Guid? GroupId { get; set; }
    public string Label { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }

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

    public Group? Group { get; set; }
    public ICollection<CostRating> CostRatings { get; set; } = [];
}