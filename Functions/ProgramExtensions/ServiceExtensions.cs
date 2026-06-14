using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Utilities.Auth0;

namespace Functions.ProgramExtensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddAuth0(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<Auth0Options>()
            .Bind(configuration.GetSection("Auth0"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHttpClient<IAuth0UserService, Auth0UserService>((sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<Auth0Options>>().Value;
            client.BaseAddress = new Uri($"https://{opts.Domain}/");
        });

        return services;
    }
}
