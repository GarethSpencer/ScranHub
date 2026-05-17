using Microsoft.Extensions.Logging;
using System.Net;
using System.Runtime.CompilerServices;
using Utilities.Models.Responses.Generic;

namespace Utilities.Helpers;

public static class CommonResponseLoggingExtensions
{
    public static CommonResponse WithResponseLog(this CommonResponse response, ILogger logger, Guid callerId = default,
        string? overrideMessage = null, [CallerMemberName] string callerName = "")
    {
        var level = response.StatusCode switch
        {
            HttpStatusCode.OK or HttpStatusCode.Created or HttpStatusCode.NoContent
                => LogLevel.Information,
            HttpStatusCode.NotFound or HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden or HttpStatusCode.Conflict or HttpStatusCode.BadRequest
                => LogLevel.Warning,
            _ => LogLevel.Error
        };

        if (!logger.IsEnabled(level))
        {
            return response;
        }

        var message = overrideMessage ?? response.Message;

        if (!string.IsNullOrEmpty(message))
        {
            logger.Log(level, "[{CallerName}] returned [{StatusCode}] for [{UserId}]: {Detail}",
                callerName, response.StatusCode, callerId, message);
        }
        else
        {
            logger.Log(level, "[{CallerName}] returned [{StatusCode}] for [{UserId}]",
                callerName, response.StatusCode, callerId);
        }

        return response;
    }
}
