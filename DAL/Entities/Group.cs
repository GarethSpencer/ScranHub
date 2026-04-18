using DAL.Entities.Base;

namespace DAL.Entities;

public class Group : AuditableEntity
{
    public required Guid GroupId { get; set; }
    public required string GroupName { get; set; }
    public required bool Active { get; set; }

    public ICollection<UserGroup> UserGroups { get; set; } = [];
    public ICollection<GroupVenue> GroupVenues { get; set; } = [];
    public ICollection<GroupCostOption> GroupCostOptions { get; set; } = [];
    public ICollection<GroupRatingOption> GroupRatingOptions { get; set; } = [];
}