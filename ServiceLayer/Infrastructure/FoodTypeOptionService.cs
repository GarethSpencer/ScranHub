using Microsoft.Extensions.Logging;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Abstractions.Generic;
using ServiceLayer.Abstractions;
using ServiceLayer.Infrastructure.Generic;
using System.Net;
using Utilities.Helpers;
using Utilities.Models.Requests.Options;
using Utilities.Models.Responses.Generic;
using Utilities.Models.Responses.Options;
using Utilities.Token;

namespace ServiceLayer.Infrastructure;

public class FoodTypeOptionService(ITokenData tokenData,
    IFoodTypeOptionRepository foodTypeOptionRepository,
    ILogger<FoodTypeOptionService> logger,
    IUserGroupRepository userGroupRepository,
    IGroupRepository groupRepository,
    IUnitOfWork unitOfWork) : TypeOptionService<IFoodTypeOptionRepository>
    (tokenData, foodTypeOptionRepository, logger, userGroupRepository, groupRepository, unitOfWork),
    IFoodTypeOptionService
{
    public override async Task<CommonResponse> SetGroupCustomOptionsAsync(SetOptionsRequest request, CancellationToken ct)
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
        var group = await _groupRepository.GetDetailsByIdAsync(request.GroupId, ct);
        if (group?.Active != true)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "The group was not found."
            }.WithResponseLog(_logger, callingUserId);
        }

        var isUserInGroup = await _userGroupRepository.IsUserInGroupAsync(request.GroupId, callingUserId, ct);
        if (!isUserInGroup)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "You are not a member of this group."
            }.WithResponseLog(_logger, callingUserId);
        }

        var usingDefaults = await _typeOptionRepository.IsGroupUsingDefaultValues(request.GroupId, ct);
        if (!usingDefaults)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "The group is already using custom options."
            }.WithResponseLog(_logger, callingUserId);
        }

        var optionIds = await _typeOptionRepository.AddRangeAsync(request, ct);
        await _groupRepository.UnsetFoodTypesForGroupAsync(request.GroupId, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new SetOptionsResponse
        {
            StatusCode = HttpStatusCode.Created,
            Message = "Custom ratings were created and mapped successfully.",
            OptionsIds = optionIds
        }.WithResponseLog(_logger, callingUserId, $"Custom ratings [{string.Join(", ", optionIds)}] were created and mapped successfully.");
    }

    public override async Task<CommonResponse> RemoveGroupCustomOptionsAsync(Guid groupId, CancellationToken ct)
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
        var group = await _groupRepository.GetDetailsByIdAsync(groupId, ct);
        if (group?.Active != true)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "The group was not found."
            }.WithResponseLog(_logger, callingUserId);
        }

        var isUserInGroup = await _userGroupRepository.IsUserInGroupAsync(groupId, callingUserId, ct);
        if (!isUserInGroup)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "You are not a member of this group."
            }.WithResponseLog(_logger, callingUserId);
        }

        var usingDefaults = await _typeOptionRepository.IsGroupUsingDefaultValues(groupId, ct);
        if (usingDefaults)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "The group is already using default options."
            }.WithResponseLog(_logger, callingUserId);
        }

        await _typeOptionRepository.RemoveCustomTypesForGroupAsync(groupId, ct);
        await _groupRepository.UnsetFoodTypesForGroupAsync(groupId, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new CommonResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Custom ratings were removed successfully.",
        }.WithResponseLog(_logger, callingUserId);
    }
}
