using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ServiceLayer;
using System.Text;
using Utilities.Models.Options;
using Utilities.Token;
using WebApi.ProgramExtensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureApiBehavior();
builder.Services.ConfigureApiVersioning();
builder.Services.ConfigureSwagger();
builder.Services.ConfigureScalar();

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
builder.Services.AddServiceLayer(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITokenData, TokenData>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.ConfigureSwagger();
    app.ConfigureScalar();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();

app.Run();