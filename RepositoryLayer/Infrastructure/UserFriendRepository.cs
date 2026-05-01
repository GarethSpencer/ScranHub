using DAL.Data;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Infrastructure.Generic;

namespace RepositoryLayer.Infrastructure;

public sealed class UserFriendRepository(ScranHubDbContext dbContext) : EFRepository<UserFriend>(dbContext), IUserFriendRepository
{
    public async Task<bool> AreUsersFriendsAsync(Guid userId1, Guid userId2, CancellationToken ct)
    {
        return await _dbSet.AnyAsync(uf =>
            ((uf.UserId == userId1 && uf.FriendId == userId2) ||
            (uf.UserId == userId2 && uf.FriendId == userId1)) && uf.Approved, ct);
    }

    public async Task<bool> IsFriendshipPending(Guid userId1, Guid userId2, CancellationToken ct)
    {
        return await _dbSet.AnyAsync(uf =>
            ((uf.UserId == userId1 && uf.FriendId == userId2) ||
            (uf.UserId == userId2 && uf.FriendId == userId1)) && !uf.Approved, ct);
    }
}
