using Microsoft.Extensions.Logging;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Abstractions.Generic;
using ServiceLayer.Abstractions;
using Utilities.Models.Requests.Users;

namespace ServiceLayer.Infrastructure;

public class AuthService(IUserRepository userRepository, IUnitOfWork unitOfWork, ILogger<AuthService> logger) : IAuthService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<AuthService> _logger = logger;

    public async Task<(Guid? userId, bool isAdmin)> ResolveUserAsync(string authId, string? email, CancellationToken ct)
    {
        var user = await _userRepository.GetByAuthId(authId, ct);

        if (user is not null)
        {
            if (email is not null && user.Email != email)
            {
                var existingUserWithEmail = await _userRepository.GetByEmailAsync(email, ct);
                if (existingUserWithEmail is not null)
                {
                    _logger.LogWarning("[ResolveUserAsync] AuthId [{authId}] email [{email}] already belongs to another user.", authId, email);
                    return (null, false);
                }

                await _userRepository.UpdateEmailAsync(user.UserId, email, ct);
                await _unitOfWork.SaveChangesAsync(ct);
                _logger.LogInformation("[ResolveUserAsync] email updated for [{UserId}] from Auth0 token.", user.UserId);
            }

            return (user.UserId, user.Admin);
        }

        if (email is null)
        {
            _logger.LogWarning("[ResolveUserAsync] not resolved for AuthId [{authId}] as email is not provided and no user found with the authId.", authId);
            return (null, false);
        }

        user = await _userRepository.GetByEmailAsync(email, ct);

        if (user is not null && user.AuthId is null)
        {
            await _userRepository.SetAuthId(user.UserId, authId, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            _logger.LogInformation("[ResolveUserAsync] resolved successfully for [{user.UserId}] and AuthId value set.", user.UserId);
            return (user.UserId, user.Admin);
        }

        if (user is not null)
        {
            _logger.LogWarning("[ResolveUserAsync] not resolved for AuthId [{authId}] email [{email}] as the authId does not match the database value.", authId, email);
            return (null, false);
        }

        var newUser = new CreateUserRequest
        {
            Email = email,
            DisplayName = email.Split('@')[0],
            Admin = false,
        };

        var newUserId = await _userRepository.CreateAsync(newUser, ct);
        await _userRepository.SetAuthId(newUserId, authId, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("[ResolveUserAsync] resolved for AuthId [{authId}] email [{email}] and new user [{newUserId}] created.", authId, email, newUserId);

        return (newUserId, newUser.Admin);
    }
}
