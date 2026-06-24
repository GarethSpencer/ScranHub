using Microsoft.Extensions.Logging;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Abstractions.Generic;
using ServiceLayer.Abstractions;
using System.Net;
using Utilities.Helpers;
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

    public async Task<CommonResponse> GetGroupVenueAsync(Guid groupVenueId, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            }.WithResponseLog(_logger);
        }

        var callingUserId = _tokenData.UserId.Value;
        var groupVenue = await _groupVenueRepository.GetByIdAsync(groupVenueId, callingUserId, ct);
        if (groupVenue == null)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Venue not found."
            }.WithResponseLog(_logger, callingUserId);
        }

        var isAdmin = await _userRepository.IsUserAdminAsync(callingUserId, ct);
        var isUserInGroup = await _userGroupRepository.IsUserInGroupAsync(groupVenue.GroupId, callingUserId, ct);
        if (!isUserInGroup && !isAdmin)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "User is not in group."
            }.WithResponseLog(_logger, callingUserId);
        }

        return new GetGroupVenueResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Venue returned successfully.",
            GroupVenue = groupVenue
        }.WithResponseLog(_logger, callingUserId);
    }

    public async Task<CommonResponse> GetAllVenuesForGroupAsync(Guid groupId, SortableGroupVenueRequest request, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            }.WithResponseLog(_logger);
        }

        var callingUserId = _tokenData.UserId!.Value;
        var isGroupActive = await _groupRepository.ExistsAsync(x => x.GroupId == groupId && x.Active, ct);
        if (!isGroupActive)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Group not found or inactive."
            }.WithResponseLog(_logger, callingUserId);
        }

        var isAdmin = await _userRepository.IsUserAdminAsync(callingUserId, ct);
        var isUserInGroup = await _userGroupRepository.IsUserInGroupAsync(groupId, callingUserId, ct);
        if (!isAdmin && !isUserInGroup)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "You cannot retrieve venues in this group."
            }.WithResponseLog(_logger, callingUserId);
        }

        var (groupVenues, totalCount) = await _groupVenueRepository.GetByGroupIdAsync(groupId, request, callingUserId, ct);

        return new GetGroupVenuesResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Venues returned successfully.",
            GroupVenues = groupVenues,
            TotalCount = totalCount
        }.WithResponseLog(_logger, callingUserId);
    }

    public async Task<CommonResponse> SearchGroupVenuesAsync(Guid groupId, SearchGroupVenueRequest request, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            }.WithResponseLog(_logger);
        }

        var callingUserId = _tokenData.UserId!.Value;
        var isGroupActive = await _groupRepository.ExistsAsync(x => x.GroupId == groupId && x.Active, ct);
        if (!isGroupActive)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Group not found or inactive."
            }.WithResponseLog(_logger, callingUserId);
        }

        var isAdmin = await _userRepository.IsUserAdminAsync(callingUserId, ct);
        var isUserInGroup = await _userGroupRepository.IsUserInGroupAsync(groupId, callingUserId, ct);
        if (!isAdmin && !isUserInGroup)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "You cannot search venues in this group."
            }.WithResponseLog(_logger, callingUserId);
        }

        var (groupVenues, totalCount) = await _groupVenueRepository.SearchByNameAsync(groupId, request, callingUserId,  ct);

        return new GetGroupVenuesResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Venues returned successfully.",
            GroupVenues = groupVenues,
            TotalCount = totalCount
        }.WithResponseLog(_logger, callingUserId);
    }

    public async Task<CommonResponse> CreateGroupVenueAsync(CreateGroupVenueRequest request, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            }.WithResponseLog(_logger);
        }

        var callingUserId = _tokenData.UserId.Value;
        var isGroupActive = await _groupRepository.ExistsAsync(x => x.GroupId == request.GroupId && x.Active, ct);
        if (!isGroupActive)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Group not found or inactive."
            }.WithResponseLog(_logger, callingUserId);
        }

        var isUserInGroup = await _userGroupRepository.IsUserInGroupAsync(request.GroupId, callingUserId, ct);
        if (!isUserInGroup)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "User is not in group."
            }.WithResponseLog(_logger, callingUserId);
        }

        var validFoodTypes = await _foodTypeOptionRepository.GetForGroupIdAsync(request.GroupId, ct);
        if (request.FoodTypeOptionId != null && !validFoodTypes.Any(fto => fto.OptionId == request.FoodTypeOptionId.Value))
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "Invalid food type provided."
            }.WithResponseLog(_logger, callingUserId);
        }

        var validVenueTypes = await _venueTypeOptionRepository.GetForGroupIdAsync(request.GroupId, ct);
        if (request.VenueTypeOptionId != null && !validVenueTypes.Any(vto => vto.OptionId == request.VenueTypeOptionId.Value))
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "Invalid venue type provided."
            }.WithResponseLog(_logger, callingUserId);
        }

        var doesGroupVenueNameExist = await _groupVenueRepository.ExistsAsync(x => x.GroupId == request.GroupId && x.VenueName == request.VenueName, ct);
        if (doesGroupVenueNameExist)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Conflict,
                Message = "Venue with this name already exists in the group."
            }.WithResponseLog(_logger, callingUserId);
        }

        var groupVenueId = await _groupVenueRepository.CreateAsync(request, ct);

        await _unitOfWork.SaveChangesAsync(ct);

        return new AddGroupVenueResponse
        {
            StatusCode = HttpStatusCode.Created,
            Message = "Venue created successfully.",
            GroupVenueId = groupVenueId,
        }.WithResponseLog(_logger, callingUserId, $"Venue [{groupVenueId}] created successfully.");
    }

    public async Task<CommonResponse> UpdateGroupVenueAsync(Guid groupVenueId, UpdateGroupVenueRequest request, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            }.WithResponseLog(_logger);
        }

        var callingUserId = _tokenData.UserId.Value;
        var groupVenue = await _groupVenueRepository.GetByIdAsync(groupVenueId, callingUserId, ct);
        if (groupVenue == null)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Venue not found."
            }.WithResponseLog(_logger, callingUserId);
        }

        var isUserInGroup = await _userGroupRepository.IsUserInGroupAsync(groupVenue.GroupId, callingUserId, ct);
        if (!isUserInGroup)
        {
            return new AddGroupVenueResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "User is not in group."
            }.WithResponseLog(_logger, callingUserId);
        }

        var validFoodTypes = await _foodTypeOptionRepository.GetForGroupIdAsync(groupVenue.GroupId, ct);
        if (request.FoodTypeOptionId != null && !validFoodTypes.Any(fto => fto.OptionId == request.FoodTypeOptionId.Value))
        {
            return new AddGroupVenueResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "Invalid food type provided."
            }.WithResponseLog(_logger, callingUserId);
        }

        var validVenueTypes = await _venueTypeOptionRepository.GetForGroupIdAsync(groupVenue.GroupId, ct);
        if (request.VenueTypeOptionId != null && !validVenueTypes.Any(vto => vto.OptionId == request.VenueTypeOptionId.Value))
        {
            return new AddGroupVenueResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "Invalid venue type provided."
            }.WithResponseLog(_logger, callingUserId);
        }

        if (!String.Equals(groupVenue.VenueName, request.VenueName, StringComparison.OrdinalIgnoreCase))
        {
            var groupVenueNameExists = await _groupVenueRepository.ExistsAsync(x => x.GroupId == groupVenue.GroupId && x.VenueName == request.VenueName, ct);
            if (groupVenueNameExists)
            {
                return new CommonResponse
                {
                    StatusCode = HttpStatusCode.Conflict,
                    Message = $"Venue with name {request.VenueName} already exists in this group."
                }.WithResponseLog(_logger, callingUserId);
            }
        }

        await _groupVenueRepository.UpdateAsync(groupVenueId, request, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new CommonResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "GroupVenue updated successfully.",
        }.WithResponseLog(_logger, callingUserId);
    }

    public async Task<CommonResponse> DeleteGroupVenueAsync(Guid groupVenueId, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            }.WithResponseLog(_logger);
        }

        var callingUserId = _tokenData.UserId!.Value;
        var groupVenue = await _groupVenueRepository.GetByIdAsync(groupVenueId, callingUserId, ct);
        if (groupVenue == null)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Venue not found."
            }.WithResponseLog(_logger, callingUserId);
        }

        var isAdmin = await _userRepository.IsUserAdminAsync(callingUserId, ct);
        var isUserInGroup = await _userGroupRepository.IsUserInGroupAsync(groupVenue.GroupId, callingUserId, ct);
        if (!isAdmin && !isUserInGroup)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "You are not an admin or group member and cannot delete this venue."
            }.WithResponseLog(_logger, callingUserId);
        }

        await _groupVenueRepository.DeleteAsync(groupVenueId, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new CommonResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Successfully deleted the venue."
        }.WithResponseLog(_logger, callingUserId, $"Successfully deleted venue [{groupVenueId}]");
    }
}
