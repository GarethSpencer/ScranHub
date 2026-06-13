using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Functions;

public class TimedFunctions(ILoggerFactory loggerFactory)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<TimedFunctions>();

    [Function("Delete Inactive Users")]
    public Task Run([TimerTrigger("0 2 * * * *")] CancellationToken ct)
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("C# Timer trigger function executed at: {executionTime}", DateTime.UtcNow);
        }

        return Task.CompletedTask;
    }
}