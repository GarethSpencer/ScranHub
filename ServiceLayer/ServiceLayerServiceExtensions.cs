using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RepositoryLayer;
using ServiceLayer.Abstractions;
using ServiceLayer.AutoMapper;
using ServiceLayer.Infrastructure;
using AutoMapper;

namespace ServiceLayer;

public static class ServiceLayerServiceExtensions
{
    public static IServiceCollection AddServiceLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRepositoryLayer(configuration);
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IGroupService, GroupService>();
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<RequestToDTOProfile>();
            cfg.AddProfile<DTOToResponseProfile>();
        });
        
        return services;
    }
}
