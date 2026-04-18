using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.Data;

public class ScranHubDbContext(DbContextOptions<ScranHubDbContext> options) : DbContext(options)
{
    private static readonly Guid _adminId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid _defaultConfigId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly DateTime _createdDate = new(2026, 4, 18, 0, 0, 0, DateTimeKind.Utc);

    public DbSet<User> Users => Set<User>();
    public DbSet<UserFriend> UserFriends => Set<UserFriend>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<UserGroup> UserGroups => Set<UserGroup>();
    public DbSet<GroupCost> GroupCosts => Set<GroupCost>();
    public DbSet<GroupRating> GroupRatings => Set<GroupRating>();
    public DbSet<GroupCostOverride> GroupCostOverrides => Set<GroupCostOverride>();
    public DbSet<GroupRatingOverride> GroupRatingOverrides => Set<GroupRatingOverride>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserFriend>()
            .HasOne(uf => uf.User)
            .WithMany(uf => uf.InitiatedFriendships)
            .HasForeignKey(uf => uf.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserFriend>()
            .HasOne(uf => uf.Friend)
            .WithMany(uf => uf.ReceivedFriendships)
            .HasForeignKey(uf => uf.FriendId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserGroup>()
            .HasOne(ug => ug.User)
            .WithMany(ug => ug.UserGroups)
            .HasForeignKey(ug => ug.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserGroup>()
            .HasOne(ug => ug.Group)
            .WithMany(ug => ug.UserGroups)
            .HasForeignKey(ug => ug.GroupId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<GroupCostOverride>()
            .HasOne(gc => gc.Group)
            .WithOne(gc => gc.GroupCostOverride)
            .HasForeignKey<GroupCostOverride>(gc => gc.GroupId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<GroupRatingOverride>()
            .HasOne(gr => gr.Group)
            .WithOne(gr => gr.GroupRatingOverride)
            .HasForeignKey<GroupRatingOverride>(gr => gr.GroupId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<User>().HasData(new User
        {
            UserId = _adminId,
            DisplayName = "Admin User",
            Active = true,
            Admin = true,
            CreatedOn = _createdDate,
            CreatedBy = _adminId
        });

        modelBuilder.Entity<GroupCost>().HasData(new GroupCost
        {
            GroupCostId = _defaultConfigId,
            Costs = "Cheap|Reasonable|Pricey",
            CreatedOn = _createdDate,
            CreatedBy = _adminId
        });

        modelBuilder.Entity<GroupRating>().HasData(new GroupRating
        {
            GroupRatingId = _defaultConfigId,
            Ratings = "Great|Good|Average|Poor",
            CreatedOn = _createdDate,
            CreatedBy = _adminId
        });
    }
}