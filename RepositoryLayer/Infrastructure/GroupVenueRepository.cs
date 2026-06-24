using DAL.Data;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Infrastructure.Generic;
using System.Linq.Expressions;
using Utilities.Enums;
using Utilities.Models.Requests.GroupVenues;
using Utilities.Models.Results;
using static Utilities.Enums.GroupVenueSortParameters;

namespace RepositoryLayer.Infrastructure;

public sealed class GroupVenueRepository(ScranHubDbContext dbContext) : EFRepository<GroupVenue>(dbContext), IGroupVenueRepository
{
    public async Task<GroupVenueResult?> GetByIdAsync(Guid groupVenueId, Guid callingUserId, CancellationToken ct)
    {
        return await _dbSet
            .Include(x => x.VenueTypeOption)
            .Include(x => x.FoodTypeOption)
            .Include(x => x.Group)
            .Where(x => x.Group!.Active)
            .Select(ToResult(callingUserId))
            .FirstOrDefaultAsync(x => x.GroupVenueId == groupVenueId, ct);
    }

    public async Task<IEnumerable<GroupVenueResult>> GetAllVenuesWithInfoByGroupIdAsync(Guid groupId, Guid callingUserId, CancellationToken ct)
    {
        var query = _dbSet.Where(x => x.GroupId == groupId)
            .Include(x => x.Group)
            .Include(x => x.FoodTypeOption)
            .Include(x => x.VenueTypeOption);

        return await query.Select(ToResult(callingUserId)).ToListAsync(ct);
    }

    public async Task<(IEnumerable<GroupVenueResult>, int)> GetByGroupIdAsync(Guid groupId, SortableGroupVenueRequest request, Guid callingUserId, CancellationToken ct)
    {
        var groupVenueQuery = _dbSet
            .Include(x => x.VenueTypeOption)
            .Include(x => x.FoodTypeOption)
            .Where(x => x.GroupId == groupId);

        var totalCount = await groupVenueQuery.CountAsync(ct);

        var results = await ApplySorting(groupVenueQuery, request.SortBy, callingUserId, request.SortDescending)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(ToResult(callingUserId))
            .ToListAsync(ct);
        return (results, totalCount);
    }

