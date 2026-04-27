using Microsoft.Extensions.DependencyInjection;
using Utilities.Token;


namespace Utilities;

public static class UtilitiesServiceExtensions
{
    public static IServiceCollection AddUtilities(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ITokenData, TokenData>();

        return services;
    }
}
