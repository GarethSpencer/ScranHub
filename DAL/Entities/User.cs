using DAL.Entities.Base;

namespace DAL.Entities;

public class User : AuditableEntity
{
    public required Guid UserId { get; set; }
    public Guid AuthId { get; set; }
    public required string DisplayName { get; set; }
    public required bool Active { get; set; }
    public required bool Admin { get; set; }

    public ICollection<UserFriend> InitiatedFriendships { get; set; } = [];
    public ICollection<UserFriend> ReceivedFriendships { get; set; } = [];
    public ICollection<UserGroup> UserGroups { get; set; } = [];
}