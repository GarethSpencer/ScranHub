using Microsoft.Extensions.Logging;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Abstractions.Generic;
using ServiceLayer.Abstractions;
using System.Net;
using Utilities.Models.Requests.GroupVenues;
using Utilities.Models.Responses.Generic;
using Utilities.Models.Responses.GroupVenues;
using Utilities.Token;

namespace ServiceLayer.Infrastructure;

public class GroupVenueService(ITokenData tokenData,
    ILogger<GroupVenueService> logger,
    IUserGroupRepository userGroupRepository,
    IGroupVenueRepository groupVenueRepository,
    IUserRepository userRepository,
    IGroupRepository groupRepository,
    IFoodTypeOptionRepository foodTypeOptionRepository,
    IVenueTypeOptionRepository venueTypeOptionRepository,
    IUnitOfWork unitOfWork) : IGroupVenueService
{
    private readonly ITokenData _tokenData = tokenData;
    private readonly ILogger<GroupVenueService> _logger = logger;
    private readonly IUserGroupRepository _userGroupRepository = userGroupRepository;
    private readonly IGroupVenueRepository _groupVenueRepository = groupVenueRepository;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IGroupRepository _groupRepository = groupRepository;
    private readonly IFoodTypeOptionRepository _foodTypeOptionRepository = foodTypeOptionRepository;
    private readonly IVenueTypeOptionRepository _venueTypeOptionRepository = venueTypeOptionRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<GetGroupVenueResponse> GetGroupVenueAsync(Guid groupVenueId, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("GetGroupVenueAsync called with no authenticated user.");
            return new GetGroupVenueResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            };
        }

        var groupVenue = await _groupVenueRepository.GetByIdAsync(groupVenueId, ct);

        if (groupVenue == null)
        {
            _logger.LogWarning("GroupVenue with ID {GroupVenueId} not found.", groupVenueId);
            return new GetGroupVenueResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Venue not found."
            };
        }

        var userId = _tokenData.UserId.Value;
        var isAdmin = await _userRepository.IsUserAdminAsync(userId, ct);
        var isUserInGroup = await _userGroupRepository.IsUserInGroupAsync(groupVenue.GroupId, userId, ct);

        if (!isUserInGroup && !isAdmin)
        {
            _logger.LogWarning("User {UserId} is not in group {GroupId}.", userId, groupVenue.GroupId);
            return new GetGroupVenueResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "User is not in group."
            };
        }

        _logger.LogInformation("GroupVenue [{GroupVenueId}] returned successfully.", groupVenue.GroupVenueId);

        return new GetGroupVenueResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Venue returned successfully.",
            GroupVenue = groupVenue
        };
    }

    public async Task<GetGroupVenuesResponse> SearchGroupVenuesAsync(Guid groupId, SearchGroupVenueRequest request, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("SearchGroupVenuesAsync called with no authenticated user.");
            return new GetGroupVenuesResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            };
        }

        var userId = _tokenData.UserId!.Value;
        var isGroupActive = await _groupRepository.ExistsAsync(x => x.GroupId == groupId && x.Active, ct);
        if (!isGroupActive)
        {
            _logger.LogWarning("Group with ID {GroupId} not found or inactive so user {UserId} cannot search venues.", groupId, userId);
            return new GetGroupVenuesResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Group not found or inactive."
            };
        }

        var isAdmin = await _userRepository.IsUserAdminAsync(userId, ct);
        var isUserInGroup = await _userGroupRepository.IsUserInGroupAsync(groupId, userId, ct);
        if (!isAdmin && !isUserInGroup)
        {
            _logger.LogWarning("User {UserId} is not an admin or group member and cannot search venues in this group {GroupId}.", userId, groupId);
            return new GetGroupVenuesResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "You cannot search venues in this group."
            };
        }

        var (groupVenues, totalCount) = await _groupVenueRepository.SearchByNameAsync(groupId, request, ct);

        return new GetGroupVenuesResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Venues returned successfully.",
            GroupVenues = groupVenues,
            TotalCount = totalCount
        };
    }

    public async Task<AddGroupVenueResponse> CreateGroupVenueAsync(CreateGroupVenueRequest request, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("CreateGroupVenueAsync called with no authenticated user.");
            return new AddGroupVenueResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            };
        }

        var callingUserId = _tokenData.UserId.Value;

        var isGroupActive = await _groupRepository.ExistsAsync(x => x.GroupId == request.GroupId && x.Active, ct);
        if (!isGroupActive)
        {
            _logger.LogWarning("Group with ID {GroupId} not found or inactive.", request.GroupId);
            return new AddGroupVenueResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Group not found or inactive."
            };
        }

        var isUserInGroup = await _userGroupRepository.IsUserInGroupAsync(request.GroupId, callingUserId, ct);
        if (!isUserInGroup)
        {
            _logger.LogWarning("User {UserId} is not in group {GroupId}.", callingUserId, request.GroupId);
            return new AddGroupVenueResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "User is not in group."
            };
        }

        var validFoodTypes = await _foodTypeOptionRepository.GetForGroupIdAsync(request.GroupId, ct);
        if (request.FoodTypeOptionId != null && !validFoodTypes.Any(fto => fto.FoodTypeOptionId == request.FoodTypeOptionId))
        {
            _logger.LogWarning("Invalid food type ID {FoodTypeOptionId} provided for group {GroupId}.", request.FoodTypeOptionId, request.GroupId);
            return new AddGroupVenueResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "Invalid food type provided."
            };
        }

        var validVenueTypes = await _venueTypeOptionRepository.GetForGroupIdAsync(request.GroupId, ct);
        if (request.VenueTypeOptionId != null && !validVenueTypes.Any(vto => vto.VenueTypeOptionId == request.VenueTypeOptionId))
        {
            _logger.LogWarning("Invalid venue type ID {VenueTypeOptionId} provided for group {GroupId}.", request.VenueTypeOptionId, request.GroupId);
            return new AddGroupVenueResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "Invalid venue type provided."
            };
        }

        var doesGroupVenueNameExist = await _groupVenueRepository.ExistsAsync(x => x.GroupId == request.GroupId && x.VenueName.ToLower() == request.VenueName.ToLower(), ct);
        if (doesGroupVenueNameExist)
        {
            _logger.LogWarning("Venue with name {VenueName} already exists in group {GroupId}.", request.VenueName, request.GroupId);
            return new AddGroupVenueResponse
            {
                StatusCode = HttpStatusCode.Conflict,
                Message = "Venue with this name already exists in the group."
            };
        }

        var groupVenueId = await _groupVenueRepository.CreateAsync(request, ct);

        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("GroupVenue [{GroupVenueId}] created successfully.", groupVenueId);

        return new AddGroupVenueResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Venue created successfully.",
            GroupVenueId = groupVenueId,
        };
    }

    public async Task<CommonResponse> UpdateGroupVenueAsync(Guid groupVenueId, UpdateGroupVenueRequest request, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("UpdateGroupVenueAsync called with no authenticated user.");
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            };
        }

        var callingUserId = _tokenData.UserId.Value;
        var groupVenue = await _groupVenueRepository.GetByIdAsync(groupVenueId, ct);
        if (groupVenue == null)
        {
            _logger.LogWarning("GroupVenue with ID {GroupVenueId} not found, so user {UserId} cannot update it.", groupVenueId, callingUserId);
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Venue not found."
            };
        }

        var isUserInGroup = await _userGroupRepository.IsUserInGroupAsync(groupVenue.GroupId, callingUserId, ct);
        if (!isUserInGroup)
        {
            _logger.LogWarning("User {UserId} is not in group {GroupId} so cannot update it.", callingUserId, groupVenue.GroupId);
            return new AddGroupVenueResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "User is not in group."
            };
        }

        var validFoodTypes = await _foodTypeOptionRepository.GetForGroupIdAsync(groupVenue.GroupId, ct);
        if (request.FoodTypeOptionId != null && !validFoodTypes.Any(fto => fto.FoodTypeOptionId == request.FoodTypeOptionId))
        {
            _logger.LogWarning("Invalid food type ID {FoodTypeOptionId} provided for group {GroupId}.", request.FoodTypeOptionId, groupVenue.GroupId);
            return new AddGroupVenueResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "Invalid food type provided."
            };
        }

        var validVenueTypes = await _venueTypeOptionRepository.GetForGroupIdAsync(groupVenue.GroupId, ct);
        if (request.VenueTypeOptionId != null && !validVenueTypes.Any(vto => vto.VenueTypeOptionId == request.VenueTypeOptionId))
        {
            _logger.LogWarning("Invalid venue type ID {VenueTypeOptionId} provided for group {GroupId}.", request.VenueTypeOptionId, groupVenue.GroupId);
            return new AddGroupVenueResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "Invalid venue type provided."
            };
        }

        if (!String.Equals(groupVenue.VenueName, request.VenueName, StringComparison.OrdinalIgnoreCase))
        {
            var groupVenueNameExists = await _groupVenueRepository.ExistsAsync(x => x.GroupId == groupVenue.GroupId && x.VenueName.ToLower() == request.VenueName.ToLower(), ct);
            if (groupVenueNameExists)
            {
                _logger.LogWarning("GroupVenue with name {GroupName} already exists.", request.VenueName);
                return new CommonResponse
                {
                    StatusCode = HttpStatusCode.Conflict,
                    Message = $"Venue with name {request.VenueName} already exists in this group."
                };
            }
        }

        await _groupVenueRepository.UpdateAsync(groupVenueId, request, ct);

        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("GroupVenue [{GroupVenueId}] updated successfully.", groupVenueId);

        return new CommonResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "GroupVenue updated successfully.",
        };
    }

    public async Task<CommonResponse> DeleteGroupVenueAsync(Guid groupVenueId, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("DeleteGroupVenueAsync called with no authenticated user.");
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            };
        }

        var userId = _tokenData.UserId!.Value;
        var groupVenue = await _groupVenueRepository.GetByIdAsync(groupVenueId, ct);
        if (groupVenue == null)
        {
            _logger.LogWarning("GroupVenue with ID {GroupVenueId} not found, so user {UserId} cannot update it.", groupVenueId, userId);
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Venue not found."
            };
        }

        var isAdmin = await _userRepository.IsUserAdminAsync(userId, ct);
        var isUserInGroup = await _userGroupRepository.IsUserInGroupAsync(groupVenue.GroupId, userId, ct);

        if (!isAdmin && !isUserInGroup)
        {
            _logger.LogWarning("User {UserId} is not an admin or group member and cannot delete group venue {GroupVenueId}.", userId, groupVenueId);
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "You are not an admin or group member and cannot delete this venue."
            };
        }

        await _groupVenueRepository.DeleteAsync(groupVenueId, ct);

        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("User {UserId} deleted group venue {GroupVenueId} successfully.", userId, groupVenueId);

        return new CommonResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Successfully deleted the venue."
        };
    }
}
