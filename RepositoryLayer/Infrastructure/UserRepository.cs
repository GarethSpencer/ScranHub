using DAL.Data;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Infrastructure.Generic;

namespace RepositoryLayer.Infrastructure;

public sealed class UserRepository(ScranHubDbContext dbContext) : EFRepository<User>(dbContext), IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct, bool trackChanges = false)
    {
        if (trackChanges)
        {
            return await _dbContext.FindAsync<User>([id], ct);
        }

        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == id, ct);
    }

    public async Task<User?> GetByDisplayNameAsync(string name, CancellationToken ct, bool trackChanges = false)
    {
        IQueryable<User> query = _dbSet.Where(x => x.DisplayName == name);

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

    public async Task<User?> GetUserWithFriendsById(Guid userId, CancellationToken ct, bool trackChanges = false)
    {
        IQueryable<User> query = _dbSet
            .Where(x => x.UserId == userId)
            .Include(x => x.InitiatedFriendships).ThenInclude(x => x.Friend)
            .Include(x => x.ReceivedFriendships).ThenInclude(x => x.User);

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(ct);
    }

    public async Task<User?> GetUserGroupsById(Guid userId, CancellationToken ct, bool trackChanges = false)
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
}