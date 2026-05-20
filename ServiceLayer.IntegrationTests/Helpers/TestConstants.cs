namespace ServiceLayer.IntegrationTests.Helpers;

public class TestConstants
{
    public static readonly Guid SeedUser1AdminId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    public static readonly Guid SeedUser2NonAdminId = Guid.Parse("00000000-0000-0000-0000-000000000002");
    public static readonly Guid TestUser3AdminId = Guid.Parse("00000000-0000-0000-0000-000000000003");
    public static readonly Guid TestUser4NonAdminId = Guid.Parse("00000000-0000-0000-0000-000000000004");
    public static readonly Guid TestUser5NonAdminId = Guid.Parse("00000000-0000-0000-0000-000000000005");

    public const string SeedUser1AdminName = "Admin User";
    public const string SeedUser2NonAdminName = "Non-Admin User";
    public const string TestUser3AdminName = "Charles";
    public const string TestUser4NonAdminName = "David";
    public const string TestUser5NonAdminName = "Ellen";

    public const string SeedUser1AdminEmail = "admin@example.com";
    public const string SeedUser2NonAdminEmail = "nonadmin@example.com";
    public const string TestUser3AdminEmail = "user3@example.com";
    public const string TestUser4NonAdminEmail = "user4@example.com";
    public const string TestUser5NonAdminEmail = "user5@example.com";

    public static readonly Guid TestGroup1Id = Guid.Parse("10000000-0000-0000-0000-000000000000");
    public static readonly Guid TestGroup2Id = Guid.Parse("20000000-0000-0000-0000-000000000000");
    public static readonly Guid TestGroup3Id = Guid.Parse("30000000-0000-0000-0000-000000000000");

    public const string TestGroup1Name = "Test Group 1";
    public const string TestGroup2Name = "Test Group 2";
    public const string TestGroup3Name = "Test Group 3";

    public static readonly Guid TestGroup1User1Id = Guid.Parse("10000001-0000-0000-0000-000000000000");
    public static readonly Guid TestGroup1User2Id = Guid.Parse("10000002-0000-0000-0000-000000000000");
    public static readonly Guid TestGroup1User3Id = Guid.Parse("10000003-0000-0000-0000-000000000000");
    public static readonly Guid TestGroup1User4Id = Guid.Parse("10000004-0000-0000-0000-000000000000");
    public static readonly Guid TestGroup1User5Id = Guid.Parse("10000005-0000-0000-0000-000000000000");
    public static readonly Guid TestGroup2User1Id = Guid.Parse("20000001-0000-0000-0000-000000000000");
    public static readonly Guid TestGroup2User2Id = Guid.Parse("20000002-0000-0000-0000-000000000000");
    public static readonly Guid TestGroup3User1Id = Guid.Parse("30000001-0000-0000-0000-000000000000");
}
