using DAL.Entities;
using RepositoryLayer.Abstractions.Generic;

namespace RepositoryLayer.Abstractions
{
    public interface IUserFriendRepository : IEFRepository<UserFriend>
    {
        Task<bool> AreUsersFriendsAsync(Guid userId1, Guid userId2, CancellationToken ct);
        Task<bool> IsFriendshipPending(Guid userId1, Guid userId2, CancellationToken ct);
    }
}