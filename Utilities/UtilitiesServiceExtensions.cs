using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Utilities.Models.Requests.Generic;
using Utilities.Models.Requests.Groups;
using Utilities.Models.Requests.GroupVenues;
using Utilities.Models.Requests.Options;
using Utilities.Models.Requests.Users;
using Utilities.Token;
using Utilities.Validators.Generic;
using Utilities.Validators.Groups;
using Utilities.Validators.GroupVenues;
using Utilities.Validators.Options;
using Utilities.Validators.Users;

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
        services.AddScoped<IValidator<PaginationBaseRequest>, PaginationBaseRequestValidator>();
        services.AddScoped<IValidator<CreateGroupRequest>, CreateGroupRequestValidator>();
        services.AddScoped<IValidator<UpdateGroupRequest>, UpdateGroupRequestValidator>();
        services.AddScoped<IValidator<SearchGroupRequest>, SearchGroupRequestValidator>();
        services.AddScoped<IValidator<CreateUserRequest>, CreateUserRequestValidator>();
        services.AddScoped<IValidator<UpdateUserFriendRequest>, UpdateUserFriendRequestValidator>();
        services.AddScoped<IValidator<UpdateUserRequest>, UpdateUserRequestValidator>();
        services.AddScoped<IValidator<SearchUserRequest>, SearchUserRequestValidator>();
        services.AddScoped<IValidator<AddFriendRequest>, AddFriendRequestValidator>();
        services.AddScoped<IValidator<CreateGroupVenueRequest>, CreateGroupVenueRequestValidator>();
        services.AddScoped<IValidator<UpdateGroupVenueRequest>, UpdateGroupVenueRequestValidator>();
        services.AddScoped<IValidator<SearchGroupVenueRequest>, SearchGroupVenueRequestValidator>();
        services.AddScoped<IValidator<SetOptionsRequest>, SetOptionsRequestValidator>();
        services.AddScoped<IValidator<SetOptionRequest>, SetOptionRequestValidator>();
        services.AddScoped<IValidator<UpdateOptionRequest>, UpdateOptionRequestValidator>();
        services.AddScoped<IValidator<GetUserFriendRequest>, GetUserFriendRequestValidator>();
    }
}
