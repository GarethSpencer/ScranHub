using Asp.Versioning;
using Microsoft.OpenApi;
using System.Text.Json.Serialization;
using Utilities.Models.Responses.GenericResponses;

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
}
