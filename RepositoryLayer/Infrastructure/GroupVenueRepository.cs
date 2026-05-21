using DAL.Data;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Infrastructure.Generic;
using Utilities.Models.Requests.GroupVenues;
using Utilities.Models.Results;

namespace RepositoryLayer.Infrastructure;

public sealed class GroupVenueRepository(ScranHubDbContext dbContext) : EFRepository<GroupVenue>(dbContext), IGroupVenueRepository
{
    public async Task<GroupVenueResult?> GetByIdAsync(Guid groupVenueId, CancellationToken ct)
    {
        return await _dbSet
            .Include(x => x.VenueTypeOption)
            .Include(x => x.FoodTypeOption)
            .Include(x => x.Group)
            .Where(x => x.Group!.Active)
            .Select(x => new GroupVenueResult
        {
            GroupVenueId = x.GroupVenueId,
            GroupId = x.GroupId,
            VenueName = x.VenueName,
            Visited = x.Visited,
            VenueType = x.VenueTypeOption!.Label,
            FoodType = x.FoodTypeOption!.Label
        }).FirstOrDefaultAsync(x => x.GroupVenueId == groupVenueId, ct);
    }

    public async Task<IEnumerable<GroupVenueResult>> GetAllVenuesWithInfoByGroupIdAsync(Guid groupId, CancellationToken ct)
    {
        var query = _dbSet.Where(x => x.GroupId == groupId)
            .Include(x => x.Group)
            .Include(x => x.FoodTypeOption)
            .Include(x => x.VenueTypeOption);

        return await query.Select(x => new GroupVenueResult
        {
            GroupVenueId = x.GroupVenueId,
            GroupId = x.GroupId,
            VenueName = x.VenueName,
            Visited = x.Visited,
            VenueType = x.VenueTypeOption!.Label,
            FoodType = x.FoodTypeOption!.Label
        }).ToListAsync(ct);
    }

    public async Task<(IEnumerable<GroupVenueResult>, int)> SearchByNameAsync(Guid groupId, SearchGroupVenueRequest request, CancellationToken ct)
    {
        var groupVenueQuery = _dbSet
            .Include(x => x.VenueTypeOption)
            .Include(x => x.FoodTypeOption)
            .Where(x => x.GroupId == groupId);

        if (request.SearchText != null)
        {
            groupVenueQuery = groupVenueQuery.Where(x => EF.Functions.Like(x.VenueName, $"%{request.SearchText}%"));
        }

        var totalCount = await groupVenueQuery.CountAsync(ct);

        var results = await groupVenueQuery
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .OrderBy(x => x.VenueName)
            .Select(x => new GroupVenueResult
            {
                GroupVenueId = x.GroupVenueId,
                GroupId = x.GroupId,
                VenueName = x.VenueName,
                Visited = x.Visited,
                VenueType = x.VenueTypeOption!.Label,
                FoodType = x.FoodTypeOption!.Label
            }).ToListAsync(ct);
        return (results, totalCount);
    }

    public async Task<Guid> CreateAsync(CreateGroupVenueRequest request, CancellationToken ct)
    {
        var newGroupVenue = new GroupVenue
        {
            GroupId = request.GroupId,
            VenueName = request.VenueName,
            VenueTypeOptionId = request.VenueTypeOptionId,
            FoodTypeOptionId = request.FoodTypeOptionId,
            Visited = false
        };

        await _dbSet.AddAsync(newGroupVenue, ct);
        return newGroupVenue.GroupVenueId;
    }

    public async Task UpdateAsync(Guid groupVenueId, UpdateGroupVenueRequest request, CancellationToken ct)
    {
        var groupVenue = await _dbSet.FindAsync([groupVenueId], ct);
        if (groupVenue != null)
        {
            groupVenue.VenueName = request.VenueName;
            groupVenue.VenueTypeOptionId = request.VenueTypeOptionId;
            groupVenue.FoodTypeOptionId = request.FoodTypeOptionId;
            groupVenue.Visited = request.Visited;
        }
    }

    public async Task DeleteAsync(Guid groupVenueId, CancellationToken ct)
    {
        var groupVenue = await _dbSet.FindAsync([groupVenueId], ct);
        if (groupVenue != null)
        {
            _dbSet.Remove(groupVenue);
        }
    }
}