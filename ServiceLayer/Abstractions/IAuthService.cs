namespace ServiceLayer.Abstractions;

public interface IAuthService
{
    Task<(Guid? UserId, bool IsAdmin)> ResolveUserAsync(string authId, string? email, CancellationToken ct);
}
