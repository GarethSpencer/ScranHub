namespace ServiceLayer.Abstractions;

public interface IAuthService
{
    Task<(Guid? userId, bool isAdmin)> ResolveUserAsync(string authId, string? email, CancellationToken ct);
}
