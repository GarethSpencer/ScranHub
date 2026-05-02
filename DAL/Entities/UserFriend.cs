using DAL.Entities.Base;
using Utilities.Enums;

namespace DAL.Entities;

public class UserFriend: AuditableEntity
{
    public Guid UserFriendId { get; set; }
    public required Guid UserId { get; set; }
    public required Guid FriendId { get; set; }
    public required FriendshipStatus Status { get; set; }

    public User? User { get; set; }
    public User? Friend { get; set; }
}
