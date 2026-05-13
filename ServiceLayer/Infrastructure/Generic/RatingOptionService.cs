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
    protected readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<SetOptionsResponse> SetGroupSpecificOptions(SetOptionsRequest request, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("SetGroupSpecificOptions called with no authenticated user.");
            return new SetOptionsResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            };
        }

        var userId = _tokenData.UserId!.Value;
        var isUserInGroup = await _userGroupRepository.IsUserInGroupAsync(userId, request.GroupId, ct);

        if (!isUserInGroup)
        {
            _logger.LogWarning("SetGroupSpecificOptions called by user {UserId} who is not in group {GroupId}.", userId, request.GroupId);
            return new SetOptionsResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "You do not have permission to set options for this group."
            };
        }

        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("User {UserId} set group-specific options for group {GroupId}.", userId, request.GroupId);

        return new SetOptionsResponse
        {
            StatusCode = HttpStatusCode.Created,
            Message = $"Quality rating created successfully.",
            OptionsIds = []
        };
    }

    public async Task<SetOptionResponse> AddOption(Guid groupId, SetOptionRequest request, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("SetGroupSpecificOptions called with no authenticated user.");
            return new SetOptionResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            };
        }

        var userId = _tokenData.UserId!.Value;
        var isUserInGroup = await _userGroupRepository.IsUserInGroupAsync(userId, groupId, ct);

        if (!isUserInGroup)
        {
            _logger.LogWarning("SetGroupSpecificOptions called by user {UserId} who is not in group {GroupId}.", userId, groupId);
            return new SetOptionResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "You do not have permission to set options for this group."
            };
        }

        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("User {UserId} updated option for group {GroupId}.", userId, groupId);

        return new SetOptionResponse
        {
            StatusCode = HttpStatusCode.Created,
            Message = $"Quality rating created successfully.",
            OptionsId = Guid.Empty
        };
    }

    public async Task<CommonResponse> UpdateOption(Guid optionId, SetOptionRequest request, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("SetGroupSpecificOptions called with no authenticated user.");
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            };
        }

        var userId = _tokenData.UserId!.Value;
        //TODO: Get groupId from optionId, return if groupId doesn't exist
        //var isUserInGroup = await _userGroupRepository.IsUserInGroupAsync(userId, groupId, ct);

        //if (!isUserInGroup)
        //{
        //    _logger.LogWarning("SetGroupSpecificOptions called by user {UserId} who is not in group {GroupId}.", userId, groupId);
        //    return new SetOptionsResponse
        //    {
        //        StatusCode = HttpStatusCode.Forbidden,
        //        Message = "You do not have permission to set options for this group."
        //    };
        //}

        //await _unitOfWork.SaveChangesAsync(ct);
        //_logger.LogInformation("User {UserId} updated option for group {GroupId}.", userId, groupId);

        return new CommonResponse
        {
            StatusCode = HttpStatusCode.Created,
            Message = $"Quality rating created successfully.",
        };
    }

    public async Task<SetOptionResponse> DeleteOption(Guid optionId, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("SetGroupSpecificOptions called with no authenticated user.");
            return new SetOptionResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            };
        }

        var userId = _tokenData.UserId!.Value;
        //TODO: Get groupId from optionId, return if groupId doesn't exist
        //var isUserInGroup = await _userGroupRepository.IsUserInGroupAsync(userId, groupId, ct);

        //if (!isUserInGroup)
        //{
        //    _logger.LogWarning("SetGroupSpecificOptions called by user {UserId} who is not in group {GroupId}.", userId, groupId);
        //    return new SetOptionResponse
        //    {
        //        StatusCode = HttpStatusCode.Forbidden,
        //        Message = "You do not have permission to set options for this group."
        //    };
        //}

        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("User {UserId} deleted option {OptionId}.", userId, optionId);

        return new SetOptionResponse
        {
            StatusCode = HttpStatusCode.Created,
            Message = $"Quality rating created successfully.",
            OptionsId = Guid.Empty
        };
    }

    public async Task<CommonResponse> RemoveGroupSpecificOptions(Guid groupId, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("RemoveGroupSpecificOptions called with no authenticated user.");
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            };
        }

        var userId = _tokenData.UserId!.Value;
        var isUserInGroup = await _userGroupRepository.IsUserInGroupAsync(userId, groupId, ct);

        if (!isUserInGroup)
        {
            _logger.LogWarning("RemoveGroupSpecificOptions called by user {UserId} who is not in group {GroupId}.", userId, groupId);
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "You do not have permission to remove options for this group."
            };
        }

        //TODO: Check the highest currently used option number for this group, return fail if the default number of groups is lower than this

        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("User {UserId} removed group-specific options for group {GroupId}.", userId, groupId);

        return new CommonResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = $"Group-specific options removed successfully."
        };
    }

    public async Task<CommonResponse> ReorderOptions(OrderOptionsRequest request, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("RemoveGroupSpecificOptions called with no authenticated user.");
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            };
        }

        var userId = _tokenData.UserId!.Value;
        var isUserInGroup = await _userGroupRepository.IsUserInGroupAsync(userId, request.GroupId, ct);

        if (!isUserInGroup)
        {
            _logger.LogWarning("RemoveGroupSpecificOptions called by user {UserId} who is not in group {GroupId}.", userId, request.GroupId);
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "You do not have permission to remove options for this group."
            };
        }

        //TODO: Check the highest currently used option number for this group, return fail if the default number of groups is lower than this

        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("User {UserId} removed group-specific options for group {GroupId}.", userId, request.GroupId);

        return new CommonResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = $"Group-specific options removed successfully."
        };
    }
}
