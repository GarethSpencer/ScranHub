using DAL.Data;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Infrastructure.Generic;
using Utilities.Enums;

namespace RepositoryLayer.Infrastructure;

public sealed class UserFriendRepository(ScranHubDbContext dbContext) : EFRepository<UserFriend>(dbContext), IUserFriendRepository
{
    public async Task<bool> DoesUserFriendExist(Guid userId1, Guid userId2, CancellationToken ct)
    {
        return await _dbSet.AnyAsync(uf =>
            ((uf.UserId == userId1 && uf.FriendId == userId2) ||
            (uf.UserId == userId2 && uf.FriendId == userId1)), ct);
    }

    public async Task<bool> IsFriendshipAcceptedAsync(Guid userId1, Guid userId2, CancellationToken ct)
    {
        return await _dbSet.AnyAsync(uf =>
            ((uf.UserId == userId1 && uf.FriendId == userId2) ||
            (uf.UserId == userId2 && uf.FriendId == userId1)) && uf.Status == FriendshipStatus.Accepted, ct);
    }

    public async Task<bool> IsFriendshipPendingAsync(Guid userId1, Guid userId2, CancellationToken ct)
    {
        return await _dbSet.AnyAsync(uf =>
            ((uf.UserId == userId1 && uf.FriendId == userId2) ||
            (uf.UserId == userId2 && uf.FriendId == userId1)) && uf.Status == FriendshipStatus.Pending, ct);
    }

    public async Task<bool> IsFriendshipDeclinedAsync(Guid userId1, Guid userId2, CancellationToken ct)
    {
        return await _dbSet.AnyAsync(uf =>
            ((uf.UserId == userId1 && uf.FriendId == userId2) ||
            (uf.UserId == userId2 && uf.FriendId == userId1)) && uf.Status == FriendshipStatus.Declined, ct);
    }

    public async Task<Guid> CreateUserFriendAsync(Guid userId, Guid friendId, CancellationToken ct)
    {
        var userFriend = new UserFriend
        {
            UserId = userId,
            FriendId = friendId,
            Status = FriendshipStatus.Pending
        };
        await _dbSet.AddAsync(userFriend, ct);
        return userFriend.UserFriendId;
    }
}
