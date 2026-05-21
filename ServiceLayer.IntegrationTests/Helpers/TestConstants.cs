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

    public static readonly Guid TestGroupVenue1Id = Guid.Parse("00000000-0001-0000-0000-000000000000");
    public static readonly Guid TestGroupVenue2Id = Guid.Parse("00000000-0002-0000-0000-000000000000");
    public static readonly Guid TestGroupVenue3Id = Guid.Parse("00000000-0003-0000-0000-000000000000");
    public static readonly Guid TestGroupVenue4Id = Guid.Parse("00000000-0004-0000-0000-000000000000");
    public static readonly Guid TestGroupVenue5Id = Guid.Parse("00000000-0005-0000-0000-000000000000");

    public const string TestGroupVenue1Name = "Test Venue 1";
    public const string TestGroupVenue2Name = "Test Venue 2";
    public const string TestGroupVenue3Name = "Test Venue 3";
    public const string TestGroupVenue4Name = "Test Venue 4";
    public const string TestGroupVenue5Name = "Test Venue 5";

    public static readonly Guid SeedCostOption1Id = Guid.Parse("00000000-0000-0000-0001-000000000001");
    public static readonly Guid SeedCostOption2Id = Guid.Parse("00000000-0000-0000-0001-000000000002");
    public static readonly Guid SeedCostOption3Id = Guid.Parse("00000000-0000-0000-0001-000000000003");
    public static readonly Guid TestCostOption4Id = Guid.Parse("00000000-0000-0000-0001-000000000004");
    public static readonly Guid TestCostOption5Id = Guid.Parse("00000000-0000-0000-0001-000000000005");

    public const string SeedCostOption1Label = "Cheap";
    public const string SeedCostOption2Label = "Reasonable";
    public const string SeedCostOption3Label = "Pricey";
    public const string TestCostOption4Label = "Cost Override 1";
    public const string TestCostOption5Label = "Cost Override 2";

    public static readonly Guid SeedQualityOption1Id = Guid.Parse("00000000-0000-0000-0002-000000000001");
    public static readonly Guid SeedQualityOption2Id = Guid.Parse("00000000-0000-0000-0002-000000000002");
    public static readonly Guid SeedQualityOption3Id = Guid.Parse("00000000-0000-0000-0002-000000000003");
    public static readonly Guid SeedQualityOption4Id = Guid.Parse("00000000-0000-0000-0002-000000000004");
    public static readonly Guid TestQualityOption5Id = Guid.Parse("00000000-0000-0000-0002-000000000005");
    public static readonly Guid TestQualityOption6Id = Guid.Parse("00000000-0000-0000-0002-000000000006");

    public const string SeedQualityOption1Label = "Great";
    public const string SeedQualityOption2Label = "Good";
    public const string SeedQualityOption3Label = "Average";
    public const string SeedQualityOption4Label = "Poor";
    public const string TestQualityOption5Label = "Quality Override 1";
    public const string TestQualityOption6Label = "Quality Override 2";

    public static readonly Guid SeedFoodTypeOption1Id = Guid.Parse("00000000-0000-0000-0003-000000000001");
    public static readonly Guid SeedFoodTypeOption2Id = Guid.Parse("00000000-0000-0000-0003-000000000002");
    public static readonly Guid SeedFoodTypeOption3Id = Guid.Parse("00000000-0000-0000-0003-000000000003");
    public static readonly Guid SeedFoodTypeOption4Id = Guid.Parse("00000000-0000-0000-0003-000000000004");
    public static readonly Guid SeedFoodTypeOption5Id = Guid.Parse("00000000-0000-0000-0003-000000000005");
    public static readonly Guid SeedFoodTypeOption6Id = Guid.Parse("00000000-0000-0000-0003-000000000006");
    public static readonly Guid TestFoodTypeOption7Id = Guid.Parse("00000000-0000-0000-0003-000000000007");
    public static readonly Guid TestFoodTypeOption8Id = Guid.Parse("00000000-0000-0000-0003-000000000008");

    public const string SeedFoodTypeOption1Label = "Café";
    public const string SeedFoodTypeOption2Label = "Burgers";
    public const string SeedFoodTypeOption3Label = "Chicken";
    public const string SeedFoodTypeOption4Label = "Pizza";
    public const string SeedFoodTypeOption5Label = "Oriental";
    public const string SeedFoodTypeOption6Label = "Other";
    public const string TestFoodTypeOption7Label = "Food Override 1";
    public const string TestFoodTypeOption8Label = "Food Override 2";

    public static readonly Guid SeedVenueTypeOption1Id = Guid.Parse("00000000-0000-0000-0004-000000000001");
    public static readonly Guid SeedVenueTypeOption2Id = Guid.Parse("00000000-0000-0000-0004-000000000002");
    public static readonly Guid SeedVenueTypeOption3Id = Guid.Parse("00000000-0000-0000-0004-000000000003");
    public static readonly Guid TestVenueTypeOption4Id = Guid.Parse("00000000-0000-0000-0004-000000000004");
    public static readonly Guid TestVenueTypeOption5Id = Guid.Parse("00000000-0000-0000-0004-000000000005");

    public const string SeedVenueTypeOption1Label = "Eat-In";
    public const string SeedVenueTypeOption2Label = "Takeaway";
    public const string SeedVenueTypeOption3Label = "Both";
    public const string TestVenueTypeOption4Label = "Venue Override 1";
    public const string TestVenueTypeOption5Label = "Venue Override 2";

    public static readonly Guid TestCostRating1Id = Guid.Parse("00000000-0000-0001-0000-000000000001");
    public static readonly Guid TestCostRating2Id = Guid.Parse("00000000-0000-0001-0000-000000000002");
    public static readonly Guid TestCostRating3Id = Guid.Parse("00000000-0000-0001-0000-000000000003");

    public static readonly Guid TestQualityRating1Id = Guid.Parse("00000000-0000-0002-0000-000000000001");
    public static readonly Guid TestQualityRating2Id = Guid.Parse("00000000-0000-0002-0000-000000000002");
    public static readonly Guid TestQualityRating3Id = Guid.Parse("00000000-0000-0002-0000-000000000003");
}
