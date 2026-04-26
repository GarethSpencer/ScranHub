using Asp.Versioning.ApiExplorer;
using Scalar.AspNetCore;

namespace WebApi.ProgramExtensions;

public static class ApplicationExtensions
{
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
