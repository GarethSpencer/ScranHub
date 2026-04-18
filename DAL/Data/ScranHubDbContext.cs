using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.Data;

public class ScranHubDbContext(DbContextOptions<ScranHubDbContext> options) : DbContext(options)
{
    private static readonly Guid _adminId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly DateTime _createdDate = new(2026, 4, 18, 0, 0, 0, DateTimeKind.Utc);

    public DbSet<User> Users => Set<User>();
    public DbSet<UserFriend> UserFriends => Set<UserFriend>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<UserGroup> UserGroups => Set<UserGroup>();
    public DbSet<GroupCostOption> GroupCostOptions => Set<GroupCostOption>();
    public DbSet<GroupRatingOption> GroupRatingOptions => Set<GroupRatingOption>();
    public DbSet<GroupVenue> GroupVenues => Set<GroupVenue>();

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

        modelBuilder.Entity<GroupCostOption>()
            .HasOne(gc => gc.Group)
            .WithMany(g => g.GroupCostOptions)
            .HasForeignKey(gc => gc.GroupId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<GroupRatingOption>()
            .HasOne(gr => gr.Group)
            .WithMany(g => g.GroupRatingOptions)
            .HasForeignKey(gr => gr.GroupId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<GroupVenue>()
            .HasOne(gv => gv.GroupCostOption)
            .WithMany()
            .HasForeignKey(gv => gv.GroupCostOptionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<GroupVenue>()
            .HasOne(gv => gv.GroupRatingOption)
            .WithMany()
            .HasForeignKey(gv => gv.GroupRatingOptionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<GroupVenue>()
            .HasOne(gv => gv.Group)
            .WithMany(gv => gv.GroupVenues)
            .HasForeignKey(gv => gv.GroupId)
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

        modelBuilder.Entity<GroupCostOption>().HasData(
            new GroupCostOption { GroupCostOptionId = Guid.Parse("00000000-0000-0000-0001-000000000001"), GroupId = null, Label = "Cheap",      DisplayOrder = 0, CreatedOn = _createdDate, CreatedBy = _adminId },
            new GroupCostOption { GroupCostOptionId = Guid.Parse("00000000-0000-0000-0001-000000000002"), GroupId = null, Label = "Reasonable", DisplayOrder = 1, CreatedOn = _createdDate, CreatedBy = _adminId },
            new GroupCostOption { GroupCostOptionId = Guid.Parse("00000000-0000-0000-0001-000000000003"), GroupId = null, Label = "Pricey",     DisplayOrder = 2, CreatedOn = _createdDate, CreatedBy = _adminId }
        );

        modelBuilder.Entity<GroupRatingOption>().HasData(
            new GroupRatingOption { GroupRatingOptionId = Guid.Parse("00000000-0000-0000-0002-000000000001"), GroupId = null, Label = "Great",   DisplayOrder = 0, CreatedOn = _createdDate, CreatedBy = _adminId },
            new GroupRatingOption { GroupRatingOptionId = Guid.Parse("00000000-0000-0000-0002-000000000002"), GroupId = null, Label = "Good",    DisplayOrder = 1, CreatedOn = _createdDate, CreatedBy = _adminId },
            new GroupRatingOption { GroupRatingOptionId = Guid.Parse("00000000-0000-0000-0002-000000000003"), GroupId = null, Label = "Average", DisplayOrder = 2, CreatedOn = _createdDate, CreatedBy = _adminId },
            new GroupRatingOption { GroupRatingOptionId = Guid.Parse("00000000-0000-0000-0002-000000000004"), GroupId = null, Label = "Poor",    DisplayOrder = 3, CreatedOn = _createdDate, CreatedBy = _adminId }
        );
    }
}