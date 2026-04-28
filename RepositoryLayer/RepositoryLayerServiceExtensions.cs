using DAL.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RepositoryLayer.Abstractions;
using RepositoryLayer.Abstractions.Generic;
using RepositoryLayer.AutoMapper;
using RepositoryLayer.Infrastructure;
using RepositoryLayer.Infrastructure.Generic;

namespace RepositoryLayer;

public static class RepositoryLayerServiceExtensions
{
    public static IServiceCollection AddRepositoryLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ScranHubDbContext>(opts =>
            opts.UseSqlServer(configuration.GetConnectionString("Default"), sql => _ = sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));

        services.AddScoped(typeof(IEFRepository<>), typeof(EFRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddRepositories();

        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<DTOToResponseProfile>();
        });

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IGroupRepository, GroupRepository>();
        services.AddScoped<ICostOptionRepository, CostOptionRepository>();
        services.AddScoped<IFoodTypeOptionRepository, FoodTypeOptionRepository>();
        services.AddScoped<IRatingOptionRepository, RatingOptionRepository>();
        services.AddScoped<IVenueTypeOptionRepository, VenueTypeOptionRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserGroupRepository, UserGroupRepository>();
        services.AddScoped<IGroupVenueRepository, GroupVenueRepository>();

        return services;
    }
}
