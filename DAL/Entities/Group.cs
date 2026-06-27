using DAL.Entities.Base;

namespace DAL.Entities;

public class Group : AuditableEntity
{
    public Guid GroupId { get; set; }
    public required string GroupName { get; set; }
    public required bool Active { get; set; }
    public string? Icon { get; set; }

    public ICollection<UserGroup> UserGroups { get; set; } = [];
    public ICollection<GroupVenue> GroupVenues { get; set; } = [];
    public ICollection<CostOption> CostOptions { get; set; } = [];
    public ICollection<QualityOption> RatingOptions { get; set; } = [];
    public ICollection<FoodTypeOption> FoodTypeOptions { get; set; } = [];
    public ICollection<VenueTypeOption> VenueTypeOptions { get; set; } = [];
    public User CreatedByUser { get; set; } = null!;
}