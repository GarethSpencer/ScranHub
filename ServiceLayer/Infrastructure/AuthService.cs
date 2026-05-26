using Microsoft.Extensions.Logging;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Abstractions.Generic;
using ServiceLayer.Abstractions;

namespace ServiceLayer.Infrastructure;

public class AuthService(IUserRepository userRepository, IUnitOfWork unitOfWork, ILogger<AuthService> logger) : IAuthService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<AuthService> _logger = logger;

    public async Task<(Guid? UserId, bool IsAdmin)> ResolveUserAsync(string authId, string? email, CancellationToken ct)
    {
        var user = await _userRepository.GetByAuthId(authId, ct);

        if (user is not null)
        {
            return (user.UserId, user.Admin);
        }

        if (email is not null)
        {
            user = await _userRepository.GetByEmailAsync(email, ct);

            if (user is not null)
            {
                await _userRepository.SetAuthId(user.UserId, authId, ct);
                await _unitOfWork.SaveChangesAsync(ct);
                _logger.LogInformation("[ResolveUserAsync] resolved successfully for [{user.UserId}] and AuthId value set.", user.UserId);
                return (user.UserId, user.Admin);
            }
        }

        _logger.LogWarning("[ResolveUserAsync] not resolved for AuthId [{authId}] email [{email}].", authId, email);
        return (null, false);
    }
}
