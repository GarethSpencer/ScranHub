using Utilities.Models.Responses.GenericResponses;
using System.Text.Json.Serialization;
using Asp.Versioning;

namespace WebApi.ProgramExtensions;

public static class ServiceExtensions
{
    public static void ConfigureApiBehavior(this IServiceCollection services)
    {
        services.AddControllers().ConfigureApiBehaviorOptions(opts =>
        {
            opts.InvalidModelStateResponseFactory = context =>
            {
                var errorResultBody = new ErrorResultResponse()
                {
                    Errors = [.. context.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)]
                };
                return new BadRequestResponse(errorResultBody);
            };
        })
            .AddJsonOptions(opts =>
            {
                opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });
    }

    public static void ConfigureApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(opts =>
        {
            opts.DefaultApiVersion = new ApiVersion(1, 0);
            opts.AssumeDefaultVersionWhenUnspecified = true;
            opts.ReportApiVersions = true;
            opts.ApiVersionReader = new UrlSegmentApiVersionReader();
        }).AddApiExplorer(opts =>
        {
            opts.GroupNameFormat = "'v'VVV";
            opts.SubstituteApiVersionInUrl = true;
        });
    }
}
