using Azure.Identity;
using Serilog;
using Serilog.Debugging;
using ServiceLayer;
using Utilities;
using WebApi.Middleware;
using WebApi.ProgramExtensions;

var builder = WebApplication.CreateBuilder(args);

if (!builder.Environment.IsDevelopment())
{
    var keyVaultUrl = builder.Configuration["AZURE_KEY_VAULT_URL"];
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUrl!),
        new DefaultAzureCredential());
}

// From ServiceExtensions
builder.Services.ConfigureApiBehavior();
builder.Services.ConfigureApiVersioning();
builder.Services.ConfigureMiddleware();
builder.Services.ConfigureSwagger(builder.Configuration);
builder.Services.ConfigureScalar(builder.Configuration);
builder.Services.ConfigureCors();
builder.Services.ConfigureAuthentication(builder.Configuration);
builder.Services.ConfigureAuthorization();
builder.Services.ConfigureHealthChecks(builder.Configuration);

// From other projects
builder.Services.AddServiceLayer(builder.Configuration);
builder.Services.AddUtilities();

builder.Host.UseSerilog((hostingContext, configuration) =>
{
    configuration.ReadFrom.Configuration(hostingContext.Configuration);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    SelfLog.Enable(Console.Error);
    app.ConfigureSwagger(builder.Configuration);
    app.ConfigureScalar(builder.Configuration);
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseCors(app.Environment.IsDevelopment() ? "DevelopmentCorsPolicy" : "ProductionCorsPolicy");
app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<UserResolutionMiddleware>();
app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();

app.Run();