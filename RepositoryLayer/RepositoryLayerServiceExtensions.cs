using DAL.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RepositoryLayer.Abstractions.Generic;
using RepositoryLayer.Infrastructure.Generic;

namespace RepositoryLayer;

public static class RepositoryLayerServiceExtensions
{
    public static IServiceCollection AddRepositoryLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ScranHubDbContext>(opts =>
            opts.UseSqlServer(configuration.GetConnectionString("Default")));

        services.AddScoped(typeof(IEFRepository<>), typeof(EFRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
