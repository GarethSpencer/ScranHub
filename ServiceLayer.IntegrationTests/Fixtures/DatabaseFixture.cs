using DAL.Data;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;
using static ServiceLayer.IntegrationTests.Helpers.TestConstants;
using Utilities.Enums;

namespace ServiceLayer.IntegrationTests.Fixtures;

public class DatabaseFixture : IAsyncLifetime
{
    public MsSqlContainer Db { get; } = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04")
        .WithCleanUp(true)
        .Build();

    public string ConnectionString => Db.GetConnectionString();

    public async Task InitializeAsync()
    {
        await Db.StartAsync();

        var options = new DbContextOptionsBuilder<ScranHubDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        await using var context = new ScranHubDbContext(options);
        await context.Database.MigrateAsync();
        await SetupDatabase(context);
    }

    public async Task DisposeAsync() => await Db.DisposeAsync();

    private static async Task SetupDatabase(ScranHubDbContext context)
    {
        context!.Users.AddRange(new User
        {
            UserId = TestUser3AdminId,
            DisplayName = TestUser3AdminName,
            Email = TestUser3AdminEmail,
            Active = true,
            Admin = true
        },
        new User
        {
            UserId = TestUser4NonAdminId,
            DisplayName = TestUser4NonAdminName,
            Email = TestUser4NonAdminEmail,
            Active = true,
            Admin = false
        },
        new User
        {
            UserId = TestUser5NonAdminId,
            DisplayName = TestUser5NonAdminName,
            Email = TestUser5NonAdminEmail,
            Active = false,
            Admin = false
        }
        );

        context.UserFriends.AddRange(new UserFriend
        {
            UserId = SeedUser1AdminId,
            FriendId = SeedUser2NonAdminId,
            Status = FriendshipStatus.Accepted
        },
        new UserFriend
        {
            UserId = TestUser3AdminId,
            FriendId = SeedUser1AdminId,
            Status = FriendshipStatus.Pending
        },
        new UserFriend
        {
            UserId = SeedUser1AdminId,
            FriendId = TestUser4NonAdminId,
            Status = FriendshipStatus.Declined
        },
        new UserFriend
        {
            UserId = SeedUser1AdminId,
            FriendId = TestUser5NonAdminId,
            Status = FriendshipStatus.Accepted
        }
        );
        await context.SaveChangesAsync();
    }
}
