using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using System.Text.Json.Serialization;
using Utilities.Models.Options;
using Utilities.Models.Responses.Generic;
using WebApi.Middleware;

namespace WebApi.ProgramExtensions;

/// <summary>
/// Extension methods for the WebApi project to configure services such as API behavior, versioning, Swagger, Scalar, CORS, authentication, authorization and health checks.
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// Configure API behavior, such as model validation and exception handling.
    /// This method adds controllers with custom API behavior options, including a custom response for invalid model states.
    /// It also adds a scoped middleware for exception handling.
    /// </summary>
    /// <param name="services"></param>
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

        services.AddScoped<ExceptionHandlingMiddleware>();
    }

    /// <summary>
    /// Configure API versioning, including default API version, versioning scheme and API explorer options for Swagger integration.
    /// </summary>
    /// <param name="services"></param>
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

    /// <summary>
    /// Configure Swagger for API documentation and UI, including support for API versioning and authorization persistence in the UI.
    /// </summary>
    /// <param name="services"></param>
    public static void ConfigureSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(opts =>
        {
            opts.SwaggerDoc("v1", new OpenApiInfo { Title = "ScranHub API", Version = "v1" });

            opts.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Name = "Authorization",
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Paste JWT Token Here"
            });

            opts.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("bearer", document)] = []
            });

            opts.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml"));
        });
    }

    /// <summary>
    /// Configure Scalar for API reference documentation and UI, including support for authentication with Bearer tokens in the UI.
    /// </summary>
    /// <param name="services"></param>
    public static void ConfigureScalar(this IServiceCollection services)
    {
        services.AddOpenApi(opts =>
        {
            opts.AddDocumentTransformer((document, context, ct) =>
            {
                document.Components ??= new();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Paste JWT Token Here"
                };

                return Task.CompletedTask;
            });
        });
    }

    /// <summary>
    /// Configure Cors policies for development and production environments.
    /// Allowing any origin, method and header in development, and restricting origins and headers in production.
    /// </summary>
    /// <param name="services"></param>
    public static void ConfigureCors(this IServiceCollection services)
    {
        services.AddCors(opts =>
        {
            opts.AddPolicy("DevelopmentCorsPolicy",
                policy => policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());

            opts.AddPolicy("ProductionCorsPolicy",
                policy => policy.WithOrigins("https://www.scranhub.com")
                    .AllowAnyMethod()
                    .WithHeaders("Authorization", "Content-Type"));
        });
    }

    /// <summary>
    /// Configure authentication using JWT Bearer tokens, including token validation parameters such as issuer, audience and signing key.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    public static void ConfigureAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication("Bearer")
            .AddJwtBearer(opts =>
            {
                opts.TokenValidationParameters = new()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration.GetValue<string>("Authentication:Issuer"),
                    ValidAudience = configuration.GetValue<string>("Authentication:Audience"),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(
                        configuration.GetValue<string>("Authentication:SecretKey")!))
                };
            });

        services.Configure<Authentication>(configuration.GetSection("Authentication"));
    }

    /// <summary>
    /// Configure authorization with a fallback policy that requires authenticated users by default for all endpoints,
    /// unless overridden with [AllowAnonymous] or specific policies on controllers or actions.
    /// </summary>
    /// <param name="services"></param>
    public static void ConfigureAuthorization(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build());
    }

    /// <summary>
    /// Configure basic health checks for the application, including a check for SQL Server connectivity.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    public static void ConfigureHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddSqlServer(configuration.GetConnectionString("Default")!);
    }
}
