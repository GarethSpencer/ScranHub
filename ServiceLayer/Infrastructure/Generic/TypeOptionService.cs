using Microsoft.Extensions.Logging;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Abstractions.Generic;
using ServiceLayer.Abstractions.Generic;
using System.Net;
using Utilities.Helpers;
using Utilities.Models.Requests.Options;
using Utilities.Models.Responses.Generic;
using Utilities.Models.Responses.Options;
using Utilities.Token;

namespace ServiceLayer.Infrastructure.Generic;

public abstract class TypeOptionService<TTypeOptionRepository>(ITokenData tokenData,
    TTypeOptionRepository typeOptionRepository,
    ILogger logger,
    IUserGroupRepository userGroupRepository,
    IGroupRepository groupRepository,
    IUnitOfWork unitOfWork)
    : ITypeOptionService
    where TTypeOptionRepository : ITypeOptionRepository
{
    protected readonly TTypeOptionRepository _typeOptionRepository = typeOptionRepository;
    protected readonly ITokenData _tokenData = tokenData;
    protected readonly ILogger _logger = logger;
    protected readonly IUserGroupRepository _userGroupRepository = userGroupRepository;
    protected readonly IGroupRepository _groupRepository = groupRepository;
    protected readonly IUnitOfWork _unitOfWork = unitOfWork;

    public abstract Task<CommonResponse> SetGroupCustomOptionsAsync(SetOptionsRequest request, CancellationToken ct);

    public abstract Task<CommonResponse> RemoveGroupCustomOptionsAsync(Guid groupId, CancellationToken ct);
    
    public async Task<CommonResponse> AddOptionAsync(SetOptionRequest request, CancellationToken ct)
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
                Message = "You do not have permission to set options for this group."
            }.WithResponseLog(_logger, callingUserId);
        }

        var usingDefaults = await _typeOptionRepository.IsGroupUsingDefaultValues(request.GroupId, ct);
        if (usingDefaults)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "The group is not using custom options so they cannot be amended."
            }.WithResponseLog(_logger, callingUserId);
        }

        var currentOptions = await _typeOptionRepository.GetForGroupIdAsync(request.GroupId, ct);
        if (currentOptions.Any(x => string.Equals(x.Label, request.Label, StringComparison.OrdinalIgnoreCase)))
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Conflict,
                Message = "An option with that label already exists for this group."
            }.WithResponseLog(_logger, callingUserId);
        }

        var optionId = await _typeOptionRepository.AddAsync(request, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new SetOptionResponse
        {
            StatusCode = HttpStatusCode.Created,
            Message = "New type option added successfully.",
            OptionsId = optionId
        }.WithResponseLog(_logger, callingUserId, $"New type option [{optionId}] added successfully.");
    }

    public async Task<CommonResponse> UpdateOptionAsync(Guid optionId, UpdateOptionRequest request, CancellationToken ct)
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
        var option = await _typeOptionRepository.GetByIdAsync(optionId, ct);
        if (option == null || option.GroupId == null)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "The option was not found."
            }.WithResponseLog(_logger, callingUserId);
        }

        var group = await _groupRepository.GetDetailsByIdAsync(option.GroupId.Value, ct);
        if (group!.Active != true)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "The group was not found."
            }.WithResponseLog(_logger, callingUserId);
        }

        var isUserInGroup = await _userGroupRepository.IsUserInGroupAsync(option.GroupId.Value, callingUserId, ct);
        if (!isUserInGroup)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "You do not have permission to update options for this group."
            }.WithResponseLog(_logger, callingUserId);
        }

        var currentOptions = await _typeOptionRepository.GetForGroupIdAsync(option.GroupId.Value, ct);
        if (currentOptions.Any(x => x.OptionId != optionId && string.Equals(x.Label, request.Label, StringComparison.OrdinalIgnoreCase)))
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Conflict,
                Message = "An option with that label already exists for this group."
            }.WithResponseLog(_logger, callingUserId);
        }

        await _typeOptionRepository.UpdateAsync(optionId, request.Label, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new CommonResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Type option updated successfully.",
        }.WithResponseLog(_logger, callingUserId, $"Type option [{optionId}] updated successfully.");
    }

    public async Task<CommonResponse> DeleteOptionAsync(Guid optionId, CancellationToken ct)
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
        var option = await _typeOptionRepository.GetByIdAsync(optionId, ct);
        if (option == null || option.GroupId == null)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "The option was not found."
            }.WithResponseLog(_logger, callingUserId);
        }

        var group = await _groupRepository.GetDetailsByIdAsync(option.GroupId.Value, ct);
        if (group!.Active != true)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "The group was not found."
            }.WithResponseLog(_logger, callingUserId);
        }

        var isUserInGroup = await _userGroupRepository.IsUserInGroupAsync(option.GroupId.Value, callingUserId, ct);
        if (!isUserInGroup)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "You do not have permission to delete options for this group."
            }.WithResponseLog(_logger, callingUserId);
        }

        var isOptionUsed = await _groupRepository.AreAnyVenuesUsingOptionIdAsync(option.GroupId.Value, optionId, ct);
        if (isOptionUsed)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "Cannot delete this option because it is applied to a venue. Amend venues to other types first."
            }.WithResponseLog(_logger, callingUserId);
        }

        await _typeOptionRepository.DeleteAsync(optionId, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new CommonResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Type option deleted successfully.",
        }.WithResponseLog(_logger, callingUserId);
    }

    public async Task<CommonResponse> GetGroupTypeOptionsAsync(Guid? groupId, CancellationToken ct)
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
        if (groupId != null)
        {
            var group = await _groupRepository.GetDetailsByIdAsync(groupId.Value, ct);
            if (group?.Active != true)
            {
                return new CommonResponse
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "The group was not found."
                }.WithResponseLog(_logger, callingUserId);
            }

            var isUserInGroup = await _userGroupRepository.IsUserInGroupAsync(groupId.Value, callingUserId, ct);
            if (!isUserInGroup)
            {
                return new CommonResponse
                {
                    StatusCode = HttpStatusCode.Forbidden,
                    Message = "You do not have permission to view options for this group."
                }.WithResponseLog(_logger, callingUserId);
            }
        }

        var options = await _typeOptionRepository.GetForGroupIdAsync(groupId, ct);

        return new GetTypeOptionsResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Type options retrieved successfully.",
            Options = options
        }.WithResponseLog(_logger, callingUserId);
    }

    public async Task<CommonResponse> GetTypeOptionAsync(Guid optionId, CancellationToken ct)
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
        var option = await _typeOptionRepository.GetByIdAsync(optionId, ct);
        if (option == null)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "The option was not found."
            }.WithResponseLog(_logger, callingUserId);
        }

        if (option.GroupId == null)
        {
            return new GetTypeOptionResponse
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Default option retrieved successfully.",
                Option = option
            }.WithResponseLog(_logger, callingUserId);
        }

        var group = await _groupRepository.GetDetailsByIdAsync(option.GroupId.Value, ct);
        if (group!.Active != true)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "The group was not found."
            }.WithResponseLog(_logger, callingUserId);
        }

        var isUserInGroup = await _userGroupRepository.IsUserInGroupAsync(option.GroupId.Value, callingUserId, ct);
        if (!isUserInGroup)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "You do not have permission to view options for this group."
            }.WithResponseLog(_logger, callingUserId);
        }

        return new GetTypeOptionResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Option retrieved successfully.",
            Option = option
        }.WithResponseLog(_logger, callingUserId);
    }
}
