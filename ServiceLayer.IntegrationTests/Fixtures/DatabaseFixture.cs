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
            Active = true
        },
        new Group
        {
            GroupId = TestGroup2Id,
            GroupName = TestGroup2Name,
            Active = false
        },
        new Group
        {
            GroupId = TestGroup3Id,
            GroupName = TestGroup3Name,
            Active = true
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
            UserGroupId = TestGroup2User1Id,
            GroupId = TestGroup2Id,
            UserId = SeedUser1AdminId,
        },
        new UserGroup
        {
            UserGroupId = TestGroup2User2Id,
            GroupId = TestGroup2Id,
            UserId = SeedUser2NonAdminId,
        },
        new UserGroup
        {
            UserGroupId = TestGroup3User1Id,
            GroupId = TestGroup3Id,
            UserId = SeedUser1AdminId,
        }
        );

        context.GroupVenues.AddRange(new GroupVenue
        {
            GroupVenueId = TestGroupVenue1Id,
            Visited = true,
            GroupId = TestGroup1Id,
            VenueName = TestGroupVenue1Name,
            FoodTypeOptionId = SeedFoodTypeOption1Id,
            VenueTypeOptionId = SeedVenueTypeOption1Id
        },
        new GroupVenue
        {
            GroupVenueId = TestGroupVenue2Id,
            Visited = false,
            GroupId = TestGroup1Id,
            VenueName = TestGroupVenue2Name,
            FoodTypeOptionId = SeedFoodTypeOption2Id,
            VenueTypeOptionId = SeedVenueTypeOption2Id
        },
        new GroupVenue
        {
            GroupVenueId = TestGroupVenue3Id,
            Visited = false,
            GroupId = TestGroup1Id,
            VenueName = TestGroupVenue3Name,
            FoodTypeOptionId = SeedFoodTypeOption1Id,
            VenueTypeOptionId = SeedVenueTypeOption1Id
        },
        new GroupVenue
        {
            GroupVenueId = TestGroupVenue4Id,
            Visited = false,
            GroupId = TestGroup1Id,
            VenueName = TestGroupVenue4Name
        },
        new GroupVenue
        {
            GroupVenueId = TestGroupVenue5Id,
            Visited = false,
            GroupId = TestGroup3Id,
            VenueName = TestGroupVenue5Name
        }
        );

        context.FoodTypeOptions.AddRange(new FoodTypeOption
        {
            FoodTypeOptionId = TestFoodTypeOption7Id,
            Label = TestFoodTypeOption7Label,
            GroupId = TestGroup3Id,
        },
        new FoodTypeOption
        {
            FoodTypeOptionId = TestFoodTypeOption8Id,
            Label = TestFoodTypeOption8Label,
            GroupId = TestGroup3Id,
        }
        );

        context.VenueTypeOptions.AddRange(new VenueTypeOption
        {
            VenueTypeOptionId = TestVenueTypeOption4Id,
            Label = TestVenueTypeOption4Label,
            GroupId = TestGroup3Id,
        },
        new VenueTypeOption
        {
            VenueTypeOptionId = TestVenueTypeOption5Id,
            Label = TestVenueTypeOption5Label,
            GroupId = TestGroup3Id,
        }
        );

        context.CostOptions.AddRange(new CostOption
        {
            CostOptionId = TestCostOption4Id,
            Label = TestCostOption4Label,
            GroupId = TestGroup3Id,
        },
        new CostOption
        {
            CostOptionId = TestCostOption5Id,
            Label = TestCostOption5Label,
            GroupId = TestGroup3Id,
        }
        );

        context.QualityOptions.AddRange(new QualityOption
        {
            QualityOptionId = TestQualityOption5Id,
            Label = TestQualityOption5Label,
            GroupId = TestGroup3Id,
        },
        new QualityOption
        {
            QualityOptionId = TestQualityOption6Id,
            Label = TestQualityOption6Label,
            GroupId = TestGroup3Id,
        }
        );

        context.CostRatings.AddRange(new CostRating
        {
            CostRatingId = TestCostRating1Id,
            CostOptionId = SeedCostOption1Id,
            UserId = SeedUser1AdminId,
            GroupVenueId = TestGroupVenue1Id
        },
        new CostRating
        {
            CostRatingId = TestCostRating2Id,
            CostOptionId = SeedCostOption1Id,
            UserId = SeedUser2NonAdminId,
            GroupVenueId = TestGroupVenue1Id
        },
        new CostRating
        {
            CostRatingId = TestCostRating3Id,
            CostOptionId = SeedCostOption1Id,
            UserId = SeedUser1AdminId,
            GroupVenueId = TestGroupVenue2Id
        }
        );

        context.QualityRatings.AddRange(new QualityRating
        {
            QualityRatingId = TestQualityRating1Id,
            QualityOptionId = SeedQualityOption1Id,
            UserId = SeedUser1AdminId,
            GroupVenueId = TestGroupVenue1Id
        },
        new QualityRating
        {
            QualityRatingId = TestQualityRating2Id,
            QualityOptionId = SeedQualityOption1Id,
            UserId = SeedUser2NonAdminId,
            GroupVenueId = TestGroupVenue1Id
        },
        new QualityRating
        {
            QualityRatingId = TestQualityRating3Id,
            QualityOptionId = SeedQualityOption1Id,
            UserId = SeedUser1AdminId,
            GroupVenueId = TestGroupVenue2Id
        }
        );

        await context.SaveChangesAsync();
    }
}
