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
            UserId = TestUser1AdminId,
            DisplayName = TestUser1AdminName,
            Email = TestUser1AdminEmail,
            Active = true,
            Admin = true
        },
        new User
        {
            UserId = TestUser2NonAdminId,
            DisplayName = TestUser2NonAdminName,
            AuthId = TestAuthId,
            Email = TestUser2NonAdminEmail,
            Active = true,
            Admin = false
        },
        new User
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
            UserId = TestUser1AdminId,
            FriendId = TestUser2NonAdminId,
            Status = FriendshipStatus.Accepted
        },
        new UserFriend
        {
            UserId = TestUser3AdminId,
            FriendId = TestUser1AdminId,
            Status = FriendshipStatus.Pending
        },
        new UserFriend
        {
            UserId = TestUser1AdminId,
            FriendId = TestUser4NonAdminId,
            Status = FriendshipStatus.Declined
        },
        new UserFriend
        {
            UserId = TestUser1AdminId,
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
            UserId = TestUser1AdminId,
        },
        new UserGroup
        {
            UserGroupId = TestGroup1User2Id,
            GroupId = TestGroup1Id,
            UserId = TestUser2NonAdminId,
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
            UserId = TestUser1AdminId,
        },
        new UserGroup
        {
            UserGroupId = TestGroup2User2Id,
            GroupId = TestGroup2Id,
            UserId = TestUser2NonAdminId,
        },
        new UserGroup
        {
            UserGroupId = TestGroup3User1Id,
            GroupId = TestGroup3Id,
            UserId = TestUser1AdminId,
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
            VenueName = TestGroupVenue5Name,
            FoodTypeOptionId = TestFoodTypeOption7Id,
            VenueTypeOptionId = TestVenueTypeOption4Id
        },
        new GroupVenue
        {
            GroupVenueId = TestGroupVenue6Id,
            Visited = false,
            GroupId = TestGroup3Id,
            VenueName = TestGroupVenue6Name
        },
        new GroupVenue
        {
            GroupVenueId = TestGroupVenue7Id,
            Visited = false,
            GroupId = TestGroup3Id,
            VenueName = TestGroupVenue7Name
        },
        new GroupVenue
        {
            GroupVenueId = TestGroupVenue8Id,
            Visited = false,
            GroupId = TestGroup3Id,
            VenueName = TestGroupVenue8Name
        },
        new GroupVenue
        {
            GroupVenueId = TestGroupVenue9Id,
            Visited = false,
            GroupId = TestGroup3Id,
            VenueName = TestGroupVenue9Name
        },
        new GroupVenue
        {
            GroupVenueId = TestGroupVenue10Id,
            Visited = false,
            GroupId = TestGroup2Id,
            VenueName = TestGroupVenue10Name,
            FoodTypeOptionId = TestFoodTypeOption9Id,
            VenueTypeOptionId = TestVenueTypeOption6Id
        }
        );

        context.FoodTypeOptions.AddRange(new FoodTypeOption
        {
            FoodTypeOptionId = TestFoodTypeOption7Id,
            Label = TestFoodTypeOption7Label,
            GroupId = TestGroup3Id
        },
        new FoodTypeOption
        {
            FoodTypeOptionId = TestFoodTypeOption8Id,
            Label = TestFoodTypeOption8Label,
            GroupId = TestGroup3Id
        },
        new FoodTypeOption
        {
            FoodTypeOptionId = TestFoodTypeOption9Id,
            Label = TestFoodTypeOption9Label,
            GroupId = TestGroup2Id
        }
        );

        context.VenueTypeOptions.AddRange(new VenueTypeOption
        {
            VenueTypeOptionId = TestVenueTypeOption4Id,
            Label = TestVenueTypeOption4Label,
            GroupId = TestGroup3Id
        },
        new VenueTypeOption
        {
            VenueTypeOptionId = TestVenueTypeOption5Id,
            Label = TestVenueTypeOption5Label,
            GroupId = TestGroup3Id
        },
        new VenueTypeOption
        {
            VenueTypeOptionId = TestVenueTypeOption6Id,
            Label = TestVenueTypeOption6Label,
            GroupId = TestGroup2Id
        }
        );

        context.CostOptions.AddRange(new CostOption
        {
            CostOptionId = TestCostOption4Id,
            Label = TestCostOption4Label,
            GroupId = TestGroup3Id,
            DisplayOrder = 1
        },
        new CostOption
        {
            CostOptionId = TestCostOption5Id,
            Label = TestCostOption5Label,
            GroupId = TestGroup3Id,
            DisplayOrder = 2
        },
        new CostOption
        {
            CostOptionId = TestCostOption6Id,
            Label = TestCostOption6Label,
            GroupId = TestGroup2Id,
            DisplayOrder = 1
        }
        );

        context.QualityOptions.AddRange(new QualityOption
        {
            QualityOptionId = TestQualityOption5Id,
            Label = TestQualityOption5Label,
            GroupId = TestGroup3Id,
            DisplayOrder = 1
        },
        new QualityOption
        {
            QualityOptionId = TestQualityOption6Id,
            Label = TestQualityOption6Label,
            GroupId = TestGroup3Id,
            DisplayOrder = 2
        },
        new QualityOption
        {
            QualityOptionId = TestQualityOption7Id,
            Label = TestQualityOption7Label,
            GroupId = TestGroup2Id,
            DisplayOrder = 1
        },
        new QualityOption
        {
            QualityOptionId = TestQualityOption8Id,
            Label = TestQualityOption8Label,
            GroupId = TestGroup3Id,
            DisplayOrder = 3
        },
        new QualityOption
        {
            QualityOptionId = TestQualityOption9Id,
            Label = TestQualityOption9Label,
            GroupId = TestGroup3Id,
            DisplayOrder = 4
        }
        );

        context.CostRatings.AddRange(new CostRating
        {
            CostRatingId = TestCostRating1Id,
            CostOptionId = SeedCostOption1Id,
            UserId = TestUser1AdminId,
            GroupVenueId = TestGroupVenue1Id
        },
        new CostRating
        {
            CostRatingId = TestCostRating2Id,
            CostOptionId = SeedCostOption2Id,
            UserId = TestUser2NonAdminId,
            GroupVenueId = TestGroupVenue1Id
        },
        new CostRating
        {
            CostRatingId = TestCostRating3Id,
            CostOptionId = SeedCostOption1Id,
            UserId = TestUser1AdminId,
            GroupVenueId = TestGroupVenue2Id
        }
        );

        context.QualityRatings.AddRange(new QualityRating
        {
            QualityRatingId = TestQualityRating1Id,
            QualityOptionId = SeedQualityOption1Id,
            UserId = TestUser1AdminId,
            GroupVenueId = TestGroupVenue1Id
        },
        new QualityRating
        {
            QualityRatingId = TestQualityRating2Id,
            QualityOptionId = SeedQualityOption2Id,
            UserId = TestUser2NonAdminId,
            GroupVenueId = TestGroupVenue1Id
        },
        new QualityRating
        {
            QualityRatingId = TestQualityRating3Id,
            QualityOptionId = SeedQualityOption1Id,
            UserId = TestUser1AdminId,
            GroupVenueId = TestGroupVenue2Id
        }
        );

        await context.SaveChangesAsync();
    }
}
