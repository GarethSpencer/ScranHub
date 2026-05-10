using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RepositoryLayer;
using RepositoryLayer.Abstractions;
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
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IGroupVenueService, GroupVenueService>();
        services.AddScoped<IQualityRatingService, QualityRatingService>();
        services.AddScoped<ICostRatingService, CostRatingService>();
        return services;
    }
}
