using Asp.Versioning.ApiExplorer;
using Scalar.AspNetCore;

namespace WebApi.ProgramExtensions;

/// <summary>
/// Extension methods for the WebApi project to configure WebApplication with Swagger and Scalar API documentation and UI.
/// </summary>
public static class ApplicationExtensions
{
    /// <summary>
    /// Configure the WebApplication to use Swagger for API documentation and UI, including support for API versioning and authorization persistence in the UI.
    /// </summary>
    /// <param name="application"></param>
    public static void ConfigureSwagger(this WebApplication application)
    {
        var apiVersionDescriptionProvider = application.Services.GetRequiredService<IApiVersionDescriptionProvider>();

        application.UseSwagger();
        application.UseSwaggerUI(options =>
        {
            foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
            }
            options.EnablePersistAuthorization();
        });
    }

    /// <summary>
    /// Configure the WebApplication to use Scalar for API reference documentation and UI, including support for authentication with Bearer tokens in the UI.
    /// </summary>
    /// <param name="application"></param>
    public static void ConfigureScalar(this WebApplication application)
    {
        application.MapOpenApi().AllowAnonymous();
        application.MapScalarApiReference(opts =>
        {
            opts.Authentication = new ScalarAuthenticationOptions
            {
                PreferredSecuritySchemes = ["Bearer"]
            };
        }).AllowAnonymous();
    }
}
