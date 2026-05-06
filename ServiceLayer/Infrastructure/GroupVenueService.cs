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
        if (!validFoodTypes.Any(fto => fto.FoodTypeOptionId == request.FoodTypeOptionId))
        {
            _logger.LogWarning("Invalid food type ID {FoodTypeOptionId} provided for group {GroupId}.", request.FoodTypeOptionId, request.GroupId);
            return new AddGroupVenueResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "Invalid food type provided."
            };
        }

        var validVenueTypes = await _venueTypeOptionRepository.GetForGroupIdAsync(request.GroupId, ct);
        if (!validVenueTypes.Any(vto => vto.VenueTypeOptionId == request.VenueTypeOptionId))
        {
            _logger.LogWarning("Invalid venue type ID {VenueTypeOptionId} provided for group {GroupId}.", request.VenueTypeOptionId, request.GroupId);
            return new AddGroupVenueResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "Invalid venue type provided."
            };
        }

        var groupVenueId = await _groupVenueRepository.CreateGroupVenue(request, ct);

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

        var isGroupActive = await _groupRepository.ExistsAsync(x => x.GroupId == groupVenue.GroupId && x.Active, ct);
        if (!isGroupActive)
        {
            _logger.LogWarning("Group with ID {GroupId} not found or inactive, so user {UserId} cannot update it.", groupVenue.GroupId, callingUserId);
            return new AddGroupVenueResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Group not found or inactive."
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
        if (!validFoodTypes.Any(fto => fto.FoodTypeOptionId == request.FoodTypeOptionId))
        {
            _logger.LogWarning("Invalid food type ID {FoodTypeOptionId} provided for group {GroupId}.", request.FoodTypeOptionId, groupVenue.GroupId);
            return new AddGroupVenueResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "Invalid food type provided."
            };
        }

        var validVenueTypes = await _venueTypeOptionRepository.GetForGroupIdAsync(groupVenue.GroupId, ct);
        if (!validVenueTypes.Any(vto => vto.VenueTypeOptionId == request.VenueTypeOptionId))
        {
            _logger.LogWarning("Invalid venue type ID {VenueTypeOptionId} provided for group {GroupId}.", request.VenueTypeOptionId, groupVenue.GroupId);
            return new AddGroupVenueResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "Invalid venue type provided."
            };
        }

        await _groupVenueRepository.UpdateGroupVenueAsync(groupVenueId, request, ct);

        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("GroupVenue [{GroupVenueId}] updated successfully.", groupVenueId);

        return new CommonResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "GroupVenue updated successfully.",
        };
    }
}
