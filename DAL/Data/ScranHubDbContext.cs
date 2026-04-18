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
    public DbSet<GroupVenue> GroupVenues => Set<GroupVenue>();
    public DbSet<CostOption> CostOptions => Set<CostOption>();
    public DbSet<RatingOption> RatingOptions => Set<RatingOption>();
    public DbSet<FoodTypeOption> FoodTypeOptions => Set<FoodTypeOption>();
    public DbSet<VenueTypeOption> VenueTypeOptions => Set<VenueTypeOption>();

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

        modelBuilder.Entity<CostOption>()
            .HasOne(gc => gc.Group)
            .WithMany(g => g.CostOptions)
            .HasForeignKey(gc => gc.GroupId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RatingOption>()
            .HasOne(gr => gr.Group)
            .WithMany(g => g.RatingOptions)
            .HasForeignKey(gr => gr.GroupId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FoodTypeOption>()
            .HasOne(gr => gr.Group)
            .WithMany(g => g.FoodTypeOptions)
            .HasForeignKey(gr => gr.GroupId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<VenueTypeOption>()
            .HasOne(gr => gr.Group)
            .WithMany(g => g.VenueTypeOptions)
            .HasForeignKey(gr => gr.GroupId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<GroupVenue>()
            .HasOne(gv => gv.CostOption)
            .WithMany(co => co.GroupVenues)
            .HasForeignKey(gv => gv.CostOptionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<GroupVenue>()
            .HasOne(gv => gv.RatingOption)
            .WithMany(ro => ro.GroupVenues)
            .HasForeignKey(gv => gv.RatingOptionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<GroupVenue>()
            .HasOne(gv => gv.FoodTypeOption)
            .WithMany(ft => ft.GroupVenues)
            .HasForeignKey(gv => gv.FoodTypeOptionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<GroupVenue>()
            .HasOne(gv => gv.VenueTypeOption)
            .WithMany(vt => vt.GroupVenues)
            .HasForeignKey(gv => gv.VenueTypeOptionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<GroupVenue>()
            .HasOne(gv => gv.Group)
            .WithMany(g => g.GroupVenues)
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

        modelBuilder.Entity<CostOption>().HasData(
            new CostOption { CostOptionId = Guid.Parse("00000000-0000-0000-0001-000000000001"), GroupId = null, Label = "Cheap",      DisplayOrder = 1, CreatedOn = _createdDate, CreatedBy = _adminId },
            new CostOption { CostOptionId = Guid.Parse("00000000-0000-0000-0001-000000000002"), GroupId = null, Label = "Reasonable", DisplayOrder = 2, CreatedOn = _createdDate, CreatedBy = _adminId },
            new CostOption { CostOptionId = Guid.Parse("00000000-0000-0000-0001-000000000003"), GroupId = null, Label = "Pricey",     DisplayOrder = 3, CreatedOn = _createdDate, CreatedBy = _adminId }
        );

        modelBuilder.Entity<RatingOption>().HasData(
            new RatingOption { RatingOptionId = Guid.Parse("00000000-0000-0000-0002-000000000001"), GroupId = null, Label = "Great",   DisplayOrder = 1, CreatedOn = _createdDate, CreatedBy = _adminId },
            new RatingOption { RatingOptionId = Guid.Parse("00000000-0000-0000-0002-000000000002"), GroupId = null, Label = "Good",    DisplayOrder = 2, CreatedOn = _createdDate, CreatedBy = _adminId },
            new RatingOption { RatingOptionId = Guid.Parse("00000000-0000-0000-0002-000000000003"), GroupId = null, Label = "Average", DisplayOrder = 3, CreatedOn = _createdDate, CreatedBy = _adminId },
            new RatingOption { RatingOptionId = Guid.Parse("00000000-0000-0000-0002-000000000004"), GroupId = null, Label = "Poor",    DisplayOrder = 4, CreatedOn = _createdDate, CreatedBy = _adminId }
        );

        modelBuilder.Entity<FoodTypeOption>().HasData(
            new FoodTypeOption { FoodTypeOptionId = Guid.Parse("00000000-0000-0000-0003-000000000001"), GroupId = null, Label = "Café",     DisplayOrder = 1, CreatedOn = _createdDate, CreatedBy = _adminId },
            new FoodTypeOption { FoodTypeOptionId = Guid.Parse("00000000-0000-0000-0003-000000000002"), GroupId = null, Label = "Burgers",  DisplayOrder = 2, CreatedOn = _createdDate, CreatedBy = _adminId },
            new FoodTypeOption { FoodTypeOptionId = Guid.Parse("00000000-0000-0000-0003-000000000003"), GroupId = null, Label = "Chicken",  DisplayOrder = 3, CreatedOn = _createdDate, CreatedBy = _adminId },
            new FoodTypeOption { FoodTypeOptionId = Guid.Parse("00000000-0000-0000-0003-000000000004"), GroupId = null, Label = "Pizza",    DisplayOrder = 4, CreatedOn = _createdDate, CreatedBy = _adminId },
            new FoodTypeOption { FoodTypeOptionId = Guid.Parse("00000000-0000-0000-0003-000000000005"), GroupId = null, Label = "Oriental", DisplayOrder = 5, CreatedOn = _createdDate, CreatedBy = _adminId },
            new FoodTypeOption { FoodTypeOptionId = Guid.Parse("00000000-0000-0000-0003-000000000006"), GroupId = null, Label = "Other",    DisplayOrder = 6, CreatedOn = _createdDate, CreatedBy = _adminId }
        );

        modelBuilder.Entity<VenueTypeOption>().HasData(
            new VenueTypeOption { VenueTypeOptionId = Guid.Parse("00000000-0000-0000-0004-000000000001"), GroupId = null, Label = "Eat-In",     DisplayOrder = 1, CreatedOn = _createdDate, CreatedBy = _adminId },
            new VenueTypeOption { VenueTypeOptionId = Guid.Parse("00000000-0000-0000-0004-000000000002"), GroupId = null, Label = "Takeaway",   DisplayOrder = 2, CreatedOn = _createdDate, CreatedBy = _adminId },
            new VenueTypeOption { VenueTypeOptionId = Guid.Parse("00000000-0000-0000-0004-000000000003"), GroupId = null, Label = "Both",       DisplayOrder = 3, CreatedOn = _createdDate, CreatedBy = _adminId }
        );
    }
}