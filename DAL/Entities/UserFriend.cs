using DAL.Entities.Base;

namespace DAL.Entities;

public class UserFriend: AuditableEntity
{
    public required Guid UserFriendId { get; set; }
    public required Guid UserId { get; set; }
    public required Guid FriendId { get; set; }
    public required bool Approved { get; set; }

    public required User User { get; set; } = null!;
    public required User Friend { get; set; } = null!;
}