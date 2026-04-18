using DAL.Entities;
using DAL.Entities.Base;
using Microsoft.EntityFrameworkCore;
using DAL.Data.Configuration;

namespace DAL.Data;

public class ScranHubDbContext(DbContextOptions<ScranHubDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<UserFriend> UserFriends => Set<UserFriend>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<UserGroup> UserGroups => Set<UserGroup>();
    public DbSet<GroupVenue> GroupVenues => Set<GroupVenue>();
    public DbSet<CostOption> CostOptions => Set<CostOption>();
    public DbSet<RatingOption> RatingOptions => Set<RatingOption>();
    public DbSet<FoodTypeOption> FoodTypeOptions => Set<FoodTypeOption>();
    public DbSet<VenueTypeOption> VenueTypeOptions => Set<VenueTypeOption>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ScranHubDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetAuditFields(SeedConstants.AdminId);

        return await base.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> SaveChangesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        SetAuditFields(userId);

        return await base.SaveChangesAsync(cancellationToken);
    }

    private void SetAuditFields(Guid userId)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>().Where(e => e.State == EntityState.Added).ToList())
        {
            entry.Entity.CreatedOn = now;
            entry.Entity.CreatedBy = userId;
        }

        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>().Where(e => e.State == EntityState.Modified).ToList())
        {
            entry.Entity.UpdatedOn = now;
            entry.Entity.UpdatedBy = userId;
        }
    }
}
