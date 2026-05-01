using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Utilities.Models.Requests.Groups;
using Utilities.Token;
using Utilities.Validators.Groups;

namespace Utilities;

public static class UtilitiesServiceExtensions
{
    public static IServiceCollection AddUtilities(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ITokenData, TokenData>();
        services.RegisterValidators();

        return services;
    }

    private static void RegisterValidators(this IServiceCollection services)
    {
        services.AddScoped<IValidator<CreateGroupRequest>, CreateGroupRequestValidator>();
        services.AddScoped<IValidator<UpdateGroupRequest>, UpdateGroupRequestValidator>();
        services.AddScoped<IValidator<SearchGroupRequest>, SearchGroupRequestValidator>();
    }
}
