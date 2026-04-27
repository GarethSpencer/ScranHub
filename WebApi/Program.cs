using Serilog;
using Serilog.Debugging;
using ServiceLayer;
using Utilities;
using WebApi.Middleware;
using WebApi.ProgramExtensions;

var builder = WebApplication.CreateBuilder(args);

// From ServiceExtensions
builder.Services.ConfigureApiBehavior();
builder.Services.ConfigureApiVersioning();
builder.Services.ConfigureSwagger();
builder.Services.ConfigureScalar();
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
    app.ConfigureSwagger();
    app.ConfigureScalar();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseCors(app.Environment.IsDevelopment() ? "DevelopmentCorsPolicy" : "ProductionCorsPolicy");
app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();

app.Run();