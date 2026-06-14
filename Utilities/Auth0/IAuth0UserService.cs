namespace Utilities.Auth0;

public interface IAuth0UserService
{
    Task DeleteUserAsync(string authId, CancellationToken ct);
}
