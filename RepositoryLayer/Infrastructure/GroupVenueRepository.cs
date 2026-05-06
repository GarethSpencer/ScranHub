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
            .Select(x => new GroupVenueResult
        {
            GroupVenueId = x.GroupVenueId,
            GroupId = x.GroupId,
            VenueName = x.VenueName,
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
            VenueType = x.VenueTypeOption!.Label,
            FoodType = x.FoodTypeOption!.Label
        }).ToListAsync(ct);
    }

    public async Task<Guid> CreateGroupVenue(CreateGroupVenueRequest request, CancellationToken ct)
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
}