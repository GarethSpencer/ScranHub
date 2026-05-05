using DAL.Entities;
using RepositoryLayer.Abstractions.Generic;
using Utilities.Enums;
using Utilities.Models.Results;

namespace RepositoryLayer.Abstractions
{
    public interface IUserFriendRepository : IEFRepository<UserFriend>
    {
        Task<UserFriendResult?> GetUserFriendAsync(Guid userId1, Guid userId2, CancellationToken ct);
        Task<bool> IsFriendshipAcceptedAsync(Guid userId1, Guid userId2, CancellationToken ct);
        Task<bool> IsFriendshipPendingAsync(Guid userId1, Guid userId2, CancellationToken ct);
        Task<bool> IsFriendshipDeclinedAsync(Guid userId1, Guid userId2, CancellationToken ct);
        Task<Guid> CreateUserFriendAsync(Guid userId, Guid friendId, CancellationToken ct);
        Task UpdateUserFriendStatusAsync(Guid userFriendId, FriendshipStatus newStatus, CancellationToken ct);
    }
}
