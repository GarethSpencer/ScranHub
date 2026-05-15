using Microsoft.Extensions.Logging;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Abstractions.Generic;
using ServiceLayer.Abstractions.Generic;
using System.Net;
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

    public abstract Task<SetOptionsResponse> SetGroupCustomOptionsAsync(SetOptionsRequest request, CancellationToken ct);

    public abstract Task<CommonResponse> RemoveGroupCustomOptionsAsync(Guid groupId, CancellationToken ct);
    
    public async Task<SetOptionResponse> AddOptionAsync(SetOptionRequest request, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("AddOptionAsync called with no authenticated user.");
            return new SetOptionResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            };
        }

        var userId = _tokenData.UserId!.Value;
        var isUserInGroup = await _userGroupRepository.IsUserInGroupAsync(request.GroupId, userId, ct);

        if (!isUserInGroup)
        {
            _logger.LogWarning("AddOptionAsync called by user {UserId} who is not in group {GroupId}.", userId, request.GroupId);
            return new SetOptionResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "You do not have permission to set options for this group."
            };
        }

        var group = await _groupRepository.GetDetailsByIdAsync(request.GroupId, ct);
        if (group?.Active != true)
        {
            _logger.LogWarning("AddOptionAsync called for inactive or non-existent group {GroupId} by user {UserId}.", request.GroupId, userId);
            return new SetOptionResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "The group does not exist or is not active."
            };
        }

        var usingDefaults = await _typeOptionRepository.IsGroupUsingDefaultValues(request.GroupId, ct);
        if (usingDefaults)
        {
            _logger.LogWarning("AddOptionAsync called for group {GroupId} by user {UserId} when group is using default options.", request.GroupId, userId);
            return new SetOptionResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "The group is not using custom options so they cannot be amended."
            };
        }

        var currentOptions = await _typeOptionRepository.GetForGroupIdAsync(request.GroupId, ct);
        if (currentOptions.Any(x => string.Equals(x.Label, request.Label, StringComparison.OrdinalIgnoreCase)))
        {
            _logger.LogWarning("AddOptionAsync called for group {GroupId} by user {UserId} with duplicate label.", request.GroupId, userId);
            return new SetOptionResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "An option with that label already exists for this group."
            };
        }

        var optionId = await _typeOptionRepository.AddAsync(request, ct);

        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("User {UserId} added type option for group {GroupId}.", userId, request.GroupId);

        return new SetOptionResponse
        {
            StatusCode = HttpStatusCode.Created,
            Message = "New type option added successfully.",
            OptionsId = optionId
        };
    }

    public async Task<CommonResponse> UpdateOptionAsync(Guid optionId, UpdateOptionRequest request, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("UpdateOptionAsync called with no authenticated user.");
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            };
        }

        var userId = _tokenData.UserId!.Value;
        var option = await _typeOptionRepository.GetByIdAsync(optionId, ct);
        if (option == null || option.GroupId == null)
        {
            _logger.LogWarning("UpdateOptionAsync called by user {UserId} for option {OptionId} which cannot be updated.", userId, optionId);
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "The option does not exist or cannot be updated."
            };
        }

        var isUserInGroup = await _userGroupRepository.IsUserInGroupAsync(option.GroupId.Value, userId, ct);
        if (!isUserInGroup)
        {
            _logger.LogWarning("UpdateOptionAsync called by user {UserId} who is not in group {GroupId}.", userId, option.GroupId);
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "You do not have permission to update options for this group."
            };
        }

        var group = await _groupRepository.GetDetailsByIdAsync(option.GroupId.Value, ct);
        if (group?.Active != true)
        {
            _logger.LogWarning("UpdateOptionAsync called for inactive or non-existent group {GroupId} by user {UserId}.", option.GroupId, userId);
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "The group does not exist or is not active."
            };
        }

        var currentOptions = await _typeOptionRepository.GetForGroupIdAsync(option.GroupId.Value, ct);
        if (currentOptions.Any(x => x.OptionId != optionId && string.Equals(x.Label, request.Label, StringComparison.OrdinalIgnoreCase)))
        {
            _logger.LogWarning("UpdateOptionAsync called for group {GroupId} by user {UserId} with duplicate label.", option.GroupId, userId);
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "An option with that label already exists for this group."
            };
        }

        await _typeOptionRepository.UpdateAsync(optionId, request.Label, ct);

        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("User {UserId} updated type option for group {GroupId}.", userId, option.GroupId);

        return new CommonResponse
        {
            StatusCode = HttpStatusCode.Created,
            Message = $"Type option updated successfully.",
        };
    }

    public async Task<CommonResponse> DeleteOptionAsync(Guid optionId, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("DeleteOptionAsync called with no authenticated user.");
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            };
        }

        var userId = _tokenData.UserId!.Value;
        var option = await _typeOptionRepository.GetByIdAsync(optionId, ct);
        if (option == null || option.GroupId == null)
        {
            _logger.LogWarning("DeleteOptionAsync called by user {UserId} for option {OptionId} which cannot be deleted.", userId, optionId);
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "The option does not exist or cannot be deleted."
            };
        }

        var isUserInGroup = await _userGroupRepository.IsUserInGroupAsync(option.GroupId.Value, userId, ct);
        if (!isUserInGroup)
        {
            _logger.LogWarning("DeleteOptionAsync called by user {UserId} who is not in group {GroupId}.", userId, option.GroupId);
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "You do not have permission to delete options for this group."
            };
        }

        var group = await _groupRepository.GetDetailsByIdAsync(option.GroupId.Value, ct);
        if (group?.Active != true)
        {
            _logger.LogWarning("DeleteOptionAsync called for inactive or non-existent group {GroupId} by user {UserId}.", option.GroupId, userId);
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "The group does not exist or is not active."
            };
        }

        var isOptionUsed = await _groupRepository.AreAnyVenuesUsingOptionIdAsync(option.GroupId.Value, optionId, ct);
        if (isOptionUsed)
        {
            _logger.LogWarning("DeleteOptionAsync called for option {OptionId} by user {UserId} which is currently in use by a venue.", optionId, userId);
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "Cannot delete this option because it is applied to a venue. Amend venues to other types first."
            };
        }

        await _typeOptionRepository.DeleteAsync(optionId, ct);

        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("User {UserId} deleted option {OptionId}.", userId, optionId);

        return new CommonResponse
        {
            StatusCode = HttpStatusCode.Created,
            Message = "Type option deleted successfully.",
        };
    }

    public async Task<GetTypeOptionsResponse> GetGroupTypeOptionsAsync(Guid? groupId, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("GetGroupTypeOptionsAsync called with no authenticated user.");
            return new GetTypeOptionsResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            };
        }

        var userId = _tokenData.UserId!.Value;

        if (groupId != null)
        {
            var isUserInGroup = await _userGroupRepository.IsUserInGroupAsync(groupId.Value, userId, ct);

            if (!isUserInGroup)
            {
                _logger.LogWarning("GetGroupTypeOptionsAsync called by user {UserId} who is not in group {GroupId}.", userId, groupId);
                return new GetTypeOptionsResponse
                {
                    StatusCode = HttpStatusCode.Forbidden,
                    Message = "You do not have permission to view options for this group."
                };
            }

            var group = await _groupRepository.GetDetailsByIdAsync(groupId.Value, ct);
            if (group?.Active != true)
            {
                _logger.LogWarning("GetGroupTypeOptionsAsync called for inactive or non-existent group {GroupId} by user {UserId}.", groupId, userId);
                return new GetTypeOptionsResponse
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "The group does not exist or is not active."
                };
            }
        }

        var options = await _typeOptionRepository.GetForGroupIdAsync(groupId, ct);
        _logger.LogInformation("User {UserId} retrieved type options for group {GroupId}.", userId, groupId);

        return new GetTypeOptionsResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Type options retrieved successfully.",
            Options = options
        };
    }

    public async Task<GetTypeOptionResponse> GetTypeOptionAsync(Guid optionId, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("GetTypeOptionAsync called with no authenticated user.");
            return new GetTypeOptionResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            };
        }

        var userId = _tokenData.UserId!.Value;
        var option = await _typeOptionRepository.GetByIdAsync(optionId, ct);

        if (option == null)
        {
            _logger.LogWarning("GetTypeOptionAsync called by user {UserId} for option {OptionId} which does not exist.", userId, optionId);
            return new GetTypeOptionResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "The option does not exist."
            };
        }

        if (option.GroupId == null)
        {
            _logger.LogInformation("User {UserId} retrieved default option {OptionId}.", userId, optionId);
            return new GetTypeOptionResponse
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Option retrieved successfully.",
                Option = option
            };
        }

        var isUserInGroup = await _userGroupRepository.IsUserInGroupAsync(option.GroupId.Value, userId, ct);

        if (!isUserInGroup)
        {
            _logger.LogWarning("GetTypeOptionAsync called by user {UserId} who is not in group {GroupId}.", userId, option.GroupId);
            return new GetTypeOptionResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "You do not have permission to view options for this group."
            };
        }

        var group = await _groupRepository.GetDetailsByIdAsync(option.GroupId.Value, ct);
        if (group?.Active != true)
        {
            _logger.LogWarning("GetTypeOptionAsync called for inactive or non-existent group {GroupId} by user {UserId}.", option.GroupId, userId);
            return new GetTypeOptionResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "The group does not exist or is not active."
            };
        }

        return new GetTypeOptionResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Option retrieved successfully.",
            Option = option
        };
    }
}
