using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Debugging;
using ServiceLayer;
using System.Text;
using Utilities.Models.Options;
using Utilities;
using WebApi.Middleware;
using WebApi.ProgramExtensions;

var builder = WebApplication.CreateBuilder(args);

// ServiceExtensions
builder.Services.ConfigureApiBehavior();
builder.Services.ConfigureApiVersioning();
builder.Services.ConfigureSwagger();
builder.Services.ConfigureScalar();
builder.Services.ConfigureCors();

builder.Host.UseSerilog((hostingContext, configuration) =>
{
    configuration.ReadFrom.Configuration(hostingContext.Configuration);
});

builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("Default")!);

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration.GetValue<string>("Authentication:Issuer"),
            ValidAudience = builder.Configuration.GetValue<string>("Authentication:Audience"),
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(
                builder.Configuration.GetValue<string>("Authentication:SecretKey")!))
        };
    });

builder.Services.Configure<Authentication>(builder.Configuration.GetSection("Authentication"));

//Add layers
builder.Services.AddServiceLayer(builder.Configuration);
builder.Services.AddUtilities();

builder.Services.AddScoped<ExceptionHandlingMiddleware>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    SelfLog.Enable(Console.Error);
    app.ConfigureSwagger();
    app.ConfigureScalar();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();

var corsPolicy = app.Environment.IsDevelopment() ? "DevelopmentCorsPolicy" : "ProductionCorsPolicy";
app.UseCors(corsPolicy);

app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();

app.Run();