using Microsoft.Extensions.Logging;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Abstractions.Generic;
using ServiceLayer.Abstractions.Generic;
using System.Net;
using Utilities.Enums;
using Utilities.Helpers;
using Utilities.Models.Requests.Options;
using Utilities.Models.Responses.Generic;
using Utilities.Models.Responses.Options;
using Utilities.Models.Results.Generic;
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

    public async Task<CommonResponse> SetGroupCustomOptionsAsync(SetOptionsRequest request, CancellationToken ct)
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

        var usingDefaults = await _ratingOptionRepository.IsGroupUsingDefaultValues(request.GroupId, ct);
        if (!usingDefaults)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "The group is already using custom options."
            }.WithResponseLog(_logger, callingUserId);
        }

        IEnumerable<RatingOptionResult> ratingOptionsInUse = await _ratingRepository.GetDistinctRatingsGivenForGroupAsync(request.GroupId, ct);
        if (ratingOptionsInUse.Count() > request.Labels.Length)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = $"You need to provide at least {ratingOptionsInUse.Count()} labels to map the current options."
            }.WithResponseLog(_logger, callingUserId);
        }

        var defaults = await _ratingOptionRepository.GetDefaultsAsync(ct);
        var optionIds = await _ratingOptionRepository.AddRangeAsync(request, ct);
        var remapStrategyUsed = await RemapRatingsToCustoms(defaults.Count(), request, optionIds, ratingOptionsInUse, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var squashed = remapStrategyUsed == RemapStrategy.SquashOrder;
        return new SetOptionsResponse
        {
            StatusCode = HttpStatusCode.Created,
            Message = "Custom ratings were created and mapped successfully" + (squashed ? ", but existing ratings were squashed." : "."),
            OptionsIds = optionIds
        }.WithResponseLog(_logger, callingUserId, $"Custom ratings [{string.Join(", ", optionIds)}] were created and mapped successfully"
            + (squashed ? ", but existing ratings were squashed." : "."));
    }

    private async Task<RemapStrategy> RemapRatingsToCustoms(int defaultsCount, SetOptionsRequest request, IEnumerable<Guid> optionIds, IEnumerable<RatingOptionResult> ratingOptionsInUse, CancellationToken ct)
    {
        if (defaultsCount == request.Labels.Length)
        {
            // We have as many default options as new options, so we can map cleanly
            await _ratingRepository.RemapRatingsMaintainDisplayOrderAsync(request.GroupId, optionIds, ct);
            return RemapStrategy.MaintainOrder;
        }
        else if (ratingOptionsInUse.Count() == request.Labels.Length)
        {
            // We have as many default ratings being used as new ratings so we can map if we don't leave gaps
            await _ratingRepository.RemapRatingsSquashDisplayOrderAsync(request.GroupId, optionIds, ct);
            return RemapStrategy.SquashOrder;
        }
        else if (ratingOptionsInUse.OrderByDescending(x => x.DisplayOrder).First().DisplayOrder <= request.Labels.Length)
        {
            // The current highest display order is less than or equal to the number of new options, so we can leave gaps
            await _ratingRepository.RemapRatingsMaintainDisplayOrderAsync(request.GroupId, optionIds, ct);
            return RemapStrategy.MaintainOrder;
        }
        else
        {
            // The current highest display order is greater than the number of new options, but we can make them fit if we squash the order
            await _ratingRepository.RemapRatingsSquashDisplayOrderAsync(request.GroupId, optionIds, ct);
            return RemapStrategy.SquashOrder;
        }
    }

    public async Task<CommonResponse> RemoveGroupCustomOptionsAsync(Guid groupId, CancellationToken ct)
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
                Message = "You do not have permission to remove options for this group."
            }.WithResponseLog(_logger, callingUserId);
        }

        var usingDefaults = await _ratingOptionRepository.IsGroupUsingDefaultValues(groupId, ct);
        if (usingDefaults)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "The group is already using default options."
            }.WithResponseLog(_logger, callingUserId);
        }

        var ratingOptionsInUse = await _ratingRepository.GetDistinctRatingsGivenForGroupAsync(groupId, ct);
        var defaultOptions = await _ratingOptionRepository.GetDefaultsAsync(ct);
        if (ratingOptionsInUse.Count() > defaultOptions.Count())
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = $"Cannot remove custom options. The group is using {ratingOptionsInUse.Count()} labels but there are only {defaultOptions.Count()} default options."
            }.WithResponseLog(_logger, callingUserId);
        }

        var currentOptions = await _ratingOptionRepository.GetForGroupIdAsync(groupId, ct);
        var remapStrategyUsed = await RemapRatingsToDefaults(groupId, defaultOptions, currentOptions, ratingOptionsInUse, ct);
        await _ratingOptionRepository.RemoveCustomRatingsForGroupAsync(groupId, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var squashed = remapStrategyUsed == RemapStrategy.SquashOrder;
        return new CommonResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Custom ratings were removed successfully" + (squashed ? ", but ratings were squashed to fit the default options." : "."),
        }.WithResponseLog(_logger, callingUserId, $"Custom ratings were removed successfully for group [{groupId}]"
            + (squashed ? ", but ratings were squashed to fit the default options." : "."));
    }

    private async Task<RemapStrategy> RemapRatingsToDefaults(Guid groupId, IEnumerable<RatingOptionResult> defaultOptions, IEnumerable<RatingOptionResult> currentOptions,
        IEnumerable<RatingOptionResult> ratingOptionsInUse, CancellationToken ct)
    {
        if (currentOptions.Count() == defaultOptions.Count())
        {
            // We have as many default options as current options, so we can map cleanly
            await _ratingRepository.RemapRatingsMaintainDisplayOrderAsync(groupId, defaultOptions.Select(x => x.OptionId), ct);
            return RemapStrategy.MaintainOrder;
        }
        else if (defaultOptions.Count() == ratingOptionsInUse.Count())
        {
            // We have as many custom options being used as default options so we can map if we don't leave gaps
            await _ratingRepository.RemapRatingsSquashDisplayOrderAsync(groupId, defaultOptions.Select(x => x.OptionId), ct);
            return RemapStrategy.SquashOrder;
        }
        else if (ratingOptionsInUse.OrderByDescending(x => x.DisplayOrder).First().DisplayOrder <= defaultOptions.Count())
        {
            // The current highest display order is less than or equal to the number of default options, so we can leave gaps
            await _ratingRepository.RemapRatingsMaintainDisplayOrderAsync(groupId, defaultOptions.Select(x => x.OptionId), ct);
            return RemapStrategy.MaintainOrder;
        }
        else
        {
            // The current highest display order is greater than the number of default options, but we can make them fit if we squash the order
            await _ratingRepository.RemapRatingsSquashDisplayOrderAsync(groupId, defaultOptions.Select(x => x.OptionId), ct);
            return RemapStrategy.SquashOrder;
        }
    }

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

        var usingDefaults = await _ratingOptionRepository.IsGroupUsingDefaultValues(request.GroupId, ct);
        if (usingDefaults)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "The group is not using custom options so they cannot be amended."
            }.WithResponseLog(_logger, callingUserId);
        }

        var currentOptions = await _ratingOptionRepository.GetForGroupIdAsync(request.GroupId, ct);
        if (currentOptions.Any(x => string.Equals(x.Label, request.Label, StringComparison.OrdinalIgnoreCase)))
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Conflict,
                Message = "An option with that label already exists for this group."
            }.WithResponseLog(_logger, callingUserId);
        }

        var optionId = await _ratingOptionRepository.AddAsync(request, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new SetOptionResponse
        {
            StatusCode = HttpStatusCode.Created,
            Message = "New rating added successfully.",
            OptionsId = optionId
        }.WithResponseLog(_logger, callingUserId, $"New rating [{optionId}] added successfully.");
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
        var option = await _ratingOptionRepository.GetByIdAsync(optionId, ct);
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
                Message = "The option was not found."
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

        var currentOptions = await _ratingOptionRepository.GetForGroupIdAsync(option.GroupId.Value, ct);
        if (currentOptions.Any(x => x.OptionId != optionId && string.Equals(x.Label, request.Label, StringComparison.OrdinalIgnoreCase)))
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Conflict,
                Message = "An option with that label already exists for this group."
            }.WithResponseLog(_logger, callingUserId);
        }

        await _ratingOptionRepository.UpdateAsync(optionId, request.Label, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new CommonResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Rating option updated successfully.",
        }.WithResponseLog(_logger, callingUserId, $"Rating option [{optionId}] updated successfully.");
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
        var option = await _ratingOptionRepository.GetByIdAsync(optionId, ct);
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
                Message = "The option was not found."
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

        var isOptionUsed = await _ratingRepository.IsOptionBeingUsedAsync(optionId, ct);
        if (isOptionUsed)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "Cannot delete this option because it is being used to rate a venue. Amend ratings to other options first."
            }.WithResponseLog(_logger, callingUserId);
        }

        await _ratingOptionRepository.DeleteAsync(optionId, ct);
        await _ratingOptionRepository.CondenseDisplayOrdersAsync(option.GroupId.Value, optionId, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new CommonResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Rating option deleted successfully.",
        }.WithResponseLog(_logger, callingUserId, $"Rating option [{optionId}] deleted successfully.");
    }

    public async Task<CommonResponse> ReorderOptionsAsync(OrderOptionsRequest request, CancellationToken ct)
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

        var usingDefaults = await _ratingOptionRepository.IsGroupUsingDefaultValues(request.GroupId, ct);
        if (usingDefaults)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "The group is not using custom options so they cannot be amended."
            }.WithResponseLog(_logger, callingUserId);
        }

        var currentOptions = await _ratingOptionRepository.GetForGroupIdAsync(request.GroupId, ct);
        if (currentOptions.Count() != request.OptionsIds.Length
            || currentOptions.Any(x => !request.OptionsIds.Contains(x.OptionId)))
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "The provided option IDs do not match the current options for this group."
            }.WithResponseLog(_logger, callingUserId);
        }

        await _ratingOptionRepository.ReorderAsync(request.GroupId, request.OptionsIds, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new CommonResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = $"Group-specific options reordered successfully."
        }.WithResponseLog(_logger, callingUserId);
    }

    public async Task<CommonResponse> GetGroupRatingOptionsAsync(Guid? groupId, CancellationToken ct)
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
                    Message = "The group wsa not found."
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

        var options = await _ratingOptionRepository.GetForGroupIdAsync(groupId, ct);

        return new GetRatingOptionsResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Options retrieved successfully.",
            Options = options
        }.WithResponseLog(_logger, callingUserId);
    }

    public async Task<CommonResponse> GetRatingOptionAsync(Guid optionId, CancellationToken ct)
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
        var option = await _ratingOptionRepository.GetByIdAsync(optionId, ct);
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
            return new GetRatingOptionResponse
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

        return new GetRatingOptionResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Option retrieved successfully.",
            Option = option
        }.WithResponseLog(_logger, callingUserId);
    }
}
