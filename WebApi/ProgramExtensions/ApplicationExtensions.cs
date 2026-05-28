using Asp.Versioning.ApiExplorer;
using Scalar.AspNetCore;

namespace WebApi.ProgramExtensions;

/// <summary>
/// Extension methods for the WebApi project to configure WebApplication with Swagger and Scalar API documentation and UI.
/// </summary>
public static class ApplicationExtensions
{
    /// <summary>
    /// Configure the WebApplication to use Swagger for API documentation and UI, including support for API versioning and Auth0
    /// </summary>
    /// <param name="application"></param>
    /// <param name="configuration"></param>
    public static void ConfigureSwagger(this WebApplication application, IConfiguration configuration)
    {
        var apiVersionDescriptionProvider = application.Services.GetRequiredService<IApiVersionDescriptionProvider>();

        application.UseSwagger();
        application.UseSwaggerUI(options =>
        {
            options.OAuthScopes("openid", "profile", "email");
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "ScranHub API v1");
            options.OAuthClientId("XSz6cNAh50w7T0nJT0S2FTgX0mySt0fy");
            options.OAuthAdditionalQueryStringParams(new Dictionary<string, string>
            {
                { "audience", configuration["Auth0:Audience"]! }
            });
            options.OAuthUsePkce();
        });
    }

    /// <summary>
    /// Configure the WebApplication to use Scalar for API reference documentation and UI, including support for Auth0
    /// </summary>
    /// <param name="application"></param>
    /// <param name="configuration"></param>
    public static void ConfigureScalar(this WebApplication application, IConfiguration configuration)
    {
        application.MapOpenApi().AllowAnonymous();
        application.MapScalarApiReference(opts =>
        {
            opts.AddPreferredSecuritySchemes("oauth2")
                .AddAuthorizationCodeFlow("oauth2", flow =>
                {
                    flow.ClientId = configuration["Auth0:ClientId"];
                    flow.Pkce = Pkce.Sha256;
                    flow.SelectedScopes = ["openid", "profile", "email"];
                    flow.AddQueryParameter("audience", configuration["Auth0:Audience"]!);
                });
        }).AllowAnonymous();
    }
}
