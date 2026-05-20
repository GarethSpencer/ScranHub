using DAL.Data;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Infrastructure.Generic;
using Utilities.Models.Results;
using Utilities.Models.Requests.Groups;
using Utilities.Enums;
using Utilities.Models.Requests.Generic;
using Utilities.Models.Results.Generic;

namespace RepositoryLayer.Infrastructure;

public sealed class GroupRepository(ScranHubDbContext dbContext) : EFRepository<Group>(dbContext), IGroupRepository
{
    public async Task<(IEnumerable<GroupDetailedResult>, int)> GetAllAsync(PaginationBaseRequest request, CancellationToken ct)
    {
        var groups = await _dbSet
            .Include(x => x.UserGroups)
            .Include(x => x.GroupVenues)
            .OrderBy(g => g.GroupName)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var groupResults = groups.Select(g => new GroupDetailedResult
        {
            GroupId = g.GroupId,
            GroupName = g.GroupName,
            Active = g.Active,
            UserCount = g.UserGroups.Count,
            VenueCount = g.GroupVenues.Count,
            CreatedOn = g.CreatedOn,
            CreatedBy = g.CreatedBy,
            UpdatedOn = g.UpdatedOn,
            UpdatedBy = g.UpdatedBy,
        });

        return (groupResults, groupResults.Count());
    }

    public async Task<GroupResult?> GetDetailsByIdAsync(Guid id, CancellationToken ct)
    {
        var group = await _dbSet.FindAsync([id], ct);

        if (group == null)
        {
            return null;
        }

        return new GroupResult
        {
            GroupId = group.GroupId,
            GroupName = group.GroupName,
            Active = group.Active,
        };
    }

    public async Task<GroupResult?> GetByNameAsync(string name, CancellationToken ct)
    {
        var group = await _dbSet.FirstOrDefaultAsync(x => x.GroupName == name, ct);

        if (group == null)
        {
            return null;
        }

        return new GroupResult
        {
            GroupId = group.GroupId,
            GroupName = group.GroupName,
            Active = group.Active,
        };
    }

    public async Task<(IEnumerable<GroupResult>, int)> SearchByNameAsync(SearchGroupRequest request, Guid userId, CancellationToken ct)
    {
        var groupsQuery = _dbSet
            .Where(x => EF.Functions.Like(x.GroupName, $"%{request.SearchText}%"));

        var user = await _dbContext.Users.FindAsync([userId], ct);
        var isAdmin = user != null && user.Admin;

        if (!isAdmin)
        {
            groupsQuery = groupsQuery.Where(g => g.Active
                && (g.UserGroups.Any(ug => ug.User!.InitiatedFriendships.Any(f => f.FriendId == userId && f.Status == FriendshipStatus.Accepted))
                || g.UserGroups.Any(ug => ug.User!.ReceivedFriendships.Any(f => f.UserId == userId && f.Status == FriendshipStatus.Accepted))));
        }

        var totalCount = await groupsQuery.CountAsync(ct);

        var groups = await groupsQuery
            .OrderBy(x => x.GroupName)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var groupResults = groups.Select(g => new GroupResult
        {
            GroupId = g.GroupId,
            GroupName = g.GroupName,
            Active = g.Active,
        });

        return (groupResults, totalCount);
    }

    public async Task<Guid> CreateAsync(string groupName, CancellationToken ct)
    {
        var group = new Group
        {
            GroupName = groupName,
            Active = true
        };

        await _dbSet.AddAsync(group, ct);
        return group.GroupId;
    }

    public async Task DeactivateAsync(Guid groupId, CancellationToken ct)
    {
        var group = await _dbSet.FindAsync([groupId], ct);

        group?.Active = false;
    }

    public async Task DeleteAsync(Guid groupId, CancellationToken ct)
    {
        var group = await _dbSet.FindAsync([groupId], ct);
        if (group != null)
        {
            _dbSet.Remove(group);
        }
    }

    public async Task<bool> DidUserCreateGroupAsync(Guid groupId, Guid userId, CancellationToken ct)
    {
        return await _dbSet.AnyAsync(ug => ug.GroupId == groupId && ug.CreatedBy == userId, ct);
    }

