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

        context.Groups.AddRange(new Group
        {
            GroupId = TestGroup1Id,
            GroupName = TestGroup1Name,
            Active = true,
            CreatedBy = SeedUser1AdminId
        },
        new Group
        {
            GroupId = TestGroup2Id,
            GroupName = TestGroup2Name,
            Active = false,
            CreatedBy = SeedUser1AdminId
        }
        );

        context.UserGroups.AddRange(new UserGroup
        {
            UserGroupId = TestGroup1User1Id,
            GroupId = TestGroup1Id,
            UserId = SeedUser1AdminId,
        },
        new UserGroup
        {
            UserGroupId = TestGroup1User2Id,
            GroupId = TestGroup1Id,
            UserId = SeedUser2NonAdminId,
        },
        new UserGroup
        {
            UserGroupId = TestGroup1User3Id,
            GroupId = TestGroup1Id,
            UserId = TestUser3AdminId,
        },
        new UserGroup
        {
            UserGroupId = TestGroup1User4Id,
            GroupId = TestGroup1Id,
            UserId = TestUser4NonAdminId,
        },
        new UserGroup
        {
            UserGroupId = TestGroup1User5Id,
            GroupId = TestGroup1Id,
            UserId = TestUser5NonAdminId,
        },
        new UserGroup
        {
            UserGroupId = TestGroup2User2Id,
            GroupId = TestGroup2Id,
            UserId = SeedUser2NonAdminId,
        }
        );

        await context.SaveChangesAsync();
    }
}
