using Microsoft.Extensions.Logging;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Abstractions.Generic;
using ServiceLayer.Abstractions;
using System.Net;
using Utilities.Enums;
using Utilities.Helpers;
using Utilities.Models.Requests.Generic;
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

    public async Task<CommonResponse> GetFriendsForUserAsync(CancellationToken ct)
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
        var userFriends = await _userRepository.GetFriendsForUserAsync(callingUserId, ct);
        if (userFriends == null)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "User not found."
            }.WithResponseLog(_logger, callingUserId);
        }

        var friendCount = userFriends.Count(x => x.Status == FriendshipStatus.Accepted);

        return new UserFriendsResponse
        {
            UserId = callingUserId,
            Friends = userFriends,
            FriendCount = friendCount,
            StatusCode = friendCount > 0 ? HttpStatusCode.OK : HttpStatusCode.NoContent,
            Message = "Friends retrieved successfully."
        }.WithResponseLog(_logger, callingUserId);
    }

    public async Task<CommonResponse> CreateUserAsync(CreateUserRequest request, CancellationToken ct)
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
        var userExists = await _userRepository.ExistsAsync(x => x.Email == request.Email, ct);
        if (userExists)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Conflict,
                Message = $"User with email {request.Email} already exists."
            }.WithResponseLog(_logger, callingUserId);
        }

        var userNameExists = await _userRepository.ExistsAsync(x => x.DisplayName == request.DisplayName, ct);
        if (userNameExists)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Conflict,
                Message = $"Display name {request.DisplayName} already taken."
            }.WithResponseLog(_logger, callingUserId);
        }

        var isAdmin = await _userRepository.IsUserAdminAsync(callingUserId, ct);
        if (!isAdmin && request.Admin)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "You cannot create an admin user."
            }.WithResponseLog(_logger, callingUserId);
        }

        var userId = await _userRepository.CreateAsync(request, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new AddUserResponse
        {
            StatusCode = HttpStatusCode.Created,
            Message = "User created successfully.",
            UserId = userId
        }.WithResponseLog(_logger, callingUserId, $"User [{userId}] created successfully.");
    }

    public async Task<CommonResponse> SearchUsersAsync(SearchUserRequest request, CancellationToken ct)
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
        var (users, totalCount) = await _userRepository.SearchByDisplayNameAsync(request, ct);

        return new GetUsersResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Users returned successfully.",
            Users = users,
            TotalCount = totalCount
        }.WithResponseLog(_logger, callingUserId);
    }

    public async Task<CommonResponse> UpdateUserAsync(Guid userId, UpdateUserRequest request, CancellationToken ct)
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
        var isAdmin = await _userRepository.IsUserAdminAsync(callingUserId, ct);
        if (!isAdmin && callingUserId != userId)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "You do not have permission to update this user."
            }.WithResponseLog(_logger, callingUserId);
        }

        if (!isAdmin && request.Admin)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "You cannot make yourself an admin."
            }.WithResponseLog(_logger, callingUserId);
        }

        var userToUpdate = await _userRepository.GetDetailsByIdAsync(userId, ct);
        if (userToUpdate == null)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "User not found."
            }.WithResponseLog(_logger, callingUserId);
        }

        var userToUpdateIsAdmin = await _userRepository.IsUserAdminAsync(userId, ct);
        if (userToUpdateIsAdmin && userId != callingUserId)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Message = "You do not have permission to update this user."
            }.WithResponseLog(_logger, callingUserId);
        }

        await _userRepository.UpdateAsync(userId, request, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new CommonResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "User updated successfully."
        }.WithResponseLog(_logger, callingUserId, $"User [{userId}] created successfully.");
    }

    public async Task<CommonResponse> GetCurrentUserAsync(CancellationToken ct)
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
        var user = await _userRepository.GetDetailsByIdAsync(callingUserId, ct);
        if (user == null)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "User was not found."
            }.WithResponseLog(_logger, callingUserId);
        }

        return new GetUserResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "User returned successfully.",
            User = user
        }.WithResponseLog(_logger, callingUserId);
    }

    public async Task<CommonResponse> GetUserAsync(Guid userId, CancellationToken ct)
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
        var user = await _userRepository.GetDetailsByIdAsync(userId, ct);
        if (user == null)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "User with that Id was not found."
            }.WithResponseLog(_logger, callingUserId);
        }

        var areUsersFriends = await _userFriendRepository.IsFriendshipAcceptedAsync(callingUserId, userId, ct);
        var isAdmin = await _userRepository.IsUserAdminAsync(callingUserId, ct);
        if (!isAdmin && !areUsersFriends && callingUserId != userId)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "Only admins or friends can search for another user by Id."
            }.WithResponseLog(_logger, callingUserId);
        }

        return new GetUserResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "User returned successfully.",
            User = user
        }.WithResponseLog(_logger, callingUserId);
    }

    public async Task<CommonResponse> AddUserFriendAsync(Guid friendId, CancellationToken ct)
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
        if (friendId == callingUserId)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "You cannot add yourself as a friend."
            }.WithResponseLog(_logger, callingUserId);
        }

        var friendExists = await _userRepository.ExistsAsync(x => x.UserId == friendId, ct);
        if (!friendExists)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Cannot add friend because they do not exist."
            }.WithResponseLog(_logger, callingUserId);
        }

        var existingUserFriend = await _userFriendRepository.GetUserFriendAsync(callingUserId, friendId, ct);
        if (existingUserFriend != null)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "Friend already requested."
            }.WithResponseLog(_logger, callingUserId);
        }

        var userFriendId = await _userFriendRepository.CreateUserFriendAsync(callingUserId, friendId, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new AddUserFriendResponse
        {
            UserFriendId = userFriendId,
            StatusCode = HttpStatusCode.Created,
            Message = "Successfully sent friend request."
        }.WithResponseLog(_logger, callingUserId, $"Successfully sent friend request to [{friendId}].");
    }

    public async Task<CommonResponse> UpdateUserFriendAsync(Guid friendId, UpdateUserFriendRequest request, CancellationToken ct)
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
        if (friendId == callingUserId)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Message = "You cannot have yourself as a friend."
            }.WithResponseLog(_logger, callingUserId);
        }

        var userFriend = await _userFriendRepository.GetUserFriendAsync(callingUserId, friendId, ct);
        if (userFriend == null)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Friend not found."
            }.WithResponseLog(_logger, callingUserId);
        }

        if (userFriend.FriendId != callingUserId)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "Only the requested user can update this friend request."
            }.WithResponseLog(_logger, callingUserId);
        }

        await _userFriendRepository.UpdateUserFriendStatusAsync(userFriend.UserFriendId, request.Status, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new CommonResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = $"Successfully updated friend status to {request.Status}."
        }.WithResponseLog(_logger, callingUserId, $"Successfully updated friend status of [{friendId}] to {request.Status}.");
    }

    public async Task<CommonResponse> DeleteUserAsync(Guid userId, CancellationToken ct)
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
        var userExists = await _userRepository.ExistsAsync(x => x.UserId == userId, ct);
        if (!userExists)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "User not found."
            }.WithResponseLog(_logger, callingUserId);
        }

        var isAdmin = await _userRepository.IsUserAdminAsync(callingUserId, ct);
        if (!isAdmin && callingUserId != userId)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "You cannot delete other users."
            }.WithResponseLog(_logger, callingUserId);
        }

        var isUserToDeleteAdmin = await _userRepository.IsUserAdminAsync(userId, ct);
        if (isUserToDeleteAdmin && callingUserId != userId)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "You cannot delete other admins."
            }.WithResponseLog(_logger, callingUserId);
        }

        await _userRepository.DeleteAsync(userId, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new CommonResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "User deleted successfully."
        }.WithResponseLog(_logger, callingUserId, $"User [{userId}] deleted successfully.");
    }

    public async Task<CommonResponse> AddUserFriendByEmailAsync(AddFriendRequest request, CancellationToken ct)
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
        var userToFriend = await _userRepository.GetByEmailAsync(request.Email, ct);
        if (userToFriend == null)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.OK,
                Message = "If a user with this email exists, a friend request will be sent to them."
            }.WithResponseLog(_logger, callingUserId, "User with that email does not exist.");
        }

        var friendId = userToFriend.UserId;
        if (friendId == callingUserId)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.OK,
                Message = "If a user with this email exists, a friend request will be sent to them."
            }.WithResponseLog(_logger, callingUserId, "User tried to add themselves as a friend.");
        }

        var existingUserFriend = await _userFriendRepository.GetUserFriendAsync(callingUserId, friendId, ct);
        if (existingUserFriend != null)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.OK,
                Message = "If a user with this email exists, a friend request will be sent to them."
            }.WithResponseLog(_logger, callingUserId, "User tried to add a friend they already have.");
        }

        //Don't return the new Id to avoid leaking whether or not the email exists to the user
        _ = await _userFriendRepository.CreateUserFriendAsync(callingUserId, friendId, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new CommonResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "If a user with this email exists, a friend request will be sent to them."
        }.WithResponseLog(_logger, callingUserId, $"Friend [{friendId}] requested successfully via email.");
    }

    public async Task<CommonResponse> GetAllUsersAsync(PaginationBaseRequest request, CancellationToken ct)
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
        var isAdmin = await _userRepository.IsUserAdminAsync(callingUserId, ct);
        if (!isAdmin)
        {
            return new CommonResponse
            {
                StatusCode = HttpStatusCode.Forbidden,
                Message = "You are not an admin."
            }.WithResponseLog(_logger, callingUserId);
        }

        var (users, totalCount) = await _userRepository.GetAllAsync(request, ct);

        return new GetUsersDetailedResponse
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Users returned successfully.",
            Users = users,
            TotalCount = totalCount,
        }.WithResponseLog(_logger, callingUserId);
    }
}
