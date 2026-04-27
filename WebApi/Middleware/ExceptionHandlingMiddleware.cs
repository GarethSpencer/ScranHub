using Utilities.Models.Responses.GenericResponses;

namespace WebApi.Middleware;

internal class ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment environment) : IMiddleware
{
    private readonly ILogger _logger = logger;
    private readonly IHostEnvironment _environment = environment;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred for {Method} {Path}.", context.Request.Method, context.Request.Path);

            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var message = _environment.IsDevelopment()
            ? exception.Message
            : "An unexpected error occurred. Please try again later.";


        await context.Response.WriteAsync(new ErrorResultResponse
        {
            Errors = [message]
        }.ToString());
    }
}
