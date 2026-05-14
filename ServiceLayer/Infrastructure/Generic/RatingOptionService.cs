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

public abstract class RatingOptionService<TRatingRepository, TRatingOptionRepository>(ITokenData tokenData,
    TRatingRepository ratingRepository,
    TRatingOptionRepository ratingOptionRepository,
    ILogger logger,
    IUserGroupRepository userGroupRepository,
    IGroupRepository groupRepository,
    IUnitOfWork unitOfWork)
    : IRatingOptionService
    where TRatingRepository : IRatingRepository
    where TRatingOptionRepository : IRatingOptionRepository
{
    protected readonly TRatingRepository _ratingRepository = ratingRepository;
    protected readonly TRatingOptionRepository _ratingOptionRepository = ratingOptionRepository;
    protected readonly ITokenData _tokenData = tokenData;
    protected readonly ILogger _logger = logger;
    protected readonly IUserGroupRepository _userGroupRepository = userGroupRepository;
    protected readonly IGroupRepository _groupRepository = groupRepository;
    protected readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<SetOptionsResponse> SetGroupCustomOptionsAsync(SetOptionsRequest request, CancellationToken ct)
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

        var usingDefaults = await _ratingOptionRepository.IsGroupUsingDefaultValues(request.GroupId, ct);
        if (!usingDefaults)
        {
            _logger.LogWarning("SetGroupCustomOptionsAsync called for group {GroupId} by user {UserId} when group already has custom options.", request.GroupId, userId);
            return new SetOptionsResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "The group is already using custom options."
            };
        }

        var ratingOptionsInUse = await _ratingRepository.GetDistinctRatingsGivenForGroupAsync(request.GroupId, ct);
        if (ratingOptionsInUse.Count() > request.Labels.Length)
        {
            _logger.LogWarning("SetGroupCustomOptionsAsync called for group {GroupId} by user {UserId} without enough labels.", request.GroupId, userId);
            return new SetOptionsResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = $"You need to provide at least {ratingOptionsInUse.Count()} labels to map the current options."
            };
        }

        var defaults = await _ratingOptionRepository.GetDefaultsAsync(ct);
        var optionIds = await _ratingOptionRepository.AddRangeAsync(request, ct);
        var squashNeeded = false;

        if (defaults.Count() == request.Labels.Length)
        {
            // We have as many default options as new options, so we can map cleanly
            await _ratingRepository.RemapRatingsMaintainDisplayOrderAsync(request.GroupId, optionIds, ct);
        }
        else if (ratingOptionsInUse.Count() == request.Labels.Length)
        {
            // We have as many default ratings being used as new ratings so we can map if we don't leave gaps
            await _ratingRepository.RemapRatingsSquashDisplayOrderAsync(request.GroupId, optionIds, ct);
            squashNeeded = true;
        }
        else if (ratingOptionsInUse.OrderByDescending(x => x.DisplayOrder).First().DisplayOrder <= request.Labels.Length)
        {
            // The current highest display order is less than or equal to the number of new options, so we can leave gaps
            await _ratingRepository.RemapRatingsMaintainDisplayOrderAsync(request.GroupId, optionIds, ct);
        }
        else
        {
            // The current highest display order is greater than the number of new options, but we can make them fit if we squash the order
            await _ratingRepository.RemapRatingsSquashDisplayOrderAsync(request.GroupId, optionIds, ct);
            squashNeeded = true;
        }

        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("User {UserId} set group-specific options for group {GroupId}.", userId, request.GroupId);

        return new SetOptionsResponse
        {
            StatusCode = HttpStatusCode.Created,
            Message = "Custom ratings were created and mapped successfully" + (squashNeeded ? ", but existing ratings were squashed." : "."),
            OptionsIds = optionIds
        };
    }

    public async Task<CommonResponse> RemoveGroupCustomOptionsAsync(Guid groupId, CancellationToken ct)
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

        var usingDefaults = await _ratingOptionRepository.IsGroupUsingDefaultValues(groupId, ct);
        if (usingDefaults)
        {
            _logger.LogWarning("RemoveGroupCustomOptionsAsync called for group {GroupId} by user {UserId} when group is already using default options.", groupId, userId);
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "The group is already using default options."
            };
        }

        var ratingOptionsInUse = await _ratingRepository.GetDistinctRatingsGivenForGroupAsync(groupId, ct);
        var defaultOptions = await _ratingOptionRepository.GetDefaultsAsync(ct);

        if (ratingOptionsInUse.Count() > defaultOptions.Count())
        {
            _logger.LogWarning("RemoveGroupCustomOptionsAsync called for group {GroupId} by user {UserId} without enough labels.", groupId, userId);
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = $"Cannot remove custom options. The group is using {ratingOptionsInUse.Count()} labels but there are only {defaultOptions.Count()} default options."
            };
        }

        var currentOptions = await _ratingOptionRepository.GetForGroupIdAsync(groupId, ct);
        var squashNeeded = false;

        if (currentOptions.Count() == defaultOptions.Count())
        {
            // We have as many default options as current options, so we can map cleanly
            await _ratingRepository.RemapRatingsMaintainDisplayOrderAsync(groupId, defaultOptions.Select(x => x.OptionId), ct);
        }
        else if (defaultOptions.Count() == ratingOptionsInUse.Count())
        {
            // We have as many default options being used as new options so we can map if we don't leave gaps
            await _ratingRepository.RemapRatingsSquashDisplayOrderAsync(groupId, defaultOptions.Select(x => x.OptionId), ct);
            squashNeeded = true;
        }
        else if (ratingOptionsInUse.OrderByDescending(x => x.DisplayOrder).First().DisplayOrder <= defaultOptions.Count())
        {
            // The current highest display order is less than or equal to the number of default options, so we can leave gaps
            await _ratingRepository.RemapRatingsMaintainDisplayOrderAsync(groupId, defaultOptions.Select(x => x.OptionId), ct);
        }
        else
        {
            // The current highest display order is greater than the number of new options, but we can make them fit if we squash the order
            await _ratingRepository.RemapRatingsSquashDisplayOrderAsync(groupId, defaultOptions.Select(x => x.OptionId), ct);
            squashNeeded = true;
        }

        await _ratingOptionRepository.RemoveCustomRatingsForGroupAsync(groupId, ct);

        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("User {UserId} removed group-specific options for group {GroupId}.", userId, groupId);

        return new CommonResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Custom ratings were removed successfully" + (squashNeeded ? ", but ratings were squashed to fit the default options." : "."),
        };
    }

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

        var usingDefaults = await _ratingOptionRepository.IsGroupUsingDefaultValues(request.GroupId, ct);
        if (usingDefaults)
        {
            _logger.LogWarning("AddOptionAsync called for group {GroupId} by user {UserId} when group is using default options.", request.GroupId, userId);
            return new SetOptionResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "The group is not using custom options so they cannot be amended."
            };
        }

        var currentOptions = await _ratingOptionRepository.GetForGroupIdAsync(request.GroupId, ct);
        if (currentOptions.Any(x => string.Equals(x.Label, request.Label, StringComparison.OrdinalIgnoreCase)))
        {
            _logger.LogWarning("AddOptionAsync called for group {GroupId} by user {UserId} with duplicate label.", request.GroupId, userId);
            return new SetOptionResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "An option with that label already exists for this group."
            };
        }

        var optionId = await _ratingOptionRepository.AddAsync(request, ct);

        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("User {UserId} added rating option for group {GroupId}.", userId, request.GroupId);

        return new SetOptionResponse
        {
            StatusCode = HttpStatusCode.Created,
            Message = "New rating added successfully.",
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
        var option = await _ratingOptionRepository.GetByIdAsync(optionId, ct);
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

        var currentOptions = await _ratingOptionRepository.GetForGroupIdAsync(option.GroupId.Value, ct);
        if (currentOptions.Any(x => x.OptionId != optionId && string.Equals(x.Label, request.Label, StringComparison.OrdinalIgnoreCase)))
        {
            _logger.LogWarning("UpdateOptionAsync called for group {GroupId} by user {UserId} with duplicate label.", option.GroupId, userId);
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "An option with that label already exists for this group."
            };
        }

        await _ratingOptionRepository.UpdateAsync(optionId, request.Label, ct);

        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("User {UserId} updated option for group {GroupId}.", userId, option.GroupId);

        return new CommonResponse
        {
            StatusCode = HttpStatusCode.Created,
            Message = $"Quality rating updated successfully.",
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
        var option = await _ratingOptionRepository.GetByIdAsync(optionId, ct);
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

        var isOptionUsed = await _ratingRepository.IsOptionBeingUsedAsync(optionId, ct);
        if (isOptionUsed)
        {
            _logger.LogWarning("DeleteOptionAsync called for option {OptionId} by user {UserId} which is currently in use.", optionId, userId);
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "Cannot delete this option because it is being used to rate a venue. Amend ratings to other options first."
            };
        }

        await _ratingOptionRepository.DeleteAsync(optionId, ct);
        await _ratingOptionRepository.CondenseDisplayOrdersAsync(option.GroupId.Value, optionId, ct);

        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("User {UserId} deleted option {OptionId}.", userId, optionId);

        return new CommonResponse
        {
            StatusCode = HttpStatusCode.Created,
            Message = "Quality rating deleted successfully.",
        };
    }

    public async Task<CommonResponse> ReorderOptionsAsync(OrderOptionsRequest request, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("ReorderOptionsAsync called with no authenticated user.");
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            };
        }

        var userId = _tokenData.UserId!.Value;
        var isUserInGroup = await _userGroupRepository.IsUserInGroupAsync(request.GroupId, userId, ct);

        if (!isUserInGroup)
        {
            _logger.LogWarning("ReorderOptionsAsync called by user {UserId} who is not in group {GroupId}.", userId, request.GroupId);
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "You do not have permission to reorder options for this group."
            };
        }

        //TODO: Check the highest currently used option number for this group, return fail if the default number of groups is lower than this

        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("User {UserId} reordered options for group {GroupId}.", userId, request.GroupId);

        return new CommonResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = $"Group-specific options reordered successfully."
        };
    }
}
