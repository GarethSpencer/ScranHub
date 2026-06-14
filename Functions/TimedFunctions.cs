using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Abstractions.Generic;

namespace Functions;

public class TimedFunctions(ILoggerFactory loggerFactory, IUserRepository userRepository, IUnitOfWork unitOfWork)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<TimedFunctions>();

    [Function("DeleteInactiveUsers")]
    public async Task Run([TimerTrigger("0 */5 * * * *")] CancellationToken ct)
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("C# Timer trigger function executed at: {executionTime}", DateTime.UtcNow);
        }

        try
        {
            var inactiveUsers = await userRepository.GetAllLongTermInactiveAsync(30, ct); //users made inactive at least 30 days ago

            var count = inactiveUsers.Count();
            if (count == 0)
            {
                _logger.LogInformation("No inactive users to delete");
                return;
            }

            foreach (var user in inactiveUsers)
            {
                await userRepository.DeleteAsync(user.UserId, ct);
            }

            await unitOfWork.SaveChangesAsync(ct);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Successfully deleted {count} inactive users", count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete inactive users");
            throw;
        }
    }
}
