using Utilities.Models.Responses.Users;

namespace ServiceLayer.Abstractions;

public interface IUserService
{
    Task<UserFriendsResponse> GetFriendsForUserAsync(CancellationToken ct);
}