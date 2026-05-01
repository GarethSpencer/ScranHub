using Microsoft.Extensions.Logging;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Abstractions.Generic;
using ServiceLayer.Abstractions;
using System.Net;
using Utilities.Models.Requests.Users;
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
            FriendCount = userFriends.Count(x => x.Approved),
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
        var areUsersFriends = await _userFriendRepository.AreUsersFriendsAsync(callingUserId, userId, ct);

        var isAdmin = await _userRepository.IsUserAdminAsync(callingUserId, ct);
        if (!isAdmin && !areUsersFriends)
        {
            _logger.LogWarning("User {CallingUserId} is not an admin or friend so cannot search {UserId} by Id.", callingUserId, userId);
            return new GetUserResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "Only admins or friends can search for a user by Id."
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
            Message = $"User returned successfully.",
            User = user
        };
    }
}
