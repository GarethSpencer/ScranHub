using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RepositoryLayer;
using ServiceLayer.Abstractions;
using ServiceLayer.Infrastructure;

namespace ServiceLayer;

public static class ServiceLayerServiceExtensions
{
    public static IServiceCollection AddServiceLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRepositoryLayer(configuration);
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IGroupService, GroupService>();
        return services;
    }
}
