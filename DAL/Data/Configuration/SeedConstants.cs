namespace DAL.Data.Configuration;

internal static class SeedConstants
{
    internal static readonly Guid AdminId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    internal static readonly Guid NonAdminId = Guid.Parse("00000000-0000-0000-0000-000000000002");
    internal static readonly DateTime CreatedDate = new(2026, 4, 18, 0, 0, 0, DateTimeKind.Utc);
}
