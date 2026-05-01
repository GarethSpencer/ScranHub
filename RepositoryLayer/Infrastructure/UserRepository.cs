using DAL.Data;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Infrastructure.Generic;
using Utilities.Models.Requests.Users;
using Utilities.Models.Results;

namespace RepositoryLayer.Infrastructure;

public sealed class UserRepository(ScranHubDbContext dbContext) : EFRepository<User>(dbContext), IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct, bool trackChanges = false)
    {
        if (trackChanges)
        {
            return await _dbSet.FindAsync([id], ct);
        }

        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == id, ct);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct, bool trackChanges = false)
    {
        IQueryable<User> query = _dbSet.Where(x => x.Email == email);

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(ct);
    }

    public async Task<IEnumerable<User>> GetAllActiveAdminsAsync(CancellationToken ct, bool trackChanges = false)
    {
        IQueryable<User> query = _dbSet.Where(x => x.Admin && x.Active);

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.ToListAsync(ct);
    }

    public async Task<IEnumerable<FriendResult>?> GetFriendsForUserAsync(Guid userId, CancellationToken ct)
    {
        var friendInfo = await _dbSet
            .Where(x => x.UserId == userId)
            .Include(x => x.InitiatedFriendships).ThenInclude(x => x.Friend)
            .Include(x => x.ReceivedFriendships).ThenInclude(x => x.User)
            .FirstOrDefaultAsync(ct);

        if (friendInfo == null)
        {
            return null;
        }

        var result = friendInfo.InitiatedFriendships.Select(f => new FriendResult
        {
            UserFriendId = f.UserFriendId,
            FriendId = f.FriendId,
            DisplayName = f.Friend!.DisplayName,
            Active = f.Friend.Active,
            Approved = f.Approved,
            Initiator = true
        })
            .Concat(friendInfo.ReceivedFriendships.Select(f => new FriendResult
        {
            UserFriendId = f.UserFriendId,
            FriendId = f.UserId,
            DisplayName = f.User!.DisplayName,
            Active = f.User.Active,
            Approved = f.Approved,
            Initiator = false
        }));

        return [.. result];
    }

    public async Task<User?> GetUserGroupsByIdAsync(Guid userId, CancellationToken ct, bool trackChanges = false)
    {
        IQueryable<User> query = _dbSet
            .Where(x => x.UserId == userId)
            .Include(x => x.UserGroups).ThenInclude(x => x.Group);

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(ct);
    }

    public async Task<bool> IsUserAdminAsync(Guid userId, CancellationToken ct)
    {
        var user = await _dbSet.FindAsync([userId], ct);
        return user != null && user.Admin;
    }

    public async Task<Guid> CreateUserAsync(CreateUserRequest request, CancellationToken ct)
    {
        var newUser = new User
        {
            DisplayName = request.DisplayName,
            Email = request.Email,
            Admin = request.Admin,
            Active = true
        };

        await _dbSet.AddAsync(newUser, ct);
        return newUser.UserId;
    }
}