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
}
