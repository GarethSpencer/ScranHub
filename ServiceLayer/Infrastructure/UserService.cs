using Microsoft.Extensions.Logging;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Abstractions.Generic;
using ServiceLayer.Abstractions;
using System.Net;
using Utilities.Enums;
using Utilities.Models.Requests.Users;
using Utilities.Models.Responses.Generic;
using Utilities.Models.Responses.Users;
using Utilities.Token;

namespace ServiceLayer.Infrastructure;

public class UserService(ITokenData tokenData,
    ILogger<UserService> logger,
    IUserRepository userRepository,
    IUserFriendRepository userFriendRepository,
    IUnitOfWork unitOfWork) : IUserService
{
    private readonly ITokenData _tokenData = tokenData;
    private readonly ILogger<UserService> _logger = logger;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IUserFriendRepository _userFriendRepository = userFriendRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;


    public async Task<UserFriendsResponse> GetFriendsForUserAsync(CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("GetFriendsForUserAsync called with no authenticated user.");
            return new UserFriendsResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            };
        }

        var userId = _tokenData.UserId!.Value;

        var userFriends = await _userRepository.GetFriendsForUserAsync(userId, ct);

        if (userFriends == null)
        {
            _logger.LogWarning("No user found with id {UserId}", userId);
            return new UserFriendsResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "No user found."
            };
        }

        _logger.LogInformation("Successfully retrieved friends for user {UserId}", userId);

        return new UserFriendsResponse
        {
            UserId = userId,
            Friends = userFriends,
            FriendCount = userFriends.Count(x => x.Status == FriendshipStatus.Accepted),
            StatusCode = HttpStatusCode.OK,
            Message = "Friends retrieved successfully."
        };
    }

    public async Task<AddUserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("GetFriendsForUserAsync called with no authenticated user.");
            return new AddUserResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            };
        }

        var userExists = await _userRepository.ExistsAsync(x => x.Email.ToLower() == request.Email.ToLower(), ct);
        if (userExists)
        {
            _logger.LogWarning("User with email {Email} already exists.", request.Email);
            return new AddUserResponse
            {
                StatusCode = HttpStatusCode.Conflict,
                Message = $"User with email {request.Email} already exists."
            };
        }

        var userNameExists = await _userRepository.ExistsAsync(x => x.DisplayName.ToLower() == request.DisplayName.ToLower(), ct);
        if (userNameExists)
        {
            _logger.LogWarning("Display name {DisplayName} already taken.", request.DisplayName);
            return new AddUserResponse
            {
                StatusCode = HttpStatusCode.Conflict,
                Message = $"Display name {request.DisplayName} already taken."
            };
        }

        var userId = await _userRepository.CreateUserAsync(request, ct);

        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("Successfully created user with id {UserId}", userId);

        return new AddUserResponse
        {
            StatusCode = HttpStatusCode.Created,
            Message = "User created successfully.",
            UserId = userId
        };
    }

    public async Task<GetUserResponse> GetUserAsync(Guid userId, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("GetUserAsync called with no authenticated user.");
            return new GetUserResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            };
        }

        var callingUserId = _tokenData.UserId!.Value;
        var areUsersFriends = await _userFriendRepository.IsFriendshipAcceptedAsync(callingUserId, userId, ct);

        var isAdmin = await _userRepository.IsUserAdminAsync(callingUserId, ct);
        if (!isAdmin && !areUsersFriends && callingUserId != userId)
        {
            _logger.LogWarning("User {CallingUserId} is not an admin or friend so cannot search {UserId} by Id.", callingUserId, userId);
            return new GetUserResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Only admins or friends can search for another user by Id."
            };
        }

        var user = await _userRepository.GetDetailsByIdAsync(userId, ct);

        if (user == null)
        {
            _logger.LogWarning("User with ID {UserId} not found.", userId);
            return new GetUserResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = $"User with ID {userId} not found."
            };
        }

        return new GetUserResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "User returned successfully.",
            User = user
        };
    }

    public async Task<AddUserFriendResponse> AddUserFriendAsync(Guid friendId, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("AddUserFriendAsync called with no authenticated user.");
            return new AddUserFriendResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            };
        }

        var userId = _tokenData.UserId!.Value;

        if (friendId == userId)
        {
            _logger.LogWarning("User {UserId} cannot add themselves as a friend.", friendId);
            return new AddUserFriendResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "You cannot add yourself as a friend."
            };
        }

        var friendExists = await _userRepository.ExistsAsync(x => x.UserId == friendId, ct);
        if (!friendExists)
        {
            _logger.LogWarning("Cannot add friend {FriendId} because they do not exist.", friendId);
            return new AddUserFriendResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Cannot add friend because they do not exist."
            };
        }

        var existingUserFriend = await _userFriendRepository.GetUserFriendAsync(userId, friendId, ct);

        if (existingUserFriend != null)
        {
            _logger.LogWarning("UserFriend between {UserId} and {FriendId} already created, cannot add again.", userId, friendId);
            return new AddUserFriendResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "Friend already requested."
            };
        }

        var userFriendId = await _userFriendRepository.CreateUserFriendAsync(userId, friendId, ct);

        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("Successfully sent friend request from {UserId} to {FriendId}.", userId, friendId);

        return new AddUserFriendResponse
        {
            UserFriendId = userFriendId,
            StatusCode = HttpStatusCode.OK,
            Message = "Successfully sent friend request to user."
        };
    }

    public async Task<CommonResponse> UpdateUserFriendAsync(Guid friendId, UpdateUserFriendRequest request, CancellationToken ct)
    {
        if (!_tokenData.UserId.HasValue)
        {
            _logger.LogWarning("UpdateUserFriendAsync called with no authenticated user.");
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Unauthorized."
            };
        }

        var userId = _tokenData.UserId!.Value;

        if (friendId == userId)
        {
            _logger.LogWarning("User {UserId} cannot have themselves as a friend.", friendId);
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "You cannot have yourself as a friend."
            };
        }

        var userFriend = await _userFriendRepository.GetUserFriendAsync(userId, friendId, ct);
        if (userFriend == null)
        {
            _logger.LogWarning("UserFriend between {UserId} and {FriendId} does not exist, cannot update.", userId, friendId);
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "Friend not found."
            };
        }

        if (userFriend.FriendId != userId)
        {
            _logger.LogWarning("User {UserId} is not the recipient of the friend request for {FriendId}.", userId, friendId);
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "Only the requested user can update this friend request."
            };
        }

        await _userFriendRepository.UpdateUserFriendStatusAsync(userFriend.UserFriendId, request.Status, ct);

        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("Successfully updated friend status for {UserId} and {FriendId}.", userId, friendId);

        return new CommonResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = $"Successfully updated friend status to {request.Status}."
        };
    }
}
