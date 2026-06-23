using DAL.Entities;
using RepositoryLayer.Abstractions.Generic;
using Utilities.Models.Requests.Generic;
using Utilities.Models.Requests.Groups;
using Utilities.Models.Results;
using Utilities.Models.Results.Generic;

namespace RepositoryLayer.Abstractions;

public interface IGroupRepository : IEFRepository<Group>
{
    Task<(IEnumerable<GroupDetailedResult>, int)> GetAllAsync(PaginationBaseRequest request, CancellationToken ct);

    Task<GroupResult?> GetDetailsByIdAsync(Guid id, CancellationToken ct);

    Task<GroupResult?> GetByNameAsync(string name, CancellationToken ct);

    Task<(IEnumerable<GroupDetailedResult>, int)> SearchAllByNameAsync(SearchGroupRequest request, Guid userId, CancellationToken ct);

    Task<(IEnumerable<GroupResult>, int)> SearchByNameAsync(SearchGroupRequest request, Guid userId, CancellationToken ct);

    Task<Guid> CreateAsync(string groupName, CancellationToken ct);

    Task DeleteAsync(Guid groupId, CancellationToken ct);

    Task<bool> DidUserCreateGroupAsync(Guid groupId, Guid userId, CancellationToken ct);

    Task<bool> DoesUserHaveFriendInGroupAsync(Guid groupId, Guid userId, CancellationToken ct);

    Task DeactivateAsync(Guid groupId, CancellationToken ct);

    Task UpdateAsync(Guid groupId, UpdateGroupRequest groupRequest, CancellationToken ct);

    Task<IEnumerable<GroupVenueRatingResult>> GetVenueCostRatingsForGroupAsync(Guid groupId, CancellationToken ct);

    Task<IEnumerable<GroupVenueRatingResult>> GetVenueQualityRatingsForGroupAsync(Guid groupId, CancellationToken ct);

    Task UnsetVenueTypesForGroupAsync(Guid groupId, CancellationToken ct);

    Task UnsetFoodTypesForGroupAsync(Guid groupId, CancellationToken ct);

    Task<bool> AreAnyVenuesUsingOptionIdAsync(Guid groupId, Guid optionId, CancellationToken ct);
}
