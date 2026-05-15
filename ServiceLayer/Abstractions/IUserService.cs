using Utilities.Models.Requests.Generic;
using Utilities.Models.Requests.Users;
using Utilities.Models.Responses.Generic;
using Utilities.Models.Responses.Users;

namespace ServiceLayer.Abstractions;

public interface IUserService
{
    Task<UserFriendsResponse> GetFriendsForUserAsync(CancellationToken ct);

    Task<AddUserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken ct);

    Task<GetUsersResponse> SearchUsersAsync(SearchUserRequest request, CancellationToken ct);

    Task<CommonResponse> UpdateUserAsync(Guid userId, UpdateUserRequest request, CancellationToken ct);

    Task<GetUserResponse> GetUserAsync(Guid userId, CancellationToken ct);

    Task<AddUserFriendResponse> AddUserFriendAsync(Guid friendId, CancellationToken ct);

    Task<CommonResponse> UpdateUserFriendAsync(Guid friendId, UpdateUserFriendRequest request, CancellationToken ct);

    Task<CommonResponse> DeleteUserAsync(Guid userId, CancellationToken ct);

    Task<CommonResponse> AddUserFriendByEmailAsync(AddFriendRequest request, CancellationToken ct);

    Task<CommonResponse> GetAllUsersAsync(PaginationBaseRequest request, CancellationToken ct);
}