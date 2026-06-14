using Azure.Identity;
using Functions.ProgramExtensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RepositoryLayer;
using Utilities;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

if (!builder.Environment.IsDevelopment())
{
    var keyVaultUrl = builder.Configuration["AZURE_KEY_VAULT_URL"];
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUrl!),
        new DefaultAzureCredential());
}

builder.Services.AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services.AddAuth0(builder.Configuration);
builder.Services.AddUtilities();
builder.Services.AddRepositoryLayer(builder.Configuration);

builder.Build().Run();
