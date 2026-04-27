using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using System.Text.Json.Serialization;
using Utilities.Models.Options;
using Utilities.Models.Responses.GenericResponses;
using WebApi.Middleware;

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

        services.AddScoped<ExceptionHandlingMiddleware>();
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

    public static void ConfigureAuthorization(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build());
    }

    public static void ConfigureHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddSqlServer(configuration.GetConnectionString("Default")!);
    }
}