    public async Task<bool> DoesUserHaveFriendInGroupAsync(Guid groupId, Guid userId, CancellationToken ct)
    {
        return await _dbSet.Where(x => x.GroupId == groupId)
            .AnyAsync(g => g.UserGroups.Any(ug => ug.User!.InitiatedFriendships.Any(f => f.FriendId == userId && f.Status == FriendshipStatus.Accepted)
                || g.UserGroups.Any(ug => ug.User!.ReceivedFriendships.Any(f => f.UserId == userId && f.Status == FriendshipStatus.Accepted))), ct);
    }

    public async Task UpdateAsync(Guid groupId, UpdateGroupRequest groupRequest, CancellationToken ct)
    {
        var group = await _dbSet.FindAsync([groupId], ct);
        if (group != null)
        {
            group.GroupName = groupRequest.GroupName;
            group.Active = groupRequest.Active;
        }
    }

    public async Task<IEnumerable<GroupVenueRatingResult>> GetVenueCostRatingsForGroupAsync(Guid groupId, CancellationToken ct)
    {
        var group = await _dbSet
            .Include(x => x.GroupVenues)
            .ThenInclude(x => x.CostRatings)
            .ThenInclude(x => x.CostOption)
            .FirstOrDefaultAsync(x => x.GroupId == groupId, ct);

        if (group == null || group.GroupVenues.Count == 0)
        {
            return [];
        }

        return group.GroupVenues.Select(gv => new GroupVenueRatingResult
        {
            GroupId = gv.GroupId,
            GroupVenueId = gv.GroupVenueId,
            VenueName = gv.VenueName,
            Ratings = gv.CostRatings.Select(cr => new RatingVenueResult
            {
                RatingId = cr.CostRatingId,
                UserId = cr.UserId,
                OptionId = cr.CostOptionId,
                Label = cr.CostOption!.Label
            })
        });
    }

    public async Task<IEnumerable<GroupVenueRatingResult>> GetVenueQualityRatingsForGroupAsync(Guid groupId, CancellationToken ct)
    {
        var group = await _dbSet
            .Include(x => x.GroupVenues)
            .ThenInclude(x => x.QualityRatings)
            .ThenInclude(x => x.QualityOption)
            .FirstOrDefaultAsync(x => x.GroupId == groupId, ct);

        if (group == null || group.GroupVenues.Count == 0)
        {
            return [];
        }

        return group.GroupVenues.Select(gv => new GroupVenueRatingResult
        {
            GroupId = gv.GroupId,
            GroupVenueId = gv.GroupVenueId,
            VenueName = gv.VenueName,
            Ratings = gv.QualityRatings.Select(qr => new RatingVenueResult
            {
                RatingId = qr.QualityRatingId,
                UserId = qr.UserId,
                OptionId = qr.QualityOptionId,
                Label = qr.QualityOption!.Label
            })
        });
    }

    public async Task UnsetVenueTypesForGroupAsync(Guid groupId, CancellationToken ct)
    {
        var group = await _dbSet
            .Include(x => x.GroupVenues)
            .Where(x => x.GroupId == groupId).FirstOrDefaultAsync(ct);

        if (group == null)
        {
            return;
        }

        foreach (var groupVenue in group.GroupVenues)
        {
            groupVenue.VenueTypeOptionId = null;
        }
    }

    public async Task UnsetFoodTypesForGroupAsync(Guid groupId, CancellationToken ct)
    {
        var group = await _dbSet
            .Include(x => x.GroupVenues)
            .Where(x => x.GroupId == groupId).FirstOrDefaultAsync(ct);

        if (group == null)
        {
            return;
        }

        foreach (var groupVenue in group.GroupVenues)
        {
            groupVenue.FoodTypeOptionId = null;
        }
    }

    public async Task<bool> AreAnyVenuesUsingOptionIdAsync(Guid groupId, Guid optionId, CancellationToken ct)
    {
        return await _dbSet
            .Where(x => x.GroupId == groupId)
            .AnyAsync(g => g.GroupVenues.Any(gv => gv.VenueTypeOptionId == optionId || gv.FoodTypeOptionId == optionId), ct);
    }
}
