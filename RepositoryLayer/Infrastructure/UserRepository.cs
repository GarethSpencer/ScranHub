using DAL.Data;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Infrastructure.Generic;
using Utilities.Enums;
using Utilities.Models.Requests.Generic;
using Utilities.Models.Requests.Users;
using Utilities.Models.Results;

namespace RepositoryLayer.Infrastructure;

public sealed class UserRepository(ScranHubDbContext dbContext) : EFRepository<User>(dbContext), IUserRepository
{
    public async Task<(IEnumerable<UserDetailedResult>, int)> GetAllAsync(PaginationBaseRequest request, CancellationToken ct)
    {
        var query = _dbSet
            .OrderBy(u => u.DisplayName);

        var total = await query.CountAsync(ct);

        var results = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new UserDetailedResult
            {
                UserId = u.UserId,
                AuthId = u.AuthId,
                DisplayName = u.DisplayName,
                Active = u.Active,
                Email = u.Email,
                Admin = u.Admin,
                FriendCount = u.InitiatedFriendships.Count() + u.ReceivedFriendships.Count(),
                CreatedOn = u.CreatedOn,
                CreatedBy = u.CreatedBy,
                UpdatedOn = u.UpdatedOn,
                UpdatedBy = u.UpdatedBy
            })
            .ToListAsync(ct);