    public async Task<(IEnumerable<GroupVenueResult>, int)> SearchByNameAsync(Guid groupId, SearchGroupVenueRequest request, Guid callingUserId, CancellationToken ct)
    {
        var groupVenueQuery = _dbSet
            .Include(x => x.VenueTypeOption)
            .Include(x => x.FoodTypeOption)
            .Where(x => x.GroupId == groupId && (EF.Functions.Like(x.VenueName, $"%{request.SearchText}%")
                || EF.Functions.Like((x.VenueTypeOption == null ? "" : x.VenueTypeOption!.Label), $"%{request.SearchText}%")
                || EF.Functions.Like((x.FoodTypeOption == null ? "" : x.FoodTypeOption!.Label), $"%{request.SearchText}%"))
            );

        var totalCount = await groupVenueQuery.CountAsync(ct);

        var results = await groupVenueQuery
            .OrderBy(x => x.VenueName)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(ToResult(callingUserId))
            .ToListAsync(ct);
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

    private static IQueryable<GroupVenue> ApplySorting(IQueryable<GroupVenue> query, GroupVenueSortParameters sortBy, Guid currentUserId, bool sortDescending)
    {
        return (sortBy, sortDescending) switch
        {
            (VenueName, false) => query.OrderBy(x => x.VenueName),
            (VenueName, true) => query.OrderByDescending(x => x.VenueName),
            (Visited, false) => query.OrderBy(x => x.Visited)
                .ThenBy(x => x.VenueName),
            (Visited, true) => query.OrderByDescending(x => x.Visited)
                .ThenByDescending(x => x.VenueName),
            (FoodType, false) => query.OrderBy(x => x.FoodTypeOption == null ? "" : x.FoodTypeOption.Label)
                .ThenBy(x => x.VenueName),
            (FoodType, true) => query.OrderByDescending(x => x.FoodTypeOption == null ? "" : x.FoodTypeOption.Label)
                .ThenByDescending(x => x.VenueName),
            (VenueType, false) => query.OrderBy(x => x.VenueTypeOption == null ? "" : x.VenueTypeOption.Label)
                .ThenBy(x => x.VenueName),
            (VenueType, true) => query.OrderByDescending(x => x.VenueTypeOption == null ? "" : x.VenueTypeOption.Label)
                .ThenByDescending(x => x.VenueName),
            (AvgCostRating, false) => query.OrderBy(x => x.CostRatings.Any() ? (decimal?)x.CostRatings.Average(r => (decimal)r.CostOption!.DisplayOrder) : null)
                .ThenBy(x => x.VenueName),
            (AvgCostRating, true) => query.OrderByDescending(x => x.CostRatings.Any() ? (decimal?)x.CostRatings.Average(r => (decimal)r.CostOption!.DisplayOrder) : null)
                .ThenByDescending(x => x.VenueName),
            (AvgQualityRating, false) => query.OrderBy(x => x.QualityRatings.Any() ? (decimal?)x.QualityRatings.Average(r => (decimal)r.QualityOption!.DisplayOrder) : null)
                .ThenBy(x => x.VenueName),
            (AvgQualityRating, true) => query.OrderByDescending(x => x.QualityRatings.Any() ? (decimal?)x.QualityRatings.Average(r => (decimal)r.QualityOption!.DisplayOrder) : null)
                .ThenByDescending(x => x.VenueName),
            (MyCostRating, false) => query.OrderBy(x => x.CostRatings.Where(r => r.UserId == currentUserId).Select(r => (int?)r.CostOption!.DisplayOrder).FirstOrDefault())
                .ThenBy(x => x.VenueName),
            (MyCostRating, true) => query.OrderByDescending(x => x.CostRatings.Where(r => r.UserId == currentUserId).Select(r => (int?)r.CostOption!.DisplayOrder).FirstOrDefault())
                .ThenByDescending(x => x.VenueName),
            (MyQualityRating, false) => query.OrderBy(x => x.QualityRatings.Where(r => r.UserId == currentUserId).Select(r => (int?)r.QualityOption!.DisplayOrder).FirstOrDefault())
                .ThenBy(x => x.VenueName),
            (MyQualityRating, true) => query.OrderByDescending(x => x.QualityRatings.Where(r => r.UserId == currentUserId).Select(r => (int?)r.QualityOption!.DisplayOrder).FirstOrDefault())
                .ThenByDescending(x => x.VenueName),
            _ => query.OrderBy(x => x.VenueName)
        };
    }

    private static Expression<Func<GroupVenue, GroupVenueResult>> ToResult(Guid currentUserId) =>
        x => new GroupVenueResult
        {
            GroupVenueId = x.GroupVenueId,
            GroupId = x.GroupId,
            VenueName = x.VenueName,
            Visited = x.Visited,
            VenueType = x.VenueTypeOption == null ? "" : x.VenueTypeOption.Label,
            FoodType = x.FoodTypeOption == null ? "" : x.FoodTypeOption.Label,
            AverageCostRating = x.CostRatings.Average(r => (decimal?)r.CostOption!.DisplayOrder),
            AverageQualityRating = x.QualityRatings.Average(r => (decimal?)r.QualityOption!.DisplayOrder),
            MyCostRating = x.CostRatings
                .Where(r => r.UserId == currentUserId)
                .Select(r => (decimal?)r.CostOption!.DisplayOrder)
                .FirstOrDefault(),
            MyQualityRating = x.QualityRatings
                .Where(r => r.UserId == currentUserId)
                .Select(r => (decimal?)r.QualityOption!.DisplayOrder)
                .FirstOrDefault(),
        };

}