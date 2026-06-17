using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Abstractions.Generic;
using Utilities.Auth0;

namespace Functions;

public class TimedFunctions(ILoggerFactory loggerFactory, IUserRepository userRepository, IUnitOfWork unitOfWork, IAuth0UserService auth0UserService)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<TimedFunctions>();

    [Function("DeleteInactiveUsers")]
    public async Task Run([TimerTrigger("0 0 2 * * *")] TimerInfo timer, CancellationToken ct)
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("C# Timer trigger function executed at: {executionTime}.", DateTime.UtcNow);
        }

        if (timer.IsPastDue)
        {
            _logger.LogWarning("Timer is running late, catching up on missed schedule.");
        }

        try
        {
            var inactiveUsers = await userRepository.GetAllLongTermInactiveAsync(30, ct);

            var count = inactiveUsers.Count();
            if (count == 0)
            {
                _logger.LogInformation("No inactive users to delete.");
                return;
            }

            var staged = 0;
            var failed = 0;

            foreach (var user in inactiveUsers)
            {
                ct.ThrowIfCancellationRequested();

                if (user.AuthId is not null)
                {
                    try
                    {
                        await auth0UserService.DeleteUserAsync(user.AuthId, ct);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        _logger.LogError(ex, "Failed to delete user {UserId} from Auth0; skipping DB delete, will retry next run.", user.UserId);
                        failed++;
                        continue;
                    }
                }

                await userRepository.DeleteAsync(user.UserId, ct);
                staged++;
            }

            if (staged > 0)
            {
                await unitOfWork.SaveChangesAsync(ct);
            }

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Deleted {Staged} inactive users. Skipped {Failed} due to Auth0 errors.", staged, failed);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete inactive users.");
            throw;
        }
    }
}