        return (results, total);
    }

    public async Task<UserDetailedResult?> GetDetailsByIdAsync(Guid id, CancellationToken ct)
    {
        var user = await _dbSet.FindAsync([id], ct);

        if (user == null)
        {
            return null;
        }

        return new UserDetailedResult
        {
            UserId = user.UserId,
            AuthId = user.AuthId,
            DisplayName = user.DisplayName,
            Email = user.Email,
            Active = user.Active,
            Admin = user.Admin,
            FriendCount = user.InitiatedFriendships.Count + user.ReceivedFriendships.Count,
            CreatedOn = user.CreatedOn,
            CreatedBy = user.CreatedBy,
            UpdatedOn = user.UpdatedOn,
            UpdatedBy = user.UpdatedBy
        };
    }

    public async Task<UserResult?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var user = await _dbSet.FindAsync([id], ct);

        if (user == null)
        {
            return null;
        }

        return new UserResult
        {
            UserId = user.UserId,
            DisplayName = user.DisplayName,
            Active = user.Active,
        };
    }

    public async Task<UserAuthResult?> GetByEmailAsync(string email, CancellationToken ct)
    {
        var user = await _dbSet.FirstOrDefaultAsync(x => x.Email == email, ct);

        if (user == null)
        {
            return null;
        }

        return new UserAuthResult
        {
            UserId = user.UserId,
            Email = user.Email,
            AuthId = user.AuthId,
            Admin = user.Admin,
            Active = user.Active
        };
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

    public async Task<(IEnumerable<FriendResult>?, int)> GetFriendsForUserAsync(Guid userId, GetUserFriendRequest request, CancellationToken ct)
    {
        var query = _dbSet
            .Where(x => x.UserId == userId)
            .Include(x => x.ReceivedFriendships.Where(f => f.Status == request.Status)).ThenInclude(x => x.User);

        // skip initiated friendships if the recipient declined them
        if (request.Status != FriendshipStatus.Declined)
        {
            query = query.Include(x => x.InitiatedFriendships.Where(f => f.Status == request.Status)).ThenInclude(x => x.Friend);
        }

        var friendInfo = await query
            .AsSplitQuery()
            .FirstOrDefaultAsync(ct);

        if (friendInfo == null)
        {
            return (null, 0);
        }

        var resultList = new List<FriendResult>();

        if (request.Status != FriendshipStatus.Declined)
        {
            resultList.AddRange(friendInfo.InitiatedFriendships
                .Where(x => x.Friend!.Active)
                .Select(f => new FriendResult
                {
                    UserFriendId = f.UserFriendId,
                    FriendId = f.FriendId,
                    DisplayName = f.Friend!.DisplayName,
                    Active = f.Friend.Active,
                    Status = f.Status,
                    Initiator = true
                }));
        }

        resultList.AddRange(friendInfo.ReceivedFriendships
            .Where(x => x.User!.Active)
            .Select(f => new FriendResult
            {
                UserFriendId = f.UserFriendId,
                FriendId = f.UserId,
                DisplayName = f.User!.DisplayName,
                Active = f.User.Active,
                Status = f.Status,
                Initiator = false
            }));

        var total = resultList.Count;

        return (resultList
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize), total);
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

    public async Task<Guid> CreateAsync(CreateUserRequest createRequest, CancellationToken ct)
    {
        var newUser = new User
        {
            DisplayName = createRequest.DisplayName,
            Email = createRequest.Email,
            Admin = createRequest.Admin,
            Active = true
        };

        await _dbSet.AddAsync(newUser, ct);
        return newUser.UserId;
    }

    public async Task UpdateAsync(Guid userId, UpdateUserRequest userRequest, CancellationToken ct)
    {
        var user = await _dbSet.FindAsync([userId], ct);
        if (user != null)
        {
            user.DisplayName = userRequest.DisplayName;
            user.Admin = userRequest.Admin;
            user.Active = userRequest.Active;
        }
    }

    public async Task SetActiveAsync(Guid userId, CancellationToken ct)
    {
        var user = await _dbSet.FindAsync([userId], ct);
        user?.Active = true;
    }

    public async Task UpdateEmailAsync(Guid userId, string email, CancellationToken ct)
    {
        var user = await _dbSet.FindAsync([userId], ct);
        user?.Email = email;
    }

    public async Task<(IEnumerable<UserResult>, int)> SearchByDisplayNameAsync(SearchUserRequest request, CancellationToken ct)
    {
        var usersQuery = _dbSet
            .Where(x => x.Active && EF.Functions.Like(x.DisplayName, $"%{request.SearchText}%"));

        var totalCount = await usersQuery.CountAsync(ct);

        var users = await usersQuery
            .OrderBy(x => x.DisplayName)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var userResults = users.Select(u => new UserResult
        {
            UserId = u.UserId,
            DisplayName = u.DisplayName,
            Active = u.Active,
        });

        return (userResults, totalCount);
    }

    public async Task DeleteAsync(Guid userId, CancellationToken ct)
    {
        var user = await _dbSet.FindAsync([userId], ct);

        if (user == null)
        {
            return;
        }

        // handle friendId references that EF doesn't manage due to DeleteBehavior.NoAction
        var receivedFriendships = await _dbContext.UserFriends
            .Where(uf => uf.FriendId == userId)
            .ToListAsync(ct);
        _dbContext.UserFriends.RemoveRange(receivedFriendships);

        _dbSet.Remove(user);
    }

    public async Task<UserAuthResult?> GetByAuthIdAsync(string authId, CancellationToken ct)
    {
        return await _dbSet.Where(x => x.AuthId == authId)
            .Select(u => new UserAuthResult
            {
                UserId = u.UserId,
                Email = u.Email,
                AuthId = u.AuthId,
                Admin = u.Admin,
                Active = u.Active
            }).FirstOrDefaultAsync(ct);
    }

    public async Task SetAuthIdAsync(Guid userId, string authId, CancellationToken ct)
    {
        var user = await _dbSet.FindAsync([userId], ct);
        user?.AuthId = authId;
    }

    public async Task<IEnumerable<UserAuthResult>> GetAllLongTermInactiveAsync(int minimumDaysInactive, CancellationToken ct)
    {
        var requiredDeactivationDate = DateTime.UtcNow.AddDays(-minimumDaysInactive);
        return await _dbSet.Where(x => !x.Active && x.UpdatedOn <= requiredDeactivationDate)
            .Select(u => new UserAuthResult
            {
             UserId = u.UserId,
             Active = u.Active,
             AuthId = u.AuthId,
             Email = u.Email,
             Admin = u.Admin
            }).ToListAsync(ct);
    }
}
