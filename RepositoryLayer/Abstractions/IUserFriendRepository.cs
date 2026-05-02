using DAL.Entities;
using RepositoryLayer.Abstractions.Generic;
using Utilities.Models.Requests.Users;

namespace RepositoryLayer.Abstractions
{
    public interface IUserFriendRepository : IEFRepository<UserFriend>
    {
        Task<bool> DoesUserFriendExist(Guid userId1, Guid userId2, CancellationToken ct);
        Task<bool> IsFriendshipAcceptedAsync(Guid userId1, Guid userId2, CancellationToken ct);
        Task<bool> IsFriendshipPendingAsync(Guid userId1, Guid userId2, CancellationToken ct);
        Task<bool> IsFriendshipDeclinedAsync(Guid userId1, Guid userId2, CancellationToken ct);
        Task<Guid> CreateUserFriendAsync(Guid userId, Guid friendId, CancellationToken ct);
    }
}