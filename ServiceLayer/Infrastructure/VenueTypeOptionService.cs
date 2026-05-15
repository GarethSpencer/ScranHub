using Microsoft.Extensions.Logging;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Abstractions.Generic;
using ServiceLayer.Abstractions;
using ServiceLayer.Infrastructure.Generic;
using System.Net;
using Utilities.Models.Requests.Options;
using Utilities.Models.Responses.Generic;
using Utilities.Models.Responses.Options;
using Utilities.Token;

namespace ServiceLayer.Infrastructure;

public class VenueTypeOptionService(ITokenData tokenData,
    IVenueTypeOptionRepository venueTypeOptionRepository,
    ILogger<VenueTypeOptionService> logger,
    IUserGroupRepository userGroupRepository,
    IGroupRepository groupRepository,
    IUnitOfWork unitOfWork) : TypeOptionService<IVenueTypeOptionRepository>
    (tokenData, venueTypeOptionRepository, logger, userGroupRepository, groupRepository, unitOfWork),
    IVenueTypeOptionService
{
    public override async Task<SetOptionsResponse> SetGroupCustomOptionsAsync(SetOptionsRequest request, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("SetGroupCustomOptionsAsync called with no authenticated user.");
            return new SetOptionsResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            };
        }

        var userId = _tokenData.UserId!.Value;
        var isUserInGroup = await _userGroupRepository.IsUserInGroupAsync(request.GroupId, userId, ct);

        if (!isUserInGroup)
        {
            _logger.LogWarning("SetGroupCustomOptionsAsync called by user {UserId} who is not in group {GroupId}.", userId, request.GroupId);
            return new SetOptionsResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "You do not have permission to set options for this group."
            };
        }

        var group = await _groupRepository.GetDetailsByIdAsync(request.GroupId, ct);
        if (group?.Active != true)
        {
            _logger.LogWarning("SetGroupCustomOptionsAsync called for inactive or non-existent group {GroupId} by user {UserId}.", request.GroupId, userId);
            return new SetOptionsResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "The group does not exist or is not active."
            };
        }

        var usingDefaults = await _typeOptionRepository.IsGroupUsingDefaultValues(request.GroupId, ct);
        if (!usingDefaults)
        {
            _logger.LogWarning("SetGroupCustomOptionsAsync called for group {GroupId} by user {UserId} when group already has custom options.", request.GroupId, userId);
            return new SetOptionsResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "The group is already using custom options."
            };
        }

        var optionIds = await _typeOptionRepository.AddRangeAsync(request, ct);
        await _groupRepository.UnsetVenueTypesForGroupAsync(request.GroupId, ct);

        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("User {UserId} set group-specific options for group {GroupId}.", userId, request.GroupId);

        return new SetOptionsResponse
        {
            StatusCode = HttpStatusCode.Created,
            Message = "Custom ratings were created and mapped successfully.",
            OptionsIds = optionIds
        };
    }

    public override async Task<CommonResponse> RemoveGroupCustomOptionsAsync(Guid groupId, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("RemoveGroupCustomOptionsAsync called with no authenticated user.");
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            };
        }

        var userId = _tokenData.UserId!.Value;
        var isUserInGroup = await _userGroupRepository.IsUserInGroupAsync(groupId, userId, ct);

        if (!isUserInGroup)
        {
            _logger.LogWarning("RemoveGroupCustomOptionsAsync called by user {UserId} who is not in group {GroupId}.", userId, groupId);
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "You do not have permission to remove options for this group."
            };
        }

        var group = await _groupRepository.GetDetailsByIdAsync(groupId, ct);
        if (group?.Active != true)
        {
            _logger.LogWarning("RemoveGroupCustomOptionsAsync called for inactive or non-existent group {GroupId} by user {UserId}.", groupId, userId);
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "The group does not exist or is not active."
            };
        }

        var usingDefaults = await _typeOptionRepository.IsGroupUsingDefaultValues(groupId, ct);
        if (usingDefaults)
        {
            _logger.LogWarning("RemoveGroupCustomOptionsAsync called for group {GroupId} by user {UserId} when group is already using default options.", groupId, userId);
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "The group is already using default options."
            };
        }

        await _typeOptionRepository.RemoveCustomTypesForGroupAsync(groupId, ct);
        await _groupRepository.UnsetVenueTypesForGroupAsync(groupId, ct);

        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("User {UserId} removed group-specific options for group {GroupId}.", userId, groupId);

        return new CommonResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Custom ratings were removed successfully.",
        };
    }
}
